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
using System.Runtime.InteropServices;
using System.Text;

/// <summary> Individual button data. </summary>
/// <since> This struct is available since SDL 3.2.0 </since>
public struct MessageBoxButtonData : IDisposable
{
    readonly MessageBoxButtonFlags Flags;
    readonly int ButtonID;
    readonly byte[]? Text;

    public MessageBoxButtonData(MessageBoxButtonFlags flags, int buttonId, scoped ReadOnlySpan<char> text)
    {
        Flags = flags;
        ButtonID = buttonId;

        if (text.IsWhiteSpace()) return;

        var length = Encoding.UTF8.GetByteCount(text);
        Text = ArrayPool<byte>.Shared.Rent(length + 1);
        Encoding.UTF8.GetBytes(text, Text);
        Text[length] = 0;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Pinned
    {
        public MessageBoxButtonFlags Flags;
        public int ButtonID;
        public nint Text;
    }

    GCHandle textPin;

    internal Pinned Pin()
    {
        textPin = GCHandle.Alloc(Text, GCHandleType.Pinned);
        return new() { Flags = Flags, ButtonID = ButtonID, Text = textPin.AddrOfPinnedObject() };
    }

    internal void Unpin()
    {
        if (textPin.IsAllocated) textPin.Free();
    }

    public void Dispose()
    {
        Unpin();

        if (Text is not null) ArrayPool<byte>.Shared.Return(Text);
    }
}