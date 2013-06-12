namespace System.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public interface IEventBindingService
    {
        string CreateUniqueMethodName(IComponent component, EventDescriptor e);
        ICollection GetCompatibleMethods(EventDescriptor e);
        EventDescriptor GetEvent(PropertyDescriptor property);
        PropertyDescriptorCollection GetEventProperties(EventDescriptorCollection events);
        PropertyDescriptor GetEventProperty(EventDescriptor e);
        bool ShowCode();
        bool ShowCode(int lineNumber);
        bool ShowCode(IComponent component, EventDescriptor e);
    }
}

