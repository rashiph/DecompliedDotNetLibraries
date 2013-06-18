namespace System.Activities.Runtime
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract]
    internal class FaultContext
    {
        internal FaultContext(System.Exception exception, ActivityInstanceReference sourceReference)
        {
            this.Exception = exception;
            this.Source = sourceReference;
        }

        [DataMember]
        public System.Exception Exception { get; private set; }

        [DataMember]
        public ActivityInstanceReference Source { get; private set; }
    }
}

