namespace System.Web.Hosting
{
    using System;
    using System.Web.Configuration;

    [Serializable]
    internal class ISAPIConfigMapPathFactory : IConfigMapPathFactory
    {
        IConfigMapPath IConfigMapPathFactory.Create(string virtualPath, string physicalPath)
        {
            return IISMapPath.GetInstance();
        }
    }
}

