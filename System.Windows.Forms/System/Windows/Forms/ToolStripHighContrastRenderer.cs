namespace System.Windows.Forms
{
    using System;
    using System.Collections.Specialized;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;

    internal class ToolStripHighContrastRenderer : ToolStripSystemRenderer
    {
        private const int GRIP_PADDING = 4;
        private BitVector32 options = new BitVector32();
        private static readonly int optionsDottedBorder = BitVector32.CreateMask();
        private static readonly int optionsDottedGrip = BitVector32.CreateMask(optionsDottedBorder);
        private static readonly int optionsFillWhenSelected = BitVector32.CreateMask(optionsDottedGrip);

        public ToolStripHighContrastRenderer(bool systemRenderMode)
        {
            this.options[(optionsDottedBorder | optionsDottedGrip) | optionsFillWhenSelected] = !systemRenderMode;
        }

        internal static bool IsHighContrastWhiteOnBlack()
        {
            return (SystemColors.Control.ToArgb() == Color.Black.ToArgb());
        }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            base.OnRenderArrow(e);
        }

        protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
        {
            if (this.FillWhenSelected)
            {
                ToolStripButton item = e.Item as ToolStripButton;
                if ((item != null) && item.Checked)
                {
                    Graphics graphics = e.Graphics;
                    Rectangle rect = new Rectangle(Point.Empty, e.Item.Size);
                    if (item.CheckState == CheckState.Checked)
                    {
                        graphics.FillRectangle(SystemBrushes.Highlight, rect);
                    }
                    graphics.DrawRectangle(SystemPens.ControlLight, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
                }
                else
                {
                    this.RenderItemInternalFilled(e);
                }
            }
            else
            {
                base.OnRenderButtonBackground(e);
            }
        }

        protected override void OnRenderDropDownButtonBackground(ToolStripItemRenderEventArgs e)
        {
            if (this.FillWhenSelected)
            {
                this.RenderItemInternalFilled(e, false);
            }
            else
            {
                base.OnRenderDropDownButtonBackground(e);
                if (e.Item.Pressed)
                {
                    e.Graphics.DrawRectangle(SystemPens.ButtonHighlight, new Rectangle(0, 0, e.Item.Width - 1, e.Item.Height - 1));
                }
            }
        }

        protected override void OnRenderGrip(ToolStripGripRenderEventArgs e)
        {
            if (this.DottedGrip)
            {
                Graphics graphics = e.Graphics;
                Rectangle gripBounds = e.GripBounds;
                ToolStrip toolStrip = e.ToolStrip;
                int num = (toolStrip.Orientation == Orientation.Horizontal) ? gripBounds.Height : gripBounds.Width;
                int num2 = (toolStrip.Orientation == Orientation.Horizontal) ? gripBounds.Width : gripBounds.Height;
                int num3 = (num - 8) / 4;
                if (num3 > 0)
                {
                    Rectangle[] rects = new Rectangle[num3];
                    int y = 4;
                    int x = num2 / 2;
                    for (int i = 0; i < num3; i++)
                    {
                        rects[i] = (toolStrip.Orientation == Orientation.Horizontal) ? new Rectangle(x, y, 2, 2) : new Rectangle(y, x, 2, 2);
                        y += 4;
                    }
                    graphics.FillRectangles(SystemBrushes.ControlLight, rects);
                }
            }
            else
            {
                base.OnRenderGrip(e);
            }
        }

        protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
        {
        }

        protected override void OnRenderItemBackground(ToolStripItemRenderEventArgs e)
        {
            base.OnRenderItemBackground(e);
        }

        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            base.OnRenderItemCheck(e);
        }

        protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)
        {
            Image image = e.Image;
            if (image != null)
            {
                if (Image.GetPixelFormatSize(image.PixelFormat) > 0x10)
                {
                    base.OnRenderItemImage(e);
                }
                else
                {
                    Graphics graphics = e.Graphics;
                    ToolStripItem item = e.Item;
                    Rectangle imageRectangle = e.ImageRectangle;
                    using (ImageAttributes attributes = new ImageAttributes())
                    {
                        if (IsHighContrastWhiteOnBlack() && (!this.FillWhenSelected || (!e.Item.Pressed && !e.Item.Selected)))
                        {
                            ColorMap map = new ColorMap();
                            ColorMap map2 = new ColorMap();
                            ColorMap map3 = new ColorMap();
                            map.OldColor = Color.Black;
                            map.NewColor = Color.White;
                            map2.OldColor = Color.White;
                            map2.NewColor = Color.Black;
                            map3.OldColor = Color.FromArgb(0, 0, 0x80);
                            map3.NewColor = Color.White;
                            attributes.SetRemapTable(new ColorMap[] { map, map2, map3 }, ColorAdjustType.Bitmap);
                        }
                        if (item.ImageScaling == ToolStripItemImageScaling.None)
                        {
                            graphics.DrawImage(image, imageRectangle, 0, 0, imageRectangle.Width, imageRectangle.Height, GraphicsUnit.Pixel, attributes);
                        }
                        else
                        {
                            graphics.DrawImage(image, imageRectangle, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
                        }
                    }
                }
            }
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            if ((e.TextColor != SystemColors.HighlightText) || (e.TextColor != SystemColors.ControlText))
            {
                e.DefaultTextColor = SystemColors.ControlText;
            }
            base.OnRenderItemText(e);
        }

        protected override void OnRenderLabelBackground(ToolStripItemRenderEventArgs e)
        {
            if (this.FillWhenSelected)
            {
                this.RenderItemInternalFilled(e);
            }
            else
            {
                base.OnRenderLabelBackground(e);
            }
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            base.OnRenderMenuItemBackground(e);
            if (!e.Item.IsOnDropDown && e.Item.Pressed)
            {
                e.Graphics.DrawRectangle(SystemPens.ButtonHighlight, 0, 0, e.Item.Width - 1, e.Item.Height - 1);
            }
        }

        protected override void OnRenderOverflowButtonBackground(ToolStripItemRenderEventArgs e)
        {
            if (this.FillWhenSelected)
            {
                this.RenderItemInternalFilled(e, false);
                ToolStripItem toolStripItem = e.Item;
                Graphics g = e.Graphics;
                Color arrowColor = toolStripItem.Enabled ? SystemColors.ControlText : SystemColors.ControlDark;
                base.DrawArrow(new ToolStripArrowRenderEventArgs(g, toolStripItem, new Rectangle(Point.Empty, toolStripItem.Size), arrowColor, ArrowDirection.Down));
            }
            else
            {
                base.OnRenderOverflowButtonBackground(e);
            }
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            Pen buttonShadow = SystemPens.ButtonShadow;
            Graphics graphics = e.Graphics;
            Rectangle rectangle = new Rectangle(Point.Empty, e.Item.Size);
            if (e.Vertical)
            {
                if (rectangle.Height >= 8)
                {
                    rectangle.Inflate(0, -4);
                }
                int num = rectangle.Width / 2;
                graphics.DrawLine(buttonShadow, num, rectangle.Top, num, rectangle.Bottom - 1);
            }
            else
            {
                if (rectangle.Width >= 4)
                {
                    rectangle.Inflate(-2, 0);
                }
                int num2 = rectangle.Height / 2;
                graphics.DrawLine(buttonShadow, rectangle.Left, num2, rectangle.Right - 1, num2);
            }
        }

        protected override void OnRenderSplitButtonBackground(ToolStripItemRenderEventArgs e)
        {
            ToolStripSplitButton item = e.Item as ToolStripSplitButton;
            Rectangle rect = new Rectangle(Point.Empty, e.Item.Size);
            Graphics g = e.Graphics;
            if (item != null)
            {
                Rectangle dropDownButtonBounds = item.DropDownButtonBounds;
                if (item.Pressed)
                {
                    g.DrawRectangle(SystemPens.ButtonHighlight, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
                }
                else if (item.Selected)
                {
                    g.FillRectangle(SystemBrushes.Highlight, rect);
                    g.DrawRectangle(SystemPens.ButtonHighlight, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
                    g.DrawRectangle(SystemPens.ButtonHighlight, dropDownButtonBounds);
                }
                base.DrawArrow(new ToolStripArrowRenderEventArgs(g, item, dropDownButtonBounds, SystemColors.ControlText, ArrowDirection.Down));
            }
        }

        protected override void OnRenderStatusStripSizingGrip(ToolStripRenderEventArgs e)
        {
            base.OnRenderStatusStripSizingGrip(e);
        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            Rectangle rectangle = new Rectangle(Point.Empty, e.ToolStrip.Size);
            Graphics graphics = e.Graphics;
            if (e.ToolStrip is ToolStripDropDown)
            {
                graphics.DrawRectangle(SystemPens.ButtonHighlight, rectangle.X, rectangle.Y, rectangle.Width - 1, rectangle.Height - 1);
                if (!(e.ToolStrip is ToolStripOverflow))
                {
                    graphics.FillRectangle(SystemBrushes.Control, e.ConnectedArea);
                }
            }
            else if (!(e.ToolStrip is MenuStrip))
            {
                if (e.ToolStrip is StatusStrip)
                {
                    graphics.DrawRectangle(SystemPens.ButtonShadow, rectangle.X, rectangle.Y, rectangle.Width - 1, rectangle.Height - 1);
                }
                else
                {
                    this.RenderToolStripBackgroundInternal(e);
                }
            }
        }

        private void RenderItemInternalFilled(ToolStripItemRenderEventArgs e)
        {
            this.RenderItemInternalFilled(e, true);
        }

        private void RenderItemInternalFilled(ToolStripItemRenderEventArgs e, bool pressFill)
        {
            Graphics graphics = e.Graphics;
            Rectangle rect = new Rectangle(Point.Empty, e.Item.Size);
            if (e.Item.Pressed)
            {
                if (pressFill)
                {
                    graphics.FillRectangle(SystemBrushes.Highlight, rect);
                }
                else
                {
                    graphics.DrawRectangle(SystemPens.ControlLight, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
                }
            }
            else if (e.Item.Selected)
            {
                graphics.FillRectangle(SystemBrushes.Highlight, rect);
                graphics.DrawRectangle(SystemPens.ControlLight, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
            }
        }

        private void RenderToolStripBackgroundInternal(ToolStripRenderEventArgs e)
        {
            Rectangle rect = new Rectangle(Point.Empty, e.ToolStrip.Size);
            Graphics graphics = e.Graphics;
            if (this.DottedBorder)
            {
                using (Pen pen = new Pen(SystemColors.ButtonShadow))
                {
                    pen.DashStyle = DashStyle.Dot;
                    bool flag = (rect.Width & 1) == 1;
                    bool flag2 = (rect.Height & 1) == 1;
                    int num = 2;
                    graphics.DrawLine(pen, rect.X + num, rect.Y, rect.Width - 1, rect.Y);
                    graphics.DrawLine(pen, (int) (rect.X + num), (int) (rect.Height - 1), (int) (rect.Width - 1), (int) (rect.Height - 1));
                    graphics.DrawLine(pen, rect.X, rect.Y + num, rect.X, rect.Height - 1);
                    graphics.DrawLine(pen, (int) (rect.Width - 1), (int) (rect.Y + num), (int) (rect.Width - 1), (int) (rect.Height - 1));
                    graphics.FillRectangle(SystemBrushes.ButtonShadow, new Rectangle(1, 1, 1, 1));
                    if (flag)
                    {
                        graphics.FillRectangle(SystemBrushes.ButtonShadow, new Rectangle(rect.Width - 2, 1, 1, 1));
                    }
                    if (flag2)
                    {
                        graphics.FillRectangle(SystemBrushes.ButtonShadow, new Rectangle(1, rect.Height - 2, 1, 1));
                    }
                    if (flag2 && flag)
                    {
                        graphics.FillRectangle(SystemBrushes.ButtonShadow, new Rectangle(rect.Width - 2, rect.Height - 2, 1, 1));
                    }
                    return;
                }
            }
            rect.Width--;
            rect.Height--;
            graphics.DrawRectangle(SystemPens.ButtonShadow, rect);
        }

        public bool DottedBorder
        {
            get
            {
                return this.options[optionsDottedBorder];
            }
        }

        public bool DottedGrip
        {
            get
            {
                return this.options[optionsDottedGrip];
            }
        }

        public bool FillWhenSelected
        {
            get
            {
                return this.options[optionsFillWhenSelected];
            }
        }

        internal override ToolStripRenderer RendererOverride
        {
            get
            {
                return null;
            }
        }
    }
}

