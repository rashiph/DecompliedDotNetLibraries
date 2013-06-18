namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Runtime;

    internal sealed class ReadonlyTypeDescriptor : CustomTypeDescriptor
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal ReadonlyTypeDescriptor(ICustomTypeDescriptor realTypeDescriptor) : base(realTypeDescriptor)
        {
        }

        public override AttributeCollection GetAttributes()
        {
            ArrayList list = new ArrayList();
            foreach (Attribute attribute in base.GetAttributes())
            {
                if (!(attribute is EditorAttribute) && !(attribute is ReadOnlyAttribute))
                {
                    list.Add(attribute);
                }
            }
            list.Add(new ReadOnlyAttribute(true));
            return new AttributeCollection((Attribute[]) list.ToArray(typeof(Attribute)));
        }

        public override EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            EventDescriptorCollection events = base.GetEvents(attributes);
            ArrayList list = new ArrayList();
            foreach (EventDescriptor descriptor in events)
            {
                BrowsableAttribute attribute = descriptor.Attributes[typeof(BrowsableAttribute)] as BrowsableAttribute;
                if ((attribute != null) && attribute.Browsable)
                {
                    list.Add(new ReadonlyEventDescriptor(descriptor));
                }
                else
                {
                    list.Add(descriptor);
                }
            }
            return new EventDescriptorCollection((EventDescriptor[]) list.ToArray(typeof(EventDescriptor)));
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            PropertyDescriptorCollection properties = base.GetProperties();
            ArrayList list = new ArrayList();
            foreach (PropertyDescriptor descriptor in properties)
            {
                BrowsableAttribute attribute = descriptor.Attributes[typeof(BrowsableAttribute)] as BrowsableAttribute;
                if (((attribute != null) && attribute.Browsable) && !(descriptor is ReadonlyPropertyDescriptor))
                {
                    list.Add(new ReadonlyPropertyDescriptor(descriptor));
                }
                else
                {
                    list.Add(descriptor);
                }
            }
            return new PropertyDescriptorCollection((PropertyDescriptor[]) list.ToArray(typeof(PropertyDescriptor)));
        }
    }
}

