namespace System.Web.UI
{
    using System;

    public interface IUserControlDesignerAccessor
    {
        string InnerText { get; set; }

        string TagName { get; set; }
    }
}

