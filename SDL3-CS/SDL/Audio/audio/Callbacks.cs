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

// This file is an altered version (storybrew fork): audio callbacks are fully managed delegates
// (no userdata parameter - capture state by closure or use the <T> context overloads); native
// thunking and rooting live in PInvoke.cs. Exceptions never propagate into native SDL; they are
// routed to SDL.UnhandledCallbackException.

namespace SDL3;

using System;

public static partial class SDL
{
    /// <code>typedef void (SDLCALL *SDL_AudioPostmixCallback)(void *userdata, const SDL_AudioSpec *spec, float *buffer, int buflen);</code>
    /// <summary>
    ///     <para> A callback that fires when data is about to be fed to an audio device. </para>
    ///     <para>
    ///         This is useful for accessing the final mix, perhaps for writing a visualizer or applying a final effect to the
    ///         audio data before playback.
    ///     </para>
    ///     <para>
    ///         The buffer span aliases the native mix buffer (always SDL_AUDIO_F32 samples): inspect or modify it in place. It
    ///         is only valid during the callback. Channel count and sample rate in <c> spec </c> can change between calls.
    ///     </para>
    ///     <para>
    ///         This callback should run as quickly as possible and not block for any significant time, as this callback delays
    ///         submission of data to the audio device, which can cause audio playback problems. It runs from a background
    ///         thread owned by SDL; the application is responsible for locking resources the callback touches.
    ///     </para>
    /// </summary>
    /// <param name="spec"> the current format of audio that is to be submitted to the audio device. </param>
    /// <param name="buffer"> the audio samples to be submitted, mutable in place, valid only during the callback. </param>
    /// <since> This datatype is available since SDL 3.2.0 </since>
    /// <seealso cref="SetAudioPostmixCallback(uint, AudioPostmixCallback?)"/>
    public delegate void AudioPostmixCallback(in AudioSpec spec, Span<float> buffer);

    /// <summary> Context-carrying variant of <see cref="AudioPostmixCallback"/> (no boxing, no closure). </summary>
    public delegate void AudioPostmixCallback<T>(in AudioSpec spec, Span<float> buffer, ref T state);

    /// <code>typedef void (SDLCALL *SDL_AudioStreamCallback)(void *userdata, SDL_AudioStream *stream, int additional_amount, int total_amount);</code>
    /// <summary>
    ///     <para> A callback that fires when data passes through an SDL_AudioStream. </para>
    ///     <para>
    ///         Two values are offered here: one is the amount of additional data needed to satisfy the immediate request (which
    ///         might be zero if the stream already has enough data queued) and the other is the total amount being requested. In a
    ///         Get call triggering a Put callback, these values can be different. In a Put call triggering a Get callback, these
    ///         values are always the same.
    ///     </para>
    ///     <para> Byte counts might be slightly overestimated due to buffering or resampling, and may change from call to call. </para>
    ///     <para>
    ///         This callback may run from any thread. The stream's lock is held while it runs, so the callback itself does not
    ///         need to manage it.
    ///     </para>
    /// </summary>
    /// <param name="stream"> the SDL audio stream associated with this callback. </param>
    /// <param name="additionalAmount"> the amount of data, in bytes, that is needed right now. </param>
    /// <param name="totalAmount"> the total amount of data requested, in bytes, that is requested or available. </param>
    /// <since> This datatype is available since SDL 3.2.0 </since>
    /// <seealso cref="SetAudioStreamGetCallback(AudioStreamHandle, AudioStreamCallback?)"/>
    /// <seealso cref="SetAudioStreamPutCallback(AudioStreamHandle, AudioStreamCallback?)"/>
    public delegate void AudioStreamCallback(AudioStreamHandle stream, int additionalAmount, int totalAmount);

    /// <summary> Context-carrying variant of <see cref="AudioStreamCallback"/> (no boxing, no closure). </summary>
    public delegate void AudioStreamCallback<T>(AudioStreamHandle stream, int additionalAmount, int totalAmount, ref T state);

    /// <code>typedef void (SDLCALL *SDL_AudioStreamDataCompleteCallback)(void *userdata, const void *buf, int buflen);</code>
    /// <summary>
    ///     <para> A callback that fires once for completed <see cref="PutAudioStreamDataNoCopy(AudioStreamHandle, nint, int, AudioStreamDataCompleteCallback?)"/> data. </para>
    ///     <para>
    ///         It receives the exact pointer and length originally handed to the stream, at which point the stream will not
    ///         access the memory again: free, unpin, or reuse it here. It fires for any reason the data is no longer needed,
    ///         including clearing or destroying the stream, and may run from any thread.
    ///     </para>
    /// </summary>
    /// <param name="buffer"> the pointer originally provided to the put call. </param>
    /// <param name="length"> the size of <paramref name="buffer"/> in bytes. </param>
    /// <since> This datatype is available since SDL 3.4.0. </since>
    public delegate void AudioStreamDataCompleteCallback(nint buffer, int length);

    /// <summary> Context-carrying variant of <see cref="AudioStreamDataCompleteCallback"/> (no boxing, no closure). </summary>
    public delegate void AudioStreamDataCompleteCallback<T>(nint buffer, int length, ref T state);
}
