namespace System.Security.Policy
{
    using System;
    using System.Collections;
    using System.Deployment.Internal.Isolation;
    using System.Deployment.Internal.Isolation.Manifest;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;

    [SecurityCritical, ComVisible(true)]
    public sealed class ApplicationTrustCollection : ICollection, IEnumerable
    {
        private const string ApplicationTrustProperty = "ApplicationTrust";
        private static Guid ClrPropertySet = new Guid("c989bb7a-8385-4715-98cf-a741a8edb823");
        private const string InstallerIdentifier = "{60051b8f-4f12-400a-8e50-dd05ebd438d1}";
        private object m_appTrusts;
        private Store m_pStore;
        private bool m_storeBounded;
        private static object s_installReference = null;

        [SecurityCritical]
        internal ApplicationTrustCollection() : this(false)
        {
        }

        internal ApplicationTrustCollection(bool storeBounded)
        {
            this.m_storeBounded = storeBounded;
        }

        [SecurityCritical]
        public int Add(ApplicationTrust trust)
        {
            if (trust == null)
            {
                throw new ArgumentNullException("trust");
            }
            if (trust.ApplicationIdentity == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_ApplicationTrustShouldHaveIdentity"));
            }
            if (this.m_storeBounded)
            {
                this.CommitApplicationTrust(trust.ApplicationIdentity, trust.ToXml().ToString());
                return -1;
            }
            return this.AppTrusts.Add(trust);
        }

        [SecurityCritical]
        public void AddRange(ApplicationTrust[] trusts)
        {
            if (trusts == null)
            {
                throw new ArgumentNullException("trusts");
            }
            int index = 0;
            try
            {
                while (index < trusts.Length)
                {
                    this.Add(trusts[index]);
                    index++;
                }
            }
            catch
            {
                for (int i = 0; i < index; i++)
                {
                    this.Remove(trusts[i]);
                }
                throw;
            }
        }

        [SecurityCritical]
        public void AddRange(ApplicationTrustCollection trusts)
        {
            if (trusts == null)
            {
                throw new ArgumentNullException("trusts");
            }
            int num = 0;
            try
            {
                ApplicationTrustEnumerator enumerator = trusts.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    ApplicationTrust current = enumerator.Current;
                    this.Add(current);
                    num++;
                }
            }
            catch
            {
                for (int i = 0; i < num; i++)
                {
                    this.Remove(trusts[i]);
                }
                throw;
            }
        }

        [SecurityCritical]
        public void Clear()
        {
            ArrayList appTrusts = this.AppTrusts;
            if (this.m_storeBounded)
            {
                foreach (ApplicationTrust trust in appTrusts)
                {
                    if (trust.ApplicationIdentity == null)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_ApplicationTrustShouldHaveIdentity"));
                    }
                    this.CommitApplicationTrust(trust.ApplicationIdentity, null);
                }
            }
            appTrusts.Clear();
        }

        [SecurityCritical]
        private void CommitApplicationTrust(ApplicationIdentity applicationIdentity, string trustXml)
        {
            StoreOperationMetadataProperty[] setProperties = new StoreOperationMetadataProperty[] { new StoreOperationMetadataProperty(ClrPropertySet, "ApplicationTrust", trustXml) };
            IEnumDefinitionIdentity identity = applicationIdentity.Identity.EnumAppPath();
            IDefinitionIdentity[] definitionIdentity = new IDefinitionIdentity[1];
            IDefinitionIdentity identity2 = null;
            if (identity.Next(1, definitionIdentity) == 1)
            {
                identity2 = definitionIdentity[0];
            }
            IDefinitionAppId deployment = IsolationInterop.AppIdAuthority.CreateDefinition();
            deployment.SetAppPath(1, new IDefinitionIdentity[] { identity2 });
            deployment.put_Codebase(applicationIdentity.CodeBase);
            using (StoreTransaction transaction = new StoreTransaction())
            {
                transaction.Add(new StoreOperationSetDeploymentMetadata(deployment, InstallReference, setProperties));
                this.RefreshStorePointer();
                this.m_pStore.Transact(transaction.Operations);
            }
            this.m_appTrusts = null;
        }

        public void CopyTo(ApplicationTrust[] array, int index)
        {
            ((ICollection) this).CopyTo(array, index);
        }

        [SecurityCritical]
        public ApplicationTrustCollection Find(ApplicationIdentity applicationIdentity, ApplicationVersionMatch versionMatch)
        {
            ApplicationTrustCollection trusts = new ApplicationTrustCollection(false);
            ApplicationTrustEnumerator enumerator = this.GetEnumerator();
            while (enumerator.MoveNext())
            {
                ApplicationTrust current = enumerator.Current;
                if (CmsUtils.CompareIdentities(current.ApplicationIdentity, applicationIdentity, versionMatch))
                {
                    trusts.Add(current);
                }
            }
            return trusts;
        }

        public ApplicationTrustEnumerator GetEnumerator()
        {
            return new ApplicationTrustEnumerator(this);
        }

        [SecurityCritical]
        private void RefreshStorePointer()
        {
            if (this.m_pStore != null)
            {
                Marshal.ReleaseComObject(this.m_pStore.InternalStore);
            }
            this.m_pStore = IsolationInterop.GetUserStore();
        }

        [SecurityCritical]
        public void Remove(ApplicationTrust trust)
        {
            if (trust == null)
            {
                throw new ArgumentNullException("trust");
            }
            if (trust.ApplicationIdentity == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_ApplicationTrustShouldHaveIdentity"));
            }
            if (this.m_storeBounded)
            {
                this.CommitApplicationTrust(trust.ApplicationIdentity, null);
            }
            else
            {
                this.AppTrusts.Remove(trust);
            }
        }

        [SecurityCritical]
        public void Remove(ApplicationIdentity applicationIdentity, ApplicationVersionMatch versionMatch)
        {
            ApplicationTrustCollection trusts = this.Find(applicationIdentity, versionMatch);
            this.RemoveRange(trusts);
        }

        [SecurityCritical]
        public void RemoveRange(ApplicationTrust[] trusts)
        {
            if (trusts == null)
            {
                throw new ArgumentNullException("trusts");
            }
            int index = 0;
            try
            {
                while (index < trusts.Length)
                {
                    this.Remove(trusts[index]);
                    index++;
                }
            }
            catch
            {
                for (int i = 0; i < index; i++)
                {
                    this.Add(trusts[i]);
                }
                throw;
            }
        }

        [SecurityCritical]
        public void RemoveRange(ApplicationTrustCollection trusts)
        {
            if (trusts == null)
            {
                throw new ArgumentNullException("trusts");
            }
            int num = 0;
            try
            {
                ApplicationTrustEnumerator enumerator = trusts.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    ApplicationTrust current = enumerator.Current;
                    this.Remove(current);
                    num++;
                }
            }
            catch
            {
                for (int i = 0; i < num; i++)
                {
                    this.Add(trusts[i]);
                }
                throw;
            }
        }

        [SecuritySafeCritical]
        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (array.Rank != 1)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_RankMultiDimNotSupported"));
            }
            if ((index < 0) || (index >= array.Length))
            {
                throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if ((array.Length - index) < this.Count)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
            }
            for (int i = 0; i < this.Count; i++)
            {
                array.SetValue(this[i], index++);
            }
        }

        [SecuritySafeCritical]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new ApplicationTrustEnumerator(this);
        }

        private ArrayList AppTrusts
        {
            [SecurityCritical]
            get
            {
                if (this.m_appTrusts == null)
                {
                    ArrayList list = new ArrayList();
                    if (this.m_storeBounded)
                    {
                        this.RefreshStorePointer();
                        foreach (IDefinitionAppId id in this.m_pStore.EnumInstallerDeployments(IsolationInterop.GUID_SXS_INSTALL_REFERENCE_SCHEME_OPAQUESTRING, "{60051b8f-4f12-400a-8e50-dd05ebd438d1}", "ApplicationTrust", null))
                        {
                            foreach (StoreOperationMetadataProperty property in this.m_pStore.EnumInstallerDeploymentProperties(IsolationInterop.GUID_SXS_INSTALL_REFERENCE_SCHEME_OPAQUESTRING, "{60051b8f-4f12-400a-8e50-dd05ebd438d1}", "ApplicationTrust", id))
                            {
                                string xml = property.Value;
                                if ((xml != null) && (xml.Length > 0))
                                {
                                    SecurityElement element = SecurityElement.FromString(xml);
                                    ApplicationTrust trust = new ApplicationTrust();
                                    trust.FromXml(element);
                                    list.Add(trust);
                                }
                            }
                        }
                    }
                    Interlocked.CompareExchange(ref this.m_appTrusts, list, null);
                }
                return (this.m_appTrusts as ArrayList);
            }
        }

        public int Count
        {
            [SecuritySafeCritical]
            get
            {
                return this.AppTrusts.Count;
            }
        }

        private static StoreApplicationReference InstallReference
        {
            get
            {
                if (s_installReference == null)
                {
                    Interlocked.CompareExchange(ref s_installReference, new StoreApplicationReference(IsolationInterop.GUID_SXS_INSTALL_REFERENCE_SCHEME_OPAQUESTRING, "{60051b8f-4f12-400a-8e50-dd05ebd438d1}", null), null);
                }
                return (StoreApplicationReference) s_installReference;
            }
        }

        public bool IsSynchronized
        {
            [SecuritySafeCritical]
            get
            {
                return false;
            }
        }

        public ApplicationTrust this[int index]
        {
            [SecurityCritical]
            get
            {
                return (this.AppTrusts[index] as ApplicationTrust);
            }
        }

        public ApplicationTrust this[string appFullName]
        {
            [SecurityCritical]
            get
            {
                ApplicationIdentity applicationIdentity = new ApplicationIdentity(appFullName);
                ApplicationTrustCollection trusts = this.Find(applicationIdentity, ApplicationVersionMatch.MatchExactVersion);
                if (trusts.Count > 0)
                {
                    return trusts[0];
                }
                return null;
            }
        }

        public object SyncRoot
        {
            [SecuritySafeCritical]
            get
            {
                return this;
            }
        }
    }
}

