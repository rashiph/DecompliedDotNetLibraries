namespace System.Web.UI.Design.Util
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    internal abstract class TaskFormBase : DesignerForm
    {
        private Label _bottomDividerLabel;
        private Label _captionLabel;
        private PictureBox _glyphPictureBox;
        private Panel _headerPanel;
        private Panel _taskPanel;

        public TaskFormBase(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this.InitializeComponent();
            this.InitializeUI();
        }

        private void InitializeComponent()
        {
            this._taskPanel = new Panel();
            this._bottomDividerLabel = new Label();
            this._captionLabel = new Label();
            this._headerPanel = new Panel();
            this._glyphPictureBox = new PictureBox();
            this._headerPanel.SuspendLayout();
            ((ISupportInitialize) this._glyphPictureBox).BeginInit();
            base.SuspendLayout();
            this._taskPanel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this._taskPanel.Location = new Point(14, 0x4e);
            this._taskPanel.Name = "_taskPanel";
            this._taskPanel.Size = new Size(0x220, 0x112);
            this._taskPanel.TabIndex = 30;
            this._bottomDividerLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this._bottomDividerLabel.BackColor = SystemColors.Window;
            this._bottomDividerLabel.Location = new Point(0, 0x16e);
            this._bottomDividerLabel.Name = "_bottomDividerLabel";
            this._bottomDividerLabel.Size = new Size(0x23c, 1);
            this._bottomDividerLabel.TabIndex = 40;
            this._headerPanel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._headerPanel.BackColor = SystemColors.Window;
            this._headerPanel.Controls.Add(this._glyphPictureBox);
            this._headerPanel.Controls.Add(this._captionLabel);
            this._headerPanel.Location = new Point(0, 0);
            this._headerPanel.Name = "_headerPanel";
            this._headerPanel.Size = new Size(0x23c, 0x40);
            this._headerPanel.TabIndex = 10;
            this._glyphPictureBox.Location = new Point(0, 0);
            this._glyphPictureBox.Name = "_glyphPictureBox";
            this._glyphPictureBox.Size = new Size(0x41, 0x40);
            this._glyphPictureBox.TabIndex = 20;
            this._glyphPictureBox.TabStop = false;
            this._captionLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._captionLabel.Location = new Point(0x47, 0x11);
            this._captionLabel.Name = "_captionLabel";
            this._captionLabel.Size = new Size(0x1e7, 0x2f);
            this._captionLabel.TabIndex = 10;
            base.ClientSize = new Size(0x23c, 0x1a0);
            base.Controls.Add(this._headerPanel);
            base.Controls.Add(this._bottomDividerLabel);
            base.Controls.Add(this._taskPanel);
            this.MinimumSize = new Size(580, 450);
            base.Name = "TaskForm";
            base.SizeGripStyle = SizeGripStyle.Show;
            this._headerPanel.ResumeLayout(false);
            ((ISupportInitialize) this._glyphPictureBox).EndInit();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void InitializeUI()
        {
            this.UpdateFonts();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            this.UpdateFonts();
        }

        private void UpdateFonts()
        {
            this._captionLabel.Font = new Font(this.Font.FontFamily, this.Font.Size + 2f, FontStyle.Bold, this.Font.Unit);
        }

        protected Label CaptionLabel
        {
            get
            {
                return this._captionLabel;
            }
        }

        public Image Glyph
        {
            get
            {
                return this._glyphPictureBox.Image;
            }
            set
            {
                this._glyphPictureBox.Image = value;
            }
        }

        protected Panel TaskPanel
        {
            get
            {
                return this._taskPanel;
            }
        }
    }
}

