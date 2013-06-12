namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Collections;
    using System.Security;

    internal class StoreDeploymentMetadataEnumeration : IEnumerator
    {
        private IDefinitionAppId _current;
        private IEnumSTORE_DEPLOYMENT_METADATA _enum;
        private bool _fValid;

        [SecuritySafeCritical]
        public StoreDeploymentMetadataEnumeration(IEnumSTORE_DEPLOYMENT_METADATA pI)
        {
            this._enum = pI;
        }

        private IDefinitionAppId GetCurrent()
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
            IDefinitionAppId[] appIds = new IDefinitionAppId[1];
            uint num = this._enum.Next(1, appIds);
            if (num == 1)
            {
                this._current = appIds[0];
            }
            return (this._fValid = num == 1);
        }

        [SecuritySafeCritical]
        public void Reset()
        {
            this._fValid = false;
            this._enum.Reset();
        }

        public IDefinitionAppId Current
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

