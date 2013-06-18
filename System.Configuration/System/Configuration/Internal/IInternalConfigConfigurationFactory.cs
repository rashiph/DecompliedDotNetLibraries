namespace System.Configuration.Internal
{
    using System;
    using System.Configuration;
    using System.Runtime.InteropServices;

    [ComVisible(false)]
    public interface IInternalConfigConfigurationFactory
    {
        System.Configuration.Configuration Create(Type typeConfigHost, params object[] hostInitConfigurationParams);
        string NormalizeLocationSubPath(string subPath, IConfigErrorInfo errorInfo);
    }
}

