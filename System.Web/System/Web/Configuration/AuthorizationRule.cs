namespace System.Web.Configuration
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Configuration;
    using System.Globalization;
    using System.Security.Principal;
    using System.Web;
    using System.Web.Util;
    using System.Xml;

    public sealed class AuthorizationRule : ConfigurationElement
    {
        private AuthorizationRuleAction _Action;
        private bool _ActionModified;
        internal string _ActionString;
        private bool _AllUsersSpecified;
        private bool _AnonUserSpecified;
        private char[] _delimiters;
        private string _ElementName;
        private bool _Everyone;
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propRoles = new ConfigurationProperty("roles", typeof(CommaDelimitedStringCollection), null, s_PropConverter, null, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propUsers = new ConfigurationProperty("users", typeof(CommaDelimitedStringCollection), null, s_PropConverter, null, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propVerbs = new ConfigurationProperty("verbs", typeof(CommaDelimitedStringCollection), null, s_PropConverter, null, ConfigurationPropertyOptions.None);
        private CommaDelimitedStringCollection _Roles;
        private StringCollection _RolesExpanded;
        private const string _strAllUsersTag = "*";
        private const string _strAnonUserTag = "?";
        private CommaDelimitedStringCollection _Users;
        private StringCollection _UsersExpanded;
        private CommaDelimitedStringCollection _Verbs;
        private bool DataReady;
        private string machineName;
        private static readonly TypeConverter s_PropConverter = new CommaDelimitedStringCollectionConverter();

        static AuthorizationRule()
        {
            _properties.Add(_propVerbs);
            _properties.Add(_propUsers);
            _properties.Add(_propRoles);
        }

        internal AuthorizationRule()
        {
            this._Action = AuthorizationRuleAction.Allow;
            this._ActionString = AuthorizationRuleAction.Allow.ToString();
            this._ElementName = "allow";
            this._delimiters = new char[] { ',' };
        }

        public AuthorizationRule(AuthorizationRuleAction action) : this()
        {
            this.Action = action;
        }

        internal void AddRole(string role)
        {
            if (!string.IsNullOrEmpty(role))
            {
                role = role.ToLower(CultureInfo.InvariantCulture);
            }
            this.Roles.Add(role);
            this.RolesExpanded.Add(this.ExpandName(role));
        }

        internal void AddUser(string user)
        {
            if (!string.IsNullOrEmpty(user))
            {
                user = user.ToLower(CultureInfo.InvariantCulture);
            }
            this.Users.Add(user);
            this.UsersExpanded.Add(this.ExpandName(user));
        }

        private StringCollection CreateExpandedCollection(StringCollection collection)
        {
            StringCollection strings = new StringCollection();
            foreach (string str in collection)
            {
                string str2 = this.ExpandName(str);
                strings.Add(str2);
            }
            return strings;
        }

        public override bool Equals(object obj)
        {
            AuthorizationRule rule = obj as AuthorizationRule;
            bool flag = false;
            if (rule != null)
            {
                this.UpdateUsersRolesVerbs();
                flag = (((rule.Verbs.ToString() == this.Verbs.ToString()) && (rule.Roles.ToString() == this.Roles.ToString())) && (rule.Users.ToString() == this.Users.ToString())) && (rule.Action == this.Action);
            }
            return flag;
        }

        private void EvaluateData()
        {
            if (!this.DataReady)
            {
                if (this.Users.Count > 0)
                {
                    foreach (string str in this.Users)
                    {
                        if (str.Length > 1)
                        {
                            int num = str.IndexOfAny(new char[] { '*', '?' });
                            if (num >= 0)
                            {
                                object[] args = new object[] { str[num].ToString(CultureInfo.InvariantCulture) };
                                throw new ConfigurationErrorsException(System.Web.SR.GetString("Auth_rule_names_cant_contain_char", args));
                            }
                        }
                        if (str.Equals("*"))
                        {
                            this._AllUsersSpecified = true;
                        }
                        if (str.Equals("?"))
                        {
                            this._AnonUserSpecified = true;
                        }
                    }
                }
                if (this.Roles.Count > 0)
                {
                    foreach (string str2 in this.Roles)
                    {
                        if (str2.Length > 0)
                        {
                            int num2 = str2.IndexOfAny(new char[] { '*', '?' });
                            if (num2 >= 0)
                            {
                                object[] objArray2 = new object[] { str2[num2].ToString(CultureInfo.InvariantCulture) };
                                throw new ConfigurationErrorsException(System.Web.SR.GetString("Auth_rule_names_cant_contain_char", objArray2));
                            }
                        }
                    }
                }
                this._Everyone = this._AllUsersSpecified && (this.Verbs.Count == 0);
                this._RolesExpanded = this.CreateExpandedCollection(this.Roles);
                this._UsersExpanded = this.CreateExpandedCollection(this.Users);
                if ((this.Roles.Count == 0) && (this.Users.Count == 0))
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Auth_rule_must_specify_users_andor_roles"));
                }
                this.DataReady = true;
            }
        }

        private string ExpandName(string name)
        {
            string str = name;
            if (!System.Web.Util.StringUtil.StringStartsWith(name, @".\"))
            {
                return str;
            }
            if (this.machineName == null)
            {
                this.machineName = HttpServerUtility.GetMachineNameInternal().ToLower(CultureInfo.InvariantCulture);
            }
            return (this.machineName + name.Substring(1));
        }

        private bool FindUser(StringCollection users, string principal)
        {
            using (StringEnumerator enumerator = users.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (string.Equals(enumerator.Current, principal, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool FindVerb(string verb)
        {
            if (this.Verbs.Count < 1)
            {
                return true;
            }
            using (StringEnumerator enumerator = this.Verbs.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (string.Equals(enumerator.Current, verb, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            string str = this.Verbs.ToString();
            string str2 = this.Roles.ToString();
            string str3 = this.Users.ToString();
            if (str == null)
            {
                str = string.Empty;
            }
            if (str2 == null)
            {
                str2 = string.Empty;
            }
            if (str3 == null)
            {
                str3 = string.Empty;
            }
            return HashCodeCombiner.CombineHashCodes(str.GetHashCode(), str2.GetHashCode(), str3.GetHashCode(), (int) this.Action);
        }

        protected override bool IsModified()
        {
            this.UpdateUsersRolesVerbs();
            if ((!this._ActionModified && !base.IsModified()) && (!((CommaDelimitedStringCollection) this.Users).IsModified && !((CommaDelimitedStringCollection) this.Roles).IsModified))
            {
                return ((CommaDelimitedStringCollection) this.Verbs).IsModified;
            }
            return true;
        }

        private bool IsTheUserInAnyRole(StringCollection roles, IPrincipal principal)
        {
            if ((!HttpRuntime.DisableProcessRequestInApplicationTrust && (HttpRuntime.NamedPermissionSet != null)) && HttpRuntime.ProcessRequestInApplicationTrust)
            {
                HttpRuntime.NamedPermissionSet.PermitOnly();
            }
            foreach (string str in roles)
            {
                if (principal.IsInRole(str))
                {
                    return true;
                }
            }
            return false;
        }

        internal int IsUserAllowed(IPrincipal user, string verb)
        {
            this.EvaluateData();
            int num = (this.Action == AuthorizationRuleAction.Allow) ? 1 : -1;
            if (this.Everyone)
            {
                return num;
            }
            if (this.FindVerb(verb))
            {
                StringCollection usersExpanded;
                StringCollection rolesExpanded;
                if (this._AllUsersSpecified)
                {
                    return num;
                }
                if (this._AnonUserSpecified && !user.Identity.IsAuthenticated)
                {
                    return num;
                }
                if (user.Identity is WindowsIdentity)
                {
                    usersExpanded = this.UsersExpanded;
                    rolesExpanded = this.RolesExpanded;
                }
                else
                {
                    usersExpanded = this.Users;
                    rolesExpanded = this.Roles;
                }
                if ((usersExpanded.Count > 0) && this.FindUser(usersExpanded, user.Identity.Name))
                {
                    return num;
                }
                if ((rolesExpanded.Count > 0) && this.IsTheUserInAnyRole(rolesExpanded, user))
                {
                    return num;
                }
            }
            return 0;
        }

        protected override void PostDeserialize()
        {
            this.EvaluateData();
        }

        protected override void PreSerialize(XmlWriter writer)
        {
            this.EvaluateData();
        }

        protected override void Reset(ConfigurationElement parentElement)
        {
            AuthorizationRule rule = parentElement as AuthorizationRule;
            if (rule != null)
            {
                rule.UpdateUsersRolesVerbs();
            }
            base.Reset(parentElement);
            this.EvaluateData();
        }

        protected override void ResetModified()
        {
            this._ActionModified = false;
            base.ResetModified();
        }

        protected override bool SerializeElement(XmlWriter writer, bool serializeCollectionKey)
        {
            bool flag = false;
            this.UpdateUsersRolesVerbs();
            if (!base.SerializeElement(null, false))
            {
                return flag;
            }
            if (writer != null)
            {
                writer.WriteStartElement(this._ElementName);
                flag |= base.SerializeElement(writer, false);
                writer.WriteEndElement();
                return flag;
            }
            return (flag | base.SerializeElement(writer, false));
        }

        protected override void SetReadOnly()
        {
            ((CommaDelimitedStringCollection) this.Users).SetReadOnly();
            ((CommaDelimitedStringCollection) this.Roles).SetReadOnly();
            ((CommaDelimitedStringCollection) this.Verbs).SetReadOnly();
            base.SetReadOnly();
        }

        protected override void Unmerge(ConfigurationElement sourceElement, ConfigurationElement parentElement, ConfigurationSaveMode saveMode)
        {
            AuthorizationRule rule = parentElement as AuthorizationRule;
            AuthorizationRule rule2 = sourceElement as AuthorizationRule;
            if (rule != null)
            {
                rule.UpdateUsersRolesVerbs();
            }
            if (rule2 != null)
            {
                rule2.UpdateUsersRolesVerbs();
            }
            base.Unmerge(sourceElement, parentElement, saveMode);
        }

        private void UpdateUsersRolesVerbs()
        {
            CommaDelimitedStringCollection roles = (CommaDelimitedStringCollection) this.Roles;
            CommaDelimitedStringCollection users = (CommaDelimitedStringCollection) this.Users;
            CommaDelimitedStringCollection verbs = (CommaDelimitedStringCollection) this.Verbs;
            if (roles.IsModified)
            {
                this._RolesExpanded = null;
                base[_propRoles] = roles;
            }
            if (users.IsModified)
            {
                this._UsersExpanded = null;
                base[_propUsers] = users;
            }
            if (verbs.IsModified)
            {
                base[_propVerbs] = verbs;
            }
        }

        public AuthorizationRuleAction Action
        {
            get
            {
                return this._Action;
            }
            set
            {
                this._ElementName = value.ToString().ToLower(CultureInfo.InvariantCulture);
                this._Action = value;
                this._ActionString = this._Action.ToString();
                this._ActionModified = true;
            }
        }

        internal bool Everyone
        {
            get
            {
                return this._Everyone;
            }
        }

        internal bool IncludesAnonymous
        {
            get
            {
                this.EvaluateData();
                return (this._AnonUserSpecified && (this.Verbs.Count == 0));
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        [TypeConverter(typeof(CommaDelimitedStringCollectionConverter)), ConfigurationProperty("roles")]
        public StringCollection Roles
        {
            get
            {
                if (this._Roles == null)
                {
                    CommaDelimitedStringCollection strings = (CommaDelimitedStringCollection) base[_propRoles];
                    if (strings == null)
                    {
                        this._Roles = new CommaDelimitedStringCollection();
                    }
                    else
                    {
                        this._Roles = strings.Clone();
                    }
                    this._RolesExpanded = null;
                }
                return this._Roles;
            }
        }

        internal StringCollection RolesExpanded
        {
            get
            {
                if (this._RolesExpanded == null)
                {
                    this._RolesExpanded = this.CreateExpandedCollection(this.Roles);
                }
                return this._RolesExpanded;
            }
        }

        [TypeConverter(typeof(CommaDelimitedStringCollectionConverter)), ConfigurationProperty("users")]
        public StringCollection Users
        {
            get
            {
                if (this._Users == null)
                {
                    CommaDelimitedStringCollection strings = (CommaDelimitedStringCollection) base[_propUsers];
                    if (strings == null)
                    {
                        this._Users = new CommaDelimitedStringCollection();
                    }
                    else
                    {
                        this._Users = strings.Clone();
                    }
                    this._UsersExpanded = null;
                }
                return this._Users;
            }
        }

        internal StringCollection UsersExpanded
        {
            get
            {
                if (this._UsersExpanded == null)
                {
                    this._UsersExpanded = this.CreateExpandedCollection(this.Users);
                }
                return this._UsersExpanded;
            }
        }

        [ConfigurationProperty("verbs"), TypeConverter(typeof(CommaDelimitedStringCollectionConverter))]
        public StringCollection Verbs
        {
            get
            {
                if (this._Verbs == null)
                {
                    CommaDelimitedStringCollection strings = (CommaDelimitedStringCollection) base[_propVerbs];
                    if (strings == null)
                    {
                        this._Verbs = new CommaDelimitedStringCollection();
                    }
                    else
                    {
                        this._Verbs = strings.Clone();
                    }
                }
                return this._Verbs;
            }
        }
    }
}

