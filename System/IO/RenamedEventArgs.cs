namespace System.IO
{
    using System;
    using System.Security.Permissions;

    public class RenamedEventArgs : FileSystemEventArgs
    {
        private string oldFullPath;
        private string oldName;

        public RenamedEventArgs(WatcherChangeTypes changeType, string directory, string name, string oldName) : base(changeType, directory, name)
        {
            if (!directory.EndsWith(@"\", StringComparison.Ordinal))
            {
                directory = directory + @"\";
            }
            this.oldName = oldName;
            this.oldFullPath = directory + oldName;
        }

        public string OldFullPath
        {
            get
            {
                new FileIOPermission(FileIOPermissionAccess.Read, Path.GetPathRoot(this.oldFullPath)).Demand();
                return this.oldFullPath;
            }
        }

        public string OldName
        {
            get
            {
                return this.oldName;
            }
        }
    }
}

