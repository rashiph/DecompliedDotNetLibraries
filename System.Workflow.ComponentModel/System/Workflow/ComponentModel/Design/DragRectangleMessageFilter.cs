namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Runtime;
    using System.Threading;
    using System.Windows.Forms;

    internal class DragRectangleMessageFilter : WorkflowDesignerMessageFilter
    {
        private bool dragStarted;
        private Point endDrag = Point.Empty;
        private Cursor previousCursor = Cursors.Default;
        private Point startDrag = Point.Empty;

        internal event EventHandler DragComplete;

        internal DragRectangleMessageFilter()
        {
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                this.DragStarted = false;
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        protected override bool OnKeyDown(KeyEventArgs eventArgs)
        {
            if (eventArgs.KeyValue == 0x1b)
            {
                this.DragStarted = false;
            }
            return false;
        }

        protected override bool OnMouseCaptureChanged()
        {
            this.DragStarted = false;
            return false;
        }

        protected override bool OnMouseDown(MouseEventArgs eventArgs)
        {
            if (eventArgs.Button == MouseButtons.Left)
            {
                this.startDrag = this.endDrag = base.ParentView.ClientPointToLogical(new Point(eventArgs.X, eventArgs.Y));
            }
            return false;
        }

        protected override bool OnMouseMove(MouseEventArgs eventArgs)
        {
            WorkflowView parentView = base.ParentView;
            Point point = parentView.ClientPointToLogical(new Point(eventArgs.X, eventArgs.Y));
            float num = (2f / ((float) parentView.Zoom)) * 100f;
            if ((!this.dragStarted && ((eventArgs.Button & MouseButtons.Left) > MouseButtons.None)) && ((Math.Abs((int) (this.startDrag.X - point.X)) > ((int) (num * SystemInformation.DragSize.Width))) || (Math.Abs((int) (this.startDrag.Y - point.Y)) > ((int) (num * SystemInformation.DragSize.Height)))))
            {
                this.DragStarted = true;
            }
            if (this.dragStarted)
            {
                if (!this.DragRectangle.IsEmpty)
                {
                    parentView.InvalidateLogicalRectangle(this.DragRectangle);
                }
                this.endDrag = point;
                if (!this.DragRectangle.IsEmpty)
                {
                    parentView.InvalidateLogicalRectangle(this.DragRectangle);
                }
            }
            return this.dragStarted;
        }

        protected override bool OnMouseUp(MouseEventArgs eventArgs)
        {
            if (!this.dragStarted)
            {
                return false;
            }
            WorkflowView parentView = base.ParentView;
            if (!this.DragRectangle.IsEmpty)
            {
                parentView.InvalidateLogicalRectangle(this.DragRectangle);
            }
            this.endDrag = parentView.ClientPointToLogical(new Point(eventArgs.X, eventArgs.Y));
            this.DragStarted = false;
            if (this.DragComplete != null)
            {
                this.DragComplete(this, EventArgs.Empty);
            }
            return true;
        }

        protected override bool OnPaint(PaintEventArgs e, Rectangle viewPort, AmbientTheme ambientTheme)
        {
            if (this.dragStarted)
            {
                using (Brush brush = new SolidBrush(Color.FromArgb(10, ambientTheme.SelectionForeColor)))
                {
                    Rectangle dragRectangle = this.DragRectangle;
                    e.Graphics.FillRectangle(brush, dragRectangle.X, dragRectangle.Y, dragRectangle.Width - 1, dragRectangle.Height - 1);
                    e.Graphics.DrawRectangle(ambientTheme.SelectionForegroundPen, dragRectangle.X, dragRectangle.Y, dragRectangle.Width - 1, dragRectangle.Height - 1);
                }
            }
            return false;
        }

        internal Rectangle DragRectangle
        {
            get
            {
                int x = Math.Min(this.startDrag.X, this.endDrag.X);
                int y = Math.Min(this.startDrag.Y, this.endDrag.Y);
                int width = Math.Abs((int) (this.endDrag.X - this.startDrag.X));
                return new Rectangle(x, y, width, Math.Abs((int) (this.endDrag.Y - this.startDrag.Y)));
            }
        }

        protected bool DragStarted
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.dragStarted;
            }
            set
            {
                if (this.dragStarted != value)
                {
                    WorkflowView parentView = base.ParentView;
                    if (value)
                    {
                        this.dragStarted = true;
                        this.previousCursor = parentView.Cursor;
                        parentView.Cursor = Cursors.Cross;
                        parentView.Capture = true;
                    }
                    else
                    {
                        parentView.Capture = false;
                        this.dragStarted = false;
                        if (this.previousCursor != null)
                        {
                            parentView.Cursor = this.previousCursor;
                        }
                        if (!this.DragRectangle.IsEmpty)
                        {
                            parentView.InvalidateLogicalRectangle(this.DragRectangle);
                        }
                    }
                }
            }
        }
    }
}

