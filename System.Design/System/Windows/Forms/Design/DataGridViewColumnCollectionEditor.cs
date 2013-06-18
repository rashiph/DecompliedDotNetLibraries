namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing.Design;
    using System.Windows.Forms;

    internal class DataGridViewColumnCollectionEditor : UITypeEditor
    {
        private DataGridViewColumnCollectionDialog dataGridViewColumnCollectionDialog;

        private DataGridViewColumnCollectionEditor()
        {
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider != null)
            {
                IWindowsFormsEditorService service = (IWindowsFormsEditorService) provider.GetService(typeof(IWindowsFormsEditorService));
                if ((service == null) || (context.Instance == null))
                {
                    return value;
                }
                IDesignerHost host = (IDesignerHost) provider.GetService(typeof(IDesignerHost));
                if (host == null)
                {
                    return value;
                }
                if (this.dataGridViewColumnCollectionDialog == null)
                {
                    this.dataGridViewColumnCollectionDialog = new DataGridViewColumnCollectionDialog(((DataGridView) context.Instance).Site);
                }
                this.dataGridViewColumnCollectionDialog.SetLiveDataGridView((DataGridView) context.Instance);
                using (DesignerTransaction transaction = host.CreateTransaction(System.Design.SR.GetString("DataGridViewColumnCollectionTransaction")))
                {
                    if (service.ShowDialog(this.dataGridViewColumnCollectionDialog) == DialogResult.OK)
                    {
                        transaction.Commit();
                        return value;
                    }
                    transaction.Cancel();
                }
            }
            return value;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }
}

