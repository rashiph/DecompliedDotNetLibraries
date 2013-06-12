namespace System.Security.Policy
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Util;
    using System.Text;

    [Serializable, ComVisible(true)]
    public sealed class PolicyStatement : ISecurityPolicyEncodable, ISecurityEncodable
    {
        internal PolicyStatementAttribute m_attributes;
        [NonSerialized]
        private List<IDelayEvaluatedEvidence> m_dependentEvidence;
        internal System.Security.PermissionSet m_permSet;

        internal PolicyStatement()
        {
            this.m_permSet = null;
            this.m_attributes = PolicyStatementAttribute.Nothing;
        }

        public PolicyStatement(System.Security.PermissionSet permSet) : this(permSet, PolicyStatementAttribute.Nothing)
        {
        }

        public PolicyStatement(System.Security.PermissionSet permSet, PolicyStatementAttribute attributes)
        {
            if (permSet == null)
            {
                this.m_permSet = new System.Security.PermissionSet(false);
            }
            else
            {
                this.m_permSet = permSet.Copy();
            }
            if (ValidProperties(attributes))
            {
                this.m_attributes = attributes;
            }
        }

        private PolicyStatement(System.Security.PermissionSet permSet, PolicyStatementAttribute attributes, bool copy)
        {
            if (permSet != null)
            {
                if (copy)
                {
                    this.m_permSet = permSet.Copy();
                }
                else
                {
                    this.m_permSet = permSet;
                }
            }
            else
            {
                this.m_permSet = new System.Security.PermissionSet(false);
            }
            this.m_attributes = attributes;
        }

        internal void AddDependentEvidence(IDelayEvaluatedEvidence dependentEvidence)
        {
            if (this.m_dependentEvidence == null)
            {
                this.m_dependentEvidence = new List<IDelayEvaluatedEvidence>();
            }
            this.m_dependentEvidence.Add(dependentEvidence);
        }

        public PolicyStatement Copy()
        {
            PolicyStatement statement = new PolicyStatement(this.m_permSet, this.Attributes, true);
            if (this.HasDependentEvidence)
            {
                statement.m_dependentEvidence = new List<IDelayEvaluatedEvidence>(this.m_dependentEvidence);
            }
            return statement;
        }

        [ComVisible(false)]
        public override bool Equals(object obj)
        {
            PolicyStatement statement = obj as PolicyStatement;
            if (statement == null)
            {
                return false;
            }
            if (this.m_attributes != statement.m_attributes)
            {
                return false;
            }
            if (!object.Equals(this.m_permSet, statement.m_permSet))
            {
                return false;
            }
            return true;
        }

        [SecuritySafeCritical]
        public void FromXml(SecurityElement et)
        {
            this.FromXml(et, null);
        }

        [SecuritySafeCritical]
        public void FromXml(SecurityElement et, PolicyLevel level)
        {
            this.FromXml(et, level, false);
        }

        [SecurityCritical]
        internal void FromXml(SecurityElement et, PolicyLevel level, bool allowInternalOnly)
        {
            if (et == null)
            {
                throw new ArgumentNullException("et");
            }
            if (!et.Tag.Equals("PolicyStatement"))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_InvalidXMLElement"), new object[] { "PolicyStatement", base.GetType().FullName }));
            }
            this.m_attributes = PolicyStatementAttribute.Nothing;
            string str = et.Attribute("Attributes");
            if (str != null)
            {
                this.m_attributes = (PolicyStatementAttribute) Enum.Parse(typeof(PolicyStatementAttribute), str);
            }
            lock (this)
            {
                this.m_permSet = null;
                if (level != null)
                {
                    string name = et.Attribute("PermissionSetName");
                    if (name != null)
                    {
                        this.m_permSet = level.GetNamedPermissionSetInternal(name);
                        if (this.m_permSet == null)
                        {
                            this.m_permSet = new System.Security.PermissionSet(PermissionState.None);
                        }
                    }
                }
                if (this.m_permSet == null)
                {
                    SecurityElement element = et.SearchForChildByTag("PermissionSet");
                    if (element == null)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_InvalidXML"));
                    }
                    string str3 = element.Attribute("class");
                    if ((str3 != null) && (str3.Equals("NamedPermissionSet") || str3.Equals("System.Security.NamedPermissionSet")))
                    {
                        this.m_permSet = new NamedPermissionSet("DefaultName", PermissionState.None);
                    }
                    else
                    {
                        this.m_permSet = new System.Security.PermissionSet(PermissionState.None);
                    }
                    try
                    {
                        this.m_permSet.FromXml(element, allowInternalOnly, true);
                    }
                    catch
                    {
                    }
                }
                if (this.m_permSet == null)
                {
                    this.m_permSet = new System.Security.PermissionSet(PermissionState.None);
                }
            }
        }

        [SecurityCritical]
        internal void FromXml(SecurityDocument doc, int position, PolicyLevel level, bool allowInternalOnly)
        {
            if (doc == null)
            {
                throw new ArgumentNullException("doc");
            }
            if (!doc.GetTagForElement(position).Equals("PolicyStatement"))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_InvalidXMLElement"), new object[] { "PolicyStatement", base.GetType().FullName }));
            }
            this.m_attributes = PolicyStatementAttribute.Nothing;
            string attributeForElement = doc.GetAttributeForElement(position, "Attributes");
            if (attributeForElement != null)
            {
                this.m_attributes = (PolicyStatementAttribute) Enum.Parse(typeof(PolicyStatementAttribute), attributeForElement);
            }
            lock (this)
            {
                this.m_permSet = null;
                if (level != null)
                {
                    string name = doc.GetAttributeForElement(position, "PermissionSetName");
                    if (name != null)
                    {
                        this.m_permSet = level.GetNamedPermissionSetInternal(name);
                        if (this.m_permSet == null)
                        {
                            this.m_permSet = new System.Security.PermissionSet(PermissionState.None);
                        }
                    }
                }
                if (this.m_permSet == null)
                {
                    ArrayList childrenPositionForElement = doc.GetChildrenPositionForElement(position);
                    int num = -1;
                    for (int i = 0; i < childrenPositionForElement.Count; i++)
                    {
                        if (doc.GetTagForElement((int) childrenPositionForElement[i]).Equals("PermissionSet"))
                        {
                            num = (int) childrenPositionForElement[i];
                        }
                    }
                    if (num == -1)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_InvalidXML"));
                    }
                    string str3 = doc.GetAttributeForElement(num, "class");
                    if ((str3 != null) && (str3.Equals("NamedPermissionSet") || str3.Equals("System.Security.NamedPermissionSet")))
                    {
                        this.m_permSet = new NamedPermissionSet("DefaultName", PermissionState.None);
                    }
                    else
                    {
                        this.m_permSet = new System.Security.PermissionSet(PermissionState.None);
                    }
                    this.m_permSet.FromXml(doc, num, allowInternalOnly);
                }
                if (this.m_permSet == null)
                {
                    this.m_permSet = new System.Security.PermissionSet(PermissionState.None);
                }
            }
        }

        private bool GetFlag(int flag)
        {
            return ((flag & this.m_attributes) != 0);
        }

        [ComVisible(false)]
        public override int GetHashCode()
        {
            int attributes = (int) this.m_attributes;
            if (this.m_permSet != null)
            {
                attributes ^= this.m_permSet.GetHashCode();
            }
            return attributes;
        }

        internal System.Security.PermissionSet GetPermissionSetNoCopy()
        {
            lock (this)
            {
                return this.m_permSet;
            }
        }

        internal void InplaceUnion(PolicyStatement childPolicy)
        {
            if (((this.Attributes & childPolicy.Attributes) & PolicyStatementAttribute.Exclusive) == PolicyStatementAttribute.Exclusive)
            {
                throw new PolicyException(Environment.GetResourceString("Policy_MultipleExclusive"));
            }
            if (childPolicy.HasDependentEvidence)
            {
                bool flag = this.m_permSet.IsSubsetOf(childPolicy.GetPermissionSetNoCopy()) && !childPolicy.GetPermissionSetNoCopy().IsSubsetOf(this.m_permSet);
                if (this.HasDependentEvidence || flag)
                {
                    if (this.m_dependentEvidence == null)
                    {
                        this.m_dependentEvidence = new List<IDelayEvaluatedEvidence>();
                    }
                    this.m_dependentEvidence.AddRange(childPolicy.DependentEvidence);
                }
            }
            if ((childPolicy.Attributes & PolicyStatementAttribute.Exclusive) == PolicyStatementAttribute.Exclusive)
            {
                this.m_permSet = childPolicy.GetPermissionSetNoCopy();
                this.Attributes = childPolicy.Attributes;
            }
            else
            {
                this.m_permSet.InplaceUnion(childPolicy.GetPermissionSetNoCopy());
                this.Attributes |= childPolicy.Attributes;
            }
        }

        internal void SetPermissionSetNoCopy(System.Security.PermissionSet permSet)
        {
            this.m_permSet = permSet;
        }

        [SecuritySafeCritical]
        public SecurityElement ToXml()
        {
            return this.ToXml(null);
        }

        [SecuritySafeCritical]
        public SecurityElement ToXml(PolicyLevel level)
        {
            return this.ToXml(level, false);
        }

        internal SecurityElement ToXml(PolicyLevel level, bool useInternal)
        {
            SecurityElement element = new SecurityElement("PolicyStatement");
            element.AddAttribute("version", "1");
            if (this.m_attributes != PolicyStatementAttribute.Nothing)
            {
                element.AddAttribute("Attributes", XMLUtil.BitFieldEnumToString(typeof(PolicyStatementAttribute), this.m_attributes));
            }
            lock (this)
            {
                if (this.m_permSet == null)
                {
                    return element;
                }
                if (this.m_permSet is NamedPermissionSet)
                {
                    NamedPermissionSet permSet = (NamedPermissionSet) this.m_permSet;
                    if ((level != null) && (level.GetNamedPermissionSet(permSet.Name) != null))
                    {
                        element.AddAttribute("PermissionSetName", permSet.Name);
                        return element;
                    }
                    if (useInternal)
                    {
                        element.AddChild(permSet.InternalToXml());
                        return element;
                    }
                    element.AddChild(permSet.ToXml());
                    return element;
                }
                if (useInternal)
                {
                    element.AddChild(this.m_permSet.InternalToXml());
                    return element;
                }
                element.AddChild(this.m_permSet.ToXml());
            }
            return element;
        }

        private static bool ValidProperties(PolicyStatementAttribute attributes)
        {
            if ((attributes & ~PolicyStatementAttribute.All) != PolicyStatementAttribute.Nothing)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidFlag"));
            }
            return true;
        }

        public PolicyStatementAttribute Attributes
        {
            get
            {
                return this.m_attributes;
            }
            set
            {
                if (ValidProperties(value))
                {
                    this.m_attributes = value;
                }
            }
        }

        public string AttributeString
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                bool flag = true;
                if (this.GetFlag(1))
                {
                    builder.Append("Exclusive");
                    flag = false;
                }
                if (this.GetFlag(2))
                {
                    if (!flag)
                    {
                        builder.Append(" ");
                    }
                    builder.Append("LevelFinal");
                }
                return builder.ToString();
            }
        }

        internal IEnumerable<IDelayEvaluatedEvidence> DependentEvidence
        {
            get
            {
                return this.m_dependentEvidence.AsReadOnly();
            }
        }

        internal bool HasDependentEvidence
        {
            get
            {
                return ((this.m_dependentEvidence != null) && (this.m_dependentEvidence.Count > 0));
            }
        }

        public System.Security.PermissionSet PermissionSet
        {
            get
            {
                lock (this)
                {
                    return this.m_permSet.Copy();
                }
            }
            set
            {
                lock (this)
                {
                    if (value == null)
                    {
                        this.m_permSet = new System.Security.PermissionSet(false);
                    }
                    else
                    {
                        this.m_permSet = value.Copy();
                    }
                }
            }
        }
    }
}

