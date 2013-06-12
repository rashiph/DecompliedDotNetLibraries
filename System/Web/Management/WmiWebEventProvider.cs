namespace System.Web.Management
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Globalization;
    using System.Security.Principal;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    public class WmiWebEventProvider : WebEventProvider
    {
        private void FillBasicWmiDataFields(ref UnsafeNativeMethods.WmiData wmiData, WebBaseEvent eventRaised)
        {
            WebApplicationInformation applicationInformation = WebBaseEvent.ApplicationInformation;
            wmiData.eventType = (int) WebBaseEvent.WebEventTypeFromWebEvent(eventRaised);
            wmiData.eventCode = eventRaised.EventCode;
            wmiData.eventDetailCode = eventRaised.EventDetailCode;
            wmiData.eventTime = this.WmiFormatTime(eventRaised.EventTime);
            wmiData.eventMessage = eventRaised.Message;
            wmiData.sequenceNumber = eventRaised.EventSequence.ToString(CultureInfo.InstalledUICulture);
            wmiData.occurrence = eventRaised.EventOccurrence.ToString(CultureInfo.InstalledUICulture);
            wmiData.eventId = eventRaised.EventID.ToString("N", CultureInfo.InstalledUICulture);
            wmiData.appDomain = applicationInformation.ApplicationDomain;
            wmiData.trustLevel = applicationInformation.TrustLevel;
            wmiData.appVirtualPath = applicationInformation.ApplicationVirtualPath;
            wmiData.appPath = applicationInformation.ApplicationPath;
            wmiData.machineName = applicationInformation.MachineName;
            if (eventRaised.IsSystemEvent)
            {
                wmiData.details = string.Empty;
            }
            else
            {
                WebEventFormatter formatter = new WebEventFormatter();
                eventRaised.FormatCustomEventDetails(formatter);
                wmiData.details = formatter.ToString();
            }
        }

        private void FillErrorWmiDataFields(ref UnsafeNativeMethods.WmiData wmiData, WebThreadInformation threadInfo)
        {
            wmiData.threadId = threadInfo.ThreadID;
            wmiData.threadAccountName = threadInfo.ThreadAccountName;
            wmiData.stackTrace = threadInfo.StackTrace;
            wmiData.isImpersonating = threadInfo.IsImpersonating;
        }

        private void FillRequestWmiDataFields(ref UnsafeNativeMethods.WmiData wmiData, WebRequestInformation reqInfo)
        {
            string name;
            string authenticationType;
            bool isAuthenticated;
            IPrincipal principal = reqInfo.Principal;
            if (principal == null)
            {
                name = string.Empty;
                authenticationType = string.Empty;
                isAuthenticated = false;
            }
            else
            {
                IIdentity identity = principal.Identity;
                name = identity.Name;
                isAuthenticated = identity.IsAuthenticated;
                authenticationType = identity.AuthenticationType;
            }
            wmiData.requestUrl = reqInfo.RequestUrl;
            wmiData.requestPath = reqInfo.RequestPath;
            wmiData.userHostAddress = reqInfo.UserHostAddress;
            wmiData.userName = name;
            wmiData.userAuthenticated = isAuthenticated;
            wmiData.userAuthenticationType = authenticationType;
            wmiData.requestThreadAccountName = reqInfo.ThreadAccountName;
        }

        public override void Flush()
        {
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            int num = UnsafeNativeMethods.InitializeWmiManager();
            if (num != 0)
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Wmi_provider_cant_initialize", new object[] { "0x" + num.ToString("X8", CultureInfo.CurrentCulture) }));
            }
            base.Initialize(name, config);
            ProviderUtil.CheckUnrecognizedAttributes(config, name);
        }

        public override void ProcessEvent(WebBaseEvent eventRaised)
        {
            UnsafeNativeMethods.WmiData wmiData = new UnsafeNativeMethods.WmiData();
            this.FillBasicWmiDataFields(ref wmiData, eventRaised);
            WebApplicationLifetimeEvent event1 = eventRaised as WebApplicationLifetimeEvent;
            if (eventRaised is WebManagementEvent)
            {
                WebProcessInformation processInformation = ((WebManagementEvent) eventRaised).ProcessInformation;
                wmiData.processId = processInformation.ProcessID;
                wmiData.processName = processInformation.ProcessName;
                wmiData.accountName = processInformation.AccountName;
            }
            if (eventRaised is WebRequestEvent)
            {
                this.FillRequestWmiDataFields(ref wmiData, ((WebRequestEvent) eventRaised).RequestInformation);
            }
            if (eventRaised is WebAuditEvent)
            {
                this.FillRequestWmiDataFields(ref wmiData, ((WebAuditEvent) eventRaised).RequestInformation);
            }
            if (eventRaised is WebAuthenticationSuccessAuditEvent)
            {
                wmiData.nameToAuthenticate = ((WebAuthenticationSuccessAuditEvent) eventRaised).NameToAuthenticate;
            }
            if (eventRaised is WebAuthenticationFailureAuditEvent)
            {
                wmiData.nameToAuthenticate = ((WebAuthenticationFailureAuditEvent) eventRaised).NameToAuthenticate;
            }
            if (eventRaised is WebViewStateFailureAuditEvent)
            {
                ViewStateException viewStateException = ((WebViewStateFailureAuditEvent) eventRaised).ViewStateException;
                wmiData.exceptionMessage = System.Web.SR.GetString(viewStateException.ShortMessage);
                wmiData.remoteAddress = viewStateException.RemoteAddress;
                wmiData.remotePort = viewStateException.RemotePort;
                wmiData.userAgent = viewStateException.UserAgent;
                wmiData.persistedState = viewStateException.PersistedState;
                wmiData.referer = viewStateException.Referer;
                wmiData.path = viewStateException.Path;
            }
            if (eventRaised is WebHeartbeatEvent)
            {
                WebHeartbeatEvent event2 = eventRaised as WebHeartbeatEvent;
                WebProcessStatistics processStatistics = event2.ProcessStatistics;
                wmiData.processStartTime = this.WmiFormatTime(processStatistics.ProcessStartTime);
                wmiData.threadCount = processStatistics.ThreadCount;
                wmiData.workingSet = processStatistics.WorkingSet.ToString(CultureInfo.InstalledUICulture);
                wmiData.peakWorkingSet = processStatistics.PeakWorkingSet.ToString(CultureInfo.InstalledUICulture);
                wmiData.managedHeapSize = processStatistics.ManagedHeapSize.ToString(CultureInfo.InstalledUICulture);
                wmiData.appdomainCount = processStatistics.AppDomainCount;
                wmiData.requestsExecuting = processStatistics.RequestsExecuting;
                wmiData.requestsQueued = processStatistics.RequestsQueued;
                wmiData.requestsRejected = processStatistics.RequestsRejected;
            }
            if (eventRaised is WebBaseErrorEvent)
            {
                Exception errorException = ((WebBaseErrorEvent) eventRaised).ErrorException;
                if (errorException == null)
                {
                    wmiData.exceptionType = string.Empty;
                    wmiData.exceptionMessage = string.Empty;
                }
                else
                {
                    wmiData.exceptionType = errorException.GetType().Name;
                    wmiData.exceptionMessage = errorException.Message;
                }
            }
            if (eventRaised is WebRequestErrorEvent)
            {
                WebRequestErrorEvent event3 = eventRaised as WebRequestErrorEvent;
                WebRequestInformation requestInformation = event3.RequestInformation;
                WebThreadInformation threadInformation = event3.ThreadInformation;
                this.FillRequestWmiDataFields(ref wmiData, requestInformation);
                this.FillErrorWmiDataFields(ref wmiData, threadInformation);
            }
            if (eventRaised is WebErrorEvent)
            {
                WebErrorEvent event4 = eventRaised as WebErrorEvent;
                WebRequestInformation reqInfo = event4.RequestInformation;
                WebThreadInformation threadInfo = event4.ThreadInformation;
                this.FillRequestWmiDataFields(ref wmiData, reqInfo);
                this.FillErrorWmiDataFields(ref wmiData, threadInfo);
            }
            int num = UnsafeNativeMethods.RaiseWmiEvent(ref wmiData, AspCompatApplicationStep.IsInAspCompatMode);
            if (num != 0)
            {
                throw new HttpException(System.Web.SR.GetString("Wmi_provider_error", new object[] { "0x" + num.ToString("X8", CultureInfo.InstalledUICulture) }));
            }
        }

        public override void Shutdown()
        {
        }

        private string WmiFormatTime(DateTime dt)
        {
            StringBuilder builder = new StringBuilder(0x1a);
            builder.Append(dt.ToString("yyyyMMddHHmmss.ffffff", CultureInfo.InstalledUICulture));
            double totalMinutes = TimeZone.CurrentTimeZone.GetUtcOffset(dt).TotalMinutes;
            if (totalMinutes >= 0.0)
            {
                builder.Append('+');
            }
            builder.Append(totalMinutes);
            return builder.ToString();
        }
    }
}

