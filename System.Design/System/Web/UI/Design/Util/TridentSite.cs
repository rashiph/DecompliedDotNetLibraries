namespace System.Web.UI.Design.Util
{
    using System;
    using System.Design;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Windows.Forms;

    [ClassInterface(ClassInterfaceType.None)]
    internal class TridentSite : System.Design.NativeMethods.IOleClientSite, System.Design.NativeMethods.IOleDocumentSite, System.Design.NativeMethods.IOleInPlaceSite, System.Design.NativeMethods.IOleInPlaceFrame, System.Design.NativeMethods.IDocHostUIHandler
    {
        protected Control parentControl;
        protected EventHandler resizeHandler;
        protected System.Design.NativeMethods.IHTMLDocument2 tridentDocument;
        protected System.Design.NativeMethods.IOleObject tridentOleObject;
        protected System.Design.NativeMethods.IOleDocumentView tridentView;

        public TridentSite(Control parent)
        {
            this.parentControl = parent;
            this.resizeHandler = new EventHandler(this.OnParentResize);
            this.parentControl.Resize += this.resizeHandler;
            this.CreateDocument();
        }

        public void Activate()
        {
            this.ActivateDocument();
        }

        protected void ActivateDocument()
        {
            try
            {
                System.Design.NativeMethods.COMRECT rect = new System.Design.NativeMethods.COMRECT();
                System.Design.NativeMethods.GetClientRect(this.parentControl.Handle, rect);
                this.tridentOleObject.DoVerb(-4, IntPtr.Zero, this, 0, this.parentControl.Handle, rect);
            }
            catch (Exception)
            {
            }
        }

        public virtual int ActivateMe(System.Design.NativeMethods.IOleDocumentView pViewToActivate)
        {
            if (pViewToActivate == null)
            {
                return -2147024809;
            }
            System.Design.NativeMethods.COMRECT rect = new System.Design.NativeMethods.COMRECT();
            System.Design.NativeMethods.GetClientRect(this.parentControl.Handle, rect);
            this.tridentView = pViewToActivate;
            this.tridentView.SetInPlaceSite(this);
            this.tridentView.UIActivate(1);
            this.tridentView.SetRect(rect);
            this.tridentView.Show(1);
            return 0;
        }

        public virtual int CanInPlaceActivate()
        {
            return 0;
        }

        public virtual void ContextSensitiveHelp(int fEnterMode)
        {
            throw new COMException(string.Empty, -2147467263);
        }

        protected void CreateDocument()
        {
            try
            {
                this.tridentDocument = (System.Design.NativeMethods.IHTMLDocument2) new System.Design.NativeMethods.HTMLDocument();
                this.tridentOleObject = (System.Design.NativeMethods.IOleObject) this.tridentDocument;
                this.tridentOleObject.SetClientSite(this);
                ((System.Design.NativeMethods.IPersistStreamInit) this.tridentDocument).InitNew();
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public virtual void DeactivateAndUndo()
        {
        }

        public virtual void DiscardUndoState()
        {
            throw new COMException("Not implemented", -2147467263);
        }

        public virtual int EnableModeless(bool fEnable)
        {
            return 0;
        }

        public virtual void EnableModeless(int fEnable)
        {
        }

        public virtual int FilterDataObject(System.Runtime.InteropServices.ComTypes.IDataObject pDO, out System.Runtime.InteropServices.ComTypes.IDataObject ppDORet)
        {
            ppDORet = null;
            return 0;
        }

        public virtual void GetBorder(System.Design.NativeMethods.COMRECT lprectBorder)
        {
            throw new COMException(string.Empty, -2147467263);
        }

        public virtual int GetContainer(out System.Design.NativeMethods.IOleContainer ppContainer)
        {
            ppContainer = null;
            return -2147467262;
        }

        public System.Design.NativeMethods.IHTMLDocument2 GetDocument()
        {
            return this.tridentDocument;
        }

        public virtual int GetDropTarget(System.Design.NativeMethods.IOleDropTarget pDropTarget, out System.Design.NativeMethods.IOleDropTarget ppDropTarget)
        {
            ppDropTarget = null;
            return 1;
        }

        public virtual int GetExternal(out object ppDispatch)
        {
            ppDispatch = null;
            return 0;
        }

        public virtual int GetHostInfo(System.Design.NativeMethods.DOCHOSTUIINFO info)
        {
            info.dwDoubleClick = 0;
            info.dwFlags = 0x95;
            return 0;
        }

        public virtual object GetMoniker(int dwAssign, int dwWhichMoniker)
        {
            throw new COMException(string.Empty, -2147467263);
        }

        public virtual int GetOptionKeyPath(string[] pbstrKey, int dw)
        {
            pbstrKey[0] = null;
            return 0;
        }

        public virtual IntPtr GetWindow()
        {
            return this.parentControl.Handle;
        }

        public virtual void GetWindowContext(out System.Design.NativeMethods.IOleInPlaceFrame ppFrame, out System.Design.NativeMethods.IOleInPlaceUIWindow ppDoc, System.Design.NativeMethods.COMRECT lprcPosRect, System.Design.NativeMethods.COMRECT lprcClipRect, System.Design.NativeMethods.tagOIFI lpFrameInfo)
        {
            ppFrame = this;
            ppDoc = null;
            System.Design.NativeMethods.GetClientRect(this.parentControl.Handle, lprcPosRect);
            System.Design.NativeMethods.GetClientRect(this.parentControl.Handle, lprcClipRect);
            lpFrameInfo.cb = Marshal.SizeOf(typeof(System.Design.NativeMethods.tagOIFI));
            lpFrameInfo.fMDIApp = 0;
            lpFrameInfo.hwndFrame = this.parentControl.Handle;
            lpFrameInfo.hAccel = IntPtr.Zero;
            lpFrameInfo.cAccelEntries = 0;
        }

        public virtual int HideUI()
        {
            return 0;
        }

        public virtual void InsertMenus(IntPtr hmenuShared, object lpMenuWidths)
        {
            throw new COMException(string.Empty, -2147467263);
        }

        public virtual int OnDocWindowActivate(bool fActivate)
        {
            return -2147467263;
        }

        public virtual int OnFrameWindowActivate(bool fActivate)
        {
            return -2147467263;
        }

        public virtual void OnInPlaceActivate()
        {
        }

        public virtual void OnInPlaceDeactivate()
        {
        }

        protected virtual void OnParentResize(object src, EventArgs e)
        {
            if (this.tridentView != null)
            {
                System.Design.NativeMethods.COMRECT rect = new System.Design.NativeMethods.COMRECT();
                System.Design.NativeMethods.GetClientRect(this.parentControl.Handle, rect);
                this.tridentView.SetRect(rect);
            }
        }

        public virtual int OnPosRectChange(System.Design.NativeMethods.COMRECT lprcPosRect)
        {
            return 0;
        }

        public virtual void OnShowWindow(int fShow)
        {
        }

        public virtual void OnUIActivate()
        {
        }

        public virtual void OnUIDeactivate(int fUndoable)
        {
        }

        public virtual void RemoveMenus(IntPtr hmenuShared)
        {
            throw new COMException(string.Empty, -2147467263);
        }

        public virtual void RequestBorderSpace(System.Design.NativeMethods.COMRECT pborderwidths)
        {
            throw new COMException(string.Empty, -2147467263);
        }

        public virtual void RequestNewObjectLayout()
        {
        }

        public virtual int ResizeBorder(System.Design.NativeMethods.COMRECT rect, System.Design.NativeMethods.IOleInPlaceUIWindow doc, bool fFrameWindow)
        {
            return -2147467263;
        }

        public virtual void SaveObject()
        {
        }

        public virtual int Scroll(System.Design.NativeMethods.tagSIZE scrollExtant)
        {
            return -2147467263;
        }

        public virtual void SetActiveObject(System.Design.NativeMethods.IOleInPlaceActiveObject pActiveObject, string pszObjName)
        {
        }

        public virtual void SetBorderSpace(System.Design.NativeMethods.COMRECT pborderwidths)
        {
            throw new COMException(string.Empty, -2147467263);
        }

        public virtual void SetMenu(IntPtr hmenuShared, IntPtr holemenu, IntPtr hwndActiveObject)
        {
            throw new COMException(string.Empty, -2147467263);
        }

        public virtual void SetStatusText(string pszStatusText)
        {
        }

        public virtual int ShowContextMenu(int dwID, System.Design.NativeMethods.POINT pt, object pcmdtReserved, object pdispReserved)
        {
            return 0;
        }

        public virtual void ShowObject()
        {
        }

        public virtual int ShowUI(int dwID, System.Design.NativeMethods.IOleInPlaceActiveObject activeObject, System.Design.NativeMethods.IOleCommandTarget commandTarget, System.Design.NativeMethods.IOleInPlaceFrame frame, System.Design.NativeMethods.IOleInPlaceUIWindow doc)
        {
            return 0;
        }

        public virtual int TranslateAccelerator(ref System.Design.NativeMethods.MSG lpmsg, short wID)
        {
            return 1;
        }

        public virtual int TranslateAccelerator(ref System.Design.NativeMethods.MSG msg, ref Guid group, int nCmdID)
        {
            return 0;
        }

        public virtual int TranslateUrl(int dwTranslate, string strUrlIn, out string pstrUrlOut)
        {
            pstrUrlOut = null;
            return -2147467263;
        }

        public virtual int UpdateUI()
        {
            return 0;
        }
    }
}

