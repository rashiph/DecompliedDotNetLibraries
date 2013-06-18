namespace System.EnterpriseServices
{
    using System;
    using System.Collections;
    using System.Reflection;

    public sealed class SecurityCallers : IEnumerable
    {
        private ISecurityCallersColl _ex;

        private SecurityCallers()
        {
        }

        internal SecurityCallers(ISecurityCallersColl ifc)
        {
            this._ex = ifc;
        }

        public IEnumerator GetEnumerator()
        {
            IEnumerator pEnum = null;
            this._ex.GetEnumerator(out pEnum);
            return new SecurityIdentityEnumerator(pEnum, this);
        }

        public int Count
        {
            get
            {
                return this._ex.Count;
            }
        }

        public SecurityIdentity this[int idx]
        {
            get
            {
                return new SecurityIdentity(this._ex.GetItem(idx));
            }
        }
    }
}

