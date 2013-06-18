namespace Microsoft.JScript
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;

    internal sealed class JSFieldMethod : JSMethod
    {
        private static readonly ParameterInfo[] EmptyParams = new ParameterInfo[0];
        internal FieldInfo field;
        internal FunctionObject func;

        internal JSFieldMethod(FieldInfo field, object obj) : base(obj)
        {
            this.field = field;
            this.func = null;
            if (field.IsLiteral)
            {
                object obj2 = (field is JSVariableField) ? ((JSVariableField) field).value : field.GetValue(null);
                if (obj2 is FunctionObject)
                {
                    this.func = (FunctionObject) obj2;
                }
            }
        }

        internal override object Construct(object[] args)
        {
            return LateBinding.CallValue(this.field.GetValue(base.obj), args, true, false, ((ScriptObject) base.obj).engine, null, JSBinder.ob, null, null);
        }

        internal ScriptObject EnclosingScope()
        {
            if (this.func != null)
            {
                return this.func.enclosing_scope;
            }
            return null;
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            if (this.func != null)
            {
                CustomAttributeList customAttributes = this.func.customAttributes;
                if (customAttributes != null)
                {
                    return (object[]) customAttributes.Evaluate(inherit);
                }
            }
            return new object[0];
        }

        internal override MethodInfo GetMethodInfo(CompilerGlobals compilerGlobals)
        {
            return this.func.GetMethodInfo(compilerGlobals);
        }

        public override ParameterInfo[] GetParameters()
        {
            if (this.func != null)
            {
                return this.func.parameter_declarations;
            }
            return EmptyParams;
        }

        [DebuggerHidden, DebuggerStepThrough]
        internal override object Invoke(object obj, object thisob, BindingFlags options, Binder binder, object[] parameters, CultureInfo culture)
        {
            bool construct = (options & BindingFlags.CreateInstance) != BindingFlags.Default;
            bool brackets = ((options & BindingFlags.GetProperty) != BindingFlags.Default) && ((options & BindingFlags.InvokeMethod) == BindingFlags.Default);
            object func = this.func;
            if (func == null)
            {
                func = this.field.GetValue(base.obj);
            }
            FunctionObject obj3 = func as FunctionObject;
            JSObject ob = obj as JSObject;
            if ((((ob != null) && (obj3 != null)) && (obj3.isMethod && ((obj3.attributes & MethodAttributes.Virtual) != MethodAttributes.PrivateScope))) && ((ob.GetParent() != obj3.enclosing_scope) && ((ClassScope) obj3.enclosing_scope).HasInstance(ob)))
            {
                LateBinding binding = new LateBinding(obj3.name) {
                    obj = ob
                };
                return binding.Call(parameters, construct, brackets, ((ScriptObject) base.obj).engine);
            }
            return LateBinding.CallValue(func, parameters, construct, brackets, ((ScriptObject) base.obj).engine, thisob, binder, culture, null);
        }

        internal bool IsAccessibleFrom(ScriptObject scope)
        {
            return ((JSMemberField) this.field).IsAccessibleFrom(scope);
        }

        internal IReflect ReturnIR()
        {
            if (this.func != null)
            {
                return this.func.ReturnType(null);
            }
            return Typeob.Object;
        }

        public override MethodAttributes Attributes
        {
            get
            {
                if (this.func != null)
                {
                    return this.func.attributes;
                }
                if (this.field.IsPublic)
                {
                    return MethodAttributes.Public;
                }
                if (this.field.IsFamily)
                {
                    return MethodAttributes.Family;
                }
                if (this.field.IsAssembly)
                {
                    return MethodAttributes.Assembly;
                }
                return MethodAttributes.Private;
            }
        }

        public override Type DeclaringType
        {
            get
            {
                if (this.func != null)
                {
                    return Microsoft.JScript.Convert.ToType(this.func.enclosing_scope);
                }
                return Typeob.Object;
            }
        }

        public override string Name
        {
            get
            {
                return this.field.Name;
            }
        }

        public override Type ReturnType
        {
            get
            {
                if (this.func != null)
                {
                    return Microsoft.JScript.Convert.ToType(this.func.ReturnType(null));
                }
                return Typeob.Object;
            }
        }
    }
}

