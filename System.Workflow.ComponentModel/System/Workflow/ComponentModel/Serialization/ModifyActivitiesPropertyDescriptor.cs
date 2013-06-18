namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.ComponentModel;
    using System.Reflection;
    using System.Workflow.ComponentModel;

    internal class ModifyActivitiesPropertyDescriptor : PropertyDescriptor
    {
        private PropertyInfo propInfo;

        public ModifyActivitiesPropertyDescriptor(PropertyInfo propInfo) : base("CanModifyActivities", new Attribute[0])
        {
            this.propInfo = propInfo;
        }

        public override bool CanResetValue(object component)
        {
            throw new NotImplementedException();
        }

        public override object GetValue(object component)
        {
            return this.propInfo.GetValue(component, null);
        }

        public override void ResetValue(object component)
        {
            throw new NotImplementedException();
        }

        public override void SetValue(object component, object value)
        {
            this.propInfo.SetValue(component, true, null);
            if (component is CompositeActivity)
            {
                (component as CompositeActivity).SetValue(Activity.CustomActivityProperty, false);
            }
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }

        public override Type ComponentType
        {
            get
            {
                return typeof(CompositeActivity);
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public override Type PropertyType
        {
            get
            {
                return typeof(bool);
            }
        }
    }
}

