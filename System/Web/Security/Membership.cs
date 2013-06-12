namespace System.Web.Security
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Configuration.Provider;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Security.Principal;
    using System.Threading;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.Util;

    public static class Membership
    {
        private static char[] punctuations = "!@#$%^&*()_-+=[{]};:>|./?".ToCharArray();
        private static bool s_HashAlgorithmFromConfig;
        private static string s_HashAlgorithmType;
        private static bool s_Initialized = false;
        private static bool s_InitializedDefaultProvider;
        private static Exception s_InitializeException = null;
        private static object s_lock = new object();
        private static MembershipProvider s_Provider;
        private static MembershipProviderCollection s_Providers;
        private static int s_UserIsOnlineTimeWindow = 15;

        public static  event MembershipValidatePasswordEventHandler ValidatingPassword
        {
            add
            {
                Provider.ValidatingPassword += value;
            }
            remove
            {
                Provider.ValidatingPassword -= value;
            }
        }

        public static MembershipUser CreateUser(string username, string password)
        {
            return CreateUser(username, password, null);
        }

        public static MembershipUser CreateUser(string username, string password, string email)
        {
            MembershipCreateStatus status;
            MembershipUser user = CreateUser(username, password, email, null, null, true, out status);
            if (user == null)
            {
                throw new MembershipCreateUserException(status);
            }
            return user;
        }

        public static MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, out MembershipCreateStatus status)
        {
            return CreateUser(username, password, email, passwordQuestion, passwordAnswer, isApproved, null, out status);
        }

        public static MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            if (!SecUtility.ValidateParameter(ref username, true, true, true, 0))
            {
                status = MembershipCreateStatus.InvalidUserName;
                return null;
            }
            if (!SecUtility.ValidatePasswordParameter(ref password, 0))
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }
            if (!SecUtility.ValidateParameter(ref email, false, false, false, 0))
            {
                status = MembershipCreateStatus.InvalidEmail;
                return null;
            }
            if (!SecUtility.ValidateParameter(ref passwordQuestion, false, true, false, 0))
            {
                status = MembershipCreateStatus.InvalidQuestion;
                return null;
            }
            if (!SecUtility.ValidateParameter(ref passwordAnswer, false, true, false, 0))
            {
                status = MembershipCreateStatus.InvalidAnswer;
                return null;
            }
            return Provider.CreateUser(username, password, email, passwordQuestion, passwordAnswer, isApproved, providerUserKey, out status);
        }

        public static bool DeleteUser(string username)
        {
            SecUtility.CheckParameter(ref username, true, true, true, 0, "username");
            return Provider.DeleteUser(username, true);
        }

        public static bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            SecUtility.CheckParameter(ref username, true, true, true, 0, "username");
            return Provider.DeleteUser(username, deleteAllRelatedData);
        }

        public static MembershipUserCollection FindUsersByEmail(string emailToMatch)
        {
            SecUtility.CheckParameter(ref emailToMatch, false, false, false, 0, "emailToMatch");
            int totalRecords = 0;
            return FindUsersByEmail(emailToMatch, 0, 0x7fffffff, out totalRecords);
        }

        public static MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            SecUtility.CheckParameter(ref emailToMatch, false, false, false, 0, "emailToMatch");
            if (pageIndex < 0)
            {
                throw new ArgumentException(System.Web.SR.GetString("PageIndex_bad"), "pageIndex");
            }
            if (pageSize < 1)
            {
                throw new ArgumentException(System.Web.SR.GetString("PageSize_bad"), "pageSize");
            }
            return Provider.FindUsersByEmail(emailToMatch, pageIndex, pageSize, out totalRecords);
        }

        public static MembershipUserCollection FindUsersByName(string usernameToMatch)
        {
            SecUtility.CheckParameter(ref usernameToMatch, true, true, false, 0, "usernameToMatch");
            int totalRecords = 0;
            return Provider.FindUsersByName(usernameToMatch, 0, 0x7fffffff, out totalRecords);
        }

        public static MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            SecUtility.CheckParameter(ref usernameToMatch, true, true, false, 0, "usernameToMatch");
            if (pageIndex < 0)
            {
                throw new ArgumentException(System.Web.SR.GetString("PageIndex_bad"), "pageIndex");
            }
            if (pageSize < 1)
            {
                throw new ArgumentException(System.Web.SR.GetString("PageSize_bad"), "pageSize");
            }
            return Provider.FindUsersByName(usernameToMatch, pageIndex, pageSize, out totalRecords);
        }

        public static string GeneratePassword(int length, int numberOfNonAlphanumericCharacters)
        {
            string str;
            int num;
            if ((length < 1) || (length > 0x80))
            {
                throw new ArgumentException(System.Web.SR.GetString("Membership_password_length_incorrect"));
            }
            if ((numberOfNonAlphanumericCharacters > length) || (numberOfNonAlphanumericCharacters < 0))
            {
                throw new ArgumentException(System.Web.SR.GetString("Membership_min_required_non_alphanumeric_characters_incorrect", new object[] { "numberOfNonAlphanumericCharacters" }));
            }
            do
            {
                byte[] data = new byte[length];
                char[] chArray = new char[length];
                int num2 = 0;
                new RNGCryptoServiceProvider().GetBytes(data);
                for (int i = 0; i < length; i++)
                {
                    int num4 = data[i] % 0x57;
                    if (num4 < 10)
                    {
                        chArray[i] = (char) (0x30 + num4);
                    }
                    else if (num4 < 0x24)
                    {
                        chArray[i] = (char) ((0x41 + num4) - 10);
                    }
                    else if (num4 < 0x3e)
                    {
                        chArray[i] = (char) ((0x61 + num4) - 0x24);
                    }
                    else
                    {
                        chArray[i] = punctuations[num4 - 0x3e];
                        num2++;
                    }
                }
                if (num2 < numberOfNonAlphanumericCharacters)
                {
                    Random random = new Random();
                    for (int j = 0; j < (numberOfNonAlphanumericCharacters - num2); j++)
                    {
                        int num6;
                        do
                        {
                            num6 = random.Next(0, length);
                        }
                        while (!char.IsLetterOrDigit(chArray[num6]));
                        chArray[num6] = punctuations[random.Next(0, punctuations.Length)];
                    }
                }
                str = new string(chArray);
            }
            while (CrossSiteScriptingValidation.IsDangerousString(str, out num));
            return str;
        }

        public static MembershipUserCollection GetAllUsers()
        {
            int totalRecords = 0;
            return GetAllUsers(0, 0x7fffffff, out totalRecords);
        }

        public static MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            if (pageIndex < 0)
            {
                throw new ArgumentException(System.Web.SR.GetString("PageIndex_bad"), "pageIndex");
            }
            if (pageSize < 1)
            {
                throw new ArgumentException(System.Web.SR.GetString("PageSize_bad"), "pageSize");
            }
            return Provider.GetAllUsers(pageIndex, pageSize, out totalRecords);
        }

        private static string GetCurrentUserName()
        {
            if (HostingEnvironment.IsHosted)
            {
                HttpContext current = HttpContext.Current;
                if (current != null)
                {
                    return current.User.Identity.Name;
                }
            }
            IPrincipal currentPrincipal = Thread.CurrentPrincipal;
            if ((currentPrincipal != null) && (currentPrincipal.Identity != null))
            {
                return currentPrincipal.Identity.Name;
            }
            return string.Empty;
        }

        public static int GetNumberOfUsersOnline()
        {
            return Provider.GetNumberOfUsersOnline();
        }

        public static MembershipUser GetUser()
        {
            return GetUser(GetCurrentUserName(), true);
        }

        public static MembershipUser GetUser(bool userIsOnline)
        {
            return GetUser(GetCurrentUserName(), userIsOnline);
        }

        public static MembershipUser GetUser(object providerUserKey)
        {
            return GetUser(providerUserKey, false);
        }

        public static MembershipUser GetUser(string username)
        {
            return GetUser(username, false);
        }

        public static MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            if (providerUserKey == null)
            {
                throw new ArgumentNullException("providerUserKey");
            }
            return Provider.GetUser(providerUserKey, userIsOnline);
        }

        public static MembershipUser GetUser(string username, bool userIsOnline)
        {
            SecUtility.CheckParameter(ref username, true, false, true, 0, "username");
            return Provider.GetUser(username, userIsOnline);
        }

        public static string GetUserNameByEmail(string emailToMatch)
        {
            SecUtility.CheckParameter(ref emailToMatch, false, false, false, 0, "emailToMatch");
            return Provider.GetUserNameByEmail(emailToMatch);
        }

        private static void Initialize()
        {
            if (!s_Initialized || !s_InitializedDefaultProvider)
            {
                if (s_InitializeException != null)
                {
                    throw s_InitializeException;
                }
                if (HostingEnvironment.IsHosted)
                {
                    HttpRuntime.CheckAspNetHostingPermission(AspNetHostingPermissionLevel.Low, "Feature_not_supported_at_this_level");
                }
                lock (s_lock)
                {
                    if (!s_Initialized || !s_InitializedDefaultProvider)
                    {
                        if (s_InitializeException != null)
                        {
                            throw s_InitializeException;
                        }
                        bool initializeGeneralSettings = !s_Initialized;
                        bool initializeDefaultProvider = !s_InitializedDefaultProvider && (!HostingEnvironment.IsHosted || (BuildManager.PreStartInitStage == PreStartInitStage.AfterPreStartInit));
                        if (initializeDefaultProvider || initializeGeneralSettings)
                        {
                            bool flag3;
                            bool flag4 = false;
                            try
                            {
                                RuntimeConfig appConfig = RuntimeConfig.GetAppConfig();
                                MembershipSection membership = appConfig.Membership;
                                flag3 = InitializeSettings(initializeGeneralSettings, appConfig, membership);
                                flag4 = InitializeDefaultProvider(initializeDefaultProvider, membership);
                            }
                            catch (Exception exception)
                            {
                                s_InitializeException = exception;
                                throw;
                            }
                            if (flag3)
                            {
                                s_Initialized = true;
                            }
                            if (flag4)
                            {
                                s_InitializedDefaultProvider = true;
                            }
                        }
                    }
                }
            }
        }

        private static bool InitializeDefaultProvider(bool initializeDefaultProvider, MembershipSection settings)
        {
            if (!initializeDefaultProvider)
            {
                return false;
            }
            s_Providers.SetReadOnly();
            if ((settings.DefaultProvider == null) || (s_Providers.Count < 1))
            {
                throw new ProviderException(System.Web.SR.GetString("Def_membership_provider_not_specified"));
            }
            s_Provider = s_Providers[settings.DefaultProvider];
            if (s_Provider == null)
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Def_membership_provider_not_found"), settings.ElementInformation.Properties["defaultProvider"].Source, settings.ElementInformation.Properties["defaultProvider"].LineNumber);
            }
            return true;
        }

        private static bool InitializeSettings(bool initializeGeneralSettings, RuntimeConfig appConfig, MembershipSection settings)
        {
            if (!initializeGeneralSettings)
            {
                return false;
            }
            s_HashAlgorithmType = settings.HashAlgorithmType;
            s_HashAlgorithmFromConfig = !string.IsNullOrEmpty(s_HashAlgorithmType);
            if (!s_HashAlgorithmFromConfig)
            {
                MachineKeyValidation validation = appConfig.MachineKey.Validation;
                if ((validation != MachineKeyValidation.AES) && (validation != MachineKeyValidation.TripleDES))
                {
                    s_HashAlgorithmType = appConfig.MachineKey.ValidationAlgorithm;
                }
                else
                {
                    s_HashAlgorithmType = "SHA1";
                }
            }
            s_Providers = new MembershipProviderCollection();
            if (HostingEnvironment.IsHosted)
            {
                ProvidersHelper.InstantiateProviders(settings.Providers, s_Providers, typeof(MembershipProvider));
            }
            else
            {
                foreach (ProviderSettings settings2 in settings.Providers)
                {
                    Type c = Type.GetType(settings2.Type, true, true);
                    if (!typeof(MembershipProvider).IsAssignableFrom(c))
                    {
                        throw new ArgumentException(System.Web.SR.GetString("Provider_must_implement_type", new object[] { typeof(MembershipProvider).ToString() }));
                    }
                    MembershipProvider provider = (MembershipProvider) Activator.CreateInstance(c);
                    NameValueCollection parameters = settings2.Parameters;
                    NameValueCollection config = new NameValueCollection(parameters.Count, StringComparer.Ordinal);
                    foreach (string str in parameters)
                    {
                        config[str] = parameters[str];
                    }
                    provider.Initialize(settings2.Name, config);
                    s_Providers.Add(provider);
                }
            }
            s_UserIsOnlineTimeWindow = (int) settings.UserIsOnlineTimeWindow.TotalMinutes;
            return true;
        }

        public static void UpdateUser(MembershipUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.Update();
        }

        public static bool ValidateUser(string username, string password)
        {
            return Provider.ValidateUser(username, password);
        }

        public static string ApplicationName
        {
            get
            {
                return Provider.ApplicationName;
            }
            set
            {
                Provider.ApplicationName = value;
            }
        }

        public static bool EnablePasswordReset
        {
            get
            {
                Initialize();
                return Provider.EnablePasswordReset;
            }
        }

        public static bool EnablePasswordRetrieval
        {
            get
            {
                Initialize();
                return Provider.EnablePasswordRetrieval;
            }
        }

        public static string HashAlgorithmType
        {
            get
            {
                Initialize();
                return s_HashAlgorithmType;
            }
        }

        internal static bool IsHashAlgorithmFromMembershipConfig
        {
            get
            {
                Initialize();
                return s_HashAlgorithmFromConfig;
            }
        }

        public static int MaxInvalidPasswordAttempts
        {
            get
            {
                Initialize();
                return Provider.MaxInvalidPasswordAttempts;
            }
        }

        public static int MinRequiredNonAlphanumericCharacters
        {
            get
            {
                Initialize();
                return Provider.MinRequiredNonAlphanumericCharacters;
            }
        }

        public static int MinRequiredPasswordLength
        {
            get
            {
                Initialize();
                return Provider.MinRequiredPasswordLength;
            }
        }

        public static int PasswordAttemptWindow
        {
            get
            {
                Initialize();
                return Provider.PasswordAttemptWindow;
            }
        }

        public static string PasswordStrengthRegularExpression
        {
            get
            {
                Initialize();
                return Provider.PasswordStrengthRegularExpression;
            }
        }

        public static MembershipProvider Provider
        {
            get
            {
                Initialize();
                if (s_Provider == null)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("Def_membership_provider_not_found"));
                }
                return s_Provider;
            }
        }

        public static MembershipProviderCollection Providers
        {
            get
            {
                Initialize();
                return s_Providers;
            }
        }

        public static bool RequiresQuestionAndAnswer
        {
            get
            {
                Initialize();
                return Provider.RequiresQuestionAndAnswer;
            }
        }

        public static int UserIsOnlineTimeWindow
        {
            get
            {
                Initialize();
                return s_UserIsOnlineTimeWindow;
            }
        }
    }
}

