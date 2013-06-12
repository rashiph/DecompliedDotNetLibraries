namespace System.Web.SessionState
{
    using System;
    using System.Web;

    public class SessionStateStoreData
    {
        private ISessionStateItemCollection _sessionItems;
        private HttpStaticObjectsCollection _staticObjects;
        private int _timeout;

        public SessionStateStoreData(ISessionStateItemCollection sessionItems, HttpStaticObjectsCollection staticObjects, int timeout)
        {
            this._sessionItems = sessionItems;
            this._staticObjects = staticObjects;
            this._timeout = timeout;
        }

        public virtual ISessionStateItemCollection Items
        {
            get
            {
                return this._sessionItems;
            }
        }

        public virtual HttpStaticObjectsCollection StaticObjects
        {
            get
            {
                return this._staticObjects;
            }
        }

        public virtual int Timeout
        {
            get
            {
                return this._timeout;
            }
            set
            {
                this._timeout = value;
            }
        }
    }
}

