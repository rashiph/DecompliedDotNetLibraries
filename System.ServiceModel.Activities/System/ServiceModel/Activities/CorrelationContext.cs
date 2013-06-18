namespace System.ServiceModel.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract]
    internal class CorrelationContext
    {
        [DataMember]
        public IDictionary<string, string> Context { get; set; }
    }
}

