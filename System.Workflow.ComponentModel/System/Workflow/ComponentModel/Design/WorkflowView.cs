namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.Drawing.Printing;
    using System.IO;
    using System.Runtime;
    using System.Security.Permissions;
    using System.Threading;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using System.Workflow.ComponentModel;

    [ToolboxItem(false), ActivityDesignerTheme(typeof(AmbientTheme), Xml="<AmbientTheme xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/workflow\" ApplyTo=\"System.Workflow.ComponentModel.Design.WorkflowView\" ShowConfigErrors=\"True\" DrawShadow=\"False\" DrawGrayscale=\"False\" DropIndicatorColor=\"0xFF006400\" SelectionForeColor=\"0xFF0000FF\" SelectionPatternColor=\"0xFF606060\" ForeColor=\"0xFF808080\" BackColor=\"0xFFFFFFFF\" ShowGrid=\"False\" GridColor=\"0xFFC0C0C0\" TextQuality=\"Aliased\" DrawRounded=\"True\" ShowDesignerBorder=\"True\" />")]
    public class WorkflowView : UserControl, IServiceProvider, IMessageFilter
    {
        private WorkflowLayout activeLayout;
        private System.Workflow.ComponentModel.Design.CommandSet commandSet;
        private List<WorkflowDesignerMessageFilter> customMessageFilters;
        private WorkflowLayout defaultLayout;
        private bool dragDropInProgress;
        private EventHandler ensureVisibleEventHandler;
        private DynamicAction fitAllAction;
        private System.Windows.Forms.HScrollBar hScrollBar;
        private EventHandler idleEventHandler;
        private EventHandler idleEventListeners;
        private EventHandler layoutEventHandler;
        private Stack<System.Workflow.ComponentModel.Design.HitTestInfo> messageHitTestContexts;
        private Point prePreviewScroll;
        private int prePreviewZoom;
        private WorkflowPrintDocument printDocument;
        private ActivityDesigner rootDesigner;
        private IServiceProvider serviceProvider;
        private int shadowDepth;
        private List<WorkflowDesignerMessageFilter> stockMessageFilters;
        internal const string ThemeXml = "<AmbientTheme xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/workflow\" ApplyTo=\"System.Workflow.ComponentModel.Design.WorkflowView\" ShowConfigErrors=\"True\" DrawShadow=\"False\" DrawGrayscale=\"False\" DropIndicatorColor=\"0xFF006400\" SelectionForeColor=\"0xFF0000FF\" SelectionPatternColor=\"0xFF606060\" ForeColor=\"0xFF808080\" BackColor=\"0xFFFFFFFF\" ShowGrid=\"False\" GridColor=\"0xFFC0C0C0\" TextQuality=\"Aliased\" DrawRounded=\"True\" ShowDesignerBorder=\"True\" />";
        private System.Workflow.ComponentModel.Design.TabControl toolContainer;
        private Bitmap viewPortBitmap;
        private System.Windows.Forms.VScrollBar vScrollBar;
        private WorkflowToolTip workflowToolTip;
        private float zoomLevel;

        public event EventHandler Idle
        {
            add
            {
                this.idleEventListeners = (EventHandler) Delegate.Combine(this.idleEventListeners, value);
                if (this.idleEventHandler == null)
                {
                    this.idleEventHandler = new EventHandler(this.OnWorkflowIdle);
                    Form topLevelControl = base.TopLevelControl as Form;
                    if (!Application.MessageLoop || ((topLevelControl != null) && topLevelControl.Modal))
                    {
                        WorkflowTimer.Default.Subscribe(100, this.idleEventHandler);
                    }
                    else
                    {
                        Application.Idle += this.idleEventHandler;
                    }
                }
            }
            remove
            {
                this.idleEventListeners = (EventHandler) Delegate.Remove(this.idleEventListeners, value);
                if ((this.idleEventHandler != null) && (this.idleEventListeners == null))
                {
                    Form topLevelControl = base.TopLevelControl as Form;
                    if ((topLevelControl != null) && topLevelControl.Modal)
                    {
                        WorkflowTimer.Default.Unsubscribe(this.idleEventHandler);
                    }
                    else
                    {
                        Application.Idle -= this.idleEventHandler;
                    }
                    this.idleEventHandler = null;
                }
            }
        }

        public event EventHandler RootDesignerChanged;

        public event EventHandler ZoomChanged;

        public WorkflowView() : this(new DesignSurface())
        {
        }

        public WorkflowView(IServiceProvider serviceProvider)
        {
            this.zoomLevel = 1f;
            this.shadowDepth = WorkflowTheme.CurrentTheme.AmbientTheme.ShadowDepth;
            this.stockMessageFilters = new List<WorkflowDesignerMessageFilter>();
            this.customMessageFilters = new List<WorkflowDesignerMessageFilter>();
            this.prePreviewZoom = 100;
            this.prePreviewScroll = Point.Empty;
            this.messageHitTestContexts = new Stack<System.Workflow.ComponentModel.Design.HitTestInfo>();
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }
            base.SuspendLayout();
            this.AllowDrop = true;
            this.AutoScroll = false;
            base.HScroll = false;
            base.VScroll = false;
            base.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.EnableNotifyMessage | ControlStyles.AllPaintingInWmPaint | ControlStyles.Selectable | ControlStyles.Opaque | ControlStyles.UserPaint, true);
            this.serviceProvider = serviceProvider;
            IServiceContainer container = this.GetService(typeof(IServiceContainer)) as IServiceContainer;
            if (container != null)
            {
                container.RemoveService(typeof(WorkflowView));
                container.AddService(typeof(WorkflowView), this);
            }
            IUIService service = this.serviceProvider.GetService(typeof(IUIService)) as IUIService;
            if (service != null)
            {
                WorkflowTheme.UIService = service;
            }
            this.EnsureScrollBars(new System.Windows.Forms.HScrollBar(), new System.Windows.Forms.VScrollBar());
            this.workflowToolTip = new WorkflowToolTip(this);
            WorkflowTheme.ThemeChanged += new EventHandler(this.OnThemeChange);
            this.PopulateMessageFilters(true);
            this.RootDesigner = ActivityDesigner.GetSafeRootDesigner(this);
            this.fitAllAction = this.CreateDynamicAction();
            if ((this.activeLayout == null) || (this.defaultLayout == null))
            {
                this.ActiveLayout = this.DefaultLayout = new WorkflowRootLayout(this.serviceProvider);
            }
            if (this.GetService(typeof(IMenuCommandService)) is IMenuCommandService)
            {
                this.commandSet = new System.Workflow.ComponentModel.Design.CommandSet(this);
                this.commandSet.UpdatePanCommands(true);
            }
            ISelectionService service3 = this.GetService(typeof(ISelectionService)) as ISelectionService;
            if (service3 != null)
            {
                service3.SelectionChanged += new EventHandler(this.OnSelectionChanged);
            }
            Application.AddMessageFilter(this);
            base.ResumeLayout(true);
        }

        public void AddDesignerMessageFilter(WorkflowDesignerMessageFilter designerMessageFilter)
        {
            if (designerMessageFilter == null)
            {
                throw new ArgumentNullException("designerMessageFilter");
            }
            if (base.Capture)
            {
                base.Capture = false;
            }
            this.customMessageFilters.Insert(0, designerMessageFilter);
            designerMessageFilter.SetParentView(this);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public Point ClientPointToLogical(Point clientPoint)
        {
            return this.ClientPointToLogical(clientPoint, true);
        }

        private Point ClientPointToLogical(Point point, bool mapToLayout)
        {
            Point[] pts = new Point[] { point };
            Matrix matrix = new Matrix();
            matrix.Translate((float) this.ScrollPosition.X, (float) this.ScrollPosition.Y);
            matrix.TransformPoints(pts);
            Matrix matrix2 = new Matrix();
            matrix2.Scale(this.ScaleZoomFactor, this.ScaleZoomFactor);
            matrix2.Invert();
            matrix2.TransformPoints(pts);
            matrix2.Invert();
            if (!mapToLayout)
            {
                return pts[0];
            }
            return this.activeLayout.MapInCoOrdToLayout(pts[0]);
        }

        public Rectangle ClientRectangleToLogical(Rectangle rectangle)
        {
            Rectangle logicalRectangle = new Rectangle(this.ClientPointToLogical(rectangle.Location, false), this.ClientSizeToLogical(rectangle.Size));
            return this.activeLayout.MapInRectangleToLayout(logicalRectangle);
        }

        public Size ClientSizeToLogical(Size clientSize)
        {
            Point[] pts = new Point[] { new Point(clientSize) };
            Matrix matrix = new Matrix();
            matrix.Scale(this.ScaleZoomFactor, this.ScaleZoomFactor);
            matrix.Invert();
            matrix.TransformPoints(pts);
            matrix.Invert();
            return new Size(pts[0]);
        }

        protected override AccessibleObject CreateAccessibilityInstance()
        {
            return new WorkflowViewAccessibleObject(this);
        }

        private DynamicAction CreateDynamicAction()
        {
            DynamicAction action = new DynamicAction {
                ButtonSize = DynamicAction.ButtonSizes.Large,
                DockAlignment = DesignerContentAlignment.BottomRight,
                DockMargin = new Size(5, 5)
            };
            ActionButton item = new ActionButton(new Image[] { DR.GetImage("FitToScreen") as Bitmap });
            item.StateChanged += new EventHandler(this.OnFitToScreen);
            action.Buttons.Add(item);
            return action;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    base.SuspendLayout();
                    Application.RemoveMessageFilter(this);
                    if (this.layoutEventHandler != null)
                    {
                        this.Idle -= this.layoutEventHandler;
                        this.layoutEventHandler = null;
                    }
                    if (this.ensureVisibleEventHandler != null)
                    {
                        this.Idle -= this.ensureVisibleEventHandler;
                        this.ensureVisibleEventHandler = null;
                    }
                    if (this.idleEventHandler != null)
                    {
                        this.idleEventListeners = null;
                        Form topLevelControl = base.TopLevelControl as Form;
                        if (!Application.MessageLoop || ((topLevelControl != null) && topLevelControl.Modal))
                        {
                            WorkflowTimer.Default.Unsubscribe(this.idleEventHandler);
                        }
                        else
                        {
                            Application.Idle -= this.idleEventHandler;
                        }
                        this.idleEventHandler = null;
                    }
                    ISelectionService service = this.GetService(typeof(ISelectionService)) as ISelectionService;
                    if (service != null)
                    {
                        service.SelectionChanged -= new EventHandler(this.OnSelectionChanged);
                    }
                    WorkflowTheme.ThemeChanged -= new EventHandler(this.OnThemeChange);
                    if (this.fitAllAction != null)
                    {
                        this.fitAllAction.Dispose();
                        this.fitAllAction = null;
                    }
                    if (this.workflowToolTip != null)
                    {
                        ((IDisposable) this.workflowToolTip).Dispose();
                        this.workflowToolTip = null;
                    }
                    this.DisposeMessageFilters(false);
                    this.DisposeMessageFilters(true);
                    this.activeLayout = null;
                    if (this.defaultLayout != null)
                    {
                        this.defaultLayout.Dispose();
                        this.defaultLayout = null;
                    }
                    if (this.viewPortBitmap != null)
                    {
                        this.viewPortBitmap.Dispose();
                        this.viewPortBitmap = null;
                    }
                    if (this.commandSet != null)
                    {
                        this.commandSet.Dispose();
                        this.commandSet = null;
                    }
                    this.HScrollBar.ValueChanged -= new EventHandler(this.OnScroll);
                    this.VScrollBar.ValueChanged -= new EventHandler(this.OnScroll);
                    if (this.toolContainer != null)
                    {
                        base.Controls.Remove(this.toolContainer);
                        this.toolContainer.TabStrip.Tabs.Clear();
                        this.toolContainer.Dispose();
                        this.toolContainer = null;
                    }
                    IServiceContainer container = this.GetService(typeof(IServiceContainer)) as IServiceContainer;
                    if (container != null)
                    {
                        container.RemoveService(typeof(WorkflowView));
                    }
                }
                finally
                {
                    base.ResumeLayout(false);
                }
            }
            base.Dispose(disposing);
        }

        private void DisposeMessageFilters(bool stockFilters)
        {
            List<WorkflowDesignerMessageFilter> list = stockFilters ? this.stockMessageFilters : this.customMessageFilters;
            ArrayList list2 = new ArrayList(list.ToArray());
            foreach (WorkflowDesignerMessageFilter filter in list2)
            {
                filter.Dispose();
            }
            list.Clear();
        }

        private void EnsureScrollBars(System.Windows.Forms.HScrollBar newHorizScrollBar, System.Windows.Forms.VScrollBar newVertScrollBar)
        {
            try
            {
                base.SuspendLayout();
                if (this.hScrollBar != newHorizScrollBar)
                {
                    if (this.hScrollBar != null)
                    {
                        this.hScrollBar.ValueChanged -= new EventHandler(this.OnScroll);
                        if (base.Controls.Contains(this.hScrollBar))
                        {
                            base.Controls.Remove(this.hScrollBar);
                        }
                    }
                    this.hScrollBar = newHorizScrollBar;
                    if (this.hScrollBar.Parent == null)
                    {
                        this.hScrollBar.TabStop = false;
                        base.Controls.Add(this.hScrollBar);
                    }
                }
                if (this.vScrollBar != newVertScrollBar)
                {
                    if (this.vScrollBar != null)
                    {
                        this.vScrollBar.ValueChanged -= new EventHandler(this.OnScroll);
                        if (base.Controls.Contains(this.vScrollBar))
                        {
                            base.Controls.Remove(this.vScrollBar);
                        }
                    }
                    this.vScrollBar = newVertScrollBar;
                    if (this.vScrollBar.Parent == null)
                    {
                        this.vScrollBar.TabStop = false;
                        base.Controls.Add(this.vScrollBar);
                    }
                }
                this.hScrollBar.ValueChanged += new EventHandler(this.OnScroll);
                this.vScrollBar.ValueChanged += new EventHandler(this.OnScroll);
            }
            finally
            {
                base.ResumeLayout(true);
            }
        }

        private void EnsureVisible(Rectangle rect)
        {
            Rectangle rectangle = this.ClientRectangleToLogical(new Rectangle(Point.Empty, this.ViewPortSize));
            if (!rectangle.Contains(rect.Location) || !rectangle.Contains(new Point(rect.Right, rect.Bottom)))
            {
                Size logicalSize = new Size();
                if (!rectangle.Contains(new Point(rect.Left, rectangle.Top)) || !rectangle.Contains(new Point(rect.Right, rectangle.Top)))
                {
                    if (rect.Width > rectangle.Width)
                    {
                        logicalSize.Width = (rect.Left + (rect.Width / 2)) - (rectangle.Left + (rectangle.Width / 2));
                    }
                    else if (rect.Left < rectangle.Left)
                    {
                        logicalSize.Width = rect.Left - rectangle.Left;
                    }
                    else
                    {
                        logicalSize.Width = rect.Right - rectangle.Right;
                    }
                }
                if (!rectangle.Contains(new Point(rectangle.Left, rect.Top)) || !rectangle.Contains(new Point(rectangle.Left, rect.Bottom)))
                {
                    if ((rect.Top < rectangle.Top) || (rect.Height > rectangle.Height))
                    {
                        logicalSize.Height = rect.Top - rectangle.Top;
                    }
                    else
                    {
                        logicalSize.Height = rect.Bottom - rectangle.Bottom;
                    }
                }
                logicalSize = this.LogicalSizeToClient(logicalSize);
                Point scrollPosition = this.ScrollPosition;
                this.ScrollPosition = new Point(scrollPosition.X + logicalSize.Width, scrollPosition.Y + logicalSize.Height);
            }
        }

        public void EnsureVisible(object selectableObject)
        {
            if (selectableObject == null)
            {
                throw new ArgumentNullException("selectableObject");
            }
            Activity activity = selectableObject as Activity;
            while (activity != null)
            {
                ActivityDesigner containedDesigner = ActivityDesigner.GetDesigner(activity);
                CompositeActivityDesigner parentDesigner = containedDesigner.ParentDesigner;
                if (parentDesigner != null)
                {
                    if (containedDesigner != null)
                    {
                        parentDesigner.EnsureVisibleContainedDesigner(containedDesigner);
                    }
                    activity = parentDesigner.Activity;
                }
                else
                {
                    activity = null;
                }
            }
            activity = selectableObject as Activity;
            if (activity != null)
            {
                CompositeActivityDesigner designer = ActivityDesigner.GetDesigner(activity) as CompositeActivityDesigner;
                if (designer != null)
                {
                    designer.EnsureVisibleContainedDesigner(designer);
                }
            }
            this.PerformLayout(false);
            if (this.ensureVisibleEventHandler == null)
            {
                this.ensureVisibleEventHandler = new EventHandler(this.OnEnsureVisible);
                this.Idle += this.ensureVisibleEventHandler;
            }
        }

        public void FitToScreenSize()
        {
            if ((this.HScrollBar.Maximum > this.ViewPortSize.Width) || (this.VScrollBar.Maximum > this.ViewPortSize.Height))
            {
                int num = (int) ((100f / this.ActiveLayout.Scaling) * Math.Min((float) (((float) this.ViewPortSize.Width) / ((float) this.ActiveLayout.Extent.Width)), (float) (((float) this.ViewPortSize.Height) / ((float) this.ActiveLayout.Extent.Height))));
                this.Zoom = Math.Min(Math.Max(num, 10), 400);
            }
        }

        public void FitToWorkflowSize()
        {
            if (this.Zoom != 100)
            {
                this.Zoom = 100;
            }
        }

        protected override object GetService(System.Type serviceType)
        {
            if (serviceType == typeof(CommandID))
            {
                return new CommandID(new Guid("5f1c3c8d-60f1-4b98-b85b-8679f97e8eac"), 0);
            }
            return this.serviceProvider.GetService(serviceType);
        }

        public void InvalidateClientRectangle(Rectangle clientRectangle)
        {
            if (this.layoutEventHandler == null)
            {
                if (!clientRectangle.IsEmpty)
                {
                    clientRectangle.Inflate(1, 1);
                    base.Invalidate(clientRectangle);
                }
                else
                {
                    base.Invalidate();
                }
            }
        }

        public void InvalidateLogicalRectangle(Rectangle logicalRectangle)
        {
            this.InvalidateClientRectangle(this.LogicalRectangleToClient(logicalRectangle));
        }

        internal bool IsClientPointInActiveLayout(Point clientPoint)
        {
            Point logicalCoOrd = this.ClientPointToLogical(clientPoint, false);
            return this.activeLayout.IsCoOrdInLayout(logicalCoOrd);
        }

        public void LoadViewState(Stream viewState)
        {
            if (viewState == null)
            {
                throw new ArgumentNullException("viewState");
            }
            Point point = new Point(0, 0);
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            if (service == null)
            {
                throw new Exception(SR.GetString("General_MissingService", new object[] { typeof(IDesignerHost).FullName }));
            }
            viewState.Position = 0L;
            BinaryReader reader = new BinaryReader(viewState);
            this.PrintPreviewMode = reader.ReadBoolean();
            this.Zoom = reader.ReadInt32();
            try
            {
                if (!DesignerHelpers.DeserializeDesignerStates(service, reader))
                {
                    point.X = reader.ReadInt32();
                    point.Y = reader.ReadInt32();
                }
            }
            finally
            {
                base.PerformLayout();
                this.ScrollPosition = point;
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public Point LogicalPointToClient(Point logicalPoint)
        {
            return this.LogicalPointToClient(logicalPoint, true);
        }

        private Point LogicalPointToClient(Point point, bool mapToLayout)
        {
            if (mapToLayout)
            {
                point = this.activeLayout.MapOutCoOrdFromLayout(point);
            }
            Matrix matrix = new Matrix();
            matrix.Scale(this.ScaleZoomFactor, this.ScaleZoomFactor);
            Point[] pts = new Point[] { point };
            matrix.TransformPoints(pts);
            Matrix matrix2 = new Matrix();
            matrix2.Translate((float) -this.ScrollPosition.X, (float) -this.ScrollPosition.Y);
            matrix2.TransformPoints(pts);
            return pts[0];
        }

        public Point LogicalPointToScreen(Point logicalPoint)
        {
            return base.PointToScreen(this.LogicalPointToClient(logicalPoint));
        }

        public Rectangle LogicalRectangleToClient(Rectangle rectangle)
        {
            Rectangle rectangle2 = (this.activeLayout != null) ? this.activeLayout.MapOutRectangleFromLayout(rectangle) : rectangle;
            return new Rectangle(this.LogicalPointToClient(rectangle2.Location, false), this.LogicalSizeToClient(rectangle2.Size));
        }

        public Size LogicalSizeToClient(Size logicalSize)
        {
            Point[] pts = new Point[] { new Point(logicalSize) };
            Matrix matrix = new Matrix();
            matrix.Scale(this.ScaleZoomFactor, this.ScaleZoomFactor);
            matrix.TransformPoints(pts);
            return new Size(pts[0]);
        }

        internal void OnCommandKey(KeyEventArgs e)
        {
            this.OnKeyDown(e);
            this.OnKeyUp(e);
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            if (((e.Control != this.VScrollBar) && (e.Control != this.HScrollBar)) && (e.Control != this.toolContainer))
            {
                throw new InvalidOperationException(SR.GetString("Error_InsertingChildControls"));
            }
        }

        protected override void OnDragDrop(DragEventArgs dragEventArgs)
        {
            base.OnDragDrop(dragEventArgs);
            using (WorkflowMessageDispatchData data = new WorkflowMessageDispatchData(this, dragEventArgs))
            {
                foreach (WorkflowDesignerMessageFilter filter in data.Filters)
                {
                    if (((IWorkflowDesignerMessageSink) filter).OnDragDrop(dragEventArgs))
                    {
                        goto Label_004D;
                    }
                }
            }
        Label_004D:
            this.dragDropInProgress = false;
        }

        protected override void OnDragEnter(DragEventArgs dragEventArgs)
        {
            base.OnDragEnter(dragEventArgs);
            this.dragDropInProgress = true;
            using (WorkflowMessageDispatchData data = new WorkflowMessageDispatchData(this, dragEventArgs))
            {
                foreach (WorkflowDesignerMessageFilter filter in data.Filters)
                {
                    if (((IWorkflowDesignerMessageSink) filter).OnDragEnter(dragEventArgs))
                    {
                        return;
                    }
                }
            }
        }

        protected override void OnDragLeave(EventArgs e)
        {
            base.OnDragLeave(e);
            using (WorkflowMessageDispatchData data = new WorkflowMessageDispatchData(this, EventArgs.Empty))
            {
                foreach (WorkflowDesignerMessageFilter filter in data.Filters)
                {
                    if (((IWorkflowDesignerMessageSink) filter).OnDragLeave())
                    {
                        goto Label_0050;
                    }
                }
            }
        Label_0050:
            this.dragDropInProgress = false;
        }

        protected override void OnDragOver(DragEventArgs dragEventArgs)
        {
            base.OnDragOver(dragEventArgs);
            using (WorkflowMessageDispatchData data = new WorkflowMessageDispatchData(this, dragEventArgs))
            {
                foreach (WorkflowDesignerMessageFilter filter in data.Filters)
                {
                    if (((IWorkflowDesignerMessageSink) filter).OnDragOver(dragEventArgs))
                    {
                        return;
                    }
                }
            }
        }

        private void OnEnsureVisible(object sender, EventArgs e)
        {
            if (this.ensureVisibleEventHandler != null)
            {
                this.Idle -= this.ensureVisibleEventHandler;
                this.ensureVisibleEventHandler = null;
            }
            ISelectionService service = (ISelectionService) this.GetService(typeof(ISelectionService));
            if ((service != null) && (service.SelectionCount > 0))
            {
                ArrayList list = new ArrayList(service.GetSelectedComponents());
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    Rectangle empty = Rectangle.Empty;
                    if (list[i] is Activity)
                    {
                        ActivityDesigner designer = ActivityDesigner.GetDesigner(list[i] as Activity);
                        if (designer != null)
                        {
                            empty = designer.Bounds;
                            empty.Inflate(WorkflowTheme.CurrentTheme.AmbientTheme.SelectionSize);
                            empty.Inflate(WorkflowTheme.CurrentTheme.AmbientTheme.SelectionSize);
                        }
                    }
                    else if (list[i] is System.Workflow.ComponentModel.Design.HitTestInfo)
                    {
                        empty = ((System.Workflow.ComponentModel.Design.HitTestInfo) list[i]).Bounds;
                    }
                    if (!empty.IsEmpty)
                    {
                        this.EnsureVisible(empty);
                    }
                }
            }
        }

        private void OnFitToScreen(object sender, EventArgs e)
        {
            ActionButton button = sender as ActionButton;
            if ((button != null) && (button.State == ActionButton.States.Pressed))
            {
                if ((this.HScrollBar.Maximum > this.ViewPortSize.Width) || (this.VScrollBar.Maximum > this.ViewPortSize.Height))
                {
                    this.FitToScreenSize();
                }
                else if (this.Zoom != 100)
                {
                    this.FitToWorkflowSize();
                }
            }
        }

        protected override void OnGiveFeedback(GiveFeedbackEventArgs gfbevent)
        {
            base.OnGiveFeedback(gfbevent);
            using (WorkflowMessageDispatchData data = new WorkflowMessageDispatchData(this, gfbevent))
            {
                foreach (WorkflowDesignerMessageFilter filter in data.Filters)
                {
                    if (((IWorkflowDesignerMessageSink) filter).OnGiveFeedback(gfbevent))
                    {
                        return;
                    }
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            using (WorkflowMessageDispatchData data = new WorkflowMessageDispatchData(this, e))
            {
                foreach (WorkflowDesignerMessageFilter filter in data.Filters)
                {
                    if (((IWorkflowDesignerMessageSink) filter).OnKeyDown(e))
                    {
                        goto Label_0046;
                    }
                }
            }
        Label_0046:
            if (!e.Handled)
            {
                base.OnKeyDown(e);
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            using (WorkflowMessageDispatchData data = new WorkflowMessageDispatchData(this, e))
            {
                foreach (WorkflowDesignerMessageFilter filter in data.Filters)
                {
                    if (((IWorkflowDesignerMessageSink) filter).OnKeyUp(e))
                    {
                        goto Label_0046;
                    }
                }
            }
        Label_0046:
            if (!e.Handled)
            {
                base.OnKeyUp(e);
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            ScrollBar hScrollBar = this.HScrollBar;
            ScrollBar vScrollBar = this.VScrollBar;
            if (base.Controls.Contains(hScrollBar))
            {
                hScrollBar.Bounds = new Rectangle(0, Math.Max(0, base.Height - SystemInformation.HorizontalScrollBarHeight), Math.Max(base.Width - (vScrollBar.Visible ? SystemInformation.VerticalScrollBarWidth : 0), 0), SystemInformation.HorizontalScrollBarHeight);
            }
            if (base.Controls.Contains(vScrollBar))
            {
                vScrollBar.Bounds = new Rectangle(Math.Max(0, base.Width - SystemInformation.VerticalScrollBarWidth), 0, SystemInformation.VerticalScrollBarWidth, Math.Max(base.Height - (hScrollBar.Visible ? SystemInformation.HorizontalScrollBarHeight : 0), 0));
            }
            if (this.toolContainer != null)
            {
                this.toolContainer.Location = new Point(base.Width - this.toolContainer.Width, 0);
                this.toolContainer.Height = base.Height - (hScrollBar.Visible ? hScrollBar.Height : 0);
            }
            using (WorkflowMessageDispatchData data = new WorkflowMessageDispatchData(this, levent))
            {
                foreach (WorkflowDesignerMessageFilter filter in data.Filters)
                {
                    ((IWorkflowDesignerMessageSink) filter).OnLayout(levent);
                }
            }
            using (Graphics graphics = base.CreateGraphics())
            {
                this.activeLayout.Update(graphics, WorkflowLayout.LayoutUpdateReason.LayoutChanged);
                if (this.rootDesigner != null)
                {
                    this.rootDesigner.Location = this.activeLayout.RootDesignerAlignment;
                }
            }
            this.UpdateScrollRange();
            this.InvalidateClientRectangle(Rectangle.Empty);
        }

        protected override void OnMouseCaptureChanged(EventArgs e)
        {
            base.OnMouseCaptureChanged(e);
            using (WorkflowMessageDispatchData data = new WorkflowMessageDispatchData(this, EventArgs.Empty))
            {
                foreach (WorkflowDesignerMessageFilter filter in data.Filters)
                {
                    if (((IWorkflowDesignerMessageSink) filter).OnMouseCaptureChanged())
                    {
                        return;
                    }
                }
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            using (WorkflowMessageDispatchData data = new WorkflowMessageDispatchData(this, e))
            {
                foreach (WorkflowDesignerMessageFilter filter in data.Filters)
                {
                    if (((IWorkflowDesignerMessageSink) filter).OnMouseDoubleClick(e))
                    {
                        return;
                    }
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            using (WorkflowMessageDispatchData data = new WorkflowMessageDispatchData(this, e))
            {
                foreach (WorkflowDesignerMessageFilter filter in data.Filters)
                {
                    if (((IWorkflowDesignerMessageSink) filter).OnMouseDown(e))
                    {
                        return;
                    }
                }
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            Point point = base.PointToClient(Control.MousePosition);
            MouseEventArgs args = new MouseEventArgs(Control.MouseButtons, 1, point.X, point.Y, 0);
            using (WorkflowMessageDispatchData data = new WorkflowMessageDispatchData(this, args))
            {
                foreach (WorkflowDesignerMessageFilter filter in data.Filters)
                {
                    if (((IWorkflowDesignerMessageSink) filter).OnMouseEnter(args))
                    {
                        return;
                    }
                }
            }
        }

        protected override void OnMouseHover(EventArgs e)
        {
            base.OnMouseHover(e);
            Point point = base.PointToClient(Control.MousePosition);
            MouseEventArgs args = new MouseEventArgs(Control.MouseButtons, 1, point.X, point.Y, 0);
            using (WorkflowMessageDispatchData data = new WorkflowMessageDispatchData(this, args))
            {
                foreach (WorkflowDesignerMessageFilter filter in data.Filters)
                {
                    if (((IWorkflowDesignerMessageSink) filter).OnMouseHover(args))
                    {
                        return;
                    }
                }
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            using (WorkflowMessageDispatchData data = new WorkflowMessageDispatchData(this, EventArgs.Empty))
            {
                foreach (WorkflowDesignerMessageFilter filter in data.Filters)
                {
                    if (((IWorkflowDesignerMessageSink) filter).OnMouseLeave())
                    {
                        return;
                    }
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            using (WorkflowMessageDispatchData data = new WorkflowMessageDispatchData(this, e))
            {
                foreach (WorkflowDesignerMessageFilter filter in data.Filters)
                {
                    if (((IWorkflowDesignerMessageSink) filter).OnMouseMove(e))
                    {
                        return;
                    }
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            using (WorkflowMessageDispatchData data = new WorkflowMessageDispatchData(this, e))
            {
                foreach (WorkflowDesignerMessageFilter filter in data.Filters)
                {
                    if (((IWorkflowDesignerMessageSink) filter).OnMouseUp(e))
                    {
                        return;
                    }
                }
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            using (WorkflowMessageDispatchData data = new WorkflowMessageDispatchData(this, e))
            {
                foreach (WorkflowDesignerMessageFilter filter in data.Filters)
                {
                    if (((IWorkflowDesignerMessageSink) filter).OnMouseWheel(e))
                    {
                        return;
                    }
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            GraphicsContainer container = e.Graphics.BeginContainer();
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
            bool flag = (this.viewPortBitmap == null) || (this.viewPortBitmap.Size != this.ViewPortSize);
            if (flag)
            {
                if (this.viewPortBitmap != null)
                {
                    this.viewPortBitmap.Dispose();
                }
                this.viewPortBitmap = new Bitmap(Math.Max(1, this.ViewPortSize.Width), Math.Max(1, this.ViewPortSize.Height), e.Graphics);
            }
            ViewPortData viewPortData = new ViewPortData {
                LogicalViewPort = this.ClientRectangleToLogical(new Rectangle(Point.Empty, this.ViewPortSize)),
                MemoryBitmap = this.viewPortBitmap,
                Scaling = new SizeF(this.ScaleZoomFactor, this.ScaleZoomFactor),
                Translation = this.ScrollPosition,
                ShadowDepth = new Size(this.shadowDepth, this.shadowDepth),
                ViewPortSize = this.ViewPortSize
            };
            if ((this.layoutEventHandler == null) || flag)
            {
                TakeWorkflowSnapShot(this, viewPortData);
            }
            try
            {
                this.activeLayout.OnPaintWorkflow(e, viewPortData);
            }
            catch (Exception)
            {
            }
            using (WorkflowMessageDispatchData data2 = new WorkflowMessageDispatchData(this, EventArgs.Empty))
            {
                foreach (WorkflowDesignerMessageFilter filter in data2.Filters)
                {
                    try
                    {
                        if (((IWorkflowDesignerMessageSink) filter).OnPaintWorkflowAdornments(e, this.ViewPortRectangle))
                        {
                            goto Label_01A0;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        Label_01A0:
            e.Graphics.EndContainer(container);
            e.Graphics.FillRectangle(SystemBrushes.Control, new Rectangle(base.Width - SystemInformation.VerticalScrollBarWidth, base.Height - SystemInformation.HorizontalScrollBarHeight, SystemInformation.VerticalScrollBarWidth, SystemInformation.HorizontalScrollBarHeight));
        }

        private void OnPerformLayout(object sender, EventArgs e)
        {
            if (this.layoutEventHandler != null)
            {
                this.Idle -= this.layoutEventHandler;
                this.layoutEventHandler = null;
                base.PerformLayout();
            }
        }

        protected override void OnQueryContinueDrag(QueryContinueDragEventArgs qcdevent)
        {
            base.OnQueryContinueDrag(qcdevent);
            using (WorkflowMessageDispatchData data = new WorkflowMessageDispatchData(this, qcdevent))
            {
                foreach (WorkflowDesignerMessageFilter filter in data.Filters)
                {
                    if (((IWorkflowDesignerMessageSink) filter).OnQueryContinueDrag(qcdevent))
                    {
                        return;
                    }
                }
            }
        }

        protected virtual void OnRootDesignerChanged()
        {
            if (this.RootDesignerChanged != null)
            {
                this.RootDesignerChanged(this, EventArgs.Empty);
            }
        }

        private void OnScroll(object sender, EventArgs e)
        {
            this.InvalidateClientRectangle(Rectangle.Empty);
            ScrollBar bar = sender as ScrollBar;
            if (bar != null)
            {
                using (WorkflowMessageDispatchData data = new WorkflowMessageDispatchData(this, e))
                {
                    foreach (WorkflowDesignerMessageFilter filter in data.Filters)
                    {
                        try
                        {
                            ((IWorkflowDesignerMessageSink) filter).OnScroll(bar, bar.Value);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            if (this.commandSet != null)
            {
                this.commandSet.UpdateCommandSet();
            }
            if ((this.RootDesigner != null) && (this.RootDesigner.Activity != null))
            {
                ISelectionService service = this.GetService(typeof(ISelectionService)) as ISelectionService;
                if ((service != null) && service.GetComponentSelected(this.RootDesigner.Activity))
                {
                    IHelpService service2 = this.GetService(typeof(IHelpService)) as IHelpService;
                    if (service2 != null)
                    {
                        service2.AddContextAttribute("Keyword", this.RootDesigner.Activity.GetType().FullName, HelpKeywordType.F1Keyword);
                    }
                }
            }
        }

        private void OnTabChange(object sender, TabSelectionChangeEventArgs e)
        {
            if (((e.CurrentItem.Identifier == 1) || (e.CurrentItem.Identifier == 2)) || (e.CurrentItem.Identifier == 3))
            {
                Rectangle selectedTabBounds = e.SelectedTabBounds;
                CommandID menuID = null;
                if (e.CurrentItem.Identifier == 1)
                {
                    menuID = WorkflowMenuCommands.PageLayoutMenu;
                }
                else if (e.CurrentItem.Identifier == 2)
                {
                    menuID = WorkflowMenuCommands.ZoomMenu;
                }
                else
                {
                    menuID = WorkflowMenuCommands.PanMenu;
                }
                IMenuCommandService service = (IMenuCommandService) this.GetService(typeof(IMenuCommandService));
                if (service != null)
                {
                    service.ShowContextMenu(menuID, selectedTabBounds.Right, selectedTabBounds.Top);
                }
            }
        }

        internal void OnThemeChange(object sender, EventArgs e)
        {
            this.ShadowDepth = WorkflowTheme.CurrentTheme.AmbientTheme.ShadowDepth;
            using (WorkflowMessageDispatchData data = new WorkflowMessageDispatchData(this, EventArgs.Empty))
            {
                foreach (WorkflowDesignerMessageFilter filter in data.Filters)
                {
                    try
                    {
                        ((IWorkflowDesignerMessageSink) filter).OnThemeChange();
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            base.PerformLayout();
        }

        private void OnWorkflowIdle(object sender, EventArgs e)
        {
            if (this.idleEventListeners != null)
            {
                this.idleEventListeners(this, e);
            }
        }

        protected virtual void OnZoomChanged()
        {
            if (this.ZoomChanged != null)
            {
                this.ZoomChanged(this, EventArgs.Empty);
            }
        }

        public void PerformLayout(bool immediateUpdate)
        {
            if (immediateUpdate)
            {
                if (this.layoutEventHandler != null)
                {
                    this.Idle -= this.layoutEventHandler;
                    this.layoutEventHandler = null;
                }
                base.PerformLayout();
            }
            else if (this.layoutEventHandler == null)
            {
                this.layoutEventHandler = new EventHandler(this.OnPerformLayout);
                this.Idle += this.layoutEventHandler;
            }
        }

        private void PopulateMessageFilters(bool stockFilters)
        {
            IList<WorkflowDesignerMessageFilter> list = stockFilters ? this.stockMessageFilters : this.customMessageFilters;
            if (stockFilters)
            {
                list.Add(new GlyphManager());
                list.Add(new WindowManager());
            }
            else
            {
                if (base.Capture)
                {
                    base.Capture = false;
                }
                foreach (WorkflowDesignerMessageFilter filter in ((IWorkflowRootDesigner) this.rootDesigner).MessageFilters)
                {
                    list.Add(filter);
                }
            }
            foreach (WorkflowDesignerMessageFilter filter2 in list)
            {
                filter2.SetParentView(this);
            }
        }

        private void RefreshDynamicAction()
        {
            DynamicActionMessageFilter service = this.GetService(typeof(DynamicActionMessageFilter)) as DynamicActionMessageFilter;
            if ((service != null) && (this.fitAllAction != null))
            {
                if ((this.HScrollBar.Maximum > this.ViewPortSize.Width) || (this.VScrollBar.Maximum > this.ViewPortSize.Height))
                {
                    this.fitAllAction.Buttons[0].Description = DR.GetString("FitToScreenDescription", new object[0]);
                    this.fitAllAction.Buttons[0].StateImages = new Bitmap[] { DR.GetImage("FitToScreen") as Bitmap };
                    service.AddAction(this.fitAllAction);
                }
                else if (this.Zoom != 100)
                {
                    this.fitAllAction.Buttons[0].Description = DR.GetString("FitToWorkflowDescription", new object[0]);
                    this.fitAllAction.Buttons[0].StateImages = new Bitmap[] { DR.GetImage("FitToWorkflow") as Bitmap };
                    service.AddAction(this.fitAllAction);
                }
                else
                {
                    service.RemoveAction(this.fitAllAction);
                    this.fitAllAction.Buttons[0].State = ActionButton.States.Normal;
                }
            }
        }

        public void RemoveDesignerMessageFilter(WorkflowDesignerMessageFilter designerMessageFilter)
        {
            if (designerMessageFilter == null)
            {
                throw new ArgumentNullException("designerMessageFilter");
            }
            if (this.customMessageFilters.Contains(designerMessageFilter))
            {
                if (base.Capture)
                {
                    base.Capture = false;
                }
                this.customMessageFilters.Remove(designerMessageFilter);
                designerMessageFilter.Dispose();
            }
        }

        public void SaveViewState(Stream viewState)
        {
            if (viewState == null)
            {
                throw new ArgumentNullException("viewState");
            }
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            if (service == null)
            {
                throw new Exception(SR.GetString("General_MissingService", new object[] { typeof(IDesignerHost).FullName }));
            }
            BinaryWriter writer = new BinaryWriter(viewState);
            writer.Write(this.PrintPreviewMode);
            writer.Write(this.Zoom);
            DesignerHelpers.SerializeDesignerStates(service, writer);
            writer.Write(this.ScrollPosition.X);
            writer.Write(this.ScrollPosition.Y);
        }

        public void SaveWorkflowImage(Stream stream, ImageFormat imageFormat)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (imageFormat == null)
            {
                throw new ArgumentNullException("imageFormat");
            }
            Bitmap bitmap = this.TakeWorkflowSnapShot();
            if (bitmap != null)
            {
                bitmap.Save(stream, imageFormat);
                bitmap.Dispose();
            }
        }

        public void SaveWorkflowImage(string imageFile, ImageFormat imageFormat)
        {
            if (imageFile == null)
            {
                throw new ArgumentNullException("imageFile");
            }
            if (imageFormat == null)
            {
                throw new ArgumentNullException("imageFormat");
            }
            Bitmap bitmap = this.TakeWorkflowSnapShot();
            if (bitmap != null)
            {
                bitmap.Save(imageFile, imageFormat);
                bitmap.Dispose();
            }
        }

        public void SaveWorkflowImageToClipboard()
        {
            Bitmap data = this.TakeWorkflowSnapShot();
            if (data != null)
            {
                Clipboard.SetDataObject(data, true);
                data.Dispose();
            }
        }

        public Point ScreenPointToLogical(Point screenPoint)
        {
            return this.ClientPointToLogical(base.PointToClient(screenPoint));
        }

        public void ShowInfoTip(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }
            this.workflowToolTip.SetText(string.Empty, text);
        }

        public void ShowInfoTip(string title, string text)
        {
            if (title == null)
            {
                throw new ArgumentNullException("title");
            }
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }
            this.workflowToolTip.SetText(title, text);
        }

        public void ShowInPlaceToolTip(string toolTipText, Rectangle toolTipRectangle)
        {
            if (toolTipText == null)
            {
                throw new ArgumentNullException("toolTipText");
            }
            if (toolTipRectangle.IsEmpty)
            {
                throw new ArgumentException(SR.GetString("Error_EmptyToolTipRectangle"));
            }
            this.workflowToolTip.SetText(toolTipText, toolTipRectangle);
        }

        object IServiceProvider.GetService(System.Type serviceType)
        {
            return this.GetService(serviceType);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        bool IMessageFilter.PreFilterMessage(ref Message m)
        {
            bool flag = false;
            if (((m.Msg != 0x100) && (m.Msg != 260)) && ((m.Msg != 0x101) && (m.Msg != 0x105)))
            {
                return flag;
            }
            Control control = Control.FromHandle(m.HWnd);
            if ((control == null) || ((control != this) && !base.Controls.Contains(control)))
            {
                return flag;
            }
            KeyEventArgs e = new KeyEventArgs(((Keys) ((int) ((long) m.WParam))) | Control.ModifierKeys);
            if ((m.Msg == 0x100) || (m.Msg == 260))
            {
                this.OnKeyDown(e);
            }
            else
            {
                this.OnKeyUp(e);
            }
            return e.Handled;
        }

        private Bitmap TakeWorkflowSnapShot()
        {
            Bitmap bitmap = null;
            ActivityDesigner rootDesigner = this.RootDesigner;
            if (rootDesigner == null)
            {
                return bitmap;
            }
            using (Graphics graphics = base.CreateGraphics())
            {
                ViewPortData data;
                data = new ViewPortData {
                    LogicalViewPort = new Rectangle(Point.Empty, new Size(rootDesigner.Bounds.Width + (2 * DefaultWorkflowLayout.Separator.Width), rootDesigner.Bounds.Height + (2 * DefaultWorkflowLayout.Separator.Height))),
                    MemoryBitmap = new Bitmap(data.LogicalViewPort.Width, data.LogicalViewPort.Height, graphics),
                    Scaling = new SizeF(1f, 1f),
                    Translation = Point.Empty,
                    ShadowDepth = new Size(0, 0),
                    ViewPortSize = data.LogicalViewPort.Size
                };
                TakeWorkflowSnapShot(this, data);
                return data.MemoryBitmap;
            }
        }

        internal static void TakeWorkflowSnapShot(WorkflowView workflowView, ViewPortData viewPortData)
        {
            Bitmap memoryBitmap = viewPortData.MemoryBitmap;
            using (Graphics graphics = Graphics.FromImage(memoryBitmap))
            {
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                using (PaintEventArgs args = new PaintEventArgs(graphics, viewPortData.LogicalViewPort))
                {
                    workflowView.ActiveLayout.OnPaint(args, viewPortData);
                }
                Matrix matrix = new Matrix();
                matrix.Scale(viewPortData.Scaling.Width, viewPortData.Scaling.Height, MatrixOrder.Prepend);
                Point[] pts = new Point[] { viewPortData.LogicalViewPort.Location };
                matrix.TransformPoints(pts);
                matrix.Translate((float) (-pts[0].X + viewPortData.ShadowDepth.Width), (float) (-pts[0].Y + viewPortData.ShadowDepth.Height), MatrixOrder.Append);
                graphics.Transform = matrix;
                if (workflowView.RootDesigner != null)
                {
                    using (Region region = new Region())
                    {
                        using (GraphicsPath path = ActivityDesignerPaint.GetDesignerPath(workflowView.RootDesigner, false))
                        {
                            Region clip = graphics.Clip;
                            region.MakeEmpty();
                            region.Union(path);
                            graphics.Clip = region;
                            AmbientTheme ambientTheme = WorkflowTheme.CurrentTheme.AmbientTheme;
                            graphics.FillRectangle(ambientTheme.BackgroundBrush, workflowView.RootDesigner.Bounds);
                            if (ambientTheme.ShowGrid)
                            {
                                ActivityDesignerPaint.DrawGrid(graphics, workflowView.RootDesigner.Bounds);
                            }
                            graphics.Clip = clip;
                            try
                            {
                                using (PaintEventArgs args2 = new PaintEventArgs(graphics, viewPortData.LogicalViewPort))
                                {
                                    ((IWorkflowDesignerMessageSink) workflowView.RootDesigner).OnPaint(args2, viewPortData.LogicalViewPort);
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }
                using (PaintEventArgs args3 = new PaintEventArgs(graphics, workflowView.RootDesigner.Bounds))
                {
                    using (WorkflowMessageDispatchData data = new WorkflowMessageDispatchData(workflowView, EventArgs.Empty))
                    {
                        foreach (WorkflowDesignerMessageFilter filter in data.Filters)
                        {
                            try
                            {
                                if (((IWorkflowDesignerMessageSink) filter).OnPaint(args3, viewPortData.LogicalViewPort))
                                {
                                    goto Label_0230;
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }
            Label_0230:
                graphics.Transform = new Matrix();
                if (!viewPortData.ShadowDepth.IsEmpty)
                {
                    Bitmap image = new Bitmap(memoryBitmap);
                    using (Brush brush = new SolidBrush(Color.FromArgb(220, Color.White)))
                    {
                        graphics.FillRectangle(brush, new Rectangle(Point.Empty, new Size((memoryBitmap.Size.Width - viewPortData.ShadowDepth.Width) - 1, (memoryBitmap.Size.Height - viewPortData.ShadowDepth.Height) - 1)));
                    }
                    ImageAttributes imageAttr = new ImageAttributes();
                    imageAttr.SetColorKey(viewPortData.TransparentColor, viewPortData.TransparentColor, ColorAdjustType.Default);
                    imageAttr.SetColorKey(viewPortData.TransparentColor, viewPortData.TransparentColor, ColorAdjustType.Bitmap);
                    graphics.DrawImage(image, new Rectangle(-viewPortData.ShadowDepth.Width, -viewPortData.ShadowDepth.Height, memoryBitmap.Width, memoryBitmap.Height), 0, 0, memoryBitmap.Width, memoryBitmap.Height, GraphicsUnit.Pixel, imageAttr);
                    image.Dispose();
                }
            }
        }

        private void UpdateLayout()
        {
            if (this.layoutEventHandler != null)
            {
                this.PerformLayout(true);
                this.InvalidateClientRectangle(Rectangle.Empty);
            }
        }

        private void UpdateScrollRange()
        {
            if ((this.ViewPortSize.Width >= 0) && (this.ViewPortSize.Height >= 0))
            {
                Size viewPortSize = this.ViewPortSize;
                Size size2 = this.LogicalSizeToClient(this.activeLayout.Extent);
                int width = Math.Min(size2.Width, viewPortSize.Width);
                Size size3 = new Size(width, Math.Min(size2.Height, viewPortSize.Height));
                if (this.hScrollBar.Maximum != size2.Width)
                {
                    this.hScrollBar.Maximum = size2.Width;
                }
                if (this.vScrollBar.Maximum != size2.Height)
                {
                    this.vScrollBar.Maximum = size2.Height;
                }
                if (this.hScrollBar.LargeChange != size3.Width)
                {
                    this.hScrollBar.SmallChange = size3.Width / 15;
                    this.hScrollBar.LargeChange = size3.Width + 1;
                }
                if (this.vScrollBar.LargeChange != size3.Height)
                {
                    this.vScrollBar.SmallChange = size3.Height / 15;
                    this.vScrollBar.LargeChange = size3.Height + 1;
                }
                int num = size2.Width - this.hScrollBar.LargeChange;
                num = (num < 0) ? 0 : num;
                if (this.hScrollBar.Value > num)
                {
                    this.hScrollBar.Value = num;
                }
                int num2 = size2.Height - this.vScrollBar.LargeChange;
                num2 = (num2 < 0) ? 0 : num2;
                if (this.vScrollBar.Value > num2)
                {
                    this.vScrollBar.Value = num2;
                }
                this.RefreshDynamicAction();
                bool visible = this.hScrollBar.Visible;
                if (base.Controls.Contains(this.hScrollBar))
                {
                    this.hScrollBar.Visible = this.hScrollBar.Maximum > viewPortSize.Width;
                }
                bool flag2 = this.vScrollBar.Visible;
                if (base.Controls.Contains(this.vScrollBar))
                {
                    this.vScrollBar.Visible = this.vScrollBar.Maximum > viewPortSize.Height;
                }
                if ((visible != this.hScrollBar.Visible) || (this.vScrollBar.Visible != flag2))
                {
                    base.PerformLayout();
                    this.Refresh();
                }
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode), UIPermission(SecurityAction.Assert, Window=UIPermissionWindow.AllWindows)]
        protected override void WndProc(ref Message m)
        {
            using (WorkflowMessageDispatchData data = new WorkflowMessageDispatchData(this, EventArgs.Empty))
            {
                foreach (WorkflowDesignerMessageFilter filter in data.Filters)
                {
                    if (((IWorkflowDesignerMessageSink) filter).ProcessMessage(m))
                    {
                        break;
                    }
                }
                if (m.Msg == 0x7b)
                {
                    int lParam = (int) m.LParam;
                    Point screenMenuPoint = (lParam != -1) ? new Point(lParam) : Control.MousePosition;
                    foreach (WorkflowDesignerMessageFilter filter2 in data.Filters)
                    {
                        if (((IWorkflowDesignerMessageSink) filter2).OnShowContextMenu(screenMenuPoint))
                        {
                            break;
                        }
                    }
                    m.Result = IntPtr.Zero;
                    return;
                }
            }
            if ((this.workflowToolTip != null) && (m.Msg == 0x4e))
            {
                this.workflowToolTip.RelayParentNotify(ref m);
            }
            try
            {
                if (m.Result == IntPtr.Zero)
                {
                    base.WndProc(ref m);
                }
            }
            catch (Exception exception)
            {
                if (exception != CheckoutException.Canceled)
                {
                    DesignerHelpers.ShowError(this, exception);
                }
            }
        }

        internal WorkflowLayout ActiveLayout
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.activeLayout;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Layout cannot be null!");
                }
                Cursor current = Cursor.Current;
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    this.activeLayout = value;
                    if (this.activeLayout != ((WorkflowPrintDocument) this.PrintDocument).PrintPreviewLayout)
                    {
                        this.DefaultLayout = this.activeLayout;
                    }
                    base.PerformLayout();
                    if (this.commandSet != null)
                    {
                        this.commandSet.UpdatePageLayoutCommands(true);
                    }
                }
                finally
                {
                    Cursor.Current = current;
                }
            }
        }

        private WorkflowLayout DefaultLayout
        {
            get
            {
                if (this.defaultLayout == null)
                {
                    this.defaultLayout = new WorkflowRootLayout(this);
                }
                return this.defaultLayout;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(DR.GetString("Error_WorkflowLayoutNull", new object[0]));
                }
                if (this.defaultLayout != value)
                {
                    if (this.defaultLayout != null)
                    {
                        this.defaultLayout.Dispose();
                    }
                    this.defaultLayout = value;
                }
            }
        }

        internal bool DragDropInProgress
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.dragDropInProgress;
            }
        }

        public bool EnableFitToScreen
        {
            get
            {
                return (this.fitAllAction != null);
            }
            set
            {
                if (this.EnableFitToScreen != value)
                {
                    if (value)
                    {
                        if (this.fitAllAction == null)
                        {
                            this.fitAllAction = this.CreateDynamicAction();
                        }
                    }
                    else if (this.fitAllAction != null)
                    {
                        this.fitAllAction.Dispose();
                        this.fitAllAction = null;
                    }
                    this.InvalidateClientRectangle(Rectangle.Empty);
                }
            }
        }

        public System.Windows.Forms.HScrollBar HScrollBar
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.hScrollBar;
            }
        }

        internal System.Workflow.ComponentModel.Design.HitTestInfo MessageHitTestContext
        {
            get
            {
                return this.messageHitTestContexts.Peek();
            }
        }

        public System.Drawing.Printing.PrintDocument PrintDocument
        {
            get
            {
                if (this.printDocument == null)
                {
                    this.printDocument = new WorkflowPrintDocument(this);
                }
                return this.printDocument;
            }
        }

        public bool PrintPreviewMode
        {
            get
            {
                return (this.activeLayout == ((WorkflowPrintDocument) this.PrintDocument).PrintPreviewLayout);
            }
            set
            {
                if (this.PrintPreviewMode != value)
                {
                    if (value && (PrinterSettings.InstalledPrinters.Count == 0))
                    {
                        DesignerHelpers.ShowError(this, DR.GetString("ThereIsNoPrinterInstalledErrorMessage", new object[0]));
                        value = false;
                    }
                    this.ActiveLayout = value ? ((WorkflowPrintDocument) this.PrintDocument).PrintPreviewLayout : this.DefaultLayout;
                    if (this.commandSet != null)
                    {
                        this.commandSet.UpdatePageLayoutCommands(true);
                    }
                    if (this.PrintPreviewMode)
                    {
                        this.prePreviewZoom = this.Zoom;
                        this.prePreviewScroll = this.ScrollPosition;
                        this.Zoom = 40;
                    }
                    else
                    {
                        this.Zoom = this.prePreviewZoom;
                        this.ScrollPosition = this.prePreviewScroll;
                    }
                }
            }
        }

        public ActivityDesigner RootDesigner
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.rootDesigner;
            }
            set
            {
                if (this.rootDesigner != value)
                {
                    this.DisposeMessageFilters(false);
                    this.rootDesigner = value;
                    if (this.rootDesigner != null)
                    {
                        this.PopulateMessageFilters(false);
                        this.ActiveLayout = this.DefaultLayout = this.rootDesigner.SupportedLayout;
                    }
                    this.OnRootDesignerChanged();
                    base.PerformLayout();
                }
            }
        }

        private float ScaleZoomFactor
        {
            get
            {
                return (this.zoomLevel * this.activeLayout.Scaling);
            }
        }

        public Point ScrollPosition
        {
            get
            {
                return new Point(this.HScrollBar.Value, this.VScrollBar.Value);
            }
            set
            {
                ScrollBar hScrollBar = this.HScrollBar;
                if (hScrollBar != null)
                {
                    value.X = Math.Min(value.X, (hScrollBar.Maximum - hScrollBar.LargeChange) + 1);
                    value.X = Math.Max(value.X, hScrollBar.Minimum);
                    hScrollBar.Value = value.X;
                }
                ScrollBar vScrollBar = this.VScrollBar;
                if (vScrollBar != null)
                {
                    value.Y = Math.Min(value.Y, (vScrollBar.Maximum - vScrollBar.LargeChange) + 1);
                    value.Y = Math.Max(value.Y, vScrollBar.Minimum);
                    vScrollBar.Value = value.Y;
                }
            }
        }

        public int ShadowDepth
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.shadowDepth;
            }
            set
            {
                if ((value < 0) || (value > 8))
                {
                    throw new NotSupportedException(DR.GetString("ShadowDepthException", new object[] { 0, 8 }));
                }
                if (this.shadowDepth != value)
                {
                    this.shadowDepth = value;
                    this.InvalidateClientRectangle(Rectangle.Empty);
                }
            }
        }

        internal bool ShowToolContainer
        {
            get
            {
                return (this.toolContainer != null);
            }
            set
            {
                if (this.ShowToolContainer != value)
                {
                    try
                    {
                        base.SuspendLayout();
                        if (value)
                        {
                            this.toolContainer = new System.Workflow.ComponentModel.Design.TabControl(DockStyle.Right, AnchorAlignment.Far);
                            base.Controls.Add(this.toolContainer);
                            this.EnsureScrollBars(this.hScrollBar, this.toolContainer.ScrollBar as System.Windows.Forms.VScrollBar);
                            string[,] strArray = new string[,] { { "MultipageLayoutCaption", "MultipageLayout" }, { "ZoomCaption", "Zoom" }, { "PanCaption", "AutoPan" } };
                            for (int i = 0; i < strArray.GetLength(0); i++)
                            {
                                Bitmap image = DR.GetImage(strArray[i, 1]) as Bitmap;
                                string text = DR.GetString(strArray[i, 0], new object[0]);
                                this.toolContainer.TabStrip.Tabs.Add(new System.Workflow.ComponentModel.Design.ItemInfo(i + 1, image, text));
                            }
                            this.toolContainer.TabStrip.TabChange += new SelectionChangeEventHandler<TabSelectionChangeEventArgs>(this.OnTabChange);
                            if (this.commandSet != null)
                            {
                                this.commandSet.UpdatePageLayoutCommands(true);
                                this.commandSet.UpdateZoomCommands(true);
                                this.commandSet.UpdatePanCommands(true);
                            }
                        }
                        else
                        {
                            this.toolContainer.TabStrip.TabChange -= new SelectionChangeEventHandler<TabSelectionChangeEventArgs>(this.OnTabChange);
                            this.toolContainer.TabStrip.Tabs.Clear();
                            base.Controls.Remove(this.toolContainer);
                            this.toolContainer.Dispose();
                            this.toolContainer = null;
                            this.EnsureScrollBars(this.hScrollBar, new System.Windows.Forms.VScrollBar());
                        }
                    }
                    finally
                    {
                        base.ResumeLayout(true);
                    }
                }
            }
        }

        public Rectangle ViewPortRectangle
        {
            get
            {
                return new Rectangle(this.ScrollPosition, this.ViewPortSize);
            }
        }

        public Size ViewPortSize
        {
            get
            {
                Size clientSize = base.ClientSize;
                if (this.HScrollBar.Visible)
                {
                    clientSize.Height = Math.Max(0, clientSize.Height - this.HScrollBar.Height);
                }
                if (this.VScrollBar.Visible)
                {
                    clientSize.Width = Math.Max(0, clientSize.Width - this.VScrollBar.Width);
                }
                return clientSize;
            }
        }

        public System.Windows.Forms.VScrollBar VScrollBar
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.vScrollBar;
            }
        }

        public int Zoom
        {
            get
            {
                return Convert.ToInt32((float) (this.zoomLevel * 100f));
            }
            set
            {
                if (this.Zoom != value)
                {
                    if ((value < 10) || (value > 400))
                    {
                        throw new NotSupportedException(DR.GetString("ZoomLevelException2", new object[] { 10, 400 }));
                    }
                    ScrollBar hScrollBar = this.HScrollBar;
                    ScrollBar vScrollBar = this.VScrollBar;
                    if ((hScrollBar != null) && (vScrollBar != null))
                    {
                        PointF empty = (PointF) Point.Empty;
                        Point point = new Point(this.ScrollPosition.X, this.ScrollPosition.Y);
                        empty = new PointF(((float) point.X) / ((float) hScrollBar.Maximum), ((float) point.Y) / ((float) vScrollBar.Maximum));
                        this.zoomLevel = ((float) value) / 100f;
                        this.UpdateScrollRange();
                        Point point2 = new Point((int) (hScrollBar.Maximum * empty.X), (int) (vScrollBar.Maximum * empty.Y));
                        this.ScrollPosition = new Point(point2.X, point2.Y);
                        if (this.rootDesigner != null)
                        {
                            this.rootDesigner.Location = this.activeLayout.RootDesignerAlignment;
                        }
                        this.InvalidateClientRectangle(Rectangle.Empty);
                        this.activeLayout.Update(null, WorkflowLayout.LayoutUpdateReason.ZoomChanged);
                        IUIService service = this.GetService(typeof(IUIService)) as IUIService;
                        if (service != null)
                        {
                            service.SetUIDirty();
                        }
                        if (this.commandSet != null)
                        {
                            this.commandSet.UpdateZoomCommands(true);
                        }
                        this.OnZoomChanged();
                    }
                }
            }
        }

        private enum TabButtonIds
        {
            MultiPage = 1,
            Pan = 3,
            Zoom = 2
        }

        private sealed class WorkflowMessageDispatchData : IDisposable
        {
            private System.Workflow.ComponentModel.Design.HitTestInfo messageContext;
            private WorkflowView workflowView;

            public WorkflowMessageDispatchData(WorkflowView workflowView, EventArgs e)
            {
                this.workflowView = workflowView;
                if ((this.workflowView.RootDesigner != null) && (this.workflowView.stockMessageFilters.Count > 0))
                {
                    Point empty = Point.Empty;
                    if ((e is MouseEventArgs) || (e is DragEventArgs))
                    {
                        if (e is MouseEventArgs)
                        {
                            empty = new Point(((MouseEventArgs) e).X, ((MouseEventArgs) e).Y);
                        }
                        else if (e is DragEventArgs)
                        {
                            empty = this.workflowView.PointToClient(new Point(((DragEventArgs) e).X, ((DragEventArgs) e).Y));
                            this.workflowView.UpdateLayout();
                        }
                        Point point = this.workflowView.ClientPointToLogical(empty);
                        System.Workflow.ComponentModel.Design.HitTestInfo info = this.workflowView.RootDesigner.HitTest(point);
                        this.messageContext = (info != null) ? info : System.Workflow.ComponentModel.Design.HitTestInfo.Nowhere;
                        this.workflowView.messageHitTestContexts.Push(this.messageContext);
                    }
                }
            }

            void IDisposable.Dispose()
            {
                if ((this.workflowView != null) && (this.messageContext != null))
                {
                    System.Workflow.ComponentModel.Design.HitTestInfo messageContext = this.messageContext;
                    this.workflowView.messageHitTestContexts.Pop();
                }
            }

            public ReadOnlyCollection<WorkflowDesignerMessageFilter> Filters
            {
                get
                {
                    List<WorkflowDesignerMessageFilter> list = new List<WorkflowDesignerMessageFilter>();
                    list.AddRange(this.workflowView.customMessageFilters);
                    list.AddRange(this.workflowView.stockMessageFilters);
                    return list.AsReadOnly();
                }
            }
        }
    }
}

