namespace Microsoft.Build.Tasks.Deployment.ManifestUtilities
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;

    [ComVisible(false)]
    public sealed class AssemblyReference : BaseReference
    {
        private Microsoft.Build.Tasks.Deployment.ManifestUtilities.AssemblyIdentity assemblyIdentity;
        private bool isPrerequisite;
        private bool isPrimary;
        private AssemblyReferenceType referenceType;

        public AssemblyReference()
        {
        }

        public AssemblyReference(string path) : base(path)
        {
        }

        public override string ToString()
        {
            string str = base.ToString();
            if (!string.IsNullOrEmpty(str))
            {
                return str;
            }
            if (this.assemblyIdentity != null)
            {
                return this.assemblyIdentity.ToString();
            }
            return string.Empty;
        }

        [XmlIgnore]
        public Microsoft.Build.Tasks.Deployment.ManifestUtilities.AssemblyIdentity AssemblyIdentity
        {
            get
            {
                return this.assemblyIdentity;
            }
            set
            {
                this.assemblyIdentity = value;
            }
        }

        [XmlIgnore]
        public bool IsPrerequisite
        {
            get
            {
                return this.isPrerequisite;
            }
            set
            {
                this.isPrerequisite = value;
            }
        }

        [XmlIgnore]
        internal bool IsPrimary
        {
            get
            {
                return this.isPrimary;
            }
            set
            {
                this.isPrimary = value;
            }
        }

        [XmlIgnore]
        internal bool IsVirtual
        {
            get
            {
                if (this.AssemblyIdentity == null)
                {
                    return false;
                }
                return (string.Compare(this.AssemblyIdentity.Name, "Microsoft.Windows.CommonLanguageRuntime", StringComparison.OrdinalIgnoreCase) == 0);
            }
        }

        [XmlIgnore]
        public AssemblyReferenceType ReferenceType
        {
            get
            {
                return this.referenceType;
            }
            set
            {
                this.referenceType = value;
            }
        }

        protected internal override string SortName
        {
            get
            {
                if (this.assemblyIdentity == null)
                {
                    return null;
                }
                string str = this.assemblyIdentity.ToString();
                if (this.IsVirtual)
                {
                    return ("1: " + str);
                }
                if (this.isPrerequisite)
                {
                    return ("2: " + str);
                }
                return ("3: " + str + ", " + base.TargetPath);
            }
        }

        [XmlElement("AssemblyIdentity"), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public Microsoft.Build.Tasks.Deployment.ManifestUtilities.AssemblyIdentity XmlAssemblyIdentity
        {
            get
            {
                return this.assemblyIdentity;
            }
            set
            {
                this.assemblyIdentity = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), XmlAttribute("IsNative")]
        public string XmlIsNative
        {
            get
            {
                if (this.referenceType != AssemblyReferenceType.NativeAssembly)
                {
                    return "false";
                }
                return "true";
            }
            set
            {
                this.referenceType = ConvertUtil.ToBoolean(value) ? AssemblyReferenceType.NativeAssembly : AssemblyReferenceType.ManagedAssembly;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), XmlAttribute("IsPrerequisite"), Browsable(false)]
        public string XmlIsPrerequisite
        {
            get
            {
                return Convert.ToString(this.isPrerequisite, CultureInfo.InvariantCulture).ToLower(CultureInfo.InvariantCulture);
            }
            set
            {
                this.isPrerequisite = ConvertUtil.ToBoolean(value);
            }
        }
    }
}

