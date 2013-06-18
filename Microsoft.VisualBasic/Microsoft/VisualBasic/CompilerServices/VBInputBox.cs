namespace Microsoft.VisualBasic.CompilerServices
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Windows.Forms;

    internal sealed class VBInputBox : Form
    {
        private System.ComponentModel.Container components;
        private System.Windows.Forms.Label Label;
        private Button MyCancelButton;
        private Button OKButton;
        public string Output;
        private System.Windows.Forms.TextBox TextBox;

        internal VBInputBox()
        {
            this.Output = "";
            this.InitializeComponent();
        }

        internal VBInputBox(string Prompt, string Title, string DefaultResponse, int XPos, int YPos)
        {
            this.Output = "";
            this.InitializeComponent();
            this.InitializeInputBox(Prompt, Title, DefaultResponse, XPos, YPos);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            ComponentResourceManager manager = new ComponentResourceManager(typeof(VBInputBox));
            this.OKButton = new Button();
            this.MyCancelButton = new Button();
            this.TextBox = new System.Windows.Forms.TextBox();
            this.Label = new System.Windows.Forms.Label();
            this.SuspendLayout();
            manager.ApplyResources(this.OKButton, "OKButton", CultureInfo.CurrentUICulture);
            this.OKButton.Name = "OKButton";
            this.MyCancelButton.DialogResult = DialogResult.Cancel;
            manager.ApplyResources(this.MyCancelButton, "MyCancelButton", CultureInfo.CurrentUICulture);
            this.MyCancelButton.Name = "MyCancelButton";
            manager.ApplyResources(this.TextBox, "TextBox", CultureInfo.CurrentUICulture);
            this.TextBox.Name = "TextBox";
            manager.ApplyResources(this.Label, "Label", CultureInfo.CurrentUICulture);
            this.Label.Name = "Label";
            this.AcceptButton = this.OKButton;
            manager.ApplyResources(this, "$this", CultureInfo.CurrentUICulture);
            this.CancelButton = this.MyCancelButton;
            this.Controls.Add(this.TextBox);
            this.Controls.Add(this.Label);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.MyCancelButton);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VBInputBox";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void InitializeInputBox(string Prompt, string Title, string DefaultResponse, int XPos, int YPos)
        {
            this.Text = Title;
            this.Label.Text = Prompt;
            this.TextBox.Text = DefaultResponse;
            this.OKButton.Click += new EventHandler(this.OKButton_Click);
            this.MyCancelButton.Click += new EventHandler(this.MyCancelButton_Click);
            Graphics graphics = this.Label.CreateGraphics();
            SizeF ef = graphics.MeasureString(Prompt, this.Label.Font, this.Label.Width);
            graphics.Dispose();
            if (ef.Height > this.Label.Height)
            {
                int num = ((int) Math.Round((double) ef.Height)) - this.Label.Height;
                System.Windows.Forms.Label label = this.Label;
                label.Height += num;
                System.Windows.Forms.TextBox textBox = this.TextBox;
                textBox.Top += num;
                this.Height += num;
            }
            if ((XPos == -1) && (YPos == -1))
            {
                this.StartPosition = FormStartPosition.CenterScreen;
            }
            else
            {
                if (XPos == -1)
                {
                    XPos = 600;
                }
                if (YPos == -1)
                {
                    YPos = 350;
                }
                this.StartPosition = FormStartPosition.Manual;
                Point point2 = new Point(XPos, YPos);
                this.DesktopLocation = point2;
            }
        }

        private void MyCancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            this.Output = this.TextBox.Text;
            this.Close();
        }
    }
}

