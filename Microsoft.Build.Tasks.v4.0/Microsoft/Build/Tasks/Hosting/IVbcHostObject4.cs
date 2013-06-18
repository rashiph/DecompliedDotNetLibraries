namespace Microsoft.Build.Tasks.Hosting
{
    using Microsoft.Build.Framework;
    using System;
    using System.Runtime.InteropServices;

    [Guid("2AE3233C-8AB3-48A0-9ED9-6E3545B3C566"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComVisible(true)]
    public interface IVbcHostObject4 : IVbcHostObject3, IVbcHostObject2, IVbcHostObject, ITaskHost
    {
        bool SetVBRuntime(string VBRuntime);
    }
}

