namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal sealed class AutoScrollingMessageFilter : WorkflowDesignerMessageFilter
    {
        private ScrollDirection autoScrollDirection = ScrollDirection.None;
        private EventHandler autoScrollEventHandler;
        private bool startAutoScroll;

        internal AutoScrollingMessageFilter()
        {
        }

        private ScrollDirection AutoScrollDirectionFromPoint(Point clientPoint)
        {
            Rectangle rectangle = new Rectangle(Point.Empty, base.ParentView.ViewPortSize);
            if (!rectangle.Contains(clientPoint))
            {
                return ScrollDirection.None;
            }
            ScrollDirection none = ScrollDirection.None;
            ScrollBar hScrollBar = base.ParentView.HScrollBar;
            if ((clientPoint.X <= (rectangle.Width / 10)) && (hScrollBar.Value > 0))
            {
                none |= ScrollDirection.Left;
            }
            else if ((clientPoint.X >= (rectangle.Right - (rectangle.Width / 10))) && (hScrollBar.Value < (hScrollBar.Maximum - hScrollBar.LargeChange)))
            {
                none |= ScrollDirection.Right;
            }
            ScrollBar vScrollBar = base.ParentView.VScrollBar;
            if ((clientPoint.Y <= (rectangle.Height / 10)) && (vScrollBar.Value > 0))
            {
                return (none | ScrollDirection.Up);
            }
            if ((clientPoint.Y >= (rectangle.Bottom - (rectangle.Height / 10))) && (vScrollBar.Value < (vScrollBar.Maximum - vScrollBar.LargeChange)))
            {
                none |= ScrollDirection.Down;
            }
            return none;
        }

        private void DrawScrollIndicators(Graphics graphics)
        {
            Image scrollIndicatorImage = AmbientTheme.ScrollIndicatorImage;
            if (scrollIndicatorImage != null)
            {
                WorkflowView parentView = base.ParentView;
                Size viewPortSize = parentView.ViewPortSize;
                Point scrollPosition = parentView.ScrollPosition;
                Rectangle[] scrollIndicatorRectangles = this.ScrollIndicatorRectangles;
                if (scrollPosition.X > 0)
                {
                    ActivityDesignerPaint.DrawImage(graphics, AmbientTheme.ScrollIndicatorImage, scrollIndicatorRectangles[0], (float) 0.7f);
                }
                if (scrollPosition.X < (parentView.HScrollBar.Maximum - viewPortSize.Width))
                {
                    scrollIndicatorImage.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    ActivityDesignerPaint.DrawImage(graphics, scrollIndicatorImage, scrollIndicatorRectangles[1], (float) 0.7f);
                    scrollIndicatorImage.RotateFlip(RotateFlipType.RotateNoneFlipX);
                }
                if (scrollPosition.Y > 0)
                {
                    scrollIndicatorImage.RotateFlip(RotateFlipType.Rotate90FlipX);
                    ActivityDesignerPaint.DrawImage(graphics, scrollIndicatorImage, scrollIndicatorRectangles[2], (float) 0.7f);
                    scrollIndicatorImage.RotateFlip(RotateFlipType.Rotate90FlipX);
                }
                if (scrollPosition.Y < (parentView.VScrollBar.Maximum - viewPortSize.Height))
                {
                    scrollIndicatorImage.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    ActivityDesignerPaint.DrawImage(graphics, scrollIndicatorImage, scrollIndicatorRectangles[3], (float) 0.7f);
                    scrollIndicatorImage.RotateFlip(RotateFlipType.Rotate90FlipNone);
                }
            }
        }

        private void OnAutoScroll(object sender, EventArgs eventArgs)
        {
            WorkflowView parentView = base.ParentView;
            Point scrollPosition = parentView.ScrollPosition;
            if ((this.autoScrollDirection & ScrollDirection.Left) > ((ScrollDirection) 0))
            {
                scrollPosition.X -= 0x19;
            }
            else if ((this.autoScrollDirection & ScrollDirection.Right) > ((ScrollDirection) 0))
            {
                scrollPosition.X += 0x19;
            }
            if ((this.autoScrollDirection & ScrollDirection.Up) > ((ScrollDirection) 0))
            {
                scrollPosition.Y -= 0x19;
            }
            else if ((this.autoScrollDirection & ScrollDirection.Down) > ((ScrollDirection) 0))
            {
                scrollPosition.Y += 0x19;
            }
            parentView.ScrollPosition = scrollPosition;
        }

        protected override bool OnDragDrop(DragEventArgs eventArgs)
        {
            this.startAutoScroll = false;
            this.AutoScrollDirection = ScrollDirection.None;
            return false;
        }

        protected override bool OnDragEnter(DragEventArgs eventArgs)
        {
            this.startAutoScroll = true;
            foreach (Rectangle rectangle in this.ScrollIndicatorRectangles)
            {
                base.ParentView.InvalidateClientRectangle(rectangle);
            }
            return false;
        }

        protected override bool OnDragLeave()
        {
            this.startAutoScroll = false;
            this.AutoScrollDirection = ScrollDirection.None;
            return false;
        }

        protected override bool OnDragOver(DragEventArgs eventArgs)
        {
            this.startAutoScroll = true;
            Point clientPoint = base.ParentView.PointToClient(new Point(eventArgs.X, eventArgs.Y));
            this.AutoScrollDirection = this.AutoScrollDirectionFromPoint(clientPoint);
            return (this.AutoScrollDirection != ScrollDirection.None);
        }

        protected override bool OnPaintWorkflowAdornments(PaintEventArgs e, Rectangle viewPort, AmbientTheme ambientTheme)
        {
            if (this.ShowAutoScrollIndicators)
            {
                this.DrawScrollIndicators(e.Graphics);
            }
            return false;
        }

        private ScrollDirection AutoScrollDirection
        {
            get
            {
                return this.autoScrollDirection;
            }
            set
            {
                if (this.autoScrollDirection != value)
                {
                    this.autoScrollDirection = value;
                    foreach (Rectangle rectangle in this.ScrollIndicatorRectangles)
                    {
                        base.ParentView.InvalidateClientRectangle(rectangle);
                    }
                    if (ScrollDirection.None == value)
                    {
                        if (this.autoScrollEventHandler != null)
                        {
                            WorkflowTimer.Default.Unsubscribe(this.autoScrollEventHandler);
                            this.autoScrollEventHandler = null;
                        }
                    }
                    else if (this.autoScrollEventHandler == null)
                    {
                        this.autoScrollEventHandler = new EventHandler(this.OnAutoScroll);
                        WorkflowTimer.Default.Subscribe(50, this.autoScrollEventHandler);
                    }
                }
            }
        }

        private Rectangle[] ScrollIndicatorRectangles
        {
            get
            {
                Rectangle rectangle = new Rectangle(Point.Empty, base.ParentView.ViewPortSize);
                Size margin = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;
                Size scrollIndicatorSize = WorkflowTheme.CurrentTheme.AmbientTheme.ScrollIndicatorSize;
                Rectangle[] rectangleArray = new Rectangle[4];
                rectangleArray[0].X = margin.Width;
                rectangleArray[0].Y = (rectangle.Height - scrollIndicatorSize.Height) / 2;
                rectangleArray[0].Size = scrollIndicatorSize;
                rectangleArray[1].X = (rectangle.Right - margin.Width) - scrollIndicatorSize.Width;
                rectangleArray[1].Y = (rectangle.Height - scrollIndicatorSize.Height) / 2;
                rectangleArray[1].Size = scrollIndicatorSize;
                rectangleArray[2].X = (rectangle.Width - scrollIndicatorSize.Width) / 2;
                rectangleArray[2].Y = margin.Height;
                rectangleArray[2].Size = scrollIndicatorSize;
                rectangleArray[3].X = (rectangle.Width - scrollIndicatorSize.Width) / 2;
                rectangleArray[3].Y = (rectangle.Bottom - margin.Height) - scrollIndicatorSize.Height;
                rectangleArray[3].Size = scrollIndicatorSize;
                return rectangleArray;
            }
        }

        private bool ShowAutoScrollIndicators
        {
            get
            {
                AmbientTheme ambientTheme = WorkflowTheme.CurrentTheme.AmbientTheme;
                if (!this.startAutoScroll)
                {
                    return false;
                }
                Size viewPortSize = base.ParentView.ViewPortSize;
                Size scrollIndicatorSize = ambientTheme.ScrollIndicatorSize;
                scrollIndicatorSize.Width += 2 * ambientTheme.Margin.Width;
                scrollIndicatorSize.Height += 2 * ambientTheme.Margin.Height;
                return ((viewPortSize.Width > (2 * scrollIndicatorSize.Width)) && (viewPortSize.Height > (2 * scrollIndicatorSize.Height)));
            }
        }

        private enum ScrollDirection
        {
            Down = 0x10,
            Left = 2,
            None = 1,
            Right = 8,
            Up = 4
        }
    }
}

