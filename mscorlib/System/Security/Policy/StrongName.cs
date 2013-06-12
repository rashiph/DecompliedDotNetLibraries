namespace System.Security.Policy
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Util;

    [Serializable, ComVisible(true)]
    public sealed class StrongName : EvidenceBase, IIdentityPermissionFactory, IDelayEvaluatedEvidence
    {
        [NonSerialized]
        private RuntimeAssembly m_assembly;
        private string m_name;
        private StrongNamePublicKeyBlob m_publicKeyBlob;
        private System.Version m_version;
        [NonSerialized]
        private bool m_wasUsed;

        internal StrongName()
        {
        }

        public StrongName(StrongNamePublicKeyBlob blob, string name, System.Version version) : this(blob, name, version, null)
        {
        }

        internal StrongName(StrongNamePublicKeyBlob blob, string name, System.Version version, Assembly assembly)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyStrongName"));
            }
            if (blob == null)
            {
                throw new ArgumentNullException("blob");
            }
            if (version == null)
            {
                throw new ArgumentNullException("version");
            }
            RuntimeAssembly assembly2 = assembly as RuntimeAssembly;
            if ((assembly != null) && (assembly2 == null))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeAssembly"), "assembly");
            }
            this.m_publicKeyBlob = blob;
            this.m_name = name;
            this.m_version = version;
            this.m_assembly = assembly2;
        }

        public override EvidenceBase Clone()
        {
            return new StrongName(this.m_publicKeyBlob, this.m_name, this.m_version);
        }

        internal static bool CompareNames(string asmName, string mcName)
        {
            if (((mcName.Length > 0) && (mcName[mcName.Length - 1] == '*')) && ((mcName.Length - 1) <= asmName.Length))
            {
                return (string.Compare(mcName, 0, asmName, 0, mcName.Length - 1, StringComparison.OrdinalIgnoreCase) == 0);
            }
            return (string.Compare(mcName, asmName, StringComparison.OrdinalIgnoreCase) == 0);
        }

        public object Copy()
        {
            return this.Clone();
        }

        public IPermission CreateIdentityPermission(Evidence evidence)
        {
            return new StrongNameIdentityPermission(this.m_publicKeyBlob, this.m_name, this.m_version);
        }

        public override bool Equals(object o)
        {
            StrongName name = o as StrongName;
            return ((((name != null) && object.Equals(this.m_publicKeyBlob, name.m_publicKeyBlob)) && object.Equals(this.m_name, name.m_name)) && object.Equals(this.m_version, name.m_version));
        }

        internal void FromXml(SecurityElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (string.Compare(element.Tag, "StrongName", StringComparison.Ordinal) != 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidXML"));
            }
            this.m_publicKeyBlob = null;
            this.m_version = null;
            string hexString = element.Attribute("Key");
            if (hexString != null)
            {
                this.m_publicKeyBlob = new StrongNamePublicKeyBlob(Hex.DecodeHexString(hexString));
            }
            this.m_name = element.Attribute("Name");
            string version = element.Attribute("Version");
            if (version != null)
            {
                this.m_version = new System.Version(version);
            }
        }

        public override int GetHashCode()
        {
            if (this.m_publicKeyBlob != null)
            {
                return this.m_publicKeyBlob.GetHashCode();
            }
            if ((this.m_name != null) || (this.m_version != null))
            {
                return (((this.m_name == null) ? 0 : this.m_name.GetHashCode()) + ((this.m_version == null) ? 0 : this.m_version.GetHashCode()));
            }
            return typeof(StrongName).GetHashCode();
        }

        internal object Normalize()
        {
            MemoryStream output = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(output);
            writer.Write(this.m_publicKeyBlob.PublicKey);
            writer.Write(this.m_version.Major);
            writer.Write(this.m_name);
            output.Position = 0L;
            return output;
        }

        void IDelayEvaluatedEvidence.MarkUsed()
        {
            this.m_wasUsed = true;
        }

        public override string ToString()
        {
            return this.ToXml().ToString();
        }

        internal SecurityElement ToXml()
        {
            SecurityElement element = new SecurityElement("StrongName");
            element.AddAttribute("version", "1");
            if (this.m_publicKeyBlob != null)
            {
                element.AddAttribute("Key", Hex.EncodeHexString(this.m_publicKeyBlob.PublicKey));
            }
            if (this.m_name != null)
            {
                element.AddAttribute("Name", this.m_name);
            }
            if (this.m_version != null)
            {
                element.AddAttribute("Version", this.m_version.ToString());
            }
            return element;
        }

        public string Name
        {
            get
            {
                return this.m_name;
            }
        }

        public StrongNamePublicKeyBlob PublicKey
        {
            get
            {
                return this.m_publicKeyBlob;
            }
        }

        bool IDelayEvaluatedEvidence.IsVerified
        {
            [SecurityCritical]
            get
            {
                return ((this.m_assembly == null) || this.m_assembly.IsStrongNameVerified);
            }
        }

        bool IDelayEvaluatedEvidence.WasUsed
        {
            get
            {
                return this.m_wasUsed;
            }
        }

        public System.Version Version
        {
            get
            {
                return this.m_version;
            }
        }
    }
}

