namespace System.Windows.Forms
{
    using System;
    using System.Drawing;

    public class ToolStripPanelRenderEventArgs : EventArgs
    {
        private System.Drawing.Graphics graphics;
        private bool handled;
        private System.Windows.Forms.ToolStripPanel toolStripPanel;

        public ToolStripPanelRenderEventArgs(System.Drawing.Graphics g, System.Windows.Forms.ToolStripPanel toolStripPanel)
        {
            this.toolStripPanel = toolStripPanel;
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

        public System.Windows.Forms.ToolStripPanel ToolStripPanel
        {
            get
            {
                return this.toolStripPanel;
            }
        }
    }
}

