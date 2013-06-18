namespace Microsoft.Build.Tasks.Hosting
{
    using Microsoft.Build.Framework;
    using System;
    using System.Runtime.InteropServices;

    [Guid("F9353662-F1ED-4a23-A323-5F5047E85F5D"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComVisible(true)]
    public interface ICscHostObject3 : ICscHostObject2, ICscHostObject, ITaskHost
    {
        bool SetApplicationConfiguration(string applicationConfiguration);
    }
}

