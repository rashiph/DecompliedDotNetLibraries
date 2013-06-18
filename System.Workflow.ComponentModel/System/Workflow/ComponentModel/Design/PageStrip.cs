namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Globalization;
    using System.Windows.Forms;

    internal sealed class PageStrip : ScrollableItemStrip
    {
        private static Brush HighliteBrush = new SolidBrush(Color.FromArgb(100, 0xff, 0xc3, 0x6b));
        private static Brush SelectionBrush = new SolidBrush(Color.FromArgb(0xff, 0xc3, 0x6b));

        public PageStrip(IServiceProvider serviceProvider, Size itemSize) : base(serviceProvider, Orientation.Horizontal, itemSize, Size.Empty)
        {
        }

        protected override ItemStrip CreateItemStrip(IServiceProvider serviceProvider, Orientation orientation, Size itemSize, Size margin)
        {
            return new PageItemStrip(serviceProvider, orientation, itemSize, margin);
        }

        public override void Draw(Graphics graphics)
        {
            GraphicsContainer container = graphics.BeginContainer();
            Rectangle bounds = base.Bounds;
            using (Region region = new Region(new Rectangle(bounds.X, bounds.Y, bounds.Width + 1, bounds.Height + 1)))
            {
                graphics.Clip = region;
                base.itemStrip.Draw(graphics);
                if (base.itemStrip.ScrollPosition > 0)
                {
                    this.DrawButton(graphics, (base.Orientation == Orientation.Horizontal) ? ScrollButton.Left : ScrollButton.Up);
                }
                if ((base.itemStrip.ScrollPosition + base.itemStrip.MaxVisibleItems) < base.itemStrip.Items.Count)
                {
                    this.DrawButton(graphics, (base.Orientation == Orientation.Horizontal) ? ScrollButton.Right : ScrollButton.Down);
                }
            }
            graphics.EndContainer(container);
        }

        private void DrawButton(Graphics graphics, ScrollButton scrollButton)
        {
            Rectangle buttonBounds = base.GetButtonBounds(scrollButton);
            if (base.Orientation == Orientation.Horizontal)
            {
                buttonBounds.Inflate(-base.itemStrip.ItemSize.Width / 6, -base.itemStrip.ItemSize.Height / 4);
            }
            else
            {
                buttonBounds.Inflate(-base.itemStrip.ItemSize.Width / 4, -base.itemStrip.ItemSize.Height / 6);
            }
            if (base.ActiveButton == scrollButton)
            {
                buttonBounds.Offset(1, 1);
                Size size = (base.Orientation == Orientation.Horizontal) ? new Size(0, 2) : new Size(2, 0);
                buttonBounds.Inflate(size.Width, size.Height);
                graphics.FillRectangle(SelectionBrush, buttonBounds);
                graphics.DrawRectangle(Pens.Black, buttonBounds);
                buttonBounds.Inflate(-size.Width, -size.Height);
            }
            using (GraphicsPath path = ActivityDesignerPaint.GetScrollIndicatorPath(buttonBounds, scrollButton))
            {
                graphics.FillPath(Brushes.Black, path);
                graphics.DrawPath(Pens.Black, path);
            }
        }

        private sealed class PageItemStrip : ItemStrip
        {
            public PageItemStrip(IServiceProvider serviceProvider, Orientation orientation, Size itemSize, Size margin) : base(serviceProvider, orientation, itemSize, margin)
            {
            }

            public override void Draw(Graphics graphics)
            {
                GraphicsContainer container = graphics.BeginContainer();
                Rectangle bounds = base.Bounds;
                using (Region region = new Region(new Rectangle(bounds.X, bounds.Y, bounds.Width + 1, bounds.Height + 1)))
                {
                    graphics.Clip = region;
                    StringFormat format = new StringFormat {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center,
                        Trimming = StringTrimming.Character,
                        FormatFlags = StringFormatFlags.NoWrap
                    };
                    int maxVisibleItems = base.MaxVisibleItems;
                    int scrollPosition = base.ScrollPosition;
                    for (int i = scrollPosition; (i < base.Items.Count) && (i < (scrollPosition + maxVisibleItems)); i++)
                    {
                        System.Workflow.ComponentModel.Design.ItemInfo itemInfo = base.Items[i];
                        Rectangle itemBounds = base.GetItemBounds(itemInfo);
                        int pageFoldSize = itemBounds.Width / 5;
                        GraphicsPath[] pathArray = ActivityDesignerPaint.GetPagePaths(itemBounds, pageFoldSize, DesignerContentAlignment.TopRight);
                        using (GraphicsPath path = pathArray[0])
                        {
                            using (GraphicsPath path2 = pathArray[1])
                            {
                                Brush white = Brushes.White;
                                if (base.SelectedItem == itemInfo)
                                {
                                    white = PageStrip.SelectionBrush;
                                }
                                else if (base.HighlitedItem == itemInfo)
                                {
                                    white = PageStrip.HighliteBrush;
                                }
                                graphics.FillPath(white, path);
                                graphics.DrawPath(Pens.DarkBlue, path);
                                graphics.FillPath(Brushes.White, path2);
                                graphics.DrawPath(Pens.DarkBlue, path2);
                                if (itemInfo.Image == null)
                                {
                                    itemBounds.Y += pageFoldSize;
                                    itemBounds.Height -= pageFoldSize;
                                    graphics.DrawString((i + 1).ToString(CultureInfo.CurrentCulture), Control.DefaultFont, SystemBrushes.ControlText, itemBounds, format);
                                }
                                else
                                {
                                    itemBounds.Y += pageFoldSize;
                                    itemBounds.Height -= pageFoldSize;
                                    itemBounds.X += (itemBounds.Width - itemBounds.Height) / 2;
                                    itemBounds.Width = itemBounds.Height;
                                    itemBounds.Inflate(-2, -2);
                                    ActivityDesignerPaint.DrawImage(graphics, itemInfo.Image, itemBounds, DesignerContentAlignment.Center);
                                }
                            }
                        }
                    }
                }
                graphics.EndContainer(container);
            }
        }
    }
}

