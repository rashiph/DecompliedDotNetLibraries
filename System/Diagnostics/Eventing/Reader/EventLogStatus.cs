namespace System.Diagnostics.Eventing.Reader
{
    using System;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class EventLogStatus
    {
        private string channelName;
        private int win32ErrorCode;

        internal EventLogStatus(string channelName, int win32ErrorCode)
        {
            this.channelName = channelName;
            this.win32ErrorCode = win32ErrorCode;
        }

        public string LogName
        {
            get
            {
                return this.channelName;
            }
        }

        public int StatusCode
        {
            get
            {
                return this.win32ErrorCode;
            }
        }
    }
}

