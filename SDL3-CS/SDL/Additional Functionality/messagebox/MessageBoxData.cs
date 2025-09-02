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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

/// <summary> MessageBox structure containing title, text, window, etc. </summary>
public struct MessageBoxData : IDisposable
{
    readonly MessageBoxFlags Flags;
    readonly nint Window;
    readonly int NumButtons;
    readonly byte[]? Title, Message, ColorScheme;
    readonly MessageBoxButtonData[]? Buttons;

    public MessageBoxData(MessageBoxFlags flags,
        nint window,
        scoped ReadOnlySpan<char> title,
        scoped ReadOnlySpan<char> message,
        scoped ReadOnlySpan<MessageBoxButtonData> buttons,
        MessageBoxColorScheme? colorScheme = null)
    {
        Flags = flags;
        Window = window;

        if (!title.IsWhiteSpace())
        {
            var length = Encoding.UTF8.GetByteCount(title);
            Title = ArrayPool<byte>.Shared.Rent(length + 1);
            Encoding.UTF8.GetBytes(title, Title);
            Title[length] = 0;
        }

        if (!message.IsWhiteSpace())
        {
            var length = Encoding.UTF8.GetByteCount(message);
            Message = ArrayPool<byte>.Shared.Rent(length + 1);
            Encoding.UTF8.GetBytes(message, Message);
            Message[length] = 0;
        }

        if (!buttons.IsEmpty)
        {
            Buttons = ArrayPool<MessageBoxButtonData>.Shared.Rent(buttons.Length);
            buttons.CopyTo(Buttons);

            NumButtons = buttons.Length;
        }

        if (colorScheme.HasValue)
        {
            ColorScheme = ArrayPool<byte>.Shared.Rent(Unsafe.SizeOf<MessageBoxColorScheme>());
            Unsafe.WriteUnaligned(ref MemoryMarshal.GetArrayDataReference(ColorScheme),
                Nullable.GetValueRefOrDefaultRef(in colorScheme));
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Pinned
    {
        public MessageBoxFlags flags;
        public nint window, title, message;
        public int numbuttons;
        public nint buttons, colorScheme;
    }

    MessageBoxButtonData.Pinned[]? buttonsPinned;
    GCHandle titlePin, messagePin, buttonsArrayPin, colorSchemePin;

    internal Pinned Pin()
    {
        if (Title is not null) titlePin = GCHandle.Alloc(Title, GCHandleType.Pinned);
        if (Message is not null) messagePin = GCHandle.Alloc(Message, GCHandleType.Pinned);
        if (ColorScheme is not null) colorSchemePin = GCHandle.Alloc(ColorScheme, GCHandleType.Pinned);

        if (NumButtons != 0)
        {
            buttonsPinned = ArrayPool<MessageBoxButtonData.Pinned>.Shared.Rent(NumButtons);
            for (var i = 0; i < NumButtons; i++) buttonsPinned[i] = Buttons[i].Pin();

            buttonsArrayPin = GCHandle.Alloc(buttonsPinned, GCHandleType.Pinned);
        }

        return new()
        {
            flags = Flags,
            window = Window,
            title = Title is null ? 0 : titlePin.AddrOfPinnedObject(),
            message = Message is null ? 0 : messagePin.AddrOfPinnedObject(),
            numbuttons = NumButtons,
            buttons = NumButtons == 0 ? 0 : buttonsArrayPin.AddrOfPinnedObject(),
            colorScheme = ColorScheme is null ? 0 : colorSchemePin.AddrOfPinnedObject()
        };
    }

    internal void Unpin()
    {
        if (titlePin.IsAllocated) titlePin.Free();
        if (messagePin.IsAllocated) messagePin.Free();
        if (colorSchemePin.IsAllocated) colorSchemePin.Free();

        if (buttonsArrayPin.IsAllocated)
        {
            buttonsArrayPin.Free();
            ArrayPool<MessageBoxButtonData.Pinned>.Shared.Return(buttonsPinned);

            for (var i = 0; i < NumButtons; ++i) Buttons[i].Unpin();
        }
    }

    public void Dispose()
    {
        Unpin();

        if (Title is not null) ArrayPool<byte>.Shared.Return(Title);
        if (Message is not null) ArrayPool<byte>.Shared.Return(Message);
        if (ColorScheme is not null) ArrayPool<byte>.Shared.Return(ColorScheme);
        if (Buttons is not null) ArrayPool<MessageBoxButtonData>.Shared.Return(Buttons);
    }
}