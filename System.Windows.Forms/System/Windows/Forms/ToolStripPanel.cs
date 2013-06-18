namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Windows.Forms.Layout;

    [ToolboxBitmap(typeof(ToolStripPanel), "ToolStripPanel_standalone.bmp"), ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch), Designer("System.Windows.Forms.Design.ToolStripPanelDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public class ToolStripPanel : ContainerControl, IArrangedElement, IComponent, IDisposable
    {
        private System.Type currentRendererType;
        internal static readonly Padding DragMargin = new Padding(10);
        private static readonly object EventRendererChanged = new object();
        [ThreadStatic]
        private static FeedbackRectangle feedbackRect;
        [ThreadStatic]
        private static Rectangle lastFeedbackRect = Rectangle.Empty;
        private System.Windows.Forms.Orientation orientation;
        private ToolStripContainer owner;
        private static readonly int PropToolStripPanelRowCollection = PropertyStore.CreateKey();
        private ToolStripRendererSwitcher rendererSwitcher;
        private Padding rowMargin;
        private BitVector32 state;
        private static readonly int stateBeginInit = BitVector32.CreateMask(stateLocked);
        private static readonly int stateChangingZOrder = BitVector32.CreateMask(stateBeginInit);
        private static readonly int stateEndInit = BitVector32.CreateMask(stateInJoin);
        private static readonly int stateInJoin = BitVector32.CreateMask(stateChangingZOrder);
        private static readonly int stateLayoutSuspended = BitVector32.CreateMask(stateEndInit);
        private static readonly int stateLocked = BitVector32.CreateMask();
        private static readonly int stateRightToLeftChanged = BitVector32.CreateMask(stateLayoutSuspended);
        internal static TraceSwitch ToolStripPanelDebug;
        internal static TraceSwitch ToolStripPanelFeedbackDebug;
        internal static TraceSwitch ToolStripPanelMissingRowDebug;

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        public event EventHandler AutoSizeChanged
        {
            add
            {
                base.AutoSizeChanged += value;
            }
            remove
            {
                base.AutoSizeChanged -= value;
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ToolStripRendererChanged")]
        public event EventHandler RendererChanged
        {
            add
            {
                base.Events.AddHandler(EventRendererChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventRendererChanged, value);
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler TabIndexChanged
        {
            add
            {
                base.TabIndexChanged += value;
            }
            remove
            {
                base.TabIndexChanged -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler TabStopChanged
        {
            add
            {
                base.TabStopChanged += value;
            }
            remove
            {
                base.TabStopChanged -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler TextChanged
        {
            add
            {
                base.TextChanged += value;
            }
            remove
            {
                base.TextChanged -= value;
            }
        }

        public ToolStripPanel()
        {
            this.rowMargin = new Padding(3, 0, 0, 0);
            this.currentRendererType = typeof(System.Type);
            this.state = new BitVector32();
            base.SuspendLayout();
            base.AutoScaleMode = AutoScaleMode.None;
            this.InitFlowLayout();
            this.AutoSize = true;
            this.MinimumSize = Size.Empty;
            this.state[(stateLocked | stateBeginInit) | stateChangingZOrder] = false;
            this.TabStop = false;
            ToolStripManager.ToolStripPanels.Add(this);
            base.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor | ControlStyles.ResizeRedraw, true);
            base.SetStyle(ControlStyles.Selectable, false);
            base.ResumeLayout(true);
        }

        internal ToolStripPanel(ToolStripContainer owner) : this()
        {
            this.owner = owner;
        }

        public void BeginInit()
        {
            this.state[stateBeginInit] = true;
        }

        internal static void ClearDragFeedback()
        {
            FeedbackRectangle feedbackRect = ToolStripPanel.feedbackRect;
            ToolStripPanel.feedbackRect = null;
            if (feedbackRect != null)
            {
                feedbackRect.Dispose();
            }
        }

        protected override Control.ControlCollection CreateControlsInstance()
        {
            return new ToolStripPanelControlCollection(this);
        }

        private ToolStripPanelRowCollection CreateToolStripPanelRowCollection()
        {
            return new ToolStripPanelRowCollection(this);
        }

        [Conditional("DEBUG")]
        private void Debug_PrintRows()
        {
            for (int i = 0; i < this.RowsInternal.Count; i++)
            {
                for (int j = 0; j < this.RowsInternal[i].ControlsInternal.Count; j++)
                {
                }
            }
        }

        [Conditional("DEBUG")]
        private void Debug_VerifyCountRows()
        {
        }

        [Conditional("DEBUG")]
        private void Debug_VerifyNoOverlaps()
        {
            foreach (Control control in base.Controls)
            {
                foreach (Control control2 in base.Controls)
                {
                    if (control != control2)
                    {
                        Rectangle bounds = control.Bounds;
                        bounds.Intersect(control2.Bounds);
                        if (!LayoutUtils.IsZeroWidthOrHeight(bounds))
                        {
                            ISupportToolStripPanel panel = control as ISupportToolStripPanel;
                            ISupportToolStripPanel panel2 = control2 as ISupportToolStripPanel;
                            string str = string.Format(CultureInfo.CurrentCulture, "OVERLAP detection:\r\n{0}: {1} row {2} row bounds {3}", new object[] { (control.Name == null) ? "" : control.Name, control.Bounds, !this.RowsInternal.Contains(panel.ToolStripPanelRow) ? "unknown" : this.RowsInternal.IndexOf(panel.ToolStripPanelRow).ToString(CultureInfo.CurrentCulture), panel.ToolStripPanelRow.Bounds }) + string.Format(CultureInfo.CurrentCulture, "\r\n{0}: {1} row {2} row bounds {3}", new object[] { (control2.Name == null) ? "" : control2.Name, control2.Bounds, !this.RowsInternal.Contains(panel2.ToolStripPanelRow) ? "unknown" : this.RowsInternal.IndexOf(panel2.ToolStripPanelRow).ToString(CultureInfo.CurrentCulture), panel2.ToolStripPanelRow.Bounds });
                        }
                    }
                }
            }
        }

        [Conditional("DEBUG")]
        private void Debug_VerifyOneToOneCellRowControlMatchup()
        {
            for (int i = 0; i < this.RowsInternal.Count; i++)
            {
                ToolStripPanelRow row = this.RowsInternal[i];
                foreach (ToolStripPanelCell cell in row.Cells)
                {
                    if (cell.Control != null)
                    {
                        ToolStripPanelRow toolStripPanelRow = ((ISupportToolStripPanel) cell.Control).ToolStripPanelRow;
                        if (toolStripPanelRow != row)
                        {
                            int num2 = (toolStripPanelRow != null) ? this.RowsInternal.IndexOf(toolStripPanelRow) : -1;
                        }
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ToolStripManager.ToolStripPanels.Remove(this);
            }
            base.Dispose(disposing);
        }

        public void EndInit()
        {
            this.state[stateBeginInit] = false;
            this.state[stateEndInit] = true;
            try
            {
                if (!this.state[stateInJoin])
                {
                    this.JoinControls();
                }
            }
            finally
            {
                this.state[stateEndInit] = false;
            }
        }

        private Point GetStartLocation(ToolStrip toolStripToDrag)
        {
            if ((toolStripToDrag.IsCurrentlyDragging && (this.Orientation == System.Windows.Forms.Orientation.Horizontal)) && (toolStripToDrag.RightToLeft == RightToLeft.Yes))
            {
                return new Point(toolStripToDrag.Right, toolStripToDrag.Top);
            }
            return toolStripToDrag.Location;
        }

        private void GiveToolStripPanelFeedback(ToolStrip toolStripToDrag, Point screenLocation)
        {
            if ((this.Orientation == System.Windows.Forms.Orientation.Horizontal) && (this.RightToLeft == RightToLeft.Yes))
            {
                screenLocation.Offset(-toolStripToDrag.Width, 0);
            }
            if (CurrentFeedbackRect == null)
            {
                CurrentFeedbackRect = new FeedbackRectangle(toolStripToDrag.ClientRectangle);
            }
            if (!CurrentFeedbackRect.Visible)
            {
                toolStripToDrag.SuspendCaputureMode();
                try
                {
                    CurrentFeedbackRect.Show(screenLocation);
                    toolStripToDrag.CaptureInternal = true;
                }
                finally
                {
                    toolStripToDrag.ResumeCaputureMode();
                }
            }
            else
            {
                CurrentFeedbackRect.Move(screenLocation);
            }
        }

        private void HandleRendererChanged(object sender, EventArgs e)
        {
            this.OnRendererChanged(e);
        }

        private void InitFlowLayout()
        {
            if (this.Orientation == System.Windows.Forms.Orientation.Horizontal)
            {
                FlowLayout.SetFlowDirection(this, FlowDirection.TopDown);
            }
            else
            {
                FlowLayout.SetFlowDirection(this, FlowDirection.LeftToRight);
            }
            FlowLayout.SetWrapContents(this, false);
        }

        public void Join(ToolStrip toolStripToDrag)
        {
            this.Join(toolStripToDrag, Point.Empty);
        }

        public void Join(ToolStrip toolStripToDrag, Point location)
        {
            if (toolStripToDrag == null)
            {
                throw new ArgumentNullException("toolStripToDrag");
            }
            if (!this.state[stateBeginInit] && !this.state[stateInJoin])
            {
                try
                {
                    this.state[stateInJoin] = true;
                    toolStripToDrag.ParentInternal = this;
                    this.MoveInsideContainer(toolStripToDrag, location);
                    return;
                }
                finally
                {
                    this.state[stateInJoin] = false;
                }
            }
            base.Controls.Add(toolStripToDrag);
            toolStripToDrag.Location = location;
        }

        public void Join(ToolStrip toolStripToDrag, int row)
        {
            if (row < 0)
            {
                throw new ArgumentOutOfRangeException("row", System.Windows.Forms.SR.GetString("IndexOutOfRange", new object[] { row.ToString(CultureInfo.CurrentCulture) }));
            }
            Point empty = Point.Empty;
            Rectangle dragBounds = Rectangle.Empty;
            if (row >= this.RowsInternal.Count)
            {
                dragBounds = this.DragBounds;
            }
            else
            {
                dragBounds = this.RowsInternal[row].DragBounds;
            }
            if (this.Orientation == System.Windows.Forms.Orientation.Horizontal)
            {
                empty = new Point(0, dragBounds.Bottom - 1);
            }
            else
            {
                empty = new Point(dragBounds.Right - 1, 0);
            }
            this.Join(toolStripToDrag, empty);
        }

        public void Join(ToolStrip toolStripToDrag, int x, int y)
        {
            this.Join(toolStripToDrag, new Point(x, y));
        }

        private void JoinControls()
        {
            this.JoinControls(false);
        }

        private void JoinControls(bool forceLayout)
        {
            ToolStripPanelControlCollection controls = base.Controls as ToolStripPanelControlCollection;
            if (controls.Count > 0)
            {
                controls.Sort();
                Control[] array = new Control[controls.Count];
                controls.CopyTo(array, 0);
                for (int i = 0; i < array.Length; i++)
                {
                    int count = this.RowsInternal.Count;
                    ISupportToolStripPanel panel = array[i] as ISupportToolStripPanel;
                    if (((panel == null) || (panel.ToolStripPanelRow == null)) || (panel.IsCurrentlyDragging || !panel.ToolStripPanelRow.Bounds.Contains(array[i].Location)))
                    {
                        if (array[i].AutoSize)
                        {
                            array[i].Size = array[i].PreferredSize;
                        }
                        Point location = array[i].Location;
                        if (this.state[stateRightToLeftChanged])
                        {
                            location = new Point(base.Width - array[i].Right, location.Y);
                        }
                        this.Join(array[i] as ToolStrip, array[i].Location);
                        if ((count < this.RowsInternal.Count) || forceLayout)
                        {
                            this.OnLayout(new LayoutEventArgs(this, PropertyNames.Rows));
                        }
                    }
                }
            }
            this.state[stateRightToLeftChanged] = false;
        }

        internal void MoveControl(ToolStrip toolStripToDrag, Point screenLocation)
        {
            if (toolStripToDrag != null)
            {
                Point pt = base.PointToClient(screenLocation);
                if (!this.DragBounds.Contains(pt))
                {
                    this.MoveOutsideContainer(toolStripToDrag, screenLocation);
                }
                else
                {
                    this.Join(toolStripToDrag, pt);
                }
            }
        }

        private void MoveInsideContainer(ToolStrip toolStripToDrag, Point clientLocation)
        {
            ISupportToolStripPanel panel = toolStripToDrag;
            if (!panel.IsCurrentlyDragging || this.DragBounds.Contains(clientLocation))
            {
                bool flag = false;
                ClearDragFeedback();
                if ((((toolStripToDrag.Site != null) && toolStripToDrag.Site.DesignMode) && base.IsHandleCreated) && ((clientLocation.X < 0) || (clientLocation.Y < 0)))
                {
                    Point pt = base.PointToClient(WindowsFormsUtils.LastCursorPoint);
                    if (base.ClientRectangle.Contains(pt))
                    {
                        clientLocation = pt;
                    }
                }
                ToolStripPanelRow toolStripPanelRow = panel.ToolStripPanelRow;
                bool flag2 = false;
                if (((toolStripPanelRow != null) && toolStripPanelRow.Visible) && (toolStripPanelRow.ToolStripPanel == this))
                {
                    if (toolStripToDrag.IsCurrentlyDragging)
                    {
                        flag2 = toolStripPanelRow.DragBounds.Contains(clientLocation);
                    }
                    else
                    {
                        flag2 = toolStripPanelRow.Bounds.Contains(clientLocation);
                    }
                }
                if (flag2)
                {
                    panel.ToolStripPanelRow.MoveControl(toolStripToDrag, this.GetStartLocation(toolStripToDrag), clientLocation);
                }
                else
                {
                    ToolStripPanelRow row2 = this.PointToRow(clientLocation);
                    if (row2 == null)
                    {
                        int count = this.RowsInternal.Count;
                        if (this.Orientation == System.Windows.Forms.Orientation.Horizontal)
                        {
                            count = (clientLocation.Y <= base.Padding.Left) ? 0 : count;
                        }
                        else
                        {
                            count = (clientLocation.X <= base.Padding.Left) ? 0 : count;
                        }
                        ToolStripPanelRow row3 = null;
                        if (this.RowsInternal.Count > 0)
                        {
                            if (count == 0)
                            {
                                row3 = this.RowsInternal[0];
                            }
                            else if (count > 0)
                            {
                                row3 = this.RowsInternal[count - 1];
                            }
                        }
                        if (((row3 != null) && (row3.ControlsInternal.Count == 1)) && row3.ControlsInternal.Contains(toolStripToDrag))
                        {
                            row2 = row3;
                            if (toolStripToDrag.IsInDesignMode)
                            {
                                Point endClientLocation = (this.Orientation == System.Windows.Forms.Orientation.Horizontal) ? new Point(clientLocation.X, row2.Bounds.Y) : new Point(row2.Bounds.X, clientLocation.Y);
                                panel.ToolStripPanelRow.MoveControl(toolStripToDrag, this.GetStartLocation(toolStripToDrag), endClientLocation);
                            }
                        }
                        else
                        {
                            row2 = new ToolStripPanelRow(this);
                            this.RowsInternal.Insert(count, row2);
                        }
                    }
                    else if (!row2.CanMove(toolStripToDrag))
                    {
                        int index = this.RowsInternal.IndexOf(row2);
                        if (((toolStripPanelRow != null) && (toolStripPanelRow.ControlsInternal.Count == 1)) && ((index > 0) && ((index - 1) == this.RowsInternal.IndexOf(toolStripPanelRow))))
                        {
                            return;
                        }
                        row2 = new ToolStripPanelRow(this);
                        this.RowsInternal.Insert(index, row2);
                        clientLocation.Y = row2.Bounds.Y;
                    }
                    flag = toolStripPanelRow != row2;
                    if ((!flag && (toolStripPanelRow != null)) && (toolStripPanelRow.ControlsInternal.Count > 1))
                    {
                        toolStripPanelRow.LeaveRow(toolStripToDrag);
                        toolStripPanelRow = null;
                        flag = true;
                    }
                    if (flag)
                    {
                        if (toolStripPanelRow != null)
                        {
                            toolStripPanelRow.LeaveRow(toolStripToDrag);
                        }
                        row2.JoinRow(toolStripToDrag, clientLocation);
                    }
                    if (flag && panel.IsCurrentlyDragging)
                    {
                        for (int i = 0; i < this.RowsInternal.Count; i++)
                        {
                            LayoutTransaction.DoLayout(this.RowsInternal[i], this, PropertyNames.Rows);
                        }
                        if (this.RowsInternal.IndexOf(row2) > 0)
                        {
                            System.Windows.Forms.IntSecurity.AdjustCursorPosition.Assert();
                            try
                            {
                                Point point3 = toolStripToDrag.PointToScreen(toolStripToDrag.GripRectangle.Location);
                                if (this.Orientation == System.Windows.Forms.Orientation.Vertical)
                                {
                                    point3.X += toolStripToDrag.GripRectangle.Width / 2;
                                    point3.Y = Cursor.Position.Y;
                                }
                                else
                                {
                                    point3.Y += toolStripToDrag.GripRectangle.Height / 2;
                                    point3.X = Cursor.Position.X;
                                }
                                Cursor.Position = point3;
                            }
                            finally
                            {
                                CodeAccessPermission.RevertAssert();
                            }
                        }
                    }
                }
            }
        }

        private void MoveOutsideContainer(ToolStrip toolStripToDrag, Point screenLocation)
        {
            ToolStripPanel controlToLayout = ToolStripManager.ToolStripPanelFromPoint(toolStripToDrag, screenLocation);
            if (controlToLayout != null)
            {
                using (new LayoutTransaction(controlToLayout, controlToLayout, null))
                {
                    controlToLayout.MoveControl(toolStripToDrag, screenLocation);
                }
                toolStripToDrag.PerformLayout();
            }
            else
            {
                this.GiveToolStripPanelFeedback(toolStripToDrag, screenLocation);
            }
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            if (!this.state[stateBeginInit] && !this.state[stateInJoin])
            {
                if (!this.state[stateLayoutSuspended])
                {
                    this.Join(e.Control as ToolStrip, e.Control.Location);
                }
                else
                {
                    this.BeginInit();
                }
            }
        }

        protected override void OnControlRemoved(ControlEventArgs e)
        {
            ISupportToolStripPanel control = e.Control as ISupportToolStripPanel;
            if ((control != null) && (control.ToolStripPanelRow != null))
            {
                control.ToolStripPanelRow.ControlsInternal.Remove(e.Control);
            }
            base.OnControlRemoved(e);
        }

        protected override void OnDockChanged(EventArgs e)
        {
            this.PerformUpdate();
            base.OnDockChanged(e);
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            if ((e.AffectedComponent != this.ParentInternal) && (e.AffectedComponent is Control))
            {
                ISupportToolStripPanel affectedComponent = e.AffectedComponent as ISupportToolStripPanel;
                if ((affectedComponent != null) && this.RowsInternal.Contains(affectedComponent.ToolStripPanelRow))
                {
                    LayoutTransaction.DoLayout(affectedComponent.ToolStripPanelRow, e.AffectedComponent as IArrangedElement, e.AffectedProperty);
                }
            }
            base.OnLayout(e);
        }

        internal override void OnLayoutResuming(bool resumeLayout)
        {
            base.OnLayoutResuming(resumeLayout);
            this.state[stateLayoutSuspended] = false;
            if (this.state[stateBeginInit])
            {
                this.EndInit();
            }
        }

        internal override void OnLayoutSuspended()
        {
            base.OnLayoutSuspended();
            this.state[stateLayoutSuspended] = true;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            ToolStripPanelRenderEventArgs args = new ToolStripPanelRenderEventArgs(e.Graphics, this);
            this.Renderer.DrawToolStripPanelBackground(args);
            if (!args.Handled)
            {
                base.OnPaintBackground(e);
            }
        }

        protected override void OnParentChanged(EventArgs e)
        {
            this.PerformUpdate();
            base.OnParentChanged(e);
        }

        protected virtual void OnRendererChanged(EventArgs e)
        {
            this.Renderer.InitializePanel(this);
            base.Invalidate();
            EventHandler handler = (EventHandler) base.Events[EventRendererChanged];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnRightToLeftChanged(EventArgs e)
        {
            base.OnRightToLeftChanged(e);
            if (!this.state[stateBeginInit])
            {
                if (base.Controls.Count > 0)
                {
                    base.SuspendLayout();
                    Control[] controlArray = new Control[base.Controls.Count];
                    Point[] pointArray = new Point[base.Controls.Count];
                    int index = 0;
                    foreach (ToolStripPanelRow row in this.RowsInternal)
                    {
                        foreach (Control control in row.ControlsInternal)
                        {
                            controlArray[index] = control;
                            pointArray[index] = new Point(row.Bounds.Width - control.Right, control.Top);
                            index++;
                        }
                    }
                    base.Controls.Clear();
                    for (int i = 0; i < controlArray.Length; i++)
                    {
                        this.Join(controlArray[i] as ToolStrip, pointArray[i]);
                    }
                    base.ResumeLayout(true);
                }
            }
            else
            {
                this.state[stateRightToLeftChanged] = true;
            }
        }

        internal void PerformUpdate()
        {
            this.PerformUpdate(false);
        }

        internal void PerformUpdate(bool forceLayout)
        {
            if (!this.state[stateBeginInit] && !this.state[stateInJoin])
            {
                this.JoinControls(forceLayout);
            }
        }

        public ToolStripPanelRow PointToRow(Point clientLocation)
        {
            foreach (ToolStripPanelRow row in this.RowsInternal)
            {
                Rectangle rectangle = LayoutUtils.InflateRect(row.Bounds, row.Margin);
                if (this.ParentInternal != null)
                {
                    if ((this.Orientation == System.Windows.Forms.Orientation.Horizontal) && (rectangle.Width == 0))
                    {
                        rectangle.Width = this.ParentInternal.DisplayRectangle.Width;
                    }
                    else if ((this.Orientation == System.Windows.Forms.Orientation.Vertical) && (rectangle.Height == 0))
                    {
                        rectangle.Height = this.ParentInternal.DisplayRectangle.Height;
                    }
                }
                if (rectangle.Contains(clientLocation))
                {
                    return row;
                }
            }
            return null;
        }

        private void ResetRenderMode()
        {
            this.RendererSwitcher.ResetRenderMode();
        }

        private bool ShouldSerializeDock()
        {
            return ((this.owner == null) && (this.Dock != DockStyle.None));
        }

        private bool ShouldSerializeRenderMode()
        {
            return this.RendererSwitcher.ShouldSerializeRenderMode();
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override bool AllowDrop
        {
            get
            {
                return base.AllowDrop;
            }
            set
            {
                base.AllowDrop = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public override bool AutoScroll
        {
            get
            {
                return base.AutoScroll;
            }
            set
            {
                base.AutoScroll = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public Size AutoScrollMargin
        {
            get
            {
                return base.AutoScrollMargin;
            }
            set
            {
                base.AutoScrollMargin = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Size AutoScrollMinSize
        {
            get
            {
                return base.AutoScrollMinSize;
            }
            set
            {
                base.AutoScrollMinSize = value;
            }
        }

        [DefaultValue(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override bool AutoSize
        {
            get
            {
                return base.AutoSize;
            }
            set
            {
                base.AutoSize = value;
            }
        }

        private static FeedbackRectangle CurrentFeedbackRect
        {
            get
            {
                return feedbackRect;
            }
            set
            {
                feedbackRect = value;
            }
        }

        protected override Padding DefaultMargin
        {
            get
            {
                return Padding.Empty;
            }
        }

        protected override Padding DefaultPadding
        {
            get
            {
                return Padding.Empty;
            }
        }

        public override DockStyle Dock
        {
            get
            {
                return base.Dock;
            }
            set
            {
                base.Dock = value;
                if ((value == DockStyle.Left) || (value == DockStyle.Right))
                {
                    this.Orientation = System.Windows.Forms.Orientation.Vertical;
                }
                else
                {
                    this.Orientation = System.Windows.Forms.Orientation.Horizontal;
                }
            }
        }

        internal Rectangle DragBounds
        {
            get
            {
                return LayoutUtils.InflateRect(base.ClientRectangle, DragMargin);
            }
        }

        internal bool IsInDesignMode
        {
            get
            {
                return base.DesignMode;
            }
        }

        public override System.Windows.Forms.Layout.LayoutEngine LayoutEngine
        {
            get
            {
                return FlowLayout.Instance;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false), DefaultValue(false)]
        public bool Locked
        {
            get
            {
                return this.state[stateLocked];
            }
            set
            {
                this.state[stateLocked] = value;
            }
        }

        public System.Windows.Forms.Orientation Orientation
        {
            get
            {
                return this.orientation;
            }
            set
            {
                if (this.orientation != value)
                {
                    this.orientation = value;
                    this.rowMargin = LayoutUtils.FlipPadding(this.rowMargin);
                    this.InitFlowLayout();
                    foreach (ToolStripPanelRow row in this.RowsInternal)
                    {
                        row.OnOrientationChanged();
                    }
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ToolStripRenderer Renderer
        {
            get
            {
                return this.RendererSwitcher.Renderer;
            }
            set
            {
                this.RendererSwitcher.Renderer = value;
            }
        }

        private ToolStripRendererSwitcher RendererSwitcher
        {
            get
            {
                if (this.rendererSwitcher == null)
                {
                    this.rendererSwitcher = new ToolStripRendererSwitcher(this);
                    this.HandleRendererChanged(this, EventArgs.Empty);
                    this.rendererSwitcher.RendererChanged += new EventHandler(this.HandleRendererChanged);
                }
                return this.rendererSwitcher;
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ToolStripRenderModeDescr")]
        public ToolStripRenderMode RenderMode
        {
            get
            {
                return this.RendererSwitcher.RenderMode;
            }
            set
            {
                this.RendererSwitcher.RenderMode = value;
            }
        }

        public Padding RowMargin
        {
            get
            {
                return this.rowMargin;
            }
            set
            {
                this.rowMargin = value;
                LayoutTransaction.DoLayout(this, this, "RowMargin");
            }
        }

        [Browsable(false), System.Windows.Forms.SRDescription("ToolStripPanelRowsDescr"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ToolStripPanelRow[] Rows
        {
            get
            {
                ToolStripPanelRow[] array = new ToolStripPanelRow[this.RowsInternal.Count];
                this.RowsInternal.CopyTo(array, 0);
                return array;
            }
        }

        [Browsable(false), System.Windows.Forms.SRDescription("ToolStripPanelRowsDescr"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        internal ToolStripPanelRowCollection RowsInternal
        {
            get
            {
                ToolStripPanelRowCollection rows = (ToolStripPanelRowCollection) base.Properties.GetObject(PropToolStripPanelRowCollection);
                if (rows == null)
                {
                    rows = this.CreateToolStripPanelRowCollection();
                    base.Properties.SetObject(PropToolStripPanelRowCollection, rows);
                }
                return rows;
            }
        }

        ArrangedElementCollection IArrangedElement.Children
        {
            get
            {
                return this.RowsInternal;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public int TabIndex
        {
            get
            {
                return base.TabIndex;
            }
            set
            {
                base.TabIndex = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool TabStop
        {
            get
            {
                return base.TabStop;
            }
            set
            {
                base.TabStop = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
            }
        }

        private class FeedbackRectangle : IDisposable
        {
            private FeedbackDropDown dropDown;

            public FeedbackRectangle(Rectangle bounds)
            {
                this.dropDown = new FeedbackDropDown(bounds);
            }

            public void Dispose()
            {
                this.Dispose(true);
            }

            protected void Dispose(bool disposing)
            {
                if (disposing && (this.dropDown != null))
                {
                    this.Visible = false;
                    this.dropDown.Dispose();
                    this.dropDown = null;
                }
            }

            ~FeedbackRectangle()
            {
                this.Dispose(false);
            }

            public void Move(Point newLocation)
            {
                this.dropDown.MoveTo(newLocation);
            }

            public void Show(Point newLocation)
            {
                this.dropDown.Show(newLocation);
            }

            public bool Visible
            {
                get
                {
                    return (((this.dropDown != null) && !this.dropDown.IsDisposed) && this.dropDown.Visible);
                }
                set
                {
                    if ((this.dropDown != null) && !this.dropDown.IsDisposed)
                    {
                        this.dropDown.Visible = value;
                    }
                }
            }

            private class FeedbackDropDown : ToolStripDropDown
            {
                private int _numPaintsServiced;
                private const int MAX_PAINTS_TO_SERVICE = 20;

                public FeedbackDropDown(Rectangle bounds)
                {
                    base.SetStyle(ControlStyles.AllPaintingInWmPaint, false);
                    base.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
                    base.SetStyle(ControlStyles.CacheText, true);
                    base.AutoClose = false;
                    this.AutoSize = false;
                    base.DropShadowEnabled = false;
                    base.Bounds = bounds;
                    Rectangle rect = bounds;
                    rect.Inflate(-1, -1);
                    Region region = new Region(bounds);
                    region.Exclude(rect);
                    System.Windows.Forms.IntSecurity.ChangeWindowRegionForTopLevel.Assert();
                    base.Region = region;
                }

                private void ForceSynchronousPaint()
                {
                    if (!base.IsDisposed && (this._numPaintsServiced == 0))
                    {
                        try
                        {
                            System.Windows.Forms.NativeMethods.MSG msg = new System.Windows.Forms.NativeMethods.MSG();
                            while (System.Windows.Forms.UnsafeNativeMethods.PeekMessage(ref msg, new HandleRef(this, IntPtr.Zero), 15, 15, 1))
                            {
                                System.Windows.Forms.SafeNativeMethods.UpdateWindow(new HandleRef(null, msg.hwnd));
                                if (this._numPaintsServiced++ > 20)
                                {
                                    return;
                                }
                            }
                        }
                        finally
                        {
                            this._numPaintsServiced = 0;
                        }
                    }
                }

                public void MoveTo(Point newLocation)
                {
                    base.Location = newLocation;
                    this.ForceSynchronousPaint();
                }

                protected override void OnOpening(CancelEventArgs e)
                {
                    base.OnOpening(e);
                    e.Cancel = false;
                }

                protected override void OnPaint(PaintEventArgs e)
                {
                }

                protected override void OnPaintBackground(PaintEventArgs e)
                {
                    base.Renderer.DrawToolStripBackground(new ToolStripRenderEventArgs(e.Graphics, this));
                    base.Renderer.DrawToolStripBorder(new ToolStripRenderEventArgs(e.Graphics, this));
                }

                [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
                protected override void WndProc(ref Message m)
                {
                    if (m.Msg == 0x84)
                    {
                        m.Result = (IntPtr) (-1);
                    }
                    base.WndProc(ref m);
                }
            }
        }

        internal class ToolStripPanelControlCollection : WindowsFormsUtils.TypedControlCollection
        {
            private ToolStripPanel owner;

            public ToolStripPanelControlCollection(ToolStripPanel owner) : base(owner, typeof(ToolStrip))
            {
                this.owner = owner;
            }

            internal override void AddInternal(Control value)
            {
                if (value != null)
                {
                    using (new LayoutTransaction(value, value, PropertyNames.Parent))
                    {
                        base.AddInternal(value);
                        return;
                    }
                }
                base.AddInternal(value);
            }

            internal void Sort()
            {
                if (this.owner.Orientation == Orientation.Horizontal)
                {
                    base.InnerList.Sort(new YXComparer());
                }
                else
                {
                    base.InnerList.Sort(new XYComparer());
                }
            }

            public class XYComparer : IComparer
            {
                public int Compare(object first, object second)
                {
                    Control control = first as Control;
                    Control control2 = second as Control;
                    if (control.Bounds.X < control2.Bounds.X)
                    {
                        return -1;
                    }
                    if ((control.Bounds.X == control2.Bounds.X) && (control.Bounds.Y < control2.Bounds.Y))
                    {
                        return -1;
                    }
                    return 1;
                }
            }

            public class YXComparer : IComparer
            {
                public int Compare(object first, object second)
                {
                    Control control = first as Control;
                    Control control2 = second as Control;
                    if (control.Bounds.Y < control2.Bounds.Y)
                    {
                        return -1;
                    }
                    if ((control.Bounds.Y == control2.Bounds.Y) && (control.Bounds.X < control2.Bounds.X))
                    {
                        return -1;
                    }
                    return 1;
                }
            }
        }

        [ListBindable(false), ComVisible(false)]
        public class ToolStripPanelRowCollection : ArrangedElementCollection, IList, ICollection, IEnumerable
        {
            private ToolStripPanel owner;

            public ToolStripPanelRowCollection(ToolStripPanel owner)
            {
                this.owner = owner;
            }

            public ToolStripPanelRowCollection(ToolStripPanel owner, ToolStripPanelRow[] value)
            {
                this.owner = owner;
                this.AddRange(value);
            }

            public int Add(ToolStripPanelRow value)
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                int index = base.InnerList.Add(value);
                this.OnAdd(value, index);
                return index;
            }

            public void AddRange(ToolStripPanelRow[] value)
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                ToolStripPanel owner = this.owner;
                if (owner != null)
                {
                    owner.SuspendLayout();
                }
                try
                {
                    for (int i = 0; i < value.Length; i++)
                    {
                        this.Add(value[i]);
                    }
                }
                finally
                {
                    if (owner != null)
                    {
                        owner.ResumeLayout();
                    }
                }
            }

            public void AddRange(ToolStripPanel.ToolStripPanelRowCollection value)
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                ToolStripPanel owner = this.owner;
                if (owner != null)
                {
                    owner.SuspendLayout();
                }
                try
                {
                    int count = value.Count;
                    for (int i = 0; i < count; i++)
                    {
                        this.Add(value[i]);
                    }
                }
                finally
                {
                    if (owner != null)
                    {
                        owner.ResumeLayout();
                    }
                }
            }

            public virtual void Clear()
            {
                if (this.owner != null)
                {
                    this.owner.SuspendLayout();
                }
                try
                {
                    while (this.Count != 0)
                    {
                        this.RemoveAt(this.Count - 1);
                    }
                }
                finally
                {
                    if (this.owner != null)
                    {
                        this.owner.ResumeLayout();
                    }
                }
            }

            public bool Contains(ToolStripPanelRow value)
            {
                return base.InnerList.Contains(value);
            }

            public void CopyTo(ToolStripPanelRow[] array, int index)
            {
                base.InnerList.CopyTo(array, index);
            }

            public int IndexOf(ToolStripPanelRow value)
            {
                return base.InnerList.IndexOf(value);
            }

            public void Insert(int index, ToolStripPanelRow value)
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                base.InnerList.Insert(index, value);
                this.OnAdd(value, index);
            }

            private void OnAdd(ToolStripPanelRow value, int index)
            {
                if (this.owner != null)
                {
                    LayoutTransaction.DoLayout(this.owner, value, PropertyNames.Parent);
                }
            }

            private void OnAfterRemove(ToolStripPanelRow row)
            {
            }

            public void Remove(ToolStripPanelRow value)
            {
                base.InnerList.Remove(value);
                this.OnAfterRemove(value);
            }

            public void RemoveAt(int index)
            {
                ToolStripPanelRow row = null;
                if ((index < this.Count) && (index >= 0))
                {
                    row = (ToolStripPanelRow) base.InnerList[index];
                }
                base.InnerList.RemoveAt(index);
                this.OnAfterRemove(row);
            }

            int IList.Add(object value)
            {
                return this.Add(value as ToolStripPanelRow);
            }

            void IList.Clear()
            {
                this.Clear();
            }

            bool IList.Contains(object value)
            {
                return base.InnerList.Contains(value);
            }

            int IList.IndexOf(object value)
            {
                return this.IndexOf(value as ToolStripPanelRow);
            }

            void IList.Insert(int index, object value)
            {
                this.Insert(index, value as ToolStripPanelRow);
            }

            void IList.Remove(object value)
            {
                this.Remove(value as ToolStripPanelRow);
            }

            void IList.RemoveAt(int index)
            {
                this.RemoveAt(index);
            }

            public virtual ToolStripPanelRow this[int index]
            {
                get
                {
                    return (ToolStripPanelRow) base.InnerList[index];
                }
            }

            bool IList.IsFixedSize
            {
                get
                {
                    return base.InnerList.IsFixedSize;
                }
            }

            bool IList.IsReadOnly
            {
                get
                {
                    return base.InnerList.IsReadOnly;
                }
            }

            object IList.this[int index]
            {
                get
                {
                    return base.InnerList[index];
                }
                set
                {
                    throw new NotSupportedException(System.Windows.Forms.SR.GetString("ToolStripCollectionMustInsertAndRemove"));
                }
            }
        }
    }
}

