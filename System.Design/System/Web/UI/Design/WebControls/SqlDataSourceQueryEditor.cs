namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing.Design;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;

    internal sealed class SqlDataSourceQueryEditor : UITypeEditor
    {
        private bool EditQueryChangeCallback(object context)
        {
            SqlDataSource first = (SqlDataSource) ((Pair) context).First;
            DataSourceOperation second = (DataSourceOperation) ((Pair) context).Second;
            IServiceProvider site = first.Site;
            IDesignerHost service = (IDesignerHost) site.GetService(typeof(IDesignerHost));
            SqlDataSourceDesigner sqlDataSourceDesigner = (SqlDataSourceDesigner) service.GetDesigner(first);
            ParameterCollection originalParameters = null;
            string command = string.Empty;
            SqlDataSourceCommandType text = SqlDataSourceCommandType.Text;
            switch (second)
            {
                case DataSourceOperation.Delete:
                    originalParameters = first.DeleteParameters;
                    command = first.DeleteCommand;
                    text = first.DeleteCommandType;
                    break;

                case DataSourceOperation.Insert:
                    originalParameters = first.InsertParameters;
                    command = first.InsertCommand;
                    text = first.InsertCommandType;
                    break;

                case DataSourceOperation.Select:
                    originalParameters = first.SelectParameters;
                    command = first.SelectCommand;
                    text = first.SelectCommandType;
                    break;

                case DataSourceOperation.Update:
                    originalParameters = first.UpdateParameters;
                    command = first.UpdateCommand;
                    text = first.UpdateCommandType;
                    break;
            }
            SqlDataSourceQueryEditorForm form = new SqlDataSourceQueryEditorForm(site, sqlDataSourceDesigner, first.ProviderName, sqlDataSourceDesigner.ConnectionString, second, text, command, originalParameters);
            if (UIServiceHelper.ShowDialog(site, form) != DialogResult.OK)
            {
                return false;
            }
            PropertyDescriptor descriptor = null;
            switch (second)
            {
                case DataSourceOperation.Delete:
                    descriptor = TypeDescriptor.GetProperties(first)["DeleteCommand"];
                    break;

                case DataSourceOperation.Insert:
                    descriptor = TypeDescriptor.GetProperties(first)["InsertCommand"];
                    break;

                case DataSourceOperation.Select:
                    descriptor = TypeDescriptor.GetProperties(first)["SelectCommand"];
                    break;

                case DataSourceOperation.Update:
                    descriptor = TypeDescriptor.GetProperties(first)["UpdateCommand"];
                    break;
            }
            if (descriptor != null)
            {
                descriptor.ResetValue(first);
                descriptor.SetValue(first, form.Command);
            }
            return true;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            ControlDesigner.InvokeTransactedChange((IComponent) context.Instance, new TransactedChangeCallback(this.EditQueryChangeCallback), new Pair(context.Instance, value), System.Design.SR.GetString("SqlDataSourceDesigner_EditQueryTransactionDescription"));
            return value;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }
}

