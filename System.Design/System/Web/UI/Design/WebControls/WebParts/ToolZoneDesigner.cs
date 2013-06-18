namespace System.Web.UI.Design.WebControls.WebParts
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Security.Permissions;
    using System.Web.UI.Design;
    using System.Web.UI.WebControls.WebParts;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class ToolZoneDesigner : WebZoneDesigner
    {
        public override void Initialize(IComponent component)
        {
            ControlDesigner.VerifyInitializeArgument(component, typeof(ToolZone));
            base.Initialize(component);
        }

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                DesignerActionListCollection lists = new DesignerActionListCollection();
                lists.AddRange(base.ActionLists);
                lists.Add(new ToolZoneDesignerActionList(this));
                return lists;
            }
        }

        protected bool ViewInBrowseMode
        {
            get
            {
                object obj2 = base.DesignerState["ViewInBrowseMode"];
                if (obj2 == null)
                {
                    return false;
                }
                return (bool) obj2;
            }
            private set
            {
                if (value != this.ViewInBrowseMode)
                {
                    base.DesignerState["ViewInBrowseMode"] = value;
                    this.UpdateDesignTimeHtml();
                }
            }
        }

        private class ToolZoneDesignerActionList : DesignerActionList
        {
            private ToolZoneDesigner _parent;

            public ToolZoneDesignerActionList(ToolZoneDesigner parent) : base(parent.Component)
            {
                this._parent = parent;
            }

            public override DesignerActionItemCollection GetSortedActionItems()
            {
                DesignerActionItemCollection items = new DesignerActionItemCollection();
                items.Add(new DesignerActionPropertyItem("ViewInBrowseMode", System.Design.SR.GetString("ToolZoneDesigner_ViewInBrowseMode"), string.Empty, System.Design.SR.GetString("ToolZoneDesigner_ViewInBrowseModeDesc")));
                return items;
            }

            public override bool AutoShow
            {
                get
                {
                    return true;
                }
                set
                {
                }
            }

            public bool ViewInBrowseMode
            {
                get
                {
                    return this._parent.ViewInBrowseMode;
                }
                set
                {
                    this._parent.ViewInBrowseMode = value;
                }
            }
        }
    }
}

