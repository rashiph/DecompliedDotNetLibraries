namespace System.Web.Compilation
{
    using System.Web.UI;

    [BuildProviderAppliesTo(BuildProviderAppliesTo.Code | BuildProviderAppliesTo.Web)]
    internal class MasterPageBuildProvider : UserControlBuildProvider
    {
        internal override BaseCodeDomTreeGenerator CreateCodeDomTreeGenerator(TemplateParser parser)
        {
            return new MasterPageCodeDomTreeGenerator((MasterPageParser) parser);
        }

        internal override DependencyParser CreateDependencyParser()
        {
            return new MasterPageDependencyParser();
        }

        internal override BuildResultNoCompileTemplateControl CreateNoCompileBuildResult()
        {
            return new BuildResultNoCompileMasterPage(base.Parser.BaseType, base.Parser);
        }

        protected override TemplateParser CreateParser()
        {
            return new MasterPageParser();
        }
    }
}

