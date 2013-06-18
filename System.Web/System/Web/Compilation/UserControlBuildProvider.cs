namespace System.Web.Compilation
{
    using System.Web.UI;

    [BuildProviderAppliesTo(BuildProviderAppliesTo.Code | BuildProviderAppliesTo.Web)]
    internal class UserControlBuildProvider : TemplateControlBuildProvider
    {
        internal override BaseCodeDomTreeGenerator CreateCodeDomTreeGenerator(TemplateParser parser)
        {
            return new UserControlCodeDomTreeGenerator((UserControlParser) parser);
        }

        internal override DependencyParser CreateDependencyParser()
        {
            return new UserControlDependencyParser();
        }

        internal override BuildResultNoCompileTemplateControl CreateNoCompileBuildResult()
        {
            return new BuildResultNoCompileUserControl(base.Parser.BaseType, base.Parser);
        }

        protected override TemplateParser CreateParser()
        {
            return new UserControlParser();
        }
    }
}

