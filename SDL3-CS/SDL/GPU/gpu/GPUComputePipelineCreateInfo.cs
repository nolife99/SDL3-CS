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
    /// <summary> A structure specifying the parameters of a compute pipeline state. </summary>
    /// <since> This struct is available since SDL 3.2.0 </since>
    /// <seealso cref="CreateGPUComputePipeline"/>
    /// <seealso cref="GPUShaderFormat"/>
    [StructLayout(LayoutKind.Sequential)]
    public struct GPUComputePipelineCreateInfo
    {
        /// <summary> The size in bytes of the compute shader code pointed to. </summary>
        public UIntPtr CodeSize;

        /// <summary> A pointer to compute shader code. </summary>
        public IntPtr Code;

        /// <summary> A pointer to a null-terminated UTF-8 string specifying the entry point function name for the shader. </summary>
        public IntPtr Entrypoint;

        /// <summary> The format of the compute shader code. </summary>
        public GPUShaderFormat Format;

        /// <summary> The number of samplers defined in the shader. </summary>
        public uint NumSamplers;

        /// <summary> The number of readonly storage textures defined in the shader. </summary>
        public uint NumReadonlyStorageTextures;

        /// <summary> The number of readonly storage buffers defined in the shader. </summary>
        public uint NumReadonlyStorageBuffers;

        /// <summary> The number of read-write storage textures defined in the shader. </summary>
        public uint NumReadwriteStorageTextures;

        /// <summary> The number of read-write storage buffers defined in the shader. </summary>
        public uint NumReadwriteStorageBuffers;

        /// <summary> The number of uniform buffers defined in the shader. </summary>
        public uint NumUniformBuffers;

        /// <summary> The number of threads in the X dimension. This should match the value in the shader. </summary>
        public uint ThreadcountX;

        /// <summary> The number of threads in the Y dimension. This should match the value in the shader. </summary>
        public uint ThreadcountY;

        /// <summary> The number of threads in the Z dimension. This should match the value in the shader. </summary>
        public uint ThreadcountZ;

        /// <summary> A properties ID for extensions. Should be 0 if no extensions are needed. </summary>
        public uint Props;
    }
}