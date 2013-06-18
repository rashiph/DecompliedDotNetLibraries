namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ServiceModel;
    using System.Text;
    using System.Xml;

    public sealed class AddressHeaderCollection : ReadOnlyCollection<AddressHeader>
    {
        private static AddressHeaderCollection emptyHeaderCollection = new AddressHeaderCollection();

        public AddressHeaderCollection() : base(new List<AddressHeader>())
        {
        }

        public AddressHeaderCollection(IEnumerable<AddressHeader> addressHeaders) : base(new List<AddressHeader>(addressHeaders))
        {
            IList<AddressHeader> list = addressHeaders as IList<AddressHeader>;
            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("MessageHeaderIsNull0")));
                    }
                }
            }
            else
            {
                using (IEnumerator<AddressHeader> enumerator = addressHeaders.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        AddressHeader current = enumerator.Current;
                        if (addressHeaders == null)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("MessageHeaderIsNull0")));
                        }
                    }
                }
            }
        }

        public void AddHeadersTo(Message message)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            for (int i = 0; i < this.InternalCount; i++)
            {
                message.Headers.Add(base[i].ToMessageHeader());
            }
        }

        public AddressHeader[] FindAll(string name, string ns)
        {
            if (name == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("name"));
            }
            if (ns == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("ns"));
            }
            List<AddressHeader> list = new List<AddressHeader>();
            for (int i = 0; i < base.Count; i++)
            {
                AddressHeader item = base[i];
                if ((item.Name == name) && (item.Namespace == ns))
                {
                    list.Add(item);
                }
            }
            return list.ToArray();
        }

        public AddressHeader FindHeader(string name, string ns)
        {
            if (name == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("name"));
            }
            if (ns == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("ns"));
            }
            AddressHeader header = null;
            for (int i = 0; i < base.Count; i++)
            {
                AddressHeader header2 = base[i];
                if ((header2.Name == name) && (header2.Namespace == ns))
                {
                    if (header != null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("MultipleMessageHeaders", new object[] { name, ns })));
                    }
                    header = header2;
                }
            }
            return header;
        }

        internal bool IsEquivalent(AddressHeaderCollection col)
        {
            if (this.InternalCount != col.InternalCount)
            {
                return false;
            }
            StringBuilder builder = new StringBuilder();
            Dictionary<string, int> headers = new Dictionary<string, int>();
            this.PopulateHeaderDictionary(builder, headers);
            Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
            col.PopulateHeaderDictionary(builder, dictionary2);
            if (headers.Count != dictionary2.Count)
            {
                return false;
            }
            foreach (KeyValuePair<string, int> pair in headers)
            {
                int num;
                if (dictionary2.TryGetValue(pair.Key, out num))
                {
                    if (num != pair.Value)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        internal void PopulateHeaderDictionary(StringBuilder builder, Dictionary<string, int> headers)
        {
            for (int i = 0; i < this.InternalCount; i++)
            {
                builder.Remove(0, builder.Length);
                string comparableForm = base[i].GetComparableForm(builder);
                if (headers.ContainsKey(comparableForm))
                {
                    headers[comparableForm] += 1;
                }
                else
                {
                    headers.Add(comparableForm, 1);
                }
            }
        }

        internal static AddressHeaderCollection ReadServiceParameters(XmlDictionaryReader reader)
        {
            return ReadServiceParameters(reader, false);
        }

        internal static AddressHeaderCollection ReadServiceParameters(XmlDictionaryReader reader, bool isReferenceProperty)
        {
            reader.MoveToContent();
            if (reader.IsEmptyElement)
            {
                reader.Skip();
                return null;
            }
            reader.ReadStartElement();
            List<AddressHeader> addressHeaders = new List<AddressHeader>();
            while (reader.IsStartElement())
            {
                addressHeaders.Add(new BufferedAddressHeader(reader, isReferenceProperty));
            }
            reader.ReadEndElement();
            return new AddressHeaderCollection(addressHeaders);
        }

        internal void WriteContentsTo(XmlDictionaryWriter writer)
        {
            for (int i = 0; i < this.InternalCount; i++)
            {
                base[i].WriteAddressHeader(writer);
            }
        }

        internal void WriteNonReferencePropertyContentsTo(XmlDictionaryWriter writer)
        {
            for (int i = 0; i < this.InternalCount; i++)
            {
                if (!base[i].IsReferenceProperty)
                {
                    base[i].WriteAddressHeader(writer);
                }
            }
        }

        internal void WriteReferencePropertyContentsTo(XmlDictionaryWriter writer)
        {
            for (int i = 0; i < this.InternalCount; i++)
            {
                if (base[i].IsReferenceProperty)
                {
                    base[i].WriteAddressHeader(writer);
                }
            }
        }

        internal static AddressHeaderCollection EmptyHeaderCollection
        {
            get
            {
                return emptyHeaderCollection;
            }
        }

        internal bool HasNonReferenceProperties
        {
            get
            {
                for (int i = 0; i < this.InternalCount; i++)
                {
                    if (!base[i].IsReferenceProperty)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        internal bool HasReferenceProperties
        {
            get
            {
                for (int i = 0; i < this.InternalCount; i++)
                {
                    if (base[i].IsReferenceProperty)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        private int InternalCount
        {
            get
            {
                if (this == emptyHeaderCollection)
                {
                    return 0;
                }
                return base.Count;
            }
        }
    }
}

