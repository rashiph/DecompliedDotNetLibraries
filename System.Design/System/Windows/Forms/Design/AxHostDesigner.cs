namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Diagnostics;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Forms.Design.Behavior;

    internal class AxHostDesigner : ControlDesigner
    {
        private static readonly HostVerbData AboutVerbData = new HostVerbData(System.Design.SR.GetString("AXAbout"), 2);
        private AxHost axHost;
        private static TraceSwitch AxHostDesignerSwitch = new TraceSwitch("AxHostDesigner", "ActiveX Designer Trace");
        private Size defaultSize = Size.Empty;
        private bool dragdropRevoked;
        private static readonly HostVerbData EditVerbData = new HostVerbData(System.Design.SR.GetString("AXEdit"), 3);
        private bool foundAbout;
        private bool foundEdit;
        private bool foundProperties;
        private EventHandler handler;
        private const int HOSTVERB_ABOUT = 2;
        private const int HOSTVERB_EDIT = 3;
        private const int HOSTVERB_PROPERTIES = 1;
        private const int OLEIVERB_UIACTIVATE = -4;
        private static readonly HostVerbData PropertiesVerbData = new HostVerbData(System.Design.SR.GetString("AXProperties"), 1);

        public AxHostDesigner()
        {
            this.handler = new EventHandler(this.OnVerb);
            base.AutoResizeHandles = true;
        }

        protected override ControlBodyGlyph GetControlGlyph(GlyphSelectionType selType)
        {
            Cursor sizeAll = Cursors.Default;
            if ((selType != GlyphSelectionType.NotSelected) && ((this.SelectionRules & SelectionRules.Moveable) != SelectionRules.None))
            {
                sizeAll = Cursors.SizeAll;
            }
            Point location = base.BehaviorService.ControlToAdornerWindow((Control) base.Component);
            return new ControlBodyGlyph(new Rectangle(location, ((Control) base.Component).Size), sizeAll, this.Control, this);
        }

        private static Size GetDefaultSize(IComponent component)
        {
            Size empty = Size.Empty;
            DefaultValueAttribute attribute = null;
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(component)["AutoSize"];
            if (((descriptor != null) && !descriptor.Attributes.Contains(DesignerSerializationVisibilityAttribute.Hidden)) && (!descriptor.Attributes.Contains(BrowsableAttribute.No) && ((bool) descriptor.GetValue(component))))
            {
                descriptor = TypeDescriptor.GetProperties(component)["PreferredSize"];
                if (descriptor != null)
                {
                    empty = (Size) descriptor.GetValue(component);
                    if (empty != Size.Empty)
                    {
                        return empty;
                    }
                }
            }
            descriptor = TypeDescriptor.GetProperties(component)["Size"];
            if (descriptor != null)
            {
                empty = (Size) descriptor.GetValue(component);
                if ((empty.Width > 0) && (empty.Height > 0))
                {
                    return empty;
                }
                attribute = (DefaultValueAttribute) descriptor.Attributes[typeof(DefaultValueAttribute)];
                if (attribute != null)
                {
                    return (Size) attribute.Value;
                }
            }
            return new Size(0x4b, 0x17);
        }

        protected override bool GetHitTest(Point p)
        {
            return this.axHost.EditMode;
        }

        public virtual void GetOleVerbs(DesignerVerbCollection rval)
        {
            System.Design.NativeMethods.IEnumOLEVERB e = null;
            System.Design.NativeMethods.IOleObject ocx = this.axHost.GetOcx() as System.Design.NativeMethods.IOleObject;
            if ((ocx == null) || System.Design.NativeMethods.Failed(ocx.EnumVerbs(out e)))
            {
                return;
            }
            if (e == null)
            {
                return;
            }
            int[] pceltFetched = new int[1];
            System.Design.NativeMethods.tagOLEVERB rgelt = new System.Design.NativeMethods.tagOLEVERB();
            this.foundEdit = false;
            this.foundAbout = false;
            this.foundProperties = false;
            while (true)
            {
                pceltFetched[0] = 0;
                rgelt.lpszVerbName = null;
                int hr = e.Next(1, rgelt, pceltFetched);
                if ((hr == 1) || System.Design.NativeMethods.Failed(hr))
                {
                    return;
                }
                if ((rgelt.grfAttribs & 2) != 0)
                {
                    this.foundEdit = this.foundEdit || (rgelt.lVerb == -4);
                    this.foundAbout = this.foundAbout || (rgelt.lVerb == 2);
                    this.foundProperties = this.foundProperties || (rgelt.lVerb == 1);
                    rval.Add(new HostVerb(new OleVerbData(rgelt), this.handler));
                }
            }
        }

        public override void Initialize(IComponent component)
        {
            base.Initialize(component);
            this.axHost = (AxHost) component;
        }

        public override void InitializeNewComponent(IDictionary defaultValues)
        {
            try
            {
                Control control = defaultValues["Parent"] as Control;
                if (control != null)
                {
                    control.ControlAdded += new ControlEventHandler(this.OnControlAdded);
                }
                base.InitializeNewComponent(defaultValues);
                if (control != null)
                {
                    control.ControlAdded -= new ControlEventHandler(this.OnControlAdded);
                }
                if (!((defaultValues != null) && defaultValues.Contains("Size")) && (this.defaultSize != Size.Empty))
                {
                    PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(this.axHost);
                    if (properties != null)
                    {
                        PropertyDescriptor descriptor = properties["Size"];
                        if (descriptor != null)
                        {
                            descriptor.SetValue(this.axHost, new Size(this.defaultSize.Width, this.defaultSize.Height));
                        }
                    }
                }
            }
            catch (NotSupportedException)
            {
                throw;
            }
            catch
            {
            }
        }

        private void OnControlAdded(object sender, ControlEventArgs e)
        {
            if (e.Control == this.axHost)
            {
                this.defaultSize = GetDefaultSize(this.axHost);
            }
        }

        protected override void OnCreateHandle()
        {
            base.OnCreateHandle();
        }

        public virtual void OnVerb(object sender, EventArgs evevent)
        {
            if ((sender != null) && (sender is HostVerb))
            {
                ((HostVerb) sender).Invoke(this.axHost);
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
            properties["SelectionStyle"] = TypeDescriptor.CreateProperty(typeof(AxHostDesigner), "SelectionStyle", typeof(int), new Attribute[] { BrowsableAttribute.No, DesignerSerializationVisibilityAttribute.Hidden, DesignOnlyAttribute.Yes });
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x84:
                    if (!this.dragdropRevoked)
                    {
                        int num = System.Design.NativeMethods.RevokeDragDrop(this.Control.Handle);
                        this.dragdropRevoked = num == 0;
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

        public override DesignerVerbCollection Verbs
        {
            get
            {
                DesignerVerbCollection rval = new DesignerVerbCollection();
                this.GetOleVerbs(rval);
                if (!this.foundAbout && this.axHost.HasAboutBox)
                {
                    rval.Add(new HostVerb(AboutVerbData, this.handler));
                }
                return rval;
            }
        }

        internal class AxHostDesignerBehavior : System.Windows.Forms.Design.Behavior.Behavior
        {
            private BehaviorService bs;
            private AxHostDesigner designer;

            internal AxHostDesignerBehavior(AxHostDesigner designer)
            {
                this.designer = designer;
            }

            private Point AdornerToControl(Point ptAdorner)
            {
                if (this.bs == null)
                {
                    this.bs = (BehaviorService) this.designer.GetService(typeof(BehaviorService));
                }
                if (this.bs != null)
                {
                    Point p = this.bs.AdornerWindowToScreen();
                    p.X += ptAdorner.X;
                    p.Y += ptAdorner.Y;
                    return this.designer.Control.PointToClient(p);
                }
                return ptAdorner;
            }

            internal bool IsTransparent(Point p)
            {
                return this.designer.GetHitTest(p);
            }

            public override void OnDragDrop(Glyph g, DragEventArgs e)
            {
                this.designer.OnDragDrop(e);
            }

            public override void OnDragEnter(Glyph g, DragEventArgs e)
            {
                this.designer.OnDragEnter(e);
            }

            public override void OnDragLeave(Glyph g, EventArgs e)
            {
                this.designer.OnDragLeave(e);
            }

            public override void OnDragOver(Glyph g, DragEventArgs e)
            {
                this.designer.OnDragOver(e);
            }

            public override void OnGiveFeedback(Glyph g, GiveFeedbackEventArgs e)
            {
                this.designer.OnGiveFeedback(e);
            }

            public override bool OnMouseDown(Glyph g, MouseButtons button, Point mouseLoc)
            {
                int num = 0;
                if (button == MouseButtons.Left)
                {
                    num = 0x201;
                }
                else if (button == MouseButtons.Right)
                {
                    num = 0x204;
                }
                if (num != 0)
                {
                    Point point = this.AdornerToControl(mouseLoc);
                    Message m = new Message {
                        HWnd = this.designer.Control.Handle,
                        Msg = num,
                        WParam = IntPtr.Zero,
                        LParam = (IntPtr) ((point.Y << 0x10) | point.X)
                    };
                    this.designer.WndProc(ref m);
                    return true;
                }
                return false;
            }

            public override bool OnMouseUp(Glyph g, MouseButtons button)
            {
                int num = 0;
                if (button == MouseButtons.Left)
                {
                    num = 0x202;
                }
                else if (button == MouseButtons.Right)
                {
                    num = 0x205;
                }
                if (num != 0)
                {
                    Point point = this.designer.Control.PointToClient(Control.MousePosition);
                    Message m = new Message {
                        HWnd = this.designer.Control.Handle,
                        Msg = num,
                        WParam = IntPtr.Zero,
                        LParam = (IntPtr) ((point.Y << 0x10) | point.X)
                    };
                    this.designer.WndProc(ref m);
                    return true;
                }
                return false;
            }
        }

        private class HostVerb : DesignerVerb
        {
            private AxHostDesigner.HostVerbData data;

            public HostVerb(AxHostDesigner.HostVerbData data, EventHandler handler) : base(data.ToString(), handler)
            {
                this.data = data;
            }

            public void Invoke(AxHost host)
            {
                this.data.Execute(host);
            }
        }

        private class HostVerbData
        {
            internal readonly int id;
            internal readonly string name;

            internal HostVerbData(string name, int id)
            {
                this.name = name;
                this.id = id;
            }

            internal virtual void Execute(AxHost ctl)
            {
                switch (this.id)
                {
                    case 1:
                        ctl.ShowPropertyPages();
                        return;

                    case 2:
                        ctl.ShowAboutBox();
                        return;

                    case 3:
                        ctl.InvokeEditMode();
                        return;
                }
            }

            public override string ToString()
            {
                return this.name;
            }
        }

        private class OleVerbData : AxHostDesigner.HostVerbData
        {
            private readonly bool dirties;

            internal OleVerbData(System.Design.NativeMethods.tagOLEVERB oleVerb) : base(System.Design.SR.GetString("AXVerbPrefix") + oleVerb.lpszVerbName, oleVerb.lVerb)
            {
                this.dirties = (oleVerb.grfAttribs & 1) == 0;
            }

            internal override void Execute(AxHost ctl)
            {
                if (this.dirties)
                {
                    ctl.MakeDirty();
                }
                ctl.DoVerb(base.id);
            }
        }
    }
}

