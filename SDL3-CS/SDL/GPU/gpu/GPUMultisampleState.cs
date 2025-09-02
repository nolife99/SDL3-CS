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
    /// <summary> A structure specifying the parameters of the graphics pipeline multisample state. </summary>
    /// <since> This struct is available since SDL 3.2.0 </since>
    /// <seealso cref="GPUGraphicsPipelineCreateInfo"/>
    [StructLayout(LayoutKind.Sequential)]
    public struct GPUMultisampleState
    {
        /// <summary> The number of samples to be used in rasterization. </summary>
        public GPUSampleCount SampleCount;

        /// <summary> eserved for future use. Must be set to 0. </summary>
        public uint SampleMask;

        /// <summary> Reserved for future use. Must be set to false. </summary>
        public byte EnableMask;

        /// <summary> true enables the alpha-to-coverage feature. </summary>
        byte EnableAlphaToCoverage;

        byte _padding2;

        byte _padding3;
    }
}