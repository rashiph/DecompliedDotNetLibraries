namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.CodeDom;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;

    internal sealed class DesignTimeConstructorInfo : ConstructorInfo
    {
        private Attribute[] attributes;
        private CodeMemberMethod codeConstructor;
        private DesignTimeType declaringType;
        private ParameterInfo[] parameters;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal DesignTimeConstructorInfo(DesignTimeType declaringType, CodeMemberMethod codeConstructor)
        {
            this.declaringType = declaringType;
            this.codeConstructor = codeConstructor;
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return this.GetCustomAttributes(typeof(object), inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }
            if (this.attributes == null)
            {
                this.attributes = Helper.LoadCustomAttributes(this.codeConstructor.CustomAttributes, this.DeclaringType as DesignTimeType);
            }
            return Helper.GetCustomAttributes(attributeType, inherit, this.attributes, this);
        }

        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            return MethodImplAttributes.IL;
        }

        public override ParameterInfo[] GetParameters()
        {
            if (this.parameters == null)
            {
                CodeParameterDeclarationExpressionCollection parameters = this.codeConstructor.Parameters;
                ParameterInfo[] infoArray = new ParameterInfo[parameters.Count];
                for (int i = 0; i < parameters.Count; i++)
                {
                    infoArray[i] = new DesignTimeParameterInfo(parameters[i], i, this);
                }
                this.parameters = infoArray;
            }
            return this.parameters;
        }

        public override object Invoke(BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            throw new NotImplementedException(TypeSystemSR.GetString("Error_RuntimeNotSupported"));
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            throw new NotImplementedException(TypeSystemSR.GetString("Error_RuntimeNotSupported"));
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }
            if (this.attributes == null)
            {
                this.attributes = Helper.LoadCustomAttributes(this.codeConstructor.CustomAttributes, this.DeclaringType as DesignTimeType);
            }
            return Helper.IsDefined(attributeType, inherit, this.attributes, this);
        }

        public override MethodAttributes Attributes
        {
            get
            {
                return Helper.ConvertToMethodAttributes(this.codeConstructor.Attributes);
            }
        }

        public override Type DeclaringType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.declaringType;
            }
        }

        public override RuntimeMethodHandle MethodHandle
        {
            get
            {
                throw new NotImplementedException(TypeSystemSR.GetString("Error_RuntimeNotSupported"));
            }
        }

        public override string Name
        {
            get
            {
                return ".ctor";
            }
        }

        public override Type ReflectedType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.declaringType;
            }
        }
    }
}

