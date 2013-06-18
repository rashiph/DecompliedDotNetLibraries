namespace System.IdentityModel.Tokens
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public enum SamlAccessDecision
    {
        [EnumMember]
        Deny = 1,
        [EnumMember]
        Indeterminate = 2,
        [EnumMember]
        Permit = 0
    }
}

