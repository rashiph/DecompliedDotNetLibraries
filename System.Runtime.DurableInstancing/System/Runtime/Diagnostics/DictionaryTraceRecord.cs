namespace System.Runtime.Diagnostics
{
    using System;
    using System.Collections;
    using System.Runtime;
    using System.Xml;

    internal class DictionaryTraceRecord : TraceRecord
    {
        private IDictionary dictionary;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal DictionaryTraceRecord(IDictionary dictionary)
        {
            this.dictionary = dictionary;
        }

        internal override void WriteTo(XmlWriter xml)
        {
            if (this.dictionary != null)
            {
                foreach (object obj2 in this.dictionary.Keys)
                {
                    object obj3 = this.dictionary[obj2];
                    xml.WriteElementString(obj2.ToString(), (obj3 == null) ? string.Empty : obj3.ToString());
                }
            }
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2006/08/ServiceModel/DictionaryTraceRecord";
            }
        }
    }
}

