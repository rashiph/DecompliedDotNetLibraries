namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Forms.Design.Behavior;

    internal class UpDownBaseDesigner : ControlDesigner
    {
        public UpDownBaseDesigner()
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
                int textBaseline = DesignerUtils.GetTextBaseline(this.Control, ContentAlignment.TopLeft);
                BorderStyle style = BorderStyle.Fixed3D;
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(base.Component)["BorderStyle"];
                if (descriptor != null)
                {
                    style = (BorderStyle) descriptor.GetValue(base.Component);
                }
                if (style == BorderStyle.None)
                {
                    textBaseline--;
                }
                else
                {
                    textBaseline += 2;
                }
                snapLines.Add(new SnapLine(SnapLineType.Baseline, textBaseline, SnapLinePriority.Medium));
                return snapLines;
            }
        }
    }
}

