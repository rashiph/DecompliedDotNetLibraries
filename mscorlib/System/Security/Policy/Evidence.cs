namespace System.Security.Policy
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.Hosting;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    [Serializable, ComVisible(true)]
    public sealed class Evidence : ICollection, IEnumerable
    {
        private const int LockTimeout = 0x1388;
        private volatile ArrayList m_assemblyList;
        [NonSerialized]
        private WeakReference m_cloneOrigin;
        [OptionalField(VersionAdded=4)]
        private bool m_deserializedTargetEvidence;
        [OptionalField(VersionAdded=4)]
        private Dictionary<Type, EvidenceTypeDescriptor> m_evidence;
        [NonSerialized]
        private ReaderWriterLock m_evidenceLock;
        private volatile ArrayList m_hostList;
        private bool m_locked;
        [NonSerialized]
        private IRuntimeEvidenceFactory m_target;
        [NonSerialized]
        private uint m_version;
        private static Type[] s_runtimeEvidenceTypes;

        public Evidence()
        {
            this.m_evidence = new Dictionary<Type, EvidenceTypeDescriptor>();
            this.m_evidenceLock = new ReaderWriterLock();
        }

        public Evidence(Evidence evidence)
        {
            this.m_evidence = new Dictionary<Type, EvidenceTypeDescriptor>();
            if (evidence != null)
            {
                using (new EvidenceLockHolder(evidence, EvidenceLockHolder.LockType.Reader))
                {
                    foreach (KeyValuePair<Type, EvidenceTypeDescriptor> pair in evidence.m_evidence)
                    {
                        EvidenceTypeDescriptor descriptor = pair.Value;
                        if (descriptor != null)
                        {
                            descriptor = descriptor.Clone();
                        }
                        this.m_evidence[pair.Key] = descriptor;
                    }
                    this.m_target = evidence.m_target;
                    this.m_locked = evidence.m_locked;
                    this.m_deserializedTargetEvidence = evidence.m_deserializedTargetEvidence;
                    if (evidence.Target != null)
                    {
                        this.m_cloneOrigin = new WeakReference(evidence);
                    }
                }
            }
            this.m_evidenceLock = new ReaderWriterLock();
        }

        [SecuritySafeCritical]
        internal Evidence(IRuntimeEvidenceFactory target)
        {
            this.m_evidence = new Dictionary<Type, EvidenceTypeDescriptor>();
            this.m_target = target;
            foreach (Type type in RuntimeEvidenceTypes)
            {
                this.m_evidence[type] = null;
            }
            this.QueryHostForPossibleEvidenceTypes();
            this.m_evidenceLock = new ReaderWriterLock();
        }

        [Obsolete("This constructor is obsolete. Please use the constructor which takes arrays of EvidenceBase instead.")]
        public Evidence(object[] hostEvidence, object[] assemblyEvidence)
        {
            this.m_evidence = new Dictionary<Type, EvidenceTypeDescriptor>();
            if (hostEvidence != null)
            {
                foreach (object obj2 in hostEvidence)
                {
                    this.AddHost(obj2);
                }
            }
            if (assemblyEvidence != null)
            {
                foreach (object obj3 in assemblyEvidence)
                {
                    this.AddAssembly(obj3);
                }
            }
            this.m_evidenceLock = new ReaderWriterLock();
        }

        public Evidence(EvidenceBase[] hostEvidence, EvidenceBase[] assemblyEvidence)
        {
            this.m_evidence = new Dictionary<Type, EvidenceTypeDescriptor>();
            if (hostEvidence != null)
            {
                foreach (EvidenceBase base2 in hostEvidence)
                {
                    this.AddHostEvidence(base2, GetEvidenceIndexType(base2), DuplicateEvidenceAction.Throw);
                }
            }
            if (assemblyEvidence != null)
            {
                foreach (EvidenceBase base3 in assemblyEvidence)
                {
                    this.AddAssemblyEvidence(base3, GetEvidenceIndexType(base3), DuplicateEvidenceAction.Throw);
                }
            }
            this.m_evidenceLock = new ReaderWriterLock();
        }

        private void AcquireReaderLock()
        {
            if (this.m_evidenceLock != null)
            {
                this.m_evidenceLock.AcquireReaderLock(0x1388);
            }
        }

        private void AcquireWriterlock()
        {
            if (this.m_evidenceLock != null)
            {
                this.m_evidenceLock.AcquireWriterLock(0x1388);
            }
        }

        [Obsolete("This method is obsolete. Please use AddAssemblyEvidence instead.")]
        public void AddAssembly(object id)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }
            if (!id.GetType().IsSerializable)
            {
                throw new ArgumentException(Environment.GetResourceString("Policy_EvidenceMustBeSerializable"), "id");
            }
            EvidenceBase evidence = WrapLegacyEvidence(id);
            Type evidenceIndexType = GetEvidenceIndexType(evidence);
            this.AddAssemblyEvidence(evidence, evidenceIndexType, DuplicateEvidenceAction.Merge);
        }

        [ComVisible(false)]
        public void AddAssemblyEvidence<T>(T evidence) where T: EvidenceBase
        {
            if (evidence == null)
            {
                throw new ArgumentNullException("evidence");
            }
            Type evidenceType = typeof(T);
            if ((typeof(T) == typeof(EvidenceBase)) || (evidence is ILegacyEvidenceAdapter))
            {
                evidenceType = GetEvidenceIndexType(evidence);
            }
            this.AddAssemblyEvidence(evidence, evidenceType, DuplicateEvidenceAction.Throw);
        }

        private void AddAssemblyEvidence(EvidenceBase evidence, Type evidenceType, DuplicateEvidenceAction duplicateAction)
        {
            using (new EvidenceLockHolder(this, EvidenceLockHolder.LockType.Writer))
            {
                this.AddAssemblyEvidenceNoLock(evidence, evidenceType, duplicateAction);
            }
        }

        private void AddAssemblyEvidenceNoLock(EvidenceBase evidence, Type evidenceType, DuplicateEvidenceAction duplicateAction)
        {
            this.DeserializeTargetEvidence();
            EvidenceTypeDescriptor evidenceTypeDescriptor = this.GetEvidenceTypeDescriptor(evidenceType, true);
            this.m_version++;
            if (evidenceTypeDescriptor.AssemblyEvidence == null)
            {
                evidenceTypeDescriptor.AssemblyEvidence = evidence;
            }
            else
            {
                evidenceTypeDescriptor.AssemblyEvidence = HandleDuplicateEvidence(evidenceTypeDescriptor.AssemblyEvidence, evidence, duplicateAction);
            }
        }

        [SecuritySafeCritical, Obsolete("This method is obsolete. Please use AddHostEvidence instead.")]
        public void AddHost(object id)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }
            if (!id.GetType().IsSerializable)
            {
                throw new ArgumentException(Environment.GetResourceString("Policy_EvidenceMustBeSerializable"), "id");
            }
            if (this.m_locked)
            {
                new SecurityPermission(SecurityPermissionFlag.ControlEvidence).Demand();
            }
            EvidenceBase evidence = WrapLegacyEvidence(id);
            Type evidenceIndexType = GetEvidenceIndexType(evidence);
            this.AddHostEvidence(evidence, evidenceIndexType, DuplicateEvidenceAction.Merge);
        }

        [ComVisible(false)]
        public void AddHostEvidence<T>(T evidence) where T: EvidenceBase
        {
            if (evidence == null)
            {
                throw new ArgumentNullException("evidence");
            }
            Type evidenceType = typeof(T);
            if ((typeof(T) == typeof(EvidenceBase)) || (evidence is ILegacyEvidenceAdapter))
            {
                evidenceType = GetEvidenceIndexType(evidence);
            }
            this.AddHostEvidence(evidence, evidenceType, DuplicateEvidenceAction.Throw);
        }

        [SecuritySafeCritical]
        private void AddHostEvidence(EvidenceBase evidence, Type evidenceType, DuplicateEvidenceAction duplicateAction)
        {
            if (this.Locked)
            {
                new SecurityPermission(SecurityPermissionFlag.ControlEvidence).Demand();
            }
            using (new EvidenceLockHolder(this, EvidenceLockHolder.LockType.Writer))
            {
                this.AddHostEvidenceNoLock(evidence, evidenceType, duplicateAction);
            }
        }

        private void AddHostEvidenceNoLock(EvidenceBase evidence, Type evidenceType, DuplicateEvidenceAction duplicateAction)
        {
            EvidenceTypeDescriptor evidenceTypeDescriptor = this.GetEvidenceTypeDescriptor(evidenceType, true);
            this.m_version++;
            if (evidenceTypeDescriptor.HostEvidence == null)
            {
                evidenceTypeDescriptor.HostEvidence = evidence;
            }
            else
            {
                evidenceTypeDescriptor.HostEvidence = HandleDuplicateEvidence(evidenceTypeDescriptor.HostEvidence, evidence, duplicateAction);
            }
        }

        [ComVisible(false), SecuritySafeCritical]
        public void Clear()
        {
            if (this.Locked)
            {
                new SecurityPermission(SecurityPermissionFlag.ControlEvidence).Demand();
            }
            using (new EvidenceLockHolder(this, EvidenceLockHolder.LockType.Writer))
            {
                this.m_version++;
                this.m_evidence.Clear();
            }
        }

        [ComVisible(false)]
        public Evidence Clone()
        {
            return new Evidence(this);
        }

        [Obsolete("Evidence should not be treated as an ICollection. Please use the GetHostEnumerator and GetAssemblyEnumerator methods rather than using CopyTo.")]
        public void CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if ((index < 0) || (index > (array.Length - this.Count)))
            {
                throw new ArgumentOutOfRangeException("index");
            }
            int num = index;
            IEnumerator hostEnumerator = this.GetHostEnumerator();
            while (hostEnumerator.MoveNext())
            {
                array.SetValue(hostEnumerator.Current, num);
                num++;
            }
            IEnumerator assemblyEnumerator = this.GetAssemblyEnumerator();
            while (assemblyEnumerator.MoveNext())
            {
                array.SetValue(assemblyEnumerator.Current, num);
                num++;
            }
        }

        private void DeserializeTargetEvidence()
        {
            if ((this.m_target != null) && !this.m_deserializedTargetEvidence)
            {
                bool flag = false;
                LockCookie lockCookie = new LockCookie();
                try
                {
                    if (!this.IsWriterLockHeld)
                    {
                        lockCookie = this.UpgradeToWriterLock();
                        flag = true;
                    }
                    this.m_deserializedTargetEvidence = true;
                    foreach (EvidenceBase base2 in this.m_target.GetFactorySuppliedEvidence())
                    {
                        this.AddAssemblyEvidenceNoLock(base2, GetEvidenceIndexType(base2), DuplicateEvidenceAction.Throw);
                    }
                }
                finally
                {
                    if (flag)
                    {
                        this.DowngradeFromWriterLock(ref lockCookie);
                    }
                }
            }
        }

        private void DowngradeFromWriterLock(ref LockCookie lockCookie)
        {
            if (this.m_evidenceLock != null)
            {
                this.m_evidenceLock.DowngradeFromWriterLock(ref lockCookie);
            }
        }

        [SecurityCritical]
        private EvidenceBase GenerateHostEvidence(Type type, bool hostCanGenerate)
        {
            if (hostCanGenerate)
            {
                AppDomain target = this.m_target.Target as AppDomain;
                Assembly assembly = this.m_target.Target as Assembly;
                EvidenceBase base2 = null;
                if (target != null)
                {
                    base2 = AppDomain.CurrentDomain.HostSecurityManager.GenerateAppDomainEvidence(type);
                }
                else if (assembly != null)
                {
                    base2 = AppDomain.CurrentDomain.HostSecurityManager.GenerateAssemblyEvidence(type, assembly);
                }
                if (base2 != null)
                {
                    if (!type.IsAssignableFrom(base2.GetType()))
                    {
                        string fullName = AppDomain.CurrentDomain.HostSecurityManager.GetType().FullName;
                        string str2 = base2.GetType().FullName;
                        string str3 = type.FullName;
                        throw new InvalidOperationException(Environment.GetResourceString("Policy_IncorrectHostEvidence", new object[] { fullName, str2, str3 }));
                    }
                    return base2;
                }
            }
            return this.m_target.GenerateEvidence(type);
        }

        public IEnumerator GetAssemblyEnumerator()
        {
            using (new EvidenceLockHolder(this, EvidenceLockHolder.LockType.Reader))
            {
                this.DeserializeTargetEvidence();
                return new EvidenceEnumerator(this, EvidenceEnumerator.Category.Assembly);
            }
        }

        [ComVisible(false)]
        public T GetAssemblyEvidence<T>() where T: EvidenceBase
        {
            return (UnwrapEvidence(this.GetAssemblyEvidence(typeof(T))) as T);
        }

        internal EvidenceBase GetAssemblyEvidence(Type type)
        {
            using (new EvidenceLockHolder(this, EvidenceLockHolder.LockType.Reader))
            {
                return this.GetAssemblyEvidenceNoLock(type);
            }
        }

        private EvidenceBase GetAssemblyEvidenceNoLock(Type type)
        {
            this.DeserializeTargetEvidence();
            EvidenceTypeDescriptor evidenceTypeDescriptor = this.GetEvidenceTypeDescriptor(type);
            if (evidenceTypeDescriptor != null)
            {
                return evidenceTypeDescriptor.AssemblyEvidence;
            }
            return null;
        }

        internal T GetDelayEvaluatedHostEvidence<T>() where T: EvidenceBase, IDelayEvaluatedEvidence
        {
            return (UnwrapEvidence(this.GetHostEvidence(typeof(T), false)) as T);
        }

        [Obsolete("GetEnumerator is obsolete. Please use GetAssemblyEnumerator and GetHostEnumerator instead.")]
        public IEnumerator GetEnumerator()
        {
            using (new EvidenceLockHolder(this, EvidenceLockHolder.LockType.Reader))
            {
                return new EvidenceEnumerator(this, EvidenceEnumerator.Category.Assembly | EvidenceEnumerator.Category.Host);
            }
        }

        private static Type GetEvidenceIndexType(EvidenceBase evidence)
        {
            ILegacyEvidenceAdapter adapter = evidence as ILegacyEvidenceAdapter;
            if (adapter != null)
            {
                return adapter.EvidenceType;
            }
            return evidence.GetType();
        }

        internal EvidenceTypeDescriptor GetEvidenceTypeDescriptor(Type evidenceType)
        {
            return this.GetEvidenceTypeDescriptor(evidenceType, false);
        }

        private EvidenceTypeDescriptor GetEvidenceTypeDescriptor(Type evidenceType, bool addIfNotExist)
        {
            EvidenceTypeDescriptor descriptor = null;
            if (!this.m_evidence.TryGetValue(evidenceType, out descriptor) && !addIfNotExist)
            {
                return null;
            }
            if (descriptor == null)
            {
                descriptor = new EvidenceTypeDescriptor();
                bool flag = false;
                LockCookie lockCookie = new LockCookie();
                try
                {
                    if (!this.IsWriterLockHeld)
                    {
                        lockCookie = this.UpgradeToWriterLock();
                        flag = true;
                    }
                    this.m_evidence[evidenceType] = descriptor;
                }
                finally
                {
                    if (flag)
                    {
                        this.DowngradeFromWriterLock(ref lockCookie);
                    }
                }
            }
            return descriptor;
        }

        public IEnumerator GetHostEnumerator()
        {
            using (new EvidenceLockHolder(this, EvidenceLockHolder.LockType.Reader))
            {
                return new EvidenceEnumerator(this, EvidenceEnumerator.Category.Host);
            }
        }

        [ComVisible(false)]
        public T GetHostEvidence<T>() where T: EvidenceBase
        {
            return (UnwrapEvidence(this.GetHostEvidence(typeof(T))) as T);
        }

        internal EvidenceBase GetHostEvidence(Type type)
        {
            return this.GetHostEvidence(type, true);
        }

        [SecuritySafeCritical]
        private EvidenceBase GetHostEvidence(Type type, bool markDelayEvaluatedEvidenceUsed)
        {
            using (new EvidenceLockHolder(this, EvidenceLockHolder.LockType.Reader))
            {
                EvidenceBase hostEvidenceNoLock = this.GetHostEvidenceNoLock(type);
                if (markDelayEvaluatedEvidenceUsed)
                {
                    IDelayEvaluatedEvidence evidence = hostEvidenceNoLock as IDelayEvaluatedEvidence;
                    if (evidence != null)
                    {
                        evidence.MarkUsed();
                    }
                }
                return hostEvidenceNoLock;
            }
        }

        [SecurityCritical]
        private EvidenceBase GetHostEvidenceNoLock(Type type)
        {
            EvidenceTypeDescriptor evidenceTypeDescriptor = this.GetEvidenceTypeDescriptor(type);
            if (evidenceTypeDescriptor != null)
            {
                if (evidenceTypeDescriptor.HostEvidence != null)
                {
                    return evidenceTypeDescriptor.HostEvidence;
                }
                if ((this.m_target != null) && !evidenceTypeDescriptor.Generated)
                {
                    using (new EvidenceUpgradeLockHolder(this))
                    {
                        evidenceTypeDescriptor.Generated = true;
                        EvidenceBase base2 = this.GenerateHostEvidence(type, evidenceTypeDescriptor.HostCanGenerate);
                        if (base2 != null)
                        {
                            evidenceTypeDescriptor.HostEvidence = base2;
                            Evidence target = (this.m_cloneOrigin != null) ? (this.m_cloneOrigin.Target as Evidence) : null;
                            if (target == null)
                            {
                                return base2;
                            }
                            using (new EvidenceLockHolder(target, EvidenceLockHolder.LockType.Writer))
                            {
                                EvidenceTypeDescriptor descriptor2 = target.GetEvidenceTypeDescriptor(type);
                                if ((descriptor2 != null) && (descriptor2.HostEvidence == null))
                                {
                                    descriptor2.HostEvidence = base2.Clone();
                                }
                            }
                        }
                        return base2;
                    }
                }
            }
            return null;
        }

        internal RawEvidenceEnumerator GetRawAssemblyEvidenceEnumerator()
        {
            this.DeserializeTargetEvidence();
            return new RawEvidenceEnumerator(this, new List<Type>(this.m_evidence.Keys), false);
        }

        internal RawEvidenceEnumerator GetRawHostEvidenceEnumerator()
        {
            return new RawEvidenceEnumerator(this, new List<Type>(this.m_evidence.Keys), true);
        }

        private static EvidenceBase HandleDuplicateEvidence(EvidenceBase original, EvidenceBase duplicate, DuplicateEvidenceAction action)
        {
            switch (action)
            {
                case DuplicateEvidenceAction.Throw:
                    throw new InvalidOperationException(Environment.GetResourceString("Policy_DuplicateEvidence", new object[] { duplicate.GetType().FullName }));

                case DuplicateEvidenceAction.Merge:
                {
                    LegacyEvidenceList list = original as LegacyEvidenceList;
                    if (list == null)
                    {
                        list = new LegacyEvidenceList();
                        list.Add(original);
                    }
                    list.Add(duplicate);
                    return list;
                }
                case DuplicateEvidenceAction.SelectNewObject:
                    return duplicate;
            }
            return null;
        }

        internal void MarkAllEvidenceAsUsed()
        {
            using (new EvidenceLockHolder(this, EvidenceLockHolder.LockType.Reader))
            {
                foreach (KeyValuePair<Type, EvidenceTypeDescriptor> pair in this.m_evidence)
                {
                    if (pair.Value != null)
                    {
                        IDelayEvaluatedEvidence hostEvidence = pair.Value.HostEvidence as IDelayEvaluatedEvidence;
                        if (hostEvidence != null)
                        {
                            hostEvidence.MarkUsed();
                        }
                        IDelayEvaluatedEvidence assemblyEvidence = pair.Value.AssemblyEvidence as IDelayEvaluatedEvidence;
                        if (assemblyEvidence != null)
                        {
                            assemblyEvidence.MarkUsed();
                        }
                    }
                }
            }
        }

        [SecuritySafeCritical]
        public void Merge(Evidence evidence)
        {
            if (evidence != null)
            {
                using (new EvidenceLockHolder(this, EvidenceLockHolder.LockType.Writer))
                {
                    bool flag = false;
                    IEnumerator hostEnumerator = evidence.GetHostEnumerator();
                    while (hostEnumerator.MoveNext())
                    {
                        if (this.Locked && !flag)
                        {
                            new SecurityPermission(SecurityPermissionFlag.ControlEvidence).Demand();
                            flag = true;
                        }
                        Type key = hostEnumerator.Current.GetType();
                        if (this.m_evidence.ContainsKey(key))
                        {
                            this.GetHostEvidenceNoLock(key);
                        }
                        EvidenceBase base2 = WrapLegacyEvidence(hostEnumerator.Current);
                        this.AddHostEvidenceNoLock(base2, GetEvidenceIndexType(base2), DuplicateEvidenceAction.Merge);
                    }
                    IEnumerator assemblyEnumerator = evidence.GetAssemblyEnumerator();
                    while (assemblyEnumerator.MoveNext())
                    {
                        EvidenceBase base3 = WrapLegacyEvidence(assemblyEnumerator.Current);
                        this.AddAssemblyEvidenceNoLock(base3, GetEvidenceIndexType(base3), DuplicateEvidenceAction.Merge);
                    }
                }
            }
        }

        internal void MergeWithNoDuplicates(Evidence evidence)
        {
            if (evidence != null)
            {
                using (new EvidenceLockHolder(this, EvidenceLockHolder.LockType.Writer))
                {
                    IEnumerator hostEnumerator = evidence.GetHostEnumerator();
                    while (hostEnumerator.MoveNext())
                    {
                        EvidenceBase base2 = WrapLegacyEvidence(hostEnumerator.Current);
                        this.AddHostEvidenceNoLock(base2, GetEvidenceIndexType(base2), DuplicateEvidenceAction.SelectNewObject);
                    }
                    IEnumerator assemblyEnumerator = evidence.GetAssemblyEnumerator();
                    while (assemblyEnumerator.MoveNext())
                    {
                        EvidenceBase base3 = WrapLegacyEvidence(assemblyEnumerator.Current);
                        this.AddAssemblyEvidenceNoLock(base3, GetEvidenceIndexType(base3), DuplicateEvidenceAction.SelectNewObject);
                    }
                }
            }
        }

        [ComVisible(false), OnDeserialized, SecurityCritical]
        private void OnDeserialized(StreamingContext context)
        {
            if (this.m_evidence == null)
            {
                this.m_evidence = new Dictionary<Type, EvidenceTypeDescriptor>();
                if (this.m_hostList != null)
                {
                    foreach (object obj2 in this.m_hostList)
                    {
                        if (obj2 != null)
                        {
                            this.AddHost(obj2);
                        }
                    }
                    this.m_hostList = null;
                }
                if (this.m_assemblyList != null)
                {
                    foreach (object obj3 in this.m_assemblyList)
                    {
                        if (obj3 != null)
                        {
                            this.AddAssembly(obj3);
                        }
                    }
                    this.m_assemblyList = null;
                }
            }
            this.m_evidenceLock = new ReaderWriterLock();
        }

        [SecurityCritical, OnSerializing, ComVisible(false), PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        private void OnSerializing(StreamingContext context)
        {
            using (new EvidenceLockHolder(this, EvidenceLockHolder.LockType.Reader))
            {
                foreach (Type type in new List<Type>(this.m_evidence.Keys))
                {
                    this.GetHostEvidenceNoLock(type);
                }
                this.DeserializeTargetEvidence();
            }
            ArrayList list = new ArrayList();
            IEnumerator hostEnumerator = this.GetHostEnumerator();
            while (hostEnumerator.MoveNext())
            {
                list.Add(hostEnumerator.Current);
            }
            this.m_hostList = list;
            ArrayList list2 = new ArrayList();
            IEnumerator assemblyEnumerator = this.GetAssemblyEnumerator();
            while (assemblyEnumerator.MoveNext())
            {
                list2.Add(assemblyEnumerator.Current);
            }
            this.m_assemblyList = list2;
        }

        [SecurityCritical]
        private void QueryHostForPossibleEvidenceTypes()
        {
            if (AppDomain.CurrentDomain.DomainManager != null)
            {
                HostSecurityManager hostSecurityManager = AppDomain.CurrentDomain.DomainManager.HostSecurityManager;
                if (hostSecurityManager != null)
                {
                    Type[] hostSuppliedAssemblyEvidenceTypes = null;
                    AppDomain target = this.m_target.Target as AppDomain;
                    Assembly assembly = this.m_target.Target as Assembly;
                    if ((assembly != null) && ((hostSecurityManager.Flags & HostSecurityManagerOptions.HostAssemblyEvidence) == HostSecurityManagerOptions.HostAssemblyEvidence))
                    {
                        hostSuppliedAssemblyEvidenceTypes = hostSecurityManager.GetHostSuppliedAssemblyEvidenceTypes(assembly);
                    }
                    else if ((target != null) && ((hostSecurityManager.Flags & HostSecurityManagerOptions.HostAppDomainEvidence) == HostSecurityManagerOptions.HostAppDomainEvidence))
                    {
                        hostSuppliedAssemblyEvidenceTypes = hostSecurityManager.GetHostSuppliedAppDomainEvidenceTypes();
                    }
                    if (hostSuppliedAssemblyEvidenceTypes != null)
                    {
                        foreach (Type type in hostSuppliedAssemblyEvidenceTypes)
                        {
                            this.GetEvidenceTypeDescriptor(type, true).HostCanGenerate = true;
                        }
                    }
                }
            }
        }

        [SecurityCritical, PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        internal byte[] RawSerialize()
        {
            byte[] buffer;
            using (new EvidenceLockHolder(this, EvidenceLockHolder.LockType.Reader))
            {
                Dictionary<Type, EvidenceBase> graph = new Dictionary<Type, EvidenceBase>();
                foreach (KeyValuePair<Type, EvidenceTypeDescriptor> pair in this.m_evidence)
                {
                    if ((pair.Value != null) && (pair.Value.HostEvidence != null))
                    {
                        graph[pair.Key] = pair.Value.HostEvidence;
                    }
                }
                using (MemoryStream stream = new MemoryStream())
                {
                    new BinaryFormatter().Serialize(stream, graph);
                    buffer = stream.ToArray();
                }
            }
            return buffer;
        }

        private void ReleaseReaderLock()
        {
            if (this.m_evidenceLock != null)
            {
                this.m_evidenceLock.ReleaseReaderLock();
            }
        }

        private void ReleaseWriterLock()
        {
            if (this.m_evidenceLock != null)
            {
                this.m_evidenceLock.ReleaseWriterLock();
            }
        }

        [SecuritySafeCritical, ComVisible(false)]
        public void RemoveType(Type t)
        {
            if (t == null)
            {
                throw new ArgumentNullException("t");
            }
            using (new EvidenceLockHolder(this, EvidenceLockHolder.LockType.Writer))
            {
                EvidenceTypeDescriptor evidenceTypeDescriptor = this.GetEvidenceTypeDescriptor(t);
                if (evidenceTypeDescriptor != null)
                {
                    this.m_version++;
                    if (this.Locked && ((evidenceTypeDescriptor.HostEvidence != null) || evidenceTypeDescriptor.HostCanGenerate))
                    {
                        new SecurityPermission(SecurityPermissionFlag.ControlEvidence).Demand();
                    }
                    this.m_evidence.Remove(t);
                }
            }
        }

        private static object UnwrapEvidence(EvidenceBase evidence)
        {
            ILegacyEvidenceAdapter adapter = evidence as ILegacyEvidenceAdapter;
            if (adapter != null)
            {
                return adapter.EvidenceObject;
            }
            return evidence;
        }

        private LockCookie UpgradeToWriterLock()
        {
            if (this.m_evidenceLock == null)
            {
                return new LockCookie();
            }
            return this.m_evidenceLock.UpgradeToWriterLock(0x1388);
        }

        private bool WasStrongNameEvidenceUsed()
        {
            using (new EvidenceLockHolder(this, EvidenceLockHolder.LockType.Reader))
            {
                EvidenceTypeDescriptor evidenceTypeDescriptor = this.GetEvidenceTypeDescriptor(typeof(StrongName));
                if (evidenceTypeDescriptor != null)
                {
                    IDelayEvaluatedEvidence hostEvidence = evidenceTypeDescriptor.HostEvidence as IDelayEvaluatedEvidence;
                    return ((hostEvidence != null) && hostEvidence.WasUsed);
                }
                return false;
            }
        }

        private static EvidenceBase WrapLegacyEvidence(object evidence)
        {
            EvidenceBase base2 = evidence as EvidenceBase;
            if (base2 == null)
            {
                base2 = new LegacyEvidenceWrapper(evidence);
            }
            return base2;
        }

        [Obsolete("Evidence should not be treated as an ICollection. Please use GetHostEnumerator and GetAssemblyEnumerator to iterate over the evidence to collect a count.")]
        public int Count
        {
            get
            {
                int num = 0;
                IEnumerator hostEnumerator = this.GetHostEnumerator();
                while (hostEnumerator.MoveNext())
                {
                    num++;
                }
                IEnumerator assemblyEnumerator = this.GetAssemblyEnumerator();
                while (assemblyEnumerator.MoveNext())
                {
                    num++;
                }
                return num;
            }
        }

        private bool IsReaderLockHeld
        {
            get
            {
                if (this.m_evidenceLock != null)
                {
                    return this.m_evidenceLock.IsReaderLockHeld;
                }
                return true;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return true;
            }
        }

        internal bool IsUnmodified
        {
            get
            {
                return (this.m_version == 0);
            }
        }

        private bool IsWriterLockHeld
        {
            get
            {
                if (this.m_evidenceLock != null)
                {
                    return this.m_evidenceLock.IsWriterLockHeld;
                }
                return true;
            }
        }

        public bool Locked
        {
            get
            {
                return this.m_locked;
            }
            [SecuritySafeCritical]
            set
            {
                if (!value)
                {
                    new SecurityPermission(SecurityPermissionFlag.ControlEvidence).Demand();
                    this.m_locked = false;
                }
                else
                {
                    this.m_locked = true;
                }
            }
        }

        [ComVisible(false)]
        internal int RawCount
        {
            get
            {
                int num = 0;
                using (new EvidenceLockHolder(this, EvidenceLockHolder.LockType.Reader))
                {
                    foreach (Type type in new List<Type>(this.m_evidence.Keys))
                    {
                        EvidenceTypeDescriptor evidenceTypeDescriptor = this.GetEvidenceTypeDescriptor(type);
                        if (evidenceTypeDescriptor != null)
                        {
                            if (evidenceTypeDescriptor.AssemblyEvidence != null)
                            {
                                num++;
                            }
                            if (evidenceTypeDescriptor.HostEvidence != null)
                            {
                                num++;
                            }
                        }
                    }
                }
                return num;
            }
        }

        internal static Type[] RuntimeEvidenceTypes
        {
            get
            {
                if (s_runtimeEvidenceTypes == null)
                {
                    List<Type> list = new List<Type>(new Type[] { typeof(ActivationArguments), typeof(ApplicationDirectory), typeof(ApplicationTrust), typeof(GacInstalled), typeof(Hash), typeof(Publisher), typeof(Site), typeof(StrongName), typeof(Url), typeof(Zone) });
                    if (AppDomain.CurrentDomain.IsLegacyCasPolicyEnabled)
                    {
                        list.Add(typeof(PermissionRequestEvidence));
                    }
                    s_runtimeEvidenceTypes = list.ToArray();
                }
                return s_runtimeEvidenceTypes;
            }
        }

        public object SyncRoot
        {
            get
            {
                return this;
            }
        }

        internal IRuntimeEvidenceFactory Target
        {
            get
            {
                return this.m_target;
            }
            [SecurityCritical]
            set
            {
                using (new EvidenceLockHolder(this, EvidenceLockHolder.LockType.Writer))
                {
                    this.m_target = value;
                    this.QueryHostForPossibleEvidenceTypes();
                }
            }
        }

        private enum DuplicateEvidenceAction
        {
            Throw,
            Merge,
            SelectNewObject
        }

        private sealed class EvidenceEnumerator : IEnumerator
        {
            private Category m_category;
            private object m_currentEvidence;
            private Stack m_enumerators;
            private Evidence m_evidence;

            internal EvidenceEnumerator(Evidence evidence, Category category)
            {
                this.m_evidence = evidence;
                this.m_category = category;
                this.ResetNoLock();
            }

            public bool MoveNext()
            {
                IEnumerator currentEnumerator = this.CurrentEnumerator;
                if (currentEnumerator == null)
                {
                    this.m_currentEvidence = null;
                    return false;
                }
                if (currentEnumerator.MoveNext())
                {
                    LegacyEvidenceWrapper current = currentEnumerator.Current as LegacyEvidenceWrapper;
                    LegacyEvidenceList list = currentEnumerator.Current as LegacyEvidenceList;
                    if (current != null)
                    {
                        this.m_currentEvidence = current.EvidenceObject;
                    }
                    else if (list != null)
                    {
                        IEnumerator enumerator = list.GetEnumerator();
                        this.m_enumerators.Push(enumerator);
                        this.MoveNext();
                    }
                    else
                    {
                        this.m_currentEvidence = currentEnumerator.Current;
                    }
                    return true;
                }
                this.m_enumerators.Pop();
                return this.MoveNext();
            }

            public void Reset()
            {
                using (new Evidence.EvidenceLockHolder(this.m_evidence, Evidence.EvidenceLockHolder.LockType.Reader))
                {
                    this.ResetNoLock();
                }
            }

            private void ResetNoLock()
            {
                this.m_currentEvidence = null;
                this.m_enumerators = new Stack();
                if ((this.m_category & Category.Host) == Category.Host)
                {
                    this.m_enumerators.Push(this.m_evidence.GetRawHostEvidenceEnumerator());
                }
                if ((this.m_category & Category.Assembly) == Category.Assembly)
                {
                    this.m_enumerators.Push(this.m_evidence.GetRawAssemblyEvidenceEnumerator());
                }
            }

            public object Current
            {
                get
                {
                    return this.m_currentEvidence;
                }
            }

            private IEnumerator CurrentEnumerator
            {
                get
                {
                    if (this.m_enumerators.Count <= 0)
                    {
                        return null;
                    }
                    return (this.m_enumerators.Peek() as IEnumerator);
                }
            }

            [Flags]
            internal enum Category
            {
                Assembly = 2,
                Host = 1
            }
        }

        private class EvidenceLockHolder : IDisposable
        {
            private LockType m_lockType;
            private Evidence m_target;

            public EvidenceLockHolder(Evidence target, LockType lockType)
            {
                this.m_target = target;
                this.m_lockType = lockType;
                if (this.m_lockType == LockType.Reader)
                {
                    this.m_target.AcquireReaderLock();
                }
                else
                {
                    this.m_target.AcquireWriterlock();
                }
            }

            public void Dispose()
            {
                if ((this.m_lockType == LockType.Reader) && this.m_target.IsReaderLockHeld)
                {
                    this.m_target.ReleaseReaderLock();
                }
                else if ((this.m_lockType == LockType.Writer) && this.m_target.IsWriterLockHeld)
                {
                    this.m_target.ReleaseWriterLock();
                }
            }

            public enum LockType
            {
                Reader,
                Writer
            }
        }

        private class EvidenceUpgradeLockHolder : IDisposable
        {
            private LockCookie m_cookie;
            private Evidence m_target;

            public EvidenceUpgradeLockHolder(Evidence target)
            {
                this.m_target = target;
                this.m_cookie = this.m_target.UpgradeToWriterLock();
            }

            public void Dispose()
            {
                if (this.m_target.IsWriterLockHeld)
                {
                    this.m_target.DowngradeFromWriterLock(ref this.m_cookie);
                }
            }
        }

        internal sealed class RawEvidenceEnumerator : IEnumerator<EvidenceBase>, IDisposable, IEnumerator
        {
            private EvidenceBase m_currentEvidence;
            private Evidence m_evidence;
            private Type[] m_evidenceTypes;
            private uint m_evidenceVersion;
            private bool m_hostEnumerator;
            private int m_typeIndex;
            private static List<Type> s_expensiveEvidence;

            public RawEvidenceEnumerator(Evidence evidence, IEnumerable<Type> evidenceTypes, bool hostEnumerator)
            {
                this.m_evidence = evidence;
                this.m_hostEnumerator = hostEnumerator;
                this.m_evidenceTypes = GenerateEvidenceTypes(evidence, evidenceTypes, hostEnumerator);
                this.m_evidenceVersion = evidence.m_version;
                this.Reset();
            }

            public void Dispose()
            {
            }

            private static Type[] GenerateEvidenceTypes(Evidence evidence, IEnumerable<Type> evidenceTypes, bool hostEvidence)
            {
                List<Type> list = new List<Type>();
                List<Type> list2 = new List<Type>();
                List<Type> list3 = new List<Type>(ExpensiveEvidence.Count);
                foreach (Type type in evidenceTypes)
                {
                    EvidenceTypeDescriptor evidenceTypeDescriptor = evidence.GetEvidenceTypeDescriptor(type);
                    if ((hostEvidence && (evidenceTypeDescriptor.HostEvidence != null)) || (!hostEvidence && (evidenceTypeDescriptor.AssemblyEvidence != null)))
                    {
                        list.Add(type);
                    }
                    else if (ExpensiveEvidence.Contains(type))
                    {
                        list3.Add(type);
                    }
                    else
                    {
                        list2.Add(type);
                    }
                }
                Type[] array = new Type[(list.Count + list2.Count) + list3.Count];
                list.CopyTo(array, 0);
                list2.CopyTo(array, list.Count);
                list3.CopyTo(array, list.Count + list2.Count);
                return array;
            }

            [SecuritySafeCritical]
            public bool MoveNext()
            {
                using (new Evidence.EvidenceLockHolder(this.m_evidence, Evidence.EvidenceLockHolder.LockType.Reader))
                {
                    if (this.m_evidence.m_version != this.m_evidenceVersion)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumFailedVersion"));
                    }
                    this.m_currentEvidence = null;
                    do
                    {
                        this.m_typeIndex++;
                        if (this.m_typeIndex < this.m_evidenceTypes.Length)
                        {
                            if (this.m_hostEnumerator)
                            {
                                this.m_currentEvidence = this.m_evidence.GetHostEvidenceNoLock(this.m_evidenceTypes[this.m_typeIndex]);
                            }
                            else
                            {
                                this.m_currentEvidence = this.m_evidence.GetAssemblyEvidenceNoLock(this.m_evidenceTypes[this.m_typeIndex]);
                            }
                        }
                    }
                    while ((this.m_typeIndex < this.m_evidenceTypes.Length) && (this.m_currentEvidence == null));
                }
                return (this.m_currentEvidence != null);
            }

            public void Reset()
            {
                if (this.m_evidence.m_version != this.m_evidenceVersion)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumFailedVersion"));
                }
                this.m_typeIndex = -1;
                this.m_currentEvidence = null;
            }

            public EvidenceBase Current
            {
                get
                {
                    if (this.m_evidence.m_version != this.m_evidenceVersion)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumFailedVersion"));
                    }
                    return this.m_currentEvidence;
                }
            }

            private static List<Type> ExpensiveEvidence
            {
                get
                {
                    if (s_expensiveEvidence == null)
                    {
                        s_expensiveEvidence = new List<Type> { typeof(Hash), typeof(Publisher) };
                    }
                    return s_expensiveEvidence;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    if (this.m_evidence.m_version != this.m_evidenceVersion)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumFailedVersion"));
                    }
                    return this.m_currentEvidence;
                }
            }
        }
    }
}

