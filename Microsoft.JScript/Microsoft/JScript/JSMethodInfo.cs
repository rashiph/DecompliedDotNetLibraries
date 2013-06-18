namespace Microsoft.JScript
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;

    public sealed class JSMethodInfo : MethodInfo
    {
        private object[] attributes;
        private Type declaringType;
        private MethodAttributes methAttributes;
        internal MethodInfo method;
        private MethodInvoker methodInvoker;
        private string name;
        private ParameterInfo[] parameters;

        internal JSMethodInfo(MethodInfo method)
        {
            this.method = method;
            this.methAttributes = method.Attributes;
        }

        public override MethodInfo GetBaseDefinition()
        {
            return this.method.GetBaseDefinition();
        }

        public sealed override object[] GetCustomAttributes(bool inherit)
        {
            object[] attributes = this.attributes;
            if (attributes != null)
            {
                return attributes;
            }
            return (this.attributes = this.method.GetCustomAttributes(true));
        }

        public sealed override object[] GetCustomAttributes(Type type, bool inherit)
        {
            if (type != typeof(JSFunctionAttribute))
            {
                return null;
            }
            object[] attributes = this.attributes;
            if (attributes != null)
            {
                return attributes;
            }
            return (this.attributes = Microsoft.JScript.CustomAttribute.GetCustomAttributes(this.method, type, true));
        }

        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            return this.method.GetMethodImplementationFlags();
        }

        public override ParameterInfo[] GetParameters()
        {
            ParameterInfo[] parameters = this.parameters;
            if (parameters != null)
            {
                return parameters;
            }
            parameters = this.method.GetParameters();
            int index = 0;
            int length = parameters.Length;
            while (index < length)
            {
                parameters[index] = new JSParameterInfo(parameters[index]);
                index++;
            }
            return (this.parameters = parameters);
        }

        [DebuggerStepThrough, DebuggerHidden]
        public override object Invoke(object obj, BindingFlags options, Binder binder, object[] parameters, CultureInfo culture)
        {
            MethodInfo method = TypeReferences.ToExecutionContext(this.method);
            if (binder != null)
            {
                try
                {
                    return method.Invoke(obj, options, binder, parameters, culture);
                }
                catch (TargetInvocationException exception)
                {
                    throw exception.InnerException;
                }
            }
            MethodInvoker methodInvoker = this.methodInvoker;
            if ((methodInvoker == null) && ((this.methodInvoker = MethodInvoker.GetInvokerFor(method)) == null))
            {
                try
                {
                    return method.Invoke(obj, options, binder, parameters, culture);
                }
                catch (TargetInvocationException exception2)
                {
                    throw exception2.InnerException;
                }
            }
            return methodInvoker.Invoke(obj, parameters);
        }

        public sealed override bool IsDefined(Type type, bool inherit)
        {
            object[] attributes = this.attributes;
            if (attributes == null)
            {
                this.attributes = attributes = Microsoft.JScript.CustomAttribute.GetCustomAttributes(this.method, type, true);
            }
            return (attributes.Length > 0);
        }

        public override string ToString()
        {
            return this.method.ToString();
        }

        public override MethodAttributes Attributes
        {
            get
            {
                return this.methAttributes;
            }
        }

        public override Type DeclaringType
        {
            get
            {
                Type declaringType = this.declaringType;
                if (declaringType == null)
                {
                    this.declaringType = declaringType = this.method.DeclaringType;
                }
                return declaringType;
            }
        }

        public override MemberTypes MemberType
        {
            get
            {
                return MemberTypes.Method;
            }
        }

        public override RuntimeMethodHandle MethodHandle
        {
            get
            {
                return this.method.MethodHandle;
            }
        }

        public override string Name
        {
            get
            {
                string name = this.name;
                if (name == null)
                {
                    this.name = name = this.method.Name;
                }
                return name;
            }
        }

        public override Type ReflectedType
        {
            get
            {
                return this.method.ReflectedType;
            }
        }

        public override Type ReturnType
        {
            get
            {
                return this.method.ReturnType;
            }
        }

        public override ICustomAttributeProvider ReturnTypeCustomAttributes
        {
            get
            {
                return this.method.ReturnTypeCustomAttributes;
            }
        }
    }
}

