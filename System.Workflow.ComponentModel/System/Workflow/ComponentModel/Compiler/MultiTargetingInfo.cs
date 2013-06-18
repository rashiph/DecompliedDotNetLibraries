namespace System.Workflow.ComponentModel.Compiler
{
    using Microsoft.Build.Utilities;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Runtime.Versioning;
    using System.Security;

    [Serializable]
    internal class MultiTargetingInfo : ISerializable
    {
        private string compilerVersion;
        internal static readonly Version DefaultTargetFramework = new Version("4.0");
        private static IDictionary<Version, string> KnownSupportedTargetFrameworksAndRelatedCompilerVersions;
        private const string SerializationItem_TargetFramework = "TargetFramework";
        private FrameworkName targetFramework;
        private static readonly Version TargetFramework30 = new Version("3.0");
        internal const string TargetFramework30CompilerVersion = "v2.0";
        private static readonly Version TargetFramework35 = new Version("3.5");
        internal const string TargetFramework35CompilerVersion = "v3.5";
        private static readonly Version TargetFramework40 = new Version("4.0");
        private const string TargetFramework40CompatiblePrefix = "v4.";
        internal const string TargetFramework40CompilerVersion = "v4.0";

        static MultiTargetingInfo()
        {
            Dictionary<Version, string> dictionary = new Dictionary<Version, string>();
            dictionary.Add(TargetFramework30, "v2.0");
            dictionary.Add(TargetFramework35, "v3.5");
            dictionary.Add(TargetFramework40, "v4.0");
            KnownSupportedTargetFrameworksAndRelatedCompilerVersions = dictionary;
        }

        public MultiTargetingInfo(string targetFramework)
        {
            this.targetFramework = new FrameworkName(targetFramework);
        }

        [SecuritySafeCritical]
        protected MultiTargetingInfo(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            this.targetFramework = new FrameworkName(info.GetString("TargetFramework"));
        }

        private static string GetCompilerVersion(Version targetFrameworkVersion)
        {
            string str;
            if (targetFrameworkVersion.Major == 4)
            {
                targetFrameworkVersion = TargetFramework40;
            }
            if (!KnownSupportedTargetFrameworksAndRelatedCompilerVersions.TryGetValue(targetFrameworkVersion, out str))
            {
                str = string.Empty;
            }
            return str;
        }

        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            info.AddValue("TargetFramework", this.targetFramework.FullName, typeof(string));
        }

        public string CompilerVersion
        {
            get
            {
                if (this.compilerVersion == null)
                {
                    this.compilerVersion = GetCompilerVersion(this.targetFramework.Version);
                }
                return this.compilerVersion;
            }
        }

        public FrameworkName TargetFramework
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.targetFramework;
            }
        }

        public static class MultiTargetingUtilities
        {
            private const string FrameworkReferencePrefix = "<FRAMEWORK>";
            private static ReferenceManager refManager;
            private static RuntimeManager runtimeManager;
            private const string RuntimeReferencePrefix = "<RUNTIME>";

            private static void EnsureReferenceManager()
            {
                if (refManager == null)
                {
                    refManager = new ReferenceManager();
                }
            }

            private static void EnsureRuntimeManager()
            {
                if (runtimeManager == null)
                {
                    runtimeManager = new RuntimeManager();
                }
            }

            public static bool IsFrameworkReferenceAssembly(string path)
            {
                EnsureReferenceManager();
                return refManager.IsFrameworkReferenceAssembly(path);
            }

            private static bool IsPathUnderDirectory(string path, string parentDirectory)
            {
                if (!path.StartsWith(parentDirectory, StringComparison.CurrentCultureIgnoreCase))
                {
                    return false;
                }
                int length = parentDirectory.Length;
                if (path.Length == length)
                {
                    return false;
                }
                if ((path[length] != Path.DirectorySeparatorChar) && (path[length] != Path.AltDirectorySeparatorChar))
                {
                    return false;
                }
                return true;
            }

            private static string NormalizePath(string path, ref bool wasNormelized)
            {
                path = Path.GetFullPath(path);
                if (IsPathUnderDirectory(path, runtimeManager.NetFxRuntimeRoot))
                {
                    wasNormelized = true;
                    return path.Replace(runtimeManager.NetFxRuntimeRoot, "<RUNTIME>");
                }
                if (IsPathUnderDirectory(path, refManager.FrameworkReferenceAssemblyRoot))
                {
                    wasNormelized = true;
                    return path.Replace(refManager.FrameworkReferenceAssemblyRoot, "<FRAMEWORK>");
                }
                return path;
            }

            public static WorkflowCompilerParameters NormalizeReferencedAssemblies(WorkflowCompilerParameters parameters)
            {
                EnsureRuntimeManager();
                EnsureReferenceManager();
                string[] newReferencedAssemblies = new string[parameters.ReferencedAssemblies.Count];
                bool wasNormelized = false;
                for (int i = 0; i < parameters.ReferencedAssemblies.Count; i++)
                {
                    newReferencedAssemblies[i] = NormalizePath(parameters.ReferencedAssemblies[i], ref wasNormelized);
                }
                if (wasNormelized)
                {
                    return new WorkflowCompilerParameters(parameters, newReferencedAssemblies);
                }
                return parameters;
            }

            private static string RenormalizePath(string path, ref bool wasRenormelized)
            {
                if (path.StartsWith("<RUNTIME>", StringComparison.Ordinal))
                {
                    wasRenormelized = true;
                    return path.Replace("<RUNTIME>", runtimeManager.NetFxRuntimeRoot);
                }
                if (path.StartsWith("<FRAMEWORK>", StringComparison.Ordinal))
                {
                    wasRenormelized = true;
                    return path.Replace("<FRAMEWORK>", refManager.FrameworkReferenceAssemblyRoot);
                }
                return path;
            }

            public static WorkflowCompilerParameters RenormalizeReferencedAssemblies(WorkflowCompilerParameters parameters)
            {
                EnsureRuntimeManager();
                EnsureReferenceManager();
                string[] newReferencedAssemblies = new string[parameters.ReferencedAssemblies.Count];
                bool wasRenormelized = false;
                for (int i = 0; i < parameters.ReferencedAssemblies.Count; i++)
                {
                    newReferencedAssemblies[i] = RenormalizePath(parameters.ReferencedAssemblies[i], ref wasRenormelized);
                }
                if (wasRenormelized)
                {
                    return new WorkflowCompilerParameters(parameters, newReferencedAssemblies);
                }
                return parameters;
            }

            private class ReferenceManager
            {
                private string frameworkReferenceAssemblyRoot = ToolLocationHelper.GetProgramFilesReferenceAssemblyRoot();
                private HashSet<string> frameworkReferenceDirectories = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);

                public ReferenceManager()
                {
                    IList<string> supportedTargetFrameworks = ToolLocationHelper.GetSupportedTargetFrameworks();
                    for (int i = 0; i < supportedTargetFrameworks.Count; i++)
                    {
                        FrameworkName frameworkName = new FrameworkName(supportedTargetFrameworks[i]);
                        IList<string> pathToReferenceAssemblies = ToolLocationHelper.GetPathToReferenceAssemblies(frameworkName);
                        for (int j = 0; j < pathToReferenceAssemblies.Count; j++)
                        {
                            string item = XomlCompilerHelper.TrimDirectorySeparatorChar(pathToReferenceAssemblies[j]);
                            if (!this.frameworkReferenceDirectories.Contains(item))
                            {
                                this.frameworkReferenceDirectories.Add(item);
                            }
                        }
                    }
                }

                public bool IsFrameworkReferenceAssembly(string path)
                {
                    string item = XomlCompilerHelper.TrimDirectorySeparatorChar(Path.GetDirectoryName(Path.GetFullPath(path)));
                    return this.frameworkReferenceDirectories.Contains(item);
                }

                public string FrameworkReferenceAssemblyRoot
                {
                    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.frameworkReferenceAssemblyRoot;
                    }
                }
            }

            private class RuntimeManager
            {
                private const string NDPSetupRegistryBranch = @"SOFTWARE\Microsoft\NET Framework Setup\NDP";
                private string netFxRuntimeRoot;

                public RuntimeManager()
                {
                    string path = XomlCompilerHelper.TrimDirectorySeparatorChar(RuntimeEnvironment.GetRuntimeDirectory());
                    this.netFxRuntimeRoot = XomlCompilerHelper.TrimDirectorySeparatorChar(Path.GetDirectoryName(path));
                }

                public string NetFxRuntimeRoot
                {
                    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.netFxRuntimeRoot;
                    }
                }
            }
        }
    }
}

