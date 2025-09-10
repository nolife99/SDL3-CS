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

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

/// <summary> Gamepad button event structure (event.gbutton.*) </summary>
/// <since> This struct is available since SDL 3.2.0 </since>
[StructLayout(LayoutKind.Sequential)]
public struct GamepadButtonEvent
{
    /// <summary> true if the button is pressed </summary>
    public bool Down
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => down != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => down = value ? (byte)1 : (byte)0;
    }

    /// <summary> <see cref="EventType.GamepadButtonDown"/> or <see cref="EventType.GamepadButtonUp"/> </summary>
    public EventType Type;

    uint _reserved;

    /// <summary> In nanoseconds, populated using <see cref="SDL.GetTicksNS"/> </summary>
    public ulong Timestamp;

    /// <summary> The joystick instance id </summary>
    public uint Which;

    /// <summary> The gamepad button </summary>
    public GamepadButton Button
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Unsafe.As<byte, GamepadButton>(ref button);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => button = Unsafe.As<GamepadButton, byte>(ref value);
    }

    byte down, button, _padding1, _padding2;
}