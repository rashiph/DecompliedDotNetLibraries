namespace System.Xml
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Diagnostics;
    using System.IO;
    using System.Security.Permissions;
    using System.Xml.XPath;

    [Obsolete("XmlDataDocument class will be removed in a future release."), HostProtection(SecurityAction.LinkDemand, Synchronization=true)]
    public class XmlDataDocument : XmlDocument
    {
        internal XmlAttribute attrXml;
        private ElementState autoFoliationState;
        private bool bForceExpandEntity;
        internal bool bHasXSINIL;
        internal bool bLoadFromDataSet;
        private ArrayList columnChangeList;
        private int countAddPointer;
        private System.Data.DataSet dataSet;
        private bool fAssociateDataRow;
        private bool fBoundToDataSet;
        private bool fBoundToDocument;
        private bool fDataRowCreatedSpecial;
        private object foliationLock;
        private bool ignoreDataSetEvents;
        private bool ignoreXmlEvents;
        private bool isFoliationEnabled;
        private DataSetMapper mapper;
        private bool optimizeStorage;
        internal Hashtable pointers;
        private DataRowState rollbackState;
        internal const string XSI = "xsi";
        internal const string XSI_NIL = "xsi:nil";

        public XmlDataDocument() : base(new XmlDataImplementation())
        {
            this.Init();
            this.AttachDataSet(new System.Data.DataSet());
            this.dataSet.EnforceConstraints = false;
        }

        public XmlDataDocument(System.Data.DataSet dataset) : base(new XmlDataImplementation())
        {
            this.Init(dataset);
        }

        internal XmlDataDocument(XmlImplementation imp) : base(imp)
        {
        }

        internal void AddPointer(IXmlDataVirtualNode pointer)
        {
            lock (this.pointers)
            {
                this.countAddPointer++;
                if (this.countAddPointer >= 5)
                {
                    ArrayList list = new ArrayList();
                    foreach (DictionaryEntry entry in this.pointers)
                    {
                        IXmlDataVirtualNode node = (IXmlDataVirtualNode) entry.Value;
                        if (!node.IsInUse())
                        {
                            list.Add(node);
                        }
                    }
                    for (int i = 0; i < list.Count; i++)
                    {
                        this.pointers.Remove(list[i]);
                    }
                    this.countAddPointer = 0;
                }
                this.pointers[pointer] = pointer;
            }
        }

        [Conditional("DEBUG")]
        private void AssertLiveRows(XmlNode node)
        {
            bool isFoliationEnabled = this.IsFoliationEnabled;
            this.IsFoliationEnabled = false;
            try
            {
                XmlBoundElement currentNode = node as XmlBoundElement;
                if (currentNode != null)
                {
                    DataRow row = currentNode.Row;
                }
                TreeIterator iterator = new TreeIterator(node);
                for (bool flag = iterator.NextRowElement(); flag; flag = iterator.NextRowElement())
                {
                    currentNode = iterator.CurrentNode as XmlBoundElement;
                }
            }
            finally
            {
                this.IsFoliationEnabled = isFoliationEnabled;
            }
        }

        [Conditional("DEBUG")]
        private void AssertNonLiveRows(XmlNode node)
        {
            bool isFoliationEnabled = this.IsFoliationEnabled;
            this.IsFoliationEnabled = false;
            try
            {
                XmlBoundElement currentNode = node as XmlBoundElement;
                if (currentNode != null)
                {
                    DataRow row = currentNode.Row;
                }
                TreeIterator iterator = new TreeIterator(node);
                for (bool flag = iterator.NextRowElement(); flag; flag = iterator.NextRowElement())
                {
                    currentNode = iterator.CurrentNode as XmlBoundElement;
                }
            }
            finally
            {
                this.IsFoliationEnabled = isFoliationEnabled;
            }
        }

        [Conditional("DEBUG")]
        internal void AssertPointerPresent(IXmlDataVirtualNode pointer)
        {
        }

        private XmlBoundElement AttachBoundElementToDataRow(DataRow row)
        {
            DataTable table = row.Table;
            XmlBoundElement e = new XmlBoundElement(string.Empty, table.EncodedTableName, table.Namespace, this) {
                IsEmpty = false
            };
            this.Bind(row, e);
            e.ElementState = ElementState.Defoliated;
            return e;
        }

        private void AttachDataSet(System.Data.DataSet ds)
        {
            if (ds.FBoundToDocument)
            {
                throw new ArgumentException(System.Data.Res.GetString("DataDom_MultipleDataSet"));
            }
            ds.FBoundToDocument = true;
            this.dataSet = ds;
            this.BindSpecialListeners();
        }

        private void Bind(bool fLoadFromDataSet)
        {
            this.ignoreDataSetEvents = true;
            this.ignoreXmlEvents = true;
            this.mapper.SetupMapping(this, this.dataSet);
            if (base.DocumentElement != null)
            {
                this.LoadDataSetFromTree();
                this.BindListeners();
            }
            else if (fLoadFromDataSet)
            {
                this.bLoadFromDataSet = true;
                this.LoadTreeFromDataSet(this.DataSet);
                this.BindListeners();
            }
            this.ignoreDataSetEvents = false;
            this.ignoreXmlEvents = false;
        }

        internal void Bind(DataRow r, XmlBoundElement e)
        {
            r.Element = e;
            e.Row = r;
        }

        private void BindForLoad()
        {
            this.ignoreDataSetEvents = true;
            this.mapper.SetupMapping(this, this.dataSet);
            if (this.dataSet.Tables.Count > 0)
            {
                this.LoadDataSetFromTree();
            }
            this.BindListeners();
            this.ignoreDataSetEvents = false;
        }

        private void BindListeners()
        {
            this.BindToDocument();
            this.BindToDataSet();
        }

        private void BindSpecialListeners()
        {
            this.dataSet.DataRowCreated += new DataRowCreatedEventHandler(this.OnDataRowCreatedSpecial);
            this.fDataRowCreatedSpecial = true;
        }

        private void BindToDataSet()
        {
            if (!this.fBoundToDataSet)
            {
                if (this.fDataRowCreatedSpecial)
                {
                    this.UnBindSpecialListeners();
                }
                this.dataSet.Tables.CollectionChanging += new CollectionChangeEventHandler(this.OnDataSetTablesChanging);
                this.dataSet.Relations.CollectionChanging += new CollectionChangeEventHandler(this.OnDataSetRelationsChanging);
                this.dataSet.DataRowCreated += new DataRowCreatedEventHandler(this.OnDataRowCreated);
                this.dataSet.PropertyChanging += new PropertyChangedEventHandler(this.OnDataSetPropertyChanging);
                this.dataSet.ClearFunctionCalled += new DataSetClearEventhandler(this.OnClearCalled);
                if (this.dataSet.Tables.Count > 0)
                {
                    foreach (DataTable table in this.dataSet.Tables)
                    {
                        this.BindToTable(table);
                    }
                }
                foreach (DataRelation relation in this.dataSet.Relations)
                {
                    relation.PropertyChanging += new PropertyChangedEventHandler(this.OnRelationPropertyChanging);
                }
                this.fBoundToDataSet = true;
            }
        }

        private void BindToDocument()
        {
            if (!this.fBoundToDocument)
            {
                base.NodeInserting += new XmlNodeChangedEventHandler(this.OnNodeInserting);
                base.NodeInserted += new XmlNodeChangedEventHandler(this.OnNodeInserted);
                base.NodeRemoving += new XmlNodeChangedEventHandler(this.OnNodeRemoving);
                base.NodeRemoved += new XmlNodeChangedEventHandler(this.OnNodeRemoved);
                base.NodeChanging += new XmlNodeChangedEventHandler(this.OnNodeChanging);
                base.NodeChanged += new XmlNodeChangedEventHandler(this.OnNodeChanged);
                this.fBoundToDocument = true;
            }
        }

        private void BindToTable(DataTable t)
        {
            t.ColumnChanged += new DataColumnChangeEventHandler(this.OnColumnChanged);
            t.RowChanging += new DataRowChangeEventHandler(this.OnRowChanging);
            t.RowChanged += new DataRowChangeEventHandler(this.OnRowChanged);
            t.RowDeleting += new DataRowChangeEventHandler(this.OnRowChanging);
            t.RowDeleted += new DataRowChangeEventHandler(this.OnRowChanged);
            t.PropertyChanging += new PropertyChangedEventHandler(this.OnTablePropertyChanging);
            t.Columns.CollectionChanging += new CollectionChangeEventHandler(this.OnTableColumnsChanging);
            foreach (DataColumn column in t.Columns)
            {
                column.PropertyChanging += new PropertyChangedEventHandler(this.OnColumnPropertyChanging);
            }
        }

        public override XmlNode CloneNode(bool deep)
        {
            XmlDataDocument document = (XmlDataDocument) base.CloneNode(false);
            document.Init(this.DataSet.Clone());
            document.dataSet.EnforceConstraints = this.dataSet.EnforceConstraints;
            if (deep)
            {
                DataPointer other = new DataPointer(this, this);
                try
                {
                    other.AddPointer();
                    for (bool flag = other.MoveToFirstChild(); flag; flag = other.MoveToNextSibling())
                    {
                        XmlNode node;
                        if (other.NodeType == XmlNodeType.Element)
                        {
                            node = document.CloneTree(other);
                        }
                        else
                        {
                            node = document.CloneNode(other);
                        }
                        document.AppendChild(node);
                    }
                }
                finally
                {
                    other.SetNoLongerUse();
                }
            }
            return document;
        }

        private XmlNode CloneNode(DataPointer dp)
        {
            switch (dp.NodeType)
            {
                case XmlNodeType.Element:
                    return this.CreateElement(dp.Prefix, dp.LocalName, dp.NamespaceURI);

                case XmlNodeType.Attribute:
                    return this.CreateAttribute(dp.Prefix, dp.LocalName, dp.NamespaceURI);

                case XmlNodeType.Text:
                    return this.CreateTextNode(dp.Value);

                case XmlNodeType.CDATA:
                    return this.CreateCDataSection(dp.Value);

                case XmlNodeType.EntityReference:
                    return this.CreateEntityReference(dp.Name);

                case XmlNodeType.ProcessingInstruction:
                    return this.CreateProcessingInstruction(dp.Name, dp.Value);

                case XmlNodeType.Comment:
                    return this.CreateComment(dp.Value);

                case XmlNodeType.DocumentType:
                    return this.CreateDocumentType(dp.Name, dp.PublicId, dp.SystemId, dp.InternalSubset);

                case XmlNodeType.DocumentFragment:
                    return this.CreateDocumentFragment();

                case XmlNodeType.Whitespace:
                    return this.CreateWhitespace(dp.Value);

                case XmlNodeType.SignificantWhitespace:
                    return this.CreateSignificantWhitespace(dp.Value);

                case XmlNodeType.XmlDeclaration:
                    return this.CreateXmlDeclaration(dp.Version, dp.Encoding, dp.Standalone);
            }
            throw new InvalidOperationException(System.Data.Res.GetString("DataDom_CloneNode", new object[] { dp.NodeType.ToString() }));
        }

        internal XmlNode CloneTree(DataPointer other)
        {
            XmlNode node;
            this.EnsurePopulatedMode();
            bool ignoreDataSetEvents = this.ignoreDataSetEvents;
            bool ignoreXmlEvents = this.ignoreXmlEvents;
            bool isFoliationEnabled = this.IsFoliationEnabled;
            bool fAssociateDataRow = this.fAssociateDataRow;
            try
            {
                this.ignoreDataSetEvents = true;
                this.ignoreXmlEvents = true;
                this.IsFoliationEnabled = false;
                this.fAssociateDataRow = false;
                node = this.CloneTreeInternal(other);
                this.LoadRows(null, node);
                this.SyncRows(null, node, false);
            }
            finally
            {
                this.ignoreDataSetEvents = ignoreDataSetEvents;
                this.ignoreXmlEvents = ignoreXmlEvents;
                this.IsFoliationEnabled = isFoliationEnabled;
                this.fAssociateDataRow = fAssociateDataRow;
            }
            return node;
        }

        private XmlNode CloneTreeInternal(DataPointer other)
        {
            XmlNode node = this.CloneNode(other);
            DataPointer pointer = new DataPointer(other);
            try
            {
                pointer.AddPointer();
                if (node.NodeType == XmlNodeType.Element)
                {
                    int attributeCount = pointer.AttributeCount;
                    for (int i = 0; i < attributeCount; i++)
                    {
                        pointer.MoveToOwnerElement();
                        if (pointer.MoveToAttribute(i))
                        {
                            node.Attributes.Append((XmlAttribute) this.CloneTreeInternal(pointer));
                        }
                    }
                    pointer.MoveTo(other);
                }
                for (bool flag = pointer.MoveToFirstChild(); flag; flag = pointer.MoveToNextSibling())
                {
                    node.AppendChild(this.CloneTreeInternal(pointer));
                }
            }
            finally
            {
                pointer.SetNoLongerUse();
            }
            return node;
        }

        public override XmlElement CreateElement(string prefix, string localName, string namespaceURI)
        {
            if (prefix == null)
            {
                prefix = string.Empty;
            }
            if (namespaceURI == null)
            {
                namespaceURI = string.Empty;
            }
            if (!this.fAssociateDataRow)
            {
                return new XmlBoundElement(prefix, localName, namespaceURI, this);
            }
            this.EnsurePopulatedMode();
            DataTable table = this.mapper.SearchMatchingTableSchema(localName, namespaceURI);
            if (table == null)
            {
                return new XmlBoundElement(prefix, localName, namespaceURI, this);
            }
            DataRow row = table.CreateEmptyRow();
            foreach (DataColumn column in table.Columns)
            {
                if (column.ColumnMapping != MappingType.Hidden)
                {
                    SetRowValueToNull(row, column);
                }
            }
            XmlBoundElement element = row.Element;
            element.Prefix = prefix;
            return element;
        }

        public override XmlEntityReference CreateEntityReference(string name)
        {
            throw new NotSupportedException(System.Data.Res.GetString("DataDom_NotSupport_EntRef"));
        }

        protected override XPathNavigator CreateNavigator(XmlNode node)
        {
            if (XPathNodePointer.xmlNodeType_To_XpathNodeType_Map[(int) node.NodeType] == -1)
            {
                return null;
            }
            if (IsTextNode(node.NodeType))
            {
                XmlNode parentNode = node.ParentNode;
                if ((parentNode != null) && (parentNode.NodeType == XmlNodeType.Attribute))
                {
                    return null;
                }
                for (XmlNode node2 = node.PreviousSibling; (node2 != null) && IsTextNode(node2.NodeType); node2 = this.SafePreviousSibling(node))
                {
                    node = node2;
                }
            }
            return new DataDocumentXPathNavigator(this, node);
        }

        private void DefoliateRegion(XmlBoundElement rowElem)
        {
            if ((this.optimizeStorage && (rowElem.ElementState == ElementState.WeakFoliation)) && this.mapper.IsRegionRadical(rowElem))
            {
                bool ignoreXmlEvents = this.IgnoreXmlEvents;
                this.IgnoreXmlEvents = true;
                rowElem.ElementState = ElementState.Defoliating;
                try
                {
                    XmlNode nextSibling;
                    rowElem.RemoveAllAttributes();
                    for (XmlNode node = rowElem.FirstChild; node != null; node = nextSibling)
                    {
                        nextSibling = node.NextSibling;
                        XmlBoundElement element = node as XmlBoundElement;
                        if ((element != null) && (element.Row != null))
                        {
                            break;
                        }
                        rowElem.RemoveChild(node);
                    }
                    rowElem.ElementState = ElementState.Defoliated;
                }
                finally
                {
                    this.IgnoreXmlEvents = ignoreXmlEvents;
                }
            }
        }

        private XmlElement DemoteDocumentElement()
        {
            XmlElement documentElement = base.DocumentElement;
            this.RemoveChild(documentElement);
            XmlElement element = this.EnsureDocumentElement();
            element.AppendChild(documentElement);
            return element;
        }

        private void EnsureDisconnectedDataRow(XmlBoundElement rowElem)
        {
            DataRow row = rowElem.Row;
            switch (row.RowState)
            {
                case DataRowState.Detached:
                    this.SetNestedParentRegion(rowElem);
                    return;

                case DataRowState.Unchanged:
                case DataRowState.Modified:
                    this.EnsureFoliation(rowElem, ElementState.WeakFoliation);
                    row.Delete();
                    return;

                case (DataRowState.Unchanged | DataRowState.Detached):
                case DataRowState.Deleted:
                    break;

                case DataRowState.Added:
                    this.EnsureFoliation(rowElem, ElementState.WeakFoliation);
                    row.Delete();
                    this.SetNestedParentRegion(rowElem);
                    break;

                default:
                    return;
            }
        }

        private XmlElement EnsureDocumentElement()
        {
            XmlElement documentElement = base.DocumentElement;
            if (documentElement == null)
            {
                string localName = XmlConvert.EncodeLocalName(this.DataSet.DataSetName);
                if ((localName == null) || (localName.Length == 0))
                {
                    localName = "Xml";
                }
                string namespaceURI = this.DataSet.Namespace;
                if (namespaceURI == null)
                {
                    namespaceURI = string.Empty;
                }
                documentElement = new XmlBoundElement(string.Empty, localName, namespaceURI, this);
                this.AppendChild(documentElement);
            }
            return documentElement;
        }

        private void EnsureFoliation(XmlBoundElement rowElem, ElementState foliation)
        {
            if (!rowElem.IsFoliated)
            {
                this.ForceFoliation(rowElem, foliation);
            }
        }

        private XmlElement EnsureNonRowDocumentElement()
        {
            XmlElement documentElement = base.DocumentElement;
            if (documentElement == null)
            {
                return this.EnsureDocumentElement();
            }
            if (this.GetRowFromElement(documentElement) == null)
            {
                return documentElement;
            }
            return this.DemoteDocumentElement();
        }

        private void EnsurePopulatedMode()
        {
            if (this.fDataRowCreatedSpecial)
            {
                this.UnBindSpecialListeners();
                this.mapper.SetupMapping(this, this.dataSet);
                this.BindListeners();
                this.fAssociateDataRow = true;
            }
        }

        private DataColumn FindAssociatedParentColumn(DataRelation relation, DataColumn childCol)
        {
            DataColumn[] columnsReference = relation.ChildKey.ColumnsReference;
            for (int i = 0; i < columnsReference.Length; i++)
            {
                if (childCol == columnsReference[i])
                {
                    return relation.ParentKey.ColumnsReference[i];
                }
            }
            return null;
        }

        private void FixNestedChildren(DataRow row, XmlElement rowElement)
        {
            foreach (DataRelation relation in this.GetNestedChildRelations(row))
            {
                foreach (DataRow row2 in row.GetChildRows(relation))
                {
                    XmlElement oldChild = row2.Element;
                    if ((oldChild != null) && (oldChild.ParentNode != rowElement))
                    {
                        oldChild.ParentNode.RemoveChild(oldChild);
                        rowElement.AppendChild(oldChild);
                    }
                }
            }
        }

        private void Foliate(XmlElement element)
        {
            if (element is XmlBoundElement)
            {
                ((XmlBoundElement) element).Foliate(ElementState.WeakFoliation);
            }
        }

        internal void Foliate(XmlBoundElement node, ElementState newState)
        {
            if (this.IsFoliationEnabled)
            {
                if (node.ElementState == ElementState.Defoliated)
                {
                    this.ForceFoliation(node, newState);
                }
                else if ((node.ElementState == ElementState.WeakFoliation) && (newState == ElementState.StrongFoliation))
                {
                    node.ElementState = newState;
                }
            }
        }

        private void FoliateIfDataPointers(DataRow row, XmlElement rowElement)
        {
            if (!this.IsFoliated(rowElement) && this.HasPointers(rowElement))
            {
                bool isFoliationEnabled = this.IsFoliationEnabled;
                this.IsFoliationEnabled = true;
                try
                {
                    this.Foliate(rowElement);
                }
                finally
                {
                    this.IsFoliationEnabled = isFoliationEnabled;
                }
            }
        }

        private void ForceFoliation(XmlBoundElement node, ElementState newState)
        {
            lock (this.foliationLock)
            {
                if (node.ElementState == ElementState.Defoliated)
                {
                    node.ElementState = ElementState.Foliating;
                    bool ignoreXmlEvents = this.IgnoreXmlEvents;
                    this.IgnoreXmlEvents = true;
                    try
                    {
                        XmlNode refChild = null;
                        DataRow row = node.Row;
                        DataRowVersion version = (row.RowState == DataRowState.Detached) ? DataRowVersion.Proposed : DataRowVersion.Current;
                        foreach (DataColumn column in row.Table.Columns)
                        {
                            if (!this.IsNotMapped(column))
                            {
                                object obj2 = row[column, version];
                                if (!Convert.IsDBNull(obj2))
                                {
                                    if (column.ColumnMapping == MappingType.Attribute)
                                    {
                                        node.SetAttribute(column.EncodedColumnName, column.Namespace, column.ConvertObjectToXml(obj2));
                                    }
                                    else
                                    {
                                        XmlNode newChild = null;
                                        if (column.ColumnMapping == MappingType.Element)
                                        {
                                            newChild = new XmlBoundElement(string.Empty, column.EncodedColumnName, column.Namespace, this);
                                            newChild.AppendChild(this.CreateTextNode(column.ConvertObjectToXml(obj2)));
                                            if (refChild != null)
                                            {
                                                node.InsertAfter(newChild, refChild);
                                            }
                                            else if (node.FirstChild != null)
                                            {
                                                node.InsertBefore(newChild, node.FirstChild);
                                            }
                                            else
                                            {
                                                node.AppendChild(newChild);
                                            }
                                            refChild = newChild;
                                        }
                                        else
                                        {
                                            newChild = this.CreateTextNode(column.ConvertObjectToXml(obj2));
                                            if (node.FirstChild != null)
                                            {
                                                node.InsertBefore(newChild, node.FirstChild);
                                            }
                                            else
                                            {
                                                node.AppendChild(newChild);
                                            }
                                            if (refChild == null)
                                            {
                                                refChild = newChild;
                                            }
                                        }
                                    }
                                }
                                else if (column.ColumnMapping == MappingType.SimpleContent)
                                {
                                    XmlAttribute newAttr = this.CreateAttribute("xsi", "nil", "http://www.w3.org/2001/XMLSchema-instance");
                                    newAttr.Value = "true";
                                    node.SetAttributeNode(newAttr);
                                    this.bHasXSINIL = true;
                                }
                            }
                        }
                    }
                    finally
                    {
                        this.IgnoreXmlEvents = ignoreXmlEvents;
                        node.ElementState = newState;
                    }
                    this.OnFoliated(node);
                }
            }
        }

        private XmlNode GetColumnInsertAfterLocation(DataRow row, DataColumn col, XmlBoundElement rowElement)
        {
            XmlNode n = null;
            XmlNode node2 = null;
            if (this.IsTextOnly(col))
            {
                return null;
            }
            n = rowElement.FirstChild;
            while (n != null)
            {
                if (!IsTextLikeNode(n))
                {
                    break;
                }
                node2 = n;
                n = n.NextSibling;
            }
            while (n != null)
            {
                if (n.NodeType != XmlNodeType.Element)
                {
                    return node2;
                }
                XmlElement e = n as XmlElement;
                if (this.mapper.GetRowFromElement(e) != null)
                {
                    return node2;
                }
                object columnSchemaForNode = this.mapper.GetColumnSchemaForNode(rowElement, n);
                if (((columnSchemaForNode == null) || !(columnSchemaForNode is DataColumn)) || (((DataColumn) columnSchemaForNode).Ordinal > col.Ordinal))
                {
                    return node2;
                }
                node2 = n;
                n = n.NextSibling;
            }
            return node2;
        }

        public override XmlElement GetElementById(string elemId)
        {
            throw new NotSupportedException(System.Data.Res.GetString("DataDom_NotSupport_GetElementById"));
        }

        public XmlElement GetElementFromRow(DataRow r)
        {
            return r.Element;
        }

        public override XmlNodeList GetElementsByTagName(string name)
        {
            XmlNodeList elementsByTagName = base.GetElementsByTagName(name);
            int count = elementsByTagName.Count;
            return elementsByTagName;
        }

        private ArrayList GetNestedChildRelations(DataRow row)
        {
            ArrayList list = new ArrayList();
            foreach (DataRelation relation in row.Table.ChildRelations)
            {
                if (relation.Nested)
                {
                    list.Add(relation);
                }
            }
            return list;
        }

        private DataRow GetNestedParent(DataRow row)
        {
            DataRelation nestedParentRelation = GetNestedParentRelation(row);
            if (nestedParentRelation != null)
            {
                return row.GetParentRow(nestedParentRelation);
            }
            return null;
        }

        private static DataRelation GetNestedParentRelation(DataRow row)
        {
            DataRelation[] nestedParentRelations = row.Table.NestedParentRelations;
            if (nestedParentRelations.Length == 0)
            {
                return null;
            }
            return nestedParentRelations[0];
        }

        public DataRow GetRowFromElement(XmlElement e)
        {
            return this.mapper.GetRowFromElement(e);
        }

        private XmlNode GetRowInsertBeforeLocation(DataRow row, XmlElement rowElement, XmlNode parentElement)
        {
            DataRow row2 = row;
            int num = 0;
            num = 0;
            while (num < row.Table.Rows.Count)
            {
                if (row == row.Table.Rows[num])
                {
                    break;
                }
                num++;
            }
            int num2 = num;
            DataRow nestedParent = this.GetNestedParent(row);
            num = num2 + 1;
            while (num < row.Table.Rows.Count)
            {
                row2 = row.Table.Rows[num];
                if ((this.GetNestedParent(row2) == nestedParent) && (this.GetElementFromRow(row2).ParentNode == parentElement))
                {
                    break;
                }
                num++;
            }
            if (num < row.Table.Rows.Count)
            {
                return this.GetElementFromRow(row2);
            }
            return null;
        }

        private DataColumn GetTextOnlyColumn(DataRow row)
        {
            return row.Table.XmlText;
        }

        internal bool HasPointers(XmlNode node)
        {
            bool flag;
        Label_0000:;
            try
            {
                if (this.pointers.Count > 0)
                {
                    foreach (DictionaryEntry entry in this.pointers)
                    {
                        if (((IXmlDataVirtualNode) entry.Value).IsOnNode(node))
                        {
                            return true;
                        }
                    }
                }
                flag = false;
            }
            catch (Exception exception)
            {
                if (!ADP.IsCatchableExceptionType(exception))
                {
                    throw;
                }
                goto Label_0000;
            }
            return flag;
        }

        private void Init()
        {
            this.pointers = new Hashtable();
            this.countAddPointer = 0;
            this.columnChangeList = new ArrayList();
            this.ignoreDataSetEvents = false;
            this.isFoliationEnabled = true;
            this.optimizeStorage = true;
            this.fDataRowCreatedSpecial = false;
            this.autoFoliationState = ElementState.StrongFoliation;
            this.fAssociateDataRow = true;
            this.mapper = new DataSetMapper();
            this.foliationLock = new object();
            this.ignoreXmlEvents = true;
            this.attrXml = this.CreateAttribute("xmlns", "xml", XPathNodePointer.s_strReservedXmlns);
            this.attrXml.Value = XPathNodePointer.s_strReservedXml;
            this.ignoreXmlEvents = false;
        }

        private void Init(System.Data.DataSet ds)
        {
            if (ds == null)
            {
                throw new ArgumentException(System.Data.Res.GetString("DataDom_DataSetNull"));
            }
            this.Init();
            if (ds.FBoundToDocument)
            {
                throw new ArgumentException(System.Data.Res.GetString("DataDom_MultipleDataSet"));
            }
            ds.FBoundToDocument = true;
            this.dataSet = ds;
            this.Bind(true);
        }

        private bool IsConnected(XmlNode node)
        {
            while (node != null)
            {
                if (node == this)
                {
                    return true;
                }
                XmlAttribute attribute = node as XmlAttribute;
                if (attribute != null)
                {
                    node = attribute.OwnerElement;
                }
                else
                {
                    node = node.ParentNode;
                }
            }
            return false;
        }

        private bool IsFoliated(XmlBoundElement be)
        {
            return be.IsFoliated;
        }

        private bool IsFoliated(XmlElement element)
        {
            if (element is XmlBoundElement)
            {
                return ((XmlBoundElement) element).IsFoliated;
            }
            return true;
        }

        internal bool IsNotMapped(DataColumn c)
        {
            return DataSetMapper.IsNotMapped(c);
        }

        private bool IsRowLive(DataRow row)
        {
            return ((row.RowState & (DataRowState.Modified | DataRowState.Added | DataRowState.Unchanged)) != 0);
        }

        private bool IsSame(DataColumn c, int recNo1, int recNo2)
        {
            return (c.Compare(recNo1, recNo2) == 0);
        }

        private bool IsSelfRelatedDataTable(DataTable rootTable)
        {
            List<DataTable> list = new List<DataTable>();
            bool flag = false;
            foreach (DataRelation relation2 in rootTable.ChildRelations)
            {
                DataTable childTable = relation2.ChildTable;
                if (childTable == rootTable)
                {
                    flag = true;
                    break;
                }
                if (!list.Contains(childTable))
                {
                    list.Add(childTable);
                }
            }
            if (!flag)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    foreach (DataRelation relation in list[i].ChildRelations)
                    {
                        DataTable item = relation.ChildTable;
                        if (item == rootTable)
                        {
                            flag = true;
                            break;
                        }
                        if (!list.Contains(item))
                        {
                            list.Add(item);
                        }
                    }
                    if (flag)
                    {
                        return flag;
                    }
                }
            }
            return flag;
        }

        internal static bool IsTextLikeNode(XmlNode n)
        {
            switch (n.NodeType)
            {
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    return true;

                case XmlNodeType.EntityReference:
                    return false;
            }
            return false;
        }

        internal static bool IsTextNode(XmlNodeType nt)
        {
            switch (nt)
            {
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    return true;
            }
            return false;
        }

        internal bool IsTextOnly(DataColumn c)
        {
            return (c.ColumnMapping == MappingType.SimpleContent);
        }

        public override void Load(Stream inStream)
        {
            this.bForceExpandEntity = true;
            base.Load(inStream);
            this.bForceExpandEntity = false;
        }

        public override void Load(TextReader txtReader)
        {
            this.bForceExpandEntity = true;
            base.Load(txtReader);
            this.bForceExpandEntity = false;
        }

        public override void Load(string filename)
        {
            this.bForceExpandEntity = true;
            base.Load(filename);
            this.bForceExpandEntity = false;
        }

        public override void Load(XmlReader reader)
        {
            if (this.FirstChild != null)
            {
                throw new InvalidOperationException(System.Data.Res.GetString("DataDom_MultipleLoad"));
            }
            try
            {
                this.ignoreXmlEvents = true;
                if (this.fDataRowCreatedSpecial)
                {
                    this.UnBindSpecialListeners();
                }
                this.fAssociateDataRow = false;
                this.isFoliationEnabled = false;
                if (this.bForceExpandEntity)
                {
                    ((XmlTextReader) reader).EntityHandling = EntityHandling.ExpandEntities;
                }
                base.Load(reader);
                this.BindForLoad();
            }
            finally
            {
                this.ignoreXmlEvents = false;
                this.isFoliationEnabled = true;
                this.autoFoliationState = ElementState.StrongFoliation;
                this.fAssociateDataRow = true;
            }
        }

        private void LoadDataSetFromTree()
        {
            this.ignoreDataSetEvents = true;
            this.ignoreXmlEvents = true;
            bool isFoliationEnabled = this.IsFoliationEnabled;
            this.IsFoliationEnabled = false;
            bool enforceConstraints = this.dataSet.EnforceConstraints;
            this.dataSet.EnforceConstraints = false;
            try
            {
                this.LoadRows(null, base.DocumentElement);
                this.SyncRows(null, base.DocumentElement, true);
                this.dataSet.EnforceConstraints = enforceConstraints;
            }
            finally
            {
                this.ignoreDataSetEvents = false;
                this.ignoreXmlEvents = false;
                this.IsFoliationEnabled = isFoliationEnabled;
            }
        }

        private void LoadRows(XmlBoundElement rowElem, XmlNode node)
        {
            XmlBoundElement elem = node as XmlBoundElement;
            if (elem != null)
            {
                DataTable table = this.mapper.SearchMatchingTableSchema(rowElem, elem);
                if (table != null)
                {
                    DataRow rowFromElement = this.GetRowFromElement(elem);
                    if (elem.ElementState == ElementState.None)
                    {
                        elem.ElementState = ElementState.WeakFoliation;
                    }
                    rowFromElement = table.CreateEmptyRow();
                    this.Bind(rowFromElement, elem);
                    rowElem = elem;
                }
            }
            for (XmlNode node2 = node.FirstChild; node2 != null; node2 = node2.NextSibling)
            {
                this.LoadRows(rowElem, node2);
            }
        }

        private void LoadTreeFromDataSet(System.Data.DataSet ds)
        {
            this.ignoreDataSetEvents = true;
            this.ignoreXmlEvents = true;
            bool isFoliationEnabled = this.IsFoliationEnabled;
            this.IsFoliationEnabled = false;
            this.fAssociateDataRow = false;
            DataTable[] tableArray = this.OrderTables(ds);
            try
            {
                for (int i = 0; i < tableArray.Length; i++)
                {
                    DataTable table = tableArray[i];
                    foreach (DataRow row in table.Rows)
                    {
                        this.AttachBoundElementToDataRow(row);
                        switch (row.RowState)
                        {
                            case DataRowState.Unchanged:
                            case DataRowState.Added:
                            case DataRowState.Modified:
                                this.OnAddRow(row);
                                break;
                        }
                    }
                }
            }
            finally
            {
                this.ignoreDataSetEvents = false;
                this.ignoreXmlEvents = false;
                this.IsFoliationEnabled = isFoliationEnabled;
                this.fAssociateDataRow = true;
            }
        }

        private bool NeedXSI_NilAttr(DataRow row)
        {
            DataTable table = row.Table;
            if (table.xmlText == null)
            {
                return false;
            }
            object obj2 = row[table.xmlText];
            return Convert.IsDBNull(obj2);
        }

        private void OnAddRow(DataRow row)
        {
            XmlBoundElement elementFromRow = (XmlBoundElement) this.GetElementFromRow(row);
            if (this.NeedXSI_NilAttr(row) && !elementFromRow.IsFoliated)
            {
                this.ForceFoliation(elementFromRow, this.AutoFoliationState);
            }
            if ((this.GetRowFromElement(base.DocumentElement) != null) && (this.GetNestedParent(row) == null))
            {
                this.DemoteDocumentElement();
            }
            this.EnsureDocumentElement().AppendChild(elementFromRow);
            this.FixNestedChildren(row, elementFromRow);
            this.OnNestedParentChange(row, elementFromRow, null);
        }

        internal void OnClearCalled(object oDataSet, DataTable table)
        {
            throw new NotSupportedException(System.Data.Res.GetString("DataDom_NotSupport_Clear"));
        }

        private void OnColumnChanged(object sender, DataColumnChangeEventArgs args)
        {
            if (!this.ignoreDataSetEvents)
            {
                bool ignoreXmlEvents = this.ignoreXmlEvents;
                this.ignoreXmlEvents = true;
                bool isFoliationEnabled = this.IsFoliationEnabled;
                this.IsFoliationEnabled = false;
                try
                {
                    DataRow row = args.Row;
                    DataColumn col = args.Column;
                    object proposedValue = args.ProposedValue;
                    if (row.RowState == DataRowState.Detached)
                    {
                        XmlBoundElement rowElement = row.Element;
                        if (rowElement.IsFoliated)
                        {
                            this.OnColumnValueChanged(row, col, rowElement);
                        }
                    }
                }
                finally
                {
                    this.IsFoliationEnabled = isFoliationEnabled;
                    this.ignoreXmlEvents = ignoreXmlEvents;
                }
            }
        }

        private void OnColumnPropertyChanging(object oColumn, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "ColumnName")
            {
                throw new InvalidOperationException(System.Data.Res.GetString("DataDom_ColumnNameChange"));
            }
            if (args.PropertyName == "Namespace")
            {
                throw new InvalidOperationException(System.Data.Res.GetString("DataDom_ColumnNamespaceChange"));
            }
            if (args.PropertyName == "ColumnMapping")
            {
                throw new InvalidOperationException(System.Data.Res.GetString("DataDom_ColumnMappingChange"));
            }
        }

        private void OnColumnValueChanged(DataRow row, DataColumn col, XmlBoundElement rowElement)
        {
            DataRelation relation;
            if (!this.IsNotMapped(col))
            {
                object obj2 = row[col];
                if (((col.ColumnMapping == MappingType.SimpleContent) && Convert.IsDBNull(obj2)) && !rowElement.IsFoliated)
                {
                    this.ForceFoliation(rowElement, ElementState.WeakFoliation);
                }
                else if (!this.IsFoliated(rowElement))
                {
                    goto Label_0310;
                }
                if (this.IsTextOnly(col))
                {
                    if (Convert.IsDBNull(obj2))
                    {
                        obj2 = string.Empty;
                        XmlAttribute attributeNode = rowElement.GetAttributeNode("xsi:nil");
                        if (attributeNode == null)
                        {
                            attributeNode = this.CreateAttribute("xsi", "nil", "http://www.w3.org/2001/XMLSchema-instance");
                            attributeNode.Value = "true";
                            rowElement.SetAttributeNode(attributeNode);
                            this.bHasXSINIL = true;
                        }
                        else
                        {
                            attributeNode.Value = "true";
                        }
                    }
                    else
                    {
                        XmlAttribute attribute4 = rowElement.GetAttributeNode("xsi:nil");
                        if (attribute4 != null)
                        {
                            attribute4.Value = "false";
                        }
                    }
                    this.ReplaceInitialChildText(rowElement, col.ConvertObjectToXml(obj2));
                }
                else
                {
                    bool flag2 = false;
                    if (col.ColumnMapping != MappingType.Attribute)
                    {
                        RegionIterator iterator = new RegionIterator(rowElement);
                        bool flag = iterator.Next();
                        while (flag)
                        {
                            if (iterator.CurrentNode.NodeType == XmlNodeType.Element)
                            {
                                XmlElement currentNode = (XmlElement) iterator.CurrentNode;
                                XmlBoundElement element3 = currentNode as XmlBoundElement;
                                if ((element3 != null) && (element3.Row != null))
                                {
                                    flag = iterator.NextRight();
                                    continue;
                                }
                                if ((currentNode.LocalName == col.EncodedColumnName) && (currentNode.NamespaceURI == col.Namespace))
                                {
                                    flag2 = true;
                                    if (Convert.IsDBNull(obj2))
                                    {
                                        this.PromoteNonValueChildren(currentNode);
                                        flag = iterator.NextRight();
                                        currentNode.ParentNode.RemoveChild(currentNode);
                                        continue;
                                    }
                                    this.ReplaceInitialChildText(currentNode, col.ConvertObjectToXml(obj2));
                                    XmlAttribute attribute3 = currentNode.GetAttributeNode("xsi:nil");
                                    if (attribute3 != null)
                                    {
                                        attribute3.Value = "false";
                                    }
                                    goto Label_0310;
                                }
                            }
                            flag = iterator.Next();
                        }
                        if (!flag2 && !Convert.IsDBNull(obj2))
                        {
                            XmlElement newChild = new XmlBoundElement(string.Empty, col.EncodedColumnName, col.Namespace, this);
                            newChild.AppendChild(this.CreateTextNode(col.ConvertObjectToXml(obj2)));
                            XmlNode refChild = this.GetColumnInsertAfterLocation(row, col, rowElement);
                            if (refChild != null)
                            {
                                rowElement.InsertAfter(newChild, refChild);
                            }
                            else if (rowElement.FirstChild != null)
                            {
                                rowElement.InsertBefore(newChild, rowElement.FirstChild);
                            }
                            else
                            {
                                rowElement.AppendChild(newChild);
                            }
                        }
                    }
                    else
                    {
                        foreach (XmlAttribute attribute in rowElement.Attributes)
                        {
                            if ((attribute.LocalName == col.EncodedColumnName) && (attribute.NamespaceURI == col.Namespace))
                            {
                                if (Convert.IsDBNull(obj2))
                                {
                                    attribute.OwnerElement.Attributes.Remove(attribute);
                                }
                                else
                                {
                                    attribute.Value = col.ConvertObjectToXml(obj2);
                                }
                                flag2 = true;
                                break;
                            }
                        }
                        if (!flag2 && !Convert.IsDBNull(obj2))
                        {
                            rowElement.SetAttribute(col.EncodedColumnName, col.Namespace, col.ConvertObjectToXml(obj2));
                        }
                    }
                }
            }
        Label_0310:
            relation = GetNestedParentRelation(row);
            if ((relation != null) && relation.ChildKey.ContainsColumn(col))
            {
                this.OnNestedParentChange(row, rowElement, col);
            }
        }

        private void OnColumnValuesChanged(DataRow row, XmlBoundElement rowElement)
        {
            if (this.columnChangeList.Count > 0)
            {
                if (((DataColumn) this.columnChangeList[0]).Table == row.Table)
                {
                    foreach (DataColumn column3 in this.columnChangeList)
                    {
                        this.OnColumnValueChanged(row, column3, rowElement);
                    }
                }
                else
                {
                    foreach (DataColumn column2 in row.Table.Columns)
                    {
                        this.OnColumnValueChanged(row, column2, rowElement);
                    }
                }
            }
            else
            {
                foreach (DataColumn column in row.Table.Columns)
                {
                    this.OnColumnValueChanged(row, column, rowElement);
                }
            }
            this.columnChangeList.Clear();
        }

        internal void OnDataRowCreated(object oDataSet, DataRow row)
        {
            this.OnNewRow(row);
        }

        internal void OnDataRowCreatedSpecial(object oDataSet, DataRow row)
        {
            this.Bind(true);
            this.OnNewRow(row);
        }

        private void OnDataSetPropertyChanging(object oDataSet, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "DataSetName")
            {
                throw new InvalidOperationException(System.Data.Res.GetString("DataDom_DataSetNameChange"));
            }
        }

        private void OnDataSetRelationsChanging(object oRelationsCollection, CollectionChangeEventArgs args)
        {
            DataRelation element = (DataRelation) args.Element;
            if ((element != null) && element.Nested)
            {
                throw new InvalidOperationException(System.Data.Res.GetString("DataDom_DataSetNestedRelationsChange"));
            }
            if (args.Action == CollectionChangeAction.Refresh)
            {
                foreach (DataRelation relation2 in (DataRelationCollection) oRelationsCollection)
                {
                    if (relation2.Nested)
                    {
                        throw new InvalidOperationException(System.Data.Res.GetString("DataDom_DataSetNestedRelationsChange"));
                    }
                }
            }
        }

        private void OnDataSetTablesChanging(object oTablesCollection, CollectionChangeEventArgs args)
        {
            throw new InvalidOperationException(System.Data.Res.GetString("DataDom_DataSetTablesChange"));
        }

        private void OnDeleteRow(DataRow row, XmlBoundElement rowElement)
        {
            if (rowElement == base.DocumentElement)
            {
                this.DemoteDocumentElement();
            }
            this.PromoteInnerRegions(rowElement);
            rowElement.ParentNode.RemoveChild(rowElement);
        }

        private void OnDeletingRow(DataRow row, XmlBoundElement rowElement)
        {
            if (!this.IsFoliated(rowElement))
            {
                bool ignoreXmlEvents = this.IgnoreXmlEvents;
                this.IgnoreXmlEvents = true;
                bool isFoliationEnabled = this.IsFoliationEnabled;
                this.IsFoliationEnabled = true;
                try
                {
                    this.Foliate(rowElement);
                }
                finally
                {
                    this.IsFoliationEnabled = isFoliationEnabled;
                    this.IgnoreXmlEvents = ignoreXmlEvents;
                }
            }
        }

        private void OnFoliated(XmlNode node)
        {
        Label_0000:;
            try
            {
                if (this.pointers.Count > 0)
                {
                    foreach (DictionaryEntry entry in this.pointers)
                    {
                        ((IXmlDataVirtualNode) entry.Value).OnFoliated(node);
                    }
                }
            }
            catch (Exception exception)
            {
                if (!ADP.IsCatchableExceptionType(exception))
                {
                    throw;
                }
                goto Label_0000;
            }
        }

        private void OnNestedParentChange(DataRow child, XmlBoundElement childElement, DataColumn childCol)
        {
            DataRow rowFromElement;
            if ((childElement == base.DocumentElement) || (childElement.ParentNode == null))
            {
                rowFromElement = null;
            }
            else
            {
                rowFromElement = this.GetRowFromElement((XmlElement) childElement.ParentNode);
            }
            DataRow nestedParent = this.GetNestedParent(child);
            if (rowFromElement != nestedParent)
            {
                if (nestedParent != null)
                {
                    this.GetElementFromRow(nestedParent).AppendChild(childElement);
                }
                else
                {
                    DataRelation nestedParentRelation = GetNestedParentRelation(child);
                    if (((childCol == null) || (nestedParentRelation == null)) || Convert.IsDBNull(child[childCol]))
                    {
                        this.EnsureNonRowDocumentElement().AppendChild(childElement);
                    }
                    else
                    {
                        DataColumn column = this.FindAssociatedParentColumn(nestedParentRelation, childCol);
                        object obj2 = column.ConvertValue(child[childCol]);
                        if ((rowFromElement.tempRecord != -1) && (column.CompareValueTo(rowFromElement.tempRecord, obj2) != 0))
                        {
                            this.EnsureNonRowDocumentElement().AppendChild(childElement);
                        }
                    }
                }
            }
        }

        internal void OnNewRow(DataRow row)
        {
            this.AttachBoundElementToDataRow(row);
        }

        private void OnNodeChanged(object sender, XmlNodeChangedEventArgs args)
        {
            if (!this.ignoreXmlEvents)
            {
                bool ignoreDataSetEvents = this.ignoreDataSetEvents;
                bool ignoreXmlEvents = this.ignoreXmlEvents;
                bool isFoliationEnabled = this.IsFoliationEnabled;
                this.ignoreDataSetEvents = true;
                this.ignoreXmlEvents = true;
                this.IsFoliationEnabled = false;
                bool fEnableCascading = this.DataSet.fEnableCascading;
                this.DataSet.fEnableCascading = false;
                try
                {
                    XmlBoundElement rowElem = null;
                    if (this.mapper.GetRegion(args.Node, out rowElem))
                    {
                        this.SynchronizeRowFromRowElement(rowElem);
                    }
                }
                finally
                {
                    this.ignoreDataSetEvents = ignoreDataSetEvents;
                    this.ignoreXmlEvents = ignoreXmlEvents;
                    this.IsFoliationEnabled = isFoliationEnabled;
                    this.DataSet.fEnableCascading = fEnableCascading;
                }
            }
        }

        private void OnNodeChanging(object sender, XmlNodeChangedEventArgs args)
        {
            if (!this.ignoreXmlEvents && this.DataSet.EnforceConstraints)
            {
                throw new InvalidOperationException(System.Data.Res.GetString("DataDom_EnforceConstraintsShouldBeOff"));
            }
        }

        private void OnNodeInserted(object sender, XmlNodeChangedEventArgs args)
        {
            if (!this.ignoreXmlEvents)
            {
                bool ignoreDataSetEvents = this.ignoreDataSetEvents;
                bool ignoreXmlEvents = this.ignoreXmlEvents;
                bool isFoliationEnabled = this.IsFoliationEnabled;
                this.ignoreDataSetEvents = true;
                this.ignoreXmlEvents = true;
                this.IsFoliationEnabled = false;
                bool fEnableCascading = this.DataSet.fEnableCascading;
                this.DataSet.fEnableCascading = false;
                try
                {
                    XmlNode node = args.Node;
                    XmlNode oldParent = args.OldParent;
                    XmlNode newParent = args.NewParent;
                    if (this.IsConnected(newParent))
                    {
                        this.OnNodeInsertedInTree(node);
                    }
                    else
                    {
                        this.OnNodeInsertedInFragment(node);
                    }
                }
                finally
                {
                    this.ignoreDataSetEvents = ignoreDataSetEvents;
                    this.ignoreXmlEvents = ignoreXmlEvents;
                    this.IsFoliationEnabled = isFoliationEnabled;
                    this.DataSet.fEnableCascading = fEnableCascading;
                }
            }
        }

        private void OnNodeInsertedInFragment(XmlNode node)
        {
            XmlBoundElement element;
            if (this.mapper.GetRegion(node, out element))
            {
                if (element == node)
                {
                    this.SetNestedParentRegion(element);
                }
                else
                {
                    ArrayList rowElemList = new ArrayList();
                    this.OnNonRowElementInsertedInFragment(node, element, rowElemList);
                    while (rowElemList.Count > 0)
                    {
                        XmlBoundElement childRowElem = (XmlBoundElement) rowElemList[0];
                        rowElemList.RemoveAt(0);
                        this.SetNestedParentRegion(childRowElem, element);
                    }
                }
            }
        }

        private void OnNodeInsertedInTree(XmlNode node)
        {
            XmlBoundElement element;
            ArrayList rowElemList = new ArrayList();
            if (this.mapper.GetRegion(node, out element))
            {
                if (element == node)
                {
                    this.OnRowElementInsertedInTree(element, rowElemList);
                }
                else
                {
                    this.OnNonRowElementInsertedInTree(node, element, rowElemList);
                }
            }
            else
            {
                TreeIterator iterator = new TreeIterator(node);
                for (bool flag = iterator.NextRowElement(); flag; flag = iterator.NextRightRowElement())
                {
                    rowElemList.Add(iterator.CurrentNode);
                }
            }
            while (rowElemList.Count > 0)
            {
                XmlBoundElement rowElem = (XmlBoundElement) rowElemList[0];
                rowElemList.RemoveAt(0);
                this.OnRowElementInsertedInTree(rowElem, rowElemList);
            }
        }

        private void OnNodeInserting(object sender, XmlNodeChangedEventArgs args)
        {
            if (!this.ignoreXmlEvents && this.DataSet.EnforceConstraints)
            {
                throw new InvalidOperationException(System.Data.Res.GetString("DataDom_EnforceConstraintsShouldBeOff"));
            }
        }

        private void OnNodeRemoved(object sender, XmlNodeChangedEventArgs args)
        {
            if (!this.ignoreXmlEvents)
            {
                bool ignoreDataSetEvents = this.ignoreDataSetEvents;
                bool ignoreXmlEvents = this.ignoreXmlEvents;
                bool isFoliationEnabled = this.IsFoliationEnabled;
                this.ignoreDataSetEvents = true;
                this.ignoreXmlEvents = true;
                this.IsFoliationEnabled = false;
                bool fEnableCascading = this.DataSet.fEnableCascading;
                this.DataSet.fEnableCascading = false;
                try
                {
                    XmlNode node2 = args.Node;
                    XmlNode oldParent = args.OldParent;
                    if (this.IsConnected(oldParent))
                    {
                        this.OnNodeRemovedFromTree(node2, oldParent);
                    }
                    else
                    {
                        this.OnNodeRemovedFromFragment(node2, oldParent);
                    }
                }
                finally
                {
                    this.ignoreDataSetEvents = ignoreDataSetEvents;
                    this.ignoreXmlEvents = ignoreXmlEvents;
                    this.IsFoliationEnabled = isFoliationEnabled;
                    this.DataSet.fEnableCascading = fEnableCascading;
                }
            }
        }

        private void OnNodeRemovedFromFragment(XmlNode node, XmlNode oldParent)
        {
            XmlBoundElement element2;
            if (this.mapper.GetRegion(oldParent, out element2))
            {
                DataRow row = element2.Row;
                if (element2.Row.RowState == DataRowState.Detached)
                {
                    this.SynchronizeRowFromRowElement(element2);
                }
            }
            XmlBoundElement childRowElem = node as XmlBoundElement;
            if ((childRowElem != null) && (childRowElem.Row != null))
            {
                this.SetNestedParentRegion(childRowElem, null);
            }
            else
            {
                TreeIterator iterator = new TreeIterator(node);
                for (bool flag = iterator.NextRowElement(); flag; flag = iterator.NextRightRowElement())
                {
                    XmlBoundElement currentNode = (XmlBoundElement) iterator.CurrentNode;
                    this.SetNestedParentRegion(currentNode, null);
                }
            }
        }

        private void OnNodeRemovedFromTree(XmlNode node, XmlNode oldParent)
        {
            XmlBoundElement element2;
            if (this.mapper.GetRegion(oldParent, out element2))
            {
                this.SynchronizeRowFromRowElement(element2);
            }
            XmlBoundElement rowElem = node as XmlBoundElement;
            if ((rowElem != null) && (rowElem.Row != null))
            {
                this.EnsureDisconnectedDataRow(rowElem);
            }
            TreeIterator iterator = new TreeIterator(node);
            for (bool flag = iterator.NextRowElement(); flag; flag = iterator.NextRowElement())
            {
                rowElem = (XmlBoundElement) iterator.CurrentNode;
                this.EnsureDisconnectedDataRow(rowElem);
            }
        }

        private void OnNodeRemoving(object sender, XmlNodeChangedEventArgs args)
        {
            if (!this.ignoreXmlEvents && this.DataSet.EnforceConstraints)
            {
                throw new InvalidOperationException(System.Data.Res.GetString("DataDom_EnforceConstraintsShouldBeOff"));
            }
        }

        private void OnNonRowElementInsertedInFragment(XmlNode node, XmlBoundElement rowElement, ArrayList rowElemList)
        {
            if (rowElement.Row.RowState == DataRowState.Detached)
            {
                this.SynchronizeRowFromRowElementEx(rowElement, rowElemList);
            }
        }

        private void OnNonRowElementInsertedInTree(XmlNode node, XmlBoundElement rowElement, ArrayList rowElemList)
        {
            DataRow row = rowElement.Row;
            this.SynchronizeRowFromRowElement(rowElement);
            if (rowElemList != null)
            {
                TreeIterator iterator = new TreeIterator(node);
                for (bool flag = iterator.NextRowElement(); flag; flag = iterator.NextRightRowElement())
                {
                    rowElemList.Add(iterator.CurrentNode);
                }
            }
        }

        private void OnRelationPropertyChanging(object oRelationsCollection, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "Nested")
            {
                throw new InvalidOperationException(System.Data.Res.GetString("DataDom_DataSetNestedRelationsChange"));
            }
        }

        private void OnRowChanged(object sender, DataRowChangeEventArgs args)
        {
            if (!this.ignoreDataSetEvents)
            {
                this.ignoreXmlEvents = true;
                bool isFoliationEnabled = this.IsFoliationEnabled;
                this.IsFoliationEnabled = false;
                try
                {
                    DataRow row = args.Row;
                    XmlBoundElement rowElement = row.Element;
                    DataRowAction action = args.Action;
                    switch (action)
                    {
                        case DataRowAction.Delete:
                            this.OnDeleteRow(row, rowElement);
                            return;

                        case DataRowAction.Change:
                            this.OnColumnValuesChanged(row, rowElement);
                            return;

                        case (DataRowAction.Change | DataRowAction.Delete):
                            return;

                        case DataRowAction.Rollback:
                            switch (this.rollbackState)
                            {
                                case DataRowState.Added:
                                    goto Label_0090;

                                case DataRowState.Modified:
                                    goto Label_009F;
                            }
                            return;

                        case DataRowAction.Commit:
                            goto Label_00B3;

                        default:
                            if (action == DataRowAction.Add)
                            {
                                this.OnAddRow(row);
                            }
                            return;
                    }
                    this.OnUndeleteRow(row, rowElement);
                    this.UpdateAllColumns(row, rowElement);
                    return;
                Label_0090:
                    rowElement.ParentNode.RemoveChild(rowElement);
                    return;
                Label_009F:
                    this.OnColumnValuesChanged(row, rowElement);
                    return;
                Label_00B3:
                    if (row.RowState == DataRowState.Detached)
                    {
                        rowElement.RemoveAll();
                    }
                }
                finally
                {
                    this.IsFoliationEnabled = isFoliationEnabled;
                    this.ignoreXmlEvents = false;
                }
            }
        }

        private void OnRowChanging(object sender, DataRowChangeEventArgs args)
        {
            DataRow row = args.Row;
            if ((args.Action == DataRowAction.Delete) && (row.Element != null))
            {
                this.OnDeletingRow(row, row.Element);
            }
            else if (!this.ignoreDataSetEvents)
            {
                bool isFoliationEnabled = this.IsFoliationEnabled;
                this.IsFoliationEnabled = false;
                try
                {
                    DataRowState rollbackState;
                    this.ignoreXmlEvents = true;
                    XmlElement elementFromRow = this.GetElementFromRow(row);
                    int recordFromVersion = -1;
                    int num = -1;
                    if (elementFromRow != null)
                    {
                        switch (args.Action)
                        {
                            case DataRowAction.Change:
                                goto Label_014E;

                            case DataRowAction.Rollback:
                                this.rollbackState = row.RowState;
                                rollbackState = this.rollbackState;
                                if (rollbackState > DataRowState.Added)
                                {
                                    goto Label_00BB;
                                }
                                if ((rollbackState == DataRowState.Detached) || (rollbackState == DataRowState.Added))
                                {
                                }
                                break;
                        }
                    }
                    return;
                Label_00BB:
                    if ((rollbackState != DataRowState.Deleted) && (rollbackState == DataRowState.Modified))
                    {
                        this.columnChangeList.Clear();
                        recordFromVersion = row.GetRecordFromVersion(DataRowVersion.Original);
                        num = row.GetRecordFromVersion(DataRowVersion.Current);
                        foreach (DataColumn column2 in row.Table.Columns)
                        {
                            if (!this.IsSame(column2, recordFromVersion, num))
                            {
                                this.columnChangeList.Add(column2);
                            }
                        }
                    }
                    return;
                Label_014E:
                    this.columnChangeList.Clear();
                    recordFromVersion = row.GetRecordFromVersion(DataRowVersion.Proposed);
                    num = row.GetRecordFromVersion(DataRowVersion.Current);
                    foreach (DataColumn column in row.Table.Columns)
                    {
                        object obj3 = row[column, DataRowVersion.Proposed];
                        object obj2 = row[column, DataRowVersion.Current];
                        if ((Convert.IsDBNull(obj3) && !Convert.IsDBNull(obj2)) && (column.ColumnMapping != MappingType.Hidden))
                        {
                            this.FoliateIfDataPointers(row, elementFromRow);
                        }
                        if (!this.IsSame(column, recordFromVersion, num))
                        {
                            this.columnChangeList.Add(column);
                        }
                    }
                }
                finally
                {
                    this.ignoreXmlEvents = false;
                    this.IsFoliationEnabled = isFoliationEnabled;
                }
            }
        }

        private void OnRowElementInsertedInTree(XmlBoundElement rowElem, ArrayList rowElemList)
        {
            DataRow row = rowElem.Row;
            DataRowState rowState = row.RowState;
            if (rowState != DataRowState.Detached)
            {
                if (rowState != DataRowState.Deleted)
                {
                    return;
                }
            }
            else
            {
                row.Table.Rows.Add(row);
                this.SetNestedParentRegion(rowElem);
                if (rowElemList != null)
                {
                    RegionIterator iterator = new RegionIterator(rowElem);
                    for (bool flag = iterator.NextRowElement(); flag; flag = iterator.NextRightRowElement())
                    {
                        rowElemList.Add(iterator.CurrentNode);
                    }
                }
                return;
            }
            row.RejectChanges();
            this.SynchronizeRowFromRowElement(rowElem, rowElemList);
            this.SetNestedParentRegion(rowElem);
        }

        private void OnTableColumnsChanging(object oColumnsCollection, CollectionChangeEventArgs args)
        {
            throw new InvalidOperationException(System.Data.Res.GetString("DataDom_TableColumnsChange"));
        }

        private void OnTablePropertyChanging(object oTable, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "TableName")
            {
                throw new InvalidOperationException(System.Data.Res.GetString("DataDom_TableNameChange"));
            }
            if (args.PropertyName == "Namespace")
            {
                throw new InvalidOperationException(System.Data.Res.GetString("DataDom_TableNamespaceChange"));
            }
        }

        private void OnUndeleteRow(DataRow row, XmlElement rowElement)
        {
            XmlElement elementFromRow;
            if (rowElement.ParentNode != null)
            {
                rowElement.ParentNode.RemoveChild(rowElement);
            }
            DataRow nestedParent = this.GetNestedParent(row);
            if (nestedParent == null)
            {
                elementFromRow = this.EnsureNonRowDocumentElement();
            }
            else
            {
                elementFromRow = this.GetElementFromRow(nestedParent);
            }
            XmlNode refChild = this.GetRowInsertBeforeLocation(row, rowElement, elementFromRow);
            if (refChild != null)
            {
                elementFromRow.InsertBefore(rowElement, refChild);
            }
            else
            {
                elementFromRow.AppendChild(rowElement);
            }
            this.FixNestedChildren(row, rowElement);
        }

        private DataTable[] OrderTables(System.Data.DataSet ds)
        {
            DataTable[] array = null;
            if ((ds == null) || (ds.Tables.Count == 0))
            {
                array = new DataTable[0];
            }
            else if (this.TablesAreOrdered(ds))
            {
                array = new DataTable[ds.Tables.Count];
                ds.Tables.CopyTo(array, 0);
            }
            if (array == null)
            {
                array = new DataTable[ds.Tables.Count];
                List<DataTable> list = new List<DataTable>();
                foreach (DataTable table3 in ds.Tables)
                {
                    if (table3.ParentRelations.Count == 0)
                    {
                        list.Add(table3);
                    }
                }
                if (list.Count > 0)
                {
                    foreach (DataTable table2 in ds.Tables)
                    {
                        if (this.IsSelfRelatedDataTable(table2))
                        {
                            list.Add(table2);
                        }
                    }
                    for (int i = 0; i < list.Count; i++)
                    {
                        foreach (DataRelation relation in list[i].ChildRelations)
                        {
                            DataTable childTable = relation.ChildTable;
                            if (!list.Contains(childTable))
                            {
                                list.Add(childTable);
                            }
                        }
                    }
                    list.CopyTo(array);
                    return array;
                }
                ds.Tables.CopyTo(array, 0);
            }
            return array;
        }

        private void PromoteChild(XmlNode child, XmlNode prevSibling)
        {
            if (child.ParentNode != null)
            {
                child.ParentNode.RemoveChild(child);
            }
            prevSibling.ParentNode.InsertAfter(child, prevSibling);
        }

        private void PromoteInnerRegions(XmlNode parent)
        {
            XmlBoundElement element2;
            XmlNode prevSibling = parent;
            this.mapper.GetRegion(parent.ParentNode, out element2);
            TreeIterator iterator = new TreeIterator(parent);
            bool flag = iterator.NextRowElement();
            while (flag)
            {
                XmlBoundElement currentNode = (XmlBoundElement) iterator.CurrentNode;
                flag = iterator.NextRightRowElement();
                this.PromoteChild(currentNode, prevSibling);
                this.SetNestedParentRegion(currentNode, element2);
            }
        }

        private void PromoteNonValueChildren(XmlNode parent)
        {
            XmlNode prevSibling = parent;
            XmlNode firstChild = parent.FirstChild;
            bool flag = true;
            XmlNode nextSibling = null;
            while (firstChild != null)
            {
                nextSibling = firstChild.NextSibling;
                if (!flag || !IsTextLikeNode(firstChild))
                {
                    flag = false;
                    nextSibling = firstChild.NextSibling;
                    this.PromoteChild(firstChild, prevSibling);
                    prevSibling = firstChild;
                }
                firstChild = nextSibling;
            }
        }

        private void RemoveInitialTextNodes(XmlNode node)
        {
            while ((node != null) && IsTextLikeNode(node))
            {
                XmlNode nextSibling = node.NextSibling;
                node.ParentNode.RemoveChild(node);
                node = nextSibling;
            }
        }

        private void ReplaceInitialChildText(XmlNode parent, string value)
        {
            XmlNode firstChild = parent.FirstChild;
            while ((firstChild != null) && (firstChild.NodeType == XmlNodeType.Whitespace))
            {
                firstChild = firstChild.NextSibling;
            }
            if (firstChild != null)
            {
                if (firstChild.NodeType == XmlNodeType.Text)
                {
                    firstChild.Value = value;
                }
                else
                {
                    firstChild = parent.InsertBefore(this.CreateTextNode(value), firstChild);
                }
                this.RemoveInitialTextNodes(firstChild.NextSibling);
            }
            else
            {
                parent.AppendChild(this.CreateTextNode(value));
            }
        }

        internal XmlNode SafeFirstChild(XmlNode n)
        {
            XmlBoundElement element = n as XmlBoundElement;
            if (element != null)
            {
                return element.SafeFirstChild;
            }
            return n.FirstChild;
        }

        internal XmlNode SafeNextSibling(XmlNode n)
        {
            XmlBoundElement element = n as XmlBoundElement;
            if (element != null)
            {
                return element.SafeNextSibling;
            }
            return n.NextSibling;
        }

        internal XmlNode SafePreviousSibling(XmlNode n)
        {
            XmlBoundElement element = n as XmlBoundElement;
            if (element != null)
            {
                return element.SafePreviousSibling;
            }
            return n.PreviousSibling;
        }

        private void SetNestedParentRegion(XmlBoundElement childRowElem)
        {
            XmlBoundElement element;
            this.mapper.GetRegion(childRowElem.ParentNode, out element);
            this.SetNestedParentRegion(childRowElem, element);
        }

        private void SetNestedParentRegion(XmlBoundElement childRowElem, XmlBoundElement parentRowElem)
        {
            DataRow childRow = childRowElem.Row;
            if (parentRowElem == null)
            {
                SetNestedParentRow(childRow, null);
            }
            else
            {
                DataRow row = parentRowElem.Row;
                DataRelation[] nestedParentRelations = childRow.Table.NestedParentRelations;
                if ((nestedParentRelations.Length != 0) && (nestedParentRelations[0].ParentTable == row.Table))
                {
                    SetNestedParentRow(childRow, row);
                }
                else
                {
                    SetNestedParentRow(childRow, null);
                }
            }
        }

        private static void SetNestedParentRow(DataRow childRow, DataRow parentRow)
        {
            DataRelation nestedParentRelation = GetNestedParentRelation(childRow);
            if (nestedParentRelation != null)
            {
                if ((parentRow == null) || (nestedParentRelation.ParentKey.Table != parentRow.Table))
                {
                    childRow.SetParentRow(null, nestedParentRelation);
                }
                else
                {
                    childRow.SetParentRow(parentRow, nestedParentRelation);
                }
            }
        }

        internal static void SetRowValueFromXmlText(DataRow row, DataColumn col, string xmlText)
        {
            object obj2;
            try
            {
                obj2 = col.ConvertXmlToObject(xmlText);
            }
            catch (Exception exception)
            {
                if (!ADP.IsCatchableExceptionType(exception))
                {
                    throw;
                }
                SetRowValueToNull(row, col);
                return;
            }
            if (!obj2.Equals(row[col]))
            {
                row[col] = obj2;
            }
        }

        internal static void SetRowValueToNull(DataRow row, DataColumn col)
        {
            if (!row.IsNull(col))
            {
                row[col] = Convert.DBNull;
            }
        }

        private void SynchronizeRowFromRowElement(XmlBoundElement rowElement)
        {
            this.SynchronizeRowFromRowElement(rowElement, null);
        }

        private void SynchronizeRowFromRowElement(XmlBoundElement rowElement, ArrayList rowElemList)
        {
            DataRow row = rowElement.Row;
            if (row.RowState != DataRowState.Deleted)
            {
                row.BeginEdit();
                this.SynchronizeRowFromRowElementEx(rowElement, rowElemList);
                row.EndEdit();
            }
        }

        private void SynchronizeRowFromRowElementEx(XmlBoundElement rowElement, ArrayList rowElemList)
        {
            bool flag;
            DataRow row = rowElement.Row;
            DataTable table = row.Table;
            Hashtable hashtable = new Hashtable();
            string str = string.Empty;
            RegionIterator iterator = new RegionIterator(rowElement);
            DataColumn textOnlyColumn = this.GetTextOnlyColumn(row);
            if (textOnlyColumn != null)
            {
                string str3;
                hashtable[textOnlyColumn] = textOnlyColumn;
                flag = iterator.NextInitialTextLikeNodes(out str3);
                if ((str3.Length == 0) && (((str = rowElement.GetAttribute("xsi:nil")) == "1") || (str == "true")))
                {
                    row[textOnlyColumn] = Convert.DBNull;
                }
                else
                {
                    SetRowValueFromXmlText(row, textOnlyColumn, str3);
                }
            }
            else
            {
                flag = iterator.Next();
            }
            while (flag)
            {
                XmlElement currentNode = iterator.CurrentNode as XmlElement;
                if (currentNode == null)
                {
                    flag = iterator.Next();
                }
                else
                {
                    XmlBoundElement element2 = currentNode as XmlBoundElement;
                    if ((element2 != null) && (element2.Row != null))
                    {
                        if (rowElemList != null)
                        {
                            rowElemList.Add(currentNode);
                        }
                        flag = iterator.NextRight();
                        continue;
                    }
                    DataColumn columnSchemaForNode = this.mapper.GetColumnSchemaForNode(rowElement, currentNode);
                    if ((columnSchemaForNode != null) && (hashtable[columnSchemaForNode] == null))
                    {
                        string str2;
                        hashtable[columnSchemaForNode] = columnSchemaForNode;
                        flag = iterator.NextInitialTextLikeNodes(out str2);
                        if ((str2.Length == 0) && (((str = currentNode.GetAttribute("xsi:nil")) == "1") || (str == "true")))
                        {
                            row[columnSchemaForNode] = Convert.DBNull;
                        }
                        else
                        {
                            SetRowValueFromXmlText(row, columnSchemaForNode, str2);
                        }
                        continue;
                    }
                    flag = iterator.Next();
                }
            }
            foreach (XmlAttribute attribute in rowElement.Attributes)
            {
                DataColumn col = this.mapper.GetColumnSchemaForNode(rowElement, attribute);
                if ((col != null) && (hashtable[col] == null))
                {
                    hashtable[col] = col;
                    SetRowValueFromXmlText(row, col, attribute.Value);
                }
            }
            foreach (DataColumn column2 in row.Table.Columns)
            {
                if ((hashtable[column2] == null) && !this.IsNotMapped(column2))
                {
                    if (!column2.AutoIncrement)
                    {
                        SetRowValueToNull(row, column2);
                    }
                    else
                    {
                        column2.Init(row.tempRecord);
                    }
                }
            }
        }

        internal void SyncRows(DataRow parentRow, XmlNode node, bool fAddRowsToTable)
        {
            XmlBoundElement rowElement = node as XmlBoundElement;
            if (rowElement != null)
            {
                DataRow childRow = rowElement.Row;
                if ((childRow != null) && (rowElement.ElementState == ElementState.Defoliated))
                {
                    return;
                }
                if (childRow != null)
                {
                    this.SynchronizeRowFromRowElement(rowElement);
                    rowElement.ElementState = ElementState.WeakFoliation;
                    this.DefoliateRegion(rowElement);
                    if (parentRow != null)
                    {
                        SetNestedParentRow(childRow, parentRow);
                    }
                    if (fAddRowsToTable && (childRow.RowState == DataRowState.Detached))
                    {
                        childRow.Table.Rows.Add(childRow);
                    }
                    parentRow = childRow;
                }
            }
            for (XmlNode node2 = node.FirstChild; node2 != null; node2 = node2.NextSibling)
            {
                this.SyncRows(parentRow, node2, fAddRowsToTable);
            }
        }

        internal void SyncTree(XmlNode node)
        {
            XmlBoundElement rowElem = null;
            this.mapper.GetRegion(node, out rowElem);
            DataRow parentRow = null;
            bool fAddRowsToTable = this.IsConnected(node);
            if (rowElem != null)
            {
                DataRow row = rowElem.Row;
                if ((row != null) && (rowElem.ElementState == ElementState.Defoliated))
                {
                    return;
                }
                if (row != null)
                {
                    this.SynchronizeRowFromRowElement(rowElem);
                    if (node == rowElem)
                    {
                        rowElem.ElementState = ElementState.WeakFoliation;
                        this.DefoliateRegion(rowElem);
                    }
                    if (fAddRowsToTable && (row.RowState == DataRowState.Detached))
                    {
                        row.Table.Rows.Add(row);
                    }
                    parentRow = row;
                }
            }
            for (XmlNode node2 = node.FirstChild; node2 != null; node2 = node2.NextSibling)
            {
                this.SyncRows(parentRow, node2, fAddRowsToTable);
            }
        }

        private bool TablesAreOrdered(System.Data.DataSet ds)
        {
            foreach (DataTable table in ds.Tables)
            {
                if (table.Namespace != ds.Namespace)
                {
                    return false;
                }
            }
            return true;
        }

        private void UnBindSpecialListeners()
        {
            this.dataSet.DataRowCreated -= new DataRowCreatedEventHandler(this.OnDataRowCreatedSpecial);
            this.fDataRowCreatedSpecial = false;
        }

        private void UpdateAllColumns(DataRow row, XmlBoundElement rowElement)
        {
            foreach (DataColumn column in row.Table.Columns)
            {
                this.OnColumnValueChanged(row, column, rowElement);
            }
        }

        internal ElementState AutoFoliationState
        {
            get
            {
                return this.autoFoliationState;
            }
            set
            {
                this.autoFoliationState = value;
            }
        }

        public System.Data.DataSet DataSet
        {
            get
            {
                return this.dataSet;
            }
        }

        internal bool IgnoreDataSetEvents
        {
            get
            {
                return this.ignoreDataSetEvents;
            }
            set
            {
                this.ignoreDataSetEvents = value;
            }
        }

        internal bool IgnoreXmlEvents
        {
            get
            {
                return this.ignoreXmlEvents;
            }
            set
            {
                this.ignoreXmlEvents = value;
            }
        }

        internal bool IsFoliationEnabled
        {
            get
            {
                return this.isFoliationEnabled;
            }
            set
            {
                this.isFoliationEnabled = value;
            }
        }

        internal DataSetMapper Mapper
        {
            get
            {
                return this.mapper;
            }
        }
    }
}

