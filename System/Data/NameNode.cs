namespace System.Data
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;

    internal sealed class NameNode : ExpressionNode
    {
        internal char close;
        internal DataColumn column;
        internal bool found;
        internal string name;
        internal char open;
        internal bool type;

        internal NameNode(DataTable table, string name) : base(table)
        {
            this.name = name;
        }

        internal NameNode(DataTable table, char[] text, int start, int pos) : base(table)
        {
            this.name = ParseName(text, start, pos);
        }

        internal override void Bind(DataTable table, List<DataColumn> list)
        {
            base.BindTable(table);
            if (table == null)
            {
                throw ExprException.UnboundName(this.name);
            }
            try
            {
                this.column = table.Columns[this.name];
            }
            catch (Exception exception)
            {
                this.found = false;
                if (!ADP.IsCatchableExceptionType(exception))
                {
                    throw;
                }
                throw ExprException.UnboundName(this.name);
            }
            if (this.column == null)
            {
                throw ExprException.UnboundName(this.name);
            }
            this.name = this.column.ColumnName;
            this.found = true;
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
        }

        internal override bool DependsOn(DataColumn column)
        {
            return ((this.column == column) || (this.column.Computed && this.column.DataExpression.DependsOn(column)));
        }

        internal override object Eval()
        {
            throw ExprException.EvalNoContext();
        }

        internal override object Eval(int[] records)
        {
            throw ExprException.ComputeNotAggregate(this.ToString());
        }

        internal override object Eval(DataRow row, DataRowVersion version)
        {
            if (!this.found)
            {
                throw ExprException.UnboundName(this.name);
            }
            if (row != null)
            {
                return this.column[row.GetRecordFromVersion(version)];
            }
            if (!this.IsTableConstant())
            {
                throw ExprException.UnboundName(this.name);
            }
            return this.column.DataExpression.Evaluate();
        }

        internal override bool HasLocalAggregate()
        {
            return (((this.column != null) && this.column.Computed) && this.column.DataExpression.HasLocalAggregate());
        }

        internal override bool HasRemoteAggregate()
        {
            return (((this.column != null) && this.column.Computed) && this.column.DataExpression.HasRemoteAggregate());
        }

        internal override bool IsConstant()
        {
            return false;
        }

        internal override bool IsTableConstant()
        {
            return (((this.column != null) && this.column.Computed) && this.column.DataExpression.IsTableAggregate());
        }

        internal override ExpressionNode Optimize()
        {
            return this;
        }

        internal static string ParseName(char[] text, int start, int pos)
        {
            char ch = '\0';
            string str = "";
            int startIndex = start;
            int num4 = pos;
            if (text[start] == '`')
            {
                start++;
                pos--;
                ch = '\\';
                str = "`";
            }
            else if (text[start] == '[')
            {
                start++;
                pos--;
                ch = '\\';
                str = @"]\";
            }
            if (ch != '\0')
            {
                int index = start;
                for (int i = start; i < pos; i++)
                {
                    if (((text[i] == ch) && ((i + 1) < pos)) && (str.IndexOf(text[i + 1]) >= 0))
                    {
                        i++;
                    }
                    text[index] = text[i];
                    index++;
                }
                pos = index;
            }
            if (pos == start)
            {
                throw ExprException.InvalidName(new string(text, startIndex, num4 - startIndex));
            }
            return new string(text, start, pos - start);
        }

        internal override bool IsSqlColumn
        {
            get
            {
                return this.column.IsSqlType;
            }
        }
    }
}

