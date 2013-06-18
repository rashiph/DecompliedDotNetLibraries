namespace System.Activities.Expressions
{
    using System;
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime;
    using System.Runtime.Collections;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Windows.Markup;

    [ContentProperty("Indices")]
    public sealed class MultidimensionalArrayItemReference<TItem> : CodeActivity<Location<TItem>>
    {
        private Collection<InArgument<int>> indices;

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (this.Indices.Count == 0)
            {
                metadata.AddValidationError(System.Activities.SR.IndicesAreNeeded(base.GetType().Name, base.DisplayName));
            }
            RuntimeArgument argument = new RuntimeArgument("Array", typeof(System.Array), ArgumentDirection.In, true);
            metadata.Bind(this.Array, argument);
            metadata.AddArgument(argument);
            for (int i = 0; i < this.Indices.Count; i++)
            {
                RuntimeArgument argument2 = new RuntimeArgument("Index_" + i, typeof(int), ArgumentDirection.In, true);
                metadata.Bind(this.Indices[i], argument2);
                metadata.AddArgument(argument2);
            }
            RuntimeArgument argument3 = new RuntimeArgument("Result", typeof(Location<TItem>), ArgumentDirection.Out);
            metadata.Bind(base.Result, argument3);
            metadata.AddArgument(argument3);
        }

        protected override Location<TItem> Execute(CodeActivityContext context)
        {
            System.Array array = this.Array.Get(context);
            if (array == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.MemberCannotBeNull("Array", base.GetType().Name, base.DisplayName)));
            }
            Type elementType = array.GetType().GetElementType();
            if (!TypeHelper.AreTypesCompatible(typeof(TItem), elementType))
            {
                throw FxTrace.Exception.AsError(new InvalidCastException(System.Activities.SR.IncompatibleTypeForMultidimensionalArrayItemReference(typeof(TItem).Name, elementType.Name)));
            }
            int[] indices = new int[this.Indices.Count];
            for (int i = 0; i < this.Indices.Count; i++)
            {
                indices[i] = this.Indices[i].Get(context);
            }
            return new MultidimensionArrayLocation<TItem>(array, indices);
        }

        [RequiredArgument, DefaultValue((string) null)]
        public InArgument<System.Array> Array { get; set; }

        [DefaultValue((string) null)]
        public Collection<InArgument<int>> Indices
        {
            get
            {
                if (this.indices == null)
                {
                    ValidatingCollection<InArgument<int>> validatings = new ValidatingCollection<InArgument<int>> {
                        OnAddValidationCallback = delegate (InArgument<int> item) {
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

        [DataContract]
        private class MultidimensionArrayLocation : Location<TItem>
        {
            [DataMember]
            private Array array;
            [DataMember(EmitDefaultValue=false)]
            private int[] indices;

            public MultidimensionArrayLocation(Array array, int[] indices)
            {
                this.array = array;
                this.indices = indices;
            }

            public override TItem Value
            {
                get
                {
                    return (TItem) this.array.GetValue(this.indices);
                }
                set
                {
                    this.array.SetValue(value, this.indices);
                }
            }
        }
    }
}

