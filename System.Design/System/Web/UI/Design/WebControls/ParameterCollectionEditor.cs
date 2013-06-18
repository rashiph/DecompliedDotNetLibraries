namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing.Design;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;

    public class ParameterCollectionEditor : UITypeEditor
    {
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            ParameterCollection parameters = value as ParameterCollection;
            if (parameters == null)
            {
                throw new ArgumentException(System.Design.SR.GetString("ParameterCollectionEditor_InvalidParameters"), "value");
            }
            System.Web.UI.Control instance = context.Instance as System.Web.UI.Control;
            ControlDesigner designer = null;
            if ((instance != null) && (instance.Site != null))
            {
                IDesignerHost service = (IDesignerHost) instance.Site.GetService(typeof(IDesignerHost));
                if (service != null)
                {
                    designer = service.GetDesigner(instance) as ControlDesigner;
                }
            }
            ParameterCollectionEditorForm form = new ParameterCollectionEditorForm(provider, parameters, designer);
            if ((form.ShowDialog() == DialogResult.OK) && (context != null))
            {
                context.OnComponentChanged();
            }
            return value;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }
}

