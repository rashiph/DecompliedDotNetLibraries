namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Reflection;
    using System.Windows.Forms;
    using System.Windows.Forms.Design.Behavior;

    internal class LabelDesigner : ControlDesigner
    {
        public LabelDesigner()
        {
            base.AutoResizeHandles = true;
        }

        private int LabelBaselineOffset(ContentAlignment alignment, BorderStyle borderStyle)
        {
            if (((alignment & DesignerUtils.anyMiddleAlignment) != ((ContentAlignment) 0)) || ((alignment & DesignerUtils.anyTopAlignment) != ((ContentAlignment) 0)))
            {
                if (borderStyle == BorderStyle.None)
                {
                    return 0;
                }
                if ((borderStyle != BorderStyle.FixedSingle) && (borderStyle != BorderStyle.Fixed3D))
                {
                    return 0;
                }
                return 1;
            }
            if (borderStyle == BorderStyle.None)
            {
                return -1;
            }
            if ((borderStyle != BorderStyle.FixedSingle) && (borderStyle != BorderStyle.Fixed3D))
            {
                return 0;
            }
            return 0;
        }

        public override System.Windows.Forms.Design.SelectionRules SelectionRules
        {
            get
            {
                System.Windows.Forms.Design.SelectionRules selectionRules = base.SelectionRules;
                object component = base.Component;
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(component)["AutoSize"];
                if ((descriptor != null) && ((bool) descriptor.GetValue(component)))
                {
                    selectionRules &= ~System.Windows.Forms.Design.SelectionRules.AllSizeable;
                }
                return selectionRules;
            }
        }

        public override IList SnapLines
        {
            get
            {
                ArrayList snapLines = base.SnapLines as ArrayList;
                ContentAlignment topLeft = ContentAlignment.TopLeft;
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(base.Component);
                PropertyDescriptor descriptor = properties["TextAlign"];
                if (descriptor != null)
                {
                    topLeft = (ContentAlignment) descriptor.GetValue(base.Component);
                }
                int textBaseline = DesignerUtils.GetTextBaseline(this.Control, topLeft);
                if (((descriptor = properties["AutoSize"]) != null) && !((bool) descriptor.GetValue(base.Component)))
                {
                    BorderStyle none = BorderStyle.None;
                    descriptor = properties["BorderStyle"];
                    if (descriptor != null)
                    {
                        none = (BorderStyle) descriptor.GetValue(base.Component);
                    }
                    textBaseline += this.LabelBaselineOffset(topLeft, none);
                }
                snapLines.Add(new SnapLine(SnapLineType.Baseline, textBaseline, SnapLinePriority.Medium));
                Label control = this.Control as Label;
                if ((control != null) && (control.BorderStyle == BorderStyle.None))
                {
                    System.Type type = System.Type.GetType("System.Windows.Forms.Label");
                    if (type == null)
                    {
                        return snapLines;
                    }
                    MethodInfo method = type.GetMethod("GetLeadingTextPaddingFromTextFormatFlags", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (method == null)
                    {
                        return snapLines;
                    }
                    int num2 = (int) method.Invoke(base.Component, null);
                    bool flag = control.RightToLeft == RightToLeft.Yes;
                    for (int i = 0; i < snapLines.Count; i++)
                    {
                        SnapLine line = snapLines[i] as SnapLine;
                        if ((line != null) && (line.SnapLineType == (flag ? SnapLineType.Right : SnapLineType.Left)))
                        {
                            line.AdjustOffset(flag ? -num2 : num2);
                            return snapLines;
                        }
                    }
                }
                return snapLines;
            }
        }
    }
}

