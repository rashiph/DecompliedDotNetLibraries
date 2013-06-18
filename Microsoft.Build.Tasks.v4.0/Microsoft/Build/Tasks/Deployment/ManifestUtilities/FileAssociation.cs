namespace Microsoft.Build.Tasks.Deployment.ManifestUtilities
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;

    [ComVisible(false)]
    public sealed class FileAssociation
    {
        private string defaultIcon;
        private string description;
        private string extension;
        private string progid;

        [XmlIgnore]
        public string DefaultIcon
        {
            get
            {
                return this.defaultIcon;
            }
            set
            {
                this.defaultIcon = value;
            }
        }

        [XmlIgnore]
        public string Description
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

        [XmlIgnore]
        public string Extension
        {
            get
            {
                return this.extension;
            }
            set
            {
                this.extension = value;
            }
        }

        [XmlIgnore]
        public string ProgId
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

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), XmlAttribute("DefaultIcon")]
        public string XmlDefaultIcon
        {
            get
            {
                return this.defaultIcon;
            }
            set
            {
                this.defaultIcon = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), XmlAttribute("Description")]
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

        [XmlAttribute("Extension"), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public string XmlExtension
        {
            get
            {
                return this.extension;
            }
            set
            {
                this.extension = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), XmlAttribute("Progid")]
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
    }
}

