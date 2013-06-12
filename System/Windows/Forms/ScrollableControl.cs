namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Windows.Forms.Layout;

    [ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch), Designer("System.Windows.Forms.Design.ScrollableControlDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public class ScrollableControl : Control, IArrangedElement, IComponent, IDisposable
    {
        internal static readonly TraceSwitch AutoScrolling;
        private Rectangle displayRect = Rectangle.Empty;
        private DockPaddingEdges dockPadding;
        private static readonly object EVENT_SCROLL = new object();
        private HScrollProperties horizontalScroll;
        private Size requestedScrollMargin = Size.Empty;
        private bool resetRTLHScrollValue;
        private Size scrollMargin = Size.Empty;
        internal Point scrollPosition = Point.Empty;
        private int scrollState;
        protected const int ScrollStateAutoScrolling = 1;
        protected const int ScrollStateFullDrag = 0x10;
        protected const int ScrollStateHScrollVisible = 2;
        protected const int ScrollStateUserHasScrolled = 8;
        protected const int ScrollStateVScrollVisible = 4;
        private Size userAutoScrollMinSize = Size.Empty;
        private VScrollProperties verticalScroll;

        [System.Windows.Forms.SRDescription("ScrollBarOnScrollDescr"), System.Windows.Forms.SRCategory("CatAction")]
        public event ScrollEventHandler Scroll
        {
            add
            {
                base.Events.AddHandler(EVENT_SCROLL, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_SCROLL, value);
            }
        }

        public ScrollableControl()
        {
            base.SetStyle(ControlStyles.ContainerControl, true);
            base.SetStyle(ControlStyles.AllPaintingInWmPaint, false);
            this.SetScrollState(1, false);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void AdjustFormScrollbars(bool displayScrollbars)
        {
            bool flag = false;
            Rectangle displayRectInternal = this.GetDisplayRectInternal();
            if (!displayScrollbars && (this.HScroll || this.VScroll))
            {
                flag = this.SetVisibleScrollbars(false, false);
            }
            if (!displayScrollbars)
            {
                Rectangle clientRectangle = base.ClientRectangle;
                displayRectInternal.Width = clientRectangle.Width;
                displayRectInternal.Height = clientRectangle.Height;
            }
            else
            {
                flag |= this.ApplyScrollbarChanges(displayRectInternal);
            }
            if (flag)
            {
                LayoutTransaction.DoLayout(this, this, PropertyNames.DisplayRectangle);
            }
        }

        private bool ApplyScrollbarChanges(Rectangle display)
        {
            bool flag = false;
            bool horiz = false;
            bool vert = false;
            Rectangle clientRectangle = base.ClientRectangle;
            Rectangle rectangle3 = clientRectangle;
            if (this.HScroll)
            {
                clientRectangle.Height += SystemInformation.HorizontalScrollBarHeight;
            }
            else
            {
                rectangle3.Height -= SystemInformation.HorizontalScrollBarHeight;
            }
            if (this.VScroll)
            {
                clientRectangle.Width += SystemInformation.VerticalScrollBarWidth;
            }
            else
            {
                rectangle3.Width -= SystemInformation.VerticalScrollBarWidth;
            }
            int width = rectangle3.Width;
            int height = rectangle3.Height;
            if (base.Controls.Count != 0)
            {
                this.scrollMargin = this.requestedScrollMargin;
                if (this.dockPadding != null)
                {
                    this.scrollMargin.Height += base.Padding.Bottom;
                    this.scrollMargin.Width += base.Padding.Right;
                }
                for (int i = 0; i < base.Controls.Count; i++)
                {
                    Control control = base.Controls[i];
                    if ((control != null) && control.GetState(2))
                    {
                        switch (control.Dock)
                        {
                            case DockStyle.Bottom:
                                this.scrollMargin.Height += control.Size.Height;
                                break;

                            case DockStyle.Right:
                                this.scrollMargin.Width += control.Size.Width;
                                break;
                        }
                    }
                }
            }
            if (!this.userAutoScrollMinSize.IsEmpty)
            {
                width = this.userAutoScrollMinSize.Width + this.scrollMargin.Width;
                height = this.userAutoScrollMinSize.Height + this.scrollMargin.Height;
                horiz = true;
                vert = true;
            }
            bool flag4 = this.LayoutEngine == DefaultLayout.Instance;
            if (!flag4 && CommonProperties.HasLayoutBounds(this))
            {
                Size layoutBounds = CommonProperties.GetLayoutBounds(this);
                if (layoutBounds.Width > width)
                {
                    horiz = true;
                    width = layoutBounds.Width;
                }
                if (layoutBounds.Height > height)
                {
                    vert = true;
                    height = layoutBounds.Height;
                }
            }
            else if (base.Controls.Count != 0)
            {
                for (int j = 0; j < base.Controls.Count; j++)
                {
                    bool flag5 = true;
                    bool flag6 = true;
                    Control control2 = base.Controls[j];
                    if ((control2 == null) || !control2.GetState(2))
                    {
                        continue;
                    }
                    if (flag4)
                    {
                        Control control3 = control2;
                        switch (control3.Dock)
                        {
                            case DockStyle.Top:
                                flag5 = false;
                                goto Label_02DD;

                            case DockStyle.Bottom:
                            case DockStyle.Right:
                            case DockStyle.Fill:
                                flag5 = false;
                                flag6 = false;
                                goto Label_02DD;

                            case DockStyle.Left:
                                flag6 = false;
                                goto Label_02DD;
                        }
                        AnchorStyles anchor = control3.Anchor;
                        if ((anchor & AnchorStyles.Right) == AnchorStyles.Right)
                        {
                            flag5 = false;
                        }
                        if ((anchor & AnchorStyles.Left) != AnchorStyles.Left)
                        {
                            flag5 = false;
                        }
                        if ((anchor & AnchorStyles.Bottom) == AnchorStyles.Bottom)
                        {
                            flag6 = false;
                        }
                        if ((anchor & AnchorStyles.Top) != AnchorStyles.Top)
                        {
                            flag6 = false;
                        }
                    }
                Label_02DD:
                    if (flag5 || flag6)
                    {
                        Rectangle bounds = control2.Bounds;
                        int num5 = ((-display.X + bounds.X) + bounds.Width) + this.scrollMargin.Width;
                        int num6 = ((-display.Y + bounds.Y) + bounds.Height) + this.scrollMargin.Height;
                        if (!flag4)
                        {
                            num5 += control2.Margin.Right;
                            num6 += control2.Margin.Bottom;
                        }
                        if ((num5 > width) && flag5)
                        {
                            horiz = true;
                            width = num5;
                        }
                        if ((num6 > height) && flag6)
                        {
                            vert = true;
                            height = num6;
                        }
                    }
                }
            }
            if (width <= clientRectangle.Width)
            {
                horiz = false;
            }
            if (height <= clientRectangle.Height)
            {
                vert = false;
            }
            Rectangle rectangle5 = clientRectangle;
            if (horiz)
            {
                rectangle5.Height -= SystemInformation.HorizontalScrollBarHeight;
            }
            if (vert)
            {
                rectangle5.Width -= SystemInformation.VerticalScrollBarWidth;
            }
            if (horiz && (height > rectangle5.Height))
            {
                vert = true;
            }
            if (vert && (width > rectangle5.Width))
            {
                horiz = true;
            }
            if (!horiz)
            {
                width = rectangle5.Width;
            }
            if (!vert)
            {
                height = rectangle5.Height;
            }
            flag = this.SetVisibleScrollbars(horiz, vert) || flag;
            if (this.HScroll || this.VScroll)
            {
                flag = this.SetDisplayRectangleSize(width, height) || flag;
            }
            else
            {
                this.SetDisplayRectangleSize(width, height);
            }
            this.SyncScrollbars(true);
            return flag;
        }

        private Rectangle GetDisplayRectInternal()
        {
            if (this.displayRect.IsEmpty)
            {
                this.displayRect = base.ClientRectangle;
            }
            if (!this.AutoScroll && this.HorizontalScroll.visible)
            {
                this.displayRect = new Rectangle(this.displayRect.X, this.displayRect.Y, this.HorizontalScroll.Maximum, this.displayRect.Height);
            }
            if (!this.AutoScroll && this.VerticalScroll.visible)
            {
                this.displayRect = new Rectangle(this.displayRect.X, this.displayRect.Y, this.displayRect.Width, this.VerticalScroll.Maximum);
            }
            return this.displayRect;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected bool GetScrollState(int bit)
        {
            return ((bit & this.scrollState) == bit);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void OnLayout(LayoutEventArgs levent)
        {
            if ((levent.AffectedControl != null) && this.AutoScroll)
            {
                base.OnLayout(levent);
            }
            this.AdjustFormScrollbars(this.AutoScroll);
            base.OnLayout(levent);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (this.VScroll)
            {
                Rectangle clientRectangle = base.ClientRectangle;
                int num = -this.displayRect.Y;
                int num2 = -(clientRectangle.Height - this.displayRect.Height);
                num = Math.Min(Math.Max(num - e.Delta, 0), num2);
                this.SetDisplayRectLocation(this.displayRect.X, -num);
                this.SyncScrollbars(this.AutoScroll);
                if (e is HandledMouseEventArgs)
                {
                    ((HandledMouseEventArgs) e).Handled = true;
                }
            }
            else if (this.HScroll)
            {
                Rectangle rectangle2 = base.ClientRectangle;
                int num3 = -this.displayRect.X;
                int num4 = -(rectangle2.Width - this.displayRect.Width);
                num3 = Math.Min(Math.Max(num3 - e.Delta, 0), num4);
                this.SetDisplayRectLocation(-num3, this.displayRect.Y);
                this.SyncScrollbars(this.AutoScroll);
                if (e is HandledMouseEventArgs)
                {
                    ((HandledMouseEventArgs) e).Handled = true;
                }
            }
            base.OnMouseWheel(e);
        }

        protected override void OnPaddingChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[Control.EventPaddingChanged];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if ((this.HScroll || this.VScroll) && ((this.BackgroundImage != null) && (((this.BackgroundImageLayout == ImageLayout.Zoom) || (this.BackgroundImageLayout == ImageLayout.Stretch)) || (this.BackgroundImageLayout == ImageLayout.Center))))
            {
                if (ControlPaint.IsImageTransparent(this.BackgroundImage))
                {
                    base.PaintTransparentBackground(e, this.displayRect);
                }
                ControlPaint.DrawBackgroundImage(e.Graphics, this.BackgroundImage, this.BackColor, this.BackgroundImageLayout, this.displayRect, this.displayRect, this.displayRect.Location);
            }
            else
            {
                base.OnPaintBackground(e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void OnRightToLeftChanged(EventArgs e)
        {
            base.OnRightToLeftChanged(e);
            this.resetRTLHScrollValue = true;
            LayoutTransaction.DoLayout(this, this, PropertyNames.RightToLeft);
        }

        protected virtual void OnScroll(ScrollEventArgs se)
        {
            ScrollEventHandler handler = (ScrollEventHandler) base.Events[EVENT_SCROLL];
            if (handler != null)
            {
                handler(this, se);
            }
        }

        private void OnSetScrollPosition(object sender, EventArgs e)
        {
            if (!base.IsMirrored)
            {
                base.SendMessage(0x114, System.Windows.Forms.NativeMethods.Util.MAKELPARAM((this.RightToLeft == RightToLeft.Yes) ? 7 : 6, 0), 0);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void OnVisibleChanged(EventArgs e)
        {
            if (base.Visible)
            {
                LayoutTransaction.DoLayout(this, this, PropertyNames.Visible);
            }
            base.OnVisibleChanged(e);
        }

        private void ResetAutoScrollMargin()
        {
            this.AutoScrollMargin = Size.Empty;
        }

        private void ResetAutoScrollMinSize()
        {
            this.AutoScrollMinSize = Size.Empty;
        }

        private void ResetScrollProperties(ScrollProperties scrollProperties)
        {
            scrollProperties.visible = false;
            scrollProperties.value = 0;
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            this.ScaleDockPadding(factor.Width, factor.Height);
            base.ScaleControl(factor, specified);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected override void ScaleCore(float dx, float dy)
        {
            this.ScaleDockPadding(dx, dy);
            base.ScaleCore(dx, dy);
        }

        internal void ScaleDockPadding(float dx, float dy)
        {
            if (this.dockPadding != null)
            {
                this.dockPadding.Scale(dx, dy);
            }
        }

        public void ScrollControlIntoView(Control activeControl)
        {
            Rectangle clientRectangle = base.ClientRectangle;
            if (((base.IsDescendant(activeControl) && this.AutoScroll) && (this.HScroll || this.VScroll)) && (((activeControl != null) && (clientRectangle.Width > 0)) && (clientRectangle.Height > 0)))
            {
                Point point = this.ScrollToControl(activeControl);
                this.SetScrollState(8, false);
                this.SetDisplayRectLocation(point.X, point.Y);
                this.SyncScrollbars(true);
            }
        }

        private int ScrollThumbPosition(int fnBar)
        {
            System.Windows.Forms.NativeMethods.SCROLLINFO si = new System.Windows.Forms.NativeMethods.SCROLLINFO {
                fMask = 0x10
            };
            System.Windows.Forms.SafeNativeMethods.GetScrollInfo(new HandleRef(this, base.Handle), fnBar, si);
            return si.nTrackPos;
        }

        protected virtual Point ScrollToControl(Control activeControl)
        {
            Rectangle clientRectangle = base.ClientRectangle;
            int x = this.displayRect.X;
            int y = this.displayRect.Y;
            int width = this.scrollMargin.Width;
            int height = this.scrollMargin.Height;
            Rectangle bounds = activeControl.Bounds;
            if (activeControl.ParentInternal != this)
            {
                bounds = base.RectangleToClient(activeControl.ParentInternal.RectangleToScreen(bounds));
            }
            if (bounds.X < width)
            {
                x = (this.displayRect.X + width) - bounds.X;
            }
            else if (((bounds.X + bounds.Width) + width) > clientRectangle.Width)
            {
                x = clientRectangle.Width - (((bounds.X + bounds.Width) + width) - this.displayRect.X);
                if (((bounds.X + x) - this.displayRect.X) < width)
                {
                    x = (this.displayRect.X + width) - bounds.X;
                }
            }
            if (bounds.Y < height)
            {
                y = (this.displayRect.Y + height) - bounds.Y;
            }
            else if (((bounds.Y + bounds.Height) + height) > clientRectangle.Height)
            {
                y = clientRectangle.Height - (((bounds.Y + bounds.Height) + height) - this.displayRect.Y);
                if (((bounds.Y + y) - this.displayRect.Y) < height)
                {
                    y = (this.displayRect.Y + height) - bounds.Y;
                }
            }
            x += activeControl.AutoScrollOffset.X;
            return new Point(x, y + activeControl.AutoScrollOffset.Y);
        }

        public void SetAutoScrollMargin(int x, int y)
        {
            if (x < 0)
            {
                x = 0;
            }
            if (y < 0)
            {
                y = 0;
            }
            if ((x != this.requestedScrollMargin.Width) || (y != this.requestedScrollMargin.Height))
            {
                this.requestedScrollMargin = new Size(x, y);
                if (this.AutoScroll)
                {
                    base.PerformLayout();
                }
            }
        }

        internal void SetDisplayFromScrollProps(int x, int y)
        {
            Rectangle displayRectInternal = this.GetDisplayRectInternal();
            this.ApplyScrollbarChanges(displayRectInternal);
            this.SetDisplayRectLocation(x, y);
        }

        private bool SetDisplayRectangleSize(int width, int height)
        {
            bool flag = false;
            if ((this.displayRect.Width != width) || (this.displayRect.Height != height))
            {
                this.displayRect.Width = width;
                this.displayRect.Height = height;
                flag = true;
            }
            int num = base.ClientRectangle.Width - width;
            int num2 = base.ClientRectangle.Height - height;
            if (num > 0)
            {
                num = 0;
            }
            if (num2 > 0)
            {
                num2 = 0;
            }
            int x = this.displayRect.X;
            int y = this.displayRect.Y;
            if (!this.HScroll)
            {
                x = 0;
            }
            if (!this.VScroll)
            {
                y = 0;
            }
            if (x < num)
            {
                x = num;
            }
            if (y < num2)
            {
                y = num2;
            }
            this.SetDisplayRectLocation(x, y);
            return flag;
        }

        protected void SetDisplayRectLocation(int x, int y)
        {
            int nXAmount = 0;
            int nYAmount = 0;
            Rectangle clientRectangle = base.ClientRectangle;
            Rectangle displayRect = this.displayRect;
            int num3 = Math.Min(clientRectangle.Width - displayRect.Width, 0);
            int num4 = Math.Min(clientRectangle.Height - displayRect.Height, 0);
            if (x > 0)
            {
                x = 0;
            }
            if (y > 0)
            {
                y = 0;
            }
            if (x < num3)
            {
                x = num3;
            }
            if (y < num4)
            {
                y = num4;
            }
            if (displayRect.X != x)
            {
                nXAmount = x - displayRect.X;
            }
            if (displayRect.Y != y)
            {
                nYAmount = y - displayRect.Y;
            }
            this.displayRect.X = x;
            this.displayRect.Y = y;
            if ((nXAmount != 0) || ((nYAmount != 0) && base.IsHandleCreated))
            {
                Rectangle rectangle3 = base.ClientRectangle;
                System.Windows.Forms.NativeMethods.RECT rectClip = System.Windows.Forms.NativeMethods.RECT.FromXYWH(rectangle3.X, rectangle3.Y, rectangle3.Width, rectangle3.Height);
                System.Windows.Forms.NativeMethods.RECT prcUpdate = System.Windows.Forms.NativeMethods.RECT.FromXYWH(rectangle3.X, rectangle3.Y, rectangle3.Width, rectangle3.Height);
                System.Windows.Forms.SafeNativeMethods.ScrollWindowEx(new HandleRef(this, base.Handle), nXAmount, nYAmount, null, ref rectClip, System.Windows.Forms.NativeMethods.NullHandleRef, ref prcUpdate, 7);
            }
            for (int i = 0; i < base.Controls.Count; i++)
            {
                Control control = base.Controls[i];
                if ((control != null) && control.IsHandleCreated)
                {
                    control.UpdateBounds();
                }
            }
        }

        protected void SetScrollState(int bit, bool value)
        {
            if (value)
            {
                this.scrollState |= bit;
            }
            else
            {
                this.scrollState &= ~bit;
            }
        }

        private bool SetVisibleScrollbars(bool horiz, bool vert)
        {
            bool flag = false;
            if (((!horiz && this.HScroll) || (horiz && !this.HScroll)) || ((!vert && this.VScroll) || (vert && !this.VScroll)))
            {
                flag = true;
            }
            if ((horiz && !this.HScroll) && (this.RightToLeft == RightToLeft.Yes))
            {
                this.resetRTLHScrollValue = true;
            }
            if (flag)
            {
                int x = this.displayRect.X;
                int y = this.displayRect.Y;
                if (!horiz)
                {
                    x = 0;
                }
                if (!vert)
                {
                    y = 0;
                }
                this.SetDisplayRectLocation(x, y);
                this.SetScrollState(8, false);
                this.HScroll = horiz;
                this.VScroll = vert;
                if (horiz)
                {
                    this.HorizontalScroll.visible = true;
                }
                else
                {
                    this.ResetScrollProperties(this.HorizontalScroll);
                }
                if (vert)
                {
                    this.VerticalScroll.visible = true;
                }
                else
                {
                    this.ResetScrollProperties(this.VerticalScroll);
                }
                base.UpdateStyles();
            }
            return flag;
        }

        private bool ShouldSerializeAutoScrollMargin()
        {
            return !this.AutoScrollMargin.Equals(new Size(0, 0));
        }

        private bool ShouldSerializeAutoScrollMinSize()
        {
            return !this.AutoScrollMinSize.Equals(new Size(0, 0));
        }

        private bool ShouldSerializeAutoScrollPosition()
        {
            if (this.AutoScroll)
            {
                Point autoScrollPosition = this.AutoScrollPosition;
                if ((autoScrollPosition.X != 0) || (autoScrollPosition.Y != 0))
                {
                    return true;
                }
            }
            return false;
        }

        private void SyncScrollbars(bool autoScroll)
        {
            Rectangle displayRect = this.displayRect;
            if (autoScroll)
            {
                if (base.IsHandleCreated)
                {
                    if (this.HScroll)
                    {
                        if (!this.HorizontalScroll.maximumSetExternally)
                        {
                            this.HorizontalScroll.maximum = displayRect.Width - 1;
                        }
                        if (!this.HorizontalScroll.largeChangeSetExternally)
                        {
                            this.HorizontalScroll.largeChange = base.ClientRectangle.Width;
                        }
                        if (!this.HorizontalScroll.smallChangeSetExternally)
                        {
                            this.HorizontalScroll.smallChange = 5;
                        }
                        if (this.resetRTLHScrollValue && !base.IsMirrored)
                        {
                            this.resetRTLHScrollValue = false;
                            base.BeginInvoke(new EventHandler(this.OnSetScrollPosition));
                        }
                        else if ((-displayRect.X >= this.HorizontalScroll.minimum) && (-displayRect.X < this.HorizontalScroll.maximum))
                        {
                            this.HorizontalScroll.value = -displayRect.X;
                        }
                        this.HorizontalScroll.UpdateScrollInfo();
                    }
                    if (this.VScroll)
                    {
                        if (!this.VerticalScroll.maximumSetExternally)
                        {
                            this.VerticalScroll.maximum = displayRect.Height - 1;
                        }
                        if (!this.VerticalScroll.largeChangeSetExternally)
                        {
                            this.VerticalScroll.largeChange = base.ClientRectangle.Height;
                        }
                        if (!this.VerticalScroll.smallChangeSetExternally)
                        {
                            this.VerticalScroll.smallChange = 5;
                        }
                        if ((-displayRect.Y >= this.VerticalScroll.minimum) && (-displayRect.Y < this.VerticalScroll.maximum))
                        {
                            this.VerticalScroll.value = -displayRect.Y;
                        }
                        this.VerticalScroll.UpdateScrollInfo();
                    }
                }
            }
            else
            {
                if (this.HorizontalScroll.Visible)
                {
                    this.HorizontalScroll.Value = -displayRect.X;
                }
                else
                {
                    this.ResetScrollProperties(this.HorizontalScroll);
                }
                if (this.VerticalScroll.Visible)
                {
                    this.VerticalScroll.Value = -displayRect.Y;
                }
                else
                {
                    this.ResetScrollProperties(this.VerticalScroll);
                }
            }
        }

        private void UpdateFullDrag()
        {
            this.SetScrollState(0x10, SystemInformation.DragFullWindows);
        }

        private void WmHScroll(ref Message m)
        {
            if (m.LParam != IntPtr.Zero)
            {
                base.WndProc(ref m);
            }
            else
            {
                Rectangle clientRectangle = base.ClientRectangle;
                int num = -this.displayRect.X;
                int oldValue = num;
                int maximum = -(clientRectangle.Width - this.displayRect.Width);
                if (!this.AutoScroll)
                {
                    maximum = this.HorizontalScroll.Maximum;
                }
                switch (System.Windows.Forms.NativeMethods.Util.LOWORD(m.WParam))
                {
                    case 0:
                        if (num <= this.HorizontalScroll.SmallChange)
                        {
                            num = 0;
                            break;
                        }
                        num -= this.HorizontalScroll.SmallChange;
                        break;

                    case 1:
                        if (num >= (maximum - this.HorizontalScroll.SmallChange))
                        {
                            num = maximum;
                            break;
                        }
                        num += this.HorizontalScroll.SmallChange;
                        break;

                    case 2:
                        if (num <= this.HorizontalScroll.LargeChange)
                        {
                            num = 0;
                            break;
                        }
                        num -= this.HorizontalScroll.LargeChange;
                        break;

                    case 3:
                        if (num >= (maximum - this.HorizontalScroll.LargeChange))
                        {
                            num = maximum;
                            break;
                        }
                        num += this.HorizontalScroll.LargeChange;
                        break;

                    case 4:
                    case 5:
                        num = this.ScrollThumbPosition(0);
                        break;

                    case 6:
                        num = 0;
                        break;

                    case 7:
                        num = maximum;
                        break;
                }
                if (this.GetScrollState(0x10) || (System.Windows.Forms.NativeMethods.Util.LOWORD(m.WParam) != 5))
                {
                    this.SetScrollState(8, true);
                    this.SetDisplayRectLocation(-num, this.displayRect.Y);
                    this.SyncScrollbars(this.AutoScroll);
                }
                this.WmOnScroll(ref m, oldValue, num, ScrollOrientation.HorizontalScroll);
            }
        }

        private void WmOnScroll(ref Message m, int oldValue, int value, ScrollOrientation scrollOrientation)
        {
            ScrollEventType type = (ScrollEventType) System.Windows.Forms.NativeMethods.Util.LOWORD(m.WParam);
            if (type != ScrollEventType.EndScroll)
            {
                ScrollEventArgs se = new ScrollEventArgs(type, oldValue, value, scrollOrientation);
                this.OnScroll(se);
            }
        }

        private void WmSettingChange(ref Message m)
        {
            base.WndProc(ref m);
            this.UpdateFullDrag();
        }

        private void WmVScroll(ref Message m)
        {
            if (m.LParam != IntPtr.Zero)
            {
                base.WndProc(ref m);
            }
            else
            {
                Rectangle clientRectangle = base.ClientRectangle;
                bool flag = System.Windows.Forms.NativeMethods.Util.LOWORD(m.WParam) != 5;
                int num = -this.displayRect.Y;
                int oldValue = num;
                int maximum = -(clientRectangle.Height - this.displayRect.Height);
                if (!this.AutoScroll)
                {
                    maximum = this.VerticalScroll.Maximum;
                }
                switch (System.Windows.Forms.NativeMethods.Util.LOWORD(m.WParam))
                {
                    case 0:
                        if (num <= 0)
                        {
                            num = 0;
                            break;
                        }
                        num -= this.VerticalScroll.SmallChange;
                        break;

                    case 1:
                        if (num >= (maximum - this.VerticalScroll.SmallChange))
                        {
                            num = maximum;
                            break;
                        }
                        num += this.VerticalScroll.SmallChange;
                        break;

                    case 2:
                        if (num <= this.VerticalScroll.LargeChange)
                        {
                            num = 0;
                            break;
                        }
                        num -= this.VerticalScroll.LargeChange;
                        break;

                    case 3:
                        if (num >= (maximum - this.VerticalScroll.LargeChange))
                        {
                            num = maximum;
                            break;
                        }
                        num += this.VerticalScroll.LargeChange;
                        break;

                    case 4:
                    case 5:
                        num = this.ScrollThumbPosition(1);
                        break;

                    case 6:
                        num = 0;
                        break;

                    case 7:
                        num = maximum;
                        break;
                }
                if (this.GetScrollState(0x10) || flag)
                {
                    this.SetScrollState(8, true);
                    this.SetDisplayRectLocation(this.displayRect.X, -num);
                    this.SyncScrollbars(this.AutoScroll);
                }
                this.WmOnScroll(ref m, oldValue, num, ScrollOrientation.VerticalScroll);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x114:
                    this.WmHScroll(ref m);
                    return;

                case 0x115:
                    this.WmVScroll(ref m);
                    return;

                case 0x1a:
                    this.WmSettingChange(ref m);
                    return;
            }
            base.WndProc(ref m);
        }

        [System.Windows.Forms.SRCategory("CatLayout"), Localizable(true), DefaultValue(false), System.Windows.Forms.SRDescription("FormAutoScrollDescr")]
        public virtual bool AutoScroll
        {
            get
            {
                return this.GetScrollState(1);
            }
            set
            {
                if (value)
                {
                    this.UpdateFullDrag();
                }
                this.SetScrollState(1, value);
                LayoutTransaction.DoLayout(this, this, PropertyNames.AutoScroll);
            }
        }

        [System.Windows.Forms.SRDescription("FormAutoScrollMarginDescr"), Localizable(true), System.Windows.Forms.SRCategory("CatLayout")]
        public Size AutoScrollMargin
        {
            get
            {
                return this.requestedScrollMargin;
            }
            set
            {
                if ((value.Width < 0) || (value.Height < 0))
                {
                    throw new ArgumentOutOfRangeException("AutoScrollMargin", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "AutoScrollMargin", value.ToString() }));
                }
                this.SetAutoScrollMargin(value.Width, value.Height);
            }
        }

        [System.Windows.Forms.SRDescription("FormAutoScrollMinSizeDescr"), System.Windows.Forms.SRCategory("CatLayout"), Localizable(true)]
        public Size AutoScrollMinSize
        {
            get
            {
                return this.userAutoScrollMinSize;
            }
            set
            {
                if (value != this.userAutoScrollMinSize)
                {
                    this.userAutoScrollMinSize = value;
                    this.AutoScroll = true;
                    base.PerformLayout();
                }
            }
        }

        [System.Windows.Forms.SRDescription("FormAutoScrollPositionDescr"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), System.Windows.Forms.SRCategory("CatLayout")]
        public Point AutoScrollPosition
        {
            get
            {
                Rectangle displayRectInternal = this.GetDisplayRectInternal();
                return new Point(displayRectInternal.X, displayRectInternal.Y);
            }
            set
            {
                if (base.Created)
                {
                    this.SetDisplayRectLocation(-value.X, -value.Y);
                    this.SyncScrollbars(true);
                }
                this.scrollPosition = value;
            }
        }

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                if (this.HScroll || this.HorizontalScroll.Visible)
                {
                    createParams.Style |= 0x100000;
                }
                else
                {
                    createParams.Style &= -1048577;
                }
                if (this.VScroll || this.VerticalScroll.Visible)
                {
                    createParams.Style |= 0x200000;
                    return createParams;
                }
                createParams.Style &= -2097153;
                return createParams;
            }
        }

        public override Rectangle DisplayRectangle
        {
            get
            {
                Rectangle clientRectangle = base.ClientRectangle;
                if (!this.displayRect.IsEmpty)
                {
                    clientRectangle.X = this.displayRect.X;
                    clientRectangle.Y = this.displayRect.Y;
                    if (this.HScroll)
                    {
                        clientRectangle.Width = this.displayRect.Width;
                    }
                    if (this.VScroll)
                    {
                        clientRectangle.Height = this.displayRect.Height;
                    }
                }
                return LayoutUtils.DeflateRect(clientRectangle, base.Padding);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public DockPaddingEdges DockPadding
        {
            get
            {
                if (this.dockPadding == null)
                {
                    this.dockPadding = new DockPaddingEdges(this);
                }
                return this.dockPadding;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Always), System.Windows.Forms.SRDescription("ScrollableControlHorizontalScrollDescr"), System.Windows.Forms.SRCategory("CatLayout")]
        public HScrollProperties HorizontalScroll
        {
            get
            {
                if (this.horizontalScroll == null)
                {
                    this.horizontalScroll = new HScrollProperties(this);
                }
                return this.horizontalScroll;
            }
        }

        protected bool HScroll
        {
            get
            {
                return this.GetScrollState(2);
            }
            set
            {
                this.SetScrollState(2, value);
            }
        }

        Rectangle IArrangedElement.DisplayRectangle
        {
            get
            {
                Rectangle displayRectangle = this.DisplayRectangle;
                if ((this.AutoScrollMinSize.Width != 0) && (this.AutoScrollMinSize.Height != 0))
                {
                    displayRectangle.Width = Math.Max(displayRectangle.Width, this.AutoScrollMinSize.Width);
                    displayRectangle.Height = Math.Max(displayRectangle.Height, this.AutoScrollMinSize.Height);
                }
                return displayRectangle;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(false), System.Windows.Forms.SRCategory("CatLayout"), System.Windows.Forms.SRDescription("ScrollableControlVerticalScrollDescr")]
        public VScrollProperties VerticalScroll
        {
            get
            {
                if (this.verticalScroll == null)
                {
                    this.verticalScroll = new VScrollProperties(this);
                }
                return this.verticalScroll;
            }
        }

        protected bool VScroll
        {
            get
            {
                return this.GetScrollState(4);
            }
            set
            {
                this.SetScrollState(4, value);
            }
        }

        [TypeConverter(typeof(ScrollableControl.DockPaddingEdgesConverter))]
        public class DockPaddingEdges : ICloneable
        {
            private int bottom;
            private int left;
            private ScrollableControl owner;
            private int right;
            private int top;

            internal DockPaddingEdges(ScrollableControl owner)
            {
                this.owner = owner;
            }

            internal DockPaddingEdges(int left, int right, int top, int bottom)
            {
                this.left = left;
                this.right = right;
                this.top = top;
                this.bottom = bottom;
            }

            public override bool Equals(object other)
            {
                ScrollableControl.DockPaddingEdges edges = other as ScrollableControl.DockPaddingEdges;
                return ((edges != null) && this.owner.Padding.Equals(edges.owner.Padding));
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            private void ResetAll()
            {
                this.All = 0;
            }

            private void ResetBottom()
            {
                this.Bottom = 0;
            }

            private void ResetLeft()
            {
                this.Left = 0;
            }

            private void ResetRight()
            {
                this.Right = 0;
            }

            private void ResetTop()
            {
                this.Top = 0;
            }

            internal void Scale(float dx, float dy)
            {
                this.owner.Padding.Scale(dx, dy);
            }

            object ICloneable.Clone()
            {
                return new ScrollableControl.DockPaddingEdges(this.Left, this.Right, this.Top, this.Bottom);
            }

            public override string ToString()
            {
                return "";
            }

            [System.Windows.Forms.SRDescription("PaddingAllDescr"), RefreshProperties(RefreshProperties.All)]
            public int All
            {
                get
                {
                    if (this.owner == null)
                    {
                        if (((this.left == this.right) && (this.top == this.bottom)) && (this.left == this.top))
                        {
                            return this.left;
                        }
                        return 0;
                    }
                    if ((this.owner.Padding.All != -1) || (((this.owner.Padding.Left == -1) && (this.owner.Padding.Top == -1)) && ((this.owner.Padding.Right == -1) && (this.owner.Padding.Bottom == -1))))
                    {
                        return this.owner.Padding.All;
                    }
                    return 0;
                }
                set
                {
                    if (this.owner == null)
                    {
                        this.left = value;
                        this.top = value;
                        this.right = value;
                        this.bottom = value;
                    }
                    else
                    {
                        this.owner.Padding = new Padding(value);
                    }
                }
            }

            [RefreshProperties(RefreshProperties.All), System.Windows.Forms.SRDescription("PaddingBottomDescr")]
            public int Bottom
            {
                get
                {
                    if (this.owner == null)
                    {
                        return this.bottom;
                    }
                    return this.owner.Padding.Bottom;
                }
                set
                {
                    if (this.owner == null)
                    {
                        this.bottom = value;
                    }
                    else
                    {
                        Padding padding = this.owner.Padding;
                        padding.Bottom = value;
                        this.owner.Padding = padding;
                    }
                }
            }

            [RefreshProperties(RefreshProperties.All), System.Windows.Forms.SRDescription("PaddingLeftDescr")]
            public int Left
            {
                get
                {
                    if (this.owner == null)
                    {
                        return this.left;
                    }
                    return this.owner.Padding.Left;
                }
                set
                {
                    if (this.owner == null)
                    {
                        this.left = value;
                    }
                    else
                    {
                        Padding padding = this.owner.Padding;
                        padding.Left = value;
                        this.owner.Padding = padding;
                    }
                }
            }

            [RefreshProperties(RefreshProperties.All), System.Windows.Forms.SRDescription("PaddingRightDescr")]
            public int Right
            {
                get
                {
                    if (this.owner == null)
                    {
                        return this.right;
                    }
                    return this.owner.Padding.Right;
                }
                set
                {
                    if (this.owner == null)
                    {
                        this.right = value;
                    }
                    else
                    {
                        Padding padding = this.owner.Padding;
                        padding.Right = value;
                        this.owner.Padding = padding;
                    }
                }
            }

            [RefreshProperties(RefreshProperties.All), System.Windows.Forms.SRDescription("PaddingTopDescr")]
            public int Top
            {
                get
                {
                    if (this.owner == null)
                    {
                        return this.bottom;
                    }
                    return this.owner.Padding.Top;
                }
                set
                {
                    if (this.owner == null)
                    {
                        this.top = value;
                    }
                    else
                    {
                        Padding padding = this.owner.Padding;
                        padding.Top = value;
                        this.owner.Padding = padding;
                    }
                }
            }
        }

        public class DockPaddingEdgesConverter : TypeConverter
        {
            public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
            {
                return TypeDescriptor.GetProperties(typeof(ScrollableControl.DockPaddingEdges), attributes).Sort(new string[] { "All", "Left", "Top", "Right", "Bottom" });
            }

            public override bool GetPropertiesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
        }
    }
}

