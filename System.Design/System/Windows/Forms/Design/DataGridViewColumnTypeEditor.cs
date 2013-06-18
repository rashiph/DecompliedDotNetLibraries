namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing.Design;

    internal class DataGridViewColumnTypeEditor : UITypeEditor
    {
        private DataGridViewColumnTypePicker columnTypePicker;

        private DataGridViewColumnTypeEditor()
        {
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider != null)
            {
                IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService) provider.GetService(typeof(IWindowsFormsEditorService));
                if ((edSvc == null) || (context.Instance == null))
                {
                    return value;
                }
                if (this.columnTypePicker == null)
                {
                    this.columnTypePicker = new DataGridViewColumnTypePicker();
                }
                DataGridViewColumnCollectionDialog.ListBoxItem instance = (DataGridViewColumnCollectionDialog.ListBoxItem) context.Instance;
                IDesignerHost service = (IDesignerHost) provider.GetService(typeof(IDesignerHost));
                ITypeDiscoveryService discoveryService = null;
                if (service != null)
                {
                    discoveryService = (ITypeDiscoveryService) service.GetService(typeof(ITypeDiscoveryService));
                }
                this.columnTypePicker.Start(edSvc, discoveryService, instance.DataGridViewColumn.GetType());
                edSvc.DropDownControl(this.columnTypePicker);
                if (this.columnTypePicker.SelectedType != null)
                {
                    value = this.columnTypePicker.SelectedType;
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

