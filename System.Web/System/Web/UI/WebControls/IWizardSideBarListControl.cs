namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Web.UI;

    internal interface IWizardSideBarListControl
    {
        event CommandEventHandler ItemCommand;

        event EventHandler<WizardSideBarListControlItemEventArgs> ItemDataBound;

        void DataBind();

        object DataSource { get; set; }

        IEnumerable Items { get; }

        ITemplate ItemTemplate { get; set; }

        int SelectedIndex { get; set; }
    }
}

