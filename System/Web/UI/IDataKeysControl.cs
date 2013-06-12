namespace System.Web.UI
{
    using System;
    using System.Web.UI.WebControls;

    public interface IDataKeysControl
    {
        string[] ClientIDRowSuffix { get; }

        DataKeyArray ClientIDRowSuffixDataKeys { get; }
    }
}

