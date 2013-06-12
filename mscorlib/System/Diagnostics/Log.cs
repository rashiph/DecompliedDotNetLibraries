namespace System.Diagnostics
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Security;

    internal static class Log
    {
        private static LogMessageEventHandler _LogMessageEventHandler;
        private static LogSwitchLevelHandler _LogSwitchLevelHandler;
        public static readonly LogSwitch GlobalSwitch = new LogSwitch("Global", "Global Switch for this log");
        internal static int iNumOfSwitches;
        private static object locker = new object();
        private static bool m_fConsoleDeviceEnabled = false;
        internal static Hashtable m_Hashtable = new Hashtable();
        private static int m_iNumOfStreamDevices = 0;
        private static int m_iStreamArraySize = 0;
        private static Stream[] m_rgStream = null;

        static Log()
        {
            GlobalSwitch.MinimumLevel = LoggingLevels.ErrorLevel;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern void AddLogSwitch(LogSwitch logSwitch);
        public static void AddOnLogMessage(LogMessageEventHandler handler)
        {
            lock (locker)
            {
                _LogMessageEventHandler = (LogMessageEventHandler) Delegate.Combine(_LogMessageEventHandler, handler);
            }
        }

        public static void AddOnLogSwitchLevel(LogSwitchLevelHandler handler)
        {
            lock (locker)
            {
                _LogSwitchLevelHandler = (LogSwitchLevelHandler) Delegate.Combine(_LogSwitchLevelHandler, handler);
            }
        }

        public static void AddStream(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (m_iStreamArraySize <= m_iNumOfStreamDevices)
            {
                Stream[] destinationArray = new Stream[m_iStreamArraySize + 4];
                if (m_iNumOfStreamDevices > 0)
                {
                    Array.Copy(m_rgStream, destinationArray, m_iNumOfStreamDevices);
                }
                m_iStreamArraySize += 4;
                m_rgStream = destinationArray;
            }
            m_rgStream[m_iNumOfStreamDevices++] = stream;
        }

        public static void Error(string message)
        {
            LogMessage(LoggingLevels.ErrorLevel, GlobalSwitch, message);
        }

        public static void Error(LogSwitch logswitch, string message)
        {
            LogMessage(LoggingLevels.ErrorLevel, logswitch, message);
        }

        public static void Error(string switchname, string message)
        {
            LogSwitch logswitch = LogSwitch.GetSwitch(switchname);
            LogMessage(LoggingLevels.ErrorLevel, logswitch, message);
        }

        internal static void InvokeLogSwitchLevelHandlers(LogSwitch ls, LoggingLevels newLevel)
        {
            LogSwitchLevelHandler handler = _LogSwitchLevelHandler;
            if (handler != null)
            {
                handler(ls, newLevel);
            }
        }

        public static void LogMessage(LoggingLevels level, string message)
        {
            LogMessage(level, GlobalSwitch, message);
        }

        [SecuritySafeCritical]
        public static void LogMessage(LoggingLevels level, LogSwitch logswitch, string message)
        {
            if (logswitch == null)
            {
                throw new ArgumentNullException("LogSwitch");
            }
            if (level < LoggingLevels.TraceLevel0)
            {
                throw new ArgumentOutOfRangeException("level", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (logswitch.CheckLevel(level))
            {
                Debugger.Log((int) level, logswitch.strName, message);
                if (m_fConsoleDeviceEnabled)
                {
                    Console.Write(message);
                }
                for (int i = 0; i < m_iNumOfStreamDevices; i++)
                {
                    StreamWriter writer = new StreamWriter(m_rgStream[i]);
                    writer.Write(message);
                    writer.Flush();
                }
            }
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern void ModifyLogSwitch(int iNewLevel, string strSwitchName, string strParentName);
        public static void Panic(string message)
        {
            LogMessage(LoggingLevels.PanicLevel, GlobalSwitch, message);
        }

        public static void RemoveOnLogMessage(LogMessageEventHandler handler)
        {
            lock (locker)
            {
                _LogMessageEventHandler = (LogMessageEventHandler) Delegate.Remove(_LogMessageEventHandler, handler);
            }
        }

        public static void RemoveOnLogSwitchLevel(LogSwitchLevelHandler handler)
        {
            lock (locker)
            {
                _LogSwitchLevelHandler = (LogSwitchLevelHandler) Delegate.Remove(_LogSwitchLevelHandler, handler);
            }
        }

        public static void Status(string message)
        {
            LogMessage(LoggingLevels.StatusLevel0, GlobalSwitch, message);
        }

        public static void Status(LogSwitch logswitch, string message)
        {
            LogMessage(LoggingLevels.StatusLevel0, logswitch, message);
        }

        public static void Status(string switchname, string message)
        {
            LogSwitch logswitch = LogSwitch.GetSwitch(switchname);
            LogMessage(LoggingLevels.StatusLevel0, logswitch, message);
        }

        public static void Trace(string message)
        {
            LogMessage(LoggingLevels.TraceLevel0, GlobalSwitch, message);
        }

        public static void Trace(LogSwitch logswitch, string message)
        {
            LogMessage(LoggingLevels.TraceLevel0, logswitch, message);
        }

        public static void Trace(string switchname, string message)
        {
            LogSwitch logswitch = LogSwitch.GetSwitch(switchname);
            LogMessage(LoggingLevels.TraceLevel0, logswitch, message);
        }

        public static void Warning(string message)
        {
            LogMessage(LoggingLevels.WarningLevel, GlobalSwitch, message);
        }

        public static void Warning(LogSwitch logswitch, string message)
        {
            LogMessage(LoggingLevels.WarningLevel, logswitch, message);
        }

        public static void Warning(string switchname, string message)
        {
            LogSwitch logswitch = LogSwitch.GetSwitch(switchname);
            LogMessage(LoggingLevels.WarningLevel, logswitch, message);
        }

        public static bool IsConsoleEnabled
        {
            get
            {
                return m_fConsoleDeviceEnabled;
            }
            set
            {
                m_fConsoleDeviceEnabled = value;
            }
        }
    }
}

