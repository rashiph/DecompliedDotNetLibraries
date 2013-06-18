namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Forms.Design.Behavior;

    internal class ButtonBaseDesigner : ControlDesigner
    {
        public ButtonBaseDesigner()
        {
            base.AutoResizeHandles = true;
        }

        private int CheckboxBaselineOffset(ContentAlignment alignment, FlatStyle flatStyle)
        {
            if ((alignment & DesignerUtils.anyMiddleAlignment) != ((ContentAlignment) 0))
            {
                if ((flatStyle != FlatStyle.Standard) && (flatStyle != FlatStyle.System))
                {
                    return 0;
                }
                return -1;
            }
            if ((alignment & DesignerUtils.anyTopAlignment) != ((ContentAlignment) 0))
            {
                if (flatStyle == FlatStyle.Standard)
                {
                    return 1;
                }
                if (flatStyle == FlatStyle.System)
                {
                    return 0;
                }
                if ((flatStyle != FlatStyle.Flat) && (flatStyle != FlatStyle.Popup))
                {
                    return 0;
                }
                return 2;
            }
            if (flatStyle == FlatStyle.Standard)
            {
                return -3;
            }
            if (flatStyle == FlatStyle.System)
            {
                return 0;
            }
            if ((flatStyle != FlatStyle.Flat) && (flatStyle != FlatStyle.Popup))
            {
                return 0;
            }
            return -2;
        }

        private int DefaultBaselineOffset(ContentAlignment alignment, FlatStyle flatStyle)
        {
            if ((alignment & DesignerUtils.anyMiddleAlignment) != ((ContentAlignment) 0))
            {
                return 0;
            }
            if ((flatStyle == FlatStyle.Standard) || (flatStyle == FlatStyle.Popup))
            {
                if ((alignment & DesignerUtils.anyTopAlignment) == ((ContentAlignment) 0))
                {
                    return -4;
                }
                return 4;
            }
            if (flatStyle == FlatStyle.System)
            {
                if ((alignment & DesignerUtils.anyTopAlignment) == ((ContentAlignment) 0))
                {
                    return -3;
                }
                return 3;
            }
            if (flatStyle != FlatStyle.Flat)
            {
                return 0;
            }
            if ((alignment & DesignerUtils.anyTopAlignment) == ((ContentAlignment) 0))
            {
                return -5;
            }
            return 5;
        }

        public override void InitializeNewComponent(IDictionary defaultValues)
        {
            base.InitializeNewComponent(defaultValues);
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(base.Component)["UseVisualStyleBackColor"];
            if ((((descriptor != null) && (descriptor.PropertyType == typeof(bool))) && (!descriptor.IsReadOnly && descriptor.IsBrowsable)) && !descriptor.ShouldSerializeValue(base.Component))
            {
                descriptor.SetValue(base.Component, true);
            }
        }

        private int RadiobuttonBaselineOffset(ContentAlignment alignment, FlatStyle flatStyle)
        {
            if ((alignment & DesignerUtils.anyMiddleAlignment) != ((ContentAlignment) 0))
            {
                if (flatStyle == FlatStyle.System)
                {
                    return -1;
                }
                return 0;
            }
            if (((flatStyle == FlatStyle.Standard) || (flatStyle == FlatStyle.Flat)) || (flatStyle == FlatStyle.Popup))
            {
                if ((alignment & DesignerUtils.anyTopAlignment) == ((ContentAlignment) 0))
                {
                    return -2;
                }
                return 2;
            }
            if (flatStyle == FlatStyle.System)
            {
                return 0;
            }
            return 0;
        }

        public override IList SnapLines
        {
            get
            {
                ArrayList snapLines = base.SnapLines as ArrayList;
                FlatStyle standard = FlatStyle.Standard;
                ContentAlignment middleCenter = ContentAlignment.MiddleCenter;
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(base.Component);
                PropertyDescriptor descriptor = properties["TextAlign"];
                if (descriptor != null)
                {
                    middleCenter = (ContentAlignment) descriptor.GetValue(base.Component);
                }
                descriptor = properties["FlatStyle"];
                if (descriptor != null)
                {
                    standard = (FlatStyle) descriptor.GetValue(base.Component);
                }
                int textBaseline = DesignerUtils.GetTextBaseline(this.Control, middleCenter);
                if ((this.Control is CheckBox) || (this.Control is RadioButton))
                {
                    Appearance normal = Appearance.Normal;
                    descriptor = properties["Appearance"];
                    if (descriptor != null)
                    {
                        normal = (Appearance) descriptor.GetValue(base.Component);
                    }
                    if (normal == Appearance.Normal)
                    {
                        if (this.Control is CheckBox)
                        {
                            textBaseline += this.CheckboxBaselineOffset(middleCenter, standard);
                        }
                        else
                        {
                            textBaseline += this.RadiobuttonBaselineOffset(middleCenter, standard);
                        }
                    }
                    else
                    {
                        textBaseline += this.DefaultBaselineOffset(middleCenter, standard);
                    }
                }
                else
                {
                    textBaseline += this.DefaultBaselineOffset(middleCenter, standard);
                }
                snapLines.Add(new SnapLine(SnapLineType.Baseline, textBaseline, SnapLinePriority.Medium));
                return snapLines;
            }
        }
    }
}

