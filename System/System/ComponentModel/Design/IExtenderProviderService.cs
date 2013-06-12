namespace System.ComponentModel.Design
{
    using System;
    using System.ComponentModel;

    public interface IExtenderProviderService
    {
        void AddExtenderProvider(IExtenderProvider provider);
        void RemoveExtenderProvider(IExtenderProvider provider);
    }
}

