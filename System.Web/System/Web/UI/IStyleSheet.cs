namespace System.Web.UI
{
    using System;
    using System.Web.UI.WebControls;

    public interface IStyleSheet
    {
        void CreateStyleRule(Style style, IUrlResolutionService urlResolver, string selector);
        void RegisterStyle(Style style, IUrlResolutionService urlResolver);
    }
}

