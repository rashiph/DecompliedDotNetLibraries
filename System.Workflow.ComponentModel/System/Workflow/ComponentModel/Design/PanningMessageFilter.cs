namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Windows.Forms;

    internal sealed class PanningMessageFilter : WorkflowDesignerMessageFilter
    {
        private static Cursor PanBeganCursor = new Cursor(typeof(WorkflowView), "Resources.panClosed.cur");
        private bool panningActive;
        private Point panPoint = Point.Empty;
        private static Cursor PanReadyCursor = new Cursor(typeof(WorkflowView), "Resources.panOpened.cur");
        private CommandID previousCommand;
        private Cursor previousCursor = Cursors.Default;

        internal PanningMessageFilter()
        {
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
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
            return true;
        }

        protected override bool OnMouseDown(MouseEventArgs eventArgs)
        {
            if (eventArgs.Button == MouseButtons.Left)
            {
                this.SetPanPoint(new Point(eventArgs.X, eventArgs.Y));
            }
            return true;
        }

        protected override bool OnMouseEnter(MouseEventArgs eventArgs)
        {
            this.RefreshUIState();
            return true;
        }

        protected override bool OnMouseMove(MouseEventArgs eventArgs)
        {
            if (this.panningActive && ((eventArgs.Button & MouseButtons.Left) > MouseButtons.None))
            {
                Size size = new Size(eventArgs.X - this.panPoint.X, eventArgs.Y - this.panPoint.Y);
                WorkflowView parentView = base.ParentView;
                parentView.ScrollPosition = new Point(parentView.ScrollPosition.X - size.Width, parentView.ScrollPosition.Y - size.Height);
                this.SetPanPoint(new Point(eventArgs.X, eventArgs.Y));
            }
            return true;
        }

        protected override bool OnMouseUp(MouseEventArgs eventArgs)
        {
            this.SetPanPoint(Point.Empty);
            return true;
        }

        protected override bool OnShowContextMenu(Point menuPoint)
        {
            IMenuCommandService service = (IMenuCommandService) base.GetService(typeof(IMenuCommandService));
            if (service != null)
            {
                service.ShowContextMenu(WorkflowMenuCommands.ZoomMenu, menuPoint.X, menuPoint.Y);
            }
            return true;
        }

        private void RefreshUIState()
        {
            base.ParentView.Cursor = this.panningActive ? PanBeganCursor : PanReadyCursor;
            IMenuCommandService service = base.GetService(typeof(IMenuCommandService)) as IMenuCommandService;
            if (service != null)
            {
                CommandID[] didArray = new CommandID[] { WorkflowMenuCommands.ZoomIn, WorkflowMenuCommands.ZoomOut, WorkflowMenuCommands.Pan, WorkflowMenuCommands.DefaultFilter };
                foreach (CommandID did in didArray)
                {
                    MenuCommand command = service.FindCommand(did);
                    if ((command != null) && command.Enabled)
                    {
                        command.Checked = command.CommandID == WorkflowMenuCommands.Pan;
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

        private void SetPanPoint(Point value)
        {
            this.panPoint = value;
            this.panningActive = this.panPoint != Point.Empty;
            base.ParentView.Capture = this.panningActive;
            this.RefreshUIState();
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
    }
}

