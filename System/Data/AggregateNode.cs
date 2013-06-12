namespace System.Data
{
    using System;
    using System.Collections.Generic;

    internal sealed class AggregateNode : ExpressionNode
    {
        private readonly Aggregate aggregate;
        private DataTable childTable;
        private DataColumn column;
        private readonly string columnName;
        private readonly bool local;
        private DataRelation relation;
        private readonly string relationName;
        private readonly AggregateType type;

        internal AggregateNode(DataTable table, FunctionId aggregateType, string columnName) : this(table, aggregateType, columnName, true, null)
        {
        }

        internal AggregateNode(DataTable table, FunctionId aggregateType, string columnName, string relationName) : this(table, aggregateType, columnName, false, relationName)
        {
        }

        internal AggregateNode(DataTable table, FunctionId aggregateType, string columnName, bool local, string relationName) : base(table)
        {
            this.aggregate = (Aggregate) aggregateType;
            if (aggregateType == FunctionId.Sum)
            {
                this.type = AggregateType.Sum;
            }
            else if (aggregateType == FunctionId.Avg)
            {
                this.type = AggregateType.Mean;
            }
            else if (aggregateType == FunctionId.Min)
            {
                this.type = AggregateType.Min;
            }
            else if (aggregateType == FunctionId.Max)
            {
                this.type = AggregateType.Max;
            }
            else if (aggregateType == FunctionId.Count)
            {
                this.type = AggregateType.Count;
            }
            else if (aggregateType == FunctionId.Var)
            {
                this.type = AggregateType.Var;
            }
            else
            {
                if (aggregateType != FunctionId.StDev)
                {
                    throw ExprException.UndefinedFunction(Function.FunctionName[(int) aggregateType]);
                }
                this.type = AggregateType.StDev;
            }
            this.local = local;
            this.relationName = relationName;
            this.columnName = columnName;
        }

        internal static void Bind(DataRelation relation, List<DataColumn> list)
        {
            if (relation != null)
            {
                foreach (DataColumn column2 in relation.ChildColumnsReference)
                {
                    if (!list.Contains(column2))
                    {
                        list.Add(column2);
                    }
                }
                foreach (DataColumn column in relation.ParentColumnsReference)
                {
                    if (!list.Contains(column))
                    {
                        list.Add(column);
                    }
                }
            }
        }

        internal override void Bind(DataTable table, List<DataColumn> list)
        {
            base.BindTable(table);
            if (table == null)
            {
                throw ExprException.AggregateUnbound(this.ToString());
            }
            if (this.local)
            {
                this.relation = null;
            }
            else
            {
                DataRelationCollection childRelations = table.ChildRelations;
                if (this.relationName == null)
                {
                    if (childRelations.Count > 1)
                    {
                        throw ExprException.UnresolvedRelation(table.TableName, this.ToString());
                    }
                    if (childRelations.Count != 1)
                    {
                        throw ExprException.AggregateUnbound(this.ToString());
                    }
                    this.relation = childRelations[0];
                }
                else
                {
                    this.relation = childRelations[this.relationName];
                }
            }
            this.childTable = (this.relation == null) ? table : this.relation.ChildTable;
            this.column = this.childTable.Columns[this.columnName];
            if (this.column == null)
            {
                throw ExprException.UnboundName(this.columnName);
            }
            int num = 0;
            while (num < list.Count)
            {
                DataColumn column = list[num];
                if (this.column == column)
                {
                    break;
                }
                num++;
            }
            if (num >= list.Count)
            {
                list.Add(this.column);
            }
            Bind(this.relation, list);
        }

        internal override bool DependsOn(DataColumn column)
        {
            return ((this.column == column) || (this.column.Computed && this.column.DataExpression.DependsOn(column)));
        }

        internal override object Eval()
        {
            return this.Eval(null, DataRowVersion.Default);
        }

        internal override object Eval(int[] records)
        {
            if (this.childTable == null)
            {
                throw ExprException.AggregateUnbound(this.ToString());
            }
            if (!this.local)
            {
                throw ExprException.ComputeNotAggregate(this.ToString());
            }
            return this.column.GetAggregateValue(records, this.type);
        }

        internal override object Eval(DataRow row, DataRowVersion version)
        {
            DataRow[] childRows;
            if (this.childTable == null)
            {
                throw ExprException.AggregateUnbound(this.ToString());
            }
            if (this.local)
            {
                childRows = new DataRow[this.childTable.Rows.Count];
                this.childTable.Rows.CopyTo(childRows, 0);
            }
            else
            {
                if (row == null)
                {
                    throw ExprException.EvalNoContext();
                }
                if (this.relation == null)
                {
                    throw ExprException.AggregateUnbound(this.ToString());
                }
                childRows = row.GetChildRows(this.relation, version);
            }
            if (version == DataRowVersion.Proposed)
            {
                version = DataRowVersion.Default;
            }
            List<int> list = new List<int>();
            for (int i = 0; i < childRows.Length; i++)
            {
                if (childRows[i].RowState == DataRowState.Deleted)
                {
                    if (DataRowAction.Rollback != childRows[i]._action)
                    {
                        continue;
                    }
                    version = DataRowVersion.Original;
                }
                else if ((DataRowAction.Rollback == childRows[i]._action) && (childRows[i].RowState == DataRowState.Added))
                {
                    continue;
                }
                if ((version != DataRowVersion.Original) || (childRows[i].oldRecord != -1))
                {
                    list.Add(childRows[i].GetRecordFromVersion(version));
                }
            }
            int[] records = list.ToArray();
            return this.column.GetAggregateValue(records, this.type);
        }

        internal override bool HasLocalAggregate()
        {
            return this.local;
        }

        internal override bool HasRemoteAggregate()
        {
            return !this.local;
        }

        internal override bool IsConstant()
        {
            return false;
        }

        internal override bool IsTableConstant()
        {
            return this.local;
        }

        internal override ExpressionNode Optimize()
        {
            return this;
        }
    }
}

