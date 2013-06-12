namespace System.Security.Principal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    [ComVisible(false)]
    internal class IdentityReferenceEnumerator : IEnumerator<IdentityReference>, IEnumerator, IDisposable
    {
        private readonly IdentityReferenceCollection _Collection;
        private int _Current;

        internal IdentityReferenceEnumerator(IdentityReferenceCollection collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            this._Collection = collection;
            this._Current = -1;
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            this._Current++;
            return (this._Current < this._Collection.Count);
        }

        public void Reset()
        {
            this._Current = -1;
        }

        public IdentityReference Current
        {
            get
            {
                return (((IEnumerator) this).Current as IdentityReference);
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return this._Collection.Identities[this._Current];
            }
        }
    }
}

