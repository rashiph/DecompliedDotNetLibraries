namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Reflection;
    using System.Xml;

    public class DsmlRequestDocument : DsmlDocument, IList, ICollection, IEnumerable
    {
        private DsmlDocumentProcessing docProcessing;
        private ArrayList dsmlRequests;
        private DsmlErrorProcessing errProcessing = DsmlErrorProcessing.Exit;
        private DsmlResponseOrder resOrder;

        public DsmlRequestDocument()
        {
            Utility.CheckOSVersion();
            this.dsmlRequests = new ArrayList();
        }

        public int Add(DirectoryRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            return this.dsmlRequests.Add(request);
        }

        public void Clear()
        {
            this.dsmlRequests.Clear();
        }

        public bool Contains(DirectoryRequest value)
        {
            return this.dsmlRequests.Contains(value);
        }

        public void CopyTo(DirectoryRequest[] value, int i)
        {
            this.dsmlRequests.CopyTo(value, i);
        }

        public IEnumerator GetEnumerator()
        {
            return this.dsmlRequests.GetEnumerator();
        }

        public int IndexOf(DirectoryRequest value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return this.dsmlRequests.IndexOf(value);
        }

        public void Insert(int index, DirectoryRequest value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            this.dsmlRequests.Insert(index, value);
        }

        public void Remove(DirectoryRequest value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            this.dsmlRequests.Remove(value);
        }

        public void RemoveAt(int index)
        {
            this.dsmlRequests.RemoveAt(index);
        }

        private void StartBatchRequest(XmlDocument xmldoc)
        {
            string xml = "<batchRequest xmlns=\"urn:oasis:names:tc:DSML:2:0:core\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" />";
            xmldoc.LoadXml(xml);
            XmlAttribute node = xmldoc.CreateAttribute("processing", null);
            switch (this.docProcessing)
            {
                case DsmlDocumentProcessing.Sequential:
                    node.InnerText = "sequential";
                    break;

                case DsmlDocumentProcessing.Parallel:
                    node.InnerText = "parallel";
                    break;
            }
            xmldoc.DocumentElement.Attributes.Append(node);
            node = xmldoc.CreateAttribute("responseOrder", null);
            switch (this.resOrder)
            {
                case DsmlResponseOrder.Sequential:
                    node.InnerText = "sequential";
                    break;

                case DsmlResponseOrder.Unordered:
                    node.InnerText = "unordered";
                    break;
            }
            xmldoc.DocumentElement.Attributes.Append(node);
            node = xmldoc.CreateAttribute("onError", null);
            switch (this.errProcessing)
            {
                case DsmlErrorProcessing.Resume:
                    node.InnerText = "resume";
                    break;

                case DsmlErrorProcessing.Exit:
                    node.InnerText = "exit";
                    break;
            }
            xmldoc.DocumentElement.Attributes.Append(node);
            if (base.dsmlRequestID != null)
            {
                node = xmldoc.CreateAttribute("requestID", null);
                node.InnerText = base.dsmlRequestID;
                xmldoc.DocumentElement.Attributes.Append(node);
            }
        }

        void ICollection.CopyTo(Array value, int i)
        {
            this.dsmlRequests.CopyTo(value, i);
        }

        int IList.Add(object request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            if (!(request is DirectoryRequest))
            {
                throw new ArgumentException(System.DirectoryServices.Protocols.Res.GetString("InvalidValueType", new object[] { "DirectoryRequest" }), "request");
            }
            return this.Add((DirectoryRequest) request);
        }

        void IList.Clear()
        {
            this.Clear();
        }

        bool IList.Contains(object value)
        {
            return this.Contains((DirectoryRequest) value);
        }

        int IList.IndexOf(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return this.IndexOf((DirectoryRequest) value);
        }

        void IList.Insert(int index, object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (!(value is DirectoryRequest))
            {
                throw new ArgumentException(System.DirectoryServices.Protocols.Res.GetString("InvalidValueType", new object[] { "DirectoryRequest" }), "value");
            }
            this.Insert(index, (DirectoryRequest) value);
        }

        void IList.Remove(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            this.Remove((DirectoryRequest) value);
        }

        void IList.RemoveAt(int index)
        {
            this.RemoveAt(index);
        }

        public override XmlDocument ToXml()
        {
            XmlDocument xmldoc = new XmlDocument();
            this.StartBatchRequest(xmldoc);
            foreach (DirectoryRequest request in this.dsmlRequests)
            {
                xmldoc.DocumentElement.AppendChild(request.ToXmlNodeHelper(xmldoc));
            }
            return xmldoc;
        }

        public int Count
        {
            get
            {
                return this.dsmlRequests.Count;
            }
        }

        public DsmlDocumentProcessing DocumentProcessing
        {
            get
            {
                return this.docProcessing;
            }
            set
            {
                if ((value < DsmlDocumentProcessing.Sequential) || (value > DsmlDocumentProcessing.Parallel))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(DsmlDocumentProcessing));
                }
                this.docProcessing = value;
            }
        }

        public DsmlErrorProcessing ErrorProcessing
        {
            get
            {
                return this.errProcessing;
            }
            set
            {
                if ((value < DsmlErrorProcessing.Resume) || (value > DsmlErrorProcessing.Exit))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(DsmlErrorProcessing));
                }
                this.errProcessing = value;
            }
        }

        protected bool IsFixedSize
        {
            get
            {
                return false;
            }
        }

        protected bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        protected bool IsSynchronized
        {
            get
            {
                return this.dsmlRequests.IsSynchronized;
            }
        }

        public DirectoryRequest this[int index]
        {
            get
            {
                return (DirectoryRequest) this.dsmlRequests[index];
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.dsmlRequests[index] = value;
            }
        }

        public string RequestId
        {
            get
            {
                return base.dsmlRequestID;
            }
            set
            {
                base.dsmlRequestID = value;
            }
        }

        public DsmlResponseOrder ResponseOrder
        {
            get
            {
                return this.resOrder;
            }
            set
            {
                if ((value < DsmlResponseOrder.Sequential) || (value > DsmlResponseOrder.Unordered))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(DsmlResponseOrder));
                }
                this.resOrder = value;
            }
        }

        protected object SyncRoot
        {
            get
            {
                return this.dsmlRequests.SyncRoot;
            }
        }

        int ICollection.Count
        {
            get
            {
                return this.dsmlRequests.Count;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return this.dsmlRequests.IsSynchronized;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return this.dsmlRequests.SyncRoot;
            }
        }

        bool IList.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        bool IList.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (!(value is DirectoryRequest))
                {
                    throw new ArgumentException(System.DirectoryServices.Protocols.Res.GetString("InvalidValueType", new object[] { "DirectoryRequest" }), "value");
                }
                this.dsmlRequests[index] = (DirectoryRequest) value;
            }
        }
    }
}

