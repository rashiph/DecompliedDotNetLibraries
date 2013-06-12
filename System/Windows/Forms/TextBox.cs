namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Windows.Forms.VisualStyles;

    [Designer("System.Windows.Forms.Design.TextBoxDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ClassInterface(ClassInterfaceType.AutoDispatch), System.Windows.Forms.SRDescription("DescriptionTextBox"), ComVisible(true)]
    public class TextBox : TextBoxBase
    {
        private bool acceptsReturn;
        private AutoCompleteStringCollection autoCompleteCustomSource;
        private System.Windows.Forms.AutoCompleteMode autoCompleteMode;
        private System.Windows.Forms.AutoCompleteSource autoCompleteSource = System.Windows.Forms.AutoCompleteSource.None;
        private System.Windows.Forms.CharacterCasing characterCasing;
        private static readonly object EVENT_TEXTALIGNCHANGED = new object();
        private bool fromHandleCreate;
        private char passwordChar;
        private System.Windows.Forms.ScrollBars scrollBars;
        private bool selectionSet;
        private StringSource stringSource;
        private HorizontalAlignment textAlign;
        private bool useSystemPasswordChar;

        [System.Windows.Forms.SRDescription("RadioButtonOnTextAlignChangedDescr"), System.Windows.Forms.SRCategory("CatPropertyChanged")]
        public event EventHandler TextAlignChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_TEXTALIGNCHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_TEXTALIGNCHANGED, value);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.ResetAutoComplete(true);
                if (this.autoCompleteCustomSource != null)
                {
                    this.autoCompleteCustomSource.CollectionChanged -= new CollectionChangeEventHandler(this.OnAutoCompleteCustomSourceChanged);
                }
                if (this.stringSource != null)
                {
                    this.stringSource.ReleaseAutoComplete();
                    this.stringSource = null;
                }
            }
            base.Dispose(disposing);
        }

        internal override Size GetPreferredSizeCore(Size proposedConstraints)
        {
            Size empty = Size.Empty;
            if ((this.Multiline && !base.WordWrap) && ((this.ScrollBars & System.Windows.Forms.ScrollBars.Horizontal) != System.Windows.Forms.ScrollBars.None))
            {
                empty.Height += SystemInformation.HorizontalScrollBarHeight;
            }
            if (this.Multiline && ((this.ScrollBars & System.Windows.Forms.ScrollBars.Vertical) != System.Windows.Forms.ScrollBars.None))
            {
                empty.Width += SystemInformation.VerticalScrollBarWidth;
            }
            proposedConstraints -= empty;
            return (base.GetPreferredSizeCore(proposedConstraints) + empty);
        }

        private string[] GetStringsForAutoComplete()
        {
            string[] strArray = new string[this.AutoCompleteCustomSource.Count];
            for (int i = 0; i < this.AutoCompleteCustomSource.Count; i++)
            {
                strArray[i] = this.AutoCompleteCustomSource[i];
            }
            return strArray;
        }

        protected override bool IsInputKey(Keys keyData)
        {
            if (this.Multiline && ((keyData & Keys.Alt) == Keys.None))
            {
                Keys keys = keyData & Keys.KeyCode;
                if (keys == Keys.Enter)
                {
                    return this.acceptsReturn;
                }
            }
            return base.IsInputKey(keyData);
        }

        private void OnAutoCompleteCustomSourceChanged(object sender, CollectionChangeEventArgs e)
        {
            if (this.AutoCompleteSource == System.Windows.Forms.AutoCompleteSource.CustomSource)
            {
                this.SetAutoComplete(true);
            }
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            if ((Application.RenderWithVisualStyles && base.IsHandleCreated) && (base.BorderStyle == BorderStyle.Fixed3D))
            {
                System.Windows.Forms.SafeNativeMethods.RedrawWindow(new HandleRef(this, base.Handle), (System.Windows.Forms.NativeMethods.COMRECT) null, System.Windows.Forms.NativeMethods.NullHandleRef, 0x401);
            }
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            if (this.AutoCompleteMode != System.Windows.Forms.AutoCompleteMode.None)
            {
                base.RecreateHandle();
            }
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            if (!this.selectionSet)
            {
                this.selectionSet = true;
                if ((this.SelectionLength == 0) && (Control.MouseButtons == MouseButtons.None))
                {
                    base.SelectAll();
                }
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            base.SetSelectionOnHandle();
            if ((this.passwordChar != '\0') && !this.useSystemPasswordChar)
            {
                base.SendMessage(0xcc, this.passwordChar, 0);
            }
            base.VerifyImeRestrictedModeChanged();
            if (this.AutoCompleteMode != System.Windows.Forms.AutoCompleteMode.None)
            {
                try
                {
                    this.fromHandleCreate = true;
                    this.SetAutoComplete(false);
                }
                finally
                {
                    this.fromHandleCreate = false;
                }
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (this.stringSource != null)
            {
                this.stringSource.ReleaseAutoComplete();
                this.stringSource = null;
            }
            base.OnHandleDestroyed(e);
        }

        protected virtual void OnTextAlignChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EVENT_TEXTALIGNCHANGED] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void Paste(string text)
        {
            base.SetSelectedTextInternal(text, false);
        }

        private void ResetAutoComplete(bool force)
        {
            if (((this.AutoCompleteMode != System.Windows.Forms.AutoCompleteMode.None) || force) && base.IsHandleCreated)
            {
                int flags = -1610612729;
                System.Windows.Forms.SafeNativeMethods.SHAutoComplete(new HandleRef(this, base.Handle), flags);
            }
        }

        private void ResetAutoCompleteCustomSource()
        {
            this.AutoCompleteCustomSource = null;
        }

        internal override void SelectInternal(int start, int length, int textLen)
        {
            this.selectionSet = true;
            base.SelectInternal(start, length, textLen);
        }

        internal void SetAutoComplete(bool reset)
        {
            if ((!this.Multiline && (this.passwordChar == '\0')) && (!this.useSystemPasswordChar && (this.AutoCompleteSource != System.Windows.Forms.AutoCompleteSource.None)))
            {
                if (this.AutoCompleteMode != System.Windows.Forms.AutoCompleteMode.None)
                {
                    if (!this.fromHandleCreate)
                    {
                        System.Windows.Forms.AutoCompleteMode autoCompleteMode = this.AutoCompleteMode;
                        this.autoCompleteMode = System.Windows.Forms.AutoCompleteMode.None;
                        base.RecreateHandle();
                        this.autoCompleteMode = autoCompleteMode;
                    }
                    if (this.AutoCompleteSource == System.Windows.Forms.AutoCompleteSource.CustomSource)
                    {
                        if (base.IsHandleCreated && (this.AutoCompleteCustomSource != null))
                        {
                            if (this.AutoCompleteCustomSource.Count == 0)
                            {
                                this.ResetAutoComplete(true);
                            }
                            else if (this.stringSource != null)
                            {
                                this.stringSource.RefreshList(this.GetStringsForAutoComplete());
                            }
                            else
                            {
                                this.stringSource = new StringSource(this.GetStringsForAutoComplete());
                                if (!this.stringSource.Bind(new HandleRef(this, base.Handle), (int) this.AutoCompleteMode))
                                {
                                    throw new ArgumentException(System.Windows.Forms.SR.GetString("AutoCompleteFailure"));
                                }
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            if (base.IsHandleCreated)
                            {
                                int num = 0;
                                if (this.AutoCompleteMode == System.Windows.Forms.AutoCompleteMode.Suggest)
                                {
                                    num |= -1879048192;
                                }
                                if (this.AutoCompleteMode == System.Windows.Forms.AutoCompleteMode.Append)
                                {
                                    num |= 0x60000000;
                                }
                                if (this.AutoCompleteMode == System.Windows.Forms.AutoCompleteMode.SuggestAppend)
                                {
                                    num |= 0x10000000;
                                    num |= 0x40000000;
                                }
                                System.Windows.Forms.SafeNativeMethods.SHAutoComplete(new HandleRef(this, base.Handle), ((int) this.AutoCompleteSource) | num);
                            }
                        }
                        catch (SecurityException)
                        {
                        }
                    }
                }
                else if (reset)
                {
                    this.ResetAutoComplete(true);
                }
            }
        }

        private void WmPrint(ref Message m)
        {
            base.WndProc(ref m);
            if ((((2 & ((int) m.LParam)) != 0) && Application.RenderWithVisualStyles) && (base.BorderStyle == BorderStyle.Fixed3D))
            {
                System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                try
                {
                    using (Graphics graphics = Graphics.FromHdc(m.WParam))
                    {
                        Rectangle rect = new Rectangle(0, 0, base.Size.Width - 1, base.Size.Height - 1);
                        using (Pen pen = new Pen(VisualStyleInformation.TextControlBorder))
                        {
                            graphics.DrawRectangle(pen, rect);
                        }
                        rect.Inflate(-1, -1);
                        graphics.DrawRectangle(SystemPens.Window, rect);
                    }
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x201:
                {
                    MouseButtons mouseButtons = Control.MouseButtons;
                    bool validationCancelled = base.ValidationCancelled;
                    this.FocusInternal();
                    if ((mouseButtons != Control.MouseButtons) || (base.ValidationCancelled && !validationCancelled))
                    {
                        break;
                    }
                    base.WndProc(ref m);
                    return;
                }
                case 0x202:
                    base.WndProc(ref m);
                    return;

                case 0x317:
                    this.WmPrint(ref m);
                    return;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(false), System.Windows.Forms.SRDescription("TextBoxAcceptsReturnDescr")]
        public bool AcceptsReturn
        {
            get
            {
                return this.acceptsReturn;
            }
            set
            {
                this.acceptsReturn = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), Localizable(true), Editor("System.Windows.Forms.Design.ListControlStringCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), System.Windows.Forms.SRDescription("TextBoxAutoCompleteCustomSourceDescr")]
        public AutoCompleteStringCollection AutoCompleteCustomSource
        {
            get
            {
                if (this.autoCompleteCustomSource == null)
                {
                    this.autoCompleteCustomSource = new AutoCompleteStringCollection();
                    this.autoCompleteCustomSource.CollectionChanged += new CollectionChangeEventHandler(this.OnAutoCompleteCustomSourceChanged);
                }
                return this.autoCompleteCustomSource;
            }
            set
            {
                if (this.autoCompleteCustomSource != value)
                {
                    if (this.autoCompleteCustomSource != null)
                    {
                        this.autoCompleteCustomSource.CollectionChanged -= new CollectionChangeEventHandler(this.OnAutoCompleteCustomSourceChanged);
                    }
                    this.autoCompleteCustomSource = value;
                    if (value != null)
                    {
                        this.autoCompleteCustomSource.CollectionChanged += new CollectionChangeEventHandler(this.OnAutoCompleteCustomSourceChanged);
                    }
                    this.SetAutoComplete(false);
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), DefaultValue(0), System.Windows.Forms.SRDescription("TextBoxAutoCompleteModeDescr"), Browsable(true)]
        public System.Windows.Forms.AutoCompleteMode AutoCompleteMode
        {
            get
            {
                return this.autoCompleteMode;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 3))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.AutoCompleteMode));
                }
                bool reset = false;
                if ((this.autoCompleteMode != System.Windows.Forms.AutoCompleteMode.None) && (value == System.Windows.Forms.AutoCompleteMode.None))
                {
                    reset = true;
                }
                this.autoCompleteMode = value;
                this.SetAutoComplete(reset);
            }
        }

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always), DefaultValue(0x80), System.Windows.Forms.SRDescription("TextBoxAutoCompleteSourceDescr"), TypeConverter(typeof(TextBoxAutoCompleteSourceConverter))]
        public System.Windows.Forms.AutoCompleteSource AutoCompleteSource
        {
            get
            {
                return this.autoCompleteSource;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid_NotSequential(value, (int) value, new int[] { 0x80, 7, 6, 0x40, 1, 0x20, 2, 0x100, 4 }))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.AutoCompleteSource));
                }
                if (value == System.Windows.Forms.AutoCompleteSource.ListItems)
                {
                    throw new NotSupportedException(System.Windows.Forms.SR.GetString("TextBoxAutoCompleteSourceNoItems"));
                }
                if ((value != System.Windows.Forms.AutoCompleteSource.None) && (value != System.Windows.Forms.AutoCompleteSource.CustomSource))
                {
                    new FileIOPermission(PermissionState.Unrestricted) { AllFiles = FileIOPermissionAccess.PathDiscovery }.Demand();
                }
                this.autoCompleteSource = value;
                this.SetAutoComplete(false);
            }
        }

        [System.Windows.Forms.SRDescription("TextBoxCharacterCasingDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(0)]
        public System.Windows.Forms.CharacterCasing CharacterCasing
        {
            get
            {
                return this.characterCasing;
            }
            set
            {
                if (this.characterCasing != value)
                {
                    if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                    {
                        throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.CharacterCasing));
                    }
                    this.characterCasing = value;
                    base.RecreateHandle();
                }
            }
        }

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                switch (this.characterCasing)
                {
                    case System.Windows.Forms.CharacterCasing.Upper:
                        createParams.Style |= 8;
                        break;

                    case System.Windows.Forms.CharacterCasing.Lower:
                        createParams.Style |= 0x10;
                        break;
                }
                HorizontalAlignment alignment = base.RtlTranslateHorizontal(this.textAlign);
                createParams.ExStyle &= -4097;
                switch (alignment)
                {
                    case HorizontalAlignment.Left:
                        createParams.Style = createParams.Style;
                        break;

                    case HorizontalAlignment.Right:
                        createParams.Style |= 2;
                        break;

                    case HorizontalAlignment.Center:
                        createParams.Style |= 1;
                        break;
                }
                if (this.Multiline)
                {
                    if ((((this.scrollBars & System.Windows.Forms.ScrollBars.Horizontal) == System.Windows.Forms.ScrollBars.Horizontal) && (this.textAlign == HorizontalAlignment.Left)) && !base.WordWrap)
                    {
                        createParams.Style |= 0x100000;
                    }
                    if ((this.scrollBars & System.Windows.Forms.ScrollBars.Vertical) == System.Windows.Forms.ScrollBars.Vertical)
                    {
                        createParams.Style |= 0x200000;
                    }
                }
                if (this.useSystemPasswordChar)
                {
                    createParams.Style |= 0x20;
                }
                return createParams;
            }
        }

        public override bool Multiline
        {
            get
            {
                return base.Multiline;
            }
            set
            {
                if (this.Multiline != value)
                {
                    base.Multiline = value;
                    if (value && (this.AutoCompleteMode != System.Windows.Forms.AutoCompleteMode.None))
                    {
                        base.RecreateHandle();
                    }
                }
            }
        }

        [System.Windows.Forms.SRDescription("TextBoxPasswordCharDescr"), RefreshProperties(RefreshProperties.Repaint), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue('\0'), Localizable(true)]
        public char PasswordChar
        {
            get
            {
                if (!base.IsHandleCreated)
                {
                    this.CreateHandle();
                }
                return (char) ((int) base.SendMessage(210, 0, 0));
            }
            set
            {
                this.passwordChar = value;
                if ((!this.useSystemPasswordChar && base.IsHandleCreated) && (this.PasswordChar != value))
                {
                    base.SendMessage(0xcc, value, 0);
                    base.VerifyImeRestrictedModeChanged();
                    this.ResetAutoComplete(false);
                    base.Invalidate();
                }
            }
        }

        internal override bool PasswordProtect
        {
            get
            {
                return (this.PasswordChar != '\0');
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("TextBoxScrollBarsDescr"), DefaultValue(0), Localizable(true)]
        public System.Windows.Forms.ScrollBars ScrollBars
        {
            get
            {
                return this.scrollBars;
            }
            set
            {
                if (this.scrollBars != value)
                {
                    if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 3))
                    {
                        throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.ScrollBars));
                    }
                    this.scrollBars = value;
                    base.RecreateHandle();
                }
            }
        }

        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
                this.selectionSet = false;
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("TextBoxTextAlignDescr"), Localizable(true), DefaultValue(0)]
        public HorizontalAlignment TextAlign
        {
            get
            {
                return this.textAlign;
            }
            set
            {
                if (this.textAlign != value)
                {
                    if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                    {
                        throw new InvalidEnumArgumentException("value", (int) value, typeof(HorizontalAlignment));
                    }
                    this.textAlign = value;
                    base.RecreateHandle();
                    this.OnTextAlignChanged(EventArgs.Empty);
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("TextBoxUseSystemPasswordCharDescr"), RefreshProperties(RefreshProperties.Repaint), DefaultValue(false)]
        public bool UseSystemPasswordChar
        {
            get
            {
                return this.useSystemPasswordChar;
            }
            set
            {
                if (value != this.useSystemPasswordChar)
                {
                    this.useSystemPasswordChar = value;
                    base.RecreateHandle();
                    if (value)
                    {
                        this.ResetAutoComplete(false);
                    }
                }
            }
        }
    }
}

