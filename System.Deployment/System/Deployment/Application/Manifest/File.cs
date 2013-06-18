namespace System.Deployment.Application.Manifest
{
    using System;
    using System.Deployment.Application;
    using System.Deployment.Internal.Isolation;
    using System.Deployment.Internal.Isolation.Manifest;
    using System.Runtime.InteropServices;

    internal class File
    {
        private readonly string _group;
        private System.Deployment.Application.HashCollection _hashCollection;
        private readonly bool _isData;
        private readonly string _loadFrom;
        private readonly string _name;
        private readonly string _nameFS;
        private readonly bool _optional;
        private readonly ulong _size;

        public File(System.Deployment.Internal.Isolation.Manifest.FileEntry fileEntry)
        {
            this._hashCollection = new System.Deployment.Application.HashCollection();
            this._name = fileEntry.Name;
            this._loadFrom = fileEntry.LoadFrom;
            this._size = fileEntry.Size;
            this._group = fileEntry.Group;
            this._optional = (fileEntry.Flags & 1) != 0;
            this._isData = (fileEntry.WritableType & 2) != 0;
            bool flag = false;
            System.Deployment.Internal.Isolation.ISection hashElements = fileEntry.HashElements;
            uint celt = (hashElements != null) ? hashElements.Count : 0;
            if (celt > 0)
            {
                uint celtFetched = 0;
                System.Deployment.Internal.Isolation.Manifest.IHashElementEntry[] rgelt = new System.Deployment.Internal.Isolation.Manifest.IHashElementEntry[celt];
                System.Deployment.Internal.Isolation.IEnumUnknown unknown = (System.Deployment.Internal.Isolation.IEnumUnknown) hashElements._NewEnum;
                Marshal.ThrowExceptionForHR(unknown.Next(celt, rgelt, ref celtFetched));
                if (celtFetched != celt)
                {
                    throw new InvalidDeploymentException(ExceptionTypes.Manifest, Resources.GetString("Ex_IsoEnumFetchNotEqualToCount"));
                }
                for (uint i = 0; i < celt; i++)
                {
                    System.Deployment.Internal.Isolation.Manifest.HashElementEntry allData = rgelt[i].AllData;
                    if (allData.DigestValueSize > 0)
                    {
                        byte[] destination = new byte[allData.DigestValueSize];
                        Marshal.Copy(allData.DigestValue, destination, 0, (int) allData.DigestValueSize);
                        this._hashCollection.AddHash(destination, (System.Deployment.Internal.Isolation.Manifest.CMS_HASH_DIGESTMETHOD) allData.DigestMethod, (System.Deployment.Internal.Isolation.Manifest.CMS_HASH_TRANSFORM) allData.Transform);
                        flag = true;
                    }
                }
            }
            if (!flag && (fileEntry.HashValueSize > 0))
            {
                byte[] buffer2 = new byte[fileEntry.HashValueSize];
                Marshal.Copy(fileEntry.HashValue, buffer2, 0, (int) fileEntry.HashValueSize);
                this._hashCollection.AddHash(buffer2, (System.Deployment.Internal.Isolation.Manifest.CMS_HASH_DIGESTMETHOD) fileEntry.HashAlgorithm, System.Deployment.Internal.Isolation.Manifest.CMS_HASH_TRANSFORM.CMS_HASH_TRANSFORM_IDENTITY);
            }
            this._nameFS = UriHelper.NormalizePathDirectorySeparators(this._name);
        }

        protected internal File(string name, ulong size)
        {
            this._hashCollection = new System.Deployment.Application.HashCollection();
            this._name = name;
            this._size = size;
            this._nameFS = UriHelper.NormalizePathDirectorySeparators(this._name);
        }

        public File(string name, byte[] hash, ulong size)
        {
            this._hashCollection = new System.Deployment.Application.HashCollection();
            this._name = name;
            this._hashCollection.AddHash(hash, System.Deployment.Internal.Isolation.Manifest.CMS_HASH_DIGESTMETHOD.CMS_HASH_DIGESTMETHOD_SHA1, System.Deployment.Internal.Isolation.Manifest.CMS_HASH_TRANSFORM.CMS_HASH_TRANSFORM_IDENTITY);
            this._size = size;
            this._nameFS = UriHelper.NormalizePathDirectorySeparators(this._name);
        }

        public string Group
        {
            get
            {
                return this._group;
            }
        }

        public System.Deployment.Application.HashCollection HashCollection
        {
            get
            {
                return this._hashCollection;
            }
        }

        public bool IsData
        {
            get
            {
                return this._isData;
            }
        }

        public bool IsOptional
        {
            get
            {
                return this._optional;
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
        }

        public string NameFS
        {
            get
            {
                return this._nameFS;
            }
        }

        public ulong Size
        {
            get
            {
                return this._size;
            }
        }
    }
}

