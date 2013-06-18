namespace System.Web.Compilation
{
    using System;
    using System.Reflection;

    internal class BuildResultCompiledAssembly : BuildResultCompiledAssemblyBase
    {
        private Assembly _assembly;

        internal BuildResultCompiledAssembly()
        {
        }

        internal BuildResultCompiledAssembly(Assembly a)
        {
            this._assembly = a;
        }

        internal override BuildResultTypeCode GetCode()
        {
            return BuildResultTypeCode.BuildResultCompiledAssembly;
        }

        internal override void GetPreservedAttributes(PreservationFileReader pfr)
        {
            base.GetPreservedAttributes(pfr);
            this.ResultAssembly = BuildResultCompiledAssemblyBase.GetPreservedAssembly(pfr);
        }

        internal override Assembly ResultAssembly
        {
            get
            {
                return this._assembly;
            }
            set
            {
                this._assembly = value;
            }
        }
    }
}

