namespace System.EnterpriseServices.Internal
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("391ffbb9-a8ee-432a-abc8-baa238dab90f")]
    internal interface IAssemblyLocator
    {
        string[] GetModules(string applicationDir, string applicationName, string assemblyName);
    }
}

