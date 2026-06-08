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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

public static partial class SDL
{
    /// <code>extern SDL_DECLSPEC Uint64 SDLCALL SDL_GetTicks(void);</code>
    /// <summary> Get the number of milliseconds that have elapsed since the SDL library initialization. </summary>
    /// <returns>
    /// n unsigned 64‑bit integer that represents the number of milliseconds that have elapsed since the SDL library was
    /// initialized (typically via a call to SDL_Init).
    /// </returns>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="GetTicksNS"/>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_GetTicks"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl), typeof(CallConvSuppressGCTransition)]),
     MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial ulong GetTicks();

    /// <code>extern SDL_DECLSPEC Uint64 SDLCALL SDL_GetTicksNS(void);</code>
    /// <summary> Get the number of nanoseconds since SDL library initialization. </summary>
    /// <returns> an unsigned 64-bit value representing the number of nanoseconds since the SDL library initialized. </returns>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_GetTicksNS"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl), typeof(CallConvSuppressGCTransition)]),
     MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial ulong GetTicksNS();

    /// <code>extern SDL_DECLSPEC Uint64 SDLCALL SDL_GetPerformanceCounter(void);</code>
    /// <summary>
    /// <para> Get the current value of the high resolution counter. </para>
    /// <para> This function is typically used for profiling. </para>
    /// <para>
    /// The counter values are only meaningful relative to each other. Differences between values can be converted to times
    /// by using <see cref="GetPerformanceFrequency"/>.
    /// </para>
    /// </summary>
    /// <returns> the current counter value. </returns>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="GetPerformanceFrequency"/>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_GetPerformanceCounter"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl), typeof(CallConvSuppressGCTransition)]),
     MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial ulong GetPerformanceCounter();

    /// <code>extern SDL_DECLSPEC Uint64 SDLCALL SDL_GetPerformanceFrequency(void);</code>
    /// <summary> Get the count per second of the high resolution counter. </summary>
    /// <returns> a platform-specific count per second. </returns>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="GetPerformanceCounter"/>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_GetPerformanceFrequency"),
     UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl), typeof(CallConvSuppressGCTransition)]),
     MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial ulong GetPerformanceFrequency();

    /// <code>extern SDL_DECLSPEC void SDLCALL SDL_Delay(Uint32 ms);</code>
    /// <summary>
    /// <para> Wait a specified number of milliseconds before returning. </para>
    /// <para>
    /// This function waits a specified number of milliseconds before returning. It waits at least the specified time, but
    /// possibly longer due to OS scheduling.
    /// </para>
    /// </summary>
    /// <param name="ms"> the number of milliseconds to delay. </param>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="DelayNS"/>
    /// <seealso cref="DelayPrecise"/>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_Delay"), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]),
     MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void Delay(uint ms);

    /// <code>extern SDL_DECLSPEC void SDLCALL SDL_DelayNS(Uint64 ns);</code>
    /// <summary>
    /// <para> Wait a specified number of nanoseconds before returning. </para>
    /// <para>
    /// This function waits a specified number of nanoseconds before returning. It waits at least the specified time, but
    /// possibly longer due to OS scheduling.
    /// </para>
    /// </summary>
    /// <param name="ns"> the number of nanoseconds to delay. </param>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="Delay"/>
    /// <seealso cref="DelayPrecise"/>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_DelayNS"), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]),
     MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void DelayNS(ulong ns);

    /// <code>extern SDL_DECLSPEC void SDLCALL SDL_DelayPrecise(Uint64 ns);</code>
    /// <summary>
    /// <para> Wait a specified number of nanoseconds before returning. </para>
    /// <para>
    /// This function waits a specified number of nanoseconds before returning. It will attempt to wait as close to the
    /// requested time as possible, busy waiting if necessary, but could return later due to OS scheduling.
    /// </para>
    /// </summary>
    /// <param name="ns"> the number of nanoseconds to delay. </param>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.1.6. </since>
    /// <seealso cref="Delay"/>
    /// <seealso cref="DelayNS"/>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_DelayPrecise"), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)]),
     MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void DelayPrecise(ulong ns);

    /// <code>extern SDL_DECLSPEC SDL_TimerID SDLCALL SDL_AddTimer(Uint32 interval, SDL_TimerCallback callback, void *userdata);</code>
    /// <summary>
    /// <para> Call a callback function at a future time. </para>
    /// <para>
    /// The callback function is passed the current timer interval and the user supplied parameter from the
    /// <see cref="AddTimer"/> call and should return the next timer interval. If the value returned from the callback is 0, the
    /// timer is canceled and will be removed.
    /// </para>
    /// <para>
    /// The callback is run on a separate thread, and for short timeouts can potentially be called before this function
    /// returns.
    /// </para>
    /// <para>
    /// Timers take into account the amount of time it took to execute the callback. For example, if the callback took 250 ms
    /// to execute and returned 1000 (ms), the timer would only wait another 750 ms before its next iteration.
    /// </para>
    /// <para>
    /// Timing may be inexact due to OS scheduling. Be sure to note the current time with <see cref="GetTicksNS"/> or
    /// <see cref="GetPerformanceCounter"/> in case your callback needs to adjust for variances.
    /// </para>
    /// </summary>
    /// <param name="interval"> the timer delay, in milliseconds, passed to <c> callback </c>. </param>
    /// <param name="callback"> the <see cref="TimerCallback"/> function to call when the specified <c> interval </c> elapses. </param>
    /// <param name="userdata"> a pointer that is passed to <c> callback </c>. </param>
    /// <returns> a timer ID or 0 on failure; call <see cref="GetError"/> for more information. </returns>
    /// <threadsafety> It is safe to call this function from any thread. </threadsafety>
    /// <since> This function is available since SDL 3.2.0 </since>
    /// <seealso cref="AddTimerNS"/>
    /// <seealso cref="RemoveTimer"/>
    [LibraryImport(SDLLibrary, EntryPoint = "SDL_AddTimer"), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial uint SDL_AddTimer(uint interval,
        delegate* unmanaged[Cdecl]<nint, uint, uint, uint> callback,
        nint userdata);

    // Live timer roots, keyed by timer id. A registration is released by whoever loses the race between
    // RemoveTimer and the callback cancelling itself by returning 0 (the timer thread may fire before
    // AddTimer even returns, hence the interlocked flag on the registration itself).
    static readonly System.Collections.Concurrent.ConcurrentDictionary<uint, TimerRegistration> timers = new();

    abstract class TimerRegistration
    {
        public GCHandle Self;
        int released;

        public virtual uint InvokeMs(uint timerId, uint interval) => 0;
        public virtual ulong InvokeNs(uint timerId, ulong interval) => 0;

        public void Release(uint id)
        {
            if (Interlocked.Exchange(ref released, 1) != 0) return;

            timers.TryRemove(id, out _);
            Self.Free();
        }

        public bool IsReleased => Volatile.Read(ref released) != 0;
    }

    sealed class MsTimerRegistration(TimerCallback callback) : TimerRegistration
    {
        public override uint InvokeMs(uint timerId, uint interval) => callback(timerId, interval);
    }

    // The typed state lives in this generic class field: no boxing, no closure capture.
    sealed class MsTimerRegistration<T>(TimerCallback<T> callback, T state) : TimerRegistration
    {
        public override uint InvokeMs(uint timerId, uint interval) => callback(state, timerId, interval);
    }

    sealed class NsTimerRegistration(NSTimerCallback callback) : TimerRegistration
    {
        public override ulong InvokeNs(uint timerId, ulong interval) => callback(timerId, interval);
    }

    sealed class NsTimerRegistration<T>(NSTimerCallback<T> callback, T state) : TimerRegistration
    {
        public override ulong InvokeNs(uint timerId, ulong interval) => callback(state, timerId, interval);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static uint TimerThunk(nint userdata, uint timerId, uint interval)
    {
        var registration = (TimerRegistration)GCHandle.FromIntPtr(userdata).Target!;
        uint next;
        try
        {
            next = registration.InvokeMs(timerId, interval);
        }
        catch (Exception exception)
        {
            // Cancel a throwing timer rather than letting it throw every tick.
            ReportCallbackException(exception);
            next = 0;
        }

        if (next == 0) registration.Release(timerId);
        return next;
    }

    static uint AddTimerCore(uint interval, TimerRegistration registration)
    {
        registration.Self = GCHandle.Alloc(registration);

        uint id;
        unsafe
        {
            id = SDL_AddTimer(interval, &TimerThunk, GCHandle.ToIntPtr(registration.Self));
        }

        if (id == 0)
        {
            registration.Self.Free();
            return 0;
        }

        // Publish for RemoveTimer; if the callback already self-cancelled, undo the publish.
        timers[id] = registration;
        if (registration.IsReleased) timers.TryRemove(id, out _);
        return id;
    }

    /// <summary>
    ///     See the native documentation above. State is captured by closure; the delegate is rooted until
    ///     the timer is removed or its callback returns 0.
    /// </summary>
    public static uint AddTimer(uint interval, TimerCallback callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        return AddTimerCore(interval, new MsTimerRegistration(callback));
    }

    /// <summary>Stateful variant: state reaches the callback without boxing — use a static lambda.</summary>
    public static uint AddTimer<T>(uint interval, TimerCallback<T> callback, T state)
    {
        ArgumentNullException.ThrowIfNull(callback);
        return AddTimerCore(interval, new MsTimerRegistration<T>(callback, state));
    }

    [LibraryImport(SDLLibrary, EntryPoint = "SDL_AddTimerNS"), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial uint SDL_AddTimerNS(ulong interval,
        delegate* unmanaged[Cdecl]<nint, uint, ulong, ulong> callback,
        nint userdata);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static ulong NSTimerThunk(nint userdata, uint timerId, ulong interval)
    {
        var registration = (TimerRegistration)GCHandle.FromIntPtr(userdata).Target!;
        ulong next;
        try
        {
            next = registration.InvokeNs(timerId, interval);
        }
        catch (Exception exception)
        {
            ReportCallbackException(exception);
            next = 0;
        }

        if (next == 0) registration.Release(timerId);
        return next;
    }

    static uint AddTimerNSCore(ulong interval, TimerRegistration registration)
    {
        registration.Self = GCHandle.Alloc(registration);

        uint id;
        unsafe
        {
            id = SDL_AddTimerNS(interval, &NSTimerThunk, GCHandle.ToIntPtr(registration.Self));
        }

        if (id == 0)
        {
            registration.Self.Free();
            return 0;
        }

        timers[id] = registration;
        if (registration.IsReleased) timers.TryRemove(id, out _);
        return id;
    }

    /// <summary>
    ///     See the native documentation above. State is captured by closure; the delegate is rooted until
    ///     the timer is removed or its callback returns 0.
    /// </summary>
    public static uint AddTimerNS(ulong interval, NSTimerCallback callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        return AddTimerNSCore(interval, new NsTimerRegistration(callback));
    }

    /// <summary>Stateful variant: state reaches the callback without boxing — use a static lambda.</summary>
    public static uint AddTimerNS<T>(ulong interval, NSTimerCallback<T> callback, T state)
    {
        ArgumentNullException.ThrowIfNull(callback);
        return AddTimerNSCore(interval, new NsTimerRegistration<T>(callback, state));
    }

    [LibraryImport(SDLLibrary, EntryPoint = "SDL_RemoveTimer"), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool SDL_RemoveTimer(uint id);

    /// <summary>Remove a timer created with <see cref="AddTimer(uint, TimerCallback)"/>, releasing its root.</summary>
    public static bool RemoveTimer(uint id)
    {
        var removed = SDL_RemoveTimer(id);
        if (timers.TryGetValue(id, out var registration)) registration.Release(id);
        return removed;
    }
}
