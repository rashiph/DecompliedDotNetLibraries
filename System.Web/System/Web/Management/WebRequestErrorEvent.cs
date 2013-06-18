namespace System.Web.Management
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Web;

    public class WebRequestErrorEvent : WebBaseErrorEvent
    {
        private WebRequestInformation _requestInfo;
        private WebThreadInformation _threadInfo;

        internal WebRequestErrorEvent()
        {
        }

        protected internal WebRequestErrorEvent(string message, object eventSource, int eventCode, Exception exception) : base(message, eventSource, eventCode, exception)
        {
            this.Init(exception);
        }

        protected internal WebRequestErrorEvent(string message, object eventSource, int eventCode, int eventDetailCode, Exception exception) : base(message, eventSource, eventCode, eventDetailCode, exception)
        {
            this.Init(exception);
        }

        internal override void FormatToString(WebEventFormatter formatter, bool includeAppInfo)
        {
            base.FormatToString(formatter, includeAppInfo);
            formatter.AppendLine(string.Empty);
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache("Webevent_event_request_information"));
            formatter.IndentationLevel++;
            this.RequestInformation.FormatToString(formatter);
            formatter.IndentationLevel--;
            formatter.AppendLine(string.Empty);
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache("Webevent_event_thread_information"));
            formatter.IndentationLevel++;
            this.ThreadInformation.FormatToString(formatter);
            formatter.IndentationLevel--;
        }

        internal override void GenerateFieldsForMarshal(List<WebEventFieldData> fields)
        {
            base.GenerateFieldsForMarshal(fields);
            fields.Add(new WebEventFieldData("RequestUrl", this.RequestInformation.RequestUrl, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("RequestPath", this.RequestInformation.RequestPath, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("UserHostAddress", this.RequestInformation.UserHostAddress, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("UserName", this.RequestInformation.Principal.Identity.Name, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("UserAuthenticated", this.RequestInformation.Principal.Identity.IsAuthenticated.ToString(), WebEventFieldType.Bool));
            fields.Add(new WebEventFieldData("UserAuthenticationType", this.RequestInformation.Principal.Identity.AuthenticationType, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("RequestThreadAccountName", this.RequestInformation.ThreadAccountName, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("ThreadID", this.ThreadInformation.ThreadID.ToString(CultureInfo.InstalledUICulture), WebEventFieldType.Int));
            fields.Add(new WebEventFieldData("ThreadAccountName", this.ThreadInformation.ThreadAccountName, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("StackTrace", this.ThreadInformation.StackTrace, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("IsImpersonating", this.ThreadInformation.IsImpersonating.ToString(), WebEventFieldType.Bool));
        }

        protected internal override void IncrementPerfCounters()
        {
            base.IncrementPerfCounters();
            PerfCounters.IncrementCounter(AppPerfCounter.EVENTS_HTTP_REQ_ERROR);
            PerfCounters.IncrementGlobalCounter(GlobalPerfCounter.GLOBAL_EVENTS_HTTP_REQ_ERROR);
        }

        private void Init(Exception e)
        {
        }

        private void InitRequestInformation()
        {
            if (this._requestInfo == null)
            {
                this._requestInfo = new WebRequestInformation();
            }
        }

        private void InitThreadInformation()
        {
            if (this._threadInfo == null)
            {
                this._threadInfo = new WebThreadInformation(base.ErrorException);
            }
        }

        internal override void PreProcessEventInit()
        {
            base.PreProcessEventInit();
            this.InitRequestInformation();
            this.InitThreadInformation();
        }

        public WebRequestInformation RequestInformation
        {
            get
            {
                this.InitRequestInformation();
                return this._requestInfo;
            }
        }

        public WebThreadInformation ThreadInformation
        {
            get
            {
                this.InitThreadInformation();
                return this._threadInfo;
            }
        }
    }
}

