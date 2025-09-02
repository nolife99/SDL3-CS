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

/// <summary>
///     <para> An entry for filters for file dialogs. </para>
/// </summary>
/// <since> This struct is available since SDL 3.2.0 </since>
/// <seealso cref="DialogFileCallback"/>
/// <seealso cref="SDL.ShowOpenFileDialog"/>
/// <seealso cref="SDL.ShowSaveFileDialog"/>
/// <seealso cref="SDL.ShowOpenFolderDialog"/>
/// <seealso cref="SDL.ShowFileDialogWithProperties"/>
public struct DialogFileFilter : IDisposable
{
    readonly byte[] name, pattern;
    GCHandle namePin, patternPin;

    /// <param name="name"> is a user-readable label for the filter (for example, "Office document"). </param>
    /// <param name="pattern">
    ///     is a semicolon-separated list of file extensions (for example, <c> "doc;docx" </c>). File extensions
    ///     may only contain alphanumeric characters, hyphens, underscores and periods. Alternatively, the whole string can be a
    ///     single asterisk (<c> "*" </c>), which serves as an "<c> All files </c>" filter.
    /// </param>
    public DialogFileFilter(scoped ReadOnlySpan<char> name, scoped ReadOnlySpan<char> pattern)
    {
        if (!name.IsWhiteSpace())
        {
            var length = Encoding.UTF8.GetByteCount(name);
            this.name = ArrayPool<byte>.Shared.Rent(length + 1);
            Encoding.UTF8.GetBytes(name, this.name);
            this.name[length] = 0;
        }

        if (!pattern.IsWhiteSpace())
        {
            var length = Encoding.UTF8.GetByteCount(pattern);
            this.pattern = ArrayPool<byte>.Shared.Rent(length + 1);
            Encoding.UTF8.GetBytes(pattern, this.pattern);
            this.pattern[length] = 0;
        }
    }

    internal (nint, nint) Pin()
    {
        if (name is not null) namePin = GCHandle.Alloc(name, GCHandleType.Pinned);
        if (pattern is not null) patternPin = GCHandle.Alloc(pattern, GCHandleType.Pinned);

        return (name is null ? 0 : namePin.AddrOfPinnedObject(), pattern is null ? 0 : patternPin.AddrOfPinnedObject());
    }

    internal void Unpin()
    {
        if (namePin.IsAllocated) namePin.Free();
        if (patternPin.IsAllocated) patternPin.Free();
    }

    public void Dispose()
    {
        Unpin();

        if (name is not null) ArrayPool<byte>.Shared.Return(name);
        if (pattern is not null) ArrayPool<byte>.Shared.Return(pattern);
    }
}