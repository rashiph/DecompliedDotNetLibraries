namespace Microsoft.JScript
{
    using System;
    using System.Globalization;
    using System.Reflection;

    internal class JSPropertyInfo : PropertyInfo
    {
        private Type declaringType;
        internal MethodInfo getter;
        private PropertyInfo property;
        internal MethodInfo setter;

        internal JSPropertyInfo(PropertyInfo property)
        {
            this.property = property;
        }

        public override MethodInfo[] GetAccessors(bool nonPublic)
        {
            throw new JScriptException(JSError.InternalError);
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return this.property.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type t, bool inherit)
        {
            return Microsoft.JScript.CustomAttribute.GetCustomAttributes(this.property, t, inherit);
        }

        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            MethodInfo getter = this.getter;
            if (getter == null)
            {
                getter = this.property.GetGetMethod(nonPublic);
                if (getter != null)
                {
                    getter = new JSMethodInfo(getter);
                }
                this.getter = getter;
            }
            return getter;
        }

        public override ParameterInfo[] GetIndexParameters()
        {
            MethodInfo getMethod = this.GetGetMethod(false);
            if (getMethod != null)
            {
                return getMethod.GetParameters();
            }
            return this.property.GetIndexParameters();
        }

        public override MethodInfo GetSetMethod(bool nonPublic)
        {
            MethodInfo setter = this.setter;
            if (setter == null)
            {
                setter = this.property.GetSetMethod(nonPublic);
                if (setter != null)
                {
                    setter = new JSMethodInfo(setter);
                }
                this.setter = setter;
            }
            return setter;
        }

        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            return this.GetGetMethod(false).Invoke(obj, invokeAttr, binder, (index == null) ? new object[0] : index, culture);
        }

        public override bool IsDefined(Type type, bool inherit)
        {
            return Microsoft.JScript.CustomAttribute.IsDefined(this.property, type, inherit);
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            if ((index == null) || (index.Length == 0))
            {
                this.GetSetMethod(false).Invoke(obj, invokeAttr, binder, new object[] { value }, culture);
            }
            else
            {
                int length = index.Length;
                object[] target = new object[length + 1];
                ArrayObject.Copy(index, 0, target, 0, length);
                target[length] = value;
                this.GetSetMethod(false).Invoke(obj, invokeAttr, binder, target, culture);
            }
        }

        public override PropertyAttributes Attributes
        {
            get
            {
                return this.property.Attributes;
            }
        }

        public override bool CanRead
        {
            get
            {
                return this.property.CanRead;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return this.property.CanWrite;
            }
        }

        public override Type DeclaringType
        {
            get
            {
                Type declaringType = this.declaringType;
                if (declaringType == null)
                {
                    this.declaringType = declaringType = this.property.DeclaringType;
                }
                return declaringType;
            }
        }

        public override string Name
        {
            get
            {
                return this.property.Name;
            }
        }

        public override Type PropertyType
        {
            get
            {
                return this.property.PropertyType;
            }
        }

        public override Type ReflectedType
        {
            get
            {
                return this.property.ReflectedType;
            }
        }
    }
}

