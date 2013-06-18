namespace Microsoft.Build.Tasks.Deployment.ManifestUtilities
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;

    [ComVisible(false)]
    public abstract class BaseReference
    {
        private string group;
        private string hash;
        private string hashAlgorithm;
        private bool includeHash;
        private string isOptional;
        private string resolvedPath;
        private string size;
        private string sourcePath;
        private string targetPath;

        protected internal BaseReference()
        {
            this.includeHash = true;
        }

        protected internal BaseReference(string path)
        {
            this.includeHash = true;
            this.sourcePath = path;
            this.targetPath = GetDefaultTargetPath(path);
        }

        internal static string GetDefaultTargetPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }
            if (path.EndsWith(".deploy", StringComparison.OrdinalIgnoreCase))
            {
                path = path.Substring(0, path.Length - ".deploy".Length);
            }
            if (!Path.IsPathRooted(path))
            {
                return path;
            }
            return Path.GetFileName(path);
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.sourcePath))
            {
                return this.sourcePath;
            }
            if (!string.IsNullOrEmpty(this.resolvedPath))
            {
                return this.resolvedPath;
            }
            if (!string.IsNullOrEmpty(this.targetPath))
            {
                return this.targetPath;
            }
            return string.Empty;
        }

        [XmlIgnore]
        public string Group
        {
            get
            {
                return this.group;
            }
            set
            {
                this.group = value;
            }
        }

        [XmlIgnore]
        public string Hash
        {
            get
            {
                if (!this.IncludeHash)
                {
                    return string.Empty;
                }
                return this.hash;
            }
            set
            {
                this.hash = value;
            }
        }

        internal bool IncludeHash
        {
            get
            {
                return this.includeHash;
            }
            set
            {
                this.includeHash = value;
            }
        }

        [XmlIgnore]
        public bool IsOptional
        {
            get
            {
                return ConvertUtil.ToBoolean(this.isOptional);
            }
            set
            {
                this.isOptional = value ? "true" : null;
            }
        }

        [XmlIgnore]
        public string ResolvedPath
        {
            get
            {
                return this.resolvedPath;
            }
            set
            {
                this.resolvedPath = value;
            }
        }

        [XmlIgnore]
        public long Size
        {
            get
            {
                return Convert.ToInt64(this.size, CultureInfo.InvariantCulture);
            }
            set
            {
                this.size = Convert.ToString(value, CultureInfo.InvariantCulture);
            }
        }

        protected internal abstract string SortName { get; }

        [XmlIgnore]
        public string SourcePath
        {
            get
            {
                return this.sourcePath;
            }
            set
            {
                this.sourcePath = value;
            }
        }

        [XmlIgnore]
        public string TargetPath
        {
            get
            {
                return this.targetPath;
            }
            set
            {
                this.targetPath = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), XmlAttribute("Group")]
        public string XmlGroup
        {
            get
            {
                return this.group;
            }
            set
            {
                this.group = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), XmlAttribute("Hash")]
        public string XmlHash
        {
            get
            {
                return this.Hash;
            }
            set
            {
                this.hash = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), XmlAttribute("HashAlg"), Browsable(false)]
        public string XmlHashAlgorithm
        {
            get
            {
                return this.hashAlgorithm;
            }
            set
            {
                this.hashAlgorithm = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), XmlAttribute("IsOptional")]
        public string XmlIsOptional
        {
            get
            {
                if (this.isOptional == null)
                {
                    return null;
                }
                return this.isOptional.ToLower(CultureInfo.InvariantCulture);
            }
            set
            {
                this.isOptional = value;
            }
        }

        [XmlAttribute("Path"), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public string XmlPath
        {
            get
            {
                return this.targetPath;
            }
            set
            {
                this.targetPath = value;
            }
        }

        [XmlAttribute("Size"), EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public string XmlSize
        {
            get
            {
                return this.size;
            }
            set
            {
                this.size = value;
            }
        }
    }
}

