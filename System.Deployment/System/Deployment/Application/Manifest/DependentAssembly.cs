namespace System.Deployment.Application.Manifest
{
    using System;
    using System.Deployment.Application;
    using System.Deployment.Internal.Isolation;
    using System.Deployment.Internal.Isolation.Manifest;
    using System.Runtime.InteropServices;

    internal class DependentAssembly
    {
        private readonly string _codebase;
        private readonly string _codebaseFS;
        private readonly string _description;
        private readonly string _group;
        private System.Deployment.Application.HashCollection _hashCollection;
        private readonly System.Deployment.Application.ReferenceIdentity _identity;
        private readonly bool _optional;
        private readonly bool _preRequisite;
        private readonly string _resourceFallbackCulture;
        private readonly bool _resourceFallbackCultureInternal;
        private readonly ulong _size;
        private readonly Uri _supportUrl;
        private readonly bool _visible;

        public DependentAssembly(System.Deployment.Application.ReferenceIdentity refId)
        {
            this._hashCollection = new System.Deployment.Application.HashCollection();
            this._identity = refId;
        }

        public DependentAssembly(System.Deployment.Internal.Isolation.Manifest.AssemblyReferenceEntry assemblyReferenceEntry)
        {
            this._hashCollection = new System.Deployment.Application.HashCollection();
            System.Deployment.Internal.Isolation.Manifest.AssemblyReferenceDependentAssemblyEntry dependentAssembly = assemblyReferenceEntry.DependentAssembly;
            this._size = dependentAssembly.Size;
            this._codebase = dependentAssembly.Codebase;
            this._group = dependentAssembly.Group;
            bool flag = false;
            System.Deployment.Internal.Isolation.ISection hashElements = dependentAssembly.HashElements;
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
            if (!flag && (dependentAssembly.HashValueSize > 0))
            {
                byte[] buffer2 = new byte[dependentAssembly.HashValueSize];
                Marshal.Copy(dependentAssembly.HashValue, buffer2, 0, (int) dependentAssembly.HashValueSize);
                this._hashCollection.AddHash(buffer2, (System.Deployment.Internal.Isolation.Manifest.CMS_HASH_DIGESTMETHOD) dependentAssembly.HashAlgorithm, System.Deployment.Internal.Isolation.Manifest.CMS_HASH_TRANSFORM.CMS_HASH_TRANSFORM_IDENTITY);
            }
            this._preRequisite = (dependentAssembly.Flags & 4) != 0;
            this._optional = (assemblyReferenceEntry.Flags & 1) != 0;
            this._visible = (dependentAssembly.Flags & 2) != 0;
            this._resourceFallbackCultureInternal = (dependentAssembly.Flags & 8) != 0;
            this._resourceFallbackCulture = dependentAssembly.ResourceFallbackCulture;
            this._description = dependentAssembly.Description;
            this._supportUrl = AssemblyManifest.UriFromMetadataEntry(dependentAssembly.SupportUrl, "Ex_DependencySupportUrlNotValid");
            System.Deployment.Internal.Isolation.IReferenceIdentity referenceIdentity = assemblyReferenceEntry.ReferenceIdentity;
            this._identity = new System.Deployment.Application.ReferenceIdentity(referenceIdentity);
            this._codebaseFS = UriHelper.NormalizePathDirectorySeparators(this._codebase);
        }

        public string Codebase
        {
            get
            {
                return this._codebase;
            }
        }

        public string CodebaseFS
        {
            get
            {
                return this._codebaseFS;
            }
        }

        public string Description
        {
            get
            {
                return this._description;
            }
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

        public System.Deployment.Application.ReferenceIdentity Identity
        {
            get
            {
                return this._identity;
            }
        }

        public bool IsOptional
        {
            get
            {
                return this._optional;
            }
        }

        public bool IsPreRequisite
        {
            get
            {
                return this._preRequisite;
            }
        }

        public string ResourceFallbackCulture
        {
            get
            {
                return this._resourceFallbackCulture;
            }
        }

        public ulong Size
        {
            get
            {
                return this._size;
            }
        }

        public Uri SupportUrl
        {
            get
            {
                return this._supportUrl;
            }
        }
    }
}

