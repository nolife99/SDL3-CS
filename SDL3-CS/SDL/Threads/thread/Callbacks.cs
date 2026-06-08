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

// This file is an altered version (storybrew fork): thread callbacks are fully managed delegates
// (state is captured by closure); native thunking and rooting live in PInvoke.cs.

namespace SDL3;

public static partial class SDL
{
    /// <code>typedef int (SDLCALL * SDL_ThreadFunction) (void *data);</code>
    /// <summary>
    /// <para> The function passed to <see cref="CreateThread"/> as the new thread's entry point. </para>
    /// <para>
    /// Exceptions never propagate into native SDL: they are routed to
    /// <see cref="UnhandledCallbackException"/> and the thread reports -1.
    /// </para>
    /// </summary>
    /// <returns> a value that can be reported through <see cref="WaitThread"/>. </returns>
    /// <since> This datatype is available since SDL 3.2.0 </since>
    public delegate int ThreadFunction();

    /// <code>typedef void (SDLCALL *SDL_TLSDestructorCallback)(void *value);</code>
    /// <summary>
    /// <para> The callback used to cleanup data passed to <see cref="SetTLS"/>. </para>
    /// <para> This is called when a thread exits, to allow an app to free any resources. </para>
    /// </summary>
    /// <param name="value"> a pointer previously handed to <see cref="SetTLS"/>. </param>
    /// <since> This datatype is available since SDL 3.2.0 </since>
    /// <seealso cref="SetTLS"/>
    public delegate void TLSDestructorCallback(nint value);
}
