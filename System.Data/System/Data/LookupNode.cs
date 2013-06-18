namespace System.Data
{
    using System;
    using System.Collections.Generic;

    internal sealed class LookupNode : ExpressionNode
    {
        private DataColumn column;
        private readonly string columnName;
        private DataRelation relation;
        private readonly string relationName;

        internal LookupNode(DataTable table, string columnName, string relationName) : base(table)
        {
            this.relationName = relationName;
            this.columnName = columnName;
        }

        internal override void Bind(DataTable table, List<DataColumn> list)
        {
            base.BindTable(table);
            this.column = null;
            this.relation = null;
            if (table == null)
            {
                throw ExprException.ExpressionUnbound(this.ToString());
            }
            DataRelationCollection parentRelations = table.ParentRelations;
            if (this.relationName == null)
            {
                if (parentRelations.Count > 1)
                {
                    throw ExprException.UnresolvedRelation(table.TableName, this.ToString());
                }
                this.relation = parentRelations[0];
            }
            else
            {
                this.relation = parentRelations[this.relationName];
            }
            if (this.relation == null)
            {
                throw ExprException.BindFailure(this.relationName);
            }
            DataTable parentTable = this.relation.ParentTable;
            this.column = parentTable.Columns[this.columnName];
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
            AggregateNode.Bind(this.relation, list);
        }

        internal override bool DependsOn(DataColumn column)
        {
            return (this.column == column);
        }

        internal override object Eval()
        {
            throw ExprException.EvalNoContext();
        }

        internal override object Eval(int[] recordNos)
        {
            throw ExprException.ComputeNotAggregate(this.ToString());
        }

        internal override object Eval(DataRow row, DataRowVersion version)
        {
            if ((this.column == null) || (this.relation == null))
            {
                throw ExprException.ExpressionUnbound(this.ToString());
            }
            DataRow parentRow = row.GetParentRow(this.relation, version);
            if (parentRow == null)
            {
                return DBNull.Value;
            }
            return parentRow[this.column, parentRow.HasVersion(version) ? version : DataRowVersion.Current];
        }

        internal override bool HasLocalAggregate()
        {
            return false;
        }

        internal override bool HasRemoteAggregate()
        {
            return false;
        }

        internal override bool IsConstant()
        {
            return false;
        }

        internal override bool IsTableConstant()
        {
            return false;
        }

        internal override ExpressionNode Optimize()
        {
            return this;
        }
    }
}

