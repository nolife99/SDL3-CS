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

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public partial class Image
{
    /// <summary> Animated image support Currently only animated GIFs are supported. </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Animation
    {
        /// <summary> The width of the frames </summary>
        public int W;

        /// <summary> The height of the frames </summary>
        public int H;

        /// <summary> The number of frames </summary>
        public int Count;

        /// <summary> An array of frames </summary>
        public IntPtr Frames;

        /// <summary> An array of frame delays, in milliseconds </summary>
        public IntPtr Delays;
    }

    /// <summary>
    ///     Opaque handle to a native <c> IMG_Animation* </c>. Unlike SDL's forward-declared handles,
    ///     <see cref="Animation"/> is a public, layout-defined struct, so this handle exposes
    ///     <see cref="AsRef"/> for direct (read-only-intent) field access — the same convenience
    ///     <see cref="SurfaceHandle"/> provides.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct AnimationHandle(nint value) : IEquatable<AnimationHandle>
    {
        public readonly nint Value = value;

        public static AnimationHandle Null => default;
        public bool IsNull => Value == 0;

        /// <summary> View of the animation header. Valid only while the animation is alive (before FreeAnimation). </summary>
        public unsafe ref Animation AsRef() => ref Unsafe.AsRef<Animation>((void*)Value);

        public bool Equals(AnimationHandle other) => Value == other.Value;
        public override bool Equals(object obj) => obj is AnimationHandle other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => $"IMG_Animation*(0x{Value:x})";

        public static bool operator ==(AnimationHandle left, AnimationHandle right) => left.Equals(right);
        public static bool operator !=(AnimationHandle left, AnimationHandle right) => !left.Equals(right);

        public static explicit operator nint(AnimationHandle handle) => handle.Value;
    }
}