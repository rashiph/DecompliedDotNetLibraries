namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.DirectoryServices;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
    public abstract class DirectoryServer : IDisposable
    {
        internal string cachedIPAddress;
        internal Guid cachedNtdsaObjectGuid = Guid.Empty;
        internal string cachedNtdsaObjectName;
        internal ReadOnlyStringCollection cachedPartitions;
        internal string cachedServerObjectName;
        internal string cachedSiteName;
        internal string cachedSiteObjectName;
        internal DirectoryContext context;
        internal DirectoryEntryManager directoryEntryMgr;
        private bool disposed;
        private const int DS_REPL_INFO_FLAG_IMPROVE_LINKED_ATTRS = 1;
        internal const int DS_REPL_NOTSUPPORTED = 50;
        internal const int DS_REPSYNC_ALL_SOURCES = 0x10;
        internal const int DS_REPSYNC_ASYNCHRONOUS_OPERATION = 1;
        internal const int DS_REPSYNCALL_ID_SERVERS_BY_DN = 4;
        private ReplicationConnectionCollection inbound;
        private ReplicationConnectionCollection outbound;
        internal string replicaName;
        internal bool siteInfoModified;

        protected DirectoryServer()
        {
        }

        internal void CheckConsistencyHelper(IntPtr dsHandle, LoadLibrarySafeHandle libHandle)
        {
            IntPtr procAddress = System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.GetProcAddress(libHandle, "DsReplicaConsistencyCheck");
            if (procAddress == IntPtr.Zero)
            {
                throw ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
            }
            System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsReplicaConsistencyCheck delegateForFunctionPointer = (System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsReplicaConsistencyCheck) Marshal.GetDelegateForFunctionPointer(procAddress, typeof(System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsReplicaConsistencyCheck));
            int errorCode = delegateForFunctionPointer(dsHandle, 0, 0);
            if (errorCode != 0)
            {
                throw ExceptionHelper.GetExceptionFromErrorCode(errorCode, this.Name);
            }
        }

        internal void CheckIfDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
        }

        [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public abstract void CheckReplicationConsistency();
        internal ReplicationFailureCollection ConstructFailures(IntPtr info, DirectoryServer server, LoadLibrarySafeHandle libHandle)
        {
            ReplicationFailureCollection failures = new ReplicationFailureCollection(server);
            try
            {
                if (info != IntPtr.Zero)
                {
                    DS_REPL_KCC_DSA_FAILURES structure = new DS_REPL_KCC_DSA_FAILURES();
                    Marshal.PtrToStructure(info, structure);
                    if (structure.cNumEntries > 0)
                    {
                        failures.AddHelper(structure, info);
                    }
                }
            }
            finally
            {
                this.FreeReplicaInfo(DS_REPL_INFO_TYPE.DS_REPL_INFO_KCC_DSA_CONNECT_FAILURES, info, libHandle);
            }
            return failures;
        }

        internal ActiveDirectoryReplicationMetadata ConstructMetaData(bool advanced, IntPtr info, DirectoryServer server, LoadLibrarySafeHandle libHandle)
        {
            ActiveDirectoryReplicationMetadata metadata = new ActiveDirectoryReplicationMetadata(server);
            int count = 0;
            if (advanced)
            {
                try
                {
                    if (info != IntPtr.Zero)
                    {
                        DS_REPL_OBJ_META_DATA_2 structure = new DS_REPL_OBJ_META_DATA_2();
                        Marshal.PtrToStructure(info, structure);
                        count = structure.cNumEntries;
                        if (count > 0)
                        {
                            metadata.AddHelper(count, info, true);
                        }
                    }
                    return metadata;
                }
                finally
                {
                    this.FreeReplicaInfo(DS_REPL_INFO_TYPE.DS_REPL_INFO_METADATA_2_FOR_OBJ, info, libHandle);
                }
            }
            try
            {
                DS_REPL_OBJ_META_DATA ds_repl_obj_meta_data = new DS_REPL_OBJ_META_DATA();
                Marshal.PtrToStructure(info, ds_repl_obj_meta_data);
                count = ds_repl_obj_meta_data.cNumEntries;
                if (count > 0)
                {
                    metadata.AddHelper(count, info, false);
                }
            }
            finally
            {
                this.FreeReplicaInfo(DS_REPL_INFO_TYPE.DS_REPL_INFO_METADATA_FOR_OBJ, info, libHandle);
            }
            return metadata;
        }

        internal ReplicationNeighborCollection ConstructNeighbors(IntPtr info, DirectoryServer server, LoadLibrarySafeHandle libHandle)
        {
            ReplicationNeighborCollection neighbors = new ReplicationNeighborCollection(server);
            try
            {
                if (info != IntPtr.Zero)
                {
                    DS_REPL_NEIGHBORS structure = new DS_REPL_NEIGHBORS();
                    Marshal.PtrToStructure(info, structure);
                    if (structure.cNumNeighbors > 0)
                    {
                        neighbors.AddHelper(structure, info);
                    }
                }
            }
            finally
            {
                this.FreeReplicaInfo(DS_REPL_INFO_TYPE.DS_REPL_INFO_NEIGHBORS, info, libHandle);
            }
            return neighbors;
        }

        internal ReplicationOperationInformation ConstructPendingOperations(IntPtr info, DirectoryServer server, LoadLibrarySafeHandle libHandle)
        {
            ReplicationOperationInformation information = new ReplicationOperationInformation();
            ReplicationOperationCollection operations = new ReplicationOperationCollection(server);
            information.collection = operations;
            try
            {
                if (info != IntPtr.Zero)
                {
                    DS_REPL_PENDING_OPS structure = new DS_REPL_PENDING_OPS();
                    Marshal.PtrToStructure(info, structure);
                    if (structure.cNumPendingOps > 0)
                    {
                        operations.AddHelper(structure, info);
                        information.startTime = DateTime.FromFileTime(structure.ftimeCurrentOpStarted);
                        information.currentOp = operations.GetFirstOperation();
                    }
                }
            }
            finally
            {
                this.FreeReplicaInfo(DS_REPL_INFO_TYPE.DS_REPL_INFO_PENDING_OPS, info, libHandle);
            }
            return information;
        }

        internal ReplicationCursorCollection ConstructReplicationCursors(IntPtr dsHandle, bool advanced, IntPtr info, string partition, DirectoryServer server, LoadLibrarySafeHandle libHandle)
        {
            int context = 0;
            int cNumCursors = 0;
            ReplicationCursorCollection cursors = new ReplicationCursorCollection(server);
            if (!advanced)
            {
                try
                {
                    if (info != IntPtr.Zero)
                    {
                        DS_REPL_CURSORS structure = new DS_REPL_CURSORS();
                        Marshal.PtrToStructure(info, structure);
                        cursors.AddHelper(partition, structure, advanced, info);
                    }
                }
                finally
                {
                    this.FreeReplicaInfo(DS_REPL_INFO_TYPE.DS_REPL_INFO_CURSORS_FOR_NC, info, libHandle);
                }
                return cursors;
            }
        Label_000F:
            try
            {
                if (info != IntPtr.Zero)
                {
                    DS_REPL_CURSORS_3 ds_repl_cursors_ = new DS_REPL_CURSORS_3();
                    Marshal.PtrToStructure(info, ds_repl_cursors_);
                    cNumCursors = ds_repl_cursors_.cNumCursors;
                    if (cNumCursors > 0)
                    {
                        cursors.AddHelper(partition, ds_repl_cursors_, advanced, info);
                    }
                    context = ds_repl_cursors_.dwEnumerationContext;
                    if ((context != -1) && (cNumCursors != 0))
                    {
                        goto Label_005F;
                    }
                }
                return cursors;
            }
            finally
            {
                this.FreeReplicaInfo(DS_REPL_INFO_TYPE.DS_REPL_INFO_CURSORS_3_FOR_NC, info, libHandle);
            }
        Label_005F:
            info = this.GetReplicationInfoHelper(dsHandle, 8, 1, partition, ref advanced, context, libHandle);
            goto Label_000F;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    foreach (DirectoryEntry entry in this.directoryEntryMgr.GetCachedDirectoryEntries())
                    {
                        entry.Dispose();
                    }
                }
                this.disposed = true;
            }
        }

        ~DirectoryServer()
        {
            this.Dispose(false);
        }

        private void FreeReplicaInfo(DS_REPL_INFO_TYPE type, IntPtr value, LoadLibrarySafeHandle libHandle)
        {
            if (value != IntPtr.Zero)
            {
                IntPtr procAddress = System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.GetProcAddress(libHandle, "DsReplicaFreeInfo");
                if (procAddress == IntPtr.Zero)
                {
                    throw ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
                }
                System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsReplicaFreeInfo delegateForFunctionPointer = (System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsReplicaFreeInfo) Marshal.GetDelegateForFunctionPointer(procAddress, typeof(System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsReplicaFreeInfo));
                delegateForFunctionPointer((int) type, value);
            }
        }

        [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
        public abstract ReplicationNeighborCollection GetAllReplicationNeighbors();
        public DirectoryEntry GetDirectoryEntry()
        {
            this.CheckIfDisposed();
            string dn = (this is DomainController) ? ((DomainController) this).ServerObjectName : ((AdamInstance) this).ServerObjectName;
            return DirectoryEntryManager.GetDirectoryEntry(this.context, dn);
        }

        internal ReplicationConnectionCollection GetInboundConnectionsHelper()
        {
            if (this.inbound == null)
            {
                this.inbound = new ReplicationConnectionCollection();
                DirectoryContext context = Utils.GetNewDirectoryContext(this.Name, DirectoryContextType.DirectoryServer, this.context);
                string str = (this is DomainController) ? ((DomainController) this).ServerObjectName : ((AdamInstance) this).ServerObjectName;
                string dn = "CN=NTDS Settings," + str;
                DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(Utils.GetNewDirectoryContext(this.Name, DirectoryContextType.DirectoryServer, this.context), dn);
                ADSearcher searcher = new ADSearcher(directoryEntry, "(&(objectClass=nTDSConnection)(objectCategory=nTDSConnection))", new string[] { "cn" }, SearchScope.OneLevel);
                SearchResultCollection results = null;
                try
                {
                    results = searcher.FindAll();
                    foreach (SearchResult result in results)
                    {
                        ReplicationConnection connection = new ReplicationConnection(context, result.GetDirectoryEntry(), (string) PropertyManager.GetSearchResultPropertyValue(result, PropertyManager.Cn));
                        this.inbound.Add(connection);
                    }
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(context, exception);
                }
                finally
                {
                    if (results != null)
                    {
                        results.Dispose();
                    }
                    directoryEntry.Dispose();
                }
            }
            return this.inbound;
        }

        internal ReplicationConnectionCollection GetOutboundConnectionsHelper()
        {
            if (this.outbound == null)
            {
                string dn = (this is DomainController) ? ((DomainController) this).SiteObjectName : ((AdamInstance) this).SiteObjectName;
                DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(Utils.GetNewDirectoryContext(this.Name, DirectoryContextType.DirectoryServer, this.context), dn);
                string str2 = (this is DomainController) ? ((DomainController) this).ServerObjectName : ((AdamInstance) this).ServerObjectName;
                ADSearcher searcher = new ADSearcher(directoryEntry, "(&(objectClass=nTDSConnection)(objectCategory=nTDSConnection)(fromServer=CN=NTDS Settings," + str2 + "))", new string[] { "objectClass", "cn" }, SearchScope.Subtree);
                SearchResultCollection results = null;
                DirectoryContext context = Utils.GetNewDirectoryContext(this.Name, DirectoryContextType.DirectoryServer, this.context);
                try
                {
                    results = searcher.FindAll();
                    this.outbound = new ReplicationConnectionCollection();
                    foreach (SearchResult result in results)
                    {
                        ReplicationConnection connection = new ReplicationConnection(context, result.GetDirectoryEntry(), (string) result.Properties["cn"][0]);
                        this.outbound.Add(connection);
                    }
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(context, exception);
                }
                finally
                {
                    if (results != null)
                    {
                        results.Dispose();
                    }
                    directoryEntry.Dispose();
                }
            }
            return this.outbound;
        }

        internal ArrayList GetPartitions()
        {
            ArrayList list = new ArrayList();
            DirectoryEntry directoryEntry = null;
            DirectoryEntry searchRootEntry = null;
            try
            {
                directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, WellKnownDN.RootDSE);
                foreach (string str in directoryEntry.Properties[PropertyManager.NamingContexts])
                {
                    list.Add(str);
                }
                string dn = (this is DomainController) ? ((DomainController) this).NtdsaObjectName : ((AdamInstance) this).NtdsaObjectName;
                searchRootEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, dn);
                ArrayList propertiesToLoad = new ArrayList();
                propertiesToLoad.Add(PropertyManager.HasPartialReplicaNCs);
                Hashtable hashtable = null;
                try
                {
                    hashtable = Utils.GetValuesWithRangeRetrieval(searchRootEntry, null, propertiesToLoad, SearchScope.Base);
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
                ArrayList list3 = (ArrayList) hashtable[PropertyManager.HasPartialReplicaNCs.ToLower(CultureInfo.InvariantCulture)];
                foreach (string str3 in list3)
                {
                    list.Add(str3);
                }
                return list;
            }
            catch (COMException exception2)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception2);
            }
            finally
            {
                if (directoryEntry != null)
                {
                    directoryEntry.Dispose();
                }
                if (searchRootEntry != null)
                {
                    searchRootEntry.Dispose();
                }
            }
            return list;
        }

        [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
        public abstract ReplicationFailureCollection GetReplicationConnectionFailures();
        [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public abstract ReplicationCursorCollection GetReplicationCursors(string partition);
        internal IntPtr GetReplicationInfoHelper(IntPtr dsHandle, int type, int secondaryType, string partition, ref bool advanced, int context, LoadLibrarySafeHandle libHandle)
        {
            IntPtr zero = IntPtr.Zero;
            int errorCode = 0;
            bool flag = true;
            IntPtr procAddress = System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.GetProcAddress(libHandle, "DsReplicaGetInfo2W");
            if (procAddress == IntPtr.Zero)
            {
                procAddress = System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.GetProcAddress(libHandle, "DsReplicaGetInfoW");
                if (procAddress == IntPtr.Zero)
                {
                    throw ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
                }
                System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsReplicaGetInfoW delegateForFunctionPointer = (System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsReplicaGetInfoW) Marshal.GetDelegateForFunctionPointer(procAddress, typeof(System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsReplicaGetInfoW));
                errorCode = delegateForFunctionPointer(dsHandle, secondaryType, partition, IntPtr.Zero, ref zero);
                advanced = false;
                flag = false;
            }
            else
            {
                System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsReplicaGetInfo2W infow = (System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsReplicaGetInfo2W) Marshal.GetDelegateForFunctionPointer(procAddress, typeof(System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsReplicaGetInfo2W));
                errorCode = infow(dsHandle, type, partition, IntPtr.Zero, null, null, 0, context, ref zero);
            }
            if (flag && (errorCode == 50))
            {
                procAddress = System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.GetProcAddress(libHandle, "DsReplicaGetInfoW");
                if (procAddress == IntPtr.Zero)
                {
                    throw ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
                }
                System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsReplicaGetInfoW ow2 = (System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsReplicaGetInfoW) Marshal.GetDelegateForFunctionPointer(procAddress, typeof(System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsReplicaGetInfoW));
                errorCode = ow2(dsHandle, secondaryType, partition, IntPtr.Zero, ref zero);
                advanced = false;
            }
            if (errorCode == 0)
            {
                return zero;
            }
            if (partition != null)
            {
                if (type == 9)
                {
                    if ((errorCode == ExceptionHelper.ERROR_DS_DRA_BAD_DN) || (errorCode == ExceptionHelper.ERROR_DS_NAME_UNPARSEABLE))
                    {
                        throw new ArgumentException(ExceptionHelper.GetErrorMessage(errorCode, false), "objectPath");
                    }
                    DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, partition);
                    try
                    {
                        directoryEntry.RefreshCache(new string[] { "name" });
                    }
                    catch (COMException exception)
                    {
                        if ((exception.ErrorCode == -2147016672) | (exception.ErrorCode == -2147016656))
                        {
                            throw new ArgumentException(Res.GetString("DSNoObject"), "objectPath");
                        }
                        if ((exception.ErrorCode == -2147463168) | (exception.ErrorCode == -2147016654))
                        {
                            throw new ArgumentException(Res.GetString("DSInvalidPath"), "objectPath");
                        }
                    }
                }
                else if (!this.Partitions.Contains(partition))
                {
                    throw new ArgumentException(Res.GetString("ServerNotAReplica"), "partition");
                }
            }
            throw ExceptionHelper.GetExceptionFromErrorCode(errorCode, this.Name);
        }

        [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public abstract ActiveDirectoryReplicationMetadata GetReplicationMetadata(string objectPath);
        [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public abstract ReplicationNeighborCollection GetReplicationNeighbors(string partition);
        [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public abstract ReplicationOperationInformation GetReplicationOperationInformation();
        public void MoveToAnotherSite(string siteName)
        {
            this.CheckIfDisposed();
            if (siteName == null)
            {
                throw new ArgumentNullException("siteName");
            }
            if (siteName.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "siteName");
            }
            if (Utils.Compare(this.SiteName, siteName) != 0)
            {
                DirectoryEntry newParent = null;
                try
                {
                    string dn = "CN=Servers,CN=" + siteName + "," + this.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.SitesContainer);
                    newParent = DirectoryEntryManager.GetDirectoryEntry(this.context, dn);
                    string distinguishedName = (this is DomainController) ? ((DomainController) this).ServerObjectName : ((AdamInstance) this).ServerObjectName;
                    DirectoryEntry cachedDirectoryEntry = this.directoryEntryMgr.GetCachedDirectoryEntry(distinguishedName);
                    string text1 = (string) PropertyManager.GetPropertyValue(this.context, cachedDirectoryEntry, PropertyManager.DistinguishedName);
                    cachedDirectoryEntry.MoveTo(newParent);
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
                finally
                {
                    if (newParent != null)
                    {
                        newParent.Dispose();
                    }
                }
                this.siteInfoModified = true;
                this.cachedSiteName = null;
                if (this.cachedSiteObjectName != null)
                {
                    this.directoryEntryMgr.RemoveIfExists(this.cachedSiteObjectName);
                    this.cachedSiteObjectName = null;
                }
                if (this.cachedServerObjectName != null)
                {
                    this.directoryEntryMgr.RemoveIfExists(this.cachedServerObjectName);
                    this.cachedServerObjectName = null;
                }
                if (this.cachedNtdsaObjectName != null)
                {
                    this.directoryEntryMgr.RemoveIfExists(this.cachedNtdsaObjectName);
                    this.cachedNtdsaObjectName = null;
                }
            }
        }

        internal bool SyncAllCallbackRoutine(IntPtr data, IntPtr update)
        {
            if (this.SyncFromAllServersCallback == null)
            {
                return true;
            }
            DS_REPSYNCALL_UPDATE structure = new DS_REPSYNCALL_UPDATE();
            Marshal.PtrToStructure(update, structure);
            SyncFromAllServersEvent eventType = structure.eventType;
            IntPtr pErrInfo = structure.pErrInfo;
            SyncFromAllServersOperationException exception = null;
            if (pErrInfo != IntPtr.Zero)
            {
                exception = ExceptionHelper.CreateSyncAllException(pErrInfo, true);
                if (exception == null)
                {
                    return true;
                }
            }
            string targetServer = null;
            string sourceServer = null;
            pErrInfo = structure.pSync;
            if (pErrInfo != IntPtr.Zero)
            {
                DS_REPSYNCALL_SYNC ds_repsyncall_sync = new DS_REPSYNCALL_SYNC();
                Marshal.PtrToStructure(pErrInfo, ds_repsyncall_sync);
                targetServer = Marshal.PtrToStringUni(ds_repsyncall_sync.pszDstId);
                sourceServer = Marshal.PtrToStringUni(ds_repsyncall_sync.pszSrcId);
            }
            return this.SyncFromAllServersCallback(eventType, targetServer, sourceServer, exception);
        }

        internal void SyncReplicaAllHelper(IntPtr handle, SyncReplicaFromAllServersCallback syncAllFunctionPointer, string partition, SyncFromAllServersOptions option, SyncUpdateCallback callback, LoadLibrarySafeHandle libHandle)
        {
            IntPtr zero = IntPtr.Zero;
            if (!this.Partitions.Contains(partition))
            {
                throw new ArgumentException(Res.GetString("ServerNotAReplica"), "partition");
            }
            IntPtr procAddress = System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.GetProcAddress(libHandle, "DsReplicaSyncAllW");
            if (procAddress == IntPtr.Zero)
            {
                throw ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
            }
            System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsReplicaSyncAllW delegateForFunctionPointer = (System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsReplicaSyncAllW) Marshal.GetDelegateForFunctionPointer(procAddress, typeof(System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsReplicaSyncAllW));
            int errorCode = delegateForFunctionPointer(handle, partition, ((int) option) | 4, syncAllFunctionPointer, IntPtr.Zero, ref zero);
            try
            {
                if (zero != IntPtr.Zero)
                {
                    SyncFromAllServersOperationException exception = ExceptionHelper.CreateSyncAllException(zero, false);
                    if (exception != null)
                    {
                        throw exception;
                    }
                }
                else if (errorCode != 0)
                {
                    throw new SyncFromAllServersOperationException(ExceptionHelper.GetErrorMessage(errorCode, false));
                }
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.LocalFree(zero);
                }
            }
        }

        [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
        public abstract void SyncReplicaFromAllServers(string partition, SyncFromAllServersOptions options);
        [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
        public abstract void SyncReplicaFromServer(string partition, string sourceServer);
        internal void SyncReplicaHelper(IntPtr dsHandle, bool isADAM, string partition, string sourceServer, int option, LoadLibrarySafeHandle libHandle)
        {
            int cb = Marshal.SizeOf(typeof(Guid));
            IntPtr zero = IntPtr.Zero;
            Guid empty = Guid.Empty;
            AdamInstance adamInstance = null;
            DomainController domainController = null;
            zero = Marshal.AllocHGlobal(cb);
            try
            {
                if (sourceServer != null)
                {
                    DirectoryContext context = Utils.GetNewDirectoryContext(sourceServer, DirectoryContextType.DirectoryServer, this.context);
                    if (isADAM)
                    {
                        adamInstance = AdamInstance.GetAdamInstance(context);
                        empty = adamInstance.NtdsaObjectGuid;
                    }
                    else
                    {
                        domainController = DomainController.GetDomainController(context);
                        empty = domainController.NtdsaObjectGuid;
                    }
                    Marshal.StructureToPtr(empty, zero, false);
                }
                IntPtr procAddress = System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.GetProcAddress(libHandle, "DsReplicaSyncW");
                if (procAddress == IntPtr.Zero)
                {
                    throw ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
                }
                System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsReplicaSyncW delegateForFunctionPointer = (System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsReplicaSyncW) Marshal.GetDelegateForFunctionPointer(procAddress, typeof(System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsReplicaSyncW));
                int errorCode = delegateForFunctionPointer(dsHandle, partition, zero, option);
                if (errorCode != 0)
                {
                    if (!this.Partitions.Contains(partition))
                    {
                        throw new ArgumentException(Res.GetString("ServerNotAReplica"), "partition");
                    }
                    string targetName = null;
                    if (errorCode == ExceptionHelper.RPC_S_SERVER_UNAVAILABLE)
                    {
                        targetName = sourceServer;
                    }
                    else if (errorCode == ExceptionHelper.RPC_S_CALL_FAILED)
                    {
                        targetName = this.Name;
                    }
                    throw ExceptionHelper.GetExceptionFromErrorCode(errorCode, targetName);
                }
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(zero);
                }
                if (adamInstance != null)
                {
                    adamInstance.Dispose();
                }
                if (domainController != null)
                {
                    domainController.Dispose();
                }
            }
        }

        public override string ToString()
        {
            return this.Name;
        }

        [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
        public abstract void TriggerSyncReplicaFromNeighbors(string partition);

        internal DirectoryContext Context
        {
            get
            {
                return this.context;
            }
        }

        public abstract ReplicationConnectionCollection InboundConnections { [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)] get; }

        public abstract string IPAddress { [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)] get; }

        public string Name
        {
            get
            {
                this.CheckIfDisposed();
                return this.replicaName;
            }
        }

        public abstract ReplicationConnectionCollection OutboundConnections { [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)] get; }

        public ReadOnlyStringCollection Partitions
        {
            get
            {
                this.CheckIfDisposed();
                if (this.cachedPartitions == null)
                {
                    this.cachedPartitions = new ReadOnlyStringCollection(this.GetPartitions());
                }
                return this.cachedPartitions;
            }
        }

        public abstract string SiteName { [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)] get; }

        public abstract SyncUpdateCallback SyncFromAllServersCallback { [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)] get; [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)] set; }
    }
}

