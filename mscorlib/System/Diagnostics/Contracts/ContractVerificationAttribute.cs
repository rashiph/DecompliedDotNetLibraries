namespace System.Diagnostics.Contracts
{
    using System;
    using System.Diagnostics;

    [Conditional("CONTRACTS_FULL"), AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly)]
    public sealed class ContractVerificationAttribute : Attribute
    {
        private bool _value;

        public ContractVerificationAttribute(bool value)
        {
            this._value = value;
        }

        public bool Value
        {
            get
            {
                return this._value;
            }
        }
    }
}

