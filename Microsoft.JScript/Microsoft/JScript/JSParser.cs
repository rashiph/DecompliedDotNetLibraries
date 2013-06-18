namespace Microsoft.JScript
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text.RegularExpressions;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public class JSParser
    {
        private ArrayList blockType;
        private int breakRecursion;
        private const int c_MaxSkippedTokenNumber = 50;
        private Context currentToken;
        private bool demandFullTrustOnFunctionCreation;
        private Context errorToken;
        private int finallyEscaped;
        private Microsoft.JScript.Globals Globals;
        private long goodTokensProcessed;
        private SimpleHashtable labelTable;
        private NoSkipTokenSet noSkipTokenSet;
        private Block program;
        private static int s_cDummyName;
        private JSScanner scanner;
        private int Severity;
        private Context sourceContext;
        private int tokensSkipped;

        public JSParser(Context context)
        {
            this.sourceContext = context;
            this.currentToken = context.Clone();
            this.scanner = new JSScanner(this.currentToken);
            this.noSkipTokenSet = new NoSkipTokenSet();
            this.errorToken = null;
            this.program = null;
            this.blockType = new ArrayList(0x10);
            this.labelTable = new SimpleHashtable(0x10);
            this.finallyEscaped = 0;
            this.Globals = context.document.engine.Globals;
            this.Severity = 5;
            this.demandFullTrustOnFunctionCreation = false;
        }

        private bool CheckForReturnFromFinally()
        {
            int num = 0;
            for (int i = this.blockType.Count - 1; i >= 0; i--)
            {
                if (((BlockType) this.blockType[i]) == BlockType.Finally)
                {
                    num++;
                }
            }
            if (num > this.finallyEscaped)
            {
                this.finallyEscaped = num;
            }
            return (num > 0);
        }

        private AST CreateExpressionNode(JSToken op, AST operand1, AST operand2)
        {
            Context context = operand1.context.CombineWith(operand2.context);
            switch (op)
            {
                case JSToken.FirstBinaryOp:
                    return new Plus(context, operand1, operand2);

                case JSToken.Minus:
                    return new NumericBinary(context, operand1, operand2, JSToken.Minus);

                case JSToken.LogicalOr:
                    return new Logical_or(context, operand1, operand2);

                case JSToken.LogicalAnd:
                    return new Logical_and(context, operand1, operand2);

                case JSToken.BitwiseOr:
                    return new BitwiseBinary(context, operand1, operand2, JSToken.BitwiseOr);

                case JSToken.BitwiseXor:
                    return new BitwiseBinary(context, operand1, operand2, JSToken.BitwiseXor);

                case JSToken.BitwiseAnd:
                    return new BitwiseBinary(context, operand1, operand2, JSToken.BitwiseAnd);

                case JSToken.Equal:
                    return new Equality(context, operand1, operand2, JSToken.Equal);

                case JSToken.NotEqual:
                    return new Equality(context, operand1, operand2, JSToken.NotEqual);

                case JSToken.StrictEqual:
                    return new StrictEquality(context, operand1, operand2, JSToken.StrictEqual);

                case JSToken.StrictNotEqual:
                    return new StrictEquality(context, operand1, operand2, JSToken.StrictNotEqual);

                case JSToken.GreaterThan:
                    return new Relational(context, operand1, operand2, JSToken.GreaterThan);

                case JSToken.LessThan:
                    return new Relational(context, operand1, operand2, JSToken.LessThan);

                case JSToken.LessThanEqual:
                    return new Relational(context, operand1, operand2, JSToken.LessThanEqual);

                case JSToken.GreaterThanEqual:
                    return new Relational(context, operand1, operand2, JSToken.GreaterThanEqual);

                case JSToken.LeftShift:
                    return new BitwiseBinary(context, operand1, operand2, JSToken.LeftShift);

                case JSToken.RightShift:
                    return new BitwiseBinary(context, operand1, operand2, JSToken.RightShift);

                case JSToken.UnsignedRightShift:
                    return new BitwiseBinary(context, operand1, operand2, JSToken.UnsignedRightShift);

                case JSToken.Multiply:
                    return new NumericBinary(context, operand1, operand2, JSToken.Multiply);

                case JSToken.Divide:
                    return new NumericBinary(context, operand1, operand2, JSToken.Divide);

                case JSToken.Modulo:
                    return new NumericBinary(context, operand1, operand2, JSToken.Modulo);

                case JSToken.Instanceof:
                    return new Instanceof(context, operand1, operand2);

                case JSToken.In:
                    return new In(context, operand1, operand2);

                case JSToken.Assign:
                    return new Assign(context, operand1, operand2);

                case JSToken.PlusAssign:
                    return new PlusAssign(context, operand1, operand2);

                case JSToken.MinusAssign:
                    return new NumericBinaryAssign(context, operand1, operand2, JSToken.Minus);

                case JSToken.MultiplyAssign:
                    return new NumericBinaryAssign(context, operand1, operand2, JSToken.Multiply);

                case JSToken.DivideAssign:
                    return new NumericBinaryAssign(context, operand1, operand2, JSToken.Divide);

                case JSToken.BitwiseAndAssign:
                    return new BitwiseBinaryAssign(context, operand1, operand2, JSToken.BitwiseAnd);

                case JSToken.BitwiseOrAssign:
                    return new BitwiseBinaryAssign(context, operand1, operand2, JSToken.BitwiseOr);

                case JSToken.BitwiseXorAssign:
                    return new BitwiseBinaryAssign(context, operand1, operand2, JSToken.BitwiseXor);

                case JSToken.ModuloAssign:
                    return new NumericBinaryAssign(context, operand1, operand2, JSToken.Modulo);

                case JSToken.LeftShiftAssign:
                    return new BitwiseBinaryAssign(context, operand1, operand2, JSToken.LeftShift);

                case JSToken.RightShiftAssign:
                    return new BitwiseBinaryAssign(context, operand1, operand2, JSToken.RightShift);

                case JSToken.UnsignedRightShiftAssign:
                    return new BitwiseBinaryAssign(context, operand1, operand2, JSToken.UnsignedRightShift);

                case JSToken.Comma:
                    return new Comma(context, operand1, operand2);
            }
            return null;
        }

        private Context CurrentPositionContext()
        {
            Context context = this.currentToken.Clone();
            context.endPos = (context.startPos < context.source_string.Length) ? (context.startPos + 1) : context.startPos;
            return context;
        }

        private void EOFError(JSError errorId)
        {
            Context context = this.sourceContext.Clone();
            context.lineNumber = this.scanner.GetCurrentLine();
            context.endLineNumber = context.lineNumber;
            context.startLinePos = this.scanner.GetStartLinePosition();
            context.endLinePos = context.startLinePos;
            context.startPos = this.sourceContext.endPos;
            context.endPos++;
            context.HandleError(errorId);
        }

        private void ForceReportInfo(JSError errorId)
        {
            this.ForceReportInfo(this.currentToken.Clone(), errorId);
        }

        private void ForceReportInfo(Context context, JSError errorId)
        {
            context.HandleError(errorId);
        }

        private void ForceReportInfo(JSError errorId, bool treatAsError)
        {
            this.currentToken.Clone().HandleError(errorId, treatAsError);
        }

        private CustomAttributeList FromASTListToCustomAttributeList(ArrayList attributes)
        {
            CustomAttributeList list = null;
            if ((attributes != null) && (attributes.Count > 0))
            {
                list = new CustomAttributeList(((AST) attributes[0]).context);
            }
            int num = 0;
            int count = attributes.Count;
            while (num < count)
            {
                ASTList args = new ASTList(null);
                if ((attributes[num] is Lookup) || (attributes[num] is Member))
                {
                    list.Append(new Microsoft.JScript.CustomAttribute(((AST) attributes[num]).context, (AST) attributes[num], args));
                }
                else
                {
                    list.Append(((Call) attributes[num]).ToCustomAttribute());
                }
                num++;
            }
            return list;
        }

        private void GetNextToken()
        {
            if (this.errorToken != null)
            {
                if (this.breakRecursion > 10)
                {
                    this.errorToken = null;
                    this.scanner.GetNextToken();
                }
                else
                {
                    this.breakRecursion++;
                    this.currentToken = this.errorToken;
                    this.errorToken = null;
                }
            }
            else
            {
                this.goodTokensProcessed += 1L;
                this.breakRecursion = 0;
                this.scanner.GetNextToken();
            }
        }

        private bool GuessIfAbstract()
        {
            JSToken token = this.currentToken.token;
            if (token <= JSToken.Interface)
            {
                switch (token)
                {
                    case JSToken.Package:
                    case JSToken.Internal:
                    case JSToken.Abstract:
                    case JSToken.Public:
                    case JSToken.Static:
                    case JSToken.Private:
                    case JSToken.Protected:
                    case JSToken.Final:
                    case JSToken.Const:
                    case JSToken.Class:
                    case JSToken.Function:
                    case JSToken.Interface:
                        goto Label_0067;

                    case JSToken.Semicolon:
                        this.GetNextToken();
                        return true;
                }
                goto Label_0071;
            }
            if ((token != JSToken.RightCurly) && (token != JSToken.Enum))
            {
                goto Label_0071;
            }
        Label_0067:
            return true;
        Label_0071:
            return false;
        }

        private int IndexOfToken(JSToken[] tokens, JSToken token)
        {
            int index = 0;
            int length = tokens.Length;
            while (index < length)
            {
                if (tokens[index] == token)
                {
                    break;
                }
                index++;
            }
            if (index >= length)
            {
                return -1;
            }
            this.errorToken = null;
            return index;
        }

        private int IndexOfToken(JSToken[] tokens, RecoveryTokenException exc)
        {
            return this.IndexOfToken(tokens, exc._token);
        }

        private AST MemberExpression(AST expression, ArrayList newContexts)
        {
            bool canBeAttribute = false;
            return this.MemberExpression(expression, newContexts, ref canBeAttribute);
        }

        private AST MemberExpression(AST expression, ArrayList newContexts, ref bool canBeAttribute)
        {
            bool flag;
            return this.MemberExpression(expression, newContexts, out flag, ref canBeAttribute);
        }

        private AST MemberExpression(AST expression, ArrayList newContexts, out bool canBeQualid, ref bool canBeAttribute)
        {
            AST ast;
            bool flag = false;
            canBeQualid = true;
        Label_0005:
            this.noSkipTokenSet.Add(NoSkipTokenSet.s_MemberExprNoSkipTokenSet);
            try
            {
                try
                {
                    ASTList list;
                    ConstantWrapper wrapper;
                    switch (this.currentToken.token)
                    {
                        case JSToken.LeftParen:
                            if (!flag)
                            {
                                break;
                            }
                            canBeAttribute = false;
                            goto Label_0048;

                        case JSToken.LeftBracket:
                            canBeQualid = false;
                            canBeAttribute = false;
                            this.noSkipTokenSet.Add(NoSkipTokenSet.s_BracketToken);
                            try
                            {
                                list = this.ParseExpressionList(JSToken.RightBracket);
                            }
                            catch (RecoveryTokenException exception3)
                            {
                                if (this.IndexOfToken(NoSkipTokenSet.s_BracketToken, exception3) == -1)
                                {
                                    if (exception3._partiallyComputedNode != null)
                                    {
                                        exception3._partiallyComputedNode = new Call(expression.context.CombineWith(this.currentToken.Clone()), expression, (ASTList) exception3._partiallyComputedNode, true);
                                    }
                                    else
                                    {
                                        exception3._partiallyComputedNode = expression;
                                    }
                                    throw exception3;
                                }
                                list = (ASTList) exception3._partiallyComputedNode;
                            }
                            finally
                            {
                                this.noSkipTokenSet.Remove(NoSkipTokenSet.s_BracketToken);
                            }
                            expression = new Call(expression.context.CombineWith(this.currentToken.Clone()), expression, list, true);
                            if ((newContexts != null) && (newContexts.Count > 0))
                            {
                                ((Context) newContexts[newContexts.Count - 1]).UpdateWith(expression.context);
                                expression.context = (Context) newContexts[newContexts.Count - 1];
                                ((Call) expression).isConstructor = true;
                                newContexts.RemoveAt(newContexts.Count - 1);
                            }
                            this.GetNextToken();
                            goto Label_0005;

                        case JSToken.AccessField:
                        {
                            if (flag)
                            {
                                canBeAttribute = false;
                            }
                            wrapper = null;
                            this.GetNextToken();
                            if (JSToken.Identifier == this.currentToken.token)
                            {
                                goto Label_03EF;
                            }
                            string str2 = JSKeyword.CanBeIdentifier(this.currentToken.token);
                            if (str2 != null)
                            {
                                this.ForceReportInfo(JSError.KeywordUsedAsIdentifier);
                                wrapper = new ConstantWrapper(str2, this.currentToken.Clone());
                            }
                            else
                            {
                                this.ReportError(JSError.NoIdentifier);
                                this.SkipTokensAndThrow(expression);
                            }
                            goto Label_040C;
                        }
                        default:
                            if (newContexts != null)
                            {
                                while (newContexts.Count > 0)
                                {
                                    ((Context) newContexts[newContexts.Count - 1]).UpdateWith(expression.context);
                                    expression = new Call((Context) newContexts[newContexts.Count - 1], expression, new ASTList(this.CurrentPositionContext()), false);
                                    ((Call) expression).isConstructor = true;
                                    newContexts.RemoveAt(newContexts.Count - 1);
                                }
                            }
                            return expression;
                    }
                    flag = true;
                Label_0048:
                    canBeQualid = false;
                    list = null;
                    RecoveryTokenException exception = null;
                    this.noSkipTokenSet.Add(NoSkipTokenSet.s_ParenToken);
                    try
                    {
                        list = this.ParseExpressionList(JSToken.RightParen);
                    }
                    catch (RecoveryTokenException exception2)
                    {
                        list = (ASTList) exception2._partiallyComputedNode;
                        if (this.IndexOfToken(NoSkipTokenSet.s_ParenToken, exception2) == -1)
                        {
                            exception = exception2;
                        }
                    }
                    finally
                    {
                        this.noSkipTokenSet.Remove(NoSkipTokenSet.s_ParenToken);
                    }
                    if (expression is Lookup)
                    {
                        string str = expression.ToString();
                        if (str.Equals("eval"))
                        {
                            expression.context.UpdateWith(list.context);
                            if (list.count == 1)
                            {
                                expression = new Eval(expression.context, list[0], null);
                            }
                            else if (list.count > 1)
                            {
                                expression = new Eval(expression.context, list[0], list[1]);
                            }
                            else
                            {
                                expression = new Eval(expression.context, new ConstantWrapper("", this.CurrentPositionContext()), null);
                            }
                            canBeAttribute = false;
                        }
                        else if (this.Globals.engine.doPrint && str.Equals("print"))
                        {
                            expression.context.UpdateWith(list.context);
                            expression = new Print(expression.context, list);
                            canBeAttribute = false;
                        }
                        else
                        {
                            expression = new Call(expression.context.CombineWith(list.context), expression, list, false);
                        }
                    }
                    else
                    {
                        expression = new Call(expression.context.CombineWith(list.context), expression, list, false);
                    }
                    if ((newContexts != null) && (newContexts.Count > 0))
                    {
                        ((Context) newContexts[newContexts.Count - 1]).UpdateWith(expression.context);
                        if (expression is Call)
                        {
                            expression.context = (Context) newContexts[newContexts.Count - 1];
                        }
                        else
                        {
                            expression = new Call((Context) newContexts[newContexts.Count - 1], expression, new ASTList(this.CurrentPositionContext()), false);
                        }
                        ((Call) expression).isConstructor = true;
                        newContexts.RemoveAt(newContexts.Count - 1);
                    }
                    if (exception != null)
                    {
                        exception._partiallyComputedNode = expression;
                        throw exception;
                    }
                    this.GetNextToken();
                    goto Label_0005;
                Label_03EF:
                    wrapper = new ConstantWrapper(this.scanner.GetIdentifier(), this.currentToken.Clone());
                Label_040C:
                    this.GetNextToken();
                    expression = new Member(expression.context.CombineWith(wrapper.context), expression, wrapper);
                }
                catch (RecoveryTokenException exception4)
                {
                    if (this.IndexOfToken(NoSkipTokenSet.s_MemberExprNoSkipTokenSet, exception4) == -1)
                    {
                        throw exception4;
                    }
                    expression = exception4._partiallyComputedNode;
                }
                goto Label_0005;
            }
            finally
            {
                this.noSkipTokenSet.Remove(NoSkipTokenSet.s_MemberExprNoSkipTokenSet);
            }
            return ast;
        }

        public ScriptBlock Parse()
        {
            return new ScriptBlock(this.sourceContext.Clone(), this.ParseStatements(false));
        }

        private AST ParseAttributes(AST statement, bool unambiguousContext, bool isInsideClass, out bool parsedOK)
        {
            JSToken none;
            bool flag;
            bool flag2;
            AST ast = statement;
            ArrayList list = new ArrayList();
            ArrayList list2 = new ArrayList();
            ArrayList list3 = new ArrayList();
            AST ast2 = null;
            ArrayList list4 = new ArrayList();
            Context context = null;
            Context context2 = null;
            Context context3 = null;
            int num = 0;
            if (unambiguousContext)
            {
                num = 2;
            }
            FieldAttributes privateScope = FieldAttributes.PrivateScope;
            FieldAttributes family = FieldAttributes.PrivateScope;
            Context enumCtx = null;
            if (statement != null)
            {
                ast2 = statement;
                list4.Add(statement);
                list.Add(this.CurrentPositionContext());
                enumCtx = statement.context.Clone();
                num = 1;
            }
            else
            {
                enumCtx = this.currentToken.Clone();
            }
            parsedOK = true;
        Label_0078:
            none = JSToken.None;
            switch (this.currentToken.token)
            {
                case JSToken.Boolean:
                case JSToken.Byte:
                case JSToken.Char:
                case JSToken.Double:
                case JSToken.Float:
                case JSToken.Int:
                case JSToken.Long:
                case JSToken.Short:
                case JSToken.Void:
                    parsedOK = false;
                    ast2 = new Lookup(this.currentToken);
                    none = JSToken.None;
                    list4.Add(ast2);
                    this.GetNextToken();
                    goto Label_0A12;

                case JSToken.Enum:
                {
                    int num8 = 0;
                    int num9 = list3.Count;
                    while (num8 < num9)
                    {
                        this.ReportError((JSError) list3[num8], (Context) list3[num8 + 1], true);
                        num8 += 2;
                    }
                    enumCtx.UpdateWith(this.currentToken);
                    if (context != null)
                    {
                        this.ReportError(JSError.IllegalVisibility, context, true);
                    }
                    if (context3 != null)
                    {
                        this.ReportError(JSError.IllegalVisibility, context3, true);
                    }
                    if (context2 != null)
                    {
                        this.ReportError(JSError.IllegalVisibility, context2, true);
                    }
                    return this.ParseEnum(privateScope, enumCtx, this.FromASTListToCustomAttributeList(list4));
                }
                case JSToken.Interface:
                    if (context != null)
                    {
                        this.ReportError(JSError.IllegalVisibility, context, true);
                        context = null;
                    }
                    if (context3 != null)
                    {
                        this.ReportError(JSError.IllegalVisibility, context3, true);
                        context3 = null;
                    }
                    if (context2 != null)
                    {
                        this.ReportError(JSError.IllegalVisibility, context2, true);
                        context2 = null;
                    }
                    break;

                case JSToken.Internal:
                case JSToken.Abstract:
                case JSToken.Public:
                case JSToken.Static:
                case JSToken.Private:
                case JSToken.Protected:
                case JSToken.Final:
                    none = this.currentToken.token;
                    goto Label_042A;

                case JSToken.Var:
                case JSToken.Const:
                {
                    int num2 = 0;
                    int num3 = list3.Count;
                    while (num2 < num3)
                    {
                        this.ReportError((JSError) list3[num2], (Context) list3[num2 + 1], true);
                        num2 += 2;
                    }
                    if (context != null)
                    {
                        this.ReportError(JSError.IllegalVisibility, context, true);
                    }
                    if (context3 != null)
                    {
                        this.ReportError(JSError.IllegalVisibility, context3, true);
                    }
                    enumCtx.UpdateWith(this.currentToken);
                    return this.ParseVariableStatement(privateScope, this.FromASTListToCustomAttributeList(list4), this.currentToken.token);
                }
                case JSToken.Class:
                    break;

                case JSToken.Function:
                {
                    int num4 = 0;
                    int num5 = list3.Count;
                    while (num4 < num5)
                    {
                        this.ReportError((JSError) list3[num4], (Context) list3[num4 + 1], true);
                        num4 += 2;
                    }
                    enumCtx.UpdateWith(this.currentToken);
                    if (context2 != null)
                    {
                        if (context != null)
                        {
                            context2.HandleError(JSError.AbstractCannotBeStatic);
                            context2 = null;
                        }
                        else if (context3 != null)
                        {
                            context3.HandleError(JSError.StaticIsAlreadyFinal);
                            context3 = null;
                        }
                    }
                    if (context != null)
                    {
                        if (context3 != null)
                        {
                            context3.HandleError(JSError.FinalPrecludesAbstract);
                            context3 = null;
                        }
                        if (family == FieldAttributes.Private)
                        {
                            context.HandleError(JSError.AbstractCannotBePrivate);
                            family = FieldAttributes.Family;
                        }
                    }
                    return this.ParseFunction(privateScope, false, enumCtx, isInsideClass, context != null, context3 != null, false, this.FromASTListToCustomAttributeList(list4));
                }
                case JSToken.Identifier:
                    goto Label_042A;

                default:
                    goto Label_047E;
            }
            int num6 = 0;
            int count = list3.Count;
            while (num6 < count)
            {
                this.ReportError((JSError) list3[num6], (Context) list3[num6 + 1], true);
                num6 += 2;
            }
            enumCtx.UpdateWith(this.currentToken);
            if ((context3 != null) && (context != null))
            {
                context3.HandleError(JSError.FinalPrecludesAbstract);
            }
            return this.ParseClass(privateScope, context2 != null, enumCtx, context != null, context3 != null, this.FromASTListToCustomAttributeList(list4));
        Label_042A:
            flag2 = true;
            statement = this.ParseUnaryExpression(out flag, ref flag2, false, none == JSToken.None);
            ast2 = statement;
            if (none != JSToken.None)
            {
                if (statement is Lookup)
                {
                    goto Label_07ED;
                }
                if (num != 2)
                {
                    list2.Add(this.currentToken.Clone());
                }
            }
            none = JSToken.None;
            if (flag2)
            {
                list4.Add(statement);
                goto Label_0A12;
            }
        Label_047E:
            parsedOK = false;
            if (num != 2)
            {
                if ((ast != statement) || (statement == null))
                {
                    statement = ast2;
                    int num10 = 0;
                    int num11 = list2.Count;
                    while (num10 < num11)
                    {
                        this.ForceReportInfo((Context) list2[num10], JSError.KeywordUsedAsIdentifier);
                        num10++;
                    }
                    int num12 = 0;
                    int num13 = list.Count;
                    while (num12 < num13)
                    {
                        if (!this.currentToken.Equals((Context) list[num12]))
                        {
                            this.ReportError(JSError.NoSemicolon, (Context) list[num12], true);
                        }
                        num12++;
                    }
                }
                return statement;
            }
            if (list4.Count > 0)
            {
                AST ast3 = (AST) list4[list4.Count - 1];
                if (ast3 is Lookup)
                {
                    if ((JSToken.Semicolon == this.currentToken.token) || (JSToken.Colon == this.currentToken.token))
                    {
                        this.ReportError(JSError.BadVariableDeclaration, ast3.context.Clone());
                        this.SkipTokensAndThrow();
                    }
                }
                else if ((ast3 is Call) && ((Call) ast3).CanBeFunctionDeclaration())
                {
                    if ((JSToken.Colon == this.currentToken.token) || (JSToken.LeftCurly == this.currentToken.token))
                    {
                        this.ReportError(JSError.BadFunctionDeclaration, ast3.context.Clone(), true);
                        if (JSToken.Colon == this.currentToken.token)
                        {
                            this.noSkipTokenSet.Add(NoSkipTokenSet.s_StartBlockNoSkipTokenSet);
                            try
                            {
                                this.SkipTokensAndThrow();
                            }
                            catch (RecoveryTokenException)
                            {
                            }
                            finally
                            {
                                this.noSkipTokenSet.Remove(NoSkipTokenSet.s_StartBlockNoSkipTokenSet);
                            }
                        }
                        this.errorToken = null;
                        if (JSToken.LeftCurly == this.currentToken.token)
                        {
                            FunctionScope item = new FunctionScope(this.Globals.ScopeStack.Peek(), isInsideClass);
                            this.Globals.ScopeStack.Push(item);
                            try
                            {
                                this.ParseBlock();
                            }
                            finally
                            {
                                this.Globals.ScopeStack.Pop();
                            }
                            this.SkipTokensAndThrow();
                        }
                    }
                    else
                    {
                        this.ReportError(JSError.SyntaxError, ast3.context.Clone());
                    }
                    this.SkipTokensAndThrow();
                }
            }
            if ((JSToken.LeftCurly == this.currentToken.token) && isInsideClass)
            {
                int num14 = 0;
                int num15 = list3.Count;
                while (num14 < num15)
                {
                    this.ReportError((JSError) list3[num14], (Context) list3[num14 + 1]);
                    num14 += 2;
                }
                if (context2 == null)
                {
                    this.ReportError(JSError.StaticMissingInStaticInit, this.CurrentPositionContext());
                }
                string name = ((ClassScope) this.Globals.ScopeStack.Peek()).name;
                bool flag3 = true;
                foreach (object obj2 in list4)
                {
                    flag3 = false;
                    if (((context2 == null) || !(obj2 is Lookup)) || ((obj2.ToString() != name) || (((Lookup) obj2).context.StartColumn <= context2.StartColumn)))
                    {
                        this.ReportError(JSError.SyntaxError, ((AST) obj2).context);
                    }
                }
                if (flag3)
                {
                    this.ReportError(JSError.NoIdentifier, this.CurrentPositionContext());
                }
                this.errorToken = null;
                parsedOK = true;
                return this.ParseStaticInitializer(enumCtx);
            }
            this.ReportError(JSError.MissingConstructForAttributes, enumCtx.CombineWith(this.currentToken));
            this.SkipTokensAndThrow();
        Label_07ED:
            switch (none)
            {
                case JSToken.Internal:
                    family = FieldAttributes.Assembly;
                    break;

                case JSToken.Abstract:
                    if (context == null)
                    {
                        context = statement.context.Clone();
                    }
                    else
                    {
                        list3.Add(JSError.SyntaxError);
                        list3.Add(statement.context.Clone());
                    }
                    goto Label_0A12;

                case JSToken.Public:
                    family = FieldAttributes.Public;
                    break;

                case JSToken.Static:
                    if (!isInsideClass)
                    {
                        list3.Add(JSError.NotInsideClass);
                        list3.Add(statement.context.Clone());
                        break;
                    }
                    family = FieldAttributes.Static;
                    if (context2 == null)
                    {
                        context2 = statement.context.Clone();
                        break;
                    }
                    list3.Add(JSError.SyntaxError);
                    list3.Add(statement.context.Clone());
                    break;

                case JSToken.Private:
                    if (!isInsideClass)
                    {
                        list3.Add(JSError.NotInsideClass);
                        list3.Add(statement.context.Clone());
                        break;
                    }
                    family = FieldAttributes.Private;
                    break;

                case JSToken.Protected:
                    if (!isInsideClass)
                    {
                        list3.Add(JSError.NotInsideClass);
                        list3.Add(statement.context.Clone());
                        break;
                    }
                    family = FieldAttributes.Family;
                    break;

                case JSToken.Final:
                    if (context3 == null)
                    {
                        context3 = statement.context.Clone();
                    }
                    else
                    {
                        list3.Add(JSError.SyntaxError);
                        list3.Add(statement.context.Clone());
                    }
                    goto Label_0A12;
            }
            if (((privateScope & FieldAttributes.FieldAccessMask) == family) && (family != FieldAttributes.PrivateScope))
            {
                list3.Add(JSError.DupVisibility);
                list3.Add(statement.context.Clone());
            }
            else if (((privateScope & FieldAttributes.FieldAccessMask) > FieldAttributes.PrivateScope) && ((family & FieldAttributes.FieldAccessMask) > FieldAttributes.PrivateScope))
            {
                if (((family == FieldAttributes.Family) && ((privateScope & FieldAttributes.FieldAccessMask) == FieldAttributes.Assembly)) || ((family == FieldAttributes.Assembly) && ((privateScope & FieldAttributes.FieldAccessMask) == FieldAttributes.Family)))
                {
                    privateScope &= ~FieldAttributes.FieldAccessMask;
                    privateScope |= FieldAttributes.FamORAssem;
                }
                else
                {
                    list3.Add(JSError.IncompatibleVisibility);
                    list3.Add(statement.context.Clone());
                }
            }
            else
            {
                privateScope |= family;
                enumCtx.UpdateWith(statement.context);
            }
        Label_0A12:
            if (num != 2)
            {
                if (this.scanner.GotEndOfLine())
                {
                    num = 0;
                }
                else
                {
                    num++;
                    list.Add(this.currentToken.Clone());
                }
            }
            goto Label_0078;
        }

        private Block ParseBlock()
        {
            Context context;
            return this.ParseBlock(out context);
        }

        private Block ParseBlock(out Context closingBraceContext)
        {
            closingBraceContext = null;
            this.blockType.Add(BlockType.Block);
            Block block = new Block(this.currentToken.Clone());
            this.GetNextToken();
            this.noSkipTokenSet.Add(NoSkipTokenSet.s_StartStatementNoSkipTokenSet);
            this.noSkipTokenSet.Add(NoSkipTokenSet.s_BlockNoSkipTokenSet);
            try
            {
                while (JSToken.RightCurly != this.currentToken.token)
                {
                    try
                    {
                        block.Append(this.ParseStatement());
                        continue;
                    }
                    catch (RecoveryTokenException exception)
                    {
                        if (exception._partiallyComputedNode != null)
                        {
                            block.Append(exception._partiallyComputedNode);
                        }
                        if (this.IndexOfToken(NoSkipTokenSet.s_StartStatementNoSkipTokenSet, exception) == -1)
                        {
                            throw exception;
                        }
                        continue;
                    }
                }
            }
            catch (RecoveryTokenException exception2)
            {
                if (this.IndexOfToken(NoSkipTokenSet.s_BlockNoSkipTokenSet, exception2) == -1)
                {
                    exception2._partiallyComputedNode = block;
                    throw exception2;
                }
            }
            finally
            {
                this.noSkipTokenSet.Remove(NoSkipTokenSet.s_BlockNoSkipTokenSet);
                this.noSkipTokenSet.Remove(NoSkipTokenSet.s_StartStatementNoSkipTokenSet);
                this.blockType.RemoveAt(this.blockType.Count - 1);
            }
            closingBraceContext = this.currentToken.Clone();
            block.context.UpdateWith(this.currentToken);
            this.GetNextToken();
            return block;
        }

        private Break ParseBreakStatement()
        {
            Context context = this.currentToken.Clone();
            int num = 0;
            this.GetNextToken();
            string identifier = null;
            if (!this.scanner.GotEndOfLine() && ((JSToken.Identifier == this.currentToken.token) || ((identifier = JSKeyword.CanBeIdentifier(this.currentToken.token)) != null)))
            {
                context.UpdateWith(this.currentToken);
                if (identifier != null)
                {
                    this.ForceReportInfo(JSError.KeywordUsedAsIdentifier);
                }
                else
                {
                    identifier = this.scanner.GetIdentifier();
                }
                object obj2 = this.labelTable[identifier];
                if (obj2 == null)
                {
                    this.ReportError(JSError.NoLabel, true);
                    this.GetNextToken();
                    return null;
                }
                num = ((int) obj2) - 1;
                this.GetNextToken();
            }
            else
            {
                num = this.blockType.Count - 1;
                while (((((BlockType) this.blockType[num]) == BlockType.Block) || (((BlockType) this.blockType[num]) == BlockType.Finally)) && (--num >= 0))
                {
                }
                num--;
                if (num < 0)
                {
                    this.ReportError(JSError.BadBreak, context, true);
                    return null;
                }
            }
            if (JSToken.Semicolon == this.currentToken.token)
            {
                context.UpdateWith(this.currentToken);
                this.GetNextToken();
            }
            else if ((JSToken.RightCurly != this.currentToken.token) && !this.scanner.GotEndOfLine())
            {
                this.ReportError(JSError.NoSemicolon, true);
            }
            int num2 = 0;
            int num3 = num;
            int count = this.blockType.Count;
            while (num3 < count)
            {
                if (((BlockType) this.blockType[num3]) == BlockType.Finally)
                {
                    num++;
                    num2++;
                }
                num3++;
            }
            if (num2 > this.finallyEscaped)
            {
                this.finallyEscaped = num2;
            }
            return new Break(context, (this.blockType.Count - num) - 1, num2 > 0);
        }

        private AST ParseClass(FieldAttributes visibilitySpec, bool isStatic, Context classCtx, bool isAbstract, bool isFinal, CustomAttributeList customAttributes)
        {
            AST name = null;
            AST ast2 = null;
            TypeExpression superTypeExpression = null;
            Block body = null;
            TypeExpression[] expressionArray;
            AST ast4;
            ArrayList list = new ArrayList();
            bool isInterface = JSToken.Interface == this.currentToken.token;
            this.GetNextToken();
            if (JSToken.Identifier == this.currentToken.token)
            {
                name = new IdentifierLiteral(this.scanner.GetIdentifier(), this.currentToken.Clone());
            }
            else
            {
                this.ReportError(JSError.NoIdentifier);
                if (((JSToken.Extends != this.currentToken.token) && (JSToken.Implements != this.currentToken.token)) && (JSToken.LeftCurly != this.currentToken.token))
                {
                    this.SkipTokensAndThrow();
                }
                name = new IdentifierLiteral("##Missing Class Name##" + s_cDummyName++, this.CurrentPositionContext());
            }
            this.GetNextToken();
            if ((JSToken.Extends == this.currentToken.token) || (JSToken.Implements == this.currentToken.token))
            {
                if (isInterface && (JSToken.Extends == this.currentToken.token))
                {
                    this.currentToken.token = JSToken.Implements;
                }
                if (JSToken.Extends == this.currentToken.token)
                {
                    this.noSkipTokenSet.Add(NoSkipTokenSet.s_ClassExtendsNoSkipTokenSet);
                    try
                    {
                        ast2 = this.ParseQualifiedIdentifier(JSError.NeedType);
                    }
                    catch (RecoveryTokenException exception)
                    {
                        if (this.IndexOfToken(NoSkipTokenSet.s_ClassExtendsNoSkipTokenSet, exception) == -1)
                        {
                            exception._partiallyComputedNode = null;
                            throw exception;
                        }
                        ast2 = exception._partiallyComputedNode;
                    }
                    finally
                    {
                        this.noSkipTokenSet.Remove(NoSkipTokenSet.s_ClassExtendsNoSkipTokenSet);
                    }
                }
                if (JSToken.Implements == this.currentToken.token)
                {
                    do
                    {
                        AST expression = null;
                        this.noSkipTokenSet.Add(NoSkipTokenSet.s_ClassImplementsNoSkipTokenSet);
                        try
                        {
                            expression = this.ParseQualifiedIdentifier(JSError.NeedType);
                            list.Add(new TypeExpression(expression));
                        }
                        catch (RecoveryTokenException exception2)
                        {
                            if (this.IndexOfToken(NoSkipTokenSet.s_ClassImplementsNoSkipTokenSet, exception2) == -1)
                            {
                                exception2._partiallyComputedNode = null;
                                throw exception2;
                            }
                            if (exception2._partiallyComputedNode != null)
                            {
                                list.Add(new TypeExpression(exception2._partiallyComputedNode));
                            }
                        }
                        finally
                        {
                            this.noSkipTokenSet.Remove(NoSkipTokenSet.s_ClassImplementsNoSkipTokenSet);
                        }
                    }
                    while (JSToken.Comma == this.currentToken.token);
                }
            }
            if (ast2 != null)
            {
                superTypeExpression = new TypeExpression(ast2);
            }
            if (JSToken.LeftCurly != this.currentToken.token)
            {
                this.ReportError(JSError.NoLeftCurly);
            }
            ArrayList blockType = this.blockType;
            this.blockType = new ArrayList(0x10);
            SimpleHashtable labelTable = this.labelTable;
            this.labelTable = new SimpleHashtable(0x10);
            this.Globals.ScopeStack.Push(new ClassScope(name, ((IActivationObject) this.Globals.ScopeStack.Peek()).GetGlobalScope()));
            try
            {
                body = this.ParseClassBody(false, isInterface);
                classCtx.UpdateWith(body.context);
                expressionArray = new TypeExpression[list.Count];
                list.CopyTo(expressionArray);
                Class target = new Class(classCtx, name, superTypeExpression, expressionArray, body, visibilitySpec, isAbstract, isFinal, isStatic, isInterface, customAttributes);
                if (customAttributes != null)
                {
                    customAttributes.SetTarget(target);
                }
                ast4 = target;
            }
            catch (RecoveryTokenException exception3)
            {
                classCtx.UpdateWith(exception3._partiallyComputedNode.context);
                expressionArray = new TypeExpression[list.Count];
                list.CopyTo(expressionArray);
                exception3._partiallyComputedNode = new Class(classCtx, name, superTypeExpression, expressionArray, (Block) exception3._partiallyComputedNode, visibilitySpec, isAbstract, isFinal, isStatic, isInterface, customAttributes);
                if (customAttributes != null)
                {
                    customAttributes.SetTarget(exception3._partiallyComputedNode);
                }
                throw exception3;
            }
            finally
            {
                this.Globals.ScopeStack.Pop();
                this.blockType = blockType;
                this.labelTable = labelTable;
            }
            return ast4;
        }

        private Block ParseClassBody(bool isEnum, bool isInterface)
        {
            this.blockType.Add(BlockType.Block);
            Block block = new Block(this.currentToken.Clone());
            try
            {
                this.GetNextToken();
                this.noSkipTokenSet.Add(NoSkipTokenSet.s_BlockNoSkipTokenSet);
                JSToken[] tokens = null;
                if (isEnum)
                {
                    tokens = NoSkipTokenSet.s_EnumBodyNoSkipTokenSet;
                }
                else if (isInterface)
                {
                    tokens = NoSkipTokenSet.s_InterfaceBodyNoSkipTokenSet;
                }
                else
                {
                    tokens = NoSkipTokenSet.s_ClassBodyNoSkipTokenSet;
                }
                try
                {
                    while (JSToken.RightCurly != this.currentToken.token)
                    {
                        if (this.currentToken.token == JSToken.EndOfFile)
                        {
                            this.ReportError(JSError.NoRightCurly, true);
                            this.SkipTokensAndThrow();
                        }
                        this.noSkipTokenSet.Add(tokens);
                        try
                        {
                            try
                            {
                                AST elem = isEnum ? this.ParseEnumMember() : this.ParseClassMember(isInterface);
                                if (elem != null)
                                {
                                    block.Append(elem);
                                }
                            }
                            catch (RecoveryTokenException exception)
                            {
                                if (exception._partiallyComputedNode != null)
                                {
                                    block.Append(exception._partiallyComputedNode);
                                }
                                if (this.IndexOfToken(tokens, exception) == -1)
                                {
                                    exception._partiallyComputedNode = null;
                                    throw exception;
                                }
                            }
                            continue;
                        }
                        finally
                        {
                            this.noSkipTokenSet.Remove(tokens);
                        }
                    }
                }
                catch (RecoveryTokenException exception2)
                {
                    exception2._partiallyComputedNode = block;
                    throw exception2;
                }
                finally
                {
                    this.noSkipTokenSet.Remove(NoSkipTokenSet.s_BlockNoSkipTokenSet);
                }
                block.context.UpdateWith(this.currentToken);
                this.GetNextToken();
            }
            finally
            {
                this.blockType.RemoveAt(this.blockType.Count - 1);
            }
            return block;
        }

        private AST ParseClassMember(bool isInterface)
        {
            bool parsedOK = false;
            if (isInterface && (this.currentToken.token == JSToken.Public))
            {
                this.GetNextToken();
            }
            switch (this.currentToken.token)
            {
                case JSToken.Import:
                    this.ReportError(JSError.InvalidImport, true);
                    try
                    {
                        this.ParseImportStatement();
                    }
                    catch (RecoveryTokenException)
                    {
                    }
                    return null;

                case JSToken.Package:
                {
                    Context packageContext = this.currentToken.Clone();
                    if (this.ParsePackage(packageContext) is Package)
                    {
                        this.ReportError(JSError.PackageInWrongContext, packageContext, true);
                    }
                    return null;
                }
                case JSToken.Internal:
                case JSToken.Abstract:
                case JSToken.Public:
                case JSToken.Static:
                case JSToken.Private:
                case JSToken.Protected:
                case JSToken.Final:
                    if (isInterface)
                    {
                        this.ReportError(JSError.BadModifierInInterface, true);
                        this.GetNextToken();
                        this.SkipTokensAndThrow();
                    }
                    return this.ParseAttributes(null, true, true, out parsedOK);

                case JSToken.Var:
                case JSToken.Const:
                    if (isInterface)
                    {
                        this.ReportError(JSError.VarIllegalInInterface, true);
                        this.GetNextToken();
                        this.SkipTokensAndThrow();
                    }
                    return this.ParseVariableStatement(FieldAttributes.PrivateScope, null, this.currentToken.token);

                case JSToken.Class:
                    if (isInterface)
                    {
                        this.ReportError(JSError.SyntaxError, true);
                        this.GetNextToken();
                        this.SkipTokensAndThrow();
                    }
                    return this.ParseClass(FieldAttributes.PrivateScope, false, this.currentToken.Clone(), false, false, null);

                case JSToken.Function:
                    return this.ParseFunction(FieldAttributes.PrivateScope, false, this.currentToken.Clone(), true, isInterface, false, isInterface, null);

                case JSToken.Semicolon:
                    this.GetNextToken();
                    return this.ParseClassMember(isInterface);

                case JSToken.Identifier:
                {
                    bool flag2;
                    if (isInterface)
                    {
                        this.ReportError(JSError.SyntaxError, true);
                        this.GetNextToken();
                        this.SkipTokensAndThrow();
                    }
                    bool canBeAttribute = true;
                    AST statement = this.ParseUnaryExpression(out flag2, ref canBeAttribute, false);
                    if (canBeAttribute)
                    {
                        statement = this.ParseAttributes(statement, true, true, out parsedOK);
                        if (parsedOK)
                        {
                            return statement;
                        }
                    }
                    this.ReportError(JSError.SyntaxError, statement.context.Clone(), true);
                    this.SkipTokensAndThrow();
                    return null;
                }
                case JSToken.Interface:
                    if (isInterface)
                    {
                        this.ReportError(JSError.InterfaceIllegalInInterface, true);
                        this.GetNextToken();
                        this.SkipTokensAndThrow();
                    }
                    return this.ParseClass(FieldAttributes.PrivateScope, false, this.currentToken.Clone(), false, false, null);

                case JSToken.RightCurly:
                    return null;

                case JSToken.Enum:
                    return this.ParseEnum(FieldAttributes.PrivateScope, this.currentToken.Clone(), null);
            }
            this.ReportError(JSError.SyntaxError, true);
            this.GetNextToken();
            this.SkipTokensAndThrow();
            return null;
        }

        private AST ParseConstructorCall(Context superCtx)
        {
            bool isSuperConstructorCall = JSToken.Super == this.currentToken.token;
            this.GetNextToken();
            Context context = this.currentToken.Clone();
            ASTList arguments = new ASTList(context);
            this.noSkipTokenSet.Add(NoSkipTokenSet.s_EndOfStatementNoSkipTokenSet);
            this.noSkipTokenSet.Add(NoSkipTokenSet.s_ParenToken);
            try
            {
                arguments = this.ParseExpressionList(JSToken.RightParen);
                this.GetNextToken();
            }
            catch (RecoveryTokenException exception)
            {
                if (exception._partiallyComputedNode != null)
                {
                    arguments = (ASTList) exception._partiallyComputedNode;
                }
                if ((this.IndexOfToken(NoSkipTokenSet.s_ParenToken, exception) == -1) && (this.IndexOfToken(NoSkipTokenSet.s_EndOfStatementNoSkipTokenSet, exception) == -1))
                {
                    exception._partiallyComputedNode = new ConstructorCall(superCtx, arguments, isSuperConstructorCall);
                    throw exception;
                }
                if (exception._token == JSToken.RightParen)
                {
                    this.GetNextToken();
                }
            }
            finally
            {
                this.noSkipTokenSet.Remove(NoSkipTokenSet.s_ParenToken);
                this.noSkipTokenSet.Remove(NoSkipTokenSet.s_EndOfStatementNoSkipTokenSet);
            }
            superCtx.UpdateWith(context);
            return new ConstructorCall(superCtx, arguments, isSuperConstructorCall);
        }

        private Continue ParseContinueStatement()
        {
            Context context = this.currentToken.Clone();
            int num = 0;
            this.GetNextToken();
            string identifier = null;
            if (!this.scanner.GotEndOfLine() && ((JSToken.Identifier == this.currentToken.token) || ((identifier = JSKeyword.CanBeIdentifier(this.currentToken.token)) != null)))
            {
                context.UpdateWith(this.currentToken);
                if (identifier != null)
                {
                    this.ForceReportInfo(JSError.KeywordUsedAsIdentifier);
                }
                else
                {
                    identifier = this.scanner.GetIdentifier();
                }
                object obj2 = this.labelTable[identifier];
                if (obj2 == null)
                {
                    this.ReportError(JSError.NoLabel, true);
                    this.GetNextToken();
                    return null;
                }
                num = (int) obj2;
                if (((BlockType) this.blockType[num]) != BlockType.Loop)
                {
                    this.ReportError(JSError.BadContinue, context.Clone(), true);
                }
                this.GetNextToken();
            }
            else
            {
                num = this.blockType.Count - 1;
                while ((num >= 0) && (((BlockType) this.blockType[num]) != BlockType.Loop))
                {
                    num--;
                }
                if (num < 0)
                {
                    this.ReportError(JSError.BadContinue, context, true);
                    return null;
                }
            }
            if (JSToken.Semicolon == this.currentToken.token)
            {
                context.UpdateWith(this.currentToken);
                this.GetNextToken();
            }
            else if ((JSToken.RightCurly != this.currentToken.token) && !this.scanner.GotEndOfLine())
            {
                this.ReportError(JSError.NoSemicolon, true);
            }
            int num2 = 0;
            int num3 = num;
            int count = this.blockType.Count;
            while (num3 < count)
            {
                if (((BlockType) this.blockType[num3]) == BlockType.Finally)
                {
                    num++;
                    num2++;
                }
                num3++;
            }
            if (num2 > this.finallyEscaped)
            {
                this.finallyEscaped = num2;
            }
            return new Continue(context, this.blockType.Count - num, num2 > 0);
        }

        private CustomAttributeList ParseCustomAttributeList()
        {
            CustomAttributeList list = new CustomAttributeList(this.currentToken.Clone());
            while (true)
            {
                bool flag;
                Context context = this.currentToken.Clone();
                bool canBeAttribute = true;
                AST func = this.ParseUnaryExpression(out flag, ref canBeAttribute, false, false);
                if (canBeAttribute)
                {
                    if ((func is Lookup) || (func is Member))
                    {
                        list.Append(new Microsoft.JScript.CustomAttribute(func.context, func, new ASTList(null)));
                    }
                    else
                    {
                        list.Append(((Call) func).ToCustomAttribute());
                    }
                }
                else if (this.tokensSkipped == 0)
                {
                    this.ReportError(JSError.SyntaxError, context);
                }
                if (this.currentToken.token == JSToken.RightBracket)
                {
                    return list;
                }
                if (this.currentToken.token == JSToken.Comma)
                {
                    this.GetNextToken();
                }
                else
                {
                    this.ReportError(JSError.NoRightBracketOrComma);
                    this.SkipTokensAndThrow();
                }
            }
        }

        private DoWhile ParseDoStatement()
        {
            Context context = null;
            AST body = null;
            AST condition = null;
            this.blockType.Add(BlockType.Loop);
            try
            {
                this.GetNextToken();
                this.noSkipTokenSet.Add(NoSkipTokenSet.s_DoWhileBodyNoSkipTokenSet);
                try
                {
                    body = this.ParseStatement();
                }
                catch (RecoveryTokenException exception)
                {
                    if (exception._partiallyComputedNode != null)
                    {
                        body = exception._partiallyComputedNode;
                    }
                    else
                    {
                        body = new Block(this.CurrentPositionContext());
                    }
                    if (this.IndexOfToken(NoSkipTokenSet.s_DoWhileBodyNoSkipTokenSet, exception) == -1)
                    {
                        exception._partiallyComputedNode = new DoWhile(this.CurrentPositionContext(), body, new ConstantWrapper(false, this.CurrentPositionContext()));
                        throw exception;
                    }
                }
                finally
                {
                    this.noSkipTokenSet.Remove(NoSkipTokenSet.s_DoWhileBodyNoSkipTokenSet);
                }
                if (JSToken.While != this.currentToken.token)
                {
                    this.ReportError(JSError.NoWhile);
                }
                context = this.currentToken.Clone();
                this.GetNextToken();
                if (JSToken.LeftParen != this.currentToken.token)
                {
                    this.ReportError(JSError.NoLeftParen);
                }
                this.GetNextToken();
                this.noSkipTokenSet.Add(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet);
                try
                {
                    condition = this.ParseExpression();
                    if (JSToken.RightParen != this.currentToken.token)
                    {
                        this.ReportError(JSError.NoRightParen);
                        context.UpdateWith(condition.context);
                    }
                    else
                    {
                        context.UpdateWith(this.currentToken);
                    }
                    this.GetNextToken();
                }
                catch (RecoveryTokenException exception2)
                {
                    if (exception2._partiallyComputedNode != null)
                    {
                        condition = exception2._partiallyComputedNode;
                    }
                    else
                    {
                        condition = new ConstantWrapper(false, this.CurrentPositionContext());
                    }
                    if (this.IndexOfToken(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet, exception2) == -1)
                    {
                        exception2._partiallyComputedNode = new DoWhile(context, body, condition);
                        throw exception2;
                    }
                    if (JSToken.RightParen == this.currentToken.token)
                    {
                        this.GetNextToken();
                    }
                }
                finally
                {
                    this.noSkipTokenSet.Remove(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet);
                }
                if (JSToken.Semicolon == this.currentToken.token)
                {
                    context.UpdateWith(this.currentToken);
                    this.GetNextToken();
                }
            }
            finally
            {
                this.blockType.RemoveAt(this.blockType.Count - 1);
            }
            return new DoWhile(context, body, condition);
        }

        private AST ParseEnum(FieldAttributes visibilitySpec, Context enumCtx, CustomAttributeList customAttributes)
        {
            IdentifierLiteral name = null;
            AST ast = null;
            TypeExpression baseType = null;
            Block body = null;
            AST ast2;
            this.GetNextToken();
            if (JSToken.Identifier == this.currentToken.token)
            {
                name = new IdentifierLiteral(this.scanner.GetIdentifier(), this.currentToken.Clone());
            }
            else
            {
                this.ReportError(JSError.NoIdentifier);
                if ((JSToken.Colon != this.currentToken.token) && (JSToken.LeftCurly != this.currentToken.token))
                {
                    this.SkipTokensAndThrow();
                }
                name = new IdentifierLiteral("##Missing Enum Name##" + s_cDummyName++, this.CurrentPositionContext());
            }
            this.GetNextToken();
            if (JSToken.Colon == this.currentToken.token)
            {
                this.noSkipTokenSet.Add(NoSkipTokenSet.s_EnumBaseTypeNoSkipTokenSet);
                try
                {
                    ast = this.ParseQualifiedIdentifier(JSError.NeedType);
                }
                catch (RecoveryTokenException exception)
                {
                    if (this.IndexOfToken(NoSkipTokenSet.s_ClassExtendsNoSkipTokenSet, exception) == -1)
                    {
                        exception._partiallyComputedNode = null;
                        throw exception;
                    }
                    ast = exception._partiallyComputedNode;
                }
                finally
                {
                    this.noSkipTokenSet.Remove(NoSkipTokenSet.s_EnumBaseTypeNoSkipTokenSet);
                }
            }
            if (ast != null)
            {
                baseType = new TypeExpression(ast);
            }
            if (JSToken.LeftCurly != this.currentToken.token)
            {
                this.ReportError(JSError.NoLeftCurly);
            }
            ArrayList blockType = this.blockType;
            this.blockType = new ArrayList(0x10);
            SimpleHashtable labelTable = this.labelTable;
            this.labelTable = new SimpleHashtable(0x10);
            this.Globals.ScopeStack.Push(new ClassScope(name, ((IActivationObject) this.Globals.ScopeStack.Peek()).GetGlobalScope()));
            try
            {
                body = this.ParseClassBody(true, false);
                enumCtx.UpdateWith(body.context);
                EnumDeclaration target = new EnumDeclaration(enumCtx, name, baseType, body, visibilitySpec, customAttributes);
                if (customAttributes != null)
                {
                    customAttributes.SetTarget(target);
                }
                ast2 = target;
            }
            catch (RecoveryTokenException exception2)
            {
                enumCtx.UpdateWith(exception2._partiallyComputedNode.context);
                exception2._partiallyComputedNode = new EnumDeclaration(enumCtx, name, baseType, (Block) exception2._partiallyComputedNode, visibilitySpec, customAttributes);
                if (customAttributes != null)
                {
                    customAttributes.SetTarget(exception2._partiallyComputedNode);
                }
                throw exception2;
            }
            finally
            {
                this.Globals.ScopeStack.Pop();
                this.blockType = blockType;
                this.labelTable = labelTable;
            }
            return ast2;
        }

        private AST ParseEnumMember()
        {
            AST ast = null;
            Lookup identifier = null;
            AST ast2 = null;
            JSToken token = this.currentToken.token;
            if (token == JSToken.Var)
            {
                this.ReportError(JSError.NoVarInEnum, true);
                this.GetNextToken();
                return this.ParseEnumMember();
            }
            if (token != JSToken.Semicolon)
            {
                if (token == JSToken.Identifier)
                {
                    identifier = new Lookup(this.currentToken.Clone());
                    Context context = this.currentToken.Clone();
                    this.GetNextToken();
                    if (JSToken.Assign == this.currentToken.token)
                    {
                        this.GetNextToken();
                        ast2 = this.ParseExpression(true);
                    }
                    if (JSToken.Comma == this.currentToken.token)
                    {
                        this.GetNextToken();
                    }
                    else if (JSToken.RightCurly != this.currentToken.token)
                    {
                        this.ReportError(JSError.NoComma, true);
                    }
                    return new Constant(context, identifier, null, ast2, FieldAttributes.Public, null);
                }
                this.ReportError(JSError.SyntaxError, true);
                this.SkipTokensAndThrow();
                return ast;
            }
            this.GetNextToken();
            return this.ParseEnumMember();
        }

        public Block ParseEvalBody()
        {
            this.demandFullTrustOnFunctionCreation = true;
            return this.ParseStatements(true);
        }

        private AST ParseExpression()
        {
            bool flag;
            AST leftHandSide = this.ParseUnaryExpression(out flag, false);
            return this.ParseExpression(leftHandSide, false, flag, JSToken.None);
        }

        private AST ParseExpression(bool single)
        {
            bool flag;
            AST leftHandSide = this.ParseUnaryExpression(out flag, false);
            return this.ParseExpression(leftHandSide, single, flag, JSToken.None);
        }

        private AST ParseExpression(bool single, JSToken inToken)
        {
            bool flag;
            AST leftHandSide = this.ParseUnaryExpression(out flag, false);
            return this.ParseExpression(leftHandSide, single, flag, inToken);
        }

        private AST ParseExpression(AST leftHandSide, bool single, bool bCanAssign, JSToken inToken)
        {
            AST ast6;
            OpListItem prev = new OpListItem(JSToken.None, OpPrec.precNone, null);
            AstListItem item2 = new AstListItem(leftHandSide, null);
            AST term = null;
            try
            {
                while (true)
                {
                    if (!JSScanner.IsProcessableOperator(this.currentToken.token) || (inToken == this.currentToken.token))
                    {
                        goto Label_01E0;
                    }
                    OpPrec operatorPrecedence = JSScanner.GetOperatorPrecedence(this.currentToken.token);
                    bool flag = JSScanner.IsRightAssociativeOperator(this.currentToken.token);
                    while ((operatorPrecedence < prev._prec) || ((operatorPrecedence == prev._prec) && !flag))
                    {
                        term = this.CreateExpressionNode(prev._operator, item2._prev._term, item2._term);
                        prev = prev._prev;
                        item2 = item2._prev._prev;
                        item2 = new AstListItem(term, item2);
                    }
                    if (JSToken.ConditionalIf == this.currentToken.token)
                    {
                        AST condition = item2._term;
                        item2 = item2._prev;
                        this.GetNextToken();
                        AST ast3 = this.ParseExpression(true);
                        if (JSToken.Colon != this.currentToken.token)
                        {
                            this.ReportError(JSError.NoColon);
                        }
                        this.GetNextToken();
                        AST ast4 = this.ParseExpression(true, inToken);
                        term = new Conditional(condition.context.CombineWith(ast4.context), condition, ast3, ast4);
                        item2 = new AstListItem(term, item2);
                    }
                    else
                    {
                        if (JSScanner.IsAssignmentOperator(this.currentToken.token))
                        {
                            if (!bCanAssign)
                            {
                                this.ReportError(JSError.IllegalAssignment);
                                this.SkipTokensAndThrow();
                            }
                        }
                        else
                        {
                            bCanAssign = false;
                        }
                        prev = new OpListItem(this.currentToken.token, operatorPrecedence, prev);
                        this.GetNextToken();
                        if (bCanAssign)
                        {
                            item2 = new AstListItem(this.ParseUnaryExpression(out bCanAssign, false), item2);
                        }
                        else
                        {
                            bool flag2;
                            item2 = new AstListItem(this.ParseUnaryExpression(out flag2, false), item2);
                            flag2 = flag2;
                        }
                    }
                }
            Label_01A7:
                term = this.CreateExpressionNode(prev._operator, item2._prev._term, item2._term);
                prev = prev._prev;
                item2 = item2._prev._prev;
                item2 = new AstListItem(term, item2);
            Label_01E0:
                if (prev._operator != JSToken.None)
                {
                    goto Label_01A7;
                }
                if (!single && (JSToken.Comma == this.currentToken.token))
                {
                    this.GetNextToken();
                    AST ast5 = this.ParseExpression(false, inToken);
                    item2._term = new Comma(item2._term.context.CombineWith(ast5.context), item2._term, ast5);
                }
                ast6 = item2._term;
            }
            catch (RecoveryTokenException exception)
            {
                exception._partiallyComputedNode = leftHandSide;
                throw exception;
            }
            return ast6;
        }

        internal ScriptBlock ParseExpressionItem()
        {
            int i = this.Globals.ScopeStack.Size();
            try
            {
                Block block = new Block(this.sourceContext.Clone());
                this.GetNextToken();
                block.Append(new Expression(this.sourceContext.Clone(), this.ParseExpression()));
                return new ScriptBlock(this.sourceContext.Clone(), block);
            }
            catch (EndOfFile)
            {
            }
            catch (ScannerException exception)
            {
                this.EOFError(exception.m_errorId);
            }
            catch (StackOverflowException)
            {
                this.Globals.ScopeStack.TrimToSize(i);
                this.ReportError(JSError.OutOfStack, true);
            }
            return null;
        }

        private ASTList ParseExpressionList(JSToken terminator)
        {
            Context context = this.currentToken.Clone();
            this.scanner.GetCurrentLine();
            this.GetNextToken();
            ASTList list = new ASTList(context);
            if (terminator != this.currentToken.token)
            {
                while (true)
                {
                    this.noSkipTokenSet.Add(NoSkipTokenSet.s_ExpressionListNoSkipTokenSet);
                    try
                    {
                        if (JSToken.BitwiseAnd == this.currentToken.token)
                        {
                            Context context2 = this.currentToken.Clone();
                            this.GetNextToken();
                            AST operand = this.ParseLeftHandSideExpression();
                            if ((operand is Member) || (operand is Lookup))
                            {
                                context2.UpdateWith(operand.context);
                                list.Append(new AddressOf(context2, operand));
                            }
                            else
                            {
                                this.ReportError(JSError.DoesNotHaveAnAddress, context2.Clone());
                                list.Append(operand);
                            }
                        }
                        else if (JSToken.Comma == this.currentToken.token)
                        {
                            list.Append(new ConstantWrapper(System.Reflection.Missing.Value, this.currentToken.Clone()));
                        }
                        else
                        {
                            if (terminator == this.currentToken.token)
                            {
                                break;
                            }
                            list.Append(this.ParseExpression(true));
                        }
                        if (terminator == this.currentToken.token)
                        {
                            break;
                        }
                        if (JSToken.Comma != this.currentToken.token)
                        {
                            if (terminator == JSToken.RightParen)
                            {
                                if ((JSToken.Semicolon == this.currentToken.token) && (JSToken.RightParen == this.scanner.PeekToken()))
                                {
                                    this.ReportError(JSError.UnexpectedSemicolon, true);
                                    this.GetNextToken();
                                    break;
                                }
                                this.ReportError(JSError.NoRightParenOrComma);
                            }
                            else
                            {
                                this.ReportError(JSError.NoRightBracketOrComma);
                            }
                            this.SkipTokensAndThrow();
                        }
                    }
                    catch (RecoveryTokenException exception)
                    {
                        if (exception._partiallyComputedNode != null)
                        {
                            list.Append(exception._partiallyComputedNode);
                        }
                        if (this.IndexOfToken(NoSkipTokenSet.s_ExpressionListNoSkipTokenSet, exception) == -1)
                        {
                            exception._partiallyComputedNode = list;
                            throw exception;
                        }
                    }
                    finally
                    {
                        this.noSkipTokenSet.Remove(NoSkipTokenSet.s_ExpressionListNoSkipTokenSet);
                    }
                    this.GetNextToken();
                }
            }
            context.UpdateWith(this.currentToken);
            return list;
        }

        private AST ParseForStatement()
        {
            this.blockType.Add(BlockType.Loop);
            AST ast = null;
            try
            {
                Context context = this.currentToken.Clone();
                this.GetNextToken();
                if (JSToken.LeftParen != this.currentToken.token)
                {
                    this.ReportError(JSError.NoLeftParen);
                }
                this.GetNextToken();
                bool flag = false;
                bool flag2 = false;
                AST var = null;
                AST ast3 = null;
                AST collection = null;
                AST incrementer = null;
                try
                {
                    if (JSToken.Var == this.currentToken.token)
                    {
                        flag = true;
                        ast3 = this.ParseIdentifierInitializer(JSToken.In, FieldAttributes.PrivateScope, null, JSToken.Var);
                        AST ast6 = null;
                        while (JSToken.Comma == this.currentToken.token)
                        {
                            flag = false;
                            ast6 = this.ParseIdentifierInitializer(JSToken.In, FieldAttributes.PrivateScope, null, JSToken.Var);
                            ast3 = new Comma(ast3.context.CombineWith(ast6.context), ast3, ast6);
                        }
                        if (flag)
                        {
                            if (JSToken.In == this.currentToken.token)
                            {
                                this.GetNextToken();
                                collection = this.ParseExpression();
                            }
                            else
                            {
                                flag = false;
                            }
                        }
                    }
                    else if (JSToken.Semicolon != this.currentToken.token)
                    {
                        bool flag3;
                        ast3 = this.ParseUnaryExpression(out flag3, false);
                        if (flag3 && (JSToken.In == this.currentToken.token))
                        {
                            flag = true;
                            var = ast3;
                            ast3 = null;
                            this.GetNextToken();
                            this.noSkipTokenSet.Add(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet);
                            try
                            {
                                try
                                {
                                    collection = this.ParseExpression();
                                }
                                catch (RecoveryTokenException exception)
                                {
                                    if (this.IndexOfToken(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet, exception) == -1)
                                    {
                                        exception._partiallyComputedNode = null;
                                        throw exception;
                                    }
                                    if (exception._partiallyComputedNode == null)
                                    {
                                        collection = new ConstantWrapper(true, this.CurrentPositionContext());
                                    }
                                    else
                                    {
                                        collection = exception._partiallyComputedNode;
                                    }
                                    if (exception._token == JSToken.RightParen)
                                    {
                                        this.GetNextToken();
                                        flag2 = true;
                                    }
                                }
                                goto Label_01E1;
                            }
                            finally
                            {
                                this.noSkipTokenSet.Remove(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet);
                            }
                        }
                        ast3 = this.ParseExpression(ast3, false, flag3, JSToken.In);
                    }
                    else
                    {
                        ast3 = new EmptyLiteral(this.CurrentPositionContext());
                    }
                }
                catch (RecoveryTokenException exception2)
                {
                    exception2._partiallyComputedNode = null;
                    throw exception2;
                }
            Label_01E1:
                if (flag)
                {
                    if (!flag2)
                    {
                        if (JSToken.RightParen != this.currentToken.token)
                        {
                            this.ReportError(JSError.NoRightParen);
                        }
                        context.UpdateWith(this.currentToken);
                        this.GetNextToken();
                    }
                    AST ast7 = null;
                    try
                    {
                        ast7 = this.ParseStatement();
                    }
                    catch (RecoveryTokenException exception3)
                    {
                        if (exception3._partiallyComputedNode == null)
                        {
                            ast7 = new Block(this.CurrentPositionContext());
                        }
                        else
                        {
                            ast7 = exception3._partiallyComputedNode;
                        }
                        exception3._partiallyComputedNode = new ForIn(context, var, ast3, collection, ast7);
                        throw exception3;
                    }
                    return new ForIn(context, var, ast3, collection, ast7);
                }
                this.noSkipTokenSet.Add(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet);
                try
                {
                    if (JSToken.Semicolon != this.currentToken.token)
                    {
                        this.ReportError(JSError.NoSemicolon);
                        if (JSToken.Colon == this.currentToken.token)
                        {
                            this.noSkipTokenSet.Add(NoSkipTokenSet.s_VariableDeclNoSkipTokenSet);
                            try
                            {
                                this.SkipTokensAndThrow();
                            }
                            catch (RecoveryTokenException exception4)
                            {
                                if (JSToken.Semicolon != this.currentToken.token)
                                {
                                    throw exception4;
                                }
                                this.errorToken = null;
                            }
                            finally
                            {
                                this.noSkipTokenSet.Remove(NoSkipTokenSet.s_VariableDeclNoSkipTokenSet);
                            }
                        }
                    }
                    this.GetNextToken();
                    if (JSToken.Semicolon != this.currentToken.token)
                    {
                        collection = this.ParseExpression();
                        if (JSToken.Semicolon != this.currentToken.token)
                        {
                            this.ReportError(JSError.NoSemicolon);
                        }
                    }
                    else
                    {
                        collection = new ConstantWrapper(true, this.CurrentPositionContext());
                    }
                    this.GetNextToken();
                    if (JSToken.RightParen != this.currentToken.token)
                    {
                        incrementer = this.ParseExpression();
                    }
                    else
                    {
                        incrementer = new EmptyLiteral(this.CurrentPositionContext());
                    }
                    if (JSToken.RightParen != this.currentToken.token)
                    {
                        this.ReportError(JSError.NoRightParen);
                    }
                    context.UpdateWith(this.currentToken);
                    this.GetNextToken();
                }
                catch (RecoveryTokenException exception5)
                {
                    if (this.IndexOfToken(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet, exception5) == -1)
                    {
                        exception5._partiallyComputedNode = null;
                        throw exception5;
                    }
                    exception5._partiallyComputedNode = null;
                    if (collection == null)
                    {
                        collection = new ConstantWrapper(true, this.CurrentPositionContext());
                    }
                    if (incrementer == null)
                    {
                        incrementer = new EmptyLiteral(this.CurrentPositionContext());
                    }
                    if (exception5._token == JSToken.RightParen)
                    {
                        this.GetNextToken();
                    }
                }
                finally
                {
                    this.noSkipTokenSet.Remove(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet);
                }
                AST body = null;
                try
                {
                    body = this.ParseStatement();
                }
                catch (RecoveryTokenException exception6)
                {
                    if (exception6._partiallyComputedNode == null)
                    {
                        body = new Block(this.CurrentPositionContext());
                    }
                    else
                    {
                        body = exception6._partiallyComputedNode;
                    }
                    exception6._partiallyComputedNode = new For(context, ast3, collection, incrementer, body);
                    throw exception6;
                }
                ast = new For(context, ast3, collection, incrementer, body);
            }
            finally
            {
                this.blockType.RemoveAt(this.blockType.Count - 1);
            }
            return ast;
        }

        private AST ParseFunction(FieldAttributes visibilitySpec, bool inExpression, Context fncCtx, bool isMethod, bool isAbstract, bool isFinal, bool isInterface, CustomAttributeList customAttributes)
        {
            return this.ParseFunction(visibilitySpec, inExpression, fncCtx, isMethod, isAbstract, isFinal, isInterface, customAttributes, null);
        }

        private AST ParseFunction(FieldAttributes visibilitySpec, bool inExpression, Context fncCtx, bool isMethod, bool isAbstract, bool isFinal, bool isInterface, CustomAttributeList customAttributes, Call function)
        {
            AST ast2;
            if (this.demandFullTrustOnFunctionCreation)
            {
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
            }
            IdentifierLiteral id = null;
            AST rootObject = null;
            ArrayList parameters = null;
            TypeExpression expression = null;
            Block body = null;
            bool isGetter = false;
            bool isSetter = false;
            if (function == null)
            {
                this.GetNextToken();
                if (isMethod)
                {
                    if (JSToken.Get == this.currentToken.token)
                    {
                        isGetter = true;
                        this.GetNextToken();
                    }
                    else if (JSToken.Set == this.currentToken.token)
                    {
                        isSetter = true;
                        this.GetNextToken();
                    }
                }
                if (JSToken.Identifier == this.currentToken.token)
                {
                    id = new IdentifierLiteral(this.scanner.GetIdentifier(), this.currentToken.Clone());
                    this.GetNextToken();
                    if (JSToken.AccessField == this.currentToken.token)
                    {
                        if (isInterface)
                        {
                            this.ReportError(JSError.SyntaxError, true);
                        }
                        this.GetNextToken();
                        if (JSToken.Identifier == this.currentToken.token)
                        {
                            rootObject = new Lookup(id.context);
                            id = new IdentifierLiteral(this.scanner.GetIdentifier(), this.currentToken.Clone());
                            this.GetNextToken();
                            while (JSToken.AccessField == this.currentToken.token)
                            {
                                this.GetNextToken();
                                if (JSToken.Identifier == this.currentToken.token)
                                {
                                    rootObject = new Member(rootObject.context.CombineWith(this.currentToken), rootObject, new ConstantWrapper(id.ToString(), id.context));
                                    id = new IdentifierLiteral(this.scanner.GetIdentifier(), this.currentToken.Clone());
                                    this.GetNextToken();
                                }
                                else
                                {
                                    this.ReportError(JSError.NoIdentifier, true);
                                }
                            }
                        }
                        else
                        {
                            this.ReportError(JSError.NoIdentifier, true);
                        }
                    }
                }
                else
                {
                    string identifier = JSKeyword.CanBeIdentifier(this.currentToken.token);
                    if (identifier != null)
                    {
                        this.ForceReportInfo(JSError.KeywordUsedAsIdentifier, isMethod);
                        id = new IdentifierLiteral(identifier, this.currentToken.Clone());
                        this.GetNextToken();
                    }
                    else
                    {
                        if (!inExpression)
                        {
                            identifier = this.currentToken.GetCode();
                            this.ReportError(JSError.NoIdentifier, true);
                            this.GetNextToken();
                        }
                        else
                        {
                            identifier = "";
                        }
                        id = new IdentifierLiteral(identifier, this.CurrentPositionContext());
                    }
                }
            }
            else
            {
                id = function.GetName();
            }
            ArrayList blockType = this.blockType;
            this.blockType = new ArrayList(0x10);
            SimpleHashtable labelTable = this.labelTable;
            this.labelTable = new SimpleHashtable(0x10);
            FunctionScope item = new FunctionScope(this.Globals.ScopeStack.Peek(), isMethod);
            this.Globals.ScopeStack.Push(item);
            try
            {
                parameters = new ArrayList();
                Context context = null;
                if (function == null)
                {
                    if (JSToken.LeftParen != this.currentToken.token)
                    {
                        this.ReportError(JSError.NoLeftParen);
                    }
                    this.GetNextToken();
                    while (JSToken.RightParen != this.currentToken.token)
                    {
                        if (context != null)
                        {
                            this.ReportError(JSError.ParamListNotLast, context, true);
                            context = null;
                        }
                        string str2 = null;
                        TypeExpression type = null;
                        this.noSkipTokenSet.Add(NoSkipTokenSet.s_FunctionDeclNoSkipTokenSet);
                        try
                        {
                            try
                            {
                                if (JSToken.ParamArray == this.currentToken.token)
                                {
                                    context = this.currentToken.Clone();
                                    this.GetNextToken();
                                }
                                if ((JSToken.Identifier != this.currentToken.token) && ((str2 = JSKeyword.CanBeIdentifier(this.currentToken.token)) == null))
                                {
                                    if (JSToken.LeftCurly == this.currentToken.token)
                                    {
                                        this.ReportError(JSError.NoRightParen);
                                        break;
                                    }
                                    if (JSToken.Comma == this.currentToken.token)
                                    {
                                        this.ReportError(JSError.SyntaxError, true);
                                    }
                                    else
                                    {
                                        this.ReportError(JSError.SyntaxError, true);
                                        this.SkipTokensAndThrow();
                                    }
                                }
                                else
                                {
                                    if (str2 == null)
                                    {
                                        str2 = this.scanner.GetIdentifier();
                                    }
                                    else
                                    {
                                        this.ForceReportInfo(JSError.KeywordUsedAsIdentifier);
                                    }
                                    Context context2 = this.currentToken.Clone();
                                    this.GetNextToken();
                                    if (JSToken.Colon == this.currentToken.token)
                                    {
                                        type = this.ParseTypeExpression();
                                        if (type != null)
                                        {
                                            context2.UpdateWith(type.context);
                                        }
                                    }
                                    CustomAttributeList list3 = null;
                                    if (context != null)
                                    {
                                        list3 = new CustomAttributeList(context);
                                        list3.Append(new Microsoft.JScript.CustomAttribute(context, new Lookup("...", context), new ASTList(null)));
                                    }
                                    parameters.Add(new ParameterDeclaration(context2, str2, type, list3));
                                }
                                if (JSToken.RightParen == this.currentToken.token)
                                {
                                    break;
                                }
                                if (JSToken.Comma != this.currentToken.token)
                                {
                                    if (JSToken.LeftCurly == this.currentToken.token)
                                    {
                                        this.ReportError(JSError.NoRightParen);
                                        break;
                                    }
                                    if ((JSToken.Identifier == this.currentToken.token) && (type == null))
                                    {
                                        this.ReportError(JSError.NoCommaOrTypeDefinitionError);
                                    }
                                    else
                                    {
                                        this.ReportError(JSError.NoComma);
                                    }
                                }
                                this.GetNextToken();
                            }
                            catch (RecoveryTokenException exception)
                            {
                                if (this.IndexOfToken(NoSkipTokenSet.s_FunctionDeclNoSkipTokenSet, exception) == -1)
                                {
                                    throw exception;
                                }
                            }
                            continue;
                        }
                        finally
                        {
                            this.noSkipTokenSet.Remove(NoSkipTokenSet.s_FunctionDeclNoSkipTokenSet);
                        }
                    }
                    fncCtx.UpdateWith(this.currentToken);
                    if (isGetter && (parameters.Count != 0))
                    {
                        this.ReportError(JSError.BadPropertyDeclaration, true);
                        isGetter = false;
                    }
                    else if (isSetter && (parameters.Count != 1))
                    {
                        this.ReportError(JSError.BadPropertyDeclaration, true);
                        isSetter = false;
                    }
                    this.GetNextToken();
                    if (JSToken.Colon == this.currentToken.token)
                    {
                        if (isSetter)
                        {
                            this.ReportError(JSError.SyntaxError);
                        }
                        this.noSkipTokenSet.Add(NoSkipTokenSet.s_StartBlockNoSkipTokenSet);
                        try
                        {
                            expression = this.ParseTypeExpression();
                        }
                        catch (RecoveryTokenException exception2)
                        {
                            if (this.IndexOfToken(NoSkipTokenSet.s_StartBlockNoSkipTokenSet, exception2) == -1)
                            {
                                exception2._partiallyComputedNode = null;
                                throw exception2;
                            }
                            if (exception2._partiallyComputedNode != null)
                            {
                                expression = (TypeExpression) exception2._partiallyComputedNode;
                            }
                        }
                        finally
                        {
                            this.noSkipTokenSet.Remove(NoSkipTokenSet.s_StartBlockNoSkipTokenSet);
                        }
                        if (isSetter)
                        {
                            expression = null;
                        }
                    }
                }
                else
                {
                    function.GetParameters(parameters);
                }
                if ((JSToken.LeftCurly != this.currentToken.token) && (isAbstract || (isMethod && this.GuessIfAbstract())))
                {
                    if (!isAbstract)
                    {
                        isAbstract = true;
                        this.ReportError(JSError.ShouldBeAbstract, fncCtx, true);
                    }
                    body = new Block(this.currentToken.Clone());
                }
                else
                {
                    if (JSToken.LeftCurly != this.currentToken.token)
                    {
                        this.ReportError(JSError.NoLeftCurly, true);
                    }
                    else if (isAbstract)
                    {
                        this.ReportError(JSError.AbstractWithBody, fncCtx, true);
                    }
                    this.blockType.Add(BlockType.Block);
                    this.noSkipTokenSet.Add(NoSkipTokenSet.s_BlockNoSkipTokenSet);
                    this.noSkipTokenSet.Add(NoSkipTokenSet.s_StartStatementNoSkipTokenSet);
                    try
                    {
                        body = new Block(this.currentToken.Clone());
                        this.GetNextToken();
                        while (JSToken.RightCurly != this.currentToken.token)
                        {
                            try
                            {
                                body.Append(this.ParseStatement());
                                continue;
                            }
                            catch (RecoveryTokenException exception3)
                            {
                                if (exception3._partiallyComputedNode != null)
                                {
                                    body.Append(exception3._partiallyComputedNode);
                                }
                                if (this.IndexOfToken(NoSkipTokenSet.s_StartStatementNoSkipTokenSet, exception3) == -1)
                                {
                                    throw exception3;
                                }
                                continue;
                            }
                        }
                        body.context.UpdateWith(this.currentToken);
                        fncCtx.UpdateWith(this.currentToken);
                    }
                    catch (RecoveryTokenException exception4)
                    {
                        if (this.IndexOfToken(NoSkipTokenSet.s_BlockNoSkipTokenSet, exception4) == -1)
                        {
                            this.Globals.ScopeStack.Pop();
                            try
                            {
                                ParameterDeclaration[] declarationArray = new ParameterDeclaration[parameters.Count];
                                parameters.CopyTo(declarationArray);
                                if (inExpression)
                                {
                                    exception4._partiallyComputedNode = new FunctionExpression(fncCtx, id, declarationArray, expression, body, item, visibilitySpec);
                                }
                                else
                                {
                                    exception4._partiallyComputedNode = new FunctionDeclaration(fncCtx, rootObject, id, declarationArray, expression, body, item, visibilitySpec, isMethod, isGetter, isSetter, isAbstract, isFinal, customAttributes);
                                }
                                if (customAttributes != null)
                                {
                                    customAttributes.SetTarget(exception4._partiallyComputedNode);
                                }
                            }
                            finally
                            {
                                this.Globals.ScopeStack.Push(item);
                            }
                            throw exception4;
                        }
                    }
                    finally
                    {
                        this.blockType.RemoveAt(this.blockType.Count - 1);
                        this.noSkipTokenSet.Remove(NoSkipTokenSet.s_StartStatementNoSkipTokenSet);
                        this.noSkipTokenSet.Remove(NoSkipTokenSet.s_BlockNoSkipTokenSet);
                    }
                    this.GetNextToken();
                }
            }
            finally
            {
                this.blockType = blockType;
                this.labelTable = labelTable;
                this.Globals.ScopeStack.Pop();
            }
            ParameterDeclaration[] array = new ParameterDeclaration[parameters.Count];
            parameters.CopyTo(array);
            if (inExpression)
            {
                ast2 = new FunctionExpression(fncCtx, id, array, expression, body, item, visibilitySpec);
            }
            else
            {
                ast2 = new FunctionDeclaration(fncCtx, rootObject, id, array, expression, body, item, visibilitySpec, isMethod, isGetter, isSetter, isAbstract, isFinal, customAttributes);
            }
            if (customAttributes != null)
            {
                customAttributes.SetTarget(ast2);
            }
            return ast2;
        }

        internal AST ParseFunctionExpression()
        {
            this.demandFullTrustOnFunctionCreation = true;
            this.GetNextToken();
            return this.ParseFunction(FieldAttributes.PrivateScope, true, this.currentToken.Clone(), false, false, false, false, null);
        }

        private AST ParseIdentifierInitializer(JSToken inToken, FieldAttributes visibility, CustomAttributeList customAttributes, JSToken kind)
        {
            Lookup identifier = null;
            TypeExpression type = null;
            AST initializer = null;
            RecoveryTokenException exception = null;
            this.GetNextToken();
            if (JSToken.Identifier != this.currentToken.token)
            {
                string name = JSKeyword.CanBeIdentifier(this.currentToken.token);
                if (name != null)
                {
                    this.ForceReportInfo(JSError.KeywordUsedAsIdentifier);
                    identifier = new Lookup(name, this.currentToken.Clone());
                }
                else
                {
                    this.ReportError(JSError.NoIdentifier);
                    identifier = new Lookup("#_Missing Identifier_#" + s_cDummyName++, this.CurrentPositionContext());
                }
            }
            else
            {
                identifier = new Lookup(this.scanner.GetIdentifier(), this.currentToken.Clone());
            }
            this.GetNextToken();
            Context context = identifier.context.Clone();
            this.noSkipTokenSet.Add(NoSkipTokenSet.s_VariableDeclNoSkipTokenSet);
            try
            {
                if (JSToken.Colon == this.currentToken.token)
                {
                    try
                    {
                        type = this.ParseTypeExpression();
                    }
                    catch (RecoveryTokenException exception2)
                    {
                        type = (TypeExpression) exception2._partiallyComputedNode;
                        throw exception2;
                    }
                    finally
                    {
                        if (type != null)
                        {
                            context.UpdateWith(type.context);
                        }
                    }
                }
                if ((JSToken.Assign == this.currentToken.token) || (JSToken.Equal == this.currentToken.token))
                {
                    if (JSToken.Equal == this.currentToken.token)
                    {
                        this.ReportError(JSError.NoEqual, true);
                    }
                    this.GetNextToken();
                    try
                    {
                        initializer = this.ParseExpression(true, inToken);
                    }
                    catch (RecoveryTokenException exception3)
                    {
                        initializer = exception3._partiallyComputedNode;
                        throw exception3;
                    }
                    finally
                    {
                        if (initializer != null)
                        {
                            context.UpdateWith(initializer.context);
                        }
                    }
                }
            }
            catch (RecoveryTokenException exception4)
            {
                if (this.IndexOfToken(NoSkipTokenSet.s_VariableDeclNoSkipTokenSet, exception4) == -1)
                {
                    exception = exception4;
                }
            }
            finally
            {
                this.noSkipTokenSet.Remove(NoSkipTokenSet.s_VariableDeclNoSkipTokenSet);
            }
            AST target = null;
            if (JSToken.Var == kind)
            {
                target = new VariableDeclaration(context, identifier, type, initializer, visibility, customAttributes);
            }
            else
            {
                if (initializer == null)
                {
                    this.ForceReportInfo(JSError.NoEqual);
                }
                target = new Constant(context, identifier, type, initializer, visibility, customAttributes);
            }
            if (customAttributes != null)
            {
                customAttributes.SetTarget(target);
            }
            if (exception != null)
            {
                exception._partiallyComputedNode = target;
                throw exception;
            }
            return target;
        }

        private If ParseIfStatement()
        {
            Context context = this.currentToken.Clone();
            AST condition = null;
            AST ast2 = null;
            AST ast3 = null;
            this.blockType.Add(BlockType.Block);
            try
            {
                this.GetNextToken();
                this.noSkipTokenSet.Add(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet);
                try
                {
                    if (JSToken.LeftParen != this.currentToken.token)
                    {
                        this.ReportError(JSError.NoLeftParen);
                    }
                    this.GetNextToken();
                    condition = this.ParseExpression();
                    if (JSToken.RightParen != this.currentToken.token)
                    {
                        context.UpdateWith(condition.context);
                        this.ReportError(JSError.NoRightParen);
                    }
                    else
                    {
                        context.UpdateWith(this.currentToken);
                    }
                    this.GetNextToken();
                }
                catch (RecoveryTokenException exception)
                {
                    if (exception._partiallyComputedNode != null)
                    {
                        condition = exception._partiallyComputedNode;
                    }
                    else
                    {
                        condition = new ConstantWrapper(true, this.CurrentPositionContext());
                    }
                    if (this.IndexOfToken(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet, exception) == -1)
                    {
                        exception._partiallyComputedNode = null;
                        throw exception;
                    }
                    if (exception._token == JSToken.RightParen)
                    {
                        this.GetNextToken();
                    }
                }
                finally
                {
                    this.noSkipTokenSet.Remove(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet);
                }
                if (condition is Assign)
                {
                    condition.context.HandleError(JSError.SuspectAssignment);
                }
                if (JSToken.Semicolon == this.currentToken.token)
                {
                    this.ForceReportInfo(JSError.SuspectSemicolon);
                }
                this.noSkipTokenSet.Add(NoSkipTokenSet.s_IfBodyNoSkipTokenSet);
                try
                {
                    ast2 = this.ParseStatement();
                }
                catch (RecoveryTokenException exception2)
                {
                    if (exception2._partiallyComputedNode != null)
                    {
                        ast2 = exception2._partiallyComputedNode;
                    }
                    else
                    {
                        ast2 = new Block(this.CurrentPositionContext());
                    }
                    if (this.IndexOfToken(NoSkipTokenSet.s_IfBodyNoSkipTokenSet, exception2) == -1)
                    {
                        exception2._partiallyComputedNode = new If(context, condition, ast2, ast3);
                        throw exception2;
                    }
                }
                finally
                {
                    this.noSkipTokenSet.Remove(NoSkipTokenSet.s_IfBodyNoSkipTokenSet);
                }
                if (JSToken.Else == this.currentToken.token)
                {
                    this.GetNextToken();
                    if (JSToken.Semicolon == this.currentToken.token)
                    {
                        this.ForceReportInfo(JSError.SuspectSemicolon);
                    }
                    try
                    {
                        ast3 = this.ParseStatement();
                    }
                    catch (RecoveryTokenException exception3)
                    {
                        if (exception3._partiallyComputedNode != null)
                        {
                            ast3 = exception3._partiallyComputedNode;
                        }
                        else
                        {
                            ast3 = new Block(this.CurrentPositionContext());
                        }
                        exception3._partiallyComputedNode = new If(context, condition, ast2, ast3);
                        throw exception3;
                    }
                }
            }
            finally
            {
                this.blockType.RemoveAt(this.blockType.Count - 1);
            }
            return new If(context, condition, ast2, ast3);
        }

        private Import ParseImportStatement()
        {
            Context context = this.currentToken.Clone();
            AST name = null;
            try
            {
                name = this.ParseQualifiedIdentifier(JSError.PackageExpected);
            }
            catch (RecoveryTokenException exception)
            {
                if (exception._partiallyComputedNode != null)
                {
                    exception._partiallyComputedNode = new Import(context, exception._partiallyComputedNode);
                }
            }
            if ((this.currentToken.token != JSToken.Semicolon) && !this.scanner.GotEndOfLine())
            {
                this.ReportError(JSError.NoSemicolon, this.currentToken.Clone());
            }
            return new Import(context, name);
        }

        private AST ParseLeftHandSideExpression()
        {
            return this.ParseLeftHandSideExpression(false);
        }

        private AST ParseLeftHandSideExpression(bool isMinus)
        {
            bool canBeAttribute = false;
            return this.ParseLeftHandSideExpression(isMinus, ref canBeAttribute, false);
        }

        private AST ParseLeftHandSideExpression(bool isMinus, ref bool canBeAttribute, bool warnForKeyword)
        {
            AST expression = null;
            Context context;
            ASTList list2;
            Context context2;
            ASTList list3;
            string str6;
            bool flag = false;
            ArrayList newContexts = null;
            while (JSToken.New == this.currentToken.token)
            {
                if (newContexts == null)
                {
                    newContexts = new ArrayList(4);
                }
                newContexts.Add(this.currentToken.Clone());
                this.GetNextToken();
            }
            JSToken token = this.currentToken.token;
            if (token <= JSToken.Divide)
            {
                switch (token)
                {
                    case JSToken.Function:
                        canBeAttribute = false;
                        expression = this.ParseFunction(FieldAttributes.PrivateScope, true, this.currentToken.Clone(), false, false, false, false, null);
                        flag = true;
                        goto Label_0956;

                    case JSToken.LeftCurly:
                    {
                        AST ast2;
                        canBeAttribute = false;
                        context2 = this.currentToken.Clone();
                        this.GetNextToken();
                        list3 = new ASTList(this.currentToken.Clone());
                        if (JSToken.RightCurly == this.currentToken.token)
                        {
                            goto Label_0830;
                        }
                    Label_05C9:
                        ast2 = null;
                        AST elem = null;
                        if (JSToken.Identifier == this.currentToken.token)
                        {
                            ast2 = new ConstantWrapper(this.scanner.GetIdentifier(), this.currentToken.Clone());
                        }
                        else if (JSToken.StringLiteral == this.currentToken.token)
                        {
                            ast2 = new ConstantWrapper(this.scanner.GetStringLiteral(), this.currentToken.Clone());
                        }
                        else if ((JSToken.IntegerLiteral == this.currentToken.token) || (JSToken.NumericLiteral == this.currentToken.token))
                        {
                            ast2 = new ConstantWrapper(Microsoft.JScript.Convert.ToNumber(this.currentToken.GetCode(), true, true, Microsoft.JScript.Missing.Value), this.currentToken.Clone());
                            ((ConstantWrapper) ast2).isNumericLiteral = true;
                        }
                        else
                        {
                            this.ReportError(JSError.NoMemberIdentifier);
                            ast2 = new IdentifierLiteral("_#Missing_Field#_" + s_cDummyName++, this.CurrentPositionContext());
                        }
                        ASTList list4 = new ASTList(ast2.context.Clone());
                        this.GetNextToken();
                        this.noSkipTokenSet.Add(NoSkipTokenSet.s_ObjectInitNoSkipTokenSet);
                        try
                        {
                            try
                            {
                                if (JSToken.Colon != this.currentToken.token)
                                {
                                    this.ReportError(JSError.NoColon, true);
                                    elem = this.ParseExpression(true);
                                }
                                else
                                {
                                    this.GetNextToken();
                                    elem = this.ParseExpression(true);
                                }
                                list4.Append(ast2);
                                list4.Append(elem);
                                list3.Append(list4);
                                if (JSToken.RightCurly == this.currentToken.token)
                                {
                                    goto Label_0830;
                                }
                                if (JSToken.Comma == this.currentToken.token)
                                {
                                    this.GetNextToken();
                                }
                                else
                                {
                                    if (this.scanner.GotEndOfLine())
                                    {
                                        this.ReportError(JSError.NoRightCurly);
                                    }
                                    else
                                    {
                                        this.ReportError(JSError.NoComma, true);
                                    }
                                    this.SkipTokensAndThrow();
                                }
                            }
                            catch (RecoveryTokenException exception4)
                            {
                                if (exception4._partiallyComputedNode != null)
                                {
                                    elem = exception4._partiallyComputedNode;
                                    list4.Append(ast2);
                                    list4.Append(elem);
                                    list3.Append(list4);
                                }
                                if (this.IndexOfToken(NoSkipTokenSet.s_ObjectInitNoSkipTokenSet, exception4) == -1)
                                {
                                    exception4._partiallyComputedNode = new ObjectLiteral(context2, list3);
                                    throw exception4;
                                }
                                if (JSToken.Comma == this.currentToken.token)
                                {
                                    this.GetNextToken();
                                }
                                if (JSToken.RightCurly == this.currentToken.token)
                                {
                                    goto Label_0830;
                                }
                            }
                            goto Label_05C9;
                        }
                        finally
                        {
                            this.noSkipTokenSet.Remove(NoSkipTokenSet.s_ObjectInitNoSkipTokenSet);
                        }
                        goto Label_0830;
                    }
                    case JSToken.Null:
                        canBeAttribute = false;
                        expression = new NullLiteral(this.currentToken.Clone());
                        goto Label_0956;

                    case JSToken.True:
                        canBeAttribute = false;
                        expression = new ConstantWrapper(true, this.currentToken.Clone());
                        goto Label_0956;

                    case JSToken.False:
                        canBeAttribute = false;
                        expression = new ConstantWrapper(false, this.currentToken.Clone());
                        goto Label_0956;

                    case JSToken.This:
                        canBeAttribute = false;
                        expression = new ThisLiteral(this.currentToken.Clone(), false);
                        goto Label_0956;

                    case JSToken.Identifier:
                        expression = new Lookup(this.scanner.GetIdentifier(), this.currentToken.Clone());
                        goto Label_0956;

                    case JSToken.StringLiteral:
                        canBeAttribute = false;
                        expression = new ConstantWrapper(this.scanner.GetStringLiteral(), this.currentToken.Clone());
                        goto Label_0956;

                    case JSToken.IntegerLiteral:
                    {
                        canBeAttribute = false;
                        object obj2 = Microsoft.JScript.Convert.LiteralToNumber(this.currentToken.GetCode(), this.currentToken);
                        if (obj2 == null)
                        {
                            obj2 = 0;
                        }
                        expression = new ConstantWrapper(obj2, this.currentToken.Clone());
                        ((ConstantWrapper) expression).isNumericLiteral = true;
                        goto Label_0956;
                    }
                    case JSToken.NumericLiteral:
                    {
                        canBeAttribute = false;
                        string str = isMinus ? ("-" + this.currentToken.GetCode()) : this.currentToken.GetCode();
                        expression = new ConstantWrapper(Microsoft.JScript.Convert.ToNumber(str, false, false, Microsoft.JScript.Missing.Value), this.currentToken.Clone());
                        ((ConstantWrapper) expression).isNumericLiteral = true;
                        goto Label_0956;
                    }
                    case JSToken.LeftParen:
                        canBeAttribute = false;
                        this.GetNextToken();
                        this.noSkipTokenSet.Add(NoSkipTokenSet.s_ParenExpressionNoSkipToken);
                        try
                        {
                            expression = this.ParseExpression();
                            if (JSToken.RightParen != this.currentToken.token)
                            {
                                this.ReportError(JSError.NoRightParen);
                            }
                        }
                        catch (RecoveryTokenException exception)
                        {
                            if (this.IndexOfToken(NoSkipTokenSet.s_ParenExpressionNoSkipToken, exception) == -1)
                            {
                                throw exception;
                            }
                            expression = exception._partiallyComputedNode;
                        }
                        finally
                        {
                            this.noSkipTokenSet.Remove(NoSkipTokenSet.s_ParenExpressionNoSkipToken);
                        }
                        if (expression == null)
                        {
                            this.SkipTokensAndThrow();
                        }
                        goto Label_0956;

                    case JSToken.LeftBracket:
                        canBeAttribute = false;
                        context = this.currentToken.Clone();
                        list2 = new ASTList(this.currentToken.Clone());
                        this.GetNextToken();
                        if ((this.currentToken.token != JSToken.Identifier) || (this.scanner.PeekToken() != JSToken.Colon))
                        {
                            goto Label_0561;
                        }
                        this.noSkipTokenSet.Add(NoSkipTokenSet.s_BracketToken);
                        try
                        {
                            try
                            {
                                if (this.currentToken.GetCode() == "assembly")
                                {
                                    this.GetNextToken();
                                    this.GetNextToken();
                                    return new AssemblyCustomAttributeList(this.ParseCustomAttributeList());
                                }
                                this.ReportError(JSError.ExpectedAssembly);
                                this.SkipTokensAndThrow();
                            }
                            catch (RecoveryTokenException exception2)
                            {
                                exception2._partiallyComputedNode = new Block(context);
                                return exception2._partiallyComputedNode;
                            }
                            goto Label_0561;
                        }
                        finally
                        {
                            if (this.currentToken.token == JSToken.RightBracket)
                            {
                                this.errorToken = null;
                                this.GetNextToken();
                            }
                            this.noSkipTokenSet.Remove(NoSkipTokenSet.s_BracketToken);
                        }
                        goto Label_046D;

                    case JSToken.Divide:
                    {
                        canBeAttribute = false;
                        string pattern = this.scanner.ScanRegExp();
                        if (pattern == null)
                        {
                            break;
                        }
                        bool flag2 = false;
                        try
                        {
                            new Regex(pattern, RegexOptions.ECMAScript);
                        }
                        catch (ArgumentException)
                        {
                            pattern = "";
                            flag2 = true;
                        }
                        string flags = this.scanner.ScanRegExpFlags();
                        if (flags == null)
                        {
                            expression = new RegExpLiteral(pattern, null, this.currentToken.Clone());
                        }
                        else
                        {
                            try
                            {
                                expression = new RegExpLiteral(pattern, flags, this.currentToken.Clone());
                            }
                            catch (JScriptException)
                            {
                                expression = new RegExpLiteral(pattern, null, this.currentToken.Clone());
                                flag2 = true;
                            }
                        }
                        if (flag2)
                        {
                            this.ReportError(JSError.RegExpSyntax, true);
                        }
                        goto Label_0956;
                    }
                }
                goto Label_0881;
            }
            if (token != JSToken.Super)
            {
                if (token == JSToken.PreProcessorConstant)
                {
                    canBeAttribute = false;
                    expression = new ConstantWrapper(this.scanner.GetPreProcessorValue(), this.currentToken.Clone());
                    goto Label_0956;
                }
                goto Label_0881;
            }
            canBeAttribute = false;
            expression = new ThisLiteral(this.currentToken.Clone(), true);
            goto Label_0956;
        Label_046D:
            if (JSToken.Comma != this.currentToken.token)
            {
                this.noSkipTokenSet.Add(NoSkipTokenSet.s_ArrayInitNoSkipTokenSet);
                try
                {
                    list2.Append(this.ParseExpression(true));
                    if (JSToken.Comma != this.currentToken.token)
                    {
                        if (JSToken.RightBracket != this.currentToken.token)
                        {
                            this.ReportError(JSError.NoRightBracket);
                        }
                        goto Label_0573;
                    }
                }
                catch (RecoveryTokenException exception3)
                {
                    if (exception3._partiallyComputedNode != null)
                    {
                        list2.Append(exception3._partiallyComputedNode);
                    }
                    if (this.IndexOfToken(NoSkipTokenSet.s_ArrayInitNoSkipTokenSet, exception3) == -1)
                    {
                        context.UpdateWith(this.CurrentPositionContext());
                        exception3._partiallyComputedNode = new ArrayLiteral(context, list2);
                        throw exception3;
                    }
                    if (JSToken.RightBracket == this.currentToken.token)
                    {
                        goto Label_0573;
                    }
                }
                finally
                {
                    this.noSkipTokenSet.Remove(NoSkipTokenSet.s_ArrayInitNoSkipTokenSet);
                }
            }
            else
            {
                list2.Append(new ConstantWrapper(Microsoft.JScript.Missing.Value, this.currentToken.Clone()));
            }
            this.GetNextToken();
        Label_0561:
            if (JSToken.RightBracket != this.currentToken.token)
            {
                goto Label_046D;
            }
        Label_0573:
            context.UpdateWith(this.currentToken);
            expression = new ArrayLiteral(context, list2);
            goto Label_0956;
        Label_0830:
            list3.context.UpdateWith(this.currentToken);
            context2.UpdateWith(this.currentToken);
            expression = new ObjectLiteral(context2, list3);
            goto Label_0956;
        Label_0881:
            str6 = JSKeyword.CanBeIdentifier(this.currentToken.token);
            if (str6 == null)
            {
                if (this.currentToken.token == JSToken.BitwiseAnd)
                {
                    this.ReportError(JSError.WrongUseOfAddressOf);
                    this.errorToken = null;
                    this.GetNextToken();
                    return this.ParseLeftHandSideExpression(isMinus, ref canBeAttribute, warnForKeyword);
                }
                this.ReportError(JSError.ExpressionExpected);
                this.SkipTokensAndThrow();
                goto Label_0956;
            }
            if (warnForKeyword)
            {
                switch (this.currentToken.token)
                {
                    case JSToken.Boolean:
                    case JSToken.Byte:
                    case JSToken.Char:
                    case JSToken.Double:
                    case JSToken.Float:
                    case JSToken.Int:
                    case JSToken.Long:
                    case JSToken.Short:
                    case JSToken.Void:
                        goto Label_08FC;
                }
                this.ForceReportInfo(JSError.KeywordUsedAsIdentifier);
            }
        Label_08FC:
            canBeAttribute = false;
            expression = new Lookup(str6, this.currentToken.Clone());
        Label_0956:
            if (!flag)
            {
                this.GetNextToken();
            }
            return this.MemberExpression(expression, newContexts, ref canBeAttribute);
        }

        internal string[] ParseNamedBreakpoint(out int argNumber)
        {
            argNumber = 0;
            AST ast = this.ParseQualifiedIdentifier(JSError.SyntaxError);
            if (ast == null)
            {
                return null;
            }
            string[] strArray = new string[4];
            strArray[0] = ast.ToString();
            if (JSToken.LeftParen == this.currentToken.token)
            {
                string name = null;
                string str2 = null;
                AST qualid = null;
                strArray[1] = "";
                this.GetNextToken();
                while (JSToken.RightParen != this.currentToken.token)
                {
                    string[] strArray2;
                    name = null;
                    if ((JSToken.Identifier != this.currentToken.token) && ((name = JSKeyword.CanBeIdentifier(this.currentToken.token)) == null))
                    {
                        return null;
                    }
                    if (name == null)
                    {
                        name = this.scanner.GetIdentifier();
                    }
                    qualid = new Lookup(name, this.currentToken.Clone());
                    this.GetNextToken();
                    if (JSToken.AccessField == this.currentToken.token)
                    {
                        str2 = this.ParseScopeSequence(qualid, JSError.SyntaxError).ToString();
                        while (JSToken.LeftBracket == this.currentToken.token)
                        {
                            this.GetNextToken();
                            if (JSToken.RightBracket != this.currentToken.token)
                            {
                                return null;
                            }
                            str2 = str2 + "[]";
                            this.GetNextToken();
                        }
                    }
                    else
                    {
                        if (JSToken.Colon == this.currentToken.token)
                        {
                            this.GetNextToken();
                            if (JSToken.RightParen == this.currentToken.token)
                            {
                                return null;
                            }
                            continue;
                        }
                        str2 = qualid.ToString();
                    }
                    (strArray2 = strArray)[1] = strArray2[1] + str2 + " ";
                    argNumber++;
                    if (JSToken.Comma == this.currentToken.token)
                    {
                        this.GetNextToken();
                        if (JSToken.RightParen == this.currentToken.token)
                        {
                            return null;
                        }
                    }
                }
                this.GetNextToken();
                if (JSToken.Colon == this.currentToken.token)
                {
                    this.GetNextToken();
                    name = null;
                    if ((JSToken.Identifier != this.currentToken.token) && ((name = JSKeyword.CanBeIdentifier(this.currentToken.token)) == null))
                    {
                        return null;
                    }
                    if (name == null)
                    {
                        name = this.scanner.GetIdentifier();
                    }
                    qualid = new Lookup(name, this.currentToken.Clone());
                    this.GetNextToken();
                    if (JSToken.AccessField == this.currentToken.token)
                    {
                        qualid = this.ParseScopeSequence(qualid, JSError.SyntaxError);
                        strArray[2] = qualid.ToString();
                        while (JSToken.LeftBracket == this.currentToken.token)
                        {
                            string[] strArray3;
                            this.GetNextToken();
                            if (JSToken.RightBracket != this.currentToken.token)
                            {
                                return null;
                            }
                            (strArray3 = strArray)[2] = strArray3[2] + "[]";
                            this.GetNextToken();
                        }
                    }
                    else
                    {
                        strArray[2] = qualid.ToString();
                    }
                }
            }
            if (JSToken.FirstBinaryOp == this.currentToken.token)
            {
                this.GetNextToken();
                if (JSToken.IntegerLiteral != this.currentToken.token)
                {
                    return null;
                }
                strArray[3] = this.currentToken.GetCode();
                this.GetNextToken();
            }
            if (this.currentToken.token != JSToken.EndOfFile)
            {
                return null;
            }
            return strArray;
        }

        private AST ParsePackage(Context packageContext)
        {
            AST ast4;
            this.GetNextToken();
            AST expression = null;
            bool flag = this.scanner.GotEndOfLine();
            if (JSToken.Identifier != this.currentToken.token)
            {
                if (JSScanner.CanParseAsExpression(this.currentToken.token))
                {
                    bool flag2;
                    this.ReportError(JSError.KeywordUsedAsIdentifier, packageContext.Clone(), true);
                    expression = new Lookup("package", packageContext);
                    expression = this.MemberExpression(expression, null);
                    expression = this.ParsePostfixExpression(expression, out flag2);
                    expression = this.ParseExpression(expression, false, flag2, JSToken.None);
                    return new Expression(expression.context.Clone(), expression);
                }
                if (flag)
                {
                    this.ReportError(JSError.KeywordUsedAsIdentifier, packageContext.Clone(), true);
                    return new Lookup("package", packageContext);
                }
                if ((JSToken.Increment == this.currentToken.token) || (JSToken.Decrement == this.currentToken.token))
                {
                    bool flag3;
                    this.ReportError(JSError.KeywordUsedAsIdentifier, packageContext.Clone(), true);
                    expression = new Lookup("package", packageContext);
                    expression = this.ParsePostfixExpression(expression, out flag3);
                    expression = this.ParseExpression(expression, false, false, JSToken.None);
                    return new Expression(expression.context.Clone(), expression);
                }
            }
            else
            {
                this.errorToken = this.currentToken;
                expression = this.ParseQualifiedIdentifier(JSError.NoIdentifier);
            }
            Context context = null;
            if ((JSToken.LeftCurly != this.currentToken.token) && (expression == null))
            {
                context = this.currentToken.Clone();
                this.GetNextToken();
            }
            if (JSToken.LeftCurly == this.currentToken.token)
            {
                if (expression == null)
                {
                    if (context == null)
                    {
                        context = this.currentToken.Clone();
                    }
                    this.ReportError(JSError.NoIdentifier, context, true);
                }
            }
            else if (expression == null)
            {
                this.ReportError(JSError.SyntaxError, packageContext);
                if (JSScanner.CanStartStatement(context.token))
                {
                    this.currentToken = context;
                    return this.ParseStatement();
                }
                if (JSScanner.CanStartStatement(this.currentToken.token))
                {
                    this.errorToken = null;
                    return this.ParseStatement();
                }
                this.ReportError(JSError.SyntaxError);
                this.SkipTokensAndThrow();
            }
            else
            {
                if (flag)
                {
                    bool flag4;
                    this.ReportError(JSError.KeywordUsedAsIdentifier, packageContext.Clone(), true);
                    Block block = new Block(packageContext.Clone());
                    block.Append(new Lookup("package", packageContext));
                    expression = this.MemberExpression(expression, null);
                    expression = this.ParsePostfixExpression(expression, out flag4);
                    expression = this.ParseExpression(expression, false, true, JSToken.None);
                    block.Append(new Expression(expression.context.Clone(), expression));
                    block.context.UpdateWith(expression.context);
                    return block;
                }
                this.ReportError(JSError.NoLeftCurly);
            }
            PackageScope item = new PackageScope(this.Globals.ScopeStack.Peek());
            this.Globals.ScopeStack.Push(item);
            try
            {
                string name = (expression != null) ? expression.ToString() : "anonymous package";
                item.name = name;
                packageContext.UpdateWith(this.currentToken);
                ASTList classList = new ASTList(packageContext);
                this.GetNextToken();
                this.noSkipTokenSet.Add(NoSkipTokenSet.s_BlockNoSkipTokenSet);
                this.noSkipTokenSet.Add(NoSkipTokenSet.s_PackageBodyNoSkipTokenSet);
                try
                {
                    while (this.currentToken.token != JSToken.RightCurly)
                    {
                        AST statement = null;
                        try
                        {
                            switch (this.currentToken.token)
                            {
                                case JSToken.Identifier:
                                {
                                    bool flag6;
                                    bool canBeAttribute = true;
                                    statement = this.ParseUnaryExpression(out flag6, ref canBeAttribute, false);
                                    if (canBeAttribute)
                                    {
                                        bool flag8;
                                        statement = this.ParseAttributes(statement, true, false, out flag8);
                                        if (flag8 && (statement is Class))
                                        {
                                            classList.Append(statement);
                                            continue;
                                        }
                                    }
                                    this.ReportError(JSError.OnlyClassesAllowed, statement.context.Clone(), true);
                                    this.SkipTokensAndThrow();
                                    continue;
                                }
                                case JSToken.Interface:
                                case JSToken.Class:
                                {
                                    classList.Append(this.ParseClass(FieldAttributes.PrivateScope, false, this.currentToken.Clone(), false, false, null));
                                    continue;
                                }
                                case JSToken.Enum:
                                {
                                    classList.Append(this.ParseEnum(FieldAttributes.PrivateScope, this.currentToken.Clone(), null));
                                    continue;
                                }
                                case JSToken.Import:
                                {
                                    this.ReportError(JSError.InvalidImport, true);
                                    try
                                    {
                                        this.ParseImportStatement();
                                    }
                                    catch (RecoveryTokenException)
                                    {
                                    }
                                    continue;
                                }
                                case JSToken.Package:
                                {
                                    Context context2 = this.currentToken.Clone();
                                    if (this.ParsePackage(context2) is Package)
                                    {
                                        this.ReportError(JSError.PackageInWrongContext, context2, true);
                                    }
                                    continue;
                                }
                                case JSToken.Internal:
                                case JSToken.Abstract:
                                case JSToken.Public:
                                case JSToken.Static:
                                case JSToken.Private:
                                case JSToken.Protected:
                                case JSToken.Final:
                                {
                                    bool flag5;
                                    statement = this.ParseAttributes(null, true, false, out flag5);
                                    if (!flag5 || !(statement is Class))
                                    {
                                        break;
                                    }
                                    classList.Append(statement);
                                    continue;
                                }
                                case JSToken.Semicolon:
                                {
                                    this.GetNextToken();
                                    continue;
                                }
                                case JSToken.EndOfFile:
                                    this.EOFError(JSError.ErrEOF);
                                    throw new EndOfFile();

                                default:
                                    goto Label_04D9;
                            }
                            this.ReportError(JSError.OnlyClassesAllowed, statement.context.Clone(), true);
                            this.SkipTokensAndThrow();
                            continue;
                        Label_04D9:
                            this.ReportError(JSError.OnlyClassesAllowed, (statement != null) ? statement.context.Clone() : this.CurrentPositionContext(), true);
                            this.SkipTokensAndThrow();
                            continue;
                        }
                        catch (RecoveryTokenException exception)
                        {
                            if ((exception._partiallyComputedNode != null) && (exception._partiallyComputedNode is Class))
                            {
                                classList.Append((Class) exception._partiallyComputedNode);
                                exception._partiallyComputedNode = null;
                            }
                            if (this.IndexOfToken(NoSkipTokenSet.s_PackageBodyNoSkipTokenSet, exception) == -1)
                            {
                                throw exception;
                            }
                            continue;
                        }
                    }
                }
                catch (RecoveryTokenException exception2)
                {
                    if (this.IndexOfToken(NoSkipTokenSet.s_BlockNoSkipTokenSet, exception2) == -1)
                    {
                        this.ReportError(JSError.NoRightCurly, this.CurrentPositionContext());
                        exception2._partiallyComputedNode = new Package(name, expression, classList, packageContext);
                        throw exception2;
                    }
                }
                finally
                {
                    this.noSkipTokenSet.Remove(NoSkipTokenSet.s_PackageBodyNoSkipTokenSet);
                    this.noSkipTokenSet.Remove(NoSkipTokenSet.s_BlockNoSkipTokenSet);
                }
                this.GetNextToken();
                ast4 = new Package(name, expression, classList, packageContext);
            }
            finally
            {
                this.Globals.ScopeStack.Pop();
            }
            return ast4;
        }

        private AST ParsePostfixExpression(AST ast, out bool isLeftHandSideExpr)
        {
            bool canBeAttribute = false;
            return this.ParsePostfixExpression(ast, out isLeftHandSideExpr, ref canBeAttribute);
        }

        private AST ParsePostfixExpression(AST ast, out bool isLeftHandSideExpr, ref bool canBeAttribute)
        {
            isLeftHandSideExpr = true;
            Context context = null;
            if ((ast != null) && !this.scanner.GotEndOfLine())
            {
                if (JSToken.Increment == this.currentToken.token)
                {
                    isLeftHandSideExpr = false;
                    context = ast.context.Clone();
                    context.UpdateWith(this.currentToken);
                    canBeAttribute = false;
                    ast = new PostOrPrefixOperator(context, ast, PostOrPrefix.PostfixIncrement);
                    this.GetNextToken();
                    return ast;
                }
                if (JSToken.Decrement == this.currentToken.token)
                {
                    isLeftHandSideExpr = false;
                    context = ast.context.Clone();
                    context.UpdateWith(this.currentToken);
                    canBeAttribute = false;
                    ast = new PostOrPrefixOperator(context, ast, PostOrPrefix.PostfixDecrement);
                    this.GetNextToken();
                }
            }
            return ast;
        }

        private AST ParseQualifiedIdentifier(JSError error)
        {
            this.GetNextToken();
            AST qualid = null;
            string name = null;
            Context context = this.currentToken.Clone();
            if (JSToken.Identifier == this.currentToken.token)
            {
                qualid = new Lookup(this.scanner.GetIdentifier(), context);
            }
            else
            {
                name = JSKeyword.CanBeIdentifier(this.currentToken.token);
                if (name == null)
                {
                    this.ReportError(error, true);
                    this.SkipTokensAndThrow();
                }
                else
                {
                    switch (this.currentToken.token)
                    {
                        case JSToken.Boolean:
                        case JSToken.Byte:
                        case JSToken.Char:
                        case JSToken.Double:
                        case JSToken.Float:
                        case JSToken.Int:
                        case JSToken.Long:
                        case JSToken.Short:
                        case JSToken.Void:
                            break;

                        default:
                            this.ForceReportInfo(JSError.KeywordUsedAsIdentifier);
                            break;
                    }
                    qualid = new Lookup(name, context);
                }
            }
            this.GetNextToken();
            if (JSToken.AccessField == this.currentToken.token)
            {
                qualid = this.ParseScopeSequence(qualid, error);
            }
            return qualid;
        }

        private Return ParseReturnStatement()
        {
            Context context = this.currentToken.Clone();
            if (this.Globals.ScopeStack.Peek() is FunctionScope)
            {
                AST operand = null;
                this.GetNextToken();
                if (!this.scanner.GotEndOfLine())
                {
                    if ((JSToken.Semicolon != this.currentToken.token) && (JSToken.RightCurly != this.currentToken.token))
                    {
                        this.noSkipTokenSet.Add(NoSkipTokenSet.s_EndOfStatementNoSkipTokenSet);
                        try
                        {
                            operand = this.ParseExpression();
                        }
                        catch (RecoveryTokenException exception)
                        {
                            operand = exception._partiallyComputedNode;
                            if (this.IndexOfToken(NoSkipTokenSet.s_EndOfStatementNoSkipTokenSet, exception) == -1)
                            {
                                if (operand != null)
                                {
                                    context.UpdateWith(operand.context);
                                }
                                exception._partiallyComputedNode = new Return(context, operand, this.CheckForReturnFromFinally());
                                throw exception;
                            }
                        }
                        finally
                        {
                            this.noSkipTokenSet.Remove(NoSkipTokenSet.s_EndOfStatementNoSkipTokenSet);
                        }
                        if (((JSToken.Semicolon != this.currentToken.token) && (JSToken.RightCurly != this.currentToken.token)) && !this.scanner.GotEndOfLine())
                        {
                            this.ReportError(JSError.NoSemicolon, true);
                        }
                    }
                    if (JSToken.Semicolon == this.currentToken.token)
                    {
                        context.UpdateWith(this.currentToken);
                        this.GetNextToken();
                    }
                    else if (operand != null)
                    {
                        context.UpdateWith(operand.context);
                    }
                }
                return new Return(context, operand, this.CheckForReturnFromFinally());
            }
            this.ReportError(JSError.BadReturn, context, true);
            this.GetNextToken();
            return null;
        }

        private AST ParseScopeSequence(AST qualid, JSError error)
        {
            ConstantWrapper memberName = null;
            string str = null;
            do
            {
                this.GetNextToken();
                if (JSToken.Identifier != this.currentToken.token)
                {
                    str = JSKeyword.CanBeIdentifier(this.currentToken.token);
                    if (str != null)
                    {
                        this.ForceReportInfo(JSError.KeywordUsedAsIdentifier);
                        memberName = new ConstantWrapper(str, this.currentToken.Clone());
                    }
                    else
                    {
                        this.ReportError(error, true);
                        this.SkipTokensAndThrow(qualid);
                    }
                }
                else
                {
                    memberName = new ConstantWrapper(this.scanner.GetIdentifier(), this.currentToken.Clone());
                }
                qualid = new Member(qualid.context.CombineWith(this.currentToken), qualid, memberName);
                this.GetNextToken();
            }
            while (JSToken.AccessField == this.currentToken.token);
            return qualid;
        }

        private AST ParseStatement()
        {
            AST leftHandSide = null;
            string key = null;
            switch (this.currentToken.token)
            {
                case JSToken.Super:
                case JSToken.This:
                {
                    Context superCtx = this.currentToken.Clone();
                    if (JSToken.LeftParen != this.scanner.PeekToken())
                    {
                        break;
                    }
                    leftHandSide = this.ParseConstructorCall(superCtx);
                    goto Label_04B0;
                }
                case JSToken.RightCurly:
                    this.ReportError(JSError.SyntaxError);
                    this.SkipTokensAndThrow();
                    goto Label_04B0;

                case JSToken.Enum:
                    return this.ParseEnum(FieldAttributes.PrivateScope, this.currentToken.Clone(), null);

                case JSToken.Interface:
                case JSToken.Class:
                    return this.ParseClass(FieldAttributes.PrivateScope, false, this.currentToken.Clone(), false, false, null);

                case JSToken.EndOfFile:
                    this.EOFError(JSError.ErrEOF);
                    throw new EndOfFile();

                case JSToken.If:
                    return this.ParseIfStatement();

                case JSToken.For:
                    return this.ParseForStatement();

                case JSToken.Do:
                    return this.ParseDoStatement();

                case JSToken.While:
                    return this.ParseWhileStatement();

                case JSToken.Continue:
                    leftHandSide = this.ParseContinueStatement();
                    if (leftHandSide != null)
                    {
                        return leftHandSide;
                    }
                    return new Block(this.CurrentPositionContext());

                case JSToken.Break:
                    leftHandSide = this.ParseBreakStatement();
                    if (leftHandSide != null)
                    {
                        return leftHandSide;
                    }
                    return new Block(this.CurrentPositionContext());

                case JSToken.Return:
                    leftHandSide = this.ParseReturnStatement();
                    if (leftHandSide != null)
                    {
                        return leftHandSide;
                    }
                    return new Block(this.CurrentPositionContext());

                case JSToken.Import:
                    this.ReportError(JSError.InvalidImport, true);
                    leftHandSide = new Block(this.currentToken.Clone());
                    try
                    {
                        this.ParseImportStatement();
                    }
                    catch (RecoveryTokenException)
                    {
                    }
                    goto Label_04B0;

                case JSToken.With:
                    return this.ParseWithStatement();

                case JSToken.Switch:
                    return this.ParseSwitchStatement();

                case JSToken.Throw:
                    leftHandSide = this.ParseThrowStatement();
                    if (leftHandSide != null)
                    {
                        goto Label_04B0;
                    }
                    return new Block(this.CurrentPositionContext());

                case JSToken.Try:
                    return this.ParseTryStatement();

                case JSToken.Package:
                {
                    Context packageContext = this.currentToken.Clone();
                    leftHandSide = this.ParsePackage(packageContext);
                    if (leftHandSide is Package)
                    {
                        this.ReportError(JSError.PackageInWrongContext, packageContext, true);
                        leftHandSide = new Block(packageContext);
                    }
                    goto Label_04B0;
                }
                case JSToken.Internal:
                case JSToken.Abstract:
                case JSToken.Public:
                case JSToken.Static:
                case JSToken.Private:
                case JSToken.Protected:
                case JSToken.Final:
                    bool flag;
                    leftHandSide = this.ParseAttributes(null, false, false, out flag);
                    if (flag)
                    {
                        return leftHandSide;
                    }
                    leftHandSide = this.ParseExpression(leftHandSide, false, true, JSToken.None);
                    leftHandSide = new Expression(leftHandSide.context.Clone(), leftHandSide);
                    goto Label_04B0;

                case JSToken.Var:
                case JSToken.Const:
                    return this.ParseVariableStatement(FieldAttributes.PrivateScope, null, this.currentToken.token);

                case JSToken.Function:
                    return this.ParseFunction(FieldAttributes.PrivateScope, false, this.currentToken.Clone(), false, false, false, false, null);

                case JSToken.LeftCurly:
                    return this.ParseBlock();

                case JSToken.Semicolon:
                    leftHandSide = new Block(this.currentToken.Clone());
                    this.GetNextToken();
                    return leftHandSide;

                case JSToken.Debugger:
                    leftHandSide = new DebugBreak(this.currentToken.Clone());
                    this.GetNextToken();
                    goto Label_04B0;

                case JSToken.Else:
                    this.ReportError(JSError.InvalidElse);
                    this.SkipTokensAndThrow();
                    goto Label_04B0;
            }
            this.noSkipTokenSet.Add(NoSkipTokenSet.s_EndOfStatementNoSkipTokenSet);
            bool flag2 = false;
            try
            {
                bool flag3;
                bool canBeAttribute = true;
                leftHandSide = this.ParseUnaryExpression(out flag3, ref canBeAttribute, false);
                if (canBeAttribute)
                {
                    if ((leftHandSide is Lookup) && (JSToken.Colon == this.currentToken.token))
                    {
                        key = leftHandSide.ToString();
                        if (this.labelTable[key] != null)
                        {
                            this.ReportError(JSError.BadLabel, leftHandSide.context.Clone(), true);
                            key = null;
                            this.GetNextToken();
                            return new Block(this.CurrentPositionContext());
                        }
                        this.GetNextToken();
                        this.labelTable[key] = this.blockType.Count;
                        if (this.currentToken.token != JSToken.EndOfFile)
                        {
                            leftHandSide = this.ParseStatement();
                        }
                        else
                        {
                            leftHandSide = new Block(this.CurrentPositionContext());
                        }
                        this.labelTable.Remove(key);
                        return leftHandSide;
                    }
                    if ((JSToken.Semicolon != this.currentToken.token) && !this.scanner.GotEndOfLine())
                    {
                        bool flag5;
                        leftHandSide = this.ParseAttributes(leftHandSide, false, false, out flag5);
                        if (flag5)
                        {
                            return leftHandSide;
                        }
                    }
                }
                leftHandSide = this.ParseExpression(leftHandSide, false, flag3, JSToken.None);
                leftHandSide = new Expression(leftHandSide.context.Clone(), leftHandSide);
            }
            catch (RecoveryTokenException exception)
            {
                if (exception._partiallyComputedNode != null)
                {
                    leftHandSide = exception._partiallyComputedNode;
                }
                if (leftHandSide == null)
                {
                    this.noSkipTokenSet.Remove(NoSkipTokenSet.s_EndOfStatementNoSkipTokenSet);
                    flag2 = true;
                    this.SkipTokensAndThrow();
                }
                if (this.IndexOfToken(NoSkipTokenSet.s_EndOfStatementNoSkipTokenSet, exception) == -1)
                {
                    exception._partiallyComputedNode = leftHandSide;
                    throw exception;
                }
            }
            finally
            {
                if (!flag2)
                {
                    this.noSkipTokenSet.Remove(NoSkipTokenSet.s_EndOfStatementNoSkipTokenSet);
                }
            }
        Label_04B0:
            if (JSToken.Semicolon == this.currentToken.token)
            {
                leftHandSide.context.UpdateWith(this.currentToken);
                this.GetNextToken();
                return leftHandSide;
            }
            if ((!this.scanner.GotEndOfLine() && (JSToken.RightCurly != this.currentToken.token)) && (this.currentToken.token != JSToken.EndOfFile))
            {
                this.ReportError(JSError.NoSemicolon, true);
            }
            return leftHandSide;
        }

        private Block ParseStatements(bool insideEval)
        {
            int i = this.Globals.ScopeStack.Size();
            this.program = new Block(this.sourceContext.Clone());
            this.blockType.Add(BlockType.Block);
            this.errorToken = null;
            try
            {
                this.GetNextToken();
                this.noSkipTokenSet.Add(NoSkipTokenSet.s_StartStatementNoSkipTokenSet);
                this.noSkipTokenSet.Add(NoSkipTokenSet.s_TopLevelNoSkipTokenSet);
                try
                {
                    while (this.currentToken.token != JSToken.EndOfFile)
                    {
                        AST elem = null;
                        try
                        {
                            if ((this.currentToken.token == JSToken.Package) && !insideEval)
                            {
                                elem = this.ParsePackage(this.currentToken.Clone());
                            }
                            else
                            {
                                if ((this.currentToken.token == JSToken.Import) && !insideEval)
                                {
                                    this.noSkipTokenSet.Add(NoSkipTokenSet.s_EndOfStatementNoSkipTokenSet);
                                    try
                                    {
                                        try
                                        {
                                            elem = this.ParseImportStatement();
                                        }
                                        catch (RecoveryTokenException exception)
                                        {
                                            if (this.IndexOfToken(NoSkipTokenSet.s_EndOfStatementNoSkipTokenSet, exception) == -1)
                                            {
                                                throw exception;
                                            }
                                            elem = exception._partiallyComputedNode;
                                            if (exception._token == JSToken.Semicolon)
                                            {
                                                this.GetNextToken();
                                            }
                                        }
                                        goto Label_0178;
                                    }
                                    finally
                                    {
                                        this.noSkipTokenSet.Remove(NoSkipTokenSet.s_EndOfStatementNoSkipTokenSet);
                                    }
                                }
                                elem = this.ParseStatement();
                            }
                        }
                        catch (RecoveryTokenException exception2)
                        {
                            if (this.TokenInList(NoSkipTokenSet.s_TopLevelNoSkipTokenSet, exception2) || this.TokenInList(NoSkipTokenSet.s_StartStatementNoSkipTokenSet, exception2))
                            {
                                elem = exception2._partiallyComputedNode;
                            }
                            else
                            {
                                this.errorToken = null;
                                do
                                {
                                    this.GetNextToken();
                                }
                                while (((this.currentToken.token != JSToken.EndOfFile) && !this.TokenInList(NoSkipTokenSet.s_TopLevelNoSkipTokenSet, this.currentToken.token)) && !this.TokenInList(NoSkipTokenSet.s_StartStatementNoSkipTokenSet, this.currentToken.token));
                            }
                        }
                    Label_0178:
                        if (elem != null)
                        {
                            this.program.Append(elem);
                        }
                    }
                }
                finally
                {
                    this.noSkipTokenSet.Remove(NoSkipTokenSet.s_TopLevelNoSkipTokenSet);
                    this.noSkipTokenSet.Remove(NoSkipTokenSet.s_StartStatementNoSkipTokenSet);
                }
            }
            catch (EndOfFile)
            {
            }
            catch (ScannerException exception3)
            {
                this.EOFError(exception3.m_errorId);
            }
            catch (StackOverflowException)
            {
                this.Globals.ScopeStack.TrimToSize(i);
                this.ReportError(JSError.OutOfStack, true);
            }
            return this.program;
        }

        private AST ParseStaticInitializer(Context initContext)
        {
            if (this.demandFullTrustOnFunctionCreation)
            {
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
            }
            Block body = null;
            FunctionScope item = new FunctionScope(this.Globals.ScopeStack.Peek()) {
                isStatic = true
            };
            ArrayList blockType = this.blockType;
            this.blockType = new ArrayList(0x10);
            SimpleHashtable labelTable = this.labelTable;
            this.labelTable = new SimpleHashtable(0x10);
            this.blockType.Add(BlockType.Block);
            this.noSkipTokenSet.Add(NoSkipTokenSet.s_BlockNoSkipTokenSet);
            this.noSkipTokenSet.Add(NoSkipTokenSet.s_StartStatementNoSkipTokenSet);
            try
            {
                this.Globals.ScopeStack.Push(item);
                body = new Block(this.currentToken.Clone());
                this.GetNextToken();
                while (JSToken.RightCurly != this.currentToken.token)
                {
                    try
                    {
                        body.Append(this.ParseStatement());
                        continue;
                    }
                    catch (RecoveryTokenException exception)
                    {
                        if (exception._partiallyComputedNode != null)
                        {
                            body.Append(exception._partiallyComputedNode);
                        }
                        if (this.IndexOfToken(NoSkipTokenSet.s_StartStatementNoSkipTokenSet, exception) == -1)
                        {
                            throw exception;
                        }
                        continue;
                    }
                }
            }
            catch (RecoveryTokenException exception2)
            {
                if (this.IndexOfToken(NoSkipTokenSet.s_BlockNoSkipTokenSet, exception2) == -1)
                {
                    exception2._partiallyComputedNode = new StaticInitializer(initContext, body, item);
                    throw exception2;
                }
            }
            finally
            {
                this.noSkipTokenSet.Remove(NoSkipTokenSet.s_StartStatementNoSkipTokenSet);
                this.noSkipTokenSet.Remove(NoSkipTokenSet.s_BlockNoSkipTokenSet);
                this.blockType = blockType;
                this.labelTable = labelTable;
                this.Globals.ScopeStack.Pop();
            }
            body.context.UpdateWith(this.currentToken);
            initContext.UpdateWith(this.currentToken);
            this.GetNextToken();
            return new StaticInitializer(initContext, body, item);
        }

        private AST ParseSwitchStatement()
        {
            Context context = this.currentToken.Clone();
            AST expression = null;
            ASTList cases = null;
            this.blockType.Add(BlockType.Switch);
            try
            {
                this.GetNextToken();
                if (JSToken.LeftParen != this.currentToken.token)
                {
                    this.ReportError(JSError.NoLeftParen);
                }
                this.GetNextToken();
                this.noSkipTokenSet.Add(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet);
                this.noSkipTokenSet.Add(NoSkipTokenSet.s_SwitchNoSkipTokenSet);
                try
                {
                    expression = this.ParseExpression();
                    if (JSToken.RightParen != this.currentToken.token)
                    {
                        this.ReportError(JSError.NoRightParen);
                    }
                    this.GetNextToken();
                    if (JSToken.LeftCurly != this.currentToken.token)
                    {
                        this.ReportError(JSError.NoLeftCurly);
                    }
                    this.GetNextToken();
                }
                catch (RecoveryTokenException exception)
                {
                    if ((this.IndexOfToken(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet, exception) == -1) && (this.IndexOfToken(NoSkipTokenSet.s_SwitchNoSkipTokenSet, exception) == -1))
                    {
                        exception._partiallyComputedNode = null;
                        throw exception;
                    }
                    if (exception._partiallyComputedNode == null)
                    {
                        expression = new ConstantWrapper(true, this.CurrentPositionContext());
                    }
                    else
                    {
                        expression = exception._partiallyComputedNode;
                    }
                    if (this.IndexOfToken(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet, exception) != -1)
                    {
                        if (exception._token == JSToken.RightParen)
                        {
                            this.GetNextToken();
                        }
                        if (JSToken.LeftCurly != this.currentToken.token)
                        {
                            this.ReportError(JSError.NoLeftCurly);
                        }
                        this.GetNextToken();
                    }
                }
                finally
                {
                    this.noSkipTokenSet.Remove(NoSkipTokenSet.s_SwitchNoSkipTokenSet);
                    this.noSkipTokenSet.Remove(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet);
                }
                cases = new ASTList(this.currentToken.Clone());
                bool flag = false;
                this.noSkipTokenSet.Add(NoSkipTokenSet.s_BlockNoSkipTokenSet);
                try
                {
                    while (JSToken.RightCurly != this.currentToken.token)
                    {
                        SwitchCase elem = null;
                        AST ast2 = null;
                        Context context2 = this.currentToken.Clone();
                        this.noSkipTokenSet.Add(NoSkipTokenSet.s_CaseNoSkipTokenSet);
                        try
                        {
                            if (JSToken.Case == this.currentToken.token)
                            {
                                this.GetNextToken();
                                ast2 = this.ParseExpression();
                            }
                            else if (JSToken.Default == this.currentToken.token)
                            {
                                if (flag)
                                {
                                    this.ReportError(JSError.DupDefault, true);
                                }
                                else
                                {
                                    flag = true;
                                }
                                this.GetNextToken();
                            }
                            else
                            {
                                flag = true;
                                this.ReportError(JSError.BadSwitch);
                            }
                            if (JSToken.Colon != this.currentToken.token)
                            {
                                this.ReportError(JSError.NoColon);
                            }
                            this.GetNextToken();
                        }
                        catch (RecoveryTokenException exception2)
                        {
                            if (this.IndexOfToken(NoSkipTokenSet.s_CaseNoSkipTokenSet, exception2) == -1)
                            {
                                exception2._partiallyComputedNode = null;
                                throw exception2;
                            }
                            ast2 = exception2._partiallyComputedNode;
                            if (exception2._token == JSToken.Colon)
                            {
                                this.GetNextToken();
                            }
                        }
                        finally
                        {
                            this.noSkipTokenSet.Remove(NoSkipTokenSet.s_CaseNoSkipTokenSet);
                        }
                        this.blockType.Add(BlockType.Block);
                        try
                        {
                            Block statements = new Block(this.currentToken.Clone());
                            this.noSkipTokenSet.Add(NoSkipTokenSet.s_SwitchNoSkipTokenSet);
                            this.noSkipTokenSet.Add(NoSkipTokenSet.s_StartStatementNoSkipTokenSet);
                            try
                            {
                                while (((JSToken.RightCurly != this.currentToken.token) && (JSToken.Case != this.currentToken.token)) && (JSToken.Default != this.currentToken.token))
                                {
                                    try
                                    {
                                        statements.Append(this.ParseStatement());
                                        continue;
                                    }
                                    catch (RecoveryTokenException exception3)
                                    {
                                        if (exception3._partiallyComputedNode != null)
                                        {
                                            statements.Append(exception3._partiallyComputedNode);
                                            exception3._partiallyComputedNode = null;
                                        }
                                        if (this.IndexOfToken(NoSkipTokenSet.s_StartStatementNoSkipTokenSet, exception3) == -1)
                                        {
                                            throw exception3;
                                        }
                                        continue;
                                    }
                                }
                            }
                            catch (RecoveryTokenException exception4)
                            {
                                if (this.IndexOfToken(NoSkipTokenSet.s_SwitchNoSkipTokenSet, exception4) == -1)
                                {
                                    if (ast2 == null)
                                    {
                                        elem = new SwitchCase(context2, statements);
                                    }
                                    else
                                    {
                                        elem = new SwitchCase(context2, ast2, statements);
                                    }
                                    cases.Append(elem);
                                    throw exception4;
                                }
                            }
                            finally
                            {
                                this.noSkipTokenSet.Remove(NoSkipTokenSet.s_StartStatementNoSkipTokenSet);
                                this.noSkipTokenSet.Remove(NoSkipTokenSet.s_SwitchNoSkipTokenSet);
                            }
                            if (JSToken.RightCurly == this.currentToken.token)
                            {
                                statements.context.UpdateWith(this.currentToken);
                            }
                            if (ast2 == null)
                            {
                                context2.UpdateWith(statements.context);
                                elem = new SwitchCase(context2, statements);
                            }
                            else
                            {
                                context2.UpdateWith(statements.context);
                                elem = new SwitchCase(context2, ast2, statements);
                            }
                            cases.Append(elem);
                            continue;
                        }
                        finally
                        {
                            this.blockType.RemoveAt(this.blockType.Count - 1);
                        }
                    }
                }
                catch (RecoveryTokenException exception5)
                {
                    if (this.IndexOfToken(NoSkipTokenSet.s_BlockNoSkipTokenSet, exception5) == -1)
                    {
                        context.UpdateWith(this.CurrentPositionContext());
                        exception5._partiallyComputedNode = new Switch(context, expression, cases);
                        throw exception5;
                    }
                }
                finally
                {
                    this.noSkipTokenSet.Remove(NoSkipTokenSet.s_BlockNoSkipTokenSet);
                }
                context.UpdateWith(this.currentToken);
                this.GetNextToken();
            }
            finally
            {
                this.blockType.RemoveAt(this.blockType.Count - 1);
            }
            return new Switch(context, expression, cases);
        }

        private AST ParseThrowStatement()
        {
            Context context = this.currentToken.Clone();
            this.GetNextToken();
            AST operand = null;
            if (!this.scanner.GotEndOfLine() && (JSToken.Semicolon != this.currentToken.token))
            {
                this.noSkipTokenSet.Add(NoSkipTokenSet.s_EndOfStatementNoSkipTokenSet);
                try
                {
                    operand = this.ParseExpression();
                }
                catch (RecoveryTokenException exception)
                {
                    operand = exception._partiallyComputedNode;
                    if (this.IndexOfToken(NoSkipTokenSet.s_EndOfStatementNoSkipTokenSet, exception) == -1)
                    {
                        if (operand != null)
                        {
                            exception._partiallyComputedNode = new Throw(context, exception._partiallyComputedNode);
                        }
                        throw exception;
                    }
                }
                finally
                {
                    this.noSkipTokenSet.Remove(NoSkipTokenSet.s_EndOfStatementNoSkipTokenSet);
                }
            }
            if (operand != null)
            {
                context.UpdateWith(operand.context);
            }
            return new Throw(context, operand);
        }

        private AST ParseTryStatement()
        {
            Context context = this.currentToken.Clone();
            Context closingBraceContext = null;
            AST body = null;
            AST identifier = null;
            AST handler = null;
            AST ast4 = null;
            RecoveryTokenException exception = null;
            TypeExpression type = null;
            this.blockType.Add(BlockType.Block);
            try
            {
                bool flag = false;
                bool flag2 = false;
                this.GetNextToken();
                if (JSToken.LeftCurly != this.currentToken.token)
                {
                    this.ReportError(JSError.NoLeftCurly);
                }
                this.noSkipTokenSet.Add(NoSkipTokenSet.s_NoTrySkipTokenSet);
                try
                {
                    try
                    {
                        body = this.ParseBlock(out closingBraceContext);
                    }
                    catch (RecoveryTokenException exception2)
                    {
                        if (this.IndexOfToken(NoSkipTokenSet.s_NoTrySkipTokenSet, exception2) == -1)
                        {
                            throw exception2;
                        }
                        body = exception2._partiallyComputedNode;
                    }
                    goto Label_02E5;
                }
                finally
                {
                    this.noSkipTokenSet.Remove(NoSkipTokenSet.s_NoTrySkipTokenSet);
                }
            Label_00A6:
                this.noSkipTokenSet.Add(NoSkipTokenSet.s_NoTrySkipTokenSet);
                try
                {
                    if (handler != null)
                    {
                        body = new Try(context, body, identifier, type, handler, null, false, closingBraceContext);
                        identifier = null;
                        type = null;
                        handler = null;
                    }
                    flag = true;
                    this.GetNextToken();
                    if (JSToken.LeftParen != this.currentToken.token)
                    {
                        this.ReportError(JSError.NoLeftParen);
                    }
                    this.GetNextToken();
                    if (JSToken.Identifier != this.currentToken.token)
                    {
                        string name = JSKeyword.CanBeIdentifier(this.currentToken.token);
                        if (name != null)
                        {
                            this.ForceReportInfo(JSError.KeywordUsedAsIdentifier);
                            identifier = new Lookup(name, this.currentToken.Clone());
                        }
                        else
                        {
                            this.ReportError(JSError.NoIdentifier);
                            identifier = new Lookup("##Exc##" + s_cDummyName++, this.CurrentPositionContext());
                        }
                    }
                    else
                    {
                        identifier = new Lookup(this.scanner.GetIdentifier(), this.currentToken.Clone());
                    }
                    this.GetNextToken();
                    this.noSkipTokenSet.Add(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet);
                    try
                    {
                        if (JSToken.Colon == this.currentToken.token)
                        {
                            type = this.ParseTypeExpression();
                        }
                        else
                        {
                            if (flag2)
                            {
                                this.ForceReportInfo(identifier.context, JSError.UnreachableCatch);
                            }
                            flag2 = true;
                        }
                        if (JSToken.RightParen != this.currentToken.token)
                        {
                            this.ReportError(JSError.NoRightParen);
                        }
                        this.GetNextToken();
                    }
                    catch (RecoveryTokenException exception3)
                    {
                        if (this.IndexOfToken(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet, exception3) == -1)
                        {
                            exception3._partiallyComputedNode = null;
                            throw exception3;
                        }
                        type = (TypeExpression) exception3._partiallyComputedNode;
                        if (this.currentToken.token == JSToken.RightParen)
                        {
                            this.GetNextToken();
                        }
                    }
                    finally
                    {
                        this.noSkipTokenSet.Remove(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet);
                    }
                    if (JSToken.LeftCurly != this.currentToken.token)
                    {
                        this.ReportError(JSError.NoLeftCurly);
                    }
                    handler = this.ParseBlock();
                    context.UpdateWith(handler.context);
                }
                catch (RecoveryTokenException exception4)
                {
                    if (exception4._partiallyComputedNode == null)
                    {
                        handler = new Block(this.CurrentPositionContext());
                    }
                    else
                    {
                        handler = exception4._partiallyComputedNode;
                    }
                    if (this.IndexOfToken(NoSkipTokenSet.s_NoTrySkipTokenSet, exception4) == -1)
                    {
                        if (type != null)
                        {
                            exception4._partiallyComputedNode = new Try(context, body, identifier, type, handler, null, false, closingBraceContext);
                        }
                        throw exception4;
                    }
                }
                finally
                {
                    this.noSkipTokenSet.Remove(NoSkipTokenSet.s_NoTrySkipTokenSet);
                }
            Label_02E5:
                if (JSToken.Catch == this.currentToken.token)
                {
                    goto Label_00A6;
                }
                try
                {
                    if (JSToken.Finally == this.currentToken.token)
                    {
                        this.GetNextToken();
                        this.blockType.Add(BlockType.Finally);
                        try
                        {
                            ast4 = this.ParseBlock();
                            flag = true;
                        }
                        finally
                        {
                            this.blockType.RemoveAt(this.blockType.Count - 1);
                        }
                        context.UpdateWith(ast4.context);
                    }
                }
                catch (RecoveryTokenException exception5)
                {
                    exception = exception5;
                }
                if (!flag)
                {
                    this.ReportError(JSError.NoCatch, true);
                    ast4 = new Block(this.CurrentPositionContext());
                }
            }
            finally
            {
                this.blockType.RemoveAt(this.blockType.Count - 1);
            }
            bool finallyHasControlFlowOutOfIt = false;
            if (this.finallyEscaped > 0)
            {
                this.finallyEscaped--;
                finallyHasControlFlowOutOfIt = true;
            }
            if (exception != null)
            {
                exception._partiallyComputedNode = new Try(context, body, identifier, type, handler, ast4, finallyHasControlFlowOutOfIt, closingBraceContext);
                throw exception;
            }
            return new Try(context, body, identifier, type, handler, ast4, finallyHasControlFlowOutOfIt, closingBraceContext);
        }

        private TypeExpression ParseTypeExpression()
        {
            AST ast = null;
            try
            {
                ast = this.ParseQualifiedIdentifier(JSError.NeedType);
            }
            catch (RecoveryTokenException exception)
            {
                if (exception._partiallyComputedNode != null)
                {
                    exception._partiallyComputedNode = new TypeExpression(exception._partiallyComputedNode);
                }
                throw exception;
            }
            TypeExpression expression = new TypeExpression(ast);
            if (expression != null)
            {
                while (!this.scanner.GotEndOfLine() && (JSToken.LeftBracket == this.currentToken.token))
                {
                    this.GetNextToken();
                    int num = 1;
                    while (JSToken.Comma == this.currentToken.token)
                    {
                        this.GetNextToken();
                        num++;
                    }
                    if (JSToken.RightBracket != this.currentToken.token)
                    {
                        this.ReportError(JSError.NoRightBracket);
                    }
                    this.GetNextToken();
                    if (expression.isArray)
                    {
                        expression = new TypeExpression(expression);
                    }
                    expression.isArray = true;
                    expression.rank = num;
                }
            }
            return expression;
        }

        private AST ParseUnaryExpression(out bool isLeftHandSideExpr, bool isMinus)
        {
            bool canBeAttribute = false;
            return this.ParseUnaryExpression(out isLeftHandSideExpr, ref canBeAttribute, isMinus, false);
        }

        private AST ParseUnaryExpression(out bool isLeftHandSideExpr, ref bool canBeAttribute, bool isMinus)
        {
            return this.ParseUnaryExpression(out isLeftHandSideExpr, ref canBeAttribute, isMinus, true);
        }

        private AST ParseUnaryExpression(out bool isLeftHandSideExpr, ref bool canBeAttribute, bool isMinus, bool warnForKeyword)
        {
            AST ast = null;
            isLeftHandSideExpr = false;
            bool flag = false;
            Context context = null;
            AST operand = null;
            switch (this.currentToken.token)
            {
                case JSToken.FirstOp:
                    context = this.currentToken.Clone();
                    this.GetNextToken();
                    canBeAttribute = false;
                    operand = this.ParseUnaryExpression(out flag, ref canBeAttribute, false);
                    context.UpdateWith(operand.context);
                    ast = new NumericUnary(context, operand, JSToken.FirstOp);
                    break;

                case JSToken.BitwiseNot:
                    context = this.currentToken.Clone();
                    this.GetNextToken();
                    canBeAttribute = false;
                    operand = this.ParseUnaryExpression(out flag, ref canBeAttribute, false);
                    context.UpdateWith(operand.context);
                    ast = new NumericUnary(context, operand, JSToken.BitwiseNot);
                    break;

                case JSToken.Delete:
                    context = this.currentToken.Clone();
                    this.GetNextToken();
                    canBeAttribute = false;
                    operand = this.ParseUnaryExpression(out flag, ref canBeAttribute, false);
                    context.UpdateWith(operand.context);
                    ast = new Delete(context, operand);
                    break;

                case JSToken.Void:
                    context = this.currentToken.Clone();
                    this.GetNextToken();
                    canBeAttribute = false;
                    operand = this.ParseUnaryExpression(out flag, ref canBeAttribute, false);
                    context.UpdateWith(operand.context);
                    ast = new VoidOp(context, operand);
                    break;

                case JSToken.Typeof:
                    context = this.currentToken.Clone();
                    this.GetNextToken();
                    canBeAttribute = false;
                    operand = this.ParseUnaryExpression(out flag, ref canBeAttribute, false);
                    context.UpdateWith(operand.context);
                    ast = new Typeof(context, operand);
                    break;

                case JSToken.Increment:
                    context = this.currentToken.Clone();
                    this.GetNextToken();
                    canBeAttribute = false;
                    operand = this.ParseUnaryExpression(out flag, ref canBeAttribute, false);
                    context.UpdateWith(operand.context);
                    ast = new PostOrPrefixOperator(context, operand, PostOrPrefix.PrefixIncrement);
                    break;

                case JSToken.Decrement:
                    context = this.currentToken.Clone();
                    this.GetNextToken();
                    canBeAttribute = false;
                    operand = this.ParseUnaryExpression(out flag, ref canBeAttribute, false);
                    context.UpdateWith(operand.context);
                    ast = new PostOrPrefixOperator(context, operand, PostOrPrefix.PrefixDecrement);
                    break;

                case JSToken.FirstBinaryOp:
                    context = this.currentToken.Clone();
                    this.GetNextToken();
                    canBeAttribute = false;
                    operand = this.ParseUnaryExpression(out flag, ref canBeAttribute, false);
                    context.UpdateWith(operand.context);
                    ast = new NumericUnary(context, operand, JSToken.FirstBinaryOp);
                    break;

                case JSToken.Minus:
                    context = this.currentToken.Clone();
                    this.GetNextToken();
                    canBeAttribute = false;
                    operand = this.ParseUnaryExpression(out flag, ref canBeAttribute, true);
                    if (operand.context.token != JSToken.NumericLiteral)
                    {
                        context.UpdateWith(operand.context);
                        ast = new NumericUnary(context, operand, JSToken.Minus);
                        break;
                    }
                    context.UpdateWith(operand.context);
                    operand.context = context;
                    ast = operand;
                    break;

                default:
                    this.noSkipTokenSet.Add(NoSkipTokenSet.s_PostfixExpressionNoSkipTokenSet);
                    try
                    {
                        ast = this.ParseLeftHandSideExpression(isMinus, ref canBeAttribute, warnForKeyword);
                    }
                    catch (RecoveryTokenException exception)
                    {
                        if (this.IndexOfToken(NoSkipTokenSet.s_PostfixExpressionNoSkipTokenSet, exception) == -1)
                        {
                            throw exception;
                        }
                        if (exception._partiallyComputedNode == null)
                        {
                            this.SkipTokensAndThrow();
                        }
                        else
                        {
                            ast = exception._partiallyComputedNode;
                        }
                    }
                    finally
                    {
                        this.noSkipTokenSet.Remove(NoSkipTokenSet.s_PostfixExpressionNoSkipTokenSet);
                    }
                    ast = this.ParsePostfixExpression(ast, out isLeftHandSideExpr, ref canBeAttribute);
                    break;
            }
            return ast;
        }

        private AST ParseVariableStatement(FieldAttributes visibility, CustomAttributeList customAttributes, JSToken kind)
        {
            Block block = new Block(this.currentToken.Clone());
            bool flag = true;
            AST elem = null;
        Label_0015:
            this.noSkipTokenSet.Add(NoSkipTokenSet.s_EndOfLineToken);
            try
            {
                elem = this.ParseIdentifierInitializer(JSToken.None, visibility, customAttributes, kind);
            }
            catch (RecoveryTokenException exception)
            {
                if ((exception._partiallyComputedNode != null) && !flag)
                {
                    block.Append(exception._partiallyComputedNode);
                    block.context.UpdateWith(exception._partiallyComputedNode.context);
                    exception._partiallyComputedNode = block;
                }
                if (this.IndexOfToken(NoSkipTokenSet.s_EndOfLineToken, exception) == -1)
                {
                    throw exception;
                }
                if (flag)
                {
                    elem = exception._partiallyComputedNode;
                }
            }
            finally
            {
                this.noSkipTokenSet.Remove(NoSkipTokenSet.s_EndOfLineToken);
            }
            if ((JSToken.Semicolon == this.currentToken.token) || (JSToken.RightCurly == this.currentToken.token))
            {
                if (JSToken.Semicolon == this.currentToken.token)
                {
                    elem.context.UpdateWith(this.currentToken);
                    this.GetNextToken();
                }
            }
            else
            {
                if (JSToken.Comma == this.currentToken.token)
                {
                    flag = false;
                    block.Append(elem);
                    goto Label_0015;
                }
                if (!this.scanner.GotEndOfLine())
                {
                    this.ReportError(JSError.NoSemicolon, true);
                }
            }
            if (flag)
            {
                return elem;
            }
            block.Append(elem);
            block.context.UpdateWith(elem.context);
            return block;
        }

        private While ParseWhileStatement()
        {
            Context context = this.currentToken.Clone();
            AST condition = null;
            AST body = null;
            this.blockType.Add(BlockType.Loop);
            try
            {
                this.GetNextToken();
                if (JSToken.LeftParen != this.currentToken.token)
                {
                    this.ReportError(JSError.NoLeftParen);
                }
                this.GetNextToken();
                this.noSkipTokenSet.Add(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet);
                try
                {
                    condition = this.ParseExpression();
                    if (JSToken.RightParen != this.currentToken.token)
                    {
                        this.ReportError(JSError.NoRightParen);
                        context.UpdateWith(condition.context);
                    }
                    else
                    {
                        context.UpdateWith(this.currentToken);
                    }
                    this.GetNextToken();
                }
                catch (RecoveryTokenException exception)
                {
                    if (this.IndexOfToken(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet, exception) == -1)
                    {
                        exception._partiallyComputedNode = null;
                        throw exception;
                    }
                    if (exception._partiallyComputedNode != null)
                    {
                        condition = exception._partiallyComputedNode;
                    }
                    else
                    {
                        condition = new ConstantWrapper(false, this.CurrentPositionContext());
                    }
                    if (JSToken.RightParen == this.currentToken.token)
                    {
                        this.GetNextToken();
                    }
                }
                finally
                {
                    this.noSkipTokenSet.Remove(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet);
                }
                try
                {
                    body = this.ParseStatement();
                }
                catch (RecoveryTokenException exception2)
                {
                    if (exception2._partiallyComputedNode != null)
                    {
                        body = exception2._partiallyComputedNode;
                    }
                    else
                    {
                        body = new Block(this.CurrentPositionContext());
                    }
                    exception2._partiallyComputedNode = new While(context, condition, body);
                    throw exception2;
                }
            }
            finally
            {
                this.blockType.RemoveAt(this.blockType.Count - 1);
            }
            return new While(context, condition, body);
        }

        private With ParseWithStatement()
        {
            Context context = this.currentToken.Clone();
            AST ast = null;
            AST block = null;
            this.blockType.Add(BlockType.Block);
            try
            {
                this.GetNextToken();
                if (JSToken.LeftParen != this.currentToken.token)
                {
                    this.ReportError(JSError.NoLeftParen);
                }
                this.GetNextToken();
                this.noSkipTokenSet.Add(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet);
                try
                {
                    ast = this.ParseExpression();
                    if (JSToken.RightParen != this.currentToken.token)
                    {
                        context.UpdateWith(ast.context);
                        this.ReportError(JSError.NoRightParen);
                    }
                    else
                    {
                        context.UpdateWith(this.currentToken);
                    }
                    this.GetNextToken();
                }
                catch (RecoveryTokenException exception)
                {
                    if (this.IndexOfToken(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet, exception) == -1)
                    {
                        exception._partiallyComputedNode = null;
                        throw exception;
                    }
                    if (exception._partiallyComputedNode == null)
                    {
                        ast = new ConstantWrapper(true, this.CurrentPositionContext());
                    }
                    else
                    {
                        ast = exception._partiallyComputedNode;
                    }
                    context.UpdateWith(ast.context);
                    if (exception._token == JSToken.RightParen)
                    {
                        this.GetNextToken();
                    }
                }
                finally
                {
                    this.noSkipTokenSet.Remove(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet);
                }
                try
                {
                    block = this.ParseStatement();
                }
                catch (RecoveryTokenException exception2)
                {
                    if (exception2._partiallyComputedNode == null)
                    {
                        block = new Block(this.CurrentPositionContext());
                    }
                    else
                    {
                        block = exception2._partiallyComputedNode;
                    }
                    exception2._partiallyComputedNode = new With(context, ast, block);
                }
            }
            finally
            {
                this.blockType.RemoveAt(this.blockType.Count - 1);
            }
            return new With(context, ast, block);
        }

        private void ReportError(JSError errorId)
        {
            this.ReportError(errorId, false);
        }

        private void ReportError(JSError errorId, Context context)
        {
            this.ReportError(errorId, context, false);
        }

        private void ReportError(JSError errorId, bool skipToken)
        {
            Context context = this.currentToken.Clone();
            context.endPos = context.startPos + 1;
            this.ReportError(errorId, context, skipToken);
        }

        private void ReportError(JSError errorId, Context context, bool skipToken)
        {
            int severity = this.Severity;
            this.Severity = new JScriptException(errorId).Severity;
            if (context.token == JSToken.EndOfFile)
            {
                this.EOFError(errorId);
            }
            else
            {
                if ((this.goodTokensProcessed > 0L) || (this.Severity < severity))
                {
                    context.HandleError(errorId);
                }
                if (skipToken)
                {
                    this.goodTokensProcessed = -1L;
                }
                else
                {
                    this.errorToken = this.currentToken;
                    this.goodTokensProcessed = 0L;
                }
            }
        }

        private void SkipTokensAndThrow()
        {
            this.SkipTokensAndThrow(null);
        }

        private void SkipTokensAndThrow(AST partialAST)
        {
            this.errorToken = null;
            bool flag = this.noSkipTokenSet.HasToken(JSToken.EndOfLine);
            while (!this.noSkipTokenSet.HasToken(this.currentToken.token))
            {
                if (flag && this.scanner.GotEndOfLine())
                {
                    this.errorToken = this.currentToken;
                    throw new RecoveryTokenException(JSToken.EndOfLine, partialAST);
                }
                this.GetNextToken();
                if (++this.tokensSkipped > 50)
                {
                    this.ForceReportInfo(JSError.TooManyTokensSkipped);
                    throw new EndOfFile();
                }
                if (this.currentToken.token == JSToken.EndOfFile)
                {
                    throw new EndOfFile();
                }
            }
            this.errorToken = this.currentToken;
            throw new RecoveryTokenException(this.currentToken.token, partialAST);
        }

        private bool TokenInList(JSToken[] tokens, JSToken token)
        {
            return (-1 != this.IndexOfToken(tokens, token));
        }

        private bool TokenInList(JSToken[] tokens, RecoveryTokenException exc)
        {
            return (-1 != this.IndexOfToken(tokens, exc._token));
        }

        internal bool HasAborted
        {
            get
            {
                return (this.tokensSkipped > 50);
            }
        }

        private enum BlockType
        {
            Block,
            Loop,
            Switch,
            Finally
        }
    }
}

