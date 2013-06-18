namespace System.Web.UI.Design
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Security.Permissions;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    internal abstract class BaseTypeViewSchema : IDataSourceViewSchema
    {
        private Type _type;
        private string _viewName;

        protected BaseTypeViewSchema(string viewName, Type type)
        {
            this._type = type;
            this._viewName = viewName;
        }

        public IDataSourceViewSchema[] GetChildren()
        {
            return null;
        }

        public IDataSourceFieldSchema[] GetFields()
        {
            List<IDataSourceFieldSchema> list = new List<IDataSourceFieldSchema>();
            Type rowType = this.GetRowType(this._type);
            if ((rowType != null) && !typeof(ICustomTypeDescriptor).IsAssignableFrom(rowType))
            {
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(rowType))
                {
                    list.Add(new TypeFieldSchema(descriptor));
                }
            }
            return list.ToArray();
        }

        protected abstract Type GetRowType(Type objectType);

        public string Name
        {
            get
            {
                return this._viewName;
            }
        }
    }
}

