namespace System.Web.UI.WebControls.WebParts
{
    using System;

    public interface IWebEditable
    {
        EditorPartCollection CreateEditorParts();

        object WebBrowsableObject { get; }
    }
}

