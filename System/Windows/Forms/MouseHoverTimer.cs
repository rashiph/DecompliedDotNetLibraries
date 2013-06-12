namespace System.Windows.Forms
{
    using System;

    internal class MouseHoverTimer : IDisposable
    {
        private ToolStripItem currentItem;
        private Timer mouseHoverTimer = new Timer();
        private const int SPI_GETMOUSEHOVERTIME_WIN9X = 400;

        public MouseHoverTimer()
        {
            int mouseHoverTime = SystemInformation.MouseHoverTime;
            if (mouseHoverTime == 0)
            {
                mouseHoverTime = 400;
            }
            this.mouseHoverTimer.Interval = mouseHoverTime;
            this.mouseHoverTimer.Tick += new EventHandler(this.OnTick);
        }

        public void Cancel()
        {
            this.mouseHoverTimer.Enabled = false;
            this.currentItem = null;
        }

        public void Cancel(ToolStripItem item)
        {
            if (item == this.currentItem)
            {
                this.Cancel();
            }
        }

        public void Dispose()
        {
            if (this.mouseHoverTimer != null)
            {
                this.Cancel();
                this.mouseHoverTimer.Dispose();
                this.mouseHoverTimer = null;
            }
        }

        private void OnTick(object sender, EventArgs e)
        {
            this.mouseHoverTimer.Enabled = false;
            if ((this.currentItem != null) && !this.currentItem.IsDisposed)
            {
                this.currentItem.FireEvent(EventArgs.Empty, ToolStripItemEventType.MouseHover);
            }
        }

        public void Start(ToolStripItem item)
        {
            if (item != this.currentItem)
            {
                this.Cancel(this.currentItem);
            }
            this.currentItem = item;
            if (this.currentItem != null)
            {
                this.mouseHoverTimer.Enabled = true;
            }
        }
    }
}

