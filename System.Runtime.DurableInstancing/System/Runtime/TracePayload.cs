namespace System.Runtime
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct TracePayload
    {
        private string serializedException;
        private string eventSource;
        private string appDomainFriendlyName;
        private string extendedData;
        private string hostReference;
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public TracePayload(string serializedException, string eventSource, string appDomainFriendlyName, string extendedData, string hostReference)
        {
            this.serializedException = serializedException;
            this.eventSource = eventSource;
            this.appDomainFriendlyName = appDomainFriendlyName;
            this.extendedData = extendedData;
            this.hostReference = hostReference;
        }

        public string SerializedException
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.serializedException;
            }
        }
        public string EventSource
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.eventSource;
            }
        }
        public string AppDomainFriendlyName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.appDomainFriendlyName;
            }
        }
        public string ExtendedData
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.extendedData;
            }
        }
        public string HostReference
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.hostReference;
            }
        }
    }
}

