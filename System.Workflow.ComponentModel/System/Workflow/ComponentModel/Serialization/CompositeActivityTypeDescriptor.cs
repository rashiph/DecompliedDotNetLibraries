namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    internal class CompositeActivityTypeDescriptor : CustomTypeDescriptor
    {
        private ICustomTypeDescriptor realTypeDescriptor;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CompositeActivityTypeDescriptor(ICustomTypeDescriptor realTypeDescriptor) : base(realTypeDescriptor)
        {
            this.realTypeDescriptor = realTypeDescriptor;
        }

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            PropertyDescriptorCollection properties = base.GetProperties(attributes);
            if (((attributes == null) || (attributes.Length != 1)) || (!(attributes[0] is DesignOnlyAttribute) || (attributes[0] as DesignOnlyAttribute).IsDesignOnly))
            {
                return properties;
            }
            ArrayList list = new ArrayList();
            foreach (PropertyDescriptor descriptor in properties)
            {
                list.Add(descriptor);
            }
            PropertyInfo property = typeof(CompositeActivity).GetProperty("CanModifyActivities", BindingFlags.NonPublic | BindingFlags.Instance);
            list.Add(new ModifyActivitiesPropertyDescriptor(property));
            return new PropertyDescriptorCollection((PropertyDescriptor[]) list.ToArray(typeof(PropertyDescriptor)));
        }
    }
}

