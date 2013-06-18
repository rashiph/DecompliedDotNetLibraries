namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Reflection;
    using System.Windows.Forms.Layout;

    [ToolboxItem(false)]
    public class ToolStripPanelRow : Component, IArrangedElement, IComponent, IDisposable
    {
        private Rectangle bounds;
        private const int DragInflateSize = 4;
        private const int MINALLOWEDWIDTH = 50;
        private System.Windows.Forms.ToolStripPanel parent;
        private static readonly int PropControlsCollection = PropertyStore.CreateKey();
        private PropertyStore propertyStore;
        private ToolStripPanelRowManager rowManager;
        private BitVector32 state;
        private static readonly int stateCachedBoundsMode = BitVector32.CreateMask(stateInitialized);
        private static readonly int stateDisposing = BitVector32.CreateMask(stateVisible);
        private static readonly int stateInitialized = BitVector32.CreateMask(stateLocked);
        private static readonly int stateInLayout = BitVector32.CreateMask(stateCachedBoundsMode);
        private static readonly int stateLocked = BitVector32.CreateMask(stateDisposing);
        private static readonly int stateVisible = BitVector32.CreateMask();
        private int suspendCount;
        internal static readonly TraceSwitch ToolStripPanelMouseDebug;
        internal static TraceSwitch ToolStripPanelRowCreationDebug;

        public ToolStripPanelRow(System.Windows.Forms.ToolStripPanel parent) : this(parent, true)
        {
        }

        internal ToolStripPanelRow(System.Windows.Forms.ToolStripPanel parent, bool visible)
        {
            this.bounds = Rectangle.Empty;
            this.state = new BitVector32();
            this.propertyStore = new PropertyStore();
            this.parent = parent;
            this.state[stateVisible] = visible;
            this.state[(stateDisposing | stateLocked) | stateInitialized] = false;
            using (new LayoutTransaction(parent, this, null))
            {
                this.Margin = this.DefaultMargin;
                CommonProperties.SetAutoSize(this, true);
            }
        }

        private void ApplyCachedBounds()
        {
            for (int i = 0; i < this.Cells.Count; i++)
            {
                IArrangedElement element = this.Cells[i];
                if (element.ParticipatesInLayout)
                {
                    ToolStripPanelCell cell = element as ToolStripPanelCell;
                    element.SetBounds(cell.CachedBounds, BoundsSpecified.None);
                }
            }
        }

        public bool CanMove(ToolStrip toolStripToDrag)
        {
            return ((!this.ToolStripPanel.Locked && !this.Locked) && this.RowManager.CanMove(toolStripToDrag));
        }

        private ToolStripPanelRowControlCollection CreateControlsInstance()
        {
            return new ToolStripPanelRowControlCollection(this);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    this.state[stateDisposing] = true;
                    this.ControlsInternal.Clear();
                }
            }
            finally
            {
                this.state[stateDisposing] = false;
                base.Dispose(disposing);
            }
        }

        internal Size GetMinimumSize(ToolStrip toolStrip)
        {
            if (toolStrip.MinimumSize == Size.Empty)
            {
                return new Size(50, 50);
            }
            return toolStrip.MinimumSize;
        }

        internal void JoinRow(ToolStrip toolStripToDrag, Point locationToDrag)
        {
            this.RowManager.JoinRow(toolStripToDrag, locationToDrag);
        }

        internal void LeaveRow(ToolStrip toolStripToDrag)
        {
            this.RowManager.LeaveRow(toolStripToDrag);
            if (this.ControlsInternal.Count == 0)
            {
                this.ToolStripPanel.RowsInternal.Remove(this);
                base.Dispose();
            }
        }

        internal void MoveControl(ToolStrip movingControl, Point startClientLocation, Point endClientLocation)
        {
            this.RowManager.MoveControl(movingControl, startClientLocation, endClientLocation);
        }

        protected void OnBoundsChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            ((IArrangedElement) this).PerformLayout(this, PropertyNames.Size);
            this.RowManager.OnBoundsChanged(oldBounds, newBounds);
        }

        protected internal virtual void OnControlAdded(Control control, int index)
        {
            ISupportToolStripPanel panel = control as ISupportToolStripPanel;
            if (panel != null)
            {
                panel.ToolStripPanelRow = this;
            }
            this.RowManager.OnControlAdded(control, index);
        }

        protected internal virtual void OnControlRemoved(Control control, int index)
        {
            if (!this.state[stateDisposing])
            {
                this.SuspendLayout();
                this.RowManager.OnControlRemoved(control, index);
                ISupportToolStripPanel panel = control as ISupportToolStripPanel;
                if ((panel != null) && (panel.ToolStripPanelRow == this))
                {
                    panel.ToolStripPanelRow = null;
                }
                this.ResumeLayout(true);
                if (this.ControlsInternal.Count <= 0)
                {
                    this.ToolStripPanel.RowsInternal.Remove(this);
                    base.Dispose();
                }
            }
        }

        protected virtual void OnLayout(LayoutEventArgs e)
        {
            if (this.Initialized && !this.state[stateInLayout])
            {
                this.state[stateInLayout] = true;
                try
                {
                    this.Margin = this.DefaultMargin;
                    this.CachedBoundsMode = true;
                    try
                    {
                        this.LayoutEngine.Layout(this, e);
                    }
                    finally
                    {
                        this.CachedBoundsMode = false;
                    }
                    if (this.RowManager.GetNextVisibleCell(this.Cells.Count - 1, false) == null)
                    {
                        this.ApplyCachedBounds();
                    }
                    else if (this.Orientation == System.Windows.Forms.Orientation.Horizontal)
                    {
                        this.OnLayoutHorizontalPostFix();
                    }
                    else
                    {
                        this.OnLayoutVerticalPostFix();
                    }
                }
                finally
                {
                    this.state[stateInLayout] = false;
                }
            }
        }

        private void OnLayoutHorizontalPostFix()
        {
            ToolStripPanelCell nextVisibleCell = this.RowManager.GetNextVisibleCell(this.Cells.Count - 1, false);
            if (nextVisibleCell == null)
            {
                this.ApplyCachedBounds();
            }
            else
            {
                int spaceToFree = nextVisibleCell.CachedBounds.Right - this.RowManager.DisplayRectangle.Right;
                if (spaceToFree <= 0)
                {
                    this.ApplyCachedBounds();
                }
                else
                {
                    int[] numArray = new int[this.Cells.Count];
                    for (int i = 0; i < this.Cells.Count; i++)
                    {
                        ToolStripPanelCell cell2 = this.Cells[i] as ToolStripPanelCell;
                        numArray[i] = cell2.Margin.Left;
                    }
                    spaceToFree -= this.RowManager.FreeSpaceFromRow(spaceToFree);
                    for (int j = 0; j < this.Cells.Count; j++)
                    {
                        ToolStripPanelCell cell3 = this.Cells[j] as ToolStripPanelCell;
                        Rectangle cachedBounds = cell3.CachedBounds;
                        cachedBounds.X -= Math.Max(0, numArray[j] - cell3.Margin.Left);
                        cell3.CachedBounds = cachedBounds;
                    }
                    if (spaceToFree <= 0)
                    {
                        this.ApplyCachedBounds();
                    }
                    else
                    {
                        int[] numArray2 = null;
                        for (int k = this.Cells.Count - 1; k >= 0; k--)
                        {
                            ToolStripPanelCell cell4 = this.Cells[k] as ToolStripPanelCell;
                            if (cell4.Visible)
                            {
                                Size minimumSize = this.GetMinimumSize(cell4.Control as ToolStrip);
                                Rectangle rectangle2 = cell4.CachedBounds;
                                if (rectangle2.Width > minimumSize.Width)
                                {
                                    spaceToFree -= rectangle2.Width - minimumSize.Width;
                                    rectangle2.Width = (spaceToFree < 0) ? (minimumSize.Width + -spaceToFree) : minimumSize.Width;
                                    for (int m = k + 1; m < this.Cells.Count; m++)
                                    {
                                        if (numArray2 == null)
                                        {
                                            numArray2 = new int[this.Cells.Count];
                                        }
                                        numArray2[m] += Math.Max(0, cell4.CachedBounds.Width - rectangle2.Width);
                                    }
                                    cell4.CachedBounds = rectangle2;
                                }
                            }
                            if (spaceToFree <= 0)
                            {
                                break;
                            }
                        }
                        if (numArray2 != null)
                        {
                            for (int n = 0; n < this.Cells.Count; n++)
                            {
                                ToolStripPanelCell cell5 = this.Cells[n] as ToolStripPanelCell;
                                Rectangle rectangle3 = cell5.CachedBounds;
                                rectangle3.X -= numArray2[n];
                                cell5.CachedBounds = rectangle3;
                            }
                        }
                        this.ApplyCachedBounds();
                    }
                }
            }
        }

        private void OnLayoutVerticalPostFix()
        {
            int spaceToFree = this.RowManager.GetNextVisibleCell(this.Cells.Count - 1, false).CachedBounds.Bottom - this.RowManager.DisplayRectangle.Bottom;
            if (spaceToFree <= 0)
            {
                this.ApplyCachedBounds();
            }
            else
            {
                int[] numArray = new int[this.Cells.Count];
                for (int i = 0; i < this.Cells.Count; i++)
                {
                    ToolStripPanelCell cell2 = this.Cells[i] as ToolStripPanelCell;
                    numArray[i] = cell2.Margin.Top;
                }
                spaceToFree -= this.RowManager.FreeSpaceFromRow(spaceToFree);
                for (int j = 0; j < this.Cells.Count; j++)
                {
                    ToolStripPanelCell cell3 = this.Cells[j] as ToolStripPanelCell;
                    Rectangle cachedBounds = cell3.CachedBounds;
                    cachedBounds.X = Math.Max(0, (cachedBounds.X - numArray[j]) - cell3.Margin.Top);
                    cell3.CachedBounds = cachedBounds;
                }
                if (spaceToFree <= 0)
                {
                    this.ApplyCachedBounds();
                }
                else
                {
                    int[] numArray2 = null;
                    for (int k = this.Cells.Count - 1; k >= 0; k--)
                    {
                        ToolStripPanelCell cell4 = this.Cells[k] as ToolStripPanelCell;
                        if (cell4.Visible)
                        {
                            Size minimumSize = this.GetMinimumSize(cell4.Control as ToolStrip);
                            Rectangle rectangle2 = cell4.CachedBounds;
                            if (rectangle2.Height > minimumSize.Height)
                            {
                                spaceToFree -= rectangle2.Height - minimumSize.Height;
                                rectangle2.Height = (spaceToFree < 0) ? (minimumSize.Height + -spaceToFree) : minimumSize.Height;
                                for (int m = k + 1; m < this.Cells.Count; m++)
                                {
                                    if (numArray2 == null)
                                    {
                                        numArray2 = new int[this.Cells.Count];
                                    }
                                    numArray2[m] += Math.Max(0, cell4.CachedBounds.Height - rectangle2.Height);
                                }
                                cell4.CachedBounds = rectangle2;
                            }
                        }
                        if (spaceToFree <= 0)
                        {
                            break;
                        }
                    }
                    if (numArray2 != null)
                    {
                        for (int n = 0; n < this.Cells.Count; n++)
                        {
                            ToolStripPanelCell cell5 = this.Cells[n] as ToolStripPanelCell;
                            Rectangle rectangle3 = cell5.CachedBounds;
                            rectangle3.Y -= numArray2[n];
                            cell5.CachedBounds = rectangle3;
                        }
                    }
                    this.ApplyCachedBounds();
                }
            }
        }

        protected internal virtual void OnOrientationChanged()
        {
            this.rowManager = null;
        }

        [Conditional("DEBUG")]
        private void PrintPlacements(int index)
        {
        }

        private void ResumeLayout(bool performLayout)
        {
            this.suspendCount--;
            if (performLayout)
            {
                ((IArrangedElement) this).PerformLayout(this, null);
            }
        }

        private void SetBounds(Rectangle bounds)
        {
            if (bounds != this.bounds)
            {
                Rectangle oldBounds = this.bounds;
                this.bounds = bounds;
                this.OnBoundsChanged(oldBounds, bounds);
            }
        }

        private void SuspendLayout()
        {
            this.suspendCount++;
        }

        Size IArrangedElement.GetPreferredSize(Size constrainingSize)
        {
            Size size = this.LayoutEngine.GetPreferredSize(this, constrainingSize - this.Padding.Size) + this.Padding.Size;
            if ((this.Orientation == System.Windows.Forms.Orientation.Horizontal) && (this.ParentInternal != null))
            {
                size.Width = this.DisplayRectangle.Width;
                return size;
            }
            size.Height = this.DisplayRectangle.Height;
            return size;
        }

        void IArrangedElement.PerformLayout(IArrangedElement container, string propertyName)
        {
            if (this.suspendCount <= 0)
            {
                this.OnLayout(new LayoutEventArgs(container, propertyName));
            }
        }

        void IArrangedElement.SetBounds(Rectangle bounds, BoundsSpecified specified)
        {
            this.SetBounds(bounds);
        }

        public Rectangle Bounds
        {
            get
            {
                return this.bounds;
            }
        }

        internal bool CachedBoundsMode
        {
            get
            {
                return this.state[stateCachedBoundsMode];
            }
            set
            {
                this.state[stateCachedBoundsMode] = value;
            }
        }

        internal ArrangedElementCollection Cells
        {
            get
            {
                return this.ControlsInternal.Cells;
            }
        }

        [Browsable(false), System.Windows.Forms.SRDescription("ControlControlsDescr"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Control[] Controls
        {
            get
            {
                Control[] array = new Control[this.ControlsInternal.Count];
                this.ControlsInternal.CopyTo(array, 0);
                return array;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("ControlControlsDescr"), Browsable(false)]
        internal ToolStripPanelRowControlCollection ControlsInternal
        {
            get
            {
                ToolStripPanelRowControlCollection controls = (ToolStripPanelRowControlCollection) this.Properties.GetObject(PropControlsCollection);
                if (controls == null)
                {
                    controls = this.CreateControlsInstance();
                    this.Properties.SetObject(PropControlsCollection, controls);
                }
                return controls;
            }
        }

        protected virtual System.Windows.Forms.Padding DefaultMargin
        {
            get
            {
                ToolStripPanelCell nextVisibleCell = this.RowManager.GetNextVisibleCell(0, true);
                if (((nextVisibleCell == null) || (nextVisibleCell.DraggedControl == null)) || !nextVisibleCell.DraggedControl.Stretch)
                {
                    return this.ToolStripPanel.RowMargin;
                }
                System.Windows.Forms.Padding rowMargin = this.ToolStripPanel.RowMargin;
                if (this.Orientation == System.Windows.Forms.Orientation.Horizontal)
                {
                    rowMargin.Left = 0;
                    rowMargin.Right = 0;
                    return rowMargin;
                }
                rowMargin.Top = 0;
                rowMargin.Bottom = 0;
                return rowMargin;
            }
        }

        protected virtual System.Windows.Forms.Padding DefaultPadding
        {
            get
            {
                return System.Windows.Forms.Padding.Empty;
            }
        }

        public Rectangle DisplayRectangle
        {
            get
            {
                return this.RowManager.DisplayRectangle;
            }
        }

        internal Rectangle DragBounds
        {
            get
            {
                return this.RowManager.DragBounds;
            }
        }

        private bool Initialized
        {
            get
            {
                return this.state[stateInitialized];
            }
            set
            {
                this.state[stateInitialized] = value;
            }
        }

        public System.Windows.Forms.Layout.LayoutEngine LayoutEngine
        {
            get
            {
                return FlowLayout.Instance;
            }
        }

        internal bool Locked
        {
            get
            {
                return this.state[stateLocked];
            }
        }

        public System.Windows.Forms.Padding Margin
        {
            get
            {
                return CommonProperties.GetMargin(this);
            }
            set
            {
                if (this.Margin != value)
                {
                    CommonProperties.SetMargin(this, value);
                }
            }
        }

        public System.Windows.Forms.Orientation Orientation
        {
            get
            {
                return this.ToolStripPanel.Orientation;
            }
        }

        public virtual System.Windows.Forms.Padding Padding
        {
            get
            {
                return CommonProperties.GetPadding(this, this.DefaultPadding);
            }
            set
            {
                if (this.Padding != value)
                {
                    CommonProperties.SetPadding(this, value);
                }
            }
        }

        internal Control ParentInternal
        {
            get
            {
                return this.parent;
            }
        }

        internal PropertyStore Properties
        {
            get
            {
                return this.propertyStore;
            }
        }

        private ToolStripPanelRowManager RowManager
        {
            get
            {
                if (this.rowManager == null)
                {
                    this.rowManager = (this.Orientation == System.Windows.Forms.Orientation.Horizontal) ? ((ToolStripPanelRowManager) new HorizontalRowManager(this)) : ((ToolStripPanelRowManager) new VerticalRowManager(this));
                    this.Initialized = true;
                }
                return this.rowManager;
            }
        }

        ArrangedElementCollection IArrangedElement.Children
        {
            get
            {
                return this.Cells;
            }
        }

        IArrangedElement IArrangedElement.Container
        {
            get
            {
                return this.ToolStripPanel;
            }
        }

        Rectangle IArrangedElement.DisplayRectangle
        {
            get
            {
                return this.Bounds;
            }
        }

        bool IArrangedElement.ParticipatesInLayout
        {
            get
            {
                return this.Visible;
            }
        }

        PropertyStore IArrangedElement.Properties
        {
            get
            {
                return this.Properties;
            }
        }

        public System.Windows.Forms.ToolStripPanel ToolStripPanel
        {
            get
            {
                return this.parent;
            }
        }

        internal bool Visible
        {
            get
            {
                return this.state[stateVisible];
            }
        }

        private class HorizontalRowManager : ToolStripPanelRow.ToolStripPanelRowManager
        {
            private const int DRAG_BOUNDS_INFLATE = 4;

            public HorizontalRowManager(ToolStripPanelRow owner) : base(owner)
            {
                owner.SuspendLayout();
                base.FlowLayoutSettings.WrapContents = false;
                base.FlowLayoutSettings.FlowDirection = FlowDirection.LeftToRight;
                owner.ResumeLayout(false);
            }

            public override bool CanMove(ToolStrip toolStripToDrag)
            {
                if (!base.CanMove(toolStripToDrag))
                {
                    return false;
                }
                Size empty = Size.Empty;
                for (int i = 0; i < base.Row.ControlsInternal.Count; i++)
                {
                    empty += base.Row.GetMinimumSize(base.Row.ControlsInternal[i] as ToolStrip);
                }
                empty += base.Row.GetMinimumSize(toolStripToDrag);
                return (empty.Width < this.DisplayRectangle.Width);
            }

            protected internal override int FreeSpaceFromRow(int spaceToFree)
            {
                int num = spaceToFree;
                if (spaceToFree > 0)
                {
                    ToolStripPanelCell nextVisibleCell = base.GetNextVisibleCell(base.Row.Cells.Count - 1, false);
                    if (nextVisibleCell == null)
                    {
                        return 0;
                    }
                    Padding margin = nextVisibleCell.Margin;
                    if (margin.Left >= spaceToFree)
                    {
                        margin.Left -= spaceToFree;
                        margin.Right = 0;
                        spaceToFree = 0;
                    }
                    else
                    {
                        spaceToFree -= nextVisibleCell.Margin.Left;
                        margin.Left = 0;
                        margin.Right = 0;
                    }
                    nextVisibleCell.Margin = margin;
                    spaceToFree -= this.MoveLeft(base.Row.Cells.Count - 1, spaceToFree);
                    if (spaceToFree > 0)
                    {
                        spaceToFree -= nextVisibleCell.Shrink(spaceToFree);
                    }
                }
                return (num - Math.Max(0, spaceToFree));
            }

            public override void JoinRow(ToolStrip toolStripToDrag, Point locationToDrag)
            {
                if (!base.Row.ControlsInternal.Contains(toolStripToDrag))
                {
                    base.Row.SuspendLayout();
                    try
                    {
                        if (base.Row.ControlsInternal.Count > 0)
                        {
                            int index = 0;
                            while (index < base.Row.Cells.Count)
                            {
                                ToolStripPanelCell cell = base.Row.Cells[index] as ToolStripPanelCell;
                                if ((cell.Visible || cell.ControlInDesignMode) && (base.Row.Cells[index].Bounds.Contains(locationToDrag) || (base.Row.Cells[index].Bounds.X >= locationToDrag.X)))
                                {
                                    break;
                                }
                                index++;
                            }
                            Control control1 = base.Row.ControlsInternal[index];
                            if (index < base.Row.ControlsInternal.Count)
                            {
                                base.Row.ControlsInternal.Insert(index, toolStripToDrag);
                            }
                            else
                            {
                                base.Row.ControlsInternal.Add(toolStripToDrag);
                            }
                            int num2 = toolStripToDrag.AutoSize ? toolStripToDrag.PreferredSize.Width : toolStripToDrag.Width;
                            int num3 = num2;
                            if (index == 0)
                            {
                                num3 += locationToDrag.X;
                            }
                            int num4 = 0;
                            if (index < (base.Row.ControlsInternal.Count - 1))
                            {
                                ToolStripPanelCell cell2 = (ToolStripPanelCell) base.Row.Cells[index + 1];
                                Padding margin = cell2.Margin;
                                if (margin.Left > num3)
                                {
                                    margin.Left -= num3;
                                    cell2.Margin = margin;
                                    num4 = num3;
                                }
                                else
                                {
                                    num4 = this.MoveRight(index + 1, num3 - num4);
                                    if (num4 > 0)
                                    {
                                        margin = cell2.Margin;
                                        margin.Left = Math.Max(0, margin.Left - num4);
                                        cell2.Margin = margin;
                                    }
                                }
                            }
                            else
                            {
                                ToolStripPanelCell nextVisibleCell = base.GetNextVisibleCell(base.Row.Cells.Count - 2, false);
                                ToolStripPanelCell cell4 = base.GetNextVisibleCell(base.Row.Cells.Count - 1, false);
                                if ((nextVisibleCell != null) && (cell4 != null))
                                {
                                    Padding padding2 = cell4.Margin;
                                    padding2.Left = Math.Max(0, locationToDrag.X - nextVisibleCell.Bounds.Right);
                                    cell4.Margin = padding2;
                                    num4 = num3;
                                }
                            }
                            if ((num4 < num3) && (index > 0))
                            {
                                num4 = this.MoveLeft(index - 1, num3 - num4);
                            }
                            if ((index == 0) && ((num4 - num2) > 0))
                            {
                                ToolStripPanelCell cell5 = base.Row.Cells[index] as ToolStripPanelCell;
                                Padding padding3 = cell5.Margin;
                                padding3.Left = num4 - num2;
                                cell5.Margin = padding3;
                            }
                        }
                        else
                        {
                            base.Row.ControlsInternal.Add(toolStripToDrag);
                            if ((base.Row.Cells.Count > 0) || toolStripToDrag.IsInDesignMode)
                            {
                                ToolStripPanelCell cell6 = base.GetNextVisibleCell(base.Row.Cells.Count - 1, false);
                                if ((cell6 == null) && toolStripToDrag.IsInDesignMode)
                                {
                                    cell6 = (ToolStripPanelCell) base.Row.Cells[base.Row.Cells.Count - 1];
                                }
                                if (cell6 != null)
                                {
                                    Padding padding4 = cell6.Margin;
                                    padding4.Left = Math.Max(0, locationToDrag.X - base.Row.Margin.Left);
                                    cell6.Margin = padding4;
                                }
                            }
                        }
                    }
                    finally
                    {
                        base.Row.ResumeLayout(true);
                    }
                }
            }

            public override void LeaveRow(ToolStrip toolStripToDrag)
            {
                base.Row.SuspendLayout();
                int index = base.Row.ControlsInternal.IndexOf(toolStripToDrag);
                if (index >= 0)
                {
                    if (index < (base.Row.ControlsInternal.Count - 1))
                    {
                        ToolStripPanelCell cell = (ToolStripPanelCell) base.Row.Cells[index];
                        if (cell.Visible)
                        {
                            int num2 = cell.Margin.Horizontal + cell.Bounds.Width;
                            ToolStripPanelCell nextVisibleCell = base.GetNextVisibleCell(index + 1, true);
                            if (nextVisibleCell != null)
                            {
                                Padding margin = nextVisibleCell.Margin;
                                margin.Left += num2;
                                nextVisibleCell.Margin = margin;
                            }
                        }
                    }
                    ((IList) base.Row.Cells).RemoveAt(index);
                }
                base.Row.ResumeLayout(true);
            }

            public override void MoveControl(ToolStrip movingControl, Point clientStartLocation, Point clientEndLocation)
            {
                if (!base.Row.Locked)
                {
                    if (this.DragBounds.Contains(clientEndLocation))
                    {
                        int index = base.Row.ControlsInternal.IndexOf(movingControl);
                        int spaceToFree = clientEndLocation.X - clientStartLocation.X;
                        if (spaceToFree < 0)
                        {
                            this.MoveLeft(index, spaceToFree * -1);
                        }
                        else
                        {
                            this.MoveRight(index, spaceToFree);
                        }
                    }
                    else
                    {
                        base.MoveControl(movingControl, clientStartLocation, clientEndLocation);
                    }
                }
            }

            private int MoveLeft(int index, int spaceToFree)
            {
                int num = 0;
                base.Row.SuspendLayout();
                try
                {
                    if ((spaceToFree == 0) || (index < 0))
                    {
                        return 0;
                    }
                    for (int i = index; i >= 0; i--)
                    {
                        ToolStripPanelCell nextVisibleCell = (ToolStripPanelCell) base.Row.Cells[i];
                        if (nextVisibleCell.Visible || nextVisibleCell.ControlInDesignMode)
                        {
                            int num3 = spaceToFree - num;
                            Padding margin = nextVisibleCell.Margin;
                            if (margin.Horizontal >= num3)
                            {
                                num += num3;
                                margin.Left -= num3;
                                margin.Right = 0;
                                nextVisibleCell.Margin = margin;
                            }
                            else
                            {
                                num += nextVisibleCell.Margin.Horizontal;
                                margin.Left = 0;
                                margin.Right = 0;
                                nextVisibleCell.Margin = margin;
                            }
                            if (num >= spaceToFree)
                            {
                                if ((index + 1) < base.Row.Cells.Count)
                                {
                                    nextVisibleCell = base.GetNextVisibleCell(index + 1, true);
                                    if (nextVisibleCell != null)
                                    {
                                        margin = nextVisibleCell.Margin;
                                        margin.Left += spaceToFree;
                                        nextVisibleCell.Margin = margin;
                                    }
                                }
                                return spaceToFree;
                            }
                        }
                    }
                }
                finally
                {
                    base.Row.ResumeLayout(true);
                }
                return num;
            }

            private int MoveRight(int index, int spaceToFree)
            {
                int num = 0;
                base.Row.SuspendLayout();
                try
                {
                    ToolStripPanelCell cell;
                    Padding margin;
                    if (((spaceToFree == 0) || (index < 0)) || (index >= base.Row.ControlsInternal.Count))
                    {
                        return 0;
                    }
                    for (int i = index + 1; i < base.Row.Cells.Count; i++)
                    {
                        cell = (ToolStripPanelCell) base.Row.Cells[i];
                        if (cell.Visible || cell.ControlInDesignMode)
                        {
                            int num3 = spaceToFree - num;
                            margin = cell.Margin;
                            if (margin.Horizontal >= num3)
                            {
                                num += num3;
                                margin.Left -= num3;
                                margin.Right = 0;
                                cell.Margin = margin;
                            }
                            else
                            {
                                num += cell.Margin.Horizontal;
                                margin.Left = 0;
                                margin.Right = 0;
                                cell.Margin = margin;
                            }
                            break;
                        }
                    }
                    if ((base.Row.Cells.Count > 0) && (spaceToFree > num))
                    {
                        ToolStripPanelCell nextVisibleCell = base.GetNextVisibleCell(base.Row.Cells.Count - 1, false);
                        if (nextVisibleCell != null)
                        {
                            num += this.DisplayRectangle.Right - nextVisibleCell.Bounds.Right;
                        }
                        else
                        {
                            num += this.DisplayRectangle.Width;
                        }
                    }
                    if (spaceToFree <= num)
                    {
                        cell = base.GetNextVisibleCell(index, true);
                        if (cell == null)
                        {
                            cell = base.Row.Cells[index] as ToolStripPanelCell;
                        }
                        if (cell != null)
                        {
                            margin = cell.Margin;
                            margin.Left += spaceToFree;
                            cell.Margin = margin;
                        }
                        return spaceToFree;
                    }
                    for (int j = index + 1; j < base.Row.Cells.Count; j++)
                    {
                        cell = (ToolStripPanelCell) base.Row.Cells[j];
                        if (cell.Visible || cell.ControlInDesignMode)
                        {
                            int shrinkBy = spaceToFree - num;
                            num += cell.Shrink(shrinkBy);
                            if (spaceToFree >= num)
                            {
                                base.Row.ResumeLayout(true);
                                return spaceToFree;
                            }
                        }
                    }
                    if (base.Row.Cells.Count == 1)
                    {
                        cell = base.GetNextVisibleCell(index, true);
                        if (cell != null)
                        {
                            margin = cell.Margin;
                            margin.Left += num;
                            cell.Margin = margin;
                        }
                    }
                }
                finally
                {
                    base.Row.ResumeLayout(true);
                }
                return num;
            }

            protected internal override void OnBoundsChanged(Rectangle oldBounds, Rectangle newBounds)
            {
                base.OnBoundsChanged(oldBounds, newBounds);
            }

            protected internal override void OnControlAdded(Control control, int index)
            {
            }

            protected internal override void OnControlRemoved(Control control, int index)
            {
            }

            public override Rectangle DisplayRectangle
            {
                get
                {
                    Rectangle displayRectangle = ((IArrangedElement) base.Row).DisplayRectangle;
                    if (base.ToolStripPanel != null)
                    {
                        Rectangle rectangle = base.ToolStripPanel.DisplayRectangle;
                        if ((!base.ToolStripPanel.Visible || LayoutUtils.IsZeroWidthOrHeight(rectangle)) && (base.ToolStripPanel.ParentInternal != null))
                        {
                            displayRectangle.Width = (base.ToolStripPanel.ParentInternal.DisplayRectangle.Width - (base.ToolStripPanel.Margin.Horizontal + base.ToolStripPanel.Padding.Horizontal)) - base.Row.Margin.Horizontal;
                            return displayRectangle;
                        }
                        displayRectangle.Width = rectangle.Width - base.Row.Margin.Horizontal;
                    }
                    return displayRectangle;
                }
            }

            public override Rectangle DragBounds
            {
                get
                {
                    Rectangle bounds = base.Row.Bounds;
                    int index = base.ToolStripPanel.RowsInternal.IndexOf(base.Row);
                    if (index > 0)
                    {
                        Rectangle rectangle2 = base.ToolStripPanel.RowsInternal[index - 1].Bounds;
                        int num2 = (rectangle2.Y + rectangle2.Height) - (rectangle2.Height >> 2);
                        bounds.Height += bounds.Y - num2;
                        bounds.Y = num2;
                    }
                    if (index < (base.ToolStripPanel.RowsInternal.Count - 1))
                    {
                        Rectangle rectangle3 = base.ToolStripPanel.RowsInternal[index + 1].Bounds;
                        bounds.Height += ((rectangle3.Height >> 2) + base.Row.Margin.Bottom) + base.ToolStripPanel.RowsInternal[index + 1].Margin.Top;
                    }
                    bounds.Width += (base.Row.Margin.Horizontal + base.ToolStripPanel.Padding.Horizontal) + 5;
                    bounds.X -= (base.Row.Margin.Left + base.ToolStripPanel.Padding.Left) + 4;
                    return bounds;
                }
            }
        }

        internal class ToolStripPanelRowControlCollection : ArrangedElementCollection, IList, ICollection, IEnumerable
        {
            private ArrangedElementCollection cellCollection;
            private ToolStripPanelRow owner;

            public ToolStripPanelRowControlCollection(ToolStripPanelRow owner)
            {
                this.owner = owner;
            }

            public ToolStripPanelRowControlCollection(ToolStripPanelRow owner, Control[] value)
            {
                this.owner = owner;
                this.AddRange(value);
            }

            [EditorBrowsable(EditorBrowsableState.Never)]
            public int Add(Control value)
            {
                ISupportToolStripPanel controlToBeDragged = value as ISupportToolStripPanel;
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (controlToBeDragged == null)
                {
                    throw new NotSupportedException(System.Windows.Forms.SR.GetString("TypedControlCollectionShouldBeOfType", new object[] { typeof(ToolStrip).Name }));
                }
                int index = base.InnerList.Add(controlToBeDragged.ToolStripPanelCell);
                this.OnAdd(controlToBeDragged, index);
                return index;
            }

            [EditorBrowsable(EditorBrowsableState.Never)]
            public void AddRange(Control[] value)
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                System.Windows.Forms.ToolStripPanel toolStripPanel = this.ToolStripPanel;
                if (toolStripPanel != null)
                {
                    toolStripPanel.SuspendLayout();
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
                    if (toolStripPanel != null)
                    {
                        toolStripPanel.ResumeLayout();
                    }
                }
            }

            public virtual void Clear()
            {
                if (this.owner != null)
                {
                    this.ToolStripPanel.SuspendLayout();
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
                        this.ToolStripPanel.ResumeLayout();
                    }
                }
            }

            public bool Contains(Control value)
            {
                for (int i = 0; i < this.Count; i++)
                {
                    if (this.GetControl(i) == value)
                    {
                        return true;
                    }
                }
                return false;
            }

            [EditorBrowsable(EditorBrowsableState.Never)]
            public void CopyTo(Control[] array, int index)
            {
                if (array == null)
                {
                    throw new ArgumentNullException("array");
                }
                if (index < 0)
                {
                    throw new ArgumentOutOfRangeException("index");
                }
                if ((index >= array.Length) || (base.InnerList.Count > (array.Length - index)))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("ToolStripPanelRowControlCollectionIncorrectIndexLength"));
                }
                for (int i = 0; i < base.InnerList.Count; i++)
                {
                    array[index++] = this.GetControl(i);
                }
            }

            private Control GetControl(int index)
            {
                Control control = null;
                ToolStripPanelCell cell = null;
                if ((index < this.Count) && (index >= 0))
                {
                    cell = (ToolStripPanelCell) base.InnerList[index];
                    control = (cell != null) ? cell.Control : null;
                }
                return control;
            }

            public override IEnumerator GetEnumerator()
            {
                return new ToolStripPanelCellToControlEnumerator(base.InnerList);
            }

            public int IndexOf(Control value)
            {
                for (int i = 0; i < this.Count; i++)
                {
                    if (this.GetControl(i) == value)
                    {
                        return i;
                    }
                }
                return -1;
            }

            private int IndexOfControl(Control c)
            {
                for (int i = 0; i < this.Count; i++)
                {
                    ToolStripPanelCell cell = (ToolStripPanelCell) base.InnerList[i];
                    if (cell.Control == c)
                    {
                        return i;
                    }
                }
                return -1;
            }

            [EditorBrowsable(EditorBrowsableState.Never)]
            public void Insert(int index, Control value)
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                ISupportToolStripPanel controlToBeDragged = value as ISupportToolStripPanel;
                if (controlToBeDragged == null)
                {
                    throw new NotSupportedException(System.Windows.Forms.SR.GetString("TypedControlCollectionShouldBeOfType", new object[] { typeof(ToolStrip).Name }));
                }
                base.InnerList.Insert(index, controlToBeDragged.ToolStripPanelCell);
                this.OnAdd(controlToBeDragged, index);
            }

            private void OnAdd(ISupportToolStripPanel controlToBeDragged, int index)
            {
                if (this.owner != null)
                {
                    LayoutTransaction transaction = null;
                    if ((this.ToolStripPanel != null) && (this.ToolStripPanel.ParentInternal != null))
                    {
                        transaction = new LayoutTransaction(this.ToolStripPanel, this.ToolStripPanel.ParentInternal, PropertyNames.Parent);
                    }
                    try
                    {
                        if (controlToBeDragged != null)
                        {
                            controlToBeDragged.ToolStripPanelRow = this.owner;
                            Control control = controlToBeDragged as Control;
                            if (control != null)
                            {
                                control.ParentInternal = this.owner.ToolStripPanel;
                                this.owner.OnControlAdded(control, index);
                            }
                        }
                    }
                    finally
                    {
                        if (transaction != null)
                        {
                            transaction.Dispose();
                        }
                    }
                }
            }

            private void OnAfterRemove(Control control, int index)
            {
                if (this.owner != null)
                {
                    using (new LayoutTransaction(this.ToolStripPanel, control, PropertyNames.Parent))
                    {
                        this.owner.ToolStripPanel.Controls.Remove(control);
                        this.owner.OnControlRemoved(control, index);
                    }
                }
            }

            [EditorBrowsable(EditorBrowsableState.Never)]
            public void Remove(Control value)
            {
                int index = this.IndexOfControl(value);
                this.RemoveAt(index);
            }

            [EditorBrowsable(EditorBrowsableState.Never)]
            public void RemoveAt(int index)
            {
                if ((index >= 0) && (index < this.Count))
                {
                    Control control = this.GetControl(index);
                    object obj1 = base.InnerList[index];
                    base.InnerList.RemoveAt(index);
                    this.OnAfterRemove(control, index);
                }
            }

            int IList.Add(object value)
            {
                return this.Add(value as Control);
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
                return this.IndexOf(value as Control);
            }

            void IList.Insert(int index, object value)
            {
                this.Insert(index, value as Control);
            }

            void IList.Remove(object value)
            {
                this.Remove(value as Control);
            }

            void IList.RemoveAt(int index)
            {
                this.RemoveAt(index);
            }

            public ArrangedElementCollection Cells
            {
                get
                {
                    if (this.cellCollection == null)
                    {
                        this.cellCollection = new ArrangedElementCollection(base.InnerList);
                    }
                    return this.cellCollection;
                }
            }

            public virtual Control this[int index]
            {
                get
                {
                    return this.GetControl(index);
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

            public System.Windows.Forms.ToolStripPanel ToolStripPanel
            {
                get
                {
                    return this.owner.ToolStripPanel;
                }
            }

            private class ToolStripPanelCellToControlEnumerator : IEnumerator, ICloneable
            {
                private IEnumerator arrayListEnumerator;

                internal ToolStripPanelCellToControlEnumerator(ArrayList list)
                {
                    this.arrayListEnumerator = list.GetEnumerator();
                }

                public object Clone()
                {
                    return base.MemberwiseClone();
                }

                public virtual bool MoveNext()
                {
                    return this.arrayListEnumerator.MoveNext();
                }

                public virtual void Reset()
                {
                    this.arrayListEnumerator.Reset();
                }

                public virtual object Current
                {
                    get
                    {
                        ToolStripPanelCell current = this.arrayListEnumerator.Current as ToolStripPanelCell;
                        if (current != null)
                        {
                            return current.Control;
                        }
                        return null;
                    }
                }
            }
        }

        private abstract class ToolStripPanelRowManager
        {
            private System.Windows.Forms.FlowLayoutSettings flowLayoutSettings;
            private ToolStripPanelRow owner;

            public ToolStripPanelRowManager(ToolStripPanelRow owner)
            {
                this.owner = owner;
            }

            public virtual bool CanMove(ToolStrip toolStripToDrag)
            {
                ISupportToolStripPanel panel = toolStripToDrag;
                if ((panel != null) && panel.Stretch)
                {
                    return false;
                }
                foreach (Control control in this.Row.ControlsInternal)
                {
                    panel = control as ISupportToolStripPanel;
                    if ((panel != null) && panel.Stretch)
                    {
                        return false;
                    }
                }
                return true;
            }

            protected internal virtual int FreeSpaceFromRow(int spaceToFree)
            {
                return 0;
            }

            public ToolStripPanelCell GetNextVisibleCell(int index, bool forward)
            {
                if (forward)
                {
                    for (int i = index; i < this.Row.Cells.Count; i++)
                    {
                        ToolStripPanelCell cell = this.Row.Cells[i] as ToolStripPanelCell;
                        if ((cell.Visible || (this.owner.parent.Visible && cell.ControlInDesignMode)) && (cell.ToolStripPanelRow == this.owner))
                        {
                            return cell;
                        }
                    }
                }
                else
                {
                    for (int j = index; j >= 0; j--)
                    {
                        ToolStripPanelCell cell2 = this.Row.Cells[j] as ToolStripPanelCell;
                        if ((cell2.Visible || (this.owner.parent.Visible && cell2.ControlInDesignMode)) && (cell2.ToolStripPanelRow == this.owner))
                        {
                            return cell2;
                        }
                    }
                }
                return null;
            }

            protected virtual int Grow(int index, int growBy)
            {
                int num = 0;
                if ((index >= 0) && (index < (this.Row.ControlsInternal.Count - 1)))
                {
                    ToolStripPanelCell cell = (ToolStripPanelCell) this.Row.Cells[index];
                    if (cell.Visible)
                    {
                        num = cell.Grow(growBy);
                    }
                }
                return num;
            }

            protected virtual int GrowControlsAfter(int index, int growBy)
            {
                if (growBy < 0)
                {
                    return 0;
                }
                int num = growBy;
                for (int i = index + 1; i < this.Row.ControlsInternal.Count; i++)
                {
                    int num3 = this.Grow(i, num);
                    if (num3 >= 0)
                    {
                        num -= num3;
                        if (num <= 0)
                        {
                            return growBy;
                        }
                    }
                }
                return (growBy - num);
            }

            protected virtual int GrowControlsBefore(int index, int growBy)
            {
                if (growBy < 0)
                {
                    return 0;
                }
                int num = growBy;
                for (int i = index - 1; i >= 0; i--)
                {
                    num -= this.Grow(i, num);
                    if (num <= 0)
                    {
                        return growBy;
                    }
                }
                return (growBy - num);
            }

            public virtual void JoinRow(ToolStrip toolStripToDrag, Point locationToDrag)
            {
            }

            public virtual void LeaveRow(ToolStrip toolStripToDrag)
            {
            }

            public virtual void MoveControl(ToolStrip movingControl, Point startClientLocation, Point endClientLocation)
            {
            }

            protected internal virtual void OnBoundsChanged(Rectangle oldBounds, Rectangle newBounds)
            {
            }

            protected internal virtual void OnControlAdded(Control c, int index)
            {
            }

            protected internal virtual void OnControlRemoved(Control c, int index)
            {
            }

            public virtual Rectangle DisplayRectangle
            {
                get
                {
                    return Rectangle.Empty;
                }
            }

            public virtual Rectangle DragBounds
            {
                get
                {
                    return Rectangle.Empty;
                }
            }

            public System.Windows.Forms.FlowLayoutSettings FlowLayoutSettings
            {
                get
                {
                    if (this.flowLayoutSettings == null)
                    {
                        this.flowLayoutSettings = new System.Windows.Forms.FlowLayoutSettings(this.owner);
                    }
                    return this.flowLayoutSettings;
                }
            }

            public ToolStripPanelRow Row
            {
                get
                {
                    return this.owner;
                }
            }

            public System.Windows.Forms.ToolStripPanel ToolStripPanel
            {
                get
                {
                    return this.owner.ToolStripPanel;
                }
            }
        }

        private class VerticalRowManager : ToolStripPanelRow.ToolStripPanelRowManager
        {
            private const int DRAG_BOUNDS_INFLATE = 4;

            public VerticalRowManager(ToolStripPanelRow owner) : base(owner)
            {
                owner.SuspendLayout();
                base.FlowLayoutSettings.WrapContents = false;
                base.FlowLayoutSettings.FlowDirection = FlowDirection.TopDown;
                owner.ResumeLayout(false);
            }

            public override bool CanMove(ToolStrip toolStripToDrag)
            {
                if (!base.CanMove(toolStripToDrag))
                {
                    return false;
                }
                Size empty = Size.Empty;
                for (int i = 0; i < base.Row.ControlsInternal.Count; i++)
                {
                    empty += base.Row.GetMinimumSize(base.Row.ControlsInternal[i] as ToolStrip);
                }
                empty += base.Row.GetMinimumSize(toolStripToDrag);
                return (empty.Height < this.DisplayRectangle.Height);
            }

            protected internal override int FreeSpaceFromRow(int spaceToFree)
            {
                int num = spaceToFree;
                if (spaceToFree > 0)
                {
                    ToolStripPanelCell nextVisibleCell = base.GetNextVisibleCell(base.Row.Cells.Count - 1, false);
                    if (nextVisibleCell == null)
                    {
                        return 0;
                    }
                    Padding margin = nextVisibleCell.Margin;
                    if (margin.Top >= spaceToFree)
                    {
                        margin.Top -= spaceToFree;
                        margin.Bottom = 0;
                        spaceToFree = 0;
                    }
                    else
                    {
                        spaceToFree -= nextVisibleCell.Margin.Top;
                        margin.Top = 0;
                        margin.Bottom = 0;
                    }
                    nextVisibleCell.Margin = margin;
                    spaceToFree -= this.MoveUp(base.Row.Cells.Count - 1, spaceToFree);
                    if (spaceToFree > 0)
                    {
                        spaceToFree -= nextVisibleCell.Shrink(spaceToFree);
                    }
                }
                return (num - Math.Max(0, spaceToFree));
            }

            public override void JoinRow(ToolStrip toolStripToDrag, Point locationToDrag)
            {
                if (!base.Row.ControlsInternal.Contains(toolStripToDrag))
                {
                    base.Row.SuspendLayout();
                    try
                    {
                        if (base.Row.ControlsInternal.Count > 0)
                        {
                            int index = 0;
                            while (index < base.Row.Cells.Count)
                            {
                                ToolStripPanelCell cell = base.Row.Cells[index] as ToolStripPanelCell;
                                if ((cell.Visible || cell.ControlInDesignMode) && (cell.Bounds.Contains(locationToDrag) || (cell.Bounds.Y >= locationToDrag.Y)))
                                {
                                    break;
                                }
                                index++;
                            }
                            Control control1 = base.Row.ControlsInternal[index];
                            if (index < base.Row.ControlsInternal.Count)
                            {
                                base.Row.ControlsInternal.Insert(index, toolStripToDrag);
                            }
                            else
                            {
                                base.Row.ControlsInternal.Add(toolStripToDrag);
                            }
                            int num2 = toolStripToDrag.AutoSize ? toolStripToDrag.PreferredSize.Height : toolStripToDrag.Height;
                            int num3 = num2;
                            if (index == 0)
                            {
                                num3 += locationToDrag.Y;
                            }
                            int num4 = 0;
                            if (index < (base.Row.ControlsInternal.Count - 1))
                            {
                                ToolStripPanelCell nextVisibleCell = base.GetNextVisibleCell(index + 1, true);
                                if (nextVisibleCell != null)
                                {
                                    Padding margin = nextVisibleCell.Margin;
                                    if (margin.Top > num3)
                                    {
                                        margin.Top -= num3;
                                        nextVisibleCell.Margin = margin;
                                        num4 = num3;
                                    }
                                    else
                                    {
                                        num4 = this.MoveDown(index + 1, num3 - num4);
                                        if (num4 > 0)
                                        {
                                            margin = nextVisibleCell.Margin;
                                            margin.Top -= num4;
                                            nextVisibleCell.Margin = margin;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                ToolStripPanelCell cell3 = base.GetNextVisibleCell(base.Row.Cells.Count - 2, false);
                                ToolStripPanelCell cell4 = base.GetNextVisibleCell(base.Row.Cells.Count - 1, false);
                                if ((cell3 != null) && (cell4 != null))
                                {
                                    Padding padding2 = cell4.Margin;
                                    padding2.Top = Math.Max(0, locationToDrag.Y - cell3.Bounds.Bottom);
                                    cell4.Margin = padding2;
                                    num4 = num3;
                                }
                            }
                            if ((num4 < num3) && (index > 0))
                            {
                                num4 = this.MoveUp(index - 1, num3 - num4);
                            }
                            if ((index == 0) && ((num4 - num2) > 0))
                            {
                                ToolStripPanelCell cell5 = base.Row.Cells[index] as ToolStripPanelCell;
                                Padding padding3 = cell5.Margin;
                                padding3.Top = num4 - num2;
                                cell5.Margin = padding3;
                            }
                        }
                        else
                        {
                            base.Row.ControlsInternal.Add(toolStripToDrag);
                            if (base.Row.Cells.Count > 0)
                            {
                                ToolStripPanelCell cell6 = base.GetNextVisibleCell(base.Row.Cells.Count - 1, false);
                                if (cell6 != null)
                                {
                                    Padding padding4 = cell6.Margin;
                                    padding4.Top = Math.Max(0, locationToDrag.Y - base.Row.Margin.Top);
                                    cell6.Margin = padding4;
                                }
                            }
                        }
                    }
                    finally
                    {
                        base.Row.ResumeLayout(true);
                    }
                }
            }

            public override void LeaveRow(ToolStrip toolStripToDrag)
            {
                base.Row.SuspendLayout();
                int index = base.Row.ControlsInternal.IndexOf(toolStripToDrag);
                if (index >= 0)
                {
                    if (index < (base.Row.ControlsInternal.Count - 1))
                    {
                        ToolStripPanelCell cell = (ToolStripPanelCell) base.Row.Cells[index];
                        if (cell.Visible)
                        {
                            int num2 = cell.Margin.Vertical + cell.Bounds.Height;
                            ToolStripPanelCell nextVisibleCell = base.GetNextVisibleCell(index + 1, true);
                            if (nextVisibleCell != null)
                            {
                                Padding margin = nextVisibleCell.Margin;
                                margin.Top += num2;
                                nextVisibleCell.Margin = margin;
                            }
                        }
                    }
                    ((IList) base.Row.Cells).RemoveAt(index);
                }
                base.Row.ResumeLayout(true);
            }

            public override void MoveControl(ToolStrip movingControl, Point clientStartLocation, Point clientEndLocation)
            {
                if (!base.Row.Locked)
                {
                    if (this.DragBounds.Contains(clientEndLocation))
                    {
                        int index = base.Row.ControlsInternal.IndexOf(movingControl);
                        int spaceToFree = clientEndLocation.Y - clientStartLocation.Y;
                        if (spaceToFree < 0)
                        {
                            this.MoveUp(index, spaceToFree * -1);
                        }
                        else
                        {
                            this.MoveDown(index, spaceToFree);
                        }
                    }
                    else
                    {
                        base.MoveControl(movingControl, clientStartLocation, clientEndLocation);
                    }
                }
            }

            private int MoveDown(int index, int spaceToFree)
            {
                int num = 0;
                base.Row.SuspendLayout();
                try
                {
                    ToolStripPanelCell cell;
                    Padding margin;
                    if (((spaceToFree == 0) || (index < 0)) || (index >= base.Row.ControlsInternal.Count))
                    {
                        return 0;
                    }
                    for (int i = index + 1; i < base.Row.Cells.Count; i++)
                    {
                        cell = (ToolStripPanelCell) base.Row.Cells[i];
                        if (cell.Visible || cell.ControlInDesignMode)
                        {
                            int num3 = spaceToFree - num;
                            margin = cell.Margin;
                            if (margin.Vertical >= num3)
                            {
                                num += num3;
                                margin.Top -= num3;
                                margin.Bottom = 0;
                                cell.Margin = margin;
                            }
                            else
                            {
                                num += cell.Margin.Vertical;
                                margin.Top = 0;
                                margin.Bottom = 0;
                                cell.Margin = margin;
                            }
                            break;
                        }
                    }
                    if ((base.Row.Cells.Count > 0) && (spaceToFree > num))
                    {
                        ToolStripPanelCell nextVisibleCell = base.GetNextVisibleCell(base.Row.Cells.Count - 1, false);
                        if (nextVisibleCell != null)
                        {
                            num += this.DisplayRectangle.Bottom - nextVisibleCell.Bounds.Bottom;
                        }
                        else
                        {
                            num += this.DisplayRectangle.Height;
                        }
                    }
                    if (spaceToFree <= num)
                    {
                        cell = (ToolStripPanelCell) base.Row.Cells[index];
                        margin = cell.Margin;
                        margin.Top += spaceToFree;
                        cell.Margin = margin;
                        return spaceToFree;
                    }
                    for (int j = index + 1; j < base.Row.Cells.Count; j++)
                    {
                        cell = (ToolStripPanelCell) base.Row.Cells[j];
                        if (cell.Visible || cell.ControlInDesignMode)
                        {
                            int shrinkBy = spaceToFree - num;
                            num += cell.Shrink(shrinkBy);
                            if (spaceToFree >= num)
                            {
                                base.Row.ResumeLayout(true);
                                return spaceToFree;
                            }
                        }
                    }
                    if (base.Row.Cells.Count == 1)
                    {
                        cell = base.GetNextVisibleCell(index, true);
                        if (cell != null)
                        {
                            margin = cell.Margin;
                            margin.Top += num;
                            cell.Margin = margin;
                        }
                    }
                }
                finally
                {
                    base.Row.ResumeLayout(true);
                }
                return (spaceToFree - num);
            }

            private int MoveUp(int index, int spaceToFree)
            {
                int num = 0;
                base.Row.SuspendLayout();
                try
                {
                    if ((spaceToFree == 0) || (index < 0))
                    {
                        return 0;
                    }
                    for (int i = index; i >= 0; i--)
                    {
                        ToolStripPanelCell nextVisibleCell = (ToolStripPanelCell) base.Row.Cells[i];
                        if (nextVisibleCell.Visible || nextVisibleCell.ControlInDesignMode)
                        {
                            int num3 = spaceToFree - num;
                            Padding margin = nextVisibleCell.Margin;
                            if (margin.Vertical >= num3)
                            {
                                num += num3;
                                margin.Top -= num3;
                                margin.Bottom = 0;
                                nextVisibleCell.Margin = margin;
                            }
                            else
                            {
                                num += nextVisibleCell.Margin.Vertical;
                                margin.Top = 0;
                                margin.Bottom = 0;
                                nextVisibleCell.Margin = margin;
                            }
                            if (num >= spaceToFree)
                            {
                                if ((index + 1) < base.Row.Cells.Count)
                                {
                                    nextVisibleCell = base.GetNextVisibleCell(index + 1, true);
                                    if (nextVisibleCell != null)
                                    {
                                        margin = nextVisibleCell.Margin;
                                        margin.Top += spaceToFree;
                                        nextVisibleCell.Margin = margin;
                                    }
                                }
                                return spaceToFree;
                            }
                        }
                    }
                }
                finally
                {
                    base.Row.ResumeLayout(true);
                }
                return num;
            }

            protected internal override void OnBoundsChanged(Rectangle oldBounds, Rectangle newBounds)
            {
                base.OnBoundsChanged(oldBounds, newBounds);
                if (base.Row.Cells.Count > 0)
                {
                    ToolStripPanelCell nextVisibleCell = base.GetNextVisibleCell(base.Row.Cells.Count - 1, false);
                    int shrinkBy = (nextVisibleCell != null) ? (nextVisibleCell.Bounds.Bottom - newBounds.Height) : 0;
                    if (shrinkBy > 0)
                    {
                        ToolStripPanelCell cell2 = base.GetNextVisibleCell(base.Row.Cells.Count - 1, false);
                        Padding margin = cell2.Margin;
                        if (margin.Top >= shrinkBy)
                        {
                            margin.Top -= shrinkBy;
                            margin.Bottom = 0;
                            cell2.Margin = margin;
                            shrinkBy = 0;
                        }
                        else
                        {
                            shrinkBy -= cell2.Margin.Top;
                            margin.Top = 0;
                            margin.Bottom = 0;
                            cell2.Margin = margin;
                        }
                        shrinkBy -= cell2.Shrink(shrinkBy);
                        this.MoveUp(base.Row.Cells.Count - 1, shrinkBy);
                    }
                }
            }

            protected internal override void OnControlAdded(Control control, int index)
            {
            }

            protected internal override void OnControlRemoved(Control c, int index)
            {
            }

            public override Rectangle DisplayRectangle
            {
                get
                {
                    Rectangle displayRectangle = ((IArrangedElement) base.Row).DisplayRectangle;
                    if (base.ToolStripPanel != null)
                    {
                        Rectangle rectangle = base.ToolStripPanel.DisplayRectangle;
                        if ((!base.ToolStripPanel.Visible || LayoutUtils.IsZeroWidthOrHeight(rectangle)) && (base.ToolStripPanel.ParentInternal != null))
                        {
                            displayRectangle.Height = (base.ToolStripPanel.ParentInternal.DisplayRectangle.Height - (base.ToolStripPanel.Margin.Vertical + base.ToolStripPanel.Padding.Vertical)) - base.Row.Margin.Vertical;
                            return displayRectangle;
                        }
                        displayRectangle.Height = rectangle.Height - base.Row.Margin.Vertical;
                    }
                    return displayRectangle;
                }
            }

            public override Rectangle DragBounds
            {
                get
                {
                    Rectangle bounds = base.Row.Bounds;
                    int index = base.ToolStripPanel.RowsInternal.IndexOf(base.Row);
                    if (index > 0)
                    {
                        Rectangle rectangle2 = base.ToolStripPanel.RowsInternal[index - 1].Bounds;
                        int num2 = (rectangle2.X + rectangle2.Width) - (rectangle2.Width >> 2);
                        bounds.Width += bounds.X - num2;
                        bounds.X = num2;
                    }
                    if (index < (base.ToolStripPanel.RowsInternal.Count - 1))
                    {
                        Rectangle rectangle3 = base.ToolStripPanel.RowsInternal[index + 1].Bounds;
                        bounds.Width += ((rectangle3.Width >> 2) + base.Row.Margin.Right) + base.ToolStripPanel.RowsInternal[index + 1].Margin.Left;
                    }
                    bounds.Height += (base.Row.Margin.Vertical + base.ToolStripPanel.Padding.Vertical) + 5;
                    bounds.Y -= (base.Row.Margin.Top + base.ToolStripPanel.Padding.Top) + 4;
                    return bounds;
                }
            }
        }
    }
}

