namespace System.Activities.Statements
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    internal enum CompensationState
    {
        [EnumMember]
        Active = 1,
        [EnumMember]
        Canceled = 8,
        [EnumMember]
        Canceling = 7,
        [EnumMember]
        Compensated = 6,
        [EnumMember]
        Compensating = 5,
        [EnumMember]
        Completed = 2,
        [EnumMember]
        Confirmed = 4,
        [EnumMember]
        Confirming = 3,
        [EnumMember]
        Creating = 0
    }
}

