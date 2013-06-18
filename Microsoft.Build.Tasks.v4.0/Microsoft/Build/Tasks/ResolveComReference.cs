namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using Microsoft.Build.Utilities;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;

    public sealed class ResolveComReference : AppDomainIsolatedTaskExtension, IComReferenceResolver
    {
        internal List<ComReferenceInfo> allDependencyRefs;
        internal List<ComReferenceInfo> allProjectRefs;
        private string aximpPath;
        private Hashtable cacheAx = new Hashtable();
        private Hashtable cachePia = new Hashtable();
        private Hashtable cacheTlb = new Hashtable();
        private bool delaySign;
        private bool executeAsTool = true;
        private bool includeVersionInInteropName;
        private string keyContainer;
        private string keyFile;
        private bool noClassMembers;
        private Version projectTargetFramework;
        private string projectTargetFrameworkAsString = string.Empty;
        private static readonly string[] requiredMetadataForNameItem = new string[] { "Guid", "VersionMajor", "VersionMinor" };
        private ITaskItem[] resolvedAssemblyReferences;
        private ITaskItem[] resolvedFiles;
        private ITaskItem[] resolvedModules;
        private string sdkToolsPath;
        private string stateFile;
        private static readonly Version TargetFrameworkVersion_40 = new Version("4.0");
        private string targetProcessorArchitecture;
        private ResolveComReferenceCache timestampCache;
        private string tlbimpPath;
        private ITaskItem[] typeLibFiles;
        private ITaskItem[] typeLibNames;
        private string wrapperOutputDirectory;

        internal void AddMissingTlbReferences()
        {
            List<ComReferenceInfo> list = new List<ComReferenceInfo>();
            foreach (ComReferenceInfo info in this.allProjectRefs)
            {
                if (!ComReferenceTypes.IsAxImp(info.taskItem.GetMetadata("WrapperTool")))
                {
                    continue;
                }
                bool flag = false;
                foreach (ComReferenceInfo info2 in this.allProjectRefs)
                {
                    string metadata = info2.taskItem.GetMetadata("WrapperTool");
                    if (((ComReferenceTypes.IsTlbImp(metadata) || ComReferenceTypes.IsPia(metadata)) || ComReferenceTypes.IsPiaOrTlbImp(metadata)) && ComReference.AreTypeLibAttrEqual(info.attr, info2.attr))
                    {
                        info.taskItem.SetMetadata("TlbReferenceName", info2.typeLibName);
                        string parameterValue = info2.taskItem.GetMetadata("EmbedInteropTypes");
                        if (ConversionUtilities.CanConvertStringToBool(parameterValue) && ConversionUtilities.ConvertStringToBool(parameterValue))
                        {
                            base.Log.LogMessageFromResources(MessageImportance.High, "ResolveComReference.TreatingTlbOfActiveXAsNonEmbedded", new object[] { info2.taskItem.ItemSpec, info.taskItem.ItemSpec });
                            info2.taskItem.SetMetadata("EmbedInteropTypes", "false");
                        }
                        info.primaryOfAxImpRef = info2;
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveComReference.AddingMissingTlbReference", new object[] { info.taskItem.ItemSpec });
                    ComReferenceInfo item = new ComReferenceInfo(info);
                    item.taskItem.SetMetadata("WrapperTool", "primaryortlbimp");
                    item.taskItem.SetMetadata("EmbedInteropTypes", "false");
                    info.primaryOfAxImpRef = item;
                    list.Add(item);
                    info.taskItem.SetMetadata("TlbReferenceName", item.typeLibName);
                }
            }
            foreach (ComReferenceInfo info4 in list)
            {
                this.allProjectRefs.Add(info4);
            }
        }

        internal bool CheckForConflictingReferences()
        {
            Hashtable hashtable = new Hashtable();
            ArrayList list = new ArrayList();
            bool flag = true;
            for (int i = 0; i < 2; i++)
            {
                foreach (ComReferenceInfo info in this.allProjectRefs)
                {
                    string metadata = info.taskItem.GetMetadata("WrapperTool");
                    if (((i == 0) && ComReferenceTypes.IsAxImp(metadata)) || ((i == 1) && ComReferenceTypes.IsTlbImp(metadata)))
                    {
                        if (hashtable.ContainsKey(info.typeLibName))
                        {
                            ComReferenceInfo info2 = (ComReferenceInfo) hashtable[info.typeLibName];
                            if (!ComReference.AreTypeLibAttrEqual(info.attr, info2.attr))
                            {
                                base.Log.LogWarningWithCodeFromResources("ResolveComReference.ConflictingReferences", new object[] { info.taskItem.ItemSpec, info2.taskItem.ItemSpec });
                                list.Add(info);
                                flag = false;
                            }
                        }
                        else
                        {
                            hashtable.Add(info.typeLibName, info);
                        }
                    }
                }
                hashtable.Clear();
            }
            foreach (ComReferenceInfo info3 in list)
            {
                this.allProjectRefs.Remove(info3);
                info3.ReleaseTypeLibPtr();
            }
            return flag;
        }

        private void Cleanup()
        {
            this.cacheAx.Clear();
            this.cachePia.Clear();
            this.cacheTlb.Clear();
            foreach (ComReferenceInfo info in this.allDependencyRefs)
            {
                info.ReleaseTypeLibPtr();
            }
            foreach (ComReferenceInfo info2 in this.allProjectRefs)
            {
                info2.ReleaseTypeLibPtr();
            }
        }

        private bool ComputePathToAxImp()
        {
            this.aximpPath = null;
            if (string.IsNullOrEmpty(this.sdkToolsPath))
            {
                this.aximpPath = this.GetPathToSDKFileWithCurrentlyTargetedArchitecture("AxImp.exe", TargetDotNetFrameworkVersion.Version35);
                if (this.aximpPath == null)
                {
                    base.Log.LogErrorWithCodeFromResources("General.PlatformSDKFileNotFound", new object[] { "AxImp.exe", ToolLocationHelper.GetDotNetFrameworkSdkInstallKeyValue(TargetDotNetFrameworkVersion.Version35), ToolLocationHelper.GetDotNetFrameworkSdkRootRegistryKey(TargetDotNetFrameworkVersion.Version35) });
                }
            }
            else
            {
                this.aximpPath = SdkToolsPathUtility.GeneratePathToTool(SdkToolsPathUtility.FileInfoExists, this.TargetProcessorArchitecture, this.SdkToolsPath, "AxImp.exe", base.Log, true);
            }
            if (this.aximpPath != null)
            {
                this.aximpPath = Path.GetDirectoryName(this.aximpPath);
            }
            return (this.aximpPath != null);
        }

        private bool ComputePathToTlbImp()
        {
            this.tlbimpPath = null;
            if (string.IsNullOrEmpty(this.sdkToolsPath))
            {
                this.tlbimpPath = this.GetPathToSDKFileWithCurrentlyTargetedArchitecture("TlbImp.exe", TargetDotNetFrameworkVersion.Version35);
                if ((this.tlbimpPath == null) && this.ExecuteAsTool)
                {
                    base.Log.LogErrorWithCodeFromResources("General.PlatformSDKFileNotFound", new object[] { "TlbImp.exe", ToolLocationHelper.GetDotNetFrameworkSdkInstallKeyValue(TargetDotNetFrameworkVersion.Version35), ToolLocationHelper.GetDotNetFrameworkSdkRootRegistryKey(TargetDotNetFrameworkVersion.Version35) });
                }
            }
            else
            {
                this.tlbimpPath = SdkToolsPathUtility.GeneratePathToTool(SdkToolsPathUtility.FileInfoExists, this.TargetProcessorArchitecture, this.SdkToolsPath, "TlbImp.exe", base.Log, this.ExecuteAsTool);
            }
            if ((this.tlbimpPath == null) && !this.ExecuteAsTool)
            {
                this.tlbimpPath = "TlbImp.exe";
                return true;
            }
            if (this.tlbimpPath != null)
            {
                this.tlbimpPath = Path.GetDirectoryName(this.tlbimpPath);
            }
            return (this.tlbimpPath != null);
        }

        private void ConvertAttrReferencesToComReferenceInfo(List<ComReferenceInfo> projectRefs, ITaskItem[] typeLibAttrs)
        {
            int num = (typeLibAttrs == null) ? 0 : typeLibAttrs.GetLength(0);
            for (int i = 0; i < num; i++)
            {
                ComReferenceInfo item = new ComReferenceInfo();
                try
                {
                    if (item.InitializeWithTypeLibAttrs(base.Log, TaskItemToTypeLibAttr(typeLibAttrs[i]), typeLibAttrs[i], this.TargetProcessorArchitecture))
                    {
                        projectRefs.Add(item);
                    }
                    else
                    {
                        item.ReleaseTypeLibPtr();
                    }
                }
                catch (COMException exception)
                {
                    base.Log.LogWarningWithCodeFromResources("ResolveComReference.CannotLoadTypeLibItemSpec", new object[] { typeLibAttrs[i].ItemSpec, exception.Message });
                    item.ReleaseTypeLibPtr();
                }
            }
        }

        private void ConvertFileReferencesToComReferenceInfo(List<ComReferenceInfo> projectRefs, ITaskItem[] tlbFiles)
        {
            int num = (tlbFiles == null) ? 0 : tlbFiles.GetLength(0);
            for (int i = 0; i < num; i++)
            {
                string itemSpec = tlbFiles[i].ItemSpec;
                if (!Path.IsPathRooted(itemSpec))
                {
                    itemSpec = Path.Combine(Directory.GetCurrentDirectory(), itemSpec);
                }
                ComReferenceInfo item = new ComReferenceInfo();
                try
                {
                    if (item.InitializeWithPath(base.Log, itemSpec, tlbFiles[i], this.TargetProcessorArchitecture))
                    {
                        projectRefs.Add(item);
                    }
                    else
                    {
                        item.ReleaseTypeLibPtr();
                    }
                }
                catch (COMException exception)
                {
                    base.Log.LogWarningWithCodeFromResources("ResolveComReference.CannotLoadTypeLibItemSpec", new object[] { tlbFiles[i].ItemSpec, exception.Message });
                    item.ReleaseTypeLibPtr();
                }
            }
        }

        public override bool Execute()
        {
            bool flag2;
            if (!this.VerifyAndInitializeInputs())
            {
                return false;
            }
            if (!this.ComputePathToAxImp() || !this.ComputePathToTlbImp())
            {
                return false;
            }
            this.allProjectRefs = new List<ComReferenceInfo>();
            this.allDependencyRefs = new List<ComReferenceInfo>();
            this.timestampCache = (ResolveComReferenceCache) StateFileBase.DeserializeCache(this.StateFile, base.Log, typeof(ResolveComReferenceCache));
            if ((this.timestampCache == null) || ((this.timestampCache != null) && !this.timestampCache.ToolPathsMatchCachePaths(this.tlbimpPath, this.aximpPath)))
            {
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveComReference.NotUsingCacheFile", new object[] { (this.StateFile == null) ? string.Empty : this.StateFile });
                this.timestampCache = new ResolveComReferenceCache(this.tlbimpPath, this.aximpPath);
            }
            else
            {
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveComReference.UsingCacheFile", new object[] { (this.StateFile == null) ? string.Empty : this.StateFile });
            }
            try
            {
                this.ConvertAttrReferencesToComReferenceInfo(this.allProjectRefs, this.TypeLibNames);
                this.ConvertFileReferencesToComReferenceInfo(this.allProjectRefs, this.TypeLibFiles);
                this.AddMissingTlbReferences();
                this.CheckForConflictingReferences();
                this.SetFrameworkVersionFromString(this.projectTargetFrameworkAsString);
                ArrayList moduleList = new ArrayList();
                ArrayList resolvedReferenceList = new ArrayList();
                ComDependencyWalker dependencyWalker = new ComDependencyWalker(new MarshalReleaseComObject(Marshal.ReleaseComObject));
                bool flag = true;
                for (int i = 0; i < 4; i++)
                {
                    foreach (ComReferenceInfo info in this.allProjectRefs)
                    {
                        string metadata = info.taskItem.GetMetadata("WrapperTool");
                        if ((((i == 0) && ComReferenceTypes.IsPia(metadata)) || ((i == 1) && ComReferenceTypes.IsTlbImp(metadata))) || (((i == 2) && ComReferenceTypes.IsPiaOrTlbImp(metadata)) || ((i == 3) && ComReferenceTypes.IsAxImp(metadata))))
                        {
                            try
                            {
                                if (!this.ResolveReferenceAndAddToList(dependencyWalker, info, resolvedReferenceList, moduleList))
                                {
                                    flag = false;
                                }
                            }
                            catch (ComReferenceResolutionException)
                            {
                            }
                            catch (StrongNameException)
                            {
                                return false;
                            }
                            catch (FileLoadException exception)
                            {
                                if (!this.DelaySign)
                                {
                                    throw;
                                }
                                base.Log.LogErrorWithCodeFromResources(null, info.SourceItemSpec, 0, 0, 0, 0, "ResolveComReference.LoadingDelaySignedAssemblyWithStrongNameVerificationEnabled", new object[] { exception.Message });
                                return false;
                            }
                            catch (ArgumentException exception2)
                            {
                                base.Log.LogErrorWithCodeFromResources("General.InvalidArgument", new object[] { exception2.Message });
                                return false;
                            }
                            catch (SystemException exception3)
                            {
                                base.Log.LogErrorWithCodeFromResources("ResolveComReference.FailedToResolveComReference", new object[] { info.attr.guid, info.attr.wMajorVerNum, info.attr.wMinorVerNum, exception3.Message });
                            }
                        }
                    }
                }
                this.SetCopyLocalToFalseOnGacOrNoPIAAssemblies(resolvedReferenceList, GlobalAssemblyCache.GetGacPath());
                this.ResolvedModules = (ITaskItem[]) moduleList.ToArray(typeof(ITaskItem));
                this.ResolvedFiles = (ITaskItem[]) resolvedReferenceList.ToArray(typeof(ITaskItem));
                flag2 = flag && !base.Log.HasLoggedErrors;
            }
            finally
            {
                if ((this.timestampCache != null) && this.timestampCache.Dirty)
                {
                    this.timestampCache.SerializeCache(this.StateFile, base.Log);
                }
                this.Cleanup();
            }
            return flag2;
        }

        private string GetPathToSDKFileWithCurrentlyTargetedArchitecture(string file, TargetDotNetFrameworkVersion targetFrameworkVersion)
        {
            string pathToDotNetFrameworkSdkFile = null;
            string targetProcessorArchitecture = this.TargetProcessorArchitecture;
            if (targetProcessorArchitecture != null)
            {
                if (!(targetProcessorArchitecture == "x86"))
                {
                    if ((targetProcessorArchitecture == "AMD64") || (targetProcessorArchitecture == "IA64"))
                    {
                        pathToDotNetFrameworkSdkFile = ToolLocationHelper.GetPathToDotNetFrameworkSdkFile(file, targetFrameworkVersion, Microsoft.Build.Utilities.DotNetFrameworkArchitecture.Bitness64);
                    }
                    else if (targetProcessorArchitecture == "MSIL")
                    {
                    }
                }
                else
                {
                    pathToDotNetFrameworkSdkFile = ToolLocationHelper.GetPathToDotNetFrameworkSdkFile(file, targetFrameworkVersion, Microsoft.Build.Utilities.DotNetFrameworkArchitecture.Bitness32);
                }
            }
            if (pathToDotNetFrameworkSdkFile == null)
            {
                pathToDotNetFrameworkSdkFile = ToolLocationHelper.GetPathToDotNetFrameworkSdkFile(file, targetFrameworkVersion);
            }
            return pathToDotNetFrameworkSdkFile;
        }

        internal IEnumerable<string> GetResolvedAssemblyReferenceItemSpecs()
        {
            if (this.ResolvedAssemblyReferences == null)
            {
                return new string[0];
            }
            return (from rar in this.ResolvedAssemblyReferences select rar.ItemSpec);
        }

        internal static void InitializeDefaultMetadataForFileItem(ITaskItem reference)
        {
            if (reference.GetMetadata("WrapperTool").Length == 0)
            {
                reference.SetMetadata("WrapperTool", "tlbimp");
            }
        }

        internal static void InitializeDefaultMetadataForNameItem(ITaskItem reference)
        {
            if (reference.GetMetadata("Lcid").Length == 0)
            {
                reference.SetMetadata("Lcid", "0");
            }
            if (reference.GetMetadata("WrapperTool").Length == 0)
            {
                reference.SetMetadata("WrapperTool", "tlbimp");
            }
        }

        internal bool IsExistingDependencyReference(System.Runtime.InteropServices.ComTypes.TYPELIBATTR typeLibAttr, out ComReferenceInfo referenceInfo)
        {
            foreach (ComReferenceInfo info in this.allDependencyRefs)
            {
                if (ComReference.AreTypeLibAttrEqual(info.attr, typeLibAttr))
                {
                    referenceInfo = info;
                    return true;
                }
            }
            referenceInfo = null;
            return false;
        }

        internal bool IsExistingProjectReference(System.Runtime.InteropServices.ComTypes.TYPELIBATTR typeLibAttr, string neededRefType, out ComReferenceInfo referenceInfo)
        {
            for (int i = 0; i < 3; i++)
            {
                if ((((i == 0) && (ComReferenceTypes.IsPia(neededRefType) || (neededRefType == null))) || ((i == 1) && (ComReferenceTypes.IsTlbImp(neededRefType) || (neededRefType == null)))) || ((i == 2) && ComReferenceTypes.IsAxImp(neededRefType)))
                {
                    foreach (ComReferenceInfo info in this.allProjectRefs)
                    {
                        string metadata = info.taskItem.GetMetadata("WrapperTool");
                        if (((((i == 0) && ComReferenceTypes.IsPia(metadata)) || ((i == 1) && ComReferenceTypes.IsTlbImp(metadata))) || ((i == 2) && ComReferenceTypes.IsAxImp(metadata))) && ComReference.AreTypeLibAttrEqual(info.attr, typeLibAttr))
                        {
                            referenceInfo = info;
                            return true;
                        }
                    }
                }
            }
            referenceInfo = null;
            return false;
        }

        bool IComReferenceResolver.ResolveComAssemblyReference(string fullAssemblyName, out string assemblyPath)
        {
            AssemblyNameExtension extension = new AssemblyNameExtension(fullAssemblyName);
            foreach (ComReferenceWrapperInfo info in this.cachePia.Values)
            {
                if (info.path != null)
                {
                    AssemblyNameExtension that = new AssemblyNameExtension(AssemblyName.GetAssemblyName(info.path));
                    if (extension.Equals(that))
                    {
                        assemblyPath = info.path;
                        return true;
                    }
                    if (extension.Equals(info.originalPiaName))
                    {
                        assemblyPath = info.path;
                        return true;
                    }
                }
            }
            foreach (ComReferenceWrapperInfo info2 in this.cacheTlb.Values)
            {
                if (info2.path != null)
                {
                    AssemblyNameExtension extension3 = new AssemblyNameExtension(AssemblyName.GetAssemblyName(info2.path));
                    if (extension.Equals(extension3))
                    {
                        assemblyPath = info2.path;
                        return true;
                    }
                }
            }
            foreach (ComReferenceWrapperInfo info3 in this.cacheAx.Values)
            {
                if (info3.path != null)
                {
                    AssemblyNameExtension extension4 = new AssemblyNameExtension(AssemblyName.GetAssemblyName(info3.path));
                    if (extension.Equals(extension4))
                    {
                        assemblyPath = info3.path;
                        return true;
                    }
                }
            }
            assemblyPath = null;
            return false;
        }

        bool IComReferenceResolver.ResolveComClassicReference(System.Runtime.InteropServices.ComTypes.TYPELIBATTR typeLibAttr, string outputDirectory, string wrapperType, string refName, out ComReferenceWrapperInfo wrapperInfo)
        {
            ComReferenceInfo info;
            bool topLevelRef = false;
            wrapperInfo = null;
            System.Runtime.InteropServices.ComTypes.TYPELIBATTR typelibattr = typeLibAttr;
            if (ComReference.RemapAdoTypeLib(base.Log, ref typeLibAttr))
            {
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveComReference.RemappingAdoTypeLib", new object[] { typelibattr.wMajorVerNum, typelibattr.wMinorVerNum });
            }
            if (this.IsExistingProjectReference(typeLibAttr, wrapperType, out info))
            {
                topLevelRef = true;
                wrapperType = info.taskItem.GetMetadata("WrapperTool");
            }
            else if (this.IsExistingDependencyReference(typeLibAttr, out info))
            {
                if ((wrapperType == null) || ComReferenceTypes.IsPiaOrTlbImp(wrapperType))
                {
                    string key = ComReference.UniqueKeyFromTypeLibAttr(typeLibAttr);
                    if (this.cachePia.ContainsKey(key))
                    {
                        wrapperType = "primary";
                    }
                    else if (this.cacheTlb.ContainsKey(key))
                    {
                        wrapperType = "tlbimp";
                    }
                }
            }
            else
            {
                try
                {
                    info = new ComReferenceInfo();
                    if (info.InitializeWithTypeLibAttrs(base.Log, typeLibAttr, null, this.TargetProcessorArchitecture))
                    {
                        this.allDependencyRefs.Add(info);
                    }
                    else
                    {
                        info.ReleaseTypeLibPtr();
                        return false;
                    }
                }
                catch (COMException exception)
                {
                    base.Log.LogWarningWithCodeFromResources("ResolveComReference.CannotLoadTypeLib", new object[] { typeLibAttr.guid, typeLibAttr.wMajorVerNum.ToString(CultureInfo.InvariantCulture), typeLibAttr.wMinorVerNum.ToString(CultureInfo.InvariantCulture), exception.Message });
                    info.ReleaseTypeLibPtr();
                    return false;
                }
            }
            if (refName == null)
            {
                refName = info.typeLibName;
            }
            return this.ResolveComClassicReference(info, outputDirectory, wrapperType, refName, topLevelRef, info.dependentWrapperPaths, out wrapperInfo);
        }

        bool IComReferenceResolver.ResolveNetAssemblyReference(string assemblyName, out string assemblyPath)
        {
            int index = assemblyName.IndexOf(',');
            if (index != -1)
            {
                assemblyName = assemblyName.Substring(0, index);
            }
            assemblyName = assemblyName + ".dll";
            for (int i = 0; i < this.ResolvedAssemblyReferences.GetLength(0); i++)
            {
                if (string.Compare(Path.GetFileName(this.ResolvedAssemblyReferences[i].ItemSpec), assemblyName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    assemblyPath = this.ResolvedAssemblyReferences[i].ItemSpec;
                    return true;
                }
            }
            assemblyPath = null;
            return false;
        }

        internal bool ResolveComClassicReference(ComReferenceInfo referenceInfo, string outputDirectory, string wrapperType, string refName, bool topLevelRef, List<string> dependencyPaths, out ComReferenceWrapperInfo wrapperInfo)
        {
            wrapperInfo = null;
            bool flag = false;
            if (ComReferenceTypes.IsPia(wrapperType))
            {
                flag = this.ResolveComReferencePia(referenceInfo, refName, out wrapperInfo);
            }
            else if (ComReferenceTypes.IsTlbImp(wrapperType))
            {
                flag = this.ResolveComReferenceTlb(referenceInfo, outputDirectory, refName, topLevelRef, dependencyPaths, out wrapperInfo);
            }
            else if (ComReferenceTypes.IsAxImp(wrapperType))
            {
                flag = this.ResolveComReferenceAx(referenceInfo, outputDirectory, refName, out wrapperInfo);
            }
            else if ((wrapperType == null) || ComReferenceTypes.IsPiaOrTlbImp(wrapperType))
            {
                flag = this.ResolveComReferencePia(referenceInfo, refName, out wrapperInfo);
                if (!flag)
                {
                    flag = this.ResolveComReferenceTlb(referenceInfo, outputDirectory, refName, false, dependencyPaths, out wrapperInfo);
                }
            }
            else
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(false, "Unknown wrapper type!");
            }
            referenceInfo.resolvedWrapper = wrapperInfo;
            this.timestampCache[referenceInfo.typeLibPath] = File.GetLastWriteTime(referenceInfo.typeLibPath);
            return flag;
        }

        internal bool ResolveComReferenceAx(ComReferenceInfo referenceInfo, string outputDirectory, string refName, out ComReferenceWrapperInfo wrapperInfo)
        {
            wrapperInfo = null;
            string key = ComReference.UniqueKeyFromTypeLibAttr(referenceInfo.attr);
            if (this.cacheAx.ContainsKey(key))
            {
                wrapperInfo = (ComReferenceWrapperInfo) this.cacheAx[key];
                return true;
            }
            try
            {
                AxReference reference = new AxReference(base.Log, this, referenceInfo, refName, outputDirectory, this.DelaySign, this.KeyFile, this.KeyContainer, this.IncludeVersionInInteropName, this.aximpPath, base.BuildEngine, this.EnvironmentVariables);
                if (!reference.FindExistingWrapper(out wrapperInfo, this.timestampCache[referenceInfo.typeLibPath]) && !reference.GenerateWrapper(out wrapperInfo))
                {
                    return false;
                }
                this.cacheAx.Add(key, wrapperInfo);
            }
            catch (Exception exception)
            {
                if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception))
                {
                    throw;
                }
                return false;
            }
            return true;
        }

        internal bool ResolveComReferencePia(ComReferenceInfo referenceInfo, string refName, out ComReferenceWrapperInfo wrapperInfo)
        {
            wrapperInfo = null;
            string key = ComReference.UniqueKeyFromTypeLibAttr(referenceInfo.attr);
            if (this.cachePia.ContainsKey(key))
            {
                wrapperInfo = (ComReferenceWrapperInfo) this.cachePia[key];
                return true;
            }
            try
            {
                PiaReference reference = new PiaReference(base.Log, referenceInfo, refName);
                if (!reference.FindExistingWrapper(out wrapperInfo, this.timestampCache[referenceInfo.typeLibPath]))
                {
                    return false;
                }
                this.cachePia.Add(key, wrapperInfo);
            }
            catch (Exception exception)
            {
                if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception))
                {
                    throw;
                }
                return false;
            }
            return true;
        }

        internal bool ResolveComReferenceTlb(ComReferenceInfo referenceInfo, string outputDirectory, string refName, bool topLevelRef, List<string> dependencyPaths, out ComReferenceWrapperInfo wrapperInfo)
        {
            wrapperInfo = null;
            string key = ComReference.UniqueKeyFromTypeLibAttr(referenceInfo.attr);
            if (this.cacheTlb.ContainsKey(key))
            {
                wrapperInfo = (ComReferenceWrapperInfo) this.cacheTlb[key];
                return true;
            }
            bool hasTemporaryWrapper = false;
            if (!topLevelRef)
            {
                foreach (ComReferenceInfo info in this.allProjectRefs)
                {
                    if ((ComReferenceTypes.IsTlbImp(info.taskItem.GetMetadata("WrapperTool")) && !ComReference.AreTypeLibAttrEqual(referenceInfo.attr, info.attr)) && (string.Compare(referenceInfo.typeLibName, info.typeLibName, StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        hasTemporaryWrapper = true;
                    }
                }
            }
            try
            {
                List<string> referenceFiles = new List<string>(this.GetResolvedAssemblyReferenceItemSpecs());
                if (dependencyPaths != null)
                {
                    referenceFiles.AddRange(dependencyPaths);
                }
                TlbReference reference = new TlbReference(base.Log, this, referenceFiles, referenceInfo, refName, outputDirectory, hasTemporaryWrapper, this.DelaySign, this.KeyFile, this.KeyContainer, this.NoClassMembers, this.TargetProcessorArchitecture, this.IncludeVersionInInteropName, this.ExecuteAsTool, this.tlbimpPath, base.BuildEngine, this.EnvironmentVariables);
                if (!reference.FindExistingWrapper(out wrapperInfo, this.timestampCache[referenceInfo.typeLibPath]) && !reference.GenerateWrapper(out wrapperInfo))
                {
                    return false;
                }
                this.cacheTlb.Add(key, wrapperInfo);
            }
            catch (Exception exception)
            {
                if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception))
                {
                    throw;
                }
                return false;
            }
            return true;
        }

        internal bool ResolveReference(ComDependencyWalker dependencyWalker, ComReferenceInfo referenceInfo, string outputDirectory, out ITaskItem referencePathItem)
        {
            if (referenceInfo.referencePathItem == null)
            {
                ComReferenceWrapperInfo info;
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveComReference.Resolving", new object[] { referenceInfo.taskItem.ItemSpec, referenceInfo.taskItem.GetMetadata("WrapperTool") });
                List<string> list = this.ScanAndResolveAllDependencies(dependencyWalker, referenceInfo);
                referenceInfo.dependentWrapperPaths = list;
                referencePathItem = new TaskItem();
                referenceInfo.referencePathItem = referencePathItem;
                if (this.ResolveComClassicReference(referenceInfo, outputDirectory, referenceInfo.taskItem.GetMetadata("WrapperTool"), referenceInfo.taskItem.ItemSpec, true, referenceInfo.dependentWrapperPaths, out info))
                {
                    referencePathItem.ItemSpec = info.path;
                    referenceInfo.taskItem.CopyMetadataTo(referencePathItem);
                    string fullName = AssemblyName.GetAssemblyName(info.path).FullName;
                    referencePathItem.SetMetadata("FusionName", fullName);
                    base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveComReference.ResolvedReference", new object[] { referenceInfo.taskItem.ItemSpec, info.path });
                    return true;
                }
                base.Log.LogWarningWithCodeFromResources("ResolveComReference.CannotFindWrapperForTypeLib", new object[] { referenceInfo.taskItem.ItemSpec });
                return false;
            }
            bool flag = !string.IsNullOrEmpty(referenceInfo.referencePathItem.ItemSpec);
            referencePathItem = referenceInfo.referencePathItem;
            return flag;
        }

        private bool ResolveReferenceAndAddToList(ComDependencyWalker dependencyWalker, ComReferenceInfo projectRefInfo, ArrayList resolvedReferenceList, ArrayList moduleList)
        {
            ITaskItem item;
            if (!this.ResolveReference(dependencyWalker, projectRefInfo, this.WrapperOutputDirectory, out item))
            {
                return false;
            }
            resolvedReferenceList.Add(item);
            bool metadataFound = false;
            bool flag2 = MetadataConversionUtilities.TryConvertItemMetadataToBool(projectRefInfo.taskItem, "Isolated", out metadataFound);
            if (metadataFound && flag2)
            {
                string typeLibPath = projectRefInfo.typeLibPath;
                if (typeLibPath == null)
                {
                    return false;
                }
                ITaskItem item2 = new TaskItem(typeLibPath);
                item2.SetMetadata("Name", projectRefInfo.taskItem.ItemSpec);
                moduleList.Add(item2);
            }
            return true;
        }

        private List<string> ScanAndResolveAllDependencies(ComDependencyWalker dependencyWalker, ComReferenceInfo reference)
        {
            dependencyWalker.ClearDependencyList();
            base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveComReference.ScanningDependencies", new object[] { reference.SourceItemSpec });
            dependencyWalker.AnalyzeTypeLibrary(reference.typeLibPointer);
            foreach (Exception exception in dependencyWalker.EncounteredProblems)
            {
                base.Log.LogWarningWithCodeFromResources("ResolveComReference.FailedToScanDependencies", new object[] { reference.SourceItemSpec, exception.Message });
            }
            dependencyWalker.EncounteredProblems.Clear();
            HashSet<string> source = new HashSet<string>();
            foreach (System.Runtime.InteropServices.ComTypes.TYPELIBATTR typelibattr in dependencyWalker.GetDependencies())
            {
                if (!ComReference.AreTypeLibAttrEqual(typelibattr, reference.attr))
                {
                    ComReferenceInfo info;
                    if (this.IsExistingProjectReference(typelibattr, null, out info))
                    {
                        ITaskItem item;
                        dependencyWalker.ClearAnalyzedTypeCache();
                        if (this.ResolveReference(dependencyWalker, info, this.WrapperOutputDirectory, out item))
                        {
                            source.Add(item.ItemSpec);
                            foreach (string str in info.dependentWrapperPaths)
                            {
                                source.Add(str);
                            }
                        }
                    }
                    else
                    {
                        ComReferenceWrapperInfo info2;
                        base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveComReference.ResolvingDependency", new object[] { typelibattr.guid, typelibattr.wMajorVerNum, typelibattr.wMinorVerNum });
                        ((IComReferenceResolver) this).ResolveComClassicReference(typelibattr, this.WrapperOutputDirectory, null, null, out info2);
                        base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveComReference.ResolvedDependentComReference", new object[] { typelibattr.guid, typelibattr.wMajorVerNum, typelibattr.wMinorVerNum, info2.path });
                        source.Add(info2.path);
                    }
                }
            }
            return source.ToList<string>();
        }

        internal void SetCopyLocalToFalseOnGacOrNoPIAAssemblies(ArrayList outputTaskItems, string gacPath)
        {
            foreach (ITaskItem item in outputTaskItems)
            {
                string metadata = item.GetMetadata("EmbedInteropTypes");
                if (((this.projectTargetFramework != null) && (this.projectTargetFramework >= TargetFrameworkVersion_40)) && ((metadata != null) && (string.Compare(metadata, "true", StringComparison.OrdinalIgnoreCase) == 0)))
                {
                    item.SetMetadata("CopyLocal", "false");
                }
                else
                {
                    string metadataValue = item.GetMetadata("Private");
                    if ((metadataValue == null) || (metadataValue.Length == 0))
                    {
                        if (string.Compare(item.ItemSpec, 0, gacPath, 0, gacPath.Length, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            item.SetMetadata("CopyLocal", "false");
                        }
                        else
                        {
                            item.SetMetadata("CopyLocal", "true");
                        }
                    }
                    else
                    {
                        item.SetMetadata("CopyLocal", metadataValue);
                    }
                }
            }
        }

        internal void SetFrameworkVersionFromString(string version)
        {
            Version version2 = null;
            if (!string.IsNullOrEmpty(version))
            {
                version2 = Microsoft.Build.Tasks.VersionUtilities.ConvertToVersion(version);
                if (version2 == null)
                {
                    base.Log.LogMessageFromResources(MessageImportance.Normal, "ResolveComReference.BadTargetFrameworkFormat", new object[] { version });
                }
            }
            this.projectTargetFramework = version2;
        }

        internal static System.Runtime.InteropServices.ComTypes.TYPELIBATTR TaskItemToTypeLibAttr(ITaskItem taskItem)
        {
            return new System.Runtime.InteropServices.ComTypes.TYPELIBATTR { guid = new Guid(taskItem.GetMetadata("Guid")), wMajorVerNum = short.Parse(taskItem.GetMetadata("VersionMajor"), NumberStyles.Integer, CultureInfo.InvariantCulture), wMinorVerNum = short.Parse(taskItem.GetMetadata("VersionMinor"), NumberStyles.Integer, CultureInfo.InvariantCulture), lcid = int.Parse(taskItem.GetMetadata("Lcid"), NumberStyles.Integer, CultureInfo.InvariantCulture) };
        }

        private bool VerifyAndInitializeInputs()
        {
            if (((this.KeyContainer != null) && (this.KeyContainer.Length != 0)) && ((this.KeyFile != null) && (this.KeyFile.Length != 0)))
            {
                base.Log.LogErrorWithCodeFromResources("ResolveComReference.CannotSpecifyBothKeyFileAndKeyContainer", new object[0]);
                return false;
            }
            if ((this.DelaySign && ((this.KeyContainer == null) || (this.KeyContainer.Length == 0))) && ((this.KeyFile == null) || (this.KeyFile.Length == 0)))
            {
                base.Log.LogErrorWithCodeFromResources("ResolveComReference.CannotSpecifyDelaySignWithoutEitherKeyFileAndKeyContainer", new object[0]);
                return false;
            }
            if (this.WrapperOutputDirectory == null)
            {
                this.WrapperOutputDirectory = string.Empty;
            }
            int num = (this.TypeLibNames == null) ? 0 : this.TypeLibNames.GetLength(0);
            int num2 = (this.TypeLibFiles == null) ? 0 : this.TypeLibFiles.GetLength(0);
            if ((num2 + num) == 0)
            {
                base.Log.LogErrorWithCodeFromResources("ResolveComReference.NoComReferencesSpecified", new object[0]);
                return false;
            }
            bool flag = true;
            for (int i = 0; i < num; i++)
            {
                string str;
                if (!VerifyReferenceMetadataForNameItem(this.TypeLibNames[i], out str))
                {
                    base.Log.LogErrorWithCodeFromResources(null, this.TypeLibNames[i].ItemSpec, 0, 0, 0, 0, "ResolveComReference.MissingOrUnknownComReferenceAttribute", new object[] { str, this.TypeLibNames[i].ItemSpec });
                    flag = false;
                }
                else
                {
                    InitializeDefaultMetadataForNameItem(this.TypeLibNames[i]);
                }
            }
            for (int j = 0; j < num2; j++)
            {
                InitializeDefaultMetadataForFileItem(this.TypeLibFiles[j]);
            }
            return flag;
        }

        internal static bool VerifyReferenceMetadataForNameItem(ITaskItem reference, out string missingOrInvalidMetadata)
        {
            missingOrInvalidMetadata = "";
            foreach (string str in requiredMetadataForNameItem)
            {
                if (reference.GetMetadata(str).Length == 0)
                {
                    missingOrInvalidMetadata = str;
                    return false;
                }
            }
            try
            {
                new Guid(reference.GetMetadata("Guid"));
            }
            catch (FormatException)
            {
                missingOrInvalidMetadata = "Guid";
                return false;
            }
            try
            {
                missingOrInvalidMetadata = "VersionMajor";
                short.Parse(reference.GetMetadata("VersionMajor"), NumberStyles.Integer, CultureInfo.InvariantCulture);
                missingOrInvalidMetadata = "VersionMinor";
                short.Parse(reference.GetMetadata("VersionMinor"), NumberStyles.Integer, CultureInfo.InvariantCulture);
                if (reference.GetMetadata("Lcid").Length > 0)
                {
                    missingOrInvalidMetadata = "Lcid";
                    int.Parse(reference.GetMetadata("Lcid"), NumberStyles.Integer, CultureInfo.InvariantCulture);
                }
                if (reference.GetMetadata("WrapperTool").Length > 0)
                {
                    missingOrInvalidMetadata = "WrapperTool";
                    string metadata = reference.GetMetadata("WrapperTool");
                    if ((!ComReferenceTypes.IsAxImp(metadata) && !ComReferenceTypes.IsTlbImp(metadata)) && !ComReferenceTypes.IsPia(metadata))
                    {
                        return false;
                    }
                }
            }
            catch (OverflowException)
            {
                return false;
            }
            catch (FormatException)
            {
                return false;
            }
            missingOrInvalidMetadata = string.Empty;
            return true;
        }

        public bool DelaySign
        {
            get
            {
                return this.delaySign;
            }
            set
            {
                this.delaySign = value;
            }
        }

        public string[] EnvironmentVariables { get; set; }

        public bool ExecuteAsTool
        {
            get
            {
                return this.executeAsTool;
            }
            set
            {
                this.executeAsTool = value;
            }
        }

        public bool IncludeVersionInInteropName
        {
            get
            {
                return this.includeVersionInInteropName;
            }
            set
            {
                this.includeVersionInInteropName = value;
            }
        }

        public string KeyContainer
        {
            get
            {
                return this.keyContainer;
            }
            set
            {
                this.keyContainer = value;
            }
        }

        public string KeyFile
        {
            get
            {
                return this.keyFile;
            }
            set
            {
                this.keyFile = value;
            }
        }

        public bool NoClassMembers
        {
            get
            {
                return this.noClassMembers;
            }
            set
            {
                this.noClassMembers = value;
            }
        }

        public ITaskItem[] ResolvedAssemblyReferences
        {
            get
            {
                return this.resolvedAssemblyReferences;
            }
            set
            {
                this.resolvedAssemblyReferences = value;
            }
        }

        [Output]
        public ITaskItem[] ResolvedFiles
        {
            get
            {
                return this.resolvedFiles;
            }
            set
            {
                this.resolvedFiles = value;
            }
        }

        [Output]
        public ITaskItem[] ResolvedModules
        {
            get
            {
                return this.resolvedModules;
            }
            set
            {
                this.resolvedModules = value;
            }
        }

        public string SdkToolsPath
        {
            get
            {
                return this.sdkToolsPath;
            }
            set
            {
                this.sdkToolsPath = value;
            }
        }

        public string StateFile
        {
            get
            {
                return this.stateFile;
            }
            set
            {
                this.stateFile = value;
            }
        }

        public string TargetFrameworkVersion
        {
            get
            {
                return this.projectTargetFrameworkAsString;
            }
            set
            {
                this.projectTargetFrameworkAsString = value;
            }
        }

        public string TargetProcessorArchitecture
        {
            get
            {
                return this.targetProcessorArchitecture;
            }
            set
            {
                if ("x86".Equals(value, StringComparison.OrdinalIgnoreCase))
                {
                    this.targetProcessorArchitecture = "x86";
                }
                else if ("MSIL".Equals(value, StringComparison.OrdinalIgnoreCase))
                {
                    this.targetProcessorArchitecture = "MSIL";
                }
                else if ("AMD64".Equals(value, StringComparison.OrdinalIgnoreCase))
                {
                    this.targetProcessorArchitecture = "AMD64";
                }
                else if ("IA64".Equals(value, StringComparison.OrdinalIgnoreCase))
                {
                    this.targetProcessorArchitecture = "IA64";
                }
                else
                {
                    this.targetProcessorArchitecture = value;
                }
            }
        }

        public ITaskItem[] TypeLibFiles
        {
            get
            {
                return this.typeLibFiles;
            }
            set
            {
                this.typeLibFiles = value;
            }
        }

        public ITaskItem[] TypeLibNames
        {
            get
            {
                return this.typeLibNames;
            }
            set
            {
                this.typeLibNames = value;
            }
        }

        public string WrapperOutputDirectory
        {
            get
            {
                return this.wrapperOutputDirectory;
            }
            set
            {
                this.wrapperOutputDirectory = value;
            }
        }

        internal class AxImp : AxTlbBaseTask
        {
            protected internal override void AddCommandLineCommands(CommandLineBuilderExtension commandLine)
            {
                commandLine.AppendFileNameIfNotNull(this.ActiveXControlName);
                commandLine.AppendWhenTrue("/nologo", base.Bag, "NoLogo");
                commandLine.AppendSwitchIfNotNull("/out:", this.OutputAssembly);
                commandLine.AppendSwitchIfNotNull("/rcw:", this.RuntimeCallableWrapperAssembly);
                commandLine.AppendWhenTrue("/silent", base.Bag, "Silent");
                commandLine.AppendWhenTrue("/source", base.Bag, "GenerateSource");
                commandLine.AppendWhenTrue("/verbose", base.Bag, "Verbose");
                base.AddCommandLineCommands(commandLine);
            }

            protected override bool ValidateParameters()
            {
                if (string.IsNullOrEmpty(this.ActiveXControlName))
                {
                    base.Log.LogErrorWithCodeFromResources("AxImp.NoInputFileSpecified", new object[0]);
                    return false;
                }
                return base.ValidateParameters();
            }

            public string ActiveXControlName
            {
                get
                {
                    return (string) base.Bag["ActiveXControlName"];
                }
                set
                {
                    base.Bag["ActiveXControlName"] = value;
                }
            }

            public bool GenerateSource
            {
                get
                {
                    return base.GetBoolParameterWithDefault("GenerateSource", false);
                }
                set
                {
                    base.Bag["GenerateSource"] = value;
                }
            }

            public bool NoLogo
            {
                get
                {
                    return base.GetBoolParameterWithDefault("NoLogo", false);
                }
                set
                {
                    base.Bag["NoLogo"] = value;
                }
            }

            public string OutputAssembly
            {
                get
                {
                    return (string) base.Bag["OutputAssembly"];
                }
                set
                {
                    base.Bag["OutputAssembly"] = value;
                }
            }

            public string RuntimeCallableWrapperAssembly
            {
                get
                {
                    return (string) base.Bag["RuntimeCallableWrapperAssembly"];
                }
                set
                {
                    base.Bag["RuntimeCallableWrapperAssembly"] = value;
                }
            }

            public bool Silent
            {
                get
                {
                    return base.GetBoolParameterWithDefault("Silent", false);
                }
                set
                {
                    base.Bag["Silent"] = value;
                }
            }

            protected override string ToolName
            {
                get
                {
                    return "AxImp.exe";
                }
            }

            public bool Verbose
            {
                get
                {
                    return base.GetBoolParameterWithDefault("Verbose", false);
                }
                set
                {
                    base.Bag["Verbose"] = value;
                }
            }
        }

        internal class TlbImp : AxTlbBaseTask
        {
            protected internal override void AddCommandLineCommands(CommandLineBuilderExtension commandLine)
            {
                commandLine.AppendFileNameIfNotNull(this.TypeLibName);
                commandLine.AppendSwitchIfNotNull("/asmversion:", (this.AssemblyVersion != null) ? this.AssemblyVersion.ToString() : null);
                commandLine.AppendSwitchIfNotNull("/namespace:", this.AssemblyNamespace);
                commandLine.AppendSwitchIfNotNull("/machine:", this.Machine);
                commandLine.AppendWhenTrue("/noclassmembers", base.Bag, "PreventClassMembers");
                commandLine.AppendWhenTrue("/nologo", base.Bag, "NoLogo");
                commandLine.AppendSwitchIfNotNull("/out:", this.OutputAssembly);
                commandLine.AppendWhenTrue("/silent", base.Bag, "Silent");
                commandLine.AppendWhenTrue("/sysarray", base.Bag, "SafeArrayAsSystemArray");
                commandLine.AppendSwitchIfNotNull("/transform:", this.ConvertTransformFlagsToCommandLineCommand(this.Transform));
                commandLine.AppendWhenTrue("/verbose", base.Bag, "Verbose");
                if (this.ReferenceFiles != null)
                {
                    foreach (string str in this.ReferenceFiles)
                    {
                        commandLine.AppendSwitchIfNotNull("/reference:", str);
                    }
                }
                base.AddCommandLineCommands(commandLine);
            }

            private string ConvertTransformFlagsToCommandLineCommand(ResolveComReference.TlbImpTransformFlags flags)
            {
                switch (flags)
                {
                    case ResolveComReference.TlbImpTransformFlags.None:
                        return null;

                    case ResolveComReference.TlbImpTransformFlags.TransformDispRetVals:
                        return "DispRet";

                    case ResolveComReference.TlbImpTransformFlags.SerializableValueClasses:
                        return "SerializableValueClasses";
                }
                return null;
            }

            private ResolveComReference.TlbImpTransformFlags GetTlbImpTransformFlagsParameterWithDefault(string parameterName, ResolveComReference.TlbImpTransformFlags defaultValue)
            {
                object obj2 = base.Bag[parameterName];
                if (obj2 != null)
                {
                    return (ResolveComReference.TlbImpTransformFlags) obj2;
                }
                return defaultValue;
            }

            protected override bool ValidateParameters()
            {
                if (string.IsNullOrEmpty(this.TypeLibName))
                {
                    base.Log.LogErrorWithCodeFromResources("TlbImp.NoInputFileSpecified", new object[0]);
                    return false;
                }
                if (!this.ValidateTransformFlags())
                {
                    base.Log.LogErrorWithCodeFromResources("TlbImp.InvalidTransformParameter", new object[] { this.Transform.ToString() });
                    return false;
                }
                return base.ValidateParameters();
            }

            private bool ValidateTransformFlags()
            {
                switch (this.Transform)
                {
                    case ResolveComReference.TlbImpTransformFlags.None:
                        return true;

                    case ResolveComReference.TlbImpTransformFlags.TransformDispRetVals:
                        return true;

                    case ResolveComReference.TlbImpTransformFlags.SerializableValueClasses:
                        return true;
                }
                return false;
            }

            public string AssemblyNamespace
            {
                get
                {
                    return (string) base.Bag["AssemblyNamespace"];
                }
                set
                {
                    base.Bag["AssemblyNamespace"] = value;
                }
            }

            public Version AssemblyVersion
            {
                get
                {
                    return (Version) base.Bag["AssemblyVersion"];
                }
                set
                {
                    base.Bag["AssemblyVersion"] = value;
                }
            }

            public string Machine
            {
                get
                {
                    return (string) base.Bag["Machine"];
                }
                set
                {
                    base.Bag["Machine"] = value;
                }
            }

            public bool NoLogo
            {
                get
                {
                    return base.GetBoolParameterWithDefault("NoLogo", false);
                }
                set
                {
                    base.Bag["NoLogo"] = value;
                }
            }

            public string OutputAssembly
            {
                get
                {
                    return (string) base.Bag["OutputAssembly"];
                }
                set
                {
                    base.Bag["OutputAssembly"] = value;
                }
            }

            public bool PreventClassMembers
            {
                get
                {
                    return base.GetBoolParameterWithDefault("PreventClassMembers", false);
                }
                set
                {
                    base.Bag["PreventClassMembers"] = value;
                }
            }

            public string[] ReferenceFiles
            {
                get
                {
                    return (string[]) base.Bag["ReferenceFiles"];
                }
                set
                {
                    base.Bag["ReferenceFiles"] = value;
                }
            }

            public bool SafeArrayAsSystemArray
            {
                get
                {
                    return base.GetBoolParameterWithDefault("SafeArrayAsSystemArray", false);
                }
                set
                {
                    base.Bag["SafeArrayAsSystemArray"] = value;
                }
            }

            public bool Silent
            {
                get
                {
                    return base.GetBoolParameterWithDefault("Silent", false);
                }
                set
                {
                    base.Bag["Silent"] = value;
                }
            }

            protected override string ToolName
            {
                get
                {
                    return "TlbImp.exe";
                }
            }

            public ResolveComReference.TlbImpTransformFlags Transform
            {
                get
                {
                    return this.GetTlbImpTransformFlagsParameterWithDefault("Transform", ResolveComReference.TlbImpTransformFlags.None);
                }
                set
                {
                    base.Bag["Transform"] = value;
                }
            }

            public string TypeLibName
            {
                get
                {
                    return (string) base.Bag["TypeLibName"];
                }
                set
                {
                    base.Bag["TypeLibName"] = value;
                }
            }

            public bool Verbose
            {
                get
                {
                    return base.GetBoolParameterWithDefault("Verbose", false);
                }
                set
                {
                    base.Bag["Verbose"] = value;
                }
            }
        }

        internal enum TlbImpTransformFlags
        {
            None,
            TransformDispRetVals,
            SerializableValueClasses
        }
    }
}

