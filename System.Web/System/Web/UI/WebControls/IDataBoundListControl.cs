namespace System.Web.UI.WebControls
{
    using System;

    public interface IDataBoundListControl : IDataBoundControl
    {
        string[] ClientIDRowSuffix { get; set; }

        DataKeyArray DataKeys { get; }

        bool EnablePersistedSelection { get; set; }

        DataKey SelectedDataKey { get; }

        int SelectedIndex { get; set; }
    }
}

