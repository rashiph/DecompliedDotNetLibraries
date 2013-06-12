namespace System
{
    using System.Runtime.InteropServices;
    using System.Security.Util;
    using System.Text;

    [Serializable, ComVisible(true)]
    public sealed class ApplicationId
    {
        private string m_culture;
        private string m_name;
        private string m_processorArchitecture;
        internal byte[] m_publicKeyToken;
        private System.Version m_version;

        internal ApplicationId()
        {
        }

        public ApplicationId(byte[] publicKeyToken, string name, System.Version version, string processorArchitecture, string culture)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyApplicationName"));
            }
            if (version == null)
            {
                throw new ArgumentNullException("version");
            }
            if (publicKeyToken == null)
            {
                throw new ArgumentNullException("publicKeyToken");
            }
            this.m_publicKeyToken = new byte[publicKeyToken.Length];
            Array.Copy(publicKeyToken, 0, this.m_publicKeyToken, 0, publicKeyToken.Length);
            this.m_name = name;
            this.m_version = version;
            this.m_processorArchitecture = processorArchitecture;
            this.m_culture = culture;
        }

        public ApplicationId Copy()
        {
            return new ApplicationId(this.m_publicKeyToken, this.m_name, this.m_version, this.m_processorArchitecture, this.m_culture);
        }

        public override bool Equals(object o)
        {
            ApplicationId id = o as ApplicationId;
            if (id == null)
            {
                return false;
            }
            if ((!object.Equals(this.m_name, id.m_name) || !object.Equals(this.m_version, id.m_version)) || (!object.Equals(this.m_processorArchitecture, id.m_processorArchitecture) || !object.Equals(this.m_culture, id.m_culture)))
            {
                return false;
            }
            if (this.m_publicKeyToken.Length != id.m_publicKeyToken.Length)
            {
                return false;
            }
            for (int i = 0; i < this.m_publicKeyToken.Length; i++)
            {
                if (this.m_publicKeyToken[i] != id.m_publicKeyToken[i])
                {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            return (this.m_name.GetHashCode() ^ this.m_version.GetHashCode());
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(this.m_name);
            if (this.m_culture != null)
            {
                builder.Append(", culture=\"");
                builder.Append(this.m_culture);
                builder.Append("\"");
            }
            builder.Append(", version=\"");
            builder.Append(this.m_version.ToString());
            builder.Append("\"");
            if (this.m_publicKeyToken != null)
            {
                builder.Append(", publicKeyToken=\"");
                builder.Append(Hex.EncodeHexString(this.m_publicKeyToken));
                builder.Append("\"");
            }
            if (this.m_processorArchitecture != null)
            {
                builder.Append(", processorArchitecture =\"");
                builder.Append(this.m_processorArchitecture);
                builder.Append("\"");
            }
            return builder.ToString();
        }

        public string Culture
        {
            get
            {
                return this.m_culture;
            }
        }

        public string Name
        {
            get
            {
                return this.m_name;
            }
        }

        public string ProcessorArchitecture
        {
            get
            {
                return this.m_processorArchitecture;
            }
        }

        public byte[] PublicKeyToken
        {
            get
            {
                byte[] destinationArray = new byte[this.m_publicKeyToken.Length];
                Array.Copy(this.m_publicKeyToken, 0, destinationArray, 0, this.m_publicKeyToken.Length);
                return destinationArray;
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

