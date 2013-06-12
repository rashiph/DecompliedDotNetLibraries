namespace System.Data
{
    using System;
    using System.Globalization;

    internal sealed class ExprException
    {
        private ExprException()
        {
        }

        private static EvaluateException _Eval(string error)
        {
            EvaluateException e = new EvaluateException(error);
            ExceptionBuilder.TraceExceptionAsReturnValue(e);
            return e;
        }

        private static EvaluateException _Eval(string error, Exception innerException)
        {
            EvaluateException e = new EvaluateException(error);
            ExceptionBuilder.TraceExceptionAsReturnValue(e);
            return e;
        }

        private static InvalidExpressionException _Expr(string error)
        {
            InvalidExpressionException e = new InvalidExpressionException(error);
            ExceptionBuilder.TraceExceptionAsReturnValue(e);
            return e;
        }

        private static OverflowException _Overflow(string error)
        {
            OverflowException e = new OverflowException(error);
            ExceptionBuilder.TraceExceptionAsReturnValue(e);
            return e;
        }

        private static SyntaxErrorException _Syntax(string error)
        {
            SyntaxErrorException e = new SyntaxErrorException(error);
            ExceptionBuilder.TraceExceptionAsReturnValue(e);
            return e;
        }

        public static Exception AggregateArgument()
        {
            return _Syntax(Res.GetString("Expr_AggregateArgument"));
        }

        public static Exception AggregateUnbound(string expr)
        {
            return _Eval(Res.GetString("Expr_AggregateUnbound", new object[] { expr }));
        }

        public static Exception AmbiguousBinop(int op, Type type1, Type type2)
        {
            return _Eval(Res.GetString("Expr_AmbiguousBinop", new object[] { Operators.ToString(op), type1.ToString(), type2.ToString() }));
        }

        public static Exception ArgumentType(string function, int arg, Type type)
        {
            return _Eval(Res.GetString("Expr_ArgumentType", new object[] { function, arg.ToString(CultureInfo.InvariantCulture), type.ToString() }));
        }

        public static Exception ArgumentTypeInteger(string function, int arg)
        {
            return _Eval(Res.GetString("Expr_ArgumentTypeInteger", new object[] { function, arg.ToString(CultureInfo.InvariantCulture) }));
        }

        internal static EvaluateException BindFailure(string relationName)
        {
            return _Eval(Res.GetString("Expr_BindFailure", new object[] { relationName }));
        }

        public static Exception ComputeNotAggregate(string expr)
        {
            return _Eval(Res.GetString("Expr_ComputeNotAggregate", new object[] { expr }));
        }

        public static Exception DatatypeConvertion(Type type1, Type type2)
        {
            return _Eval(Res.GetString("Expr_DatatypeConvertion", new object[] { type1.ToString(), type2.ToString() }));
        }

        public static Exception DatavalueConvertion(object value, Type type, Exception innerException)
        {
            return _Eval(Res.GetString("Expr_DatavalueConvertion", new object[] { value.ToString(), type.ToString() }), innerException);
        }

        public static Exception EvalNoContext()
        {
            return _Eval(Res.GetString("Expr_EvalNoContext"));
        }

        public static Exception ExpressionTooComplex()
        {
            return _Eval(Res.GetString("Expr_ExpressionTooComplex"));
        }

        public static Exception ExpressionUnbound(string expr)
        {
            return _Eval(Res.GetString("Expr_ExpressionUnbound", new object[] { expr }));
        }

        public static Exception FilterConvertion(string expr)
        {
            return _Eval(Res.GetString("Expr_FilterConvertion", new object[] { expr }));
        }

        public static Exception FunctionArgumentCount(string name)
        {
            return _Eval(Res.GetString("Expr_FunctionArgumentCount", new object[] { name }));
        }

        public static Exception FunctionArgumentOutOfRange(string arg, string func)
        {
            return ExceptionBuilder._ArgumentOutOfRange(arg, Res.GetString("Expr_ArgumentOutofRange", new object[] { func }));
        }

        public static Exception InvalidDate(string date)
        {
            return _Syntax(Res.GetString("Expr_InvalidDate", new object[] { date }));
        }

        public static Exception InvalidHoursArgument()
        {
            return _Eval(Res.GetString("Expr_InvalidHoursArgument"));
        }

        public static Exception InvalidIsSyntax()
        {
            return _Syntax(Res.GetString("Expr_IsSyntax"));
        }

        public static Exception InvalidMinutesArgument()
        {
            return _Eval(Res.GetString("Expr_InvalidMinutesArgument"));
        }

        public static Exception InvalidName(string name)
        {
            return _Syntax(Res.GetString("Expr_InvalidName", new object[] { name }));
        }

        public static Exception InvalidNameBracketing(string name)
        {
            return _Syntax(Res.GetString("Expr_InvalidNameBracketing", new object[] { name }));
        }

        public static Exception InvalidPattern(string pat)
        {
            return _Eval(Res.GetString("Expr_InvalidPattern", new object[] { pat }));
        }

        public static Exception InvalidString(string str)
        {
            return _Syntax(Res.GetString("Expr_InvalidString", new object[] { str }));
        }

        public static Exception InvalidTimeZoneRange()
        {
            return _Eval(Res.GetString("Expr_InvalidTimeZoneRange"));
        }

        public static Exception InvalidType(string typeName)
        {
            return _Eval(Res.GetString("Expr_InvalidType", new object[] { typeName }));
        }

        public static Exception InvokeArgument()
        {
            return ExceptionBuilder._Argument(Res.GetString("Expr_InvokeArgument"));
        }

        public static Exception InWithoutList()
        {
            return _Syntax(Res.GetString("Expr_InWithoutList"));
        }

        public static Exception InWithoutParentheses()
        {
            return _Syntax(Res.GetString("Expr_InWithoutParentheses"));
        }

        public static Exception LookupArgument()
        {
            return _Syntax(Res.GetString("Expr_LookupArgument"));
        }

        public static Exception MismatchKindandTimeSpan()
        {
            return _Eval(Res.GetString("Expr_MismatchKindandTimeSpan"));
        }

        public static Exception MissingOperand(OperatorInfo before)
        {
            return _Syntax(Res.GetString("Expr_MissingOperand", new object[] { Operators.ToString(before.op) }));
        }

        public static Exception MissingOperandBefore(string op)
        {
            return _Syntax(Res.GetString("Expr_MissingOperandBefore", new object[] { op }));
        }

        public static Exception MissingOperator(string token)
        {
            return _Syntax(Res.GetString("Expr_MissingOperand", new object[] { token }));
        }

        public static Exception MissingRightParen()
        {
            return _Syntax(Res.GetString("Expr_MissingRightParen"));
        }

        public static Exception NonConstantArgument()
        {
            return _Eval(Res.GetString("Expr_NonConstantArgument"));
        }

        public static Exception NYI(string moreinfo)
        {
            return _Expr(Res.GetString("Expr_NYI", new object[] { moreinfo }));
        }

        public static Exception Overflow(Type type)
        {
            return _Overflow(Res.GetString("Expr_Overflow", new object[] { type.Name }));
        }

        public static Exception SyntaxError()
        {
            return _Syntax(Res.GetString("Expr_Syntax"));
        }

        public static Exception TooManyRightParentheses()
        {
            return _Syntax(Res.GetString("Expr_TooManyRightParentheses"));
        }

        public static Exception TypeMismatch(string expr)
        {
            return _Eval(Res.GetString("Expr_TypeMismatch", new object[] { expr }));
        }

        public static Exception TypeMismatchInBinop(int op, Type type1, Type type2)
        {
            return _Eval(Res.GetString("Expr_TypeMismatchInBinop", new object[] { Operators.ToString(op), type1.ToString(), type2.ToString() }));
        }

        public static Exception UnboundName(string name)
        {
            return _Eval(Res.GetString("Expr_UnboundName", new object[] { name }));
        }

        public static Exception UndefinedFunction(string name)
        {
            return _Eval(Res.GetString("Expr_UndefinedFunction", new object[] { name }));
        }

        public static Exception UnknownToken(string token, int position)
        {
            return _Syntax(Res.GetString("Expr_UnknownToken", new object[] { token, position.ToString(CultureInfo.InvariantCulture) }));
        }

        public static Exception UnknownToken(Tokens tokExpected, Tokens tokCurr, int position)
        {
            return _Syntax(Res.GetString("Expr_UnknownToken1", new object[] { tokExpected.ToString(), tokCurr.ToString(), position.ToString(CultureInfo.InvariantCulture) }));
        }

        public static Exception UnresolvedRelation(string name, string expr)
        {
            return _Eval(Res.GetString("Expr_UnresolvedRelation", new object[] { name, expr }));
        }

        public static Exception UnsupportedDataType(Type type)
        {
            return ExceptionBuilder._Argument(Res.GetString("Expr_UnsupportedType", new object[] { type.FullName }));
        }

        public static Exception UnsupportedOperator(int op)
        {
            return _Eval(Res.GetString("Expr_UnsupportedOperator", new object[] { Operators.ToString(op) }));
        }
    }
}

