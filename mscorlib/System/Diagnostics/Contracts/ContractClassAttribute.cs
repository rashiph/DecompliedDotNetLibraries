namespace System.Diagnostics.Contracts
{
    using System;
    using System.Diagnostics;

    [Conditional("CONTRACTS_FULL"), Conditional("DEBUG"), AttributeUsage(AttributeTargets.Delegate | AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
    public sealed class ContractClassAttribute : Attribute
    {
        private Type _typeWithContracts;

        public ContractClassAttribute(Type typeContainingContracts)
        {
            this._typeWithContracts = typeContainingContracts;
        }

        public Type TypeContainingContracts
        {
            get
            {
                return this._typeWithContracts;
            }
        }
    }
}

