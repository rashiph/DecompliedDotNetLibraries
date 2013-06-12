namespace System.Resources
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Security;

    internal sealed class RuntimeResourceSet : ResourceSet, IEnumerable
    {
        private Dictionary<string, ResourceLocator> _caseInsensitiveTable;
        private ResourceReader _defaultReader;
        private bool _haveReadFromReader;
        private Dictionary<string, ResourceLocator> _resCache;
        internal const int Version = 2;

        [SecurityCritical]
        internal RuntimeResourceSet(Stream stream) : base(false)
        {
            this._resCache = new Dictionary<string, ResourceLocator>(FastResourceComparer.Default);
            this._defaultReader = new ResourceReader(stream, this._resCache);
            base.Reader = this._defaultReader;
        }

        [SecurityCritical]
        internal RuntimeResourceSet(string fileName) : base(false)
        {
            this._resCache = new Dictionary<string, ResourceLocator>(FastResourceComparer.Default);
            Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            this._defaultReader = new ResourceReader(stream, this._resCache);
            base.Reader = this._defaultReader;
        }

        protected override void Dispose(bool disposing)
        {
            if (base.Reader != null)
            {
                if (disposing)
                {
                    lock (base.Reader)
                    {
                        this._resCache = null;
                        if (this._defaultReader != null)
                        {
                            this._defaultReader.Close();
                            this._defaultReader = null;
                        }
                        this._caseInsensitiveTable = null;
                        base.Dispose(disposing);
                        return;
                    }
                }
                this._resCache = null;
                this._caseInsensitiveTable = null;
                this._defaultReader = null;
                base.Dispose(disposing);
            }
        }

        public override IDictionaryEnumerator GetEnumerator()
        {
            return this.GetEnumeratorHelper();
        }

        private IDictionaryEnumerator GetEnumeratorHelper()
        {
            IResourceReader reader = base.Reader;
            if ((reader == null) || (this._resCache == null))
            {
                throw new ObjectDisposedException(null, Environment.GetResourceString("ObjectDisposed_ResourceSet"));
            }
            return reader.GetEnumerator();
        }

        public override object GetObject(string key)
        {
            return this.GetObject(key, false, false);
        }

        public override object GetObject(string key, bool ignoreCase)
        {
            return this.GetObject(key, ignoreCase, false);
        }

        private object GetObject(string key, bool ignoreCase, bool isString)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if ((base.Reader == null) || (this._resCache == null))
            {
                throw new ObjectDisposedException(null, Environment.GetResourceString("ObjectDisposed_ResourceSet"));
            }
            object obj2 = null;
            lock (base.Reader)
            {
                ResourceLocator locator;
                if (base.Reader == null)
                {
                    throw new ObjectDisposedException(null, Environment.GetResourceString("ObjectDisposed_ResourceSet"));
                }
                if (this._defaultReader != null)
                {
                    int pos = -1;
                    if (this._resCache.TryGetValue(key, out locator))
                    {
                        obj2 = locator.Value;
                        pos = locator.DataPosition;
                    }
                    if ((pos == -1) && (obj2 == null))
                    {
                        pos = this._defaultReader.FindPosForResource(key);
                    }
                    if ((pos != -1) && (obj2 == null))
                    {
                        ResourceTypeCode code;
                        if (isString)
                        {
                            obj2 = this._defaultReader.LoadString(pos);
                            code = ResourceTypeCode.String;
                        }
                        else
                        {
                            obj2 = this._defaultReader.LoadObject(pos, out code);
                        }
                        locator = new ResourceLocator(pos, ResourceLocator.CanCache(code) ? obj2 : null);
                        lock (this._resCache)
                        {
                            this._resCache[key] = locator;
                        }
                    }
                    if ((obj2 != null) || !ignoreCase)
                    {
                        return obj2;
                    }
                }
                if (!this._haveReadFromReader)
                {
                    if (ignoreCase && (this._caseInsensitiveTable == null))
                    {
                        this._caseInsensitiveTable = new Dictionary<string, ResourceLocator>(StringComparer.OrdinalIgnoreCase);
                    }
                    if (this._defaultReader == null)
                    {
                        IDictionaryEnumerator enumerator = base.Reader.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            DictionaryEntry entry = enumerator.Entry;
                            string str = (string) entry.Key;
                            ResourceLocator locator2 = new ResourceLocator(-1, entry.Value);
                            this._resCache.Add(str, locator2);
                            if (ignoreCase)
                            {
                                this._caseInsensitiveTable.Add(str, locator2);
                            }
                        }
                        if (!ignoreCase)
                        {
                            base.Reader.Close();
                        }
                    }
                    else
                    {
                        ResourceReader.ResourceEnumerator enumeratorInternal = this._defaultReader.GetEnumeratorInternal();
                        while (enumeratorInternal.MoveNext())
                        {
                            string str2 = (string) enumeratorInternal.Key;
                            int dataPosition = enumeratorInternal.DataPosition;
                            ResourceLocator locator3 = new ResourceLocator(dataPosition, null);
                            this._caseInsensitiveTable.Add(str2, locator3);
                        }
                    }
                    this._haveReadFromReader = true;
                }
                object obj3 = null;
                bool flag2 = false;
                bool keyInWrongCase = false;
                if ((this._defaultReader != null) && this._resCache.TryGetValue(key, out locator))
                {
                    flag2 = true;
                    obj3 = this.ResolveResourceLocator(locator, key, this._resCache, keyInWrongCase);
                }
                if ((!flag2 && ignoreCase) && this._caseInsensitiveTable.TryGetValue(key, out locator))
                {
                    flag2 = true;
                    keyInWrongCase = true;
                    obj3 = this.ResolveResourceLocator(locator, key, this._resCache, keyInWrongCase);
                }
                return obj3;
            }
        }

        public override string GetString(string key)
        {
            return (string) this.GetObject(key, false, true);
        }

        public override string GetString(string key, bool ignoreCase)
        {
            return (string) this.GetObject(key, ignoreCase, true);
        }

        private object ResolveResourceLocator(ResourceLocator resLocation, string key, Dictionary<string, ResourceLocator> copyOfCache, bool keyInWrongCase)
        {
            object obj2 = resLocation.Value;
            if (obj2 == null)
            {
                ResourceTypeCode code;
                lock (base.Reader)
                {
                    obj2 = this._defaultReader.LoadObject(resLocation.DataPosition, out code);
                }
                if (!keyInWrongCase && ResourceLocator.CanCache(code))
                {
                    resLocation.Value = obj2;
                    copyOfCache[key] = resLocation;
                }
            }
            return obj2;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumeratorHelper();
        }
    }
}

