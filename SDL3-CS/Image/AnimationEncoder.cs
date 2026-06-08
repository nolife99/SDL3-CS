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

using System;
using System.Runtime.InteropServices;

public partial class Image
{
    /// <summary>
    ///     Opaque handle to a native <c> IMG_AnimationEncoder* </c>. Unlike <see cref="AnimationHandle"/> (whose
    ///     <c> IMG_Animation </c> has a public, layout-defined header), the encoder is a forward-declared type with no
    ///     exposed layout, so this handle is opaque and offers no field view.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct AnimationEncoderHandle(nint value) : IEquatable<AnimationEncoderHandle>
    {
        public readonly nint Value = value;

        public static AnimationEncoderHandle Null => default;
        public bool IsNull => Value == 0;

        public bool Equals(AnimationEncoderHandle other) => Value == other.Value;
        public override bool Equals(object obj) => obj is AnimationEncoderHandle other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => $"IMG_AnimationEncoder*(0x{Value:x})";

        public static bool operator ==(AnimationEncoderHandle left, AnimationEncoderHandle right) => left.Equals(right);
        public static bool operator !=(AnimationEncoderHandle left, AnimationEncoderHandle right) => !left.Equals(right);

        public static explicit operator nint(AnimationEncoderHandle handle) => handle.Value;
    }

    /// <summary>
    ///     Property names accepted by <see cref="CreateAnimationEncoderWithProperties"/>. Build a property group with the
    ///     SDL property API, set the keys you need, and pass its <see cref="PropertiesID"/> to create an encoder.
    /// </summary>
    public static class EncoderProps
    {
        /// <summary> The file to save to, used if <see cref="CreateIOStream"/> isn't set. </summary>
        public static readonly PropertyKey<string> CreateFilename = "SDL_image.animation_encoder.create.filename";

        /// <summary> An <c> SDL_IOStream </c> to write to, used if <see cref="CreateFilename"/> isn't set. </summary>
        public static readonly PropertyKey<nint> CreateIOStream = "SDL_image.animation_encoder.create.iostream";

        /// <summary> True to close the <see cref="CreateIOStream"/> when the encoder is closed. </summary>
        public static readonly PropertyKey<bool> CreateIOStreamAutoClose =
            "SDL_image.animation_encoder.create.iostream.autoclose";

        /// <summary> The output file type (e.g. "gif", "webp", "avif"); required when only <see cref="CreateIOStream"/> is set. </summary>
        public static readonly PropertyKey<string> CreateType = "SDL_image.animation_encoder.create.type";

        /// <summary> Compression quality. </summary>
        public static readonly PropertyKey<long> CreateQuality = "SDL_image.animation_encoder.create.quality";

        /// <summary> Frame-duration timebase numerator. </summary>
        public static readonly PropertyKey<long> CreateTimebaseNumerator =
            "SDL_image.animation_encoder.create.timebase.numerator";

        /// <summary> Frame-duration timebase denominator. </summary>
        public static readonly PropertyKey<long> CreateTimebaseDenominator =
            "SDL_image.animation_encoder.create.timebase.denominator";

        /// <summary> AVIF: maximum encoder threads. </summary>
        public static readonly PropertyKey<long> CreateAVIFMaxThreads =
            "SDL_image.animation_encoder.create.avif.max_threads";

        /// <summary> AVIF: keyframe interval. </summary>
        public static readonly PropertyKey<long> CreateAVIFKeyFrameInterval =
            "SDL_image.animation_encoder.create.avif.keyframe_interval";

        /// <summary> GIF: use a local color table (LUT). </summary>
        public static readonly PropertyKey<bool> CreateGIFUseLut = "SDL_image.animation_encoder.create.gif.use_lut";
    }
}
