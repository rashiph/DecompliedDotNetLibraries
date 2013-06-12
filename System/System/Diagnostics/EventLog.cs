namespace System.Diagnostics
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;

    [InstallerType("System.Diagnostics.EventLogInstaller, System.Configuration.Install, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultEvent("EntryWritten"), MonitoringDescription("EventLogDesc")]
    public class EventLog : Component, ISupportInitialize
    {
        private const int DefaultMaxSize = 0x80000;
        private const int DefaultRetention = 0x93a80;
        internal const string DllName = "EventLogMessages.dll";
        private const string EventLogKey = @"SYSTEM\CurrentControlSet\Services\EventLog";
        private const string eventLogMutexName = "netfxeventlog.1.0";
        private EventLogInternal m_underlyingEventLog;
        private static bool s_CheckedOsVersion;
        private static bool s_SkipRegPatch;
        private const int SecondsPerDay = 0x15180;

        [MonitoringDescription("LogEntryWritten")]
        public event EntryWrittenEventHandler EntryWritten
        {
            add
            {
                this.m_underlyingEventLog.EntryWritten += value;
            }
            remove
            {
                this.m_underlyingEventLog.EntryWritten -= value;
            }
        }

        public EventLog() : this("", ".", "")
        {
        }

        public EventLog(string logName) : this(logName, ".", "")
        {
        }

        public EventLog(string logName, string machineName) : this(logName, machineName, "")
        {
        }

        public EventLog(string logName, string machineName, string source)
        {
            this.m_underlyingEventLog = new EventLogInternal(logName, machineName, source, this);
        }

        private static string _InternalLogNameFromSourceName(string source, string machineName)
        {
            using (RegistryKey key = FindSourceRegistration(source, machineName, true))
            {
                if (key == null)
                {
                    return "";
                }
                string name = key.Name;
                int num = name.LastIndexOf('\\');
                return name.Substring(num + 1);
            }
        }

        internal static PermissionSet _UnsafeGetAssertPermSet()
        {
            PermissionSet set = new PermissionSet(PermissionState.None);
            RegistryPermission perm = new RegistryPermission(PermissionState.Unrestricted);
            set.AddPermission(perm);
            EnvironmentPermission permission2 = new EnvironmentPermission(PermissionState.Unrestricted);
            set.AddPermission(permission2);
            SecurityPermission permission3 = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
            set.AddPermission(permission3);
            return set;
        }

        public void BeginInit()
        {
            this.m_underlyingEventLog.BeginInit();
        }

        private static bool CharIsPrintable(char c)
        {
            UnicodeCategory unicodeCategory = char.GetUnicodeCategory(c);
            if (((unicodeCategory == UnicodeCategory.Control) && (unicodeCategory != UnicodeCategory.Format)) && ((unicodeCategory != UnicodeCategory.LineSeparator) && (unicodeCategory != UnicodeCategory.ParagraphSeparator)))
            {
                return (unicodeCategory == UnicodeCategory.OtherNotAssigned);
            }
            return true;
        }

        private static string CheckAndNormalizeSourceName(string source)
        {
            if (source == null)
            {
                source = string.Empty;
            }
            if ((source.Length + @"SYSTEM\CurrentControlSet\Services\EventLog".Length) > 0xfe)
            {
                throw new ArgumentException(SR.GetString("ParameterTooLong", new object[] { "source", 0xfe - @"SYSTEM\CurrentControlSet\Services\EventLog".Length }));
            }
            return source;
        }

        public void Clear()
        {
            this.m_underlyingEventLog.Clear();
        }

        public void Close()
        {
            this.m_underlyingEventLog.Close();
        }

        internal object ComponentGetService(Type service)
        {
            return this.GetService(service);
        }

        public static void CreateEventSource(EventSourceCreationData sourceData)
        {
            if (sourceData == null)
            {
                throw new ArgumentNullException("sourceData");
            }
            string logName = sourceData.LogName;
            string source = sourceData.Source;
            string machineName = sourceData.MachineName;
            if (!SyntaxCheck.CheckMachineName(machineName))
            {
                throw new ArgumentException(SR.GetString("InvalidParameter", new object[] { "machineName", machineName }));
            }
            if ((logName == null) || (logName.Length == 0))
            {
                logName = "Application";
            }
            if (!ValidLogName(logName, false))
            {
                throw new ArgumentException(SR.GetString("BadLogName"));
            }
            if ((source == null) || (source.Length == 0))
            {
                throw new ArgumentException(SR.GetString("MissingParameter", new object[] { "source" }));
            }
            if ((source.Length + @"SYSTEM\CurrentControlSet\Services\EventLog".Length) > 0xfe)
            {
                throw new ArgumentException(SR.GetString("ParameterTooLong", new object[] { "source", 0xfe - @"SYSTEM\CurrentControlSet\Services\EventLog".Length }));
            }
            new EventLogPermission(EventLogPermissionAccess.Administer, machineName).Demand();
            Mutex mutex = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                SharedUtils.EnterMutex("netfxeventlog.1.0", ref mutex);
                if (SourceExists(source, machineName, true))
                {
                    if (".".Equals(machineName))
                    {
                        throw new ArgumentException(SR.GetString("LocalSourceAlreadyExists", new object[] { source }));
                    }
                    throw new ArgumentException(SR.GetString("SourceAlreadyExists", new object[] { source, machineName }));
                }
                _UnsafeGetAssertPermSet().Assert();
                RegistryKey localMachine = null;
                RegistryKey keyParent = null;
                RegistryKey logKey = null;
                RegistryKey sourceLogKey = null;
                RegistryKey key5 = null;
                try
                {
                    if (machineName == ".")
                    {
                        localMachine = Registry.LocalMachine;
                    }
                    else
                    {
                        localMachine = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, machineName);
                    }
                    keyParent = localMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\EventLog", true);
                    if (keyParent == null)
                    {
                        if (!".".Equals(machineName))
                        {
                            throw new InvalidOperationException(SR.GetString("RegKeyMissing", new object[] { @"SYSTEM\CurrentControlSet\Services\EventLog", logName, source, machineName }));
                        }
                        throw new InvalidOperationException(SR.GetString("LocalRegKeyMissing", new object[] { @"SYSTEM\CurrentControlSet\Services\EventLog", logName, source }));
                    }
                    logKey = keyParent.OpenSubKey(logName, true);
                    if ((logKey == null) && (logName.Length >= 8))
                    {
                        string strA = logName.Substring(0, 8);
                        if (((string.Compare(strA, "AppEvent", StringComparison.OrdinalIgnoreCase) == 0) || (string.Compare(strA, "SecEvent", StringComparison.OrdinalIgnoreCase) == 0)) || (string.Compare(strA, "SysEvent", StringComparison.OrdinalIgnoreCase) == 0))
                        {
                            throw new ArgumentException(SR.GetString("InvalidCustomerLogName", new object[] { logName }));
                        }
                        string str5 = FindSame8FirstCharsLog(keyParent, logName);
                        if (str5 != null)
                        {
                            throw new ArgumentException(SR.GetString("DuplicateLogName", new object[] { logName, str5 }));
                        }
                    }
                    bool flag = logKey == null;
                    if (flag)
                    {
                        if (SourceExists(logName, machineName, true))
                        {
                            if (".".Equals(machineName))
                            {
                                throw new ArgumentException(SR.GetString("LocalLogAlreadyExistsAsSource", new object[] { logName }));
                            }
                            throw new ArgumentException(SR.GetString("LogAlreadyExistsAsSource", new object[] { logName, machineName }));
                        }
                        logKey = keyParent.CreateSubKey(logName);
                        if (!SkipRegPatch)
                        {
                            logKey.SetValue("Sources", new string[] { logName, source }, RegistryValueKind.MultiString);
                        }
                        SetSpecialLogRegValues(logKey, logName);
                        sourceLogKey = logKey.CreateSubKey(logName);
                        SetSpecialSourceRegValues(sourceLogKey, sourceData);
                    }
                    if (logName != source)
                    {
                        if (!flag)
                        {
                            SetSpecialLogRegValues(logKey, logName);
                            if (!SkipRegPatch)
                            {
                                string[] array = logKey.GetValue("Sources") as string[];
                                if (array == null)
                                {
                                    logKey.SetValue("Sources", new string[] { logName, source }, RegistryValueKind.MultiString);
                                }
                                else if (Array.IndexOf<string>(array, source) == -1)
                                {
                                    string[] destinationArray = new string[array.Length + 1];
                                    Array.Copy(array, destinationArray, array.Length);
                                    destinationArray[array.Length] = source;
                                    logKey.SetValue("Sources", destinationArray, RegistryValueKind.MultiString);
                                }
                            }
                        }
                        key5 = logKey.CreateSubKey(source);
                        SetSpecialSourceRegValues(key5, sourceData);
                    }
                }
                finally
                {
                    if (localMachine != null)
                    {
                        localMachine.Close();
                    }
                    if (keyParent != null)
                    {
                        keyParent.Close();
                    }
                    if (logKey != null)
                    {
                        logKey.Flush();
                        logKey.Close();
                    }
                    if (sourceLogKey != null)
                    {
                        sourceLogKey.Flush();
                        sourceLogKey.Close();
                    }
                    if (key5 != null)
                    {
                        key5.Flush();
                        key5.Close();
                    }
                    CodeAccessPermission.RevertAssert();
                }
            }
            finally
            {
                if (mutex != null)
                {
                    mutex.ReleaseMutex();
                    mutex.Close();
                }
            }
        }

        public static void CreateEventSource(string source, string logName)
        {
            CreateEventSource(new EventSourceCreationData(source, logName, "."));
        }

        [Obsolete("This method has been deprecated.  Please use System.Diagnostics.EventLog.CreateEventSource(EventSourceCreationData sourceData) instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
        public static void CreateEventSource(string source, string logName, string machineName)
        {
            CreateEventSource(new EventSourceCreationData(source, logName, machineName));
        }

        public static void Delete(string logName)
        {
            Delete(logName, ".");
        }

        public static void Delete(string logName, string machineName)
        {
            if (!SyntaxCheck.CheckMachineName(machineName))
            {
                throw new ArgumentException(SR.GetString("InvalidParameterFormat", new object[] { "machineName" }));
            }
            if ((logName == null) || (logName.Length == 0))
            {
                throw new ArgumentException(SR.GetString("NoLogName"));
            }
            if (!ValidLogName(logName, false))
            {
                throw new InvalidOperationException(SR.GetString("BadLogName"));
            }
            new EventLogPermission(EventLogPermissionAccess.Administer, machineName).Demand();
            SharedUtils.CheckEnvironment();
            _UnsafeGetAssertPermSet().Assert();
            RegistryKey eventLogRegKey = null;
            Mutex mutex = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                SharedUtils.EnterMutex("netfxeventlog.1.0", ref mutex);
                try
                {
                    eventLogRegKey = GetEventLogRegKey(machineName, true);
                    if (eventLogRegKey == null)
                    {
                        throw new InvalidOperationException(SR.GetString("RegKeyNoAccess", new object[] { @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\EventLog", machineName }));
                    }
                    using (RegistryKey key2 = eventLogRegKey.OpenSubKey(logName))
                    {
                        if (key2 == null)
                        {
                            throw new InvalidOperationException(SR.GetString("MissingLog", new object[] { logName, machineName }));
                        }
                        EventLog log = new EventLog(logName, machineName);
                        try
                        {
                            log.Clear();
                        }
                        finally
                        {
                            log.Close();
                        }
                        string path = null;
                        try
                        {
                            path = (string) key2.GetValue("File");
                        }
                        catch
                        {
                        }
                        if (path != null)
                        {
                            try
                            {
                                File.Delete(path);
                            }
                            catch
                            {
                            }
                        }
                    }
                    eventLogRegKey.DeleteSubKeyTree(logName);
                }
                finally
                {
                    if (eventLogRegKey != null)
                    {
                        eventLogRegKey.Close();
                    }
                    CodeAccessPermission.RevertAssert();
                }
            }
            finally
            {
                if (mutex != null)
                {
                    mutex.ReleaseMutex();
                }
            }
        }

        public static void DeleteEventSource(string source)
        {
            DeleteEventSource(source, ".");
        }

        public static void DeleteEventSource(string source, string machineName)
        {
            if (!SyntaxCheck.CheckMachineName(machineName))
            {
                throw new ArgumentException(SR.GetString("InvalidParameter", new object[] { "machineName", machineName }));
            }
            new EventLogPermission(EventLogPermissionAccess.Administer, machineName).Demand();
            SharedUtils.CheckEnvironment();
            _UnsafeGetAssertPermSet().Assert();
            Mutex mutex = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                SharedUtils.EnterMutex("netfxeventlog.1.0", ref mutex);
                RegistryKey key = null;
                using (key = FindSourceRegistration(source, machineName, true))
                {
                    if (key == null)
                    {
                        if (machineName == null)
                        {
                            throw new ArgumentException(SR.GetString("LocalSourceNotRegistered", new object[] { source }));
                        }
                        throw new ArgumentException(SR.GetString("SourceNotRegistered", new object[] { source, machineName, @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\EventLog" }));
                    }
                    string name = key.Name;
                    int num = name.LastIndexOf('\\');
                    if (string.Compare(name, num + 1, source, 0, name.Length - num, StringComparison.Ordinal) == 0)
                    {
                        throw new InvalidOperationException(SR.GetString("CannotDeleteEqualSource", new object[] { source }));
                    }
                }
                try
                {
                    key = FindSourceRegistration(source, machineName, false);
                    key.DeleteSubKeyTree(source);
                    if (!SkipRegPatch)
                    {
                        string[] strArray = (string[]) key.GetValue("Sources");
                        ArrayList list = new ArrayList(strArray.Length - 1);
                        for (int i = 0; i < strArray.Length; i++)
                        {
                            if (strArray[i] != source)
                            {
                                list.Add(strArray[i]);
                            }
                        }
                        string[] array = new string[list.Count];
                        list.CopyTo(array);
                        key.SetValue("Sources", array, RegistryValueKind.MultiString);
                    }
                }
                finally
                {
                    if (key != null)
                    {
                        key.Flush();
                        key.Close();
                    }
                    CodeAccessPermission.RevertAssert();
                }
            }
            finally
            {
                if (mutex != null)
                {
                    mutex.ReleaseMutex();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (this.m_underlyingEventLog != null)
            {
                this.m_underlyingEventLog.Dispose(disposing);
            }
            base.Dispose(disposing);
        }

        public void EndInit()
        {
            this.m_underlyingEventLog.EndInit();
        }

        public static bool Exists(string logName)
        {
            return Exists(logName, ".");
        }

        public static bool Exists(string logName, string machineName)
        {
            bool flag;
            if (!SyntaxCheck.CheckMachineName(machineName))
            {
                throw new ArgumentException(SR.GetString("InvalidParameterFormat", new object[] { "machineName" }));
            }
            new EventLogPermission(EventLogPermissionAccess.Administer, machineName).Demand();
            if ((logName == null) || (logName.Length == 0))
            {
                return false;
            }
            SharedUtils.CheckEnvironment();
            _UnsafeGetAssertPermSet().Assert();
            RegistryKey eventLogRegKey = null;
            RegistryKey key2 = null;
            try
            {
                eventLogRegKey = GetEventLogRegKey(machineName, false);
                if (eventLogRegKey == null)
                {
                    return false;
                }
                key2 = eventLogRegKey.OpenSubKey(logName, false);
                flag = key2 != null;
            }
            finally
            {
                if (eventLogRegKey != null)
                {
                    eventLogRegKey.Close();
                }
                if (key2 != null)
                {
                    key2.Close();
                }
                CodeAccessPermission.RevertAssert();
            }
            return flag;
        }

        private static string FindSame8FirstCharsLog(RegistryKey keyParent, string logName)
        {
            string strB = logName.Substring(0, 8);
            foreach (string str2 in keyParent.GetSubKeyNames())
            {
                if ((str2.Length >= 8) && (string.Compare(str2.Substring(0, 8), strB, StringComparison.OrdinalIgnoreCase) == 0))
                {
                    return str2;
                }
            }
            return null;
        }

        private static RegistryKey FindSourceRegistration(string source, string machineName, bool readOnly)
        {
            return FindSourceRegistration(source, machineName, readOnly, false);
        }

        private static RegistryKey FindSourceRegistration(string source, string machineName, bool readOnly, bool wantToCreate)
        {
            if ((source != null) && (source.Length != 0))
            {
                SharedUtils.CheckEnvironment();
                _UnsafeGetAssertPermSet().Assert();
                RegistryKey eventLogRegKey = null;
                try
                {
                    eventLogRegKey = GetEventLogRegKey(machineName, !readOnly);
                    if (eventLogRegKey == null)
                    {
                        return null;
                    }
                    StringBuilder builder = null;
                    string[] subKeyNames = eventLogRegKey.GetSubKeyNames();
                    for (int i = 0; i < subKeyNames.Length; i++)
                    {
                        RegistryKey key2 = null;
                        try
                        {
                            RegistryKey key3 = eventLogRegKey.OpenSubKey(subKeyNames[i], !readOnly);
                            if (key3 != null)
                            {
                                key2 = key3.OpenSubKey(source, !readOnly);
                                if (key2 != null)
                                {
                                    return key3;
                                }
                            }
                        }
                        catch (UnauthorizedAccessException)
                        {
                            if (builder == null)
                            {
                                builder = new StringBuilder(subKeyNames[i]);
                            }
                            else
                            {
                                builder.Append(", ");
                                builder.Append(subKeyNames[i]);
                            }
                        }
                        catch (SecurityException)
                        {
                            if (builder == null)
                            {
                                builder = new StringBuilder(subKeyNames[i]);
                            }
                            else
                            {
                                builder.Append(", ");
                                builder.Append(subKeyNames[i]);
                            }
                        }
                        finally
                        {
                            if (key2 != null)
                            {
                                key2.Close();
                            }
                        }
                    }
                    if (builder != null)
                    {
                        throw new SecurityException(SR.GetString(wantToCreate ? "SomeLogsInaccessibleToCreate" : "SomeLogsInaccessible", new object[] { builder.ToString() }));
                    }
                }
                finally
                {
                    if (eventLogRegKey != null)
                    {
                        eventLogRegKey.Close();
                    }
                    CodeAccessPermission.RevertAssert();
                }
            }
            return null;
        }

        private static string FixupPath(string path)
        {
            if (path[0] == '%')
            {
                return path;
            }
            return Path.GetFullPath(path);
        }

        internal static string GetDllPath(string machineName)
        {
            return Path.Combine(SharedUtils.GetLatestBuildDllDirectory(machineName), "EventLogMessages.dll");
        }

        internal static RegistryKey GetEventLogRegKey(string machine, bool writable)
        {
            RegistryKey localMachine = null;
            try
            {
                if (machine.Equals("."))
                {
                    localMachine = Registry.LocalMachine;
                }
                else
                {
                    localMachine = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, machine);
                }
                if (localMachine != null)
                {
                    return localMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\EventLog", writable);
                }
            }
            finally
            {
                if (localMachine != null)
                {
                    localMachine.Close();
                }
            }
            return null;
        }

        public static EventLog[] GetEventLogs()
        {
            return GetEventLogs(".");
        }

        public static EventLog[] GetEventLogs(string machineName)
        {
            if (!SyntaxCheck.CheckMachineName(machineName))
            {
                throw new ArgumentException(SR.GetString("InvalidParameter", new object[] { "machineName", machineName }));
            }
            new EventLogPermission(EventLogPermissionAccess.Administer, machineName).Demand();
            SharedUtils.CheckEnvironment();
            string[] subKeyNames = new string[0];
            _UnsafeGetAssertPermSet().Assert();
            RegistryKey eventLogRegKey = null;
            try
            {
                eventLogRegKey = GetEventLogRegKey(machineName, false);
                if (eventLogRegKey == null)
                {
                    throw new InvalidOperationException(SR.GetString("RegKeyMissingShort", new object[] { @"SYSTEM\CurrentControlSet\Services\EventLog", machineName }));
                }
                subKeyNames = eventLogRegKey.GetSubKeyNames();
            }
            finally
            {
                if (eventLogRegKey != null)
                {
                    eventLogRegKey.Close();
                }
                CodeAccessPermission.RevertAssert();
            }
            EventLog[] logArray = new EventLog[subKeyNames.Length];
            for (int i = 0; i < subKeyNames.Length; i++)
            {
                logArray[i] = new EventLog(subKeyNames[i], machineName);
            }
            return logArray;
        }

        public static string LogNameFromSourceName(string source, string machineName)
        {
            new EventLogPermission(EventLogPermissionAccess.Administer, machineName).Demand();
            return _InternalLogNameFromSourceName(source, machineName);
        }

        [ComVisible(false)]
        public void ModifyOverflowPolicy(System.Diagnostics.OverflowAction action, int retentionDays)
        {
            this.m_underlyingEventLog.ModifyOverflowPolicy(action, retentionDays);
        }

        [ComVisible(false)]
        public void RegisterDisplayName(string resourceFile, long resourceId)
        {
            this.m_underlyingEventLog.RegisterDisplayName(resourceFile, resourceId);
        }

        private static void SetSpecialLogRegValues(RegistryKey logKey, string logName)
        {
            if (logKey.GetValue("MaxSize") == null)
            {
                logKey.SetValue("MaxSize", 0x80000, RegistryValueKind.DWord);
            }
            if (logKey.GetValue("AutoBackupLogFiles") == null)
            {
                logKey.SetValue("AutoBackupLogFiles", 0, RegistryValueKind.DWord);
            }
            if (!SkipRegPatch)
            {
                if (logKey.GetValue("Retention") == null)
                {
                    logKey.SetValue("Retention", 0x93a80, RegistryValueKind.DWord);
                }
                if (logKey.GetValue("File") == null)
                {
                    string str;
                    if (logName.Length > 8)
                    {
                        str = @"%SystemRoot%\System32\config\" + logName.Substring(0, 8) + ".evt";
                    }
                    else
                    {
                        str = @"%SystemRoot%\System32\config\" + logName + ".evt";
                    }
                    logKey.SetValue("File", str, RegistryValueKind.ExpandString);
                }
            }
        }

        private static void SetSpecialSourceRegValues(RegistryKey sourceLogKey, EventSourceCreationData sourceData)
        {
            if (string.IsNullOrEmpty(sourceData.MessageResourceFile))
            {
                sourceLogKey.SetValue("EventMessageFile", GetDllPath(sourceData.MachineName), RegistryValueKind.ExpandString);
            }
            else
            {
                sourceLogKey.SetValue("EventMessageFile", FixupPath(sourceData.MessageResourceFile), RegistryValueKind.ExpandString);
            }
            if (!string.IsNullOrEmpty(sourceData.ParameterResourceFile))
            {
                sourceLogKey.SetValue("ParameterMessageFile", FixupPath(sourceData.ParameterResourceFile), RegistryValueKind.ExpandString);
            }
            if (!string.IsNullOrEmpty(sourceData.CategoryResourceFile))
            {
                sourceLogKey.SetValue("CategoryMessageFile", FixupPath(sourceData.CategoryResourceFile), RegistryValueKind.ExpandString);
                sourceLogKey.SetValue("CategoryCount", sourceData.CategoryCount, RegistryValueKind.DWord);
            }
        }

        public static bool SourceExists(string source)
        {
            return SourceExists(source, ".");
        }

        public static bool SourceExists(string source, string machineName)
        {
            return SourceExists(source, machineName, false);
        }

        private static bool SourceExists(string source, string machineName, bool wantToCreate)
        {
            if (!SyntaxCheck.CheckMachineName(machineName))
            {
                throw new ArgumentException(SR.GetString("InvalidParameter", new object[] { "machineName", machineName }));
            }
            new EventLogPermission(EventLogPermissionAccess.Write, machineName).Demand();
            using (RegistryKey key = FindSourceRegistration(source, machineName, true, wantToCreate))
            {
                return (key != null);
            }
        }

        internal static string TryFormatMessage(Microsoft.Win32.SafeHandles.SafeLibraryHandle hModule, uint messageNum, string[] insertionStrings)
        {
            if (insertionStrings.Length != 0)
            {
                string str = UnsafeTryFormatMessage(hModule, messageNum, new string[0]);
                if (str == null)
                {
                    return null;
                }
                int num = 0;
                for (int i = 0; i < str.Length; i++)
                {
                    if ((str[i] == '%') && (str.Length > (i + 1)))
                    {
                        StringBuilder builder = new StringBuilder();
                        while (((i + 1) < str.Length) && char.IsDigit(str[i + 1]))
                        {
                            builder.Append(str[i + 1]);
                            i++;
                        }
                        i++;
                        if (builder.Length > 0)
                        {
                            int result = -1;
                            if (int.TryParse(builder.ToString(), NumberStyles.None, CultureInfo.InvariantCulture, out result))
                            {
                                num = Math.Max(num, result);
                            }
                        }
                    }
                }
                if (num > insertionStrings.Length)
                {
                    string[] destinationArray = new string[num];
                    Array.Copy(insertionStrings, destinationArray, insertionStrings.Length);
                    for (int j = insertionStrings.Length; j < destinationArray.Length; j++)
                    {
                        destinationArray[j] = "%" + (j + 1);
                    }
                    insertionStrings = destinationArray;
                }
            }
            return UnsafeTryFormatMessage(hModule, messageNum, insertionStrings);
        }

        internal static string UnsafeTryFormatMessage(Microsoft.Win32.SafeHandles.SafeLibraryHandle hModule, uint messageNum, string[] insertionStrings)
        {
            string str = null;
            int num = 0;
            StringBuilder lpBuffer = new StringBuilder(0x400);
            int dwFlags = 0x2800;
            IntPtr[] ptrArray = new IntPtr[insertionStrings.Length];
            GCHandle[] handleArray = new GCHandle[insertionStrings.Length];
            GCHandle handle = GCHandle.Alloc(ptrArray, GCHandleType.Pinned);
            if (insertionStrings.Length == 0)
            {
                dwFlags |= 0x200;
            }
            try
            {
                for (int i = 0; i < handleArray.Length; i++)
                {
                    handleArray[i] = GCHandle.Alloc(insertionStrings[i], GCHandleType.Pinned);
                    ptrArray[i] = handleArray[i].AddrOfPinnedObject();
                }
                int num4 = 0x7a;
                while ((num == 0) && (num4 == 0x7a))
                {
                    num = Microsoft.Win32.SafeNativeMethods.FormatMessage(dwFlags, hModule, messageNum, 0, lpBuffer, lpBuffer.Capacity, ptrArray);
                    if (num == 0)
                    {
                        num4 = Marshal.GetLastWin32Error();
                        if (num4 == 0x7a)
                        {
                            lpBuffer.Capacity *= 2;
                        }
                    }
                }
            }
            catch
            {
                num = 0;
            }
            finally
            {
                for (int j = 0; j < handleArray.Length; j++)
                {
                    if (handleArray[j].IsAllocated)
                    {
                        handleArray[j].Free();
                    }
                }
                handle.Free();
            }
            if (num > 0)
            {
                str = lpBuffer.ToString();
                if ((str.Length > 1) && (str[str.Length - 1] == '\n'))
                {
                    str = str.Substring(0, str.Length - 2);
                }
            }
            return str;
        }

        internal static bool ValidLogName(string logName, bool ignoreEmpty)
        {
            if ((logName.Length == 0) && !ignoreEmpty)
            {
                return false;
            }
            foreach (char ch in logName)
            {
                if ((!CharIsPrintable(ch) || (ch == '\\')) || ((ch == '*') || (ch == '?')))
                {
                    return false;
                }
            }
            return true;
        }

        public void WriteEntry(string message)
        {
            this.WriteEntry(message, EventLogEntryType.Information, 0, 0, null);
        }

        public void WriteEntry(string message, EventLogEntryType type)
        {
            this.WriteEntry(message, type, 0, 0, null);
        }

        public static void WriteEntry(string source, string message)
        {
            WriteEntry(source, message, EventLogEntryType.Information, 0, 0, null);
        }

        public void WriteEntry(string message, EventLogEntryType type, int eventID)
        {
            this.WriteEntry(message, type, eventID, 0, null);
        }

        public static void WriteEntry(string source, string message, EventLogEntryType type)
        {
            WriteEntry(source, message, type, 0, 0, null);
        }

        public void WriteEntry(string message, EventLogEntryType type, int eventID, short category)
        {
            this.WriteEntry(message, type, eventID, category, null);
        }

        public static void WriteEntry(string source, string message, EventLogEntryType type, int eventID)
        {
            WriteEntry(source, message, type, eventID, 0, null);
        }

        public void WriteEntry(string message, EventLogEntryType type, int eventID, short category, byte[] rawData)
        {
            this.m_underlyingEventLog.WriteEntry(message, type, eventID, category, rawData);
        }

        public static void WriteEntry(string source, string message, EventLogEntryType type, int eventID, short category)
        {
            WriteEntry(source, message, type, eventID, category, null);
        }

        public static void WriteEntry(string source, string message, EventLogEntryType type, int eventID, short category, byte[] rawData)
        {
            using (EventLogInternal internal2 = new EventLogInternal("", ".", CheckAndNormalizeSourceName(source)))
            {
                internal2.WriteEntry(message, type, eventID, category, rawData);
            }
        }

        [ComVisible(false)]
        public void WriteEvent(EventInstance instance, params object[] values)
        {
            this.WriteEvent(instance, null, values);
        }

        [ComVisible(false)]
        public void WriteEvent(EventInstance instance, byte[] data, params object[] values)
        {
            this.m_underlyingEventLog.WriteEvent(instance, data, values);
        }

        public static void WriteEvent(string source, EventInstance instance, params object[] values)
        {
            using (EventLogInternal internal2 = new EventLogInternal("", ".", CheckAndNormalizeSourceName(source)))
            {
                internal2.WriteEvent(instance, null, values);
            }
        }

        public static void WriteEvent(string source, EventInstance instance, byte[] data, params object[] values)
        {
            using (EventLogInternal internal2 = new EventLogInternal("", ".", CheckAndNormalizeSourceName(source)))
            {
                internal2.WriteEvent(instance, data, values);
            }
        }

        internal bool ComponentDesignMode
        {
            get
            {
                return base.DesignMode;
            }
        }

        [DefaultValue(false), MonitoringDescription("LogMonitoring"), Browsable(false)]
        public bool EnableRaisingEvents
        {
            get
            {
                return this.m_underlyingEventLog.EnableRaisingEvents;
            }
            set
            {
                this.m_underlyingEventLog.EnableRaisingEvents = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription("LogEntries")]
        public EventLogEntryCollection Entries
        {
            get
            {
                return this.m_underlyingEventLog.Entries;
            }
        }

        [ReadOnly(true), TypeConverter("System.Diagnostics.Design.LogConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), MonitoringDescription("LogLog"), DefaultValue(""), SettingsBindable(true)]
        public string Log
        {
            get
            {
                return this.m_underlyingEventLog.Log;
            }
            set
            {
                EventLogInternal internal2 = new EventLogInternal(value, this.m_underlyingEventLog.MachineName, this.m_underlyingEventLog.Source, this);
                EventLogInternal underlyingEventLog = this.m_underlyingEventLog;
                new EventLogPermission(EventLogPermissionAccess.Write, underlyingEventLog.machineName).Assert();
                if (underlyingEventLog.EnableRaisingEvents)
                {
                    internal2.onEntryWrittenHandler = underlyingEventLog.onEntryWrittenHandler;
                    internal2.EnableRaisingEvents = true;
                }
                this.m_underlyingEventLog = internal2;
                underlyingEventLog.Close();
            }
        }

        [Browsable(false)]
        public string LogDisplayName
        {
            get
            {
                return this.m_underlyingEventLog.LogDisplayName;
            }
        }

        [DefaultValue("."), ReadOnly(true), MonitoringDescription("LogMachineName"), SettingsBindable(true)]
        public string MachineName
        {
            get
            {
                return this.m_underlyingEventLog.MachineName;
            }
            set
            {
                EventLogInternal internal2 = new EventLogInternal(this.m_underlyingEventLog.logName, value, this.m_underlyingEventLog.sourceName, this);
                EventLogInternal underlyingEventLog = this.m_underlyingEventLog;
                new EventLogPermission(EventLogPermissionAccess.Write, underlyingEventLog.machineName).Assert();
                if (underlyingEventLog.EnableRaisingEvents)
                {
                    internal2.onEntryWrittenHandler = underlyingEventLog.onEntryWrittenHandler;
                    internal2.EnableRaisingEvents = true;
                }
                this.m_underlyingEventLog = internal2;
                underlyingEventLog.Close();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ComVisible(false), Browsable(false)]
        public long MaximumKilobytes
        {
            get
            {
                return this.m_underlyingEventLog.MaximumKilobytes;
            }
            set
            {
                this.m_underlyingEventLog.MaximumKilobytes = value;
            }
        }

        [ComVisible(false), Browsable(false)]
        public int MinimumRetentionDays
        {
            get
            {
                return this.m_underlyingEventLog.MinimumRetentionDays;
            }
        }

        [Browsable(false), ComVisible(false)]
        public System.Diagnostics.OverflowAction OverflowAction
        {
            get
            {
                return this.m_underlyingEventLog.OverflowAction;
            }
        }

        private static bool SkipRegPatch
        {
            get
            {
                if (!s_CheckedOsVersion)
                {
                    OperatingSystem oSVersion = Environment.OSVersion;
                    s_SkipRegPatch = (oSVersion.Platform == PlatformID.Win32NT) && (oSVersion.Version.Major > 5);
                    s_CheckedOsVersion = true;
                }
                return s_SkipRegPatch;
            }
        }

        [ReadOnly(true), DefaultValue(""), TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), MonitoringDescription("LogSource"), SettingsBindable(true)]
        public string Source
        {
            get
            {
                return this.m_underlyingEventLog.Source;
            }
            set
            {
                EventLogInternal internal2 = new EventLogInternal(this.m_underlyingEventLog.Log, this.m_underlyingEventLog.MachineName, CheckAndNormalizeSourceName(value), this);
                EventLogInternal underlyingEventLog = this.m_underlyingEventLog;
                new EventLogPermission(EventLogPermissionAccess.Write, underlyingEventLog.machineName).Assert();
                if (underlyingEventLog.EnableRaisingEvents)
                {
                    internal2.onEntryWrittenHandler = underlyingEventLog.onEntryWrittenHandler;
                    internal2.EnableRaisingEvents = true;
                }
                this.m_underlyingEventLog = internal2;
                underlyingEventLog.Close();
            }
        }

        [MonitoringDescription("LogSynchronizingObject"), Browsable(false), DefaultValue((string) null)]
        public ISynchronizeInvoke SynchronizingObject
        {
            [HostProtection(SecurityAction.LinkDemand, Synchronization=true)]
            get
            {
                return this.m_underlyingEventLog.SynchronizingObject;
            }
            set
            {
                this.m_underlyingEventLog.SynchronizingObject = value;
            }
        }
    }
}

