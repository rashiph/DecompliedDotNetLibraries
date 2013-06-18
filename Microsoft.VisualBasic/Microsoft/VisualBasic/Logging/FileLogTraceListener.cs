namespace Microsoft.VisualBasic.Logging
{
    using Microsoft.VisualBasic;
    using Microsoft.VisualBasic.CompilerServices;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Windows.Forms;

    [ComVisible(false)]
    public class FileLogTraceListener : TraceListener
    {
        private const int APPEND_INDEX = 0;
        private const int AUTOFLUSH_INDEX = 1;
        private const int BASEFILENAME_INDEX = 2;
        private const int CUSTOMLOCATION_INDEX = 3;
        private const string DATE_FORMAT = "yyyy-MM-dd";
        private const string DEFAULT_NAME = "FileLogTraceListener";
        private const int DELIMITER_INDEX = 4;
        private const int DISKSPACEEXHAUSTEDBEHAVIOR_INDEX = 5;
        private const int ENCODING_INDEX = 6;
        private const string FILE_EXTENSION = ".log";
        private const int INCLUDEHOSTNAME_INDEX = 7;
        private const string KEY_APPEND = "append";
        private const string KEY_APPEND_PASCAL = "Append";
        private const string KEY_AUTOFLUSH = "autoflush";
        private const string KEY_AUTOFLUSH_CAMEL = "autoFlush";
        private const string KEY_AUTOFLUSH_PASCAL = "AutoFlush";
        private const string KEY_BASEFILENAME = "basefilename";
        private const string KEY_BASEFILENAME_CAMEL = "baseFilename";
        private const string KEY_BASEFILENAME_CAMEL_ALT = "baseFileName";
        private const string KEY_BASEFILENAME_PASCAL = "BaseFilename";
        private const string KEY_BASEFILENAME_PASCAL_ALT = "BaseFileName";
        private const string KEY_CUSTOMLOCATION = "customlocation";
        private const string KEY_CUSTOMLOCATION_CAMEL = "customLocation";
        private const string KEY_CUSTOMLOCATION_PASCAL = "CustomLocation";
        private const string KEY_DELIMITER = "delimiter";
        private const string KEY_DELIMITER_PASCAL = "Delimiter";
        private const string KEY_DISKSPACEEXHAUSTEDBEHAVIOR = "diskspaceexhaustedbehavior";
        private const string KEY_DISKSPACEEXHAUSTEDBEHAVIOR_CAMEL = "diskSpaceExhaustedBehavior";
        private const string KEY_DISKSPACEEXHAUSTEDBEHAVIOR_PASCAL = "DiskSpaceExhaustedBehavior";
        private const string KEY_ENCODING = "encoding";
        private const string KEY_ENCODING_PASCAL = "Encoding";
        private const string KEY_INCLUDEHOSTNAME = "includehostname";
        private const string KEY_INCLUDEHOSTNAME_CAMEL = "includeHostName";
        private const string KEY_INCLUDEHOSTNAME_PASCAL = "IncludeHostName";
        private const string KEY_LOCATION = "location";
        private const string KEY_LOCATION_PASCAL = "Location";
        private const string KEY_LOGFILECREATIONSCHEDULE = "logfilecreationschedule";
        private const string KEY_LOGFILECREATIONSCHEDULE_CAMEL = "logFileCreationSchedule";
        private const string KEY_LOGFILECREATIONSCHEDULE_PASCAL = "LogFileCreationSchedule";
        private const string KEY_MAXFILESIZE = "maxfilesize";
        private const string KEY_MAXFILESIZE_CAMEL = "maxFileSize";
        private const string KEY_MAXFILESIZE_PASCAL = "MaxFileSize";
        private const string KEY_RESERVEDISKSPACE = "reservediskspace";
        private const string KEY_RESERVEDISKSPACE_CAMEL = "reserveDiskSpace";
        private const string KEY_RESERVEDISKSPACE_PASCAL = "ReserveDiskSpace";
        private const int LOCATION_INDEX = 8;
        private const int LOGFILECREATIONSCHEDULE_INDEX = 9;
        private bool m_Append;
        private bool m_AutoFlush;
        private string m_BaseFileName;
        private string m_CustomLocation;
        private DateTime m_Day;
        private string m_Delimiter;
        private DiskSpaceExhaustedOption m_DiskSpaceExhaustedBehavior;
        private System.Text.Encoding m_Encoding;
        private DateTime m_FirstDayOfWeek;
        private string m_FullFileName;
        private string m_HostName;
        private bool m_IncludeHostName;
        private LogFileLocation m_Location;
        private LogFileCreationScheduleOption m_LogFileDateStamp;
        private long m_MaxFileSize;
        private BitArray m_PropertiesSet;
        private long m_ReserveDiskSpace;
        private ReferencedStream m_Stream;
        private static Dictionary<string, ReferencedStream> m_Streams = new Dictionary<string, ReferencedStream>();
        private string[] m_SupportedAttributes;
        private const int MAX_OPEN_ATTEMPTS = 0x7fffffff;
        private const int MAXFILESIZE_INDEX = 10;
        private const int MIN_FILE_SIZE = 0x3e8;
        private const int PROPERTY_COUNT = 12;
        private const int RESERVEDISKSPACE_INDEX = 11;
        private const string STACK_DELIMITER = ", ";

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries"), HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt)]
        public FileLogTraceListener() : this("FileLogTraceListener")
        {
        }

        [HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt)]
        public FileLogTraceListener(string name) : base(name)
        {
            this.m_Location = LogFileLocation.LocalUserApplicationDirectory;
            this.m_AutoFlush = false;
            this.m_Append = true;
            this.m_IncludeHostName = false;
            this.m_DiskSpaceExhaustedBehavior = DiskSpaceExhaustedOption.DiscardMessages;
            this.m_BaseFileName = Path.GetFileNameWithoutExtension(Application.ExecutablePath);
            this.m_LogFileDateStamp = LogFileCreationScheduleOption.None;
            this.m_MaxFileSize = 0x4c4b40L;
            this.m_ReserveDiskSpace = 0x989680L;
            this.m_Delimiter = "\t";
            this.m_Encoding = System.Text.Encoding.UTF8;
            this.m_CustomLocation = Application.UserAppDataPath;
            this.m_Day = DateAndTime.Now.Date;
            this.m_FirstDayOfWeek = GetFirstDayOfWeek(DateAndTime.Now.Date);
            this.m_PropertiesSet = new BitArray(12, false);
            this.m_SupportedAttributes = new string[] { 
                "append", "Append", "autoflush", "AutoFlush", "autoFlush", "basefilename", "BaseFilename", "baseFilename", "BaseFileName", "baseFileName", "customlocation", "CustomLocation", "customLocation", "delimiter", "Delimiter", "diskspaceexhaustedbehavior", 
                "DiskSpaceExhaustedBehavior", "diskSpaceExhaustedBehavior", "encoding", "Encoding", "includehostname", "IncludeHostName", "includeHostName", "location", "Location", "logfilecreationschedule", "LogFileCreationSchedule", "logFileCreationSchedule", "maxfilesize", "MaxFileSize", "maxFileSize", "reservediskspace", 
                "ReserveDiskSpace", "reserveDiskSpace"
             };
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries"), HostProtection(SecurityAction.LinkDemand, Synchronization=true)]
        public override void Close()
        {
            this.Dispose(true);
        }

        private void CloseCurrentStream()
        {
            if (this.m_Stream != null)
            {
                Dictionary<string, ReferencedStream> streams = m_Streams;
                lock (streams)
                {
                    this.m_Stream.CloseStream();
                    if (!this.m_Stream.IsInUse)
                    {
                        m_Streams.Remove(this.m_FullFileName.ToUpper(CultureInfo.InvariantCulture));
                    }
                    this.m_Stream = null;
                }
            }
        }

        private bool DayChanged()
        {
            return (DateTime.Compare(this.m_Day.Date, DateAndTime.Now.Date) != 0);
        }

        [SecurityCritical]
        private void DemandWritePermission()
        {
            string directoryName = Path.GetDirectoryName(this.LogFileName);
            new FileIOPermission(FileIOPermissionAccess.Write, directoryName).Demand();
        }

        [HostProtection(SecurityAction.LinkDemand, Synchronization=true)]
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.CloseCurrentStream();
            }
        }

        private void EnsureStreamIsOpen()
        {
            if (this.m_Stream == null)
            {
                this.m_Stream = this.GetStream();
            }
        }

        [HostProtection(SecurityAction.LinkDemand, Synchronization=true)]
        public override void Flush()
        {
            if (this.m_Stream != null)
            {
                this.m_Stream.Flush();
            }
        }

        private System.Text.Encoding GetFileEncoding(string fileName)
        {
            if (File.Exists(fileName))
            {
                StreamReader reader = null;
                try
                {
                    reader = new StreamReader(fileName, this.Encoding, true);
                    if (reader.BaseStream.Length > 0L)
                    {
                        reader.ReadLine();
                        return reader.CurrentEncoding;
                    }
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                    }
                }
            }
            return null;
        }

        private static DateTime GetFirstDayOfWeek(DateTime checkDate)
        {
            return checkDate.AddDays((double) (0 - checkDate.DayOfWeek)).Date;
        }

        [SecuritySafeCritical]
        private long GetFreeDiskSpace()
        {
            long num3;
            long num4;
            string pathRoot = Path.GetPathRoot(Path.GetFullPath(this.FullLogFileName));
            long userSpaceFree = -1L;
            new FileIOPermission(FileIOPermissionAccess.PathDiscovery, pathRoot).Demand();
            if (!Microsoft.VisualBasic.CompilerServices.UnsafeNativeMethods.GetDiskFreeSpaceEx(pathRoot, ref userSpaceFree, ref num4, ref num3) || (userSpaceFree <= -1L))
            {
                throw ExceptionUtils.GetWin32Exception("ApplicationLog_FreeSpaceError", new string[0]);
            }
            return userSpaceFree;
        }

        [SecuritySafeCritical]
        private ReferencedStream GetStream()
        {
            int num = 0;
            ReferencedStream stream2 = null;
            string fullPath = Path.GetFullPath(this.LogFileName + ".log");
            while ((stream2 == null) && (num < 0x7fffffff))
            {
                string str3;
                if (num == 0)
                {
                    str3 = Path.GetFullPath(this.LogFileName + ".log");
                }
                else
                {
                    str3 = Path.GetFullPath(this.LogFileName + "-" + num.ToString(CultureInfo.InvariantCulture) + ".log");
                }
                string key = str3.ToUpper(CultureInfo.InvariantCulture);
                Dictionary<string, ReferencedStream> streams = m_Streams;
                lock (streams)
                {
                    if (m_Streams.ContainsKey(key))
                    {
                        stream2 = m_Streams[key];
                        if (!stream2.IsInUse)
                        {
                            m_Streams.Remove(key);
                            stream2 = null;
                        }
                        else
                        {
                            if (this.Append)
                            {
                                new FileIOPermission(FileIOPermissionAccess.Write, str3).Demand();
                                stream2.AddReference();
                                this.m_FullFileName = str3;
                                return stream2;
                            }
                            num++;
                            stream2 = null;
                            continue;
                        }
                    }
                    System.Text.Encoding fileEncoding = this.Encoding;
                    try
                    {
                        if (this.Append)
                        {
                            fileEncoding = this.GetFileEncoding(str3);
                            if (fileEncoding == null)
                            {
                                fileEncoding = this.Encoding;
                            }
                        }
                        StreamWriter stream = new StreamWriter(str3, this.Append, fileEncoding);
                        stream2 = new ReferencedStream(stream);
                        stream2.AddReference();
                        m_Streams.Add(key, stream2);
                        this.m_FullFileName = str3;
                        return stream2;
                    }
                    catch (IOException)
                    {
                    }
                    num++;
                    continue;
                }
            }
            throw ExceptionUtils.GetInvalidOperationException("ApplicationLog_ExhaustedPossibleStreamNames", new string[] { fullPath });
        }

        [HostProtection(SecurityAction.LinkDemand, Synchronization=true)]
        protected override string[] GetSupportedAttributes()
        {
            return this.m_SupportedAttributes;
        }

        private void HandleDateChange()
        {
            if (this.LogFileCreationSchedule == LogFileCreationScheduleOption.Daily)
            {
                if (this.DayChanged())
                {
                    this.CloseCurrentStream();
                }
            }
            else if ((this.LogFileCreationSchedule == LogFileCreationScheduleOption.Weekly) && this.WeekChanged())
            {
                this.CloseCurrentStream();
            }
        }

        private bool ResourcesAvailable(long newEntrySize)
        {
            if ((this.ListenerStream.FileSize + newEntrySize) > this.MaxFileSize)
            {
                if (this.DiskSpaceExhaustedBehavior == DiskSpaceExhaustedOption.ThrowException)
                {
                    throw new InvalidOperationException(Utils.GetResourceString("ApplicationLog_FileExceedsMaximumSize"));
                }
                return false;
            }
            if ((this.GetFreeDiskSpace() - newEntrySize) >= this.ReserveDiskSpace)
            {
                return true;
            }
            if (this.DiskSpaceExhaustedBehavior == DiskSpaceExhaustedOption.ThrowException)
            {
                throw new InvalidOperationException(Utils.GetResourceString("ApplicationLog_ReservedSpaceEncroached"));
            }
            return false;
        }

        private static string StackToString(Stack stack)
        {
            IEnumerator enumerator;
            int length = ", ".Length;
            StringBuilder builder = new StringBuilder();
            try
            {
                enumerator = stack.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    builder.Append(enumerator.Current.ToString() + ", ");
                }
            }
            finally
            {
                if (enumerator is IDisposable)
                {
                    (enumerator as IDisposable).Dispose();
                }
            }
            builder.Replace("\"", "\"\"");
            if (builder.Length >= length)
            {
                builder.Remove(builder.Length - length, length);
            }
            return ("\"" + builder.ToString() + "\"");
        }

        [HostProtection(SecurityAction.LinkDemand, Synchronization=true)]
        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            string message = "";
            if (data != null)
            {
                message = data.ToString();
            }
            this.TraceEvent(eventCache, source, eventType, id, message);
        }

        [HostProtection(SecurityAction.LinkDemand, Synchronization=true)]
        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
        {
            StringBuilder builder = new StringBuilder();
            if (data != null)
            {
                int num = data.Length - 1;
                int num3 = num;
                for (int i = 0; i <= num3; i++)
                {
                    builder.Append(data[i].ToString());
                    if (i != num)
                    {
                        builder.Append(this.Delimiter);
                    }
                }
            }
            this.TraceEvent(eventCache, source, eventType, id, builder.ToString());
        }

        [HostProtection(SecurityAction.LinkDemand, Synchronization=true)]
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            if ((this.Filter == null) || this.Filter.ShouldTrace(eventCache, source, eventType, id, message, null, null, null))
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(source + this.Delimiter);
                builder.Append(Enum.GetName(typeof(TraceEventType), eventType) + this.Delimiter);
                builder.Append(id.ToString(CultureInfo.InvariantCulture) + this.Delimiter);
                builder.Append(message);
                if ((this.TraceOutputOptions & TraceOptions.Callstack) == TraceOptions.Callstack)
                {
                    builder.Append(this.Delimiter + eventCache.Callstack);
                }
                if ((this.TraceOutputOptions & TraceOptions.LogicalOperationStack) == TraceOptions.LogicalOperationStack)
                {
                    builder.Append(this.Delimiter + StackToString(eventCache.LogicalOperationStack));
                }
                if ((this.TraceOutputOptions & TraceOptions.DateTime) == TraceOptions.DateTime)
                {
                    builder.Append(this.Delimiter + eventCache.DateTime.ToString("u", CultureInfo.InvariantCulture));
                }
                if ((this.TraceOutputOptions & TraceOptions.ProcessId) == TraceOptions.ProcessId)
                {
                    builder.Append(this.Delimiter + eventCache.ProcessId.ToString(CultureInfo.InvariantCulture));
                }
                if ((this.TraceOutputOptions & TraceOptions.ThreadId) == TraceOptions.ThreadId)
                {
                    builder.Append(this.Delimiter + eventCache.ThreadId);
                }
                if ((this.TraceOutputOptions & TraceOptions.Timestamp) == TraceOptions.Timestamp)
                {
                    builder.Append(this.Delimiter + eventCache.Timestamp.ToString(CultureInfo.InvariantCulture));
                }
                if (this.IncludeHostName)
                {
                    builder.Append(this.Delimiter + this.HostName);
                }
                this.WriteLine(builder.ToString());
            }
        }

        [HostProtection(SecurityAction.LinkDemand, Synchronization=true)]
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            string message = null;
            if (args != null)
            {
                message = string.Format(CultureInfo.InvariantCulture, format, args);
            }
            else
            {
                message = format;
            }
            this.TraceEvent(eventCache, source, eventType, id, message);
        }

        private void ValidateDiskSpaceExhaustedOptionEnumValue(DiskSpaceExhaustedOption value, string paramName)
        {
            if ((value < DiskSpaceExhaustedOption.ThrowException) || (value > DiskSpaceExhaustedOption.DiscardMessages))
            {
                throw new InvalidEnumArgumentException(paramName, (int) value, typeof(DiskSpaceExhaustedOption));
            }
        }

        private void ValidateLogFileCreationScheduleOptionEnumValue(LogFileCreationScheduleOption value, string paramName)
        {
            if ((value < LogFileCreationScheduleOption.None) || (value > LogFileCreationScheduleOption.Weekly))
            {
                throw new InvalidEnumArgumentException(paramName, (int) value, typeof(LogFileCreationScheduleOption));
            }
        }

        private void ValidateLogFileLocationEnumValue(LogFileLocation value, string paramName)
        {
            if ((value < LogFileLocation.TempDirectory) || (value > LogFileLocation.Custom))
            {
                throw new InvalidEnumArgumentException(paramName, (int) value, typeof(LogFileLocation));
            }
        }

        private bool WeekChanged()
        {
            return (DateTime.Compare(this.m_FirstDayOfWeek.Date, GetFirstDayOfWeek(DateAndTime.Now.Date)) != 0);
        }

        [HostProtection(SecurityAction.LinkDemand, Synchronization=true)]
        public override void Write(string message)
        {
            try
            {
                this.HandleDateChange();
                long byteCount = this.Encoding.GetByteCount(message);
                if (this.ResourcesAvailable(byteCount))
                {
                    this.ListenerStream.Write(message);
                    if (this.AutoFlush)
                    {
                        this.ListenerStream.Flush();
                    }
                }
            }
            catch (Exception)
            {
                this.CloseCurrentStream();
                throw;
            }
        }

        [HostProtection(SecurityAction.LinkDemand, Synchronization=true)]
        public override void WriteLine(string message)
        {
            try
            {
                this.HandleDateChange();
                long byteCount = this.Encoding.GetByteCount(message + "\r\n");
                if (this.ResourcesAvailable(byteCount))
                {
                    this.ListenerStream.WriteLine(message);
                    if (this.AutoFlush)
                    {
                        this.ListenerStream.Flush();
                    }
                }
            }
            catch (Exception)
            {
                this.CloseCurrentStream();
                throw;
            }
        }

        public bool Append
        {
            get
            {
                if (!this.m_PropertiesSet[0] && this.Attributes.ContainsKey("append"))
                {
                    this.Append = Convert.ToBoolean(this.Attributes["append"], CultureInfo.InvariantCulture);
                }
                return this.m_Append;
            }
            [SecuritySafeCritical]
            set
            {
                this.DemandWritePermission();
                if (value != this.m_Append)
                {
                    this.CloseCurrentStream();
                }
                this.m_Append = value;
                this.m_PropertiesSet[0] = true;
            }
        }

        public bool AutoFlush
        {
            get
            {
                if (!this.m_PropertiesSet[1] && this.Attributes.ContainsKey("autoflush"))
                {
                    this.AutoFlush = Convert.ToBoolean(this.Attributes["autoflush"], CultureInfo.InvariantCulture);
                }
                return this.m_AutoFlush;
            }
            [SecuritySafeCritical]
            set
            {
                this.DemandWritePermission();
                this.m_AutoFlush = value;
                this.m_PropertiesSet[1] = true;
            }
        }

        public string BaseFileName
        {
            get
            {
                if (!this.m_PropertiesSet[2] && this.Attributes.ContainsKey("basefilename"))
                {
                    this.BaseFileName = this.Attributes["basefilename"];
                }
                return this.m_BaseFileName;
            }
            set
            {
                if (value == "")
                {
                    throw ExceptionUtils.GetArgumentNullException("value", "ApplicationLogBaseNameNull", new string[0]);
                }
                Path.GetFullPath(value);
                if (string.Compare(value, this.m_BaseFileName, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    this.CloseCurrentStream();
                    this.m_BaseFileName = value;
                }
                this.m_PropertiesSet[2] = true;
            }
        }

        public string CustomLocation
        {
            [SecuritySafeCritical]
            get
            {
                if (!this.m_PropertiesSet[3] && this.Attributes.ContainsKey("customlocation"))
                {
                    this.CustomLocation = this.Attributes["customlocation"];
                }
                string fullPath = Path.GetFullPath(this.m_CustomLocation);
                new FileIOPermission(FileIOPermissionAccess.PathDiscovery, fullPath).Demand();
                return fullPath;
            }
            set
            {
                string fullPath = Path.GetFullPath(value);
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                }
                if ((this.Location == LogFileLocation.Custom) & (string.Compare(fullPath, this.m_CustomLocation, StringComparison.OrdinalIgnoreCase) != 0))
                {
                    this.CloseCurrentStream();
                }
                this.Location = LogFileLocation.Custom;
                this.m_CustomLocation = fullPath;
                this.m_PropertiesSet[3] = true;
            }
        }

        public string Delimiter
        {
            get
            {
                if (!this.m_PropertiesSet[4] && this.Attributes.ContainsKey("delimiter"))
                {
                    this.Delimiter = this.Attributes["delimiter"];
                }
                return this.m_Delimiter;
            }
            set
            {
                this.m_Delimiter = value;
                this.m_PropertiesSet[4] = true;
            }
        }

        public DiskSpaceExhaustedOption DiskSpaceExhaustedBehavior
        {
            get
            {
                if (!this.m_PropertiesSet[5] && this.Attributes.ContainsKey("diskspaceexhaustedbehavior"))
                {
                    TypeConverter converter = TypeDescriptor.GetConverter(typeof(DiskSpaceExhaustedOption));
                    this.DiskSpaceExhaustedBehavior = (DiskSpaceExhaustedOption) converter.ConvertFromInvariantString(this.Attributes["diskspaceexhaustedbehavior"]);
                }
                return this.m_DiskSpaceExhaustedBehavior;
            }
            [SecuritySafeCritical]
            set
            {
                this.DemandWritePermission();
                this.ValidateDiskSpaceExhaustedOptionEnumValue(value, "value");
                this.m_DiskSpaceExhaustedBehavior = value;
                this.m_PropertiesSet[5] = true;
            }
        }

        public System.Text.Encoding Encoding
        {
            get
            {
                if (!this.m_PropertiesSet[6] && this.Attributes.ContainsKey("encoding"))
                {
                    this.Encoding = System.Text.Encoding.GetEncoding(this.Attributes["encoding"]);
                }
                return this.m_Encoding;
            }
            set
            {
                if (value == null)
                {
                    throw ExceptionUtils.GetArgumentNullException("value");
                }
                this.m_Encoding = value;
                this.m_PropertiesSet[6] = true;
            }
        }

        public string FullLogFileName
        {
            [SecuritySafeCritical]
            get
            {
                this.EnsureStreamIsOpen();
                string fullFileName = this.m_FullFileName;
                new FileIOPermission(FileIOPermissionAccess.PathDiscovery, fullFileName).Demand();
                return fullFileName;
            }
        }

        private string HostName
        {
            get
            {
                if (this.m_HostName == "")
                {
                    this.m_HostName = Environment.MachineName;
                }
                return this.m_HostName;
            }
        }

        public bool IncludeHostName
        {
            get
            {
                if (!this.m_PropertiesSet[7] && this.Attributes.ContainsKey("includehostname"))
                {
                    this.IncludeHostName = Convert.ToBoolean(this.Attributes["includehostname"], CultureInfo.InvariantCulture);
                }
                return this.m_IncludeHostName;
            }
            [SecuritySafeCritical]
            set
            {
                this.DemandWritePermission();
                this.m_IncludeHostName = value;
                this.m_PropertiesSet[7] = true;
            }
        }

        private ReferencedStream ListenerStream
        {
            get
            {
                this.EnsureStreamIsOpen();
                return this.m_Stream;
            }
        }

        public LogFileLocation Location
        {
            get
            {
                if (!this.m_PropertiesSet[8] && this.Attributes.ContainsKey("location"))
                {
                    TypeConverter converter = TypeDescriptor.GetConverter(typeof(LogFileLocation));
                    this.Location = (LogFileLocation) converter.ConvertFromInvariantString(this.Attributes["location"]);
                }
                return this.m_Location;
            }
            set
            {
                this.ValidateLogFileLocationEnumValue(value, "value");
                if (this.m_Location != value)
                {
                    this.CloseCurrentStream();
                }
                this.m_Location = value;
                this.m_PropertiesSet[8] = true;
            }
        }

        public LogFileCreationScheduleOption LogFileCreationSchedule
        {
            get
            {
                if (!this.m_PropertiesSet[9] && this.Attributes.ContainsKey("logfilecreationschedule"))
                {
                    TypeConverter converter = TypeDescriptor.GetConverter(typeof(LogFileCreationScheduleOption));
                    this.LogFileCreationSchedule = (LogFileCreationScheduleOption) converter.ConvertFromInvariantString(this.Attributes["logfilecreationschedule"]);
                }
                return this.m_LogFileDateStamp;
            }
            set
            {
                this.ValidateLogFileCreationScheduleOptionEnumValue(value, "value");
                if (value != this.m_LogFileDateStamp)
                {
                    this.CloseCurrentStream();
                    this.m_LogFileDateStamp = value;
                }
                this.m_PropertiesSet[9] = true;
            }
        }

        private string LogFileName
        {
            get
            {
                string tempPath;
                switch (this.Location)
                {
                    case LogFileLocation.TempDirectory:
                        tempPath = Path.GetTempPath();
                        break;

                    case LogFileLocation.LocalUserApplicationDirectory:
                        tempPath = Application.UserAppDataPath;
                        break;

                    case LogFileLocation.CommonApplicationDirectory:
                        tempPath = Application.CommonAppDataPath;
                        break;

                    case LogFileLocation.ExecutableDirectory:
                        tempPath = Path.GetDirectoryName(Application.ExecutablePath);
                        break;

                    case LogFileLocation.Custom:
                        if (this.CustomLocation != "")
                        {
                            tempPath = this.CustomLocation;
                            break;
                        }
                        tempPath = Application.UserAppDataPath;
                        break;

                    default:
                        tempPath = Application.UserAppDataPath;
                        break;
                }
                string baseFileName = this.BaseFileName;
                switch (this.LogFileCreationSchedule)
                {
                    case LogFileCreationScheduleOption.Daily:
                        baseFileName = baseFileName + "-" + DateAndTime.Now.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        break;

                    case LogFileCreationScheduleOption.Weekly:
                        this.m_FirstDayOfWeek = DateAndTime.Now.AddDays((double) (0 - DateAndTime.Now.DayOfWeek));
                        baseFileName = baseFileName + "-" + this.m_FirstDayOfWeek.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        break;
                }
                return Path.Combine(tempPath, baseFileName);
            }
        }

        public long MaxFileSize
        {
            get
            {
                if (!this.m_PropertiesSet[10] && this.Attributes.ContainsKey("maxfilesize"))
                {
                    this.MaxFileSize = Convert.ToInt64(this.Attributes["maxfilesize"], CultureInfo.InvariantCulture);
                }
                return this.m_MaxFileSize;
            }
            [SecuritySafeCritical]
            set
            {
                this.DemandWritePermission();
                if (value < 0x3e8L)
                {
                    throw ExceptionUtils.GetArgumentExceptionWithArgName("value", "ApplicationLogNumberTooSmall", new string[] { "MaxFileSize" });
                }
                this.m_MaxFileSize = value;
                this.m_PropertiesSet[10] = true;
            }
        }

        public long ReserveDiskSpace
        {
            get
            {
                if (!this.m_PropertiesSet[11] && this.Attributes.ContainsKey("reservediskspace"))
                {
                    this.ReserveDiskSpace = Convert.ToInt64(this.Attributes["reservediskspace"], CultureInfo.InvariantCulture);
                }
                return this.m_ReserveDiskSpace;
            }
            [SecuritySafeCritical]
            set
            {
                this.DemandWritePermission();
                if (value < 0L)
                {
                    throw ExceptionUtils.GetArgumentExceptionWithArgName("value", "ApplicationLog_NegativeNumber", new string[] { "ReserveDiskSpace" });
                }
                this.m_ReserveDiskSpace = value;
                this.m_PropertiesSet[11] = true;
            }
        }

        internal class ReferencedStream : IDisposable
        {
            private bool m_Disposed = false;
            private int m_ReferenceCount = 0;
            private StreamWriter m_Stream;
            private object m_SyncObject = new object();

            internal ReferencedStream(StreamWriter stream)
            {
                this.m_Stream = stream;
            }

            internal void AddReference()
            {
                object syncObject = this.m_SyncObject;
                ObjectFlowControl.CheckForSyncLockOnValueType(syncObject);
                lock (syncObject)
                {
                    this.m_ReferenceCount++;
                }
            }

            internal void CloseStream()
            {
                object syncObject = this.m_SyncObject;
                ObjectFlowControl.CheckForSyncLockOnValueType(syncObject);
                lock (syncObject)
                {
                    try
                    {
                        this.m_ReferenceCount--;
                        this.m_Stream.Flush();
                    }
                    finally
                    {
                        if (this.m_ReferenceCount <= 0)
                        {
                            this.m_Stream.Close();
                            this.m_Stream = null;
                        }
                    }
                }
            }

            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (disposing && !this.m_Disposed)
                {
                    if (this.m_Stream != null)
                    {
                        this.m_Stream.Close();
                    }
                    this.m_Disposed = true;
                }
            }

            protected override void Finalize()
            {
                this.Dispose(false);
                base.Finalize();
            }

            internal void Flush()
            {
                object syncObject = this.m_SyncObject;
                ObjectFlowControl.CheckForSyncLockOnValueType(syncObject);
                lock (syncObject)
                {
                    this.m_Stream.Flush();
                }
            }

            internal void Write(string message)
            {
                object syncObject = this.m_SyncObject;
                ObjectFlowControl.CheckForSyncLockOnValueType(syncObject);
                lock (syncObject)
                {
                    this.m_Stream.Write(message);
                }
            }

            internal void WriteLine(string message)
            {
                object syncObject = this.m_SyncObject;
                ObjectFlowControl.CheckForSyncLockOnValueType(syncObject);
                lock (syncObject)
                {
                    this.m_Stream.WriteLine(message);
                }
            }

            internal long FileSize
            {
                get
                {
                    return this.m_Stream.BaseStream.Length;
                }
            }

            internal bool IsInUse
            {
                get
                {
                    return (this.m_Stream != null);
                }
            }
        }
    }
}

