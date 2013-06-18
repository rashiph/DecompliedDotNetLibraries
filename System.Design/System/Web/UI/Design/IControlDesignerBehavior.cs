namespace System.Web.UI.Design
{
    using System;

    [Obsolete("The recommended alternative is System.Web.UI.Design.IControlDesignerTag and System.Web.UI.Design.IControlDesignerView. http://go.microsoft.com/fwlink/?linkid=14202")]
    public interface IControlDesignerBehavior
    {
        void OnTemplateModeChanged();

        object DesignTimeElementView { get; }

        string DesignTimeHtml { get; set; }
    }
}

