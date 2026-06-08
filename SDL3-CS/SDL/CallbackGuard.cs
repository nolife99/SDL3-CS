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

// This file is an addition of the storybrew fork: shared exception barrier for managed callbacks
// invoked from native SDL.

#endregion

namespace SDL3;

using System;
using System.Diagnostics;

public static partial class SDL
{
    /// <summary>
    /// <para>
    /// Raised when a managed callback invoked from native SDL throws. A managed exception must never
    /// unwind into native code (the callback often runs on an SDL-owned thread, where unwinding would
    /// corrupt or abort the process), so the wrapper catches it at the boundary and reports it here.
    /// </para>
    /// <para>
    /// When no handler is attached, the exception is written to the SDL log under
    /// <see cref="LogCategory.Application"/>. Handlers must not throw; anything they throw is swallowed
    /// after a best-effort trace.
    /// </para>
    /// </summary>
    public static event Action<Exception>? UnhandledCallbackException;

    /// <summary>
    ///     Boundary reporter used by native-to-managed thunks. Guaranteed not to throw — this is the
    ///     last line of defense before native code. Thunks that are themselves part of the SDL logging
    ///     pipeline pass <paramref name="allowSdlLog"/> false to avoid re-entering it.
    /// </summary>
    internal static void ReportCallbackException(Exception exception, bool allowSdlLog = true)
    {
        try
        {
            var handler = UnhandledCallbackException;
            if (handler is not null)
            {
                handler(exception);
                return;
            }

            if (!allowSdlLog)
            {
                Trace.WriteLine($"Unhandled exception in an SDL callback: {exception}");
                return;
            }

            LogError(LogCategory.Application, $"Unhandled exception in an SDL callback: {exception}");
        }
        catch (Exception secondary)
        {
            try
            {
                Trace.WriteLine($"SDL callback exception reporting failed: {secondary}\noriginal: {exception}");
            }
            catch
            {
                // Nothing left to do; swallowing is the only option at a native boundary.
            }
        }
    }
}
