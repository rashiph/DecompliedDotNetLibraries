namespace System.Net
{
    using System;
    using System.Security;
    using System.Security.Permissions;

    public class NetworkCredential : ICredentials, ICredentialsByHost
    {
        private static readonly object lockingObject = new object();
        private string m_domain;
        private static EnvironmentPermission m_environmentDomainNamePermission;
        private static EnvironmentPermission m_environmentUserNamePermission;
        private SecureString m_password;
        private string m_userName;

        public NetworkCredential()
        {
        }

        public NetworkCredential(string userName, SecureString password) : this(userName, password, string.Empty)
        {
        }

        public NetworkCredential(string userName, string password) : this(userName, password, string.Empty)
        {
        }

        public NetworkCredential(string userName, SecureString password, string domain)
        {
            this.UserName = userName;
            this.SecurePassword = password;
            this.Domain = domain;
        }

        public NetworkCredential(string userName, string password, string domain)
        {
            this.UserName = userName;
            this.Password = password;
            this.Domain = domain;
        }

        public NetworkCredential GetCredential(Uri uri, string authType)
        {
            return this;
        }

        public NetworkCredential GetCredential(string host, int port, string authenticationType)
        {
            return this;
        }

        private void InitializePart1()
        {
            if (m_environmentUserNamePermission == null)
            {
                lock (lockingObject)
                {
                    if (m_environmentUserNamePermission == null)
                    {
                        m_environmentDomainNamePermission = new EnvironmentPermission(EnvironmentPermissionAccess.Read, "USERDOMAIN");
                        m_environmentUserNamePermission = new EnvironmentPermission(EnvironmentPermissionAccess.Read, "USERNAME");
                    }
                }
            }
        }

        internal string InternalGetDomain()
        {
            return this.m_domain;
        }

        internal string InternalGetDomainUserName()
        {
            string domain = this.InternalGetDomain();
            if (domain.Length != 0)
            {
                domain = domain + @"\";
            }
            return (domain + this.InternalGetUserName());
        }

        internal string InternalGetPassword()
        {
            return UnsafeNclNativeMethods.SecureStringHelper.CreateString(this.m_password);
        }

        internal SecureString InternalGetSecurePassword()
        {
            return this.m_password;
        }

        internal string InternalGetUserName()
        {
            return this.m_userName;
        }

        public string Domain
        {
            get
            {
                this.InitializePart1();
                m_environmentDomainNamePermission.Demand();
                return this.InternalGetDomain();
            }
            set
            {
                if (value == null)
                {
                    this.m_domain = string.Empty;
                }
                else
                {
                    this.m_domain = value;
                }
            }
        }

        public string Password
        {
            get
            {
                ExceptionHelper.UnmanagedPermission.Demand();
                return this.InternalGetPassword();
            }
            set
            {
                this.m_password = UnsafeNclNativeMethods.SecureStringHelper.CreateSecureString(value);
            }
        }

        public SecureString SecurePassword
        {
            get
            {
                ExceptionHelper.UnmanagedPermission.Demand();
                return this.InternalGetSecurePassword().Copy();
            }
            set
            {
                if (value == null)
                {
                    this.m_password = new SecureString();
                }
                else
                {
                    this.m_password = value.Copy();
                }
            }
        }

        public string UserName
        {
            get
            {
                this.InitializePart1();
                m_environmentUserNamePermission.Demand();
                return this.InternalGetUserName();
            }
            set
            {
                if (value == null)
                {
                    this.m_userName = string.Empty;
                }
                else
                {
                    this.m_userName = value;
                }
            }
        }
    }
}

