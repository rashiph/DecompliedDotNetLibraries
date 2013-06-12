namespace System.Windows.Forms
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Media;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Windows.Forms.VisualStyles;

    [ComVisible(true), Designer("System.Windows.Forms.Design.MaskedTextBoxDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), System.Windows.Forms.SRDescription("DescriptionMaskedTextBox"), DefaultEvent("MaskInputRejected"), DefaultBindingProperty("Text"), DefaultProperty("Mask"), ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class MaskedTextBox : TextBoxBase
    {
        private const bool backward = false;
        private static int BEEP_ON_ERROR = BitVector32.CreateMask(HIDE_PROMPT_ON_LEAVE);
        private int caretTestPos;
        private static int CUTCOPYINCLUDELITERALS = BitVector32.CreateMask(CUTCOPYINCLUDEPROMPT);
        private static int CUTCOPYINCLUDEPROMPT = BitVector32.CreateMask(INSERT_TOGGLED);
        private static readonly object EVENT_ISOVERWRITEMODECHANGED = new object();
        private static readonly object EVENT_MASKCHANGED = new object();
        private static readonly object EVENT_MASKINPUTREJECTED = new object();
        private static readonly object EVENT_TEXTALIGNCHANGED = new object();
        private static readonly object EVENT_VALIDATIONCOMPLETED = new object();
        private BitVector32 flagState;
        private IFormatProvider formatProvider;
        private const bool forward = true;
        private static int HANDLE_KEY_PRESS = BitVector32.CreateMask(IME_COMPLETING);
        private static int HIDE_PROMPT_ON_LEAVE = BitVector32.CreateMask(REJECT_INPUT_ON_FIRST_FAILURE);
        private static int IME_COMPLETING = BitVector32.CreateMask(IME_ENDING_COMPOSITION);
        private static int IME_ENDING_COMPOSITION = BitVector32.CreateMask();
        private const byte imeConvertionCompleted = 2;
        private const byte imeConvertionNone = 0;
        private const byte imeConvertionUpdate = 1;
        private static int INSERT_TOGGLED = BitVector32.CreateMask(USE_SYSTEM_PASSWORD_CHAR);
        private System.Windows.Forms.InsertKeyMode insertMode;
        private static int IS_NULL_MASK = BitVector32.CreateMask(HANDLE_KEY_PRESS);
        private int lastSelLength;
        private System.ComponentModel.MaskedTextProvider maskedTextProvider;
        private const string nullMask = "<>";
        private char passwordChar;
        private static int QUERY_BASE_TEXT = BitVector32.CreateMask(IS_NULL_MASK);
        private static int REJECT_INPUT_ON_FIRST_FAILURE = BitVector32.CreateMask(QUERY_BASE_TEXT);
        private static char systemPwdChar;
        private HorizontalAlignment textAlign;
        private static int USE_SYSTEM_PASSWORD_CHAR = BitVector32.CreateMask(BEEP_ON_ERROR);
        private System.Type validatingType;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler AcceptsTabChanged
        {
            add
            {
            }
            remove
            {
            }
        }

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("MaskedTextBoxIsOverwriteModeChangedDescr")]
        public event EventHandler IsOverwriteModeChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_ISOVERWRITEMODECHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_ISOVERWRITEMODECHANGED, value);
            }
        }

        [System.Windows.Forms.SRDescription("MaskedTextBoxMaskChangedDescr"), System.Windows.Forms.SRCategory("CatPropertyChanged")]
        public event EventHandler MaskChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_MASKCHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_MASKCHANGED, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("MaskedTextBoxMaskInputRejectedDescr")]
        public event MaskInputRejectedEventHandler MaskInputRejected
        {
            add
            {
                base.Events.AddHandler(EVENT_MASKINPUTREJECTED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_MASKINPUTREJECTED, value);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler MultilineChanged
        {
            add
            {
            }
            remove
            {
            }
        }

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("RadioButtonOnTextAlignChangedDescr")]
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

        [System.Windows.Forms.SRDescription("MaskedTextBoxTypeValidationCompletedDescr"), System.Windows.Forms.SRCategory("CatFocus")]
        public event TypeValidationEventHandler TypeValidationCompleted
        {
            add
            {
                base.Events.AddHandler(EVENT_VALIDATIONCOMPLETED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_VALIDATIONCOMPLETED, value);
            }
        }

        public MaskedTextBox()
        {
            System.ComponentModel.MaskedTextProvider maskedTextProvider = new System.ComponentModel.MaskedTextProvider("<>", CultureInfo.CurrentCulture);
            this.flagState[IS_NULL_MASK] = true;
            this.Initialize(maskedTextProvider);
        }

        public MaskedTextBox(System.ComponentModel.MaskedTextProvider maskedTextProvider)
        {
            if (maskedTextProvider == null)
            {
                throw new ArgumentNullException();
            }
            this.flagState[IS_NULL_MASK] = false;
            this.Initialize(maskedTextProvider);
        }

        public MaskedTextBox(string mask)
        {
            if (mask == null)
            {
                throw new ArgumentNullException();
            }
            System.ComponentModel.MaskedTextProvider maskedTextProvider = new System.ComponentModel.MaskedTextProvider(mask, CultureInfo.CurrentCulture);
            this.flagState[IS_NULL_MASK] = false;
            this.Initialize(maskedTextProvider);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void ClearUndo()
        {
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), UIPermission(SecurityAction.InheritanceDemand, Window=UIPermissionWindow.AllWindows)]
        protected override void CreateHandle()
        {
            if (!this.flagState[IS_NULL_MASK] && base.RecreatingHandle)
            {
                this.SetWindowText(this.GetFormattedDisplayString(), false, false);
            }
            base.CreateHandle();
        }

        private void Delete(Keys keyCode, int startPosition, int selectionLen)
        {
            int num;
            MaskedTextResultHint hint;
            this.caretTestPos = startPosition;
            if (selectionLen == 0)
            {
                if (keyCode == Keys.Back)
                {
                    if (startPosition == 0)
                    {
                        return;
                    }
                    startPosition--;
                }
                else if ((startPosition + selectionLen) == this.maskedTextProvider.Length)
                {
                    return;
                }
            }
            int endPosition = (selectionLen > 0) ? ((startPosition + selectionLen) - 1) : startPosition;
            string textOutput = this.TextOutput;
            if (this.maskedTextProvider.RemoveAt(startPosition, endPosition, out num, out hint))
            {
                if (this.TextOutput != textOutput)
                {
                    this.SetText();
                    this.caretTestPos = startPosition;
                }
                else if (selectionLen > 0)
                {
                    this.caretTestPos = startPosition;
                }
                else if (hint == MaskedTextResultHint.NoEffect)
                {
                    if (keyCode == Keys.Delete)
                    {
                        this.caretTestPos = this.maskedTextProvider.FindEditPositionFrom(startPosition, true);
                    }
                    else
                    {
                        if (this.maskedTextProvider.FindAssignedEditPositionFrom(startPosition, true) == System.ComponentModel.MaskedTextProvider.InvalidIndex)
                        {
                            this.caretTestPos = this.maskedTextProvider.FindAssignedEditPositionFrom(startPosition, false);
                        }
                        else
                        {
                            this.caretTestPos = this.maskedTextProvider.FindEditPositionFrom(startPosition, false);
                        }
                        if (this.caretTestPos != System.ComponentModel.MaskedTextProvider.InvalidIndex)
                        {
                            this.caretTestPos++;
                        }
                    }
                    if (this.caretTestPos == System.ComponentModel.MaskedTextProvider.InvalidIndex)
                    {
                        this.caretTestPos = startPosition;
                    }
                }
                else if (keyCode == Keys.Back)
                {
                    this.caretTestPos = startPosition;
                }
            }
            else
            {
                this.OnMaskInputRejected(new MaskInputRejectedEventArgs(num, hint));
            }
            base.SelectInternal(this.caretTestPos, 0, this.maskedTextProvider.Length);
        }

        public override char GetCharFromPosition(Point pt)
        {
            char charFromPosition;
            this.flagState[QUERY_BASE_TEXT] = true;
            try
            {
                charFromPosition = base.GetCharFromPosition(pt);
            }
            finally
            {
                this.flagState[QUERY_BASE_TEXT] = false;
            }
            return charFromPosition;
        }

        public override int GetCharIndexFromPosition(Point pt)
        {
            int charIndexFromPosition;
            this.flagState[QUERY_BASE_TEXT] = true;
            try
            {
                charIndexFromPosition = base.GetCharIndexFromPosition(pt);
            }
            finally
            {
                this.flagState[QUERY_BASE_TEXT] = false;
            }
            return charIndexFromPosition;
        }

        internal override int GetEndPosition()
        {
            if (this.flagState[IS_NULL_MASK])
            {
                return base.GetEndPosition();
            }
            int num = this.maskedTextProvider.FindEditPositionFrom(this.maskedTextProvider.LastAssignedPosition + 1, true);
            if (num == System.ComponentModel.MaskedTextProvider.InvalidIndex)
            {
                num = this.maskedTextProvider.LastAssignedPosition + 1;
            }
            return num;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public int GetFirstCharIndexFromLine(int lineNumber)
        {
            return 0;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public int GetFirstCharIndexOfCurrentLine()
        {
            return 0;
        }

        private string GetFormattedDisplayString()
        {
            bool flag;
            if (this.ReadOnly)
            {
                flag = false;
            }
            else if (base.DesignMode)
            {
                flag = true;
            }
            else
            {
                flag = !this.HidePromptOnLeave || this.Focused;
            }
            return this.maskedTextProvider.ToString(false, flag, true, 0, this.maskedTextProvider.Length);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetLineFromCharIndex(int index)
        {
            return 0;
        }

        public override Point GetPositionFromCharIndex(int index)
        {
            Point positionFromCharIndex;
            this.flagState[QUERY_BASE_TEXT] = true;
            try
            {
                positionFromCharIndex = base.GetPositionFromCharIndex(index);
            }
            finally
            {
                this.flagState[QUERY_BASE_TEXT] = false;
            }
            return positionFromCharIndex;
        }

        internal override Size GetPreferredSizeCore(Size proposedConstraints)
        {
            Size preferredSizeCore;
            this.flagState[QUERY_BASE_TEXT] = true;
            try
            {
                preferredSizeCore = base.GetPreferredSizeCore(proposedConstraints);
            }
            finally
            {
                this.flagState[QUERY_BASE_TEXT] = false;
            }
            return preferredSizeCore;
        }

        private string GetSelectedText()
        {
            int num;
            int num2;
            base.GetSelectionStartAndLength(out num, out num2);
            if (num2 == 0)
            {
                return string.Empty;
            }
            bool includePrompt = (this.CutCopyMaskFormat & MaskFormat.IncludePrompt) != MaskFormat.ExcludePromptAndLiterals;
            bool includeLiterals = (this.CutCopyMaskFormat & MaskFormat.IncludeLiterals) != MaskFormat.ExcludePromptAndLiterals;
            return this.maskedTextProvider.ToString(true, includePrompt, includeLiterals, num, num2);
        }

        private void ImeComplete()
        {
            this.flagState[IME_COMPLETING] = true;
            this.ImeNotify(1);
        }

        private void ImeNotify(int action)
        {
            HandleRef hWnd = new HandleRef(this, base.Handle);
            IntPtr handle = System.Windows.Forms.UnsafeNativeMethods.ImmGetContext(hWnd);
            if (handle != IntPtr.Zero)
            {
                try
                {
                    System.Windows.Forms.UnsafeNativeMethods.ImmNotifyIME(new HandleRef(null, handle), 0x15, action, 0);
                }
                finally
                {
                    System.Windows.Forms.UnsafeNativeMethods.ImmReleaseContext(hWnd, new HandleRef(null, handle));
                }
            }
        }

        private void Initialize(System.ComponentModel.MaskedTextProvider maskedTextProvider)
        {
            this.maskedTextProvider = maskedTextProvider;
            if (!this.flagState[IS_NULL_MASK])
            {
                this.SetWindowText();
            }
            this.passwordChar = this.maskedTextProvider.PasswordChar;
            this.insertMode = System.Windows.Forms.InsertKeyMode.Default;
            this.flagState[HIDE_PROMPT_ON_LEAVE] = false;
            this.flagState[BEEP_ON_ERROR] = false;
            this.flagState[USE_SYSTEM_PASSWORD_CHAR] = false;
            this.flagState[REJECT_INPUT_ON_FIRST_FAILURE] = false;
            this.flagState[CUTCOPYINCLUDEPROMPT] = this.maskedTextProvider.IncludePrompt;
            this.flagState[CUTCOPYINCLUDELITERALS] = this.maskedTextProvider.IncludeLiterals;
            this.flagState[HANDLE_KEY_PRESS] = true;
            this.caretTestPos = 0;
        }

        protected override bool IsInputKey(Keys keyData)
        {
            if ((keyData & Keys.KeyCode) == Keys.Enter)
            {
                return false;
            }
            return base.IsInputKey(keyData);
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            if ((Application.RenderWithVisualStyles && base.IsHandleCreated) && (base.BorderStyle == BorderStyle.Fixed3D))
            {
                System.Windows.Forms.SafeNativeMethods.RedrawWindow(new HandleRef(this, base.Handle), (System.Windows.Forms.NativeMethods.COMRECT) null, System.Windows.Forms.NativeMethods.NullHandleRef, 0x401);
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            base.SetSelectionOnHandle();
            if (this.flagState[IS_NULL_MASK] && this.maskedTextProvider.IsPassword)
            {
                this.SetEditControlPasswordChar(this.maskedTextProvider.PasswordChar);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnIsOverwriteModeChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EVENT_ISOVERWRITEMODECHANGED] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (!this.flagState[IS_NULL_MASK])
            {
                Keys keyCode = e.KeyCode;
                switch (keyCode)
                {
                    case Keys.Enter:
                    case Keys.Escape:
                        this.flagState[HANDLE_KEY_PRESS] = false;
                        break;
                }
                if (((keyCode == Keys.Insert) && (e.Modifiers == Keys.None)) && (this.insertMode == System.Windows.Forms.InsertKeyMode.Default))
                {
                    this.flagState[INSERT_TOGGLED] = !this.flagState[INSERT_TOGGLED];
                    this.OnIsOverwriteModeChanged(EventArgs.Empty);
                }
                else
                {
                    if (e.Control && char.IsLetter((char) ((ushort) keyCode)))
                    {
                        switch (keyCode)
                        {
                            case Keys.H:
                                this.flagState[HANDLE_KEY_PRESS] = false;
                                return;
                        }
                        keyCode = Keys.Back;
                    }
                    if (((keyCode == Keys.Delete) || (keyCode == Keys.Back)) && !this.ReadOnly)
                    {
                        int num;
                        int num2;
                        base.GetSelectionStartAndLength(out num2, out num);
                        Keys modifiers = e.Modifiers;
                        if (modifiers == Keys.Shift)
                        {
                            if (keyCode == Keys.Delete)
                            {
                                keyCode = Keys.Back;
                            }
                        }
                        else if ((modifiers == Keys.Control) && (num == 0))
                        {
                            if (keyCode == Keys.Delete)
                            {
                                num = this.maskedTextProvider.Length - num2;
                            }
                            else
                            {
                                num = (num2 == this.maskedTextProvider.Length) ? num2 : (num2 + 1);
                                num2 = 0;
                            }
                        }
                        if (!this.flagState[HANDLE_KEY_PRESS])
                        {
                            this.flagState[HANDLE_KEY_PRESS] = true;
                        }
                        this.Delete(keyCode, num2, num);
                        e.SuppressKeyPress = true;
                    }
                }
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if (!this.flagState[IS_NULL_MASK])
            {
                if (!this.flagState[HANDLE_KEY_PRESS])
                {
                    this.flagState[HANDLE_KEY_PRESS] = true;
                    if (!char.IsLetter(e.KeyChar))
                    {
                        return;
                    }
                }
                if (!this.ReadOnly)
                {
                    MaskedTextResultHint hint;
                    int num;
                    int num2;
                    base.GetSelectionStartAndLength(out num, out num2);
                    string textOutput = this.TextOutput;
                    if (this.PlaceChar(e.KeyChar, num, num2, this.IsOverwriteMode, out hint))
                    {
                        if (this.TextOutput != textOutput)
                        {
                            this.SetText();
                        }
                        base.SelectionStart = ++this.caretTestPos;
                        if ((ImeModeConversion.InputLanguageTable == ImeModeConversion.KoreanTable) && (this.maskedTextProvider.FindUnassignedEditPositionFrom(this.caretTestPos, true) == System.ComponentModel.MaskedTextProvider.InvalidIndex))
                        {
                            this.ImeComplete();
                        }
                    }
                    else
                    {
                        this.OnMaskInputRejected(new MaskInputRejectedEventArgs(this.caretTestPos, hint));
                    }
                    if (num2 > 0)
                    {
                        this.SelectionLength = 0;
                    }
                    e.Handled = true;
                }
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (this.flagState[IME_COMPLETING])
            {
                this.flagState[IME_COMPLETING] = false;
            }
            if (this.flagState[IME_ENDING_COMPOSITION])
            {
                this.flagState[IME_ENDING_COMPOSITION] = false;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnMaskChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EVENT_MASKCHANGED] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnMaskInputRejected(MaskInputRejectedEventArgs e)
        {
            if (this.BeepOnError)
            {
                new SoundPlayer().Play();
            }
            MaskInputRejectedEventHandler handler = base.Events[EVENT_MASKINPUTREJECTED] as MaskInputRejectedEventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected override void OnMultilineChanged(EventArgs e)
        {
        }

        protected virtual void OnTextAlignChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EVENT_TEXTALIGNCHANGED] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            bool flag = this.flagState[QUERY_BASE_TEXT];
            this.flagState[QUERY_BASE_TEXT] = false;
            try
            {
                base.OnTextChanged(e);
            }
            finally
            {
                this.flagState[QUERY_BASE_TEXT] = flag;
            }
        }

        private void OnTypeValidationCompleted(TypeValidationEventArgs e)
        {
            TypeValidationEventHandler handler = base.Events[EVENT_VALIDATIONCOMPLETED] as TypeValidationEventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void OnValidating(CancelEventArgs e)
        {
            this.PerformTypeValidation(e);
            base.OnValidating(e);
        }

        private void PasteInt(string text)
        {
            int num;
            int num2;
            base.GetSelectionStartAndLength(out num, out num2);
            if (string.IsNullOrEmpty(text))
            {
                this.Delete(Keys.Delete, num, num2);
            }
            else
            {
                this.Replace(text, num, num2);
            }
        }

        private object PerformTypeValidation(CancelEventArgs e)
        {
            object returnValue = null;
            if (this.validatingType != null)
            {
                string message = null;
                if (!this.flagState[IS_NULL_MASK] && !this.maskedTextProvider.MaskCompleted)
                {
                    message = System.Windows.Forms.SR.GetString("MaskedTextBoxIncompleteMsg");
                }
                else
                {
                    string text;
                    if (!this.flagState[IS_NULL_MASK])
                    {
                        text = this.maskedTextProvider.ToString(false, this.IncludeLiterals);
                    }
                    else
                    {
                        text = base.Text;
                    }
                    try
                    {
                        returnValue = Formatter.ParseObject(text, this.validatingType, typeof(string), null, null, this.formatProvider, null, Formatter.GetDefaultDataSourceNullValue(this.validatingType));
                    }
                    catch (Exception innerException)
                    {
                        if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(innerException))
                        {
                            throw;
                        }
                        if (innerException.InnerException != null)
                        {
                            innerException = innerException.InnerException;
                        }
                        message = innerException.GetType().ToString() + ": " + innerException.Message;
                    }
                }
                bool isValidInput = false;
                if (message == null)
                {
                    isValidInput = true;
                    message = System.Windows.Forms.SR.GetString("MaskedTextBoxTypeValidationSucceeded");
                }
                TypeValidationEventArgs args = new TypeValidationEventArgs(this.validatingType, isValidInput, returnValue, message);
                this.OnTypeValidationCompleted(args);
                if (e != null)
                {
                    e.Cancel = args.Cancel;
                }
            }
            return returnValue;
        }

        private bool PlaceChar(char ch, int startPosition, int length, bool overwrite, out MaskedTextResultHint hint)
        {
            return this.PlaceChar(this.maskedTextProvider, ch, startPosition, length, overwrite, out hint);
        }

        private bool PlaceChar(System.ComponentModel.MaskedTextProvider provider, char ch, int startPosition, int length, bool overwrite, out MaskedTextResultHint hint)
        {
            this.caretTestPos = startPosition;
            if (startPosition < this.maskedTextProvider.Length)
            {
                if (length > 0)
                {
                    int endPosition = (startPosition + length) - 1;
                    return provider.Replace(ch, startPosition, endPosition, out this.caretTestPos, out hint);
                }
                if (overwrite)
                {
                    return provider.Replace(ch, startPosition, out this.caretTestPos, out hint);
                }
                return provider.InsertAt(ch, startPosition, out this.caretTestPos, out hint);
            }
            hint = MaskedTextResultHint.UnavailableEditPosition;
            return false;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            bool flag = base.ProcessCmdKey(ref msg, keyData);
            if (!flag && (keyData == (Keys.Control | Keys.A)))
            {
                base.SelectAll();
                flag = true;
            }
            return flag;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected internal override bool ProcessKeyMessage(ref Message m)
        {
            bool flag = base.ProcessKeyMessage(ref m);
            if (this.flagState[IS_NULL_MASK])
            {
                return flag;
            }
            return (((m.Msg == 0x102) && (base.ImeWmCharsToIgnore > 0)) || flag);
        }

        private void Replace(string text, int startPosition, int selectionLen)
        {
            System.ComponentModel.MaskedTextProvider provider = (System.ComponentModel.MaskedTextProvider) this.maskedTextProvider.Clone();
            int caretTestPos = this.caretTestPos;
            MaskedTextResultHint noEffect = MaskedTextResultHint.NoEffect;
            int endPosition = (startPosition + selectionLen) - 1;
            if (this.RejectInputOnFirstFailure)
            {
                if (!((startPosition > endPosition) ? provider.InsertAt(text, startPosition, out this.caretTestPos, out noEffect) : provider.Replace(text, startPosition, endPosition, out this.caretTestPos, out noEffect)))
                {
                    this.OnMaskInputRejected(new MaskInputRejectedEventArgs(this.caretTestPos, noEffect));
                }
            }
            else
            {
                MaskedTextResultHint hint = noEffect;
                foreach (char ch in text)
                {
                    if (!this.maskedTextProvider.VerifyEscapeChar(ch, startPosition))
                    {
                        int num3 = provider.FindEditPositionFrom(startPosition, true);
                        if (num3 == System.ComponentModel.MaskedTextProvider.InvalidIndex)
                        {
                            this.OnMaskInputRejected(new MaskInputRejectedEventArgs(startPosition, MaskedTextResultHint.UnavailableEditPosition));
                            continue;
                        }
                        startPosition = num3;
                    }
                    int length = (endPosition >= startPosition) ? 1 : 0;
                    bool overwrite = length > 0;
                    if (this.PlaceChar(provider, ch, startPosition, length, overwrite, out hint))
                    {
                        startPosition = this.caretTestPos + 1;
                        if ((hint == MaskedTextResultHint.Success) && (noEffect != hint))
                        {
                            noEffect = hint;
                        }
                    }
                    else
                    {
                        this.OnMaskInputRejected(new MaskInputRejectedEventArgs(startPosition, hint));
                    }
                }
                if ((selectionLen > 0) && (startPosition <= endPosition))
                {
                    if (!provider.RemoveAt(startPosition, endPosition, out this.caretTestPos, out hint))
                    {
                        this.OnMaskInputRejected(new MaskInputRejectedEventArgs(this.caretTestPos, hint));
                    }
                    if ((noEffect == MaskedTextResultHint.NoEffect) && (noEffect != hint))
                    {
                        noEffect = hint;
                    }
                }
            }
            bool flag3 = this.TextOutput != provider.ToString();
            this.maskedTextProvider = provider;
            if (flag3)
            {
                this.SetText();
                this.caretTestPos = startPosition;
                base.SelectInternal(this.caretTestPos, 0, this.maskedTextProvider.Length);
            }
            else
            {
                this.caretTestPos = caretTestPos;
            }
        }

        private void ResetCulture()
        {
            this.Culture = CultureInfo.CurrentCulture;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void ScrollToCaret()
        {
        }

        private void SetEditControlPasswordChar(char pwdChar)
        {
            if (base.IsHandleCreated)
            {
                base.SendMessage(0xcc, pwdChar, 0);
                base.Invalidate();
            }
        }

        private void SetMaskedTextProvider(System.ComponentModel.MaskedTextProvider newProvider)
        {
            this.SetMaskedTextProvider(newProvider, null);
        }

        private void SetMaskedTextProvider(System.ComponentModel.MaskedTextProvider newProvider, string textOnInitializingMask)
        {
            newProvider.IncludePrompt = this.maskedTextProvider.IncludePrompt;
            newProvider.IncludeLiterals = this.maskedTextProvider.IncludeLiterals;
            newProvider.SkipLiterals = this.maskedTextProvider.SkipLiterals;
            newProvider.ResetOnPrompt = this.maskedTextProvider.ResetOnPrompt;
            newProvider.ResetOnSpace = this.maskedTextProvider.ResetOnSpace;
            if (this.flagState[IS_NULL_MASK] && (textOnInitializingMask == null))
            {
                this.maskedTextProvider = newProvider;
            }
            else
            {
                string textOutput;
                int testPosition = 0;
                bool flag = false;
                MaskedTextResultHint noEffect = MaskedTextResultHint.NoEffect;
                System.ComponentModel.MaskedTextProvider maskedTextProvider = this.maskedTextProvider;
                bool preserveCaret = maskedTextProvider.Mask == newProvider.Mask;
                if (textOnInitializingMask != null)
                {
                    textOutput = textOnInitializingMask;
                    flag = !newProvider.Set(textOnInitializingMask, out testPosition, out noEffect);
                }
                else
                {
                    textOutput = this.TextOutput;
                    int assignedEditPositionCount = maskedTextProvider.AssignedEditPositionCount;
                    int position = 0;
                    int num4 = 0;
                    while (assignedEditPositionCount > 0)
                    {
                        position = maskedTextProvider.FindAssignedEditPositionFrom(position, true);
                        if (preserveCaret)
                        {
                            num4 = position;
                        }
                        else
                        {
                            num4 = newProvider.FindEditPositionFrom(num4, true);
                            if (num4 == System.ComponentModel.MaskedTextProvider.InvalidIndex)
                            {
                                newProvider.Clear();
                                testPosition = newProvider.Length;
                                noEffect = MaskedTextResultHint.UnavailableEditPosition;
                                break;
                            }
                        }
                        if (!newProvider.Replace(maskedTextProvider[position], num4, out testPosition, out noEffect))
                        {
                            preserveCaret = false;
                            newProvider.Clear();
                            break;
                        }
                        position++;
                        num4++;
                        assignedEditPositionCount--;
                    }
                    flag = !System.ComponentModel.MaskedTextProvider.GetOperationResultFromHint(noEffect);
                }
                this.maskedTextProvider = newProvider;
                if (this.flagState[IS_NULL_MASK])
                {
                    this.flagState[IS_NULL_MASK] = false;
                }
                if (flag)
                {
                    this.OnMaskInputRejected(new MaskInputRejectedEventArgs(testPosition, noEffect));
                }
                if (newProvider.IsPassword)
                {
                    this.SetEditControlPasswordChar('\0');
                }
                EventArgs empty = EventArgs.Empty;
                if ((textOnInitializingMask != null) || (maskedTextProvider.Mask != newProvider.Mask))
                {
                    this.OnMaskChanged(empty);
                }
                this.SetWindowText(this.GetFormattedDisplayString(), textOutput != this.TextOutput, preserveCaret);
            }
        }

        internal override void SetSelectedTextInternal(string value, bool clearUndo)
        {
            if (this.flagState[IS_NULL_MASK])
            {
                base.SetSelectedTextInternal(value, true);
            }
            else
            {
                this.PasteInt(value);
            }
        }

        private void SetText()
        {
            this.SetWindowText(this.GetFormattedDisplayString(), true, false);
        }

        private void SetWindowText()
        {
            this.SetWindowText(this.GetFormattedDisplayString(), false, true);
        }

        private void SetWindowText(string text, bool raiseTextChangedEvent, bool preserveCaret)
        {
            this.flagState[QUERY_BASE_TEXT] = true;
            try
            {
                if (preserveCaret)
                {
                    this.caretTestPos = base.SelectionStart;
                }
                this.WindowText = text;
                if (raiseTextChangedEvent)
                {
                    this.OnTextChanged(EventArgs.Empty);
                }
                if (preserveCaret)
                {
                    base.SelectionStart = this.caretTestPos;
                }
            }
            finally
            {
                this.flagState[QUERY_BASE_TEXT] = false;
            }
        }

        private bool ShouldSerializeCulture()
        {
            return !CultureInfo.CurrentCulture.Equals(this.Culture);
        }

        public override string ToString()
        {
            string str;
            if (this.flagState[IS_NULL_MASK])
            {
                return base.ToString();
            }
            bool includePrompt = this.IncludePrompt;
            bool includeLiterals = this.IncludeLiterals;
            try
            {
                this.IncludePrompt = this.IncludeLiterals = true;
                str = base.ToString();
            }
            finally
            {
                this.IncludePrompt = includePrompt;
                this.IncludeLiterals = includeLiterals;
            }
            return str;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Undo()
        {
        }

        public object ValidateText()
        {
            return this.PerformTypeValidation(null);
        }

        private bool WmClear()
        {
            if (!this.ReadOnly)
            {
                int num;
                int num2;
                base.GetSelectionStartAndLength(out num, out num2);
                this.Delete(Keys.Delete, num, num2);
                return true;
            }
            return false;
        }

        private bool WmCopy()
        {
            if (this.maskedTextProvider.IsPassword)
            {
                return false;
            }
            string selectedText = this.GetSelectedText();
            try
            {
                System.Windows.Forms.IntSecurity.ClipboardWrite.Assert();
                if (selectedText.Length == 0)
                {
                    Clipboard.Clear();
                }
                else
                {
                    Clipboard.SetText(selectedText);
                }
            }
            catch (Exception exception)
            {
                if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception))
                {
                    throw;
                }
            }
            return true;
        }

        private bool WmImeComposition(ref Message m)
        {
            if (ImeModeConversion.InputLanguageTable == ImeModeConversion.KoreanTable)
            {
                byte num = 0;
                if ((m.LParam.ToInt32() & 8) != 0)
                {
                    num = 1;
                }
                else if ((m.LParam.ToInt32() & 0x800) != 0)
                {
                    num = 2;
                }
                if ((num != 0) && this.flagState[IME_ENDING_COMPOSITION])
                {
                    return this.flagState[IME_COMPLETING];
                }
            }
            return false;
        }

        private bool WmImeStartComposition()
        {
            int num;
            int num2;
            base.GetSelectionStartAndLength(out num, out num2);
            int startPosition = this.maskedTextProvider.FindEditPositionFrom(num, true);
            if (startPosition != System.ComponentModel.MaskedTextProvider.InvalidIndex)
            {
                if ((num2 > 0) && (ImeModeConversion.InputLanguageTable == ImeModeConversion.KoreanTable))
                {
                    int num4 = this.maskedTextProvider.FindEditPositionFrom((num + num2) - 1, false);
                    if (num4 < startPosition)
                    {
                        this.ImeComplete();
                        this.OnMaskInputRejected(new MaskInputRejectedEventArgs(num, MaskedTextResultHint.UnavailableEditPosition));
                        return true;
                    }
                    num2 = (num4 - startPosition) + 1;
                    this.Delete(Keys.Delete, startPosition, num2);
                }
                if (num != startPosition)
                {
                    this.caretTestPos = startPosition;
                    base.SelectionStart = this.caretTestPos;
                }
                this.SelectionLength = 0;
                return false;
            }
            this.ImeComplete();
            this.OnMaskInputRejected(new MaskInputRejectedEventArgs(num, MaskedTextResultHint.UnavailableEditPosition));
            return true;
        }

        private void WmKillFocus()
        {
            base.GetSelectionStartAndLength(out this.caretTestPos, out this.lastSelLength);
            if (this.HidePromptOnLeave && !this.MaskFull)
            {
                this.SetWindowText();
                base.SelectInternal(this.caretTestPos, this.lastSelLength, this.maskedTextProvider.Length);
            }
        }

        private void WmPaste()
        {
            if (!this.ReadOnly)
            {
                string text;
                try
                {
                    System.Windows.Forms.IntSecurity.ClipboardRead.Assert();
                    text = Clipboard.GetText();
                }
                catch (Exception exception)
                {
                    if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception))
                    {
                        throw;
                    }
                    return;
                }
                this.PasteInt(text);
            }
        }

        private void WmPrint(ref Message m)
        {
            base.WndProc(ref m);
            if ((((2 & ((int) ((long) m.LParam))) != 0) && Application.RenderWithVisualStyles) && (base.BorderStyle == BorderStyle.Fixed3D))
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

        private void WmSetFocus()
        {
            if (this.HidePromptOnLeave && !this.MaskFull)
            {
                this.SetWindowText();
            }
            base.SelectInternal(this.caretTestPos, this.lastSelLength, this.maskedTextProvider.Length);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            int msg = m.Msg;
            if (msg <= 0xb7)
            {
                switch (msg)
                {
                    case 0x7b:
                        goto Label_004E;

                    case 0xb7:
                        return;
                }
                goto Label_005C;
            }
            switch (msg)
            {
                case 0xc5:
                case 0xc7:
                case 0x304:
                    return;

                case 0xc6:
                    break;

                default:
                    if (msg != 0x317)
                    {
                        goto Label_005C;
                    }
                    this.WmPrint(ref m);
                    return;
            }
        Label_004E:
            base.ClearUndo();
            base.WndProc(ref m);
            return;
        Label_005C:
            if (this.flagState[IS_NULL_MASK])
            {
                base.WndProc(ref m);
            }
            else
            {
                switch (m.Msg)
                {
                    case 7:
                        this.WmSetFocus();
                        base.WndProc(ref m);
                        return;

                    case 8:
                        base.WndProc(ref m);
                        this.WmKillFocus();
                        return;

                    case 0x10d:
                        if (!this.WmImeStartComposition())
                        {
                            break;
                        }
                        return;

                    case 270:
                        this.flagState[IME_ENDING_COMPOSITION] = true;
                        break;

                    case 0x10f:
                        if (!this.WmImeComposition(ref m))
                        {
                            break;
                        }
                        return;

                    case 0x300:
                        if (!this.ReadOnly && this.WmCopy())
                        {
                            this.WmClear();
                        }
                        return;

                    case 0x301:
                        this.WmCopy();
                        return;

                    case 770:
                        this.WmPaste();
                        return;

                    case 0x303:
                        this.WmClear();
                        return;
                }
                base.WndProc(ref m);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public bool AcceptsTab
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(true), System.Windows.Forms.SRDescription("MaskedTextBoxAllowPromptAsInputDescr")]
        public bool AllowPromptAsInput
        {
            get
            {
                return this.maskedTextProvider.AllowPromptAsInput;
            }
            set
            {
                if (value != this.maskedTextProvider.AllowPromptAsInput)
                {
                    System.ComponentModel.MaskedTextProvider newProvider = new System.ComponentModel.MaskedTextProvider(this.maskedTextProvider.Mask, this.maskedTextProvider.Culture, value, this.maskedTextProvider.PromptChar, this.maskedTextProvider.PasswordChar, this.maskedTextProvider.AsciiOnly);
                    this.SetMaskedTextProvider(newProvider);
                }
            }
        }

        [DefaultValue(false), System.Windows.Forms.SRDescription("MaskedTextBoxAsciiOnlyDescr"), RefreshProperties(RefreshProperties.Repaint), System.Windows.Forms.SRCategory("CatBehavior")]
        public bool AsciiOnly
        {
            get
            {
                return this.maskedTextProvider.AsciiOnly;
            }
            set
            {
                if (value != this.maskedTextProvider.AsciiOnly)
                {
                    System.ComponentModel.MaskedTextProvider newProvider = new System.ComponentModel.MaskedTextProvider(this.maskedTextProvider.Mask, this.maskedTextProvider.Culture, this.maskedTextProvider.AllowPromptAsInput, this.maskedTextProvider.PromptChar, this.maskedTextProvider.PasswordChar, value);
                    this.SetMaskedTextProvider(newProvider);
                }
            }
        }

        [System.Windows.Forms.SRDescription("MaskedTextBoxBeepOnErrorDescr"), DefaultValue(false), System.Windows.Forms.SRCategory("CatBehavior")]
        public bool BeepOnError
        {
            get
            {
                return this.flagState[BEEP_ON_ERROR];
            }
            set
            {
                this.flagState[BEEP_ON_ERROR] = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public bool CanUndo
        {
            get
            {
                return false;
            }
        }

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                HorizontalAlignment alignment = base.RtlTranslateHorizontal(this.textAlign);
                createParams.ExStyle &= -4097;
                switch (alignment)
                {
                    case HorizontalAlignment.Left:
                        createParams.Style = createParams.Style;
                        return createParams;

                    case HorizontalAlignment.Right:
                        createParams.Style |= 2;
                        return createParams;

                    case HorizontalAlignment.Center:
                        createParams.Style |= 1;
                        return createParams;
                }
                return createParams;
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("MaskedTextBoxCultureDescr"), RefreshProperties(RefreshProperties.Repaint)]
        public CultureInfo Culture
        {
            get
            {
                return this.maskedTextProvider.Culture;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }
                if (!this.maskedTextProvider.Culture.Equals(value))
                {
                    System.ComponentModel.MaskedTextProvider newProvider = new System.ComponentModel.MaskedTextProvider(this.maskedTextProvider.Mask, value, this.maskedTextProvider.AllowPromptAsInput, this.maskedTextProvider.PromptChar, this.maskedTextProvider.PasswordChar, this.maskedTextProvider.AsciiOnly);
                    this.SetMaskedTextProvider(newProvider);
                }
            }
        }

        [RefreshProperties(RefreshProperties.Repaint), System.Windows.Forms.SRDescription("MaskedTextBoxCutCopyMaskFormat"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(2)]
        public MaskFormat CutCopyMaskFormat
        {
            get
            {
                if (this.flagState[CUTCOPYINCLUDEPROMPT])
                {
                    if (this.flagState[CUTCOPYINCLUDELITERALS])
                    {
                        return MaskFormat.IncludePromptAndLiterals;
                    }
                    return MaskFormat.IncludePrompt;
                }
                if (this.flagState[CUTCOPYINCLUDELITERALS])
                {
                    return MaskFormat.IncludeLiterals;
                }
                return MaskFormat.ExcludePromptAndLiterals;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 3))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(MaskFormat));
                }
                if (value == MaskFormat.IncludePrompt)
                {
                    this.flagState[CUTCOPYINCLUDEPROMPT] = true;
                    this.flagState[CUTCOPYINCLUDELITERALS] = false;
                }
                else if (value == MaskFormat.IncludeLiterals)
                {
                    this.flagState[CUTCOPYINCLUDEPROMPT] = false;
                    this.flagState[CUTCOPYINCLUDELITERALS] = true;
                }
                else
                {
                    bool flag = value == MaskFormat.IncludePromptAndLiterals;
                    this.flagState[CUTCOPYINCLUDEPROMPT] = flag;
                    this.flagState[CUTCOPYINCLUDELITERALS] = flag;
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IFormatProvider FormatProvider
        {
            get
            {
                return this.formatProvider;
            }
            set
            {
                this.formatProvider = value;
            }
        }

        [RefreshProperties(RefreshProperties.Repaint), System.Windows.Forms.SRDescription("MaskedTextBoxHidePromptOnLeaveDescr"), DefaultValue(false), System.Windows.Forms.SRCategory("CatBehavior")]
        public bool HidePromptOnLeave
        {
            get
            {
                return this.flagState[HIDE_PROMPT_ON_LEAVE];
            }
            set
            {
                if (this.flagState[HIDE_PROMPT_ON_LEAVE] != value)
                {
                    this.flagState[HIDE_PROMPT_ON_LEAVE] = value;
                    if ((!this.flagState[IS_NULL_MASK] && !this.Focused) && (!this.MaskFull && !base.DesignMode))
                    {
                        this.SetWindowText();
                    }
                }
            }
        }

        private bool IncludeLiterals
        {
            get
            {
                return this.maskedTextProvider.IncludeLiterals;
            }
            set
            {
                this.maskedTextProvider.IncludeLiterals = value;
            }
        }

        private bool IncludePrompt
        {
            get
            {
                return this.maskedTextProvider.IncludePrompt;
            }
            set
            {
                this.maskedTextProvider.IncludePrompt = value;
            }
        }

        [System.Windows.Forms.SRDescription("MaskedTextBoxInsertKeyModeDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(0)]
        public System.Windows.Forms.InsertKeyMode InsertKeyMode
        {
            get
            {
                return this.insertMode;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.InsertKeyMode));
                }
                if (this.insertMode != value)
                {
                    bool isOverwriteMode = this.IsOverwriteMode;
                    this.insertMode = value;
                    if (isOverwriteMode != this.IsOverwriteMode)
                    {
                        this.OnIsOverwriteModeChanged(EventArgs.Empty);
                    }
                }
            }
        }

        [Browsable(false)]
        public bool IsOverwriteMode
        {
            get
            {
                if (!this.flagState[IS_NULL_MASK])
                {
                    switch (this.insertMode)
                    {
                        case System.Windows.Forms.InsertKeyMode.Default:
                            return this.flagState[INSERT_TOGGLED];

                        case System.Windows.Forms.InsertKeyMode.Insert:
                            return false;

                        case System.Windows.Forms.InsertKeyMode.Overwrite:
                            return true;
                    }
                }
                return false;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public string[] Lines
        {
            get
            {
                string[] lines;
                this.flagState[QUERY_BASE_TEXT] = true;
                try
                {
                    lines = base.Lines;
                }
                finally
                {
                    this.flagState[QUERY_BASE_TEXT] = false;
                }
                return lines;
            }
            set
            {
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), RefreshProperties(RefreshProperties.Repaint), Localizable(true), System.Windows.Forms.SRDescription("MaskedTextBoxMaskDescr"), Editor("System.Windows.Forms.Design.MaskPropertyEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), DefaultValue(""), MergableProperty(false)]
        public string Mask
        {
            get
            {
                if (!this.flagState[IS_NULL_MASK])
                {
                    return this.maskedTextProvider.Mask;
                }
                return string.Empty;
            }
            set
            {
                if ((this.flagState[IS_NULL_MASK] != string.IsNullOrEmpty(value)) || (!this.flagState[IS_NULL_MASK] && (value != this.maskedTextProvider.Mask)))
                {
                    string textOnInitializingMask = null;
                    string mask = value;
                    if (string.IsNullOrEmpty(value))
                    {
                        string textOutput = this.TextOutput;
                        string text = this.maskedTextProvider.ToString(false, false);
                        this.flagState[IS_NULL_MASK] = true;
                        if (this.maskedTextProvider.IsPassword)
                        {
                            this.SetEditControlPasswordChar(this.maskedTextProvider.PasswordChar);
                        }
                        this.SetWindowText(text, false, false);
                        EventArgs empty = EventArgs.Empty;
                        this.OnMaskChanged(empty);
                        if (text != textOutput)
                        {
                            this.OnTextChanged(empty);
                        }
                        mask = "<>";
                    }
                    else
                    {
                        foreach (char ch in value)
                        {
                            if (!System.ComponentModel.MaskedTextProvider.IsValidMaskChar(ch))
                            {
                                throw new ArgumentException(System.Windows.Forms.SR.GetString("MaskedTextBoxMaskInvalidChar"));
                            }
                        }
                        if (this.flagState[IS_NULL_MASK])
                        {
                            textOnInitializingMask = this.Text;
                        }
                    }
                    System.ComponentModel.MaskedTextProvider newProvider = new System.ComponentModel.MaskedTextProvider(mask, this.maskedTextProvider.Culture, this.maskedTextProvider.AllowPromptAsInput, this.maskedTextProvider.PromptChar, this.maskedTextProvider.PasswordChar, this.maskedTextProvider.AsciiOnly);
                    this.SetMaskedTextProvider(newProvider, textOnInitializingMask);
                }
            }
        }

        [Browsable(false)]
        public bool MaskCompleted
        {
            get
            {
                return this.maskedTextProvider.MaskCompleted;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public System.ComponentModel.MaskedTextProvider MaskedTextProvider
        {
            get
            {
                if (!this.flagState[IS_NULL_MASK])
                {
                    return (System.ComponentModel.MaskedTextProvider) this.maskedTextProvider.Clone();
                }
                return null;
            }
        }

        [Browsable(false)]
        public bool MaskFull
        {
            get
            {
                return this.maskedTextProvider.MaskFull;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public override int MaxLength
        {
            get
            {
                return base.MaxLength;
            }
            set
            {
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public override bool Multiline
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        [RefreshProperties(RefreshProperties.Repaint), System.Windows.Forms.SRDescription("MaskedTextBoxPasswordCharDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue('\0')]
        public char PasswordChar
        {
            get
            {
                return this.maskedTextProvider.PasswordChar;
            }
            set
            {
                if (!System.ComponentModel.MaskedTextProvider.IsValidPasswordChar(value))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("MaskedTextBoxInvalidCharError"));
                }
                if (this.passwordChar != value)
                {
                    if (value == this.maskedTextProvider.PromptChar)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("MaskedTextBoxPasswordAndPromptCharError"));
                    }
                    this.passwordChar = value;
                    if (!this.UseSystemPasswordChar)
                    {
                        this.maskedTextProvider.PasswordChar = value;
                        if (this.flagState[IS_NULL_MASK])
                        {
                            this.SetEditControlPasswordChar(value);
                        }
                        else
                        {
                            this.SetWindowText();
                        }
                        base.VerifyImeRestrictedModeChanged();
                    }
                }
            }
        }

        internal override bool PasswordProtect
        {
            get
            {
                if (this.maskedTextProvider != null)
                {
                    return this.maskedTextProvider.IsPassword;
                }
                return base.PasswordProtect;
            }
        }

        [Localizable(true), RefreshProperties(RefreshProperties.Repaint), DefaultValue('_'), System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("MaskedTextBoxPromptCharDescr")]
        public char PromptChar
        {
            get
            {
                return this.maskedTextProvider.PromptChar;
            }
            set
            {
                if (!System.ComponentModel.MaskedTextProvider.IsValidInputChar(value))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("MaskedTextBoxInvalidCharError"));
                }
                if (this.maskedTextProvider.PromptChar != value)
                {
                    if ((value == this.passwordChar) || (value == this.maskedTextProvider.PasswordChar))
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("MaskedTextBoxPasswordAndPromptCharError"));
                    }
                    System.ComponentModel.MaskedTextProvider newProvider = new System.ComponentModel.MaskedTextProvider(this.maskedTextProvider.Mask, this.maskedTextProvider.Culture, this.maskedTextProvider.AllowPromptAsInput, value, this.maskedTextProvider.PasswordChar, this.maskedTextProvider.AsciiOnly);
                    this.SetMaskedTextProvider(newProvider);
                }
            }
        }

        public bool ReadOnly
        {
            get
            {
                return base.ReadOnly;
            }
            set
            {
                if (this.ReadOnly != value)
                {
                    base.ReadOnly = value;
                    if (!this.flagState[IS_NULL_MASK])
                    {
                        this.SetWindowText();
                    }
                }
            }
        }

        [DefaultValue(false), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("MaskedTextBoxRejectInputOnFirstFailureDescr")]
        public bool RejectInputOnFirstFailure
        {
            get
            {
                return this.flagState[REJECT_INPUT_ON_FIRST_FAILURE];
            }
            set
            {
                this.flagState[REJECT_INPUT_ON_FIRST_FAILURE] = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("MaskedTextBoxResetOnPrompt"), DefaultValue(true)]
        public bool ResetOnPrompt
        {
            get
            {
                return this.maskedTextProvider.ResetOnPrompt;
            }
            set
            {
                this.maskedTextProvider.ResetOnPrompt = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(true), System.Windows.Forms.SRDescription("MaskedTextBoxResetOnSpace")]
        public bool ResetOnSpace
        {
            get
            {
                return this.maskedTextProvider.ResetOnSpace;
            }
            set
            {
                this.maskedTextProvider.ResetOnSpace = value;
            }
        }

        public override string SelectedText
        {
            get
            {
                if (this.flagState[IS_NULL_MASK])
                {
                    return base.SelectedText;
                }
                return this.GetSelectedText();
            }
            set
            {
                this.SetSelectedTextInternal(value, true);
            }
        }

        [System.Windows.Forms.SRDescription("MaskedTextBoxSkipLiterals"), DefaultValue(true), System.Windows.Forms.SRCategory("CatBehavior")]
        public bool SkipLiterals
        {
            get
            {
                return this.maskedTextProvider.SkipLiterals;
            }
            set
            {
                this.maskedTextProvider.SkipLiterals = value;
            }
        }

        private char SystemPasswordChar
        {
            get
            {
                if (systemPwdChar == '\0')
                {
                    System.Windows.Forms.TextBox box = new System.Windows.Forms.TextBox {
                        UseSystemPasswordChar = true
                    };
                    systemPwdChar = box.PasswordChar;
                    box.Dispose();
                }
                return systemPwdChar;
            }
        }

        [Editor("System.Windows.Forms.Design.MaskedTextBoxTextEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), System.Windows.Forms.SRCategory("CatAppearance"), Localizable(true), RefreshProperties(RefreshProperties.Repaint), Bindable(true), DefaultValue("")]
        public override string Text
        {
            get
            {
                if (!this.flagState[IS_NULL_MASK] && !this.flagState[QUERY_BASE_TEXT])
                {
                    return this.TextOutput;
                }
                return base.Text;
            }
            set
            {
                if (this.flagState[IS_NULL_MASK])
                {
                    base.Text = value;
                }
                else if (string.IsNullOrEmpty(value))
                {
                    this.Delete(Keys.Delete, 0, this.maskedTextProvider.Length);
                }
                else if (this.RejectInputOnFirstFailure)
                {
                    MaskedTextResultHint hint;
                    string textOutput = this.TextOutput;
                    if (this.maskedTextProvider.Set(value, out this.caretTestPos, out hint))
                    {
                        if (this.TextOutput != textOutput)
                        {
                            this.SetText();
                        }
                        base.SelectionStart = ++this.caretTestPos;
                    }
                    else
                    {
                        this.OnMaskInputRejected(new MaskInputRejectedEventArgs(this.caretTestPos, hint));
                    }
                }
                else
                {
                    this.Replace(value, 0, this.maskedTextProvider.Length);
                }
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

        [Browsable(false)]
        public override int TextLength
        {
            get
            {
                if (this.flagState[IS_NULL_MASK])
                {
                    return base.TextLength;
                }
                return this.GetFormattedDisplayString().Length;
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), RefreshProperties(RefreshProperties.Repaint), DefaultValue(2), System.Windows.Forms.SRDescription("MaskedTextBoxTextMaskFormat")]
        public MaskFormat TextMaskFormat
        {
            get
            {
                if (this.IncludePrompt)
                {
                    if (this.IncludeLiterals)
                    {
                        return MaskFormat.IncludePromptAndLiterals;
                    }
                    return MaskFormat.IncludePrompt;
                }
                if (this.IncludeLiterals)
                {
                    return MaskFormat.IncludeLiterals;
                }
                return MaskFormat.ExcludePromptAndLiterals;
            }
            set
            {
                if (this.TextMaskFormat != value)
                {
                    if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 3))
                    {
                        throw new InvalidEnumArgumentException("value", (int) value, typeof(MaskFormat));
                    }
                    string str = this.flagState[IS_NULL_MASK] ? null : this.TextOutput;
                    if (value == MaskFormat.IncludePrompt)
                    {
                        this.IncludePrompt = true;
                        this.IncludeLiterals = false;
                    }
                    else if (value == MaskFormat.IncludeLiterals)
                    {
                        this.IncludePrompt = false;
                        this.IncludeLiterals = true;
                    }
                    else
                    {
                        bool flag = value == MaskFormat.IncludePromptAndLiterals;
                        this.IncludePrompt = flag;
                        this.IncludeLiterals = flag;
                    }
                    if ((str != null) && (str != this.TextOutput))
                    {
                        this.OnTextChanged(EventArgs.Empty);
                    }
                }
            }
        }

        private string TextOutput
        {
            get
            {
                return this.maskedTextProvider.ToString();
            }
        }

        [DefaultValue(false), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("MaskedTextBoxUseSystemPasswordCharDescr"), RefreshProperties(RefreshProperties.Repaint)]
        public bool UseSystemPasswordChar
        {
            get
            {
                return this.flagState[USE_SYSTEM_PASSWORD_CHAR];
            }
            set
            {
                if (value != this.flagState[USE_SYSTEM_PASSWORD_CHAR])
                {
                    if (value)
                    {
                        if (this.SystemPasswordChar == this.PromptChar)
                        {
                            throw new InvalidOperationException(System.Windows.Forms.SR.GetString("MaskedTextBoxPasswordAndPromptCharError"));
                        }
                        this.maskedTextProvider.PasswordChar = this.SystemPasswordChar;
                    }
                    else
                    {
                        this.maskedTextProvider.PasswordChar = this.passwordChar;
                    }
                    this.flagState[USE_SYSTEM_PASSWORD_CHAR] = value;
                    if (this.flagState[IS_NULL_MASK])
                    {
                        this.SetEditControlPasswordChar(this.maskedTextProvider.PasswordChar);
                    }
                    else
                    {
                        this.SetWindowText();
                    }
                    base.VerifyImeRestrictedModeChanged();
                }
            }
        }

        [DefaultValue((string) null), Browsable(false)]
        public System.Type ValidatingType
        {
            get
            {
                return this.validatingType;
            }
            set
            {
                if (this.validatingType != value)
                {
                    this.validatingType = value;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public bool WordWrap
        {
            get
            {
                return false;
            }
            set
            {
            }
        }
    }
}

