namespace System.Workflow.Runtime
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    internal static class HashHelper
    {
        internal static Guid HashServiceType(string serviceFullTypeName)
        {
            MD5 md = new MD5CryptoServiceProvider();
            byte[] bytes = new UnicodeEncoding().GetBytes(serviceFullTypeName);
            return new Guid(md.ComputeHash(bytes));
        }

        internal static Guid HashServiceType(Type serviceType)
        {
            return HashServiceType(serviceType.AssemblyQualifiedName);
        }
    }
}

