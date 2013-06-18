namespace System.Web.UI.HtmlControls
{
    using System;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    internal sealed class SelectorStyleInfo
    {
        public string selector;
        public Style style;
        public IUrlResolutionService urlResolver;
    }
}

