namespace System.Windows.Forms
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;

    [ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch), DefaultProperty("Url"), DefaultEvent("DocumentCompleted"), Docking(DockingBehavior.AutoDock), System.Windows.Forms.SRDescription("DescriptionWebBrowser"), Designer("System.Windows.Forms.Design.WebBrowserDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust"), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public class WebBrowser : WebBrowserBase
    {
        private System.Windows.Forms.UnsafeNativeMethods.IWebBrowser2 axIWebBrowser2;
        private AxHost.ConnectionPointCookie cookie;
        private static bool createdInIE;
        private Stream documentStreamToSetOnLoad;
        private WebBrowserEncryptionLevel encryptionLevel;
        private HtmlShimManager htmlShimManager;
        private object objectForScripting;
        internal string statusText;
        private WebBrowserEvent webBrowserEvent;
        private BitVector32 webBrowserState;
        private const int WEBBROWSERSTATE_allowNavigation = 0x40;
        private const int WEBBROWSERSTATE_canGoBack = 8;
        private const int WEBBROWSERSTATE_canGoForward = 0x10;
        private const int WEBBROWSERSTATE_documentStreamJustSet = 2;
        private const int WEBBROWSERSTATE_isWebBrowserContextMenuEnabled = 4;
        private const int WEBBROWSERSTATE_scrollbarsEnabled = 0x20;
        private const int WEBBROWSERSTATE_webBrowserShortcutsEnabled = 1;

        [System.Windows.Forms.SRDescription("WebBrowserCanGoBackChangedDescr"), Browsable(false), System.Windows.Forms.SRCategory("CatPropertyChanged")]
        public event EventHandler CanGoBackChanged;

        [Browsable(false), System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("WebBrowserCanGoForwardChangedDescr")]
        public event EventHandler CanGoForwardChanged;

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("WebBrowserDocumentCompletedDescr")]
        public event WebBrowserDocumentCompletedEventHandler DocumentCompleted;

        [Browsable(false), System.Windows.Forms.SRDescription("WebBrowserDocumentTitleChangedDescr"), System.Windows.Forms.SRCategory("CatPropertyChanged")]
        public event EventHandler DocumentTitleChanged;

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("WebBrowserEncryptionLevelChangedDescr"), Browsable(false)]
        public event EventHandler EncryptionLevelChanged;

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("WebBrowserFileDownloadDescr")]
        public event EventHandler FileDownload;

        [System.Windows.Forms.SRCategory("CatAction"), System.Windows.Forms.SRDescription("WebBrowserNavigatedDescr")]
        public event WebBrowserNavigatedEventHandler Navigated;

        [System.Windows.Forms.SRCategory("CatAction"), System.Windows.Forms.SRDescription("WebBrowserNavigatingDescr")]
        public event WebBrowserNavigatingEventHandler Navigating;

        [System.Windows.Forms.SRDescription("WebBrowserNewWindowDescr"), System.Windows.Forms.SRCategory("CatAction")]
        public event CancelEventHandler NewWindow;

        [EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRCategory("CatLayout"), System.Windows.Forms.SRDescription("ControlOnPaddingChangedDescr"), Browsable(false)]
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

        [System.Windows.Forms.SRDescription("WebBrowserProgressChangedDescr"), System.Windows.Forms.SRCategory("CatAction")]
        public event WebBrowserProgressChangedEventHandler ProgressChanged;

        [System.Windows.Forms.SRDescription("WebBrowserStatusTextChangedDescr"), System.Windows.Forms.SRCategory("CatPropertyChanged"), Browsable(false)]
        public event EventHandler StatusTextChanged;

        [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        public WebBrowser() : base("8856f961-340a-11d0-a96b-00c04fd705a2")
        {
            this.statusText = "";
            this.CheckIfCreatedInIE();
            this.webBrowserState = new BitVector32(0x25);
            this.AllowNavigation = true;
        }

        protected override void AttachInterfaces(object nativeActiveXObject)
        {
            this.axIWebBrowser2 = (System.Windows.Forms.UnsafeNativeMethods.IWebBrowser2) nativeActiveXObject;
        }

        private void CheckIfCreatedInIE()
        {
            if (createdInIE)
            {
                if (this.ParentInternal != null)
                {
                    this.ParentInternal.Controls.Remove(this);
                    base.Dispose();
                }
                else
                {
                    base.Dispose();
                    throw new NotSupportedException(System.Windows.Forms.SR.GetString("WebBrowserInIENotSupported"));
                }
            }
        }

        protected override void CreateSink()
        {
            object activeXInstance = base.activeXInstance;
            if (activeXInstance != null)
            {
                this.webBrowserEvent = new WebBrowserEvent(this);
                this.webBrowserEvent.AllowNavigation = this.AllowNavigation;
                this.cookie = new AxHost.ConnectionPointCookie(activeXInstance, this.webBrowserEvent, typeof(System.Windows.Forms.UnsafeNativeMethods.DWebBrowserEvents2));
            }
        }

        protected override WebBrowserSiteBase CreateWebBrowserSiteBase()
        {
            return new WebBrowserSite(this);
        }

        protected override void DetachInterfaces()
        {
            this.axIWebBrowser2 = null;
        }

        protected override void DetachSink()
        {
            if (this.cookie != null)
            {
                this.cookie.Disconnect();
                this.cookie = null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.htmlShimManager != null)
                {
                    this.htmlShimManager.Dispose();
                }
                this.DetachSink();
                base.ActiveXSite.Dispose();
            }
            base.Dispose(disposing);
        }

        internal static void EnsureUrlConnectPermission(Uri url)
        {
            new WebPermission(NetworkAccess.Connect, url.ToString()).Demand();
        }

        public bool GoBack()
        {
            bool flag = true;
            try
            {
                this.AxIWebBrowser2.GoBack();
            }
            catch (Exception exception)
            {
                if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception))
                {
                    throw;
                }
                flag = false;
            }
            return flag;
        }

        public bool GoForward()
        {
            bool flag = true;
            try
            {
                this.AxIWebBrowser2.GoForward();
            }
            catch (Exception exception)
            {
                if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception))
                {
                    throw;
                }
                flag = false;
            }
            return flag;
        }

        public void GoHome()
        {
            this.AxIWebBrowser2.GoHome();
        }

        public void GoSearch()
        {
            this.AxIWebBrowser2.GoSearch();
        }

        public void Navigate(string urlString)
        {
            this.PerformNavigateHelper(this.ReadyNavigateToUrl(urlString), false, null, null, null);
        }

        public void Navigate(Uri url)
        {
            this.Url = url;
        }

        public void Navigate(string urlString, bool newWindow)
        {
            this.PerformNavigateHelper(this.ReadyNavigateToUrl(urlString), newWindow, null, null, null);
        }

        public void Navigate(string urlString, string targetFrameName)
        {
            this.PerformNavigateHelper(this.ReadyNavigateToUrl(urlString), false, targetFrameName, null, null);
        }

        public void Navigate(Uri url, bool newWindow)
        {
            this.PerformNavigateHelper(this.ReadyNavigateToUrl(url), newWindow, null, null, null);
        }

        public void Navigate(Uri url, string targetFrameName)
        {
            this.PerformNavigateHelper(this.ReadyNavigateToUrl(url), false, targetFrameName, null, null);
        }

        public void Navigate(string urlString, string targetFrameName, byte[] postData, string additionalHeaders)
        {
            this.PerformNavigateHelper(this.ReadyNavigateToUrl(urlString), false, targetFrameName, postData, additionalHeaders);
        }

        public void Navigate(Uri url, string targetFrameName, byte[] postData, string additionalHeaders)
        {
            this.PerformNavigateHelper(this.ReadyNavigateToUrl(url), false, targetFrameName, postData, additionalHeaders);
        }

        protected virtual void OnCanGoBackChanged(EventArgs e)
        {
            if (this.CanGoBackChanged != null)
            {
                this.CanGoBackChanged(this, e);
            }
        }

        protected virtual void OnCanGoForwardChanged(EventArgs e)
        {
            if (this.CanGoForwardChanged != null)
            {
                this.CanGoForwardChanged(this, e);
            }
        }

        protected virtual void OnDocumentCompleted(WebBrowserDocumentCompletedEventArgs e)
        {
            this.AxIWebBrowser2.RegisterAsDropTarget = this.AllowWebBrowserDrop;
            if (this.DocumentCompleted != null)
            {
                this.DocumentCompleted(this, e);
            }
        }

        protected virtual void OnDocumentTitleChanged(EventArgs e)
        {
            if (this.DocumentTitleChanged != null)
            {
                this.DocumentTitleChanged(this, e);
            }
        }

        protected virtual void OnEncryptionLevelChanged(EventArgs e)
        {
            if (this.EncryptionLevelChanged != null)
            {
                this.EncryptionLevelChanged(this, e);
            }
        }

        protected virtual void OnFileDownload(EventArgs e)
        {
            if (this.FileDownload != null)
            {
                this.FileDownload(this, e);
            }
        }

        protected virtual void OnNavigated(WebBrowserNavigatedEventArgs e)
        {
            if (this.Navigated != null)
            {
                this.Navigated(this, e);
            }
        }

        protected virtual void OnNavigating(WebBrowserNavigatingEventArgs e)
        {
            if (this.Navigating != null)
            {
                this.Navigating(this, e);
            }
        }

        protected virtual void OnNewWindow(CancelEventArgs e)
        {
            if (this.NewWindow != null)
            {
                this.NewWindow(this, e);
            }
        }

        protected virtual void OnProgressChanged(WebBrowserProgressChangedEventArgs e)
        {
            if (this.ProgressChanged != null)
            {
                this.ProgressChanged(this, e);
            }
        }

        protected virtual void OnStatusTextChanged(EventArgs e)
        {
            if (this.StatusTextChanged != null)
            {
                this.StatusTextChanged(this, e);
            }
        }

        internal override void OnTopMostActiveXParentChanged(EventArgs e)
        {
            if (base.TopMostParent.IsIEParent)
            {
                createdInIE = true;
                this.CheckIfCreatedInIE();
            }
            else
            {
                createdInIE = false;
                base.OnTopMostActiveXParentChanged(e);
            }
        }

        private void PerformNavigate2(ref object URL, ref object flags, ref object targetFrameName, ref object postData, ref object headers)
        {
            try
            {
                this.AxIWebBrowser2.Navigate2(ref URL, ref flags, ref targetFrameName, ref postData, ref headers);
            }
            catch (COMException exception)
            {
                if (exception.ErrorCode != -2147023673)
                {
                    throw;
                }
            }
        }

        private void PerformNavigateHelper(string urlString, bool newWindow, string targetFrameName, byte[] postData, string headers)
        {
            object uRL = urlString;
            object flags = newWindow ? 1 : 0;
            object obj4 = targetFrameName;
            object obj5 = postData;
            object obj6 = headers;
            this.PerformNavigate2(ref uRL, ref flags, ref obj4, ref obj5, ref obj6);
        }

        public void Print()
        {
            System.Windows.Forms.IntSecurity.DefaultPrinting.Demand();
            try
            {
                this.AxIWebBrowser2.ExecWB(System.Windows.Forms.NativeMethods.OLECMDID.OLECMDID_PRINT, System.Windows.Forms.NativeMethods.OLECMDEXECOPT.OLECMDEXECOPT_DONTPROMPTUSER, null, IntPtr.Zero);
            }
            catch (Exception exception)
            {
                if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception))
                {
                    throw;
                }
            }
        }

        private string ReadyNavigateToUrl(string urlString)
        {
            if (string.IsNullOrEmpty(urlString))
            {
                urlString = "about:blank";
            }
            if (!this.webBrowserState[2])
            {
                this.documentStreamToSetOnLoad = null;
            }
            return urlString;
        }

        private string ReadyNavigateToUrl(Uri url)
        {
            string str;
            if (url == null)
            {
                str = "about:blank";
            }
            else
            {
                if (!url.IsAbsoluteUri)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("WebBrowserNavigateAbsoluteUri", new object[] { "uri" }));
                }
                str = url.ToString();
            }
            return this.ReadyNavigateToUrl(str);
        }

        public override void Refresh()
        {
            try
            {
                if (this.ShouldSerializeDocumentText())
                {
                    string documentText = this.DocumentText;
                    this.AxIWebBrowser2.Refresh();
                    this.DocumentText = documentText;
                }
                else
                {
                    this.AxIWebBrowser2.Refresh();
                }
            }
            catch (Exception exception)
            {
                if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception))
                {
                    throw;
                }
            }
        }

        public void Refresh(WebBrowserRefreshOption opt)
        {
            object level = opt;
            try
            {
                if (this.ShouldSerializeDocumentText())
                {
                    string documentText = this.DocumentText;
                    this.AxIWebBrowser2.Refresh2(ref level);
                    this.DocumentText = documentText;
                }
                else
                {
                    this.AxIWebBrowser2.Refresh2(ref level);
                }
            }
            catch (Exception exception)
            {
                if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception))
                {
                    throw;
                }
            }
        }

        private bool ShouldSerializeDocumentText()
        {
            return this.IsValidUrl;
        }

        private bool ShouldSerializeUrl()
        {
            return !this.ShouldSerializeDocumentText();
        }

        private bool ShowContextMenu(int x, int y)
        {
            Point point;
            ContextMenuStrip contextMenuStrip = this.ContextMenuStrip;
            ContextMenu menu = (contextMenuStrip != null) ? null : this.ContextMenu;
            if ((contextMenuStrip == null) && (menu == null))
            {
                return false;
            }
            bool isKeyboardActivated = false;
            if (x == -1)
            {
                isKeyboardActivated = true;
                point = new Point(base.Width / 2, base.Height / 2);
            }
            else
            {
                point = base.PointToClientInternal(new Point(x, y));
            }
            if (!base.ClientRectangle.Contains(point))
            {
                return false;
            }
            if (contextMenuStrip != null)
            {
                contextMenuStrip.ShowInternal(this, point, isKeyboardActivated);
            }
            else if (menu != null)
            {
                menu.Show(this, point);
            }
            return true;
        }

        public void ShowPageSetupDialog()
        {
            System.Windows.Forms.IntSecurity.SafePrinting.Demand();
            try
            {
                this.AxIWebBrowser2.ExecWB(System.Windows.Forms.NativeMethods.OLECMDID.OLECMDID_PAGESETUP, System.Windows.Forms.NativeMethods.OLECMDEXECOPT.OLECMDEXECOPT_PROMPTUSER, null, IntPtr.Zero);
            }
            catch (Exception exception)
            {
                if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception))
                {
                    throw;
                }
            }
        }

        public void ShowPrintDialog()
        {
            System.Windows.Forms.IntSecurity.SafePrinting.Demand();
            try
            {
                this.AxIWebBrowser2.ExecWB(System.Windows.Forms.NativeMethods.OLECMDID.OLECMDID_PRINT, System.Windows.Forms.NativeMethods.OLECMDEXECOPT.OLECMDEXECOPT_PROMPTUSER, null, IntPtr.Zero);
            }
            catch (Exception exception)
            {
                if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception))
                {
                    throw;
                }
            }
        }

        public void ShowPrintPreviewDialog()
        {
            System.Windows.Forms.IntSecurity.SafePrinting.Demand();
            try
            {
                this.AxIWebBrowser2.ExecWB(System.Windows.Forms.NativeMethods.OLECMDID.OLECMDID_PRINTPREVIEW, System.Windows.Forms.NativeMethods.OLECMDEXECOPT.OLECMDEXECOPT_PROMPTUSER, null, IntPtr.Zero);
            }
            catch (Exception exception)
            {
                if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception))
                {
                    throw;
                }
            }
        }

        public void ShowPropertiesDialog()
        {
            try
            {
                this.AxIWebBrowser2.ExecWB(System.Windows.Forms.NativeMethods.OLECMDID.OLECMDID_PROPERTIES, System.Windows.Forms.NativeMethods.OLECMDEXECOPT.OLECMDEXECOPT_PROMPTUSER, null, IntPtr.Zero);
            }
            catch (Exception exception)
            {
                if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception))
                {
                    throw;
                }
            }
        }

        public void ShowSaveAsDialog()
        {
            System.Windows.Forms.IntSecurity.FileDialogSaveFile.Demand();
            try
            {
                this.AxIWebBrowser2.ExecWB(System.Windows.Forms.NativeMethods.OLECMDID.OLECMDID_SAVEAS, System.Windows.Forms.NativeMethods.OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT, null, IntPtr.Zero);
            }
            catch (Exception exception)
            {
                if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception))
                {
                    throw;
                }
            }
        }

        public void Stop()
        {
            try
            {
                this.AxIWebBrowser2.Stop();
            }
            catch (Exception exception)
            {
                if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception))
                {
                    throw;
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x7b)
            {
                int x = System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam);
                int y = System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam);
                if (!this.ShowContextMenu(x, y))
                {
                    this.DefWndProc(ref m);
                }
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        [DefaultValue(true), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("WebBrowserAllowNavigationDescr")]
        public bool AllowNavigation
        {
            get
            {
                return this.webBrowserState[0x40];
            }
            set
            {
                this.webBrowserState[0x40] = value;
                if (this.webBrowserEvent != null)
                {
                    this.webBrowserEvent.AllowNavigation = value;
                }
            }
        }

        [DefaultValue(true), System.Windows.Forms.SRDescription("WebBrowserAllowWebBrowserDropDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public bool AllowWebBrowserDrop
        {
            get
            {
                return this.AxIWebBrowser2.RegisterAsDropTarget;
            }
            set
            {
                if (value != this.AllowWebBrowserDrop)
                {
                    this.AxIWebBrowser2.RegisterAsDropTarget = value;
                    this.Refresh();
                }
            }
        }

        private System.Windows.Forms.UnsafeNativeMethods.IWebBrowser2 AxIWebBrowser2
        {
            get
            {
                if (this.axIWebBrowser2 == null)
                {
                    if (base.IsDisposed)
                    {
                        throw new ObjectDisposedException(base.GetType().Name);
                    }
                    base.TransitionUpTo(WebBrowserHelper.AXState.InPlaceActive);
                }
                if (this.axIWebBrowser2 == null)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("WebBrowserNoCastToIWebBrowser2"));
                }
                return this.axIWebBrowser2;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool CanGoBack
        {
            get
            {
                return this.CanGoBackInternal;
            }
        }

        internal bool CanGoBackInternal
        {
            get
            {
                return this.webBrowserState[8];
            }
            set
            {
                if (value != this.CanGoBackInternal)
                {
                    this.webBrowserState[8] = value;
                    this.OnCanGoBackChanged(EventArgs.Empty);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public bool CanGoForward
        {
            get
            {
                return this.CanGoForwardInternal;
            }
        }

        internal bool CanGoForwardInternal
        {
            get
            {
                return this.webBrowserState[0x10];
            }
            set
            {
                if (value != this.CanGoForwardInternal)
                {
                    this.webBrowserState[0x10] = value;
                    this.OnCanGoForwardChanged(EventArgs.Empty);
                }
            }
        }

        protected override Size DefaultSize
        {
            get
            {
                return new Size(250, 250);
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public HtmlDocument Document
        {
            get
            {
                object obj2 = this.AxIWebBrowser2.Document;
                if (obj2 != null)
                {
                    System.Windows.Forms.UnsafeNativeMethods.IHTMLDocument2 document = obj2 as System.Windows.Forms.UnsafeNativeMethods.IHTMLDocument2;
                    if (document != null)
                    {
                        System.Windows.Forms.UnsafeNativeMethods.IHTMLLocation location = document.GetLocation();
                        if (location != null)
                        {
                            string href = location.GetHref();
                            if (!string.IsNullOrEmpty(href))
                            {
                                Uri url = new Uri(href);
                                EnsureUrlConnectPermission(url);
                                return new HtmlDocument(this.ShimManager, document as System.Windows.Forms.UnsafeNativeMethods.IHTMLDocument);
                            }
                        }
                    }
                }
                return null;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Stream DocumentStream
        {
            get
            {
                HtmlDocument document = this.Document;
                if (document == null)
                {
                    return null;
                }
                System.Windows.Forms.UnsafeNativeMethods.IPersistStreamInit domDocument = document.DomDocument as System.Windows.Forms.UnsafeNativeMethods.IPersistStreamInit;
                if (domDocument == null)
                {
                    return null;
                }
                MemoryStream dataStream = new MemoryStream();
                System.Windows.Forms.UnsafeNativeMethods.IStream pstm = new System.Windows.Forms.UnsafeNativeMethods.ComStreamFromDataStream(dataStream);
                domDocument.Save(pstm, false);
                return new MemoryStream(dataStream.GetBuffer(), 0, (int) dataStream.Length, false);
            }
            set
            {
                this.documentStreamToSetOnLoad = value;
                try
                {
                    this.webBrowserState[2] = true;
                    this.Url = new Uri("about:blank");
                }
                finally
                {
                    this.webBrowserState[2] = false;
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string DocumentText
        {
            get
            {
                Stream documentStream = this.DocumentStream;
                if (documentStream == null)
                {
                    return "";
                }
                StreamReader reader = new StreamReader(documentStream);
                documentStream.Position = 0L;
                return reader.ReadToEnd();
            }
            set
            {
                if (value == null)
                {
                    value = "";
                }
                MemoryStream stream = new MemoryStream(value.Length);
                StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);
                writer.Write(value);
                writer.Flush();
                stream.Position = 0L;
                this.DocumentStream = stream;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public string DocumentTitle
        {
            get
            {
                HtmlDocument document = this.Document;
                if (document == null)
                {
                    return this.AxIWebBrowser2.LocationName;
                }
                System.Windows.Forms.UnsafeNativeMethods.IHTMLDocument2 domDocument = document.DomDocument as System.Windows.Forms.UnsafeNativeMethods.IHTMLDocument2;
                try
                {
                    return domDocument.GetTitle();
                }
                catch (COMException)
                {
                    return "";
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public string DocumentType
        {
            get
            {
                string str = "";
                HtmlDocument document = this.Document;
                if (document == null)
                {
                    return str;
                }
                System.Windows.Forms.UnsafeNativeMethods.IHTMLDocument2 domDocument = document.DomDocument as System.Windows.Forms.UnsafeNativeMethods.IHTMLDocument2;
                try
                {
                    return domDocument.GetMimeType();
                }
                catch (COMException)
                {
                    return "";
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public WebBrowserEncryptionLevel EncryptionLevel
        {
            get
            {
                if (this.Document == null)
                {
                    this.encryptionLevel = WebBrowserEncryptionLevel.Unknown;
                }
                return this.encryptionLevel;
            }
        }

        public override bool Focused
        {
            get
            {
                if (base.Focused)
                {
                    return true;
                }
                IntPtr focus = System.Windows.Forms.UnsafeNativeMethods.GetFocus();
                return ((focus != IntPtr.Zero) && System.Windows.Forms.SafeNativeMethods.IsChild(new HandleRef(this, base.Handle), new HandleRef(null, focus)));
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public bool IsBusy
        {
            get
            {
                if (this.Document == null)
                {
                    return false;
                }
                return this.AxIWebBrowser2.Busy;
            }
        }

        [System.Windows.Forms.SRDescription("WebBrowserIsOfflineDescr"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public bool IsOffline
        {
            get
            {
                return this.AxIWebBrowser2.Offline;
            }
        }

        private bool IsValidUrl
        {
            get
            {
                if (this.Url != null)
                {
                    return (this.Url.AbsoluteUri == "about:blank");
                }
                return true;
            }
        }

        [System.Windows.Forms.SRDescription("WebBrowserIsWebBrowserContextMenuEnabledDescr"), DefaultValue(true), System.Windows.Forms.SRCategory("CatBehavior")]
        public bool IsWebBrowserContextMenuEnabled
        {
            get
            {
                return this.webBrowserState[4];
            }
            set
            {
                this.webBrowserState[4] = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object ObjectForScripting
        {
            get
            {
                return this.objectForScripting;
            }
            set
            {
                if ((value != null) && !Marshal.IsTypeVisibleFromCom(value.GetType()))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("WebBrowserObjectForScriptingComVisibleOnly"));
                }
                this.objectForScripting = value;
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

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public WebBrowserReadyState ReadyState
        {
            get
            {
                if (this.Document == null)
                {
                    return WebBrowserReadyState.Uninitialized;
                }
                return this.AxIWebBrowser2.ReadyState;
            }
        }

        [System.Windows.Forms.SRDescription("WebBrowserScriptErrorsSuppressedDescr"), DefaultValue(false), System.Windows.Forms.SRCategory("CatBehavior")]
        public bool ScriptErrorsSuppressed
        {
            get
            {
                return this.AxIWebBrowser2.Silent;
            }
            set
            {
                if (value != this.ScriptErrorsSuppressed)
                {
                    this.AxIWebBrowser2.Silent = value;
                }
            }
        }

        [System.Windows.Forms.SRDescription("WebBrowserScrollBarsEnabledDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(true)]
        public bool ScrollBarsEnabled
        {
            get
            {
                return this.webBrowserState[0x20];
            }
            set
            {
                if (value != this.webBrowserState[0x20])
                {
                    this.webBrowserState[0x20] = value;
                    this.Refresh();
                }
            }
        }

        internal HtmlShimManager ShimManager
        {
            get
            {
                if (this.htmlShimManager == null)
                {
                    this.htmlShimManager = new HtmlShimManager();
                }
                return this.htmlShimManager;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual string StatusText
        {
            get
            {
                if (this.Document == null)
                {
                    this.statusText = "";
                }
                return this.statusText;
            }
        }

        [DefaultValue((string) null), TypeConverter(typeof(WebBrowserUriTypeConverter)), System.Windows.Forms.SRDescription("WebBrowserUrlDescr"), Bindable(true), System.Windows.Forms.SRCategory("CatBehavior")]
        public Uri Url
        {
            get
            {
                string locationURL = this.AxIWebBrowser2.LocationURL;
                if (string.IsNullOrEmpty(locationURL))
                {
                    return null;
                }
                try
                {
                    return new Uri(locationURL);
                }
                catch (UriFormatException)
                {
                    return null;
                }
            }
            set
            {
                if ((value != null) && (value.ToString() == ""))
                {
                    value = null;
                }
                this.PerformNavigateHelper(this.ReadyNavigateToUrl(value), false, null, null, null);
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Version Version
        {
            get
            {
                FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "mshtml.dll"));
                return new System.Version(versionInfo.FileMajorPart, versionInfo.FileMinorPart, versionInfo.FileBuildPart, versionInfo.FilePrivatePart);
            }
        }

        [System.Windows.Forms.SRDescription("WebBrowserWebBrowserShortcutsEnabledDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(true)]
        public bool WebBrowserShortcutsEnabled
        {
            get
            {
                return this.webBrowserState[1];
            }
            set
            {
                this.webBrowserState[1] = value;
            }
        }

        [ClassInterface(ClassInterfaceType.None)]
        private class WebBrowserEvent : StandardOleMarshalObject, System.Windows.Forms.UnsafeNativeMethods.DWebBrowserEvents2
        {
            private bool allowNavigation;
            private bool haveNavigated;
            private WebBrowser parent;

            public WebBrowserEvent(WebBrowser parent)
            {
                this.parent = parent;
            }

            public void BeforeNavigate2(object pDisp, ref object urlObject, ref object flags, ref object targetFrameName, ref object postData, ref object headers, ref bool cancel)
            {
                if (this.AllowNavigation || !this.haveNavigated)
                {
                    if (targetFrameName == null)
                    {
                        targetFrameName = "";
                    }
                    if (headers == null)
                    {
                        headers = "";
                    }
                    string uriString = (urlObject == null) ? "" : ((string) urlObject);
                    WebBrowserNavigatingEventArgs e = new WebBrowserNavigatingEventArgs(new Uri(uriString), (targetFrameName == null) ? "" : ((string) targetFrameName));
                    this.parent.OnNavigating(e);
                    cancel = e.Cancel;
                }
                else
                {
                    cancel = true;
                }
            }

            public void ClientToHostWindow(ref long cX, ref long cY)
            {
            }

            public void CommandStateChange(long command, bool enable)
            {
                if (command == 2L)
                {
                    this.parent.CanGoBackInternal = enable;
                }
                else if (command == 1L)
                {
                    this.parent.CanGoForwardInternal = enable;
                }
            }

            public void DocumentComplete(object pDisp, ref object urlObject)
            {
                this.haveNavigated = true;
                if ((this.parent.documentStreamToSetOnLoad != null) && (((string) urlObject) == "about:blank"))
                {
                    HtmlDocument document = this.parent.Document;
                    if (document != null)
                    {
                        System.Windows.Forms.UnsafeNativeMethods.IPersistStreamInit domDocument = document.DomDocument as System.Windows.Forms.UnsafeNativeMethods.IPersistStreamInit;
                        System.Windows.Forms.UnsafeNativeMethods.IStream pstm = new System.Windows.Forms.UnsafeNativeMethods.ComStreamFromDataStream(this.parent.documentStreamToSetOnLoad);
                        domDocument.Load(pstm);
                        document.Encoding = "unicode";
                    }
                    this.parent.documentStreamToSetOnLoad = null;
                }
                else
                {
                    string uriString = (urlObject == null) ? "" : urlObject.ToString();
                    WebBrowserDocumentCompletedEventArgs e = new WebBrowserDocumentCompletedEventArgs(new Uri(uriString));
                    this.parent.OnDocumentCompleted(e);
                }
            }

            public void DownloadBegin()
            {
                this.parent.OnFileDownload(EventArgs.Empty);
            }

            public void DownloadComplete()
            {
            }

            public void FileDownload(ref bool cancel)
            {
            }

            public void NavigateComplete2(object pDisp, ref object urlObject)
            {
                string uriString = (urlObject == null) ? "" : ((string) urlObject);
                WebBrowserNavigatedEventArgs e = new WebBrowserNavigatedEventArgs(new Uri(uriString));
                this.parent.OnNavigated(e);
            }

            public void NavigateError(object pDisp, ref object url, ref object frame, ref object statusCode, ref bool cancel)
            {
            }

            public void NewWindow2(ref object ppDisp, ref bool cancel)
            {
                CancelEventArgs e = new CancelEventArgs();
                this.parent.OnNewWindow(e);
                cancel = e.Cancel;
            }

            public void OnFullScreen(bool fullScreen)
            {
            }

            public void OnMenuBar(bool menuBar)
            {
            }

            public void OnQuit()
            {
            }

            public void OnStatusBar(bool statusBar)
            {
            }

            public void OnTheaterMode(bool theaterMode)
            {
            }

            public void OnToolBar(bool toolBar)
            {
            }

            public void OnVisible(bool visible)
            {
            }

            public void PrintTemplateInstantiation(object pDisp)
            {
            }

            public void PrintTemplateTeardown(object pDisp)
            {
            }

            public void PrivacyImpactedStateChange(bool bImpacted)
            {
            }

            public void ProgressChange(int progress, int progressMax)
            {
                WebBrowserProgressChangedEventArgs e = new WebBrowserProgressChangedEventArgs((long) progress, (long) progressMax);
                this.parent.OnProgressChanged(e);
            }

            public void PropertyChange(string szProperty)
            {
            }

            public void SetSecureLockIcon(int secureLockIcon)
            {
                this.parent.encryptionLevel = (WebBrowserEncryptionLevel) secureLockIcon;
                this.parent.OnEncryptionLevelChanged(EventArgs.Empty);
            }

            public void StatusTextChange(string text)
            {
                this.parent.statusText = (text == null) ? "" : text;
                this.parent.OnStatusTextChanged(EventArgs.Empty);
            }

            public void TitleChange(string text)
            {
                this.parent.OnDocumentTitleChanged(EventArgs.Empty);
            }

            public void UpdatePageStatus(object pDisp, ref object nPage, ref object fDone)
            {
            }

            public void WindowClosing(bool isChildWindow, ref bool cancel)
            {
            }

            public void WindowSetHeight(int height)
            {
            }

            public void WindowSetLeft(int left)
            {
            }

            public void WindowSetResizable(bool resizable)
            {
            }

            public void WindowSetTop(int top)
            {
            }

            public void WindowSetWidth(int width)
            {
            }

            public bool AllowNavigation
            {
                get
                {
                    return this.allowNavigation;
                }
                set
                {
                    this.allowNavigation = value;
                }
            }
        }

        [ComVisible(false), SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected class WebBrowserSite : WebBrowserSiteBase, System.Windows.Forms.UnsafeNativeMethods.IDocHostUIHandler
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public WebBrowserSite(WebBrowser host) : base(host)
            {
            }

            internal override void OnPropertyChanged(int dispid)
            {
                if (dispid != -525)
                {
                    base.OnPropertyChanged(dispid);
                }
            }

            int System.Windows.Forms.UnsafeNativeMethods.IDocHostUIHandler.EnableModeless(bool fEnable)
            {
                return -2147467263;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IDocHostUIHandler.FilterDataObject(System.Runtime.InteropServices.ComTypes.IDataObject pDO, out System.Runtime.InteropServices.ComTypes.IDataObject ppDORet)
            {
                ppDORet = null;
                return 1;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IDocHostUIHandler.GetDropTarget(System.Windows.Forms.UnsafeNativeMethods.IOleDropTarget pDropTarget, out System.Windows.Forms.UnsafeNativeMethods.IOleDropTarget ppDropTarget)
            {
                ppDropTarget = null;
                return -2147467263;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IDocHostUIHandler.GetExternal(out object ppDispatch)
            {
                WebBrowser host = (WebBrowser) base.Host;
                ppDispatch = host.ObjectForScripting;
                return 0;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IDocHostUIHandler.GetHostInfo(System.Windows.Forms.NativeMethods.DOCHOSTUIINFO info)
            {
                WebBrowser host = (WebBrowser) base.Host;
                info.dwDoubleClick = 0;
                info.dwFlags = 0x200010;
                if (host.ScrollBarsEnabled)
                {
                    info.dwFlags |= 0x80;
                }
                else
                {
                    info.dwFlags |= 8;
                }
                if (Application.RenderWithVisualStyles)
                {
                    info.dwFlags |= 0x40000;
                }
                else
                {
                    info.dwFlags |= 0x80000;
                }
                return 0;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IDocHostUIHandler.GetOptionKeyPath(string[] pbstrKey, int dw)
            {
                return -2147467263;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IDocHostUIHandler.HideUI()
            {
                return -2147467263;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IDocHostUIHandler.OnDocWindowActivate(bool fActivate)
            {
                return -2147467263;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IDocHostUIHandler.OnFrameWindowActivate(bool fActivate)
            {
                return -2147467263;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IDocHostUIHandler.ResizeBorder(System.Windows.Forms.NativeMethods.COMRECT rect, System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceUIWindow doc, bool fFrameWindow)
            {
                return -2147467263;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IDocHostUIHandler.ShowContextMenu(int dwID, System.Windows.Forms.NativeMethods.POINT pt, object pcmdtReserved, object pdispReserved)
            {
                WebBrowser host = (WebBrowser) base.Host;
                if (host.IsWebBrowserContextMenuEnabled)
                {
                    return 1;
                }
                if ((pt.x == 0) && (pt.y == 0))
                {
                    pt.x = -1;
                    pt.y = -1;
                }
                host.ShowContextMenu(pt.x, pt.y);
                return 0;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IDocHostUIHandler.ShowUI(int dwID, System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceActiveObject activeObject, System.Windows.Forms.NativeMethods.IOleCommandTarget commandTarget, System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceFrame frame, System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceUIWindow doc)
            {
                return 1;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IDocHostUIHandler.TranslateAccelerator(ref System.Windows.Forms.NativeMethods.MSG msg, ref Guid group, int nCmdID)
            {
                WebBrowser host = (WebBrowser) base.Host;
                if (!host.WebBrowserShortcutsEnabled)
                {
                    int num = ((int) msg.wParam) | Control.ModifierKeys;
                    if ((msg.message != 0x102) && System.Enum.IsDefined(typeof(Shortcut), (Shortcut) num))
                    {
                        return 0;
                    }
                }
                return 1;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IDocHostUIHandler.TranslateUrl(int dwTranslate, string strUrlIn, out string pstrUrlOut)
            {
                pstrUrlOut = null;
                return 1;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IDocHostUIHandler.UpdateUI()
            {
                return -2147467263;
            }
        }
    }
}

