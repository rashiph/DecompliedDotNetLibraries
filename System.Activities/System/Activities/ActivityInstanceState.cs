namespace System.Activities
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public enum ActivityInstanceState
    {
        [EnumMember]
        Canceled = 2,
        [EnumMember]
        Closed = 1,
        [EnumMember]
        Executing = 0,
        [EnumMember]
        Faulted = 3
    }
}

