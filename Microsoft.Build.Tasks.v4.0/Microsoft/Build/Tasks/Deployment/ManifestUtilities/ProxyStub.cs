namespace Microsoft.Build.Tasks.Deployment.ManifestUtilities
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;

    [ComVisible(false)]
    public class ProxyStub
    {
        private string baseInterface;
        private string iid;
        private string name;
        private string numMethods;
        private string tlbid;

        [XmlIgnore]
        public string BaseInterface
        {
            get
            {
                return this.baseInterface;
            }
        }

        [XmlIgnore]
        public string IID
        {
            get
            {
                return this.iid;
            }
        }

        [XmlIgnore]
        public string Name
        {
            get
            {
                return this.name;
            }
        }

        [XmlIgnore]
        public string NumMethods
        {
            get
            {
                return this.numMethods;
            }
        }

        [XmlIgnore]
        public string TlbId
        {
            get
            {
                return this.tlbid;
            }
        }

        [XmlAttribute("BaseInterface"), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public string XmlBaseInterface
        {
            get
            {
                return this.baseInterface;
            }
            set
            {
                this.baseInterface = value;
            }
        }

        [XmlAttribute("Iid"), EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public string XmlIID
        {
            get
            {
                return this.iid;
            }
            set
            {
                this.iid = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), XmlAttribute("Name"), Browsable(false)]
        public string XmlName
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), XmlAttribute("NumMethods")]
        public string XmlNumMethods
        {
            get
            {
                return this.numMethods;
            }
            set
            {
                this.numMethods = value;
            }
        }

        [XmlAttribute("Tlbid"), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public string XmlTlbId
        {
            get
            {
                return this.tlbid;
            }
            set
            {
                this.tlbid = value;
            }
        }
    }
}

