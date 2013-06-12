namespace System.Data
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;

    internal sealed class Select
    {
        private ColumnInfo[] candidateColumns;
        private bool candidatesForBinarySearch;
        private ExpressionNode expression;
        private Index index;
        private readonly int[] indexDesc;
        private readonly IndexField[] IndexFields;
        private ExpressionNode linearExpression;
        private int matchedCandidates;
        private int nCandidates;
        private int recordCount;
        private int[] records;
        private DataViewRowState recordStates;
        private DataExpression rowFilter;
        private readonly DataTable table;

        public Select(DataTable table, string filterExpression, string sort, DataViewRowState recordStates)
        {
            this.table = table;
            this.IndexFields = table.ParseSortString(sort);
            this.indexDesc = ConvertIndexFieldtoIndexDesc(this.IndexFields);
            if ((filterExpression != null) && (filterExpression.Length > 0))
            {
                this.rowFilter = new DataExpression(this.table, filterExpression);
                this.expression = this.rowFilter.ExpressionNode;
            }
            this.recordStates = recordStates;
        }

        private bool AcceptRecord(int record)
        {
            bool flag;
            DataRow row = this.table.recordManager[record];
            if (row == null)
            {
                return true;
            }
            DataRowVersion original = DataRowVersion.Default;
            if (row.oldRecord == record)
            {
                original = DataRowVersion.Original;
            }
            else if (row.newRecord == record)
            {
                original = DataRowVersion.Current;
            }
            else if (row.tempRecord == record)
            {
                original = DataRowVersion.Proposed;
            }
            object obj2 = this.linearExpression.Eval(row, original);
            try
            {
                flag = DataExpression.ToBoolean(obj2);
            }
            catch (Exception exception)
            {
                if (!ADP.IsCatchableExceptionType(exception))
                {
                    throw;
                }
                throw ExprException.FilterConvertion(this.rowFilter.Expression);
            }
            return flag;
        }

        private void AnalyzeExpression(BinaryNode expr)
        {
            if (this.linearExpression != this.expression)
            {
                if (expr.op == 0x1b)
                {
                    this.linearExpression = this.expression;
                }
                else if (expr.op == 0x1a)
                {
                    bool flag = false;
                    bool flag2 = false;
                    if (expr.left is BinaryNode)
                    {
                        this.AnalyzeExpression((BinaryNode) expr.left);
                        if (this.linearExpression == this.expression)
                        {
                            return;
                        }
                        flag = true;
                    }
                    else
                    {
                        UnaryNode left = expr.left as UnaryNode;
                        if (left != null)
                        {
                            while (((left.op == 0) && (left.right is UnaryNode)) && (((UnaryNode) left.right).op == 0))
                            {
                                left = (UnaryNode) left.right;
                            }
                            if ((left.op == 0) && (left.right is BinaryNode))
                            {
                                this.AnalyzeExpression((BinaryNode) left.right);
                                if (this.linearExpression == this.expression)
                                {
                                    return;
                                }
                                flag = true;
                            }
                        }
                    }
                    if (expr.right is BinaryNode)
                    {
                        this.AnalyzeExpression((BinaryNode) expr.right);
                        if (this.linearExpression == this.expression)
                        {
                            return;
                        }
                        flag2 = true;
                    }
                    else
                    {
                        UnaryNode right = expr.right as UnaryNode;
                        if (right != null)
                        {
                            while (((right.op == 0) && (right.right is UnaryNode)) && (((UnaryNode) right.right).op == 0))
                            {
                                right = (UnaryNode) right.right;
                            }
                            if ((right.op == 0) && (right.right is BinaryNode))
                            {
                                this.AnalyzeExpression((BinaryNode) right.right);
                                if (this.linearExpression == this.expression)
                                {
                                    return;
                                }
                                flag2 = true;
                            }
                        }
                    }
                    if (!flag || !flag2)
                    {
                        ExpressionNode node3 = flag ? expr.right : expr.left;
                        this.linearExpression = (this.linearExpression == null) ? node3 : new BinaryNode(this.table, 0x1a, node3, this.linearExpression);
                    }
                }
                else
                {
                    if (this.IsSupportedOperator(expr.op))
                    {
                        if ((expr.left is NameNode) && (expr.right is ConstNode))
                        {
                            ColumnInfo info2 = this.candidateColumns[((NameNode) expr.left).column.Ordinal];
                            info2.expr = (info2.expr == null) ? expr : new BinaryNode(this.table, 0x1a, expr, info2.expr);
                            if (expr.op == 7)
                            {
                                info2.equalsOperator = true;
                            }
                            this.candidatesForBinarySearch = true;
                            return;
                        }
                        if ((expr.right is NameNode) && (expr.left is ConstNode))
                        {
                            ExpressionNode node4 = expr.left;
                            expr.left = expr.right;
                            expr.right = node4;
                            switch (expr.op)
                            {
                                case 8:
                                    expr.op = 9;
                                    break;

                                case 9:
                                    expr.op = 8;
                                    break;

                                case 10:
                                    expr.op = 11;
                                    break;

                                case 11:
                                    expr.op = 10;
                                    break;
                            }
                            ColumnInfo info = this.candidateColumns[((NameNode) expr.left).column.Ordinal];
                            info.expr = (info.expr == null) ? expr : new BinaryNode(this.table, 0x1a, expr, info.expr);
                            if (expr.op == 7)
                            {
                                info.equalsOperator = true;
                            }
                            this.candidatesForBinarySearch = true;
                            return;
                        }
                    }
                    this.linearExpression = (this.linearExpression == null) ? expr : new BinaryNode(this.table, 0x1a, expr, this.linearExpression);
                }
            }
        }

        private void BuildLinearExpression()
        {
            int num;
            int[] indexDesc = this.index.IndexDesc;
            for (num = 0; num < this.matchedCandidates; num++)
            {
                ColumnInfo info = this.candidateColumns[DataKey.ColumnOrder(indexDesc[num])];
                info.flag = true;
            }
            int length = this.candidateColumns.Length;
            for (num = 0; num < length; num++)
            {
                if (this.candidateColumns[num] != null)
                {
                    if (!this.candidateColumns[num].flag)
                    {
                        if (this.candidateColumns[num].expr != null)
                        {
                            this.linearExpression = (this.linearExpression == null) ? this.candidateColumns[num].expr : new BinaryNode(this.table, 0x1a, this.candidateColumns[num].expr, this.linearExpression);
                        }
                    }
                    else
                    {
                        this.candidateColumns[num].flag = false;
                    }
                }
            }
        }

        private int CompareClosestCandidateIndexDesc(int[] id)
        {
            int num2 = (id.Length < this.nCandidates) ? id.Length : this.nCandidates;
            int index = 0;
            while (index < num2)
            {
                ColumnInfo info = this.candidateColumns[DataKey.ColumnOrder(id[index])];
                if ((info == null) || (info.expr == null))
                {
                    return index;
                }
                if (!info.equalsOperator)
                {
                    return (index + 1);
                }
                index++;
            }
            return index;
        }

        private int CompareRecords(int record1, int record2)
        {
            int length = this.indexDesc.Length;
            for (int i = 0; i < length; i++)
            {
                int indexDesc = this.indexDesc[i];
                int num3 = this.table.Columns[DataKey.ColumnOrder(indexDesc)].Compare(record1, record2);
                if (num3 != 0)
                {
                    if (DataKey.SortDecending(indexDesc))
                    {
                        num3 = -num3;
                    }
                    return num3;
                }
            }
            long recordState = (this.table.recordManager[record1] == null) ? 0L : this.table.recordManager[record1].rowID;
            long num = (this.table.recordManager[record2] == null) ? 0L : this.table.recordManager[record2].rowID;
            int num5 = (recordState < num) ? -1 : ((num < recordState) ? 1 : 0);
            if (((num5 == 0) && (record1 != record2)) && ((this.table.recordManager[record1] != null) && (this.table.recordManager[record2] != null)))
            {
                recordState = (long) this.table.recordManager[record1].GetRecordState(record1);
                num = (long) this.table.recordManager[record2].GetRecordState(record2);
                num5 = (recordState < num) ? -1 : ((num < recordState) ? 1 : 0);
            }
            return num5;
        }

        private bool CompareSortIndexDesc(int[] id)
        {
            if (id.Length < this.indexDesc.Length)
            {
                return false;
            }
            int index = 0;
            for (int i = 0; (i < id.Length) && (index < this.indexDesc.Length); i++)
            {
                if (id[i] == this.indexDesc[index])
                {
                    index++;
                }
                else
                {
                    ColumnInfo info = this.candidateColumns[DataKey.ColumnOrder(id[i])];
                    if ((info == null) || !info.equalsOperator)
                    {
                        return false;
                    }
                }
            }
            return (index == this.indexDesc.Length);
        }

        internal static int[] ConvertIndexFieldtoIndexDesc(IndexField[] fields)
        {
            int[] numArray = new int[fields.Length];
            for (int i = 0; i < fields.Length; i++)
            {
                numArray[i] = fields[i].Column.Ordinal | (fields[i].IsDescending ? -2147483648 : 0);
            }
            return numArray;
        }

        private void CreateIndex()
        {
            if (this.index == null)
            {
                if (this.nCandidates == 0)
                {
                    this.index = new Index(this.table, this.IndexFields, this.recordStates, null);
                    this.index.AddRef();
                }
                else
                {
                    int num;
                    int length = this.candidateColumns.Length;
                    int num5 = this.indexDesc.Length;
                    bool flag = true;
                    for (num = 0; num < length; num++)
                    {
                        if ((this.candidateColumns[num] != null) && !this.candidateColumns[num].equalsOperator)
                        {
                            flag = false;
                            break;
                        }
                    }
                    int num2 = 0;
                    for (num = 0; num < num5; num++)
                    {
                        ColumnInfo info4 = this.candidateColumns[DataKey.ColumnOrder(this.indexDesc[num])];
                        if (info4 != null)
                        {
                            info4.flag = true;
                            num2++;
                        }
                    }
                    int num7 = num5 - num2;
                    int[] ndexDesc = new int[this.nCandidates + num7];
                    if (flag)
                    {
                        num2 = 0;
                        for (num = 0; num < length; num++)
                        {
                            if (this.candidateColumns[num] != null)
                            {
                                ndexDesc[num2++] = num;
                                this.candidateColumns[num].flag = false;
                            }
                        }
                        for (num = 0; num < num5; num++)
                        {
                            ColumnInfo info = this.candidateColumns[DataKey.ColumnOrder(this.indexDesc[num])];
                            if ((info == null) || info.flag)
                            {
                                ndexDesc[num2++] = this.indexDesc[num];
                                if (info != null)
                                {
                                    info.flag = false;
                                }
                            }
                        }
                        for (num = 0; num < this.candidateColumns.Length; num++)
                        {
                            if (this.candidateColumns[num] != null)
                            {
                                this.candidateColumns[num].flag = false;
                            }
                        }
                        IndexField[] zeroIndexField = DataTable.zeroIndexField;
                        if (0 < ndexDesc.Length)
                        {
                            zeroIndexField = new IndexField[ndexDesc.Length];
                            for (int i = 0; i < ndexDesc.Length; i++)
                            {
                                DataColumn column2 = this.table.Columns[DataKey.ColumnOrder(ndexDesc[i])];
                                bool isDescending = DataKey.SortDecending(ndexDesc[i]);
                                zeroIndexField[i] = new IndexField(column2, isDescending);
                            }
                        }
                        this.index = new Index(this.table, ndexDesc, zeroIndexField, this.recordStates, null);
                        if (!this.IsOperatorIn(this.expression))
                        {
                            this.index.AddRef();
                        }
                        this.matchedCandidates = this.nCandidates;
                    }
                    else
                    {
                        num = 0;
                        while (num < num5)
                        {
                            ndexDesc[num] = this.indexDesc[num];
                            ColumnInfo info3 = this.candidateColumns[DataKey.ColumnOrder(this.indexDesc[num])];
                            if (info3 != null)
                            {
                                info3.flag = true;
                            }
                            num++;
                        }
                        num2 = num;
                        for (num = 0; num < length; num++)
                        {
                            if (this.candidateColumns[num] != null)
                            {
                                if (!this.candidateColumns[num].flag)
                                {
                                    ndexDesc[num2++] = num;
                                }
                                else
                                {
                                    this.candidateColumns[num].flag = false;
                                }
                            }
                        }
                        IndexField[] indexFields = DataTable.zeroIndexField;
                        if (0 < ndexDesc.Length)
                        {
                            indexFields = new IndexField[ndexDesc.Length];
                            for (int j = 0; j < ndexDesc.Length; j++)
                            {
                                DataColumn column = this.table.Columns[DataKey.ColumnOrder(ndexDesc[j])];
                                bool flag2 = DataKey.SortDecending(ndexDesc[j]);
                                indexFields[j] = new IndexField(column, flag2);
                            }
                        }
                        this.index = new Index(this.table, ndexDesc, indexFields, this.recordStates, null);
                        this.matchedCandidates = 0;
                        if (this.linearExpression != this.expression)
                        {
                            int[] indexDesc = this.index.IndexDesc;
                            while (this.matchedCandidates < num2)
                            {
                                ColumnInfo info2 = this.candidateColumns[DataKey.ColumnOrder(indexDesc[this.matchedCandidates])];
                                if ((info2 == null) || (info2.expr == null))
                                {
                                    break;
                                }
                                this.matchedCandidates++;
                                if (!info2.equalsOperator)
                                {
                                    break;
                                }
                            }
                        }
                        for (num = 0; num < this.candidateColumns.Length; num++)
                        {
                            if (this.candidateColumns[num] != null)
                            {
                                this.candidateColumns[num].flag = false;
                            }
                        }
                    }
                }
            }
        }

        private int Eval(BinaryNode expr, DataRow row, DataRowVersion version)
        {
            if (expr.op == 0x1a)
            {
                int num4 = this.Eval((BinaryNode) expr.left, row, version);
                if (num4 != 0)
                {
                    return num4;
                }
                int num3 = this.Eval((BinaryNode) expr.right, row, version);
                if (num3 != 0)
                {
                    return num3;
                }
                return 0;
            }
            long num = 0L;
            object obj3 = expr.left.Eval(row, version);
            if ((expr.op != 13) && (expr.op != 0x27))
            {
                StorageType type;
                object obj2 = expr.right.Eval(row, version);
                bool lc = expr.left is ConstNode;
                bool rc = expr.right is ConstNode;
                if ((obj3 == DBNull.Value) || (expr.left.IsSqlColumn && DataStorage.IsObjectSqlNull(obj3)))
                {
                    return -1;
                }
                if ((obj2 == DBNull.Value) || (expr.right.IsSqlColumn && DataStorage.IsObjectSqlNull(obj2)))
                {
                    return 1;
                }
                StorageType storageType = DataStorage.GetStorageType(obj3.GetType());
                if (StorageType.Char == storageType)
                {
                    if (rc || !expr.right.IsSqlColumn)
                    {
                        obj2 = Convert.ToChar(obj2, this.table.FormatProvider);
                    }
                    else
                    {
                        obj2 = SqlConvert.ChangeType2(obj2, StorageType.Char, typeof(char), this.table.FormatProvider);
                    }
                }
                StorageType right = DataStorage.GetStorageType(obj2.GetType());
                if (expr.left.IsSqlColumn || expr.right.IsSqlColumn)
                {
                    type = expr.ResultSqlType(storageType, right, lc, rc, expr.op);
                }
                else
                {
                    type = expr.ResultType(storageType, right, lc, rc, expr.op);
                }
                if (type == StorageType.Empty)
                {
                    expr.SetTypeMismatchError(expr.op, obj3.GetType(), obj2.GetType());
                }
                num = expr.BinaryCompare(obj3, obj2, type, expr.op);
            }
            switch (expr.op)
            {
                case 7:
                    num = (num == 0L) ? ((long) 0) : ((num < 0L) ? ((long) (-1)) : ((long) 1));
                    break;

                case 8:
                    num = (num > 0L) ? ((long) 0) : ((long) (-1));
                    break;

                case 9:
                    num = (num < 0L) ? ((long) 0) : ((long) 1);
                    break;

                case 10:
                    num = (num >= 0L) ? ((long) 0) : ((long) (-1));
                    break;

                case 11:
                    num = (num <= 0L) ? ((long) 0) : ((long) 1);
                    break;

                case 13:
                    num = (obj3 == DBNull.Value) ? ((long) 0) : ((long) (-1));
                    break;

                case 0x27:
                    num = (obj3 != DBNull.Value) ? ((long) 0) : ((long) 1);
                    break;
            }
            return (int) num;
        }

        private int Evaluate(int record)
        {
            DataRow row = this.table.recordManager[record];
            if (row != null)
            {
                DataRowVersion original = DataRowVersion.Default;
                if (row.oldRecord == record)
                {
                    original = DataRowVersion.Original;
                }
                else if (row.newRecord == record)
                {
                    original = DataRowVersion.Current;
                }
                else if (row.tempRecord == record)
                {
                    original = DataRowVersion.Proposed;
                }
                int[] indexDesc = this.index.IndexDesc;
                for (int i = 0; i < this.matchedCandidates; i++)
                {
                    int num2 = this.Eval(this.candidateColumns[DataKey.ColumnOrder(indexDesc[i])].expr, row, original);
                    if (num2 != 0)
                    {
                        if (!DataKey.SortDecending(indexDesc[i]))
                        {
                            return num2;
                        }
                        return -num2;
                    }
                }
            }
            return 0;
        }

        private bool FindClosestCandidateIndex()
        {
            this.index = null;
            this.matchedCandidates = 0;
            bool flag = true;
            this.table.indexesLock.AcquireReaderLock(-1);
            try
            {
                int count = this.table.indexes.Count;
                int num1 = this.table.Rows.Count;
                for (int i = 0; i < count; i++)
                {
                    Index index = this.table.indexes[i];
                    if ((index.RecordStates == this.recordStates) && index.IsSharable)
                    {
                        int num2 = this.CompareClosestCandidateIndexDesc(index.IndexDesc);
                        if ((num2 > this.matchedCandidates) || ((num2 == this.matchedCandidates) && !flag))
                        {
                            this.matchedCandidates = num2;
                            this.index = index;
                            flag = this.CompareSortIndexDesc(index.IndexDesc);
                            if ((this.matchedCandidates == this.nCandidates) && flag)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            finally
            {
                this.table.indexesLock.ReleaseReaderLock();
            }
            if (this.index == null)
            {
                return false;
            }
            return flag;
        }

        private int FindFirstMatchingRecord()
        {
            int num5 = -1;
            int num3 = 0;
            int num2 = this.index.RecordCount - 1;
            while (num3 <= num2)
            {
                int recordIndex = (num3 + num2) >> 1;
                int record = this.index.GetRecord(recordIndex);
                int num4 = this.Evaluate(record);
                if (num4 == 0)
                {
                    num5 = recordIndex;
                }
                if (num4 < 0)
                {
                    num3 = recordIndex + 1;
                }
                else
                {
                    num2 = recordIndex - 1;
                }
            }
            return num5;
        }

        private int FindLastMatchingRecord(int lo)
        {
            int num4 = -1;
            int num2 = this.index.RecordCount - 1;
            while (lo <= num2)
            {
                int recordIndex = (lo + num2) >> 1;
                int record = this.index.GetRecord(recordIndex);
                int num3 = this.Evaluate(record);
                if (num3 == 0)
                {
                    num4 = recordIndex;
                }
                if (num3 <= 0)
                {
                    lo = recordIndex + 1;
                }
                else
                {
                    num2 = recordIndex - 1;
                }
            }
            return num4;
        }

        private bool FindSortIndex()
        {
            this.index = null;
            this.table.indexesLock.AcquireReaderLock(-1);
            try
            {
                int count = this.table.indexes.Count;
                int num1 = this.table.Rows.Count;
                for (int i = 0; i < count; i++)
                {
                    Index index = this.table.indexes[i];
                    if (((index.RecordStates == this.recordStates) && index.IsSharable) && this.CompareSortIndexDesc(index.IndexDesc))
                    {
                        this.index = index;
                        return true;
                    }
                }
            }
            finally
            {
                this.table.indexesLock.ReleaseReaderLock();
            }
            return false;
        }

        private Range GetBinaryFilteredRecords()
        {
            if (this.matchedCandidates == 0)
            {
                return new Range(0, this.index.RecordCount - 1);
            }
            int min = this.FindFirstMatchingRecord();
            if (min == -1)
            {
                return new Range();
            }
            return new Range(min, this.FindLastMatchingRecord(min));
        }

        private int[] GetLinearFilteredRecords(Range range)
        {
            if (this.linearExpression == null)
            {
                int[] numArray = new int[range.Count];
                RBTree<int>.RBTreeEnumerator enumerator2 = this.index.GetEnumerator(range.Min);
                for (int j = 0; (j < range.Count) && enumerator2.MoveNext(); j++)
                {
                    numArray[j] = enumerator2.Current;
                }
                return numArray;
            }
            List<int> list = new List<int>();
            RBTree<int>.RBTreeEnumerator enumerator = this.index.GetEnumerator(range.Min);
            for (int i = 0; (i < range.Count) && enumerator.MoveNext(); i++)
            {
                if (this.AcceptRecord(enumerator.Current))
                {
                    list.Add(enumerator.Current);
                }
            }
            return list.ToArray();
        }

        private DataRow[] GetLinearFilteredRows(Range range)
        {
            if (this.linearExpression == null)
            {
                return this.index.GetRows(range);
            }
            List<DataRow> list = new List<DataRow>();
            RBTree<int>.RBTreeEnumerator enumerator = this.index.GetEnumerator(range.Min);
            for (int i = 0; (i < range.Count) && enumerator.MoveNext(); i++)
            {
                if (this.AcceptRecord(enumerator.Current))
                {
                    list.Add(this.table.recordManager[enumerator.Current]);
                }
            }
            DataRow[] array = this.table.NewRowArray(list.Count);
            list.CopyTo(array);
            return array;
        }

        public DataRow[] GetRows()
        {
            DataRow[] rowArray = this.table.NewRowArray(this.recordCount);
            for (int i = 0; i < rowArray.Length; i++)
            {
                rowArray[i] = this.table.recordManager[this.records[i]];
            }
            return rowArray;
        }

        private void InitCandidateColumns()
        {
            this.nCandidates = 0;
            this.candidateColumns = new ColumnInfo[this.table.Columns.Count];
            if (this.rowFilter != null)
            {
                DataColumn[] dependency = this.rowFilter.GetDependency();
                for (int i = 0; i < dependency.Length; i++)
                {
                    if (dependency[i].Table == this.table)
                    {
                        this.candidateColumns[dependency[i].Ordinal] = new ColumnInfo();
                        this.nCandidates++;
                    }
                }
            }
        }

        private bool IsOperatorIn(ExpressionNode enode)
        {
            BinaryNode node = enode as BinaryNode;
            if ((node == null) || (((5 != node.op) && !this.IsOperatorIn(node.right)) && !this.IsOperatorIn(node.left)))
            {
                return false;
            }
            return true;
        }

        private bool IsSupportedOperator(int op)
        {
            if (((op < 7) || (op > 11)) && (op != 13))
            {
                return (op == 0x27);
            }
            return true;
        }

        public DataRow[] SelectRows()
        {
            Range binaryFilteredRecords;
            bool flag = true;
            this.InitCandidateColumns();
            if (this.expression is BinaryNode)
            {
                this.AnalyzeExpression((BinaryNode) this.expression);
                if (!this.candidatesForBinarySearch)
                {
                    this.linearExpression = this.expression;
                }
                if (this.linearExpression == this.expression)
                {
                    for (int i = 0; i < this.candidateColumns.Length; i++)
                    {
                        if (this.candidateColumns[i] != null)
                        {
                            this.candidateColumns[i].equalsOperator = false;
                            this.candidateColumns[i].expr = null;
                        }
                    }
                }
                else
                {
                    flag = !this.FindClosestCandidateIndex();
                }
            }
            else
            {
                this.linearExpression = this.expression;
            }
            if ((this.index == null) && ((this.indexDesc.Length > 0) || (this.linearExpression == this.expression)))
            {
                flag = !this.FindSortIndex();
            }
            if (this.index == null)
            {
                this.CreateIndex();
                flag = false;
            }
            if (this.index.RecordCount == 0)
            {
                return this.table.NewRowArray(0);
            }
            if (this.matchedCandidates == 0)
            {
                binaryFilteredRecords = new Range(0, this.index.RecordCount - 1);
                this.linearExpression = this.expression;
                return this.GetLinearFilteredRows(binaryFilteredRecords);
            }
            binaryFilteredRecords = this.GetBinaryFilteredRecords();
            if (binaryFilteredRecords.Count == 0)
            {
                return this.table.NewRowArray(0);
            }
            if (this.matchedCandidates < this.nCandidates)
            {
                this.BuildLinearExpression();
            }
            if (!flag)
            {
                return this.GetLinearFilteredRows(binaryFilteredRecords);
            }
            this.records = this.GetLinearFilteredRecords(binaryFilteredRecords);
            this.recordCount = this.records.Length;
            if (this.recordCount == 0)
            {
                return this.table.NewRowArray(0);
            }
            this.Sort(0, this.recordCount - 1);
            return this.GetRows();
        }

        private void Sort(int left, int right)
        {
            int num2;
            do
            {
                num2 = left;
                int index = right;
                int num3 = this.records[(num2 + index) >> 1];
                do
                {
                    while (this.CompareRecords(this.records[num2], num3) < 0)
                    {
                        num2++;
                    }
                    while (this.CompareRecords(this.records[index], num3) > 0)
                    {
                        index--;
                    }
                    if (num2 <= index)
                    {
                        int num4 = this.records[num2];
                        this.records[num2] = this.records[index];
                        this.records[index] = num4;
                        num2++;
                        index--;
                    }
                }
                while (num2 <= index);
                if (left < index)
                {
                    this.Sort(left, index);
                }
                left = num2;
            }
            while (num2 < right);
        }

        private sealed class ColumnInfo
        {
            public bool equalsOperator;
            public BinaryNode expr;
            public bool flag;
        }
    }
}

