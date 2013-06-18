namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;

    internal class DataSourceListEditor : UITypeEditor
    {
        private DesignBindingPicker designBindingPicker;

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if ((provider != null) && (context.Instance != null))
            {
                if (this.designBindingPicker == null)
                {
                    this.designBindingPicker = new DesignBindingPicker();
                }
                DesignBinding initialSelectedItem = new DesignBinding(value, "");
                DesignBinding binding2 = this.designBindingPicker.Pick(context, provider, true, false, false, null, string.Empty, initialSelectedItem);
                if (binding2 != null)
                {
                    value = binding2.DataSource;
                }
            }
            return value;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override bool IsDropDownResizable
        {
            get
            {
                return true;
            }
        }
    }
}

