namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public sealed class Assign<T> : CodeActivity
    {
        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            Collection<RuntimeArgument> arguments = new Collection<RuntimeArgument>();
            RuntimeArgument argument = new RuntimeArgument("Value", typeof(T), ArgumentDirection.In, true);
            metadata.Bind(this.Value, argument);
            RuntimeArgument argument2 = new RuntimeArgument("To", typeof(T), ArgumentDirection.Out, true);
            metadata.Bind(this.To, argument2);
            arguments.Add(argument);
            arguments.Add(argument2);
            metadata.SetArgumentsCollection(arguments);
        }

        protected override void Execute(CodeActivityContext context)
        {
            context.SetValue<T>(this.To, this.Value.Get(context));
        }

        [DefaultValue((string) null), RequiredArgument]
        public OutArgument<T> To { get; set; }

        [DefaultValue((string) null), RequiredArgument]
        public InArgument<T> Value { get; set; }
    }
}

