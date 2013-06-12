namespace System.Security.Policy
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Util;

    [Serializable, ComVisible(true)]
    public abstract class CodeGroup
    {
        private IList m_children;
        private string m_description;
        private SecurityElement m_element;
        private IMembershipCondition m_membershipCondition;
        private string m_name;
        private PolicyLevel m_parentLevel;
        private System.Security.Policy.PolicyStatement m_policy;

        internal CodeGroup()
        {
        }

        internal CodeGroup(IMembershipCondition membershipCondition, PermissionSet permSet)
        {
            this.m_membershipCondition = membershipCondition;
            this.m_policy = new System.Security.Policy.PolicyStatement();
            this.m_policy.SetPermissionSetNoCopy(permSet);
            this.m_children = ArrayList.Synchronized(new ArrayList());
            this.m_element = null;
            this.m_parentLevel = null;
        }

        protected CodeGroup(IMembershipCondition membershipCondition, System.Security.Policy.PolicyStatement policy)
        {
            if (membershipCondition == null)
            {
                throw new ArgumentNullException("membershipCondition");
            }
            if (policy == null)
            {
                this.m_policy = null;
            }
            else
            {
                this.m_policy = policy.Copy();
            }
            this.m_membershipCondition = membershipCondition.Copy();
            this.m_children = ArrayList.Synchronized(new ArrayList());
            this.m_element = null;
            this.m_parentLevel = null;
        }

        [SecuritySafeCritical]
        public void AddChild(CodeGroup group)
        {
            if (group == null)
            {
                throw new ArgumentNullException("group");
            }
            if (this.m_children == null)
            {
                this.ParseChildren();
            }
            lock (this)
            {
                this.m_children.Add(group.Copy());
            }
        }

        [SecurityCritical]
        internal void AddChildInternal(CodeGroup group)
        {
            if (group == null)
            {
                throw new ArgumentNullException("group");
            }
            if (this.m_children == null)
            {
                this.ParseChildren();
            }
            lock (this)
            {
                this.m_children.Add(group);
            }
        }

        public abstract CodeGroup Copy();
        protected virtual void CreateXml(SecurityElement element, PolicyLevel level)
        {
        }

        [SecuritySafeCritical]
        public override bool Equals(object o)
        {
            CodeGroup group = o as CodeGroup;
            if (((group != null) && base.GetType().Equals(group.GetType())) && (object.Equals(this.m_name, group.m_name) && object.Equals(this.m_description, group.m_description)))
            {
                if ((this.m_membershipCondition == null) && (this.m_element != null))
                {
                    this.ParseMembershipCondition();
                }
                if ((group.m_membershipCondition == null) && (group.m_element != null))
                {
                    group.ParseMembershipCondition();
                }
                if (object.Equals(this.m_membershipCondition, group.m_membershipCondition))
                {
                    return true;
                }
            }
            return false;
        }

        [SecuritySafeCritical]
        public bool Equals(CodeGroup cg, bool compareChildren)
        {
            if (!this.Equals(cg))
            {
                return false;
            }
            if (compareChildren)
            {
                if (this.m_children == null)
                {
                    this.ParseChildren();
                }
                if (cg.m_children == null)
                {
                    cg.ParseChildren();
                }
                ArrayList list = new ArrayList(this.m_children);
                ArrayList list2 = new ArrayList(cg.m_children);
                if (list.Count != list2.Count)
                {
                    return false;
                }
                for (int i = 0; i < list.Count; i++)
                {
                    if (!((CodeGroup) list[i]).Equals((CodeGroup) list2[i], true))
                    {
                        return false;
                    }
                }
            }
            return true;
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
            lock (this)
            {
                this.m_element = e;
                this.m_parentLevel = level;
                this.m_children = null;
                this.m_membershipCondition = null;
                this.m_policy = null;
                this.m_name = e.Attribute("Name");
                this.m_description = e.Attribute("Description");
                this.ParseXml(e, level);
            }
        }

        [SecurityCritical]
        internal IList GetChildrenInternal()
        {
            if (this.m_children == null)
            {
                this.ParseChildren();
            }
            return this.m_children;
        }

        [SecuritySafeCritical]
        public override int GetHashCode()
        {
            if ((this.m_membershipCondition == null) && (this.m_element != null))
            {
                this.ParseMembershipCondition();
            }
            if ((this.m_name != null) || (this.m_membershipCondition != null))
            {
                return (((this.m_name == null) ? 0 : this.m_name.GetHashCode()) + ((this.m_membershipCondition == null) ? 0 : this.m_membershipCondition.GetHashCode()));
            }
            return base.GetType().GetHashCode();
        }

        internal virtual string GetTypeName()
        {
            return base.GetType().FullName;
        }

        [SecurityCritical]
        internal void ParseChildren()
        {
            lock (this)
            {
                ArrayList list = ArrayList.Synchronized(new ArrayList());
                if ((this.m_element != null) && (this.m_element.InternalChildren != null))
                {
                    this.m_element.Children = (ArrayList) this.m_element.InternalChildren.Clone();
                    ArrayList list2 = ArrayList.Synchronized(new ArrayList());
                    Evidence evidence = new Evidence();
                    int count = this.m_element.InternalChildren.Count;
                    int index = 0;
                    while (index < count)
                    {
                        SecurityElement el = (SecurityElement) this.m_element.Children[index];
                        if (el.Tag.Equals("CodeGroup"))
                        {
                            CodeGroup group = XMLUtil.CreateCodeGroup(el);
                            if (group != null)
                            {
                                group.FromXml(el, this.m_parentLevel);
                                if (this.ParseMembershipCondition(true))
                                {
                                    group.Resolve(evidence);
                                    group.MembershipCondition.Check(evidence);
                                    list.Add(group);
                                    index++;
                                }
                                else
                                {
                                    this.m_element.InternalChildren.RemoveAt(index);
                                    count = this.m_element.InternalChildren.Count;
                                    list2.Add(new CodeGroupPositionMarker(index, list.Count, el));
                                }
                            }
                            else
                            {
                                this.m_element.InternalChildren.RemoveAt(index);
                                count = this.m_element.InternalChildren.Count;
                                list2.Add(new CodeGroupPositionMarker(index, list.Count, el));
                            }
                        }
                        else
                        {
                            index++;
                        }
                    }
                    IEnumerator enumerator = list2.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        CodeGroupPositionMarker current = (CodeGroupPositionMarker) enumerator.Current;
                        CodeGroup group2 = XMLUtil.CreateCodeGroup(current.element);
                        if (group2 == null)
                        {
                            throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_FailedCodeGroup"), new object[] { current.element.Attribute("class") }));
                        }
                        group2.FromXml(current.element, this.m_parentLevel);
                        group2.Resolve(evidence);
                        group2.MembershipCondition.Check(evidence);
                        list.Insert(current.groupIndex, group2);
                        this.m_element.InternalChildren.Insert(current.elementIndex, current.element);
                    }
                }
                this.m_children = list;
            }
        }

        [SecurityCritical]
        private void ParseMembershipCondition()
        {
            this.ParseMembershipCondition(false);
        }

        [SecurityCritical]
        private bool ParseMembershipCondition(bool safeLoad)
        {
            lock (this)
            {
                IMembershipCondition condition = null;
                SecurityElement el = this.m_element.SearchForChildByTag("IMembershipCondition");
                if (el == null)
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_InvalidXMLElement"), new object[] { "IMembershipCondition", base.GetType().FullName }));
                }
                try
                {
                    condition = XMLUtil.CreateMembershipCondition(el);
                    if (condition == null)
                    {
                        return false;
                    }
                }
                catch (Exception exception)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_MembershipConditionElement"), exception);
                }
                condition.FromXml(el, this.m_parentLevel);
                this.m_membershipCondition = condition;
                return true;
            }
        }

        private void ParsePolicy()
        {
            System.Security.Policy.PolicyStatement statement;
        Label_0000:
            statement = new System.Security.Policy.PolicyStatement();
            bool flag = false;
            SecurityElement et = new SecurityElement("PolicyStatement");
            et.AddAttribute("version", "1");
            SecurityElement element = this.m_element;
            lock (this)
            {
                if (this.m_element != null)
                {
                    string str = this.m_element.Attribute("PermissionSetName");
                    if (str != null)
                    {
                        et.AddAttribute("PermissionSetName", str);
                        flag = true;
                    }
                    else
                    {
                        SecurityElement child = this.m_element.SearchForChildByTag("PermissionSet");
                        if (child != null)
                        {
                            et.AddChild(child);
                            flag = true;
                        }
                        else
                        {
                            et.AddChild(new PermissionSet(false).ToXml());
                            flag = true;
                        }
                    }
                    string str2 = this.m_element.Attribute("Attributes");
                    if (str2 != null)
                    {
                        et.AddAttribute("Attributes", str2);
                        flag = true;
                    }
                }
            }
            if (flag)
            {
                statement.FromXml(et, this.m_parentLevel);
            }
            else
            {
                statement.PermissionSet = null;
            }
            lock (this)
            {
                if ((element == this.m_element) && (this.m_policy == null))
                {
                    this.m_policy = statement;
                }
                else if (this.m_policy == null)
                {
                    goto Label_0000;
                }
            }
            if ((this.m_policy != null) && (this.m_children != null))
            {
                IMembershipCondition membershipCondition = this.m_membershipCondition;
            }
        }

        protected virtual void ParseXml(SecurityElement e, PolicyLevel level)
        {
        }

        [SecuritySafeCritical]
        public void RemoveChild(CodeGroup group)
        {
            if (group != null)
            {
                if (this.m_children == null)
                {
                    this.ParseChildren();
                }
                lock (this)
                {
                    int index = this.m_children.IndexOf(group);
                    if (index != -1)
                    {
                        this.m_children.RemoveAt(index);
                    }
                }
            }
        }

        public abstract System.Security.Policy.PolicyStatement Resolve(Evidence evidence);
        public abstract CodeGroup ResolveMatchingCodeGroups(Evidence evidence);
        [SecuritySafeCritical]
        public SecurityElement ToXml()
        {
            return this.ToXml(null);
        }

        [SecuritySafeCritical]
        public SecurityElement ToXml(PolicyLevel level)
        {
            return this.ToXml(level, this.GetTypeName());
        }

        [SecurityCritical]
        internal SecurityElement ToXml(PolicyLevel level, string policyClassName)
        {
            if ((this.m_membershipCondition == null) && (this.m_element != null))
            {
                this.ParseMembershipCondition();
            }
            if (this.m_children == null)
            {
                this.ParseChildren();
            }
            if ((this.m_policy == null) && (this.m_element != null))
            {
                this.ParsePolicy();
            }
            SecurityElement element = new SecurityElement("CodeGroup");
            XMLUtil.AddClassAttribute(element, base.GetType(), policyClassName);
            element.AddAttribute("version", "1");
            element.AddChild(this.m_membershipCondition.ToXml(level));
            if (this.m_policy != null)
            {
                PermissionSet permissionSetNoCopy = this.m_policy.GetPermissionSetNoCopy();
                NamedPermissionSet set2 = permissionSetNoCopy as NamedPermissionSet;
                if (((set2 != null) && (level != null)) && (level.GetNamedPermissionSetInternal(set2.Name) != null))
                {
                    element.AddAttribute("PermissionSetName", set2.Name);
                }
                else if (!permissionSetNoCopy.IsEmpty())
                {
                    element.AddChild(permissionSetNoCopy.ToXml());
                }
                if (this.m_policy.Attributes != PolicyStatementAttribute.Nothing)
                {
                    element.AddAttribute("Attributes", XMLUtil.BitFieldEnumToString(typeof(PolicyStatementAttribute), this.m_policy.Attributes));
                }
            }
            if (this.m_children.Count > 0)
            {
                lock (this)
                {
                    IEnumerator enumerator = this.m_children.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        element.AddChild(((CodeGroup) enumerator.Current).ToXml(level));
                    }
                }
            }
            if (this.m_name != null)
            {
                element.AddAttribute("Name", SecurityElement.Escape(this.m_name));
            }
            if (this.m_description != null)
            {
                element.AddAttribute("Description", SecurityElement.Escape(this.m_description));
            }
            this.CreateXml(element, level);
            return element;
        }

        public virtual string AttributeString
        {
            [SecuritySafeCritical]
            get
            {
                if ((this.m_policy == null) && (this.m_element != null))
                {
                    this.ParsePolicy();
                }
                if (this.m_policy != null)
                {
                    return this.m_policy.AttributeString;
                }
                return null;
            }
        }

        public IList Children
        {
            [SecuritySafeCritical]
            get
            {
                if (this.m_children == null)
                {
                    this.ParseChildren();
                }
                lock (this)
                {
                    IList list = new ArrayList(this.m_children.Count);
                    IEnumerator enumerator = this.m_children.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        list.Add(((CodeGroup) enumerator.Current).Copy());
                    }
                    return list;
                }
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Children");
                }
                ArrayList list = ArrayList.Synchronized(new ArrayList(value.Count));
                IEnumerator enumerator = value.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    CodeGroup current = enumerator.Current as CodeGroup;
                    if (current == null)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_CodeGroupChildrenMustBeCodeGroups"));
                    }
                    list.Add(current.Copy());
                }
                this.m_children = list;
            }
        }

        public string Description
        {
            get
            {
                return this.m_description;
            }
            set
            {
                this.m_description = value;
            }
        }

        public IMembershipCondition MembershipCondition
        {
            [SecuritySafeCritical]
            get
            {
                if ((this.m_membershipCondition == null) && (this.m_element != null))
                {
                    this.ParseMembershipCondition();
                }
                return this.m_membershipCondition.Copy();
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("MembershipCondition");
                }
                this.m_membershipCondition = value.Copy();
            }
        }

        public abstract string MergeLogic { get; }

        public string Name
        {
            get
            {
                return this.m_name;
            }
            set
            {
                this.m_name = value;
            }
        }

        public virtual string PermissionSetName
        {
            [SecuritySafeCritical]
            get
            {
                if ((this.m_policy == null) && (this.m_element != null))
                {
                    this.ParsePolicy();
                }
                if (this.m_policy != null)
                {
                    NamedPermissionSet permissionSetNoCopy = this.m_policy.GetPermissionSetNoCopy() as NamedPermissionSet;
                    if (permissionSetNoCopy != null)
                    {
                        return permissionSetNoCopy.Name;
                    }
                }
                return null;
            }
        }

        public System.Security.Policy.PolicyStatement PolicyStatement
        {
            [SecuritySafeCritical]
            get
            {
                if ((this.m_policy == null) && (this.m_element != null))
                {
                    this.ParsePolicy();
                }
                if (this.m_policy != null)
                {
                    return this.m_policy.Copy();
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    this.m_policy = value.Copy();
                }
                else
                {
                    this.m_policy = null;
                }
            }
        }
    }
}

