namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime;
    using System.Runtime.CompilerServices;

    public sealed class Assign : CodeActivity
    {
        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            Collection<RuntimeArgument> arguments = new Collection<RuntimeArgument>();
            Type objectType = TypeHelper.ObjectType;
            if (this.Value != null)
            {
                objectType = this.Value.ArgumentType;
            }
            RuntimeArgument argument = new RuntimeArgument("Value", objectType, ArgumentDirection.In, true);
            metadata.Bind(this.Value, argument);
            Type argumentType = TypeHelper.ObjectType;
            if (this.To != null)
            {
                argumentType = this.To.ArgumentType;
            }
            RuntimeArgument argument2 = new RuntimeArgument("To", argumentType, ArgumentDirection.Out, true);
            metadata.Bind(this.To, argument2);
            arguments.Add(argument);
            arguments.Add(argument2);
            metadata.SetArgumentsCollection(arguments);
            if (((this.Value != null) && (this.To != null)) && !TypeHelper.AreTypesCompatible(this.Value.ArgumentType, this.To.ArgumentType))
            {
                metadata.AddValidationError(System.Activities.SR.TypeMismatchForAssign(this.Value.ArgumentType, this.To.ArgumentType, base.DisplayName));
            }
        }

        protected override void Execute(CodeActivityContext context)
        {
            this.To.Set(context, this.Value.Get(context));
        }

        [DefaultValue((string) null), RequiredArgument]
        public OutArgument To { get; set; }

        [RequiredArgument, DefaultValue((string) null)]
        public InArgument Value { get; set; }
    }
}

