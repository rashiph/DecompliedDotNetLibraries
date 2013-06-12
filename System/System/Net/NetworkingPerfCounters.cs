namespace System.Net
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.Versioning;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;

    internal sealed class NetworkingPerfCounters
    {
        private const string categoryName = ".NET CLR Networking 4.0.0.0";
        private volatile bool cleanupCalled;
        private static readonly string[] counterNames = new string[] { "Connections Established", "Bytes Received", "Bytes Sent", "Datagrams Received", "Datagrams Sent", "HttpWebRequests Created/Sec", "HttpWebRequests Average Lifetime", "HttpWebRequests Average Lifetime Base", "HttpWebRequests Queued/Sec", "HttpWebRequests Average Queue Time", "HttpWebRequests Average Queue Time Base", "HttpWebRequests Aborted/Sec", "HttpWebRequests Failed/Sec" };
        private CounterPair[] counters;
        private bool enabled = SettingsSectionInternal.Section.PerformanceCountersEnabled;
        private const string globalInstanceName = "_Global_";
        private volatile bool initDone;
        private bool initSuccessful;
        private static NetworkingPerfCounters instance;
        private const int instanceNameMaxLength = 0x7f;
        private static object lockObject = new object();

        private NetworkingPerfCounters()
        {
        }

        private void Cleanup()
        {
            lock (lockObject)
            {
                if (!this.cleanupCalled)
                {
                    this.cleanupCalled = true;
                    if (this.counters != null)
                    {
                        foreach (CounterPair pair in this.counters)
                        {
                            if (!Environment.HasShutdownStarted && (pair != null))
                            {
                                try
                                {
                                    pair.InstanceCounter.RemoveInstance();
                                }
                                catch (InvalidOperationException exception)
                                {
                                    if (Logging.On)
                                    {
                                        Logging.Exception(Logging.Web, "NetworkingPerfCounters", "Cleanup", exception);
                                    }
                                }
                                catch (Win32Exception exception2)
                                {
                                    if (Logging.On)
                                    {
                                        Logging.Exception(Logging.Web, "NetworkingPerfCounters", "Cleanup", exception2);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool CounterAvailable()
        {
            if (!this.enabled || this.cleanupCalled)
            {
                return false;
            }
            return (this.initDone && this.initSuccessful);
        }

        private static CounterPair CreateCounterPair(string counterName, string instanceName)
        {
            PerformanceCounter globalCounter = new PerformanceCounter(".NET CLR Networking 4.0.0.0", counterName, "_Global_", false);
            return new CounterPair(new PerformanceCounter { CategoryName = ".NET CLR Networking 4.0.0.0", CounterName = counterName, InstanceName = instanceName, InstanceLifetime = PerformanceCounterInstanceLifetime.Process, ReadOnly = false, RawValue = 0L }, globalCounter);
        }

        private static void CreateInstance()
        {
            instance = new NetworkingPerfCounters();
            if ((instance.Enabled && !ThreadPool.QueueUserWorkItem(new WaitCallback(instance.Initialize))) && Logging.On)
            {
                Logging.PrintError(Logging.Web, SR.GetString("net_perfcounter_cant_queue_workitem"));
            }
        }

        public void Decrement(NetworkingPerfCounterName perfCounter)
        {
            this.Increment(perfCounter, -1L);
        }

        public void Decrement(NetworkingPerfCounterName perfCounter, long amount)
        {
            this.Increment(perfCounter, -amount);
        }

        private void ExceptionEventHandler(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.IsTerminating)
            {
                this.Cleanup();
            }
        }

        private void ExitEventHandler(object sender, EventArgs e)
        {
            this.Cleanup();
        }

        private static string GetInstanceName()
        {
            string str = ReplaceInvalidChars(AppDomain.CurrentDomain.FriendlyName);
            string str2 = VersioningHelper.MakeVersionSafeName(string.Empty, ResourceScope.Machine, ResourceScope.AppDomain);
            string str3 = str + str2;
            if (str3.Length > 0x7f)
            {
                str3 = str.Substring(0, 0x7f - str2.Length) + str2;
            }
            return str3;
        }

        public static long GetTimestamp()
        {
            return Stopwatch.GetTimestamp();
        }

        public void Increment(NetworkingPerfCounterName perfCounter)
        {
            this.Increment(perfCounter, 1L);
        }

        public void Increment(NetworkingPerfCounterName perfCounter, long amount)
        {
            if (this.CounterAvailable())
            {
                try
                {
                    CounterPair pair = this.counters[(int) perfCounter];
                    pair.InstanceCounter.IncrementBy(amount);
                    pair.GlobalCounter.IncrementBy(amount);
                }
                catch (InvalidOperationException exception)
                {
                    if (Logging.On)
                    {
                        Logging.Exception(Logging.Web, "NetworkingPerfCounters", "Increment", exception);
                    }
                }
                catch (Win32Exception exception2)
                {
                    if (Logging.On)
                    {
                        Logging.Exception(Logging.Web, "NetworkingPerfCounters", "Increment", exception2);
                    }
                }
            }
        }

        public void IncrementAverage(NetworkingPerfCounterName perfCounter, long startTimestamp)
        {
            if (this.CounterAvailable())
            {
                long amount = ((GetTimestamp() - startTimestamp) * 0x3e8L) / Stopwatch.Frequency;
                this.Increment(perfCounter, amount);
                this.Increment(perfCounter + 1, 1L);
            }
        }

        private void Initialize(object state)
        {
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.Web, SR.GetString("net_perfcounter_initialization_started"));
            }
            new PerformanceCounterPermission(PermissionState.Unrestricted).Assert();
            try
            {
                if (!PerformanceCounterCategory.Exists(".NET CLR Networking 4.0.0.0"))
                {
                    if (Logging.On)
                    {
                        Logging.PrintError(Logging.Web, SR.GetString("net_perfcounter_nocategory", new object[] { ".NET CLR Networking 4.0.0.0" }));
                    }
                }
                else
                {
                    string instanceName = GetInstanceName();
                    this.counters = new CounterPair[counterNames.Length];
                    for (int i = 0; i < counterNames.Length; i++)
                    {
                        this.counters[i] = CreateCounterPair(counterNames[i], instanceName);
                    }
                    AppDomain.CurrentDomain.DomainUnload += new EventHandler(this.UnloadEventHandler);
                    AppDomain.CurrentDomain.ProcessExit += new EventHandler(this.ExitEventHandler);
                    AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(this.ExceptionEventHandler);
                    this.initSuccessful = true;
                }
            }
            catch (Win32Exception exception)
            {
                if (Logging.On)
                {
                    Logging.Exception(Logging.Web, "NetworkingPerfCounters", "Initialize", exception);
                }
                this.Cleanup();
            }
            catch (InvalidOperationException exception2)
            {
                if (Logging.On)
                {
                    Logging.Exception(Logging.Web, "NetworkingPerfCounters", "Initialize", exception2);
                }
                this.Cleanup();
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
                this.initDone = true;
                if (Logging.On)
                {
                    if (this.initSuccessful)
                    {
                        Logging.PrintInfo(Logging.Web, SR.GetString("net_perfcounter_initialized_success"));
                    }
                    else
                    {
                        Logging.PrintInfo(Logging.Web, SR.GetString("net_perfcounter_initialized_error"));
                    }
                }
            }
        }

        private static string ReplaceInvalidChars(string instanceName)
        {
            StringBuilder builder = new StringBuilder(instanceName);
            for (int i = 0; i < builder.Length; i++)
            {
                switch (builder[i])
                {
                    case '/':
                    case '\\':
                    case '#':
                        builder[i] = '_';
                        break;

                    case '(':
                        builder[i] = '[';
                        break;

                    case ')':
                        builder[i] = ']';
                        break;
                }
            }
            return builder.ToString();
        }

        private void UnloadEventHandler(object sender, EventArgs e)
        {
            this.Cleanup();
        }

        public bool Enabled
        {
            get
            {
                return this.enabled;
            }
        }

        public static NetworkingPerfCounters Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObject)
                    {
                        if (instance == null)
                        {
                            CreateInstance();
                        }
                    }
                }
                return instance;
            }
        }

        private class CounterPair
        {
            private PerformanceCounter globalCounter;
            private PerformanceCounter instanceCounter;

            public CounterPair(PerformanceCounter instanceCounter, PerformanceCounter globalCounter)
            {
                this.instanceCounter = instanceCounter;
                this.globalCounter = globalCounter;
            }

            public PerformanceCounter GlobalCounter
            {
                get
                {
                    return this.globalCounter;
                }
            }

            public PerformanceCounter InstanceCounter
            {
                get
                {
                    return this.instanceCounter;
                }
            }
        }
    }
}

