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
 */

// This file is an altered version (storybrew fork): updated to the rewritten managed dialog API.
#endregion

using SDL3;

namespace File_Dialog;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        if (!SDL.Init(SDL.InitFlags.Video))
        {
            SDL.LogError(LogCategory.System, $"SDL could not initialize: {SDL.GetError()}");
            return;
        }

        if (!SDL.CreateWindowAndRenderer("SDL3 File Dialog", 800, 600, 0, out var window, out var renderer))
        {
            SDL.LogError(LogCategory.Application, $"Error creating window and rendering: {SDL.GetError()}");
            return;
        }

        SDL.SetRenderVSync(renderer, 1);

        // Filters are plain managed values; the dialog functions copy them into native memory for
        // exactly as long as SDL needs them. Nothing to pin, keep alive, or dispose.
        var openFileFilters = new DialogFileFilter[]
        {
            new("All files", "*"),
            new("Image files", "jpg;jpeg;png;bmp;gif;webp"),
        };

        var saveFileFilters = new DialogFileFilter[]
        {
            new("SDL File", "sdl")
        };

        SDL.SetRenderDrawColor(renderer, 100, 149, 237, 0);

        var loop = true;

        while (loop)
        {
            while (SDL.PollEvent(out var e))
            {
                if (e.Type == (uint)SDL.EventType.Quit)
                {
                    loop = false;
                }

                if (e.Type == (uint)SDL.EventType.KeyDown && e.Key.Key == SDL.Keycode.Alpha1)
                {
                    SDL.ShowOpenFileDialog(OnDialogResult, window, openFileFilters, allowMany: true);
                }

                if (e.Type == (uint)SDL.EventType.KeyDown && e.Key.Key == SDL.Keycode.Alpha2)
                {
                    SDL.ShowSaveFileDialog(OnDialogResult, window, saveFileFilters, "test");
                }

                if (e.Type == (uint)SDL.EventType.KeyDown && e.Key.Key == SDL.Keycode.Alpha3)
                {
                    SDL.ShowOpenFolderDialog(OnDialogResult, window);
                }
            }

            SDL.RenderClear(renderer);
            SDL.RenderPresent(renderer);
        }

        SDL.DestroyRenderer(renderer);
        SDL.DestroyWindow(window);

        SDL.Quit();
    }

    // The view is allocation-free and only valid inside this callback (the compiler enforces it).
    // Call files.GetString(i) / files.GetChars(i, buffer) for any entry that must outlive it.
    // Exceptions thrown here are caught at the native boundary and routed to
    // SDL.UnhandledCallbackException (or the SDL log).
    private static void OnDialogResult(DialogFileList files, int filter)
    {
        if (files.IsError)
        {
            SDL.LogError(LogCategory.Application, $"SDL Error: {SDL.GetError()}");
            return;
        }

        var type = filter switch
        {
            0 => "file",
            1 => "image",
            _ => "unknown"
        };

        switch (files.Count)
        {
            case 0:
                SDL.LogInfo(LogCategory.Application, "File not selected");
                break;

            case 1:
                SDL.LogInfo(LogCategory.Application, $"Selected filter: {filter}, Selected {type}: {files.GetString(0)}");
                break;

            default:
                SDL.LogInfo(LogCategory.Application, $"Selected filter: {filter}, Selected {type}s:");
                for (var i = 0; i < files.Count; i++) Console.WriteLine($"[{i}] {files.GetString(i)}");
                break;
        }
    }
}
