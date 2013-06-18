namespace Microsoft.JScript
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Reflection.Emit;

    internal class JSProperty : PropertyInfo
    {
        private ParameterInfo[] formal_parameters;
        internal JSMethod getter;
        internal PropertyBuilder metaData;
        private string name;
        internal JSMethod setter;

        internal JSProperty(string name)
        {
            this.name = name;
            this.formal_parameters = null;
            this.getter = null;
            this.setter = null;
        }

        public override MethodInfo[] GetAccessors(bool nonPublic)
        {
            if ((this.getter != null) && (nonPublic || this.getter.IsPublic))
            {
                if ((this.setter != null) && (nonPublic || this.setter.IsPublic))
                {
                    return new MethodInfo[] { this.getter, this.setter };
                }
                return new MethodInfo[] { this.getter };
            }
            if ((this.setter == null) || (!nonPublic && !this.setter.IsPublic))
            {
                return new MethodInfo[0];
            }
            return new MethodInfo[] { this.setter };
        }

        internal virtual string GetClassFullName()
        {
            if (this.getter != null)
            {
                return this.getter.GetClassFullName();
            }
            return this.setter.GetClassFullName();
        }

        public sealed override object[] GetCustomAttributes(bool inherit)
        {
            if (this.getter != null)
            {
                return this.getter.GetCustomAttributes(true);
            }
            if (this.setter != null)
            {
                return this.setter.GetCustomAttributes(true);
            }
            return new object[0];
        }

        public sealed override object[] GetCustomAttributes(Type t, bool inherit)
        {
            return new object[0];
        }

        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            if (this.getter == null)
            {
                try
                {
                    IReflect superType = ((ClassScope) this.setter.obj).GetSuperType();
                    BindingFlags @public = BindingFlags.Public;
                    if (this.setter.IsStatic)
                    {
                        @public |= BindingFlags.FlattenHierarchy | BindingFlags.Static;
                    }
                    else
                    {
                        @public |= BindingFlags.Instance;
                    }
                    if (nonPublic)
                    {
                        @public |= BindingFlags.NonPublic;
                    }
                    PropertyInfo prop = superType.GetProperty(this.name, @public, null, null, new Type[0], null);
                    if (prop is JSProperty)
                    {
                        return prop.GetGetMethod(nonPublic);
                    }
                    return GetGetMethod(prop, nonPublic);
                }
                catch (AmbiguousMatchException)
                {
                }
            }
            if (!nonPublic && !this.getter.IsPublic)
            {
                return null;
            }
            return this.getter;
        }

        internal static MethodInfo GetGetMethod(PropertyInfo prop, bool nonPublic)
        {
            if (prop != null)
            {
                JSProperty property = prop as JSProperty;
                if (property != null)
                {
                    return property.GetGetMethod(nonPublic);
                }
                MethodInfo getMethod = prop.GetGetMethod(nonPublic);
                if (getMethod != null)
                {
                    return getMethod;
                }
                Type declaringType = prop.DeclaringType;
                if (declaringType == null)
                {
                    return null;
                }
                Type baseType = declaringType.BaseType;
                if (baseType == null)
                {
                    return null;
                }
                getMethod = prop.GetGetMethod(nonPublic);
                if (getMethod == null)
                {
                    return null;
                }
                BindingFlags @public = BindingFlags.Public;
                if (getMethod.IsStatic)
                {
                    @public |= BindingFlags.FlattenHierarchy | BindingFlags.Static;
                }
                else
                {
                    @public |= BindingFlags.Instance;
                }
                if (nonPublic)
                {
                    @public |= BindingFlags.NonPublic;
                }
                string name = prop.Name;
                prop = null;
                try
                {
                    prop = baseType.GetProperty(name, @public, null, null, new Type[0], null);
                }
                catch (AmbiguousMatchException)
                {
                }
                if (prop != null)
                {
                    return GetGetMethod(prop, nonPublic);
                }
            }
            return null;
        }

        public override ParameterInfo[] GetIndexParameters()
        {
            if (this.formal_parameters == null)
            {
                if (this.getter != null)
                {
                    this.formal_parameters = this.getter.GetParameters();
                }
                else
                {
                    ParameterInfo[] parameters = this.setter.GetParameters();
                    int length = parameters.Length;
                    if (length <= 1)
                    {
                        length = 1;
                    }
                    this.formal_parameters = new ParameterInfo[length - 1];
                    for (int i = 0; i < (length - 1); i++)
                    {
                        this.formal_parameters[i] = parameters[i];
                    }
                }
            }
            return this.formal_parameters;
        }

        public override MethodInfo GetSetMethod(bool nonPublic)
        {
            if (this.setter == null)
            {
                try
                {
                    IReflect superType = ((ClassScope) this.getter.obj).GetSuperType();
                    BindingFlags @public = BindingFlags.Public;
                    if (this.getter.IsStatic)
                    {
                        @public |= BindingFlags.FlattenHierarchy | BindingFlags.Static;
                    }
                    else
                    {
                        @public |= BindingFlags.Instance;
                    }
                    if (nonPublic)
                    {
                        @public |= BindingFlags.NonPublic;
                    }
                    PropertyInfo prop = superType.GetProperty(this.name, @public, null, null, new Type[0], null);
                    if (prop is JSProperty)
                    {
                        return prop.GetSetMethod(nonPublic);
                    }
                    return GetSetMethod(prop, nonPublic);
                }
                catch (AmbiguousMatchException)
                {
                }
            }
            if (!nonPublic && !this.setter.IsPublic)
            {
                return null;
            }
            return this.setter;
        }

        internal static MethodInfo GetSetMethod(PropertyInfo prop, bool nonPublic)
        {
            if (prop != null)
            {
                JSProperty property = prop as JSProperty;
                if (property != null)
                {
                    return property.GetSetMethod(nonPublic);
                }
                MethodInfo setMethod = prop.GetSetMethod(nonPublic);
                if (setMethod != null)
                {
                    return setMethod;
                }
                Type declaringType = prop.DeclaringType;
                if (declaringType == null)
                {
                    return null;
                }
                Type baseType = declaringType.BaseType;
                if (baseType == null)
                {
                    return null;
                }
                setMethod = prop.GetGetMethod(nonPublic);
                if (setMethod == null)
                {
                    return null;
                }
                BindingFlags @public = BindingFlags.Public;
                if (setMethod.IsStatic)
                {
                    @public |= BindingFlags.FlattenHierarchy | BindingFlags.Static;
                }
                else
                {
                    @public |= BindingFlags.Instance;
                }
                if (nonPublic)
                {
                    @public |= BindingFlags.NonPublic;
                }
                string name = prop.Name;
                prop = null;
                try
                {
                    prop = baseType.GetProperty(name, @public, null, null, new Type[0], null);
                }
                catch (AmbiguousMatchException)
                {
                }
                if (prop != null)
                {
                    return GetSetMethod(prop, nonPublic);
                }
            }
            return null;
        }

        internal bool GetterAndSetterAreConsistent()
        {
            if ((this.getter == null) || (this.setter == null))
            {
                return true;
            }
            ((JSFieldMethod) this.getter).func.PartiallyEvaluate();
            ((JSFieldMethod) this.setter).func.PartiallyEvaluate();
            ParameterInfo[] parameters = this.getter.GetParameters();
            ParameterInfo[] infoArray2 = this.setter.GetParameters();
            int length = parameters.Length;
            int num2 = infoArray2.Length;
            if (length != (num2 - 1))
            {
                return false;
            }
            if (!((JSFieldMethod) this.getter).func.ReturnType(null).Equals(((ParameterDeclaration) infoArray2[length]).type.ToIReflect()))
            {
                return false;
            }
            for (int i = 0; i < length; i++)
            {
                if (((ParameterDeclaration) parameters[i]).type.ToIReflect() != ((ParameterDeclaration) infoArray2[i]).type.ToIReflect())
                {
                    return false;
                }
            }
            return ((this.getter.Attributes & ~MethodAttributes.Abstract) == (this.setter.Attributes & ~MethodAttributes.Abstract));
        }

        [DebuggerStepThrough, DebuggerHidden]
        internal static object GetValue(PropertyInfo prop, object obj, object[] index)
        {
            JSProperty property = prop as JSProperty;
            if (property != null)
            {
                return property.GetValue(obj, BindingFlags.ExactBinding, null, index, null);
            }
            JSWrappedProperty property2 = prop as JSWrappedProperty;
            if (property2 != null)
            {
                return property2.GetValue(obj, BindingFlags.ExactBinding, null, index, null);
            }
            MethodInfo getMethod = GetGetMethod(prop, false);
            if (getMethod != null)
            {
                try
                {
                    return getMethod.Invoke(obj, BindingFlags.ExactBinding, null, index, null);
                }
                catch (TargetInvocationException exception)
                {
                    throw exception.InnerException;
                }
            }
            throw new MissingMethodException();
        }

        [DebuggerHidden, DebuggerStepThrough]
        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            MethodInfo getter = this.getter;
            JSObject obj2 = obj as JSObject;
            if ((getter == null) && (obj2 != null))
            {
                getter = obj2.GetMethod("get_" + this.name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                JSWrappedMethod method = getter as JSWrappedMethod;
                if (method != null)
                {
                    getter = method.method;
                }
            }
            if (getter == null)
            {
                getter = this.GetGetMethod(false);
            }
            if (getter != null)
            {
                try
                {
                    return getter.Invoke(obj, invokeAttr, binder, (index == null) ? new object[0] : index, culture);
                }
                catch (TargetInvocationException exception)
                {
                    throw exception.InnerException;
                }
            }
            return Microsoft.JScript.Missing.Value;
        }

        public sealed override bool IsDefined(Type type, bool inherit)
        {
            return false;
        }

        internal IReflect PropertyIR()
        {
            if (this.getter is JSFieldMethod)
            {
                return ((JSFieldMethod) this.getter).ReturnIR();
            }
            if (this.setter != null)
            {
                ParameterInfo[] parameters = this.setter.GetParameters();
                if (parameters.Length > 0)
                {
                    ParameterInfo info = parameters[parameters.Length - 1];
                    if (info is ParameterDeclaration)
                    {
                        return ((ParameterDeclaration) info).ParameterIReflect;
                    }
                    return info.ParameterType;
                }
            }
            return Typeob.Void;
        }

        [DebuggerStepThrough, DebuggerHidden]
        internal static void SetValue(PropertyInfo prop, object obj, object value, object[] index)
        {
            JSProperty property = prop as JSProperty;
            if (property != null)
            {
                property.SetValue(obj, value, BindingFlags.ExactBinding, null, index, null);
            }
            else
            {
                MethodInfo setMethod = GetSetMethod(prop, false);
                if (setMethod == null)
                {
                    throw new MissingMethodException();
                }
                int n = (index == null) ? 0 : index.Length;
                object[] target = new object[n + 1];
                if (n > 0)
                {
                    ArrayObject.Copy(index, 0, target, 0, n);
                }
                target[n] = value;
                setMethod.Invoke(obj, BindingFlags.ExactBinding, null, target, null);
            }
        }

        [DebuggerHidden, DebuggerStepThrough]
        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            MethodInfo setter = this.setter;
            JSObject obj2 = obj as JSObject;
            if ((setter == null) && (obj2 != null))
            {
                setter = obj2.GetMethod("set_" + this.name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                JSWrappedMethod method = setter as JSWrappedMethod;
                if (method != null)
                {
                    setter = method.method;
                }
            }
            if (setter == null)
            {
                setter = this.GetSetMethod(false);
            }
            if (setter != null)
            {
                if ((index == null) || (index.Length == 0))
                {
                    setter.Invoke(obj, invokeAttr, binder, new object[] { value }, culture);
                }
                else
                {
                    int length = index.Length;
                    object[] target = new object[length + 1];
                    ArrayObject.Copy(index, 0, target, 0, length);
                    target[length] = value;
                    setter.Invoke(obj, invokeAttr, binder, target, culture);
                }
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
                return (GetGetMethod(this, true) != null);
            }
        }

        public override bool CanWrite
        {
            get
            {
                return (GetSetMethod(this, true) != null);
            }
        }

        public override Type DeclaringType
        {
            get
            {
                if (this.getter != null)
                {
                    return this.getter.DeclaringType;
                }
                return this.setter.DeclaringType;
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
                return this.name;
            }
        }

        public override Type PropertyType
        {
            get
            {
                if (this.getter != null)
                {
                    return this.getter.ReturnType;
                }
                if (this.setter != null)
                {
                    ParameterInfo[] parameters = this.setter.GetParameters();
                    if (parameters.Length > 0)
                    {
                        return parameters[parameters.Length - 1].ParameterType;
                    }
                }
                return Typeob.Void;
            }
        }

        public override Type ReflectedType
        {
            get
            {
                if (this.getter != null)
                {
                    return this.getter.ReflectedType;
                }
                return this.setter.ReflectedType;
            }
        }
    }
}

