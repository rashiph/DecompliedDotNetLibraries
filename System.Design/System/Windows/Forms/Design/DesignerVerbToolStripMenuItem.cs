namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel.Design;
    using System.Windows.Forms;

    internal class DesignerVerbToolStripMenuItem : ToolStripMenuItem
    {
        private DesignerVerb verb;

        public DesignerVerbToolStripMenuItem(DesignerVerb verb)
        {
            this.verb = verb;
            this.Text = verb.Text;
            this.RefreshItem();
        }

        protected override void OnClick(EventArgs e)
        {
            if (this.verb != null)
            {
                this.verb.Invoke();
            }
        }

        public void RefreshItem()
        {
            if (this.verb != null)
            {
                base.Visible = this.verb.Visible;
                this.Enabled = this.verb.Enabled;
                base.Checked = this.verb.Checked;
            }
        }
    }
}

