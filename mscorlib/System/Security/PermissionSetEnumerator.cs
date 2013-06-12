namespace System.Security
{
    using System;
    using System.Collections;

    internal class PermissionSetEnumerator : IEnumerator
    {
        private PermissionSetEnumeratorInternal enm;

        internal PermissionSetEnumerator(PermissionSet permSet)
        {
            this.enm = new PermissionSetEnumeratorInternal(permSet);
        }

        public bool MoveNext()
        {
            return this.enm.MoveNext();
        }

        public void Reset()
        {
            this.enm.Reset();
        }

        public object Current
        {
            get
            {
                return this.enm.Current;
            }
        }
    }
}

