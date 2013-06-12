namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Data;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.UI;

    public class ObjectDataSourceView : DataSourceView, IStateManager
    {
        private ConflictOptions _conflictDetection;
        private HttpContext _context;
        private bool _convertNullToDBNull;
        private string _dataObjectTypeName;
        private string _deleteMethod;
        private ParameterCollection _deleteParameters;
        private bool _enablePaging;
        private string _filterExpression;
        private ParameterCollection _filterParameters;
        private string _insertMethod;
        private ParameterCollection _insertParameters;
        private string _maximumRowsParameterName;
        private string _oldValuesParameterFormatString;
        private ObjectDataSource _owner;
        private string _selectCountMethod;
        private string _selectMethod;
        private ParameterCollection _selectParameters;
        private string _sortParameterName;
        private string _startRowIndexParameterName;
        private bool _tracking;
        private string _typeName;
        private string _updateMethod;
        private ParameterCollection _updateParameters;
        private static readonly object EventDeleted = new object();
        private static readonly object EventDeleting = new object();
        private static readonly object EventFiltering = new object();
        private static readonly object EventInserted = new object();
        private static readonly object EventInserting = new object();
        private static readonly object EventObjectCreated = new object();
        private static readonly object EventObjectCreating = new object();
        private static readonly object EventObjectDisposing = new object();
        private static readonly object EventSelected = new object();
        private static readonly object EventSelecting = new object();
        private static readonly object EventUpdated = new object();
        private static readonly object EventUpdating = new object();

        public event ObjectDataSourceStatusEventHandler Deleted
        {
            add
            {
                base.Events.AddHandler(EventDeleted, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventDeleted, value);
            }
        }

        public event ObjectDataSourceMethodEventHandler Deleting
        {
            add
            {
                base.Events.AddHandler(EventDeleting, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventDeleting, value);
            }
        }

        public event ObjectDataSourceFilteringEventHandler Filtering
        {
            add
            {
                base.Events.AddHandler(EventFiltering, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventFiltering, value);
            }
        }

        public event ObjectDataSourceStatusEventHandler Inserted
        {
            add
            {
                base.Events.AddHandler(EventInserted, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventInserted, value);
            }
        }

        public event ObjectDataSourceMethodEventHandler Inserting
        {
            add
            {
                base.Events.AddHandler(EventInserting, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventInserting, value);
            }
        }

        public event ObjectDataSourceObjectEventHandler ObjectCreated
        {
            add
            {
                base.Events.AddHandler(EventObjectCreated, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventObjectCreated, value);
            }
        }

        public event ObjectDataSourceObjectEventHandler ObjectCreating
        {
            add
            {
                base.Events.AddHandler(EventObjectCreating, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventObjectCreating, value);
            }
        }

        public event ObjectDataSourceDisposingEventHandler ObjectDisposing
        {
            add
            {
                base.Events.AddHandler(EventObjectDisposing, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventObjectDisposing, value);
            }
        }

        public event ObjectDataSourceStatusEventHandler Selected
        {
            add
            {
                base.Events.AddHandler(EventSelected, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventSelected, value);
            }
        }

        public event ObjectDataSourceSelectingEventHandler Selecting
        {
            add
            {
                base.Events.AddHandler(EventSelecting, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventSelecting, value);
            }
        }

        public event ObjectDataSourceStatusEventHandler Updated
        {
            add
            {
                base.Events.AddHandler(EventUpdated, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventUpdated, value);
            }
        }

        public event ObjectDataSourceMethodEventHandler Updating
        {
            add
            {
                base.Events.AddHandler(EventUpdating, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventUpdating, value);
            }
        }

        public ObjectDataSourceView(ObjectDataSource owner, string name, HttpContext context) : base(owner, name)
        {
            this._owner = owner;
            this._context = context;
        }

        private object BuildDataObject(Type dataObjectType, IDictionary inputParameters)
        {
            object component = Activator.CreateInstance(dataObjectType);
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(component);
            foreach (DictionaryEntry entry in inputParameters)
            {
                string name = (entry.Key == null) ? string.Empty : entry.Key.ToString();
                PropertyDescriptor descriptor = properties.Find(name, true);
                if (descriptor == null)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("ObjectDataSourceView_DataObjectPropertyNotFound", new object[] { name, this._owner.ID }));
                }
                if (descriptor.IsReadOnly)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("ObjectDataSourceView_DataObjectPropertyReadOnly", new object[] { name, this._owner.ID }));
                }
                object obj3 = BuildObjectValue(entry.Value, descriptor.PropertyType, name);
                descriptor.SetValue(component, obj3);
            }
            return component;
        }

        private static object BuildObjectValue(object value, Type destinationType, string paramName)
        {
            if ((value != null) && !destinationType.IsInstanceOfType(value))
            {
                Type elementType = destinationType;
                bool flag = false;
                if (destinationType.IsGenericType && (destinationType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                {
                    elementType = destinationType.GetGenericArguments()[0];
                    flag = true;
                }
                else if (destinationType.IsByRef)
                {
                    elementType = destinationType.GetElementType();
                }
                value = ConvertType(value, elementType, paramName);
                if (flag)
                {
                    Type type = value.GetType();
                    if (elementType != type)
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("ObjectDataSourceView_CannotConvertType", new object[] { paramName, type.FullName, string.Format(CultureInfo.InvariantCulture, "Nullable<{0}>", new object[] { destinationType.GetGenericArguments()[0].FullName }) }));
                    }
                }
            }
            return value;
        }

        private static object ConvertType(object value, Type type, string paramName)
        {
            string text = value as string;
            if (text != null)
            {
                TypeConverter converter = TypeDescriptor.GetConverter(type);
                if (converter == null)
                {
                    return value;
                }
                try
                {
                    value = converter.ConvertFromInvariantString(text);
                }
                catch (NotSupportedException)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("ObjectDataSourceView_CannotConvertType", new object[] { paramName, typeof(string).FullName, type.FullName }));
                }
                catch (FormatException)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("ObjectDataSourceView_CannotConvertType", new object[] { paramName, typeof(string).FullName, type.FullName }));
                }
            }
            return value;
        }

        private IEnumerable CreateEnumerableData(object dataObject, DataSourceSelectArguments arguments)
        {
            if (this.FilterExpression.Length > 0)
            {
                throw new NotSupportedException(System.Web.SR.GetString("ObjectDataSourceView_FilterNotSupported", new object[] { this._owner.ID }));
            }
            if (!string.IsNullOrEmpty(arguments.SortExpression))
            {
                throw new NotSupportedException(System.Web.SR.GetString("ObjectDataSourceView_SortNotSupportedOnIEnumerable", new object[] { this._owner.ID }));
            }
            IEnumerable enumerable = dataObject as IEnumerable;
            if (enumerable != null)
            {
                if ((!this.EnablePaging && arguments.RetrieveTotalRowCount) && (this.SelectCountMethod.Length == 0))
                {
                    ICollection is2 = enumerable as ICollection;
                    if (is2 != null)
                    {
                        arguments.TotalRowCount = is2.Count;
                    }
                }
                return enumerable;
            }
            if (arguments.RetrieveTotalRowCount && (this.SelectCountMethod.Length == 0))
            {
                arguments.TotalRowCount = 1;
            }
            return new object[] { dataObject };
        }

        private IEnumerable CreateFilteredDataView(DataTable dataTable, string sortExpression, string filterExpression)
        {
            IOrderedDictionary values = this.FilterParameters.GetValues(this._context, this._owner);
            if (filterExpression.Length > 0)
            {
                ObjectDataSourceFilteringEventArgs e = new ObjectDataSourceFilteringEventArgs(values);
                this.OnFiltering(e);
                if (e.Cancel)
                {
                    return null;
                }
            }
            return FilteredDataSetHelper.CreateFilteredDataView(dataTable, sortExpression, filterExpression, values);
        }

        public int Delete(IDictionary keys, IDictionary oldValues)
        {
            return this.ExecuteDelete(keys, oldValues);
        }

        protected override int ExecuteDelete(IDictionary keys, IDictionary oldValues)
        {
            ObjectDataSourceMethod method;
            if (!this.CanDelete)
            {
                throw new NotSupportedException(System.Web.SR.GetString("ObjectDataSourceView_DeleteNotSupported", new object[] { this._owner.ID }));
            }
            Type type = this.GetType(this.TypeName);
            Type dataObjectType = this.TryGetDataObjectType();
            if (dataObjectType != null)
            {
                IDictionary destination = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
                MergeDictionaries(this.DeleteParameters, keys, destination);
                if (this.ConflictDetection == ConflictOptions.CompareAllValues)
                {
                    if (oldValues == null)
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("ObjectDataSourceView_Pessimistic", new object[] { System.Web.SR.GetString("DataSourceView_delete"), this._owner.ID, "oldValues" }));
                    }
                    MergeDictionaries(this.DeleteParameters, oldValues, destination);
                }
                object oldDataObject = this.BuildDataObject(dataObjectType, destination);
                method = this.GetResolvedMethodData(type, this.DeleteMethod, dataObjectType, oldDataObject, null, DataSourceOperation.Delete);
                ObjectDataSourceMethodEventArgs e = new ObjectDataSourceMethodEventArgs(method.Parameters);
                this.OnDeleting(e);
                if (e.Cancel)
                {
                    return 0;
                }
            }
            else
            {
                IOrderedDictionary dictionary2 = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
                string oldValuesParameterFormatString = this.OldValuesParameterFormatString;
                MergeDictionaries(this.DeleteParameters, this.DeleteParameters.GetValues(this._context, this._owner), dictionary2);
                MergeDictionaries(this.DeleteParameters, keys, dictionary2, oldValuesParameterFormatString);
                if (this.ConflictDetection == ConflictOptions.CompareAllValues)
                {
                    if (oldValues == null)
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("ObjectDataSourceView_Pessimistic", new object[] { System.Web.SR.GetString("DataSourceView_delete"), this._owner.ID, "oldValues" }));
                    }
                    MergeDictionaries(this.DeleteParameters, oldValues, dictionary2, oldValuesParameterFormatString);
                }
                ObjectDataSourceMethodEventArgs args2 = new ObjectDataSourceMethodEventArgs(dictionary2);
                this.OnDeleting(args2);
                if (args2.Cancel)
                {
                    return 0;
                }
                method = this.GetResolvedMethodData(type, this.DeleteMethod, dictionary2, DataSourceOperation.Delete);
            }
            ObjectDataSourceResult result = this.InvokeMethod(method);
            if (this._owner.Cache.Enabled)
            {
                this._owner.InvalidateCacheEntry();
            }
            this.OnDataSourceViewChanged(EventArgs.Empty);
            return result.AffectedRows;
        }

        protected override int ExecuteInsert(IDictionary values)
        {
            ObjectDataSourceMethod method;
            if (!this.CanInsert)
            {
                throw new NotSupportedException(System.Web.SR.GetString("ObjectDataSourceView_InsertNotSupported", new object[] { this._owner.ID }));
            }
            Type type = this.GetType(this.TypeName);
            Type dataObjectType = this.TryGetDataObjectType();
            if (dataObjectType != null)
            {
                if ((values == null) || (values.Count == 0))
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("ObjectDataSourceView_InsertRequiresValues", new object[] { this._owner.ID }));
                }
                IDictionary destination = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
                MergeDictionaries(this.InsertParameters, values, destination);
                object newDataObject = this.BuildDataObject(dataObjectType, destination);
                method = this.GetResolvedMethodData(type, this.InsertMethod, dataObjectType, null, newDataObject, DataSourceOperation.Insert);
                ObjectDataSourceMethodEventArgs e = new ObjectDataSourceMethodEventArgs(method.Parameters);
                this.OnInserting(e);
                if (e.Cancel)
                {
                    return 0;
                }
            }
            else
            {
                IOrderedDictionary dictionary2 = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
                MergeDictionaries(this.InsertParameters, this.InsertParameters.GetValues(this._context, this._owner), dictionary2);
                MergeDictionaries(this.InsertParameters, values, dictionary2);
                ObjectDataSourceMethodEventArgs args2 = new ObjectDataSourceMethodEventArgs(dictionary2);
                this.OnInserting(args2);
                if (args2.Cancel)
                {
                    return 0;
                }
                method = this.GetResolvedMethodData(type, this.InsertMethod, dictionary2, DataSourceOperation.Insert);
            }
            ObjectDataSourceResult result = this.InvokeMethod(method);
            if (this._owner.Cache.Enabled)
            {
                this._owner.InvalidateCacheEntry();
            }
            this.OnDataSourceViewChanged(EventArgs.Empty);
            return result.AffectedRows;
        }

        protected internal override IEnumerable ExecuteSelect(DataSourceSelectArguments arguments)
        {
            if (this.SelectMethod.Length == 0)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("ObjectDataSourceView_SelectNotSupported", new object[] { this._owner.ID }));
            }
            if (this.CanSort)
            {
                arguments.AddSupportedCapabilities(DataSourceCapabilities.Sort);
            }
            if (this.CanPage)
            {
                arguments.AddSupportedCapabilities(DataSourceCapabilities.Page);
            }
            if (this.CanRetrieveTotalRowCount)
            {
                arguments.AddSupportedCapabilities(DataSourceCapabilities.RetrieveTotalRowCount);
            }
            arguments.RaiseUnsupportedCapabilitiesError(this);
            IOrderedDictionary parameters = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
            foreach (DictionaryEntry entry in this.SelectParameters.GetValues(this._context, this._owner))
            {
                parameters[entry.Key] = entry.Value;
            }
            bool enabled = this._owner.Cache.Enabled;
            if (enabled)
            {
                object dataObject = this._owner.LoadDataFromCache(arguments.StartRowIndex, arguments.MaximumRows);
                if (dataObject != null)
                {
                    DataView view = dataObject as DataView;
                    if (view != null)
                    {
                        if (arguments.RetrieveTotalRowCount && (this.SelectCountMethod.Length == 0))
                        {
                            arguments.TotalRowCount = view.Count;
                        }
                        if (this.FilterExpression.Length > 0)
                        {
                            throw new NotSupportedException(System.Web.SR.GetString("ObjectDataSourceView_FilterNotSupported", new object[] { this._owner.ID }));
                        }
                        if (string.IsNullOrEmpty(arguments.SortExpression))
                        {
                            return view;
                        }
                    }
                    else
                    {
                        DataTable table = FilteredDataSetHelper.GetDataTable(this._owner, dataObject);
                        if (table != null)
                        {
                            this.ProcessPagingData(arguments, parameters);
                            return this.CreateFilteredDataView(table, arguments.SortExpression, this.FilterExpression);
                        }
                        IEnumerable enumerable = this.CreateEnumerableData(dataObject, arguments);
                        this.ProcessPagingData(arguments, parameters);
                        return enumerable;
                    }
                }
            }
            ObjectDataSourceSelectingEventArgs e = new ObjectDataSourceSelectingEventArgs(parameters, arguments, false);
            this.OnSelecting(e);
            if (e.Cancel)
            {
                return null;
            }
            OrderedDictionary mergedParameters = new OrderedDictionary(parameters.Count);
            foreach (DictionaryEntry entry2 in parameters)
            {
                mergedParameters.Add(entry2.Key, entry2.Value);
            }
            string sortParameterName = this.SortParameterName;
            if (sortParameterName.Length > 0)
            {
                parameters[sortParameterName] = arguments.SortExpression;
                arguments.SortExpression = string.Empty;
            }
            if (this.EnablePaging)
            {
                string maximumRowsParameterName = this.MaximumRowsParameterName;
                string startRowIndexParameterName = this.StartRowIndexParameterName;
                if (string.IsNullOrEmpty(maximumRowsParameterName) || string.IsNullOrEmpty(startRowIndexParameterName))
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("ObjectDataSourceView_MissingPagingSettings", new object[] { this._owner.ID }));
                }
                IDictionary source = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
                source[maximumRowsParameterName] = arguments.MaximumRows;
                source[startRowIndexParameterName] = arguments.StartRowIndex;
                MergeDictionaries(this.SelectParameters, source, parameters);
            }
            Type type = this.GetType(this.TypeName);
            object instance = null;
            ObjectDataSourceResult result = null;
            try
            {
                ObjectDataSourceMethod method = this.GetResolvedMethodData(type, this.SelectMethod, parameters, DataSourceOperation.Select);
                result = this.InvokeMethod(method, false, ref instance);
                if (result.ReturnValue == null)
                {
                    return null;
                }
                if (arguments.RetrieveTotalRowCount && (this.SelectCountMethod.Length > 0))
                {
                    int totalRowCount = -1;
                    if (enabled)
                    {
                        totalRowCount = this._owner.LoadTotalRowCountFromCache();
                        if (totalRowCount >= 0)
                        {
                            arguments.TotalRowCount = totalRowCount;
                        }
                    }
                    if (totalRowCount < 0)
                    {
                        totalRowCount = this.QueryTotalRowCount(mergedParameters, arguments, true, ref instance);
                        arguments.TotalRowCount = totalRowCount;
                        if (enabled)
                        {
                            this._owner.SaveTotalRowCountToCache(totalRowCount);
                        }
                    }
                }
            }
            finally
            {
                if (instance != null)
                {
                    this.ReleaseInstance(instance);
                }
            }
            DataView returnValue = result.ReturnValue as DataView;
            if (returnValue != null)
            {
                if (arguments.RetrieveTotalRowCount && (this.SelectCountMethod.Length == 0))
                {
                    arguments.TotalRowCount = returnValue.Count;
                }
                if (this.FilterExpression.Length > 0)
                {
                    throw new NotSupportedException(System.Web.SR.GetString("ObjectDataSourceView_FilterNotSupported", new object[] { this._owner.ID }));
                }
                if (!string.IsNullOrEmpty(arguments.SortExpression))
                {
                    if (enabled)
                    {
                        throw new NotSupportedException(System.Web.SR.GetString("ObjectDataSourceView_CacheNotSupportedOnSortedDataView", new object[] { this._owner.ID }));
                    }
                    returnValue.Sort = arguments.SortExpression;
                }
                if (enabled)
                {
                    this.SaveDataAndRowCountToCache(arguments, result.ReturnValue);
                }
                return returnValue;
            }
            DataTable dataTable = FilteredDataSetHelper.GetDataTable(this._owner, result.ReturnValue);
            if (dataTable != null)
            {
                if (arguments.RetrieveTotalRowCount && (this.SelectCountMethod.Length == 0))
                {
                    arguments.TotalRowCount = dataTable.Rows.Count;
                }
                if (enabled)
                {
                    this.SaveDataAndRowCountToCache(arguments, result.ReturnValue);
                }
                return this.CreateFilteredDataView(dataTable, arguments.SortExpression, this.FilterExpression);
            }
            IEnumerable data = this.CreateEnumerableData(result.ReturnValue, arguments);
            if (enabled)
            {
                if (data is IDataReader)
                {
                    throw new NotSupportedException(System.Web.SR.GetString("ObjectDataSourceView_CacheNotSupportedOnIDataReader", new object[] { this._owner.ID }));
                }
                this.SaveDataAndRowCountToCache(arguments, data);
            }
            return data;
        }

        protected override int ExecuteUpdate(IDictionary keys, IDictionary values, IDictionary oldValues)
        {
            ObjectDataSourceMethod method;
            if (!this.CanUpdate)
            {
                throw new NotSupportedException(System.Web.SR.GetString("ObjectDataSourceView_UpdateNotSupported", new object[] { this._owner.ID }));
            }
            Type type = this.GetType(this.TypeName);
            Type dataObjectType = this.TryGetDataObjectType();
            if (dataObjectType != null)
            {
                if (this.ConflictDetection == ConflictOptions.CompareAllValues)
                {
                    if (oldValues == null)
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("ObjectDataSourceView_Pessimistic", new object[] { System.Web.SR.GetString("DataSourceView_update"), this._owner.ID, "oldValues" }));
                    }
                    IDictionary destination = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
                    IDictionary dictionary2 = null;
                    MergeDictionaries(this.UpdateParameters, oldValues, destination);
                    MergeDictionaries(this.UpdateParameters, keys, destination);
                    MergeDictionaries(this.UpdateParameters, values, destination);
                    if (oldValues == null)
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("ObjectDataSourceView_Pessimistic", new object[] { System.Web.SR.GetString("DataSourceView_update"), this._owner.ID, "oldValues" }));
                    }
                    dictionary2 = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
                    MergeDictionaries(this.UpdateParameters, oldValues, dictionary2);
                    MergeDictionaries(this.UpdateParameters, keys, dictionary2);
                    object newDataObject = this.BuildDataObject(dataObjectType, destination);
                    object oldDataObject = this.BuildDataObject(dataObjectType, dictionary2);
                    method = this.GetResolvedMethodData(type, this.UpdateMethod, dataObjectType, oldDataObject, newDataObject, DataSourceOperation.Update);
                }
                else
                {
                    IDictionary dictionary3 = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
                    MergeDictionaries(this.UpdateParameters, oldValues, dictionary3);
                    MergeDictionaries(this.UpdateParameters, keys, dictionary3);
                    MergeDictionaries(this.UpdateParameters, values, dictionary3);
                    object obj4 = this.BuildDataObject(dataObjectType, dictionary3);
                    method = this.GetResolvedMethodData(type, this.UpdateMethod, dataObjectType, null, obj4, DataSourceOperation.Update);
                }
                ObjectDataSourceMethodEventArgs e = new ObjectDataSourceMethodEventArgs(method.Parameters);
                this.OnUpdating(e);
                if (e.Cancel)
                {
                    return 0;
                }
            }
            else
            {
                IOrderedDictionary dictionary4 = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
                string oldValuesParameterFormatString = this.OldValuesParameterFormatString;
                IDictionary source = this.UpdateParameters.GetValues(this._context, this._owner);
                if (keys != null)
                {
                    foreach (DictionaryEntry entry in keys)
                    {
                        if (source.Contains(entry.Key))
                        {
                            source.Remove(entry.Key);
                        }
                    }
                }
                MergeDictionaries(this.UpdateParameters, source, dictionary4);
                MergeDictionaries(this.UpdateParameters, values, dictionary4);
                if (this.ConflictDetection == ConflictOptions.CompareAllValues)
                {
                    MergeDictionaries(this.UpdateParameters, oldValues, dictionary4, oldValuesParameterFormatString);
                }
                MergeDictionaries(this.UpdateParameters, keys, dictionary4, oldValuesParameterFormatString);
                ObjectDataSourceMethodEventArgs args2 = new ObjectDataSourceMethodEventArgs(dictionary4);
                this.OnUpdating(args2);
                if (args2.Cancel)
                {
                    return 0;
                }
                method = this.GetResolvedMethodData(type, this.UpdateMethod, dictionary4, DataSourceOperation.Update);
            }
            ObjectDataSourceResult result = this.InvokeMethod(method);
            if (this._owner.Cache.Enabled)
            {
                this._owner.InvalidateCacheEntry();
            }
            this.OnDataSourceViewChanged(EventArgs.Empty);
            return result.AffectedRows;
        }

        private static DataObjectMethodType GetMethodTypeFromOperation(DataSourceOperation operation)
        {
            switch (operation)
            {
                case DataSourceOperation.Delete:
                    return DataObjectMethodType.Delete;

                case DataSourceOperation.Insert:
                    return DataObjectMethodType.Insert;

                case DataSourceOperation.Select:
                    return DataObjectMethodType.Select;

                case DataSourceOperation.Update:
                    return DataObjectMethodType.Update;
            }
            throw new ArgumentOutOfRangeException("operation");
        }

        private IDictionary GetOutputParameters(ParameterInfo[] parameters, object[] values)
        {
            IDictionary dictionary = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo info = parameters[i];
                if (info.ParameterType.IsByRef)
                {
                    dictionary[info.Name] = values[i];
                }
            }
            return dictionary;
        }

        private ObjectDataSourceMethod GetResolvedMethodData(Type type, string methodName, IDictionary allParameters, DataSourceOperation operation)
        {
            bool flag = operation == DataSourceOperation.SelectCount;
            DataObjectMethodType select = DataObjectMethodType.Select;
            if (!flag)
            {
                select = GetMethodTypeFromOperation(operation);
            }
            MethodInfo[] methods = type.GetMethods(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            MethodInfo methodInfo = null;
            ParameterInfo[] infoArray2 = null;
            int num = -1;
            bool flag2 = false;
            int count = allParameters.Count;
            foreach (MethodInfo info2 in methods)
            {
                if (!string.Equals(methodName, info2.Name, StringComparison.OrdinalIgnoreCase) || info2.IsGenericMethodDefinition)
                {
                    continue;
                }
                ParameterInfo[] infoArray3 = info2.GetParameters();
                if (infoArray3.Length == count)
                {
                    bool flag3 = false;
                    foreach (ParameterInfo info3 in infoArray3)
                    {
                        if (!allParameters.Contains(info3.Name))
                        {
                            flag3 = true;
                            break;
                        }
                    }
                    if (!flag3)
                    {
                        int num4 = 0;
                        if (!flag)
                        {
                            DataObjectMethodAttribute attribute = Attribute.GetCustomAttribute(info2, typeof(DataObjectMethodAttribute), true) as DataObjectMethodAttribute;
                            if ((attribute != null) && (attribute.MethodType == select))
                            {
                                if (attribute.IsDefault)
                                {
                                    num4 = 2;
                                }
                                else
                                {
                                    num4 = 1;
                                }
                            }
                        }
                        if (num4 == num)
                        {
                            flag2 = true;
                        }
                        else if (num4 > num)
                        {
                            num = num4;
                            flag2 = false;
                            methodInfo = info2;
                            infoArray2 = infoArray3;
                        }
                    }
                }
            }
            if (flag2)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("ObjectDataSourceView_MultipleOverloads", new object[] { this._owner.ID }));
            }
            if (methodInfo == null)
            {
                if (count == 0)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("ObjectDataSourceView_MethodNotFoundNoParams", new object[] { this._owner.ID, methodName }));
                }
                string[] array = new string[count];
                allParameters.Keys.CopyTo(array, 0);
                string str = string.Join(", ", array);
                throw new InvalidOperationException(System.Web.SR.GetString("ObjectDataSourceView_MethodNotFoundWithParams", new object[] { this._owner.ID, methodName, str }));
            }
            OrderedDictionary parameters = null;
            int length = infoArray2.Length;
            if (length > 0)
            {
                parameters = new OrderedDictionary(length, StringComparer.OrdinalIgnoreCase);
                bool convertNullToDBNull = this.ConvertNullToDBNull;
                for (int i = 0; i < infoArray2.Length; i++)
                {
                    ParameterInfo info4 = infoArray2[i];
                    string name = info4.Name;
                    object obj2 = allParameters[name];
                    if (convertNullToDBNull && (obj2 == null))
                    {
                        obj2 = DBNull.Value;
                    }
                    else
                    {
                        obj2 = BuildObjectValue(obj2, info4.ParameterType, name);
                    }
                    parameters.Add(name, obj2);
                }
            }
            return new ObjectDataSourceMethod(operation, type, methodInfo, parameters);
        }

        private ObjectDataSourceMethod GetResolvedMethodData(Type type, string methodName, Type dataObjectType, object oldDataObject, object newDataObject, DataSourceOperation operation)
        {
            int num;
            MethodInfo[] methods = type.GetMethods(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            MethodInfo methodInfo = null;
            ParameterInfo[] infoArray2 = null;
            if (oldDataObject == null)
            {
                num = 1;
            }
            else if (newDataObject == null)
            {
                num = 1;
            }
            else
            {
                num = 2;
            }
            foreach (MethodInfo info2 in methods)
            {
                if (string.Equals(methodName, info2.Name, StringComparison.OrdinalIgnoreCase) && !info2.IsGenericMethodDefinition)
                {
                    ParameterInfo[] parameters = info2.GetParameters();
                    if (parameters.Length == num)
                    {
                        if ((num == 1) && (parameters[0].ParameterType == dataObjectType))
                        {
                            methodInfo = info2;
                            infoArray2 = parameters;
                            break;
                        }
                        if (((num == 2) && (parameters[0].ParameterType == dataObjectType)) && (parameters[1].ParameterType == dataObjectType))
                        {
                            methodInfo = info2;
                            infoArray2 = parameters;
                            break;
                        }
                    }
                }
            }
            if (methodInfo == null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("ObjectDataSourceView_MethodNotFoundForDataObject", new object[] { this._owner.ID, methodName, dataObjectType.FullName }));
            }
            OrderedDictionary dictionary = new OrderedDictionary(2, StringComparer.OrdinalIgnoreCase);
            if (oldDataObject == null)
            {
                dictionary.Add(infoArray2[0].Name, newDataObject);
            }
            else if (newDataObject == null)
            {
                dictionary.Add(infoArray2[0].Name, oldDataObject);
            }
            else
            {
                string name = infoArray2[0].Name;
                string a = infoArray2[1].Name;
                string b = string.Format(CultureInfo.InvariantCulture, this.OldValuesParameterFormatString, new object[] { name });
                if (string.Equals(a, b, StringComparison.OrdinalIgnoreCase))
                {
                    dictionary.Add(name, newDataObject);
                    dictionary.Add(a, oldDataObject);
                }
                else
                {
                    b = string.Format(CultureInfo.InvariantCulture, this.OldValuesParameterFormatString, new object[] { a });
                    if (!string.Equals(name, b, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("ObjectDataSourceView_NoOldValuesParams", new object[] { this._owner.ID }));
                    }
                    dictionary.Add(name, oldDataObject);
                    dictionary.Add(a, newDataObject);
                }
            }
            return new ObjectDataSourceMethod(operation, type, methodInfo, dictionary.AsReadOnly());
        }

        private Type GetType(string typeName)
        {
            if (this.TypeName.Length == 0)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("ObjectDataSourceView_TypeNotSpecified", new object[] { this._owner.ID }));
            }
            Type type = BuildManager.GetType(this.TypeName, false, true);
            if (type == null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("ObjectDataSourceView_TypeNotFound", new object[] { this._owner.ID }));
            }
            return type;
        }

        public int Insert(IDictionary values)
        {
            return this.ExecuteInsert(values);
        }

        private ObjectDataSourceResult InvokeMethod(ObjectDataSourceMethod method)
        {
            object instance = null;
            return this.InvokeMethod(method, true, ref instance);
        }

        private ObjectDataSourceResult InvokeMethod(ObjectDataSourceMethod method, bool disposeInstance, ref object instance)
        {
            if (method.MethodInfo.IsStatic)
            {
                if (instance != null)
                {
                    this.ReleaseInstance(instance);
                }
                instance = null;
            }
            else if (instance == null)
            {
                ObjectDataSourceEventArgs e = new ObjectDataSourceEventArgs(null);
                this.OnObjectCreating(e);
                if (e.ObjectInstance == null)
                {
                    e.ObjectInstance = Activator.CreateInstance(method.Type);
                    this.OnObjectCreated(e);
                }
                instance = e.ObjectInstance;
            }
            object returnValue = null;
            int affectedRows = -1;
            bool flag = false;
            object[] parameters = null;
            if ((method.Parameters != null) && (method.Parameters.Count > 0))
            {
                parameters = new object[method.Parameters.Count];
                for (int i = 0; i < method.Parameters.Count; i++)
                {
                    parameters[i] = method.Parameters[i];
                }
            }
            try
            {
                returnValue = method.MethodInfo.Invoke(instance, parameters);
            }
            catch (Exception exception)
            {
                IDictionary outputParameters = this.GetOutputParameters(method.MethodInfo.GetParameters(), parameters);
                ObjectDataSourceStatusEventArgs args2 = new ObjectDataSourceStatusEventArgs(returnValue, outputParameters, exception);
                flag = true;
                switch (method.Operation)
                {
                    case DataSourceOperation.Delete:
                        this.OnDeleted(args2);
                        break;

                    case DataSourceOperation.Insert:
                        this.OnInserted(args2);
                        break;

                    case DataSourceOperation.Select:
                        this.OnSelected(args2);
                        break;

                    case DataSourceOperation.Update:
                        this.OnUpdated(args2);
                        break;

                    case DataSourceOperation.SelectCount:
                        this.OnSelected(args2);
                        break;
                }
                affectedRows = args2.AffectedRows;
                if (!args2.ExceptionHandled)
                {
                    throw;
                }
            }
            finally
            {
                try
                {
                    if (!flag)
                    {
                        IDictionary dictionary2 = this.GetOutputParameters(method.MethodInfo.GetParameters(), parameters);
                        ObjectDataSourceStatusEventArgs args3 = new ObjectDataSourceStatusEventArgs(returnValue, dictionary2);
                        switch (method.Operation)
                        {
                            case DataSourceOperation.Delete:
                                this.OnDeleted(args3);
                                break;

                            case DataSourceOperation.Insert:
                                this.OnInserted(args3);
                                break;

                            case DataSourceOperation.Select:
                                this.OnSelected(args3);
                                break;

                            case DataSourceOperation.Update:
                                this.OnUpdated(args3);
                                break;

                            case DataSourceOperation.SelectCount:
                                this.OnSelected(args3);
                                break;
                        }
                        affectedRows = args3.AffectedRows;
                    }
                }
                finally
                {
                    if ((instance != null) && disposeInstance)
                    {
                        this.ReleaseInstance(instance);
                        instance = null;
                    }
                }
            }
            return new ObjectDataSourceResult(returnValue, affectedRows);
        }

        protected virtual void LoadViewState(object savedState)
        {
            if (savedState != null)
            {
                Pair pair = (Pair) savedState;
                if (pair.First != null)
                {
                    ((IStateManager) this.SelectParameters).LoadViewState(pair.First);
                }
                if (pair.Second != null)
                {
                    ((IStateManager) this.FilterParameters).LoadViewState(pair.Second);
                }
            }
        }

        private static void MergeDictionaries(ParameterCollection reference, IDictionary source, IDictionary destination)
        {
            MergeDictionaries(reference, source, destination, null);
        }

        private static void MergeDictionaries(ParameterCollection reference, IDictionary source, IDictionary destination, string parameterNameFormatString)
        {
            if (source != null)
            {
                foreach (DictionaryEntry entry in source)
                {
                    object obj2 = entry.Value;
                    Parameter parameter = null;
                    string key = (string) entry.Key;
                    if (parameterNameFormatString != null)
                    {
                        key = string.Format(CultureInfo.InvariantCulture, parameterNameFormatString, new object[] { key });
                    }
                    foreach (Parameter parameter2 in reference)
                    {
                        if (string.Equals(parameter2.Name, key, StringComparison.OrdinalIgnoreCase))
                        {
                            parameter = parameter2;
                            break;
                        }
                    }
                    if (parameter != null)
                    {
                        obj2 = parameter.GetValue(obj2, true);
                    }
                    destination[key] = obj2;
                }
            }
        }

        protected virtual void OnDeleted(ObjectDataSourceStatusEventArgs e)
        {
            ObjectDataSourceStatusEventHandler handler = base.Events[EventDeleted] as ObjectDataSourceStatusEventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnDeleting(ObjectDataSourceMethodEventArgs e)
        {
            ObjectDataSourceMethodEventHandler handler = base.Events[EventDeleting] as ObjectDataSourceMethodEventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnFiltering(ObjectDataSourceFilteringEventArgs e)
        {
            ObjectDataSourceFilteringEventHandler handler = base.Events[EventFiltering] as ObjectDataSourceFilteringEventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnInserted(ObjectDataSourceStatusEventArgs e)
        {
            ObjectDataSourceStatusEventHandler handler = base.Events[EventInserted] as ObjectDataSourceStatusEventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnInserting(ObjectDataSourceMethodEventArgs e)
        {
            ObjectDataSourceMethodEventHandler handler = base.Events[EventInserting] as ObjectDataSourceMethodEventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnObjectCreated(ObjectDataSourceEventArgs e)
        {
            ObjectDataSourceObjectEventHandler handler = base.Events[EventObjectCreated] as ObjectDataSourceObjectEventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnObjectCreating(ObjectDataSourceEventArgs e)
        {
            ObjectDataSourceObjectEventHandler handler = base.Events[EventObjectCreating] as ObjectDataSourceObjectEventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnObjectDisposing(ObjectDataSourceDisposingEventArgs e)
        {
            ObjectDataSourceDisposingEventHandler handler = base.Events[EventObjectDisposing] as ObjectDataSourceDisposingEventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnSelected(ObjectDataSourceStatusEventArgs e)
        {
            ObjectDataSourceStatusEventHandler handler = base.Events[EventSelected] as ObjectDataSourceStatusEventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnSelecting(ObjectDataSourceSelectingEventArgs e)
        {
            ObjectDataSourceSelectingEventHandler handler = base.Events[EventSelecting] as ObjectDataSourceSelectingEventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnUpdated(ObjectDataSourceStatusEventArgs e)
        {
            ObjectDataSourceStatusEventHandler handler = base.Events[EventUpdated] as ObjectDataSourceStatusEventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnUpdating(ObjectDataSourceMethodEventArgs e)
        {
            ObjectDataSourceMethodEventHandler handler = base.Events[EventUpdating] as ObjectDataSourceMethodEventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void ProcessPagingData(DataSourceSelectArguments arguments, IOrderedDictionary parameters)
        {
            if (arguments.RetrieveTotalRowCount)
            {
                int totalRowCount = this._owner.LoadTotalRowCountFromCache();
                if (totalRowCount >= 0)
                {
                    arguments.TotalRowCount = totalRowCount;
                }
                else
                {
                    object instance = null;
                    totalRowCount = this.QueryTotalRowCount(parameters, arguments, true, ref instance);
                    arguments.TotalRowCount = totalRowCount;
                    this._owner.SaveTotalRowCountToCache(totalRowCount);
                }
            }
        }

        private int QueryTotalRowCount(IOrderedDictionary mergedParameters, DataSourceSelectArguments arguments, bool disposeInstance, ref object instance)
        {
            if (this.SelectCountMethod.Length > 0)
            {
                ObjectDataSourceSelectingEventArgs e = new ObjectDataSourceSelectingEventArgs(mergedParameters, arguments, true);
                this.OnSelecting(e);
                if (e.Cancel)
                {
                    return -1;
                }
                Type type = this.GetType(this.TypeName);
                ObjectDataSourceMethod method = this.GetResolvedMethodData(type, this.SelectCountMethod, mergedParameters, DataSourceOperation.SelectCount);
                ObjectDataSourceResult result = this.InvokeMethod(method, disposeInstance, ref instance);
                if ((result.ReturnValue != null) && (result.ReturnValue is int))
                {
                    return (int) result.ReturnValue;
                }
            }
            return -1;
        }

        private void ReleaseInstance(object instance)
        {
            ObjectDataSourceDisposingEventArgs e = new ObjectDataSourceDisposingEventArgs(instance);
            this.OnObjectDisposing(e);
            if (!e.Cancel)
            {
                IDisposable disposable = instance as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
        }

        private void SaveDataAndRowCountToCache(DataSourceSelectArguments arguments, object data)
        {
            if (arguments.RetrieveTotalRowCount && (this._owner.LoadTotalRowCountFromCache() != arguments.TotalRowCount))
            {
                this._owner.SaveTotalRowCountToCache(arguments.TotalRowCount);
            }
            this._owner.SaveDataToCache(arguments.StartRowIndex, arguments.MaximumRows, data);
        }

        protected virtual object SaveViewState()
        {
            Pair pair = new Pair {
                First = (this._selectParameters != null) ? ((IStateManager) this._selectParameters).SaveViewState() : null,
                Second = (this._filterParameters != null) ? ((IStateManager) this._filterParameters).SaveViewState() : null
            };
            if ((pair.First == null) && (pair.Second == null))
            {
                return null;
            }
            return pair;
        }

        public IEnumerable Select(DataSourceSelectArguments arguments)
        {
            return this.ExecuteSelect(arguments);
        }

        private void SelectParametersChangedEventHandler(object o, EventArgs e)
        {
            this.OnDataSourceViewChanged(EventArgs.Empty);
        }

        void IStateManager.LoadViewState(object savedState)
        {
            this.LoadViewState(savedState);
        }

        object IStateManager.SaveViewState()
        {
            return this.SaveViewState();
        }

        void IStateManager.TrackViewState()
        {
            this.TrackViewState();
        }

        protected virtual void TrackViewState()
        {
            this._tracking = true;
            if (this._selectParameters != null)
            {
                ((IStateManager) this._selectParameters).TrackViewState();
            }
            if (this._filterParameters != null)
            {
                ((IStateManager) this._filterParameters).TrackViewState();
            }
        }

        private Type TryGetDataObjectType()
        {
            string dataObjectTypeName = this.DataObjectTypeName;
            if (dataObjectTypeName.Length == 0)
            {
                return null;
            }
            Type type = BuildManager.GetType(dataObjectTypeName, false, true);
            if (type == null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("ObjectDataSourceView_DataObjectTypeNotFound", new object[] { this._owner.ID }));
            }
            return type;
        }

        public int Update(IDictionary keys, IDictionary values, IDictionary oldValues)
        {
            return this.ExecuteUpdate(keys, values, oldValues);
        }

        public override bool CanDelete
        {
            get
            {
                return (this.DeleteMethod.Length != 0);
            }
        }

        public override bool CanInsert
        {
            get
            {
                return (this.InsertMethod.Length != 0);
            }
        }

        public override bool CanPage
        {
            get
            {
                return this.EnablePaging;
            }
        }

        public override bool CanRetrieveTotalRowCount
        {
            get
            {
                if (this.SelectCountMethod.Length <= 0)
                {
                    return !this.EnablePaging;
                }
                return true;
            }
        }

        public override bool CanSort
        {
            get
            {
                return true;
            }
        }

        public override bool CanUpdate
        {
            get
            {
                return (this.UpdateMethod.Length != 0);
            }
        }

        public ConflictOptions ConflictDetection
        {
            get
            {
                return this._conflictDetection;
            }
            set
            {
                if ((value < ConflictOptions.OverwriteChanges) || (value > ConflictOptions.CompareAllValues))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this._conflictDetection = value;
                this.OnDataSourceViewChanged(EventArgs.Empty);
            }
        }

        public bool ConvertNullToDBNull
        {
            get
            {
                return this._convertNullToDBNull;
            }
            set
            {
                this._convertNullToDBNull = value;
            }
        }

        public string DataObjectTypeName
        {
            get
            {
                if (this._dataObjectTypeName == null)
                {
                    return string.Empty;
                }
                return this._dataObjectTypeName;
            }
            set
            {
                if (this.DataObjectTypeName != value)
                {
                    this._dataObjectTypeName = value;
                    this.OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }

        public string DeleteMethod
        {
            get
            {
                if (this._deleteMethod == null)
                {
                    return string.Empty;
                }
                return this._deleteMethod;
            }
            set
            {
                this._deleteMethod = value;
            }
        }

        public ParameterCollection DeleteParameters
        {
            get
            {
                if (this._deleteParameters == null)
                {
                    this._deleteParameters = new ParameterCollection();
                }
                return this._deleteParameters;
            }
        }

        public bool EnablePaging
        {
            get
            {
                return this._enablePaging;
            }
            set
            {
                if (this.EnablePaging != value)
                {
                    this._enablePaging = value;
                    this.OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }

        public string FilterExpression
        {
            get
            {
                if (this._filterExpression == null)
                {
                    return string.Empty;
                }
                return this._filterExpression;
            }
            set
            {
                if (this.FilterExpression != value)
                {
                    this._filterExpression = value;
                    this.OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }

        public ParameterCollection FilterParameters
        {
            get
            {
                if (this._filterParameters == null)
                {
                    this._filterParameters = new ParameterCollection();
                    this._filterParameters.ParametersChanged += new EventHandler(this.SelectParametersChangedEventHandler);
                    if (this._tracking)
                    {
                        ((IStateManager) this._filterParameters).TrackViewState();
                    }
                }
                return this._filterParameters;
            }
        }

        public string InsertMethod
        {
            get
            {
                if (this._insertMethod == null)
                {
                    return string.Empty;
                }
                return this._insertMethod;
            }
            set
            {
                this._insertMethod = value;
            }
        }

        public ParameterCollection InsertParameters
        {
            get
            {
                if (this._insertParameters == null)
                {
                    this._insertParameters = new ParameterCollection();
                }
                return this._insertParameters;
            }
        }

        protected bool IsTrackingViewState
        {
            get
            {
                return this._tracking;
            }
        }

        public string MaximumRowsParameterName
        {
            get
            {
                if (this._maximumRowsParameterName == null)
                {
                    return "maximumRows";
                }
                return this._maximumRowsParameterName;
            }
            set
            {
                if (this.MaximumRowsParameterName != value)
                {
                    this._maximumRowsParameterName = value;
                    this.OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }

        [WebSysDescription("DataSource_OldValuesParameterFormatString"), WebCategory("Data"), DefaultValue("{0}")]
        public string OldValuesParameterFormatString
        {
            get
            {
                if (this._oldValuesParameterFormatString == null)
                {
                    return "{0}";
                }
                return this._oldValuesParameterFormatString;
            }
            set
            {
                this._oldValuesParameterFormatString = value;
                this.OnDataSourceViewChanged(EventArgs.Empty);
            }
        }

        public string SelectCountMethod
        {
            get
            {
                if (this._selectCountMethod == null)
                {
                    return string.Empty;
                }
                return this._selectCountMethod;
            }
            set
            {
                if (this.SelectCountMethod != value)
                {
                    this._selectCountMethod = value;
                    this.OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }

        public string SelectMethod
        {
            get
            {
                if (this._selectMethod == null)
                {
                    return string.Empty;
                }
                return this._selectMethod;
            }
            set
            {
                if (this.SelectMethod != value)
                {
                    this._selectMethod = value;
                    this.OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }

        public ParameterCollection SelectParameters
        {
            get
            {
                if (this._selectParameters == null)
                {
                    this._selectParameters = new ParameterCollection();
                    this._selectParameters.ParametersChanged += new EventHandler(this.SelectParametersChangedEventHandler);
                    if (this._tracking)
                    {
                        ((IStateManager) this._selectParameters).TrackViewState();
                    }
                }
                return this._selectParameters;
            }
        }

        public string SortParameterName
        {
            get
            {
                if (this._sortParameterName == null)
                {
                    return string.Empty;
                }
                return this._sortParameterName;
            }
            set
            {
                if (this.SortParameterName != value)
                {
                    this._sortParameterName = value;
                    this.OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }

        public string StartRowIndexParameterName
        {
            get
            {
                if (this._startRowIndexParameterName == null)
                {
                    return "startRowIndex";
                }
                return this._startRowIndexParameterName;
            }
            set
            {
                if (this.StartRowIndexParameterName != value)
                {
                    this._startRowIndexParameterName = value;
                    this.OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }

        bool IStateManager.IsTrackingViewState
        {
            get
            {
                return this.IsTrackingViewState;
            }
        }

        public string TypeName
        {
            get
            {
                if (this._typeName == null)
                {
                    return string.Empty;
                }
                return this._typeName;
            }
            set
            {
                if (this.TypeName != value)
                {
                    this._typeName = value;
                    this.OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }

        public string UpdateMethod
        {
            get
            {
                if (this._updateMethod == null)
                {
                    return string.Empty;
                }
                return this._updateMethod;
            }
            set
            {
                this._updateMethod = value;
            }
        }

        public ParameterCollection UpdateParameters
        {
            get
            {
                if (this._updateParameters == null)
                {
                    this._updateParameters = new ParameterCollection();
                }
                return this._updateParameters;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ObjectDataSourceMethod
        {
            internal DataSourceOperation Operation;
            internal System.Type Type;
            internal OrderedDictionary Parameters;
            internal System.Reflection.MethodInfo MethodInfo;
            internal ObjectDataSourceMethod(DataSourceOperation operation, System.Type type, System.Reflection.MethodInfo methodInfo, OrderedDictionary parameters)
            {
                this.Operation = operation;
                this.Type = type;
                this.Parameters = parameters;
                this.MethodInfo = methodInfo;
            }
        }

        private class ObjectDataSourceResult
        {
            internal int AffectedRows;
            internal object ReturnValue;

            internal ObjectDataSourceResult(object returnValue, int affectedRows)
            {
                this.ReturnValue = returnValue;
                this.AffectedRows = affectedRows;
            }
        }
    }
}

