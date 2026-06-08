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

// This file is an altered version (storybrew fork): the log output callback is a fully managed
// delegate receiving the decoded message as a transient span; native thunking lives in PInvoke.cs.

namespace SDL3;

using System;

/// <code>typedef void (SDLCALL *SDL_LogOutputFunction)(void *userdata, int category, SDL_LogPriority priority, const char *message);</code>
/// <summary> The prototype for the log output callback function. </summary>
/// <remarks>
/// <para>
/// This function is called by SDL when there is new text to be logged. A mutex is held so that this function is never
/// called by more than one thread at once.
/// </para>
/// <para>
/// The message span is decoded without allocation and is only valid during the callback — call
/// <c> message.ToString() </c> for text that must outlive it. Exceptions never propagate into native SDL;
/// they are routed to <see cref="SDL.UnhandledCallbackException"/> (or tracing — never back into the SDL
/// log, which would recurse).
/// </para>
/// </remarks>
/// <param name="category"> the category of the message. </param>
/// <param name="priority"> the priority of the message. </param>
/// <param name="message"> the message being output, valid only during the callback. </param>
/// <since> This datatype is available since SDL 3.2.0 </since>
public delegate void LogOutputFunction(LogCategory category, LogPriority priority, scoped ReadOnlySpan<char> message);