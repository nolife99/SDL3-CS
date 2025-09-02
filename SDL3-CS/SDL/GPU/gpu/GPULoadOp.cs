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

public static partial class SDL
{
    /// <summary> Specifies how the contents of a texture attached to a render pass are treated at the beginning of the render pass. </summary>
    /// <since> This enum is available since SDL 3.2.0 </since>
    /// <seealso cref="BeginGPURenderPass(nint, nint, uint, nint)"/>
    public enum GPULoadOp
    {
        /// <summary> The previous contents of the texture will be preserved. </summary>
        Load,

        /// <summary> The contents of the texture will be cleared to a color. </summary>
        Clear,

        /// <summary> The previous contents of the texture need not be preserved. The contents will be undefined. </summary>
        DontCare
    }
}