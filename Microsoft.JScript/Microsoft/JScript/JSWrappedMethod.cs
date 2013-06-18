namespace Microsoft.JScript
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;

    internal sealed class JSWrappedMethod : JSMethod, IWrappedMember
    {
        internal MethodInfo method;
        private ParameterInfo[] pars;

        internal JSWrappedMethod(MethodInfo method, object obj) : base(obj)
        {
            base.obj = obj;
            if (method is JSMethodInfo)
            {
                method = ((JSMethodInfo) method).method;
            }
            this.method = method.GetBaseDefinition();
            this.pars = this.method.GetParameters();
            if ((obj is JSObject) && !Typeob.JSObject.IsAssignableFrom(method.DeclaringType))
            {
                if (obj is BooleanObject)
                {
                    base.obj = ((BooleanObject) obj).value;
                }
                else if (obj is NumberObject)
                {
                    base.obj = ((NumberObject) obj).value;
                }
                else if (obj is StringObject)
                {
                    base.obj = ((StringObject) obj).value;
                }
                else if (obj is ArrayWrapper)
                {
                    base.obj = ((ArrayWrapper) obj).value;
                }
            }
        }

        private object[] CheckArguments(object[] args)
        {
            object[] target = args;
            if ((args != null) && (args.Length < this.pars.Length))
            {
                target = new object[this.pars.Length];
                ArrayObject.Copy(args, target, args.Length);
                int length = args.Length;
                int num2 = this.pars.Length;
                while (length < num2)
                {
                    target[length] = Type.Missing;
                    length++;
                }
            }
            return target;
        }

        internal override object Construct(object[] args)
        {
            if (this.method is JSMethod)
            {
                return ((JSMethod) this.method).Construct(args);
            }
            if ((this.method.GetParameters().Length == 0) && (this.method.ReturnType == Typeob.Object))
            {
                object obj2 = this.method.Invoke(base.obj, BindingFlags.SuppressChangeType, null, null, null);
                if (obj2 is ScriptFunction)
                {
                    return ((ScriptFunction) obj2).Construct(args);
                }
            }
            throw new JScriptException(JSError.NoConstructor);
        }

        internal override string GetClassFullName()
        {
            if (this.method is JSMethod)
            {
                return ((JSMethod) this.method).GetClassFullName();
            }
            return this.method.DeclaringType.FullName;
        }

        internal override MethodInfo GetMethodInfo(CompilerGlobals compilerGlobals)
        {
            if (this.method is JSMethod)
            {
                return ((JSMethod) this.method).GetMethodInfo(compilerGlobals);
            }
            return this.method;
        }

        internal override PackageScope GetPackage()
        {
            if (this.method is JSMethod)
            {
                return ((JSMethod) this.method).GetPackage();
            }
            return null;
        }

        public override ParameterInfo[] GetParameters()
        {
            return this.pars;
        }

        public object GetWrappedObject()
        {
            return base.obj;
        }

        [DebuggerHidden, DebuggerStepThrough]
        public override object Invoke(object obj, BindingFlags options, Binder binder, object[] parameters, CultureInfo culture)
        {
            parameters = this.CheckArguments(parameters);
            return this.Invoke(base.obj, base.obj, options, binder, parameters, culture);
        }

        [DebuggerHidden, DebuggerStepThrough]
        internal override object Invoke(object obj, object thisob, BindingFlags options, Binder binder, object[] parameters, CultureInfo culture)
        {
            parameters = this.CheckArguments(parameters);
            if ((base.obj != null) && !(base.obj is Type))
            {
                obj = base.obj;
            }
            if (this.method is JSMethod)
            {
                return ((JSMethod) this.method).Invoke(obj, thisob, options, binder, parameters, culture);
            }
            return this.method.Invoke(obj, options, binder, parameters, culture);
        }

        public override string ToString()
        {
            return this.method.ToString();
        }

        public override MethodAttributes Attributes
        {
            get
            {
                return this.method.Attributes;
            }
        }

        public override Type DeclaringType
        {
            get
            {
                return this.method.DeclaringType;
            }
        }

        public override string Name
        {
            get
            {
                return this.method.Name;
            }
        }

        public override Type ReturnType
        {
            get
            {
                return this.method.ReturnType;
            }
        }
    }
}

