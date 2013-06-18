namespace System.Windows.Forms
{
    using System;
    using System.Security.Permissions;
    using System.Windows.Forms.Layout;

    public class ToolStripDropDownItemAccessibleObject : ToolStripItem.ToolStripItemAccessibleObject
    {
        private ToolStripDropDownItem owner;

        public ToolStripDropDownItemAccessibleObject(ToolStripDropDownItem item) : base(item)
        {
            this.owner = item;
        }

        [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public override void DoDefaultAction()
        {
            ToolStripDropDownItem owner = base.Owner as ToolStripDropDownItem;
            if ((owner != null) && owner.HasDropDownItems)
            {
                owner.ShowDropDown();
            }
            else
            {
                base.DoDefaultAction();
            }
        }

        public override AccessibleObject GetChild(int index)
        {
            if ((this.owner != null) && this.owner.HasDropDownItems)
            {
                return this.owner.DropDown.AccessibilityObject.GetChild(index);
            }
            return null;
        }

        public override int GetChildCount()
        {
            if ((this.owner == null) || !this.owner.HasDropDownItems)
            {
                return -1;
            }
            if (this.owner.DropDown.LayoutRequired)
            {
                LayoutTransaction.DoLayout(this.owner.DropDown, this.owner.DropDown, PropertyNames.Items);
            }
            return this.owner.DropDown.AccessibilityObject.GetChildCount();
        }

        public override AccessibleRole Role
        {
            get
            {
                AccessibleRole accessibleRole = base.Owner.AccessibleRole;
                if (accessibleRole != AccessibleRole.Default)
                {
                    return accessibleRole;
                }
                return AccessibleRole.MenuItem;
            }
        }
    }
}

