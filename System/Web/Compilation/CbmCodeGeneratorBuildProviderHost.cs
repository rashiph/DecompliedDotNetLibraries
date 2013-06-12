namespace System.Web.Compilation
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Web.Configuration;
    using System.Web.Util;

    internal class CbmCodeGeneratorBuildProviderHost : AssemblyBuilder
    {
        private string _generatedFilesDir;

        internal CbmCodeGeneratorBuildProviderHost(CompilationSection compConfig, ICollection referencedAssemblies, CompilerType compilerType, string generatedFilesDir, string outputAssemblyName) : base(compConfig, referencedAssemblies, compilerType, outputAssemblyName)
        {
            if (Directory.Exists(generatedFilesDir))
            {
                foreach (FileData data in (IEnumerable) FileEnumerator.Create(generatedFilesDir))
                {
                    if (!data.IsDirectory)
                    {
                        File.Delete(data.FullName);
                    }
                }
            }
            Directory.CreateDirectory(generatedFilesDir);
            this._generatedFilesDir = generatedFilesDir;
        }

        internal override void AddBuildProvider(System.Web.Compilation.BuildProvider buildProvider)
        {
            if (!(buildProvider is SourceFileBuildProvider))
            {
                base.AddBuildProvider(buildProvider);
            }
        }

        internal override TextWriter CreateCodeFile(System.Web.Compilation.BuildProvider buildProvider, out string filename)
        {
            string cacheKeyFromVirtualPath = BuildManager.GetCacheKeyFromVirtualPath(buildProvider.VirtualPathObject);
            cacheKeyFromVirtualPath = FileUtil.TruncatePathIfNeeded(Path.Combine(this._generatedFilesDir, cacheKeyFromVirtualPath), 10) + "." + base._codeProvider.FileExtension;
            filename = cacheKeyFromVirtualPath;
            BuildManager.GenerateFileTable[buildProvider.VirtualPathObject.VirtualPathStringNoTrailingSlash] = cacheKeyFromVirtualPath;
            return new StreamWriter(new FileStream(cacheKeyFromVirtualPath, FileMode.Create, FileAccess.Write, FileShare.Read), Encoding.UTF8);
        }
    }
}

