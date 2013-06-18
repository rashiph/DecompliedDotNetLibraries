namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing.Design;
    using System.Web.UI.Design;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;

    public class DataControlFieldTypeEditor : UITypeEditor
    {
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            DataBoundControl dataBoundControl = context.Instance as DataBoundControl;
            if (dataBoundControl != null)
            {
                IDesignerHost service = (IDesignerHost) provider.GetService(typeof(IDesignerHost));
                DataBoundControlDesigner designer = (DataBoundControlDesigner) service.GetDesigner(dataBoundControl);
                IComponentChangeService changeService = (IComponentChangeService) provider.GetService(typeof(IComponentChangeService));
                ControlDesigner.InvokeTransactedChange(dataBoundControl, delegate (object callbackContext) {
                    DataControlFieldsEditor form = new DataControlFieldsEditor(designer);
                    DialogResult result = UIServiceHelper.ShowDialog(provider, form);
                    if ((result == DialogResult.OK) && (changeService != null))
                    {
                        changeService.OnComponentChanged(dataBoundControl, null, null, null);
                    }
                    return result == DialogResult.OK;
                }, null, System.Design.SR.GetString("GridView_EditFieldsTransaction"));
                return value;
            }
            return null;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }
}

