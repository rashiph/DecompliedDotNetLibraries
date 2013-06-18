namespace System.Workflow.ComponentModel
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;

    internal class ActivityBindPropertyInfo : PropertyInfo
    {
        private Type declaringType;
        private MethodInfo getMethod;
        private PropertyInfo originalPropertyInfo;
        private string propertyName;
        private MethodInfo setMethod;

        public ActivityBindPropertyInfo(Type declaringType, MethodInfo getMethod, MethodInfo setMethod, string propertyName, PropertyInfo originalPropertyInfo)
        {
            if (declaringType == null)
            {
                throw new ArgumentNullException("declaringType");
            }
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }
            this.declaringType = declaringType;
            this.getMethod = getMethod;
            this.setMethod = setMethod;
            this.propertyName = propertyName;
            this.originalPropertyInfo = originalPropertyInfo;
        }

        public override MethodInfo[] GetAccessors(bool nonPublic)
        {
            return new MethodInfo[] { this.getMethod, this.setMethod };
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return new object[0];
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return new object[0];
        }

        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            return this.getMethod;
        }

        public override ParameterInfo[] GetIndexParameters()
        {
            if (this.getMethod != null)
            {
                return this.getMethod.GetParameters();
            }
            if (this.originalPropertyInfo != null)
            {
                return this.originalPropertyInfo.GetIndexParameters();
            }
            return new ParameterInfo[0];
        }

        public override MethodInfo GetSetMethod(bool nonPublic)
        {
            return this.setMethod;
        }

        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            if ((this.getMethod == null) && ((this.originalPropertyInfo == null) || !this.originalPropertyInfo.CanRead))
            {
                throw new InvalidOperationException(SR.GetString("Error_PropertyHasNoGetterDefined", new object[] { this.propertyName }));
            }
            if (this.getMethod != null)
            {
                return this.getMethod.Invoke(obj, invokeAttr, binder, index, culture);
            }
            return this.originalPropertyInfo.GetValue(obj, invokeAttr, binder, index, culture);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotSupportedException();
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            if ((this.setMethod == null) && ((this.originalPropertyInfo == null) || !this.originalPropertyInfo.CanWrite))
            {
                throw new InvalidOperationException(SR.GetString("Error_PropertyHasNoSetterDefined", new object[] { this.propertyName }));
            }
            if (this.setMethod != null)
            {
                object[] array = new object[((index != null) ? index.Length : 0) + 1];
                array[(index != null) ? index.Length : 0] = value;
                if (index != null)
                {
                    index.CopyTo(array, 0);
                }
                this.setMethod.Invoke(obj, invokeAttr, binder, array, culture);
            }
            else
            {
                this.originalPropertyInfo.SetValue(obj, value, invokeAttr, binder, index, culture);
            }
        }

        public override PropertyAttributes Attributes
        {
            get
            {
                return PropertyAttributes.None;
            }
        }

        public override bool CanRead
        {
            get
            {
                return ((this.getMethod != null) || ((this.originalPropertyInfo != null) && this.originalPropertyInfo.CanRead));
            }
        }

        public override bool CanWrite
        {
            get
            {
                return ((this.setMethod != null) || ((this.originalPropertyInfo != null) && this.originalPropertyInfo.CanWrite));
            }
        }

        public override Type DeclaringType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.declaringType;
            }
        }

        public override string Name
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.propertyName;
            }
        }

        public override Type PropertyType
        {
            get
            {
                if (this.getMethod != null)
                {
                    return this.getMethod.ReturnType;
                }
                if (this.originalPropertyInfo != null)
                {
                    return this.originalPropertyInfo.PropertyType;
                }
                return typeof(object);
            }
        }

        public override Type ReflectedType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.declaringType;
            }
        }
    }
}

