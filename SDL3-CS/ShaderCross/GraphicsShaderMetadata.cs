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

public partial class ShaderCross
{
    [StructLayout(LayoutKind.Sequential)]
    public struct GraphicsShaderMetadata
    {
        /// <summary> The number of samplers defined in the shader. </summary>
        public uint NumSamplers;

        /// <summary> The number of storage textures defined in the shader. </summary>
        public uint NumStorageTextures;

        /// <summary> The number of storage buffers defined in the shader. </summary>
        public uint NumStorageBuffers;

        /// <summary> The number of uniform buffers defined in the shader. </summary>
        public uint NumUniformBuffers;

        /// <summary> The inputs defined in the shader. </summary>
        public IntPtr Inputs;

        /// <summary> The number of outputs defined in the shader. </summary>
        public uint NumOutputs;

        /// <summary> The outputs defined in the shader. </summary>
        public IntPtr Outputs;
    }
}