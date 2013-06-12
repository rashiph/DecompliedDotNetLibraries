namespace System.Web.Management
{
    using System;
    using System.Diagnostics;
    using System.Globalization;

    public sealed class WebThreadInformation
    {
        private string _accountName = HttpApplication.GetCurrentWindowsIdentityWithAssert().Name;
        private bool _isImpersonating;
        private string _stackTrace;
        private int _threadId = Thread.CurrentThread.ManagedThreadId;
        internal const string IsImpersonatingKey = "ASPIMPERSONATING";

        internal WebThreadInformation(Exception exception)
        {
            if (exception != null)
            {
                this._stackTrace = new System.Diagnostics.StackTrace(exception, true).ToString();
                this._isImpersonating = exception.Data.Contains("ASPIMPERSONATING");
            }
            else
            {
                this._stackTrace = string.Empty;
                this._isImpersonating = false;
            }
        }

        public void FormatToString(WebEventFormatter formatter)
        {
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache("Webevent_event_thread_id", this.ThreadID.ToString(CultureInfo.InstalledUICulture)));
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache("Webevent_event_thread_account_name", this.ThreadAccountName));
            if (this.IsImpersonating)
            {
                formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache("Webevent_event_is_impersonating"));
            }
            else
            {
                formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache("Webevent_event_is_not_impersonating"));
            }
            formatter.AppendLine(WebBaseEvent.FormatResourceStringWithCache("Webevent_event_stack_trace", this.StackTrace));
        }

        public bool IsImpersonating
        {
            get
            {
                return this._isImpersonating;
            }
        }

        public string StackTrace
        {
            get
            {
                return this._stackTrace;
            }
        }

        public string ThreadAccountName
        {
            get
            {
                return this._accountName;
            }
        }

        public int ThreadID
        {
            get
            {
                return this._threadId;
            }
        }
    }
}

