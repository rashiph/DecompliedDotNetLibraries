namespace System.Web.UI.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Data;
    using System.Design;
    using System.Globalization;
    using System.Reflection;
    using System.Security.Permissions;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public sealed class DesignTimeData
    {
        public static readonly EventHandler DataBindingHandler = new EventHandler(GlobalDataBindingHandler.OnDataBind);

        private DesignTimeData()
        {
        }

        public static DataTable CreateDummyDataBoundDataTable()
        {
            DataTable table = new DataTable {
                Locale = CultureInfo.InvariantCulture
            };
            DataColumnCollection columns = table.Columns;
            columns.Add(System.Design.SR.GetString("Sample_Databound_Column", new object[] { 0 }), typeof(string));
            columns.Add(System.Design.SR.GetString("Sample_Databound_Column", new object[] { 1 }), typeof(int));
            columns.Add(System.Design.SR.GetString("Sample_Databound_Column", new object[] { 2 }), typeof(string));
            return table;
        }

        public static DataTable CreateDummyDataTable()
        {
            DataTable table = new DataTable {
                Locale = CultureInfo.InvariantCulture
            };
            DataColumnCollection columns = table.Columns;
            columns.Add(System.Design.SR.GetString("Sample_Column", new object[] { 0 }), typeof(string));
            columns.Add(System.Design.SR.GetString("Sample_Column", new object[] { 1 }), typeof(string));
            columns.Add(System.Design.SR.GetString("Sample_Column", new object[] { 2 }), typeof(string));
            return table;
        }

        public static DataTable CreateSampleDataTable(IEnumerable referenceData)
        {
            return CreateSampleDataTableInternal(referenceData, false);
        }

        public static DataTable CreateSampleDataTable(IEnumerable referenceData, bool useDataBoundData)
        {
            return CreateSampleDataTableInternal(referenceData, useDataBoundData);
        }

        private static DataTable CreateSampleDataTableInternal(IEnumerable referenceData, bool useDataBoundData)
        {
            DataTable table = new DataTable {
                Locale = CultureInfo.InvariantCulture
            };
            DataColumnCollection columns = table.Columns;
            PropertyDescriptorCollection dataFields = GetDataFields(referenceData);
            if (dataFields != null)
            {
                foreach (PropertyDescriptor descriptor in dataFields)
                {
                    Type propertyType = descriptor.PropertyType;
                    if (((!propertyType.IsPrimitive && (propertyType != typeof(DateTime))) && ((propertyType != typeof(decimal)) && (propertyType != typeof(DateTimeOffset)))) && (propertyType != typeof(TimeSpan)))
                    {
                        propertyType = typeof(string);
                    }
                    columns.Add(descriptor.Name, propertyType);
                }
            }
            if (columns.Count != 0)
            {
                return table;
            }
            if (useDataBoundData)
            {
                return CreateDummyDataBoundDataTable();
            }
            return CreateDummyDataTable();
        }

        public static PropertyDescriptorCollection GetDataFields(IEnumerable dataSource)
        {
            if (dataSource is ITypedList)
            {
                return ((ITypedList) dataSource).GetItemProperties(new PropertyDescriptor[0]);
            }
            PropertyInfo info = dataSource.GetType().GetProperty("Item", BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, null, new Type[] { typeof(int) }, null);
            if ((info != null) && (info.PropertyType != typeof(object)))
            {
                return TypeDescriptor.GetProperties(info.PropertyType);
            }
            return null;
        }

        public static IEnumerable GetDataMember(IListSource dataSource, string dataMember)
        {
            IEnumerable enumerable = null;
            IList list = dataSource.GetList();
            if ((list != null) && (list is ITypedList))
            {
                if (!dataSource.ContainsListCollection)
                {
                    if ((dataMember != null) && (dataMember.Length != 0))
                    {
                        throw new ArgumentException(System.Design.SR.GetString("DesignTimeData_BadDataMember"));
                    }
                    return list;
                }
                PropertyDescriptorCollection itemProperties = ((ITypedList) list).GetItemProperties(new PropertyDescriptor[0]);
                if ((itemProperties == null) || (itemProperties.Count == 0))
                {
                    return enumerable;
                }
                PropertyDescriptor descriptor = null;
                if ((dataMember == null) || (dataMember.Length == 0))
                {
                    descriptor = itemProperties[0];
                }
                else
                {
                    descriptor = itemProperties.Find(dataMember, true);
                }
                if (descriptor != null)
                {
                    object component = list[0];
                    object obj3 = descriptor.GetValue(component);
                    if ((obj3 != null) && (obj3 is IEnumerable))
                    {
                        enumerable = (IEnumerable) obj3;
                    }
                }
            }
            return enumerable;
        }

        public static string[] GetDataMembers(object dataSource)
        {
            IListSource source = dataSource as IListSource;
            if ((source != null) && source.ContainsListCollection)
            {
                ITypedList list = ((IListSource) dataSource).GetList() as ITypedList;
                if (list != null)
                {
                    PropertyDescriptorCollection itemProperties = list.GetItemProperties(new PropertyDescriptor[0]);
                    if (itemProperties != null)
                    {
                        ArrayList list3 = new ArrayList(itemProperties.Count);
                        foreach (PropertyDescriptor descriptor in itemProperties)
                        {
                            list3.Add(descriptor.Name);
                        }
                        return (string[]) list3.ToArray(typeof(string));
                    }
                }
            }
            return null;
        }

        public static IEnumerable GetDesignTimeDataSource(DataTable dataTable, int minimumRows)
        {
            int count = dataTable.Rows.Count;
            if (count < minimumRows)
            {
                int num2 = minimumRows - count;
                DataRowCollection rows = dataTable.Rows;
                DataColumnCollection columns = dataTable.Columns;
                int num3 = columns.Count;
                for (int i = 0; i < num2; i++)
                {
                    DataRow row = dataTable.NewRow();
                    int num5 = count + i;
                    for (int j = 0; j < num3; j++)
                    {
                        Type dataType = columns[j].DataType;
                        object today = null;
                        if (dataType == typeof(string))
                        {
                            today = System.Design.SR.GetString("Sample_Databound_Text_Alt");
                        }
                        else if ((((dataType == typeof(int)) || (dataType == typeof(short))) || ((dataType == typeof(long)) || (dataType == typeof(uint)))) || ((dataType == typeof(ushort)) || (dataType == typeof(ulong))))
                        {
                            today = num5;
                        }
                        else if ((dataType == typeof(byte)) || (dataType == typeof(sbyte)))
                        {
                            today = ((num5 % 2) != 0) ? 1 : 0;
                        }
                        else if (dataType == typeof(bool))
                        {
                            today = (num5 % 2) != 0;
                        }
                        else if (dataType == typeof(DateTime))
                        {
                            today = DateTime.Today;
                        }
                        else if (((dataType == typeof(double)) || (dataType == typeof(float))) || (dataType == typeof(decimal)))
                        {
                            today = ((double) i) / 10.0;
                        }
                        else if (dataType == typeof(char))
                        {
                            today = 'x';
                        }
                        else if (dataType == typeof(TimeSpan))
                        {
                            today = TimeSpan.Zero;
                        }
                        else if (dataType == typeof(DateTimeOffset))
                        {
                            today = DateTimeOffset.Now;
                        }
                        else
                        {
                            today = DBNull.Value;
                        }
                        row[j] = today;
                    }
                    rows.Add(row);
                }
            }
            return new DataView(dataTable);
        }

        public static object GetSelectedDataSource(IComponent component, string dataSource)
        {
            object obj2 = null;
            ISite site = component.Site;
            if (site != null)
            {
                IContainer service = (IContainer) site.GetService(typeof(IContainer));
                if (service == null)
                {
                    return obj2;
                }
                IComponent component2 = service.Components[dataSource];
                if ((component2 is IEnumerable) || (component2 is IListSource))
                {
                    return component2;
                }
            }
            return obj2;
        }

        public static IEnumerable GetSelectedDataSource(IComponent component, string dataSource, string dataMember)
        {
            IEnumerable enumerable = null;
            object selectedDataSource = GetSelectedDataSource(component, dataSource);
            if (selectedDataSource == null)
            {
                return enumerable;
            }
            IListSource source = selectedDataSource as IListSource;
            if (source != null)
            {
                if (!source.ContainsListCollection)
                {
                    return source.GetList();
                }
                return GetDataMember(source, dataMember);
            }
            return (IEnumerable) selectedDataSource;
        }
    }
}

