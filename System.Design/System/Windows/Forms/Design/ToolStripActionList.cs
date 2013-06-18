namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Windows.Forms;

    internal class ToolStripActionList : DesignerActionList
    {
        private bool _autoShow;
        private ToolStrip _toolStrip;
        private ChangeToolStripParentVerb changeParentVerb;
        private ToolStripDesigner designer;
        private StandardMenuStripVerb standardItemsVerb;

        public ToolStripActionList(ToolStripDesigner designer) : base(designer.Component)
        {
            this._toolStrip = (ToolStrip) designer.Component;
            this.designer = designer;
            this.changeParentVerb = new ChangeToolStripParentVerb(System.Design.SR.GetString("ToolStripDesignerEmbedVerb"), designer);
            if (!(this._toolStrip is StatusStrip))
            {
                this.standardItemsVerb = new StandardMenuStripVerb(System.Design.SR.GetString("ToolStripDesignerStandardItemsVerb"), designer);
            }
        }

        private void ChangeProperty(string propertyName, object value)
        {
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this._toolStrip)[propertyName];
            if (descriptor != null)
            {
                descriptor.SetValue(this._toolStrip, value);
            }
        }

        private object GetProperty(string propertyName)
        {
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this._toolStrip)[propertyName];
            if (descriptor != null)
            {
                return descriptor.GetValue(this._toolStrip);
            }
            return null;
        }

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            DesignerActionItemCollection items = new DesignerActionItemCollection();
            if (!this.IsReadOnly)
            {
                items.Add(new DesignerActionMethodItem(this, "InvokeEmbedVerb", System.Design.SR.GetString("ToolStripDesignerEmbedVerb"), "", System.Design.SR.GetString("ToolStripDesignerEmbedVerbDesc"), true));
            }
            if (this.CanAddItems)
            {
                if (!(this._toolStrip is StatusStrip))
                {
                    items.Add(new DesignerActionMethodItem(this, "InvokeInsertStandardItemsVerb", System.Design.SR.GetString("ToolStripDesignerStandardItemsVerb"), "", System.Design.SR.GetString("ToolStripDesignerStandardItemsVerbDesc"), true));
                }
                items.Add(new DesignerActionPropertyItem("RenderMode", System.Design.SR.GetString("ToolStripActionList_RenderMode"), System.Design.SR.GetString("ToolStripActionList_Layout"), System.Design.SR.GetString("ToolStripActionList_RenderModeDesc")));
            }
            if (!(this._toolStrip.Parent is ToolStripPanel))
            {
                items.Add(new DesignerActionPropertyItem("Dock", System.Design.SR.GetString("ToolStripActionList_Dock"), System.Design.SR.GetString("ToolStripActionList_Layout"), System.Design.SR.GetString("ToolStripActionList_DockDesc")));
            }
            if (!(this._toolStrip is StatusStrip))
            {
                items.Add(new DesignerActionPropertyItem("GripStyle", System.Design.SR.GetString("ToolStripActionList_GripStyle"), System.Design.SR.GetString("ToolStripActionList_Layout"), System.Design.SR.GetString("ToolStripActionList_GripStyleDesc")));
            }
            return items;
        }

        private void InvokeEmbedVerb()
        {
            DesignerActionUIService service = (DesignerActionUIService) this._toolStrip.Site.GetService(typeof(DesignerActionUIService));
            if (service != null)
            {
                service.HideUI(this._toolStrip);
            }
            this.changeParentVerb.ChangeParent();
        }

        private void InvokeInsertStandardItemsVerb()
        {
            this.standardItemsVerb.InsertItems();
        }

        public override bool AutoShow
        {
            get
            {
                return this._autoShow;
            }
            set
            {
                if (this._autoShow != value)
                {
                    this._autoShow = value;
                }
            }
        }

        private bool CanAddItems
        {
            get
            {
                InheritanceAttribute attribute = (InheritanceAttribute) TypeDescriptor.GetAttributes(this._toolStrip)[typeof(InheritanceAttribute)];
                if ((attribute != null) && (attribute.InheritanceLevel != InheritanceLevel.NotInherited))
                {
                    return false;
                }
                return true;
            }
        }

        public DockStyle Dock
        {
            get
            {
                return (DockStyle) this.GetProperty("Dock");
            }
            set
            {
                if (value != this.Dock)
                {
                    this.ChangeProperty("Dock", value);
                }
            }
        }

        public ToolStripGripStyle GripStyle
        {
            get
            {
                return (ToolStripGripStyle) this.GetProperty("GripStyle");
            }
            set
            {
                if (value != this.GripStyle)
                {
                    this.ChangeProperty("GripStyle", value);
                }
            }
        }

        private bool IsReadOnly
        {
            get
            {
                InheritanceAttribute attribute = (InheritanceAttribute) TypeDescriptor.GetAttributes(this._toolStrip)[typeof(InheritanceAttribute)];
                if ((attribute != null) && (attribute.InheritanceLevel != InheritanceLevel.InheritedReadOnly))
                {
                    return false;
                }
                return true;
            }
        }

        public ToolStripRenderMode RenderMode
        {
            get
            {
                return (ToolStripRenderMode) this.GetProperty("RenderMode");
            }
            set
            {
                if (value != this.RenderMode)
                {
                    this.ChangeProperty("RenderMode", value);
                }
            }
        }
    }
}

