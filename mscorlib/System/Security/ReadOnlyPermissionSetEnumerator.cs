namespace System.Security
{
    using System;
    using System.Collections;

    internal sealed class ReadOnlyPermissionSetEnumerator : IEnumerator
    {
        private IEnumerator m_permissionSetEnumerator;

        internal ReadOnlyPermissionSetEnumerator(IEnumerator permissionSetEnumerator)
        {
            this.m_permissionSetEnumerator = permissionSetEnumerator;
        }

        public bool MoveNext()
        {
            return this.m_permissionSetEnumerator.MoveNext();
        }

        public void Reset()
        {
            this.m_permissionSetEnumerator.Reset();
        }

        public object Current
        {
            get
            {
                IPermission current = this.m_permissionSetEnumerator.Current as IPermission;
                if (current == null)
                {
                    return null;
                }
                return current.Copy();
            }
        }
    }
}

