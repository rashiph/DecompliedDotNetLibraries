namespace System.Web.UI.Design
{
    using System;

    [Flags]
    public enum ViewFlags
    {
        CustomPaint = 1,
        DesignTimeHtmlRequiresLoadComplete = 2,
        TemplateEditing = 4
    }
}

