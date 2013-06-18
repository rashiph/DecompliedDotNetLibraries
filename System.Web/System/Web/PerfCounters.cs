namespace System.Web
{
    using System;
    using System.Runtime;

    internal sealed class PerfCounters
    {
        private static IntPtr _global = IntPtr.Zero;
        private static PerfInstanceDataHandle _instance = null;
        private static IntPtr _stateService = IntPtr.Zero;

        private PerfCounters()
        {
        }

        internal static void DecrementCounter(AppPerfCounter counter)
        {
            if (_instance != null)
            {
                UnsafeNativeMethods.PerfDecrementCounter(_instance.UnsafeHandle, (int) counter);
            }
        }

        internal static void DecrementGlobalCounter(GlobalPerfCounter counter)
        {
            if (_global != IntPtr.Zero)
            {
                UnsafeNativeMethods.PerfDecrementCounter(_global, (int) counter);
            }
        }

        internal static void DecrementStateServiceCounter(StateServicePerfCounter counter)
        {
            if (_stateService != IntPtr.Zero)
            {
                UnsafeNativeMethods.PerfDecrementCounter(_stateService, (int) counter);
                switch (counter)
                {
                    case StateServicePerfCounter.STATE_SERVICE_SESSIONS_ACTIVE:
                        DecrementGlobalCounter(GlobalPerfCounter.STATE_SERVER_SESSIONS_ACTIVE);
                        return;

                    case StateServicePerfCounter.STATE_SERVICE_SESSIONS_ABANDONED:
                        DecrementGlobalCounter(GlobalPerfCounter.STATE_SERVER_SESSIONS_ABANDONED);
                        return;

                    case StateServicePerfCounter.STATE_SERVICE_SESSIONS_TIMED_OUT:
                        DecrementGlobalCounter(GlobalPerfCounter.STATE_SERVER_SESSIONS_TIMED_OUT);
                        return;

                    case StateServicePerfCounter.STATE_SERVICE_SESSIONS_TOTAL:
                        DecrementGlobalCounter(GlobalPerfCounter.STATE_SERVER_SESSIONS_TOTAL);
                        return;
                }
            }
        }

        internal static int GetGlobalCounter(GlobalPerfCounter counter)
        {
            if (_global != IntPtr.Zero)
            {
                return UnsafeNativeMethods.PerfGetCounter(_global, (int) counter);
            }
            return -1;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal static void IncrementCounter(AppPerfCounter counter)
        {
            if (_instance != null)
            {
                UnsafeNativeMethods.PerfIncrementCounter(_instance.UnsafeHandle, (int) counter);
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal static void IncrementCounterEx(AppPerfCounter counter, int delta)
        {
            if (_instance != null)
            {
                UnsafeNativeMethods.PerfIncrementCounterEx(_instance.UnsafeHandle, (int) counter, delta);
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal static void IncrementGlobalCounter(GlobalPerfCounter counter)
        {
            if (_global != IntPtr.Zero)
            {
                UnsafeNativeMethods.PerfIncrementCounter(_global, (int) counter);
            }
        }

        internal static void IncrementStateServiceCounter(StateServicePerfCounter counter)
        {
            if (_stateService != IntPtr.Zero)
            {
                UnsafeNativeMethods.PerfIncrementCounter(_stateService, (int) counter);
                switch (counter)
                {
                    case StateServicePerfCounter.STATE_SERVICE_SESSIONS_ACTIVE:
                        IncrementGlobalCounter(GlobalPerfCounter.STATE_SERVER_SESSIONS_ACTIVE);
                        return;

                    case StateServicePerfCounter.STATE_SERVICE_SESSIONS_ABANDONED:
                        IncrementGlobalCounter(GlobalPerfCounter.STATE_SERVER_SESSIONS_ABANDONED);
                        return;

                    case StateServicePerfCounter.STATE_SERVICE_SESSIONS_TIMED_OUT:
                        IncrementGlobalCounter(GlobalPerfCounter.STATE_SERVER_SESSIONS_TIMED_OUT);
                        return;

                    case StateServicePerfCounter.STATE_SERVICE_SESSIONS_TOTAL:
                        IncrementGlobalCounter(GlobalPerfCounter.STATE_SERVER_SESSIONS_TOTAL);
                        return;
                }
            }
        }

        internal static void Open(string appName)
        {
            OpenCounter(appName);
        }

        private static void OpenCounter(string appName)
        {
            try
            {
                if (HttpRuntime.IsEngineLoaded)
                {
                    if (_global == IntPtr.Zero)
                    {
                        _global = UnsafeNativeMethods.PerfOpenGlobalCounters();
                    }
                    if (appName == null)
                    {
                        if (_stateService == IntPtr.Zero)
                        {
                            _stateService = UnsafeNativeMethods.PerfOpenStateCounters();
                        }
                    }
                    else if (appName != null)
                    {
                        _instance = UnsafeNativeMethods.PerfOpenAppCounters(appName);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        internal static void OpenStateCounters()
        {
            OpenCounter(null);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal static void SetCounter(AppPerfCounter counter, int value)
        {
            if (_instance != null)
            {
                UnsafeNativeMethods.PerfSetCounter(_instance.UnsafeHandle, (int) counter, value);
            }
        }

        internal static void SetGlobalCounter(GlobalPerfCounter counter, int value)
        {
            if (_global != IntPtr.Zero)
            {
                UnsafeNativeMethods.PerfSetCounter(_global, (int) counter, value);
            }
        }

        internal static void SetStateServiceCounter(StateServicePerfCounter counter, int value)
        {
            if (_stateService != IntPtr.Zero)
            {
                UnsafeNativeMethods.PerfSetCounter(_stateService, (int) counter, value);
                switch (counter)
                {
                    case StateServicePerfCounter.STATE_SERVICE_SESSIONS_ACTIVE:
                        SetGlobalCounter(GlobalPerfCounter.STATE_SERVER_SESSIONS_ACTIVE, value);
                        return;

                    case StateServicePerfCounter.STATE_SERVICE_SESSIONS_ABANDONED:
                        SetGlobalCounter(GlobalPerfCounter.STATE_SERVER_SESSIONS_ABANDONED, value);
                        return;

                    case StateServicePerfCounter.STATE_SERVICE_SESSIONS_TIMED_OUT:
                        SetGlobalCounter(GlobalPerfCounter.STATE_SERVER_SESSIONS_TIMED_OUT, value);
                        return;

                    case StateServicePerfCounter.STATE_SERVICE_SESSIONS_TOTAL:
                        SetGlobalCounter(GlobalPerfCounter.STATE_SERVER_SESSIONS_TOTAL, value);
                        return;
                }
            }
        }
    }
}

