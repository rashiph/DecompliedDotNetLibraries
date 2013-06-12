namespace System.Web.Compilation
{
    using System;
    using System.Web.Configuration;
    using System.Web.UI;
    using System.Web.Util;

    internal class BuildResultCompiledTemplateType : BuildResultCompiledType
    {
        public BuildResultCompiledTemplateType()
        {
        }

        public BuildResultCompiledTemplateType(Type t) : base(t)
        {
        }

        protected override void ComputeHashCode(HashCodeCombiner hashCodeCombiner)
        {
            base.ComputeHashCode(hashCodeCombiner);
            PagesSection pagesConfig = MTConfigUtil.GetPagesConfig(base.VirtualPath);
            hashCodeCombiner.AddObject(Util.GetRecompilationHash(pagesConfig));
        }

        internal override BuildResultTypeCode GetCode()
        {
            return BuildResultTypeCode.BuildResultCompiledTemplateType;
        }
    }
}

