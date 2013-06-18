namespace System.Web.Security
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration.Provider;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Text;
    using System.Web;
    using System.Web.Hosting;
    using System.Web.Util;

    [Serializable]
    public class RolePrincipal : IPrincipal, ISerializable
    {
        private bool _CachedListChanged;
        private DateTime _ExpireDate;
        [NonSerialized]
        private bool _GetRolesCalled;
        private IIdentity _Identity;
        private bool _IsRoleListCached;
        private DateTime _IssueDate;
        private string _ProviderName;
        [NonSerialized]
        private HybridDictionary _Roles;
        private string _Username;
        private int _Version;

        public RolePrincipal(IIdentity identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
            this._Identity = identity;
            this.Init();
        }

        protected RolePrincipal(SerializationInfo info, StreamingContext context)
        {
            this._Version = info.GetInt32("_Version");
            this._ExpireDate = info.GetDateTime("_ExpireDate");
            this._IssueDate = info.GetDateTime("_IssueDate");
            try
            {
                this._Identity = info.GetValue("_Identity", typeof(IIdentity)) as IIdentity;
            }
            catch
            {
            }
            this._ProviderName = info.GetString("_ProviderName");
            this._Username = info.GetString("_Username");
            this._IsRoleListCached = info.GetBoolean("_IsRoleListCached");
            this._Roles = new HybridDictionary(true);
            string str = info.GetString("_AllRoles");
            if (str != null)
            {
                foreach (string str2 in str.Split(new char[] { ',' }))
                {
                    if (this._Roles[str2] == null)
                    {
                        this._Roles.Add(str2, string.Empty);
                    }
                }
            }
        }

        public RolePrincipal(IIdentity identity, string encryptedTicket)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
            if (encryptedTicket == null)
            {
                throw new ArgumentNullException("encryptedTicket");
            }
            this._Identity = identity;
            this._ProviderName = Roles.Provider.Name;
            if (identity.IsAuthenticated)
            {
                this.InitFromEncryptedTicket(encryptedTicket);
            }
            else
            {
                this.Init();
            }
        }

        public RolePrincipal(string providerName, IIdentity identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
            if (providerName == null)
            {
                throw new ArgumentException(System.Web.SR.GetString("Role_provider_name_invalid"), "providerName");
            }
            this._ProviderName = providerName;
            if (Roles.Providers[providerName] == null)
            {
                throw new ArgumentException(System.Web.SR.GetString("Role_provider_name_invalid"), "providerName");
            }
            this._Identity = identity;
            this.Init();
        }

        public RolePrincipal(string providerName, IIdentity identity, string encryptedTicket)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
            if (encryptedTicket == null)
            {
                throw new ArgumentNullException("encryptedTicket");
            }
            if (providerName == null)
            {
                throw new ArgumentException(System.Web.SR.GetString("Role_provider_name_invalid"), "providerName");
            }
            this._ProviderName = providerName;
            if (Roles.Providers[this._ProviderName] == null)
            {
                throw new ArgumentException(System.Web.SR.GetString("Role_provider_name_invalid"), "providerName");
            }
            this._Identity = identity;
            if (identity.IsAuthenticated)
            {
                this.InitFromEncryptedTicket(encryptedTicket);
            }
            else
            {
                this.Init();
            }
        }

        protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("_Version", this._Version);
            info.AddValue("_ExpireDate", this._ExpireDate);
            info.AddValue("_IssueDate", this._IssueDate);
            try
            {
                info.AddValue("_Identity", this._Identity);
            }
            catch
            {
            }
            info.AddValue("_ProviderName", this._ProviderName);
            info.AddValue("_Username", (this._Identity == null) ? this._Username : this._Identity.Name);
            info.AddValue("_IsRoleListCached", this._IsRoleListCached);
            if (this._Roles.Count > 0)
            {
                StringBuilder builder = new StringBuilder(this._Roles.Count * 10);
                foreach (object obj2 in this._Roles.Keys)
                {
                    builder.Append(((string) obj2) + ",");
                }
                string str = builder.ToString();
                info.AddValue("_AllRoles", str.Substring(0, str.Length - 1));
            }
            else
            {
                info.AddValue("_AllRoles", string.Empty);
            }
        }

        public string[] GetRoles()
        {
            string[] rolesForUser;
            if (this._Identity == null)
            {
                throw new ProviderException(System.Web.SR.GetString("Role_Principal_not_fully_constructed"));
            }
            if (!this._Identity.IsAuthenticated)
            {
                return new string[0];
            }
            if (!this._IsRoleListCached || !this._GetRolesCalled)
            {
                this._Roles.Clear();
                rolesForUser = Roles.Providers[this._ProviderName].GetRolesForUser(this.Identity.Name);
                foreach (string str in rolesForUser)
                {
                    if (this._Roles[str] == null)
                    {
                        this._Roles.Add(str, string.Empty);
                    }
                }
                this._IsRoleListCached = true;
                this._CachedListChanged = true;
                this._GetRolesCalled = true;
                return rolesForUser;
            }
            rolesForUser = new string[this._Roles.Count];
            int num = 0;
            foreach (string str2 in this._Roles.Keys)
            {
                rolesForUser[num++] = str2;
            }
            return rolesForUser;
        }

        private void Init()
        {
            this._Version = 1;
            this._IssueDate = DateTime.UtcNow;
            this._ExpireDate = DateTime.UtcNow.AddMinutes((double) Roles.CookieTimeout);
            this._IsRoleListCached = false;
            this._CachedListChanged = false;
            if (this._ProviderName == null)
            {
                this._ProviderName = Roles.Provider.Name;
            }
            if (this._Roles == null)
            {
                this._Roles = new HybridDictionary(true);
            }
            if (this._Identity != null)
            {
                this._Username = this._Identity.Name;
            }
        }

        private void InitFromEncryptedTicket(string encryptedTicket)
        {
            if (HostingEnvironment.IsHosted && EtwTrace.IsTraceEnabled(4, 8))
            {
                EtwTrace.Trace(EtwTraceType.ETW_TYPE_ROLE_BEGIN, HttpContext.Current.WorkerRequest);
            }
            if (!string.IsNullOrEmpty(encryptedTicket))
            {
                byte[] buffer = CookieProtectionHelper.Decode(Roles.CookieProtectionValue, encryptedTicket);
                if (buffer != null)
                {
                    RolePrincipal principal = null;
                    MemoryStream serializationStream = null;
                    try
                    {
                        serializationStream = new MemoryStream(buffer);
                        principal = new BinaryFormatter().Deserialize(serializationStream) as RolePrincipal;
                    }
                    catch
                    {
                    }
                    finally
                    {
                        serializationStream.Close();
                    }
                    if (((principal != null) && StringUtil.EqualsIgnoreCase(principal._Username, this._Identity.Name)) && (StringUtil.EqualsIgnoreCase(principal._ProviderName, this._ProviderName) && (DateTime.UtcNow <= principal._ExpireDate)))
                    {
                        this._Version = principal._Version;
                        this._ExpireDate = principal._ExpireDate;
                        this._IssueDate = principal._IssueDate;
                        this._IsRoleListCached = principal._IsRoleListCached;
                        this._CachedListChanged = false;
                        this._Username = principal._Username;
                        this._Roles = principal._Roles;
                        this.RenewIfOld();
                        if (HostingEnvironment.IsHosted && EtwTrace.IsTraceEnabled(4, 8))
                        {
                            EtwTrace.Trace(EtwTraceType.ETW_TYPE_ROLE_END, HttpContext.Current.WorkerRequest, "RolePrincipal", this._Identity.Name);
                        }
                        return;
                    }
                }
            }
            this.Init();
            this._CachedListChanged = true;
            if (HostingEnvironment.IsHosted && EtwTrace.IsTraceEnabled(4, 8))
            {
                EtwTrace.Trace(EtwTraceType.ETW_TYPE_ROLE_END, HttpContext.Current.WorkerRequest, "RolePrincipal", this._Identity.Name);
            }
        }

        public bool IsInRole(string role)
        {
            if (this._Identity == null)
            {
                throw new ProviderException(System.Web.SR.GetString("Role_Principal_not_fully_constructed"));
            }
            if (!this._Identity.IsAuthenticated || (role == null))
            {
                return false;
            }
            role = role.Trim();
            if (!this.IsRoleListCached)
            {
                this._Roles.Clear();
                foreach (string str in Roles.Providers[this._ProviderName].GetRolesForUser(this.Identity.Name))
                {
                    if (this._Roles[str] == null)
                    {
                        this._Roles.Add(str, string.Empty);
                    }
                }
                this._IsRoleListCached = true;
                this._CachedListChanged = true;
            }
            return (this._Roles[role] != null);
        }

        private void RenewIfOld()
        {
            if (Roles.CookieSlidingExpiration)
            {
                DateTime utcNow = DateTime.UtcNow;
                TimeSpan span = (TimeSpan) (utcNow - this._IssueDate);
                TimeSpan span2 = (TimeSpan) (this._ExpireDate - utcNow);
                if (span2 <= span)
                {
                    this._ExpireDate = utcNow + (this._ExpireDate - this._IssueDate);
                    this._IssueDate = utcNow;
                    this._CachedListChanged = true;
                }
            }
        }

        public void SetDirty()
        {
            this._IsRoleListCached = false;
            this._CachedListChanged = true;
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            this.GetObjectData(info, context);
        }

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.SerializationFormatter)]
        public string ToEncryptedTicket()
        {
            if (!Roles.Enabled)
            {
                return null;
            }
            if ((this._Identity != null) && !this._Identity.IsAuthenticated)
            {
                return null;
            }
            if ((this._Identity == null) && string.IsNullOrEmpty(this._Username))
            {
                return null;
            }
            if (this._Roles.Count > Roles.MaxCachedResults)
            {
                return null;
            }
            MemoryStream serializationStream = new MemoryStream();
            byte[] buf = null;
            IIdentity identity = this._Identity;
            try
            {
                this._Identity = null;
                new BinaryFormatter().Serialize(serializationStream, this);
                buf = serializationStream.ToArray();
            }
            finally
            {
                serializationStream.Close();
                this._Identity = identity;
            }
            return CookieProtectionHelper.Encode(Roles.CookieProtectionValue, buf, buf.Length);
        }

        public bool CachedListChanged
        {
            get
            {
                return this._CachedListChanged;
            }
        }

        public string CookiePath
        {
            get
            {
                return Roles.CookiePath;
            }
        }

        public bool Expired
        {
            get
            {
                return (this._ExpireDate < DateTime.UtcNow);
            }
        }

        public DateTime ExpireDate
        {
            get
            {
                return this._ExpireDate.ToLocalTime();
            }
        }

        public IIdentity Identity
        {
            get
            {
                return this._Identity;
            }
        }

        public bool IsRoleListCached
        {
            get
            {
                return this._IsRoleListCached;
            }
        }

        public DateTime IssueDate
        {
            get
            {
                return this._IssueDate.ToLocalTime();
            }
        }

        public string ProviderName
        {
            get
            {
                return this._ProviderName;
            }
        }

        public int Version
        {
            get
            {
                return this._Version;
            }
        }
    }
}

