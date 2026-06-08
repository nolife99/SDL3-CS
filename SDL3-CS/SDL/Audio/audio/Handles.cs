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

// This file is part of the storybrew fork: strongly typed native handles. Each wraps exactly one
// pointer (or id) so distinct native object kinds cannot be interchanged by accident; the structs
// are blittable and pass through LibraryImport unchanged.

namespace SDL3;

using System;
using System.Runtime.InteropServices;

/// <summary> Opaque handle to a native <c> SDL_AudioStream* </c>. </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct AudioStreamHandle(nint value) : IEquatable<AudioStreamHandle>
{
    public readonly nint Value = value;

    public static AudioStreamHandle Null => default;
    public bool IsNull => Value == 0;

    public bool Equals(AudioStreamHandle other) => Value == other.Value;
    public override bool Equals(object obj) => obj is AudioStreamHandle other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => $"SDL_AudioStream*(0x{Value:x})";

    public static bool operator ==(AudioStreamHandle left, AudioStreamHandle right) => left.Equals(right);
    public static bool operator !=(AudioStreamHandle left, AudioStreamHandle right) => !left.Equals(right);

    public static explicit operator nint(AudioStreamHandle handle) => handle.Value;
}

/// <summary> Opaque handle to a native <c> SDL_IOStream* </c>. </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct IOStreamHandle(nint value) : IEquatable<IOStreamHandle>
{
    public readonly nint Value = value;

    public static IOStreamHandle Null => default;
    public bool IsNull => Value == 0;

    public bool Equals(IOStreamHandle other) => Value == other.Value;
    public override bool Equals(object obj) => obj is IOStreamHandle other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => $"SDL_IOStream*(0x{Value:x})";

    public static bool operator ==(IOStreamHandle left, IOStreamHandle right) => left.Equals(right);
    public static bool operator !=(IOStreamHandle left, IOStreamHandle right) => !left.Equals(right);

    public static explicit operator nint(IOStreamHandle handle) => handle.Value;
}

/// <summary> An SDL audio device instance id (<c> SDL_AudioDeviceID </c>). </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct AudioDeviceID(uint value) : IEquatable<AudioDeviceID>
{
    public readonly uint Value = value;

    /// <summary> SDL_AUDIO_DEVICE_DEFAULT_PLAYBACK: the most reasonable default playback device. </summary>
    public static AudioDeviceID DefaultPlayback => new(0xFFFFFFFFu);

    /// <summary> SDL_AUDIO_DEVICE_DEFAULT_RECORDING: the most reasonable default recording device. </summary>
    public static AudioDeviceID DefaultRecording => new(0xFFFFFFFEu);

    public static AudioDeviceID Null => default;
    public bool IsNull => Value == 0;

    public bool Equals(AudioDeviceID other) => Value == other.Value;
    public override bool Equals(object obj) => obj is AudioDeviceID other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => $"SDL_AudioDeviceID({Value})";

    public static bool operator ==(AudioDeviceID left, AudioDeviceID right) => left.Equals(right);
    public static bool operator !=(AudioDeviceID left, AudioDeviceID right) => !left.Equals(right);

    public static explicit operator uint(AudioDeviceID id) => id.Value;
}
