namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Windows.Forms.Internal;
    using System.Windows.Forms.Layout;

    [ToolboxItem("System.Windows.Forms.Design.AutoSizeToolboxItem,System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), System.Windows.Forms.SRDescription("DescriptionLinkLabel"), ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch), DefaultEvent("LinkClicked")]
    public class LinkLabel : Label, IButtonControl
    {
        private Color activeLinkColor = Color.Empty;
        private DialogResult dialogResult;
        private Color disabledLinkColor = Color.Empty;
        private static readonly object EventLinkClicked = new object();
        private Link focusLink;
        private Font hoverLinkFont;
        private static Color iedisabledLinkColor = Color.Empty;
        private System.Windows.Forms.LinkBehavior linkBehavior;
        private LinkCollection linkCollection;
        private Color linkColor = Color.Empty;
        private static LinkComparer linkComparer = new LinkComparer();
        private Font linkFont;
        private ArrayList links = new ArrayList(2);
        private Cursor overrideCursor;
        private bool processingOnGotFocus;
        private bool receivedDoubleClick;
        private bool textLayoutValid;
        private Region textRegion;
        private Color visitedLinkColor = Color.Empty;

        [WinCategory("Action"), System.Windows.Forms.SRDescription("LinkLabelLinkClickedDescr")]
        public event LinkLabelLinkClickedEventHandler LinkClicked
        {
            add
            {
                base.Events.AddHandler(EventLinkClicked, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventLinkClicked, value);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        public event EventHandler TabStopChanged
        {
            add
            {
                base.TabStopChanged += value;
            }
            remove
            {
                base.TabStopChanged -= value;
            }
        }

        public LinkLabel()
        {
            base.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.StandardClick | ControlStyles.ResizeRedraw | ControlStyles.Opaque | ControlStyles.UserPaint, true);
            this.ResetLinkArea();
        }

        private CharacterRange[] AdjustCharacterRangesForSurrogateChars()
        {
            string text = this.Text;
            if (string.IsNullOrEmpty(text))
            {
                return new CharacterRange[0];
            }
            StringInfo info = new StringInfo(text);
            int lengthInTextElements = info.LengthInTextElements;
            ArrayList list = new ArrayList(this.Links.Count);
            foreach (Link link in this.Links)
            {
                int start = ConvertToCharIndex(link.Start, text);
                int num3 = ConvertToCharIndex(link.Start + link.Length, text);
                if (this.LinkInText(start, num3 - start))
                {
                    int num4 = Math.Min(link.Length, lengthInTextElements - link.Start);
                    list.Add(new CharacterRange(start, ConvertToCharIndex(link.Start + num4, text) - start));
                }
            }
            CharacterRange[] array = new CharacterRange[list.Count + 1];
            list.CopyTo(array, 0);
            array[array.Length - 1] = new CharacterRange(0, text.Length);
            return array;
        }

        internal static Rectangle CalcTextRenderBounds(Rectangle textRect, Rectangle clientRect, ContentAlignment align)
        {
            int x;
            int y;
            int width;
            int height;
            if ((align & WindowsFormsUtils.AnyRightAlign) != ((ContentAlignment) 0))
            {
                x = clientRect.Right - textRect.Width;
            }
            else if ((align & WindowsFormsUtils.AnyCenterAlign) != ((ContentAlignment) 0))
            {
                x = (clientRect.Width - textRect.Width) / 2;
            }
            else
            {
                x = clientRect.X;
            }
            if ((align & WindowsFormsUtils.AnyBottomAlign) != ((ContentAlignment) 0))
            {
                y = clientRect.Bottom - textRect.Height;
            }
            else if ((align & WindowsFormsUtils.AnyMiddleAlign) != ((ContentAlignment) 0))
            {
                y = (clientRect.Height - textRect.Height) / 2;
            }
            else
            {
                y = clientRect.Y;
            }
            if (textRect.Width > clientRect.Width)
            {
                x = clientRect.X;
                width = clientRect.Width;
            }
            else
            {
                width = textRect.Width;
            }
            if (textRect.Height > clientRect.Height)
            {
                y = clientRect.Y;
                height = clientRect.Height;
            }
            else
            {
                height = textRect.Height;
            }
            return new Rectangle(x, y, width, height);
        }

        private static int ConvertToCharIndex(int index, string text)
        {
            if (index <= 0)
            {
                return 0;
            }
            if (string.IsNullOrEmpty(text))
            {
                return index;
            }
            StringInfo info = new StringInfo(text);
            int lengthInTextElements = info.LengthInTextElements;
            if (index > lengthInTextElements)
            {
                return ((index - lengthInTextElements) + text.Length);
            }
            return info.SubstringByTextElements(0, index).Length;
        }

        protected override AccessibleObject CreateAccessibilityInstance()
        {
            return new LinkLabelAccessibleObject(this);
        }

        protected override void CreateHandle()
        {
            base.CreateHandle();
            this.InvalidateTextLayout();
        }

        internal override StringFormat CreateStringFormat()
        {
            StringFormat format = base.CreateStringFormat();
            if (!string.IsNullOrEmpty(this.Text))
            {
                CharacterRange[] ranges = this.AdjustCharacterRangesForSurrogateChars();
                format.SetMeasurableCharacterRanges(ranges);
            }
            return format;
        }

        private void EnsureRun(Graphics g)
        {
            if (!this.textLayoutValid)
            {
                if (this.textRegion != null)
                {
                    this.textRegion.Dispose();
                    this.textRegion = null;
                }
                if (this.Text.Length == 0)
                {
                    this.Links.Clear();
                    this.Links.Add(new Link(0, -1));
                    this.textLayoutValid = true;
                }
                else
                {
                    StringFormat stringFormat = this.CreateStringFormat();
                    string text = this.Text;
                    try
                    {
                        Font font = new Font(this.Font, this.Font.Style | FontStyle.Underline);
                        Graphics graphics = null;
                        try
                        {
                            if (g == null)
                            {
                                g = graphics = base.CreateGraphicsInternal();
                            }
                            if (this.UseCompatibleTextRendering)
                            {
                                Region[] regionArray = g.MeasureCharacterRanges(text, font, this.ClientRectWithPadding, stringFormat);
                                int index = 0;
                                for (int i = 0; i < this.Links.Count; i++)
                                {
                                    Link link = this.Links[i];
                                    int start = ConvertToCharIndex(link.Start, text);
                                    int num4 = ConvertToCharIndex(link.Start + link.Length, text);
                                    if (this.LinkInText(start, num4 - start))
                                    {
                                        this.Links[i].VisualRegion = regionArray[index];
                                        index++;
                                    }
                                }
                                this.textRegion = regionArray[regionArray.Length - 1];
                            }
                            else
                            {
                                int iLeftMargin;
                                int iRightMargin;
                                Rectangle clientRectWithPadding = this.ClientRectWithPadding;
                                Size constrainingSize = new Size(clientRectWithPadding.Width, clientRectWithPadding.Height);
                                TextFormatFlags flags = this.CreateTextFormatFlags(constrainingSize);
                                Size size2 = TextRenderer.MeasureText(text, font, constrainingSize, flags);
                                using (WindowsGraphics graphics2 = WindowsGraphics.FromGraphics(g))
                                {
                                    if ((flags & TextFormatFlags.NoPadding) == TextFormatFlags.NoPadding)
                                    {
                                        graphics2.TextPadding = TextPaddingOptions.NoPadding;
                                    }
                                    else if ((flags & TextFormatFlags.LeftAndRightPadding) == TextFormatFlags.LeftAndRightPadding)
                                    {
                                        graphics2.TextPadding = TextPaddingOptions.LeftAndRightPadding;
                                    }
                                    using (WindowsFont font2 = WindowsGraphicsCacheManager.GetWindowsFont(this.Font))
                                    {
                                        IntNativeMethods.DRAWTEXTPARAMS textMargins = graphics2.GetTextMargins(font2);
                                        iLeftMargin = textMargins.iLeftMargin;
                                        iRightMargin = textMargins.iRightMargin;
                                    }
                                }
                                Rectangle textRect = new Rectangle(clientRectWithPadding.X + iLeftMargin, clientRectWithPadding.Y, (size2.Width - iRightMargin) - iLeftMargin, size2.Height);
                                Region region = new Region(CalcTextRenderBounds(textRect, clientRectWithPadding, base.RtlTranslateContent(this.TextAlign)));
                                if ((this.links != null) && (this.links.Count == 1))
                                {
                                    this.Links[0].VisualRegion = region;
                                }
                                this.textRegion = region;
                            }
                        }
                        finally
                        {
                            font.Dispose();
                            font = null;
                            if (graphics != null)
                            {
                                graphics.Dispose();
                                graphics = null;
                            }
                        }
                        this.textLayoutValid = true;
                    }
                    finally
                    {
                        stringFormat.Dispose();
                    }
                }
            }
        }

        private bool FocusNextLink(bool forward)
        {
            int focusIndex = -1;
            if (this.focusLink != null)
            {
                for (int i = 0; i < this.links.Count; i++)
                {
                    if (this.links[i] == this.focusLink)
                    {
                        focusIndex = i;
                        break;
                    }
                }
            }
            focusIndex = this.GetNextLinkIndex(focusIndex, forward);
            if (focusIndex != -1)
            {
                this.FocusLink = this.Links[focusIndex];
                return true;
            }
            this.FocusLink = null;
            return false;
        }

        private int GetNextLinkIndex(int focusIndex, bool forward)
        {
            Link link;
            string text = this.Text;
            int start = 0;
            int num2 = 0;
            if (forward)
            {
                do
                {
                    focusIndex++;
                    if (focusIndex < this.Links.Count)
                    {
                        link = this.Links[focusIndex];
                        start = ConvertToCharIndex(link.Start, text);
                        num2 = ConvertToCharIndex(link.Start + link.Length, text);
                    }
                    else
                    {
                        link = null;
                    }
                }
                while (((link != null) && !link.Enabled) && this.LinkInText(start, num2 - start));
            }
            else
            {
                do
                {
                    focusIndex--;
                    if (focusIndex >= 0)
                    {
                        link = this.Links[focusIndex];
                        start = ConvertToCharIndex(link.Start, text);
                        num2 = ConvertToCharIndex(link.Start + link.Length, text);
                    }
                    else
                    {
                        link = null;
                    }
                }
                while (((link != null) && !link.Enabled) && this.LinkInText(start, num2 - start));
            }
            if ((focusIndex >= 0) && (focusIndex < this.links.Count))
            {
                return focusIndex;
            }
            return -1;
        }

        private void InvalidateLink(Link link)
        {
            if (base.IsHandleCreated)
            {
                if (((link == null) || (link.VisualRegion == null)) || this.IsOneLink())
                {
                    base.Invalidate();
                }
                else
                {
                    base.Invalidate(link.VisualRegion);
                }
            }
        }

        private void InvalidateLinkFonts()
        {
            if (this.linkFont != null)
            {
                this.linkFont.Dispose();
            }
            if ((this.hoverLinkFont != null) && (this.hoverLinkFont != this.linkFont))
            {
                this.hoverLinkFont.Dispose();
            }
            this.linkFont = null;
            this.hoverLinkFont = null;
        }

        private void InvalidateTextLayout()
        {
            this.textLayoutValid = false;
        }

        private bool IsOneLink()
        {
            if (((this.links == null) || (this.links.Count != 1)) || (this.Text == null))
            {
                return false;
            }
            StringInfo info = new StringInfo(this.Text);
            return ((this.LinkArea.Start == 0) && (this.LinkArea.Length == info.LengthInTextElements));
        }

        private bool LinkInText(int start, int length)
        {
            return (((0 <= start) && (start < this.Text.Length)) && (0 < length));
        }

        internal override void OnAutoEllipsisChanged()
        {
            base.OnAutoEllipsisChanged();
            this.InvalidateTextLayout();
        }

        protected override void OnAutoSizeChanged(EventArgs e)
        {
            base.OnAutoSizeChanged(e);
            this.InvalidateTextLayout();
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            if (!base.Enabled)
            {
                for (int i = 0; i < this.links.Count; i++)
                {
                    Link link1 = (Link) this.links[i];
                    link1.State &= ~(LinkState.Active | LinkState.Hover);
                }
                this.OverrideCursor = null;
            }
            this.InvalidateTextLayout();
            base.Invalidate();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            this.InvalidateTextLayout();
            this.InvalidateLinkFonts();
            base.Invalidate();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            if (!this.processingOnGotFocus)
            {
                base.OnGotFocus(e);
                this.processingOnGotFocus = true;
            }
            try
            {
                Link focusLink = this.FocusLink;
                if (focusLink == null)
                {
                    System.Windows.Forms.IntSecurity.ModifyFocus.Assert();
                    this.Select(true, true);
                }
                else
                {
                    this.InvalidateLink(focusLink);
                    this.UpdateAccessibilityLink(focusLink);
                }
            }
            finally
            {
                if (this.processingOnGotFocus)
                {
                    this.processingOnGotFocus = false;
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (((e.KeyCode == Keys.Enter) && (this.FocusLink != null)) && this.FocusLink.Enabled)
            {
                this.OnLinkClicked(new LinkLabelLinkClickedEventArgs(this.FocusLink));
            }
        }

        protected virtual void OnLinkClicked(LinkLabelLinkClickedEventArgs e)
        {
            LinkLabelLinkClickedEventHandler handler = (LinkLabelLinkClickedEventHandler) base.Events[EventLinkClicked];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            if (this.FocusLink != null)
            {
                this.InvalidateLink(this.FocusLink);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (!base.Enabled || (e.Clicks > 1))
            {
                this.receivedDoubleClick = true;
            }
            else
            {
                for (int i = 0; i < this.links.Count; i++)
                {
                    if ((((Link) this.links[i]).State & LinkState.Hover) == LinkState.Hover)
                    {
                        Link link1 = (Link) this.links[i];
                        link1.State |= LinkState.Active;
                        this.FocusInternal();
                        if (((Link) this.links[i]).Enabled)
                        {
                            this.FocusLink = (Link) this.links[i];
                            this.InvalidateLink(this.FocusLink);
                        }
                        base.CaptureInternal = true;
                        return;
                    }
                }
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (base.Enabled)
            {
                foreach (Link link in this.links)
                {
                    if (((link.State & LinkState.Hover) == LinkState.Hover) || ((link.State & LinkState.Active) == LinkState.Active))
                    {
                        bool flag = (link.State & LinkState.Active) == LinkState.Active;
                        link.State &= ~(LinkState.Active | LinkState.Hover);
                        if (flag || (this.hoverLinkFont != this.linkFont))
                        {
                            this.InvalidateLink(link);
                        }
                        this.OverrideCursor = null;
                    }
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (base.Enabled)
            {
                Link link = null;
                foreach (Link link2 in this.links)
                {
                    if ((link2.State & LinkState.Hover) == LinkState.Hover)
                    {
                        link = link2;
                        break;
                    }
                }
                Link link3 = this.PointInLink(e.X, e.Y);
                if (link3 != link)
                {
                    if (link != null)
                    {
                        link.State &= ~LinkState.Hover;
                    }
                    if (link3 != null)
                    {
                        link3.State |= LinkState.Hover;
                        if (link3.Enabled)
                        {
                            this.OverrideCursor = Cursors.Hand;
                        }
                    }
                    else
                    {
                        this.OverrideCursor = null;
                    }
                    if (this.hoverLinkFont != this.linkFont)
                    {
                        if (link != null)
                        {
                            this.InvalidateLink(link);
                        }
                        if (link3 != null)
                        {
                            this.InvalidateLink(link3);
                        }
                    }
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (!base.Disposing && !base.IsDisposed)
            {
                if ((!base.Enabled || (e.Clicks > 1)) || this.receivedDoubleClick)
                {
                    this.receivedDoubleClick = false;
                }
                else
                {
                    for (int i = 0; i < this.links.Count; i++)
                    {
                        if ((((Link) this.links[i]).State & LinkState.Active) == LinkState.Active)
                        {
                            Link link1 = (Link) this.links[i];
                            link1.State &= ~LinkState.Active;
                            this.InvalidateLink((Link) this.links[i]);
                            base.CaptureInternal = false;
                            Link link = this.PointInLink(e.X, e.Y);
                            if (((link != null) && (link == this.FocusLink)) && link.Enabled)
                            {
                                this.OnLinkClicked(new LinkLabelLinkClickedEventArgs(link, e.Button));
                            }
                        }
                    }
                }
            }
        }

        protected override void OnPaddingChanged(EventArgs e)
        {
            base.OnPaddingChanged(e);
            this.InvalidateTextLayout();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            RectangleF empty = RectangleF.Empty;
            base.Animate();
            ImageAnimator.UpdateFrames();
            this.EnsureRun(e.Graphics);
            if (this.Text.Length == 0)
            {
                this.PaintLinkBackground(e.Graphics);
            }
            else
            {
                if (base.AutoEllipsis)
                {
                    Rectangle clientRectWithPadding = this.ClientRectWithPadding;
                    Size preferredSize = this.GetPreferredSize(new Size(clientRectWithPadding.Width, clientRectWithPadding.Height));
                    base.showToolTip = (clientRectWithPadding.Width < preferredSize.Width) || (clientRectWithPadding.Height < preferredSize.Height);
                }
                else
                {
                    base.showToolTip = false;
                }
                if (base.Enabled)
                {
                    bool optimizeBackgroundRendering = !base.GetStyle(ControlStyles.OptimizedDoubleBuffer);
                    SolidBrush foreBrush = new SolidBrush(this.ForeColor);
                    SolidBrush linkBrush = new SolidBrush(this.LinkColor);
                    try
                    {
                        if (!optimizeBackgroundRendering)
                        {
                            this.PaintLinkBackground(e.Graphics);
                        }
                        LinkUtilities.EnsureLinkFonts(this.Font, this.LinkBehavior, ref this.linkFont, ref this.hoverLinkFont);
                        Region region = e.Graphics.Clip;
                        try
                        {
                            if (this.IsOneLink())
                            {
                                e.Graphics.Clip = region;
                                RectangleF[] regionScans = ((Link) this.links[0]).VisualRegion.GetRegionScans(e.Graphics.Transform);
                                if ((regionScans == null) || (regionScans.Length <= 0))
                                {
                                    goto Label_02B7;
                                }
                                if (this.UseCompatibleTextRendering)
                                {
                                    empty = new RectangleF(regionScans[0].Location, SizeF.Empty);
                                    foreach (RectangleF ef2 in regionScans)
                                    {
                                        empty = RectangleF.Union(empty, ef2);
                                    }
                                }
                                else
                                {
                                    empty = this.ClientRectWithPadding;
                                    Size proposedConstraints = empty.Size.ToSize();
                                    Size size3 = base.MeasureTextCache.GetTextSize(this.Text, this.Font, proposedConstraints, this.CreateTextFormatFlags(proposedConstraints));
                                    empty.Width = size3.Width;
                                    if (size3.Height < empty.Height)
                                    {
                                        empty.Height = size3.Height;
                                    }
                                    empty = CalcTextRenderBounds(Rectangle.Round(empty), this.ClientRectWithPadding, base.RtlTranslateContent(this.TextAlign));
                                }
                                using (Region region2 = new Region(empty))
                                {
                                    e.Graphics.ExcludeClip(region2);
                                    goto Label_02B7;
                                }
                            }
                            foreach (Link link in this.links)
                            {
                                if (link.VisualRegion != null)
                                {
                                    e.Graphics.ExcludeClip(link.VisualRegion);
                                }
                            }
                        Label_02B7:
                            if (!this.IsOneLink())
                            {
                                this.PaintLink(e.Graphics, null, foreBrush, linkBrush, optimizeBackgroundRendering, empty);
                            }
                            foreach (Link link2 in this.links)
                            {
                                this.PaintLink(e.Graphics, link2, foreBrush, linkBrush, optimizeBackgroundRendering, empty);
                            }
                            if (optimizeBackgroundRendering)
                            {
                                e.Graphics.Clip = region;
                                e.Graphics.ExcludeClip(this.textRegion);
                                this.PaintLinkBackground(e.Graphics);
                            }
                        }
                        finally
                        {
                            e.Graphics.Clip = region;
                        }
                        goto Label_045B;
                    }
                    finally
                    {
                        foreBrush.Dispose();
                        linkBrush.Dispose();
                    }
                }
                Region clip = e.Graphics.Clip;
                try
                {
                    this.PaintLinkBackground(e.Graphics);
                    e.Graphics.IntersectClip(this.textRegion);
                    if (this.UseCompatibleTextRendering)
                    {
                        StringFormat format = this.CreateStringFormat();
                        ControlPaint.DrawStringDisabled(e.Graphics, this.Text, this.Font, base.DisabledColor, this.ClientRectWithPadding, format);
                    }
                    else
                    {
                        Color nearestColor;
                        IntPtr hdc = e.Graphics.GetHdc();
                        try
                        {
                            using (WindowsGraphics graphics = WindowsGraphics.FromHdc(hdc))
                            {
                                nearestColor = graphics.GetNearestColor(base.DisabledColor);
                            }
                        }
                        finally
                        {
                            e.Graphics.ReleaseHdc();
                        }
                        Rectangle layoutRectangle = this.ClientRectWithPadding;
                        ControlPaint.DrawStringDisabled(e.Graphics, this.Text, this.Font, nearestColor, layoutRectangle, this.CreateTextFormatFlags(layoutRectangle.Size));
                    }
                }
                finally
                {
                    e.Graphics.Clip = clip;
                }
            }
        Label_045B:
            base.RaisePaintEvent(this, e);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Image image = base.Image;
            if (image != null)
            {
                Region clip = e.Graphics.Clip;
                Rectangle rect = base.CalcImageRenderBounds(image, base.ClientRectangle, base.RtlTranslateAlignment(base.ImageAlign));
                e.Graphics.ExcludeClip(rect);
                try
                {
                    base.OnPaintBackground(e);
                }
                finally
                {
                    e.Graphics.Clip = clip;
                }
                e.Graphics.IntersectClip(rect);
                try
                {
                    base.OnPaintBackground(e);
                    base.DrawImage(e.Graphics, image, base.ClientRectangle, base.RtlTranslateAlignment(base.ImageAlign));
                }
                finally
                {
                    e.Graphics.Clip = clip;
                }
            }
            else
            {
                base.OnPaintBackground(e);
            }
        }

        protected override void OnTextAlignChanged(EventArgs e)
        {
            base.OnTextAlignChanged(e);
            this.InvalidateTextLayout();
            this.UpdateSelectability();
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            this.InvalidateTextLayout();
            this.UpdateSelectability();
        }

        private void PaintLink(Graphics g, Link link, SolidBrush foreBrush, SolidBrush linkBrush, bool optimizeBackgroundRendering, RectangleF finalrect)
        {
            Font hoverLinkFont = this.Font;
            if (link != null)
            {
                if (link.VisualRegion != null)
                {
                    Color empty = Color.Empty;
                    LinkState state = link.State;
                    if ((state & LinkState.Hover) == LinkState.Hover)
                    {
                        hoverLinkFont = this.hoverLinkFont;
                    }
                    else
                    {
                        hoverLinkFont = this.linkFont;
                    }
                    if (link.Enabled)
                    {
                        if ((state & LinkState.Active) == LinkState.Active)
                        {
                            empty = this.ActiveLinkColor;
                        }
                        else if ((state & LinkState.Visited) == LinkState.Visited)
                        {
                            empty = this.VisitedLinkColor;
                        }
                    }
                    else
                    {
                        empty = this.DisabledLinkColor;
                    }
                    if (this.IsOneLink())
                    {
                        g.Clip = new Region(finalrect);
                    }
                    else
                    {
                        g.Clip = link.VisualRegion;
                    }
                    if (optimizeBackgroundRendering)
                    {
                        this.PaintLinkBackground(g);
                    }
                    if (this.UseCompatibleTextRendering)
                    {
                        SolidBrush brush = (empty == Color.Empty) ? linkBrush : new SolidBrush(empty);
                        StringFormat format = this.CreateStringFormat();
                        g.DrawString(this.Text, hoverLinkFont, brush, this.ClientRectWithPadding, format);
                        if (brush != linkBrush)
                        {
                            brush.Dispose();
                        }
                    }
                    else
                    {
                        if (empty == Color.Empty)
                        {
                            empty = linkBrush.Color;
                        }
                        IntPtr hdc = g.GetHdc();
                        try
                        {
                            using (WindowsGraphics graphics = WindowsGraphics.FromHdc(hdc))
                            {
                                empty = graphics.GetNearestColor(empty);
                            }
                        }
                        finally
                        {
                            g.ReleaseHdc();
                        }
                        Rectangle clientRectWithPadding = this.ClientRectWithPadding;
                        TextRenderer.DrawText(g, this.Text, hoverLinkFont, clientRectWithPadding, empty, this.CreateTextFormatFlags(clientRectWithPadding.Size));
                    }
                    if ((this.Focused && this.ShowFocusCues) && (this.FocusLink == link))
                    {
                        RectangleF[] regionScans = link.VisualRegion.GetRegionScans(g.Transform);
                        if ((regionScans != null) && (regionScans.Length > 0))
                        {
                            if (this.IsOneLink())
                            {
                                Rectangle rectangle = Rectangle.Ceiling(finalrect);
                                ControlPaint.DrawFocusRectangle(g, rectangle, this.ForeColor, this.BackColor);
                            }
                            else
                            {
                                foreach (RectangleF ef in regionScans)
                                {
                                    ControlPaint.DrawFocusRectangle(g, Rectangle.Ceiling(ef), this.ForeColor, this.BackColor);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                g.IntersectClip(this.textRegion);
                if (optimizeBackgroundRendering)
                {
                    this.PaintLinkBackground(g);
                }
                if (this.UseCompatibleTextRendering)
                {
                    StringFormat format2 = this.CreateStringFormat();
                    g.DrawString(this.Text, hoverLinkFont, foreBrush, this.ClientRectWithPadding, format2);
                }
                else
                {
                    Color nearestColor;
                    IntPtr hDc = g.GetHdc();
                    try
                    {
                        using (WindowsGraphics graphics2 = WindowsGraphics.FromHdc(hDc))
                        {
                            nearestColor = graphics2.GetNearestColor(foreBrush.Color);
                        }
                    }
                    finally
                    {
                        g.ReleaseHdc();
                    }
                    Rectangle bounds = this.ClientRectWithPadding;
                    TextRenderer.DrawText(g, this.Text, hoverLinkFont, bounds, nearestColor, this.CreateTextFormatFlags(bounds.Size));
                }
            }
        }

        private void PaintLinkBackground(Graphics g)
        {
            using (PaintEventArgs args = new PaintEventArgs(g, base.ClientRectangle))
            {
                base.InvokePaintBackground(this, args);
            }
        }

        protected Link PointInLink(int x, int y)
        {
            Graphics g = base.CreateGraphicsInternal();
            Link link = null;
            try
            {
                this.EnsureRun(g);
                foreach (Link link2 in this.links)
                {
                    if ((link2.VisualRegion != null) && link2.VisualRegion.IsVisible(x, y, g))
                    {
                        return link2;
                    }
                }
                return link;
            }
            finally
            {
                g.Dispose();
                g = null;
            }
            return link;
        }

        [UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows)]
        protected override bool ProcessDialogKey(Keys keyData)
        {
            if ((keyData & (Keys.Alt | Keys.Control)) != Keys.Alt)
            {
                switch ((keyData & Keys.KeyCode))
                {
                    case Keys.Left:
                    case Keys.Up:
                        if (!this.FocusNextLink(false))
                        {
                            break;
                        }
                        return true;

                    case Keys.Right:
                    case Keys.Down:
                        if (!this.FocusNextLink(true))
                        {
                            break;
                        }
                        return true;

                    case Keys.Tab:
                        if (this.TabStop)
                        {
                            bool forward = (keyData & Keys.Shift) != Keys.Shift;
                            if (this.FocusNextLink(forward))
                            {
                                return true;
                            }
                        }
                        break;
                }
            }
            return base.ProcessDialogKey(keyData);
        }

        internal void ResetActiveLinkColor()
        {
            this.activeLinkColor = Color.Empty;
        }

        internal void ResetDisabledLinkColor()
        {
            this.disabledLinkColor = Color.Empty;
        }

        private void ResetLinkArea()
        {
            this.LinkArea = new System.Windows.Forms.LinkArea(0, -1);
        }

        internal void ResetLinkColor()
        {
            this.linkColor = Color.Empty;
            this.InvalidateLink(null);
        }

        private void ResetVisitedLinkColor()
        {
            this.visitedLinkColor = Color.Empty;
        }

        protected override void Select(bool directed, bool forward)
        {
            if (directed && (this.links.Count > 0))
            {
                int focusIndex = -1;
                if (this.FocusLink != null)
                {
                    focusIndex = this.links.IndexOf(this.FocusLink);
                }
                this.FocusLink = null;
                int nextLinkIndex = this.GetNextLinkIndex(focusIndex, forward);
                if (nextLinkIndex == -1)
                {
                    if (forward)
                    {
                        nextLinkIndex = this.GetNextLinkIndex(-1, forward);
                    }
                    else
                    {
                        nextLinkIndex = this.GetNextLinkIndex(this.links.Count, forward);
                    }
                }
                if (nextLinkIndex != -1)
                {
                    this.FocusLink = (Link) this.links[nextLinkIndex];
                }
            }
            base.Select(directed, forward);
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            this.InvalidateTextLayout();
            base.Invalidate();
            base.SetBoundsCore(x, y, width, height, specified);
        }

        internal bool ShouldSerializeActiveLinkColor()
        {
            return !this.activeLinkColor.IsEmpty;
        }

        internal bool ShouldSerializeDisabledLinkColor()
        {
            return !this.disabledLinkColor.IsEmpty;
        }

        private bool ShouldSerializeLinkArea()
        {
            if ((this.links.Count == 1) && (this.Links[0].Start == 0))
            {
                return (this.Links[0].length != -1);
            }
            return true;
        }

        internal bool ShouldSerializeLinkColor()
        {
            return !this.linkColor.IsEmpty;
        }

        private bool ShouldSerializeUseCompatibleTextRendering()
        {
            if (this.CanUseTextRenderer)
            {
                return (this.UseCompatibleTextRendering != Control.UseCompatibleTextRenderingDefault);
            }
            return true;
        }

        private bool ShouldSerializeVisitedLinkColor()
        {
            return !this.visitedLinkColor.IsEmpty;
        }

        void IButtonControl.NotifyDefault(bool value)
        {
        }

        void IButtonControl.PerformClick()
        {
            if ((this.FocusLink == null) && (this.Links.Count > 0))
            {
                string text = this.Text;
                foreach (Link link in this.Links)
                {
                    int start = ConvertToCharIndex(link.Start, text);
                    int num2 = ConvertToCharIndex(link.Start + link.Length, text);
                    if (link.Enabled && this.LinkInText(start, num2 - start))
                    {
                        this.FocusLink = link;
                        break;
                    }
                }
            }
            if (this.FocusLink != null)
            {
                this.OnLinkClicked(new LinkLabelLinkClickedEventArgs(this.FocusLink));
            }
        }

        private void UpdateAccessibilityLink(Link focusLink)
        {
            if (base.IsHandleCreated)
            {
                int childID = -1;
                for (int i = 0; i < this.links.Count; i++)
                {
                    if (this.links[i] == focusLink)
                    {
                        childID = i;
                    }
                }
                base.AccessibilityNotifyClients(AccessibleEvents.Focus, childID);
            }
        }

        private void UpdateSelectability()
        {
            System.Windows.Forms.LinkArea linkArea = this.LinkArea;
            bool flag = false;
            string text = this.Text;
            int start = ConvertToCharIndex(linkArea.Start, text);
            int num2 = ConvertToCharIndex(linkArea.Start + linkArea.Length, text);
            if (this.LinkInText(start, num2 - start))
            {
                flag = true;
            }
            else if (this.FocusLink != null)
            {
                this.FocusLink = null;
            }
            this.OverrideCursor = null;
            this.TabStop = flag;
            base.SetStyle(ControlStyles.Selectable, flag);
        }

        internal override bool UseGDIMeasuring()
        {
            return !this.UseCompatibleTextRendering;
        }

        private void ValidateNoOverlappingLinks()
        {
            for (int i = 0; i < this.links.Count; i++)
            {
                Link link = (Link) this.links[i];
                if (link.Length < 0)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("LinkLabelOverlap"));
                }
                for (int j = i; j < this.links.Count; j++)
                {
                    if (i != j)
                    {
                        Link link2 = (Link) this.links[j];
                        int num3 = Math.Max(link.Start, link2.Start);
                        int num4 = Math.Min((int) (link.Start + link.Length), (int) (link2.Start + link2.Length));
                        if (num3 < num4)
                        {
                            throw new InvalidOperationException(System.Windows.Forms.SR.GetString("LinkLabelOverlap"));
                        }
                    }
                }
            }
        }

        private void WmSetCursor(ref Message m)
        {
            if ((m.WParam == base.InternalHandle) && (System.Windows.Forms.NativeMethods.Util.LOWORD(m.LParam) == 1))
            {
                if (this.OverrideCursor != null)
                {
                    Cursor.CurrentInternal = this.OverrideCursor;
                }
                else
                {
                    Cursor.CurrentInternal = this.Cursor;
                }
            }
            else
            {
                this.DefWndProc(ref m);
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message msg)
        {
            if (msg.Msg == 0x20)
            {
                this.WmSetCursor(ref msg);
            }
            else
            {
                base.WndProc(ref msg);
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("LinkLabelActiveLinkColorDescr")]
        public Color ActiveLinkColor
        {
            get
            {
                if (this.activeLinkColor.IsEmpty)
                {
                    return this.IEActiveLinkColor;
                }
                return this.activeLinkColor;
            }
            set
            {
                if (this.activeLinkColor != value)
                {
                    this.activeLinkColor = value;
                    this.InvalidateLink(null);
                }
            }
        }

        internal override bool CanUseTextRenderer
        {
            get
            {
                StringInfo info = new StringInfo(this.Text);
                if (this.LinkArea.Start != 0)
                {
                    return false;
                }
                if (this.LinkArea.Length != 0)
                {
                    return (this.LinkArea.Length == info.LengthInTextElements);
                }
                return true;
            }
        }

        private Rectangle ClientRectWithPadding
        {
            get
            {
                return LayoutUtils.DeflateRect(base.ClientRectangle, this.Padding);
            }
        }

        [System.Windows.Forms.SRDescription("LinkLabelDisabledLinkColorDescr"), System.Windows.Forms.SRCategory("CatAppearance")]
        public Color DisabledLinkColor
        {
            get
            {
                if (this.disabledLinkColor.IsEmpty)
                {
                    return this.IEDisabledLinkColor;
                }
                return this.disabledLinkColor;
            }
            set
            {
                if (this.disabledLinkColor != value)
                {
                    this.disabledLinkColor = value;
                    this.InvalidateLink(null);
                }
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public System.Windows.Forms.FlatStyle FlatStyle
        {
            get
            {
                return base.FlatStyle;
            }
            set
            {
                base.FlatStyle = value;
            }
        }

        private Link FocusLink
        {
            get
            {
                return this.focusLink;
            }
            set
            {
                if (this.focusLink != value)
                {
                    if (this.focusLink != null)
                    {
                        this.InvalidateLink(this.focusLink);
                    }
                    this.focusLink = value;
                    if (this.focusLink != null)
                    {
                        this.InvalidateLink(this.focusLink);
                        this.UpdateAccessibilityLink(this.focusLink);
                    }
                }
            }
        }

        private Color IEActiveLinkColor
        {
            get
            {
                return LinkUtilities.IEActiveLinkColor;
            }
        }

        private Color IEDisabledLinkColor
        {
            get
            {
                if (iedisabledLinkColor.IsEmpty)
                {
                    iedisabledLinkColor = ControlPaint.Dark(base.DisabledColor);
                }
                return iedisabledLinkColor;
            }
        }

        private Color IELinkColor
        {
            get
            {
                return LinkUtilities.IELinkColor;
            }
        }

        private Color IEVisitedLinkColor
        {
            get
            {
                return LinkUtilities.IEVisitedLinkColor;
            }
        }

        [System.Windows.Forms.SRDescription("LinkLabelLinkAreaDescr"), Editor("System.Windows.Forms.Design.LinkAreaEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), RefreshProperties(RefreshProperties.Repaint), Localizable(true), System.Windows.Forms.SRCategory("CatBehavior")]
        public System.Windows.Forms.LinkArea LinkArea
        {
            get
            {
                if (this.links.Count == 0)
                {
                    return new System.Windows.Forms.LinkArea(0, 0);
                }
                return new System.Windows.Forms.LinkArea(((Link) this.links[0]).Start, ((Link) this.links[0]).Length);
            }
            set
            {
                System.Windows.Forms.LinkArea linkArea = this.LinkArea;
                this.links.Clear();
                if (!value.IsEmpty)
                {
                    if (value.Start < 0)
                    {
                        throw new ArgumentOutOfRangeException("LinkArea", value, System.Windows.Forms.SR.GetString("LinkLabelAreaStart"));
                    }
                    if (value.Length < -1)
                    {
                        throw new ArgumentOutOfRangeException("LinkArea", value, System.Windows.Forms.SR.GetString("LinkLabelAreaLength"));
                    }
                    if ((value.Start != 0) || (value.Length != 0))
                    {
                        this.Links.Add(new Link(this));
                        ((Link) this.links[0]).Start = value.Start;
                        ((Link) this.links[0]).Length = value.Length;
                    }
                }
                this.UpdateSelectability();
                if (!linkArea.Equals(this.LinkArea))
                {
                    this.InvalidateTextLayout();
                    LayoutTransaction.DoLayout(this.ParentInternal, this, PropertyNames.LinkArea);
                    base.AdjustSize();
                    base.Invalidate();
                }
            }
        }

        [DefaultValue(0), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("LinkLabelLinkBehaviorDescr")]
        public System.Windows.Forms.LinkBehavior LinkBehavior
        {
            get
            {
                return this.linkBehavior;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 3))
                {
                    throw new InvalidEnumArgumentException("LinkBehavior", (int) value, typeof(System.Windows.Forms.LinkBehavior));
                }
                if (value != this.linkBehavior)
                {
                    this.linkBehavior = value;
                    this.InvalidateLinkFonts();
                    this.InvalidateLink(null);
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("LinkLabelLinkColorDescr")]
        public Color LinkColor
        {
            get
            {
                if (!this.linkColor.IsEmpty)
                {
                    return this.linkColor;
                }
                if (SystemInformation.HighContrast)
                {
                    return SystemColors.HotTrack;
                }
                return this.IELinkColor;
            }
            set
            {
                if (this.linkColor != value)
                {
                    this.linkColor = value;
                    this.InvalidateLink(null);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public LinkCollection Links
        {
            get
            {
                if (this.linkCollection == null)
                {
                    this.linkCollection = new LinkCollection(this);
                }
                return this.linkCollection;
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(false), System.Windows.Forms.SRDescription("LinkLabelLinkVisitedDescr")]
        public bool LinkVisited
        {
            get
            {
                if (this.links.Count == 0)
                {
                    return false;
                }
                return ((Link) this.links[0]).Visited;
            }
            set
            {
                if (value != this.LinkVisited)
                {
                    if (this.links.Count == 0)
                    {
                        this.Links.Add(new Link(this));
                    }
                    ((Link) this.links[0]).Visited = value;
                }
            }
        }

        protected Cursor OverrideCursor
        {
            get
            {
                return this.overrideCursor;
            }
            set
            {
                if (this.overrideCursor != value)
                {
                    this.overrideCursor = value;
                    if (base.IsHandleCreated)
                    {
                        System.Windows.Forms.NativeMethods.POINT pt = new System.Windows.Forms.NativeMethods.POINT();
                        System.Windows.Forms.NativeMethods.RECT rect = new System.Windows.Forms.NativeMethods.RECT();
                        System.Windows.Forms.UnsafeNativeMethods.GetCursorPos(pt);
                        System.Windows.Forms.UnsafeNativeMethods.GetWindowRect(new HandleRef(this, base.Handle), ref rect);
                        if ((((rect.left <= pt.x) && (pt.x < rect.right)) && ((rect.top <= pt.y) && (pt.y < rect.bottom))) || (System.Windows.Forms.UnsafeNativeMethods.GetCapture() == base.Handle))
                        {
                            base.SendMessage(0x20, base.Handle, 1);
                        }
                    }
                }
            }
        }

        internal override bool OwnerDraw
        {
            get
            {
                return true;
            }
        }

        [RefreshProperties(RefreshProperties.Repaint)]
        public System.Windows.Forms.Padding Padding
        {
            get
            {
                return base.Padding;
            }
            set
            {
                base.Padding = value;
            }
        }

        DialogResult IButtonControl.DialogResult
        {
            get
            {
                return this.dialogResult;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 7))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(DialogResult));
                }
                this.dialogResult = value;
            }
        }

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        public bool TabStop
        {
            get
            {
                return base.TabStop;
            }
            set
            {
                base.TabStop = value;
            }
        }

        [RefreshProperties(RefreshProperties.Repaint)]
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("UseCompatibleTextRenderingDescr"), RefreshProperties(RefreshProperties.Repaint)]
        public bool UseCompatibleTextRendering
        {
            get
            {
                return base.UseCompatibleTextRendering;
            }
            set
            {
                if (base.UseCompatibleTextRendering != value)
                {
                    base.UseCompatibleTextRendering = value;
                    this.InvalidateTextLayout();
                }
            }
        }

        [System.Windows.Forms.SRDescription("LinkLabelVisitedLinkColorDescr"), System.Windows.Forms.SRCategory("CatAppearance")]
        public Color VisitedLinkColor
        {
            get
            {
                if (!this.visitedLinkColor.IsEmpty)
                {
                    return this.visitedLinkColor;
                }
                if (SystemInformation.HighContrast)
                {
                    int red = ((SystemColors.Window.R + SystemColors.WindowText.R) + 1) / 2;
                    int g = SystemColors.WindowText.G;
                    int blue = ((SystemColors.Window.B + SystemColors.WindowText.B) + 1) / 2;
                    return Color.FromArgb(red, g, blue);
                }
                return this.IEVisitedLinkColor;
            }
            set
            {
                if (this.visitedLinkColor != value)
                {
                    this.visitedLinkColor = value;
                    this.InvalidateLink(null);
                }
            }
        }

        [TypeConverter(typeof(LinkConverter))]
        public class Link
        {
            private string description;
            private bool enabled;
            internal int length;
            private object linkData;
            private string name;
            private LinkLabel owner;
            private int start;
            private LinkState state;
            private object userData;
            private Region visualRegion;

            public Link()
            {
                this.enabled = true;
            }

            internal Link(LinkLabel owner)
            {
                this.enabled = true;
                this.owner = owner;
            }

            public Link(int start, int length)
            {
                this.enabled = true;
                this.start = start;
                this.length = length;
            }

            public Link(int start, int length, object linkData)
            {
                this.enabled = true;
                this.start = start;
                this.length = length;
                this.linkData = linkData;
            }

            public string Description
            {
                get
                {
                    return this.description;
                }
                set
                {
                    this.description = value;
                }
            }

            [DefaultValue(true)]
            public bool Enabled
            {
                get
                {
                    return this.enabled;
                }
                set
                {
                    if (this.enabled != value)
                    {
                        this.enabled = value;
                        if ((this.state & (LinkState.Active | LinkState.Hover)) != LinkState.Normal)
                        {
                            this.state &= ~(LinkState.Active | LinkState.Hover);
                            if (this.owner != null)
                            {
                                this.owner.OverrideCursor = null;
                            }
                        }
                        if (this.owner != null)
                        {
                            this.owner.InvalidateLink(this);
                        }
                    }
                }
            }

            public int Length
            {
                get
                {
                    if (this.length != -1)
                    {
                        return this.length;
                    }
                    if ((this.owner != null) && !string.IsNullOrEmpty(this.owner.Text))
                    {
                        StringInfo info = new StringInfo(this.owner.Text);
                        return (info.LengthInTextElements - this.Start);
                    }
                    return 0;
                }
                set
                {
                    if (this.length != value)
                    {
                        this.length = value;
                        if (this.owner != null)
                        {
                            this.owner.InvalidateTextLayout();
                            this.owner.Invalidate();
                        }
                    }
                }
            }

            [DefaultValue((string) null)]
            public object LinkData
            {
                get
                {
                    return this.linkData;
                }
                set
                {
                    this.linkData = value;
                }
            }

            [System.Windows.Forms.SRDescription("TreeNodeNodeNameDescr"), DefaultValue(""), System.Windows.Forms.SRCategory("CatAppearance")]
            public string Name
            {
                get
                {
                    if (this.name != null)
                    {
                        return this.name;
                    }
                    return "";
                }
                set
                {
                    this.name = value;
                }
            }

            internal LinkLabel Owner
            {
                get
                {
                    return this.owner;
                }
                set
                {
                    this.owner = value;
                }
            }

            public int Start
            {
                get
                {
                    return this.start;
                }
                set
                {
                    if (this.start != value)
                    {
                        this.start = value;
                        if (this.owner != null)
                        {
                            this.owner.links.Sort(LinkLabel.linkComparer);
                            this.owner.InvalidateTextLayout();
                            this.owner.Invalidate();
                        }
                    }
                }
            }

            internal LinkState State
            {
                get
                {
                    return this.state;
                }
                set
                {
                    this.state = value;
                }
            }

            [DefaultValue((string) null), TypeConverter(typeof(StringConverter)), System.Windows.Forms.SRDescription("ControlTagDescr"), Localizable(false), Bindable(true), System.Windows.Forms.SRCategory("CatData")]
            public object Tag
            {
                get
                {
                    return this.userData;
                }
                set
                {
                    this.userData = value;
                }
            }

            [DefaultValue(false)]
            public bool Visited
            {
                get
                {
                    return ((this.State & LinkState.Visited) == LinkState.Visited);
                }
                set
                {
                    bool visited = this.Visited;
                    if (value)
                    {
                        this.State |= LinkState.Visited;
                    }
                    else
                    {
                        this.State &= ~LinkState.Visited;
                    }
                    if ((visited != this.Visited) && (this.owner != null))
                    {
                        this.owner.InvalidateLink(this);
                    }
                }
            }

            internal Region VisualRegion
            {
                get
                {
                    return this.visualRegion;
                }
                set
                {
                    this.visualRegion = value;
                }
            }
        }

        [ComVisible(true)]
        internal class LinkAccessibleObject : AccessibleObject
        {
            private LinkLabel.Link link;

            public LinkAccessibleObject(LinkLabel.Link link)
            {
                this.link = link;
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override void DoDefaultAction()
            {
                this.link.Owner.OnLinkClicked(new LinkLabelLinkClickedEventArgs(this.link));
            }

            public override Rectangle Bounds
            {
                get
                {
                    Rectangle rectangle;
                    Region visualRegion = this.link.VisualRegion;
                    Graphics g = null;
                    System.Windows.Forms.IntSecurity.ObjectFromWin32Handle.Assert();
                    try
                    {
                        g = Graphics.FromHwnd(this.link.Owner.Handle);
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                    if (visualRegion == null)
                    {
                        this.link.Owner.EnsureRun(g);
                        visualRegion = this.link.VisualRegion;
                        if (visualRegion == null)
                        {
                            g.Dispose();
                            return Rectangle.Empty;
                        }
                    }
                    try
                    {
                        rectangle = Rectangle.Ceiling(visualRegion.GetBounds(g));
                    }
                    finally
                    {
                        g.Dispose();
                    }
                    return this.link.Owner.RectangleToScreen(rectangle);
                }
            }

            public override string DefaultAction
            {
                get
                {
                    return System.Windows.Forms.SR.GetString("AccessibleActionClick");
                }
            }

            public override string Description
            {
                get
                {
                    return this.link.Description;
                }
            }

            public override string Name
            {
                get
                {
                    string text = this.link.Owner.Text;
                    int startIndex = LinkLabel.ConvertToCharIndex(this.link.Start, text);
                    int num2 = LinkLabel.ConvertToCharIndex(this.link.Start + this.link.Length, text);
                    return text.Substring(startIndex, num2 - startIndex);
                }
                set
                {
                    base.Name = value;
                }
            }

            public override AccessibleObject Parent
            {
                [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
                get
                {
                    return this.link.Owner.AccessibilityObject;
                }
            }

            public override AccessibleRole Role
            {
                get
                {
                    return AccessibleRole.Link;
                }
            }

            public override AccessibleStates State
            {
                get
                {
                    AccessibleStates focusable = AccessibleStates.Focusable;
                    if (this.link.Owner.FocusLink == this.link)
                    {
                        focusable |= AccessibleStates.Focused;
                    }
                    return focusable;
                }
            }

            public override string Value
            {
                [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
                get
                {
                    return this.Name;
                }
            }
        }

        public class LinkCollection : IList, ICollection, IEnumerable
        {
            private int lastAccessedIndex = -1;
            private bool linksAdded;
            private LinkLabel owner;

            public LinkCollection(LinkLabel owner)
            {
                if (owner == null)
                {
                    throw new ArgumentNullException("owner");
                }
                this.owner = owner;
            }

            public int Add(LinkLabel.Link value)
            {
                if ((value != null) && (value.Length != 0))
                {
                    this.linksAdded = true;
                }
                if (((this.owner.links.Count == 1) && (this[0].Start == 0)) && (this[0].length == -1))
                {
                    this.owner.links.Clear();
                    this.owner.FocusLink = null;
                }
                value.Owner = this.owner;
                this.owner.links.Add(value);
                if (this.owner.AutoSize)
                {
                    LayoutTransaction.DoLayout(this.owner.ParentInternal, this.owner, PropertyNames.Links);
                    this.owner.AdjustSize();
                    this.owner.Invalidate();
                }
                if (this.owner.Links.Count > 1)
                {
                    this.owner.links.Sort(LinkLabel.linkComparer);
                }
                this.owner.ValidateNoOverlappingLinks();
                this.owner.UpdateSelectability();
                this.owner.InvalidateTextLayout();
                this.owner.Invalidate();
                if (this.owner.Links.Count > 1)
                {
                    return this.IndexOf(value);
                }
                return 0;
            }

            public LinkLabel.Link Add(int start, int length)
            {
                if (length != 0)
                {
                    this.linksAdded = true;
                }
                return this.Add(start, length, null);
            }

            public LinkLabel.Link Add(int start, int length, object linkData)
            {
                if (length != 0)
                {
                    this.linksAdded = true;
                }
                if (((this.owner.links.Count == 1) && (this[0].Start == 0)) && (this[0].length == -1))
                {
                    this.owner.links.Clear();
                    this.owner.FocusLink = null;
                }
                LinkLabel.Link link = new LinkLabel.Link(this.owner) {
                    Start = start,
                    Length = length,
                    LinkData = linkData
                };
                this.Add(link);
                return link;
            }

            public virtual void Clear()
            {
                bool flag = (this.owner.links.Count > 0) && this.owner.AutoSize;
                this.owner.links.Clear();
                if (flag)
                {
                    LayoutTransaction.DoLayout(this.owner.ParentInternal, this.owner, PropertyNames.Links);
                    this.owner.AdjustSize();
                    this.owner.Invalidate();
                }
                this.owner.UpdateSelectability();
                this.owner.InvalidateTextLayout();
                this.owner.Invalidate();
            }

            public bool Contains(LinkLabel.Link link)
            {
                return this.owner.links.Contains(link);
            }

            public virtual bool ContainsKey(string key)
            {
                return this.IsValidIndex(this.IndexOfKey(key));
            }

            public IEnumerator GetEnumerator()
            {
                if (this.owner.links != null)
                {
                    return this.owner.links.GetEnumerator();
                }
                return new LinkLabel.Link[0].GetEnumerator();
            }

            public int IndexOf(LinkLabel.Link link)
            {
                return this.owner.links.IndexOf(link);
            }

            public virtual int IndexOfKey(string key)
            {
                if (!string.IsNullOrEmpty(key))
                {
                    if (this.IsValidIndex(this.lastAccessedIndex) && WindowsFormsUtils.SafeCompareStrings(this[this.lastAccessedIndex].Name, key, true))
                    {
                        return this.lastAccessedIndex;
                    }
                    for (int i = 0; i < this.Count; i++)
                    {
                        if (WindowsFormsUtils.SafeCompareStrings(this[i].Name, key, true))
                        {
                            this.lastAccessedIndex = i;
                            return i;
                        }
                    }
                    this.lastAccessedIndex = -1;
                }
                return -1;
            }

            private bool IsValidIndex(int index)
            {
                return ((index >= 0) && (index < this.Count));
            }

            public void Remove(LinkLabel.Link value)
            {
                if (value.Owner == this.owner)
                {
                    this.owner.links.Remove(value);
                    if (this.owner.AutoSize)
                    {
                        LayoutTransaction.DoLayout(this.owner.ParentInternal, this.owner, PropertyNames.Links);
                        this.owner.AdjustSize();
                        this.owner.Invalidate();
                    }
                    this.owner.links.Sort(LinkLabel.linkComparer);
                    this.owner.ValidateNoOverlappingLinks();
                    this.owner.UpdateSelectability();
                    this.owner.InvalidateTextLayout();
                    this.owner.Invalidate();
                    if ((this.owner.FocusLink == null) && (this.owner.links.Count > 0))
                    {
                        this.owner.FocusLink = (LinkLabel.Link) this.owner.links[0];
                    }
                }
            }

            public void RemoveAt(int index)
            {
                this.Remove(this[index]);
            }

            public virtual void RemoveByKey(string key)
            {
                int index = this.IndexOfKey(key);
                if (this.IsValidIndex(index))
                {
                    this.RemoveAt(index);
                }
            }

            void ICollection.CopyTo(Array dest, int index)
            {
                this.owner.links.CopyTo(dest, index);
            }

            int IList.Add(object value)
            {
                if (!(value is LinkLabel.Link))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("LinkLabelBadLink"), "value");
                }
                return this.Add((LinkLabel.Link) value);
            }

            bool IList.Contains(object link)
            {
                return ((link is LinkLabel.Link) && this.Contains((LinkLabel.Link) link));
            }

            int IList.IndexOf(object link)
            {
                if (link is LinkLabel.Link)
                {
                    return this.IndexOf((LinkLabel.Link) link);
                }
                return -1;
            }

            void IList.Insert(int index, object value)
            {
                if (!(value is LinkLabel.Link))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("LinkLabelBadLink"), "value");
                }
                this.Add((LinkLabel.Link) value);
            }

            void IList.Remove(object value)
            {
                if (value is LinkLabel.Link)
                {
                    this.Remove((LinkLabel.Link) value);
                }
            }

            [Browsable(false)]
            public int Count
            {
                get
                {
                    return this.owner.links.Count;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            public virtual LinkLabel.Link this[int index]
            {
                get
                {
                    return (LinkLabel.Link) this.owner.links[index];
                }
                set
                {
                    this.owner.links[index] = value;
                    this.owner.links.Sort(LinkLabel.linkComparer);
                    this.owner.InvalidateTextLayout();
                    this.owner.Invalidate();
                }
            }

            public virtual LinkLabel.Link this[string key]
            {
                get
                {
                    if (!string.IsNullOrEmpty(key))
                    {
                        int index = this.IndexOfKey(key);
                        if (this.IsValidIndex(index))
                        {
                            return this[index];
                        }
                    }
                    return null;
                }
            }

            public bool LinksAdded
            {
                get
                {
                    return this.linksAdded;
                }
            }

            bool ICollection.IsSynchronized
            {
                get
                {
                    return false;
                }
            }

            object ICollection.SyncRoot
            {
                get
                {
                    return this;
                }
            }

            bool IList.IsFixedSize
            {
                get
                {
                    return false;
                }
            }

            object IList.this[int index]
            {
                get
                {
                    return this[index];
                }
                set
                {
                    if (!(value is LinkLabel.Link))
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("LinkLabelBadLink"), "value");
                    }
                    this[index] = (LinkLabel.Link) value;
                }
            }
        }

        private class LinkComparer : IComparer
        {
            int IComparer.Compare(object link1, object link2)
            {
                int start = ((LinkLabel.Link) link1).Start;
                int num2 = ((LinkLabel.Link) link2).Start;
                return (start - num2);
            }
        }

        [ComVisible(true)]
        internal class LinkLabelAccessibleObject : Label.LabelAccessibleObject
        {
            public LinkLabelAccessibleObject(LinkLabel owner) : base(owner)
            {
            }

            public override AccessibleObject GetChild(int index)
            {
                if ((index >= 0) && (index < ((LinkLabel) base.Owner).Links.Count))
                {
                    return new LinkLabel.LinkAccessibleObject(((LinkLabel) base.Owner).Links[index]);
                }
                return null;
            }

            public override int GetChildCount()
            {
                return ((LinkLabel) base.Owner).Links.Count;
            }

            public override AccessibleObject HitTest(int x, int y)
            {
                Point point = base.Owner.PointToClient(new Point(x, y));
                LinkLabel.Link link = ((LinkLabel) base.Owner).PointInLink(point.X, point.Y);
                if (link != null)
                {
                    return new LinkLabel.LinkAccessibleObject(link);
                }
                if (this.Bounds.Contains(x, y))
                {
                    return this;
                }
                return null;
            }
        }
    }
}

