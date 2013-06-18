namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Text;
    using System.Runtime;
    using System.Threading;
    using System.Windows.Forms;
    using System.Workflow.ComponentModel;

    internal sealed class ItemPalette
    {
        private Font font;
        private ItemList<System.Workflow.ComponentModel.Design.ItemInfo> items;
        private Palette palette;
        private System.Workflow.ComponentModel.Design.ItemInfo selectedItem;

        public event EventHandler Closed;

        public event SelectionChangeEventHandler<SelectionChangeEventArgs> SelectionChanged;

        public ItemPalette()
        {
            this.items = new ItemList<System.Workflow.ComponentModel.Design.ItemInfo>(this);
        }

        private void DestroyPalette(Palette palette)
        {
            if (palette != null)
            {
                if (this.Closed != null)
                {
                    this.Closed(this, EventArgs.Empty);
                }
                palette.LostFocus -= new EventHandler(this.OnPaletteLostFocus);
                palette.Close();
                palette.Dispose();
                this.palette = null;
            }
        }

        private void OnPaletteLostFocus(object sender, EventArgs e)
        {
            this.DestroyPalette(sender as Palette);
        }

        public void SetFont(Font font)
        {
            this.font = font;
        }

        public void Show(Point location)
        {
            if (this.palette != null)
            {
                this.DestroyPalette(this.palette);
            }
            this.palette = new Palette(this, location);
            this.palette.Font = this.font;
            this.palette.Show();
            this.palette.Focus();
            this.palette.LostFocus += new EventHandler(this.OnPaletteLostFocus);
        }

        public bool IsVisible
        {
            get
            {
                return ((this.palette != null) && this.palette.Visible);
            }
        }

        public IList<System.Workflow.ComponentModel.Design.ItemInfo> Items
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.items;
            }
        }

        public System.Workflow.ComponentModel.Design.ItemInfo SelectedItem
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.selectedItem;
            }
            set
            {
                if (this.selectedItem != value)
                {
                    System.Workflow.ComponentModel.Design.ItemInfo selectedItem = this.selectedItem;
                    this.selectedItem = value;
                    if (this.SelectionChanged != null)
                    {
                        this.SelectionChanged(this, new SelectionChangeEventArgs(selectedItem, this.selectedItem));
                        if (this.palette != null)
                        {
                            this.palette.Invalidate();
                        }
                    }
                }
            }
        }

        private sealed class Palette : Form
        {
            private int activeIndex = -1;
            private static readonly int DropShadowWidth = 4;
            private ItemList<System.Workflow.ComponentModel.Design.ItemInfo> enabledItems;
            private Rectangle formRectangle;
            private Size imageRectangle = new Size(20, 20);
            private Size imageSize = new Size(0x10, 0x10);
            private int itemHeight;
            private List<Rectangle> itemRectangles = new List<Rectangle>();
            private int itemWidth;
            private Rectangle leftGradientRectangle;
            private int leftTextMargin = 5;
            private int maximumTextWidth = 500;
            private int maxTextHeight;
            private int menuItemCount;
            private PaletteShadow paletteShadow;
            private ItemPalette parent;
            private int rightTextMargin = 20;
            private Size selectionItemMargin = new Size(1, 1);
            private Rectangle workingRectangle = Rectangle.Empty;

            public Palette(ItemPalette parent, Point location)
            {
                this.parent = parent;
                this.enabledItems = new ItemList<System.Workflow.ComponentModel.Design.ItemInfo>(this);
                foreach (System.Workflow.ComponentModel.Design.ItemInfo info in this.parent.items)
                {
                    ActivityDesignerVerb verb = info.UserData[DesignerUserDataKeys.DesignerVerb] as ActivityDesignerVerb;
                    if ((verb == null) || verb.Enabled)
                    {
                        this.enabledItems.Add(info);
                    }
                }
                this.menuItemCount = this.enabledItems.Count;
                base.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.SupportsTransparentBackColor | ControlStyles.UserPaint, true);
                base.FormBorderStyle = FormBorderStyle.None;
                this.BackColor = Color.White;
                base.ShowInTaskbar = false;
                base.MaximizeBox = false;
                base.ControlBox = false;
                base.StartPosition = FormStartPosition.Manual;
                Screen screen = Screen.FromPoint(location);
                this.workingRectangle = screen.WorkingArea;
                this.PreparePalette(location);
                this.paletteShadow = new PaletteShadow(this);
            }

            private void DestroyShadow()
            {
                if (this.paletteShadow != null)
                {
                    this.paletteShadow.Close();
                    this.paletteShadow.Dispose();
                    this.paletteShadow = null;
                }
            }

            private Rectangle GetItemBounds(int index)
            {
                if ((index >= 0) && (index < this.itemRectangles.Count))
                {
                    return this.itemRectangles[index];
                }
                return Rectangle.Empty;
            }

            private void LayoutPalette()
            {
                this.itemRectangles.Clear();
                this.leftGradientRectangle = Rectangle.Empty;
                using (Graphics graphics = base.CreateGraphics())
                {
                    Size empty = Size.Empty;
                    foreach (System.Workflow.ComponentModel.Design.ItemInfo info in this.enabledItems)
                    {
                        SizeF ef = graphics.MeasureString(info.Text, this.Font);
                        empty.Width = Math.Max(Convert.ToInt32(Math.Ceiling((double) ef.Width)), empty.Width);
                        empty.Height = Math.Max(Convert.ToInt32(Math.Ceiling((double) ef.Height)), empty.Height);
                    }
                    empty.Width = Math.Min(empty.Width, this.maximumTextWidth);
                    this.maxTextHeight = empty.Height;
                    this.itemHeight = Math.Max(this.imageRectangle.Height, empty.Height + 2) + 3;
                    this.itemWidth = (((this.imageRectangle.Width + (2 * this.selectionItemMargin.Width)) + this.leftTextMargin) + empty.Width) + this.rightTextMargin;
                }
                int y = 2;
                using (List<System.Workflow.ComponentModel.Design.ItemInfo>.Enumerator enumerator2 = this.enabledItems.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        System.Workflow.ComponentModel.Design.ItemInfo current = enumerator2.Current;
                        this.itemRectangles.Add(new Rectangle(2, y, this.itemWidth, this.itemHeight));
                        y += this.itemHeight + (2 * this.selectionItemMargin.Height);
                    }
                }
                this.leftGradientRectangle = new Rectangle(2, 2, 0x18, y - 4);
                this.formRectangle = new Rectangle(0, 0, this.itemWidth + 4, y);
            }

            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            protected override void OnClosing(CancelEventArgs e)
            {
                this.DestroyShadow();
            }

            protected override void OnKeyDown(KeyEventArgs e)
            {
                base.OnKeyDown(e);
                if (e.KeyCode == Keys.Enter)
                {
                    if (this.ActiveItem != null)
                    {
                        try
                        {
                            this.parent.SelectedItem = this.ActiveItem;
                        }
                        finally
                        {
                            this.parent.DestroyPalette(this);
                        }
                    }
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    this.parent.DestroyPalette(this);
                }
                else if ((e.KeyCode == Keys.Up) || (e.KeyCode == Keys.Down))
                {
                    int index = -1;
                    if (this.activeIndex != -1)
                    {
                        index = this.activeIndex;
                    }
                    if (index >= 0)
                    {
                        if (e.KeyCode == Keys.Up)
                        {
                            index--;
                        }
                        else if (e.KeyCode == Keys.Down)
                        {
                            index++;
                        }
                    }
                    else
                    {
                        index = 0;
                    }
                    if ((index >= 0) && (index < this.enabledItems.Count))
                    {
                        this.SetActiveItem(index);
                    }
                }
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);
                System.Workflow.ComponentModel.Design.ItemInfo info = null;
                Point pt = new Point(e.X, e.Y);
                for (int i = 0; i < this.enabledItems.Count; i++)
                {
                    if (this.GetItemBounds(i).Contains(pt))
                    {
                        info = this.enabledItems[i];
                        break;
                    }
                }
                if (info != null)
                {
                    try
                    {
                        this.parent.SelectedItem = info;
                    }
                    finally
                    {
                        this.parent.DestroyPalette(this);
                    }
                }
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                base.OnMouseLeave(e);
                this.SetActiveItem(-1);
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);
                Point pt = new Point(e.X, e.Y);
                for (int i = 0; i < this.enabledItems.Count; i++)
                {
                    if (this.GetItemBounds(i).Contains(pt))
                    {
                        this.SetActiveItem(i);
                        return;
                    }
                }
            }

            protected override void OnPaint(PaintEventArgs paintArgs)
            {
                Graphics graphics = paintArgs.Graphics;
                graphics.FillRectangle(SystemBrushes.Window, this.formRectangle);
                graphics.DrawRectangle(SystemPens.ControlDarkDark, this.formRectangle.X, this.formRectangle.Y, this.formRectangle.Width - 1, this.formRectangle.Height - 1);
                using (Brush brush = new LinearGradientBrush(new Point(this.leftGradientRectangle.Left, this.leftGradientRectangle.Top), new Point(this.leftGradientRectangle.Right, this.leftGradientRectangle.Top), SystemColors.Window, SystemColors.ScrollBar))
                {
                    graphics.FillRectangle(brush, this.leftGradientRectangle);
                }
                for (int i = 0; i < this.enabledItems.Count; i++)
                {
                    Rectangle itemBounds = this.GetItemBounds(i);
                    if (this.activeIndex == i)
                    {
                        graphics.FillRectangle(SystemBrushes.InactiveCaptionText, itemBounds.X, itemBounds.Y, itemBounds.Width - 1, itemBounds.Height - 1);
                        graphics.DrawRectangle(SystemPens.ActiveCaption, itemBounds.X, itemBounds.Y, itemBounds.Width - 1, itemBounds.Height - 1);
                    }
                    if (this.enabledItems[i].Image != null)
                    {
                        Point location = new Point(itemBounds.Left + 3, itemBounds.Top + 3);
                        Size size = this.enabledItems[i].Image.Size;
                        graphics.DrawImage(this.enabledItems[i].Image, new Rectangle(location, size), new Rectangle(Point.Empty, size), GraphicsUnit.Pixel);
                    }
                    Rectangle boundingRect = new Rectangle(((itemBounds.Left + 20) + 5) + 2, itemBounds.Top + 1, this.itemWidth - 0x1d, this.itemHeight - 3);
                    int num2 = boundingRect.Height - this.maxTextHeight;
                    num2 = (num2 > 0) ? (num2 / 2) : 0;
                    boundingRect.Height = Math.Min(boundingRect.Height, this.maxTextHeight);
                    boundingRect.Y += num2;
                    graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                    string text = this.enabledItems[i].Text.Replace("&", "");
                    ActivityDesignerPaint.DrawText(graphics, this.Font, text, boundingRect, StringAlignment.Near, TextQuality.Aliased, SystemBrushes.ControlText);
                }
            }

            protected override void OnVisibleChanged(EventArgs e)
            {
                base.OnVisibleChanged(e);
                if (base.Visible)
                {
                    this.paletteShadow.Show();
                    base.BringToFront();
                    base.Focus();
                }
                else
                {
                    this.paletteShadow.Hide();
                }
            }

            private void PreparePalette(Point location)
            {
                this.LayoutPalette();
                Point pos = location;
                Rectangle formRectangle = this.formRectangle;
                formRectangle.Offset(pos);
                Size empty = Size.Empty;
                formRectangle.Width += DropShadowWidth;
                formRectangle.Height += DropShadowWidth;
                Rectangle a = Rectangle.Empty;
                foreach (Screen screen in Screen.AllScreens)
                {
                    a = Rectangle.Union(a, screen.Bounds);
                }
                if (this.workingRectangle.Top > formRectangle.Top)
                {
                    empty.Height += this.workingRectangle.Top - formRectangle.Top;
                }
                else if (this.workingRectangle.Bottom < formRectangle.Bottom)
                {
                    empty.Height -= formRectangle.Bottom - this.workingRectangle.Bottom;
                }
                if (this.workingRectangle.Left > formRectangle.Left)
                {
                    empty.Width += this.workingRectangle.Left - formRectangle.Left;
                }
                else if (this.workingRectangle.Right < formRectangle.Right)
                {
                    empty.Width -= formRectangle.Right - this.workingRectangle.Right;
                }
                pos += empty;
                base.Location = pos;
                GraphicsPath path = new GraphicsPath();
                path.AddRectangle(this.formRectangle);
                base.Size = this.formRectangle.Size;
                base.Region = new Region(path);
            }

            private void SetActiveItem(int index)
            {
                if (this.activeIndex != index)
                {
                    if (this.activeIndex != -1)
                    {
                        base.Invalidate(this.GetItemBounds(this.activeIndex));
                    }
                    this.activeIndex = index;
                    if (this.activeIndex != -1)
                    {
                        base.Invalidate(this.GetItemBounds(this.activeIndex));
                    }
                }
            }

            private System.Workflow.ComponentModel.Design.ItemInfo ActiveItem
            {
                get
                {
                    if (this.activeIndex <= -1)
                    {
                        return null;
                    }
                    return this.enabledItems[this.activeIndex];
                }
            }

            private sealed class PaletteShadow : Form
            {
                private ItemPalette.Palette parent;

                public PaletteShadow(ItemPalette.Palette parent)
                {
                    this.parent = parent;
                    base.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.SupportsTransparentBackColor | ControlStyles.UserPaint, true);
                    base.FormBorderStyle = FormBorderStyle.None;
                    this.BackColor = Color.White;
                    base.ShowInTaskbar = false;
                    base.MaximizeBox = false;
                    base.ControlBox = false;
                    base.Opacity = 0.5;
                    base.StartPosition = FormStartPosition.Manual;
                    base.Enabled = false;
                    base.Region = parent.Region;
                    base.Location = new Point(this.parent.Location.X + ItemPalette.Palette.DropShadowWidth, this.parent.Location.Y + ItemPalette.Palette.DropShadowWidth);
                }

                protected override void OnPaint(PaintEventArgs e)
                {
                    base.OnPaint(e);
                    Rectangle formRectangle = this.parent.formRectangle;
                    formRectangle.Offset(-ItemPalette.Palette.DropShadowWidth, -ItemPalette.Palette.DropShadowWidth);
                    ActivityDesignerPaint.DrawDropShadow(e.Graphics, formRectangle, Color.Black, 4, LightSourcePosition.Top | LightSourcePosition.Left, 0.2f, false);
                }
            }
        }
    }
}

