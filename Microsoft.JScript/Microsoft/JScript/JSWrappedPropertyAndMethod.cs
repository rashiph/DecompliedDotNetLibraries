namespace Microsoft.JScript
{
    using System;
    using System.Globalization;
    using System.Reflection;

    internal class JSWrappedPropertyAndMethod : JSWrappedProperty
    {
        protected MethodInfo method;
        private ParameterInfo[] parameters;

        internal JSWrappedPropertyAndMethod(PropertyInfo property, MethodInfo method, object obj) : base(property, obj)
        {
            this.method = method;
            this.parameters = method.GetParameters();
        }

        private object[] CheckArguments(object[] arguments)
        {
            if (arguments == null)
            {
                return arguments;
            }
            int length = arguments.Length;
            int num2 = this.parameters.Length;
            if (length >= num2)
            {
                return arguments;
            }
            object[] target = new object[num2];
            ArrayObject.Copy(arguments, target, length);
            for (int i = length; i < num2; i++)
            {
                target[i] = Type.Missing;
            }
            return target;
        }

        internal object Invoke(object obj, BindingFlags options, Binder binder, object[] parameters, CultureInfo culture)
        {
            parameters = this.CheckArguments(parameters);
            if ((base.obj != null) && !(base.obj is Type))
            {
                obj = base.obj;
            }
            return this.method.Invoke(obj, options, binder, parameters, culture);
        }

        public MethodInfo Method
        {
            get
            {
                return this.method;
            }
        }
    }
}

