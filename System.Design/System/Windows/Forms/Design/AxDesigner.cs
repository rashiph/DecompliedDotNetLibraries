namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Design;
    using System.Diagnostics;
    using System.Windows.Forms;

    internal class AxDesigner : ControlDesigner
    {
        private static TraceSwitch AxDesignerSwitch = new TraceSwitch("AxDesigner", "ActiveX Designer Trace");
        private bool dragdropRevoked;
        private WebBrowserBase webBrowserBase;

        public override void Initialize(IComponent component)
        {
            base.Initialize(component);
            base.AutoResizeHandles = true;
            this.webBrowserBase = (WebBrowserBase) component;
        }

        public override void InitializeNewComponent(IDictionary defaultValues)
        {
            try
            {
                base.InitializeNewComponent(defaultValues);
            }
            catch
            {
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            object obj2 = properties["Enabled"];
            base.PreFilterProperties(properties);
            if (obj2 != null)
            {
                properties["Enabled"] = obj2;
            }
            properties["SelectionStyle"] = TypeDescriptor.CreateProperty(typeof(AxDesigner), "SelectionStyle", typeof(int), new Attribute[] { BrowsableAttribute.No, DesignerSerializationVisibilityAttribute.Hidden, DesignOnlyAttribute.Yes });
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x84:
                    if (!this.dragdropRevoked)
                    {
                        IntPtr handle = this.Control.Handle;
                        this.dragdropRevoked = true;
                        while ((handle != IntPtr.Zero) && this.dragdropRevoked)
                        {
                            System.Design.NativeMethods.RevokeDragDrop(handle);
                            handle = System.Design.NativeMethods.GetWindow(handle, 5);
                        }
                    }
                    base.WndProc(ref m);
                    if ((((int) ((long) m.Result)) != -1) && (((int) ((long) m.Result)) <= 1))
                    {
                        break;
                    }
                    m.Result = (IntPtr) 1;
                    return;

                case 0x210:
                    if (((int) ((long) m.WParam)) == 1)
                    {
                        base.HookChildHandles(m.LParam);
                    }
                    base.WndProc(ref m);
                    return;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        private int SelectionStyle
        {
            get
            {
                return 0;
            }
            set
            {
            }
        }
    }
}

