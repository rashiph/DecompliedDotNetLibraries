namespace Microsoft.Build.Tasks.Deployment.ManifestUtilities
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml.Serialization;

    [ComVisible(false)]
    public class TypeLib
    {
        private string flags;
        private string helpDirectory;
        private string resourceid;
        private string tlbid;
        private string version;

        public TypeLib()
        {
        }

        internal TypeLib(Guid tlbId, System.Version version, string helpDirectory, int resourceId, int flags)
        {
            this.tlbid = tlbId.ToString("B");
            this.version = version.ToString(2);
            this.helpDirectory = helpDirectory;
            this.resourceid = Convert.ToString(resourceId, 0x10);
            this.flags = FlagsFromInt(flags);
        }

        private static string FlagsFromInt(int flags)
        {
            StringBuilder builder = new StringBuilder();
            if ((flags & 1) != 0)
            {
                builder.Append("RESTRICTED,");
            }
            if ((flags & 2) != 0)
            {
                builder.Append("CONTROL,");
            }
            if ((flags & 4) != 0)
            {
                builder.Append("HIDDEN,");
            }
            if ((flags & 8) != 0)
            {
                builder.Append("HASDISKIMAGE,");
            }
            return builder.ToString().TrimEnd(new char[] { ',' });
        }

        [XmlIgnore]
        public string Flags
        {
            get
            {
                return this.flags;
            }
        }

        [XmlIgnore]
        public string HelpDirectory
        {
            get
            {
                return this.helpDirectory;
            }
        }

        [XmlIgnore]
        public string ResourceId
        {
            get
            {
                return this.resourceid;
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

        [XmlIgnore]
        public string Version
        {
            get
            {
                return this.version;
            }
        }

        [XmlAttribute("Flags"), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public string XmlFlags
        {
            get
            {
                return this.flags;
            }
            set
            {
                this.flags = value;
            }
        }

        [XmlAttribute("HelpDir"), EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public string XmlHelpDirectory
        {
            get
            {
                return this.helpDirectory;
            }
            set
            {
                this.helpDirectory = value;
            }
        }

        [XmlAttribute("ResourceId"), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public string XmlResourceId
        {
            get
            {
                return this.resourceid;
            }
            set
            {
                this.resourceid = value;
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

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), XmlAttribute("Version")]
        public string XmlVersion
        {
            get
            {
                return this.version;
            }
            set
            {
                this.version = value;
            }
        }
    }
}

