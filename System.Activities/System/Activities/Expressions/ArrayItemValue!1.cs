namespace System.Activities.Expressions
{
    using System;
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public sealed class ArrayItemValue<TItem> : CodeActivity<TItem>
    {
        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            RuntimeArgument argument = new RuntimeArgument("Array", typeof(TItem[]), ArgumentDirection.In, true);
            metadata.Bind(this.Array, argument);
            RuntimeArgument argument2 = new RuntimeArgument("Index", typeof(int), ArgumentDirection.In, true);
            metadata.Bind(this.Index, argument2);
            metadata.SetArgumentsCollection(new Collection<RuntimeArgument> { argument, argument2 });
        }

        protected override TItem Execute(CodeActivityContext context)
        {
            TItem[] localArray = this.Array.Get(context);
            if (localArray == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.MemberCannotBeNull("Array", base.GetType().Name, base.DisplayName)));
            }
            int index = this.Index.Get(context);
            return localArray[index];
        }

        [DefaultValue((string) null), RequiredArgument]
        public InArgument<TItem[]> Array { get; set; }

        [RequiredArgument, DefaultValue((string) null)]
        public InArgument<int> Index { get; set; }
    }
}

