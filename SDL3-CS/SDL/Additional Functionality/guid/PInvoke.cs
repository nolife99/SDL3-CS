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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

public static unsafe partial class SDL
{
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_GUIDToString"), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]),
     MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static partial void GUIDToStringNative(GUID guid,
        byte* pszGUID, // unmanaged UTF-8 buffer
        int cbGUID);

    [LibraryImport(SDLLibrary, EntryPoint = "SDL_StringToGUID"), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]),
     MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static partial GUID StringToGUIDNative(byte* pchGUID);

    /// <code>extern SDL_DECLSPEC void SDLCALL SDL_GUIDToString(SDL_GUID guid, char *pszGUID, int cbGUID);</code>
    /// <summary> Get an ASCII string representation for a given <see cref="GUID"/>. </summary>
    /// <param name="guid"> the <see cref="GUID"/> you wish to convert to string. </param>
    /// <param name="pszGUID"> buffer in which to write the ASCII string, should be at least 33 bytes. </param>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="StringToGUID"/>
    public static void GUIDToString(GUID guid, Span<char> pszGUID)
    {
        // SDL docs: needs at least 33 bytes
        Span<byte> buffer = stackalloc byte[33];

        fixed (byte* pBuffer = buffer) GUIDToStringNative(guid, pBuffer, buffer.Length);

        Encoding.UTF8.GetChars(buffer[..buffer.IndexOf((byte)0)], pszGUID);
    }

    /// <code>extern SDL_DECLSPEC SDL_GUID SDLCALL SDL_StringToGUID(const char *pchGUID);</code>
    /// <summary>
    ///     <para> Convert a GUID string into a <see cref="GUID"/> structure. </para>
    ///     <para>
    ///         Performs no error checking. If this function is given a string containing an invalid GUID, the function will
    ///         silently succeed, but the GUID generated will not be useful.
    ///     </para>
    /// </summary>
    /// <param name="guidString"> string containing an ASCII representation of a GUID. </param>
    /// <returns> a <see cref="GUID"/> structure. </returns>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="GUIDToString"/>
    public static GUID StringToGUID(ReadOnlySpan<char> guidString)
    {
        // Worst case UTF-8 byte count + 1 for null terminator
        var byteCount = Encoding.UTF8.GetByteCount(guidString) + 1;
        byte[]? rented = null;
        var buffer = byteCount <= 512 ?
            stackalloc byte[byteCount] :
            (rented = ArrayPool<byte>.Shared.Rent(byteCount)).AsSpan(0, byteCount);

        try
        {
            var written = Encoding.UTF8.GetBytes(guidString, buffer);
            buffer[written] = 0; // null terminator

            fixed (byte* pBuffer = buffer) return StringToGUIDNative(pBuffer);
        }
        finally
        {
            if (rented != null) ArrayPool<byte>.Shared.Return(rented);
        }
    }
}