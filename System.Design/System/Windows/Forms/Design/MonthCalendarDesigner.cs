namespace System.Windows.Forms.Design
{
    using System;

    internal class MonthCalendarDesigner : ControlDesigner
    {
        public MonthCalendarDesigner()
        {
            base.AutoResizeHandles = true;
        }

        public override System.Windows.Forms.Design.SelectionRules SelectionRules
        {
            get
            {
                System.Windows.Forms.Design.SelectionRules selectionRules = base.SelectionRules;
                if ((this.Control.Parent == null) || ((this.Control.Parent != null) && !this.Control.Parent.IsMirrored))
                {
                    return (selectionRules & ~(System.Windows.Forms.Design.SelectionRules.LeftSizeable | System.Windows.Forms.Design.SelectionRules.TopSizeable));
                }
                return (selectionRules & ~(System.Windows.Forms.Design.SelectionRules.RightSizeable | System.Windows.Forms.Design.SelectionRules.TopSizeable));
            }
        }
    }
}

