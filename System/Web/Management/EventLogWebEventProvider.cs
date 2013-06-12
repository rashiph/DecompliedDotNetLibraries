namespace System.Web.Management
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Security.Principal;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    public sealed class EventLogWebEventProvider : WebEventProvider, IInternalWebEventProvider
    {
        private int _maxTruncatedParamLen;
        private const string _truncateWarning = "...";
        private const int EventLogParameterMaxLength = 0x7ffe;
        private const int MAX_CHARS_IN_EXCEPTION_MSG = 0x1f40;

        internal EventLogWebEventProvider()
        {
        }

        private void AddBasicDataFields(ArrayList dataFields, WebBaseEvent eventRaised)
        {
            WebApplicationInformation applicationInformation = WebBaseEvent.ApplicationInformation;
            dataFields.Add(eventRaised.EventCode.ToString(CultureInfo.InstalledUICulture));
            dataFields.Add(eventRaised.Message);
            dataFields.Add(eventRaised.EventTime.ToString());
            dataFields.Add(eventRaised.EventTimeUtc.ToString());
            dataFields.Add(eventRaised.EventID.ToString("N", CultureInfo.InstalledUICulture));
            dataFields.Add(eventRaised.EventSequence.ToString(CultureInfo.InstalledUICulture));
            dataFields.Add(eventRaised.EventOccurrence.ToString(CultureInfo.InstalledUICulture));
            dataFields.Add(eventRaised.EventDetailCode.ToString(CultureInfo.InstalledUICulture));
            dataFields.Add(applicationInformation.ApplicationDomain);
            dataFields.Add(applicationInformation.TrustLevel);
            dataFields.Add(applicationInformation.ApplicationVirtualPath);
            dataFields.Add(applicationInformation.ApplicationPath);
            dataFields.Add(applicationInformation.MachineName);
            if (eventRaised.IsSystemEvent)
            {
                dataFields.Add(null);
            }
            else
            {
                WebEventFormatter formatter = new WebEventFormatter();
                eventRaised.FormatCustomEventDetails(formatter);
                dataFields.Add(formatter.ToString());
            }
        }

        private void AddExceptionDataFields(ArrayList dataFields, Exception exception)
        {
            if (exception == null)
            {
                dataFields.Add(null);
                dataFields.Add(null);
            }
            else
            {
                dataFields.Add(exception.GetType().Name);
                StringBuilder builder = new StringBuilder(0x400);
                int num = 0;
                while ((num < 0x1f40) && (exception != null))
                {
                    string str = ReplaceInsertionStringPlaceholders(exception.Message);
                    builder.Append(str);
                    num += str.Length;
                    int length = 0x1f40 - num;
                    if (length > 0)
                    {
                        string stackTrace = exception.StackTrace;
                        if (!string.IsNullOrEmpty(stackTrace))
                        {
                            if (stackTrace.Length > length)
                            {
                                stackTrace = stackTrace.Substring(0, length);
                            }
                            builder.Append("\n");
                            builder.Append(stackTrace);
                            num += stackTrace.Length + 1;
                        }
                        builder.Append("\n\n");
                        num += 2;
                    }
                    exception = exception.InnerException;
                }
                dataFields.Add(builder.ToString());
            }
        }

        private void AddViewStateExceptionDataFields(ArrayList dataFields, ViewStateException vse)
        {
            dataFields.Add(System.Web.SR.GetString(vse.ShortMessage));
            dataFields.Add(vse.RemoteAddress);
            dataFields.Add(vse.RemotePort);
            dataFields.Add(vse.UserAgent);
            dataFields.Add(vse.PersistedState);
            dataFields.Add(vse.Referer);
            dataFields.Add(vse.Path);
        }

        private void AddWebProcessInformationDataFields(ArrayList dataFields, WebProcessInformation processEventInfo)
        {
            dataFields.Add(processEventInfo.ProcessID.ToString(CultureInfo.InstalledUICulture));
            dataFields.Add(processEventInfo.ProcessName);
            dataFields.Add(processEventInfo.AccountName);
        }

        private void AddWebProcessStatisticsDataFields(ArrayList dataFields, WebProcessStatistics procStats)
        {
            dataFields.Add(procStats.ProcessStartTime.ToString(CultureInfo.InstalledUICulture));
            dataFields.Add(procStats.ThreadCount.ToString(CultureInfo.InstalledUICulture));
            dataFields.Add(procStats.WorkingSet.ToString(CultureInfo.InstalledUICulture));
            dataFields.Add(procStats.PeakWorkingSet.ToString(CultureInfo.InstalledUICulture));
            dataFields.Add(procStats.ManagedHeapSize.ToString(CultureInfo.InstalledUICulture));
            dataFields.Add(procStats.AppDomainCount.ToString(CultureInfo.InstalledUICulture));
            dataFields.Add(procStats.RequestsExecuting.ToString(CultureInfo.InstalledUICulture));
            dataFields.Add(procStats.RequestsQueued.ToString(CultureInfo.InstalledUICulture));
            dataFields.Add(procStats.RequestsRejected.ToString(CultureInfo.InstalledUICulture));
        }

        private void AddWebRequestInformationDataFields(ArrayList dataFields, WebRequestInformation reqInfo)
        {
            string name;
            string authenticationType;
            bool isAuthenticated;
            IPrincipal principal = reqInfo.Principal;
            if (principal == null)
            {
                name = null;
                isAuthenticated = false;
                authenticationType = null;
            }
            else
            {
                IIdentity identity = principal.Identity;
                name = identity.Name;
                isAuthenticated = identity.IsAuthenticated;
                authenticationType = identity.AuthenticationType;
            }
            dataFields.Add(HttpUtility.UrlDecode(reqInfo.RequestUrl));
            dataFields.Add(reqInfo.RequestPath);
            dataFields.Add(reqInfo.UserHostAddress);
            dataFields.Add(name);
            dataFields.Add(isAuthenticated.ToString());
            dataFields.Add(authenticationType);
            dataFields.Add(reqInfo.ThreadAccountName);
        }

        private void AddWebThreadInformationDataFields(ArrayList dataFields, WebThreadInformation threadInfo)
        {
            dataFields.Add(threadInfo.ThreadID.ToString(CultureInfo.InstalledUICulture));
            dataFields.Add(threadInfo.ThreadAccountName);
            dataFields.Add(threadInfo.IsImpersonating.ToString(CultureInfo.InstalledUICulture));
            dataFields.Add(threadInfo.StackTrace);
        }

        public override void Flush()
        {
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            this._maxTruncatedParamLen = 0x7ffe - "...".Length;
            base.Initialize(name, config);
            ProviderUtil.CheckUnrecognizedAttributes(config, name);
        }

        public override void ProcessEvent(WebBaseEvent eventRaised)
        {
            ArrayList dataFields = new ArrayList(0x23);
            WebEventType type = WebBaseEvent.WebEventTypeFromWebEvent(eventRaised);
            this.AddBasicDataFields(dataFields, eventRaised);
            if (eventRaised is WebManagementEvent)
            {
                this.AddWebProcessInformationDataFields(dataFields, ((WebManagementEvent) eventRaised).ProcessInformation);
            }
            if (eventRaised is WebHeartbeatEvent)
            {
                this.AddWebProcessStatisticsDataFields(dataFields, ((WebHeartbeatEvent) eventRaised).ProcessStatistics);
            }
            if (eventRaised is WebRequestEvent)
            {
                this.AddWebRequestInformationDataFields(dataFields, ((WebRequestEvent) eventRaised).RequestInformation);
            }
            if (eventRaised is WebBaseErrorEvent)
            {
                this.AddExceptionDataFields(dataFields, ((WebBaseErrorEvent) eventRaised).ErrorException);
            }
            if (eventRaised is WebAuditEvent)
            {
                this.AddWebRequestInformationDataFields(dataFields, ((WebAuditEvent) eventRaised).RequestInformation);
            }
            if (eventRaised is WebRequestErrorEvent)
            {
                this.AddWebRequestInformationDataFields(dataFields, ((WebRequestErrorEvent) eventRaised).RequestInformation);
                this.AddWebThreadInformationDataFields(dataFields, ((WebRequestErrorEvent) eventRaised).ThreadInformation);
            }
            if (eventRaised is WebErrorEvent)
            {
                this.AddWebRequestInformationDataFields(dataFields, ((WebErrorEvent) eventRaised).RequestInformation);
                this.AddWebThreadInformationDataFields(dataFields, ((WebErrorEvent) eventRaised).ThreadInformation);
            }
            if (eventRaised is WebAuthenticationSuccessAuditEvent)
            {
                dataFields.Add(((WebAuthenticationSuccessAuditEvent) eventRaised).NameToAuthenticate);
            }
            if (eventRaised is WebAuthenticationFailureAuditEvent)
            {
                dataFields.Add(((WebAuthenticationFailureAuditEvent) eventRaised).NameToAuthenticate);
            }
            if (eventRaised is WebViewStateFailureAuditEvent)
            {
                this.AddViewStateExceptionDataFields(dataFields, ((WebViewStateFailureAuditEvent) eventRaised).ViewStateException);
            }
            for (int i = 0; i < dataFields.Count; i++)
            {
                object obj2 = dataFields[i];
                if ((obj2 != null) && (((string) obj2).Length > 0x7ffe))
                {
                    dataFields[i] = ((string) obj2).Substring(0, this._maxTruncatedParamLen) + "...";
                }
            }
            int num = UnsafeNativeMethods.RaiseEventlogEvent((int) type, (string[]) dataFields.ToArray(typeof(string)), dataFields.Count);
            if (num != 0)
            {
                throw new HttpException(System.Web.SR.GetString("Event_log_provider_error", new object[] { "0x" + num.ToString("X8", CultureInfo.InstalledUICulture) }));
            }
        }

        private static string ReplaceInsertionStringPlaceholders(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }
            int length = s.Length;
            int num2 = length - 1;
            int num3 = 0;
            for (int i = 0; i < num2; i++)
            {
                if ((s[i] == '%') && char.IsDigit(s[i + 1]))
                {
                    num3++;
                }
            }
            if (num3 == 0)
            {
                return s;
            }
            char[] chArray = new char[length + (2 * num3)];
            int num5 = 0;
            for (int j = 0; j < num2; j++)
            {
                if ((s[j] == '%') && char.IsDigit(s[j + 1]))
                {
                    chArray[num5++] = '[';
                    chArray[num5++] = '%';
                    chArray[num5++] = ']';
                }
                else
                {
                    chArray[num5++] = s[j];
                }
            }
            chArray[chArray.Length - 1] = s[num2];
            return new string(chArray);
        }

        public override void Shutdown()
        {
        }
    }
}

