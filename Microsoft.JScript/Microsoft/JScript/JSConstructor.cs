namespace Microsoft.JScript
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;

    public sealed class JSConstructor : ConstructorInfo
    {
        internal FunctionObject cons;

        internal JSConstructor(FunctionObject cons)
        {
            this.cons = cons;
        }

        internal object Construct(object thisob, object[] args)
        {
            return LateBinding.CallValue(this.cons, args, true, false, this.cons.engine, thisob, JSBinder.ob, null, null);
        }

        internal string GetClassFullName()
        {
            return ((ClassScope) this.cons.enclosing_scope).GetFullName();
        }

        internal ClassScope GetClassScope()
        {
            return (ClassScope) this.cons.enclosing_scope;
        }

        internal ConstructorInfo GetConstructorInfo(CompilerGlobals compilerGlobals)
        {
            return this.cons.GetConstructorInfo(compilerGlobals);
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            if (this.cons != null)
            {
                CustomAttributeList customAttributes = this.cons.customAttributes;
                if (customAttributes != null)
                {
                    return (object[]) customAttributes.Evaluate(false);
                }
            }
            return new object[0];
        }

        public override object[] GetCustomAttributes(Type t, bool inherit)
        {
            return new object[0];
        }

        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            return MethodImplAttributes.IL;
        }

        internal PackageScope GetPackage()
        {
            return ((ClassScope) this.cons.enclosing_scope).GetPackage();
        }

        public override ParameterInfo[] GetParameters()
        {
            return this.cons.parameter_declarations;
        }

        [DebuggerStepThrough, DebuggerHidden]
        public override object Invoke(BindingFlags options, Binder binder, object[] parameters, CultureInfo culture)
        {
            return LateBinding.CallValue(this.cons, parameters, true, false, this.cons.engine, null, binder, culture, null);
        }

        [DebuggerStepThrough, DebuggerHidden]
        public override object Invoke(object obj, BindingFlags options, Binder binder, object[] parameters, CultureInfo culture)
        {
            return this.cons.Call(parameters, obj, binder, culture);
        }

        internal bool IsAccessibleFrom(ScriptObject scope)
        {
            while ((scope != null) && !(scope is ClassScope))
            {
                scope = scope.GetParent();
            }
            ClassScope other = (ClassScope) this.cons.enclosing_scope;
            if (base.IsPrivate)
            {
                if (scope == null)
                {
                    return false;
                }
                if (scope != other)
                {
                    return ((ClassScope) scope).IsNestedIn(other, false);
                }
                return true;
            }
            if (base.IsFamily)
            {
                if (scope == null)
                {
                    return false;
                }
                if (!((ClassScope) scope).IsSameOrDerivedFrom(other))
                {
                    return ((ClassScope) scope).IsNestedIn(other, false);
                }
                return true;
            }
            if ((base.IsFamilyOrAssembly && (scope != null)) && (((ClassScope) scope).IsSameOrDerivedFrom(other) || ((ClassScope) scope).IsNestedIn(other, false)))
            {
                return true;
            }
            if (scope == null)
            {
                return (other.GetPackage() == null);
            }
            return (other.GetPackage() == ((ClassScope) scope).GetPackage());
        }

        public override bool IsDefined(Type type, bool inherit)
        {
            return false;
        }

        internal Type OuterClassType()
        {
            FieldInfo outerClassField = ((ClassScope) this.cons.enclosing_scope).outerClassField;
            if (outerClassField != null)
            {
                return outerClassField.FieldType;
            }
            return null;
        }

        public override MethodAttributes Attributes
        {
            get
            {
                return this.cons.attributes;
            }
        }

        public override Type DeclaringType
        {
            get
            {
                return Microsoft.JScript.Convert.ToType(this.cons.enclosing_scope);
            }
        }

        public override MemberTypes MemberType
        {
            get
            {
                return MemberTypes.Constructor;
            }
        }

        public override RuntimeMethodHandle MethodHandle
        {
            get
            {
                return this.GetConstructorInfo(null).MethodHandle;
            }
        }

        public override string Name
        {
            get
            {
                return this.cons.name;
            }
        }

        public override Type ReflectedType
        {
            get
            {
                return this.DeclaringType;
            }
        }
    }
}

