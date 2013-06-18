namespace System.Data
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    internal sealed class ConstNode : ExpressionNode
    {
        internal readonly object val;

        internal ConstNode(DataTable table, System.Data.ValueType type, object constant) : this(table, type, constant, true)
        {
        }

        internal ConstNode(DataTable table, System.Data.ValueType type, object constant, bool fParseQuotes) : base(table)
        {
            switch (type)
            {
                case System.Data.ValueType.Null:
                    this.val = DBNull.Value;
                    return;

                case System.Data.ValueType.Bool:
                    this.val = Convert.ToBoolean(constant, CultureInfo.InvariantCulture);
                    return;

                case System.Data.ValueType.Numeric:
                    this.val = this.SmallestNumeric(constant);
                    return;

                case System.Data.ValueType.Str:
                    if (!fParseQuotes)
                    {
                        this.val = (string) constant;
                        return;
                    }
                    this.val = ((string) constant).Replace("''", "'");
                    return;

                case System.Data.ValueType.Float:
                    this.val = Convert.ToDouble(constant, NumberFormatInfo.InvariantInfo);
                    return;

                case System.Data.ValueType.Decimal:
                    this.val = this.SmallestDecimal(constant);
                    return;

                case System.Data.ValueType.Date:
                    this.val = DateTime.Parse((string) constant, CultureInfo.InvariantCulture);
                    return;
            }
            this.val = constant;
        }

        internal override void Bind(DataTable table, List<DataColumn> list)
        {
            base.BindTable(table);
        }

        internal override object Eval()
        {
            return this.val;
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

        private object SmallestDecimal(object constant)
        {
            double num;
            decimal num2;
            if (constant == null)
            {
                return 0.0;
            }
            string s = constant as string;
            if (s == null)
            {
                IConvertible convertible = constant as IConvertible;
                if (convertible != null)
                {
                    try
                    {
                        return convertible.ToDecimal(NumberFormatInfo.InvariantInfo);
                    }
                    catch (ArgumentException exception8)
                    {
                        ExceptionBuilder.TraceExceptionWithoutRethrow(exception8);
                    }
                    catch (FormatException exception7)
                    {
                        ExceptionBuilder.TraceExceptionWithoutRethrow(exception7);
                    }
                    catch (InvalidCastException exception6)
                    {
                        ExceptionBuilder.TraceExceptionWithoutRethrow(exception6);
                    }
                    catch (OverflowException exception5)
                    {
                        ExceptionBuilder.TraceExceptionWithoutRethrow(exception5);
                    }
                    try
                    {
                        return convertible.ToDouble(NumberFormatInfo.InvariantInfo);
                    }
                    catch (ArgumentException exception4)
                    {
                        ExceptionBuilder.TraceExceptionWithoutRethrow(exception4);
                    }
                    catch (FormatException exception3)
                    {
                        ExceptionBuilder.TraceExceptionWithoutRethrow(exception3);
                    }
                    catch (InvalidCastException exception2)
                    {
                        ExceptionBuilder.TraceExceptionWithoutRethrow(exception2);
                    }
                    catch (OverflowException exception)
                    {
                        ExceptionBuilder.TraceExceptionWithoutRethrow(exception);
                    }
                }
                return constant;
            }
            if (decimal.TryParse(s, NumberStyles.Number, NumberFormatInfo.InvariantInfo, out num2))
            {
                return num2;
            }
            if (!double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, (IFormatProvider) NumberFormatInfo.InvariantInfo, out num))
            {
                return constant;
            }
            return num;
        }

        private object SmallestNumeric(object constant)
        {
            double num;
            long num2;
            int num3;
            if (constant == null)
            {
                return 0;
            }
            string s = constant as string;
            if (s == null)
            {
                IConvertible convertible = constant as IConvertible;
                if (convertible != null)
                {
                    try
                    {
                        return convertible.ToInt32(NumberFormatInfo.InvariantInfo);
                    }
                    catch (ArgumentException exception12)
                    {
                        ExceptionBuilder.TraceExceptionWithoutRethrow(exception12);
                    }
                    catch (FormatException exception11)
                    {
                        ExceptionBuilder.TraceExceptionWithoutRethrow(exception11);
                    }
                    catch (InvalidCastException exception10)
                    {
                        ExceptionBuilder.TraceExceptionWithoutRethrow(exception10);
                    }
                    catch (OverflowException exception9)
                    {
                        ExceptionBuilder.TraceExceptionWithoutRethrow(exception9);
                    }
                    try
                    {
                        return convertible.ToInt64(NumberFormatInfo.InvariantInfo);
                    }
                    catch (ArgumentException exception8)
                    {
                        ExceptionBuilder.TraceExceptionWithoutRethrow(exception8);
                    }
                    catch (FormatException exception7)
                    {
                        ExceptionBuilder.TraceExceptionWithoutRethrow(exception7);
                    }
                    catch (InvalidCastException exception6)
                    {
                        ExceptionBuilder.TraceExceptionWithoutRethrow(exception6);
                    }
                    catch (OverflowException exception5)
                    {
                        ExceptionBuilder.TraceExceptionWithoutRethrow(exception5);
                    }
                    try
                    {
                        return convertible.ToDouble(NumberFormatInfo.InvariantInfo);
                    }
                    catch (ArgumentException exception4)
                    {
                        ExceptionBuilder.TraceExceptionWithoutRethrow(exception4);
                    }
                    catch (FormatException exception3)
                    {
                        ExceptionBuilder.TraceExceptionWithoutRethrow(exception3);
                    }
                    catch (InvalidCastException exception2)
                    {
                        ExceptionBuilder.TraceExceptionWithoutRethrow(exception2);
                    }
                    catch (OverflowException exception)
                    {
                        ExceptionBuilder.TraceExceptionWithoutRethrow(exception);
                    }
                }
                return constant;
            }
            if (int.TryParse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out num3))
            {
                return num3;
            }
            if (long.TryParse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out num2))
            {
                return num2;
            }
            if (!double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, (IFormatProvider) NumberFormatInfo.InvariantInfo, out num))
            {
                return constant;
            }
            return num;
        }
    }
}

