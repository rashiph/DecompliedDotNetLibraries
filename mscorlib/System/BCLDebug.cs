namespace System
{
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;

    [SecuritySafeCritical]
    internal static class BCLDebug
    {
        private static readonly LogLevel[] levelConversions;
        internal static bool m_correctnessWarnings;
        internal static bool m_loggingNotEnabled = false;
        internal static PermissionSet m_MakeConsoleErrorLoggingWork;
        internal static bool m_perfWarnings;
        internal static bool m_registryChecked = false;
        internal static bool m_safeHandleStackTraces;
        private static readonly SwitchStructure[] switches = new SwitchStructure[] { new SwitchStructure("NLS", 1), new SwitchStructure("SER", 2), new SwitchStructure("DYNIL", 4), new SwitchStructure("REMOTE", 8), new SwitchStructure("BINARY", 0x10), new SwitchStructure("SOAP", 0x20), new SwitchStructure("REMOTINGCHANNELS", 0x40), new SwitchStructure("CACHE", 0x80), new SwitchStructure("RESMGRFILEFORMAT", 0x100), new SwitchStructure("PERF", 0x200), new SwitchStructure("CORRECTNESS", 0x400), new SwitchStructure("MEMORYFAILPOINT", 0x800), new SwitchStructure("DATETIME", 0x1000) };

        static BCLDebug()
        {
            LogLevel[] levelArray = new LogLevel[11];
            levelArray[0] = LogLevel.Panic;
            levelArray[1] = LogLevel.Error;
            levelArray[2] = LogLevel.Error;
            levelArray[3] = LogLevel.Warning;
            levelArray[4] = LogLevel.Warning;
            levelArray[5] = LogLevel.Status;
            levelArray[6] = LogLevel.Status;
            levelConversions = levelArray;
        }

        [Conditional("_DEBUG")]
        public static void Assert(bool condition, string message)
        {
        }

        internal static bool CheckEnabled(string switchName)
        {
            if (AppDomain.CurrentDomain.IsUnloadingForcedFinalize())
            {
                return false;
            }
            if (!m_registryChecked)
            {
                CheckRegistry();
            }
            LogSwitch switch2 = LogSwitch.GetSwitch(switchName);
            if (switch2 == null)
            {
                return false;
            }
            return (switch2.MinimumLevel <= LoggingLevels.TraceLevel0);
        }

        private static bool CheckEnabled(string switchName, LogLevel level, out LogSwitch logSwitch)
        {
            if (AppDomain.CurrentDomain.IsUnloadingForcedFinalize())
            {
                logSwitch = null;
                return false;
            }
            logSwitch = LogSwitch.GetSwitch(switchName);
            if (logSwitch == null)
            {
                return false;
            }
            return (logSwitch.MinimumLevel <= ((LoggingLevels) ((int) level)));
        }

        private static void CheckRegistry()
        {
            if (!AppDomain.CurrentDomain.IsUnloadingForcedFinalize() && !m_registryChecked)
            {
                bool flag;
                bool flag2;
                int num;
                m_registryChecked = true;
                int num2 = GetRegistryLoggingValues(out flag, out flag2, out num, out m_perfWarnings, out m_correctnessWarnings, out m_safeHandleStackTraces);
                if (!flag)
                {
                    m_loggingNotEnabled = true;
                }
                if (flag && (levelConversions != null))
                {
                    try
                    {
                        num = (int) levelConversions[num];
                        if (num2 > 0)
                        {
                            for (int i = 0; i < switches.Length; i++)
                            {
                                if ((switches[i].value & num2) != 0)
                                {
                                    LogSwitch switch2 = new LogSwitch(switches[i].name, switches[i].name, System.Diagnostics.Log.GlobalSwitch) {
                                        MinimumLevel = (LoggingLevels) num
                                    };
                                }
                            }
                            System.Diagnostics.Log.GlobalSwitch.MinimumLevel = (LoggingLevels) num;
                            System.Diagnostics.Log.IsConsoleEnabled = flag2;
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }

        [Conditional("_DEBUG")]
        internal static void ConsoleError(string msg)
        {
            if (!AppDomain.CurrentDomain.IsUnloadingForcedFinalize())
            {
                if (m_MakeConsoleErrorLoggingWork == null)
                {
                    PermissionSet set = new PermissionSet();
                    set.AddPermission(new EnvironmentPermission(PermissionState.Unrestricted));
                    set.AddPermission(new FileIOPermission(FileIOPermissionAccess.AllAccess, Path.GetFullPath(".")));
                    m_MakeConsoleErrorLoggingWork = set;
                }
                m_MakeConsoleErrorLoggingWork.Assert();
                using (TextWriter writer = File.AppendText("ConsoleErrors.log"))
                {
                    writer.WriteLine(msg);
                }
            }
        }

        [Conditional("_DEBUG")]
        internal static void Correctness(bool expr, string msg)
        {
        }

        internal static bool CorrectnessEnabled()
        {
            if (AppDomain.CurrentDomain.IsUnloadingForcedFinalize())
            {
                return false;
            }
            if (!m_registryChecked)
            {
                CheckRegistry();
            }
            return m_correctnessWarnings;
        }

        [Conditional("_LOGGING")]
        public static void DumpStack(string switchName)
        {
            LogSwitch switch2;
            if (!m_registryChecked)
            {
                CheckRegistry();
            }
            if (CheckEnabled(switchName, LogLevel.Trace, out switch2))
            {
                System.Diagnostics.Log.LogMessage(LoggingLevels.TraceLevel0, switch2, new StackTrace().ToString());
            }
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern int GetRegistryLoggingValues(out bool loggingEnabled, out bool logToConsole, out int logLevel, out bool perfWarnings, out bool correctnessWarnings, out bool safeHandleStackTraces);
        [Conditional("_LOGGING")]
        public static void Log(string message)
        {
            if (!AppDomain.CurrentDomain.IsUnloadingForcedFinalize())
            {
                if (!m_registryChecked)
                {
                    CheckRegistry();
                }
                System.Diagnostics.Log.Trace(message);
                System.Diagnostics.Log.Trace(Environment.NewLine);
            }
        }

        [Conditional("_LOGGING")]
        public static void Log(string switchName, string message)
        {
            if (!AppDomain.CurrentDomain.IsUnloadingForcedFinalize())
            {
                if (!m_registryChecked)
                {
                    CheckRegistry();
                }
                try
                {
                    LogSwitch logswitch = LogSwitch.GetSwitch(switchName);
                    if (logswitch != null)
                    {
                        System.Diagnostics.Log.Trace(logswitch, message);
                        System.Diagnostics.Log.Trace(logswitch, Environment.NewLine);
                    }
                }
                catch
                {
                    System.Diagnostics.Log.Trace("Exception thrown in logging." + Environment.NewLine);
                    System.Diagnostics.Log.Trace("Switch was: " + ((switchName == null) ? "<null>" : switchName) + Environment.NewLine);
                    System.Diagnostics.Log.Trace("Message was: " + ((message == null) ? "<null>" : message) + Environment.NewLine);
                }
            }
        }

        [Conditional("_LOGGING")]
        public static void Log(string switchName, LogLevel level, params object[] messages)
        {
            if (!AppDomain.CurrentDomain.IsUnloadingForcedFinalize())
            {
                LogSwitch switch2;
                if (!m_registryChecked)
                {
                    CheckRegistry();
                }
                if (CheckEnabled(switchName, level, out switch2))
                {
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < messages.Length; i++)
                    {
                        string str;
                        try
                        {
                            if (messages[i] == null)
                            {
                                str = "<null>";
                            }
                            else
                            {
                                str = messages[i].ToString();
                            }
                        }
                        catch
                        {
                            str = "<unable to convert>";
                        }
                        builder.Append(str);
                    }
                    System.Diagnostics.Log.LogMessage((LoggingLevels) level, switch2, builder.ToString());
                }
            }
        }

        [Conditional("_DEBUG")]
        internal static void Perf(bool expr, string msg)
        {
            if (!AppDomain.CurrentDomain.IsUnloadingForcedFinalize())
            {
                if (!m_registryChecked)
                {
                    CheckRegistry();
                }
                if (m_perfWarnings)
                {
                    System.Diagnostics.Assert.Check(expr, "BCL Perf Warning: Your perf may be less than perfect because...", msg);
                }
            }
        }

        [Conditional("_LOGGING")]
        public static void Trace(string switchName, params object[] messages)
        {
            LogSwitch switch2;
            if (!m_loggingNotEnabled && CheckEnabled(switchName, LogLevel.Trace, out switch2))
            {
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < messages.Length; i++)
                {
                    string str;
                    try
                    {
                        if (messages[i] == null)
                        {
                            str = "<null>";
                        }
                        else
                        {
                            str = messages[i].ToString();
                        }
                    }
                    catch
                    {
                        str = "<unable to convert>";
                    }
                    builder.Append(str);
                }
                builder.Append(Environment.NewLine);
                System.Diagnostics.Log.LogMessage(LoggingLevels.TraceLevel0, switch2, builder.ToString());
            }
        }

        [Conditional("_LOGGING")]
        public static void Trace(string switchName, string format, params object[] messages)
        {
            LogSwitch switch2;
            if (!m_loggingNotEnabled && CheckEnabled(switchName, LogLevel.Trace, out switch2))
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendFormat(format, messages);
                builder.Append(Environment.NewLine);
                System.Diagnostics.Log.LogMessage(LoggingLevels.TraceLevel0, switch2, builder.ToString());
            }
        }

        internal static bool SafeHandleStackTracesEnabled
        {
            get
            {
                return false;
            }
        }
    }
}

