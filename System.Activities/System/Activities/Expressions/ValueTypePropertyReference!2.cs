namespace System.Activities.Expressions
{
    using System;
    using System.Activities;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    public sealed class ValueTypePropertyReference<TOperand, TResult> : CodeActivity<Location<TResult>>
    {
        private PropertyInfo propertyInfo;

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (!typeof(TOperand).IsValueType)
            {
                metadata.AddValidationError(System.Activities.SR.TypeMustbeValueType(typeof(TOperand).Name));
            }
            if (typeof(TOperand).IsEnum)
            {
                metadata.AddValidationError(System.Activities.SR.TargetTypeCannotBeEnum(base.GetType().Name, base.DisplayName));
            }
            else if (string.IsNullOrEmpty(this.PropertyName))
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
            }
            bool isRequired = false;
            if (this.propertyInfo != null)
            {
                MethodInfo setMethod = this.propertyInfo.GetSetMethod();
                if (setMethod == null)
                {
                    metadata.AddValidationError(System.Activities.SR.MemberIsReadOnly(this.propertyInfo.Name, typeof(TOperand)));
                }
                if ((setMethod != null) && !setMethod.IsStatic)
                {
                    isRequired = true;
                }
            }
            MemberExpressionHelper.AddOperandLocationArgument<TOperand>(metadata, this.OperandLocation, isRequired);
        }

        protected override Location<TResult> Execute(CodeActivityContext context)
        {
            return new PropertyLocation<TOperand, TResult>(this.propertyInfo, this.OperandLocation.GetLocation(context));
        }

        [DefaultValue((string) null)]
        public InOutArgument<TOperand> OperandLocation { get; set; }

        [DefaultValue((string) null)]
        public string PropertyName { get; set; }

        [DataContract]
        private class PropertyLocation : Location<TResult>
        {
            [DataMember(EmitDefaultValue=false)]
            private Location<TOperand> ownerLocation;
            [DataMember]
            private PropertyInfo propertyInfo;

            public PropertyLocation(PropertyInfo propertyInfo, Location<TOperand> ownerLocation)
            {
                this.propertyInfo = propertyInfo;
                this.ownerLocation = ownerLocation;
            }

            public override TResult Value
            {
                get
                {
                    if ((this.propertyInfo.GetGetMethod() == null) && !TypeHelper.AreTypesCompatible(this.propertyInfo.DeclaringType, typeof(System.Activities.Location)))
                    {
                        throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.WriteonlyPropertyCannotBeRead(this.propertyInfo.DeclaringType, this.propertyInfo.Name)));
                    }
                    return (TResult) this.propertyInfo.GetValue(this.ownerLocation.Value, null);
                }
                set
                {
                    object obj2 = this.ownerLocation.Value;
                    this.propertyInfo.SetValue(obj2, value, null);
                    this.ownerLocation.Value = (TOperand) obj2;
                }
            }
        }
    }
}

