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
    public sealed class AddToCollection<T> : CodeActivity
    {
        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            Collection<RuntimeArgument> arguments = new Collection<RuntimeArgument>();
            RuntimeArgument argument = new RuntimeArgument("Collection", typeof(ICollection<T>), ArgumentDirection.In, true);
            metadata.Bind(this.Collection, argument);
            arguments.Add(argument);
            RuntimeArgument argument2 = new RuntimeArgument("Item", typeof(T), ArgumentDirection.In, true);
            metadata.Bind(this.Item, argument2);
            arguments.Add(argument2);
            metadata.SetArgumentsCollection(arguments);
        }

        protected override void Execute(CodeActivityContext context)
        {
            ICollection<T> is2 = this.Collection.Get(context);
            if (is2 == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.CollectionActivityRequiresCollection(base.DisplayName)));
            }
            T item = this.Item.Get(context);
            is2.Add(item);
        }

        [RequiredArgument, DefaultValue((string) null)]
        public InArgument<ICollection<T>> Collection { get; set; }

        [RequiredArgument, DefaultValue((string) null)]
        public InArgument<T> Item { get; set; }
    }
}

