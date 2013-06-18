namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    internal class ListViewDesigner : ControlDesigner
    {
        private DesignerActionListCollection _actionLists;
        private System.Design.NativeMethods.HDHITTESTINFO hdrhit = new System.Design.NativeMethods.HDHITTESTINFO();
        private bool inShowErrorDialog;

        protected override bool GetHitTest(Point point)
        {
            ListView component = (ListView) base.Component;
            if (component.View == System.Windows.Forms.View.Details)
            {
                Point point2 = this.Control.PointToClient(point);
                IntPtr handle = component.Handle;
                IntPtr ptr2 = System.Design.NativeMethods.ChildWindowFromPointEx(handle, point2.X, point2.Y, 1);
                if ((ptr2 != IntPtr.Zero) && (ptr2 != handle))
                {
                    IntPtr hWndTo = System.Design.NativeMethods.SendMessage(handle, 0x101f, IntPtr.Zero, IntPtr.Zero);
                    if (ptr2 == hWndTo)
                    {
                        System.Design.NativeMethods.POINT pt = new System.Design.NativeMethods.POINT {
                            x = point.X,
                            y = point.Y
                        };
                        System.Design.NativeMethods.MapWindowPoints(IntPtr.Zero, hWndTo, pt, 1);
                        this.hdrhit.pt_x = pt.x;
                        this.hdrhit.pt_y = pt.y;
                        System.Design.NativeMethods.SendMessage(hWndTo, 0x1206, IntPtr.Zero, this.hdrhit);
                        if (this.hdrhit.flags == 4)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public override void Initialize(IComponent component)
        {
            ListView view = (ListView) component;
            this.OwnerDraw = view.OwnerDraw;
            view.OwnerDraw = false;
            view.UseCompatibleStateImageBehavior = false;
            base.AutoResizeHandles = true;
            base.Initialize(component);
            if (view.View == System.Windows.Forms.View.Details)
            {
                base.HookChildHandles(this.Control.Handle);
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties["OwnerDraw"];
            if (oldPropertyDescriptor != null)
            {
                properties["OwnerDraw"] = TypeDescriptor.CreateProperty(typeof(ListViewDesigner), oldPropertyDescriptor, new Attribute[0]);
            }
            PropertyDescriptor descriptor2 = (PropertyDescriptor) properties["View"];
            if (descriptor2 != null)
            {
                properties["View"] = TypeDescriptor.CreateProperty(typeof(ListViewDesigner), descriptor2, new Attribute[0]);
            }
            base.PreFilterProperties(properties);
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x4e:
                case 0x204e:
                {
                    System.Design.NativeMethods.NMHDR nmhdr = (System.Design.NativeMethods.NMHDR) Marshal.PtrToStructure(m.LParam, typeof(System.Design.NativeMethods.NMHDR));
                    if (nmhdr.code == System.Design.NativeMethods.HDN_ENDTRACK)
                    {
                        try
                        {
                            ((IComponentChangeService) this.GetService(typeof(IComponentChangeService))).OnComponentChanged(base.Component, null, null, null);
                        }
                        catch (InvalidOperationException exception)
                        {
                            if (!this.inShowErrorDialog)
                            {
                                IUIService service = (IUIService) base.Component.Site.GetService(typeof(IUIService));
                                this.inShowErrorDialog = true;
                                try
                                {
                                    DataGridViewDesigner.ShowErrorDialog(service, exception, (ListView) base.Component);
                                }
                                finally
                                {
                                    this.inShowErrorDialog = false;
                                }
                            }
                            return;
                        }
                    }
                    break;
                }
            }
            base.WndProc(ref m);
        }

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                if (this._actionLists == null)
                {
                    this._actionLists = new DesignerActionListCollection();
                    this._actionLists.Add(new ListViewActionList(this));
                }
                return this._actionLists;
            }
        }

        public override ICollection AssociatedComponents
        {
            get
            {
                ListView control = this.Control as ListView;
                if (control != null)
                {
                    return control.Columns;
                }
                return base.AssociatedComponents;
            }
        }

        private bool OwnerDraw
        {
            get
            {
                return (bool) base.ShadowProperties["OwnerDraw"];
            }
            set
            {
                base.ShadowProperties["OwnerDraw"] = value;
            }
        }

        private System.Windows.Forms.View View
        {
            get
            {
                return ((ListView) base.Component).View;
            }
            set
            {
                ((ListView) base.Component).View = value;
                if (value == System.Windows.Forms.View.Details)
                {
                    base.HookChildHandles(this.Control.Handle);
                }
            }
        }
    }
}

