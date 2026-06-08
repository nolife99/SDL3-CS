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

#endregion

namespace SDL3;

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

public static partial class SDL
{
    /// <code>extern SDL_DECLSPEC bool SDLCALL SDL_SetHintWithPriority(const char *name, const char *value, SDL_HintPriority priority);</code>
    /// <summary>
    /// <para> Set a hint with a specific priority. </para>
    /// <para>
    /// The priority controls the behavior when setting a hint that already has a value. Hints will replace existing hints of
    /// their priority and lower. Environment variables are considered to have override priority.
    /// </para>
    /// </summary>
    /// <param name="name"> the hint to set. </param>
    /// <param name="value"> the value of the hint variable. </param>
    /// <param name="priority"> the <see cref="HintPriority"/> level for the hint. </param>
    /// <returns> <c> true </c> on success or <c> false </c> on failure; call <see cref="GetError"/> for more information. </returns>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="GetHint"/>
    /// <seealso cref="ResetHint"/>
    /// <seealso cref="SetHint"/>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_SetHintWithPriority"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool SetHintWithPriority([MarshalAs(UnmanagedType.LPUTF8Str)] string name,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string value,
        HintPriority priority);

    /// <code>extern SDL_DECLSPEC bool SDLCALL SDL_SetHint(const char *name, const char *value);</code>
    /// <summary>
    /// <para> Set a hint with normal priority. </para>
    /// <para>
    /// Hints will not be set if there is an existing override hint or environment variable that takes precedence. You can
    /// use <see cref="SetHintWithPriority"/> to set the hint with override priority instead.
    /// </para>
    /// </summary>
    /// <param name="name"> the hint to set. </param>
    /// <param name="value"> the value of the hint variable. </param>
    /// <returns> <c> true </c> on success or <c> false </c> on failure; call <see cref="GetError"/> for more information. </returns>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="GetHint"/>
    /// <seealso cref="ResetHint"/>
    /// <seealso cref="SetHintWithPriority"/>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_SetHint"), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool SetHint([MarshalAs(UnmanagedType.LPUTF8Str)] string name,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string value);

    /// <code>extern SDL_DECLSPEC bool SDLCALL SDL_ResetHint(const char *name);</code>
    /// <summary>
    /// <para> Reset a hint to the default value. </para>
    /// <para>
    /// This will reset a hint to the value of the environment variable, or <c> null </c> if the environment isn't set.
    /// Callbacks will be called normally with this change.
    /// </para>
    /// </summary>
    /// <param name="name"> the hint to set. </param>
    /// <returns> <c> true </c> on success or <c> false </c> on failure; call <see cref="GetError"/> for more information. </returns>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="SetHint"/>
    /// <seealso cref="ResetHints"/>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_ResetHint"), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool ResetHint([MarshalAs(UnmanagedType.LPUTF8Str)] string name);

    /// <code>extern SDL_DECLSPEC void SDLCALL SDL_ResetHints(void);</code>
    /// <summary>
    /// <para> Reset all hints to the default values. </para>
    /// <para>
    /// This will reset all hints to the value of the associated environment variable, or <c> null </c> if the environment
    /// isn't set. Callbacks will be called normally with this change.
    /// </para>
    /// </summary>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="ResetHint"/>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_ResetHints"), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial void ResetHints();

    [LibraryImport(SDLLibrary, EntryPoint = "SDL_GetHint"), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial nint SDL_GetHint([MarshalAs(UnmanagedType.LPUTF8Str)] string name);

    /// <code>extern SDL_DECLSPEC const char *SDLCALL SDL_GetHint(const char *name);</code>
    /// <summary> Get the value of a hint. </summary>
    /// <param name="name"> name the hint to query. </param>
    /// <returns> the string value of a hint or <c> null </c> if the hint isn't set. </returns>
    /// <threadsafety>
    /// It is safe to call this function from any thread, however the return value only remains valid until the hint
    /// is changed; if another thread might do so, the app should supply locks and/or make a copy of the string. Note that using a
    /// hint callback instead is always thread-safe, as SDL holds a lock on the thread subsystem during the callback.
    /// </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="SetHint"/>
    /// <seealso cref="SetHintWithPriority"/>
    public static string? GetHint(string name)
    {
        var value = SDL_GetHint(name);
        return value == nint.Zero ? null : Marshal.PtrToStringUTF8(value);
    }

    /// <code>extern SDL_DECLSPEC bool SDLCALL SDL_GetHintBoolean(const char *name, bool default_value);</code>
    /// <summary> Get the boolean value of a hint variable. </summary>
    /// <param name="name"> the name of the hint to get the boolean value from. </param>
    /// <param name="defaultValue"> the value to return if the hint does not exist. </param>
    /// <returns> the boolean value of a hint or the provided default value if the hint does not exist. </returns>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="GetHint"/>
    /// <seealso cref="SetHint"/>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_GetHintBoolean"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool GetHintBoolean([MarshalAs(UnmanagedType.LPUTF8Str)] string name,
        [MarshalAs(UnmanagedType.I1)] bool defaultValue);

    /// <code>extern SDL_DECLSPEC bool SDLCALL SDL_AddHintCallback(const char *name, SDL_HintCallback callback, void *userdata);</code>
    /// <summary>
    /// <para> Add a function to watch a particular hint. </para>
    /// <para>
    /// The callback function is called _during_ this function, to provide it an initial value, and again each time the
    /// hint's value changes.
    /// </para>
    /// </summary>
    /// <param name="name"> the hint to watch. </param>
    /// <param name="callback"> An <see cref="HintCallback"/> function that will be called when the hint value changes. </param>
    /// <param name="userdata"> a pointer to pass to the callback function. </param>
    /// <returns> <c> true </c> on success or <c> false </c> on failure; call <see cref="GetError"/> for more information. </returns>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="RemoveHintCallback"/>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_AddHintCallback"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial int SDL_AddHintCallback(byte* name,
        delegate* unmanaged[Cdecl]<nint, byte*, byte*, byte*, void> callback,
        nint userdata);

    // Hint watch roots: one handle per delegate instance, reference-counted because the same delegate
    // may watch several hints while SDL identifies each registration by (name, thunk, userdata).
    static readonly Dictionary<HintCallback, (GCHandle Handle, int Count)> hintCallbacks = new();

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static unsafe void HintThunk(nint userdata, byte* name, byte* oldValue, byte* newValue)
    {
        try
        {
            // Zero-copy UTF-8 views over SDL's strings; empty when SDL passes null.
            ((HintCallback)GCHandle.FromIntPtr(userdata).Target!)(
                MemoryMarshal.CreateReadOnlySpanFromNullTerminated(name),
                MemoryMarshal.CreateReadOnlySpanFromNullTerminated(oldValue),
                MemoryMarshal.CreateReadOnlySpanFromNullTerminated(newValue));
        }
        catch (Exception exception)
        {
            ReportCallbackException(exception);
        }
    }

    /// <summary>
    ///     See the native documentation above. The callback is also invoked during this call with the
    ///     hint's current value. The delegate is rooted by the wrapper until removed via
    ///     <see cref="RemoveHintCallback"/> with the same instance.
    /// </summary>
    public static unsafe int AddHintCallback(scoped ReadOnlySpan<char> name, HintCallback callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        var byteCount = Encoding.UTF8.GetByteCount(name) + 1;
        byte[]? rented = null;
        scoped Span<byte> utf8 = byteCount <= 512 ?
            stackalloc byte[byteCount] :
            (rented = ArrayPool<byte>.Shared.Rent(byteCount)).AsSpan(0, byteCount);

        Encoding.UTF8.GetBytes(name, utf8);
        utf8[^1] = 0;

        try
        {
            lock (hintCallbacks)
            {
                var entry = hintCallbacks.TryGetValue(callback, out var existing)
                    ? existing
                    : (GCHandle.Alloc(callback), 0);

                int result;
                fixed (byte* namePtr = utf8)
                    result = SDL_AddHintCallback(namePtr, &HintThunk, GCHandle.ToIntPtr(entry.Item1));

                if (result == 0 && entry.Item2 == 0)
                {
                    entry.Item1.Free();
                    return result;
                }

                hintCallbacks[callback] = (entry.Item1, entry.Item2 + 1);
                return result;
            }
        }
        finally
        {
            if (rented is not null) ArrayPool<byte>.Shared.Return(rented);
        }
    }

    [LibraryImport(SDLLibrary, EntryPoint = "SDL_RemoveHintCallback"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial void SDL_RemoveHintCallback(byte* name,
        delegate* unmanaged[Cdecl]<nint, byte*, byte*, byte*, void> callback,
        nint userdata);

    /// <summary>Remove a hint watch added with <see cref="AddHintCallback"/> (same delegate instance).</summary>
    public static unsafe void RemoveHintCallback(scoped ReadOnlySpan<char> name, HintCallback callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        var byteCount = Encoding.UTF8.GetByteCount(name) + 1;
        byte[]? rented = null;
        scoped Span<byte> utf8 = byteCount <= 512 ?
            stackalloc byte[byteCount] :
            (rented = ArrayPool<byte>.Shared.Rent(byteCount)).AsSpan(0, byteCount);

        Encoding.UTF8.GetBytes(name, utf8);
        utf8[^1] = 0;

        try
        {
            lock (hintCallbacks)
            {
                if (!hintCallbacks.TryGetValue(callback, out var entry)) return;

                fixed (byte* namePtr = utf8)
                    SDL_RemoveHintCallback(namePtr, &HintThunk, GCHandle.ToIntPtr(entry.Handle));

                if (entry.Count <= 1)
                {
                    entry.Handle.Free();
                    hintCallbacks.Remove(callback);
                }
                else hintCallbacks[callback] = (entry.Handle, entry.Count - 1);
            }
        }
        finally
        {
            if (rented is not null) ArrayPool<byte>.Shared.Return(rented);
        }
    }
}
