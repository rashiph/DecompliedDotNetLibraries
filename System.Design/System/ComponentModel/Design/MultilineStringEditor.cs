namespace System.ComponentModel.Design
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Design;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Security.Permissions;
    using System.Text;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    public sealed class MultilineStringEditor : UITypeEditor
    {
        private MultilineStringEditorUI _editorUI;

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider != null)
            {
                IWindowsFormsEditorService editorService = (IWindowsFormsEditorService) provider.GetService(typeof(IWindowsFormsEditorService));
                if (editorService == null)
                {
                    return value;
                }
                if (this._editorUI == null)
                {
                    this._editorUI = new MultilineStringEditorUI();
                }
                this._editorUI.BeginEdit(editorService, value);
                editorService.DropDownControl(this._editorUI);
                object obj2 = this._editorUI.Value;
                if (this._editorUI.EndEdit())
                {
                    value = obj2;
                }
            }
            return value;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return false;
        }

        private class MultilineStringEditorUI : RichTextBox
        {
            private const int _caretPadding = 3;
            private bool _contentsResizedRaised;
            private bool _ctrlEnterPressed;
            private bool _editing;
            private IWindowsFormsEditorService _editorService;
            private bool _escapePressed;
            private Hashtable _fallbackFonts;
            private bool _firstTimeResizeToContent = true;
            private Size _minimumSize = Size.Empty;
            private SolidBrush _watermarkBrush;
            private readonly StringFormat _watermarkFormat;
            private Size _watermarkSize = Size.Empty;
            private const int _workAreaPadding = 0x10;

            internal MultilineStringEditorUI()
            {
                this.InitializeComponent();
                this._watermarkFormat = new StringFormat();
                this._watermarkFormat.Alignment = StringAlignment.Center;
                this._watermarkFormat.LineAlignment = StringAlignment.Center;
                this._fallbackFonts = new Hashtable(2);
            }

            internal void BeginEdit(IWindowsFormsEditorService editorService, object value)
            {
                this._editing = true;
                this._editorService = editorService;
                this._minimumSize = Size.Empty;
                this._watermarkSize = Size.Empty;
                this._escapePressed = false;
                this._ctrlEnterPressed = false;
                this.Text = (string) value;
            }

            [SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            protected override object CreateRichEditOleCallback()
            {
                return new MultilineStringEditor.OleCallback(this);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing && (this._watermarkBrush != null))
                {
                    this._watermarkBrush.Dispose();
                    this._watermarkBrush = null;
                }
                base.Dispose(disposing);
            }

            internal bool EndEdit()
            {
                this._editing = false;
                this._editorService = null;
                this._ctrlEnterPressed = false;
                this.Text = null;
                return !this._escapePressed;
            }

            private void InitializeComponent()
            {
                base.RichTextShortcutsEnabled = false;
                base.WordWrap = false;
                base.BorderStyle = BorderStyle.None;
                this.Multiline = true;
                base.ScrollBars = RichTextBoxScrollBars.Both;
                base.DetectUrls = false;
            }

            protected override bool IsInputKey(Keys keyData)
            {
                return (((((keyData & Keys.KeyCode) == Keys.Enter) && this.Multiline) && ((keyData & Keys.Alt) == Keys.None)) || base.IsInputKey(keyData));
            }

            protected override void OnContentsResized(ContentsResizedEventArgs e)
            {
                this._contentsResizedRaised = true;
                this.ResizeToContent();
                base.OnContentsResized(e);
            }

            protected override void OnKeyDown(KeyEventArgs e)
            {
                if (this.ShouldShowWatermark)
                {
                    base.Invalidate();
                }
                if ((e.Control && (e.KeyCode == Keys.Enter)) && (e.Modifiers == Keys.Control))
                {
                    this._editorService.CloseDropDown();
                    this._ctrlEnterPressed = true;
                }
            }

            protected override void OnTextChanged(EventArgs e)
            {
                if (!this._contentsResizedRaised)
                {
                    this.ResizeToContent();
                }
                this._contentsResizedRaised = false;
                base.OnTextChanged(e);
            }

            protected override void OnVisibleChanged(EventArgs e)
            {
                if (base.Visible)
                {
                    this.ProcessSurrogateFonts(0, this.Text.Length);
                    base.Select(this.Text.Length, 0);
                }
                this.ResizeToContent();
                base.OnVisibleChanged(e);
            }

            protected override bool ProcessDialogKey(Keys keyData)
            {
                if ((keyData & (Keys.Alt | Keys.Shift)) == Keys.None)
                {
                    Keys keys = keyData & Keys.KeyCode;
                    if ((keys == Keys.Escape) && ((keyData & Keys.Control) == Keys.None))
                    {
                        this._escapePressed = true;
                    }
                }
                return base.ProcessDialogKey(keyData);
            }

            public void ProcessSurrogateFonts(int start, int length)
            {
                string text = this.Text;
                if (text != null)
                {
                    int[] numArray = StringInfo.ParseCombiningCharacters(text);
                    if (numArray.Length != text.Length)
                    {
                        for (int i = 0; i < numArray.Length; i++)
                        {
                            if ((numArray[i] >= start) && (numArray[i] < (start + length)))
                            {
                                string str2 = null;
                                char ch = text[numArray[i]];
                                char ch2 = '\0';
                                if ((numArray[i] + 1) < text.Length)
                                {
                                    ch2 = text[numArray[i] + 1];
                                }
                                if (((ch >= 0xd800) && (ch <= 0xdbff)) && ((ch2 >= 0xdc00) && (ch2 <= 0xdfff)))
                                {
                                    int num2 = ((ch / '@') - 0x360) + 1;
                                    System.Drawing.Font font = this._fallbackFonts[num2] as System.Drawing.Font;
                                    if (font == null)
                                    {
                                        using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\LanguagePack\SurrogateFallback"))
                                        {
                                            if (key != null)
                                            {
                                                str2 = (string) key.GetValue("Plane" + num2);
                                                if (!string.IsNullOrEmpty(str2))
                                                {
                                                    font = new System.Drawing.Font(str2, base.Font.Size, base.Font.Style);
                                                }
                                                this._fallbackFonts[num2] = font;
                                            }
                                        }
                                    }
                                    if (font != null)
                                    {
                                        int num3 = (i == (numArray.Length - 1)) ? (text.Length - numArray[i]) : (numArray[i + 1] - numArray[i]);
                                        base.Select(numArray[i], num3);
                                        base.SelectionFont = font;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            private void ResizeToContent()
            {
                if (this._firstTimeResizeToContent)
                {
                    this._firstTimeResizeToContent = false;
                }
                else if (!base.Visible)
                {
                    return;
                }
                Size contentSize = this.ContentSize;
                contentSize.Width += SystemInformation.VerticalScrollBarWidth;
                contentSize.Width = Math.Max(contentSize.Width, this.MinimumSize.Width);
                Rectangle workingArea = Screen.GetWorkingArea(this);
                int num = base.PointToScreen(base.Location).X - workingArea.Left;
                int num2 = Math.Min(contentSize.Width - base.ClientSize.Width, num);
                base.ClientSize = new Size(base.ClientSize.Width + num2, this.MinimumSize.Height);
            }

            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);
                if ((m.Msg == 15) && this.ShouldShowWatermark)
                {
                    using (Graphics graphics = base.CreateGraphics())
                    {
                        graphics.DrawString(System.Design.SR.GetString("MultilineStringEditorWatermark"), this.Font, this.WatermarkBrush, new RectangleF(0f, 0f, (float) base.ClientSize.Width, (float) base.ClientSize.Height), this._watermarkFormat);
                    }
                }
            }

            private Size ContentSize
            {
                get
                {
                    System.Design.NativeMethods.RECT lpRect = new System.Design.NativeMethods.RECT();
                    HandleRef hDC = new HandleRef(null, System.Design.UnsafeNativeMethods.GetDC(System.Design.NativeMethods.NullHandleRef));
                    HandleRef hObject = new HandleRef(null, this.Font.ToHfont());
                    HandleRef ref4 = new HandleRef(null, System.Design.SafeNativeMethods.SelectObject(hDC, hObject));
                    try
                    {
                        System.Design.SafeNativeMethods.DrawText(hDC, this.Text, this.Text.Length, ref lpRect, 0x400);
                    }
                    finally
                    {
                        System.Design.NativeMethods.ExternalDeleteObject(hObject);
                        System.Design.SafeNativeMethods.SelectObject(hDC, ref4);
                        System.Design.UnsafeNativeMethods.ReleaseDC(System.Design.NativeMethods.NullHandleRef, hDC);
                    }
                    return new Size((lpRect.right - lpRect.left) + 3, lpRect.bottom - lpRect.top);
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
                }
            }

            public override Size MinimumSize
            {
                get
                {
                    if (this._minimumSize == Size.Empty)
                    {
                        Rectangle workingArea = Screen.GetWorkingArea(this);
                        this._minimumSize = new Size((int) Math.Min(Math.Ceiling((double) (this.WatermarkSize.Width * 1.75)), (double) (workingArea.Width / 3)), Math.Min((int) (this.Font.Height * 10), (int) (workingArea.Height / 3)));
                    }
                    return this._minimumSize;
                }
            }

            private bool ShouldShowWatermark
            {
                get
                {
                    if (this.Text.Length != 0)
                    {
                        return false;
                    }
                    return (this.WatermarkSize.Width < base.ClientSize.Width);
                }
            }

            public override string Text
            {
                get
                {
                    if (!base.IsHandleCreated)
                    {
                        return "";
                    }
                    StringBuilder lpString = new StringBuilder(System.Design.SafeNativeMethods.GetWindowTextLength(new HandleRef(this, base.Handle)) + 1);
                    System.Design.UnsafeNativeMethods.GetWindowText(new HandleRef(this, base.Handle), lpString, lpString.Capacity);
                    if (!this._ctrlEnterPressed)
                    {
                        return lpString.ToString();
                    }
                    string str = lpString.ToString();
                    int startIndex = str.LastIndexOf("\r\n");
                    return str.Remove(startIndex, 2);
                }
                set
                {
                    base.Text = value;
                }
            }

            internal object Value
            {
                get
                {
                    return this.Text;
                }
            }

            private Brush WatermarkBrush
            {
                get
                {
                    if (this._watermarkBrush == null)
                    {
                        System.Drawing.Color window = SystemColors.Window;
                        System.Drawing.Color windowText = SystemColors.WindowText;
                        System.Drawing.Color color = System.Drawing.Color.FromArgb((short) ((windowText.R * 0.3) + (window.R * 0.7)), (short) ((windowText.G * 0.3) + (window.G * 0.7)), (short) ((windowText.B * 0.3) + (window.B * 0.7)));
                        this._watermarkBrush = new SolidBrush(color);
                    }
                    return this._watermarkBrush;
                }
            }

            private Size WatermarkSize
            {
                get
                {
                    if (this._watermarkSize == Size.Empty)
                    {
                        SizeF ef;
                        using (Graphics graphics = base.CreateGraphics())
                        {
                            ef = graphics.MeasureString(System.Design.SR.GetString("MultilineStringEditorWatermark"), this.Font);
                        }
                        this._watermarkSize = new Size((int) Math.Ceiling((double) ef.Width), (int) Math.Ceiling((double) ef.Height));
                    }
                    return this._watermarkSize;
                }
            }
        }

        private class OleCallback : System.Design.UnsafeNativeMethods.IRichTextBoxOleCallback
        {
            private RichTextBox owner;
            private static TraceSwitch richTextDbg;
            private bool unrestricted;

            internal OleCallback(RichTextBox owner)
            {
                this.owner = owner;
            }

            public int ContextSensitiveHelp(int fEnterMode)
            {
                return -2147467263;
            }

            public int DeleteObject(IntPtr lpoleobj)
            {
                return 0;
            }

            public int GetClipboardData(System.Design.NativeMethods.CHARRANGE lpchrg, int reco, IntPtr lplpdataobj)
            {
                return -2147467263;
            }

            public int GetContextMenu(short seltype, IntPtr lpoleobj, System.Design.NativeMethods.CHARRANGE lpchrg, out IntPtr hmenu)
            {
                TextBox box = new TextBox {
                    Visible = true
                };
                ContextMenu contextMenu = box.ContextMenu;
                if ((contextMenu == null) || !this.owner.ShortcutsEnabled)
                {
                    hmenu = IntPtr.Zero;
                }
                else
                {
                    hmenu = contextMenu.Handle;
                }
                return 0;
            }

            public int GetDragDropEffect(bool fDrag, int grfKeyState, ref int pdwEffect)
            {
                pdwEffect = 0;
                return 0;
            }

            public int GetInPlaceContext(IntPtr lplpFrame, IntPtr lplpDoc, IntPtr lpFrameInfo)
            {
                return -2147467263;
            }

            public int GetNewStorage(out System.Design.UnsafeNativeMethods.IStorage storage)
            {
                System.Design.UnsafeNativeMethods.ILockBytes iLockBytes = System.Design.UnsafeNativeMethods.CreateILockBytesOnHGlobal(System.Design.NativeMethods.NullHandleRef, true);
                storage = System.Design.UnsafeNativeMethods.StgCreateDocfileOnILockBytes(iLockBytes, 0x1012, 0);
                return 0;
            }

            public int QueryAcceptData(System.Runtime.InteropServices.ComTypes.IDataObject lpdataobj, IntPtr lpcfFormat, int reco, int fReally, IntPtr hMetaPict)
            {
                if (reco != 0)
                {
                    return -2147467263;
                }
                DataObject obj2 = new DataObject(lpdataobj);
                if ((obj2 == null) || (!obj2.GetDataPresent(DataFormats.Text) && !obj2.GetDataPresent(DataFormats.UnicodeText)))
                {
                    return -2147467259;
                }
                return 0;
            }

            public int QueryInsertObject(ref Guid lpclsid, IntPtr lpstg, int cp)
            {
                if (!this.unrestricted)
                {
                    string str;
                    Guid pclsid = new Guid();
                    if (!System.Design.NativeMethods.Succeeded(System.Design.UnsafeNativeMethods.ReadClassStg(new HandleRef(null, lpstg), ref pclsid)))
                    {
                        return 1;
                    }
                    if (pclsid == Guid.Empty)
                    {
                        pclsid = lpclsid;
                    }
                    if (((str = pclsid.ToString().ToUpper(CultureInfo.InvariantCulture)) == null) || ((!(str == "00000315-0000-0000-C000-000000000046") && !(str == "00000316-0000-0000-C000-000000000046")) && (!(str == "00000319-0000-0000-C000-000000000046") && !(str == "0003000A-0000-0000-C000-000000000046"))))
                    {
                        return 1;
                    }
                }
                return 0;
            }

            public int ShowContainerUI(int fShow)
            {
                return 0;
            }

            private static TraceSwitch RichTextDbg
            {
                get
                {
                    if (richTextDbg == null)
                    {
                        richTextDbg = new TraceSwitch("RichTextDbg", "Debug info about RichTextBox");
                    }
                    return richTextDbg;
                }
            }
        }
    }
}

