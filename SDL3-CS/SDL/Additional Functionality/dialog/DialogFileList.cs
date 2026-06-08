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

// This file is an addition of the storybrew fork: allocation-free view over SDL's dialog result.

#endregion

namespace SDL3;

using System;
using System.Runtime.InteropServices;
using System.Text;

/// <summary>
/// <para>
/// An allocation-free view over the file list SDL passes to a dialog callback. It points directly at
/// SDL's UTF-8 buffers, which are freed when the callback returns — being a <c> ref struct </c>, it
/// cannot be stored, boxed, or captured, so the compiler guarantees it never outlives them.
/// </para>
/// <para>
/// Nothing is decoded or copied up front. Read entries as UTF-8 via the indexer, decode into a caller
/// buffer with <see cref="GetChars"/>, or materialize a managed copy with <see cref="GetString"/> for
/// exactly the entries that need to escape the callback (e.g. handed to another thread).
/// </para>
/// </summary>
/// <seealso cref="DialogFileCallback"/>
public readonly unsafe ref struct DialogFileList
{
    readonly byte** files;

    internal DialogFileList(byte** files, int count)
    {
        this.files = files;
        Count = count;
    }

    /// <summary>
    ///     True when the dialog reported an error (details via <see cref="SDL.GetError"/>). Otherwise a
    ///     <see cref="Count"/> of zero means the user canceled or selected nothing.
    /// </summary>
    public bool IsError => files == null;

    /// <summary>Number of selected entries.</summary>
    public int Count { get; }

    /// <summary>The UTF-8 bytes of one path, without the null terminator. Valid only during the callback.</summary>
    public ReadOnlySpan<byte> this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)Count, nameof(index));
            return MemoryMarshal.CreateReadOnlySpanFromNullTerminated(files[index]);
        }
    }

    public int GetCharCount(int index) => Encoding.UTF8.GetCharCount(this[index]);

    public int GetChars(int index, scoped Span<char> destination) => Encoding.UTF8.GetChars(this[index], destination);

    public string GetString(int index) => Encoding.UTF8.GetString(this[index]);
}
