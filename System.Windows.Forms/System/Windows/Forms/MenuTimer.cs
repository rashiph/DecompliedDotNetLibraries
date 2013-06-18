namespace System.Windows.Forms
{
    using System;

    internal class MenuTimer
    {
        private Timer autoMenuExpandTimer = new Timer();
        private ToolStripMenuItem currentItem;
        private ToolStripMenuItem fromItem;
        private bool inTransition;
        private int quickShow = 1;
        private int slowShow;

        public MenuTimer()
        {
            this.autoMenuExpandTimer.Tick += new EventHandler(this.OnTick);
            this.slowShow = Math.Max(this.quickShow, SystemInformation.MenuShowDelay);
        }

        public void Cancel()
        {
            if (!this.InTransition)
            {
                this.CancelCore();
            }
        }

        public void Cancel(ToolStripMenuItem item)
        {
            if (!this.InTransition && (item == this.CurrentItem))
            {
                this.CancelCore();
            }
        }

        private void CancelCore()
        {
            this.autoMenuExpandTimer.Enabled = false;
            this.CurrentItem = null;
        }

        private void EndTransition(bool forceClose)
        {
            ToolStripMenuItem fromItem = this.fromItem;
            this.fromItem = null;
            if (this.InTransition)
            {
                this.InTransition = false;
                if (((forceClose || (((this.CurrentItem != null) && (this.CurrentItem != fromItem)) && this.CurrentItem.Selected)) && (fromItem != null)) && fromItem.HasDropDownItems)
                {
                    fromItem.HideDropDown();
                }
            }
        }

        internal void HandleToolStripMouseLeave(ToolStrip toolStrip)
        {
            if (this.InTransition && (toolStrip == this.fromItem.ParentInternal))
            {
                if (this.CurrentItem != null)
                {
                    this.CurrentItem.Select();
                }
            }
            else if (toolStrip.IsDropDown && (toolStrip.ActiveDropDowns.Count > 0))
            {
                ToolStripDropDown down = toolStrip.ActiveDropDowns[0] as ToolStripDropDown;
                ToolStripMenuItem item = (down == null) ? null : (down.OwnerItem as ToolStripMenuItem);
                if ((item != null) && item.Pressed)
                {
                    item.Select();
                }
            }
        }

        private void OnTick(object sender, EventArgs e)
        {
            this.autoMenuExpandTimer.Enabled = false;
            if (this.CurrentItem != null)
            {
                this.EndTransition(false);
                if (((this.CurrentItem != null) && !this.CurrentItem.IsDisposed) && (this.CurrentItem.Selected && ToolStripManager.ModalMenuFilter.InMenuMode))
                {
                    this.CurrentItem.OnMenuAutoExpand();
                }
            }
        }

        public void Start(ToolStripMenuItem item)
        {
            if (!this.InTransition)
            {
                this.StartCore(item);
            }
        }

        private void StartCore(ToolStripMenuItem item)
        {
            if (item != this.CurrentItem)
            {
                this.Cancel(this.CurrentItem);
            }
            this.CurrentItem = item;
            if (item != null)
            {
                this.CurrentItem = item;
                this.autoMenuExpandTimer.Interval = item.IsOnDropDown ? this.slowShow : this.quickShow;
                this.autoMenuExpandTimer.Enabled = true;
            }
        }

        public void Transition(ToolStripMenuItem fromItem, ToolStripMenuItem toItem)
        {
            if ((toItem == null) && this.InTransition)
            {
                this.Cancel();
                this.EndTransition(true);
            }
            else
            {
                if (this.fromItem != fromItem)
                {
                    this.fromItem = fromItem;
                    this.CancelCore();
                    this.StartCore(toItem);
                }
                this.CurrentItem = toItem;
                this.InTransition = true;
            }
        }

        private ToolStripMenuItem CurrentItem
        {
            get
            {
                return this.currentItem;
            }
            set
            {
                this.currentItem = value;
            }
        }

        public bool InTransition
        {
            get
            {
                return this.inTransition;
            }
            set
            {
                this.inTransition = value;
            }
        }
    }
}

