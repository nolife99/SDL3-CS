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
    ///     <para> A structure containing a template for the <see cref="HAPTIC_CUSTOM"/> effect. </para>
    ///     <para> This struct is exclusively for the <see cref="HAPTIC_CUSTOM"/> effect. </para>
    ///     <para>
    ///         A custom force feedback effect is much like a periodic effect, where the application can define its exact shape.
    ///         You will have to allocate the data yourself. Data should consist of channels * samples Uint16 samples.
    ///     </para>
    ///     <para>
    ///         If channels is one, the effect is rotated using the defined direction. Otherwise it uses the samples in data for
    ///         the different axes.
    ///     </para>
    /// </summary>
    /// <since> This struct is available since SDL 3.2.0 </since>
    /// <seealso cref="HAPTIC_CUSTOM"/>
    /// <seealso cref="HapticEffect"/>
    [StructLayout(LayoutKind.Sequential)]
    public struct HapticCustom
    {
        /// <summary>
        ///     <see cref="HAPTIC_CUSTOM"/>
        /// </summary>
        public UInt16 Type;

        /// <summary> Direction of the effect. </summary>
        public HapticDirection Direction;

        /// <summary> Duration of the effect. </summary>
        public int Length;

        /// <summary> Delay before starting the effect. </summary>
        public UInt16 Delay;

        /// <summary> Button that triggers the effect. </summary>
        public UInt16 Button;

        /// <summary> How soon it can be triggered again after button. </summary>
        public UInt16 Interval;

        /// <summary> Axes to use, minimum of one. </summary>
        public byte Channels;

        /// <summary> Sample periods. </summary>
        public UInt16 Period;

        /// <summary> Amount of samples. </summary>
        public UInt16 Samples;

        /// <summary> Should contain channels*samples items. </summary>
        public ushort[] Data;

        /// <summary> Duration of the attack. </summary>
        public UInt16 AttackLength;

        /// <summary> Level at the start of the attack. </summary>
        public UInt16 AttackLevel;

        /// <summary> Duration of the fade. </summary>
        public UInt16 FadeLength;

        /// <summary> Level at the end of the fade. </summary>
        public UInt16 FadeLevel;
    }
}