namespace System.Security.Policy
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.Deployment.Internal;
    using System.Deployment.Internal.CodeSigning;
    using System.Deployment.Internal.Isolation;
    using System.Deployment.Internal.Isolation.Manifest;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;
    using System.Windows.Forms;
    using System.Xml;

    internal class TrustManager : IApplicationTrustManager, ISecurityEncodable
    {
        public const string PromptingLevelKeyName = @"Software\Microsoft\.NETFramework\Security\TrustManager\PromptingLevel";

        private static bool AnalyzeCertificate(ParsedData parsedData, MemoryStream ms, out bool distrustedPublisher, out bool trustedPublisher, out bool noCertificate)
        {
            distrustedPublisher = false;
            trustedPublisher = false;
            noCertificate = false;
            System.Deployment.Internal.CodeSigning.SignedCmiManifest manifest = null;
            try
            {
                XmlDocument manifestDom = new XmlDocument {
                    PreserveWhitespace = true
                };
                manifestDom.Load(ms);
                manifest = new System.Deployment.Internal.CodeSigning.SignedCmiManifest(manifestDom);
                manifest.Verify(System.Deployment.Internal.CodeSigning.CmiManifestVerifyFlags.None);
            }
            catch (Exception exception)
            {
                if (!(exception is CryptographicException) || (manifest.AuthenticodeSignerInfo == null))
                {
                    return false;
                }
                int errorCode = manifest.AuthenticodeSignerInfo.ErrorCode;
                switch (errorCode)
                {
                    case -2146762479:
                    case -2146885616:
                        distrustedPublisher = true;
                        return true;
                }
                if (errorCode != -2146762748)
                {
                    noCertificate = true;
                }
                return true;
            }
            finally
            {
                if (((manifest != null) && (manifest.AuthenticodeSignerInfo != null)) && (manifest.AuthenticodeSignerInfo.SignerChain != null))
                {
                    parsedData.Certificate = manifest.AuthenticodeSignerInfo.SignerChain.ChainElements[0].Certificate;
                    parsedData.AuthenticodedPublisher = parsedData.Certificate.GetNameInfo(X509NameType.SimpleName, false);
                }
            }
            if ((manifest == null) || (manifest.AuthenticodeSignerInfo == null))
            {
                noCertificate = true;
            }
            else
            {
                trustedPublisher = true;
            }
            return true;
        }

        private static bool AppRequestsBeyondDefaultTrust(ApplicationSecurityInfo info)
        {
            try
            {
                PermissionSet standardSandbox = SecurityManager.GetStandardSandbox(info.ApplicationEvidence);
                PermissionSet requestedPermissionSet = GetRequestedPermissionSet(info);
                if ((standardSandbox == null) && (requestedPermissionSet != null))
                {
                    return true;
                }
                if ((standardSandbox != null) && (requestedPermissionSet == null))
                {
                    return false;
                }
                return !requestedPermissionSet.IsSubsetOf(standardSandbox);
            }
            catch (Exception)
            {
                return true;
            }
        }

        private static ApplicationTrust BasicInstallPrompt(ActivationContext activationContext, ParsedData parsedData, string deploymentUrl, HostContextInternal hostContextInternal, ApplicationSecurityInfo info, ApplicationTrustExtraInfo appTrustExtraInfo, string zoneName, bool permissionElevationRequired)
        {
            DialogResult no;
            TrustManagerPromptOptions options = CompletePromptOptions(permissionElevationRequired ? TrustManagerPromptOptions.RequiresPermissions : TrustManagerPromptOptions.None, appTrustExtraInfo, zoneName, info);
            try
            {
                no = new TrustManagerPromptUIThread(string.IsNullOrEmpty(parsedData.AppName) ? info.ApplicationId.Name : parsedData.AppName, DefaultBrowserExePath, parsedData.SupportUrl, GetHostFromDeploymentUrl(deploymentUrl), parsedData.AuthenticodedPublisher, parsedData.Certificate, options).ShowDialog();
            }
            catch (Exception)
            {
                no = DialogResult.No;
            }
            return CreateApplicationTrust(activationContext, info, appTrustExtraInfo, no == DialogResult.OK, hostContextInternal.Persist && (no == DialogResult.OK));
        }

        private static ApplicationTrust BlockingPrompt(ActivationContext activationContext, ParsedData parsedData, string deploymentUrl, ApplicationSecurityInfo info, ApplicationTrustExtraInfo appTrustExtraInfo, string zoneName, bool permissionElevationRequired)
        {
            TrustManagerPromptOptions options = CompletePromptOptions(permissionElevationRequired ? (TrustManagerPromptOptions.RequiresPermissions | TrustManagerPromptOptions.StopApp) : TrustManagerPromptOptions.StopApp, appTrustExtraInfo, zoneName, info);
            try
            {
                new TrustManagerPromptUIThread(string.IsNullOrEmpty(parsedData.AppName) ? info.ApplicationId.Name : parsedData.AppName, DefaultBrowserExePath, parsedData.SupportUrl, GetHostFromDeploymentUrl(deploymentUrl), parsedData.AuthenticodedPublisher, parsedData.Certificate, options).ShowDialog();
            }
            catch (Exception)
            {
            }
            return CreateApplicationTrust(activationContext, info, appTrustExtraInfo, false, false);
        }

        private static TrustManagerPromptOptions CompletePromptOptions(TrustManagerPromptOptions options, ApplicationTrustExtraInfo appTrustExtraInfo, string zoneName, ApplicationSecurityInfo info)
        {
            if (appTrustExtraInfo.RequestsShellIntegration)
            {
                options |= TrustManagerPromptOptions.AddsShortcut;
            }
            if (zoneName != null)
            {
                if (string.Compare(zoneName, "Internet", true, CultureInfo.InvariantCulture) == 0)
                {
                    options |= TrustManagerPromptOptions.InternetSource;
                }
                else if (string.Compare(zoneName, "TrustedSites", true, CultureInfo.InvariantCulture) == 0)
                {
                    options |= TrustManagerPromptOptions.TrustedSitesSource;
                }
                else if (string.Compare(zoneName, "UntrustedSites", true, CultureInfo.InvariantCulture) == 0)
                {
                    options |= TrustManagerPromptOptions.UntrustedSitesSource;
                }
                else if (string.Compare(zoneName, "LocalIntranet", true, CultureInfo.InvariantCulture) == 0)
                {
                    options |= TrustManagerPromptOptions.LocalNetworkSource;
                }
                else if (string.Compare(zoneName, "MyComputer", true, CultureInfo.InvariantCulture) == 0)
                {
                    options |= TrustManagerPromptOptions.LocalComputerSource;
                }
            }
            if (info != null)
            {
                PermissionSet defaultRequestSet = info.DefaultRequestSet;
                if ((defaultRequestSet != null) && defaultRequestSet.IsUnrestricted())
                {
                    options |= TrustManagerPromptOptions.WillHaveFullTrust;
                }
            }
            return options;
        }

        private static ApplicationTrust CreateApplicationTrust(ActivationContext activationContext, ApplicationSecurityInfo info, ApplicationTrustExtraInfo appTrustExtraInfo, bool trust, bool persist)
        {
            return new ApplicationTrust(activationContext.Identity) { ExtraInfo = appTrustExtraInfo, IsApplicationTrustedToRun = trust, DefaultGrantSet = new PolicyStatement(info.DefaultRequestSet, PolicyStatementAttribute.Nothing), Persist = persist };
        }

        [HostProtection(SecurityAction.LinkDemand, UI=true)]
        public ApplicationTrust DetermineApplicationTrust(ActivationContext activationContext, TrustManagerContext trustManagerContext)
        {
            MemoryStream stream;
            bool flag;
            bool flag2;
            bool flag3;
            MemoryStream stream2;
            ArrayList list;
            if (activationContext == null)
            {
                throw new ArgumentNullException("activationContext");
            }
            ApplicationSecurityInfo info = new ApplicationSecurityInfo(activationContext);
            ApplicationTrustExtraInfo appTrustExtraInfo = new ApplicationTrustExtraInfo();
            HostContextInternal hostContextInternal = new HostContextInternal(trustManagerContext);
            System.Deployment.Internal.Isolation.Manifest.ICMS deploymentComponentManifest = (System.Deployment.Internal.Isolation.Manifest.ICMS) InternalActivationContextHelper.GetDeploymentComponentManifest(activationContext);
            ParsedData parsedData = new ParsedData();
            if (ParseManifest(deploymentComponentManifest, parsedData))
            {
                appTrustExtraInfo.RequestsShellIntegration = parsedData.RequestsShellIntegration;
            }
            string deploymentUrl = GetDeploymentUrl(info);
            string zoneNameFromDeploymentUrl = GetZoneNameFromDeploymentUrl(deploymentUrl);
            if (!ExtractManifestContent(deploymentComponentManifest, out stream))
            {
                return BlockingPrompt(activationContext, parsedData, deploymentUrl, info, appTrustExtraInfo, zoneNameFromDeploymentUrl, AppRequestsBeyondDefaultTrust(info));
            }
            AnalyzeCertificate(parsedData, stream, out flag, out flag2, out flag3);
            System.Deployment.Internal.Isolation.Manifest.ICMS applicationComponentManifest = (System.Deployment.Internal.Isolation.Manifest.ICMS) InternalActivationContextHelper.GetApplicationComponentManifest(activationContext);
            ParsedData data2 = new ParsedData();
            if ((ParseManifest(applicationComponentManifest, data2) && data2.UseManifestForTrust) && ExtractManifestContent(applicationComponentManifest, out stream2))
            {
                bool flag4;
                bool flag5;
                bool flag6;
                AnalyzeCertificate(parsedData, stream2, out flag4, out flag5, out flag6);
                flag = flag4;
                flag2 = flag5;
                flag3 = flag6;
                parsedData.AppName = data2.AppName;
                parsedData.AppPublisher = data2.AppPublisher;
                parsedData.SupportUrl = data2.SupportUrl;
            }
            if (flag)
            {
                if (GetPromptsAllowed(hostContextInternal, zoneNameFromDeploymentUrl, parsedData) == PromptsAllowed.None)
                {
                    return CreateApplicationTrust(activationContext, info, appTrustExtraInfo, false, false);
                }
                return BlockingPrompt(activationContext, parsedData, deploymentUrl, info, appTrustExtraInfo, zoneNameFromDeploymentUrl, AppRequestsBeyondDefaultTrust(info));
            }
            if (flag3)
            {
                parsedData.AuthenticodedPublisher = null;
                parsedData.Certificate = null;
            }
            if ((!hostContextInternal.IgnorePersistedDecision && SearchPreviousTrustedVersion(activationContext, hostContextInternal.PreviousAppId, out list)) && ExistingTrustApplicable(info, list))
            {
                if ((appTrustExtraInfo.RequestsShellIntegration && !SomePreviousTrustedVersionRequiresShellIntegration(list)) && !flag2)
                {
                    switch (GetPromptsAllowed(hostContextInternal, zoneNameFromDeploymentUrl, parsedData))
                    {
                        case PromptsAllowed.All:
                            return BasicInstallPrompt(activationContext, parsedData, deploymentUrl, hostContextInternal, info, appTrustExtraInfo, zoneNameFromDeploymentUrl, AppRequestsBeyondDefaultTrust(info));

                        case PromptsAllowed.BlockingOnly:
                            return BlockingPrompt(activationContext, parsedData, deploymentUrl, info, appTrustExtraInfo, zoneNameFromDeploymentUrl, AppRequestsBeyondDefaultTrust(info));

                        case PromptsAllowed.None:
                            return CreateApplicationTrust(activationContext, info, appTrustExtraInfo, false, false);
                    }
                }
                return CreateApplicationTrust(activationContext, info, appTrustExtraInfo, true, hostContextInternal.Persist);
            }
            bool permissionElevationRequired = AppRequestsBeyondDefaultTrust(info);
            if (!permissionElevationRequired || flag2)
            {
                if (flag2)
                {
                    return CreateApplicationTrust(activationContext, info, appTrustExtraInfo, true, hostContextInternal.Persist);
                }
                switch (GetPromptsAllowed(hostContextInternal, zoneNameFromDeploymentUrl, parsedData))
                {
                    case PromptsAllowed.All:
                    case PromptsAllowed.None:
                        return BasicInstallPrompt(activationContext, parsedData, deploymentUrl, hostContextInternal, info, appTrustExtraInfo, zoneNameFromDeploymentUrl, false);

                    case PromptsAllowed.BlockingOnly:
                        return BlockingPrompt(activationContext, parsedData, deploymentUrl, info, appTrustExtraInfo, zoneNameFromDeploymentUrl, permissionElevationRequired);
                }
            }
            switch (GetPromptsAllowed(hostContextInternal, zoneNameFromDeploymentUrl, parsedData))
            {
                case PromptsAllowed.BlockingOnly:
                    return BlockingPrompt(activationContext, parsedData, deploymentUrl, info, appTrustExtraInfo, zoneNameFromDeploymentUrl, true);

                case PromptsAllowed.None:
                    return CreateApplicationTrust(activationContext, info, appTrustExtraInfo, false, false);
            }
            return HighRiskPrompt(activationContext, parsedData, deploymentUrl, hostContextInternal, info, appTrustExtraInfo, zoneNameFromDeploymentUrl);
        }

        private static bool ExistingTrustApplicable(ApplicationSecurityInfo info, ArrayList matchingTrusts)
        {
            int index = 0;
            while (index < matchingTrusts.Count)
            {
                ApplicationTrust trust = (ApplicationTrust) matchingTrusts[index];
                if (!trust.IsApplicationTrustedToRun)
                {
                    matchingTrusts.RemoveAt(index);
                }
                PermissionSet requestedPermissionSet = GetRequestedPermissionSet(info);
                PermissionSet permissionSet = trust.DefaultGrantSet.PermissionSet;
                if ((permissionSet == null) && (requestedPermissionSet != null))
                {
                    matchingTrusts.RemoveAt(index);
                }
                else if ((permissionSet != null) && (requestedPermissionSet == null))
                {
                    index++;
                    continue;
                }
                if (requestedPermissionSet.IsSubsetOf(permissionSet))
                {
                    index++;
                }
                else
                {
                    matchingTrusts.RemoveAt(index);
                }
            }
            return (matchingTrusts.Count > 0);
        }

        private static unsafe bool ExtractManifestContent(System.Deployment.Internal.Isolation.Manifest.ICMS cms, out MemoryStream ms)
        {
            ms = new MemoryStream();
            try
            {
                System.Runtime.InteropServices.ComTypes.IStream stream = cms as System.Runtime.InteropServices.ComTypes.IStream;
                if (stream == null)
                {
                    return false;
                }
                byte[] pv = new byte[0x1000];
                int cb = 0x1000;
                do
                {
                    stream.Read(pv, cb, new IntPtr((void*) &cb));
                    ms.Write(pv, 0, cb);
                }
                while (cb == 0x1000);
                ms.Position = 0L;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void FromXml(SecurityElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (!string.Equals(element.Tag, "IApplicationTrustManager", StringComparison.Ordinal))
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("TrustManagerBadXml", new object[] { "IApplicationTrustManager" }));
            }
        }

        private static PromptingLevel GetDefaultPromptingLevel(string zoneName)
        {
            switch (zoneName)
            {
                case "Internet":
                case "LocalIntranet":
                case "MyComputer":
                case "TrustedSites":
                    return PromptingLevel.Prompt;

                case "UntrustedSites":
                    return PromptingLevel.Disabled;
            }
            return PromptingLevel.Disabled;
        }

        private static string GetDeploymentUrl(ApplicationSecurityInfo info)
        {
            Url hostEvidence = info.ApplicationEvidence.GetHostEvidence<Url>();
            if (hostEvidence != null)
            {
                return hostEvidence.Value;
            }
            return null;
        }

        private static string GetHostFromDeploymentUrl(string deploymentUrl)
        {
            if (deploymentUrl == null)
            {
                return string.Empty;
            }
            string absolutePath = null;
            try
            {
                Uri uri = new Uri(deploymentUrl);
                if ((uri.Scheme == Uri.UriSchemeHttp) || (uri.Scheme == Uri.UriSchemeHttps))
                {
                    absolutePath = uri.Host;
                }
                if (!string.IsNullOrEmpty(absolutePath))
                {
                    return absolutePath;
                }
                absolutePath = uri.AbsolutePath;
                int startIndex = -1;
                if (string.IsNullOrEmpty(uri.Host) && absolutePath.StartsWith("/"))
                {
                    absolutePath = absolutePath.TrimStart(new char[] { '/' });
                    startIndex = absolutePath.IndexOf('/');
                }
                else if ((uri.LocalPath.Length > 2) && ((uri.LocalPath[1] == ':') || uri.LocalPath.StartsWith(@"\\")))
                {
                    absolutePath = uri.LocalPath;
                    startIndex = absolutePath.LastIndexOf('\\');
                }
                if (startIndex != -1)
                {
                    absolutePath = absolutePath.Remove(startIndex);
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
            return absolutePath;
        }

        private static PromptsAllowed GetPromptsAllowed(HostContextInternal hostContextInternal, string zoneName, ParsedData parsedData)
        {
            if (hostContextInternal.NoPrompt)
            {
                return PromptsAllowed.None;
            }
            PromptingLevel zonePromptingLevel = GetZonePromptingLevel(zoneName);
            if ((zonePromptingLevel != PromptingLevel.Disabled) && ((zonePromptingLevel != PromptingLevel.PromptOnlyForAuthenticode) || (parsedData.AuthenticodedPublisher != null)))
            {
                return PromptsAllowed.All;
            }
            return PromptsAllowed.BlockingOnly;
        }

        private static PermissionSet GetRequestedPermissionSet(ApplicationSecurityInfo info)
        {
            PermissionSet defaultRequestSet = info.DefaultRequestSet;
            PermissionSet set2 = null;
            if (defaultRequestSet != null)
            {
                set2 = defaultRequestSet.Copy();
            }
            return set2;
        }

        private static string GetZoneNameFromDeploymentUrl(string deploymentUrl)
        {
            Zone zone = Zone.CreateFromUrl(deploymentUrl);
            if ((zone != null) && (zone.SecurityZone != SecurityZone.NoZone))
            {
                switch (zone.SecurityZone)
                {
                    case SecurityZone.MyComputer:
                        return "MyComputer";

                    case SecurityZone.Intranet:
                        return "LocalIntranet";

                    case SecurityZone.Trusted:
                        return "TrustedSites";

                    case SecurityZone.Internet:
                        return "Internet";

                    case SecurityZone.Untrusted:
                        return "UntrustedSites";
                }
            }
            return "UntrustedSites";
        }

        private static PromptingLevel GetZonePromptingLevel(string zoneName)
        {
            try
            {
                string str = null;
                new RegistryPermission(PermissionState.Unrestricted).Assert();
                try
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\.NETFramework\Security\TrustManager\PromptingLevel"))
                    {
                        if (key != null)
                        {
                            str = (string) key.GetValue(zoneName);
                        }
                    }
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
                if (!string.IsNullOrEmpty(str))
                {
                    if (string.Compare(str, "Enabled", true, CultureInfo.InvariantCulture) == 0)
                    {
                        return PromptingLevel.Prompt;
                    }
                    if (string.Compare(str, "Disabled", true, CultureInfo.InvariantCulture) == 0)
                    {
                        return PromptingLevel.Disabled;
                    }
                    if (string.Compare(str, "AuthenticodeRequired", true, CultureInfo.InvariantCulture) == 0)
                    {
                        return PromptingLevel.PromptOnlyForAuthenticode;
                    }
                }
                return GetDefaultPromptingLevel(zoneName);
            }
            catch (Exception)
            {
                return GetDefaultPromptingLevel(zoneName);
            }
        }

        private static ApplicationTrust HighRiskPrompt(ActivationContext activationContext, ParsedData parsedData, string deploymentUrl, HostContextInternal hostContextInternal, ApplicationSecurityInfo info, ApplicationTrustExtraInfo appTrustExtraInfo, string zoneName)
        {
            DialogResult no;
            TrustManagerPromptOptions options = CompletePromptOptions(TrustManagerPromptOptions.RequiresPermissions, appTrustExtraInfo, zoneName, info);
            try
            {
                no = new TrustManagerPromptUIThread(string.IsNullOrEmpty(parsedData.AppName) ? info.ApplicationId.Name : parsedData.AppName, DefaultBrowserExePath, parsedData.SupportUrl, GetHostFromDeploymentUrl(deploymentUrl), parsedData.AuthenticodedPublisher, parsedData.Certificate, options).ShowDialog();
            }
            catch (Exception)
            {
                no = DialogResult.No;
            }
            return CreateApplicationTrust(activationContext, info, appTrustExtraInfo, no == DialogResult.OK, hostContextInternal.Persist && (no == DialogResult.OK));
        }

        private static bool IsInternetZone(string zoneName)
        {
            return (string.Compare(zoneName, "Internet", true, CultureInfo.InvariantCulture) == 0);
        }

        private static bool ParseManifest(System.Deployment.Internal.Isolation.Manifest.ICMS cms, ParsedData parsedData)
        {
            try
            {
                if ((cms != null) && (cms.MetadataSectionEntry != null))
                {
                    System.Deployment.Internal.Isolation.Manifest.IMetadataSectionEntry metadataSectionEntry = cms.MetadataSectionEntry as System.Deployment.Internal.Isolation.Manifest.IMetadataSectionEntry;
                    if (metadataSectionEntry != null)
                    {
                        System.Deployment.Internal.Isolation.Manifest.IDescriptionMetadataEntry descriptionData = metadataSectionEntry.DescriptionData;
                        if (descriptionData != null)
                        {
                            parsedData.SupportUrl = descriptionData.SupportUrl;
                            parsedData.AppName = descriptionData.Product;
                            parsedData.AppPublisher = descriptionData.Publisher;
                        }
                        System.Deployment.Internal.Isolation.Manifest.IDeploymentMetadataEntry deploymentData = metadataSectionEntry.DeploymentData;
                        if (deploymentData != null)
                        {
                            parsedData.RequestsShellIntegration = (deploymentData.DeploymentFlags & 0x20) != 0;
                        }
                        if ((metadataSectionEntry.ManifestFlags & 8) != 0)
                        {
                            parsedData.UseManifestForTrust = true;
                        }
                        else
                        {
                            parsedData.UseManifestForTrust = false;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private static bool SearchPreviousTrustedVersion(ActivationContext activationContext, ApplicationIdentity previousAppId, out ArrayList matchingTrusts)
        {
            matchingTrusts = null;
            ApplicationTrustCollection userApplicationTrusts = ApplicationSecurityManager.UserApplicationTrusts;
            ApplicationTrustEnumerator enumerator = userApplicationTrusts.GetEnumerator();
            while (enumerator.MoveNext())
            {
                ApplicationTrust current = enumerator.Current;
                System.Deployment.Internal.Isolation.IDefinitionAppId id = System.Deployment.Internal.Isolation.IsolationInterop.AppIdAuthority.TextToDefinition(0, current.ApplicationIdentity.FullName);
                System.Deployment.Internal.Isolation.IDefinitionAppId id2 = System.Deployment.Internal.Isolation.IsolationInterop.AppIdAuthority.TextToDefinition(0, activationContext.Identity.FullName);
                if (System.Deployment.Internal.Isolation.IsolationInterop.AppIdAuthority.AreDefinitionsEqual(1, id, id2))
                {
                    if (matchingTrusts == null)
                    {
                        matchingTrusts = new ArrayList();
                    }
                    matchingTrusts.Add(current);
                }
            }
            if (previousAppId != null)
            {
                ApplicationTrustEnumerator enumerator2 = userApplicationTrusts.GetEnumerator();
                while (enumerator2.MoveNext())
                {
                    ApplicationTrust trust2 = enumerator2.Current;
                    System.Deployment.Internal.Isolation.IDefinitionAppId id3 = System.Deployment.Internal.Isolation.IsolationInterop.AppIdAuthority.TextToDefinition(0, trust2.ApplicationIdentity.FullName);
                    System.Deployment.Internal.Isolation.IDefinitionAppId id4 = System.Deployment.Internal.Isolation.IsolationInterop.AppIdAuthority.TextToDefinition(0, previousAppId.FullName);
                    if (System.Deployment.Internal.Isolation.IsolationInterop.AppIdAuthority.AreDefinitionsEqual(1, id3, id4))
                    {
                        if (matchingTrusts == null)
                        {
                            matchingTrusts = new ArrayList();
                        }
                        matchingTrusts.Add(trust2);
                    }
                }
            }
            return (matchingTrusts != null);
        }

        private static bool SomePreviousTrustedVersionRequiresShellIntegration(ArrayList matchingTrusts)
        {
            foreach (ApplicationTrust trust in matchingTrusts)
            {
                ApplicationTrustExtraInfo extraInfo = trust.ExtraInfo as ApplicationTrustExtraInfo;
                if ((extraInfo != null) && extraInfo.RequestsShellIntegration)
                {
                    return true;
                }
                if ((extraInfo == null) && trust.DefaultGrantSet.PermissionSet.IsUnrestricted())
                {
                    return true;
                }
            }
            return false;
        }

        public SecurityElement ToXml()
        {
            SecurityElement element = new SecurityElement("IApplicationTrustManager");
            element.AddAttribute("class", SecurityElement.Escape(base.GetType().AssemblyQualifiedName));
            element.AddAttribute("version", "1");
            return element;
        }

        private static string DefaultBrowserExePath
        {
            get
            {
                try
                {
                    string str = null;
                    new RegistryPermission(PermissionState.Unrestricted).Assert();
                    try
                    {
                        RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"http\shell\open\command");
                        if (key != null)
                        {
                            string str2 = (string) key.GetValue(string.Empty);
                            key.Close();
                            if (str2 != null)
                            {
                                str2 = str2.Trim();
                                if (str2.Length != 0)
                                {
                                    if (str2[0] == '"')
                                    {
                                        int index = str2.IndexOf('"', 1);
                                        if (index != -1)
                                        {
                                            str = str2.Substring(1, index - 1);
                                        }
                                    }
                                    else
                                    {
                                        int length = str2.IndexOf(' ');
                                        if (length != -1)
                                        {
                                            str = str2.Substring(0, length);
                                        }
                                        else
                                        {
                                            str = str2;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                    return str;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        private enum PromptingLevel
        {
            Disabled,
            Prompt,
            PromptOnlyForAuthenticode
        }

        private enum PromptsAllowed
        {
            All,
            BlockingOnly,
            None
        }
    }
}

