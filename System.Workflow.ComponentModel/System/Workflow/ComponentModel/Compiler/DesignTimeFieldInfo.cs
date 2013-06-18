namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.CodeDom;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;

    internal sealed class DesignTimeFieldInfo : FieldInfo
    {
        private Attribute[] attributes;
        private CodeMemberField codeDomField;
        private DesignTimeType declaringType;
        private FieldAttributes fieldAttributes;

        internal DesignTimeFieldInfo(DesignTimeType declaringType, CodeMemberField codeDomField)
        {
            if (declaringType == null)
            {
                throw new ArgumentNullException("Declaring Type");
            }
            if (codeDomField == null)
            {
                throw new ArgumentNullException("codeDomEvent");
            }
            this.declaringType = declaringType;
            this.codeDomField = codeDomField;
            this.fieldAttributes = Helper.ConvertToFieldAttributes(codeDomField.Attributes);
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
                this.attributes = Helper.LoadCustomAttributes(this.codeDomField.CustomAttributes, this.DeclaringType as DesignTimeType);
            }
            return Helper.GetCustomAttributes(attributeType, inherit, this.attributes, this);
        }

        public override object GetValue(object obj)
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
                this.attributes = Helper.LoadCustomAttributes(this.codeDomField.CustomAttributes, this.DeclaringType as DesignTimeType);
            }
            return Helper.IsDefined(attributeType, inherit, this.attributes, this);
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture)
        {
            throw new NotImplementedException(TypeSystemSR.GetString("Error_RuntimeNotSupported"));
        }

        public override FieldAttributes Attributes
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.fieldAttributes;
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

        public override RuntimeFieldHandle FieldHandle
        {
            get
            {
                throw new NotImplementedException(TypeSystemSR.GetString("Error_RuntimeNotSupported"));
            }
        }

        public override Type FieldType
        {
            get
            {
                return this.declaringType.ResolveType(DesignTimeType.GetTypeNameFromCodeTypeReference(this.codeDomField.Type, this.declaringType));
            }
        }

        public override string Name
        {
            get
            {
                return Helper.EnsureTypeName(this.codeDomField.Name);
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

