namespace System.Data
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.SqlTypes;

    internal class BinaryNode : ExpressionNode
    {
        internal ExpressionNode left;
        internal int op;
        internal ExpressionNode right;

        internal BinaryNode(DataTable table, int op, ExpressionNode left, ExpressionNode right) : base(table)
        {
            this.op = op;
            this.left = left;
            this.right = right;
        }

        internal int BinaryCompare(object vLeft, object vRight, StorageType resultType, int op)
        {
            int num2 = 0;
            try
            {
                if (!DataStorage.IsSqlType(resultType))
                {
                    switch (resultType)
                    {
                        case StorageType.Boolean:
                            if ((op != 7) && (op != 12))
                            {
                                break;
                            }
                            return (Convert.ToInt32(DataExpression.ToBoolean(vLeft), base.FormatProvider) - Convert.ToInt32(DataExpression.ToBoolean(vRight), base.FormatProvider));

                        case StorageType.Char:
                            return Convert.ToInt32(vLeft, base.FormatProvider).CompareTo(Convert.ToInt32(vRight, base.FormatProvider));

                        case StorageType.SByte:
                        case StorageType.Byte:
                        case StorageType.Int16:
                        case StorageType.UInt16:
                        case StorageType.Int32:
                            return Convert.ToInt32(vLeft, base.FormatProvider).CompareTo(Convert.ToInt32(vRight, base.FormatProvider));

                        case StorageType.UInt32:
                        case StorageType.Int64:
                        case StorageType.UInt64:
                        case StorageType.Decimal:
                            return decimal.Compare(Convert.ToDecimal(vLeft, base.FormatProvider), Convert.ToDecimal(vRight, base.FormatProvider));

                        case StorageType.Single:
                            return Convert.ToSingle(vLeft, base.FormatProvider).CompareTo(Convert.ToSingle(vRight, base.FormatProvider));

                        case StorageType.Double:
                            return Convert.ToDouble(vLeft, base.FormatProvider).CompareTo(Convert.ToDouble(vRight, base.FormatProvider));

                        case StorageType.DateTime:
                            return DateTime.Compare(Convert.ToDateTime(vLeft, base.FormatProvider), Convert.ToDateTime(vRight, base.FormatProvider));

                        case StorageType.String:
                            return base.table.Compare(Convert.ToString(vLeft, base.FormatProvider), Convert.ToString(vRight, base.FormatProvider));

                        case StorageType.Guid:
                        {
                            Guid guid2 = (Guid) vLeft;
                            return guid2.CompareTo((Guid) vRight);
                        }
                        case StorageType.DateTimeOffset:
                            return DateTimeOffset.Compare((DateTimeOffset) vLeft, (DateTimeOffset) vRight);
                    }
                }
                else
                {
                    switch (resultType)
                    {
                        case StorageType.SByte:
                        case StorageType.Byte:
                        case StorageType.Int16:
                        case StorageType.UInt16:
                        case StorageType.Int32:
                        case StorageType.SqlByte:
                        case StorageType.SqlInt16:
                        case StorageType.SqlInt32:
                            return SqlConvert.ConvertToSqlInt32(vLeft).CompareTo(SqlConvert.ConvertToSqlInt32(vRight));

                        case StorageType.UInt32:
                        case StorageType.Int64:
                        case StorageType.SqlInt64:
                            return SqlConvert.ConvertToSqlInt64(vLeft).CompareTo(SqlConvert.ConvertToSqlInt64(vRight));

                        case StorageType.UInt64:
                        case StorageType.SqlDecimal:
                            return SqlConvert.ConvertToSqlDecimal(vLeft).CompareTo(SqlConvert.ConvertToSqlDecimal(vRight));

                        case StorageType.SqlBinary:
                            return SqlConvert.ConvertToSqlBinary(vLeft).CompareTo(SqlConvert.ConvertToSqlBinary(vRight));

                        case StorageType.SqlBoolean:
                            goto Label_034B;

                        case StorageType.SqlDateTime:
                            return SqlConvert.ConvertToSqlDateTime(vLeft).CompareTo(SqlConvert.ConvertToSqlDateTime(vRight));

                        case StorageType.SqlDouble:
                            return SqlConvert.ConvertToSqlDouble(vLeft).CompareTo(SqlConvert.ConvertToSqlDouble(vRight));

                        case StorageType.SqlGuid:
                        {
                            SqlGuid guid = (SqlGuid) vLeft;
                            return guid.CompareTo(vRight);
                        }
                        case StorageType.SqlMoney:
                            return SqlConvert.ConvertToSqlMoney(vLeft).CompareTo(SqlConvert.ConvertToSqlMoney(vRight));

                        case StorageType.SqlSingle:
                            return SqlConvert.ConvertToSqlSingle(vLeft).CompareTo(SqlConvert.ConvertToSqlSingle(vRight));

                        case StorageType.SqlString:
                            return base.table.Compare(vLeft.ToString(), vRight.ToString());
                    }
                }
                goto Label_0483;
            Label_034B:
                if ((op == 7) || (op == 12))
                {
                    num2 = 1;
                    if (((vLeft.GetType() == typeof(SqlBoolean)) && ((vRight.GetType() == typeof(SqlBoolean)) || (vRight.GetType() == typeof(bool)))) || ((vRight.GetType() == typeof(SqlBoolean)) && ((vLeft.GetType() == typeof(SqlBoolean)) || (vLeft.GetType() == typeof(bool)))))
                    {
                        return SqlConvert.ConvertToSqlBoolean(vLeft).CompareTo(SqlConvert.ConvertToSqlBoolean(vRight));
                    }
                }
            }
            catch (ArgumentException exception5)
            {
                ExceptionBuilder.TraceExceptionWithoutRethrow(exception5);
            }
            catch (FormatException exception4)
            {
                ExceptionBuilder.TraceExceptionWithoutRethrow(exception4);
            }
            catch (InvalidCastException exception3)
            {
                ExceptionBuilder.TraceExceptionWithoutRethrow(exception3);
            }
            catch (OverflowException exception2)
            {
                ExceptionBuilder.TraceExceptionWithoutRethrow(exception2);
            }
            catch (EvaluateException exception)
            {
                ExceptionBuilder.TraceExceptionWithoutRethrow(exception);
            }
        Label_0483:
            this.SetTypeMismatchError(op, vLeft.GetType(), vRight.GetType());
            return num2;
        }

        internal override void Bind(DataTable table, List<DataColumn> list)
        {
            base.BindTable(table);
            this.left.Bind(table, list);
            this.right.Bind(table, list);
        }

        internal override bool DependsOn(DataColumn column)
        {
            return (this.left.DependsOn(column) || this.right.DependsOn(column));
        }

        internal override object Eval()
        {
            return this.Eval(null, DataRowVersion.Default);
        }

        internal override object Eval(int[] recordNos)
        {
            return this.EvalBinaryOp(this.op, this.left, this.right, null, DataRowVersion.Default, recordNos);
        }

        internal override object Eval(DataRow row, DataRowVersion version)
        {
            return this.EvalBinaryOp(this.op, this.left, this.right, row, version, null);
        }

        private static object Eval(ExpressionNode expr, DataRow row, DataRowVersion version, int[] recordNos)
        {
            if (recordNos == null)
            {
                return expr.Eval(row, version);
            }
            return expr.Eval(recordNos);
        }

        private object EvalBinaryOp(int op, ExpressionNode left, ExpressionNode right, DataRow row, DataRowVersion version, int[] recordNos)
        {
            object obj2;
            object obj3;
            StorageType empty;
            if ((((op != 0x1b) && (op != 0x1a)) && ((op != 5) && (op != 13))) && (op != 0x27))
            {
                obj2 = Eval(left, row, version, recordNos);
                obj3 = Eval(right, row, version, recordNos);
                Type dataType = obj2.GetType();
                Type type4 = obj3.GetType();
                StorageType storageType = DataStorage.GetStorageType(dataType);
                StorageType type2 = DataStorage.GetStorageType(type4);
                bool flag3 = DataStorage.IsSqlType(storageType);
                bool flag2 = DataStorage.IsSqlType(type2);
                if (flag3 && DataStorage.IsObjectSqlNull(obj2))
                {
                    return obj2;
                }
                if (flag2 && DataStorage.IsObjectSqlNull(obj3))
                {
                    return obj3;
                }
                if ((obj2 == DBNull.Value) || (obj3 == DBNull.Value))
                {
                    return DBNull.Value;
                }
                if (flag3 || flag2)
                {
                    empty = this.ResultSqlType(storageType, type2, left is ConstNode, right is ConstNode, op);
                }
                else
                {
                    empty = this.ResultType(storageType, type2, left is ConstNode, right is ConstNode, op);
                }
                if (empty == StorageType.Empty)
                {
                    this.SetTypeMismatchError(op, dataType, type4);
                }
            }
            else
            {
                obj2 = obj3 = DBNull.Value;
                empty = StorageType.Empty;
            }
            object isTrue = DBNull.Value;
            bool flag = false;
            try
            {
                double num4;
                switch (op)
                {
                    case 5:
                        if (!(right is FunctionNode))
                        {
                            throw ExprException.InWithoutParentheses();
                        }
                        goto Label_165C;

                    case 7:
                        if (((obj2 != DBNull.Value) && (!left.IsSqlColumn || !DataStorage.IsObjectSqlNull(obj2))) && ((obj3 != DBNull.Value) && (!right.IsSqlColumn || !DataStorage.IsObjectSqlNull(obj3))))
                        {
                            return (0 == this.BinaryCompare(obj2, obj3, empty, 7));
                        }
                        return DBNull.Value;

                    case 8:
                        if (((obj2 != DBNull.Value) && (!left.IsSqlColumn || !DataStorage.IsObjectSqlNull(obj2))) && ((obj3 != DBNull.Value) && (!right.IsSqlColumn || !DataStorage.IsObjectSqlNull(obj3))))
                        {
                            return (0 < this.BinaryCompare(obj2, obj3, empty, op));
                        }
                        return DBNull.Value;

                    case 9:
                        if (((obj2 != DBNull.Value) && (!left.IsSqlColumn || !DataStorage.IsObjectSqlNull(obj2))) && ((obj3 != DBNull.Value) && (!right.IsSqlColumn || !DataStorage.IsObjectSqlNull(obj3))))
                        {
                            return (0 > this.BinaryCompare(obj2, obj3, empty, op));
                        }
                        return DBNull.Value;

                    case 10:
                        if (((obj2 != DBNull.Value) && (!left.IsSqlColumn || !DataStorage.IsObjectSqlNull(obj2))) && ((obj3 != DBNull.Value) && (!right.IsSqlColumn || !DataStorage.IsObjectSqlNull(obj3))))
                        {
                            return (0 <= this.BinaryCompare(obj2, obj3, empty, op));
                        }
                        return DBNull.Value;

                    case 11:
                        if (((obj2 != DBNull.Value) && (!left.IsSqlColumn || !DataStorage.IsObjectSqlNull(obj2))) && ((obj3 != DBNull.Value) && (!right.IsSqlColumn || !DataStorage.IsObjectSqlNull(obj3))))
                        {
                            return (0 >= this.BinaryCompare(obj2, obj3, empty, op));
                        }
                        return DBNull.Value;

                    case 12:
                        if (((obj2 != DBNull.Value) && (!left.IsSqlColumn || !DataStorage.IsObjectSqlNull(obj2))) && ((obj3 != DBNull.Value) && (!right.IsSqlColumn || !DataStorage.IsObjectSqlNull(obj3))))
                        {
                            return (0 != this.BinaryCompare(obj2, obj3, empty, op));
                        }
                        return DBNull.Value;

                    case 13:
                        obj2 = Eval(left, row, version, recordNos);
                        if ((obj2 != DBNull.Value) && (!left.IsSqlColumn || !DataStorage.IsObjectSqlNull(obj2)))
                        {
                            return false;
                        }
                        return true;

                    case 15:
                        switch (empty)
                        {
                            case StorageType.Char:
                            case StorageType.String:
                                goto Label_03FB;

                            case StorageType.SByte:
                                goto Label_025F;

                            case StorageType.Int16:
                                goto Label_0293;

                            case StorageType.UInt16:
                                goto Label_02C7;

                            case StorageType.Int32:
                                goto Label_02FB;

                            case StorageType.UInt32:
                                goto Label_031F;

                            case StorageType.Int64:
                                goto Label_0367;

                            case StorageType.UInt64:
                                goto Label_0343;

                            case StorageType.Single:
                                goto Label_03B3;

                            case StorageType.Double:
                                goto Label_03D7;

                            case StorageType.Decimal:
                                goto Label_038B;

                            case StorageType.DateTime:
                                goto Label_041E;

                            case StorageType.TimeSpan:
                                goto Label_047E;

                            case StorageType.SqlByte:
                                goto Label_055E;

                            case StorageType.SqlDateTime:
                                goto Label_0596;

                            case StorageType.SqlDecimal:
                                goto Label_0526;

                            case StorageType.SqlDouble:
                                goto Label_04EE;

                            case StorageType.SqlInt16:
                                goto Label_049A;

                            case StorageType.SqlInt32:
                                goto Label_04B6;

                            case StorageType.SqlInt64:
                                goto Label_04D2;

                            case StorageType.SqlMoney:
                                goto Label_0542;

                            case StorageType.SqlSingle:
                                goto Label_050A;

                            case StorageType.SqlString:
                                goto Label_057A;
                        }
                        goto Label_061C;

                    case 0x10:
                        switch (empty)
                        {
                            case StorageType.SByte:
                                goto Label_0711;

                            case StorageType.Byte:
                                goto Label_06C1;

                            case StorageType.Int16:
                                goto Label_0745;

                            case StorageType.UInt16:
                                goto Label_0795;

                            case StorageType.Int32:
                                goto Label_07C9;

                            case StorageType.UInt32:
                                goto Label_0809;

                            case StorageType.Int64:
                                goto Label_082D;

                            case StorageType.UInt64:
                                goto Label_086D;

                            case StorageType.Single:
                                goto Label_08D5;

                            case StorageType.Double:
                                goto Label_0915;

                            case StorageType.Decimal:
                                goto Label_0891;

                            case StorageType.DateTime:
                                goto Label_0971;

                            case StorageType.TimeSpan:
                                goto Label_098D;

                            case StorageType.SqlByte:
                                goto Label_06F5;

                            case StorageType.SqlDateTime:
                                goto Label_09CD;

                            case StorageType.SqlDecimal:
                                goto Label_08B9;

                            case StorageType.SqlDouble:
                                goto Label_0939;

                            case StorageType.SqlInt16:
                                goto Label_0779;

                            case StorageType.SqlInt32:
                                goto Label_07ED;

                            case StorageType.SqlInt64:
                                goto Label_0851;

                            case StorageType.SqlMoney:
                                goto Label_0955;

                            case StorageType.SqlSingle:
                                goto Label_08F9;
                        }
                        goto Label_0A53;

                    case 0x11:
                        switch (empty)
                        {
                            case StorageType.SByte:
                                goto Label_0B48;

                            case StorageType.Byte:
                                goto Label_0AF8;

                            case StorageType.Int16:
                                goto Label_0B7C;

                            case StorageType.UInt16:
                                goto Label_0BCC;

                            case StorageType.Int32:
                                goto Label_0C00;

                            case StorageType.UInt32:
                                goto Label_0C40;

                            case StorageType.Int64:
                                goto Label_0C64;

                            case StorageType.UInt64:
                                goto Label_0CA4;

                            case StorageType.Single:
                                goto Label_0D0C;

                            case StorageType.Double:
                                goto Label_0D68;

                            case StorageType.Decimal:
                                goto Label_0CC8;

                            case StorageType.SqlByte:
                                goto Label_0B2C;

                            case StorageType.SqlDecimal:
                                goto Label_0CF0;

                            case StorageType.SqlDouble:
                                goto Label_0D8C;

                            case StorageType.SqlInt16:
                                goto Label_0BB0;

                            case StorageType.SqlInt32:
                                goto Label_0C24;

                            case StorageType.SqlInt64:
                                goto Label_0C88;

                            case StorageType.SqlMoney:
                                goto Label_0D4C;

                            case StorageType.SqlSingle:
                                goto Label_0D30;
                        }
                        goto Label_0DA8;

                    case 0x12:
                        switch (empty)
                        {
                            case StorageType.SByte:
                                goto Label_0E9D;

                            case StorageType.Byte:
                                goto Label_0E4D;

                            case StorageType.Int16:
                                goto Label_0ED1;

                            case StorageType.UInt16:
                                goto Label_0F21;

                            case StorageType.Int32:
                                goto Label_0F55;

                            case StorageType.UInt32:
                                goto Label_0F95;

                            case StorageType.Int64:
                                goto Label_0FDD;

                            case StorageType.UInt64:
                                goto Label_0FB9;

                            case StorageType.Single:
                                goto Label_1061;

                            case StorageType.Double:
                                goto Label_10BD;

                            case StorageType.Decimal:
                                goto Label_101D;

                            case StorageType.SqlByte:
                                goto Label_0E81;

                            case StorageType.SqlDecimal:
                                goto Label_1045;

                            case StorageType.SqlDouble:
                                goto Label_10E5;

                            case StorageType.SqlInt16:
                                goto Label_0F05;

                            case StorageType.SqlInt32:
                                goto Label_0F79;

                            case StorageType.SqlInt64:
                                goto Label_1001;

                            case StorageType.SqlMoney:
                                goto Label_10A1;

                            case StorageType.SqlSingle:
                                goto Label_1085;
                        }
                        goto Label_1101;

                    case 20:
                        if (!ExpressionNode.IsIntegerSql(empty))
                        {
                            goto Label_1646;
                        }
                        if (empty != StorageType.UInt64)
                        {
                            goto Label_15A2;
                        }
                        isTrue = Convert.ToUInt64(obj2, base.FormatProvider) % Convert.ToUInt64(obj3, base.FormatProvider);
                        goto Label_1715;

                    case 0x1a:
                        obj2 = Eval(left, row, version, recordNos);
                        if ((obj2 != DBNull.Value) && (!left.IsSqlColumn || !DataStorage.IsObjectSqlNull(obj2)))
                        {
                            goto Label_13BF;
                        }
                        return DBNull.Value;

                    case 0x1b:
                        obj2 = Eval(left, row, version, recordNos);
                        if ((obj2 == DBNull.Value) || DataStorage.IsObjectSqlNull(obj2))
                        {
                            goto Label_14EF;
                        }
                        if ((obj2 is bool) || (obj2 is SqlBoolean))
                        {
                            goto Label_14DB;
                        }
                        obj3 = Eval(right, row, version, recordNos);
                        flag = true;
                        goto Label_1715;

                    case 0x27:
                        obj2 = Eval(left, row, version, recordNos);
                        if ((obj2 == DBNull.Value) || (left.IsSqlColumn && DataStorage.IsObjectSqlNull(obj2)))
                        {
                            return false;
                        }
                        return true;

                    default:
                        throw ExprException.UnsupportedOperator(op);
                }
                isTrue = Convert.ToByte(Convert.ToByte(obj2, base.FormatProvider) + Convert.ToByte(obj3, base.FormatProvider), base.FormatProvider);
                goto Label_1715;
            Label_025F:
                isTrue = Convert.ToSByte(Convert.ToSByte(obj2, base.FormatProvider) + Convert.ToSByte(obj3, base.FormatProvider), base.FormatProvider);
                goto Label_1715;
            Label_0293:
                isTrue = Convert.ToInt16(Convert.ToInt16(obj2, base.FormatProvider) + Convert.ToInt16(obj3, base.FormatProvider), base.FormatProvider);
                goto Label_1715;
            Label_02C7:
                isTrue = Convert.ToUInt16(Convert.ToUInt16(obj2, base.FormatProvider) + Convert.ToUInt16(obj3, base.FormatProvider), base.FormatProvider);
                goto Label_1715;
            Label_02FB:
                isTrue = Convert.ToInt32(obj2, base.FormatProvider) + Convert.ToInt32(obj3, base.FormatProvider);
                goto Label_1715;
            Label_031F:
                isTrue = Convert.ToUInt32(obj2, base.FormatProvider) + Convert.ToUInt32(obj3, base.FormatProvider);
                goto Label_1715;
            Label_0343:
                isTrue = Convert.ToUInt64(obj2, base.FormatProvider) + Convert.ToUInt64(obj3, base.FormatProvider);
                goto Label_1715;
            Label_0367:
                isTrue = Convert.ToInt64(obj2, base.FormatProvider) + Convert.ToInt64(obj3, base.FormatProvider);
                goto Label_1715;
            Label_038B:
                isTrue = Convert.ToDecimal(obj2, base.FormatProvider) + Convert.ToDecimal(obj3, base.FormatProvider);
                goto Label_1715;
            Label_03B3:
                isTrue = Convert.ToSingle(obj2, base.FormatProvider) + Convert.ToSingle(obj3, base.FormatProvider);
                goto Label_1715;
            Label_03D7:
                isTrue = Convert.ToDouble(obj2, base.FormatProvider) + Convert.ToDouble(obj3, base.FormatProvider);
                goto Label_1715;
            Label_03FB:
                isTrue = Convert.ToString(obj2, base.FormatProvider) + Convert.ToString(obj3, base.FormatProvider);
                goto Label_1715;
            Label_041E:
                if ((obj2 is TimeSpan) && (obj3 is DateTime))
                {
                    isTrue = ((DateTime) obj3) + ((TimeSpan) obj2);
                }
                else if ((obj2 is DateTime) && (obj3 is TimeSpan))
                {
                    isTrue = ((DateTime) obj2) + ((TimeSpan) obj3);
                }
                else
                {
                    flag = true;
                }
                goto Label_1715;
            Label_047E:
                isTrue = ((TimeSpan) obj2) + ((TimeSpan) obj3);
                goto Label_1715;
            Label_049A:
                isTrue = SqlConvert.ConvertToSqlInt16(obj2) + SqlConvert.ConvertToSqlInt16(obj3);
                goto Label_1715;
            Label_04B6:
                isTrue = SqlConvert.ConvertToSqlInt32(obj2) + SqlConvert.ConvertToSqlInt32(obj3);
                goto Label_1715;
            Label_04D2:
                isTrue = SqlConvert.ConvertToSqlInt64(obj2) + SqlConvert.ConvertToSqlInt64(obj3);
                goto Label_1715;
            Label_04EE:
                isTrue = SqlConvert.ConvertToSqlDouble(obj2) + SqlConvert.ConvertToSqlDouble(obj3);
                goto Label_1715;
            Label_050A:
                isTrue = SqlConvert.ConvertToSqlSingle(obj2) + SqlConvert.ConvertToSqlSingle(obj3);
                goto Label_1715;
            Label_0526:
                isTrue = SqlConvert.ConvertToSqlDecimal(obj2) + SqlConvert.ConvertToSqlDecimal(obj3);
                goto Label_1715;
            Label_0542:
                isTrue = SqlConvert.ConvertToSqlMoney(obj2) + SqlConvert.ConvertToSqlMoney(obj3);
                goto Label_1715;
            Label_055E:
                isTrue = SqlConvert.ConvertToSqlByte(obj2) + SqlConvert.ConvertToSqlByte(obj3);
                goto Label_1715;
            Label_057A:
                isTrue = SqlConvert.ConvertToSqlString(obj2) + SqlConvert.ConvertToSqlString(obj3);
                goto Label_1715;
            Label_0596:
                if ((obj2 is TimeSpan) && (obj3 is SqlDateTime))
                {
                    isTrue = SqlConvert.ConvertToSqlDateTime(SqlConvert.ConvertToSqlDateTime(obj3).Value + ((TimeSpan) obj2));
                }
                else if ((obj2 is SqlDateTime) && (obj3 is TimeSpan))
                {
                    isTrue = SqlConvert.ConvertToSqlDateTime(SqlConvert.ConvertToSqlDateTime(obj2).Value + ((TimeSpan) obj3));
                }
                else
                {
                    flag = true;
                }
                goto Label_1715;
            Label_061C:
                flag = true;
                goto Label_1715;
            Label_06C1:
                isTrue = Convert.ToByte(Convert.ToByte(obj2, base.FormatProvider) - Convert.ToByte(obj3, base.FormatProvider), base.FormatProvider);
                goto Label_1715;
            Label_06F5:
                isTrue = SqlConvert.ConvertToSqlByte(obj2) - SqlConvert.ConvertToSqlByte(obj3);
                goto Label_1715;
            Label_0711:
                isTrue = Convert.ToSByte(Convert.ToSByte(obj2, base.FormatProvider) - Convert.ToSByte(obj3, base.FormatProvider), base.FormatProvider);
                goto Label_1715;
            Label_0745:
                isTrue = Convert.ToInt16(Convert.ToInt16(obj2, base.FormatProvider) - Convert.ToInt16(obj3, base.FormatProvider), base.FormatProvider);
                goto Label_1715;
            Label_0779:
                isTrue = SqlConvert.ConvertToSqlInt16(obj2) - SqlConvert.ConvertToSqlInt16(obj3);
                goto Label_1715;
            Label_0795:
                isTrue = Convert.ToUInt16(Convert.ToUInt16(obj2, base.FormatProvider) - Convert.ToUInt16(obj3, base.FormatProvider), base.FormatProvider);
                goto Label_1715;
            Label_07C9:
                isTrue = Convert.ToInt32(obj2, base.FormatProvider) - Convert.ToInt32(obj3, base.FormatProvider);
                goto Label_1715;
            Label_07ED:
                isTrue = SqlConvert.ConvertToSqlInt32(obj2) - SqlConvert.ConvertToSqlInt32(obj3);
                goto Label_1715;
            Label_0809:
                isTrue = Convert.ToUInt32(obj2, base.FormatProvider) - Convert.ToUInt32(obj3, base.FormatProvider);
                goto Label_1715;
            Label_082D:
                isTrue = Convert.ToInt64(obj2, base.FormatProvider) - Convert.ToInt64(obj3, base.FormatProvider);
                goto Label_1715;
            Label_0851:
                isTrue = SqlConvert.ConvertToSqlInt64(obj2) - SqlConvert.ConvertToSqlInt64(obj3);
                goto Label_1715;
            Label_086D:
                isTrue = Convert.ToUInt64(obj2, base.FormatProvider) - Convert.ToUInt64(obj3, base.FormatProvider);
                goto Label_1715;
            Label_0891:
                isTrue = Convert.ToDecimal(obj2, base.FormatProvider) - Convert.ToDecimal(obj3, base.FormatProvider);
                goto Label_1715;
            Label_08B9:
                isTrue = SqlConvert.ConvertToSqlDecimal(obj2) - SqlConvert.ConvertToSqlDecimal(obj3);
                goto Label_1715;
            Label_08D5:
                isTrue = Convert.ToSingle(obj2, base.FormatProvider) - Convert.ToSingle(obj3, base.FormatProvider);
                goto Label_1715;
            Label_08F9:
                isTrue = SqlConvert.ConvertToSqlSingle(obj2) - SqlConvert.ConvertToSqlSingle(obj3);
                goto Label_1715;
            Label_0915:
                isTrue = Convert.ToDouble(obj2, base.FormatProvider) - Convert.ToDouble(obj3, base.FormatProvider);
                goto Label_1715;
            Label_0939:
                isTrue = SqlConvert.ConvertToSqlDouble(obj2) - SqlConvert.ConvertToSqlDouble(obj3);
                goto Label_1715;
            Label_0955:
                isTrue = SqlConvert.ConvertToSqlMoney(obj2) - SqlConvert.ConvertToSqlMoney(obj3);
                goto Label_1715;
            Label_0971:
                isTrue = ((DateTime) obj2) - ((TimeSpan) obj3);
                goto Label_1715;
            Label_098D:
                if (obj2 is DateTime)
                {
                    isTrue = (TimeSpan) (((DateTime) obj2) - ((DateTime) obj3));
                }
                else
                {
                    isTrue = ((TimeSpan) obj2) - ((TimeSpan) obj3);
                }
                goto Label_1715;
            Label_09CD:
                if ((obj2 is TimeSpan) && (obj3 is SqlDateTime))
                {
                    isTrue = SqlConvert.ConvertToSqlDateTime(SqlConvert.ConvertToSqlDateTime(obj3).Value - ((TimeSpan) obj2));
                }
                else if ((obj2 is SqlDateTime) && (obj3 is TimeSpan))
                {
                    isTrue = SqlConvert.ConvertToSqlDateTime(SqlConvert.ConvertToSqlDateTime(obj2).Value - ((TimeSpan) obj3));
                }
                else
                {
                    flag = true;
                }
                goto Label_1715;
            Label_0A53:
                flag = true;
                goto Label_1715;
            Label_0AF8:
                isTrue = Convert.ToByte(Convert.ToByte(obj2, base.FormatProvider) * Convert.ToByte(obj3, base.FormatProvider), base.FormatProvider);
                goto Label_1715;
            Label_0B2C:
                isTrue = SqlConvert.ConvertToSqlByte(obj2) * SqlConvert.ConvertToSqlByte(obj3);
                goto Label_1715;
            Label_0B48:
                isTrue = Convert.ToSByte(Convert.ToSByte(obj2, base.FormatProvider) * Convert.ToSByte(obj3, base.FormatProvider), base.FormatProvider);
                goto Label_1715;
            Label_0B7C:
                isTrue = Convert.ToInt16(Convert.ToInt16(obj2, base.FormatProvider) * Convert.ToInt16(obj3, base.FormatProvider), base.FormatProvider);
                goto Label_1715;
            Label_0BB0:
                isTrue = SqlConvert.ConvertToSqlInt16(obj2) * SqlConvert.ConvertToSqlInt16(obj3);
                goto Label_1715;
            Label_0BCC:
                isTrue = Convert.ToUInt16(Convert.ToUInt16(obj2, base.FormatProvider) * Convert.ToUInt16(obj3, base.FormatProvider), base.FormatProvider);
                goto Label_1715;
            Label_0C00:
                isTrue = Convert.ToInt32(obj2, base.FormatProvider) * Convert.ToInt32(obj3, base.FormatProvider);
                goto Label_1715;
            Label_0C24:
                isTrue = SqlConvert.ConvertToSqlInt32(obj2) * SqlConvert.ConvertToSqlInt32(obj3);
                goto Label_1715;
            Label_0C40:
                isTrue = Convert.ToUInt32(obj2, base.FormatProvider) * Convert.ToUInt32(obj3, base.FormatProvider);
                goto Label_1715;
            Label_0C64:
                isTrue = Convert.ToInt64(obj2, base.FormatProvider) * Convert.ToInt64(obj3, base.FormatProvider);
                goto Label_1715;
            Label_0C88:
                isTrue = SqlConvert.ConvertToSqlInt64(obj2) * SqlConvert.ConvertToSqlInt64(obj3);
                goto Label_1715;
            Label_0CA4:
                isTrue = Convert.ToUInt64(obj2, base.FormatProvider) * Convert.ToUInt64(obj3, base.FormatProvider);
                goto Label_1715;
            Label_0CC8:
                isTrue = Convert.ToDecimal(obj2, base.FormatProvider) * Convert.ToDecimal(obj3, base.FormatProvider);
                goto Label_1715;
            Label_0CF0:
                isTrue = SqlConvert.ConvertToSqlDecimal(obj2) * SqlConvert.ConvertToSqlDecimal(obj3);
                goto Label_1715;
            Label_0D0C:
                isTrue = Convert.ToSingle(obj2, base.FormatProvider) * Convert.ToSingle(obj3, base.FormatProvider);
                goto Label_1715;
            Label_0D30:
                isTrue = SqlConvert.ConvertToSqlSingle(obj2) * SqlConvert.ConvertToSqlSingle(obj3);
                goto Label_1715;
            Label_0D4C:
                isTrue = SqlConvert.ConvertToSqlMoney(obj2) * SqlConvert.ConvertToSqlMoney(obj3);
                goto Label_1715;
            Label_0D68:
                isTrue = Convert.ToDouble(obj2, base.FormatProvider) * Convert.ToDouble(obj3, base.FormatProvider);
                goto Label_1715;
            Label_0D8C:
                isTrue = SqlConvert.ConvertToSqlDouble(obj2) * SqlConvert.ConvertToSqlDouble(obj3);
                goto Label_1715;
            Label_0DA8:
                flag = true;
                goto Label_1715;
            Label_0E4D:
                isTrue = Convert.ToByte(Convert.ToByte(obj2, base.FormatProvider) / Convert.ToByte(obj3, base.FormatProvider), base.FormatProvider);
                goto Label_1715;
            Label_0E81:
                isTrue = SqlConvert.ConvertToSqlByte(obj2) / SqlConvert.ConvertToSqlByte(obj3);
                goto Label_1715;
            Label_0E9D:
                isTrue = Convert.ToSByte(Convert.ToSByte(obj2, base.FormatProvider) / Convert.ToSByte(obj3, base.FormatProvider), base.FormatProvider);
                goto Label_1715;
            Label_0ED1:
                isTrue = Convert.ToInt16(Convert.ToInt16(obj2, base.FormatProvider) / Convert.ToInt16(obj3, base.FormatProvider), base.FormatProvider);
                goto Label_1715;
            Label_0F05:
                isTrue = SqlConvert.ConvertToSqlInt16(obj2) / SqlConvert.ConvertToSqlInt16(obj3);
                goto Label_1715;
            Label_0F21:
                isTrue = Convert.ToUInt16(Convert.ToUInt16(obj2, base.FormatProvider) / Convert.ToUInt16(obj3, base.FormatProvider), base.FormatProvider);
                goto Label_1715;
            Label_0F55:
                isTrue = Convert.ToInt32(obj2, base.FormatProvider) / Convert.ToInt32(obj3, base.FormatProvider);
                goto Label_1715;
            Label_0F79:
                isTrue = SqlConvert.ConvertToSqlInt32(obj2) / SqlConvert.ConvertToSqlInt32(obj3);
                goto Label_1715;
            Label_0F95:
                isTrue = Convert.ToUInt32(obj2, base.FormatProvider) / Convert.ToUInt32(obj3, base.FormatProvider);
                goto Label_1715;
            Label_0FB9:
                isTrue = Convert.ToUInt64(obj2, base.FormatProvider) / Convert.ToUInt64(obj3, base.FormatProvider);
                goto Label_1715;
            Label_0FDD:
                isTrue = Convert.ToInt64(obj2, base.FormatProvider) / Convert.ToInt64(obj3, base.FormatProvider);
                goto Label_1715;
            Label_1001:
                isTrue = SqlConvert.ConvertToSqlInt64(obj2) / SqlConvert.ConvertToSqlInt64(obj3);
                goto Label_1715;
            Label_101D:
                isTrue = Convert.ToDecimal(obj2, base.FormatProvider) / Convert.ToDecimal(obj3, base.FormatProvider);
                goto Label_1715;
            Label_1045:
                isTrue = SqlConvert.ConvertToSqlDecimal(obj2) / SqlConvert.ConvertToSqlDecimal(obj3);
                goto Label_1715;
            Label_1061:
                isTrue = Convert.ToSingle(obj2, base.FormatProvider) / Convert.ToSingle(obj3, base.FormatProvider);
                goto Label_1715;
            Label_1085:
                isTrue = SqlConvert.ConvertToSqlSingle(obj2) / SqlConvert.ConvertToSqlSingle(obj3);
                goto Label_1715;
            Label_10A1:
                isTrue = SqlConvert.ConvertToSqlMoney(obj2) / SqlConvert.ConvertToSqlMoney(obj3);
                goto Label_1715;
            Label_10BD:
                num4 = Convert.ToDouble(obj3, base.FormatProvider);
                isTrue = Convert.ToDouble(obj2, base.FormatProvider) / num4;
                goto Label_1715;
            Label_10E5:
                isTrue = SqlConvert.ConvertToSqlDouble(obj2) / SqlConvert.ConvertToSqlDouble(obj3);
                goto Label_1715;
            Label_1101:
                flag = true;
                goto Label_1715;
            Label_13BF:
                if (!(obj2 is bool) && !(obj2 is SqlBoolean))
                {
                    obj3 = Eval(right, row, version, recordNos);
                    flag = true;
                    goto Label_1715;
                }
                if (obj2 is bool)
                {
                    if ((bool) obj2)
                    {
                        goto Label_141D;
                    }
                    isTrue = false;
                    goto Label_1715;
                }
                SqlBoolean flag6 = (SqlBoolean) obj2;
                if (flag6.IsFalse)
                {
                    isTrue = false;
                    goto Label_1715;
                }
            Label_141D:
                obj3 = Eval(right, row, version, recordNos);
                if ((obj3 == DBNull.Value) || (right.IsSqlColumn && DataStorage.IsObjectSqlNull(obj3)))
                {
                    return DBNull.Value;
                }
                if ((obj3 is bool) || (obj3 is SqlBoolean))
                {
                    if (obj3 is bool)
                    {
                        isTrue = (bool) obj3;
                    }
                    else
                    {
                        SqlBoolean flag5 = (SqlBoolean) obj3;
                        isTrue = flag5.IsTrue;
                    }
                }
                else
                {
                    flag = true;
                }
                goto Label_1715;
            Label_14DB:
                if ((bool) obj2)
                {
                    isTrue = true;
                    goto Label_1715;
                }
            Label_14EF:
                obj3 = Eval(right, row, version, recordNos);
                if ((obj3 == DBNull.Value) || DataStorage.IsObjectSqlNull(obj3))
                {
                    return obj2;
                }
                if ((obj2 == DBNull.Value) || DataStorage.IsObjectSqlNull(obj2))
                {
                    return obj3;
                }
                if ((obj3 is bool) || (obj3 is SqlBoolean))
                {
                    isTrue = (obj3 is bool) ? ((bool) obj3) : ((SqlBoolean) obj3).IsTrue;
                }
                else
                {
                    flag = true;
                }
                goto Label_1715;
            Label_15A2:
                if (DataStorage.IsSqlType(empty))
                {
                    SqlInt64 num3 = SqlConvert.ConvertToSqlInt64(obj2) % SqlConvert.ConvertToSqlInt64(obj3);
                    switch (empty)
                    {
                        case StorageType.SqlInt32:
                            isTrue = num3.ToSqlInt32();
                            goto Label_1715;

                        case StorageType.SqlInt16:
                            isTrue = num3.ToSqlInt16();
                            goto Label_1715;

                        case StorageType.SqlByte:
                            isTrue = num3.ToSqlByte();
                            goto Label_1715;
                    }
                    isTrue = num3;
                }
                else
                {
                    isTrue = Convert.ToInt64(obj2, base.FormatProvider) % Convert.ToInt64(obj3, base.FormatProvider);
                    isTrue = Convert.ChangeType(isTrue, DataStorage.GetTypeStorage(empty), base.FormatProvider);
                }
                goto Label_1715;
            Label_1646:
                flag = true;
                goto Label_1715;
            Label_165C:
                obj2 = Eval(left, row, version, recordNos);
                if ((obj2 == DBNull.Value) || (left.IsSqlColumn && DataStorage.IsObjectSqlNull(obj2)))
                {
                    return DBNull.Value;
                }
                isTrue = false;
                FunctionNode node = (FunctionNode) right;
                for (int i = 0; i < node.argumentCount; i++)
                {
                    obj3 = node.arguments[i].Eval();
                    if ((obj3 != DBNull.Value) && (!right.IsSqlColumn || !DataStorage.IsObjectSqlNull(obj3)))
                    {
                        empty = DataStorage.GetStorageType(obj2.GetType());
                        if (this.BinaryCompare(obj2, obj3, empty, 7) == 0)
                        {
                            isTrue = true;
                            break;
                        }
                    }
                }
            }
            catch (OverflowException)
            {
                throw ExprException.Overflow(DataStorage.GetTypeStorage(empty));
            }
        Label_1715:
            if (flag)
            {
                this.SetTypeMismatchError(op, obj2.GetType(), obj3.GetType());
            }
            return isTrue;
        }

        private DataTypePrecedence GetPrecedence(StorageType storageType)
        {
            switch (storageType)
            {
                case StorageType.Boolean:
                    return DataTypePrecedence.Boolean;

                case StorageType.Char:
                    return DataTypePrecedence.Char;

                case StorageType.SByte:
                    return DataTypePrecedence.SByte;

                case StorageType.Byte:
                    return DataTypePrecedence.Byte;

                case StorageType.Int16:
                    return DataTypePrecedence.Int16;

                case StorageType.UInt16:
                    return DataTypePrecedence.UInt16;

                case StorageType.Int32:
                    return DataTypePrecedence.Int32;

                case StorageType.UInt32:
                    return DataTypePrecedence.UInt32;

                case StorageType.Int64:
                    return DataTypePrecedence.Int64;

                case StorageType.UInt64:
                    return DataTypePrecedence.UInt64;

                case StorageType.Single:
                    return DataTypePrecedence.Single;

                case StorageType.Double:
                    return DataTypePrecedence.Double;

                case StorageType.Decimal:
                    return DataTypePrecedence.Decimal;

                case StorageType.DateTime:
                    return DataTypePrecedence.DateTime;

                case StorageType.TimeSpan:
                    return DataTypePrecedence.TimeSpan;

                case StorageType.String:
                    return DataTypePrecedence.String;

                case StorageType.DateTimeOffset:
                    return DataTypePrecedence.DateTimeOffset;

                case StorageType.SqlBinary:
                    return DataTypePrecedence.SqlBinary;

                case StorageType.SqlBoolean:
                    return DataTypePrecedence.SqlBoolean;

                case StorageType.SqlByte:
                    return DataTypePrecedence.SqlByte;

                case StorageType.SqlBytes:
                    return DataTypePrecedence.SqlBytes;

                case StorageType.SqlChars:
                    return DataTypePrecedence.SqlChars;

                case StorageType.SqlDateTime:
                    return DataTypePrecedence.SqlDateTime;

                case StorageType.SqlDecimal:
                    return DataTypePrecedence.SqlDecimal;

                case StorageType.SqlDouble:
                    return DataTypePrecedence.SqlDouble;

                case StorageType.SqlGuid:
                    return DataTypePrecedence.SqlGuid;

                case StorageType.SqlInt16:
                    return DataTypePrecedence.SqlInt16;

                case StorageType.SqlInt32:
                    return DataTypePrecedence.SqlInt32;

                case StorageType.SqlInt64:
                    return DataTypePrecedence.SqlInt64;

                case StorageType.SqlMoney:
                    return DataTypePrecedence.SqlMoney;

                case StorageType.SqlSingle:
                    return DataTypePrecedence.SqlSingle;

                case StorageType.SqlString:
                    return DataTypePrecedence.SqlString;
            }
            return DataTypePrecedence.Error;
        }

        private static StorageType GetPrecedenceType(DataTypePrecedence code)
        {
            switch (code)
            {
                case DataTypePrecedence.SqlBinary:
                    return StorageType.SqlBinary;

                case DataTypePrecedence.Char:
                    return StorageType.Char;

                case DataTypePrecedence.String:
                    return StorageType.String;

                case DataTypePrecedence.SqlString:
                    return StorageType.SqlString;

                case DataTypePrecedence.SqlGuid:
                    return StorageType.SqlGuid;

                case DataTypePrecedence.Boolean:
                    return StorageType.Boolean;

                case DataTypePrecedence.SqlBoolean:
                    return StorageType.SqlBoolean;

                case DataTypePrecedence.SByte:
                    return StorageType.SByte;

                case DataTypePrecedence.SqlByte:
                    return StorageType.SqlByte;

                case DataTypePrecedence.Byte:
                    return StorageType.Byte;

                case DataTypePrecedence.Int16:
                    return StorageType.Int16;

                case DataTypePrecedence.SqlInt16:
                    return StorageType.SqlInt16;

                case DataTypePrecedence.UInt16:
                    return StorageType.UInt16;

                case DataTypePrecedence.Int32:
                    return StorageType.Int32;

                case DataTypePrecedence.SqlInt32:
                    return StorageType.SqlInt32;

                case DataTypePrecedence.UInt32:
                    return StorageType.UInt32;

                case DataTypePrecedence.Int64:
                    return StorageType.Int64;

                case DataTypePrecedence.SqlInt64:
                    return StorageType.SqlInt64;

                case DataTypePrecedence.UInt64:
                    return StorageType.UInt64;

                case DataTypePrecedence.SqlMoney:
                    return StorageType.SqlMoney;

                case DataTypePrecedence.Decimal:
                    return StorageType.Decimal;

                case DataTypePrecedence.SqlDecimal:
                    return StorageType.SqlDecimal;

                case DataTypePrecedence.Single:
                    return StorageType.Single;

                case DataTypePrecedence.SqlSingle:
                    return StorageType.SqlSingle;

                case DataTypePrecedence.Double:
                    return StorageType.Double;

                case DataTypePrecedence.SqlDouble:
                    return StorageType.SqlDouble;

                case DataTypePrecedence.TimeSpan:
                    return StorageType.TimeSpan;

                case DataTypePrecedence.DateTime:
                    return StorageType.DateTime;

                case DataTypePrecedence.DateTimeOffset:
                    return StorageType.DateTimeOffset;

                case DataTypePrecedence.SqlDateTime:
                    return StorageType.SqlDateTime;
            }
            return StorageType.Empty;
        }

        internal override bool HasLocalAggregate()
        {
            if (!this.left.HasLocalAggregate())
            {
                return this.right.HasLocalAggregate();
            }
            return true;
        }

        internal override bool HasRemoteAggregate()
        {
            if (!this.left.HasRemoteAggregate())
            {
                return this.right.HasRemoteAggregate();
            }
            return true;
        }

        internal override bool IsConstant()
        {
            return (this.left.IsConstant() && this.right.IsConstant());
        }

        private bool IsMixed(StorageType left, StorageType right)
        {
            return ((ExpressionNode.IsSigned(left) && ExpressionNode.IsUnsigned(right)) || (ExpressionNode.IsUnsigned(left) && ExpressionNode.IsSigned(right)));
        }

        private bool IsMixedSql(StorageType left, StorageType right)
        {
            return ((ExpressionNode.IsSignedSql(left) && ExpressionNode.IsUnsignedSql(right)) || (ExpressionNode.IsUnsignedSql(left) && ExpressionNode.IsSignedSql(right)));
        }

        internal override bool IsTableConstant()
        {
            return (this.left.IsTableConstant() && this.right.IsTableConstant());
        }

        internal override ExpressionNode Optimize()
        {
            this.left = this.left.Optimize();
            if (this.op == 13)
            {
                if (this.right is UnaryNode)
                {
                    UnaryNode right = (UnaryNode) this.right;
                    if (right.op != 3)
                    {
                        throw ExprException.InvalidIsSyntax();
                    }
                    this.op = 0x27;
                    this.right = right.right;
                }
                if (!(this.right is ZeroOpNode))
                {
                    throw ExprException.InvalidIsSyntax();
                }
                if (((ZeroOpNode) this.right).op != 0x20)
                {
                    throw ExprException.InvalidIsSyntax();
                }
            }
            else
            {
                this.right = this.right.Optimize();
            }
            if (!this.IsConstant())
            {
                return this;
            }
            object constant = this.Eval();
            if (constant == DBNull.Value)
            {
                return new ZeroOpNode(0x20);
            }
            if (!(constant is bool))
            {
                return new ConstNode(base.table, System.Data.ValueType.Object, constant, false);
            }
            if ((bool) constant)
            {
                return new ZeroOpNode(0x21);
            }
            return new ZeroOpNode(0x22);
        }

        internal StorageType ResultSqlType(StorageType left, StorageType right, bool lc, bool rc, int op)
        {
            int num = (int) this.GetPrecedence(left);
            if (num == 0)
            {
                return StorageType.Empty;
            }
            int num2 = (int) this.GetPrecedence(right);
            if (num2 == 0)
            {
                return StorageType.Empty;
            }
            if (Operators.IsLogical(op))
            {
                if (((left != StorageType.Boolean) && (left != StorageType.SqlBoolean)) || ((right != StorageType.Boolean) && (right != StorageType.SqlBoolean)))
                {
                    return StorageType.Empty;
                }
                if ((left == StorageType.Boolean) && (right == StorageType.Boolean))
                {
                    return StorageType.Boolean;
                }
                return StorageType.SqlBoolean;
            }
            if (op == 15)
            {
                if ((left == StorageType.SqlString) || (right == StorageType.SqlString))
                {
                    return StorageType.SqlString;
                }
                if ((left == StorageType.String) || (right == StorageType.String))
                {
                    return StorageType.String;
                }
            }
            if (((left == StorageType.SqlBinary) && (right != StorageType.SqlBinary)) || ((left != StorageType.SqlBinary) && (right == StorageType.SqlBinary)))
            {
                return StorageType.Empty;
            }
            if (((left == StorageType.SqlGuid) && (right != StorageType.SqlGuid)) || ((left != StorageType.SqlGuid) && (right == StorageType.SqlGuid)))
            {
                return StorageType.Empty;
            }
            if ((num > 0x13) && (num2 < 20))
            {
                return StorageType.Empty;
            }
            if ((num < 20) && (num2 > 0x13))
            {
                return StorageType.Empty;
            }
            if (num > 0x13)
            {
                if ((op == 15) || (op == 0x10))
                {
                    if (left == StorageType.TimeSpan)
                    {
                        return right;
                    }
                    if (right == StorageType.TimeSpan)
                    {
                        return left;
                    }
                    return StorageType.Empty;
                }
                if (!Operators.IsRelational(op))
                {
                    return StorageType.Empty;
                }
                return left;
            }
            DataTypePrecedence code = (DataTypePrecedence) Math.Max(num, num2);
            StorageType precedenceType = GetPrecedenceType(code);
            precedenceType = GetPrecedenceType((DataTypePrecedence) this.SqlResultType((int) code));
            if ((Operators.IsArithmetical(op) && (precedenceType != StorageType.String)) && ((precedenceType != StorageType.Char) && (precedenceType != StorageType.SqlString)))
            {
                if (!ExpressionNode.IsNumericSql(left))
                {
                    return StorageType.Empty;
                }
                if (!ExpressionNode.IsNumericSql(right))
                {
                    return StorageType.Empty;
                }
            }
            if ((op == 0x12) && ExpressionNode.IsIntegerSql(precedenceType))
            {
                return StorageType.SqlDouble;
            }
            if (((precedenceType == StorageType.SqlMoney) && (left != StorageType.SqlMoney)) && (right != StorageType.SqlMoney))
            {
                precedenceType = StorageType.SqlDecimal;
            }
            if (!this.IsMixedSql(left, right) || !ExpressionNode.IsUnsignedSql(precedenceType))
            {
                return precedenceType;
            }
            if (code >= DataTypePrecedence.UInt64)
            {
                throw ExprException.AmbiguousBinop(op, DataStorage.GetTypeStorage(left), DataStorage.GetTypeStorage(right));
            }
            return GetPrecedenceType(code + 1);
        }

        internal StorageType ResultType(StorageType left, StorageType right, bool lc, bool rc, int op)
        {
            if (((left == StorageType.Guid) && (right == StorageType.Guid)) && Operators.IsRelational(op))
            {
                return left;
            }
            if (((left == StorageType.String) && (right == StorageType.Guid)) && Operators.IsRelational(op))
            {
                return left;
            }
            if (((left == StorageType.Guid) && (right == StorageType.String)) && Operators.IsRelational(op))
            {
                return right;
            }
            int num2 = (int) this.GetPrecedence(left);
            if (num2 == 0)
            {
                return StorageType.Empty;
            }
            int num = (int) this.GetPrecedence(right);
            if (num == 0)
            {
                return StorageType.Empty;
            }
            if (Operators.IsLogical(op))
            {
                if ((left == StorageType.Boolean) && (right == StorageType.Boolean))
                {
                    return StorageType.Boolean;
                }
                return StorageType.Empty;
            }
            if ((left == StorageType.DateTimeOffset) || (right == StorageType.DateTimeOffset))
            {
                if ((Operators.IsRelational(op) && (left == StorageType.DateTimeOffset)) && (right == StorageType.DateTimeOffset))
                {
                    return StorageType.DateTimeOffset;
                }
                return StorageType.Empty;
            }
            if ((op == 15) && ((left == StorageType.String) || (right == StorageType.String)))
            {
                return StorageType.String;
            }
            DataTypePrecedence code = (DataTypePrecedence) Math.Max(num2, num);
            StorageType precedenceType = GetPrecedenceType(code);
            if ((Operators.IsArithmetical(op) && (precedenceType != StorageType.String)) && (precedenceType != StorageType.Char))
            {
                if (!ExpressionNode.IsNumeric(left))
                {
                    return StorageType.Empty;
                }
                if (!ExpressionNode.IsNumeric(right))
                {
                    return StorageType.Empty;
                }
            }
            if ((op == 0x12) && ExpressionNode.IsInteger(precedenceType))
            {
                return StorageType.Double;
            }
            if (!this.IsMixed(left, right))
            {
                return precedenceType;
            }
            if (lc && !rc)
            {
                return right;
            }
            if (!lc && rc)
            {
                return left;
            }
            if (!ExpressionNode.IsUnsigned(precedenceType))
            {
                return precedenceType;
            }
            if (code >= DataTypePrecedence.UInt64)
            {
                throw ExprException.AmbiguousBinop(op, DataStorage.GetTypeStorage(left), DataStorage.GetTypeStorage(right));
            }
            return GetPrecedenceType(code + 1);
        }

        internal void SetTypeMismatchError(int op, Type left, Type right)
        {
            throw ExprException.TypeMismatchInBinop(op, left, right);
        }

        private int SqlResultType(int typeCode)
        {
            switch (typeCode)
            {
                case -8:
                    return -7;

                case -7:
                case -6:
                case -4:
                case -3:
                case -1:
                case 0:
                case 2:
                case 5:
                case 8:
                case 11:
                case 13:
                case 15:
                case 0x11:
                case 0x13:
                    return typeCode;

                case -5:
                    return -4;

                case -2:
                    return -1;

                case 1:
                    return 2;

                case 3:
                case 4:
                    return 5;

                case 6:
                case 7:
                    return 8;

                case 9:
                case 10:
                    return 11;

                case 12:
                    return 13;

                case 14:
                    return 15;

                case 0x10:
                    return 0x11;

                case 0x12:
                    return 0x13;

                case 20:
                    return 0x15;

                case 0x17:
                    return 0x18;
            }
            return typeCode;
        }

        private enum DataTypePrecedence
        {
            Boolean = -2,
            Byte = 3,
            Char = -8,
            DateTime = 0x17,
            DateTimeOffset = 0x18,
            Decimal = 14,
            Double = 0x12,
            Error = 0,
            Int16 = 4,
            Int32 = 7,
            Int64 = 10,
            SByte = 1,
            Single = 0x10,
            SqlBinary = -10,
            SqlBoolean = -1,
            SqlByte = 2,
            SqlBytes = -9,
            SqlChars = -7,
            SqlDateTime = 0x19,
            SqlDecimal = 15,
            SqlDouble = 0x13,
            SqlGuid = -3,
            SqlInt16 = 5,
            SqlInt32 = 8,
            SqlInt64 = 11,
            SqlMoney = 13,
            SqlSingle = 0x11,
            SqlString = -4,
            SqlXml = -6,
            String = -5,
            TimeSpan = 20,
            UInt16 = 6,
            UInt32 = 9,
            UInt64 = 12
        }
    }
}

