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

// This file is an altered version (storybrew fork): the filter is a fully managed delegate (state is
// captured by closure instead of a userdata pointer); native thunking lives in PInvoke.cs, which also
// keeps registered delegates rooted so they can never be garbage-collected behind SDL's back.

#endregion

namespace SDL3;

/// <code>typedef bool (SDLCALL *SDL_EventFilter)(void *userdata, SDL_Event *event);</code>
/// <summary> A callback that watches or filters the event queue. </summary>
/// <param name="event"> the event that triggered the callback. </param>
/// <returns>
/// true to permit event to be added to the queue, and false to disallow it. When used with
/// <see cref="SDL.AddEventWatch"/>, the return value is ignored.
/// </returns>
/// <threadsafety>
/// SDL may call this callback at any time from any thread; the application is responsible for locking resources
/// the callback touches that need to be protected. Exceptions never propagate into native SDL: they are routed to
/// <see cref="SDL.UnhandledCallbackException"/> (or the SDL log) and the event is permitted.
/// </threadsafety>
/// <since> This datatype is available since SDL 3.2.0 </since>
/// <seealso cref="SDL.SetEventFilter"/>
/// <seealso cref="SDL.AddEventWatch"/>
public delegate bool EventFilter(ref readonly Event @event);

/// <summary>
///     Stateful variant of <see cref="EventFilter"/> for the call-scoped
///     <see cref="SDL.FilterEvents{T}(EventFilter{T}, T)"/>: <paramref name="state"/> is delivered without
///     boxing — use a static lambda instead of capturing.
/// </summary>
public delegate bool EventFilter<in T>(T state, ref readonly Event @event);