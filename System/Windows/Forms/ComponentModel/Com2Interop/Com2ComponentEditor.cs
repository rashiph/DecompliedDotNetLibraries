namespace System.Windows.Forms.ComponentModel.Com2Interop
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    internal class Com2ComponentEditor : WindowsFormsComponentEditor
    {
        public override bool EditComponent(ITypeDescriptorContext context, object obj, IWin32Window parent)
        {
            IntPtr handle = (parent == null) ? IntPtr.Zero : parent.Handle;
            if (obj is System.Windows.Forms.NativeMethods.IPerPropertyBrowsing)
            {
                Guid empty = Guid.Empty;
                if ((((System.Windows.Forms.NativeMethods.IPerPropertyBrowsing) obj).MapPropertyToPage(-1, out empty) == 0) && !empty.Equals(Guid.Empty))
                {
                    object pobjs = obj;
                    Guid[] pClsid = new Guid[] { empty };
                    SafeNativeMethods.OleCreatePropertyFrame(new HandleRef(parent, handle), 0, 0, "PropertyPages", 1, ref pobjs, 1, pClsid, Application.CurrentCulture.LCID, 0, IntPtr.Zero);
                    return true;
                }
            }
            if (obj is System.Windows.Forms.NativeMethods.ISpecifyPropertyPages)
            {
                Exception exception;
                bool flag = false;
                try
                {
                    System.Windows.Forms.NativeMethods.tagCAUUID pPages = new System.Windows.Forms.NativeMethods.tagCAUUID();
                    try
                    {
                        ((System.Windows.Forms.NativeMethods.ISpecifyPropertyPages) obj).GetPages(pPages);
                        if (pPages.cElems <= 0)
                        {
                            return false;
                        }
                    }
                    catch
                    {
                        return false;
                    }
                    try
                    {
                        object obj3 = obj;
                        SafeNativeMethods.OleCreatePropertyFrame(new HandleRef(parent, handle), 0, 0, "PropertyPages", 1, ref obj3, pPages.cElems, new HandleRef(pPages, pPages.pElems), Application.CurrentCulture.LCID, 0, IntPtr.Zero);
                        return true;
                    }
                    finally
                    {
                        if (pPages.pElems != IntPtr.Zero)
                        {
                            Marshal.FreeCoTaskMem(pPages.pElems);
                        }
                    }
                }
                catch (Exception exception2)
                {
                    flag = true;
                    exception = exception2;
                }
                if (flag)
                {
                    string text = System.Windows.Forms.SR.GetString("ErrorPropertyPageFailed");
                    IUIService service = (context != null) ? ((IUIService) context.GetService(typeof(IUIService))) : null;
                    if (service == null)
                    {
                        RTLAwareMessageBox.Show(null, text, System.Windows.Forms.SR.GetString("PropertyGridTitle"), MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, 0);
                    }
                    else if (exception != null)
                    {
                        service.ShowError(exception, text);
                    }
                    else
                    {
                        service.ShowError(text);
                    }
                }
            }
            return false;
        }

        public static bool NeedsComponentEditor(object obj)
        {
            if (obj is System.Windows.Forms.NativeMethods.IPerPropertyBrowsing)
            {
                Guid empty = Guid.Empty;
                if ((((System.Windows.Forms.NativeMethods.IPerPropertyBrowsing) obj).MapPropertyToPage(-1, out empty) == 0) && !empty.Equals(Guid.Empty))
                {
                    return true;
                }
            }
            if (obj is System.Windows.Forms.NativeMethods.ISpecifyPropertyPages)
            {
                try
                {
                    System.Windows.Forms.NativeMethods.tagCAUUID pPages = new System.Windows.Forms.NativeMethods.tagCAUUID();
                    try
                    {
                        ((System.Windows.Forms.NativeMethods.ISpecifyPropertyPages) obj).GetPages(pPages);
                        if (pPages.cElems > 0)
                        {
                            return true;
                        }
                    }
                    finally
                    {
                        if (pPages.pElems != IntPtr.Zero)
                        {
                            Marshal.FreeCoTaskMem(pPages.pElems);
                        }
                    }
                }
                catch
                {
                }
            }
            return false;
        }
    }
}

