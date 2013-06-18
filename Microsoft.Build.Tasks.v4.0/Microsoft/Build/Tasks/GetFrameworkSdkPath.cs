namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using Microsoft.Build.Utilities;
    using System;

    public class GetFrameworkSdkPath : TaskExtension
    {
        private static string path;
        private static string version20Path;
        private static string version35Path;
        private static string version40Path;

        public override bool Execute()
        {
            return true;
        }

        [Output]
        public string FrameworkSdkVersion20Path
        {
            get
            {
                if (version20Path == null)
                {
                    version20Path = ToolLocationHelper.GetPathToDotNetFrameworkSdk(TargetDotNetFrameworkVersion.Version20);
                    if (string.IsNullOrEmpty(version20Path))
                    {
                        base.Log.LogMessageFromResources(MessageImportance.High, "GetFrameworkSdkPath.CouldNotFindSDK", new object[] { ToolLocationHelper.GetDotNetFrameworkSdkInstallKeyValue(TargetDotNetFrameworkVersion.Version20), ToolLocationHelper.GetDotNetFrameworkSdkRootRegistryKey(TargetDotNetFrameworkVersion.Version20) });
                        version20Path = string.Empty;
                    }
                    else
                    {
                        version20Path = Microsoft.Build.Shared.FileUtilities.EnsureTrailingSlash(version20Path);
                        base.Log.LogMessageFromResources(MessageImportance.Low, "GetFrameworkSdkPath.FoundSDK", new object[] { version20Path });
                    }
                }
                return version20Path;
            }
        }

        [Output]
        public string FrameworkSdkVersion35Path
        {
            get
            {
                if (version35Path == null)
                {
                    version35Path = ToolLocationHelper.GetPathToDotNetFrameworkSdk(TargetDotNetFrameworkVersion.Version35);
                    if (string.IsNullOrEmpty(version35Path))
                    {
                        base.Log.LogMessageFromResources(MessageImportance.High, "GetFrameworkSdkPath.CouldNotFindSDK", new object[] { ToolLocationHelper.GetDotNetFrameworkSdkInstallKeyValue(TargetDotNetFrameworkVersion.Version35), ToolLocationHelper.GetDotNetFrameworkSdkRootRegistryKey(TargetDotNetFrameworkVersion.Version35) });
                        version35Path = string.Empty;
                    }
                    else
                    {
                        version35Path = Microsoft.Build.Shared.FileUtilities.EnsureTrailingSlash(version35Path);
                        base.Log.LogMessageFromResources(MessageImportance.Low, "GetFrameworkSdkPath.FoundSDK", new object[] { version35Path });
                    }
                }
                return version35Path;
            }
        }

        [Output]
        public string FrameworkSdkVersion40Path
        {
            get
            {
                if (version40Path == null)
                {
                    version40Path = ToolLocationHelper.GetPathToDotNetFrameworkSdk(TargetDotNetFrameworkVersion.Version40);
                    if (string.IsNullOrEmpty(version40Path))
                    {
                        base.Log.LogMessageFromResources(MessageImportance.High, "GetFrameworkSdkPath.CouldNotFindSDK", new object[] { ToolLocationHelper.GetDotNetFrameworkSdkInstallKeyValue(TargetDotNetFrameworkVersion.Version40), ToolLocationHelper.GetDotNetFrameworkSdkRootRegistryKey(TargetDotNetFrameworkVersion.Version40) });
                        version40Path = string.Empty;
                    }
                    else
                    {
                        version40Path = Microsoft.Build.Shared.FileUtilities.EnsureTrailingSlash(version40Path);
                        base.Log.LogMessageFromResources(MessageImportance.Low, "GetFrameworkSdkPath.FoundSDK", new object[] { version40Path });
                    }
                }
                return version40Path;
            }
        }

        [Output]
        public string Path
        {
            get
            {
                if (path == null)
                {
                    path = ToolLocationHelper.GetPathToDotNetFrameworkSdk(TargetDotNetFrameworkVersion.Version40);
                    if (string.IsNullOrEmpty(path))
                    {
                        base.Log.LogMessageFromResources(MessageImportance.High, "GetFrameworkSdkPath.CouldNotFindSDK", new object[] { ToolLocationHelper.GetDotNetFrameworkSdkInstallKeyValue(TargetDotNetFrameworkVersion.Version40), ToolLocationHelper.GetDotNetFrameworkSdkRootRegistryKey(TargetDotNetFrameworkVersion.Version40) });
                        path = string.Empty;
                    }
                    else
                    {
                        path = Microsoft.Build.Shared.FileUtilities.EnsureTrailingSlash(path);
                        base.Log.LogMessageFromResources(MessageImportance.Low, "GetFrameworkSdkPath.FoundSDK", new object[] { path });
                    }
                }
                return path;
            }
            set
            {
            }
        }
    }
}

