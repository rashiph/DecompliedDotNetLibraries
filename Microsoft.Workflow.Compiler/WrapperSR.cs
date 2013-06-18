using System;
using System.Globalization;
using System.Reflection;
using System.Resources;

internal static class WrapperSR
{
    internal const string InvalidArgumentsToMain = "InvalidArgumentsToMain";
    internal const string InvalidAssemblyHeader = "InvalidAssemblyHeader";
    private static ResourceManager resources;

    internal static string GetString(string name)
    {
        return GetString(Culture, name);
    }

    internal static string GetString(CultureInfo culture, string name)
    {
        return Resources.GetString(name, culture);
    }

    internal static string GetString(string name, params object[] args)
    {
        return GetString(Culture, name, args);
    }

    internal static string GetString(CultureInfo culture, string name, params object[] args)
    {
        string format = Resources.GetString(name, culture);
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

    private static ResourceManager Resources
    {
        get
        {
            if (resources == null)
            {
                resources = new ResourceManager("Microsoft.Workflow.Compiler.StringResources", Assembly.GetExecutingAssembly());
            }
            return resources;
        }
    }
}

