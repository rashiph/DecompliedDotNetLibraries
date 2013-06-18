namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;

    [ContentProperty("Body")]
    public sealed class HandleScope<THandle> : NativeActivity where THandle: System.Activities.Handle
    {
        private Variable<THandle> declaredHandle;

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            RuntimeArgument argument = new RuntimeArgument("Handle", typeof(THandle), ArgumentDirection.In);
            metadata.Bind(this.Handle, argument);
            metadata.SetArgumentsCollection(new Collection<RuntimeArgument> { argument });
            if (this.Body != null)
            {
                metadata.SetChildrenCollection(new Collection<Activity> { this.Body });
            }
            Collection<Variable> collection = null;
            if ((this.Handle == null) || this.Handle.IsEmpty)
            {
                if (this.declaredHandle == null)
                {
                    this.declaredHandle = new Variable<THandle>();
                }
            }
            else
            {
                this.declaredHandle = null;
            }
            if (this.declaredHandle != null)
            {
                ActivityUtilities.Add<Variable>(ref collection, this.declaredHandle);
            }
            metadata.SetImplementationVariablesCollection(collection);
        }

        protected override void Execute(NativeActivityContext context)
        {
            System.Activities.Handle property = null;
            if ((this.Handle == null) || this.Handle.IsEmpty)
            {
                property = this.declaredHandle.Get(context);
            }
            else
            {
                property = this.Handle.Get(context);
            }
            if (property == null)
            {
                throw FxTrace.Exception.ArgumentNull("Handle");
            }
            context.Properties.Add(property.ExecutionPropertyName, property);
            if (this.Body != null)
            {
                context.ScheduleActivity(this.Body);
            }
        }

        public Activity Body { get; set; }

        public InArgument<THandle> Handle { get; set; }
    }
}

