namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Reflection;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    public sealed class PropertyValidationContext
    {
        private object propertyInfo;
        private string propertyName;
        private object propertyOwner;

        public PropertyValidationContext(object propertyOwner, DependencyProperty dependencyProperty)
        {
            this.propertyName = string.Empty;
            if (propertyOwner == null)
            {
                throw new ArgumentNullException("propertyOwner");
            }
            this.propertyOwner = propertyOwner;
            this.propertyInfo = dependencyProperty;
        }

        public PropertyValidationContext(object propertyOwner, PropertyInfo propertyInfo, string propertyName)
        {
            this.propertyName = string.Empty;
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }
            if (propertyOwner == null)
            {
                throw new ArgumentNullException("propertyOwner");
            }
            this.propertyOwner = propertyOwner;
            this.propertyName = propertyName;
            this.propertyInfo = propertyInfo;
        }

        public object Property
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.propertyInfo;
            }
        }

        public string PropertyName
        {
            get
            {
                if (this.propertyInfo is DependencyProperty)
                {
                    return ((DependencyProperty) this.propertyInfo).Name;
                }
                return this.propertyName;
            }
        }

        public object PropertyOwner
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.propertyOwner;
            }
        }
    }
}

