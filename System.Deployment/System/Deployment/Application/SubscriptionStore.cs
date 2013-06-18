namespace System.Deployment.Application
{
    using System;
    using System.Collections;
    using System.Deployment.Application.Manifest;
    using System.Deployment.Application.Win32InterOp;
    using System.Deployment.Internal.Isolation;
    using System.Deployment.Internal.Isolation.Manifest;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;

    internal class SubscriptionStore
    {
        private ComponentStore _compStore;
        private static object _currentUserLock = new object();
        private string _deployPath;
        private object _subscriptionStoreLock;
        private string _tempPath;
        private static SubscriptionStore _userStore;

        private SubscriptionStore(string deployPath, string tempPath, ComponentStoreType storeType)
        {
            this._deployPath = deployPath;
            this._tempPath = tempPath;
            Directory.CreateDirectory(this._deployPath);
            Directory.CreateDirectory(this._tempPath);
            using (this.AcquireStoreWriterLock())
            {
                this._compStore = ComponentStore.GetStore(storeType, this);
            }
        }

        private IDisposable AcquireLock(System.Deployment.Application.DefinitionIdentity asmId, bool writer)
        {
            string keyForm = asmId.KeyForm;
            Directory.CreateDirectory(this._deployPath);
            return LockedFile.AcquireLock(Path.Combine(this._deployPath, keyForm), Constants.LockTimeout, writer);
        }

        public FileStream AcquireReferenceTransaction(out long transactionId)
        {
            transactionId = 0L;
            return null;
        }

        public IDisposable AcquireStoreReaderLock()
        {
            return this.AcquireLock(this.SubscriptionStoreLock, false);
        }

        public IDisposable AcquireStoreWriterLock()
        {
            return this.AcquireLock(this.SubscriptionStoreLock, true);
        }

        public IDisposable AcquireSubscriptionReaderLock(SubscriptionState subState)
        {
            subState.Invalidate();
            return this.AcquireStoreReaderLock();
        }

        public IDisposable AcquireSubscriptionWriterLock(SubscriptionState subState)
        {
            subState.Invalidate();
            return this.AcquireStoreWriterLock();
        }

        public TempDirectory AcquireTempDirectory()
        {
            return new TempDirectory(this._tempPath);
        }

        public TempFile AcquireTempFile(string suffix)
        {
            return new TempFile(this._tempPath, suffix);
        }

        public void ActivateApplication(System.Deployment.Application.DefinitionAppId appId, string activationParameter, bool useActivationParameter)
        {
            using (this.AcquireStoreReaderLock())
            {
                this._compStore.ActivateApplication(appId, activationParameter, useActivationParameter);
            }
        }

        public bool CheckAndReferenceApplication(SubscriptionState subState, System.Deployment.Application.DefinitionAppId appId, long transactionId)
        {
            System.Deployment.Application.DefinitionIdentity deploymentIdentity = appId.DeploymentIdentity;
            System.Deployment.Application.DefinitionIdentity applicationIdentity = appId.ApplicationIdentity;
            if (!subState.IsInstalled || !this.IsAssemblyInstalled(deploymentIdentity))
            {
                return false;
            }
            if (!this.IsAssemblyInstalled(applicationIdentity))
            {
                throw new DeploymentException(ExceptionTypes.Subscription, Resources.GetString("Ex_IllegalApplicationId"));
            }
            if (!appId.Equals(subState.CurrentBind))
            {
                return appId.Equals(subState.PreviousBind);
            }
            return true;
        }

        private void CheckApplicationPayload(CommitApplicationParams commitParams)
        {
            if ((commitParams.AppGroup == null) && (commitParams.appType != AppType.CustomHostSpecified))
            {
                SystemUtils.CheckSupportedImageAndCLRVersions(Path.Combine(commitParams.AppPayloadPath, commitParams.AppManifest.EntryPoints[0].CommandFile));
            }
            string directoryName = null;
            System.Deployment.Internal.Isolation.Store.IPathLock @lock = null;
            try
            {
                @lock = this._compStore.LockAssemblyPath(commitParams.AppManifest.Identity);
                directoryName = Path.GetDirectoryName(@lock.Path);
                directoryName = Path.Combine(directoryName, "manifests");
                directoryName = Path.Combine(directoryName, Path.GetFileName(@lock.Path) + ".manifest");
            }
            catch (DeploymentException)
            {
            }
            catch (COMException)
            {
            }
            finally
            {
                if (@lock != null)
                {
                    @lock.Dispose();
                }
            }
            if ((!string.IsNullOrEmpty(directoryName) && System.IO.File.Exists(directoryName)) && (!string.IsNullOrEmpty(commitParams.AppManifestPath) && System.IO.File.Exists(commitParams.AppManifestPath)))
            {
                byte[] bytes = ComponentVerifier.GenerateDigestValue(directoryName, System.Deployment.Internal.Isolation.Manifest.CMS_HASH_DIGESTMETHOD.CMS_HASH_DIGESTMETHOD_SHA1, System.Deployment.Internal.Isolation.Manifest.CMS_HASH_TRANSFORM.CMS_HASH_TRANSFORM_IDENTITY);
                byte[] buffer2 = ComponentVerifier.GenerateDigestValue(commitParams.AppManifestPath, System.Deployment.Internal.Isolation.Manifest.CMS_HASH_DIGESTMETHOD.CMS_HASH_DIGESTMETHOD_SHA1, System.Deployment.Internal.Isolation.Manifest.CMS_HASH_TRANSFORM.CMS_HASH_TRANSFORM_IDENTITY);
                bool flag = false;
                Logger.AddInternalState("In-place update check. Existing manifest path = " + directoryName + ", Existing manifest hash=" + Encoding.UTF8.GetString(bytes) + ", New manifest path=" + commitParams.AppManifestPath + ", New manifest hash=" + Encoding.UTF8.GetString(buffer2));
                if (bytes.Length == buffer2.Length)
                {
                    int index = 0;
                    while (index < bytes.Length)
                    {
                        if (bytes[index] != buffer2[index])
                        {
                            break;
                        }
                        index++;
                    }
                    if (index >= bytes.Length)
                    {
                        flag = true;
                    }
                }
                if (!flag)
                {
                    throw new DeploymentException(ExceptionTypes.Subscription, Resources.GetString("Ex_ApplicationInplaceUpdate"));
                }
            }
        }

        public void CheckCustomUXFlag(SubscriptionState subState, AssemblyManifest application)
        {
            if (subState.IsInstalled)
            {
                if (application.EntryPoints[0].CustomUX && (subState.appType != AppType.CustomUX))
                {
                    throw new DeploymentException(Resources.GetString("Ex_CustomUXAlready"));
                }
                if (!application.EntryPoints[0].CustomUX && (subState.appType == AppType.CustomUX))
                {
                    throw new DeploymentException(Resources.GetString("Ex_NotCustomUXAlready"));
                }
            }
        }

        public void CheckDeploymentSubscriptionState(SubscriptionState subState, AssemblyManifest deployment)
        {
            if (subState.IsInstalled)
            {
                CheckOnlineShellVisibleConflict(subState, deployment);
                CheckInstalledAndUpdateableConflict(subState, deployment);
                CheckMinimumRequiredVersion(subState, deployment);
            }
        }

        public void CheckForDeploymentUpdate(SubscriptionState subState)
        {
            this.CheckInstalledAndShellVisible(subState);
            Uri deploymentProviderUri = subState.DeploymentProviderUri;
            using (TempFile file = null)
            {
                AssemblyManifest deployment = DownloadManager.DownloadDeploymentManifest(subState.SubscriptionStore, ref deploymentProviderUri, out file);
                Version version = this.CheckUpdateInManifest(subState, deploymentProviderUri, deployment, subState.CurrentDeployment.Version);
                System.Deployment.Application.DefinitionIdentity deployId = (version != null) ? deployment.Identity : null;
                this.SetPendingDeployment(subState, deployId, DateTime.UtcNow);
                if ((version != null) && deployment.Identity.Equals(subState.PendingDeployment))
                {
                    Logger.AddPhaseInformation(Resources.GetString("Upd_FoundUpdate"), new object[] { subState.SubscriptionId.ToString(), deployment.Identity.Version.ToString(), deploymentProviderUri.AbsoluteUri });
                }
            }
        }

        public bool CheckGroupInstalled(SubscriptionState subState, System.Deployment.Application.DefinitionAppId appId, string groupName)
        {
            using (this.AcquireSubscriptionReaderLock(subState))
            {
                return this._compStore.CheckGroupInstalled(appId, groupName);
            }
        }

        public bool CheckGroupInstalled(SubscriptionState subState, System.Deployment.Application.DefinitionAppId appId, AssemblyManifest appManifest, string groupName)
        {
            using (this.AcquireSubscriptionReaderLock(subState))
            {
                return this._compStore.CheckGroupInstalled(appId, appManifest, groupName);
            }
        }

        public static void CheckInstalled(SubscriptionState subState)
        {
            if (!subState.IsInstalled)
            {
                throw new DeploymentException(ExceptionTypes.SubscriptionState, Resources.GetString("Ex_SubNotInstalled"));
            }
        }

        public void CheckInstalledAndShellVisible(SubscriptionState subState)
        {
            CheckInstalled(subState);
            CheckShellVisible(subState);
        }

        private static void CheckInstalledAndUpdateableConflict(SubscriptionState subState, AssemblyManifest deployment)
        {
        }

        private static void CheckMinimumRequiredVersion(SubscriptionState subState, AssemblyManifest deployment)
        {
            if (subState.MinimumRequiredVersion != null)
            {
                if (deployment.Identity.Version < subState.MinimumRequiredVersion)
                {
                    throw new DeploymentException(ExceptionTypes.SubscriptionState, Resources.GetString("Ex_DeploymentBelowMinimumRequiredVersion"));
                }
                if ((deployment.Deployment.MinimumRequiredVersion != null) && (deployment.Deployment.MinimumRequiredVersion < subState.MinimumRequiredVersion))
                {
                    throw new DeploymentException(ExceptionTypes.SubscriptionState, Resources.GetString("Ex_DecreasingMinimumRequiredVersion"));
                }
            }
        }

        private static void CheckOnlineShellVisibleConflict(SubscriptionState subState, AssemblyManifest deployment)
        {
            if (!deployment.Deployment.Install && subState.IsShellVisible)
            {
                throw new DeploymentException(ExceptionTypes.SubscriptionState, Resources.GetString("Ex_OnlineAlreadyShellVisible"));
            }
        }

        public static void CheckShellVisible(SubscriptionState subState)
        {
            if (!subState.IsShellVisible)
            {
                throw new DeploymentException(ExceptionTypes.SubscriptionState, Resources.GetString("Ex_SubNotShellVisible"));
            }
        }

        public Version CheckUpdateInManifest(SubscriptionState subState, Uri updateCodebaseUri, AssemblyManifest deployment, Version currentVersion)
        {
            bool bUpdateInPKTGroup = false;
            return this.CheckUpdateInManifest(subState, updateCodebaseUri, deployment, currentVersion, ref bUpdateInPKTGroup);
        }

        public Version CheckUpdateInManifest(SubscriptionState subState, Uri updateCodebaseUri, AssemblyManifest deployment, Version currentVersion, ref bool bUpdateInPKTGroup)
        {
            CheckOnlineShellVisibleConflict(subState, deployment);
            CheckInstalledAndUpdateableConflict(subState, deployment);
            CheckMinimumRequiredVersion(subState, deployment);
            SubscriptionState subscriptionState = this.GetSubscriptionState(deployment);
            if (!subscriptionState.SubscriptionId.Equals(subState.SubscriptionId))
            {
                Logger.AddInternalState("Cross family update detected. Check if only PKT has changed between versions.");
                Logger.AddInternalState(string.Concat(new object[] { "updateCodebaseUri=", updateCodebaseUri, ", subState.DeploymentProviderUri=", subState.DeploymentProviderUri }));
                Logger.AddInternalState(string.Concat(new object[] { "subState=", subState.SubscriptionId, ", manSubState.SubscriptionId=", subscriptionState.SubscriptionId }));
                if (!updateCodebaseUri.Equals(subState.DeploymentProviderUri) || !subState.PKTGroupId.Equals(subscriptionState.PKTGroupId))
                {
                    throw new DeploymentException(ExceptionTypes.SubscriptionState, Resources.GetString("Ex_DeploymentIdentityNotInSubscription"));
                }
                Logger.AddInternalState("PKT has changed.");
                bUpdateInPKTGroup = true;
            }
            Version version = deployment.Identity.Version;
            if (version.CompareTo(currentVersion) == 0)
            {
                return null;
            }
            return version;
        }

        public void CleanOnlineAppCache()
        {
            using (this.AcquireStoreWriterLock())
            {
                this._compStore.RefreshStorePointer();
                this._compStore.CleanOnlineAppCache();
            }
        }

        public void CommitApplication(ref SubscriptionState subState, CommitApplicationParams commitParams)
        {
            Logger.AddMethodCall("CommitApplication called.");
            using (this.AcquireSubscriptionWriterLock(subState))
            {
                if (commitParams.CommitDeploy)
                {
                    Logger.AddInternalState("Commiting Deployment :  subscription metadata.");
                    UriHelper.ValidateSupportedScheme(commitParams.DeploySourceUri);
                    this.CheckDeploymentSubscriptionState(subState, commitParams.DeployManifest);
                    this.ValidateFileAssoctiation(subState, commitParams);
                    if (commitParams.IsUpdate && !commitParams.IsUpdateInPKTGroup)
                    {
                        CheckInstalled(subState);
                    }
                }
                if (commitParams.CommitApp)
                {
                    Logger.AddInternalState("Commiting Application:  application binaries.");
                    UriHelper.ValidateSupportedScheme(commitParams.AppSourceUri);
                    if (commitParams.AppGroup != null)
                    {
                        CheckInstalled(subState);
                    }
                    this.CheckApplicationPayload(commitParams);
                }
                bool flag = false;
                bool identityGroupFound = false;
                bool locationGroupFound = false;
                string identityGroupProductName = "";
                ArrayList list = this._compStore.CollectCrossGroupApplications(commitParams.DeploySourceUri, commitParams.DeployManifest.Identity, ref identityGroupFound, ref locationGroupFound, ref identityGroupProductName);
                if (list.Count > 0)
                {
                    flag = true;
                    Logger.AddInternalState("This installation is a Cross Group: identityGroupFound=" + identityGroupFound.ToString() + ",locationGroupFound=" + locationGroupFound.ToString());
                }
                if ((subState.IsShellVisible && identityGroupFound) && locationGroupFound)
                {
                    throw new DeploymentException(ExceptionTypes.GroupMultipleMatch, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_GroupMultipleMatch"), new object[] { identityGroupProductName }));
                }
                subState = this.GetSubscriptionState(commitParams.DeployManifest);
                this._compStore.CommitApplication(subState, commitParams);
                if (flag)
                {
                    uint num;
                    Logger.AddInternalState("Performing cross group migration.");
                    System.Deployment.Internal.Isolation.IActContext context = System.Deployment.Internal.Isolation.IsolationInterop.CreateActContext(subState.CurrentBind.ComPointer);
                    context.PrepareForExecution(IntPtr.Zero, IntPtr.Zero);
                    context.SetApplicationRunningState(0, 1, out num);
                    context.SetApplicationRunningState(0, 2, out num);
                    Logger.AddInternalState("Uninstalling all cross groups.");
                    foreach (ComponentStore.CrossGroupApplicationData data in list)
                    {
                        if (data.CrossGroupType == ComponentStore.CrossGroupApplicationData.GroupType.LocationGroup)
                        {
                            if (data.SubState.appType == AppType.CustomHostSpecified)
                            {
                                Logger.AddInternalState("UninstallCustomHostSpecifiedSubscription : " + ((data.SubState.SubscriptionId != null) ? data.SubState.SubscriptionId.ToString() : "null"));
                                this.UninstallCustomHostSpecifiedSubscription(data.SubState);
                            }
                            else if (data.SubState.appType == AppType.CustomUX)
                            {
                                Logger.AddInternalState("UninstallCustomUXSubscription : " + ((data.SubState.SubscriptionId != null) ? data.SubState.SubscriptionId.ToString() : "null"));
                                this.UninstallCustomUXSubscription(data.SubState);
                            }
                            else if (data.SubState.IsShellVisible)
                            {
                                Logger.AddInternalState("UninstallSubscription : " + ((data.SubState.SubscriptionId != null) ? data.SubState.SubscriptionId.ToString() : "null"));
                                this.UninstallSubscription(data.SubState);
                            }
                        }
                        else if (data.CrossGroupType == ComponentStore.CrossGroupApplicationData.GroupType.IdentityGroup)
                        {
                            Logger.AddInternalState("Not uninstalled :" + ((data.SubState.SubscriptionId != null) ? data.SubState.SubscriptionId.ToString() : "null") + ". It is in the identity group.");
                        }
                    }
                }
                if ((commitParams.IsConfirmed && subState.IsInstalled) && (subState.IsShellVisible && (commitParams.appType != AppType.CustomUX)))
                {
                    this.UpdateSubscriptionExposure(subState);
                }
                if (commitParams.appType == AppType.CustomUX)
                {
                    ShellExposure.ShellExposureInformation shellExposureInformation = ShellExposure.ShellExposureInformation.CreateShellExposureInformation(subState.SubscriptionId);
                    ShellExposure.UpdateShellExtensions(subState, ref shellExposureInformation);
                }
                OnDeploymentAdded(subState);
            }
        }

        internal ulong GetOnlineAppQuotaInBytes()
        {
            return this._compStore.GetOnlineAppQuotaInBytes();
        }

        internal ulong GetPrivateSize(System.Deployment.Application.DefinitionAppId appId)
        {
            ArrayList deployAppIds = new ArrayList();
            deployAppIds.Add(appId);
            using (this.AcquireStoreReaderLock())
            {
                return this._compStore.GetPrivateSize(deployAppIds);
            }
        }

        internal ulong GetSharedSize(System.Deployment.Application.DefinitionAppId appId)
        {
            ArrayList deployAppIds = new ArrayList();
            deployAppIds.Add(appId);
            using (this.AcquireStoreReaderLock())
            {
                return this._compStore.GetSharedSize(deployAppIds);
            }
        }

        internal ulong GetSizeLimitInBytesForSemiTrustApps()
        {
            return (this.GetOnlineAppQuotaInBytes() / ((ulong) 2L));
        }

        public SubscriptionState GetSubscriptionState(System.Deployment.Application.DefinitionIdentity subId)
        {
            return new SubscriptionState(this, subId);
        }

        public SubscriptionState GetSubscriptionState(AssemblyManifest deployment)
        {
            return new SubscriptionState(this, deployment);
        }

        public SubscriptionStateInternal GetSubscriptionStateInternal(SubscriptionState subState)
        {
            using (this.AcquireSubscriptionReaderLock(subState))
            {
                return this._compStore.GetSubscriptionStateInternal(subState);
            }
        }

        private bool IsAssemblyInstalled(System.Deployment.Application.DefinitionIdentity asmId)
        {
            using (this.AcquireStoreReaderLock())
            {
                return this._compStore.IsAssemblyInstalled(asmId);
            }
        }

        internal System.Deployment.Internal.Isolation.Store.IPathLock LockApplicationPath(System.Deployment.Application.DefinitionAppId definitionAppId)
        {
            using (this.AcquireStoreReaderLock())
            {
                return this._compStore.LockApplicationPath(definitionAppId);
            }
        }

        private static void OnDeploymentAdded(SubscriptionState subState)
        {
        }

        private static void OnDeploymentRemoved(SubscriptionState subState)
        {
        }

        public void RefreshStorePointer()
        {
            using (this.AcquireStoreWriterLock())
            {
                this._compStore.RefreshStorePointer();
            }
        }

        private static void RemoveSubscriptionExposure(SubscriptionState subState)
        {
            ShellExposure.RemoveSubscriptionShellExposure(subState);
        }

        public void RollbackSubscription(SubscriptionState subState)
        {
            using (this.AcquireSubscriptionWriterLock(subState))
            {
                this.CheckInstalledAndShellVisible(subState);
                if (subState.RollbackDeployment == null)
                {
                    throw new DeploymentException(ExceptionTypes.SubscriptionState, Resources.GetString("Ex_SubNoRollbackDeployment"));
                }
                if (subState.CurrentApplicationManifest != null)
                {
                    string productName = null;
                    if ((subState.CurrentDeploymentManifest != null) && (subState.CurrentDeploymentManifest.Description != null))
                    {
                        productName = subState.CurrentDeploymentManifest.Description.Product;
                    }
                    if (productName == null)
                    {
                        productName = subState.SubscriptionId.Name;
                    }
                    ShellExposure.RemoveShellExtensions(subState.SubscriptionId, subState.CurrentApplicationManifest, productName);
                }
                this._compStore.RollbackSubscription(subState);
                this.UpdateSubscriptionExposure(subState);
                OnDeploymentRemoved(subState);
            }
        }

        public void SetLastCheckTimeToNow(SubscriptionState subState)
        {
            using (this.AcquireSubscriptionWriterLock(subState))
            {
                CheckInstalled(subState);
                this._compStore.SetPendingDeployment(subState, null, DateTime.UtcNow);
            }
        }

        public void SetPendingDeployment(SubscriptionState subState, System.Deployment.Application.DefinitionIdentity deployId, DateTime checkTime)
        {
            using (this.AcquireSubscriptionWriterLock(subState))
            {
                this.CheckInstalledAndShellVisible(subState);
                this._compStore.SetPendingDeployment(subState, deployId, checkTime);
            }
        }

        public void SetUpdateSkipTime(SubscriptionState subState, System.Deployment.Application.DefinitionIdentity updateSkippedDeployment, DateTime updateSkipTime)
        {
            using (this.AcquireSubscriptionWriterLock(subState))
            {
                this.CheckInstalledAndShellVisible(subState);
                this._compStore.SetUpdateSkipTime(subState, updateSkippedDeployment, updateSkipTime);
            }
        }

        public void UninstallCustomHostSpecifiedSubscription(SubscriptionState subState)
        {
            using (this.AcquireSubscriptionWriterLock(subState))
            {
                CheckInstalled(subState);
                if (subState.appType != AppType.CustomHostSpecified)
                {
                    throw new InvalidOperationException(Resources.GetString("Ex_CannotCallUninstallCustomAddIn"));
                }
                this._compStore.RemoveSubscription(subState);
                OnDeploymentRemoved(subState);
            }
        }

        public void UninstallCustomUXSubscription(SubscriptionState subState)
        {
            using (this.AcquireSubscriptionWriterLock(subState))
            {
                CheckInstalled(subState);
                if (subState.appType != AppType.CustomUX)
                {
                    throw new InvalidOperationException(Resources.GetString("Ex_CannotCallUninstallCustomUXApplication"));
                }
                if (subState.CurrentApplicationManifest != null)
                {
                    string productName = null;
                    if ((subState.CurrentDeploymentManifest != null) && (subState.CurrentDeploymentManifest.Description != null))
                    {
                        productName = subState.CurrentDeploymentManifest.Description.Product;
                    }
                    if (productName == null)
                    {
                        productName = subState.SubscriptionId.Name;
                    }
                    ShellExposure.RemoveShellExtensions(subState.SubscriptionId, subState.CurrentApplicationManifest, productName);
                }
                this._compStore.RemoveSubscription(subState);
                OnDeploymentRemoved(subState);
            }
        }

        public void UninstallSubscription(SubscriptionState subState)
        {
            using (this.AcquireSubscriptionWriterLock(subState))
            {
                this.CheckInstalledAndShellVisible(subState);
                if (subState.CurrentApplicationManifest != null)
                {
                    string productName = null;
                    if ((subState.CurrentDeploymentManifest != null) && (subState.CurrentDeploymentManifest.Description != null))
                    {
                        productName = subState.CurrentDeploymentManifest.Description.Product;
                    }
                    if (productName == null)
                    {
                        productName = subState.SubscriptionId.Name;
                    }
                    ShellExposure.RemoveShellExtensions(subState.SubscriptionId, subState.CurrentApplicationManifest, productName);
                    ShellExposure.RemovePins(subState);
                }
                this._compStore.RemoveSubscription(subState);
                RemoveSubscriptionExposure(subState);
                OnDeploymentRemoved(subState);
            }
        }

        private void UpdateSubscriptionExposure(SubscriptionState subState)
        {
            this.CheckInstalledAndShellVisible(subState);
            ShellExposure.UpdateSubscriptionShellExposure(subState);
        }

        public void ValidateFileAssoctiation(SubscriptionState subState, CommitApplicationParams commitParams)
        {
            if (((commitParams.DeployManifest != null) && (commitParams.AppManifest != null)) && (!commitParams.DeployManifest.Deployment.Install && (commitParams.AppManifest.FileAssociations.Length > 0)))
            {
                throw new DeploymentException(ExceptionTypes.SubscriptionState, Resources.GetString("Ex_OnlineAppWithFileAssociation"));
            }
        }

        public static SubscriptionStore CurrentUser
        {
            get
            {
                if (_userStore == null)
                {
                    lock (_currentUserLock)
                    {
                        if (_userStore == null)
                        {
                            string deployPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Deployment");
                            string tempPath = Path.Combine(Path.GetTempPath(), "Deployment");
                            _userStore = new SubscriptionStore(deployPath, tempPath, ComponentStoreType.UserStore);
                        }
                    }
                }
                return _userStore;
            }
        }

        private System.Deployment.Application.DefinitionIdentity SubscriptionStoreLock
        {
            get
            {
                if (this._subscriptionStoreLock == null)
                {
                    Interlocked.CompareExchange(ref this._subscriptionStoreLock, new System.Deployment.Application.DefinitionIdentity("__SubscriptionStoreLock__"), null);
                }
                return (System.Deployment.Application.DefinitionIdentity) this._subscriptionStoreLock;
            }
        }
    }
}

