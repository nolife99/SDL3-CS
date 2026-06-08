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

// This file is part of the storybrew fork: typed accessors over the stringly-typed property API.
// Each overload binds a PropertyKey<T> to the matching native Set*/Get* call, so the value type is
// checked at compile time and callers never pick the wrong setter for a key.

namespace SDL3;

using System.Runtime.CompilerServices;

public static partial class SDL
{
    /// <summary> Set a pointer property by typed key (pass <c> handle.Value </c> for typed handles). </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool SetProperty(PropertiesID props, PropertyKey<nint> key, nint value)
        => SetPointerProperty(props, key.Name, value);

    /// <summary> Set a string property by typed key (a null value deletes it). </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool SetProperty(PropertiesID props, PropertyKey<string> key, string? value)
        => SetStringProperty(props, key.Name, value);

    /// <summary> Set an integer property by typed key. </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool SetProperty(PropertiesID props, PropertyKey<long> key, long value)
        => SetNumberProperty(props, key.Name, value);

    /// <summary> Set a floating-point property by typed key. </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool SetProperty(PropertiesID props, PropertyKey<float> key, float value)
        => SetFloatProperty(props, key.Name, value);

    /// <summary> Set a boolean property by typed key. </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool SetProperty(PropertiesID props, PropertyKey<bool> key, bool value)
        => SetBooleanProperty(props, key.Name, value);

    /// <summary> Read a pointer property by typed key (default 0). </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nint GetProperty(PropertiesID props, PropertyKey<nint> key, nint defaultValue = 0)
        => GetPointerProperty(props, key.Name, defaultValue);

    /// <summary> Read a string property by typed key (default empty string). </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetProperty(PropertiesID props, PropertyKey<string> key, string defaultValue = "")
        => GetStringProperty(props, key.Name, defaultValue);

    /// <summary> Read an integer property by typed key. </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long GetProperty(PropertiesID props, PropertyKey<long> key, long defaultValue = 0)
        => GetNumberProperty(props, key.Name, defaultValue);

    /// <summary> Read a floating-point property by typed key. </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float GetProperty(PropertiesID props, PropertyKey<float> key, float defaultValue = 0)
        => GetFloatProperty(props, key.Name, defaultValue);

    /// <summary> Read a boolean property by typed key. </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GetProperty(PropertiesID props, PropertyKey<bool> key, bool defaultValue = false)
        => GetBooleanProperty(props, key.Name, defaultValue);

    /// <summary> Whether a property exists, addressed by typed key. </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasProperty<T>(PropertiesID props, PropertyKey<T> key) => HasProperty(props, key.Name);

    /// <summary> Clear a property, addressed by typed key. </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ClearProperty<T>(PropertiesID props, PropertyKey<T> key) => ClearProperty(props, key.Name);
}
