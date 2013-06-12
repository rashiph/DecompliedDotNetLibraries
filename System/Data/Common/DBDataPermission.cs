namespace System.Data.Common
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Globalization;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;

    [Serializable, SecurityPermission(SecurityAction.InheritanceDemand, ControlEvidence=true, ControlPolicy=true)]
    public abstract class DBDataPermission : CodeAccessPermission, IUnrestrictedPermission
    {
        private bool _allowBlankPassword;
        private bool _isUnrestricted;
        private ArrayList _keyvalues;
        private NameValuePermission _keyvaluetree;

        [Obsolete("DBDataPermission() has been deprecated.  Use the DBDataPermission(PermissionState.None) constructor.  http://go.microsoft.com/fwlink/?linkid=14202", true)]
        protected DBDataPermission() : this(PermissionState.None)
        {
        }

        internal DBDataPermission(DbConnectionOptions connectionOptions)
        {
            this._keyvaluetree = NameValuePermission.Default;
            if (connectionOptions != null)
            {
                this._allowBlankPassword = connectionOptions.HasBlankPassword;
                this.AddPermissionEntry(new DBConnectionString(connectionOptions));
            }
        }

        protected DBDataPermission(DBDataPermission permission)
        {
            this._keyvaluetree = NameValuePermission.Default;
            if (permission == null)
            {
                throw ADP.ArgumentNull("permissionAttribute");
            }
            this.CopyFrom(permission);
        }

        protected DBDataPermission(DBDataPermissionAttribute permissionAttribute)
        {
            this._keyvaluetree = NameValuePermission.Default;
            if (permissionAttribute == null)
            {
                throw ADP.ArgumentNull("permissionAttribute");
            }
            this._isUnrestricted = permissionAttribute.Unrestricted;
            if (!this._isUnrestricted)
            {
                this._allowBlankPassword = permissionAttribute.AllowBlankPassword;
                if (permissionAttribute.ShouldSerializeConnectionString() || permissionAttribute.ShouldSerializeKeyRestrictions())
                {
                    this.Add(permissionAttribute.ConnectionString, permissionAttribute.KeyRestrictions, permissionAttribute.KeyRestrictionBehavior);
                }
            }
        }

        protected DBDataPermission(PermissionState state)
        {
            this._keyvaluetree = NameValuePermission.Default;
            if (state == PermissionState.Unrestricted)
            {
                this._isUnrestricted = true;
            }
            else
            {
                if (state != PermissionState.None)
                {
                    throw ADP.InvalidPermissionState(state);
                }
                this._isUnrestricted = false;
            }
        }

        [Obsolete("DBDataPermission(PermissionState state,Boolean allowBlankPassword) has been deprecated.  Use the DBDataPermission(PermissionState.None) constructor.  http://go.microsoft.com/fwlink/?linkid=14202", true)]
        protected DBDataPermission(PermissionState state, bool allowBlankPassword) : this(state)
        {
            this.AllowBlankPassword = allowBlankPassword;
        }

        public virtual void Add(string connectionString, string restrictions, KeyRestrictionBehavior behavior)
        {
            DBConnectionString entry = new DBConnectionString(connectionString, restrictions, behavior, null, false);
            this.AddPermissionEntry(entry);
        }

        internal void AddPermissionEntry(DBConnectionString entry)
        {
            if (this._keyvaluetree == null)
            {
                this._keyvaluetree = new NameValuePermission();
            }
            if (this._keyvalues == null)
            {
                this._keyvalues = new ArrayList();
            }
            NameValuePermission.AddEntry(this._keyvaluetree, this._keyvalues, entry);
            this._isUnrestricted = false;
        }

        protected void Clear()
        {
            this._keyvaluetree = null;
            this._keyvalues = null;
        }

        public override IPermission Copy()
        {
            DBDataPermission permission = this.CreateInstance();
            permission.CopyFrom(this);
            return permission;
        }

        private void CopyFrom(DBDataPermission permission)
        {
            this._isUnrestricted = permission.IsUnrestricted();
            if (!this._isUnrestricted)
            {
                this._allowBlankPassword = permission.AllowBlankPassword;
                if (permission._keyvalues != null)
                {
                    this._keyvalues = (ArrayList) permission._keyvalues.Clone();
                    if (permission._keyvaluetree != null)
                    {
                        this._keyvaluetree = permission._keyvaluetree.CopyNameValue();
                    }
                }
            }
        }

        [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        protected virtual DBDataPermission CreateInstance()
        {
            return (Activator.CreateInstance(base.GetType(), BindingFlags.Public | BindingFlags.Instance, null, null, CultureInfo.InvariantCulture, null) as DBDataPermission);
        }

        private string DecodeXmlValue(string value)
        {
            if ((value != null) && (0 < value.Length))
            {
                value = value.Replace("&quot;", "\"");
                value = value.Replace("&apos;", "'");
                value = value.Replace("&lt;", "<");
                value = value.Replace("&gt;", ">");
                value = value.Replace("&amp;", "&");
            }
            return value;
        }

        private string EncodeXmlValue(string value)
        {
            if ((value != null) && (0 < value.Length))
            {
                value = value.Replace('\0', ' ');
                value = value.Trim();
                value = value.Replace("&", "&amp;");
                value = value.Replace(">", "&gt;");
                value = value.Replace("<", "&lt;");
                value = value.Replace("'", "&apos;");
                value = value.Replace("\"", "&quot;");
            }
            return value;
        }

        public override void FromXml(SecurityElement securityElement)
        {
            if (securityElement == null)
            {
                throw ADP.ArgumentNull("securityElement");
            }
            string tag = securityElement.Tag;
            if (!tag.Equals("Permission") && !tag.Equals("IPermission"))
            {
                throw ADP.NotAPermissionElement();
            }
            string str7 = securityElement.Attribute("version");
            if ((str7 != null) && !str7.Equals("1"))
            {
                throw ADP.InvalidXMLBadVersion();
            }
            string str6 = securityElement.Attribute("Unrestricted");
            this._isUnrestricted = (str6 != null) && bool.Parse(str6);
            this.Clear();
            if (!this._isUnrestricted)
            {
                string str5 = securityElement.Attribute("AllowBlankPassword");
                this._allowBlankPassword = (str5 != null) && bool.Parse(str5);
                ArrayList children = securityElement.Children;
                if (children != null)
                {
                    foreach (SecurityElement element in children)
                    {
                        tag = element.Tag;
                        if (("add" == tag) || ((tag != null) && ("add" == tag.ToLower(CultureInfo.InvariantCulture))))
                        {
                            string str3 = element.Attribute("ConnectionString");
                            string str2 = element.Attribute("KeyRestrictions");
                            string str4 = element.Attribute("KeyRestrictionBehavior");
                            KeyRestrictionBehavior allowOnly = KeyRestrictionBehavior.AllowOnly;
                            if (str4 != null)
                            {
                                allowOnly = (KeyRestrictionBehavior) Enum.Parse(typeof(KeyRestrictionBehavior), str4, true);
                            }
                            str3 = this.DecodeXmlValue(str3);
                            str2 = this.DecodeXmlValue(str2);
                            this.Add(str3, str2, allowOnly);
                        }
                    }
                }
            }
            else
            {
                this._allowBlankPassword = false;
            }
        }

        public override IPermission Intersect(IPermission target)
        {
            if (target == null)
            {
                return null;
            }
            if (target.GetType() != base.GetType())
            {
                throw ADP.PermissionTypeMismatch();
            }
            if (this.IsUnrestricted())
            {
                return target.Copy();
            }
            DBDataPermission permission2 = (DBDataPermission) target;
            if (permission2.IsUnrestricted())
            {
                return this.Copy();
            }
            DBDataPermission permission = (DBDataPermission) permission2.Copy();
            permission._allowBlankPassword &= this.AllowBlankPassword;
            if ((this._keyvalues != null) && (permission._keyvalues != null))
            {
                permission._keyvalues.Clear();
                permission._keyvaluetree.Intersect(permission._keyvalues, this._keyvaluetree);
            }
            else
            {
                permission._keyvalues = null;
                permission._keyvaluetree = null;
            }
            if (permission.IsEmpty())
            {
                permission = null;
            }
            return permission;
        }

        private bool IsEmpty()
        {
            ArrayList list = this._keyvalues;
            return ((!this.IsUnrestricted() && !this.AllowBlankPassword) && ((list == null) || (0 == list.Count)));
        }

        public override bool IsSubsetOf(IPermission target)
        {
            if (target == null)
            {
                return this.IsEmpty();
            }
            if (target.GetType() != base.GetType())
            {
                throw ADP.PermissionTypeMismatch();
            }
            DBDataPermission permission = target as DBDataPermission;
            bool flag = permission.IsUnrestricted();
            if ((!flag && !this.IsUnrestricted()) && (!this.AllowBlankPassword || permission.AllowBlankPassword))
            {
                if ((this._keyvalues != null) && (permission._keyvaluetree == null))
                {
                    return flag;
                }
                flag = true;
                if (this._keyvalues == null)
                {
                    return flag;
                }
                foreach (DBConnectionString str in this._keyvalues)
                {
                    if (!permission._keyvaluetree.CheckValueForKeyPermit(str))
                    {
                        return false;
                    }
                }
            }
            return flag;
        }

        public bool IsUnrestricted()
        {
            return this._isUnrestricted;
        }

        public override SecurityElement ToXml()
        {
            Type type = base.GetType();
            SecurityElement element = new SecurityElement("IPermission");
            element.AddAttribute("class", type.AssemblyQualifiedName.Replace('"', '\''));
            element.AddAttribute("version", "1");
            if (this.IsUnrestricted())
            {
                element.AddAttribute("Unrestricted", "true");
                return element;
            }
            element.AddAttribute("AllowBlankPassword", this._allowBlankPassword.ToString(CultureInfo.InvariantCulture));
            if (this._keyvalues != null)
            {
                foreach (DBConnectionString str2 in this._keyvalues)
                {
                    SecurityElement child = new SecurityElement("add");
                    string connectionString = str2.ConnectionString;
                    connectionString = this.EncodeXmlValue(connectionString);
                    if (!ADP.IsEmpty(connectionString))
                    {
                        child.AddAttribute("ConnectionString", connectionString);
                    }
                    connectionString = str2.Restrictions;
                    connectionString = this.EncodeXmlValue(connectionString);
                    if (connectionString == null)
                    {
                        connectionString = ADP.StrEmpty;
                    }
                    child.AddAttribute("KeyRestrictions", connectionString);
                    connectionString = str2.Behavior.ToString();
                    child.AddAttribute("KeyRestrictionBehavior", connectionString);
                    element.AddChild(child);
                }
            }
            return element;
        }

        public override IPermission Union(IPermission target)
        {
            if (target == null)
            {
                return this.Copy();
            }
            if (target.GetType() != base.GetType())
            {
                throw ADP.PermissionTypeMismatch();
            }
            if (this.IsUnrestricted())
            {
                return this.Copy();
            }
            DBDataPermission permission = (DBDataPermission) target.Copy();
            if (!permission.IsUnrestricted())
            {
                permission._allowBlankPassword |= this.AllowBlankPassword;
                if (this._keyvalues != null)
                {
                    foreach (DBConnectionString str in this._keyvalues)
                    {
                        permission.AddPermissionEntry(str);
                    }
                }
            }
            if (!permission.IsEmpty())
            {
                return permission;
            }
            return null;
        }

        public bool AllowBlankPassword
        {
            get
            {
                return this._allowBlankPassword;
            }
            set
            {
                this._allowBlankPassword = value;
            }
        }
    }
}

