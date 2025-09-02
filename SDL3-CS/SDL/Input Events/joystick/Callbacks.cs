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

public static partial class SDL
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void VirtualJoystickCleanupCallback(nint userdata);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public delegate bool VirtualJoystickRumbleCallback(nint userdata,
        ushort lowFrequencyRumble,
        ushort highFrequencyRumble);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public delegate bool VirtualJoystickRumbleTriggersCallback(nint userdata, ushort leftRumble, ushort rightRumble);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public delegate bool VirtualJoystickSendEffectCallback(nint userdata, nint data, int size);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public delegate bool VirtualJoystickSetLEDCallback(nint userdata, byte red, byte green, byte blue);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void VirtualJoystickSetPlayerIndexCallback(nint userdata, int playerIndex);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public delegate bool VirtualJoystickSetSensorsEnabledCallback(nint userdata, bool enabled);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void VirtualJoystickUpdateCallback(nint userdata);
}