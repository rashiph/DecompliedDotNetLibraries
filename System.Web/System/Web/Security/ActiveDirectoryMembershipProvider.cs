namespace System.Web.Security
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Configuration.Provider;
    using System.DirectoryServices;
    using System.DirectoryServices.Protocols;
    using System.Globalization;
    using System.Net;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.DataAccess;
    using System.Web.Hosting;
    using System.Web.Management;
    using System.Web.Util;

    [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
    public class ActiveDirectoryMembershipProvider : MembershipProvider
    {
        private MembershipPasswordCompatibilityMode _LegacyPasswordCompatibilityMode;
        private const int AD_SALT_SIZE_IN_BYTES = 0x10;
        private string adConnectionString;
        private string appName;
        private string attributeMapEmail = "mail";
        private string attributeMapFailedPasswordAnswerCount;
        private string attributeMapFailedPasswordAnswerLockoutTime;
        private string attributeMapFailedPasswordAnswerTime;
        private string attributeMapPasswordAnswer;
        private string attributeMapPasswordQuestion;
        private string attributeMapUsername = "userPrincipalName";
        private Hashtable attributesInUse = new Hashtable(StringComparer.OrdinalIgnoreCase);
        private AuthType authTypeForValidation;
        private LdapConnection connection;
        private readonly DateTime DefaultLastLockoutDate = new DateTime(0x6da, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private DirectoryInformation directoryInfo;
        private bool enablePasswordReset;
        private bool enablePasswordRetrieval;
        private bool enableSearchMethods;
        private bool initialized;
        private int maxCommentLength = 0x400;
        private int maxEmailLength = 0x100;
        private int maxInvalidPasswordAttempts;
        private int maxPasswordAnswerLength = 0x80;
        private int maxPasswordLength = 0x80;
        private int maxPasswordQuestionLength = 0x100;
        private int maxUsernameLength = 0x100;
        private int maxUsernameLengthForCreation = 0x40;
        private int minRequiredNonalphanumericCharacters;
        private int minRequiredPasswordLength;
        private const int PASSWORD_SIZE = 14;
        private int passwordAnswerAttemptLockoutDuration;
        private int passwordAttemptWindow;
        private string passwordStrengthRegularExpression;
        private bool requiresQuestionAndAnswer;
        private bool requiresUniqueEmail;
        private Hashtable syntaxes = new Hashtable();
        private const int UF_ACCOUNT_DISABLED = 2;
        private const int UF_LOCKOUT = 0x10;
        private bool usernameIsSAMAccountName;
        private bool usernameIsUPN = true;
        private Hashtable userObjectAttributes;

        [DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            if (!this.initialized)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("ADMembership_Provider_not_initialized"));
            }
            this.CheckUserName(ref username, this.maxUsernameLength, "username");
            this.CheckPassword(oldPassword, this.maxPasswordLength, "oldPassword");
            this.CheckPassword(newPassword, this.maxPasswordLength, "newPassword");
            if (newPassword.Length < this.MinRequiredPasswordLength)
            {
                throw new ArgumentException(System.Web.SR.GetString("Password_too_short", new object[] { "newPassword", this.MinRequiredPasswordLength.ToString(CultureInfo.InvariantCulture) }));
            }
            int num = 0;
            for (int i = 0; i < newPassword.Length; i++)
            {
                if (!char.IsLetterOrDigit(newPassword, i))
                {
                    num++;
                }
            }
            if (num < this.MinRequiredNonAlphanumericCharacters)
            {
                throw new ArgumentException(System.Web.SR.GetString("Password_need_more_non_alpha_numeric_chars", new object[] { "newPassword", this.MinRequiredNonAlphanumericCharacters.ToString(CultureInfo.InvariantCulture) }));
            }
            if ((this.PasswordStrengthRegularExpression.Length > 0) && !Regex.IsMatch(newPassword, this.PasswordStrengthRegularExpression))
            {
                throw new ArgumentException(System.Web.SR.GetString("Password_does_not_match_regular_expression", new object[] { "newPassword" }));
            }
            ValidatePasswordEventArgs e = new ValidatePasswordEventArgs(username, newPassword, false);
            this.OnValidatingPassword(e);
            if (e.Cancel)
            {
                if (e.FailureInformation != null)
                {
                    throw e.FailureInformation;
                }
                throw new ArgumentException(System.Web.SR.GetString("Membership_Custom_Password_Validation_Failure"), "newPassword");
            }
            try
            {
                DirectoryEntryHolder holder = ActiveDirectoryConnectionHelper.GetDirectoryEntry(this.directoryInfo, this.directoryInfo.ContainerDN, true);
                DirectoryEntry directoryEntry = holder.DirectoryEntry;
                DirectoryEntry userEntry = null;
                bool resetBadPasswordAnswerAttributes = false;
                string str = null;
                try
                {
                    if (this.EnablePasswordReset)
                    {
                        MembershipUser user = null;
                        if (((this.directoryInfo.DirectoryType == DirectoryType.AD) && this.usernameIsUPN) && (username.IndexOf('@') == -1))
                        {
                            string sAMAccountName = null;
                            user = this.FindUserAndSAMAccountName(directoryEntry, "(" + this.attributeMapUsername + "=" + this.GetEscapedFilterValue(username) + ")", out userEntry, out resetBadPasswordAnswerAttributes, out sAMAccountName);
                            str = this.directoryInfo.DomainName + @"\" + sAMAccountName;
                        }
                        else
                        {
                            user = this.FindUser(directoryEntry, "(" + this.attributeMapUsername + "=" + this.GetEscapedFilterValue(username) + ")", out userEntry, out resetBadPasswordAnswerAttributes);
                            str = username;
                        }
                        if ((user == null) || user.IsLockedOut)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (((this.directoryInfo.DirectoryType == DirectoryType.AD) && this.usernameIsUPN) && (username.IndexOf('@') == -1))
                        {
                            string str3 = null;
                            userEntry = this.FindUserEntryAndSAMAccountName(directoryEntry, "(" + this.attributeMapUsername + "=" + this.GetEscapedFilterValue(username) + ")", out str3);
                            str = this.directoryInfo.DomainName + @"\" + str3;
                        }
                        else
                        {
                            userEntry = this.FindUserEntry(directoryEntry, "(" + this.attributeMapUsername + "=" + this.GetEscapedFilterValue(username) + ")");
                            str = username;
                        }
                        if (userEntry == null)
                        {
                            return false;
                        }
                    }
                    userEntry.Username = this.usernameIsSAMAccountName ? (this.directoryInfo.DomainName + @"\" + str) : str;
                    userEntry.Password = oldPassword;
                    userEntry.AuthenticationType = this.directoryInfo.GetAuthenticationTypes(this.directoryInfo.ConnectionProtection, (this.directoryInfo.DirectoryType == DirectoryType.AD) ? CredentialsType.Windows : CredentialsType.NonWindows);
                    try
                    {
                        this.SetPasswordPortIfApplicable(userEntry);
                        userEntry.Invoke("ChangePassword", new object[] { oldPassword, newPassword });
                    }
                    catch (COMException exception)
                    {
                        if (exception.ErrorCode != -2147023570)
                        {
                            throw;
                        }
                        return false;
                    }
                    catch (TargetInvocationException exception2)
                    {
                        if (!(exception2.InnerException is COMException))
                        {
                            throw;
                        }
                        COMException innerException = (COMException) exception2.InnerException;
                        int errorCode = innerException.ErrorCode;
                        switch (errorCode)
                        {
                            case -2147022651:
                            case -2147016657:
                            case -2147023571:
                            case -2147023569:
                                throw new MembershipPasswordException(System.Web.SR.GetString("Membership_InvalidPassword"), innerException);
                        }
                        if ((errorCode == -2147463155) && (this.directoryInfo.DirectoryType == DirectoryType.ADAM))
                        {
                            throw new ProviderException(System.Web.SR.GetString("ADMembership_No_secure_conn_for_password"));
                        }
                        throw;
                    }
                    if (this.EnablePasswordReset && resetBadPasswordAnswerAttributes)
                    {
                        userEntry.Username = this.directoryInfo.GetUsername();
                        userEntry.Password = this.directoryInfo.GetPassword();
                        userEntry.AuthenticationType = this.directoryInfo.AuthenticationTypes;
                        this.ResetBadPasswordAnswerAttributes(userEntry);
                    }
                }
                finally
                {
                    if (userEntry != null)
                    {
                        userEntry.Dispose();
                    }
                    holder.Close();
                }
            }
            catch
            {
                throw;
            }
            return true;
        }

        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            string str;
            if (!this.initialized)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("ADMembership_Provider_not_initialized"));
            }
            if ((newPasswordQuestion != null) && (this.attributeMapPasswordQuestion == null))
            {
                throw new NotSupportedException(System.Web.SR.GetString("ADMembership_PasswordQ_not_supported"));
            }
            if ((newPasswordAnswer != null) && (this.attributeMapPasswordAnswer == null))
            {
                throw new NotSupportedException(System.Web.SR.GetString("ADMembership_PasswordA_not_supported"));
            }
            this.CheckUserName(ref username, this.maxUsernameLength, "username");
            this.CheckPassword(password, this.maxPasswordLength, "password");
            SecUtility.CheckParameter(ref newPasswordQuestion, this.RequiresQuestionAndAnswer, true, false, this.maxPasswordQuestionLength, "newPasswordQuestion");
            this.CheckPasswordAnswer(ref newPasswordAnswer, this.RequiresQuestionAndAnswer, this.maxPasswordAnswerLength, "newPasswordAnswer");
            if (!string.IsNullOrEmpty(newPasswordAnswer))
            {
                str = this.Encrypt(newPasswordAnswer);
                if ((this.maxPasswordAnswerLength > 0) && (str.Length > this.maxPasswordAnswerLength))
                {
                    throw new ArgumentException(System.Web.SR.GetString("ADMembership_Parameter_too_long", new object[] { "newPasswordAnswer" }), "newPasswordAnswer");
                }
            }
            else
            {
                str = newPasswordAnswer;
            }
            try
            {
                DirectoryEntryHolder holder = ActiveDirectoryConnectionHelper.GetDirectoryEntry(this.directoryInfo, this.directoryInfo.ContainerDN, true);
                DirectoryEntry directoryEntry = holder.DirectoryEntry;
                DirectoryEntry userEntry = null;
                bool resetBadPasswordAnswerAttributes = false;
                string str2 = null;
                try
                {
                    if (this.EnablePasswordReset)
                    {
                        MembershipUser user = null;
                        if (((this.directoryInfo.DirectoryType == DirectoryType.AD) && this.usernameIsUPN) && (username.IndexOf('@') == -1))
                        {
                            string sAMAccountName = null;
                            user = this.FindUserAndSAMAccountName(directoryEntry, "(" + this.attributeMapUsername + "=" + this.GetEscapedFilterValue(username) + ")", out userEntry, out resetBadPasswordAnswerAttributes, out sAMAccountName);
                            str2 = this.directoryInfo.DomainName + @"\" + sAMAccountName;
                        }
                        else
                        {
                            user = this.FindUser(directoryEntry, "(" + this.attributeMapUsername + "=" + this.GetEscapedFilterValue(username) + ")", out userEntry, out resetBadPasswordAnswerAttributes);
                            str2 = username;
                        }
                        if ((user == null) || user.IsLockedOut)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (((this.directoryInfo.DirectoryType == DirectoryType.AD) && this.usernameIsUPN) && (username.IndexOf('@') == -1))
                        {
                            string str4 = null;
                            userEntry = this.FindUserEntryAndSAMAccountName(directoryEntry, "(" + this.attributeMapUsername + "=" + this.GetEscapedFilterValue(username) + ")", out str4);
                            str2 = this.directoryInfo.DomainName + @"\" + str4;
                        }
                        else
                        {
                            userEntry = this.FindUserEntry(directoryEntry, "(" + this.attributeMapUsername + "=" + this.GetEscapedFilterValue(username) + ")");
                            str2 = username;
                        }
                        if (userEntry == null)
                        {
                            return false;
                        }
                    }
                    if (!this.ValidateCredentials(str2, password))
                    {
                        return false;
                    }
                    if (this.EnablePasswordReset && resetBadPasswordAnswerAttributes)
                    {
                        userEntry.Properties[this.attributeMapFailedPasswordAnswerCount].Value = 0;
                        userEntry.Properties[this.attributeMapFailedPasswordAnswerTime].Value = 0;
                        userEntry.Properties[this.attributeMapFailedPasswordAnswerLockoutTime].Value = 0;
                    }
                    if (newPasswordQuestion == null)
                    {
                        if ((this.attributeMapPasswordQuestion != null) && userEntry.Properties.Contains(this.attributeMapPasswordQuestion))
                        {
                            userEntry.Properties[this.attributeMapPasswordQuestion].Clear();
                        }
                    }
                    else
                    {
                        userEntry.Properties[this.attributeMapPasswordQuestion].Value = newPasswordQuestion;
                    }
                    if (newPasswordAnswer == null)
                    {
                        if ((this.attributeMapPasswordAnswer != null) && userEntry.Properties.Contains(this.attributeMapPasswordAnswer))
                        {
                            userEntry.Properties[this.attributeMapPasswordAnswer].Clear();
                        }
                    }
                    else
                    {
                        userEntry.Properties[this.attributeMapPasswordAnswer].Value = str;
                    }
                    userEntry.CommitChanges();
                }
                finally
                {
                    if (userEntry != null)
                    {
                        userEntry.Dispose();
                    }
                    holder.Close();
                }
            }
            catch
            {
                throw;
            }
            return true;
        }

        private void CheckPassword(string password, int maxSize, string paramName)
        {
            if (password == null)
            {
                throw new ArgumentNullException(paramName);
            }
            if (password.Trim().Length < 1)
            {
                throw new ArgumentException(System.Web.SR.GetString("Parameter_can_not_be_empty", new object[] { paramName }), paramName);
            }
            if ((maxSize > 0) && (password.Length > maxSize))
            {
                throw new ArgumentException(System.Web.SR.GetString("Parameter_too_long", new object[] { paramName, maxSize.ToString(CultureInfo.InvariantCulture) }), paramName);
            }
        }

        private void CheckPasswordAnswer(ref string passwordAnswer, bool checkForNull, int maxSize, string paramName)
        {
            if (passwordAnswer == null)
            {
                if (checkForNull)
                {
                    throw new ArgumentNullException(paramName);
                }
            }
            else
            {
                passwordAnswer = passwordAnswer.Trim();
                if (passwordAnswer.Length < 1)
                {
                    throw new ArgumentException(System.Web.SR.GetString("Parameter_can_not_be_empty", new object[] { paramName }), paramName);
                }
                if ((maxSize > 0) && (passwordAnswer.Length > maxSize))
                {
                    throw new ArgumentException(System.Web.SR.GetString("ADMembership_Parameter_too_long", new object[] { paramName }), paramName);
                }
            }
        }

        private void CheckUserName(ref string username, int maxSize, string paramName)
        {
            SecUtility.CheckParameter(ref username, true, true, true, maxSize, paramName);
            if (this.usernameIsUPN && (username.IndexOf('\\') != -1))
            {
                throw new ArgumentException(System.Web.SR.GetString("ADMembership_UPN_contains_backslash", new object[] { paramName }), paramName);
            }
        }

        [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true), DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            string str;
            status = MembershipCreateStatus.Success;
            MembershipUser user = null;
            if (!this.initialized)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("ADMembership_Provider_not_initialized"));
            }
            if (providerUserKey != null)
            {
                throw new NotSupportedException(System.Web.SR.GetString("ADMembership_Setting_UserId_not_supported"));
            }
            if ((passwordQuestion != null) && (this.attributeMapPasswordQuestion == null))
            {
                throw new NotSupportedException(System.Web.SR.GetString("ADMembership_PasswordQ_not_supported"));
            }
            if ((passwordAnswer != null) && (this.attributeMapPasswordAnswer == null))
            {
                throw new NotSupportedException(System.Web.SR.GetString("ADMembership_PasswordA_not_supported"));
            }
            if (!SecUtility.ValidateParameter(ref username, true, true, true, this.maxUsernameLengthForCreation))
            {
                status = MembershipCreateStatus.InvalidUserName;
                return null;
            }
            if (this.usernameIsUPN && (username.IndexOf('\\') != -1))
            {
                status = MembershipCreateStatus.InvalidUserName;
                return null;
            }
            if (!this.ValidatePassword(password, this.maxPasswordLength))
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }
            if (!SecUtility.ValidateParameter(ref email, this.RequiresUniqueEmail, true, false, this.maxEmailLength))
            {
                status = MembershipCreateStatus.InvalidEmail;
                return null;
            }
            if (!SecUtility.ValidateParameter(ref passwordQuestion, this.RequiresQuestionAndAnswer, true, false, this.maxPasswordQuestionLength))
            {
                status = MembershipCreateStatus.InvalidQuestion;
                return null;
            }
            if (!SecUtility.ValidateParameter(ref passwordAnswer, this.RequiresQuestionAndAnswer, true, false, this.maxPasswordAnswerLength))
            {
                status = MembershipCreateStatus.InvalidAnswer;
                return null;
            }
            if (!string.IsNullOrEmpty(passwordAnswer))
            {
                str = this.Encrypt(passwordAnswer);
                if ((this.maxPasswordAnswerLength > 0) && (str.Length > this.maxPasswordAnswerLength))
                {
                    status = MembershipCreateStatus.InvalidAnswer;
                    return null;
                }
            }
            else
            {
                str = passwordAnswer;
            }
            if (password.Length < this.MinRequiredPasswordLength)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }
            int num = 0;
            for (int i = 0; i < password.Length; i++)
            {
                if (!char.IsLetterOrDigit(password, i))
                {
                    num++;
                }
            }
            if (num < this.MinRequiredNonAlphanumericCharacters)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }
            if ((this.PasswordStrengthRegularExpression.Length > 0) && !Regex.IsMatch(password, this.PasswordStrengthRegularExpression))
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }
            ValidatePasswordEventArgs e = new ValidatePasswordEventArgs(username, password, true);
            this.OnValidatingPassword(e);
            if (e.Cancel)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }
            try
            {
                DirectoryEntryHolder holder = ActiveDirectoryConnectionHelper.GetDirectoryEntry(this.directoryInfo, this.directoryInfo.CreationContainerDN, true);
                DirectoryEntry containerEntry = null;
                DirectoryEntry userEntry = null;
                try
                {
                    string str3;
                    containerEntry = holder.DirectoryEntry;
                    containerEntry.AuthenticationType |= AuthenticationTypes.FastBind;
                    userEntry = containerEntry.Children.Add(this.GetEscapedRdn("CN=" + username), "user");
                    if (this.directoryInfo.DirectoryType == DirectoryType.AD)
                    {
                        string str2 = null;
                        bool flag = false;
                        if (this.usernameIsSAMAccountName)
                        {
                            str2 = username;
                            flag = true;
                        }
                        else if (this.GetDomainControllerLevel(containerEntry.Options.GetCurrentServerName()) != 2)
                        {
                            str2 = this.GenerateAccountName();
                            flag = true;
                        }
                        if (flag)
                        {
                            userEntry.Properties["sAMAccountName"].Value = str2;
                        }
                    }
                    if (this.usernameIsUPN)
                    {
                        if ((this.directoryInfo.DirectoryType == DirectoryType.AD) && !this.IsUpnUnique(username))
                        {
                            status = MembershipCreateStatus.DuplicateUserName;
                            return null;
                        }
                        userEntry.Properties["userPrincipalName"].Value = username;
                    }
                    if (email != null)
                    {
                        if (this.RequiresUniqueEmail && !this.IsEmailUnique(containerEntry, username, email, false))
                        {
                            status = MembershipCreateStatus.DuplicateEmail;
                            return null;
                        }
                        userEntry.Properties[this.attributeMapEmail].Value = email;
                    }
                    if (passwordQuestion != null)
                    {
                        userEntry.Properties[this.attributeMapPasswordQuestion].Value = passwordQuestion;
                    }
                    if (passwordAnswer != null)
                    {
                        userEntry.Properties[this.attributeMapPasswordAnswer].Value = str;
                    }
                    try
                    {
                        userEntry.CommitChanges();
                    }
                    catch (COMException exception)
                    {
                        if ((exception.ErrorCode == -2147019886) || (exception.ErrorCode == -2147016691))
                        {
                            status = MembershipCreateStatus.DuplicateUserName;
                            return null;
                        }
                        if ((exception.ErrorCode != -2147024865) || !(exception is DirectoryServicesCOMException))
                        {
                            throw;
                        }
                        DirectoryServicesCOMException exception2 = exception as DirectoryServicesCOMException;
                        if (exception2.ExtendedError != 0x523)
                        {
                            throw;
                        }
                        status = MembershipCreateStatus.InvalidUserName;
                        return null;
                    }
                    try
                    {
                        this.SetPasswordPortIfApplicable(userEntry);
                        userEntry.Invoke("SetPassword", new object[] { password });
                        if (isApproved)
                        {
                            if (this.directoryInfo.DirectoryType == DirectoryType.AD)
                            {
                                int propertyValue = (int) PropertyManager.GetPropertyValue(userEntry, "userAccountControl");
                                propertyValue &= -35;
                                userEntry.Properties["userAccountControl"].Value = propertyValue;
                            }
                            else
                            {
                                userEntry.Properties["msDS-UserAccountDisabled"].Value = false;
                            }
                            userEntry.CommitChanges();
                        }
                        else if (this.directoryInfo.DirectoryType == DirectoryType.ADAM)
                        {
                            userEntry.Properties["msDS-UserAccountDisabled"].Value = true;
                            userEntry.CommitChanges();
                        }
                        if (this.directoryInfo.DirectoryType == DirectoryType.ADAM)
                        {
                            DirectoryEntry entry3 = new DirectoryEntry(this.directoryInfo.GetADsPath("CN=Readers,CN=Roles," + this.directoryInfo.ADAMPartitionDN), this.directoryInfo.GetUsername(), this.directoryInfo.GetPassword(), this.directoryInfo.AuthenticationTypes);
                            entry3.Properties["member"].Add(PropertyManager.GetPropertyValue(userEntry, "distinguishedName"));
                            entry3.CommitChanges();
                        }
                    }
                    catch (COMException)
                    {
                        containerEntry.Children.Remove(userEntry);
                        throw;
                    }
                    catch (ProviderException)
                    {
                        containerEntry.Children.Remove(userEntry);
                        throw;
                    }
                    catch (TargetInvocationException exception3)
                    {
                        containerEntry.Children.Remove(userEntry);
                        if (!(exception3.InnerException is COMException))
                        {
                            throw;
                        }
                        COMException innerException = (COMException) exception3.InnerException;
                        int errorCode = innerException.ErrorCode;
                        switch (errorCode)
                        {
                            case -2147022651:
                            case -2147016657:
                            case -2147023571:
                            case -2147023569:
                                status = MembershipCreateStatus.InvalidPassword;
                                return null;
                        }
                        if ((errorCode == -2147463155) && (this.directoryInfo.DirectoryType == DirectoryType.ADAM))
                        {
                            throw new ProviderException(System.Web.SR.GetString("ADMembership_No_secure_conn_for_password"));
                        }
                        throw;
                    }
                    DirectoryEntry entry4 = null;
                    bool resetBadPasswordAnswerAttributes = false;
                    user = this.FindUser(userEntry, "(objectClass=*)", System.DirectoryServices.SearchScope.Base, false, out entry4, out resetBadPasswordAnswerAttributes, out str3);
                }
                finally
                {
                    if (userEntry != null)
                    {
                        userEntry.Dispose();
                    }
                    holder.Close();
                }
            }
            catch
            {
                throw;
            }
            return user;
        }

        private string Decrypt(string encryptedString)
        {
            byte[] encodedPassword = Convert.FromBase64String(encryptedString);
            byte[] bytes = this.DecryptPassword(encodedPassword);
            return Encoding.Unicode.GetString(bytes, 0x10, bytes.Length - 0x10);
        }

        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            if (!this.initialized)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("ADMembership_Provider_not_initialized"));
            }
            this.CheckUserName(ref username, this.maxUsernameLength, "username");
            try
            {
                DirectoryEntryHolder holder = ActiveDirectoryConnectionHelper.GetDirectoryEntry(this.directoryInfo, this.directoryInfo.CreationContainerDN, true);
                DirectoryEntry directoryEntry = holder.DirectoryEntry;
                directoryEntry.AuthenticationType |= AuthenticationTypes.FastBind;
                DirectoryEntry entry = null;
                try
                {
                    string str;
                    entry = this.FindUserEntry(directoryEntry, "(" + this.attributeMapUsername + "=" + this.GetEscapedFilterValue(username) + ")", System.DirectoryServices.SearchScope.OneLevel, false, out str);
                    if (entry == null)
                    {
                        return false;
                    }
                    directoryEntry.Children.Remove(entry);
                }
                catch (COMException exception)
                {
                    if (exception.ErrorCode != -2147016656)
                    {
                        throw;
                    }
                    return false;
                }
                finally
                {
                    if (entry != null)
                    {
                        entry.Dispose();
                    }
                    holder.Close();
                }
            }
            catch
            {
                throw;
            }
            return true;
        }

        private string Encrypt(string clearTextString)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(clearTextString);
            byte[] data = new byte[0x10];
            new RNGCryptoServiceProvider().GetBytes(data);
            byte[] dst = new byte[data.Length + bytes.Length];
            Buffer.BlockCopy(data, 0, dst, 0, data.Length);
            Buffer.BlockCopy(bytes, 0, dst, data.Length, bytes.Length);
            return Convert.ToBase64String(this.EncryptPassword(dst, this._LegacyPasswordCompatibilityMode));
        }

        private MembershipUser FindUser(DirectoryEntry containerEntry, string filter, out DirectoryEntry userEntry, out bool resetBadPasswordAnswerAttributes)
        {
            string str;
            return this.FindUser(containerEntry, filter, System.DirectoryServices.SearchScope.Subtree, false, out userEntry, out resetBadPasswordAnswerAttributes, out str);
        }

        private MembershipUser FindUser(DirectoryEntry containerEntry, string filter, System.DirectoryServices.SearchScope searchScope, bool retrieveSAMAccountName, out DirectoryEntry userEntry, out bool resetBadPasswordAnswerAttributes, out string sAMAccountName)
        {
            MembershipUser membershipUserFromSearchResult = null;
            DirectorySearcher searcher = new DirectorySearcher(containerEntry) {
                SearchScope = searchScope,
                Filter = "(&(objectCategory=person)(objectClass=user)" + filter + ")"
            };
            if (this.directoryInfo.ClientSearchTimeout != -1)
            {
                searcher.ClientTimeout = new TimeSpan(0, this.directoryInfo.ClientSearchTimeout, 0);
            }
            if (this.directoryInfo.ServerSearchTimeout != -1)
            {
                searcher.ServerPageTimeLimit = new TimeSpan(0, this.directoryInfo.ServerSearchTimeout, 0);
            }
            searcher.PropertiesToLoad.Add(this.attributeMapUsername);
            searcher.PropertiesToLoad.Add("objectSid");
            searcher.PropertiesToLoad.Add(this.attributeMapEmail);
            searcher.PropertiesToLoad.Add("comment");
            searcher.PropertiesToLoad.Add("whenCreated");
            searcher.PropertiesToLoad.Add("pwdLastSet");
            searcher.PropertiesToLoad.Add("msDS-User-Account-Control-Computed");
            searcher.PropertiesToLoad.Add("lockoutTime");
            if (retrieveSAMAccountName)
            {
                searcher.PropertiesToLoad.Add("sAMAccountName");
            }
            if (this.attributeMapPasswordQuestion != null)
            {
                searcher.PropertiesToLoad.Add(this.attributeMapPasswordQuestion);
            }
            if (this.directoryInfo.DirectoryType == DirectoryType.AD)
            {
                searcher.PropertiesToLoad.Add("userAccountControl");
            }
            else
            {
                searcher.PropertiesToLoad.Add("msDS-UserAccountDisabled");
            }
            if (this.EnablePasswordReset)
            {
                searcher.PropertiesToLoad.Add(this.attributeMapFailedPasswordAnswerCount);
                searcher.PropertiesToLoad.Add(this.attributeMapFailedPasswordAnswerTime);
                searcher.PropertiesToLoad.Add(this.attributeMapFailedPasswordAnswerLockoutTime);
            }
            SearchResult res = searcher.FindOne();
            resetBadPasswordAnswerAttributes = false;
            sAMAccountName = null;
            if (res != null)
            {
                membershipUserFromSearchResult = this.GetMembershipUserFromSearchResult(res);
                userEntry = res.GetDirectoryEntry();
                if (retrieveSAMAccountName)
                {
                    sAMAccountName = (string) PropertyManager.GetSearchResultPropertyValue(res, "sAMAccountName");
                }
                if (this.EnablePasswordReset && res.Properties.Contains(this.attributeMapFailedPasswordAnswerCount))
                {
                    resetBadPasswordAnswerAttributes = ((int) PropertyManager.GetSearchResultPropertyValue(res, this.attributeMapFailedPasswordAnswerCount)) > 0;
                }
                return membershipUserFromSearchResult;
            }
            userEntry = null;
            return membershipUserFromSearchResult;
        }

        private MembershipUser FindUserAndSAMAccountName(DirectoryEntry containerEntry, string filter, out DirectoryEntry userEntry, out bool resetBadPasswordAnswerAttributes, out string sAMAccountName)
        {
            return this.FindUser(containerEntry, filter, System.DirectoryServices.SearchScope.Subtree, true, out userEntry, out resetBadPasswordAnswerAttributes, out sAMAccountName);
        }

        private DirectoryEntry FindUserEntry(DirectoryEntry containerEntry, string filter)
        {
            string str;
            return this.FindUserEntry(containerEntry, filter, System.DirectoryServices.SearchScope.Subtree, false, out str);
        }

        private DirectoryEntry FindUserEntry(DirectoryEntry containerEntry, string filter, System.DirectoryServices.SearchScope searchScope, bool retrieveSAMAccountName, out string sAMAccountName)
        {
            DirectorySearcher searcher = new DirectorySearcher(containerEntry) {
                SearchScope = searchScope,
                Filter = "(&(objectCategory=person)(objectClass=user)" + filter + ")"
            };
            if (this.directoryInfo.ClientSearchTimeout != -1)
            {
                searcher.ClientTimeout = new TimeSpan(0, this.directoryInfo.ClientSearchTimeout, 0);
            }
            if (this.directoryInfo.ServerSearchTimeout != -1)
            {
                searcher.ServerPageTimeLimit = new TimeSpan(0, this.directoryInfo.ServerSearchTimeout, 0);
            }
            if (retrieveSAMAccountName)
            {
                searcher.PropertiesToLoad.Add("sAMAccountName");
            }
            SearchResult res = searcher.FindOne();
            sAMAccountName = null;
            if (res == null)
            {
                return null;
            }
            if (retrieveSAMAccountName)
            {
                sAMAccountName = (string) PropertyManager.GetSearchResultPropertyValue(res, "sAMAccountName");
            }
            return res.GetDirectoryEntry();
        }

        private DirectoryEntry FindUserEntryAndSAMAccountName(DirectoryEntry containerEntry, string filter, out string sAMAccountName)
        {
            return this.FindUserEntry(containerEntry, filter, System.DirectoryServices.SearchScope.Subtree, true, out sAMAccountName);
        }

        private MembershipUserCollection FindUsers(DirectoryEntry containerEntry, string filter, string sortKey, int pageIndex, int pageSize, out int totalRecords)
        {
            MembershipUserCollection users = new MembershipUserCollection();
            int num = (pageIndex + 1) * pageSize;
            int num2 = (num - pageSize) + 1;
            DirectorySearcher searcher = new DirectorySearcher(containerEntry) {
                SearchScope = System.DirectoryServices.SearchScope.Subtree,
                Filter = "(&(objectCategory=person)(objectClass=user)" + filter + ")"
            };
            if (this.directoryInfo.ClientSearchTimeout != -1)
            {
                searcher.ClientTimeout = new TimeSpan(0, this.directoryInfo.ClientSearchTimeout, 0);
            }
            if (this.directoryInfo.ServerSearchTimeout != -1)
            {
                searcher.ServerPageTimeLimit = new TimeSpan(0, this.directoryInfo.ServerSearchTimeout, 0);
            }
            searcher.PropertiesToLoad.Add(this.attributeMapUsername);
            searcher.PropertiesToLoad.Add("objectSid");
            searcher.PropertiesToLoad.Add(this.attributeMapEmail);
            searcher.PropertiesToLoad.Add("comment");
            searcher.PropertiesToLoad.Add("whenCreated");
            searcher.PropertiesToLoad.Add("pwdLastSet");
            searcher.PropertiesToLoad.Add("msDS-User-Account-Control-Computed");
            searcher.PropertiesToLoad.Add("lockoutTime");
            if (this.attributeMapPasswordQuestion != null)
            {
                searcher.PropertiesToLoad.Add(this.attributeMapPasswordQuestion);
            }
            if (this.directoryInfo.DirectoryType == DirectoryType.AD)
            {
                searcher.PropertiesToLoad.Add("userAccountControl");
            }
            else
            {
                searcher.PropertiesToLoad.Add("msDS-UserAccountDisabled");
            }
            if (this.EnablePasswordReset)
            {
                searcher.PropertiesToLoad.Add(this.attributeMapFailedPasswordAnswerCount);
                searcher.PropertiesToLoad.Add(this.attributeMapFailedPasswordAnswerTime);
                searcher.PropertiesToLoad.Add(this.attributeMapFailedPasswordAnswerLockoutTime);
            }
            searcher.PageSize = 0x200;
            searcher.Sort = new SortOption(sortKey, SortDirection.Ascending);
            using (SearchResultCollection results = searcher.FindAll())
            {
                int num3 = 0;
                totalRecords = 0;
                foreach (SearchResult result in results)
                {
                    num3++;
                    if ((num3 >= num2) && (num3 <= num))
                    {
                        users.Add(this.GetMembershipUserFromSearchResult(result));
                    }
                }
                totalRecords = num3;
            }
            return users;
        }

        [DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true), DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            MembershipUserCollection users;
            if (!this.initialized)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("ADMembership_Provider_not_initialized"));
            }
            if (!this.EnableSearchMethods)
            {
                throw new NotSupportedException(System.Web.SR.GetString("ADMembership_Provider_SearchMethods_not_supported"));
            }
            SecUtility.CheckParameter(ref emailToMatch, false, true, false, this.maxEmailLength, "emailToMatch");
            if (pageIndex < 0)
            {
                throw new ArgumentException(System.Web.SR.GetString("PageIndex_bad"), "pageIndex");
            }
            if (pageSize < 1)
            {
                throw new ArgumentException(System.Web.SR.GetString("PageSize_bad"), "pageSize");
            }
            long num = ((pageIndex * pageSize) + pageSize) - 1L;
            if (num > 0x7fffffffL)
            {
                throw new ArgumentException(System.Web.SR.GetString("PageIndex_PageSize_bad"), "pageIndex and pageSize");
            }
            try
            {
                DirectoryEntryHolder holder = ActiveDirectoryConnectionHelper.GetDirectoryEntry(this.directoryInfo, this.directoryInfo.ContainerDN, true);
                DirectoryEntry directoryEntry = holder.DirectoryEntry;
                try
                {
                    totalRecords = 0;
                    string filter = null;
                    if (emailToMatch != null)
                    {
                        filter = "(" + this.attributeMapUsername + "=*)(" + this.attributeMapEmail + "=" + this.GetEscapedFilterValue(emailToMatch, false) + ")";
                    }
                    else
                    {
                        filter = "(" + this.attributeMapUsername + "=*)(!(" + this.attributeMapEmail + "=*))";
                    }
                    users = this.FindUsers(directoryEntry, filter, this.attributeMapEmail, pageIndex, pageSize, out totalRecords);
                }
                finally
                {
                    holder.Close();
                }
            }
            catch
            {
                throw;
            }
            return users;
        }

        [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            MembershipUserCollection users;
            if (!this.initialized)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("ADMembership_Provider_not_initialized"));
            }
            if (!this.EnableSearchMethods)
            {
                throw new NotSupportedException(System.Web.SR.GetString("ADMembership_Provider_SearchMethods_not_supported"));
            }
            SecUtility.CheckParameter(ref usernameToMatch, true, true, true, this.maxUsernameLength, "usernameToMatch");
            if (pageIndex < 0)
            {
                throw new ArgumentException(System.Web.SR.GetString("PageIndex_bad"), "pageIndex");
            }
            if (pageSize < 1)
            {
                throw new ArgumentException(System.Web.SR.GetString("PageSize_bad"), "pageSize");
            }
            long num = ((pageIndex * pageSize) + pageSize) - 1L;
            if (num > 0x7fffffffL)
            {
                throw new ArgumentException(System.Web.SR.GetString("PageIndex_PageSize_bad"), "pageIndex and pageSize");
            }
            try
            {
                DirectoryEntryHolder holder = ActiveDirectoryConnectionHelper.GetDirectoryEntry(this.directoryInfo, this.directoryInfo.ContainerDN, true);
                DirectoryEntry directoryEntry = holder.DirectoryEntry;
                try
                {
                    totalRecords = 0;
                    users = this.FindUsers(directoryEntry, "(" + this.attributeMapUsername + "=" + this.GetEscapedFilterValue(usernameToMatch, false) + ")", this.attributeMapUsername, pageIndex, pageSize, out totalRecords);
                }
                finally
                {
                    holder.Close();
                }
            }
            catch
            {
                throw;
            }
            return users;
        }

        private string GenerateAccountName()
        {
            char[] chArray = new char[] { 
                '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 
                'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V'
             };
            char[] chArray2 = new char[20];
            byte[] data = new byte[12];
            new RNGCryptoServiceProvider().GetBytes(data);
            uint num = 0;
            uint num2 = 0;
            uint num3 = 0;
            for (int i = 0; i < 4; i++)
            {
                num |= (uint) (data[i] << (8 * i));
            }
            for (int j = 0; j < 4; j++)
            {
                num2 |= (uint) (data[4 + j] << (8 * j));
            }
            for (int k = 0; k < 4; k++)
            {
                num3 |= (uint) (data[8 + k] << (8 * k));
            }
            chArray2[0] = '$';
            for (int m = 1; m <= 6; m++)
            {
                chArray2[m] = chArray[(int) ((IntPtr) (num & 0x1f))];
                num = num >> 5;
            }
            chArray2[7] = '-';
            for (int n = 8; n <= 13; n++)
            {
                chArray2[n] = chArray[(int) ((IntPtr) (num2 & 0x1f))];
                num2 = num2 >> 5;
            }
            for (int num9 = 13; num9 <= 0x13; num9++)
            {
                chArray2[num9] = chArray[(int) ((IntPtr) (num3 & 0x1f))];
                num3 = num3 >> 5;
            }
            return new string(chArray2);
        }

        public virtual string GeneratePassword()
        {
            return Membership.GeneratePassword((this.MinRequiredPasswordLength < 14) ? 14 : this.MinRequiredPasswordLength, this.MinRequiredNonAlphanumericCharacters);
        }

        [DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            return this.FindUsersByName("*", pageIndex, pageSize, out totalRecords);
        }

        private string GetAttributeMapping(NameValueCollection config, string valueName, out int maxLength)
        {
            string attributeName = config[valueName];
            maxLength = -1;
            if (attributeName == null)
            {
                return null;
            }
            attributeName = attributeName.Trim();
            if (attributeName.Length == 0)
            {
                throw new ProviderException(System.Web.SR.GetString("ADMembership_Schema_mappings_must_not_be_empty", new object[] { valueName }));
            }
            return this.GetValidatedSchemaMapping(valueName, attributeName, out maxLength);
        }

        private string GetConnectionString(string connectionStringName, bool appLevel)
        {
            if (string.IsNullOrEmpty(connectionStringName))
            {
                return null;
            }
            RuntimeConfig config = appLevel ? RuntimeConfig.GetAppConfig() : RuntimeConfig.GetConfig();
            ConnectionStringSettings settings = config.ConnectionStrings.ConnectionStrings[connectionStringName];
            if (settings == null)
            {
                throw new ProviderException(System.Web.SR.GetString("Connection_string_not_found", new object[] { connectionStringName }));
            }
            return settings.ConnectionString;
        }

        private DateTime GetDateTimeFromLargeInteger(NativeComInterfaces.IAdsLargeInteger largeIntValue)
        {
            long fileTime = (largeIntValue.HighPart * 0x100000000L) + ((uint) largeIntValue.LowPart);
            return DateTime.FromFileTimeUtc(fileTime);
        }

        private int GetDomainControllerLevel(string serverName)
        {
            int num = 0;
            DirectoryEntry entry = new DirectoryEntry("LDAP://" + serverName + "/RootDSE", this.directoryInfo.GetUsername(), this.directoryInfo.GetPassword(), this.directoryInfo.AuthenticationTypes);
            string s = (string) entry.Properties["domainControllerFunctionality"].Value;
            if (s != null)
            {
                num = int.Parse(s, NumberFormatInfo.InvariantInfo);
            }
            return num;
        }

        internal string GetEscapedFilterValue(string filterValue)
        {
            return this.GetEscapedFilterValue(filterValue, true);
        }

        internal string GetEscapedFilterValue(string filterValue, bool escapeWildChar)
        {
            int length = -1;
            char[] anyOf = new char[] { '(', ')', '*', '\\' };
            char[] chArray2 = new char[] { '(', ')', '\\' };
            length = escapeWildChar ? filterValue.IndexOfAny(anyOf) : filterValue.IndexOfAny(chArray2);
            if (length == -1)
            {
                return filterValue;
            }
            StringBuilder builder = new StringBuilder(2 * filterValue.Length);
            builder.Append(filterValue.Substring(0, length));
            for (int i = length; i < filterValue.Length; i++)
            {
                switch (filterValue[i])
                {
                    case '(':
                    {
                        builder.Append(@"\28");
                        continue;
                    }
                    case ')':
                    {
                        builder.Append(@"\29");
                        continue;
                    }
                    case '*':
                    {
                        if (!escapeWildChar)
                        {
                            break;
                        }
                        builder.Append(@"\2A");
                        continue;
                    }
                    case '\\':
                    {
                        if ((!escapeWildChar && ((filterValue.Length - i) >= 3)) && ((filterValue[i + 1] == '2') && ((filterValue[i + 2] == 'A') || (filterValue[i + 2] == 'a'))))
                        {
                            goto Label_0119;
                        }
                        builder.Append(@"\5C");
                        continue;
                    }
                    default:
                        goto Label_0127;
                }
                builder.Append("*");
                continue;
            Label_0119:
                builder.Append(@"\");
                continue;
            Label_0127:
                builder.Append(filterValue[i]);
            }
            return builder.ToString();
        }

        private string GetEscapedRdn(string rdn)
        {
            NativeComInterfaces.IAdsPathname pathname = (NativeComInterfaces.IAdsPathname) new NativeComInterfaces.Pathname();
            return pathname.GetEscapedElement(0, rdn);
        }

        private NativeComInterfaces.IAdsLargeInteger GetLargeIntegerFromDateTime(DateTime dateTimeValue)
        {
            long num = dateTimeValue.ToFileTimeUtc();
            NativeComInterfaces.IAdsLargeInteger integer = (NativeComInterfaces.IAdsLargeInteger) new NativeComInterfaces.LargeInteger();
            integer.HighPart = (int) (num >> 0x20);
            integer.LowPart = (int) (((ulong) num) & 0xffffffffL);
            return integer;
        }

        private MembershipUser GetMembershipUserFromSearchResult(SearchResult res)
        {
            bool flag;
            string searchResultPropertyValue = (string) PropertyManager.GetSearchResultPropertyValue(res, this.attributeMapUsername);
            byte[] binaryForm = (byte[]) PropertyManager.GetSearchResultPropertyValue(res, "objectSid");
            object providerUserKey = new SecurityIdentifier(binaryForm, 0);
            string email = res.Properties.Contains(this.attributeMapEmail) ? ((string) res.Properties[this.attributeMapEmail][0]) : null;
            string passwordQuestion = null;
            if ((this.attributeMapPasswordQuestion != null) && res.Properties.Contains(this.attributeMapPasswordQuestion))
            {
                passwordQuestion = (string) PropertyManager.GetSearchResultPropertyValue(res, this.attributeMapPasswordQuestion);
            }
            string comment = res.Properties.Contains("comment") ? ((string) res.Properties["comment"][0]) : null;
            bool isLockedOut = false;
            if (this.directoryInfo.DirectoryType == DirectoryType.AD)
            {
                int num = (int) PropertyManager.GetSearchResultPropertyValue(res, "userAccountControl");
                if ((num & 2) == 0)
                {
                    flag = true;
                }
                else
                {
                    flag = false;
                }
                if (res.Properties.Contains("msDS-User-Account-Control-Computed"))
                {
                    int num2 = (int) PropertyManager.GetSearchResultPropertyValue(res, "msDS-User-Account-Control-Computed");
                    if ((num2 & 0x10) != 0)
                    {
                        isLockedOut = true;
                    }
                }
                else if (res.Properties.Contains("lockoutTime"))
                {
                    DateTime time = DateTime.FromFileTimeUtc((long) PropertyManager.GetSearchResultPropertyValue(res, "lockoutTime"));
                    isLockedOut = DateTime.UtcNow.Subtract(time) <= this.directoryInfo.ADLockoutDuration;
                }
            }
            else
            {
                flag = true;
                if (res.Properties.Contains("msDS-UserAccountDisabled"))
                {
                    flag = !((bool) PropertyManager.GetSearchResultPropertyValue(res, "msDS-UserAccountDisabled"));
                }
                int num3 = (int) PropertyManager.GetSearchResultPropertyValue(res, "msDS-User-Account-Control-Computed");
                if ((num3 & 0x10) != 0)
                {
                    isLockedOut = true;
                }
            }
            DateTime defaultLastLockoutDate = this.DefaultLastLockoutDate;
            if (isLockedOut)
            {
                defaultLastLockoutDate = DateTime.FromFileTime((long) PropertyManager.GetSearchResultPropertyValue(res, "lockoutTime"));
            }
            if (this.EnablePasswordReset && res.Properties.Contains(this.attributeMapFailedPasswordAnswerLockoutTime))
            {
                DateTime time4 = DateTime.FromFileTimeUtc((long) PropertyManager.GetSearchResultPropertyValue(res, this.attributeMapFailedPasswordAnswerLockoutTime));
                if (DateTime.UtcNow.Subtract(time4) <= new TimeSpan(0, this.PasswordAnswerAttemptLockoutDuration, 0))
                {
                    if (isLockedOut)
                    {
                        if (DateTime.Compare(time4, DateTime.FromFileTimeUtc((long) PropertyManager.GetSearchResultPropertyValue(res, "lockoutTime"))) > 0)
                        {
                            defaultLastLockoutDate = DateTime.FromFileTime((long) PropertyManager.GetSearchResultPropertyValue(res, this.attributeMapFailedPasswordAnswerLockoutTime));
                        }
                    }
                    else
                    {
                        isLockedOut = true;
                        defaultLastLockoutDate = DateTime.FromFileTime((long) PropertyManager.GetSearchResultPropertyValue(res, this.attributeMapFailedPasswordAnswerLockoutTime));
                    }
                }
            }
            DateTime creationDate = ((DateTime) PropertyManager.GetSearchResultPropertyValue(res, "whenCreated")).ToLocalTime();
            DateTime minValue = DateTime.MinValue;
            DateTime lastActivityDate = DateTime.MinValue;
            return new ActiveDirectoryMembershipUser(this.Name, searchResultPropertyValue, binaryForm, providerUserKey, email, passwordQuestion, comment, flag, isLockedOut, creationDate, minValue, lastActivityDate, DateTime.FromFileTime((long) PropertyManager.GetSearchResultPropertyValue(res, "pwdLastSet")), defaultLastLockoutDate, true);
        }

        public override int GetNumberOfUsersOnline()
        {
            throw new NotSupportedException(System.Web.SR.GetString("ADMembership_OnlineUsers_not_supported"));
        }

        public override string GetPassword(string username, string passwordAnswer)
        {
            throw new NotSupportedException(System.Web.SR.GetString("ADMembership_PasswordRetrieval_not_supported_AD"));
        }

        private int GetRangeUpperForSchemaAttribute(string attributeName)
        {
            int num = -1;
            DirectoryEntry entry = new DirectoryEntry(this.directoryInfo.GetADsPath("schema") + "/" + attributeName, this.directoryInfo.GetUsername(), this.directoryInfo.GetPassword(), this.directoryInfo.AuthenticationTypes);
            try
            {
                num = (int) entry.InvokeGet("MaxRange");
            }
            catch (TargetInvocationException exception)
            {
                if (!(exception.InnerException is COMException) || (((COMException) exception.InnerException).ErrorCode != -2147463155))
                {
                    throw;
                }
                return num;
            }
            return num;
        }

        [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            MembershipUser user = null;
            if (!this.initialized)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("ADMembership_Provider_not_initialized"));
            }
            if (providerUserKey == null)
            {
                throw new ArgumentNullException("providerUserKey");
            }
            if (!(providerUserKey is SecurityIdentifier))
            {
                throw new ArgumentException(System.Web.SR.GetString("ADMembership_InvalidProviderUserKey"), "providerUserKey");
            }
            try
            {
                DirectoryEntryHolder holder = ActiveDirectoryConnectionHelper.GetDirectoryEntry(this.directoryInfo, this.directoryInfo.ContainerDN, true);
                DirectoryEntry directoryEntry = holder.DirectoryEntry;
                try
                {
                    DirectoryEntry entry2;
                    SecurityIdentifier identifier = providerUserKey as SecurityIdentifier;
                    StringBuilder builder = new StringBuilder();
                    int binaryLength = identifier.BinaryLength;
                    byte[] binaryForm = new byte[binaryLength];
                    identifier.GetBinaryForm(binaryForm, 0);
                    for (int i = 0; i < binaryLength; i++)
                    {
                        builder.Append(@"\");
                        builder.Append(binaryForm[i].ToString("x2", NumberFormatInfo.InvariantInfo));
                    }
                    bool resetBadPasswordAnswerAttributes = false;
                    user = this.FindUser(directoryEntry, "(" + this.attributeMapUsername + "=*)(objectSid=" + builder.ToString() + ")", out entry2, out resetBadPasswordAnswerAttributes);
                }
                finally
                {
                    holder.Close();
                }
            }
            catch
            {
                throw;
            }
            return user;
        }

        [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true), DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            MembershipUser user = null;
            if (!this.initialized)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("ADMembership_Provider_not_initialized"));
            }
            this.CheckUserName(ref username, this.maxUsernameLength, "username");
            try
            {
                DirectoryEntryHolder holder = ActiveDirectoryConnectionHelper.GetDirectoryEntry(this.directoryInfo, this.directoryInfo.ContainerDN, true);
                DirectoryEntry directoryEntry = holder.DirectoryEntry;
                try
                {
                    DirectoryEntry entry2;
                    bool resetBadPasswordAnswerAttributes = false;
                    user = this.FindUser(directoryEntry, "(" + this.attributeMapUsername + "=" + this.GetEscapedFilterValue(username) + ")", out entry2, out resetBadPasswordAnswerAttributes);
                }
                finally
                {
                    holder.Close();
                }
            }
            catch
            {
                throw;
            }
            return user;
        }

        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public override string GetUserNameByEmail(string email)
        {
            if (!this.initialized)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("ADMembership_Provider_not_initialized"));
            }
            SecUtility.CheckParameter(ref email, false, true, false, this.maxEmailLength, "email");
            string searchResultPropertyValue = null;
            try
            {
                DirectoryEntryHolder holder = ActiveDirectoryConnectionHelper.GetDirectoryEntry(this.directoryInfo, this.directoryInfo.ContainerDN, true);
                DirectoryEntry directoryEntry = holder.DirectoryEntry;
                SearchResultCollection results = null;
                try
                {
                    DirectorySearcher searcher = new DirectorySearcher(directoryEntry);
                    if (email != null)
                    {
                        searcher.Filter = "(&(objectCategory=person)(objectClass=user)(" + this.attributeMapUsername + "=*)(" + this.attributeMapEmail + "=" + this.GetEscapedFilterValue(email) + "))";
                    }
                    else
                    {
                        searcher.Filter = "(&(objectCategory=person)(objectClass=user)(" + this.attributeMapUsername + "=*)(!(" + this.attributeMapEmail + "=*)))";
                    }
                    searcher.SearchScope = System.DirectoryServices.SearchScope.Subtree;
                    searcher.PropertiesToLoad.Add(this.attributeMapUsername);
                    if (this.directoryInfo.ClientSearchTimeout != -1)
                    {
                        searcher.ClientTimeout = new TimeSpan(0, this.directoryInfo.ClientSearchTimeout, 0);
                    }
                    if (this.directoryInfo.ServerSearchTimeout != -1)
                    {
                        searcher.ServerPageTimeLimit = new TimeSpan(0, this.directoryInfo.ServerSearchTimeout, 0);
                    }
                    results = searcher.FindAll();
                    bool flag = false;
                    foreach (SearchResult result in results)
                    {
                        if (!flag)
                        {
                            searchResultPropertyValue = (string) PropertyManager.GetSearchResultPropertyValue(result, this.attributeMapUsername);
                            flag = true;
                            if (!this.RequiresUniqueEmail)
                            {
                                return searchResultPropertyValue;
                            }
                        }
                        else
                        {
                            if (this.RequiresUniqueEmail)
                            {
                                throw new ProviderException(System.Web.SR.GetString("Membership_more_than_one_user_with_email"));
                            }
                            return searchResultPropertyValue;
                        }
                    }
                    return searchResultPropertyValue;
                }
                finally
                {
                    if (results != null)
                    {
                        results.Dispose();
                    }
                    holder.Close();
                }
            }
            catch
            {
                throw;
            }
            return searchResultPropertyValue;
        }

        private Hashtable GetUserObjectAttributes()
        {
            DirectoryEntry entry = new DirectoryEntry(this.directoryInfo.GetADsPath("schema") + "/user", this.directoryInfo.GetUsername(), this.directoryInfo.GetPassword(), this.directoryInfo.AuthenticationTypes);
            object key = null;
            bool flag = false;
            Hashtable hashtable = new Hashtable(StringComparer.OrdinalIgnoreCase);
            try
            {
                key = entry.InvokeGet("MandatoryProperties");
            }
            catch (COMException exception)
            {
                if (exception.ErrorCode != -2147463155)
                {
                    throw;
                }
                flag = true;
            }
            if (!flag)
            {
                if (key is ICollection)
                {
                    foreach (string str in (ICollection) key)
                    {
                        if (!hashtable.Contains(str))
                        {
                            hashtable.Add(str, null);
                        }
                    }
                }
                else if (!hashtable.Contains(key))
                {
                    hashtable.Add(key, null);
                }
            }
            flag = false;
            try
            {
                key = entry.InvokeGet("OptionalProperties");
            }
            catch (COMException exception2)
            {
                if (exception2.ErrorCode != -2147463155)
                {
                    throw;
                }
                flag = true;
            }
            if (!flag)
            {
                if (key is ICollection)
                {
                    foreach (string str2 in (ICollection) key)
                    {
                        if (!hashtable.Contains(str2))
                        {
                            hashtable.Add(str2, null);
                        }
                    }
                    return hashtable;
                }
                if (!hashtable.Contains(key))
                {
                    hashtable.Add(key, null);
                }
            }
            return hashtable;
        }

        private string GetValidatedSchemaMapping(string valueName, string attributeName, out int maxLength)
        {
            if (string.Compare(valueName, "attributeMapUsername", StringComparison.Ordinal) == 0)
            {
                if (this.directoryInfo.DirectoryType != DirectoryType.AD)
                {
                    if (!System.Web.Util.StringUtil.EqualsIgnoreCase(attributeName, "userPrincipalName"))
                    {
                        throw new ProviderException(System.Web.SR.GetString("ADMembership_Username_mapping_invalid_ADAM"));
                    }
                }
                else if (!System.Web.Util.StringUtil.EqualsIgnoreCase(attributeName, "sAMAccountName") && !System.Web.Util.StringUtil.EqualsIgnoreCase(attributeName, "userPrincipalName"))
                {
                    throw new ProviderException(System.Web.SR.GetString("ADMembership_Username_mapping_invalid"));
                }
            }
            else
            {
                if (this.attributesInUse.Contains(attributeName))
                {
                    throw new ProviderException(System.Web.SR.GetString("ADMembership_mapping_not_unique", new object[] { valueName, attributeName }));
                }
                if (!this.userObjectAttributes.Contains(attributeName))
                {
                    throw new ProviderException(System.Web.SR.GetString("ADMembership_MappedAttribute_does_not_exist_on_user", new object[] { attributeName, valueName }));
                }
            }
            try
            {
                DirectoryEntry entry = new DirectoryEntry(this.directoryInfo.GetADsPath("schema") + "/" + attributeName, this.directoryInfo.GetUsername(), this.directoryInfo.GetPassword(), this.directoryInfo.AuthenticationTypes);
                string str = (string) entry.InvokeGet("Syntax");
                if (!System.Web.Util.StringUtil.EqualsIgnoreCase(str, (string) this.syntaxes[valueName]))
                {
                    throw new ProviderException(System.Web.SR.GetString("ADMembership_Wrong_syntax", new object[] { valueName, (string) this.syntaxes[valueName] }));
                }
                maxLength = -1;
                if (System.Web.Util.StringUtil.EqualsIgnoreCase(str, "DirectoryString"))
                {
                    try
                    {
                        maxLength = (int) entry.InvokeGet("MaxRange");
                    }
                    catch (TargetInvocationException exception)
                    {
                        if (!(exception.InnerException is COMException) || (((COMException) exception.InnerException).ErrorCode != -2147463155))
                        {
                            throw;
                        }
                    }
                }
                if ((string.Compare(valueName, "attributeMapUsername", StringComparison.Ordinal) != 0) && ((bool) entry.InvokeGet("MultiValued")))
                {
                    throw new ProviderException(System.Web.SR.GetString("ADMembership_attribute_not_single_valued", new object[] { valueName }));
                }
            }
            catch (COMException exception2)
            {
                if (exception2.ErrorCode == -2147463168)
                {
                    throw new ProviderException(System.Web.SR.GetString("ADMembership_MappedAttribute_does_not_exist", new object[] { attributeName, valueName }), exception2);
                }
                throw;
            }
            return attributeName;
        }

        [DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true), DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public override void Initialize(string name, NameValueCollection config)
        {
            if (HostingEnvironment.IsHosted)
            {
                HttpRuntime.CheckAspNetHostingPermission(AspNetHostingPermissionLevel.Low, "Feature_not_supported_at_this_level");
            }
            if (this.initialized)
            {
                return;
            }
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }
            if (string.IsNullOrEmpty(name))
            {
                name = "AspNetActiveDirectoryMembershipProvider";
            }
            if (string.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", System.Web.SR.GetString("ADMembership_Description"));
            }
            base.Initialize(name, config);
            this.appName = config["applicationName"];
            if (string.IsNullOrEmpty(this.appName))
            {
                this.appName = SecUtility.GetDefaultAppName();
            }
            if (this.appName.Length > 0x100)
            {
                throw new ProviderException(System.Web.SR.GetString("Provider_application_name_too_long"));
            }
            string str = config["connectionStringName"];
            if (string.IsNullOrEmpty(str))
            {
                throw new ProviderException(System.Web.SR.GetString("Connection_name_not_specified"));
            }
            this.adConnectionString = this.GetConnectionString(str, true);
            if (string.IsNullOrEmpty(this.adConnectionString))
            {
                throw new ProviderException(System.Web.SR.GetString("Connection_string_not_found", new object[] { str }));
            }
            string strA = config["connectionProtection"];
            if (strA == null)
            {
                strA = "Secure";
            }
            else if ((string.Compare(strA, "Secure", StringComparison.Ordinal) != 0) && (string.Compare(strA, "None", StringComparison.Ordinal) != 0))
            {
                throw new ProviderException(System.Web.SR.GetString("ADMembership_InvalidConnectionProtection", new object[] { strA }));
            }
            string userName = config["connectionUsername"];
            if ((userName != null) && (userName.Length == 0))
            {
                throw new ProviderException(System.Web.SR.GetString("ADMembership_Connection_username_must_not_be_empty"));
            }
            string password = config["connectionPassword"];
            if ((password != null) && (password.Length == 0))
            {
                throw new ProviderException(System.Web.SR.GetString("ADMembership_Connection_password_must_not_be_empty"));
            }
            if (((userName != null) && (password == null)) || ((password != null) && (userName == null)))
            {
                throw new ProviderException(System.Web.SR.GetString("ADMembership_Username_and_password_reqd"));
            }
            NetworkCredential credentials = new NetworkCredential(userName, password);
            int clientSearchTimeout = SecUtility.GetIntValue(config, "clientSearchTimeout", -1, false, 0);
            int serverSearchTimeout = SecUtility.GetIntValue(config, "serverSearchTimeout", -1, false, 0);
            this.enableSearchMethods = SecUtility.GetBooleanValue(config, "enableSearchMethods", false);
            this.requiresUniqueEmail = SecUtility.GetBooleanValue(config, "requiresUniqueEmail", false);
            this.enablePasswordReset = SecUtility.GetBooleanValue(config, "enablePasswordReset", false);
            this.requiresQuestionAndAnswer = SecUtility.GetBooleanValue(config, "requiresQuestionAndAnswer", false);
            this.minRequiredPasswordLength = SecUtility.GetIntValue(config, "minRequiredPasswordLength", 7, false, 0x80);
            this.minRequiredNonalphanumericCharacters = SecUtility.GetIntValue(config, "minRequiredNonalphanumericCharacters", 1, true, 0x80);
            this.passwordStrengthRegularExpression = config["passwordStrengthRegularExpression"];
            if (this.passwordStrengthRegularExpression != null)
            {
                this.passwordStrengthRegularExpression = this.passwordStrengthRegularExpression.Trim();
                if (this.passwordStrengthRegularExpression.Length == 0)
                {
                    goto Label_02DD;
                }
                try
                {
                    new Regex(this.passwordStrengthRegularExpression);
                    goto Label_02DD;
                }
                catch (ArgumentException exception)
                {
                    throw new ProviderException(exception.Message, exception);
                }
            }
            this.passwordStrengthRegularExpression = string.Empty;
        Label_02DD:
            if (this.minRequiredNonalphanumericCharacters > this.minRequiredPasswordLength)
            {
                throw new HttpException(System.Web.SR.GetString("MinRequiredNonalphanumericCharacters_can_not_be_more_than_MinRequiredPasswordLength"));
            }
            using (new ApplicationImpersonationContext())
            {
                int rangeUpperForSchemaAttribute;
                this.directoryInfo = new DirectoryInformation(this.adConnectionString, credentials, strA, clientSearchTimeout, serverSearchTimeout, this.enablePasswordReset);
                this.syntaxes.Add("attributeMapUsername", "DirectoryString");
                this.syntaxes.Add("attributeMapEmail", "DirectoryString");
                this.syntaxes.Add("attributeMapPasswordQuestion", "DirectoryString");
                this.syntaxes.Add("attributeMapPasswordAnswer", "DirectoryString");
                this.syntaxes.Add("attributeMapFailedPasswordAnswerCount", "Integer");
                this.syntaxes.Add("attributeMapFailedPasswordAnswerTime", "Integer8");
                this.syntaxes.Add("attributeMapFailedPasswordAnswerLockoutTime", "Integer8");
                this.attributesInUse.Add("objectclass", null);
                this.attributesInUse.Add("objectsid", null);
                this.attributesInUse.Add("comment", null);
                this.attributesInUse.Add("whencreated", null);
                this.attributesInUse.Add("pwdlastset", null);
                this.attributesInUse.Add("msds-user-account-control-computed", null);
                this.attributesInUse.Add("lockouttime", null);
                if (this.directoryInfo.DirectoryType == DirectoryType.AD)
                {
                    this.attributesInUse.Add("useraccountcontrol", null);
                }
                else
                {
                    this.attributesInUse.Add("msds-useraccountdisabled", null);
                }
                this.userObjectAttributes = this.GetUserObjectAttributes();
                string str5 = this.GetAttributeMapping(config, "attributeMapUsername", out rangeUpperForSchemaAttribute);
                if (str5 != null)
                {
                    this.attributeMapUsername = str5;
                    if (rangeUpperForSchemaAttribute != -1)
                    {
                        if (rangeUpperForSchemaAttribute < this.maxUsernameLength)
                        {
                            this.maxUsernameLength = rangeUpperForSchemaAttribute;
                        }
                        if (rangeUpperForSchemaAttribute < this.maxUsernameLengthForCreation)
                        {
                            this.maxUsernameLengthForCreation = rangeUpperForSchemaAttribute;
                        }
                    }
                }
                this.attributesInUse.Add(this.attributeMapUsername, null);
                if (System.Web.Util.StringUtil.EqualsIgnoreCase(this.attributeMapUsername, "sAMAccountName"))
                {
                    this.usernameIsSAMAccountName = true;
                    this.usernameIsUPN = false;
                }
                str5 = this.GetAttributeMapping(config, "attributeMapEmail", out rangeUpperForSchemaAttribute);
                if (str5 != null)
                {
                    this.attributeMapEmail = str5;
                    if ((rangeUpperForSchemaAttribute != -1) && (rangeUpperForSchemaAttribute < this.maxEmailLength))
                    {
                        this.maxEmailLength = rangeUpperForSchemaAttribute;
                    }
                }
                this.attributesInUse.Add(this.attributeMapEmail, null);
                rangeUpperForSchemaAttribute = this.GetRangeUpperForSchemaAttribute("comment");
                if ((rangeUpperForSchemaAttribute != -1) && (rangeUpperForSchemaAttribute < this.maxCommentLength))
                {
                    this.maxCommentLength = rangeUpperForSchemaAttribute;
                }
                if (this.enablePasswordReset)
                {
                    if (!this.requiresQuestionAndAnswer)
                    {
                        throw new ProviderException(System.Web.SR.GetString("ADMembership_PasswordReset_without_question_not_supported"));
                    }
                    this.maxInvalidPasswordAttempts = SecUtility.GetIntValue(config, "maxInvalidPasswordAttempts", 5, false, 0);
                    this.passwordAttemptWindow = SecUtility.GetIntValue(config, "passwordAttemptWindow", 10, false, 0);
                    this.passwordAnswerAttemptLockoutDuration = SecUtility.GetIntValue(config, "passwordAnswerAttemptLockoutDuration", 30, false, 0);
                    this.attributeMapFailedPasswordAnswerCount = this.GetAttributeMapping(config, "attributeMapFailedPasswordAnswerCount", out rangeUpperForSchemaAttribute);
                    if (this.attributeMapFailedPasswordAnswerCount != null)
                    {
                        this.attributesInUse.Add(this.attributeMapFailedPasswordAnswerCount, null);
                    }
                    this.attributeMapFailedPasswordAnswerTime = this.GetAttributeMapping(config, "attributeMapFailedPasswordAnswerTime", out rangeUpperForSchemaAttribute);
                    if (this.attributeMapFailedPasswordAnswerTime != null)
                    {
                        this.attributesInUse.Add(this.attributeMapFailedPasswordAnswerTime, null);
                    }
                    this.attributeMapFailedPasswordAnswerLockoutTime = this.GetAttributeMapping(config, "attributeMapFailedPasswordAnswerLockoutTime", out rangeUpperForSchemaAttribute);
                    if (this.attributeMapFailedPasswordAnswerLockoutTime != null)
                    {
                        this.attributesInUse.Add(this.attributeMapFailedPasswordAnswerLockoutTime, null);
                    }
                    if (((this.attributeMapFailedPasswordAnswerCount == null) || (this.attributeMapFailedPasswordAnswerTime == null)) || (this.attributeMapFailedPasswordAnswerLockoutTime == null))
                    {
                        throw new ProviderException(System.Web.SR.GetString("ADMembership_BadPasswordAnswerMappings_not_specified"));
                    }
                }
                this.attributeMapPasswordQuestion = this.GetAttributeMapping(config, "attributeMapPasswordQuestion", out rangeUpperForSchemaAttribute);
                if (this.attributeMapPasswordQuestion != null)
                {
                    if ((rangeUpperForSchemaAttribute != -1) && (rangeUpperForSchemaAttribute < this.maxPasswordQuestionLength))
                    {
                        this.maxPasswordQuestionLength = rangeUpperForSchemaAttribute;
                    }
                    this.attributesInUse.Add(this.attributeMapPasswordQuestion, null);
                }
                this.attributeMapPasswordAnswer = this.GetAttributeMapping(config, "attributeMapPasswordAnswer", out rangeUpperForSchemaAttribute);
                if (this.attributeMapPasswordAnswer != null)
                {
                    if ((rangeUpperForSchemaAttribute != -1) && (rangeUpperForSchemaAttribute < this.maxPasswordAnswerLength))
                    {
                        this.maxPasswordAnswerLength = rangeUpperForSchemaAttribute;
                    }
                    this.attributesInUse.Add(this.attributeMapPasswordAnswer, null);
                }
                if (this.requiresQuestionAndAnswer && ((this.attributeMapPasswordQuestion == null) || (this.attributeMapPasswordAnswer == null)))
                {
                    throw new ProviderException(System.Web.SR.GetString("ADMembership_PasswordQuestionAnswerMapping_not_specified"));
                }
                if (this.directoryInfo.DirectoryType == DirectoryType.ADAM)
                {
                    this.authTypeForValidation = AuthType.Basic;
                }
                else
                {
                    this.authTypeForValidation = this.directoryInfo.GetLdapAuthenticationTypes(this.directoryInfo.ConnectionProtection, CredentialsType.NonWindows);
                }
                if (this.directoryInfo.DirectoryType == DirectoryType.AD)
                {
                    if (this.enablePasswordReset)
                    {
                        this.directoryInfo.SelectServer();
                    }
                    this.directoryInfo.InitializeDomainAndForestName();
                }
            }
            this.connection = this.directoryInfo.CreateNewLdapConnection(this.authTypeForValidation);
            str = config["passwordCompatMode"];
            if (!string.IsNullOrEmpty(str))
            {
                this._LegacyPasswordCompatibilityMode = (MembershipPasswordCompatibilityMode) System.Enum.Parse(typeof(MembershipPasswordCompatibilityMode), str);
            }
            config.Remove("name");
            config.Remove("applicationName");
            config.Remove("connectionStringName");
            config.Remove("requiresUniqueEmail");
            config.Remove("enablePasswordReset");
            config.Remove("requiresQuestionAndAnswer");
            config.Remove("attributeMapPasswordQuestion");
            config.Remove("attributeMapPasswordAnswer");
            config.Remove("attributeMapUsername");
            config.Remove("attributeMapEmail");
            config.Remove("connectionProtection");
            config.Remove("connectionUsername");
            config.Remove("connectionPassword");
            config.Remove("clientSearchTimeout");
            config.Remove("serverSearchTimeout");
            config.Remove("enableSearchMethods");
            config.Remove("maxInvalidPasswordAttempts");
            config.Remove("passwordAttemptWindow");
            config.Remove("passwordAnswerAttemptLockoutDuration");
            config.Remove("attributeMapFailedPasswordAnswerCount");
            config.Remove("attributeMapFailedPasswordAnswerTime");
            config.Remove("attributeMapFailedPasswordAnswerLockoutTime");
            config.Remove("minRequiredPasswordLength");
            config.Remove("minRequiredNonalphanumericCharacters");
            config.Remove("passwordStrengthRegularExpression");
            config.Remove("passwordCompatMode");
            if (config.Count > 0)
            {
                string key = config.GetKey(0);
                if (!string.IsNullOrEmpty(key))
                {
                    throw new ProviderException(System.Web.SR.GetString("Provider_unrecognized_attribute", new object[] { key }));
                }
            }
            this.initialized = true;
        }

        private bool IsEmailUnique(DirectoryEntry containerEntry, string username, string email, bool existing)
        {
            bool flag2;
            bool flag = false;
            if (containerEntry == null)
            {
                containerEntry = new DirectoryEntry(this.directoryInfo.GetADsPath(this.directoryInfo.ContainerDN), this.directoryInfo.GetUsername(), this.directoryInfo.GetPassword(), this.directoryInfo.AuthenticationTypes);
                flag = true;
            }
            DirectorySearcher searcher = new DirectorySearcher(containerEntry);
            if (existing)
            {
                searcher.Filter = "(&(objectCategory=person)(objectClass=user)(" + this.attributeMapUsername + "=*)(" + this.attributeMapEmail + "=" + this.GetEscapedFilterValue(email) + ")(!(" + this.GetEscapedRdn("cn=" + this.GetEscapedFilterValue(username)) + ")))";
            }
            else
            {
                searcher.Filter = "(&(objectCategory=person)(objectClass=user)(" + this.attributeMapUsername + "=*)(" + this.attributeMapEmail + "=" + this.GetEscapedFilterValue(email) + "))";
            }
            searcher.SearchScope = System.DirectoryServices.SearchScope.Subtree;
            if (this.directoryInfo.ClientSearchTimeout != -1)
            {
                searcher.ClientTimeout = new TimeSpan(0, this.directoryInfo.ClientSearchTimeout, 0);
            }
            if (this.directoryInfo.ServerSearchTimeout != -1)
            {
                searcher.ServerPageTimeLimit = new TimeSpan(0, this.directoryInfo.ServerSearchTimeout, 0);
            }
            try
            {
                flag2 = searcher.FindOne() == null;
            }
            finally
            {
                if (flag)
                {
                    containerEntry.Dispose();
                    containerEntry = null;
                }
            }
            return flag2;
        }

        private bool IsUpnUnique(string username)
        {
            bool flag;
            DirectoryEntry searchRoot = new DirectoryEntry("GC://" + this.directoryInfo.ForestName, this.directoryInfo.GetUsername(), this.directoryInfo.GetPassword(), this.directoryInfo.AuthenticationTypes);
            DirectorySearcher searcher = new DirectorySearcher(searchRoot) {
                Filter = "(&(objectCategory=person)(objectClass=user)(userPrincipalName=" + this.GetEscapedFilterValue(username) + "))",
                SearchScope = System.DirectoryServices.SearchScope.Subtree
            };
            if (this.directoryInfo.ClientSearchTimeout != -1)
            {
                searcher.ClientTimeout = new TimeSpan(0, this.directoryInfo.ClientSearchTimeout, 0);
            }
            if (this.directoryInfo.ServerSearchTimeout != -1)
            {
                searcher.ServerPageTimeLimit = new TimeSpan(0, this.directoryInfo.ServerSearchTimeout, 0);
            }
            try
            {
                flag = searcher.FindOne() == null;
            }
            finally
            {
                searchRoot.Dispose();
            }
            return flag;
        }

        private void ResetBadPasswordAnswerAttributes(DirectoryEntry userEntry)
        {
            userEntry.Properties[this.attributeMapFailedPasswordAnswerCount].Value = 0;
            userEntry.Properties[this.attributeMapFailedPasswordAnswerTime].Value = 0;
            userEntry.Properties[this.attributeMapFailedPasswordAnswerLockoutTime].Value = 0;
            userEntry.CommitChanges();
        }

        [DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true), DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public override string ResetPassword(string username, string passwordAnswer)
        {
            string password = null;
            if (!this.initialized)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("ADMembership_Provider_not_initialized"));
            }
            if (!this.EnablePasswordReset)
            {
                throw new NotSupportedException(System.Web.SR.GetString("Not_configured_to_support_password_resets"));
            }
            this.CheckUserName(ref username, this.maxUsernameLength, "username");
            this.CheckPasswordAnswer(ref passwordAnswer, this.RequiresQuestionAndAnswer, this.maxPasswordAnswerLength, "passwordAnswer");
            try
            {
                DirectoryEntryHolder holder = ActiveDirectoryConnectionHelper.GetDirectoryEntry(this.directoryInfo, this.directoryInfo.ContainerDN, true);
                DirectoryEntry directoryEntry = holder.DirectoryEntry;
                DirectoryEntry userEntry = null;
                bool resetBadPasswordAnswerAttributes = false;
                try
                {
                    try
                    {
                        MembershipUser user = this.FindUser(directoryEntry, "(" + this.attributeMapUsername + "=" + this.GetEscapedFilterValue(username) + ")", out userEntry, out resetBadPasswordAnswerAttributes);
                        if (user == null)
                        {
                            throw new ProviderException(System.Web.SR.GetString("Membership_UserNotFound"));
                        }
                        if (user.IsLockedOut)
                        {
                            throw new MembershipPasswordException(System.Web.SR.GetString("Membership_AccountLockOut"));
                        }
                        string str2 = this.Decrypt((string) PropertyManager.GetPropertyValue(userEntry, this.attributeMapPasswordAnswer));
                        if (!System.Web.Util.StringUtil.EqualsIgnoreCase(passwordAnswer, str2))
                        {
                            this.UpdateBadPasswordAnswerAttributes(userEntry);
                            throw new MembershipPasswordException(System.Web.SR.GetString("Membership_WrongAnswer"));
                        }
                        if (resetBadPasswordAnswerAttributes)
                        {
                            this.ResetBadPasswordAnswerAttributes(userEntry);
                        }
                        this.SetPasswordPortIfApplicable(userEntry);
                        password = this.GeneratePassword();
                        ValidatePasswordEventArgs e = new ValidatePasswordEventArgs(username, password, false);
                        this.OnValidatingPassword(e);
                        if (e.Cancel)
                        {
                            if (e.FailureInformation != null)
                            {
                                throw e.FailureInformation;
                            }
                            throw new ProviderException(System.Web.SR.GetString("Membership_Custom_Password_Validation_Failure"));
                        }
                        userEntry.Invoke("SetPassword", new object[] { password });
                    }
                    catch (TargetInvocationException exception)
                    {
                        if (!(exception.InnerException is COMException))
                        {
                            throw;
                        }
                        COMException innerException = (COMException) exception.InnerException;
                        int errorCode = innerException.ErrorCode;
                        switch (errorCode)
                        {
                            case -2147022651:
                            case -2147016657:
                            case -2147023571:
                            case -2147023569:
                                throw new ProviderException(System.Web.SR.GetString("ADMembership_Generated_password_not_complex"), innerException);
                        }
                        if ((errorCode == -2147463155) && (this.directoryInfo.DirectoryType == DirectoryType.ADAM))
                        {
                            throw new ProviderException(System.Web.SR.GetString("ADMembership_No_secure_conn_for_password"));
                        }
                        throw;
                    }
                    return password;
                }
                finally
                {
                    if (userEntry != null)
                    {
                        userEntry.Dispose();
                    }
                    holder.Close();
                }
            }
            catch
            {
                throw;
            }
            return password;
        }

        private void SetPasswordPortIfApplicable(DirectoryEntry userEntry)
        {
            if (this.directoryInfo.DirectoryType == DirectoryType.ADAM)
            {
                try
                {
                    if ((this.directoryInfo.ConnectionProtection == ActiveDirectoryConnectionProtection.Ssl) && this.directoryInfo.PortSpecified)
                    {
                        userEntry.Options.PasswordPort = this.directoryInfo.Port;
                        userEntry.Options.PasswordEncoding = PasswordEncodingMethod.PasswordEncodingSsl;
                    }
                    else if ((this.directoryInfo.ConnectionProtection == ActiveDirectoryConnectionProtection.SignAndSeal) || (this.directoryInfo.ConnectionProtection == ActiveDirectoryConnectionProtection.None))
                    {
                        userEntry.Options.PasswordPort = this.directoryInfo.Port;
                        userEntry.Options.PasswordEncoding = PasswordEncodingMethod.PasswordEncodingClear;
                    }
                }
                catch (COMException exception)
                {
                    if (exception.ErrorCode != -2147463160)
                    {
                        throw;
                    }
                    if ((this.directoryInfo.Port != 0x27c) || (this.directoryInfo.ConnectionProtection != ActiveDirectoryConnectionProtection.Ssl))
                    {
                        throw new ProviderException(System.Web.SR.GetString("ADMembership_unable_to_set_password_port"));
                    }
                }
            }
        }

        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public override bool UnlockUser(string username)
        {
            if (!this.initialized)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("ADMembership_Provider_not_initialized"));
            }
            this.CheckUserName(ref username, this.maxUsernameLength, "username");
            try
            {
                DirectoryEntryHolder holder = ActiveDirectoryConnectionHelper.GetDirectoryEntry(this.directoryInfo, this.directoryInfo.ContainerDN, true);
                DirectoryEntry directoryEntry = holder.DirectoryEntry;
                DirectoryEntry entry2 = null;
                try
                {
                    entry2 = this.FindUserEntry(directoryEntry, "(" + this.attributeMapUsername + "=" + this.GetEscapedFilterValue(username) + ")");
                    if (entry2 == null)
                    {
                        return false;
                    }
                    entry2.Properties["lockoutTime"].Value = 0;
                    if (this.EnablePasswordReset)
                    {
                        entry2.Properties[this.attributeMapFailedPasswordAnswerCount].Value = 0;
                        entry2.Properties[this.attributeMapFailedPasswordAnswerTime].Value = 0;
                        entry2.Properties[this.attributeMapFailedPasswordAnswerLockoutTime].Value = 0;
                    }
                    entry2.CommitChanges();
                }
                finally
                {
                    if (entry2 != null)
                    {
                        entry2.Dispose();
                    }
                    holder.Close();
                }
            }
            catch
            {
                throw;
            }
            return true;
        }

        private void UpdateBadPasswordAnswerAttributes(DirectoryEntry userEntry)
        {
            int num = 0;
            bool flag = false;
            DateTime utcNow = DateTime.UtcNow;
            if (userEntry.Properties.Contains(this.attributeMapFailedPasswordAnswerTime))
            {
                DateTime dateTimeFromLargeInteger = this.GetDateTimeFromLargeInteger((NativeComInterfaces.IAdsLargeInteger) PropertyManager.GetPropertyValue(userEntry, this.attributeMapFailedPasswordAnswerTime));
                flag = utcNow.Subtract(dateTimeFromLargeInteger) <= new TimeSpan(0, this.PasswordAttemptWindow, 0);
            }
            int propertyValue = 0;
            if (userEntry.Properties.Contains(this.attributeMapFailedPasswordAnswerCount))
            {
                propertyValue = (int) PropertyManager.GetPropertyValue(userEntry, this.attributeMapFailedPasswordAnswerCount);
            }
            if (flag && (propertyValue > 0))
            {
                num = propertyValue + 1;
            }
            else
            {
                num = 1;
            }
            userEntry.Properties[this.attributeMapFailedPasswordAnswerCount].Value = num;
            userEntry.Properties[this.attributeMapFailedPasswordAnswerTime].Value = this.GetLargeIntegerFromDateTime(utcNow);
            if (num >= this.maxInvalidPasswordAttempts)
            {
                userEntry.Properties[this.attributeMapFailedPasswordAnswerLockoutTime].Value = this.GetLargeIntegerFromDateTime(utcNow);
            }
            userEntry.CommitChanges();
        }

        [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
        public override void UpdateUser(MembershipUser user)
        {
            bool emailModified = true;
            bool commentModified = true;
            bool isApprovedModified = true;
            if (!this.initialized)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("ADMembership_Provider_not_initialized"));
            }
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            ActiveDirectoryMembershipUser user2 = user as ActiveDirectoryMembershipUser;
            if (user2 != null)
            {
                emailModified = user2.emailModified;
                commentModified = user2.commentModified;
                isApprovedModified = user2.isApprovedModified;
            }
            string userName = user.UserName;
            this.CheckUserName(ref userName, this.maxUsernameLength, "UserName");
            string email = user.Email;
            if (emailModified)
            {
                SecUtility.CheckParameter(ref email, this.RequiresUniqueEmail, true, false, this.maxEmailLength, "Email");
            }
            if (commentModified && (user.Comment != null))
            {
                if (user.Comment.Length == 0)
                {
                    throw new ArgumentException(System.Web.SR.GetString("Parameter_can_not_be_empty", new object[] { "Comment" }), "Comment");
                }
                if ((this.maxCommentLength > 0) && (user.Comment.Length > this.maxCommentLength))
                {
                    throw new ArgumentException(System.Web.SR.GetString("Parameter_too_long", new object[] { "Comment", this.maxCommentLength.ToString(CultureInfo.InvariantCulture) }), "Comment");
                }
            }
            try
            {
                DirectoryEntryHolder holder = ActiveDirectoryConnectionHelper.GetDirectoryEntry(this.directoryInfo, this.directoryInfo.ContainerDN, true);
                DirectoryEntry directoryEntry = holder.DirectoryEntry;
                DirectoryEntry entry2 = null;
                try
                {
                    entry2 = this.FindUserEntry(directoryEntry, "(" + this.attributeMapUsername + "=" + this.GetEscapedFilterValue(user.UserName) + ")");
                    if (entry2 == null)
                    {
                        throw new ProviderException(System.Web.SR.GetString("Membership_UserNotFound"));
                    }
                    if ((emailModified || commentModified) || isApprovedModified)
                    {
                        if (emailModified)
                        {
                            if (email == null)
                            {
                                if (entry2.Properties.Contains(this.attributeMapEmail))
                                {
                                    entry2.Properties[this.attributeMapEmail].Clear();
                                }
                            }
                            else
                            {
                                if (this.RequiresUniqueEmail && !this.IsEmailUnique(null, user.UserName, email, true))
                                {
                                    throw new ProviderException(System.Web.SR.GetString("Membership_DuplicateEmail"));
                                }
                                entry2.Properties[this.attributeMapEmail].Value = email;
                            }
                        }
                        if (commentModified)
                        {
                            if (user.Comment == null)
                            {
                                if (entry2.Properties.Contains("comment"))
                                {
                                    entry2.Properties["comment"].Clear();
                                }
                            }
                            else
                            {
                                entry2.Properties["comment"].Value = user.Comment;
                            }
                        }
                        if (isApprovedModified)
                        {
                            if (this.directoryInfo.DirectoryType == DirectoryType.AD)
                            {
                                int propertyValue = (int) PropertyManager.GetPropertyValue(entry2, "userAccountControl");
                                if (user.IsApproved)
                                {
                                    propertyValue &= -3;
                                }
                                else
                                {
                                    propertyValue |= 2;
                                }
                                entry2.Properties["userAccountControl"].Value = propertyValue;
                            }
                            else
                            {
                                entry2.Properties["msDS-UserAccountDisabled"].Value = !user.IsApproved;
                            }
                        }
                        entry2.CommitChanges();
                        if (user2 != null)
                        {
                            user2.emailModified = false;
                            user2.commentModified = false;
                            user2.isApprovedModified = false;
                        }
                    }
                }
                finally
                {
                    if (entry2 != null)
                    {
                        entry2.Dispose();
                    }
                    holder.Close();
                }
            }
            catch
            {
                throw;
            }
        }

        private bool ValidateCredentials(string username, string password)
        {
            bool flag = false;
            NetworkCredential newCredential = this.usernameIsSAMAccountName ? new NetworkCredential(username, password, this.directoryInfo.DomainName) : DirectoryInformation.GetCredentialsWithDomain(new NetworkCredential(username, password));
            if (this.directoryInfo.ConcurrentBindSupported)
            {
                try
                {
                    this.connection.Bind(newCredential);
                    return true;
                }
                catch (LdapException exception)
                {
                    if (exception.ErrorCode != 0x31)
                    {
                        throw;
                    }
                    return false;
                }
            }
            LdapConnection connection = this.directoryInfo.CreateNewLdapConnection(this.authTypeForValidation);
            try
            {
                connection.Bind(newCredential);
                flag = true;
            }
            catch (LdapException exception2)
            {
                if (exception2.ErrorCode != 0x31)
                {
                    throw;
                }
                return false;
            }
            finally
            {
                connection.Dispose();
            }
            return flag;
        }

        private bool ValidatePassword(string password, int maxSize)
        {
            if (password == null)
            {
                return false;
            }
            if (password.Trim().Length < 1)
            {
                return false;
            }
            if ((maxSize > 0) && (password.Length > maxSize))
            {
                return false;
            }
            return true;
        }

        [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public override bool ValidateUser(string username, string password)
        {
            if (this.ValidateUserCore(username, password))
            {
                PerfCounters.IncrementCounter(AppPerfCounter.MEMBER_SUCCESS);
                WebBaseEvent.RaiseSystemEvent(null, 0xfa2, username);
                return true;
            }
            PerfCounters.IncrementCounter(AppPerfCounter.MEMBER_FAIL);
            WebBaseEvent.RaiseSystemEvent(null, 0xfa6, username);
            return false;
        }

        [DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true), DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        private bool ValidateUserCore(string username, string password)
        {
            if (!this.initialized)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("ADMembership_Provider_not_initialized"));
            }
            if (!SecUtility.ValidateParameter(ref username, true, true, true, this.maxUsernameLength))
            {
                return false;
            }
            if (this.usernameIsUPN && (username.IndexOf('\\') != -1))
            {
                return false;
            }
            if (!this.ValidatePassword(password, this.maxPasswordLength))
            {
                return false;
            }
            bool flag = false;
            try
            {
                DirectoryEntryHolder holder = ActiveDirectoryConnectionHelper.GetDirectoryEntry(this.directoryInfo, this.directoryInfo.ContainerDN, true);
                DirectoryEntry directoryEntry = holder.DirectoryEntry;
                DirectoryEntry userEntry = null;
                bool resetBadPasswordAnswerAttributes = false;
                string str = null;
                try
                {
                    if (this.EnablePasswordReset)
                    {
                        MembershipUser user = null;
                        if (((this.directoryInfo.DirectoryType == DirectoryType.AD) && this.usernameIsUPN) && (username.IndexOf('@') == -1))
                        {
                            string sAMAccountName = null;
                            user = this.FindUserAndSAMAccountName(directoryEntry, "(" + this.attributeMapUsername + "=" + this.GetEscapedFilterValue(username) + ")", out userEntry, out resetBadPasswordAnswerAttributes, out sAMAccountName);
                            str = this.directoryInfo.DomainName + @"\" + sAMAccountName;
                        }
                        else
                        {
                            user = this.FindUser(directoryEntry, "(" + this.attributeMapUsername + "=" + this.GetEscapedFilterValue(username) + ")", out userEntry, out resetBadPasswordAnswerAttributes);
                            str = username;
                        }
                        if ((user == null) || user.IsLockedOut)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (((this.directoryInfo.DirectoryType == DirectoryType.AD) && this.usernameIsUPN) && (username.IndexOf('@') == -1))
                        {
                            string str3 = null;
                            userEntry = this.FindUserEntryAndSAMAccountName(directoryEntry, "(" + this.attributeMapUsername + "=" + this.GetEscapedFilterValue(username) + ")", out str3);
                            str = this.directoryInfo.DomainName + @"\" + str3;
                        }
                        else
                        {
                            userEntry = this.FindUserEntry(directoryEntry, "(" + this.attributeMapUsername + "=" + this.GetEscapedFilterValue(username) + ")");
                            str = username;
                        }
                        if (userEntry == null)
                        {
                            return false;
                        }
                    }
                    flag = this.ValidateCredentials(str, password);
                    if ((this.EnablePasswordReset && flag) && resetBadPasswordAnswerAttributes)
                    {
                        this.ResetBadPasswordAnswerAttributes(userEntry);
                    }
                }
                finally
                {
                    if (userEntry != null)
                    {
                        userEntry.Dispose();
                    }
                    holder.Close();
                }
            }
            catch
            {
                throw;
            }
            return flag;
        }

        public override string ApplicationName
        {
            get
            {
                if (!this.initialized)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("ADMembership_Provider_not_initialized"));
                }
                return this.appName;
            }
            set
            {
                throw new NotSupportedException(System.Web.SR.GetString("ADMembership_Setting_ApplicationName_not_supported"));
            }
        }

        public ActiveDirectoryConnectionProtection CurrentConnectionProtection
        {
            get
            {
                if (!this.initialized)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("ADMembership_Provider_not_initialized"));
                }
                return this.directoryInfo.ConnectionProtection;
            }
        }

        public override bool EnablePasswordReset
        {
            get
            {
                if (!this.initialized)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("ADMembership_Provider_not_initialized"));
                }
                return this.enablePasswordReset;
            }
        }

        public override bool EnablePasswordRetrieval
        {
            get
            {
                if (!this.initialized)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("ADMembership_Provider_not_initialized"));
                }
                return this.enablePasswordRetrieval;
            }
        }

        public bool EnableSearchMethods
        {
            get
            {
                if (!this.initialized)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("ADMembership_Provider_not_initialized"));
                }
                return this.enableSearchMethods;
            }
        }

        public override int MaxInvalidPasswordAttempts
        {
            get
            {
                if (!this.initialized)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("ADMembership_Provider_not_initialized"));
                }
                return this.maxInvalidPasswordAttempts;
            }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get
            {
                if (!this.initialized)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("ADMembership_Provider_not_initialized"));
                }
                return this.minRequiredNonalphanumericCharacters;
            }
        }

        public override int MinRequiredPasswordLength
        {
            get
            {
                if (!this.initialized)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("ADMembership_Provider_not_initialized"));
                }
                return this.minRequiredPasswordLength;
            }
        }

        public int PasswordAnswerAttemptLockoutDuration
        {
            get
            {
                if (!this.initialized)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("ADMembership_Provider_not_initialized"));
                }
                return this.passwordAnswerAttemptLockoutDuration;
            }
        }

        public override int PasswordAttemptWindow
        {
            get
            {
                if (!this.initialized)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("ADMembership_Provider_not_initialized"));
                }
                return this.passwordAttemptWindow;
            }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get
            {
                return MembershipPasswordFormat.Hashed;
            }
        }

        public override string PasswordStrengthRegularExpression
        {
            get
            {
                if (!this.initialized)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("ADMembership_Provider_not_initialized"));
                }
                return this.passwordStrengthRegularExpression;
            }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get
            {
                if (!this.initialized)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("ADMembership_Provider_not_initialized"));
                }
                return this.requiresQuestionAndAnswer;
            }
        }

        public override bool RequiresUniqueEmail
        {
            get
            {
                if (!this.initialized)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("ADMembership_Provider_not_initialized"));
                }
                return this.requiresUniqueEmail;
            }
        }
    }
}

