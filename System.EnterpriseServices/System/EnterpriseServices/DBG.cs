namespace System.EnterpriseServices
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Security.Permissions;
    using System.Threading;

    internal static class DBG
    {
        private static System.EnterpriseServices.BooleanSwitch _conSwitch;
        private static System.EnterpriseServices.TraceSwitch _crmSwitch;
        private static System.EnterpriseServices.BooleanSwitch _dbgDisable;
        private static System.EnterpriseServices.TraceSwitch _genSwitch;
        private static volatile bool _initialized;
        private static System.EnterpriseServices.TraceSwitch _perfSwitch;
        private static System.EnterpriseServices.TraceSwitch _platSwitch;
        private static System.EnterpriseServices.TraceSwitch _poolSwitch;
        private static System.EnterpriseServices.TraceSwitch _regSwitch;
        private static System.EnterpriseServices.TraceSwitch _scSwitch;
        private static System.EnterpriseServices.BooleanSwitch _stackSwitch;
        private static System.EnterpriseServices.TraceSwitch _thkSwitch;
        private static object initializeLock = new object();

        [Conditional("_DEBUG")]
        public static void Assert(bool cond, string msg)
        {
            if (!_initialized)
            {
                InitDBG();
            }
        }

        [Conditional("_DEBUG")]
        public static void Assert(bool cond, string msg, string detail)
        {
            if (!_initialized)
            {
                InitDBG();
            }
        }

        [Conditional("_DEBUG")]
        private static void DoAssert(string msg, string detail)
        {
            StackTrace trace = new StackTrace();
            string lpText = string.Concat(new object[] { msg, "\n\n", detail, "\n", trace, "\n\nPress RETRY to launch a debugger." });
            string lpCaption = "ALERT: System.EnterpriseServices,  TID=" + TID();
            Util.OutputDebugString(lpCaption + "\n\n" + lpText);
            if (!Debugger.IsAttached)
            {
                switch (Util.MessageBox(0, lpText, lpCaption, 50))
                {
                    case 3:
                        Environment.Exit(1);
                        return;

                    case 4:
                        if (!Debugger.IsAttached)
                        {
                            Debugger.Launch();
                            return;
                        }
                        Debugger.Break();
                        return;

                    case 5:
                        return;
                }
            }
            else
            {
                Debugger.Break();
            }
        }

        [Conditional("_DEBUG")]
        public static void Error(System.EnterpriseServices.TraceSwitch sw, string msg)
        {
        }

        [Conditional("_DEBUG")]
        public static void Info(System.EnterpriseServices.TraceSwitch sw, string msg)
        {
        }

        public static void InitDBG()
        {
            if (!_initialized)
            {
                lock (initializeLock)
                {
                    if (!_initialized)
                    {
                        new RegistryPermission(PermissionState.Unrestricted).Assert();
                        _genSwitch = new System.EnterpriseServices.TraceSwitch("General");
                        _platSwitch = new System.EnterpriseServices.TraceSwitch("Platform");
                        _regSwitch = new System.EnterpriseServices.TraceSwitch("Registration");
                        _crmSwitch = new System.EnterpriseServices.TraceSwitch("CRM");
                        _perfSwitch = new System.EnterpriseServices.TraceSwitch("PerfLog");
                        _poolSwitch = new System.EnterpriseServices.TraceSwitch("ObjectPool");
                        _thkSwitch = new System.EnterpriseServices.TraceSwitch("Thunk");
                        _scSwitch = new System.EnterpriseServices.TraceSwitch("ServicedComponent");
                        _conSwitch = new System.EnterpriseServices.BooleanSwitch("ConsoleOutput");
                        _dbgDisable = new System.EnterpriseServices.BooleanSwitch("DisableDebugOutput");
                        _stackSwitch = new System.EnterpriseServices.BooleanSwitch("PrintStacks");
                        _initialized = true;
                    }
                }
            }
        }

        [Conditional("_DEBUG")]
        public static void Status(System.EnterpriseServices.TraceSwitch sw, string msg)
        {
        }

        private static int TID()
        {
            return Thread.CurrentThread.GetHashCode();
        }

        [Conditional("_DEBUG")]
        public static void Trace(System.EnterpriseServices.TraceLevel level, System.EnterpriseServices.TraceSwitch sw, string msg)
        {
            if (!_initialized)
            {
                InitDBG();
            }
            if ((sw.Level != 0) && (sw.Level >= level))
            {
                string str = string.Concat(new object[] { TID(), ": ", sw.Name, ": ", msg });
                if (_stackSwitch.Enabled)
                {
                    str = str + new StackTrace(2).ToString();
                }
                if (_conSwitch.Enabled)
                {
                    Console.WriteLine(str);
                }
                if (!_dbgDisable.Enabled)
                {
                    Util.OutputDebugString(str + "\n");
                }
            }
        }

        [Conditional("_DEBUG")]
        public static void Warning(System.EnterpriseServices.TraceSwitch sw, string msg)
        {
        }

        public static System.EnterpriseServices.TraceSwitch CRM
        {
            get
            {
                if (!_initialized)
                {
                    InitDBG();
                }
                return _crmSwitch;
            }
        }

        public static System.EnterpriseServices.TraceSwitch General
        {
            get
            {
                if (!_initialized)
                {
                    InitDBG();
                }
                return _genSwitch;
            }
        }

        public static System.EnterpriseServices.TraceSwitch Perf
        {
            get
            {
                if (!_initialized)
                {
                    InitDBG();
                }
                return _perfSwitch;
            }
        }

        public static System.EnterpriseServices.TraceSwitch Platform
        {
            get
            {
                if (!_initialized)
                {
                    InitDBG();
                }
                return _platSwitch;
            }
        }

        public static System.EnterpriseServices.TraceSwitch Pool
        {
            get
            {
                if (!_initialized)
                {
                    InitDBG();
                }
                return _poolSwitch;
            }
        }

        public static System.EnterpriseServices.TraceSwitch Registration
        {
            get
            {
                if (!_initialized)
                {
                    InitDBG();
                }
                return _regSwitch;
            }
        }

        public static System.EnterpriseServices.TraceSwitch SC
        {
            get
            {
                if (!_initialized)
                {
                    InitDBG();
                }
                return _scSwitch;
            }
        }

        public static System.EnterpriseServices.TraceSwitch Thunk
        {
            get
            {
                if (!_initialized)
                {
                    InitDBG();
                }
                return _thkSwitch;
            }
        }
    }
}

