namespace System.Web.Caching
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Web;
    using System.Web.Util;

    public class CacheDependency : IDisposable
    {
        private SafeBitVector32 _bits;
        private object _depFileInfos;
        private object _entries;
        private ICacheDependencyChanged _objNotify;
        private string _uniqueID;
        private DateTime _utcLastModified;
        private const int BASE_DISPOSED = 8;
        private const int BASE_INIT = 1;
        private const int CHANGED = 4;
        private const int DERIVED_DISPOSED = 0x40;
        private const int DERIVED_INIT = 0x20;
        private static readonly TimeSpan FUTURE_FILETIME_BUFFER = new TimeSpan(0, 1, 0);
        private static readonly CacheDependency s_dependencyEmpty = new CacheDependency(0);
        private static readonly DepFileInfo[] s_depFileInfosEmpty = new DepFileInfo[0];
        private static readonly CacheEntry[] s_entriesEmpty = new CacheEntry[0];
        private static readonly string[] s_stringsEmpty = new string[0];
        private const int USED = 2;
        private const int WANTS_DISPOSE = 0x10;

        protected CacheDependency()
        {
            this.Init(true, null, null, null, DateTime.MaxValue);
        }

        private CacheDependency(int bogus)
        {
        }

        public CacheDependency(string filename) : this(filename, DateTime.MaxValue)
        {
        }

        public CacheDependency(string[] filenames)
        {
            this.Init(true, filenames, null, null, DateTime.MaxValue);
        }

        public CacheDependency(string filename, DateTime start)
        {
            if (filename != null)
            {
                DateTime utcStart = DateTimeUtil.ConvertToUniversalTime(start);
                string[] filenamesArg = new string[] { filename };
                this.Init(true, filenamesArg, null, null, utcStart);
            }
        }

        public CacheDependency(string[] filenames, DateTime start)
        {
            DateTime utcStart = DateTimeUtil.ConvertToUniversalTime(start);
            this.Init(true, filenames, null, null, utcStart);
        }

        public CacheDependency(string[] filenames, string[] cachekeys)
        {
            this.Init(true, filenames, cachekeys, null, DateTime.MaxValue);
        }

        internal CacheDependency(int dummy, string filename) : this(dummy, filename, DateTime.MaxValue)
        {
        }

        internal CacheDependency(int dummy, string[] filenames)
        {
            this.Init(false, filenames, null, null, DateTime.MaxValue);
        }

        public CacheDependency(string[] filenames, string[] cachekeys, DateTime start)
        {
            DateTime utcStart = DateTimeUtil.ConvertToUniversalTime(start);
            this.Init(true, filenames, cachekeys, null, utcStart);
        }

        internal CacheDependency(int dummy, string filename, DateTime utcStart)
        {
            if (filename != null)
            {
                string[] filenamesArg = new string[] { filename };
                this.Init(false, filenamesArg, null, null, utcStart);
            }
        }

        internal CacheDependency(int dummy, string[] filenames, DateTime utcStart)
        {
            this.Init(false, filenames, null, null, utcStart);
        }

        public CacheDependency(string[] filenames, string[] cachekeys, CacheDependency dependency)
        {
            this.Init(true, filenames, cachekeys, dependency, DateTime.MaxValue);
        }

        internal CacheDependency(int dummy, string[] filenames, string[] cachekeys)
        {
            this.Init(false, filenames, cachekeys, null, DateTime.MaxValue);
        }

        public CacheDependency(string[] filenames, string[] cachekeys, CacheDependency dependency, DateTime start)
        {
            DateTime utcStart = DateTimeUtil.ConvertToUniversalTime(start);
            this.Init(true, filenames, cachekeys, dependency, utcStart);
        }

        internal CacheDependency(int dummy, string[] filenames, string[] cachekeys, DateTime utcStart)
        {
            this.Init(false, filenames, cachekeys, null, utcStart);
        }

        internal CacheDependency(int dummy, string[] filenames, string[] cachekeys, CacheDependency dependency)
        {
            this.Init(false, filenames, cachekeys, dependency, DateTime.MaxValue);
        }

        internal CacheDependency(int dummy, string[] filenames, string[] cachekeys, CacheDependency dependency, DateTime utcStart)
        {
            this.Init(false, filenames, cachekeys, dependency, utcStart);
        }

        internal void AppendFileUniqueId(DepFileInfo depFileInfo, StringBuilder sb)
        {
            FileAttributesData nonExistantAttributesData = depFileInfo._fad;
            if (nonExistantAttributesData == null)
            {
                nonExistantAttributesData = FileAttributesData.NonExistantAttributesData;
            }
            sb.Append(depFileInfo._filename);
            sb.Append(nonExistantAttributesData.UtcLastWriteTime.Ticks.ToString("d", NumberFormatInfo.InvariantInfo));
            sb.Append(nonExistantAttributesData.FileSize.ToString(CultureInfo.InvariantCulture));
        }

        protected virtual void DependencyDispose()
        {
        }

        public void Dispose()
        {
            this._bits[0x20] = true;
            if (this.Use())
            {
                this.DisposeInternal();
            }
        }

        internal void DisposeInternal()
        {
            this._bits[0x10] = true;
            if (this._bits[0x20] && this._bits.ChangeValue(0x40, true))
            {
                this.DependencyDispose();
            }
            if (this._bits[1] && this._bits.ChangeValue(8, true))
            {
                this.DisposeOurself();
            }
        }

        private void DisposeOurself()
        {
            object obj2 = this._depFileInfos;
            object obj3 = this._entries;
            this._objNotify = null;
            this._depFileInfos = null;
            this._entries = null;
            if (obj2 != null)
            {
                FileChangesMonitor fileChangesMonitor = HttpRuntime.FileChangesMonitor;
                DepFileInfo info = obj2 as DepFileInfo;
                if (info != null)
                {
                    fileChangesMonitor.StopMonitoringPath(info._filename, this);
                }
                else
                {
                    DepFileInfo[] infoArray = (DepFileInfo[]) obj2;
                    foreach (DepFileInfo info2 in infoArray)
                    {
                        string alias = info2._filename;
                        if (alias != null)
                        {
                            fileChangesMonitor.StopMonitoringPath(alias, this);
                        }
                    }
                }
            }
            if (obj3 != null)
            {
                CacheEntry entry = obj3 as CacheEntry;
                if (entry != null)
                {
                    entry.RemoveCacheDependencyNotify(this);
                }
                else
                {
                    CacheEntry[] entryArray = (CacheEntry[]) obj3;
                    foreach (CacheEntry entry2 in entryArray)
                    {
                        if (entry2 != null)
                        {
                            entry2.RemoveCacheDependencyNotify(this);
                        }
                    }
                }
            }
        }

        private void FileChange(object sender, FileChangeEvent e)
        {
            this.NotifyDependencyChanged(sender, e);
        }

        protected internal void FinishInit()
        {
            this._bits[0x20] = true;
            if (this._bits[0x10])
            {
                this.DisposeInternal();
            }
        }

        internal virtual string[] GetFileDependencies()
        {
            object obj2 = this._depFileInfos;
            if (obj2 == null)
            {
                return null;
            }
            DepFileInfo info = obj2 as DepFileInfo;
            if (info != null)
            {
                return new string[] { info._filename };
            }
            DepFileInfo[] infoArray = (DepFileInfo[]) obj2;
            string[] strArray = new string[infoArray.Length];
            for (int i = 0; i < infoArray.Length; i++)
            {
                strArray[i] = infoArray[i]._filename;
            }
            return strArray;
        }

        public virtual string GetUniqueID()
        {
            return this._uniqueID;
        }

        private void Init(bool isPublic, string[] filenamesArg, string[] cachekeysArg, CacheDependency dependency, DateTime utcStart)
        {
            string[] strArray;
            string[] strArray2;
            DepFileInfo[] infoArray = s_depFileInfosEmpty;
            CacheEntry[] entryArray = s_entriesEmpty;
            this._bits = new SafeBitVector32(0);
            if (filenamesArg != null)
            {
                strArray = (string[]) filenamesArg.Clone();
            }
            else
            {
                strArray = null;
            }
            if (cachekeysArg != null)
            {
                strArray2 = (string[]) cachekeysArg.Clone();
            }
            else
            {
                strArray2 = null;
            }
            this._utcLastModified = DateTime.MinValue;
            try
            {
                if (strArray == null)
                {
                    strArray = s_stringsEmpty;
                }
                else
                {
                    foreach (string str in strArray)
                    {
                        if (str == null)
                        {
                            throw new ArgumentNullException("filenamesArg");
                        }
                        if (isPublic)
                        {
                            InternalSecurityPermissions.PathDiscovery(str).Demand();
                        }
                    }
                }
                if (strArray2 == null)
                {
                    strArray2 = s_stringsEmpty;
                }
                else
                {
                    string[] strArray4 = strArray2;
                    for (int i = 0; i < strArray4.Length; i++)
                    {
                        if (strArray4[i] == null)
                        {
                            throw new ArgumentNullException("cachekeysArg");
                        }
                    }
                }
                if (dependency == null)
                {
                    dependency = s_dependencyEmpty;
                }
                else
                {
                    if (dependency.GetType() != s_dependencyEmpty.GetType())
                    {
                        throw new ArgumentException(System.Web.SR.GetString("Invalid_Dependency_Type"));
                    }
                    object obj2 = dependency._depFileInfos;
                    object obj3 = dependency._entries;
                    DateTime time = dependency._utcLastModified;
                    if (dependency._bits[4])
                    {
                        this._bits[4] = true;
                        this.DisposeInternal();
                        return;
                    }
                    if (obj2 != null)
                    {
                        if (obj2 is DepFileInfo)
                        {
                            infoArray = new DepFileInfo[] { (DepFileInfo) obj2 };
                        }
                        else
                        {
                            infoArray = (DepFileInfo[]) obj2;
                        }
                        foreach (DepFileInfo info in infoArray)
                        {
                            string path = info._filename;
                            if (path == null)
                            {
                                this._bits[4] = true;
                                this.DisposeInternal();
                                return;
                            }
                            if (isPublic)
                            {
                                InternalSecurityPermissions.PathDiscovery(path).Demand();
                            }
                        }
                    }
                    if (obj3 != null)
                    {
                        if (obj3 is CacheEntry)
                        {
                            entryArray = new CacheEntry[] { (CacheEntry) obj3 };
                        }
                        else
                        {
                            entryArray = (CacheEntry[]) obj3;
                            CacheEntry[] entryArray4 = entryArray;
                            for (int j = 0; j < entryArray4.Length; j++)
                            {
                                if (entryArray4[j] == null)
                                {
                                    this._bits[4] = true;
                                    this.DisposeInternal();
                                    return;
                                }
                            }
                        }
                    }
                    this._utcLastModified = time;
                }
                int num = infoArray.Length + strArray.Length;
                if (num > 0)
                {
                    int num2;
                    DepFileInfo[] infoArray2 = new DepFileInfo[num];
                    FileChangeEventHandler callback = new FileChangeEventHandler(this.FileChange);
                    FileChangesMonitor fileChangesMonitor = HttpRuntime.FileChangesMonitor;
                    for (num2 = 0; num2 < num; num2++)
                    {
                        infoArray2[num2] = new DepFileInfo();
                    }
                    num2 = 0;
                    foreach (DepFileInfo info2 in infoArray)
                    {
                        string alias = info2._filename;
                        fileChangesMonitor.StartMonitoringPath(alias, callback, out infoArray2[num2]._fad);
                        infoArray2[num2]._filename = alias;
                        num2++;
                    }
                    DateTime minValue = DateTime.MinValue;
                    foreach (string str5 in strArray)
                    {
                        DateTime time3 = fileChangesMonitor.StartMonitoringPath(str5, callback, out infoArray2[num2]._fad);
                        infoArray2[num2]._filename = str5;
                        num2++;
                        if (time3 > this._utcLastModified)
                        {
                            this._utcLastModified = time3;
                        }
                        if (utcStart < DateTime.MaxValue)
                        {
                            if (minValue == DateTime.MinValue)
                            {
                                minValue = DateTime.UtcNow;
                            }
                            if ((time3 >= utcStart) && ((time3 - minValue) <= FUTURE_FILETIME_BUFFER))
                            {
                                this._bits[4] = true;
                                break;
                            }
                        }
                    }
                    if (infoArray2.Length == 1)
                    {
                        this._depFileInfos = infoArray2[0];
                    }
                    else
                    {
                        this._depFileInfos = infoArray2;
                    }
                }
                int num3 = entryArray.Length + strArray2.Length;
                if ((num3 > 0) && !this._bits[4])
                {
                    CacheEntry[] entryArray2 = new CacheEntry[num3];
                    int num4 = 0;
                    foreach (CacheEntry entry2 in entryArray)
                    {
                        entry2.AddCacheDependencyNotify(this);
                        entryArray2[num4++] = entry2;
                    }
                    CacheInternal cacheInternal = HttpRuntime.CacheInternal;
                    foreach (string str6 in strArray2)
                    {
                        CacheEntry entry3 = (CacheEntry) cacheInternal.DoGet(isPublic, str6, CacheGetOptions.ReturnCacheEntry);
                        if (entry3 != null)
                        {
                            entry3.AddCacheDependencyNotify(this);
                            entryArray2[num4++] = entry3;
                            if (entry3.UtcCreated > this._utcLastModified)
                            {
                                this._utcLastModified = entry3.UtcCreated;
                            }
                            if ((entry3.State == CacheEntry.EntryState.AddedToCache) && (entry3.UtcCreated <= utcStart))
                            {
                                continue;
                            }
                            this._bits[4] = true;
                        }
                        else
                        {
                            this._bits[4] = true;
                        }
                        break;
                    }
                    if (entryArray2.Length == 1)
                    {
                        this._entries = entryArray2[0];
                    }
                    else
                    {
                        this._entries = entryArray2;
                    }
                }
                this._bits[1] = true;
                if (dependency._bits[4])
                {
                    this._bits[4] = true;
                }
                if (this._bits[0x10] || this._bits[4])
                {
                    this.DisposeInternal();
                }
            }
            catch
            {
                this._bits[1] = true;
                this.DisposeInternal();
                throw;
            }
            finally
            {
                this.InitUniqueID();
            }
        }

        private void InitUniqueID()
        {
            StringBuilder sb = null;
            object obj2 = this._depFileInfos;
            if (obj2 != null)
            {
                DepFileInfo depFileInfo = obj2 as DepFileInfo;
                if (depFileInfo != null)
                {
                    sb = new StringBuilder();
                    this.AppendFileUniqueId(depFileInfo, sb);
                }
                else
                {
                    DepFileInfo[] infoArray = (DepFileInfo[]) obj2;
                    foreach (DepFileInfo info2 in infoArray)
                    {
                        if (info2._filename != null)
                        {
                            if (sb == null)
                            {
                                sb = new StringBuilder();
                            }
                            this.AppendFileUniqueId(info2, sb);
                        }
                    }
                }
            }
            object obj3 = this._entries;
            if (obj3 != null)
            {
                CacheEntry entry = obj3 as CacheEntry;
                if (entry != null)
                {
                    if (sb == null)
                    {
                        sb = new StringBuilder();
                    }
                    sb.Append(entry.Key);
                    sb.Append(entry.UtcCreated.Ticks.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    CacheEntry[] entryArray = (CacheEntry[]) obj3;
                    foreach (CacheEntry entry2 in entryArray)
                    {
                        if (entry2 != null)
                        {
                            if (sb == null)
                            {
                                sb = new StringBuilder();
                            }
                            sb.Append(entry2.Key);
                            sb.Append(entry2.UtcCreated.Ticks.ToString(CultureInfo.InvariantCulture));
                        }
                    }
                }
            }
            if (sb != null)
            {
                this._uniqueID = sb.ToString();
            }
        }

        internal virtual bool IsFileDependency()
        {
            object obj3 = this._entries;
            if (obj3 != null)
            {
                if (obj3 is CacheEntry)
                {
                    return false;
                }
                CacheEntry[] entryArray = (CacheEntry[]) obj3;
                if ((entryArray != null) && (entryArray.Length > 0))
                {
                    return false;
                }
            }
            object obj2 = this._depFileInfos;
            if (obj2 != null)
            {
                if (obj2 is DepFileInfo)
                {
                    return true;
                }
                DepFileInfo[] infoArray = (DepFileInfo[]) obj2;
                if ((infoArray != null) && (infoArray.Length > 0))
                {
                    return true;
                }
            }
            return false;
        }

        internal void ItemRemoved()
        {
            this.NotifyDependencyChanged(this, EventArgs.Empty);
        }

        protected void NotifyDependencyChanged(object sender, EventArgs e)
        {
            if (this._bits.ChangeValue(4, true))
            {
                this._utcLastModified = DateTime.UtcNow;
                ICacheDependencyChanged changed = this._objNotify;
                if ((changed != null) && !this._bits[8])
                {
                    changed.DependencyChanged(sender, e);
                }
                this.DisposeInternal();
            }
        }

        internal void SetCacheDependencyChanged(ICacheDependencyChanged objNotify)
        {
            this._bits[0x20] = true;
            if (!this._bits[8])
            {
                this._objNotify = objNotify;
            }
        }

        protected void SetUtcLastModified(DateTime utcLastModified)
        {
            this._utcLastModified = utcLastModified;
        }

        internal bool Use()
        {
            return this._bits.ChangeValue(2, true);
        }

        internal CacheEntry[] CacheEntries
        {
            get
            {
                if (this._entries == null)
                {
                    return null;
                }
                CacheEntry entry = this._entries as CacheEntry;
                if (entry != null)
                {
                    return new CacheEntry[] { entry };
                }
                return (CacheEntry[]) this._entries;
            }
        }

        public bool HasChanged
        {
            get
            {
                return this._bits[4];
            }
        }

        public DateTime UtcLastModified
        {
            get
            {
                return this._utcLastModified;
            }
        }

        internal class DepFileInfo
        {
            internal FileAttributesData _fad;
            internal string _filename;
        }
    }
}

