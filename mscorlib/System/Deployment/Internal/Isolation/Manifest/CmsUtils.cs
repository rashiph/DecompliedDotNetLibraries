namespace System.Deployment.Internal.Isolation.Manifest
{
    using Microsoft.Win32;
    using System;
    using System.Deployment.Internal.Isolation;
    using System.IO;
    using System.Runtime.Hosting;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Text;

    [SecuritySafeCritical, SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode)]
    internal static class CmsUtils
    {
        internal static bool CompareIdentities(ActivationContext activationContext1, ActivationContext activationContext2)
        {
            if ((activationContext1 != null) && (activationContext2 != null))
            {
                return IsolationInterop.AppIdAuthority.AreDefinitionsEqual(0, activationContext1.Identity.Identity, activationContext2.Identity.Identity);
            }
            return (activationContext1 == activationContext2);
        }

        internal static bool CompareIdentities(ApplicationIdentity applicationIdentity1, ApplicationIdentity applicationIdentity2, ApplicationVersionMatch versionMatch)
        {
            uint num;
            if ((applicationIdentity1 == null) || (applicationIdentity2 == null))
            {
                return (applicationIdentity1 == applicationIdentity2);
            }
            switch (versionMatch)
            {
                case ApplicationVersionMatch.MatchExactVersion:
                    num = 0;
                    break;

                case ApplicationVersionMatch.MatchAllVersions:
                    num = 1;
                    break;

                default:
                    throw new ArgumentException(Environment.GetResourceString("Arg_EnumIllegalVal", new object[] { (int) versionMatch }), "versionMatch");
            }
            return IsolationInterop.AppIdAuthority.AreDefinitionsEqual(num, applicationIdentity1.Identity, applicationIdentity2.Identity);
        }

        internal static void CreateActivationContext(string fullName, string[] manifestPaths, bool useFusionActivationContext, out ApplicationIdentity applicationIdentity, out ActivationContext activationContext)
        {
            applicationIdentity = new ApplicationIdentity(fullName);
            activationContext = null;
            if (useFusionActivationContext)
            {
                if (manifestPaths != null)
                {
                    activationContext = new ActivationContext(applicationIdentity, manifestPaths);
                }
                else
                {
                    activationContext = new ActivationContext(applicationIdentity);
                }
            }
        }

        internal static IAssemblyReferenceEntry[] GetDependentAssemblies(ActivationContext activationContext)
        {
            IAssemblyReferenceEntry[] rgelt = null;
            ICMS applicationComponentManifest = activationContext.ApplicationComponentManifest;
            if (applicationComponentManifest != null)
            {
                ISection assemblyReferenceSection = applicationComponentManifest.AssemblyReferenceSection;
                uint celt = (assemblyReferenceSection != null) ? assemblyReferenceSection.Count : 0;
                if (celt <= 0)
                {
                    return rgelt;
                }
                uint celtFetched = 0;
                rgelt = new IAssemblyReferenceEntry[celt];
                int num3 = ((IEnumUnknown) assemblyReferenceSection._NewEnum).Next(celt, rgelt, ref celtFetched);
                if ((celtFetched == celt) && (num3 >= 0))
                {
                    return rgelt;
                }
            }
            return null;
        }

        internal static void GetEntryPoint(ActivationContext activationContext, out string fileName, out string parameters)
        {
            parameters = null;
            fileName = null;
            ICMS applicationComponentManifest = activationContext.ApplicationComponentManifest;
            if ((applicationComponentManifest == null) || (applicationComponentManifest.EntryPointSection == null))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_NoMain"));
            }
            IEnumUnknown unknown = (IEnumUnknown) applicationComponentManifest.EntryPointSection._NewEnum;
            uint celtFetched = 0;
            object[] rgelt = new object[1];
            if ((unknown.Next(1, rgelt, ref celtFetched) == 0) && (celtFetched == 1))
            {
                IEntryPointEntry entry = (IEntryPointEntry) rgelt[0];
                EntryPointEntry allData = entry.AllData;
                if ((allData.CommandLine_File != null) && (allData.CommandLine_File.Length > 0))
                {
                    fileName = allData.CommandLine_File;
                }
                else
                {
                    IAssemblyReferenceEntry entry3 = null;
                    object ppUnknown = null;
                    if (allData.Identity != null)
                    {
                        ((ISectionWithReferenceIdentityKey) applicationComponentManifest.AssemblyReferenceSection).Lookup(allData.Identity, out ppUnknown);
                        entry3 = (IAssemblyReferenceEntry) ppUnknown;
                        fileName = entry3.DependentAssembly.Codebase;
                    }
                }
                parameters = allData.CommandLine_Parameters;
            }
        }

        internal static string GetEntryPointFullPath(ActivationContext activationContext)
        {
            string str;
            string str2;
            GetEntryPoint(activationContext, out str, out str2);
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            string applicationDirectory = activationContext.ApplicationDirectory;
            if ((applicationDirectory == null) || (applicationDirectory.Length == 0))
            {
                StringBuilder lpBuffer = new StringBuilder(0x105);
                if (Win32Native.GetCurrentDirectory(lpBuffer.Capacity, lpBuffer) == 0)
                {
                    __Error.WinIOError();
                }
                applicationDirectory = lpBuffer.ToString();
            }
            return Path.Combine(applicationDirectory, str);
        }

        internal static string GetEntryPointFullPath(ActivationArguments activationArguments)
        {
            return GetEntryPointFullPath(activationArguments.ActivationContext);
        }

        internal static string GetFriendlyName(ActivationContext activationContext)
        {
            IMetadataSectionEntry metadataSectionEntry = (IMetadataSectionEntry) activationContext.DeploymentComponentManifest.MetadataSectionEntry;
            IDescriptionMetadataEntry descriptionData = metadataSectionEntry.DescriptionData;
            string str = string.Empty;
            if (descriptionData != null)
            {
                DescriptionMetadataEntry allData = descriptionData.AllData;
                str = (allData.Publisher != null) ? string.Format("{0} {1}", allData.Publisher, allData.Product) : allData.Product;
            }
            return str;
        }

        internal static Evidence MergeApplicationEvidence(Evidence evidence, ApplicationIdentity applicationIdentity, ActivationContext activationContext, string[] activationData)
        {
            return MergeApplicationEvidence(evidence, applicationIdentity, activationContext, activationData, null);
        }

        internal static Evidence MergeApplicationEvidence(Evidence evidence, ApplicationIdentity applicationIdentity, ActivationContext activationContext, string[] activationData, ApplicationTrust applicationTrust)
        {
            Evidence evidence2 = new Evidence();
            ActivationArguments arguments = (activationContext == null) ? new ActivationArguments(applicationIdentity, activationData) : new ActivationArguments(activationContext, activationData);
            evidence2 = new Evidence();
            evidence2.AddHostEvidence<ActivationArguments>(arguments);
            if (applicationTrust != null)
            {
                evidence2.AddHostEvidence<ApplicationTrust>(applicationTrust);
            }
            if (activationContext != null)
            {
                Evidence applicationEvidence = new ApplicationSecurityInfo(activationContext).ApplicationEvidence;
                if (applicationEvidence != null)
                {
                    evidence2.MergeWithNoDuplicates(applicationEvidence);
                }
            }
            if (evidence != null)
            {
                evidence2.MergeWithNoDuplicates(evidence);
            }
            return evidence2;
        }
    }
}

