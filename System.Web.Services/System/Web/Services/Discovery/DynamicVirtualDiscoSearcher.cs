namespace System.Web.Services.Discovery
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.DirectoryServices;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using System.Web.Services;
    using System.Web.Services.Diagnostics;

    internal class DynamicVirtualDiscoSearcher : DynamicDiscoSearcher
    {
        private Hashtable Adsi;
        private string entryPathPrefix;
        private string rootPathAsdi;
        private string startDir;
        private Hashtable webApps;

        internal DynamicVirtualDiscoSearcher(string startDir, string[] excludedUrls, string rootUrl) : base(excludedUrls)
        {
            this.webApps = new Hashtable();
            this.Adsi = new Hashtable();
            base.origUrl = rootUrl;
            this.entryPathPrefix = this.GetWebServerForUrl(rootUrl) + "/ROOT";
            this.startDir = startDir;
            string localPath = new Uri(rootUrl).LocalPath;
            if (localPath.Equals("/"))
            {
                localPath = "";
            }
            this.rootPathAsdi = this.entryPathPrefix + localPath;
        }

        private void CleanupCache()
        {
            foreach (DictionaryEntry entry in this.Adsi)
            {
                ((DirectoryEntry) entry.Value).Dispose();
            }
            this.rootPathAsdi = null;
            this.entryPathPrefix = null;
            this.startDir = null;
            this.Adsi = null;
            this.webApps = null;
        }

        private AppSettings GetAppSettings(DirectoryEntry entry)
        {
            string path = entry.Path;
            AppSettings settings = null;
            object obj2 = this.webApps[path];
            if (obj2 == null)
            {
                lock (this.webApps)
                {
                    obj2 = this.webApps[path];
                    if (obj2 == null)
                    {
                        settings = new AppSettings(entry);
                        this.webApps[path] = settings;
                    }
                    goto Label_0063;
                }
            }
            settings = (AppSettings) obj2;
        Label_0063:
            if (!settings.AccessRead)
            {
                return null;
            }
            return settings;
        }

        protected override DirectoryInfo GetPhysicalDir(string dir)
        {
            DirectoryEntry entry = (DirectoryEntry) this.Adsi[dir];
            if (entry == null)
            {
                if (!DirectoryEntry.Exists(dir))
                {
                    return null;
                }
                entry = new DirectoryEntry(dir);
                this.Adsi[dir] = entry;
            }
            try
            {
                DirectoryInfo info = null;
                AppSettings appSettings = this.GetAppSettings(entry);
                if (appSettings == null)
                {
                    return null;
                }
                if (appSettings.VPath == null)
                {
                    if (!dir.StartsWith(this.rootPathAsdi, StringComparison.Ordinal))
                    {
                        throw new ArgumentException(System.Web.Services.Res.GetString("WebVirtualDisoRoot", new object[] { dir, this.rootPathAsdi }), "dir");
                    }
                    string str = dir.Substring(this.rootPathAsdi.Length).Replace('/', '\\');
                    info = new DirectoryInfo(this.startDir + str);
                }
                else
                {
                    info = new DirectoryInfo(appSettings.VPath);
                }
                if (info.Exists)
                {
                    return info;
                }
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                bool traceVerbose = System.ComponentModel.CompModSwitches.DynamicDiscoverySearcher.TraceVerbose;
                if (Tracing.On)
                {
                    Tracing.ExceptionCatch(TraceEventType.Warning, this, "GetPhysicalDir", exception);
                }
                return null;
            }
            return null;
        }

        private string GetWebServerForUrl(string url)
        {
            Uri uri = new Uri(url);
            DirectoryEntry entry = new DirectoryEntry("IIS://" + uri.Host + "/W3SVC");
            foreach (DirectoryEntry entry2 in entry.Children)
            {
                DirectoryEntry entry3 = (DirectoryEntry) this.Adsi[entry2.Path];
                if (entry3 == null)
                {
                    entry3 = entry2;
                    this.Adsi[entry2.Path] = entry2;
                }
                else
                {
                    entry2.Dispose();
                }
                AppSettings appSettings = this.GetAppSettings(entry3);
                if ((appSettings != null) && (appSettings.Bindings != null))
                {
                    foreach (string str in appSettings.Bindings)
                    {
                        bool traceVerbose = System.ComponentModel.CompModSwitches.DynamicDiscoverySearcher.TraceVerbose;
                        string[] strArray = str.Split(new char[] { ':' });
                        string strA = strArray[0];
                        string str3 = strArray[1];
                        string str4 = strArray[2];
                        if (Convert.ToInt32(str3, CultureInfo.InvariantCulture) == uri.Port)
                        {
                            if (uri.HostNameType == UriHostNameType.Dns)
                            {
                                if ((str4.Length == 0) || (string.Compare(str4, uri.Host, StringComparison.OrdinalIgnoreCase) == 0))
                                {
                                    return entry3.Path;
                                }
                            }
                            else if ((strA.Length == 0) || (string.Compare(strA, uri.Host, StringComparison.OrdinalIgnoreCase) == 0))
                            {
                                return entry3.Path;
                            }
                        }
                    }
                }
            }
            return null;
        }

        protected override string MakeAbsExcludedPath(string pathRelativ)
        {
            return (this.rootPathAsdi + '/' + pathRelativ.Replace('\\', '/'));
        }

        protected override string MakeResultPath(string dirName, string fileName)
        {
            return string.Concat(new object[] { base.origUrl, dirName.Substring(this.rootPathAsdi.Length, dirName.Length - this.rootPathAsdi.Length), '/', fileName });
        }

        internal override void Search(string fileToSkipAtBegin)
        {
            this.SearchInit(fileToSkipAtBegin);
            base.ScanDirectory(this.rootPathAsdi);
            this.CleanupCache();
        }

        protected override void SearchSubDirectories(string nameAdsiDir)
        {
            bool traceVerbose = System.ComponentModel.CompModSwitches.DynamicDiscoverySearcher.TraceVerbose;
            DirectoryEntry entry = (DirectoryEntry) this.Adsi[nameAdsiDir];
            if (entry == null)
            {
                if (!DirectoryEntry.Exists(nameAdsiDir))
                {
                    return;
                }
                entry = new DirectoryEntry(nameAdsiDir);
                this.Adsi[nameAdsiDir] = entry;
            }
            foreach (DirectoryEntry entry2 in entry.Children)
            {
                DirectoryEntry entry3 = (DirectoryEntry) this.Adsi[entry2.Path];
                if (entry3 == null)
                {
                    entry3 = entry2;
                    this.Adsi[entry2.Path] = entry2;
                }
                else
                {
                    entry2.Dispose();
                }
                if (this.GetAppSettings(entry3) != null)
                {
                    base.ScanDirectory(entry3.Path);
                }
            }
        }

        protected override bool IsVirtualSearch
        {
            get
            {
                return true;
            }
        }

        private class AppSettings
        {
            internal readonly bool AccessRead;
            internal readonly string[] Bindings;
            internal readonly string VPath;

            internal AppSettings(DirectoryEntry entry)
            {
                string schemaClassName = entry.SchemaClassName;
                this.AccessRead = true;
                switch (schemaClassName)
                {
                    case "IIsWebVirtualDir":
                    case "IIsWebDirectory":
                        if (!((bool) entry.Properties["AccessRead"][0]))
                        {
                            this.AccessRead = false;
                            return;
                        }
                        if (schemaClassName == "IIsWebVirtualDir")
                        {
                            this.VPath = (string) entry.Properties["Path"][0];
                            return;
                        }
                        break;

                    case "IIsWebServer":
                        this.Bindings = new string[entry.Properties["ServerBindings"].Count];
                        for (int i = 0; i < this.Bindings.Length; i++)
                        {
                            this.Bindings[i] = (string) entry.Properties["ServerBindings"][i];
                        }
                        return;

                    default:
                        this.AccessRead = false;
                        break;
                }
            }
        }
    }
}

