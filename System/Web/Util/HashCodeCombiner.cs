namespace System.Web.Util
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Web;
    using System.Web.UI;

    internal class HashCodeCombiner
    {
        private long _combinedHash;

        internal HashCodeCombiner()
        {
            this._combinedHash = 0x1505L;
        }

        internal HashCodeCombiner(long initialCombinedHash)
        {
            this._combinedHash = initialCombinedHash;
        }

        internal void AddArray(string[] a)
        {
            if (a != null)
            {
                int length = a.Length;
                for (int i = 0; i < length; i++)
                {
                    this.AddObject(a[i]);
                }
            }
        }

        internal void AddCaseInsensitiveString(string s)
        {
            if (s != null)
            {
                this.AddInt(StringComparer.InvariantCultureIgnoreCase.GetHashCode(s));
            }
        }

        internal void AddDateTime(DateTime dt)
        {
            this.AddInt(dt.GetHashCode());
        }

        internal void AddDirectory(string directoryName)
        {
            DirectoryInfo info = new DirectoryInfo(directoryName);
            if (info.Exists)
            {
                this.AddObject(directoryName);
                foreach (FileData data in (IEnumerable) FileEnumerator.Create(directoryName))
                {
                    if (data.IsDirectory)
                    {
                        this.AddDirectory(data.FullName);
                    }
                    else
                    {
                        this.AddExistingFile(data.FullName);
                    }
                }
                this.AddDateTime(info.CreationTimeUtc);
                this.AddDateTime(info.LastWriteTimeUtc);
            }
        }

        private void AddExistingFile(string fileName)
        {
            this.AddInt(StringUtil.GetStringHashCode(fileName));
            FileInfo info = new FileInfo(fileName);
            this.AddDateTime(info.CreationTimeUtc);
            this.AddDateTime(info.LastWriteTimeUtc);
            this.AddFileSize(info.Length);
        }

        internal void AddFile(string fileName)
        {
            if (!FileUtil.FileExists(fileName))
            {
                if (FileUtil.DirectoryExists(fileName))
                {
                    this.AddDirectory(fileName);
                }
            }
            else
            {
                this.AddExistingFile(fileName);
            }
        }

        private void AddFileSize(long fileSize)
        {
            this.AddInt(fileSize.GetHashCode());
        }

        internal void AddInt(int n)
        {
            this._combinedHash = ((this._combinedHash << 5) + this._combinedHash) ^ n;
        }

        internal void AddObject(bool b)
        {
            this.AddInt(b.GetHashCode());
        }

        internal void AddObject(byte b)
        {
            this.AddInt(b.GetHashCode());
        }

        internal void AddObject(int n)
        {
            this.AddInt(n);
        }

        internal void AddObject(long l)
        {
            this.AddInt(l.GetHashCode());
        }

        internal void AddObject(object o)
        {
            if (o != null)
            {
                this.AddInt(o.GetHashCode());
            }
        }

        internal void AddObject(string s)
        {
            if (s != null)
            {
                this.AddInt(StringUtil.GetStringHashCode(s));
            }
        }

        internal void AddObject(Type t)
        {
            if (t != null)
            {
                this.AddObject(Util.GetAssemblyQualifiedTypeName(t));
            }
        }

        internal void AddResourcesDirectory(string directoryName)
        {
            DirectoryInfo info = new DirectoryInfo(directoryName);
            if (info.Exists)
            {
                this.AddObject(directoryName);
                foreach (FileData data in (IEnumerable) FileEnumerator.Create(directoryName))
                {
                    if (data.IsDirectory)
                    {
                        this.AddResourcesDirectory(data.FullName);
                    }
                    else
                    {
                        string fullName = data.FullName;
                        if (Util.GetCultureName(fullName) == null)
                        {
                            this.AddExistingFile(fullName);
                        }
                    }
                }
                this.AddDateTime(info.CreationTimeUtc);
            }
        }

        internal static int CombineHashCodes(int h1, int h2)
        {
            return (((h1 << 5) + h1) ^ h2);
        }

        internal static int CombineHashCodes(int h1, int h2, int h3)
        {
            return CombineHashCodes(CombineHashCodes(h1, h2), h3);
        }

        internal static int CombineHashCodes(int h1, int h2, int h3, int h4)
        {
            return CombineHashCodes(CombineHashCodes(h1, h2), CombineHashCodes(h3, h4));
        }

        internal static int CombineHashCodes(int h1, int h2, int h3, int h4, int h5)
        {
            return CombineHashCodes(CombineHashCodes(h1, h2, h3, h4), h5);
        }

        internal static string GetDirectoryHash(VirtualPath virtualDir)
        {
            HashCodeCombiner combiner = new HashCodeCombiner();
            combiner.AddDirectory(virtualDir.MapPathInternal());
            return combiner.CombinedHashString;
        }

        internal long CombinedHash
        {
            get
            {
                return this._combinedHash;
            }
        }

        internal int CombinedHash32
        {
            get
            {
                return this._combinedHash.GetHashCode();
            }
        }

        internal string CombinedHashString
        {
            get
            {
                return this._combinedHash.ToString("x", CultureInfo.InvariantCulture);
            }
        }
    }
}

