namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;

    internal sealed class JSNativeMethod : JSMethod
    {
        private VsaEngine engine;
        private ParameterInfo[] formalParams;
        private bool hasEngine;
        private bool hasThis;
        private bool hasVarargs;
        private MethodInfo method;

        internal JSNativeMethod(MethodInfo method, object obj, VsaEngine engine) : base(obj)
        {
            this.method = method;
            this.formalParams = method.GetParameters();
            object[] objArray = Microsoft.JScript.CustomAttribute.GetCustomAttributes(method, typeof(JSFunctionAttribute), false);
            JSFunctionAttribute attribute = (objArray.Length > 0) ? ((JSFunctionAttribute) objArray[0]) : new JSFunctionAttribute(JSFunctionAttributeEnum.None);
            JSFunctionAttributeEnum attributeValue = attribute.attributeValue;
            if ((attributeValue & JSFunctionAttributeEnum.HasThisObject) != JSFunctionAttributeEnum.None)
            {
                this.hasThis = true;
            }
            if ((attributeValue & JSFunctionAttributeEnum.HasEngine) != JSFunctionAttributeEnum.None)
            {
                this.hasEngine = true;
            }
            if ((attributeValue & JSFunctionAttributeEnum.HasVarArgs) != JSFunctionAttributeEnum.None)
            {
                this.hasVarargs = true;
            }
            this.engine = engine;
        }

        internal override object Construct(object[] args)
        {
            throw new JScriptException(JSError.NoConstructor);
        }

        private object[] ConvertParams(int offset, object[] parameters, Binder binder, CultureInfo culture)
        {
            int length = this.formalParams.Length;
            if (this.hasVarargs)
            {
                length--;
            }
            for (int i = offset; i < length; i++)
            {
                Type parameterType = this.formalParams[i].ParameterType;
                if (parameterType != Typeob.Object)
                {
                    parameters[i] = binder.ChangeType(parameters[i], parameterType, culture);
                }
            }
            return parameters;
        }

        internal override MethodInfo GetMethodInfo(CompilerGlobals compilerGlobals)
        {
            return this.method;
        }

        public override ParameterInfo[] GetParameters()
        {
            return this.formalParams;
        }

        [DebuggerHidden, DebuggerStepThrough]
        internal override object Invoke(object obj, object thisob, BindingFlags options, Binder binder, object[] parameters, CultureInfo culture)
        {
            int length = this.formalParams.Length;
            int n = (parameters != null) ? parameters.Length : 0;
            if ((!this.hasThis && !this.hasVarargs) && (length == n))
            {
                if (binder != null)
                {
                    return TypeReferences.ToExecutionContext(this.method).Invoke(base.obj, BindingFlags.SuppressChangeType, null, this.ConvertParams(0, parameters, binder, culture), null);
                }
                return TypeReferences.ToExecutionContext(this.method).Invoke(base.obj, options, binder, parameters, culture);
            }
            int index = (this.hasThis ? 1 : 0) + (this.hasEngine ? 1 : 0);
            object[] target = new object[length];
            if (this.hasThis)
            {
                target[0] = thisob;
                if (this.hasEngine)
                {
                    target[1] = this.engine;
                }
            }
            else if (this.hasEngine)
            {
                target[0] = this.engine;
            }
            if (this.hasVarargs)
            {
                if (length == (index + 1))
                {
                    target[index] = parameters;
                }
                else
                {
                    int num4 = (length - 1) - index;
                    if (n > num4)
                    {
                        ArrayObject.Copy(parameters, 0, target, index, num4);
                        int num5 = n - num4;
                        object[] objArray2 = new object[num5];
                        ArrayObject.Copy(parameters, num4, objArray2, 0, num5);
                        target[length - 1] = objArray2;
                    }
                    else
                    {
                        ArrayObject.Copy(parameters, 0, target, index, n);
                        for (int i = n; i < num4; i++)
                        {
                            target[i + index] = Microsoft.JScript.Missing.Value;
                        }
                        target[length - 1] = new object[0];
                    }
                }
            }
            else
            {
                if (parameters != null)
                {
                    if ((length - index) < n)
                    {
                        ArrayObject.Copy(parameters, 0, target, index, length - index);
                    }
                    else
                    {
                        ArrayObject.Copy(parameters, 0, target, index, n);
                    }
                }
                if ((length - index) > n)
                {
                    for (int j = n + index; j < length; j++)
                    {
                        if (((j == (length - 1)) && this.formalParams[j].ParameterType.IsArray) && Microsoft.JScript.CustomAttribute.IsDefined(this.formalParams[j], typeof(ParamArrayAttribute), true))
                        {
                            target[j] = Array.CreateInstance(this.formalParams[j].ParameterType.GetElementType(), 0);
                        }
                        else
                        {
                            target[j] = Microsoft.JScript.Missing.Value;
                        }
                    }
                }
            }
            if (binder != null)
            {
                return TypeReferences.ToExecutionContext(this.method).Invoke(base.obj, BindingFlags.SuppressChangeType, null, this.ConvertParams(index, target, binder, culture), null);
            }
            return TypeReferences.ToExecutionContext(this.method).Invoke(base.obj, options, binder, target, culture);
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

