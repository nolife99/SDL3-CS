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

// This file is an altered version (storybrew fork): the filter is now a plain managed value; UTF-8
// marshaling and native lifetime are handled by the dialog functions, which keep the native copies
// alive until the dialog's callback has run (the previous implementation unpinned them as soon as the
// native call returned, while the asynchronous dialog could still read them).

#endregion

namespace SDL3;

using System;

/// <summary>
/// <para> An entry for filters for file dialogs. </para>
/// <para>
/// A plain managed value: the dialog functions copy it into native memory for exactly as long as SDL
/// needs it, so instances carry no native state and require no disposal.
/// </para>
/// </summary>
/// <since> This struct is available since SDL 3.2.0 </since>
/// <seealso cref="DialogFileCallback"/>
/// <seealso cref="SDL.ShowOpenFileDialog"/>
/// <seealso cref="SDL.ShowSaveFileDialog"/>
/// <seealso cref="SDL.ShowFileDialogWithProperties"/>
public readonly record struct DialogFileFilter
{
    /// <param name="name"> a user-readable label for the filter (for example, "Office document"). </param>
    /// <param name="pattern">
    /// a semicolon-separated list of file extensions (for example, <c> "doc;docx" </c>). File extensions
    /// may only contain alphanumeric characters, hyphens, underscores and periods. Alternatively, the
    /// whole string can be a single asterisk to match anything.
    /// </param>
    public DialogFileFilter(string name, string pattern)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentException.ThrowIfNullOrEmpty(pattern);

        Name = name;
        Pattern = pattern;
    }

    /// <summary>A user-readable label for the filter.</summary>
    public string Name { get; }

    /// <summary>A semicolon-separated list of file extensions, or <c> "*" </c> to match anything.</summary>
    public string Pattern { get; }
}
