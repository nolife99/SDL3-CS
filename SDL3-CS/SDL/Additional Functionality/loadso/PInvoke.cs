﻿#region License

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
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_LoadObject"), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]),
     MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static partial nint LoadObjectNative(byte* sofile);

    [LibraryImport(SDLLibrary, EntryPoint = "SDL_LoadFunction"), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]),
     MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static partial FunctionPointer? LoadFunctionNative(nint handle, byte* name);

    /// <code>extern SDL_DECLSPEC SDL_SharedObject * SDLCALL SDL_LoadObject(const char *sofile);</code>
    /// <summary> Dynamically load a shared object. </summary>
    /// <param name="sofile"> a system-dependent name of the object file. </param>
    /// <returns>
    ///     an opaque pointer to the object handle or <c> null </c> on failure; call <see cref="GetError"/> for more
    ///     information.
    /// </returns>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="LoadFunction"/>
    /// <seealso cref="UnloadObject"/>
    public static nint LoadObject(ReadOnlySpan<char> sofile)
    {
        var byteCount = Encoding.UTF8.GetByteCount(sofile) + 1;

        byte[]? rented = null;
        var buffer = byteCount <= 512 ?
            stackalloc byte[byteCount] :
            (rented = ArrayPool<byte>.Shared.Rent(byteCount)).AsSpan(0, byteCount);

        try
        {
            var written = Encoding.UTF8.GetBytes(sofile, buffer);
            buffer[written] = 0; // null terminator

            fixed (byte* pBuffer = buffer) return LoadObjectNative(pBuffer);
        }
        finally
        {
            if (rented != null) ArrayPool<byte>.Shared.Return(rented);
        }
    }

    /// <code>extern SDL_DECLSPEC SDL_FunctionPointer SDLCALL SDL_LoadFunction(SDL_SharedObject *handle, const char *name);</code>
    /// <summary>
    ///     <para> Look up the address of the named function in a shared object. </para>
    ///     <para> This function pointer is no longer valid after calling <see cref="UnloadObject"/>. </para>
    ///     <para>
    ///         This function can only look up C function names. Other languages may have name mangling and intrinsic language
    ///         support that varies from compiler to compiler.
    ///     </para>
    ///     <para>
    ///         Make sure you declare your function pointers with the same calling convention as the actual library function.
    ///         Your code will crash mysteriously if you do not do this.
    ///     </para>
    ///     <para> If the requested function doesn't exist, <c> null </c> is returned. </para>
    /// </summary>
    /// <param name="handle"> a valid shared object handle returned by <see cref="LoadObject"/>. </param>
    /// <param name="name"> the name of the function to look up. </param>
    /// <returns> a pointer to the function or <c> null </c> on failure; call <see cref="GetError"/> for more information. </returns>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="LoadObject"/>
    public static FunctionPointer? LoadFunction(nint handle, ReadOnlySpan<char> name)
    {
        var byteCount = Encoding.UTF8.GetByteCount(name) + 1;

        byte[]? rented = null;
        var buffer = byteCount <= 512 ?
            stackalloc byte[byteCount] :
            (rented = ArrayPool<byte>.Shared.Rent(byteCount)).AsSpan(0, byteCount);

        try
        {
            var written = Encoding.UTF8.GetBytes(name, buffer);
            buffer[written] = 0; // null terminator

            fixed (byte* pBuffer = buffer) return LoadFunctionNative(handle, pBuffer);
        }
        finally
        {
            if (rented != null) ArrayPool<byte>.Shared.Return(rented);
        }
    }

    /// <code>extern SDL_DECLSPEC void SDLCALL SDL_UnloadObject(SDL_SharedObject *handle);</code>
    /// <summary>
    ///     <para> Unload a shared object from memory. </para>
    ///     <para> Note that any pointers from this object looked up through <see cref="LoadFunction"/> will no longer be valid. </para>
    /// </summary>
    /// <param name="handle"> a valid shared object handle returned by <see cref="LoadObject"/>. </param>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="LoadObject"/>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_UnloadObject"), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]),
     MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void UnloadObject(nint handle);
}