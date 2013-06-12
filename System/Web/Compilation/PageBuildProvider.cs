namespace System.Web.Compilation
{
    using System.Web.UI;

    [BuildProviderAppliesTo(BuildProviderAppliesTo.Web)]
    internal class PageBuildProvider : TemplateControlBuildProvider
    {
        internal override BaseCodeDomTreeGenerator CreateCodeDomTreeGenerator(TemplateParser parser)
        {
            return new PageCodeDomTreeGenerator((PageParser) parser);
        }

        internal override DependencyParser CreateDependencyParser()
        {
            return new PageDependencyParser();
        }

        internal override BuildResultNoCompileTemplateControl CreateNoCompileBuildResult()
        {
            return new BuildResultNoCompilePage(base.Parser.BaseType, base.Parser);
        }

        protected override TemplateParser CreateParser()
        {
            return new PageParser();
        }
    }
}

