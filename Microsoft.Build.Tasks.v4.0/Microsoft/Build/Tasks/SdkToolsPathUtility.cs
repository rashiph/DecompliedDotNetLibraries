namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using Microsoft.Build.Utilities;
    using System;
    using System.IO;

    internal static class SdkToolsPathUtility
    {
        private static Microsoft.Build.Shared.FileExists fileInfoExists;

        private static bool FileExists(string filePath)
        {
            return new FileInfo(filePath).Exists;
        }

        internal static string FindSDKToolUsingToolsLocationHelper(string toolName)
        {
            return ToolLocationHelper.GetPathToDotNetFrameworkSdkFile(toolName, TargetDotNetFrameworkVersion.Version40);
        }

        internal static string GeneratePathToTool(Microsoft.Build.Shared.FileExists fileExists, string currentArchitecture, string sdkToolsPath, string toolName, TaskLoggingHelper log, bool logErrorsAndWarnings)
        {
            string path = null;
            if (!string.IsNullOrEmpty(sdkToolsPath))
            {
                string str2 = string.Empty;
                try
                {
                    string str4 = currentArchitecture;
                    if (str4 == null)
                    {
                        goto Label_0061;
                    }
                    if (!(str4 == "AMD64"))
                    {
                        if (str4 == "IA64")
                        {
                            goto Label_0053;
                        }
                        if (str4 == "x86")
                        {
                        }
                        goto Label_0061;
                    }
                    str2 = Path.Combine(sdkToolsPath, "x64");
                    goto Label_0063;
                Label_0053:
                    str2 = Path.Combine(sdkToolsPath, "ia64");
                    goto Label_0063;
                Label_0061:
                    str2 = sdkToolsPath;
                Label_0063:
                    path = Path.Combine(str2, toolName);
                    if (!fileExists(path))
                    {
                        if (currentArchitecture != "x86")
                        {
                            path = Path.Combine(sdkToolsPath, toolName);
                        }
                    }
                    else
                    {
                        return path;
                    }
                }
                catch (ArgumentException exception)
                {
                    log.LogErrorWithCodeFromResources("General.SdkToolsPathError", new object[] { toolName, exception.Message });
                    return null;
                }
                if (fileExists(path))
                {
                    return path;
                }
                if (logErrorsAndWarnings)
                {
                    log.LogWarningWithCodeFromResources("General.PlatformSDKFileNotFoundSdkToolsPath", new object[] { toolName, str2, sdkToolsPath });
                }
            }
            else if (logErrorsAndWarnings)
            {
                log.LogMessageFromResources(MessageImportance.Low, "General.SdkToolsPathNotSpecifiedOrToolDoesNotExist", new object[] { toolName, sdkToolsPath });
            }
            if ((path == null) || !fileExists(path))
            {
                path = FindSDKToolUsingToolsLocationHelper(toolName);
                if ((path == null) && logErrorsAndWarnings)
                {
                    log.LogErrorWithCodeFromResources("General.SdkToolsPathToolDoesNotExist", new object[] { toolName, sdkToolsPath, ToolLocationHelper.GetDotNetFrameworkSdkRootRegistryKey(TargetDotNetFrameworkVersion.Version40) });
                }
            }
            return path;
        }

        internal static Microsoft.Build.Shared.FileExists FileInfoExists
        {
            get
            {
                if (fileInfoExists == null)
                {
                    fileInfoExists = new Microsoft.Build.Shared.FileExists(SdkToolsPathUtility.FileExists);
                }
                return fileInfoExists;
            }
        }
    }
}

