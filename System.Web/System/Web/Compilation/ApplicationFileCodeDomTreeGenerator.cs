namespace System.Web.Compilation
{
    using System;
    using System.Web.UI;

    internal class ApplicationFileCodeDomTreeGenerator : BaseCodeDomTreeGenerator
    {
        protected ApplicationFileParser _appParser;

        internal ApplicationFileCodeDomTreeGenerator(ApplicationFileParser appParser) : base(appParser)
        {
            this._appParser = appParser;
        }

        protected override bool IsGlobalAsaxGenerator
        {
            get
            {
                return true;
            }
        }
    }
}

