namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal class AxReference : AxTlbBaseReference
    {
        internal AxReference(TaskLoggingHelper taskLoggingHelper, IComReferenceResolver resolverCallback, ComReferenceInfo referenceInfo, string itemName, string outputDirectory, bool delaySign, string keyFile, string keyContainer, bool includeTypeLibVersionInName, string sdkToolsPath, IBuildEngine buildEngine, string[] environmentVariables) : base(taskLoggingHelper, resolverCallback, referenceInfo, itemName, outputDirectory, delaySign, keyFile, keyContainer, includeTypeLibVersionInName, true, sdkToolsPath, buildEngine, environmentVariables)
        {
        }

        internal bool GenerateWrapper(out ComReferenceWrapperInfo wrapperInfo)
        {
            wrapperInfo = null;
            StrongNameKeyPair keyPair = null;
            byte[] publicKey = null;
            StrongNameUtils.GetStrongNameKey(base.Log, base.KeyFile, base.KeyContainer, out keyPair, out publicKey);
            if (!base.DelaySign && (keyPair == null))
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
            bool flag = true;
            this.ReferenceInfo.taskItem.GetMetadata("TlbReferenceName");
            ResolveComReference.AxImp imp = new ResolveComReference.AxImp {
                ActiveXControlName = this.ReferenceInfo.typeLibPath,
                BuildEngine = base.BuildEngine,
                ToolPath = base.ToolPath,
                EnvironmentVariables = base.EnvironmentVariables,
                DelaySign = base.DelaySign,
                GenerateSource = false,
                KeyContainer = base.KeyContainer,
                KeyFile = base.KeyFile
            };
            if (((this.ReferenceInfo != null) && (this.ReferenceInfo.primaryOfAxImpRef != null)) && ((this.ReferenceInfo.primaryOfAxImpRef.resolvedWrapper != null) && (this.ReferenceInfo.primaryOfAxImpRef.resolvedWrapper.path != null)))
            {
                imp.RuntimeCallableWrapperAssembly = this.ReferenceInfo.primaryOfAxImpRef.resolvedWrapper.path;
            }
            imp.OutputAssembly = Path.Combine(this.OutputDirectory, base.GetWrapperFileName());
            flag = imp.Execute();
            string wrapperPath = base.GetWrapperPath();
            wrapperInfo = new ComReferenceWrapperInfo();
            wrapperInfo.path = wrapperPath;
            wrapperInfo.assembly = Assembly.UnsafeLoadFrom(wrapperInfo.path);
            return flag;
        }

        protected override string GetWrapperFileNameInternal(string typeLibName)
        {
            return AxTlbBaseReference.GetWrapperFileName("AxInterop.", typeLibName, base.IncludeTypeLibVersionInName, this.ReferenceInfo.attr.wMajorVerNum, this.ReferenceInfo.attr.wMinorVerNum);
        }
    }
}

