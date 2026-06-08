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

public partial class SDL
{
    /// <code>typedef void (SDLCALL *SDL_TrayCallback)(void *userdata, SDL_TrayEntry *entry);</code>
    /// <summary>
    /// A callback that is invoked when a tray entry is selected. State is captured by closure; the
    /// delegate is rooted by the wrapper until the entry's callback is replaced or cleared. Exceptions
    /// never propagate into native SDL: they are routed to <see cref="UnhandledCallbackException"/>.
    /// </summary>
    /// <param name="entry"> the tray entry that was selected. </param>
    /// <since> This datatype is available since SDL 3.2.0 </since>
    /// <seealso cref="SetTrayEntryCallback"/>
    public delegate void TrayCallback(nint entry);
}
