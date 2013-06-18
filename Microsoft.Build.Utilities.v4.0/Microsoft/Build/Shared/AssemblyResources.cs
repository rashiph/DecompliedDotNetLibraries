namespace Microsoft.Build.Shared
{
    using System;
    using System.Globalization;
    using System.Resources;
    using System.Runtime;

    internal static class AssemblyResources
    {
        private static readonly ResourceManager resources = new ResourceManager("Microsoft.Build.Utilities.Strings", Assembly.GetExecutingAssembly());
        private static readonly ResourceManager sharedResources = new ResourceManager("Microsoft.Build.Utilities.Strings.shared", Assembly.GetExecutingAssembly());

        internal static string FormatResourceString(string resourceName, params object[] args)
        {
            ErrorUtilities.VerifyThrowArgumentNull(resourceName, "resourceName");
            return FormatString(GetString(resourceName), args);
        }

        internal static string FormatString(string unformatted, params object[] args)
        {
            ErrorUtilities.VerifyThrowArgumentNull(unformatted, "unformatted");
            return ResourceUtilities.FormatString(unformatted, args);
        }

        internal static string GetString(string name)
        {
            string str = resources.GetString(name, CultureInfo.CurrentUICulture);
            if (str == null)
            {
                str = sharedResources.GetString(name, CultureInfo.CurrentUICulture);
            }
            ErrorUtilities.VerifyThrow(str != null, "Missing resource '{0}'", name);
            return str;
        }

        internal static ResourceManager PrimaryResources
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return resources;
            }
        }

        internal static ResourceManager SharedResources
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return sharedResources;
            }
        }
    }
}

