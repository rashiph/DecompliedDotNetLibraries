namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.Security.Permissions;
    using System.Windows.Forms;

    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public abstract class WindowsFormsComponentEditor : ComponentEditor
    {
        protected WindowsFormsComponentEditor()
        {
        }

        public override bool EditComponent(ITypeDescriptorContext context, object component)
        {
            return this.EditComponent(context, component, null);
        }

        public bool EditComponent(object component, IWin32Window owner)
        {
            return this.EditComponent(null, component, owner);
        }

        public virtual bool EditComponent(ITypeDescriptorContext context, object component, IWin32Window owner)
        {
            bool flag = false;
            System.Type[] componentEditorPages = this.GetComponentEditorPages();
            if ((componentEditorPages != null) && (componentEditorPages.Length != 0))
            {
                ComponentEditorForm form = new ComponentEditorForm(component, componentEditorPages);
                if (form.ShowForm(owner, this.GetInitialComponentEditorPageIndex()) == DialogResult.OK)
                {
                    flag = true;
                }
            }
            return flag;
        }

        protected virtual System.Type[] GetComponentEditorPages()
        {
            return null;
        }

        protected virtual int GetInitialComponentEditorPageIndex()
        {
            return 0;
        }
    }
}

