namespace System.Activities.Runtime
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    internal class DelegateCompletionCallbackWrapper : CompletionCallbackWrapper
    {
        private static Type[] callbackParameterTypes = new Type[] { typeof(NativeActivityContext), typeof(System.Activities.ActivityInstance), typeof(IDictionary<string, object>) };
        private static Type callbackType = typeof(DelegateCompletionCallback);
        [DataMember(EmitDefaultValue=false)]
        private Dictionary<string, object> results;

        public DelegateCompletionCallbackWrapper(DelegateCompletionCallback callback, System.Activities.ActivityInstance owningInstance) : base(callback, owningInstance)
        {
            base.NeedsToGatherOutputs = true;
        }

        protected override void GatherOutputs(System.Activities.ActivityInstance completedInstance)
        {
            if (completedInstance.Activity.HandlerOf != null)
            {
                IList<RuntimeDelegateArgument> runtimeDelegateArguments = completedInstance.Activity.HandlerOf.RuntimeDelegateArguments;
                LocationEnvironment environment = completedInstance.Environment;
                for (int i = 0; i < runtimeDelegateArguments.Count; i++)
                {
                    RuntimeDelegateArgument argument = runtimeDelegateArguments[i];
                    if ((argument.BoundArgument != null) && ArgumentDirectionHelper.IsOut(argument.Direction))
                    {
                        Location specificLocation = environment.GetSpecificLocation(argument.BoundArgument.Id);
                        if (specificLocation != null)
                        {
                            if (this.results == null)
                            {
                                this.results = new Dictionary<string, object>();
                            }
                            this.results.Add(argument.Name, specificLocation.Value);
                        }
                    }
                }
            }
        }

        protected internal override void Invoke(NativeActivityContext context, System.Activities.ActivityInstance completedInstance)
        {
            base.EnsureCallback(callbackType, callbackParameterTypes);
            DelegateCompletionCallback callback = (DelegateCompletionCallback) base.Callback;
            IDictionary<string, object> results = this.results;
            if (results == null)
            {
                results = ActivityUtilities.EmptyParameters;
            }
            callback(context, completedInstance, results);
        }
    }
}

