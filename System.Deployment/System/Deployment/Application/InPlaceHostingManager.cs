namespace System.Deployment.Application
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Security.Permissions;
    using System.Threading;

    [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
    public class InPlaceHostingManager : IDisposable
    {
        private AppType _appType;
        private DeploymentManager _deploymentManager;
        private bool _isCached;
        private bool _isLaunchInHostProcess;
        private object _lock;
        private Logger.LogIdentity _log;
        private State _state;

        public event EventHandler<DownloadApplicationCompletedEventArgs> DownloadApplicationCompleted;

        public event EventHandler<DownloadProgressChangedEventArgs> DownloadProgressChanged;

        public event EventHandler<GetManifestCompletedEventArgs> GetManifestCompleted;

        public InPlaceHostingManager(Uri deploymentManifest) : this(deploymentManifest, true)
        {
        }

        public InPlaceHostingManager(Uri deploymentManifest, bool launchInHostProcess)
        {
            if (!PlatformSpecific.OnXPOrAbove)
            {
                throw new PlatformNotSupportedException(Resources.GetString("Ex_RequiresXPOrHigher"));
            }
            if (deploymentManifest == null)
            {
                throw new ArgumentNullException("deploymentManifest");
            }
            UriHelper.ValidateSupportedSchemeInArgument(deploymentManifest, "deploymentSource");
            this._deploymentManager = new DeploymentManager(deploymentManifest, false, true, null, null);
            this._log = this._deploymentManager.LogId;
            this._isLaunchInHostProcess = launchInHostProcess;
            this._Initialize();
            Logger.AddInternalState(this._log, "Activation through IPHM APIs started.");
            Logger.AddMethodCall(this._log, string.Concat(new object[] { "InPlaceHostingManager(", deploymentManifest, ",", launchInHostProcess.ToString(), ") called." }));
        }

        private void _Initialize()
        {
            this._lock = new object();
            this._deploymentManager.BindCompleted += new BindCompletedEventHandler(this.OnBindCompleted);
            this._deploymentManager.SynchronizeCompleted += new SynchronizeCompletedEventHandler(this.OnSynchronizeCompleted);
            this._deploymentManager.ProgressChanged += new DeploymentProgressChangedEventHandler(this.OnProgressChanged);
            this._state = State.Ready;
        }

        public void AssertApplicationRequirements()
        {
            lock (this._lock)
            {
                try
                {
                    Logger.AddMethodCall(this._log, "InPlaceHostingManager.AssertApplicationRequirements() called.");
                    if (this._appType == AppType.CustomHostSpecified)
                    {
                        throw new InvalidOperationException(Resources.GetString("Ex_CannotCallAssertApplicationRequirements"));
                    }
                    this.AssertApplicationRequirements(false);
                }
                catch (Exception exception)
                {
                    Logger.AddInternalState(this._log, "Exception thrown in AssertApplicationRequirements(): " + exception.GetType().ToString() + " : " + exception.Message + "\r\n" + exception.StackTrace);
                    throw;
                }
            }
        }

        public void AssertApplicationRequirements(bool grantApplicationTrust)
        {
            lock (this._lock)
            {
                try
                {
                    Logger.AddMethodCall(this._log, "InPlaceHostingManager.AssertApplicationRequirements(" + grantApplicationTrust.ToString() + ") called.");
                    if (this._appType == AppType.CustomHostSpecified)
                    {
                        throw new InvalidOperationException(Resources.GetString("Ex_CannotCallAssertApplicationRequirements"));
                    }
                    this.AssertState(State.GetManifestSucceeded, State.DownloadingApplication);
                    this.ChangeState(State.VerifyingRequirements);
                    this._deploymentManager.DeterminePlatformRequirements();
                    if (grantApplicationTrust)
                    {
                        Logger.AddMethodCall(this._log, "Persisting trust without evaluation.");
                        this._deploymentManager.PersistTrustWithoutEvaluation();
                    }
                    else
                    {
                        TrustParams trustParams = new TrustParams {
                            NoPrompt = true
                        };
                        this._deploymentManager.DetermineTrust(trustParams);
                    }
                    this.ChangeState(State.VerifyRequirementsSucceeded);
                }
                catch (Exception exception)
                {
                    Logger.AddInternalState(this._log, "Exception thrown in AssertApplicationRequirements(bool grantApplicationTrust): " + exception.GetType().ToString() + " : " + exception.Message + "\r\n" + exception.StackTrace);
                    this.ChangeState(State.Done);
                    throw;
                }
            }
        }

        private void AssertState(State validState)
        {
            if (this._state == State.Done)
            {
                throw new InvalidOperationException(Resources.GetString("Ex_NoFurtherOperations"));
            }
            if (validState != this._state)
            {
                throw new InvalidOperationException(Resources.GetString("Ex_InvalidSequence"));
            }
        }

        private void AssertState(State validState0, State validState1)
        {
            if (((this._state == State.Done) && (validState0 != this._state)) && (validState1 != this._state))
            {
                throw new InvalidOperationException(Resources.GetString("Ex_NoFurtherOperations"));
            }
            if ((validState0 != this._state) && (validState1 != this._state))
            {
                throw new InvalidOperationException(Resources.GetString("Ex_InvalidSequence"));
            }
        }

        private void AssertState(State validState0, State validState1, State validState2)
        {
            if (((this._state == State.Done) && (validState0 != this._state)) && ((validState1 != this._state) && (validState2 != this._state)))
            {
                throw new InvalidOperationException(Resources.GetString("Ex_NoFurtherOperations"));
            }
            if (((validState0 != this._state) && (validState1 != this._state)) && (validState2 != this._state))
            {
                throw new InvalidOperationException(Resources.GetString("Ex_InvalidSequence"));
            }
        }

        public void CancelAsync()
        {
            lock (this._lock)
            {
                try
                {
                    Logger.AddMethodCall(this._log, "InPlaceHostingManager.CancelAsync() called.");
                    this.ChangeState(State.Done);
                    this._deploymentManager.CancelAsync();
                }
                catch (Exception exception)
                {
                    Logger.AddInternalState(this._log, "Exception thrown in CancelAsync(): " + exception.GetType().ToString() + " : " + exception.Message + "\r\n" + exception.StackTrace);
                    throw;
                }
            }
        }

        private void ChangeState(State nextState)
        {
            this._state = nextState;
            Logger.AddInternalState(this._log, "Internal state=" + this._state);
        }

        private void ChangeState(State nextState, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled || (e.Error != null))
            {
                this._state = State.Done;
            }
            else
            {
                this._state = nextState;
            }
        }

        public void Dispose()
        {
            lock (this._lock)
            {
                try
                {
                    Logger.AddMethodCall(this._log, "InPlaceHostingManager.Dispose() called.");
                    this.ChangeState(State.Done);
                    this._deploymentManager.BindCompleted -= new BindCompletedEventHandler(this.OnBindCompleted);
                    this._deploymentManager.SynchronizeCompleted -= new SynchronizeCompletedEventHandler(this.OnSynchronizeCompleted);
                    this._deploymentManager.ProgressChanged -= new DeploymentProgressChangedEventHandler(this.OnProgressChanged);
                    this._deploymentManager.Dispose();
                    GC.SuppressFinalize(this);
                }
                catch (Exception exception)
                {
                    Logger.AddInternalState(this._log, "Exception thrown in Dispose(): " + exception.GetType().ToString() + " : " + exception.Message + "\r\n" + exception.StackTrace);
                    throw;
                }
            }
        }

        public void DownloadApplicationAsync()
        {
            lock (this._lock)
            {
                try
                {
                    Logger.AddMethodCall(this._log, "InPlaceHostingManager.DownloadApplicationAsync() called.");
                    if (this._appType == AppType.CustomHostSpecified)
                    {
                        this.AssertState(State.GetManifestSucceeded);
                    }
                    else if (this._isCached)
                    {
                        this.AssertState(State.GetManifestSucceeded, State.VerifyRequirementsSucceeded);
                    }
                    else
                    {
                        this.AssertState(State.GetManifestSucceeded, State.VerifyRequirementsSucceeded);
                    }
                    this.ChangeState(State.DownloadingApplication);
                    this._deploymentManager.SynchronizeAsync();
                }
                catch (Exception exception)
                {
                    Logger.AddInternalState(this._log, "Exception thrown DownloadApplicationAsync(): " + exception.GetType().ToString() + " : " + exception.Message + "\r\n" + exception.StackTrace);
                    this.ChangeState(State.Done);
                    throw;
                }
            }
        }

        public ObjectHandle Execute()
        {
            ObjectHandle handle;
            lock (this._lock)
            {
                try
                {
                    Logger.AddMethodCall(this._log, "InPlaceHostingManager.Execute() called.");
                    this.AssertState(State.DownloadApplicationSucceeded);
                    this.ChangeState(State.Done);
                    handle = this._deploymentManager.ExecuteNewDomain();
                }
                catch (Exception exception)
                {
                    Logger.AddInternalState(this._log, "Exception thrown in Execute(): " + exception.GetType().ToString() + " : " + exception.Message + "\r\n" + exception.StackTrace);
                    throw;
                }
            }
            return handle;
        }

        public void GetManifestAsync()
        {
            lock (this._lock)
            {
                try
                {
                    this.AssertState(State.Ready);
                    Logger.AddMethodCall(this._log, "InPlaceHostingManager.GetManifestAsync() called.");
                    this.ChangeState(State.GettingManifest);
                    this._deploymentManager.BindAsync();
                }
                catch (Exception exception)
                {
                    Logger.AddInternalState(this._log, "Exception thrown in  GetManifestAsync(): " + exception.GetType().ToString() + " : " + exception.Message + "\r\n" + exception.StackTrace);
                    this.ChangeState(State.Done);
                    throw;
                }
            }
        }

        private static DefinitionIdentity GetSubIdAndValidate(string subscriptionId)
        {
            if (subscriptionId == null)
            {
                throw new ArgumentNullException("subscriptionId", Resources.GetString("Ex_ComArgSubIdentityNull"));
            }
            DefinitionIdentity identity = null;
            try
            {
                identity = new DefinitionIdentity(subscriptionId);
            }
            catch (COMException exception)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.GetString("Ex_ComArgSubIdentityNotValid"), new object[] { subscriptionId }), exception);
            }
            catch (SEHException exception2)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.GetString("Ex_ComArgSubIdentityNotValid"), new object[] { subscriptionId }), exception2);
            }
            catch (ArgumentException exception3)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.GetString("Ex_ComArgSubIdentityNotValid"), new object[] { subscriptionId }), exception3);
            }
            if (identity.Name == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.GetString("Ex_ComArgSubIdentityNotValid"), new object[] { subscriptionId }));
            }
            if (identity.PublicKeyToken == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.GetString("Ex_ComArgSubIdentityNotValid"), new object[] { subscriptionId }));
            }
            if (identity.ProcessorArchitecture == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.GetString("Ex_ComArgSubIdentityNotValid"), new object[] { subscriptionId }));
            }
            if (identity.Version != null)
            {
                throw new ArgumentException(Resources.GetString("Ex_ComArgSubIdentityWithVersion"));
            }
            return identity;
        }

        private void OnBindCompleted(object sender, BindCompletedEventArgs e)
        {
            lock (this._lock)
            {
                GetManifestCompletedEventArgs args = null;
                try
                {
                    this.AssertState(State.GettingManifest, State.Done);
                    if (this._state != State.Done)
                    {
                        if (e.Cancelled || (e.Error != null))
                        {
                            this.ChangeState(State.Done);
                        }
                        else
                        {
                            this.ChangeState(State.GetManifestSucceeded, e);
                        }
                    }
                    if (this.GetManifestCompleted == null)
                    {
                        goto Label_0311;
                    }
                    if ((e.Error != null) || e.Cancelled)
                    {
                        if (e.Cancelled)
                        {
                            Logger.AddInternalState(this._log, "GetManifestAsync call cancelled.");
                        }
                        args = new GetManifestCompletedEventArgs(e, this._deploymentManager.LogFilePath);
                    }
                    else
                    {
                        this._isCached = e.IsCached;
                        bool install = this._deploymentManager.ActivationDescription.DeployManifest.Deployment.Install;
                        bool hostInBrowser = this._deploymentManager.ActivationDescription.AppManifest.EntryPoints[0].HostInBrowser;
                        this._appType = this._deploymentManager.ActivationDescription.appType;
                        bool useManifestForTrust = this._deploymentManager.ActivationDescription.AppManifest.UseManifestForTrust;
                        Uri providerCodebaseUri = this._deploymentManager.ActivationDescription.DeployManifest.Deployment.ProviderCodebaseUri;
                        if ((this._isLaunchInHostProcess && (this._appType != AppType.CustomHostSpecified)) && !hostInBrowser)
                        {
                            args = new GetManifestCompletedEventArgs(e, new InvalidOperationException(Resources.GetString("Ex_HostInBrowserFlagMustBeTrue")), this._deploymentManager.LogFilePath);
                        }
                        else if (install && (this._isLaunchInHostProcess || (this._appType == AppType.CustomHostSpecified)))
                        {
                            args = new GetManifestCompletedEventArgs(e, new InvalidOperationException(Resources.GetString("Ex_InstallFlagMustBeFalse")), this._deploymentManager.LogFilePath);
                        }
                        else if (useManifestForTrust && (this._appType == AppType.CustomHostSpecified))
                        {
                            args = new GetManifestCompletedEventArgs(e, new InvalidOperationException(Resources.GetString("Ex_CannotHaveUseManifestForTrustFlag")), this._deploymentManager.LogFilePath);
                        }
                        else if ((providerCodebaseUri != null) && (this._appType == AppType.CustomHostSpecified))
                        {
                            args = new GetManifestCompletedEventArgs(e, new InvalidOperationException(Resources.GetString("Ex_CannotHaveDeploymentProvider")), this._deploymentManager.LogFilePath);
                        }
                        else if (hostInBrowser && (this._appType == AppType.CustomUX))
                        {
                            args = new GetManifestCompletedEventArgs(e, new InvalidOperationException(Resources.GetString("Ex_CannotHaveCustomUXFlag")), this._deploymentManager.LogFilePath);
                        }
                        else
                        {
                            args = new GetManifestCompletedEventArgs(e, this._deploymentManager.ActivationDescription, this._deploymentManager.LogFilePath, this._log);
                        }
                        if (args.Error != null)
                        {
                            Logger.AddInternalState(this._log, "Exception thrown after binding: " + args.Error.GetType().ToString() + " : " + args.Error.Message + "\r\n" + args.Error.StackTrace);
                        }
                    }
                }
                catch (Exception exception)
                {
                    Logger.AddInternalState(this._log, "Exception thrown:" + exception.GetType().ToString() + " : " + exception.Message);
                    this.ChangeState(State.Done);
                    throw;
                }
                this.GetManifestCompleted(this, args);
            Label_0311:;
            }
        }

        private void OnProgressChanged(object sender, DeploymentProgressChangedEventArgs e)
        {
            lock (this._lock)
            {
                if (this.DownloadProgressChanged != null)
                {
                    DownloadProgressChangedEventArgs args = new DownloadProgressChangedEventArgs(e.ProgressPercentage, e.UserState, e.BytesCompleted, e.BytesTotal, e.State);
                    this.DownloadProgressChanged(this, args);
                }
            }
        }

        private void OnSynchronizeCompleted(object sender, SynchronizeCompletedEventArgs e)
        {
            lock (this._lock)
            {
                try
                {
                    this.AssertState(State.DownloadingApplication, State.VerifyRequirementsSucceeded, State.Done);
                    if (this._state != State.Done)
                    {
                        if (e.Cancelled || (e.Error != null))
                        {
                            this.ChangeState(State.Done);
                        }
                        else
                        {
                            this.ChangeState(State.DownloadApplicationSucceeded, e);
                        }
                    }
                    if ((!this._isLaunchInHostProcess || (this._appType == AppType.CustomHostSpecified)) && (this._appType != AppType.CustomUX))
                    {
                        this.ChangeState(State.Done);
                    }
                    if (this.DownloadApplicationCompleted != null)
                    {
                        DownloadApplicationCompletedEventArgs args = new DownloadApplicationCompletedEventArgs(e, this._deploymentManager.LogFilePath, this._deploymentManager.ShortcutAppId);
                        this.DownloadApplicationCompleted(this, args);
                    }
                }
                catch (Exception exception)
                {
                    Logger.AddInternalState(this._log, "Exception thrown:" + exception.GetType().ToString() + " : " + exception.Message);
                    throw;
                }
            }
        }

        public static void UninstallCustomAddIn(string subscriptionId)
        {
            DefinitionIdentity subId = null;
            subId = GetSubIdAndValidate(subscriptionId);
            SubscriptionStore currentUser = SubscriptionStore.CurrentUser;
            currentUser.RefreshStorePointer();
            SubscriptionState subscriptionState = currentUser.GetSubscriptionState(subId);
            subscriptionState.SubscriptionStore.UninstallCustomHostSpecifiedSubscription(subscriptionState);
        }

        public static void UninstallCustomUXApplication(string subscriptionId)
        {
            DefinitionIdentity subId = null;
            subId = GetSubIdAndValidate(subscriptionId);
            SubscriptionStore currentUser = SubscriptionStore.CurrentUser;
            currentUser.RefreshStorePointer();
            SubscriptionState subscriptionState = currentUser.GetSubscriptionState(subId);
            subscriptionState.SubscriptionStore.UninstallCustomUXSubscription(subscriptionState);
        }

        private enum State
        {
            Ready,
            GettingManifest,
            GetManifestSucceeded,
            VerifyingRequirements,
            VerifyRequirementsSucceeded,
            DownloadingApplication,
            DownloadApplicationSucceeded,
            Done
        }
    }
}

