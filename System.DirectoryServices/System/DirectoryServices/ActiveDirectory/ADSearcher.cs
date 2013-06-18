namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections.Specialized;
    using System.DirectoryServices;
    using System.Security.Permissions;

    [DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
    internal class ADSearcher
    {
        private static TimeSpan defaultTimeSpan = new TimeSpan(0, 120, 0);
        private DirectorySearcher searcher;

        public ADSearcher(DirectoryEntry searchRoot, string filter, string[] propertiesToLoad, SearchScope scope)
        {
            this.searcher = new DirectorySearcher(searchRoot, filter, propertiesToLoad, scope);
            this.searcher.CacheResults = false;
            this.searcher.ClientTimeout = defaultTimeSpan;
            this.searcher.ServerPageTimeLimit = defaultTimeSpan;
            this.searcher.PageSize = 0x200;
        }

        public ADSearcher(DirectoryEntry searchRoot, string filter, string[] propertiesToLoad, SearchScope scope, bool pagedSearch, bool cacheResults)
        {
            this.searcher = new DirectorySearcher(searchRoot, filter, propertiesToLoad, scope);
            this.searcher.ClientTimeout = defaultTimeSpan;
            if (pagedSearch)
            {
                this.searcher.PageSize = 0x200;
                this.searcher.ServerPageTimeLimit = defaultTimeSpan;
            }
            if (cacheResults)
            {
                this.searcher.CacheResults = true;
            }
            else
            {
                this.searcher.CacheResults = false;
            }
        }

        public void Dispose()
        {
            this.searcher.Dispose();
        }

        public SearchResultCollection FindAll()
        {
            return this.searcher.FindAll();
        }

        public SearchResult FindOne()
        {
            return this.searcher.FindOne();
        }

        public string Filter
        {
            get
            {
                return this.searcher.Filter;
            }
            set
            {
                this.searcher.Filter = value;
            }
        }

        public StringCollection PropertiesToLoad
        {
            get
            {
                return this.searcher.PropertiesToLoad;
            }
        }
    }
}

