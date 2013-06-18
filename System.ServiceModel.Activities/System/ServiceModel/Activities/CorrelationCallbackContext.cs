namespace System.ServiceModel.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    [DataContract]
    internal class CorrelationCallbackContext
    {
        [DataMember]
        public IDictionary<string, string> Context { get; set; }

        [DataMember]
        public EndpointAddress10 ListenAddress { get; set; }
    }
}

