namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Deployment.Internal.Isolation.Manifest;
    using System.Runtime.InteropServices;
    using System.Security;

    internal class Store
    {
        private IStore _pStore;

        public Store(IStore pStore)
        {
            if (pStore == null)
            {
                throw new ArgumentNullException("pStore");
            }
            this._pStore = pStore;
        }

        [SecuritySafeCritical]
        public IDefinitionIdentity BindReferenceToAssemblyIdentity(uint Flags, IReferenceIdentity ReferenceIdentity, uint cDeploymentsToIgnore, IDefinitionIdentity[] DefinitionIdentity_DeploymentsToIgnore)
        {
            Guid riid = IsolationInterop.IID_IDefinitionIdentity;
            return (IDefinitionIdentity) this._pStore.BindReferenceToAssembly(Flags, ReferenceIdentity, cDeploymentsToIgnore, DefinitionIdentity_DeploymentsToIgnore, ref riid);
        }

        [SecuritySafeCritical]
        public ICMS BindReferenceToAssemblyManifest(uint Flags, IReferenceIdentity ReferenceIdentity, uint cDeploymentsToIgnore, IDefinitionIdentity[] DefinitionIdentity_DeploymentsToIgnore)
        {
            Guid riid = IsolationInterop.IID_ICMS;
            return (ICMS) this._pStore.BindReferenceToAssembly(Flags, ReferenceIdentity, cDeploymentsToIgnore, DefinitionIdentity_DeploymentsToIgnore, ref riid);
        }

        [SecuritySafeCritical]
        public void CalculateDelimiterOfDeploymentsBasedOnQuota(uint dwFlags, uint cDeployments, IDefinitionAppId[] rgpIDefinitionAppId_Deployments, ref StoreApplicationReference InstallerReference, ulong ulonglongQuota, ref uint Delimiter, ref ulong SizeSharedWithExternalDeployment, ref ulong SizeConsumedByInputDeploymentArray)
        {
            IntPtr zero = IntPtr.Zero;
            this._pStore.CalculateDelimiterOfDeploymentsBasedOnQuota(dwFlags, new IntPtr((long) cDeployments), rgpIDefinitionAppId_Deployments, ref InstallerReference, ulonglongQuota, ref zero, ref SizeSharedWithExternalDeployment, ref SizeConsumedByInputDeploymentArray);
            Delimiter = (uint) zero.ToInt64();
        }

        public StoreAssemblyEnumeration EnumAssemblies(EnumAssembliesFlags Flags)
        {
            return this.EnumAssemblies(Flags, null);
        }

        [SecuritySafeCritical]
        public StoreAssemblyEnumeration EnumAssemblies(EnumAssembliesFlags Flags, IReferenceIdentity refToMatch)
        {
            Guid guidOfType = IsolationInterop.GetGuidOfType(typeof(IEnumSTORE_ASSEMBLY));
            return new StoreAssemblyEnumeration((IEnumSTORE_ASSEMBLY) this._pStore.EnumAssemblies((uint) Flags, refToMatch, ref guidOfType));
        }

        [SecuritySafeCritical]
        public StoreCategoryEnumeration EnumCategories(EnumCategoriesFlags Flags, IReferenceIdentity CategoryMatch)
        {
            Guid guidOfType = IsolationInterop.GetGuidOfType(typeof(IEnumSTORE_CATEGORY));
            return new StoreCategoryEnumeration((IEnumSTORE_CATEGORY) this._pStore.EnumCategories((uint) Flags, CategoryMatch, ref guidOfType));
        }

        [SecuritySafeCritical]
        public StoreCategoryInstanceEnumeration EnumCategoryInstances(EnumCategoryInstancesFlags Flags, IDefinitionIdentity Category, string SubCat)
        {
            Guid guidOfType = IsolationInterop.GetGuidOfType(typeof(IEnumSTORE_CATEGORY_INSTANCE));
            return new StoreCategoryInstanceEnumeration((IEnumSTORE_CATEGORY_INSTANCE) this._pStore.EnumCategoryInstances((uint) Flags, Category, SubCat, ref guidOfType));
        }

        [SecuritySafeCritical]
        public StoreAssemblyFileEnumeration EnumFiles(EnumAssemblyFilesFlags Flags, IDefinitionIdentity Assembly)
        {
            Guid guidOfType = IsolationInterop.GetGuidOfType(typeof(IEnumSTORE_ASSEMBLY_FILE));
            return new StoreAssemblyFileEnumeration((IEnumSTORE_ASSEMBLY_FILE) this._pStore.EnumFiles((uint) Flags, Assembly, ref guidOfType));
        }

        [SecuritySafeCritical]
        public IEnumSTORE_ASSEMBLY_INSTALLATION_REFERENCE EnumInstallationReferences(EnumAssemblyInstallReferenceFlags Flags, IDefinitionIdentity Assembly)
        {
            Guid guidOfType = IsolationInterop.GetGuidOfType(typeof(IEnumSTORE_ASSEMBLY_INSTALLATION_REFERENCE));
            return (IEnumSTORE_ASSEMBLY_INSTALLATION_REFERENCE) this._pStore.EnumInstallationReferences((uint) Flags, Assembly, ref guidOfType);
        }

        [SecuritySafeCritical]
        public StoreDeploymentMetadataPropertyEnumeration EnumInstallerDeploymentProperties(Guid InstallerId, string InstallerName, string InstallerMetadata, IDefinitionAppId Deployment)
        {
            StoreApplicationReference reference = new StoreApplicationReference(InstallerId, InstallerName, InstallerMetadata);
            return new StoreDeploymentMetadataPropertyEnumeration((IEnumSTORE_DEPLOYMENT_METADATA_PROPERTY) this._pStore.EnumInstallerDeploymentMetadataProperties(0, ref reference, Deployment, ref IsolationInterop.IID_IEnumSTORE_DEPLOYMENT_METADATA_PROPERTY));
        }

        [SecuritySafeCritical]
        public StoreDeploymentMetadataEnumeration EnumInstallerDeployments(Guid InstallerId, string InstallerName, string InstallerMetadata, IReferenceAppId DeploymentFilter)
        {
            StoreApplicationReference reference = new StoreApplicationReference(InstallerId, InstallerName, InstallerMetadata);
            return new StoreDeploymentMetadataEnumeration((IEnumSTORE_DEPLOYMENT_METADATA) this._pStore.EnumInstallerDeploymentMetadata(0, ref reference, DeploymentFilter, ref IsolationInterop.IID_IEnumSTORE_DEPLOYMENT_METADATA));
        }

        [SecuritySafeCritical]
        public StoreAssemblyFileEnumeration EnumPrivateFiles(EnumApplicationPrivateFiles Flags, IDefinitionAppId Application, IDefinitionIdentity Assembly)
        {
            Guid guidOfType = IsolationInterop.GetGuidOfType(typeof(IEnumSTORE_ASSEMBLY_FILE));
            return new StoreAssemblyFileEnumeration((IEnumSTORE_ASSEMBLY_FILE) this._pStore.EnumPrivateFiles((uint) Flags, Application, Assembly, ref guidOfType));
        }

        public StoreSubcategoryEnumeration EnumSubcategories(EnumSubcategoriesFlags Flags, IDefinitionIdentity CategoryMatch)
        {
            return this.EnumSubcategories(Flags, CategoryMatch, null);
        }

        [SecuritySafeCritical]
        public StoreSubcategoryEnumeration EnumSubcategories(EnumSubcategoriesFlags Flags, IDefinitionIdentity Category, string SearchPattern)
        {
            Guid guidOfType = IsolationInterop.GetGuidOfType(typeof(IEnumSTORE_CATEGORY_SUBCATEGORY));
            return new StoreSubcategoryEnumeration((IEnumSTORE_CATEGORY_SUBCATEGORY) this._pStore.EnumSubcategories((uint) Flags, Category, SearchPattern, ref guidOfType));
        }

        [SecuritySafeCritical]
        public IDefinitionIdentity GetAssemblyIdentity(uint Flags, IDefinitionIdentity DefinitionIdentity)
        {
            Guid riid = IsolationInterop.IID_IDefinitionIdentity;
            return (IDefinitionIdentity) this._pStore.GetAssemblyInformation(Flags, DefinitionIdentity, ref riid);
        }

        [SecuritySafeCritical]
        public ICMS GetAssemblyManifest(uint Flags, IDefinitionIdentity DefinitionIdentity)
        {
            Guid riid = IsolationInterop.IID_ICMS;
            return (ICMS) this._pStore.GetAssemblyInformation(Flags, DefinitionIdentity, ref riid);
        }

        [SecurityCritical]
        public byte[] GetDeploymentProperty(GetPackagePropertyFlags Flags, IDefinitionAppId Deployment, StoreApplicationReference Reference, Guid PropertySet, string PropertyName)
        {
            BLOB blob = new BLOB();
            byte[] destination = null;
            try
            {
                this._pStore.GetDeploymentProperty((uint) Flags, Deployment, ref Reference, ref PropertySet, PropertyName, out blob);
                destination = new byte[blob.Size];
                Marshal.Copy(blob.BlobData, destination, 0, (int) blob.Size);
            }
            finally
            {
                blob.Dispose();
            }
            return destination;
        }

        [SecuritySafeCritical]
        public IPathLock LockApplicationPath(IDefinitionAppId app)
        {
            IntPtr ptr;
            return new ApplicationPathLock(this._pStore, ptr, this._pStore.LockApplicationPath(0, app, out ptr));
        }

        [SecuritySafeCritical]
        public IPathLock LockAssemblyPath(IDefinitionIdentity asm)
        {
            IntPtr ptr;
            return new AssemblyPathLock(this._pStore, ptr, this._pStore.LockAssemblyPath(0, asm, out ptr));
        }

        [SecuritySafeCritical]
        public ulong QueryChangeID(IDefinitionIdentity asm)
        {
            return this._pStore.QueryChangeID(asm);
        }

        [SecuritySafeCritical]
        public uint[] Transact(StoreTransactionOperation[] operations)
        {
            if ((operations == null) || (operations.Length == 0))
            {
                throw new ArgumentException("operations");
            }
            uint[] rgDispositions = new uint[operations.Length];
            int[] rgResults = new int[operations.Length];
            this._pStore.Transact(new IntPtr(operations.Length), operations, rgDispositions, rgResults);
            return rgDispositions;
        }

        public IStore InternalStore
        {
            get
            {
                return this._pStore;
            }
        }

        private class ApplicationPathLock : Store.IPathLock, IDisposable
        {
            private string _path;
            private IntPtr _pLockCookie = IntPtr.Zero;
            private IStore _pSourceStore;

            public ApplicationPathLock(IStore s, IntPtr c, string path)
            {
                this._pSourceStore = s;
                this._pLockCookie = c;
                this._path = path;
            }

            [SecuritySafeCritical]
            private void Dispose(bool fDisposing)
            {
                if (fDisposing)
                {
                    GC.SuppressFinalize(this);
                }
                if (this._pLockCookie != IntPtr.Zero)
                {
                    this._pSourceStore.ReleaseApplicationPath(this._pLockCookie);
                    this._pLockCookie = IntPtr.Zero;
                }
            }

            ~ApplicationPathLock()
            {
                this.Dispose(false);
            }

            void IDisposable.Dispose()
            {
                this.Dispose(true);
            }

            public string Path
            {
                get
                {
                    return this._path;
                }
            }
        }

        private class AssemblyPathLock : Store.IPathLock, IDisposable
        {
            private string _path;
            private IntPtr _pLockCookie = IntPtr.Zero;
            private IStore _pSourceStore;

            public AssemblyPathLock(IStore s, IntPtr c, string path)
            {
                this._pSourceStore = s;
                this._pLockCookie = c;
                this._path = path;
            }

            [SecuritySafeCritical]
            private void Dispose(bool fDisposing)
            {
                if (fDisposing)
                {
                    GC.SuppressFinalize(this);
                }
                if (this._pLockCookie != IntPtr.Zero)
                {
                    this._pSourceStore.ReleaseAssemblyPath(this._pLockCookie);
                    this._pLockCookie = IntPtr.Zero;
                }
            }

            ~AssemblyPathLock()
            {
                this.Dispose(false);
            }

            void IDisposable.Dispose()
            {
                this.Dispose(true);
            }

            public string Path
            {
                get
                {
                    return this._path;
                }
            }
        }

        [Flags]
        public enum EnumApplicationPrivateFiles
        {
            Nothing,
            IncludeInstalled,
            IncludeMissing
        }

        [Flags]
        public enum EnumAssembliesFlags
        {
            ForceLibrarySemantics = 4,
            MatchServicing = 2,
            Nothing = 0,
            VisibleOnly = 1
        }

        [Flags]
        public enum EnumAssemblyFilesFlags
        {
            Nothing,
            IncludeInstalled,
            IncludeMissing
        }

        [Flags]
        public enum EnumAssemblyInstallReferenceFlags
        {
            Nothing
        }

        [Flags]
        public enum EnumCategoriesFlags
        {
            Nothing
        }

        [Flags]
        public enum EnumCategoryInstancesFlags
        {
            Nothing
        }

        [Flags]
        public enum EnumSubcategoriesFlags
        {
            Nothing
        }

        [Flags]
        public enum GetPackagePropertyFlags
        {
            Nothing
        }

        public interface IPathLock : IDisposable
        {
            string Path { get; }
        }
    }
}

