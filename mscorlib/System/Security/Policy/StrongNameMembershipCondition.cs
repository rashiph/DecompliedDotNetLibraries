namespace System.Security.Policy
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Util;

    [Serializable, ComVisible(true)]
    public sealed class StrongNameMembershipCondition : IConstantMembershipCondition, IReportMatchMembershipCondition, IMembershipCondition, ISecurityEncodable, ISecurityPolicyEncodable
    {
        private SecurityElement m_element;
        private string m_name;
        private StrongNamePublicKeyBlob m_publicKeyBlob;
        private System.Version m_version;
        private const string s_tagName = "Name";
        private const string s_tagPublicKeyBlob = "PublicKeyBlob";
        private const string s_tagVersion = "AssemblyVersion";

        internal StrongNameMembershipCondition()
        {
        }

        public StrongNameMembershipCondition(StrongNamePublicKeyBlob blob, string name, System.Version version)
        {
            if (blob == null)
            {
                throw new ArgumentNullException("blob");
            }
            if ((name != null) && name.Equals(""))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyStrongName"));
            }
            this.m_publicKeyBlob = blob;
            this.m_name = name;
            this.m_version = version;
        }

        public bool Check(Evidence evidence)
        {
            object usedEvidence = null;
            return ((IReportMatchMembershipCondition) this).Check(evidence, out usedEvidence);
        }

        public IMembershipCondition Copy()
        {
            return new StrongNameMembershipCondition(this.PublicKey, this.Name, this.Version);
        }

        public override bool Equals(object o)
        {
            StrongNameMembershipCondition condition = o as StrongNameMembershipCondition;
            if (condition != null)
            {
                if ((this.m_publicKeyBlob == null) && (this.m_element != null))
                {
                    this.ParseKeyBlob();
                }
                if ((condition.m_publicKeyBlob == null) && (condition.m_element != null))
                {
                    condition.ParseKeyBlob();
                }
                if (object.Equals(this.m_publicKeyBlob, condition.m_publicKeyBlob))
                {
                    if ((this.m_name == null) && (this.m_element != null))
                    {
                        this.ParseName();
                    }
                    if ((condition.m_name == null) && (condition.m_element != null))
                    {
                        condition.ParseName();
                    }
                    if (object.Equals(this.m_name, condition.m_name))
                    {
                        if ((this.m_version == null) && (this.m_element != null))
                        {
                            this.ParseVersion();
                        }
                        if ((condition.m_version == null) && (condition.m_element != null))
                        {
                            condition.ParseVersion();
                        }
                        if (object.Equals(this.m_version, condition.m_version))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public void FromXml(SecurityElement e)
        {
            this.FromXml(e, null);
        }

        public void FromXml(SecurityElement e, PolicyLevel level)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            if (!e.Tag.Equals("IMembershipCondition"))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MembershipConditionElement"));
            }
            lock (this)
            {
                this.m_name = null;
                this.m_publicKeyBlob = null;
                this.m_version = null;
                this.m_element = e;
            }
        }

        public override int GetHashCode()
        {
            if ((this.m_publicKeyBlob == null) && (this.m_element != null))
            {
                this.ParseKeyBlob();
            }
            if (this.m_publicKeyBlob != null)
            {
                return this.m_publicKeyBlob.GetHashCode();
            }
            if ((this.m_name == null) && (this.m_element != null))
            {
                this.ParseName();
            }
            if ((this.m_version == null) && (this.m_element != null))
            {
                this.ParseVersion();
            }
            if ((this.m_name != null) || (this.m_version != null))
            {
                return (((this.m_name == null) ? 0 : this.m_name.GetHashCode()) + ((this.m_version == null) ? 0 : this.m_version.GetHashCode()));
            }
            return typeof(StrongNameMembershipCondition).GetHashCode();
        }

        private void ParseKeyBlob()
        {
            lock (this)
            {
                if (this.m_element != null)
                {
                    string hexString = this.m_element.Attribute("PublicKeyBlob");
                    StrongNamePublicKeyBlob blob = new StrongNamePublicKeyBlob();
                    if (hexString == null)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_BlobCannotBeNull"));
                    }
                    blob.PublicKey = Hex.DecodeHexString(hexString);
                    this.m_publicKeyBlob = blob;
                    if (((this.m_version != null) && (this.m_name != null)) && (this.m_publicKeyBlob != null))
                    {
                        this.m_element = null;
                    }
                }
            }
        }

        private void ParseName()
        {
            lock (this)
            {
                if (this.m_element != null)
                {
                    string str = this.m_element.Attribute("Name");
                    this.m_name = (str == null) ? null : str;
                    if (((this.m_version != null) && (this.m_name != null)) && (this.m_publicKeyBlob != null))
                    {
                        this.m_element = null;
                    }
                }
            }
        }

        private void ParseVersion()
        {
            lock (this)
            {
                if (this.m_element != null)
                {
                    string version = this.m_element.Attribute("AssemblyVersion");
                    this.m_version = (version == null) ? null : new System.Version(version);
                    if (((this.m_version != null) && (this.m_name != null)) && (this.m_publicKeyBlob != null))
                    {
                        this.m_element = null;
                    }
                }
            }
        }

        bool IReportMatchMembershipCondition.Check(Evidence evidence, out object usedEvidence)
        {
            usedEvidence = null;
            if (evidence != null)
            {
                StrongName delayEvaluatedHostEvidence = evidence.GetDelayEvaluatedHostEvidence<StrongName>();
                if (delayEvaluatedHostEvidence != null)
                {
                    bool flag = (this.PublicKey != null) && this.PublicKey.Equals(delayEvaluatedHostEvidence.PublicKey);
                    bool flag2 = (this.Name == null) || ((delayEvaluatedHostEvidence.Name != null) && StrongName.CompareNames(delayEvaluatedHostEvidence.Name, this.Name));
                    bool flag3 = (this.Version == null) || ((delayEvaluatedHostEvidence.Version != null) && (delayEvaluatedHostEvidence.Version.CompareTo(this.Version) == 0));
                    if ((flag && flag2) && flag3)
                    {
                        usedEvidence = delayEvaluatedHostEvidence;
                        return true;
                    }
                }
            }
            return false;
        }

        public override string ToString()
        {
            string str = "";
            string str2 = "";
            if (this.Name != null)
            {
                str = " " + string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("StrongName_Name"), new object[] { this.Name });
            }
            if (this.Version != null)
            {
                str2 = " " + string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("StrongName_Version"), new object[] { this.Version });
            }
            return string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("StrongName_ToString"), new object[] { Hex.EncodeHexString(this.PublicKey.PublicKey), str, str2 });
        }

        public SecurityElement ToXml()
        {
            return this.ToXml(null);
        }

        public SecurityElement ToXml(PolicyLevel level)
        {
            SecurityElement element = new SecurityElement("IMembershipCondition");
            XMLUtil.AddClassAttribute(element, base.GetType(), "System.Security.Policy.StrongNameMembershipCondition");
            element.AddAttribute("version", "1");
            if (this.PublicKey != null)
            {
                element.AddAttribute("PublicKeyBlob", Hex.EncodeHexString(this.PublicKey.PublicKey));
            }
            if (this.Name != null)
            {
                element.AddAttribute("Name", this.Name);
            }
            if (this.Version != null)
            {
                element.AddAttribute("AssemblyVersion", this.Version.ToString());
            }
            return element;
        }

        public string Name
        {
            get
            {
                if ((this.m_name == null) && (this.m_element != null))
                {
                    this.ParseName();
                }
                return this.m_name;
            }
            set
            {
                if (value == null)
                {
                    if ((this.m_publicKeyBlob == null) && (this.m_element != null))
                    {
                        this.ParseKeyBlob();
                    }
                    if ((this.m_version == null) && (this.m_element != null))
                    {
                        this.ParseVersion();
                    }
                    this.m_element = null;
                }
                else if (value.Length == 0)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"));
                }
                this.m_name = value;
            }
        }

        public StrongNamePublicKeyBlob PublicKey
        {
            get
            {
                if ((this.m_publicKeyBlob == null) && (this.m_element != null))
                {
                    this.ParseKeyBlob();
                }
                return this.m_publicKeyBlob;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("PublicKey");
                }
                this.m_publicKeyBlob = value;
            }
        }

        public System.Version Version
        {
            get
            {
                if ((this.m_version == null) && (this.m_element != null))
                {
                    this.ParseVersion();
                }
                return this.m_version;
            }
            set
            {
                if (value == null)
                {
                    if ((this.m_name == null) && (this.m_element != null))
                    {
                        this.ParseName();
                    }
                    if ((this.m_publicKeyBlob == null) && (this.m_element != null))
                    {
                        this.ParseKeyBlob();
                    }
                    this.m_element = null;
                }
                this.m_version = value;
            }
        }
    }
}

