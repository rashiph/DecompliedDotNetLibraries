namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Security.Permissions;
    using System.Web.UI.Design;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public abstract class BaseDataListComponentEditor : WindowsFormsComponentEditor
    {
        private int initialPage;

        public BaseDataListComponentEditor(int initialPage)
        {
            this.initialPage = initialPage;
        }

        public override bool EditComponent(ITypeDescriptorContext context, object obj, IWin32Window parent)
        {
            bool flag = false;
            bool inTemplateModeInternal = false;
            IComponent component = (IComponent) obj;
            ISite site = component.Site;
            if (site != null)
            {
                IDesignerHost service = (IDesignerHost) site.GetService(typeof(IDesignerHost));
                TemplatedControlDesigner designer = (TemplatedControlDesigner) service.GetDesigner(component);
                inTemplateModeInternal = designer.InTemplateModeInternal;
            }
            if (!inTemplateModeInternal)
            {
                System.Type[] componentEditorPages = this.GetComponentEditorPages();
                if ((componentEditorPages != null) && (componentEditorPages.Length != 0))
                {
                    ComponentEditorForm form = new ComponentEditorForm(obj, componentEditorPages);
                    if (!string.Equals(System.Design.SR.GetString("RTL"), "RTL_False", StringComparison.Ordinal))
                    {
                        form.RightToLeft = RightToLeft.Yes;
                        form.RightToLeftLayout = true;
                    }
                    if (form.ShowForm(parent, this.GetInitialComponentEditorPageIndex()) == DialogResult.OK)
                    {
                        flag = true;
                    }
                }
                return flag;
            }
            System.Windows.Forms.Design.RTLAwareMessageBox.Show(null, System.Design.SR.GetString("BDL_TemplateModePropBuilder"), System.Design.SR.GetString("BDL_PropertyBuilder"), MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1, 0);
            return flag;
        }

        protected override int GetInitialComponentEditorPageIndex()
        {
            return this.initialPage;
        }
    }
}

