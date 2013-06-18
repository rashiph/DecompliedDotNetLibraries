namespace System.Web.Compilation
{
    using System;
    using System.Web.UI;

    internal class BuildResultNoCompileUserControl : BuildResultNoCompileTemplateControl
    {
        private PartialCachingAttribute _cachingAttribute;

        internal BuildResultNoCompileUserControl(Type baseType, TemplateParser parser) : base(baseType, parser)
        {
            UserControlParser parser2 = (UserControlParser) parser;
            OutputCacheParameters outputCacheParameters = parser2.OutputCacheParameters;
            if ((outputCacheParameters != null) && (outputCacheParameters.Duration > 0))
            {
                this._cachingAttribute = new PartialCachingAttribute(outputCacheParameters.Duration, outputCacheParameters.VaryByParam, outputCacheParameters.VaryByControl, outputCacheParameters.VaryByCustom, outputCacheParameters.SqlDependency, parser2.FSharedPartialCaching);
                this._cachingAttribute.ProviderName = parser2.Provider;
            }
        }

        internal PartialCachingAttribute CachingAttribute
        {
            get
            {
                return this._cachingAttribute;
            }
        }
    }
}

