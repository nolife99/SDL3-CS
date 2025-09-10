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

/// <summary> Audio device event structure (event.adevice.*) </summary>
/// <remarks>
/// Note that SDL will send a <see cref="EventType.AudioDeviceAdded"/> event for every device it discovers during
/// initialization. After that, this event will only arrive when a device is hotplugged during the program's run.
/// </remarks>
/// <since> This struct is available since SDL 3.2.0 </since>
[StructLayout(LayoutKind.Sequential)]
public struct AudioDeviceEvent
{
    /// <summary> false if a playback device, true if a recording device. </summary>
    public bool Recording
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => recording != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => recording = value ? (byte)1 : (byte)0;
    }

    /// <summary>
    /// <see cref="EventType.AudioDeviceAdded"/>, or <see cref="EventType.AudioDeviceRemoved"/>, or
    /// <see cref="EventType.AudioDeviceFormatChanged"/>
    /// </summary>
    public EventType Type;

    uint _reserved;

    /// <summary> In nanoseconds, populated using <see cref="SDL.GetTicksNS"/> </summary>
    public ulong Timestamp;

    /// <summary> SDL_AudioDeviceID for the device being added or removed or changing </summary>
    public uint Which;

    byte recording, _padding1, _padding2, _padding3;
}