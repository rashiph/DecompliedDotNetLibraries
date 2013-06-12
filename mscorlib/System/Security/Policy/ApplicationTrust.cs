namespace System.Security.Policy
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Util;

    [Serializable, ComVisible(true)]
    public sealed class ApplicationTrust : EvidenceBase, ISecurityEncodable
    {
        private System.ApplicationIdentity m_appId;
        private bool m_appTrustedToRun;
        private SecurityElement m_elExtraInfo;
        private object m_extraInfo;
        private IList<StrongName> m_fullTrustAssemblies;
        [NonSerialized]
        private int m_grantSetSpecialFlags;
        private bool m_persist;
        private PolicyStatement m_psDefaultGrant;

        public ApplicationTrust() : this(new PermissionSet(PermissionState.None))
        {
        }

        public ApplicationTrust(System.ApplicationIdentity applicationIdentity) : this()
        {
            this.ApplicationIdentity = applicationIdentity;
        }

        internal ApplicationTrust(PermissionSet defaultGrantSet) : this(defaultGrantSet, new StrongName[0])
        {
        }

        public ApplicationTrust(PermissionSet defaultGrantSet, IEnumerable<StrongName> fullTrustAssemblies)
        {
            if (defaultGrantSet == null)
            {
                throw new ArgumentNullException("defaultGrantSet");
            }
            if (fullTrustAssemblies == null)
            {
                throw new ArgumentNullException("fullTrustAssemblies");
            }
            this.DefaultGrantSet = new PolicyStatement(defaultGrantSet);
            List<StrongName> list = new List<StrongName>();
            foreach (StrongName name in fullTrustAssemblies)
            {
                if (name == null)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_NullFullTrustAssembly"));
                }
                list.Add(new StrongName(name.PublicKey, name.Name, name.Version));
            }
            this.m_fullTrustAssemblies = list.AsReadOnly();
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override EvidenceBase Clone()
        {
            return base.Clone();
        }

        public void FromXml(SecurityElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (string.Compare(element.Tag, "ApplicationTrust", StringComparison.Ordinal) != 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidXML"));
            }
            this.m_appTrustedToRun = false;
            string strA = element.Attribute("TrustedToRun");
            if ((strA != null) && (string.Compare(strA, "true", StringComparison.Ordinal) == 0))
            {
                this.m_appTrustedToRun = true;
            }
            this.m_persist = false;
            string str2 = element.Attribute("Persist");
            if ((str2 != null) && (string.Compare(str2, "true", StringComparison.Ordinal) == 0))
            {
                this.m_persist = true;
            }
            this.m_appId = null;
            string applicationIdentityFullName = element.Attribute("FullName");
            if ((applicationIdentityFullName != null) && (applicationIdentityFullName.Length > 0))
            {
                this.m_appId = new System.ApplicationIdentity(applicationIdentityFullName);
            }
            this.m_psDefaultGrant = null;
            this.m_grantSetSpecialFlags = 0;
            SecurityElement element2 = element.SearchForChildByTag("DefaultGrant");
            if (element2 != null)
            {
                SecurityElement et = element2.SearchForChildByTag("PolicyStatement");
                if (et != null)
                {
                    PolicyStatement statement = new PolicyStatement(null);
                    statement.FromXml(et);
                    this.m_psDefaultGrant = statement;
                    this.m_grantSetSpecialFlags = SecurityManager.GetSpecialFlags(statement.PermissionSet, null);
                }
            }
            List<StrongName> list = new List<StrongName>();
            SecurityElement element4 = element.SearchForChildByTag("FullTrustAssemblies");
            if ((element4 != null) && (element4.InternalChildren != null))
            {
                IEnumerator enumerator = element4.Children.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    StrongName item = new StrongName();
                    item.FromXml(enumerator.Current as SecurityElement);
                    list.Add(item);
                }
            }
            this.m_fullTrustAssemblies = list.AsReadOnly();
            this.m_elExtraInfo = element.SearchForChildByTag("ExtraInfo");
        }

        private static object ObjectFromXml(SecurityElement elObject)
        {
            if (elObject.Attribute("class") != null)
            {
                ISecurityEncodable encodable = XMLUtil.CreateCodeGroup(elObject) as ISecurityEncodable;
                if (encodable != null)
                {
                    encodable.FromXml(elObject);
                    return encodable;
                }
            }
            MemoryStream serializationStream = new MemoryStream(Hex.DecodeHexString(elObject.Attribute("Data")));
            BinaryFormatter formatter = new BinaryFormatter();
            return formatter.Deserialize(serializationStream);
        }

        private static SecurityElement ObjectToXml(string tag, object obj)
        {
            ISecurityEncodable encodable = obj as ISecurityEncodable;
            if ((encodable != null) && !encodable.ToXml().Tag.Equals(tag))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidXML"));
            }
            MemoryStream serializationStream = new MemoryStream();
            new BinaryFormatter().Serialize(serializationStream, obj);
            byte[] sArray = serializationStream.ToArray();
            SecurityElement element = new SecurityElement(tag);
            element.AddAttribute("Data", Hex.EncodeHexString(sArray));
            return element;
        }

        public SecurityElement ToXml()
        {
            SecurityElement element = new SecurityElement("ApplicationTrust");
            element.AddAttribute("version", "1");
            if (this.m_appId != null)
            {
                element.AddAttribute("FullName", SecurityElement.Escape(this.m_appId.FullName));
            }
            if (this.m_appTrustedToRun)
            {
                element.AddAttribute("TrustedToRun", "true");
            }
            if (this.m_persist)
            {
                element.AddAttribute("Persist", "true");
            }
            if (this.m_psDefaultGrant != null)
            {
                SecurityElement child = new SecurityElement("DefaultGrant");
                child.AddChild(this.m_psDefaultGrant.ToXml());
                element.AddChild(child);
            }
            if (this.m_fullTrustAssemblies.Count > 0)
            {
                SecurityElement element3 = new SecurityElement("FullTrustAssemblies");
                foreach (StrongName name in this.m_fullTrustAssemblies)
                {
                    element3.AddChild(name.ToXml());
                }
                element.AddChild(element3);
            }
            if (this.ExtraInfo != null)
            {
                element.AddChild(ObjectToXml("ExtraInfo", this.ExtraInfo));
            }
            return element;
        }

        public System.ApplicationIdentity ApplicationIdentity
        {
            get
            {
                return this.m_appId;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(Environment.GetResourceString("Argument_InvalidAppId"));
                }
                this.m_appId = value;
            }
        }

        public PolicyStatement DefaultGrantSet
        {
            get
            {
                if (this.m_psDefaultGrant == null)
                {
                    return new PolicyStatement(new PermissionSet(PermissionState.None));
                }
                return this.m_psDefaultGrant;
            }
            set
            {
                if (value == null)
                {
                    this.m_psDefaultGrant = null;
                    this.m_grantSetSpecialFlags = 0;
                }
                else
                {
                    this.m_psDefaultGrant = value;
                    this.m_grantSetSpecialFlags = SecurityManager.GetSpecialFlags(this.m_psDefaultGrant.PermissionSet, null);
                }
            }
        }

        public object ExtraInfo
        {
            [SecuritySafeCritical]
            get
            {
                if (this.m_elExtraInfo != null)
                {
                    this.m_extraInfo = ObjectFromXml(this.m_elExtraInfo);
                    this.m_elExtraInfo = null;
                }
                return this.m_extraInfo;
            }
            set
            {
                this.m_elExtraInfo = null;
                this.m_extraInfo = value;
            }
        }

        public IList<StrongName> FullTrustAssemblies
        {
            get
            {
                return this.m_fullTrustAssemblies;
            }
        }

        public bool IsApplicationTrustedToRun
        {
            get
            {
                return this.m_appTrustedToRun;
            }
            set
            {
                this.m_appTrustedToRun = value;
            }
        }

        public bool Persist
        {
            get
            {
                return this.m_persist;
            }
            set
            {
                this.m_persist = value;
            }
        }
    }
}

