namespace System.Windows.Forms
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Windows.Forms.Layout;

    [System.Windows.Forms.SRDescription("DescriptionRichTextBox"), ClassInterface(ClassInterfaceType.AutoDispatch), ComVisible(true), Docking(DockingBehavior.Ask), Designer("System.Windows.Forms.Design.RichTextBoxDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public class RichTextBox : TextBoxBase
    {
        private static readonly BitVector32.Section allowOleDropSection = BitVector32.CreateSection(1, linkcursorSection);
        private static readonly BitVector32.Section allowOleObjectsSection = BitVector32.CreateSection(1, richTextShortcutsEnabledSection);
        internal const int ANSI = 4;
        private static readonly BitVector32.Section autoUrlDetectSection = BitVector32.CreateSection(1, showSelBarSection);
        private static readonly BitVector32.Section autoWordSelectionSection = BitVector32.CreateSection(1);
        private int bulletIndent;
        private static readonly BitVector32.Section callOnContentsResizedSection = BitVector32.CreateSection(1, suppressTextChangedEventSection);
        private const int CHAR_BUFFER_LEN = 0x200;
        private int curSelEnd;
        private int curSelStart;
        private short curSelType;
        internal const int DIRECTIONMASK = 3;
        private const int DV_E_DVASPECT = -2147221397;
        private const int DVASPECT_CONTENT = 1;
        private const int DVASPECT_DOCPRINT = 8;
        private const int DVASPECT_ICON = 4;
        private const int DVASPECT_THUMBNAIL = 2;
        private Stream editStream;
        private static readonly BitVector32.Section enableAutoDragDropSection = BitVector32.CreateSection(1, scrollBarsSection);
        private static readonly object EVENT_HSCROLL = new object();
        private static readonly object EVENT_IMECHANGE = new object();
        private static readonly object EVENT_LINKACTIVATE = new object();
        private static readonly object EVENT_PROTECTED = new object();
        private static readonly object EVENT_REQUESTRESIZE = new object();
        private static readonly object EVENT_SELCHANGE = new object();
        private static readonly object EVENT_VSCROLL = new object();
        private static readonly BitVector32.Section fInCtorSection = BitVector32.CreateSection(1, autoUrlDetectSection);
        internal const int FORMATMASK = 12;
        internal const int INPUT = 1;
        internal const int KINDMASK = 0x70;
        private RichTextBoxLanguageOptions languageOption = (RichTextBoxLanguageOptions.DualFont | RichTextBoxLanguageOptions.AutoFont);
        private static readonly BitVector32.Section linkcursorSection = BitVector32.CreateSection(1, protectedErrorSection);
        private static int logPixelsX;
        private static int logPixelsY;
        private static IntPtr moduleHandle;
        private object oleCallback;
        internal const int OUTPUT = 2;
        private static readonly BitVector32.Section protectedErrorSection = BitVector32.CreateSection(1, fInCtorSection);
        private static int richEditMajorVersion = 3;
        private BitVector32 richTextBoxFlags = new BitVector32();
        private static TraceSwitch richTextDbg;
        private static readonly BitVector32.Section richTextShortcutsEnabledSection = BitVector32.CreateSection(1, callOnContentsResizedSection);
        private int rightMargin;
        internal const int RTF = 0x40;
        private static readonly BitVector32.Section scrollBarsSection = BitVector32.CreateSection(0x13, allowOleObjectsSection);
        private System.Drawing.Color selectionBackColorToSetOnHandleCreated;
        private static int[] shortcutsToDisable;
        private static readonly BitVector32.Section showSelBarSection = BitVector32.CreateSection(1, autoWordSelectionSection);
        private static readonly BitVector32.Section suppressTextChangedEventSection = BitVector32.CreateSection(1, allowOleDropSection);
        private static readonly string SZ_RTF_TAG = @"{\rtf";
        internal const int TEXTCRLF = 0x20;
        internal const int TEXTLF = 0x10;
        private string textPlain;
        private string textRtf;
        internal const int UNICODE = 8;
        private float zoomMultiplier = 1f;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("RichTextBoxContentsResized")]
        public event ContentsResizedEventHandler ContentsResized
        {
            add
            {
                base.Events.AddHandler(EVENT_REQUESTRESIZE, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_REQUESTRESIZE, value);
            }
        }

        [Browsable(false)]
        public event DragEventHandler DragDrop
        {
            add
            {
                base.DragDrop += value;
            }
            remove
            {
                base.DragDrop -= value;
            }
        }

        [Browsable(false)]
        public event DragEventHandler DragEnter
        {
            add
            {
                base.DragEnter += value;
            }
            remove
            {
                base.DragEnter -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler DragLeave
        {
            add
            {
                base.DragLeave += value;
            }
            remove
            {
                base.DragLeave -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event DragEventHandler DragOver
        {
            add
            {
                base.DragOver += value;
            }
            remove
            {
                base.DragOver -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event GiveFeedbackEventHandler GiveFeedback
        {
            add
            {
                base.GiveFeedback += value;
            }
            remove
            {
                base.GiveFeedback -= value;
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("RichTextBoxHScroll")]
        public event EventHandler HScroll
        {
            add
            {
                base.Events.AddHandler(EVENT_HSCROLL, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_HSCROLL, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("RichTextBoxIMEChange")]
        public event EventHandler ImeChange
        {
            add
            {
                base.Events.AddHandler(EVENT_IMECHANGE, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_IMECHANGE, value);
            }
        }

        [System.Windows.Forms.SRDescription("RichTextBoxLinkClick"), System.Windows.Forms.SRCategory("CatBehavior")]
        public event LinkClickedEventHandler LinkClicked
        {
            add
            {
                base.Events.AddHandler(EVENT_LINKACTIVATE, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_LINKACTIVATE, value);
            }
        }

        [System.Windows.Forms.SRDescription("RichTextBoxProtected"), System.Windows.Forms.SRCategory("CatBehavior")]
        public event EventHandler Protected
        {
            add
            {
                base.Events.AddHandler(EVENT_PROTECTED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_PROTECTED, value);
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event QueryContinueDragEventHandler QueryContinueDrag
        {
            add
            {
                base.QueryContinueDrag += value;
            }
            remove
            {
                base.QueryContinueDrag -= value;
            }
        }

        [System.Windows.Forms.SRDescription("RichTextBoxSelChange"), System.Windows.Forms.SRCategory("CatBehavior")]
        public event EventHandler SelectionChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_SELCHANGE, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_SELCHANGE, value);
            }
        }

        [System.Windows.Forms.SRDescription("RichTextBoxVScroll"), System.Windows.Forms.SRCategory("CatBehavior")]
        public event EventHandler VScroll
        {
            add
            {
                base.Events.AddHandler(EVENT_VSCROLL, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_VSCROLL, value);
            }
        }

        public RichTextBox()
        {
            this.InConstructor = true;
            this.richTextBoxFlags[autoWordSelectionSection] = 0;
            this.DetectUrls = true;
            this.ScrollBars = RichTextBoxScrollBars.Both;
            this.RichTextShortcutsEnabled = true;
            this.MaxLength = 0x7fffffff;
            this.Multiline = true;
            this.AutoSize = false;
            this.curSelStart = this.curSelEnd = this.curSelType = -1;
            this.InConstructor = false;
        }

        public bool CanPaste(DataFormats.Format clipFormat)
        {
            return (((int) ((long) base.SendMessage(0x432, clipFormat.Id, 0))) != 0);
        }

        private string CharRangeToString(System.Windows.Forms.NativeMethods.CHARRANGE c)
        {
            System.Windows.Forms.NativeMethods.TEXTRANGE lParam = new System.Windows.Forms.NativeMethods.TEXTRANGE {
                chrg = c
            };
            if ((c.cpMax > this.Text.Length) || ((c.cpMax - c.cpMin) <= 0))
            {
                return string.Empty;
            }
            int size = (c.cpMax - c.cpMin) + 1;
            System.Windows.Forms.UnsafeNativeMethods.CharBuffer buffer = System.Windows.Forms.UnsafeNativeMethods.CharBuffer.CreateBuffer(size);
            IntPtr ptr = buffer.AllocCoTaskMem();
            if (ptr == IntPtr.Zero)
            {
                throw new OutOfMemoryException(System.Windows.Forms.SR.GetString("OutOfMemory"));
            }
            lParam.lpstrText = ptr;
            int num1 = (int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x44b, 0, lParam);
            buffer.PutCoTaskMem(ptr);
            if (lParam.lpstrText != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(ptr);
            }
            return buffer.GetString();
        }

        private static unsafe System.Windows.Forms.NativeMethods.ENLINK ConvertFromENLINK64(System.Windows.Forms.NativeMethods.ENLINK64 es64)
        {
            System.Windows.Forms.NativeMethods.ENLINK enlink = new System.Windows.Forms.NativeMethods.ENLINK();
            fixed (byte* numRef = es64.contents)
            {
                enlink.nmhdr = new System.Windows.Forms.NativeMethods.NMHDR();
                enlink.charrange = new System.Windows.Forms.NativeMethods.CHARRANGE();
                enlink.nmhdr.hwndFrom = Marshal.ReadIntPtr((IntPtr) numRef);
                enlink.nmhdr.idFrom = Marshal.ReadIntPtr((IntPtr) (numRef + 8));
                enlink.nmhdr.code = Marshal.ReadInt32((IntPtr) (numRef + 0x10));
                enlink.msg = Marshal.ReadInt32((IntPtr) (numRef + 0x18));
                enlink.wParam = Marshal.ReadIntPtr((IntPtr) (numRef + 0x1c));
                enlink.lParam = Marshal.ReadIntPtr((IntPtr) (numRef + 0x24));
                enlink.charrange.cpMin = Marshal.ReadInt32((IntPtr) (numRef + 0x2c));
                enlink.charrange.cpMax = Marshal.ReadInt32((IntPtr) (numRef + 0x30));
            }
            return enlink;
        }

        private unsafe System.Windows.Forms.NativeMethods.ENPROTECTED ConvertFromENPROTECTED64(System.Windows.Forms.NativeMethods.ENPROTECTED64 es64)
        {
            System.Windows.Forms.NativeMethods.ENPROTECTED enprotected = new System.Windows.Forms.NativeMethods.ENPROTECTED();
            fixed (byte* numRef = es64.contents)
            {
                enprotected.nmhdr = new System.Windows.Forms.NativeMethods.NMHDR();
                enprotected.chrg = new System.Windows.Forms.NativeMethods.CHARRANGE();
                enprotected.nmhdr.hwndFrom = Marshal.ReadIntPtr((IntPtr) numRef);
                enprotected.nmhdr.idFrom = Marshal.ReadIntPtr((IntPtr) (numRef + 8));
                enprotected.nmhdr.code = Marshal.ReadInt32((IntPtr) (numRef + 0x10));
                enprotected.msg = Marshal.ReadInt32((IntPtr) (numRef + 0x18));
                enprotected.wParam = Marshal.ReadIntPtr((IntPtr) (numRef + 0x1c));
                enprotected.lParam = Marshal.ReadIntPtr((IntPtr) (numRef + 0x24));
                enprotected.chrg.cpMin = Marshal.ReadInt32((IntPtr) (numRef + 0x2c));
                enprotected.chrg.cpMax = Marshal.ReadInt32((IntPtr) (numRef + 0x30));
            }
            return enprotected;
        }

        private unsafe System.Windows.Forms.NativeMethods.EDITSTREAM64 ConvertToEDITSTREAM64(System.Windows.Forms.NativeMethods.EDITSTREAM es)
        {
            System.Windows.Forms.NativeMethods.EDITSTREAM64 editstream = new System.Windows.Forms.NativeMethods.EDITSTREAM64();
            fixed (byte* numRef = editstream.contents)
            {
                *((long*) numRef) = (long) es.dwCookie;
                numRef[8] = (byte) es.dwError;
                long functionPointerForDelegate = (long) Marshal.GetFunctionPointerForDelegate(es.pfnCallback);
                byte* numPtr = (byte*) &functionPointerForDelegate;
                for (int i = 0; i < 8; i++)
                {
                    editstream.contents[i + 12] = numPtr[i];
                }
            }
            return editstream;
        }

        [SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected virtual object CreateRichEditOleCallback()
        {
            return new OleCallback(this);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void DrawToBitmap(Bitmap bitmap, Rectangle targetBounds)
        {
            base.DrawToBitmap(bitmap, targetBounds);
        }

        private unsafe int EditStreamProc(IntPtr dwCookie, IntPtr buf, int cb, out int transferred)
        {
            int num = 0;
            byte[] buffer = new byte[cb];
            int num2 = (int) dwCookie;
            transferred = 0;
            try
            {
                switch ((num2 & 3))
                {
                    case 1:
                        if (this.editStream == null)
                        {
                            goto Label_01C0;
                        }
                        transferred = this.editStream.Read(buffer, 0, cb);
                        Marshal.Copy(buffer, 0, buf, transferred);
                        if (transferred < 0)
                        {
                            transferred = 0;
                        }
                        return num;

                    case 2:
                        if (this.editStream == null)
                        {
                            this.editStream = new MemoryStream();
                        }
                        switch ((num2 & 0x70))
                        {
                            case 0x10:
                                goto Label_0079;

                            case 0x20:
                            case 0x40:
                                Marshal.Copy(buf, buffer, 0, cb);
                                this.editStream.Write(buffer, 0, cb);
                                break;
                        }
                        goto Label_018A;
                }
                return num;
            Label_0079:
                if ((num2 & 8) != 0)
                {
                    int num3 = cb / 2;
                    int num4 = 0;
                    try
                    {
                        fixed (byte* numRef = buffer)
                        {
                            char* chPtr = (char*) numRef;
                            char* chPtr2 = (char*) ((long) buf);
                            for (int i = 0; i < num3; i++)
                            {
                                if (chPtr2[0] == '\r')
                                {
                                    chPtr2++;
                                }
                                else
                                {
                                    chPtr[0] = chPtr2[0];
                                    chPtr++;
                                    chPtr2++;
                                    num4++;
                                }
                            }
                        }
                    }
                    finally
                    {
                        numRef = null;
                    }
                    this.editStream.Write(buffer, 0, num4 * 2);
                }
                else
                {
                    int num6 = cb;
                    int count = 0;
                    try
                    {
                        fixed (byte* numRef2 = buffer)
                        {
                            byte* numPtr = numRef2;
                            byte* numPtr2 = (byte*) ((long) buf);
                            for (int j = 0; j < num6; j++)
                            {
                                if (numPtr2[0] == 13)
                                {
                                    numPtr2++;
                                }
                                else
                                {
                                    numPtr[0] = numPtr2[0];
                                    numPtr++;
                                    numPtr2++;
                                    count++;
                                }
                            }
                        }
                    }
                    finally
                    {
                        numRef2 = null;
                    }
                    this.editStream.Write(buffer, 0, count);
                }
            Label_018A:
                transferred = cb;
                return num;
            Label_01C0:
                transferred = 0;
            }
            catch (IOException)
            {
                transferred = 0;
                num = 1;
            }
            return num;
        }

        private void EnLinkMsgHandler(ref Message m)
        {
            System.Windows.Forms.NativeMethods.ENLINK lParam;
            if (IntPtr.Size == 8)
            {
                lParam = ConvertFromENLINK64((System.Windows.Forms.NativeMethods.ENLINK64) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.ENLINK64)));
            }
            else
            {
                lParam = (System.Windows.Forms.NativeMethods.ENLINK) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.ENLINK));
            }
            switch (lParam.msg)
            {
                case 0x20:
                    this.LinkCursor = true;
                    m.Result = (IntPtr) 1;
                    return;

                case 0x201:
                {
                    string str = this.CharRangeToString(lParam.charrange);
                    if (!string.IsNullOrEmpty(str))
                    {
                        this.OnLinkClicked(new LinkClickedEventArgs(str));
                    }
                    m.Result = (IntPtr) 1;
                    return;
                }
            }
            m.Result = IntPtr.Zero;
        }

        public int Find(string str)
        {
            return this.Find(str, 0, 0, RichTextBoxFinds.None);
        }

        public int Find(char[] characterSet)
        {
            return this.Find(characterSet, 0, -1);
        }

        public int Find(char[] characterSet, int start)
        {
            return this.Find(characterSet, start, -1);
        }

        public int Find(string str, RichTextBoxFinds options)
        {
            return this.Find(str, 0, 0, options);
        }

        public int Find(char[] characterSet, int start, int end)
        {
            System.Windows.Forms.NativeMethods.CHARRANGE charrange;
            bool flag = true;
            bool negate = false;
            int textLength = this.TextLength;
            if (characterSet == null)
            {
                throw new ArgumentNullException("characterSet");
            }
            if ((start < 0) || (start > textLength))
            {
                throw new ArgumentOutOfRangeException("start", System.Windows.Forms.SR.GetString("InvalidBoundArgument", new object[] { "start", start, 0, textLength }));
            }
            if ((end < start) && (end != -1))
            {
                throw new ArgumentOutOfRangeException("end", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", new object[] { "end", end, "start" }));
            }
            if (characterSet.Length == 0)
            {
                return -1;
            }
            int windowTextLength = System.Windows.Forms.SafeNativeMethods.GetWindowTextLength(new HandleRef(this, base.Handle));
            if (start == end)
            {
                start = 0;
                end = windowTextLength;
            }
            if (end == -1)
            {
                end = windowTextLength;
            }
            charrange = new System.Windows.Forms.NativeMethods.CHARRANGE {
                cpMax = charrange.cpMin = start
            };
            System.Windows.Forms.NativeMethods.TEXTRANGE lParam = new System.Windows.Forms.NativeMethods.TEXTRANGE {
                chrg = new System.Windows.Forms.NativeMethods.CHARRANGE()
            };
            lParam.chrg.cpMin = charrange.cpMin;
            lParam.chrg.cpMax = charrange.cpMax;
            System.Windows.Forms.UnsafeNativeMethods.CharBuffer buffer = System.Windows.Forms.UnsafeNativeMethods.CharBuffer.CreateBuffer(0x201);
            lParam.lpstrText = buffer.AllocCoTaskMem();
            if (lParam.lpstrText == IntPtr.Zero)
            {
                throw new OutOfMemoryException();
            }
            try
            {
                bool flag3 = false;
                while (!flag3)
                {
                    if (flag)
                    {
                        lParam.chrg.cpMin = charrange.cpMax;
                        lParam.chrg.cpMax += 0x200;
                    }
                    else
                    {
                        lParam.chrg.cpMax = charrange.cpMin;
                        lParam.chrg.cpMin -= 0x200;
                        if (lParam.chrg.cpMin < 0)
                        {
                            lParam.chrg.cpMin = 0;
                        }
                    }
                    if (end != -1)
                    {
                        lParam.chrg.cpMax = Math.Min(lParam.chrg.cpMax, end);
                    }
                    int num3 = (int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x44b, 0, lParam);
                    if (num3 == 0)
                    {
                        charrange.cpMax = charrange.cpMin = -1;
                        goto Label_02F4;
                    }
                    buffer.PutCoTaskMem(lParam.lpstrText);
                    string str = buffer.GetString();
                    if (flag)
                    {
                        for (int i = 0; i < num3; i++)
                        {
                            if (this.GetCharInCharSet(str[i], characterSet, negate))
                            {
                                flag3 = true;
                                continue;
                            }
                            charrange.cpMax++;
                        }
                    }
                    else
                    {
                        int num5 = num3;
                        while (num5-- != 0)
                        {
                            if (this.GetCharInCharSet(str[num5], characterSet, negate))
                            {
                                flag3 = true;
                                continue;
                            }
                            charrange.cpMin--;
                        }
                    }
                }
            }
            finally
            {
                if (lParam.lpstrText != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(lParam.lpstrText);
                }
            }
        Label_02F4:
            return (flag ? charrange.cpMax : charrange.cpMin);
        }

        public int Find(string str, int start, RichTextBoxFinds options)
        {
            return this.Find(str, start, -1, options);
        }

        public int Find(string str, int start, int end, RichTextBoxFinds options)
        {
            int textLength = this.TextLength;
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
            if ((start < 0) || (start > textLength))
            {
                throw new ArgumentOutOfRangeException("start", System.Windows.Forms.SR.GetString("InvalidBoundArgument", new object[] { "start", start, 0, textLength }));
            }
            if (end < -1)
            {
                throw new ArgumentOutOfRangeException("end", System.Windows.Forms.SR.GetString("RichTextFindEndInvalid", new object[] { end }));
            }
            bool flag = true;
            System.Windows.Forms.NativeMethods.FINDTEXT lParam = new System.Windows.Forms.NativeMethods.FINDTEXT {
                chrg = new System.Windows.Forms.NativeMethods.CHARRANGE(),
                lpstrText = str
            };
            if (end == -1)
            {
                end = textLength;
            }
            if (start > end)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("RichTextFindEndInvalid", new object[] { end }));
            }
            if ((options & RichTextBoxFinds.Reverse) != RichTextBoxFinds.Reverse)
            {
                lParam.chrg.cpMin = start;
                lParam.chrg.cpMax = end;
            }
            else
            {
                lParam.chrg.cpMin = end;
                lParam.chrg.cpMax = start;
            }
            if (lParam.chrg.cpMin == lParam.chrg.cpMax)
            {
                if ((options & RichTextBoxFinds.Reverse) != RichTextBoxFinds.Reverse)
                {
                    lParam.chrg.cpMin = 0;
                    lParam.chrg.cpMax = -1;
                }
                else
                {
                    lParam.chrg.cpMin = textLength;
                    lParam.chrg.cpMax = 0;
                }
            }
            int wParam = 0;
            if ((options & RichTextBoxFinds.WholeWord) == RichTextBoxFinds.WholeWord)
            {
                wParam |= 2;
            }
            if ((options & RichTextBoxFinds.MatchCase) == RichTextBoxFinds.MatchCase)
            {
                wParam |= 4;
            }
            if ((options & RichTextBoxFinds.NoHighlight) == RichTextBoxFinds.NoHighlight)
            {
                flag = false;
            }
            if ((options & RichTextBoxFinds.Reverse) != RichTextBoxFinds.Reverse)
            {
                wParam |= 1;
            }
            int startIndex = (int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x438, wParam, lParam);
            if ((startIndex != -1) && flag)
            {
                System.Windows.Forms.NativeMethods.CHARRANGE charrange = new System.Windows.Forms.NativeMethods.CHARRANGE {
                    cpMin = startIndex
                };
                char ch = 'ـ';
                string text = this.Text;
                int index = text.Substring(startIndex, str.Length).IndexOf(ch);
                if (index == -1)
                {
                    charrange.cpMax = startIndex + str.Length;
                }
                else
                {
                    int num5 = index;
                    int num6 = startIndex + index;
                    while (num5 < str.Length)
                    {
                        while ((text[num6] == ch) && (str[num5] != ch))
                        {
                            num6++;
                        }
                        num5++;
                        num6++;
                    }
                    charrange.cpMax = num6;
                }
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x437, 0, charrange);
                base.SendMessage(0xb7, 0, 0);
            }
            return startIndex;
        }

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode)]
        private static void FontToLogFont(System.Drawing.Font value, System.Windows.Forms.NativeMethods.LOGFONT logfont)
        {
            value.ToLogFont(logfont);
        }

        private void ForceHandleCreate()
        {
            if (!base.IsHandleCreated)
            {
                this.CreateHandle();
            }
        }

        private System.Windows.Forms.NativeMethods.CHARFORMATA GetCharFormat(bool fSelection)
        {
            System.Windows.Forms.NativeMethods.CHARFORMATA lParam = new System.Windows.Forms.NativeMethods.CHARFORMATA();
            System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x43a, fSelection ? 1 : 0, lParam);
            return lParam;
        }

        private RichTextBoxSelectionAttribute GetCharFormat(int mask, int effect)
        {
            RichTextBoxSelectionAttribute none = RichTextBoxSelectionAttribute.None;
            if (base.IsHandleCreated)
            {
                System.Windows.Forms.NativeMethods.CHARFORMATA charFormat = this.GetCharFormat(true);
                if (((charFormat.dwMask & mask) != 0) && ((charFormat.dwEffects & effect) != 0))
                {
                    none = RichTextBoxSelectionAttribute.All;
                }
            }
            return none;
        }

        private System.Windows.Forms.NativeMethods.CHARFORMAT2A GetCharFormat2(bool fSelection)
        {
            System.Windows.Forms.NativeMethods.CHARFORMAT2A lParam = new System.Windows.Forms.NativeMethods.CHARFORMAT2A();
            System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x43a, fSelection ? 1 : 0, lParam);
            return lParam;
        }

        private System.Drawing.Font GetCharFormatFont(bool selectionOnly)
        {
            this.ForceHandleCreate();
            System.Windows.Forms.NativeMethods.CHARFORMATA charFormat = this.GetCharFormat(selectionOnly);
            if ((charFormat.dwMask & 0x20000000) != 0)
            {
                string familyName = Encoding.Default.GetString(charFormat.szFaceName);
                int index = familyName.IndexOf('\0');
                if (index != -1)
                {
                    familyName = familyName.Substring(0, index);
                }
                float emSize = 13f;
                if ((charFormat.dwMask & -2147483648) != 0)
                {
                    emSize = ((float) charFormat.yHeight) / 20f;
                    if ((emSize == 0f) && (charFormat.yHeight > 0))
                    {
                        emSize = 1f;
                    }
                }
                FontStyle regular = FontStyle.Regular;
                if (((charFormat.dwMask & 1) != 0) && ((charFormat.dwEffects & 1) != 0))
                {
                    regular |= FontStyle.Bold;
                }
                if (((charFormat.dwMask & 2) != 0) && ((charFormat.dwEffects & 2) != 0))
                {
                    regular |= FontStyle.Italic;
                }
                if (((charFormat.dwMask & 8) != 0) && ((charFormat.dwEffects & 8) != 0))
                {
                    regular |= FontStyle.Strikeout;
                }
                if (((charFormat.dwMask & 4) != 0) && ((charFormat.dwEffects & 4) != 0))
                {
                    regular |= FontStyle.Underline;
                }
                try
                {
                    return new System.Drawing.Font(familyName, emSize, regular, GraphicsUnit.Point, charFormat.bCharSet);
                }
                catch
                {
                }
            }
            return null;
        }

        private bool GetCharInCharSet(char c, char[] charSet, bool negate)
        {
            bool flag = false;
            int length = charSet.Length;
            for (int i = 0; !flag && (i < length); i++)
            {
                flag = c == charSet[i];
            }
            if (!negate)
            {
                return flag;
            }
            return !flag;
        }

        public override int GetCharIndexFromPosition(Point pt)
        {
            System.Windows.Forms.NativeMethods.POINT lParam = new System.Windows.Forms.NativeMethods.POINT(pt.X, pt.Y);
            int num = (int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0xd7, 0, lParam);
            string text = this.Text;
            if (num >= text.Length)
            {
                num = Math.Max(text.Length - 1, 0);
            }
            return num;
        }

        private string GetEditorActionName(int actionID)
        {
            switch (actionID)
            {
                case 1:
                    return System.Windows.Forms.SR.GetString("RichTextBox_IDTyping");

                case 2:
                    return System.Windows.Forms.SR.GetString("RichTextBox_IDDelete");

                case 3:
                    return System.Windows.Forms.SR.GetString("RichTextBox_IDDragDrop");

                case 4:
                    return System.Windows.Forms.SR.GetString("RichTextBox_IDCut");

                case 5:
                    return System.Windows.Forms.SR.GetString("RichTextBox_IDPaste");
            }
            return System.Windows.Forms.SR.GetString("RichTextBox_IDUnknown");
        }

        private unsafe int GetErrorValue64(System.Windows.Forms.NativeMethods.EDITSTREAM64 es64)
        {
            int num;
            fixed (byte* numRef = es64.contents)
            {
                num = numRef[8];
            }
            return num;
        }

        public override int GetLineFromCharIndex(int index)
        {
            return (int) ((long) base.SendMessage(0x436, 0, index));
        }

        public override Point GetPositionFromCharIndex(int index)
        {
            if (richEditMajorVersion == 2)
            {
                return base.GetPositionFromCharIndex(index);
            }
            if ((index < 0) || (index > this.Text.Length))
            {
                return Point.Empty;
            }
            System.Windows.Forms.NativeMethods.POINT wParam = new System.Windows.Forms.NativeMethods.POINT();
            System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0xd6, wParam, index);
            return new Point(wParam.x, wParam.y);
        }

        internal override Size GetPreferredSizeCore(Size proposedConstraints)
        {
            Size empty = Size.Empty;
            if ((!base.WordWrap && this.Multiline) && ((this.ScrollBars & RichTextBoxScrollBars.Horizontal) != RichTextBoxScrollBars.None))
            {
                empty.Height += SystemInformation.HorizontalScrollBarHeight;
            }
            if (this.Multiline && ((this.ScrollBars & RichTextBoxScrollBars.Vertical) != RichTextBoxScrollBars.None))
            {
                empty.Width += SystemInformation.VerticalScrollBarWidth;
            }
            proposedConstraints -= empty;
            return (base.GetPreferredSizeCore(proposedConstraints) + empty);
        }

        private bool GetProtectedError()
        {
            if (this.ProtectedError)
            {
                this.ProtectedError = false;
                return true;
            }
            return false;
        }

        private bool InternalSetForeColor(System.Drawing.Color value)
        {
            System.Windows.Forms.NativeMethods.CHARFORMATA charFormat = this.GetCharFormat(false);
            if (((charFormat.dwMask & 0x40000000) != 0) && (ColorTranslator.ToWin32(value) == charFormat.crTextColor))
            {
                return true;
            }
            charFormat.dwMask = 0x40000000;
            charFormat.dwEffects = 0;
            charFormat.crTextColor = ColorTranslator.ToWin32(value);
            return this.SetCharFormat(4, charFormat);
        }

        public void LoadFile(string path)
        {
            this.LoadFile(path, RichTextBoxStreamType.RichText);
        }

        public void LoadFile(Stream data, RichTextBoxStreamType fileType)
        {
            int num;
            if (!System.Windows.Forms.ClientUtils.IsEnumValid(fileType, (int) fileType, 0, 4))
            {
                throw new InvalidEnumArgumentException("fileType", (int) fileType, typeof(RichTextBoxStreamType));
            }
            switch (fileType)
            {
                case RichTextBoxStreamType.RichText:
                    num = 2;
                    break;

                case RichTextBoxStreamType.PlainText:
                    this.Rtf = "";
                    num = 1;
                    break;

                case RichTextBoxStreamType.UnicodePlainText:
                    num = 0x11;
                    break;

                default:
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidFileType"));
            }
            this.StreamIn(data, num);
        }

        public void LoadFile(string path, RichTextBoxStreamType fileType)
        {
            if (!System.Windows.Forms.ClientUtils.IsEnumValid(fileType, (int) fileType, 0, 4))
            {
                throw new InvalidEnumArgumentException("fileType", (int) fileType, typeof(RichTextBoxStreamType));
            }
            Stream data = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            try
            {
                this.LoadFile(data, fileType);
            }
            finally
            {
                data.Close();
            }
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            if (base.IsHandleCreated)
            {
                base.SendMessage(0x443, 0, ColorTranslator.ToWin32(this.BackColor));
            }
            base.OnBackColorChanged(e);
        }

        protected virtual void OnContentsResized(ContentsResizedEventArgs e)
        {
            ContentsResizedEventHandler handler = (ContentsResizedEventHandler) base.Events[EVENT_REQUESTRESIZE];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnContextMenuChanged(EventArgs e)
        {
            base.OnContextMenuChanged(e);
            this.UpdateOleCallback();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            this.curSelStart = this.curSelEnd = this.curSelType = -1;
            this.UpdateMaxLength();
            base.SendMessage(0x445, 0, 0x4bf000f);
            int rightMargin = this.rightMargin;
            this.rightMargin = 0;
            this.RightMargin = rightMargin;
            base.SendMessage(0x45b, this.DetectUrls ? 1 : 0, 0);
            if (this.selectionBackColorToSetOnHandleCreated != System.Drawing.Color.Empty)
            {
                this.SelectionBackColor = this.selectionBackColorToSetOnHandleCreated;
            }
            this.AutoWordSelection = this.AutoWordSelection;
            base.SendMessage(0x443, 0, ColorTranslator.ToWin32(this.BackColor));
            this.InternalSetForeColor(this.ForeColor);
            base.OnHandleCreated(e);
            this.UpdateOleCallback();
            try
            {
                this.SuppressTextChangedEvent = true;
                if (this.textRtf != null)
                {
                    string textRtf = this.textRtf;
                    this.textRtf = null;
                    this.Rtf = textRtf;
                }
                else if (this.textPlain != null)
                {
                    string textPlain = this.textPlain;
                    this.textPlain = null;
                    this.Text = textPlain;
                }
            }
            finally
            {
                this.SuppressTextChangedEvent = false;
            }
            base.SetSelectionOnHandle();
            if (this.ShowSelectionMargin)
            {
                System.Windows.Forms.UnsafeNativeMethods.PostMessage(new HandleRef(this, base.Handle), 0x44d, (IntPtr) 2, (IntPtr) 0x1000000);
            }
            if (this.languageOption != this.LanguageOption)
            {
                this.LanguageOption = this.languageOption;
            }
            base.ClearUndo();
            this.SendZoomFactor(this.zoomMultiplier);
            SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(this.UserPreferenceChangedHandler);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            base.OnHandleDestroyed(e);
            if (!this.InConstructor)
            {
                this.textRtf = this.Rtf;
                if (this.textRtf.Length == 0)
                {
                    this.textRtf = null;
                }
            }
            this.oleCallback = null;
            SystemEvents.UserPreferenceChanged -= new UserPreferenceChangedEventHandler(this.UserPreferenceChangedHandler);
        }

        protected virtual void OnHScroll(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EVENT_HSCROLL];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnImeChange(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EVENT_IMECHANGE];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnLinkClicked(LinkClickedEventArgs e)
        {
            LinkClickedEventHandler handler = (LinkClickedEventHandler) base.Events[EVENT_LINKACTIVATE];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnProtected(EventArgs e)
        {
            this.ProtectedError = true;
            EventHandler handler = (EventHandler) base.Events[EVENT_PROTECTED];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnRightToLeftChanged(EventArgs e)
        {
            base.OnRightToLeftChanged(e);
            string windowText = this.WindowText;
            base.ForceWindowText(null);
            base.ForceWindowText(windowText);
        }

        protected virtual void OnSelectionChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EVENT_SELCHANGE];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnVScroll(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EVENT_VSCROLL];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void Paste(DataFormats.Format clipFormat)
        {
            System.Windows.Forms.IntSecurity.ClipboardRead.Demand();
            this.PasteUnsafe(clipFormat, 0);
        }

        private void PasteUnsafe(DataFormats.Format clipFormat, int hIcon)
        {
            System.Windows.Forms.NativeMethods.REPASTESPECIAL lParam = null;
            if (hIcon != 0)
            {
                lParam = new System.Windows.Forms.NativeMethods.REPASTESPECIAL {
                    dwAspect = 4,
                    dwParam = hIcon
                };
            }
            System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x440, clipFormat.Id, lParam);
        }

        private static int Pixel2Twip(IntPtr hDC, int v, bool xDirection)
        {
            SetupLogPixels(hDC);
            int num = xDirection ? logPixelsX : logPixelsY;
            return (int) (((((double) v) / ((double) num)) * 72.0) * 20.0);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override bool ProcessCmdKey(ref Message m, Keys keyData)
        {
            if (!this.RichTextShortcutsEnabled)
            {
                foreach (int num in shortcutsToDisable)
                {
                    if (keyData == num)
                    {
                        return true;
                    }
                }
            }
            return base.ProcessCmdKey(ref m, keyData);
        }

        public void Redo()
        {
            base.SendMessage(0x454, 0, 0);
        }

        public void SaveFile(string path)
        {
            this.SaveFile(path, RichTextBoxStreamType.RichText);
        }

        public void SaveFile(Stream data, RichTextBoxStreamType fileType)
        {
            int num;
            switch (fileType)
            {
                case RichTextBoxStreamType.RichText:
                    num = 2;
                    break;

                case RichTextBoxStreamType.PlainText:
                    num = 1;
                    break;

                case RichTextBoxStreamType.RichNoOleObjs:
                    num = 3;
                    break;

                case RichTextBoxStreamType.TextTextOleObjs:
                    num = 4;
                    break;

                case RichTextBoxStreamType.UnicodePlainText:
                    num = 0x11;
                    break;

                default:
                    throw new InvalidEnumArgumentException("fileType", (int) fileType, typeof(RichTextBoxStreamType));
            }
            this.StreamOut(data, num, true);
        }

        public void SaveFile(string path, RichTextBoxStreamType fileType)
        {
            if (!System.Windows.Forms.ClientUtils.IsEnumValid(fileType, (int) fileType, 0, 4))
            {
                throw new InvalidEnumArgumentException("fileType", (int) fileType, typeof(RichTextBoxStreamType));
            }
            Stream data = File.Create(path);
            try
            {
                this.SaveFile(data, fileType);
            }
            finally
            {
                data.Close();
            }
        }

        private void SendZoomFactor(float zoom)
        {
            int num;
            int num2;
            if (zoom == 1f)
            {
                num2 = 0;
                num = 0;
            }
            else
            {
                num2 = 0x3e8;
                float num3 = 1000f * zoom;
                num = (int) Math.Ceiling((double) num3);
                if (num >= 0xfa00)
                {
                    num = (int) Math.Floor((double) num3);
                }
            }
            if (base.IsHandleCreated)
            {
                base.SendMessage(0x4e1, num, num2);
            }
            if (num != 0)
            {
                this.zoomMultiplier = ((float) num) / ((float) num2);
            }
            else
            {
                this.zoomMultiplier = 1f;
            }
        }

        private bool SetCharFormat(int charRange, System.Windows.Forms.NativeMethods.CHARFORMATA cf)
        {
            return (IntPtr.Zero != System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x444, charRange, cf));
        }

        private bool SetCharFormat(int mask, int effect, RichTextBoxSelectionAttribute charFormat)
        {
            if (!base.IsHandleCreated)
            {
                return false;
            }
            System.Windows.Forms.NativeMethods.CHARFORMATA lParam = new System.Windows.Forms.NativeMethods.CHARFORMATA {
                dwMask = mask
            };
            switch (charFormat)
            {
                case RichTextBoxSelectionAttribute.None:
                    lParam.dwEffects = 0;
                    break;

                case RichTextBoxSelectionAttribute.All:
                    lParam.dwEffects = effect;
                    break;

                default:
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("UnknownAttr"));
            }
            return (IntPtr.Zero != System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x444, 1, lParam));
        }

        private void SetCharFormatFont(bool selectionOnly, System.Drawing.Font value)
        {
            byte[] bytes;
            this.ForceHandleCreate();
            System.Windows.Forms.NativeMethods.LOGFONT logfont = new System.Windows.Forms.NativeMethods.LOGFONT();
            FontToLogFont(value, logfont);
            int num = -1476394993;
            int num2 = 0;
            if (value.Bold)
            {
                num2 |= 1;
            }
            if (value.Italic)
            {
                num2 |= 2;
            }
            if (value.Strikeout)
            {
                num2 |= 8;
            }
            if (value.Underline)
            {
                num2 |= 4;
            }
            if (Marshal.SystemDefaultCharSize == 1)
            {
                bytes = Encoding.Default.GetBytes(logfont.lfFaceName);
                System.Windows.Forms.NativeMethods.CHARFORMATA lParam = new System.Windows.Forms.NativeMethods.CHARFORMATA();
                for (int i = 0; i < bytes.Length; i++)
                {
                    lParam.szFaceName[i] = bytes[i];
                }
                lParam.dwMask = num;
                lParam.dwEffects = num2;
                lParam.yHeight = (int) (value.SizeInPoints * 20f);
                lParam.bCharSet = logfont.lfCharSet;
                lParam.bPitchAndFamily = logfont.lfPitchAndFamily;
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x444, selectionOnly ? 1 : 4, lParam);
            }
            else
            {
                bytes = Encoding.Unicode.GetBytes(logfont.lfFaceName);
                System.Windows.Forms.NativeMethods.CHARFORMATW charformatw = new System.Windows.Forms.NativeMethods.CHARFORMATW();
                for (int j = 0; j < bytes.Length; j++)
                {
                    charformatw.szFaceName[j] = bytes[j];
                }
                charformatw.dwMask = num;
                charformatw.dwEffects = num2;
                charformatw.yHeight = (int) (value.SizeInPoints * 20f);
                charformatw.bCharSet = logfont.lfCharSet;
                charformatw.bPitchAndFamily = logfont.lfPitchAndFamily;
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x444, selectionOnly ? 1 : 4, charformatw);
            }
        }

        private static void SetupLogPixels(IntPtr hDC)
        {
            bool flag = false;
            if (hDC == IntPtr.Zero)
            {
                hDC = System.Windows.Forms.UnsafeNativeMethods.GetDC(System.Windows.Forms.NativeMethods.NullHandleRef);
                flag = true;
            }
            if (hDC != IntPtr.Zero)
            {
                logPixelsX = System.Windows.Forms.UnsafeNativeMethods.GetDeviceCaps(new HandleRef(null, hDC), 0x58);
                logPixelsY = System.Windows.Forms.UnsafeNativeMethods.GetDeviceCaps(new HandleRef(null, hDC), 90);
                if (flag)
                {
                    System.Windows.Forms.UnsafeNativeMethods.ReleaseDC(System.Windows.Forms.NativeMethods.NullHandleRef, new HandleRef(null, hDC));
                }
            }
        }

        private void StreamIn(Stream data, int flags)
        {
            if ((flags & 0x8000) == 0)
            {
                System.Windows.Forms.NativeMethods.CHARRANGE lParam = new System.Windows.Forms.NativeMethods.CHARRANGE();
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x437, 0, lParam);
            }
            try
            {
                this.editStream = data;
                if ((flags & 2) != 0)
                {
                    long position = this.editStream.Position;
                    byte[] buffer = new byte[SZ_RTF_TAG.Length];
                    this.editStream.Read(buffer, (int) position, SZ_RTF_TAG.Length);
                    string str = Encoding.Default.GetString(buffer);
                    if (!SZ_RTF_TAG.Equals(str))
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidFileFormat"));
                    }
                    this.editStream.Position = position;
                }
                int num2 = 0;
                System.Windows.Forms.NativeMethods.EDITSTREAM es = new System.Windows.Forms.NativeMethods.EDITSTREAM();
                if ((flags & 0x10) != 0)
                {
                    num2 = 9;
                }
                else
                {
                    num2 = 5;
                }
                if ((flags & 2) != 0)
                {
                    num2 |= 0x40;
                }
                else
                {
                    num2 |= 0x10;
                }
                es.dwCookie = (IntPtr) num2;
                es.pfnCallback = new System.Windows.Forms.NativeMethods.EditStreamCallback(this.EditStreamProc);
                base.SendMessage(0x435, 0, 0x7fffffff);
                if (IntPtr.Size == 8)
                {
                    System.Windows.Forms.NativeMethods.EDITSTREAM64 editstream2 = this.ConvertToEDITSTREAM64(es);
                    System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x449, flags, editstream2);
                    es.dwError = this.GetErrorValue64(editstream2);
                }
                else
                {
                    System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x449, flags, es);
                }
                this.UpdateMaxLength();
                if (!this.GetProtectedError())
                {
                    if (es.dwError != 0)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("LoadTextError"));
                    }
                    base.SendMessage(0xb9, -1, 0);
                    base.SendMessage(0xba, 0, 0);
                }
            }
            finally
            {
                this.editStream = null;
            }
        }

        private void StreamIn(string str, int flags)
        {
            if (str.Length == 0)
            {
                if ((0x8000 & flags) != 0)
                {
                    base.SendMessage(0x303, 0, 0);
                    this.ProtectedError = false;
                }
                else
                {
                    base.SendMessage(12, 0, "");
                }
            }
            else
            {
                byte[] bytes;
                int index = str.IndexOf('\0');
                if (index != -1)
                {
                    str = str.Substring(0, index);
                }
                if ((flags & 0x10) != 0)
                {
                    bytes = Encoding.Unicode.GetBytes(str);
                }
                else
                {
                    bytes = Encoding.Default.GetBytes(str);
                }
                this.editStream = new MemoryStream(bytes.Length);
                this.editStream.Write(bytes, 0, bytes.Length);
                this.editStream.Position = 0L;
                this.StreamIn(this.editStream, flags);
            }
        }

        private string StreamOut(int flags)
        {
            Stream data = new MemoryStream();
            this.StreamOut(data, flags, false);
            data.Position = 0L;
            int length = (int) data.Length;
            string str = string.Empty;
            if (length > 0)
            {
                byte[] buffer = new byte[length];
                data.Read(buffer, 0, length);
                if ((flags & 0x10) != 0)
                {
                    str = Encoding.Unicode.GetString(buffer, 0, buffer.Length);
                }
                else
                {
                    str = Encoding.Default.GetString(buffer, 0, buffer.Length);
                }
                if (!string.IsNullOrEmpty(str) && (str[str.Length - 1] == '\0'))
                {
                    str = str.Substring(0, str.Length - 1);
                }
            }
            return str;
        }

        private void StreamOut(Stream data, int flags, bool includeCrLfs)
        {
            this.editStream = data;
            try
            {
                int num = 0;
                System.Windows.Forms.NativeMethods.EDITSTREAM es = new System.Windows.Forms.NativeMethods.EDITSTREAM();
                if ((flags & 0x10) != 0)
                {
                    num = 10;
                }
                else
                {
                    num = 6;
                }
                if ((flags & 2) != 0)
                {
                    num |= 0x40;
                }
                else if (includeCrLfs)
                {
                    num |= 0x20;
                }
                else
                {
                    num |= 0x10;
                }
                es.dwCookie = (IntPtr) num;
                es.pfnCallback = new System.Windows.Forms.NativeMethods.EditStreamCallback(this.EditStreamProc);
                if (IntPtr.Size == 8)
                {
                    System.Windows.Forms.NativeMethods.EDITSTREAM64 lParam = this.ConvertToEDITSTREAM64(es);
                    System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x44a, flags, lParam);
                    es.dwError = this.GetErrorValue64(lParam);
                }
                else
                {
                    System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x44a, flags, es);
                }
                if (es.dwError != 0)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("SaveTextError"));
                }
            }
            finally
            {
                this.editStream = null;
            }
        }

        private static int Twip2Pixel(IntPtr hDC, int v, bool xDirection)
        {
            SetupLogPixels(hDC);
            int num = xDirection ? logPixelsX : logPixelsY;
            return (int) (((((double) v) / 20.0) / 72.0) * num);
        }

        internal override void UpdateMaxLength()
        {
            if (base.IsHandleCreated)
            {
                base.SendMessage(0x435, 0, this.MaxLength);
            }
        }

        private void UpdateOleCallback()
        {
            if (base.IsHandleCreated)
            {
                if (this.oleCallback == null)
                {
                    bool flag = false;
                    try
                    {
                        System.Windows.Forms.IntSecurity.UnmanagedCode.Demand();
                        flag = true;
                    }
                    catch (SecurityException)
                    {
                        flag = false;
                    }
                    if (flag)
                    {
                        this.AllowOleObjects = true;
                    }
                    else
                    {
                        this.AllowOleObjects = 0 != ((int) ((long) base.SendMessage(0x50e, 0, 1)));
                    }
                    this.oleCallback = this.CreateRichEditOleCallback();
                    IntPtr iUnknownForObject = Marshal.GetIUnknownForObject(this.oleCallback);
                    try
                    {
                        IntPtr ptr2;
                        Guid gUID = typeof(System.Windows.Forms.UnsafeNativeMethods.IRichEditOleCallback).GUID;
                        Marshal.QueryInterface(iUnknownForObject, ref gUID, out ptr2);
                        try
                        {
                            System.Windows.Forms.UnsafeNativeMethods.SendCallbackMessage(new HandleRef(this, base.Handle), 0x446, IntPtr.Zero, ptr2);
                        }
                        finally
                        {
                            Marshal.Release(ptr2);
                        }
                    }
                    finally
                    {
                        Marshal.Release(iUnknownForObject);
                    }
                }
                System.Windows.Forms.UnsafeNativeMethods.DragAcceptFiles(new HandleRef(this, base.Handle), false);
            }
        }

        private void UserPreferenceChangedHandler(object o, UserPreferenceChangedEventArgs e)
        {
            if (base.IsHandleCreated)
            {
                if (this.BackColor.IsSystemColor)
                {
                    base.SendMessage(0x443, 0, ColorTranslator.ToWin32(this.BackColor));
                }
                if (this.ForeColor.IsSystemColor)
                {
                    this.InternalSetForeColor(this.ForeColor);
                }
            }
        }

        private void WmReflectCommand(ref Message m)
        {
            if ((m.LParam == base.Handle) && !base.GetState(0x40000))
            {
                switch (System.Windows.Forms.NativeMethods.Util.HIWORD(m.WParam))
                {
                    case 0x601:
                        this.OnHScroll(EventArgs.Empty);
                        return;

                    case 0x602:
                        this.OnVScroll(EventArgs.Empty);
                        return;
                }
                base.WndProc(ref m);
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        internal void WmReflectNotify(ref Message m)
        {
            if (!(m.HWnd == base.Handle))
            {
                base.WndProc(ref m);
            }
            else
            {
                System.Windows.Forms.NativeMethods.ENPROTECTED enprotected;
                System.Windows.Forms.NativeMethods.NMHDR lParam = (System.Windows.Forms.NativeMethods.NMHDR) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.NMHDR));
                int code = lParam.code;
                switch (code)
                {
                    case 0x701:
                        if (!this.CallOnContentsResized)
                        {
                            System.Windows.Forms.NativeMethods.REQRESIZE reqresize = (System.Windows.Forms.NativeMethods.REQRESIZE) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.REQRESIZE));
                            if (base.BorderStyle == BorderStyle.Fixed3D)
                            {
                                reqresize.rc.bottom++;
                            }
                            this.OnContentsResized(new ContentsResizedEventArgs(Rectangle.FromLTRB(reqresize.rc.left, reqresize.rc.top, reqresize.rc.right, reqresize.rc.bottom)));
                        }
                        return;

                    case 0x702:
                    {
                        System.Windows.Forms.NativeMethods.SELCHANGE selChange = (System.Windows.Forms.NativeMethods.SELCHANGE) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.SELCHANGE));
                        this.WmSelectionChange(selChange);
                        return;
                    }
                    case 0x703:
                    {
                        System.Windows.Forms.NativeMethods.ENDROPFILES wrapper = (System.Windows.Forms.NativeMethods.ENDROPFILES) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.ENDROPFILES));
                        StringBuilder lpszFile = new StringBuilder(260);
                        System.Windows.Forms.UnsafeNativeMethods.DragQueryFile(new HandleRef(wrapper, wrapper.hDrop), 0, lpszFile, 260);
                        try
                        {
                            this.LoadFile(lpszFile.ToString(), RichTextBoxStreamType.RichText);
                        }
                        catch
                        {
                            try
                            {
                                this.LoadFile(lpszFile.ToString(), RichTextBoxStreamType.PlainText);
                            }
                            catch
                            {
                            }
                        }
                        m.Result = (IntPtr) 1;
                        return;
                    }
                    case 0x704:
                        if (IntPtr.Size != 8)
                        {
                            enprotected = (System.Windows.Forms.NativeMethods.ENPROTECTED) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.ENPROTECTED));
                            break;
                        }
                        enprotected = this.ConvertFromENPROTECTED64((System.Windows.Forms.NativeMethods.ENPROTECTED64) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.ENPROTECTED64)));
                        break;

                    default:
                        if (code != 0x70b)
                        {
                            base.WndProc(ref m);
                            return;
                        }
                        this.EnLinkMsgHandler(ref m);
                        return;
                }
                switch (enprotected.msg)
                {
                    case 0x447:
                    case 0xc2:
                        break;

                    case 0x449:
                        if ((((int) ((long) enprotected.wParam)) & 0x8000) != 0)
                        {
                            break;
                        }
                        m.Result = IntPtr.Zero;
                        return;

                    case 0x444:
                    {
                        System.Windows.Forms.NativeMethods.CHARFORMATA charformata = (System.Windows.Forms.NativeMethods.CHARFORMATA) System.Windows.Forms.UnsafeNativeMethods.PtrToStructure(enprotected.lParam, typeof(System.Windows.Forms.NativeMethods.CHARFORMATA));
                        if ((charformata.dwMask & 0x10) == 0)
                        {
                            break;
                        }
                        m.Result = IntPtr.Zero;
                        return;
                    }
                    case 0x435:
                    case 12:
                    case 0x301:
                        m.Result = IntPtr.Zero;
                        return;

                    default:
                        System.Windows.Forms.SafeNativeMethods.MessageBeep(0);
                        break;
                }
                this.OnProtected(EventArgs.Empty);
                m.Result = (IntPtr) 1;
            }
        }

        private void WmSelectionChange(System.Windows.Forms.NativeMethods.SELCHANGE selChange)
        {
            int cpMin = selChange.chrg.cpMin;
            int cpMax = selChange.chrg.cpMax;
            short seltyp = (short) selChange.seltyp;
            if (((base.ImeMode == ImeMode.Hangul) || (base.ImeMode == ImeMode.HangulFull)) && (((int) ((long) base.SendMessage(0x47a, 0, 0))) != 0))
            {
                int windowTextLength = System.Windows.Forms.SafeNativeMethods.GetWindowTextLength(new HandleRef(this, base.Handle));
                if ((cpMin == cpMax) && (windowTextLength == this.MaxLength))
                {
                    base.SendMessage(8, 0, 0);
                    base.SendMessage(7, 0, 0);
                    System.Windows.Forms.UnsafeNativeMethods.PostMessage(new HandleRef(this, base.Handle), 0xb1, cpMax - 1, cpMax);
                }
            }
            if (((cpMin != this.curSelStart) || (cpMax != this.curSelEnd)) || (seltyp != this.curSelType))
            {
                this.curSelStart = cpMin;
                this.curSelEnd = cpMax;
                this.curSelType = seltyp;
                this.OnSelectionChanged(EventArgs.Empty);
            }
        }

        private void WmSetFont(ref Message m)
        {
            try
            {
                this.SuppressTextChangedEvent = true;
                base.WndProc(ref m);
            }
            finally
            {
                this.SuppressTextChangedEvent = false;
            }
            this.InternalSetForeColor(this.ForeColor);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            int num;
            switch (m.Msg)
            {
                case 0x3d:
                    base.WndProc(ref m);
                    if (((int) ((long) m.LParam)) != -12)
                    {
                        break;
                    }
                    m.Result = (Marshal.SystemDefaultCharSize == 1) ? ((IntPtr) 0x1001d) : ((IntPtr) 0x1001e);
                    return;

                case 0x87:
                    base.WndProc(ref m);
                    m.Result = base.AcceptsTab ? ((IntPtr) (((int) ((long) m.Result)) | 2)) : ((IntPtr) (((int) ((long) m.Result)) & -3));
                    return;

                case 0x20:
                    this.LinkCursor = false;
                    this.DefWndProc(ref m);
                    if (this.LinkCursor && !this.Cursor.Equals(Cursors.WaitCursor))
                    {
                        System.Windows.Forms.UnsafeNativeMethods.SetCursor(new HandleRef(Cursors.Hand, Cursors.Hand.Handle));
                        m.Result = (IntPtr) 1;
                        return;
                    }
                    base.WndProc(ref m);
                    return;

                case 0x30:
                    this.WmSetFont(ref m);
                    return;

                case 0x114:
                    base.WndProc(ref m);
                    num = System.Windows.Forms.NativeMethods.Util.LOWORD(m.WParam);
                    if (num == 5)
                    {
                        this.OnHScroll(EventArgs.Empty);
                    }
                    if (num != 4)
                    {
                        break;
                    }
                    this.OnHScroll(EventArgs.Empty);
                    return;

                case 0x115:
                    base.WndProc(ref m);
                    num = System.Windows.Forms.NativeMethods.Util.LOWORD(m.WParam);
                    if (num != 5)
                    {
                        if (num != 4)
                        {
                            break;
                        }
                        this.OnVScroll(EventArgs.Empty);
                        return;
                    }
                    this.OnVScroll(EventArgs.Empty);
                    return;

                case 0x205:
                {
                    bool style = base.GetStyle(ControlStyles.UserMouse);
                    base.SetStyle(ControlStyles.UserMouse, true);
                    base.WndProc(ref m);
                    base.SetStyle(ControlStyles.UserMouse, style);
                    return;
                }
                case 0x282:
                    this.OnImeChange(EventArgs.Empty);
                    base.WndProc(ref m);
                    return;

                case 0x204e:
                    this.WmReflectNotify(ref m);
                    return;

                case 0x2111:
                    this.WmReflectCommand(ref m);
                    return;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        [Browsable(false)]
        public override bool AllowDrop
        {
            get
            {
                return (this.richTextBoxFlags[allowOleDropSection] != 0);
            }
            set
            {
                if (value)
                {
                    try
                    {
                        System.Windows.Forms.IntSecurity.ClipboardRead.Demand();
                    }
                    catch (Exception exception)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DragDropRegFailed"), exception);
                    }
                }
                this.richTextBoxFlags[allowOleDropSection] = value ? 1 : 0;
                this.UpdateOleCallback();
            }
        }

        internal bool AllowOleObjects
        {
            get
            {
                return (this.richTextBoxFlags[allowOleObjectsSection] != 0);
            }
            set
            {
                this.richTextBoxFlags[allowOleObjectsSection] = value ? 1 : 0;
            }
        }

        [RefreshProperties(RefreshProperties.Repaint), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), EditorBrowsable(EditorBrowsableState.Never), Browsable(false), DefaultValue(false)]
        public override bool AutoSize
        {
            get
            {
                return base.AutoSize;
            }
            set
            {
                base.AutoSize = value;
            }
        }

        [DefaultValue(false), System.Windows.Forms.SRDescription("RichTextBoxAutoWordSelection"), System.Windows.Forms.SRCategory("CatBehavior")]
        public bool AutoWordSelection
        {
            get
            {
                return (this.richTextBoxFlags[autoWordSelectionSection] != 0);
            }
            set
            {
                this.richTextBoxFlags[autoWordSelectionSection] = value ? 1 : 0;
                if (base.IsHandleCreated)
                {
                    base.SendMessage(0x44d, value ? 2 : 4, 1);
                }
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

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
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

        [System.Windows.Forms.SRDescription("RichTextBoxBulletIndent"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(0), Localizable(true)]
        public int BulletIndent
        {
            get
            {
                return this.bulletIndent;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("BulletIndent", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "BulletIndent", value.ToString(CultureInfo.CurrentCulture) }));
                }
                this.bulletIndent = value;
                if (base.IsHandleCreated && this.SelectionBullet)
                {
                    this.SelectionBullet = true;
                }
            }
        }

        private bool CallOnContentsResized
        {
            get
            {
                return (this.richTextBoxFlags[callOnContentsResizedSection] != 0);
            }
            set
            {
                this.richTextBoxFlags[callOnContentsResizedSection] = value ? 1 : 0;
            }
        }

        internal override bool CanRaiseTextChangedEvent
        {
            get
            {
                return !this.SuppressTextChangedEvent;
            }
        }

        [Browsable(false), System.Windows.Forms.SRDescription("RichTextBoxCanRedoDescr"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool CanRedo
        {
            get
            {
                return (base.IsHandleCreated && (((int) ((long) base.SendMessage(0x455, 0, 0))) != 0));
            }
        }

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                if (moduleHandle == IntPtr.Zero)
                {
                    FileVersionInfo versionInfo;
                    moduleHandle = System.Windows.Forms.UnsafeNativeMethods.LoadLibrary("RichEd20.DLL");
                    int error = Marshal.GetLastWin32Error();
                    if (((long) moduleHandle) < 0x20L)
                    {
                        throw new Win32Exception(error, System.Windows.Forms.SR.GetString("LoadDLLError", new object[] { "RichEd20.DLL" }));
                    }
                    StringBuilder buffer = new StringBuilder(260);
                    System.Windows.Forms.UnsafeNativeMethods.GetModuleFileName(new HandleRef(null, moduleHandle), buffer, buffer.Capacity);
                    string path = buffer.ToString();
                    new FileIOPermission(FileIOPermissionAccess.Read, path).Assert();
                    try
                    {
                        versionInfo = FileVersionInfo.GetVersionInfo(path);
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                    if ((versionInfo != null) && !string.IsNullOrEmpty(versionInfo.ProductVersion))
                    {
                        int num2;
                        char ch = versionInfo.ProductVersion[0];
                        if (int.TryParse(ch.ToString(), out num2))
                        {
                            richEditMajorVersion = num2;
                        }
                    }
                }
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                if (Marshal.SystemDefaultCharSize == 1)
                {
                    createParams.ClassName = "RichEdit20A";
                }
                else
                {
                    createParams.ClassName = "RichEdit20W";
                }
                if (this.Multiline)
                {
                    if (((this.ScrollBars & RichTextBoxScrollBars.Horizontal) != RichTextBoxScrollBars.None) && !base.WordWrap)
                    {
                        createParams.Style |= 0x100000;
                        if ((this.ScrollBars & ((RichTextBoxScrollBars) 0x10)) != RichTextBoxScrollBars.None)
                        {
                            createParams.Style |= 0x2000;
                        }
                    }
                    if ((this.ScrollBars & RichTextBoxScrollBars.Vertical) != RichTextBoxScrollBars.None)
                    {
                        createParams.Style |= 0x200000;
                        if ((this.ScrollBars & ((RichTextBoxScrollBars) 0x10)) != RichTextBoxScrollBars.None)
                        {
                            createParams.Style |= 0x2000;
                        }
                    }
                }
                if ((BorderStyle.FixedSingle == base.BorderStyle) && ((createParams.Style & 0x800000) != 0))
                {
                    createParams.Style &= -8388609;
                    createParams.ExStyle |= 0x200;
                }
                return createParams;
            }
        }

        protected override Size DefaultSize
        {
            get
            {
                return new Size(100, 0x60);
            }
        }

        [System.Windows.Forms.SRDescription("RichTextBoxDetectURLs"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(true)]
        public bool DetectUrls
        {
            get
            {
                return (this.richTextBoxFlags[autoUrlDetectSection] != 0);
            }
            set
            {
                if (value != this.DetectUrls)
                {
                    this.richTextBoxFlags[autoUrlDetectSection] = value ? 1 : 0;
                    if (base.IsHandleCreated)
                    {
                        base.SendMessage(0x45b, value ? 1 : 0, 0);
                        base.RecreateHandle();
                    }
                }
            }
        }

        [System.Windows.Forms.SRDescription("RichTextBoxEnableAutoDragDrop"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(false)]
        public bool EnableAutoDragDrop
        {
            get
            {
                return (this.richTextBoxFlags[enableAutoDragDropSection] != 0);
            }
            set
            {
                if (value)
                {
                    try
                    {
                        System.Windows.Forms.IntSecurity.ClipboardRead.Demand();
                    }
                    catch (Exception exception)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DragDropRegFailed"), exception);
                    }
                }
                this.richTextBoxFlags[enableAutoDragDropSection] = value ? 1 : 0;
                this.UpdateOleCallback();
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
                if (base.IsHandleCreated)
                {
                    if (System.Windows.Forms.SafeNativeMethods.GetWindowTextLength(new HandleRef(this, base.Handle)) > 0)
                    {
                        if (value == null)
                        {
                            base.Font = null;
                            this.SetCharFormatFont(false, this.Font);
                            return;
                        }
                        try
                        {
                            System.Drawing.Font charFormatFont = this.GetCharFormatFont(false);
                            if ((charFormatFont == null) || !charFormatFont.Equals(value))
                            {
                                this.SetCharFormatFont(false, value);
                                this.CallOnContentsResized = true;
                                base.Font = this.GetCharFormatFont(false);
                            }
                            return;
                        }
                        finally
                        {
                            this.CallOnContentsResized = false;
                        }
                    }
                    base.Font = value;
                }
                else
                {
                    base.Font = value;
                }
            }
        }

        public override System.Drawing.Color ForeColor
        {
            get
            {
                return base.ForeColor;
            }
            set
            {
                if (base.IsHandleCreated)
                {
                    if (this.InternalSetForeColor(value))
                    {
                        base.ForeColor = value;
                    }
                }
                else
                {
                    base.ForeColor = value;
                }
            }
        }

        private bool InConstructor
        {
            get
            {
                return (this.richTextBoxFlags[fInCtorSection] != 0);
            }
            set
            {
                this.richTextBoxFlags[fInCtorSection] = value ? 1 : 0;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public RichTextBoxLanguageOptions LanguageOption
        {
            get
            {
                if (base.IsHandleCreated)
                {
                    return (RichTextBoxLanguageOptions) ((int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x479, 0, 0));
                }
                return this.languageOption;
            }
            set
            {
                if (this.LanguageOption != value)
                {
                    this.languageOption = value;
                    if (base.IsHandleCreated)
                    {
                        System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x478, 0, (int) value);
                    }
                }
            }
        }

        private bool LinkCursor
        {
            get
            {
                return (this.richTextBoxFlags[linkcursorSection] != 0);
            }
            set
            {
                this.richTextBoxFlags[linkcursorSection] = value ? 1 : 0;
            }
        }

        [DefaultValue(0x7fffffff)]
        public override int MaxLength
        {
            get
            {
                return base.MaxLength;
            }
            set
            {
                base.MaxLength = value;
            }
        }

        [DefaultValue(true)]
        public override bool Multiline
        {
            get
            {
                return base.Multiline;
            }
            set
            {
                base.Multiline = value;
            }
        }

        private bool ProtectedError
        {
            get
            {
                return (this.richTextBoxFlags[protectedErrorSection] != 0);
            }
            set
            {
                this.richTextBoxFlags[protectedErrorSection] = value ? 1 : 0;
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("RichTextBoxRedoActionNameDescr")]
        public string RedoActionName
        {
            get
            {
                if (!this.CanRedo)
                {
                    return "";
                }
                int actionID = (int) ((long) base.SendMessage(0x457, 0, 0));
                return this.GetEditorActionName(actionID);
            }
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

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DefaultValue(true)]
        public bool RichTextShortcutsEnabled
        {
            get
            {
                return (this.richTextBoxFlags[richTextShortcutsEnabledSection] != 0);
            }
            set
            {
                if (shortcutsToDisable == null)
                {
                    shortcutsToDisable = new int[] { 0x2004c, 0x20052, 0x20045, 0x2004a };
                }
                this.richTextBoxFlags[richTextShortcutsEnabledSection] = value ? 1 : 0;
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), Localizable(true), System.Windows.Forms.SRDescription("RichTextBoxRightMargin"), DefaultValue(0)]
        public int RightMargin
        {
            get
            {
                return this.rightMargin;
            }
            set
            {
                if (this.rightMargin != value)
                {
                    if (value < 0)
                    {
                        object[] args = new object[] { "RightMargin", value.ToString(CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture) };
                        throw new ArgumentOutOfRangeException("RightMargin", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", args));
                    }
                    this.rightMargin = value;
                    if (value == 0)
                    {
                        base.RecreateHandle();
                    }
                    else if (base.IsHandleCreated)
                    {
                        IntPtr wparam = System.Windows.Forms.UnsafeNativeMethods.CreateIC("DISPLAY", null, null, new HandleRef(null, IntPtr.Zero));
                        try
                        {
                            base.SendMessage(0x448, wparam, (IntPtr) Pixel2Twip(wparam, value, true));
                        }
                        finally
                        {
                            if (wparam != IntPtr.Zero)
                            {
                                System.Windows.Forms.UnsafeNativeMethods.DeleteDC(new HandleRef(null, wparam));
                            }
                        }
                    }
                }
            }
        }

        [System.Windows.Forms.SRDescription("RichTextBoxRTF"), RefreshProperties(RefreshProperties.All), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public string Rtf
        {
            get
            {
                if (base.IsHandleCreated)
                {
                    return this.StreamOut(2);
                }
                if (this.textPlain != null)
                {
                    this.ForceHandleCreate();
                    return this.StreamOut(2);
                }
                return this.textRtf;
            }
            set
            {
                if (value == null)
                {
                    value = "";
                }
                if (!value.Equals(this.Rtf))
                {
                    this.ForceHandleCreate();
                    this.textRtf = value;
                    this.StreamIn(value, 2);
                    if (this.CanRaiseTextChangedEvent)
                    {
                        this.OnTextChanged(EventArgs.Empty);
                    }
                }
            }
        }

        [System.Windows.Forms.SRDescription("RichTextBoxScrollBars"), System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(3), Localizable(true)]
        public RichTextBoxScrollBars ScrollBars
        {
            get
            {
                return (RichTextBoxScrollBars) this.richTextBoxFlags[scrollBarsSection];
            }
            set
            {
                int[] enumValues = new int[7];
                enumValues[0] = 3;
                enumValues[2] = 1;
                enumValues[3] = 2;
                enumValues[4] = 0x11;
                enumValues[5] = 0x12;
                enumValues[6] = 0x13;
                if (!System.Windows.Forms.ClientUtils.IsEnumValid_NotSequential(value, (int) value, enumValues))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(RichTextBoxScrollBars));
                }
                if (value != this.ScrollBars)
                {
                    using (LayoutTransaction.CreateTransactionIf(this.AutoSize, this.ParentInternal, this, PropertyNames.ScrollBars))
                    {
                        this.richTextBoxFlags[scrollBarsSection] = (int) value;
                        base.RecreateHandle();
                    }
                }
            }
        }

        [Browsable(false), System.Windows.Forms.SRDescription("RichTextBoxSelRTF"), DefaultValue(""), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SelectedRtf
        {
            get
            {
                this.ForceHandleCreate();
                return this.StreamOut(0x8002);
            }
            set
            {
                this.ForceHandleCreate();
                if (value == null)
                {
                    value = "";
                }
                this.StreamIn(value, 0x8002);
            }
        }

        [System.Windows.Forms.SRDescription("RichTextBoxSelText"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue(""), Browsable(false)]
        public override string SelectedText
        {
            get
            {
                this.ForceHandleCreate();
                return this.StreamOut(0x8011);
            }
            set
            {
                this.ForceHandleCreate();
                this.StreamIn(value, 0x8011);
            }
        }

        [System.Windows.Forms.SRDescription("RichTextBoxSelAlignment"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue(0), Browsable(false)]
        public HorizontalAlignment SelectionAlignment
        {
            get
            {
                HorizontalAlignment left = HorizontalAlignment.Left;
                this.ForceHandleCreate();
                System.Windows.Forms.NativeMethods.PARAFORMAT lParam = new System.Windows.Forms.NativeMethods.PARAFORMAT {
                    rgxTabs = new int[0x20]
                };
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x43d, 0, lParam);
                if ((8 & lParam.dwMask) != 0)
                {
                    switch (lParam.wAlignment)
                    {
                        case 1:
                            return HorizontalAlignment.Left;

                        case 2:
                            return HorizontalAlignment.Right;

                        case 3:
                            return HorizontalAlignment.Center;
                    }
                }
                return left;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(HorizontalAlignment));
                }
                this.ForceHandleCreate();
                System.Windows.Forms.NativeMethods.PARAFORMAT lParam = new System.Windows.Forms.NativeMethods.PARAFORMAT {
                    dwMask = 8
                };
                switch (value)
                {
                    case HorizontalAlignment.Left:
                        lParam.wAlignment = 1;
                        break;

                    case HorizontalAlignment.Right:
                        lParam.wAlignment = 2;
                        break;

                    case HorizontalAlignment.Center:
                        lParam.wAlignment = 3;
                        break;
                }
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x447, 0, lParam);
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("RichTextBoxSelBackColor")]
        public System.Drawing.Color SelectionBackColor
        {
            get
            {
                System.Drawing.Color empty = System.Drawing.Color.Empty;
                if (base.IsHandleCreated)
                {
                    System.Windows.Forms.NativeMethods.CHARFORMAT2A charformata = this.GetCharFormat2(true);
                    if ((charformata.dwEffects & 0x4000000) != 0)
                    {
                        return this.BackColor;
                    }
                    if ((charformata.dwMask & 0x4000000) != 0)
                    {
                        empty = ColorTranslator.FromOle(charformata.crBackColor);
                    }
                    return empty;
                }
                return this.selectionBackColorToSetOnHandleCreated;
            }
            set
            {
                this.selectionBackColorToSetOnHandleCreated = value;
                if (base.IsHandleCreated)
                {
                    System.Windows.Forms.NativeMethods.CHARFORMAT2A lParam = new System.Windows.Forms.NativeMethods.CHARFORMAT2A();
                    if (value == System.Drawing.Color.Empty)
                    {
                        lParam.dwEffects = 0x4000000;
                    }
                    else
                    {
                        lParam.dwMask = 0x4000000;
                        lParam.crBackColor = ColorTranslator.ToWin32(value);
                    }
                    System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x444, 1, lParam);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue(false), System.Windows.Forms.SRDescription("RichTextBoxSelBullet"), Browsable(false)]
        public bool SelectionBullet
        {
            get
            {
                RichTextBoxSelectionAttribute none = RichTextBoxSelectionAttribute.None;
                this.ForceHandleCreate();
                System.Windows.Forms.NativeMethods.PARAFORMAT lParam = new System.Windows.Forms.NativeMethods.PARAFORMAT {
                    rgxTabs = new int[0x20]
                };
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x43d, 0, lParam);
                if ((0x20 & lParam.dwMask) != 0)
                {
                    if (1 == lParam.wNumbering)
                    {
                        none = RichTextBoxSelectionAttribute.All;
                    }
                    return (none == RichTextBoxSelectionAttribute.All);
                }
                return false;
            }
            set
            {
                this.ForceHandleCreate();
                System.Windows.Forms.NativeMethods.PARAFORMAT lParam = new System.Windows.Forms.NativeMethods.PARAFORMAT {
                    dwMask = 0x24
                };
                if (!value)
                {
                    lParam.wNumbering = 0;
                    lParam.dxOffset = 0;
                }
                else
                {
                    lParam.wNumbering = 1;
                    lParam.dxOffset = Pixel2Twip(IntPtr.Zero, this.bulletIndent, true);
                }
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x447, 0, lParam);
            }
        }

        [System.Windows.Forms.SRDescription("RichTextBoxSelCharOffset"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue(0)]
        public int SelectionCharOffset
        {
            get
            {
                int v = 0;
                this.ForceHandleCreate();
                System.Windows.Forms.NativeMethods.CHARFORMATA charFormat = this.GetCharFormat(true);
                if ((charFormat.dwMask & 0x10000000) != 0)
                {
                    v = charFormat.yOffset;
                }
                else
                {
                    v = charFormat.yOffset;
                }
                return Twip2Pixel(IntPtr.Zero, v, false);
            }
            set
            {
                if ((value > 0x7d0) || (value < -2000))
                {
                    throw new ArgumentOutOfRangeException("SelectionCharOffset", System.Windows.Forms.SR.GetString("InvalidBoundArgument", new object[] { "SelectionCharOffset", value, -2000, 0x7d0 }));
                }
                this.ForceHandleCreate();
                System.Windows.Forms.NativeMethods.CHARFORMATA lParam = new System.Windows.Forms.NativeMethods.CHARFORMATA {
                    dwMask = 0x10000000,
                    yOffset = Pixel2Twip(IntPtr.Zero, value, false)
                };
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x444, 1, lParam);
            }
        }

        [System.Windows.Forms.SRDescription("RichTextBoxSelColor"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public System.Drawing.Color SelectionColor
        {
            get
            {
                System.Drawing.Color empty = System.Drawing.Color.Empty;
                this.ForceHandleCreate();
                System.Windows.Forms.NativeMethods.CHARFORMATA charFormat = this.GetCharFormat(true);
                if ((charFormat.dwMask & 0x40000000) != 0)
                {
                    empty = ColorTranslator.FromOle(charFormat.crTextColor);
                }
                return empty;
            }
            set
            {
                this.ForceHandleCreate();
                System.Windows.Forms.NativeMethods.CHARFORMATA charFormat = this.GetCharFormat(true);
                charFormat.dwMask = 0x40000000;
                charFormat.dwEffects = 0;
                charFormat.crTextColor = ColorTranslator.ToWin32(value);
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x444, 1, charFormat);
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("RichTextBoxSelFont")]
        public System.Drawing.Font SelectionFont
        {
            get
            {
                return this.GetCharFormatFont(true);
            }
            set
            {
                this.SetCharFormatFont(true, value);
            }
        }

        [DefaultValue(0), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("RichTextBoxSelHangingIndent")]
        public int SelectionHangingIndent
        {
            get
            {
                int v = 0;
                this.ForceHandleCreate();
                System.Windows.Forms.NativeMethods.PARAFORMAT lParam = new System.Windows.Forms.NativeMethods.PARAFORMAT {
                    rgxTabs = new int[0x20]
                };
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x43d, 0, lParam);
                if ((4 & lParam.dwMask) != 0)
                {
                    v = lParam.dxOffset;
                }
                return Twip2Pixel(IntPtr.Zero, v, true);
            }
            set
            {
                this.ForceHandleCreate();
                System.Windows.Forms.NativeMethods.PARAFORMAT lParam = new System.Windows.Forms.NativeMethods.PARAFORMAT {
                    dwMask = 4,
                    dxOffset = Pixel2Twip(IntPtr.Zero, value, true)
                };
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x447, 0, lParam);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue(0), System.Windows.Forms.SRDescription("RichTextBoxSelIndent"), Browsable(false)]
        public int SelectionIndent
        {
            get
            {
                int v = 0;
                this.ForceHandleCreate();
                System.Windows.Forms.NativeMethods.PARAFORMAT lParam = new System.Windows.Forms.NativeMethods.PARAFORMAT {
                    rgxTabs = new int[0x20]
                };
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x43d, 0, lParam);
                if ((1 & lParam.dwMask) != 0)
                {
                    v = lParam.dxStartIndent;
                }
                return Twip2Pixel(IntPtr.Zero, v, true);
            }
            set
            {
                this.ForceHandleCreate();
                System.Windows.Forms.NativeMethods.PARAFORMAT lParam = new System.Windows.Forms.NativeMethods.PARAFORMAT {
                    dwMask = 1,
                    dxStartIndent = Pixel2Twip(IntPtr.Zero, value, true)
                };
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x447, 0, lParam);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("TextBoxSelectionLengthDescr"), Browsable(false)]
        public override int SelectionLength
        {
            get
            {
                if (!base.IsHandleCreated)
                {
                    return base.SelectionLength;
                }
                return this.SelectedText.Length;
            }
            set
            {
                base.SelectionLength = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), DefaultValue(false), System.Windows.Forms.SRDescription("RichTextBoxSelProtected")]
        public bool SelectionProtected
        {
            get
            {
                this.ForceHandleCreate();
                return (this.GetCharFormat(0x10, 0x10) == RichTextBoxSelectionAttribute.All);
            }
            set
            {
                this.ForceHandleCreate();
                this.SetCharFormat(0x10, value ? 0x10 : 0, RichTextBoxSelectionAttribute.All);
            }
        }

        [Browsable(false), System.Windows.Forms.SRDescription("RichTextBoxSelRightIndent"), DefaultValue(0), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectionRightIndent
        {
            get
            {
                int v = 0;
                this.ForceHandleCreate();
                System.Windows.Forms.NativeMethods.PARAFORMAT lParam = new System.Windows.Forms.NativeMethods.PARAFORMAT {
                    rgxTabs = new int[0x20]
                };
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x43d, 0, lParam);
                if ((2 & lParam.dwMask) != 0)
                {
                    v = lParam.dxRightIndent;
                }
                return Twip2Pixel(IntPtr.Zero, v, true);
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("SelectionRightIndent", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", new object[] { "SelectionRightIndent", value, 0 }));
                }
                this.ForceHandleCreate();
                System.Windows.Forms.NativeMethods.PARAFORMAT lParam = new System.Windows.Forms.NativeMethods.PARAFORMAT {
                    dwMask = 2,
                    dxRightIndent = Pixel2Twip(IntPtr.Zero, value, true)
                };
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x447, 0, lParam);
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("RichTextBoxSelTabs")]
        public int[] SelectionTabs
        {
            get
            {
                int[] numArray = new int[0];
                this.ForceHandleCreate();
                System.Windows.Forms.NativeMethods.PARAFORMAT lParam = new System.Windows.Forms.NativeMethods.PARAFORMAT {
                    rgxTabs = new int[0x20]
                };
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x43d, 0, lParam);
                if ((0x10 & lParam.dwMask) != 0)
                {
                    numArray = new int[lParam.cTabCount];
                    for (int i = 0; i < lParam.cTabCount; i++)
                    {
                        numArray[i] = Twip2Pixel(IntPtr.Zero, lParam.rgxTabs[i], true);
                    }
                }
                return numArray;
            }
            set
            {
                if ((value != null) && (value.Length > 0x20))
                {
                    throw new ArgumentOutOfRangeException("SelectionTabs", System.Windows.Forms.SR.GetString("SelTabCountRange"));
                }
                this.ForceHandleCreate();
                System.Windows.Forms.NativeMethods.PARAFORMAT lParam = new System.Windows.Forms.NativeMethods.PARAFORMAT {
                    rgxTabs = new int[0x20]
                };
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x43d, 0, lParam);
                lParam.cTabCount = (value == null) ? ((short) 0) : ((short) value.Length);
                lParam.dwMask = 0x10;
                for (int i = 0; i < lParam.cTabCount; i++)
                {
                    lParam.rgxTabs[i] = Pixel2Twip(IntPtr.Zero, value[i], true);
                }
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x447, 0, lParam);
            }
        }

        [Browsable(false), System.Windows.Forms.SRDescription("RichTextBoxSelTypeDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RichTextBoxSelectionTypes SelectionType
        {
            get
            {
                this.ForceHandleCreate();
                if (this.SelectionLength > 0)
                {
                    int num = (int) ((long) base.SendMessage(0x442, 0, 0));
                    return (RichTextBoxSelectionTypes) num;
                }
                return RichTextBoxSelectionTypes.Empty;
            }
        }

        internal override bool SelectionUsesDbcsOffsetsInWin9x
        {
            get
            {
                return false;
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("RichTextBoxSelMargin"), DefaultValue(false)]
        public bool ShowSelectionMargin
        {
            get
            {
                return (this.richTextBoxFlags[showSelBarSection] != 0);
            }
            set
            {
                if (value != this.ShowSelectionMargin)
                {
                    this.richTextBoxFlags[showSelBarSection] = value ? 1 : 0;
                    if (base.IsHandleCreated)
                    {
                        base.SendMessage(0x44d, value ? 2 : 4, 0x1000000);
                    }
                }
            }
        }

        private bool SuppressTextChangedEvent
        {
            get
            {
                return (this.richTextBoxFlags[suppressTextChangedEventSection] != 0);
            }
            set
            {
                bool suppressTextChangedEvent = this.SuppressTextChangedEvent;
                if (value != suppressTextChangedEvent)
                {
                    this.richTextBoxFlags[suppressTextChangedEventSection] = value ? 1 : 0;
                    CommonProperties.xClearPreferredSizeCache(this);
                }
            }
        }

        [RefreshProperties(RefreshProperties.All), Localizable(true)]
        public override string Text
        {
            get
            {
                if (base.IsDisposed)
                {
                    return base.Text;
                }
                if (base.RecreatingHandle || base.GetAnyDisposingInHierarchy())
                {
                    return "";
                }
                if (!base.IsHandleCreated && (this.textRtf == null))
                {
                    if (this.textPlain != null)
                    {
                        return this.textPlain;
                    }
                    return base.Text;
                }
                this.ForceHandleCreate();
                return this.StreamOut(0x11);
            }
            set
            {
                using (LayoutTransaction.CreateTransactionIf(this.AutoSize, this.ParentInternal, this, PropertyNames.Text))
                {
                    this.textRtf = null;
                    if (!base.IsHandleCreated)
                    {
                        this.textPlain = value;
                    }
                    else
                    {
                        this.textPlain = null;
                        if (value == null)
                        {
                            value = "";
                        }
                        this.StreamIn(value, 0x11);
                        base.SendMessage(0xb9, 0, 0);
                    }
                }
            }
        }

        [Browsable(false)]
        public override int TextLength
        {
            get
            {
                System.Windows.Forms.NativeMethods.GETTEXTLENGTHEX wParam = new System.Windows.Forms.NativeMethods.GETTEXTLENGTHEX {
                    flags = 8
                };
                if (Marshal.SystemDefaultCharSize == 1)
                {
                    wParam.codepage = 0;
                }
                else
                {
                    wParam.codepage = 0x4b0;
                }
                return (int) ((long) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x45f, wParam, 0));
            }
        }

        [System.Windows.Forms.SRDescription("RichTextBoxUndoActionNameDescr"), System.Windows.Forms.SRCategory("CatBehavior"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string UndoActionName
        {
            get
            {
                if (!base.CanUndo)
                {
                    return "";
                }
                int actionID = (int) ((long) base.SendMessage(0x456, 0, 0));
                return this.GetEditorActionName(actionID);
            }
        }

        [System.Windows.Forms.SRDescription("RichTextBoxZoomFactor"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue((float) 1f), Localizable(true)]
        public float ZoomFactor
        {
            get
            {
                if (base.IsHandleCreated)
                {
                    int wparam = 0;
                    int lparam = 0;
                    base.SendMessage(0x4e0, ref wparam, ref lparam);
                    if ((wparam != 0) && (lparam != 0))
                    {
                        this.zoomMultiplier = ((float) wparam) / ((float) lparam);
                    }
                    else
                    {
                        this.zoomMultiplier = 1f;
                    }
                }
                return this.zoomMultiplier;
            }
            set
            {
                if (this.zoomMultiplier != value)
                {
                    if ((value <= 0.015625f) || (value >= 64f))
                    {
                        object[] args = new object[] { "ZoomFactor", value.ToString(CultureInfo.CurrentCulture), 0.015625f.ToString(CultureInfo.CurrentCulture), 64f.ToString(CultureInfo.CurrentCulture) };
                        throw new ArgumentOutOfRangeException("ZoomFactor", System.Windows.Forms.SR.GetString("InvalidExBoundArgument", args));
                    }
                    this.SendZoomFactor(value);
                }
            }
        }

        private class OleCallback : System.Windows.Forms.UnsafeNativeMethods.IRichEditOleCallback
        {
            private System.Windows.Forms.IDataObject lastDataObject;
            private DragDropEffects lastEffect;
            private RichTextBox owner;

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

            public int GetClipboardData(System.Windows.Forms.NativeMethods.CHARRANGE lpchrg, int reco, IntPtr lplpdataobj)
            {
                return -2147467263;
            }

            public int GetContextMenu(short seltype, IntPtr lpoleobj, System.Windows.Forms.NativeMethods.CHARRANGE lpchrg, out IntPtr hmenu)
            {
                int num;
                ContextMenu contextMenu = this.owner.ContextMenu;
                if ((contextMenu == null) || !this.owner.ShortcutsEnabled)
                {
                    hmenu = IntPtr.Zero;
                    goto Label_00B7;
                }
                contextMenu.sourceControl = this.owner;
                contextMenu.OnPopup(EventArgs.Empty);
                IntPtr handle = contextMenu.Handle;
                Menu menu = contextMenu;
            Label_004D:
                num = 0;
                int itemCount = menu.ItemCount;
                while (num < itemCount)
                {
                    if (menu.items[num].handle != IntPtr.Zero)
                    {
                        menu = menu.items[num];
                        break;
                    }
                    num++;
                }
                if (num != itemCount)
                {
                    goto Label_004D;
                }
                menu.handle = IntPtr.Zero;
                menu.created = false;
                if (menu != contextMenu)
                {
                    menu = ((MenuItem) menu).Menu;
                    goto Label_004D;
                }
                hmenu = handle;
            Label_00B7:
                return 0;
            }

            public int GetDragDropEffect(bool fDrag, int grfKeyState, ref int pdwEffect)
            {
                if (this.owner.AllowDrop || this.owner.EnableAutoDragDrop)
                {
                    if (fDrag && (grfKeyState == 0))
                    {
                        if (this.owner.EnableAutoDragDrop)
                        {
                            this.lastEffect = DragDropEffects.Move | DragDropEffects.Copy | DragDropEffects.Scroll;
                        }
                        else
                        {
                            this.lastEffect = DragDropEffects.None;
                        }
                    }
                    else if ((!fDrag && (this.lastDataObject != null)) && (grfKeyState != 0))
                    {
                        DragEventArgs drgevent = new DragEventArgs(this.lastDataObject, grfKeyState, Control.MousePosition.X, Control.MousePosition.Y, DragDropEffects.Move | DragDropEffects.Copy | DragDropEffects.Scroll, this.lastEffect);
                        if (this.lastEffect != DragDropEffects.None)
                        {
                            drgevent.Effect = ((grfKeyState & 8) == 8) ? DragDropEffects.Copy : DragDropEffects.Move;
                        }
                        this.owner.OnDragOver(drgevent);
                        this.lastEffect = drgevent.Effect;
                    }
                    pdwEffect = (int) this.lastEffect;
                }
                else
                {
                    pdwEffect = 0;
                }
                return 0;
            }

            public int GetInPlaceContext(IntPtr lplpFrame, IntPtr lplpDoc, IntPtr lpFrameInfo)
            {
                return -2147467263;
            }

            public int GetNewStorage(out System.Windows.Forms.UnsafeNativeMethods.IStorage storage)
            {
                if (!this.owner.AllowOleObjects)
                {
                    storage = null;
                    return -2147467259;
                }
                System.Windows.Forms.UnsafeNativeMethods.ILockBytes iLockBytes = System.Windows.Forms.UnsafeNativeMethods.CreateILockBytesOnHGlobal(System.Windows.Forms.NativeMethods.NullHandleRef, true);
                storage = System.Windows.Forms.UnsafeNativeMethods.StgCreateDocfileOnILockBytes(iLockBytes, 0x1012, 0);
                return 0;
            }

            public int QueryAcceptData(System.Runtime.InteropServices.ComTypes.IDataObject lpdataobj, IntPtr lpcfFormat, int reco, int fReally, IntPtr hMetaPict)
            {
                if (reco != 1)
                {
                    return -2147467263;
                }
                if (this.owner.AllowDrop || this.owner.EnableAutoDragDrop)
                {
                    MouseButtons mouseButtons = Control.MouseButtons;
                    Keys modifierKeys = Control.ModifierKeys;
                    int keyState = 0;
                    if ((mouseButtons & MouseButtons.Left) == MouseButtons.Left)
                    {
                        keyState |= 1;
                    }
                    if ((mouseButtons & MouseButtons.Right) == MouseButtons.Right)
                    {
                        keyState |= 2;
                    }
                    if ((mouseButtons & MouseButtons.Middle) == MouseButtons.Middle)
                    {
                        keyState |= 0x10;
                    }
                    if ((modifierKeys & Keys.Control) == Keys.Control)
                    {
                        keyState |= 8;
                    }
                    if ((modifierKeys & Keys.Shift) == Keys.Shift)
                    {
                        keyState |= 4;
                    }
                    this.lastDataObject = new DataObject(lpdataobj);
                    if (!this.owner.EnableAutoDragDrop)
                    {
                        this.lastEffect = DragDropEffects.None;
                    }
                    DragEventArgs drgevent = new DragEventArgs(this.lastDataObject, keyState, Control.MousePosition.X, Control.MousePosition.Y, DragDropEffects.Move | DragDropEffects.Copy | DragDropEffects.Scroll, this.lastEffect);
                    if (fReally == 0)
                    {
                        drgevent.Effect = ((keyState & 8) == 8) ? DragDropEffects.Copy : DragDropEffects.Move;
                        this.owner.OnDragEnter(drgevent);
                    }
                    else
                    {
                        this.owner.OnDragDrop(drgevent);
                        this.lastDataObject = null;
                    }
                    this.lastEffect = drgevent.Effect;
                    if (drgevent.Effect == DragDropEffects.None)
                    {
                        return -2147467259;
                    }
                    return 0;
                }
                this.lastDataObject = null;
                return -2147467259;
            }

            public int QueryInsertObject(ref Guid lpclsid, IntPtr lpstg, int cp)
            {
                string str;
                try
                {
                    System.Windows.Forms.IntSecurity.UnmanagedCode.Demand();
                    return 0;
                }
                catch (SecurityException)
                {
                }
                Guid pclsid = new Guid();
                if (!System.Windows.Forms.NativeMethods.Succeeded(System.Windows.Forms.UnsafeNativeMethods.ReadClassStg(new HandleRef(null, lpstg), ref pclsid)))
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
                return 0;
            }

            public int ShowContainerUI(int fShow)
            {
                return 0;
            }
        }
    }
}

