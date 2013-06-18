namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Globalization;
    using System.Resources;

    internal class TypeSystemSR
    {
        private static TypeSystemSR loader;
        private ResourceManager resources = new ResourceManager("System.Workflow.ComponentModel.Compiler.StringResources", Assembly.GetExecutingAssembly());

        internal TypeSystemSR()
        {
        }

        private static TypeSystemSR GetLoader()
        {
            if (loader == null)
            {
                loader = new TypeSystemSR();
            }
            return loader;
        }

        internal static string GetString(string name)
        {
            return GetString(Culture, name);
        }

        internal static string GetString(CultureInfo culture, string name)
        {
            TypeSystemSR loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetString(name, culture);
        }

        internal static string GetString(string name, params object[] args)
        {
            return GetString(Culture, name, args);
        }

        internal static string GetString(CultureInfo culture, string name, params object[] args)
        {
            TypeSystemSR loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            string format = loader.resources.GetString(name, culture);
            if ((args != null) && (args.Length > 0))
            {
                return string.Format(CultureInfo.CurrentCulture, format, args);
            }
            return format;
        }

        private static CultureInfo Culture
        {
            get
            {
                return null;
            }
        }
    }
}

