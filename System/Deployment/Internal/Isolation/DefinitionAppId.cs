namespace System.Deployment.Internal.Isolation
{
    using System;

    internal sealed class DefinitionAppId
    {
        internal System.Deployment.Internal.Isolation.IDefinitionAppId _id;

        internal DefinitionAppId(System.Deployment.Internal.Isolation.IDefinitionAppId id)
        {
            if (id == null)
            {
                throw new ArgumentNullException();
            }
            this._id = id;
        }

        private void SetAppPath(System.Deployment.Internal.Isolation.IDefinitionIdentity[] Ids)
        {
            this._id.SetAppPath((uint) Ids.Length, Ids);
        }

        public System.Deployment.Internal.Isolation.EnumDefinitionIdentity AppPath
        {
            get
            {
                return new System.Deployment.Internal.Isolation.EnumDefinitionIdentity(this._id.EnumAppPath());
            }
        }

        public string Codebase
        {
            get
            {
                return this._id.get_Codebase();
            }
            set
            {
                this._id.put_Codebase(value);
            }
        }

        public string SubscriptionId
        {
            get
            {
                return this._id.get_SubscriptionId();
            }
            set
            {
                this._id.put_SubscriptionId(value);
            }
        }
    }
}

