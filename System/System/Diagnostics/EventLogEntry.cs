namespace System.Diagnostics
{
    using Microsoft.Win32;
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Text;

    [Serializable, ToolboxItem(false), DesignTimeVisible(false)]
    public sealed class EventLogEntry : Component, ISerializable
    {
        private static readonly DateTime beginningOfTime = new DateTime(0x7b2, 1, 1, 0, 0, 0);
        internal int bufOffset;
        private string category;
        internal byte[] dataBuf;
        private string message;
        private const int OFFSETFIXUP = 0x38;
        private EventLogInternal owner;

        private EventLogEntry(SerializationInfo info, StreamingContext context)
        {
            this.dataBuf = (byte[]) info.GetValue("DataBuffer", typeof(byte[]));
            string logName = info.GetString("LogName");
            string machineName = info.GetString("MachineName");
            this.owner = new EventLogInternal(logName, machineName, "");
            GC.SuppressFinalize(this);
        }

        internal EventLogEntry(byte[] buf, int offset, EventLogInternal log)
        {
            this.dataBuf = buf;
            this.bufOffset = offset;
            this.owner = log;
            GC.SuppressFinalize(this);
        }

        private char CharFrom(byte[] buf, int offset)
        {
            return (char) ((ushort) this.ShortFrom(buf, offset));
        }

        public bool Equals(EventLogEntry otherEntry)
        {
            if (otherEntry == null)
            {
                return false;
            }
            int num = this.IntFrom(this.dataBuf, this.bufOffset);
            int num2 = this.IntFrom(otherEntry.dataBuf, otherEntry.bufOffset);
            if (num != num2)
            {
                return false;
            }
            int bufOffset = this.bufOffset;
            int num4 = this.bufOffset + num;
            int index = otherEntry.bufOffset;
            int num6 = bufOffset;
            while (num6 < num4)
            {
                if (this.dataBuf[num6] != otherEntry.dataBuf[index])
                {
                    return false;
                }
                num6++;
                index++;
            }
            return true;
        }

        private string GetMessageLibraryNames(string libRegKey)
        {
            string str = null;
            RegistryKey key = null;
            try
            {
                key = GetSourceRegKey(this.owner.Log, this.Source, this.owner.MachineName);
                if (key != null)
                {
                    str = (string) key.GetValue(libRegKey);
                }
            }
            finally
            {
                if (key != null)
                {
                    key.Close();
                }
            }
            if (str == null)
            {
                return null;
            }
            if (!(this.owner.MachineName != "."))
            {
                return str;
            }
            string[] strArray = str.Split(new char[] { ';' });
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < strArray.Length; i++)
            {
                if (strArray[i].EndsWith("EventLogMessages.dll", StringComparison.Ordinal))
                {
                    builder.Append(EventLog.GetDllPath("."));
                    builder.Append(';');
                }
                else if ((strArray[i].Length >= 2) && (strArray[i][1] == ':'))
                {
                    builder.Append(@"\\");
                    builder.Append(this.owner.MachineName);
                    builder.Append(@"\");
                    builder.Append(strArray[i][0]);
                    builder.Append("$");
                    builder.Append(strArray[i], 2, strArray[i].Length - 2);
                    builder.Append(';');
                }
            }
            if (builder.Length == 0)
            {
                return null;
            }
            return builder.ToString(0, builder.Length - 1);
        }

        private static RegistryKey GetSourceRegKey(string logName, string source, string machineName)
        {
            RegistryKey eventLogRegKey = null;
            RegistryKey key2 = null;
            RegistryKey key3;
            try
            {
                eventLogRegKey = EventLog.GetEventLogRegKey(machineName, false);
                if (eventLogRegKey == null)
                {
                    return null;
                }
                if (logName == null)
                {
                    key2 = eventLogRegKey.OpenSubKey("Application", false);
                }
                else
                {
                    key2 = eventLogRegKey.OpenSubKey(logName, false);
                }
                if (key2 == null)
                {
                    return null;
                }
                key3 = key2.OpenSubKey(source, false);
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
            }
            return key3;
        }

        private int IntFrom(byte[] buf, int offset)
        {
            return ((((-16777216 & (buf[offset + 3] << 0x18)) | (0xff0000 & (buf[offset + 2] << 0x10))) | (0xff00 & (buf[offset + 1] << 8))) | (0xff & buf[offset]));
        }

        internal string ReplaceMessageParameters(string msg, string[] insertionStrings)
        {
            int index = msg.IndexOf('%');
            if (index < 0)
            {
                return msg;
            }
            int startIndex = 0;
            int length = msg.Length;
            StringBuilder builder = new StringBuilder();
            string messageLibraryNames = this.GetMessageLibraryNames("ParameterMessageFile");
            while (index >= 0)
            {
                string str2 = null;
                int num4 = index + 1;
                while ((num4 < length) && char.IsDigit(msg, num4))
                {
                    num4++;
                }
                uint result = 0;
                if (num4 != (index + 1))
                {
                    uint.TryParse(msg.Substring(index + 1, (num4 - index) - 1), out result);
                }
                if (result != 0)
                {
                    str2 = this.owner.FormatMessageWrapper(messageLibraryNames, result, insertionStrings);
                }
                if (str2 != null)
                {
                    if (index > startIndex)
                    {
                        builder.Append(msg, startIndex, index - startIndex);
                    }
                    builder.Append(str2);
                    startIndex = num4;
                }
                index = msg.IndexOf('%', index + 1);
            }
            if ((length - startIndex) > 0)
            {
                builder.Append(msg, startIndex, length - startIndex);
            }
            return builder.ToString();
        }

        private short ShortFrom(byte[] buf, int offset)
        {
            return (short) ((0xff00 & (buf[offset + 1] << 8)) | (0xff & buf[offset]));
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            int length = this.IntFrom(this.dataBuf, this.bufOffset);
            byte[] destinationArray = new byte[length];
            Array.Copy(this.dataBuf, this.bufOffset, destinationArray, 0, length);
            info.AddValue("DataBuffer", destinationArray, typeof(byte[]));
            info.AddValue("LogName", this.owner.Log);
            info.AddValue("MachineName", this.owner.MachineName);
        }

        [MonitoringDescription("LogEntryCategory")]
        public string Category
        {
            get
            {
                if (this.category == null)
                {
                    string messageLibraryNames = this.GetMessageLibraryNames("CategoryMessageFile");
                    string str2 = this.owner.FormatMessageWrapper(messageLibraryNames, (uint) this.CategoryNumber, null);
                    if (str2 == null)
                    {
                        this.category = "(" + this.CategoryNumber.ToString(CultureInfo.CurrentCulture) + ")";
                    }
                    else
                    {
                        this.category = str2;
                    }
                }
                return this.category;
            }
        }

        [MonitoringDescription("LogEntryCategoryNumber")]
        public short CategoryNumber
        {
            get
            {
                return this.ShortFrom(this.dataBuf, this.bufOffset + 0x1c);
            }
        }

        [MonitoringDescription("LogEntryData")]
        public byte[] Data
        {
            get
            {
                int length = this.IntFrom(this.dataBuf, this.bufOffset + 0x30);
                byte[] destinationArray = new byte[length];
                Array.Copy(this.dataBuf, this.bufOffset + this.IntFrom(this.dataBuf, this.bufOffset + 0x34), destinationArray, 0, length);
                return destinationArray;
            }
        }

        [MonitoringDescription("LogEntryEntryType")]
        public EventLogEntryType EntryType
        {
            get
            {
                return (EventLogEntryType) this.ShortFrom(this.dataBuf, this.bufOffset + 0x18);
            }
        }

        [Obsolete("This property has been deprecated.  Please use System.Diagnostics.EventLogEntry.InstanceId instead.  http://go.microsoft.com/fwlink/?linkid=14202"), MonitoringDescription("LogEntryEventID")]
        public int EventID
        {
            get
            {
                return (this.IntFrom(this.dataBuf, this.bufOffset + 20) & 0x3fffffff);
            }
        }

        [MonitoringDescription("LogEntryIndex")]
        public int Index
        {
            get
            {
                return this.IntFrom(this.dataBuf, this.bufOffset + 8);
            }
        }

        [ComVisible(false), MonitoringDescription("LogEntryResourceId")]
        public long InstanceId
        {
            get
            {
                return (long) ((ulong) this.IntFrom(this.dataBuf, this.bufOffset + 20));
            }
        }

        [MonitoringDescription("LogEntryMachineName")]
        public string MachineName
        {
            get
            {
                int offset = this.bufOffset + 0x38;
                while (this.CharFrom(this.dataBuf, offset) != '\0')
                {
                    offset += 2;
                }
                offset += 2;
                char ch = this.CharFrom(this.dataBuf, offset);
                StringBuilder builder = new StringBuilder();
                while (ch != '\0')
                {
                    builder.Append(ch);
                    offset += 2;
                    ch = this.CharFrom(this.dataBuf, offset);
                }
                return builder.ToString();
            }
        }

        [MonitoringDescription("LogEntryMessage"), Editor("System.ComponentModel.Design.BinaryEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string Message
        {
            get
            {
                if (this.message == null)
                {
                    string messageLibraryNames = this.GetMessageLibraryNames("EventMessageFile");
                    int num = this.IntFrom(this.dataBuf, this.bufOffset + 20);
                    string msg = this.owner.FormatMessageWrapper(messageLibraryNames, (uint) num, this.ReplacementStrings);
                    if (msg == null)
                    {
                        StringBuilder builder = new StringBuilder(SR.GetString("MessageNotFormatted", new object[] { num, this.Source }));
                        string[] replacementStrings = this.ReplacementStrings;
                        for (int i = 0; i < replacementStrings.Length; i++)
                        {
                            if (i != 0)
                            {
                                builder.Append(", ");
                            }
                            builder.Append("'");
                            builder.Append(replacementStrings[i]);
                            builder.Append("'");
                        }
                        msg = builder.ToString();
                    }
                    else
                    {
                        msg = this.ReplaceMessageParameters(msg, this.ReplacementStrings);
                    }
                    this.message = msg;
                }
                return this.message;
            }
        }

        [MonitoringDescription("LogEntryReplacementStrings")]
        public string[] ReplacementStrings
        {
            get
            {
                string[] strArray = new string[this.ShortFrom(this.dataBuf, this.bufOffset + 0x1a)];
                int index = 0;
                int offset = this.bufOffset + this.IntFrom(this.dataBuf, this.bufOffset + 0x24);
                StringBuilder builder = new StringBuilder();
                while (index < strArray.Length)
                {
                    char ch = this.CharFrom(this.dataBuf, offset);
                    if (ch != '\0')
                    {
                        builder.Append(ch);
                    }
                    else
                    {
                        strArray[index] = builder.ToString();
                        index++;
                        builder = new StringBuilder();
                    }
                    offset += 2;
                }
                return strArray;
            }
        }

        [MonitoringDescription("LogEntrySource")]
        public string Source
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                int offset = this.bufOffset + 0x38;
                for (char ch = this.CharFrom(this.dataBuf, offset); ch != '\0'; ch = this.CharFrom(this.dataBuf, offset))
                {
                    builder.Append(ch);
                    offset += 2;
                }
                return builder.ToString();
            }
        }

        [MonitoringDescription("LogEntryTimeGenerated")]
        public DateTime TimeGenerated
        {
            get
            {
                return beginningOfTime.AddSeconds((double) this.IntFrom(this.dataBuf, this.bufOffset + 12)).ToLocalTime();
            }
        }

        [MonitoringDescription("LogEntryTimeWritten")]
        public DateTime TimeWritten
        {
            get
            {
                return beginningOfTime.AddSeconds((double) this.IntFrom(this.dataBuf, this.bufOffset + 0x10)).ToLocalTime();
            }
        }

        [MonitoringDescription("LogEntryUserName")]
        public string UserName
        {
            get
            {
                int num = this.IntFrom(this.dataBuf, this.bufOffset + 40);
                if (num == 0)
                {
                    return null;
                }
                byte[] destinationArray = new byte[num];
                Array.Copy(this.dataBuf, this.bufOffset + this.IntFrom(this.dataBuf, this.bufOffset + 0x2c), destinationArray, 0, destinationArray.Length);
                int capacity = 0x100;
                int num3 = 0x100;
                int eUse = 0;
                StringBuilder szUserName = new StringBuilder(capacity);
                StringBuilder szDomainName = new StringBuilder(num3);
                StringBuilder builder3 = new StringBuilder();
                if (Microsoft.Win32.UnsafeNativeMethods.LookupAccountSid(this.MachineName, destinationArray, szUserName, ref capacity, szDomainName, ref num3, ref eUse) != 0)
                {
                    builder3.Append(szDomainName.ToString());
                    builder3.Append(@"\");
                    builder3.Append(szUserName.ToString());
                }
                return builder3.ToString();
            }
        }

        private static class FieldOffsets
        {
            internal const int CLOSINGRECORDNUMBER = 0x20;
            internal const int DATALENGTH = 0x30;
            internal const int DATAOFFSET = 0x34;
            internal const int EVENTCATEGORY = 0x1c;
            internal const int EVENTID = 20;
            internal const int EVENTTYPE = 0x18;
            internal const int LENGTH = 0;
            internal const int NUMSTRINGS = 0x1a;
            internal const int RAWDATA = 0x38;
            internal const int RECORDNUMBER = 8;
            internal const int RESERVED = 4;
            internal const int RESERVEDFLAGS = 30;
            internal const int STRINGOFFSET = 0x24;
            internal const int TIMEGENERATED = 12;
            internal const int TIMEWRITTEN = 0x10;
            internal const int USERSIDLENGTH = 40;
            internal const int USERSIDOFFSET = 0x2c;
        }
    }
}

