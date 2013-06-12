namespace System.Web.Compilation
{
    using System;
    using System.Collections;

    public sealed class BuildDependencySet
    {
        private BuildResult _result;

        internal BuildDependencySet(BuildResult result)
        {
            this._result = result;
        }

        public string HashCode
        {
            get
            {
                return this._result.VirtualPathDependenciesHash;
            }
        }

        public IEnumerable VirtualPaths
        {
            get
            {
                return this._result.VirtualPathDependencies;
            }
        }
    }
}

