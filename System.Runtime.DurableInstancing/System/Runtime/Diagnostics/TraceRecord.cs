namespace System.Runtime.Diagnostics
{
    using System;
    using System.Runtime;
    using System.Xml;

    [Serializable]
    internal class TraceRecord
    {
        protected const string EventIdBase = "http://schemas.microsoft.com/2006/08/ServiceModel/";
        protected const string NamespaceSuffix = "TraceRecord";

        protected string BuildEventId(string eventId)
        {
            return ("http://schemas.microsoft.com/2006/08/ServiceModel/" + eventId + "TraceRecord");
        }

        internal virtual void WriteTo(XmlWriter writer)
        {
        }

        protected string XmlEncode(string text)
        {
            return DiagnosticTrace.XmlEncode(text);
        }

        internal virtual string EventId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.BuildEventId("Empty");
            }
        }
    }
}

