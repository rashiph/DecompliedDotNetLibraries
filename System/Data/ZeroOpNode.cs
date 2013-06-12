namespace System.Data
{
    using System;
    using System.Collections.Generic;

    internal sealed class ZeroOpNode : ExpressionNode
    {
        internal readonly int op;
        internal const int zop_False = 0;
        internal const int zop_Null = -1;
        internal const int zop_True = 1;

        internal ZeroOpNode(int op) : base(null)
        {
            this.op = op;
        }

        internal override void Bind(DataTable table, List<DataColumn> list)
        {
        }

        internal override object Eval()
        {
            switch (this.op)
            {
                case 0x20:
                    return DBNull.Value;

                case 0x21:
                    return true;

                case 0x22:
                    return false;
            }
            return DBNull.Value;
        }

        internal override object Eval(int[] recordNos)
        {
            return this.Eval();
        }

        internal override object Eval(DataRow row, DataRowVersion version)
        {
            return this.Eval();
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
            return true;
        }

        internal override bool IsTableConstant()
        {
            return true;
        }

        internal override ExpressionNode Optimize()
        {
            return this;
        }
    }
}

