namespace System.Web.Hosting
{
    using System;
    using System.Collections;
    using System.Web;

    internal class MapPathBasedVirtualPathCollection : MarshalByRefObject, IEnumerable
    {
        private RequestedEntryType _requestedEntryType;
        private VirtualPath _virtualPath;

        internal MapPathBasedVirtualPathCollection(VirtualPath virtualPath, RequestedEntryType requestedEntryType)
        {
            this._virtualPath = virtualPath;
            this._requestedEntryType = requestedEntryType;
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new MapPathBasedVirtualPathEnumerator(this._virtualPath, this._requestedEntryType);
        }
    }
}

