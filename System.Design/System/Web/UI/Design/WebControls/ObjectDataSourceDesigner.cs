namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Data;
    using System.Design;
    using System.Globalization;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class ObjectDataSourceDesigner : DataSourceDesigner
    {
        private bool _forceSchemaRetrieval;
        private bool _inWizard;
        private System.Type _selectMethodReturnType;
        private const string DesignerStateDataSourceSchemaKey = "DataSourceSchema";
        private const string DesignerStateDataSourceSchemaSelectMethodKey = "DataSourceSchemaSelectMethod";
        private const string DesignerStateDataSourceSchemaSelectMethodReturnTypeNameKey = "DataSourceSchemaSelectMethodReturnTypeName";
        private const string DesignerStateDataSourceSchemaTypeNameKey = "DataSourceSchemaTypeName";
        private const string DesignerStateShowOnlyDataComponentsStateKey = "ShowOnlyDataComponentsState";
        internal const BindingFlags MethodFilter = (BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);

        public override void Configure()
        {
            this._inWizard = true;
            ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.ConfigureDataSourceChangeCallback), null, System.Design.SR.GetString("DataSource_ConfigureTransactionDescription"));
            this._inWizard = false;
        }

        private bool ConfigureDataSourceChangeCallback(object context)
        {
            bool flag;
            try
            {
                this.SuppressDataSourceEvents();
                IServiceProvider site = base.Component.Site;
                ObjectDataSourceWizardForm form = new ObjectDataSourceWizardForm(site, this);
                if (UIServiceHelper.ShowDialog(site, form) == DialogResult.OK)
                {
                    this.OnDataSourceChanged(EventArgs.Empty);
                    return true;
                }
                flag = false;
            }
            finally
            {
                this.ResumeDataSourceEvents();
            }
            return flag;
        }

        private static DataTable[] ConvertSchemaToDataTables(TypeSchema schema)
        {
            if (schema == null)
            {
                return null;
            }
            IDataSourceViewSchema[] views = schema.GetViews();
            if (views == null)
            {
                return null;
            }
            DataTable[] tableArray = new DataTable[views.Length];
            for (int i = 0; i < views.Length; i++)
            {
                IDataSourceViewSchema schema2 = views[i];
                tableArray[i] = new DataTable(schema2.Name);
                IDataSourceFieldSchema[] fields = schema2.GetFields();
                if (fields != null)
                {
                    List<DataColumn> list = new List<DataColumn>();
                    for (int j = 0; j < fields.Length; j++)
                    {
                        IDataSourceFieldSchema schema3 = fields[j];
                        DataColumn column = new DataColumn {
                            AllowDBNull = schema3.Nullable,
                            AutoIncrement = schema3.Identity,
                            ColumnName = schema3.Name,
                            DataType = schema3.DataType
                        };
                        if (column.DataType == typeof(string))
                        {
                            column.MaxLength = schema3.Length;
                        }
                        column.ReadOnly = schema3.IsReadOnly;
                        column.Unique = schema3.IsUnique;
                        tableArray[i].Columns.Add(column);
                        if (schema3.PrimaryKey)
                        {
                            list.Add(column);
                        }
                    }
                    if (list.Count > 0)
                    {
                        tableArray[i].PrimaryKey = list.ToArray();
                    }
                }
            }
            return tableArray;
        }

        private static Parameter CreateMergedParameter(ParameterInfo methodParameter, Parameter[] parameters)
        {
            foreach (Parameter parameter in parameters)
            {
                if (ParametersMatch(methodParameter, parameter))
                {
                    return parameter;
                }
            }
            Parameter parameter2 = new Parameter(methodParameter.Name);
            if (methodParameter.IsOut)
            {
                parameter2.Direction = ParameterDirection.Output;
            }
            else if (methodParameter.ParameterType.IsByRef)
            {
                parameter2.Direction = ParameterDirection.InputOutput;
            }
            else
            {
                parameter2.Direction = ParameterDirection.Input;
            }
            SetParameterType(parameter2, methodParameter.ParameterType);
            return parameter2;
        }

        private static DbType GetDbTypeForType(System.Type type)
        {
            type = RemoveNullableFromType(type);
            if (typeof(DateTimeOffset).IsAssignableFrom(type))
            {
                return DbType.DateTimeOffset;
            }
            if (typeof(TimeSpan).IsAssignableFrom(type))
            {
                return DbType.Time;
            }
            if (typeof(Guid).IsAssignableFrom(type))
            {
                return DbType.Guid;
            }
            return DbType.Object;
        }

        internal static System.Type GetType(IServiceProvider serviceProvider, string typeName, bool silent)
        {
            ITypeResolutionService service = null;
            if (serviceProvider != null)
            {
                service = (ITypeResolutionService) serviceProvider.GetService(typeof(ITypeResolutionService));
            }
            if (service == null)
            {
                return null;
            }
            try
            {
                return service.GetType(typeName, true, true);
            }
            catch (Exception exception)
            {
                if (!silent)
                {
                    UIServiceHelper.ShowError(serviceProvider, exception, System.Design.SR.GetString("ObjectDataSourceDesigner_CannotGetType", new object[] { typeName }));
                }
                return null;
            }
        }

        private static TypeCode GetTypeCodeForType(System.Type type)
        {
            type = RemoveNullableFromType(type);
            if (typeof(bool).IsAssignableFrom(type))
            {
                return TypeCode.Boolean;
            }
            if (typeof(byte).IsAssignableFrom(type))
            {
                return TypeCode.Byte;
            }
            if (typeof(char).IsAssignableFrom(type))
            {
                return TypeCode.Char;
            }
            if (typeof(DateTime).IsAssignableFrom(type))
            {
                return TypeCode.DateTime;
            }
            if (typeof(DBNull).IsAssignableFrom(type))
            {
                return TypeCode.DBNull;
            }
            if (typeof(decimal).IsAssignableFrom(type))
            {
                return TypeCode.Decimal;
            }
            if (typeof(double).IsAssignableFrom(type))
            {
                return TypeCode.Double;
            }
            if (typeof(short).IsAssignableFrom(type))
            {
                return TypeCode.Int16;
            }
            if (typeof(int).IsAssignableFrom(type))
            {
                return TypeCode.Int32;
            }
            if (typeof(long).IsAssignableFrom(type))
            {
                return TypeCode.Int64;
            }
            if (typeof(sbyte).IsAssignableFrom(type))
            {
                return TypeCode.SByte;
            }
            if (typeof(float).IsAssignableFrom(type))
            {
                return TypeCode.Single;
            }
            if (typeof(string).IsAssignableFrom(type))
            {
                return TypeCode.String;
            }
            if (typeof(ushort).IsAssignableFrom(type))
            {
                return TypeCode.UInt16;
            }
            if (typeof(uint).IsAssignableFrom(type))
            {
                return TypeCode.UInt32;
            }
            if (typeof(ulong).IsAssignableFrom(type))
            {
                return TypeCode.UInt64;
            }
            return TypeCode.Object;
        }

        public override DesignerDataSourceView GetView(string viewName)
        {
            string[] viewNames = this.GetViewNames();
            if ((viewNames == null) || (viewNames.Length <= 0))
            {
                return new ObjectDesignerDataSourceView(this, string.Empty);
            }
            if (string.IsNullOrEmpty(viewName))
            {
                viewName = viewNames[0];
            }
            foreach (string str in viewNames)
            {
                if (string.Equals(viewName, str, StringComparison.OrdinalIgnoreCase))
                {
                    return new ObjectDesignerDataSourceView(this, viewName);
                }
            }
            return null;
        }

        public override string[] GetViewNames()
        {
            List<string> list = new List<string>();
            DataTable[] tableArray = this.LoadSchema();
            if ((tableArray != null) && (tableArray.Length > 0))
            {
                foreach (DataTable table in tableArray)
                {
                    list.Add(table.TableName);
                }
            }
            return list.ToArray();
        }

        internal static bool IsMatchingMethod(MethodInfo method, string methodName, ParameterCollection parameters, System.Type dataObjectType)
        {
            if (!string.Equals(methodName, method.Name, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            ParameterInfo[] infoArray = method.GetParameters();
            if ((dataObjectType == null) || (((infoArray.Length != 1) || (infoArray[0].ParameterType != dataObjectType)) && (((infoArray.Length != 2) || (infoArray[0].ParameterType != dataObjectType)) || (infoArray[1].ParameterType != dataObjectType))))
            {
                if (infoArray.Length != parameters.Count)
                {
                    return false;
                }
                Hashtable hashtable = new Hashtable(StringComparer.Create(CultureInfo.InvariantCulture, true));
                foreach (Parameter parameter in parameters)
                {
                    if (!hashtable.Contains(parameter.Name))
                    {
                        hashtable.Add(parameter.Name, null);
                    }
                }
                foreach (ParameterInfo info in infoArray)
                {
                    if (!hashtable.Contains(info.Name))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        internal DataTable[] LoadSchema()
        {
            if (!this._forceSchemaRetrieval)
            {
                string a = base.DesignerState["DataSourceSchemaTypeName"] as string;
                string str2 = base.DesignerState["DataSourceSchemaSelectMethod"] as string;
                if (!string.Equals(a, this.TypeName, StringComparison.OrdinalIgnoreCase) || !string.Equals(str2, this.SelectMethod, StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }
            }
            DataTable[] tableArray = null;
            Pair pair = base.DesignerState["DataSourceSchema"] as Pair;
            if (pair != null)
            {
                string[] first = pair.First as string[];
                DataTable[] second = pair.Second as DataTable[];
                if ((first == null) || (second == null))
                {
                    return tableArray;
                }
                int length = first.Length;
                tableArray = new DataTable[length];
                for (int i = 0; i < length; i++)
                {
                    tableArray[i] = second[i].Clone();
                    tableArray[i].TableName = first[i];
                }
            }
            return tableArray;
        }

        internal static Parameter[] MergeParameters(Parameter[] parameters, MethodInfo methodInfo)
        {
            ParameterInfo[] infoArray = methodInfo.GetParameters();
            Parameter[] parameterArray = new Parameter[infoArray.Length];
            for (int i = 0; i < infoArray.Length; i++)
            {
                ParameterInfo methodParameter = infoArray[i];
                parameterArray[i] = CreateMergedParameter(methodParameter, parameters);
            }
            return parameterArray;
        }

        internal static void MergeParameters(ParameterCollection parameters, MethodInfo methodInfo, System.Type dataObjectType)
        {
            Parameter[] parameterArray = new Parameter[parameters.Count];
            parameters.CopyTo(parameterArray, 0);
            parameters.Clear();
            if ((methodInfo != null) && (dataObjectType == null))
            {
                foreach (ParameterInfo info in methodInfo.GetParameters())
                {
                    Parameter parameter = CreateMergedParameter(info, parameterArray);
                    if (parameters[parameter.Name] == null)
                    {
                        parameters.Add(parameter);
                    }
                }
            }
        }

        private static bool ParametersMatch(ParameterInfo methodParameter, Parameter parameter)
        {
            if (!string.Equals(methodParameter.Name, parameter.Name, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            switch (parameter.Direction)
            {
                case ParameterDirection.Input:
                    if (!methodParameter.IsOut && !methodParameter.ParameterType.IsByRef)
                    {
                        break;
                    }
                    return false;

                case ParameterDirection.Output:
                    if (methodParameter.IsOut)
                    {
                        break;
                    }
                    return false;

                case ParameterDirection.InputOutput:
                    if (methodParameter.ParameterType.IsByRef)
                    {
                        break;
                    }
                    return false;

                case ParameterDirection.ReturnValue:
                    return false;
            }
            DbType dbTypeForType = GetDbTypeForType(methodParameter.ParameterType);
            if (dbTypeForType != DbType.Object)
            {
                return (dbTypeForType == parameter.DbType);
            }
            TypeCode typeCodeForType = GetTypeCodeForType(methodParameter.ParameterType);
            return ((((typeCodeForType == TypeCode.Object) || (typeCodeForType == TypeCode.Empty)) && ((parameter.Type == TypeCode.Object) || (parameter.Type == TypeCode.Empty))) || (typeCodeForType == parameter.Type));
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties["TypeName"];
            properties["TypeName"] = TypeDescriptor.CreateProperty(base.GetType(), oldPropertyDescriptor, new Attribute[0]);
            oldPropertyDescriptor = (PropertyDescriptor) properties["SelectMethod"];
            properties["SelectMethod"] = TypeDescriptor.CreateProperty(base.GetType(), oldPropertyDescriptor, new Attribute[0]);
        }

        public override void RefreshSchema(bool preferSilent)
        {
            try
            {
                this.SuppressDataSourceEvents();
                Cursor current = Cursor.Current;
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    System.Type type = GetType(base.Component.Site, this.TypeName, preferSilent);
                    if (type != null)
                    {
                        MethodInfo[] methods = type.GetMethods(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                        MethodInfo info = null;
                        MethodInfo info2 = null;
                        bool flag = false;
                        System.Type dataObjectType = null;
                        if (!string.IsNullOrEmpty(this.ObjectDataSource.DataObjectTypeName))
                        {
                            dataObjectType = GetType(base.Component.Site, this.ObjectDataSource.DataObjectTypeName, preferSilent);
                        }
                        foreach (MethodInfo info3 in methods)
                        {
                            if (string.Equals(info3.Name, this.SelectMethod, StringComparison.OrdinalIgnoreCase))
                            {
                                if ((info2 != null) && (info2.ReturnType != info3.ReturnType))
                                {
                                    flag = true;
                                }
                                else
                                {
                                    info2 = info3;
                                }
                                if (IsMatchingMethod(info3, this.SelectMethod, this.ObjectDataSource.SelectParameters, dataObjectType))
                                {
                                    info = info3;
                                    break;
                                }
                            }
                        }
                        if (((info == null) && (info2 != null)) && !flag)
                        {
                            info = info2;
                        }
                        if (info != null)
                        {
                            this.RefreshSchema(info.ReflectedType, info.Name, info.ReturnType, preferSilent);
                        }
                    }
                }
                finally
                {
                    Cursor.Current = current;
                }
            }
            finally
            {
                this.ResumeDataSourceEvents();
            }
        }

        internal void RefreshSchema(System.Type objectType, string methodName, System.Type schemaType, bool preferSilent)
        {
            if (((objectType != null) && !string.IsNullOrEmpty(methodName)) && (schemaType != null))
            {
                try
                {
                    TypeSchema schema = new TypeSchema(schemaType);
                    this._forceSchemaRetrieval = true;
                    DataTable[] tables = this.LoadSchema();
                    this._forceSchemaRetrieval = false;
                    IDataSourceSchema schema2 = (tables == null) ? null : new DataTableArraySchema(tables);
                    this.SaveSchema(objectType, methodName, ConvertSchemaToDataTables(schema), schemaType);
                    DataTable[] tableArray2 = this.LoadSchema();
                    IDataSourceSchema schema3 = (tableArray2 == null) ? null : new DataTableArraySchema(tableArray2);
                    if (!DataSourceDesigner.SchemasEquivalent(schema2, schema3))
                    {
                        this.OnSchemaRefreshed(EventArgs.Empty);
                    }
                }
                catch (Exception exception)
                {
                    if (!preferSilent)
                    {
                        UIServiceHelper.ShowError(base.Component.Site, exception, System.Design.SR.GetString("ObjectDataSourceDesigner_CannotGetSchema", new object[] { schemaType.FullName }));
                    }
                }
            }
        }

        private static System.Type RemoveNullableFromType(System.Type type)
        {
            if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                type = type.GetGenericArguments()[0];
                return type;
            }
            if (type.IsByRef)
            {
                type = type.GetElementType();
            }
            return type;
        }

        private void SaveSchema(System.Type objectType, string methodName, DataTable[] schemaTables, System.Type schemaType)
        {
            Pair pair = null;
            if (schemaTables != null)
            {
                int length = schemaTables.Length;
                string[] x = new string[length];
                for (int i = 0; i < length; i++)
                {
                    x[i] = schemaTables[i].TableName;
                    schemaTables[i].TableName = "Table" + i.ToString(CultureInfo.InvariantCulture);
                }
                pair = new Pair(x, schemaTables);
            }
            base.DesignerState["DataSourceSchema"] = pair;
            base.DesignerState["DataSourceSchemaTypeName"] = (objectType == null) ? string.Empty : objectType.FullName;
            base.DesignerState["DataSourceSchemaSelectMethod"] = methodName;
            string a = base.DesignerState["DataSourceSchemaSelectMethodReturnTypeName"] as string;
            if (!string.Equals(a, schemaType.FullName, StringComparison.OrdinalIgnoreCase))
            {
                base.DesignerState["DataSourceSchemaSelectMethodReturnTypeName"] = schemaType.FullName;
                this._selectMethodReturnType = schemaType;
            }
        }

        internal static void SetParameterType(Parameter parameter, System.Type type)
        {
            parameter.DbType = GetDbTypeForType(type);
            if (parameter.DbType == DbType.Object)
            {
                parameter.Type = GetTypeCodeForType(type);
            }
            else
            {
                parameter.Type = TypeCode.Empty;
            }
        }

        public override bool CanConfigure
        {
            get
            {
                return this.TypeServiceAvailable;
            }
        }

        public override bool CanRefreshSchema
        {
            get
            {
                return ((!string.IsNullOrEmpty(this.TypeName) && !string.IsNullOrEmpty(this.SelectMethod)) && this.TypeServiceAvailable);
            }
        }

        internal System.Web.UI.WebControls.ObjectDataSource ObjectDataSource
        {
            get
            {
                return (System.Web.UI.WebControls.ObjectDataSource) base.Component;
            }
        }

        public string SelectMethod
        {
            get
            {
                return this.ObjectDataSource.SelectMethod;
            }
            set
            {
                if (value != this.SelectMethod)
                {
                    this.ObjectDataSource.SelectMethod = value;
                    this.UpdateDesignTimeHtml();
                    if (this.CanRefreshSchema && !this._inWizard)
                    {
                        this.RefreshSchema(true);
                    }
                    else
                    {
                        this.OnDataSourceChanged(EventArgs.Empty);
                    }
                }
            }
        }

        internal System.Type SelectMethodReturnType
        {
            get
            {
                if (this._selectMethodReturnType == null)
                {
                    string str = base.DesignerState["DataSourceSchemaSelectMethodReturnTypeName"] as string;
                    if (!string.IsNullOrEmpty(str))
                    {
                        this._selectMethodReturnType = GetType(base.Component.Site, str, true);
                    }
                }
                return this._selectMethodReturnType;
            }
        }

        internal object ShowOnlyDataComponentsState
        {
            get
            {
                return base.DesignerState["ShowOnlyDataComponentsState"];
            }
            set
            {
                base.DesignerState["ShowOnlyDataComponentsState"] = value;
            }
        }

        public string TypeName
        {
            get
            {
                return this.ObjectDataSource.TypeName;
            }
            set
            {
                if (value != this.TypeName)
                {
                    this.ObjectDataSource.TypeName = value;
                    this.UpdateDesignTimeHtml();
                    if (this.CanRefreshSchema)
                    {
                        this.RefreshSchema(true);
                    }
                    else
                    {
                        this.OnDataSourceChanged(EventArgs.Empty);
                    }
                }
            }
        }

        private bool TypeServiceAvailable
        {
            get
            {
                IServiceProvider site = base.Component.Site;
                if (site == null)
                {
                    return false;
                }
                ITypeResolutionService service = (ITypeResolutionService) site.GetService(typeof(ITypeResolutionService));
                ITypeDiscoveryService service2 = (ITypeDiscoveryService) site.GetService(typeof(ITypeDiscoveryService));
                if (service == null)
                {
                    return (service2 != null);
                }
                return true;
            }
        }

        private sealed class DataTableArraySchema : IDataSourceSchema
        {
            private DataTable[] _tables;

            public DataTableArraySchema(DataTable[] tables)
            {
                this._tables = tables;
            }

            public IDataSourceViewSchema[] GetViews()
            {
                DataSetViewSchema[] schemaArray = new DataSetViewSchema[this._tables.Length];
                for (int i = 0; i < this._tables.Length; i++)
                {
                    schemaArray[i] = new DataSetViewSchema(this._tables[i]);
                }
                return schemaArray;
            }
        }
    }
}

