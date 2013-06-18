namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Web.UI.WebControls;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class SubMenuStyleCollectionEditor : CollectionEditor
    {
        public SubMenuStyleCollectionEditor(Type type) : base(type)
        {
        }

        protected override bool CanSelectMultipleInstances()
        {
            return false;
        }

        protected override CollectionEditor.CollectionForm CreateCollectionForm()
        {
            CollectionEditor.CollectionForm form = base.CreateCollectionForm();
            form.Text = System.Design.SR.GetString("CollectionEditorCaption", new object[] { "SubMenuStyle" });
            return form;
        }

        protected override object CreateInstance(Type itemType)
        {
            return Activator.CreateInstance(itemType, BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance, null, null, null);
        }

        protected override Type[] CreateNewItemTypes()
        {
            return new Type[] { typeof(SubMenuStyle) };
        }
    }
}

