namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;

    internal class DataMemberListEditor : UITypeEditor
    {
        private DesignBindingPicker designBindingPicker;

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (((provider != null) && (context != null)) && (context.Instance != null))
            {
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(context.Instance)["DataSource"];
                if (descriptor == null)
                {
                    return value;
                }
                object dataSource = descriptor.GetValue(context.Instance);
                if (this.designBindingPicker == null)
                {
                    this.designBindingPicker = new DesignBindingPicker();
                }
                DesignBinding initialSelectedItem = new DesignBinding(dataSource, (string) value);
                DesignBinding binding2 = this.designBindingPicker.Pick(context, provider, false, true, true, dataSource, string.Empty, initialSelectedItem);
                if ((dataSource != null) && (binding2 != null))
                {
                    value = binding2.DataMember;
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

