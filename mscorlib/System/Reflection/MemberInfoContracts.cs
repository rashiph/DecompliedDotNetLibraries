namespace System.Reflection
{
    using System;
    using System.Diagnostics.Contracts;

    internal abstract class MemberInfoContracts : MemberInfo
    {
        protected MemberInfoContracts()
        {
        }

        public override string Name
        {
            get
            {
                return Contract.Result<string>();
            }
        }
    }
}

