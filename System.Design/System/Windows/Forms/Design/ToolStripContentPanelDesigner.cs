namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Windows.Forms;

    internal class ToolStripContentPanelDesigner : PanelDesigner
    {
        private BaseContextMenuStrip contextMenu;

        public override bool CanBeParentedTo(IDesigner parentDesigner)
        {
            return false;
        }

        protected override void OnContextMenu(int x, int y)
        {
            ToolStripContentPanel component = base.Component as ToolStripContentPanel;
            if ((component != null) && (component.Parent is ToolStripContainer))
            {
                this.DesignerContextMenu.Show(x, y);
            }
            else
            {
                base.OnContextMenu(x, y);
            }
        }

        protected override void PreFilterEvents(IDictionary events)
        {
            base.PreFilterEvents(events);
            string[] strArray = new string[] { "BindingContextChanged", "ChangeUICues", "ClientSizeChanged", "EnabledChanged", "FontChanged", "ForeColorChanged", "GiveFeedback", "ImeModeChanged", "Move", "QueryAccessibilityHelp", "Validated", "Validating", "VisibleChanged" };
            for (int i = 0; i < strArray.Length; i++)
            {
                EventDescriptor oldEventDescriptor = (EventDescriptor) events[strArray[i]];
                if (oldEventDescriptor != null)
                {
                    events[strArray[i]] = TypeDescriptor.CreateEvent(oldEventDescriptor.ComponentType, oldEventDescriptor, new Attribute[] { BrowsableAttribute.No });
                }
            }
        }

        private ContextMenuStrip DesignerContextMenu
        {
            get
            {
                if (this.contextMenu == null)
                {
                    this.contextMenu = new BaseContextMenuStrip(base.Component.Site, base.Component as Component);
                    this.contextMenu.GroupOrdering.Clear();
                    this.contextMenu.GroupOrdering.AddRange(new string[] { "Code", "Verbs", "Custom", "Selection", "Edit", "Properties" });
                    this.contextMenu.Text = "CustomContextMenu";
                }
                return this.contextMenu;
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

