namespace System.Windows.Forms
{
    using System;
    using System.Drawing;
    using System.Windows.Forms.VisualStyles;

    public class ToolStripSystemRenderer : ToolStripRenderer
    {
        [ThreadStatic]
        private static System.Windows.Forms.VisualStyles.VisualStyleRenderer renderer;
        private ToolStripRenderer toolStripHighContrastRenderer;

        public ToolStripSystemRenderer()
        {
        }

        internal ToolStripSystemRenderer(bool isDefault) : base(isDefault)
        {
        }

        private static void FillBackground(Graphics g, Rectangle bounds, Color backColor)
        {
            if (backColor.IsSystemColor)
            {
                g.FillRectangle(SystemBrushes.FromSystemColor(backColor), bounds);
            }
            else
            {
                using (Brush brush = new SolidBrush(backColor))
                {
                    g.FillRectangle(brush, bounds);
                }
            }
        }

        private static int GetItemState(ToolStripItem item)
        {
            return (int) GetToolBarState(item);
        }

        private static bool GetPen(Color color, ref Pen pen)
        {
            if (color.IsSystemColor)
            {
                pen = SystemPens.FromSystemColor(color);
                return false;
            }
            pen = new Pen(color);
            return true;
        }

        private static int GetSplitButtonDropDownItemState(ToolStripSplitButton item)
        {
            return (int) GetSplitButtonToolBarState(item, true);
        }

        private static int GetSplitButtonItemState(ToolStripSplitButton item)
        {
            return (int) GetSplitButtonToolBarState(item, false);
        }

        private static ToolBarState GetSplitButtonToolBarState(ToolStripSplitButton button, bool dropDownButton)
        {
            ToolBarState normal = ToolBarState.Normal;
            if (button != null)
            {
                if (!button.Enabled)
                {
                    return ToolBarState.Disabled;
                }
                if (dropDownButton)
                {
                    if (button.DropDownButtonPressed || button.ButtonPressed)
                    {
                        return ToolBarState.Pressed;
                    }
                    if (!button.DropDownButtonSelected && !button.ButtonSelected)
                    {
                        return normal;
                    }
                    return ToolBarState.Hot;
                }
                if (button.ButtonPressed)
                {
                    return ToolBarState.Pressed;
                }
                if (button.ButtonSelected)
                {
                    normal = ToolBarState.Hot;
                }
            }
            return normal;
        }

        private static ToolBarState GetToolBarState(ToolStripItem item)
        {
            ToolBarState normal = ToolBarState.Normal;
            if (item != null)
            {
                if (!item.Enabled)
                {
                    normal = ToolBarState.Disabled;
                }
                if ((item is ToolStripButton) && ((ToolStripButton) item).Checked)
                {
                    return ToolBarState.Checked;
                }
                if (item.Pressed)
                {
                    return ToolBarState.Pressed;
                }
                if (item.Selected)
                {
                    normal = ToolBarState.Hot;
                }
            }
            return normal;
        }

        protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
        {
            this.RenderItemInternal(e);
        }

        protected override void OnRenderDropDownButtonBackground(ToolStripItemRenderEventArgs e)
        {
            this.RenderItemInternal(e);
        }

        protected override void OnRenderGrip(ToolStripGripRenderEventArgs e)
        {
            Graphics dc = e.Graphics;
            Rectangle bounds = new Rectangle(Point.Empty, e.GripBounds.Size);
            bool flag = e.GripDisplayStyle == ToolStripGripDisplayStyle.Vertical;
            if (ToolStripManager.VisualStylesEnabled && System.Windows.Forms.VisualStyles.VisualStyleRenderer.IsElementDefined(VisualStyleElement.Rebar.Gripper.Normal))
            {
                System.Windows.Forms.VisualStyles.VisualStyleRenderer visualStyleRenderer = VisualStyleRenderer;
                if (flag)
                {
                    visualStyleRenderer.SetParameters(VisualStyleElement.Rebar.Gripper.Normal);
                    bounds.Height = ((bounds.Height - 2) / 4) * 4;
                    bounds.Y = Math.Max(0, ((e.GripBounds.Height - bounds.Height) - 2) / 2);
                }
                else
                {
                    visualStyleRenderer.SetParameters(VisualStyleElement.Rebar.GripperVertical.Normal);
                }
                visualStyleRenderer.DrawBackground(dc, bounds);
            }
            else
            {
                Color backColor = e.ToolStrip.BackColor;
                FillBackground(dc, bounds, backColor);
                if (flag)
                {
                    if (bounds.Height >= 4)
                    {
                        bounds.Inflate(0, -2);
                    }
                    bounds.Width = 3;
                }
                else
                {
                    if (bounds.Width >= 4)
                    {
                        bounds.Inflate(-2, 0);
                    }
                    bounds.Height = 3;
                }
                this.RenderSmall3DBorderInternal(dc, bounds, ToolBarState.Hot, e.ToolStrip.RightToLeft == RightToLeft.Yes);
            }
        }

        protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
        {
        }

        protected override void OnRenderItemBackground(ToolStripItemRenderEventArgs e)
        {
        }

        protected override void OnRenderLabelBackground(ToolStripItemRenderEventArgs e)
        {
            RenderLabelInternal(e);
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            ToolStripMenuItem item = e.Item as ToolStripMenuItem;
            Graphics g = e.Graphics;
            if (!(item is MdiControlStrip.SystemMenuItem) && (item != null))
            {
                Rectangle bounds = new Rectangle(Point.Empty, item.Size);
                if (item.IsTopLevel && !ToolStripManager.VisualStylesEnabled)
                {
                    if (item.BackgroundImage != null)
                    {
                        ControlPaint.DrawBackgroundImage(g, item.BackgroundImage, item.BackColor, item.BackgroundImageLayout, item.ContentRectangle, item.ContentRectangle);
                    }
                    else if (item.RawBackColor != Color.Empty)
                    {
                        FillBackground(g, item.ContentRectangle, item.BackColor);
                    }
                    ToolBarState toolBarState = GetToolBarState(item);
                    this.RenderSmall3DBorderInternal(g, bounds, toolBarState, item.RightToLeft == RightToLeft.Yes);
                }
                else
                {
                    Rectangle rect = new Rectangle(Point.Empty, item.Size);
                    if (item.IsOnDropDown)
                    {
                        rect.X += 2;
                        rect.Width -= 3;
                    }
                    if (item.Selected || item.Pressed)
                    {
                        g.FillRectangle(SystemBrushes.Highlight, rect);
                    }
                    else if (item.BackgroundImage != null)
                    {
                        ControlPaint.DrawBackgroundImage(g, item.BackgroundImage, item.BackColor, item.BackgroundImageLayout, item.ContentRectangle, rect);
                    }
                    else if (!ToolStripManager.VisualStylesEnabled && (item.RawBackColor != Color.Empty))
                    {
                        FillBackground(g, rect, item.BackColor);
                    }
                }
            }
        }

        protected override void OnRenderOverflowButtonBackground(ToolStripItemRenderEventArgs e)
        {
            ToolStripItem item = e.Item;
            Graphics dc = e.Graphics;
            if (ToolStripManager.VisualStylesEnabled && System.Windows.Forms.VisualStyles.VisualStyleRenderer.IsElementDefined(VisualStyleElement.Rebar.Chevron.Normal))
            {
                VisualStyleElement normal = VisualStyleElement.Rebar.Chevron.Normal;
                System.Windows.Forms.VisualStyles.VisualStyleRenderer visualStyleRenderer = VisualStyleRenderer;
                visualStyleRenderer.SetParameters(normal.ClassName, normal.Part, GetItemState(item));
                visualStyleRenderer.DrawBackground(dc, new Rectangle(Point.Empty, item.Size));
            }
            else
            {
                this.RenderItemInternal(e);
                Color arrowColor = item.Enabled ? SystemColors.ControlText : SystemColors.ControlDark;
                base.DrawArrow(new ToolStripArrowRenderEventArgs(dc, item, new Rectangle(Point.Empty, item.Size), arrowColor, ArrowDirection.Down));
            }
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            this.RenderSeparatorInternal(e.Graphics, e.Item, new Rectangle(Point.Empty, e.Item.Size), e.Vertical);
        }

        protected override void OnRenderSplitButtonBackground(ToolStripItemRenderEventArgs e)
        {
            ToolStripSplitButton item = e.Item as ToolStripSplitButton;
            Graphics dc = e.Graphics;
            bool rightToLeft = item.RightToLeft == RightToLeft.Yes;
            Color arrowColor = item.Enabled ? SystemColors.ControlText : SystemColors.ControlDark;
            VisualStyleElement element = rightToLeft ? VisualStyleElement.ToolBar.SplitButton.Normal : VisualStyleElement.ToolBar.SplitButtonDropDown.Normal;
            VisualStyleElement element2 = rightToLeft ? VisualStyleElement.ToolBar.DropDownButton.Normal : VisualStyleElement.ToolBar.SplitButton.Normal;
            Rectangle bounds = new Rectangle(Point.Empty, item.Size);
            if ((ToolStripManager.VisualStylesEnabled && System.Windows.Forms.VisualStyles.VisualStyleRenderer.IsElementDefined(element)) && System.Windows.Forms.VisualStyles.VisualStyleRenderer.IsElementDefined(element2))
            {
                System.Windows.Forms.VisualStyles.VisualStyleRenderer visualStyleRenderer = VisualStyleRenderer;
                visualStyleRenderer.SetParameters(element2.ClassName, element2.Part, GetSplitButtonItemState(item));
                Rectangle buttonBounds = item.ButtonBounds;
                if (rightToLeft)
                {
                    buttonBounds.Inflate(2, 0);
                }
                visualStyleRenderer.DrawBackground(dc, buttonBounds);
                visualStyleRenderer.SetParameters(element.ClassName, element.Part, GetSplitButtonDropDownItemState(item));
                visualStyleRenderer.DrawBackground(dc, item.DropDownButtonBounds);
                Rectangle contentRectangle = item.ContentRectangle;
                if (item.BackgroundImage != null)
                {
                    ControlPaint.DrawBackgroundImage(dc, item.BackgroundImage, item.BackColor, item.BackgroundImageLayout, contentRectangle, contentRectangle);
                }
                this.RenderSeparatorInternal(dc, item, item.SplitterBounds, true);
                if (rightToLeft || (item.BackgroundImage != null))
                {
                    base.DrawArrow(new ToolStripArrowRenderEventArgs(dc, item, item.DropDownButtonBounds, arrowColor, ArrowDirection.Down));
                }
            }
            else
            {
                Rectangle rectangle4 = item.ButtonBounds;
                if (item.BackgroundImage != null)
                {
                    Rectangle clipRect = item.Selected ? item.ContentRectangle : bounds;
                    if (item.BackgroundImage != null)
                    {
                        ControlPaint.DrawBackgroundImage(dc, item.BackgroundImage, item.BackColor, item.BackgroundImageLayout, bounds, clipRect);
                    }
                }
                else
                {
                    FillBackground(dc, rectangle4, item.BackColor);
                }
                ToolBarState splitButtonToolBarState = GetSplitButtonToolBarState(item, false);
                this.RenderSmall3DBorderInternal(dc, rectangle4, splitButtonToolBarState, rightToLeft);
                Rectangle dropDownButtonBounds = item.DropDownButtonBounds;
                if (item.BackgroundImage == null)
                {
                    FillBackground(dc, dropDownButtonBounds, item.BackColor);
                }
                splitButtonToolBarState = GetSplitButtonToolBarState(item, true);
                switch (splitButtonToolBarState)
                {
                    case ToolBarState.Pressed:
                    case ToolBarState.Hot:
                        this.RenderSmall3DBorderInternal(dc, dropDownButtonBounds, splitButtonToolBarState, rightToLeft);
                        break;
                }
                base.DrawArrow(new ToolStripArrowRenderEventArgs(dc, item, dropDownButtonBounds, arrowColor, ArrowDirection.Down));
            }
        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            ToolStrip toolStrip = e.ToolStrip;
            Graphics g = e.Graphics;
            Rectangle affectedBounds = e.AffectedBounds;
            if (base.ShouldPaintBackground(toolStrip))
            {
                if (toolStrip is StatusStrip)
                {
                    RenderStatusStripBackground(e);
                }
                else if (DisplayInformation.HighContrast)
                {
                    FillBackground(g, affectedBounds, SystemColors.ButtonFace);
                }
                else if (DisplayInformation.LowResolution)
                {
                    FillBackground(g, affectedBounds, (toolStrip is ToolStripDropDown) ? SystemColors.ControlLight : e.BackColor);
                }
                else if (toolStrip.IsDropDown)
                {
                    FillBackground(g, affectedBounds, !ToolStripManager.VisualStylesEnabled ? e.BackColor : SystemColors.Menu);
                }
                else if (toolStrip is MenuStrip)
                {
                    FillBackground(g, affectedBounds, !ToolStripManager.VisualStylesEnabled ? e.BackColor : SystemColors.MenuBar);
                }
                else if (ToolStripManager.VisualStylesEnabled && System.Windows.Forms.VisualStyles.VisualStyleRenderer.IsElementDefined(VisualStyleElement.Rebar.Band.Normal))
                {
                    System.Windows.Forms.VisualStyles.VisualStyleRenderer visualStyleRenderer = VisualStyleRenderer;
                    visualStyleRenderer.SetParameters(VisualStyleElement.ToolBar.Bar.Normal);
                    visualStyleRenderer.DrawBackground(g, affectedBounds);
                }
                else
                {
                    FillBackground(g, affectedBounds, !ToolStripManager.VisualStylesEnabled ? e.BackColor : SystemColors.MenuBar);
                }
            }
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            ToolStrip toolStrip = e.ToolStrip;
            Graphics graphics = e.Graphics;
            Rectangle clientRectangle = e.ToolStrip.ClientRectangle;
            if (toolStrip is StatusStrip)
            {
                this.RenderStatusStripBorder(e);
            }
            else if (toolStrip is ToolStripDropDown)
            {
                ToolStripDropDown down = toolStrip as ToolStripDropDown;
                if (down.DropShadowEnabled && ToolStripManager.VisualStylesEnabled)
                {
                    clientRectangle.Width--;
                    clientRectangle.Height--;
                    e.Graphics.DrawRectangle(new Pen(SystemColors.ControlDark), clientRectangle);
                }
                else
                {
                    ControlPaint.DrawBorder3D(e.Graphics, clientRectangle, Border3DStyle.Raised);
                }
            }
            else if (ToolStripManager.VisualStylesEnabled)
            {
                e.Graphics.DrawLine(SystemPens.ButtonHighlight, 0, clientRectangle.Bottom - 1, clientRectangle.Width, clientRectangle.Bottom - 1);
                e.Graphics.DrawLine(SystemPens.InactiveBorder, 0, clientRectangle.Bottom - 2, clientRectangle.Width, clientRectangle.Bottom - 2);
            }
            else
            {
                e.Graphics.DrawLine(SystemPens.ButtonHighlight, 0, clientRectangle.Bottom - 1, clientRectangle.Width, clientRectangle.Bottom - 1);
                e.Graphics.DrawLine(SystemPens.ButtonShadow, 0, clientRectangle.Bottom - 2, clientRectangle.Width, clientRectangle.Bottom - 2);
            }
        }

        protected override void OnRenderToolStripStatusLabelBackground(ToolStripItemRenderEventArgs e)
        {
            RenderLabelInternal(e);
            ToolStripStatusLabel item = e.Item as ToolStripStatusLabel;
            ControlPaint.DrawBorder3D(e.Graphics, new Rectangle(0, 0, item.Width - 1, item.Height - 1), item.BorderStyle, (Border3DSide) item.BorderSides);
        }

        private void RenderItemInternal(ToolStripItemRenderEventArgs e)
        {
            ToolStripItem item = e.Item;
            Graphics dc = e.Graphics;
            ToolBarState toolBarState = GetToolBarState(item);
            VisualStyleElement normal = VisualStyleElement.ToolBar.Button.Normal;
            if (ToolStripManager.VisualStylesEnabled && System.Windows.Forms.VisualStyles.VisualStyleRenderer.IsElementDefined(normal))
            {
                System.Windows.Forms.VisualStyles.VisualStyleRenderer visualStyleRenderer = VisualStyleRenderer;
                visualStyleRenderer.SetParameters(normal.ClassName, normal.Part, (int) toolBarState);
                visualStyleRenderer.DrawBackground(dc, new Rectangle(Point.Empty, item.Size));
            }
            else
            {
                this.RenderSmall3DBorderInternal(dc, new Rectangle(Point.Empty, item.Size), toolBarState, item.RightToLeft == RightToLeft.Yes);
            }
            Rectangle contentRectangle = item.ContentRectangle;
            if (item.BackgroundImage != null)
            {
                ControlPaint.DrawBackgroundImage(dc, item.BackgroundImage, item.BackColor, item.BackgroundImageLayout, contentRectangle, contentRectangle);
            }
            else
            {
                ToolStrip currentParent = item.GetCurrentParent();
                if (((currentParent != null) && (toolBarState != ToolBarState.Checked)) && (item.BackColor != currentParent.BackColor))
                {
                    FillBackground(dc, contentRectangle, item.BackColor);
                }
            }
        }

        private static void RenderLabelInternal(ToolStripItemRenderEventArgs e)
        {
            ToolStripItem item = e.Item;
            Graphics g = e.Graphics;
            Rectangle contentRectangle = item.ContentRectangle;
            if (item.BackgroundImage != null)
            {
                ControlPaint.DrawBackgroundImage(g, item.BackgroundImage, item.BackColor, item.BackgroundImageLayout, contentRectangle, contentRectangle);
            }
            else if ((VisualStyleRenderer == null) || (item.BackColor != SystemColors.Control))
            {
                FillBackground(g, contentRectangle, item.BackColor);
            }
        }

        private void RenderSeparatorInternal(Graphics g, ToolStripItem item, Rectangle bounds, bool vertical)
        {
            VisualStyleElement element = vertical ? VisualStyleElement.ToolBar.SeparatorHorizontal.Normal : VisualStyleElement.ToolBar.SeparatorVertical.Normal;
            if (ToolStripManager.VisualStylesEnabled && System.Windows.Forms.VisualStyles.VisualStyleRenderer.IsElementDefined(element))
            {
                System.Windows.Forms.VisualStyles.VisualStyleRenderer visualStyleRenderer = VisualStyleRenderer;
                visualStyleRenderer.SetParameters(element.ClassName, element.Part, GetItemState(item));
                visualStyleRenderer.DrawBackground(g, bounds);
            }
            else
            {
                Color foreColor = item.ForeColor;
                Color backColor = item.BackColor;
                Pen controlDark = SystemPens.ControlDark;
                bool pen = GetPen(foreColor, ref controlDark);
                try
                {
                    if (vertical)
                    {
                        if (bounds.Height >= 4)
                        {
                            bounds.Inflate(0, -2);
                        }
                        bool flag2 = item.RightToLeft == RightToLeft.Yes;
                        Pen pen2 = flag2 ? SystemPens.ButtonHighlight : controlDark;
                        Pen pen3 = flag2 ? controlDark : SystemPens.ButtonHighlight;
                        int num = bounds.Width / 2;
                        g.DrawLine(pen2, num, bounds.Top, num, bounds.Bottom);
                        num++;
                        g.DrawLine(pen3, num, bounds.Top, num, bounds.Bottom);
                    }
                    else
                    {
                        if (bounds.Width >= 4)
                        {
                            bounds.Inflate(-2, 0);
                        }
                        int num2 = bounds.Height / 2;
                        g.DrawLine(controlDark, bounds.Left, num2, bounds.Right, num2);
                        num2++;
                        g.DrawLine(SystemPens.ButtonHighlight, bounds.Left, num2, bounds.Right, num2);
                    }
                }
                finally
                {
                    if (pen && (controlDark != null))
                    {
                        controlDark.Dispose();
                    }
                }
            }
        }

        private void RenderSmall3DBorderInternal(Graphics g, Rectangle bounds, ToolBarState state, bool rightToLeft)
        {
            if (((state == ToolBarState.Hot) || (state == ToolBarState.Pressed)) || (state == ToolBarState.Checked))
            {
                Pen pen2 = (state == ToolBarState.Hot) ? SystemPens.ButtonHighlight : SystemPens.ButtonShadow;
                Pen pen4 = (state == ToolBarState.Hot) ? SystemPens.ButtonShadow : SystemPens.ButtonHighlight;
                Pen pen = rightToLeft ? pen4 : pen2;
                Pen pen3 = rightToLeft ? pen2 : pen4;
                g.DrawLine(pen2, bounds.Left, bounds.Top, bounds.Right - 1, bounds.Top);
                g.DrawLine(pen, bounds.Left, bounds.Top, bounds.Left, bounds.Bottom - 1);
                g.DrawLine(pen3, bounds.Right - 1, bounds.Top, bounds.Right - 1, bounds.Bottom - 1);
                g.DrawLine(pen4, bounds.Left, bounds.Bottom - 1, bounds.Right - 1, bounds.Bottom - 1);
            }
        }

        private static void RenderStatusStripBackground(ToolStripRenderEventArgs e)
        {
            if (Application.RenderWithVisualStyles)
            {
                System.Windows.Forms.VisualStyles.VisualStyleRenderer visualStyleRenderer = VisualStyleRenderer;
                visualStyleRenderer.SetParameters(VisualStyleElement.Status.Bar.Normal);
                visualStyleRenderer.DrawBackground(e.Graphics, new Rectangle(0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1));
            }
            else if (!SystemInformation.InLockedTerminalSession())
            {
                e.Graphics.Clear(e.BackColor);
            }
        }

        private void RenderStatusStripBorder(ToolStripRenderEventArgs e)
        {
            if (!Application.RenderWithVisualStyles)
            {
                e.Graphics.DrawLine(SystemPens.ButtonHighlight, 0, 0, e.ToolStrip.Width, 0);
            }
        }

        internal ToolStripRenderer HighContrastRenderer
        {
            get
            {
                if (this.toolStripHighContrastRenderer == null)
                {
                    this.toolStripHighContrastRenderer = new ToolStripHighContrastRenderer(true);
                }
                return this.toolStripHighContrastRenderer;
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
                return null;
            }
        }

        private static System.Windows.Forms.VisualStyles.VisualStyleRenderer VisualStyleRenderer
        {
            get
            {
                if (Application.RenderWithVisualStyles)
                {
                    if ((renderer == null) && System.Windows.Forms.VisualStyles.VisualStyleRenderer.IsElementDefined(VisualStyleElement.ToolBar.Button.Normal))
                    {
                        renderer = new System.Windows.Forms.VisualStyles.VisualStyleRenderer(VisualStyleElement.ToolBar.Button.Normal);
                    }
                }
                else
                {
                    renderer = null;
                }
                return renderer;
            }
        }
    }
}

