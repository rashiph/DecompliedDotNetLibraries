namespace System.ServiceModel
{
    using System;

    internal static class ServiceModelAttributeTargets
    {
        public const AttributeTargets CallbackBehavior = AttributeTargets.Class;
        public const AttributeTargets ClientBehavior = AttributeTargets.Interface;
        public const AttributeTargets ContractBehavior = (AttributeTargets.Interface | AttributeTargets.Class);
        public const AttributeTargets MessageContract = (AttributeTargets.Struct | AttributeTargets.Class);
        public const AttributeTargets MessageMember = (AttributeTargets.Field | AttributeTargets.Property);
        public const AttributeTargets OperationBehavior = AttributeTargets.Method;
        public const AttributeTargets OperationContract = AttributeTargets.Method;
        public const AttributeTargets Parameter = (AttributeTargets.ReturnValue | AttributeTargets.Parameter);
        public const AttributeTargets ServiceBehavior = AttributeTargets.Class;
        public const AttributeTargets ServiceContract = (AttributeTargets.Interface | AttributeTargets.Class);
    }
}

