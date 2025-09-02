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
    /// <summary> Mouse button event structure (event.button.*) </summary>
    /// <since> This struct is available since SDL 3.2.0 </since>
    [StructLayout(LayoutKind.Sequential)]
    public struct MouseButtonEvent
    {
        /// <summary> <see cref="EventType.GamepadButtonDown"/> or <see cref="EventType.MouseButtonUp"/> </summary>
        public EventType Type;

        uint _reserved;

        /// <summary> In nanoseconds, populated using <see cref="GetTicksNS"/> </summary>
        public ulong Timestamp;

        /// <summary> The window with mouse focus, if any </summary>
        public uint WindowID;

        /// <summary> The mouse instance id in relative mode, <see cref="TouchMouseID"/> for touch events, or 0 </summary>
        public uint Which;

        /// <summary> The mouse button index </summary>
        public byte Button;

        /// <summary> true if the button is pressed </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool Down;

        /// <summary> 1 for single-click, 2 for double-click, etc. </summary>
        public byte Clicks;

        byte _padding;

        /// <summary> X coordinate, relative to window </summary>
        public float X;

        /// <summary> Y coordinate, relative to window </summary>
        public float Y;
    }
}