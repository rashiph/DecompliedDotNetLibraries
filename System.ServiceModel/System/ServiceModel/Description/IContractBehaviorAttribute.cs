namespace System.ServiceModel.Description
{
    using System;

    public interface IContractBehaviorAttribute
    {
        Type TargetContract { get; }
    }
}

