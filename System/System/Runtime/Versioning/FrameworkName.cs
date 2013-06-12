namespace System.Runtime.Versioning
{
    using System;
    using System.Text;

    [Serializable]
    public sealed class FrameworkName : IEquatable<FrameworkName>
    {
        private const char c_componentSeparator = ',';
        private const char c_keyValueSeparator = '=';
        private const string c_profileKey = "Profile";
        private const string c_versionKey = "Version";
        private const char c_versionValuePrefix = 'v';
        private string m_fullName;
        private readonly string m_identifier;
        private readonly string m_profile;
        private readonly System.Version m_version;

        public FrameworkName(string frameworkName)
        {
            if (frameworkName == null)
            {
                throw new ArgumentNullException("frameworkName");
            }
            if (frameworkName.Length == 0)
            {
                throw new ArgumentException(SR.GetString("net_emptystringcall", new object[] { "frameworkName" }), "frameworkName");
            }
            string[] strArray = frameworkName.Split(new char[] { ',' });
            if ((strArray.Length < 2) || (strArray.Length > 3))
            {
                throw new ArgumentException(SR.GetString("Argument_FrameworkNameTooShort"), "frameworkName");
            }
            this.m_identifier = strArray[0].Trim();
            if (this.m_identifier.Length == 0)
            {
                throw new ArgumentException(SR.GetString("Argument_FrameworkNameInvalid"), "frameworkName");
            }
            bool flag = false;
            this.m_profile = string.Empty;
            for (int i = 1; i < strArray.Length; i++)
            {
                string[] strArray2 = strArray[i].Split(new char[] { '=' });
                if (strArray2.Length != 2)
                {
                    throw new ArgumentException(SR.GetString("Argument_FrameworkNameInvalid"), "frameworkName");
                }
                string str = strArray2[0].Trim();
                string version = strArray2[1].Trim();
                if (str.Equals("Version", StringComparison.OrdinalIgnoreCase))
                {
                    flag = true;
                    if ((version.Length > 0) && ((version[0] == 'v') || (version[0] == 'V')))
                    {
                        version = version.Substring(1);
                    }
                    try
                    {
                        this.m_version = new System.Version(version);
                        continue;
                    }
                    catch (Exception exception)
                    {
                        throw new ArgumentException(SR.GetString("Argument_FrameworkNameInvalidVersion"), "frameworkName", exception);
                    }
                }
                if (!str.Equals("Profile", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException(SR.GetString("Argument_FrameworkNameInvalid"), "frameworkName");
                }
                if (!string.IsNullOrEmpty(version))
                {
                    this.m_profile = version;
                }
            }
            if (!flag)
            {
                throw new ArgumentException(SR.GetString("Argument_FrameworkNameMissingVersion"), "frameworkName");
            }
        }

        public FrameworkName(string identifier, System.Version version) : this(identifier, version, null)
        {
        }

        public FrameworkName(string identifier, System.Version version, string profile)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException("identifier");
            }
            if (identifier.Trim().Length == 0)
            {
                throw new ArgumentException(SR.GetString("net_emptystringcall", new object[] { "identifier" }), "identifier");
            }
            if (version == null)
            {
                throw new ArgumentNullException("version");
            }
            this.m_identifier = identifier.Trim();
            this.m_version = (System.Version) version.Clone();
            this.m_profile = (profile == null) ? string.Empty : profile.Trim();
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as FrameworkName);
        }

        public bool Equals(FrameworkName other)
        {
            if (object.ReferenceEquals(other, null))
            {
                return false;
            }
            return (((this.Identifier == other.Identifier) && (this.Version == other.Version)) && (this.Profile == other.Profile));
        }

        public override int GetHashCode()
        {
            return ((this.Identifier.GetHashCode() ^ this.Version.GetHashCode()) ^ this.Profile.GetHashCode());
        }

        public static bool operator ==(FrameworkName left, FrameworkName right)
        {
            if (object.ReferenceEquals(left, null))
            {
                return object.ReferenceEquals(right, null);
            }
            return left.Equals(right);
        }

        public static bool operator !=(FrameworkName left, FrameworkName right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return this.FullName;
        }

        public string FullName
        {
            get
            {
                if (this.m_fullName == null)
                {
                    StringBuilder builder = new StringBuilder();
                    builder.Append(this.Identifier);
                    builder.Append(',');
                    builder.Append("Version").Append('=');
                    builder.Append('v');
                    builder.Append(this.Version);
                    if (!string.IsNullOrEmpty(this.Profile))
                    {
                        builder.Append(',');
                        builder.Append("Profile").Append('=');
                        builder.Append(this.Profile);
                    }
                    this.m_fullName = builder.ToString();
                }
                return this.m_fullName;
            }
        }

        public string Identifier
        {
            get
            {
                return this.m_identifier;
            }
        }

        public string Profile
        {
            get
            {
                return this.m_profile;
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

