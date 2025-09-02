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

/// <summary> The structure that defines a display mode. </summary>
/// <since> This struct is available since SDL 3.2.0 </since>
/// <seealso cref="SDL.GetFullscreenDisplayModes"/>
/// <seealso cref="SDL.GetDesktopDisplayMode(uint)"/>
/// <seealso cref="SDL.GetCurrentDisplayMode(uint)"/>
/// <seealso cref="SDL.SetWindowFullscreenMode(nint, nint)"/>
/// <seealso cref="SDL.GetWindowFullscreenMode(nint)"/>
[StructLayout(LayoutKind.Sequential)]
public struct DisplayMode
{
    /// <summary> The display this mode is associated with </summary>
    public uint DisplayID;

    /// <summary> Pixel format </summary>
    public SDL.PixelFormat Format;

    /// <summary> Width </summary>
    public int W;

    /// <summary> Height </summary>
    public int H;

    /// <summary> Scale converting size to pixels (e.g. a 1920x1080 mode with 2.0 scale would have 3840x2160 pixels) </summary>
    public float PixelDensity;

    /// <summary> Refresh rate (or 0.0f for unspecified) </summary>
    public float RefreshRate;

    /// <summary> Precise refresh rate numerator (or 0 for unspecified) </summary>
    public int RefreshRateNumerator;

    /// <summary> Precise refresh rate denominator </summary>
    public int RefreshRateDenominator;

    /// <summary> Private </summary>
    IntPtr _internal;
}