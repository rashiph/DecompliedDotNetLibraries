namespace System.Data
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Configuration;
    using System.Data.Common;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [Serializable, DefaultProperty("DataSetName"), System.Data.ResDescription("DataSetDescr"), ToolboxItem("Microsoft.VSDesigner.Data.VS.DataSetToolboxItem, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), Designer("Microsoft.VSDesigner.Data.VS.DataSetDesigner, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), XmlSchemaProvider("GetDataSetSchema"), XmlRoot("DataSet")]
    public class DataSet : MarshalByValueComponent, IListSource, IXmlSerializable, ISupportInitializeNotification, ISupportInitialize, ISerializable
    {
        private bool _caseSensitive;
        private CultureInfo _culture;
        private bool _cultureUserSet;
        private string _datasetPrefix;
        private object _defaultViewManagerLock;
        private readonly int _objectID;
        private static int _objectTypeCount;
        private SerializationFormat _remotingFormat;
        private string dataSetName;
        private DataViewManager defaultViewManager;
        private bool enforceConstraints;
        internal PropertyCollection extendedProperties;
        private bool fBoundToDocument;
        internal bool fEnableCascading;
        internal bool fInitInProgress;
        internal bool fInLoadDiffgram;
        internal bool fInReadXml;
        internal bool fIsSchemaLoading;
        internal bool fTopLevelTable;
        private const string KEY_XMLDIFFGRAM = "XmlDiffGram";
        private const string KEY_XMLSCHEMA = "XmlSchema";
        internal string mainTableName;
        internal string namespaceURI;
        private readonly DataRelationCollection relationCollection;
        private static XmlSchemaComplexType schemaTypeForWSDL = null;
        private readonly DataTableCollection tableCollection;
        internal bool UdtIsWrapped;
        internal bool UseDataSetSchemaOnly;
        internal static readonly DataTable[] zeroTables = new DataTable[0];

        internal event DataSetClearEventhandler ClearFunctionCalled;

        internal event DataRowCreatedEventHandler DataRowCreated;

        [System.Data.ResDescription("DataSetInitializedDescr"), System.Data.ResCategory("DataCategory_Action")]
        public event EventHandler Initialized;

        [System.Data.ResCategory("DataCategory_Action"), System.Data.ResDescription("DataSetMergeFailedDescr")]
        public event MergeFailedEventHandler MergeFailed;

        internal event PropertyChangedEventHandler PropertyChanging;

        public DataSet()
        {
            this.dataSetName = "NewDataSet";
            this._datasetPrefix = string.Empty;
            this.namespaceURI = string.Empty;
            this.enforceConstraints = true;
            this.fEnableCascading = true;
            this.mainTableName = "";
            this._defaultViewManagerLock = new object();
            this._objectID = Interlocked.Increment(ref _objectTypeCount);
            GC.SuppressFinalize(this);
            Bid.Trace("<ds.DataSet.DataSet|API> %d#\n", this.ObjectID);
            this.tableCollection = new DataTableCollection(this);
            this.relationCollection = new DataRelationCollection.DataSetRelationCollection(this);
            this._culture = CultureInfo.CurrentCulture;
        }

        public DataSet(string dataSetName) : this()
        {
            this.DataSetName = dataSetName;
        }

        protected DataSet(SerializationInfo info, StreamingContext context) : this(info, context, true)
        {
        }

        protected DataSet(SerializationInfo info, StreamingContext context, bool ConstructSchema) : this()
        {
            SerializationFormat xml = SerializationFormat.Xml;
            System.Data.SchemaSerializationMode includeSchema = System.Data.SchemaSerializationMode.IncludeSchema;
            SerializationInfoEnumerator enumerator = info.GetEnumerator();
            while (enumerator.MoveNext())
            {
                string name = enumerator.Name;
                if (name != null)
                {
                    if (!(name == "DataSet.RemotingFormat"))
                    {
                        if (name == "SchemaSerializationMode.DataSet")
                        {
                            goto Label_0047;
                        }
                    }
                    else
                    {
                        xml = (SerializationFormat) enumerator.Value;
                    }
                }
                continue;
            Label_0047:
                includeSchema = (System.Data.SchemaSerializationMode) enumerator.Value;
            }
            if (includeSchema == System.Data.SchemaSerializationMode.ExcludeSchema)
            {
                this.InitializeDerivedDataSet();
            }
            if ((xml != SerializationFormat.Xml) || ConstructSchema)
            {
                this.DeserializeDataSet(info, context, xml, includeSchema);
            }
        }

        public void AcceptChanges()
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataSet.AcceptChanges|API> %d#\n", this.ObjectID);
            try
            {
                for (int i = 0; i < this.Tables.Count; i++)
                {
                    this.Tables[i].AcceptChanges();
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        public void BeginInit()
        {
            this.fInitInProgress = true;
        }

        public void Clear()
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataSet.Clear|API> %d#\n", this.ObjectID);
            try
            {
                this.OnClearFunctionCalled(null);
                bool enforceConstraints = this.EnforceConstraints;
                this.EnforceConstraints = false;
                for (int i = 0; i < this.Tables.Count; i++)
                {
                    this.Tables[i].Clear();
                }
                this.EnforceConstraints = enforceConstraints;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        public virtual DataSet Clone()
        {
            DataSet set2;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataSet.Clone|API> %d#\n", this.ObjectID);
            try
            {
                DataSet cloneDS = (DataSet) Activator.CreateInstance(base.GetType(), true);
                if (cloneDS.Tables.Count > 0)
                {
                    cloneDS.Reset();
                }
                cloneDS.DataSetName = this.DataSetName;
                cloneDS.CaseSensitive = this.CaseSensitive;
                cloneDS._culture = this._culture;
                cloneDS._cultureUserSet = this._cultureUserSet;
                cloneDS.EnforceConstraints = this.EnforceConstraints;
                cloneDS.Namespace = this.Namespace;
                cloneDS.Prefix = this.Prefix;
                cloneDS.RemotingFormat = this.RemotingFormat;
                cloneDS.fIsSchemaLoading = true;
                DataTableCollection tables = this.Tables;
                for (int i = 0; i < tables.Count; i++)
                {
                    DataTable table2 = tables[i].Clone(cloneDS);
                    table2.tableNamespace = tables[i].Namespace;
                    cloneDS.Tables.Add(table2);
                }
                for (int j = 0; j < tables.Count; j++)
                {
                    ConstraintCollection constraints = tables[j].Constraints;
                    for (int n = 0; n < constraints.Count; n++)
                    {
                        if (!(constraints[n] is UniqueConstraint))
                        {
                            ForeignKeyConstraint constraint = constraints[n] as ForeignKeyConstraint;
                            if (constraint.Table != constraint.RelatedTable)
                            {
                                cloneDS.Tables[j].Constraints.Add(constraints[n].Clone(cloneDS));
                            }
                        }
                    }
                }
                DataRelationCollection relations = this.Relations;
                for (int k = 0; k < relations.Count; k++)
                {
                    DataRelation relation = relations[k].Clone(cloneDS);
                    relation.CheckMultipleNested = false;
                    cloneDS.Relations.Add(relation);
                    relation.CheckMultipleNested = true;
                }
                if (this.extendedProperties != null)
                {
                    foreach (object obj2 in this.extendedProperties.Keys)
                    {
                        cloneDS.ExtendedProperties[obj2] = this.extendedProperties[obj2];
                    }
                }
                foreach (DataTable table in this.Tables)
                {
                    foreach (DataColumn column in table.Columns)
                    {
                        if (column.Expression.Length != 0)
                        {
                            cloneDS.Tables[table.TableName, table.Namespace].Columns[column.ColumnName].Expression = column.Expression;
                        }
                    }
                }
                for (int m = 0; m < tables.Count; m++)
                {
                    cloneDS.Tables[m].tableNamespace = tables[m].tableNamespace;
                }
                cloneDS.fIsSchemaLoading = false;
                set2 = cloneDS;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return set2;
        }

        public DataSet Copy()
        {
            DataSet set2;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataSet.Copy|API> %d#\n", this.ObjectID);
            try
            {
                DataSet set = this.Clone();
                bool enforceConstraints = set.EnforceConstraints;
                set.EnforceConstraints = false;
                foreach (DataTable table in this.Tables)
                {
                    DataTable table2 = set.Tables[table.TableName, table.Namespace];
                    foreach (DataRow row in table.Rows)
                    {
                        table.CopyRow(table2, row);
                    }
                }
                set.EnforceConstraints = enforceConstraints;
                set2 = set;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return set2;
        }

        public DataTableReader CreateDataReader()
        {
            if (this.Tables.Count == 0)
            {
                throw ExceptionBuilder.CannotCreateDataReaderOnEmptyDataSet();
            }
            DataTable[] dataTables = new DataTable[this.Tables.Count];
            for (int i = 0; i < this.Tables.Count; i++)
            {
                dataTables[i] = this.Tables[i];
            }
            return this.CreateDataReader(dataTables);
        }

        public DataTableReader CreateDataReader(params DataTable[] dataTables)
        {
            DataTableReader reader;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataSet.GetDataReader|API> %d#\n", this.ObjectID);
            try
            {
                if (dataTables.Length == 0)
                {
                    throw ExceptionBuilder.DataTableReaderArgumentIsEmpty();
                }
                for (int i = 0; i < dataTables.Length; i++)
                {
                    if (dataTables[i] == null)
                    {
                        throw ExceptionBuilder.ArgumentContainsNullValue();
                    }
                }
                reader = new DataTableReader(dataTables);
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return reader;
        }

        internal void DeserializeDataSet(SerializationInfo info, StreamingContext context, SerializationFormat remotingFormat, System.Data.SchemaSerializationMode schemaSerializationMode)
        {
            this.DeserializeDataSetSchema(info, context, remotingFormat, schemaSerializationMode);
            this.DeserializeDataSetData(info, context, remotingFormat);
        }

        private void DeserializeDataSetData(SerializationInfo info, StreamingContext context, SerializationFormat remotingFormat)
        {
            if (remotingFormat != SerializationFormat.Xml)
            {
                for (int i = 0; i < this.Tables.Count; i++)
                {
                    this.Tables[i].DeserializeTableData(info, context, i);
                }
            }
            else
            {
                string s = (string) info.GetValue("XmlDiffGram", typeof(string));
                if (s != null)
                {
                    this.ReadXml(new XmlTextReader(new StringReader(s)), XmlReadMode.DiffGram);
                }
            }
        }

        private void DeserializeDataSetProperties(SerializationInfo info, StreamingContext context)
        {
            this.dataSetName = info.GetString("DataSet.DataSetName");
            this.namespaceURI = info.GetString("DataSet.Namespace");
            this._datasetPrefix = info.GetString("DataSet.Prefix");
            this._caseSensitive = info.GetBoolean("DataSet.CaseSensitive");
            int culture = (int) info.GetValue("DataSet.LocaleLCID", typeof(int));
            this._culture = new CultureInfo(culture);
            this._cultureUserSet = true;
            this.enforceConstraints = info.GetBoolean("DataSet.EnforceConstraints");
            this.extendedProperties = (PropertyCollection) info.GetValue("DataSet.ExtendedProperties", typeof(PropertyCollection));
        }

        private void DeserializeDataSetSchema(SerializationInfo info, StreamingContext context, SerializationFormat remotingFormat, System.Data.SchemaSerializationMode schemaSerializationMode)
        {
            if (remotingFormat != SerializationFormat.Xml)
            {
                if (schemaSerializationMode == System.Data.SchemaSerializationMode.IncludeSchema)
                {
                    this.DeserializeDataSetProperties(info, context);
                    int num4 = info.GetInt32("DataSet.Tables.Count");
                    for (int i = 0; i < num4; i++)
                    {
                        byte[] buffer = (byte[]) info.GetValue(string.Format(CultureInfo.InvariantCulture, "DataSet.Tables_{0}", new object[] { i }), typeof(byte[]));
                        MemoryStream serializationStream = new MemoryStream(buffer) {
                            Position = 0L
                        };
                        BinaryFormatter formatter = new BinaryFormatter(null, new StreamingContext(context.State, false));
                        DataTable table = (DataTable) formatter.Deserialize(serializationStream);
                        this.Tables.Add(table);
                    }
                    for (int j = 0; j < num4; j++)
                    {
                        this.Tables[j].DeserializeConstraints(info, context, j, true);
                    }
                    this.DeserializeRelations(info, context);
                    for (int k = 0; k < num4; k++)
                    {
                        this.Tables[k].DeserializeExpressionColumns(info, context, k);
                    }
                }
                else
                {
                    this.DeserializeDataSetProperties(info, context);
                }
            }
            else
            {
                string s = (string) info.GetValue("XmlSchema", typeof(string));
                if (s != null)
                {
                    this.ReadXmlSchema(new XmlTextReader(new StringReader(s)), true);
                }
            }
        }

        private void DeserializeRelations(SerializationInfo info, StreamingContext context)
        {
            ArrayList list2 = (ArrayList) info.GetValue("DataSet.Relations", typeof(ArrayList));
            foreach (ArrayList list in list2)
            {
                string relationName = (string) list[0];
                int[] numArray2 = (int[]) list[1];
                int[] numArray = (int[]) list[2];
                bool flag = (bool) list[3];
                PropertyCollection propertys = (PropertyCollection) list[4];
                DataColumn[] parentColumns = new DataColumn[numArray2.Length - 1];
                for (int i = 0; i < parentColumns.Length; i++)
                {
                    parentColumns[i] = this.Tables[numArray2[0]].Columns[numArray2[i + 1]];
                }
                DataColumn[] childColumns = new DataColumn[numArray.Length - 1];
                for (int j = 0; j < childColumns.Length; j++)
                {
                    childColumns[j] = this.Tables[numArray[0]].Columns[numArray[j + 1]];
                }
                DataRelation relation = new DataRelation(relationName, parentColumns, childColumns, false) {
                    CheckMultipleNested = false,
                    Nested = flag,
                    extendedProperties = propertys
                };
                this.Relations.Add(relation);
                relation.CheckMultipleNested = true;
            }
        }

        protected System.Data.SchemaSerializationMode DetermineSchemaSerializationMode(XmlReader reader)
        {
            System.Data.SchemaSerializationMode includeSchema = System.Data.SchemaSerializationMode.IncludeSchema;
            reader.MoveToContent();
            if ((reader.NodeType == XmlNodeType.Element) && reader.HasAttributes)
            {
                string attribute = reader.GetAttribute("SchemaSerializationMode", "urn:schemas-microsoft-com:xml-msdata");
                if (string.Compare(attribute, "ExcludeSchema", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return System.Data.SchemaSerializationMode.ExcludeSchema;
                }
                if (string.Compare(attribute, "IncludeSchema", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return System.Data.SchemaSerializationMode.IncludeSchema;
                }
                if (attribute != null)
                {
                    throw ExceptionBuilder.InvalidSchemaSerializationMode(typeof(System.Data.SchemaSerializationMode), attribute);
                }
            }
            return includeSchema;
        }

        protected System.Data.SchemaSerializationMode DetermineSchemaSerializationMode(SerializationInfo info, StreamingContext context)
        {
            System.Data.SchemaSerializationMode includeSchema = System.Data.SchemaSerializationMode.IncludeSchema;
            SerializationInfoEnumerator enumerator = info.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Name == "SchemaSerializationMode.DataSet")
                {
                    return (System.Data.SchemaSerializationMode) enumerator.Value;
                }
            }
            return includeSchema;
        }

        internal void EnableConstraints()
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataSet.EnableConstraints|INFO> %d#\n", this.ObjectID);
            try
            {
                bool flag = false;
                ConstraintEnumerator enumerator3 = new ConstraintEnumerator(this);
                while (enumerator3.GetNext())
                {
                    Constraint constraint = enumerator3.GetConstraint();
                    flag |= constraint.IsConstraintViolated();
                }
                foreach (DataTable table in this.Tables)
                {
                    foreach (DataColumn column in table.Columns)
                    {
                        if (!column.AllowDBNull)
                        {
                            flag |= column.IsNotAllowDBNullViolated();
                        }
                        if (column.MaxLength >= 0)
                        {
                            flag |= column.IsMaxLengthViolated();
                        }
                    }
                }
                if (flag)
                {
                    this.FailedEnableConstraints();
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        public void EndInit()
        {
            this.Tables.FinishInitCollection();
            for (int i = 0; i < this.Tables.Count; i++)
            {
                this.Tables[i].Columns.FinishInitCollection();
            }
            for (int j = 0; j < this.Tables.Count; j++)
            {
                this.Tables[j].Constraints.FinishInitConstraints();
            }
            ((DataRelationCollection.DataSetRelationCollection) this.Relations).FinishInitRelations();
            this.fInitInProgress = false;
            this.OnInitialized();
        }

        internal int EstimatedXmlStringSize()
        {
            int num4 = 100;
            for (int i = 0; i < this.Tables.Count; i++)
            {
                int num = (this.Tables[i].TableName.Length + 4) << 2;
                DataTable table = this.Tables[i];
                for (int j = 0; j < table.Columns.Count; j++)
                {
                    num += (table.Columns[j].ColumnName.Length + 4) << 2;
                    num += 20;
                }
                num4 += table.Rows.Count * num;
            }
            return num4;
        }

        internal void FailedEnableConstraints()
        {
            this.EnforceConstraints = false;
            throw ExceptionBuilder.EnforceConstraint();
        }

        internal DataTable FindTable(DataTable baseTable, PropertyDescriptor[] props, int propStart)
        {
            if (props.Length < (propStart + 1))
            {
                return baseTable;
            }
            PropertyDescriptor descriptor = props[propStart];
            if (baseTable == null)
            {
                if (descriptor is DataTablePropertyDescriptor)
                {
                    return this.FindTable(((DataTablePropertyDescriptor) descriptor).Table, props, propStart + 1);
                }
                return null;
            }
            if (descriptor is DataRelationPropertyDescriptor)
            {
                return this.FindTable(((DataRelationPropertyDescriptor) descriptor).Relation.ChildTable, props, propStart + 1);
            }
            return null;
        }

        public DataSet GetChanges()
        {
            return this.GetChanges(DataRowState.Modified | DataRowState.Deleted | DataRowState.Added);
        }

        public DataSet GetChanges(DataRowState rowStates)
        {
            DataSet set2;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataSet.GetChanges|API> %d#, rowStates=%d{ds.DataRowState}\n", this.ObjectID, (int) rowStates);
            try
            {
                DataSet set = null;
                bool enforceConstraints = false;
                if ((rowStates & ~(DataRowState.Modified | DataRowState.Deleted | DataRowState.Added | DataRowState.Unchanged)) != 0)
                {
                    throw ExceptionBuilder.InvalidRowState(rowStates);
                }
                TableChanges[] bitMatrix = new TableChanges[this.Tables.Count];
                for (int i = 0; i < bitMatrix.Length; i++)
                {
                    bitMatrix[i] = new TableChanges(this.Tables[i].Rows.Count);
                }
                this.MarkModifiedRows(bitMatrix, rowStates);
                for (int j = 0; j < bitMatrix.Length; j++)
                {
                    if (0 < bitMatrix[j].HasChanges)
                    {
                        if (set == null)
                        {
                            set = this.Clone();
                            enforceConstraints = set.EnforceConstraints;
                            set.EnforceConstraints = false;
                        }
                        DataTable table = this.Tables[j];
                        DataTable table2 = set.Tables[table.TableName, table.Namespace];
                        for (int k = 0; 0 < bitMatrix[j].HasChanges; k++)
                        {
                            if (bitMatrix[j][k])
                            {
                                table.CopyRow(table2, table.Rows[k]);
                                bitMatrix[j].HasChanges--;
                            }
                        }
                    }
                }
                if (set != null)
                {
                    set.EnforceConstraints = enforceConstraints;
                }
                set2 = set;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return set2;
        }

        public static XmlSchemaComplexType GetDataSetSchema(XmlSchemaSet schemaSet)
        {
            if (schemaTypeForWSDL == null)
            {
                XmlSchemaComplexType type = new XmlSchemaComplexType();
                XmlSchemaSequence sequence = new XmlSchemaSequence();
                if (PublishLegacyWSDL())
                {
                    XmlSchemaElement item = new XmlSchemaElement {
                        RefName = new XmlQualifiedName("schema", "http://www.w3.org/2001/XMLSchema")
                    };
                    sequence.Items.Add(item);
                    XmlSchemaAny any2 = new XmlSchemaAny();
                    sequence.Items.Add(any2);
                }
                else
                {
                    XmlSchemaAny any = new XmlSchemaAny {
                        Namespace = "http://www.w3.org/2001/XMLSchema",
                        MinOccurs = 0M,
                        ProcessContents = XmlSchemaContentProcessing.Lax
                    };
                    sequence.Items.Add(any);
                    any = new XmlSchemaAny {
                        Namespace = "urn:schemas-microsoft-com:xml-diffgram-v1",
                        MinOccurs = 0M,
                        ProcessContents = XmlSchemaContentProcessing.Lax
                    };
                    sequence.Items.Add(any);
                    sequence.MaxOccurs = 79228162514264337593543950335M;
                }
                type.Particle = sequence;
                schemaTypeForWSDL = type;
            }
            return schemaTypeForWSDL;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            SerializationFormat remotingFormat = this.RemotingFormat;
            this.SerializeDataSet(info, context, remotingFormat);
        }

        internal DataRelationCollection GetParentRelations(DataTable table)
        {
            return table.ParentRelations;
        }

        internal string GetRemotingDiffGram(DataTable table)
        {
            StringWriter w = new StringWriter(CultureInfo.InvariantCulture);
            XmlTextWriter xmlw = new XmlTextWriter(w) {
                Formatting = Formatting.Indented
            };
            if (w != null)
            {
                new NewDiffgramGen(table, false).Save(xmlw, table);
            }
            return w.ToString();
        }

        protected virtual XmlSchema GetSchemaSerializable()
        {
            return null;
        }

        protected void GetSerializationData(SerializationInfo info, StreamingContext context)
        {
            SerializationFormat xml = SerializationFormat.Xml;
            SerializationInfoEnumerator enumerator = info.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Name == "DataSet.RemotingFormat")
                {
                    xml = (SerializationFormat) enumerator.Value;
                    break;
                }
            }
            this.DeserializeDataSetData(info, context, xml);
        }

        public string GetXml()
        {
            string str;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataSet.GetXml|API> %d#\n", this.ObjectID);
            try
            {
                StringWriter w = new StringWriter(CultureInfo.InvariantCulture);
                if (w != null)
                {
                    XmlTextWriter xw = new XmlTextWriter(w) {
                        Formatting = Formatting.Indented
                    };
                    new XmlDataTreeWriter(this).Save(xw, false);
                }
                str = w.ToString();
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return str;
        }

        public string GetXmlSchema()
        {
            string str;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataSet.GetXmlSchema|API> %d#\n", this.ObjectID);
            try
            {
                StringWriter w = new StringWriter(CultureInfo.InvariantCulture);
                XmlTextWriter xw = new XmlTextWriter(w) {
                    Formatting = Formatting.Indented
                };
                if (w != null)
                {
                    new XmlTreeGen(SchemaFormat.Public).Save(this, xw);
                }
                str = w.ToString();
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return str;
        }

        internal string GetXmlSchemaForRemoting(DataTable table)
        {
            StringWriter w = new StringWriter(CultureInfo.InvariantCulture);
            XmlTextWriter xw = new XmlTextWriter(w) {
                Formatting = Formatting.Indented
            };
            if (w != null)
            {
                if (table == null)
                {
                    if (this.SchemaSerializationMode == System.Data.SchemaSerializationMode.ExcludeSchema)
                    {
                        new XmlTreeGen(SchemaFormat.RemotingSkipSchema).Save(this, xw);
                    }
                    else
                    {
                        new XmlTreeGen(SchemaFormat.Remoting).Save(this, xw);
                    }
                }
                else
                {
                    new XmlTreeGen(SchemaFormat.Remoting).Save(table, xw);
                }
            }
            return w.ToString();
        }

        public bool HasChanges()
        {
            return this.HasChanges(DataRowState.Modified | DataRowState.Deleted | DataRowState.Added);
        }

        public bool HasChanges(DataRowState rowStates)
        {
            bool flag;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataSet.HasChanges|API> %d#, rowStates=%d{ds.DataRowState}\n", this.ObjectID, (int) rowStates);
            try
            {
                if ((rowStates & ~(DataRowState.Modified | DataRowState.Deleted | DataRowState.Added | DataRowState.Unchanged | DataRowState.Detached)) != 0)
                {
                    throw ExceptionBuilder.ArgumentOutOfRange("rowState");
                }
                for (int i = 0; i < this.Tables.Count; i++)
                {
                    DataTable table = this.Tables[i];
                    for (int j = 0; j < table.Rows.Count; j++)
                    {
                        DataRow row = table.Rows[j];
                        if ((row.RowState & rowStates) != 0)
                        {
                            return true;
                        }
                    }
                }
                flag = false;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return flag;
        }

        internal void InferSchema(XmlDocument xdoc, string[] excludedNamespaces, XmlReadMode mode)
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataSet.InferSchema|INFO> %d#, mode=%d{ds.XmlReadMode}\n", this.ObjectID, (int) mode);
            try
            {
                string namespaceURI = xdoc.DocumentElement.NamespaceURI;
                if (excludedNamespaces == null)
                {
                    excludedNamespaces = new string[0];
                }
                XmlNodeReader instanceDocument = new XmlIgnoreNamespaceReader(xdoc, excludedNamespaces);
                XmlSchemaInference inference = new XmlSchemaInference {
                    Occurrence = XmlSchemaInference.InferenceOption.Relaxed
                };
                if (mode == XmlReadMode.InferTypedSchema)
                {
                    inference.TypeInference = XmlSchemaInference.InferenceOption.Restricted;
                }
                else
                {
                    inference.TypeInference = XmlSchemaInference.InferenceOption.Relaxed;
                }
                XmlSchemaSet schemaSet = inference.InferSchema(instanceDocument);
                schemaSet.Compile();
                XSDSchema schema = new XSDSchema {
                    FromInference = true
                };
                try
                {
                    schema.LoadSchema(schemaSet, this);
                }
                finally
                {
                    schema.FromInference = false;
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        public void InferXmlSchema(Stream stream, string[] nsArray)
        {
            if (stream != null)
            {
                this.InferXmlSchema(new XmlTextReader(stream), nsArray);
            }
        }

        public void InferXmlSchema(TextReader reader, string[] nsArray)
        {
            if (reader != null)
            {
                this.InferXmlSchema(new XmlTextReader(reader), nsArray);
            }
        }

        public void InferXmlSchema(string fileName, string[] nsArray)
        {
            XmlTextReader reader = new XmlTextReader(fileName);
            try
            {
                this.InferXmlSchema(reader, nsArray);
            }
            finally
            {
                reader.Close();
            }
        }

        public void InferXmlSchema(XmlReader reader, string[] nsArray)
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataSet.InferXmlSchema|API> %d#\n", this.ObjectID);
            try
            {
                if (reader != null)
                {
                    XmlDocument xdoc = new XmlDocument();
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        XmlNode newChild = xdoc.ReadNode(reader);
                        xdoc.AppendChild(newChild);
                    }
                    else
                    {
                        xdoc.Load(reader);
                    }
                    if (xdoc.DocumentElement != null)
                    {
                        this.InferSchema(xdoc, nsArray, XmlReadMode.InferSchema);
                    }
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        protected virtual void InitializeDerivedDataSet()
        {
        }

        protected bool IsBinarySerialized(SerializationInfo info, StreamingContext context)
        {
            SerializationFormat xml = SerializationFormat.Xml;
            SerializationInfoEnumerator enumerator = info.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Name == "DataSet.RemotingFormat")
                {
                    xml = (SerializationFormat) enumerator.Value;
                    break;
                }
            }
            return (xml == SerializationFormat.Binary);
        }

        private bool IsEmpty()
        {
            foreach (DataTable table in this.Tables)
            {
                if (table.Rows.Count > 0)
                {
                    return false;
                }
            }
            return true;
        }

        public void Load(IDataReader reader, LoadOption loadOption, params DataTable[] tables)
        {
            this.Load(reader, loadOption, null, tables);
        }

        public void Load(IDataReader reader, LoadOption loadOption, params string[] tables)
        {
            ADP.CheckArgumentNull(tables, "tables");
            DataTable[] tableArray = new DataTable[tables.Length];
            for (int i = 0; i < tables.Length; i++)
            {
                DataTable table = this.Tables[tables[i]];
                if (table == null)
                {
                    table = new DataTable(tables[i]);
                    this.Tables.Add(table);
                }
                tableArray[i] = table;
            }
            this.Load(reader, loadOption, null, tableArray);
        }

        public virtual void Load(IDataReader reader, LoadOption loadOption, FillErrorEventHandler errorHandler, params DataTable[] tables)
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataSet.Load|API> reader, loadOption=%d{ds.LoadOption}", (int) loadOption);
            try
            {
                foreach (DataTable table in tables)
                {
                    ADP.CheckArgumentNull(table, "tables");
                    if (table.DataSet != this)
                    {
                        throw ExceptionBuilder.TableNotInTheDataSet(table.TableName);
                    }
                }
                LoadAdapter adapter = new LoadAdapter {
                    FillLoadOption = loadOption,
                    MissingSchemaAction = MissingSchemaAction.AddWithKey
                };
                if (errorHandler != null)
                {
                    adapter.FillError += errorHandler;
                }
                adapter.FillFromReader(tables, reader, 0, 0);
                if (!reader.IsClosed && !reader.NextResult())
                {
                    reader.Close();
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        private void MarkModifiedRows(TableChanges[] bitMatrix, DataRowState rowStates)
        {
            for (int i = 0; i < bitMatrix.Length; i++)
            {
                DataRowCollection rows = this.Tables[i].Rows;
                int count = rows.Count;
                for (int j = 0; j < count; j++)
                {
                    DataRow row = rows[j];
                    DataRowState rowState = row.RowState;
                    if (((rowStates & rowState) != 0) && !bitMatrix[i][j])
                    {
                        bitMatrix[i][j] = true;
                        if (DataRowState.Deleted != rowState)
                        {
                            this.MarkRelatedRowsAsModified(bitMatrix, row);
                        }
                    }
                }
            }
        }

        private void MarkRelatedRowsAsModified(TableChanges[] bitMatrix, DataRow row)
        {
            DataRelationCollection parentRelations = row.Table.ParentRelations;
            int count = parentRelations.Count;
            for (int i = 0; i < count; i++)
            {
                foreach (DataRow row2 in row.GetParentRows(parentRelations[i], DataRowVersion.Current))
                {
                    int index = this.Tables.IndexOf(row2.Table);
                    int num3 = row2.Table.Rows.IndexOf(row2);
                    if (!bitMatrix[index][num3])
                    {
                        bitMatrix[index][num3] = true;
                        if (DataRowState.Deleted != row2.RowState)
                        {
                            this.MarkRelatedRowsAsModified(bitMatrix, row2);
                        }
                    }
                }
            }
        }

        public void Merge(DataSet dataSet)
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataSet.Merge|API> %d#, dataSet=%d\n", this.ObjectID, (dataSet != null) ? dataSet.ObjectID : 0);
            try
            {
                this.Merge(dataSet, false, MissingSchemaAction.Add);
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        public void Merge(DataTable table)
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataSet.Merge|API> %d#, table=%d\n", this.ObjectID, (table != null) ? table.ObjectID : 0);
            try
            {
                this.Merge(table, false, MissingSchemaAction.Add);
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        public void Merge(DataRow[] rows)
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataSet.Merge|API> %d#, rows\n", this.ObjectID);
            try
            {
                this.Merge(rows, false, MissingSchemaAction.Add);
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        public void Merge(DataSet dataSet, bool preserveChanges)
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataSet.Merge|API> %d#, dataSet=%d, preserveChanges=%d{bool}\n", this.ObjectID, (dataSet != null) ? dataSet.ObjectID : 0, preserveChanges);
            try
            {
                this.Merge(dataSet, preserveChanges, MissingSchemaAction.Add);
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        public void Merge(DataSet dataSet, bool preserveChanges, MissingSchemaAction missingSchemaAction)
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataSet.Merge|API> %d#, dataSet=%d, preserveChanges=%d{bool}, missingSchemaAction=%d{ds.MissingSchemaAction}\n", this.ObjectID, (dataSet != null) ? dataSet.ObjectID : 0, preserveChanges, (int) missingSchemaAction);
            try
            {
                if (dataSet == null)
                {
                    throw ExceptionBuilder.ArgumentNull("dataSet");
                }
                switch (missingSchemaAction)
                {
                    case MissingSchemaAction.Add:
                    case MissingSchemaAction.Ignore:
                    case MissingSchemaAction.Error:
                    case MissingSchemaAction.AddWithKey:
                        new Merger(this, preserveChanges, missingSchemaAction).MergeDataSet(dataSet);
                        return;
                }
                throw ADP.InvalidMissingSchemaAction(missingSchemaAction);
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        public void Merge(DataTable table, bool preserveChanges, MissingSchemaAction missingSchemaAction)
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataSet.Merge|API> %d#, table=%d, preserveChanges=%d{bool}, missingSchemaAction=%d{ds.MissingSchemaAction}\n", this.ObjectID, (table != null) ? table.ObjectID : 0, preserveChanges, (int) missingSchemaAction);
            try
            {
                if (table == null)
                {
                    throw ExceptionBuilder.ArgumentNull("table");
                }
                switch (missingSchemaAction)
                {
                    case MissingSchemaAction.Add:
                    case MissingSchemaAction.Ignore:
                    case MissingSchemaAction.Error:
                    case MissingSchemaAction.AddWithKey:
                        new Merger(this, preserveChanges, missingSchemaAction).MergeTable(table);
                        return;
                }
                throw ADP.InvalidMissingSchemaAction(missingSchemaAction);
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        public void Merge(DataRow[] rows, bool preserveChanges, MissingSchemaAction missingSchemaAction)
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataSet.Merge|API> %d#, preserveChanges=%d{bool}, missingSchemaAction=%d{ds.MissingSchemaAction}\n", this.ObjectID, preserveChanges, (int) missingSchemaAction);
            try
            {
                if (rows == null)
                {
                    throw ExceptionBuilder.ArgumentNull("rows");
                }
                switch (missingSchemaAction)
                {
                    case MissingSchemaAction.Add:
                    case MissingSchemaAction.Ignore:
                    case MissingSchemaAction.Error:
                    case MissingSchemaAction.AddWithKey:
                        new Merger(this, preserveChanges, missingSchemaAction).MergeRows(rows);
                        return;
                }
                throw ADP.InvalidMissingSchemaAction(missingSchemaAction);
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        private static void MoveToElement(XmlReader reader)
        {
            while ((!reader.EOF && (reader.NodeType != XmlNodeType.EndElement)) && (reader.NodeType != XmlNodeType.Element))
            {
                reader.Read();
            }
        }

        internal bool MoveToElement(XmlReader reader, int depth)
        {
            while ((!reader.EOF && (reader.NodeType != XmlNodeType.EndElement)) && ((reader.NodeType != XmlNodeType.Element) && (reader.Depth > depth)))
            {
                reader.Read();
            }
            return (reader.NodeType == XmlNodeType.Element);
        }

        internal void OnClearFunctionCalled(DataTable table)
        {
            if (this.onClearFunctionCalled != null)
            {
                this.onClearFunctionCalled(this, table);
            }
        }

        internal void OnDataRowCreated(DataRow row)
        {
            if (this.onDataRowCreated != null)
            {
                this.onDataRowCreated(this, row);
            }
        }

        private void OnInitialized()
        {
            if (this.onInitialized != null)
            {
                this.onInitialized(this, EventArgs.Empty);
            }
        }

        internal void OnMergeFailed(MergeFailedEventArgs mfevent)
        {
            if (this.onMergeFailed == null)
            {
                throw ExceptionBuilder.MergeFailed(mfevent.Conflict);
            }
            this.onMergeFailed(this, mfevent);
        }

        protected virtual void OnPropertyChanging(PropertyChangedEventArgs pcevent)
        {
            if (this.onPropertyChangingDelegate != null)
            {
                this.onPropertyChangingDelegate(this, pcevent);
            }
        }

        internal void OnRemovedTable(DataTable table)
        {
            DataViewManager defaultViewManager = this.defaultViewManager;
            if (defaultViewManager != null)
            {
                defaultViewManager.DataViewSettings.Remove(table);
            }
        }

        protected virtual void OnRemoveRelation(DataRelation relation)
        {
        }

        internal void OnRemoveRelationHack(DataRelation relation)
        {
            this.OnRemoveRelation(relation);
        }

        protected internal virtual void OnRemoveTable(DataTable table)
        {
        }

        private static bool PublishLegacyWSDL()
        {
            float num = 1f;
            NameValueCollection section = (NameValueCollection) System.Configuration.PrivilegedConfigurationManager.GetSection("system.data.dataset");
            if (section != null)
            {
                string[] values = section.GetValues("WSDL_VERSION");
                if (((values != null) && (0 < values.Length)) && (values[0] != null))
                {
                    num = float.Parse(values[0], CultureInfo.InvariantCulture);
                }
            }
            return (num < 2f);
        }

        internal void RaiseMergeFailed(DataTable table, string conflict, MissingSchemaAction missingSchemaAction)
        {
            if (MissingSchemaAction.Error == missingSchemaAction)
            {
                throw ExceptionBuilder.MergeFailed(conflict);
            }
            MergeFailedEventArgs mfevent = new MergeFailedEventArgs(table, conflict);
            this.OnMergeFailed(mfevent);
        }

        protected internal void RaisePropertyChanging(string name)
        {
            this.OnPropertyChanging(new PropertyChangedEventArgs(name));
        }

        internal void ReadEndElement(XmlReader reader)
        {
            while (reader.NodeType == XmlNodeType.Whitespace)
            {
                reader.Skip();
            }
            if (reader.NodeType == XmlNodeType.None)
            {
                reader.Skip();
            }
            else if (reader.NodeType == XmlNodeType.EndElement)
            {
                reader.ReadEndElement();
            }
        }

        internal void ReadXDRSchema(XmlReader reader)
        {
            XmlDocument document = new XmlDocument();
            XmlNode newChild = document.ReadNode(reader);
            document.AppendChild(newChild);
            XDRSchema schema = new XDRSchema(this, false);
            this.DataSetName = document.DocumentElement.LocalName;
            schema.LoadSchema((XmlElement) newChild, this);
        }

        public XmlReadMode ReadXml(Stream stream)
        {
            if (stream == null)
            {
                return XmlReadMode.Auto;
            }
            return this.ReadXml(new XmlTextReader(stream), false);
        }

        public XmlReadMode ReadXml(TextReader reader)
        {
            if (reader == null)
            {
                return XmlReadMode.Auto;
            }
            return this.ReadXml(new XmlTextReader(reader), false);
        }

        public XmlReadMode ReadXml(string fileName)
        {
            XmlReadMode mode;
            XmlTextReader reader = new XmlTextReader(fileName);
            try
            {
                mode = this.ReadXml(reader, false);
            }
            finally
            {
                reader.Close();
            }
            return mode;
        }

        public XmlReadMode ReadXml(XmlReader reader)
        {
            return this.ReadXml(reader, false);
        }

        public XmlReadMode ReadXml(Stream stream, XmlReadMode mode)
        {
            if (stream == null)
            {
                return XmlReadMode.Auto;
            }
            XmlTextReader reader = (mode == XmlReadMode.Fragment) ? new XmlTextReader(stream, XmlNodeType.Element, null) : new XmlTextReader(stream);
            return this.ReadXml(reader, mode, false);
        }

        public XmlReadMode ReadXml(TextReader reader, XmlReadMode mode)
        {
            if (reader == null)
            {
                return XmlReadMode.Auto;
            }
            XmlTextReader reader2 = (mode == XmlReadMode.Fragment) ? new XmlTextReader(reader.ReadToEnd(), XmlNodeType.Element, null) : new XmlTextReader(reader);
            return this.ReadXml(reader2, mode, false);
        }

        public XmlReadMode ReadXml(string fileName, XmlReadMode mode)
        {
            XmlTextReader reader = null;
            XmlReadMode mode2;
            if (mode == XmlReadMode.Fragment)
            {
                FileStream xmlFragment = new FileStream(fileName, FileMode.Open);
                reader = new XmlTextReader(xmlFragment, XmlNodeType.Element, null);
            }
            else
            {
                reader = new XmlTextReader(fileName);
            }
            try
            {
                mode2 = this.ReadXml(reader, mode, false);
            }
            finally
            {
                reader.Close();
            }
            return mode2;
        }

        internal XmlReadMode ReadXml(XmlReader reader, bool denyResolving)
        {
            XmlReadMode mode2;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataSet.ReadXml|INFO> %d#, denyResolving=%d{bool}\n", this.ObjectID, denyResolving);
            try
            {
                try
                {
                    bool flag7 = false;
                    bool flag = false;
                    bool flag6 = false;
                    bool isXdr = false;
                    int depth = -1;
                    XmlReadMode auto = XmlReadMode.Auto;
                    bool flag2 = false;
                    bool flag5 = false;
                    for (int i = 0; i < this.Tables.Count; i++)
                    {
                        this.Tables[i].rowDiffId = null;
                    }
                    if (reader != null)
                    {
                        if (this.Tables.Count == 0)
                        {
                            flag2 = true;
                        }
                        if (reader is XmlTextReader)
                        {
                            ((XmlTextReader) reader).WhitespaceHandling = WhitespaceHandling.Significant;
                        }
                        XmlDocument xdoc = new XmlDocument();
                        XmlDataLoader loader = null;
                        reader.MoveToContent();
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            depth = reader.Depth;
                        }
                        if (reader.NodeType != XmlNodeType.Element)
                        {
                            return auto;
                        }
                        if ((reader.LocalName == "diffgram") && (reader.NamespaceURI == "urn:schemas-microsoft-com:xml-diffgram-v1"))
                        {
                            this.ReadXmlDiffgram(reader);
                            this.ReadEndElement(reader);
                            return XmlReadMode.DiffGram;
                        }
                        if ((reader.LocalName == "Schema") && (reader.NamespaceURI == "urn:schemas-microsoft-com:xml-data"))
                        {
                            this.ReadXDRSchema(reader);
                            return XmlReadMode.ReadSchema;
                        }
                        if ((reader.LocalName == "schema") && (reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema"))
                        {
                            this.ReadXSDSchema(reader, denyResolving);
                            return XmlReadMode.ReadSchema;
                        }
                        if ((reader.LocalName == "schema") && reader.NamespaceURI.StartsWith("http://www.w3.org/", StringComparison.Ordinal))
                        {
                            throw ExceptionBuilder.DataSetUnsupportedSchema("http://www.w3.org/2001/XMLSchema");
                        }
                        XmlElement topNode = xdoc.CreateElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
                        if (reader.HasAttributes)
                        {
                            int attributeCount = reader.AttributeCount;
                            for (int j = 0; j < attributeCount; j++)
                            {
                                reader.MoveToAttribute(j);
                                if (reader.NamespaceURI.Equals("http://www.w3.org/2000/xmlns/"))
                                {
                                    topNode.SetAttribute(reader.Name, reader.GetAttribute(j));
                                }
                                else
                                {
                                    XmlAttribute attribute = topNode.SetAttributeNode(reader.LocalName, reader.NamespaceURI);
                                    attribute.Prefix = reader.Prefix;
                                    attribute.Value = reader.GetAttribute(j);
                                }
                            }
                        }
                        reader.Read();
                        string str = reader.Value;
                        while (this.MoveToElement(reader, depth))
                        {
                            if ((reader.LocalName == "diffgram") && (reader.NamespaceURI == "urn:schemas-microsoft-com:xml-diffgram-v1"))
                            {
                                this.ReadXmlDiffgram(reader);
                                auto = XmlReadMode.DiffGram;
                            }
                            if ((!flag && !flag7) && ((reader.LocalName == "Schema") && (reader.NamespaceURI == "urn:schemas-microsoft-com:xml-data")))
                            {
                                this.ReadXDRSchema(reader);
                                flag = true;
                                isXdr = true;
                            }
                            else if ((reader.LocalName == "schema") && (reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema"))
                            {
                                this.ReadXSDSchema(reader, denyResolving);
                                flag = true;
                            }
                            else
                            {
                                if ((reader.LocalName == "schema") && reader.NamespaceURI.StartsWith("http://www.w3.org/", StringComparison.Ordinal))
                                {
                                    throw ExceptionBuilder.DataSetUnsupportedSchema("http://www.w3.org/2001/XMLSchema");
                                }
                                if (!(reader.LocalName == "diffgram") || !(reader.NamespaceURI == "urn:schemas-microsoft-com:xml-diffgram-v1"))
                                {
                                    goto Label_0340;
                                }
                                this.ReadXmlDiffgram(reader);
                                flag6 = true;
                                auto = XmlReadMode.DiffGram;
                            }
                            continue;
                        Label_0339:
                            reader.Read();
                        Label_0340:
                            if (!reader.EOF && (reader.NodeType == XmlNodeType.Whitespace))
                            {
                                goto Label_0339;
                            }
                            if (reader.NodeType == XmlNodeType.Element)
                            {
                                flag7 = true;
                                if (!flag && (this.Tables.Count == 0))
                                {
                                    XmlNode newChild = xdoc.ReadNode(reader);
                                    topNode.AppendChild(newChild);
                                }
                                else
                                {
                                    if (loader == null)
                                    {
                                        loader = new XmlDataLoader(this, isXdr, topNode, false);
                                    }
                                    loader.LoadData(reader);
                                    flag5 = true;
                                    if (flag)
                                    {
                                        auto = XmlReadMode.ReadSchema;
                                        continue;
                                    }
                                    auto = XmlReadMode.IgnoreSchema;
                                }
                            }
                        }
                        this.ReadEndElement(reader);
                        bool flag4 = false;
                        bool fTopLevelTable = this.fTopLevelTable;
                        if ((!flag && (this.Tables.Count == 0)) && !topNode.HasChildNodes)
                        {
                            this.fTopLevelTable = true;
                            flag4 = true;
                            if ((str != null) && (str.Length > 0))
                            {
                                topNode.InnerText = str;
                            }
                        }
                        if ((!flag2 && (str != null)) && (str.Length > 0))
                        {
                            topNode.InnerText = str;
                        }
                        xdoc.AppendChild(topNode);
                        if (loader == null)
                        {
                            loader = new XmlDataLoader(this, isXdr, topNode, false);
                        }
                        if (!flag2 && !flag5)
                        {
                            XmlElement documentElement = xdoc.DocumentElement;
                            if ((documentElement.ChildNodes.Count == 0) || ((documentElement.ChildNodes.Count == 1) && (documentElement.FirstChild.GetType() == typeof(XmlText))))
                            {
                                bool flag8 = this.fTopLevelTable;
                                if (((this.DataSetName != documentElement.Name) && (this.namespaceURI != documentElement.NamespaceURI)) && this.Tables.Contains(documentElement.Name, (documentElement.NamespaceURI.Length == 0) ? null : documentElement.NamespaceURI, false, true))
                                {
                                    this.fTopLevelTable = true;
                                }
                                try
                                {
                                    loader.LoadData(xdoc);
                                }
                                finally
                                {
                                    this.fTopLevelTable = flag8;
                                }
                            }
                        }
                        if (!flag6)
                        {
                            if (!flag && (this.Tables.Count == 0))
                            {
                                this.InferSchema(xdoc, null, XmlReadMode.Auto);
                                auto = XmlReadMode.InferSchema;
                                loader.FromInference = true;
                                try
                                {
                                    loader.LoadData(xdoc);
                                }
                                finally
                                {
                                    loader.FromInference = false;
                                }
                            }
                            if (flag4)
                            {
                                this.fTopLevelTable = fTopLevelTable;
                            }
                        }
                    }
                    return auto;
                }
                finally
                {
                    for (int k = 0; k < this.Tables.Count; k++)
                    {
                        this.Tables[k].rowDiffId = null;
                    }
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return mode2;
        }

        public XmlReadMode ReadXml(XmlReader reader, XmlReadMode mode)
        {
            return this.ReadXml(reader, mode, false);
        }

        internal XmlReadMode ReadXml(XmlReader reader, XmlReadMode mode, bool denyResolving)
        {
            XmlReadMode mode3;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataSet.ReadXml|INFO> %d#, mode=%d{ds.XmlReadMode}, denyResolving=%d{bool}\n", this.ObjectID, (int) mode, denyResolving);
            try
            {
                bool flag2 = false;
                bool flag3 = false;
                bool isXdr = false;
                int depth = -1;
                XmlReadMode diffGram = mode;
                if (reader == null)
                {
                    return diffGram;
                }
                if (mode == XmlReadMode.Auto)
                {
                    return this.ReadXml(reader);
                }
                if (reader is XmlTextReader)
                {
                    ((XmlTextReader) reader).WhitespaceHandling = WhitespaceHandling.Significant;
                }
                XmlDocument xdoc = new XmlDocument();
                if ((mode != XmlReadMode.Fragment) && (reader.NodeType == XmlNodeType.Element))
                {
                    depth = reader.Depth;
                }
                reader.MoveToContent();
                XmlDataLoader loader = null;
                if (reader.NodeType == XmlNodeType.Element)
                {
                    XmlElement topNode = null;
                    if (mode == XmlReadMode.Fragment)
                    {
                        xdoc.AppendChild(xdoc.CreateElement("ds_sqlXmlWraPPeR"));
                        topNode = xdoc.DocumentElement;
                    }
                    else
                    {
                        if ((reader.LocalName == "diffgram") && (reader.NamespaceURI == "urn:schemas-microsoft-com:xml-diffgram-v1"))
                        {
                            if ((mode == XmlReadMode.DiffGram) || (mode == XmlReadMode.IgnoreSchema))
                            {
                                this.ReadXmlDiffgram(reader);
                                this.ReadEndElement(reader);
                                return diffGram;
                            }
                            reader.Skip();
                            return diffGram;
                        }
                        if ((reader.LocalName == "Schema") && (reader.NamespaceURI == "urn:schemas-microsoft-com:xml-data"))
                        {
                            if (((mode != XmlReadMode.IgnoreSchema) && (mode != XmlReadMode.InferSchema)) && (mode != XmlReadMode.InferTypedSchema))
                            {
                                this.ReadXDRSchema(reader);
                                return diffGram;
                            }
                            reader.Skip();
                            return diffGram;
                        }
                        if ((reader.LocalName == "schema") && (reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema"))
                        {
                            if (((mode != XmlReadMode.IgnoreSchema) && (mode != XmlReadMode.InferSchema)) && (mode != XmlReadMode.InferTypedSchema))
                            {
                                this.ReadXSDSchema(reader, denyResolving);
                                return diffGram;
                            }
                            reader.Skip();
                            return diffGram;
                        }
                        if ((reader.LocalName == "schema") && reader.NamespaceURI.StartsWith("http://www.w3.org/", StringComparison.Ordinal))
                        {
                            throw ExceptionBuilder.DataSetUnsupportedSchema("http://www.w3.org/2001/XMLSchema");
                        }
                        topNode = xdoc.CreateElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
                        if (reader.HasAttributes)
                        {
                            int attributeCount = reader.AttributeCount;
                            for (int i = 0; i < attributeCount; i++)
                            {
                                reader.MoveToAttribute(i);
                                if (reader.NamespaceURI.Equals("http://www.w3.org/2000/xmlns/"))
                                {
                                    topNode.SetAttribute(reader.Name, reader.GetAttribute(i));
                                }
                                else
                                {
                                    XmlAttribute attribute = topNode.SetAttributeNode(reader.LocalName, reader.NamespaceURI);
                                    attribute.Prefix = reader.Prefix;
                                    attribute.Value = reader.GetAttribute(i);
                                }
                            }
                        }
                        reader.Read();
                    }
                    while (this.MoveToElement(reader, depth))
                    {
                        if ((reader.LocalName == "Schema") && (reader.NamespaceURI == "urn:schemas-microsoft-com:xml-data"))
                        {
                            if (((!flag2 && !flag3) && ((mode != XmlReadMode.IgnoreSchema) && (mode != XmlReadMode.InferSchema))) && (mode != XmlReadMode.InferTypedSchema))
                            {
                                this.ReadXDRSchema(reader);
                                flag2 = true;
                                isXdr = true;
                            }
                            else
                            {
                                reader.Skip();
                            }
                        }
                        else
                        {
                            if ((reader.LocalName == "schema") && (reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema"))
                            {
                                if (((mode != XmlReadMode.IgnoreSchema) && (mode != XmlReadMode.InferSchema)) && (mode != XmlReadMode.InferTypedSchema))
                                {
                                    this.ReadXSDSchema(reader, denyResolving);
                                    flag2 = true;
                                }
                                else
                                {
                                    reader.Skip();
                                }
                                continue;
                            }
                            if ((reader.LocalName == "diffgram") && (reader.NamespaceURI == "urn:schemas-microsoft-com:xml-diffgram-v1"))
                            {
                                if ((mode == XmlReadMode.DiffGram) || (mode == XmlReadMode.IgnoreSchema))
                                {
                                    this.ReadXmlDiffgram(reader);
                                    diffGram = XmlReadMode.DiffGram;
                                }
                                else
                                {
                                    reader.Skip();
                                }
                                continue;
                            }
                            if ((reader.LocalName == "schema") && reader.NamespaceURI.StartsWith("http://www.w3.org/", StringComparison.Ordinal))
                            {
                                throw ExceptionBuilder.DataSetUnsupportedSchema("http://www.w3.org/2001/XMLSchema");
                            }
                            if (mode == XmlReadMode.DiffGram)
                            {
                                reader.Skip();
                            }
                            else
                            {
                                flag3 = true;
                                if ((mode == XmlReadMode.InferSchema) || (mode == XmlReadMode.InferTypedSchema))
                                {
                                    XmlNode newChild = xdoc.ReadNode(reader);
                                    topNode.AppendChild(newChild);
                                    continue;
                                }
                                if (loader == null)
                                {
                                    loader = new XmlDataLoader(this, isXdr, topNode, mode == XmlReadMode.IgnoreSchema);
                                }
                                loader.LoadData(reader);
                            }
                        }
                    }
                    this.ReadEndElement(reader);
                    xdoc.AppendChild(topNode);
                    if (loader == null)
                    {
                        loader = new XmlDataLoader(this, isXdr, mode == XmlReadMode.IgnoreSchema);
                    }
                    if (mode == XmlReadMode.DiffGram)
                    {
                        return diffGram;
                    }
                    if ((mode == XmlReadMode.InferSchema) || (mode == XmlReadMode.InferTypedSchema))
                    {
                        this.InferSchema(xdoc, null, mode);
                        diffGram = XmlReadMode.InferSchema;
                        loader.FromInference = true;
                        try
                        {
                            loader.LoadData(xdoc);
                        }
                        finally
                        {
                            loader.FromInference = false;
                        }
                    }
                }
                mode3 = diffGram;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return mode3;
        }

        private void ReadXmlDiffgram(XmlReader reader)
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataSet.ReadXmlDiffgram|INFO> %d#\n", this.ObjectID);
            try
            {
                DataSet set;
                int depth = reader.Depth;
                bool enforceConstraints = this.EnforceConstraints;
                this.EnforceConstraints = false;
                bool flag = this.IsEmpty();
                if (flag)
                {
                    set = this;
                }
                else
                {
                    set = this.Clone();
                    set.EnforceConstraints = false;
                }
                foreach (DataTable table3 in set.Tables)
                {
                    table3.Rows.nullInList = 0;
                }
                reader.MoveToContent();
                if ((reader.LocalName == "diffgram") || (reader.NamespaceURI == "urn:schemas-microsoft-com:xml-diffgram-v1"))
                {
                    reader.Read();
                    if (reader.NodeType == XmlNodeType.Whitespace)
                    {
                        this.MoveToElement(reader, reader.Depth - 1);
                    }
                    set.fInLoadDiffgram = true;
                    if (reader.Depth > depth)
                    {
                        if ((reader.NamespaceURI != "urn:schemas-microsoft-com:xml-diffgram-v1") && (reader.NamespaceURI != "urn:schemas-microsoft-com:xml-msdata"))
                        {
                            XmlElement topNode = new XmlDocument().CreateElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
                            reader.Read();
                            if (reader.NodeType == XmlNodeType.Whitespace)
                            {
                                this.MoveToElement(reader, reader.Depth - 1);
                            }
                            if ((reader.Depth - 1) > depth)
                            {
                                new XmlDataLoader(set, false, topNode, false) { isDiffgram = true }.LoadData(reader);
                            }
                            this.ReadEndElement(reader);
                            if (reader.NodeType == XmlNodeType.Whitespace)
                            {
                                this.MoveToElement(reader, reader.Depth - 1);
                            }
                        }
                        if (((reader.LocalName == "before") && (reader.NamespaceURI == "urn:schemas-microsoft-com:xml-diffgram-v1")) || ((reader.LocalName == "errors") && (reader.NamespaceURI == "urn:schemas-microsoft-com:xml-diffgram-v1")))
                        {
                            new XMLDiffLoader().LoadDiffGram(set, reader);
                        }
                        while (reader.Depth > depth)
                        {
                            reader.Read();
                        }
                        this.ReadEndElement(reader);
                    }
                    foreach (DataTable table2 in set.Tables)
                    {
                        if (table2.Rows.nullInList > 0)
                        {
                            throw ExceptionBuilder.RowInsertMissing(table2.TableName);
                        }
                    }
                    set.fInLoadDiffgram = false;
                    foreach (DataTable table in set.Tables)
                    {
                        DataRelation[] nestedParentRelations = table.NestedParentRelations;
                        foreach (DataRelation relation2 in nestedParentRelations)
                        {
                            if (relation2.ParentTable == table)
                            {
                                foreach (DataRow row in table.Rows)
                                {
                                    foreach (DataRelation relation in nestedParentRelations)
                                    {
                                        row.CheckForLoops(relation);
                                    }
                                }
                            }
                        }
                    }
                    if (!flag)
                    {
                        this.Merge(set);
                        if (this.dataSetName == "NewDataSet")
                        {
                            this.dataSetName = set.dataSetName;
                        }
                        set.EnforceConstraints = enforceConstraints;
                    }
                    this.EnforceConstraints = enforceConstraints;
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        public void ReadXmlSchema(Stream stream)
        {
            if (stream != null)
            {
                this.ReadXmlSchema(new XmlTextReader(stream), false);
            }
        }

        public void ReadXmlSchema(TextReader reader)
        {
            if (reader != null)
            {
                this.ReadXmlSchema(new XmlTextReader(reader), false);
            }
        }

        public void ReadXmlSchema(string fileName)
        {
            XmlTextReader reader = new XmlTextReader(fileName);
            try
            {
                this.ReadXmlSchema(reader, false);
            }
            finally
            {
                reader.Close();
            }
        }

        public void ReadXmlSchema(XmlReader reader)
        {
            this.ReadXmlSchema(reader, false);
        }

        internal void ReadXmlSchema(XmlReader reader, bool denyResolving)
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataSet.ReadXmlSchema|INFO> %d#, reader, denyResolving=%d{bool}\n", this.ObjectID, denyResolving);
            try
            {
                int depth = -1;
                if (reader != null)
                {
                    if (reader is XmlTextReader)
                    {
                        ((XmlTextReader) reader).WhitespaceHandling = WhitespaceHandling.None;
                    }
                    XmlDocument xdoc = new XmlDocument();
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        depth = reader.Depth;
                    }
                    reader.MoveToContent();
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if ((reader.LocalName == "Schema") && (reader.NamespaceURI == "urn:schemas-microsoft-com:xml-data"))
                        {
                            this.ReadXDRSchema(reader);
                        }
                        else if ((reader.LocalName == "schema") && (reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema"))
                        {
                            this.ReadXSDSchema(reader, denyResolving);
                        }
                        else
                        {
                            if ((reader.LocalName == "schema") && reader.NamespaceURI.StartsWith("http://www.w3.org/", StringComparison.Ordinal))
                            {
                                throw ExceptionBuilder.DataSetUnsupportedSchema("http://www.w3.org/2001/XMLSchema");
                            }
                            XmlElement newChild = xdoc.CreateElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
                            if (reader.HasAttributes)
                            {
                                int attributeCount = reader.AttributeCount;
                                for (int i = 0; i < attributeCount; i++)
                                {
                                    reader.MoveToAttribute(i);
                                    if (reader.NamespaceURI.Equals("http://www.w3.org/2000/xmlns/"))
                                    {
                                        newChild.SetAttribute(reader.Name, reader.GetAttribute(i));
                                    }
                                    else
                                    {
                                        XmlAttribute attribute = newChild.SetAttributeNode(reader.LocalName, reader.NamespaceURI);
                                        attribute.Prefix = reader.Prefix;
                                        attribute.Value = reader.GetAttribute(i);
                                    }
                                }
                            }
                            reader.Read();
                            while (this.MoveToElement(reader, depth))
                            {
                                if ((reader.LocalName == "Schema") && (reader.NamespaceURI == "urn:schemas-microsoft-com:xml-data"))
                                {
                                    this.ReadXDRSchema(reader);
                                    return;
                                }
                                if ((reader.LocalName == "schema") && (reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema"))
                                {
                                    this.ReadXSDSchema(reader, denyResolving);
                                    return;
                                }
                                if ((reader.LocalName == "schema") && reader.NamespaceURI.StartsWith("http://www.w3.org/", StringComparison.Ordinal))
                                {
                                    throw ExceptionBuilder.DataSetUnsupportedSchema("http://www.w3.org/2001/XMLSchema");
                                }
                                XmlNode node = xdoc.ReadNode(reader);
                                newChild.AppendChild(node);
                            }
                            this.ReadEndElement(reader);
                            xdoc.AppendChild(newChild);
                            this.InferSchema(xdoc, null, XmlReadMode.Auto);
                        }
                    }
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        protected virtual void ReadXmlSerializable(XmlReader reader)
        {
            this.UseDataSetSchemaOnly = false;
            this.UdtIsWrapped = false;
            if (reader.HasAttributes)
            {
                if (reader.MoveToAttribute("xsi:nil") && (string.Compare(reader.GetAttribute("xsi:nil"), "true", StringComparison.Ordinal) == 0))
                {
                    this.MoveToElement(reader, 1);
                    return;
                }
                if (reader.MoveToAttribute("msdata:UseDataSetSchemaOnly"))
                {
                    string attribute = reader.GetAttribute("msdata:UseDataSetSchemaOnly");
                    if (string.Equals(attribute, "true", StringComparison.Ordinal) || string.Equals(attribute, "1", StringComparison.Ordinal))
                    {
                        this.UseDataSetSchemaOnly = true;
                    }
                    else if (!string.Equals(attribute, "false", StringComparison.Ordinal) && !string.Equals(attribute, "0", StringComparison.Ordinal))
                    {
                        throw ExceptionBuilder.InvalidAttributeValue("UseDataSetSchemaOnly", attribute);
                    }
                }
                if (reader.MoveToAttribute("msdata:UDTColumnValueWrapped"))
                {
                    string a = reader.GetAttribute("msdata:UDTColumnValueWrapped");
                    if (string.Equals(a, "true", StringComparison.Ordinal) || string.Equals(a, "1", StringComparison.Ordinal))
                    {
                        this.UdtIsWrapped = true;
                    }
                    else if (!string.Equals(a, "false", StringComparison.Ordinal) && !string.Equals(a, "0", StringComparison.Ordinal))
                    {
                        throw ExceptionBuilder.InvalidAttributeValue("UDTColumnValueWrapped", a);
                    }
                }
            }
            this.ReadXml(reader, XmlReadMode.DiffGram, true);
        }

        internal void ReadXSDSchema(XmlReader reader, bool denyResolving)
        {
            XmlSchemaSet schemaSet = new XmlSchemaSet();
            int num = 1;
            if (((reader.LocalName == "schema") && (reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema")) && reader.HasAttributes)
            {
                string attribute = reader.GetAttribute("schemafragmentcount", "urn:schemas-microsoft-com:xml-msdata");
                if (!ADP.IsEmpty(attribute))
                {
                    num = int.Parse(attribute, (IFormatProvider) null);
                }
            }
            while ((reader.LocalName == "schema") && (reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema"))
            {
                XmlSchema schema = XmlSchema.Read(reader, null);
                schemaSet.Add(schema);
                this.ReadEndElement(reader);
                if (--num > 0)
                {
                    MoveToElement(reader);
                }
                while (reader.NodeType == XmlNodeType.Whitespace)
                {
                    reader.Skip();
                }
            }
            schemaSet.Compile();
            new XSDSchema().LoadSchema(schemaSet, this);
        }

        public virtual void RejectChanges()
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataSet.RejectChanges|API> %d#\n", this.ObjectID);
            try
            {
                bool enforceConstraints = this.EnforceConstraints;
                this.EnforceConstraints = false;
                for (int i = 0; i < this.Tables.Count; i++)
                {
                    this.Tables[i].RejectChanges();
                }
                this.EnforceConstraints = enforceConstraints;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        public virtual void Reset()
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataSet.Reset|API> %d#\n", this.ObjectID);
            try
            {
                for (int i = 0; i < this.Tables.Count; i++)
                {
                    ConstraintCollection constraints = this.Tables[i].Constraints;
                    int num = 0;
                    while (num < constraints.Count)
                    {
                        if (constraints[num] is ForeignKeyConstraint)
                        {
                            constraints.Remove(constraints[num]);
                        }
                        else
                        {
                            num++;
                        }
                    }
                }
                this.Clear();
                this.Relations.Clear();
                this.Tables.Clear();
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        private void ResetRelations()
        {
            this.Relations.Clear();
        }

        private void ResetTables()
        {
            this.Tables.Clear();
        }

        internal void RestoreEnforceConstraints(bool value)
        {
            this.enforceConstraints = value;
        }

        private void SerializeDataSet(SerializationInfo info, StreamingContext context, SerializationFormat remotingFormat)
        {
            info.AddValue("DataSet.RemotingVersion", new Version(2, 0));
            if (remotingFormat != SerializationFormat.Xml)
            {
                info.AddValue("DataSet.RemotingFormat", remotingFormat);
            }
            if (System.Data.SchemaSerializationMode.IncludeSchema != this.SchemaSerializationMode)
            {
                info.AddValue("SchemaSerializationMode.DataSet", this.SchemaSerializationMode);
            }
            if (remotingFormat != SerializationFormat.Xml)
            {
                if (this.SchemaSerializationMode == System.Data.SchemaSerializationMode.IncludeSchema)
                {
                    this.SerializeDataSetProperties(info, context);
                    info.AddValue("DataSet.Tables.Count", this.Tables.Count);
                    for (int j = 0; j < this.Tables.Count; j++)
                    {
                        BinaryFormatter formatter = new BinaryFormatter(null, new StreamingContext(context.State, false));
                        MemoryStream serializationStream = new MemoryStream();
                        formatter.Serialize(serializationStream, this.Tables[j]);
                        serializationStream.Position = 0L;
                        info.AddValue(string.Format(CultureInfo.InvariantCulture, "DataSet.Tables_{0}", new object[] { j }), serializationStream.GetBuffer());
                    }
                    for (int k = 0; k < this.Tables.Count; k++)
                    {
                        this.Tables[k].SerializeConstraints(info, context, k, true);
                    }
                    this.SerializeRelations(info, context);
                    for (int m = 0; m < this.Tables.Count; m++)
                    {
                        this.Tables[m].SerializeExpressionColumns(info, context, m);
                    }
                }
                else
                {
                    this.SerializeDataSetProperties(info, context);
                }
                for (int i = 0; i < this.Tables.Count; i++)
                {
                    this.Tables[i].SerializeTableData(info, context, i);
                }
            }
            else
            {
                string xmlSchemaForRemoting = this.GetXmlSchemaForRemoting(null);
                string str = null;
                info.AddValue("XmlSchema", xmlSchemaForRemoting);
                StringBuilder sb = new StringBuilder(this.EstimatedXmlStringSize() * 2);
                StringWriter w = new StringWriter(sb, CultureInfo.InvariantCulture);
                XmlTextWriter writer = new XmlTextWriter(w);
                this.WriteXml(writer, XmlWriteMode.DiffGram);
                str = w.ToString();
                info.AddValue("XmlDiffGram", str);
            }
        }

        private void SerializeDataSetProperties(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("DataSet.DataSetName", this.DataSetName);
            info.AddValue("DataSet.Namespace", this.Namespace);
            info.AddValue("DataSet.Prefix", this.Prefix);
            info.AddValue("DataSet.CaseSensitive", this.CaseSensitive);
            info.AddValue("DataSet.LocaleLCID", this.Locale.LCID);
            info.AddValue("DataSet.EnforceConstraints", this.EnforceConstraints);
            info.AddValue("DataSet.ExtendedProperties", this.ExtendedProperties);
        }

        private void SerializeRelations(SerializationInfo info, StreamingContext context)
        {
            ArrayList list2 = new ArrayList();
            foreach (DataRelation relation in this.Relations)
            {
                int[] numArray2 = new int[relation.ParentColumns.Length + 1];
                numArray2[0] = this.Tables.IndexOf(relation.ParentTable);
                for (int i = 1; i < numArray2.Length; i++)
                {
                    numArray2[i] = relation.ParentColumns[i - 1].Ordinal;
                }
                int[] numArray = new int[relation.ChildColumns.Length + 1];
                numArray[0] = this.Tables.IndexOf(relation.ChildTable);
                for (int j = 1; j < numArray.Length; j++)
                {
                    numArray[j] = relation.ChildColumns[j - 1].Ordinal;
                }
                ArrayList list = new ArrayList();
                list.Add(relation.RelationName);
                list.Add(numArray2);
                list.Add(numArray);
                list.Add(relation.Nested);
                list.Add(relation.extendedProperties);
                list2.Add(list);
            }
            info.AddValue("DataSet.Relations", list2);
        }

        internal void SetLocaleValue(CultureInfo value, bool userSet)
        {
            bool flag = false;
            bool flag2 = false;
            int num2 = 0;
            CultureInfo culture = this._culture;
            bool flag3 = this._cultureUserSet;
            try
            {
                this._culture = value;
                this._cultureUserSet = userSet;
                foreach (DataTable table3 in this.Tables)
                {
                    if (!table3.ShouldSerializeLocale())
                    {
                        table3.SetLocaleValue(value, false, false);
                    }
                }
                flag = this.ValidateLocaleConstraint();
                if (flag)
                {
                    flag = false;
                    foreach (DataTable table2 in this.Tables)
                    {
                        num2++;
                        if (!table2.ShouldSerializeLocale())
                        {
                            table2.SetLocaleValue(value, false, true);
                        }
                    }
                    flag = true;
                }
            }
            catch
            {
                flag2 = true;
                throw;
            }
            finally
            {
                if (!flag)
                {
                    this._culture = culture;
                    this._cultureUserSet = flag3;
                    foreach (DataTable table in this.Tables)
                    {
                        if (!table.ShouldSerializeLocale())
                        {
                            table.SetLocaleValue(culture, false, false);
                        }
                    }
                    try
                    {
                        for (int i = 0; i < num2; i++)
                        {
                            if (!this.Tables[i].ShouldSerializeLocale())
                            {
                                this.Tables[i].SetLocaleValue(culture, false, true);
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        if (!ADP.IsCatchableExceptionType(exception))
                        {
                            throw;
                        }
                        ADP.TraceExceptionWithoutRethrow(exception);
                    }
                    if (!flag2)
                    {
                        throw ExceptionBuilder.CannotChangeCaseLocale(null);
                    }
                }
            }
        }

        internal bool ShouldSerializeLocale()
        {
            return this._cultureUserSet;
        }

        protected virtual bool ShouldSerializeRelations()
        {
            return true;
        }

        protected virtual bool ShouldSerializeTables()
        {
            return true;
        }

        IList IListSource.GetList()
        {
            return this.DefaultViewManager;
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            if (base.GetType() == typeof(DataSet))
            {
                return null;
            }
            MemoryStream w = new MemoryStream();
            XmlWriter xw = new XmlTextWriter(w, null);
            if (xw != null)
            {
                new XmlTreeGen(SchemaFormat.WebService).Save(this, xw);
            }
            w.Position = 0L;
            return XmlSchema.Read(new XmlTextReader(w), null);
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            bool normalized = true;
            XmlTextReader reader2 = null;
            IXmlTextParser parser = reader as IXmlTextParser;
            if (parser != null)
            {
                normalized = parser.Normalized;
                parser.Normalized = false;
            }
            else
            {
                reader2 = reader as XmlTextReader;
                if (reader2 != null)
                {
                    normalized = reader2.Normalization;
                    reader2.Normalization = false;
                }
            }
            this.ReadXmlSerializable(reader);
            if (parser != null)
            {
                parser.Normalized = normalized;
            }
            else if (reader2 != null)
            {
                reader2.Normalization = normalized;
            }
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            this.WriteXmlSchema(writer, SchemaFormat.WebService, null);
            this.WriteXml(writer, XmlWriteMode.DiffGram);
        }

        internal DataTable[] TopLevelTables()
        {
            return this.TopLevelTables(false);
        }

        internal DataTable[] TopLevelTables(bool forSchema)
        {
            List<DataTable> list = new List<DataTable>();
            if (forSchema)
            {
                for (int j = 0; j < this.Tables.Count; j++)
                {
                    DataTable item = this.Tables[j];
                    if ((item.NestedParentsCount > 1) || item.SelfNested)
                    {
                        list.Add(item);
                    }
                }
            }
            for (int i = 0; i < this.Tables.Count; i++)
            {
                DataTable table = this.Tables[i];
                if ((table.NestedParentsCount == 0) && !list.Contains(table))
                {
                    list.Add(table);
                }
            }
            if (list.Count == 0)
            {
                return zeroTables;
            }
            return list.ToArray();
        }

        internal bool ValidateCaseConstraint()
        {
            bool flag;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataSet.ValidateCaseConstraint|INFO> %d#\n", this.ObjectID);
            try
            {
                DataRelation relation = null;
                for (int i = 0; i < this.Relations.Count; i++)
                {
                    relation = this.Relations[i];
                    if (relation.ChildTable.CaseSensitive != relation.ParentTable.CaseSensitive)
                    {
                        return false;
                    }
                }
                ForeignKeyConstraint constraint = null;
                ConstraintCollection constraints = null;
                for (int j = 0; j < this.Tables.Count; j++)
                {
                    constraints = this.Tables[j].Constraints;
                    for (int k = 0; k < constraints.Count; k++)
                    {
                        if (constraints[k] is ForeignKeyConstraint)
                        {
                            constraint = (ForeignKeyConstraint) constraints[k];
                            if (constraint.Table.CaseSensitive != constraint.RelatedTable.CaseSensitive)
                            {
                                return false;
                            }
                        }
                    }
                }
                flag = true;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return flag;
        }

        internal bool ValidateLocaleConstraint()
        {
            bool flag;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataSet.ValidateLocaleConstraint|INFO> %d#\n", this.ObjectID);
            try
            {
                DataRelation relation = null;
                for (int i = 0; i < this.Relations.Count; i++)
                {
                    relation = this.Relations[i];
                    if (relation.ChildTable.Locale.LCID != relation.ParentTable.Locale.LCID)
                    {
                        return false;
                    }
                }
                ForeignKeyConstraint constraint = null;
                ConstraintCollection constraints = null;
                for (int j = 0; j < this.Tables.Count; j++)
                {
                    constraints = this.Tables[j].Constraints;
                    for (int k = 0; k < constraints.Count; k++)
                    {
                        if (constraints[k] is ForeignKeyConstraint)
                        {
                            constraint = (ForeignKeyConstraint) constraints[k];
                            if (constraint.Table.Locale.LCID != constraint.RelatedTable.Locale.LCID)
                            {
                                return false;
                            }
                        }
                    }
                }
                flag = true;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return flag;
        }

        public void WriteXml(Stream stream)
        {
            this.WriteXml(stream, XmlWriteMode.IgnoreSchema);
        }

        public void WriteXml(TextWriter writer)
        {
            this.WriteXml(writer, XmlWriteMode.IgnoreSchema);
        }

        public void WriteXml(string fileName)
        {
            this.WriteXml(fileName, XmlWriteMode.IgnoreSchema);
        }

        public void WriteXml(XmlWriter writer)
        {
            this.WriteXml(writer, XmlWriteMode.IgnoreSchema);
        }

        public void WriteXml(Stream stream, XmlWriteMode mode)
        {
            if (stream != null)
            {
                XmlTextWriter writer = new XmlTextWriter(stream, null) {
                    Formatting = Formatting.Indented
                };
                this.WriteXml(writer, mode);
            }
        }

        public void WriteXml(TextWriter writer, XmlWriteMode mode)
        {
            if (writer != null)
            {
                XmlTextWriter writer2 = new XmlTextWriter(writer) {
                    Formatting = Formatting.Indented
                };
                this.WriteXml(writer2, mode);
            }
        }

        public void WriteXml(string fileName, XmlWriteMode mode)
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataSet.WriteXml|API> %d#, fileName='%ls', mode=%d{ds.XmlWriteMode}\n", this.ObjectID, fileName, (int) mode);
            XmlTextWriter xmlw = new XmlTextWriter(fileName, null);
            try
            {
                xmlw.Formatting = Formatting.Indented;
                xmlw.WriteStartDocument(true);
                if (xmlw != null)
                {
                    if (mode == XmlWriteMode.DiffGram)
                    {
                        new NewDiffgramGen(this).Save(xmlw);
                    }
                    else
                    {
                        new XmlDataTreeWriter(this).Save(xmlw, mode == XmlWriteMode.WriteSchema);
                    }
                }
                xmlw.WriteEndDocument();
            }
            finally
            {
                xmlw.Close();
                Bid.ScopeLeave(ref ptr);
            }
        }

        public void WriteXml(XmlWriter writer, XmlWriteMode mode)
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataSet.WriteXml|API> %d#, mode=%d{ds.XmlWriteMode}\n", this.ObjectID, (int) mode);
            try
            {
                if (writer != null)
                {
                    if (mode == XmlWriteMode.DiffGram)
                    {
                        new NewDiffgramGen(this).Save(writer);
                    }
                    else
                    {
                        new XmlDataTreeWriter(this).Save(writer, mode == XmlWriteMode.WriteSchema);
                    }
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        public void WriteXmlSchema(Stream stream)
        {
            this.WriteXmlSchema(stream, SchemaFormat.Public, null);
        }

        public void WriteXmlSchema(TextWriter writer)
        {
            this.WriteXmlSchema(writer, SchemaFormat.Public, null);
        }

        public void WriteXmlSchema(string fileName)
        {
            this.WriteXmlSchema(fileName, SchemaFormat.Public, null);
        }

        public void WriteXmlSchema(XmlWriter writer)
        {
            this.WriteXmlSchema(writer, SchemaFormat.Public, null);
        }

        public void WriteXmlSchema(Stream stream, Converter<Type, string> multipleTargetConverter)
        {
            ADP.CheckArgumentNull(multipleTargetConverter, "multipleTargetConverter");
            this.WriteXmlSchema(stream, SchemaFormat.Public, multipleTargetConverter);
        }

        public void WriteXmlSchema(TextWriter writer, Converter<Type, string> multipleTargetConverter)
        {
            ADP.CheckArgumentNull(multipleTargetConverter, "multipleTargetConverter");
            this.WriteXmlSchema(writer, SchemaFormat.Public, multipleTargetConverter);
        }

        public void WriteXmlSchema(string fileName, Converter<Type, string> multipleTargetConverter)
        {
            ADP.CheckArgumentNull(multipleTargetConverter, "multipleTargetConverter");
            this.WriteXmlSchema(fileName, SchemaFormat.Public, multipleTargetConverter);
        }

        public void WriteXmlSchema(XmlWriter writer, Converter<Type, string> multipleTargetConverter)
        {
            ADP.CheckArgumentNull(multipleTargetConverter, "multipleTargetConverter");
            this.WriteXmlSchema(writer, SchemaFormat.Public, multipleTargetConverter);
        }

        private void WriteXmlSchema(Stream stream, SchemaFormat schemaFormat, Converter<Type, string> multipleTargetConverter)
        {
            if (stream != null)
            {
                XmlTextWriter writer = new XmlTextWriter(stream, null) {
                    Formatting = Formatting.Indented
                };
                this.WriteXmlSchema(writer, schemaFormat, multipleTargetConverter);
            }
        }

        private void WriteXmlSchema(TextWriter writer, SchemaFormat schemaFormat, Converter<Type, string> multipleTargetConverter)
        {
            if (writer != null)
            {
                XmlTextWriter writer2 = new XmlTextWriter(writer) {
                    Formatting = Formatting.Indented
                };
                this.WriteXmlSchema(writer2, schemaFormat, multipleTargetConverter);
            }
        }

        private void WriteXmlSchema(string fileName, SchemaFormat schemaFormat, Converter<Type, string> multipleTargetConverter)
        {
            XmlTextWriter writer = new XmlTextWriter(fileName, null);
            try
            {
                writer.Formatting = Formatting.Indented;
                writer.WriteStartDocument(true);
                this.WriteXmlSchema(writer, schemaFormat, multipleTargetConverter);
                writer.WriteEndDocument();
            }
            finally
            {
                writer.Close();
            }
        }

        private void WriteXmlSchema(XmlWriter writer, SchemaFormat schemaFormat, Converter<Type, string> multipleTargetConverter)
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataSet.WriteXmlSchema|INFO> %d#, schemaFormat=%d{ds.SchemaFormat}\n", this.ObjectID, (int) schemaFormat);
            try
            {
                if (writer != null)
                {
                    XmlTreeGen gen = null;
                    if (((schemaFormat == SchemaFormat.WebService) && (this.SchemaSerializationMode == System.Data.SchemaSerializationMode.ExcludeSchema)) && (writer.WriteState == WriteState.Element))
                    {
                        gen = new XmlTreeGen(SchemaFormat.WebServiceSkipSchema);
                    }
                    else
                    {
                        gen = new XmlTreeGen(schemaFormat);
                    }
                    gen.Save(this, null, writer, false, multipleTargetConverter);
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        [System.Data.ResCategory("DataCategory_Data"), DefaultValue(false), System.Data.ResDescription("DataSetCaseSensitiveDescr")]
        public bool CaseSensitive
        {
            get
            {
                return this._caseSensitive;
            }
            set
            {
                if (this._caseSensitive != value)
                {
                    bool flag = this._caseSensitive;
                    this._caseSensitive = value;
                    if (!this.ValidateCaseConstraint())
                    {
                        this._caseSensitive = flag;
                        throw ExceptionBuilder.CannotChangeCaseLocale();
                    }
                    foreach (DataTable table in this.Tables)
                    {
                        table.SetCaseSensitiveValue(value, false, true);
                    }
                }
            }
        }

        [DefaultValue(""), System.Data.ResDescription("DataSetDataSetNameDescr"), System.Data.ResCategory("DataCategory_Data")]
        public string DataSetName
        {
            get
            {
                return this.dataSetName;
            }
            set
            {
                Bid.Trace("<ds.DataSet.set_DataSetName|API> %d#, '%ls'\n", this.ObjectID, value);
                if (value != this.dataSetName)
                {
                    if ((value == null) || (value.Length == 0))
                    {
                        throw ExceptionBuilder.SetDataSetNameToEmpty();
                    }
                    DataTable table = this.Tables[value, this.Namespace];
                    if ((table != null) && !table.fNestedInDataset)
                    {
                        throw ExceptionBuilder.SetDataSetNameConflicting(value);
                    }
                    this.RaisePropertyChanging("DataSetName");
                    this.dataSetName = value;
                }
            }
        }

        [System.Data.ResDescription("DataSetDefaultViewDescr"), Browsable(false)]
        public DataViewManager DefaultViewManager
        {
            get
            {
                if (this.defaultViewManager == null)
                {
                    lock (this._defaultViewManagerLock)
                    {
                        if (this.defaultViewManager == null)
                        {
                            this.defaultViewManager = new DataViewManager(this, true);
                        }
                    }
                }
                return this.defaultViewManager;
            }
        }

        [System.Data.ResDescription("DataSetEnforceConstraintsDescr"), DefaultValue(true)]
        public bool EnforceConstraints
        {
            get
            {
                return this.enforceConstraints;
            }
            set
            {
                IntPtr ptr;
                Bid.ScopeEnter(out ptr, "<ds.DataSet.set_EnforceConstraints|API> %d#, %d{bool}\n", this.ObjectID, value);
                try
                {
                    if (this.enforceConstraints != value)
                    {
                        if (value)
                        {
                            this.EnableConstraints();
                        }
                        this.enforceConstraints = value;
                    }
                }
                finally
                {
                    Bid.ScopeLeave(ref ptr);
                }
            }
        }

        [System.Data.ResDescription("ExtendedPropertiesDescr"), System.Data.ResCategory("DataCategory_Data"), Browsable(false)]
        public PropertyCollection ExtendedProperties
        {
            get
            {
                if (this.extendedProperties == null)
                {
                    this.extendedProperties = new PropertyCollection();
                }
                return this.extendedProperties;
            }
        }

        internal bool FBoundToDocument
        {
            get
            {
                return this.fBoundToDocument;
            }
            set
            {
                this.fBoundToDocument = value;
            }
        }

        [Browsable(false), System.Data.ResDescription("DataSetHasErrorsDescr")]
        public bool HasErrors
        {
            get
            {
                for (int i = 0; i < this.Tables.Count; i++)
                {
                    if (this.Tables[i].HasErrors)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        [Browsable(false)]
        public bool IsInitialized
        {
            get
            {
                return !this.fInitInProgress;
            }
        }

        [System.Data.ResCategory("DataCategory_Data"), System.Data.ResDescription("DataSetLocaleDescr")]
        public CultureInfo Locale
        {
            get
            {
                return this._culture;
            }
            set
            {
                IntPtr ptr;
                Bid.ScopeEnter(out ptr, "<ds.DataSet.set_Locale|API> %d#\n", this.ObjectID);
                try
                {
                    if (value != null)
                    {
                        if (!this._culture.Equals(value))
                        {
                            this.SetLocaleValue(value, true);
                        }
                        this._cultureUserSet = true;
                    }
                }
                finally
                {
                    Bid.ScopeLeave(ref ptr);
                }
            }
        }

        internal string MainTableName
        {
            get
            {
                return this.mainTableName;
            }
            set
            {
                this.mainTableName = value;
            }
        }

        [System.Data.ResDescription("DataSetNamespaceDescr"), DefaultValue(""), System.Data.ResCategory("DataCategory_Data")]
        public string Namespace
        {
            get
            {
                return this.namespaceURI;
            }
            set
            {
                Bid.Trace("<ds.DataSet.set_Namespace|API> %d#, '%ls'\n", this.ObjectID, value);
                if (value == null)
                {
                    value = string.Empty;
                }
                if (value != this.namespaceURI)
                {
                    this.RaisePropertyChanging("Namespace");
                    foreach (DataTable table in this.Tables)
                    {
                        if ((table.tableNamespace == null) && ((table.NestedParentRelations.Length == 0) || ((table.NestedParentRelations.Length == 1) && (table.NestedParentRelations[0].ChildTable == table))))
                        {
                            if (this.Tables.Contains(table.TableName, value, false, true))
                            {
                                throw ExceptionBuilder.DuplicateTableName2(table.TableName, value);
                            }
                            table.CheckCascadingNamespaceConflict(value);
                            table.DoRaiseNamespaceChange();
                        }
                    }
                    this.namespaceURI = value;
                    if (ADP.IsEmpty(value))
                    {
                        this._datasetPrefix = string.Empty;
                    }
                }
            }
        }

        internal int ObjectID
        {
            get
            {
                return this._objectID;
            }
        }

        [DefaultValue(""), System.Data.ResCategory("DataCategory_Data"), System.Data.ResDescription("DataSetPrefixDescr")]
        public string Prefix
        {
            get
            {
                return this._datasetPrefix;
            }
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }
                if ((XmlConvert.DecodeName(value) == value) && (XmlConvert.EncodeName(value) != value))
                {
                    throw ExceptionBuilder.InvalidPrefix(value);
                }
                if (value != this._datasetPrefix)
                {
                    this.RaisePropertyChanging("Prefix");
                    this._datasetPrefix = value;
                }
            }
        }

        [System.Data.ResDescription("DataSetRelationsDescr"), System.Data.ResCategory("DataCategory_Data"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public DataRelationCollection Relations
        {
            get
            {
                return this.relationCollection;
            }
        }

        [DefaultValue(0)]
        public SerializationFormat RemotingFormat
        {
            get
            {
                return this._remotingFormat;
            }
            set
            {
                if ((value != SerializationFormat.Binary) && (value != SerializationFormat.Xml))
                {
                    throw ExceptionBuilder.InvalidRemotingFormat(value);
                }
                this._remotingFormat = value;
                for (int i = 0; i < this.Tables.Count; i++)
                {
                    this.Tables[i].RemotingFormat = value;
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual System.Data.SchemaSerializationMode SchemaSerializationMode
        {
            get
            {
                return System.Data.SchemaSerializationMode.IncludeSchema;
            }
            set
            {
                if (value != System.Data.SchemaSerializationMode.IncludeSchema)
                {
                    throw ExceptionBuilder.CannotChangeSchemaSerializationMode();
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public override ISite Site
        {
            get
            {
                return base.Site;
            }
            set
            {
                ISite site = this.Site;
                if ((value == null) && (site != null))
                {
                    IContainer container = site.Container;
                    if (container != null)
                    {
                        for (int i = 0; i < this.Tables.Count; i++)
                        {
                            if (this.Tables[i].Site != null)
                            {
                                container.Remove(this.Tables[i]);
                            }
                        }
                    }
                }
                base.Site = value;
            }
        }

        bool IListSource.ContainsListCollection
        {
            get
            {
                return true;
            }
        }

        [System.Data.ResCategory("DataCategory_Data"), System.Data.ResDescription("DataSetTablesDescr"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public DataTableCollection Tables
        {
            get
            {
                return this.tableCollection;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TableChanges
        {
            private BitArray _rowChanges;
            private int _hasChanges;
            internal TableChanges(int rowCount)
            {
                this._rowChanges = new BitArray(rowCount);
                this._hasChanges = 0;
            }

            internal int HasChanges
            {
                get
                {
                    return this._hasChanges;
                }
                set
                {
                    this._hasChanges = value;
                }
            }
            internal bool this[int index]
            {
                get
                {
                    return this._rowChanges[index];
                }
                set
                {
                    this._rowChanges[index] = value;
                    this._hasChanges++;
                }
            }
        }
    }
}

