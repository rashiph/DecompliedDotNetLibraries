namespace Microsoft.Build.Tasks
{
    using System;
    using System.Globalization;
    using System.Resources;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class SR
    {
        internal const string ClassComments1 = "ClassComments1";
        internal const string ClassComments3 = "ClassComments3";
        internal const string ClassDocComment = "ClassDocComment";
        internal const string CulturePropertyComment1 = "CulturePropertyComment1";
        internal const string CulturePropertyComment2 = "CulturePropertyComment2";
        internal const string InvalidIdentifier = "InvalidIdentifier";
        private static Microsoft.Build.Tasks.SR loader;
        internal const string MismatchedResourceName = "MismatchedResourceName";
        internal const string ResMgrPropertyComment = "ResMgrPropertyComment";
        private ResourceManager resources;
        internal const string StringPropertyComment = "StringPropertyComment";
        internal const string StringPropertyTruncatedComment = "StringPropertyTruncatedComment";

        internal SR()
        {
            this.resources = new ResourceManager("System.Design", base.GetType().Assembly);
        }

        private static Microsoft.Build.Tasks.SR GetLoader()
        {
            if (loader == null)
            {
                Microsoft.Build.Tasks.SR sr = new Microsoft.Build.Tasks.SR();
                Interlocked.CompareExchange<Microsoft.Build.Tasks.SR>(ref loader, sr, null);
            }
            return loader;
        }

        public static object GetObject(string name)
        {
            Microsoft.Build.Tasks.SR loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetObject(name, Culture);
        }

        public static string GetString(string name)
        {
            Microsoft.Build.Tasks.SR loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetString(name, Culture);
        }

        public static string GetString(string name, params object[] args)
        {
            Microsoft.Build.Tasks.SR loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            string format = loader.resources.GetString(name, Culture);
            if ((args == null) || (args.Length <= 0))
            {
                return format;
            }
            for (int i = 0; i < args.Length; i++)
            {
                string str2 = args[i] as string;
                if ((str2 != null) && (str2.Length > 0x400))
                {
                    args[i] = str2.Substring(0, 0x3fd) + "...";
                }
            }
            return string.Format(CultureInfo.CurrentCulture, format, args);
        }

        public static string GetString(string name, out bool usedFallback)
        {
            usedFallback = false;
            return GetString(name);
        }

        private static CultureInfo Culture
        {
            get
            {
                return null;
            }
        }

        public static ResourceManager Resources
        {
            get
            {
                return GetLoader().resources;
            }
        }
    }
}

