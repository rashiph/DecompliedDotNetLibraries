namespace System.Web.Compilation
{
    using System;
    using System.Collections;
    using System.Globalization;

    public interface IImplicitResourceProvider
    {
        ICollection GetImplicitResourceKeys(string keyPrefix);
        object GetObject(ImplicitResourceKey key, CultureInfo culture);
    }
}

