namespace System.ServiceModel.Activities
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.Collections;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;

    [ContentProperty("CorrelationData")]
    public sealed class InitializeCorrelation : NativeActivity
    {
        public InitializeCorrelation()
        {
            this.CorrelationData = new OrderedDictionary<string, InArgument<string>>();
        }

        protected override void Execute(NativeActivityContext context)
        {
            CorrelationHandle handle = (this.Correlation == null) ? null : this.Correlation.Get(context);
            if (handle == null)
            {
                handle = context.Properties.Find(CorrelationHandle.StaticExecutionPropertyName) as CorrelationHandle;
                if (handle == null)
                {
                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.NullCorrelationHandleInInitializeCorrelation(base.DisplayName)));
                }
            }
            CorrelationExtension extension = context.GetExtension<CorrelationExtension>();
            if (extension == null)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.InitializeCorrelationRequiresWorkflowServiceHost(base.DisplayName)));
            }
            Dictionary<string, string> keyData = new Dictionary<string, string>();
            foreach (KeyValuePair<string, InArgument<string>> pair in this.CorrelationData)
            {
                keyData.Add(pair.Key, pair.Value.Get(context));
            }
            handle.InitializeBookmarkScope(context, extension.GenerateKey(keyData));
        }

        [DefaultValue((string) null)]
        public InArgument<CorrelationHandle> Correlation { get; set; }

        public IDictionary<string, InArgument<string>> CorrelationData { get; private set; }
    }
}

