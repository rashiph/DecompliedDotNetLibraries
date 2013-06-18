namespace Microsoft.JScript
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;

    internal class JSWrappedProperty : PropertyInfo, IWrappedMember
    {
        internal object obj;
        internal PropertyInfo property;

        internal JSWrappedProperty(PropertyInfo property, object obj)
        {
            this.obj = obj;
            this.property = property;
            if (obj is JSObject)
            {
                Type declaringType = property.DeclaringType;
                if (((declaringType == Typeob.Object) || (declaringType == Typeob.String)) || (declaringType.IsPrimitive || (declaringType == Typeob.Array)))
                {
                    if (obj is BooleanObject)
                    {
                        this.obj = ((BooleanObject) obj).value;
                    }
                    else if (obj is NumberObject)
                    {
                        this.obj = ((NumberObject) obj).value;
                    }
                    else if (obj is StringObject)
                    {
                        this.obj = ((StringObject) obj).value;
                    }
                    else if (obj is ArrayWrapper)
                    {
                        this.obj = ((ArrayWrapper) obj).value;
                    }
                }
            }
        }

        public override MethodInfo[] GetAccessors(bool nonPublic)
        {
            return this.property.GetAccessors(nonPublic);
        }

        internal virtual string GetClassFullName()
        {
            if (this.property is JSProperty)
            {
                return ((JSProperty) this.property).GetClassFullName();
            }
            return this.property.DeclaringType.FullName;
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
            MethodInfo getMethod = JSProperty.GetGetMethod(this.property, nonPublic);
            if (getMethod == null)
            {
                return null;
            }
            return new JSWrappedMethod(getMethod, this.obj);
        }

        public override ParameterInfo[] GetIndexParameters()
        {
            return this.property.GetIndexParameters();
        }

        public override MethodInfo GetSetMethod(bool nonPublic)
        {
            MethodInfo setMethod = JSProperty.GetSetMethod(this.property, nonPublic);
            if (setMethod == null)
            {
                return null;
            }
            return new JSWrappedMethod(setMethod, this.obj);
        }

        [DebuggerHidden, DebuggerStepThrough]
        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            return this.property.GetValue(this.obj, invokeAttr, binder, index, culture);
        }

        public object GetWrappedObject()
        {
            return this.obj;
        }

        public override bool IsDefined(Type type, bool inherit)
        {
            return Microsoft.JScript.CustomAttribute.IsDefined(this.property, type, inherit);
        }

        [DebuggerStepThrough, DebuggerHidden]
        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            this.property.SetValue(this.obj, value, invokeAttr, binder, index, culture);
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
                return this.property.DeclaringType;
            }
        }

        public override MemberTypes MemberType
        {
            get
            {
                return MemberTypes.Property;
            }
        }

        public override string Name
        {
            get
            {
                if ((this.obj is LenientGlobalObject) && this.property.Name.StartsWith("Slow", StringComparison.Ordinal))
                {
                    return this.property.Name.Substring(4);
                }
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

