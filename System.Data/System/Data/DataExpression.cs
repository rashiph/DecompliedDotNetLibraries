namespace System.Data
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.SqlTypes;

    internal sealed class DataExpression : IFilter
    {
        private readonly Type _dataType;
        private readonly StorageType _storageType;
        private bool bound;
        private DataColumn[] dependency;
        private System.Data.ExpressionNode expr;
        internal string originalExpression;
        private bool parsed;
        private DataTable table;

        internal DataExpression(DataTable table, string expression) : this(table, expression, null)
        {
        }

        internal DataExpression(DataTable table, string expression, Type type)
        {
            this.dependency = DataTable.zeroColumns;
            ExpressionParser parser = new ExpressionParser(table);
            parser.LoadExpression(expression);
            this.originalExpression = expression;
            this.expr = null;
            if (expression != null)
            {
                this._storageType = DataStorage.GetStorageType(type);
                if (this._storageType == StorageType.BigInteger)
                {
                    throw ExprException.UnsupportedDataType(type);
                }
                this._dataType = type;
                this.expr = parser.Parse();
                this.parsed = true;
                if ((this.expr != null) && (table != null))
                {
                    this.Bind(table);
                }
                else
                {
                    this.bound = false;
                }
            }
        }

        internal void Bind(DataTable table)
        {
            this.table = table;
            if ((table != null) && (this.expr != null))
            {
                List<DataColumn> list = new List<DataColumn>();
                this.expr.Bind(table, list);
                this.expr = this.expr.Optimize();
                this.table = table;
                this.bound = true;
                this.dependency = list.ToArray();
            }
        }

        internal bool DependsOn(DataColumn column)
        {
            return ((this.expr != null) && this.expr.DependsOn(column));
        }

        internal object Evaluate()
        {
            return this.Evaluate((DataRow) null, DataRowVersion.Default);
        }

        internal object Evaluate(DataRow[] rows)
        {
            return this.Evaluate(rows, DataRowVersion.Default);
        }

        internal object Evaluate(DataRow row, DataRowVersion version)
        {
            if (!this.bound)
            {
                this.Bind(this.table);
            }
            if (this.expr != null)
            {
                object obj2 = this.expr.Eval(row, version);
                if ((obj2 == DBNull.Value) && (StorageType.Uri >= this._storageType))
                {
                    return obj2;
                }
                try
                {
                    if (StorageType.Object != this._storageType)
                    {
                        obj2 = SqlConvert.ChangeType2(obj2, this._storageType, this._dataType, this.table.FormatProvider);
                    }
                    return obj2;
                }
                catch (Exception exception)
                {
                    if (!ADP.IsCatchableExceptionType(exception))
                    {
                        throw;
                    }
                    ExceptionBuilder.TraceExceptionForCapture(exception);
                    throw ExprException.DatavalueConvertion(obj2, this._dataType, exception);
                }
            }
            return null;
        }

        internal object Evaluate(DataRow[] rows, DataRowVersion version)
        {
            if (!this.bound)
            {
                this.Bind(this.table);
            }
            if (this.expr == null)
            {
                return DBNull.Value;
            }
            List<int> list = new List<int>();
            foreach (DataRow row in rows)
            {
                if ((row.RowState != DataRowState.Deleted) && ((version != DataRowVersion.Original) || (row.oldRecord != -1)))
                {
                    list.Add(row.GetRecordFromVersion(version));
                }
            }
            int[] recordNos = list.ToArray();
            return this.expr.Eval(recordNos);
        }

        internal DataColumn[] GetDependency()
        {
            return this.dependency;
        }

        internal bool HasLocalAggregate()
        {
            return ((this.expr != null) && this.expr.HasLocalAggregate());
        }

        internal bool HasRemoteAggregate()
        {
            return ((this.expr != null) && this.expr.HasRemoteAggregate());
        }

        public bool Invoke(DataRow row, DataRowVersion version)
        {
            bool flag;
            if (this.expr == null)
            {
                return true;
            }
            if (row == null)
            {
                throw ExprException.InvokeArgument();
            }
            object obj2 = this.expr.Eval(row, version);
            try
            {
                flag = ToBoolean(obj2);
            }
            catch (EvaluateException)
            {
                throw ExprException.FilterConvertion(this.Expression);
            }
            return flag;
        }

        internal bool IsTableAggregate()
        {
            return ((this.expr != null) && this.expr.IsTableConstant());
        }

        internal static bool IsUnknown(object value)
        {
            return DataStorage.IsObjectNull(value);
        }

        internal static bool ToBoolean(object value)
        {
            if (IsUnknown(value))
            {
                return false;
            }
            if (value is bool)
            {
                return (bool) value;
            }
            if (value is SqlBoolean)
            {
                SqlBoolean flag2 = (SqlBoolean) value;
                return flag2.IsTrue;
            }
            if (value is string)
            {
                try
                {
                    return bool.Parse((string) value);
                }
                catch (Exception exception)
                {
                    if (!ADP.IsCatchableExceptionType(exception))
                    {
                        throw;
                    }
                    ExceptionBuilder.TraceExceptionForCapture(exception);
                    throw ExprException.DatavalueConvertion(value, typeof(bool), exception);
                }
            }
            throw ExprException.DatavalueConvertion(value, typeof(bool), null);
        }

        internal string Expression
        {
            get
            {
                if (this.originalExpression == null)
                {
                    return "";
                }
                return this.originalExpression;
            }
        }

        internal System.Data.ExpressionNode ExpressionNode
        {
            get
            {
                return this.expr;
            }
        }

        internal bool HasValue
        {
            get
            {
                return (null != this.expr);
            }
        }
    }
}

