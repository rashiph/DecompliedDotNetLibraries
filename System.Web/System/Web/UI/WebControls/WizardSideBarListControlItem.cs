namespace System.Web.UI.WebControls
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Web.UI;

    internal sealed class WizardSideBarListControlItem
    {
        private Control _container;

        public WizardSideBarListControlItem(object dataItem, ListItemType itemType, int itemIndex, Control container)
        {
            this.DataItem = dataItem;
            this.ItemType = itemType;
            this.ItemIndex = itemIndex;
            this._container = container;
        }

        internal Control FindControl(string id)
        {
            return this._container.FindControl(id);
        }

        public object DataItem { get; private set; }

        public int ItemIndex { get; private set; }

        public ListItemType ItemType { get; private set; }
    }
}

