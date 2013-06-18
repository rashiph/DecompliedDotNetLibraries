namespace System.Data
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Globalization;

    internal abstract class ExpressionNode
    {
        private DataTable _table;

        protected ExpressionNode(DataTable table)
        {
            this._table = table;
        }

        internal abstract void Bind(DataTable table, List<DataColumn> list);
        protected void BindTable(DataTable table)
        {
            this._table = table;
        }

        internal virtual bool DependsOn(DataColumn column)
        {
            return false;
        }

        internal abstract object Eval();
        internal abstract object Eval(int[] recordNos);
        internal abstract object Eval(DataRow row, DataRowVersion version);
        internal abstract bool HasLocalAggregate();
        internal abstract bool HasRemoteAggregate();
        internal abstract bool IsConstant();
        internal static bool IsFloat(StorageType type)
        {
            if ((type != StorageType.Single) && (type != StorageType.Double))
            {
                return (type == StorageType.Decimal);
            }
            return true;
        }

        internal static bool IsFloatSql(StorageType type)
        {
            if ((((type != StorageType.Single) && (type != StorageType.Double)) && ((type != StorageType.Decimal) && (type != StorageType.SqlDouble))) && ((type != StorageType.SqlDecimal) && (type != StorageType.SqlMoney)))
            {
                return (type == StorageType.SqlSingle);
            }
            return true;
        }

        internal static bool IsInteger(StorageType type)
        {
            if ((((type != StorageType.Int16) && (type != StorageType.Int32)) && ((type != StorageType.Int64) && (type != StorageType.UInt16))) && (((type != StorageType.UInt32) && (type != StorageType.UInt64)) && (type != StorageType.SByte)))
            {
                return (type == StorageType.Byte);
            }
            return true;
        }

        internal static bool IsIntegerSql(StorageType type)
        {
            if (((((type != StorageType.Int16) && (type != StorageType.Int32)) && ((type != StorageType.Int64) && (type != StorageType.UInt16))) && (((type != StorageType.UInt32) && (type != StorageType.UInt64)) && ((type != StorageType.SByte) && (type != StorageType.Byte)))) && (((type != StorageType.SqlInt64) && (type != StorageType.SqlInt32)) && (type != StorageType.SqlInt16)))
            {
                return (type == StorageType.SqlByte);
            }
            return true;
        }

        internal static bool IsNumeric(StorageType type)
        {
            if (!IsFloat(type))
            {
                return IsInteger(type);
            }
            return true;
        }

        internal static bool IsNumericSql(StorageType type)
        {
            if (!IsFloatSql(type))
            {
                return IsIntegerSql(type);
            }
            return true;
        }

        internal static bool IsSigned(StorageType type)
        {
            if (((type != StorageType.Int16) && (type != StorageType.Int32)) && ((type != StorageType.Int64) && (type != StorageType.SByte)))
            {
                return IsFloat(type);
            }
            return true;
        }

        internal static bool IsSignedSql(StorageType type)
        {
            if ((((type != StorageType.Int16) && (type != StorageType.Int32)) && ((type != StorageType.Int64) && (type != StorageType.SByte))) && (((type != StorageType.SqlInt64) && (type != StorageType.SqlInt32)) && (type != StorageType.SqlInt16)))
            {
                return IsFloatSql(type);
            }
            return true;
        }

        internal abstract bool IsTableConstant();
        internal static bool IsUnsigned(StorageType type)
        {
            if (((type != StorageType.UInt16) && (type != StorageType.UInt32)) && (type != StorageType.UInt64))
            {
                return (type == StorageType.Byte);
            }
            return true;
        }

        internal static bool IsUnsignedSql(StorageType type)
        {
            if (((type != StorageType.UInt16) && (type != StorageType.UInt32)) && ((type != StorageType.UInt64) && (type != StorageType.SqlByte)))
            {
                return (type == StorageType.Byte);
            }
            return true;
        }

        internal abstract ExpressionNode Optimize();

        internal IFormatProvider FormatProvider
        {
            get
            {
                if (this._table == null)
                {
                    return CultureInfo.CurrentCulture;
                }
                return this._table.FormatProvider;
            }
        }

        internal virtual bool IsSqlColumn
        {
            get
            {
                return false;
            }
        }

        protected DataTable table
        {
            get
            {
                return this._table;
            }
        }
    }
}

