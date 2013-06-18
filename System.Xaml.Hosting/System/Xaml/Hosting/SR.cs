namespace System.Xaml.Hosting
{
    using System;
    using System.Globalization;
    using System.Resources;
    using System.Runtime;

    internal class SR
    {
        private static CultureInfo resourceCulture;
        private static System.Resources.ResourceManager resourceManager;

        private SR()
        {
        }

        internal static string CouldNotResolveType(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("CouldNotResolveType", Culture), new object[] { param0 });
        }

        internal static string HttpHandlerForXamlTypeNotFound(object param0, object param1, object param2)
        {
            return string.Format(Culture, ResourceManager.GetString("HttpHandlerForXamlTypeNotFound", Culture), new object[] { param0, param1, param2 });
        }

        internal static string NotHttpHandlerType(object param0, object param1, object param2)
        {
            return string.Format(Culture, ResourceManager.GetString("NotHttpHandlerType", Culture), new object[] { param0, param1, param2 });
        }

        internal static string ConfigSectionNotFound
        {
            get
            {
                return ResourceManager.GetString("ConfigSectionNotFound", Culture);
            }
        }

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

        internal static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceManager, null))
                {
                    System.Resources.ResourceManager manager = new System.Resources.ResourceManager("System.Xaml.Hosting.SR", typeof(System.Xaml.Hosting.SR).Assembly);
                    resourceManager = manager;
                }
                return resourceManager;
            }
        }

        internal static string ResourceNotFound
        {
            get
            {
                return ResourceManager.GetString("ResourceNotFound", Culture);
            }
        }

        internal static string UnexpectedEof
        {
            get
            {
                return ResourceManager.GetString("UnexpectedEof", Culture);
            }
        }
    }
}

