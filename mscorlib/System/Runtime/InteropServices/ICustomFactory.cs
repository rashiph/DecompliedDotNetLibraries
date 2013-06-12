namespace System.Runtime.InteropServices
{
    using System;

    [ComVisible(true)]
    public interface ICustomFactory
    {
        MarshalByRefObject CreateInstance(Type serverType);
    }
}

