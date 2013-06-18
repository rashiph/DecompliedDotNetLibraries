namespace System.DirectoryServices
{
    using System;
    using System.Collections;
    using System.Configuration;
    using System.DirectoryServices.Interop;
    using System.Net;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;

    [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
    public class SearchResultCollection : MarshalByRefObject, ICollection, IEnumerable, IDisposable
    {
        private const string ADS_DIRSYNC_COOKIE = "fc8cb04d-311d-406c-8cb9-1ae8b843b418";
        private const string ADS_VLV_RESPONSE = "fc8cb04d-311d-406c-8cb9-1ae8b843b419";
        private IntPtr AdsDirsynCookieName = Marshal.StringToCoTaskMemUni("fc8cb04d-311d-406c-8cb9-1ae8b843b418");
        private IntPtr AdsVLVResponseName = Marshal.StringToCoTaskMemUni("fc8cb04d-311d-406c-8cb9-1ae8b843b419");
        private bool disposed;
        private string filter;
        private IntPtr handle;
        private ArrayList innerList;
        private string[] properties;
        private DirectoryEntry rootEntry;
        private System.DirectoryServices.Interop.UnsafeNativeMethods.IDirectorySearch searchObject;
        internal DirectorySearcher srch;

        internal SearchResultCollection(DirectoryEntry root, IntPtr searchHandle, string[] propertiesLoaded, DirectorySearcher srch)
        {
            this.handle = searchHandle;
            this.properties = propertiesLoaded;
            this.filter = srch.Filter;
            this.rootEntry = root;
            this.srch = srch;
        }

        public bool Contains(SearchResult result)
        {
            return this.InnerList.Contains(result);
        }

        public void CopyTo(SearchResult[] results, int index)
        {
            this.InnerList.CopyTo(results, index);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (((this.handle != IntPtr.Zero) && (this.searchObject != null)) && disposing)
                {
                    this.searchObject.CloseSearchHandle(this.handle);
                    this.handle = IntPtr.Zero;
                }
                if (disposing)
                {
                    this.rootEntry.Dispose();
                }
                if (this.AdsDirsynCookieName != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(this.AdsDirsynCookieName);
                }
                if (this.AdsVLVResponseName != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(this.AdsVLVResponseName);
                }
                this.disposed = true;
            }
        }

        ~SearchResultCollection()
        {
            this.Dispose(false);
        }

        public IEnumerator GetEnumerator()
        {
            return new ResultsEnumerator(this, this.rootEntry.GetUsername(), this.rootEntry.GetPassword(), this.rootEntry.AuthenticationType);
        }

        public int IndexOf(SearchResult result)
        {
            return this.InnerList.IndexOf(result);
        }

        internal unsafe byte[] RetrieveDirectorySynchronizationCookie()
        {
            byte[] buffer2;
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            AdsSearchColumn column = new AdsSearchColumn();
            AdsSearchColumn* columnPtr = &column;
            this.SearchObject.GetColumn(this.Handle, this.AdsDirsynCookieName, (IntPtr) ((ulong) columnPtr));
            try
            {
                buffer2 = (byte[]) new AdsValueHelper(column.pADsValues[0]).GetValue();
            }
            finally
            {
                try
                {
                    this.SearchObject.FreeColumn((IntPtr) ((ulong) columnPtr));
                }
                catch (COMException)
                {
                }
            }
            return buffer2;
        }

        internal unsafe DirectoryVirtualListView RetrieveVLVResponse()
        {
            DirectoryVirtualListView view2;
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            AdsSearchColumn column = new AdsSearchColumn();
            AdsSearchColumn* columnPtr = &column;
            this.SearchObject.GetColumn(this.Handle, this.AdsVLVResponseName, (IntPtr) ((ulong) columnPtr));
            try
            {
                view2 = (DirectoryVirtualListView) new AdsValueHelper(column.pADsValues[0]).GetVlvValue();
            }
            finally
            {
                try
                {
                    this.SearchObject.FreeColumn((IntPtr) ((ulong) columnPtr));
                }
                catch (COMException)
                {
                }
            }
            return view2;
        }

        void ICollection.CopyTo(Array array, int index)
        {
            this.InnerList.CopyTo(array, index);
        }

        public int Count
        {
            get
            {
                return this.InnerList.Count;
            }
        }

        internal byte[] DirsyncCookie
        {
            get
            {
                return this.RetrieveDirectorySynchronizationCookie();
            }
        }

        internal string Filter
        {
            get
            {
                return this.filter;
            }
        }

        public IntPtr Handle
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                return this.handle;
            }
        }

        private ArrayList InnerList
        {
            get
            {
                if (this.innerList == null)
                {
                    this.innerList = new ArrayList();
                    IEnumerator enumerator = new ResultsEnumerator(this, this.rootEntry.GetUsername(), this.rootEntry.GetPassword(), this.rootEntry.AuthenticationType);
                    while (enumerator.MoveNext())
                    {
                        this.innerList.Add(enumerator.Current);
                    }
                }
                return this.innerList;
            }
        }

        public SearchResult this[int index]
        {
            get
            {
                return (SearchResult) this.InnerList[index];
            }
        }

        public string[] PropertiesLoaded
        {
            get
            {
                return this.properties;
            }
        }

        internal System.DirectoryServices.Interop.UnsafeNativeMethods.IDirectorySearch SearchObject
        {
            get
            {
                if (this.searchObject == null)
                {
                    this.searchObject = (System.DirectoryServices.Interop.UnsafeNativeMethods.IDirectorySearch) this.rootEntry.AdsObject;
                }
                return this.searchObject;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return this;
            }
        }

        internal DirectoryVirtualListView VLVResponse
        {
            get
            {
                return this.RetrieveVLVResponse();
            }
        }

        private class ResultsEnumerator : IEnumerator
        {
            private SearchResult currentResult;
            private bool eof;
            private bool initialized;
            private AuthenticationTypes parentAuthenticationType;
            private NetworkCredential parentCredentials;
            private SearchResultCollection results;
            private bool waitForResult;

            internal ResultsEnumerator(SearchResultCollection results, string parentUserName, string parentPassword, AuthenticationTypes parentAuthenticationType)
            {
                if ((parentUserName != null) && (parentPassword != null))
                {
                    this.parentCredentials = new NetworkCredential(parentUserName, parentPassword);
                }
                this.parentAuthenticationType = parentAuthenticationType;
                this.results = results;
                this.initialized = false;
                object section = System.Configuration.PrivilegedConfigurationManager.GetSection("system.directoryservices");
                if ((section != null) && (section is bool))
                {
                    this.waitForResult = (bool) section;
                }
            }

            private void CleanLastError()
            {
                SafeNativeMethods.ADsSetLastError(0, null, null);
            }

            private unsafe SearchResult GetCurrentResult()
            {
                SearchResult result = new SearchResult(this.parentCredentials, this.parentAuthenticationType);
                int num = 0;
                IntPtr zero = IntPtr.Zero;
                for (num = this.results.SearchObject.GetNextColumnName(this.results.Handle, (IntPtr) ((ulong) ((IntPtr) &zero))); num == 0; num = this.results.SearchObject.GetNextColumnName(this.results.Handle, (IntPtr) ((ulong) ((IntPtr) &zero))))
                {
                    try
                    {
                        AdsSearchColumn column = new AdsSearchColumn();
                        AdsSearchColumn* columnPtr = &column;
                        this.results.SearchObject.GetColumn(this.results.Handle, zero, (IntPtr) ((ulong) columnPtr));
                        try
                        {
                            int dwNumValues = column.dwNumValues;
                            AdsValue* pADsValues = column.pADsValues;
                            object[] values = new object[dwNumValues];
                            for (int i = 0; i < dwNumValues; i++)
                            {
                                values[i] = new AdsValueHelper(pADsValues[0]).GetValue();
                                pADsValues++;
                            }
                            result.Properties.Add(Marshal.PtrToStringUni(zero), new ResultPropertyValueCollection(values));
                        }
                        finally
                        {
                            try
                            {
                                this.results.SearchObject.FreeColumn((IntPtr) ((ulong) columnPtr));
                            }
                            catch (COMException)
                            {
                            }
                        }
                    }
                    finally
                    {
                        SafeNativeMethods.FreeADsMem(zero);
                    }
                }
                return result;
            }

            private int GetLastError(ref int errorCode)
            {
                StringBuilder errorBuffer = new StringBuilder();
                StringBuilder nameBuffer = new StringBuilder();
                errorCode = 0;
                return SafeNativeMethods.ADsGetLastError(out errorCode, errorBuffer, 0, nameBuffer, 0);
            }

            public bool MoveNext()
            {
                int errorCode = 0;
                if (this.eof)
                {
                    return false;
                }
                this.currentResult = null;
                if (!this.initialized)
                {
                    int firstRow = this.results.SearchObject.GetFirstRow(this.results.Handle);
                    switch (firstRow)
                    {
                        case 0x5012:
                            this.initialized = true;
                            break;

                        case -2147016642:
                            throw new ArgumentException(Res.GetString("DSInvalidSearchFilter", new object[] { this.results.Filter }));

                        default:
                            if (firstRow != 0)
                            {
                                throw COMExceptionHelper.CreateFormattedComException(firstRow);
                            }
                            this.eof = false;
                            this.initialized = true;
                            return true;
                    }
                }
            Label_0091:
                this.CleanLastError();
                errorCode = 0;
                int nextRow = this.results.SearchObject.GetNextRow(this.results.Handle);
                switch (nextRow)
                {
                    case 0x5012:
                    case -2147016669:
                        if (nextRow == 0x5012)
                        {
                            nextRow = this.GetLastError(ref errorCode);
                            if (nextRow != 0)
                            {
                                throw COMExceptionHelper.CreateFormattedComException(nextRow);
                            }
                        }
                        if (errorCode != 0xea)
                        {
                            if (this.results.srch.directorySynchronizationSpecified)
                            {
                                DirectorySynchronization directorySynchronization = this.results.srch.DirectorySynchronization;
                            }
                            if (this.results.srch.directoryVirtualListViewSpecified)
                            {
                                DirectoryVirtualListView virtualListView = this.results.srch.VirtualListView;
                            }
                            this.results.srch.searchResult = null;
                            this.eof = true;
                            this.initialized = false;
                            return false;
                        }
                        if (!this.waitForResult)
                        {
                            uint num4 = (uint) errorCode;
                            num4 = ((num4 & 0xffff) | 0x70000) | 0x80000000;
                            throw COMExceptionHelper.CreateFormattedComException((int) num4);
                        }
                        goto Label_0091;

                    case -2147016642:
                        throw new ArgumentException(Res.GetString("DSInvalidSearchFilter", new object[] { this.results.Filter }));
                }
                if (nextRow != 0)
                {
                    throw COMExceptionHelper.CreateFormattedComException(nextRow);
                }
                this.eof = false;
                return true;
            }

            public void Reset()
            {
                this.eof = false;
                this.initialized = false;
            }

            public SearchResult Current
            {
                get
                {
                    if (!this.initialized || this.eof)
                    {
                        throw new InvalidOperationException(Res.GetString("DSNoCurrentEntry"));
                    }
                    if (this.currentResult == null)
                    {
                        this.currentResult = this.GetCurrentResult();
                    }
                    return this.currentResult;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }
        }
    }
}

