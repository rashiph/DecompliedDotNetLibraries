namespace System.Web.Management
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Messaging;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;
    using System.Web;
    using System.Web.Caching;
    using System.Web.Configuration;
    using System.Web.UI;
    using System.Web.Util;

    public class WebBaseEvent
    {
        private int _code;
        private int _detailCode;
        private DateTime _eventTimeUtc;
        private Guid _id;
        private string _message;
        private long _occurrenceNumber;
        private long _sequenceNumber;
        private object _source;
        private static WebApplicationInformation s_applicationInfo = new WebApplicationInformation();
        private static Hashtable s_customEventCodeOccurrence = new Hashtable();
        private static readonly long[,] s_eventCodeOccurrence = new long[WebEventCodes.GetEventArrayDimensionSize(0), WebEventCodes.GetEventArrayDimensionSize(1)];
        private static readonly SystemEventType[,] s_eventCodeToSystemEventTypeMappings = new SystemEventType[WebEventCodes.GetEventArrayDimensionSize(0), WebEventCodes.GetEventArrayDimensionSize(1)];
        private static long s_globalSequenceNumber = 0L;
        private static ReadWriteSpinLock s_lockCustomEventCodeOccurrence;
        private static SystemEventTypeInfo[] s_systemEventTypeInfos = new SystemEventTypeInfo[10];
        private const string WEBEVENT_RAISE_IN_PROGRESS = "_WEvtRIP";

        static WebBaseEvent()
        {
            for (int i = 0; i < s_eventCodeToSystemEventTypeMappings.GetLength(0); i++)
            {
                for (int k = 0; k < s_eventCodeToSystemEventTypeMappings.GetLength(1); k++)
                {
                    s_eventCodeToSystemEventTypeMappings[i, k] = SystemEventType.Unknown;
                }
            }
            for (int j = 0; j < s_eventCodeOccurrence.GetLength(0); j++)
            {
                for (int m = 0; m < s_eventCodeOccurrence.GetLength(1); m++)
                {
                    s_eventCodeOccurrence[j, m] = 0L;
                }
            }
        }

        internal WebBaseEvent()
        {
            this._id = Guid.Empty;
        }

        protected internal WebBaseEvent(string message, object eventSource, int eventCode)
        {
            this._id = Guid.Empty;
            this.Init(message, eventSource, eventCode, 0);
        }

        protected internal WebBaseEvent(string message, object eventSource, int eventCode, int eventDetailCode)
        {
            this._id = Guid.Empty;
            this.Init(message, eventSource, eventCode, eventDetailCode);
        }

        private static WebBaseEvent CreateDummySystemEvent(SystemEventType systemEventType)
        {
            return NewEventFromSystemEventType(true, systemEventType, null, null, 0, 0, null, null);
        }

        private static string CreateWebEventResourceCacheKey(string key)
        {
            return ("x" + key);
        }

        internal void DeconstructWebEvent(out int eventType, out int fieldCount, out string[] fieldNames, out int[] fieldTypes, out string[] fieldData)
        {
            List<WebEventFieldData> fields = new List<WebEventFieldData>();
            eventType = (int) WebEventTypeFromWebEvent(this);
            this.GenerateFieldsForMarshal(fields);
            fieldCount = fields.Count;
            fieldNames = new string[fieldCount];
            fieldData = new string[fieldCount];
            fieldTypes = new int[fieldCount];
            for (int i = 0; i < fieldCount; i++)
            {
                fieldNames[i] = fields[i].Name;
                fieldData[i] = fields[i].Data;
                fieldTypes[i] = (int) fields[i].Type;
            }
        }

        private static void FindEventCode(Exception e, ref int eventCode, ref int eventDetailsCode, ref Exception eStack)
        {
            eventDetailsCode = 0;
            if (e is ConfigurationException)
            {
                eventCode = 0xbc0;
            }
            else if (e is HttpRequestValidationException)
            {
                eventCode = 0xbbb;
            }
            else if (e is HttpCompileException)
            {
                eventCode = 0xbbf;
            }
            else if (e is SecurityException)
            {
                eventCode = 0xfaa;
            }
            else if (e is UnauthorizedAccessException)
            {
                eventCode = 0xfab;
            }
            else if (e is HttpParseException)
            {
                eventCode = 0xbbe;
            }
            else if ((e is HttpException) && (e.InnerException is ViewStateException))
            {
                ViewStateException innerException = (ViewStateException) e.InnerException;
                eventCode = 0xfa9;
                if (innerException._macValidationError)
                {
                    eventDetailsCode = 0xc41b;
                }
                else
                {
                    eventDetailsCode = 0xc41c;
                }
                eStack = innerException;
            }
            else if ((e is HttpException) && (((HttpException) e).WebEventCode != 0))
            {
                eventCode = ((HttpException) e).WebEventCode;
            }
            else if (e.InnerException != null)
            {
                if (eStack == null)
                {
                    eStack = e.InnerException;
                }
                FindEventCode(e.InnerException, ref eventCode, ref eventDetailsCode, ref eStack);
            }
            else
            {
                eventCode = 0xbbd;
            }
            if (eStack == null)
            {
                eStack = e;
            }
        }

        public virtual void FormatCustomEventDetails(WebEventFormatter formatter)
        {
        }

        internal static string FormatResourceStringWithCache(string key)
        {
            CacheInternal cacheInternal = HttpRuntime.CacheInternal;
            if (cacheInternal.IsDisposed)
            {
                return System.Web.SR.Resources.GetString(key, CultureInfo.InstalledUICulture);
            }
            string str2 = CreateWebEventResourceCacheKey(key);
            string str = (string) cacheInternal.Get(str2);
            if (str == null)
            {
                str = System.Web.SR.Resources.GetString(key, CultureInfo.InstalledUICulture);
                if (str != null)
                {
                    cacheInternal.UtcInsert(str2, str);
                }
            }
            return str;
        }

        internal static string FormatResourceStringWithCache(string key, string arg0)
        {
            string format = FormatResourceStringWithCache(key);
            if (format == null)
            {
                return null;
            }
            return string.Format(format, arg0);
        }

        internal virtual void FormatToString(WebEventFormatter formatter, bool includeAppInfo)
        {
            formatter.AppendLine(FormatResourceStringWithCache("Webevent_event_code", this.EventCode.ToString(CultureInfo.InstalledUICulture)));
            formatter.AppendLine(FormatResourceStringWithCache("Webevent_event_message", this.Message));
            formatter.AppendLine(FormatResourceStringWithCache("Webevent_event_time", this.EventTime.ToString(CultureInfo.InstalledUICulture)));
            formatter.AppendLine(FormatResourceStringWithCache("Webevent_event_time_Utc", this.EventTimeUtc.ToString(CultureInfo.InstalledUICulture)));
            formatter.AppendLine(FormatResourceStringWithCache("Webevent_event_id", this.EventID.ToString("N", CultureInfo.InstalledUICulture)));
            formatter.AppendLine(FormatResourceStringWithCache("Webevent_event_sequence", this.EventSequence.ToString(CultureInfo.InstalledUICulture)));
            formatter.AppendLine(FormatResourceStringWithCache("Webevent_event_occurrence", this.EventOccurrence.ToString(CultureInfo.InstalledUICulture)));
            formatter.AppendLine(FormatResourceStringWithCache("Webevent_event_detail_code", this.EventDetailCode.ToString(CultureInfo.InstalledUICulture)));
            if (includeAppInfo)
            {
                formatter.AppendLine(string.Empty);
                formatter.AppendLine(FormatResourceStringWithCache("Webevent_event_application_information"));
                formatter.IndentationLevel++;
                ApplicationInformation.FormatToString(formatter);
                formatter.IndentationLevel--;
            }
        }

        internal virtual void GenerateFieldsForMarshal(List<WebEventFieldData> fields)
        {
            fields.Add(new WebEventFieldData("EventTime", this.EventTimeUtc.ToString(), WebEventFieldType.String));
            fields.Add(new WebEventFieldData("EventID", this.EventID.ToString(), WebEventFieldType.String));
            fields.Add(new WebEventFieldData("EventMessage", this.Message, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("ApplicationDomain", ApplicationInformation.ApplicationDomain, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("TrustLevel", ApplicationInformation.TrustLevel, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("ApplicationVirtualPath", ApplicationInformation.ApplicationVirtualPath, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("ApplicationPath", ApplicationInformation.ApplicationPath, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("MachineName", ApplicationInformation.MachineName, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("EventCode", this.EventCode.ToString(CultureInfo.InstalledUICulture), WebEventFieldType.Int));
            fields.Add(new WebEventFieldData("EventDetailCode", this.EventDetailCode.ToString(CultureInfo.InstalledUICulture), WebEventFieldType.Int));
            fields.Add(new WebEventFieldData("SequenceNumber", this.EventSequence.ToString(CultureInfo.InstalledUICulture), WebEventFieldType.Long));
            fields.Add(new WebEventFieldData("Occurrence", this.EventOccurrence.ToString(CultureInfo.InstalledUICulture), WebEventFieldType.Long));
        }

        private static void GetSystemEventTypeInfo(int eventCode, int index0, int index1, out SystemEventTypeInfo info, out SystemEventType systemEventType)
        {
            systemEventType = s_eventCodeToSystemEventTypeMappings[index0, index1];
            if (systemEventType == SystemEventType.Unknown)
            {
                systemEventType = SystemEventTypeFromEventCode(eventCode);
                s_eventCodeToSystemEventTypeMappings[index0, index1] = systemEventType;
            }
            info = s_systemEventTypeInfos[(int) systemEventType];
            if (info == null)
            {
                info = new SystemEventTypeInfo(CreateDummySystemEvent(systemEventType));
                s_systemEventTypeInfos[(int) systemEventType] = info;
            }
        }

        protected internal virtual void IncrementPerfCounters()
        {
            PerfCounters.IncrementCounter(AppPerfCounter.EVENTS_TOTAL);
        }

        internal void IncrementTotalCounters(int index0, int index1)
        {
            this._sequenceNumber = Interlocked.Increment(ref s_globalSequenceNumber);
            if (index0 != -1)
            {
                this._occurrenceNumber = Interlocked.Increment(ref s_eventCodeOccurrence[index0, index1]);
            }
            else
            {
                CustomEventCodeOccurrence occurrence = (CustomEventCodeOccurrence) s_customEventCodeOccurrence[this._code];
                if (occurrence == null)
                {
                    s_lockCustomEventCodeOccurrence.AcquireWriterLock();
                    try
                    {
                        occurrence = (CustomEventCodeOccurrence) s_customEventCodeOccurrence[this._code];
                        if (occurrence == null)
                        {
                            occurrence = new CustomEventCodeOccurrence();
                            s_customEventCodeOccurrence[this._code] = occurrence;
                        }
                    }
                    finally
                    {
                        s_lockCustomEventCodeOccurrence.ReleaseWriterLock();
                    }
                }
                this._occurrenceNumber = Interlocked.Increment(ref occurrence._occurrence);
            }
        }

        internal int InferEtwTraceVerbosity()
        {
            switch (WebEventTypeFromWebEvent(this))
            {
                case WebEventType.WEBEVENT_BASE_ERROR_EVENT:
                case WebEventType.WEBEVENT_REQUEST_ERROR_EVENT:
                case WebEventType.WEBEVENT_ERROR_EVENT:
                case WebEventType.WEBEVENT_FAILURE_AUDIT_EVENT:
                case WebEventType.WEBEVENT_AUTHENTICATION_FAILURE_AUDIT_EVENT:
                case WebEventType.WEBEVENT_VIEWSTATE_FAILURE_AUDIT_EVENT:
                    return 3;

                case WebEventType.WEBEVENT_AUDIT_EVENT:
                case WebEventType.WEBEVENT_SUCCESS_AUDIT_EVENT:
                case WebEventType.WEBEVENT_AUTHENTICATION_SUCCESS_AUDIT_EVENT:
                    return 4;
            }
            return 5;
        }

        private void Init(string message, object eventSource, int eventCode, int eventDetailCode)
        {
            if (eventCode < 0)
            {
                throw new ArgumentOutOfRangeException("eventCode", System.Web.SR.GetString("Invalid_eventCode_error"));
            }
            if (eventDetailCode < 0)
            {
                throw new ArgumentOutOfRangeException("eventDetailCode", System.Web.SR.GetString("Invalid_eventDetailCode_error"));
            }
            this._code = eventCode;
            this._detailCode = eventDetailCode;
            this._source = eventSource;
            this._eventTimeUtc = DateTime.UtcNow;
            this._message = message;
        }

        private static WebBaseEvent NewEventFromSystemEventType(bool createDummy, SystemEventType systemEventType, string message, object source, int eventCode, int eventDetailCode, Exception exception, string nameToAuthenticate)
        {
            if (!createDummy && (message == null))
            {
                message = WebEventCodes.MessageFromEventCode(eventCode, eventDetailCode);
            }
            switch (systemEventType)
            {
                case SystemEventType.WebApplicationLifetimeEvent:
                    if (createDummy)
                    {
                        return new WebApplicationLifetimeEvent();
                    }
                    return new WebApplicationLifetimeEvent(message, source, eventCode, eventDetailCode);

                case SystemEventType.WebHeartbeatEvent:
                    if (createDummy)
                    {
                        return new WebHeartbeatEvent();
                    }
                    return new WebHeartbeatEvent(message, eventCode);

                case SystemEventType.WebRequestEvent:
                    if (createDummy)
                    {
                        return new WebRequestEvent();
                    }
                    return new WebRequestEvent(message, source, eventCode, eventDetailCode);

                case SystemEventType.WebRequestErrorEvent:
                    if (createDummy)
                    {
                        return new WebRequestErrorEvent();
                    }
                    return new WebRequestErrorEvent(message, source, eventCode, eventDetailCode, exception);

                case SystemEventType.WebErrorEvent:
                    if (createDummy)
                    {
                        return new WebErrorEvent();
                    }
                    return new WebErrorEvent(message, source, eventCode, eventDetailCode, exception);

                case SystemEventType.WebAuthenticationSuccessAuditEvent:
                    if (createDummy)
                    {
                        return new WebAuthenticationSuccessAuditEvent();
                    }
                    return new WebAuthenticationSuccessAuditEvent(message, source, eventCode, eventDetailCode, nameToAuthenticate);

                case SystemEventType.WebSuccessAuditEvent:
                    if (createDummy)
                    {
                        return new WebSuccessAuditEvent();
                    }
                    return new WebSuccessAuditEvent(message, source, eventCode, eventDetailCode);

                case SystemEventType.WebAuthenticationFailureAuditEvent:
                    if (createDummy)
                    {
                        return new WebAuthenticationFailureAuditEvent();
                    }
                    return new WebAuthenticationFailureAuditEvent(message, source, eventCode, eventDetailCode, nameToAuthenticate);

                case SystemEventType.WebFailureAuditEvent:
                    if (createDummy)
                    {
                        return new WebFailureAuditEvent();
                    }
                    return new WebFailureAuditEvent(message, source, eventCode, eventDetailCode);

                case SystemEventType.WebViewStateFailureAuditEvent:
                    if (createDummy)
                    {
                        return new WebViewStateFailureAuditEvent();
                    }
                    return new WebViewStateFailureAuditEvent(message, source, eventCode, eventDetailCode, (ViewStateException) exception);
            }
            return null;
        }

        internal virtual void PreProcessEventInit()
        {
        }

        [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.Medium)]
        public virtual void Raise()
        {
            Raise(this);
        }

        [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.Medium)]
        public static void Raise(WebBaseEvent eventRaised)
        {
            if (eventRaised.EventCode < 0x186a0)
            {
                object[] args = new object[] { eventRaised.EventCode.ToString(CultureInfo.CurrentCulture), 0x186a0.ToString(CultureInfo.CurrentCulture) };
                throw new HttpException(System.Web.SR.GetString("System_eventCode_not_allowed", args));
            }
            if (HealthMonitoringManager.Enabled)
            {
                RaiseInternal(eventRaised, null, -1, -1);
            }
        }

        internal static void RaiseInternal(WebBaseEvent eventRaised, ArrayList firingRuleInfos, int index0, int index1)
        {
            bool flag = false;
            bool flag2 = false;
            ProcessImpersonationContext context = null;
            HttpContext current = HttpContext.Current;
            object data = CallContext.GetData("_WEvtRIP");
            if ((data == null) || !((bool) data))
            {
                eventRaised.IncrementPerfCounters();
                eventRaised.IncrementTotalCounters(index0, index1);
                if (firingRuleInfos == null)
                {
                    firingRuleInfos = HealthMonitoringManager.Manager()._sectionHelper.FindFiringRuleInfos(eventRaised.GetType(), eventRaised.EventCode);
                }
                if (firingRuleInfos.Count != 0)
                {
                    try
                    {
                        bool[] flagArray = null;
                        if (EtwTrace.IsTraceEnabled(5, 1) && (current != null))
                        {
                            EtwTrace.Trace(EtwTraceType.ETW_TYPE_WEB_EVENT_RAISE_START, current.WorkerRequest, eventRaised.GetType().FullName, eventRaised.EventCode.ToString(CultureInfo.InstalledUICulture), eventRaised.EventDetailCode.ToString(CultureInfo.InstalledUICulture), null);
                        }
                        try
                        {
                            foreach (HealthMonitoringSectionHelper.FiringRuleInfo info in firingRuleInfos)
                            {
                                HealthMonitoringSectionHelper.RuleInfo info2 = info._ruleInfo;
                                if (info2._ruleFiringRecord.CheckAndUpdate(eventRaised) && (info2._referencedProvider != null))
                                {
                                    if (!flag)
                                    {
                                        eventRaised.PreProcessEventInit();
                                        flag = true;
                                    }
                                    if (info._indexOfFirstRuleInfoWithSameProvider != -1)
                                    {
                                        if (flagArray == null)
                                        {
                                            flagArray = new bool[firingRuleInfos.Count];
                                        }
                                        if (flagArray[info._indexOfFirstRuleInfoWithSameProvider])
                                        {
                                            continue;
                                        }
                                        flagArray[info._indexOfFirstRuleInfoWithSameProvider] = true;
                                    }
                                    if (EtwTrace.IsTraceEnabled(5, 1) && (current != null))
                                    {
                                        EtwTrace.Trace(EtwTraceType.ETW_TYPE_WEB_EVENT_DELIVER_START, current.WorkerRequest, info2._ruleSettings.Provider, info2._ruleSettings.Name, info2._ruleSettings.EventName, null);
                                    }
                                    try
                                    {
                                        if (context == null)
                                        {
                                            context = new ProcessImpersonationContext();
                                        }
                                        if (!flag2)
                                        {
                                            CallContext.SetData("_WEvtRIP", true);
                                            flag2 = true;
                                        }
                                        info2._referencedProvider.ProcessEvent(eventRaised);
                                    }
                                    catch (Exception exception)
                                    {
                                        try
                                        {
                                            info2._referencedProvider.LogException(exception);
                                        }
                                        catch
                                        {
                                        }
                                    }
                                    finally
                                    {
                                        if (EtwTrace.IsTraceEnabled(5, 1) && (current != null))
                                        {
                                            EtwTrace.Trace(EtwTraceType.ETW_TYPE_WEB_EVENT_DELIVER_END, current.WorkerRequest);
                                        }
                                    }
                                }
                            }
                        }
                        finally
                        {
                            if (context != null)
                            {
                                context.Undo();
                            }
                            if (flag2)
                            {
                                CallContext.FreeNamedDataSlot("_WEvtRIP");
                            }
                            if (EtwTrace.IsTraceEnabled(5, 1) && (current != null))
                            {
                                EtwTrace.Trace(EtwTraceType.ETW_TYPE_WEB_EVENT_RAISE_END, current.WorkerRequest);
                            }
                        }
                    }
                    catch
                    {
                        throw;
                    }
                }
            }
        }

        internal static void RaisePropertyDeserializationWebErrorEvent(SettingsProperty property, object source, Exception exception)
        {
            if (HttpContext.Current != null)
            {
                RaiseSystemEvent(System.Web.SR.GetString("Webevent_msg_Property_Deserialization", new object[] { property.Name, property.SerializeAs.ToString(), property.PropertyType.AssemblyQualifiedName }), source, 0xbc2, 0, exception);
            }
        }

        internal static void RaiseRuntimeError(Exception e, object source)
        {
            if (HealthMonitoringManager.Enabled)
            {
                try
                {
                    int eventCode = 0;
                    int eventDetailsCode = 0;
                    HttpContext current = HttpContext.Current;
                    Exception eStack = null;
                    if (current != null)
                    {
                        Page handler = current.Handler as Page;
                        if (((handler != null) && handler.IsTransacted) && ((e.GetType() == typeof(HttpException)) && (e.InnerException != null)))
                        {
                            e = e.InnerException;
                        }
                    }
                    FindEventCode(e, ref eventCode, ref eventDetailsCode, ref eStack);
                    RaiseSystemEvent(source, eventCode, eventDetailsCode, eStack);
                }
                catch
                {
                }
            }
        }

        internal static void RaiseSystemEvent(object source, int eventCode)
        {
            RaiseSystemEventInternal(null, source, eventCode, 0, null, null);
        }

        internal static void RaiseSystemEvent(object source, int eventCode, int eventDetailCode)
        {
            RaiseSystemEventInternal(null, source, eventCode, eventDetailCode, null, null);
        }

        internal static void RaiseSystemEvent(object source, int eventCode, string nameToAuthenticate)
        {
            RaiseSystemEventInternal(null, source, eventCode, 0, null, nameToAuthenticate);
        }

        internal static void RaiseSystemEvent(object source, int eventCode, int eventDetailCode, Exception exception)
        {
            RaiseSystemEventInternal(null, source, eventCode, eventDetailCode, exception, null);
        }

        internal static void RaiseSystemEvent(string message, object source, int eventCode, int eventDetailCode, Exception exception)
        {
            RaiseSystemEventInternal(message, source, eventCode, eventDetailCode, exception, null);
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        private static void RaiseSystemEventInternal(string message, object source, int eventCode, int eventDetailCode, Exception exception, string nameToAuthenticate)
        {
            if (HealthMonitoringManager.Enabled)
            {
                SystemEventTypeInfo info;
                SystemEventType type;
                int num;
                int num2;
                WebEventCodes.GetEventArrayIndexsFromEventCode(eventCode, out num, out num2);
                GetSystemEventTypeInfo(eventCode, num, num2, out info, out type);
                if (info != null)
                {
                    ArrayList firingRuleInfos = HealthMonitoringManager.Manager()._sectionHelper.FindFiringRuleInfos(info._type, eventCode);
                    if (firingRuleInfos.Count == 0)
                    {
                        info._dummyEvent.IncrementPerfCounters();
                        info._dummyEvent.IncrementTotalCounters(num, num2);
                    }
                    else
                    {
                        RaiseInternal(NewEventFromSystemEventType(false, type, message, source, eventCode, eventDetailCode, exception, nameToAuthenticate), firingRuleInfos, num, num2);
                    }
                }
            }
        }

        private static SystemEventType SystemEventTypeFromEventCode(int eventCode)
        {
            if ((eventCode >= 0x3e8) && (eventCode <= 0x3ed))
            {
                switch (eventCode)
                {
                    case 0x3e9:
                    case 0x3ea:
                    case 0x3eb:
                    case 0x3ec:
                        return SystemEventType.WebApplicationLifetimeEvent;

                    case 0x3ed:
                        return SystemEventType.WebHeartbeatEvent;
                }
            }
            if ((eventCode >= 0x7d0) && (eventCode <= 0x7d2))
            {
                switch (eventCode)
                {
                    case 0x7d1:
                    case 0x7d2:
                        return SystemEventType.WebRequestEvent;
                }
            }
            if ((eventCode >= 0xbb8) && (eventCode <= 0xbc3))
            {
                switch (eventCode)
                {
                    case 0xbb9:
                    case 0xbba:
                    case 0xbbb:
                    case 0xbbc:
                    case 0xbbd:
                        return SystemEventType.WebRequestErrorEvent;

                    case 0xbbe:
                    case 0xbbf:
                    case 0xbc0:
                    case 0xbc1:
                    case 0xbc2:
                    case 0xbc3:
                        return SystemEventType.WebErrorEvent;
                }
            }
            if ((eventCode >= 0xfa0) && (eventCode <= 0xfab))
            {
                switch (eventCode)
                {
                    case 0xfa1:
                    case 0xfa2:
                        return SystemEventType.WebAuthenticationSuccessAuditEvent;

                    case 0xfa3:
                    case 0xfa4:
                        return SystemEventType.WebSuccessAuditEvent;

                    case 0xfa5:
                    case 0xfa6:
                        return SystemEventType.WebAuthenticationFailureAuditEvent;

                    case 0xfa7:
                    case 0xfa8:
                    case 0xfaa:
                    case 0xfab:
                        return SystemEventType.WebFailureAuditEvent;

                    case 0xfa9:
                        return SystemEventType.WebViewStateFailureAuditEvent;
                }
            }
            if ((eventCode >= 0x1770) && (eventCode <= 0x1771))
            {
                if (eventCode == 0x1771)
                {
                    return SystemEventType.Unknown;
                }
            }
            return SystemEventType.Unknown;
        }

        public override string ToString()
        {
            return this.ToString(true, true);
        }

        public virtual string ToString(bool includeAppInfo, bool includeCustomEventDetails)
        {
            WebEventFormatter formatter = new WebEventFormatter();
            this.FormatToString(formatter, includeAppInfo);
            if (!this.IsSystemEvent && includeCustomEventDetails)
            {
                formatter.AppendLine(string.Empty);
                formatter.AppendLine(FormatResourceStringWithCache("Webevent_event_custom_event_details"));
                formatter.IndentationLevel++;
                this.FormatCustomEventDetails(formatter);
                formatter.IndentationLevel--;
            }
            return formatter.ToString();
        }

        internal static WebEventType WebEventTypeFromWebEvent(WebBaseEvent eventRaised)
        {
            if (eventRaised is WebAuthenticationSuccessAuditEvent)
            {
                return WebEventType.WEBEVENT_AUTHENTICATION_SUCCESS_AUDIT_EVENT;
            }
            if (eventRaised is WebAuthenticationFailureAuditEvent)
            {
                return WebEventType.WEBEVENT_AUTHENTICATION_FAILURE_AUDIT_EVENT;
            }
            if (eventRaised is WebViewStateFailureAuditEvent)
            {
                return WebEventType.WEBEVENT_VIEWSTATE_FAILURE_AUDIT_EVENT;
            }
            if (eventRaised is WebRequestErrorEvent)
            {
                return WebEventType.WEBEVENT_REQUEST_ERROR_EVENT;
            }
            if (eventRaised is WebErrorEvent)
            {
                return WebEventType.WEBEVENT_ERROR_EVENT;
            }
            if (eventRaised is WebSuccessAuditEvent)
            {
                return WebEventType.WEBEVENT_SUCCESS_AUDIT_EVENT;
            }
            if (eventRaised is WebFailureAuditEvent)
            {
                return WebEventType.WEBEVENT_FAILURE_AUDIT_EVENT;
            }
            if (eventRaised is WebHeartbeatEvent)
            {
                return WebEventType.WEBEVENT_HEARTBEAT_EVENT;
            }
            if (eventRaised is WebApplicationLifetimeEvent)
            {
                return WebEventType.WEBEVENT_APP_LIFETIME_EVENT;
            }
            if (eventRaised is WebRequestEvent)
            {
                return WebEventType.WEBEVENT_REQUEST_EVENT;
            }
            if (eventRaised is WebBaseErrorEvent)
            {
                return WebEventType.WEBEVENT_BASE_ERROR_EVENT;
            }
            if (eventRaised is WebAuditEvent)
            {
                return WebEventType.WEBEVENT_AUDIT_EVENT;
            }
            if (eventRaised is WebManagementEvent)
            {
                return WebEventType.WEBEVENT_MANAGEMENT_EVENT;
            }
            return WebEventType.WEBEVENT_BASE_EVENT;
        }

        public static WebApplicationInformation ApplicationInformation
        {
            get
            {
                return s_applicationInfo;
            }
        }

        public int EventCode
        {
            get
            {
                return this._code;
            }
        }

        public int EventDetailCode
        {
            get
            {
                return this._detailCode;
            }
        }

        public Guid EventID
        {
            get
            {
                if (this._id == Guid.Empty)
                {
                    lock (this)
                    {
                        if (this._id == Guid.Empty)
                        {
                            this._id = Guid.NewGuid();
                        }
                    }
                }
                return this._id;
            }
        }

        public long EventOccurrence
        {
            get
            {
                return this._occurrenceNumber;
            }
        }

        public long EventSequence
        {
            get
            {
                return this._sequenceNumber;
            }
        }

        public object EventSource
        {
            get
            {
                return this._source;
            }
        }

        public DateTime EventTime
        {
            get
            {
                return this._eventTimeUtc.ToLocalTime();
            }
        }

        public DateTime EventTimeUtc
        {
            get
            {
                return this._eventTimeUtc;
            }
        }

        internal bool IsSystemEvent
        {
            get
            {
                return (this._code < 0x186a0);
            }
        }

        public string Message
        {
            get
            {
                return this._message;
            }
        }

        private class CustomEventCodeOccurrence
        {
            internal long _occurrence;
        }

        private enum SystemEventType
        {
            Last = 10,
            Unknown = -1,
            WebApplicationLifetimeEvent = 0,
            WebAuthenticationFailureAuditEvent = 7,
            WebAuthenticationSuccessAuditEvent = 5,
            WebErrorEvent = 4,
            WebFailureAuditEvent = 8,
            WebHeartbeatEvent = 1,
            WebRequestErrorEvent = 3,
            WebRequestEvent = 2,
            WebSuccessAuditEvent = 6,
            WebViewStateFailureAuditEvent = 9
        }

        private class SystemEventTypeInfo
        {
            internal WebBaseEvent _dummyEvent;
            internal Type _type;

            internal SystemEventTypeInfo(WebBaseEvent dummyEvent)
            {
                this._dummyEvent = dummyEvent;
                this._type = dummyEvent.GetType();
            }
        }
    }
}

