namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Windows.Forms;

    internal class DataGridViewColumnDataPropertyNameEditor : UITypeEditor
    {
        private DesignBindingPicker designBindingPicker;

        private DataGridViewColumnDataPropertyNameEditor()
        {
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (((provider != null) && (context != null)) && (context.Instance != null))
            {
                DataGridView dataGridView;
                DataGridViewColumnCollectionDialog.ListBoxItem instance = context.Instance as DataGridViewColumnCollectionDialog.ListBoxItem;
                if (instance != null)
                {
                    dataGridView = instance.DataGridViewColumn.DataGridView;
                }
                else
                {
                    DataGridViewColumn column = context.Instance as DataGridViewColumn;
                    if (column != null)
                    {
                        dataGridView = column.DataGridView;
                    }
                    else
                    {
                        dataGridView = null;
                    }
                }
                if (dataGridView == null)
                {
                    return value;
                }
                object dataSource = dataGridView.DataSource;
                string dataMember = dataGridView.DataMember;
                string str2 = (string) value;
                string str3 = dataMember + "." + str2;
                if (dataSource == null)
                {
                    dataMember = string.Empty;
                    str3 = str2;
                }
                if (this.designBindingPicker == null)
                {
                    this.designBindingPicker = new DesignBindingPicker();
                }
                DesignBinding initialSelectedItem = new DesignBinding(dataSource, str3);
                DesignBinding binding2 = this.designBindingPicker.Pick(context, provider, false, true, false, dataSource, dataMember, initialSelectedItem);
                if ((dataSource != null) && (binding2 != null))
                {
                    value = binding2.DataField;
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

