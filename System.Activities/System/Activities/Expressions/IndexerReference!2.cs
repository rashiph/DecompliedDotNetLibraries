namespace System.Activities.Expressions
{
    using System;
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime.Collections;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Windows.Markup;

    [ContentProperty("Indices")]
    public sealed class IndexerReference<TOperand, TItem> : CodeActivity<Location<TItem>>
    {
        private MethodInfo getMethod;
        private Collection<InArgument> indices;
        private MethodInfo setMethod;

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (typeof(TOperand).IsValueType)
            {
                metadata.AddValidationError(System.Activities.SR.TargetTypeIsValueType(base.GetType().Name, base.DisplayName));
            }
            if (this.Indices.Count == 0)
            {
                metadata.AddValidationError(System.Activities.SR.IndicesAreNeeded(base.GetType().Name, base.DisplayName));
            }
            else
            {
                IndexerHelper.CacheMethod<TOperand, TItem>(this.Indices, ref this.getMethod, ref this.setMethod);
                if (this.setMethod == null)
                {
                    metadata.AddValidationError(System.Activities.SR.SpecialMethodNotFound("set_Item", typeof(TOperand).Name));
                }
            }
            RuntimeArgument argument = new RuntimeArgument("Operand", typeof(TOperand), ArgumentDirection.In, true);
            metadata.Bind(this.Operand, argument);
            metadata.AddArgument(argument);
            IndexerHelper.OnGetArguments<TItem>(this.Indices, base.Result, metadata);
        }

        protected override Location<TItem> Execute(CodeActivityContext context)
        {
            object[] indices = new object[this.Indices.Count];
            for (int i = 0; i < this.Indices.Count; i++)
            {
                indices[i] = this.Indices[i].Get(context);
            }
            TOperand operand = this.Operand.Get(context);
            if (operand == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.MemberCannotBeNull("Operand", base.GetType().Name, base.DisplayName)));
            }
            return new IndexerLocation<TOperand, TItem>(operand, indices, this.getMethod, this.setMethod);
        }

        [DefaultValue((string) null), RequiredArgument]
        public Collection<InArgument> Indices
        {
            get
            {
                if (this.indices == null)
                {
                    ValidatingCollection<InArgument> validatings = new ValidatingCollection<InArgument> {
                        OnAddValidationCallback = delegate (InArgument item) {
                            if (item == null)
                            {
                                throw FxTrace.Exception.ArgumentNull("item");
                            }
                        }
                    };
                    this.indices = validatings;
                }
                return this.indices;
            }
        }

        [RequiredArgument, DefaultValue((string) null)]
        public InArgument<TOperand> Operand { get; set; }

        [DataContract]
        private class IndexerLocation : Location<TItem>
        {
            [DataMember(EmitDefaultValue=false)]
            private MethodInfo getMethod;
            [DataMember(EmitDefaultValue=false)]
            private object[] indices;
            [DataMember(EmitDefaultValue=false)]
            private TOperand operand;
            [DataMember(EmitDefaultValue=false)]
            private object[] parameters;
            [DataMember(EmitDefaultValue=false)]
            private MethodInfo setMethod;

            public IndexerLocation(TOperand operand, object[] indices, MethodInfo getMethod, MethodInfo setMethod)
            {
                this.operand = operand;
                this.indices = indices;
                this.getMethod = getMethod;
                this.setMethod = setMethod;
            }

            public override TItem Value
            {
                get
                {
                    if (this.getMethod == null)
                    {
                        throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.SpecialMethodNotFound("get_Item", typeof(TOperand).Name)));
                    }
                    return (TItem) this.getMethod.Invoke(this.operand, this.indices);
                }
                set
                {
                    if (this.parameters == null)
                    {
                        this.parameters = new object[this.indices.Length + 1];
                        for (int i = 0; i < this.indices.Length; i++)
                        {
                            this.parameters[i] = this.indices[i];
                        }
                        this.parameters[this.parameters.Length - 1] = value;
                    }
                    this.setMethod.Invoke(this.operand, this.parameters);
                }
            }
        }
    }
}

