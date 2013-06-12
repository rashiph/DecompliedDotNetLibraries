namespace System.Linq.Expressions
{
    using System;
    using System.Diagnostics;

    [DebuggerTypeProxy(typeof(Expression.ParameterExpressionProxy))]
    public class ParameterExpression : Expression
    {
        private readonly string _name;

        internal ParameterExpression(string name)
        {
            this._name = name;
        }

        protected internal override Expression Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitParameter(this);
        }

        internal virtual bool GetIsByRef()
        {
            return false;
        }

        internal static ParameterExpression Make(System.Type type, string name, bool isByRef)
        {
            if (isByRef)
            {
                return new ByRefParameterExpression(type, name);
            }
            if (!type.IsEnum)
            {
                switch (System.Type.GetTypeCode(type))
                {
                    case TypeCode.Object:
                        if (!(type == typeof(object)))
                        {
                            if (type == typeof(Exception))
                            {
                                return new PrimitiveParameterExpression<Exception>(name);
                            }
                            if (type == typeof(object[]))
                            {
                                return new PrimitiveParameterExpression<object[]>(name);
                            }
                            break;
                        }
                        return new ParameterExpression(name);

                    case TypeCode.DBNull:
                        return new PrimitiveParameterExpression<DBNull>(name);

                    case TypeCode.Boolean:
                        return new PrimitiveParameterExpression<bool>(name);

                    case TypeCode.Char:
                        return new PrimitiveParameterExpression<char>(name);

                    case TypeCode.SByte:
                        return new PrimitiveParameterExpression<sbyte>(name);

                    case TypeCode.Byte:
                        return new PrimitiveParameterExpression<byte>(name);

                    case TypeCode.Int16:
                        return new PrimitiveParameterExpression<short>(name);

                    case TypeCode.UInt16:
                        return new PrimitiveParameterExpression<ushort>(name);

                    case TypeCode.Int32:
                        return new PrimitiveParameterExpression<int>(name);

                    case TypeCode.UInt32:
                        return new PrimitiveParameterExpression<uint>(name);

                    case TypeCode.Int64:
                        return new PrimitiveParameterExpression<long>(name);

                    case TypeCode.UInt64:
                        return new PrimitiveParameterExpression<ulong>(name);

                    case TypeCode.Single:
                        return new PrimitiveParameterExpression<float>(name);

                    case TypeCode.Double:
                        return new PrimitiveParameterExpression<double>(name);

                    case TypeCode.Decimal:
                        return new PrimitiveParameterExpression<decimal>(name);

                    case TypeCode.DateTime:
                        return new PrimitiveParameterExpression<DateTime>(name);

                    case TypeCode.String:
                        return new PrimitiveParameterExpression<string>(name);
                }
            }
            return new TypedParameterExpression(type, name);
        }

        public bool IsByRef
        {
            get
            {
                return this.GetIsByRef();
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
        }

        public sealed override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.Parameter;
            }
        }

        public override System.Type Type
        {
            get
            {
                return typeof(object);
            }
        }
    }
}

