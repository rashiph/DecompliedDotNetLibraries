namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;

    [ContentProperty("Collection")]
    public sealed class RemoveFromCollection<T> : CodeActivity<bool>
    {
        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            RuntimeArgument argument = new RuntimeArgument("Collection", typeof(ICollection<T>), ArgumentDirection.In, true);
            metadata.Bind(this.Collection, argument);
            RuntimeArgument argument2 = new RuntimeArgument("Item", typeof(T), ArgumentDirection.In, true);
            metadata.Bind(this.Item, argument2);
            RuntimeArgument argument3 = new RuntimeArgument("Result", typeof(bool), ArgumentDirection.Out);
            metadata.Bind(base.Result, argument3);
            metadata.SetArgumentsCollection(new Collection<RuntimeArgument> { argument, argument2, argument3 });
        }

        protected override bool Execute(CodeActivityContext context)
        {
            ICollection<T> is2 = this.Collection.Get(context);
            if (is2 == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.CollectionActivityRequiresCollection(base.DisplayName)));
            }
            T item = this.Item.Get(context);
            return is2.Remove(item);
        }

        [DefaultValue((string) null), RequiredArgument]
        public InArgument<ICollection<T>> Collection { get; set; }

        [RequiredArgument, DefaultValue((string) null)]
        public InArgument<T> Item { get; set; }
    }
}

