namespace System.Deployment.Internal.Isolation
{
    using System;

    internal sealed class ReferenceAppId
    {
        internal System.Deployment.Internal.Isolation.IReferenceAppId _id;

        internal ReferenceAppId(System.Deployment.Internal.Isolation.IReferenceAppId id)
        {
            if (id == null)
            {
                throw new ArgumentNullException();
            }
            this._id = id;
        }

        public System.Deployment.Internal.Isolation.EnumReferenceIdentity AppPath
        {
            get
            {
                return new System.Deployment.Internal.Isolation.EnumReferenceIdentity(this._id.EnumAppPath());
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

