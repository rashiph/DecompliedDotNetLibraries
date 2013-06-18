namespace System.Windows.Forms
{
    using System;
    using System.Drawing;

    public class ToolStripContentPanelRenderEventArgs : EventArgs
    {
        private System.Windows.Forms.ToolStripContentPanel contentPanel;
        private System.Drawing.Graphics graphics;
        private bool handled;

        public ToolStripContentPanelRenderEventArgs(System.Drawing.Graphics g, System.Windows.Forms.ToolStripContentPanel contentPanel)
        {
            this.contentPanel = contentPanel;
            this.graphics = g;
        }

        public System.Drawing.Graphics Graphics
        {
            get
            {
                return this.graphics;
            }
        }

        public bool Handled
        {
            get
            {
                return this.handled;
            }
            set
            {
                this.handled = value;
            }
        }

        public System.Windows.Forms.ToolStripContentPanel ToolStripContentPanel
        {
            get
            {
                return this.contentPanel;
            }
        }
    }
}

