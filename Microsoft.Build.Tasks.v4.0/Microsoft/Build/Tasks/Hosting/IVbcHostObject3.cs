namespace Microsoft.Build.Tasks.Hosting
{
    using Microsoft.Build.Framework;
    using System;
    using System.Runtime.InteropServices;

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComVisible(true), Guid("1186fe8f-8aba-48d6-8ce3-32ca42f53728")]
    public interface IVbcHostObject3 : IVbcHostObject2, IVbcHostObject, ITaskHost
    {
        bool SetLanguageVersion(string languageVersion);
    }
}

