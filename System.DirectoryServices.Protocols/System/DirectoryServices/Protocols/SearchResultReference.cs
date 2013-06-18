namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Xml;

    public class SearchResultReference
    {
        private XmlNode dsmlNode;
        private XmlNamespaceManager dsmlNS;
        private bool dsmlRequest;
        private DirectoryControl[] resultControls;
        private Uri[] resultReferences;

        internal SearchResultReference(XmlNode node)
        {
            this.dsmlNode = node;
            this.dsmlNS = NamespaceUtils.GetDsmlNamespaceManager();
            this.dsmlRequest = true;
        }

        internal SearchResultReference(Uri[] uris)
        {
            this.resultReferences = uris;
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

        private Uri[] UriHelper()
        {
            XmlNodeList list = this.dsmlNode.SelectNodes("dsml:ref", this.dsmlNS);
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

        public Uri[] Reference
        {
            get
            {
                if (this.dsmlRequest && (this.resultReferences == null))
                {
                    this.resultReferences = this.UriHelper();
                }
                if (this.resultReferences == null)
                {
                    return new Uri[0];
                }
                Uri[] uriArray = new Uri[this.resultReferences.Length];
                for (int i = 0; i < this.resultReferences.Length; i++)
                {
                    uriArray[i] = new Uri(this.resultReferences[i].AbsoluteUri);
                }
                return uriArray;
            }
        }
    }
}

