namespace System.Deployment.Internal.Isolation
{
    using System;

    internal sealed class ReferenceIdentity
    {
        internal System.Deployment.Internal.Isolation.IReferenceIdentity _id;

        internal ReferenceIdentity(System.Deployment.Internal.Isolation.IReferenceIdentity i)
        {
            if (i == null)
            {
                throw new ArgumentNullException();
            }
            this._id = i;
        }

        private void DeleteAttribute(string n)
        {
            this.SetAttribute(null, n, null);
        }

        private void DeleteAttribute(string ns, string n)
        {
            this.SetAttribute(ns, n, null);
        }

        private string GetAttribute(string n)
        {
            return this._id.GetAttribute(null, n);
        }

        private string GetAttribute(string ns, string n)
        {
            return this._id.GetAttribute(ns, n);
        }

        private void SetAttribute(string n, string v)
        {
            this.SetAttribute(null, n, v);
        }

        private void SetAttribute(string ns, string n, string v)
        {
            this._id.SetAttribute(ns, n, v);
        }
    }
}

