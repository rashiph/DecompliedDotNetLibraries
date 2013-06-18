namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Runtime.InteropServices;

    internal class WebBrowserContainer : UnsafeNativeMethods.IOleContainer, UnsafeNativeMethods.IOleInPlaceFrame
    {
        private IContainer assocContainer;
        private Hashtable components;
        private Hashtable containerCache = new Hashtable();
        private WebBrowserBase ctlInEditMode;
        private WebBrowserBase parent;
        private WebBrowserBase siteActive;
        private WebBrowserBase siteUIActive;

        internal WebBrowserContainer(WebBrowserBase parent)
        {
            this.parent = parent;
        }

        internal void AddControl(Control ctl)
        {
            if (this.containerCache.Contains(ctl))
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("AXDuplicateControl", new object[] { this.GetNameForControl(ctl) }), "ctl");
            }
            this.containerCache.Add(ctl, ctl);
            if (this.assocContainer == null)
            {
                ISite site = ctl.Site;
                if (site != null)
                {
                    this.assocContainer = site.Container;
                    IComponentChangeService service = (IComponentChangeService) site.GetService(typeof(IComponentChangeService));
                    if (service != null)
                    {
                        service.ComponentRemoved += new ComponentEventHandler(this.OnComponentRemoved);
                    }
                }
            }
        }

        private void FillComponentsTable(IContainer container)
        {
            if (container != null)
            {
                ComponentCollection components = container.Components;
                if (components != null)
                {
                    this.components = new Hashtable();
                    foreach (IComponent component in components)
                    {
                        if (((component is Control) && (component != this.parent)) && (component.Site != null))
                        {
                            this.components.Add(component, component);
                        }
                    }
                    return;
                }
            }
            bool flag = true;
            Control[] array = new Control[this.containerCache.Values.Count];
            this.containerCache.Values.CopyTo(array, 0);
            if (array != null)
            {
                if ((array.Length > 0) && (this.components == null))
                {
                    this.components = new Hashtable();
                    flag = false;
                }
                for (int i = 0; i < array.Length; i++)
                {
                    if (flag && !this.components.Contains(array[i]))
                    {
                        this.components.Add(array[i], array[i]);
                    }
                }
            }
            this.GetAllChildren(this.parent);
        }

        internal static WebBrowserContainer FindContainerForControl(WebBrowserBase ctl)
        {
            if (ctl != null)
            {
                if (ctl.container != null)
                {
                    return ctl.container;
                }
                if (ctl.ContainingControl != null)
                {
                    WebBrowserContainer container = ctl.CreateWebBrowserContainer();
                    if (container.RegisterControl(ctl))
                    {
                        container.AddControl(ctl);
                        return container;
                    }
                }
            }
            return null;
        }

        private void GetAllChildren(Control ctl)
        {
            if (ctl != null)
            {
                if (this.components == null)
                {
                    this.components = new Hashtable();
                }
                if ((ctl != this.parent) && !this.components.Contains(ctl))
                {
                    this.components.Add(ctl, ctl);
                }
                foreach (Control control in ctl.Controls)
                {
                    this.GetAllChildren(control);
                }
            }
        }

        private Hashtable GetComponents()
        {
            return this.GetComponents(this.GetParentsContainer());
        }

        private Hashtable GetComponents(IContainer cont)
        {
            this.FillComponentsTable(cont);
            return this.components;
        }

        internal string GetNameForControl(Control ctl)
        {
            string str = (ctl.Site != null) ? ctl.Site.Name : ctl.Name;
            return (str ?? "");
        }

        private IContainer GetParentIContainer()
        {
            ISite site = this.parent.Site;
            if ((site != null) && site.DesignMode)
            {
                return site.Container;
            }
            return null;
        }

        private IContainer GetParentsContainer()
        {
            IContainer parentIContainer = this.GetParentIContainer();
            if (parentIContainer != null)
            {
                return parentIContainer;
            }
            return this.assocContainer;
        }

        private void ListAXControls(ArrayList list, bool fuseOcx)
        {
            Hashtable components = this.GetComponents();
            if (components != null)
            {
                Control[] array = new Control[components.Keys.Count];
                components.Keys.CopyTo(array, 0);
                if (array != null)
                {
                    for (int i = 0; i < array.Length; i++)
                    {
                        Control control = array[i];
                        WebBrowserBase base2 = control as WebBrowserBase;
                        if (base2 != null)
                        {
                            if (fuseOcx)
                            {
                                object activeXInstance = base2.activeXInstance;
                                if (activeXInstance != null)
                                {
                                    list.Add(activeXInstance);
                                }
                            }
                            else
                            {
                                list.Add(control);
                            }
                        }
                    }
                }
            }
        }

        private void OnComponentRemoved(object sender, ComponentEventArgs e)
        {
            Control component = e.Component as Control;
            if ((sender == this.assocContainer) && (component != null))
            {
                this.RemoveControl(component);
            }
        }

        internal void OnExitEditMode(WebBrowserBase ctl)
        {
            if (this.ctlInEditMode == ctl)
            {
                this.ctlInEditMode = null;
            }
        }

        internal void OnInPlaceDeactivate(WebBrowserBase site)
        {
            if (this.siteActive == site)
            {
                this.siteActive = null;
                ContainerControl control = this.parent.FindContainerControlInternal();
                if (control != null)
                {
                    control.SetActiveControlInternal(null);
                }
            }
        }

        internal void OnUIActivate(WebBrowserBase site)
        {
            if (this.siteUIActive != site)
            {
                if ((this.siteUIActive != null) && (this.siteUIActive != site))
                {
                    this.siteUIActive.AXInPlaceObject.UIDeactivate();
                }
                site.AddSelectionHandler();
                this.siteUIActive = site;
                ContainerControl containingControl = site.ContainingControl;
                if ((containingControl != null) && containingControl.Contains(site))
                {
                    containingControl.SetActiveControlInternal(site);
                }
            }
        }

        internal void OnUIDeactivate(WebBrowserBase site)
        {
            this.siteUIActive = null;
            site.RemoveSelectionHandler();
            site.SetSelectionStyle(WebBrowserHelper.SelectionStyle.Selected);
            site.SetEditMode(WebBrowserHelper.AXEditMode.None);
        }

        private bool RegisterControl(WebBrowserBase ctl)
        {
            ISite site = ctl.Site;
            if (site != null)
            {
                IContainer container = site.Container;
                if (container != null)
                {
                    if (this.assocContainer != null)
                    {
                        return (container == this.assocContainer);
                    }
                    this.assocContainer = container;
                    IComponentChangeService service = (IComponentChangeService) site.GetService(typeof(IComponentChangeService));
                    if (service != null)
                    {
                        service.ComponentRemoved += new ComponentEventHandler(this.OnComponentRemoved);
                    }
                    return true;
                }
            }
            return false;
        }

        internal void RemoveControl(Control ctl)
        {
            this.containerCache.Remove(ctl);
        }

        int UnsafeNativeMethods.IOleContainer.EnumObjects(int grfFlags, out UnsafeNativeMethods.IEnumUnknown ppenum)
        {
            ppenum = null;
            if ((grfFlags & 1) != 0)
            {
                ArrayList list = new ArrayList();
                this.ListAXControls(list, true);
                if (list.Count > 0)
                {
                    object[] array = new object[list.Count];
                    list.CopyTo(array, 0);
                    ppenum = new AxHost.EnumUnknown(array);
                    return 0;
                }
            }
            ppenum = new AxHost.EnumUnknown(null);
            return 0;
        }

        int UnsafeNativeMethods.IOleContainer.LockContainer(bool fLock)
        {
            return -2147467263;
        }

        int UnsafeNativeMethods.IOleContainer.ParseDisplayName(object pbc, string pszDisplayName, int[] pchEaten, object[] ppmkOut)
        {
            if (ppmkOut != null)
            {
                ppmkOut[0] = null;
            }
            return -2147467263;
        }

        int UnsafeNativeMethods.IOleInPlaceFrame.ContextSensitiveHelp(int fEnterMode)
        {
            return 0;
        }

        int UnsafeNativeMethods.IOleInPlaceFrame.EnableModeless(bool fEnable)
        {
            return -2147467263;
        }

        int UnsafeNativeMethods.IOleInPlaceFrame.GetBorder(System.Windows.Forms.NativeMethods.COMRECT lprectBorder)
        {
            return -2147467263;
        }

        IntPtr UnsafeNativeMethods.IOleInPlaceFrame.GetWindow()
        {
            return this.parent.Handle;
        }

        int UnsafeNativeMethods.IOleInPlaceFrame.InsertMenus(IntPtr hmenuShared, System.Windows.Forms.NativeMethods.tagOleMenuGroupWidths lpMenuWidths)
        {
            return 0;
        }

        int UnsafeNativeMethods.IOleInPlaceFrame.RemoveMenus(IntPtr hmenuShared)
        {
            return -2147467263;
        }

        int UnsafeNativeMethods.IOleInPlaceFrame.RequestBorderSpace(System.Windows.Forms.NativeMethods.COMRECT pborderwidths)
        {
            return -2147467263;
        }

        int UnsafeNativeMethods.IOleInPlaceFrame.SetActiveObject(UnsafeNativeMethods.IOleInPlaceActiveObject pActiveObject, string pszObjName)
        {
            if (pActiveObject == null)
            {
                if (this.ctlInEditMode != null)
                {
                    this.ctlInEditMode.SetEditMode(WebBrowserHelper.AXEditMode.None);
                    this.ctlInEditMode = null;
                }
                return 0;
            }
            WebBrowserBase aXHost = null;
            UnsafeNativeMethods.IOleObject obj2 = pActiveObject as UnsafeNativeMethods.IOleObject;
            if (obj2 != null)
            {
                try
                {
                    WebBrowserSiteBase clientSite = obj2.GetClientSite() as WebBrowserSiteBase;
                    if (clientSite != null)
                    {
                        aXHost = clientSite.GetAXHost();
                    }
                }
                catch (COMException)
                {
                }
                if (this.ctlInEditMode != null)
                {
                    this.ctlInEditMode.SetSelectionStyle(WebBrowserHelper.SelectionStyle.Selected);
                    this.ctlInEditMode.SetEditMode(WebBrowserHelper.AXEditMode.None);
                }
                if (aXHost == null)
                {
                    this.ctlInEditMode = null;
                }
                else if (!aXHost.IsUserMode)
                {
                    this.ctlInEditMode = aXHost;
                    aXHost.SetEditMode(WebBrowserHelper.AXEditMode.Object);
                    aXHost.AddSelectionHandler();
                    aXHost.SetSelectionStyle(WebBrowserHelper.SelectionStyle.Active);
                }
            }
            return 0;
        }

        int UnsafeNativeMethods.IOleInPlaceFrame.SetBorderSpace(System.Windows.Forms.NativeMethods.COMRECT pborderwidths)
        {
            return -2147467263;
        }

        int UnsafeNativeMethods.IOleInPlaceFrame.SetMenu(IntPtr hmenuShared, IntPtr holemenu, IntPtr hwndActiveObject)
        {
            return -2147467263;
        }

        int UnsafeNativeMethods.IOleInPlaceFrame.SetStatusText(string pszStatusText)
        {
            return -2147467263;
        }

        int UnsafeNativeMethods.IOleInPlaceFrame.TranslateAccelerator(ref System.Windows.Forms.NativeMethods.MSG lpmsg, short wID)
        {
            return 1;
        }
    }
}

