namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Globalization;
    using System.Xml;

    public abstract class DirectoryResponse : DirectoryOperation
    {
        internal DirectoryControl[] directoryControls;
        internal string directoryMessage;
        internal Uri[] directoryReferral;
        internal string dn;
        internal XmlNode dsmlNode;
        internal XmlNamespaceManager dsmlNS;
        internal bool dsmlRequest;
        private string requestID;
        internal System.DirectoryServices.Protocols.ResultCode result;

        internal DirectoryResponse(XmlNode node)
        {
            this.result = ~System.DirectoryServices.Protocols.ResultCode.Success;
            this.dsmlNode = node;
            this.dsmlNS = NamespaceUtils.GetDsmlNamespaceManager();
            this.dsmlRequest = true;
        }

        internal DirectoryResponse(string dn, DirectoryControl[] controls, System.DirectoryServices.Protocols.ResultCode result, string message, Uri[] referral)
        {
            this.result = ~System.DirectoryServices.Protocols.ResultCode.Success;
            this.dn = dn;
            this.directoryControls = controls;
            this.result = result;
            this.directoryMessage = message;
            this.directoryReferral = referral;
        }

        internal DirectoryControl[] ControlsHelper(string primaryXPath)
        {
            XmlNodeList list = this.dsmlNode.SelectNodes(primaryXPath, this.dsmlNS);
            if (list.Count == 0)
            {
                return new DirectoryControl[0];
            }
            DirectoryControl[] controlArray = new DirectoryControl[list.Count];
            int index = 0;
            foreach (XmlNode node in list)
            {
                controlArray[index] = new DirectoryControl((XmlElement) node);
                index++;
            }
            return controlArray;
        }

        internal string ErrorMessageHelper(string primaryXPath)
        {
            XmlElement element = (XmlElement) this.dsmlNode.SelectSingleNode(primaryXPath, this.dsmlNS);
            if (element != null)
            {
                return element.InnerText;
            }
            return null;
        }

        internal string MatchedDNHelper(string primaryXPath, string secondaryXPath)
        {
            XmlAttribute attribute = (XmlAttribute) this.dsmlNode.SelectSingleNode(primaryXPath, this.dsmlNS);
            if (attribute == null)
            {
                attribute = (XmlAttribute) this.dsmlNode.SelectSingleNode(secondaryXPath, this.dsmlNS);
                if (attribute == null)
                {
                    return null;
                }
            }
            return attribute.Value;
        }

        internal Uri[] ReferralHelper(string primaryXPath)
        {
            XmlNodeList list = this.dsmlNode.SelectNodes(primaryXPath, this.dsmlNS);
            if (list.Count == 0)
            {
                return new Uri[0];
            }
            Uri[] uriArray = new Uri[list.Count];
            int index = 0;
            foreach (XmlNode node in list)
            {
                uriArray[index] = new Uri(node.InnerText);
                index++;
            }
            return uriArray;
        }

        internal System.DirectoryServices.Protocols.ResultCode ResultCodeHelper(string primaryXPath, string secondaryXPath)
        {
            int num;
            XmlAttribute attribute = (XmlAttribute) this.dsmlNode.SelectSingleNode(primaryXPath, this.dsmlNS);
            if (attribute == null)
            {
                attribute = (XmlAttribute) this.dsmlNode.SelectSingleNode(secondaryXPath, this.dsmlNS);
                if (attribute == null)
                {
                    throw new DsmlInvalidDocumentException(System.DirectoryServices.Protocols.Res.GetString("MissingOperationResponseResultCode"));
                }
            }
            string s = attribute.Value;
            try
            {
                num = int.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                throw new DsmlInvalidDocumentException(System.DirectoryServices.Protocols.Res.GetString("BadOperationResponseResultCode", new object[] { s }));
            }
            catch (OverflowException)
            {
                throw new DsmlInvalidDocumentException(System.DirectoryServices.Protocols.Res.GetString("BadOperationResponseResultCode", new object[] { s }));
            }
            if (!Utility.IsResultCode((System.DirectoryServices.Protocols.ResultCode) num))
            {
                throw new DsmlInvalidDocumentException(System.DirectoryServices.Protocols.Res.GetString("BadOperationResponseResultCode", new object[] { s }));
            }
            return (System.DirectoryServices.Protocols.ResultCode) num;
        }

        public virtual DirectoryControl[] Controls
        {
            get
            {
                if (this.dsmlRequest && (this.directoryControls == null))
                {
                    this.directoryControls = this.ControlsHelper("dsml:control");
                }
                if (this.directoryControls == null)
                {
                    return new DirectoryControl[0];
                }
                DirectoryControl[] controls = new DirectoryControl[this.directoryControls.Length];
                for (int i = 0; i < this.directoryControls.Length; i++)
                {
                    controls[i] = new DirectoryControl(this.directoryControls[i].Type, this.directoryControls[i].GetValue(), this.directoryControls[i].IsCritical, this.directoryControls[i].ServerSide);
                }
                DirectoryControl.TransformControls(controls);
                return controls;
            }
        }

        public virtual string ErrorMessage
        {
            get
            {
                if (this.dsmlRequest && (this.directoryMessage == null))
                {
                    this.directoryMessage = this.ErrorMessageHelper("dsml:errorMessage");
                }
                return this.directoryMessage;
            }
        }

        public virtual string MatchedDN
        {
            get
            {
                if (this.dsmlRequest && (this.dn == null))
                {
                    this.dn = this.MatchedDNHelper("@dsml:matchedDN", "@matchedDN");
                }
                return this.dn;
            }
        }

        public virtual Uri[] Referral
        {
            get
            {
                if (this.dsmlRequest && (this.directoryReferral == null))
                {
                    this.directoryReferral = this.ReferralHelper("dsml:referral");
                }
                if (this.directoryReferral == null)
                {
                    return new Uri[0];
                }
                Uri[] uriArray = new Uri[this.directoryReferral.Length];
                for (int i = 0; i < this.directoryReferral.Length; i++)
                {
                    uriArray[i] = new Uri(this.directoryReferral[i].AbsoluteUri);
                }
                return uriArray;
            }
        }

        public string RequestId
        {
            get
            {
                if (this.dsmlRequest && (this.requestID == null))
                {
                    XmlAttribute attribute = (XmlAttribute) this.dsmlNode.SelectSingleNode("@dsml:requestID", this.dsmlNS);
                    if (attribute == null)
                    {
                        attribute = (XmlAttribute) this.dsmlNode.SelectSingleNode("@requestID", this.dsmlNS);
                    }
                    if (attribute != null)
                    {
                        this.requestID = attribute.Value;
                    }
                }
                return this.requestID;
            }
        }

        public virtual System.DirectoryServices.Protocols.ResultCode ResultCode
        {
            get
            {
                if (this.dsmlRequest && (this.result == ~System.DirectoryServices.Protocols.ResultCode.Success))
                {
                    this.result = this.ResultCodeHelper("dsml:resultCode/@dsml:code", "dsml:resultCode/@code");
                }
                return this.result;
            }
        }
    }
}

