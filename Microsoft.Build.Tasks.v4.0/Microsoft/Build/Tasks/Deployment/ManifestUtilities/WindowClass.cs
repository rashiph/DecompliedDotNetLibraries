namespace Microsoft.Build.Tasks.Deployment.ManifestUtilities
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;

    [ComVisible(false)]
    public class WindowClass
    {
        private string name;
        private string versioned;

        public WindowClass()
        {
        }

        public WindowClass(string name, bool versioned)
        {
            this.name = name;
            this.versioned = versioned ? "yes" : "no";
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
        public bool Versioned
        {
            get
            {
                if ((string.Compare(this.versioned, "yes", StringComparison.OrdinalIgnoreCase) != 0) && (string.Compare(this.versioned, "no", StringComparison.OrdinalIgnoreCase) == 0))
                {
                    return false;
                }
                return true;
            }
        }

        [Browsable(false), XmlAttribute("Name"), EditorBrowsable(EditorBrowsableState.Never)]
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

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), XmlAttribute("Versioned")]
        public string XmlVersioned
        {
            get
            {
                return this.versioned;
            }
            set
            {
                this.versioned = value;
            }
        }
    }
}

