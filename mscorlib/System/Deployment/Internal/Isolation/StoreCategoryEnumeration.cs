namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Collections;
    using System.Security;

    internal class StoreCategoryEnumeration : IEnumerator
    {
        private STORE_CATEGORY _current;
        private IEnumSTORE_CATEGORY _enum;
        private bool _fValid;

        [SecuritySafeCritical]
        public StoreCategoryEnumeration(IEnumSTORE_CATEGORY pI)
        {
            this._enum = pI;
        }

        private STORE_CATEGORY GetCurrent()
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
            STORE_CATEGORY[] rgElements = new STORE_CATEGORY[1];
            uint num = this._enum.Next(1, rgElements);
            if (num == 1)
            {
                this._current = rgElements[0];
            }
            return (this._fValid = num == 1);
        }

        [SecuritySafeCritical]
        public void Reset()
        {
            this._fValid = false;
            this._enum.Reset();
        }

        public STORE_CATEGORY Current
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

