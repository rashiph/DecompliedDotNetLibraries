namespace System.Web.UI.Design
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Security.Permissions;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public sealed class TypeSchema : IDataSourceSchema
    {
        private IDataSourceViewSchema[] _schema;
        private Type _type;

        public TypeSchema(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            this._type = type;
            if (typeof(DataTable).IsAssignableFrom(this._type))
            {
                this._schema = GetDataTableSchema(this._type);
            }
            else if (typeof(DataSet).IsAssignableFrom(this._type))
            {
                this._schema = GetDataSetSchema(this._type);
            }
            else if (IsBoundGenericEnumerable(this._type))
            {
                this._schema = GetGenericEnumerableSchema(this._type);
            }
            else if (typeof(IEnumerable).IsAssignableFrom(this._type))
            {
                this._schema = GetEnumerableSchema(this._type);
            }
            else
            {
                this._schema = GetTypeSchema(this._type);
            }
        }

        private static IDataSourceViewSchema[] GetDataSetSchema(Type t)
        {
            try
            {
                DataSet set = Activator.CreateInstance(t) as DataSet;
                List<IDataSourceViewSchema> list = new List<IDataSourceViewSchema>();
                foreach (DataTable table in set.Tables)
                {
                    list.Add(new DataSetViewSchema(table));
                }
                return list.ToArray();
            }
            catch
            {
                return null;
            }
        }

        private static IDataSourceViewSchema[] GetDataTableSchema(Type t)
        {
            try
            {
                DataTable dataTable = Activator.CreateInstance(t) as DataTable;
                DataSetViewSchema schema = new DataSetViewSchema(dataTable);
                return new IDataSourceViewSchema[] { schema };
            }
            catch
            {
                return null;
            }
        }

        private static IDataSourceViewSchema[] GetEnumerableSchema(Type t)
        {
            TypeEnumerableViewSchema schema = new TypeEnumerableViewSchema(string.Empty, t);
            return new IDataSourceViewSchema[] { schema };
        }

        private static IDataSourceViewSchema[] GetGenericEnumerableSchema(Type t)
        {
            TypeGenericEnumerableViewSchema schema = new TypeGenericEnumerableViewSchema(string.Empty, t);
            return new IDataSourceViewSchema[] { schema };
        }

        private static IDataSourceViewSchema[] GetTypeSchema(Type t)
        {
            TypeViewSchema schema = new TypeViewSchema(string.Empty, t);
            return new IDataSourceViewSchema[] { schema };
        }

        public IDataSourceViewSchema[] GetViews()
        {
            return this._schema;
        }

        internal static bool IsBoundGenericEnumerable(Type t)
        {
            Type[] interfaces = null;
            if ((t.IsInterface && t.IsGenericType) && (t.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
            {
                interfaces = new Type[] { t };
            }
            else
            {
                interfaces = t.GetInterfaces();
            }
            foreach (Type type in interfaces)
            {
                if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
                {
                    return !type.GetGenericArguments()[0].IsGenericParameter;
                }
            }
            return false;
        }
    }
}

