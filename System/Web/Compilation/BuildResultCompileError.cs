namespace System.Web.Compilation
{
    using System;
    using System.Web;

    internal class BuildResultCompileError : BuildResult
    {
        private HttpCompileException _compileException;

        internal BuildResultCompileError(VirtualPath virtualPath, HttpCompileException compileException)
        {
            base.VirtualPath = virtualPath;
            this._compileException = compileException;
        }

        internal override bool CacheToDisk
        {
            get
            {
                return false;
            }
        }

        internal HttpCompileException CompileException
        {
            get
            {
                return this._compileException;
            }
        }

        internal override DateTime MemoryCacheExpiration
        {
            get
            {
                return DateTime.UtcNow.AddSeconds(10.0);
            }
        }
    }
}

