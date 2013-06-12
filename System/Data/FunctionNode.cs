namespace System.Data
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.SqlTypes;

    internal sealed class FunctionNode : ExpressionNode
    {
        internal int argumentCount;
        internal ExpressionNode[] arguments;
        private static readonly Function[] funcs = new Function[] { new Function("Abs", FunctionId.Abs, typeof(object), true, false, 1, typeof(object), null, null), new Function("IIf", FunctionId.Iif, typeof(object), false, false, 3, typeof(object), typeof(object), typeof(object)), new Function("In", FunctionId.In, typeof(bool), false, true, 1, null, null, null), new Function("IsNull", FunctionId.IsNull, typeof(object), false, false, 2, typeof(object), typeof(object), null), new Function("Len", FunctionId.Len, typeof(int), true, false, 1, typeof(string), null, null), new Function("Substring", FunctionId.Substring, typeof(string), true, false, 3, typeof(string), typeof(int), typeof(int)), new Function("Trim", FunctionId.Trim, typeof(string), true, false, 1, typeof(string), null, null), new Function("Convert", FunctionId.Convert, typeof(object), false, true, 1, typeof(object), null, null), new Function("DateTimeOffset", FunctionId.DateTimeOffset, typeof(DateTimeOffset), false, true, 3, typeof(DateTime), typeof(int), typeof(int)), new Function("Max", FunctionId.Max, typeof(object), false, false, 1, null, null, null), new Function("Min", FunctionId.Min, typeof(object), false, false, 1, null, null, null), new Function("Sum", FunctionId.Sum, typeof(object), false, false, 1, null, null, null), new Function("Count", FunctionId.Count, typeof(object), false, false, 1, null, null, null), new Function("Var", FunctionId.Var, typeof(object), false, false, 1, null, null, null), new Function("StDev", FunctionId.StDev, typeof(object), false, false, 1, null, null, null), new Function("Avg", FunctionId.Avg, typeof(object), false, false, 1, null, null, null) };
        internal readonly int info;
        internal const int initialCapacity = 1;
        internal readonly string name;

        internal FunctionNode(DataTable table, string name) : base(table)
        {
            this.info = -1;
            this.name = name;
            for (int i = 0; i < funcs.Length; i++)
            {
                if (string.Compare(funcs[i].name, name, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    this.info = i;
                    break;
                }
            }
            if (this.info < 0)
            {
                throw ExprException.UndefinedFunction(this.name);
            }
        }

        internal void AddArgument(ExpressionNode argument)
        {
            if (!funcs[this.info].IsVariantArgumentList && (this.argumentCount >= funcs[this.info].argumentCount))
            {
                throw ExprException.FunctionArgumentCount(this.name);
            }
            if (this.arguments == null)
            {
                this.arguments = new ExpressionNode[1];
            }
            else if (this.argumentCount == this.arguments.Length)
            {
                ExpressionNode[] destinationArray = new ExpressionNode[this.argumentCount * 2];
                Array.Copy(this.arguments, 0, destinationArray, 0, this.argumentCount);
                this.arguments = destinationArray;
            }
            this.arguments[this.argumentCount++] = argument;
        }

        internal override void Bind(DataTable table, List<DataColumn> list)
        {
            base.BindTable(table);
            this.Check();
            if (funcs[this.info].id == FunctionId.Convert)
            {
                if (this.argumentCount != 2)
                {
                    throw ExprException.FunctionArgumentCount(this.name);
                }
                this.arguments[0].Bind(table, list);
                if (this.arguments[1].GetType() == typeof(NameNode))
                {
                    NameNode node = (NameNode) this.arguments[1];
                    this.arguments[1] = new ConstNode(table, System.Data.ValueType.Str, node.name);
                }
                this.arguments[1].Bind(table, list);
            }
            else
            {
                for (int i = 0; i < this.argumentCount; i++)
                {
                    this.arguments[i].Bind(table, list);
                }
            }
        }

        internal void Check()
        {
            Function function1 = funcs[this.info];
            if (this.info < 0)
            {
                throw ExprException.UndefinedFunction(this.name);
            }
            if (funcs[this.info].IsVariantArgumentList)
            {
                if (this.argumentCount < funcs[this.info].argumentCount)
                {
                    if (funcs[this.info].id == FunctionId.In)
                    {
                        throw ExprException.InWithoutList();
                    }
                    throw ExprException.FunctionArgumentCount(this.name);
                }
            }
            else if (this.argumentCount != funcs[this.info].argumentCount)
            {
                throw ExprException.FunctionArgumentCount(this.name);
            }
        }

        internal override bool DependsOn(DataColumn column)
        {
            for (int i = 0; i < this.argumentCount; i++)
            {
                if (this.arguments[i].DependsOn(column))
                {
                    return true;
                }
            }
            return false;
        }

        internal override object Eval()
        {
            return this.Eval(null, DataRowVersion.Default);
        }

        internal override object Eval(int[] recordNos)
        {
            throw ExprException.ComputeNotAggregate(this.ToString());
        }

        internal override object Eval(DataRow row, DataRowVersion version)
        {
            object[] argumentValues = new object[this.argumentCount];
            if (funcs[this.info].id == FunctionId.Convert)
            {
                if (this.argumentCount != 2)
                {
                    throw ExprException.FunctionArgumentCount(this.name);
                }
                argumentValues[0] = this.arguments[0].Eval(row, version);
                argumentValues[1] = this.GetDataType(this.arguments[1]);
            }
            else if (funcs[this.info].id != FunctionId.Iif)
            {
                for (int i = 0; i < this.argumentCount; i++)
                {
                    argumentValues[i] = this.arguments[i].Eval(row, version);
                    if (funcs[this.info].IsValidateArguments)
                    {
                        if ((argumentValues[i] == DBNull.Value) || (typeof(object) == funcs[this.info].parameters[i]))
                        {
                            return DBNull.Value;
                        }
                        if (argumentValues[i].GetType() != funcs[this.info].parameters[i])
                        {
                            if ((funcs[this.info].parameters[i] == typeof(int)) && ExpressionNode.IsInteger(DataStorage.GetStorageType(argumentValues[i].GetType())))
                            {
                                argumentValues[i] = Convert.ToInt32(argumentValues[i], base.FormatProvider);
                            }
                            else
                            {
                                if (((funcs[this.info].id != FunctionId.Trim) && (funcs[this.info].id != FunctionId.Substring)) && (funcs[this.info].id != FunctionId.Len))
                                {
                                    throw ExprException.ArgumentType(funcs[this.info].name, i + 1, funcs[this.info].parameters[i]);
                                }
                                if ((typeof(string) != argumentValues[i].GetType()) && (typeof(SqlString) != argumentValues[i].GetType()))
                                {
                                    throw ExprException.ArgumentType(funcs[this.info].name, i + 1, funcs[this.info].parameters[i]);
                                }
                            }
                        }
                    }
                }
            }
            return this.EvalFunction(funcs[this.info].id, argumentValues, row, version);
        }

        private object EvalFunction(FunctionId id, object[] argumentValues, DataRow row, DataRowVersion version)
        {
            StorageType type;
            switch (id)
            {
                case FunctionId.Charindex:
                    if (!DataStorage.IsObjectNull(argumentValues[0]) && !DataStorage.IsObjectNull(argumentValues[1]))
                    {
                        if (argumentValues[0] is SqlString)
                        {
                            SqlString str6 = (SqlString) argumentValues[0];
                            argumentValues[0] = str6.Value;
                        }
                        if (argumentValues[1] is SqlString)
                        {
                            SqlString str5 = (SqlString) argumentValues[1];
                            argumentValues[1] = str5.Value;
                        }
                        return ((string) argumentValues[1]).IndexOf((string) argumentValues[0], StringComparison.Ordinal);
                    }
                    return DBNull.Value;

                case FunctionId.Len:
                {
                    if (!(argumentValues[0] is SqlString))
                    {
                        goto Label_02D4;
                    }
                    SqlString str4 = (SqlString) argumentValues[0];
                    if (!str4.IsNull)
                    {
                        SqlString str3 = (SqlString) argumentValues[0];
                        argumentValues[0] = str3.Value;
                        goto Label_02D4;
                    }
                    return DBNull.Value;
                }
                case FunctionId.Substring:
                {
                    int startIndex = ((int) argumentValues[1]) - 1;
                    int length = (int) argumentValues[2];
                    if (startIndex < 0)
                    {
                        throw ExprException.FunctionArgumentOutOfRange("index", "Substring");
                    }
                    if (length < 0)
                    {
                        throw ExprException.FunctionArgumentOutOfRange("length", "Substring");
                    }
                    if (length == 0)
                    {
                        return "";
                    }
                    if (argumentValues[0] is SqlString)
                    {
                        SqlString str2 = (SqlString) argumentValues[0];
                        argumentValues[0] = str2.Value;
                    }
                    int num3 = ((string) argumentValues[0]).Length;
                    if (startIndex > num3)
                    {
                        return DBNull.Value;
                    }
                    if ((startIndex + length) > num3)
                    {
                        length = num3 - startIndex;
                    }
                    return ((string) argumentValues[0]).Substring(startIndex, length);
                }
                case FunctionId.IsNull:
                    if (!DataStorage.IsObjectNull(argumentValues[0]))
                    {
                        return argumentValues[0];
                    }
                    return argumentValues[1];

                case FunctionId.Iif:
                    if (!DataExpression.ToBoolean(this.arguments[0].Eval(row, version)))
                    {
                        return this.arguments[2].Eval(row, version);
                    }
                    return this.arguments[1].Eval(row, version);

                case FunctionId.Convert:
                {
                    if (this.argumentCount != 2)
                    {
                        throw ExprException.FunctionArgumentCount(this.name);
                    }
                    if (argumentValues[0] == DBNull.Value)
                    {
                        return DBNull.Value;
                    }
                    Type dataType = (Type) argumentValues[1];
                    StorageType storageType = DataStorage.GetStorageType(dataType);
                    type = DataStorage.GetStorageType(argumentValues[0].GetType());
                    if ((storageType == StorageType.DateTimeOffset) && (type == StorageType.String))
                    {
                        return SqlConvert.ConvertStringToDateTimeOffset((string) argumentValues[0], base.FormatProvider);
                    }
                    if (StorageType.Object == storageType)
                    {
                        return argumentValues[0];
                    }
                    if ((storageType == StorageType.Guid) && (type == StorageType.String))
                    {
                        return new Guid((string) argumentValues[0]);
                    }
                    if (ExpressionNode.IsFloatSql(type) && ExpressionNode.IsIntegerSql(storageType))
                    {
                        if (StorageType.Single == type)
                        {
                            return SqlConvert.ChangeType2((float) SqlConvert.ChangeType2(argumentValues[0], StorageType.Single, typeof(float), base.FormatProvider), storageType, dataType, base.FormatProvider);
                        }
                        if (StorageType.Double == type)
                        {
                            return SqlConvert.ChangeType2((double) SqlConvert.ChangeType2(argumentValues[0], StorageType.Double, typeof(double), base.FormatProvider), storageType, dataType, base.FormatProvider);
                        }
                        if (StorageType.Decimal == type)
                        {
                            return SqlConvert.ChangeType2((decimal) SqlConvert.ChangeType2(argumentValues[0], StorageType.Decimal, typeof(decimal), base.FormatProvider), storageType, dataType, base.FormatProvider);
                        }
                    }
                    return SqlConvert.ChangeType2(argumentValues[0], storageType, dataType, base.FormatProvider);
                }
                case FunctionId.cInt:
                    return Convert.ToInt32(argumentValues[0], base.FormatProvider);

                case FunctionId.cBool:
                {
                    StorageType type4 = DataStorage.GetStorageType(argumentValues[0].GetType());
                    if (type4 > StorageType.Int32)
                    {
                        switch (type4)
                        {
                            case StorageType.Double:
                                return !(((double) argumentValues[0]) == 0.0);

                            case StorageType.String:
                                return bool.Parse((string) argumentValues[0]);
                        }
                        break;
                    }
                    switch (type4)
                    {
                        case StorageType.Boolean:
                            return (bool) argumentValues[0];

                        case StorageType.Int32:
                            return (((int) argumentValues[0]) != 0);
                    }
                    break;
                }
                case FunctionId.cDate:
                    return Convert.ToDateTime(argumentValues[0], base.FormatProvider);

                case FunctionId.cDbl:
                    return Convert.ToDouble(argumentValues[0], base.FormatProvider);

                case FunctionId.cStr:
                    return Convert.ToString(argumentValues[0], base.FormatProvider);

                case FunctionId.Abs:
                    type = DataStorage.GetStorageType(argumentValues[0].GetType());
                    if (!ExpressionNode.IsInteger(type))
                    {
                        if (!ExpressionNode.IsNumeric(type))
                        {
                            throw ExprException.ArgumentTypeInteger(funcs[this.info].name, 1);
                        }
                        return Math.Abs((double) argumentValues[0]);
                    }
                    return Math.Abs((long) argumentValues[0]);

                case FunctionId.In:
                    throw ExprException.NYI(funcs[this.info].name);

                case FunctionId.Trim:
                    if (!DataStorage.IsObjectNull(argumentValues[0]))
                    {
                        if (argumentValues[0] is SqlString)
                        {
                            SqlString str = (SqlString) argumentValues[0];
                            argumentValues[0] = str.Value;
                        }
                        return ((string) argumentValues[0]).Trim();
                    }
                    return DBNull.Value;

                case FunctionId.DateTimeOffset:
                    if (((argumentValues[0] != DBNull.Value) && (argumentValues[1] != DBNull.Value)) && (argumentValues[2] != DBNull.Value))
                    {
                        DateTime time = (DateTime) argumentValues[0];
                        switch (time.Kind)
                        {
                            case DateTimeKind.Utc:
                                if ((((int) argumentValues[1]) != 0) && (((int) argumentValues[2]) != 0))
                                {
                                    throw ExprException.MismatchKindandTimeSpan();
                                }
                                break;

                            case DateTimeKind.Local:
                                if ((DateTimeOffset.Now.Offset.Hours != ((int) argumentValues[1])) && (DateTimeOffset.Now.Offset.Minutes != ((int) argumentValues[2])))
                                {
                                    throw ExprException.MismatchKindandTimeSpan();
                                }
                                break;
                        }
                        if ((((int) argumentValues[1]) < -14) || (((int) argumentValues[1]) > 14))
                        {
                            throw ExprException.InvalidHoursArgument();
                        }
                        if ((((int) argumentValues[2]) < -59) || (((int) argumentValues[2]) > 0x3b))
                        {
                            throw ExprException.InvalidMinutesArgument();
                        }
                        if ((((int) argumentValues[1]) == 14) && (((int) argumentValues[2]) > 0))
                        {
                            throw ExprException.InvalidTimeZoneRange();
                        }
                        if ((((int) argumentValues[1]) == -14) && (((int) argumentValues[2]) < 0))
                        {
                            throw ExprException.InvalidTimeZoneRange();
                        }
                        return new DateTimeOffset((DateTime) argumentValues[0], new TimeSpan((int) argumentValues[1], (int) argumentValues[2], 0));
                    }
                    return DBNull.Value;

                default:
                    throw ExprException.UndefinedFunction(funcs[this.info].name);
            }
            throw ExprException.DatatypeConvertion(argumentValues[0].GetType(), typeof(bool));
        Label_02D4:
            return ((string) argumentValues[0]).Length;
        }

        private Type GetDataType(ExpressionNode node)
        {
            Type type2 = node.GetType();
            string typeName = null;
            if (type2 == typeof(NameNode))
            {
                typeName = ((NameNode) node).name;
            }
            if (type2 == typeof(ConstNode))
            {
                typeName = ((ConstNode) node).val.ToString();
            }
            if (typeName == null)
            {
                throw ExprException.ArgumentType(funcs[this.info].name, 2, typeof(Type));
            }
            Type type = Type.GetType(typeName);
            if (type == null)
            {
                throw ExprException.InvalidType(typeName);
            }
            return type;
        }

        internal override bool HasLocalAggregate()
        {
            for (int i = 0; i < this.argumentCount; i++)
            {
                if (this.arguments[i].HasLocalAggregate())
                {
                    return true;
                }
            }
            return false;
        }

        internal override bool HasRemoteAggregate()
        {
            for (int i = 0; i < this.argumentCount; i++)
            {
                if (this.arguments[i].HasRemoteAggregate())
                {
                    return true;
                }
            }
            return false;
        }

        internal override bool IsConstant()
        {
            bool flag = true;
            for (int i = 0; i < this.argumentCount; i++)
            {
                flag = flag && this.arguments[i].IsConstant();
            }
            return flag;
        }

        internal override bool IsTableConstant()
        {
            for (int i = 0; i < this.argumentCount; i++)
            {
                if (!this.arguments[i].IsTableConstant())
                {
                    return false;
                }
            }
            return true;
        }

        internal override ExpressionNode Optimize()
        {
            for (int i = 0; i < this.argumentCount; i++)
            {
                this.arguments[i] = this.arguments[i].Optimize();
            }
            if (funcs[this.info].id == FunctionId.In)
            {
                if (!this.IsConstant())
                {
                    throw ExprException.NonConstantArgument();
                }
            }
            else if (this.IsConstant())
            {
                return new ConstNode(base.table, System.Data.ValueType.Object, this.Eval(), false);
            }
            return this;
        }

        internal FunctionId Aggregate
        {
            get
            {
                if (this.IsAggregate)
                {
                    return funcs[this.info].id;
                }
                return FunctionId.none;
            }
        }

        internal bool IsAggregate
        {
            get
            {
                return (((((funcs[this.info].id == FunctionId.Sum) || (funcs[this.info].id == FunctionId.Avg)) || ((funcs[this.info].id == FunctionId.Min) || (funcs[this.info].id == FunctionId.Max))) || ((funcs[this.info].id == FunctionId.Count) || (funcs[this.info].id == FunctionId.StDev))) || (funcs[this.info].id == FunctionId.Var));
            }
        }
    }
}

