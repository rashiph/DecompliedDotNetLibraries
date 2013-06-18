namespace System.Deployment.Application
{
    using System;
    using System.ComponentModel;
    using System.Deployment.Application.Manifest;
    using System.Deployment.Internal;
    using System.Globalization;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    public sealed class ApplicationDeployment
    {
        private bool _cancellationPending;
        private static ApplicationDeployment _currentDeployment = null;
        private Version _currentVersion;
        private EventHandlerList _events;
        private DefinitionAppId _fullAppId;
        private int _guard;
        private SubscriptionState _subState;
        private SubscriptionStore _subStore;
        private object _syncGroupDeploymentManager;
        private readonly CodeAccessPermission accessPermission;
        private readonly AsyncOperation asyncOperation;
        private static readonly object checkForUpdateCompletedKey = new object();
        private static readonly object checkForUpdateProgressChangedKey = new object();
        private static readonly object downloadFileGroupCompletedKey = new object();
        private static readonly object downloadFileGroupProgressChangedKey = new object();
        private const int guardAsync = 1;
        private const int guardInitial = 0;
        private const int guardSync = 2;
        private static readonly object lockObject = new object();
        private static readonly object updateCompletedKey = new object();
        private static readonly object updateProgressChangedKey = new object();

        public event CheckForUpdateCompletedEventHandler CheckForUpdateCompleted
        {
            add
            {
                this.Events.AddHandler(checkForUpdateCompletedKey, value);
            }
            remove
            {
                this.Events.RemoveHandler(checkForUpdateCompletedKey, value);
            }
        }

        public event DeploymentProgressChangedEventHandler CheckForUpdateProgressChanged
        {
            add
            {
                this.Events.AddHandler(checkForUpdateProgressChangedKey, value);
            }
            remove
            {
                this.Events.RemoveHandler(checkForUpdateProgressChangedKey, value);
            }
        }

        public event DownloadFileGroupCompletedEventHandler DownloadFileGroupCompleted
        {
            add
            {
                this.Events.AddHandler(downloadFileGroupCompletedKey, value);
            }
            remove
            {
                this.Events.RemoveHandler(downloadFileGroupCompletedKey, value);
            }
        }

        public event DeploymentProgressChangedEventHandler DownloadFileGroupProgressChanged
        {
            add
            {
                this.Events.AddHandler(downloadFileGroupProgressChangedKey, value);
            }
            remove
            {
                this.Events.RemoveHandler(downloadFileGroupProgressChangedKey, value);
            }
        }

        public event AsyncCompletedEventHandler UpdateCompleted
        {
            add
            {
                this.Events.AddHandler(updateCompletedKey, value);
            }
            remove
            {
                this.Events.RemoveHandler(updateCompletedKey, value);
            }
        }

        public event DeploymentProgressChangedEventHandler UpdateProgressChanged
        {
            add
            {
                this.Events.AddHandler(updateProgressChangedKey, value);
            }
            remove
            {
                this.Events.RemoveHandler(updateProgressChangedKey, value);
            }
        }

        private ApplicationDeployment(string fullAppId)
        {
            if (fullAppId.Length > 0x10000)
            {
                throw new InvalidDeploymentException(Resources.GetString("Ex_AppIdTooLong"));
            }
            try
            {
                this._fullAppId = new DefinitionAppId(fullAppId);
            }
            catch (COMException exception)
            {
                throw new InvalidDeploymentException(string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_SubAppIdNotValid"), new object[] { fullAppId }), exception);
            }
            catch (SEHException exception2)
            {
                throw new InvalidDeploymentException(string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_SubAppIdNotValid"), new object[] { fullAppId }), exception2);
            }
            DefinitionIdentity deploymentIdentity = this._fullAppId.DeploymentIdentity;
            this._currentVersion = deploymentIdentity.Version;
            DefinitionIdentity subId = deploymentIdentity.ToSubscriptionId();
            this._subStore = SubscriptionStore.CurrentUser;
            this._subState = this._subStore.GetSubscriptionState(subId);
            if (!this._subState.IsInstalled)
            {
                throw new InvalidDeploymentException(Resources.GetString("Ex_SubNotInstalled"));
            }
            if (!this._fullAppId.Equals(this._subState.CurrentBind))
            {
                throw new InvalidDeploymentException(Resources.GetString("Ex_AppIdNotMatchInstalled"));
            }
            Uri uri = new Uri(this._fullAppId.Codebase);
            if (uri.IsFile)
            {
                this.accessPermission = new FileIOPermission(FileIOPermissionAccess.Read, uri.LocalPath);
            }
            else
            {
                this.accessPermission = new WebPermission(NetworkAccess.Connect, this._fullAppId.Codebase);
            }
            this.accessPermission.Demand();
            this._events = new EventHandlerList();
            this.asyncOperation = AsyncOperationManager.CreateOperation(null);
        }

        public UpdateCheckInfo CheckForDetailedUpdate()
        {
            return this.CheckForDetailedUpdate(true);
        }

        public UpdateCheckInfo CheckForDetailedUpdate(bool persistUpdateCheckResult)
        {
            new NamedPermissionSet("FullTrust").Demand();
            if (Interlocked.CompareExchange(ref this._guard, 2, 0) != 0)
            {
                throw new InvalidOperationException(Resources.GetString("Ex_SingleOperation"));
            }
            this._cancellationPending = false;
            UpdateCheckInfo info = null;
            try
            {
                using (DeploymentManager manager = this.CreateDeploymentManager())
                {
                    manager.Bind();
                    info = this.DetermineUpdateCheckResult(manager.ActivationDescription);
                    if (info.UpdateAvailable)
                    {
                        manager.DeterminePlatformRequirements();
                        try
                        {
                            TrustParams trustParams = new TrustParams {
                                NoPrompt = true
                            };
                            manager.DetermineTrust(trustParams);
                        }
                        catch (TrustNotGrantedException)
                        {
                            if (!manager.ActivationDescription.IsUpdateInPKTGroup)
                            {
                                throw;
                            }
                        }
                    }
                    if (persistUpdateCheckResult)
                    {
                        this.ProcessUpdateCheckResult(info, manager.ActivationDescription);
                    }
                }
            }
            finally
            {
                Interlocked.Exchange(ref this._guard, 0);
            }
            return info;
        }

        public bool CheckForUpdate()
        {
            return this.CheckForUpdate(true);
        }

        public bool CheckForUpdate(bool persistUpdateCheckResult)
        {
            return this.CheckForDetailedUpdate(persistUpdateCheckResult).UpdateAvailable;
        }

        public void CheckForUpdateAsync()
        {
            new NamedPermissionSet("FullTrust").Demand();
            if (Interlocked.CompareExchange(ref this._guard, 1, 0) != 0)
            {
                throw new InvalidOperationException(Resources.GetString("Ex_SingleOperation"));
            }
            this._cancellationPending = false;
            DeploymentManager manager = this.CreateDeploymentManager();
            manager.ProgressChanged += new DeploymentProgressChangedEventHandler(this.CheckForUpdateProgressChangedEventHandler);
            manager.BindCompleted += new BindCompletedEventHandler(this.CheckForUpdateBindCompletedEventHandler);
            manager.BindAsync();
        }

        public void CheckForUpdateAsyncCancel()
        {
            if (this._guard == 1)
            {
                this._cancellationPending = true;
            }
        }

        private void CheckForUpdateBindCompletedEventHandler(object sender, BindCompletedEventArgs e)
        {
            Exception error = null;
            DeploymentManager manager = null;
            bool updateAvailable = false;
            Version availableVersion = null;
            bool isUpdateRequired = false;
            Version minimumRequiredVersion = null;
            long updateSize = 0L;
            new NamedPermissionSet("FullTrust").Assert();
            try
            {
                manager = (DeploymentManager) sender;
                if ((e.Error == null) && !e.Cancelled)
                {
                    UpdateCheckInfo info = this.DetermineUpdateCheckResult(manager.ActivationDescription);
                    if (info.UpdateAvailable)
                    {
                        manager.DeterminePlatformRequirements();
                        try
                        {
                            TrustParams trustParams = new TrustParams {
                                NoPrompt = true
                            };
                            manager.DetermineTrust(trustParams);
                        }
                        catch (TrustNotGrantedException)
                        {
                            if (!manager.ActivationDescription.IsUpdateInPKTGroup)
                            {
                                throw;
                            }
                        }
                    }
                    this.ProcessUpdateCheckResult(info, manager.ActivationDescription);
                    if (info.UpdateAvailable)
                    {
                        updateAvailable = true;
                        availableVersion = info.AvailableVersion;
                        isUpdateRequired = info.IsUpdateRequired;
                        minimumRequiredVersion = info.MinimumRequiredVersion;
                        updateSize = info.UpdateSizeBytes;
                    }
                }
                else
                {
                    error = e.Error;
                }
            }
            catch (Exception exception2)
            {
                if (ExceptionUtility.IsHardException(exception2))
                {
                    throw;
                }
                error = exception2;
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
                Interlocked.Exchange(ref this._guard, 0);
                CheckForUpdateCompletedEventArgs args = new CheckForUpdateCompletedEventArgs(error, e.Cancelled, null, updateAvailable, availableVersion, isUpdateRequired, minimumRequiredVersion, updateSize);
                CheckForUpdateCompletedEventHandler handler = (CheckForUpdateCompletedEventHandler) this.Events[checkForUpdateCompletedKey];
                if (handler != null)
                {
                    handler(this, args);
                }
                if (manager != null)
                {
                    manager.ProgressChanged -= new DeploymentProgressChangedEventHandler(this.CheckForUpdateProgressChangedEventHandler);
                    manager.BindCompleted -= new BindCompletedEventHandler(this.CheckForUpdateBindCompletedEventHandler);
                    new NamedPermissionSet("FullTrust").Assert();
                    try
                    {
                        manager.Dispose();
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                }
            }
        }

        private void CheckForUpdateProgressChangedEventHandler(object sender, DeploymentProgressChangedEventArgs e)
        {
            if (this._cancellationPending)
            {
                ((DeploymentManager) sender).CancelAsync();
            }
            DeploymentProgressChangedEventHandler handler = (DeploymentProgressChangedEventHandler) this.Events[checkForUpdateProgressChangedKey];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private DeploymentManager CreateDeploymentManager()
        {
            this._subState.Invalidate();
            return new DeploymentManager(this._subState.DeploymentProviderUri, true, true, null, this.asyncOperation) { Callertype = DeploymentManager.CallerType.ApplicationDeployment };
        }

        private void DemandPermission()
        {
            this.accessPermission.Demand();
        }

        private UpdateCheckInfo DetermineUpdateCheckResult(ActivationDescription actDesc)
        {
            bool updateAvailable = false;
            Version availableVersion = null;
            bool isUpdateRequired = false;
            Version minimumRequiredVersion = null;
            long updateSize = 0L;
            bool bUpdateInPKTGroup = false;
            AssemblyManifest deployManifest = actDesc.DeployManifest;
            this._subState.Invalidate();
            Version version3 = this._subStore.CheckUpdateInManifest(this._subState, actDesc.DeploySourceUri, deployManifest, this._currentVersion, ref bUpdateInPKTGroup);
            if ((version3 != null) && !deployManifest.Identity.Equals(this._subState.ExcludedDeployment))
            {
                updateAvailable = true;
                availableVersion = version3;
                minimumRequiredVersion = deployManifest.Deployment.MinimumRequiredVersion;
                if ((minimumRequiredVersion != null) && (minimumRequiredVersion.CompareTo(this._currentVersion) > 0))
                {
                    isUpdateRequired = true;
                }
                ulong num2 = actDesc.AppManifest.CalculateDependenciesSize();
                if (num2 > 0x7fffffffffffffffL)
                {
                    updateSize = 0x7fffffffffffffffL;
                }
                else
                {
                    updateSize = (long) num2;
                }
                actDesc.IsUpdateInPKTGroup = bUpdateInPKTGroup;
            }
            return new UpdateCheckInfo(updateAvailable, availableVersion, isUpdateRequired, minimumRequiredVersion, updateSize);
        }

        [PermissionSet(SecurityAction.Assert, Name="FullTrust")]
        public void DownloadFileGroup(string groupName)
        {
            if (groupName == null)
            {
                throw new ArgumentNullException("groupName");
            }
            this._subState.Invalidate();
            if (!this._fullAppId.Equals(this._subState.CurrentBind))
            {
                throw new InvalidOperationException(Resources.GetString("Ex_DownloadGroupAfterUpdate"));
            }
            this.SyncGroupDeploymentManager.Synchronize(groupName);
        }

        public void DownloadFileGroupAsync(string groupName)
        {
            this.DownloadFileGroupAsync(groupName, null);
        }

        [PermissionSet(SecurityAction.Assert, Name="FullTrust")]
        public void DownloadFileGroupAsync(string groupName, object userState)
        {
            if (groupName == null)
            {
                throw new ArgumentNullException("groupName");
            }
            this._subState.Invalidate();
            if (!this._fullAppId.Equals(this._subState.CurrentBind))
            {
                throw new InvalidOperationException(Resources.GetString("Ex_DownloadGroupAfterUpdate"));
            }
            this.SyncGroupDeploymentManager.SynchronizeAsync(groupName, userState);
        }

        [PermissionSet(SecurityAction.Assert, Name="FullTrust")]
        public void DownloadFileGroupAsyncCancel(string groupName)
        {
            if (groupName == null)
            {
                throw new ArgumentNullException("groupName");
            }
            this.SyncGroupDeploymentManager.CancelAsync(groupName);
        }

        private void DownloadFileGroupProgressChangedEventHandler(object sender, DeploymentProgressChangedEventArgs e)
        {
            DeploymentProgressChangedEventHandler handler = (DeploymentProgressChangedEventHandler) this.Events[downloadFileGroupProgressChangedKey];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void EndUpdateAsync(DeploymentManager dm, Exception error, bool cancelled)
        {
            Interlocked.Exchange(ref this._guard, 0);
            AsyncCompletedEventArgs e = new AsyncCompletedEventArgs(error, cancelled, null);
            AsyncCompletedEventHandler handler = (AsyncCompletedEventHandler) this.Events[updateCompletedKey];
            if (handler != null)
            {
                handler(this, e);
            }
            if (dm != null)
            {
                dm.ProgressChanged -= new DeploymentProgressChangedEventHandler(this.UpdateProgressChangedEventHandler);
                dm.BindCompleted -= new BindCompletedEventHandler(this.UpdateBindCompletedEventHandler);
                dm.SynchronizeCompleted -= new SynchronizeCompletedEventHandler(this.SynchronizeNullCompletedEventHandler);
                new NamedPermissionSet("FullTrust").Assert();
                try
                {
                    dm.Dispose();
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
            }
        }

        [PermissionSet(SecurityAction.Assert, Name="FullTrust")]
        public bool IsFileGroupDownloaded(string groupName)
        {
            return this._subStore.CheckGroupInstalled(this._subState, this._fullAppId, groupName);
        }

        private void ProcessUpdateCheckResult(UpdateCheckInfo info, ActivationDescription actDesc)
        {
            if (this._subState.IsShellVisible)
            {
                AssemblyManifest deployManifest = actDesc.DeployManifest;
                DefinitionIdentity deployId = info.UpdateAvailable ? deployManifest.Identity : null;
                this._subStore.SetPendingDeployment(this._subState, deployId, DateTime.UtcNow);
            }
        }

        private void SynchronizeGroupCompletedEventHandler(object sender, SynchronizeCompletedEventArgs e)
        {
            try
            {
                DeploymentManager manager1 = (DeploymentManager) sender;
                Exception error = e.Error;
            }
            catch (Exception exception)
            {
                if (ExceptionUtility.IsHardException(exception))
                {
                    throw;
                }
            }
            finally
            {
                DownloadFileGroupCompletedEventArgs args = new DownloadFileGroupCompletedEventArgs(e.Error, e.Cancelled, e.UserState, e.Group);
                DownloadFileGroupCompletedEventHandler handler = (DownloadFileGroupCompletedEventHandler) this.Events[downloadFileGroupCompletedKey];
                if (handler != null)
                {
                    handler(this, args);
                }
            }
        }

        private void SynchronizeNullCompletedEventHandler(object sender, SynchronizeCompletedEventArgs e)
        {
            Exception error = null;
            DeploymentManager dm = null;
            new NamedPermissionSet("FullTrust").Assert();
            try
            {
                dm = (DeploymentManager) sender;
                error = e.Error;
            }
            catch (Exception exception2)
            {
                if (ExceptionUtility.IsHardException(exception2))
                {
                    throw;
                }
                error = exception2;
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
                this.EndUpdateAsync(dm, error, e.Cancelled);
            }
        }

        public bool Update()
        {
            new NamedPermissionSet("FullTrust").Demand();
            if (Interlocked.CompareExchange(ref this._guard, 2, 0) != 0)
            {
                throw new InvalidOperationException(Resources.GetString("Ex_SingleOperation"));
            }
            this._cancellationPending = false;
            try
            {
                using (DeploymentManager manager = this.CreateDeploymentManager())
                {
                    manager.Bind();
                    UpdateCheckInfo info = this.DetermineUpdateCheckResult(manager.ActivationDescription);
                    if (info.UpdateAvailable)
                    {
                        manager.DeterminePlatformRequirements();
                        try
                        {
                            TrustParams trustParams = new TrustParams {
                                NoPrompt = true
                            };
                            manager.DetermineTrust(trustParams);
                        }
                        catch (TrustNotGrantedException)
                        {
                            if (!manager.ActivationDescription.IsUpdateInPKTGroup)
                            {
                                throw;
                            }
                        }
                    }
                    this.ProcessUpdateCheckResult(info, manager.ActivationDescription);
                    if (!info.UpdateAvailable)
                    {
                        return false;
                    }
                    manager.Synchronize();
                    if (manager.ActivationDescription.IsUpdateInPKTGroup)
                    {
                        this._subState = this._subStore.GetSubscriptionState(manager.ActivationDescription.DeployManifest);
                    }
                }
            }
            finally
            {
                Interlocked.Exchange(ref this._guard, 0);
            }
            return true;
        }

        public void UpdateAsync()
        {
            new NamedPermissionSet("FullTrust").Demand();
            if (Interlocked.CompareExchange(ref this._guard, 1, 0) != 0)
            {
                throw new InvalidOperationException(Resources.GetString("Ex_SingleOperation"));
            }
            this._cancellationPending = false;
            DeploymentManager manager = this.CreateDeploymentManager();
            manager.ProgressChanged += new DeploymentProgressChangedEventHandler(this.UpdateProgressChangedEventHandler);
            manager.BindCompleted += new BindCompletedEventHandler(this.UpdateBindCompletedEventHandler);
            manager.SynchronizeCompleted += new SynchronizeCompletedEventHandler(this.SynchronizeNullCompletedEventHandler);
            manager.BindAsync();
        }

        public void UpdateAsyncCancel()
        {
            if (this._guard == 1)
            {
                this._cancellationPending = true;
            }
        }

        private void UpdateBindCompletedEventHandler(object sender, BindCompletedEventArgs e)
        {
            Exception error = null;
            DeploymentManager dm = null;
            bool flag = false;
            new NamedPermissionSet("FullTrust").Assert();
            try
            {
                dm = (DeploymentManager) sender;
                if ((e.Error == null) && !e.Cancelled)
                {
                    UpdateCheckInfo info = this.DetermineUpdateCheckResult(dm.ActivationDescription);
                    if (info.UpdateAvailable)
                    {
                        dm.DeterminePlatformRequirements();
                        try
                        {
                            TrustParams trustParams = new TrustParams {
                                NoPrompt = true
                            };
                            dm.DetermineTrust(trustParams);
                        }
                        catch (TrustNotGrantedException)
                        {
                            if (!dm.ActivationDescription.IsUpdateInPKTGroup)
                            {
                                throw;
                            }
                        }
                    }
                    this.ProcessUpdateCheckResult(info, dm.ActivationDescription);
                    if (info.UpdateAvailable)
                    {
                        flag = true;
                        dm.SynchronizeAsync();
                    }
                    if (dm.ActivationDescription.IsUpdateInPKTGroup)
                    {
                        this._subState = this._subStore.GetSubscriptionState(dm.ActivationDescription.DeployManifest);
                    }
                }
                else
                {
                    error = e.Error;
                }
            }
            catch (Exception exception2)
            {
                if (ExceptionUtility.IsHardException(exception2))
                {
                    throw;
                }
                error = exception2;
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
                if (!flag)
                {
                    this.EndUpdateAsync(dm, error, e.Cancelled);
                }
            }
        }

        private void UpdateProgressChangedEventHandler(object sender, DeploymentProgressChangedEventArgs e)
        {
            if (this._cancellationPending)
            {
                ((DeploymentManager) sender).CancelAsync();
            }
            DeploymentProgressChangedEventHandler handler = (DeploymentProgressChangedEventHandler) this.Events[updateProgressChangedKey];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public Uri ActivationUri
        {
            [PermissionSet(SecurityAction.Assert, Name="FullTrust")]
            get
            {
                this._subState.Invalidate();
                if (this._subState.CurrentDeploymentManifest.Deployment.TrustURLParameters)
                {
                    string[] activationData = AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData;
                    if ((activationData == null) || (activationData[0] == null))
                    {
                        return null;
                    }
                    Uri uri = new Uri(activationData[0]);
                    if (!uri.IsFile && !uri.IsUnc)
                    {
                        return uri;
                    }
                }
                return null;
            }
        }

        public static ApplicationDeployment CurrentDeployment
        {
            [PermissionSet(SecurityAction.Assert, Name="FullTrust")]
            get
            {
                bool flag = false;
                if (_currentDeployment == null)
                {
                    lock (lockObject)
                    {
                        if (_currentDeployment == null)
                        {
                            string fullName = null;
                            ActivationContext activationContext = AppDomain.CurrentDomain.ActivationContext;
                            if (activationContext != null)
                            {
                                fullName = activationContext.Identity.FullName;
                            }
                            if (string.IsNullOrEmpty(fullName))
                            {
                                throw new InvalidDeploymentException(Resources.GetString("Ex_AppIdNotSet"));
                            }
                            _currentDeployment = new ApplicationDeployment(fullName);
                            flag = true;
                        }
                    }
                }
                if (!flag)
                {
                    _currentDeployment.DemandPermission();
                }
                return _currentDeployment;
            }
        }

        public Version CurrentVersion
        {
            get
            {
                return this._currentVersion;
            }
        }

        public string DataDirectory
        {
            get
            {
                object data = AppDomain.CurrentDomain.GetData("DataDirectory");
                if (data == null)
                {
                    return null;
                }
                return data.ToString();
            }
        }

        private EventHandlerList Events
        {
            get
            {
                return this._events;
            }
        }

        public bool IsFirstRun
        {
            [PermissionSet(SecurityAction.Assert, Name="FullTrust")]
            get
            {
                return InternalActivationContextHelper.IsFirstRun(AppDomain.CurrentDomain.ActivationContext);
            }
        }

        public static bool IsNetworkDeployed
        {
            get
            {
                bool flag = true;
                try
                {
                    ApplicationDeployment currentDeployment = CurrentDeployment;
                }
                catch (InvalidDeploymentException)
                {
                    flag = false;
                }
                return flag;
            }
        }

        private DeploymentManager SyncGroupDeploymentManager
        {
            get
            {
                if (this._syncGroupDeploymentManager == null)
                {
                    DeploymentManager manager = null;
                    bool flag = false;
                    try
                    {
                        manager = new DeploymentManager(this._fullAppId.ToString(), true, true, null, this.asyncOperation) {
                            Callertype = DeploymentManager.CallerType.ApplicationDeployment
                        };
                        manager.Bind();
                        flag = Interlocked.CompareExchange(ref this._syncGroupDeploymentManager, manager, null) == null;
                    }
                    finally
                    {
                        if (!flag && (manager != null))
                        {
                            manager.Dispose();
                        }
                    }
                    if (flag)
                    {
                        manager.ProgressChanged += new DeploymentProgressChangedEventHandler(this.DownloadFileGroupProgressChangedEventHandler);
                        manager.SynchronizeCompleted += new SynchronizeCompletedEventHandler(this.SynchronizeGroupCompletedEventHandler);
                    }
                }
                return (DeploymentManager) this._syncGroupDeploymentManager;
            }
        }

        public DateTime TimeOfLastUpdateCheck
        {
            [PermissionSet(SecurityAction.Assert, Name="FullTrust")]
            get
            {
                this._subState.Invalidate();
                return this._subState.LastCheckTime;
            }
        }

        public string UpdatedApplicationFullName
        {
            [PermissionSet(SecurityAction.Assert, Name="FullTrust")]
            get
            {
                this._subState.Invalidate();
                return this._subState.CurrentBind.ToString();
            }
        }

        public Version UpdatedVersion
        {
            [PermissionSet(SecurityAction.Assert, Name="FullTrust")]
            get
            {
                this._subState.Invalidate();
                return this._subState.CurrentDeployment.Version;
            }
        }

        public Uri UpdateLocation
        {
            [PermissionSet(SecurityAction.Assert, Name="FullTrust")]
            get
            {
                this._subState.Invalidate();
                return this._subState.DeploymentProviderUri;
            }
        }
    }
}

