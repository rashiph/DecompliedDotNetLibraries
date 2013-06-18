namespace System.Web.Services.Discovery
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;

    internal abstract class DynamicDiscoSearcher
    {
        protected System.Web.Services.Discovery.DiscoveryDocument discoDoc = new System.Web.Services.Discovery.DiscoveryDocument();
        protected string[] excludedUrls;
        protected Hashtable excludedUrlsTable;
        protected ArrayList filesFound;
        protected string fileToSkipFirst;
        protected string origUrl;
        protected DiscoverySearchPattern[] primarySearchPatterns;
        protected DiscoverySearchPattern[] secondarySearchPatterns;
        protected int subDirLevel;

        internal DynamicDiscoSearcher(string[] excludeUrlsList)
        {
            this.excludedUrls = excludeUrlsList;
            this.filesFound = new ArrayList();
        }

        protected abstract DirectoryInfo GetPhysicalDir(string dir);
        protected bool IsExcluded(string url)
        {
            if (this.excludedUrlsTable == null)
            {
                this.excludedUrlsTable = new Hashtable();
                foreach (string str in this.excludedUrls)
                {
                    this.excludedUrlsTable.Add(this.MakeAbsExcludedPath(str).ToLower(CultureInfo.InvariantCulture), null);
                }
            }
            return this.excludedUrlsTable.Contains(url.ToLower(CultureInfo.InvariantCulture));
        }

        protected abstract string MakeAbsExcludedPath(string pathRelativ);
        protected abstract string MakeResultPath(string dirName, string fileName);
        protected bool ScanDirByPattern(string dir, bool IsPrimary, DiscoverySearchPattern[] patterns)
        {
            DirectoryInfo physicalDir = this.GetPhysicalDir(dir);
            if (physicalDir == null)
            {
                return false;
            }
            bool traceVerbose = System.ComponentModel.CompModSwitches.DynamicDiscoverySearcher.TraceVerbose;
            bool flag = false;
            for (int i = 0; i < patterns.Length; i++)
            {
                foreach (FileInfo info2 in physicalDir.GetFiles(patterns[i].Pattern))
                {
                    if ((info2.Attributes & FileAttributes.Directory) == 0)
                    {
                        bool flag2 = System.ComponentModel.CompModSwitches.DynamicDiscoverySearcher.TraceVerbose;
                        if (string.Compare(info2.Name, this.fileToSkipFirst, StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            string str = this.MakeResultPath(dir, info2.Name);
                            this.filesFound.Add(str);
                            this.discoDoc.References.Add(patterns[i].GetDiscoveryReference(str));
                            flag = true;
                        }
                    }
                }
            }
            return (IsPrimary && flag);
        }

        protected void ScanDirectory(string directory)
        {
            bool traceVerbose = System.ComponentModel.CompModSwitches.DynamicDiscoverySearcher.TraceVerbose;
            if (!this.IsExcluded(directory) && !this.ScanDirByPattern(directory, true, this.PrimarySearchPattern))
            {
                if (!this.IsVirtualSearch)
                {
                    this.ScanDirByPattern(directory, false, this.SecondarySearchPattern);
                }
                else if (this.subDirLevel != 0)
                {
                    DiscoverySearchPattern[] patterns = new DiscoverySearchPattern[] { new DiscoveryDocumentLinksPattern() };
                    this.ScanDirByPattern(directory, false, patterns);
                }
                if (!this.IsVirtualSearch || (this.subDirLevel <= 0))
                {
                    this.subDirLevel++;
                    this.fileToSkipFirst = "";
                    this.SearchSubDirectories(directory);
                    this.subDirLevel--;
                }
            }
        }

        internal abstract void Search(string fileToSkipAtBegin);
        internal virtual void SearchInit(string fileToSkipAtBegin)
        {
            this.subDirLevel = 0;
            this.fileToSkipFirst = fileToSkipAtBegin;
        }

        protected abstract void SearchSubDirectories(string directory);

        internal System.Web.Services.Discovery.DiscoveryDocument DiscoveryDocument
        {
            get
            {
                return this.discoDoc;
            }
        }

        protected abstract bool IsVirtualSearch { get; }

        internal DiscoverySearchPattern[] PrimarySearchPattern
        {
            get
            {
                if (this.primarySearchPatterns == null)
                {
                    this.primarySearchPatterns = new DiscoverySearchPattern[] { new DiscoveryDocumentSearchPattern() };
                }
                return this.primarySearchPatterns;
            }
        }

        internal DiscoverySearchPattern[] SecondarySearchPattern
        {
            get
            {
                if (this.secondarySearchPatterns == null)
                {
                    this.secondarySearchPatterns = new DiscoverySearchPattern[] { new ContractSearchPattern(), new DiscoveryDocumentLinksPattern() };
                }
                return this.secondarySearchPatterns;
            }
        }
    }
}

