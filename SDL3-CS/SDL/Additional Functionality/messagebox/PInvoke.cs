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

using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

public static unsafe partial class SDL
{
    [LibraryImport("SDL3", EntryPoint = "SDL_ShowMessageBox"), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]),
     MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool ShowMessageBox(void* data, out int buttonId);

    /// <code>extern SDL_DECLSPEC bool SDLCALL SDL_ShowMessageBox(const SDL_MessageBoxData *messageboxdata, int *buttonid);</code>
    /// <summary>
    ///     <para> Create a modal message box. </para>
    ///     <para> If your needs aren't complex, it might be easier to use <see cref="ShowSimpleMessageBox"/>. </para>
    ///     <para>
    ///         This function should be called on the thread that created the parent window, or on the main thread if the
    ///         messagebox has no parent. It will block execution of that thread until the user clicks a button or closes the
    ///         messagebox.
    ///     </para>
    ///     <para>
    ///         This function may be called at any time, even before <see cref="Init"/>. This makes it useful for reporting
    ///         errors like a failure to create a renderer or OpenGL context.
    ///     </para>
    ///     <para> On X11, SDL rolls its own dialog box with X11 primitives instead of a formal toolkit like GTK+ or Qt. </para>
    ///     <para>
    ///         Note that if <see cref="Init"/> would fail because there isn't any available video target, this function is
    ///         likely to fail for the same reasons. If this is a concern, check the return value from this function and fall back
    ///         to writing to stderr if you can.
    ///     </para>
    /// </summary>
    /// <param name="messageboxdata"> the <see cref="MessageBoxData"/> structure with title, text and other options. </param>
    /// <param name="buttonid"> the pointer to which user id of hit button should be copied. </param>
    /// <returns> <c> true </c> on success or <c> false </c> on failure; call <see cref="GetError"/> for more information. </returns>
    /// <since> This function is available since SDL 3.2.0 </since>
    public static bool ShowMessageBox(scoped ref readonly MessageBoxData messageboxdata, out int buttonid)
    {
        var pinned = Unsafe.AsRef(in messageboxdata).Pin();
        try
        {
            return ShowMessageBox(&pinned, out buttonid);
        }
        finally
        {
            Unsafe.AsRef(in messageboxdata).Unpin();
        }
    }

    /// <code>extern SDL_DECLSPEC bool SDLCALL SDL_ShowSimpleMessageBox(SDL_MessageBoxFlags flags, const char *title, const char *message, SDL_Window *window);</code>
    /// <summary>
    ///     <para> Display a simple modal message box. </para>
    ///     <para> If your needs aren't complex, this function is preferred over <see cref="ShowMessageBox"/>. </para>
    ///     <para> <c> flags </c> may be any of the following: </para>
    ///     <list type="bullet">
    ///         <item> <see cref="MessageBoxFlags.Error"/>: error dialog </item>
    ///         <item> <see cref="MessageBoxFlags.Warning"/>: warning dialog </item>
    ///         <item> <see cref="MessageBoxFlags.Information"/>: informational dialog </item>
    ///     </list>
    ///     <para>
    ///         This function should be called on the thread that created the parent window, or on the main thread if the
    ///         messagebox has no parent. It will block execution of that thread until the user clicks a button or closes the
    ///         messagebox.
    ///     </para>
    ///     <para>
    ///         This function may be called at any time, even before <see cref="Init"/>. This makes it useful for reporting
    ///         errors like a failure to create a renderer or OpenGL context.
    ///     </para>
    ///     <para> On X11, SDL rolls its own dialog box with X11 primitives instead of a formal toolkit like GTK+ or Qt. </para>
    ///     <para>
    ///         Note that if <see cref="Init"/> would fail because there isn't any available video target, this function is
    ///         likely to fail for the same reasons. If this is a concern, check the return value from this function and fall back
    ///         to writing to stderr if you can.
    ///     </para>
    /// </summary>
    /// <param name="flags"> an <see cref="MessageBoxFlags"/> value. </param>
    /// <param name="title"> UTF-8 title text. </param>
    /// <param name="message"> UTF-8 message text. </param>
    /// <param name="window"> the parent window, or <c> null </c> for no parent. </param>
    /// <returns> <c> true </c> on success or <c> false </c> on failure; call <see cref="GetError"/> for more information. </returns>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="ShowMessageBox"/>
    public static bool ShowSimpleMessageBox(MessageBoxFlags flags,
        ReadOnlySpan<char> title,
        ReadOnlySpan<char> message,
        nint window)
    {
        scoped Span<byte> titleSpan = default, messageSpan = default;
        byte[]? titleArray = null, messageArray = null;

        if (!title.IsWhiteSpace())
        {
            var byteCount = Encoding.UTF8.GetByteCount(title) + 1;
            titleSpan = byteCount <= 512 ?
                stackalloc byte[byteCount] :
                (titleArray = ArrayPool<byte>.Shared.Rent(byteCount)).AsSpan(0, byteCount);

            Encoding.UTF8.GetBytes(title, titleSpan);
            titleSpan[^1] = 0;
        }

        if (!message.IsWhiteSpace())
        {
            var byteCount = Encoding.UTF8.GetByteCount(message) + 1;
            messageSpan = byteCount <= 512 ?
                stackalloc byte[byteCount] :
                (messageArray = ArrayPool<byte>.Shared.Rent(byteCount)).AsSpan(0, byteCount);

            Encoding.UTF8.GetBytes(message, messageSpan);
            messageSpan[^1] = 0;
        }

        try
        {
            fixed (byte* pTitle = titleSpan)
            fixed (byte* pMsg = messageSpan)
                return ShowSimpleMessageBoxNative(flags, pTitle, pMsg, window);
        }
        finally
        {
            if (titleArray is not null) ArrayPool<byte>.Shared.Return(titleArray);
            if (messageArray is not null) ArrayPool<byte>.Shared.Return(messageArray);
        }
    }

    [LibraryImport(SDLLibrary, EntryPoint = "SDL_ShowSimpleMessageBox"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool ShowSimpleMessageBoxNative(MessageBoxFlags flags,
        byte* title,
        byte* message,
        nint window);
}