namespace System.Windows.Forms
{
    using System;

    public class StatusBarPanelClickEventArgs : MouseEventArgs
    {
        private readonly System.Windows.Forms.StatusBarPanel statusBarPanel;

        public StatusBarPanelClickEventArgs(System.Windows.Forms.StatusBarPanel statusBarPanel, MouseButtons button, int clicks, int x, int y) : base(button, clicks, x, y, 0)
        {
            this.statusBarPanel = statusBarPanel;
        }

        public System.Windows.Forms.StatusBarPanel StatusBarPanel
        {
            get
            {
                return this.statusBarPanel;
            }
        }
    }
}

