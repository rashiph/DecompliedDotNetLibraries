namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;

    internal class SplitterPanelDesigner : PanelDesigner
    {
        private IDesignerHost designerHost;
        private bool selected;
        private SplitContainerDesigner splitContainerDesigner;
        private SplitterPanel splitterPanel;

        public override bool CanBeParentedTo(IDesigner parentDesigner)
        {
            return (parentDesigner is SplitContainerDesigner);
        }

        protected override void Dispose(bool disposing)
        {
            IComponentChangeService service = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
            if (service != null)
            {
                service.ComponentChanged -= new ComponentChangedEventHandler(this.OnComponentChanged);
            }
            base.Dispose(disposing);
        }

        internal void DrawSelectedBorder()
        {
            Control control = this.Control;
            Rectangle clientRectangle = control.ClientRectangle;
            using (Graphics graphics = control.CreateGraphics())
            {
                Color color;
                if (control.BackColor.GetBrightness() < 0.5)
                {
                    color = ControlPaint.Light(control.BackColor);
                }
                else
                {
                    color = ControlPaint.Dark(control.BackColor);
                }
                using (Pen pen = new Pen(color))
                {
                    pen.DashStyle = DashStyle.Dash;
                    clientRectangle.Inflate(-4, -4);
                    graphics.DrawRectangle(pen, clientRectangle);
                }
            }
        }

        internal void DrawWaterMark(Graphics g)
        {
            Control control = this.Control;
            Rectangle clientRectangle = control.ClientRectangle;
            string name = control.Name;
            using (Font font = new Font("Arial", 8f))
            {
                int x = (clientRectangle.Width / 2) - (((int) g.MeasureString(name, font).Width) / 2);
                int y = clientRectangle.Height / 2;
                TextRenderer.DrawText(g, name, font, new Point(x, y), Color.Black, TextFormatFlags.Default);
            }
        }

        internal void EraseBorder()
        {
            Control control = this.Control;
            Rectangle clientRectangle = control.ClientRectangle;
            Graphics graphics = control.CreateGraphics();
            Pen pen = new Pen(control.BackColor) {
                DashStyle = DashStyle.Dash
            };
            clientRectangle.Inflate(-4, -4);
            graphics.DrawRectangle(pen, clientRectangle);
            pen.Dispose();
            graphics.Dispose();
            control.Invalidate();
        }

        public override void Initialize(IComponent component)
        {
            base.Initialize(component);
            this.splitterPanel = (SplitterPanel) component;
            this.designerHost = (IDesignerHost) component.Site.GetService(typeof(IDesignerHost));
            this.splitContainerDesigner = (SplitContainerDesigner) this.designerHost.GetDesigner(this.splitterPanel.Parent);
            IComponentChangeService service = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
            if (service != null)
            {
                service.ComponentChanged += new ComponentChangedEventHandler(this.OnComponentChanged);
            }
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(component)["Locked"];
            if ((descriptor != null) && (this.splitterPanel.Parent is SplitContainer))
            {
                descriptor.SetValue(component, true);
            }
        }

        private void OnComponentChanged(object sender, ComponentChangedEventArgs e)
        {
            if (this.splitterPanel.Parent != null)
            {
                if (this.splitterPanel.Controls.Count == 0)
                {
                    Graphics g = this.splitterPanel.CreateGraphics();
                    this.DrawWaterMark(g);
                    g.Dispose();
                }
                else
                {
                    this.splitterPanel.Invalidate();
                }
            }
        }

        protected override void OnDragDrop(DragEventArgs de)
        {
            if (this.InheritanceAttribute == System.ComponentModel.InheritanceAttribute.InheritedReadOnly)
            {
                de.Effect = DragDropEffects.None;
            }
            else
            {
                base.OnDragDrop(de);
            }
        }

        protected override void OnDragEnter(DragEventArgs de)
        {
            if (this.InheritanceAttribute == System.ComponentModel.InheritanceAttribute.InheritedReadOnly)
            {
                de.Effect = DragDropEffects.None;
            }
            else
            {
                base.OnDragEnter(de);
            }
        }

        protected override void OnDragLeave(EventArgs e)
        {
            if (this.InheritanceAttribute != System.ComponentModel.InheritanceAttribute.InheritedReadOnly)
            {
                base.OnDragLeave(e);
            }
        }

        protected override void OnDragOver(DragEventArgs de)
        {
            if (this.InheritanceAttribute == System.ComponentModel.InheritanceAttribute.InheritedReadOnly)
            {
                de.Effect = DragDropEffects.None;
            }
            else
            {
                base.OnDragOver(de);
            }
        }

        protected override void OnMouseHover()
        {
            if (this.splitContainerDesigner != null)
            {
                this.splitContainerDesigner.SplitterPanelHover();
            }
        }

        protected override void OnPaintAdornments(PaintEventArgs pe)
        {
            base.OnPaintAdornments(pe);
            if (this.splitterPanel.BorderStyle == BorderStyle.None)
            {
                this.DrawBorder(pe.Graphics);
            }
            if (this.Selected)
            {
                this.DrawSelectedBorder();
            }
            if (this.splitterPanel.Controls.Count == 0)
            {
                this.DrawWaterMark(pe.Graphics);
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            properties.Remove("Modifiers");
            properties.Remove("Locked");
            properties.Remove("GenerateMember");
            foreach (DictionaryEntry entry in properties)
            {
                PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) entry.Value;
                if (oldPropertyDescriptor.Name.Equals("Name") && oldPropertyDescriptor.DesignTimeOnly)
                {
                    properties[entry.Key] = TypeDescriptor.CreateProperty(oldPropertyDescriptor.ComponentType, oldPropertyDescriptor, new Attribute[] { BrowsableAttribute.No, DesignerSerializationVisibilityAttribute.Hidden });
                    break;
                }
            }
        }

        protected override System.ComponentModel.InheritanceAttribute InheritanceAttribute
        {
            get
            {
                if ((this.splitterPanel != null) && (this.splitterPanel.Parent != null))
                {
                    return (System.ComponentModel.InheritanceAttribute) TypeDescriptor.GetAttributes(this.splitterPanel.Parent)[typeof(System.ComponentModel.InheritanceAttribute)];
                }
                return base.InheritanceAttribute;
            }
        }

        internal bool Selected
        {
            get
            {
                return this.selected;
            }
            set
            {
                this.selected = value;
                if (this.selected)
                {
                    this.DrawSelectedBorder();
                }
                else
                {
                    this.EraseBorder();
                }
            }
        }

        public override System.Windows.Forms.Design.SelectionRules SelectionRules
        {
            get
            {
                System.Windows.Forms.Design.SelectionRules none = System.Windows.Forms.Design.SelectionRules.None;
                if (this.Control.Parent is SplitContainer)
                {
                    none = System.Windows.Forms.Design.SelectionRules.None | System.Windows.Forms.Design.SelectionRules.Locked;
                }
                return none;
            }
        }

        public override IList SnapLines
        {
            get
            {
                ArrayList snapLines = null;
                base.AddPaddingSnapLines(ref snapLines);
                return snapLines;
            }
        }
    }
}

