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
using System.Text;

/// <summary>
/// <para> Keyboard text input event structure (event.text.*) </para>
/// <para>
/// This event will never be delivered unless text input is enabled by calling <see cref="SDL.StartTextInput"/>. Text
/// input is disabled by default!
/// </para>
/// </summary>
/// <since> This struct is available since SDL 3.2.0 </since>
/// <seealso cref="SDL.StartTextInput"/>
/// <seealso cref="SDL.StopTextInput"/>
[StructLayout(LayoutKind.Sequential)]
public struct TextInputEvent
{
    /// <summary>
    /// <see cref="EventType.TextInput"/>
    /// </summary>
    public EventType Type;

    uint _reserved;

    /// <summary> In nanoseconds, populated using <see cref="SDL.GetTicksNS"/> </summary>
    public ulong Timestamp;

    /// <summary> The window with keyboard focus, if any </summary>
    public uint WindowID;

    nint text;

    /// <summary> The input text, UTF-8 encoded </summary>
    public unsafe ReadOnlySpan<byte> Text
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => MemoryMarshal.CreateReadOnlySpanFromNullTerminated((byte*)text);
    }

    /// <summary> The input text length in unicode characters </summary>
    public int TextLength
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Encoding.UTF8.GetCharCount(Text);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<char> GetUnicodeText(Span<char> buffer)
    {
        Encoding.UTF8.GetChars(Text, buffer);
        return buffer;
    }
}