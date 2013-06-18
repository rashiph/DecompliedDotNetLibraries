namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.CodeDom;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;

    internal class DesignTimeMethodInfo : MethodInfo
    {
        private Attribute[] attributes;
        private DesignTimeType declaringType;
        private bool isSpecialName;
        private CodeMemberMethod methodInfo;
        private ParameterInfo[] parameters;
        private ParameterInfo returnParam;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal DesignTimeMethodInfo(DesignTimeType declaringType, CodeMemberMethod methodInfo)
        {
            this.declaringType = declaringType;
            this.methodInfo = methodInfo;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal DesignTimeMethodInfo(DesignTimeType declaringType, CodeMemberMethod methodInfo, bool isSpecialName)
        {
            this.declaringType = declaringType;
            this.methodInfo = methodInfo;
            this.isSpecialName = isSpecialName;
        }

        public override MethodInfo GetBaseDefinition()
        {
            throw new NotImplementedException();
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
                if (this.methodInfo == null)
                {
                    this.attributes = new Attribute[0];
                }
                else
                {
                    this.attributes = Helper.LoadCustomAttributes(this.methodInfo.CustomAttributes, this.DeclaringType as DesignTimeType);
                }
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
                CodeParameterDeclarationExpressionCollection parameters = this.methodInfo.Parameters;
                ParameterInfo[] infoArray = new ParameterInfo[parameters.Count];
                for (int i = 0; i < parameters.Count; i++)
                {
                    infoArray[i] = new DesignTimeParameterInfo(parameters[i], i, this);
                }
                this.parameters = infoArray;
            }
            return this.parameters;
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
                this.attributes = Helper.LoadCustomAttributes(this.methodInfo.CustomAttributes, this.DeclaringType as DesignTimeType);
            }
            return Helper.IsDefined(attributeType, inherit, this.attributes, this);
        }

        public override MethodAttributes Attributes
        {
            get
            {
                return (Helper.ConvertToMethodAttributes(this.methodInfo.Attributes) | (this.isSpecialName ? MethodAttributes.SpecialName : MethodAttributes.PrivateScope));
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
                return Helper.EnsureTypeName(this.methodInfo.Name);
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

        public override ParameterInfo ReturnParameter
        {
            get
            {
                if (this.returnParam == null)
                {
                    this.returnParam = new DesignTimeParameterInfo(this.methodInfo.ReturnType, this);
                }
                return this.returnParam;
            }
        }

        public override Type ReturnType
        {
            get
            {
                return this.declaringType.ResolveType(DesignTimeType.GetTypeNameFromCodeTypeReference(this.methodInfo.ReturnType, this.declaringType));
            }
        }

        public override ICustomAttributeProvider ReturnTypeCustomAttributes
        {
            get
            {
                return null;
            }
        }
    }
}

