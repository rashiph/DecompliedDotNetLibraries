namespace System.Data.Common
{
    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Data.ProviderBase;
    using System.Globalization;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Threading;

    public class DataAdapter : Component, IDataAdapter
    {
        private bool _acceptChangesDuringFill;
        private bool _acceptChangesDuringUpdate;
        private bool _acceptChangesDuringUpdateAfterInsert;
        private bool _continueUpdateOnError;
        private LoadOption _fillLoadOption;
        private bool _hasFillErrorHandler;
        private System.Data.MissingMappingAction _missingMappingAction;
        private System.Data.MissingSchemaAction _missingSchemaAction;
        internal readonly int _objectID;
        private static int _objectTypeCount;
        private bool _returnProviderSpecificTypes;
        private DataTableMappingCollection _tableMappings;
        private static readonly object EventFillError = new object();

        [ResDescription("DataAdapter_FillError"), ResCategory("DataCategory_Fill")]
        public event FillErrorEventHandler FillError
        {
            add
            {
                this._hasFillErrorHandler = true;
                base.Events.AddHandler(EventFillError, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventFillError, value);
            }
        }

        protected DataAdapter()
        {
            this._acceptChangesDuringUpdate = true;
            this._acceptChangesDuringUpdateAfterInsert = true;
            this._acceptChangesDuringFill = true;
            this._missingMappingAction = System.Data.MissingMappingAction.Passthrough;
            this._missingSchemaAction = System.Data.MissingSchemaAction.Add;
            this._objectID = Interlocked.Increment(ref _objectTypeCount);
            GC.SuppressFinalize(this);
        }

        protected DataAdapter(DataAdapter from)
        {
            this._acceptChangesDuringUpdate = true;
            this._acceptChangesDuringUpdateAfterInsert = true;
            this._acceptChangesDuringFill = true;
            this._missingMappingAction = System.Data.MissingMappingAction.Passthrough;
            this._missingSchemaAction = System.Data.MissingSchemaAction.Add;
            this._objectID = Interlocked.Increment(ref _objectTypeCount);
            this.CloneFrom(from);
        }

        private static DataTable[] AddDataTableToArray(DataTable[] tables, DataTable newTable)
        {
            for (int i = 0; i < tables.Length; i++)
            {
                if (tables[i] == newTable)
                {
                    return tables;
                }
            }
            DataTable[] tableArray = new DataTable[tables.Length + 1];
            for (int j = 0; j < tables.Length; j++)
            {
                tableArray[j] = tables[j];
            }
            tableArray[tables.Length] = newTable;
            return tableArray;
        }

        private void CloneFrom(DataAdapter from)
        {
            this._acceptChangesDuringUpdate = from._acceptChangesDuringUpdate;
            this._acceptChangesDuringUpdateAfterInsert = from._acceptChangesDuringUpdateAfterInsert;
            this._continueUpdateOnError = from._continueUpdateOnError;
            this._returnProviderSpecificTypes = from._returnProviderSpecificTypes;
            this._acceptChangesDuringFill = from._acceptChangesDuringFill;
            this._fillLoadOption = from._fillLoadOption;
            this._missingMappingAction = from._missingMappingAction;
            this._missingSchemaAction = from._missingSchemaAction;
            if ((from._tableMappings != null) && (0 < from.TableMappings.Count))
            {
                DataTableMappingCollection tableMappings = this.TableMappings;
                foreach (object obj2 in from.TableMappings)
                {
                    tableMappings.Add((obj2 is ICloneable) ? ((ICloneable) obj2).Clone() : obj2);
                }
            }
        }

        [Obsolete("CloneInternals() has been deprecated.  Use the DataAdapter(DataAdapter from) constructor.  http://go.microsoft.com/fwlink/?linkid=14202"), PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        protected virtual DataAdapter CloneInternals()
        {
            DataAdapter adapter = (DataAdapter) Activator.CreateInstance(base.GetType(), BindingFlags.Public | BindingFlags.Instance, null, null, CultureInfo.InvariantCulture, null);
            adapter.CloneFrom(this);
            return adapter;
        }

        protected virtual DataTableMappingCollection CreateTableMappings()
        {
            Bid.Trace("<comm.DataAdapter.CreateTableMappings|API> %d#\n", this.ObjectID);
            return new DataTableMappingCollection();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._tableMappings = null;
            }
            base.Dispose(disposing);
        }

        public virtual int Fill(DataSet dataSet)
        {
            throw ADP.NotSupported();
        }

        protected virtual int Fill(DataTable dataTable, IDataReader dataReader)
        {
            DataTable[] dataTables = new DataTable[] { dataTable };
            return this.Fill(dataTables, dataReader, 0, 0);
        }

        protected virtual int Fill(DataTable[] dataTables, IDataReader dataReader, int startRecord, int maxRecords)
        {
            int num3;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<comm.DataAdapter.Fill|API> %d#, dataTables[], dataReader, startRecord, maxRecords\n", this.ObjectID);
            try
            {
                ADP.CheckArgumentLength(dataTables, "tables");
                if (((dataTables == null) || (dataTables.Length == 0)) || (dataTables[0] == null))
                {
                    throw ADP.FillRequires("dataTable");
                }
                if (dataReader == null)
                {
                    throw ADP.FillRequires("dataReader");
                }
                if ((1 < dataTables.Length) && ((startRecord != 0) || (maxRecords != 0)))
                {
                    throw ADP.NotSupported();
                }
                int num2 = 0;
                bool enforceConstraints = false;
                DataSet dataSet = dataTables[0].DataSet;
                try
                {
                    if (dataSet != null)
                    {
                        enforceConstraints = dataSet.EnforceConstraints;
                        dataSet.EnforceConstraints = false;
                    }
                    for (int i = 0; i < dataTables.Length; i++)
                    {
                        if (dataReader.IsClosed)
                        {
                            goto Label_00DE;
                        }
                        DataReaderContainer container = DataReaderContainer.Create(dataReader, this.ReturnProviderSpecificTypes);
                        if (container.FieldCount > 0)
                        {
                            if ((0 < i) && !this.FillNextResult(container))
                            {
                                goto Label_00DE;
                            }
                            int num4 = this.FillFromReader(null, dataTables[i], null, container, startRecord, maxRecords, null, null);
                            if (i == 0)
                            {
                                num2 = num4;
                            }
                        }
                    }
                }
                catch (ConstraintException)
                {
                    enforceConstraints = false;
                    throw;
                }
                finally
                {
                    if (enforceConstraints)
                    {
                        dataSet.EnforceConstraints = true;
                    }
                }
            Label_00DE:
                num3 = num2;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return num3;
        }

        protected virtual int Fill(DataSet dataSet, string srcTable, IDataReader dataReader, int startRecord, int maxRecords)
        {
            int num;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<comm.DataAdapter.Fill|API> %d#, dataSet, srcTable, dataReader, startRecord, maxRecords\n", this.ObjectID);
            try
            {
                if (dataSet == null)
                {
                    throw ADP.FillRequires("dataSet");
                }
                if (ADP.IsEmpty(srcTable))
                {
                    throw ADP.FillRequiresSourceTableName("srcTable");
                }
                if (dataReader == null)
                {
                    throw ADP.FillRequires("dataReader");
                }
                if (startRecord < 0)
                {
                    throw ADP.InvalidStartRecord("startRecord", startRecord);
                }
                if (maxRecords < 0)
                {
                    throw ADP.InvalidMaxRecords("maxRecords", maxRecords);
                }
                if (dataReader.IsClosed)
                {
                    return 0;
                }
                DataReaderContainer container = DataReaderContainer.Create(dataReader, this.ReturnProviderSpecificTypes);
                num = this.FillFromReader(dataSet, null, srcTable, container, startRecord, maxRecords, null, null);
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return num;
        }

        internal int FillFromReader(DataSet dataset, DataTable datatable, string srcTable, DataReaderContainer dataReader, int startRecord, int maxRecords, DataColumn parentChapterColumn, object parentChapterValue)
        {
            int num2 = 0;
            int schemaCount = 0;
            do
            {
                if (0 < dataReader.FieldCount)
                {
                    SchemaMapping mapping = this.FillMapping(dataset, datatable, srcTable, dataReader, schemaCount, parentChapterColumn, parentChapterValue);
                    schemaCount++;
                    if (((mapping != null) && (mapping.DataValues != null)) && (mapping.DataTable != null))
                    {
                        mapping.DataTable.BeginLoadData();
                        try
                        {
                            if ((1 == schemaCount) && ((0 < startRecord) || (0 < maxRecords)))
                            {
                                num2 = this.FillLoadDataRowChunk(mapping, startRecord, maxRecords);
                            }
                            else
                            {
                                int num3 = this.FillLoadDataRow(mapping);
                                if (1 == schemaCount)
                                {
                                    num2 = num3;
                                }
                            }
                        }
                        finally
                        {
                            mapping.DataTable.EndLoadData();
                        }
                        if (datatable != null)
                        {
                            return num2;
                        }
                    }
                }
            }
            while (this.FillNextResult(dataReader));
            return num2;
        }

        private int FillLoadDataRow(SchemaMapping mapping)
        {
            int num = 0;
            DataReaderContainer dataReader = mapping.DataReader;
            if (!this._hasFillErrorHandler)
            {
                while (dataReader.Read())
                {
                    mapping.LoadDataRow();
                    num++;
                }
                return num;
            }
            while (dataReader.Read())
            {
                try
                {
                    mapping.LoadDataRowWithClear();
                    num++;
                    continue;
                }
                catch (Exception exception)
                {
                    if (!ADP.IsCatchableExceptionType(exception))
                    {
                        throw;
                    }
                    ADP.TraceExceptionForCapture(exception);
                    this.OnFillErrorHandler(exception, mapping.DataTable, mapping.DataValues);
                    continue;
                }
            }
            return num;
        }

        private int FillLoadDataRowChunk(SchemaMapping mapping, int startRecord, int maxRecords)
        {
            DataReaderContainer dataReader = mapping.DataReader;
            while (0 < startRecord)
            {
                if (!dataReader.Read())
                {
                    return 0;
                }
                startRecord--;
            }
            int num = 0;
            if (0 >= maxRecords)
            {
                return this.FillLoadDataRow(mapping);
            }
            while ((num < maxRecords) && dataReader.Read())
            {
                if (this._hasFillErrorHandler)
                {
                    try
                    {
                        mapping.LoadDataRowWithClear();
                        num++;
                    }
                    catch (Exception exception)
                    {
                        if (!ADP.IsCatchableExceptionType(exception))
                        {
                            throw;
                        }
                        ADP.TraceExceptionForCapture(exception);
                        this.OnFillErrorHandler(exception, mapping.DataTable, mapping.DataValues);
                    }
                }
                else
                {
                    mapping.LoadDataRow();
                    num++;
                }
            }
            return num;
        }

        private SchemaMapping FillMapping(DataSet dataset, DataTable datatable, string srcTable, DataReaderContainer dataReader, int schemaCount, DataColumn parentChapterColumn, object parentChapterValue)
        {
            SchemaMapping mapping = null;
            if (this._hasFillErrorHandler)
            {
                try
                {
                    mapping = this.FillMappingInternal(dataset, datatable, srcTable, dataReader, schemaCount, parentChapterColumn, parentChapterValue);
                }
                catch (Exception exception)
                {
                    if (!ADP.IsCatchableExceptionType(exception))
                    {
                        throw;
                    }
                    ADP.TraceExceptionForCapture(exception);
                    this.OnFillErrorHandler(exception, null, null);
                }
                return mapping;
            }
            return this.FillMappingInternal(dataset, datatable, srcTable, dataReader, schemaCount, parentChapterColumn, parentChapterValue);
        }

        private SchemaMapping FillMappingInternal(DataSet dataset, DataTable datatable, string srcTable, DataReaderContainer dataReader, int schemaCount, DataColumn parentChapterColumn, object parentChapterValue)
        {
            bool keyInfo = System.Data.MissingSchemaAction.AddWithKey == this.MissingSchemaAction;
            string sourceTableName = null;
            if (dataset != null)
            {
                sourceTableName = GetSourceTableName(srcTable, schemaCount);
            }
            return new SchemaMapping(this, dataset, datatable, dataReader, keyInfo, SchemaType.Mapped, sourceTableName, true, parentChapterColumn, parentChapterValue);
        }

        private bool FillNextResult(DataReaderContainer dataReader)
        {
            bool flag = true;
            if (this._hasFillErrorHandler)
            {
                try
                {
                    flag = dataReader.NextResult();
                }
                catch (Exception exception)
                {
                    if (!ADP.IsCatchableExceptionType(exception))
                    {
                        throw;
                    }
                    ADP.TraceExceptionForCapture(exception);
                    this.OnFillErrorHandler(exception, null, null);
                }
                return flag;
            }
            return dataReader.NextResult();
        }

        public virtual DataTable[] FillSchema(DataSet dataSet, SchemaType schemaType)
        {
            throw ADP.NotSupported();
        }

        protected virtual DataTable FillSchema(DataTable dataTable, SchemaType schemaType, IDataReader dataReader)
        {
            DataTable table;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<comm.DataAdapter.FillSchema|API> %d#, dataTable, schemaType, dataReader\n", this.ObjectID);
            try
            {
                if (dataTable == null)
                {
                    throw ADP.ArgumentNull("dataTable");
                }
                if ((SchemaType.Source != schemaType) && (SchemaType.Mapped != schemaType))
                {
                    throw ADP.InvalidSchemaType(schemaType);
                }
                if ((dataReader == null) || dataReader.IsClosed)
                {
                    throw ADP.FillRequires("dataReader");
                }
                table = (DataTable) this.FillSchemaFromReader(null, dataTable, schemaType, null, dataReader);
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return table;
        }

        protected virtual DataTable[] FillSchema(DataSet dataSet, SchemaType schemaType, string srcTable, IDataReader dataReader)
        {
            DataTable[] tableArray;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<comm.DataAdapter.FillSchema|API> %d#, dataSet, schemaType=%d{ds.SchemaType}, srcTable, dataReader\n", this.ObjectID, (int) schemaType);
            try
            {
                if (dataSet == null)
                {
                    throw ADP.ArgumentNull("dataSet");
                }
                if ((SchemaType.Source != schemaType) && (SchemaType.Mapped != schemaType))
                {
                    throw ADP.InvalidSchemaType(schemaType);
                }
                if (ADP.IsEmpty(srcTable))
                {
                    throw ADP.FillSchemaRequiresSourceTableName("srcTable");
                }
                if ((dataReader == null) || dataReader.IsClosed)
                {
                    throw ADP.FillRequires("dataReader");
                }
                tableArray = (DataTable[]) this.FillSchemaFromReader(dataSet, null, schemaType, srcTable, dataReader);
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return tableArray;
        }

        internal object FillSchemaFromReader(DataSet dataset, DataTable datatable, SchemaType schemaType, string srcTable, IDataReader dataReader)
        {
            DataTable[] tables = null;
            int index = 0;
            do
            {
                DataReaderContainer container = DataReaderContainer.Create(dataReader, this.ReturnProviderSpecificTypes);
                if (0 < container.FieldCount)
                {
                    string sourceTableName = null;
                    if (dataset != null)
                    {
                        sourceTableName = GetSourceTableName(srcTable, index);
                        index++;
                    }
                    SchemaMapping mapping = new SchemaMapping(this, dataset, datatable, container, true, schemaType, sourceTableName, false, null, null);
                    if (datatable != null)
                    {
                        return mapping.DataTable;
                    }
                    if (mapping.DataTable != null)
                    {
                        if (tables == null)
                        {
                            tables = new DataTable[] { mapping.DataTable };
                        }
                        else
                        {
                            tables = AddDataTableToArray(tables, mapping.DataTable);
                        }
                    }
                }
            }
            while (dataReader.NextResult());
            object obj2 = tables;
            if ((obj2 == null) && (datatable == null))
            {
                obj2 = new DataTable[0];
            }
            return obj2;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public virtual IDataParameter[] GetFillParameters()
        {
            return new IDataParameter[0];
        }

        private static string GetSourceTableName(string srcTable, int index)
        {
            if (index == 0)
            {
                return srcTable;
            }
            return (srcTable + index.ToString(CultureInfo.InvariantCulture));
        }

        internal DataTableMapping GetTableMappingBySchemaAction(string sourceTableName, string dataSetTableName, System.Data.MissingMappingAction mappingAction)
        {
            return DataTableMappingCollection.GetTableMappingBySchemaAction(this._tableMappings, sourceTableName, dataSetTableName, mappingAction);
        }

        protected bool HasTableMappings()
        {
            return ((this._tableMappings != null) && (0 < this.TableMappings.Count));
        }

        internal int IndexOfDataSetTable(string dataSetTable)
        {
            if (this._tableMappings != null)
            {
                return this.TableMappings.IndexOfDataSetTable(dataSetTable);
            }
            return -1;
        }

        protected virtual void OnFillError(FillErrorEventArgs value)
        {
            FillErrorEventHandler handler = (FillErrorEventHandler) base.Events[EventFillError];
            if (handler != null)
            {
                handler(this, value);
            }
        }

        private void OnFillErrorHandler(Exception e, DataTable dataTable, object[] dataValues)
        {
            FillErrorEventArgs args = new FillErrorEventArgs(dataTable, dataValues) {
                Errors = e
            };
            this.OnFillError(args);
            if (!args.Continue)
            {
                if (args.Errors != null)
                {
                    throw args.Errors;
                }
                throw e;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void ResetFillLoadOption()
        {
            this._fillLoadOption = (LoadOption) 0;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual bool ShouldSerializeAcceptChangesDuringFill()
        {
            return (((LoadOption) 0) == this._fillLoadOption);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual bool ShouldSerializeFillLoadOption()
        {
            return (((LoadOption) 0) != this._fillLoadOption);
        }

        protected virtual bool ShouldSerializeTableMappings()
        {
            return true;
        }

        public virtual int Update(DataSet dataSet)
        {
            throw ADP.NotSupported();
        }

        [ResCategory("DataCategory_Fill"), ResDescription("DataAdapter_AcceptChangesDuringFill"), DefaultValue(true)]
        public bool AcceptChangesDuringFill
        {
            get
            {
                return this._acceptChangesDuringFill;
            }
            set
            {
                this._acceptChangesDuringFill = value;
            }
        }

        [ResCategory("DataCategory_Update"), DefaultValue(true), ResDescription("DataAdapter_AcceptChangesDuringUpdate")]
        public bool AcceptChangesDuringUpdate
        {
            get
            {
                return this._acceptChangesDuringUpdate;
            }
            set
            {
                this._acceptChangesDuringUpdate = value;
            }
        }

        [DefaultValue(false), ResCategory("DataCategory_Update"), ResDescription("DataAdapter_ContinueUpdateOnError")]
        public bool ContinueUpdateOnError
        {
            get
            {
                return this._continueUpdateOnError;
            }
            set
            {
                this._continueUpdateOnError = value;
            }
        }

        [ResDescription("DataAdapter_FillLoadOption"), ResCategory("DataCategory_Fill"), RefreshProperties(RefreshProperties.All)]
        public LoadOption FillLoadOption
        {
            get
            {
                if (this._fillLoadOption == ((LoadOption) 0))
                {
                    return LoadOption.OverwriteChanges;
                }
                return this._fillLoadOption;
            }
            set
            {
                switch (value)
                {
                    case ((LoadOption) 0):
                    case LoadOption.OverwriteChanges:
                    case LoadOption.PreserveChanges:
                    case LoadOption.Upsert:
                        this._fillLoadOption = value;
                        return;
                }
                throw ADP.InvalidLoadOption(value);
            }
        }

        [DefaultValue(1), ResCategory("DataCategory_Mapping"), ResDescription("DataAdapter_MissingMappingAction")]
        public System.Data.MissingMappingAction MissingMappingAction
        {
            get
            {
                return this._missingMappingAction;
            }
            set
            {
                switch (value)
                {
                    case System.Data.MissingMappingAction.Passthrough:
                    case System.Data.MissingMappingAction.Ignore:
                    case System.Data.MissingMappingAction.Error:
                        this._missingMappingAction = value;
                        return;
                }
                throw ADP.InvalidMissingMappingAction(value);
            }
        }

        [ResCategory("DataCategory_Mapping"), ResDescription("DataAdapter_MissingSchemaAction"), DefaultValue(1)]
        public System.Data.MissingSchemaAction MissingSchemaAction
        {
            get
            {
                return this._missingSchemaAction;
            }
            set
            {
                switch (value)
                {
                    case System.Data.MissingSchemaAction.Add:
                    case System.Data.MissingSchemaAction.Ignore:
                    case System.Data.MissingSchemaAction.Error:
                    case System.Data.MissingSchemaAction.AddWithKey:
                        this._missingSchemaAction = value;
                        return;
                }
                throw ADP.InvalidMissingSchemaAction(value);
            }
        }

        internal int ObjectID
        {
            get
            {
                return this._objectID;
            }
        }

        [ResDescription("DataAdapter_ReturnProviderSpecificTypes"), DefaultValue(false), ResCategory("DataCategory_Fill")]
        public virtual bool ReturnProviderSpecificTypes
        {
            get
            {
                return this._returnProviderSpecificTypes;
            }
            set
            {
                this._returnProviderSpecificTypes = value;
            }
        }

        ITableMappingCollection IDataAdapter.TableMappings
        {
            get
            {
                return this.TableMappings;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), ResDescription("DataAdapter_TableMappings"), ResCategory("DataCategory_Mapping")]
        public DataTableMappingCollection TableMappings
        {
            get
            {
                DataTableMappingCollection mappings = this._tableMappings;
                if (mappings == null)
                {
                    mappings = this.CreateTableMappings();
                    if (mappings == null)
                    {
                        mappings = new DataTableMappingCollection();
                    }
                    this._tableMappings = mappings;
                }
                return mappings;
            }
        }
    }
}

