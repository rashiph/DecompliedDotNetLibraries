namespace System.Web.UI.Design
{
    using System;
    using System.Security.Permissions;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    internal sealed class TypeViewSchema : BaseTypeViewSchema
    {
        public TypeViewSchema(string viewName, Type type) : base(viewName, type)
        {
        }

        protected override Type GetRowType(Type objectType)
        {
            return objectType;
        }
    }
}

