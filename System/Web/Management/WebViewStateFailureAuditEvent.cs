namespace System.Web.Management
{
    using System;
    using System.Collections.Generic;
    using System.Web.UI;

    public class WebViewStateFailureAuditEvent : WebFailureAuditEvent
    {
        private System.Web.UI.ViewStateException _viewStateException;

        internal WebViewStateFailureAuditEvent()
        {
        }

        protected internal WebViewStateFailureAuditEvent(string message, object eventSource, int eventCode, System.Web.UI.ViewStateException viewStateException) : base(message, eventSource, eventCode)
        {
            this._viewStateException = viewStateException;
        }

        protected internal WebViewStateFailureAuditEvent(string message, object eventSource, int eventCode, int eventDetailCode, System.Web.UI.ViewStateException viewStateException) : base(message, eventSource, eventCode, eventDetailCode)
        {
            this._viewStateException = viewStateException;
        }

        internal override void FormatToString(WebEventFormatter formatter, bool includeAppInfo)
        {
            base.FormatToString(formatter, includeAppInfo);
            formatter.AppendLine(string.Empty);
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache("Webevent_event_ViewStateException_information"));
            formatter.IndentationLevel++;
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache("Webevent_event_exception_message", this._viewStateException.Message));
            formatter.IndentationLevel--;
        }

        internal override void GenerateFieldsForMarshal(List<WebEventFieldData> fields)
        {
            base.GenerateFieldsForMarshal(fields);
            fields.Add(new WebEventFieldData("ViewStateExceptionMessage", this.ViewStateException.Message, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("RemoteAddress", this.ViewStateException.RemoteAddress, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("RemotePort", this.ViewStateException.RemotePort, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("UserAgent", this.ViewStateException.UserAgent, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("PersistedState", this.ViewStateException.PersistedState, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("Path", this.ViewStateException.Path, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("Referer", this.ViewStateException.Referer, WebEventFieldType.String));
        }

        public System.Web.UI.ViewStateException ViewStateException
        {
            get
            {
                return this._viewStateException;
            }
        }
    }
}

