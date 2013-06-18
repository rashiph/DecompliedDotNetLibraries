namespace System.Web.Compilation
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Web;
    using System.Web.Hosting;
    using System.Web.UI;
    using System.Web.Util;

    internal abstract class DiskBuildResultCache : BuildResultCache
    {
        protected string _cacheDir;
        protected const string dotDelete = ".delete";
        protected const string preservationFileExtension = ".compiled";
        private static bool s_inUseAssemblyWasDeleted;
        private static int s_maxRecompilations = -1;
        private static int s_recompilations;
        private static int s_shutdownStatus;
        private const int SHUTDOWN_NEEDED = 1;
        private const int SHUTDOWN_STARTED = 2;

        internal DiskBuildResultCache(string cacheDir)
        {
            this._cacheDir = cacheDir;
            if (s_maxRecompilations < 0)
            {
                s_maxRecompilations = CompilationUtil.GetRecompilationsBeforeAppRestarts();
            }
        }

        internal override void CacheBuildResult(string cacheKey, BuildResult result, long hashCode, DateTime utcStart)
        {
            if (result.CacheToDisk)
            {
                if (HostingEnvironment.ShutdownInitiated)
                {
                    BuildResultCompiledAssemblyBase base2 = result as BuildResultCompiledAssemblyBase;
                    if (base2 != null)
                    {
                        this.MarkAssemblyAndRelatedFilesForDeletion(base2.ResultAssembly.GetName().Name);
                    }
                }
                else
                {
                    string preservedDataFileName = this.GetPreservedDataFileName(cacheKey);
                    new PreservationFileWriter(this.PrecompilationMode).SaveBuildResultToFile(preservedDataFileName, result, hashCode);
                }
            }
        }

        internal static bool CheckAndRemoveDotDeleteFile(FileInfo f)
        {
            if (f.Extension != ".delete")
            {
                return false;
            }
            string filename = Path.GetDirectoryName(f.FullName) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(f.FullName);
            if (FileUtil.FileExists(filename))
            {
                try
                {
                    File.Delete(filename);
                }
                catch
                {
                    return false;
                }
            }
            try
            {
                f.Delete();
            }
            catch
            {
            }
            return true;
        }

        private static void CreateDotDeleteFile(FileInfo f)
        {
            if (f.Extension != ".delete")
            {
                string path = f.FullName + ".delete";
                if (!File.Exists(path))
                {
                    try
                    {
                        new StreamWriter(path).Close();
                    }
                    catch
                    {
                    }
                }
            }
        }

        protected void EnsureDiskCacheDirectoryCreated()
        {
            if (!FileUtil.DirectoryExists(this._cacheDir))
            {
                try
                {
                    Directory.CreateDirectory(this._cacheDir);
                }
                catch (IOException exception)
                {
                    throw new HttpException(System.Web.SR.GetString("Failed_to_create_temp_dir", new object[] { HttpRuntime.GetSafePath(this._cacheDir) }), exception);
                }
            }
        }

        internal override BuildResult GetBuildResult(string cacheKey, VirtualPath virtualPath, long hashCode, bool ensureIsUpToDate)
        {
            string preservedDataFileName = this.GetPreservedDataFileName(cacheKey);
            PreservationFileReader reader = new PreservationFileReader(this, this.PrecompilationMode);
            return reader.ReadBuildResultFromFile(virtualPath, preservedDataFileName, hashCode, ensureIsUpToDate);
        }

        private string GetPreservedDataFileName(string cacheKey)
        {
            cacheKey = Util.MakeValidFileName(cacheKey);
            cacheKey = Path.Combine(this._cacheDir, cacheKey);
            cacheKey = FileUtil.TruncatePathIfNeeded(cacheKey, 9);
            return (cacheKey + ".compiled");
        }

        internal static bool HasDotDeleteFile(string s)
        {
            return File.Exists(s + ".delete");
        }

        private void MarkAssemblyAndRelatedFilesForDeletion(string assemblyName)
        {
            DirectoryInfo info = new DirectoryInfo(this._cacheDir);
            string str = assemblyName.Substring("App_Web_".Length);
            foreach (FileInfo info2 in info.GetFiles("*" + str + ".*"))
            {
                CreateDotDeleteFile(info2);
            }
        }

        internal static void RemoveAssembly(FileInfo f)
        {
            if (HostingEnvironment.ShutdownInitiated)
            {
                CreateDotDeleteFile(f);
            }
            else if (!HasDotDeleteFile(f.FullName) && !TryDeleteFile(f))
            {
                if (++s_recompilations == s_maxRecompilations)
                {
                    s_shutdownStatus = 1;
                }
                s_inUseAssemblyWasDeleted = true;
            }
        }

        internal virtual void RemoveAssemblyAndRelatedFiles(string assemblyName)
        {
            if (assemblyName.StartsWith("App_Web_", StringComparison.Ordinal))
            {
                string str = assemblyName.Substring("App_Web_".Length);
                bool gotLock = false;
                try
                {
                    CompilationLock.GetLock(ref gotLock);
                    DirectoryInfo info = new DirectoryInfo(this._cacheDir);
                    foreach (FileInfo info2 in info.GetFiles("*" + str + ".*"))
                    {
                        if (info2.Extension == ".dll")
                        {
                            string assemblyCacheKey = BuildResultCache.GetAssemblyCacheKey(info2.FullName);
                            HttpRuntime.CacheInternal.Remove(assemblyCacheKey);
                            RemoveAssembly(info2);
                            StandardDiskBuildResultCache.RemoveSatelliteAssemblies(assemblyName);
                        }
                        else if (info2.Extension == ".delete")
                        {
                            CheckAndRemoveDotDeleteFile(info2);
                        }
                        else
                        {
                            TryDeleteFile(info2);
                        }
                    }
                }
                finally
                {
                    if (gotLock)
                    {
                        CompilationLock.ReleaseLock();
                    }
                    ShutDownAppDomainIfRequired();
                }
            }
        }

        internal static void ResetAssemblyDeleted()
        {
            s_inUseAssemblyWasDeleted = false;
        }

        internal static void ShutDownAppDomainIfRequired()
        {
            if ((s_shutdownStatus == 1) && (Interlocked.Exchange(ref s_shutdownStatus, 2) == 1))
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(DiskBuildResultCache.ShutdownCallBack));
            }
        }

        private static void ShutdownCallBack(object state)
        {
            HttpRuntime.ShutdownAppDomain(ApplicationShutdownReason.MaxRecompilationsReached, "Recompilation limit of " + s_maxRecompilations + " reached");
        }

        internal static bool TryDeleteFile(FileInfo f)
        {
            if (f.Extension == ".delete")
            {
                return CheckAndRemoveDotDeleteFile(f);
            }
            try
            {
                f.Delete();
                return true;
            }
            catch
            {
            }
            CreateDotDeleteFile(f);
            return false;
        }

        internal static bool TryDeleteFile(string s)
        {
            return TryDeleteFile(new FileInfo(s));
        }

        internal static bool InUseAssemblyWasDeleted
        {
            get
            {
                return s_inUseAssemblyWasDeleted;
            }
        }

        protected virtual bool PrecompilationMode
        {
            get
            {
                return false;
            }
        }
    }
}

