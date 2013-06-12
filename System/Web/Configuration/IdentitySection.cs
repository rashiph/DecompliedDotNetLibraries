namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Web;
    using System.Web.Util;

    public sealed class IdentitySection : ConfigurationSection
    {
        private bool _credentialsValidated;
        private object _credentialsValidatedLock = new object();
        private ImpersonateTokenRef _impersonateTokenRef = new ImpersonateTokenRef(IntPtr.Zero);
        private string _password;
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propImpersonate = new ConfigurationProperty("impersonate", typeof(bool), false, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propPassword = new ConfigurationProperty("password", typeof(string), string.Empty, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propUserName = new ConfigurationProperty("userName", typeof(string), string.Empty, ConfigurationPropertyOptions.None);
        private string _username;
        private string error = string.Empty;
        private bool impersonateCache;
        private bool impersonateCached = false;

        static IdentitySection()
        {
            _properties.Add(_propImpersonate);
            _properties.Add(_propUserName);
            _properties.Add(_propPassword);
        }

        internal static IntPtr CreateUserToken(string name, string password, out string error)
        {
            IntPtr zero = IntPtr.Zero;
            if (VersionInfo.ExeName == "aspnet_wp")
            {
                byte[] bufferOut = new byte[IntPtr.Size];
                byte[] bytes = Encoding.Unicode.GetBytes(name + "\t" + password);
                byte[] dst = new byte[bytes.Length + 2];
                Buffer.BlockCopy(bytes, 0, dst, 0, bytes.Length);
                if (UnsafeNativeMethods.PMCallISAPI(IntPtr.Zero, UnsafeNativeMethods.CallISAPIFunc.GenerateToken, dst, dst.Length, bufferOut, bufferOut.Length) == 1)
                {
                    long num = 0L;
                    for (int i = 0; i < IntPtr.Size; i++)
                    {
                        num = (num * 0x100L) + bufferOut[i];
                    }
                    zero = (IntPtr) num;
                }
            }
            if (zero == IntPtr.Zero)
            {
                StringBuilder strError = new StringBuilder(0x100);
                zero = UnsafeNativeMethods.CreateUserToken(name, password, 1, strError, 0x100);
                error = strError.ToString();
                if (!(zero != IntPtr.Zero))
                {
                }
            }
            else
            {
                error = string.Empty;
            }
            bool flag1 = zero == IntPtr.Zero;
            return zero;
        }

        protected override object GetRuntimeObject()
        {
            if (!this._credentialsValidated)
            {
                lock (this._credentialsValidatedLock)
                {
                    if (!this._credentialsValidated)
                    {
                        this.ValidateCredentials();
                        this._credentialsValidated = true;
                    }
                }
            }
            return base.GetRuntimeObject();
        }

        private void InitializeToken()
        {
            this.error = string.Empty;
            IntPtr token = CreateUserToken(this._username, this._password, out this.error);
            this._impersonateTokenRef = new ImpersonateTokenRef(token);
            if (this._impersonateTokenRef.Handle == IntPtr.Zero)
            {
                if (this.error.Length > 0)
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_credentials_2", new object[] { this.error }), base.ElementInformation.Properties["userName"].Source, base.ElementInformation.Properties["userName"].LineNumber);
                }
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_credentials"), base.ElementInformation.Properties["userName"].Source, base.ElementInformation.Properties["userName"].LineNumber);
            }
        }

        protected override void Reset(ConfigurationElement parentElement)
        {
            base.Reset(parentElement);
            IdentitySection section = parentElement as IdentitySection;
            if (section != null)
            {
                this._impersonateTokenRef = section._impersonateTokenRef;
                if (this.Impersonate)
                {
                    this.UserName = null;
                    this.Password = null;
                    this._impersonateTokenRef = new ImpersonateTokenRef(IntPtr.Zero);
                }
                this.impersonateCached = false;
                this._credentialsValidated = false;
            }
        }

        protected override void Unmerge(ConfigurationElement sourceElement, ConfigurationElement parentElement, ConfigurationSaveMode saveMode)
        {
            base.Unmerge(sourceElement, parentElement, saveMode);
            IdentitySection section = sourceElement as IdentitySection;
            if (this.Impersonate != section.Impersonate)
            {
                this.Impersonate = section.Impersonate;
            }
            if (this.Impersonate && (section.ElementInformation.Properties[_propUserName.Name].IsModified || section.ElementInformation.Properties[_propPassword.Name].IsModified))
            {
                this.UserName = section.UserName;
                this.Password = section.Password;
            }
        }

        private void ValidateCredentials()
        {
            this._username = this.UserName;
            this._password = this.Password;
            if (!System.Web.Configuration.HandlerBase.CheckAndReadRegistryValue(ref this._username, false))
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_registry_config"), base.ElementInformation.Source, base.ElementInformation.LineNumber);
            }
            if (!System.Web.Configuration.HandlerBase.CheckAndReadRegistryValue(ref this._password, false))
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_registry_config"), base.ElementInformation.Source, base.ElementInformation.LineNumber);
            }
            if ((this._username != null) && (this._username.Length < 1))
            {
                this._username = null;
            }
            if ((this._username != null) && this.Impersonate)
            {
                if (this._password == null)
                {
                    this._password = string.Empty;
                }
            }
            else if (((this._password != null) && (this._username == null)) && ((this._password.Length > 0) && this.Impersonate))
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_credentials"), base.ElementInformation.Properties["password"].Source, base.ElementInformation.Properties["password"].LineNumber);
            }
            if ((this.Impersonate && (this.ImpersonateToken == IntPtr.Zero)) && (this._username != null))
            {
                if (this.error.Length > 0)
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_credentials_2", new object[] { this.error }), base.ElementInformation.Properties["userName"].Source, base.ElementInformation.Properties["userName"].LineNumber);
                }
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_credentials"), base.ElementInformation.Properties["userName"].Source, base.ElementInformation.Properties["userName"].LineNumber);
            }
        }

        [ConfigurationProperty("impersonate", DefaultValue=false)]
        public bool Impersonate
        {
            get
            {
                if (!this.impersonateCached)
                {
                    this.impersonateCache = (bool) base[_propImpersonate];
                    this.impersonateCached = true;
                }
                return this.impersonateCache;
            }
            set
            {
                base[_propImpersonate] = value;
                this.impersonateCache = value;
            }
        }

        internal IntPtr ImpersonateToken
        {
            get
            {
                if (((this._impersonateTokenRef.Handle == IntPtr.Zero) && (this._username != null)) && this.Impersonate)
                {
                    this.InitializeToken();
                }
                return this._impersonateTokenRef.Handle;
            }
        }

        [ConfigurationProperty("password", DefaultValue="")]
        public string Password
        {
            get
            {
                return (string) base[_propPassword];
            }
            set
            {
                base[_propPassword] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        internal ContextInformation ProtectedEvaluationContext
        {
            get
            {
                return base.EvaluationContext;
            }
        }

        [ConfigurationProperty("userName", DefaultValue="")]
        public string UserName
        {
            get
            {
                return (string) base[_propUserName];
            }
            set
            {
                base[_propUserName] = value;
            }
        }
    }
}

