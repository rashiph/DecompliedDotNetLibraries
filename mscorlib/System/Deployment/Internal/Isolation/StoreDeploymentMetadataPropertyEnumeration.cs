namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Collections;
    using System.Security;

    internal class StoreDeploymentMetadataPropertyEnumeration : IEnumerator
    {
        private StoreOperationMetadataProperty _current;
        private IEnumSTORE_DEPLOYMENT_METADATA_PROPERTY _enum;
        private bool _fValid;

        [SecuritySafeCritical]
        public StoreDeploymentMetadataPropertyEnumeration(IEnumSTORE_DEPLOYMENT_METADATA_PROPERTY pI)
        {
            this._enum = pI;
        }

        private StoreOperationMetadataProperty GetCurrent()
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
            StoreOperationMetadataProperty[] appIds = new StoreOperationMetadataProperty[1];
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

        public StoreOperationMetadataProperty Current
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

