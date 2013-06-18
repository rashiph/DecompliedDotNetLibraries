namespace System.Deployment.Application
{
    using System;
    using System.Deployment.Application.Manifest;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [StructLayout(LayoutKind.Sequential), Guid("20FD4E26-8E0F-4F73-A0E0-F27B8C57BE6F"), ClassInterface(ClassInterfaceType.AutoDispatch), ComVisible(true), PermissionSet(SecurityAction.Demand, Name="FullTrust")]
    public class DeploymentServiceCom
    {
        public DeploymentServiceCom()
        {
            LifetimeManager.ExtendLifetime();
        }

        public void ActivateDeployment(string deploymentLocation, bool isShortcut)
        {
            new ApplicationActivator().ActivateDeployment(deploymentLocation, isShortcut);
        }

        public void ActivateDeploymentEx(string deploymentLocation, int unsignedPolicy, int signedPolicy)
        {
            new ApplicationActivator().ActivateDeploymentEx(deploymentLocation, unsignedPolicy, signedPolicy);
        }

        public void ActivateApplicationExtension(string textualSubId, string deploymentProviderUrl, string targetAssociatedFile)
        {
            new ApplicationActivator().ActivateApplicationExtension(textualSubId, deploymentProviderUrl, targetAssociatedFile);
        }

        public void MaintainSubscription(string textualSubId)
        {
            LifetimeManager.StartOperation();
            try
            {
                this.MaintainSubscriptionInternal(textualSubId);
            }
            finally
            {
                LifetimeManager.EndOperation();
            }
        }

        public void CheckForDeploymentUpdate(string textualSubId)
        {
            LifetimeManager.StartOperation();
            try
            {
                this.CheckForDeploymentUpdateInternal(textualSubId);
            }
            finally
            {
                LifetimeManager.EndOperation();
            }
        }

        public void EndServiceRightNow()
        {
            LifetimeManager.EndImmediately();
        }

        public void CleanOnlineAppCache()
        {
            LifetimeManager.StartOperation();
            try
            {
                this.CleanOnlineAppCacheInternal();
            }
            finally
            {
                LifetimeManager.EndOperation();
            }
        }

        private void MaintainSubscriptionInternal(string textualSubId)
        {
            bool flag = false;
            string[] strArray = new string[] { "Maintain_Exception", "Maintain_Completed", "Maintain_Failed", "Maintain_FailedMsg" };
            bool flag2 = false;
            Exception exception = null;
            bool flag3 = false;
            bool flag4 = false;
            string linkUrlMessage = Resources.GetString("ErrorMessage_GenericLinkUrlMessage");
            string linkUrl = null;
            string errorReportUrl = null;
            Logger.StartCurrentThreadLogging();
            Logger.SetTextualSubscriptionIdentity(textualSubId);
            using (UserInterface interface2 = new UserInterface())
            {
                MaintenanceInfo maintenanceInfo = new MaintenanceInfo();
                try
                {
                    UserInterfaceInfo info = new UserInterfaceInfo();
                    Logger.AddPhaseInformation(Resources.GetString("PhaseLog_StoreQueryForMaintenanceInfo"));
                    SubscriptionState subscriptionState = this.GetSubscriptionState(textualSubId);
                    try
                    {
                        subscriptionState.SubscriptionStore.CheckInstalledAndShellVisible(subscriptionState);
                        if (subscriptionState.RollbackDeployment == null)
                        {
                            maintenanceInfo.maintenanceFlags |= MaintenanceFlags.RemoveSelected;
                        }
                        else
                        {
                            maintenanceInfo.maintenanceFlags |= MaintenanceFlags.RestorationPossible;
                            maintenanceInfo.maintenanceFlags |= MaintenanceFlags.RestoreSelected;
                        }
                        AssemblyManifest currentDeploymentManifest = subscriptionState.CurrentDeploymentManifest;
                        if ((currentDeploymentManifest != null) && (currentDeploymentManifest.Description != null))
                        {
                            errorReportUrl = currentDeploymentManifest.Description.ErrorReportUrl;
                        }
                        Description effectiveDescription = subscriptionState.EffectiveDescription;
                        info.productName = effectiveDescription.Product;
                        info.supportUrl = effectiveDescription.SupportUrl;
                        info.formTitle = string.Format(CultureInfo.CurrentUICulture, Resources.GetString("UI_MaintenanceTitle"), new object[] { info.productName });
                        flag3 = true;
                    }
                    catch (DeploymentException exception2)
                    {
                        flag3 = false;
                        Logger.AddErrorInformation(Resources.GetString("MaintainLogMsg_FailedStoreLookup"), exception2);
                        maintenanceInfo.maintenanceFlags |= MaintenanceFlags.RemoveSelected;
                    }
                    catch (FormatException exception3)
                    {
                        flag3 = false;
                        Logger.AddErrorInformation(Resources.GetString("MaintainLogMsg_FailedStoreLookup"), exception3);
                        maintenanceInfo.maintenanceFlags |= MaintenanceFlags.RemoveSelected;
                    }
                    bool flag5 = false;
                    if (flag3)
                    {
                        if (interface2.ShowMaintenance(info, maintenanceInfo) == UserInterfaceModalResult.Ok)
                        {
                            flag5 = true;
                        }
                    }
                    else
                    {
                        maintenanceInfo.maintenanceFlags = MaintenanceFlags.RemoveSelected;
                        flag5 = true;
                    }
                    if (flag5)
                    {
                        flag2 = true;
                        if ((maintenanceInfo.maintenanceFlags & MaintenanceFlags.RestoreSelected) != MaintenanceFlags.ClearFlag)
                        {
                            strArray = new string[] { "Rollback_Exception", "Rollback_Completed", "Rollback_Failed", "Rollback_FailedMsg" };
                            subscriptionState.SubscriptionStore.RollbackSubscription(subscriptionState);
                            flag2 = false;
                            interface2.ShowMessage(Resources.GetString("UI_RollbackCompletedMsg"), Resources.GetString("UI_RollbackCompletedTitle"));
                        }
                        else if ((maintenanceInfo.maintenanceFlags & MaintenanceFlags.RemoveSelected) != MaintenanceFlags.ClearFlag)
                        {
                            strArray = new string[] { "Uninstall_Exception", "Uninstall_Completed", "Uninstall_Failed", "Uninstall_FailedMsg" };
                            try
                            {
                                subscriptionState.SubscriptionStore.UninstallSubscription(subscriptionState);
                                flag2 = false;
                            }
                            catch (DeploymentException exception4)
                            {
                                Logger.AddErrorInformation(Resources.GetString("MaintainLogMsg_UninstallFailed"), exception4);
                                flag4 = true;
                                ShellExposure.RemoveSubscriptionShellExposure(subscriptionState);
                                flag4 = false;
                            }
                        }
                        flag = true;
                    }
                }
                catch (DeploymentException exception5)
                {
                    Logger.AddErrorInformation(exception5, Resources.GetString(strArray[0]), new object[] { textualSubId });
                    exception = exception5;
                }
                finally
                {
                    Logger.AddPhaseInformation(Resources.GetString(flag ? strArray[1] : strArray[2]), new object[] { textualSubId });
                    if ((((maintenanceInfo.maintenanceFlags & MaintenanceFlags.RestoreSelected) != MaintenanceFlags.ClearFlag) && flag2) || ((((maintenanceInfo.maintenanceFlags & MaintenanceFlags.RemoveSelected) != MaintenanceFlags.ClearFlag) && flag4) && flag2))
                    {
                        string logFilePath = Logger.GetLogFilePath();
                        if (!Logger.FlushCurrentThreadLogs())
                        {
                            logFilePath = null;
                        }
                        if ((errorReportUrl != null) && (exception != null))
                        {
                            Exception innerMostException = this.GetInnerMostException(exception);
                            linkUrl = string.Format("{0}?outer={1}&&inner={2}&&msg={3}", new object[] { errorReportUrl, exception.GetType().ToString(), innerMostException.GetType().ToString(), innerMostException.Message });
                            if (linkUrl.Length > 0x800)
                            {
                                linkUrl = linkUrl.Substring(0, 0x800);
                            }
                        }
                        interface2.ShowError(Resources.GetString("UI_MaintenceErrorTitle"), Resources.GetString(strArray[3]), logFilePath, linkUrl, linkUrlMessage);
                    }
                    Logger.EndCurrentThreadLogging();
                }
            }
        }

        private void CheckForDeploymentUpdateInternal(string textualSubId)
        {
            bool flag = false;
            Logger.StartCurrentThreadLogging();
            Logger.SetTextualSubscriptionIdentity(textualSubId);
            try
            {
                SubscriptionState shellVisibleSubscriptionState = this.GetShellVisibleSubscriptionState(textualSubId);
                shellVisibleSubscriptionState.SubscriptionStore.CheckForDeploymentUpdate(shellVisibleSubscriptionState);
                flag = true;
            }
            catch (DeploymentException exception)
            {
                Logger.AddErrorInformation(Resources.GetString("Upd_Exception"), exception);
            }
            finally
            {
                Logger.AddPhaseInformation(Resources.GetString(flag ? "Upd_Completed" : "Upd_Failed"));
                Logger.EndCurrentThreadLogging();
            }
        }

        private void CleanOnlineAppCacheInternal()
        {
            bool flag = false;
            Logger.StartCurrentThreadLogging();
            try
            {
                SubscriptionStore.CurrentUser.CleanOnlineAppCache();
                flag = true;
            }
            catch (Exception exception)
            {
                Logger.AddErrorInformation(Resources.GetString("Ex_CleanOnlineAppCache"), exception);
                throw;
            }
            finally
            {
                Logger.AddPhaseInformation(Resources.GetString(flag ? "CleanOnlineCache_Completed" : "CleanOnlineCache_Failed"));
                Logger.EndCurrentThreadLogging();
            }
        }

        private SubscriptionState GetShellVisibleSubscriptionState(string textualSubId)
        {
            SubscriptionState subscriptionState = this.GetSubscriptionState(textualSubId);
            subscriptionState.SubscriptionStore.CheckInstalledAndShellVisible(subscriptionState);
            return subscriptionState;
        }

        private SubscriptionState GetSubscriptionState(string textualSubId)
        {
            if (textualSubId == null)
            {
                throw new ArgumentNullException("textualSubId", Resources.GetString("Ex_ComArgSubIdentityNull"));
            }
            DefinitionIdentity subId = null;
            try
            {
                subId = new DefinitionIdentity(textualSubId);
            }
            catch (COMException exception)
            {
                throw new DeploymentException(ExceptionTypes.SubscriptionState, string.Format(CultureInfo.CurrentCulture, Resources.GetString("Ex_ComArgSubIdentityNotValid"), new object[] { textualSubId }), exception);
            }
            catch (SEHException exception2)
            {
                throw new DeploymentException(ExceptionTypes.SubscriptionState, string.Format(CultureInfo.CurrentCulture, Resources.GetString("Ex_ComArgSubIdentityNotValid"), new object[] { textualSubId }), exception2);
            }
            if (subId.Version != null)
            {
                throw new DeploymentException(ExceptionTypes.SubscriptionState, Resources.GetString("Ex_ComArgSubIdentityWithVersion"));
            }
            SubscriptionStore currentUser = SubscriptionStore.CurrentUser;
            currentUser.RefreshStorePointer();
            return currentUser.GetSubscriptionState(subId);
        }

        private Exception GetInnerMostException(Exception exception)
        {
            if (exception.InnerException != null)
            {
                return this.GetInnerMostException(exception.InnerException);
            }
            return exception;
        }
    }
}

