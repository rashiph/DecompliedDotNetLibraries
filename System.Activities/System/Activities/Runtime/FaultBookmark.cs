namespace System.Activities.Runtime
{
    using System;
    using System.Activities;
    using System.Runtime.Serialization;

    [DataContract]
    internal class FaultBookmark
    {
        [DataMember]
        private FaultCallbackWrapper callbackWrapper;

        public FaultBookmark(FaultCallbackWrapper callbackWrapper)
        {
            this.callbackWrapper = callbackWrapper;
        }

        public System.Activities.Runtime.WorkItem GenerateWorkItem(Exception propagatedException, System.Activities.ActivityInstance propagatedFrom, ActivityInstanceReference originalExceptionSource)
        {
            return this.callbackWrapper.CreateWorkItem(propagatedException, propagatedFrom, originalExceptionSource);
        }
    }
}

