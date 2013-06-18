namespace System.Deployment.Application
{
    using Microsoft.Win32;
    using System;
    using System.Deployment.Application.Manifest;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Policy;

    internal static class DownloadManager
    {
        private static void AddDependencies(FileDownloader downloader, AssemblyManifest deployManifest, AssemblyManifest appManifest, Uri sourceUriBase, string targetDirectory, string group)
        {
            Uri uri;
            long total = 0L;
            System.Deployment.Application.Manifest.File[] filesInGroup = appManifest.GetFilesInGroup(group, true);
            ReorderFilesForIconFile(appManifest, filesInGroup);
            foreach (System.Deployment.Application.Manifest.File file in filesInGroup)
            {
                uri = MapFileSourceUri(deployManifest, sourceUriBase, file.Name);
                AddFileToDownloader(downloader, deployManifest, appManifest, file, uri, targetDirectory, file.NameFS, file.HashCollection);
                total += (long) file.Size;
            }
            DependentAssembly[] privateAssembliesInGroup = appManifest.GetPrivateAssembliesInGroup(group, true);
            foreach (DependentAssembly assembly in privateAssembliesInGroup)
            {
                uri = MapFileSourceUri(deployManifest, sourceUriBase, assembly.Codebase);
                AddFileToDownloader(downloader, deployManifest, appManifest, assembly, uri, targetDirectory, assembly.CodebaseFS, assembly.HashCollection);
                total += (long) assembly.Size;
            }
            downloader.SetExpectedBytesTotal(total);
            if ((filesInGroup.Length == 0) && (privateAssembliesInGroup.Length == 0))
            {
                throw new InvalidDeploymentException(string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_NoSuchDownloadGroup"), new object[] { group }));
            }
        }

        private static void AddFileToDownloader(FileDownloader downloader, AssemblyManifest deployManifest, AssemblyManifest appManifest, object manifestElement, Uri fileSourceUri, string targetDirectory, string targetFileName, HashCollection hashCollection)
        {
            string targetFilePath = Path.Combine(targetDirectory, targetFileName);
            DependencyDownloadCookie cookie = new DependencyDownloadCookie(manifestElement, deployManifest, appManifest);
            downloader.AddFile(fileSourceUri, targetFilePath, cookie, hashCollection);
        }

        public static AssemblyManifest DownloadApplicationManifest(AssemblyManifest deploymentManifest, string targetDir, Uri deploymentUri, out Uri appSourceUri, out string appManifestPath)
        {
            return DownloadApplicationManifest(deploymentManifest, targetDir, deploymentUri, null, null, out appSourceUri, out appManifestPath);
        }

        public static AssemblyManifest DownloadApplicationManifest(AssemblyManifest deploymentManifest, string targetDir, Uri deploymentUri, IDownloadNotification notification, DownloadOptions options, out Uri appSourceUri, out string appManifestPath)
        {
            ServerInformation information;
            Logger.AddMethodCall("DownloadApplicationManifest called.");
            DependentAssembly mainDependentAssembly = deploymentManifest.MainDependentAssembly;
            if ((mainDependentAssembly == null) || (mainDependentAssembly.Codebase == null))
            {
                throw new InvalidDeploymentException(ExceptionTypes.ManifestSemanticValidation, Resources.GetString("Ex_NoAppInDeploymentManifest"));
            }
            appSourceUri = new Uri(deploymentUri, mainDependentAssembly.Codebase);
            Zone zone = Zone.CreateFromUrl(deploymentUri.AbsoluteUri);
            Zone zone2 = Zone.CreateFromUrl(appSourceUri.AbsoluteUri);
            if (!zone.Equals(zone2))
            {
                Logger.AddInternalState(string.Concat(new object[] { "Deployment and application does not have matching security zones. deploymentZone=", zone, ",applicationZone=", zone2 }));
                throw new InvalidDeploymentException(ExceptionTypes.Zone, Resources.GetString("Ex_DeployAppZoneMismatch"));
            }
            appManifestPath = Path.Combine(targetDir, mainDependentAssembly.Identity.Name + ".manifest");
            AssemblyManifest manifest = DownloadManifest(ref appSourceUri, appManifestPath, notification, options, AssemblyManifest.ManifestType.Application, out information);
            Logger.SetApplicationUrl(appSourceUri);
            Logger.SetApplicationServerInformation(information);
            zone2 = Zone.CreateFromUrl(appSourceUri.AbsoluteUri);
            if (!zone.Equals(zone2))
            {
                Logger.AddInternalState(string.Concat(new object[] { "Deployment and application does not have matching security zones. deploymentZone=", zone, ",applicationZone=", zone2 }));
                throw new InvalidDeploymentException(ExceptionTypes.Zone, Resources.GetString("Ex_DeployAppZoneMismatch"));
            }
            if (manifest.Identity.Equals(deploymentManifest.Identity))
            {
                throw new InvalidDeploymentException(ExceptionTypes.ManifestSemanticValidation, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_DepSameDeploymentAndApplicationIdentity"), new object[] { manifest.Identity.ToString() }));
            }
            if (!manifest.Identity.Matches(mainDependentAssembly.Identity, manifest.Application))
            {
                throw new InvalidDeploymentException(ExceptionTypes.SubscriptionSemanticValidation, Resources.GetString("Ex_RefDefMismatch"));
            }
            if (!PolicyKeys.SkipApplicationDependencyHashCheck())
            {
                try
                {
                    ComponentVerifier.VerifyFileHash(appManifestPath, mainDependentAssembly.HashCollection);
                }
                catch (InvalidDeploymentException exception)
                {
                    if (exception.SubType == ExceptionTypes.HashValidation)
                    {
                        throw new InvalidDeploymentException(ExceptionTypes.HashValidation, Resources.GetString("Ex_AppManInvalidHash"), exception);
                    }
                    throw;
                }
            }
            if (manifest.RequestedExecutionLevel != null)
            {
                Logger.AddInternalState("Application manifest has RequestedExecutionLevel specified. Check requested privileges.");
                VerifyRequestedPrivilegesSupport(manifest.RequestedExecutionLevel);
            }
            return manifest;
        }

        public static void DownloadDependencies(SubscriptionState subState, AssemblyManifest deployManifest, AssemblyManifest appManifest, Uri sourceUriBase, string targetDirectory, string group, IDownloadNotification notification, DownloadOptions options)
        {
            Logger.AddMethodCall("DownloadDependencies called.");
            Logger.AddInternalState("sourceUriBase=" + sourceUriBase);
            Logger.AddInternalState("targetDirectory=" + targetDirectory);
            Logger.AddInternalState("group=" + group);
            Logger.AddInternalState("DownloadOptions=" + options);
            FileDownloader downloader = FileDownloader.Create();
            downloader.Options = options;
            if (group == null)
            {
                downloader.CheckForSizeLimit(appManifest.CalculateDependenciesSize(), false);
            }
            AddDependencies(downloader, deployManifest, appManifest, sourceUriBase, targetDirectory, group);
            downloader.DownloadModified += new FileDownloader.DownloadModifiedEventHandler(DownloadManager.ProcessDownloadedFile);
            if (notification != null)
            {
                downloader.AddNotification(notification);
            }
            try
            {
                downloader.Download(subState);
                downloader.ComponentVerifier.VerifyComponents();
                VerifyRequestedPrivilegesSupport(appManifest, targetDirectory);
            }
            finally
            {
                if (notification != null)
                {
                    downloader.RemoveNotification(notification);
                }
                downloader.DownloadModified -= new FileDownloader.DownloadModifiedEventHandler(DownloadManager.ProcessDownloadedFile);
            }
        }

        public static AssemblyManifest DownloadDeploymentManifest(SubscriptionStore subStore, ref Uri sourceUri, out TempFile tempFile)
        {
            return DownloadDeploymentManifest(subStore, ref sourceUri, out tempFile, null, null);
        }

        public static AssemblyManifest DownloadDeploymentManifest(SubscriptionStore subStore, ref Uri sourceUri, out TempFile tempFile, IDownloadNotification notification, DownloadOptions options)
        {
            AssemblyManifest manifest;
            Logger.AddMethodCall("DownloadDeploymentManifest called.");
            Logger.AddInternalState("SourceUri=" + sourceUri);
            Logger.AddInternalState("DownloadOptions=" + ((options != null) ? options.ToString() : "null"));
            tempFile = null;
            TempFile file = null;
            TempFile file2 = null;
            try
            {
                ServerInformation information;
                manifest = DownloadDeploymentManifestDirect(subStore, ref sourceUri, out file, notification, options, out information);
                Logger.SetSubscriptionServerInformation(information);
                bool flag = FollowDeploymentProviderUri(subStore, ref manifest, ref sourceUri, out file2, notification, options);
                tempFile = flag ? file2 : file;
            }
            finally
            {
                if ((file != null) && (file != tempFile))
                {
                    file.Dispose();
                    file = null;
                }
                if ((file2 != null) && (file2 != tempFile))
                {
                    file2.Dispose();
                    file2 = null;
                }
            }
            return manifest;
        }

        public static AssemblyManifest DownloadDeploymentManifestBypass(SubscriptionStore subStore, ref Uri sourceUri, out TempFile tempFile, out SubscriptionState subState, IDownloadNotification notification, DownloadOptions options)
        {
            AssemblyManifest manifest;
            Logger.AddMethodCall("DownloadDeploymentManifestBypass called.");
            tempFile = null;
            subState = null;
            TempFile file = null;
            TempFile file2 = null;
            try
            {
                ServerInformation information;
                manifest = DownloadDeploymentManifestDirectBypass(subStore, ref sourceUri, out file, out subState, notification, options, out information);
                Logger.SetSubscriptionServerInformation(information);
                if (subState != null)
                {
                    tempFile = file;
                    return manifest;
                }
                bool flag = FollowDeploymentProviderUri(subStore, ref manifest, ref sourceUri, out file2, notification, options);
                tempFile = flag ? file2 : file;
            }
            finally
            {
                if ((file != null) && (file != tempFile))
                {
                    file.Dispose();
                }
                if ((file2 != null) && (file2 != tempFile))
                {
                    file2.Dispose();
                }
            }
            return manifest;
        }

        private static AssemblyManifest DownloadDeploymentManifestDirect(SubscriptionStore subStore, ref Uri sourceUri, out TempFile tempFile, IDownloadNotification notification, DownloadOptions options, out ServerInformation serverInformation)
        {
            Logger.AddMethodCall("DownloadDeploymentManifestDirect(" + sourceUri + ") called.");
            tempFile = subStore.AcquireTempFile(".application");
            AssemblyManifest manifest = DownloadManifest(ref sourceUri, tempFile.Path, notification, options, AssemblyManifest.ManifestType.Deployment, out serverInformation);
            if (manifest.Identity.Version == null)
            {
                throw new InvalidDeploymentException(ExceptionTypes.ManifestSemanticValidation, Resources.GetString("Ex_DeploymentManifestNoVersion"));
            }
            if (manifest.Deployment == null)
            {
                throw new InvalidDeploymentException(ExceptionTypes.ManifestSemanticValidation, Resources.GetString("Ex_InvalidDeploymentManifest"));
            }
            return manifest;
        }

        private static AssemblyManifest DownloadDeploymentManifestDirectBypass(SubscriptionStore subStore, ref Uri sourceUri, out TempFile tempFile, out SubscriptionState subState, IDownloadNotification notification, DownloadOptions options, out ServerInformation serverInformation)
        {
            Logger.AddMethodCall("DownloadDeploymentManifestDirectBypass called.");
            subState = null;
            tempFile = subStore.AcquireTempFile(".application");
            DownloadManifestAsRawFile(ref sourceUri, tempFile.Path, notification, options, out serverInformation);
            bool flag = false;
            AssemblyManifest deployment = null;
            DefinitionIdentity identity = null;
            DefinitionIdentity identity2 = null;
            DefinitionAppId appId = null;
            try
            {
                deployment = ManifestReader.FromDocumentNoValidation(tempFile.Path);
                identity = deployment.Identity;
                identity2 = new DefinitionIdentity(deployment.MainDependentAssembly.Identity);
                Uri uri = ((sourceUri.Query != null) && (sourceUri.Query.Length > 0)) ? new Uri(sourceUri.GetLeftPart(UriPartial.Path)) : sourceUri;
                appId = new DefinitionAppId(uri.AbsoluteUri, new DefinitionIdentity[] { identity, identity2 });
                Logger.AddInternalState("expectedAppId=" + appId.ToString());
            }
            catch (InvalidDeploymentException)
            {
                flag = true;
            }
            catch (COMException)
            {
                flag = true;
            }
            catch (SEHException)
            {
                flag = true;
            }
            catch (IndexOutOfRangeException)
            {
                flag = true;
            }
            if (!flag)
            {
                long num;
                SubscriptionState subscriptionState = subStore.GetSubscriptionState(deployment);
                bool flag2 = false;
                using (subStore.AcquireReferenceTransaction(out num))
                {
                    flag2 = subStore.CheckAndReferenceApplication(subscriptionState, appId, num);
                }
                if (flag2 && appId.Equals(subscriptionState.CurrentBind))
                {
                    Logger.AddInternalState("Application is found in store and it is the CurrentBind, bypass validation and further downloads.");
                    subState = subscriptionState;
                    return subState.CurrentDeploymentManifest;
                }
                flag = true;
                if (flag2)
                {
                    Logger.AddInternalState("Application is found in store and but it is not the CurrentBind.");
                }
            }
            else
            {
                Logger.AddInternalState("Application is not found in store.");
            }
            Logger.AddInternalState("Reparse the deployment manifest for validations.");
            AssemblyManifest manifest2 = ManifestReader.FromDocument(tempFile.Path, AssemblyManifest.ManifestType.Deployment, sourceUri);
            if (manifest2.Identity.Version == null)
            {
                throw new InvalidDeploymentException(ExceptionTypes.ManifestSemanticValidation, Resources.GetString("Ex_DeploymentManifestNoVersion"));
            }
            if (manifest2.Deployment == null)
            {
                throw new InvalidDeploymentException(ExceptionTypes.ManifestSemanticValidation, Resources.GetString("Ex_InvalidDeploymentManifest"));
            }
            return manifest2;
        }

        private static AssemblyManifest DownloadManifest(ref Uri sourceUri, string targetPath, IDownloadNotification notification, DownloadOptions options, AssemblyManifest.ManifestType manifestType, out ServerInformation serverInformation)
        {
            Logger.AddMethodCall("DownloadManifest called.");
            DownloadManifestAsRawFile(ref sourceUri, targetPath, notification, options, out serverInformation);
            return ManifestReader.FromDocument(targetPath, manifestType, sourceUri);
        }

        private static void DownloadManifestAsRawFile(ref Uri sourceUri, string targetPath, IDownloadNotification notification, DownloadOptions options, out ServerInformation serverInformation)
        {
            Logger.AddMethodCall("DownloadManifestAsRawFile called.");
            FileDownloader downloader = FileDownloader.Create();
            downloader.Options = options;
            if (notification != null)
            {
                downloader.AddNotification(notification);
            }
            try
            {
                downloader.AddFile(sourceUri, targetPath, 0x1000000);
                downloader.Download(null);
                sourceUri = downloader.DownloadResults[0].ResponseUri;
                serverInformation = downloader.DownloadResults[0].ServerInformation;
            }
            finally
            {
                if (notification != null)
                {
                    downloader.RemoveNotification(notification);
                }
            }
        }

        public static bool FollowDeploymentProviderUri(SubscriptionStore subStore, ref AssemblyManifest deployment, ref Uri sourceUri, out TempFile tempFile, IDownloadNotification notification, DownloadOptions options)
        {
            Logger.AddMethodCall("FollowDeploymentProviderUri called.");
            tempFile = null;
            bool flag = false;
            Zone zone = Zone.CreateFromUrl(sourceUri.AbsoluteUri);
            bool flag2 = false;
            if (zone.SecurityZone != SecurityZone.MyComputer)
            {
                Logger.AddInternalState("Deployment manifest zone is not local machine. Zone = " + zone.SecurityZone);
                flag2 = true;
            }
            else
            {
                Logger.AddInternalState("Deployment manifest zone is local machine. Zone = " + zone.SecurityZone);
                DependentAssembly mainDependentAssembly = deployment.MainDependentAssembly;
                if ((mainDependentAssembly == null) || (mainDependentAssembly.Codebase == null))
                {
                    throw new InvalidDeploymentException(ExceptionTypes.ManifestSemanticValidation, Resources.GetString("Ex_NoAppInDeploymentManifest"));
                }
                Uri uri = new Uri(sourceUri, mainDependentAssembly.Codebase);
                Zone zone2 = Zone.CreateFromUrl(uri.AbsoluteUri);
                if (zone2.SecurityZone == SecurityZone.MyComputer)
                {
                    Logger.AddInternalState("Application manifest zone is local machine. Zone = " + zone2.SecurityZone);
                    if (!System.IO.File.Exists(uri.LocalPath))
                    {
                        Logger.AddInternalState(uri.LocalPath + " does not exist in local machine.");
                        flag2 = true;
                    }
                }
            }
            if (flag2)
            {
                Uri providerCodebaseUri = deployment.Deployment.ProviderCodebaseUri;
                Logger.SetDeploymentProviderUrl(providerCodebaseUri);
                Logger.AddInternalState(string.Concat(new object[] { "providerUri=", providerCodebaseUri, ",sourceUri=", sourceUri }));
                if ((!PolicyKeys.SkipDeploymentProvider() && (providerCodebaseUri != null)) && !providerCodebaseUri.Equals(sourceUri))
                {
                    ServerInformation information;
                    AssemblyManifest manifest = null;
                    try
                    {
                        manifest = DownloadDeploymentManifestDirect(subStore, ref providerCodebaseUri, out tempFile, notification, options, out information);
                    }
                    catch (InvalidDeploymentException exception)
                    {
                        if (((exception.SubType != ExceptionTypes.Manifest) && (exception.SubType != ExceptionTypes.ManifestLoad)) && ((exception.SubType != ExceptionTypes.ManifestParse) && (exception.SubType != ExceptionTypes.ManifestSemanticValidation)))
                        {
                            throw;
                        }
                        throw new InvalidDeploymentException(ExceptionTypes.Manifest, Resources.GetString("Ex_InvalidProviderManifest"), exception);
                    }
                    Logger.SetDeploymentProviderServerInformation(information);
                    SubscriptionState subscriptionState = subStore.GetSubscriptionState(deployment);
                    if (!subStore.GetSubscriptionState(manifest).SubscriptionId.Equals(subscriptionState.SubscriptionId))
                    {
                        throw new InvalidDeploymentException(ExceptionTypes.SubscriptionSemanticValidation, Resources.GetString("Ex_ProviderNotInSubscription"));
                    }
                    Logger.AddInternalState("Deployment provider followed: " + providerCodebaseUri);
                    deployment = manifest;
                    sourceUri = providerCodebaseUri;
                    flag = true;
                }
            }
            if (!flag)
            {
                Logger.AddInternalState("Deployment provider not followed.");
            }
            return flag;
        }

        private static Uri MapFileSourceUri(AssemblyManifest deployManifest, Uri sourceUriBase, string fileName)
        {
            return UriHelper.UriFromRelativeFilePath(sourceUriBase, deployManifest.Deployment.MapFileExtensions ? (fileName + ".deploy") : fileName);
        }

        private static void ProcessDownloadedFile(object sender, DownloadEventArgs e)
        {
            if (e.Cookie != null)
            {
                string fileName = Path.GetFileName(e.FileLocalPath);
                FileDownloader downloader = (FileDownloader) sender;
                if ((e.FileResponseUri != null) && !e.FileResponseUri.Equals(e.FileSourceUri))
                {
                    throw new InvalidDeploymentException(ExceptionTypes.AppFileLocationValidation, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_DownloadAppFileAsmRedirected"), new object[] { fileName }));
                }
                DependencyDownloadCookie cookie = (DependencyDownloadCookie) e.Cookie;
                if (cookie.ManifestElement is DependentAssembly)
                {
                    DependentAssembly manifestElement = (DependentAssembly) cookie.ManifestElement;
                    AssemblyManifest deployManifest = cookie.DeployManifest;
                    AssemblyManifest appManifest = cookie.AppManifest;
                    AssemblyManifest assemblyManifest = new AssemblyManifest(e.FileLocalPath);
                    if (!assemblyManifest.Identity.Matches(manifestElement.Identity, true))
                    {
                        throw new InvalidDeploymentException(ExceptionTypes.RefDefValidation, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_DownloadRefDefMismatch"), new object[] { fileName }));
                    }
                    if (assemblyManifest.Identity.Equals(deployManifest.Identity) || assemblyManifest.Identity.Equals(appManifest.Identity))
                    {
                        throw new InvalidDeploymentException(ExceptionTypes.ManifestSemanticValidation, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_AppPrivAsmIdSameAsDeployOrApp"), new object[] { assemblyManifest.Identity.ToString() }));
                    }
                    System.Deployment.Application.Manifest.File[] files = assemblyManifest.Files;
                    for (int i = 0; i < files.Length; i++)
                    {
                        Uri fileSourceUri = MapFileSourceUri(deployManifest, e.FileSourceUri, files[i].Name);
                        if (!fileSourceUri.AbsoluteUri.Equals(e.FileSourceUri.AbsoluteUri, StringComparison.OrdinalIgnoreCase))
                        {
                            string directoryName = Path.GetDirectoryName(e.FileLocalPath);
                            AddFileToDownloader(downloader, deployManifest, appManifest, files[i], fileSourceUri, directoryName, files[i].NameFS, files[i].HashCollection);
                        }
                    }
                    downloader.ComponentVerifier.AddFileForVerification(e.FileLocalPath, manifestElement.HashCollection);
                    if (assemblyManifest.Identity.PublicKeyToken == null)
                    {
                        downloader.ComponentVerifier.AddSimplyNamedAssemblyForVerification(e.FileLocalPath, assemblyManifest);
                    }
                    else
                    {
                        downloader.ComponentVerifier.AddStrongNameAssemblyForVerification(e.FileLocalPath, assemblyManifest);
                    }
                }
                else if (cookie.ManifestElement is System.Deployment.Application.Manifest.File)
                {
                    System.Deployment.Application.Manifest.File file = (System.Deployment.Application.Manifest.File) cookie.ManifestElement;
                    downloader.ComponentVerifier.AddFileForVerification(e.FileLocalPath, file.HashCollection);
                }
            }
        }

        private static void ReorderFilesForIconFile(AssemblyManifest manifest, System.Deployment.Application.Manifest.File[] files)
        {
            if ((manifest.Description != null) && (manifest.Description.IconFile != null))
            {
                for (int i = 0; i < files.Length; i++)
                {
                    if (string.Compare(files[i].NameFS, manifest.Description.IconFileFS, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        if (i != 0)
                        {
                            System.Deployment.Application.Manifest.File file = files[0];
                            files[0] = files[i];
                            files[i] = file;
                        }
                        return;
                    }
                }
            }
        }

        private static void VerifyRequestedPrivilegesSupport(string requestedExecutionLevel)
        {
            Logger.AddMethodCall("VerifyRequestedPrivilegesSupport(" + requestedExecutionLevel + ") called.");
            if (PlatformSpecific.OnVistaOrAbove)
            {
                bool flag = false;
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System");
                if ((key != null) && (key.GetValue("EnableLUA") != null))
                {
                    Logger.AddInternalState("LUA policy key = " + key.Name);
                    if (((int) key.GetValue("EnableLUA")) != 0)
                    {
                        flag = true;
                        Logger.AddInternalState("LUA is enabled.");
                    }
                }
                if (flag && ((string.Compare(requestedExecutionLevel, "requireAdministrator", StringComparison.OrdinalIgnoreCase) == 0) || (string.Compare(requestedExecutionLevel, "highestAvailable", StringComparison.OrdinalIgnoreCase) == 0)))
                {
                    throw new InvalidDeploymentException(ExceptionTypes.UnsupportedElevetaionRequest, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_ManifestExecutionLevelNotSupported"), new object[0]));
                }
            }
        }

        private static void VerifyRequestedPrivilegesSupport(AssemblyManifest appManifest, string targetDirectory)
        {
            if (!appManifest.EntryPoints[0].CustomHostSpecified)
            {
                string path = Path.Combine(targetDirectory, appManifest.EntryPoints[0].Assembly.Codebase);
                if (System.IO.File.Exists(path))
                {
                    AssemblyManifest manifest = new AssemblyManifest(path);
                    if (manifest.Id1ManifestPresent && (manifest.Id1RequestedExecutionLevel != null))
                    {
                        VerifyRequestedPrivilegesSupport(manifest.Id1RequestedExecutionLevel);
                    }
                }
                else
                {
                    Logger.AddInternalState("Main exe=" + path + " does not exist. No Requested Priviliges Verification done.");
                }
            }
        }

        private class DependencyDownloadCookie
        {
            public readonly AssemblyManifest AppManifest;
            public readonly AssemblyManifest DeployManifest;
            public readonly object ManifestElement;

            public DependencyDownloadCookie(object manifestElement, AssemblyManifest deployManifest, AssemblyManifest appManifest)
            {
                this.ManifestElement = manifestElement;
                this.DeployManifest = deployManifest;
                this.AppManifest = appManifest;
            }
        }
    }
}

