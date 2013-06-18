namespace System.Activities.Runtime
{
    using System;
    using System.Activities;
    using System.Runtime.Serialization;

    [DataContract]
    internal class ActivityCompletionCallbackWrapper : CompletionCallbackWrapper
    {
        private static Type[] completionCallbackParameters = new Type[] { typeof(NativeActivityContext), typeof(System.Activities.ActivityInstance) };
        private static Type completionCallbackType = typeof(CompletionCallback);

        public ActivityCompletionCallbackWrapper(CompletionCallback callback, System.Activities.ActivityInstance owningInstance) : base(callback, owningInstance)
        {
        }

        protected internal override void Invoke(NativeActivityContext context, System.Activities.ActivityInstance completedInstance)
        {
            base.EnsureCallback(completionCallbackType, completionCallbackParameters);
            CompletionCallback callback = (CompletionCallback) base.Callback;
            callback(context, completedInstance);
        }
    }
}

