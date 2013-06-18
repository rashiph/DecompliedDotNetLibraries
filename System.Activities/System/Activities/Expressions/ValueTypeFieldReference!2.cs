namespace System.Activities.Expressions
{
    using System;
    using System.Activities;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    public sealed class ValueTypeFieldReference<TOperand, TResult> : CodeActivity<Location<TResult>>
    {
        private FieldInfo fieldInfo;

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            bool isRequired = false;
            if (!typeof(TOperand).IsValueType)
            {
                metadata.AddValidationError(System.Activities.SR.TypeMustbeValueType(typeof(TOperand).Name));
            }
            if (typeof(TOperand).IsEnum)
            {
                metadata.AddValidationError(System.Activities.SR.TargetTypeCannotBeEnum(base.GetType().Name, base.DisplayName));
            }
            if (string.IsNullOrEmpty(this.FieldName))
            {
                metadata.AddValidationError(System.Activities.SR.ActivityPropertyMustBeSet("FieldName", base.DisplayName));
            }
            else
            {
                this.fieldInfo = typeof(TOperand).GetField(this.FieldName);
                isRequired = (this.fieldInfo != null) && !this.fieldInfo.IsStatic;
                if (this.fieldInfo == null)
                {
                    metadata.AddValidationError(System.Activities.SR.MemberNotFound(this.FieldName, typeof(TOperand).Name));
                }
                else if (this.fieldInfo.IsInitOnly)
                {
                    metadata.AddValidationError(System.Activities.SR.MemberIsReadOnly(this.FieldName, typeof(TOperand).Name));
                }
            }
            MemberExpressionHelper.AddOperandLocationArgument<TOperand>(metadata, this.OperandLocation, isRequired);
        }

        protected override Location<TResult> Execute(CodeActivityContext context)
        {
            return new FieldLocation<TOperand, TResult>(this.fieldInfo, this.OperandLocation.GetLocation(context));
        }

        [DefaultValue((string) null)]
        public string FieldName { get; set; }

        [DefaultValue((string) null)]
        public InOutArgument<TOperand> OperandLocation { get; set; }

        [DataContract]
        private class FieldLocation : Location<TResult>
        {
            [DataMember]
            private FieldInfo fieldInfo;
            [DataMember(EmitDefaultValue=false)]
            private Location<TOperand> ownerLocation;

            public FieldLocation(FieldInfo fieldInfo, Location<TOperand> ownerLocation)
            {
                this.fieldInfo = fieldInfo;
                this.ownerLocation = ownerLocation;
            }

            public override TResult Value
            {
                get
                {
                    return (TResult) this.fieldInfo.GetValue(this.ownerLocation.Value);
                }
                set
                {
                    object obj2 = this.ownerLocation.Value;
                    this.fieldInfo.SetValue(obj2, value);
                    this.ownerLocation.Value = (TOperand) obj2;
                }
            }
        }
    }
}

