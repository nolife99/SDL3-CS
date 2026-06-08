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

// This file is part of the storybrew fork: strongly typed native handles.

namespace SDL3;

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

/// <summary> Opaque handle to a native <c> SDL_Surface* </c>. </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct SurfaceHandle(nint value) : IEquatable<SurfaceHandle>
{
    public readonly nint Value = value;

    public static SurfaceHandle Null => default;
    public bool IsNull => Value == 0;

    /// <summary>
    ///     View of the surface header (<see cref="Surface"/>: flags, format, size, pitch, pixels).
    ///     The reference is only valid while the surface is alive, and pixel access additionally
    ///     requires the lock rules described by <see cref="SDL.MustLock"/>/<see cref="SDL.LockSurface"/>.
    /// </summary>
    public unsafe ref Surface AsRef() => ref Unsafe.AsRef<Surface>((void*)Value);

    public bool Equals(SurfaceHandle other) => Value == other.Value;
    public override bool Equals(object obj) => obj is SurfaceHandle other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => $"SDL_Surface*(0x{Value:x})";

    public static bool operator ==(SurfaceHandle left, SurfaceHandle right) => left.Equals(right);
    public static bool operator !=(SurfaceHandle left, SurfaceHandle right) => !left.Equals(right);

    public static explicit operator nint(SurfaceHandle handle) => handle.Value;
}
