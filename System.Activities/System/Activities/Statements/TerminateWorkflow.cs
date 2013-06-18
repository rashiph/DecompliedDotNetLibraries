namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public sealed class TerminateWorkflow : NativeActivity
    {
        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            Collection<RuntimeArgument> arguments = new Collection<RuntimeArgument>();
            RuntimeArgument argument = new RuntimeArgument("Reason", typeof(string), ArgumentDirection.In, false);
            metadata.Bind(this.Reason, argument);
            RuntimeArgument argument2 = new RuntimeArgument("Exception", typeof(System.Exception), ArgumentDirection.In, false);
            metadata.Bind(this.Exception, argument2);
            arguments.Add(argument);
            arguments.Add(argument2);
            metadata.SetArgumentsCollection(arguments);
            if (((this.Reason == null) || this.Reason.IsEmpty) && ((this.Exception == null) || this.Exception.IsEmpty))
            {
                metadata.AddValidationError(System.Activities.SR.OneOfTwoPropertiesMustBeSet("Reason", "Exception", "TerminateWorkflow", base.DisplayName));
            }
        }

        protected override void Execute(NativeActivityContext context)
        {
            string str = this.Reason.Get(context);
            System.Exception innerException = this.Exception.Get(context);
            if (!string.IsNullOrEmpty(str))
            {
                context.Terminate(new WorkflowTerminatedException(str, innerException));
            }
            else if (innerException != null)
            {
                context.Terminate(innerException);
            }
            else
            {
                context.Terminate(new WorkflowTerminatedException());
            }
        }

        [DefaultValue((string) null)]
        public InArgument<System.Exception> Exception { get; set; }

        [DefaultValue((string) null)]
        public InArgument<string> Reason { get; set; }
    }
}

