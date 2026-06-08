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

// This file is part of the storybrew fork: SDL3_mixer (MIX_*) bindings, written against
// SDL_mixer release-3.2.4. Opaque native objects (MIX_Mixer, MIX_Audio, MIX_Track, MIX_Group,
// MIX_AudioDecoder) are represented as nint handles.

namespace SDL3;

using System;
using System.Runtime.InteropServices;

public static partial class MIX
{
    const string MixerLibrary = "SDL3_mixer";

    /// <summary> Opaque handle to a native <c> MIX_Mixer* </c>. </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Mixer(nint value) : IEquatable<Mixer>
    {
        public readonly nint Value = value;

        public static Mixer Null => default;
        public bool IsNull => Value == 0;

        public bool Equals(Mixer other) => Value == other.Value;
        public override bool Equals(object obj) => obj is Mixer other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => $"MIX_Mixer*(0x{Value:x})";

        public static bool operator ==(Mixer left, Mixer right) => left.Equals(right);
        public static bool operator !=(Mixer left, Mixer right) => !left.Equals(right);
    }

    /// <summary> Opaque handle to a native <c> MIX_Audio* </c>. </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Audio(nint value) : IEquatable<Audio>
    {
        public readonly nint Value = value;

        public static Audio Null => default;
        public bool IsNull => Value == 0;

        public bool Equals(Audio other) => Value == other.Value;
        public override bool Equals(object obj) => obj is Audio other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => $"MIX_Audio*(0x{Value:x})";

        public static bool operator ==(Audio left, Audio right) => left.Equals(right);
        public static bool operator !=(Audio left, Audio right) => !left.Equals(right);
    }

    /// <summary> Opaque handle to a native <c> MIX_Track* </c>. </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Track(nint value) : IEquatable<Track>
    {
        public readonly nint Value = value;

        public static Track Null => default;
        public bool IsNull => Value == 0;

        public bool Equals(Track other) => Value == other.Value;
        public override bool Equals(object obj) => obj is Track other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => $"MIX_Track*(0x{Value:x})";

        public static bool operator ==(Track left, Track right) => left.Equals(right);
        public static bool operator !=(Track left, Track right) => !left.Equals(right);
    }

    /// <summary> Opaque handle to a native <c> MIX_Group* </c>. </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Group(nint value) : IEquatable<Group>
    {
        public readonly nint Value = value;

        public static Group Null => default;
        public bool IsNull => Value == 0;

        public bool Equals(Group other) => Value == other.Value;
        public override bool Equals(object obj) => obj is Group other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => $"MIX_Group*(0x{Value:x})";

        public static bool operator ==(Group left, Group right) => left.Equals(right);
        public static bool operator !=(Group left, Group right) => !left.Equals(right);
    }

    /// <summary> Opaque handle to a native <c> MIX_AudioDecoder* </c>. </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct AudioDecoder(nint value) : IEquatable<AudioDecoder>
    {
        public readonly nint Value = value;

        public static AudioDecoder Null => default;
        public bool IsNull => Value == 0;

        public bool Equals(AudioDecoder other) => Value == other.Value;
        public override bool Equals(object obj) => obj is AudioDecoder other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => $"MIX_AudioDecoder*(0x{Value:x})";

        public static bool operator ==(AudioDecoder left, AudioDecoder right) => left.Equals(right);
        public static bool operator !=(AudioDecoder left, AudioDecoder right) => !left.Equals(right);
    }

    /// <summary> Rooting hook shared with the core bindings: ensure the resolver in <see cref="SDL"/> is installed. </summary>
    static MIX() => System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(SDL).TypeHandle);

    /// <code>typedef struct MIX_StereoGains { float left; float right; } MIX_StereoGains;</code>
    /// <summary> Left/right channel gains used by <see cref="SetTrackStereo(nint, in StereoGains)"/>. </summary>
    /// <since> This struct is available since SDL_mixer 3.0.0 </since>
    [StructLayout(LayoutKind.Sequential)]
    public struct StereoGains
    {
        /// <summary> left channel gain </summary>
        public float Left;

        /// <summary> right channel gain </summary>
        public float Right;
    }

    /// <code>typedef struct MIX_Point3D { float x; float y; float z; } MIX_Point3D;</code>
    /// <summary> A position in 3D space for spatialized tracks. </summary>
    /// <since> This struct is available since SDL_mixer 3.0.0 </since>
    [StructLayout(LayoutKind.Sequential)]
    public struct Point3D
    {
        /// <summary> X coordinate (negative left, positive right). </summary>
        public float X;

        /// <summary> Y coordinate (negative down, positive up). </summary>
        public float Y;

        /// <summary> Z coordinate (negative forward, positive back). </summary>
        public float Z;
    }

    /// <summary>
    ///     Typed property keys (MIX_PROP_*). Each carries its value type, so the
    ///     <see cref="SDL.SetProperty(PropertiesID, PropertyKey{nint}, nint)"/> /
    ///     <see cref="SDL.GetProperty(PropertiesID, PropertyKey{string}, string)"/> overloads pick the
    ///     right call and reject a wrong-typed value at compile time. Grouped by the operation that
    ///     accepts them; prefer the restricted <see cref="AudioLoadProperties"/> builder and the
    ///     <see cref="AudioMetadata"/> reader over poking these directly.
    /// </summary>
    public static class Props
    {
        public static readonly PropertyKey<long> MixerDevice = "SDL_mixer.mixer.device";

        // Audio load — accepted by MIX_LoadAudioWithProperties.
        public static readonly PropertyKey<nint> AudioLoadIOStream = "SDL_mixer.audio.load.iostream";
        public static readonly PropertyKey<bool> AudioLoadCloseIO = "SDL_mixer.audio.load.closeio";
        public static readonly PropertyKey<bool> AudioLoadPredecode = "SDL_mixer.audio.load.predecode";
        public static readonly PropertyKey<nint> AudioLoadPreferredMixer = "SDL_mixer.audio.load.preferred_mixer";
        public static readonly PropertyKey<bool> AudioLoadSkipMetadataTags = "SDL_mixer.audio.load.skip_metadata_tags";
        public static readonly PropertyKey<bool> AudioLoadIgnoreLoops = "SDL_mixer.audio.load.ignore_loops";
        public static readonly PropertyKey<string> AudioDecoder = "SDL_mixer.audio.decoder";

        /// <summary>
        ///     (storybrew fork) Internal-but-stable property (verified against the release-3.2.4
        ///     sources): skips precaching the compressed file into RAM, decoding from the kept-open
        ///     IOStream on demand instead. Only one track/decoder may consume such an audio at a time.
        /// </summary>
        public static readonly PropertyKey<bool> AudioLoadOnDemand = "SDL_mixer.audio.load.ondemand";

        // Metadata — exposed by MIX_GetAudioProperties (read via the AudioMetadata reader).
        public static readonly PropertyKey<string> MetadataTitle = "SDL_mixer.metadata.title";
        public static readonly PropertyKey<string> MetadataArtist = "SDL_mixer.metadata.artist";
        public static readonly PropertyKey<string> MetadataAlbum = "SDL_mixer.metadata.album";
        public static readonly PropertyKey<string> MetadataCopyright = "SDL_mixer.metadata.copyright";
        public static readonly PropertyKey<long> MetadataTrack = "SDL_mixer.metadata.track";
        public static readonly PropertyKey<long> MetadataTotalTracks = "SDL_mixer.metadata.total_tracks";
        public static readonly PropertyKey<long> MetadataYear = "SDL_mixer.metadata.year";
        public static readonly PropertyKey<long> MetadataDurationFrames = "SDL_mixer.metadata.duration_frames";
        public static readonly PropertyKey<bool> MetadataDurationInfinite = "SDL_mixer.metadata.duration_infinite";

        // Play options — accepted by MIX_PlayTrack / MIX_PlayTag.
        public static readonly PropertyKey<long> PlayLoops = "SDL_mixer.play.loops";
        public static readonly PropertyKey<long> PlayMaxFrame = "SDL_mixer.play.max_frame";
        public static readonly PropertyKey<long> PlayMaxMilliseconds = "SDL_mixer.play.max_milliseconds";
        public static readonly PropertyKey<long> PlayStartFrame = "SDL_mixer.play.start_frame";
        public static readonly PropertyKey<long> PlayStartMillisecond = "SDL_mixer.play.start_millisecond";
        public static readonly PropertyKey<long> PlayStartOrder = "SDL_mixer.play.start_order";
        public static readonly PropertyKey<long> PlayLoopStartFrame = "SDL_mixer.play.loop_start_frame";
        public static readonly PropertyKey<long> PlayLoopStartMillisecond = "SDL_mixer.play.loop_start_millisecond";
        public static readonly PropertyKey<long> PlayFadeInFrames = "SDL_mixer.play.fade_in_frames";
        public static readonly PropertyKey<long> PlayFadeInMilliseconds = "SDL_mixer.play.fade_in_milliseconds";
        public static readonly PropertyKey<float> PlayFadeInStartGain = "SDL_mixer.play.fade_in_start_gain";
        public static readonly PropertyKey<long> PlayAppendSilenceFrames = "SDL_mixer.play.append_silence_frames";
        public static readonly PropertyKey<long> PlayAppendSilenceMilliseconds = "SDL_mixer.play.append_silence_milliseconds";
        public static readonly PropertyKey<bool> PlayHaltWhenExhausted = "SDL_mixer.play.halt_when_exhausted";
    }

    /// <summary>
    ///     A restricted, fluent view over a property set that <see cref="LoadAudioWithProperties"/>
    ///     accepts — only the documented audio-load properties are reachable, so an irrelevant key
    ///     cannot be queued. It is a ref struct so it cannot escape the configuration scope; obtain one
    ///     through <see cref="LoadAudio(Mixer, AudioLoadConfigurator)"/>, which owns the lifetime.
    /// </summary>
    public readonly ref struct AudioLoadProperties
    {
        readonly PropertiesID props;

        internal AudioLoadProperties(PropertiesID props) => this.props = props;

        /// <summary> Decode from this (seekable) IOStream; <paramref name="closeWhenDone"/> hands its ownership to SDL_mixer. </summary>
        public AudioLoadProperties IOStream(IOStreamHandle io, bool closeWhenDone = true)
        {
            SDL.SetProperty(props, Props.AudioLoadIOStream, io.Value);
            SDL.SetProperty(props, Props.AudioLoadCloseIO, closeWhenDone);
            return this;
        }

        /// <summary> Decode everything up front (true) versus on demand while playing (false). </summary>
        public AudioLoadProperties Predecode(bool predecode = true)
        {
            SDL.SetProperty(props, Props.AudioLoadPredecode, predecode);
            return this;
        }

        /// <summary> Stream from the kept-open IOStream without precaching the compressed file into RAM. </summary>
        public AudioLoadProperties OnDemand(bool onDemand = true)
        {
            SDL.SetProperty(props, Props.AudioLoadOnDemand, onDemand);
            return this;
        }

        /// <summary> Hint the mixer this audio will play on, so it can prepare a matching format. </summary>
        public AudioLoadProperties PreferredMixer(Mixer mixer)
        {
            SDL.SetProperty(props, Props.AudioLoadPreferredMixer, mixer.Value);
            return this;
        }

        public AudioLoadProperties SkipMetadataTags(bool skip = true)
        {
            SDL.SetProperty(props, Props.AudioLoadSkipMetadataTags, skip);
            return this;
        }

        public AudioLoadProperties IgnoreLoops(bool ignore = true)
        {
            SDL.SetProperty(props, Props.AudioLoadIgnoreLoops, ignore);
            return this;
        }
    }

    /// <summary> Configures a restricted <see cref="AudioLoadProperties"/> set in place. </summary>
    public delegate void AudioLoadConfigurator(AudioLoadProperties properties);

    /// <summary>
    ///     Load a MIX_Audio while setting only the audio-load properties that apply, through a
    ///     restricted builder whose property set is created and destroyed for you.
    /// </summary>
    /// <returns> an audio handle, or 0 on failure; call <see cref="SDL.GetError"/> for more information. </returns>
    public static Audio LoadAudio(Mixer mixer, AudioLoadConfigurator configure)
    {
        System.ArgumentNullException.ThrowIfNull(configure);

        var props = SDL.CreateProperties();
        if (props.IsNull) return Audio.Null;

        try
        {
            configure(new AudioLoadProperties(props));
            return LoadAudioWithProperties(props);
        }
        finally
        {
            SDL.DestroyProperties(props);
        }
    }

    /// <summary>
    ///     A typed, read-only view over a MIX_Audio's metadata property set (from
    ///     <see cref="GetAudioProperties"/>): only the documented metadata keys are exposed, each with
    ///     its correct type, so reads can't pick the wrong getter or invent a key. Obtain via
    ///     <see cref="GetMetadata"/>.
    /// </summary>
    public readonly struct AudioMetadata
    {
        readonly PropertiesID props;

        internal AudioMetadata(PropertiesID props) => this.props = props;

        public bool IsValid => !props.IsNull;

        public string Title => SDL.GetProperty(props, Props.MetadataTitle);
        public string Artist => SDL.GetProperty(props, Props.MetadataArtist);
        public string Album => SDL.GetProperty(props, Props.MetadataAlbum);
        public string Copyright => SDL.GetProperty(props, Props.MetadataCopyright);
        public long TrackNumber => SDL.GetProperty(props, Props.MetadataTrack);
        public long TotalTracks => SDL.GetProperty(props, Props.MetadataTotalTracks);
        public long Year => SDL.GetProperty(props, Props.MetadataYear);

        /// <summary> Total length in sample frames (0 when unknown; check <see cref="DurationInfinite"/>). </summary>
        public long DurationFrames => SDL.GetProperty(props, Props.MetadataDurationFrames);

        public bool DurationInfinite => SDL.GetProperty(props, Props.MetadataDurationInfinite);
    }

    /// <summary> Read a MIX_Audio's metadata through the typed <see cref="AudioMetadata"/> view. </summary>
    public static AudioMetadata GetMetadata(Audio audio) => new(GetAudioProperties(audio));
}
