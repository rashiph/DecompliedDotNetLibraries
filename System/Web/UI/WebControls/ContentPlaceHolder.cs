namespace System.Web.UI.WebControls
{
    using System.ComponentModel;
    using System.Web.UI;

    [ToolboxData("<{0}:ContentPlaceHolder runat=\"server\"></{0}:ContentPlaceHolder>"), ControlBuilder(typeof(ContentPlaceHolderBuilder)), Designer("System.Web.UI.Design.WebControls.ContentPlaceHolderDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ToolboxItemFilter("System.Web.UI"), ToolboxItemFilter("Microsoft.VisualStudio.Web.WebForms.MasterPageWebFormDesigner", ToolboxItemFilterType.Require)]
    public class ContentPlaceHolder : Control, INonBindingContainer, INamingContainer
    {
    }
}

