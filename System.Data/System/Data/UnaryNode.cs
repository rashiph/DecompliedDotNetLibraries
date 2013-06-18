namespace System.Data
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.SqlTypes;

    internal sealed class UnaryNode : ExpressionNode
    {
        internal readonly int op;
        internal ExpressionNode right;

        internal UnaryNode(DataTable table, int op, ExpressionNode right) : base(table)
        {
            this.op = op;
            this.right = right;
        }

        internal override void Bind(DataTable table, List<DataColumn> list)
        {
            base.BindTable(table);
            this.right.Bind(table, list);
        }

        internal override bool DependsOn(DataColumn column)
        {
            return this.right.DependsOn(column);
        }

        internal override object Eval()
        {
            return this.Eval(null, DataRowVersion.Default);
        }

        internal override object Eval(int[] recordNos)
        {
            return this.right.Eval(recordNos);
        }

        internal override object Eval(DataRow row, DataRowVersion version)
        {
            return this.EvalUnaryOp(this.op, this.right.Eval(row, version));
        }

        private object EvalUnaryOp(int op, object vl)
        {
            if (!DataExpression.IsUnknown(vl))
            {
                switch (op)
                {
                    case 0:
                        return vl;

                    case 1:
                    {
                        StorageType storageType = DataStorage.GetStorageType(vl.GetType());
                        if (!ExpressionNode.IsNumericSql(storageType))
                        {
                            throw ExprException.TypeMismatch(this.ToString());
                        }
                        switch (storageType)
                        {
                            case StorageType.Byte:
                                return (int) -((byte) vl);

                            case StorageType.Int16:
                                return (int) -((short) vl);

                            case StorageType.Int32:
                                return -((int) vl);

                            case StorageType.Int64:
                                return -((long) vl);

                            case StorageType.Single:
                                return -((float) vl);

                            case StorageType.Double:
                                return -((double) vl);

                            case StorageType.Decimal:
                                return -((decimal) vl);

                            case StorageType.SqlDecimal:
                                return -((SqlDecimal) vl);

                            case StorageType.SqlDouble:
                                return -((SqlDouble) vl);

                            case StorageType.SqlInt16:
                                return -((SqlInt16) vl);

                            case StorageType.SqlInt32:
                                return -((SqlInt32) vl);

                            case StorageType.SqlInt64:
                                return -((SqlInt64) vl);

                            case StorageType.SqlMoney:
                                return -((SqlMoney) vl);

                            case StorageType.SqlSingle:
                                return -((SqlSingle) vl);
                        }
                        break;
                    }
                    case 2:
                        if (!ExpressionNode.IsNumericSql(DataStorage.GetStorageType(vl.GetType())))
                        {
                            throw ExprException.TypeMismatch(this.ToString());
                        }
                        return vl;

                    case 3:
                    {
                        if (!(vl is SqlBoolean))
                        {
                            if (DataExpression.ToBoolean(vl))
                            {
                                return false;
                            }
                            return true;
                        }
                        SqlBoolean flag2 = (SqlBoolean) vl;
                        if (!flag2.IsFalse)
                        {
                            SqlBoolean flag = (SqlBoolean) vl;
                            if (!flag.IsTrue)
                            {
                                throw ExprException.UnsupportedOperator(op);
                            }
                            return SqlBoolean.False;
                        }
                        return SqlBoolean.True;
                    }
                    default:
                        throw ExprException.UnsupportedOperator(op);
                }
            }
            return DBNull.Value;
        }

        internal override bool HasLocalAggregate()
        {
            return this.right.HasLocalAggregate();
        }

        internal override bool HasRemoteAggregate()
        {
            return this.right.HasRemoteAggregate();
        }

        internal override bool IsConstant()
        {
            return this.right.IsConstant();
        }

        internal override bool IsTableConstant()
        {
            return this.right.IsTableConstant();
        }

        internal override ExpressionNode Optimize()
        {
            this.right = this.right.Optimize();
            if (this.IsConstant())
            {
                return new ConstNode(base.table, System.Data.ValueType.Object, this.Eval(), false);
            }
            return this;
        }
    }
}

