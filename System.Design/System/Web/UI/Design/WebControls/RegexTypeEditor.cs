namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Security.Permissions;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class RegexTypeEditor : UITypeEditor
    {
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if ((provider != null) && (((IWindowsFormsEditorService) provider.GetService(typeof(IWindowsFormsEditorService))) != null))
            {
                ISite site = null;
                if (context.Instance is IComponent)
                {
                    site = ((IComponent) context.Instance).Site;
                }
                else if (context.Instance is object[])
                {
                    object[] instance = (object[]) context.Instance;
                    if (instance[0] is IComponent)
                    {
                        site = ((IComponent) instance[0]).Site;
                    }
                }
                RegexEditorDialog dialog = new RegexEditorDialog(site) {
                    RegularExpression = value.ToString()
                };
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    value = dialog.RegularExpression;
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

