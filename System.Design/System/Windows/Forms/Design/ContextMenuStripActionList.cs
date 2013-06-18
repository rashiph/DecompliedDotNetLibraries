namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Windows.Forms;

    internal class ContextMenuStripActionList : DesignerActionList
    {
        private bool _autoShow;
        private ToolStripDropDown _toolStripDropDown;

        public ContextMenuStripActionList(ToolStripDropDownDesigner designer) : base(designer.Component)
        {
            this._toolStripDropDown = (ToolStripDropDown) designer.Component;
        }

        private void ChangeProperty(string propertyName, object value)
        {
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this._toolStripDropDown)[propertyName];
            if (descriptor != null)
            {
                descriptor.SetValue(this._toolStripDropDown, value);
            }
        }

        private object GetProperty(string propertyName)
        {
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this._toolStripDropDown)[propertyName];
            if (descriptor != null)
            {
                return descriptor.GetValue(this._toolStripDropDown);
            }
            return null;
        }

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            DesignerActionItemCollection items = new DesignerActionItemCollection();
            items.Add(new DesignerActionPropertyItem("RenderMode", System.Design.SR.GetString("ToolStripActionList_RenderMode"), System.Design.SR.GetString("ToolStripActionList_Layout"), System.Design.SR.GetString("ToolStripActionList_RenderModeDesc")));
            if (this._toolStripDropDown is ToolStripDropDownMenu)
            {
                items.Add(new DesignerActionPropertyItem("ShowImageMargin", System.Design.SR.GetString("ContextMenuStripActionList_ShowImageMargin"), System.Design.SR.GetString("ToolStripActionList_Layout"), System.Design.SR.GetString("ContextMenuStripActionList_ShowImageMarginDesc")));
                items.Add(new DesignerActionPropertyItem("ShowCheckMargin", System.Design.SR.GetString("ContextMenuStripActionList_ShowCheckMargin"), System.Design.SR.GetString("ToolStripActionList_Layout"), System.Design.SR.GetString("ContextMenuStripActionList_ShowCheckMarginDesc")));
            }
            return items;
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

        public bool ShowCheckMargin
        {
            get
            {
                return (bool) this.GetProperty("ShowCheckMargin");
            }
            set
            {
                if (value != this.ShowCheckMargin)
                {
                    this.ChangeProperty("ShowCheckMargin", value);
                }
            }
        }

        public bool ShowImageMargin
        {
            get
            {
                return (bool) this.GetProperty("ShowImageMargin");
            }
            set
            {
                if (value != this.ShowImageMargin)
                {
                    this.ChangeProperty("ShowImageMargin", value);
                }
            }
        }
    }
}

