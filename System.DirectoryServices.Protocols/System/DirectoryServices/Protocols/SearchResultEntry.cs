namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Xml;

    public class SearchResultEntry
    {
        private SearchResultAttributeCollection attributes;
        private string distinguishedName;
        private XmlNode dsmlNode;
        private XmlNamespaceManager dsmlNS;
        private bool dsmlRequest;
        private DirectoryControl[] resultControls;

        internal SearchResultEntry(string dn)
        {
            this.attributes = new SearchResultAttributeCollection();
            this.distinguishedName = dn;
        }

        internal SearchResultEntry(XmlNode node)
        {
            this.attributes = new SearchResultAttributeCollection();
            this.dsmlNode = node;
            this.dsmlNS = NamespaceUtils.GetDsmlNamespaceManager();
            this.dsmlRequest = true;
        }

        internal SearchResultEntry(string dn, SearchResultAttributeCollection attrs)
        {
            this.attributes = new SearchResultAttributeCollection();
            this.distinguishedName = dn;
            this.attributes = attrs;
        }

        private SearchResultAttributeCollection AttributesHelper()
        {
            SearchResultAttributeCollection attributes = new SearchResultAttributeCollection();
            XmlNodeList list = this.dsmlNode.SelectNodes("dsml:attr", this.dsmlNS);
            if (list.Count != 0)
            {
                foreach (XmlNode node in list)
                {
                    DirectoryAttribute attribute = new DirectoryAttribute((XmlElement) node);
                    attributes.Add(attribute.Name, attribute);
                }
            }
            return attributes;
        }

        private DirectoryControl[] ControlsHelper()
        {
            XmlNodeList list = this.dsmlNode.SelectNodes("dsml:control", this.dsmlNS);
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

        private string DNHelper(string primaryXPath, string secondaryXPath)
        {
            XmlAttribute attribute = (XmlAttribute) this.dsmlNode.SelectSingleNode(primaryXPath, this.dsmlNS);
            if (attribute == null)
            {
                attribute = (XmlAttribute) this.dsmlNode.SelectSingleNode(secondaryXPath, this.dsmlNS);
                if (attribute == null)
                {
                    throw new DsmlInvalidDocumentException(System.DirectoryServices.Protocols.Res.GetString("MissingSearchResultEntryDN"));
                }
            }
            return attribute.Value;
        }

        public SearchResultAttributeCollection Attributes
        {
            get
            {
                if (this.dsmlRequest && (this.attributes.Count == 0))
                {
                    this.attributes = this.AttributesHelper();
                }
                return this.attributes;
            }
        }

        public DirectoryControl[] Controls
        {
            get
            {
                DirectoryControl[] controls = null;
                if (this.dsmlRequest && (this.resultControls == null))
                {
                    this.resultControls = this.ControlsHelper();
                }
                if (this.resultControls == null)
                {
                    return new DirectoryControl[0];
                }
                controls = new DirectoryControl[this.resultControls.Length];
                for (int i = 0; i < this.resultControls.Length; i++)
                {
                    controls[i] = new DirectoryControl(this.resultControls[i].Type, this.resultControls[i].GetValue(), this.resultControls[i].IsCritical, this.resultControls[i].ServerSide);
                }
                DirectoryControl.TransformControls(controls);
                return controls;
            }
        }

        public string DistinguishedName
        {
            get
            {
                if (this.dsmlRequest && (this.distinguishedName == null))
                {
                    this.distinguishedName = this.DNHelper("@dsml:dn", "@dn");
                }
                return this.distinguishedName;
            }
        }
    }
}

