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

// This file is an altered version (storybrew fork): clipboard callbacks are fully managed; the data
// provider returns a transient span that the wrapper copies into SDL-visible storage. Native thunking
// and lifetime live in PInvoke.cs.

namespace SDL3;

using System;

public static partial class SDL
{
    /// <code>typedef void (SDLCALL *SDL_ClipboardCleanupCallback)(void *userdata);</code>
    /// <summary>
    /// Callback function that will be called when the clipboard is cleared, or when new data is set.
    /// Invoked exactly once per offer; afterwards the offer's resources are released.
    /// </summary>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="SetClipboardData(ClipboardDataCallback, ClipboardCleanupCallback, string[])"/>
    public delegate void ClipboardCleanupCallback();

    /// <code>typedef const void *(SDLCALL *SDL_ClipboardDataCallback)(void *userdata, const char *mime_type, size_t *size);</code>
    /// <summary>
    /// <para> Callback function that will be called when data for the specified mime-type is requested by the OS. </para>
    /// <para>
    /// The callback function is called with <c> null </c> as the mime_type when the clipboard is cleared or new data is set.
    /// The clipboard is automatically cleared in <see cref="Quit()"/>.
    /// </para>
    /// <para>
    /// The returned span is copied by the wrapper into storage that satisfies SDL's retention requirement,
    /// so it may be transient (pooled or stack memory included). Returning an empty span sends no data —
    /// receivers may handle that poorly, so prefer always producing data for offered mime-types.
    /// Exceptions never propagate into native SDL: they are routed to
    /// <see cref="UnhandledCallbackException"/> and no data is sent.
    /// </para>
    /// </summary>
    /// <param name="mimeType"> the requested mime-type as transient UTF-8 bytes; empty on clear. </param>
    /// <returns> the data for the provided mime-type. </returns>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="SetClipboardData(ClipboardDataCallback, ClipboardCleanupCallback, string[])"/>
    public delegate ReadOnlySpan<byte> ClipboardDataCallback(ReadOnlySpan<byte> mimeType);
}
