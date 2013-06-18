namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract]
    internal class TriggerCompletedEvent
    {
        [DataMember]
        public System.Activities.Bookmark Bookmark { get; set; }

        [DataMember]
        public int TriggedId { get; set; }
    }
}

