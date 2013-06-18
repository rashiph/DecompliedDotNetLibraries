namespace System.Runtime.Diagnostics
{
    using System;
    using System.Runtime;
    using System.Xml;

    internal class StringTraceRecord : TraceRecord
    {
        private string content;
        private string elementName;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal StringTraceRecord(string elementName, string content)
        {
            this.elementName = elementName;
            this.content = content;
        }

        internal override void WriteTo(XmlWriter writer)
        {
            writer.WriteElementString(this.elementName, this.content);
        }

        internal override string EventId
        {
            get
            {
                return base.BuildEventId("String");
            }
        }
    }
}

