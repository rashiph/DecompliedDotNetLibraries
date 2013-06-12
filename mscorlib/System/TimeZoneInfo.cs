namespace System
{
    using Microsoft.Win32;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.AccessControl;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;

    [Serializable, TypeForwardedFrom("System.Core, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=b77a5c561934e089"), HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class TimeZoneInfo : IEquatable<TimeZoneInfo>, ISerializable, IDeserializationCallback
    {
        private const string c_daylightValue = "Dlt";
        private const string c_disableDST = "DisableAutoDaylightTimeSet";
        private const string c_displayValue = "Display";
        private const string c_firstEntryValue = "FirstEntry";
        private const string c_lastEntryValue = "LastEntry";
        private const string c_localId = "Local";
        private const int c_maxKeyLength = 0xff;
        private const string c_muiDaylightValue = "MUI_Dlt";
        private const string c_muiDisplayValue = "MUI_Display";
        private const string c_muiStandardValue = "MUI_Std";
        private const int c_regByteLength = 0x2c;
        private const string c_standardValue = "Std";
        private const long c_ticksPerDay = 0xc92a69c000L;
        private const long c_ticksPerDayRange = 0xc92a6998f0L;
        private const long c_ticksPerHour = 0x861c46800L;
        private const long c_ticksPerMillisecond = 0x2710L;
        private const long c_ticksPerMinute = 0x23c34600L;
        private const long c_ticksPerSecond = 0x989680L;
        private const string c_timeZoneInfoRegistryHive = @"SYSTEM\CurrentControlSet\Control\TimeZoneInformation";
        private const string c_timeZoneInfoRegistryHivePermissionList = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\TimeZoneInformation";
        private const string c_timeZoneInfoValue = "TZI";
        private const string c_timeZonesRegistryHive = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Time Zones";
        private const string c_timeZonesRegistryHivePermissionList = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Time Zones";
        private const string c_utcId = "UTC";
        private static readonly bool c_VistaOrNewer = ((Environment.OSVersion.Platform == PlatformID.Win32NT) && (Environment.OSVersion.Version >= new Version(6, 0)));
        private AdjustmentRule[] m_adjustmentRules;
        private TimeSpan m_baseUtcOffset;
        private string m_daylightDisplayName;
        private string m_displayName;
        private string m_id;
        private string m_standardDisplayName;
        private bool m_supportsDaylightSavingTime;
        private static bool s_allSystemTimeZonesRead = false;
        private static object s_hiddenInternalSyncObject;
        private static Dictionary<string, TimeZoneInfo> s_hiddenSystemTimeZones;
        private static TimeZoneInfo s_localTimeZone;
        [ThreadStatic]
        private static OffsetAndRule s_oneYearLocalFromLocal;
        [ThreadStatic]
        private static OffsetAndRule s_oneYearLocalFromUtc;
        private static ReadOnlyCollection<TimeZoneInfo> s_readOnlySystemTimeZones;
        private static TimeZoneInfo s_utcTimeZone;

        [SecurityCritical]
        private TimeZoneInfo(Win32Native.TimeZoneInformation zone, bool dstDisabled)
        {
            if (string.IsNullOrEmpty(zone.StandardName))
            {
                this.m_id = "Local";
            }
            else
            {
                this.m_id = zone.StandardName;
            }
            this.m_baseUtcOffset = new TimeSpan(0, -zone.Bias, 0);
            if (!dstDisabled)
            {
                Win32Native.RegistryTimeZoneInformation timeZoneInformation = new Win32Native.RegistryTimeZoneInformation(zone);
                AdjustmentRule rule = CreateAdjustmentRuleFromTimeZoneInformation(timeZoneInformation, DateTime.MinValue.Date, DateTime.MaxValue.Date);
                if (rule != null)
                {
                    this.m_adjustmentRules = new AdjustmentRule[] { rule };
                }
            }
            ValidateTimeZoneInfo(this.m_id, this.m_baseUtcOffset, this.m_adjustmentRules, out this.m_supportsDaylightSavingTime);
            this.m_displayName = zone.StandardName;
            this.m_standardDisplayName = zone.StandardName;
            this.m_daylightDisplayName = zone.DaylightName;
        }

        private TimeZoneInfo(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            this.m_id = (string) info.GetValue("Id", typeof(string));
            this.m_displayName = (string) info.GetValue("DisplayName", typeof(string));
            this.m_standardDisplayName = (string) info.GetValue("StandardName", typeof(string));
            this.m_daylightDisplayName = (string) info.GetValue("DaylightName", typeof(string));
            this.m_baseUtcOffset = (TimeSpan) info.GetValue("BaseUtcOffset", typeof(TimeSpan));
            this.m_adjustmentRules = (AdjustmentRule[]) info.GetValue("AdjustmentRules", typeof(AdjustmentRule[]));
            this.m_supportsDaylightSavingTime = (bool) info.GetValue("SupportsDaylightSavingTime", typeof(bool));
        }

        private TimeZoneInfo(string id, TimeSpan baseUtcOffset, string displayName, string standardDisplayName, string daylightDisplayName, AdjustmentRule[] adjustmentRules, bool disableDaylightSavingTime)
        {
            bool flag;
            ValidateTimeZoneInfo(id, baseUtcOffset, adjustmentRules, out flag);
            if ((!disableDaylightSavingTime && (adjustmentRules != null)) && (adjustmentRules.Length > 0))
            {
                this.m_adjustmentRules = (AdjustmentRule[]) adjustmentRules.Clone();
            }
            this.m_id = id;
            this.m_baseUtcOffset = baseUtcOffset;
            this.m_displayName = displayName;
            this.m_standardDisplayName = standardDisplayName;
            this.m_daylightDisplayName = disableDaylightSavingTime ? null : daylightDisplayName;
            this.m_supportsDaylightSavingTime = flag && !disableDaylightSavingTime;
        }

        [SecuritySafeCritical]
        private static bool CheckDaylightSavingTimeDisabledDownlevel()
        {
            if (!c_VistaOrNewer)
            {
                try
                {
                    PermissionSet set = new PermissionSet(PermissionState.None);
                    set.AddPermission(new RegistryPermission(RegistryPermissionAccess.Read, @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\TimeZoneInformation"));
                    set.Assert();
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\TimeZoneInformation", RegistryKeyPermissionCheck.Default, RegistryRights.ExecuteKey))
                    {
                        if (key == null)
                        {
                            return false;
                        }
                        int num = 0;
                        try
                        {
                            num = (int) key.GetValue("DisableAutoDaylightTimeSet", 0, RegistryValueOptions.None);
                        }
                        catch (InvalidCastException)
                        {
                        }
                        if (num == 1)
                        {
                            return true;
                        }
                    }
                }
                finally
                {
                    PermissionSet.RevertAssert();
                }
            }
            return false;
        }

        [SecurityCritical]
        private static bool CheckDaylightSavingTimeNotSupported(Win32Native.TimeZoneInformation timeZone)
        {
            return (((((timeZone.DaylightDate.Year == timeZone.StandardDate.Year) && (timeZone.DaylightDate.Month == timeZone.StandardDate.Month)) && ((timeZone.DaylightDate.DayOfWeek == timeZone.StandardDate.DayOfWeek) && (timeZone.DaylightDate.Day == timeZone.StandardDate.Day))) && (((timeZone.DaylightDate.Hour == timeZone.StandardDate.Hour) && (timeZone.DaylightDate.Minute == timeZone.StandardDate.Minute)) && (timeZone.DaylightDate.Second == timeZone.StandardDate.Second))) && (timeZone.DaylightDate.Milliseconds == timeZone.StandardDate.Milliseconds));
        }

        private static bool CheckIsDst(DateTime startTime, DateTime time, DateTime endTime)
        {
            if (startTime.Year != endTime.Year)
            {
                endTime = endTime.AddYears(startTime.Year - endTime.Year);
            }
            if (startTime.Year != time.Year)
            {
                time = time.AddYears(startTime.Year - time.Year);
            }
            if (startTime > endTime)
            {
                return ((time < endTime) || (time >= startTime));
            }
            return ((time >= startTime) && (time < endTime));
        }

        public static void ClearCachedData()
        {
            lock (s_internalSyncObject)
            {
                s_localTimeZone = null;
                s_utcTimeZone = null;
                s_systemTimeZones = null;
                s_readOnlySystemTimeZones = null;
                s_allSystemTimeZonesRead = false;
                s_oneYearLocalFromLocal = null;
                s_oneYearLocalFromUtc = null;
            }
        }

        public static DateTime ConvertTime(DateTime dateTime, TimeZoneInfo destinationTimeZone)
        {
            if (destinationTimeZone == null)
            {
                throw new ArgumentNullException("destinationTimeZone");
            }
            if (dateTime.Kind == DateTimeKind.Utc)
            {
                lock (s_internalSyncObject)
                {
                    return ConvertTime(dateTime, Utc, destinationTimeZone);
                }
            }
            lock (s_internalSyncObject)
            {
                return ConvertTime(dateTime, Local, destinationTimeZone);
            }
        }

        public static DateTimeOffset ConvertTime(DateTimeOffset dateTimeOffset, TimeZoneInfo destinationTimeZone)
        {
            if (destinationTimeZone == null)
            {
                throw new ArgumentNullException("destinationTimeZone");
            }
            DateTime utcDateTime = dateTimeOffset.UtcDateTime;
            TimeSpan utcOffsetFromUtc = GetUtcOffsetFromUtc(utcDateTime, destinationTimeZone);
            long ticks = utcDateTime.Ticks + utcOffsetFromUtc.Ticks;
            if (ticks > DateTimeOffset.MaxValue.Ticks)
            {
                return DateTimeOffset.MaxValue;
            }
            if (ticks < DateTimeOffset.MinValue.Ticks)
            {
                return DateTimeOffset.MinValue;
            }
            return new DateTimeOffset(ticks, utcOffsetFromUtc);
        }

        public static DateTime ConvertTime(DateTime dateTime, TimeZoneInfo sourceTimeZone, TimeZoneInfo destinationTimeZone)
        {
            return ConvertTime(dateTime, sourceTimeZone, destinationTimeZone, TimeZoneInfoOptions.None);
        }

        internal static DateTime ConvertTime(DateTime dateTime, TimeZoneInfo sourceTimeZone, TimeZoneInfo destinationTimeZone, TimeZoneInfoOptions flags)
        {
            if (sourceTimeZone == null)
            {
                throw new ArgumentNullException("sourceTimeZone");
            }
            if (destinationTimeZone == null)
            {
                throw new ArgumentNullException("destinationTimeZone");
            }
            DateTimeKind correspondingKind = sourceTimeZone.GetCorrespondingKind();
            if ((((flags & TimeZoneInfoOptions.NoThrowOnInvalidTime) == 0) && (dateTime.Kind != DateTimeKind.Unspecified)) && (dateTime.Kind != correspondingKind))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_ConvertMismatch"), "sourceTimeZone");
            }
            AdjustmentRule adjustmentRuleForTime = sourceTimeZone.GetAdjustmentRuleForTime(dateTime);
            TimeSpan baseUtcOffset = sourceTimeZone.BaseUtcOffset;
            if (adjustmentRuleForTime != null)
            {
                bool flag = false;
                DaylightTime daylightTime = GetDaylightTime(dateTime.Year, adjustmentRuleForTime);
                if (((flags & TimeZoneInfoOptions.NoThrowOnInvalidTime) == 0) && GetIsInvalidTime(dateTime, adjustmentRuleForTime, daylightTime))
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_DateTimeIsInvalid"), "dateTime");
                }
                flag = GetIsDaylightSavings(dateTime, adjustmentRuleForTime, daylightTime, flags);
                baseUtcOffset += flag ? adjustmentRuleForTime.DaylightDelta : TimeSpan.Zero;
            }
            DateTimeKind kind = destinationTimeZone.GetCorrespondingKind();
            if (((dateTime.Kind != DateTimeKind.Unspecified) && (correspondingKind != DateTimeKind.Unspecified)) && (correspondingKind == kind))
            {
                return dateTime;
            }
            long ticks = dateTime.Ticks - baseUtcOffset.Ticks;
            bool isAmbiguousLocalDst = false;
            DateTime time2 = ConvertUtcToTimeZone(ticks, destinationTimeZone, out isAmbiguousLocalDst);
            if (kind == DateTimeKind.Local)
            {
                return new DateTime(time2.Ticks, DateTimeKind.Local, isAmbiguousLocalDst);
            }
            return new DateTime(time2.Ticks, kind);
        }

        public static DateTime ConvertTimeBySystemTimeZoneId(DateTime dateTime, string destinationTimeZoneId)
        {
            return ConvertTime(dateTime, FindSystemTimeZoneById(destinationTimeZoneId));
        }

        public static DateTimeOffset ConvertTimeBySystemTimeZoneId(DateTimeOffset dateTimeOffset, string destinationTimeZoneId)
        {
            return ConvertTime(dateTimeOffset, FindSystemTimeZoneById(destinationTimeZoneId));
        }

        public static DateTime ConvertTimeBySystemTimeZoneId(DateTime dateTime, string sourceTimeZoneId, string destinationTimeZoneId)
        {
            if ((dateTime.Kind == DateTimeKind.Local) && (string.Compare(sourceTimeZoneId, Local.Id, StringComparison.OrdinalIgnoreCase) == 0))
            {
                lock (s_internalSyncObject)
                {
                    return ConvertTime(dateTime, Local, FindSystemTimeZoneById(destinationTimeZoneId));
                }
            }
            if ((dateTime.Kind == DateTimeKind.Utc) && (string.Compare(sourceTimeZoneId, Utc.Id, StringComparison.OrdinalIgnoreCase) == 0))
            {
                lock (s_internalSyncObject)
                {
                    return ConvertTime(dateTime, Utc, FindSystemTimeZoneById(destinationTimeZoneId));
                }
            }
            return ConvertTime(dateTime, FindSystemTimeZoneById(sourceTimeZoneId), FindSystemTimeZoneById(destinationTimeZoneId));
        }

        public static DateTime ConvertTimeFromUtc(DateTime dateTime, TimeZoneInfo destinationTimeZone)
        {
            lock (s_internalSyncObject)
            {
                return ConvertTime(dateTime, Utc, destinationTimeZone);
            }
        }

        public static DateTime ConvertTimeToUtc(DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Utc)
            {
                return dateTime;
            }
            lock (s_internalSyncObject)
            {
                return ConvertTime(dateTime, Local, Utc);
            }
        }

        public static DateTime ConvertTimeToUtc(DateTime dateTime, TimeZoneInfo sourceTimeZone)
        {
            lock (s_internalSyncObject)
            {
                return ConvertTime(dateTime, sourceTimeZone, Utc);
            }
        }

        internal static DateTime ConvertTimeToUtc(DateTime dateTime, TimeZoneInfoOptions flags)
        {
            if (dateTime.Kind == DateTimeKind.Utc)
            {
                return dateTime;
            }
            lock (s_internalSyncObject)
            {
                return ConvertTime(dateTime, Local, Utc, flags);
            }
        }

        private static DateTime ConvertUtcToTimeZone(long ticks, TimeZoneInfo destinationTimeZone, out bool isAmbiguousLocalDst)
        {
            DateTime maxValue;
            if (ticks > DateTime.MaxValue.Ticks)
            {
                maxValue = DateTime.MaxValue;
            }
            else if (ticks < DateTime.MinValue.Ticks)
            {
                maxValue = DateTime.MinValue;
            }
            else
            {
                maxValue = new DateTime(ticks);
            }
            TimeSpan span = GetUtcOffsetFromUtc(maxValue, destinationTimeZone, out isAmbiguousLocalDst);
            ticks += span.Ticks;
            if (ticks > DateTime.MaxValue.Ticks)
            {
                return DateTime.MaxValue;
            }
            if (ticks < DateTime.MinValue.Ticks)
            {
                return DateTime.MinValue;
            }
            return new DateTime(ticks);
        }

        [SecurityCritical]
        private static AdjustmentRule CreateAdjustmentRuleFromTimeZoneInformation(Win32Native.RegistryTimeZoneInformation timeZoneInformation, DateTime startDate, DateTime endDate)
        {
            TransitionTime time;
            TransitionTime time2;
            if (timeZoneInformation.StandardDate.Month == 0)
            {
                return null;
            }
            if (!TransitionTimeFromTimeZoneInformation(timeZoneInformation, out time, true))
            {
                return null;
            }
            if (!TransitionTimeFromTimeZoneInformation(timeZoneInformation, out time2, false))
            {
                return null;
            }
            if (time.Equals(time2))
            {
                return null;
            }
            return AdjustmentRule.CreateAdjustmentRule(startDate, endDate, new TimeSpan(0, -timeZoneInformation.DaylightBias, 0), time, time2);
        }

        public static TimeZoneInfo CreateCustomTimeZone(string id, TimeSpan baseUtcOffset, string displayName, string standardDisplayName)
        {
            return new TimeZoneInfo(id, baseUtcOffset, displayName, standardDisplayName, standardDisplayName, null, false);
        }

        public static TimeZoneInfo CreateCustomTimeZone(string id, TimeSpan baseUtcOffset, string displayName, string standardDisplayName, string daylightDisplayName, AdjustmentRule[] adjustmentRules)
        {
            return new TimeZoneInfo(id, baseUtcOffset, displayName, standardDisplayName, daylightDisplayName, adjustmentRules, false);
        }

        public static TimeZoneInfo CreateCustomTimeZone(string id, TimeSpan baseUtcOffset, string displayName, string standardDisplayName, string daylightDisplayName, AdjustmentRule[] adjustmentRules, bool disableDaylightSavingTime)
        {
            return new TimeZoneInfo(id, baseUtcOffset, displayName, standardDisplayName, daylightDisplayName, adjustmentRules, disableDaylightSavingTime);
        }

        public bool Equals(TimeZoneInfo other)
        {
            return (((other != null) && (string.Compare(this.m_id, other.m_id, StringComparison.OrdinalIgnoreCase) == 0)) && this.HasSameRules(other));
        }

        [SecuritySafeCritical]
        private static string FindIdFromTimeZoneInformation(Win32Native.TimeZoneInformation timeZone, out bool dstDisabled)
        {
            dstDisabled = false;
            try
            {
                PermissionSet set = new PermissionSet(PermissionState.None);
                set.AddPermission(new RegistryPermission(RegistryPermissionAccess.Read, @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Time Zones"));
                set.Assert();
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Time Zones", RegistryKeyPermissionCheck.Default, RegistryRights.ExecuteKey))
                {
                    if (key == null)
                    {
                        return null;
                    }
                    foreach (string str in key.GetSubKeyNames())
                    {
                        if (TryCompareTimeZoneInformationToRegistry(timeZone, str, out dstDisabled))
                        {
                            return str;
                        }
                    }
                }
            }
            finally
            {
                PermissionSet.RevertAssert();
            }
            return null;
        }

        public static TimeZoneInfo FindSystemTimeZoneById(string id)
        {
            if (string.Compare(id, "UTC", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return Utc;
            }
            lock (s_internalSyncObject)
            {
                return GetTimeZone(id);
            }
        }

        public static TimeZoneInfo FromSerializedString(string source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (source.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidSerializedString", new object[] { source }), "source");
            }
            return StringSerializer.GetDeserializedTimeZoneInfo(source);
        }

        [SecuritySafeCritical]
        private AdjustmentRule GetAdjustmentRuleForTime(DateTime dateTime)
        {
            if ((this.m_adjustmentRules != null) && (this.m_adjustmentRules.Length != 0))
            {
                if (!c_VistaOrNewer && (this.GetCorrespondingKind() == DateTimeKind.Local))
                {
                    return GetOneYearLocalFromLocal(dateTime.Year).rule;
                }
                DateTime date = dateTime.Date;
                for (int i = 0; i < this.m_adjustmentRules.Length; i++)
                {
                    if ((this.m_adjustmentRules[i].DateStart <= date) && (this.m_adjustmentRules[i].DateEnd >= date))
                    {
                        return this.m_adjustmentRules[i];
                    }
                }
            }
            return null;
        }

        public AdjustmentRule[] GetAdjustmentRules()
        {
            if (this.m_adjustmentRules == null)
            {
                return new AdjustmentRule[0];
            }
            return (AdjustmentRule[]) this.m_adjustmentRules.Clone();
        }

        public TimeSpan[] GetAmbiguousTimeOffsets(DateTime dateTime)
        {
            DateTime time;
            bool flag3;
            if (!this.SupportsDaylightSavingTime)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_DateTimeIsNotAmbiguous"), "dateTime");
            }
            if (dateTime.Kind == DateTimeKind.Local)
            {
                lock (s_internalSyncObject)
                {
                    time = ConvertTime(dateTime, Local, this);
                    goto Label_0089;
                }
            }
            if (dateTime.Kind == DateTimeKind.Utc)
            {
                lock (s_internalSyncObject)
                {
                    time = ConvertTime(dateTime, Utc, this);
                    goto Label_0089;
                }
            }
            time = dateTime;
        Label_0089:
            flag3 = false;
            AdjustmentRule adjustmentRuleForTime = this.GetAdjustmentRuleForTime(time);
            if (adjustmentRuleForTime != null)
            {
                DaylightTime daylightTime = GetDaylightTime(time.Year, adjustmentRuleForTime);
                flag3 = GetIsAmbiguousTime(time, adjustmentRuleForTime, daylightTime);
            }
            if (!flag3)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_DateTimeIsNotAmbiguous"), "dateTime");
            }
            TimeSpan[] spanArray = new TimeSpan[2];
            if (adjustmentRuleForTime.DaylightDelta > TimeSpan.Zero)
            {
                spanArray[0] = this.m_baseUtcOffset;
                spanArray[1] = this.m_baseUtcOffset + adjustmentRuleForTime.DaylightDelta;
                return spanArray;
            }
            spanArray[0] = this.m_baseUtcOffset + adjustmentRuleForTime.DaylightDelta;
            spanArray[1] = this.m_baseUtcOffset;
            return spanArray;
        }

        public TimeSpan[] GetAmbiguousTimeOffsets(DateTimeOffset dateTimeOffset)
        {
            if (!this.SupportsDaylightSavingTime)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_DateTimeOffsetIsNotAmbiguous"), "dateTimeOffset");
            }
            DateTime dateTime = ConvertTime(dateTimeOffset, this).DateTime;
            bool flag = false;
            AdjustmentRule adjustmentRuleForTime = this.GetAdjustmentRuleForTime(dateTime);
            if (adjustmentRuleForTime != null)
            {
                DaylightTime daylightTime = GetDaylightTime(dateTime.Year, adjustmentRuleForTime);
                flag = GetIsAmbiguousTime(dateTime, adjustmentRuleForTime, daylightTime);
            }
            if (!flag)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_DateTimeOffsetIsNotAmbiguous"), "dateTimeOffset");
            }
            TimeSpan[] spanArray = new TimeSpan[2];
            if (adjustmentRuleForTime.DaylightDelta > TimeSpan.Zero)
            {
                spanArray[0] = this.m_baseUtcOffset;
                spanArray[1] = this.m_baseUtcOffset + adjustmentRuleForTime.DaylightDelta;
                return spanArray;
            }
            spanArray[0] = this.m_baseUtcOffset + adjustmentRuleForTime.DaylightDelta;
            spanArray[1] = this.m_baseUtcOffset;
            return spanArray;
        }

        private DateTimeKind GetCorrespondingKind()
        {
            if (this == s_utcTimeZone)
            {
                return DateTimeKind.Utc;
            }
            if (this == s_localTimeZone)
            {
                return DateTimeKind.Local;
            }
            return DateTimeKind.Unspecified;
        }

        [SecurityCritical]
        private static TimeZoneInfo GetCurrentOneYearLocal()
        {
            Win32Native.TimeZoneInformation lpTimeZoneInformation = new Win32Native.TimeZoneInformation();
            long timeZoneInformation = UnsafeNativeMethods.GetTimeZoneInformation(out lpTimeZoneInformation);
            if (timeZoneInformation == -1L)
            {
                return CreateCustomTimeZone("Local", TimeSpan.Zero, "Local", "Local");
            }
            return GetLocalTimeZoneFromWin32Data(lpTimeZoneInformation, false);
        }

        internal static TimeSpan GetDateTimeNowUtcOffsetFromUtc(DateTime time, out bool isAmbiguousLocalDst)
        {
            bool flag = false;
            isAmbiguousLocalDst = false;
            OffsetAndRule oneYearLocalFromUtc = GetOneYearLocalFromUtc(time.Year);
            TimeSpan offset = oneYearLocalFromUtc.offset;
            if (oneYearLocalFromUtc.rule != null)
            {
                flag = GetIsDaylightSavingsFromUtc(time, time.Year, oneYearLocalFromUtc.offset, oneYearLocalFromUtc.rule, out isAmbiguousLocalDst);
                offset += flag ? oneYearLocalFromUtc.rule.DaylightDelta : TimeSpan.Zero;
            }
            return offset;
        }

        private static DaylightTime GetDaylightTime(int year, AdjustmentRule rule)
        {
            TimeSpan daylightDelta = rule.DaylightDelta;
            DateTime start = TransitionTimeToDateTime(year, rule.DaylightTransitionStart);
            return new DaylightTime(start, TransitionTimeToDateTime(year, rule.DaylightTransitionEnd), daylightDelta);
        }

        public override int GetHashCode()
        {
            return this.m_id.ToUpper(CultureInfo.InvariantCulture).GetHashCode();
        }

        private static bool GetIsAmbiguousTime(DateTime time, AdjustmentRule rule, DaylightTime daylightTime)
        {
            bool flag = false;
            if ((rule != null) && (rule.DaylightDelta != TimeSpan.Zero))
            {
                DateTime end;
                DateTime time3;
                DateTime time4;
                DateTime time5;
                if (rule.DaylightDelta > TimeSpan.Zero)
                {
                    end = daylightTime.End;
                    time3 = daylightTime.End - rule.DaylightDelta;
                }
                else
                {
                    end = daylightTime.Start;
                    time3 = daylightTime.Start + rule.DaylightDelta;
                }
                flag = (time >= time3) && (time < end);
                if (flag || (end.Year == time3.Year))
                {
                    return flag;
                }
                try
                {
                    time4 = end.AddYears(1);
                    time5 = time3.AddYears(1);
                    flag = (time >= time5) && (time < time4);
                }
                catch (ArgumentOutOfRangeException)
                {
                }
                if (flag)
                {
                    return flag;
                }
                try
                {
                    time4 = end.AddYears(-1);
                    time5 = time3.AddYears(-1);
                    flag = (time >= time5) && (time < time4);
                }
                catch (ArgumentOutOfRangeException)
                {
                }
            }
            return flag;
        }

        private static bool GetIsDaylightSavings(DateTime time, AdjustmentRule rule, DaylightTime daylightTime, TimeZoneInfoOptions flags)
        {
            DateTime time2;
            DateTime end;
            if (rule == null)
            {
                return false;
            }
            if (time.Kind == DateTimeKind.Local)
            {
                time2 = daylightTime.Start + daylightTime.Delta;
                end = daylightTime.End;
            }
            else
            {
                bool flag = rule.DaylightDelta > TimeSpan.Zero;
                time2 = daylightTime.Start + (flag ? rule.DaylightDelta : TimeSpan.Zero);
                end = daylightTime.End + (flag ? -rule.DaylightDelta : TimeSpan.Zero);
            }
            bool flag2 = CheckIsDst(time2, time, end);
            if ((flag2 && (time.Kind == DateTimeKind.Local)) && GetIsAmbiguousTime(time, rule, daylightTime))
            {
                flag2 = time.IsAmbiguousDaylightSavingTime();
            }
            return flag2;
        }

        private static bool GetIsDaylightSavingsFromUtc(DateTime time, int Year, TimeSpan utc, AdjustmentRule rule, out bool isAmbiguousLocalDst)
        {
            DateTime time5;
            DateTime time6;
            isAmbiguousLocalDst = false;
            if (rule == null)
            {
                return false;
            }
            TimeSpan span = utc;
            DaylightTime daylightTime = GetDaylightTime(Year, rule);
            DateTime startTime = daylightTime.Start - span;
            DateTime endTime = (daylightTime.End - span) - rule.DaylightDelta;
            if (daylightTime.Delta.Ticks > 0L)
            {
                time5 = endTime - daylightTime.Delta;
                time6 = endTime;
            }
            else
            {
                time5 = startTime;
                time6 = startTime - daylightTime.Delta;
            }
            bool flag = CheckIsDst(startTime, time, endTime);
            if (flag)
            {
                isAmbiguousLocalDst = (time >= time5) && (time < time6);
                if (isAmbiguousLocalDst || (time5.Year == time6.Year))
                {
                    return flag;
                }
                try
                {
                    time5.AddYears(1);
                    time6.AddYears(1);
                    isAmbiguousLocalDst = (time >= time5) && (time < time6);
                }
                catch (ArgumentOutOfRangeException)
                {
                }
                if (isAmbiguousLocalDst)
                {
                    return flag;
                }
                try
                {
                    time5.AddYears(-1);
                    time6.AddYears(-1);
                    isAmbiguousLocalDst = (time >= time5) && (time < time6);
                }
                catch (ArgumentOutOfRangeException)
                {
                }
            }
            return flag;
        }

        private static bool GetIsInvalidTime(DateTime time, AdjustmentRule rule, DaylightTime daylightTime)
        {
            bool flag = false;
            if ((rule != null) && (rule.DaylightDelta != TimeSpan.Zero))
            {
                DateTime end;
                DateTime time3;
                DateTime time4;
                DateTime time5;
                if (rule.DaylightDelta < TimeSpan.Zero)
                {
                    end = daylightTime.End;
                    time3 = daylightTime.End - rule.DaylightDelta;
                }
                else
                {
                    end = daylightTime.Start;
                    time3 = daylightTime.Start + rule.DaylightDelta;
                }
                flag = (time >= end) && (time < time3);
                if (flag || (end.Year == time3.Year))
                {
                    return flag;
                }
                try
                {
                    time4 = end.AddYears(1);
                    time5 = time3.AddYears(1);
                    flag = (time >= time4) && (time < time5);
                }
                catch (ArgumentOutOfRangeException)
                {
                }
                if (flag)
                {
                    return flag;
                }
                try
                {
                    time4 = end.AddYears(-1);
                    time5 = time3.AddYears(-1);
                    flag = (time >= time4) && (time < time5);
                }
                catch (ArgumentOutOfRangeException)
                {
                }
            }
            return flag;
        }

        [SecuritySafeCritical]
        private static TimeZoneInfo GetLocalTimeZone()
        {
            string id = null;
            bool flag3;
            TimeZoneInfo info3;
            Exception exception3;
            if (c_VistaOrNewer)
            {
                TimeZoneInfo info;
                Exception exception;
                bool flag2;
                TimeZoneInfo info2;
                Exception exception2;
                Win32Native.DynamicTimeZoneInformation lpDynamicTimeZoneInformation = new Win32Native.DynamicTimeZoneInformation();
                long dynamicTimeZoneInformation = UnsafeNativeMethods.GetDynamicTimeZoneInformation(out lpDynamicTimeZoneInformation);
                if (dynamicTimeZoneInformation == -1L)
                {
                    return CreateCustomTimeZone("Local", TimeSpan.Zero, "Local", "Local");
                }
                Win32Native.TimeZoneInformation information2 = new Win32Native.TimeZoneInformation(lpDynamicTimeZoneInformation);
                if (lpDynamicTimeZoneInformation.DynamicDaylightTimeDisabled)
                {
                    return GetLocalTimeZoneFromWin32Data(information2, false);
                }
                if (!string.IsNullOrEmpty(lpDynamicTimeZoneInformation.TimeZoneKeyName) && (TryGetTimeZone(lpDynamicTimeZoneInformation.TimeZoneKeyName, false, out info, out exception) == TimeZoneInfoResult.Success))
                {
                    return info;
                }
                id = FindIdFromTimeZoneInformation(information2, out flag2);
                if ((id != null) && (TryGetTimeZone(id, false, out info2, out exception2) == TimeZoneInfoResult.Success))
                {
                    return info2;
                }
                return GetLocalTimeZoneFromWin32Data(information2, flag2);
            }
            Win32Native.TimeZoneInformation lpTimeZoneInformation = new Win32Native.TimeZoneInformation();
            long timeZoneInformation = UnsafeNativeMethods.GetTimeZoneInformation(out lpTimeZoneInformation);
            if (timeZoneInformation == -1L)
            {
                return CreateCustomTimeZone("Local", TimeSpan.Zero, "Local", "Local");
            }
            id = FindIdFromTimeZoneInformation(lpTimeZoneInformation, out flag3);
            if ((id != null) && (TryGetTimeZone(id, flag3, out info3, out exception3) == TimeZoneInfoResult.Success))
            {
                return info3;
            }
            return GetLocalTimeZoneFromWin32Data(lpTimeZoneInformation, flag3);
        }

        [SecurityCritical]
        private static TimeZoneInfo GetLocalTimeZoneFromWin32Data(Win32Native.TimeZoneInformation timeZoneInformation, bool dstDisabled)
        {
            try
            {
                return new TimeZoneInfo(timeZoneInformation, dstDisabled);
            }
            catch (ArgumentException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
            if (!dstDisabled)
            {
                try
                {
                    return new TimeZoneInfo(timeZoneInformation, true);
                }
                catch (ArgumentException)
                {
                }
                catch (InvalidTimeZoneException)
                {
                }
            }
            return CreateCustomTimeZone("Local", TimeSpan.Zero, "Local", "Local");
        }

        [SecuritySafeCritical]
        private static OffsetAndRule GetOneYearLocalFromLocal(int year)
        {
            if ((s_oneYearLocalFromLocal == null) || (s_oneYearLocalFromLocal.year != year))
            {
                TimeZoneInfo currentOneYearLocal = GetCurrentOneYearLocal();
                AdjustmentRule rule = (currentOneYearLocal.m_adjustmentRules == null) ? null : currentOneYearLocal.m_adjustmentRules[0];
                s_oneYearLocalFromLocal = new OffsetAndRule(year, currentOneYearLocal.BaseUtcOffset, rule);
            }
            return s_oneYearLocalFromLocal;
        }

        [SecuritySafeCritical]
        private static OffsetAndRule GetOneYearLocalFromUtc(int year)
        {
            if ((s_oneYearLocalFromUtc == null) || (s_oneYearLocalFromUtc.year != year))
            {
                TimeZoneInfo currentOneYearLocal = GetCurrentOneYearLocal();
                AdjustmentRule rule = (currentOneYearLocal.m_adjustmentRules == null) ? null : currentOneYearLocal.m_adjustmentRules[0];
                s_oneYearLocalFromUtc = new OffsetAndRule(year, currentOneYearLocal.BaseUtcOffset, rule);
            }
            return s_oneYearLocalFromUtc;
        }

        [SecuritySafeCritical]
        public static ReadOnlyCollection<TimeZoneInfo> GetSystemTimeZones()
        {
            lock (s_internalSyncObject)
            {
                if (!s_allSystemTimeZonesRead)
                {
                    PermissionSet set = new PermissionSet(PermissionState.None);
                    set.AddPermission(new RegistryPermission(RegistryPermissionAccess.Read, @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Time Zones"));
                    set.Assert();
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Time Zones", RegistryKeyPermissionCheck.Default, RegistryRights.ExecuteKey))
                    {
                        if (key == null)
                        {
                            List<TimeZoneInfo> list;
                            if (s_systemTimeZones != null)
                            {
                                list = new List<TimeZoneInfo>(s_systemTimeZones.Values);
                            }
                            else
                            {
                                list = new List<TimeZoneInfo>();
                            }
                            s_readOnlySystemTimeZones = new ReadOnlyCollection<TimeZoneInfo>(list);
                            s_allSystemTimeZonesRead = true;
                            return s_readOnlySystemTimeZones;
                        }
                        foreach (string str in key.GetSubKeyNames())
                        {
                            TimeZoneInfo info;
                            Exception exception;
                            TryGetTimeZone(str, false, out info, out exception);
                        }
                    }
                    IComparer<TimeZoneInfo> comparer = new TimeZoneInfoComparer();
                    List<TimeZoneInfo> list2 = new List<TimeZoneInfo>(s_systemTimeZones.Values);
                    list2.Sort(comparer);
                    s_readOnlySystemTimeZones = new ReadOnlyCollection<TimeZoneInfo>(list2);
                    s_allSystemTimeZonesRead = true;
                }
                return s_readOnlySystemTimeZones;
            }
        }

        private static TimeZoneInfo GetTimeZone(string id)
        {
            TimeZoneInfo info;
            Exception exception;
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }
            if (((id.Length == 0) || (id.Length > 0xff)) || id.Contains("\0"))
            {
                throw new TimeZoneNotFoundException(Environment.GetResourceString("TimeZoneNotFound_MissingRegistryData", new object[] { id }));
            }
            switch (TryGetTimeZone(id, false, out info, out exception))
            {
                case TimeZoneInfoResult.Success:
                    return info;

                case TimeZoneInfoResult.InvalidTimeZoneException:
                    throw new InvalidTimeZoneException(Environment.GetResourceString("InvalidTimeZone_InvalidRegistryData", new object[] { id }), exception);

                case TimeZoneInfoResult.SecurityException:
                    throw new SecurityException(Environment.GetResourceString("Security_CannotReadRegistryData", new object[] { id }), exception);
            }
            throw new TimeZoneNotFoundException(Environment.GetResourceString("TimeZoneNotFound_MissingRegistryData", new object[] { id }), exception);
        }

        public TimeSpan GetUtcOffset(DateTime dateTime)
        {
            return this.GetUtcOffset(dateTime, TimeZoneInfoOptions.NoThrowOnInvalidTime);
        }

        public TimeSpan GetUtcOffset(DateTimeOffset dateTimeOffset)
        {
            return GetUtcOffsetFromUtc(dateTimeOffset.UtcDateTime, this);
        }

        internal TimeSpan GetUtcOffset(DateTime dateTime, TimeZoneInfoOptions flags)
        {
            if (dateTime.Kind == DateTimeKind.Local)
            {
                DateTime time;
                lock (s_internalSyncObject)
                {
                    if (this.GetCorrespondingKind() == DateTimeKind.Local)
                    {
                        return GetUtcOffset(dateTime, this, flags);
                    }
                    time = ConvertTime(dateTime, Local, Utc, flags);
                }
                return GetUtcOffsetFromUtc(time, this);
            }
            if (dateTime.Kind != DateTimeKind.Utc)
            {
                return GetUtcOffset(dateTime, this, flags);
            }
            if (this.GetCorrespondingKind() == DateTimeKind.Utc)
            {
                return this.m_baseUtcOffset;
            }
            return GetUtcOffsetFromUtc(dateTime, this);
        }

        private static TimeSpan GetUtcOffset(DateTime time, TimeZoneInfo zone, TimeZoneInfoOptions flags)
        {
            TimeSpan baseUtcOffset = zone.BaseUtcOffset;
            AdjustmentRule adjustmentRuleForTime = zone.GetAdjustmentRuleForTime(time);
            if (adjustmentRuleForTime != null)
            {
                DaylightTime daylightTime = GetDaylightTime(time.Year, adjustmentRuleForTime);
                bool flag = GetIsDaylightSavings(time, adjustmentRuleForTime, daylightTime, flags);
                baseUtcOffset += flag ? adjustmentRuleForTime.DaylightDelta : TimeSpan.Zero;
            }
            return baseUtcOffset;
        }

        private static TimeSpan GetUtcOffsetFromUtc(DateTime time, TimeZoneInfo zone)
        {
            bool flag;
            return GetUtcOffsetFromUtc(time, zone, out flag);
        }

        private static TimeSpan GetUtcOffsetFromUtc(DateTime time, TimeZoneInfo zone, out bool isDaylightSavings)
        {
            bool flag;
            return GetUtcOffsetFromUtc(time, zone, out isDaylightSavings, out flag);
        }

        internal static TimeSpan GetUtcOffsetFromUtc(DateTime time, TimeZoneInfo zone, out bool isDaylightSavings, out bool isAmbiguousLocalDst)
        {
            int year;
            AdjustmentRule adjustmentRuleForTime;
            isDaylightSavings = false;
            isAmbiguousLocalDst = false;
            TimeSpan baseUtcOffset = zone.BaseUtcOffset;
            if (time > new DateTime(0x270f, 12, 0x1f))
            {
                adjustmentRuleForTime = zone.GetAdjustmentRuleForTime(DateTime.MaxValue);
                year = 0x270f;
            }
            else if (time < new DateTime(1, 1, 2))
            {
                adjustmentRuleForTime = zone.GetAdjustmentRuleForTime(DateTime.MinValue);
                year = 1;
            }
            else
            {
                DateTime dateTime = time + baseUtcOffset;
                year = time.Year;
                adjustmentRuleForTime = zone.GetAdjustmentRuleForTime(dateTime);
            }
            if (adjustmentRuleForTime != null)
            {
                isDaylightSavings = GetIsDaylightSavingsFromUtc(time, year, zone.m_baseUtcOffset, adjustmentRuleForTime, out isAmbiguousLocalDst);
                baseUtcOffset += isDaylightSavings ? adjustmentRuleForTime.DaylightDelta : TimeSpan.Zero;
            }
            return baseUtcOffset;
        }

        public bool HasSameRules(TimeZoneInfo other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            if ((this.m_baseUtcOffset != other.m_baseUtcOffset) || (this.m_supportsDaylightSavingTime != other.m_supportsDaylightSavingTime))
            {
                return false;
            }
            AdjustmentRule[] adjustmentRules = this.m_adjustmentRules;
            AdjustmentRule[] ruleArray2 = other.m_adjustmentRules;
            bool flag = ((adjustmentRules == null) && (ruleArray2 == null)) || ((adjustmentRules != null) && (ruleArray2 != null));
            if (!flag)
            {
                return false;
            }
            if (adjustmentRules != null)
            {
                if (adjustmentRules.Length != ruleArray2.Length)
                {
                    return false;
                }
                for (int i = 0; i < adjustmentRules.Length; i++)
                {
                    if (!adjustmentRules[i].Equals(ruleArray2[i]))
                    {
                        return false;
                    }
                }
            }
            return flag;
        }

        public bool IsAmbiguousTime(DateTime dateTime)
        {
            return this.IsAmbiguousTime(dateTime, TimeZoneInfoOptions.NoThrowOnInvalidTime);
        }

        public bool IsAmbiguousTime(DateTimeOffset dateTimeOffset)
        {
            if (!this.m_supportsDaylightSavingTime)
            {
                return false;
            }
            DateTimeOffset offset = ConvertTime(dateTimeOffset, this);
            return this.IsAmbiguousTime(offset.DateTime);
        }

        internal bool IsAmbiguousTime(DateTime dateTime, TimeZoneInfoOptions flags)
        {
            DateTime time;
            AdjustmentRule rule;
            if (!this.m_supportsDaylightSavingTime)
            {
                return false;
            }
            if (dateTime.Kind == DateTimeKind.Local)
            {
                lock (s_internalSyncObject)
                {
                    time = ConvertTime(dateTime, Local, this, flags);
                    goto Label_0078;
                }
            }
            if (dateTime.Kind == DateTimeKind.Utc)
            {
                lock (s_internalSyncObject)
                {
                    time = ConvertTime(dateTime, Utc, this, flags);
                    goto Label_0078;
                }
            }
            time = dateTime;
        Label_0078:
            rule = this.GetAdjustmentRuleForTime(time);
            if (rule != null)
            {
                DaylightTime daylightTime = GetDaylightTime(time.Year, rule);
                return GetIsAmbiguousTime(time, rule, daylightTime);
            }
            return false;
        }

        public bool IsDaylightSavingTime(DateTime dateTime)
        {
            return this.IsDaylightSavingTime(dateTime, TimeZoneInfoOptions.NoThrowOnInvalidTime);
        }

        public bool IsDaylightSavingTime(DateTimeOffset dateTimeOffset)
        {
            bool flag;
            GetUtcOffsetFromUtc(dateTimeOffset.UtcDateTime, this, out flag);
            return flag;
        }

        internal bool IsDaylightSavingTime(DateTime dateTime, TimeZoneInfoOptions flags)
        {
            DateTime time;
            AdjustmentRule rule;
            if (!this.m_supportsDaylightSavingTime || (this.m_adjustmentRules == null))
            {
                return false;
            }
            if (dateTime.Kind == DateTimeKind.Local)
            {
                lock (s_internalSyncObject)
                {
                    time = ConvertTime(dateTime, Local, this, flags);
                    goto Label_006B;
                }
            }
            if (dateTime.Kind == DateTimeKind.Utc)
            {
                bool flag2;
                if (this.GetCorrespondingKind() == DateTimeKind.Utc)
                {
                    return false;
                }
                GetUtcOffsetFromUtc(dateTime, this, out flag2);
                return flag2;
            }
            time = dateTime;
        Label_006B:
            rule = this.GetAdjustmentRuleForTime(time);
            if (rule != null)
            {
                DaylightTime daylightTime = GetDaylightTime(time.Year, rule);
                return GetIsDaylightSavings(time, rule, daylightTime, flags);
            }
            return false;
        }

        public bool IsInvalidTime(DateTime dateTime)
        {
            bool flag = false;
            if ((dateTime.Kind != DateTimeKind.Unspecified) && ((dateTime.Kind != DateTimeKind.Local) || (this.GetCorrespondingKind() != DateTimeKind.Local)))
            {
                return flag;
            }
            AdjustmentRule adjustmentRuleForTime = this.GetAdjustmentRuleForTime(dateTime);
            if (adjustmentRuleForTime != null)
            {
                DaylightTime daylightTime = GetDaylightTime(dateTime.Year, adjustmentRuleForTime);
                return GetIsInvalidTime(dateTime, adjustmentRuleForTime, daylightTime);
            }
            return false;
        }

        void IDeserializationCallback.OnDeserialization(object sender)
        {
            try
            {
                bool flag;
                ValidateTimeZoneInfo(this.m_id, this.m_baseUtcOffset, this.m_adjustmentRules, out flag);
                if (flag != this.m_supportsDaylightSavingTime)
                {
                    throw new SerializationException(Environment.GetResourceString("Serialization_CorruptField", new object[] { "SupportsDaylightSavingTime" }));
                }
            }
            catch (ArgumentException exception)
            {
                throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"), exception);
            }
            catch (InvalidTimeZoneException exception2)
            {
                throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"), exception2);
            }
        }

        [SecurityCritical]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            info.AddValue("Id", this.m_id);
            info.AddValue("DisplayName", this.m_displayName);
            info.AddValue("StandardName", this.m_standardDisplayName);
            info.AddValue("DaylightName", this.m_daylightDisplayName);
            info.AddValue("BaseUtcOffset", this.m_baseUtcOffset);
            info.AddValue("AdjustmentRules", this.m_adjustmentRules);
            info.AddValue("SupportsDaylightSavingTime", this.m_supportsDaylightSavingTime);
        }

        public string ToSerializedString()
        {
            return StringSerializer.GetSerializedString(this);
        }

        public override string ToString()
        {
            return this.DisplayName;
        }

        [SecurityCritical]
        private static bool TransitionTimeFromTimeZoneInformation(Win32Native.RegistryTimeZoneInformation timeZoneInformation, out TransitionTime transitionTime, bool readStartDate)
        {
            if (timeZoneInformation.StandardDate.Month == 0)
            {
                transitionTime = new TransitionTime();
                return false;
            }
            if (readStartDate)
            {
                if (timeZoneInformation.DaylightDate.Year == 0)
                {
                    transitionTime = TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, timeZoneInformation.DaylightDate.Hour, timeZoneInformation.DaylightDate.Minute, timeZoneInformation.DaylightDate.Second, timeZoneInformation.DaylightDate.Milliseconds), timeZoneInformation.DaylightDate.Month, timeZoneInformation.DaylightDate.Day, (DayOfWeek) timeZoneInformation.DaylightDate.DayOfWeek);
                }
                else
                {
                    transitionTime = TransitionTime.CreateFixedDateRule(new DateTime(1, 1, 1, timeZoneInformation.DaylightDate.Hour, timeZoneInformation.DaylightDate.Minute, timeZoneInformation.DaylightDate.Second, timeZoneInformation.DaylightDate.Milliseconds), timeZoneInformation.DaylightDate.Month, timeZoneInformation.DaylightDate.Day);
                }
            }
            else if (timeZoneInformation.StandardDate.Year == 0)
            {
                transitionTime = TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, timeZoneInformation.StandardDate.Hour, timeZoneInformation.StandardDate.Minute, timeZoneInformation.StandardDate.Second, timeZoneInformation.StandardDate.Milliseconds), timeZoneInformation.StandardDate.Month, timeZoneInformation.StandardDate.Day, (DayOfWeek) timeZoneInformation.StandardDate.DayOfWeek);
            }
            else
            {
                transitionTime = TransitionTime.CreateFixedDateRule(new DateTime(1, 1, 1, timeZoneInformation.StandardDate.Hour, timeZoneInformation.StandardDate.Minute, timeZoneInformation.StandardDate.Second, timeZoneInformation.StandardDate.Milliseconds), timeZoneInformation.StandardDate.Month, timeZoneInformation.StandardDate.Day);
            }
            return true;
        }

        private static DateTime TransitionTimeToDateTime(int year, TransitionTime transitionTime)
        {
            DateTime time;
            DateTime timeOfDay = transitionTime.TimeOfDay;
            if (transitionTime.IsFixedDateRule)
            {
                int num = DateTime.DaysInMonth(year, transitionTime.Month);
                return new DateTime(year, transitionTime.Month, (num < transitionTime.Day) ? num : transitionTime.Day, timeOfDay.Hour, timeOfDay.Minute, timeOfDay.Second, timeOfDay.Millisecond);
            }
            if (transitionTime.Week <= 4)
            {
                time = new DateTime(year, transitionTime.Month, 1, timeOfDay.Hour, timeOfDay.Minute, timeOfDay.Second, timeOfDay.Millisecond);
                int dayOfWeek = (int) time.DayOfWeek;
                int num3 = ((int) transitionTime.DayOfWeek) - dayOfWeek;
                if (num3 < 0)
                {
                    num3 += 7;
                }
                num3 += 7 * (transitionTime.Week - 1);
                if (num3 > 0)
                {
                    time = time.AddDays((double) num3);
                }
                return time;
            }
            int day = DateTime.DaysInMonth(year, transitionTime.Month);
            time = new DateTime(year, transitionTime.Month, day, timeOfDay.Hour, timeOfDay.Minute, timeOfDay.Second, timeOfDay.Millisecond);
            int num6 = (int) (time.DayOfWeek - transitionTime.DayOfWeek);
            if (num6 < 0)
            {
                num6 += 7;
            }
            if (num6 > 0)
            {
                time = time.AddDays((double) -num6);
            }
            return time;
        }

        [SecurityCritical]
        private static bool TryCompareStandardDate(Win32Native.TimeZoneInformation timeZone, Win32Native.RegistryTimeZoneInformation registryTimeZoneInfo)
        {
            return ((((((timeZone.Bias == registryTimeZoneInfo.Bias) && (timeZone.StandardBias == registryTimeZoneInfo.StandardBias)) && ((timeZone.StandardDate.Year == registryTimeZoneInfo.StandardDate.Year) && (timeZone.StandardDate.Month == registryTimeZoneInfo.StandardDate.Month))) && (((timeZone.StandardDate.DayOfWeek == registryTimeZoneInfo.StandardDate.DayOfWeek) && (timeZone.StandardDate.Day == registryTimeZoneInfo.StandardDate.Day)) && ((timeZone.StandardDate.Hour == registryTimeZoneInfo.StandardDate.Hour) && (timeZone.StandardDate.Minute == registryTimeZoneInfo.StandardDate.Minute)))) && (timeZone.StandardDate.Second == registryTimeZoneInfo.StandardDate.Second)) && (timeZone.StandardDate.Milliseconds == registryTimeZoneInfo.StandardDate.Milliseconds));
        }

        [SecuritySafeCritical]
        private static bool TryCompareTimeZoneInformationToRegistry(Win32Native.TimeZoneInformation timeZone, string id, out bool dstDisabled)
        {
            bool flag2;
            dstDisabled = false;
            try
            {
                PermissionSet set = new PermissionSet(PermissionState.None);
                set.AddPermission(new RegistryPermission(RegistryPermissionAccess.Read, @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Time Zones"));
                set.Assert();
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(string.Format(CultureInfo.InvariantCulture, @"{0}\{1}", new object[] { @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Time Zones", id }), RegistryKeyPermissionCheck.Default, RegistryRights.ExecuteKey))
                {
                    if (key == null)
                    {
                        return false;
                    }
                    byte[] bytes = (byte[]) key.GetValue("TZI", null, RegistryValueOptions.None);
                    if ((bytes == null) || (bytes.Length != 0x2c))
                    {
                        return false;
                    }
                    Win32Native.RegistryTimeZoneInformation registryTimeZoneInfo = new Win32Native.RegistryTimeZoneInformation(bytes);
                    if (!TryCompareStandardDate(timeZone, registryTimeZoneInfo))
                    {
                        return false;
                    }
                    dstDisabled = CheckDaylightSavingTimeDisabledDownlevel();
                    bool flag = (dstDisabled || CheckDaylightSavingTimeNotSupported(timeZone)) || (((((timeZone.DaylightBias == registryTimeZoneInfo.DaylightBias) && (timeZone.DaylightDate.Year == registryTimeZoneInfo.DaylightDate.Year)) && ((timeZone.DaylightDate.Month == registryTimeZoneInfo.DaylightDate.Month) && (timeZone.DaylightDate.DayOfWeek == registryTimeZoneInfo.DaylightDate.DayOfWeek))) && (((timeZone.DaylightDate.Day == registryTimeZoneInfo.DaylightDate.Day) && (timeZone.DaylightDate.Hour == registryTimeZoneInfo.DaylightDate.Hour)) && ((timeZone.DaylightDate.Minute == registryTimeZoneInfo.DaylightDate.Minute) && (timeZone.DaylightDate.Second == registryTimeZoneInfo.DaylightDate.Second)))) && (timeZone.DaylightDate.Milliseconds == registryTimeZoneInfo.DaylightDate.Milliseconds));
                    if (flag)
                    {
                        string strA = key.GetValue("Std", string.Empty, RegistryValueOptions.None) as string;
                        flag = string.Compare(strA, timeZone.StandardName, StringComparison.Ordinal) == 0;
                    }
                    flag2 = flag;
                }
            }
            finally
            {
                PermissionSet.RevertAssert();
            }
            return flag2;
        }

        [SecurityCritical]
        private static bool TryCreateAdjustmentRules(string id, Win32Native.RegistryTimeZoneInformation defaultTimeZoneInformation, out AdjustmentRule[] rules, out Exception e)
        {
            e = null;
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(string.Format(CultureInfo.InvariantCulture, @"{0}\{1}\Dynamic DST", new object[] { @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Time Zones", id }), RegistryKeyPermissionCheck.Default, RegistryRights.ExecuteKey))
                {
                    if (key == null)
                    {
                        AdjustmentRule rule = CreateAdjustmentRuleFromTimeZoneInformation(defaultTimeZoneInformation, DateTime.MinValue.Date, DateTime.MaxValue.Date);
                        if (rule == null)
                        {
                            rules = null;
                        }
                        else
                        {
                            rules = new AdjustmentRule[] { rule };
                        }
                        return true;
                    }
                    int year = (int) key.GetValue("FirstEntry", -1, RegistryValueOptions.None);
                    int num2 = (int) key.GetValue("LastEntry", -1, RegistryValueOptions.None);
                    if (((year == -1) || (num2 == -1)) || (year > num2))
                    {
                        rules = null;
                        return false;
                    }
                    byte[] bytes = key.GetValue(year.ToString(CultureInfo.InvariantCulture), null, RegistryValueOptions.None) as byte[];
                    if ((bytes == null) || (bytes.Length != 0x2c))
                    {
                        rules = null;
                        return false;
                    }
                    Win32Native.RegistryTimeZoneInformation timeZoneInformation = new Win32Native.RegistryTimeZoneInformation(bytes);
                    if (year == num2)
                    {
                        AdjustmentRule rule2 = CreateAdjustmentRuleFromTimeZoneInformation(timeZoneInformation, DateTime.MinValue.Date, DateTime.MaxValue.Date);
                        if (rule2 == null)
                        {
                            rules = null;
                        }
                        else
                        {
                            rules = new AdjustmentRule[] { rule2 };
                        }
                        return true;
                    }
                    List<AdjustmentRule> list = new List<AdjustmentRule>(1);
                    AdjustmentRule item = CreateAdjustmentRuleFromTimeZoneInformation(timeZoneInformation, DateTime.MinValue.Date, new DateTime(year, 12, 0x1f));
                    if (item != null)
                    {
                        list.Add(item);
                    }
                    for (int i = year + 1; i < num2; i++)
                    {
                        bytes = key.GetValue(i.ToString(CultureInfo.InvariantCulture), null, RegistryValueOptions.None) as byte[];
                        if ((bytes == null) || (bytes.Length != 0x2c))
                        {
                            rules = null;
                            return false;
                        }
                        timeZoneInformation = new Win32Native.RegistryTimeZoneInformation(bytes);
                        AdjustmentRule rule4 = CreateAdjustmentRuleFromTimeZoneInformation(timeZoneInformation, new DateTime(i, 1, 1), new DateTime(i, 12, 0x1f));
                        if (rule4 != null)
                        {
                            list.Add(rule4);
                        }
                    }
                    bytes = key.GetValue(num2.ToString(CultureInfo.InvariantCulture), null, RegistryValueOptions.None) as byte[];
                    timeZoneInformation = new Win32Native.RegistryTimeZoneInformation(bytes);
                    if ((bytes == null) || (bytes.Length != 0x2c))
                    {
                        rules = null;
                        return false;
                    }
                    AdjustmentRule rule5 = CreateAdjustmentRuleFromTimeZoneInformation(timeZoneInformation, new DateTime(num2, 1, 1), DateTime.MaxValue.Date);
                    if (rule5 != null)
                    {
                        list.Add(rule5);
                    }
                    rules = list.ToArray();
                    if ((rules != null) && (rules.Length == 0))
                    {
                        rules = null;
                    }
                }
            }
            catch (InvalidCastException exception)
            {
                rules = null;
                e = exception;
                return false;
            }
            catch (ArgumentOutOfRangeException exception2)
            {
                rules = null;
                e = exception2;
                return false;
            }
            catch (ArgumentException exception3)
            {
                rules = null;
                e = exception3;
                return false;
            }
            return true;
        }

        [SecuritySafeCritical, FileIOPermission(SecurityAction.Assert, AllLocalFiles=FileIOPermissionAccess.PathDiscovery)]
        private static string TryGetLocalizedNameByMuiNativeResource(string resource)
        {
            string str;
            int num;
            if (string.IsNullOrEmpty(resource))
            {
                return string.Empty;
            }
            string[] strArray = resource.Split(new char[] { ',' }, StringSplitOptions.None);
            if (strArray.Length != 2)
            {
                return string.Empty;
            }
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.System);
            string str3 = strArray[0].TrimStart(new char[] { '@' });
            try
            {
                str = Path.Combine(folderPath, str3);
            }
            catch (ArgumentException)
            {
                return string.Empty;
            }
            if (!int.TryParse(strArray[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out num))
            {
                return string.Empty;
            }
            num = -num;
            try
            {
                StringBuilder fileMuiPath = new StringBuilder(260) {
                    Length = 260
                };
                int fileMuiPathLength = 260;
                int languageLength = 0;
                long enumerator = 0L;
                if (!UnsafeNativeMethods.GetFileMUIPath(0x10, str, null, ref languageLength, fileMuiPath, ref fileMuiPathLength, ref enumerator))
                {
                    return string.Empty;
                }
                return TryGetLocalizedNameByNativeResource(fileMuiPath.ToString(), num);
            }
            catch (EntryPointNotFoundException)
            {
                return string.Empty;
            }
        }

        [SecuritySafeCritical]
        private static string TryGetLocalizedNameByNativeResource(string filePath, int resource)
        {
            using (SafeLibraryHandle handle = UnsafeNativeMethods.LoadLibraryEx(filePath, IntPtr.Zero, 2))
            {
                if (!handle.IsInvalid)
                {
                    StringBuilder buffer = new StringBuilder(500) {
                        Length = 500
                    };
                    if (UnsafeNativeMethods.LoadString(handle, resource, buffer, buffer.Length) != 0)
                    {
                        return buffer.ToString();
                    }
                }
            }
            return string.Empty;
        }

        private static bool TryGetLocalizedNamesByRegistryKey(RegistryKey key, out string displayName, out string standardName, out string daylightName)
        {
            displayName = string.Empty;
            standardName = string.Empty;
            daylightName = string.Empty;
            string str = key.GetValue("MUI_Display", string.Empty, RegistryValueOptions.None) as string;
            string str2 = key.GetValue("MUI_Std", string.Empty, RegistryValueOptions.None) as string;
            string str3 = key.GetValue("MUI_Dlt", string.Empty, RegistryValueOptions.None) as string;
            if (!string.IsNullOrEmpty(str))
            {
                displayName = TryGetLocalizedNameByMuiNativeResource(str);
            }
            if (!string.IsNullOrEmpty(str2))
            {
                standardName = TryGetLocalizedNameByMuiNativeResource(str2);
            }
            if (!string.IsNullOrEmpty(str3))
            {
                daylightName = TryGetLocalizedNameByMuiNativeResource(str3);
            }
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = key.GetValue("Display", string.Empty, RegistryValueOptions.None) as string;
            }
            if (string.IsNullOrEmpty(standardName))
            {
                standardName = key.GetValue("Std", string.Empty, RegistryValueOptions.None) as string;
            }
            if (string.IsNullOrEmpty(daylightName))
            {
                daylightName = key.GetValue("Dlt", string.Empty, RegistryValueOptions.None) as string;
            }
            return true;
        }

        private static TimeZoneInfoResult TryGetTimeZone(string id, bool dstDisabled, out TimeZoneInfo value, out Exception e)
        {
            TimeZoneInfoResult success = TimeZoneInfoResult.Success;
            e = null;
            TimeZoneInfo info = null;
            if (s_systemTimeZones.TryGetValue(id, out info))
            {
                if (dstDisabled && info.m_supportsDaylightSavingTime)
                {
                    value = CreateCustomTimeZone(info.m_id, info.m_baseUtcOffset, info.m_displayName, info.m_standardDisplayName);
                    return success;
                }
                value = new TimeZoneInfo(info.m_id, info.m_baseUtcOffset, info.m_displayName, info.m_standardDisplayName, info.m_daylightDisplayName, info.m_adjustmentRules, false);
                return success;
            }
            if (!s_allSystemTimeZonesRead)
            {
                success = TryGetTimeZoneByRegistryKey(id, out info, out e);
                if (success == TimeZoneInfoResult.Success)
                {
                    s_systemTimeZones.Add(id, info);
                    if (dstDisabled && info.m_supportsDaylightSavingTime)
                    {
                        value = CreateCustomTimeZone(info.m_id, info.m_baseUtcOffset, info.m_displayName, info.m_standardDisplayName);
                        return success;
                    }
                    value = new TimeZoneInfo(info.m_id, info.m_baseUtcOffset, info.m_displayName, info.m_standardDisplayName, info.m_daylightDisplayName, info.m_adjustmentRules, false);
                    return success;
                }
                value = null;
                return success;
            }
            success = TimeZoneInfoResult.TimeZoneNotFoundException;
            value = null;
            return success;
        }

        [SecuritySafeCritical]
        private static TimeZoneInfoResult TryGetTimeZoneByRegistryKey(string id, out TimeZoneInfo value, out Exception e)
        {
            TimeZoneInfoResult invalidTimeZoneException;
            e = null;
            try
            {
                PermissionSet set = new PermissionSet(PermissionState.None);
                set.AddPermission(new RegistryPermission(RegistryPermissionAccess.Read, @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Time Zones"));
                set.Assert();
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(string.Format(CultureInfo.InvariantCulture, @"{0}\{1}", new object[] { @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Time Zones", id }), RegistryKeyPermissionCheck.Default, RegistryRights.ExecuteKey))
                {
                    AdjustmentRule[] ruleArray;
                    string str;
                    string str2;
                    string str3;
                    if (key == null)
                    {
                        value = null;
                        return TimeZoneInfoResult.TimeZoneNotFoundException;
                    }
                    byte[] bytes = key.GetValue("TZI", null, RegistryValueOptions.None) as byte[];
                    if ((bytes == null) || (bytes.Length != 0x2c))
                    {
                        value = null;
                        return TimeZoneInfoResult.InvalidTimeZoneException;
                    }
                    Win32Native.RegistryTimeZoneInformation defaultTimeZoneInformation = new Win32Native.RegistryTimeZoneInformation(bytes);
                    if (!TryCreateAdjustmentRules(id, defaultTimeZoneInformation, out ruleArray, out e))
                    {
                        value = null;
                        return TimeZoneInfoResult.InvalidTimeZoneException;
                    }
                    if (!TryGetLocalizedNamesByRegistryKey(key, out str, out str2, out str3))
                    {
                        value = null;
                        invalidTimeZoneException = TimeZoneInfoResult.InvalidTimeZoneException;
                    }
                    else
                    {
                        try
                        {
                            value = new TimeZoneInfo(id, new TimeSpan(0, -defaultTimeZoneInformation.Bias, 0), str, str2, str3, ruleArray, false);
                            invalidTimeZoneException = TimeZoneInfoResult.Success;
                        }
                        catch (ArgumentException exception)
                        {
                            value = null;
                            e = exception;
                            invalidTimeZoneException = TimeZoneInfoResult.InvalidTimeZoneException;
                        }
                        catch (InvalidTimeZoneException exception2)
                        {
                            value = null;
                            e = exception2;
                            invalidTimeZoneException = TimeZoneInfoResult.InvalidTimeZoneException;
                        }
                    }
                }
            }
            finally
            {
                PermissionSet.RevertAssert();
            }
            return invalidTimeZoneException;
        }

        internal static bool UtcOffsetOutOfRange(TimeSpan offset)
        {
            if (offset.TotalHours >= -14.0)
            {
                return (offset.TotalHours > 14.0);
            }
            return true;
        }

        private static void ValidateTimeZoneInfo(string id, TimeSpan baseUtcOffset, AdjustmentRule[] adjustmentRules, out bool adjustmentRulesSupportDst)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }
            if (id.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidId", new object[] { id }), "id");
            }
            if (UtcOffsetOutOfRange(baseUtcOffset))
            {
                throw new ArgumentOutOfRangeException("baseUtcOffset", Environment.GetResourceString("ArgumentOutOfRange_UtcOffset"));
            }
            if ((baseUtcOffset.Ticks % 0x23c34600L) != 0L)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_TimeSpanHasSeconds"), "baseUtcOffset");
            }
            adjustmentRulesSupportDst = false;
            if ((adjustmentRules != null) && (adjustmentRules.Length != 0))
            {
                adjustmentRulesSupportDst = true;
                AdjustmentRule rule = null;
                AdjustmentRule rule2 = null;
                for (int i = 0; i < adjustmentRules.Length; i++)
                {
                    rule = rule2;
                    rule2 = adjustmentRules[i];
                    if (rule2 == null)
                    {
                        throw new InvalidTimeZoneException(Environment.GetResourceString("Argument_AdjustmentRulesNoNulls"));
                    }
                    if (UtcOffsetOutOfRange(baseUtcOffset + rule2.DaylightDelta))
                    {
                        throw new InvalidTimeZoneException(Environment.GetResourceString("ArgumentOutOfRange_UtcOffsetAndDaylightDelta"));
                    }
                    if ((rule != null) && (rule2.DateStart <= rule.DateEnd))
                    {
                        throw new InvalidTimeZoneException(Environment.GetResourceString("Argument_AdjustmentRulesOutOfOrder"));
                    }
                }
            }
        }

        public TimeSpan BaseUtcOffset
        {
            get
            {
                return this.m_baseUtcOffset;
            }
        }

        public string DaylightName
        {
            get
            {
                if (this.m_daylightDisplayName != null)
                {
                    return this.m_daylightDisplayName;
                }
                return string.Empty;
            }
        }

        public string DisplayName
        {
            get
            {
                if (this.m_displayName != null)
                {
                    return this.m_displayName;
                }
                return string.Empty;
            }
        }

        public string Id
        {
            get
            {
                return this.m_id;
            }
        }

        public static TimeZoneInfo Local
        {
            get
            {
                TimeZoneInfo info = s_localTimeZone;
                if (info != null)
                {
                    return info;
                }
                lock (s_internalSyncObject)
                {
                    if (s_localTimeZone == null)
                    {
                        TimeZoneInfo localTimeZone = GetLocalTimeZone();
                        s_localTimeZone = new TimeZoneInfo(localTimeZone.m_id, localTimeZone.m_baseUtcOffset, localTimeZone.m_displayName, localTimeZone.m_standardDisplayName, localTimeZone.m_daylightDisplayName, localTimeZone.m_adjustmentRules, false);
                    }
                    return s_localTimeZone;
                }
            }
        }

        private static object s_internalSyncObject
        {
            get
            {
                if (s_hiddenInternalSyncObject == null)
                {
                    object obj2 = new object();
                    Interlocked.CompareExchange<object>(ref s_hiddenInternalSyncObject, obj2, null);
                }
                return s_hiddenInternalSyncObject;
            }
        }

        private static Dictionary<string, TimeZoneInfo> s_systemTimeZones
        {
            get
            {
                if (s_hiddenSystemTimeZones == null)
                {
                    s_hiddenSystemTimeZones = new Dictionary<string, TimeZoneInfo>();
                }
                return s_hiddenSystemTimeZones;
            }
            set
            {
                s_hiddenSystemTimeZones = value;
            }
        }

        public string StandardName
        {
            get
            {
                if (this.m_standardDisplayName != null)
                {
                    return this.m_standardDisplayName;
                }
                return string.Empty;
            }
        }

        public bool SupportsDaylightSavingTime
        {
            get
            {
                return this.m_supportsDaylightSavingTime;
            }
        }

        public static TimeZoneInfo Utc
        {
            get
            {
                TimeZoneInfo info = s_utcTimeZone;
                if (info != null)
                {
                    return info;
                }
                lock (s_internalSyncObject)
                {
                    if (s_utcTimeZone == null)
                    {
                        s_utcTimeZone = CreateCustomTimeZone("UTC", TimeSpan.Zero, "UTC", "UTC");
                    }
                    return s_utcTimeZone;
                }
            }
        }

        [Serializable, TypeForwardedFrom("System.Core, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=b77a5c561934e089"), HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
        public sealed class AdjustmentRule : IEquatable<TimeZoneInfo.AdjustmentRule>, ISerializable, IDeserializationCallback
        {
            private DateTime m_dateEnd;
            private DateTime m_dateStart;
            private TimeSpan m_daylightDelta;
            private TimeZoneInfo.TransitionTime m_daylightTransitionEnd;
            private TimeZoneInfo.TransitionTime m_daylightTransitionStart;

            private AdjustmentRule()
            {
            }

            private AdjustmentRule(SerializationInfo info, StreamingContext context)
            {
                if (info == null)
                {
                    throw new ArgumentNullException("info");
                }
                this.m_dateStart = (DateTime) info.GetValue("DateStart", typeof(DateTime));
                this.m_dateEnd = (DateTime) info.GetValue("DateEnd", typeof(DateTime));
                this.m_daylightDelta = (TimeSpan) info.GetValue("DaylightDelta", typeof(TimeSpan));
                this.m_daylightTransitionStart = (TimeZoneInfo.TransitionTime) info.GetValue("DaylightTransitionStart", typeof(TimeZoneInfo.TransitionTime));
                this.m_daylightTransitionEnd = (TimeZoneInfo.TransitionTime) info.GetValue("DaylightTransitionEnd", typeof(TimeZoneInfo.TransitionTime));
            }

            public static TimeZoneInfo.AdjustmentRule CreateAdjustmentRule(DateTime dateStart, DateTime dateEnd, TimeSpan daylightDelta, TimeZoneInfo.TransitionTime daylightTransitionStart, TimeZoneInfo.TransitionTime daylightTransitionEnd)
            {
                ValidateAdjustmentRule(dateStart, dateEnd, daylightDelta, daylightTransitionStart, daylightTransitionEnd);
                return new TimeZoneInfo.AdjustmentRule { m_dateStart = dateStart, m_dateEnd = dateEnd, m_daylightDelta = daylightDelta, m_daylightTransitionStart = daylightTransitionStart, m_daylightTransitionEnd = daylightTransitionEnd };
            }

            public bool Equals(TimeZoneInfo.AdjustmentRule other)
            {
                return ((((((other != null) && (this.m_dateStart == other.m_dateStart)) && (this.m_dateEnd == other.m_dateEnd)) && (this.m_daylightDelta == other.m_daylightDelta)) && this.m_daylightTransitionEnd.Equals(other.m_daylightTransitionEnd)) && this.m_daylightTransitionStart.Equals(other.m_daylightTransitionStart));
            }

            public override int GetHashCode()
            {
                return this.m_dateStart.GetHashCode();
            }

            void IDeserializationCallback.OnDeserialization(object sender)
            {
                try
                {
                    ValidateAdjustmentRule(this.m_dateStart, this.m_dateEnd, this.m_daylightDelta, this.m_daylightTransitionStart, this.m_daylightTransitionEnd);
                }
                catch (ArgumentException exception)
                {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"), exception);
                }
            }

            [SecurityCritical]
            void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
            {
                if (info == null)
                {
                    throw new ArgumentNullException("info");
                }
                info.AddValue("DateStart", this.m_dateStart);
                info.AddValue("DateEnd", this.m_dateEnd);
                info.AddValue("DaylightDelta", this.m_daylightDelta);
                info.AddValue("DaylightTransitionStart", this.m_daylightTransitionStart);
                info.AddValue("DaylightTransitionEnd", this.m_daylightTransitionEnd);
            }

            private static void ValidateAdjustmentRule(DateTime dateStart, DateTime dateEnd, TimeSpan daylightDelta, TimeZoneInfo.TransitionTime daylightTransitionStart, TimeZoneInfo.TransitionTime daylightTransitionEnd)
            {
                if (dateStart.Kind != DateTimeKind.Unspecified)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_DateTimeKindMustBeUnspecified"), "dateStart");
                }
                if (dateEnd.Kind != DateTimeKind.Unspecified)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_DateTimeKindMustBeUnspecified"), "dateEnd");
                }
                if (daylightTransitionStart.Equals(daylightTransitionEnd))
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_TransitionTimesAreIdentical"), "daylightTransitionEnd");
                }
                if (dateStart > dateEnd)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_OutOfOrderDateTimes"), "dateStart");
                }
                if (TimeZoneInfo.UtcOffsetOutOfRange(daylightDelta))
                {
                    throw new ArgumentOutOfRangeException("daylightDelta", daylightDelta, Environment.GetResourceString("ArgumentOutOfRange_UtcOffset"));
                }
                if ((daylightDelta.Ticks % 0x23c34600L) != 0L)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_TimeSpanHasSeconds"), "daylightDelta");
                }
                if (dateStart.TimeOfDay != TimeSpan.Zero)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_DateTimeHasTimeOfDay"), "dateStart");
                }
                if (dateEnd.TimeOfDay != TimeSpan.Zero)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_DateTimeHasTimeOfDay"), "dateEnd");
                }
            }

            public DateTime DateEnd
            {
                get
                {
                    return this.m_dateEnd;
                }
            }

            public DateTime DateStart
            {
                get
                {
                    return this.m_dateStart;
                }
            }

            public TimeSpan DaylightDelta
            {
                get
                {
                    return this.m_daylightDelta;
                }
            }

            public TimeZoneInfo.TransitionTime DaylightTransitionEnd
            {
                get
                {
                    return this.m_daylightTransitionEnd;
                }
            }

            public TimeZoneInfo.TransitionTime DaylightTransitionStart
            {
                get
                {
                    return this.m_daylightTransitionStart;
                }
            }
        }

        private class OffsetAndRule
        {
            public TimeSpan offset;
            public TimeZoneInfo.AdjustmentRule rule;
            public int year;

            public OffsetAndRule(int year, TimeSpan offset, TimeZoneInfo.AdjustmentRule rule)
            {
                this.year = year;
                this.offset = offset;
                this.rule = rule;
            }
        }

        private sealed class StringSerializer
        {
            private const string dateTimeFormat = "MM:dd:yyyy";
            private const char esc = '\\';
            private const string escapedEsc = @"\\";
            private const string escapedLhs = @"\[";
            private const string escapedRhs = @"\]";
            private const string escapedSep = @"\;";
            private const string escString = @"\";
            private const int initialCapacityForString = 0x40;
            private const char lhs = '[';
            private const string lhsString = "[";
            private int m_currentTokenStartIndex;
            private string m_serializedText;
            private State m_state;
            private const char rhs = ']';
            private const string rhsString = "]";
            private const char sep = ';';
            private const string sepString = ";";
            private const string timeOfDayFormat = "HH:mm:ss.FFF";

            private StringSerializer(string str)
            {
                this.m_serializedText = str;
                this.m_state = State.StartOfToken;
            }

            public static TimeZoneInfo GetDeserializedTimeZoneInfo(string source)
            {
                TimeZoneInfo info;
                TimeZoneInfo.StringSerializer serializer = new TimeZoneInfo.StringSerializer(source);
                string nextStringValue = serializer.GetNextStringValue(false);
                TimeSpan nextTimeSpanValue = serializer.GetNextTimeSpanValue(false);
                string displayName = serializer.GetNextStringValue(false);
                string standardDisplayName = serializer.GetNextStringValue(false);
                string daylightDisplayName = serializer.GetNextStringValue(false);
                TimeZoneInfo.AdjustmentRule[] nextAdjustmentRuleArrayValue = serializer.GetNextAdjustmentRuleArrayValue(false);
                try
                {
                    info = TimeZoneInfo.CreateCustomTimeZone(nextStringValue, nextTimeSpanValue, displayName, standardDisplayName, daylightDisplayName, nextAdjustmentRuleArrayValue);
                }
                catch (ArgumentException exception)
                {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"), exception);
                }
                catch (InvalidTimeZoneException exception2)
                {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"), exception2);
                }
                return info;
            }

            private TimeZoneInfo.AdjustmentRule[] GetNextAdjustmentRuleArrayValue(bool canEndWithoutSeparator)
            {
                List<TimeZoneInfo.AdjustmentRule> list = new List<TimeZoneInfo.AdjustmentRule>(1);
                int num = 0;
                for (TimeZoneInfo.AdjustmentRule rule = this.GetNextAdjustmentRuleValue(true); rule != null; rule = this.GetNextAdjustmentRuleValue(true))
                {
                    list.Add(rule);
                    num++;
                }
                if (!canEndWithoutSeparator)
                {
                    if (this.m_state == State.EndOfLine)
                    {
                        throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));
                    }
                    if ((this.m_currentTokenStartIndex < 0) || (this.m_currentTokenStartIndex >= this.m_serializedText.Length))
                    {
                        throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));
                    }
                }
                if (num == 0)
                {
                    return null;
                }
                return list.ToArray();
            }

            private TimeZoneInfo.AdjustmentRule GetNextAdjustmentRuleValue(bool canEndWithoutSeparator)
            {
                TimeZoneInfo.AdjustmentRule rule;
                if (this.m_state == State.EndOfLine)
                {
                    if (!canEndWithoutSeparator)
                    {
                        throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));
                    }
                    return null;
                }
                if ((this.m_currentTokenStartIndex < 0) || (this.m_currentTokenStartIndex >= this.m_serializedText.Length))
                {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));
                }
                if (this.m_serializedText[this.m_currentTokenStartIndex] == ';')
                {
                    return null;
                }
                if (this.m_serializedText[this.m_currentTokenStartIndex] != '[')
                {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));
                }
                this.m_currentTokenStartIndex++;
                DateTime nextDateTimeValue = this.GetNextDateTimeValue(false, "MM:dd:yyyy");
                DateTime dateEnd = this.GetNextDateTimeValue(false, "MM:dd:yyyy");
                TimeSpan nextTimeSpanValue = this.GetNextTimeSpanValue(false);
                TimeZoneInfo.TransitionTime nextTransitionTimeValue = this.GetNextTransitionTimeValue(false);
                TimeZoneInfo.TransitionTime daylightTransitionEnd = this.GetNextTransitionTimeValue(false);
                if ((this.m_state == State.EndOfLine) || (this.m_currentTokenStartIndex >= this.m_serializedText.Length))
                {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));
                }
                if (this.m_serializedText[this.m_currentTokenStartIndex] != ']')
                {
                    this.SkipVersionNextDataFields(1);
                }
                else
                {
                    this.m_currentTokenStartIndex++;
                }
                try
                {
                    rule = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(nextDateTimeValue, dateEnd, nextTimeSpanValue, nextTransitionTimeValue, daylightTransitionEnd);
                }
                catch (ArgumentException exception)
                {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"), exception);
                }
                if (this.m_currentTokenStartIndex >= this.m_serializedText.Length)
                {
                    this.m_state = State.EndOfLine;
                    return rule;
                }
                this.m_state = State.StartOfToken;
                return rule;
            }

            private DateTime GetNextDateTimeValue(bool canEndWithoutSeparator, string format)
            {
                DateTime time;
                if (!DateTime.TryParseExact(this.GetNextStringValue(canEndWithoutSeparator), format, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out time))
                {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));
                }
                return time;
            }

            private int GetNextInt32Value(bool canEndWithoutSeparator)
            {
                int num;
                if (!int.TryParse(this.GetNextStringValue(canEndWithoutSeparator), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out num))
                {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));
                }
                return num;
            }

            private string GetNextStringValue(bool canEndWithoutSeparator)
            {
                if (this.m_state == State.EndOfLine)
                {
                    if (!canEndWithoutSeparator)
                    {
                        throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));
                    }
                    return null;
                }
                if ((this.m_currentTokenStartIndex < 0) || (this.m_currentTokenStartIndex >= this.m_serializedText.Length))
                {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));
                }
                State notEscaped = State.NotEscaped;
                StringBuilder builder = new StringBuilder(0x40);
                for (int i = this.m_currentTokenStartIndex; i < this.m_serializedText.Length; i++)
                {
                    if (notEscaped == State.Escaped)
                    {
                        VerifyIsEscapableCharacter(this.m_serializedText[i]);
                        builder.Append(this.m_serializedText[i]);
                        notEscaped = State.NotEscaped;
                    }
                    else
                    {
                        if (notEscaped == State.NotEscaped)
                        {
                            switch (this.m_serializedText[i])
                            {
                                case '[':
                                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));

                                case '\\':
                                    notEscaped = State.Escaped;
                                    goto Label_015D;

                                case ']':
                                    if (!canEndWithoutSeparator)
                                    {
                                        throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));
                                    }
                                    this.m_currentTokenStartIndex = i;
                                    this.m_state = State.StartOfToken;
                                    return builder.ToString();

                                case ';':
                                    this.m_currentTokenStartIndex = i + 1;
                                    if (this.m_currentTokenStartIndex >= this.m_serializedText.Length)
                                    {
                                        this.m_state = State.EndOfLine;
                                    }
                                    else
                                    {
                                        this.m_state = State.StartOfToken;
                                    }
                                    return builder.ToString();

                                case '\0':
                                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));
                            }
                            builder.Append(this.m_serializedText[i]);
                        }
                    Label_015D:;
                    }
                }
                if (notEscaped == State.Escaped)
                {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidEscapeSequence", new object[] { string.Empty }));
                }
                if (!canEndWithoutSeparator)
                {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));
                }
                this.m_currentTokenStartIndex = this.m_serializedText.Length;
                this.m_state = State.EndOfLine;
                return builder.ToString();
            }

            private TimeSpan GetNextTimeSpanValue(bool canEndWithoutSeparator)
            {
                TimeSpan span;
                int minutes = this.GetNextInt32Value(canEndWithoutSeparator);
                try
                {
                    span = new TimeSpan(0, minutes, 0);
                }
                catch (ArgumentOutOfRangeException exception)
                {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"), exception);
                }
                return span;
            }

            private TimeZoneInfo.TransitionTime GetNextTransitionTimeValue(bool canEndWithoutSeparator)
            {
                TimeZoneInfo.TransitionTime time;
                if ((this.m_state == State.EndOfLine) || ((this.m_currentTokenStartIndex < this.m_serializedText.Length) && (this.m_serializedText[this.m_currentTokenStartIndex] == ']')))
                {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));
                }
                if ((this.m_currentTokenStartIndex < 0) || (this.m_currentTokenStartIndex >= this.m_serializedText.Length))
                {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));
                }
                if (this.m_serializedText[this.m_currentTokenStartIndex] != '[')
                {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));
                }
                this.m_currentTokenStartIndex++;
                int num = this.GetNextInt32Value(false);
                if ((num != 0) && (num != 1))
                {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));
                }
                DateTime nextDateTimeValue = this.GetNextDateTimeValue(false, "HH:mm:ss.FFF");
                nextDateTimeValue = new DateTime(1, 1, 1, nextDateTimeValue.Hour, nextDateTimeValue.Minute, nextDateTimeValue.Second, nextDateTimeValue.Millisecond);
                int month = this.GetNextInt32Value(false);
                if (num == 1)
                {
                    int day = this.GetNextInt32Value(false);
                    try
                    {
                        time = TimeZoneInfo.TransitionTime.CreateFixedDateRule(nextDateTimeValue, month, day);
                        goto Label_015B;
                    }
                    catch (ArgumentException exception)
                    {
                        throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"), exception);
                    }
                }
                int week = this.GetNextInt32Value(false);
                int num5 = this.GetNextInt32Value(false);
                try
                {
                    time = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(nextDateTimeValue, month, week, (DayOfWeek) num5);
                }
                catch (ArgumentException exception2)
                {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"), exception2);
                }
            Label_015B:
                if ((this.m_state == State.EndOfLine) || (this.m_currentTokenStartIndex >= this.m_serializedText.Length))
                {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));
                }
                if (this.m_serializedText[this.m_currentTokenStartIndex] != ']')
                {
                    this.SkipVersionNextDataFields(1);
                }
                else
                {
                    this.m_currentTokenStartIndex++;
                }
                bool flag = false;
                if ((this.m_currentTokenStartIndex < this.m_serializedText.Length) && (this.m_serializedText[this.m_currentTokenStartIndex] == ';'))
                {
                    this.m_currentTokenStartIndex++;
                    flag = true;
                }
                if (!flag && !canEndWithoutSeparator)
                {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));
                }
                if (this.m_currentTokenStartIndex >= this.m_serializedText.Length)
                {
                    this.m_state = State.EndOfLine;
                    return time;
                }
                this.m_state = State.StartOfToken;
                return time;
            }

            public static string GetSerializedString(TimeZoneInfo zone)
            {
                StringBuilder serializedText = new StringBuilder();
                serializedText.Append(SerializeSubstitute(zone.Id));
                serializedText.Append(';');
                serializedText.Append(SerializeSubstitute(zone.BaseUtcOffset.TotalMinutes.ToString(CultureInfo.InvariantCulture)));
                serializedText.Append(';');
                serializedText.Append(SerializeSubstitute(zone.DisplayName));
                serializedText.Append(';');
                serializedText.Append(SerializeSubstitute(zone.StandardName));
                serializedText.Append(';');
                serializedText.Append(SerializeSubstitute(zone.DaylightName));
                serializedText.Append(';');
                TimeZoneInfo.AdjustmentRule[] adjustmentRules = zone.GetAdjustmentRules();
                if ((adjustmentRules != null) && (adjustmentRules.Length > 0))
                {
                    for (int i = 0; i < adjustmentRules.Length; i++)
                    {
                        TimeZoneInfo.AdjustmentRule rule = adjustmentRules[i];
                        serializedText.Append('[');
                        serializedText.Append(SerializeSubstitute(rule.DateStart.ToString("MM:dd:yyyy", DateTimeFormatInfo.InvariantInfo)));
                        serializedText.Append(';');
                        serializedText.Append(SerializeSubstitute(rule.DateEnd.ToString("MM:dd:yyyy", DateTimeFormatInfo.InvariantInfo)));
                        serializedText.Append(';');
                        serializedText.Append(SerializeSubstitute(rule.DaylightDelta.TotalMinutes.ToString(CultureInfo.InvariantCulture)));
                        serializedText.Append(';');
                        SerializeTransitionTime(rule.DaylightTransitionStart, serializedText);
                        serializedText.Append(';');
                        SerializeTransitionTime(rule.DaylightTransitionEnd, serializedText);
                        serializedText.Append(';');
                        serializedText.Append(']');
                    }
                }
                serializedText.Append(';');
                return serializedText.ToString();
            }

            private static string SerializeSubstitute(string text)
            {
                text = text.Replace(@"\", @"\\");
                text = text.Replace("[", @"\[");
                text = text.Replace("]", @"\]");
                return text.Replace(";", @"\;");
            }

            private static void SerializeTransitionTime(TimeZoneInfo.TransitionTime time, StringBuilder serializedText)
            {
                serializedText.Append('[');
                serializedText.Append((time.IsFixedDateRule ? 1 : 0).ToString(CultureInfo.InvariantCulture));
                serializedText.Append(';');
                if (time.IsFixedDateRule)
                {
                    serializedText.Append(SerializeSubstitute(time.TimeOfDay.ToString("HH:mm:ss.FFF", DateTimeFormatInfo.InvariantInfo)));
                    serializedText.Append(';');
                    serializedText.Append(SerializeSubstitute(time.Month.ToString(CultureInfo.InvariantCulture)));
                    serializedText.Append(';');
                    serializedText.Append(SerializeSubstitute(time.Day.ToString(CultureInfo.InvariantCulture)));
                    serializedText.Append(';');
                }
                else
                {
                    serializedText.Append(SerializeSubstitute(time.TimeOfDay.ToString("HH:mm:ss.FFF", DateTimeFormatInfo.InvariantInfo)));
                    serializedText.Append(';');
                    serializedText.Append(SerializeSubstitute(time.Month.ToString(CultureInfo.InvariantCulture)));
                    serializedText.Append(';');
                    serializedText.Append(SerializeSubstitute(time.Week.ToString(CultureInfo.InvariantCulture)));
                    serializedText.Append(';');
                    serializedText.Append(SerializeSubstitute(((int) time.DayOfWeek).ToString(CultureInfo.InvariantCulture)));
                    serializedText.Append(';');
                }
                serializedText.Append(']');
            }

            private void SkipVersionNextDataFields(int depth)
            {
                if ((this.m_currentTokenStartIndex < 0) || (this.m_currentTokenStartIndex >= this.m_serializedText.Length))
                {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));
                }
                State notEscaped = State.NotEscaped;
                for (int i = this.m_currentTokenStartIndex; i < this.m_serializedText.Length; i++)
                {
                    switch (notEscaped)
                    {
                        case State.Escaped:
                            VerifyIsEscapableCharacter(this.m_serializedText[i]);
                            notEscaped = State.NotEscaped;
                            break;

                        case State.NotEscaped:
                            switch (this.m_serializedText[i])
                            {
                                case '[':
                                {
                                    depth++;
                                    continue;
                                }
                                case '\\':
                                {
                                    notEscaped = State.Escaped;
                                    continue;
                                }
                                case ']':
                                    depth--;
                                    if (depth != 0)
                                    {
                                        continue;
                                    }
                                    this.m_currentTokenStartIndex = i + 1;
                                    if (this.m_currentTokenStartIndex < this.m_serializedText.Length)
                                    {
                                        goto Label_00B5;
                                    }
                                    this.m_state = State.EndOfLine;
                                    return;

                                case '\0':
                                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));
                            }
                            break;
                    }
                    continue;
                Label_00B5:
                    this.m_state = State.StartOfToken;
                    return;
                }
                throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"));
            }

            private static void VerifyIsEscapableCharacter(char c)
            {
                if (((c != '\\') && (c != ';')) && ((c != '[') && (c != ']')))
                {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidEscapeSequence", new object[] { c }));
                }
            }

            private enum State
            {
                Escaped,
                NotEscaped,
                StartOfToken,
                EndOfLine
            }
        }

        private class TimeZoneInfoComparer : IComparer<TimeZoneInfo>
        {
            int IComparer<TimeZoneInfo>.Compare(TimeZoneInfo x, TimeZoneInfo y)
            {
                int num = x.BaseUtcOffset.CompareTo(y.BaseUtcOffset);
                if (num != 0)
                {
                    return num;
                }
                return string.Compare(x.DisplayName, y.DisplayName, StringComparison.Ordinal);
            }
        }

        private enum TimeZoneInfoResult
        {
            Success,
            TimeZoneNotFoundException,
            InvalidTimeZoneException,
            SecurityException
        }

        [Serializable, StructLayout(LayoutKind.Sequential), TypeForwardedFrom("System.Core, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=b77a5c561934e089"), HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
        public struct TransitionTime : IEquatable<TimeZoneInfo.TransitionTime>, ISerializable, IDeserializationCallback
        {
            private DateTime m_timeOfDay;
            private byte m_month;
            private byte m_week;
            private byte m_day;
            private System.DayOfWeek m_dayOfWeek;
            private bool m_isFixedDateRule;
            public DateTime TimeOfDay
            {
                get
                {
                    return this.m_timeOfDay;
                }
            }
            public int Month
            {
                get
                {
                    return this.m_month;
                }
            }
            public int Week
            {
                get
                {
                    return this.m_week;
                }
            }
            public int Day
            {
                get
                {
                    return this.m_day;
                }
            }
            public System.DayOfWeek DayOfWeek
            {
                get
                {
                    return this.m_dayOfWeek;
                }
            }
            public bool IsFixedDateRule
            {
                get
                {
                    return this.m_isFixedDateRule;
                }
            }
            public override bool Equals(object obj)
            {
                return ((obj is TimeZoneInfo.TransitionTime) && this.Equals((TimeZoneInfo.TransitionTime) obj));
            }

            public static bool operator ==(TimeZoneInfo.TransitionTime t1, TimeZoneInfo.TransitionTime t2)
            {
                return t1.Equals(t2);
            }

            public static bool operator !=(TimeZoneInfo.TransitionTime t1, TimeZoneInfo.TransitionTime t2)
            {
                return !t1.Equals(t2);
            }

            public bool Equals(TimeZoneInfo.TransitionTime other)
            {
                bool flag = ((this.m_isFixedDateRule == other.m_isFixedDateRule) && (this.m_timeOfDay == other.m_timeOfDay)) && (this.m_month == other.m_month);
                if (!flag)
                {
                    return flag;
                }
                if (other.m_isFixedDateRule)
                {
                    return (this.m_day == other.m_day);
                }
                return ((this.m_week == other.m_week) && (this.m_dayOfWeek == other.m_dayOfWeek));
            }

            public override int GetHashCode()
            {
                return (this.m_month ^ (this.m_week << 8));
            }

            public static TimeZoneInfo.TransitionTime CreateFixedDateRule(DateTime timeOfDay, int month, int day)
            {
                return CreateTransitionTime(timeOfDay, month, 1, day, System.DayOfWeek.Sunday, true);
            }

            public static TimeZoneInfo.TransitionTime CreateFloatingDateRule(DateTime timeOfDay, int month, int week, System.DayOfWeek dayOfWeek)
            {
                return CreateTransitionTime(timeOfDay, month, week, 1, dayOfWeek, false);
            }

            private static TimeZoneInfo.TransitionTime CreateTransitionTime(DateTime timeOfDay, int month, int week, int day, System.DayOfWeek dayOfWeek, bool isFixedDateRule)
            {
                ValidateTransitionTime(timeOfDay, month, week, day, dayOfWeek);
                return new TimeZoneInfo.TransitionTime { m_isFixedDateRule = isFixedDateRule, m_timeOfDay = timeOfDay, m_dayOfWeek = dayOfWeek, m_day = (byte) day, m_week = (byte) week, m_month = (byte) month };
            }

            private static void ValidateTransitionTime(DateTime timeOfDay, int month, int week, int day, System.DayOfWeek dayOfWeek)
            {
                if (timeOfDay.Kind != DateTimeKind.Unspecified)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_DateTimeKindMustBeUnspecified"), "timeOfDay");
                }
                if ((month < 1) || (month > 12))
                {
                    throw new ArgumentOutOfRangeException("month", Environment.GetResourceString("ArgumentOutOfRange_MonthParam"));
                }
                if ((day < 1) || (day > 0x1f))
                {
                    throw new ArgumentOutOfRangeException("day", Environment.GetResourceString("ArgumentOutOfRange_DayParam"));
                }
                if ((week < 1) || (week > 5))
                {
                    throw new ArgumentOutOfRangeException("week", Environment.GetResourceString("ArgumentOutOfRange_Week"));
                }
                if ((dayOfWeek < System.DayOfWeek.Sunday) || (dayOfWeek > System.DayOfWeek.Saturday))
                {
                    throw new ArgumentOutOfRangeException("dayOfWeek", Environment.GetResourceString("ArgumentOutOfRange_DayOfWeek"));
                }
                if (((timeOfDay.Year != 1) || (timeOfDay.Month != 1)) || ((timeOfDay.Day != 1) || ((timeOfDay.Ticks % 0x2710L) != 0L)))
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_DateTimeHasTicks"), "timeOfDay");
                }
            }

            void IDeserializationCallback.OnDeserialization(object sender)
            {
                try
                {
                    ValidateTransitionTime(this.m_timeOfDay, this.m_month, this.m_week, this.m_day, this.m_dayOfWeek);
                }
                catch (ArgumentException exception)
                {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidData"), exception);
                }
            }

            [SecurityCritical]
            void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
            {
                if (info == null)
                {
                    throw new ArgumentNullException("info");
                }
                info.AddValue("TimeOfDay", this.m_timeOfDay);
                info.AddValue("Month", this.m_month);
                info.AddValue("Week", this.m_week);
                info.AddValue("Day", this.m_day);
                info.AddValue("DayOfWeek", this.m_dayOfWeek);
                info.AddValue("IsFixedDateRule", this.m_isFixedDateRule);
            }

            private TransitionTime(SerializationInfo info, StreamingContext context)
            {
                if (info == null)
                {
                    throw new ArgumentNullException("info");
                }
                this.m_timeOfDay = (DateTime) info.GetValue("TimeOfDay", typeof(DateTime));
                this.m_month = (byte) info.GetValue("Month", typeof(byte));
                this.m_week = (byte) info.GetValue("Week", typeof(byte));
                this.m_day = (byte) info.GetValue("Day", typeof(byte));
                this.m_dayOfWeek = (System.DayOfWeek) info.GetValue("DayOfWeek", typeof(System.DayOfWeek));
                this.m_isFixedDateRule = (bool) info.GetValue("IsFixedDateRule", typeof(bool));
            }
        }
    }
}

