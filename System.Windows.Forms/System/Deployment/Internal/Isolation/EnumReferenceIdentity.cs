namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Collections;

    internal sealed class EnumReferenceIdentity : IEnumerator
    {
        private System.Deployment.Internal.Isolation.IReferenceIdentity _current;
        private System.Deployment.Internal.Isolation.IEnumReferenceIdentity _enum;
        private System.Deployment.Internal.Isolation.IReferenceIdentity[] _fetchList = new System.Deployment.Internal.Isolation.IReferenceIdentity[1];

        internal EnumReferenceIdentity(System.Deployment.Internal.Isolation.IEnumReferenceIdentity e)
        {
            this._enum = e;
        }

        private System.Deployment.Internal.Isolation.ReferenceIdentity GetCurrent()
        {
            if (this._current == null)
            {
                throw new InvalidOperationException();
            }
            return new System.Deployment.Internal.Isolation.ReferenceIdentity(this._current);
        }

        public IEnumerator GetEnumerator()
        {
            return this;
        }

        public bool MoveNext()
        {
            if (this._enum.Next(1, this._fetchList) == 1)
            {
                this._current = this._fetchList[0];
                return true;
            }
            this._current = null;
            return false;
        }

        public void Reset()
        {
            this._current = null;
            this._enum.Reset();
        }

        public System.Deployment.Internal.Isolation.ReferenceIdentity Current
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

