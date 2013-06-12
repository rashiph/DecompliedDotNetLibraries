namespace System.Reflection
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public class AssemblyNameProxy : MarshalByRefObject
    {
        [SecuritySafeCritical]
        public AssemblyName GetAssemblyName(string assemblyFile)
        {
            return AssemblyName.GetAssemblyName(assemblyFile);
        }
    }
}

