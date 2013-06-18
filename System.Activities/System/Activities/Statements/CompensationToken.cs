namespace System.Activities.Statements
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class CompensationToken
    {
        internal const string PropertyName = "System.Compensation.CompensationToken";
        internal const long RootCompensationId = 0L;

        internal CompensationToken(CompensationTokenData tokenData)
        {
            this.CompensationId = tokenData.CompensationId;
        }

        [DataMember(EmitDefaultValue=false)]
        internal bool CompensateCalled { get; set; }

        [DataMember(EmitDefaultValue=false)]
        internal long CompensationId { get; private set; }

        [DataMember(EmitDefaultValue=false)]
        internal bool ConfirmCalled { get; set; }
    }
}

