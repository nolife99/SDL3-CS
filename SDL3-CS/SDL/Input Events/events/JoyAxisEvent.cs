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

using System.Runtime.InteropServices;

/// <summary> Joystick axis motion event structure (event.jaxis.*) </summary>
/// <since> This struct is available since SDL 3.2.0 </since>
[StructLayout(LayoutKind.Sequential)]
public struct JoyAxisEvent
{
    /// <summary>
    /// <see cref="EventType.JoystickAxisMotion"/>
    /// </summary>
    public EventType Type;

    uint _reserved;

    /// <summary> In nanoseconds, populated using <see cref="SDL.GetTicksNS"/> </summary>
    public ulong Timestamp;

    /// <summary> The joystick instance id </summary>
    public uint Which;

    /// <summary> The joystick axis index </summary>
    public byte Axis;

    byte _padding1, _padding2, _padding3;

    /// <summary> The axis value (range: -32768 to 32767) </summary>
    public short Value;

    byte _padding4;
}