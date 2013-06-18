namespace Microsoft.Build.Shared
{
    using System;
    using System.Globalization;
    using System.Resources;

    internal static class AssemblyResources
    {
        private static readonly ResourceManager resources = new ResourceManager("Microsoft.Build.Tasks.Strings", Assembly.GetExecutingAssembly());
        private static readonly ResourceManager sharedResources = new ResourceManager("Microsoft.Build.Tasks.Strings.shared", Assembly.GetExecutingAssembly());

        internal static string GetString(string name)
        {
            string str = resources.GetString(name, CultureInfo.CurrentUICulture);
            if (str == null)
            {
                str = sharedResources.GetString(name, CultureInfo.CurrentUICulture);
            }
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(str != null, "Missing resource '{0}'", name);
            return str;
        }

        internal static ResourceManager PrimaryResources
        {
            get
            {
                return resources;
            }
        }

        internal static ResourceManager SharedResources
        {
            get
            {
                return sharedResources;
            }
        }
    }
}

