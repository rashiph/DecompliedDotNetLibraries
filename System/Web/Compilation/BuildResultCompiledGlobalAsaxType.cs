namespace System.Web.Compilation
{
    using System;

    internal class BuildResultCompiledGlobalAsaxType : BuildResultCompiledType
    {
        public BuildResultCompiledGlobalAsaxType()
        {
        }

        public BuildResultCompiledGlobalAsaxType(Type t) : base(t)
        {
        }

        internal override BuildResultTypeCode GetCode()
        {
            return BuildResultTypeCode.BuildResultCompiledGlobalAsaxType;
        }

        internal bool HasAppOrSessionObjects
        {
            get
            {
                return this._flags[0x80000];
            }
            set
            {
                this._flags[0x80000] = value;
            }
        }
    }
}

