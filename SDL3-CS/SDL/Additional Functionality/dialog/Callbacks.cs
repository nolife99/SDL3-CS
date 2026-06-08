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

// This file is an altered version (storybrew fork): the dialog callback surface was rewritten
// around a fully managed delegate; native thunking lives in PInvoke.cs.

#endregion

namespace SDL3;

/// <code>typedef void (SDLCALL *SDL_DialogFileCallback)(void *userdata, const char * const *filelist, int filter);</code>
/// <summary>
/// <para> Callback used by file dialog functions. </para>
/// <para> Inspect <paramref name="files"/>: </para>
/// <list type="bullet">
/// <item> <see cref="DialogFileList.IsError"/>: an error occurred. Details via <see cref="SDL.GetError"/>. </item>
/// <item> <see cref="DialogFileList.Count"/> of zero: the user didn't choose any file or canceled the dialog. </item>
/// <item> otherwise: the user chose one or more files, readable as UTF-8 spans without any allocation. </item>
/// </list>
/// <para>
/// The view is allocation-free and points at SDL-owned memory that is freed when the callback returns;
/// the <c> ref struct </c> rules prevent it from escaping. Copy exactly what must outlive the callback
/// with <see cref="DialogFileList.GetString"/> or <see cref="DialogFileList.GetChars"/>.
/// </para>
/// <para>
/// <paramref name="filterIndex"/> is the index of the selected filter, or <c> -1 </c> if no filter was
/// selected or the platform doesn't support fetching it.
/// </para>
/// <para>
/// The callback may be invoked on a different thread than the one that opened the dialog. Exceptions it
/// throws never propagate into native SDL: they are routed to <see cref="SDL.UnhandledCallbackException"/>
/// (or the SDL log when no handler is attached).
/// </para>
/// <para>
/// On Android, the paths are <c> content:// </c> URIs. They should be opened using
/// <see cref="SDL.IOFromFile"/> with appropriate modes. This applies both to open and save file dialogs.
/// </para>
/// </summary>
/// <param name="files"> a callback-scoped view of the file(s) chosen by the user. </param>
/// <param name="filterIndex"> index of the selected filter, or <c> -1 </c>. </param>
/// <since> This datatype is available since SDL 3.2.0 </since>
/// <seealso cref="DialogFileFilter"/>
/// <seealso cref="DialogFileList"/>
/// <seealso cref="SDL.ShowOpenFileDialog"/>
/// <seealso cref="SDL.ShowSaveFileDialog"/>
/// <seealso cref="SDL.ShowOpenFolderDialog"/>
/// <seealso cref="SDL.ShowFileDialogWithProperties"/>
public delegate void DialogFileCallback(DialogFileList files, int filterIndex);
