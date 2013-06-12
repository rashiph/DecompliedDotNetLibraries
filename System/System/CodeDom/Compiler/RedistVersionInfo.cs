namespace System.CodeDom.Compiler
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.IO;

    internal static class RedistVersionInfo
    {
        internal const string DefaultVersion = "v4.0";
        internal const string DirectoryPath = "CompilerDirectoryPath";
        private const string dotNetFrameworkRegistryPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSBuild\ToolsVersions\";
        internal const string InPlaceVersion = "v4.0";
        private const string MSBuildToolsPath = "MSBuildToolsPath";
        internal const string NameTag = "CompilerVersion";
        internal const string RedistVersion = "v3.5";
        internal const string RedistVersion20 = "v2.0";

        public static string GetCompilerPath(IDictionary<string, string> provOptions, string compilerExecutable)
        {
            string runtimeInstallDirectory = Executor.GetRuntimeInstallDirectory();
            if (provOptions != null)
            {
                string str2;
                string str3;
                bool flag = provOptions.TryGetValue("CompilerDirectoryPath", out str2);
                bool flag2 = provOptions.TryGetValue("CompilerVersion", out str3);
                if (flag && flag2)
                {
                    throw new InvalidOperationException(SR.GetString("Cannot_Specify_Both_Compiler_Path_And_Version", new object[] { "CompilerDirectoryPath", "CompilerVersion" }));
                }
                if (flag)
                {
                    return str2;
                }
                if (flag2)
                {
                    string str4 = str3;
                    if (str4 == null)
                    {
                        goto Label_00A9;
                    }
                    if (str4 != "v4.0")
                    {
                        if (!(str4 == "v3.5"))
                        {
                            if (str4 == "v2.0")
                            {
                                runtimeInstallDirectory = GetCompilerPathFromRegistry(str3);
                                goto Label_00AB;
                            }
                            goto Label_00A9;
                        }
                        runtimeInstallDirectory = GetCompilerPathFromRegistry(str3);
                    }
                }
            }
            goto Label_00AB;
        Label_00A9:
            runtimeInstallDirectory = null;
        Label_00AB:
            if (runtimeInstallDirectory == null)
            {
                throw new InvalidOperationException(SR.GetString("CompilerNotFound", new object[] { compilerExecutable }));
            }
            return runtimeInstallDirectory;
        }

        private static string GetCompilerPathFromRegistry(string versionVal)
        {
            string path = null;
            string environmentVariable = Environment.GetEnvironmentVariable("COMPLUS_InstallRoot");
            string str3 = Environment.GetEnvironmentVariable("COMPLUS_Version");
            if (!string.IsNullOrEmpty(environmentVariable) && !string.IsNullOrEmpty(str3))
            {
                path = Path.Combine(environmentVariable, str3);
                if (Directory.Exists(path))
                {
                    return path;
                }
            }
            string str4 = versionVal.Substring(1);
            path = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSBuild\ToolsVersions\" + str4, "MSBuildToolsPath", null) as string;
            if ((path != null) && Directory.Exists(path))
            {
                return path;
            }
            return null;
        }
    }
}

