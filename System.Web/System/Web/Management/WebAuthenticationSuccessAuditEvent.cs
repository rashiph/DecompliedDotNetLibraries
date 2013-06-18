namespace System.Web.Management
{
    using System;
    using System.Collections.Generic;

    public class WebAuthenticationSuccessAuditEvent : WebSuccessAuditEvent
    {
        private string _nameToAuthenticate;

        internal WebAuthenticationSuccessAuditEvent()
        {
        }

        protected internal WebAuthenticationSuccessAuditEvent(string message, object eventSource, int eventCode, string nameToAuthenticate) : base(message, eventSource, eventCode)
        {
            this.Init(nameToAuthenticate);
        }

        protected internal WebAuthenticationSuccessAuditEvent(string message, object eventSource, int eventCode, int eventDetailCode, string nameToAuthenticate) : base(message, eventSource, eventCode, eventDetailCode)
        {
            this.Init(nameToAuthenticate);
        }

        internal override void FormatToString(WebEventFormatter formatter, bool includeAppInfo)
        {
            base.FormatToString(formatter, includeAppInfo);
            formatter.AppendLine(string.Empty);
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache("Webevent_event_name_to_authenticate", this._nameToAuthenticate));
        }

        internal override void GenerateFieldsForMarshal(List<WebEventFieldData> fields)
        {
            base.GenerateFieldsForMarshal(fields);
            fields.Add(new WebEventFieldData("NameToAuthenticate", this.NameToAuthenticate, WebEventFieldType.String));
        }

        private void Init(string name)
        {
            this._nameToAuthenticate = name;
        }

        public string NameToAuthenticate
        {
            get
            {
                return this._nameToAuthenticate;
            }
        }
    }
}

