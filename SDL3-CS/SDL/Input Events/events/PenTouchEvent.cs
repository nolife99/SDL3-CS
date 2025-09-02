﻿#region License

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

using System.Runtime.InteropServices;

public static partial class SDL
{
    /// <summary>
    ///     <para> Pressure-sensitive pen touched event structure (event.ptouch.*) </para>
    ///     <para> These events come when a pen touches a surface (a tablet, etc), or lifts off from one. </para>
    /// </summary>
    /// <since> This struct is available since SDL 3.2.0 </since>
    [StructLayout(LayoutKind.Sequential)]
    public struct PenTouchEvent
    {
        /// <summary> <see cref="EventType.PenDown"/> or <see cref="EventType.PenUp"/> </summary>
        public EventType Type;

        uint _reserved;

        /// <summary> In nanoseconds, populated using <see cref="GetTicksNS"/> </summary>
        public ulong Timestamp;

        /// <summary> The window with pen focus, if any </summary>
        public uint WindowID;

        /// <summary> The pen instance id </summary>
        public uint Which;

        /// <summary> Complete pen input state at time of event </summary>
        public PenInputFlags PenState;

        /// <summary> X coordinate, relative to window </summary>
        public float X;

        /// <summary> Y coordinate, relative to window </summary>
        public float Y;

        /// <summary> true if eraser end is used (not all pens support this). </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool Eraser;

        /// <summary> true if the pen is touching or false if the pen is lifted off </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool Down;
    }
}