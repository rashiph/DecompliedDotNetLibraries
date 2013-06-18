namespace System.Web.UI.Design
{
    using System;
    using System.Windows.Forms;

    public interface IWebFormsBuilderUIService
    {
        string BuildColor(Control owner, string initialColor);
        string BuildUrl(Control owner, string initialUrl, string baseUrl, string caption, string filter, UrlBuilderOptions options);
    }
}

