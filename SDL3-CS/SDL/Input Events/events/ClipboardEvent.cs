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

using System.Buffers;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

/// <summary> An event triggered when the clipboard contents have changed (event.clipboard.*) </summary>
/// <since> This struct is available since SDL 3.2.0 </since>
[StructLayout(LayoutKind.Sequential)]
public struct ClipboardEvent
{
    /// <summary> are we owning the clipboard (internal update) </summary>
    public bool Owner
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => owner != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => owner = value ? (byte)1 : (byte)0;
    }

    /// <summary> current mime types as an array of UTF8 strings </summary>
    public unsafe MimeTypeEnumerator MimeTypes
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(new((void*)mimeTypes, NumMimeTypes));
    }

    /// <summary>
    /// <see cref="EventType.ClipboardUpdate"/>
    /// </summary>
    public EventType Type;

    uint _reserved;

    /// <summary> In nanoseconds, populated using <see cref="SDL.GetTicksNS"/> </summary>
    public ulong Timestamp;

    byte owner;
    int NumMimeTypes;

    nint mimeTypes;

    public ref struct MimeTypeEnumerator : IEnumerator<ArraySegment<char>>
    {
        public readonly Span<nint> MimeTypes;
        int _index, currentStrLength;
        char[]? currentUnicode;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal MimeTypeEnumerator(Span<nint> ptrs)
        {
            MimeTypes = ptrs;
            _index = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MimeTypeEnumerator GetEnumerator() => this;

        public unsafe bool MoveNext()
        {
            if (++_index < MimeTypes.Length)
            {
                if (currentUnicode is not null) ArrayPool<char>.Shared.Return(currentUnicode);

                var utf8Text = MemoryMarshal.CreateReadOnlySpanFromNullTerminated((byte*)MimeTypes[_index]);
                currentUnicode = ArrayPool<char>.Shared.Rent(currentStrLength = Encoding.UTF8.GetCharCount(utf8Text));

                Encoding.UTF8.GetChars(utf8Text, currentUnicode);

                return true;
            }

            if (currentUnicode is null) return false;

            ArrayPool<char>.Shared.Return(currentUnicode);
            currentUnicode = null;
            return false;
        }

        public void Reset()
        {
            _index = -1;
            if (currentUnicode is null) return;

            ArrayPool<char>.Shared.Return(currentUnicode);
            currentUnicode = null;
        }

        object? IEnumerator.Current => currentUnicode;

        /// <inheritdoc/>
        public ArraySegment<char> Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(currentUnicode, 0, currentStrLength);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (currentUnicode is not null) ArrayPool<char>.Shared.Return(currentUnicode);
        }
    }
}