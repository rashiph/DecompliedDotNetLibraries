namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using Microsoft.Build.Utilities;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;

    internal class TlbReference : AxTlbBaseReference, ITypeLibImporterNotifySink
    {
        private bool hasTemporaryWrapper;
        private bool noClassMembers;
        private IEnumerable<string> referenceFiles;
        private string targetProcessorArchitecture;

        internal TlbReference(TaskLoggingHelper taskLoggingHelper, IComReferenceResolver resolverCallback, IEnumerable<string> referenceFiles, ComReferenceInfo referenceInfo, string itemName, string outputDirectory, bool hasTemporaryWrapper, bool delaySign, string keyFile, string keyContainer, bool noClassMembers, string targetProcessorArchitecture, bool includeTypeLibVersionInName, bool executeAsTool, string sdkToolsPath, IBuildEngine buildEngine, string[] environmentVariables) : base(taskLoggingHelper, resolverCallback, referenceInfo, itemName, outputDirectory, delaySign, keyFile, keyContainer, includeTypeLibVersionInName, executeAsTool, sdkToolsPath, buildEngine, environmentVariables)
        {
            this.hasTemporaryWrapper = hasTemporaryWrapper;
            this.noClassMembers = noClassMembers;
            this.targetProcessorArchitecture = targetProcessorArchitecture;
            this.referenceFiles = referenceFiles;
        }

        internal override bool FindExistingWrapper(out ComReferenceWrapperInfo wrapperInfo, DateTime componentTimestamp)
        {
            if (!this.HasTemporaryWrapper)
            {
                return base.FindExistingWrapper(out wrapperInfo, componentTimestamp);
            }
            wrapperInfo = null;
            return false;
        }

        internal bool GenerateWrapper(out ComReferenceWrapperInfo wrapperInfo)
        {
            wrapperInfo = null;
            string typeLibName = this.ReferenceInfo.typeLibName;
            string wrapperPath = base.GetWrapperPath();
            StrongNameKeyPair keyPair = null;
            byte[] publicKey = null;
            StrongNameUtils.GetStrongNameKey(base.Log, base.KeyFile, base.KeyContainer, out keyPair, out publicKey);
            if (base.DelaySign)
            {
                keyPair = null;
                if (publicKey == null)
                {
                    base.Log.LogErrorWithCodeFromResources(null, this.ReferenceInfo.SourceItemSpec, 0, 0, 0, 0, "StrongNameUtils.NoPublicKeySpecified", new object[0]);
                    throw new StrongNameException();
                }
            }
            else
            {
                publicKey = null;
                if (keyPair == null)
                {
                    if ((base.KeyContainer != null) && (base.KeyContainer.Length > 0))
                    {
                        base.Log.LogErrorWithCodeFromResources(null, this.ReferenceInfo.SourceItemSpec, 0, 0, 0, 0, "ResolveComReference.StrongNameUtils.NoKeyPairInContainer", new object[] { base.KeyContainer });
                        throw new StrongNameException();
                    }
                    if ((base.KeyFile != null) && (base.KeyFile.Length > 0))
                    {
                        base.Log.LogErrorWithCodeFromResources(null, this.ReferenceInfo.SourceItemSpec, 0, 0, 0, 0, "ResolveComReference.StrongNameUtils.NoKeyPairInFile", new object[] { base.KeyFile });
                        throw new StrongNameException();
                    }
                }
            }
            bool flag = true;
            if (!base.ExecuteAsTool)
            {
                TypeLibConverter converter = new TypeLibConverter();
                AssemblyBuilder assemblyBuilder = null;
                try
                {
                    TypeLibImporterFlags flags = TypeLibImporterFlags.TransformDispRetVals | TypeLibImporterFlags.SafeArrayAsSystemArray;
                    if (this.noClassMembers)
                    {
                        flags |= TypeLibImporterFlags.PreventClassMembers;
                    }
                    string str4 = this.targetProcessorArchitecture;
                    if (str4 != null)
                    {
                        if (!(str4 == "MSIL"))
                        {
                            if (str4 == "AMD64")
                            {
                                goto Label_0323;
                            }
                            if (str4 == "IA64")
                            {
                                goto Label_032F;
                            }
                            if (str4 == "x86")
                            {
                                goto Label_033B;
                            }
                        }
                        else
                        {
                            flags |= TypeLibImporterFlags.ImportAsAgnostic;
                        }
                    }
                    goto Label_0345;
                Label_0323:
                    flags |= TypeLibImporterFlags.ImportAsX64;
                    goto Label_0345;
                Label_032F:
                    flags |= TypeLibImporterFlags.ImportAsItanium;
                    goto Label_0345;
                Label_033B:
                    flags |= TypeLibImporterFlags.ImportAsX86;
                Label_0345:
                    assemblyBuilder = converter.ConvertTypeLibToAssembly(this.ReferenceInfo.typeLibPointer, wrapperPath, flags, this, publicKey, keyPair, typeLibName, null);
                }
                catch (COMException exception)
                {
                    base.Log.LogWarningWithCodeFromResources("ResolveComReference.ErrorCreatingWrapperAssembly", new object[] { this.ItemName, exception.Message });
                    throw new ComReferenceResolutionException(exception);
                }
                if (!this.HasTemporaryWrapper)
                {
                    this.WriteWrapperToDisk(assemblyBuilder, wrapperPath);
                }
                wrapperInfo = new ComReferenceWrapperInfo();
                wrapperInfo.path = this.HasTemporaryWrapper ? null : wrapperPath;
                wrapperInfo.assembly = assemblyBuilder;
                return flag;
            }
            ResolveComReference.TlbImp imp = new ResolveComReference.TlbImp {
                BuildEngine = base.BuildEngine,
                EnvironmentVariables = base.EnvironmentVariables,
                DelaySign = base.DelaySign,
                KeyContainer = base.KeyContainer,
                KeyFile = base.KeyFile,
                OutputAssembly = wrapperPath,
                ToolPath = base.ToolPath,
                TypeLibName = this.ReferenceInfo.typeLibPath,
                AssemblyNamespace = typeLibName,
                AssemblyVersion = null,
                PreventClassMembers = this.noClassMembers,
                SafeArrayAsSystemArray = true,
                Transform = ResolveComReference.TlbImpTransformFlags.TransformDispRetVals
            };
            if (this.referenceFiles != null)
            {
                string fullPathToOutput = Path.GetFullPath(wrapperPath);
                imp.ReferenceFiles = (from rf in this.referenceFiles
                    where string.Compare(fullPathToOutput, rf, StringComparison.OrdinalIgnoreCase) != 0
                    select rf).ToArray<string>();
            }
            string targetProcessorArchitecture = this.targetProcessorArchitecture;
            if (targetProcessorArchitecture != null)
            {
                if (!(targetProcessorArchitecture == "MSIL"))
                {
                    if (targetProcessorArchitecture == "AMD64")
                    {
                        imp.Machine = "X64";
                    }
                    else if (targetProcessorArchitecture == "IA64")
                    {
                        imp.Machine = "Itanium";
                    }
                    else if (targetProcessorArchitecture == "x86")
                    {
                        imp.Machine = "X86";
                    }
                    else
                    {
                        imp.Machine = this.targetProcessorArchitecture;
                    }
                }
                else
                {
                    imp.Machine = "Agnostic";
                }
            }
            flag = imp.Execute();
            wrapperInfo = new ComReferenceWrapperInfo();
            wrapperInfo.path = this.HasTemporaryWrapper ? null : wrapperPath;
            return flag;
        }

        internal static string GetWrapperFileName(string typeLibName)
        {
            return GetWrapperFileName(typeLibName, false, 1, 0);
        }

        internal static string GetWrapperFileName(string typeLibName, bool includeTypeLibVersionInName, short majorVerNum, short minorVerNum)
        {
            return AxTlbBaseReference.GetWrapperFileName("Interop.", typeLibName, includeTypeLibVersionInName, majorVerNum, minorVerNum);
        }

        protected override string GetWrapperFileNameInternal(string typeLibName)
        {
            return AxTlbBaseReference.GetWrapperFileName("Interop.", typeLibName, base.IncludeTypeLibVersionInName, this.ReferenceInfo.attr.wMajorVerNum, this.ReferenceInfo.attr.wMinorVerNum);
        }

        void ITypeLibImporterNotifySink.ReportEvent(ImporterEventKind eventKind, int eventCode, string eventMsg)
        {
            if (eventKind == ImporterEventKind.ERROR_REFTOINVALIDTYPELIB)
            {
                base.Log.LogWarning(eventMsg, new object[0]);
            }
            else if (eventKind == ImporterEventKind.NOTIF_CONVERTWARNING)
            {
                base.Log.LogWarning(eventMsg, new object[0]);
            }
            else if (eventKind == ImporterEventKind.NOTIF_TYPECONVERTED)
            {
                base.Log.LogMessage(MessageImportance.Low, eventMsg, new object[0]);
            }
            else
            {
                base.Log.LogMessage(MessageImportance.Low, eventMsg, new object[0]);
            }
        }

        Assembly ITypeLibImporterNotifySink.ResolveRef(object objTypeLib)
        {
            System.Runtime.InteropServices.ComTypes.TYPELIBATTR typelibattr;
            ComReferenceWrapperInfo info;
            ITypeLib typeLib = (ITypeLib) objTypeLib;
            ComReference.GetTypeLibAttrForTypeLib(ref typeLib, out typelibattr);
            if (!base.ResolverCallback.ResolveComClassicReference(typelibattr, base.OutputDirectory, null, null, out info))
            {
                base.Log.LogWarningWithCodeFromResources("ResolveComReference.FailedToResolveDependentComReference", new object[] { typelibattr.guid, typelibattr.wMajorVerNum, typelibattr.wMinorVerNum });
                throw new ComReferenceResolutionException();
            }
            if (info.assembly == null)
            {
                throw new ComReferenceResolutionException();
            }
            base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveComReference.ResolvedDependentComReference", new object[] { typelibattr.guid, typelibattr.wMajorVerNum, typelibattr.wMinorVerNum, info.path });
            return info.assembly;
        }

        private void WriteWrapperToDisk(AssemblyBuilder assemblyBuilder, string wrapperPath)
        {
            try
            {
                FileInfo info = new FileInfo(wrapperPath);
                if (info.Exists)
                {
                    info.Delete();
                }
                string targetProcessorArchitecture = this.targetProcessorArchitecture;
                if (targetProcessorArchitecture == null)
                {
                    goto Label_0091;
                }
                if (!(targetProcessorArchitecture == "x86"))
                {
                    if (targetProcessorArchitecture == "AMD64")
                    {
                        goto Label_0069;
                    }
                    if (targetProcessorArchitecture == "IA64")
                    {
                        goto Label_007D;
                    }
                    if (targetProcessorArchitecture == "MSIL")
                    {
                    }
                    goto Label_0091;
                }
                assemblyBuilder.Save(info.Name, PortableExecutableKinds.Required32Bit | PortableExecutableKinds.ILOnly, ImageFileMachine.I386);
                goto Label_009D;
            Label_0069:
                assemblyBuilder.Save(info.Name, PortableExecutableKinds.PE32Plus | PortableExecutableKinds.ILOnly, ImageFileMachine.AMD64);
                goto Label_009D;
            Label_007D:
                assemblyBuilder.Save(info.Name, PortableExecutableKinds.PE32Plus | PortableExecutableKinds.ILOnly, ImageFileMachine.IA64);
                goto Label_009D;
            Label_0091:
                assemblyBuilder.Save(info.Name);
            Label_009D:
                File.GetLastWriteTime(wrapperPath);
            }
            catch (Exception exception)
            {
                if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception))
                {
                    throw;
                }
                base.Log.LogWarningWithCodeFromResources("ResolveComReference.ErrorCreatingWrapperAssembly", new object[] { this.ItemName, exception.Message });
                throw new ComReferenceResolutionException(exception);
            }
        }

        private bool HasTemporaryWrapper
        {
            get
            {
                return this.hasTemporaryWrapper;
            }
        }

        protected override string OutputDirectory
        {
            get
            {
                if (!this.HasTemporaryWrapper)
                {
                    return base.OutputDirectory;
                }
                return Path.GetTempPath();
            }
        }
    }
}

