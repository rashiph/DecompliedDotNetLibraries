namespace System.Resources
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class ResourceSet : IDisposable, IEnumerable
    {
        private Hashtable _caseInsensitiveTable;
        [NonSerialized]
        protected IResourceReader Reader;
        protected Hashtable Table;

        protected ResourceSet()
        {
            this.CommonInit();
        }

        internal ResourceSet(bool junk)
        {
        }

        [SecurityCritical]
        public ResourceSet(Stream stream)
        {
            this.Reader = new ResourceReader(stream);
            this.CommonInit();
            this.ReadResources();
        }

        public ResourceSet(IResourceReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            this.Reader = reader;
            this.CommonInit();
            this.ReadResources();
        }

        [SecuritySafeCritical]
        public ResourceSet(string fileName)
        {
            this.Reader = new ResourceReader(fileName);
            this.CommonInit();
            this.ReadResources();
        }

        public virtual void Close()
        {
            this.Dispose(true);
        }

        private void CommonInit()
        {
            this.Table = new Hashtable();
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                IResourceReader reader = this.Reader;
                this.Reader = null;
                if (reader != null)
                {
                    reader.Close();
                }
            }
            this.Reader = null;
            this._caseInsensitiveTable = null;
            this.Table = null;
        }

        private object GetCaseInsensitiveObjectInternal(string name)
        {
            Hashtable table = this.Table;
            if (table == null)
            {
                throw new ObjectDisposedException(null, Environment.GetResourceString("ObjectDisposed_ResourceSet"));
            }
            Hashtable hashtable2 = this._caseInsensitiveTable;
            if (hashtable2 == null)
            {
                hashtable2 = new Hashtable(StringComparer.OrdinalIgnoreCase);
                IDictionaryEnumerator enumerator = table.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    hashtable2.Add(enumerator.Key, enumerator.Value);
                }
                this._caseInsensitiveTable = hashtable2;
            }
            return hashtable2[name];
        }

        public virtual Type GetDefaultReader()
        {
            return typeof(ResourceReader);
        }

        public virtual Type GetDefaultWriter()
        {
            return typeof(ResourceWriter);
        }

        [ComVisible(false)]
        public virtual IDictionaryEnumerator GetEnumerator()
        {
            return this.GetEnumeratorHelper();
        }

        private IDictionaryEnumerator GetEnumeratorHelper()
        {
            Hashtable table = this.Table;
            if (table == null)
            {
                throw new ObjectDisposedException(null, Environment.GetResourceString("ObjectDisposed_ResourceSet"));
            }
            return table.GetEnumerator();
        }

        public virtual object GetObject(string name)
        {
            return this.GetObjectInternal(name);
        }

        public virtual object GetObject(string name, bool ignoreCase)
        {
            object objectInternal = this.GetObjectInternal(name);
            if ((objectInternal == null) && ignoreCase)
            {
                return this.GetCaseInsensitiveObjectInternal(name);
            }
            return objectInternal;
        }

        private object GetObjectInternal(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            Hashtable table = this.Table;
            if (table == null)
            {
                throw new ObjectDisposedException(null, Environment.GetResourceString("ObjectDisposed_ResourceSet"));
            }
            return table[name];
        }

        public virtual string GetString(string name)
        {
            string str;
            object objectInternal = this.GetObjectInternal(name);
            try
            {
                str = (string) objectInternal;
            }
            catch (InvalidCastException)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ResourceNotString_Name", new object[] { name }));
            }
            return str;
        }

        public virtual string GetString(string name, bool ignoreCase)
        {
            string str;
            string str2;
            object objectInternal = this.GetObjectInternal(name);
            try
            {
                str = (string) objectInternal;
            }
            catch (InvalidCastException)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ResourceNotString_Name", new object[] { name }));
            }
            if ((str != null) || !ignoreCase)
            {
                return str;
            }
            objectInternal = this.GetCaseInsensitiveObjectInternal(name);
            try
            {
                str2 = (string) objectInternal;
            }
            catch (InvalidCastException)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ResourceNotString_Name", new object[] { name }));
            }
            return str2;
        }

        protected virtual void ReadResources()
        {
            IDictionaryEnumerator enumerator = this.Reader.GetEnumerator();
            while (enumerator.MoveNext())
            {
                object obj2 = enumerator.Value;
                this.Table.Add(enumerator.Key, obj2);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumeratorHelper();
        }
    }
}

