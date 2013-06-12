namespace System.Windows.Forms
{
    using Microsoft.Win32;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Runtime.InteropServices;
    using System.Windows.Forms.Design;
    using System.Windows.Forms.Layout;

    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ContextMenuStrip | ToolStripItemDesignerAvailability.MenuStrip | ToolStripItemDesignerAvailability.ToolStrip)]
    public class ToolStripTextBox : ToolStripControlHost
    {
        internal static readonly object EventAcceptsTabChanged = new object();
        internal static readonly object EventBorderStyleChanged = new object();
        internal static readonly object EventHideSelectionChanged = new object();
        internal static readonly object EventModifiedChanged = new object();
        internal static readonly object EventMultilineChanged = new object();
        internal static readonly object EventReadOnlyChanged = new object();
        internal static readonly object EventTextBoxTextAlignChanged = new object();

        [System.Windows.Forms.SRDescription("TextBoxBaseOnAcceptsTabChangedDescr"), System.Windows.Forms.SRCategory("CatPropertyChanged")]
        public event EventHandler AcceptsTabChanged
        {
            add
            {
                base.Events.AddHandler(EventAcceptsTabChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventAcceptsTabChanged, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("TextBoxBaseOnBorderStyleChangedDescr")]
        public event EventHandler BorderStyleChanged
        {
            add
            {
                base.Events.AddHandler(EventBorderStyleChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventBorderStyleChanged, value);
            }
        }

        [System.Windows.Forms.SRDescription("TextBoxBaseOnHideSelectionChangedDescr"), System.Windows.Forms.SRCategory("CatPropertyChanged")]
        public event EventHandler HideSelectionChanged
        {
            add
            {
                base.Events.AddHandler(EventHideSelectionChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventHideSelectionChanged, value);
            }
        }

        [System.Windows.Forms.SRDescription("TextBoxBaseOnModifiedChangedDescr"), System.Windows.Forms.SRCategory("CatPropertyChanged")]
        public event EventHandler ModifiedChanged
        {
            add
            {
                base.Events.AddHandler(EventModifiedChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventModifiedChanged, value);
            }
        }

        [System.Windows.Forms.SRDescription("TextBoxBaseOnMultilineChangedDescr"), Browsable(false), EditorBrowsable(EditorBrowsableState.Never), System.Windows.Forms.SRCategory("CatPropertyChanged")]
        public event EventHandler MultilineChanged
        {
            add
            {
                base.Events.AddHandler(EventMultilineChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventMultilineChanged, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("TextBoxBaseOnReadOnlyChangedDescr")]
        public event EventHandler ReadOnlyChanged
        {
            add
            {
                base.Events.AddHandler(EventReadOnlyChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventReadOnlyChanged, value);
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripTextBoxTextBoxTextAlignChangedDescr"), System.Windows.Forms.SRCategory("CatPropertyChanged")]
        public event EventHandler TextBoxTextAlignChanged
        {
            add
            {
                base.Events.AddHandler(EventTextBoxTextAlignChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventTextBoxTextAlignChanged, value);
            }
        }

        public ToolStripTextBox() : base(CreateControlInstance())
        {
            ToolStripTextBoxControl control = base.Control as ToolStripTextBoxControl;
            control.Owner = this;
        }

        public ToolStripTextBox(string name) : this()
        {
            base.Name = name;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public ToolStripTextBox(Control c) : base(c)
        {
            throw new NotSupportedException(System.Windows.Forms.SR.GetString("ToolStripMustSupplyItsOwnTextBox"));
        }

        public void AppendText(string text)
        {
            this.TextBox.AppendText(text);
        }

        public void Clear()
        {
            this.TextBox.Clear();
        }

        public void ClearUndo()
        {
            this.TextBox.ClearUndo();
        }

        public void Copy()
        {
            this.TextBox.Copy();
        }

        private static Control CreateControlInstance()
        {
            return new ToolStripTextBoxControl { BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D, AutoSize = true };
        }

        public void Cut()
        {
            this.TextBox.Copy();
        }

        public void DeselectAll()
        {
            this.TextBox.DeselectAll();
        }

        public char GetCharFromPosition(Point pt)
        {
            return this.TextBox.GetCharFromPosition(pt);
        }

        public int GetCharIndexFromPosition(Point pt)
        {
            return this.TextBox.GetCharIndexFromPosition(pt);
        }

        public int GetFirstCharIndexFromLine(int lineNumber)
        {
            return this.TextBox.GetFirstCharIndexFromLine(lineNumber);
        }

        public int GetFirstCharIndexOfCurrentLine()
        {
            return this.TextBox.GetFirstCharIndexOfCurrentLine();
        }

        public int GetLineFromCharIndex(int index)
        {
            return this.TextBox.GetLineFromCharIndex(index);
        }

        public Point GetPositionFromCharIndex(int index)
        {
            return this.TextBox.GetPositionFromCharIndex(index);
        }

        public override Size GetPreferredSize(Size constrainingSize)
        {
            return new Size(CommonProperties.GetSpecifiedBounds(this.TextBox).Width, this.TextBox.PreferredHeight);
        }

        private void HandleAcceptsTabChanged(object sender, EventArgs e)
        {
            this.OnAcceptsTabChanged(e);
        }

        private void HandleBorderStyleChanged(object sender, EventArgs e)
        {
            this.OnBorderStyleChanged(e);
        }

        private void HandleHideSelectionChanged(object sender, EventArgs e)
        {
            this.OnHideSelectionChanged(e);
        }

        private void HandleModifiedChanged(object sender, EventArgs e)
        {
            this.OnModifiedChanged(e);
        }

        private void HandleMultilineChanged(object sender, EventArgs e)
        {
            this.OnMultilineChanged(e);
        }

        private void HandleReadOnlyChanged(object sender, EventArgs e)
        {
            this.OnReadOnlyChanged(e);
        }

        private void HandleTextBoxTextAlignChanged(object sender, EventArgs e)
        {
            base.RaiseEvent(EventTextBoxTextAlignChanged, e);
        }

        protected virtual void OnAcceptsTabChanged(EventArgs e)
        {
            base.RaiseEvent(EventAcceptsTabChanged, e);
        }

        protected virtual void OnBorderStyleChanged(EventArgs e)
        {
            base.RaiseEvent(EventBorderStyleChanged, e);
        }

        protected virtual void OnHideSelectionChanged(EventArgs e)
        {
            base.RaiseEvent(EventHideSelectionChanged, e);
        }

        protected virtual void OnModifiedChanged(EventArgs e)
        {
            base.RaiseEvent(EventModifiedChanged, e);
        }

        protected virtual void OnMultilineChanged(EventArgs e)
        {
            base.RaiseEvent(EventMultilineChanged, e);
        }

        protected virtual void OnReadOnlyChanged(EventArgs e)
        {
            base.RaiseEvent(EventReadOnlyChanged, e);
        }

        protected override void OnSubscribeControlEvents(Control control)
        {
            System.Windows.Forms.TextBox box = control as System.Windows.Forms.TextBox;
            if (box != null)
            {
                box.AcceptsTabChanged += new EventHandler(this.HandleAcceptsTabChanged);
                box.BorderStyleChanged += new EventHandler(this.HandleBorderStyleChanged);
                box.HideSelectionChanged += new EventHandler(this.HandleHideSelectionChanged);
                box.ModifiedChanged += new EventHandler(this.HandleModifiedChanged);
                box.MultilineChanged += new EventHandler(this.HandleMultilineChanged);
                box.ReadOnlyChanged += new EventHandler(this.HandleReadOnlyChanged);
                box.TextAlignChanged += new EventHandler(this.HandleTextBoxTextAlignChanged);
            }
            base.OnSubscribeControlEvents(control);
        }

        protected override void OnUnsubscribeControlEvents(Control control)
        {
            System.Windows.Forms.TextBox box = control as System.Windows.Forms.TextBox;
            if (box != null)
            {
                box.AcceptsTabChanged -= new EventHandler(this.HandleAcceptsTabChanged);
                box.BorderStyleChanged -= new EventHandler(this.HandleBorderStyleChanged);
                box.HideSelectionChanged -= new EventHandler(this.HandleHideSelectionChanged);
                box.ModifiedChanged -= new EventHandler(this.HandleModifiedChanged);
                box.MultilineChanged -= new EventHandler(this.HandleMultilineChanged);
                box.ReadOnlyChanged -= new EventHandler(this.HandleReadOnlyChanged);
                box.TextAlignChanged -= new EventHandler(this.HandleTextBoxTextAlignChanged);
            }
            base.OnUnsubscribeControlEvents(control);
        }

        public void Paste()
        {
            this.TextBox.Paste();
        }

        public void ScrollToCaret()
        {
            this.TextBox.ScrollToCaret();
        }

        public void Select(int start, int length)
        {
            this.TextBox.Select(start, length);
        }

        public void SelectAll()
        {
            this.TextBox.SelectAll();
        }

        internal override bool ShouldSerializeFont()
        {
            return (this.Font != ToolStripManager.DefaultFont);
        }

        public void Undo()
        {
            this.TextBox.Undo();
        }

        [DefaultValue(false), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("TextBoxAcceptsReturnDescr")]
        public bool AcceptsReturn
        {
            get
            {
                return this.TextBox.AcceptsReturn;
            }
            set
            {
                this.TextBox.AcceptsReturn = value;
            }
        }

        [DefaultValue(false), System.Windows.Forms.SRDescription("TextBoxAcceptsTabDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public bool AcceptsTab
        {
            get
            {
                return this.TextBox.AcceptsTab;
            }
            set
            {
                this.TextBox.AcceptsTab = value;
            }
        }

        [Editor("System.Windows.Forms.Design.ListControlStringCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), Browsable(true), EditorBrowsable(EditorBrowsableState.Always), Localizable(true), System.Windows.Forms.SRDescription("TextBoxAutoCompleteCustomSourceDescr"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public AutoCompleteStringCollection AutoCompleteCustomSource
        {
            get
            {
                return this.TextBox.AutoCompleteCustomSource;
            }
            set
            {
                this.TextBox.AutoCompleteCustomSource = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(0), System.Windows.Forms.SRDescription("TextBoxAutoCompleteModeDescr")]
        public System.Windows.Forms.AutoCompleteMode AutoCompleteMode
        {
            get
            {
                return this.TextBox.AutoCompleteMode;
            }
            set
            {
                this.TextBox.AutoCompleteMode = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(0x80), System.Windows.Forms.SRDescription("TextBoxAutoCompleteSourceDescr")]
        public System.Windows.Forms.AutoCompleteSource AutoCompleteSource
        {
            get
            {
                return this.TextBox.AutoCompleteSource;
            }
            set
            {
                this.TextBox.AutoCompleteSource = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

        [System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(2), System.Windows.Forms.SRDescription("TextBoxBorderDescr"), DispId(-504)]
        public System.Windows.Forms.BorderStyle BorderStyle
        {
            get
            {
                return this.TextBox.BorderStyle;
            }
            set
            {
                this.TextBox.BorderStyle = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("TextBoxCanUndoDescr"), Browsable(false)]
        public bool CanUndo
        {
            get
            {
                return this.TextBox.CanUndo;
            }
        }

        [DefaultValue(0), System.Windows.Forms.SRDescription("TextBoxCharacterCasingDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public System.Windows.Forms.CharacterCasing CharacterCasing
        {
            get
            {
                return this.TextBox.CharacterCasing;
            }
            set
            {
                this.TextBox.CharacterCasing = value;
            }
        }

        protected internal override Padding DefaultMargin
        {
            get
            {
                if (base.IsOnDropDown)
                {
                    return new Padding(1);
                }
                return new Padding(1, 0, 1, 0);
            }
        }

        protected override Size DefaultSize
        {
            get
            {
                return new Size(100, 0x16);
            }
        }

        [System.Windows.Forms.SRDescription("TextBoxHideSelectionDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(true)]
        public bool HideSelection
        {
            get
            {
                return this.TextBox.HideSelection;
            }
            set
            {
                this.TextBox.HideSelection = value;
            }
        }

        [Editor("System.Windows.Forms.Design.StringArrayEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), System.Windows.Forms.SRDescription("TextBoxLinesDescr"), Localizable(true), System.Windows.Forms.SRCategory("CatAppearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string[] Lines
        {
            get
            {
                return this.TextBox.Lines;
            }
            set
            {
                this.TextBox.Lines = value;
            }
        }

        [Localizable(true), DefaultValue(0x7fff), System.Windows.Forms.SRDescription("TextBoxMaxLengthDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public int MaxLength
        {
            get
            {
                return this.TextBox.MaxLength;
            }
            set
            {
                this.TextBox.MaxLength = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("TextBoxModifiedDescr"), Browsable(false), System.Windows.Forms.SRCategory("CatBehavior")]
        public bool Modified
        {
            get
            {
                return this.TextBox.Modified;
            }
            set
            {
                this.TextBox.Modified = value;
            }
        }

        [System.Windows.Forms.SRDescription("TextBoxMultilineDescr"), EditorBrowsable(EditorBrowsableState.Never), DefaultValue(false), Localizable(true), Browsable(false), System.Windows.Forms.SRCategory("CatBehavior"), RefreshProperties(RefreshProperties.All)]
        public bool Multiline
        {
            get
            {
                return this.TextBox.Multiline;
            }
            set
            {
                this.TextBox.Multiline = value;
            }
        }

        [System.Windows.Forms.SRDescription("TextBoxReadOnlyDescr"), DefaultValue(false), System.Windows.Forms.SRCategory("CatBehavior")]
        public bool ReadOnly
        {
            get
            {
                return this.TextBox.ReadOnly;
            }
            set
            {
                this.TextBox.ReadOnly = value;
            }
        }

        [Browsable(false), System.Windows.Forms.SRDescription("TextBoxSelectedTextDescr"), System.Windows.Forms.SRCategory("CatAppearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SelectedText
        {
            get
            {
                return this.TextBox.SelectedText;
            }
            set
            {
                this.TextBox.SelectedText = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("TextBoxSelectionLengthDescr"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectionLength
        {
            get
            {
                return this.TextBox.SelectionLength;
            }
            set
            {
                this.TextBox.SelectionLength = value;
            }
        }

        [System.Windows.Forms.SRDescription("TextBoxSelectionStartDescr"), System.Windows.Forms.SRCategory("CatAppearance"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectionStart
        {
            get
            {
                return this.TextBox.SelectionStart;
            }
            set
            {
                this.TextBox.SelectionStart = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("TextBoxShortcutsEnabledDescr"), DefaultValue(true)]
        public bool ShortcutsEnabled
        {
            get
            {
                return this.TextBox.ShortcutsEnabled;
            }
            set
            {
                this.TextBox.ShortcutsEnabled = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Windows.Forms.TextBox TextBox
        {
            get
            {
                return (base.Control as System.Windows.Forms.TextBox);
            }
        }

        [Localizable(true), System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(0), System.Windows.Forms.SRDescription("TextBoxTextAlignDescr")]
        public HorizontalAlignment TextBoxTextAlign
        {
            get
            {
                return this.TextBox.TextAlign;
            }
            set
            {
                this.TextBox.TextAlign = value;
            }
        }

        [Browsable(false)]
        public int TextLength
        {
            get
            {
                return this.TextBox.TextLength;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Localizable(true), System.Windows.Forms.SRDescription("TextBoxWordWrapDescr"), Browsable(false), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(true)]
        public bool WordWrap
        {
            get
            {
                return this.TextBox.WordWrap;
            }
            set
            {
                this.TextBox.WordWrap = value;
            }
        }

        private class ToolStripTextBoxControl : TextBox
        {
            private bool isFontSet = true;
            private bool mouseIsOver;
            private int numberHooked;
            private ToolStripTextBox ownerItem;

            public ToolStripTextBoxControl()
            {
                this.Font = ToolStripManager.DefaultFont;
                this.isFontSet = false;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    this.HookStaticEvents(false);
                }
                base.Dispose(disposing);
            }

            private void HookStaticEvents(bool hook)
            {
                if (hook)
                {
                    try
                    {
                        SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(this.OnUserPreferenceChanged);
                        return;
                    }
                    finally
                    {
                        this.numberHooked++;
                    }
                }
                if (this.numberHooked > 0)
                {
                    try
                    {
                        SystemEvents.UserPreferenceChanged -= new UserPreferenceChangedEventHandler(this.OnUserPreferenceChanged);
                    }
                    finally
                    {
                        this.numberHooked--;
                    }
                }
            }

            private void InvalidateNonClient()
            {
                if (this.IsPopupTextBox)
                {
                    System.Windows.Forms.NativeMethods.RECT absoluteClientRECT = this.AbsoluteClientRECT;
                    HandleRef nullHandleRef = System.Windows.Forms.NativeMethods.NullHandleRef;
                    HandleRef ref3 = System.Windows.Forms.NativeMethods.NullHandleRef;
                    HandleRef ref4 = System.Windows.Forms.NativeMethods.NullHandleRef;
                    try
                    {
                        ref4 = new HandleRef(this, System.Windows.Forms.SafeNativeMethods.CreateRectRgn(0, 0, base.Width, base.Height));
                        ref3 = new HandleRef(this, System.Windows.Forms.SafeNativeMethods.CreateRectRgn(absoluteClientRECT.left, absoluteClientRECT.top, absoluteClientRECT.right, absoluteClientRECT.bottom));
                        nullHandleRef = new HandleRef(this, System.Windows.Forms.SafeNativeMethods.CreateRectRgn(0, 0, 0, 0));
                        System.Windows.Forms.SafeNativeMethods.CombineRgn(nullHandleRef, ref4, ref3, 3);
                        System.Windows.Forms.NativeMethods.RECT rcUpdate = new System.Windows.Forms.NativeMethods.RECT();
                        System.Windows.Forms.SafeNativeMethods.RedrawWindow(new HandleRef(this, base.Handle), ref rcUpdate, nullHandleRef, 0x705);
                    }
                    finally
                    {
                        try
                        {
                            if (nullHandleRef.Handle != IntPtr.Zero)
                            {
                                System.Windows.Forms.SafeNativeMethods.DeleteObject(nullHandleRef);
                            }
                        }
                        finally
                        {
                            try
                            {
                                if (ref3.Handle != IntPtr.Zero)
                                {
                                    System.Windows.Forms.SafeNativeMethods.DeleteObject(ref3);
                                }
                            }
                            finally
                            {
                                if (ref4.Handle != IntPtr.Zero)
                                {
                                    System.Windows.Forms.SafeNativeMethods.DeleteObject(ref4);
                                }
                            }
                        }
                    }
                }
            }

            protected override void OnGotFocus(EventArgs e)
            {
                base.OnGotFocus(e);
                this.InvalidateNonClient();
            }

            protected override void OnLostFocus(EventArgs e)
            {
                base.OnLostFocus(e);
                this.InvalidateNonClient();
            }

            protected override void OnMouseEnter(EventArgs e)
            {
                base.OnMouseEnter(e);
                this.MouseIsOver = true;
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                base.OnMouseLeave(e);
                this.MouseIsOver = false;
            }

            private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
            {
                if ((e.Category == UserPreferenceCategory.Window) && !this.isFontSet)
                {
                    this.Font = ToolStripManager.DefaultFont;
                }
            }

            protected override void OnVisibleChanged(EventArgs e)
            {
                base.OnVisibleChanged(e);
                if (!base.Disposing && !base.IsDisposed)
                {
                    this.HookStaticEvents(base.Visible);
                }
            }

            private void WmNCPaint(ref Message m)
            {
                if (!this.IsPopupTextBox)
                {
                    base.WndProc(ref m);
                }
                else
                {
                    HandleRef hDC = new HandleRef(this, System.Windows.Forms.UnsafeNativeMethods.GetWindowDC(new HandleRef(this, m.HWnd)));
                    if (hDC.Handle == IntPtr.Zero)
                    {
                        throw new Win32Exception();
                    }
                    try
                    {
                        System.Drawing.Color controlDark = (this.MouseIsOver || this.Focused) ? this.ColorTable.TextBoxBorder : this.BackColor;
                        System.Drawing.Color backColor = this.BackColor;
                        if (!base.Enabled)
                        {
                            controlDark = SystemColors.ControlDark;
                            backColor = SystemColors.Control;
                        }
                        using (Graphics graphics = Graphics.FromHdcInternal(hDC.Handle))
                        {
                            Rectangle absoluteClientRectangle = this.AbsoluteClientRectangle;
                            using (Brush brush = new SolidBrush(backColor))
                            {
                                graphics.FillRectangle(brush, 0, 0, base.Width, absoluteClientRectangle.Top);
                                graphics.FillRectangle(brush, 0, 0, absoluteClientRectangle.Left, base.Height);
                                graphics.FillRectangle(brush, 0, absoluteClientRectangle.Bottom, base.Width, base.Height - absoluteClientRectangle.Height);
                                graphics.FillRectangle(brush, absoluteClientRectangle.Right, 0, base.Width - absoluteClientRectangle.Right, base.Height);
                            }
                            using (Pen pen = new Pen(controlDark))
                            {
                                graphics.DrawRectangle(pen, 0, 0, base.Width - 1, base.Height - 1);
                            }
                        }
                    }
                    finally
                    {
                        System.Windows.Forms.UnsafeNativeMethods.ReleaseDC(new HandleRef(this, base.Handle), hDC);
                    }
                    m.Result = IntPtr.Zero;
                }
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == 0x85)
                {
                    this.WmNCPaint(ref m);
                }
                else
                {
                    base.WndProc(ref m);
                }
            }

            private System.Windows.Forms.NativeMethods.RECT AbsoluteClientRECT
            {
                get
                {
                    System.Windows.Forms.NativeMethods.RECT lpRect = new System.Windows.Forms.NativeMethods.RECT();
                    CreateParams createParams = this.CreateParams;
                    System.Windows.Forms.SafeNativeMethods.AdjustWindowRectEx(ref lpRect, createParams.Style, this.HasMenu, createParams.ExStyle);
                    int num = -lpRect.left;
                    int num2 = -lpRect.top;
                    System.Windows.Forms.UnsafeNativeMethods.GetClientRect(new HandleRef(this, base.Handle), ref lpRect);
                    lpRect.left += num;
                    lpRect.right += num;
                    lpRect.top += num2;
                    lpRect.bottom += num2;
                    return lpRect;
                }
            }

            private Rectangle AbsoluteClientRectangle
            {
                get
                {
                    System.Windows.Forms.NativeMethods.RECT absoluteClientRECT = this.AbsoluteClientRECT;
                    return Rectangle.FromLTRB(absoluteClientRECT.top, absoluteClientRECT.top, absoluteClientRECT.right, absoluteClientRECT.bottom);
                }
            }

            private ProfessionalColorTable ColorTable
            {
                get
                {
                    if (this.Owner != null)
                    {
                        ToolStripProfessionalRenderer renderer = this.Owner.Renderer as ToolStripProfessionalRenderer;
                        if (renderer != null)
                        {
                            return renderer.ColorTable;
                        }
                    }
                    return ProfessionalColors.ColorTable;
                }
            }

            public override System.Drawing.Font Font
            {
                get
                {
                    return base.Font;
                }
                set
                {
                    base.Font = value;
                    this.isFontSet = this.ShouldSerializeFont();
                }
            }

            private bool IsPopupTextBox
            {
                get
                {
                    if (base.BorderStyle != BorderStyle.Fixed3D)
                    {
                        return false;
                    }
                    return ((this.Owner != null) && (this.Owner.Renderer is ToolStripProfessionalRenderer));
                }
            }

            internal bool MouseIsOver
            {
                get
                {
                    return this.mouseIsOver;
                }
                set
                {
                    if (this.mouseIsOver != value)
                    {
                        this.mouseIsOver = value;
                        if (!this.Focused)
                        {
                            this.InvalidateNonClient();
                        }
                    }
                }
            }

            public ToolStripTextBox Owner
            {
                get
                {
                    return this.ownerItem;
                }
                set
                {
                    this.ownerItem = value;
                }
            }
        }
    }
}

