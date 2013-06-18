namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Windows.Forms;

    internal sealed class ZoomingMessageFilter : WorkflowDesignerMessageFilter
    {
        private ZoomState currentState;
        private DragRectangleMessageFilter fastZoomingMessageFilter;
        private ZoomState initialState;
        private CommandID previousCommand;
        private Cursor previousCursor = Cursors.Default;
        private static Cursor ZoomDisabledCursor = new Cursor(typeof(WorkflowView), "Resources.zoomno.cur");
        private static int ZoomIncrement = 20;
        private static Cursor ZoomInCursor = new Cursor(typeof(WorkflowView), "Resources.zoomin.cur");
        private static Cursor ZoomOutCursor = new Cursor(typeof(WorkflowView), "Resources.zoomout.cur");

        internal ZoomingMessageFilter(bool initiateZoomIn)
        {
            this.currentState = this.initialState = initiateZoomIn ? ZoomState.In : ZoomState.Out;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (this.fastZoomingMessageFilter != null)
                {
                    this.fastZoomingMessageFilter.DragComplete -= new EventHandler(this.OnZoomRectComplete);
                    base.ParentView.RemoveDesignerMessageFilter(this.fastZoomingMessageFilter);
                    this.fastZoomingMessageFilter.Dispose();
                    this.fastZoomingMessageFilter = null;
                }
                this.RestoreUIState();
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        protected override void Initialize(WorkflowView parentView)
        {
            base.Initialize(parentView);
            this.StoreUIState();
            this.RefreshUIState();
        }

        protected override bool OnDragEnter(DragEventArgs eventArgs)
        {
            base.ParentView.RemoveDesignerMessageFilter(this);
            return false;
        }

        protected override bool OnKeyDown(KeyEventArgs eventArgs)
        {
            if (eventArgs.KeyValue == 0x1b)
            {
                base.ParentView.RemoveDesignerMessageFilter(this);
            }
            else
            {
                this.currentState = ((eventArgs.Modifiers & Keys.Shift) != Keys.None) ? ((this.initialState == ZoomState.In) ? ZoomState.Out : ZoomState.In) : this.initialState;
                this.RefreshUIState();
            }
            return true;
        }

        protected override bool OnKeyUp(KeyEventArgs eventArgs)
        {
            this.currentState = ((eventArgs.Modifiers & Keys.Shift) != Keys.None) ? ((this.initialState == ZoomState.In) ? ZoomState.Out : ZoomState.In) : this.initialState;
            this.RefreshUIState();
            return true;
        }

        protected override bool OnMouseCaptureChanged()
        {
            return true;
        }

        protected override bool OnMouseDoubleClick(MouseEventArgs eventArgs)
        {
            return true;
        }

        protected override bool OnMouseDown(MouseEventArgs eventArgs)
        {
            if (eventArgs.Button == MouseButtons.Left)
            {
                this.currentState = ((Control.ModifierKeys & Keys.Shift) != Keys.None) ? ((this.initialState == ZoomState.In) ? ZoomState.Out : ZoomState.In) : this.initialState;
                bool flag = this.fastZoomingMessageFilter == null;
                this.RefreshUIState();
                if (flag && (this.fastZoomingMessageFilter != null))
                {
                    ((IWorkflowDesignerMessageSink) this.fastZoomingMessageFilter).OnMouseDown(eventArgs);
                }
            }
            return true;
        }

        protected override bool OnMouseEnter(MouseEventArgs eventArgs)
        {
            this.RefreshUIState();
            return true;
        }

        protected override bool OnMouseLeave()
        {
            return true;
        }

        protected override bool OnMouseMove(MouseEventArgs eventArgs)
        {
            return true;
        }

        protected override bool OnMouseUp(MouseEventArgs eventArgs)
        {
            if ((eventArgs.Button == MouseButtons.Left) && this.CanContinueZooming)
            {
                WorkflowView parentView = base.ParentView;
                int zoomLevel = parentView.Zoom + ((this.currentState == ZoomState.In) ? ZoomIncrement : (-1 * ZoomIncrement));
                Point center = new Point((this.currentState == ZoomState.In) ? eventArgs.X : (parentView.ViewPortSize.Width / 2), (this.currentState == ZoomState.In) ? eventArgs.Y : (parentView.ViewPortSize.Height / 2));
                this.UpdateZoom(zoomLevel, center);
            }
            return true;
        }

        protected override bool OnShowContextMenu(Point menuPoint)
        {
            IMenuCommandService service = (IMenuCommandService) base.GetService(typeof(IMenuCommandService));
            if (service != null)
            {
                service.ShowContextMenu(WorkflowMenuCommands.ZoomMenu, menuPoint.X, menuPoint.Y);
                this.RefreshUIState();
            }
            return true;
        }

        private void OnZoomRectComplete(object sender, EventArgs e)
        {
            if ((this.CanContinueZooming && (this.currentState == ZoomState.In)) && ((this.fastZoomingMessageFilter != null) && !this.fastZoomingMessageFilter.DragRectangle.IsEmpty))
            {
                Rectangle dragRectangle = this.fastZoomingMessageFilter.DragRectangle;
                WorkflowView parentView = base.ParentView;
                Point center = parentView.LogicalPointToClient(new Point(dragRectangle.Location.X + (dragRectangle.Width / 2), dragRectangle.Location.Y + (dragRectangle.Height / 2)));
                int zoomLevel = (int) (Math.Min((float) (((float) parentView.ViewPortSize.Width) / ((float) dragRectangle.Width)), (float) (((float) parentView.ViewPortSize.Height) / ((float) dragRectangle.Height))) * 100f);
                this.UpdateZoom(zoomLevel, center);
            }
        }

        private void RefreshUIState()
        {
            WorkflowView parentView = base.ParentView;
            if (!this.CanContinueZooming)
            {
                parentView.Cursor = ZoomDisabledCursor;
            }
            else if (this.currentState == ZoomState.In)
            {
                parentView.Cursor = ZoomInCursor;
            }
            else
            {
                parentView.Cursor = ZoomOutCursor;
            }
            if (((this.fastZoomingMessageFilter == null) && this.CanContinueZooming) && (this.currentState == ZoomState.In))
            {
                this.fastZoomingMessageFilter = new DragRectangleMessageFilter();
                this.fastZoomingMessageFilter.DragComplete += new EventHandler(this.OnZoomRectComplete);
                parentView.AddDesignerMessageFilter(this.fastZoomingMessageFilter);
            }
            else if ((this.fastZoomingMessageFilter != null) && (!this.CanContinueZooming || (this.currentState != ZoomState.In)))
            {
                this.fastZoomingMessageFilter.DragComplete -= new EventHandler(this.OnZoomRectComplete);
                parentView.RemoveDesignerMessageFilter(this.fastZoomingMessageFilter);
                this.fastZoomingMessageFilter = null;
            }
            IMenuCommandService service = base.GetService(typeof(IMenuCommandService)) as IMenuCommandService;
            if (service != null)
            {
                foreach (CommandID did in CommandSet.NavigationToolCommandIds)
                {
                    MenuCommand command = service.FindCommand(did);
                    if ((command != null) && command.Enabled)
                    {
                        command.Checked = command.CommandID == ((this.initialState == ZoomState.In) ? WorkflowMenuCommands.ZoomIn : WorkflowMenuCommands.ZoomOut);
                    }
                }
            }
        }

        private void RestoreUIState()
        {
            IMenuCommandService service = base.GetService(typeof(IMenuCommandService)) as IMenuCommandService;
            if (service != null)
            {
                foreach (CommandID did in CommandSet.NavigationToolCommandIds)
                {
                    MenuCommand command = service.FindCommand(did);
                    if ((command != null) && command.Enabled)
                    {
                        command.Checked = command.CommandID == this.previousCommand;
                    }
                }
            }
            base.ParentView.Cursor = this.previousCursor;
        }

        private void StoreUIState()
        {
            IMenuCommandService service = base.GetService(typeof(IMenuCommandService)) as IMenuCommandService;
            if (service != null)
            {
                foreach (CommandID did in CommandSet.NavigationToolCommandIds)
                {
                    MenuCommand command = service.FindCommand(did);
                    if (((command != null) && command.Enabled) && command.Checked)
                    {
                        this.previousCommand = command.CommandID;
                        break;
                    }
                }
            }
            this.previousCursor = base.ParentView.Cursor;
        }

        private void UpdateZoom(int zoomLevel, Point center)
        {
            PointF empty = PointF.Empty;
            WorkflowView parentView = base.ParentView;
            Point point = parentView.LogicalPointToClient(Point.Empty);
            center.X -= point.X;
            center.Y -= point.Y;
            empty = new PointF(((float) center.X) / ((float) parentView.HScrollBar.Maximum), ((float) center.Y) / ((float) parentView.VScrollBar.Maximum));
            parentView.Zoom = Math.Min(Math.Max(zoomLevel, 10), 400);
            Point point2 = new Point((int) (parentView.HScrollBar.Maximum * empty.X), (int) (parentView.VScrollBar.Maximum * empty.Y));
            parentView.ScrollPosition = new Point(point2.X - (parentView.HScrollBar.LargeChange / 2), point2.Y - (parentView.VScrollBar.LargeChange / 2));
            this.currentState = ((Control.ModifierKeys & Keys.Shift) != Keys.None) ? ((this.initialState == ZoomState.In) ? ZoomState.Out : ZoomState.In) : this.initialState;
            this.RefreshUIState();
        }

        private bool CanContinueZooming
        {
            get
            {
                WorkflowView parentView = base.ParentView;
                return (((this.currentState == ZoomState.Out) && (parentView.Zoom > 10)) || ((this.currentState == ZoomState.In) && (parentView.Zoom < 400)));
            }
        }

        internal bool ZoomingIn
        {
            get
            {
                return (this.initialState == ZoomState.In);
            }
        }

        private enum ZoomState
        {
            In,
            Out
        }
    }
}

