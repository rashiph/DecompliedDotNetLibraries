namespace System.ComponentModel.Design
{
    using System;
    using System.Collections;

    public interface IComponentDiscoveryService
    {
        ICollection GetComponentTypes(IDesignerHost designerHost, Type baseType);
    }
}

