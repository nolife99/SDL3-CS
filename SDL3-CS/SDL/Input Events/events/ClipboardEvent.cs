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

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
        set => owner = value ? 1 : 0;
    }

    /// <summary> current mime types as an array of UTF8 strings </summary>
    public unsafe MimeTypeEnumerator MimeType
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(new((void*)mimeTypes, NumMimeTypes));
    }

    /// <summary>
    /// <see cref="SDL.EventType.ClipboardUpdate"/>
    /// </summary>
    public SDL.EventType Type;

    uint _reserved;

    /// <summary> In nanoseconds, populated using <see cref="SDL.GetTicksNS"/> </summary>
    public ulong Timestamp;

    int owner, NumMimeTypes;

    IntPtr mimeTypes;

    public ref struct MimeTypeEnumerator
    {
        public readonly Span<nint> MimeTypes;
        int _index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal MimeTypeEnumerator(Span<nint> ptrs)
        {
            MimeTypes = ptrs;
            _index = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MimeTypeEnumerator GetEnumerator() => this;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() => ++_index < MimeTypes.Length;

        public unsafe ReadOnlySpan<byte> Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var p = MimeTypes[_index];
                return new((void*)p, SDL.IndexOfNullByte(p));
            }
        }
    }
}