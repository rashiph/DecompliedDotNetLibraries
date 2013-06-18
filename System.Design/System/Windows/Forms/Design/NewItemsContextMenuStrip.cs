namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.Windows.Forms;

    internal class NewItemsContextMenuStrip : GroupedContextMenuStrip
    {
        private IComponent component;
        private bool convertTo;
        private ToolStripItem currentItem;
        private EventHandler onClick;
        private IServiceProvider serviceProvider;

        public NewItemsContextMenuStrip(IComponent component, ToolStripItem currentItem, EventHandler onClick, bool convertTo, IServiceProvider serviceProvider)
        {
            this.component = component;
            this.onClick = onClick;
            this.convertTo = convertTo;
            this.serviceProvider = serviceProvider;
            this.currentItem = currentItem;
            IUIService service = serviceProvider.GetService(typeof(IUIService)) as IUIService;
            if (service != null)
            {
                base.Renderer = (ToolStripProfessionalRenderer) service.Styles["VsRenderer"];
            }
        }

        protected override void OnOpening(CancelEventArgs e)
        {
            base.Groups["StandardList"].Items.Clear();
            base.Groups["CustomList"].Items.Clear();
            base.Populated = false;
            foreach (ToolStripItem item in ToolStripDesignerUtils.GetStandardItemMenuItems(this.component, this.onClick, this.convertTo))
            {
                base.Groups["StandardList"].Items.Add(item);
                if (this.convertTo)
                {
                    ItemTypeToolStripMenuItem item2 = item as ItemTypeToolStripMenuItem;
                    if (((item2 != null) && (this.currentItem != null)) && (item2.ItemType == this.currentItem.GetType()))
                    {
                        item2.Enabled = false;
                    }
                }
            }
            foreach (ToolStripItem item3 in ToolStripDesignerUtils.GetCustomItemMenuItems(this.component, this.onClick, this.convertTo, this.serviceProvider))
            {
                base.Groups["CustomList"].Items.Add(item3);
                if (this.convertTo)
                {
                    ItemTypeToolStripMenuItem item4 = item3 as ItemTypeToolStripMenuItem;
                    if (((item4 != null) && (this.currentItem != null)) && (item4.ItemType == this.currentItem.GetType()))
                    {
                        item4.Enabled = false;
                    }
                }
            }
            base.OnOpening(e);
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            switch ((keyData & Keys.KeyCode))
            {
                case Keys.Left:
                case Keys.Right:
                    base.Close();
                    return true;
            }
            return base.ProcessDialogKey(keyData);
        }
    }
}

