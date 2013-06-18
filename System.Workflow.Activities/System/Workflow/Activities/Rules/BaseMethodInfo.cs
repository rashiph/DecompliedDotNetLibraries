namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;

    internal abstract class BaseMethodInfo : MethodInfo
    {
        protected MethodInfo actualMethod;
        protected ParameterInfo[] expectedParameters;
        protected Type resultType;

        public BaseMethodInfo(MethodInfo method)
        {
            this.actualMethod = method;
            this.resultType = method.ReturnType;
            this.expectedParameters = method.GetParameters();
        }

        public override bool Equals(object obj)
        {
            BaseMethodInfo info = obj as BaseMethodInfo;
            if (((info == null) || (this.actualMethod != info.actualMethod)) || ((this.resultType != info.resultType) || (this.expectedParameters.Length != info.expectedParameters.Length)))
            {
                return false;
            }
            for (int i = 0; i < this.expectedParameters.Length; i++)
            {
                if (this.expectedParameters[i].ParameterType != info.expectedParameters[i].ParameterType)
                {
                    return false;
                }
            }
            return true;
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

        public override int GetHashCode()
        {
            int num = this.actualMethod.GetHashCode() ^ this.resultType.GetHashCode();
            for (int i = 0; i < this.expectedParameters.Length; i++)
            {
                num ^= this.expectedParameters[i].GetHashCode();
            }
            return num;
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
            throw new NotImplementedException();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return this.actualMethod.IsDefined(attributeType, inherit);
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
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.resultType;
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

