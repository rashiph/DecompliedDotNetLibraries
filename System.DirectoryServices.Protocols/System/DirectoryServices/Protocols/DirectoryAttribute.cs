namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Text;
    using System.Xml;

    public class DirectoryAttribute : CollectionBase
    {
        private string attributeName;
        private static UTF8Encoding encoder = new UTF8Encoding();
        internal bool isSearchResult;
        private static UTF8Encoding utf8EncoderWithErrorDetection = new UTF8Encoding(false, true);

        public DirectoryAttribute()
        {
            this.attributeName = "";
            Utility.CheckOSVersion();
        }

        internal DirectoryAttribute(XmlElement node)
        {
            this.attributeName = "";
            string xpath = "@dsml:name";
            string str2 = "@name";
            XmlNamespaceManager dsmlNamespaceManager = NamespaceUtils.GetDsmlNamespaceManager();
            XmlAttribute attribute = (XmlAttribute) node.SelectSingleNode(xpath, dsmlNamespaceManager);
            if (attribute == null)
            {
                attribute = (XmlAttribute) node.SelectSingleNode(str2, dsmlNamespaceManager);
                if (attribute == null)
                {
                    throw new DsmlInvalidDocumentException(System.DirectoryServices.Protocols.Res.GetString("MissingSearchResultEntryAttributeName"));
                }
                this.attributeName = attribute.Value;
            }
            else
            {
                this.attributeName = attribute.Value;
            }
            XmlNodeList list = node.SelectNodes("dsml:value", dsmlNamespaceManager);
            if (list.Count != 0)
            {
                foreach (XmlNode node2 in list)
                {
                    XmlAttribute attribute2 = (XmlAttribute) node2.SelectSingleNode("@xsi:type", dsmlNamespaceManager);
                    if (attribute2 == null)
                    {
                        this.Add(node2.InnerText);
                    }
                    else if (string.Compare(attribute2.Value, "xsd:string", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        this.Add(node2.InnerText);
                    }
                    else if (string.Compare(attribute2.Value, "xsd:base64Binary", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        byte[] buffer;
                        string innerText = node2.InnerText;
                        try
                        {
                            buffer = Convert.FromBase64String(innerText);
                        }
                        catch (FormatException)
                        {
                            throw new DsmlInvalidDocumentException(System.DirectoryServices.Protocols.Res.GetString("BadBase64Value"));
                        }
                        this.Add(buffer);
                    }
                    else if (string.Compare(attribute2.Value, "xsd:anyURI", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Uri uri = new Uri(node2.InnerText);
                        this.Add(uri);
                    }
                }
            }
        }

        public DirectoryAttribute(string name, byte[] value) : this(name, value)
        {
        }

        internal DirectoryAttribute(string name, object value) : this()
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            this.Name = name;
            this.Add(value);
        }

        public DirectoryAttribute(string name, string value) : this(name, value)
        {
        }

        public DirectoryAttribute(string name, Uri value) : this(name, value)
        {
        }

        public DirectoryAttribute(string name, params object[] values) : this()
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }
            this.Name = name;
            for (int i = 0; i < values.Length; i++)
            {
                this.Add(values[i]);
            }
        }

        public int Add(byte[] value)
        {
            return this.Add(value);
        }

        internal int Add(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if ((!(value is string) && !(value is byte[])) && !(value is Uri))
            {
                throw new ArgumentException(System.DirectoryServices.Protocols.Res.GetString("ValidValueType"), "value");
            }
            return base.List.Add(value);
        }

        public int Add(string value)
        {
            return this.Add(value);
        }

        public int Add(Uri value)
        {
            return this.Add(value);
        }

        public void AddRange(object[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }
            if ((!(values is string[]) && !(values is byte[][])) && !(values is Uri[]))
            {
                throw new ArgumentException(System.DirectoryServices.Protocols.Res.GetString("ValidValuesType"), "values");
            }
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == null)
                {
                    throw new ArgumentException(System.DirectoryServices.Protocols.Res.GetString("NullValueArray"), "values");
                }
            }
            base.InnerList.AddRange(values);
        }

        public bool Contains(object value)
        {
            return base.List.Contains(value);
        }

        public void CopyTo(object[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public object[] GetValues(Type valuesType)
        {
            if (valuesType == typeof(byte[]))
            {
                int num = base.List.Count;
                byte[][] bufferArray = new byte[num][];
                for (int j = 0; j < num; j++)
                {
                    if (base.List[j] is string)
                    {
                        bufferArray[j] = encoder.GetBytes((string) base.List[j]);
                    }
                    else
                    {
                        if (!(base.List[j] is byte[]))
                        {
                            throw new NotSupportedException(System.DirectoryServices.Protocols.Res.GetString("DirectoryAttributeConversion"));
                        }
                        bufferArray[j] = (byte[]) base.List[j];
                    }
                }
                return bufferArray;
            }
            if (!(valuesType == typeof(string)))
            {
                throw new ArgumentException(System.DirectoryServices.Protocols.Res.GetString("ValidDirectoryAttributeType"), "valuesType");
            }
            int count = base.List.Count;
            string[] strArray = new string[count];
            for (int i = 0; i < count; i++)
            {
                if (base.List[i] is string)
                {
                    strArray[i] = (string) base.List[i];
                }
                else
                {
                    if (!(base.List[i] is byte[]))
                    {
                        throw new NotSupportedException(System.DirectoryServices.Protocols.Res.GetString("DirectoryAttributeConversion"));
                    }
                    strArray[i] = encoder.GetString((byte[]) base.List[i]);
                }
            }
            return strArray;
        }

        public int IndexOf(object value)
        {
            return base.List.IndexOf(value);
        }

        public void Insert(int index, byte[] value)
        {
            this.Insert(index, value);
        }

        private void Insert(int index, object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            base.List.Insert(index, value);
        }

        public void Insert(int index, string value)
        {
            this.Insert(index, value);
        }

        public void Insert(int index, Uri value)
        {
            this.Insert(index, value);
        }

        protected override void OnValidate(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if ((!(value is string) && !(value is byte[])) && !(value is Uri))
            {
                throw new ArgumentException(System.DirectoryServices.Protocols.Res.GetString("ValidValueType"), "value");
            }
        }

        public void Remove(object value)
        {
            base.List.Remove(value);
        }

        internal XmlElement ToXmlNode(XmlDocument doc, string elementName)
        {
            XmlElement elemBase = doc.CreateElement(elementName, "urn:oasis:names:tc:DSML:2:0:core");
            this.ToXmlNodeCommon(elemBase);
            return elemBase;
        }

        internal void ToXmlNodeCommon(XmlElement elemBase)
        {
            XmlDocument ownerDocument = elemBase.OwnerDocument;
            XmlAttribute node = ownerDocument.CreateAttribute("name", null);
            node.InnerText = this.Name;
            elemBase.Attributes.Append(node);
            if (base.Count != 0)
            {
                foreach (object obj2 in base.InnerList)
                {
                    XmlElement newChild = ownerDocument.CreateElement("value", "urn:oasis:names:tc:DSML:2:0:core");
                    if (obj2 is byte[])
                    {
                        newChild.InnerText = Convert.ToBase64String((byte[]) obj2);
                        XmlAttribute attribute2 = ownerDocument.CreateAttribute("xsi:type", "http://www.w3.org/2001/XMLSchema-instance");
                        attribute2.InnerText = "xsd:base64Binary";
                        newChild.Attributes.Append(attribute2);
                    }
                    else if (obj2 is Uri)
                    {
                        newChild.InnerText = obj2.ToString();
                        XmlAttribute attribute3 = ownerDocument.CreateAttribute("xsi:type", "http://www.w3.org/2001/XMLSchema-instance");
                        attribute3.InnerText = "xsd:anyURI";
                        newChild.Attributes.Append(attribute3);
                    }
                    else
                    {
                        newChild.InnerText = obj2.ToString();
                        if (newChild.InnerText.StartsWith(" ", StringComparison.Ordinal) || newChild.InnerText.EndsWith(" ", StringComparison.Ordinal))
                        {
                            XmlAttribute attribute4 = ownerDocument.CreateAttribute("xml:space");
                            attribute4.InnerText = "preserve";
                            newChild.Attributes.Append(attribute4);
                        }
                    }
                    elemBase.AppendChild(newChild);
                }
            }
        }

        public object this[int index]
        {
            get
            {
                if (this.isSearchResult)
                {
                    byte[] bytes = base.List[index] as byte[];
                    if (bytes != null)
                    {
                        try
                        {
                            return utf8EncoderWithErrorDetection.GetString(bytes);
                        }
                        catch (ArgumentException)
                        {
                            return base.List[index];
                        }
                    }
                }
                return base.List[index];
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if ((!(value is string) && !(value is byte[])) && !(value is Uri))
                {
                    throw new ArgumentException(System.DirectoryServices.Protocols.Res.GetString("ValidValueType"), "value");
                }
                base.List[index] = value;
            }
        }

        public string Name
        {
            get
            {
                return this.attributeName;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.attributeName = value;
            }
        }
    }
}

