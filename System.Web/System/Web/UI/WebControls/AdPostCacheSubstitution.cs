namespace System.Web.UI.WebControls
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Web;
    using System.Web.UI;

    internal class AdPostCacheSubstitution
    {
        private AdRotator _adRotatorHelper;

        private AdPostCacheSubstitution()
        {
        }

        internal AdPostCacheSubstitution(AdRotator adRotator)
        {
            this._adRotatorHelper = new AdRotator();
            this._adRotatorHelper.CopyFrom(adRotator);
            this._adRotatorHelper.IsPostCacheAdHelper = true;
            this._adRotatorHelper.Page = new Page();
        }

        internal void RegisterPostCacheCallBack(HttpContext context, Page page, HtmlTextWriter writer)
        {
            HttpResponseSubstitutionCallback callback = new HttpResponseSubstitutionCallback(this.Render);
            context.Response.WriteSubstitution(callback);
        }

        internal string Render(HttpContext context)
        {
            StringWriter tw = new StringWriter(CultureInfo.CurrentCulture);
            HtmlTextWriter writer = this._adRotatorHelper.Page.CreateHtmlTextWriter(tw);
            this._adRotatorHelper.RenderControl(writer);
            return tw.ToString();
        }
    }
}

