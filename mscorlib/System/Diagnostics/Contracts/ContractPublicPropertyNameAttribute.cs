namespace System.Diagnostics.Contracts
{
    using System;
    using System.Diagnostics;

    [Conditional("CONTRACTS_FULL"), AttributeUsage(AttributeTargets.Field)]
    public sealed class ContractPublicPropertyNameAttribute : Attribute
    {
        private string _publicName;

        public ContractPublicPropertyNameAttribute(string name)
        {
            this._publicName = name;
        }

        public string Name
        {
            get
            {
                return this._publicName;
            }
        }
    }
}

