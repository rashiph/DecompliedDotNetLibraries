namespace Microsoft.Build.Tasks.Deployment.ManifestUtilities
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;

    [ComVisible(false)]
    public sealed class CompatibleFramework
    {
        private string profile;
        private string supportedRuntime;
        private string version;

        [XmlIgnore]
        public string Profile
        {
            get
            {
                return this.profile;
            }
            set
            {
                this.profile = value;
            }
        }

        [XmlIgnore]
        public string SupportedRuntime
        {
            get
            {
                return this.supportedRuntime;
            }
            set
            {
                this.supportedRuntime = value;
            }
        }

        [XmlIgnore]
        public string Version
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

        [EditorBrowsable(EditorBrowsableState.Never), XmlAttribute("Profile"), Browsable(false)]
        public string XmlProfile
        {
            get
            {
                return this.profile;
            }
            set
            {
                this.profile = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), XmlAttribute("SupportedRuntime")]
        public string XmlSupportedRuntime
        {
            get
            {
                return this.supportedRuntime;
            }
            set
            {
                this.supportedRuntime = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), XmlAttribute("Version"), Browsable(false)]
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

