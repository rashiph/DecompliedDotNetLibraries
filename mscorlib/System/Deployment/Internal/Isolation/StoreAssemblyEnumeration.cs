namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Collections;
    using System.Security;

    internal class StoreAssemblyEnumeration : IEnumerator
    {
        private STORE_ASSEMBLY _current;
        private IEnumSTORE_ASSEMBLY _enum;
        private bool _fValid;

        [SecuritySafeCritical]
        public StoreAssemblyEnumeration(IEnumSTORE_ASSEMBLY pI)
        {
            this._enum = pI;
        }

        private STORE_ASSEMBLY GetCurrent()
        {
            if (!this._fValid)
            {
                throw new InvalidOperationException();
            }
            return this._current;
        }

        public IEnumerator GetEnumerator()
        {
            return this;
        }

        [SecuritySafeCritical]
        public bool MoveNext()
        {
            STORE_ASSEMBLY[] rgelt = new STORE_ASSEMBLY[1];
            uint num = this._enum.Next(1, rgelt);
            if (num == 1)
            {
                this._current = rgelt[0];
            }
            return (this._fValid = num == 1);
        }

        [SecuritySafeCritical]
        public void Reset()
        {
            this._fValid = false;
            this._enum.Reset();
        }

        public STORE_ASSEMBLY Current
        {
            get
            {
                return this.GetCurrent();
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return this.GetCurrent();
            }
        }
    }
}

