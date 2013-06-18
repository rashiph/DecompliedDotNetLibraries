namespace System.Deployment.Application
{
    using Microsoft.Internal.Performance;
    using System;
    using System.Collections;
    using System.Deployment.Application.Manifest;
    using System.Deployment.Internal;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal class ApplicationActivator
    {
        private static Hashtable _activationsInProgress = new Hashtable();
        private bool _fullTrust;
        private const int _liveActivationLimitUINotVisible = 0;
        private static int _liveActivationLimitUIStatus = 0;
        private const int _liveActivationLimitUIVisible = 1;
        private bool _remActivationInProgressEntry;
        private SubscriptionStore _subStore;
        private UserInterface _ui;
        private const int ActivateArgumentCount = 5;

        private void Activate(DefinitionAppId appId, AssemblyManifest appManifest, string activationParameter, bool useActivationParameter)
        {
            using (ActivationContext context = ActivationContext.CreatePartialActivationContext(appId.ToApplicationIdentity()))
            {
                InternalActivationContextHelper.PrepareForExecution(context);
                this._subStore.ActivateApplication(appId, activationParameter, useActivationParameter);
            }
        }

        public void ActivateApplicationExtension(string textualSubId, string deploymentProviderUrl, string targetAssociatedFile)
        {
            LifetimeManager.StartOperation();
            bool flag = false;
            try
            {
                object[] state = new object[] { targetAssociatedFile, false, textualSubId, deploymentProviderUrl, null };
                flag = ThreadPool.QueueUserWorkItem(new WaitCallback(this.ActivateDeploymentWorker), state);
                if (!flag)
                {
                    throw new OutOfMemoryException();
                }
            }
            finally
            {
                if (!flag)
                {
                    LifetimeManager.EndOperation();
                }
            }
        }

        public void ActivateDeployment(string activationUrl, bool isShortcut)
        {
            LifetimeManager.StartOperation();
            bool flag = false;
            try
            {
                object[] state = new object[] { activationUrl, isShortcut, null, null, null };
                flag = ThreadPool.QueueUserWorkItem(new WaitCallback(this.ActivateDeploymentWorker), state);
                if (!flag)
                {
                    throw new OutOfMemoryException();
                }
            }
            finally
            {
                if (!flag)
                {
                    LifetimeManager.EndOperation();
                }
            }
        }

        public void ActivateDeploymentEx(string activationUrl, int unsignedPolicy, int signedPolicy)
        {
            LifetimeManager.StartOperation();
            bool flag = false;
            try
            {
                BrowserSettings settings = new BrowserSettings {
                    ManagedSignedFlag = BrowserSettings.GetManagedFlagValue(signedPolicy),
                    ManagedUnSignedFlag = BrowserSettings.GetManagedFlagValue(unsignedPolicy)
                };
                object[] state = new object[] { activationUrl, false, null, null, settings };
                flag = ThreadPool.QueueUserWorkItem(new WaitCallback(this.ActivateDeploymentWorker), state);
                if (!flag)
                {
                    throw new OutOfMemoryException();
                }
            }
            finally
            {
                if (!flag)
                {
                    LifetimeManager.EndOperation();
                }
            }
        }

        private void ActivateDeploymentWorker(object state)
        {
            string subscrioptionUrl = null;
            string textualSubId = null;
            string deploymentProviderUrlFromExtension = null;
            try
            {
                CodeMarker_Singleton.Instance.CodeMarker(CodeMarkerEvent.perfNewApptBegin);
                object[] args = (object[]) state;
                subscrioptionUrl = (string) args[0];
                Logger.StartCurrentThreadLogging();
                Logger.SetSubscriptionUrl(subscrioptionUrl);
                Logger.AddInternalState("Activation through dfsvc.exe started.");
                Logger.AddMethodCall("ActivateDeploymentWorker({0},{1},{2},{3},{4}) called.", args);
                bool isShortcut = (bool) args[1];
                if (args[2] != null)
                {
                    textualSubId = (string) args[2];
                }
                if (args[3] != null)
                {
                    deploymentProviderUrlFromExtension = (string) args[3];
                }
                BrowserSettings browserSettings = null;
                if (args[4] != null)
                {
                    browserSettings = (BrowserSettings) args[4];
                }
                Uri uri = null;
                string errorPageUrl = null;
                try
                {
                    int num = this.CheckActivationInProgress(subscrioptionUrl);
                    this._ui = new UserInterface(false);
                    if (!PolicyKeys.SuppressLimitOnNumberOfActivations() && (num > 8))
                    {
                        throw new DeploymentException(ExceptionTypes.ActivationLimitExceeded, Resources.GetString("Ex_TooManyLiveActivation"));
                    }
                    if (subscrioptionUrl.Length > 0x4000)
                    {
                        throw new DeploymentException(ExceptionTypes.Activation, Resources.GetString("Ex_UrlTooLong"));
                    }
                    uri = new Uri(subscrioptionUrl);
                    try
                    {
                        UriHelper.ValidateSupportedSchemeInArgument(uri, "activationUrl");
                    }
                    catch (ArgumentException exception)
                    {
                        throw new InvalidDeploymentException(ExceptionTypes.UriSchemeNotSupported, Resources.GetString("Ex_NotSupportedUriScheme"), exception);
                    }
                    Logger.AddPhaseInformation(Resources.GetString("PhaseLog_StartOfActivation"), new object[] { subscrioptionUrl });
                    this.PerformDeploymentActivation(uri, isShortcut, textualSubId, deploymentProviderUrlFromExtension, browserSettings, ref errorPageUrl);
                    Logger.AddPhaseInformation(Resources.GetString("ActivateManifestSucceeded"), new object[] { subscrioptionUrl });
                }
                catch (DependentPlatformMissingException exception2)
                {
                    Logger.AddErrorInformation(exception2, Resources.GetString("ActivateManifestException"), new object[] { subscrioptionUrl });
                    if (this._ui == null)
                    {
                        this._ui = new UserInterface();
                    }
                    if (!this._ui.SplashCancelled())
                    {
                        this.DisplayPlatformDetectionFailureUI(exception2);
                    }
                }
                catch (DownloadCancelledException exception3)
                {
                    Logger.AddErrorInformation(exception3, Resources.GetString("ActivateManifestException"), new object[] { subscrioptionUrl });
                }
                catch (TrustNotGrantedException exception4)
                {
                    Logger.AddErrorInformation(exception4, Resources.GetString("ActivateManifestException"), new object[] { subscrioptionUrl });
                }
                catch (DeploymentException exception5)
                {
                    Logger.AddErrorInformation(exception5, Resources.GetString("ActivateManifestException"), new object[] { subscrioptionUrl });
                    if (exception5.SubType != ExceptionTypes.ActivationInProgress)
                    {
                        if (this._ui == null)
                        {
                            this._ui = new UserInterface();
                        }
                        if (!this._ui.SplashCancelled())
                        {
                            if (exception5.SubType == ExceptionTypes.ActivationLimitExceeded)
                            {
                                if (Interlocked.CompareExchange(ref _liveActivationLimitUIStatus, 1, 0) == 0)
                                {
                                    this.DisplayActivationFailureReason(exception5, errorPageUrl);
                                    Interlocked.CompareExchange(ref _liveActivationLimitUIStatus, 0, 1);
                                }
                            }
                            else
                            {
                                this.DisplayActivationFailureReason(exception5, errorPageUrl);
                            }
                        }
                    }
                }
                catch (Exception exception6)
                {
                    if ((exception6 is AccessViolationException) || (exception6 is OutOfMemoryException))
                    {
                        throw;
                    }
                    if (PolicyKeys.DisableGenericExceptionHandler())
                    {
                        throw;
                    }
                    Logger.AddErrorInformation(exception6, Resources.GetString("ActivateManifestException"), new object[] { subscrioptionUrl });
                    if (this._ui == null)
                    {
                        this._ui = new UserInterface();
                    }
                    if (!this._ui.SplashCancelled())
                    {
                        this.DisplayActivationFailureReason(exception6, errorPageUrl);
                    }
                }
            }
            finally
            {
                this.RemoveActivationInProgressEntry(subscrioptionUrl);
                if (this._ui != null)
                {
                    this._ui.Dispose();
                    this._ui = null;
                }
                CodeMarker_Singleton.Instance.CodeMarker(CodeMarkerEvent.perfNewApptEnd);
                Logger.EndCurrentThreadLogging();
                LifetimeManager.EndOperation();
            }
        }

        private void ActivateUI()
        {
            if (this._ui != null)
            {
                this._ui.Activate();
            }
        }

        private int CheckActivationInProgress(string activationUrl)
        {
            lock (_activationsInProgress.SyncRoot)
            {
                if (_activationsInProgress.Contains(activationUrl))
                {
                    ((ApplicationActivator) _activationsInProgress[activationUrl]).ActivateUI();
                    this._remActivationInProgressEntry = false;
                    throw new DeploymentException(ExceptionTypes.ActivationInProgress, Resources.GetString("Ex_ActivationInProgressException"));
                }
                _activationsInProgress.Add(activationUrl, this);
                this._remActivationInProgressEntry = true;
                return _activationsInProgress.Count;
            }
        }

        private void CheckDeploymentProviderValidity(ActivationDescription actDesc, SubscriptionState subState)
        {
            if ((actDesc.DeployManifest.Deployment.Install && (actDesc.DeployManifest.Deployment.ProviderCodebaseUri == null)) && ((subState != null) && (subState.DeploymentProviderUri != null)))
            {
                Uri uri = ((subState.DeploymentProviderUri.Query != null) && (subState.DeploymentProviderUri.Query.Length > 0)) ? new Uri(subState.DeploymentProviderUri.GetLeftPart(UriPartial.Path)) : subState.DeploymentProviderUri;
                Logger.AddInternalState("Checking deployment provider validity.");
                Logger.AddInternalState("providerCodebaseUri=" + uri);
                Logger.AddInternalState("actDesc.ToAppCodebase()=" + actDesc.ToAppCodebase());
                if (!uri.Equals(actDesc.ToAppCodebase()))
                {
                    throw new DeploymentException(ExceptionTypes.DeploymentUriDifferent, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("ErrorMessage_DeploymentUriDifferent"), new object[] { actDesc.DeployManifest.Description.FilteredProduct }), new DeploymentException(ExceptionTypes.DeploymentUriDifferent, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_DeploymentUriDifferentExText"), new object[] { actDesc.DeployManifest.Description.FilteredProduct, actDesc.DeploySourceUri.AbsoluteUri, subState.DeploymentProviderUri.AbsoluteUri })));
                }
            }
        }

        private void ConsumeUpdatedDeployment(ref SubscriptionState subState, ActivationDescription actDesc)
        {
            DefinitionIdentity updateSkippedDeployment = actDesc.DeployManifest.Identity;
            Uri deploySourceUri = actDesc.DeploySourceUri;
            Logger.AddPhaseInformation(Resources.GetString("PhaseLog_ConsumeUpdatedDeployment"));
            Logger.AddInternalState("Consuming new update.");
            if (!actDesc.IsRequiredUpdate)
            {
                Logger.AddInternalState("Update is not a required update.");
                Description effectiveDescription = subState.EffectiveDescription;
                UserInterfaceInfo info = new UserInterfaceInfo {
                    formTitle = Resources.GetString("UI_UpdateTitle"),
                    productName = effectiveDescription.Product,
                    supportUrl = effectiveDescription.SupportUrl,
                    sourceSite = UserInterface.GetDisplaySite(deploySourceUri)
                };
                switch (this._ui.ShowUpdate(info))
                {
                    case UserInterfaceModalResult.Skip:
                    {
                        TimeSpan span = new TimeSpan(7, 0, 0, 0);
                        DateTime updateSkipTime = DateTime.UtcNow + span;
                        this._subStore.SetUpdateSkipTime(subState, updateSkippedDeployment, updateSkipTime);
                        Logger.AddPhaseInformation(Resources.GetString("Upd_DeployUpdateSkipping"));
                        Logger.AddInternalState("User has decided to skip the update.");
                        return;
                    }
                    case UserInterfaceModalResult.Cancel:
                        Logger.AddInternalState("Do not update now, but prompt for update on next activation.");
                        return;
                }
            }
            this.InstallApplication(ref subState, actDesc);
            Logger.AddPhaseInformation(Resources.GetString("Upd_Consumed"), new object[] { updateSkippedDeployment.ToString(), deploySourceUri });
            Logger.AddInternalState("Update consumed.");
        }

        private void DisplayActivationFailureReason(Exception exception, string errorPageUrl)
        {
            string message = Resources.GetString("ErrorMessage_GenericActivationFailure");
            string linkUrlMessage = Resources.GetString("ErrorMessage_GenericLinkUrlMessage");
            Exception innerMostException = this.GetInnerMostException(exception);
            if (exception is DeploymentDownloadException)
            {
                message = Resources.GetString("ErrorMessage_NetworkError");
                DeploymentDownloadException exception3 = (DeploymentDownloadException) exception;
                if (exception3.SubType == ExceptionTypes.SizeLimitForPartialTrustOnlineAppExceeded)
                {
                    message = Resources.GetString("ErrorMessage_SizeLimitForPartialTrustOnlineAppExceeded");
                }
                if (innerMostException is WebException)
                {
                    WebException exception4 = (WebException) innerMostException;
                    if ((exception4.Response != null) && (exception4.Response is HttpWebResponse))
                    {
                        HttpWebResponse response = (HttpWebResponse) exception4.Response;
                        if (response.StatusCode == HttpStatusCode.NotFound)
                        {
                            message = Resources.GetString("ErrorMessage_FileMissing");
                        }
                        else if (response.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            message = Resources.GetString("ErrorMessage_AuthenticationError");
                        }
                        else if (response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            message = Resources.GetString("ErrorMessage_Forbidden");
                        }
                    }
                }
                else if ((innerMostException is FileNotFoundException) || (innerMostException is DirectoryNotFoundException))
                {
                    message = Resources.GetString("ErrorMessage_FileMissing");
                }
                else if (innerMostException is UnauthorizedAccessException)
                {
                    message = Resources.GetString("ErrorMessage_AuthenticationError");
                }
                else if ((innerMostException is IOException) && !this.IsWebExceptionInExceptionStack(exception))
                {
                    message = Resources.GetString("ErrorMessage_DownloadIOError");
                }
            }
            else if (exception is InvalidDeploymentException)
            {
                InvalidDeploymentException exception5 = (InvalidDeploymentException) exception;
                if (exception5.SubType == ExceptionTypes.ManifestLoad)
                {
                    message = Resources.GetString("ErrorMessage_ManifestCannotBeLoaded");
                }
                else if (((exception5.SubType == ExceptionTypes.Manifest) || (exception5.SubType == ExceptionTypes.ManifestParse)) || (exception5.SubType == ExceptionTypes.ManifestSemanticValidation))
                {
                    message = Resources.GetString("ErrorMessage_InvalidManifest");
                }
                else if ((((exception5.SubType == ExceptionTypes.Validation) || (exception5.SubType == ExceptionTypes.HashValidation)) || ((exception5.SubType == ExceptionTypes.SignatureValidation) || (exception5.SubType == ExceptionTypes.RefDefValidation))) || (((exception5.SubType == ExceptionTypes.ClrValidation) || (exception5.SubType == ExceptionTypes.StronglyNamedAssemblyVerification)) || (((exception5.SubType == ExceptionTypes.IdentityMatchValidationForMixedModeAssembly) || (exception5.SubType == ExceptionTypes.AppFileLocationValidation)) || (exception5.SubType == ExceptionTypes.FileSizeValidation))))
                {
                    message = Resources.GetString("ErrorMessage_ValidationFailed");
                }
                else if (exception5.SubType == ExceptionTypes.UnsupportedElevetaionRequest)
                {
                    message = Resources.GetString("ErrorMessage_ManifestExecutionLevelNotSupported");
                }
            }
            else if (exception is DeploymentException)
            {
                if (((DeploymentException) exception).SubType == ExceptionTypes.ComponentStore)
                {
                    message = Resources.GetString("ErrorMessage_StoreError");
                }
                else if (((DeploymentException) exception).SubType == ExceptionTypes.ActivationLimitExceeded)
                {
                    message = Resources.GetString("ErrorMessage_ConcurrentActivationLimitExceeded");
                }
                else if (((DeploymentException) exception).SubType == ExceptionTypes.DiskIsFull)
                {
                    message = Resources.GetString("ErrorMessage_DiskIsFull");
                }
                else if (((DeploymentException) exception).SubType == ExceptionTypes.DeploymentUriDifferent)
                {
                    message = exception.Message;
                }
                else if (((DeploymentException) exception).SubType == ExceptionTypes.GroupMultipleMatch)
                {
                    message = exception.Message;
                }
                else if (((DeploymentException) exception).SubType == ExceptionTypes.TrustFailDependentPlatform)
                {
                    message = exception.Message;
                }
            }
            string logFilePath = Logger.GetLogFilePath();
            if (!Logger.FlushCurrentThreadLogs())
            {
                logFilePath = null;
            }
            string linkUrl = null;
            if (errorPageUrl != null)
            {
                linkUrl = string.Format("{0}?outer={1}&&inner={2}&&msg={3}", new object[] { errorPageUrl, exception.GetType().ToString(), innerMostException.GetType().ToString(), innerMostException.Message });
                if (linkUrl.Length > 0x800)
                {
                    linkUrl = linkUrl.Substring(0, 0x800);
                }
            }
            this._ui.ShowError(Resources.GetString("UI_ErrorTitle"), message, logFilePath, linkUrl, linkUrlMessage);
        }

        private void DisplayPlatformDetectionFailureUI(DependentPlatformMissingException ex)
        {
            Uri supportUrl = null;
            if (this._fullTrust)
            {
                supportUrl = ex.SupportUrl;
            }
            this._ui.ShowPlatform(ex.Message, supportUrl);
        }

        private bool DownloadApplication(SubscriptionState subState, ActivationDescription actDesc, long transactionId, out TempDirectory downloadTemp)
        {
            Uri uri;
            string str;
            bool flag = false;
            Logger.AddMethodCall("DownloadApplication called.");
            downloadTemp = this._subStore.AcquireTempDirectory();
            Logger.AddInternalState("Start processing application manifest.");
            AssemblyManifest appManifest = DownloadManager.DownloadApplicationManifest(actDesc.DeployManifest, downloadTemp.Path, actDesc.DeploySourceUri, out uri, out str);
            AssemblyManifest.ReValidateManifestSignatures(actDesc.DeployManifest, appManifest);
            if (appManifest.EntryPoints[0].HostInBrowser)
            {
                throw new DeploymentException(ExceptionTypes.ManifestSemanticValidation, Resources.GetString("Ex_HostInBrowserAppNotSupported"));
            }
            if (appManifest.EntryPoints[0].CustomHostSpecified)
            {
                throw new DeploymentException(ExceptionTypes.ManifestSemanticValidation, Resources.GetString("Ex_CustomHostSpecifiedAppNotSupported"));
            }
            if (appManifest.EntryPoints[0].CustomUX && (((actDesc.ActType == ActivationType.InstallViaDotApplication) || (actDesc.ActType == ActivationType.InstallViaFileAssociation)) || ((actDesc.ActType == ActivationType.InstallViaShortcut) || (actDesc.ActType == ActivationType.None))))
            {
                throw new DeploymentException(ExceptionTypes.ManifestSemanticValidation, Resources.GetString("Ex_CustomUXAppNotSupported"));
            }
            Logger.AddPhaseInformation(Resources.GetString("PhaseLog_ProcessingApplicationManifestComplete"));
            Logger.AddInternalState("Processing of application manifest has successfully completed.");
            actDesc.SetApplicationManifest(appManifest, uri, str);
            Logger.SetApplicationManifest(appManifest);
            this._subStore.CheckCustomUXFlag(subState, actDesc.AppManifest);
            actDesc.AppId = new DefinitionAppId(actDesc.ToAppCodebase(), new DefinitionIdentity[] { actDesc.DeployManifest.Identity, actDesc.AppManifest.Identity });
            Logger.AddInternalState("Start request of trust and detection of platform.");
            if (appManifest.EntryPoints[0].CustomUX)
            {
                Logger.AddInternalState("This is a CustomUX application. Calling PersistTrustWithoutEvaluation.");
                actDesc.Trust = ApplicationTrust.PersistTrustWithoutEvaluation(actDesc.ToActivationContext());
            }
            else
            {
                this._ui.Hide();
                if (this._ui.SplashCancelled())
                {
                    throw new DownloadCancelledException();
                }
                if (subState.IsInstalled && !string.Equals(subState.EffectiveCertificatePublicKeyToken, actDesc.EffectiveCertificatePublicKeyToken, StringComparison.Ordinal))
                {
                    Logger.AddInternalState("EffectiveCertificatePublicKeyToken has changed between versions: subState.EffectiveCertificatePublicKeyToken=" + subState.EffectiveCertificatePublicKeyToken + ",actDesc.EffectiveCertificatePublicKeyToken=" + actDesc.EffectiveCertificatePublicKeyToken);
                    Logger.AddInternalState("Removing the cached trust decision for CurrentBind.");
                    ApplicationTrust.RemoveCachedTrust(subState.CurrentBind);
                }
                try
                {
                    actDesc.Trust = ApplicationTrust.RequestTrust(subState, actDesc.DeployManifest.Deployment.Install, actDesc.IsUpdate, actDesc.ToActivationContext());
                }
                catch (Exception exception)
                {
                    Logger.AddErrorInformation(Resources.GetString("Ex_DetermineTrustFailed"), exception);
                    if (!(exception is TrustNotGrantedException))
                    {
                        try
                        {
                            PlatformDetector.VerifyPlatformDependencies(actDesc.AppManifest, actDesc.DeployManifest, downloadTemp.Path);
                        }
                        catch (Exception exception2)
                        {
                            if (exception2 is DependentPlatformMissingException)
                            {
                                throw new DeploymentException(ExceptionTypes.TrustFailDependentPlatform, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("ErrorMessage_TrustFailDependentPlatformMissing"), new object[] { exception2.Message }), exception);
                            }
                        }
                    }
                    throw;
                }
            }
            this._fullTrust = actDesc.Trust.DefaultGrantSet.PermissionSet.IsUnrestricted();
            Logger.AddInternalState("_fullTrust = " + this._fullTrust.ToString());
            if (!this._fullTrust && (actDesc.AppManifest.FileAssociations.Length > 0))
            {
                throw new DeploymentException(ExceptionTypes.ManifestSemanticValidation, Resources.GetString("Ex_FileExtensionNotSupported"));
            }
            PlatformDetector.VerifyPlatformDependencies(actDesc.AppManifest, actDesc.DeployManifest, downloadTemp.Path);
            Logger.AddPhaseInformation(Resources.GetString("PhaseLog_PlatformDetectAndTrustGrantComplete"));
            Logger.AddInternalState("Request of trust and detection of platform is complete.");
            Logger.AddInternalState("Start downloading  and verifying dependencies.");
            if (!this._subStore.CheckAndReferenceApplication(subState, actDesc.AppId, transactionId))
            {
                flag = true;
                Description effectiveDescription = actDesc.EffectiveDescription;
                UserInterfaceInfo info = new UserInterfaceInfo {
                    productName = effectiveDescription.Product
                };
                if (actDesc.IsUpdate)
                {
                    if (actDesc.IsRequiredUpdate)
                    {
                        info.formTitle = string.Format(CultureInfo.CurrentUICulture, Resources.GetString("UI_ProgressTitleRequiredUpdate"), new object[] { info.productName });
                    }
                    else
                    {
                        info.formTitle = string.Format(CultureInfo.CurrentUICulture, Resources.GetString("UI_ProgressTitleUpdate"), new object[] { info.productName });
                    }
                }
                else if (!actDesc.DeployManifest.Deployment.Install)
                {
                    info.formTitle = string.Format(CultureInfo.CurrentUICulture, Resources.GetString("UI_ProgressTitleDownload"), new object[] { info.productName });
                }
                else
                {
                    info.formTitle = string.Format(CultureInfo.CurrentUICulture, Resources.GetString("UI_ProgressTitleInstall"), new object[] { info.productName });
                }
                info.supportUrl = effectiveDescription.SupportUrl;
                info.sourceSite = UserInterface.GetDisplaySite(actDesc.DeploySourceUri);
                if ((appManifest.Description != null) && (appManifest.Description.IconFileFS != null))
                {
                    info.iconFilePath = Path.Combine(downloadTemp.Path, appManifest.Description.IconFileFS);
                }
                ProgressPiece notification = this._ui.ShowProgress(info);
                DownloadOptions options = null;
                bool flag2 = !actDesc.DeployManifest.Deployment.Install;
                if (!this._fullTrust && flag2)
                {
                    options = new DownloadOptions {
                        EnforceSizeLimit = true,
                        SizeLimit = this._subStore.GetSizeLimitInBytesForSemiTrustApps(),
                        Size = actDesc.DeployManifest.SizeInBytes + actDesc.AppManifest.SizeInBytes
                    };
                }
                DownloadManager.DownloadDependencies(subState, actDesc.DeployManifest, actDesc.AppManifest, actDesc.AppSourceUri, downloadTemp.Path, null, notification, options);
                Logger.AddPhaseInformation(Resources.GetString("PhaseLog_DownloadDependenciesComplete"));
                actDesc.CommitApp = true;
                actDesc.AppPayloadPath = downloadTemp.Path;
                actDesc.AppGroup = null;
            }
            return flag;
        }

        private Exception GetInnerMostException(Exception exception)
        {
            if (exception.InnerException != null)
            {
                return this.GetInnerMostException(exception.InnerException);
            }
            return exception;
        }

        private bool InstallApplication(ref SubscriptionState subState, ActivationDescription actDesc)
        {
            long num;
            bool flag = false;
            Logger.AddMethodCall("InstallApplication called.");
            Logger.AddPhaseInformation(Resources.GetString("PhaseLog_InstallApplication"));
            this._subStore.CheckDeploymentSubscriptionState(subState, actDesc.DeployManifest);
            using (this._subStore.AcquireReferenceTransaction(out num))
            {
                using (TempDirectory directory = null)
                {
                    flag = this.DownloadApplication(subState, actDesc, num, out directory);
                    actDesc.CommitDeploy = true;
                    actDesc.IsConfirmed = true;
                    actDesc.TimeStamp = DateTime.UtcNow;
                    Logger.AddPhaseInformation(Resources.GetString("PhaseLog_CommitApplication"));
                    this._subStore.CommitApplication(ref subState, actDesc);
                }
            }
            return flag;
        }

        private bool IsWebExceptionInExceptionStack(Exception exception)
        {
            if (exception == null)
            {
                return false;
            }
            return ((exception is WebException) || this.IsWebExceptionInExceptionStack(exception.InnerException));
        }

        private void PerformDeploymentActivation(Uri activationUri, bool isShortcut, string textualSubId, string deploymentProviderUrlFromExtension, BrowserSettings browserSettings, ref string errorPageUrl)
        {
            TempFile deployFile = null;
            Logger.AddMethodCall("PerformDeploymentActivation called.");
            try
            {
                ActivationDescription description;
                string shortcutFile = null;
                Uri uri = null;
                bool flag = false;
                this._subStore = SubscriptionStore.CurrentUser;
                this._subStore.RefreshStorePointer();
                Uri sourceUri = activationUri;
                bool flag2 = false;
                if (textualSubId != null)
                {
                    Logger.AddInternalState("Activating through file association.");
                    flag2 = true;
                    description = this.ProcessOrFollowExtension(activationUri, textualSubId, deploymentProviderUrlFromExtension, ref errorPageUrl, out deployFile);
                    if (description == null)
                    {
                        return;
                    }
                }
                else if (isShortcut)
                {
                    Logger.AddInternalState("Activating through shortcut.");
                    shortcutFile = activationUri.LocalPath;
                    description = this.ProcessOrFollowShortcut(shortcutFile, ref errorPageUrl, out deployFile);
                    if (description == null)
                    {
                        return;
                    }
                }
                else
                {
                    SubscriptionState state;
                    Logger.AddInternalState("Activating through deployment manifest.");
                    Logger.AddInternalState("Start processing deployment manifest.");
                    AssemblyManifest deploymentManifest = DownloadManager.DownloadDeploymentManifestBypass(this._subStore, ref sourceUri, out deployFile, out state, null, null);
                    if ((browserSettings != null) && (deployFile != null))
                    {
                        browserSettings.Validate(deployFile.Path);
                    }
                    if (deploymentManifest.Description != null)
                    {
                        errorPageUrl = deploymentManifest.Description.ErrorReportUrl;
                    }
                    description = new ActivationDescription();
                    if (state != null)
                    {
                        shortcutFile = null;
                        description.SetApplicationManifest(state.CurrentApplicationManifest, null, null);
                        description.AppId = state.CurrentBind;
                        Logger.AddInternalState("Running from the store. Bypass further downloads and verifications.");
                        flag = true;
                    }
                    else
                    {
                        shortcutFile = deployFile.Path;
                    }
                    Logger.SetDeploymentManifest(deploymentManifest);
                    Logger.AddPhaseInformation(Resources.GetString("PhaseLog_ProcessingDeploymentManifestComplete"));
                    Logger.AddInternalState("Processing of deployment manifest has successfully completed.");
                    description.SetDeploymentManifest(deploymentManifest, sourceUri, shortcutFile);
                    description.IsUpdate = false;
                    description.ActType = ActivationType.InstallViaDotApplication;
                    uri = activationUri;
                }
                if (this._ui.SplashCancelled())
                {
                    throw new DownloadCancelledException();
                }
                if (description.DeployManifest.Deployment == null)
                {
                    throw new DeploymentException(ExceptionTypes.Activation, Resources.GetString("Ex_NotDeploymentOrShortcut"));
                }
                bool flag3 = false;
                SubscriptionState subscriptionState = this._subStore.GetSubscriptionState(description.DeployManifest);
                this.CheckDeploymentProviderValidity(description, subscriptionState);
                if (!flag)
                {
                    Logger.AddInternalState("Could not find application in store. Continue with downloading application manifest.");
                    flag3 = this.InstallApplication(ref subscriptionState, description);
                    Logger.AddPhaseInformation(Resources.GetString("PhaseLog_InstallationComplete"));
                    Logger.AddInternalState("Installation of application has successfully completed.");
                }
                else
                {
                    this._subStore.SetLastCheckTimeToNow(subscriptionState);
                }
                if ((description.DeployManifest.Deployment.DisallowUrlActivation && !isShortcut) && (!activationUri.IsFile || activationUri.IsUnc))
                {
                    if (flag3)
                    {
                        this._ui.ShowMessage(Resources.GetString("Activation_DisallowUrlActivationMessageAfterInstall"), Resources.GetString("Activation_DisallowUrlActivationCaptionAfterInstall"));
                    }
                    else
                    {
                        this._ui.ShowMessage(Resources.GetString("Activation_DisallowUrlActivationMessage"), Resources.GetString("Activation_DisallowUrlActivationCaption"));
                    }
                }
                else if (flag2)
                {
                    this.Activate(description.AppId, description.AppManifest, activationUri.AbsoluteUri, true);
                }
                else if (isShortcut)
                {
                    string activationParameter = null;
                    int index = shortcutFile.IndexOf('|', 0);
                    if ((index > 0) && ((index + 1) < shortcutFile.Length))
                    {
                        activationParameter = shortcutFile.Substring(index + 1);
                    }
                    if (activationParameter == null)
                    {
                        this.Activate(description.AppId, description.AppManifest, null, false);
                    }
                    else
                    {
                        this.Activate(description.AppId, description.AppManifest, activationParameter, true);
                    }
                }
                else
                {
                    this.Activate(description.AppId, description.AppManifest, uri.AbsoluteUri, false);
                }
            }
            finally
            {
                if (deployFile != null)
                {
                    deployFile.Dispose();
                }
            }
        }

        private void PerformDeploymentUpdate(ref SubscriptionState subState, ref string errorPageUrl)
        {
            DeploymentUpdate deploymentUpdate = subState.CurrentDeploymentManifest.Deployment.DeploymentUpdate;
            bool flag = (deploymentUpdate != null) && deploymentUpdate.BeforeApplicationStartup;
            Logger.AddPhaseInformation(Resources.GetString("PhaseLog_DeploymentUpdateCheck"));
            Logger.AddMethodCall("PerformDeploymentUpdate called.");
            Logger.AddInternalState(string.Concat(new object[] { "UpdateOnStart=", flag.ToString(), ",PendingDeployment=", subState.PendingDeployment }));
            if (flag || ((subState.PendingDeployment != null) && !SkipUpdate(subState, subState.PendingDeployment)))
            {
                using (TempFile file = null)
                {
                    AssemblyManifest manifest;
                    Uri deploymentProviderUri = subState.DeploymentProviderUri;
                    try
                    {
                        Logger.AddInternalState("Start processing deployment manifest for update check : " + deploymentProviderUri);
                        manifest = DownloadManager.DownloadDeploymentManifest(this._subStore, ref deploymentProviderUri, out file);
                        Logger.AddInternalState("End processing deployment manifest.");
                        if (manifest.Description != null)
                        {
                            errorPageUrl = manifest.Description.ErrorReportUrl;
                        }
                    }
                    catch (DeploymentDownloadException exception)
                    {
                        Logger.AddErrorInformation(exception, Resources.GetString("Upd_UpdateCheckDownloadFailed"), new object[] { subState.SubscriptionId.ToString() });
                        return;
                    }
                    if (this._ui.SplashCancelled())
                    {
                        throw new DownloadCancelledException();
                    }
                    if ((!SkipUpdate(subState, manifest.Identity) && (this._subStore.CheckUpdateInManifest(subState, deploymentProviderUri, manifest, subState.CurrentDeployment.Version) != null)) && !manifest.Identity.Equals(subState.ExcludedDeployment))
                    {
                        Logger.AddInternalState("Update available in the deployment server.");
                        ActivationDescription actDesc = new ActivationDescription();
                        actDesc.SetDeploymentManifest(manifest, deploymentProviderUri, file.Path);
                        actDesc.IsUpdate = true;
                        actDesc.IsRequiredUpdate = false;
                        actDesc.ActType = ActivationType.UpdateViaShortcutOrFA;
                        if ((manifest.Deployment.MinimumRequiredVersion != null) && (manifest.Deployment.MinimumRequiredVersion.CompareTo(subState.CurrentDeployment.Version) > 0))
                        {
                            actDesc.IsRequiredUpdate = true;
                        }
                        this.CheckDeploymentProviderValidity(actDesc, subState);
                        this.ConsumeUpdatedDeployment(ref subState, actDesc);
                    }
                }
            }
        }

        private ActivationDescription ProcessOrFollowExtension(Uri associatedFile, string textualSubId, string deploymentProviderUrlFromExtension, ref string errorPageUrl, out TempFile deployFile)
        {
            deployFile = null;
            Logger.AddMethodCall(string.Concat(new object[] { "ProcessOrFollowExtension(", associatedFile, ",", textualSubId, ",", deploymentProviderUrlFromExtension, ",", errorPageUrl, ") called." }));
            DefinitionIdentity subId = new DefinitionIdentity(textualSubId);
            SubscriptionState subscriptionState = this._subStore.GetSubscriptionState(subId);
            ActivationDescription description = null;
            if (subscriptionState.IsInstalled && subscriptionState.IsShellVisible)
            {
                Logger.AddInternalState("Application family is already installed and Shell Visible.");
                this.PerformDeploymentUpdate(ref subscriptionState, ref errorPageUrl);
                this.Activate(subscriptionState.CurrentBind, subscriptionState.CurrentApplicationManifest, associatedFile.AbsoluteUri, true);
                return description;
            }
            Logger.AddInternalState("Application family is not installed or is not Shell-Visible.  Try to deploy it from the deployment provider specified in the extension : " + deploymentProviderUrlFromExtension);
            if (string.IsNullOrEmpty(deploymentProviderUrlFromExtension))
            {
                throw new DeploymentException(ExceptionTypes.Activation, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_FileAssociationNoDpUrl"), new object[] { textualSubId }));
            }
            Uri sourceUri = new Uri(deploymentProviderUrlFromExtension);
            Logger.AddInternalState("Start processing deployment manifest.");
            AssemblyManifest manifest = DownloadManager.DownloadDeploymentManifest(this._subStore, ref sourceUri, out deployFile);
            if (manifest.Description != null)
            {
                errorPageUrl = manifest.Description.ErrorReportUrl;
            }
            Logger.AddInternalState("Processing of deployment manifest has successfully completed.");
            if (!manifest.Deployment.Install)
            {
                throw new DeploymentException(ExceptionTypes.Activation, Resources.GetString("Ex_FileAssociationRefOnline"));
            }
            description = new ActivationDescription();
            description.SetDeploymentManifest(manifest, sourceUri, deployFile.Path);
            description.IsUpdate = false;
            description.ActType = ActivationType.InstallViaFileAssociation;
            return description;
        }

        private ActivationDescription ProcessOrFollowShortcut(string shortcutFile, ref string errorPageUrl, out TempFile deployFile)
        {
            DefinitionIdentity identity;
            Uri uri;
            deployFile = null;
            Logger.AddMethodCall("ProcessOrFollowShortcut(shortcutFile=" + shortcutFile + ",errorPageUrl=" + errorPageUrl + ") called.");
            string str = shortcutFile;
            string activationParameter = null;
            int index = shortcutFile.IndexOf('|', 0);
            if (index > 0)
            {
                str = shortcutFile.Substring(0, index);
                if ((index + 1) < shortcutFile.Length)
                {
                    activationParameter = shortcutFile.Substring(index + 1);
                }
            }
            Logger.AddInternalState("shortcutParameter=" + activationParameter);
            ShellExposure.ParseAppShortcut(str, out identity, out uri);
            SubscriptionState subscriptionState = this._subStore.GetSubscriptionState(identity);
            ActivationDescription description = null;
            if (subscriptionState.IsInstalled && subscriptionState.IsShellVisible)
            {
                Logger.AddInternalState("Application family is already installed and Shell Visible.");
                this.PerformDeploymentUpdate(ref subscriptionState, ref errorPageUrl);
                if (activationParameter == null)
                {
                    this.Activate(subscriptionState.CurrentBind, subscriptionState.CurrentApplicationManifest, null, false);
                    return description;
                }
                this.Activate(subscriptionState.CurrentBind, subscriptionState.CurrentApplicationManifest, activationParameter, true);
                return description;
            }
            Uri sourceUri = uri;
            Logger.AddInternalState("Application family is not installed or is not Shell-Visible.  Try to deploy it from the deployment provider specified in the shortcut : " + sourceUri);
            Logger.AddInternalState("Start processing deployment manifest.");
            AssemblyManifest manifest = DownloadManager.DownloadDeploymentManifest(this._subStore, ref sourceUri, out deployFile);
            Logger.AddInternalState("Processing of deployment manifest has successfully completed.");
            if (manifest.Description != null)
            {
                errorPageUrl = manifest.Description.ErrorReportUrl;
            }
            if (!manifest.Deployment.Install)
            {
                throw new DeploymentException(ExceptionTypes.Activation, Resources.GetString("Ex_ShortcutRefOnlineOnly"));
            }
            description = new ActivationDescription();
            description.SetDeploymentManifest(manifest, sourceUri, deployFile.Path);
            description.IsUpdate = false;
            description.ActType = ActivationType.InstallViaShortcut;
            return description;
        }

        private void RemoveActivationInProgressEntry(string activationUrl)
        {
            if (this._remActivationInProgressEntry && (activationUrl != null))
            {
                lock (_activationsInProgress.SyncRoot)
                {
                    _activationsInProgress.Remove(activationUrl);
                }
            }
        }

        private static bool SkipUpdate(SubscriptionState subState, DefinitionIdentity targetIdentity)
        {
            Logger.AddMethodCall("SkipUpdate called.");
            if (((subState.UpdateSkippedDeployment != null) && (targetIdentity != null)) && (subState.UpdateSkippedDeployment.Equals(targetIdentity) && (subState.UpdateSkipTime > DateTime.UtcNow)))
            {
                Logger.AddInternalState("Skipped Update. UpdateSkipTime was " + subState.UpdateSkipTime);
                return true;
            }
            Logger.AddInternalState("Update is not skipped.");
            return false;
        }

        private class BrowserSettings
        {
            public ManagedFlags ManagedSignedFlag = ManagedFlags.URLPOLICY_DISALLOW;
            public ManagedFlags ManagedUnSignedFlag = ManagedFlags.URLPOLICY_DISALLOW;

            public static ManagedFlags GetManagedFlagValue(int policyValue)
            {
                switch (policyValue)
                {
                    case 0:
                        return ManagedFlags.URLPOLICY_ALLOW;

                    case 1:
                        return ManagedFlags.URLPOLICY_QUERY;

                    case 3:
                        return ManagedFlags.URLPOLICY_DISALLOW;
                }
                return ManagedFlags.URLPOLICY_DISALLOW;
            }

            public void Validate(string manifestPath)
            {
                Logger.AddMethodCall("BrowserSettings.Validate(" + manifestPath + ") called.");
                switch (AssemblyManifest.AnalyzeManifestCertificate(manifestPath))
                {
                    case AssemblyManifest.CertificateStatus.TrustedPublisher:
                    case AssemblyManifest.CertificateStatus.AuthenticodedNotInTrustedList:
                        if ((this.ManagedSignedFlag != ManagedFlags.URLPOLICY_ALLOW) && (this.ManagedSignedFlag != ManagedFlags.URLPOLICY_QUERY))
                        {
                            throw new InvalidDeploymentException(ExceptionTypes.Manifest, Resources.GetString("Ex_SignedManifestDisallow"));
                        }
                        break;

                    default:
                        if ((this.ManagedUnSignedFlag != ManagedFlags.URLPOLICY_ALLOW) && (this.ManagedUnSignedFlag != ManagedFlags.URLPOLICY_QUERY))
                        {
                            throw new InvalidDeploymentException(ExceptionTypes.Manifest, Resources.GetString("Ex_UnSignedManifestDisallow"));
                        }
                        break;
                }
                Logger.AddInternalState(string.Concat(new object[] { "Browser settings allow activation. ManagedSignedFlag=", this.ManagedSignedFlag, ",ManagedUnSignedFlag=", this.ManagedUnSignedFlag }));
            }

            public enum ManagedFlags
            {
                URLPOLICY_ALLOW = 0,
                URLPOLICY_DISALLOW = 3,
                URLPOLICY_QUERY = 1
            }
        }
    }
}

