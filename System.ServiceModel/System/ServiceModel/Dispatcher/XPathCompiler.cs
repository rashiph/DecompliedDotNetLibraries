namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.Xml.XPath;

    internal class XPathCompiler
    {
        private QueryCompilerFlags flags;
        private int nestingLevel;
        private bool pushInitialContext;

        internal XPathCompiler(QueryCompilerFlags flags)
        {
            this.flags = flags;
            this.pushInitialContext = false;
        }

        internal virtual OpcodeBlock Compile(XPathExpr expr)
        {
            this.nestingLevel = 1;
            this.pushInitialContext = false;
            OpcodeBlock block = new XPathExprCompiler(this).Compile(expr);
            if (this.pushInitialContext)
            {
                OpcodeBlock block2 = new OpcodeBlock();
                block2.Append(new PushContextNodeOpcode());
                block2.Append(block);
                block2.Append(new PopContextNodes());
                return block2;
            }
            return block;
        }

        private void SetPushInitialContext(bool pushInitial)
        {
            if (pushInitial)
            {
                this.pushInitialContext = pushInitial;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct XPathExprCompiler
        {
            private OpcodeBlock codeBlock;
            private XPathCompiler compiler;
            internal XPathExprCompiler(XPathCompiler compiler)
            {
                this.compiler = compiler;
                this.codeBlock = new OpcodeBlock();
            }

            private XPathExprCompiler(XPathCompiler.XPathExprCompiler xpathCompiler)
            {
                this.compiler = xpathCompiler.compiler;
                this.codeBlock = new OpcodeBlock();
            }

            internal OpcodeBlock Compile(XPathExpr expr)
            {
                this.codeBlock = new OpcodeBlock();
                this.CompileExpression(expr);
                return this.codeBlock;
            }

            private OpcodeBlock CompileBlock(XPathExpr expr)
            {
                XPathCompiler.XPathExprCompiler compiler = new XPathCompiler.XPathExprCompiler(this);
                return compiler.Compile(expr);
            }

            private void CompileBoolean(XPathExpr expr, bool testValue)
            {
                if (this.compiler.nestingLevel == 1)
                {
                    this.CompileBasicBoolean(expr, testValue);
                }
                else
                {
                    OpcodeBlock block = new OpcodeBlock();
                    Opcode jump = new BlockEndOpcode();
                    block.Append(new PushBooleanOpcode(testValue));
                    XPathExprList subExpr = expr.SubExpr;
                    XPathExpr expr2 = subExpr[0];
                    block.Append(this.CompileBlock(expr2));
                    if (expr2.ReturnType != ValueDataType.Boolean)
                    {
                        block.Append(new TypecastOpcode(ValueDataType.Boolean));
                    }
                    block.Append(new ApplyBooleanOpcode(jump, testValue));
                    for (int i = 1; i < subExpr.Count; i++)
                    {
                        expr2 = subExpr[i];
                        block.Append(new StartBooleanOpcode(testValue));
                        block.Append(this.CompileBlock(expr2));
                        if (expr2.ReturnType != ValueDataType.Boolean)
                        {
                            block.Append(new TypecastOpcode(ValueDataType.Boolean));
                        }
                        block.Append(new EndBooleanOpcode(jump, testValue));
                    }
                    block.Append(jump);
                    this.codeBlock.Append(block);
                }
            }

            private void CompileBasicBoolean(XPathExpr expr, bool testValue)
            {
                OpcodeBlock block = new OpcodeBlock();
                Opcode jump = new BlockEndOpcode();
                XPathExprList subExpr = expr.SubExpr;
                for (int i = 0; i < subExpr.Count; i++)
                {
                    XPathExpr expr2 = subExpr[i];
                    block.Append(this.CompileBlock(expr2));
                    if (expr2.ReturnType != ValueDataType.Boolean)
                    {
                        block.Append(new TypecastOpcode(ValueDataType.Boolean));
                    }
                    if (i < (subExpr.Count - 1))
                    {
                        block.Append(new JumpIfOpcode(jump, testValue));
                    }
                }
                block.Append(jump);
                this.codeBlock.Append(block);
            }

            private void CompileExpression(XPathExpr expr)
            {
                switch (expr.Type)
                {
                    case XPathExprType.Or:
                        this.CompileBoolean(expr, false);
                        break;

                    case XPathExprType.And:
                        this.CompileBoolean(expr, true);
                        break;

                    case XPathExprType.Relational:
                        this.CompileRelational((XPathRelationExpr) expr);
                        break;

                    case XPathExprType.Union:
                    {
                        XPathConjunctExpr expr2 = (XPathConjunctExpr) expr;
                        this.CompileExpression(expr2.Left);
                        this.CompileExpression(expr2.Right);
                        this.codeBlock.Append(new UnionOpcode());
                        break;
                    }
                    case XPathExprType.LocationPath:
                        if (expr.SubExprCount > 0)
                        {
                            this.CompileLocationPath(expr);
                            this.codeBlock.Append(new PopSequenceToValueStackOpcode());
                        }
                        break;

                    case XPathExprType.RelativePath:
                        this.CompileRelativePath(expr, true);
                        break;

                    case XPathExprType.XsltVariable:
                        this.CompileXsltVariable((XPathXsltVariableExpr) expr);
                        break;

                    case XPathExprType.String:
                        this.codeBlock.Append(new PushStringOpcode(((XPathStringExpr) expr).String));
                        break;

                    case XPathExprType.Number:
                    {
                        XPathNumberExpr expr3 = (XPathNumberExpr) expr;
                        double number = expr3.Number;
                        if (expr3.Negate)
                        {
                            expr3.Negate = false;
                            number = -number;
                        }
                        this.codeBlock.Append(new PushNumberOpcode(number));
                        break;
                    }
                    case XPathExprType.Function:
                        this.CompileFunction((XPathFunctionExpr) expr);
                        break;

                    case XPathExprType.XsltFunction:
                        this.CompileXsltFunction((XPathXsltFunctionExpr) expr);
                        break;

                    case XPathExprType.Math:
                        this.CompileMath((XPathMathExpr) expr);
                        break;

                    case XPathExprType.Filter:
                        this.CompileFilter(expr);
                        if (expr.ReturnType == ValueDataType.Sequence)
                        {
                            this.codeBlock.Append(new PopSequenceToValueStackOpcode());
                        }
                        break;

                    case XPathExprType.Path:
                        this.CompilePath(expr);
                        if ((expr.SubExprCount == 0) && (expr.ReturnType == ValueDataType.Sequence))
                        {
                            this.codeBlock.Append(new PopSequenceToValueStackOpcode());
                        }
                        break;

                    default:
                        this.ThrowError(QueryCompileError.UnsupportedExpression);
                        break;
                }
                this.NegateIfRequired(expr);
            }

            private void CompileFilter(XPathExpr expr)
            {
                XPathExprList subExpr = expr.SubExpr;
                XPathExpr expr2 = subExpr[0];
                if ((subExpr.Count > 1) && (ValueDataType.Sequence != expr2.ReturnType))
                {
                    this.ThrowError(QueryCompileError.InvalidExpression);
                }
                this.CompileExpression(expr2);
                if (expr2.ReturnType == ValueDataType.Sequence)
                {
                    if (!this.IsSpecialInternalFunction(expr2) && (expr.SubExprCount > 1))
                    {
                        this.codeBlock.Append(new MergeOpcode());
                        this.codeBlock.Append(new PopSequenceToSequenceStackOpcode());
                    }
                    else if (this.IsSpecialInternalFunction(expr2) && (expr.SubExprCount > 1))
                    {
                        this.codeBlock.DetachLast();
                    }
                    this.compiler.nestingLevel++;
                    if (this.compiler.nestingLevel > 3)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryCompileException(QueryCompileError.PredicateNestingTooDeep));
                    }
                    for (int i = 1; i < expr.SubExprCount; i++)
                    {
                        this.CompilePredicate(subExpr[i]);
                    }
                    this.compiler.nestingLevel--;
                }
            }

            private bool IsSpecialInternalFunction(XPathExpr expr)
            {
                if (expr.Type != XPathExprType.XsltFunction)
                {
                    return false;
                }
                XPathMessageFunction function = ((XPathXsltFunctionExpr) expr).Function as XPathMessageFunction;
                if (function == null)
                {
                    return false;
                }
                return ((function.ReturnType == XPathResultType.NodeSet) && (function.Maxargs == 0));
            }

            private void CompileFunction(XPathFunctionExpr expr)
            {
                if (!this.CompileFunctionSpecial(expr))
                {
                    QueryFunction function = expr.Function;
                    if (expr.SubExprCount > 0)
                    {
                        for (int i = expr.SubExpr.Count - 1; i >= 0; i--)
                        {
                            this.CompileFunctionParam(function, expr.SubExpr, i);
                        }
                    }
                    this.codeBlock.Append(new FunctionCallOpcode(function));
                    if ((1 == this.compiler.nestingLevel) && function.TestFlag(QueryFunctionFlag.UsesContextNode))
                    {
                        this.compiler.SetPushInitialContext(true);
                    }
                }
            }

            private void CompileFunctionParam(QueryFunction function, XPathExprList paramList, int index)
            {
                XPathExpr expr = paramList[index];
                this.CompileExpression(expr);
                if ((function.ParamTypes[index] != ValueDataType.None) && (expr.ReturnType != function.ParamTypes[index]))
                {
                    if (function.ParamTypes[index] == ValueDataType.Sequence)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryCompileException(QueryCompileError.InvalidTypeConversion));
                    }
                    this.CompileTypecast(function.ParamTypes[index]);
                }
            }

            private bool CompileFunctionSpecial(XPathFunctionExpr expr)
            {
                XPathFunction function = expr.Function as XPathFunction;
                if (((function != null) && (XPathFunctionID.StartsWith == function.ID)) && (XPathExprType.String == expr.SubExpr[1].Type))
                {
                    this.CompileFunctionParam(function, expr.SubExpr, 0);
                    this.codeBlock.Append(new StringPrefixOpcode(((XPathStringExpr) expr.SubExpr[1]).String));
                    return true;
                }
                return false;
            }

            private void CompileLiteralRelation(XPathRelationExpr expr)
            {
                XPathLiteralExpr left = (XPathLiteralExpr) expr.Left;
                XPathLiteralExpr right = (XPathLiteralExpr) expr.Right;
                bool literal = QueryValueModel.CompileTimeCompare(left.Literal, right.Literal, expr.Op);
                this.codeBlock.Append(new PushBooleanOpcode(literal));
            }

            private void CompileLiteralOrdinal(XPathExpr expr)
            {
                int ordinal = 0;
                try
                {
                    XPathNumberExpr expr2 = (XPathNumberExpr) expr;
                    ordinal = Convert.ToInt32(expr2.Number);
                    if (expr2.Negate)
                    {
                        ordinal = -ordinal;
                        expr2.Negate = false;
                    }
                    if (ordinal < 1)
                    {
                        this.ThrowError(QueryCompileError.InvalidOrdinal);
                    }
                }
                catch (OverflowException)
                {
                    this.ThrowError(QueryCompileError.InvalidOrdinal);
                }
                if ((this.compiler.flags & QueryCompilerFlags.InverseQuery) != QueryCompilerFlags.None)
                {
                    this.codeBlock.Append(new PushContextPositionOpcode());
                    this.codeBlock.Append(new NumberEqualsOpcode((double) ordinal));
                }
                else
                {
                    this.codeBlock.Append(new LiteralOrdinalOpcode(ordinal));
                }
            }

            private void CompileLocationPath(XPathExpr expr)
            {
                XPathStepExpr expr2 = (XPathStepExpr) expr.SubExpr[0];
                this.CompileSteps(expr.SubExpr);
                if (1 == this.compiler.nestingLevel)
                {
                    this.compiler.SetPushInitialContext(expr2.SelectDesc.Type != QueryNodeType.Root);
                }
            }

            private void CompileMath(XPathMathExpr mathExpr)
            {
                if ((XPathExprType.Number != mathExpr.Right.Type) || (XPathExprType.Number != mathExpr.Left.Type))
                {
                    this.CompileExpression(mathExpr.Right);
                    if (ValueDataType.Double != mathExpr.Right.ReturnType)
                    {
                        this.CompileTypecast(ValueDataType.Double);
                    }
                    this.CompileExpression(mathExpr.Left);
                    if (ValueDataType.Double != mathExpr.Left.ReturnType)
                    {
                        this.CompileTypecast(ValueDataType.Double);
                    }
                    this.codeBlock.Append(this.CreateMathOpcode(mathExpr.Op));
                }
                else
                {
                    double number = ((XPathNumberExpr) mathExpr.Left).Number;
                    if (((XPathNumberExpr) mathExpr.Left).Negate)
                    {
                        ((XPathNumberExpr) mathExpr.Left).Negate = false;
                        number = -number;
                    }
                    double num2 = ((XPathNumberExpr) mathExpr.Right).Number;
                    if (((XPathNumberExpr) mathExpr.Right).Negate)
                    {
                        ((XPathNumberExpr) mathExpr.Right).Negate = false;
                        num2 = -num2;
                    }
                    switch (mathExpr.Op)
                    {
                        case MathOperator.Plus:
                            number += num2;
                            break;

                        case MathOperator.Minus:
                            number -= num2;
                            break;

                        case MathOperator.Div:
                            number /= num2;
                            break;

                        case MathOperator.Multiply:
                            number *= num2;
                            break;

                        case MathOperator.Mod:
                            number = number % num2;
                            break;
                    }
                    this.codeBlock.Append(new PushNumberOpcode(number));
                }
            }

            private void CompileNumberLiteralEquality(XPathRelationExpr expr)
            {
                bool flag = XPathExprType.Number == expr.Left.Type;
                XPathExprType type = expr.Right.Type;
                this.CompileExpression(flag ? expr.Right : expr.Left);
                XPathNumberExpr expr2 = flag ? ((XPathNumberExpr) expr.Left) : ((XPathNumberExpr) expr.Right);
                double number = expr2.Number;
                if (expr2.Negate)
                {
                    expr2.Negate = false;
                    number = -number;
                }
                this.codeBlock.Append(new NumberEqualsOpcode(number));
            }

            private void CompileNumberRelation(XPathRelationExpr expr)
            {
                if (expr.Op == RelationOperator.Eq)
                {
                    this.CompileNumberLiteralEquality(expr);
                }
                else
                {
                    bool flag = XPathExprType.Number == expr.Left.Type;
                    XPathExprType type = expr.Right.Type;
                    this.CompileExpression(flag ? expr.Right : expr.Left);
                    XPathNumberExpr expr2 = flag ? ((XPathNumberExpr) expr.Left) : ((XPathNumberExpr) expr.Right);
                    double number = expr2.Number;
                    if (expr2.Negate)
                    {
                        expr2.Negate = false;
                        number = -number;
                    }
                    if (flag)
                    {
                        switch (expr.Op)
                        {
                            case RelationOperator.Gt:
                                expr.Op = RelationOperator.Lt;
                                break;

                            case RelationOperator.Ge:
                                expr.Op = RelationOperator.Le;
                                break;

                            case RelationOperator.Lt:
                                expr.Op = RelationOperator.Gt;
                                break;

                            case RelationOperator.Le:
                                expr.Op = RelationOperator.Ge;
                                break;
                        }
                    }
                    if ((this.compiler.flags & QueryCompilerFlags.InverseQuery) != QueryCompilerFlags.None)
                    {
                        this.codeBlock.Append(new NumberIntervalOpcode(number, expr.Op));
                    }
                    else
                    {
                        this.codeBlock.Append(new NumberRelationOpcode(number, expr.Op));
                    }
                }
            }

            private void CompilePath(XPathExpr expr)
            {
                if (expr.Type == XPathExprType.Filter)
                {
                    this.CompileFilter(expr.SubExpr[0]);
                }
                else
                {
                    this.CompileExpression(expr.SubExpr[0]);
                    if (expr.SubExpr[0].ReturnType == ValueDataType.Sequence)
                    {
                        if (this.IsSpecialInternalFunction(expr.SubExpr[0]))
                        {
                            this.codeBlock.DetachLast();
                        }
                        else
                        {
                            this.codeBlock.Append(new MergeOpcode());
                            this.codeBlock.Append(new PopSequenceToSequenceStackOpcode());
                        }
                    }
                }
                if (expr.SubExprCount == 2)
                {
                    this.CompileRelativePath(expr.SubExpr[1], false);
                }
                else if (expr.SubExprCount == 3)
                {
                    XPathExpr expr2 = expr.SubExpr[1];
                    XPathStepExpr expr3 = (XPathStepExpr) expr2;
                    if (!expr3.SelectDesc.Axis.IsSupported())
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryCompileException(QueryCompileError.UnsupportedAxis));
                    }
                    this.codeBlock.Append(new SelectOpcode(expr3.SelectDesc));
                    if (expr3.SubExprCount > 0)
                    {
                        this.compiler.nestingLevel++;
                        if (this.compiler.nestingLevel > 3)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryCompileException(QueryCompileError.PredicateNestingTooDeep));
                        }
                        this.CompilePredicates(expr3.SubExpr);
                        this.compiler.nestingLevel--;
                    }
                    this.CompileRelativePath(expr.SubExpr[2], false);
                }
            }

            private void CompilePredicate(XPathExpr expr)
            {
                if (expr.IsLiteral && (XPathExprType.Number == expr.Type))
                {
                    this.CompileLiteralOrdinal(expr);
                }
                else
                {
                    this.CompileExpression(expr);
                    if (expr.ReturnType == ValueDataType.Double)
                    {
                        this.codeBlock.Append(new OrdinalOpcode());
                    }
                    else if (expr.ReturnType != ValueDataType.Boolean)
                    {
                        this.CompileTypecast(ValueDataType.Boolean);
                    }
                }
                this.codeBlock.Append(new ApplyFilterOpcode());
            }

            private void CompilePredicates(XPathExprList exprList)
            {
                for (int i = 0; i < exprList.Count; i++)
                {
                    this.CompilePredicate(exprList[i]);
                }
            }

            private void CompileRelational(XPathRelationExpr expr)
            {
                if (expr.Left.IsLiteral && expr.Right.IsLiteral)
                {
                    this.CompileLiteralRelation(expr);
                }
                else
                {
                    if (expr.Op != RelationOperator.Ne)
                    {
                        if ((XPathExprType.Number == expr.Left.Type) || (XPathExprType.Number == expr.Right.Type))
                        {
                            this.CompileNumberRelation(expr);
                            return;
                        }
                        if ((expr.Op == RelationOperator.Eq) && ((XPathExprType.String == expr.Left.Type) || (XPathExprType.String == expr.Right.Type)))
                        {
                            this.CompileStringLiteralEquality(expr);
                            return;
                        }
                    }
                    this.CompileExpression(expr.Left);
                    this.CompileExpression(expr.Right);
                    this.codeBlock.Append(new RelationOpcode(expr.Op));
                }
            }

            private void CompileRelativePath(XPathExpr expr, bool start)
            {
                this.CompileSteps(expr.SubExpr, start);
                this.codeBlock.Append(new PopSequenceToValueStackOpcode());
            }

            private void CompileStringLiteralEquality(XPathRelationExpr expr)
            {
                bool flag = XPathExprType.String == expr.Left.Type;
                XPathExprType type = expr.Right.Type;
                this.CompileExpression(flag ? expr.Right : expr.Left);
                string literal = flag ? ((XPathStringExpr) expr.Left).String : ((XPathStringExpr) expr.Right).String;
                this.codeBlock.Append(new StringEqualsOpcode(literal));
            }

            private void CompileSteps(XPathExprList steps)
            {
                this.CompileSteps(steps, true);
            }

            private void CompileSteps(XPathExprList steps, bool start)
            {
                for (int i = 0; i < steps.Count; i++)
                {
                    XPathStepExpr expr = (XPathStepExpr) steps[i];
                    if (!expr.SelectDesc.Axis.IsSupported())
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryCompileException(QueryCompileError.UnsupportedAxis));
                    }
                    Opcode opcode = null;
                    if (start && (i == 0))
                    {
                        if (QueryNodeType.Root == expr.SelectDesc.Type)
                        {
                            opcode = new SelectRootOpcode();
                        }
                        else
                        {
                            opcode = new InitialSelectOpcode(expr.SelectDesc);
                        }
                    }
                    else
                    {
                        opcode = new SelectOpcode(expr.SelectDesc);
                    }
                    this.codeBlock.Append(opcode);
                    if (expr.SubExprCount > 0)
                    {
                        this.compiler.nestingLevel++;
                        if (this.compiler.nestingLevel > 3)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryCompileException(QueryCompileError.PredicateNestingTooDeep));
                        }
                        this.CompilePredicates(expr.SubExpr);
                        this.compiler.nestingLevel--;
                    }
                }
            }

            private void CompileTypecast(ValueDataType destType)
            {
                this.codeBlock.Append(new TypecastOpcode(destType));
            }

            private void CompileXsltFunction(XPathXsltFunctionExpr expr)
            {
                if (expr.SubExprCount > 0)
                {
                    XPathExprList subExpr = expr.SubExpr;
                    for (int i = subExpr.Count - 1; i >= 0; i--)
                    {
                        XPathExpr expr2 = subExpr[i];
                        this.CompileExpression(expr2);
                        ValueDataType destType = XPathXsltFunctionExpr.ConvertTypeFromXslt(expr.Function.ArgTypes[i]);
                        if ((destType != ValueDataType.None) && (expr2.ReturnType != destType))
                        {
                            this.CompileTypecast(destType);
                        }
                    }
                }
                if (expr.Function is XPathMessageFunction)
                {
                    this.codeBlock.Append(new XPathMessageFunctionCallOpcode((XPathMessageFunction) expr.Function, expr.SubExprCount));
                    if (this.IsSpecialInternalFunction(expr))
                    {
                        this.codeBlock.Append(new PopSequenceToValueStackOpcode());
                    }
                }
                else
                {
                    this.codeBlock.Append(new XsltFunctionCallOpcode(expr.Context, expr.Function, expr.SubExprCount));
                }
            }

            private void CompileXsltVariable(XPathXsltVariableExpr expr)
            {
                this.codeBlock.Append(new PushXsltVariableOpcode(expr.Context, expr.Variable));
            }

            private MathOpcode CreateMathOpcode(MathOperator op)
            {
                MathOpcode opcode = null;
                switch (op)
                {
                    case MathOperator.None:
                        return opcode;

                    case MathOperator.Plus:
                        return new PlusOpcode();

                    case MathOperator.Minus:
                        return new MinusOpcode();

                    case MathOperator.Div:
                        return new DivideOpcode();

                    case MathOperator.Multiply:
                        return new MultiplyOpcode();

                    case MathOperator.Mod:
                        return new ModulusOpcode();

                    case MathOperator.Negate:
                        return new NegateOpcode();
                }
                return opcode;
            }

            private void NegateIfRequired(XPathExpr expr)
            {
                this.TypecastIfRequired(expr);
                if (expr.Negate)
                {
                    expr.Negate = false;
                    this.codeBlock.Append(new NegateOpcode());
                }
            }

            private void TypecastIfRequired(XPathExpr expr)
            {
                if (expr.TypecastRequired)
                {
                    expr.TypecastRequired = false;
                    this.CompileTypecast(expr.ReturnType);
                }
            }

            private void ThrowError(QueryCompileError error)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryCompileException(error));
            }
        }
    }
}

