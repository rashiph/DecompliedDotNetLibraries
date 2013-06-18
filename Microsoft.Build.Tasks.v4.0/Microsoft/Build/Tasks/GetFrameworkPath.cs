namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using System;

    public class GetFrameworkPath : TaskExtension
    {
        private static string path;
        private static string version11Path;
        private static string version20Path;
        private static string version30Path;
        private static string version35Path;
        private static string version40Path;

        public override bool Execute()
        {
            return true;
        }

        [Output]
        public string FrameworkVersion11Path
        {
            get
            {
                if (version11Path == null)
                {
                    version11Path = ToolLocationHelper.GetPathToDotNetFramework(TargetDotNetFrameworkVersion.Version11);
                }
                return version11Path;
            }
        }

        [Output]
        public string FrameworkVersion20Path
        {
            get
            {
                if (version20Path == null)
                {
                    version20Path = ToolLocationHelper.GetPathToDotNetFramework(TargetDotNetFrameworkVersion.Version20);
                }
                return version20Path;
            }
        }

        [Output]
        public string FrameworkVersion30Path
        {
            get
            {
                if (version30Path == null)
                {
                    version30Path = ToolLocationHelper.GetPathToDotNetFramework(TargetDotNetFrameworkVersion.Version30);
                }
                return version30Path;
            }
        }

        [Output]
        public string FrameworkVersion35Path
        {
            get
            {
                if (version35Path == null)
                {
                    version35Path = ToolLocationHelper.GetPathToDotNetFramework(TargetDotNetFrameworkVersion.Version35);
                }
                return version35Path;
            }
        }

        [Output]
        public string FrameworkVersion40Path
        {
            get
            {
                if (version40Path == null)
                {
                    version40Path = ToolLocationHelper.GetPathToDotNetFramework(TargetDotNetFrameworkVersion.Version40);
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
                    path = ToolLocationHelper.GetPathToDotNetFramework(TargetDotNetFrameworkVersion.Version40);
                }
                return path;
            }
            set
            {
                path = value;
            }
        }
    }
}

