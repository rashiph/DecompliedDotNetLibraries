namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Forms.Design.Behavior;

    internal class TextBoxBaseDesigner : ControlDesigner
    {
        public TextBoxBaseDesigner()
        {
            base.AutoResizeHandles = true;
        }

        public override void InitializeNewComponent(IDictionary defaultValues)
        {
            base.InitializeNewComponent(defaultValues);
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(base.Component)["Text"];
            if (((descriptor != null) && (descriptor.PropertyType == typeof(string))) && (!descriptor.IsReadOnly && descriptor.IsBrowsable))
            {
                descriptor.SetValue(base.Component, "");
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            string[] strArray = new string[] { "Text" };
            Attribute[] attributes = new Attribute[0];
            for (int i = 0; i < strArray.Length; i++)
            {
                PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties[strArray[i]];
                if (oldPropertyDescriptor != null)
                {
                    properties[strArray[i]] = TypeDescriptor.CreateProperty(typeof(TextBoxBaseDesigner), oldPropertyDescriptor, attributes);
                }
            }
        }

        private void ResetText()
        {
            this.Control.Text = "";
        }

        private bool ShouldSerializeText()
        {
            return TypeDescriptor.GetProperties(typeof(TextBoxBase))["Text"].ShouldSerializeValue(base.Component);
        }

        public override System.Windows.Forms.Design.SelectionRules SelectionRules
        {
            get
            {
                System.Windows.Forms.Design.SelectionRules selectionRules = base.SelectionRules;
                object component = base.Component;
                selectionRules |= System.Windows.Forms.Design.SelectionRules.AllSizeable;
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(component)["Multiline"];
                if (descriptor != null)
                {
                    object obj3 = descriptor.GetValue(component);
                    if ((obj3 is bool) && !((bool) obj3))
                    {
                        PropertyDescriptor descriptor2 = TypeDescriptor.GetProperties(component)["AutoSize"];
                        if (descriptor2 != null)
                        {
                            object obj4 = descriptor2.GetValue(component);
                            if ((obj4 is bool) && ((bool) obj4))
                            {
                                selectionRules &= ~(System.Windows.Forms.Design.SelectionRules.BottomSizeable | System.Windows.Forms.Design.SelectionRules.TopSizeable);
                            }
                        }
                    }
                }
                return selectionRules;
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
                    textBaseline = textBaseline;
                }
                else if (style == BorderStyle.FixedSingle)
                {
                    textBaseline += 2;
                }
                else if (style == BorderStyle.Fixed3D)
                {
                    textBaseline += 3;
                }
                else
                {
                    textBaseline = textBaseline;
                }
                snapLines.Add(new SnapLine(SnapLineType.Baseline, textBaseline, SnapLinePriority.Medium));
                return snapLines;
            }
        }

        private string Text
        {
            get
            {
                return this.Control.Text;
            }
            set
            {
                this.Control.Text = value;
                ((TextBoxBase) this.Control).Select(0, 0);
            }
        }
    }
}

