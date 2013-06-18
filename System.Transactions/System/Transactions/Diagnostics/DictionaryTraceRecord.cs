namespace System.Transactions.Diagnostics
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Text;
    using System.Xml;

    internal class DictionaryTraceRecord : TraceRecord
    {
        private IDictionary dictionary;

        internal DictionaryTraceRecord(IDictionary dictionary)
        {
            this.dictionary = dictionary;
        }

        public override string ToString()
        {
            if (this.dictionary != null)
            {
                StringBuilder builder = new StringBuilder();
                foreach (object obj2 in this.dictionary.Keys)
                {
                    builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}: {1}", new object[] { obj2, this.dictionary[obj2].ToString() }));
                }
            }
            return null;
        }

        internal override void WriteTo(XmlWriter xml)
        {
            if (this.dictionary != null)
            {
                foreach (object obj2 in this.dictionary.Keys)
                {
                    xml.WriteElementString(obj2.ToString(), this.dictionary[obj2].ToString());
                }
            }
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2004/03/Transactions/DictionaryTraceRecord";
            }
        }
    }
}

