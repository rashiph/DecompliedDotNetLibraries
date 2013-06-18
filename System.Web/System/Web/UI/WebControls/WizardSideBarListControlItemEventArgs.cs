namespace System.Web.UI.WebControls
{
    using System;
    using System.Runtime.CompilerServices;

    internal sealed class WizardSideBarListControlItemEventArgs : EventArgs
    {
        public WizardSideBarListControlItemEventArgs(WizardSideBarListControlItem item)
        {
            this.Item = item;
        }

        public WizardSideBarListControlItem Item { get; private set; }
    }
}

