namespace System.Diagnostics.Contracts
{
    using System;
    using System.Diagnostics;

    [Conditional("CONTRACTS_FULL"), AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
    public sealed class ContractClassForAttribute : Attribute
    {
        private Type _typeIAmAContractFor;

        public ContractClassForAttribute(Type typeContractsAreFor)
        {
            this._typeIAmAContractFor = typeContractsAreFor;
        }

        public Type TypeContractsAreFor
        {
            get
            {
                return this._typeIAmAContractFor;
            }
        }
    }
}

