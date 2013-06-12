namespace System.Web.Management
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.InheritanceDemand, Unrestricted=true)]
    public class WebManagementEvent : WebBaseEvent
    {
        private static WebProcessInformation s_processInfo = new WebProcessInformation();

        internal WebManagementEvent()
        {
        }

        protected internal WebManagementEvent(string message, object eventSource, int eventCode) : base(message, eventSource, eventCode)
        {
        }

        protected internal WebManagementEvent(string message, object eventSource, int eventCode, int eventDetailCode) : base(message, eventSource, eventCode, eventDetailCode)
        {
        }

        internal override void FormatToString(WebEventFormatter formatter, bool includeAppInfo)
        {
            base.FormatToString(formatter, includeAppInfo);
            formatter.AppendLine(string.Empty);
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache("Webevent_event_process_information"));
            formatter.IndentationLevel++;
            this.ProcessInformation.FormatToString(formatter);
            formatter.IndentationLevel--;
        }

        internal override void GenerateFieldsForMarshal(List<WebEventFieldData> fields)
        {
            base.GenerateFieldsForMarshal(fields);
            fields.Add(new WebEventFieldData("AccountName", this.ProcessInformation.AccountName, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("ProcessName", this.ProcessInformation.ProcessName, WebEventFieldType.String));
            fields.Add(new WebEventFieldData("ProcessID", this.ProcessInformation.ProcessID.ToString(CultureInfo.InstalledUICulture), WebEventFieldType.Int));
        }

        public WebProcessInformation ProcessInformation
        {
            get
            {
                return s_processInfo;
            }
        }
    }
}

