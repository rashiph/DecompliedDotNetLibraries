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
    public sealed class ClearCollection<T> : CodeActivity
    {
        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            RuntimeArgument argument = new RuntimeArgument("Collection", typeof(ICollection<T>), ArgumentDirection.In, true);
            metadata.Bind(this.Collection, argument);
            metadata.SetArgumentsCollection(new Collection<RuntimeArgument> { argument });
        }

        protected override void Execute(CodeActivityContext context)
        {
            ICollection<T> is2 = this.Collection.Get(context);
            if (is2 == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.CollectionActivityRequiresCollection(base.DisplayName)));
            }
            is2.Clear();
        }

        [DefaultValue((string) null), RequiredArgument]
        public InArgument<ICollection<T>> Collection { get; set; }
    }
}

