namespace System.Windows.Forms
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms.Layout;

    public class ToolStripProfessionalRenderer : ToolStripRenderer
    {
        private Padding dropDownMenuItemPaintPadding;
        private const int GRIP_PADDING = 4;
        private const int ICON_WELL_GRADIENT_WIDTH = 12;
        private static readonly Size onePix = new Size(1, 1);
        private const int overflowButtonWidth = 12;
        private ProfessionalColorTable professionalColorTable;
        private bool roundedEdges;
        private ToolStripRenderer toolStripHighContrastRenderer;
        private ToolStripRenderer toolStripLowResolutionRenderer;

        public ToolStripProfessionalRenderer()
        {
            this.dropDownMenuItemPaintPadding = new Padding(2, 0, 1, 0);
            this.roundedEdges = true;
        }

        internal ToolStripProfessionalRenderer(bool isDefault) : base(isDefault)
        {
            this.dropDownMenuItemPaintPadding = new Padding(2, 0, 1, 0);
            this.roundedEdges = true;
        }

        public ToolStripProfessionalRenderer(ProfessionalColorTable professionalColorTable)
        {
            this.dropDownMenuItemPaintPadding = new Padding(2, 0, 1, 0);
            this.roundedEdges = true;
            this.professionalColorTable = professionalColorTable;
        }

        private void FillWithDoubleGradient(Color beginColor, Color middleColor, Color endColor, Graphics g, Rectangle bounds, int firstGradientWidth, int secondGradientWidth, LinearGradientMode mode, bool flipHorizontal)
        {
            if ((bounds.Width != 0) && (bounds.Height != 0))
            {
                Rectangle rect = bounds;
                Rectangle rectangle2 = bounds;
                bool flag = true;
                if (mode == LinearGradientMode.Horizontal)
                {
                    if (flipHorizontal)
                    {
                        Color color = endColor;
                        endColor = beginColor;
                        beginColor = color;
                    }
                    rectangle2.Width = firstGradientWidth;
                    rect.Width = secondGradientWidth + 1;
                    rect.X = bounds.Right - rect.Width;
                    flag = bounds.Width > (firstGradientWidth + secondGradientWidth);
                }
                else
                {
                    rectangle2.Height = firstGradientWidth;
                    rect.Height = secondGradientWidth + 1;
                    rect.Y = bounds.Bottom - rect.Height;
                    flag = bounds.Height > (firstGradientWidth + secondGradientWidth);
                }
                if (flag)
                {
                    using (Brush brush = new SolidBrush(middleColor))
                    {
                        g.FillRectangle(brush, bounds);
                    }
                    using (Brush brush2 = new LinearGradientBrush(rectangle2, beginColor, middleColor, mode))
                    {
                        g.FillRectangle(brush2, rectangle2);
                    }
                    using (LinearGradientBrush brush3 = new LinearGradientBrush(rect, middleColor, endColor, mode))
                    {
                        if (mode == LinearGradientMode.Horizontal)
                        {
                            rect.X++;
                            rect.Width--;
                        }
                        else
                        {
                            rect.Y++;
                            rect.Height--;
                        }
                        g.FillRectangle(brush3, rect);
                        return;
                    }
                }
                using (Brush brush4 = new LinearGradientBrush(bounds, beginColor, endColor, mode))
                {
                    g.FillRectangle(brush4, bounds);
                }
            }
        }

        internal override Region GetTransparentRegion(ToolStrip toolStrip)
        {
            Rectangle rectangle7;
            Rectangle rectangle8;
            if (((toolStrip is ToolStripDropDown) || (toolStrip is MenuStrip)) || (toolStrip is StatusStrip))
            {
                return null;
            }
            if (!this.RoundedEdges)
            {
                return null;
            }
            Rectangle rectangle = new Rectangle(Point.Empty, toolStrip.Size);
            if (toolStrip.ParentInternal == null)
            {
                return null;
            }
            Point empty = Point.Empty;
            Point point2 = new Point(rectangle.Width - 1, 0);
            Point location = new Point(0, rectangle.Height - 1);
            Point point4 = new Point(rectangle.Width - 1, rectangle.Height - 1);
            Rectangle rect = new Rectangle(empty, onePix);
            Rectangle rectangle3 = new Rectangle(location, new Size(2, 1));
            Rectangle rectangle4 = new Rectangle(location.X, location.Y - 1, 1, 2);
            Rectangle rectangle5 = new Rectangle(point4.X - 1, point4.Y, 2, 1);
            Rectangle rectangle6 = new Rectangle(point4.X, point4.Y - 1, 1, 2);
            if (toolStrip.OverflowButton.Visible)
            {
                rectangle7 = new Rectangle(point2.X - 1, point2.Y, 1, 1);
                rectangle8 = new Rectangle(point2.X, point2.Y, 1, 2);
            }
            else
            {
                rectangle7 = new Rectangle(point2.X - 2, point2.Y, 2, 1);
                rectangle8 = new Rectangle(point2.X, point2.Y, 1, 3);
            }
            Region region = new Region(rect);
            region.Union(rect);
            region.Union(rectangle3);
            region.Union(rectangle4);
            region.Union(rectangle5);
            region.Union(rectangle6);
            region.Union(rectangle7);
            region.Union(rectangle8);
            return region;
        }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            if (this.RendererOverride != null)
            {
                base.OnRenderArrow(e);
            }
            else
            {
                ToolStripItem item = e.Item;
                if (item is ToolStripDropDownItem)
                {
                    e.DefaultArrowColor = item.Enabled ? SystemColors.ControlText : SystemColors.ControlDark;
                }
                base.OnRenderArrow(e);
            }
        }

        protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
        {
            if (this.RendererOverride != null)
            {
                base.OnRenderButtonBackground(e);
            }
            else
            {
                ToolStripButton item = e.Item as ToolStripButton;
                Graphics g = e.Graphics;
                Rectangle bounds = new Rectangle(Point.Empty, item.Size);
                if (item.CheckState == CheckState.Unchecked)
                {
                    this.RenderItemInternal(e, true);
                }
                else
                {
                    Rectangle clipRect = item.Selected ? item.ContentRectangle : bounds;
                    if (item.BackgroundImage != null)
                    {
                        ControlPaint.DrawBackgroundImage(g, item.BackgroundImage, item.BackColor, item.BackgroundImageLayout, bounds, clipRect);
                    }
                    if (this.UseSystemColors)
                    {
                        if (item.Selected)
                        {
                            this.RenderPressedButtonFill(g, bounds);
                        }
                        else
                        {
                            this.RenderCheckedButtonFill(g, bounds);
                        }
                        using (Pen pen = new Pen(this.ColorTable.ButtonSelectedBorder))
                        {
                            g.DrawRectangle(pen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
                            return;
                        }
                    }
                    if (item.Selected)
                    {
                        this.RenderPressedButtonFill(g, bounds);
                    }
                    else
                    {
                        this.RenderCheckedButtonFill(g, bounds);
                    }
                    using (Pen pen2 = new Pen(this.ColorTable.ButtonSelectedBorder))
                    {
                        g.DrawRectangle(pen2, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
                    }
                }
            }
        }

        protected override void OnRenderDropDownButtonBackground(ToolStripItemRenderEventArgs e)
        {
            if (this.RendererOverride != null)
            {
                base.OnRenderDropDownButtonBackground(e);
            }
            else
            {
                ToolStripDropDownItem item = e.Item as ToolStripDropDownItem;
                if (((item != null) && item.Pressed) && item.HasDropDownItems)
                {
                    Rectangle bounds = new Rectangle(Point.Empty, item.Size);
                    this.RenderPressedGradient(e.Graphics, bounds);
                }
                else
                {
                    this.RenderItemInternal(e, true);
                }
            }
        }

        protected override void OnRenderGrip(ToolStripGripRenderEventArgs e)
        {
            if (this.RendererOverride != null)
            {
                base.OnRenderGrip(e);
            }
            else
            {
                Graphics graphics = e.Graphics;
                Rectangle gripBounds = e.GripBounds;
                ToolStrip toolStrip = e.ToolStrip;
                bool flag = e.ToolStrip.RightToLeft == RightToLeft.Yes;
                int num = (toolStrip.Orientation == Orientation.Horizontal) ? gripBounds.Height : gripBounds.Width;
                int num2 = (toolStrip.Orientation == Orientation.Horizontal) ? gripBounds.Width : gripBounds.Height;
                int num3 = (num - 8) / 4;
                if (num3 > 0)
                {
                    int num4 = (toolStrip is MenuStrip) ? 2 : 0;
                    Rectangle[] rects = new Rectangle[num3];
                    int y = 5 + num4;
                    int x = num2 / 2;
                    for (int i = 0; i < num3; i++)
                    {
                        rects[i] = (toolStrip.Orientation == Orientation.Horizontal) ? new Rectangle(x, y, 2, 2) : new Rectangle(y, x, 2, 2);
                        y += 4;
                    }
                    int num8 = flag ? 1 : -1;
                    if (flag)
                    {
                        for (int k = 0; k < num3; k++)
                        {
                            rects[k].Offset(-num8, 0);
                        }
                    }
                    using (Brush brush = new SolidBrush(this.ColorTable.GripLight))
                    {
                        graphics.FillRectangles(brush, rects);
                    }
                    for (int j = 0; j < num3; j++)
                    {
                        rects[j].Offset(num8, -1);
                    }
                    using (Brush brush2 = new SolidBrush(this.ColorTable.GripDark))
                    {
                        graphics.FillRectangles(brush2, rects);
                    }
                }
            }
        }

        protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
        {
            if (this.RendererOverride != null)
            {
                base.OnRenderImageMargin(e);
            }
            else
            {
                Graphics graphics = e.Graphics;
                Rectangle affectedBounds = e.AffectedBounds;
                affectedBounds.Y += 2;
                affectedBounds.Height -= 4;
                RightToLeft rightToLeft = e.ToolStrip.RightToLeft;
                Color beginColor = (rightToLeft == RightToLeft.No) ? this.ColorTable.ImageMarginGradientBegin : this.ColorTable.ImageMarginGradientEnd;
                Color endColor = (rightToLeft == RightToLeft.No) ? this.ColorTable.ImageMarginGradientEnd : this.ColorTable.ImageMarginGradientBegin;
                this.FillWithDoubleGradient(beginColor, this.ColorTable.ImageMarginGradientMiddle, endColor, e.Graphics, affectedBounds, 12, 12, LinearGradientMode.Horizontal, e.ToolStrip.RightToLeft == RightToLeft.Yes);
            }
        }

        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            if (this.RendererOverride != null)
            {
                base.OnRenderItemCheck(e);
            }
            else
            {
                this.RenderCheckBackground(e);
                base.OnRenderItemCheck(e);
            }
        }

        protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)
        {
            if (this.RendererOverride != null)
            {
                base.OnRenderItemImage(e);
            }
            else
            {
                Rectangle imageRectangle = e.ImageRectangle;
                Image image = e.Image;
                if (e.Item is ToolStripMenuItem)
                {
                    ToolStripMenuItem item = e.Item as ToolStripMenuItem;
                    if (item.CheckState != CheckState.Unchecked)
                    {
                        ToolStripDropDownMenu parentInternal = item.ParentInternal as ToolStripDropDownMenu;
                        if (((parentInternal != null) && !parentInternal.ShowCheckMargin) && parentInternal.ShowImageMargin)
                        {
                            this.RenderCheckBackground(e);
                        }
                    }
                }
                if ((imageRectangle != Rectangle.Empty) && (image != null))
                {
                    if (!e.Item.Enabled)
                    {
                        base.OnRenderItemImage(e);
                    }
                    else if (e.Item.ImageScaling == ToolStripItemImageScaling.None)
                    {
                        e.Graphics.DrawImage(image, imageRectangle, new Rectangle(Point.Empty, imageRectangle.Size), GraphicsUnit.Pixel);
                    }
                    else
                    {
                        e.Graphics.DrawImage(image, imageRectangle);
                    }
                }
            }
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            if (this.RendererOverride != null)
            {
                base.OnRenderItemText(e);
            }
            else
            {
                if ((e.Item is ToolStripMenuItem) && (e.Item.Selected || e.Item.Pressed))
                {
                    e.DefaultTextColor = e.Item.ForeColor;
                }
                base.OnRenderItemText(e);
            }
        }

        protected override void OnRenderLabelBackground(ToolStripItemRenderEventArgs e)
        {
            if (this.RendererOverride != null)
            {
                base.OnRenderLabelBackground(e);
            }
            else
            {
                RenderLabelInternal(e);
            }
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (this.RendererOverride != null)
            {
                base.OnRenderMenuItemBackground(e);
            }
            else
            {
                ToolStripItem item = e.Item;
                Graphics g = e.Graphics;
                Rectangle rect = new Rectangle(Point.Empty, item.Size);
                if (((rect.Width != 0) && (rect.Height != 0)) && !(item is MdiControlStrip.SystemMenuItem))
                {
                    if (item.IsOnDropDown)
                    {
                        rect = LayoutUtils.DeflateRect(rect, this.dropDownMenuItemPaintPadding);
                        if (item.Selected)
                        {
                            Color menuItemBorder = this.ColorTable.MenuItemBorder;
                            if (item.Enabled)
                            {
                                if (this.UseSystemColors)
                                {
                                    menuItemBorder = SystemColors.Highlight;
                                    this.RenderSelectedButtonFill(g, rect);
                                }
                                else
                                {
                                    using (Brush brush = new SolidBrush(this.ColorTable.MenuItemSelected))
                                    {
                                        g.FillRectangle(brush, rect);
                                    }
                                }
                            }
                            using (Pen pen = new Pen(menuItemBorder))
                            {
                                g.DrawRectangle(pen, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
                                return;
                            }
                        }
                        Rectangle clipRect = rect;
                        if (item.BackgroundImage != null)
                        {
                            ControlPaint.DrawBackgroundImage(g, item.BackgroundImage, item.BackColor, item.BackgroundImageLayout, rect, clipRect);
                            return;
                        }
                        if ((item.Owner == null) || !(item.BackColor != item.Owner.BackColor))
                        {
                            return;
                        }
                        using (Brush brush2 = new SolidBrush(item.BackColor))
                        {
                            g.FillRectangle(brush2, clipRect);
                            return;
                        }
                    }
                    if (item.Pressed)
                    {
                        this.RenderPressedGradient(g, rect);
                    }
                    else
                    {
                        if (item.Selected)
                        {
                            Color color = this.ColorTable.MenuItemBorder;
                            if (item.Enabled)
                            {
                                if (this.UseSystemColors)
                                {
                                    color = SystemColors.Highlight;
                                    this.RenderSelectedButtonFill(g, rect);
                                }
                                else
                                {
                                    using (Brush brush3 = new LinearGradientBrush(rect, this.ColorTable.MenuItemSelectedGradientBegin, this.ColorTable.MenuItemSelectedGradientEnd, LinearGradientMode.Vertical))
                                    {
                                        g.FillRectangle(brush3, rect);
                                    }
                                }
                            }
                            using (Pen pen2 = new Pen(color))
                            {
                                g.DrawRectangle(pen2, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
                                return;
                            }
                        }
                        Rectangle rectangle3 = rect;
                        if (item.BackgroundImage != null)
                        {
                            ControlPaint.DrawBackgroundImage(g, item.BackgroundImage, item.BackColor, item.BackgroundImageLayout, rect, rectangle3);
                        }
                        else if ((item.Owner != null) && (item.BackColor != item.Owner.BackColor))
                        {
                            using (Brush brush4 = new SolidBrush(item.BackColor))
                            {
                                g.FillRectangle(brush4, rectangle3);
                            }
                        }
                    }
                }
            }
        }

        protected override void OnRenderOverflowButtonBackground(ToolStripItemRenderEventArgs e)
        {
            if (this.RendererOverride != null)
            {
                base.OnRenderOverflowButtonBackground(e);
            }
            else
            {
                ToolStripItem item = e.Item;
                Graphics g = e.Graphics;
                bool rightToLeft = item.RightToLeft == RightToLeft.Yes;
                this.RenderOverflowBackground(e, rightToLeft);
                bool flag2 = e.ToolStrip.Orientation == Orientation.Horizontal;
                Rectangle empty = Rectangle.Empty;
                if (rightToLeft)
                {
                    empty = new Rectangle(0, item.Height - 8, 9, 5);
                }
                else
                {
                    empty = new Rectangle(item.Width - 12, item.Height - 8, 9, 5);
                }
                ArrowDirection direction = flag2 ? ArrowDirection.Down : ArrowDirection.Right;
                int x = (rightToLeft && flag2) ? -1 : 1;
                empty.Offset(x, 1);
                this.RenderArrowInternal(g, empty, direction, SystemBrushes.ButtonHighlight);
                empty.Offset(-1 * x, -1);
                this.RenderArrowInternal(g, empty, direction, SystemBrushes.ControlText);
                if (flag2)
                {
                    x = rightToLeft ? -2 : 0;
                    g.DrawLine(SystemPens.ControlText, (int) (empty.Right - 6), (int) (empty.Y - 2), (int) (empty.Right - 2), (int) (empty.Y - 2));
                    g.DrawLine(SystemPens.ButtonHighlight, (int) ((empty.Right - 5) + x), (int) (empty.Y - 1), (int) ((empty.Right - 1) + x), (int) (empty.Y - 1));
                }
                else
                {
                    g.DrawLine(SystemPens.ControlText, empty.X, empty.Y, empty.X, empty.Bottom - 1);
                    g.DrawLine(SystemPens.ButtonHighlight, empty.X + 1, empty.Y + 1, empty.X + 1, empty.Bottom);
                }
            }
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            if (this.RendererOverride != null)
            {
                base.OnRenderSeparator(e);
            }
            else
            {
                this.RenderSeparatorInternal(e.Graphics, e.Item, new Rectangle(Point.Empty, e.Item.Size), e.Vertical);
            }
        }

        protected override void OnRenderSplitButtonBackground(ToolStripItemRenderEventArgs e)
        {
            if (this.RendererOverride != null)
            {
                base.OnRenderSplitButtonBackground(e);
            }
            else
            {
                ToolStripSplitButton item = e.Item as ToolStripSplitButton;
                Graphics g = e.Graphics;
                if (item != null)
                {
                    Rectangle bounds = new Rectangle(Point.Empty, item.Size);
                    if (item.BackgroundImage != null)
                    {
                        Rectangle clipRect = item.Selected ? item.ContentRectangle : bounds;
                        ControlPaint.DrawBackgroundImage(g, item.BackgroundImage, item.BackColor, item.BackgroundImageLayout, bounds, clipRect);
                    }
                    bool flag = ((item.Pressed || item.ButtonPressed) || item.Selected) || item.ButtonSelected;
                    if (flag)
                    {
                        this.RenderItemInternal(e, true);
                    }
                    if (item.ButtonPressed)
                    {
                        Rectangle buttonBounds = item.ButtonBounds;
                        Padding padding = (item.RightToLeft == RightToLeft.Yes) ? new Padding(0, 1, 1, 1) : new Padding(1, 1, 0, 1);
                        buttonBounds = LayoutUtils.DeflateRect(buttonBounds, padding);
                        this.RenderPressedButtonFill(g, buttonBounds);
                    }
                    else if (item.Pressed)
                    {
                        this.RenderPressedGradient(e.Graphics, bounds);
                    }
                    Rectangle dropDownButtonBounds = item.DropDownButtonBounds;
                    if (flag && !item.Pressed)
                    {
                        using (Brush brush = new SolidBrush(this.ColorTable.ButtonSelectedBorder))
                        {
                            g.FillRectangle(brush, item.SplitterBounds);
                        }
                    }
                    base.DrawArrow(new ToolStripArrowRenderEventArgs(g, item, dropDownButtonBounds, SystemColors.ControlText, ArrowDirection.Down));
                }
            }
        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            if (this.RendererOverride != null)
            {
                base.OnRenderToolStripBackground(e);
            }
            else
            {
                ToolStrip toolStrip = e.ToolStrip;
                if (base.ShouldPaintBackground(toolStrip))
                {
                    if (toolStrip is ToolStripDropDown)
                    {
                        this.RenderToolStripDropDownBackground(e);
                    }
                    else if (toolStrip is MenuStrip)
                    {
                        this.RenderMenuStripBackground(e);
                    }
                    else if (toolStrip is StatusStrip)
                    {
                        this.RenderStatusStripBackground(e);
                    }
                    else
                    {
                        this.RenderToolStripBackgroundInternal(e);
                    }
                }
            }
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            if (this.RendererOverride != null)
            {
                base.OnRenderToolStripBorder(e);
            }
            else
            {
                ToolStrip toolStrip = e.ToolStrip;
                Graphics graphics = e.Graphics;
                if (toolStrip is ToolStripDropDown)
                {
                    this.RenderToolStripDropDownBorder(e);
                }
                else if (!(toolStrip is MenuStrip))
                {
                    if (toolStrip is StatusStrip)
                    {
                        this.RenderStatusStripBorder(e);
                    }
                    else
                    {
                        Rectangle rectangle = new Rectangle(Point.Empty, toolStrip.Size);
                        using (Pen pen = new Pen(this.ColorTable.ToolStripBorder))
                        {
                            if (toolStrip.Orientation == Orientation.Horizontal)
                            {
                                graphics.DrawLine(pen, rectangle.Left, rectangle.Height - 1, rectangle.Right, rectangle.Height - 1);
                                if (this.RoundedEdges)
                                {
                                    graphics.DrawLine(pen, (int) (rectangle.Width - 2), (int) (rectangle.Height - 2), (int) (rectangle.Width - 1), (int) (rectangle.Height - 3));
                                }
                            }
                            else
                            {
                                graphics.DrawLine(pen, rectangle.Width - 1, 0, rectangle.Width - 1, rectangle.Height - 1);
                                if (this.RoundedEdges)
                                {
                                    graphics.DrawLine(pen, (int) (rectangle.Width - 2), (int) (rectangle.Height - 2), (int) (rectangle.Width - 1), (int) (rectangle.Height - 3));
                                }
                            }
                        }
                        if (this.RoundedEdges)
                        {
                            if (toolStrip.OverflowButton.Visible)
                            {
                                this.RenderOverflowButtonEffectsOverBorder(e);
                            }
                            else
                            {
                                Rectangle empty = Rectangle.Empty;
                                if (toolStrip.Orientation == Orientation.Horizontal)
                                {
                                    empty = new Rectangle(rectangle.Width - 1, 3, 1, rectangle.Height - 3);
                                }
                                else
                                {
                                    empty = new Rectangle(3, rectangle.Height - 1, rectangle.Width - 3, rectangle.Height - 1);
                                }
                                this.FillWithDoubleGradient(this.ColorTable.OverflowButtonGradientBegin, this.ColorTable.OverflowButtonGradientMiddle, this.ColorTable.OverflowButtonGradientEnd, e.Graphics, empty, 12, 12, LinearGradientMode.Vertical, false);
                                this.RenderToolStripCurve(e);
                            }
                        }
                    }
                }
            }
        }

        protected override void OnRenderToolStripContentPanelBackground(ToolStripContentPanelRenderEventArgs e)
        {
            if (this.RendererOverride != null)
            {
                base.OnRenderToolStripContentPanelBackground(e);
            }
            else
            {
                ToolStripContentPanel toolStripContentPanel = e.ToolStripContentPanel;
                if (base.ShouldPaintBackground(toolStripContentPanel) && !SystemInformation.InLockedTerminalSession())
                {
                    e.Handled = true;
                    e.Graphics.Clear(this.ColorTable.ToolStripContentPanelGradientEnd);
                }
            }
        }

        protected override void OnRenderToolStripPanelBackground(ToolStripPanelRenderEventArgs e)
        {
            if (this.RendererOverride != null)
            {
                base.OnRenderToolStripPanelBackground(e);
            }
            else
            {
                ToolStripPanel toolStripPanel = e.ToolStripPanel;
                if (base.ShouldPaintBackground(toolStripPanel))
                {
                    e.Handled = true;
                    this.RenderBackgroundGradient(e.Graphics, toolStripPanel, this.ColorTable.ToolStripPanelGradientBegin, this.ColorTable.ToolStripPanelGradientEnd);
                }
            }
        }

        protected override void OnRenderToolStripStatusLabelBackground(ToolStripItemRenderEventArgs e)
        {
            if (this.RendererOverride != null)
            {
                base.OnRenderToolStripStatusLabelBackground(e);
            }
            else
            {
                RenderLabelInternal(e);
                ToolStripStatusLabel item = e.Item as ToolStripStatusLabel;
                ControlPaint.DrawBorder3D(e.Graphics, new Rectangle(0, 0, item.Width, item.Height), item.BorderStyle, (Border3DSide) item.BorderSides);
            }
        }

        internal void RenderArrowInternal(Graphics g, Rectangle dropDownRect, ArrowDirection direction, Brush brush)
        {
            Point point;
            point = new Point(dropDownRect.Left + (dropDownRect.Width / 2), dropDownRect.Top + (dropDownRect.Height / 2)) {
                X = point.X + (dropDownRect.Width % 2)
            };
            Point[] points = null;
            switch (direction)
            {
                case ArrowDirection.Left:
                    points = new Point[] { new Point(point.X + 2, point.Y - 3), new Point(point.X + 2, point.Y + 3), new Point(point.X - 1, point.Y) };
                    break;

                case ArrowDirection.Up:
                    points = new Point[] { new Point(point.X - 2, point.Y + 1), new Point(point.X + 3, point.Y + 1), new Point(point.X, point.Y - 2) };
                    break;

                case ArrowDirection.Right:
                    points = new Point[] { new Point(point.X - 2, point.Y - 3), new Point(point.X - 2, point.Y + 3), new Point(point.X + 1, point.Y) };
                    break;

                default:
                    points = new Point[] { new Point(point.X - 2, point.Y - 1), new Point(point.X + 3, point.Y - 1), new Point(point.X, point.Y + 2) };
                    break;
            }
            g.FillPolygon(brush, points);
        }

        private void RenderBackgroundGradient(Graphics g, Control control, Color beginColor, Color endColor)
        {
            this.RenderBackgroundGradient(g, control, beginColor, endColor, Orientation.Horizontal);
        }

        private void RenderBackgroundGradient(Graphics g, Control control, Color beginColor, Color endColor, Orientation orientation)
        {
            if (control.RightToLeft == RightToLeft.Yes)
            {
                Color color = beginColor;
                beginColor = endColor;
                endColor = color;
            }
            if (orientation == Orientation.Horizontal)
            {
                Control parentInternal = control.ParentInternal;
                if (parentInternal != null)
                {
                    Rectangle rectangle = new Rectangle(Point.Empty, parentInternal.Size);
                    if (LayoutUtils.IsZeroWidthOrHeight(rectangle))
                    {
                        return;
                    }
                    using (LinearGradientBrush brush = new LinearGradientBrush(rectangle, beginColor, endColor, LinearGradientMode.Horizontal))
                    {
                        brush.TranslateTransform((float) (parentInternal.Width - control.Location.X), (float) (parentInternal.Height - control.Location.Y));
                        g.FillRectangle(brush, new Rectangle(Point.Empty, control.Size));
                        return;
                    }
                }
                Rectangle rectangle2 = new Rectangle(Point.Empty, control.Size);
                if (LayoutUtils.IsZeroWidthOrHeight(rectangle2))
                {
                    return;
                }
                using (LinearGradientBrush brush2 = new LinearGradientBrush(rectangle2, beginColor, endColor, LinearGradientMode.Horizontal))
                {
                    g.FillRectangle(brush2, rectangle2);
                    return;
                }
            }
            using (Brush brush3 = new SolidBrush(beginColor))
            {
                g.FillRectangle(brush3, new Rectangle(Point.Empty, control.Size));
            }
        }

        private void RenderCheckBackground(ToolStripItemImageRenderEventArgs e)
        {
            Rectangle rect = new Rectangle(e.ImageRectangle.Left - 2, 1, e.ImageRectangle.Width + 4, e.Item.Height - 2);
            Graphics g = e.Graphics;
            if (!this.UseSystemColors)
            {
                Color color = e.Item.Selected ? this.ColorTable.CheckSelectedBackground : this.ColorTable.CheckBackground;
                color = e.Item.Pressed ? this.ColorTable.CheckPressedBackground : color;
                using (Brush brush = new SolidBrush(color))
                {
                    g.FillRectangle(brush, rect);
                }
                using (Pen pen = new Pen(this.ColorTable.ButtonSelectedBorder))
                {
                    g.DrawRectangle(pen, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
                    return;
                }
            }
            if (e.Item.Pressed)
            {
                this.RenderPressedButtonFill(g, rect);
            }
            else
            {
                this.RenderSelectedButtonFill(g, rect);
            }
            g.DrawRectangle(SystemPens.Highlight, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
        }

        private void RenderCheckedButtonFill(Graphics g, Rectangle bounds)
        {
            if ((bounds.Width != 0) && (bounds.Height != 0))
            {
                if (!this.UseSystemColors)
                {
                    using (Brush brush = new LinearGradientBrush(bounds, this.ColorTable.ButtonCheckedGradientBegin, this.ColorTable.ButtonCheckedGradientEnd, LinearGradientMode.Vertical))
                    {
                        g.FillRectangle(brush, bounds);
                        return;
                    }
                }
                using (Brush brush2 = new SolidBrush(this.ColorTable.ButtonCheckedHighlight))
                {
                    g.FillRectangle(brush2, bounds);
                }
            }
        }

        private void RenderItemInternal(ToolStripItemRenderEventArgs e, bool useHotBorder)
        {
            Graphics g = e.Graphics;
            ToolStripItem item = e.Item;
            Rectangle bounds = new Rectangle(Point.Empty, item.Size);
            bool flag = false;
            Rectangle clipRect = item.Selected ? item.ContentRectangle : bounds;
            if (item.BackgroundImage != null)
            {
                ControlPaint.DrawBackgroundImage(g, item.BackgroundImage, item.BackColor, item.BackgroundImageLayout, bounds, clipRect);
            }
            if (item.Pressed)
            {
                this.RenderPressedButtonFill(g, bounds);
                flag = useHotBorder;
            }
            else if (item.Selected)
            {
                this.RenderSelectedButtonFill(g, bounds);
                flag = useHotBorder;
            }
            else if ((item.Owner != null) && (item.BackColor != item.Owner.BackColor))
            {
                using (Brush brush = new SolidBrush(item.BackColor))
                {
                    g.FillRectangle(brush, bounds);
                }
            }
            if (flag)
            {
                using (Pen pen = new Pen(this.ColorTable.ButtonSelectedBorder))
                {
                    g.DrawRectangle(pen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
                }
            }
        }

        private static void RenderLabelInternal(ToolStripItemRenderEventArgs e)
        {
            Graphics g = e.Graphics;
            ToolStripItem item = e.Item;
            Rectangle bounds = new Rectangle(Point.Empty, item.Size);
            Rectangle clipRect = item.Selected ? item.ContentRectangle : bounds;
            if (item.BackgroundImage != null)
            {
                ControlPaint.DrawBackgroundImage(g, item.BackgroundImage, item.BackColor, item.BackgroundImageLayout, bounds, clipRect);
            }
        }

        private void RenderMenuStripBackground(ToolStripRenderEventArgs e)
        {
            this.RenderBackgroundGradient(e.Graphics, e.ToolStrip, this.ColorTable.MenuStripGradientBegin, this.ColorTable.MenuStripGradientEnd, e.ToolStrip.Orientation);
        }

        private void RenderOverflowBackground(ToolStripItemRenderEventArgs e, bool rightToLeft)
        {
            Color buttonPressedGradientBegin;
            Color buttonPressedGradientMiddle;
            Color buttonPressedGradientEnd;
            Color buttonSelectedGradientMiddle;
            Color color5;
            Graphics g = e.Graphics;
            ToolStripOverflowButton item = e.Item as ToolStripOverflowButton;
            Rectangle bounds = new Rectangle(Point.Empty, e.Item.Size);
            Rectangle withinBounds = bounds;
            bool flag = this.RoundedEdges && !(item.GetCurrentParent() is MenuStrip);
            bool flag2 = e.ToolStrip.Orientation == Orientation.Horizontal;
            if (flag2)
            {
                bounds.X += (bounds.Width - 12) + 1;
                bounds.Width = 12;
                if (rightToLeft)
                {
                    bounds = LayoutUtils.RTLTranslate(bounds, withinBounds);
                }
            }
            else
            {
                bounds.Y = (bounds.Height - 12) + 1;
                bounds.Height = 12;
            }
            if (item.Pressed)
            {
                buttonPressedGradientBegin = this.ColorTable.ButtonPressedGradientBegin;
                buttonPressedGradientMiddle = this.ColorTable.ButtonPressedGradientMiddle;
                buttonPressedGradientEnd = this.ColorTable.ButtonPressedGradientEnd;
                buttonSelectedGradientMiddle = this.ColorTable.ButtonPressedGradientBegin;
                color5 = buttonSelectedGradientMiddle;
            }
            else if (item.Selected)
            {
                buttonPressedGradientBegin = this.ColorTable.ButtonSelectedGradientBegin;
                buttonPressedGradientMiddle = this.ColorTable.ButtonSelectedGradientMiddle;
                buttonPressedGradientEnd = this.ColorTable.ButtonSelectedGradientEnd;
                buttonSelectedGradientMiddle = this.ColorTable.ButtonSelectedGradientMiddle;
                color5 = buttonSelectedGradientMiddle;
            }
            else
            {
                buttonPressedGradientBegin = this.ColorTable.OverflowButtonGradientBegin;
                buttonPressedGradientMiddle = this.ColorTable.OverflowButtonGradientMiddle;
                buttonPressedGradientEnd = this.ColorTable.OverflowButtonGradientEnd;
                buttonSelectedGradientMiddle = this.ColorTable.ToolStripBorder;
                color5 = flag2 ? this.ColorTable.ToolStripGradientMiddle : this.ColorTable.ToolStripGradientEnd;
            }
            if (flag)
            {
                using (Pen pen = new Pen(buttonSelectedGradientMiddle))
                {
                    Point point = new Point(bounds.Left - 1, bounds.Height - 2);
                    Point point2 = new Point(bounds.Left, bounds.Height - 2);
                    if (rightToLeft)
                    {
                        point.X = bounds.Right + 1;
                        point2.X = bounds.Right;
                    }
                    g.DrawLine(pen, point, point2);
                }
            }
            LinearGradientMode mode = flag2 ? LinearGradientMode.Vertical : LinearGradientMode.Horizontal;
            this.FillWithDoubleGradient(buttonPressedGradientBegin, buttonPressedGradientMiddle, buttonPressedGradientEnd, g, bounds, 12, 12, mode, false);
            if (flag)
            {
                using (Brush brush = new SolidBrush(color5))
                {
                    if (flag2)
                    {
                        Point point3 = new Point(bounds.X - 2, 0);
                        Point point4 = new Point(bounds.X - 1, 1);
                        if (rightToLeft)
                        {
                            point3.X = bounds.Right + 1;
                            point4.X = bounds.Right;
                        }
                        g.FillRectangle(brush, point3.X, point3.Y, 1, 1);
                        g.FillRectangle(brush, point4.X, point4.Y, 1, 1);
                    }
                    else
                    {
                        g.FillRectangle(brush, bounds.Width - 3, bounds.Top - 1, 1, 1);
                        g.FillRectangle(brush, bounds.Width - 2, bounds.Top - 2, 1, 1);
                    }
                }
                using (Brush brush2 = new SolidBrush(buttonPressedGradientBegin))
                {
                    if (flag2)
                    {
                        Rectangle rect = new Rectangle(bounds.X - 1, 0, 1, 1);
                        if (rightToLeft)
                        {
                            rect.X = bounds.Right;
                        }
                        g.FillRectangle(brush2, rect);
                    }
                    else
                    {
                        g.FillRectangle(brush2, bounds.X, bounds.Top - 1, 1, 1);
                    }
                }
            }
        }

        private void RenderOverflowButtonEffectsOverBorder(ToolStripRenderEventArgs e)
        {
            ToolStrip toolStrip = e.ToolStrip;
            ToolStripItem overflowButton = toolStrip.OverflowButton;
            if (overflowButton.Visible)
            {
                Color buttonPressedGradientBegin;
                Color toolStripGradientMiddle;
                Graphics graphics = e.Graphics;
                if (overflowButton.Pressed)
                {
                    buttonPressedGradientBegin = this.ColorTable.ButtonPressedGradientBegin;
                    toolStripGradientMiddle = buttonPressedGradientBegin;
                }
                else if (overflowButton.Selected)
                {
                    buttonPressedGradientBegin = this.ColorTable.ButtonSelectedGradientMiddle;
                    toolStripGradientMiddle = buttonPressedGradientBegin;
                }
                else
                {
                    buttonPressedGradientBegin = this.ColorTable.ToolStripBorder;
                    toolStripGradientMiddle = this.ColorTable.ToolStripGradientMiddle;
                }
                using (Brush brush = new SolidBrush(buttonPressedGradientBegin))
                {
                    graphics.FillRectangle(brush, toolStrip.Width - 1, toolStrip.Height - 2, 1, 1);
                    graphics.FillRectangle(brush, toolStrip.Width - 2, toolStrip.Height - 1, 1, 1);
                }
                using (Brush brush2 = new SolidBrush(toolStripGradientMiddle))
                {
                    graphics.FillRectangle(brush2, toolStrip.Width - 2, 0, 1, 1);
                    graphics.FillRectangle(brush2, toolStrip.Width - 1, 1, 1, 1);
                }
            }
        }

        private void RenderPressedButtonFill(Graphics g, Rectangle bounds)
        {
            if ((bounds.Width != 0) && (bounds.Height != 0))
            {
                if (!this.UseSystemColors)
                {
                    using (Brush brush = new LinearGradientBrush(bounds, this.ColorTable.ButtonPressedGradientBegin, this.ColorTable.ButtonPressedGradientEnd, LinearGradientMode.Vertical))
                    {
                        g.FillRectangle(brush, bounds);
                        return;
                    }
                }
                using (Brush brush2 = new SolidBrush(this.ColorTable.ButtonPressedHighlight))
                {
                    g.FillRectangle(brush2, bounds);
                }
            }
        }

        private void RenderPressedGradient(Graphics g, Rectangle bounds)
        {
            if ((bounds.Width != 0) && (bounds.Height != 0))
            {
                using (Brush brush = new LinearGradientBrush(bounds, this.ColorTable.MenuItemPressedGradientBegin, this.ColorTable.MenuItemPressedGradientEnd, LinearGradientMode.Vertical))
                {
                    g.FillRectangle(brush, bounds);
                }
                using (Pen pen = new Pen(this.ColorTable.MenuBorder))
                {
                    g.DrawRectangle(pen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
                }
            }
        }

        private void RenderSelectedButtonFill(Graphics g, Rectangle bounds)
        {
            if ((bounds.Width != 0) && (bounds.Height != 0))
            {
                if (!this.UseSystemColors)
                {
                    using (Brush brush = new LinearGradientBrush(bounds, this.ColorTable.ButtonSelectedGradientBegin, this.ColorTable.ButtonSelectedGradientEnd, LinearGradientMode.Vertical))
                    {
                        g.FillRectangle(brush, bounds);
                        return;
                    }
                }
                using (Brush brush2 = new SolidBrush(this.ColorTable.ButtonSelectedHighlight))
                {
                    g.FillRectangle(brush2, bounds);
                }
            }
        }

        private void RenderSeparatorInternal(Graphics g, ToolStripItem item, Rectangle bounds, bool vertical)
        {
            Color separatorDark = this.ColorTable.SeparatorDark;
            Color separatorLight = this.ColorTable.SeparatorLight;
            Pen pen = new Pen(separatorDark);
            Pen pen2 = new Pen(separatorLight);
            bool flag = true;
            bool flag2 = true;
            bool flag3 = item is ToolStripSeparator;
            bool flag4 = false;
            if (flag3)
            {
                if (vertical)
                {
                    if (!item.IsOnDropDown)
                    {
                        bounds.Y += 3;
                        bounds.Height = Math.Max(0, bounds.Height - 6);
                    }
                }
                else
                {
                    ToolStripDropDownMenu currentParent = item.GetCurrentParent() as ToolStripDropDownMenu;
                    if (currentParent != null)
                    {
                        if (currentParent.RightToLeft == RightToLeft.No)
                        {
                            bounds.X += currentParent.Padding.Left - 2;
                            bounds.Width = currentParent.Width - bounds.X;
                        }
                        else
                        {
                            bounds.X += 2;
                            bounds.Width = (currentParent.Width - bounds.X) - currentParent.Padding.Right;
                        }
                    }
                    else
                    {
                        flag4 = true;
                    }
                }
            }
            try
            {
                if (vertical)
                {
                    if (bounds.Height >= 4)
                    {
                        bounds.Inflate(0, -2);
                    }
                    bool flag5 = item.RightToLeft == RightToLeft.Yes;
                    Pen pen3 = flag5 ? pen2 : pen;
                    Pen pen4 = flag5 ? pen : pen2;
                    int num = bounds.Width / 2;
                    g.DrawLine(pen3, num, bounds.Top, num, bounds.Bottom - 1);
                    num++;
                    g.DrawLine(pen4, num, bounds.Top + 1, num, bounds.Bottom);
                }
                else
                {
                    if (flag4 && (bounds.Width >= 4))
                    {
                        bounds.Inflate(-2, 0);
                    }
                    int num2 = bounds.Height / 2;
                    g.DrawLine(pen, bounds.Left, num2, bounds.Right - 1, num2);
                    if (!flag3 || flag4)
                    {
                        num2++;
                        g.DrawLine(pen2, bounds.Left + 1, num2, bounds.Right - 1, num2);
                    }
                }
            }
            finally
            {
                if (flag && (pen != null))
                {
                    pen.Dispose();
                }
                if (flag2 && (pen2 != null))
                {
                    pen2.Dispose();
                }
            }
        }

        private void RenderStatusStripBackground(ToolStripRenderEventArgs e)
        {
            StatusStrip toolStrip = e.ToolStrip as StatusStrip;
            this.RenderBackgroundGradient(e.Graphics, toolStrip, this.ColorTable.StatusStripGradientBegin, this.ColorTable.StatusStripGradientEnd, toolStrip.Orientation);
        }

        private void RenderStatusStripBorder(ToolStripRenderEventArgs e)
        {
            e.Graphics.DrawLine(SystemPens.ButtonHighlight, 0, 0, e.ToolStrip.Width, 0);
        }

        private void RenderToolStripBackgroundInternal(ToolStripRenderEventArgs e)
        {
            ToolStrip toolStrip = e.ToolStrip;
            Graphics graphics = e.Graphics;
            Rectangle bounds = new Rectangle(Point.Empty, e.ToolStrip.Size);
            LinearGradientMode mode = (toolStrip.Orientation == Orientation.Horizontal) ? LinearGradientMode.Vertical : LinearGradientMode.Horizontal;
            this.FillWithDoubleGradient(this.ColorTable.ToolStripGradientBegin, this.ColorTable.ToolStripGradientMiddle, this.ColorTable.ToolStripGradientEnd, e.Graphics, bounds, 12, 12, mode, false);
        }

        private void RenderToolStripCurve(ToolStripRenderEventArgs e)
        {
            Rectangle rectangle = new Rectangle(Point.Empty, e.ToolStrip.Size);
            Rectangle displayRectangle = e.ToolStrip.DisplayRectangle;
            Graphics graphics = e.Graphics;
            Point empty = Point.Empty;
            Point location = new Point(rectangle.Width - 1, 0);
            Point point3 = new Point(0, rectangle.Height - 1);
            using (Brush brush = new SolidBrush(this.ColorTable.ToolStripGradientMiddle))
            {
                Rectangle rectangle5;
                Rectangle rectangle3 = new Rectangle(empty, onePix);
                rectangle3.X++;
                Rectangle rectangle4 = new Rectangle(empty, onePix);
                rectangle4.Y++;
                rectangle5 = new Rectangle(location, onePix) {
                    X = rectangle5.X - 2
                };
                Rectangle rectangle6 = rectangle5;
                rectangle6.Y++;
                rectangle6.X++;
                Rectangle[] rects = new Rectangle[] { rectangle3, rectangle4, rectangle5, rectangle6 };
                for (int i = 0; i < rects.Length; i++)
                {
                    if (displayRectangle.IntersectsWith(rects[i]))
                    {
                        rects[i] = Rectangle.Empty;
                    }
                }
                graphics.FillRectangles(brush, rects);
            }
            using (Brush brush2 = new SolidBrush(this.ColorTable.ToolStripGradientEnd))
            {
                Point pt = point3;
                pt.Offset(1, -1);
                if (!displayRectangle.Contains(pt))
                {
                    graphics.FillRectangle(brush2, new Rectangle(pt, onePix));
                }
                Rectangle rect = new Rectangle(point3.X, point3.Y - 2, 1, 1);
                if (!displayRectangle.IntersectsWith(rect))
                {
                    graphics.FillRectangle(brush2, rect);
                }
            }
        }

        private void RenderToolStripDropDownBackground(ToolStripRenderEventArgs e)
        {
            ToolStrip toolStrip = e.ToolStrip;
            Rectangle rect = new Rectangle(Point.Empty, e.ToolStrip.Size);
            using (Brush brush = new SolidBrush(this.ColorTable.ToolStripDropDownBackground))
            {
                e.Graphics.FillRectangle(brush, rect);
            }
        }

        private void RenderToolStripDropDownBorder(ToolStripRenderEventArgs e)
        {
            ToolStripDropDown toolStrip = e.ToolStrip as ToolStripDropDown;
            Graphics graphics = e.Graphics;
            if (toolStrip != null)
            {
                Rectangle rectangle = new Rectangle(Point.Empty, toolStrip.Size);
                using (Pen pen = new Pen(this.ColorTable.MenuBorder))
                {
                    graphics.DrawRectangle(pen, rectangle.X, rectangle.Y, rectangle.Width - 1, rectangle.Height - 1);
                }
                if (!(toolStrip is ToolStripOverflow))
                {
                    using (Brush brush = new SolidBrush(this.ColorTable.ToolStripDropDownBackground))
                    {
                        graphics.FillRectangle(brush, e.ConnectedArea);
                    }
                }
            }
        }

        public ProfessionalColorTable ColorTable
        {
            get
            {
                if (this.professionalColorTable == null)
                {
                    return ProfessionalColors.ColorTable;
                }
                return this.professionalColorTable;
            }
        }

        internal ToolStripRenderer HighContrastRenderer
        {
            get
            {
                if (this.toolStripHighContrastRenderer == null)
                {
                    this.toolStripHighContrastRenderer = new ToolStripHighContrastRenderer(false);
                }
                return this.toolStripHighContrastRenderer;
            }
        }

        internal ToolStripRenderer LowResolutionRenderer
        {
            get
            {
                if (this.toolStripLowResolutionRenderer == null)
                {
                    this.toolStripLowResolutionRenderer = new ToolStripProfessionalLowResolutionRenderer();
                }
                return this.toolStripLowResolutionRenderer;
            }
        }

        internal override ToolStripRenderer RendererOverride
        {
            get
            {
                if (DisplayInformation.HighContrast)
                {
                    return this.HighContrastRenderer;
                }
                if (DisplayInformation.LowResolution)
                {
                    return this.LowResolutionRenderer;
                }
                return null;
            }
        }

        public bool RoundedEdges
        {
            get
            {
                return this.roundedEdges;
            }
            set
            {
                this.roundedEdges = value;
            }
        }

        private bool UseSystemColors
        {
            get
            {
                if (!this.ColorTable.UseSystemColors)
                {
                    return !ToolStripManager.VisualStylesEnabled;
                }
                return true;
            }
        }
    }
}

