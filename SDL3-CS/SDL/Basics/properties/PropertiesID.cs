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

// This file is part of the storybrew fork: a strongly typed wrapper for SDL_PropertiesID plus the
// typed-name layer used to restrict writes and read results safely (see PropertyAccess.cs).

namespace SDL3;

using System;
using System.Runtime.InteropServices;

/// <summary>
///     An SDL property group id (<c> SDL_PropertiesID </c>). Distinct from the raw <see cref="uint"/>
///     so a property group can't be mistaken for an audio device id, display id, or any other 32-bit
///     SDL handle.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct PropertiesID(uint value) : IEquatable<PropertiesID>
{
    public readonly uint Value = value;

    /// <summary> The invalid/empty property group (id 0). </summary>
    public static PropertiesID Null => default;

    public bool IsNull => Value == 0;

    public bool Equals(PropertiesID other) => Value == other.Value;
    public override bool Equals(object obj) => obj is PropertiesID other && Equals(other);
    public override int GetHashCode() => (int)Value;
    public override string ToString() => $"SDL_PropertiesID({Value})";

    public static bool operator ==(PropertiesID left, PropertiesID right) => left.Equals(right);
    public static bool operator !=(PropertiesID left, PropertiesID right) => !left.Equals(right);

    public static explicit operator uint(PropertiesID id) => id.Value;
}

/// <summary>
///     A typed property name: the SDL string key paired, at the type level, with the value type it
///     stores. Pass these to <see cref="SDL.SetProperty(PropertiesID, PropertyKey{nint}, nint)"/> /
///     <see cref="SDL.GetProperty(PropertiesID, PropertyKey{string}, string)"/> overloads — the key's
///     <typeparamref name="T"/> selects the correct underlying SDL call, so a string key cannot be set
///     with a number and vice-versa. Only the value types SDL actually supports (<see cref="nint"/>,
///     <see cref="string"/>, <see cref="long"/>, <see cref="float"/>, <see cref="bool"/>) have
///     accessor overloads, so an ill-typed key simply won't compile against Set/Get.
///     <para>
///         Restriction pattern: expose, per operation, only the keys that operation accepts (see
///         <c> MIX.AudioLoadProperties </c> / <c> MIX.AudioMetadata </c>). Callers then physically
///         cannot queue an irrelevant property.
///     </para>
/// </summary>
public readonly record struct PropertyKey<T>(string Name)
{
    public override string ToString() => Name;

    public static implicit operator PropertyKey<T>(string name) => new(name);
}
