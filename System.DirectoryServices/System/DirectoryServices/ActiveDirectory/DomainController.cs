namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.DirectoryServices;
    using System.Globalization;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
    public class DomainController : DirectoryServer
    {
        private IntPtr authIdentity;
        private string[] becomeRoleOwnerAttrs;
        private string cachedComputerObjectName;
        private System.DirectoryServices.ActiveDirectory.Domain cachedDomain;
        private double cachedNumericOSVersion;
        private string cachedOSVersion;
        private ActiveDirectoryRoleCollection cachedRoles;
        private System.DirectoryServices.ActiveDirectory.Forest currentForest;
        private bool dcInfoInitialized;
        private bool disposed;
        private IntPtr dsHandle;
        internal SyncReplicaFromAllServersCallback syncAllFunctionPointer;
        internal SyncUpdateCallback userDelegate;

        protected DomainController()
        {
            this.dsHandle = IntPtr.Zero;
            this.authIdentity = IntPtr.Zero;
        }

        internal DomainController(DirectoryContext context, string domainControllerName) : this(context, domainControllerName, new DirectoryEntryManager(context))
        {
        }

        internal DomainController(DirectoryContext context, string domainControllerName, DirectoryEntryManager directoryEntryMgr)
        {
            this.dsHandle = IntPtr.Zero;
            this.authIdentity = IntPtr.Zero;
            base.context = context;
            base.replicaName = domainControllerName;
            base.directoryEntryMgr = directoryEntryMgr;
            this.becomeRoleOwnerAttrs = new string[] { PropertyManager.BecomeSchemaMaster, PropertyManager.BecomeDomainMaster, PropertyManager.BecomePdc, PropertyManager.BecomeRidMaster, PropertyManager.BecomeInfrastructureMaster };
            this.syncAllFunctionPointer = new SyncReplicaFromAllServersCallback(this.SyncAllCallbackRoutine);
        }

        [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public override void CheckReplicationConsistency()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            this.GetDSHandle();
            base.CheckConsistencyHelper(this.dsHandle, DirectoryContext.ADHandle);
        }

        protected override void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                try
                {
                    this.FreeDSHandle();
                    this.disposed = true;
                }
                finally
                {
                    base.Dispose();
                }
            }
        }

        [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public virtual GlobalCatalog EnableGlobalCatalog()
        {
            base.CheckIfDisposed();
            try
            {
                DirectoryEntry cachedDirectoryEntry = base.directoryEntryMgr.GetCachedDirectoryEntry(this.NtdsaObjectName);
                int num = 0;
                if (cachedDirectoryEntry.Properties[PropertyManager.Options].Value != null)
                {
                    num = (int) cachedDirectoryEntry.Properties[PropertyManager.Options].Value;
                }
                cachedDirectoryEntry.Properties[PropertyManager.Options].Value = num | 1;
                cachedDirectoryEntry.CommitChanges();
            }
            catch (COMException exception)
            {
                throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromCOMException(base.context, exception);
            }
            return new GlobalCatalog(base.context, base.Name);
        }

        ~DomainController()
        {
            this.Dispose(false);
        }

        public static DomainControllerCollection FindAll(DirectoryContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (context.ContextType != DirectoryContextType.Domain)
            {
                throw new ArgumentException(Res.GetString("TargetShouldBeDomain"), "context");
            }
            context = new DirectoryContext(context);
            return FindAllInternal(context, context.Name, false, null);
        }

        public static DomainControllerCollection FindAll(DirectoryContext context, string siteName)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (context.ContextType != DirectoryContextType.Domain)
            {
                throw new ArgumentException(Res.GetString("TargetShouldBeDomain"), "context");
            }
            if (siteName == null)
            {
                throw new ArgumentNullException("siteName");
            }
            context = new DirectoryContext(context);
            return FindAllInternal(context, context.Name, false, siteName);
        }

        internal static DomainControllerCollection FindAllInternal(DirectoryContext context, string domainName, bool isDnsDomainName, string siteName)
        {
            ArrayList values = new ArrayList();
            if ((siteName != null) && (siteName.Length == 0))
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "siteName");
            }
            if ((domainName == null) || !isDnsDomainName)
            {
                DomainControllerInfo info;
                int errorCode = Locator.DsGetDcNameWrapper(null, (domainName != null) ? domainName : DirectoryContext.GetLoggedOnDomain(), null, 0x10L, out info);
                if (errorCode == 0x54b)
                {
                    return new DomainControllerCollection(values);
                }
                if (errorCode != 0)
                {
                    throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromErrorCode(errorCode);
                }
                domainName = info.DomainName;
            }
            foreach (string str in Utils.GetReplicaList(context, Utils.GetDNFromDnsName(domainName), siteName, true, false, false))
            {
                DirectoryContext context2 = Utils.GetNewDirectoryContext(str, DirectoryContextType.DirectoryServer, context);
                values.Add(new DomainController(context2, str));
            }
            return new DomainControllerCollection(values);
        }

        public static DomainController FindOne(DirectoryContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (context.ContextType != DirectoryContextType.Domain)
            {
                throw new ArgumentException(Res.GetString("TargetShouldBeDomain"), "context");
            }
            return FindOneWithCredentialValidation(context, null, 0L);
        }

        public static DomainController FindOne(DirectoryContext context, LocatorOptions flag)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (context.ContextType != DirectoryContextType.Domain)
            {
                throw new ArgumentException(Res.GetString("TargetShouldBeDomain"), "context");
            }
            return FindOneWithCredentialValidation(context, null, flag);
        }

        public static DomainController FindOne(DirectoryContext context, string siteName)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (context.ContextType != DirectoryContextType.Domain)
            {
                throw new ArgumentException(Res.GetString("TargetShouldBeDomain"), "context");
            }
            if (siteName == null)
            {
                throw new ArgumentNullException("siteName");
            }
            return FindOneWithCredentialValidation(context, siteName, 0L);
        }

        public static DomainController FindOne(DirectoryContext context, string siteName, LocatorOptions flag)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (context.ContextType != DirectoryContextType.Domain)
            {
                throw new ArgumentException(Res.GetString("TargetShouldBeDomain"), "context");
            }
            if (siteName == null)
            {
                throw new ArgumentNullException("siteName");
            }
            return FindOneWithCredentialValidation(context, siteName, flag);
        }

        internal static DomainController FindOneInternal(DirectoryContext context, string domainName, string siteName, LocatorOptions flag)
        {
            DomainControllerInfo info;
            int errorCode = 0;
            if ((siteName != null) && (siteName.Length == 0))
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "siteName");
            }
            if ((flag & ~(LocatorOptions.AvoidSelf | LocatorOptions.ForceRediscovery | LocatorOptions.KdcRequired | LocatorOptions.TimeServerRequired | LocatorOptions.WriteableRequired)) != 0L)
            {
                throw new ArgumentException(Res.GetString("InvalidFlags"), "flag");
            }
            if (domainName == null)
            {
                domainName = DirectoryContext.GetLoggedOnDomain();
            }
            errorCode = Locator.DsGetDcNameWrapper(null, domainName, siteName, flag | 0x10L, out info);
            switch (errorCode)
            {
                case 0x54b:
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DCNotFoundInDomain", new object[] { domainName }), typeof(DomainController), null);

                case 0x3ec:
                    throw new ArgumentException(Res.GetString("InvalidFlags"), "flag");
            }
            if (errorCode != 0)
            {
                throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromErrorCode(errorCode);
            }
            string domainControllerName = info.DomainControllerName.Substring(2);
            return new DomainController(Utils.GetNewDirectoryContext(domainControllerName, DirectoryContextType.DirectoryServer, context), domainControllerName);
        }

        internal static DomainController FindOneWithCredentialValidation(DirectoryContext context, string siteName, LocatorOptions flag)
        {
            bool flag2 = false;
            bool flag3 = false;
            context = new DirectoryContext(context);
            DomainController dc = FindOneInternal(context, context.Name, siteName, flag);
            try
            {
                ValidateCredential(dc, context);
                flag3 = true;
            }
            catch (COMException exception)
            {
                if (exception.ErrorCode != -2147016646)
                {
                    throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromCOMException(context, exception);
                }
                if ((flag & LocatorOptions.ForceRediscovery) != 0L)
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DCNotFoundInDomain", new object[] { context.Name }), typeof(DomainController), null);
                }
                flag2 = true;
            }
            finally
            {
                if (!flag3)
                {
                    dc.Dispose();
                }
            }
            if (flag2)
            {
                flag3 = false;
                dc = FindOneInternal(context, context.Name, siteName, flag | LocatorOptions.ForceRediscovery);
                try
                {
                    ValidateCredential(dc, context);
                    flag3 = true;
                }
                catch (COMException exception2)
                {
                    if (exception2.ErrorCode == -2147016646)
                    {
                        throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DCNotFoundInDomain", new object[] { context.Name }), typeof(DomainController), null);
                    }
                    throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromCOMException(context, exception2);
                }
                finally
                {
                    if (!flag3)
                    {
                        dc.Dispose();
                    }
                }
            }
            return dc;
        }

        internal void FreeDSHandle()
        {
            lock (this)
            {
                Utils.FreeDSHandle(this.dsHandle, DirectoryContext.ADHandle);
                Utils.FreeAuthIdentity(this.authIdentity, DirectoryContext.ADHandle);
            }
        }

        [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public override ReplicationNeighborCollection GetAllReplicationNeighbors()
        {
            IntPtr zero = IntPtr.Zero;
            bool advanced = true;
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            this.GetDSHandle();
            zero = base.GetReplicationInfoHelper(this.dsHandle, 0, 0, null, ref advanced, 0, DirectoryContext.ADHandle);
            return base.ConstructNeighbors(zero, this, DirectoryContext.ADHandle);
        }

        [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public virtual DirectorySearcher GetDirectorySearcher()
        {
            base.CheckIfDisposed();
            return this.InternalGetDirectorySearcher();
        }

        public static DomainController GetDomainController(DirectoryContext context)
        {
            string domainControllerName = null;
            DirectoryEntryManager directoryEntryMgr = null;
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (context.ContextType != DirectoryContextType.DirectoryServer)
            {
                throw new ArgumentException(Res.GetString("TargetShouldBeDC"), "context");
            }
            if (!context.isServer())
            {
                throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DCNotFound", new object[] { context.Name }), typeof(DomainController), context.Name);
            }
            context = new DirectoryContext(context);
            try
            {
                directoryEntryMgr = new DirectoryEntryManager(context);
                DirectoryEntry cachedDirectoryEntry = directoryEntryMgr.GetCachedDirectoryEntry(WellKnownDN.RootDSE);
                if (!Utils.CheckCapability(cachedDirectoryEntry, Capability.ActiveDirectory))
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DCNotFound", new object[] { context.Name }), typeof(DomainController), context.Name);
                }
                domainControllerName = (string) PropertyManager.GetPropertyValue(context, cachedDirectoryEntry, PropertyManager.DnsHostName);
            }
            catch (COMException exception)
            {
                if (exception.ErrorCode == -2147016646)
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DCNotFound", new object[] { context.Name }), typeof(DomainController), context.Name);
                }
                throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromCOMException(context, exception);
            }
            return new DomainController(context, domainControllerName, directoryEntryMgr);
        }

        private void GetDomainControllerInfo()
        {
            int errorCode = 0;
            int dcCount = 0;
            IntPtr zero = IntPtr.Zero;
            int infoLevel = 0;
            bool flag = false;
            this.GetDSHandle();
            IntPtr procAddress = System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.GetProcAddress(DirectoryContext.ADHandle, "DsGetDomainControllerInfoW");
            if (procAddress == IntPtr.Zero)
            {
                throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
            }
            System.DirectoryServices.ActiveDirectory.NativeMethods.DsGetDomainControllerInfo delegateForFunctionPointer = (System.DirectoryServices.ActiveDirectory.NativeMethods.DsGetDomainControllerInfo) Marshal.GetDelegateForFunctionPointer(procAddress, typeof(System.DirectoryServices.ActiveDirectory.NativeMethods.DsGetDomainControllerInfo));
            infoLevel = 3;
            errorCode = delegateForFunctionPointer(this.dsHandle, this.Domain.Name, infoLevel, out dcCount, out zero);
            if (errorCode != 0)
            {
                infoLevel = 2;
                errorCode = delegateForFunctionPointer(this.dsHandle, this.Domain.Name, infoLevel, out dcCount, out zero);
            }
            if (errorCode != 0)
            {
                throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromErrorCode(errorCode, base.Name);
            }
            try
            {
                IntPtr ptr = zero;
                for (int i = 0; i < dcCount; i++)
                {
                    if (infoLevel == 3)
                    {
                        DsDomainControllerInfo3 structure = new DsDomainControllerInfo3();
                        Marshal.PtrToStructure(ptr, structure);
                        if ((structure != null) && (Utils.Compare(structure.dnsHostName, base.replicaName) == 0))
                        {
                            flag = true;
                            base.cachedSiteName = structure.siteName;
                            base.cachedSiteObjectName = structure.siteObjectName;
                            this.cachedComputerObjectName = structure.computerObjectName;
                            base.cachedServerObjectName = structure.serverObjectName;
                            base.cachedNtdsaObjectName = structure.ntdsaObjectName;
                            base.cachedNtdsaObjectGuid = structure.ntdsDsaObjectGuid;
                        }
                        ptr = (IntPtr) (((long) ptr) + Marshal.SizeOf(structure));
                    }
                    else
                    {
                        DsDomainControllerInfo2 info3 = new DsDomainControllerInfo2();
                        Marshal.PtrToStructure(ptr, info3);
                        if ((info3 != null) && (Utils.Compare(info3.dnsHostName, base.replicaName) == 0))
                        {
                            flag = true;
                            base.cachedSiteName = info3.siteName;
                            base.cachedSiteObjectName = info3.siteObjectName;
                            this.cachedComputerObjectName = info3.computerObjectName;
                            base.cachedServerObjectName = info3.serverObjectName;
                            base.cachedNtdsaObjectName = info3.ntdsaObjectName;
                            base.cachedNtdsaObjectGuid = info3.ntdsDsaObjectGuid;
                        }
                        ptr = (IntPtr) (((long) ptr) + Marshal.SizeOf(info3));
                    }
                }
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    procAddress = System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.GetProcAddress(DirectoryContext.ADHandle, "DsFreeDomainControllerInfoW");
                    if (procAddress == IntPtr.Zero)
                    {
                        throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
                    }
                    System.DirectoryServices.ActiveDirectory.NativeMethods.DsFreeDomainControllerInfo info4 = (System.DirectoryServices.ActiveDirectory.NativeMethods.DsFreeDomainControllerInfo) Marshal.GetDelegateForFunctionPointer(procAddress, typeof(System.DirectoryServices.ActiveDirectory.NativeMethods.DsFreeDomainControllerInfo));
                    info4(infoLevel, dcCount, zero);
                }
            }
            if (!flag)
            {
                throw new ActiveDirectoryOperationException(Res.GetString("DCInfoNotFound"));
            }
            this.dcInfoInitialized = true;
            base.siteInfoModified = false;
        }

        internal void GetDSHandle()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            lock (this)
            {
                if (this.dsHandle == IntPtr.Zero)
                {
                    if (this.authIdentity == IntPtr.Zero)
                    {
                        this.authIdentity = Utils.GetAuthIdentity(base.context, DirectoryContext.ADHandle);
                    }
                    this.dsHandle = Utils.GetDSHandle(base.replicaName, null, this.authIdentity, DirectoryContext.ADHandle);
                }
            }
        }

        [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
        public override ReplicationFailureCollection GetReplicationConnectionFailures()
        {
            return this.GetReplicationFailures(DS_REPL_INFO_TYPE.DS_REPL_INFO_KCC_DSA_CONNECT_FAILURES);
        }

        [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
        public override ReplicationCursorCollection GetReplicationCursors(string partition)
        {
            IntPtr zero = IntPtr.Zero;
            int context = 0;
            bool advanced = true;
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            if (partition == null)
            {
                throw new ArgumentNullException("partition");
            }
            if (partition.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "partition");
            }
            this.GetDSHandle();
            zero = base.GetReplicationInfoHelper(this.dsHandle, 8, 1, partition, ref advanced, context, DirectoryContext.ADHandle);
            return base.ConstructReplicationCursors(this.dsHandle, advanced, zero, partition, this, DirectoryContext.ADHandle);
        }

        internal ReplicationFailureCollection GetReplicationFailures(DS_REPL_INFO_TYPE type)
        {
            IntPtr zero = IntPtr.Zero;
            bool advanced = true;
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            this.GetDSHandle();
            zero = base.GetReplicationInfoHelper(this.dsHandle, (int) type, (int) type, null, ref advanced, 0, DirectoryContext.ADHandle);
            return base.ConstructFailures(zero, this, DirectoryContext.ADHandle);
        }

        [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
        public override ActiveDirectoryReplicationMetadata GetReplicationMetadata(string objectPath)
        {
            IntPtr zero = IntPtr.Zero;
            bool advanced = true;
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            if (objectPath == null)
            {
                throw new ArgumentNullException("objectPath");
            }
            if (objectPath.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "objectPath");
            }
            this.GetDSHandle();
            zero = base.GetReplicationInfoHelper(this.dsHandle, 9, 2, objectPath, ref advanced, 0, DirectoryContext.ADHandle);
            return base.ConstructMetaData(advanced, zero, this, DirectoryContext.ADHandle);
        }

        [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public override ReplicationNeighborCollection GetReplicationNeighbors(string partition)
        {
            IntPtr zero = IntPtr.Zero;
            bool advanced = true;
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            if (partition == null)
            {
                throw new ArgumentNullException("partition");
            }
            if (partition.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "partition");
            }
            this.GetDSHandle();
            zero = base.GetReplicationInfoHelper(this.dsHandle, 0, 0, partition, ref advanced, 0, DirectoryContext.ADHandle);
            return base.ConstructNeighbors(zero, this, DirectoryContext.ADHandle);
        }

        [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public override ReplicationOperationInformation GetReplicationOperationInformation()
        {
            IntPtr zero = IntPtr.Zero;
            bool advanced = true;
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            this.GetDSHandle();
            zero = base.GetReplicationInfoHelper(this.dsHandle, 5, 5, null, ref advanced, 0, DirectoryContext.ADHandle);
            return base.ConstructPendingOperations(zero, this, DirectoryContext.ADHandle);
        }

        private ArrayList GetRoles()
        {
            ArrayList list = new ArrayList();
            int errorCode = 0;
            IntPtr zero = IntPtr.Zero;
            this.GetDSHandle();
            IntPtr procAddress = System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.GetProcAddress(DirectoryContext.ADHandle, "DsListRolesW");
            if (procAddress == IntPtr.Zero)
            {
                throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
            }
            System.DirectoryServices.ActiveDirectory.NativeMethods.DsListRoles delegateForFunctionPointer = (System.DirectoryServices.ActiveDirectory.NativeMethods.DsListRoles) Marshal.GetDelegateForFunctionPointer(procAddress, typeof(System.DirectoryServices.ActiveDirectory.NativeMethods.DsListRoles));
            errorCode = delegateForFunctionPointer(this.dsHandle, out zero);
            if (errorCode == 0)
            {
                try
                {
                    DsNameResult structure = new DsNameResult();
                    Marshal.PtrToStructure(zero, structure);
                    IntPtr items = structure.items;
                    for (int i = 0; i < structure.itemCount; i++)
                    {
                        DsNameResultItem item = new DsNameResultItem();
                        Marshal.PtrToStructure(items, item);
                        if ((item.status == 0) && item.name.Equals(this.NtdsaObjectName))
                        {
                            list.Add((ActiveDirectoryRole) i);
                        }
                        items = (IntPtr) (((long) items) + Marshal.SizeOf(item));
                    }
                    return list;
                }
                finally
                {
                    if (zero != IntPtr.Zero)
                    {
                        procAddress = System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.GetProcAddress(DirectoryContext.ADHandle, "DsFreeNameResultW");
                        if (procAddress == IntPtr.Zero)
                        {
                            throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
                        }
                        System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsFreeNameResultW tw = (System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsFreeNameResultW) Marshal.GetDelegateForFunctionPointer(procAddress, typeof(System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsFreeNameResultW));
                        tw(zero);
                    }
                }
            }
            throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromErrorCode(errorCode, base.Name);
        }

        [DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
        private DirectorySearcher InternalGetDirectorySearcher()
        {
            DirectoryEntry searchRoot = new DirectoryEntry("LDAP://" + base.Name);
            if (DirectoryContext.ServerBindSupported)
            {
                searchRoot.AuthenticationType = Utils.DefaultAuthType | AuthenticationTypes.ServerBind;
            }
            else
            {
                searchRoot.AuthenticationType = Utils.DefaultAuthType;
            }
            searchRoot.Username = base.context.UserName;
            searchRoot.Password = base.context.Password;
            return new DirectorySearcher(searchRoot);
        }

        [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
        public virtual bool IsGlobalCatalog()
        {
            bool flag;
            base.CheckIfDisposed();
            try
            {
                DirectoryEntry cachedDirectoryEntry = base.directoryEntryMgr.GetCachedDirectoryEntry(this.NtdsaObjectName);
                cachedDirectoryEntry.RefreshCache();
                int num = 0;
                if (cachedDirectoryEntry.Properties[PropertyManager.Options].Value != null)
                {
                    num = (int) cachedDirectoryEntry.Properties[PropertyManager.Options].Value;
                }
                if ((num & 1) == 1)
                {
                    return true;
                }
                flag = false;
            }
            catch (COMException exception)
            {
                throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromCOMException(base.context, exception);
            }
            return flag;
        }

        private DateTime ParseDateTime(string dateTime)
        {
            int year = int.Parse(dateTime.Substring(0, 4), NumberFormatInfo.InvariantInfo);
            int month = int.Parse(dateTime.Substring(4, 2), NumberFormatInfo.InvariantInfo);
            int day = int.Parse(dateTime.Substring(6, 2), NumberFormatInfo.InvariantInfo);
            int hour = int.Parse(dateTime.Substring(8, 2), NumberFormatInfo.InvariantInfo);
            int minute = int.Parse(dateTime.Substring(10, 2), NumberFormatInfo.InvariantInfo);
            return new DateTime(year, month, day, hour, minute, int.Parse(dateTime.Substring(12, 2), NumberFormatInfo.InvariantInfo), 0);
        }

        public void SeizeRoleOwnership(ActiveDirectoryRole role)
        {
            string dn = null;
            base.CheckIfDisposed();
            switch (role)
            {
                case ActiveDirectoryRole.SchemaRole:
                    dn = base.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.SchemaNamingContext);
                    break;

                case ActiveDirectoryRole.NamingRole:
                    dn = base.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.PartitionsContainer);
                    break;

                case ActiveDirectoryRole.PdcRole:
                    dn = base.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.DefaultNamingContext);
                    break;

                case ActiveDirectoryRole.RidRole:
                    dn = base.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.RidManager);
                    break;

                case ActiveDirectoryRole.InfrastructureRole:
                    dn = base.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.Infrastructure);
                    break;

                default:
                    throw new InvalidEnumArgumentException("role", (int) role, typeof(ActiveDirectoryRole));
            }
            DirectoryEntry directoryEntry = null;
            try
            {
                directoryEntry = DirectoryEntryManager.GetDirectoryEntry(base.context, dn);
                directoryEntry.Properties[PropertyManager.FsmoRoleOwner].Value = this.NtdsaObjectName;
                directoryEntry.CommitChanges();
            }
            catch (COMException exception)
            {
                throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromCOMException(base.context, exception);
            }
            finally
            {
                if (directoryEntry != null)
                {
                    directoryEntry.Dispose();
                }
            }
            this.cachedRoles = null;
        }

        [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public override void SyncReplicaFromAllServers(string partition, SyncFromAllServersOptions options)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            if (partition == null)
            {
                throw new ArgumentNullException("partition");
            }
            if (partition.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "partition");
            }
            this.GetDSHandle();
            base.SyncReplicaAllHelper(this.dsHandle, this.syncAllFunctionPointer, partition, options, this.SyncFromAllServersCallback, DirectoryContext.ADHandle);
        }

        [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public override void SyncReplicaFromServer(string partition, string sourceServer)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            if (partition == null)
            {
                throw new ArgumentNullException("partition");
            }
            if (partition.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "partition");
            }
            if (sourceServer == null)
            {
                throw new ArgumentNullException("sourceServer");
            }
            if (sourceServer.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "sourceServer");
            }
            this.GetDSHandle();
            base.SyncReplicaHelper(this.dsHandle, false, partition, sourceServer, 0, DirectoryContext.ADHandle);
        }

        public void TransferRoleOwnership(ActiveDirectoryRole role)
        {
            base.CheckIfDisposed();
            if ((role < ActiveDirectoryRole.SchemaRole) || (role > ActiveDirectoryRole.InfrastructureRole))
            {
                throw new InvalidEnumArgumentException("role", (int) role, typeof(ActiveDirectoryRole));
            }
            try
            {
                DirectoryEntry cachedDirectoryEntry = base.directoryEntryMgr.GetCachedDirectoryEntry(WellKnownDN.RootDSE);
                cachedDirectoryEntry.Properties[this.becomeRoleOwnerAttrs[(int) role]].Value = 1;
                cachedDirectoryEntry.CommitChanges();
            }
            catch (COMException exception)
            {
                throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromCOMException(base.context, exception);
            }
            this.cachedRoles = null;
        }

        [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
        public override void TriggerSyncReplicaFromNeighbors(string partition)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            if (partition == null)
            {
                throw new ArgumentNullException("partition");
            }
            if (partition.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "partition");
            }
            this.GetDSHandle();
            base.SyncReplicaHelper(this.dsHandle, false, partition, null, 0x11, DirectoryContext.ADHandle);
        }

        [DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
        internal static void ValidateCredential(DomainController dc, DirectoryContext context)
        {
            DirectoryEntry entry;
            if (DirectoryContext.ServerBindSupported)
            {
                entry = new DirectoryEntry("LDAP://" + dc.Name + "/RootDSE", context.UserName, context.Password, Utils.DefaultAuthType | AuthenticationTypes.ServerBind);
            }
            else
            {
                entry = new DirectoryEntry("LDAP://" + dc.Name + "/RootDSE", context.UserName, context.Password, Utils.DefaultAuthType);
            }
            entry.Bind(true);
        }

        internal string ComputerObjectName
        {
            get
            {
                base.CheckIfDisposed();
                if (!this.dcInfoInitialized)
                {
                    this.GetDomainControllerInfo();
                }
                if (this.cachedComputerObjectName == null)
                {
                    throw new ActiveDirectoryOperationException(Res.GetString("ComputerObjectNameNotFound", new object[] { base.Name }));
                }
                return this.cachedComputerObjectName;
            }
        }

        public DateTime CurrentTime
        {
            get
            {
                base.CheckIfDisposed();
                DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(base.context, WellKnownDN.RootDSE);
                string dateTime = null;
                try
                {
                    dateTime = (string) PropertyManager.GetPropertyValue(base.context, directoryEntry, PropertyManager.CurrentTime);
                }
                finally
                {
                    directoryEntry.Dispose();
                }
                return this.ParseDateTime(dateTime);
            }
        }

        public System.DirectoryServices.ActiveDirectory.Domain Domain
        {
            get
            {
                base.CheckIfDisposed();
                if (this.cachedDomain == null)
                {
                    string domainName = null;
                    try
                    {
                        domainName = Utils.GetDnsNameFromDN(base.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.DefaultNamingContext));
                    }
                    catch (COMException exception)
                    {
                        throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromCOMException(base.context, exception);
                    }
                    DirectoryContext context = Utils.GetNewDirectoryContext(base.Name, DirectoryContextType.DirectoryServer, base.context);
                    this.cachedDomain = new System.DirectoryServices.ActiveDirectory.Domain(context, domainName);
                }
                return this.cachedDomain;
            }
        }

        public System.DirectoryServices.ActiveDirectory.Forest Forest
        {
            get
            {
                base.CheckIfDisposed();
                if (this.currentForest == null)
                {
                    DirectoryContext context = Utils.GetNewDirectoryContext(base.Name, DirectoryContextType.DirectoryServer, base.context);
                    this.currentForest = System.DirectoryServices.ActiveDirectory.Forest.GetForest(context);
                }
                return this.currentForest;
            }
        }

        internal IntPtr Handle
        {
            get
            {
                this.GetDSHandle();
                return this.dsHandle;
            }
        }

        public long HighestCommittedUsn
        {
            get
            {
                base.CheckIfDisposed();
                DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(base.context, WellKnownDN.RootDSE);
                string s = null;
                try
                {
                    s = (string) PropertyManager.GetPropertyValue(base.context, directoryEntry, PropertyManager.HighestCommittedUSN);
                }
                finally
                {
                    directoryEntry.Dispose();
                }
                return long.Parse(s, NumberFormatInfo.InvariantInfo);
            }
        }

        public override ReplicationConnectionCollection InboundConnections
        {
            [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
            get
            {
                return base.GetInboundConnectionsHelper();
            }
        }

        public override string IPAddress
        {
            [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true), DnsPermission(SecurityAction.Assert, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
            get
            {
                base.CheckIfDisposed();
                IPHostEntry hostEntry = Dns.GetHostEntry(base.Name);
                if (hostEntry.AddressList.GetLength(0) > 0)
                {
                    return hostEntry.AddressList[0].ToString();
                }
                return null;
            }
        }

        internal Guid NtdsaObjectGuid
        {
            get
            {
                base.CheckIfDisposed();
                if (!this.dcInfoInitialized || base.siteInfoModified)
                {
                    this.GetDomainControllerInfo();
                }
                if (this.cachedNtdsaObjectGuid.Equals(Guid.Empty))
                {
                    throw new ActiveDirectoryOperationException(Res.GetString("NtdsaObjectGuidNotFound", new object[] { base.Name }));
                }
                return base.cachedNtdsaObjectGuid;
            }
        }

        internal string NtdsaObjectName
        {
            get
            {
                base.CheckIfDisposed();
                if (!this.dcInfoInitialized || base.siteInfoModified)
                {
                    this.GetDomainControllerInfo();
                }
                if (base.cachedNtdsaObjectName == null)
                {
                    throw new ActiveDirectoryOperationException(Res.GetString("NtdsaObjectNameNotFound", new object[] { base.Name }));
                }
                return base.cachedNtdsaObjectName;
            }
        }

        internal double NumericOSVersion
        {
            get
            {
                base.CheckIfDisposed();
                if (this.cachedNumericOSVersion == 0.0)
                {
                    DirectoryEntry cachedDirectoryEntry = base.directoryEntryMgr.GetCachedDirectoryEntry(this.ComputerObjectName);
                    string s = (string) PropertyManager.GetPropertyValue(base.context, cachedDirectoryEntry, PropertyManager.OperatingSystemVersion);
                    int index = s.IndexOf('(');
                    if (index != -1)
                    {
                        s = s.Substring(0, index);
                    }
                    this.cachedNumericOSVersion = double.Parse(s, NumberFormatInfo.InvariantInfo);
                }
                return this.cachedNumericOSVersion;
            }
        }

        public string OSVersion
        {
            get
            {
                base.CheckIfDisposed();
                if (this.cachedOSVersion == null)
                {
                    DirectoryEntry cachedDirectoryEntry = base.directoryEntryMgr.GetCachedDirectoryEntry(this.ComputerObjectName);
                    this.cachedOSVersion = (string) PropertyManager.GetPropertyValue(base.context, cachedDirectoryEntry, PropertyManager.OperatingSystem);
                }
                return this.cachedOSVersion;
            }
        }

        public override ReplicationConnectionCollection OutboundConnections
        {
            [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
            get
            {
                return base.GetOutboundConnectionsHelper();
            }
        }

        public ActiveDirectoryRoleCollection Roles
        {
            get
            {
                base.CheckIfDisposed();
                if (this.cachedRoles == null)
                {
                    this.cachedRoles = new ActiveDirectoryRoleCollection(this.GetRoles());
                }
                return this.cachedRoles;
            }
        }

        internal string ServerObjectName
        {
            get
            {
                base.CheckIfDisposed();
                if (!this.dcInfoInitialized || base.siteInfoModified)
                {
                    this.GetDomainControllerInfo();
                }
                if (base.cachedServerObjectName == null)
                {
                    throw new ActiveDirectoryOperationException(Res.GetString("ServerObjectNameNotFound", new object[] { base.Name }));
                }
                return base.cachedServerObjectName;
            }
        }

        public override string SiteName
        {
            [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
            get
            {
                base.CheckIfDisposed();
                if (!this.dcInfoInitialized || base.siteInfoModified)
                {
                    this.GetDomainControllerInfo();
                }
                if (base.cachedSiteName == null)
                {
                    throw new ActiveDirectoryOperationException(Res.GetString("SiteNameNotFound", new object[] { base.Name }));
                }
                return base.cachedSiteName;
            }
        }

        internal string SiteObjectName
        {
            get
            {
                base.CheckIfDisposed();
                if (!this.dcInfoInitialized || base.siteInfoModified)
                {
                    this.GetDomainControllerInfo();
                }
                if (base.cachedSiteObjectName == null)
                {
                    throw new ActiveDirectoryOperationException(Res.GetString("SiteObjectNameNotFound", new object[] { base.Name }));
                }
                return base.cachedSiteObjectName;
            }
        }

        public override SyncUpdateCallback SyncFromAllServersCallback
        {
            [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                return this.userDelegate;
            }
            [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
            set
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                this.userDelegate = value;
            }
        }
    }
}

