namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Text;
    using System.Xml;

    public class DsmlResponseDocument : DsmlDocument, ICollection, IEnumerable
    {
        private XmlElement dsmlBatchResponse;
        private XmlDocument dsmlDocument;
        private XmlNamespaceManager dsmlNS;
        private ArrayList dsmlResponse;

        private DsmlResponseDocument()
        {
            this.dsmlResponse = new ArrayList();
        }

        private DsmlResponseDocument(string responseString) : this(new StringBuilder(responseString), "se:Envelope/se:Body/dsml:batchResponse")
        {
        }

        internal DsmlResponseDocument(HttpWebResponse resp, string xpathToResponse) : this()
        {
            StreamReader txtReader = new StreamReader(resp.GetResponseStream());
            try
            {
                this.dsmlDocument = new XmlDocument();
                try
                {
                    this.dsmlDocument.Load(txtReader);
                }
                catch (XmlException)
                {
                    throw new DsmlInvalidDocumentException(System.DirectoryServices.Protocols.Res.GetString("NotWellFormedResponse"));
                }
                this.dsmlNS = NamespaceUtils.GetDsmlNamespaceManager();
                this.dsmlBatchResponse = (XmlElement) this.dsmlDocument.SelectSingleNode(xpathToResponse, this.dsmlNS);
                if (this.dsmlBatchResponse == null)
                {
                    throw new DsmlInvalidDocumentException(System.DirectoryServices.Protocols.Res.GetString("NotWellFormedResponse"));
                }
                foreach (XmlNode node in this.dsmlBatchResponse.ChildNodes)
                {
                    if (node.NodeType == XmlNodeType.Element)
                    {
                        DirectoryResponse response = this.ConstructElement((XmlElement) node);
                        this.dsmlResponse.Add(response);
                    }
                }
            }
            finally
            {
                txtReader.Close();
            }
        }

        internal DsmlResponseDocument(StringBuilder responseString, string xpathToResponse) : this()
        {
            this.dsmlDocument = new XmlDocument();
            try
            {
                this.dsmlDocument.LoadXml(responseString.ToString());
            }
            catch (XmlException)
            {
                throw new DsmlInvalidDocumentException(System.DirectoryServices.Protocols.Res.GetString("NotWellFormedResponse"));
            }
            this.dsmlNS = NamespaceUtils.GetDsmlNamespaceManager();
            this.dsmlBatchResponse = (XmlElement) this.dsmlDocument.SelectSingleNode(xpathToResponse, this.dsmlNS);
            if (this.dsmlBatchResponse == null)
            {
                throw new DsmlInvalidDocumentException(System.DirectoryServices.Protocols.Res.GetString("NotWellFormedResponse"));
            }
            foreach (XmlNode node in this.dsmlBatchResponse.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    DirectoryResponse response = this.ConstructElement((XmlElement) node);
                    this.dsmlResponse.Add(response);
                }
            }
        }

        private DirectoryResponse ConstructElement(XmlElement node)
        {
            switch (node.LocalName)
            {
                case "errorResponse":
                    return new DsmlErrorResponse(node);

                case "searchResponse":
                    return new SearchResponse(node);

                case "modifyResponse":
                    return new ModifyResponse(node);

                case "addResponse":
                    return new AddResponse(node);

                case "delResponse":
                    return new DeleteResponse(node);

                case "modDNResponse":
                    return new ModifyDNResponse(node);

                case "compareResponse":
                    return new CompareResponse(node);

                case "extendedResponse":
                    return new ExtendedResponse(node);

                case "authResponse":
                    return new DsmlAuthResponse(node);
            }
            throw new DsmlInvalidDocumentException(System.DirectoryServices.Protocols.Res.GetString("UnknownResponseElement"));
        }

        public void CopyTo(DirectoryResponse[] value, int i)
        {
            this.dsmlResponse.CopyTo(value, i);
        }

        public IEnumerator GetEnumerator()
        {
            return this.dsmlResponse.GetEnumerator();
        }

        void ICollection.CopyTo(Array value, int i)
        {
            this.dsmlResponse.CopyTo(value, i);
        }

        public override XmlDocument ToXml()
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml(this.dsmlBatchResponse.OuterXml);
            return document;
        }

        public int Count
        {
            get
            {
                return this.dsmlResponse.Count;
            }
        }

        public bool IsErrorResponse
        {
            get
            {
                foreach (DirectoryResponse response in this.dsmlResponse)
                {
                    if (response is DsmlErrorResponse)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool IsOperationError
        {
            get
            {
                foreach (DirectoryResponse response in this.dsmlResponse)
                {
                    if (!(response is DsmlErrorResponse))
                    {
                        ResultCode resultCode = response.ResultCode;
                        if ((((resultCode != ResultCode.Success) && (ResultCode.CompareTrue != resultCode)) && ((ResultCode.CompareFalse != resultCode) && (ResultCode.Referral != resultCode))) && (ResultCode.ReferralV2 != resultCode))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        protected bool IsSynchronized
        {
            get
            {
                return this.dsmlResponse.IsSynchronized;
            }
        }

        public DirectoryResponse this[int index]
        {
            get
            {
                return (DirectoryResponse) this.dsmlResponse[index];
            }
        }

        public string RequestId
        {
            get
            {
                XmlAttribute attribute = (XmlAttribute) this.dsmlBatchResponse.SelectSingleNode("@dsml:requestID", this.dsmlNS);
                if (attribute == null)
                {
                    attribute = (XmlAttribute) this.dsmlBatchResponse.SelectSingleNode("@requestID", this.dsmlNS);
                    if (attribute == null)
                    {
                        return null;
                    }
                }
                return attribute.Value;
            }
        }

        internal string ResponseString
        {
            get
            {
                if (this.dsmlDocument != null)
                {
                    return this.dsmlDocument.InnerXml;
                }
                return null;
            }
        }

        protected object SyncRoot
        {
            get
            {
                return this.dsmlResponse.SyncRoot;
            }
        }

        int ICollection.Count
        {
            get
            {
                return this.dsmlResponse.Count;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return this.dsmlResponse.IsSynchronized;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return this.dsmlResponse.SyncRoot;
            }
        }
    }
}

