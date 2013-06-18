namespace Microsoft.Build.Tasks.Deployment.ManifestUtilities
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;

    [ComVisible(false)]
    public class ComClass
    {
        private string clsid;
        private string description;
        private string progid;
        private string threadingModel;
        private string tlbid;

        public ComClass()
        {
        }

        internal ComClass(Guid tlbId, Guid clsId, string progId, string threadingModel, string description)
        {
            this.tlbid = tlbId.ToString("B");
            this.clsid = clsId.ToString("B");
            this.progid = progId;
            this.threadingModel = threadingModel;
            this.description = description;
        }

        [XmlIgnore]
        public string ClsId
        {
            get
            {
                return this.clsid;
            }
        }

        [XmlIgnore]
        public string Description
        {
            get
            {
                return this.description;
            }
        }

        [XmlIgnore]
        public string ProgId
        {
            get
            {
                return this.progid;
            }
        }

        [XmlIgnore]
        public string ThreadingModel
        {
            get
            {
                return this.threadingModel;
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

        [XmlAttribute("Clsid"), EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public string XmlClsId
        {
            get
            {
                return this.clsid;
            }
            set
            {
                this.clsid = value;
            }
        }

        [Browsable(false), XmlAttribute("Description"), EditorBrowsable(EditorBrowsableState.Never)]
        public string XmlDescription
        {
            get
            {
                return this.description;
            }
            set
            {
                this.description = value;
            }
        }

        [XmlAttribute("Progid"), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public string XmlProgId
        {
            get
            {
                return this.progid;
            }
            set
            {
                this.progid = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), XmlAttribute("ThreadingModel")]
        public string XmlThreadingModel
        {
            get
            {
                return this.threadingModel;
            }
            set
            {
                this.threadingModel = value;
            }
        }

        [XmlAttribute("Tlbid"), EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
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

