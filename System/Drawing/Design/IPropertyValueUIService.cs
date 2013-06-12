namespace System.Drawing.Design
{
    using System;
    using System.ComponentModel;

    public interface IPropertyValueUIService
    {
        event EventHandler PropertyUIValueItemsChanged;

        void AddPropertyValueUIHandler(PropertyValueUIHandler newHandler);
        PropertyValueUIItem[] GetPropertyUIValueItems(ITypeDescriptorContext context, PropertyDescriptor propDesc);
        void NotifyPropertyValueUIItemsChanged();
        void RemovePropertyValueUIHandler(PropertyValueUIHandler newHandler);
    }
}

