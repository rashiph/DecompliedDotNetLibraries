namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;

    [ContentProperty("Exception")]
    public sealed class Throw : CodeActivity
    {
        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            RuntimeArgument argument = new RuntimeArgument("Exception", typeof(System.Exception), ArgumentDirection.In, true);
            metadata.Bind(this.Exception, argument);
            metadata.SetArgumentsCollection(new Collection<RuntimeArgument> { argument });
        }

        protected override void Execute(CodeActivityContext context)
        {
            System.Exception exception = this.Exception.Get(context);
            throw FxTrace.Exception.AsError(exception);
        }

        [RequiredArgument, DefaultValue((string) null)]
        public InArgument<System.Exception> Exception { get; set; }
    }
}

