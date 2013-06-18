namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Collections;
    using System.Runtime.Diagnostics;
    using System.Xml;

    internal class CollectionTraceRecord : TraceRecord
    {
        private string collectionName;
        private string elementName;
        private IEnumerable entries;

        public CollectionTraceRecord(string collectionName, string elementName, IEnumerable entries)
        {
            this.collectionName = string.IsNullOrEmpty(collectionName) ? "Elements" : collectionName;
            this.elementName = string.IsNullOrEmpty(elementName) ? "Element" : elementName;
            this.entries = entries;
        }

        internal override void WriteTo(XmlWriter xml)
        {
            if (this.entries != null)
            {
                xml.WriteStartElement(this.collectionName);
                foreach (object obj2 in this.entries)
                {
                    xml.WriteElementString(this.elementName, (obj2 == null) ? "null" : obj2.ToString());
                }
                xml.WriteEndElement();
            }
        }

        internal override string EventId
        {
            get
            {
                return base.BuildEventId("Collection");
            }
        }
    }
}

