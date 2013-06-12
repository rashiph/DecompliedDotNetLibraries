namespace System.Web.Compilation
{
    using System;
    using System.Globalization;
    using System.Resources;

    public interface IResourceProvider
    {
        object GetObject(string resourceKey, CultureInfo culture);

        IResourceReader ResourceReader { get; }
    }
}

