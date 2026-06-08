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

// This file is part of the storybrew fork: SDL3_mixer (MIX_*) bindings against release-3.2.4.
//
// Callback rooting model: the userdata passed to native SDL_mixer is a GCHandle to an invoker
// object, so thunks never consult a registry on the (hot) mixing thread. The per-handle
// dictionaries below exist only to free the previous GCHandle when a callback is replaced,
// removed, or its owner (track/group/mixer) is destroyed through these wrappers.

namespace SDL3;

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

public static partial class MIX
{
    #region Library, init

    /// <code>extern SDL_DECLSPEC int SDLCALL MIX_Version(void);</code>
    /// <summary> Get the version of SDL_mixer that is linked against your program. </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_Version"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial int Version();

    /// <code>extern SDL_DECLSPEC bool SDLCALL MIX_Init(void);</code>
    /// <summary> Initialize the SDL_mixer library. Safe to call multiple times (refcounted). </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_Init"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool Init();

    /// <code>extern SDL_DECLSPEC void SDLCALL MIX_Quit(void);</code>
    /// <summary> Deinitialize the SDL_mixer library (one call per successful <see cref="Init"/>). </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_Quit"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void Quit();

    /// <code>extern SDL_DECLSPEC int SDLCALL MIX_GetNumAudioDecoders(void);</code>
    /// <summary> Report the number of audio decoders available for use. </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_GetNumAudioDecoders"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial int GetNumAudioDecoders();

    [LibraryImport(MixerLibrary, EntryPoint = "MIX_GetAudioDecoder"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial nint SDL_GetAudioDecoder(int index);

    /// <code>extern SDL_DECLSPEC const char * SDLCALL MIX_GetAudioDecoder(int index);</code>
    /// <summary>
    /// Report the name of a specific audio decoder ("WAV", "MP3", etc.) as a zero-copy UTF-8 view of
    /// static native memory. Call <c> Encoding.UTF8.GetString </c> on it for managed text.
    /// </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    public static unsafe ReadOnlySpan<byte> GetAudioDecoder(int index)
    {
        var ptr = SDL_GetAudioDecoder(index);
        return ptr == 0 ? default : MemoryMarshal.CreateReadOnlySpanFromNullTerminated((byte*)ptr);
    }

    #endregion

    #region Mixer lifecycle

    [LibraryImport(MixerLibrary, EntryPoint = "MIX_CreateMixerDevice"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial Mixer SDL_CreateMixerDevice(AudioDeviceID devid, SDL.AudioSpec* spec);

    /// <code>extern SDL_DECLSPEC MIX_Mixer * SDLCALL MIX_CreateMixerDevice(SDL_AudioDeviceID devid, const SDL_AudioSpec *spec);</code>
    /// <summary>
    /// Create a mixer that plays sound directly to an audio device (<paramref name="devid"/> may be
    /// <see cref="SDL.AudioDeviceDefaultPlayback"/>). This overload lets SDL_mixer pick the format.
    /// </summary>
    /// <returns> a mixer handle, or 0 on failure; call <see cref="SDL.GetError"/> for more information. </returns>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    public static unsafe Mixer CreateMixerDevice(AudioDeviceID devid) => SDL_CreateMixerDevice(devid, null);

    /// <inheritdoc cref="CreateMixerDevice(AudioDeviceID)"/>
    /// <param name="devid"> the audio device to play to. </param>
    /// <param name="spec"> the format the mixer should process audio in. </param>
    public static unsafe Mixer CreateMixerDevice(AudioDeviceID devid, in SDL.AudioSpec spec)
    {
        fixed (SDL.AudioSpec* specPtr = &spec) return SDL_CreateMixerDevice(devid, specPtr);
    }

    /// <code>extern SDL_DECLSPEC MIX_Mixer * SDLCALL MIX_CreateMixer(const SDL_AudioSpec *spec);</code>
    /// <summary>
    /// Create a mixer that generates audio to a memory buffer on demand via <see cref="Generate(Mixer, Span{byte})"/>,
    /// without a device.
    /// </summary>
    /// <returns> a mixer handle, or 0 on failure; call <see cref="SDL.GetError"/> for more information. </returns>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_CreateMixer"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial Mixer CreateMixer(in SDL.AudioSpec spec);

    [LibraryImport(MixerLibrary, EntryPoint = "MIX_DestroyMixer"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void SDL_DestroyMixer(Mixer mixer);

    /// <code>extern SDL_DECLSPEC void SDLCALL MIX_DestroyMixer(MIX_Mixer *mixer);</code>
    /// <summary>
    /// Destroy a mixer and release its post-mix callback rooting. Destroy tracks and groups created
    /// on this mixer through <see cref="DestroyTrack"/>/<see cref="DestroyGroup"/> first so their
    /// callback rootings are released too.
    /// </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    public static void DestroyMixer(Mixer mixer)
    {
        SDL_DestroyMixer(mixer);
        ReleaseCallback(mixerPostMixCallbacks, mixer.Value);
    }

    /// <code>extern SDL_DECLSPEC SDL_PropertiesID SDLCALL MIX_GetMixerProperties(MIX_Mixer *mixer);</code>
    /// <summary> Get the properties associated with a mixer (see <see cref="Props"/>). </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_GetMixerProperties"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial PropertiesID GetMixerProperties(Mixer mixer);

    /// <code>extern SDL_DECLSPEC bool SDLCALL MIX_GetMixerFormat(MIX_Mixer *mixer, SDL_AudioSpec *spec);</code>
    /// <summary> Get the audio format a mixer is processing in. </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_GetMixerFormat"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool GetMixerFormat(Mixer mixer, out SDL.AudioSpec spec);

    /// <code>extern SDL_DECLSPEC void SDLCALL MIX_LockMixer(MIX_Mixer *mixer);</code>
    /// <summary> Lock the mixer so several operations apply atomically against the mixing thread. </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_LockMixer"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void LockMixer(Mixer mixer);

    /// <code>extern SDL_DECLSPEC void SDLCALL MIX_UnlockMixer(MIX_Mixer *mixer);</code>
    /// <summary> Unlock the mixer. </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_UnlockMixer"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void UnlockMixer(Mixer mixer);

    #endregion

    #region Audio loading

    /// <code>extern SDL_DECLSPEC MIX_Audio * SDLCALL MIX_LoadAudio_IO(MIX_Mixer *mixer, SDL_IOStream *io, bool predecode, bool closeio);</code>
    /// <summary>
    /// Load audio for playback from an SDL_IOStream. With <paramref name="predecode"/> false the data
    /// is decompressed on demand (streamed); true decodes everything upfront (best for short SFX).
    /// </summary>
    /// <returns> an audio handle, or 0 on failure; call <see cref="SDL.GetError"/> for more information. </returns>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_LoadAudio_IO"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial Audio LoadAudioIO(Mixer mixer,
        IOStreamHandle io,
        [MarshalAs(UnmanagedType.I1)] bool predecode,
        [MarshalAs(UnmanagedType.I1)] bool closeio);

    [LibraryImport(MixerLibrary, EntryPoint = "MIX_LoadAudio"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial Audio SDL_LoadAudio(Mixer mixer, byte* path, [MarshalAs(UnmanagedType.I1)] bool predecode);

    /// <code>extern SDL_DECLSPEC MIX_Audio * SDLCALL MIX_LoadAudio(MIX_Mixer *mixer, const char *path, bool predecode);</code>
    /// <summary> Load audio for playback from a file path. See <see cref="LoadAudioIO"/> for predecode semantics. </summary>
    /// <returns> an audio handle, or 0 on failure; call <see cref="SDL.GetError"/> for more information. </returns>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    public static unsafe Audio LoadAudio(Mixer mixer, scoped ReadOnlySpan<char> path, bool predecode)
    {
        var byteCount = Encoding.UTF8.GetByteCount(path) + 1;
        byte[]? rented = null;
        scoped Span<byte> utf8 = byteCount <= 512 ?
            stackalloc byte[byteCount] :
            (rented = ArrayPool<byte>.Shared.Rent(byteCount)).AsSpan(0, byteCount);

        Encoding.UTF8.GetBytes(path, utf8);
        utf8[^1] = 0;

        try
        {
            fixed (byte* pathPtr = utf8) return SDL_LoadAudio(mixer, pathPtr, predecode);
        }
        finally
        {
            if (rented is not null) ArrayPool<byte>.Shared.Return(rented);
        }
    }

    /// <code>extern SDL_DECLSPEC MIX_Audio * SDLCALL MIX_LoadAudioNoCopy(MIX_Mixer *mixer, const void *data, size_t datalen, bool free_when_done);</code>
    /// <summary>
    /// Load audio from a raw pointer WITHOUT copying: the memory must stay valid and unchanged for the
    /// lifetime of the returned audio. With <paramref name="freeWhenDone"/> the pointer is released
    /// with SDL_free on destruction (it must come from SDL's allocator).
    /// </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_LoadAudioNoCopy"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial Audio LoadAudioNoCopy(Mixer mixer,
        nint data,
        nuint datalen,
        [MarshalAs(UnmanagedType.I1)] bool freeWhenDone);

    /// <code>extern SDL_DECLSPEC MIX_Audio * SDLCALL MIX_LoadAudioWithProperties(SDL_PropertiesID props);</code>
    /// <summary> Load audio through a property set (see <see cref="Props"/> AudioLoad* keys). </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_LoadAudioWithProperties"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial Audio LoadAudioWithProperties(PropertiesID props);

    /// <summary>
    ///     (storybrew fork helper) Load audio for streaming WITHOUT holding the compressed file in
    ///     RAM: decoding reads from the (seekable) IOStream on demand via
    ///     <see cref="Props.AudioLoadOnDemand"/>. Only one track/decoder may consume the returned
    ///     audio at a time.
    ///     <para>
    ///         WARNING: as of SDL_mixer 3.2.4 this path produced broken playback in storybrew's
    ///         seek-heavy pipeline (the default precache load did not). Treat as experimental;
    ///         prefer <see cref="LoadAudio(Mixer, System.ReadOnlySpan{char}, bool)"/> with
    ///         predecode false, which keeps one compressed copy in RAM but never holds the decoded audio.
    ///     </para>
    /// </summary>
    /// <returns> an audio handle, or 0 on failure; call <see cref="SDL.GetError"/> for more information. </returns>
    public static Audio LoadAudioOnDemand(Mixer mixer, IOStreamHandle io, bool closeio)
        => io.IsNull
            ? Audio.Null
            : LoadAudio(mixer, p => p.IOStream(io, closeio).OnDemand().PreferredMixer(mixer));

    /// <code>extern SDL_DECLSPEC MIX_Audio * SDLCALL MIX_LoadRawAudio_IO(MIX_Mixer *mixer, SDL_IOStream *io, const SDL_AudioSpec *spec, bool closeio);</code>
    /// <summary> Load raw (headerless) PCM from an SDL_IOStream in the given format. </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_LoadRawAudio_IO"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial Audio LoadRawAudioIO(Mixer mixer,
        IOStreamHandle io,
        in SDL.AudioSpec spec,
        [MarshalAs(UnmanagedType.I1)] bool closeio);

    [LibraryImport(MixerLibrary, EntryPoint = "MIX_LoadRawAudio"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial Audio SDL_LoadRawAudio(Mixer mixer, byte* data, nuint datalen, SDL.AudioSpec* spec);

    /// <code>extern SDL_DECLSPEC MIX_Audio * SDLCALL MIX_LoadRawAudio(MIX_Mixer *mixer, const void *data, size_t datalen, const SDL_AudioSpec *spec);</code>
    /// <summary> Load raw (headerless) PCM from a buffer; the data is copied, the span may be released afterwards. </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    public static unsafe Audio LoadRawAudio(Mixer mixer, scoped ReadOnlySpan<byte> data, in SDL.AudioSpec spec)
    {
        fixed (byte* dataPtr = data)
        fixed (SDL.AudioSpec* specPtr = &spec)
            return SDL_LoadRawAudio(mixer, dataPtr, (nuint)data.Length, specPtr);
    }

    /// <code>extern SDL_DECLSPEC MIX_Audio * SDLCALL MIX_LoadRawAudioNoCopy(MIX_Mixer *mixer, const void *data, size_t datalen, const SDL_AudioSpec *spec, bool free_when_done);</code>
    /// <summary> Raw PCM variant of <see cref="LoadAudioNoCopy"/>; the same lifetime rules apply to <paramref name="data"/>. </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_LoadRawAudioNoCopy"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial Audio LoadRawAudioNoCopy(Mixer mixer,
        nint data,
        nuint datalen,
        in SDL.AudioSpec spec,
        [MarshalAs(UnmanagedType.I1)] bool freeWhenDone);

    /// <code>extern SDL_DECLSPEC MIX_Audio * SDLCALL MIX_CreateSineWaveAudio(MIX_Mixer *mixer, int hz, float amplitude, Sint64 ms);</code>
    /// <summary> Create a MIX_Audio that generates a sine wave (mostly for testing). </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_CreateSineWaveAudio"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial Audio CreateSineWaveAudio(Mixer mixer, int hz, float amplitude, long ms);

    /// <code>extern SDL_DECLSPEC SDL_PropertiesID SDLCALL MIX_GetAudioProperties(MIX_Audio *audio);</code>
    /// <summary> Get the properties associated with a MIX_Audio (metadata lives here, see <see cref="Props"/>). </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_GetAudioProperties"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial PropertiesID GetAudioProperties(Audio audio);

    /// <code>extern SDL_DECLSPEC Sint64 SDLCALL MIX_GetAudioDuration(MIX_Audio *audio);</code>
    /// <summary>
    /// Get the length of a MIX_Audio's playback in sample frames (-1 on error, 0 for infinite/unknown).
    /// Use <see cref="AudioFramesToMS"/> to convert to milliseconds.
    /// </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_GetAudioDuration"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial long GetAudioDuration(Audio audio);

    /// <code>extern SDL_DECLSPEC bool SDLCALL MIX_GetAudioFormat(MIX_Audio *audio, SDL_AudioSpec *spec);</code>
    /// <summary> Get the format of a MIX_Audio's decoded data. </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_GetAudioFormat"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool GetAudioFormat(Audio audio, out SDL.AudioSpec spec);

    /// <code>extern SDL_DECLSPEC void SDLCALL MIX_DestroyAudio(MIX_Audio *audio);</code>
    /// <summary> Destroy a MIX_Audio (reference-counted: tracks still using it keep it alive). </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_DestroyAudio"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void DestroyAudio(Audio audio);

    #endregion

    #region Tracks

    /// <code>extern SDL_DECLSPEC MIX_Track * SDLCALL MIX_CreateTrack(MIX_Mixer *mixer);</code>
    /// <summary> Create a track on a mixer. Tracks are the unit of playback. </summary>
    /// <returns> a track handle, or 0 on failure; call <see cref="SDL.GetError"/> for more information. </returns>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_CreateTrack"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial Track CreateTrack(Mixer mixer);

    [LibraryImport(MixerLibrary, EntryPoint = "MIX_DestroyTrack"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void SDL_DestroyTrack(Track track);

    /// <code>extern SDL_DECLSPEC void SDLCALL MIX_DestroyTrack(MIX_Track *track);</code>
    /// <summary> Destroy a track and release any callback rooting registered for it through these wrappers. </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    public static void DestroyTrack(Track track)
    {
        SDL_DestroyTrack(track);
        ReleaseCallback(trackStoppedCallbacks, track.Value);
        ReleaseCallback(trackRawCallbacks, track.Value);
        ReleaseCallback(trackCookedCallbacks, track.Value);
    }

    /// <code>extern SDL_DECLSPEC SDL_PropertiesID SDLCALL MIX_GetTrackProperties(MIX_Track *track);</code>
    /// <summary> Get the properties associated with a track. </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_GetTrackProperties"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial PropertiesID GetTrackProperties(Track track);

    /// <code>extern SDL_DECLSPEC MIX_Mixer * SDLCALL MIX_GetTrackMixer(MIX_Track *track);</code>
    /// <summary> Get the mixer that owns a track. </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_GetTrackMixer"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial Mixer GetTrackMixer(Track track);

    /// <code>extern SDL_DECLSPEC bool SDLCALL MIX_SetTrackAudio(MIX_Track *track, MIX_Audio *audio);</code>
    /// <summary> Set a track's input to a MIX_Audio (0 clears the input). </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_SetTrackAudio"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool SetTrackAudio(Track track, Audio audio);

    /// <code>extern SDL_DECLSPEC bool SDLCALL MIX_SetTrackAudioStream(MIX_Track *track, SDL_AudioStream *stream);</code>
    /// <summary>
    /// Set a track's input to an SDL_AudioStream the app feeds (or whose get-callback feeds). Note
    /// that stream-fed tracks cannot seek via <see cref="SetTrackPlaybackPosition"/>.
    /// </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_SetTrackAudioStream"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool SetTrackAudioStream(Track track, AudioStreamHandle stream);

    /// <code>extern SDL_DECLSPEC bool SDLCALL MIX_SetTrackIOStream(MIX_Track *track, SDL_IOStream *io, bool closeio);</code>
    /// <summary> Set a track's input to an SDL_IOStream containing a supported audio file format. </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_SetTrackIOStream"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool SetTrackIOStream(Track track, IOStreamHandle io, [MarshalAs(UnmanagedType.I1)] bool closeio);

    /// <code>extern SDL_DECLSPEC bool SDLCALL MIX_SetTrackRawIOStream(MIX_Track *track, SDL_IOStream *io, const SDL_AudioSpec *spec, bool closeio);</code>
    /// <summary> Set a track's input to an SDL_IOStream of raw (headerless) PCM in the given format. </summary>
    /// <since> This function is available since SDL_mixer 3.2.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_SetTrackRawIOStream"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool SetTrackRawIOStream(Track track,
        IOStreamHandle io,
        in SDL.AudioSpec spec,
        [MarshalAs(UnmanagedType.I1)] bool closeio);

    /// <code>extern SDL_DECLSPEC bool SDLCALL MIX_SetTrackPlaybackPosition(MIX_Track *track, Sint64 frames);</code>
    /// <summary>
    /// Seek a track to a sample-frame position in its input. Requires a seekable input - this cannot
    /// be used when the input was set with <see cref="SetTrackAudioStream"/>.
    /// </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_SetTrackPlaybackPosition"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool SetTrackPlaybackPosition(Track track, long frames);

    /// <code>extern SDL_DECLSPEC Sint64 SDLCALL MIX_GetTrackPlaybackPosition(MIX_Track *track);</code>
    /// <summary> Get a track's current input position in sample frames (-1 on error). </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_GetTrackPlaybackPosition"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial long GetTrackPlaybackPosition(Track track);

    /// <code>extern SDL_DECLSPEC Sint64 SDLCALL MIX_GetTrackFadeFrames(MIX_Track *track);</code>
    /// <summary> Get the number of sample frames remaining in a track's current fade (0 if not fading). </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_GetTrackFadeFrames"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial long GetTrackFadeFrames(Track track);

    /// <code>extern SDL_DECLSPEC int SDLCALL MIX_GetTrackLoops(MIX_Track *track);</code>
    /// <summary> Get the number of loops remaining on a track (-1 means looping forever). </summary>
    /// <since> This function is available since SDL_mixer 3.2.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_GetTrackLoops"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial int GetTrackLoops(Track track);

    /// <code>extern SDL_DECLSPEC bool SDLCALL MIX_SetTrackLoops(MIX_Track *track, int num_loops);</code>
    /// <summary> Change the loop count of a (possibly playing) track. </summary>
    /// <since> This function is available since SDL_mixer 3.2.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_SetTrackLoops"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool SetTrackLoops(Track track, int numLoops);

    /// <code>extern SDL_DECLSPEC MIX_Audio * SDLCALL MIX_GetTrackAudio(MIX_Track *track);</code>
    /// <summary> Get the MIX_Audio assigned to a track (0 if none or input is a stream). </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_GetTrackAudio"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial Audio GetTrackAudio(Track track);

    /// <code>extern SDL_DECLSPEC SDL_AudioStream * SDLCALL MIX_GetTrackAudioStream(MIX_Track *track);</code>
    /// <summary> Get the SDL_AudioStream assigned to a track (0 if none or input is a MIX_Audio). </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_GetTrackAudioStream"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial AudioStreamHandle GetTrackAudioStream(Track track);

    /// <code>extern SDL_DECLSPEC Sint64 SDLCALL MIX_GetTrackRemaining(MIX_Track *track);</code>
    /// <summary> Get the sample frames remaining to be played on a track. </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_GetTrackRemaining"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial long GetTrackRemaining(Track track);

    #endregion

    #region Frame/time conversion

    /// <code>extern SDL_DECLSPEC Sint64 SDLCALL MIX_TrackMSToFrames(MIX_Track *track, Sint64 ms);</code>
    /// <summary> Convert milliseconds to sample frames at the track's current input rate (-1 on error). </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_TrackMSToFrames"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial long TrackMSToFrames(Track track, long ms);

    /// <code>extern SDL_DECLSPEC Sint64 SDLCALL MIX_TrackFramesToMS(MIX_Track *track, Sint64 frames);</code>
    /// <summary> Convert sample frames at the track's current input rate to milliseconds (-1 on error). </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_TrackFramesToMS"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial long TrackFramesToMS(Track track, long frames);

    /// <code>extern SDL_DECLSPEC Sint64 SDLCALL MIX_AudioMSToFrames(MIX_Audio *audio, Sint64 ms);</code>
    /// <summary> Convert milliseconds to sample frames at a MIX_Audio's native rate (-1 on error). </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_AudioMSToFrames"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial long AudioMSToFrames(Audio audio, long ms);

    /// <code>extern SDL_DECLSPEC Sint64 SDLCALL MIX_AudioFramesToMS(MIX_Audio *audio, Sint64 frames);</code>
    /// <summary> Convert sample frames at a MIX_Audio's native rate to milliseconds (-1 on error). </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_AudioFramesToMS"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial long AudioFramesToMS(Audio audio, long frames);

    /// <code>extern SDL_DECLSPEC Sint64 SDLCALL MIX_MSToFrames(int sample_rate, Sint64 ms);</code>
    /// <summary> Convert milliseconds to sample frames at an arbitrary rate (-1 on error). </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_MSToFrames"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial long MSToFrames(int sampleRate, long ms);

    /// <code>extern SDL_DECLSPEC Sint64 SDLCALL MIX_FramesToMS(int sample_rate, Sint64 frames);</code>
    /// <summary> Convert sample frames at an arbitrary rate to milliseconds (-1 on error). </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_FramesToMS"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial long FramesToMS(int sampleRate, long frames);

    #endregion

    #region Playback control

    /// <code>extern SDL_DECLSPEC bool SDLCALL MIX_PlayTrack(MIX_Track *track, SDL_PropertiesID options);</code>
    /// <summary>
    /// Start (or restart) playing a track. <paramref name="options"/> is an SDL_PropertiesID with
    /// <see cref="Props"/> Play* keys, or 0 for defaults.
    /// </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_PlayTrack"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool PlayTrack(Track track, PropertiesID options = default);

    /// <code>extern SDL_DECLSPEC bool SDLCALL MIX_PlayAudio(MIX_Mixer *mixer, MIX_Audio *audio);</code>
    /// <summary> Fire-and-forget: play a MIX_Audio once on an internal track with default options. </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_PlayAudio"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool PlayAudio(Mixer mixer, Audio audio);

    /// <code>extern SDL_DECLSPEC bool SDLCALL MIX_StopTrack(MIX_Track *track, Sint64 fade_out_frames);</code>
    /// <summary> Halt a playing track, optionally fading out over <paramref name="fadeOutFrames"/> sample frames. </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_StopTrack"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool StopTrack(Track track, long fadeOutFrames = 0);

    /// <code>extern SDL_DECLSPEC bool SDLCALL MIX_StopAllTracks(MIX_Mixer *mixer, Sint64 fade_out_ms);</code>
    /// <summary> Halt all playing tracks on a mixer, optionally fading out over <paramref name="fadeOutMS"/> milliseconds. </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_StopAllTracks"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool StopAllTracks(Mixer mixer, long fadeOutMS = 0);

    /// <code>extern SDL_DECLSPEC bool SDLCALL MIX_PauseTrack(MIX_Track *track);</code>
    /// <summary> Pause a playing track (resumable with <see cref="ResumeTrack"/>). </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_PauseTrack"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool PauseTrack(Track track);

    /// <code>extern SDL_DECLSPEC bool SDLCALL MIX_PauseAllTracks(MIX_Mixer *mixer);</code>
    /// <summary> Pause all playing tracks on a mixer. </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_PauseAllTracks"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool PauseAllTracks(Mixer mixer);

    /// <code>extern SDL_DECLSPEC bool SDLCALL MIX_ResumeTrack(MIX_Track *track);</code>
    /// <summary> Resume a paused track. </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_ResumeTrack"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool ResumeTrack(Track track);

    /// <code>extern SDL_DECLSPEC bool SDLCALL MIX_ResumeAllTracks(MIX_Mixer *mixer);</code>
    /// <summary> Resume all paused tracks on a mixer. </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_ResumeAllTracks"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool ResumeAllTracks(Mixer mixer);

    /// <code>extern SDL_DECLSPEC bool SDLCALL MIX_TrackPlaying(MIX_Track *track);</code>
    /// <summary> Query whether a track is currently playing (paused tracks report false). </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_TrackPlaying"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool TrackPlaying(Track track);

    /// <code>extern SDL_DECLSPEC bool SDLCALL MIX_TrackPaused(MIX_Track *track);</code>
    /// <summary> Query whether a track is currently paused. </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_TrackPaused"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool TrackPaused(Track track);

    #endregion

    #region Gain, frequency, spatialization

    /// <code>extern SDL_DECLSPEC bool SDLCALL MIX_SetMixerGain(MIX_Mixer *mixer, float gain);</code>
    /// <summary> Set the master gain applied to the final mix (1.0 = no change, 0 = silence). </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_SetMixerGain"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool SetMixerGain(Mixer mixer, float gain);

    /// <code>extern SDL_DECLSPEC float SDLCALL MIX_GetMixerGain(MIX_Mixer *mixer);</code>
    /// <summary> Get the mixer's master gain (-1 on error). </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_GetMixerGain"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial float GetMixerGain(Mixer mixer);

    /// <code>extern SDL_DECLSPEC bool SDLCALL MIX_SetTrackGain(MIX_Track *track, float gain);</code>
    /// <summary> Set a track's gain (1.0 = no change, 0 = silence). </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_SetTrackGain"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool SetTrackGain(Track track, float gain);

    /// <code>extern SDL_DECLSPEC float SDLCALL MIX_GetTrackGain(MIX_Track *track);</code>
    /// <summary> Get a track's gain (-1 on error). </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_GetTrackGain"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial float GetTrackGain(Track track);

    /// <code>extern SDL_DECLSPEC bool SDLCALL MIX_SetMixerFrequencyRatio(MIX_Mixer *mixer, float ratio);</code>
    /// <summary> Change the frequency ratio of the whole mix (0.01 to 100; speed and pitch change together). </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_SetMixerFrequencyRatio"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool SetMixerFrequencyRatio(Mixer mixer, float ratio);

    /// <code>extern SDL_DECLSPEC float SDLCALL MIX_GetMixerFrequencyRatio(MIX_Mixer *mixer);</code>
    /// <summary> Get the mixer's frequency ratio (-1 on error). </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_GetMixerFrequencyRatio"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial float GetMixerFrequencyRatio(Mixer mixer);

    /// <code>extern SDL_DECLSPEC bool SDLCALL MIX_SetTrackFrequencyRatio(MIX_Track *track, float ratio);</code>
    /// <summary>
    /// Change the frequency ratio of a track (0.01 to 100). Values above 1.0 play faster and at a
    /// higher pitch, below 1.0 slower and lower; speed and pitch change together.
    /// </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_SetTrackFrequencyRatio"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool SetTrackFrequencyRatio(Track track, float ratio);

    /// <code>extern SDL_DECLSPEC float SDLCALL MIX_GetTrackFrequencyRatio(MIX_Track *track);</code>
    /// <summary> Get a track's frequency ratio (-1 on error). </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_GetTrackFrequencyRatio"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial float GetTrackFrequencyRatio(Track track);

    [LibraryImport(MixerLibrary, EntryPoint = "MIX_SetTrackOutputChannelMap"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial byte SDL_SetTrackOutputChannelMap(Track track, int* chmap, int count);

    /// <code>extern SDL_DECLSPEC bool SDLCALL MIX_SetTrackOutputChannelMap(MIX_Track *track, const int *chmap, int count);</code>
    /// <summary> Set a track's output channel remapping (empty span resets to default). </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    public static unsafe bool SetTrackOutputChannelMap(Track track, scoped ReadOnlySpan<int> chmap)
    {
        fixed (int* chmapPtr = chmap)
            return SDL_SetTrackOutputChannelMap(track, chmap.IsEmpty ? null : chmapPtr, chmap.Length) != 0;
    }

    [LibraryImport(MixerLibrary, EntryPoint = "MIX_SetTrackStereo"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial byte SDL_SetTrackStereo(Track track, StereoGains* gains);

    /// <code>extern SDL_DECLSPEC bool SDLCALL MIX_SetTrackStereo(MIX_Track *track, const MIX_StereoGains *gains);</code>
    /// <summary> Force a track to stereo output with explicit left/right gains (this disables 3D positioning). </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    public static unsafe bool SetTrackStereo(Track track, in StereoGains gains)
    {
        fixed (StereoGains* gainsPtr = &gains) return SDL_SetTrackStereo(track, gainsPtr) != 0;
    }

    /// <summary> Disable forced-stereo on a track (passes NULL to the native call). </summary>
    /// <inheritdoc cref="SetTrackStereo(Track, in StereoGains)"/>
    public static unsafe bool SetTrackStereo(Track track) => SDL_SetTrackStereo(track, null) != 0;

    [LibraryImport(MixerLibrary, EntryPoint = "MIX_SetTrack3DPosition"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial byte SDL_SetTrack3DPosition(Track track, Point3D* position);

    /// <code>extern SDL_DECLSPEC bool SDLCALL MIX_SetTrack3DPosition(MIX_Track *track, const MIX_Point3D *position);</code>
    /// <summary> Set a track's position in 3D space (this disables forced-stereo). </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    public static unsafe bool SetTrack3DPosition(Track track, in Point3D position)
    {
        fixed (Point3D* positionPtr = &position) return SDL_SetTrack3DPosition(track, positionPtr) != 0;
    }

    /// <summary> Disable 3D positioning on a track (passes NULL to the native call). </summary>
    /// <inheritdoc cref="SetTrack3DPosition(Track, in Point3D)"/>
    public static unsafe bool SetTrack3DPosition(Track track) => SDL_SetTrack3DPosition(track, null) != 0;

    /// <code>extern SDL_DECLSPEC bool SDLCALL MIX_GetTrack3DPosition(MIX_Track *track, MIX_Point3D *position);</code>
    /// <summary> Get a track's 3D position. </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_GetTrack3DPosition"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool GetTrack3DPosition(Track track, out Point3D position);

    #endregion

    #region Groups

    /// <code>extern SDL_DECLSPEC MIX_Group * SDLCALL MIX_CreateGroup(MIX_Mixer *mixer);</code>
    /// <summary> Create a mixing group (lets several tracks be post-processed together). </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_CreateGroup"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial Group CreateGroup(Mixer mixer);

    [LibraryImport(MixerLibrary, EntryPoint = "MIX_DestroyGroup"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void SDL_DestroyGroup(Group group);

    /// <code>extern SDL_DECLSPEC void SDLCALL MIX_DestroyGroup(MIX_Group *group);</code>
    /// <summary> Destroy a mixing group and release its callback rooting. </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    public static void DestroyGroup(Group group)
    {
        SDL_DestroyGroup(group);
        ReleaseCallback(groupMixCallbacks, group.Value);
    }

    /// <code>extern SDL_DECLSPEC SDL_PropertiesID SDLCALL MIX_GetGroupProperties(MIX_Group *group);</code>
    /// <summary> Get the properties associated with a group. </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_GetGroupProperties"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial PropertiesID GetGroupProperties(Group group);

    /// <code>extern SDL_DECLSPEC MIX_Mixer * SDLCALL MIX_GetGroupMixer(MIX_Group *group);</code>
    /// <summary> Get the mixer that owns a group. </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_GetGroupMixer"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial Mixer GetGroupMixer(Group group);

    /// <code>extern SDL_DECLSPEC bool SDLCALL MIX_SetTrackGroup(MIX_Track *track, MIX_Group *group);</code>
    /// <summary> Assign a track to a group (0 returns it to the mixer's default group). </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_SetTrackGroup"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool SetTrackGroup(Track track, Group group);

    #endregion

    #region Tags

    [LibraryImport(MixerLibrary, EntryPoint = "MIX_TagTrack"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial byte SDL_TagTrack(Track track, byte* tag);

    [LibraryImport(MixerLibrary, EntryPoint = "MIX_UntagTrack"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial void SDL_UntagTrack(Track track, byte* tag);

    [LibraryImport(MixerLibrary, EntryPoint = "MIX_PlayTag"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial byte SDL_PlayTag(Mixer mixer, byte* tag, PropertiesID options);

    [LibraryImport(MixerLibrary, EntryPoint = "MIX_StopTag"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial byte SDL_StopTag(Mixer mixer, byte* tag, long fadeOutMS);

    [LibraryImport(MixerLibrary, EntryPoint = "MIX_PauseTag"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial byte SDL_PauseTag(Mixer mixer, byte* tag);

    [LibraryImport(MixerLibrary, EntryPoint = "MIX_ResumeTag"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial byte SDL_ResumeTag(Mixer mixer, byte* tag);

    [LibraryImport(MixerLibrary, EntryPoint = "MIX_SetTagGain"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial byte SDL_SetTagGain(Mixer mixer, byte* tag, float gain);

    [LibraryImport(MixerLibrary, EntryPoint = "MIX_GetTrackTags"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial nint SDL_GetTrackTags(Track track, out int count);

    [LibraryImport(MixerLibrary, EntryPoint = "MIX_GetTaggedTracks"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial nint SDL_GetTaggedTracks(Mixer mixer, byte* tag, out int count);

    /// <code>extern SDL_DECLSPEC bool SDLCALL MIX_TagTrack(MIX_Track *track, const char *tag);</code>
    /// <summary> Add an arbitrary tag to a track (tracks can be addressed as a group by tag). </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    public static unsafe bool TagTrack(Track track, scoped ReadOnlySpan<char> tag)
    {
        using TagUtf8 utf8 = new(tag);
        fixed (byte* tagPtr = utf8.Span) return SDL_TagTrack(track, tagPtr) != 0;
    }

    /// <code>extern SDL_DECLSPEC void SDLCALL MIX_UntagTrack(MIX_Track *track, const char *tag);</code>
    /// <summary> Remove a tag from a track. </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    public static unsafe void UntagTrack(Track track, scoped ReadOnlySpan<char> tag)
    {
        using TagUtf8 utf8 = new(tag);
        fixed (byte* tagPtr = utf8.Span) SDL_UntagTrack(track, tagPtr);
    }

    /// <code>extern SDL_DECLSPEC bool SDLCALL MIX_PlayTag(MIX_Mixer *mixer, const char *tag, SDL_PropertiesID options);</code>
    /// <summary> Start playing all tracks with a specific tag with the same options (see <see cref="PlayTrack"/>). </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    public static unsafe bool PlayTag(Mixer mixer, scoped ReadOnlySpan<char> tag, PropertiesID options = default)
    {
        using TagUtf8 utf8 = new(tag);
        fixed (byte* tagPtr = utf8.Span) return SDL_PlayTag(mixer, tagPtr, options) != 0;
    }

    /// <code>extern SDL_DECLSPEC bool SDLCALL MIX_StopTag(MIX_Mixer *mixer, const char *tag, Sint64 fade_out_ms);</code>
    /// <summary> Halt all tracks with a specific tag, optionally fading out. </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    public static unsafe bool StopTag(Mixer mixer, scoped ReadOnlySpan<char> tag, long fadeOutMS = 0)
    {
        using TagUtf8 utf8 = new(tag);
        fixed (byte* tagPtr = utf8.Span) return SDL_StopTag(mixer, tagPtr, fadeOutMS) != 0;
    }

    /// <code>extern SDL_DECLSPEC bool SDLCALL MIX_PauseTag(MIX_Mixer *mixer, const char *tag);</code>
    /// <summary> Pause all tracks with a specific tag. </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    public static unsafe bool PauseTag(Mixer mixer, scoped ReadOnlySpan<char> tag)
    {
        using TagUtf8 utf8 = new(tag);
        fixed (byte* tagPtr = utf8.Span) return SDL_PauseTag(mixer, tagPtr) != 0;
    }

    /// <code>extern SDL_DECLSPEC bool SDLCALL MIX_ResumeTag(MIX_Mixer *mixer, const char *tag);</code>
    /// <summary> Resume all tracks with a specific tag. </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    public static unsafe bool ResumeTag(Mixer mixer, scoped ReadOnlySpan<char> tag)
    {
        using TagUtf8 utf8 = new(tag);
        fixed (byte* tagPtr = utf8.Span) return SDL_ResumeTag(mixer, tagPtr) != 0;
    }

    /// <code>extern SDL_DECLSPEC bool SDLCALL MIX_SetTagGain(MIX_Mixer *mixer, const char *tag, float gain);</code>
    /// <summary> Set the gain of all tracks with a specific tag. </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    public static unsafe bool SetTagGain(Mixer mixer, scoped ReadOnlySpan<char> tag, float gain)
    {
        using TagUtf8 utf8 = new(tag);
        fixed (byte* tagPtr = utf8.Span) return SDL_SetTagGain(mixer, tagPtr, gain) != 0;
    }

    /// <code>extern SDL_DECLSPEC char ** SDLCALL MIX_GetTrackTags(MIX_Track *track, int *count);</code>
    /// <summary> Get all tags on a track (allocates; intended for diagnostics, not hot paths). </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    public static string[]? GetTrackTags(Track track)
    {
        var ptr = SDL_GetTrackTags(track, out var count);
        if (ptr == 0) return null;

        var tags = SDL.PointerToStringArray(ptr, count);
        SDL.Free(ptr);
        return tags;
    }

    /// <code>extern SDL_DECLSPEC MIX_Track ** SDLCALL MIX_GetTaggedTracks(MIX_Mixer *mixer, const char *tag, int *count);</code>
    /// <summary> Get all tracks with a specific tag (allocates; intended for diagnostics, not hot paths). </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    public static unsafe Track[]? GetTaggedTracks(Mixer mixer, scoped ReadOnlySpan<char> tag)
    {
        nint ptr;
        int count;

        using (TagUtf8 utf8 = new(tag))
            fixed (byte* tagPtr = utf8.Span)
                ptr = SDL_GetTaggedTracks(mixer, tagPtr, out count);

        if (ptr == 0) return null;

        var tracks = new ReadOnlySpan<Track>((void*)ptr, count).ToArray();
        SDL.Free(ptr);
        return tracks;
    }

    /// <summary> Pooled scratch for null-terminated UTF-8 tag encoding (tags are never hot paths). </summary>
    readonly ref struct TagUtf8
    {
        readonly byte[] rented;
        public readonly ReadOnlySpan<byte> Span;

        public TagUtf8(scoped ReadOnlySpan<char> text)
        {
            var byteCount = Encoding.UTF8.GetByteCount(text) + 1;
            rented = ArrayPool<byte>.Shared.Rent(byteCount);

            Encoding.UTF8.GetBytes(text, rented);
            rented[byteCount - 1] = 0;
            Span = rented.AsSpan(0, byteCount);
        }

        public void Dispose() => ArrayPool<byte>.Shared.Return(rented);
    }

    #endregion

    #region Callbacks

    static readonly Dictionary<nint, GCHandle> trackStoppedCallbacks = [];
    static readonly Dictionary<nint, GCHandle> trackRawCallbacks = [];
    static readonly Dictionary<nint, GCHandle> trackCookedCallbacks = [];
    static readonly Dictionary<nint, GCHandle> groupMixCallbacks = [];
    static readonly Dictionary<nint, GCHandle> mixerPostMixCallbacks = [];
    static readonly object callbackLock = new();

    abstract class StoppedInvoker
    {
        public abstract void Invoke(Track track);
    }

    sealed class PlainStoppedInvoker(TrackStoppedCallback callback) : StoppedInvoker
    {
        public override void Invoke(Track track) => callback(track);
    }

    sealed class StatefulStoppedInvoker<T>(TrackStoppedCallback<T> callback, T state) : StoppedInvoker
    {
        T state = state;
        public override void Invoke(Track track) => callback(track, ref state);
    }

    abstract class MixInvoker
    {
        public abstract void Invoke(nint handle, in SDL.AudioSpec spec, Span<float> pcm);
    }

    sealed class PlainTrackMixInvoker(TrackMixCallback callback) : MixInvoker
    {
        public override void Invoke(nint handle, in SDL.AudioSpec spec, Span<float> pcm)
            => callback(new(handle), in spec, pcm);
    }

    sealed class StatefulTrackMixInvoker<T>(TrackMixCallback<T> callback, T state) : MixInvoker
    {
        T state = state;

        public override void Invoke(nint handle, in SDL.AudioSpec spec, Span<float> pcm)
            => callback(new(handle), in spec, pcm, ref state);
    }

    sealed class PlainGroupMixInvoker(GroupMixCallback callback) : MixInvoker
    {
        public override void Invoke(nint handle, in SDL.AudioSpec spec, Span<float> pcm)
            => callback(new(handle), in spec, pcm);
    }

    sealed class StatefulGroupMixInvoker<T>(GroupMixCallback<T> callback, T state) : MixInvoker
    {
        T state = state;

        public override void Invoke(nint handle, in SDL.AudioSpec spec, Span<float> pcm)
            => callback(new(handle), in spec, pcm, ref state);
    }

    sealed class PlainPostMixInvoker(PostMixCallback callback) : MixInvoker
    {
        public override void Invoke(nint handle, in SDL.AudioSpec spec, Span<float> pcm)
            => callback(new(handle), in spec, pcm);
    }

    sealed class StatefulPostMixInvoker<T>(PostMixCallback<T> callback, T state) : MixInvoker
    {
        T state = state;

        public override void Invoke(nint handle, in SDL.AudioSpec spec, Span<float> pcm)
            => callback(new(handle), in spec, pcm, ref state);
    }

    /// <summary>
    /// Commits a successful native registration: roots the new invoker handle (or removes the entry
    /// for null) and frees the previous rooting. The previous GCHandle must only be freed AFTER the
    /// native setter returned, because the native call is what synchronizes against a mixing thread
    /// that may still be inside the old callback.
    /// </summary>
    static void CommitCallback(Dictionary<nint, GCHandle> registry, nint key, GCHandle handle, bool rooted)
    {
        lock (callbackLock)
        {
            if (registry.Remove(key, out var previous)) previous.Free();
            if (rooted) registry[key] = handle;
        }
    }

    static void ReleaseCallback(Dictionary<nint, GCHandle> registry, nint key)
    {
        lock (callbackLock)
            if (registry.Remove(key, out var handle))
                handle.Free();
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static void TrackStoppedThunk(nint userdata, Track track)
    {
        try
        {
            ((StoppedInvoker)GCHandle.FromIntPtr(userdata).Target!).Invoke(track);
        }
        catch (Exception exception)
        {
            SDL.ReportCallbackException(exception);
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static unsafe void MixThunk(nint userdata, nint handle, SDL.AudioSpec* spec, float* pcm, int samples)
    {
        try
        {
            ((MixInvoker)GCHandle.FromIntPtr(userdata).Target!).Invoke(handle, in *spec, new(pcm, samples));
        }
        catch (Exception exception)
        {
            SDL.ReportCallbackException(exception);
        }
    }

    [LibraryImport(MixerLibrary, EntryPoint = "MIX_SetTrackStoppedCallback"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial byte SDL_SetTrackStoppedCallback(nint owner,
        delegate* unmanaged[Cdecl]<nint, Track, void> cb,
        nint userdata);

    [LibraryImport(MixerLibrary, EntryPoint = "MIX_SetTrackRawCallback"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial byte SDL_SetTrackRawCallback(nint owner,
        delegate* unmanaged[Cdecl]<nint, nint, SDL.AudioSpec*, float*, int, void> cb,
        nint userdata);

    [LibraryImport(MixerLibrary, EntryPoint = "MIX_SetTrackCookedCallback"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial byte SDL_SetTrackCookedCallback(nint owner,
        delegate* unmanaged[Cdecl]<nint, nint, SDL.AudioSpec*, float*, int, void> cb,
        nint userdata);

    [LibraryImport(MixerLibrary, EntryPoint = "MIX_SetGroupPostMixCallback"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial byte SDL_SetGroupPostMixCallback(nint owner,
        delegate* unmanaged[Cdecl]<nint, nint, SDL.AudioSpec*, float*, int, void> cb,
        nint userdata);

    [LibraryImport(MixerLibrary, EntryPoint = "MIX_SetPostMixCallback"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial byte SDL_SetPostMixCallback(nint owner,
        delegate* unmanaged[Cdecl]<nint, nint, SDL.AudioSpec*, float*, int, void> cb,
        nint userdata);

    static unsafe bool SetStoppedCallbackCore(Track track, object? invoker)
    {
        var handle = invoker is null ? default : GCHandle.Alloc(invoker);

        var ok = SDL_SetTrackStoppedCallback(track.Value,
            invoker is null ? null : &TrackStoppedThunk,
            invoker is null ? 0 : GCHandle.ToIntPtr(handle)) != 0;

        if (ok) CommitCallback(trackStoppedCallbacks, track.Value, handle, invoker is not null);
        else if (invoker is not null) handle.Free();

        return ok;
    }

    static unsafe bool SetMixCallbackCore(Dictionary<nint, GCHandle> registry,
        nint owner,
        object? invoker,
        delegate* managed<nint, delegate* unmanaged[Cdecl]<nint, nint, SDL.AudioSpec*, float*, int, void>, nint, byte>
            register)
    {
        var handle = invoker is null ? default : GCHandle.Alloc(invoker);

        var ok = register(owner, invoker is null ? null : &MixThunk, invoker is null ? 0 : GCHandle.ToIntPtr(handle)) != 0;

        if (ok) CommitCallback(registry, owner, handle, invoker is not null);
        else if (invoker is not null) handle.Free();

        return ok;
    }

    /// <code>extern SDL_DECLSPEC bool SDLCALL MIX_SetTrackStoppedCallback(MIX_Track *track, MIX_TrackStoppedCallback cb, void *userdata);</code>
    /// <summary>
    /// Set (or with null, clear) a callback that fires when the track finishes playback. The delegate
    /// is rooted by the wrapper until replaced, cleared, or the track is destroyed via
    /// <see cref="DestroyTrack"/>.
    /// </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    public static bool SetTrackStoppedCallback(Track track, TrackStoppedCallback? callback)
        => SetStoppedCallbackCore(track, callback is null ? null : new PlainStoppedInvoker(callback));

    /// <summary> Context-carrying variant of <see cref="SetTrackStoppedCallback(Track, TrackStoppedCallback?)"/> (no boxing, no closure). </summary>
    public static bool SetTrackStoppedCallback<T>(Track track, TrackStoppedCallback<T> callback, T state)
    {
        ArgumentNullException.ThrowIfNull(callback);
        return SetStoppedCallbackCore(track, new StatefulStoppedInvoker<T>(callback, state));
    }

    /// <code>extern SDL_DECLSPEC bool SDLCALL MIX_SetTrackRawCallback(MIX_Track *track, MIX_TrackMixCallback cb, void *userdata);</code>
    /// <summary>
    /// Set (or with null, clear) a callback that observes/mutates the track's decoded audio before
    /// track transformations. Runs on the mixing thread; same rooting rules as
    /// <see cref="SetTrackStoppedCallback(Track, TrackStoppedCallback?)"/>.
    /// </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    public static unsafe bool SetTrackRawCallback(Track track, TrackMixCallback? callback)
        => SetMixCallbackCore(trackRawCallbacks,
            track.Value,
            callback is null ? null : new PlainTrackMixInvoker(callback),
            &SDL_SetTrackRawCallback);

    /// <summary> Context-carrying variant of <see cref="SetTrackRawCallback(Track, TrackMixCallback?)"/> (no boxing, no closure). </summary>
    public static unsafe bool SetTrackRawCallback<T>(Track track, TrackMixCallback<T> callback, T state)
    {
        ArgumentNullException.ThrowIfNull(callback);
        return SetMixCallbackCore(trackRawCallbacks,
            track.Value,
            new StatefulTrackMixInvoker<T>(callback, state),
            &SDL_SetTrackRawCallback);
    }

    /// <code>extern SDL_DECLSPEC bool SDLCALL MIX_SetTrackCookedCallback(MIX_Track *track, MIX_TrackMixCallback cb, void *userdata);</code>
    /// <summary>
    /// Set (or with null, clear) a callback that observes/mutates the track's audio after all track
    /// transformations, just before it joins the mix. Runs on the mixing thread.
    /// </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    public static unsafe bool SetTrackCookedCallback(Track track, TrackMixCallback? callback)
        => SetMixCallbackCore(trackCookedCallbacks,
            track.Value,
            callback is null ? null : new PlainTrackMixInvoker(callback),
            &SDL_SetTrackCookedCallback);

    /// <summary> Context-carrying variant of <see cref="SetTrackCookedCallback(Track, TrackMixCallback?)"/> (no boxing, no closure). </summary>
    public static unsafe bool SetTrackCookedCallback<T>(Track track, TrackMixCallback<T> callback, T state)
    {
        ArgumentNullException.ThrowIfNull(callback);
        return SetMixCallbackCore(trackCookedCallbacks,
            track.Value,
            new StatefulTrackMixInvoker<T>(callback, state),
            &SDL_SetTrackCookedCallback);
    }

    /// <code>extern SDL_DECLSPEC bool SDLCALL MIX_SetGroupPostMixCallback(MIX_Group *group, MIX_GroupMixCallback cb, void *userdata);</code>
    /// <summary>
    /// Set (or with null, clear) a callback that observes/mutates a group's combined audio before it
    /// joins the master mix. Runs on the mixing thread; rooting released by <see cref="DestroyGroup"/>.
    /// </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    public static unsafe bool SetGroupPostMixCallback(Group group, GroupMixCallback? callback)
        => SetMixCallbackCore(groupMixCallbacks,
            group.Value,
            callback is null ? null : new PlainGroupMixInvoker(callback),
            &SDL_SetGroupPostMixCallback);

    /// <summary> Context-carrying variant of <see cref="SetGroupPostMixCallback(Group, GroupMixCallback?)"/> (no boxing, no closure). </summary>
    public static unsafe bool SetGroupPostMixCallback<T>(Group group, GroupMixCallback<T> callback, T state)
    {
        ArgumentNullException.ThrowIfNull(callback);
        return SetMixCallbackCore(groupMixCallbacks,
            group.Value,
            new StatefulGroupMixInvoker<T>(callback, state),
            &SDL_SetGroupPostMixCallback);
    }

    /// <code>extern SDL_DECLSPEC bool SDLCALL MIX_SetPostMixCallback(MIX_Mixer *mixer, MIX_PostMixCallback cb, void *userdata);</code>
    /// <summary>
    /// Set (or with null, clear) a callback that observes/mutates the final mix before it reaches the
    /// device. Runs on the mixing thread; rooting released by <see cref="DestroyMixer"/>.
    /// </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    public static unsafe bool SetPostMixCallback(Mixer mixer, PostMixCallback? callback)
        => SetMixCallbackCore(mixerPostMixCallbacks,
            mixer.Value,
            callback is null ? null : new PlainPostMixInvoker(callback),
            &SDL_SetPostMixCallback);

    /// <summary> Context-carrying variant of <see cref="SetPostMixCallback(Mixer, PostMixCallback?)"/> (no boxing, no closure). </summary>
    public static unsafe bool SetPostMixCallback<T>(Mixer mixer, PostMixCallback<T> callback, T state)
    {
        ArgumentNullException.ThrowIfNull(callback);
        return SetMixCallbackCore(mixerPostMixCallbacks,
            mixer.Value,
            new StatefulPostMixInvoker<T>(callback, state),
            &SDL_SetPostMixCallback);
    }

    #endregion

    #region Offline generation and decoding

    [LibraryImport(MixerLibrary, EntryPoint = "MIX_Generate"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial int SDL_Generate(Mixer mixer, byte* buffer, int buflen);

    /// <code>extern SDL_DECLSPEC int SDLCALL MIX_Generate(MIX_Mixer *mixer, void *buffer, int buflen);</code>
    /// <summary>
    /// Run the mixer (created with <see cref="CreateMixer"/>, not a device mixer) and fill
    /// <paramref name="buffer"/> with mixed audio in the mixer's format. Returns the bytes generated,
    /// or -1 on error.
    /// </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    public static unsafe int Generate(Mixer mixer, Span<byte> buffer)
    {
        fixed (byte* bufferPtr = buffer) return SDL_Generate(mixer, bufferPtr, buffer.Length);
    }

    /// <summary> Float convenience over <see cref="Generate(Mixer, Span{byte})"/>; returns SAMPLES generated, or -1 on error. </summary>
    public static int Generate(Mixer mixer, Span<float> buffer)
    {
        var bytes = Generate(mixer, MemoryMarshal.AsBytes(buffer));
        return bytes < 0 ? bytes : bytes / sizeof(float);
    }

    [LibraryImport(MixerLibrary, EntryPoint = "MIX_CreateAudioDecoder"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial AudioDecoder SDL_CreateAudioDecoder(byte* path, PropertiesID props);

    /// <code>extern SDL_DECLSPEC MIX_AudioDecoder * SDLCALL MIX_CreateAudioDecoder(const char *path, SDL_PropertiesID props);</code>
    /// <summary> Create a decode-only object for an audio file (no mixer involved). </summary>
    /// <returns> a decoder handle, or 0 on failure; call <see cref="SDL.GetError"/> for more information. </returns>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    public static unsafe AudioDecoder CreateAudioDecoder(scoped ReadOnlySpan<char> path, PropertiesID props = default)
    {
        var byteCount = Encoding.UTF8.GetByteCount(path) + 1;
        byte[]? rented = null;
        scoped Span<byte> utf8 = byteCount <= 512 ?
            stackalloc byte[byteCount] :
            (rented = ArrayPool<byte>.Shared.Rent(byteCount)).AsSpan(0, byteCount);

        Encoding.UTF8.GetBytes(path, utf8);
        utf8[^1] = 0;

        try
        {
            fixed (byte* pathPtr = utf8) return SDL_CreateAudioDecoder(pathPtr, props);
        }
        finally
        {
            if (rented is not null) ArrayPool<byte>.Shared.Return(rented);
        }
    }

    /// <code>extern SDL_DECLSPEC MIX_AudioDecoder * SDLCALL MIX_CreateAudioDecoder_IO(SDL_IOStream *io, bool closeio, SDL_PropertiesID props);</code>
    /// <summary> Create a decode-only object reading from an SDL_IOStream. </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_CreateAudioDecoder_IO"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial AudioDecoder CreateAudioDecoderIO(IOStreamHandle io,
        [MarshalAs(UnmanagedType.I1)] bool closeio,
        PropertiesID props = default);

    /// <code>extern SDL_DECLSPEC void SDLCALL MIX_DestroyAudioDecoder(MIX_AudioDecoder *audiodecoder);</code>
    /// <summary> Destroy an audio decoder. </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_DestroyAudioDecoder"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void DestroyAudioDecoder(AudioDecoder audiodecoder);

    /// <code>extern SDL_DECLSPEC SDL_PropertiesID SDLCALL MIX_GetAudioDecoderProperties(MIX_AudioDecoder *audiodecoder);</code>
    /// <summary> Get the properties associated with an audio decoder (metadata lives here). </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_GetAudioDecoderProperties"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial PropertiesID GetAudioDecoderProperties(AudioDecoder audiodecoder);

    /// <code>extern SDL_DECLSPEC bool SDLCALL MIX_GetAudioDecoderFormat(MIX_AudioDecoder *audiodecoder, SDL_AudioSpec *spec);</code>
    /// <summary> Get the native format of the decoder's output. </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    [LibraryImport(MixerLibrary, EntryPoint = "MIX_GetAudioDecoderFormat"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool GetAudioDecoderFormat(AudioDecoder audiodecoder, out SDL.AudioSpec spec);

    [LibraryImport(MixerLibrary, EntryPoint = "MIX_DecodeAudio"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial int SDL_DecodeAudio(AudioDecoder audiodecoder, byte* buffer, int buflen, SDL.AudioSpec* spec);

    /// <code>extern SDL_DECLSPEC int SDLCALL MIX_DecodeAudio(MIX_AudioDecoder *audiodecoder, void *buffer, int buflen, const SDL_AudioSpec *spec);</code>
    /// <summary>
    /// Decode the next chunk of audio into <paramref name="buffer"/>, converted to
    /// <paramref name="spec"/>. Returns the bytes decoded (0 on end of data, -1 on error).
    /// </summary>
    /// <since> This function is available since SDL_mixer 3.0.0 </since>
    public static unsafe int DecodeAudio(AudioDecoder audiodecoder, Span<byte> buffer, in SDL.AudioSpec spec)
    {
        fixed (byte* bufferPtr = buffer)
        fixed (SDL.AudioSpec* specPtr = &spec)
            return SDL_DecodeAudio(audiodecoder, bufferPtr, buffer.Length, specPtr);
    }

    /// <summary> Decode in the decoder's native format (no conversion). </summary>
    /// <inheritdoc cref="DecodeAudio(nint, Span{byte}, in SDL.AudioSpec)"/>
    public static unsafe int DecodeAudio(AudioDecoder audiodecoder, Span<byte> buffer)
    {
        fixed (byte* bufferPtr = buffer) return SDL_DecodeAudio(audiodecoder, bufferPtr, buffer.Length, null);
    }

    /// <summary> Float convenience over <see cref="DecodeAudio(nint, Span{byte}, in SDL.AudioSpec)"/>; returns SAMPLES decoded. </summary>
    public static int DecodeAudio(AudioDecoder audiodecoder, Span<float> buffer, in SDL.AudioSpec spec)
    {
        var bytes = DecodeAudio(audiodecoder, MemoryMarshal.AsBytes(buffer), in spec);
        return bytes < 0 ? bytes : bytes / sizeof(float);
    }

    #endregion
}
