namespace System.Web.Compilation
{
    using System;
    using System.CodeDom;
    using System.Web.UI;

    internal class UserControlCodeDomTreeGenerator : TemplateControlCodeDomTreeGenerator
    {
        protected UserControlParser _ucParser;

        internal UserControlCodeDomTreeGenerator(UserControlParser ucParser) : base(ucParser)
        {
            this._ucParser = ucParser;
        }

        protected override void GenerateClassAttributes()
        {
            base.GenerateClassAttributes();
            if ((base._sourceDataClass != null) && (this.Parser.OutputCacheParameters != null))
            {
                OutputCacheParameters outputCacheParameters = this.Parser.OutputCacheParameters;
                if (outputCacheParameters.Duration > 0)
                {
                    CodeAttributeDeclaration declaration = new CodeAttributeDeclaration("System.Web.UI.PartialCachingAttribute");
                    CodeAttributeArgument argument = new CodeAttributeArgument(new CodePrimitiveExpression(outputCacheParameters.Duration));
                    declaration.Arguments.Add(argument);
                    argument = new CodeAttributeArgument(new CodePrimitiveExpression(outputCacheParameters.VaryByParam));
                    declaration.Arguments.Add(argument);
                    argument = new CodeAttributeArgument(new CodePrimitiveExpression(outputCacheParameters.VaryByControl));
                    declaration.Arguments.Add(argument);
                    argument = new CodeAttributeArgument(new CodePrimitiveExpression(outputCacheParameters.VaryByCustom));
                    declaration.Arguments.Add(argument);
                    argument = new CodeAttributeArgument(new CodePrimitiveExpression(outputCacheParameters.SqlDependency));
                    declaration.Arguments.Add(argument);
                    argument = new CodeAttributeArgument(new CodePrimitiveExpression(this.Parser.FSharedPartialCaching));
                    declaration.Arguments.Add(argument);
                    if (MultiTargetingUtil.IsTargetFramework40OrAbove)
                    {
                        argument = new CodeAttributeArgument("ProviderName", new CodePrimitiveExpression(this.Parser.Provider));
                        declaration.Arguments.Add(argument);
                    }
                    base._sourceDataClass.CustomAttributes.Add(declaration);
                }
            }
        }

        private UserControlParser Parser
        {
            get
            {
                return this._ucParser;
            }
        }
    }
}

