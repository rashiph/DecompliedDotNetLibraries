namespace System.Windows.Forms.PropertyGridInternal
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal class DocComment : PropertyGrid.SnappableControl
    {
        protected const int CBORDER = 3;
        protected const int CXDEF = 0;
        protected const int CYDEF = 0x3b;
        private string fullDesc;
        protected int lineHeight;
        private Label m_labelDesc;
        private Label m_labelTitle;
        protected const int MIN_LINES = 2;
        private bool needUpdateUIWithFont;
        internal Rectangle rect;

        internal DocComment(PropertyGrid owner) : base(owner)
        {
            this.needUpdateUIWithFont = true;
            this.rect = Rectangle.Empty;
            base.SuspendLayout();
            this.m_labelTitle = new Label();
            this.m_labelTitle.UseMnemonic = false;
            this.m_labelTitle.Cursor = Cursors.Default;
            this.m_labelDesc = new Label();
            this.m_labelDesc.AutoEllipsis = true;
            this.m_labelDesc.Cursor = Cursors.Default;
            this.UpdateTextRenderingEngine();
            base.Controls.Add(this.m_labelTitle);
            base.Controls.Add(this.m_labelDesc);
            base.Size = new Size(0, 0x3b);
            this.Text = System.Windows.Forms.SR.GetString("PBRSDocCommentPaneTitle");
            base.SetStyle(ControlStyles.Selectable, false);
            base.ResumeLayout(false);
        }

        public override int GetOptimalHeight(int width)
        {
            this.UpdateUIWithFont();
            int height = this.m_labelTitle.Size.Height;
            if (base.ownerGrid.IsHandleCreated && !base.IsHandleCreated)
            {
                base.CreateControl();
            }
            Graphics g = this.m_labelDesc.CreateGraphicsInternal();
            Size size = Size.Ceiling(PropertyGrid.MeasureTextHelper.MeasureText(base.ownerGrid, g, this.m_labelTitle.Text, this.Font, width));
            g.Dispose();
            height += (size.Height * 2) + 2;
            return Math.Max(height + 4, 0x3b);
        }

        internal virtual void LayoutWindow()
        {
        }

        protected override void OnFontChanged(EventArgs e)
        {
            this.needUpdateUIWithFont = true;
            base.PerformLayout();
            base.OnFontChanged(e);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            this.UpdateUIWithFont();
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            this.UpdateUIWithFont();
            Size clientSize = base.ClientSize;
            clientSize.Width = Math.Max(0, clientSize.Width - 6);
            clientSize.Height = Math.Max(0, clientSize.Height - 6);
            this.m_labelTitle.SetBounds(this.m_labelTitle.Top, this.m_labelTitle.Left, clientSize.Width, Math.Min(this.lineHeight, clientSize.Height), BoundsSpecified.Size);
            this.m_labelDesc.SetBounds(this.m_labelDesc.Top, this.m_labelDesc.Left, clientSize.Width, Math.Max(0, (clientSize.Height - this.lineHeight) - 1), BoundsSpecified.Size);
            this.m_labelDesc.Text = this.fullDesc;
            this.m_labelDesc.AccessibleName = this.fullDesc;
            base.OnLayout(e);
        }

        protected override void OnResize(EventArgs e)
        {
            Rectangle clientRectangle = base.ClientRectangle;
            if (!this.rect.IsEmpty && (clientRectangle.Width > this.rect.Width))
            {
                Rectangle rc = new Rectangle(this.rect.Width - 1, 0, (clientRectangle.Width - this.rect.Width) + 1, this.rect.Height);
                base.Invalidate(rc);
            }
            this.rect = clientRectangle;
            base.OnResize(e);
        }

        public virtual void SetComment(string title, string desc)
        {
            if (this.m_labelDesc.Text != title)
            {
                this.m_labelTitle.Text = title;
            }
            if (desc != this.fullDesc)
            {
                this.fullDesc = desc;
                this.m_labelDesc.Text = this.fullDesc;
                this.m_labelDesc.AccessibleName = this.fullDesc;
            }
        }

        public override int SnapHeightRequest(int cyNew)
        {
            this.UpdateUIWithFont();
            int num = Math.Max(2, cyNew / this.lineHeight);
            return (1 + (num * this.lineHeight));
        }

        internal void UpdateTextRenderingEngine()
        {
            this.m_labelTitle.UseCompatibleTextRendering = base.ownerGrid.UseCompatibleTextRendering;
            this.m_labelDesc.UseCompatibleTextRendering = base.ownerGrid.UseCompatibleTextRendering;
        }

        private void UpdateUIWithFont()
        {
            if (base.IsHandleCreated && this.needUpdateUIWithFont)
            {
                try
                {
                    this.m_labelTitle.Font = new Font(this.Font, FontStyle.Bold);
                }
                catch
                {
                }
                this.lineHeight = this.Font.Height + 2;
                this.m_labelTitle.Location = new Point(3, 3);
                this.m_labelDesc.Location = new Point(3, 3 + this.lineHeight);
                this.needUpdateUIWithFont = false;
                base.PerformLayout();
            }
        }

        public virtual int Lines
        {
            get
            {
                this.UpdateUIWithFont();
                return (base.Height / this.lineHeight);
            }
            set
            {
                this.UpdateUIWithFont();
                base.Size = new Size(base.Width, 1 + (value * this.lineHeight));
            }
        }
    }
}

