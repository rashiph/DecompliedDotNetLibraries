namespace System.EnterpriseServices
{
    using System;
    using System.Collections;

    internal class SecurityIdentityEnumerator : IEnumerator
    {
        private SecurityCallers _callers;
        private IEnumerator _E;

        internal SecurityIdentityEnumerator(IEnumerator E, SecurityCallers c)
        {
            this._E = E;
            this._callers = c;
        }

        public bool MoveNext()
        {
            return this._E.MoveNext();
        }

        public void Reset()
        {
            this._E.Reset();
        }

        public object Current
        {
            get
            {
                object current = this._E.Current;
                return this._callers[(int) current];
            }
        }
    }
}

