namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.Drawing;
    using System.Windows.Forms.Design.Behavior;

    internal class DateTimePickerDesigner : ControlDesigner
    {
        public DateTimePickerDesigner()
        {
            base.AutoResizeHandles = true;
        }

        public override System.Windows.Forms.Design.SelectionRules SelectionRules
        {
            get
            {
                return (base.SelectionRules & ~(System.Windows.Forms.Design.SelectionRules.BottomSizeable | System.Windows.Forms.Design.SelectionRules.TopSizeable));
            }
        }

        public override IList SnapLines
        {
            get
            {
                ArrayList snapLines = base.SnapLines as ArrayList;
                int offset = DesignerUtils.GetTextBaseline(this.Control, ContentAlignment.MiddleLeft) + 2;
                snapLines.Add(new SnapLine(SnapLineType.Baseline, offset, SnapLinePriority.Medium));
                return snapLines;
            }
        }
    }
}

