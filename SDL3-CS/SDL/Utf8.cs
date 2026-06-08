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

using System;
using System.Buffers;
using System.Text;

/// <summary>
///     (storybrew fork) Encodes a span of UTF-16 chars into a null-terminated UTF-8 byte buffer for passing to a native
///     <c> const char* </c> parameter for the duration of a single call. When the encoded length (including the
///     terminator) fits the caller-provided <c> stackalloc </c> scratch, no allocation occurs; larger strings rent from
///     the shared <see cref="ArrayPool{T}"/> and return the buffer on <see cref="Dispose"/>. Pin <see cref="Bytes"/>
///     with a <c> fixed </c> statement to obtain the pointer.
/// </summary>
/// <example>
///     <code>
///     using var utf8 = new Utf8(path, stackalloc byte[512]);
///     fixed (byte* p = utf8.Bytes) return SDL_CreateDirectory((nint)p);
///     </code>
/// </example>
internal ref struct Utf8
{
    readonly byte[]? rented;

    /// <summary> The null-terminated UTF-8 bytes. Pin with <c> fixed </c> to pass as a native <c> const char* </c>. </summary>
    public readonly Span<byte> Bytes;

    public Utf8(scoped ReadOnlySpan<char> text, Span<byte> scratch)
    {
        var count = Encoding.UTF8.GetByteCount(text) + 1;

        if (count <= scratch.Length)
        {
            Bytes = scratch[..count];
            rented = null;
        }
        else
        {
            rented = ArrayPool<byte>.Shared.Rent(count);
            Bytes = rented.AsSpan(0, count);
        }

        Encoding.UTF8.GetBytes(text, Bytes);
        Bytes[^1] = 0;
    }

    public readonly void Dispose()
    {
        if (rented is not null) ArrayPool<byte>.Shared.Return(rented);
    }
}
