namespace System.ComponentModel
{
    using System;

    public interface ITypedList
    {
        PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors);
        string GetListName(PropertyDescriptor[] listAccessors);
    }
}

