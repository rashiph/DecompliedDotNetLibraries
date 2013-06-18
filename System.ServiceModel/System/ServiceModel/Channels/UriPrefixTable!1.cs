namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.Collections;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    internal sealed class UriPrefixTable<TItem> where TItem: class
    {
        private int count;
        private const int HopperSize = 0x80;
        private bool includePortInComparison;
        private volatile HopperCache lookupCache;
        private SegmentHierarchyNode<TItem> root;
        private bool useWeakReferences;

        public UriPrefixTable() : this(false)
        {
        }

        public UriPrefixTable(bool includePortInComparison) : this(includePortInComparison, false)
        {
        }

        internal UriPrefixTable(UriPrefixTable<TItem> objectToClone) : this(objectToClone.includePortInComparison, objectToClone.useWeakReferences)
        {
            if (objectToClone.Count > 0)
            {
                foreach (KeyValuePair<BaseUriWithWildcard, TItem> pair in objectToClone.GetAll())
                {
                    this.RegisterUri(pair.Key.BaseAddress, pair.Key.HostNameComparisonMode, pair.Value);
                }
            }
        }

        public UriPrefixTable(bool includePortInComparison, bool useWeakReferences)
        {
            this.includePortInComparison = includePortInComparison;
            this.useWeakReferences = useWeakReferences;
            this.root = new SegmentHierarchyNode<TItem>(null, useWeakReferences);
            this.lookupCache = new HopperCache(0x80, useWeakReferences);
        }

        private void AddToCache(BaseUriWithWildcard key, TItem item)
        {
            if (item == null)
            {
            }
            this.lookupCache.Add(key, (TItem) DBNull.Value);
        }

        private void ClearCache()
        {
            this.lookupCache = new HopperCache(0x80, this.useWeakReferences);
        }

        private SegmentHierarchyNode<TItem> FindDataNode(string[] path, out bool exactMatch)
        {
            exactMatch = false;
            SegmentHierarchyNode<TItem> root = this.root;
            SegmentHierarchyNode<TItem> node2 = null;
            for (int i = 0; i < path.Length; i++)
            {
                SegmentHierarchyNode<TItem> node3;
                if (!root.TryGetChild(path[i], out node3))
                {
                    return node2;
                }
                if (node3.Data != null)
                {
                    node2 = node3;
                    exactMatch = i == (path.Length - 1);
                }
                root = node3;
            }
            return node2;
        }

        private SegmentHierarchyNode<TItem> FindOrCreateNode(BaseUriWithWildcard baseUri)
        {
            string[] strArray = UriSegmenter<TItem>.ToPath(baseUri.BaseAddress, baseUri.HostNameComparisonMode, this.includePortInComparison);
            SegmentHierarchyNode<TItem> root = this.root;
            for (int i = 0; i < strArray.Length; i++)
            {
                SegmentHierarchyNode<TItem> node2;
                if (!root.TryGetChild(strArray[i], out node2))
                {
                    node2 = new SegmentHierarchyNode<TItem>(strArray[i], this.useWeakReferences);
                    root.SetChildNode(strArray[i], node2);
                }
                root = node2;
            }
            return root;
        }

        public IEnumerable<KeyValuePair<BaseUriWithWildcard, TItem>> GetAll()
        {
            lock (this.ThisLock)
            {
                List<KeyValuePair<BaseUriWithWildcard, TItem>> result = new List<KeyValuePair<BaseUriWithWildcard, TItem>>();
                this.root.Collect(result);
                return result;
            }
        }

        public bool IsRegistered(BaseUriWithWildcard key)
        {
            bool flag;
            SegmentHierarchyNode<TItem> node;
            string[] path = UriSegmenter<TItem>.ToPath(key.BaseAddress, key.HostNameComparisonMode, this.includePortInComparison);
            lock (this.ThisLock)
            {
                node = this.FindDataNode(path, out flag);
            }
            return ((flag && (node != null)) && (node.Data != null));
        }

        public void RegisterUri(Uri uri, HostNameComparisonMode hostNameComparisonMode, TItem item)
        {
            lock (this.ThisLock)
            {
                this.ClearCache();
                BaseUriWithWildcard baseUri = new BaseUriWithWildcard(uri, hostNameComparisonMode);
                SegmentHierarchyNode<TItem> node = this.FindOrCreateNode(baseUri);
                if (node.Data != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("DuplicateRegistration", new object[] { uri })));
                }
                node.SetData(item, baseUri);
                this.count++;
            }
        }

        private bool TryCacheLookup(BaseUriWithWildcard key, out TItem item)
        {
            object obj2 = this.lookupCache.GetValue(this.ThisLock, key);
            item = (obj2 == DBNull.Value) ? default(TItem) : ((TItem) obj2);
            return (obj2 != null);
        }

        public bool TryLookupUri(Uri uri, HostNameComparisonMode hostNameComparisonMode, out TItem item)
        {
            BaseUriWithWildcard key = new BaseUriWithWildcard(uri, hostNameComparisonMode);
            if (this.TryCacheLookup(key, out item))
            {
                return (((TItem) item) != null);
            }
            lock (this.ThisLock)
            {
                bool flag;
                SegmentHierarchyNode<TItem> node = this.FindDataNode(UriSegmenter<TItem>.ToPath(key.BaseAddress, hostNameComparisonMode, this.includePortInComparison), out flag);
                if (node != null)
                {
                    item = node.Data;
                }
                this.AddToCache(key, item);
                return (((TItem) item) != null);
            }
        }

        public void UnregisterUri(Uri uri, HostNameComparisonMode hostNameComparisonMode)
        {
            lock (this.ThisLock)
            {
                this.ClearCache();
                string[] path = UriSegmenter<TItem>.ToPath(uri, hostNameComparisonMode, this.includePortInComparison);
                if (path.Length == 0)
                {
                    this.root.RemoveData();
                }
                else
                {
                    this.root.RemovePath(path, 0);
                }
                this.count--;
            }
        }

        public int Count
        {
            get
            {
                return this.count;
            }
        }

        private object ThisLock
        {
            get
            {
                return this;
            }
        }

        private static class UriSegmenter
        {
            internal static string[] ToPath(Uri uriPath, HostNameComparisonMode hostNameComparisonMode, bool includePortInComparison)
            {
                if (null == uriPath)
                {
                    return new string[0];
                }
                UriSegmentEnum<TItem> enum2 = new UriSegmentEnum<TItem>(uriPath);
                return enum2.GetSegments(hostNameComparisonMode, includePortInComparison);
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct UriSegmentEnum
            {
                private string segment;
                private int segmentStartAt;
                private int segmentLength;
                private UriSegmentType<TItem> type;
                private Uri uri;
                internal UriSegmentEnum(Uri uri)
                {
                    this.uri = uri;
                    this.type = UriSegmentType<TItem>.Unknown;
                    this.segment = null;
                    this.segmentStartAt = 0;
                    this.segmentLength = 0;
                }

                private void ClearSegment()
                {
                    this.type = UriSegmentType<TItem>.None;
                    this.segment = string.Empty;
                    this.segmentStartAt = 0;
                    this.segmentLength = 0;
                }

                public string[] GetSegments(HostNameComparisonMode hostNameComparisonMode, bool includePortInComparison)
                {
                    List<string> list = new List<string>();
                    while (this.Next())
                    {
                        switch (this.type)
                        {
                            case UriSegmentType<TItem>.Host:
                            {
                                if (hostNameComparisonMode != HostNameComparisonMode.StrongWildcard)
                                {
                                    break;
                                }
                                list.Add("+");
                                continue;
                            }
                            case UriSegmentType<TItem>.Port:
                            {
                                if (includePortInComparison || (hostNameComparisonMode == HostNameComparisonMode.Exact))
                                {
                                    list.Add(this.segment);
                                }
                                continue;
                            }
                            case UriSegmentType<TItem>.Path:
                            {
                                list.Add(this.segment.Substring(this.segmentStartAt, this.segmentLength));
                                continue;
                            }
                            default:
                                goto Label_008B;
                        }
                        if (hostNameComparisonMode == HostNameComparisonMode.Exact)
                        {
                            list.Add(this.segment);
                        }
                        else
                        {
                            list.Add("*");
                        }
                        continue;
                    Label_008B:
                        list.Add(this.segment);
                    }
                    return list.ToArray();
                }

                public bool Next()
                {
                    switch (this.type)
                    {
                        case UriSegmentType<TItem>.Unknown:
                            this.type = UriSegmentType<TItem>.Scheme;
                            this.SetSegment(this.uri.Scheme);
                            return true;

                        case UriSegmentType<TItem>.Scheme:
                        {
                            this.type = UriSegmentType<TItem>.Host;
                            string host = this.uri.Host;
                            string userInfo = this.uri.UserInfo;
                            if ((userInfo != null) && (userInfo.Length > 0))
                            {
                                host = userInfo + '@' + host;
                            }
                            this.SetSegment(host);
                            return true;
                        }
                        case UriSegmentType<TItem>.Host:
                            this.type = UriSegmentType<TItem>.Port;
                            this.SetSegment(this.uri.Port.ToString(CultureInfo.InvariantCulture));
                            return true;

                        case UriSegmentType<TItem>.Port:
                        {
                            this.type = UriSegmentType<TItem>.Path;
                            string absolutePath = this.uri.AbsolutePath;
                            if (absolutePath.Length != 0)
                            {
                                this.segment = absolutePath;
                                this.segmentStartAt = 0;
                                this.segmentLength = 0;
                                return this.NextPathSegment();
                            }
                            this.ClearSegment();
                            return false;
                        }
                        case UriSegmentType<TItem>.Path:
                            return this.NextPathSegment();

                        case UriSegmentType<TItem>.None:
                            return false;
                    }
                    return false;
                }

                public bool NextPathSegment()
                {
                    this.segmentStartAt += this.segmentLength;
                    while ((this.segmentStartAt < this.segment.Length) && (this.segment[this.segmentStartAt] == '/'))
                    {
                        this.segmentStartAt++;
                    }
                    if (this.segmentStartAt < this.segment.Length)
                    {
                        int index = this.segment.IndexOf('/', this.segmentStartAt);
                        if (-1 == index)
                        {
                            this.segmentLength = this.segment.Length - this.segmentStartAt;
                        }
                        else
                        {
                            this.segmentLength = index - this.segmentStartAt;
                        }
                        return true;
                    }
                    this.ClearSegment();
                    return false;
                }

                private void SetSegment(string segment)
                {
                    this.segment = segment;
                    this.segmentStartAt = 0;
                    this.segmentLength = segment.Length;
                }
                private enum UriSegmentType
                {
                    public const UriPrefixTable<TItem>.UriSegmenter.UriSegmentEnum.UriSegmentType Host = UriPrefixTable<TItem>.UriSegmenter.UriSegmentEnum.UriSegmentType.Host;,
                    public const UriPrefixTable<TItem>.UriSegmenter.UriSegmentEnum.UriSegmentType None = UriPrefixTable<TItem>.UriSegmenter.UriSegmentEnum.UriSegmentType.None;,
                    public const UriPrefixTable<TItem>.UriSegmenter.UriSegmentEnum.UriSegmentType Path = UriPrefixTable<TItem>.UriSegmenter.UriSegmentEnum.UriSegmentType.Path;,
                    public const UriPrefixTable<TItem>.UriSegmenter.UriSegmentEnum.UriSegmentType Port = UriPrefixTable<TItem>.UriSegmenter.UriSegmentEnum.UriSegmentType.Port;,
                    public const UriPrefixTable<TItem>.UriSegmenter.UriSegmentEnum.UriSegmentType Scheme = UriPrefixTable<TItem>.UriSegmenter.UriSegmentEnum.UriSegmentType.Scheme;,
                    public const UriPrefixTable<TItem>.UriSegmenter.UriSegmentEnum.UriSegmentType Unknown = UriPrefixTable<TItem>.UriSegmenter.UriSegmentEnum.UriSegmentType.Unknown;
                }
            }
        }
    }
}

