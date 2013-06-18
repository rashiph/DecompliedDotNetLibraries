namespace System.Data.OracleClient
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Data.Common;
    using System.Globalization;
    using System.Security;
    using System.Security.Permissions;

    [Serializable, Obsolete("OraclePermission has been deprecated. http://go.microsoft.com/fwlink/?LinkID=144260", false)]
    public sealed class OraclePermission : CodeAccessPermission, IUnrestrictedPermission
    {
        private bool _allowBlankPassword;
        private bool _isUnrestricted;
        private ArrayList _keyvalues;
        private System.Data.OracleClient.NameValuePermission _keyvaluetree;

        internal OraclePermission(OracleConnectionString connectionOptions)
        {
            this._keyvaluetree = System.Data.OracleClient.NameValuePermission.Default;
            if (connectionOptions != null)
            {
                this._allowBlankPassword = connectionOptions.HasBlankPassword;
                this.AddPermissionEntry(new System.Data.OracleClient.DBConnectionString(connectionOptions));
            }
        }

        private OraclePermission(OraclePermission permission)
        {
            this._keyvaluetree = System.Data.OracleClient.NameValuePermission.Default;
            if (permission == null)
            {
                throw System.Data.Common.ADP.ArgumentNull("permissionAttribute");
            }
            this.CopyFrom(permission);
        }

        internal OraclePermission(OraclePermissionAttribute permissionAttribute)
        {
            this._keyvaluetree = System.Data.OracleClient.NameValuePermission.Default;
            if (permissionAttribute == null)
            {
                throw System.Data.Common.ADP.ArgumentNull("permissionAttribute");
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

        public OraclePermission(PermissionState state)
        {
            this._keyvaluetree = System.Data.OracleClient.NameValuePermission.Default;
            if (state == PermissionState.Unrestricted)
            {
                this._isUnrestricted = true;
            }
            else
            {
                if (state != PermissionState.None)
                {
                    throw System.Data.Common.ADP.InvalidPermissionState(state);
                }
                this._isUnrestricted = false;
            }
        }

        public void Add(string connectionString, string restrictions, KeyRestrictionBehavior behavior)
        {
            System.Data.OracleClient.DBConnectionString entry = new System.Data.OracleClient.DBConnectionString(connectionString, restrictions, behavior, OracleConnectionString.GetParseSynonyms(), false);
            this.AddPermissionEntry(entry);
        }

        internal void AddPermissionEntry(System.Data.OracleClient.DBConnectionString entry)
        {
            if (this._keyvaluetree == null)
            {
                this._keyvaluetree = new System.Data.OracleClient.NameValuePermission();
            }
            if (this._keyvalues == null)
            {
                this._keyvalues = new ArrayList();
            }
            System.Data.OracleClient.NameValuePermission.AddEntry(this._keyvaluetree, this._keyvalues, entry);
            this._isUnrestricted = false;
        }

        private void Clear()
        {
            this._keyvaluetree = null;
            this._keyvalues = null;
        }

        public override IPermission Copy()
        {
            return new OraclePermission(this);
        }

        private void CopyFrom(OraclePermission permission)
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
                throw System.Data.Common.ADP.ArgumentNull("securityElement");
            }
            string tag = securityElement.Tag;
            if (!tag.Equals("Permission") && !tag.Equals("IPermission"))
            {
                throw System.Data.Common.ADP.NotAPermissionElement();
            }
            string str7 = securityElement.Attribute("version");
            if ((str7 != null) && !str7.Equals("1"))
            {
                throw System.Data.Common.ADP.InvalidXMLBadVersion();
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
                throw System.Data.Common.ADP.PermissionTypeMismatch();
            }
            if (this.IsUnrestricted())
            {
                return target.Copy();
            }
            OraclePermission permission2 = (OraclePermission) target;
            if (permission2.IsUnrestricted())
            {
                return this.Copy();
            }
            OraclePermission permission = (OraclePermission) permission2.Copy();
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
                throw System.Data.Common.ADP.PermissionTypeMismatch();
            }
            OraclePermission permission = target as OraclePermission;
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
                foreach (System.Data.OracleClient.DBConnectionString str in this._keyvalues)
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
                foreach (System.Data.OracleClient.DBConnectionString str2 in this._keyvalues)
                {
                    SecurityElement child = new SecurityElement("add");
                    string connectionString = str2.ConnectionString;
                    connectionString = this.EncodeXmlValue(connectionString);
                    if (!System.Data.Common.ADP.IsEmpty(connectionString))
                    {
                        child.AddAttribute("ConnectionString", connectionString);
                    }
                    connectionString = str2.Restrictions;
                    connectionString = this.EncodeXmlValue(connectionString);
                    if (connectionString == null)
                    {
                        connectionString = System.Data.Common.ADP.StrEmpty;
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
                throw System.Data.Common.ADP.PermissionTypeMismatch();
            }
            if (this.IsUnrestricted())
            {
                return this.Copy();
            }
            OraclePermission permission = (OraclePermission) target.Copy();
            if (!permission.IsUnrestricted())
            {
                permission._allowBlankPassword |= this.AllowBlankPassword;
                if (this._keyvalues != null)
                {
                    foreach (System.Data.OracleClient.DBConnectionString str in this._keyvalues)
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

