namespace System.Web
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Resources;
    using System.Runtime;
    using System.Runtime.CompilerServices;

    [CompilerGenerated, GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0"), DebuggerNonUserCode]
    internal class ApplicationServicesStrings
    {
        private static CultureInfo resourceCulture;
        private static System.Resources.ResourceManager resourceMan;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal ApplicationServicesStrings()
        {
        }

        internal static string Can_not_use_encrypted_passwords_with_autogen_keys
        {
            get
            {
                return ResourceManager.GetString("Can_not_use_encrypted_passwords_with_autogen_keys", resourceCulture);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static CultureInfo Culture
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return resourceCulture;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                resourceCulture = value;
            }
        }

        internal static string Membership_DuplicateEmail
        {
            get
            {
                return ResourceManager.GetString("Membership_DuplicateEmail", resourceCulture);
            }
        }

        internal static string Membership_DuplicateProviderUserKey
        {
            get
            {
                return ResourceManager.GetString("Membership_DuplicateProviderUserKey", resourceCulture);
            }
        }

        internal static string Membership_DuplicateUserName
        {
            get
            {
                return ResourceManager.GetString("Membership_DuplicateUserName", resourceCulture);
            }
        }

        internal static string Membership_InvalidAnswer
        {
            get
            {
                return ResourceManager.GetString("Membership_InvalidAnswer", resourceCulture);
            }
        }

        internal static string Membership_InvalidEmail
        {
            get
            {
                return ResourceManager.GetString("Membership_InvalidEmail", resourceCulture);
            }
        }

        internal static string Membership_InvalidPassword
        {
            get
            {
                return ResourceManager.GetString("Membership_InvalidPassword", resourceCulture);
            }
        }

        internal static string Membership_InvalidProviderUserKey
        {
            get
            {
                return ResourceManager.GetString("Membership_InvalidProviderUserKey", resourceCulture);
            }
        }

        internal static string Membership_InvalidQuestion
        {
            get
            {
                return ResourceManager.GetString("Membership_InvalidQuestion", resourceCulture);
            }
        }

        internal static string Membership_InvalidUserName
        {
            get
            {
                return ResourceManager.GetString("Membership_InvalidUserName", resourceCulture);
            }
        }

        internal static string Membership_no_error
        {
            get
            {
                return ResourceManager.GetString("Membership_no_error", resourceCulture);
            }
        }

        internal static string Membership_provider_name_invalid
        {
            get
            {
                return ResourceManager.GetString("Membership_provider_name_invalid", resourceCulture);
            }
        }

        internal static string Membership_UserRejected
        {
            get
            {
                return ResourceManager.GetString("Membership_UserRejected", resourceCulture);
            }
        }

        internal static string Parameter_can_not_be_empty
        {
            get
            {
                return ResourceManager.GetString("Parameter_can_not_be_empty", resourceCulture);
            }
        }

        internal static string Platform_not_supported
        {
            get
            {
                return ResourceManager.GetString("Platform_not_supported", resourceCulture);
            }
        }

        internal static string Provider_Error
        {
            get
            {
                return ResourceManager.GetString("Provider_Error", resourceCulture);
            }
        }

        internal static string Provider_must_implement_type
        {
            get
            {
                return ResourceManager.GetString("Provider_must_implement_type", resourceCulture);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceMan, null))
                {
                    System.Resources.ResourceManager manager = new System.Resources.ResourceManager("System.Web.ApplicationServicesStrings", typeof(ApplicationServicesStrings).Assembly);
                    resourceMan = manager;
                }
                return resourceMan;
            }
        }
    }
}

