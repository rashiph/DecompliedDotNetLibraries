namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    internal sealed class ActivityBindNamePropertyDescriptor : DynamicPropertyDescriptor
    {
        private ITypeDescriptorContext context;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ActivityBindNamePropertyDescriptor(ITypeDescriptorContext context, PropertyDescriptor realPropertyDescriptor) : base(context, realPropertyDescriptor)
        {
            this.context = context;
        }

        public override object GetValue(object component)
        {
            object obj2 = base.GetValue(component);
            string str = obj2 as string;
            if (!string.IsNullOrEmpty(str))
            {
                Activity context = PropertyDescriptorUtils.GetComponent(this.context) as Activity;
                context = (context != null) ? Helpers.ParseActivityForBind(context, str) : null;
                obj2 = (context != null) ? context.QualifiedName : str;
            }
            return obj2;
        }

        public override void SetValue(object component, object value)
        {
            string str = value as string;
            if (string.IsNullOrEmpty(str))
            {
                throw new InvalidOperationException(SR.GetString("Error_ActivityIdentifierCanNotBeEmpty"));
            }
            Activity context = PropertyDescriptorUtils.GetComponent(this.context) as Activity;
            if ((context != null) && (Helpers.ParseActivityForBind(context, str) == null))
            {
                throw new InvalidOperationException(SR.GetString("Error_InvalidActivityIdentifier", new object[] { str }));
            }
            base.SetValue(component, value);
        }
    }
}

