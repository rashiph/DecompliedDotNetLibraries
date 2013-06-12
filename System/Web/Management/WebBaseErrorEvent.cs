namespace System.Web.Management
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Web;

    public class WebBaseErrorEvent : WebManagementEvent
    {
        private Exception _exception;

        internal WebBaseErrorEvent()
        {
        }

        protected internal WebBaseErrorEvent(string message, object eventSource, int eventCode, Exception e) : base(message, eventSource, eventCode)
        {
            this.Init(e);
        }

        protected internal WebBaseErrorEvent(string message, object eventSource, int eventCode, int eventDetailCode, Exception e) : base(message, eventSource, eventCode, eventDetailCode)
        {
            this.Init(e);
        }

        internal override void FormatToString(WebEventFormatter formatter, bool includeAppInfo)
        {
            base.FormatToString(formatter, includeAppInfo);
            if (this._exception != null)
            {
                Exception innerException = this._exception;
                for (int i = 0; (innerException != null) && (i <= 2); i++)
                {
                    formatter.AppendLine(string.Empty);
                    if (i == 0)
                    {
                        formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache("Webevent_event_exception_information"));
                    }
                    else
                    {
                        formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache("Webevent_event_inner_exception_information", i.ToString(CultureInfo.InstalledUICulture)));
                    }
                    formatter.IndentationLevel++;
                    formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache("Webevent_event_exception_type", innerException.GetType().ToString()));
                    formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache("Webevent_event_exception_message", innerException.Message));
                    formatter.IndentationLevel--;
                    innerException = innerException.InnerException;
                }
            }
        }

        internal override void GenerateFieldsForMarshal(List<WebEventFieldData> fields)
        {
            base.GenerateFieldsForMarshal(fields);
            fields.Add(new WebEventFieldData("ExceptionType", this.ErrorException.GetType().ToString(), WebEventFieldType.String));
            fields.Add(new WebEventFieldData("ExceptionMessage", this.ErrorException.Message, WebEventFieldType.String));
        }

        protected internal override void IncrementPerfCounters()
        {
            base.IncrementPerfCounters();
            PerfCounters.IncrementCounter(AppPerfCounter.EVENTS_ERROR);
            PerfCounters.IncrementGlobalCounter(GlobalPerfCounter.GLOBAL_EVENTS_ERROR);
        }

        private void Init(Exception e)
        {
            this._exception = e;
        }

        public Exception ErrorException
        {
            get
            {
                return this._exception;
            }
        }
    }
}

