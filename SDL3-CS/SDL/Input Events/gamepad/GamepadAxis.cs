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

/// <summary>
/// <para> The list of axes available on a gamepad </para>
/// <para>
/// Thumbstick axis values range from <see cref="SDL.JoystickAxisMin"/> to <see cref="SDL.JoystickAxisMax"/>, and are
/// centered within ~8000 of zero, though advanced UI will allow users to set or autodetect the dead zone, which varies between
/// gamepads.
/// </para>
/// <para>
/// Trigger axis values range from 0 (released) to <see cref="SDL.JoystickAxisMax"/> (fully pressed) when reported by
/// <see cref="SDL.GetGamepadAxis"/>. Note that this is not the same range that will be reported by the lower-level
/// <see cref="SDL.GetJoystickAxis"/>.
/// </para>
/// </summary>
/// <since> This enum is available since SDL 3.2.0 </since>
public enum GamepadAxis
{
    Invalid = -1, LeftX, LeftY, RightX, RightY, LeftTrigger, RightTrigger, Count
}