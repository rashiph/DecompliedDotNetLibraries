namespace System.Runtime.Remoting.Proxies
{
    using System;

    [Serializable]
    internal enum CallType
    {
        InvalidCall,
        MethodCall,
        ConstructorCall
    }
}

