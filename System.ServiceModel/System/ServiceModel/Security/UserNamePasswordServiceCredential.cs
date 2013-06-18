namespace System.ServiceModel.Security
{
    using System;
    using System.IdentityModel.Selectors;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.Web.Security;

    public sealed class UserNamePasswordServiceCredential
    {
        private TimeSpan cachedLogonTokenLifetime;
        private bool cacheLogonTokens;
        internal static readonly TimeSpan DefaultCachedLogonTokenLifetime = TimeSpan.Parse("00:15:00", CultureInfo.InvariantCulture);
        internal const string DefaultCachedLogonTokenLifetimeString = "00:15:00";
        internal const bool DefaultCacheLogonTokens = false;
        internal const int DefaultMaxCachedLogonTokens = 0x80;
        internal const System.ServiceModel.Security.UserNamePasswordValidationMode DefaultUserNamePasswordValidationMode = System.ServiceModel.Security.UserNamePasswordValidationMode.Windows;
        private bool includeWindowsGroups;
        private bool isReadOnly;
        private int maxCachedLogonTokens;
        private object membershipProvider;
        private System.ServiceModel.Security.UserNamePasswordValidationMode validationMode;
        private UserNamePasswordValidator validator;

        internal UserNamePasswordServiceCredential()
        {
            this.includeWindowsGroups = true;
            this.maxCachedLogonTokens = 0x80;
            this.cachedLogonTokenLifetime = DefaultCachedLogonTokenLifetime;
        }

        internal UserNamePasswordServiceCredential(UserNamePasswordServiceCredential other)
        {
            this.includeWindowsGroups = true;
            this.maxCachedLogonTokens = 0x80;
            this.cachedLogonTokenLifetime = DefaultCachedLogonTokenLifetime;
            this.includeWindowsGroups = other.includeWindowsGroups;
            this.membershipProvider = other.membershipProvider;
            this.validationMode = other.validationMode;
            this.validator = other.validator;
            this.cacheLogonTokens = other.cacheLogonTokens;
            this.maxCachedLogonTokens = other.maxCachedLogonTokens;
            this.cachedLogonTokenLifetime = other.cachedLogonTokenLifetime;
            this.isReadOnly = other.isReadOnly;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private UserNamePasswordValidator GetMembershipProviderValidator()
        {
            System.Web.Security.MembershipProvider membershipProvider;
            if (this.membershipProvider != null)
            {
                membershipProvider = (System.Web.Security.MembershipProvider) this.membershipProvider;
            }
            else
            {
                membershipProvider = SystemWebHelper.GetMembershipProvider();
            }
            if (membershipProvider == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MissingMembershipProvider")));
            }
            return UserNamePasswordValidator.CreateMembershipProviderValidator(membershipProvider);
        }

        internal UserNamePasswordValidator GetUserNamePasswordValidator()
        {
            if (this.validationMode == System.ServiceModel.Security.UserNamePasswordValidationMode.MembershipProvider)
            {
                return this.GetMembershipProviderValidator();
            }
            if (this.validationMode != System.ServiceModel.Security.UserNamePasswordValidationMode.Custom)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
            if (this.validator == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MissingCustomUserNamePasswordValidator")));
            }
            return this.validator;
        }

        internal void MakeReadOnly()
        {
            this.isReadOnly = true;
        }

        private void ThrowIfImmutable()
        {
            if (this.isReadOnly)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ObjectIsReadOnly")));
            }
        }

        public TimeSpan CachedLogonTokenLifetime
        {
            get
            {
                return this.cachedLogonTokenLifetime;
            }
            set
            {
                if (value <= TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", System.ServiceModel.SR.GetString("TimeSpanMustbeGreaterThanTimeSpanZero")));
                }
                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRangeTooBig")));
                }
                this.ThrowIfImmutable();
                this.cachedLogonTokenLifetime = value;
            }
        }

        public bool CacheLogonTokens
        {
            get
            {
                return this.cacheLogonTokens;
            }
            set
            {
                this.ThrowIfImmutable();
                this.cacheLogonTokens = value;
            }
        }

        public UserNamePasswordValidator CustomUserNamePasswordValidator
        {
            get
            {
                return this.validator;
            }
            set
            {
                this.ThrowIfImmutable();
                this.validator = value;
            }
        }

        public bool IncludeWindowsGroups
        {
            get
            {
                return this.includeWindowsGroups;
            }
            set
            {
                this.ThrowIfImmutable();
                this.includeWindowsGroups = value;
            }
        }

        public int MaxCachedLogonTokens
        {
            get
            {
                return this.maxCachedLogonTokens;
            }
            set
            {
                if (value <= 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", System.ServiceModel.SR.GetString("ValueMustBeGreaterThanZero")));
                }
                this.ThrowIfImmutable();
                this.maxCachedLogonTokens = value;
            }
        }

        public System.Web.Security.MembershipProvider MembershipProvider
        {
            get
            {
                return (System.Web.Security.MembershipProvider) this.membershipProvider;
            }
            set
            {
                this.ThrowIfImmutable();
                this.membershipProvider = value;
            }
        }

        public System.ServiceModel.Security.UserNamePasswordValidationMode UserNamePasswordValidationMode
        {
            get
            {
                return this.validationMode;
            }
            set
            {
                UserNamePasswordValidationModeHelper.Validate(value);
                this.ThrowIfImmutable();
                this.validationMode = value;
            }
        }
    }
}

