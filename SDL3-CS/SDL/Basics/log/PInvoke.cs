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
    /// <code>extern SDL_DECLSPEC void SDLCALL SDL_SetLogPriorities(SDL_LogPriority priority);</code>
    /// <summary> Set the priority of all log categories. </summary>
    /// <param name="priority"> the <see cref="LogPriority"/> to assign. </param>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="ResetLogPriorities"/>
    /// <seealso cref="SetLogPriority"/>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_SetLogPriorities"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void SetLogPriorities(LogPriority priority);

    /// <code>extern SDL_DECLSPEC void SDLCALL SDL_SetLogPriority(int category, SDL_LogPriority priority);</code>
    /// <summary> Set the priority of a particular log category. </summary>
    /// <param name="category"> the category to assign a priority to. </param>
    /// <param name="priority"> the <see cref="LogPriority"/> to assign. </param>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="GetLogPriority"/>
    /// <seealso cref="ResetLogPriorities"/>
    /// <seealso cref="SetLogPriorities"/>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_SetLogPriority"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void SetLogPriority(LogCategory category, LogPriority priority);

    /// <code>extern SDL_DECLSPEC SDL_LogPriority SDLCALL SDL_GetLogPriority(int category);</code>
    /// <summary> Get the priority of a particular log category. </summary>
    /// <param name="category"> the category to query. </param>
    /// <returns> the <see cref="LogPriority"/> for the requested category. </returns>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="SetLogPriority"/>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_GetLogPriority"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial LogPriority GetLogPriority(LogCategory category);

    /// <code>extern SDL_DECLSPEC void SDLCALL SDL_ResetLogPriorities(void);</code>
    /// <summary> Reset all priorities to default. </summary>
    /// <remarks> This is called by <see cref="Quit"/>. </remarks>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="SetLogPriorities"/>
    /// <seealso cref="SetLogPriority"/>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_ResetLogPriorities"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void ResetLogPriorities();

    /// <code>extern SDL_DECLSPEC bool SDLCALL SDL_SetLogPriorityPrefix(SDL_LogPriority priority, const char *prefix);</code>
    /// <summary>
    /// <para> Set the text prepended to log messages of a given priority. </para>
    /// <para>
    /// By default <see cref="LogPriority.Info"/> and below have no prefix, and <see cref="LogPriority.Warn"/> and higher
    /// have a prefix showing their priority, e.g. <c> "WARNING: " </c>.
    /// </para>
    /// <para>
    /// This function makes a copy of its string argument, <b> prefix </b>, so it is not necessary to keep the value of
    /// **prefix** alive after the call returns.
    /// </para>
    /// </summary>
    /// <param name="priority"> the <see cref="LogPriority"/> to modify. </param>
    /// <param name="prefix"> the prefix to use for that log priority, or <c> null </c> to use no prefix. </param>
    /// <returns> <c> true </c> on success or <c> false </c> on failure; call <see cref="GetError"/> for more information. </returns>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="SetLogPriorities"/>
    /// <seealso cref="SetLogPriority"/>
    public static bool SetLogPriorityPrefix(LogPriority priority, scoped ReadOnlySpan<char> prefix)
    {
        scoped Span<byte> utf8 = default;
        byte[]? rented = null;

        if (!prefix.IsWhiteSpace())
        {
            var byteCount = Encoding.UTF8.GetByteCount(prefix) + 1;
            utf8 = byteCount <= 512 ?
                stackalloc byte[byteCount] :
                (rented = ArrayPool<byte>.Shared.Rent(byteCount)).AsSpan(0, byteCount);

            Encoding.UTF8.GetBytes(prefix, utf8);
            utf8[^1] = 0;
        }

        fixed (byte* pPrefix = utf8)
        {
            var result = SDL_SetLogPriorityPrefix(priority, pPrefix);
            if (rented is not null) ArrayPool<byte>.Shared.Return(rented);
            return result;
        }
    }

    [LibraryImport(SDLLibrary, EntryPoint = "SDL_SetLogPriorityPrefix"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool SDL_SetLogPriorityPrefix(LogPriority priority, byte* prefix);

    /// <code>extern SDL_DECLSPEC void SDLCALL SDL_Log(SDL_PRINTF_FORMAT_STRING const char *fmt, ...) SDL_PRINTF_VARARG_FUNC(1);</code>
    /// <summary> Log a message with <see cref="LogCategory.Application"/> and <see cref="LogPriority.Info"/>. </summary>
    /// <param name="fmt"> a printf() style message format string. </param>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="LogCritical"/>
    /// <seealso cref="LogDebug"/>
    /// <seealso cref="LogError"/>
    /// <seealso cref="LogInfo"/>
    /// <seealso cref="LogMessage"/>
    /// <seealso cref="LogMessageV"/>
    /// <seealso cref="LogTrace"/>
    /// <seealso cref="LogVerbose"/>
    /// <seealso cref="LogWarn"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Log(scoped ReadOnlySpan<char> fmt) => EncodeAndCall(fmt, &SDL_Log);

    [LibraryImport(SDLLibrary, EntryPoint = "SDL_Log"), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void SDL_Log(byte* fmt);

    /// <code>extern SDL_DECLSPEC void SDLCALL SDL_LogTrace(int category, SDL_PRINTF_FORMAT_STRING const char *fmt, ...) SDL_PRINTF_VARARG_FUNC(2);</code>
    /// <summary> Log a message with <see cref="LogPriority.Trace"/>. </summary>
    /// <param name="category"> the category of the message. </param>
    /// <param name="fmt"> a printf() style message format string. </param>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="Log"/>
    /// <seealso cref="LogCritical"/>
    /// <seealso cref="LogDebug"/>
    /// <seealso cref="LogError"/>
    /// <seealso cref="LogInfo"/>
    /// <seealso cref="LogMessage"/>
    /// <seealso cref="LogMessageV"/>
    /// <seealso cref="LogTrace"/>
    /// <seealso cref="LogVerbose"/>
    /// <seealso cref="LogWarn"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogTrace(LogCategory category, scoped ReadOnlySpan<char> fmt)
        => EncodeAndCall(fmt, &SDL_LogTrace, category);

    [LibraryImport(SDLLibrary, EntryPoint = "SDL_LogTrace"), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]),
     MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static partial void SDL_LogTrace(LogCategory category, byte* fmt);

    /// <code>extern SDL_DECLSPEC void SDLCALL SDL_LogVerbose(int category, SDL_PRINTF_FORMAT_STRING const char *fmt, ...) SDL_PRINTF_VARARG_FUNC(2);</code>
    /// <summary> Log a message with <see cref="LogPriority.Verbose"/>, </summary>
    /// <param name="category"> the category of the message. </param>
    /// <param name="fmt"> a printf() style message format string. </param>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="Log"/>
    /// <seealso cref="LogCritical"/>
    /// <seealso cref="LogDebug"/>
    /// <seealso cref="LogError"/>
    /// <seealso cref="LogInfo"/>
    /// <seealso cref="LogMessage"/>
    /// <seealso cref="LogMessageV"/>
    /// <seealso cref="LogWarn"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogVerbose(LogCategory category, scoped ReadOnlySpan<char> fmt)
        => EncodeAndCall(fmt, &SDL_LogVerbose, category);

    [LibraryImport(SDLLibrary, EntryPoint = "SDL_LogVerbose"), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]),
     MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static partial void SDL_LogVerbose(LogCategory category, byte* fmt);

    /// <code>extern SDL_DECLSPEC void SDLCALL SDL_LogDebug(int category, SDL_PRINTF_FORMAT_STRING const char *fmt, ...) SDL_PRINTF_VARARG_FUNC(2);</code>
    /// <summary> Log a message with <see cref="LogPriority.Debug"/>. </summary>
    /// <param name="category"> the category of the message. </param>
    /// <param name="message"> a printf() style message format string. </param>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="Log"/>
    /// <seealso cref="LogCritical"/>
    /// <seealso cref="LogError"/>
    /// <seealso cref="LogInfo"/>
    /// <seealso cref="LogMessage"/>
    /// <seealso cref="LogMessageV"/>
    /// <seealso cref="LogTrace"/>
    /// <seealso cref="LogVerbose"/>
    /// <seealso cref="LogWarn"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogDebug(LogCategory category, scoped ReadOnlySpan<char> message)
        => EncodeAndCall(message, &SDL_LogDebug, category);

    [LibraryImport(SDLLibrary, EntryPoint = "SDL_LogDebug"), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]),
     MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static partial void SDL_LogDebug(LogCategory category, byte* message);

    /// <code>extern SDL_DECLSPEC void SDLCALL SDL_LogInfo(int category, SDL_PRINTF_FORMAT_STRING const char *fmt, ...) SDL_PRINTF_VARARG_FUNC(2);</code>
    /// <summary> Log a message with <see cref="LogPriority.Info"/>. </summary>
    /// <param name="category"> the category of the message. </param>
    /// <param name="fmt"> a printf() style message format string. </param>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="Log"/>
    /// <seealso cref="LogCritical"/>
    /// <seealso cref="LogDebug"/>
    /// <seealso cref="LogError"/>
    /// <seealso cref="LogMessage"/>
    /// <seealso cref="LogMessageV"/>
    /// <seealso cref="LogTrace"/>
    /// <seealso cref="LogVerbose"/>
    /// <seealso cref="LogWarn"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogInfo(LogCategory category, scoped ReadOnlySpan<char> fmt)
        => EncodeAndCall(fmt, &SDL_LogInfo, category);

    [LibraryImport(SDLLibrary, EntryPoint = "SDL_LogInfo"), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]),
     MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static partial void SDL_LogInfo(LogCategory category, byte* fmt);

    /// <code>extern SDL_DECLSPEC void SDLCALL SDL_LogWarn(int category, SDL_PRINTF_FORMAT_STRING const char *fmt, ...) SDL_PRINTF_VARARG_FUNC(2);</code>
    /// <summary> Log a message with <see cref="LogPriority.Warn"/>. </summary>
    /// <param name="category"> the category of the message. </param>
    /// <param name="fmt"> a printf() style message format string. </param>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="Log"/>
    /// <seealso cref="LogCritical"/>
    /// <seealso cref="LogDebug"/>
    /// <seealso cref="LogError"/>
    /// <seealso cref="LogInfo"/>
    /// <seealso cref="LogMessage"/>
    /// <seealso cref="LogMessageV"/>
    /// <seealso cref="LogTrace"/>
    /// <seealso cref="LogVerbose"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogWarn(LogCategory category, scoped ReadOnlySpan<char> fmt)
        => EncodeAndCall(fmt, &SDL_LogWarn, category);

    [LibraryImport(SDLLibrary, EntryPoint = "SDL_LogWarn"), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]),
     MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static partial void SDL_LogWarn(LogCategory category, byte* fmt);

    /// <code>extern SDL_DECLSPEC void SDLCALL SDL_LogError(int category, SDL_PRINTF_FORMAT_STRING const char *fmt, ...) SDL_PRINTF_VARARG_FUNC(2);</code>
    /// <summary> Log a message with <see cref="LogPriority.Error"/>. </summary>
    /// <param name="category"> the category of the message. </param>
    /// <param name="fmt"> a printf() style message format string. </param>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="Log"/>
    /// <seealso cref="LogCritical"/>
    /// <seealso cref="LogDebug"/>
    /// <seealso cref="LogInfo"/>
    /// <seealso cref="LogMessage"/>
    /// <seealso cref="LogMessageV"/>
    /// <seealso cref="LogTrace"/>
    /// <seealso cref="LogVerbose"/>
    /// <seealso cref="LogWarn"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogError(LogCategory category, scoped ReadOnlySpan<char> fmt)
        => EncodeAndCall(fmt, &SDL_LogError, category);

    [LibraryImport(SDLLibrary, EntryPoint = "SDL_LogError"), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]),
     MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static partial void SDL_LogError(LogCategory category, byte* fmt);

    /// <code>extern SDL_DECLSPEC void SDLCALL SDL_LogCritical(int category, SDL_PRINTF_FORMAT_STRING const char *fmt, ...) SDL_PRINTF_VARARG_FUNC(2);</code>
    /// <summary> Log a message with <see cref="LogPriority.Critical"/>. </summary>
    /// <param name="category"> the category of the message. </param>
    /// <param name="fmt"> a printf() style message format string. </param>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="Log"/>
    /// <seealso cref="LogDebug"/>
    /// <seealso cref="LogError"/>
    /// <seealso cref="LogInfo"/>
    /// <seealso cref="LogMessageV"/>
    /// <seealso cref="LogTrace"/>
    /// <seealso cref="LogVerbose"/>
    /// <seealso cref="LogWarn"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogCritical(LogCategory category, scoped ReadOnlySpan<char> fmt)
        => EncodeAndCall(fmt, &SDL_LogCritical, category);

    [LibraryImport(SDLLibrary, EntryPoint = "SDL_LogCritical"), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]),
     MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static partial void SDL_LogCritical(LogCategory category, byte* fmt);

    /// <code>extern SDL_DECLSPEC void SDLCALL SDL_LogMessage(int category, SDL_LogPriority priority, SDL_PRINTF_FORMAT_STRING const char *fmt, ...) SDL_PRINTF_VARARG_FUNC(3);</code>
    /// <summary> Log a message with the specified category and priority. </summary>
    /// <param name="category"> the category of the message. </param>
    /// <param name="priority"> the priority of the message. </param>
    /// <param name="fmt"> the priority of the message. </param>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="Log"/>
    /// <seealso cref="LogCritical"/>
    /// <seealso cref="LogDebug"/>
    /// <seealso cref="LogError"/>
    /// <seealso cref="LogInfo"/>
    /// <seealso cref="LogMessageV"/>
    /// <seealso cref="LogTrace"/>
    /// <seealso cref="LogVerbose"/>
    /// <seealso cref="LogWarn"/>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_LogMessage"), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]),
     MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void LogMessage(LogCategory category,
        LogPriority priority,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string fmt);

    /// <code>extern SDL_DECLSPEC void SDLCALL SDL_LogMessageV(int category, SDL_LogPriority priority, SDL_PRINTF_FORMAT_STRING const char *fmt, va_list ap) SDL_PRINTF_VARARG_FUNCV(3);</code>
    /// <summary> Log a message with the specified category and priority. </summary>
    /// <param name="category"> the category of the message. </param>
    /// <param name="priority"> the priority of the message. </param>
    /// <param name="fmt"> a printf() style message format string. </param>
    /// <param name="ap"> a variable argument list. </param>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="Log"/>
    /// <seealso cref="LogCritical"/>
    /// <seealso cref="LogDebug"/>
    /// <seealso cref="LogError"/>
    /// <seealso cref="LogInfo"/>
    /// <seealso cref="LogMessage"/>
    /// <seealso cref="LogTrace"/>
    /// <seealso cref="LogVerbose"/>
    /// <seealso cref="LogWarn"/>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_LogMessageV"), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]),
     MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void LogMessageV(LogCategory category,
        LogPriority priority,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string fmt,
        [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPUTF8Str)] string[] ap);

    /// <code>extern SDL_DECLSPEC SDL_LogOutputFunction SDLCALL SDL_GetDefaultLogOutputFunction(void);</code>
    /// <summary> Get the default log output function. </summary>
    /// <returns> the default log output callback. </returns>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.1.6. </since>
    /// <seealso cref="SetLogOutputFunction"/>
    /// <seealso cref="GetLogOutputFunction"/>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_LogOutputFunction"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial LogOutputFunction GetDefaultLogOutputFunction();

    /// <code>extern SDL_DECLSPEC void SDLCALL SDL_GetLogOutputFunction(SDL_LogOutputFunction *callback, void **userdata);</code>
    /// <summary>
    /// <para> Get the current log output function. </para>
    /// </summary>
    /// <param name="callback"> an <see cref="LogOutputFunction"/> filled in with the current log callback. </param>
    /// <param name="userdata"> a pointer filled in with the pointer that is passed to <c> callback </c>. </param>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="GetDefaultLogOutputFunction"/>
    /// <seealso cref="SetLogOutputFunction"/>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_GetLogOutputFunction"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void GetLogOutputFunction(out LogOutputFunction callback, out nint userdata);

    /// <code>extern SDL_DECLSPEC void SDLCALL SDL_SetLogOutputFunction(SDL_LogOutputFunction callback, void *userdata);</code>
    /// <summary> Replace the default log output function with one of your own. </summary>
    /// <param name="callback"> an <see cref="LogOutputFunction"/> to call instead of the default. </param>
    /// <param name="userdata"> a pointer that is passed to <c> callback </c>. </param>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="GetDefaultLogOutputFunction"/>
    /// <seealso cref="GetLogOutputFunction"/>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_SetLogOutputFunction"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void SetLogOutputFunction(LogOutputFunction callback, nint userdata);

    static void EncodeAndCall<TState>(scoped ReadOnlySpan<char> text, delegate*<TState, byte*, void> call, TState state)
    {
        scoped Span<byte> utf8 = default;
        byte[]? rented = null;

        if (!text.IsWhiteSpace())
        {
            var byteCount = Encoding.UTF8.GetByteCount(text) + 1;
            utf8 = byteCount <= 512 ?
                stackalloc byte[byteCount] :
                (rented = ArrayPool<byte>.Shared.Rent(byteCount)).AsSpan(0, byteCount);

            Encoding.UTF8.GetBytes(text, utf8);
            utf8[^1] = 0;
        }

        fixed (byte* pText = utf8) call(state, pText);

        if (rented is not null) ArrayPool<byte>.Shared.Return(rented);
    }

    static void EncodeAndCall(scoped ReadOnlySpan<char> text, delegate*<byte*, void> call)
    {
        scoped Span<byte> utf8 = default;
        byte[]? rented = null;

        if (!text.IsWhiteSpace())
        {
            var byteCount = Encoding.UTF8.GetByteCount(text) + 1;
            utf8 = byteCount <= 512 ?
                stackalloc byte[byteCount] :
                (rented = ArrayPool<byte>.Shared.Rent(byteCount)).AsSpan(0, byteCount);

            Encoding.UTF8.GetBytes(text, utf8);
            utf8[^1] = 0;
        }

        fixed (byte* pText = utf8) call(pText);

        if (rented is not null) ArrayPool<byte>.Shared.Return(rented);
    }
}