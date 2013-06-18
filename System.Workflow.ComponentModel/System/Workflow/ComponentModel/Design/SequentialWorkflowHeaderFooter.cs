namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Runtime;

    public class SequentialWorkflowHeaderFooter
    {
        private System.Drawing.Image image;
        private bool isHeader = true;
        private SequentialWorkflowRootDesigner rootDesigner;
        private string text = string.Empty;
        internal Size textSize = Size.Empty;

        public SequentialWorkflowHeaderFooter(SequentialWorkflowRootDesigner parent, bool isHeader)
        {
            this.rootDesigner = parent;
            this.isHeader = isHeader;
        }

        public virtual void OnLayout(ActivityDesignerLayoutEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            if ((!string.IsNullOrEmpty(this.Text) && (e.DesignerTheme != null)) && (e.DesignerTheme.Font != null))
            {
                using (Font font = new Font(e.DesignerTheme.Font.FontFamily, e.DesignerTheme.Font.SizeInPoints + 1f, FontStyle.Bold))
                {
                    this.textSize = ActivityDesignerPaint.MeasureString(e.Graphics, font, this.Text, StringAlignment.Center, Size.Empty);
                }
            }
        }

        public virtual void OnPaint(ActivityDesignerPaintEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            if ((!string.IsNullOrEmpty(this.Text) && !this.TextRectangle.Size.IsEmpty) && ((e.DesignerTheme != null) && (e.DesignerTheme.Font != null)))
            {
                using (Font font = new Font(e.DesignerTheme.Font.FontFamily, e.DesignerTheme.Font.SizeInPoints + 1f, this.AssociatedDesigner.SmartTagVisible ? FontStyle.Bold : FontStyle.Regular))
                {
                    ActivityDesignerPaint.DrawText(e.Graphics, font, this.Text, this.TextRectangle, StringAlignment.Center, TextQuality.AntiAliased, e.DesignerTheme.ForegroundBrush);
                }
            }
            if ((this.Image != null) && !this.ImageRectangle.Size.IsEmpty)
            {
                ActivityDesignerPaint.DrawImage(e.Graphics, this.Image, this.ImageRectangle, DesignerContentAlignment.Fill);
            }
        }

        protected SequentialWorkflowRootDesigner AssociatedDesigner
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.rootDesigner;
            }
        }

        public virtual Rectangle Bounds
        {
            get
            {
                Rectangle empty = Rectangle.Empty;
                Size margin = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;
                Rectangle textRectangle = this.TextRectangle;
                Rectangle imageRectangle = this.ImageRectangle;
                if (!textRectangle.Size.IsEmpty || !imageRectangle.Size.IsEmpty)
                {
                    empty.Width = Math.Max(imageRectangle.Width, textRectangle.Width) + (2 * margin.Width);
                    empty.Height = margin.Height + imageRectangle.Height;
                    empty.Height += (imageRectangle.Height > 0) ? margin.Height : 0;
                    empty.Height += textRectangle.Height;
                    empty.Height += (textRectangle.Height > 0) ? margin.Height : 0;
                    Rectangle bounds = this.rootDesigner.Bounds;
                    empty.X = (bounds.Left + (bounds.Width / 2)) - (empty.Width / 2);
                    empty.Y = this.isHeader ? bounds.Top : (bounds.Bottom - empty.Height);
                }
                return empty;
            }
        }

        public virtual System.Drawing.Image Image
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.image;
            }
            set
            {
                if (this.image != value)
                {
                    this.image = value;
                    this.AssociatedDesigner.InternalPerformLayout();
                }
            }
        }

        public virtual Rectangle ImageRectangle
        {
            get
            {
                Rectangle empty = Rectangle.Empty;
                if (this.Image != null)
                {
                    ActivityDesignerTheme designerTheme = this.rootDesigner.DesignerTheme;
                    Size margin = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;
                    Rectangle bounds = this.rootDesigner.Bounds;
                    Rectangle textRectangle = this.TextRectangle;
                    empty.Size = designerTheme.ImageSize;
                    empty.X = (bounds.Left + (bounds.Width / 2)) - (empty.Width / 2);
                    if (this.isHeader)
                    {
                        empty.Y = bounds.Top + margin.Height;
                        empty.Y += textRectangle.Height;
                        empty.Y += (textRectangle.Height > 0) ? margin.Height : 0;
                        return empty;
                    }
                    empty.Y = bounds.Bottom - margin.Height;
                    empty.Y -= textRectangle.Height;
                    empty.Y -= (textRectangle.Height > 0) ? margin.Height : 0;
                    empty.Y -= empty.Height;
                }
                return empty;
            }
        }

        public virtual string Text
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.text;
            }
            set
            {
                if (this.text != value)
                {
                    this.text = value;
                    this.AssociatedDesigner.InternalPerformLayout();
                }
            }
        }

        public virtual Rectangle TextRectangle
        {
            get
            {
                Rectangle empty = Rectangle.Empty;
                if (!string.IsNullOrEmpty(this.Text))
                {
                    Size margin = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;
                    Rectangle bounds = this.rootDesigner.Bounds;
                    empty.Size = this.textSize;
                    empty.X = (bounds.Left + (bounds.Width / 2)) - (this.textSize.Width / 2);
                    empty.Y = this.isHeader ? (bounds.Top + margin.Height) : ((bounds.Bottom - margin.Height) - this.textSize.Height);
                }
                return empty;
            }
        }
    }
}

