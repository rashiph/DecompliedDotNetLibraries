namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;

    internal class LinkedDataMemberFieldEditor : UITypeEditor
    {
        private DesignBindingPicker designBindingPicker;

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (((provider != null) && (context != null)) && (context.Instance != null))
            {
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(context.Instance)["LinkedDataSource"];
                if (descriptor == null)
                {
                    return value;
                }
                object rootDataSource = descriptor.GetValue(context.Instance);
                if (rootDataSource == null)
                {
                    return value;
                }
                if (this.designBindingPicker == null)
                {
                    this.designBindingPicker = new DesignBindingPicker();
                }
                DesignBinding initialSelectedItem = new DesignBinding(null, (string) value);
                DesignBinding binding2 = this.designBindingPicker.Pick(context, provider, false, true, false, rootDataSource, string.Empty, initialSelectedItem);
                if (binding2 != null)
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
    }
}

