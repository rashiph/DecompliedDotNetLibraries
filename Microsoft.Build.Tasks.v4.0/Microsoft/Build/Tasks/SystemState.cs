namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Shared;
    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    internal sealed class SystemState : StateFileBase, ISerializable
    {
        private Microsoft.Build.Shared.FileExists fileExists;
        private Microsoft.Build.Tasks.GetAssemblyMetadata getAssemblyMetadata;
        private Microsoft.Build.Tasks.GetAssemblyName getAssemblyName;
        private GetAssemblyRuntimeVersion getAssemblyRuntimeVersion;
        private Microsoft.Build.Tasks.GetDirectories getDirectories;
        private GetLastWriteTime getLastWriteTime;
        private Hashtable instanceLocalDirectories;
        private Hashtable instanceLocalFileExists;
        private Hashtable instanceLocalFileStateCache;
        private bool isDirty;
        private static Hashtable processWideFileStateCache = new Hashtable();
        private RedistList redistList;

        internal SystemState()
        {
            this.instanceLocalFileStateCache = new Hashtable();
            this.instanceLocalFileExists = new Hashtable();
            this.instanceLocalDirectories = new Hashtable();
        }

        internal SystemState(SerializationInfo info, StreamingContext context)
        {
            this.instanceLocalFileStateCache = new Hashtable();
            this.instanceLocalFileExists = new Hashtable();
            this.instanceLocalDirectories = new Hashtable();
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(info, "info");
            this.instanceLocalFileStateCache = (Hashtable) info.GetValue("fileState", typeof(Hashtable));
            this.isDirty = false;
        }

        internal Microsoft.Build.Shared.FileExists CacheDelegate(Microsoft.Build.Shared.FileExists fileExistsValue)
        {
            this.fileExists = fileExistsValue;
            return new Microsoft.Build.Shared.FileExists(this.FileExists);
        }

        internal Microsoft.Build.Tasks.GetAssemblyMetadata CacheDelegate(Microsoft.Build.Tasks.GetAssemblyMetadata getAssemblyMetadataValue)
        {
            this.getAssemblyMetadata = getAssemblyMetadataValue;
            return new Microsoft.Build.Tasks.GetAssemblyMetadata(this.GetAssemblyMetadata);
        }

        internal Microsoft.Build.Tasks.GetAssemblyName CacheDelegate(Microsoft.Build.Tasks.GetAssemblyName getAssemblyNameValue)
        {
            this.getAssemblyName = getAssemblyNameValue;
            return new Microsoft.Build.Tasks.GetAssemblyName(this.GetAssemblyName);
        }

        internal GetAssemblyRuntimeVersion CacheDelegate(GetAssemblyRuntimeVersion getAssemblyRuntimeVersion)
        {
            this.getAssemblyRuntimeVersion = getAssemblyRuntimeVersion;
            return new GetAssemblyRuntimeVersion(this.getAssemblyRuntimeVersion.Invoke);
        }

        internal Microsoft.Build.Tasks.GetDirectories CacheDelegate(Microsoft.Build.Tasks.GetDirectories getDirectoriesValue)
        {
            this.getDirectories = getDirectoriesValue;
            return new Microsoft.Build.Tasks.GetDirectories(this.GetDirectories);
        }

        private bool FileExists(string path)
        {
            object obj2 = this.instanceLocalFileExists[path];
            if (obj2 != null)
            {
                return (bool) obj2;
            }
            bool flag = this.fileExists(path);
            this.instanceLocalFileExists[path] = flag;
            return flag;
        }

        private void GetAssemblyMetadata(string path, out AssemblyNameExtension[] dependencies, out string[] scatterFiles)
        {
            FileState fileState = this.GetFileState(path);
            if (fileState.dependencies == null)
            {
                this.getAssemblyMetadata(path, out fileState.dependencies, out fileState.scatterFiles);
                this.isDirty = true;
            }
            dependencies = fileState.dependencies;
            scatterFiles = fileState.scatterFiles;
        }

        private AssemblyNameExtension GetAssemblyName(string path)
        {
            if ((this.redistList != null) && (string.Compare(Path.GetExtension(path), ".dll", StringComparison.OrdinalIgnoreCase) == 0))
            {
                AssemblyEntry[] entryArray = this.redistList.FindAssemblyNameFromSimpleName(Path.GetFileNameWithoutExtension(path));
                for (int i = 0; i < entryArray.Length; i++)
                {
                    string fileName = Path.GetFileName(path);
                    string b = Path.Combine(entryArray[i].FrameworkDirectory, fileName);
                    if (string.Equals(path, b, StringComparison.OrdinalIgnoreCase))
                    {
                        return new AssemblyNameExtension(entryArray[i].FullName);
                    }
                }
            }
            FileState fileState = this.GetFileState(path);
            if (fileState.Assembly == null)
            {
                fileState.Assembly = this.getAssemblyName(path);
                if (fileState.Assembly == null)
                {
                    fileState.Assembly = AssemblyNameExtension.UnnamedAssembly;
                }
                this.isDirty = true;
            }
            if (fileState.Assembly.IsUnnamedAssembly)
            {
                return null;
            }
            return fileState.Assembly;
        }

        private string[] GetDirectories(string path, string pattern)
        {
            if (!(pattern == "*."))
            {
                return this.getDirectories(path, pattern);
            }
            object obj2 = this.instanceLocalDirectories[path];
            if (obj2 == null)
            {
                string[] strArray = this.getDirectories(path, pattern);
                this.instanceLocalDirectories[path] = strArray;
                return strArray;
            }
            return (string[]) obj2;
        }

        private FileState GetFileState(string path)
        {
            FileState state = null;
            FileState state2 = (FileState) processWideFileStateCache[path];
            FileState state3 = (FileState) this.instanceLocalFileStateCache[path];
            if ((state2 == null) && (state3 != null))
            {
                state = state3;
                processWideFileStateCache[path] = state3;
            }
            else if ((state2 != null) && (state3 == null))
            {
                state = state2;
                this.instanceLocalFileStateCache[path] = state2;
            }
            else if ((state2 != null) && (state3 != null))
            {
                if (state2.LastModified > state3.LastModified)
                {
                    state = state2;
                    this.instanceLocalFileStateCache[path] = state2;
                }
                else
                {
                    state = state3;
                    processWideFileStateCache[path] = state3;
                }
            }
            if (state == null)
            {
                state = new FileState {
                    LastModified = this.getLastWriteTime(path)
                };
                this.instanceLocalFileStateCache[path] = state;
                processWideFileStateCache[path] = state;
                this.isDirty = true;
                return state;
            }
            if (this.getLastWriteTime(path) != state.LastModified)
            {
                state = new FileState {
                    LastModified = this.getLastWriteTime(path)
                };
                this.instanceLocalFileStateCache[path] = state;
                processWideFileStateCache[path] = state;
                this.isDirty = true;
            }
            return state;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(info, "info");
            info.AddValue("fileState", this.instanceLocalFileStateCache);
        }

        private string GetRuntimeVersion(string path)
        {
            FileState fileState = this.GetFileState(path);
            if (string.IsNullOrEmpty(fileState.RuntimeVersion))
            {
                fileState.RuntimeVersion = this.getAssemblyRuntimeVersion(path);
                this.isDirty = true;
            }
            return fileState.RuntimeVersion;
        }

        internal void SetGetLastWriteTime(GetLastWriteTime getLastWriteTimeValue)
        {
            this.getLastWriteTime = getLastWriteTimeValue;
        }

        internal void SetInstalledAssemblyInformation(AssemblyTableInfo[] installedAssemblyTableInfos)
        {
            this.redistList = RedistList.GetRedistList(installedAssemblyTableInfos);
        }

        internal bool IsDirty
        {
            get
            {
                return this.isDirty;
            }
        }

        [Serializable]
        private sealed class FileState : ISerializable
        {
            private AssemblyNameExtension assemblyName;
            internal AssemblyNameExtension[] dependencies;
            private DateTime lastModified;
            internal string runtimeVersion;
            internal string[] scatterFiles;

            internal FileState()
            {
            }

            internal FileState(SerializationInfo info, StreamingContext context)
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(info, "info");
                this.lastModified = info.GetDateTime("lastModified");
                this.assemblyName = (AssemblyNameExtension) info.GetValue("assemblyName", typeof(AssemblyNameExtension));
                this.dependencies = (AssemblyNameExtension[]) info.GetValue("dependencies", typeof(AssemblyNameExtension[]));
                this.scatterFiles = (string[]) info.GetValue("scatterFiles", typeof(string[]));
                this.runtimeVersion = (string) info.GetValue("runtimeVersion", typeof(string));
            }

            [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(info, "info");
                info.AddValue("lastModified", this.lastModified);
                info.AddValue("assemblyName", this.assemblyName);
                info.AddValue("dependencies", this.dependencies);
                info.AddValue("scatterFiles", this.scatterFiles);
                info.AddValue("runtimeVersion", this.runtimeVersion);
            }

            internal AssemblyNameExtension Assembly
            {
                get
                {
                    return this.assemblyName;
                }
                set
                {
                    this.assemblyName = value;
                }
            }

            internal DateTime LastModified
            {
                get
                {
                    return this.lastModified;
                }
                set
                {
                    this.lastModified = value;
                }
            }

            internal string RuntimeVersion
            {
                get
                {
                    return this.runtimeVersion;
                }
                set
                {
                    this.runtimeVersion = value;
                }
            }
        }
    }
}

