namespace System.Windows.Forms
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Windows.Forms.Layout;

    internal class ToolStripSplitStackLayout : LayoutEngine
    {
        private int backwardsWalkingIndex;
        internal static readonly TraceSwitch DebugLayoutTraceSwitch;
        private Rectangle displayRectangle = Rectangle.Empty;
        private int forwardsWalkingIndex;
        private Point noMansLand;
        private bool overflowRequired;
        private int overflowSpace;
        private System.Windows.Forms.ToolStrip toolStrip;

        internal ToolStripSplitStackLayout(System.Windows.Forms.ToolStrip owner)
        {
            this.toolStrip = owner;
        }

        private void CalculatePlacementsHorizontal()
        {
            this.ResetItemPlacements();
            System.Windows.Forms.ToolStrip toolStrip = this.ToolStrip;
            int num = 0;
            if (this.ToolStrip.CanOverflow)
            {
                this.ForwardsWalkingIndex = 0;
                while (this.ForwardsWalkingIndex < toolStrip.Items.Count)
                {
                    ToolStripItem item = toolStrip.Items[this.ForwardsWalkingIndex];
                    if (((IArrangedElement) item).ParticipatesInLayout)
                    {
                        if (item.Overflow == ToolStripItemOverflow.Always)
                        {
                            this.OverflowRequired = true;
                        }
                        if ((item.Overflow != ToolStripItemOverflow.Always) && (item.Placement == ToolStripItemPlacement.None))
                        {
                            Size size = item.AutoSize ? item.GetPreferredSize(this.displayRectangle.Size) : item.Size;
                            num += size.Width + item.Margin.Horizontal;
                            int num2 = this.OverflowRequired ? this.OverflowButtonSize.Width : 0;
                            if (num > (this.displayRectangle.Width - num2))
                            {
                                int num3 = this.SendNextItemToOverflow((num + num2) - this.displayRectangle.Width, true);
                                num -= num3;
                            }
                        }
                    }
                    this.ForwardsWalkingIndex++;
                }
            }
            this.PlaceItems();
        }

        private void CalculatePlacementsVertical()
        {
            this.ResetItemPlacements();
            System.Windows.Forms.ToolStrip toolStrip = this.ToolStrip;
            int num = 0;
            if (this.ToolStrip.CanOverflow)
            {
                this.ForwardsWalkingIndex = 0;
                while (this.ForwardsWalkingIndex < this.ToolStrip.Items.Count)
                {
                    ToolStripItem item = toolStrip.Items[this.ForwardsWalkingIndex];
                    if (((IArrangedElement) item).ParticipatesInLayout)
                    {
                        if (item.Overflow == ToolStripItemOverflow.Always)
                        {
                            this.OverflowRequired = true;
                        }
                        if ((item.Overflow != ToolStripItemOverflow.Always) && (item.Placement == ToolStripItemPlacement.None))
                        {
                            Size size = item.AutoSize ? item.GetPreferredSize(this.displayRectangle.Size) : item.Size;
                            int num2 = this.OverflowRequired ? this.OverflowButtonSize.Height : 0;
                            num += size.Height + item.Margin.Vertical;
                            if (num > (this.displayRectangle.Height - num2))
                            {
                                int num3 = this.SendNextItemToOverflow(num - this.displayRectangle.Height, false);
                                num -= num3;
                            }
                        }
                    }
                    this.ForwardsWalkingIndex++;
                }
            }
            this.PlaceItems();
        }

        internal override Size GetPreferredSize(IArrangedElement container, Size proposedConstraints)
        {
            if (!(container is System.Windows.Forms.ToolStrip))
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("ToolStripSplitStackLayoutContainerMustBeAToolStrip"));
            }
            if (this.toolStrip.LayoutStyle == ToolStripLayoutStyle.HorizontalStackWithOverflow)
            {
                return System.Windows.Forms.ToolStrip.GetPreferredSizeHorizontal(container, proposedConstraints);
            }
            return System.Windows.Forms.ToolStrip.GetPreferredSizeVertical(container, proposedConstraints);
        }

        private void InvalidateLayout()
        {
            this.forwardsWalkingIndex = 0;
            this.backwardsWalkingIndex = -1;
            this.overflowSpace = 0;
            this.overflowRequired = false;
            this.displayRectangle = Rectangle.Empty;
        }

        internal override bool LayoutCore(IArrangedElement container, LayoutEventArgs layoutEventArgs)
        {
            if (!(container is System.Windows.Forms.ToolStrip))
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("ToolStripSplitStackLayoutContainerMustBeAToolStrip"));
            }
            this.InvalidateLayout();
            this.displayRectangle = this.toolStrip.DisplayRectangle;
            this.noMansLand = this.displayRectangle.Location;
            this.noMansLand.X += this.toolStrip.ClientSize.Width + 1;
            this.noMansLand.Y += this.toolStrip.ClientSize.Height + 1;
            if (this.toolStrip.LayoutStyle == ToolStripLayoutStyle.HorizontalStackWithOverflow)
            {
                this.LayoutHorizontal();
            }
            else
            {
                this.LayoutVertical();
            }
            return CommonProperties.GetAutoSize(container);
        }

        private bool LayoutHorizontal()
        {
            System.Windows.Forms.ToolStrip toolStrip = this.ToolStrip;
            Rectangle clientRectangle = toolStrip.ClientRectangle;
            int right = this.displayRectangle.Right;
            int left = this.displayRectangle.Left;
            Size empty = Size.Empty;
            Rectangle a = Rectangle.Empty;
            Rectangle rectangle3 = Rectangle.Empty;
            this.CalculatePlacementsHorizontal();
            bool flag2 = toolStrip.CanOverflow && (this.OverflowRequired || (this.OverflowSpace >= this.OverflowButtonSize.Width));
            toolStrip.OverflowButton.Visible = flag2;
            if (flag2)
            {
                if (toolStrip.RightToLeft == RightToLeft.No)
                {
                    right = clientRectangle.Right;
                }
                else
                {
                    left = clientRectangle.Left;
                }
            }
            for (int i = -1; i < toolStrip.Items.Count; i++)
            {
                ToolStripItem overflowButton = null;
                if (i == -1)
                {
                    if (flag2)
                    {
                        overflowButton = toolStrip.OverflowButton;
                        overflowButton.SetPlacement(ToolStripItemPlacement.Main);
                        empty = this.OverflowButtonSize;
                        goto Label_011F;
                    }
                    toolStrip.OverflowButton.SetPlacement(ToolStripItemPlacement.None);
                    continue;
                }
                overflowButton = toolStrip.Items[i];
                if (!((IArrangedElement) overflowButton).ParticipatesInLayout)
                {
                    continue;
                }
                empty = overflowButton.AutoSize ? overflowButton.GetPreferredSize(Size.Empty) : overflowButton.Size;
            Label_011F:
                if ((!flag2 && (overflowButton.Overflow == ToolStripItemOverflow.AsNeeded)) && (overflowButton.Placement == ToolStripItemPlacement.Overflow))
                {
                    overflowButton.SetPlacement(ToolStripItemPlacement.Main);
                }
                if ((overflowButton != null) && (overflowButton.Placement == ToolStripItemPlacement.Main))
                {
                    int x = this.displayRectangle.Left;
                    int top = this.displayRectangle.Top;
                    Padding margin = overflowButton.Margin;
                    if (((overflowButton.Alignment == ToolStripItemAlignment.Right) && (toolStrip.RightToLeft == RightToLeft.No)) || ((overflowButton.Alignment == ToolStripItemAlignment.Left) && (toolStrip.RightToLeft == RightToLeft.Yes)))
                    {
                        x = right - (margin.Right + empty.Width);
                        top += margin.Top;
                        right = x - margin.Left;
                        rectangle3 = (rectangle3 == Rectangle.Empty) ? new Rectangle(x, top, empty.Width, empty.Height) : Rectangle.Union(rectangle3, new Rectangle(x, top, empty.Width, empty.Height));
                    }
                    else
                    {
                        x = left + margin.Left;
                        top += margin.Top;
                        left = (x + empty.Width) + margin.Right;
                        a = (a == Rectangle.Empty) ? new Rectangle(x, top, empty.Width, empty.Height) : Rectangle.Union(a, new Rectangle(x, top, empty.Width, empty.Height));
                    }
                    overflowButton.ParentInternal = this.ToolStrip;
                    Point itemLocation = new Point(x, top);
                    if (!clientRectangle.Contains(x, top))
                    {
                        overflowButton.SetPlacement(ToolStripItemPlacement.None);
                    }
                    else if (((rectangle3.Width > 0) && (a.Width > 0)) && rectangle3.IntersectsWith(a))
                    {
                        itemLocation = this.noMansLand;
                        overflowButton.SetPlacement(ToolStripItemPlacement.None);
                    }
                    if (overflowButton.AutoSize)
                    {
                        empty.Height = Math.Max(this.displayRectangle.Height - margin.Vertical, 0);
                    }
                    else
                    {
                        Rectangle rectangle4 = LayoutUtils.VAlign(overflowButton.Size, this.displayRectangle, AnchorStyles.None);
                        itemLocation.Y = rectangle4.Y;
                    }
                    this.SetItemLocation(overflowButton, itemLocation, empty);
                }
                else
                {
                    overflowButton.ParentInternal = (overflowButton.Placement == ToolStripItemPlacement.Overflow) ? toolStrip.OverflowButton.DropDown : null;
                }
            }
            return false;
        }

        private bool LayoutVertical()
        {
            System.Windows.Forms.ToolStrip toolStrip = this.ToolStrip;
            Rectangle clientRectangle = toolStrip.ClientRectangle;
            int bottom = this.displayRectangle.Bottom;
            int top = this.displayRectangle.Top;
            Size empty = Size.Empty;
            Rectangle a = Rectangle.Empty;
            Rectangle rectangle3 = Rectangle.Empty;
            Size size = this.displayRectangle.Size;
            DockStyle dock = toolStrip.Dock;
            if (toolStrip.AutoSize && ((!toolStrip.IsInToolStripPanel && (dock == DockStyle.Left)) || (dock == DockStyle.Right)))
            {
                size = System.Windows.Forms.ToolStrip.GetPreferredSizeVertical(toolStrip, Size.Empty) - toolStrip.Padding.Size;
            }
            this.CalculatePlacementsVertical();
            bool flag2 = toolStrip.CanOverflow && (this.OverflowRequired || (this.OverflowSpace >= this.OverflowButtonSize.Height));
            toolStrip.OverflowButton.Visible = flag2;
            for (int i = -1; i < this.ToolStrip.Items.Count; i++)
            {
                ToolStripItem overflowButton = null;
                if (i == -1)
                {
                    if (flag2)
                    {
                        overflowButton = toolStrip.OverflowButton;
                        overflowButton.SetPlacement(ToolStripItemPlacement.Main);
                    }
                    else
                    {
                        toolStrip.OverflowButton.SetPlacement(ToolStripItemPlacement.None);
                        continue;
                    }
                    empty = this.OverflowButtonSize;
                }
                else
                {
                    overflowButton = toolStrip.Items[i];
                    if (!((IArrangedElement) overflowButton).ParticipatesInLayout)
                    {
                        continue;
                    }
                    empty = overflowButton.AutoSize ? overflowButton.GetPreferredSize(Size.Empty) : overflowButton.Size;
                }
                if ((!flag2 && (overflowButton.Overflow == ToolStripItemOverflow.AsNeeded)) && (overflowButton.Placement == ToolStripItemPlacement.Overflow))
                {
                    overflowButton.SetPlacement(ToolStripItemPlacement.Main);
                }
                if ((overflowButton != null) && (overflowButton.Placement == ToolStripItemPlacement.Main))
                {
                    Padding margin = overflowButton.Margin;
                    int x = this.displayRectangle.Left + margin.Left;
                    int y = this.displayRectangle.Top;
                    switch (overflowButton.Alignment)
                    {
                        case ToolStripItemAlignment.Right:
                            y = bottom - (margin.Bottom + empty.Height);
                            bottom = y - margin.Top;
                            rectangle3 = (rectangle3 == Rectangle.Empty) ? new Rectangle(x, y, empty.Width, empty.Height) : Rectangle.Union(rectangle3, new Rectangle(x, y, empty.Width, empty.Height));
                            break;

                        default:
                            y = top + margin.Top;
                            top = (y + empty.Height) + margin.Bottom;
                            a = (a == Rectangle.Empty) ? new Rectangle(x, y, empty.Width, empty.Height) : Rectangle.Union(a, new Rectangle(x, y, empty.Width, empty.Height));
                            break;
                    }
                    overflowButton.ParentInternal = this.ToolStrip;
                    Point itemLocation = new Point(x, y);
                    if (!clientRectangle.Contains(x, y))
                    {
                        overflowButton.SetPlacement(ToolStripItemPlacement.None);
                    }
                    else if (((rectangle3.Width > 0) && (a.Width > 0)) && rectangle3.IntersectsWith(a))
                    {
                        itemLocation = this.noMansLand;
                        overflowButton.SetPlacement(ToolStripItemPlacement.None);
                    }
                    if (overflowButton.AutoSize)
                    {
                        empty.Width = Math.Max((size.Width - margin.Horizontal) - 1, 0);
                    }
                    else
                    {
                        Rectangle rectangle4 = LayoutUtils.HAlign(overflowButton.Size, this.displayRectangle, AnchorStyles.None);
                        itemLocation.X = rectangle4.X;
                    }
                    this.SetItemLocation(overflowButton, itemLocation, empty);
                    continue;
                }
                overflowButton.ParentInternal = (overflowButton.Placement == ToolStripItemPlacement.Overflow) ? toolStrip.OverflowButton.DropDown : null;
            }
            return false;
        }

        private void PlaceItems()
        {
            System.Windows.Forms.ToolStrip toolStrip = this.ToolStrip;
            for (int i = 0; i < toolStrip.Items.Count; i++)
            {
                ToolStripItem item = toolStrip.Items[i];
                if (item.Placement == ToolStripItemPlacement.None)
                {
                    if (item.Overflow != ToolStripItemOverflow.Always)
                    {
                        item.SetPlacement(ToolStripItemPlacement.Main);
                    }
                    else
                    {
                        item.SetPlacement(ToolStripItemPlacement.Overflow);
                    }
                }
            }
        }

        private void ResetItemPlacements()
        {
            System.Windows.Forms.ToolStrip toolStrip = this.ToolStrip;
            for (int i = 0; i < toolStrip.Items.Count; i++)
            {
                if (toolStrip.Items[i].Placement == ToolStripItemPlacement.Overflow)
                {
                    toolStrip.Items[i].ParentInternal = null;
                }
                toolStrip.Items[i].SetPlacement(ToolStripItemPlacement.None);
            }
        }

        private int SendNextItemToOverflow(int spaceNeeded, bool horizontal)
        {
            int num = 0;
            int backwardsWalkingIndex = this.BackwardsWalkingIndex;
            this.BackwardsWalkingIndex = (backwardsWalkingIndex == -1) ? (this.ToolStrip.Items.Count - 1) : (backwardsWalkingIndex - 1);
            while (this.BackwardsWalkingIndex >= 0)
            {
                ToolStripItem item = this.ToolStrip.Items[this.BackwardsWalkingIndex];
                if (((IArrangedElement) item).ParticipatesInLayout)
                {
                    Padding margin = item.Margin;
                    if ((item.Overflow == ToolStripItemOverflow.AsNeeded) && (item.Placement != ToolStripItemPlacement.Overflow))
                    {
                        Size size = item.AutoSize ? item.GetPreferredSize(this.displayRectangle.Size) : item.Size;
                        if (this.BackwardsWalkingIndex <= this.ForwardsWalkingIndex)
                        {
                            num += horizontal ? (size.Width + margin.Horizontal) : (size.Height + margin.Vertical);
                        }
                        item.SetPlacement(ToolStripItemPlacement.Overflow);
                        if (!this.OverflowRequired)
                        {
                            spaceNeeded += horizontal ? this.OverflowButtonSize.Width : this.OverflowButtonSize.Height;
                            this.OverflowRequired = true;
                        }
                        this.OverflowSpace += horizontal ? (size.Width + margin.Horizontal) : (size.Height + margin.Vertical);
                    }
                    if (num > spaceNeeded)
                    {
                        return num;
                    }
                }
                this.BackwardsWalkingIndex--;
            }
            return num;
        }

        private void SetItemLocation(ToolStripItem item, Point itemLocation, Size itemSize)
        {
            if ((item.Placement == ToolStripItemPlacement.Main) && !(item is ToolStripOverflowButton))
            {
                bool flag = this.ToolStrip.LayoutStyle == ToolStripLayoutStyle.HorizontalStackWithOverflow;
                Rectangle rectangle = new Rectangle(itemLocation, itemSize);
                if (flag)
                {
                    if ((rectangle.Right > this.displayRectangle.Right) || (rectangle.Left < this.displayRectangle.Left))
                    {
                        itemLocation = this.noMansLand;
                        item.SetPlacement(ToolStripItemPlacement.None);
                    }
                }
                else if ((rectangle.Bottom > this.displayRectangle.Bottom) || (rectangle.Top < this.displayRectangle.Top))
                {
                    itemLocation = this.noMansLand;
                    item.SetPlacement(ToolStripItemPlacement.None);
                }
            }
            item.SetBounds(new Rectangle(itemLocation, itemSize));
        }

        protected int BackwardsWalkingIndex
        {
            get
            {
                return this.backwardsWalkingIndex;
            }
            set
            {
                this.backwardsWalkingIndex = value;
            }
        }

        protected int ForwardsWalkingIndex
        {
            get
            {
                return this.forwardsWalkingIndex;
            }
            set
            {
                this.forwardsWalkingIndex = value;
            }
        }

        private Size OverflowButtonSize
        {
            get
            {
                System.Windows.Forms.ToolStrip toolStrip = this.ToolStrip;
                if (!toolStrip.CanOverflow)
                {
                    return Size.Empty;
                }
                Size size = toolStrip.OverflowButton.AutoSize ? toolStrip.OverflowButton.GetPreferredSize(this.displayRectangle.Size) : toolStrip.OverflowButton.Size;
                return (size + toolStrip.OverflowButton.Margin.Size);
            }
        }

        private bool OverflowRequired
        {
            get
            {
                return this.overflowRequired;
            }
            set
            {
                this.overflowRequired = value;
            }
        }

        private int OverflowSpace
        {
            get
            {
                return this.overflowSpace;
            }
            set
            {
                this.overflowSpace = value;
            }
        }

        public System.Windows.Forms.ToolStrip ToolStrip
        {
            get
            {
                return this.toolStrip;
            }
        }
    }
}

