namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;
    using System.Windows.Forms.Layout;

    [ClassInterface(ClassInterfaceType.AutoDispatch), ComVisible(true), Designer("System.Windows.Forms.Design.TextBoxBaseDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultEvent("TextChanged"), DefaultBindingProperty("Text")]
    public abstract class TextBoxBase : Control
    {
        private static readonly int acceptsTab = BitVector32.CreateMask(readOnly);
        private static readonly int autoSize = BitVector32.CreateMask();
        private System.Windows.Forms.BorderStyle borderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        private static readonly int codeUpdateText = BitVector32.CreateMask(creatingHandle);
        private static readonly int creatingHandle = BitVector32.CreateMask(wordWrap);
        private bool doubleClickFired;
        private static readonly object EVENT_ACCEPTSTABCHANGED = new object();
        private static readonly object EVENT_BORDERSTYLECHANGED = new object();
        private static readonly object EVENT_HIDESELECTIONCHANGED = new object();
        private static readonly object EVENT_MODIFIEDCHANGED = new object();
        private static readonly object EVENT_MULTILINECHANGED = new object();
        private static readonly object EVENT_READONLYCHANGED = new object();
        private static readonly int hideSelection = BitVector32.CreateMask(autoSize);
        private bool integralHeightAdjust;
        private int maxLength = 0x7fff;
        private static readonly int modified = BitVector32.CreateMask(multiline);
        private static readonly int multiline = BitVector32.CreateMask(hideSelection);
        private static readonly int readOnly = BitVector32.CreateMask(modified);
        private int requestedHeight;
        private static readonly int scrollToCaretOnHandleCreated = BitVector32.CreateMask(shortcutsEnabled);
        private int selectionLength;
        private int selectionStart;
        private static readonly int setSelectionOnHandleCreated = BitVector32.CreateMask(scrollToCaretOnHandleCreated);
        private static readonly int shortcutsEnabled = BitVector32.CreateMask(codeUpdateText);
        private static int[] shortcutsToDisable;
        private BitVector32 textBoxFlags = new BitVector32();
        private static readonly int wordWrap = BitVector32.CreateMask(acceptsTab);

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("TextBoxBaseOnAcceptsTabChangedDescr")]
        public event EventHandler AcceptsTabChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_ACCEPTSTABCHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_ACCEPTSTABCHANGED, value);
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler AutoSizeChanged
        {
            add
            {
                base.AutoSizeChanged += value;
            }
            remove
            {
                base.AutoSizeChanged -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler BackgroundImageChanged
        {
            add
            {
                base.BackgroundImageChanged += value;
            }
            remove
            {
                base.BackgroundImageChanged -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler BackgroundImageLayoutChanged
        {
            add
            {
                base.BackgroundImageLayoutChanged += value;
            }
            remove
            {
                base.BackgroundImageLayoutChanged -= value;
            }
        }

        [System.Windows.Forms.SRDescription("TextBoxBaseOnBorderStyleChangedDescr"), System.Windows.Forms.SRCategory("CatPropertyChanged")]
        public event EventHandler BorderStyleChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_BORDERSTYLECHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_BORDERSTYLECHANGED, value);
            }
        }

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        public event EventHandler Click
        {
            add
            {
                base.Click += value;
            }
            remove
            {
                base.Click -= value;
            }
        }

        [System.Windows.Forms.SRDescription("TextBoxBaseOnHideSelectionChangedDescr"), System.Windows.Forms.SRCategory("CatPropertyChanged")]
        public event EventHandler HideSelectionChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_HIDESELECTIONCHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_HIDESELECTIONCHANGED, value);
            }
        }

        [System.Windows.Forms.SRDescription("TextBoxBaseOnModifiedChangedDescr"), System.Windows.Forms.SRCategory("CatPropertyChanged")]
        public event EventHandler ModifiedChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_MODIFIEDCHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_MODIFIEDCHANGED, value);
            }
        }

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        public event MouseEventHandler MouseClick
        {
            add
            {
                base.MouseClick += value;
            }
            remove
            {
                base.MouseClick -= value;
            }
        }

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("TextBoxBaseOnMultilineChangedDescr")]
        public event EventHandler MultilineChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_MULTILINECHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_MULTILINECHANGED, value);
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRCategory("CatLayout"), System.Windows.Forms.SRDescription("ControlOnPaddingChangedDescr")]
        public event EventHandler PaddingChanged
        {
            add
            {
                base.PaddingChanged += value;
            }
            remove
            {
                base.PaddingChanged -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event PaintEventHandler Paint
        {
            add
            {
                base.Paint += value;
            }
            remove
            {
                base.Paint -= value;
            }
        }

        [System.Windows.Forms.SRDescription("TextBoxBaseOnReadOnlyChangedDescr"), System.Windows.Forms.SRCategory("CatPropertyChanged")]
        public event EventHandler ReadOnlyChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_READONLYCHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_READONLYCHANGED, value);
            }
        }

        internal TextBoxBase()
        {
            base.SetState2(0x800, true);
            this.textBoxFlags[((autoSize | hideSelection) | wordWrap) | shortcutsEnabled] = true;
            base.SetStyle(ControlStyles.FixedHeight, this.textBoxFlags[autoSize]);
            base.SetStyle(ControlStyles.UseTextForAccessibility | ControlStyles.StandardDoubleClick | ControlStyles.StandardClick | ControlStyles.UserPaint, false);
            this.requestedHeight = base.Height;
        }

        private void AdjustHeight(bool returnIfAnchored)
        {
            if (!returnIfAnchored || ((this.Anchor & (AnchorStyles.Bottom | AnchorStyles.Top)) != (AnchorStyles.Bottom | AnchorStyles.Top)))
            {
                int requestedHeight = this.requestedHeight;
                try
                {
                    if (this.textBoxFlags[autoSize] && !this.textBoxFlags[multiline])
                    {
                        base.Height = this.PreferredHeight;
                    }
                    else
                    {
                        int height = base.Height;
                        if (this.textBoxFlags[multiline])
                        {
                            base.Height = Math.Max(requestedHeight, this.PreferredHeight + 2);
                        }
                        this.integralHeightAdjust = true;
                        try
                        {
                            base.Height = requestedHeight;
                        }
                        finally
                        {
                            this.integralHeightAdjust = false;
                        }
                    }
                }
                finally
                {
                    this.requestedHeight = requestedHeight;
                }
            }
        }

        internal void AdjustSelectionStartAndEnd(int selStart, int selLength, out int start, out int end, int textLen)
        {
            start = selStart;
            end = 0;
            if (start <= -1)
            {
                start = -1;
            }
            else
            {
                int textLength;
                if (textLen >= 0)
                {
                    textLength = textLen;
                }
                else
                {
                    textLength = this.TextLength;
                }
                if (start > textLength)
                {
                    start = textLength;
                }
                try
                {
                    end = start + selLength;
                }
                catch (OverflowException)
                {
                    end = (start > 0) ? 0x7fffffff : -2147483648;
                }
                if (end < 0)
                {
                    end = 0;
                }
                else if (end > textLength)
                {
                    end = textLength;
                }
                if (this.SelectionUsesDbcsOffsetsInWin9x && (Marshal.SystemDefaultCharSize == 1))
                {
                    ToDbcsOffsets(this.WindowText, ref start, ref end);
                }
            }
        }

        public void AppendText(string text)
        {
            if (text.Length > 0)
            {
                int num;
                int num2;
                this.GetSelectionStartAndLength(out num, out num2);
                try
                {
                    int endPosition = this.GetEndPosition();
                    this.SelectInternal(endPosition, endPosition, endPosition);
                    this.SelectedText = text;
                }
                finally
                {
                    if ((base.Width == 0) || (base.Height == 0))
                    {
                        this.Select(num, num2);
                    }
                }
            }
        }

        public void Clear()
        {
            this.Text = null;
        }

        public void ClearUndo()
        {
            if (base.IsHandleCreated)
            {
                base.SendMessage(0xcd, 0, 0);
            }
        }

        [UIPermission(SecurityAction.Demand, Clipboard=UIPermissionClipboard.OwnClipboard)]
        public void Copy()
        {
            base.SendMessage(0x301, 0, 0);
        }

        protected override void CreateHandle()
        {
            this.textBoxFlags[creatingHandle] = true;
            try
            {
                base.CreateHandle();
                if (this.SetSelectionInCreateHandle)
                {
                    this.SetSelectionOnHandle();
                }
            }
            finally
            {
                this.textBoxFlags[creatingHandle] = false;
            }
        }

        public void Cut()
        {
            base.SendMessage(0x300, 0, 0);
        }

        public void DeselectAll()
        {
            this.SelectionLength = 0;
        }

        internal void ForceWindowText(string value)
        {
            if (value == null)
            {
                value = "";
            }
            this.textBoxFlags[codeUpdateText] = true;
            try
            {
                if (base.IsHandleCreated)
                {
                    System.Windows.Forms.UnsafeNativeMethods.SetWindowText(new HandleRef(this, base.Handle), value);
                }
                else if (value.Length == 0)
                {
                    this.Text = null;
                }
                else
                {
                    this.Text = value;
                }
            }
            finally
            {
                this.textBoxFlags[codeUpdateText] = false;
            }
        }

        public virtual char GetCharFromPosition(Point pt)
        {
            string text = this.Text;
            int charIndexFromPosition = this.GetCharIndexFromPosition(pt);
            if ((charIndexFromPosition >= 0) && (charIndexFromPosition < text.Length))
            {
                return text[charIndexFromPosition];
            }
            return '\0';
        }

        public virtual int GetCharIndexFromPosition(Point pt)
        {
            int lParam = System.Windows.Forms.NativeMethods.Util.MAKELONG(pt.X, pt.Y);
            int n = (int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0xd7, 0, lParam);
            n = System.Windows.Forms.NativeMethods.Util.LOWORD(n);
            if (n < 0)
            {
                return 0;
            }
            string text = this.Text;
            if (n >= text.Length)
            {
                n = Math.Max(text.Length - 1, 0);
            }
            return n;
        }

        internal virtual int GetEndPosition()
        {
            if (!base.IsHandleCreated)
            {
                return this.TextLength;
            }
            return (this.TextLength + 1);
        }

        public int GetFirstCharIndexFromLine(int lineNumber)
        {
            if (lineNumber < 0)
            {
                throw new ArgumentOutOfRangeException("lineNumber", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "lineNumber", lineNumber.ToString(CultureInfo.CurrentCulture) }));
            }
            return (int) ((long) base.SendMessage(0xbb, lineNumber, 0));
        }

        public int GetFirstCharIndexOfCurrentLine()
        {
            return (int) ((long) base.SendMessage(0xbb, -1, 0));
        }

        public virtual int GetLineFromCharIndex(int index)
        {
            return (int) ((long) base.SendMessage(0xc9, index, 0));
        }

        public virtual Point GetPositionFromCharIndex(int index)
        {
            if ((index < 0) || (index >= this.Text.Length))
            {
                return Point.Empty;
            }
            int n = (int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0xd6, index, 0);
            return new Point(System.Windows.Forms.NativeMethods.Util.LOWORD(n), System.Windows.Forms.NativeMethods.Util.HIWORD(n));
        }

        internal override Size GetPreferredSizeCore(Size proposedConstraints)
        {
            Size size = this.SizeFromClientSize(Size.Empty) + this.Padding.Size;
            if (this.BorderStyle != System.Windows.Forms.BorderStyle.None)
            {
                size += new Size(0, 3);
            }
            if (this.BorderStyle == System.Windows.Forms.BorderStyle.FixedSingle)
            {
                size.Width += 2;
                size.Height += 2;
            }
            proposedConstraints -= size;
            TextFormatFlags noPrefix = TextFormatFlags.NoPrefix;
            if (!this.Multiline)
            {
                noPrefix |= TextFormatFlags.SingleLine;
            }
            else if (this.WordWrap)
            {
                noPrefix |= TextFormatFlags.WordBreak;
            }
            Size size2 = TextRenderer.MeasureText(this.Text, this.Font, proposedConstraints, noPrefix);
            size2.Height = Math.Max(size2.Height, base.FontHeight);
            return (size2 + size);
        }

        internal void GetSelectionStartAndLength(out int start, out int length)
        {
            int end = 0;
            if (!base.IsHandleCreated)
            {
                this.AdjustSelectionStartAndEnd(this.selectionStart, this.selectionLength, out start, out end, -1);
                length = end - start;
            }
            else
            {
                start = 0;
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0xb0, ref start, ref end);
                start = Math.Max(0, start);
                end = Math.Max(0, end);
                if (this.SelectionUsesDbcsOffsetsInWin9x && (Marshal.SystemDefaultCharSize == 1))
                {
                    ToUnicodeOffsets(this.WindowText, ref start, ref end);
                }
                length = end - start;
            }
        }

        internal override IntPtr InitializeDCForWmCtlColor(IntPtr dc, int msg)
        {
            if ((msg == 0x138) && !this.ShouldSerializeBackColor())
            {
                return IntPtr.Zero;
            }
            return base.InitializeDCForWmCtlColor(dc, msg);
        }

        protected override bool IsInputKey(Keys keyData)
        {
            if ((keyData & Keys.Alt) != Keys.Alt)
            {
                switch ((keyData & Keys.KeyCode))
                {
                    case Keys.Back:
                        if (this.ReadOnly)
                        {
                            break;
                        }
                        return true;

                    case Keys.Tab:
                        if (!this.Multiline || !this.textBoxFlags[acceptsTab])
                        {
                            return false;
                        }
                        return ((keyData & Keys.Control) == Keys.None);

                    case Keys.Escape:
                        if (this.Multiline)
                        {
                            return false;
                        }
                        break;

                    case Keys.PageUp:
                    case Keys.Next:
                    case Keys.End:
                    case Keys.Home:
                        return true;
                }
            }
            return base.IsInputKey(keyData);
        }

        protected virtual void OnAcceptsTabChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EVENT_ACCEPTSTABCHANGED] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnBorderStyleChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EVENT_BORDERSTYLECHANGED] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            this.AdjustHeight(false);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            CommonProperties.xClearPreferredSizeCache(this);
            this.AdjustHeight(true);
            this.UpdateMaxLength();
            if (this.textBoxFlags[modified])
            {
                base.SendMessage(0xb9, 1, 0);
            }
            if (this.textBoxFlags[scrollToCaretOnHandleCreated])
            {
                this.ScrollToCaret();
                this.textBoxFlags[scrollToCaretOnHandleCreated] = false;
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            this.textBoxFlags[modified] = this.Modified;
            this.textBoxFlags[setSelectionOnHandleCreated] = true;
            this.GetSelectionStartAndLength(out this.selectionStart, out this.selectionLength);
            base.OnHandleDestroyed(e);
        }

        protected virtual void OnHideSelectionChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EVENT_HIDESELECTIONCHANGED] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnModifiedChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EVENT_MODIFIEDCHANGED] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            Point point = base.PointToScreen(mevent.Location);
            if (mevent.Button == MouseButtons.Left)
            {
                if (!base.ValidationCancelled && (System.Windows.Forms.UnsafeNativeMethods.WindowFromPoint(point.X, point.Y) == base.Handle))
                {
                    if (!this.doubleClickFired)
                    {
                        this.OnClick(mevent);
                        this.OnMouseClick(mevent);
                    }
                    else
                    {
                        this.doubleClickFired = false;
                        this.OnDoubleClick(mevent);
                        this.OnMouseDoubleClick(mevent);
                    }
                }
                this.doubleClickFired = false;
            }
            base.OnMouseUp(mevent);
        }

        protected virtual void OnMultilineChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EVENT_MULTILINECHANGED] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnPaddingChanged(EventArgs e)
        {
            base.OnPaddingChanged(e);
            this.AdjustHeight(false);
        }

        protected virtual void OnReadOnlyChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EVENT_READONLYCHANGED] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            CommonProperties.xClearPreferredSizeCache(this);
            base.OnTextChanged(e);
        }

        [UIPermission(SecurityAction.Demand, Clipboard=UIPermissionClipboard.OwnClipboard)]
        public void Paste()
        {
            System.Windows.Forms.IntSecurity.ClipboardRead.Demand();
            base.SendMessage(770, 0, 0);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            bool flag = base.ProcessCmdKey(ref msg, keyData);
            if (!this.ShortcutsEnabled)
            {
                foreach (int num in shortcutsToDisable)
                {
                    if ((keyData == num) || (keyData == (num | 0x10000)))
                    {
                        return true;
                    }
                }
            }
            if (!this.textBoxFlags[readOnly])
            {
                return flag;
            }
            int num2 = (int) keyData;
            if (((num2 != 0x2004c) && (num2 != 0x20052)) && ((num2 != 0x20045) && (num2 != 0x2004a)))
            {
                return flag;
            }
            return true;
        }

        [UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows)]
        protected override bool ProcessDialogKey(Keys keyData)
        {
            Keys keys = keyData & Keys.KeyCode;
            if (((keys == Keys.Tab) && this.AcceptsTab) && ((keyData & Keys.Control) != Keys.None))
            {
                keyData &= ~Keys.Control;
            }
            return base.ProcessDialogKey(keyData);
        }

        public void ScrollToCaret()
        {
            if (base.IsHandleCreated)
            {
                if (!string.IsNullOrEmpty(this.WindowText))
                {
                    bool flag = false;
                    object editOle = null;
                    IntPtr zero = IntPtr.Zero;
                    try
                    {
                        if (System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x43c, 0, out editOle) != 0)
                        {
                            zero = Marshal.GetIUnknownForObject(editOle);
                            if (zero != IntPtr.Zero)
                            {
                                IntPtr ppv = IntPtr.Zero;
                                Guid gUID = typeof(System.Windows.Forms.UnsafeNativeMethods.ITextDocument).GUID;
                                try
                                {
                                    Marshal.QueryInterface(zero, ref gUID, out ppv);
                                    System.Windows.Forms.UnsafeNativeMethods.ITextDocument objectForIUnknown = Marshal.GetObjectForIUnknown(ppv) as System.Windows.Forms.UnsafeNativeMethods.ITextDocument;
                                    if (objectForIUnknown != null)
                                    {
                                        int num;
                                        int num2;
                                        this.GetSelectionStartAndLength(out num, out num2);
                                        int lineFromCharIndex = this.GetLineFromCharIndex(num);
                                        objectForIUnknown.Range(this.WindowText.Length - 1, this.WindowText.Length - 1).ScrollIntoView(0);
                                        int num4 = (int) ((long) base.SendMessage(0xce, 0, 0));
                                        if (num4 > lineFromCharIndex)
                                        {
                                            objectForIUnknown.Range(num, num + num2).ScrollIntoView(0x20);
                                        }
                                        flag = true;
                                    }
                                }
                                finally
                                {
                                    if (ppv != IntPtr.Zero)
                                    {
                                        Marshal.Release(ppv);
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        if (zero != IntPtr.Zero)
                        {
                            Marshal.Release(zero);
                        }
                    }
                    if (!flag)
                    {
                        base.SendMessage(0xb7, 0, 0);
                    }
                }
            }
            else
            {
                this.textBoxFlags[scrollToCaretOnHandleCreated] = true;
            }
        }

        public void Select(int start, int length)
        {
            if (start < 0)
            {
                throw new ArgumentOutOfRangeException("start", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "start", start.ToString(CultureInfo.CurrentCulture) }));
            }
            int textLength = this.TextLength;
            if (start > textLength)
            {
                long num2 = Math.Min(0L, (long) ((length + start) - textLength));
                if (num2 < -2147483648L)
                {
                    length = -2147483648;
                }
                else
                {
                    length = (int) num2;
                }
                start = textLength;
            }
            this.SelectInternal(start, length, textLength);
        }

        public void SelectAll()
        {
            int textLength = this.TextLength;
            this.SelectInternal(0, textLength, textLength);
        }

        internal virtual void SelectInternal(int start, int length, int textLen)
        {
            if (base.IsHandleCreated)
            {
                int num;
                int num2;
                this.AdjustSelectionStartAndEnd(start, length, out num, out num2, textLen);
                base.SendMessage(0xb1, num, num2);
            }
            else
            {
                this.selectionStart = start;
                this.selectionLength = length;
                this.textBoxFlags[setSelectionOnHandleCreated] = true;
            }
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            if (!this.integralHeightAdjust && (height != base.Height))
            {
                this.requestedHeight = height;
            }
            if (this.textBoxFlags[autoSize] && !this.textBoxFlags[multiline])
            {
                height = this.PreferredHeight;
            }
            base.SetBoundsCore(x, y, width, height, specified);
        }

        internal virtual void SetSelectedTextInternal(string text, bool clearUndo)
        {
            if (!base.IsHandleCreated)
            {
                this.CreateHandle();
            }
            if (text == null)
            {
                text = "";
            }
            base.SendMessage(0xc5, 0, 0);
            if (clearUndo)
            {
                base.SendMessage(0xc2, 0, text);
                base.SendMessage(0xb9, 0, 0);
                this.ClearUndo();
            }
            else
            {
                base.SendMessage(0xc2, -1, text);
            }
            base.SendMessage(0xc5, this.maxLength, 0);
        }

        internal void SetSelectionOnHandle()
        {
            if (this.textBoxFlags[setSelectionOnHandleCreated])
            {
                int num;
                int num2;
                this.textBoxFlags[setSelectionOnHandleCreated] = false;
                this.AdjustSelectionStartAndEnd(this.selectionStart, this.selectionLength, out num, out num2, -1);
                base.SendMessage(0xb1, num, num2);
            }
        }

        private static void Swap(ref int n1, ref int n2)
        {
            int num = n2;
            n2 = n1;
            n1 = num;
        }

        internal static void ToDbcsOffsets(string str, ref int start, ref int end)
        {
            Encoding encoding = Encoding.Default;
            bool flag = start > end;
            if (flag)
            {
                Swap(ref start, ref end);
            }
            if (start < 0)
            {
                start = 0;
            }
            if (start > str.Length)
            {
                start = str.Length;
            }
            if (end < start)
            {
                end = start;
            }
            if (end > str.Length)
            {
                end = str.Length;
            }
            int num = (start == 0) ? 0 : encoding.GetByteCount(str.Substring(0, start));
            end = num + encoding.GetByteCount(str.Substring(start, end - start));
            start = num;
            if (flag)
            {
                Swap(ref start, ref end);
            }
        }

        public override string ToString()
        {
            string str = base.ToString();
            string text = this.Text;
            if (text.Length > 40)
            {
                text = text.Substring(0, 40) + "...";
            }
            return (str + ", Text: " + text.ToString());
        }

        private static void ToUnicodeOffsets(string str, ref int start, ref int end)
        {
            Encoding encoding = Encoding.Default;
            byte[] bytes = encoding.GetBytes(str);
            bool flag = start > end;
            if (flag)
            {
                Swap(ref start, ref end);
            }
            if (start < 0)
            {
                start = 0;
            }
            if (start > bytes.Length)
            {
                start = bytes.Length;
            }
            if (end > bytes.Length)
            {
                end = bytes.Length;
            }
            int num = (start == 0) ? 0 : encoding.GetCharCount(bytes, 0, start);
            end = num + encoding.GetCharCount(bytes, start, end - start);
            start = num;
            if (flag)
            {
                Swap(ref start, ref end);
            }
        }

        public void Undo()
        {
            base.SendMessage(0xc7, 0, 0);
        }

        internal virtual void UpdateMaxLength()
        {
            if (base.IsHandleCreated)
            {
                base.SendMessage(0xc5, this.maxLength, 0);
            }
        }

        private void WmGetDlgCode(ref Message m)
        {
            base.WndProc(ref m);
            if (this.AcceptsTab)
            {
                m.Result = (IntPtr) (((int) ((long) m.Result)) | 2);
            }
            else
            {
                m.Result = (IntPtr) (((int) ((long) m.Result)) & -7);
            }
        }

        private void WmReflectCommand(ref Message m)
        {
            if (!this.textBoxFlags[codeUpdateText] && !this.textBoxFlags[creatingHandle])
            {
                if ((System.Windows.Forms.NativeMethods.Util.HIWORD(m.WParam) == 0x300) && this.CanRaiseTextChangedEvent)
                {
                    this.OnTextChanged(EventArgs.Empty);
                }
                else if (System.Windows.Forms.NativeMethods.Util.HIWORD(m.WParam) == 0x400)
                {
                    bool modified = this.Modified;
                }
            }
        }

        private void WmSetFont(ref Message m)
        {
            base.WndProc(ref m);
            if (!this.textBoxFlags[multiline])
            {
                base.SendMessage(0xd3, 3, 0);
            }
        }

        private void WmTextBoxContextMenu(ref Message m)
        {
            if ((this.ContextMenu != null) || (this.ContextMenuStrip != null))
            {
                Point point;
                int x = System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam);
                int y = System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam);
                bool isKeyboardActivated = false;
                if (((int) ((long) m.LParam)) == -1)
                {
                    isKeyboardActivated = true;
                    point = new Point(base.Width / 2, base.Height / 2);
                }
                else
                {
                    point = base.PointToClientInternal(new Point(x, y));
                }
                if (base.ClientRectangle.Contains(point))
                {
                    if (this.ContextMenu != null)
                    {
                        this.ContextMenu.Show(this, point);
                    }
                    else if (this.ContextMenuStrip != null)
                    {
                        this.ContextMenuStrip.ShowInternal(this, point, isKeyboardActivated);
                    }
                    else
                    {
                        this.DefWndProc(ref m);
                    }
                }
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x87:
                    this.WmGetDlgCode(ref m);
                    return;

                case 0x203:
                    this.doubleClickFired = true;
                    base.WndProc(ref m);
                    return;

                case 0x2111:
                    this.WmReflectCommand(ref m);
                    return;

                case 0x30:
                    this.WmSetFont(ref m);
                    return;

                case 0x7b:
                    if (this.ShortcutsEnabled)
                    {
                        base.WndProc(ref m);
                        return;
                    }
                    this.WmTextBoxContextMenu(ref m);
                    return;
            }
            base.WndProc(ref m);
        }

        [System.Windows.Forms.SRDescription("TextBoxAcceptsTabDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(false)]
        public bool AcceptsTab
        {
            get
            {
                return this.textBoxFlags[acceptsTab];
            }
            set
            {
                if (this.textBoxFlags[acceptsTab] != value)
                {
                    this.textBoxFlags[acceptsTab] = value;
                    this.OnAcceptsTabChanged(EventArgs.Empty);
                }
            }
        }

        [RefreshProperties(RefreshProperties.Repaint), Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DefaultValue(true), Localizable(true), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("TextBoxAutoSizeDescr")]
        public override bool AutoSize
        {
            get
            {
                return this.textBoxFlags[autoSize];
            }
            set
            {
                if (this.textBoxFlags[autoSize] != value)
                {
                    this.textBoxFlags[autoSize] = value;
                    if (!this.Multiline)
                    {
                        base.SetStyle(ControlStyles.FixedHeight, value);
                        this.AdjustHeight(false);
                    }
                    this.OnAutoSizeChanged(EventArgs.Empty);
                }
            }
        }

        [System.Windows.Forms.SRDescription("ControlBackColorDescr"), System.Windows.Forms.SRCategory("CatAppearance"), DispId(-501)]
        public override Color BackColor
        {
            get
            {
                if (this.ShouldSerializeBackColor())
                {
                    return base.BackColor;
                }
                if (this.ReadOnly)
                {
                    return SystemColors.Control;
                }
                return SystemColors.Window;
            }
            set
            {
                base.BackColor = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override Image BackgroundImage
        {
            get
            {
                return base.BackgroundImage;
            }
            set
            {
                base.BackgroundImage = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override ImageLayout BackgroundImageLayout
        {
            get
            {
                return base.BackgroundImageLayout;
            }
            set
            {
                base.BackgroundImageLayout = value;
            }
        }

        [DefaultValue(2), DispId(-504), System.Windows.Forms.SRDescription("TextBoxBorderDescr"), System.Windows.Forms.SRCategory("CatAppearance")]
        public System.Windows.Forms.BorderStyle BorderStyle
        {
            get
            {
                return this.borderStyle;
            }
            set
            {
                if (this.borderStyle != value)
                {
                    if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                    {
                        throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.BorderStyle));
                    }
                    this.borderStyle = value;
                    base.UpdateStyles();
                    base.RecreateHandle();
                    using (LayoutTransaction.CreateTransactionIf(this.AutoSize, this.ParentInternal, this, PropertyNames.BorderStyle))
                    {
                        this.OnBorderStyleChanged(EventArgs.Empty);
                    }
                }
            }
        }

        protected override bool CanEnableIme
        {
            get
            {
                return ((!this.ReadOnly && !this.PasswordProtect) && base.CanEnableIme);
            }
        }

        internal virtual bool CanRaiseTextChangedEvent
        {
            get
            {
                return true;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("TextBoxCanUndoDescr")]
        public bool CanUndo
        {
            get
            {
                return (base.IsHandleCreated && (((int) ((long) base.SendMessage(0xc6, 0, 0))) != 0));
            }
        }

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                createParams.ClassName = "EDIT";
                createParams.Style |= 0xc0;
                if (!this.textBoxFlags[hideSelection])
                {
                    createParams.Style |= 0x100;
                }
                if (this.textBoxFlags[readOnly])
                {
                    createParams.Style |= 0x800;
                }
                createParams.ExStyle &= -513;
                createParams.Style &= -8388609;
                switch (this.borderStyle)
                {
                    case System.Windows.Forms.BorderStyle.FixedSingle:
                        createParams.Style |= 0x800000;
                        break;

                    case System.Windows.Forms.BorderStyle.Fixed3D:
                        createParams.ExStyle |= 0x200;
                        break;
                }
                if (this.textBoxFlags[multiline])
                {
                    createParams.Style |= 4;
                    if (this.textBoxFlags[wordWrap])
                    {
                        createParams.Style &= -129;
                    }
                }
                return createParams;
            }
        }

        protected override Cursor DefaultCursor
        {
            get
            {
                return Cursors.IBeam;
            }
        }

        protected override Size DefaultSize
        {
            get
            {
                return new Size(100, this.PreferredHeight);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected override bool DoubleBuffered
        {
            get
            {
                return base.DoubleBuffered;
            }
            set
            {
                base.DoubleBuffered = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ControlForeColorDescr"), DispId(-513)]
        public override Color ForeColor
        {
            get
            {
                if (this.ShouldSerializeForeColor())
                {
                    return base.ForeColor;
                }
                return SystemColors.WindowText;
            }
            set
            {
                base.ForeColor = value;
            }
        }

        [DefaultValue(true), System.Windows.Forms.SRDescription("TextBoxHideSelectionDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public bool HideSelection
        {
            get
            {
                return this.textBoxFlags[hideSelection];
            }
            set
            {
                if (this.textBoxFlags[hideSelection] != value)
                {
                    this.textBoxFlags[hideSelection] = value;
                    base.RecreateHandle();
                    this.OnHideSelectionChanged(EventArgs.Empty);
                }
            }
        }

        protected override ImeMode ImeModeBase
        {
            get
            {
                if (base.DesignMode)
                {
                    return base.ImeModeBase;
                }
                return (this.CanEnableIme ? base.ImeModeBase : ImeMode.Disable);
            }
            set
            {
                base.ImeModeBase = value;
            }
        }

        [Localizable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("TextBoxLinesDescr"), Editor("System.Windows.Forms.Design.StringArrayEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), MergableProperty(false)]
        public string[] Lines
        {
            get
            {
                int num2;
                string text = this.Text;
                ArrayList list = new ArrayList();
                for (int i = 0; i < text.Length; i = num2)
                {
                    num2 = i;
                    while (num2 < text.Length)
                    {
                        char ch = text[num2];
                        if ((ch == '\r') || (ch == '\n'))
                        {
                            break;
                        }
                        num2++;
                    }
                    string str2 = text.Substring(i, num2 - i);
                    list.Add(str2);
                    if ((num2 < text.Length) && (text[num2] == '\r'))
                    {
                        num2++;
                    }
                    if ((num2 < text.Length) && (text[num2] == '\n'))
                    {
                        num2++;
                    }
                }
                if ((text.Length > 0) && ((text[text.Length - 1] == '\r') || (text[text.Length - 1] == '\n')))
                {
                    list.Add("");
                }
                return (string[]) list.ToArray(typeof(string));
            }
            set
            {
                if ((value != null) && (value.Length > 0))
                {
                    StringBuilder builder = new StringBuilder(value[0]);
                    for (int i = 1; i < value.Length; i++)
                    {
                        builder.Append("\r\n");
                        builder.Append(value[i]);
                    }
                    this.Text = builder.ToString();
                }
                else
                {
                    this.Text = "";
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), Localizable(true), System.Windows.Forms.SRDescription("TextBoxMaxLengthDescr"), DefaultValue(0x7fff)]
        public virtual int MaxLength
        {
            get
            {
                return this.maxLength;
            }
            set
            {
                if (value < 0)
                {
                    object[] args = new object[] { "MaxLength", value.ToString(CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentOutOfRangeException("MaxLength", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", args));
                }
                if (this.maxLength != value)
                {
                    this.maxLength = value;
                    this.UpdateMaxLength();
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("TextBoxModifiedDescr"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Modified
        {
            get
            {
                if (!base.IsHandleCreated)
                {
                    return this.textBoxFlags[modified];
                }
                bool flag = 0 != ((int) ((long) base.SendMessage(0xb8, 0, 0)));
                if (this.textBoxFlags[modified] != flag)
                {
                    this.textBoxFlags[modified] = flag;
                    this.OnModifiedChanged(EventArgs.Empty);
                }
                return flag;
            }
            set
            {
                if (this.Modified != value)
                {
                    if (base.IsHandleCreated)
                    {
                        base.SendMessage(0xb9, value ? 1 : 0, 0);
                    }
                    this.textBoxFlags[modified] = value;
                    this.OnModifiedChanged(EventArgs.Empty);
                }
            }
        }

        [System.Windows.Forms.SRDescription("TextBoxMultilineDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(false), Localizable(true), RefreshProperties(RefreshProperties.All)]
        public virtual bool Multiline
        {
            get
            {
                return this.textBoxFlags[multiline];
            }
            set
            {
                if (this.textBoxFlags[multiline] != value)
                {
                    using (LayoutTransaction.CreateTransactionIf(this.AutoSize, this.ParentInternal, this, PropertyNames.Multiline))
                    {
                        this.textBoxFlags[multiline] = value;
                        if (value)
                        {
                            base.SetStyle(ControlStyles.FixedHeight, false);
                        }
                        else
                        {
                            base.SetStyle(ControlStyles.FixedHeight, this.AutoSize);
                        }
                        base.RecreateHandle();
                        this.AdjustHeight(false);
                        this.OnMultilineChanged(EventArgs.Empty);
                    }
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
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

        internal virtual bool PasswordProtect
        {
            get
            {
                return false;
            }
        }

        [System.Windows.Forms.SRDescription("TextBoxPreferredHeightDescr"), System.Windows.Forms.SRCategory("CatLayout"), Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int PreferredHeight
        {
            get
            {
                int fontHeight = base.FontHeight;
                if (this.borderStyle != System.Windows.Forms.BorderStyle.None)
                {
                    fontHeight += (SystemInformation.BorderSize.Height * 4) + 3;
                }
                return fontHeight;
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("TextBoxReadOnlyDescr"), DefaultValue(false), RefreshProperties(RefreshProperties.Repaint)]
        public bool ReadOnly
        {
            get
            {
                return this.textBoxFlags[readOnly];
            }
            set
            {
                if (this.textBoxFlags[readOnly] != value)
                {
                    this.textBoxFlags[readOnly] = value;
                    if (base.IsHandleCreated)
                    {
                        base.SendMessage(0xcf, value ? -1 : 0, 0);
                    }
                    this.OnReadOnlyChanged(EventArgs.Empty);
                    base.VerifyImeRestrictedModeChanged();
                }
            }
        }

        [System.Windows.Forms.SRDescription("TextBoxSelectedTextDescr"), System.Windows.Forms.SRCategory("CatAppearance"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual string SelectedText
        {
            get
            {
                int num;
                int num2;
                this.GetSelectionStartAndLength(out num, out num2);
                return this.Text.Substring(num, num2);
            }
            set
            {
                this.SetSelectedTextInternal(value, true);
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("TextBoxSelectionLengthDescr"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual int SelectionLength
        {
            get
            {
                int num;
                int num2;
                this.GetSelectionStartAndLength(out num, out num2);
                return num2;
            }
            set
            {
                int num;
                int num2;
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("SelectionLength", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "SelectionLength", value.ToString(CultureInfo.CurrentCulture) }));
                }
                this.GetSelectionStartAndLength(out num, out num2);
                if (value != num2)
                {
                    this.Select(num, value);
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("TextBoxSelectionStartDescr"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectionStart
        {
            get
            {
                int num;
                int num2;
                this.GetSelectionStartAndLength(out num, out num2);
                return num;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("SelectionStart", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "SelectionStart", value.ToString(CultureInfo.CurrentCulture) }));
                }
                this.Select(value, this.SelectionLength);
            }
        }

        internal virtual bool SelectionUsesDbcsOffsetsInWin9x
        {
            get
            {
                return true;
            }
        }

        internal virtual bool SetSelectionInCreateHandle
        {
            get
            {
                return true;
            }
        }

        [DefaultValue(true), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("TextBoxShortcutsEnabledDescr")]
        public virtual bool ShortcutsEnabled
        {
            get
            {
                return this.textBoxFlags[shortcutsEnabled];
            }
            set
            {
                if (shortcutsToDisable == null)
                {
                    shortcutsToDisable = new int[] { 0x2005a, 0x20043, 0x20058, 0x20056, 0x20041, 0x2004c, 0x20052, 0x20045, 0x20059, 0x20008, 0x2002e, 0x1002e, 0x1002d, 0x2004a };
                }
                this.textBoxFlags[shortcutsEnabled] = value;
            }
        }

        [Localizable(true), Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                if (value != base.Text)
                {
                    base.Text = value;
                    if (base.IsHandleCreated)
                    {
                        base.SendMessage(0xb9, 0, 0);
                    }
                }
            }
        }

        [Browsable(false)]
        public virtual int TextLength
        {
            get
            {
                if (base.IsHandleCreated && (Marshal.SystemDefaultCharSize == 2))
                {
                    return System.Windows.Forms.SafeNativeMethods.GetWindowTextLength(new HandleRef(this, base.Handle));
                }
                return this.Text.Length;
            }
        }

        internal override string WindowText
        {
            get
            {
                return base.WindowText;
            }
            set
            {
                if (value == null)
                {
                    value = "";
                }
                if (!this.WindowText.Equals(value))
                {
                    this.textBoxFlags[codeUpdateText] = true;
                    try
                    {
                        base.WindowText = value;
                    }
                    finally
                    {
                        this.textBoxFlags[codeUpdateText] = false;
                    }
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(true), System.Windows.Forms.SRDescription("TextBoxWordWrapDescr"), Localizable(true)]
        public bool WordWrap
        {
            get
            {
                return this.textBoxFlags[wordWrap];
            }
            set
            {
                using (LayoutTransaction.CreateTransactionIf(this.AutoSize, this.ParentInternal, this, PropertyNames.WordWrap))
                {
                    if (this.textBoxFlags[wordWrap] != value)
                    {
                        this.textBoxFlags[wordWrap] = value;
                        base.RecreateHandle();
                    }
                }
            }
        }
    }
}

