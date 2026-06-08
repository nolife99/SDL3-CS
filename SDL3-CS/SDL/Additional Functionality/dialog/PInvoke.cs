#region License

/* Copyright (c) 2024-2025 Eduard Gushchin.
 *
 * This software is provided 'as-is', without any express or implied warranty.
 * In no event will the authors be held liable for any damages arising from
 * the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 * claim that you wrote the original software. If you use this software in a
 * product, an acknowledgment in the product documentation would be
 * appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not be
 * misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source distribution.
 */

// This file is an altered version (storybrew fork). The dialog thunking was rewritten:
//   * The native callback is a single [UnmanagedCallersOnly] static thunk passed as a C function
//     pointer — no delegate marshaling, no runtime-generated thunks, AOT-friendly.
//   * Per-request state is one managed object behind one normal GCHandle (previously: a pinned pooled
//     array containing two more GCHandles).
//   * Filters and the default location are copied into ONE native block that lives until the dialog's
//     callback runs. The previous implementation unpinned the filter memory as soon as the native call
//     returned, while the asynchronous dialog could still read it (use-after-free).
//   * Results are delivered as managed string[] — safe to store or move across threads (previously:
//     pool-rented memory that was returned to the pool the moment the callback returned).
//   * A managed exception thrown by the callback never unwinds into native SDL; it is routed through
//     SDL.ReportCallbackException.

#endregion

namespace SDL3;

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

public static unsafe partial class SDL
{
    /// <summary>Mirrors the native SDL_DialogFileFilter: two UTF-8 string pointers.</summary>
    struct NativeDialogFilter
    {
        public byte* Name;
        public byte* Pattern;
    }

    /// <summary>
    ///     Per-request state for one dialog invocation, referenced from native code through a single
    ///     normal <see cref="GCHandle"/> passed as SDL's userdata. The native block holds the filter
    ///     array and strings (and the default location) and is freed by the thunk, because SDL is
    ///     allowed to read it until the callback has run.
    /// </summary>
    sealed class DialogRequest(DialogFileCallback callback)
    {
        public readonly DialogFileCallback Callback = callback;
        public nint NativeBlock;

        public void FreeNativeBlock()
        {
            var block = NativeBlock;
            NativeBlock = 0;
            if (block != 0) NativeMemory.Free((void*)block);
        }
    }

    /// <summary>
    ///     The one native-callable entry point for every dialog function. Exactly-once by SDL contract
    ///     (cancel and error paths also invoke it), so it owns the cleanup of the request state.
    /// </summary>
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static void DialogFileThunk(nint userdata, byte** filelist, int filter)
    {
        var handle = GCHandle.FromIntPtr(userdata);
        DialogRequest? request = null;
        try
        {
            request = (DialogRequest)handle.Target!;

            // Nothing is decoded or copied here: the callback receives a ref-struct view straight over
            // SDL's buffers, and materializes only what must outlive it.
            var count = 0;
            if (filelist != null)
                while (filelist[count] != null)
                    count++;

            request.Callback(new(filelist, count), filter);
        }
        catch (Exception exception)
        {
            // Never let a managed exception unwind into native SDL (often on a foreign dialog thread).
            ReportCallbackException(exception);
        }
        finally
        {
            request?.FreeNativeBlock();
            handle.Free();
        }
    }

    /// <summary>
    ///     Copies the filters and default location into one native allocation owned by
    ///     <paramref name="request"/>. SDL requires both to stay valid until the callback is invoked,
    ///     so the thunk frees the block, not the caller.
    /// </summary>
    static void MarshalDialogRequest(DialogRequest request,
        scoped ReadOnlySpan<DialogFileFilter> filters,
        scoped ReadOnlySpan<char> defaultLocation,
        out NativeDialogFilter* filtersNative,
        out byte* locationNative)
    {
        filtersNative = null;
        locationNative = null;

        var headerBytes = filters.Length * sizeof(NativeDialogFilter);
        var total = headerBytes;
        foreach (var filter in filters)
            total += Encoding.UTF8.GetByteCount(filter.Name) + 1 + Encoding.UTF8.GetByteCount(filter.Pattern) + 1;

        var hasLocation = !defaultLocation.IsWhiteSpace();
        if (hasLocation) total += Encoding.UTF8.GetByteCount(defaultLocation) + 1;

        if (total == 0) return;

        var block = (byte*)NativeMemory.Alloc((nuint)total);
        request.NativeBlock = (nint)block;

        var cursor = block + headerBytes;
        if (filters.Length > 0)
        {
            filtersNative = (NativeDialogFilter*)block;
            for (var i = 0; i < filters.Length; ++i)
            {
                filtersNative[i].Name = WriteUtf8(filters[i].Name, ref cursor);
                filtersNative[i].Pattern = WriteUtf8(filters[i].Pattern, ref cursor);
            }
        }

        if (hasLocation) locationNative = WriteUtf8(defaultLocation, ref cursor);
    }

    static byte* WriteUtf8(scoped ReadOnlySpan<char> text, ref byte* cursor)
    {
        var count = Encoding.UTF8.GetBytes(text, new Span<byte>(cursor, Encoding.UTF8.GetByteCount(text)));
        cursor[count] = 0;

        var start = cursor;
        cursor += count + 1;
        return start;
    }

    [LibraryImport(SDLLibrary), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void SDL_ShowOpenFileDialog(delegate* unmanaged[Cdecl]<nint, byte**, int, void> callback,
        nint userdata,
        nint window,
        NativeDialogFilter* filters,
        int nfilters,
        byte* defaultLocation,
        [MarshalAs(UnmanagedType.I1)] bool allowMany);

    /// <code>extern SDL_DECLSPEC void SDLCALL SDL_ShowOpenFileDialog(SDL_DialogFileCallback callback, void *userdata, SDL_Window *window, const SDL_DialogFileFilter *filters, int nfilters, const char *default_location, bool allow_many);</code>
    /// <summary>
    /// <para> Displays a dialog that lets the user select a file on their filesystem. </para>
    /// <para> This function should only be invoked from the main thread. </para>
    /// <para> This is an asynchronous function; it will return immediately, and the result will be passed to the callback. </para>
    /// <para>
    /// The callback will be invoked with the list of files the user chose: empty if the user canceled the dialog,
    /// <c> null </c> if an error occurred.
    /// </para>
    /// <para> Note that the callback may be called from a different thread than the one the function was invoked on. </para>
    /// <para> Depending on the platform, the user may be allowed to input paths that don't yet exist. </para>
    /// <para>
    /// On Linux, dialogs may require XDG Portals, which requires DBus, which requires an event-handling loop. Apps that do
    /// not use SDL to handle events should add a call to <see cref="PumpEvents()"/> in their main loop.
    /// </para>
    /// </summary>
    /// <param name="callback"> invoked when the user accepts or cancels the dialog, or an error occurs. </param>
    /// <param name="window"> the window the dialog should be modal for, may be <c> 0 </c>. Not all platforms support this. </param>
    /// <param name="filters">
    /// a list of filters, may be empty. Copied for the lifetime of the dialog — the caller keeps no
    /// obligations. Not all platforms support filters, and platforms that do may let the user ignore them.
    /// </param>
    /// <param name="defaultLocation"> the default folder or file to start the dialog at, may be empty. </param>
    /// <param name="allowMany"> whether the user is allowed to select multiple entries. Not all platforms support this. </param>
    /// <threadsafety>
    /// This function should be called only from the main thread. The callback may be invoked from the same thread or
    /// from a different one, depending on the OS's constraints.
    /// </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="DialogFileCallback"/>
    /// <seealso cref="DialogFileFilter"/>
    /// <seealso cref="ShowSaveFileDialog"/>
    /// <seealso cref="ShowOpenFolderDialog"/>
    /// <seealso cref="ShowFileDialogWithProperties"/>
    public static void ShowOpenFileDialog(DialogFileCallback callback,
        nint window = 0,
        scoped ReadOnlySpan<DialogFileFilter> filters = default,
        scoped ReadOnlySpan<char> defaultLocation = default,
        bool allowMany = false)
    {
        ArgumentNullException.ThrowIfNull(callback);

        DialogRequest request = new(callback);
        var handle = GCHandle.Alloc(request);
        try
        {
            MarshalDialogRequest(request, filters, defaultLocation, out var filtersNative, out var locationNative);
            SDL_ShowOpenFileDialog(&DialogFileThunk,
                GCHandle.ToIntPtr(handle),
                window,
                filtersNative,
                filters.Length,
                locationNative,
                allowMany);
        }
        catch
        {
            // The native call never ran, so the thunk will not clean up.
            request.FreeNativeBlock();
            handle.Free();
            throw;
        }
    }

    [LibraryImport(SDLLibrary), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void SDL_ShowSaveFileDialog(delegate* unmanaged[Cdecl]<nint, byte**, int, void> callback,
        nint userdata,
        nint window,
        NativeDialogFilter* filters,
        int nfilters,
        byte* defaultLocation);

    /// <code>extern SDL_DECLSPEC void SDLCALL SDL_ShowSaveFileDialog(SDL_DialogFileCallback callback, void *userdata, SDL_Window *window, const SDL_DialogFileFilter *filters, int nfilters, const char *default_location);</code>
    /// <summary>
    /// <para> Displays a dialog that lets the user choose a new or existing file on their filesystem. </para>
    /// <para> This function should only be invoked from the main thread. </para>
    /// <para> This is an asynchronous function; it will return immediately, and the result will be passed to the callback. </para>
    /// <para>
    /// The callback will be invoked with the list of files the user chose: empty if the user canceled the dialog,
    /// <c> null </c> if an error occurred.
    /// </para>
    /// <para> Note that the callback may be called from a different thread than the one the function was invoked on. </para>
    /// <para> The chosen file may or may not already exist. </para>
    /// <para>
    /// On Linux, dialogs may require XDG Portals, which requires DBus, which requires an event-handling loop. Apps that do
    /// not use SDL to handle events should add a call to <see cref="PumpEvents()"/> in their main loop.
    /// </para>
    /// </summary>
    /// <param name="callback"> invoked when the user accepts or cancels the dialog, or an error occurs. </param>
    /// <param name="window"> the window the dialog should be modal for, may be <c> 0 </c>. Not all platforms support this. </param>
    /// <param name="filters">
    /// a list of filters, may be empty. Copied for the lifetime of the dialog — the caller keeps no
    /// obligations. Not all platforms support filters, and platforms that do may let the user ignore them.
    /// </param>
    /// <param name="defaultLocation"> the default folder or file to start the dialog at, may be empty. </param>
    /// <threadsafety>
    /// This function should be called only from the main thread. The callback may be invoked from the same thread or
    /// from a different one, depending on the OS's constraints.
    /// </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="DialogFileCallback"/>
    /// <seealso cref="DialogFileFilter"/>
    /// <seealso cref="ShowOpenFileDialog"/>
    /// <seealso cref="ShowOpenFolderDialog"/>
    /// <seealso cref="ShowFileDialogWithProperties"/>
    public static void ShowSaveFileDialog(DialogFileCallback callback,
        nint window = 0,
        scoped ReadOnlySpan<DialogFileFilter> filters = default,
        scoped ReadOnlySpan<char> defaultLocation = default)
    {
        ArgumentNullException.ThrowIfNull(callback);

        DialogRequest request = new(callback);
        var handle = GCHandle.Alloc(request);
        try
        {
            MarshalDialogRequest(request, filters, defaultLocation, out var filtersNative, out var locationNative);
            SDL_ShowSaveFileDialog(&DialogFileThunk,
                GCHandle.ToIntPtr(handle),
                window,
                filtersNative,
                filters.Length,
                locationNative);
        }
        catch
        {
            request.FreeNativeBlock();
            handle.Free();
            throw;
        }
    }

    [LibraryImport(SDLLibrary), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void SDL_ShowOpenFolderDialog(delegate* unmanaged[Cdecl]<nint, byte**, int, void> callback,
        nint userdata,
        nint window,
        byte* defaultLocation,
        [MarshalAs(UnmanagedType.I1)] bool allowMany);

    /// <code>extern SDL_DECLSPEC void SDLCALL SDL_ShowOpenFolderDialog(SDL_DialogFileCallback callback, void *userdata, SDL_Window *window, const char *default_location, bool allow_many);</code>
    /// <summary>
    /// <para> Displays a dialog that lets the user select a folder on their filesystem. </para>
    /// <para> This function should only be invoked from the main thread. </para>
    /// <para> This is an asynchronous function; it will return immediately, and the result will be passed to the callback. </para>
    /// <para>
    /// The callback will be invoked with the list of folders the user chose: empty if the user canceled the dialog,
    /// <c> null </c> if an error occurred.
    /// </para>
    /// <para> Note that the callback may be called from a different thread than the one the function was invoked on. </para>
    /// <para> Depending on the platform, the user may be allowed to input paths that don't yet exist. </para>
    /// <para>
    /// On Linux, dialogs may require XDG Portals, which requires DBus, which requires an event-handling loop. Apps that do
    /// not use SDL to handle events should add a call to <see cref="PumpEvents()"/> in their main loop.
    /// </para>
    /// </summary>
    /// <param name="callback"> invoked when the user accepts or cancels the dialog, or an error occurs. </param>
    /// <param name="window"> the window the dialog should be modal for, may be <c> 0 </c>. Not all platforms support this. </param>
    /// <param name="defaultLocation"> the default folder to start the dialog at, may be empty. </param>
    /// <param name="allowMany"> whether the user is allowed to select multiple entries. Not all platforms support this. </param>
    /// <threadsafety>
    /// This function should be called only from the main thread. The callback may be invoked from the same thread or
    /// from a different one, depending on the OS's constraints.
    /// </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="DialogFileCallback"/>
    /// <seealso cref="ShowOpenFileDialog"/>
    /// <seealso cref="ShowSaveFileDialog"/>
    /// <seealso cref="ShowFileDialogWithProperties"/>
    public static void ShowOpenFolderDialog(DialogFileCallback callback,
        nint window = 0,
        scoped ReadOnlySpan<char> defaultLocation = default,
        bool allowMany = false)
    {
        ArgumentNullException.ThrowIfNull(callback);

        DialogRequest request = new(callback);
        var handle = GCHandle.Alloc(request);
        try
        {
            MarshalDialogRequest(request, default, defaultLocation, out _, out var locationNative);
            SDL_ShowOpenFolderDialog(&DialogFileThunk, GCHandle.ToIntPtr(handle), window, locationNative, allowMany);
        }
        catch
        {
            request.FreeNativeBlock();
            handle.Free();
            throw;
        }
    }

    [LibraryImport(SDLLibrary), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void SDL_ShowFileDialogWithProperties(FileDialogType type,
        delegate* unmanaged[Cdecl]<nint, byte**, int, void> callback,
        nint userdata,
        PropertiesID props);

    /// <code>extern SDL_DECLSPEC void SDLCALL SDL_ShowFileDialogWithProperties(SDL_FileDialogType type, SDL_DialogFileCallback callback, void *userdata, SDL_PropertiesID props);</code>
    /// <summary>
    /// <para> Create and launch a file dialog with the specified properties. </para>
    /// <para> These are the supported properties: </para>
    /// <list type="bullet">
    /// <item>
    /// <see cref="Props.FileDialogFiltersPointer"/>: a pointer to a list of native SDL_DialogFileFilter structs, which
    /// will be used as filters for file-based selections. Ignored if the dialog is an "Open Folder" dialog. If non-NULL, the array
    /// of filters must remain valid at least until the callback is invoked (the caller manages that memory).
    /// </item>
    /// <item> <see cref="Props.FileDialogNFiltersNumber"/>: the number of filters in the array of filters, if it exists. </item>
    /// <item> <see cref="Props.FileDialogWindowPointer"/>: the window that the dialog should be modal for. </item>
    /// <item> <see cref="Props.FileDialogLocationString"/>: the default folder or file to start the dialog at. </item>
    /// <item> <see cref="Props.FileDialogManyBoolean"/>: true to allow the user to select more than one entry. </item>
    /// <item> <see cref="Props.FileDialogTitleString"/>: the title for the dialog. </item>
    /// <item> <see cref="Props.FileDialogAcceptString"/>: the label that the accept button should have. </item>
    /// <item> <see cref="Props.FileDialogCancelString"/>: the label that the cancel button should have. </item>
    /// </list>
    /// <para> Note that each platform may or may not support any of the properties. </para>
    /// </summary>
    /// <param name="type"> the type of file dialog. </param>
    /// <param name="callback"> invoked when the user accepts or cancels the dialog, or an error occurs. </param>
    /// <param name="props"> the properties to use. </param>
    /// <threadsafety>
    /// This function should be called only from the main thread. The callback may be invoked from the same thread or
    /// from a different one, depending on the OS's constraints.
    /// </threadsafety>
    /// <since> This function is available since SDL 3.1.8. </since>
    /// <seealso cref="FileDialogType"/>
    /// <seealso cref="DialogFileCallback"/>
    /// <seealso cref="ShowOpenFileDialog"/>
    /// <seealso cref="ShowSaveFileDialog"/>
    /// <seealso cref="ShowOpenFolderDialog"/>
    public static void ShowFileDialogWithProperties(FileDialogType type, DialogFileCallback callback, PropertiesID props)
    {
        ArgumentNullException.ThrowIfNull(callback);

        DialogRequest request = new(callback);
        var handle = GCHandle.Alloc(request);
        try
        {
            SDL_ShowFileDialogWithProperties(type, &DialogFileThunk, GCHandle.ToIntPtr(handle), props);
        }
        catch
        {
            handle.Free();
            throw;
        }
    }
}
