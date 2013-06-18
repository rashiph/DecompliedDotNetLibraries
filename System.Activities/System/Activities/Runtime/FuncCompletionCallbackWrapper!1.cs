namespace System.Activities.Runtime
{
    using System;
    using System.Activities;
    using System.Runtime;
    using System.Runtime.Serialization;

    [DataContract]
    internal class FuncCompletionCallbackWrapper<T> : CompletionCallbackWrapper
    {
        private static Type[] callbackParameterTypes;
        private static Type callbackType;
        [DataMember(EmitDefaultValue=false)]
        private T resultValue;

        static FuncCompletionCallbackWrapper()
        {
            FuncCompletionCallbackWrapper<T>.callbackType = typeof(CompletionCallback<T>);
            FuncCompletionCallbackWrapper<T>.callbackParameterTypes = new Type[] { typeof(NativeActivityContext), typeof(System.Activities.ActivityInstance), typeof(T) };
        }

        public FuncCompletionCallbackWrapper(CompletionCallback<T> callback, System.Activities.ActivityInstance owningInstance) : base(callback, owningInstance)
        {
            base.NeedsToGatherOutputs = true;
        }

        protected override void GatherOutputs(System.Activities.ActivityInstance completedInstance)
        {
            int id = -1;
            if (completedInstance.Activity.HandlerOf != null)
            {
                DelegateOutArgument resultArgument = completedInstance.Activity.HandlerOf.GetResultArgument();
                if (resultArgument != null)
                {
                    id = resultArgument.Id;
                }
                else
                {
                    ActivityWithResult activity = completedInstance.Activity as ActivityWithResult;
                    if ((activity != null) && TypeHelper.AreTypesCompatible(activity.ResultType, typeof(T)))
                    {
                        id = this.GetResultId(activity);
                    }
                }
            }
            else
            {
                id = this.GetResultId((ActivityWithResult) completedInstance.Activity);
            }
            if (id >= 0)
            {
                System.Activities.Location specificLocation = completedInstance.Environment.GetSpecificLocation(id);
                Location<T> location2 = specificLocation as Location<T>;
                if (location2 != null)
                {
                    this.resultValue = location2.Value;
                }
                else if (specificLocation != null)
                {
                    this.resultValue = TypeHelper.Convert<T>(specificLocation.Value);
                }
            }
        }

        private int GetResultId(ActivityWithResult activity)
        {
            if (activity.Result != null)
            {
                return activity.Result.Id;
            }
            for (int i = 0; i < activity.RuntimeArguments.Count; i++)
            {
                RuntimeArgument argument = activity.RuntimeArguments[i];
                if (argument.IsResult)
                {
                    return argument.Id;
                }
            }
            return -1;
        }

        protected internal override void Invoke(NativeActivityContext context, System.Activities.ActivityInstance completedInstance)
        {
            base.EnsureCallback(FuncCompletionCallbackWrapper<T>.callbackType, FuncCompletionCallbackWrapper<T>.callbackParameterTypes, FuncCompletionCallbackWrapper<T>.callbackParameterTypes[2]);
            CompletionCallback<T> callback = (CompletionCallback<T>) base.Callback;
            callback(context, completedInstance, this.resultValue);
        }

        protected override void OnSerializingGenericCallback()
        {
            base.ValidateCallbackResolution(FuncCompletionCallbackWrapper<T>.callbackType, FuncCompletionCallbackWrapper<T>.callbackParameterTypes, FuncCompletionCallbackWrapper<T>.callbackParameterTypes[2]);
        }
    }
}

