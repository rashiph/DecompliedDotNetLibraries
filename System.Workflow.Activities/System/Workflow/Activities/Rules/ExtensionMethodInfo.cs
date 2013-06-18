namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;

    internal class ExtensionMethodInfo : MethodInfo
    {
        private MethodInfo actualMethod;
        private int actualParameterLength;
        private Type assumedDeclaringType;
        private ParameterInfo[] expectedParameters;
        private bool hasOutOrRefParameters;

        public ExtensionMethodInfo(MethodInfo method, ParameterInfo[] actualParameters)
        {
            this.actualMethod = method;
            this.actualParameterLength = actualParameters.Length;
            if (this.actualParameterLength < 2)
            {
                this.expectedParameters = new ParameterInfo[0];
            }
            else
            {
                this.expectedParameters = new ParameterInfo[this.actualParameterLength - 1];
                Array.Copy(actualParameters, 1, this.expectedParameters, 0, this.actualParameterLength - 1);
                foreach (ParameterInfo info in this.expectedParameters)
                {
                    if (info.ParameterType.IsByRef)
                    {
                        this.hasOutOrRefParameters = true;
                    }
                }
            }
            this.assumedDeclaringType = actualParameters[0].ParameterType;
        }

        public override MethodInfo GetBaseDefinition()
        {
            return this.actualMethod.GetBaseDefinition();
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return this.actualMethod.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return this.actualMethod.GetCustomAttributes(attributeType, inherit);
        }

        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            return this.actualMethod.GetMethodImplementationFlags();
        }

        public override ParameterInfo[] GetParameters()
        {
            return this.expectedParameters;
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            object[] destinationArray = new object[this.actualParameterLength];
            if (this.actualParameterLength > 1)
            {
                Array.Copy(parameters, 0, destinationArray, 1, this.actualParameterLength - 1);
            }
            if (obj == null)
            {
                destinationArray[0] = null;
            }
            else
            {
                destinationArray[0] = Executor.AdjustType(obj.GetType(), obj, this.assumedDeclaringType);
            }
            object obj2 = this.actualMethod.Invoke(null, invokeAttr, binder, destinationArray, culture);
            if (this.hasOutOrRefParameters)
            {
                Array.Copy(destinationArray, 1, parameters, 0, this.actualParameterLength - 1);
            }
            return obj2;
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return this.actualMethod.IsDefined(attributeType, inherit);
        }

        public Type AssumedDeclaringType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.assumedDeclaringType;
            }
        }

        public override MethodAttributes Attributes
        {
            get
            {
                return (this.actualMethod.Attributes & ~MethodAttributes.Static);
            }
        }

        public override Type DeclaringType
        {
            get
            {
                return this.actualMethod.DeclaringType;
            }
        }

        public override RuntimeMethodHandle MethodHandle
        {
            get
            {
                return this.actualMethod.MethodHandle;
            }
        }

        public override string Name
        {
            get
            {
                return this.actualMethod.Name;
            }
        }

        public override Type ReflectedType
        {
            get
            {
                return this.actualMethod.ReflectedType;
            }
        }

        public override Type ReturnType
        {
            get
            {
                return this.actualMethod.ReturnType;
            }
        }

        public override ICustomAttributeProvider ReturnTypeCustomAttributes
        {
            get
            {
                return this.actualMethod.ReturnTypeCustomAttributes;
            }
        }
    }
}

