namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.Xml;

    internal class WmiPutTraceRecord : TraceRecord
    {
        private string newValue;
        private string originalValue;
        private string valueName;

        internal WmiPutTraceRecord(string valueName, object originalValue, object newValue)
        {
            this.valueName = valueName;
            this.originalValue = (originalValue == null) ? System.ServiceModel.SR.GetString("ConfigNull") : originalValue.ToString();
            this.newValue = (newValue == null) ? System.ServiceModel.SR.GetString("ConfigNull") : newValue.ToString();
        }

        internal override void WriteTo(XmlWriter xml)
        {
            xml.WriteElementString("ValueName", this.valueName);
            xml.WriteElementString("OriginalValue", this.originalValue);
            xml.WriteElementString("NewValue", this.newValue);
        }

        internal override string EventId
        {
            get
            {
                return base.BuildEventId("WmiPut");
            }
        }
    }
}

