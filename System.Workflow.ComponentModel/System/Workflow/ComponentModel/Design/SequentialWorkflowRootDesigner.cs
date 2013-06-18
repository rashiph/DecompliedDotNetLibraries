namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Runtime;

    public class SequentialWorkflowRootDesigner : SequentialActivityDesigner
    {
        private WorkflowFooter footer;
        private static readonly System.Drawing.Image FooterImage = DR.GetImage("EndWorkflow");
        private WorkflowHeader header;
        private const int HeaderFooterSizeIncr = 8;
        private static readonly System.Drawing.Image HeaderImage = DR.GetImage("StartWorkflow");
        private static readonly Size MinSize = new Size(240, 240);
        private static readonly Size PageStripItemSize = new Size(0x18, 20);

        public override bool CanBeParentedTo(CompositeActivityDesigner parentActivityDesigner)
        {
            return false;
        }

        protected override ReadOnlyCollection<Point> GetInnerConnections(DesignerEdges edges)
        {
            List<Point> list = new List<Point>(base.GetInnerConnections(edges));
            if (((list.Count > 0) && (this.Footer != null)) && ((edges & DesignerEdges.Bottom) > DesignerEdges.None))
            {
                Point point = list[list.Count - 1];
                Point point2 = list[list.Count - 1];
                list[list.Count - 1] = new Point(point.X, point2.Y - this.Footer.Bounds.Height);
            }
            return list.AsReadOnly();
        }

        internal void InternalPerformLayout()
        {
            base.PerformLayout();
        }

        protected override Size OnLayoutSize(ActivityDesignerLayoutEventArgs e)
        {
            Size size = base.OnLayoutSize(e);
            WorkflowFooter footer = this.Footer as WorkflowFooter;
            if (footer != null)
            {
                size.Height += (footer.ImageRectangle.Height + (2 * e.AmbientTheme.Margin.Height)) + footer.FooterBarRectangle.Size.Height;
            }
            if (this.Header != null)
            {
                this.Header.OnLayout(e);
            }
            if (this.Footer != null)
            {
                this.Footer.OnLayout(e);
            }
            return size;
        }

        protected override void OnPaint(ActivityDesignerPaintEventArgs e)
        {
            base.OnPaint(e);
            CompositeDesignerTheme designerTheme = e.DesignerTheme as CompositeDesignerTheme;
            if (designerTheme != null)
            {
                if (designerTheme.WatermarkImage != null)
                {
                    Rectangle bounds = base.Bounds;
                    bounds.Inflate(-e.AmbientTheme.Margin.Width, -e.AmbientTheme.Margin.Height);
                    ActivityDesignerPaint.GetRectangleFromAlignment(designerTheme.WatermarkAlignment, bounds, designerTheme.WatermarkImage.Size);
                }
                if (this.Header != null)
                {
                    this.Header.OnPaint(e);
                }
                if (this.Footer != null)
                {
                    this.Footer.OnPaint(e);
                }
            }
        }

        protected override void OnSmartTagVisibilityChanged(bool visible)
        {
            base.OnSmartTagVisibilityChanged(visible);
            if ((this.Header != null) && !this.Header.TextRectangle.IsEmpty)
            {
                base.Invalidate(this.Header.TextRectangle);
            }
        }

        public override bool CanExpandCollapse
        {
            get
            {
                return false;
            }
        }

        protected virtual SequentialWorkflowHeaderFooter Footer
        {
            get
            {
                if (this.footer == null)
                {
                    this.footer = new WorkflowFooter(this);
                }
                return this.footer;
            }
        }

        protected internal override ActivityDesignerGlyphCollection Glyphs
        {
            get
            {
                ActivityDesignerGlyphCollection glyphs = new ActivityDesignerGlyphCollection(base.Glyphs);
                if (this.InvokingDesigner != null)
                {
                    glyphs.Add(LockedActivityGlyph.Default);
                }
                return glyphs;
            }
        }

        protected virtual SequentialWorkflowHeaderFooter Header
        {
            get
            {
                if (this.header == null)
                {
                    this.header = new WorkflowHeader(this);
                }
                return this.header;
            }
        }

        public override System.Drawing.Image Image
        {
            get
            {
                return this.Header.Image;
            }
        }

        protected override Rectangle ImageRectangle
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return Rectangle.Empty;
            }
        }

        protected override CompositeActivityDesigner InvokingDesigner
        {
            get
            {
                return base.InvokingDesigner;
            }
            set
            {
                base.InvokingDesigner = value;
            }
        }

        public override Size MinimumSize
        {
            get
            {
                Size minimumSize = base.MinimumSize;
                minimumSize.Width = Math.Max(minimumSize.Width, MinSize.Width);
                minimumSize.Height = Math.Max(minimumSize.Width, MinSize.Height);
                if (base.IsRootDesigner && (this.InvokingDesigner == null))
                {
                    minimumSize.Width = Math.Max(minimumSize.Width, base.ParentView.ViewPortSize.Width - (2 * DefaultWorkflowLayout.Separator.Width));
                    minimumSize.Height = Math.Max(minimumSize.Height, base.ParentView.ViewPortSize.Height - (2 * DefaultWorkflowLayout.Separator.Height));
                }
                return minimumSize;
            }
        }

        private int OptimalHeight
        {
            get
            {
                CompositeDesignerTheme designerTheme = base.DesignerTheme as CompositeDesignerTheme;
                if (designerTheme == null)
                {
                    return 0;
                }
                int num = 0;
                if (this.ContainedDesigners.Count == 0)
                {
                    num += designerTheme.ConnectorSize.Height;
                    num += base.HelpTextSize.Height;
                    return (num + designerTheme.ConnectorSize.Height);
                }
                ActivityDesigner activeDesigner = base.ActiveDesigner;
                if (activeDesigner == this)
                {
                    num += designerTheme.ConnectorSize.Height;
                }
                AmbientTheme ambientTheme = WorkflowTheme.CurrentTheme.AmbientTheme;
                foreach (ActivityDesigner designer2 in this.ContainedDesigners)
                {
                    Size size = designer2.Size;
                    num += size.Height;
                    if (activeDesigner == this)
                    {
                        num += designerTheme.ConnectorSize.Height;
                    }
                    else
                    {
                        num += 2 * ambientTheme.SelectionSize.Height;
                    }
                }
                return num;
            }
        }

        protected override bool ShowSmartTag
        {
            get
            {
                return ((((this.Header != null) && !string.IsNullOrEmpty(this.Header.Text)) && (this.Views.Count > 1)) || base.ShowSmartTag);
            }
        }

        protected override Rectangle SmartTagRectangle
        {
            get
            {
                Rectangle empty = Rectangle.Empty;
                if (this.Header != null)
                {
                    empty = this.Header.ImageRectangle;
                }
                return empty;
            }
        }

        internal override WorkflowLayout SupportedLayout
        {
            get
            {
                return new WorkflowRootLayout(base.Activity.Site);
            }
        }

        public override string Text
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return string.Empty;
            }
        }

        protected override int TitleHeight
        {
            get
            {
                int titleHeight = base.TitleHeight;
                if (this.Header != null)
                {
                    titleHeight += this.Header.Bounds.Height;
                }
                return titleHeight;
            }
        }

        private sealed class WorkflowFooter : SequentialWorkflowHeaderFooter
        {
            public WorkflowFooter(SequentialWorkflowRootDesigner parent) : base(parent, false)
            {
                this.Image = SequentialWorkflowRootDesigner.FooterImage;
            }

            public override void OnPaint(ActivityDesignerPaintEventArgs e)
            {
                if (e == null)
                {
                    throw new ArgumentNullException("e");
                }
                Rectangle footerBarRectangle = this.FooterBarRectangle;
                if (!this.FooterBarRectangle.IsEmpty)
                {
                    Color empty = Color.Empty;
                    Color color2 = Color.FromArgb(50, e.DesignerTheme.BorderColor);
                    using (Brush brush = new LinearGradientBrush(footerBarRectangle, color2, empty, LinearGradientMode.Vertical))
                    {
                        e.Graphics.FillRectangle(brush, footerBarRectangle);
                        e.Graphics.DrawLine(e.DesignerTheme.BorderPen, footerBarRectangle.Left, footerBarRectangle.Top, footerBarRectangle.Right, footerBarRectangle.Top);
                    }
                }
                base.OnPaint(e);
            }

            public override Rectangle Bounds
            {
                get
                {
                    Rectangle bounds = base.Bounds;
                    SequentialWorkflowRootDesigner associatedDesigner = base.AssociatedDesigner;
                    bounds.Height = Math.Max(bounds.Height, (associatedDesigner.Size.Height - associatedDesigner.TitleHeight) - associatedDesigner.OptimalHeight);
                    bounds.Y = (associatedDesigner.Location.Y + associatedDesigner.TitleHeight) + associatedDesigner.OptimalHeight;
                    int height = this.ImageRectangle.Height;
                    height += (height > 0) ? (2 * WorkflowTheme.CurrentTheme.AmbientTheme.Margin.Height) : 0;
                    height += this.MinFooterBarHeight;
                    bounds.Height = Math.Max(height, bounds.Height);
                    return bounds;
                }
            }

            internal Rectangle FooterBarRectangle
            {
                get
                {
                    return Rectangle.Empty;
                }
            }

            public override Rectangle ImageRectangle
            {
                get
                {
                    Rectangle imageRectangle = base.ImageRectangle;
                    if (this.Image != null)
                    {
                        SequentialWorkflowRootDesigner associatedDesigner = base.AssociatedDesigner;
                        imageRectangle.X -= 4;
                        imageRectangle.Width += 8;
                        imageRectangle.Height += 8;
                        imageRectangle.Y = (associatedDesigner.Location.Y + associatedDesigner.TitleHeight) + associatedDesigner.OptimalHeight;
                        imageRectangle.Y += WorkflowTheme.CurrentTheme.AmbientTheme.Margin.Height;
                    }
                    return imageRectangle;
                }
            }

            private int MinFooterBarHeight
            {
                get
                {
                    return 0;
                }
            }
        }

        private sealed class WorkflowHeader : SequentialWorkflowHeaderFooter
        {
            public WorkflowHeader(SequentialWorkflowRootDesigner parent) : base(parent, true)
            {
                this.Image = SequentialWorkflowRootDesigner.HeaderImage;
            }

            public override void OnPaint(ActivityDesignerPaintEventArgs e)
            {
                if (e == null)
                {
                    throw new ArgumentNullException("e");
                }
                Rectangle headerBarRectangle = this.HeaderBarRectangle;
                Color empty = Color.Empty;
                Color color2 = Color.FromArgb(50, e.DesignerTheme.BorderColor);
                using (Brush brush = new LinearGradientBrush(headerBarRectangle, empty, color2, LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(brush, headerBarRectangle);
                    e.Graphics.DrawLine(e.DesignerTheme.BorderPen, headerBarRectangle.Left, headerBarRectangle.Bottom, headerBarRectangle.Right, headerBarRectangle.Bottom);
                }
                base.OnPaint(e);
            }

            public override Rectangle Bounds
            {
                get
                {
                    Rectangle bounds = base.Bounds;
                    Rectangle textRectangle = base.TextRectangle;
                    if (this.MinHeaderBarHeight > textRectangle.Height)
                    {
                        bounds.Height += this.MinHeaderBarHeight - textRectangle.Height;
                    }
                    return bounds;
                }
            }

            private Rectangle HeaderBarRectangle
            {
                get
                {
                    return new Rectangle { Location = base.AssociatedDesigner.Location, Width = base.AssociatedDesigner.Size.Width, Height = Math.Max((2 * WorkflowTheme.CurrentTheme.AmbientTheme.Margin.Height) + this.textSize.Height, this.MinHeaderBarHeight) };
                }
            }

            public override Rectangle ImageRectangle
            {
                get
                {
                    Rectangle imageRectangle = base.ImageRectangle;
                    if (this.Image != null)
                    {
                        ActivityDesignerTheme designerTheme = base.AssociatedDesigner.DesignerTheme;
                        imageRectangle.X -= 4;
                        imageRectangle.Y = this.HeaderBarRectangle.Bottom + WorkflowTheme.CurrentTheme.AmbientTheme.Margin.Height;
                        imageRectangle.Width += 8;
                        imageRectangle.Height += 8;
                    }
                    return imageRectangle;
                }
            }

            private int MinHeaderBarHeight
            {
                get
                {
                    return (2 * WorkflowTheme.CurrentTheme.AmbientTheme.Margin.Height);
                }
            }

            public override Rectangle TextRectangle
            {
                get
                {
                    Rectangle textRectangle = base.TextRectangle;
                    if (this.MinHeaderBarHeight > textRectangle.Height)
                    {
                        textRectangle.Y += (this.MinHeaderBarHeight - textRectangle.Height) / 2;
                    }
                    return textRectangle;
                }
            }
        }
    }
}

