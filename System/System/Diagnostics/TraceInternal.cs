namespace System.Diagnostics
{
    using System;
    using System.IO;
    using System.Security.Permissions;

    internal static class TraceInternal
    {
        private static string appName = null;
        private static bool autoFlush;
        internal static readonly object critSec = new object();
        private static bool defaultInitialized;
        [ThreadStatic]
        private static int indentLevel;
        private static int indentSize;
        private static TraceListenerCollection listeners;
        private static bool settingsInitialized;
        private static bool useGlobalLock;

        public static void Assert(bool condition)
        {
            if (!condition)
            {
                Fail(string.Empty);
            }
        }

        public static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                Fail(message);
            }
        }

        public static void Assert(bool condition, string message, string detailMessage)
        {
            if (!condition)
            {
                Fail(message, detailMessage);
            }
        }

        public static void Close()
        {
            if (listeners != null)
            {
                lock (critSec)
                {
                    foreach (TraceListener listener in Listeners)
                    {
                        listener.Close();
                    }
                }
            }
        }

        public static void Fail(string message)
        {
            if (UseGlobalLock)
            {
                lock (critSec)
                {
                    foreach (TraceListener listener in Listeners)
                    {
                        listener.Fail(message);
                        if (AutoFlush)
                        {
                            listener.Flush();
                        }
                    }
                    return;
                }
            }
            foreach (TraceListener listener2 in Listeners)
            {
                if (!listener2.IsThreadSafe)
                {
                    lock (listener2)
                    {
                        listener2.Fail(message);
                        if (AutoFlush)
                        {
                            listener2.Flush();
                        }
                        continue;
                    }
                }
                listener2.Fail(message);
                if (AutoFlush)
                {
                    listener2.Flush();
                }
            }
        }

        public static void Fail(string message, string detailMessage)
        {
            if (UseGlobalLock)
            {
                lock (critSec)
                {
                    foreach (TraceListener listener in Listeners)
                    {
                        listener.Fail(message, detailMessage);
                        if (AutoFlush)
                        {
                            listener.Flush();
                        }
                    }
                    return;
                }
            }
            foreach (TraceListener listener2 in Listeners)
            {
                if (!listener2.IsThreadSafe)
                {
                    lock (listener2)
                    {
                        listener2.Fail(message, detailMessage);
                        if (AutoFlush)
                        {
                            listener2.Flush();
                        }
                        continue;
                    }
                }
                listener2.Fail(message, detailMessage);
                if (AutoFlush)
                {
                    listener2.Flush();
                }
            }
        }

        public static void Flush()
        {
            if (listeners != null)
            {
                if (UseGlobalLock)
                {
                    lock (critSec)
                    {
                        foreach (TraceListener listener in Listeners)
                        {
                            listener.Flush();
                        }
                        return;
                    }
                }
                foreach (TraceListener listener2 in Listeners)
                {
                    if (!listener2.IsThreadSafe)
                    {
                        lock (listener2)
                        {
                            listener2.Flush();
                            continue;
                        }
                    }
                    listener2.Flush();
                }
            }
        }

        public static void Indent()
        {
            lock (critSec)
            {
                InitializeSettings();
                if (indentLevel < 0x7fffffff)
                {
                    indentLevel++;
                }
                foreach (TraceListener listener in Listeners)
                {
                    listener.IndentLevel = indentLevel;
                }
            }
        }

        private static void InitializeSettings()
        {
            if (!settingsInitialized || (defaultInitialized && DiagnosticsConfiguration.IsInitialized()))
            {
                lock (critSec)
                {
                    if (!settingsInitialized || (defaultInitialized && DiagnosticsConfiguration.IsInitialized()))
                    {
                        defaultInitialized = DiagnosticsConfiguration.IsInitializing();
                        SetIndentSize(DiagnosticsConfiguration.IndentSize);
                        autoFlush = DiagnosticsConfiguration.AutoFlush;
                        useGlobalLock = DiagnosticsConfiguration.UseGlobalLock;
                        settingsInitialized = true;
                    }
                }
            }
        }

        internal static void Refresh()
        {
            lock (critSec)
            {
                settingsInitialized = false;
                listeners = null;
            }
            InitializeSettings();
        }

        private static void SetIndentSize(int value)
        {
            lock (critSec)
            {
                if (value < 0)
                {
                    value = 0;
                }
                indentSize = value;
                if (listeners != null)
                {
                    foreach (TraceListener listener in Listeners)
                    {
                        listener.IndentSize = indentSize;
                    }
                }
            }
        }

        public static void TraceEvent(TraceEventType eventType, int id, string format, params object[] args)
        {
            TraceEventCache eventCache = new TraceEventCache();
            if (UseGlobalLock)
            {
                lock (critSec)
                {
                    if (args == null)
                    {
                        foreach (TraceListener listener in Listeners)
                        {
                            listener.TraceEvent(eventCache, AppName, eventType, id, format);
                            if (AutoFlush)
                            {
                                listener.Flush();
                            }
                        }
                    }
                    else
                    {
                        foreach (TraceListener listener2 in Listeners)
                        {
                            listener2.TraceEvent(eventCache, AppName, eventType, id, format, args);
                            if (AutoFlush)
                            {
                                listener2.Flush();
                            }
                        }
                    }
                    return;
                }
            }
            if (args == null)
            {
                foreach (TraceListener listener3 in Listeners)
                {
                    if (!listener3.IsThreadSafe)
                    {
                        lock (listener3)
                        {
                            listener3.TraceEvent(eventCache, AppName, eventType, id, format);
                            if (AutoFlush)
                            {
                                listener3.Flush();
                            }
                            continue;
                        }
                    }
                    listener3.TraceEvent(eventCache, AppName, eventType, id, format);
                    if (AutoFlush)
                    {
                        listener3.Flush();
                    }
                }
            }
            else
            {
                foreach (TraceListener listener4 in Listeners)
                {
                    if (!listener4.IsThreadSafe)
                    {
                        lock (listener4)
                        {
                            listener4.TraceEvent(eventCache, AppName, eventType, id, format, args);
                            if (AutoFlush)
                            {
                                listener4.Flush();
                            }
                            continue;
                        }
                    }
                    listener4.TraceEvent(eventCache, AppName, eventType, id, format, args);
                    if (AutoFlush)
                    {
                        listener4.Flush();
                    }
                }
            }
        }

        public static void Unindent()
        {
            lock (critSec)
            {
                InitializeSettings();
                if (indentLevel > 0)
                {
                    indentLevel--;
                }
                foreach (TraceListener listener in Listeners)
                {
                    listener.IndentLevel = indentLevel;
                }
            }
        }

        public static void Write(object value)
        {
            if (UseGlobalLock)
            {
                lock (critSec)
                {
                    foreach (TraceListener listener in Listeners)
                    {
                        listener.Write(value);
                        if (AutoFlush)
                        {
                            listener.Flush();
                        }
                    }
                    return;
                }
            }
            foreach (TraceListener listener2 in Listeners)
            {
                if (!listener2.IsThreadSafe)
                {
                    lock (listener2)
                    {
                        listener2.Write(value);
                        if (AutoFlush)
                        {
                            listener2.Flush();
                        }
                        continue;
                    }
                }
                listener2.Write(value);
                if (AutoFlush)
                {
                    listener2.Flush();
                }
            }
        }

        public static void Write(string message)
        {
            if (UseGlobalLock)
            {
                lock (critSec)
                {
                    foreach (TraceListener listener in Listeners)
                    {
                        listener.Write(message);
                        if (AutoFlush)
                        {
                            listener.Flush();
                        }
                    }
                    return;
                }
            }
            foreach (TraceListener listener2 in Listeners)
            {
                if (!listener2.IsThreadSafe)
                {
                    lock (listener2)
                    {
                        listener2.Write(message);
                        if (AutoFlush)
                        {
                            listener2.Flush();
                        }
                        continue;
                    }
                }
                listener2.Write(message);
                if (AutoFlush)
                {
                    listener2.Flush();
                }
            }
        }

        public static void Write(object value, string category)
        {
            if (UseGlobalLock)
            {
                lock (critSec)
                {
                    foreach (TraceListener listener in Listeners)
                    {
                        listener.Write(value, category);
                        if (AutoFlush)
                        {
                            listener.Flush();
                        }
                    }
                    return;
                }
            }
            foreach (TraceListener listener2 in Listeners)
            {
                if (!listener2.IsThreadSafe)
                {
                    lock (listener2)
                    {
                        listener2.Write(value, category);
                        if (AutoFlush)
                        {
                            listener2.Flush();
                        }
                        continue;
                    }
                }
                listener2.Write(value, category);
                if (AutoFlush)
                {
                    listener2.Flush();
                }
            }
        }

        public static void Write(string message, string category)
        {
            if (UseGlobalLock)
            {
                lock (critSec)
                {
                    foreach (TraceListener listener in Listeners)
                    {
                        listener.Write(message, category);
                        if (AutoFlush)
                        {
                            listener.Flush();
                        }
                    }
                    return;
                }
            }
            foreach (TraceListener listener2 in Listeners)
            {
                if (!listener2.IsThreadSafe)
                {
                    lock (listener2)
                    {
                        listener2.Write(message, category);
                        if (AutoFlush)
                        {
                            listener2.Flush();
                        }
                        continue;
                    }
                }
                listener2.Write(message, category);
                if (AutoFlush)
                {
                    listener2.Flush();
                }
            }
        }

        public static void WriteIf(bool condition, object value)
        {
            if (condition)
            {
                Write(value);
            }
        }

        public static void WriteIf(bool condition, string message)
        {
            if (condition)
            {
                Write(message);
            }
        }

        public static void WriteIf(bool condition, object value, string category)
        {
            if (condition)
            {
                Write(value, category);
            }
        }

        public static void WriteIf(bool condition, string message, string category)
        {
            if (condition)
            {
                Write(message, category);
            }
        }

        public static void WriteLine(object value)
        {
            if (UseGlobalLock)
            {
                lock (critSec)
                {
                    foreach (TraceListener listener in Listeners)
                    {
                        listener.WriteLine(value);
                        if (AutoFlush)
                        {
                            listener.Flush();
                        }
                    }
                    return;
                }
            }
            foreach (TraceListener listener2 in Listeners)
            {
                if (!listener2.IsThreadSafe)
                {
                    lock (listener2)
                    {
                        listener2.WriteLine(value);
                        if (AutoFlush)
                        {
                            listener2.Flush();
                        }
                        continue;
                    }
                }
                listener2.WriteLine(value);
                if (AutoFlush)
                {
                    listener2.Flush();
                }
            }
        }

        public static void WriteLine(string message)
        {
            if (UseGlobalLock)
            {
                lock (critSec)
                {
                    foreach (TraceListener listener in Listeners)
                    {
                        listener.WriteLine(message);
                        if (AutoFlush)
                        {
                            listener.Flush();
                        }
                    }
                    return;
                }
            }
            foreach (TraceListener listener2 in Listeners)
            {
                if (!listener2.IsThreadSafe)
                {
                    lock (listener2)
                    {
                        listener2.WriteLine(message);
                        if (AutoFlush)
                        {
                            listener2.Flush();
                        }
                        continue;
                    }
                }
                listener2.WriteLine(message);
                if (AutoFlush)
                {
                    listener2.Flush();
                }
            }
        }

        public static void WriteLine(object value, string category)
        {
            if (UseGlobalLock)
            {
                lock (critSec)
                {
                    foreach (TraceListener listener in Listeners)
                    {
                        listener.WriteLine(value, category);
                        if (AutoFlush)
                        {
                            listener.Flush();
                        }
                    }
                    return;
                }
            }
            foreach (TraceListener listener2 in Listeners)
            {
                if (!listener2.IsThreadSafe)
                {
                    lock (listener2)
                    {
                        listener2.WriteLine(value, category);
                        if (AutoFlush)
                        {
                            listener2.Flush();
                        }
                        continue;
                    }
                }
                listener2.WriteLine(value, category);
                if (AutoFlush)
                {
                    listener2.Flush();
                }
            }
        }

        public static void WriteLine(string message, string category)
        {
            if (UseGlobalLock)
            {
                lock (critSec)
                {
                    foreach (TraceListener listener in Listeners)
                    {
                        listener.WriteLine(message, category);
                        if (AutoFlush)
                        {
                            listener.Flush();
                        }
                    }
                    return;
                }
            }
            foreach (TraceListener listener2 in Listeners)
            {
                if (!listener2.IsThreadSafe)
                {
                    lock (listener2)
                    {
                        listener2.WriteLine(message, category);
                        if (AutoFlush)
                        {
                            listener2.Flush();
                        }
                        continue;
                    }
                }
                listener2.WriteLine(message, category);
                if (AutoFlush)
                {
                    listener2.Flush();
                }
            }
        }

        public static void WriteLineIf(bool condition, object value)
        {
            if (condition)
            {
                WriteLine(value);
            }
        }

        public static void WriteLineIf(bool condition, string message)
        {
            if (condition)
            {
                WriteLine(message);
            }
        }

        public static void WriteLineIf(bool condition, object value, string category)
        {
            if (condition)
            {
                WriteLine(value, category);
            }
        }

        public static void WriteLineIf(bool condition, string message, string category)
        {
            if (condition)
            {
                WriteLine(message, category);
            }
        }

        internal static string AppName
        {
            get
            {
                if (appName == null)
                {
                    new EnvironmentPermission(EnvironmentPermissionAccess.Read, "Path").Assert();
                    appName = Path.GetFileName(Environment.GetCommandLineArgs()[0]);
                }
                return appName;
            }
        }

        public static bool AutoFlush
        {
            get
            {
                InitializeSettings();
                return autoFlush;
            }
            set
            {
                InitializeSettings();
                autoFlush = value;
            }
        }

        public static int IndentLevel
        {
            get
            {
                return indentLevel;
            }
            set
            {
                lock (critSec)
                {
                    if (value < 0)
                    {
                        value = 0;
                    }
                    indentLevel = value;
                    if (listeners != null)
                    {
                        foreach (TraceListener listener in Listeners)
                        {
                            listener.IndentLevel = indentLevel;
                        }
                    }
                }
            }
        }

        public static int IndentSize
        {
            get
            {
                InitializeSettings();
                return indentSize;
            }
            set
            {
                InitializeSettings();
                SetIndentSize(value);
            }
        }

        public static TraceListenerCollection Listeners
        {
            get
            {
                InitializeSettings();
                if (listeners == null)
                {
                    lock (critSec)
                    {
                        if (listeners == null)
                        {
                            SystemDiagnosticsSection systemDiagnosticsSection = DiagnosticsConfiguration.SystemDiagnosticsSection;
                            if (systemDiagnosticsSection != null)
                            {
                                listeners = systemDiagnosticsSection.Trace.Listeners.GetRuntimeObject();
                            }
                            else
                            {
                                listeners = new TraceListenerCollection();
                                TraceListener listener = new DefaultTraceListener {
                                    IndentLevel = indentLevel,
                                    IndentSize = indentSize
                                };
                                listeners.Add(listener);
                            }
                        }
                    }
                }
                return listeners;
            }
        }

        public static bool UseGlobalLock
        {
            get
            {
                InitializeSettings();
                return useGlobalLock;
            }
            set
            {
                InitializeSettings();
                useGlobalLock = value;
            }
        }
    }
}

