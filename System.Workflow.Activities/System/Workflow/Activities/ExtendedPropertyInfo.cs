namespace System.Workflow.Activities
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;

    internal sealed class ExtendedPropertyInfo : PropertyInfo
    {
        private GetValueHandler OnGetValue;
        private PropertyInfo realPropertyInfo;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal ExtendedPropertyInfo(PropertyInfo propertyInfo, GetValueHandler getValueHandler)
        {
            this.realPropertyInfo = propertyInfo;
            this.OnGetValue = getValueHandler;
        }

        public override MethodInfo[] GetAccessors(bool nonPublic)
        {
            return this.realPropertyInfo.GetAccessors(nonPublic);
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return this.realPropertyInfo.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return this.realPropertyInfo.GetCustomAttributes(attributeType, inherit);
        }

        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            return this.realPropertyInfo.GetGetMethod(nonPublic);
        }

        public override ParameterInfo[] GetIndexParameters()
        {
            return this.realPropertyInfo.GetIndexParameters();
        }

        public override MethodInfo GetSetMethod(bool nonPublic)
        {
            return this.realPropertyInfo.GetSetMethod(nonPublic);
        }

        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            if (this.OnGetValue != null)
            {
                return this.OnGetValue(this, obj);
            }
            return null;
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return this.realPropertyInfo.IsDefined(attributeType, inherit);
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            this.realPropertyInfo.SetValue(obj, value, invokeAttr, binder, index, culture);
        }

        public override PropertyAttributes Attributes
        {
            get
            {
                return this.realPropertyInfo.Attributes;
            }
        }

        public override bool CanRead
        {
            get
            {
                return this.realPropertyInfo.CanRead;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return this.realPropertyInfo.CanWrite;
            }
        }

        public override Type DeclaringType
        {
            get
            {
                return this.realPropertyInfo.DeclaringType;
            }
        }

        public override string Name
        {
            get
            {
                return this.realPropertyInfo.Name;
            }
        }

        public override Type PropertyType
        {
            get
            {
                return this.realPropertyInfo.PropertyType;
            }
        }

        public override Type ReflectedType
        {
            get
            {
                return this.realPropertyInfo.ReflectedType;
            }
        }
    }
}

