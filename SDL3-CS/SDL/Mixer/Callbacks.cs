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

// This file is part of the storybrew fork: managed callback surface for the SDL3_mixer bindings.
// Native thunking and rooting live in PInvoke.cs. Exceptions thrown by these callbacks never
// propagate into native SDL_mixer; they are routed to SDL.UnhandledCallbackException.

namespace SDL3;

using System;

public static partial class MIX
{
    /// <code>typedef void (SDLCALL *MIX_TrackStoppedCallback)(void *userdata, MIX_Track *track);</code>
    /// <summary>
    /// <para> Fired when a track has finished playback (it does not fire on pause). </para>
    /// <para>
    /// This runs on the mixer's mixing thread - keep the work minimal and never call back into the
    /// mixer from here except where the native documentation permits it.
    /// </para>
    /// </summary>
    /// <param name="track"> the track that stopped. </param>
    /// <since> This datatype is available since SDL_mixer 3.0.0 </since>
    public delegate void TrackStoppedCallback(Track track);

    /// <summary> Context-carrying variant of <see cref="TrackStoppedCallback"/> (no boxing, no closure). </summary>
    public delegate void TrackStoppedCallback<T>(Track track, ref T state);

    /// <code>typedef void (SDLCALL *MIX_TrackMixCallback)(void *userdata, MIX_Track *track, const SDL_AudioSpec *spec, float *pcm, int samples);</code>
    /// <summary>
    /// <para>
    /// Fired as a track's audio passes through the mixing pipeline (raw = before track transformations,
    /// cooked = after). The pcm span aliases the native mix buffer: mutate it in place to alter the
    /// audio. It is only valid during the callback.
    /// </para>
    /// <para> This runs on the mixer's mixing thread. </para>
    /// </summary>
    /// <param name="track"> the track being mixed. </param>
    /// <param name="spec"> format of the pcm data. </param>
    /// <param name="pcm"> the audio samples, mutable in place, valid only during the callback. </param>
    /// <since> This datatype is available since SDL_mixer 3.0.0 </since>
    public delegate void TrackMixCallback(Track track, in SDL.AudioSpec spec, Span<float> pcm);

    /// <summary> Context-carrying variant of <see cref="TrackMixCallback"/> (no boxing, no closure). </summary>
    public delegate void TrackMixCallback<T>(Track track, in SDL.AudioSpec spec, Span<float> pcm, ref T state);

    /// <code>typedef void (SDLCALL *MIX_GroupMixCallback)(void *userdata, MIX_Group *group, const SDL_AudioSpec *spec, float *pcm, int samples);</code>
    /// <summary>
    /// Fired after a group's tracks have been mixed together, before the result lands in the master
    /// mix. Same buffer/lifetime/thread rules as <see cref="TrackMixCallback"/>.
    /// </summary>
    /// <since> This datatype is available since SDL_mixer 3.0.0 </since>
    public delegate void GroupMixCallback(Group group, in SDL.AudioSpec spec, Span<float> pcm);

    /// <summary> Context-carrying variant of <see cref="GroupMixCallback"/> (no boxing, no closure). </summary>
    public delegate void GroupMixCallback<T>(Group group, in SDL.AudioSpec spec, Span<float> pcm, ref T state);

    /// <code>typedef void (SDLCALL *MIX_PostMixCallback)(void *userdata, MIX_Mixer *mixer, const SDL_AudioSpec *spec, float *pcm, int samples);</code>
    /// <summary>
    /// Fired with the final mixed audio just before it is handed to the output device (or
    /// <see cref="Generate(Mixer, Span{byte})"/> caller). Same buffer/lifetime/thread rules as
    /// <see cref="TrackMixCallback"/>.
    /// </summary>
    /// <since> This datatype is available since SDL_mixer 3.0.0 </since>
    public delegate void PostMixCallback(Mixer mixer, in SDL.AudioSpec spec, Span<float> pcm);

    /// <summary> Context-carrying variant of <see cref="PostMixCallback"/> (no boxing, no closure). </summary>
    public delegate void PostMixCallback<T>(Mixer mixer, in SDL.AudioSpec spec, Span<float> pcm, ref T state);
}
