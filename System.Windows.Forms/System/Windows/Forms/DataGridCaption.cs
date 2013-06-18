namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Imaging;

    internal class DataGridCaption
    {
        private bool backActive;
        private SolidBrush backBrush = DefaultBackBrush;
        private Rectangle backButtonRect = new Rectangle();
        private bool backButtonVisible;
        private bool backPressed;
        private const int buttonToText = 4;
        private static ColorMap[] colorMap = new ColorMap[] { new ColorMap() };
        private DataGrid dataGrid;
        private System.Drawing.Font dataGridFont;
        private bool downActive;
        private bool downButtonDown;
        private Rectangle downButtonRect = new Rectangle();
        private bool downButtonVisible;
        private bool downPressed;
        private static readonly object EVENT_BACKWARDCLICKED = new object();
        private static readonly object EVENT_CAPTIONCLICKED = new object();
        private static readonly object EVENT_DOWNCLICKED = new object();
        private EventEntry eventList;
        internal EventHandlerList events;
        private SolidBrush foreBrush = DefaultForeBrush;
        private CaptionLocation lastMouseLocation;
        private static Bitmap leftButtonBitmap;
        private static Bitmap leftButtonBitmap_bidi;
        private static Bitmap magnifyingGlassBitmap;
        private static readonly Point minimumBounds = new Point(50, 30);
        private string text = "";
        private Pen textBorderPen = DefaultTextBorderPen;
        private bool textBorderVisible;
        private System.Drawing.Font textFont;
        private const int textPadding = 2;
        private Rectangle textRect = new Rectangle();
        private const int xOffset = 3;
        private const int yOffset = 1;

        internal event EventHandler BackwardClicked
        {
            add
            {
                this.Events.AddHandler(EVENT_BACKWARDCLICKED, value);
            }
            remove
            {
                this.Events.RemoveHandler(EVENT_BACKWARDCLICKED, value);
            }
        }

        internal event EventHandler CaptionClicked
        {
            add
            {
                this.Events.AddHandler(EVENT_CAPTIONCLICKED, value);
            }
            remove
            {
                this.Events.RemoveHandler(EVENT_CAPTIONCLICKED, value);
            }
        }

        internal event EventHandler DownClicked
        {
            add
            {
                this.Events.AddHandler(EVENT_DOWNCLICKED, value);
            }
            remove
            {
                this.Events.RemoveHandler(EVENT_DOWNCLICKED, value);
            }
        }

        internal DataGridCaption(DataGrid dataGrid)
        {
            this.dataGrid = dataGrid;
            this.downButtonVisible = dataGrid.ParentRowsVisible;
            colorMap[0].OldColor = Color.White;
            colorMap[0].NewColor = this.ForeColor;
            this.OnGridFontChanged();
        }

        protected virtual void AddEventHandler(object key, Delegate handler)
        {
            lock (this)
            {
                if (handler != null)
                {
                    for (EventEntry entry = this.eventList; entry != null; entry = entry.next)
                    {
                        if (entry.key == key)
                        {
                            entry.handler = Delegate.Combine(entry.handler, handler);
                            goto Label_0060;
                        }
                    }
                    this.eventList = new EventEntry(this.eventList, key, handler);
                }
            Label_0060:;
            }
        }

        private CaptionLocation FindLocation(int x, int y)
        {
            if (!this.backButtonRect.IsEmpty && this.backButtonRect.Contains(x, y))
            {
                return CaptionLocation.BackButton;
            }
            if (!this.downButtonRect.IsEmpty && this.downButtonRect.Contains(x, y))
            {
                return CaptionLocation.DownButton;
            }
            if (!this.textRect.IsEmpty && this.textRect.Contains(x, y))
            {
                return CaptionLocation.Text;
            }
            return CaptionLocation.Nowhere;
        }

        private Bitmap GetBackButtonBmp(bool alignRight)
        {
            if (alignRight)
            {
                if (leftButtonBitmap_bidi == null)
                {
                    leftButtonBitmap_bidi = this.GetBitmap("DataGridCaption.backarrow_bidi.bmp");
                }
                return leftButtonBitmap_bidi;
            }
            if (leftButtonBitmap == null)
            {
                leftButtonBitmap = this.GetBitmap("DataGridCaption.backarrow.bmp");
            }
            return leftButtonBitmap;
        }

        internal Rectangle GetBackButtonRect(Rectangle bounds, bool alignRight, int downButtonWidth)
        {
            Size size;
            Bitmap backButtonBmp = this.GetBackButtonBmp(false);
            lock (backButtonBmp)
            {
                size = backButtonBmp.Size;
            }
            return new Rectangle(((bounds.Right - 12) - downButtonWidth) - size.Width, (bounds.Y + 1) + 2, size.Width, size.Height);
        }

        private Bitmap GetBitmap(string bitmapName)
        {
            Bitmap bitmap = null;
            try
            {
                bitmap = new Bitmap(typeof(DataGridCaption), bitmapName);
                bitmap.MakeTransparent();
            }
            catch (Exception)
            {
            }
            return bitmap;
        }

        private Bitmap GetDetailsBmp()
        {
            if (magnifyingGlassBitmap == null)
            {
                magnifyingGlassBitmap = this.GetBitmap("DataGridCaption.Details.bmp");
            }
            return magnifyingGlassBitmap;
        }

        internal Rectangle GetDetailsButtonRect(Rectangle bounds, bool alignRight)
        {
            Size size;
            Bitmap detailsBmp = this.GetDetailsBmp();
            lock (detailsBmp)
            {
                size = detailsBmp.Size;
            }
            int width = size.Width;
            return new Rectangle((bounds.Right - 6) - width, (bounds.Y + 1) + 2, width, size.Height);
        }

        internal int GetDetailsButtonWidth()
        {
            Bitmap detailsBmp = this.GetDetailsBmp();
            lock (detailsBmp)
            {
                return detailsBmp.Size.Width;
            }
        }

        internal bool GetDownButtonDirection()
        {
            return this.DownButtonDown;
        }

        protected virtual Delegate GetEventHandler(object key)
        {
            lock (this)
            {
                for (EventEntry entry = this.eventList; entry != null; entry = entry.next)
                {
                    if (entry.key == key)
                    {
                        return entry.handler;
                    }
                }
                return null;
            }
        }

        private void Invalidate()
        {
            if (this.dataGrid != null)
            {
                this.dataGrid.InvalidateCaption();
            }
        }

        private void InvalidateCaptionRect(Rectangle r)
        {
            if (this.dataGrid != null)
            {
                this.dataGrid.InvalidateCaptionRect(r);
            }
        }

        private void InvalidateLocation(CaptionLocation loc)
        {
            Rectangle backButtonRect;
            switch (loc)
            {
                case CaptionLocation.BackButton:
                    backButtonRect = this.backButtonRect;
                    backButtonRect.Inflate(1, 1);
                    this.InvalidateCaptionRect(backButtonRect);
                    return;

                case CaptionLocation.DownButton:
                    backButtonRect = this.downButtonRect;
                    backButtonRect.Inflate(1, 1);
                    this.InvalidateCaptionRect(backButtonRect);
                    return;
            }
        }

        internal void MouseDown(int x, int y)
        {
            CaptionLocation loc = this.FindLocation(x, y);
            switch (loc)
            {
                case CaptionLocation.BackButton:
                    this.backPressed = true;
                    this.InvalidateLocation(loc);
                    return;

                case CaptionLocation.DownButton:
                    this.downPressed = true;
                    this.InvalidateLocation(loc);
                    return;

                case CaptionLocation.Text:
                    this.OnCaptionClicked(EventArgs.Empty);
                    return;
            }
        }

        internal void MouseLeft()
        {
            CaptionLocation lastMouseLocation = this.lastMouseLocation;
            this.lastMouseLocation = CaptionLocation.Nowhere;
            this.InvalidateLocation(lastMouseLocation);
        }

        internal void MouseOver(int x, int y)
        {
            CaptionLocation loc = this.FindLocation(x, y);
            this.InvalidateLocation(this.lastMouseLocation);
            this.InvalidateLocation(loc);
            this.lastMouseLocation = loc;
        }

        internal void MouseUp(int x, int y)
        {
            switch (this.FindLocation(x, y))
            {
                case CaptionLocation.BackButton:
                    if (this.backPressed)
                    {
                        this.backPressed = false;
                        this.OnBackwardClicked(EventArgs.Empty);
                    }
                    break;

                case CaptionLocation.DownButton:
                    if (!this.downPressed)
                    {
                        break;
                    }
                    this.downPressed = false;
                    this.OnDownClicked(EventArgs.Empty);
                    return;

                default:
                    return;
            }
        }

        protected void OnBackwardClicked(EventArgs e)
        {
            if (this.backActive)
            {
                EventHandler handler = (EventHandler) this.Events[EVENT_BACKWARDCLICKED];
                if (handler != null)
                {
                    handler(this, e);
                }
            }
        }

        protected void OnCaptionClicked(EventArgs e)
        {
            EventHandler handler = (EventHandler) this.Events[EVENT_CAPTIONCLICKED];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected void OnDownClicked(EventArgs e)
        {
            if (this.downActive && this.downButtonVisible)
            {
                EventHandler handler = (EventHandler) this.Events[EVENT_DOWNCLICKED];
                if (handler != null)
                {
                    handler(this, e);
                }
            }
        }

        internal void OnGridFontChanged()
        {
            if ((this.dataGridFont == null) || !this.dataGridFont.Equals(this.dataGrid.Font))
            {
                try
                {
                    this.dataGridFont = new System.Drawing.Font(this.dataGrid.Font, FontStyle.Bold);
                }
                catch
                {
                }
            }
        }

        internal void Paint(Graphics g, Rectangle bounds, bool alignRight)
        {
            Size size = new Size(((int) g.MeasureString(this.text, this.Font).Width) + 2, this.Font.Height + 2);
            this.downButtonRect = this.GetDetailsButtonRect(bounds, alignRight);
            int detailsButtonWidth = this.GetDetailsButtonWidth();
            this.backButtonRect = this.GetBackButtonRect(bounds, alignRight, detailsButtonWidth);
            int num2 = this.backButtonVisible ? ((this.backButtonRect.Width + 3) + 4) : 0;
            int num3 = (this.downButtonVisible && !this.dataGrid.ParentRowsIsEmpty()) ? ((detailsButtonWidth + 3) + 4) : 0;
            int num4 = ((bounds.Width - 3) - num2) - num3;
            this.textRect = new Rectangle(bounds.X, bounds.Y + 1, Math.Min(num4, 4 + size.Width), 4 + size.Height);
            if (alignRight)
            {
                this.textRect.X = bounds.Right - this.textRect.Width;
                this.backButtonRect.X = (bounds.X + 12) + detailsButtonWidth;
                this.downButtonRect.X = bounds.X + 6;
            }
            g.FillRectangle(this.backBrush, bounds);
            if (this.backButtonVisible)
            {
                this.PaintBackButton(g, this.backButtonRect, alignRight);
                if (this.backActive && (this.lastMouseLocation == CaptionLocation.BackButton))
                {
                    this.backButtonRect.Inflate(1, 1);
                    ControlPaint.DrawBorder3D(g, this.backButtonRect, this.backPressed ? Border3DStyle.SunkenInner : Border3DStyle.RaisedInner);
                }
            }
            this.PaintText(g, this.textRect, alignRight);
            if (this.downButtonVisible && !this.dataGrid.ParentRowsIsEmpty())
            {
                this.PaintDownButton(g, this.downButtonRect);
                if (this.lastMouseLocation == CaptionLocation.DownButton)
                {
                    this.downButtonRect.Inflate(1, 1);
                    ControlPaint.DrawBorder3D(g, this.downButtonRect, this.downPressed ? Border3DStyle.SunkenInner : Border3DStyle.RaisedInner);
                }
            }
        }

        private void PaintBackButton(Graphics g, Rectangle bounds, bool alignRight)
        {
            Bitmap backButtonBmp = this.GetBackButtonBmp(alignRight);
            lock (backButtonBmp)
            {
                this.PaintIcon(g, bounds, backButtonBmp);
            }
        }

        private void PaintDownButton(Graphics g, Rectangle bounds)
        {
            Bitmap detailsBmp = this.GetDetailsBmp();
            lock (detailsBmp)
            {
                this.PaintIcon(g, bounds, detailsBmp);
            }
        }

        private void PaintIcon(Graphics g, Rectangle bounds, Bitmap b)
        {
            ImageAttributes imageAttr = new ImageAttributes();
            imageAttr.SetRemapTable(colorMap, ColorAdjustType.Bitmap);
            g.DrawImage(b, bounds, 0, 0, bounds.Width, bounds.Height, GraphicsUnit.Pixel, imageAttr);
            imageAttr.Dispose();
        }

        private void PaintText(Graphics g, Rectangle bounds, bool alignToRight)
        {
            Rectangle rect = bounds;
            if ((rect.Width > 0) && (rect.Height > 0))
            {
                if (this.textBorderVisible)
                {
                    g.DrawRectangle(this.textBorderPen, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
                    rect.Inflate(-1, -1);
                }
                Rectangle rectangle2 = rect;
                rectangle2.Height = 2;
                g.FillRectangle(this.backBrush, rectangle2);
                rectangle2.Y = rect.Bottom - 2;
                g.FillRectangle(this.backBrush, rectangle2);
                rectangle2 = new Rectangle(rect.X, rect.Y + 2, 2, rect.Height - 4);
                g.FillRectangle(this.backBrush, rectangle2);
                rectangle2.X = rect.Right - 2;
                g.FillRectangle(this.backBrush, rectangle2);
                rect.Inflate(-2, -2);
                g.FillRectangle(this.backBrush, rect);
                StringFormat format = new StringFormat();
                if (alignToRight)
                {
                    format.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
                    format.Alignment = StringAlignment.Far;
                }
                g.DrawString(this.text, this.Font, this.foreBrush, rect, format);
                format.Dispose();
            }
        }

        protected virtual void RaiseEvent(object key, EventArgs e)
        {
            Delegate eventHandler = this.GetEventHandler(key);
            if (eventHandler != null)
            {
                ((EventHandler) eventHandler)(this, e);
            }
        }

        protected virtual void RemoveEventHandler(object key, Delegate handler)
        {
            lock (this)
            {
                if (handler != null)
                {
                    EventEntry eventList = this.eventList;
                    EventEntry entry2 = null;
                    while (eventList != null)
                    {
                        if (eventList.key == key)
                        {
                            eventList.handler = Delegate.Remove(eventList.handler, handler);
                            if (eventList.handler == null)
                            {
                                if (entry2 == null)
                                {
                                    this.eventList = eventList.next;
                                }
                                else
                                {
                                    entry2.next = eventList.next;
                                }
                            }
                            break;
                        }
                        entry2 = eventList;
                        eventList = eventList.next;
                    }
                }
            }
        }

        protected virtual void RemoveEventHandlers()
        {
            this.eventList = null;
        }

        internal void ResetBackColor()
        {
            if (this.ShouldSerializeBackColor())
            {
                this.backBrush = DefaultBackBrush;
                this.Invalidate();
            }
        }

        internal void ResetFont()
        {
            this.textFont = null;
            this.Invalidate();
        }

        internal void ResetForeColor()
        {
            if (this.ShouldSerializeForeColor())
            {
                this.foreBrush = DefaultForeBrush;
                this.Invalidate();
            }
        }

        internal void SetDownButtonDirection(bool pointDown)
        {
            this.DownButtonDown = pointDown;
        }

        internal bool ShouldSerializeBackColor()
        {
            return !this.backBrush.Equals(DefaultBackBrush);
        }

        internal bool ShouldSerializeFont()
        {
            return ((this.textFont != null) && !this.textFont.Equals(this.dataGridFont));
        }

        internal bool ShouldSerializeForeColor()
        {
            return !this.foreBrush.Equals(DefaultForeBrush);
        }

        internal bool ToggleDownButtonDirection()
        {
            this.DownButtonDown = !this.DownButtonDown;
            return this.DownButtonDown;
        }

        internal bool BackButtonActive
        {
            get
            {
                return this.backActive;
            }
            set
            {
                if (this.backActive != value)
                {
                    this.backActive = value;
                    this.InvalidateCaptionRect(this.backButtonRect);
                }
            }
        }

        internal bool BackButtonVisible
        {
            get
            {
                return this.backButtonVisible;
            }
            set
            {
                if (this.backButtonVisible != value)
                {
                    this.backButtonVisible = value;
                    this.Invalidate();
                }
            }
        }

        internal Color BackColor
        {
            get
            {
                return this.backBrush.Color;
            }
            set
            {
                if (!this.backBrush.Color.Equals(value))
                {
                    if (value.IsEmpty)
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridEmptyColor", new object[] { "Caption BackColor" }));
                    }
                    this.backBrush = new SolidBrush(value);
                    this.Invalidate();
                }
            }
        }

        internal static SolidBrush DefaultBackBrush
        {
            get
            {
                return (SolidBrush) SystemBrushes.ActiveCaption;
            }
        }

        internal static SolidBrush DefaultForeBrush
        {
            get
            {
                return (SolidBrush) SystemBrushes.ActiveCaptionText;
            }
        }

        internal static Pen DefaultTextBorderPen
        {
            get
            {
                return new Pen(SystemColors.ActiveCaptionText);
            }
        }

        internal bool DownButtonActive
        {
            get
            {
                return this.downActive;
            }
            set
            {
                if (this.downActive != value)
                {
                    this.downActive = value;
                    this.InvalidateCaptionRect(this.downButtonRect);
                }
            }
        }

        private bool DownButtonDown
        {
            get
            {
                return this.downButtonDown;
            }
            set
            {
                if (this.downButtonDown != value)
                {
                    this.downButtonDown = value;
                    this.InvalidateLocation(CaptionLocation.DownButton);
                }
            }
        }

        internal bool DownButtonVisible
        {
            get
            {
                return this.downButtonVisible;
            }
            set
            {
                if (this.downButtonVisible != value)
                {
                    this.downButtonVisible = value;
                    this.Invalidate();
                }
            }
        }

        internal EventHandlerList Events
        {
            get
            {
                if (this.events == null)
                {
                    this.events = new EventHandlerList();
                }
                return this.events;
            }
        }

        internal System.Drawing.Font Font
        {
            get
            {
                if (this.textFont == null)
                {
                    return this.dataGridFont;
                }
                return this.textFont;
            }
            set
            {
                if ((this.textFont == null) || !this.textFont.Equals(value))
                {
                    this.textFont = value;
                    if (this.dataGrid.Caption != null)
                    {
                        this.dataGrid.RecalculateFonts();
                        this.dataGrid.PerformLayout();
                        this.dataGrid.Invalidate();
                    }
                }
            }
        }

        internal Color ForeColor
        {
            get
            {
                return this.foreBrush.Color;
            }
            set
            {
                if (value.IsEmpty)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridEmptyColor", new object[] { "Caption ForeColor" }));
                }
                this.foreBrush = new SolidBrush(value);
                colorMap[0].NewColor = this.ForeColor;
                this.Invalidate();
            }
        }

        internal Point MinimumBounds
        {
            get
            {
                return minimumBounds;
            }
        }

        internal string Text
        {
            get
            {
                return this.text;
            }
            set
            {
                if (value == null)
                {
                    this.text = "";
                }
                else
                {
                    this.text = value;
                }
                this.Invalidate();
            }
        }

        internal bool TextBorderVisible
        {
            get
            {
                return this.textBorderVisible;
            }
            set
            {
                this.textBorderVisible = value;
                this.Invalidate();
            }
        }

        internal enum CaptionLocation
        {
            Nowhere,
            BackButton,
            DownButton,
            Text
        }

        private sealed class EventEntry
        {
            internal Delegate handler;
            internal object key;
            internal DataGridCaption.EventEntry next;

            internal EventEntry(DataGridCaption.EventEntry next, object key, Delegate handler)
            {
                this.next = next;
                this.key = key;
                this.handler = handler;
            }
        }
    }
}

