namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Security.Permissions;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class EmbeddedMailObjectCollectionEditor : CollectionEditor
    {
        public EmbeddedMailObjectCollectionEditor(Type type) : base(type)
        {
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            object obj2;
            try
            {
                context.OnComponentChanging();
                obj2 = base.EditValue(context, provider, value);
            }
            finally
            {
                context.OnComponentChanged();
            }
            return obj2;
        }
    }
}

