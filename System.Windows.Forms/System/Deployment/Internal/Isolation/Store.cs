namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Deployment.Internal.Isolation.Manifest;
    using System.Runtime.InteropServices;
    using System.Security;

    internal class Store
    {
        private System.Deployment.Internal.Isolation.IStore _pStore;

        public Store(System.Deployment.Internal.Isolation.IStore pStore)
        {
            if (pStore == null)
            {
                throw new ArgumentNullException("pStore");
            }
            this._pStore = pStore;
        }

        [SecuritySafeCritical]
        public System.Deployment.Internal.Isolation.IDefinitionIdentity BindReferenceToAssemblyIdentity(uint Flags, System.Deployment.Internal.Isolation.IReferenceIdentity ReferenceIdentity, uint cDeploymentsToIgnore, System.Deployment.Internal.Isolation.IDefinitionIdentity[] DefinitionIdentity_DeploymentsToIgnore)
        {
            Guid riid = System.Deployment.Internal.Isolation.IsolationInterop.IID_IDefinitionIdentity;
            return (System.Deployment.Internal.Isolation.IDefinitionIdentity) this._pStore.BindReferenceToAssembly(Flags, ReferenceIdentity, cDeploymentsToIgnore, DefinitionIdentity_DeploymentsToIgnore, ref riid);
        }

        [SecuritySafeCritical]
        public System.Deployment.Internal.Isolation.Manifest.ICMS BindReferenceToAssemblyManifest(uint Flags, System.Deployment.Internal.Isolation.IReferenceIdentity ReferenceIdentity, uint cDeploymentsToIgnore, System.Deployment.Internal.Isolation.IDefinitionIdentity[] DefinitionIdentity_DeploymentsToIgnore)
        {
            Guid riid = System.Deployment.Internal.Isolation.IsolationInterop.IID_ICMS;
            return (System.Deployment.Internal.Isolation.Manifest.ICMS) this._pStore.BindReferenceToAssembly(Flags, ReferenceIdentity, cDeploymentsToIgnore, DefinitionIdentity_DeploymentsToIgnore, ref riid);
        }

        [SecuritySafeCritical]
        public void CalculateDelimiterOfDeploymentsBasedOnQuota(uint dwFlags, uint cDeployments, System.Deployment.Internal.Isolation.IDefinitionAppId[] rgpIDefinitionAppId_Deployments, ref System.Deployment.Internal.Isolation.StoreApplicationReference InstallerReference, ulong ulonglongQuota, ref uint Delimiter, ref ulong SizeSharedWithExternalDeployment, ref ulong SizeConsumedByInputDeploymentArray)
        {
            IntPtr zero = IntPtr.Zero;
            this._pStore.CalculateDelimiterOfDeploymentsBasedOnQuota(dwFlags, new IntPtr((long) cDeployments), rgpIDefinitionAppId_Deployments, ref InstallerReference, ulonglongQuota, ref zero, ref SizeSharedWithExternalDeployment, ref SizeConsumedByInputDeploymentArray);
            Delimiter = (uint) zero.ToInt64();
        }

        public System.Deployment.Internal.Isolation.StoreAssemblyEnumeration EnumAssemblies(EnumAssembliesFlags Flags)
        {
            return this.EnumAssemblies(Flags, null);
        }

        [SecuritySafeCritical]
        public System.Deployment.Internal.Isolation.StoreAssemblyEnumeration EnumAssemblies(EnumAssembliesFlags Flags, System.Deployment.Internal.Isolation.IReferenceIdentity refToMatch)
        {
            Guid guidOfType = System.Deployment.Internal.Isolation.IsolationInterop.GetGuidOfType(typeof(System.Deployment.Internal.Isolation.IEnumSTORE_ASSEMBLY));
            return new System.Deployment.Internal.Isolation.StoreAssemblyEnumeration((System.Deployment.Internal.Isolation.IEnumSTORE_ASSEMBLY) this._pStore.EnumAssemblies((uint) Flags, refToMatch, ref guidOfType));
        }

        [SecuritySafeCritical]
        public System.Deployment.Internal.Isolation.StoreCategoryEnumeration EnumCategories(EnumCategoriesFlags Flags, System.Deployment.Internal.Isolation.IReferenceIdentity CategoryMatch)
        {
            Guid guidOfType = System.Deployment.Internal.Isolation.IsolationInterop.GetGuidOfType(typeof(System.Deployment.Internal.Isolation.IEnumSTORE_CATEGORY));
            return new System.Deployment.Internal.Isolation.StoreCategoryEnumeration((System.Deployment.Internal.Isolation.IEnumSTORE_CATEGORY) this._pStore.EnumCategories((uint) Flags, CategoryMatch, ref guidOfType));
        }

        [SecuritySafeCritical]
        public System.Deployment.Internal.Isolation.StoreCategoryInstanceEnumeration EnumCategoryInstances(EnumCategoryInstancesFlags Flags, System.Deployment.Internal.Isolation.IDefinitionIdentity Category, string SubCat)
        {
            Guid guidOfType = System.Deployment.Internal.Isolation.IsolationInterop.GetGuidOfType(typeof(System.Deployment.Internal.Isolation.IEnumSTORE_CATEGORY_INSTANCE));
            return new System.Deployment.Internal.Isolation.StoreCategoryInstanceEnumeration((System.Deployment.Internal.Isolation.IEnumSTORE_CATEGORY_INSTANCE) this._pStore.EnumCategoryInstances((uint) Flags, Category, SubCat, ref guidOfType));
        }

        [SecuritySafeCritical]
        public System.Deployment.Internal.Isolation.StoreAssemblyFileEnumeration EnumFiles(EnumAssemblyFilesFlags Flags, System.Deployment.Internal.Isolation.IDefinitionIdentity Assembly)
        {
            Guid guidOfType = System.Deployment.Internal.Isolation.IsolationInterop.GetGuidOfType(typeof(System.Deployment.Internal.Isolation.IEnumSTORE_ASSEMBLY_FILE));
            return new System.Deployment.Internal.Isolation.StoreAssemblyFileEnumeration((System.Deployment.Internal.Isolation.IEnumSTORE_ASSEMBLY_FILE) this._pStore.EnumFiles((uint) Flags, Assembly, ref guidOfType));
        }

        [SecuritySafeCritical]
        public System.Deployment.Internal.Isolation.IEnumSTORE_ASSEMBLY_INSTALLATION_REFERENCE EnumInstallationReferences(EnumAssemblyInstallReferenceFlags Flags, System.Deployment.Internal.Isolation.IDefinitionIdentity Assembly)
        {
            Guid guidOfType = System.Deployment.Internal.Isolation.IsolationInterop.GetGuidOfType(typeof(System.Deployment.Internal.Isolation.IEnumSTORE_ASSEMBLY_INSTALLATION_REFERENCE));
            return (System.Deployment.Internal.Isolation.IEnumSTORE_ASSEMBLY_INSTALLATION_REFERENCE) this._pStore.EnumInstallationReferences((uint) Flags, Assembly, ref guidOfType);
        }

        [SecuritySafeCritical]
        public System.Deployment.Internal.Isolation.StoreDeploymentMetadataPropertyEnumeration EnumInstallerDeploymentProperties(Guid InstallerId, string InstallerName, string InstallerMetadata, System.Deployment.Internal.Isolation.IDefinitionAppId Deployment)
        {
            System.Deployment.Internal.Isolation.StoreApplicationReference reference = new System.Deployment.Internal.Isolation.StoreApplicationReference(InstallerId, InstallerName, InstallerMetadata);
            return new System.Deployment.Internal.Isolation.StoreDeploymentMetadataPropertyEnumeration((System.Deployment.Internal.Isolation.IEnumSTORE_DEPLOYMENT_METADATA_PROPERTY) this._pStore.EnumInstallerDeploymentMetadataProperties(0, ref reference, Deployment, ref System.Deployment.Internal.Isolation.IsolationInterop.IID_IEnumSTORE_DEPLOYMENT_METADATA_PROPERTY));
        }

        [SecuritySafeCritical]
        public System.Deployment.Internal.Isolation.StoreDeploymentMetadataEnumeration EnumInstallerDeployments(Guid InstallerId, string InstallerName, string InstallerMetadata, System.Deployment.Internal.Isolation.IReferenceAppId DeploymentFilter)
        {
            System.Deployment.Internal.Isolation.StoreApplicationReference reference = new System.Deployment.Internal.Isolation.StoreApplicationReference(InstallerId, InstallerName, InstallerMetadata);
            return new System.Deployment.Internal.Isolation.StoreDeploymentMetadataEnumeration((System.Deployment.Internal.Isolation.IEnumSTORE_DEPLOYMENT_METADATA) this._pStore.EnumInstallerDeploymentMetadata(0, ref reference, DeploymentFilter, ref System.Deployment.Internal.Isolation.IsolationInterop.IID_IEnumSTORE_DEPLOYMENT_METADATA));
        }

        [SecuritySafeCritical]
        public System.Deployment.Internal.Isolation.StoreAssemblyFileEnumeration EnumPrivateFiles(EnumApplicationPrivateFiles Flags, System.Deployment.Internal.Isolation.IDefinitionAppId Application, System.Deployment.Internal.Isolation.IDefinitionIdentity Assembly)
        {
            Guid guidOfType = System.Deployment.Internal.Isolation.IsolationInterop.GetGuidOfType(typeof(System.Deployment.Internal.Isolation.IEnumSTORE_ASSEMBLY_FILE));
            return new System.Deployment.Internal.Isolation.StoreAssemblyFileEnumeration((System.Deployment.Internal.Isolation.IEnumSTORE_ASSEMBLY_FILE) this._pStore.EnumPrivateFiles((uint) Flags, Application, Assembly, ref guidOfType));
        }

        public System.Deployment.Internal.Isolation.StoreSubcategoryEnumeration EnumSubcategories(EnumSubcategoriesFlags Flags, System.Deployment.Internal.Isolation.IDefinitionIdentity CategoryMatch)
        {
            return this.EnumSubcategories(Flags, CategoryMatch, null);
        }

        [SecuritySafeCritical]
        public System.Deployment.Internal.Isolation.StoreSubcategoryEnumeration EnumSubcategories(EnumSubcategoriesFlags Flags, System.Deployment.Internal.Isolation.IDefinitionIdentity Category, string SearchPattern)
        {
            Guid guidOfType = System.Deployment.Internal.Isolation.IsolationInterop.GetGuidOfType(typeof(System.Deployment.Internal.Isolation.IEnumSTORE_CATEGORY_SUBCATEGORY));
            return new System.Deployment.Internal.Isolation.StoreSubcategoryEnumeration((System.Deployment.Internal.Isolation.IEnumSTORE_CATEGORY_SUBCATEGORY) this._pStore.EnumSubcategories((uint) Flags, Category, SearchPattern, ref guidOfType));
        }

        [SecuritySafeCritical]
        public System.Deployment.Internal.Isolation.IDefinitionIdentity GetAssemblyIdentity(uint Flags, System.Deployment.Internal.Isolation.IDefinitionIdentity DefinitionIdentity)
        {
            Guid riid = System.Deployment.Internal.Isolation.IsolationInterop.IID_IDefinitionIdentity;
            return (System.Deployment.Internal.Isolation.IDefinitionIdentity) this._pStore.GetAssemblyInformation(Flags, DefinitionIdentity, ref riid);
        }

        [SecuritySafeCritical]
        public System.Deployment.Internal.Isolation.Manifest.ICMS GetAssemblyManifest(uint Flags, System.Deployment.Internal.Isolation.IDefinitionIdentity DefinitionIdentity)
        {
            Guid riid = System.Deployment.Internal.Isolation.IsolationInterop.IID_ICMS;
            return (System.Deployment.Internal.Isolation.Manifest.ICMS) this._pStore.GetAssemblyInformation(Flags, DefinitionIdentity, ref riid);
        }

        [SecurityCritical]
        public byte[] GetDeploymentProperty(GetPackagePropertyFlags Flags, System.Deployment.Internal.Isolation.IDefinitionAppId Deployment, System.Deployment.Internal.Isolation.StoreApplicationReference Reference, Guid PropertySet, string PropertyName)
        {
            System.Deployment.Internal.Isolation.BLOB blob = new System.Deployment.Internal.Isolation.BLOB();
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
        public IPathLock LockApplicationPath(System.Deployment.Internal.Isolation.IDefinitionAppId app)
        {
            IntPtr ptr;
            return new ApplicationPathLock(this._pStore, ptr, this._pStore.LockApplicationPath(0, app, out ptr));
        }

        [SecuritySafeCritical]
        public IPathLock LockAssemblyPath(System.Deployment.Internal.Isolation.IDefinitionIdentity asm)
        {
            IntPtr ptr;
            return new AssemblyPathLock(this._pStore, ptr, this._pStore.LockAssemblyPath(0, asm, out ptr));
        }

        [SecuritySafeCritical]
        public ulong QueryChangeID(System.Deployment.Internal.Isolation.IDefinitionIdentity asm)
        {
            return this._pStore.QueryChangeID(asm);
        }

        [SecuritySafeCritical]
        public uint[] Transact(System.Deployment.Internal.Isolation.StoreTransactionOperation[] operations)
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

        public void Transact(System.Deployment.Internal.Isolation.StoreTransactionOperation[] operations, uint[] rgDispositions, int[] rgResults)
        {
            if ((operations == null) || (operations.Length == 0))
            {
                throw new ArgumentException("operations");
            }
            this._pStore.Transact(new IntPtr(operations.Length), operations, rgDispositions, rgResults);
        }

        public System.Deployment.Internal.Isolation.IStore InternalStore
        {
            get
            {
                return this._pStore;
            }
        }

        private class ApplicationPathLock : System.Deployment.Internal.Isolation.Store.IPathLock, IDisposable
        {
            private string _path;
            private IntPtr _pLockCookie = IntPtr.Zero;
            private System.Deployment.Internal.Isolation.IStore _pSourceStore;

            public ApplicationPathLock(System.Deployment.Internal.Isolation.IStore s, IntPtr c, string path)
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

        private class AssemblyPathLock : System.Deployment.Internal.Isolation.Store.IPathLock, IDisposable
        {
            private string _path;
            private IntPtr _pLockCookie = IntPtr.Zero;
            private System.Deployment.Internal.Isolation.IStore _pSourceStore;

            public AssemblyPathLock(System.Deployment.Internal.Isolation.IStore s, IntPtr c, string path)
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

