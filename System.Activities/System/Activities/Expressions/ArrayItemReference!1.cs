namespace System.Activities.Expressions
{
    using System;
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    public sealed class ArrayItemReference<TItem> : CodeActivity<Location<TItem>>
    {
        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            RuntimeArgument argument = new RuntimeArgument("Array", typeof(TItem[]), ArgumentDirection.In, true);
            metadata.Bind(this.Array, argument);
            RuntimeArgument argument2 = new RuntimeArgument("Index", typeof(int), ArgumentDirection.In, true);
            metadata.Bind(this.Index, argument2);
            RuntimeArgument argument3 = new RuntimeArgument("Result", typeof(Location<TItem>), ArgumentDirection.Out);
            metadata.Bind(base.Result, argument3);
            metadata.SetArgumentsCollection(new Collection<RuntimeArgument> { argument, argument2, argument3 });
        }

        protected override Location<TItem> Execute(CodeActivityContext context)
        {
            TItem[] array = this.Array.Get(context);
            if (array == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.MemberCannotBeNull("Array", base.GetType().Name, base.DisplayName)));
            }
            return new ArrayLocation<TItem>(array, this.Index.Get(context));
        }

        [RequiredArgument, DefaultValue((string) null)]
        public InArgument<TItem[]> Array { get; set; }

        [RequiredArgument, DefaultValue((string) null)]
        public InArgument<int> Index { get; set; }

        [DataContract]
        private class ArrayLocation : Location<TItem>
        {
            [DataMember]
            private TItem[] array;
            [DataMember(EmitDefaultValue=false)]
            private int index;

            public ArrayLocation(TItem[] array, int index)
            {
                this.array = array;
                this.index = index;
            }

            public override TItem Value
            {
                get
                {
                    return this.array[this.index];
                }
                set
                {
                    this.array[this.index] = value;
                }
            }
        }
    }
}

