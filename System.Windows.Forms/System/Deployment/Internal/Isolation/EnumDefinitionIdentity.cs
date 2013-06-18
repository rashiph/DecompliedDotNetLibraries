namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Collections;

    internal sealed class EnumDefinitionIdentity : IEnumerator
    {
        private System.Deployment.Internal.Isolation.IDefinitionIdentity _current;
        private System.Deployment.Internal.Isolation.IEnumDefinitionIdentity _enum;
        private System.Deployment.Internal.Isolation.IDefinitionIdentity[] _fetchList = new System.Deployment.Internal.Isolation.IDefinitionIdentity[1];

        internal EnumDefinitionIdentity(System.Deployment.Internal.Isolation.IEnumDefinitionIdentity e)
        {
            if (e == null)
            {
                throw new ArgumentNullException();
            }
            this._enum = e;
        }

        private System.Deployment.Internal.Isolation.DefinitionIdentity GetCurrent()
        {
            if (this._current == null)
            {
                throw new InvalidOperationException();
            }
            return new System.Deployment.Internal.Isolation.DefinitionIdentity(this._current);
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

        public System.Deployment.Internal.Isolation.DefinitionIdentity Current
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

