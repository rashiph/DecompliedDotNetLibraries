namespace System.ComponentModel
{
    using System;

    public interface ICustomTypeDescriptor
    {
        AttributeCollection GetAttributes();
        string GetClassName();
        string GetComponentName();
        TypeConverter GetConverter();
        EventDescriptor GetDefaultEvent();
        PropertyDescriptor GetDefaultProperty();
        object GetEditor(Type editorBaseType);
        EventDescriptorCollection GetEvents();
        EventDescriptorCollection GetEvents(Attribute[] attributes);
        PropertyDescriptorCollection GetProperties();
        PropertyDescriptorCollection GetProperties(Attribute[] attributes);
        object GetPropertyOwner(PropertyDescriptor pd);
    }
}

