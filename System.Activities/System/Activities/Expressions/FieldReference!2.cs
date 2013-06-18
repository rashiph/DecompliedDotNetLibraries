namespace System.Activities.Expressions
{
    using System;
    using System.Activities;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    public sealed class FieldReference<TOperand, TResult> : CodeActivity<Location<TResult>>
    {
        private FieldInfo fieldInfo;

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            bool isRequired = false;
            if (typeof(TOperand).IsEnum)
            {
                metadata.AddValidationError(System.Activities.SR.TargetTypeCannotBeEnum(base.GetType().Name, base.DisplayName));
            }
            else if (typeof(TOperand).IsValueType)
            {
                metadata.AddValidationError(System.Activities.SR.TargetTypeIsValueType(base.GetType().Name, base.DisplayName));
            }
            if (string.IsNullOrEmpty(this.FieldName))
            {
                metadata.AddValidationError(System.Activities.SR.ActivityPropertyMustBeSet("FieldName", base.DisplayName));
            }
            else
            {
                this.fieldInfo = typeof(TOperand).GetField(this.FieldName);
                if (this.fieldInfo == null)
                {
                    metadata.AddValidationError(System.Activities.SR.MemberNotFound(this.FieldName, typeof(TOperand).Name));
                }
                else
                {
                    if (this.fieldInfo.IsInitOnly)
                    {
                        metadata.AddValidationError(System.Activities.SR.MemberIsReadOnly(this.FieldName, typeof(TOperand).Name));
                    }
                    isRequired = !this.fieldInfo.IsStatic;
                }
            }
            MemberExpressionHelper.AddOperandArgument<TOperand>(metadata, this.Operand, isRequired);
        }

        protected override Location<TResult> Execute(CodeActivityContext context)
        {
            return new FieldLocation<TOperand, TResult>(this.fieldInfo, this.Operand.Get(context));
        }

        [DefaultValue((string) null)]
        public string FieldName { get; set; }

        [DefaultValue((string) null)]
        public InArgument<TOperand> Operand { get; set; }

        [DataContract]
        private class FieldLocation : Location<TResult>
        {
            [DataMember]
            private FieldInfo fieldInfo;
            [DataMember(EmitDefaultValue=false)]
            private object owner;

            public FieldLocation(FieldInfo fieldInfo, object owner)
            {
                this.fieldInfo = fieldInfo;
                this.owner = owner;
            }

            public override TResult Value
            {
                get
                {
                    return (TResult) this.fieldInfo.GetValue(this.owner);
                }
                set
                {
                    this.fieldInfo.SetValue(this.owner, value);
                }
            }
        }
    }
}

