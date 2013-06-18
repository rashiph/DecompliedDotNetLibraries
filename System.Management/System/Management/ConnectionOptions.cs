namespace System.Management
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security;

    public class ConnectionOptions : ManagementOptions
    {
        private AuthenticationLevel authentication;
        private string authority;
        internal const AuthenticationLevel DEFAULTAUTHENTICATION = AuthenticationLevel.Unchanged;
        internal const string DEFAULTAUTHORITY = null;
        internal const bool DEFAULTENABLEPRIVILEGES = false;
        internal const ImpersonationLevel DEFAULTIMPERSONATION = ImpersonationLevel.Impersonate;
        internal const string DEFAULTLOCALE = null;
        private bool enablePrivileges;
        private ImpersonationLevel impersonation;
        private string locale;
        private SecureString securePassword;
        private string username;

        public ConnectionOptions() : this(null, null, (string) null, null, ImpersonationLevel.Impersonate, AuthenticationLevel.Unchanged, false, null, ManagementOptions.InfiniteTimeout)
        {
        }

        internal ConnectionOptions(ManagementNamedValueCollection context) : base(context, ManagementOptions.InfiniteTimeout)
        {
        }

        internal ConnectionOptions(ManagementNamedValueCollection context, TimeSpan timeout, int flags) : base(context, timeout, flags)
        {
        }

        public ConnectionOptions(string locale, string username, SecureString password, string authority, ImpersonationLevel impersonation, AuthenticationLevel authentication, bool enablePrivileges, ManagementNamedValueCollection context, TimeSpan timeout) : base(context, timeout)
        {
            if (locale != null)
            {
                this.locale = locale;
            }
            this.username = username;
            this.enablePrivileges = enablePrivileges;
            if (password != null)
            {
                this.securePassword = password.Copy();
            }
            if (authority != null)
            {
                this.authority = authority;
            }
            if (impersonation != ImpersonationLevel.Default)
            {
                this.impersonation = impersonation;
            }
            if (authentication != AuthenticationLevel.Default)
            {
                this.authentication = authentication;
            }
        }

        public ConnectionOptions(string locale, string username, string password, string authority, ImpersonationLevel impersonation, AuthenticationLevel authentication, bool enablePrivileges, ManagementNamedValueCollection context, TimeSpan timeout) : base(context, timeout)
        {
            if (locale != null)
            {
                this.locale = locale;
            }
            this.username = username;
            this.enablePrivileges = enablePrivileges;
            if (password != null)
            {
                this.securePassword = new SecureString();
                for (int i = 0; i < password.Length; i++)
                {
                    this.securePassword.AppendChar(password[i]);
                }
            }
            if (authority != null)
            {
                this.authority = authority;
            }
            if (impersonation != ImpersonationLevel.Default)
            {
                this.impersonation = impersonation;
            }
            if (authentication != AuthenticationLevel.Default)
            {
                this.authentication = authentication;
            }
        }

        internal static ConnectionOptions _Clone(ConnectionOptions options)
        {
            return _Clone(options, null);
        }

        internal static ConnectionOptions _Clone(ConnectionOptions options, IdentifierChangedEventHandler handler)
        {
            ConnectionOptions options2;
            if (options != null)
            {
                options2 = new ConnectionOptions(options.Context, options.Timeout, options.Flags) {
                    locale = options.locale,
                    username = options.username,
                    enablePrivileges = options.enablePrivileges
                };
                if (options.securePassword != null)
                {
                    options2.securePassword = options.securePassword.Copy();
                }
                else
                {
                    options2.securePassword = null;
                }
                if (options.authority != null)
                {
                    options2.authority = options.authority;
                }
                if (options.impersonation != ImpersonationLevel.Default)
                {
                    options2.impersonation = options.impersonation;
                }
                if (options.authentication != AuthenticationLevel.Default)
                {
                    options2.authentication = options.authentication;
                }
            }
            else
            {
                options2 = new ConnectionOptions();
            }
            if (handler != null)
            {
                options2.IdentifierChanged += handler;
                return options2;
            }
            if (options != null)
            {
                options2.IdentifierChanged += new IdentifierChangedEventHandler(options.HandleIdentifierChange);
            }
            return options2;
        }

        public override object Clone()
        {
            ManagementNamedValueCollection context = null;
            if (base.Context != null)
            {
                context = base.Context.Clone();
            }
            return new ConnectionOptions(this.locale, this.username, this.GetSecurePassword(), this.authority, this.impersonation, this.authentication, this.enablePrivileges, context, base.Timeout);
        }

        internal IntPtr GetPassword()
        {
            if (this.securePassword != null)
            {
                try
                {
                    return Marshal.SecureStringToBSTR(this.securePassword);
                }
                catch (OutOfMemoryException)
                {
                    return IntPtr.Zero;
                }
            }
            return IntPtr.Zero;
        }

        internal SecureString GetSecurePassword()
        {
            if (this.securePassword != null)
            {
                return this.securePassword.Copy();
            }
            return null;
        }

        public AuthenticationLevel Authentication
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.authentication;
            }
            set
            {
                if (this.authentication != value)
                {
                    this.authentication = value;
                    base.FireIdentifierChanged();
                }
            }
        }

        public string Authority
        {
            get
            {
                if (this.authority == null)
                {
                    return string.Empty;
                }
                return this.authority;
            }
            set
            {
                if (this.authority != value)
                {
                    this.authority = value;
                    base.FireIdentifierChanged();
                }
            }
        }

        public bool EnablePrivileges
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.enablePrivileges;
            }
            set
            {
                if (this.enablePrivileges != value)
                {
                    this.enablePrivileges = value;
                    base.FireIdentifierChanged();
                }
            }
        }

        public ImpersonationLevel Impersonation
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.impersonation;
            }
            set
            {
                if (this.impersonation != value)
                {
                    this.impersonation = value;
                    base.FireIdentifierChanged();
                }
            }
        }

        public string Locale
        {
            get
            {
                if (this.locale == null)
                {
                    return string.Empty;
                }
                return this.locale;
            }
            set
            {
                if (this.locale != value)
                {
                    this.locale = value;
                    base.FireIdentifierChanged();
                }
            }
        }

        public string Password
        {
            set
            {
                if (value != null)
                {
                    if (this.securePassword == null)
                    {
                        this.securePassword = new SecureString();
                        for (int i = 0; i < value.Length; i++)
                        {
                            this.securePassword.AppendChar(value[i]);
                        }
                    }
                    else
                    {
                        SecureString str = new SecureString();
                        for (int j = 0; j < value.Length; j++)
                        {
                            str.AppendChar(value[j]);
                        }
                        this.securePassword.Clear();
                        this.securePassword = str.Copy();
                        base.FireIdentifierChanged();
                        str.Dispose();
                    }
                }
                else if (this.securePassword != null)
                {
                    this.securePassword.Dispose();
                    this.securePassword = null;
                    base.FireIdentifierChanged();
                }
            }
        }

        public SecureString SecurePassword
        {
            set
            {
                if (value != null)
                {
                    if (this.securePassword == null)
                    {
                        this.securePassword = value.Copy();
                    }
                    else
                    {
                        this.securePassword.Clear();
                        this.securePassword = value.Copy();
                        base.FireIdentifierChanged();
                    }
                }
                else if (this.securePassword != null)
                {
                    this.securePassword.Dispose();
                    this.securePassword = null;
                    base.FireIdentifierChanged();
                }
            }
        }

        public string Username
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.username;
            }
            set
            {
                if (this.username != value)
                {
                    this.username = value;
                    base.FireIdentifierChanged();
                }
            }
        }
    }
}

