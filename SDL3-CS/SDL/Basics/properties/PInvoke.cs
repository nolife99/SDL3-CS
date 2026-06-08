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
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

public static partial class SDL
{
    /// <code>extern SDL_DECLSPEC SDL_PropertiesID SDLCALL SDL_GetGlobalProperties(void);</code>
    /// <summary> Get the global SDL properties. </summary>
    /// <returns> a valid property ID on success or <c> 0 </c> on failure; call <see cref="GetError"/> for more information. </returns>
    /// <since> This function is available since SDL 3.2.0 </since>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_GetGlobalProperties"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial PropertiesID GetGlobalProperties();

    /// <code>extern SDL_DECLSPEC SDL_PropertiesID SDLCALL SDL_CreateProperties(void);</code>
    /// <summary>
    /// <para> Create a group of properties. </para>
    /// <para> All properties are automatically destroyed when <see cref="Quit"/> is called. </para>
    /// </summary>
    /// <returns> an ID for a new group of properties, or <c> 0 </c> on failure; call <see cref="GetError"/> for more information. </returns>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="DestroyProperties"/>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_CreateProperties"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial PropertiesID CreateProperties();

    /// <code>extern SDL_DECLSPEC bool SDLCALL SDL_CopyProperties(SDL_PropertiesID src, SDL_PropertiesID dst);</code>
    /// <summary>
    /// <para> Copy a group of properties. </para>
    /// <para>
    /// Copy all the properties from one group of properties to another, with the exception of properties requiring cleanup
    /// (set using <see cref="SetPointerPropertyWithCleanup"/>), which will not be copied. Any property that already exists on `dst`
    /// will be overwritten.
    /// </para>
    /// </summary>
    /// <param name="src"> the properties to copy. </param>
    /// <param name="dst"> the destination properties. </param>
    /// <returns> <c> true </c> on success or <c> false </c> on failure; call <see cref="GetError"/> for more information. </returns>
    /// <threadsafety>
    /// It is safe to call this function from any thread. This function acquires simultaneous mutex locks on both the
    /// source and destination property sets.
    /// </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_CopyProperties"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool CopyProperties(PropertiesID src, PropertiesID dst);

    /// <code>extern SDL_DECLSPEC bool SDLCALL SDL_LockProperties(SDL_PropertiesID props);</code>
    /// <summary>
    /// <para> Lock a group of properties. </para>
    /// <para>
    /// Obtain a multi-threaded lock for these properties. Other threads will wait while trying to lock these properties
    /// until they are unlocked. Properties must be unlocked before they are destroyed.
    /// </para>
    /// <para>
    /// The lock is automatically taken when setting individual properties, this function is only needed when you want to set
    /// several properties atomically or want to guarantee that properties being queried aren't freed in another thread.
    /// </para>
    /// </summary>
    /// <param name="props"> the properties to lock. </param>
    /// <returns> <c> true </c> on success or <c> false </c> on failure; call <see cref="GetError"/> for more information. </returns>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="UnlockProperties"/>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_LockProperties"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool LockProperties(PropertiesID props);

    /// <code>extern SDL_DECLSPEC void SDLCALL SDL_UnlockProperties(SDL_PropertiesID props);</code>
    /// <summary> Unlock a group of properties. </summary>
    /// <param name="props"> the properties to unlock. </param>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="LockProperties"/>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_UnlockProperties"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void UnlockProperties(PropertiesID props);

    // ReSharper disable once InvalidXmlDocComment
    /// <code>extern SDL_DECLSPEC bool SDLCALL SDL_SetPointerPropertyWithCleanup(SDL_PropertiesID props, const char *name, void *value, SDL_CleanupPropertyCallback cleanup, void *userdata);</code>
    /// <summary>
    /// <para> Set a pointer property in a group of properties with a cleanup function that is called when the property is deleted. </para>
    /// <para> The cleanup function is also called if setting the property fails for any reason. </para>
    /// <para>
    /// For simply setting basic data types, like numbers, bools, or strings, use <see cref="SetNumberProperty"/>,
    /// <see cref="SetBooleanProperty"/>, or <see cref="SetStringProperty"/> instead, as those functions will handle cleanup on your
    /// behalf. This function is only for more complex, custom data.
    /// </para>
    /// </summary>
    /// <param name="props"> the properties to modify. </param>
    /// <param name="name"> the name of the property to modify. </param>
    /// <param name="value"> the new value of the property, or <c> null </c> to delete the property. </param>
    /// <param name="cleanup"> the function to call when this property is deleted, or <c> null </c> if no cleanup is necessary. </param>
    /// <param name="userdata"> a pointer that is passed to the cleanup function. </param>
    /// <returns> true on success or false on failure; call <see cref="GetError"/> for more information. </returns>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="GetPointerProperty"/>
    /// <seealso cref="SetPointerProperty"/>
    /// <seealso cref="CleanupPropertyCallback"/>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_SetPointerPropertyWithCleanup"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.I1)]
    private static unsafe partial bool SDL_SetPointerPropertyWithCleanup(PropertiesID props,
        byte* name,
        nint value,
        delegate* unmanaged[Cdecl]<nint, nint, void> cleanup,
        nint userdata);

    abstract class PropertyCleanup
    {
        public abstract void Invoke(nint value);
    }

    sealed class PlainPropertyCleanup(CleanupPropertyCallback callback) : PropertyCleanup
    {
        public override void Invoke(nint value) => callback(value);
    }

    // The typed state lives in this generic class field: no boxing, no closure capture.
    sealed class StatefulPropertyCleanup<T>(CleanupPropertyCallback<T> callback, T state) : PropertyCleanup
    {
        public override void Invoke(nint value) => callback(state, value);
    }

    /// <summary>One-shot: SDL invokes the cleanup exactly once (deletion, replacement, or failed set).</summary>
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static void PropertyCleanupThunk(nint userdata, nint value)
    {
        var handle = GCHandle.FromIntPtr(userdata);
        try
        {
            ((PropertyCleanup)handle.Target!).Invoke(value);
        }
        catch (Exception exception)
        {
            ReportCallbackException(exception);
        }
        finally
        {
            handle.Free();
        }
    }

    static unsafe bool SetPointerPropertyWithCleanupCore(PropertiesID props,
        scoped ReadOnlySpan<char> name,
        nint value,
        PropertyCleanup cleanup)
    {
        var byteCount = Encoding.UTF8.GetByteCount(name) + 1;
        byte[]? rented = null;
        scoped var utf8 = byteCount <= 512 ?
            stackalloc byte[byteCount] :
            (rented = ArrayPool<byte>.Shared.Rent(byteCount)).AsSpan(0, byteCount);

        Encoding.UTF8.GetBytes(name, utf8);
        utf8[^1] = 0;

        try
        {
            fixed (byte* namePtr = utf8)
                return SDL_SetPointerPropertyWithCleanup(props,
                    namePtr,
                    value,
                    &PropertyCleanupThunk,
                    GCHandle.ToIntPtr(GCHandle.Alloc(cleanup)));
        }
        finally
        {
            if (rented is not null) ArrayPool<byte>.Shared.Return(rented);
        }
    }

    /// <summary>
    ///     See the native documentation above. The cleanup delegate is rooted until SDL invokes it, which
    ///     happens exactly once — when the property is deleted or replaced, or during this call on failure.
    /// </summary>
    public static bool SetPointerPropertyWithCleanup(PropertiesID props,
        scoped ReadOnlySpan<char> name,
        nint value,
        CleanupPropertyCallback cleanup)
    {
        ArgumentNullException.ThrowIfNull(cleanup);
        return SetPointerPropertyWithCleanupCore(props, name, value, new PlainPropertyCleanup(cleanup));
    }

    /// <summary>
    ///     Stateful variant: <paramref name="state"/> reaches the cleanup without boxing or closure
    ///     captures — use a static lambda.
    /// </summary>
    public static bool SetPointerPropertyWithCleanup<T>(PropertiesID props,
        scoped ReadOnlySpan<char> name,
        nint value,
        CleanupPropertyCallback<T> cleanup,
        T state)
    {
        ArgumentNullException.ThrowIfNull(cleanup);
        return SetPointerPropertyWithCleanupCore(props, name, value, new StatefulPropertyCleanup<T>(cleanup, state));
    }

    /// <code>extern SDL_DECLSPEC bool SDLCALL SDL_SetPointerProperty(SDL_PropertiesID props, const char *name, void *value);</code>
    /// <summary>
    /// <para> Set a pointer property in a group of properties. </para>
    /// </summary>
    /// <param name="props"> the properties to modify. </param>
    /// <param name="name"> the name of the property to modify. </param>
    /// <param name="value"> the new value of the property, or <c> null </c> to delete the property. </param>
    /// <returns> <c> true </c> on success or <c> false </c> on failure; call <see cref="GetError"/> for more information. </returns>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="GetPointerProperty"/>
    /// <seealso cref="HasProperty"/>
    /// <seealso cref="SetBooleanProperty"/>
    /// <seealso cref="SetFloatProperty"/>
    /// <seealso cref="SetNumberProperty"/>
    /// <seealso cref="SetPointerPropertyWithCleanup"/>
    /// <seealso cref="SetStringProperty"/>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_SetPointerProperty"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool SetPointerProperty(PropertiesID props,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string name,
        nint value);

    /// <code>extern SDL_DECLSPEC bool SDLCALL SDL_SetStringProperty(SDL_PropertiesID props, const char *name, const char *value);</code>
    /// <summary>
    /// <para> Set a string property in a group of properties. </para>
    /// <para> This function makes a copy of the string; the caller does not have to preserve the data after this call completes. </para>
    /// </summary>
    /// <param name="props"> the properties to modify. </param>
    /// <param name="name"> the name of the property to modify. </param>
    /// <param name="value"> the new value of the property, or <c> null </c> to delete the property. </param>
    /// <returns> <c> true </c> on success or <c> false </c> on failure; call <see cref="GetError"/> for more information. </returns>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="GetStringProperty"/>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_SetStringProperty"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool SetStringProperty(PropertiesID props,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string name,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string? value);

    /// <code>extern SDL_DECLSPEC bool SDLCALL SDL_SetNumberProperty(SDL_PropertiesID props, const char *name, Sint64 value);</code>
    /// <summary> Set an integer property in a group of properties. </summary>
    /// <param name="props"> the properties to modify. </param>
    /// <param name="name"> the name of the property to modify. </param>
    /// <param name="value"> the new value of the property. </param>
    /// <returns> <c> true </c> on success or <c> false </c> on failure; call <see cref="GetError"/> for more information. </returns>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="GetNumberProperty"/>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_SetNumberProperty"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool SetNumberProperty(PropertiesID props,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string name,
        long value);

    /// <code>extern SDL_DECLSPEC bool SDLCALL SDL_SetFloatProperty(SDL_PropertiesID props, const char *name, float value);</code>
    /// <summary> Set a floating point property in a group of properties. </summary>
    /// <param name="props"> the properties to modify. </param>
    /// <param name="name"> the name of the property to modify. </param>
    /// <param name="value"> the new value of the property. </param>
    /// <returns> <c> true </c> on success or <c> false </c> on failure; call <see cref="GetError"/> for more information. </returns>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="GetFloatProperty"/>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_SetFloatProperty"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool SetFloatProperty(PropertiesID props,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string name,
        float value);

    /// <code>extern SDL_DECLSPEC bool SDLCALL SDL_SetBooleanProperty(SDL_PropertiesID props, const char *name, bool value);</code>
    /// <summary> Set a boolean property in a group of properties. </summary>
    /// <param name="props"> the properties to modify. </param>
    /// <param name="name"> the name of the property to modify. </param>
    /// <param name="value"> the new value of the property. </param>
    /// <returns> <c> true </c> on success or <c> false </c> on failure; call <see cref="GetError"/> for more information. </returns>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="GetBooleanProperty"/>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_SetBooleanProperty"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool SetBooleanProperty(PropertiesID props,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string name,
        [MarshalAs(UnmanagedType.I1)] bool value);

    /// <code>extern SDL_DECLSPEC bool SDLCALL SDL_HasProperty(SDL_PropertiesID props, const char *name);</code>
    /// <summary> Return whether a property exists in a group of properties. </summary>
    /// <param name="props"> the properties to query. </param>
    /// <param name="name"> the name of the property to query. </param>
    /// <returns> <c> true </c> if the property exists, or <c> false </c> if it doesn't. </returns>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="GetPropertyType"/>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_HasProperty"), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool HasProperty(PropertiesID props, [MarshalAs(UnmanagedType.LPUTF8Str)] string name);

    /// <code>extern SDL_DECLSPEC SDL_PropertyType SDLCALL SDL_GetPropertyType(SDL_PropertiesID props, const char *name);</code>
    /// <summary> Get the type of a property in a group of properties. </summary>
    /// <param name="props"> the properties to query. </param>
    /// <param name="name"> the name of the property to query. </param>
    /// <returns> the type of the property, or <see cref="PropertyType.Invalid"/> if it is not set. </returns>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="HasProperty"/>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_GetPropertyType"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial PropertyType GetPropertyType(PropertiesID props, [MarshalAs(UnmanagedType.LPUTF8Str)] string name);

    /// <code>extern SDL_DECLSPEC void * SDLCALL SDL_GetPointerProperty(SDL_PropertiesID props, const char *name, void *default_value);</code>
    /// <summary>
    /// <para> Get a pointer property from a group of properties. </para>
    /// <para>
    /// By convention, the names of properties that SDL exposes on objects will start with <c> "SDL." </c>, and properties
    /// that SDL uses internally will start with <c> "SDL.internal." </c>. These should be considered read-only and should not be
    /// modified by applications.
    /// </para>
    /// </summary>
    /// <param name="props"> the properties to query. </param>
    /// <param name="name"> the name of the property to query. </param>
    /// <param name="defaultValue"> the default value of the property. </param>
    /// <returns> the value of the property, or <c> 'default_value' </c> if it is not set or not a pointer property. </returns>
    /// <threadsafety>
    /// It is safe to call this function from any thread, although the data returned is not protected and could
    /// potentially be freed if you call <see cref="SetPointerProperty"/> or <see cref="ClearProperty"/> on these properties from
    /// another thread. If you need to avoid this, use <see cref="LockProperties"/> and <see cref="UnlockProperties"/>.
    /// </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="GetBooleanProperty"/>
    /// <seealso cref="GetFloatProperty"/>
    /// <seealso cref="GetNumberProperty"/>
    /// <seealso cref="GetPropertyType"/>
    /// <seealso cref="GetStringProperty"/>
    /// <seealso cref="HasProperty"/>
    /// <seealso cref="SetPointerProperty"/>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_GetPointerProperty"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial nint GetPointerProperty(PropertiesID props,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string name,
        nint defaultValue);

    [LibraryImport(SDLLibrary, EntryPoint = "SDL_GetStringProperty"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial nint SDL_GetStringProperty(PropertiesID props,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string name,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string defaultValue);

    /// <code>extern SDL_DECLSPEC const char * SDLCALL SDL_GetStringProperty(SDL_PropertiesID props, const char *name, const char *default_value);</code>
    /// <summary> Get a string property from a group of properties. </summary>
    /// <param name="props"> the properties to query. </param>
    /// <param name="name"> the name of the property to query. </param>
    /// <param name="defaultValue"> the default value of the property. </param>
    /// <returns> the value of the property, or <c> `default_value` </c> if it is not set or not a string property. </returns>
    /// <threadsafety>
    /// It is safe to call this function from any thread, although the data returned is not protected and could
    /// potentially be freed if you call <see cref="SetStringProperty"/> or <see cref="ClearProperty"/> on these properties from
    /// another thread. If you need to avoid this, use <see cref="LockProperties"/> and <see cref="UnlockProperties"/>.
    /// </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="GetPropertyType"/>
    /// <seealso cref="HasProperty"/>
    /// <seealso cref="SetStringProperty"/>
    public static string GetStringProperty(PropertiesID props, string name, string defaultValue)
    {
        var value = SDL_GetStringProperty(props, name, defaultValue);
        return value == nint.Zero ? "" : Marshal.PtrToStringUTF8(value)!;
    }

    /// <code>extern SDL_DECLSPEC Sint64 SDLCALL SDL_GetNumberProperty(SDL_PropertiesID props, const char *name, Sint64 default_value);</code>
    /// <summary>
    /// <para> Get a number property from a group of properties. </para>
    /// <para> You can use <see cref="GetPropertyType"/> to query whether the property exists and is a number property. </para>
    /// </summary>
    /// <param name="props"> the properties to query. </param>
    /// <param name="name"> the name of the property to query. </param>
    /// <param name="defaultValue"> the default value of the property. </param>
    /// <returns> the value of the property, or <c> `default_value` </c> if it is not set or not a number property. </returns>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="GetPropertyType"/>
    /// <seealso cref="HasProperty"/>
    /// <seealso cref="SetNumberProperty"/>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_GetNumberProperty"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial long GetNumberProperty(PropertiesID props,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string name,
        long defaultValue);

    /// <code>extern SDL_DECLSPEC float SDLCALL SDL_GetFloatProperty(SDL_PropertiesID props, const char *name, float default_value);</code>
    /// <summary>
    /// <para> Get a floating point property from a group of properties. </para>
    /// <para> You can use <see cref="GetPropertyType"/> to query whether the property exists and is a floating point property. </para>
    /// </summary>
    /// <param name="props"> the properties to query. </param>
    /// <param name="name"> the name of the property to query. </param>
    /// <param name="defaultValue"> the default value of the property. </param>
    /// <returns> the value of the property, or <c> `default_value` </c> if it is not set or not a float property. </returns>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="GetPropertyType"/>
    /// <seealso cref="HasProperty"/>
    /// <seealso cref="SetFloatProperty"/>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_GetFloatProperty"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial float GetFloatProperty(PropertiesID props,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string name,
        float defaultValue);

    /// <code>extern SDL_DECLSPEC bool SDLCALL SDL_GetBooleanProperty(SDL_PropertiesID props, const char *name, bool default_value);</code>
    /// <summary>
    /// <para> Get a boolean property from a group of properties. </para>
    /// <para> You can use <see cref="GetPropertyType"/> to query whether the property exists and is a boolean property. </para>
    /// </summary>
    /// <param name="props"> the properties to query. </param>
    /// <param name="name"> the name of the property to query. </param>
    /// <param name="defaultValue"> the default value of the property. </param>
    /// <returns> the value of the property, or <c> `default_value` </c> if it is not set or not a boolean property. </returns>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="GetPropertyType"/>
    /// <seealso cref="HasProperty"/>
    /// <seealso cref="SetBooleanProperty"/>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_GetBooleanProperty"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool GetBooleanProperty(PropertiesID props,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string name,
        [MarshalAs(UnmanagedType.I1)] bool defaultValue);

    /// <code>extern SDL_DECLSPEC bool SDLCALL SDL_ClearProperty(SDL_PropertiesID props, const char *name);</code>
    /// <summary> Clear a property from a group of properties. </summary>
    /// <param name="props"> the properties to modify. </param>
    /// <param name="name"> the name of the property to clear. </param>
    /// <returns> <c> true </c> on success or <c> false </c> on failure; call <see cref="GetError"/> for more information. </returns>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_ClearProperty"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool ClearProperty(PropertiesID props, [MarshalAs(UnmanagedType.LPUTF8Str)] string name);

    /// <code>extern SDL_DECLSPEC bool SDLCALL SDL_EnumerateProperties(SDL_PropertiesID props, SDL_EnumeratePropertiesCallback callback, void *userdata);</code>
    /// <summary>
    ///     <para> Enumerate the properties contained in a group of properties. </para>
    ///     <para>
    ///         The callback function is called for each property in the group of properties. The properties are locked during
    ///         enumeration.
    ///     </para>
    /// </summary>
    /// <param name="props"> the properties to query. </param>
    /// <param name="callback"> the function to call for each property. </param>
    /// <param name="userdata"> a pointer that is passed to <c> callback </c>. </param>
    /// <returns> true on success or false on failure; call <see cref="GetError"/> for more information. </returns>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_EnumerateProperties"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.I1)]
    private static unsafe partial bool SDL_EnumerateProperties(PropertiesID props,
        delegate* unmanaged[Cdecl]<nint, uint, byte*, void> callback,
        nint userdata);

    abstract class PropertyEnumerator
    {
        public abstract void Invoke(PropertiesID props, ReadOnlySpan<byte> name);

        /// <summary> Clear the captured callback/state and return to the single-slot pool. </summary>
        public abstract void Recycle();
    }

    sealed class PlainPropertyEnumerator : PropertyEnumerator
    {
        static PlainPropertyEnumerator? pooled;
        EnumeratePropertiesCallback? callback;

        public static PlainPropertyEnumerator Rent(EnumeratePropertiesCallback callback)
        {
            var invoker = Interlocked.Exchange(ref pooled, null) ?? new PlainPropertyEnumerator();
            invoker.callback = callback;
            return invoker;
        }

        public override void Invoke(PropertiesID props, ReadOnlySpan<byte> name) => callback!(props, name);

        public override void Recycle()
        {
            callback = null;
            Volatile.Write(ref pooled, this);
        }
    }

    // The typed state lives in this generic class field: no boxing, no closure capture.
    sealed class StatefulPropertyEnumerator<T> : PropertyEnumerator
    {
        static StatefulPropertyEnumerator<T>? pooled;
        EnumeratePropertiesCallback<T>? callback;
        T state;

        public static StatefulPropertyEnumerator<T> Rent(EnumeratePropertiesCallback<T> callback, T state)
        {
            var invoker = Interlocked.Exchange(ref pooled, null) ?? new StatefulPropertyEnumerator<T>();
            invoker.callback = callback;
            invoker.state = state;
            return invoker;
        }

        public override void Invoke(PropertiesID props, ReadOnlySpan<byte> name) => callback!(state, props, name);

        public override void Recycle()
        {
            callback = null;
            state = default!;
            Volatile.Write(ref pooled, this);
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static unsafe void EnumeratePropertiesThunk(nint userdata, uint props, byte* name)
    {
        try
        {
            var view = name is null ? default : MemoryMarshal.CreateReadOnlySpanFromNullTerminated(name);
            ((PropertyEnumerator)GCHandle.FromIntPtr(userdata).Target!).Invoke(new(props), view);
        }
        catch (Exception exception)
        {
            ReportCallbackException(exception);
        }
    }

    static unsafe bool EnumeratePropertiesCore(PropertiesID props, PropertyEnumerator enumerator)
    {
        // Call-scoped: enumeration is synchronous, so the root is freed as soon as the native call
        // returns — the callback can no longer fire past that point.
        var handle = GCHandle.Alloc(enumerator);
        try
        {
            return SDL_EnumerateProperties(props, &EnumeratePropertiesThunk, GCHandle.ToIntPtr(handle));
        }
        finally
        {
            handle.Free();
            enumerator.Recycle();
        }
    }

    /// <code>extern SDL_DECLSPEC bool SDLCALL SDL_EnumerateProperties(SDL_PropertiesID props, SDL_EnumeratePropertiesCallback callback, void *userdata);</code>
    /// <summary>
    ///     See the native documentation above. <paramref name="callback"/> runs once per property with
    ///     the name as a transient UTF-8 span (valid only during the call); the set is locked throughout.
    /// </summary>
    public static bool EnumerateProperties(PropertiesID props, EnumeratePropertiesCallback callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        return EnumeratePropertiesCore(props, PlainPropertyEnumerator.Rent(callback));
    }

    /// <summary> Context-carrying variant of <see cref="EnumerateProperties(PropertiesID, EnumeratePropertiesCallback)"/> (no boxing, no closure). </summary>
    public static bool EnumerateProperties<T>(PropertiesID props, EnumeratePropertiesCallback<T> callback, T state)
    {
        ArgumentNullException.ThrowIfNull(callback);
        return EnumeratePropertiesCore(props, StatefulPropertyEnumerator<T>.Rent(callback, state));
    }

    /// <code>extern SDL_DECLSPEC void SDLCALL SDL_DestroyProperties(SDL_PropertiesID props);</code>
    /// <summary>
    ///     <para> Destroy a group of properties. </para>
    ///     <para> All properties are deleted and their cleanup functions will be called, if any. </para>
    /// </summary>
    /// <param name="props"> the properties to destroy. </param>
    /// <threadsafety>
    ///     This function should not be called while these properties are locked or other threads might be setting or
    ///     getting values from these properties.
    /// </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="CreateProperties"/>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_DestroyProperties"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void DestroyProperties(PropertiesID props);
}