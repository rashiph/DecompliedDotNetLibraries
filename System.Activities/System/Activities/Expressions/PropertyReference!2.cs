namespace System.Activities.Expressions
{
    using System;
    using System.Activities;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    public sealed class PropertyReference<TOperand, TResult> : CodeActivity<Location<TResult>>
    {
        private PropertyInfo propertyInfo;

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
            if (string.IsNullOrEmpty(this.PropertyName))
            {
                metadata.AddValidationError(System.Activities.SR.ActivityPropertyMustBeSet("PropertyName", base.DisplayName));
            }
            else
            {
                this.propertyInfo = typeof(TOperand).GetProperty(this.PropertyName);
                if (this.propertyInfo == null)
                {
                    metadata.AddValidationError(System.Activities.SR.MemberNotFound(this.PropertyName, typeof(TOperand).Name));
                }
                else
                {
                    MethodInfo getMethod = this.propertyInfo.GetGetMethod();
                    MethodInfo setMethod = this.propertyInfo.GetSetMethod();
                    if ((setMethod == null) && !TypeHelper.AreTypesCompatible(this.propertyInfo.DeclaringType, typeof(System.Activities.Location)))
                    {
                        metadata.AddValidationError(System.Activities.SR.ReadonlyPropertyCannotBeSet(this.propertyInfo.DeclaringType, this.propertyInfo.Name));
                    }
                    if (((getMethod != null) && !getMethod.IsStatic) || ((setMethod != null) && !setMethod.IsStatic))
                    {
                        isRequired = true;
                    }
                }
            }
            MemberExpressionHelper.AddOperandArgument<TOperand>(metadata, this.Operand, isRequired);
        }

        protected override Location<TResult> Execute(CodeActivityContext context)
        {
            return new PropertyLocation<TOperand, TResult, TResult>(this.propertyInfo, this.Operand.Get(context));
        }

        public InArgument<TOperand> Operand { get; set; }

        [DefaultValue((string) null)]
        public string PropertyName { get; set; }

        [DataContract]
        private class PropertyLocation<T> : Location<T>
        {
            [DataMember(EmitDefaultValue=false)]
            private object owner;
            [DataMember]
            private PropertyInfo propertyInfo;

            public PropertyLocation(PropertyInfo propertyInfo, object owner)
            {
                this.propertyInfo = propertyInfo;
                this.owner = owner;
            }

            public override T Value
            {
                get
                {
                    if ((this.propertyInfo.GetGetMethod() == null) && !TypeHelper.AreTypesCompatible(this.propertyInfo.DeclaringType, typeof(System.Activities.Location)))
                    {
                        throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.WriteonlyPropertyCannotBeRead(this.propertyInfo.DeclaringType, this.propertyInfo.Name)));
                    }
                    return (T) this.propertyInfo.GetValue(this.owner, null);
                }
                set
                {
                    this.propertyInfo.SetValue(this.owner, value, null);
                }
            }
        }
    }
}

