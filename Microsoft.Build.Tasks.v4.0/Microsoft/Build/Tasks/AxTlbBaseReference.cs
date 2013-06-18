namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    internal abstract class AxTlbBaseReference : ComReference
    {
        private bool includeTypeLibVersionInName;
        private string outputDirectory;
        private IComReferenceResolver resolverCallback;

        internal AxTlbBaseReference(TaskLoggingHelper taskLoggingHelper, IComReferenceResolver resolverCallback, ComReferenceInfo referenceInfo, string itemName, string outputDirectory, bool delaySign, string keyFile, string keyContainer, bool includeTypeLibVersionInName, bool executeAsTool, string toolPath, IBuildEngine buildEngine, string[] environmentVariables) : base(taskLoggingHelper, referenceInfo, itemName)
        {
            this.resolverCallback = resolverCallback;
            this.outputDirectory = outputDirectory;
            this.includeTypeLibVersionInName = includeTypeLibVersionInName;
            this.BuildEngine = buildEngine;
            this.EnvironmentVariables = environmentVariables;
            this.DelaySign = delaySign;
            this.ExecuteAsTool = executeAsTool;
            this.KeyFile = keyFile;
            this.KeyContainer = keyContainer;
            this.ToolPath = toolPath;
        }

        internal override bool FindExistingWrapper(out ComReferenceWrapperInfo wrapperInfo, DateTime componentTimestamp)
        {
            wrapperInfo = null;
            string wrapperPath = this.GetWrapperPath();
            if (!File.Exists(wrapperPath))
            {
                return false;
            }
            wrapperInfo = new ComReferenceWrapperInfo();
            wrapperInfo.path = wrapperPath;
            return this.IsWrapperUpToDate(wrapperInfo, componentTimestamp);
        }

        internal string GetWrapperFileName()
        {
            return this.GetWrapperFileNameInternal(this.ReferenceInfo.typeLibName);
        }

        internal static string GetWrapperFileName(string interopDllHeader, string typeLibName, bool includeTypeLibVersionInName, short majorVerNum, short minorVerNum)
        {
            StringBuilder builder = new StringBuilder(interopDllHeader);
            builder.Append(typeLibName);
            if (includeTypeLibVersionInName)
            {
                builder.Append('.');
                builder.Append(majorVerNum);
                builder.Append('.');
                builder.Append(minorVerNum);
            }
            builder.Append(".dll");
            return builder.ToString();
        }

        protected abstract string GetWrapperFileNameInternal(string typeLibName);
        internal string GetWrapperPath()
        {
            return Path.Combine(this.OutputDirectory, this.GetWrapperFileName());
        }

        protected virtual bool IsWrapperUpToDate(ComReferenceWrapperInfo wrapperInfo, DateTime componentTimestamp)
        {
            if ((this.ReferenceInfo.typeLibPath == null) || (this.ReferenceInfo.typeLibPath.Length == 0))
            {
                throw new ComReferenceResolutionException();
            }
            if (!File.Exists(wrapperInfo.path))
            {
                return false;
            }
            if (DateTime.Compare(File.GetLastWriteTime(this.ReferenceInfo.typeLibPath), componentTimestamp) != 0)
            {
                return false;
            }
            StrongNameLevel none = StrongNameLevel.None;
            if (((this.KeyFile != null) && (this.KeyFile.Length > 0)) || ((this.KeyContainer != null) && (this.KeyContainer.Length > 0)))
            {
                if (this.DelaySign)
                {
                    none = StrongNameLevel.DelaySigned;
                }
                else
                {
                    none = StrongNameLevel.FullySigned;
                }
            }
            StrongNameLevel assemblyStrongNameLevel = StrongNameUtils.GetAssemblyStrongNameLevel(wrapperInfo.path);
            if (none != assemblyStrongNameLevel)
            {
                return false;
            }
            if ((none == StrongNameLevel.DelaySigned) || (none == StrongNameLevel.FullySigned))
            {
                StrongNameKeyPair pair;
                byte[] publicKey = null;
                StrongNameUtils.GetStrongNameKey(base.Log, this.KeyFile, this.KeyContainer, out pair, out publicKey);
                AssemblyName assemblyName = AssemblyName.GetAssemblyName(wrapperInfo.path);
                if (assemblyName == null)
                {
                    return false;
                }
                byte[] buffer2 = assemblyName.GetPublicKey();
                if (buffer2.Length != publicKey.Length)
                {
                    return false;
                }
                for (int i = 0; i < buffer2.Length; i++)
                {
                    if (buffer2[i] != publicKey[i])
                    {
                        return false;
                    }
                }
            }
            try
            {
                wrapperInfo.assembly = Assembly.UnsafeLoadFrom(wrapperInfo.path);
            }
            catch (BadImageFormatException)
            {
                wrapperInfo.assembly = null;
            }
            return (wrapperInfo.assembly != null);
        }

        protected IBuildEngine BuildEngine { get; set; }

        protected bool DelaySign { get; set; }

        protected string[] EnvironmentVariables { get; set; }

        protected bool ExecuteAsTool { get; set; }

        protected bool IncludeTypeLibVersionInName
        {
            get
            {
                return this.includeTypeLibVersionInName;
            }
            set
            {
                this.includeTypeLibVersionInName = value;
            }
        }

        protected string KeyContainer { get; set; }

        protected string KeyFile { get; set; }

        protected virtual string OutputDirectory
        {
            get
            {
                return this.outputDirectory;
            }
        }

        protected IComReferenceResolver ResolverCallback
        {
            get
            {
                return this.resolverCallback;
            }
        }

        protected string ToolPath { get; set; }
    }
}

