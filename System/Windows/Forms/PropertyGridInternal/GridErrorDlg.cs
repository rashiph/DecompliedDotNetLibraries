namespace System.Windows.Forms.PropertyGridInternal
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    internal class GridErrorDlg : Form
    {
        private TableLayoutPanel buttonTableLayoutPanel;
        private Button cancelBtn;
        private Bitmap collapseImage;
        private TextBox details;
        private Button detailsBtn;
        private Bitmap expandImage;
        private Label lblMessage;
        private Button okBtn;
        private TableLayoutPanel overarchingTableLayoutPanel;
        private PropertyGrid ownerGrid;
        private PictureBox pictureBox;
        private TableLayoutPanel pictureLabelTableLayoutPanel;

        public GridErrorDlg(PropertyGrid owner)
        {
            this.ownerGrid = owner;
            this.expandImage = new Bitmap(typeof(ThreadExceptionDialog), "down.bmp");
            this.expandImage.MakeTransparent();
            this.collapseImage = new Bitmap(typeof(ThreadExceptionDialog), "up.bmp");
            this.collapseImage.MakeTransparent();
            this.InitializeComponent();
            foreach (Control control in base.Controls)
            {
                if (control.SupportsUseCompatibleTextRendering)
                {
                    control.UseCompatibleTextRenderingInt = this.ownerGrid.UseCompatibleTextRendering;
                }
            }
            this.pictureBox.Image = SystemIcons.Warning.ToBitmap();
            this.detailsBtn.Text = " " + System.Windows.Forms.SR.GetString("ExDlgShowDetails");
            this.details.AccessibleName = System.Windows.Forms.SR.GetString("ExDlgDetailsText");
            this.okBtn.Text = System.Windows.Forms.SR.GetString("ExDlgOk");
            this.cancelBtn.Text = System.Windows.Forms.SR.GetString("ExDlgCancel");
            this.detailsBtn.Image = this.expandImage;
        }

        private void DetailsClick(object sender, EventArgs devent)
        {
            int num = this.details.Height + 8;
            if (this.details.Visible)
            {
                this.detailsBtn.Image = this.expandImage;
                base.Height -= num;
            }
            else
            {
                this.detailsBtn.Image = this.collapseImage;
                this.details.Width = this.overarchingTableLayoutPanel.Width - this.details.Margin.Horizontal;
                base.Height += num;
            }
            this.details.Visible = !this.details.Visible;
        }

        private void InitializeComponent()
        {
            if (IsRTLResources)
            {
                this.RightToLeft = RightToLeft.Yes;
            }
            this.detailsBtn = new Button();
            this.overarchingTableLayoutPanel = new TableLayoutPanel();
            this.buttonTableLayoutPanel = new TableLayoutPanel();
            this.okBtn = new Button();
            this.cancelBtn = new Button();
            this.pictureLabelTableLayoutPanel = new TableLayoutPanel();
            this.lblMessage = new Label();
            this.pictureBox = new PictureBox();
            this.details = new TextBox();
            this.overarchingTableLayoutPanel.SuspendLayout();
            this.buttonTableLayoutPanel.SuspendLayout();
            this.pictureLabelTableLayoutPanel.SuspendLayout();
            ((ISupportInitialize) this.pictureBox).BeginInit();
            base.SuspendLayout();
            this.lblMessage.Location = new Point(0x49, 30);
            this.lblMessage.Margin = new Padding(3, 30, 3, 0);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new Size(0xd0, 0x2b);
            this.lblMessage.TabIndex = 0;
            this.pictureBox.Location = new Point(3, 3);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new Size(0x40, 0x40);
            this.pictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
            this.pictureBox.TabIndex = 5;
            this.pictureBox.TabStop = false;
            this.detailsBtn.ImageAlign = ContentAlignment.MiddleLeft;
            this.detailsBtn.Location = new Point(3, 3);
            this.detailsBtn.Margin = new Padding(12, 3, 0x1d, 3);
            this.detailsBtn.Name = "detailsBtn";
            this.detailsBtn.Size = new Size(100, 0x17);
            this.detailsBtn.TabIndex = 4;
            this.detailsBtn.Click += new EventHandler(this.DetailsClick);
            this.overarchingTableLayoutPanel.AutoSize = true;
            this.overarchingTableLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.overarchingTableLayoutPanel.ColumnCount = 1;
            this.overarchingTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
            this.overarchingTableLayoutPanel.Controls.Add(this.buttonTableLayoutPanel, 0, 1);
            this.overarchingTableLayoutPanel.Controls.Add(this.pictureLabelTableLayoutPanel, 0, 0);
            this.overarchingTableLayoutPanel.Location = new Point(1, 0);
            this.overarchingTableLayoutPanel.MinimumSize = new Size(0x117, 50);
            this.overarchingTableLayoutPanel.Name = "overarchingTableLayoutPanel";
            this.overarchingTableLayoutPanel.RowCount = 2;
            this.overarchingTableLayoutPanel.RowStyles.Add(new RowStyle());
            this.overarchingTableLayoutPanel.RowStyles.Add(new RowStyle());
            this.overarchingTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20f));
            this.overarchingTableLayoutPanel.Size = new Size(290, 0x6c);
            this.overarchingTableLayoutPanel.TabIndex = 6;
            this.buttonTableLayoutPanel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.buttonTableLayoutPanel.AutoSize = true;
            this.buttonTableLayoutPanel.ColumnCount = 3;
            this.overarchingTableLayoutPanel.SetColumnSpan(this.buttonTableLayoutPanel, 2);
            this.buttonTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.buttonTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
            this.buttonTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
            this.buttonTableLayoutPanel.Controls.Add(this.cancelBtn, 2, 0);
            this.buttonTableLayoutPanel.Controls.Add(this.okBtn, 1, 0);
            this.buttonTableLayoutPanel.Controls.Add(this.detailsBtn, 0, 0);
            this.buttonTableLayoutPanel.Location = new Point(0, 0x4f);
            this.buttonTableLayoutPanel.Name = "buttonTableLayoutPanel";
            this.buttonTableLayoutPanel.RowCount = 1;
            this.buttonTableLayoutPanel.RowStyles.Add(new RowStyle());
            this.buttonTableLayoutPanel.Size = new Size(290, 0x1d);
            this.buttonTableLayoutPanel.TabIndex = 8;
            this.okBtn.AutoSize = true;
            this.okBtn.DialogResult = DialogResult.OK;
            this.okBtn.Location = new Point(0x83, 3);
            this.okBtn.Name = "okBtn";
            this.okBtn.Size = new Size(0x4b, 0x17);
            this.okBtn.TabIndex = 1;
            this.okBtn.Click += new EventHandler(this.OnButtonClick);
            this.cancelBtn.AutoSize = true;
            this.cancelBtn.DialogResult = DialogResult.Cancel;
            this.cancelBtn.Location = new Point(0xd4, 3);
            this.cancelBtn.Margin = new Padding(0, 3, 3, 3);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new Size(0x4b, 0x17);
            this.cancelBtn.TabIndex = 2;
            this.cancelBtn.Click += new EventHandler(this.OnButtonClick);
            this.pictureLabelTableLayoutPanel.ColumnCount = 2;
            this.pictureLabelTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
            this.pictureLabelTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.pictureLabelTableLayoutPanel.Controls.Add(this.lblMessage, 1, 0);
            this.pictureLabelTableLayoutPanel.Controls.Add(this.pictureBox, 0, 0);
            this.pictureLabelTableLayoutPanel.Dock = DockStyle.Fill;
            this.pictureLabelTableLayoutPanel.Location = new Point(3, 3);
            this.pictureLabelTableLayoutPanel.Name = "pictureLabelTableLayoutPanel";
            this.pictureLabelTableLayoutPanel.RowCount = 1;
            this.pictureLabelTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 70f));
            this.pictureLabelTableLayoutPanel.Size = new Size(0x11c, 0x49);
            this.pictureLabelTableLayoutPanel.TabIndex = 4;
            this.details.Location = new Point(4, 0x72);
            this.details.Multiline = true;
            this.details.Name = "details";
            this.details.ReadOnly = true;
            this.details.ScrollBars = ScrollBars.Vertical;
            this.details.Size = new Size(0x111, 100);
            this.details.TabIndex = 3;
            this.details.TabStop = false;
            this.details.Visible = false;
            base.AcceptButton = this.okBtn;
            this.AutoSize = true;
            base.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            base.CancelButton = this.cancelBtn;
            base.ClientSize = new Size(0x12b, 0x71);
            base.Controls.Add(this.details);
            base.Controls.Add(this.overarchingTableLayoutPanel);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "Form1";
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.CenterScreen;
            this.overarchingTableLayoutPanel.ResumeLayout(false);
            this.overarchingTableLayoutPanel.PerformLayout();
            this.buttonTableLayoutPanel.ResumeLayout(false);
            this.buttonTableLayoutPanel.PerformLayout();
            this.pictureLabelTableLayoutPanel.ResumeLayout(false);
            ((ISupportInitialize) this.pictureBox).EndInit();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void OnButtonClick(object s, EventArgs e)
        {
            base.DialogResult = ((Button) s).DialogResult;
            base.Close();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            if (base.Visible)
            {
                using (Graphics graphics = base.CreateGraphics())
                {
                    int num = (int) Math.Ceiling((double) PropertyGrid.MeasureTextHelper.MeasureText(this.ownerGrid, graphics, this.detailsBtn.Text, this.detailsBtn.Font).Width);
                    num += this.detailsBtn.Image.Width;
                    this.detailsBtn.Width = (int) Math.Ceiling((double) (num * (this.ownerGrid.UseCompatibleTextRendering ? 1.15f : 1.4f)));
                    this.detailsBtn.Height = this.okBtn.Height;
                }
                if (this.details.Visible)
                {
                    this.DetailsClick(this.details, EventArgs.Empty);
                }
            }
            this.okBtn.Focus();
        }

        public string Details
        {
            set
            {
                this.details.Text = value;
            }
        }

        private static bool IsRTLResources
        {
            get
            {
                return (System.Windows.Forms.SR.GetString("RTL") != "RTL_False");
            }
        }

        public string Message
        {
            set
            {
                this.lblMessage.Text = value;
            }
        }
    }
}

