namespace System.ServiceModel.Activities
{
    using System;
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class CorrelationScope : NativeActivity
    {
        private Variable<CorrelationHandle> declaredHandle = new Variable<CorrelationHandle>();

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            metadata.AddChild(this.Body);
            metadata.SetImplementationVariablesCollection(new Collection<Variable> { this.declaredHandle });
            RuntimeArgument argument = new RuntimeArgument("CorrelatesWith", typeof(CorrelationHandle), ArgumentDirection.In);
            metadata.Bind(this.CorrelatesWith, argument);
            metadata.SetArgumentsCollection(new Collection<RuntimeArgument> { argument });
        }

        protected override void Execute(NativeActivityContext context)
        {
            if (this.Body != null)
            {
                CorrelationHandle property = null;
                if ((this.CorrelatesWith != null) && (this.CorrelatesWith.Expression != null))
                {
                    property = this.CorrelatesWith.Get(context);
                }
                if (property == null)
                {
                    property = this.declaredHandle.Get(context);
                }
                context.Properties.Add(CorrelationHandle.StaticExecutionPropertyName, property);
                context.ScheduleActivity(this.Body);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeCorrelatesWith()
        {
            return ((this.CorrelatesWith != null) && (this.CorrelatesWith.Expression != null));
        }

        public Activity Body { get; set; }

        public InArgument<CorrelationHandle> CorrelatesWith { get; set; }
    }
}

