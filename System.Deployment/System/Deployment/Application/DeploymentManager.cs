namespace System.Deployment.Application
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Deployment.Application.Manifest;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Threading;

    internal class DeploymentManager : IDisposable, IDownloadNotification
    {
        private ActivationContext _actCtx;
        private System.Deployment.Application.ActivationDescription _actDesc;
        private ManualResetEvent[] _assertApplicationReqEvents;
        private DefinitionAppId _bindAppId;
        private int _bindGuard;
        private bool _cached;
        private CallerType _callerType;
        private bool _cancellationPending;
        private Uri _deploySource;
        private long _downloadedAppSize;
        private DownloadOptions _downloadOptions;
        private EventHandlerList _events;
        private bool _isConfirmed;
        private bool _isupdate;
        private Logger.LogIdentity _log;
        private ManualResetEvent _platformRequirementsFailedEvent;
        private FileStream _referenceTransaction;
        private DeploymentProgressState _state;
        private SubscriptionStore _subStore;
        private Hashtable _syncGroupMap;
        private int _syncGuard;
        private TempDirectory _tempApplicationDirectory;
        private TempFile _tempDeployment;
        private ManualResetEvent _trustGrantedEvent;
        private ManualResetEvent _trustNotGrantedEvent;
        private readonly AsyncOperation asyncOperation;
        private readonly SendOrPostCallback bindCompleted;
        private static readonly object bindCompletedKey = new object();
        private readonly ThreadStart bindWorker;
        private static readonly object progressChangedKey = new object();
        private readonly SendOrPostCallback progressReporter;
        private readonly SendOrPostCallback synchronizeCompleted;
        private static readonly object synchronizeCompletedKey = new object();
        private readonly WaitCallback synchronizeGroupWorker;
        private readonly ThreadStart synchronizeWorker;

        public event BindCompletedEventHandler BindCompleted
        {
            add
            {
                this.Events.AddHandler(bindCompletedKey, value);
            }
            remove
            {
                this.Events.RemoveHandler(bindCompletedKey, value);
            }
        }

        public event DeploymentProgressChangedEventHandler ProgressChanged
        {
            add
            {
                this.Events.AddHandler(progressChangedKey, value);
            }
            remove
            {
                this.Events.RemoveHandler(progressChangedKey, value);
            }
        }

        public event SynchronizeCompletedEventHandler SynchronizeCompleted
        {
            add
            {
                this.Events.AddHandler(synchronizeCompletedKey, value);
            }
            remove
            {
                this.Events.RemoveHandler(synchronizeCompletedKey, value);
            }
        }

        [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        public DeploymentManager(string appId) : this(appId, false, true, null, null)
        {
            if (appId == null)
            {
                throw new ArgumentNullException("appId");
            }
        }

        [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        public DeploymentManager(Uri deploymentSource) : this(deploymentSource, false, true, null, null)
        {
            if (deploymentSource == null)
            {
                throw new ArgumentNullException("deploymentSource");
            }
            UriHelper.ValidateSupportedSchemeInArgument(deploymentSource, "deploymentSource");
        }

        internal DeploymentManager(string appId, bool isUpdate, bool isConfirmed, DownloadOptions downloadOptions, AsyncOperation optionalAsyncOp) : this((Uri) null, isUpdate, isConfirmed, downloadOptions, optionalAsyncOp)
        {
            this._bindAppId = new DefinitionAppId(appId);
        }

        internal DeploymentManager(Uri deploymentSource, bool isUpdate, bool isConfirmed, DownloadOptions downloadOptions, AsyncOperation optionalAsyncOp)
        {
            this._trustNotGrantedEvent = new ManualResetEvent(false);
            this._trustGrantedEvent = new ManualResetEvent(false);
            this._platformRequirementsFailedEvent = new ManualResetEvent(false);
            this._isConfirmed = true;
            this._state = DeploymentProgressState.DownloadingApplicationFiles;
            this._deploySource = deploymentSource;
            this._isupdate = isUpdate;
            this._isConfirmed = isConfirmed;
            this._downloadOptions = downloadOptions;
            this._events = new EventHandlerList();
            this._syncGroupMap = CollectionsUtil.CreateCaseInsensitiveHashtable();
            this._subStore = SubscriptionStore.CurrentUser;
            this.bindWorker = new ThreadStart(this.BindAsyncWorker);
            this.synchronizeWorker = new ThreadStart(this.SynchronizeAsyncWorker);
            this.synchronizeGroupWorker = new WaitCallback(this.SynchronizeGroupAsyncWorker);
            this.bindCompleted = new SendOrPostCallback(this.BindAsyncCompleted);
            this.synchronizeCompleted = new SendOrPostCallback(this.SynchronizeAsyncCompleted);
            this.progressReporter = new SendOrPostCallback(this.ProgressReporter);
            if (optionalAsyncOp == null)
            {
                this.asyncOperation = AsyncOperationManager.CreateOperation(null);
            }
            else
            {
                this.asyncOperation = optionalAsyncOp;
            }
            this._log = Logger.StartLogging();
            if (deploymentSource != null)
            {
                Logger.SetSubscriptionUrl(this._log, deploymentSource);
            }
            this._assertApplicationReqEvents = new ManualResetEvent[] { this._trustNotGrantedEvent, this._platformRequirementsFailedEvent, this._trustGrantedEvent };
            this._callerType = CallerType.Other;
            PolicyKeys.SkipApplicationDependencyHashCheck();
            PolicyKeys.SkipDeploymentProvider();
            PolicyKeys.SkipSchemaValidation();
            PolicyKeys.SkipSemanticValidation();
            PolicyKeys.SkipSignatureValidation();
        }

        private SyncGroupHelper AttachToGroup(string groupName, object userState, out bool created)
        {
            created = false;
            SyncGroupHelper helper = null;
            lock (this._syncGroupMap.SyncRoot)
            {
                helper = (SyncGroupHelper) this._syncGroupMap[groupName];
                if (helper == null)
                {
                    helper = new SyncGroupHelper(groupName, userState, this.asyncOperation, this.progressReporter);
                    this._syncGroupMap[groupName] = helper;
                    created = true;
                }
            }
            return helper;
        }

        public ActivationContext Bind()
        {
            if (Interlocked.Exchange(ref this._bindGuard, 1) != 0)
            {
                throw new InvalidOperationException(Resources.GetString("Ex_BindOnce"));
            }
            bool flag = false;
            TempFile tempDeploy = null;
            TempDirectory tempAppDir = null;
            FileStream refTransaction = null;
            try
            {
                string productName = null;
                this.BindCore(true, ref tempDeploy, ref tempAppDir, ref refTransaction, ref productName);
            }
            catch (Exception)
            {
                flag = true;
                throw;
            }
            finally
            {
                this._state = DeploymentProgressState.DownloadingApplicationFiles;
                if (flag)
                {
                    if (tempAppDir != null)
                    {
                        tempAppDir.Dispose();
                    }
                    if (tempDeploy != null)
                    {
                        tempDeploy.Dispose();
                    }
                    if (refTransaction != null)
                    {
                        refTransaction.Close();
                    }
                }
            }
            return this._actCtx;
        }

        public void BindAsync()
        {
            Logger.AddMethodCall(this._log, "DeploymentManager.BindAsync() called.");
            if (!this._cancellationPending)
            {
                if (Interlocked.Exchange(ref this._bindGuard, 1) != 0)
                {
                    throw new InvalidOperationException(Resources.GetString("Ex_BindOnce"));
                }
                this.bindWorker.BeginInvoke(null, null);
            }
        }

        private void BindAsyncCompleted(object arg)
        {
            BindCompletedEventArgs e = (BindCompletedEventArgs) arg;
            BindCompletedEventHandler handler = (BindCompletedEventHandler) this.Events[bindCompletedKey];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void BindAsyncWorker()
        {
            Exception error = null;
            bool cancelled = false;
            string productName = null;
            TempFile tempDeploy = null;
            TempDirectory tempAppDir = null;
            FileStream refTransaction = null;
            try
            {
                Logger.AddInternalState(this._log, "Binding started in a worker thread.");
                cancelled = this.BindCore(false, ref tempDeploy, ref tempAppDir, ref refTransaction, ref productName);
                Logger.AddInternalState(this._log, "Binding is successful.");
            }
            catch (Exception exception2)
            {
                if (ExceptionUtility.IsHardException(exception2))
                {
                    throw;
                }
                if (exception2 is DownloadCancelledException)
                {
                    cancelled = true;
                }
                else
                {
                    error = exception2;
                }
            }
            finally
            {
                this._state = DeploymentProgressState.DownloadingApplicationFiles;
                if ((error != null) || cancelled)
                {
                    if (tempAppDir != null)
                    {
                        tempAppDir.Dispose();
                    }
                    if (tempDeploy != null)
                    {
                        tempDeploy.Dispose();
                    }
                    if (refTransaction != null)
                    {
                        refTransaction.Close();
                    }
                }
                BindCompletedEventArgs arg = new BindCompletedEventArgs(error, cancelled, null, this._actCtx, productName, this._cached);
                this.asyncOperation.Post(this.bindCompleted, arg);
            }
        }

        private bool BindCore(bool blocking, ref TempFile tempDeploy, ref TempDirectory tempAppDir, ref FileStream refTransaction, ref string productName)
        {
            try
            {
                long num;
                Uri uri2;
                string str2;
                if (this._deploySource == null)
                {
                    return this.BindCoreWithAppId(blocking, ref refTransaction, ref productName);
                }
                bool flag = false;
                AssemblyManifest manifest = null;
                string manifestPath = null;
                Uri sourceUri = this._deploySource;
                this._state = DeploymentProgressState.DownloadingDeploymentInformation;
                Logger.AddInternalState(this._log, "Internal state=" + this._state);
                manifest = DownloadManager.DownloadDeploymentManifest(this._subStore, ref sourceUri, out tempDeploy, blocking ? null : this, this._downloadOptions);
                manifestPath = tempDeploy.Path;
                System.Deployment.Application.ActivationDescription actDesc = new System.Deployment.Application.ActivationDescription();
                actDesc.SetDeploymentManifest(manifest, sourceUri, manifestPath);
                Logger.SetDeploymentManifest(this._log, manifest);
                actDesc.IsUpdate = this._isupdate;
                if (actDesc.DeployManifest.Deployment == null)
                {
                    throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_NotDeploymentOrShortcut"));
                }
                if (!blocking && this._cancellationPending)
                {
                    return true;
                }
                refTransaction = this._subStore.AcquireReferenceTransaction(out num);
                SubscriptionState subscriptionState = this._subStore.GetSubscriptionState(actDesc.DeployManifest);
                if (((actDesc.DeployManifest.Deployment.Install && (actDesc.DeployManifest.Deployment.ProviderCodebaseUri == null)) && ((subscriptionState != null) && (subscriptionState.DeploymentProviderUri != null))) && !subscriptionState.DeploymentProviderUri.Equals(sourceUri))
                {
                    throw new DeploymentException(ExceptionTypes.DeploymentUriDifferent, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_DeploymentUriDifferentExText"), new object[] { actDesc.DeployManifest.Description.FilteredProduct, sourceUri.AbsoluteUri, subscriptionState.DeploymentProviderUri.AbsoluteUri }));
                }
                DefinitionAppId appId = null;
                try
                {
                    appId = new DefinitionAppId(actDesc.ToAppCodebase(), new DefinitionIdentity[] { actDesc.DeployManifest.Identity, new DefinitionIdentity(actDesc.DeployManifest.MainDependentAssembly.Identity) });
                }
                catch (COMException exception)
                {
                    throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_IdentityIsNotValid"), exception);
                }
                catch (SEHException exception2)
                {
                    throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_IdentityIsNotValid"), exception2);
                }
                Logger.AddInternalState(this._log, "expectedAppId=" + appId.ToString());
                flag = this._subStore.CheckAndReferenceApplication(subscriptionState, appId, num);
                if (flag && appId.Equals(subscriptionState.CurrentBind))
                {
                    Logger.AddInternalState(this._log, "Application is found in store and it is the CurrentBind. Binding with appid.");
                    this._bindAppId = appId;
                    return this.BindCoreWithAppId(blocking, ref refTransaction, ref productName);
                }
                if (flag)
                {
                    Logger.AddInternalState(this._log, "Application is found in store but it is not the CurrentBind.");
                }
                else
                {
                    Logger.AddInternalState(this._log, "Application is not found in store.");
                }
                if (!blocking && this._cancellationPending)
                {
                    return true;
                }
                this._state = DeploymentProgressState.DownloadingApplicationInformation;
                Logger.AddInternalState(this._log, "Internal state=" + this._state);
                tempAppDir = this._subStore.AcquireTempDirectory();
                AssemblyManifest appManifest = DownloadManager.DownloadApplicationManifest(actDesc.DeployManifest, tempAppDir.Path, actDesc.DeploySourceUri, blocking ? null : this, this._downloadOptions, out uri2, out str2);
                AssemblyManifest.ReValidateManifestSignatures(actDesc.DeployManifest, appManifest);
                Logger.SetApplicationManifest(this._log, appManifest);
                Logger.SetApplicationUrl(this._log, uri2);
                actDesc.SetApplicationManifest(appManifest, uri2, str2);
                actDesc.AppId = new DefinitionAppId(actDesc.ToAppCodebase(), new DefinitionIdentity[] { actDesc.DeployManifest.Identity, actDesc.AppManifest.Identity });
                flag = this._subStore.CheckAndReferenceApplication(subscriptionState, actDesc.AppId, num);
                if (!blocking && this._cancellationPending)
                {
                    return true;
                }
                Description effectiveDescription = actDesc.EffectiveDescription;
                productName = effectiveDescription.Product;
                this._cached = flag;
                Logger.AddInternalState(this._log, "_cached=" + this._cached.ToString());
                Logger.AddInternalState(this._log, "_isupdate=" + this._isupdate.ToString());
                this._tempApplicationDirectory = tempAppDir;
                this._tempDeployment = tempDeploy;
                this._referenceTransaction = refTransaction;
                this._actCtx = ConstructActivationContext(actDesc);
                this._actDesc = actDesc;
            }
            catch (Exception exception3)
            {
                this.LogError(Resources.GetString("Ex_FailedToDownloadManifest"), exception3);
                Logger.AddInternalState(this._log, "Exception thrown in  BindCore(): " + exception3.GetType().ToString() + " : " + exception3.Message + "\r\n" + exception3.StackTrace);
                throw;
            }
            return false;
        }

        private bool BindCoreWithAppId(bool blocking, ref FileStream refTransaction, ref string productName)
        {
            long num;
            bool flag = false;
            DefinitionIdentity subId = this._bindAppId.DeploymentIdentity.ToSubscriptionId();
            SubscriptionState subscriptionState = this._subStore.GetSubscriptionState(subId);
            if (!subscriptionState.IsInstalled)
            {
                throw new InvalidDeploymentException(Resources.GetString("Ex_BindAppIdNotInstalled"));
            }
            if (!this._bindAppId.Equals(subscriptionState.CurrentBind))
            {
                throw new InvalidDeploymentException(Resources.GetString("Ex_BindAppIdNotCurrrent"));
            }
            if (!blocking && this._cancellationPending)
            {
                return true;
            }
            refTransaction = this._subStore.AcquireReferenceTransaction(out num);
            flag = this._subStore.CheckAndReferenceApplication(subscriptionState, this._bindAppId, num);
            System.Deployment.Application.ActivationDescription description = new System.Deployment.Application.ActivationDescription();
            description.SetDeploymentManifest(subscriptionState.CurrentDeploymentManifest, subscriptionState.CurrentDeploymentSourceUri, null);
            Logger.SetDeploymentManifest(this._log, subscriptionState.CurrentDeploymentManifest);
            description.IsUpdate = this._isupdate;
            description.SetApplicationManifest(subscriptionState.CurrentApplicationManifest, subscriptionState.CurrentApplicationSourceUri, null);
            Logger.SetApplicationManifest(this._log, subscriptionState.CurrentApplicationManifest);
            Logger.SetApplicationUrl(this._log, subscriptionState.CurrentApplicationSourceUri);
            description.AppId = new DefinitionAppId(description.ToAppCodebase(), new DefinitionIdentity[] { description.DeployManifest.Identity, description.AppManifest.Identity });
            if (!blocking && this._cancellationPending)
            {
                return true;
            }
            Description effectiveDescription = subscriptionState.EffectiveDescription;
            productName = effectiveDescription.Product;
            this._cached = flag;
            Logger.AddInternalState(this._log, "_cached=" + this._cached.ToString());
            Logger.AddInternalState(this._log, "_isupdate=" + this._isupdate.ToString());
            this._referenceTransaction = refTransaction;
            this._actCtx = ConstructActivationContextFromStore(description.AppId);
            this._actDesc = description;
            return false;
        }

        public void CancelAsync()
        {
            this._cancellationPending = true;
        }

        public void CancelAsync(string groupName)
        {
            if (groupName == null)
            {
                this.CancelAsync();
            }
            else
            {
                lock (this._syncGroupMap.SyncRoot)
                {
                    SyncGroupHelper helper = (SyncGroupHelper) this._syncGroupMap[groupName];
                    if (helper != null)
                    {
                        helper.CancelAsync();
                    }
                }
            }
        }

        private void CheckSizeLimit()
        {
            if ((this._actDesc.appType != AppType.CustomHostSpecified) && (this._actDesc.Trust != null))
            {
                bool flag = this._actDesc.Trust.DefaultGrantSet.PermissionSet.IsUnrestricted();
                bool flag2 = !this._actDesc.DeployManifest.Deployment.Install;
                if (!flag && flag2)
                {
                    ulong sizeLimitInBytesForSemiTrustApps = this._subStore.GetSizeLimitInBytesForSemiTrustApps();
                    if (this._downloadedAppSize > sizeLimitInBytesForSemiTrustApps)
                    {
                        throw new DeploymentDownloadException(ExceptionTypes.SizeLimitForPartialTrustOnlineAppExceeded, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_OnlineSemiTrustAppSizeLimitExceeded"), new object[] { sizeLimitInBytesForSemiTrustApps }));
                    }
                }
            }
        }

        private static ActivationContext ConstructActivationContext(System.Deployment.Application.ActivationDescription actDesc)
        {
            ApplicationIdentity identity = actDesc.AppId.ToApplicationIdentity();
            string[] manifestPaths = new string[] { actDesc.DeployManifestPath, actDesc.AppManifestPath };
            return ActivationContext.CreatePartialActivationContext(identity, manifestPaths);
        }

        private static ActivationContext ConstructActivationContextFromStore(DefinitionAppId defAppId)
        {
            return ActivationContext.CreatePartialActivationContext(defAppId.ToApplicationIdentity());
        }

        private void DetachFromGroup(SyncGroupHelper sgh)
        {
            string group = sgh.Group;
            lock (this._syncGroupMap.SyncRoot)
            {
                this._syncGroupMap.Remove(group);
            }
            sgh.SetComplete();
        }

        public void DeterminePlatformRequirements()
        {
            try
            {
                if (this._actDesc == null)
                {
                    throw new InvalidOperationException(Resources.GetString("Ex_BindFirst"));
                }
                this.DeterminePlatformRequirementsCore(true);
            }
            catch (Exception)
            {
                this._platformRequirementsFailedEvent.Set();
                throw;
            }
        }

        private bool DeterminePlatformRequirementsCore(bool blocking)
        {
            try
            {
                Logger.AddMethodCall(this._log, "DeploymentManager.DeterminePlatformRequirementsCore(" + blocking.ToString() + ") called.");
                if (!blocking && this._cancellationPending)
                {
                    return true;
                }
                using (TempDirectory directory = this._subStore.AcquireTempDirectory())
                {
                    PlatformDetector.VerifyPlatformDependencies(this._actDesc.AppManifest, this._actDesc.DeployManifest, directory.Path);
                }
            }
            catch (Exception exception)
            {
                this.LogError(Resources.GetString("Ex_DeterminePlatformRequirementsFailed"), exception);
                Logger.AddInternalState(this._log, "Exception thrown in  DeterminePlatformRequirementsCore(): " + exception.GetType().ToString() + " : " + exception.Message + "\r\n" + exception.StackTrace);
                throw;
            }
            return false;
        }

        public void DetermineTrust(TrustParams trustParams)
        {
            try
            {
                if (this._actDesc == null)
                {
                    throw new InvalidOperationException(Resources.GetString("Ex_BindFirst"));
                }
                this.DetermineTrustCore(true, trustParams);
            }
            catch (Exception)
            {
                this._trustNotGrantedEvent.Set();
                throw;
            }
            this._trustGrantedEvent.Set();
        }

        private bool DetermineTrustCore(bool blocking, TrustParams tp)
        {
            try
            {
                Logger.AddMethodCall(this._log, "DeploymentManager.DetermineTrustCore() called.");
                SubscriptionState subscriptionState = this._subStore.GetSubscriptionState(this._actDesc.DeployManifest);
                TrustManagerContext tmc = new TrustManagerContext {
                    IgnorePersistedDecision = false,
                    NoPrompt = false,
                    Persist = true
                };
                if (tp != null)
                {
                    tmc.NoPrompt = tp.NoPrompt;
                }
                if (!blocking && this._cancellationPending)
                {
                    return true;
                }
                if (subscriptionState.IsInstalled && !string.Equals(subscriptionState.EffectiveCertificatePublicKeyToken, this._actDesc.EffectiveCertificatePublicKeyToken, StringComparison.Ordinal))
                {
                    Logger.AddInternalState(this._log, "Application family is installed but effective certificate public key token has changed between versions: subState.EffectiveCertificatePublicKeyToken=" + subscriptionState.EffectiveCertificatePublicKeyToken + ",_actDesc.EffectiveCertificatePublicKeyToken=" + this._actDesc.EffectiveCertificatePublicKeyToken);
                    Logger.AddInternalState(this._log, "Removing cached trust for the CurrentBind.");
                    System.Deployment.Application.ApplicationTrust.RemoveCachedTrust(subscriptionState.CurrentBind);
                }
                bool isUpdate = false;
                if (this._actDesc.IsUpdate)
                {
                    isUpdate = true;
                }
                if (this._actDesc.IsUpdateInPKTGroup)
                {
                    isUpdate = false;
                    ApplicationSecurityInfo info = new ApplicationSecurityInfo(this._actCtx);
                    this._actDesc.IsFullTrustRequested = info.DefaultRequestSet.IsUnrestricted();
                }
                this._actDesc.Trust = System.Deployment.Application.ApplicationTrust.RequestTrust(subscriptionState, this._actDesc.DeployManifest.Deployment.Install, isUpdate, this._actCtx, tmc);
            }
            catch (Exception exception)
            {
                this.LogError(Resources.GetString("Ex_DetermineTrustFailed"), exception);
                Logger.AddInternalState(this._log, "Exception thrown in  DetermineTrustCore(): " + exception.GetType().ToString() + " : " + exception.Message + "\r\n" + exception.StackTrace);
                throw;
            }
            return false;
        }

        public void Dispose()
        {
            this._events.Dispose();
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                Logger.EndLogging(this._log);
                if (this._tempDeployment != null)
                {
                    this._tempDeployment.Dispose();
                }
                if (this._tempApplicationDirectory != null)
                {
                    this._tempApplicationDirectory.Dispose();
                }
                if (this._referenceTransaction != null)
                {
                    this._referenceTransaction.Close();
                }
                if (this._actCtx != null)
                {
                    this._actCtx.Dispose();
                }
                if (this._events != null)
                {
                    this._events.Dispose();
                }
                if (this._trustNotGrantedEvent != null)
                {
                    this._trustNotGrantedEvent.Close();
                }
                if (this._trustGrantedEvent != null)
                {
                    this._trustGrantedEvent.Close();
                }
                if (this._platformRequirementsFailedEvent != null)
                {
                    this._platformRequirementsFailedEvent.Close();
                }
            }
        }

        public ObjectHandle ExecuteNewDomain()
        {
            if (this._actDesc == null)
            {
                throw new InvalidOperationException(Resources.GetString("Ex_BindFirst"));
            }
            if (!this._cached)
            {
                throw new InvalidOperationException(Resources.GetString("Ex_SyncNullFirst"));
            }
            Logger.AddInternalState(this._log, "Activating " + (((this._actCtx != null) && (this._actCtx.Identity != null)) ? this._actCtx.Identity.ToString() : "null") + " in a new domain.");
            return Activator.CreateInstance(this._actCtx);
        }

        public void ExecuteNewProcess()
        {
            if (this._actDesc == null)
            {
                throw new InvalidOperationException(Resources.GetString("Ex_BindFirst"));
            }
            if (!this._cached)
            {
                throw new InvalidOperationException(Resources.GetString("Ex_SyncNullFirst"));
            }
            this._subStore.ActivateApplication(this._actDesc.AppId, null, false);
        }

        private void LogError(string message, Exception ex)
        {
            Logger.AddErrorInformation(this._log, message, ex);
            Logger.FlushLog(this._log);
        }

        public void PersistTrustWithoutEvaluation()
        {
            try
            {
                this._actDesc.Trust = System.Deployment.Application.ApplicationTrust.PersistTrustWithoutEvaluation(this._actCtx);
            }
            catch (Exception)
            {
                this._trustNotGrantedEvent.Set();
                throw;
            }
            this._trustGrantedEvent.Set();
        }

        private void ProgressReporter(object arg)
        {
            DeploymentProgressChangedEventArgs e = (DeploymentProgressChangedEventArgs) arg;
            DeploymentProgressChangedEventHandler handler = (DeploymentProgressChangedEventHandler) this.Events[progressChangedKey];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void Synchronize()
        {
            if (this._actDesc == null)
            {
                throw new InvalidOperationException(Resources.GetString("Ex_BindFirst"));
            }
            if (Interlocked.Exchange(ref this._syncGuard, 1) != 0)
            {
                throw new InvalidOperationException(Resources.GetString("Ex_SyncNullOnce"));
            }
            this.SynchronizeCore(true);
        }

        public void Synchronize(string groupName)
        {
            if (groupName == null)
            {
                this.Synchronize();
            }
            else
            {
                bool flag;
                if (this._actDesc == null)
                {
                    throw new InvalidOperationException(Resources.GetString("Ex_BindFirst"));
                }
                if (!this._cached)
                {
                    throw new InvalidOperationException(Resources.GetString("Ex_SyncNullFirst"));
                }
                SyncGroupHelper sgh = this.AttachToGroup(groupName, null, out flag);
                if (!flag)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_SyncGroupOnce"), new object[] { groupName }));
                }
                this.SynchronizeGroupCore(true, sgh);
            }
        }

        public void SynchronizeAsync()
        {
            Logger.AddMethodCall(this._log, "DeploymentManager.SynchronizeAsync() called.");
            if (!this._cancellationPending)
            {
                if (this._actDesc == null)
                {
                    throw new InvalidOperationException(Resources.GetString("Ex_BindFirst"));
                }
                if (Interlocked.Exchange(ref this._syncGuard, 1) != 0)
                {
                    throw new InvalidOperationException(Resources.GetString("Ex_SyncNullOnce"));
                }
                this.synchronizeWorker.BeginInvoke(null, null);
            }
        }

        public void SynchronizeAsync(string groupName)
        {
            this.SynchronizeAsync(groupName, null);
        }

        public void SynchronizeAsync(string groupName, object userState)
        {
            if (groupName == null)
            {
                this.SynchronizeAsync();
            }
            else
            {
                bool flag;
                if (this._actDesc == null)
                {
                    throw new InvalidOperationException(Resources.GetString("Ex_BindFirst"));
                }
                if (!this._cached)
                {
                    throw new InvalidOperationException(Resources.GetString("Ex_SyncNullFirst"));
                }
                SyncGroupHelper state = this.AttachToGroup(groupName, userState, out flag);
                if (!flag)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_SyncGroupOnce"), new object[] { groupName }));
                }
                ThreadPool.QueueUserWorkItem(this.synchronizeGroupWorker, state);
            }
        }

        private void SynchronizeAsyncCompleted(object arg)
        {
            SynchronizeCompletedEventArgs e = (SynchronizeCompletedEventArgs) arg;
            SynchronizeCompletedEventHandler handler = (SynchronizeCompletedEventHandler) this.Events[synchronizeCompletedKey];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void SynchronizeAsyncWorker()
        {
            Exception error = null;
            bool cancelled = false;
            try
            {
                Logger.AddInternalState(this._log, "Download and install of the application started in a worker thread.");
                cancelled = this.SynchronizeCore(false);
                Logger.AddInternalState(this._log, "Installation is successful.");
            }
            catch (Exception exception2)
            {
                if (ExceptionUtility.IsHardException(exception2))
                {
                    throw;
                }
                if (exception2 is DownloadCancelledException)
                {
                    cancelled = true;
                }
                else
                {
                    error = exception2;
                }
            }
            finally
            {
                SynchronizeCompletedEventArgs arg = new SynchronizeCompletedEventArgs(error, cancelled, null, null);
                this.asyncOperation.Post(this.synchronizeCompleted, arg);
            }
        }

        private bool SynchronizeCore(bool blocking)
        {
            try
            {
                AssemblyManifest deployManifest = this._actDesc.DeployManifest;
                SubscriptionState subscriptionState = this._subStore.GetSubscriptionState(deployManifest);
                this._subStore.CheckDeploymentSubscriptionState(subscriptionState, deployManifest);
                this._subStore.CheckCustomUXFlag(subscriptionState, this._actDesc.AppManifest);
                if (this._actDesc.DeployManifestPath != null)
                {
                    this._actDesc.CommitDeploy = true;
                    this._actDesc.IsConfirmed = this._isConfirmed;
                    this._actDesc.TimeStamp = DateTime.UtcNow;
                }
                else
                {
                    this._actDesc.CommitDeploy = false;
                }
                if (!blocking && this._cancellationPending)
                {
                    return true;
                }
                if (!this._cached)
                {
                    Logger.AddInternalState(this._log, "Application is not cached.");
                    bool flag = false;
                    if (this._actDesc.appType != AppType.CustomHostSpecified)
                    {
                        if (this._actDesc.Trust != null)
                        {
                            bool flag2 = this._actDesc.Trust.DefaultGrantSet.PermissionSet.IsUnrestricted();
                            Logger.AddInternalState(this._log, "fullTrust=" + flag2.ToString());
                            if (!flag2 && (this._actDesc.AppManifest.FileAssociations.Length > 0))
                            {
                                throw new DeploymentException(ExceptionTypes.ManifestSemanticValidation, Resources.GetString("Ex_FileExtensionNotSupported"));
                            }
                            bool flag3 = !this._actDesc.DeployManifest.Deployment.Install;
                            if (!flag2 && flag3)
                            {
                                Logger.AddInternalState(this._log, "Application is semi-trust and online. Size limits will be checked during download.");
                                if (this._downloadOptions == null)
                                {
                                    this._downloadOptions = new DownloadOptions();
                                }
                                this._downloadOptions.EnforceSizeLimit = true;
                                this._downloadOptions.SizeLimit = this._subStore.GetSizeLimitInBytesForSemiTrustApps();
                                this._downloadOptions.Size = this._actDesc.DeployManifest.SizeInBytes + this._actDesc.AppManifest.SizeInBytes;
                            }
                        }
                        else
                        {
                            flag = true;
                        }
                    }
                    DownloadManager.DownloadDependencies(subscriptionState, this._actDesc.DeployManifest, this._actDesc.AppManifest, this._actDesc.AppSourceUri, this._tempApplicationDirectory.Path, null, blocking ? null : this, this._downloadOptions);
                    if (!blocking && this._cancellationPending)
                    {
                        return true;
                    }
                    this.WaitForAssertApplicationRequirements();
                    if (flag)
                    {
                        this.CheckSizeLimit();
                    }
                    this._actDesc.CommitApp = true;
                    this._actDesc.AppPayloadPath = this._tempApplicationDirectory.Path;
                }
                else
                {
                    Logger.AddInternalState(this._log, "Application is cached.");
                    this.WaitForAssertApplicationRequirements();
                }
                if (this._actDesc.CommitDeploy || this._actDesc.CommitApp)
                {
                    this._subStore.CommitApplication(ref subscriptionState, this._actDesc);
                    Logger.AddInternalState(this._log, "Application is successfully committed to the store.");
                }
                if (this._tempApplicationDirectory != null)
                {
                    this._tempApplicationDirectory.Dispose();
                    this._tempApplicationDirectory = null;
                }
                if (this._tempDeployment != null)
                {
                    this._tempDeployment.Dispose();
                    this._tempDeployment = null;
                }
                if (this._referenceTransaction != null)
                {
                    this._referenceTransaction.Close();
                    this._referenceTransaction = null;
                }
                Logger.AddInternalState(this._log, "Refreshing ActivationContext from store.");
                ActivationContext context = this._actCtx;
                this._actCtx = ConstructActivationContextFromStore(this._actDesc.AppId);
                context.Dispose();
                this._cached = true;
            }
            catch (Exception exception)
            {
                this.LogError(Resources.GetString("Ex_DownloadApplicationFailed"), exception);
                Logger.AddInternalState(this._log, "Exception thrown in  SynchronizeCore(): " + exception.GetType().ToString() + " : " + exception.Message + "\r\n" + exception.StackTrace);
                throw;
            }
            return false;
        }

        private void SynchronizeGroupAsyncWorker(object arg)
        {
            Exception error = null;
            bool cancelled = false;
            string groupName = null;
            object userState = null;
            try
            {
                SyncGroupHelper sgh = (SyncGroupHelper) arg;
                groupName = sgh.Group;
                userState = sgh.UserState;
                cancelled = this.SynchronizeGroupCore(false, sgh);
            }
            catch (Exception exception2)
            {
                if (ExceptionUtility.IsHardException(exception2))
                {
                    throw;
                }
                if (exception2 is DownloadCancelledException)
                {
                    cancelled = true;
                }
                else
                {
                    error = exception2;
                }
            }
            finally
            {
                SynchronizeCompletedEventArgs args = new SynchronizeCompletedEventArgs(error, cancelled, userState, groupName);
                this.asyncOperation.Post(this.synchronizeCompleted, args);
            }
        }

        [PermissionSet(SecurityAction.Assert, Name="FullTrust")]
        private bool SynchronizeGroupCore(bool blocking, SyncGroupHelper sgh)
        {
            TempDirectory directory = null;
            try
            {
                string group = sgh.Group;
                SubscriptionState subscriptionState = this._subStore.GetSubscriptionState(this._actDesc.DeployManifest);
                if (this._subStore.CheckGroupInstalled(subscriptionState, this._actDesc.AppId, this._actDesc.AppManifest, group))
                {
                    return false;
                }
                bool flag = AppDomain.CurrentDomain.ApplicationTrust.DefaultGrantSet.PermissionSet.IsUnrestricted();
                if (!flag && (this._actDesc.AppManifest.FileAssociations.Length > 0))
                {
                    throw new DeploymentException(ExceptionTypes.ManifestSemanticValidation, Resources.GetString("Ex_FileExtensionNotSupported"));
                }
                bool flag2 = !this._actDesc.DeployManifest.Deployment.Install;
                if (!flag && flag2)
                {
                    if (this._downloadOptions == null)
                    {
                        this._downloadOptions = new DownloadOptions();
                    }
                    this._downloadOptions.EnforceSizeLimit = true;
                    this._downloadOptions.SizeLimit = this._subStore.GetSizeLimitInBytesForSemiTrustApps();
                    this._downloadOptions.Size = this._subStore.GetPrivateSize(this._actDesc.AppId);
                }
                directory = this._subStore.AcquireTempDirectory();
                DownloadManager.DownloadDependencies(subscriptionState, this._actDesc.DeployManifest, this._actDesc.AppManifest, this._actDesc.AppSourceUri, directory.Path, group, blocking ? null : sgh, this._downloadOptions);
                if (!blocking && sgh.CancellationPending)
                {
                    return true;
                }
                CommitApplicationParams commitParams = new CommitApplicationParams(this._actDesc) {
                    CommitApp = true,
                    AppPayloadPath = directory.Path,
                    AppManifestPath = null,
                    AppGroup = group,
                    CommitDeploy = false
                };
                this._subStore.CommitApplication(ref subscriptionState, commitParams);
            }
            finally
            {
                this.DetachFromGroup(sgh);
                if (directory != null)
                {
                    directory.Dispose();
                }
            }
            return false;
        }

        void IDownloadNotification.DownloadCompleted(object sender, DownloadEventArgs e)
        {
            this._downloadedAppSize = e.BytesCompleted;
        }

        void IDownloadNotification.DownloadModified(object sender, DownloadEventArgs e)
        {
            if (this._cancellationPending)
            {
                ((FileDownloader) sender).Cancel();
            }
            this.asyncOperation.Post(this.progressReporter, new DeploymentProgressChangedEventArgs(e.Progress, null, e.BytesCompleted, e.BytesTotal, this._state, null));
        }

        private void WaitForAssertApplicationRequirements()
        {
            if ((this._actDesc.appType != AppType.CustomHostSpecified) && (this._callerType != CallerType.ApplicationDeployment))
            {
                Logger.AddInternalState(this._log, "WaitForAssertApplicationRequirements() called.");
                switch (WaitHandle.WaitAny(this._assertApplicationReqEvents, Constants.AssertApplicationRequirementsTimeout, false))
                {
                    case 0x102:
                        throw new DeploymentException(Resources.GetString("Ex_CannotCommitNoTrustDecision"));

                    case 0:
                        throw new DeploymentException(Resources.GetString("Ex_CannotCommitTrustFailed"));

                    case 1:
                        throw new DeploymentException(Resources.GetString("Ex_CannotCommitPlatformRequirementsFailed"));
                }
                Logger.AddInternalState(this._log, "WaitForAssertApplicationRequirements() returned.");
            }
        }

        internal System.Deployment.Application.ActivationDescription ActivationDescription
        {
            get
            {
                return this._actDesc;
            }
        }

        public CallerType Callertype
        {
            get
            {
                return this._callerType;
            }
            set
            {
                this._callerType = value;
            }
        }

        public bool CancellationPending
        {
            get
            {
                return this._cancellationPending;
            }
        }

        private EventHandlerList Events
        {
            get
            {
                return this._events;
            }
        }

        public string LogFilePath
        {
            get
            {
                string logFilePath = Logger.GetLogFilePath(this._log);
                if (!Logger.FlushLog(this._log))
                {
                    logFilePath = null;
                }
                return logFilePath;
            }
        }

        public Logger.LogIdentity LogId
        {
            get
            {
                return this._log;
            }
        }

        public string ShortcutAppId
        {
            get
            {
                AssemblyManifest deployManifest = this._actDesc.DeployManifest;
                SubscriptionState subscriptionState = this._subStore.GetSubscriptionState(deployManifest);
                string str = null;
                if (subscriptionState.IsInstalled)
                {
                    str = string.Format("{0}#{1}", subscriptionState.DeploymentProviderUri.AbsoluteUri, subscriptionState.SubscriptionId.ToString());
                }
                return str;
            }
        }

        public enum CallerType
        {
            Other,
            ApplicationDeployment,
            InPlaceHostingManager
        }
    }
}

