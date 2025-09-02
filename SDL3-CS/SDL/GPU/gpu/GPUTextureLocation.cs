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
    ///     <para> A structure specifying a location in a texture. </para>
    ///     <para> Used when copying data from one texture to another. </para>
    /// </summary>
    /// <since> This struct is available since SDL 3.2.0 </since>
    /// <seealso cref="CopyGPUTextureToTexture"/>
    [StructLayout(LayoutKind.Sequential)]
    public struct GPUTextureLocation
    {
        /// <summary> The texture used in the copy operation. </summary>
        public IntPtr Texture;

        /// <summary> The mip level index of the location. </summary>
        public uint MipLevel;

        /// <summary> The layer index of the location. </summary>
        public uint Layer;

        /// <summary> The left offset of the location. </summary>
        public uint X;

        /// <summary> The top offset of the location. </summary>
        public uint Y;

        /// <summary> The front offset of the location. </summary>
        public uint Z;
    }
}