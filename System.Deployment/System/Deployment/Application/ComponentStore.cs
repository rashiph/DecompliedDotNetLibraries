namespace System.Deployment.Application
{
    using Microsoft.Internal.Performance;
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.Deployment.Application.Manifest;
    using System.Deployment.Internal.Isolation;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;

    internal class ComponentStore
    {
        private bool _firstRefresh;
        private static object _installReference;
        private System.Deployment.Internal.Isolation.IStateManager _stateMgr;
        private System.Deployment.Internal.Isolation.Store _store;
        private ComponentStoreType _storeType;
        private SubscriptionStore _subStore;
        private const string DateTimeFormatString = "yyyy/MM/dd HH:mm:ss";

        private ComponentStore(ComponentStoreType storeType, SubscriptionStore subStore)
        {
            if (storeType != ComponentStoreType.UserStore)
            {
                throw new NotImplementedException();
            }
            this._storeType = storeType;
            this._subStore = subStore;
            this._store = System.Deployment.Internal.Isolation.IsolationInterop.GetUserStore();
            Guid guidOfType = System.Deployment.Internal.Isolation.IsolationInterop.GetGuidOfType(typeof(System.Deployment.Internal.Isolation.IStateManager));
            this._stateMgr = System.Deployment.Internal.Isolation.IsolationInterop.GetUserStateManager(0, IntPtr.Zero, ref guidOfType) as System.Deployment.Internal.Isolation.IStateManager;
            this._firstRefresh = true;
        }

        public void ActivateApplication(System.Deployment.Application.DefinitionAppId appId, string activationParameter, bool useActivationParameter)
        {
            HostType hostTypeFromMetadata = this.GetHostTypeFromMetadata(appId);
            uint hostType = 0;
            switch (PolicyKeys.ClrHostType())
            {
                case PolicyKeys.HostType.AppLaunch:
                    hostTypeFromMetadata = HostType.AppLaunch;
                    break;

                case PolicyKeys.HostType.Cor:
                    hostTypeFromMetadata = HostType.CorFlag;
                    break;
            }
            string applicationFullName = appId.ToString();
            AssemblyManifest assemblyManifest = this.GetAssemblyManifest(appId.DeploymentIdentity);
            Logger.AddMethodCall("ComponentStore.ActivateApplication(appId=[" + applicationFullName + "] ,activationParameter=" + activationParameter + ",useActivationParameter=" + useActivationParameter.ToString() + ") called.");
            Logger.AddInternalState("HostType=" + ((uint) hostTypeFromMetadata));
            int activationDataCount = 0;
            string[] activationData = null;
            if (activationParameter != null)
            {
                if (assemblyManifest.Deployment.TrustURLParameters || useActivationParameter)
                {
                    activationDataCount = 1;
                    activationData = new string[] { activationParameter };
                }
                else
                {
                    Logger.AddInternalState("Activation parameters are not passed.");
                }
            }
            hostType = (uint) hostTypeFromMetadata;
            if (!assemblyManifest.Deployment.Install)
            {
                hostType |= 0x80000000;
            }
            try
            {
                Logger.AddInternalState("Activating application via CorLaunchApplication.");
                System.Deployment.Application.NativeMethods.CorLaunchApplication(hostType, applicationFullName, 0, null, activationDataCount, activationData, new System.Deployment.Application.NativeMethods.PROCESS_INFORMATION());
            }
            catch (COMException exception)
            {
                int num3 = exception.ErrorCode & 0xffff;
                if ((num3 >= 0x36b0) && (num3 <= 0x3a97))
                {
                    throw new DeploymentException(ExceptionTypes.Activation, Resources.GetString("Ex_ActivationFailureDueToSxSError"), exception);
                }
                if (exception.ErrorCode == -2147024784)
                {
                    throw new DeploymentException(ExceptionTypes.DiskIsFull, Resources.GetString("Ex_StoreOperationFailed"), exception);
                }
                throw;
            }
            catch (UnauthorizedAccessException exception2)
            {
                throw new DeploymentException(ExceptionTypes.Activation, Resources.GetString("Ex_GenericActivationFailure"), exception2);
            }
            catch (IOException exception3)
            {
                throw new DeploymentException(ExceptionTypes.Activation, Resources.GetString("Ex_GenericActivationFailure"), exception3);
            }
        }

        private int CalculateDeploymentsUnderQuota(int numberOfDeployments, System.Deployment.Internal.Isolation.IDefinitionAppId[] deployAppIdPtrs, ulong quotaSize, ref ulong privateSize, ref ulong sharedSize)
        {
            uint delimiter = 0;
            System.Deployment.Internal.Isolation.StoreApplicationReference installReference = this.InstallReference;
            this._store.CalculateDelimiterOfDeploymentsBasedOnQuota(0, (uint) numberOfDeployments, deployAppIdPtrs, ref installReference, quotaSize, ref delimiter, ref sharedSize, ref privateSize);
            return (int) delimiter;
        }

        public bool CheckGroupInstalled(System.Deployment.Application.DefinitionAppId appId, string groupName)
        {
            AssemblyManifest assemblyManifest = this.GetAssemblyManifest(appId.ApplicationIdentity);
            return this.CheckGroupInstalled(appId, assemblyManifest, groupName);
        }

        public bool CheckGroupInstalled(System.Deployment.Application.DefinitionAppId appId, AssemblyManifest appManifest, string groupName)
        {
            System.Deployment.Internal.Isolation.Store.IPathLock @lock = null;
            using (@lock = this.LockApplicationPath(appId))
            {
                string path = @lock.Path;
                System.Deployment.Application.Manifest.File[] filesInGroup = appManifest.GetFilesInGroup(groupName, true);
                foreach (System.Deployment.Application.Manifest.File file in filesInGroup)
                {
                    if (!System.IO.File.Exists(Path.Combine(path, file.NameFS)))
                    {
                        return false;
                    }
                }
                DependentAssembly[] privateAssembliesInGroup = appManifest.GetPrivateAssembliesInGroup(groupName, true);
                foreach (DependentAssembly assembly in privateAssembliesInGroup)
                {
                    if (!System.IO.File.Exists(Path.Combine(path, assembly.CodebaseFS)))
                    {
                        return false;
                    }
                }
                if ((filesInGroup.Length + privateAssembliesInGroup.Length) == 0)
                {
                    throw new InvalidDeploymentException(string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_NoSuchDownloadGroup"), new object[] { groupName }));
                }
            }
            return true;
        }

        public void CleanOnlineAppCache()
        {
            using (StoreTransactionContext context = new StoreTransactionContext(this))
            {
                context.ScavengeContext.CleanOnlineAppCache();
            }
        }

        internal ArrayList CollectCrossGroupApplications(Uri codebaseUri, System.Deployment.Application.DefinitionIdentity deploymentIdentity, ref bool identityGroupFound, ref bool locationGroupFound, ref string identityGroupProductName)
        {
            Hashtable hashtable = new Hashtable();
            ArrayList list = new ArrayList();
            foreach (System.Deployment.Internal.Isolation.STORE_ASSEMBLY store_assembly in this._store.EnumAssemblies(System.Deployment.Internal.Isolation.Store.EnumAssembliesFlags.Nothing))
            {
                System.Deployment.Application.DefinitionIdentity subId = new System.Deployment.Application.DefinitionIdentity(store_assembly.DefinitionIdentity).ToSubscriptionId();
                SubscriptionState subscriptionState = this._subStore.GetSubscriptionState(subId);
                if (subscriptionState.IsInstalled)
                {
                    bool flag = subscriptionState.DeploymentProviderUri.Equals(codebaseUri);
                    bool flag2 = subscriptionState.PKTGroupId.Equals(deploymentIdentity.ToPKTGroupId());
                    bool flag3 = subscriptionState.SubscriptionId.PublicKeyToken.Equals(deploymentIdentity.ToSubscriptionId().PublicKeyToken);
                    if ((!flag || !flag2) || !flag3)
                    {
                        if ((flag && flag2) && !flag3)
                        {
                            if (!hashtable.Contains(subId))
                            {
                                hashtable.Add(subId, subscriptionState);
                                list.Add(new CrossGroupApplicationData(subscriptionState, CrossGroupApplicationData.GroupType.LocationGroup));
                                locationGroupFound = true;
                            }
                        }
                        else if ((!flag && flag2) && (flag3 && !hashtable.Contains(subId)))
                        {
                            hashtable.Add(subId, subscriptionState);
                            list.Add(new CrossGroupApplicationData(subscriptionState, CrossGroupApplicationData.GroupType.IdentityGroup));
                            if (((subscriptionState.CurrentDeploymentManifest != null) && (subscriptionState.CurrentDeploymentManifest.Description != null)) && (subscriptionState.CurrentDeploymentManifest.Description.Product != null))
                            {
                                identityGroupProductName = subscriptionState.CurrentDeploymentManifest.Description.Product;
                            }
                            identityGroupFound = true;
                        }
                    }
                }
            }
            return list;
        }

        public void CommitApplication(SubscriptionState subState, CommitApplicationParams commitParams)
        {
            try
            {
                using (StoreTransactionContext context = new StoreTransactionContext(this))
                {
                    this.PrepareCommitApplication(context, subState, commitParams);
                    this.SubmitStoreTransactionCheckQuota(context, subState);
                }
            }
            catch (COMException exception)
            {
                if (exception.ErrorCode == -2147024784)
                {
                    throw new DeploymentException(ExceptionTypes.DiskIsFull, Resources.GetString("Ex_StoreOperationFailed"), exception);
                }
                if (exception.ErrorCode == -2147023590)
                {
                    throw new DeploymentException(ExceptionTypes.ComponentStore, Resources.GetString("Ex_InplaceUpdateOfApplicationAttempted"), exception);
                }
                throw;
            }
        }

        private static System.Deployment.Internal.Isolation.IDefinitionAppId[] DeployAppIdsToComPtrs(ArrayList deployAppIdList)
        {
            System.Deployment.Internal.Isolation.IDefinitionAppId[] idArray = new System.Deployment.Internal.Isolation.IDefinitionAppId[deployAppIdList.Count];
            for (int i = 0; i < deployAppIdList.Count; i++)
            {
                idArray[i] = ((System.Deployment.Application.DefinitionAppId) deployAppIdList[i]).ComPointer;
            }
            return idArray;
        }

        private static void FinalizeSubscriptionState(SubscriptionStateInternal newState)
        {
            if (!newState.IsInstalled)
            {
                newState.Reset();
            }
            else
            {
                System.Deployment.Application.DefinitionAppId currentBind = newState.CurrentBind;
                System.Deployment.Application.DefinitionIdentity deploymentIdentity = currentBind.DeploymentIdentity;
                System.Deployment.Application.DefinitionAppId previousBind = newState.PreviousBind;
                if ((previousBind != null) && previousBind.Equals(currentBind))
                {
                    newState.PreviousBind = (System.Deployment.Application.DefinitionAppId) (previousBind = null);
                }
                System.Deployment.Application.DefinitionIdentity identity2 = (previousBind != null) ? previousBind.DeploymentIdentity : null;
                System.Deployment.Application.DefinitionIdentity excludedDeployment = newState.ExcludedDeployment;
                if ((excludedDeployment != null) && (excludedDeployment.Equals(deploymentIdentity) || excludedDeployment.Equals(identity2)))
                {
                    newState.ExcludedDeployment = (System.Deployment.Application.DefinitionIdentity) (excludedDeployment = null);
                }
                System.Deployment.Application.DefinitionIdentity pendingDeployment = newState.PendingDeployment;
                if ((pendingDeployment != null) && (pendingDeployment.Equals(deploymentIdentity) || pendingDeployment.Equals(excludedDeployment)))
                {
                    newState.PendingDeployment = (System.Deployment.Application.DefinitionIdentity) (pendingDeployment = null);
                }
                System.Deployment.Application.DefinitionAppId pendingBind = newState.PendingBind;
                if ((pendingBind != null) && (!pendingBind.DeploymentIdentity.Equals(pendingDeployment) || pendingBind.Equals(previousBind)))
                {
                    newState.PendingBind = (System.Deployment.Application.DefinitionAppId) (pendingBind = null);
                }
            }
        }

        private AssemblyManifest GetAssemblyManifest(System.Deployment.Application.DefinitionIdentity asmId)
        {
            return new AssemblyManifest(this._store.GetAssemblyManifest(0, asmId.ComPointer));
        }

        private HostType GetHostTypeFromMetadata(System.Deployment.Application.DefinitionAppId defAppId)
        {
            HostType appLaunch = HostType.Default;
            try
            {
                if (this.GetPropertyBoolean(defAppId, "IsFullTrust"))
                {
                    return HostType.CorFlag;
                }
                appLaunch = HostType.AppLaunch;
            }
            catch (DeploymentException)
            {
            }
            return appLaunch;
        }

        internal ulong GetOnlineAppQuotaInBytes()
        {
            uint num = 0x3e800;
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Classes\Software\Microsoft\Windows\CurrentVersion\Deployment"))
            {
                if (key != null)
                {
                    object obj2 = key.GetValue("OnlineAppQuotaInKB");
                    if (obj2 is int)
                    {
                        int num2 = (int) obj2;
                        num = (num2 >= 0) ? ((uint) num2) : ((uint) ((-1 - -num2) + 1));
                    }
                }
            }
            return (ulong) (num * 0x400L);
        }

        private void GetPrivateAndSharedSizes(ArrayList deployAppIds, out ulong privateSize, out ulong sharedSize)
        {
            privateSize = 0L;
            sharedSize = 0L;
            if ((deployAppIds != null) && (deployAppIds.Count > 0))
            {
                System.Deployment.Internal.Isolation.IDefinitionAppId[] deployAppIdPtrs = DeployAppIdsToComPtrs(deployAppIds);
                this.CalculateDeploymentsUnderQuota(deployAppIdPtrs.Length, deployAppIdPtrs, ulong.MaxValue, ref privateSize, ref sharedSize);
            }
        }

        internal ulong GetPrivateSize(ArrayList deployAppIds)
        {
            ulong num;
            ulong num2;
            this.GetPrivateAndSharedSizes(deployAppIds, out num, out num2);
            return num;
        }

        private AppType GetPropertyAppType(System.Deployment.Application.DefinitionAppId appId, string propName)
        {
            string propertyString = null;
            AppType none;
            try
            {
                propertyString = this.GetPropertyString(appId, propName);
                if (propertyString == null)
                {
                    return AppType.None;
                }
                switch (Convert.ToUInt16(propertyString, CultureInfo.InvariantCulture))
                {
                    case 0:
                        return AppType.None;

                    case 1:
                        return AppType.Installed;

                    case 2:
                        return AppType.Online;

                    case 3:
                        return AppType.CustomHostSpecified;

                    case 4:
                        return AppType.CustomUX;
                }
                none = AppType.None;
            }
            catch (DeploymentException)
            {
                none = AppType.None;
            }
            catch (FormatException exception)
            {
                Logger.AddInternalState("Unable to convert store property," + propName + ", from string to UInt16." + propName + "=" + propertyString);
                throw new DeploymentException(ExceptionTypes.SubscriptionState, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_InvalidStoreMetaData"), new object[] { propName }), exception);
            }
            catch (OverflowException exception2)
            {
                Logger.AddInternalState("Unable to convert store property," + propName + ", from string to UInt16." + propName + "=" + propertyString);
                throw new DeploymentException(ExceptionTypes.SubscriptionState, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_InvalidStoreMetaData"), new object[] { propName }), exception2);
            }
            return none;
        }

        private bool GetPropertyBoolean(System.Deployment.Application.DefinitionAppId appId, string propName)
        {
            bool flag;
            try
            {
                string propertyString = this.GetPropertyString(appId, propName);
                flag = ((propertyString != null) && (propertyString.Length > 0)) ? Convert.ToBoolean(propertyString, CultureInfo.InvariantCulture) : false;
            }
            catch (FormatException exception)
            {
                throw new DeploymentException(ExceptionTypes.SubscriptionState, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_InvalidStoreMetaData"), new object[] { propName }), exception);
            }
            return flag;
        }

        private DateTime GetPropertyDateTime(System.Deployment.Application.DefinitionAppId appId, string propName)
        {
            DateTime time;
            try
            {
                string propertyString = this.GetPropertyString(appId, propName);
                time = ((propertyString != null) && (propertyString.Length > 0)) ? DateTime.ParseExact(propertyString, "yyyy/MM/dd HH:mm:ss", DateTimeFormatInfo.InvariantInfo) : DateTime.MinValue;
            }
            catch (FormatException exception)
            {
                throw new DeploymentException(ExceptionTypes.SubscriptionState, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_InvalidStoreMetaData"), new object[] { propName }), exception);
            }
            return time;
        }

        private System.Deployment.Application.DefinitionAppId GetPropertyDefinitionAppId(System.Deployment.Application.DefinitionAppId appId, string propName)
        {
            System.Deployment.Application.DefinitionAppId id;
            try
            {
                string propertyString = this.GetPropertyString(appId, propName);
                id = ((propertyString != null) && (propertyString.Length > 0)) ? new System.Deployment.Application.DefinitionAppId(propertyString) : null;
            }
            catch (COMException exception)
            {
                throw new DeploymentException(ExceptionTypes.SubscriptionState, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_InvalidStoreMetaData"), new object[] { propName }), exception);
            }
            catch (SEHException exception2)
            {
                throw new DeploymentException(ExceptionTypes.SubscriptionState, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_InvalidStoreMetaData"), new object[] { propName }), exception2);
            }
            return id;
        }

        private System.Deployment.Application.DefinitionIdentity GetPropertyDefinitionIdentity(System.Deployment.Application.DefinitionAppId appId, string propName)
        {
            System.Deployment.Application.DefinitionIdentity identity;
            try
            {
                string propertyString = this.GetPropertyString(appId, propName);
                identity = ((propertyString != null) && (propertyString.Length > 0)) ? new System.Deployment.Application.DefinitionIdentity(propertyString) : null;
            }
            catch (COMException exception)
            {
                throw new DeploymentException(ExceptionTypes.SubscriptionState, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_InvalidStoreMetaData"), new object[] { propName }), exception);
            }
            catch (SEHException exception2)
            {
                throw new DeploymentException(ExceptionTypes.SubscriptionState, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_InvalidStoreMetaData"), new object[] { propName }), exception2);
            }
            return identity;
        }

        private string GetPropertyString(System.Deployment.Application.DefinitionAppId appId, string propName)
        {
            byte[] buffer;
            try
            {
                buffer = this._store.GetDeploymentProperty(System.Deployment.Internal.Isolation.Store.GetPackagePropertyFlags.Nothing, appId.ComPointer, this.InstallReference, Constants.DeploymentPropertySet, propName);
            }
            catch (COMException)
            {
                return null;
            }
            int length = buffer.Length;
            if (((length == 0) || ((buffer.Length % 2) != 0)) || ((buffer[length - 2] != 0) || (buffer[length - 1] != 0)))
            {
                throw new DeploymentException(ExceptionTypes.SubscriptionState, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_InvalidStoreMetaData"), new object[] { propName }));
            }
            return Encoding.Unicode.GetString(buffer, 0, length - 2);
        }

        private Uri GetPropertyUri(System.Deployment.Application.DefinitionAppId appId, string propName)
        {
            Uri uri;
            try
            {
                string propertyString = this.GetPropertyString(appId, propName);
                uri = ((propertyString != null) && (propertyString.Length > 0)) ? new Uri(propertyString) : null;
            }
            catch (UriFormatException exception)
            {
                throw new DeploymentException(ExceptionTypes.SubscriptionState, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_InvalidStoreMetaData"), new object[] { propName }), exception);
            }
            return uri;
        }

        private Version GetPropertyVersion(System.Deployment.Application.DefinitionAppId appId, string propName)
        {
            Version version;
            try
            {
                string propertyString = this.GetPropertyString(appId, propName);
                version = ((propertyString != null) && (propertyString.Length > 0)) ? new Version(propertyString) : null;
            }
            catch (ArgumentException exception)
            {
                throw new DeploymentException(ExceptionTypes.SubscriptionState, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_InvalidStoreMetaData"), new object[] { propName }), exception);
            }
            catch (FormatException exception2)
            {
                throw new DeploymentException(ExceptionTypes.SubscriptionState, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_InvalidStoreMetaData"), new object[] { propName }), exception2);
            }
            return version;
        }

        internal ulong GetSharedSize(ArrayList deployAppIds)
        {
            ulong num;
            ulong num2;
            this.GetPrivateAndSharedSizes(deployAppIds, out num, out num2);
            return num2;
        }

        public static ComponentStore GetStore(ComponentStoreType storeType, SubscriptionStore subStore)
        {
            return new ComponentStore(storeType, subStore);
        }

        public SubscriptionStateInternal GetSubscriptionStateInternal(System.Deployment.Application.DefinitionIdentity subId)
        {
            SubscriptionStateInternal internal2 = new SubscriptionStateInternal {
                IsInstalled = this.IsSubscriptionInstalled(subId)
            };
            if (internal2.IsInstalled)
            {
                System.Deployment.Application.DefinitionAppId appId = new System.Deployment.Application.DefinitionAppId(new System.Deployment.Application.DefinitionIdentity[] { subId });
                internal2.IsShellVisible = this.GetPropertyBoolean(appId, "IsShellVisible");
                internal2.CurrentBind = this.GetPropertyDefinitionAppId(appId, "CurrentBind");
                internal2.PreviousBind = this.GetPropertyDefinitionAppId(appId, "PreviousBind");
                internal2.PendingBind = this.GetPropertyDefinitionAppId(appId, "PendingBind");
                internal2.ExcludedDeployment = this.GetPropertyDefinitionIdentity(appId, "ExcludedDeployment");
                internal2.PendingDeployment = this.GetPropertyDefinitionIdentity(appId, "PendingDeployment");
                internal2.DeploymentProviderUri = this.GetPropertyUri(appId, "DeploymentProviderUri");
                internal2.MinimumRequiredVersion = this.GetPropertyVersion(appId, "MinimumRequiredVersion");
                internal2.LastCheckTime = this.GetPropertyDateTime(appId, "LastCheckTime");
                internal2.UpdateSkippedDeployment = this.GetPropertyDefinitionIdentity(appId, "UpdateSkippedDeployment");
                internal2.UpdateSkipTime = this.GetPropertyDateTime(appId, "UpdateSkipTime");
                internal2.appType = this.GetPropertyAppType(appId, "AppType");
                if (internal2.CurrentBind == null)
                {
                    throw new InvalidDeploymentException(Resources.GetString("Ex_NoCurrentBind"));
                }
                internal2.CurrentDeployment = internal2.CurrentBind.DeploymentIdentity;
                internal2.CurrentDeploymentManifest = this.GetAssemblyManifest(internal2.CurrentDeployment);
                internal2.CurrentDeploymentSourceUri = this.GetPropertyUri(internal2.CurrentBind, "DeploymentSourceUri");
                internal2.CurrentApplication = internal2.CurrentBind.ApplicationIdentity;
                internal2.CurrentApplicationManifest = this.GetAssemblyManifest(internal2.CurrentBind.ApplicationIdentity);
                internal2.CurrentApplicationSourceUri = this.GetPropertyUri(internal2.CurrentBind, "ApplicationSourceUri");
                System.Deployment.Application.DefinitionIdentity identity = (internal2.PreviousBind != null) ? internal2.PreviousBind.DeploymentIdentity : null;
                internal2.RollbackDeployment = ((identity != null) && ((internal2.MinimumRequiredVersion == null) || (identity.Version >= internal2.MinimumRequiredVersion))) ? identity : null;
                if (internal2.PreviousBind == null)
                {
                    return internal2;
                }
                try
                {
                    internal2.PreviousApplication = internal2.PreviousBind.ApplicationIdentity;
                    internal2.PreviousApplicationManifest = this.GetAssemblyManifest(internal2.PreviousBind.ApplicationIdentity);
                }
                catch (Exception exception)
                {
                    if (ExceptionUtility.IsHardException(exception))
                    {
                        throw;
                    }
                    Logger.AddInternalState("Exception thrown for GetAssemblyManifest in GetSubscriptionStateInternal: " + exception.GetType().ToString() + ":" + exception.Message);
                    internal2.PreviousBind = null;
                    internal2.RollbackDeployment = null;
                    internal2.PreviousApplication = null;
                    internal2.PreviousApplicationManifest = null;
                }
            }
            return internal2;
        }

        public SubscriptionStateInternal GetSubscriptionStateInternal(SubscriptionState subState)
        {
            return this.GetSubscriptionStateInternal(subState.SubscriptionId);
        }

        public bool IsAssemblyInstalled(System.Deployment.Application.DefinitionIdentity asmId)
        {
            System.Deployment.Internal.Isolation.IDefinitionIdentity o = null;
            bool flag;
            try
            {
                o = this._store.GetAssemblyIdentity(0, asmId.ComPointer);
                flag = true;
            }
            catch (COMException)
            {
                flag = false;
            }
            finally
            {
                if (o != null)
                {
                    Marshal.ReleaseComObject(o);
                }
            }
            return flag;
        }

        private bool IsSubscriptionInstalled(System.Deployment.Application.DefinitionIdentity subId)
        {
            System.Deployment.Application.DefinitionAppId appId = new System.Deployment.Application.DefinitionAppId(new System.Deployment.Application.DefinitionIdentity[] { subId });
            try
            {
                return (this.GetPropertyDefinitionAppId(appId, "CurrentBind") != null);
            }
            catch (DeploymentException)
            {
                return false;
            }
        }

        public System.Deployment.Internal.Isolation.Store.IPathLock LockApplicationPath(System.Deployment.Application.DefinitionAppId definitionAppId)
        {
            System.Deployment.Internal.Isolation.Store.IPathLock @lock;
            try
            {
                @lock = this._store.LockApplicationPath(definitionAppId.ComPointer);
            }
            catch (COMException exception)
            {
                if (exception.ErrorCode == -2147024784)
                {
                    throw new DeploymentException(ExceptionTypes.DiskIsFull, Resources.GetString("Ex_StoreOperationFailed"), exception);
                }
                throw;
            }
            return @lock;
        }

        public System.Deployment.Internal.Isolation.Store.IPathLock LockAssemblyPath(System.Deployment.Application.DefinitionIdentity asmId)
        {
            System.Deployment.Internal.Isolation.Store.IPathLock @lock;
            try
            {
                @lock = this._store.LockAssemblyPath(asmId.ComPointer);
            }
            catch (COMException exception)
            {
                if (exception.ErrorCode == -2147024784)
                {
                    throw new DeploymentException(ExceptionTypes.DiskIsFull, Resources.GetString("Ex_StoreOperationFailed"), exception);
                }
                throw;
            }
            return @lock;
        }

        private void PrepareCommitApplication(StoreTransactionContext storeTxn, SubscriptionState subState, CommitApplicationParams commitParams)
        {
            System.Deployment.Application.DefinitionAppId appId = commitParams.AppId;
            SubscriptionStateInternal newState = null;
            if (commitParams.CommitDeploy)
            {
                newState = this.PrepareCommitDeploymentState(storeTxn, subState, commitParams);
                if ((commitParams.IsConfirmed && appId.Equals(newState.CurrentBind)) || (!commitParams.IsConfirmed && appId.Equals(newState.PendingBind)))
                {
                    this.PrepareStageDeploymentComponent(storeTxn, subState, commitParams);
                }
            }
            if (commitParams.CommitApp)
            {
                this.PrepareStageAppComponent(storeTxn, commitParams);
                if (!commitParams.DeployManifest.Deployment.Install && (commitParams.appType != AppType.CustomHostSpecified))
                {
                    storeTxn.ScavengeContext.AddOnlineAppToCommit(appId, subState);
                }
            }
            if (commitParams.CommitDeploy)
            {
                this.PrepareSetSubscriptionState(storeTxn, subState, newState);
            }
        }

        private SubscriptionStateInternal PrepareCommitDeploymentState(StoreTransactionContext storeTxn, SubscriptionState subState, CommitApplicationParams commitParams)
        {
            System.Deployment.Application.DefinitionAppId appId = commitParams.AppId;
            AssemblyManifest deployManifest = commitParams.DeployManifest;
            SubscriptionStateInternal newState = new SubscriptionStateInternal(subState);
            if (commitParams.IsConfirmed)
            {
                newState.IsInstalled = true;
                newState.IsShellVisible = deployManifest.Deployment.Install;
                newState.DeploymentProviderUri = (deployManifest.Deployment.ProviderCodebaseUri != null) ? deployManifest.Deployment.ProviderCodebaseUri : commitParams.DeploySourceUri;
                if (deployManifest.Deployment.MinimumRequiredVersion != null)
                {
                    newState.MinimumRequiredVersion = deployManifest.Deployment.MinimumRequiredVersion;
                }
                if (!appId.Equals(subState.CurrentBind))
                {
                    newState.CurrentBind = appId;
                    newState.PreviousBind = (newState.IsShellVisible && !subState.IsShellVisible) ? null : subState.CurrentBind;
                }
                newState.PendingBind = null;
                newState.PendingDeployment = null;
                newState.ExcludedDeployment = null;
                newState.appType = commitParams.appType;
                ResetUpdateSkippedState(newState);
            }
            else
            {
                newState.PendingBind = appId;
                newState.PendingDeployment = appId.DeploymentIdentity;
                if (!newState.PendingDeployment.Equals(subState.UpdateSkippedDeployment))
                {
                    ResetUpdateSkippedState(newState);
                }
            }
            newState.LastCheckTime = commitParams.TimeStamp;
            FinalizeSubscriptionState(newState);
            return newState;
        }

        private void PrepareFinalizeSubscription(StoreTransactionContext storeTxn, SubscriptionState subState, SubscriptionStateInternal newState)
        {
            if (newState.IsInstalled && ((!subState.IsInstalled || (newState.IsShellVisible != subState.IsShellVisible)) || !newState.CurrentBind.Equals(subState.CurrentBind)))
            {
                System.Deployment.Application.DefinitionAppId deployAppId = newState.CurrentBind.ToDeploymentAppId();
                if (newState.IsShellVisible)
                {
                    this.PrepareInstallUninstallDeployment(storeTxn, deployAppId, true);
                }
                else
                {
                    this.PreparePinUnpinDeployment(storeTxn, deployAppId, true);
                }
            }
        }

        private void PrepareFinalizeSubscriptionState(StoreTransactionContext storeTxn, SubscriptionState subState, SubscriptionStateInternal newState)
        {
            FinalizeSubscriptionState(newState);
            this.PrepareSetSubscriptionState(storeTxn, subState, newState);
        }

        private void PrepareInstallFile(StoreTransactionContext storeTxn, System.Deployment.Application.Manifest.File file, System.Deployment.Application.DefinitionAppId appId, System.Deployment.Application.DefinitionIdentity asmId, string asmPayloadPath)
        {
            string srcFile = Path.Combine(asmPayloadPath, file.NameFS);
            string name = file.Name;
            storeTxn.Add(new System.Deployment.Internal.Isolation.StoreOperationStageComponentFile(appId.ComPointer, (asmId != null) ? asmId.ComPointer : null, name, srcFile));
        }

        private void PrepareInstallPrivateAssembly(StoreTransactionContext storeTxn, DependentAssembly privAsm, System.Deployment.Application.DefinitionAppId appId, string appPayloadPath)
        {
            string codebaseFS = privAsm.CodebaseFS;
            string path = Path.Combine(appPayloadPath, codebaseFS);
            string directoryName = Path.GetDirectoryName(path);
            AssemblyManifest manifest = new AssemblyManifest(path);
            System.Deployment.Application.DefinitionIdentity asmId = manifest.Identity;
            string rawXmlFilePath = manifest.RawXmlFilePath;
            if (rawXmlFilePath == null)
            {
                rawXmlFilePath = path + ".genman";
                asmId = ManifestGenerator.GenerateManifest(privAsm.Identity, manifest, rawXmlFilePath);
            }
            storeTxn.Add(new System.Deployment.Internal.Isolation.StoreOperationStageComponent(appId.ComPointer, asmId.ComPointer, rawXmlFilePath));
            foreach (System.Deployment.Application.Manifest.File file in manifest.Files)
            {
                this.PrepareInstallFile(storeTxn, file, appId, asmId, directoryName);
            }
        }

        private void PrepareInstallUninstallDeployment(StoreTransactionContext storeTxn, System.Deployment.Application.DefinitionAppId deployAppId, bool isInstall)
        {
            if (isInstall)
            {
                storeTxn.Add(new System.Deployment.Internal.Isolation.StoreOperationInstallDeployment(deployAppId.ComPointer, this.InstallReference));
            }
            else
            {
                storeTxn.Add(new System.Deployment.Internal.Isolation.StoreOperationUninstallDeployment(deployAppId.ComPointer, this.InstallReference));
            }
        }

        private void PreparePinUnpinDeployment(StoreTransactionContext storeTxn, System.Deployment.Application.DefinitionAppId deployAppId, bool isPin)
        {
            if (isPin)
            {
                storeTxn.Add(new System.Deployment.Internal.Isolation.StoreOperationPinDeployment(deployAppId.ComPointer, this.InstallReference));
            }
            else
            {
                storeTxn.Add(new System.Deployment.Internal.Isolation.StoreOperationUnpinDeployment(deployAppId.ComPointer, this.InstallReference));
            }
        }

        private void PrepareRemoveDeployment(StoreTransactionContext storeTxn, SubscriptionState subState, System.Deployment.Application.DefinitionAppId appId)
        {
            System.Deployment.Application.DefinitionAppId deployAppId = appId.ToDeploymentAppId();
            if (subState.IsShellVisible)
            {
                this.PrepareInstallUninstallDeployment(storeTxn, deployAppId, false);
            }
            else
            {
                this.PreparePinUnpinDeployment(storeTxn, deployAppId, false);
            }
            this.PrepareSetDeploymentProperties(storeTxn, appId, null);
            storeTxn.ScavengeContext.AddDeploymentToUnpin(deployAppId, subState);
            ApplicationTrust.RemoveCachedTrust(appId);
        }

        private void PrepareRemoveOrphanedDeployments(StoreTransactionContext storeTxn, SubscriptionState subState, SubscriptionStateInternal newState)
        {
            ArrayList list = new ArrayList();
            list.Add(subState.CurrentBind);
            list.Add(subState.PreviousBind);
            list.Add(subState.PendingBind);
            list.Remove(newState.CurrentBind);
            list.Remove(newState.PreviousBind);
            list.Remove(newState.PendingBind);
            foreach (System.Deployment.Application.DefinitionAppId id in list)
            {
                if (id != null)
                {
                    this.PrepareRemoveDeployment(storeTxn, subState, id);
                }
            }
        }

        private void PrepareRemoveSubscription(StoreTransactionContext storeTxn, SubscriptionState subState)
        {
            SubscriptionStateInternal newState = new SubscriptionStateInternal(subState) {
                IsInstalled = false
            };
            this.PrepareFinalizeSubscriptionState(storeTxn, subState, newState);
        }

        private void PrepareRollbackSubscription(StoreTransactionContext storeTxn, SubscriptionState subState)
        {
            SubscriptionStateInternal newState = new SubscriptionStateInternal(subState) {
                ExcludedDeployment = subState.CurrentBind.DeploymentIdentity,
                CurrentBind = subState.PreviousBind,
                PreviousBind = null
            };
            this.PrepareFinalizeSubscriptionState(storeTxn, subState, newState);
        }

        private void PrepareSetDeploymentProperties(StoreTransactionContext storeTxn, System.Deployment.Application.DefinitionAppId appId, CommitApplicationParams commitParams)
        {
            string str = null;
            string str2 = null;
            string str3 = null;
            if (commitParams != null)
            {
                str = ToPropertyString(commitParams.DeploySourceUri);
                str2 = ToPropertyString(commitParams.AppSourceUri);
                if ((commitParams.IsUpdateInPKTGroup && (commitParams.Trust == null)) && commitParams.IsFullTrustRequested)
                {
                    str3 = ToPropertyString(commitParams.IsFullTrustRequested);
                }
                else if (commitParams.IsUpdate && (commitParams.Trust == null))
                {
                    str3 = null;
                }
                else if (commitParams.appType == AppType.CustomHostSpecified)
                {
                    str3 = null;
                }
                else
                {
                    str3 = ToPropertyString(commitParams.Trust.DefaultGrantSet.PermissionSet.IsUnrestricted());
                }
            }
            System.Deployment.Internal.Isolation.StoreOperationMetadataProperty[] setProperties = new System.Deployment.Internal.Isolation.StoreOperationMetadataProperty[] { new System.Deployment.Internal.Isolation.StoreOperationMetadataProperty(Constants.DeploymentPropertySet, "DeploymentSourceUri", str), new System.Deployment.Internal.Isolation.StoreOperationMetadataProperty(Constants.DeploymentPropertySet, "ApplicationSourceUri", str2), new System.Deployment.Internal.Isolation.StoreOperationMetadataProperty(Constants.DeploymentPropertySet, "IsFullTrust", str3) };
            storeTxn.Add(new System.Deployment.Internal.Isolation.StoreOperationSetDeploymentMetadata(appId.ComPointer, this.InstallReference, setProperties));
        }

        private void PrepareSetPendingDeployment(StoreTransactionContext storeTxn, SubscriptionState subState, System.Deployment.Application.DefinitionIdentity deployId, DateTime checkTime)
        {
            SubscriptionStateInternal newState = new SubscriptionStateInternal(subState) {
                PendingDeployment = deployId,
                LastCheckTime = checkTime
            };
            if ((newState.PendingDeployment != null) && !newState.PendingDeployment.Equals(subState.UpdateSkippedDeployment))
            {
                ResetUpdateSkippedState(newState);
            }
            this.PrepareFinalizeSubscriptionState(storeTxn, subState, newState);
        }

        private void PrepareSetSubscriptionProperties(StoreTransactionContext storeTxn, SubscriptionState subState, SubscriptionStateInternal newState)
        {
            Logger.AddInternalState("Changing Subscription Properties:");
            Logger.AddInternalState("Old subscription state = " + subState.ToString());
            Logger.AddInternalState("New subscription state = " + newState.ToString());
            SubscriptionStateVariable[] variableArray = new SubscriptionStateVariable[] { new SubscriptionStateVariable("IsShellVisible", newState.IsShellVisible, subState.IsShellVisible), new SubscriptionStateVariable("PreviousBind", newState.PreviousBind, subState.PreviousBind), new SubscriptionStateVariable("PendingBind", newState.PendingBind, subState.PendingBind), new SubscriptionStateVariable("ExcludedDeployment", newState.ExcludedDeployment, subState.ExcludedDeployment), new SubscriptionStateVariable("PendingDeployment", newState.PendingDeployment, subState.PendingDeployment), new SubscriptionStateVariable("DeploymentProviderUri", newState.DeploymentProviderUri, subState.DeploymentProviderUri), new SubscriptionStateVariable("MinimumRequiredVersion", newState.MinimumRequiredVersion, subState.MinimumRequiredVersion), new SubscriptionStateVariable("LastCheckTime", newState.LastCheckTime, subState.LastCheckTime), new SubscriptionStateVariable("UpdateSkippedDeployment", newState.UpdateSkippedDeployment, subState.UpdateSkippedDeployment), new SubscriptionStateVariable("UpdateSkipTime", newState.UpdateSkipTime, subState.UpdateSkipTime), new SubscriptionStateVariable("AppType", (ushort) newState.appType, (ushort) subState.appType), new SubscriptionStateVariable("CurrentBind", newState.CurrentBind, subState.CurrentBind) };
            ArrayList list = new ArrayList();
            foreach (SubscriptionStateVariable variable in variableArray)
            {
                if ((!subState.IsInstalled || !variable.IsUnchanged) || !newState.IsInstalled)
                {
                    list.Add(new System.Deployment.Internal.Isolation.StoreOperationMetadataProperty(Constants.DeploymentPropertySet, variable.PropertyName, newState.IsInstalled ? ToPropertyString(variable.NewValue) : null));
                }
            }
            if (list.Count > 0)
            {
                System.Deployment.Internal.Isolation.StoreOperationMetadataProperty[] setProperties = (System.Deployment.Internal.Isolation.StoreOperationMetadataProperty[]) list.ToArray(typeof(System.Deployment.Internal.Isolation.StoreOperationMetadataProperty));
                System.Deployment.Application.DefinitionAppId id = new System.Deployment.Application.DefinitionAppId(new System.Deployment.Application.DefinitionIdentity[] { subState.SubscriptionId });
                storeTxn.Add(new System.Deployment.Internal.Isolation.StoreOperationSetDeploymentMetadata(id.ComPointer, this.InstallReference, setProperties));
            }
        }

        private void PrepareSetSubscriptionState(StoreTransactionContext storeTxn, SubscriptionState subState, SubscriptionStateInternal newState)
        {
            this.PrepareFinalizeSubscription(storeTxn, subState, newState);
            this.PrepareSetSubscriptionProperties(storeTxn, subState, newState);
            this.PrepareRemoveOrphanedDeployments(storeTxn, subState, newState);
        }

        private void PrepareStageAppComponent(StoreTransactionContext storeTxn, CommitApplicationParams commitParams)
        {
            System.Deployment.Application.DefinitionAppId appId = commitParams.AppId;
            AssemblyManifest appManifest = commitParams.AppManifest;
            string appManifestPath = commitParams.AppManifestPath;
            string appPayloadPath = commitParams.AppPayloadPath;
            string appGroup = commitParams.AppGroup;
            if (appGroup == null)
            {
                if (appManifestPath == null)
                {
                    throw new ArgumentNullException("commitParams");
                }
                storeTxn.Add(new System.Deployment.Internal.Isolation.StoreOperationStageComponent(appId.ComPointer, appManifestPath));
            }
            foreach (System.Deployment.Application.Manifest.File file in appManifest.GetFilesInGroup(appGroup, true))
            {
                this.PrepareInstallFile(storeTxn, file, appId, null, appPayloadPath);
            }
            foreach (DependentAssembly assembly in appManifest.GetPrivateAssembliesInGroup(appGroup, true))
            {
                this.PrepareInstallPrivateAssembly(storeTxn, assembly, appId, appPayloadPath);
            }
        }

        private void PrepareStageDeploymentComponent(StoreTransactionContext storeTxn, SubscriptionState subState, CommitApplicationParams commitParams)
        {
            System.Deployment.Application.DefinitionAppId id = commitParams.AppId.ToDeploymentAppId();
            string deployManifestPath = commitParams.DeployManifestPath;
            storeTxn.Add(new System.Deployment.Internal.Isolation.StoreOperationStageComponent(id.ComPointer, deployManifestPath));
            this.PrepareSetDeploymentProperties(storeTxn, commitParams.AppId, commitParams);
        }

        private void PrepareUpdateSkipTime(StoreTransactionContext storeTxn, SubscriptionState subState, System.Deployment.Application.DefinitionIdentity updateSkippedDeployment, DateTime updateSkipTime)
        {
            SubscriptionStateInternal newState = new SubscriptionStateInternal(subState) {
                UpdateSkippedDeployment = updateSkippedDeployment,
                UpdateSkipTime = updateSkipTime
            };
            this.PrepareFinalizeSubscriptionState(storeTxn, subState, newState);
        }

        public void RefreshStorePointer()
        {
            if (this._firstRefresh)
            {
                this._firstRefresh = false;
            }
            else
            {
                if (this._storeType != ComponentStoreType.UserStore)
                {
                    throw new NotImplementedException();
                }
                Marshal.ReleaseComObject(this._store.InternalStore);
                Marshal.ReleaseComObject(this._stateMgr);
                this._store = System.Deployment.Internal.Isolation.IsolationInterop.GetUserStore();
                Guid guidOfType = System.Deployment.Internal.Isolation.IsolationInterop.GetGuidOfType(typeof(System.Deployment.Internal.Isolation.IStateManager));
                this._stateMgr = System.Deployment.Internal.Isolation.IsolationInterop.GetUserStateManager(0, IntPtr.Zero, ref guidOfType) as System.Deployment.Internal.Isolation.IStateManager;
            }
        }

        internal void RemoveApplicationInstance(SubscriptionState subState, System.Deployment.Application.DefinitionAppId appId)
        {
            using (StoreTransactionContext context = new StoreTransactionContext(this))
            {
                this.PrepareRemoveDeployment(context, subState, appId);
                this.SubmitStoreTransaction(context, subState);
            }
        }

        public void RemoveSubscription(SubscriptionState subState)
        {
            try
            {
                using (StoreTransactionContext context = new StoreTransactionContext(this))
                {
                    this.PrepareRemoveSubscription(context, subState);
                    this.SubmitStoreTransactionCheckQuota(context, subState);
                }
            }
            catch (COMException exception)
            {
                if (exception.ErrorCode == -2147024784)
                {
                    throw new DeploymentException(ExceptionTypes.DiskIsFull, Resources.GetString("Ex_StoreOperationFailed"), exception);
                }
                throw;
            }
        }

        private static void ResetUpdateSkippedState(SubscriptionStateInternal newState)
        {
            newState.UpdateSkippedDeployment = null;
            newState.UpdateSkipTime = DateTime.MinValue;
        }

        public void RollbackSubscription(SubscriptionState subState)
        {
            try
            {
                using (StoreTransactionContext context = new StoreTransactionContext(this))
                {
                    this.PrepareRollbackSubscription(context, subState);
                    this.SubmitStoreTransactionCheckQuota(context, subState);
                }
            }
            catch (COMException exception)
            {
                if (exception.ErrorCode == -2147024784)
                {
                    throw new DeploymentException(ExceptionTypes.DiskIsFull, Resources.GetString("Ex_StoreOperationFailed"), exception);
                }
                throw;
            }
        }

        public void SetPendingDeployment(SubscriptionState subState, System.Deployment.Application.DefinitionIdentity deployId, DateTime checkTime)
        {
            try
            {
                using (StoreTransactionContext context = new StoreTransactionContext(this))
                {
                    this.PrepareSetPendingDeployment(context, subState, deployId, checkTime);
                    this.SubmitStoreTransaction(context, subState);
                }
            }
            catch (COMException exception)
            {
                if (exception.ErrorCode == -2147024784)
                {
                    throw new DeploymentException(ExceptionTypes.DiskIsFull, Resources.GetString("Ex_StoreOperationFailed"), exception);
                }
                throw;
            }
        }

        public void SetUpdateSkipTime(SubscriptionState subState, System.Deployment.Application.DefinitionIdentity updateSkippedDeployment, DateTime updateSkipTime)
        {
            try
            {
                using (StoreTransactionContext context = new StoreTransactionContext(this))
                {
                    this.PrepareUpdateSkipTime(context, subState, updateSkippedDeployment, updateSkipTime);
                    this.SubmitStoreTransaction(context, subState);
                }
            }
            catch (COMException exception)
            {
                if (exception.ErrorCode == -2147024784)
                {
                    throw new DeploymentException(ExceptionTypes.DiskIsFull, Resources.GetString("Ex_StoreOperationFailed"), exception);
                }
                throw;
            }
        }

        private void SubmitStoreTransaction(StoreTransactionContext storeTxn, SubscriptionState subState)
        {
            CodeMarker_Singleton.Instance.CodeMarker(CodeMarkerEvent.perfPersisterWriteStart);
            storeTxn.Add(new System.Deployment.Internal.Isolation.StoreOperationScavenge(false));
            System.Deployment.Internal.Isolation.StoreTransactionOperation[] operations = storeTxn.Operations;
            if (operations.Length > 0)
            {
                uint[] rgDispositions = new uint[operations.Length];
                int[] rgResults = new int[operations.Length];
                try
                {
                    uint num;
                    this._store.Transact(operations, rgDispositions, rgResults);
                    this._stateMgr.Scavenge(0, out num);
                }
                catch (DirectoryNotFoundException exception)
                {
                    throw new DeploymentException(ExceptionTypes.ComponentStore, Resources.GetString("Ex_TransactDirectoryNotFoundException"), exception);
                }
                catch (ArgumentException exception2)
                {
                    throw new DeploymentException(ExceptionTypes.ComponentStore, Resources.GetString("Ex_StoreOperationFailed"), exception2);
                }
                catch (UnauthorizedAccessException exception3)
                {
                    throw new DeploymentException(ExceptionTypes.ComponentStore, Resources.GetString("Ex_StoreOperationFailed"), exception3);
                }
                catch (IOException exception4)
                {
                    throw new DeploymentException(ExceptionTypes.ComponentStore, Resources.GetString("Ex_StoreOperationFailed"), exception4);
                }
                finally
                {
                    CodeMarker_Singleton.Instance.CodeMarker(CodeMarkerEvent.perfPersisterWriteEnd);
                    Logger.AddTransactionInformation(operations, rgDispositions, rgResults);
                }
                if (subState != null)
                {
                    subState.Invalidate();
                }
            }
        }

        private void SubmitStoreTransactionCheckQuota(StoreTransactionContext storeTxn, SubscriptionState subState)
        {
            storeTxn.ScavengeContext.CalculateSizesPreTransact();
            this.SubmitStoreTransaction(storeTxn, subState);
            storeTxn.ScavengeContext.CalculateSizesPostTransact();
            storeTxn.ScavengeContext.CheckQuotaAndScavenge();
        }

        private static string ToPropertyString(object propValue)
        {
            if (propValue == null)
            {
                return string.Empty;
            }
            if (propValue is bool)
            {
                bool flag = (bool) propValue;
                return flag.ToString(CultureInfo.InvariantCulture);
            }
            if (propValue is DateTime)
            {
                DateTime time = (DateTime) propValue;
                return time.ToString("yyyy/MM/dd HH:mm:ss", DateTimeFormatInfo.InvariantInfo);
            }
            if (propValue is Uri)
            {
                return ((Uri) propValue).AbsoluteUri;
            }
            return propValue.ToString();
        }

        private System.Deployment.Internal.Isolation.StoreApplicationReference InstallReference
        {
            get
            {
                if (_installReference == null)
                {
                    Interlocked.CompareExchange(ref _installReference, new System.Deployment.Internal.Isolation.StoreApplicationReference(System.Deployment.Internal.Isolation.IsolationInterop.GUID_SXS_INSTALL_REFERENCE_SCHEME_OPAQUESTRING, "{3f471841-eef2-47d6-89c0-d028f03a4ad5}", null), null);
                }
                return (System.Deployment.Internal.Isolation.StoreApplicationReference) _installReference;
            }
        }

        internal class CrossGroupApplicationData
        {
            public GroupType CrossGroupType;
            public SubscriptionState SubState;

            public CrossGroupApplicationData(SubscriptionState subState, GroupType groupType)
            {
                this.SubState = subState;
                this.CrossGroupType = groupType;
            }

            public enum GroupType
            {
                UndefinedGroup,
                LocationGroup,
                IdentityGroup
            }
        }

        private enum HostType
        {
            Default,
            AppLaunch,
            CorFlag
        }

        private class ScavengeContext
        {
            private ArrayList _addinDeploysToUnpin;
            private ulong _addinToUnpinSharedSize;
            private ComponentStore _compStore;
            private ArrayList _onlineDeploysToPin;
            private ArrayList _onlineDeploysToPinAlreadyPinned;
            private ArrayList _onlineDeploysToUnpin;
            private ulong _onlineToPinPrivateSizePostTransact;
            private ulong _onlineToPinPrivateSizePreTransact;
            private ulong _onlineToUnpinPrivateSize;
            private ArrayList _shellVisbleDeploysToUnpin;
            private ulong _shellVisibleToUnpinSharedSize;

            public ScavengeContext(ComponentStore compStore)
            {
                this._compStore = compStore;
            }

            private static void AddDeploymentToList(ref ArrayList list, System.Deployment.Application.DefinitionAppId deployAppId)
            {
                if (list == null)
                {
                    list = new ArrayList();
                }
                if (!list.Contains(deployAppId))
                {
                    list.Add(deployAppId);
                }
            }

            public void AddDeploymentToUnpin(System.Deployment.Application.DefinitionAppId deployAppId, SubscriptionState subState)
            {
                if (subState.IsShellVisible)
                {
                    AddDeploymentToList(ref this._shellVisbleDeploysToUnpin, deployAppId);
                }
                else if (subState.appType == AppType.CustomHostSpecified)
                {
                    AddDeploymentToList(ref this._addinDeploysToUnpin, deployAppId);
                }
                else
                {
                    AddDeploymentToList(ref this._onlineDeploysToUnpin, deployAppId);
                }
            }

            public void AddOnlineAppToCommit(System.Deployment.Application.DefinitionAppId appId, SubscriptionState subState)
            {
                System.Deployment.Application.DefinitionAppId deployAppId = appId.ToDeploymentAppId();
                AddDeploymentToList(ref this._onlineDeploysToPin, deployAppId);
                if (appId.Equals(subState.CurrentBind) || appId.Equals(subState.PreviousBind))
                {
                    AddDeploymentToList(ref this._onlineDeploysToPinAlreadyPinned, deployAppId);
                }
            }

            public void CalculateSizesPostTransact()
            {
                this._onlineToPinPrivateSizePostTransact = this._compStore.GetPrivateSize(this._onlineDeploysToPin);
            }

            public void CalculateSizesPreTransact()
            {
                this._onlineToPinPrivateSizePreTransact = this._compStore.GetPrivateSize(this._onlineDeploysToPinAlreadyPinned);
                this._onlineToUnpinPrivateSize = this._compStore.GetPrivateSize(this._onlineDeploysToUnpin);
                this._shellVisibleToUnpinSharedSize = this._compStore.GetSharedSize(this._shellVisbleDeploysToUnpin);
                this._addinToUnpinSharedSize = this._compStore.GetSharedSize(this._addinDeploysToUnpin);
            }

            public void CheckQuotaAndScavenge()
            {
                ulong maxValue;
                ulong onlineAppQuotaInBytes = this._compStore.GetOnlineAppQuotaInBytes();
                ulong onlineAppQuotaUsageEstimate = this.GetOnlineAppQuotaUsageEstimate();
                long num3 = (long) ((((this._onlineToPinPrivateSizePostTransact - this._onlineToPinPrivateSizePreTransact) - this._onlineToUnpinPrivateSize) + this._shellVisibleToUnpinSharedSize) + this._addinToUnpinSharedSize);
                if (num3 >= 0L)
                {
                    maxValue = onlineAppQuotaUsageEstimate + ((ulong) num3);
                    if (maxValue < onlineAppQuotaUsageEstimate)
                    {
                        maxValue = ulong.MaxValue;
                    }
                }
                else
                {
                    maxValue = onlineAppQuotaUsageEstimate - ((ulong) -num3);
                    if (maxValue > onlineAppQuotaUsageEstimate)
                    {
                        maxValue = ulong.MaxValue;
                    }
                }
                if (maxValue > onlineAppQuotaInBytes)
                {
                    System.Deployment.Internal.Isolation.IDefinitionAppId[] idArray;
                    SubInstance[] subs = this.CollectOnlineAppsMRU(out idArray);
                    ulong privateSize = 0L;
                    ulong sharedSize = 0L;
                    if (idArray.Length > 0)
                    {
                        this._compStore.CalculateDeploymentsUnderQuota(idArray.Length, idArray, ulong.MaxValue, ref privateSize, ref sharedSize);
                        if (privateSize > onlineAppQuotaInBytes)
                        {
                            bool flag;
                            ulong quotaSize = onlineAppQuotaInBytes / ((ulong) 2L);
                            int num8 = this._compStore.CalculateDeploymentsUnderQuota(idArray.Length, idArray, quotaSize, ref privateSize, ref sharedSize);
                            this.ScavengeAppsOverQuota(subs, idArray.Length - num8, out flag);
                            if (flag)
                            {
                                this.CollectOnlineApps(out idArray);
                                this._compStore.CalculateDeploymentsUnderQuota(idArray.Length, idArray, ulong.MaxValue, ref privateSize, ref sharedSize);
                            }
                        }
                    }
                    maxValue = privateSize;
                }
                PersistOnlineAppQuotaUsageEstimate(maxValue);
            }

            public void CleanOnlineAppCache()
            {
                System.Deployment.Internal.Isolation.IDefinitionAppId[] idArray;
                SubInstance[] instanceArray = this.CollectOnlineApps(out idArray);
                using (ComponentStore.StoreTransactionContext context = new ComponentStore.StoreTransactionContext(this._compStore))
                {
                    foreach (SubInstance instance in instanceArray)
                    {
                        SubscriptionStateInternal newState = new SubscriptionStateInternal(instance.SubState) {
                            IsInstalled = false
                        };
                        this._compStore.PrepareFinalizeSubscriptionState(context, instance.SubState, newState);
                    }
                    this._compStore.SubmitStoreTransaction(context, null);
                }
                instanceArray = this.CollectOnlineApps(out idArray);
                ulong privateSize = 0L;
                ulong sharedSize = 0L;
                if (idArray.Length > 0)
                {
                    this._compStore.CalculateDeploymentsUnderQuota(idArray.Length, idArray, ulong.MaxValue, ref privateSize, ref sharedSize);
                }
                PersistOnlineAppQuotaUsageEstimate(privateSize);
            }

            private SubInstance[] CollectOnlineApps(out System.Deployment.Internal.Isolation.IDefinitionAppId[] deployAppIdPtrs)
            {
                Hashtable hashtable = new Hashtable();
                foreach (System.Deployment.Internal.Isolation.STORE_ASSEMBLY store_assembly in this._compStore._store.EnumAssemblies(System.Deployment.Internal.Isolation.Store.EnumAssembliesFlags.Nothing))
                {
                    System.Deployment.Application.DefinitionIdentity subId = new System.Deployment.Application.DefinitionIdentity(store_assembly.DefinitionIdentity).ToSubscriptionId();
                    SubscriptionState subscriptionState = this._compStore._subStore.GetSubscriptionState(subId);
                    if ((subscriptionState.IsInstalled && !subscriptionState.IsShellVisible) && ((subscriptionState.appType != AppType.CustomHostSpecified) && !hashtable.Contains(subId)))
                    {
                        SubInstance instance = new SubInstance {
                            SubState = subscriptionState,
                            LastAccessTime = subscriptionState.LastCheckTime
                        };
                        hashtable.Add(subId, instance);
                    }
                }
                SubInstance[] array = new SubInstance[hashtable.Count];
                hashtable.Values.CopyTo(array, 0);
                ArrayList list = new ArrayList();
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i].SubState.CurrentBind != null)
                    {
                        list.Add(array[i].SubState.CurrentBind.ToDeploymentAppId().ComPointer);
                    }
                    if (array[i].SubState.PreviousBind != null)
                    {
                        list.Add(array[i].SubState.PreviousBind.ToDeploymentAppId().ComPointer);
                    }
                }
                deployAppIdPtrs = (System.Deployment.Internal.Isolation.IDefinitionAppId[]) list.ToArray(typeof(System.Deployment.Internal.Isolation.IDefinitionAppId));
                return array;
            }

            private SubInstance[] CollectOnlineAppsMRU(out System.Deployment.Internal.Isolation.IDefinitionAppId[] deployAppIdPtrs)
            {
                Hashtable hashtable = new Hashtable();
                foreach (System.Deployment.Internal.Isolation.STORE_ASSEMBLY store_assembly in this._compStore._store.EnumAssemblies(System.Deployment.Internal.Isolation.Store.EnumAssembliesFlags.Nothing))
                {
                    System.Deployment.Application.DefinitionIdentity subId = new System.Deployment.Application.DefinitionIdentity(store_assembly.DefinitionIdentity).ToSubscriptionId();
                    SubscriptionState subscriptionState = this._compStore._subStore.GetSubscriptionState(subId);
                    if ((subscriptionState.IsInstalled && !subscriptionState.IsShellVisible) && ((subscriptionState.appType != AppType.CustomHostSpecified) && !hashtable.Contains(subId)))
                    {
                        SubInstance instance = new SubInstance {
                            SubState = subscriptionState,
                            LastAccessTime = subscriptionState.LastCheckTime
                        };
                        hashtable.Add(subId, instance);
                    }
                }
                SubInstance[] array = new SubInstance[hashtable.Count];
                hashtable.Values.CopyTo(array, 0);
                Array.Sort<SubInstance>(array);
                ArrayList list = new ArrayList();
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i].SubState.CurrentBind != null)
                    {
                        list.Add(array[i].SubState.CurrentBind.ToDeploymentAppId().ComPointer);
                    }
                    if (array[i].SubState.PreviousBind != null)
                    {
                        list.Add(array[i].SubState.PreviousBind.ToDeploymentAppId().ComPointer);
                    }
                }
                deployAppIdPtrs = (System.Deployment.Internal.Isolation.IDefinitionAppId[]) list.ToArray(typeof(System.Deployment.Internal.Isolation.IDefinitionAppId));
                return array;
            }

            private ulong GetOnlineAppQuotaUsageEstimate()
            {
                ulong maxValue = ulong.MaxValue;
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Classes\Software\Microsoft\Windows\CurrentVersion\Deployment"))
                {
                    if (key != null)
                    {
                        object obj2 = key.GetValue("OnlineAppQuotaUsageEstimate");
                        if (obj2 is long)
                        {
                            long num2 = (long) obj2;
                            maxValue = (num2 >= 0L) ? ((ulong) num2) : ((ulong) ((-1L - -num2) + 1L));
                        }
                    }
                    return maxValue;
                }
            }

            private static void PersistOnlineAppQuotaUsageEstimate(ulong usage)
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Classes\Software\Microsoft\Windows\CurrentVersion\Deployment"))
                {
                    if (key != null)
                    {
                        key.SetValue("OnlineAppQuotaUsageEstimate", usage, RegistryValueKind.QWord);
                    }
                }
            }

            private void ScavengeAppsOverQuota(SubInstance[] subs, int deploysToScavenge, out bool appExcluded)
            {
                appExcluded = false;
                DateTime time = DateTime.UtcNow - Constants.OnlineAppScavengingGracePeriod;
                using (ComponentStore.StoreTransactionContext context = new ComponentStore.StoreTransactionContext(this._compStore))
                {
                    for (int i = subs.Length - 1; (i >= 0) && (deploysToScavenge > 0); i--)
                    {
                        bool flag = false;
                        bool flag2 = false;
                        if (subs[i].SubState.PreviousBind != null)
                        {
                            if (subs[i].LastAccessTime >= time)
                            {
                                appExcluded = true;
                            }
                            else
                            {
                                flag = true;
                            }
                            deploysToScavenge--;
                        }
                        if (deploysToScavenge > 0)
                        {
                            if (subs[i].LastAccessTime >= time)
                            {
                                appExcluded = true;
                            }
                            else
                            {
                                flag2 = true;
                            }
                            deploysToScavenge--;
                        }
                        if (flag2 || flag)
                        {
                            SubscriptionStateInternal newState = new SubscriptionStateInternal(subs[i].SubState);
                            if (flag2)
                            {
                                newState.IsInstalled = false;
                            }
                            else
                            {
                                newState.PreviousBind = null;
                            }
                            this._compStore.PrepareFinalizeSubscriptionState(context, subs[i].SubState, newState);
                        }
                    }
                    this._compStore.SubmitStoreTransaction(context, null);
                }
            }

            private class SubInstance : IComparable
            {
                public DateTime LastAccessTime;
                public SubscriptionState SubState;

                public int CompareTo(object other)
                {
                    return ((ComponentStore.ScavengeContext.SubInstance) other).LastAccessTime.CompareTo(this.LastAccessTime);
                }
            }
        }

        private class StoreTransactionContext : System.Deployment.Internal.Isolation.StoreTransaction
        {
            private ComponentStore _compStore;
            private object _scavengeContext;

            public StoreTransactionContext(ComponentStore compStore)
            {
                this._compStore = compStore;
            }

            public System.Deployment.Application.ComponentStore.ScavengeContext ScavengeContext
            {
                get
                {
                    if (this._scavengeContext == null)
                    {
                        Interlocked.CompareExchange(ref this._scavengeContext, new System.Deployment.Application.ComponentStore.ScavengeContext(this._compStore), null);
                    }
                    return (System.Deployment.Application.ComponentStore.ScavengeContext) this._scavengeContext;
                }
            }
        }
    }
}

