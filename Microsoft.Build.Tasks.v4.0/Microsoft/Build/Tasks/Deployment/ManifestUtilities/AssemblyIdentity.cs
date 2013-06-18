namespace Microsoft.Build.Tasks.Deployment.ManifestUtilities
{
    using Microsoft.Build.Tasks;
    using Microsoft.Build.Utilities;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.Serialization;

    [ComVisible(false), XmlRoot("AssemblyIdentity")]
    public sealed class AssemblyIdentity
    {
        private string culture;
        private string name;
        private string processorArchitecture;
        private string publicKeyToken;
        private string type;
        private string version;

        public AssemblyIdentity()
        {
        }

        public AssemblyIdentity(AssemblyIdentity identity)
        {
            if (identity != null)
            {
                this.name = identity.name;
                this.version = identity.version;
                this.publicKeyToken = identity.publicKeyToken;
                this.culture = identity.culture;
                this.processorArchitecture = identity.processorArchitecture;
                this.type = identity.type;
            }
        }

        public AssemblyIdentity(string name)
        {
            this.name = name;
        }

        public AssemblyIdentity(string name, string version)
        {
            this.name = name;
            this.version = version;
        }

        public AssemblyIdentity(string name, string version, string publicKeyToken, string culture)
        {
            this.name = name;
            this.version = version;
            this.publicKeyToken = publicKeyToken;
            this.culture = culture;
        }

        public AssemblyIdentity(string name, string version, string publicKeyToken, string culture, string processorArchitecture)
        {
            this.name = name;
            this.version = version;
            this.publicKeyToken = publicKeyToken;
            this.culture = culture;
            this.processorArchitecture = processorArchitecture;
        }

        public AssemblyIdentity(string name, string version, string publicKeyToken, string culture, string processorArchitecture, string type)
        {
            this.name = name;
            this.version = version;
            this.publicKeyToken = publicKeyToken;
            this.culture = culture;
            this.processorArchitecture = processorArchitecture;
            this.type = type;
        }

        public static AssemblyIdentity FromAssemblyName(string assemblyName)
        {
            Match match = new Regex("^(?<name>[^,]*)(, Version=(?<version>[^,]*))?(, Culture=(?<culture>[^,]*))?(, PublicKeyToken=(?<pkt>[^,]*))?(, ProcessorArchitecture=(?<pa>[^,]*))?(, Type=(?<type>[^,]*))?").Match(assemblyName);
            string name = match.Result("${name}");
            string version = match.Result("${version}");
            string publicKeyToken = match.Result("${pkt}");
            string culture = match.Result("${culture}");
            string processorArchitecture = match.Result("${pa}");
            return new AssemblyIdentity(name, version, publicKeyToken, culture, processorArchitecture, match.Result("${type}"));
        }

        public static AssemblyIdentity FromFile(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }
            AssemblyIdentity identity = null;
            identity = FromNativeAssembly(path);
            if (identity == null)
            {
                identity = FromManagedAssembly(path);
            }
            return identity;
        }

        public static AssemblyIdentity FromManagedAssembly(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }
            using (MetadataReader reader = MetadataReader.Create(path))
            {
                if (reader != null)
                {
                    return new AssemblyIdentity(reader.Name, reader.Version, reader.PublicKeyToken, reader.Culture, reader.ProcessorArchitecture);
                }
                return null;
            }
        }

        private static AssemblyIdentity FromManifest(Stream s)
        {
            XmlDocument document = new XmlDocument();
            try
            {
                document.Load(s);
            }
            catch (XmlException)
            {
                return null;
            }
            return FromManifest(document);
        }

        public static AssemblyIdentity FromManifest(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }
            XmlDocument document = new XmlDocument();
            try
            {
                document.Load(path);
            }
            catch (XmlException)
            {
                return null;
            }
            return FromManifest(document);
        }

        private static AssemblyIdentity FromManifest(XmlDocument document)
        {
            XmlNamespaceManager namespaceManager = XmlNamespaces.GetNamespaceManager(document.NameTable);
            XmlElement element = (XmlElement) document.SelectSingleNode("/asmv1:assembly/asmv1:assemblyIdentity|/asmv1:assembly/asmv2:assemblyIdentity", namespaceManager);
            if (element == null)
            {
                return null;
            }
            XmlNode namedItem = element.Attributes.GetNamedItem("name");
            string name = (namedItem != null) ? namedItem.Value : null;
            namedItem = element.Attributes.GetNamedItem("version");
            string version = (namedItem != null) ? namedItem.Value : null;
            namedItem = element.Attributes.GetNamedItem("publicKeyToken");
            string publicKeyToken = (namedItem != null) ? namedItem.Value : null;
            namedItem = element.Attributes.GetNamedItem("language");
            string culture = (namedItem != null) ? namedItem.Value : null;
            namedItem = element.Attributes.GetNamedItem("processorArchitecture");
            string processorArchitecture = (namedItem != null) ? namedItem.Value : null;
            namedItem = element.Attributes.GetNamedItem("type");
            return new AssemblyIdentity(name, version, publicKeyToken, culture, processorArchitecture, (namedItem != null) ? namedItem.Value : null);
        }

        public static AssemblyIdentity FromNativeAssembly(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }
            if (!PathUtil.IsPEFile(path))
            {
                return FromManifest(path);
            }
            Stream s = EmbeddedManifestReader.Read(path);
            if (s == null)
            {
                return null;
            }
            return FromManifest(s);
        }

        public string GetFullName(FullNameFlags flags)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(this.name);
            if (!string.IsNullOrEmpty(this.version))
            {
                builder.Append(string.Format(CultureInfo.InvariantCulture, ", Version={0}", new object[] { this.version }));
            }
            if (!string.IsNullOrEmpty(this.culture))
            {
                builder.Append(string.Format(CultureInfo.InvariantCulture, ", Culture={0}", new object[] { this.culture }));
            }
            if (!string.IsNullOrEmpty(this.publicKeyToken))
            {
                builder.Append(string.Format(CultureInfo.InvariantCulture, ", PublicKeyToken={0}", new object[] { this.publicKeyToken }));
            }
            if (!string.IsNullOrEmpty(this.processorArchitecture) && ((flags & FullNameFlags.ProcessorArchitecture) != FullNameFlags.Default))
            {
                builder.Append(string.Format(CultureInfo.InvariantCulture, ", ProcessorArchitecture={0}", new object[] { this.processorArchitecture }));
            }
            if (!string.IsNullOrEmpty(this.type) && ((flags & FullNameFlags.Type) != FullNameFlags.Default))
            {
                builder.Append(string.Format(CultureInfo.InvariantCulture, ", Type={0}", new object[] { this.type }));
            }
            return builder.ToString();
        }

        internal static bool IsEqual(AssemblyIdentity a1, AssemblyIdentity a2)
        {
            return IsEqual(a1, a2, true);
        }

        internal static bool IsEqual(AssemblyIdentity a1, AssemblyIdentity a2, bool specificVersion)
        {
            if ((a1 == null) || (a2 == null))
            {
                return false;
            }
            if (!specificVersion)
            {
                return string.Equals(a1.name, a2.name, StringComparison.OrdinalIgnoreCase);
            }
            return (((string.Equals(a1.name, a2.name, StringComparison.OrdinalIgnoreCase) && string.Equals(a1.publicKeyToken, a2.publicKeyToken, StringComparison.OrdinalIgnoreCase)) && (string.Equals(a1.version, a2.version, StringComparison.OrdinalIgnoreCase) && string.Equals(a1.culture, a2.culture, StringComparison.OrdinalIgnoreCase))) && string.Equals(a1.processorArchitecture, a2.processorArchitecture, StringComparison.OrdinalIgnoreCase));
        }

        internal string Resolve(string[] searchPaths)
        {
            return this.Resolve(searchPaths, this.IsStrongName);
        }

        internal string Resolve(string[] searchPaths, bool specificVersion)
        {
            if (searchPaths == null)
            {
                searchPaths = new string[] { @".\" };
            }
            foreach (string str in searchPaths)
            {
                string str2 = string.Format(CultureInfo.InvariantCulture, "{0}.dll", new object[] { this.name });
                string path = Path.Combine(str, str2);
                if (File.Exists(path) && IsEqual(this, FromFile(path), specificVersion))
                {
                    return path;
                }
                str2 = string.Format(CultureInfo.InvariantCulture, "{0}.manifest", new object[] { this.name });
                path = Path.Combine(str, str2);
                if (File.Exists(path) && IsEqual(this, FromManifest(path), specificVersion))
                {
                    return path;
                }
            }
            return null;
        }

        public override string ToString()
        {
            return this.GetFullName(FullNameFlags.All);
        }

        [XmlIgnore]
        public string Culture
        {
            get
            {
                return this.culture;
            }
            set
            {
                this.culture = value;
            }
        }

        [XmlIgnore]
        public bool IsFrameworkAssembly
        {
            get
            {
                Dictionary<string, RedistList> dictionary = new Dictionary<string, RedistList>();
                foreach (string str in ToolLocationHelper.GetSupportedTargetFrameworks())
                {
                    FrameworkName frameworkName = new FrameworkName(str);
                    foreach (string str2 in ToolLocationHelper.GetPathToReferenceAssemblies(frameworkName))
                    {
                        if (!dictionary.ContainsKey(str2))
                        {
                            dictionary.Add(str2, RedistList.GetRedistListFromPath(str2));
                        }
                    }
                }
                string fullName = this.GetFullName(FullNameFlags.Default);
                foreach (RedistList list2 in dictionary.Values)
                {
                    if ((list2 != null) && list2.IsFrameworkAssembly(fullName))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        [XmlIgnore]
        public bool IsNeutralPlatform
        {
            get
            {
                if (!string.IsNullOrEmpty(this.processorArchitecture))
                {
                    return string.Equals(this.processorArchitecture, "msil", StringComparison.OrdinalIgnoreCase);
                }
                return true;
            }
        }

        [XmlIgnore]
        public bool IsStrongName
        {
            get
            {
                return ((!string.IsNullOrEmpty(this.name) && !string.IsNullOrEmpty(this.version)) && !string.IsNullOrEmpty(this.publicKeyToken));
            }
        }

        [XmlIgnore]
        public string Name
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

        [XmlIgnore]
        public string ProcessorArchitecture
        {
            get
            {
                return this.processorArchitecture;
            }
            set
            {
                this.processorArchitecture = value;
            }
        }

        [XmlIgnore]
        public string PublicKeyToken
        {
            get
            {
                return this.publicKeyToken;
            }
            set
            {
                this.publicKeyToken = value;
            }
        }

        [XmlIgnore]
        public string Type
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = value;
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

        [Browsable(false), XmlAttribute("Culture"), EditorBrowsable(EditorBrowsableState.Never)]
        public string XmlCulture
        {
            get
            {
                return this.culture;
            }
            set
            {
                this.culture = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), XmlAttribute("Name")]
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

        [XmlAttribute("ProcessorArchitecture"), EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public string XmlProcessorArchitecture
        {
            get
            {
                return this.processorArchitecture;
            }
            set
            {
                this.processorArchitecture = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), XmlAttribute("PublicKeyToken")]
        public string XmlPublicKeyToken
        {
            get
            {
                return this.publicKeyToken;
            }
            set
            {
                this.publicKeyToken = value;
            }
        }

        [XmlAttribute("Type"), EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public string XmlType
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = value;
            }
        }

        [XmlAttribute("Version"), EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
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

        [Flags]
        public enum FullNameFlags
        {
            Default,
            ProcessorArchitecture,
            Type,
            All
        }
    }
}

