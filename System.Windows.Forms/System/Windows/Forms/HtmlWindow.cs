namespace System.Windows.Forms
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public sealed class HtmlWindow
    {
        internal static readonly object EventError = new object();
        internal static readonly object EventGotFocus = new object();
        internal static readonly object EventLoad = new object();
        internal static readonly object EventLostFocus = new object();
        internal static readonly object EventResize = new object();
        internal static readonly object EventScroll = new object();
        internal static readonly object EventUnload = new object();
        private System.Windows.Forms.UnsafeNativeMethods.IHTMLWindow2 htmlWindow2;
        private HtmlShimManager shimManager;

        public event HtmlElementErrorEventHandler Error
        {
            add
            {
                this.WindowShim.AddHandler(EventError, value);
            }
            remove
            {
                this.WindowShim.RemoveHandler(EventError, value);
            }
        }

        public event HtmlElementEventHandler GotFocus
        {
            add
            {
                this.WindowShim.AddHandler(EventGotFocus, value);
            }
            remove
            {
                this.WindowShim.RemoveHandler(EventGotFocus, value);
            }
        }

        public event HtmlElementEventHandler Load
        {
            add
            {
                this.WindowShim.AddHandler(EventLoad, value);
            }
            remove
            {
                this.WindowShim.RemoveHandler(EventLoad, value);
            }
        }

        public event HtmlElementEventHandler LostFocus
        {
            add
            {
                this.WindowShim.AddHandler(EventLostFocus, value);
            }
            remove
            {
                this.WindowShim.RemoveHandler(EventLostFocus, value);
            }
        }

        public event HtmlElementEventHandler Resize
        {
            add
            {
                this.WindowShim.AddHandler(EventResize, value);
            }
            remove
            {
                this.WindowShim.RemoveHandler(EventResize, value);
            }
        }

        public event HtmlElementEventHandler Scroll
        {
            add
            {
                this.WindowShim.AddHandler(EventScroll, value);
            }
            remove
            {
                this.WindowShim.RemoveHandler(EventScroll, value);
            }
        }

        public event HtmlElementEventHandler Unload
        {
            add
            {
                this.WindowShim.AddHandler(EventUnload, value);
            }
            remove
            {
                this.WindowShim.RemoveHandler(EventUnload, value);
            }
        }

        [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        internal HtmlWindow(HtmlShimManager shimManager, System.Windows.Forms.UnsafeNativeMethods.IHTMLWindow2 win)
        {
            this.htmlWindow2 = win;
            this.shimManager = shimManager;
        }

        public void Alert(string message)
        {
            this.NativeHtmlWindow.Alert(message);
        }

        public void AttachEventHandler(string eventName, EventHandler eventHandler)
        {
            this.WindowShim.AttachEventHandler(eventName, eventHandler);
        }

        public void Close()
        {
            this.NativeHtmlWindow.Close();
        }

        public bool Confirm(string message)
        {
            return this.NativeHtmlWindow.Confirm(message);
        }

        public void DetachEventHandler(string eventName, EventHandler eventHandler)
        {
            this.WindowShim.DetachEventHandler(eventName, eventHandler);
        }

        public override bool Equals(object obj)
        {
            return (this == ((HtmlWindow) obj));
        }

        public void Focus()
        {
            this.NativeHtmlWindow.Focus();
        }

        public override int GetHashCode()
        {
            if (this.htmlWindow2 != null)
            {
                return this.htmlWindow2.GetHashCode();
            }
            return 0;
        }

        public void MoveTo(Point point)
        {
            this.NativeHtmlWindow.MoveTo(point.X, point.Y);
        }

        public void MoveTo(int x, int y)
        {
            this.NativeHtmlWindow.MoveTo(x, y);
        }

        public void Navigate(string urlString)
        {
            this.NativeHtmlWindow.Navigate(urlString);
        }

        public void Navigate(Uri url)
        {
            this.NativeHtmlWindow.Navigate(url.ToString());
        }

        public static bool operator ==(HtmlWindow left, HtmlWindow right)
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
                zero = Marshal.GetIUnknownForObject(left.NativeHtmlWindow);
                pUnk = Marshal.GetIUnknownForObject(right.NativeHtmlWindow);
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

        public static bool operator !=(HtmlWindow left, HtmlWindow right)
        {
            return !(left == right);
        }

        public HtmlWindow Open(string urlString, string target, string windowOptions, bool replaceEntry)
        {
            System.Windows.Forms.UnsafeNativeMethods.IHTMLWindow2 win = this.NativeHtmlWindow.Open(urlString, target, windowOptions, replaceEntry);
            if (win == null)
            {
                return null;
            }
            return new HtmlWindow(this.ShimManager, win);
        }

        public HtmlWindow Open(Uri url, string target, string windowOptions, bool replaceEntry)
        {
            return this.Open(url.ToString(), target, windowOptions, replaceEntry);
        }

        public HtmlWindow OpenNew(string urlString, string windowOptions)
        {
            System.Windows.Forms.UnsafeNativeMethods.IHTMLWindow2 win = this.NativeHtmlWindow.Open(urlString, "_blank", windowOptions, true);
            if (win == null)
            {
                return null;
            }
            return new HtmlWindow(this.ShimManager, win);
        }

        public HtmlWindow OpenNew(Uri url, string windowOptions)
        {
            return this.OpenNew(url.ToString(), windowOptions);
        }

        public string Prompt(string message, string defaultInputValue)
        {
            return this.NativeHtmlWindow.Prompt(message, defaultInputValue).ToString();
        }

        public void RemoveFocus()
        {
            this.NativeHtmlWindow.Blur();
        }

        public void ResizeTo(System.Drawing.Size size)
        {
            this.NativeHtmlWindow.ResizeTo(size.Width, size.Height);
        }

        public void ResizeTo(int width, int height)
        {
            this.NativeHtmlWindow.ResizeTo(width, height);
        }

        public void ScrollTo(Point point)
        {
            this.NativeHtmlWindow.ScrollTo(point.X, point.Y);
        }

        public void ScrollTo(int x, int y)
        {
            this.NativeHtmlWindow.ScrollTo(x, y);
        }

        public HtmlDocument Document
        {
            get
            {
                System.Windows.Forms.UnsafeNativeMethods.IHTMLDocument doc = this.NativeHtmlWindow.GetDocument() as System.Windows.Forms.UnsafeNativeMethods.IHTMLDocument;
                if (doc == null)
                {
                    return null;
                }
                return new HtmlDocument(this.ShimManager, doc);
            }
        }

        public object DomWindow
        {
            get
            {
                return this.NativeHtmlWindow;
            }
        }

        public HtmlWindowCollection Frames
        {
            get
            {
                System.Windows.Forms.UnsafeNativeMethods.IHTMLFramesCollection2 frames = this.NativeHtmlWindow.GetFrames();
                if (frames == null)
                {
                    return null;
                }
                return new HtmlWindowCollection(this.ShimManager, frames);
            }
        }

        public HtmlHistory History
        {
            get
            {
                System.Windows.Forms.UnsafeNativeMethods.IOmHistory history = this.NativeHtmlWindow.GetHistory();
                if (history == null)
                {
                    return null;
                }
                return new HtmlHistory(history);
            }
        }

        public bool IsClosed
        {
            get
            {
                return this.NativeHtmlWindow.GetClosed();
            }
        }

        public string Name
        {
            get
            {
                return this.NativeHtmlWindow.GetName();
            }
            set
            {
                this.NativeHtmlWindow.SetName(value);
            }
        }

        internal System.Windows.Forms.UnsafeNativeMethods.IHTMLWindow2 NativeHtmlWindow
        {
            get
            {
                return this.htmlWindow2;
            }
        }

        public HtmlWindow Opener
        {
            get
            {
                System.Windows.Forms.UnsafeNativeMethods.IHTMLWindow2 opener = this.NativeHtmlWindow.GetOpener() as System.Windows.Forms.UnsafeNativeMethods.IHTMLWindow2;
                if (opener == null)
                {
                    return null;
                }
                return new HtmlWindow(this.ShimManager, opener);
            }
        }

        public HtmlWindow Parent
        {
            get
            {
                System.Windows.Forms.UnsafeNativeMethods.IHTMLWindow2 parent = this.NativeHtmlWindow.GetParent();
                if (parent == null)
                {
                    return null;
                }
                return new HtmlWindow(this.ShimManager, parent);
            }
        }

        public Point Position
        {
            get
            {
                return new Point(((System.Windows.Forms.UnsafeNativeMethods.IHTMLWindow3) this.NativeHtmlWindow).GetScreenLeft(), ((System.Windows.Forms.UnsafeNativeMethods.IHTMLWindow3) this.NativeHtmlWindow).GetScreenTop());
            }
        }

        private HtmlShimManager ShimManager
        {
            get
            {
                return this.shimManager;
            }
        }

        public System.Drawing.Size Size
        {
            get
            {
                System.Windows.Forms.UnsafeNativeMethods.IHTMLElement body = this.NativeHtmlWindow.GetDocument().GetBody();
                return new System.Drawing.Size(body.GetOffsetWidth(), body.GetOffsetHeight());
            }
            set
            {
                this.ResizeTo(value.Width, value.Height);
            }
        }

        public string StatusBarText
        {
            get
            {
                return this.NativeHtmlWindow.GetStatus();
            }
            set
            {
                this.NativeHtmlWindow.SetStatus(value);
            }
        }

        public Uri Url
        {
            get
            {
                System.Windows.Forms.UnsafeNativeMethods.IHTMLLocation location = this.NativeHtmlWindow.GetLocation();
                string str = (location == null) ? "" : location.GetHref();
                if (!string.IsNullOrEmpty(str))
                {
                    return new Uri(str);
                }
                return null;
            }
        }

        public HtmlElement WindowFrameElement
        {
            get
            {
                System.Windows.Forms.UnsafeNativeMethods.IHTMLElement element = ((System.Windows.Forms.UnsafeNativeMethods.IHTMLWindow4) this.NativeHtmlWindow).frameElement() as System.Windows.Forms.UnsafeNativeMethods.IHTMLElement;
                if (element == null)
                {
                    return null;
                }
                return new HtmlElement(this.ShimManager, element);
            }
        }

        private HtmlWindowShim WindowShim
        {
            get
            {
                if (this.ShimManager == null)
                {
                    return null;
                }
                HtmlWindowShim windowShim = this.ShimManager.GetWindowShim(this);
                if (windowShim == null)
                {
                    this.shimManager.AddWindowShim(this);
                    windowShim = this.ShimManager.GetWindowShim(this);
                }
                return windowShim;
            }
        }

        [ClassInterface(ClassInterfaceType.None)]
        private class HTMLWindowEvents2 : StandardOleMarshalObject, System.Windows.Forms.UnsafeNativeMethods.DHTMLWindowEvents2
        {
            private HtmlWindow parent;

            public HTMLWindowEvents2(HtmlWindow htmlWindow)
            {
                this.parent = htmlWindow;
            }

            private void FireEvent(object key, EventArgs e)
            {
                if (this.parent != null)
                {
                    this.parent.WindowShim.FireEvent(key, e);
                }
            }

            public void onafterprint(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public void onbeforeprint(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public void onbeforeunload(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
            }

            public void onblur(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs e = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                this.FireEvent(HtmlWindow.EventLostFocus, e);
            }

            public bool onerror(string description, string urlString, int line)
            {
                HtmlElementErrorEventArgs e = new HtmlElementErrorEventArgs(description, urlString, line);
                this.FireEvent(HtmlWindow.EventError, e);
                return e.Handled;
            }

            public void onfocus(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs e = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                this.FireEvent(HtmlWindow.EventGotFocus, e);
            }

            public bool onhelp(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs args = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                return args.ReturnValue;
            }

            public void onload(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs e = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                this.FireEvent(HtmlWindow.EventLoad, e);
            }

            public void onresize(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs e = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                this.FireEvent(HtmlWindow.EventResize, e);
            }

            public void onscroll(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs e = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                this.FireEvent(HtmlWindow.EventScroll, e);
            }

            public void onunload(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj)
            {
                HtmlElementEventArgs e = new HtmlElementEventArgs(this.parent.ShimManager, evtObj);
                this.FireEvent(HtmlWindow.EventUnload, e);
                if (this.parent != null)
                {
                    this.parent.WindowShim.OnWindowUnload();
                }
            }
        }

        internal class HtmlWindowShim : HtmlShim
        {
            private AxHost.ConnectionPointCookie cookie;
            private HtmlWindow htmlWindow;

            public HtmlWindowShim(HtmlWindow window)
            {
                this.htmlWindow = window;
            }

            public override void AttachEventHandler(string eventName, EventHandler eventHandler)
            {
                HtmlToClrEventProxy pdisp = base.AddEventProxy(eventName, eventHandler);
                ((System.Windows.Forms.UnsafeNativeMethods.IHTMLWindow3) this.NativeHtmlWindow).AttachEvent(eventName, pdisp);
            }

            public override void ConnectToEvents()
            {
                if ((this.cookie == null) || !this.cookie.Connected)
                {
                    this.cookie = new AxHost.ConnectionPointCookie(this.NativeHtmlWindow, new HtmlWindow.HTMLWindowEvents2(this.htmlWindow), typeof(System.Windows.Forms.UnsafeNativeMethods.DHTMLWindowEvents2), false);
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
                    ((System.Windows.Forms.UnsafeNativeMethods.IHTMLWindow3) this.NativeHtmlWindow).DetachEvent(eventName, pdisp);
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
                    if ((this.htmlWindow != null) && (this.htmlWindow.NativeHtmlWindow != null))
                    {
                        Marshal.FinalReleaseComObject(this.htmlWindow.NativeHtmlWindow);
                    }
                    this.htmlWindow = null;
                }
            }

            protected override object GetEventSender()
            {
                return this.htmlWindow;
            }

            public void OnWindowUnload()
            {
                if (this.htmlWindow != null)
                {
                    this.htmlWindow.ShimManager.OnWindowUnloaded(this.htmlWindow);
                }
            }

            public override System.Windows.Forms.UnsafeNativeMethods.IHTMLWindow2 AssociatedWindow
            {
                get
                {
                    return this.htmlWindow.NativeHtmlWindow;
                }
            }

            public System.Windows.Forms.UnsafeNativeMethods.IHTMLWindow2 NativeHtmlWindow
            {
                get
                {
                    return this.htmlWindow.NativeHtmlWindow;
                }
            }
        }
    }
}

