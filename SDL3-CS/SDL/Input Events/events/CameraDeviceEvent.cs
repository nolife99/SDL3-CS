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

/// <summary> Camera device event structure (event.cdevice.*) </summary>
/// <requires> SDL 3.2.0 </requires>
[StructLayout(LayoutKind.Sequential)]
public struct CameraDeviceEvent
{
    /// <summary>
    /// <see cref="SDL.EventType.CameraDeviceAdded"/>, <see cref="SDL.EventType.CameraDeviceRemoved"/>,
    /// <see cref="SDL.EventType.CameraDeviceApproved"/>, <see cref="SDL.EventType.CameraDeviceDenied"/>
    /// </summary>
    public SDL.EventType Type;

    uint _reserved;

    /// <summary> In nanoseconds, populated using <see cref="SDL.GetTicksNS"/> </summary>
    public ulong Timestamp;

    /// <summary> SDL_CameraID for the device being added or removed or changing </summary>
    public uint Which;
}