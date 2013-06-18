namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.CodeDom;
    using System.Reflection;
    using System.Runtime;

    internal sealed class DesignTimeEventInfo : EventInfo
    {
        private DesignTimeMethodInfo addMethod;
        private Attribute[] attributes;
        private CodeMemberEvent codeDomEvent;
        private DesignTimeType declaringType;
        private MemberAttributes memberAttributes;
        private string name;
        private DesignTimeMethodInfo removeMethod;

        internal DesignTimeEventInfo(DesignTimeType declaringType, CodeMemberEvent codeDomEvent)
        {
            if (declaringType == null)
            {
                throw new ArgumentNullException("Declaring Type");
            }
            if (codeDomEvent == null)
            {
                throw new ArgumentNullException("codeDomEvent");
            }
            this.declaringType = declaringType;
            this.codeDomEvent = codeDomEvent;
            this.name = Helper.EnsureTypeName(codeDomEvent.Name);
            this.memberAttributes = codeDomEvent.Attributes;
            this.addMethod = null;
            this.removeMethod = null;
        }

        public override MethodInfo GetAddMethod(bool nonPublic)
        {
            if ((this.addMethod == null) && (this.declaringType.ResolveType(DesignTimeType.GetTypeNameFromCodeTypeReference(this.codeDomEvent.Type, this.declaringType)) != null))
            {
                CodeMemberMethod methodInfo = new CodeMemberMethod {
                    Name = "add_" + this.name,
                    ReturnType = new CodeTypeReference(typeof(void))
                };
                methodInfo.Parameters.Add(new CodeParameterDeclarationExpression(this.codeDomEvent.Type, "Handler"));
                methodInfo.Attributes = this.memberAttributes;
                this.addMethod = new DesignTimeMethodInfo(this.declaringType, methodInfo, true);
            }
            return this.addMethod;
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
                this.attributes = Helper.LoadCustomAttributes(this.codeDomEvent.CustomAttributes, this.DeclaringType as DesignTimeType);
            }
            return Helper.GetCustomAttributes(attributeType, inherit, this.attributes, this);
        }

        public override MethodInfo GetRaiseMethod(bool nonPublic)
        {
            return null;
        }

        public override MethodInfo GetRemoveMethod(bool nonPublic)
        {
            if (this.removeMethod == null)
            {
                Type type = this.declaringType.ResolveType(DesignTimeType.GetTypeNameFromCodeTypeReference(this.codeDomEvent.Type, this.declaringType));
                if (type != null)
                {
                    CodeMemberMethod methodInfo = new CodeMemberMethod {
                        Name = "remove_" + this.name,
                        ReturnType = new CodeTypeReference(typeof(void))
                    };
                    methodInfo.Parameters.Add(new CodeParameterDeclarationExpression(type, "Handler"));
                    methodInfo.Attributes = this.memberAttributes;
                    this.removeMethod = new DesignTimeMethodInfo(this.declaringType, methodInfo, true);
                }
            }
            return this.removeMethod;
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }
            if (this.attributes == null)
            {
                this.attributes = Helper.LoadCustomAttributes(this.codeDomEvent.CustomAttributes, this.DeclaringType as DesignTimeType);
            }
            return Helper.IsDefined(attributeType, inherit, this.attributes, this);
        }

        public override EventAttributes Attributes
        {
            get
            {
                return EventAttributes.None;
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

        internal bool IsPublic
        {
            get
            {
                return ((this.memberAttributes & MemberAttributes.Public) != ((MemberAttributes) 0));
            }
        }

        internal bool IsStatic
        {
            get
            {
                return ((this.memberAttributes & MemberAttributes.Static) != ((MemberAttributes) 0));
            }
        }

        public override string Name
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.name;
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

