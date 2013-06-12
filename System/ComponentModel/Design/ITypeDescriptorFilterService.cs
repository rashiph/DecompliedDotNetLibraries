namespace System.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;

    public interface ITypeDescriptorFilterService
    {
        bool FilterAttributes(IComponent component, IDictionary attributes);
        bool FilterEvents(IComponent component, IDictionary events);
        bool FilterProperties(IComponent component, IDictionary properties);
    }
}

