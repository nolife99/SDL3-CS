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

// This file is an altered version (storybrew fork): the enumeration callbacks are fully managed and
// allocation-free — entries arrive as transient UTF-8 spans, and the generic form passes a typed state
// without boxing or closure captures. Native thunking lives in PInvoke.cs.

namespace SDL3;

using System;

/// <code>typedef SDL_EnumerationResult (SDLCALL *SDL_EnumerateDirectoryCallback)(void *userdata, const char *dirname, const char *fname);</code>
/// <summary>
/// <para> Callback for directory enumeration. </para>
/// <para>
/// Enumeration of directory entries will continue until either all entries have been provided to the callback, or the
/// callback has requested a stop through its return value.
/// </para>
/// <para>
/// Returning <see cref="SDL.EnumerationResult.Continue"/> will let enumeration proceed, calling the callback with further
/// entries. <see cref="SDL.EnumerationResult.Success"/> and <see cref="SDL.EnumerationResult.Failure"/> will terminate the
/// enumeration early, and dictate the return value of the enumeration function itself.
/// </para>
/// <para>
/// <c> directory </c> is guaranteed to end with a path separator (<c> \\ </c> on Windows, <c> / </c> on most other
/// platforms). Both spans are UTF-8 views over SDL-owned memory, valid only during the callback — decode or copy what
/// must outlive it. Exceptions never propagate into native SDL: they are routed to
/// <see cref="SDL.UnhandledCallbackException"/> and terminate the enumeration with
/// <see cref="SDL.EnumerationResult.Failure"/>.
/// </para>
/// </summary>
/// <param name="directory"> the directory that is being enumerated, as transient UTF-8 bytes. </param>
/// <param name="file"> the next entry in the enumeration, as transient UTF-8 bytes. </param>
/// <returns> how the enumeration should proceed. </returns>
/// <since> This datatype is available since SDL 3.2.0 </since>
/// <seealso cref="SDL.EnumerateDirectory(string, EnumerateDirectoryCallback)"/>
public delegate SDL.EnumerationResult EnumerateDirectoryCallback(ReadOnlySpan<byte> directory, ReadOnlySpan<byte> file);

/// <summary>
///     Stateful variant of <see cref="EnumerateDirectoryCallback"/>: <paramref name="state"/> is the value passed to
///     <see cref="SDL.EnumerateDirectory{T}(string, EnumerateDirectoryCallback{T}, T)"/>, delivered without boxing —
///     use a static lambda and collect results into it instead of capturing.
/// </summary>
public delegate SDL.EnumerationResult EnumerateDirectoryCallback<in T>(T state,
    ReadOnlySpan<byte> directory,
    ReadOnlySpan<byte> file);