namespace System.ComponentModel.Design
{
    using System;
    using System.Collections;

    public interface ITypeDiscoveryService
    {
        ICollection GetTypes(Type baseType, bool excludeGlobalTypes);
    }
}

