namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Collections;
    using System.Security;

    internal class StoreCategoryInstanceEnumeration : IEnumerator
    {
        private STORE_CATEGORY_INSTANCE _current;
        private IEnumSTORE_CATEGORY_INSTANCE _enum;
        private bool _fValid;

        [SecuritySafeCritical]
        public StoreCategoryInstanceEnumeration(IEnumSTORE_CATEGORY_INSTANCE pI)
        {
            this._enum = pI;
        }

        private STORE_CATEGORY_INSTANCE GetCurrent()
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
            STORE_CATEGORY_INSTANCE[] rgInstances = new STORE_CATEGORY_INSTANCE[1];
            uint num = this._enum.Next(1, rgInstances);
            if (num == 1)
            {
                this._current = rgInstances[0];
            }
            return (this._fValid = num == 1);
        }

        [SecuritySafeCritical]
        public void Reset()
        {
            this._fValid = false;
            this._enum.Reset();
        }

        public STORE_CATEGORY_INSTANCE Current
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

