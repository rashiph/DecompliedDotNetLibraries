namespace System.Windows.Forms
{
    using System;
    using System.Drawing;

    internal sealed class ToolStripSplitStackDragDropHandler : IDropTarget, ISupportOleDropSource
    {
        private ToolStrip owner;

        public ToolStripSplitStackDragDropHandler(ToolStrip owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }
            this.owner = owner;
        }

        private RelativeLocation ComparePositions(Rectangle orig, Point check)
        {
            if (this.owner.Orientation == Orientation.Horizontal)
            {
                int num = orig.Width / 2;
                if ((orig.Left + num) >= check.X)
                {
                    return RelativeLocation.Left;
                }
                if ((orig.Right - num) <= check.X)
                {
                    return RelativeLocation.Right;
                }
            }
            if (this.owner.Orientation == Orientation.Vertical)
            {
                int num2 = orig.Height / 2;
                return ((check.Y <= (orig.Top + num2)) ? RelativeLocation.Above : RelativeLocation.Below);
            }
            return RelativeLocation.Left;
        }

        private int GetItemInsertionIndex(Point ownerClientAreaRelativeDropPoint)
        {
            for (int i = 0; i < this.owner.DisplayedItems.Count; i++)
            {
                Rectangle bounds = this.owner.DisplayedItems[i].Bounds;
                bounds.Inflate(this.owner.DisplayedItems[i].Margin.Size);
                if (bounds.Contains(ownerClientAreaRelativeDropPoint))
                {
                    return this.owner.Items.IndexOf(this.owner.DisplayedItems[i]);
                }
            }
            if (this.owner.DisplayedItems.Count <= 0)
            {
                return -1;
            }
            for (int j = 0; j < this.owner.DisplayedItems.Count; j++)
            {
                if (this.owner.DisplayedItems[j].Alignment == ToolStripItemAlignment.Right)
                {
                    if (j > 0)
                    {
                        return this.owner.Items.IndexOf(this.owner.DisplayedItems[j - 1]);
                    }
                    return this.owner.Items.IndexOf(this.owner.DisplayedItems[j]);
                }
            }
            return this.owner.Items.IndexOf(this.owner.DisplayedItems[this.owner.DisplayedItems.Count - 1]);
        }

        public void OnDragDrop(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ToolStripItem)))
            {
                ToolStripItem data = (ToolStripItem) e.Data.GetData(typeof(ToolStripItem));
                this.OnDropItem(data, this.owner.PointToClient(new Point(e.X, e.Y)));
            }
        }

        public void OnDragEnter(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ToolStripItem)))
            {
                e.Effect = DragDropEffects.Move;
                this.ShowItemDropPoint(this.owner.PointToClient(new Point(e.X, e.Y)));
            }
        }

        public void OnDragLeave(EventArgs e)
        {
            this.owner.ClearInsertionMark();
        }

        public void OnDragOver(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ToolStripItem)))
            {
                if (this.ShowItemDropPoint(this.owner.PointToClient(new Point(e.X, e.Y))))
                {
                    e.Effect = DragDropEffects.Move;
                }
                else
                {
                    if (this.owner != null)
                    {
                        this.owner.ClearInsertionMark();
                    }
                    e.Effect = DragDropEffects.None;
                }
            }
        }

        private void OnDropItem(ToolStripItem droppedItem, Point ownerClientAreaRelativeDropPoint)
        {
            int itemInsertionIndex = this.GetItemInsertionIndex(ownerClientAreaRelativeDropPoint);
            if (itemInsertionIndex < 0)
            {
                if ((itemInsertionIndex == -1) && (this.owner.Items.Count == 0))
                {
                    this.owner.Items.Add(droppedItem);
                    this.owner.ClearInsertionMark();
                }
            }
            else
            {
                ToolStripItem item = this.owner.Items[itemInsertionIndex];
                if (item == droppedItem)
                {
                    this.owner.ClearInsertionMark();
                }
                else
                {
                    RelativeLocation location = this.ComparePositions(item.Bounds, ownerClientAreaRelativeDropPoint);
                    droppedItem.Alignment = item.Alignment;
                    int num2 = Math.Max(0, itemInsertionIndex);
                    switch (location)
                    {
                        case RelativeLocation.Above:
                            num2 = (item.Alignment == ToolStripItemAlignment.Left) ? num2 : (num2 + 1);
                            break;

                        case RelativeLocation.Below:
                            num2 = (item.Alignment == ToolStripItemAlignment.Left) ? num2 : (num2 - 1);
                            break;

                        default:
                            if (((item.Alignment == ToolStripItemAlignment.Left) && (location == RelativeLocation.Left)) || ((item.Alignment == ToolStripItemAlignment.Right) && (location == RelativeLocation.Right)))
                            {
                                num2 = Math.Max(0, (this.owner.RightToLeft == RightToLeft.Yes) ? (num2 + 1) : num2);
                            }
                            else
                            {
                                num2 = Math.Max(0, (this.owner.RightToLeft == RightToLeft.No) ? (num2 + 1) : num2);
                            }
                            break;
                    }
                    if (this.owner.Items.IndexOf(droppedItem) < num2)
                    {
                        num2--;
                    }
                    this.owner.Items.MoveItem(Math.Max(0, num2), droppedItem);
                    this.owner.ClearInsertionMark();
                }
            }
        }

        public void OnGiveFeedback(GiveFeedbackEventArgs e)
        {
        }

        public void OnQueryContinueDrag(QueryContinueDragEventArgs e)
        {
        }

        private bool ShowItemDropPoint(Point ownerClientAreaRelativeDropPoint)
        {
            int itemInsertionIndex = this.GetItemInsertionIndex(ownerClientAreaRelativeDropPoint);
            if (itemInsertionIndex < 0)
            {
                if (this.owner.Items.Count == 0)
                {
                    Rectangle displayRectangle = this.owner.DisplayRectangle;
                    displayRectangle.Width = 6;
                    this.owner.PaintInsertionMark(displayRectangle);
                    return true;
                }
                return false;
            }
            ToolStripItem item = this.owner.Items[itemInsertionIndex];
            RelativeLocation location = this.ComparePositions(item.Bounds, ownerClientAreaRelativeDropPoint);
            Rectangle empty = Rectangle.Empty;
            switch (location)
            {
                case RelativeLocation.Above:
                    empty = new Rectangle(this.owner.Margin.Left, item.Bounds.Top, (this.owner.Width - this.owner.Margin.Horizontal) - 1, 6);
                    break;

                case RelativeLocation.Below:
                    empty = new Rectangle(this.owner.Margin.Left, item.Bounds.Bottom, (this.owner.Width - this.owner.Margin.Horizontal) - 1, 6);
                    break;

                case RelativeLocation.Right:
                    empty = new Rectangle(item.Bounds.Right, this.owner.Margin.Top, 6, (this.owner.Height - this.owner.Margin.Vertical) - 1);
                    break;

                case RelativeLocation.Left:
                    empty = new Rectangle(item.Bounds.Left, this.owner.Margin.Top, 6, (this.owner.Height - this.owner.Margin.Vertical) - 1);
                    break;
            }
            this.owner.PaintInsertionMark(empty);
            return true;
        }

        private enum RelativeLocation
        {
            Above,
            Below,
            Right,
            Left
        }
    }
}

