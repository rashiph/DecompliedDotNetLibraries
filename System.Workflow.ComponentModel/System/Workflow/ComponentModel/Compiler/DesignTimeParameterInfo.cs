namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.CodeDom;
    using System.Reflection;

    internal sealed class DesignTimeParameterInfo : ParameterInfo
    {
        private CodeTypeReference codeParameterType;
        private bool isRef;

        internal DesignTimeParameterInfo(CodeTypeReference codeParameterType, MemberInfo member)
        {
            base.MemberImpl = member;
            base.NameImpl = null;
            this.codeParameterType = codeParameterType;
            base.AttrsImpl = ParameterAttributes.None;
            base.PositionImpl = -1;
        }

        internal DesignTimeParameterInfo(CodeParameterDeclarationExpression codeParameter, int position, MemberInfo member)
        {
            base.MemberImpl = member;
            base.NameImpl = Helper.EnsureTypeName(codeParameter.Name);
            this.codeParameterType = codeParameter.Type;
            base.AttrsImpl = Helper.ConvertToParameterAttributes(codeParameter.Direction);
            this.isRef = codeParameter.Direction == FieldDirection.Ref;
            base.PositionImpl = position;
        }

        public override Type ParameterType
        {
            get
            {
                string typeNameFromCodeTypeReference = DesignTimeType.GetTypeNameFromCodeTypeReference(this.codeParameterType, this.Member.DeclaringType as DesignTimeType);
                if (((base.AttrsImpl & ParameterAttributes.Out) > ParameterAttributes.None) || this.isRef)
                {
                    typeNameFromCodeTypeReference = typeNameFromCodeTypeReference + '&';
                }
                base.ClassImpl = (this.Member.DeclaringType as DesignTimeType).ResolveType(typeNameFromCodeTypeReference);
                return base.ParameterType;
            }
        }
    }
}

