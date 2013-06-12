namespace System.Runtime.Remoting
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public class TypeEntry
    {
        private string _assemblyName;
        private RemoteAppEntry _cachedRemoteAppEntry;
        private string _typeName;

        protected TypeEntry()
        {
        }

        internal void CacheRemoteAppEntry(RemoteAppEntry entry)
        {
            this._cachedRemoteAppEntry = entry;
        }

        internal RemoteAppEntry GetRemoteAppEntry()
        {
            return this._cachedRemoteAppEntry;
        }

        public string AssemblyName
        {
            get
            {
                return this._assemblyName;
            }
            set
            {
                this._assemblyName = value;
            }
        }

        public string TypeName
        {
            get
            {
                return this._typeName;
            }
            set
            {
                this._typeName = value;
            }
        }
    }
}

