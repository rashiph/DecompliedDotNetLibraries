namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public sealed class HtmlDocument
    {
        internal static object EventClick = new object();
        internal static object EventContextMenuShowing = new object();
        internal static object EventFocusing = new object();
        internal static object EventLosingFocus = new object();
        internal static object EventMouseDown = new object();
        internal static object EventMouseLeave = new object();
        internal static object EventMouseMove = new object();
        internal static object EventMouseOver = new object();
        internal static object EventMouseUp = new object();
        internal static object EventStop = new object();
        private System.Windows.Forms.UnsafeNativeMethods.IHTMLDocument2 htmlDocument2;
        private HtmlShimManager shimManager;
        private static readonly int VariantSize = ((int) Marshal.OffsetOf(typeof(FindSizeOfVariant), "b"));

        public event HtmlElementEventHandler Click
        {
            add
            {
                this.DocumentShim.AddHandler(EventClick, value);
            }
            remove
            {
                this.DocumentShim.RemoveHandler(EventClick, value);
            }
        }

        public event HtmlElementEventHandler ContextMenuShowing
        {
            add
            {
                this.DocumentShim.AddHandler(EventContextMenuShowing, value);
            }
            remove
            {
                this.DocumentShim.RemoveHandler(EventContextMenuShowing, value);
            }
        }

        public event HtmlElementEventHandler Focusing
        {
            add
            {
                this.DocumentShim.AddHandler(EventFocusing, value);
            }
            remove
            {
                this.DocumentShim.RemoveHandler(EventFocusing, value);
            }
        }

        public event HtmlElementEventHandler LosingFocus
        {
            add
            {
                this.DocumentShim.AddHandler(EventLosingFocus, value);
            }
            remove
            {
                this.DocumentShim.RemoveHandler(EventLosingFocus, value);
            }
        }

        public event HtmlElementEventHandler MouseDown
        {
            add
            {
                this.DocumentShim.AddHandler(EventMouseDown, value);
            }
            remove
            {
                this.DocumentShim.RemoveHandler(EventMouseDown, value);
            }
        }

        public event HtmlElementEventHandler MouseLeave
        {
            add
            {
                this.DocumentShim.AddHandler(EventMouseLeave, value);
            }
            remove
            {
                this.DocumentShim.RemoveHandler(EventMouseLeave, value);
            }
        }

        public event HtmlElementEventHandler MouseMove
        {
            add
            {
                this.DocumentShim.AddHandler(EventMouseMove, value);
            }
            remove
            {
                this.DocumentShim.RemoveHandler(EventMouseMove, value);
            }
        }

        public event HtmlElementEventHandler MouseOver
        {
            add
            {
                this.DocumentShim.AddHandler(EventMouseOver, value);
            }
            remove
            {
                this.DocumentShim.RemoveHandler(EventMouseOver, value);
            }
        }

        public event HtmlElementEventHandler MouseUp
        {
            add
            {
                this.DocumentShim.AddHandler(EventMouseUp, value);
            }
            remove
            {
                this.DocumentShim.RemoveHandler(EventMouseUp, value);
            }
        }

        public event HtmlElementEventHandler Stop
        {
            add
            {
                this.DocumentShim.AddHandler(EventStop, value);
            }
            remove
            {
                this.DocumentShim.RemoveHandler(EventStop, value);
            }
        }

        [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        internal HtmlDocument(HtmlShimManager shimManager, System.Windows.Forms.UnsafeNativeMethods.IHTMLDocument doc)
        {
            this.htmlDocument2 = (System.Windows.Forms.UnsafeNativeMethods.IHTMLDocument2) doc;
            this.shimManager = shimManager;
        }

        internal static unsafe IntPtr ArrayToVARIANTVector(object[] args)
        {
            int length = args.Length;
            IntPtr ptr = Marshal.AllocCoTaskMem(length * VariantSize);
            byte* numPtr = (byte*) ptr;
            for (int i = 0; i < length; i++)
            {
                Marshal.GetNativeVariantForObject(args[i], (IntPtr) (numPtr + (VariantSize * i)));
            }
            return ptr;
        }

        public void AttachEventHandler(string eventName, EventHandler eventHandler)
        {
            HtmlDocumentShim documentShim = this.DocumentShim;
            if (documentShim != null)
            {
                documentShim.AttachEventHandler(eventName, eventHandler);
            }
        }

        private Color ColorFromObject(object oColor)
        {
            try
            {
                if (oColor is string)
                {
                    string name = oColor as string;
                    int index = name.IndexOf('#');
                    if (index >= 0)
                    {
                        string s = name.Substring(index + 1);
                        return Color.FromArgb(0xff, Color.FromArgb(int.Parse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture)));
                    }
                    return Color.FromName(name);
                }
                if (oColor is int)
                {
                    return Color.FromArgb(0xff, Color.FromArgb((int) oColor));
                }
            }
            catch (Exception exception)
            {
                if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception))
                {
                    throw;
                }
            }
            return Color.Empty;
        }

        public HtmlElement CreateElement(string elementTag)
        {
            System.Windows.Forms.UnsafeNativeMethods.IHTMLElement element = this.NativeHtmlDocument2.CreateElement(elementTag);
            if (element == null)
            {
                return null;
            }
            return new HtmlElement(this.ShimManager, element);
        }

        public void DetachEventHandler(string eventName, EventHandler eventHandler)
        {
            HtmlDocumentShim documentShim = this.DocumentShim;
            if (documentShim != null)
            {
                documentShim.DetachEventHandler(eventName, eventHandler);
            }
        }

        public override bool Equals(object obj)
        {
            return (this == ((HtmlDocument) obj));
        }

        public void ExecCommand(string command, bool showUI, object value)
        {
            this.NativeHtmlDocument2.ExecCommand(command, showUI, value);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void Focus()
        {
            ((System.Windows.Forms.UnsafeNativeMethods.IHTMLDocument4) this.NativeHtmlDocument2).Focus();
            ((System.Windows.Forms.UnsafeNativeMethods.IHTMLDocument4) this.NativeHtmlDocument2).Focus();
        }

        internal static unsafe void FreeVARIANTVector(IntPtr mem, int len)
        {
            byte* numPtr = (byte*) mem;
            for (int i = 0; i < len; i++)
            {
                System.Windows.Forms.SafeNativeMethods.VariantClear(new HandleRef(null, (IntPtr) (numPtr + (VariantSize * i))));
            }
            Marshal.FreeCoTaskMem(mem);
        }

        public HtmlElement GetElementById(string id)
        {
            System.Windows.Forms.UnsafeNativeMethods.IHTMLElement elementById = ((System.Windows.Forms.UnsafeNativeMethods.IHTMLDocument3) this.NativeHtmlDocument2).GetElementById(id);
            if (elementById == null)
            {
                return null;
            }
            return new HtmlElement(this.ShimManager, elementById);
        }

        public HtmlElement GetElementFromPoint(Point point)
        {
            System.Windows.Forms.UnsafeNativeMethods.IHTMLElement element = this.NativeHtmlDocument2.ElementFromPoint(point.X, point.Y);
            if (element == null)
            {
                return null;
            }
            return new HtmlElement(this.ShimManager, element);
        }

        public HtmlElementCollection GetElementsByTagName(string tagName)
        {
            System.Windows.Forms.UnsafeNativeMethods.IHTMLElementCollection elementsByTagName = ((System.Windows.Forms.UnsafeNativeMethods.IHTMLDocument3) this.NativeHtmlDocument2).GetElementsByTagName(tagName);
            if (elementsByTagName == null)
            {
                return new HtmlElementCollection(this.ShimManager);
            }
            return new HtmlElementCollection(this.ShimManager, elementsByTagName);
        }

        public override int GetHashCode()
        {
            if (this.htmlDocument2 != null)
            {
                return this.htmlDocument2.GetHashCode();
            }
            return 0;
        }

        public object InvokeScript(string scriptName)
        {
            return this.InvokeScript(scriptName, null);
        }

        public object InvokeScript(string scriptName, object[] args)
        {
            object obj2 = null;
            System.Windows.Forms.NativeMethods.tagDISPPARAMS pDispParams = new System.Windows.Forms.NativeMethods.tagDISPPARAMS {
                rgvarg = IntPtr.Zero
            };
            try
            {
                System.Windows.Forms.UnsafeNativeMethods.IDispatch script = this.NativeHtmlDocument2.GetScript() as System.Windows.Forms.UnsafeNativeMethods.IDispatch;
                if (script != null)
                {
                    Guid empty = Guid.Empty;
                    string[] rgszNames = new string[] { scriptName };
                    int[] rgDispId = new int[] { -1 };
                    if (!System.Windows.Forms.NativeMethods.Succeeded(script.GetIDsOfNames(ref empty, rgszNames, 1, System.Windows.Forms.SafeNativeMethods.GetThreadLCID(), rgDispId)) || (rgDispId[0] == -1))
                    {
                        return obj2;
                    }
                    if (args != null)
                    {
                        Array.Reverse(args);
                    }
                    pDispParams.rgvarg = (args == null) ? IntPtr.Zero : ArrayToVARIANTVector(args);
                    pDispParams.cArgs = (args == null) ? 0 : args.Length;
                    pDispParams.rgdispidNamedArgs = IntPtr.Zero;
                    pDispParams.cNamedArgs = 0;
                    object[] pVarResult = new object[1];
                    if (script.Invoke(rgDispId[0], ref empty, System.Windows.Forms.SafeNativeMethods.GetThreadLCID(), 1, pDispParams, pVarResult, new System.Windows.Forms.NativeMethods.tagEXCEPINFO(), null) == 0)
                    {
                        obj2 = pVarResult[0];
                    }
                }
                return obj2;
            }
            catch (Exception exception)
            {
                if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception))
                {
                    throw;
                }
            }
            finally
            {
                if (pDispParams.rgvarg != IntPtr.Zero)
                {
                    FreeVARIANTVector(pDispParams.rgvarg, args.Length);
                }
            }
            return obj2;
        }

        public static bool operator ==(HtmlDocument left, HtmlDocument right)
        {
            bool flag;
            if (object.ReferenceEquals(left, null) != object.ReferenceEquals(right, null))
            {
                return false;
            }
            if (object.ReferenceEquals(left, null))
            {
                return true;
            }
            IntPtr zero = IntPtr.Zero;
            IntPtr pUnk = IntPtr.Zero;
            try
            {
                zero = Marshal.GetIUnknownForObject(left.NativeHtmlDocument2);
                pUnk = Marshal.GetIUnknownForObject(right.NativeHtmlDocument2);
                flag = zero == pUnk;
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.Release(zero);
                }
                if (pUnk != IntPtr.Zero)
                {
                    Marshal.Release(pUnk);
                }
            }
            return flag;
        }

        public static bool operator !=(HtmlDocument left, HtmlDocument right)
        {
            return !(left == right);
        }

        public HtmlDocument OpenNew(bool replaceInHistory)
        {
            object name = replaceInHistory ? "replace" : "";
            object features = null;
            System.Windows.Forms.UnsafeNativeMethods.IHTMLDocument doc = this.NativeHtmlDocument2.Open("text/html", name, features, features) as System.Windows.Forms.UnsafeNativeMethods.IHTMLDocument;
            if (doc == null)
            {
                return null;
            }
            return new HtmlDocument(this.ShimManager, doc);
        }

        public void Write(string text)
        {
            object[] psarray = new object[] { text };
            this.NativeHtmlDocument2.Write(psarray);
        }

        public HtmlElement ActiveElement
        {
            get
            {
                System.Windows.Forms.UnsafeNativeMethods.IHTMLElement activeElement = this.NativeHtmlDocument2.GetActiveElement();
                if (activeElement == null)
                {
                    return null;
                }
                return new HtmlElement(this.ShimManager, activeElement);
            }
        }

        public Color ActiveLinkColor
        {
            get
            {
                Color empty = Color.Empty;
                try
                {
                    empty = this.ColorFromObject(this.NativeHtmlDocument2.GetAlinkColor());
                }
                catch (Exception exception)
                {
                    if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception))
                    {
                        throw;
                    }
                }
                return empty;
            }
            set
            {
                int c = ((value.R << 0x10) | (value.G << 8)) | value.B;
                this.NativeHtmlDocument2.SetAlinkColor(c);
            }
        }

        public HtmlElementCollection All
        {
            get
            {
                System.Windows.Forms.UnsafeNativeMethods.IHTMLElementCollection all = this.NativeHtmlDocument2.GetAll();
                if (all == null)
                {
                    return new HtmlElementCollection(this.ShimManager);
                }
                return new HtmlElementCollection(this.ShimManager, all);
            }
        }

        public Color BackColor
        {
            get
            {
                Color empty = Color.Empty;
                try
                {
                    empty = this.ColorFromObject(this.NativeHtmlDocument2.GetBgColor());
                }
                catch (Exception exception)
                {
                    if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception))
                    {
                        throw;
                    }
                }
                return empty;
            }
            set
            {
                int c = ((value.R << 0x10) | (value.G << 8)) | value.B;
                this.NativeHtmlDocument2.SetBgColor(c);
            }
        }

        public HtmlElement Body
        {
            get
            {
                System.Windows.Forms.UnsafeNativeMethods.IHTMLElement body = this.NativeHtmlDocument2.GetBody();
                if (body == null)
                {
                    return null;
                }
                return new HtmlElement(this.ShimManager, body);
            }
        }

        public string Cookie
        {
            get
            {
                return this.NativeHtmlDocument2.GetCookie();
            }
            set
            {
                this.NativeHtmlDocument2.SetCookie(value);
            }
        }

        public string DefaultEncoding
        {
            get
            {
                return this.NativeHtmlDocument2.GetDefaultCharset();
            }
        }

        private HtmlDocumentShim DocumentShim
        {
            get
            {
                if (this.ShimManager == null)
                {
                    return null;
                }
                HtmlDocumentShim documentShim = this.ShimManager.GetDocumentShim(this);
                if (documentShim == null)
                {
                    this.shimManager.AddDocumentShim(this);
                    documentShim = this.ShimManager.GetDocumentShim(this);
                }
                return documentShim;
            }
        }

        public string Domain
        {
            get
            {
                return this.NativeHtmlDocument2.GetDomain();
            }
            set
            {
                try
                {
                    this.NativeHtmlDocument2.SetDomain(value);
                }
                catch (ArgumentException)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("HtmlDocumentInvalidDomain"));
                }
            }
        }

        public object DomDocument
        {
            get
            {
                return this.NativeHtmlDocument2;
            }
        }

        public string Encoding
        {
            get
            {
                return this.NativeHtmlDocument2.GetCharset();
            }
            set
            {
                this.NativeHtmlDocument2.SetCharset(value);
            }
        }

        public bool Focused
        {
            get
            {
                return ((System.Windows.Forms.UnsafeNativeMethods.IHTMLDocument4) this.NativeHtmlDocument2).HasFocus();
            }
        }

        public Color ForeColor
        {
            get
            {
                Color empty = Color.Empty;
                try
                {
                    empty = this.ColorFromObject(this.NativeHtmlDocument2.GetFgColor());
                }
                catch (Exception exception)
                {
                    if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception))
                    {
                        throw;
                    }
                }
                return empty;
            }
            set
            {
                int c = ((value.R << 0x10) | (value.G << 8)) | value.B;
                this.NativeHtmlDocument2.SetFgColor(c);
            }
        }

        public HtmlElementCollection Forms
        {
            get
            {
                System.Windows.Forms.UnsafeNativeMethods.IHTMLElementCollection forms = this.NativeHtmlDocument2.GetForms();
                if (forms == null)
                {
                    return new HtmlElementCollection(this.ShimManager);
                }
                return new HtmlElementCollection(this.ShimManager, forms);
            }
        }

        public HtmlElementCollection Images
        {
            get
            {
                System.Windows.Forms.UnsafeNativeMethods.IHTMLElementCollection images = this.NativeHtmlDocument2.GetImages();
                if (images == null)
                {
                    return new HtmlElementCollection(this.ShimManager);
                }
                return new HtmlElementCollection(this.ShimManager, images);
            }
        }

        public Color LinkColor
        {
            get
            {
                Color empty = Color.Empty;
                try
                {
                    empty = this.ColorFromObject(this.NativeHtmlDocument2.GetLinkColor());
                }
                catch (Exception exception)
                {
                    if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception))
                    {
                        throw;
                    }
                }
                return empty;
            }
            set
            {
                int c = ((value.R << 0x10) | (value.G << 8)) | value.B;
                this.NativeHtmlDocument2.SetLinkColor(c);
            }
        }

        public HtmlElementCollection Links
        {
            get
            {
                System.Windows.Forms.UnsafeNativeMethods.IHTMLElementCollection links = this.NativeHtmlDocument2.GetLinks();
                if (links == null)
                {
                    return new HtmlElementCollection(this.ShimManager);
                }
                return new HtmlElementCollection(this.ShimManager, links);
            }
        }

        internal System.Windows.Forms.UnsafeNativeMethods.IHTMLDocument2 NativeHtmlDocument2
        {
            get
            {
                return this.htmlDocument2;
            }
        }

        public bool RightToLeft
        {
            get
            {
                return (((System.Windows.Forms.UnsafeNativeMethods.IHTMLDocument3) this.NativeHtmlDocument2).GetDir() == "rtl");
            }
            set
            {
                ((System.Windows.Forms.UnsafeNativeMethods.IHTMLDocument3) this.NativeHtmlDocument2).SetDir(value ? "rtl" : "ltr");
            }
        }

        private HtmlShimManager ShimManager
        {
            get
            {
                return this.shimManager;
            }
        }

        public string Title
        {
            get
            {
                return this.NativeHtmlDocument2.GetTitle();
            }
            set
            {
                this.NativeHtmlDocument2.SetTitle(value);
            }
        }

        public Uri Url
        {
            get
            {
                System.Windows.Forms.UnsafeNativeMethods.IHTMLLocation location = this.NativeHtmlDocument2.GetLocation();
                string str = (location == null) ? "" : location.GetHref();
                if (!string.IsNullOrEmpty(str))
                {
                    return new Uri(str);
                }
                return null;
            }
        }

        public Color VisitedLinkColor
        {
            get
            {
                Color empty = Color.Empty;
                try
                {
                    empty = this.ColorFromObject(this.NativeHtmlDocument2.GetVlinkColor());
                }
                catch (Exception exception)
                {
                    if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception))
                    {
                        throw;
                    }
                }
                return empty;
            }
            set
            {
                int c = ((value.R << 0x10) | (value.G << 8)) | value.B;
                this.NativeHtmlDocument2.SetVlinkColor(c);
            }
        }

        public HtmlWindow Window
        {
            get
            {
                System.Windows.Forms.UnsafeNativeMethods.IHTMLWindow2 parentWindow = this.NativeHtmlDocument2.GetParentWindow();
                if (parentWindow == null)
                {
                    return null;
                }
                return new HtmlWindow(this.ShimManager, parentWindow);
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack=1)]
        private struct FindSizeOfVariant
        {
            [MarshalAs(UnmanagedType.Struct)]
            public object var;
            public byte b;
        }

        [ClassInterface(ClassInterfaceType.None)]
        private class HTMLDocumentEvents2 : StandardOleMarshalObject, System.Windows.Forms.UnsafeNativeMethods.DHTMLDocumentEvents2
        {
            private HtmlDocument parent;

            public HTMLDocumentEvents2(HtmlDocument htmlDocument)
            {
                this.parent = htmlDocument;
            }

            private void FireEvent(object key, EventArgs e)
            {
                if (this.parent != null)
                {
                    this.parent.DocumentShim.FireEvent(key, e);
                }
            }

            public void onactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public void onafterupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public bool onbeforeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs args = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                return args.ReturnValue;
            }

            public bool onbeforedeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs args = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                return args.ReturnValue;
            }

            public void onbeforeeditfocus(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public bool onbeforeupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs args = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                return args.ReturnValue;
            }

            public void oncellchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public bool onclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs e = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                this.FireEvent(HtmlDocument.EventClick, e);
                return e.ReturnValue;
            }

            public bool oncontextmenu(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs e = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                this.FireEvent(HtmlDocument.EventContextMenuShowing, e);
                return e.ReturnValue;
            }

            public bool oncontrolselect(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs args = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                return args.ReturnValue;
            }

            public void ondataavailable(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public void ondatasetchanged(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public void ondatasetcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public bool ondblclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs args = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                return args.ReturnValue;
            }

            public void ondeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public bool ondragstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs args = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                return args.ReturnValue;
            }

            public bool onerrorupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs args = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                return args.ReturnValue;
            }

            public void onfocusin(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs e = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                this.FireEvent(HtmlDocument.EventFocusing, e);
            }

            public void onfocusout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs e = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                this.FireEvent(HtmlDocument.EventLosingFocus, e);
            }

            public bool onhelp(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs args = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                return args.ReturnValue;
            }

            public void onkeydown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public bool onkeypress(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs args = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                return args.ReturnValue;
            }

            public void onkeyup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public void onmousedown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs e = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                this.FireEvent(HtmlDocument.EventMouseDown, e);
            }

            public void onmousemove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs e = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                this.FireEvent(HtmlDocument.EventMouseMove, e);
            }

            public void onmouseout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs e = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                this.FireEvent(HtmlDocument.EventMouseLeave, e);
            }

            public void onmouseover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs e = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                this.FireEvent(HtmlDocument.EventMouseOver, e);
            }

            public void onmouseup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs e = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                this.FireEvent(HtmlDocument.EventMouseUp, e);
            }

            public bool onmousewheel(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs args = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                return args.ReturnValue;
            }

            public void onpropertychange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public void onreadystatechange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public void onrowenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public bool onrowexit(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs args = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                return args.ReturnValue;
            }

            public void onrowsdelete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public void onrowsinserted(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public void onselectionchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public bool onselectstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs args = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                return args.ReturnValue;
            }

            public bool onstop(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs e = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                this.FireEvent(HtmlDocument.EventStop, e);
                return e.ReturnValue;
            }
        }

        internal class HtmlDocumentShim : HtmlShim
        {
            private System.Windows.Forms.UnsafeNativeMethods.IHTMLWindow2 associatedWindow;
            private AxHost.ConnectionPointCookie cookie;
            private HtmlDocument htmlDocument;

            internal HtmlDocumentShim(HtmlDocument htmlDocument)
            {
                this.htmlDocument = htmlDocument;
                if (this.htmlDocument != null)
                {
                    HtmlWindow window = htmlDocument.Window;
                    if (window != null)
                    {
                        this.associatedWindow = window.NativeHtmlWindow;
                    }
                }
            }

            public override void AttachEventHandler(string eventName, EventHandler eventHandler)
            {
                HtmlToClrEventProxy pdisp = base.AddEventProxy(eventName, eventHandler);
                ((System.Windows.Forms.UnsafeNativeMethods.IHTMLDocument3) this.NativeHtmlDocument2).AttachEvent(eventName, pdisp);
            }

            public override void ConnectToEvents()
            {
                if ((this.cookie == null) || !this.cookie.Connected)
                {
                    this.cookie = new AxHost.ConnectionPointCookie(this.NativeHtmlDocument2, new HtmlDocument.HTMLDocumentEvents2(this.htmlDocument), typeof(System.Windows.Forms.UnsafeNativeMethods.DHTMLDocumentEvents2), false);
                    if (!this.cookie.Connected)
                    {
                        this.cookie = null;
                    }
                }
            }

            public override void DetachEventHandler(string eventName, EventHandler eventHandler)
            {
                HtmlToClrEventProxy pdisp = base.RemoveEventProxy(eventHandler);
                if (pdisp != null)
                {
                    ((System.Windows.Forms.UnsafeNativeMethods.IHTMLDocument3) this.NativeHtmlDocument2).DetachEvent(eventName, pdisp);
                }
            }

            public override void DisconnectFromEvents()
            {
                if (this.cookie != null)
                {
                    this.cookie.Disconnect();
                    this.cookie = null;
                }
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                if (disposing)
                {
                    if (this.htmlDocument != null)
                    {
                        Marshal.FinalReleaseComObject(this.htmlDocument.NativeHtmlDocument2);
                    }
                    this.htmlDocument = null;
                }
            }

            protected override object GetEventSender()
            {
                return this.htmlDocument;
            }

            public override System.Windows.Forms.UnsafeNativeMethods.IHTMLWindow2 AssociatedWindow
            {
                get
                {
                    return this.associatedWindow;
                }
            }

            internal HtmlDocument Document
            {
                get
                {
                    return this.htmlDocument;
                }
            }

            public System.Windows.Forms.UnsafeNativeMethods.IHTMLDocument2 NativeHtmlDocument2
            {
                get
                {
                    return this.htmlDocument.NativeHtmlDocument2;
                }
            }
        }
    }
}

