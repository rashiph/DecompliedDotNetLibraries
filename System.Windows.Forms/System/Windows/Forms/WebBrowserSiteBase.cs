namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Runtime.InteropServices;

    public class WebBrowserSiteBase : UnsafeNativeMethods.IOleControlSite, UnsafeNativeMethods.IOleClientSite, UnsafeNativeMethods.IOleInPlaceSite, UnsafeNativeMethods.ISimpleFrameSite, UnsafeNativeMethods.IPropertyNotifySink, IDisposable
    {
        private AxHost.ConnectionPointCookie connectionPoint;
        private WebBrowserBase host;

        internal WebBrowserSiteBase(WebBrowserBase h)
        {
            if (h == null)
            {
                throw new ArgumentNullException("h");
            }
            this.host = h;
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.StopEvents();
            }
        }

        internal WebBrowserBase GetAXHost()
        {
            return this.Host;
        }

        private int OnActiveXRectChange(System.Windows.Forms.NativeMethods.COMRECT lprcPosRect)
        {
            this.Host.AXInPlaceObject.SetObjectRects(System.Windows.Forms.NativeMethods.COMRECT.FromXYWH(0, 0, lprcPosRect.right - lprcPosRect.left, lprcPosRect.bottom - lprcPosRect.top), WebBrowserHelper.GetClipRect());
            this.Host.MakeDirty();
            return 0;
        }

        internal virtual void OnPropertyChanged(int dispid)
        {
            try
            {
                ISite site = this.Host.Site;
                if (site != null)
                {
                    IComponentChangeService service = (IComponentChangeService) site.GetService(typeof(IComponentChangeService));
                    if (service != null)
                    {
                        try
                        {
                            service.OnComponentChanging(this.Host, null);
                        }
                        catch (CheckoutException exception)
                        {
                            if (exception != CheckoutException.Canceled)
                            {
                                throw exception;
                            }
                            return;
                        }
                        service.OnComponentChanged(this.Host, null, null, null);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        internal void StartEvents()
        {
            if (this.connectionPoint == null)
            {
                object activeXInstance = this.Host.activeXInstance;
                if (activeXInstance != null)
                {
                    try
                    {
                        this.connectionPoint = new AxHost.ConnectionPointCookie(activeXInstance, this, typeof(UnsafeNativeMethods.IPropertyNotifySink));
                    }
                    catch (Exception exception)
                    {
                        if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                        {
                            throw;
                        }
                    }
                }
            }
        }

        internal void StopEvents()
        {
            if (this.connectionPoint != null)
            {
                this.connectionPoint.Disconnect();
                this.connectionPoint = null;
            }
        }

        int UnsafeNativeMethods.IOleClientSite.GetContainer(out UnsafeNativeMethods.IOleContainer container)
        {
            container = this.Host.GetParentContainer();
            return 0;
        }

        int UnsafeNativeMethods.IOleClientSite.GetMoniker(int dwAssign, int dwWhichMoniker, out object moniker)
        {
            moniker = null;
            return -2147467263;
        }

        int UnsafeNativeMethods.IOleClientSite.OnShowWindow(int fShow)
        {
            return 0;
        }

        int UnsafeNativeMethods.IOleClientSite.RequestNewObjectLayout()
        {
            return -2147467263;
        }

        int UnsafeNativeMethods.IOleClientSite.SaveObject()
        {
            return -2147467263;
        }

        int UnsafeNativeMethods.IOleClientSite.ShowObject()
        {
            if (this.Host.ActiveXState >= WebBrowserHelper.AXState.InPlaceActive)
            {
                IntPtr ptr;
                if (System.Windows.Forms.NativeMethods.Succeeded(this.Host.AXInPlaceObject.GetWindow(out ptr)))
                {
                    if ((this.Host.GetHandleNoCreate() != ptr) && (ptr != IntPtr.Zero))
                    {
                        this.Host.AttachWindow(ptr);
                        this.OnActiveXRectChange(new System.Windows.Forms.NativeMethods.COMRECT(this.Host.Bounds));
                    }
                }
                else if (this.Host.AXInPlaceObject is UnsafeNativeMethods.IOleInPlaceObjectWindowless)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("AXWindowlessControl"));
                }
            }
            return 0;
        }

        int UnsafeNativeMethods.IOleControlSite.GetExtendedControl(out object ppDisp)
        {
            ppDisp = null;
            return -2147467263;
        }

        int UnsafeNativeMethods.IOleControlSite.LockInPlaceActive(int fLock)
        {
            return -2147467263;
        }

        int UnsafeNativeMethods.IOleControlSite.OnControlInfoChanged()
        {
            return 0;
        }

        int UnsafeNativeMethods.IOleControlSite.OnFocus(int fGotFocus)
        {
            return 0;
        }

        int UnsafeNativeMethods.IOleControlSite.ShowPropertyFrame()
        {
            return -2147467263;
        }

        int UnsafeNativeMethods.IOleControlSite.TransformCoords(System.Windows.Forms.NativeMethods._POINTL pPtlHimetric, System.Windows.Forms.NativeMethods.tagPOINTF pPtfContainer, int dwFlags)
        {
            if ((dwFlags & 4) != 0)
            {
                if ((dwFlags & 2) == 0)
                {
                    if ((dwFlags & 1) == 0)
                    {
                        return -2147024809;
                    }
                    pPtfContainer.x = WebBrowserHelper.HM2Pix(pPtlHimetric.x, WebBrowserHelper.LogPixelsX);
                    pPtfContainer.y = WebBrowserHelper.HM2Pix(pPtlHimetric.y, WebBrowserHelper.LogPixelsY);
                }
                else
                {
                    pPtfContainer.x = WebBrowserHelper.HM2Pix(pPtlHimetric.x, WebBrowserHelper.LogPixelsX);
                    pPtfContainer.y = WebBrowserHelper.HM2Pix(pPtlHimetric.y, WebBrowserHelper.LogPixelsY);
                }
            }
            else
            {
                if ((dwFlags & 8) != 0)
                {
                    if ((dwFlags & 2) != 0)
                    {
                        pPtlHimetric.x = WebBrowserHelper.Pix2HM((int) pPtfContainer.x, WebBrowserHelper.LogPixelsX);
                        pPtlHimetric.y = WebBrowserHelper.Pix2HM((int) pPtfContainer.y, WebBrowserHelper.LogPixelsY);
                        goto Label_00F6;
                    }
                    if ((dwFlags & 1) != 0)
                    {
                        pPtlHimetric.x = WebBrowserHelper.Pix2HM((int) pPtfContainer.x, WebBrowserHelper.LogPixelsX);
                        pPtlHimetric.y = WebBrowserHelper.Pix2HM((int) pPtfContainer.y, WebBrowserHelper.LogPixelsY);
                        goto Label_00F6;
                    }
                }
                return -2147024809;
            }
        Label_00F6:
            return 0;
        }

        int UnsafeNativeMethods.IOleControlSite.TranslateAccelerator(ref System.Windows.Forms.NativeMethods.MSG pMsg, int grfModifiers)
        {
            int num;
            this.Host.SetAXHostState(WebBrowserHelper.siteProcessedInputKey, true);
            Message msg = new Message {
                Msg = pMsg.message,
                WParam = pMsg.wParam,
                LParam = pMsg.lParam,
                HWnd = pMsg.hwnd
            };
            try
            {
                num = (this.Host.PreProcessControlMessage(ref msg) == PreProcessControlState.MessageProcessed) ? 0 : 1;
            }
            finally
            {
                this.Host.SetAXHostState(WebBrowserHelper.siteProcessedInputKey, false);
            }
            return num;
        }

        int UnsafeNativeMethods.IOleInPlaceSite.CanInPlaceActivate()
        {
            return 0;
        }

        int UnsafeNativeMethods.IOleInPlaceSite.ContextSensitiveHelp(int fEnterMode)
        {
            return -2147467263;
        }

        int UnsafeNativeMethods.IOleInPlaceSite.DeactivateAndUndo()
        {
            return this.Host.AXInPlaceObject.UIDeactivate();
        }

        int UnsafeNativeMethods.IOleInPlaceSite.DiscardUndoState()
        {
            return 0;
        }

        IntPtr UnsafeNativeMethods.IOleInPlaceSite.GetWindow()
        {
            IntPtr parent;
            try
            {
                parent = UnsafeNativeMethods.GetParent(new HandleRef(this.Host, this.Host.Handle));
            }
            catch (Exception)
            {
                throw;
            }
            return parent;
        }

        int UnsafeNativeMethods.IOleInPlaceSite.GetWindowContext(out UnsafeNativeMethods.IOleInPlaceFrame ppFrame, out UnsafeNativeMethods.IOleInPlaceUIWindow ppDoc, System.Windows.Forms.NativeMethods.COMRECT lprcPosRect, System.Windows.Forms.NativeMethods.COMRECT lprcClipRect, System.Windows.Forms.NativeMethods.tagOIFI lpFrameInfo)
        {
            ppDoc = null;
            ppFrame = this.Host.GetParentContainer();
            lprcPosRect.left = this.Host.Bounds.X;
            lprcPosRect.top = this.Host.Bounds.Y;
            lprcPosRect.right = this.Host.Bounds.Width + this.Host.Bounds.X;
            lprcPosRect.bottom = this.Host.Bounds.Height + this.Host.Bounds.Y;
            lprcClipRect = WebBrowserHelper.GetClipRect();
            if (lpFrameInfo != null)
            {
                lpFrameInfo.cb = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.tagOIFI));
                lpFrameInfo.fMDIApp = false;
                lpFrameInfo.hAccel = IntPtr.Zero;
                lpFrameInfo.cAccelEntries = 0;
                lpFrameInfo.hwndFrame = (this.Host.ParentInternal == null) ? IntPtr.Zero : this.Host.ParentInternal.Handle;
            }
            return 0;
        }

        int UnsafeNativeMethods.IOleInPlaceSite.OnInPlaceActivate()
        {
            this.Host.ActiveXState = WebBrowserHelper.AXState.InPlaceActive;
            this.OnActiveXRectChange(new System.Windows.Forms.NativeMethods.COMRECT(this.Host.Bounds));
            return 0;
        }

        int UnsafeNativeMethods.IOleInPlaceSite.OnInPlaceDeactivate()
        {
            if (this.Host.ActiveXState == WebBrowserHelper.AXState.UIActive)
            {
                ((UnsafeNativeMethods.IOleInPlaceSite) this).OnUIDeactivate(0);
            }
            this.Host.GetParentContainer().OnInPlaceDeactivate(this.Host);
            this.Host.ActiveXState = WebBrowserHelper.AXState.Running;
            return 0;
        }

        int UnsafeNativeMethods.IOleInPlaceSite.OnPosRectChange(System.Windows.Forms.NativeMethods.COMRECT lprcPosRect)
        {
            return this.OnActiveXRectChange(lprcPosRect);
        }

        int UnsafeNativeMethods.IOleInPlaceSite.OnUIActivate()
        {
            this.Host.ActiveXState = WebBrowserHelper.AXState.UIActive;
            this.Host.GetParentContainer().OnUIActivate(this.Host);
            return 0;
        }

        int UnsafeNativeMethods.IOleInPlaceSite.OnUIDeactivate(int fUndoable)
        {
            this.Host.GetParentContainer().OnUIDeactivate(this.Host);
            if (this.Host.ActiveXState > WebBrowserHelper.AXState.InPlaceActive)
            {
                this.Host.ActiveXState = WebBrowserHelper.AXState.InPlaceActive;
            }
            return 0;
        }

        int UnsafeNativeMethods.IOleInPlaceSite.Scroll(System.Windows.Forms.NativeMethods.tagSIZE scrollExtant)
        {
            return 1;
        }

        void UnsafeNativeMethods.IPropertyNotifySink.OnChanged(int dispid)
        {
            if (this.Host.NoComponentChangeEvents == 0)
            {
                WebBrowserBase host = this.Host;
                host.NoComponentChangeEvents++;
                try
                {
                    this.OnPropertyChanged(dispid);
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    WebBrowserBase base2 = this.Host;
                    base2.NoComponentChangeEvents--;
                }
            }
        }

        int UnsafeNativeMethods.IPropertyNotifySink.OnRequestEdit(int dispid)
        {
            return 0;
        }

        int UnsafeNativeMethods.ISimpleFrameSite.PostMessageFilter(IntPtr hwnd, int msg, IntPtr wp, IntPtr lp, ref IntPtr plResult, int dwCookie)
        {
            return 1;
        }

        int UnsafeNativeMethods.ISimpleFrameSite.PreMessageFilter(IntPtr hwnd, int msg, IntPtr wp, IntPtr lp, ref IntPtr plResult, ref int pdwCookie)
        {
            return 0;
        }

        internal WebBrowserBase Host
        {
            get
            {
                return this.host;
            }
        }
    }
}

