namespace System.Linq.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Dynamic.Utils;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal sealed class DebugViewWriter : ExpressionVisitor
    {
        private int _column;
        private int _delta;
        private Flow _flow;
        private Dictionary<LabelTarget, int> _labelIds;
        private Dictionary<LambdaExpression, int> _lambdaIds;
        private Queue<LambdaExpression> _lambdas;
        private TextWriter _out;
        private Dictionary<ParameterExpression, int> _paramIds;
        private Stack<int> _stack = new Stack<int>();
        private const int MaxColumn = 120;
        private const int Tab = 4;

        private DebugViewWriter(TextWriter file)
        {
            this._out = file;
        }

        private Flow CheckBreak(Flow flow)
        {
            if ((flow & Flow.Break) != Flow.None)
            {
                if (this._column > (120 + this.Depth))
                {
                    flow = Flow.NewLine;
                    return flow;
                }
                flow &= ~Flow.Break;
            }
            return flow;
        }

        private static bool ContainsWhiteSpace(string name)
        {
            foreach (char ch in name)
            {
                if (char.IsWhiteSpace(ch))
                {
                    return true;
                }
            }
            return false;
        }

        private void Dedent()
        {
            this._delta -= 4;
        }

        private void DumpLabel(LabelTarget target)
        {
            this.Out(string.Format(CultureInfo.CurrentCulture, ".LabelTarget {0}:", new object[] { this.GetLabelTargetName(target) }));
        }

        private static string FormatBinder(CallSiteBinder binder)
        {
            ConvertBinder binder2 = binder as ConvertBinder;
            if (binder2 != null)
            {
                return ("Convert " + binder2.Type.ToString());
            }
            GetMemberBinder binder3 = binder as GetMemberBinder;
            if (binder3 != null)
            {
                return ("GetMember " + binder3.Name);
            }
            SetMemberBinder binder4 = binder as SetMemberBinder;
            if (binder4 != null)
            {
                return ("SetMember " + binder4.Name);
            }
            DeleteMemberBinder binder5 = binder as DeleteMemberBinder;
            if (binder5 != null)
            {
                return ("DeleteMember " + binder5.Name);
            }
            if (binder is GetIndexBinder)
            {
                return "GetIndex";
            }
            if (binder is SetIndexBinder)
            {
                return "SetIndex";
            }
            if (binder is DeleteIndexBinder)
            {
                return "DeleteIndex";
            }
            InvokeMemberBinder binder6 = binder as InvokeMemberBinder;
            if (binder6 != null)
            {
                return ("Call " + binder6.Name);
            }
            if (binder is InvokeBinder)
            {
                return "Invoke";
            }
            if (binder is CreateInstanceBinder)
            {
                return "Create";
            }
            UnaryOperationBinder binder7 = binder as UnaryOperationBinder;
            if (binder7 != null)
            {
                return ("UnaryOperation " + binder7.Operation);
            }
            BinaryOperationBinder binder8 = binder as BinaryOperationBinder;
            if (binder8 != null)
            {
                return ("BinaryOperation " + binder8.Operation);
            }
            return binder.ToString();
        }

        private static string GetConstantValueSuffix(Type type)
        {
            if (type == typeof(uint))
            {
                return "U";
            }
            if (type == typeof(long))
            {
                return "L";
            }
            if (type == typeof(ulong))
            {
                return "UL";
            }
            if (type == typeof(double))
            {
                return "D";
            }
            if (type == typeof(float))
            {
                return "F";
            }
            if (type == typeof(decimal))
            {
                return "M";
            }
            return null;
        }

        private static string GetDisplayName(string name)
        {
            if (ContainsWhiteSpace(name))
            {
                return QuoteName(name);
            }
            return name;
        }

        private Flow GetFlow(Flow flow)
        {
            Flow flow2 = this.CheckBreak(this._flow);
            flow = this.CheckBreak(flow);
            return (Flow) Math.Max((int) flow2, (int) flow);
        }

        private static int GetId<T>(T e, ref Dictionary<T, int> ids)
        {
            int num;
            if (ids == null)
            {
                ids = new Dictionary<T, int>();
                ids.Add(e, 1);
                return 1;
            }
            if (!ids.TryGetValue(e, out num))
            {
                num = ids.Count + 1;
                ids.Add(e, num);
            }
            return num;
        }

        private int GetLabelTargetId(LabelTarget target)
        {
            return GetId<LabelTarget>(target, ref this._labelIds);
        }

        private string GetLabelTargetName(LabelTarget target)
        {
            if (string.IsNullOrEmpty(target.Name))
            {
                return string.Format(CultureInfo.CurrentCulture, "#Label{0}", new object[] { this.GetLabelTargetId(target) });
            }
            return GetDisplayName(target.Name);
        }

        private int GetLambdaId(LambdaExpression le)
        {
            return GetId<LambdaExpression>(le, ref this._lambdaIds);
        }

        private string GetLambdaName(LambdaExpression lambda)
        {
            if (string.IsNullOrEmpty(lambda.Name))
            {
                return ("#Lambda" + this.GetLambdaId(lambda));
            }
            return GetDisplayName(lambda.Name);
        }

        private static int GetOperatorPrecedence(Expression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return 10;

                case ExpressionType.And:
                    return 6;

                case ExpressionType.AndAlso:
                    return 3;

                case ExpressionType.Coalesce:
                case ExpressionType.Assign:
                case ExpressionType.AddAssign:
                case ExpressionType.AndAssign:
                case ExpressionType.DivideAssign:
                case ExpressionType.ExclusiveOrAssign:
                case ExpressionType.LeftShiftAssign:
                case ExpressionType.ModuloAssign:
                case ExpressionType.MultiplyAssign:
                case ExpressionType.OrAssign:
                case ExpressionType.PowerAssign:
                case ExpressionType.RightShiftAssign:
                case ExpressionType.SubtractAssign:
                case ExpressionType.AddAssignChecked:
                case ExpressionType.MultiplyAssignChecked:
                case ExpressionType.SubtractAssignChecked:
                    return 1;

                case ExpressionType.Constant:
                case ExpressionType.Parameter:
                    return 15;

                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.Negate:
                case ExpressionType.UnaryPlus:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Decrement:
                case ExpressionType.Increment:
                case ExpressionType.Throw:
                case ExpressionType.Unbox:
                case ExpressionType.PreIncrementAssign:
                case ExpressionType.PreDecrementAssign:
                case ExpressionType.OnesComplement:
                case ExpressionType.IsTrue:
                case ExpressionType.IsFalse:
                    return 12;

                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return 11;

                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                    return 7;

                case ExpressionType.ExclusiveOr:
                    return 5;

                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.TypeAs:
                case ExpressionType.TypeIs:
                case ExpressionType.TypeEqual:
                    return 8;

                case ExpressionType.LeftShift:
                case ExpressionType.RightShift:
                    return 9;

                case ExpressionType.Or:
                    return 4;

                case ExpressionType.OrElse:
                    return 2;

                case ExpressionType.Power:
                    return 13;
            }
            return 14;
        }

        private int GetParamId(ParameterExpression p)
        {
            return GetId<ParameterExpression>(p, ref this._paramIds);
        }

        private void Indent()
        {
            this._delta += 4;
        }

        private static bool IsSimpleExpression(Expression node)
        {
            BinaryExpression expression = node as BinaryExpression;
            if (expression == null)
            {
                return false;
            }
            return (!(expression.Left is BinaryExpression) && !(expression.Right is BinaryExpression));
        }

        private static bool NeedsParentheses(Expression parent, Expression child)
        {
            if (child == null)
            {
                return false;
            }
            switch (parent.NodeType)
            {
                case ExpressionType.IsTrue:
                case ExpressionType.IsFalse:
                case ExpressionType.Unbox:
                case ExpressionType.Decrement:
                case ExpressionType.Increment:
                    return true;

                default:
                {
                    int operatorPrecedence = GetOperatorPrecedence(child);
                    int num2 = GetOperatorPrecedence(parent);
                    if (operatorPrecedence != num2)
                    {
                        if (((child == null) || (child.NodeType != ExpressionType.Constant)) || ((parent.NodeType != ExpressionType.Negate) && (parent.NodeType != ExpressionType.NegateChecked)))
                        {
                            return (operatorPrecedence < num2);
                        }
                        return true;
                    }
                    switch (parent.NodeType)
                    {
                        case ExpressionType.Add:
                        case ExpressionType.AddChecked:
                        case ExpressionType.Multiply:
                        case ExpressionType.MultiplyChecked:
                            return false;

                        case ExpressionType.And:
                        case ExpressionType.AndAlso:
                        case ExpressionType.ExclusiveOr:
                        case ExpressionType.Or:
                        case ExpressionType.OrElse:
                            return false;

                        case ExpressionType.Divide:
                        case ExpressionType.Modulo:
                        case ExpressionType.Subtract:
                        case ExpressionType.SubtractChecked:
                        {
                            BinaryExpression expression = parent as BinaryExpression;
                            return (child == expression.Right);
                        }
                    }
                    break;
                }
            }
            return true;
        }

        private void NewLine()
        {
            this._flow = Flow.NewLine;
        }

        private void Out(string s)
        {
            this.Out(Flow.None, s, Flow.None);
        }

        private void Out(Flow before, string s)
        {
            this.Out(before, s, Flow.None);
        }

        private void Out(string s, Flow after)
        {
            this.Out(Flow.None, s, after);
        }

        private void Out(Flow before, string s, Flow after)
        {
            switch (this.GetFlow(before))
            {
                case Flow.Space:
                    this.Write(" ");
                    break;

                case Flow.NewLine:
                    this.WriteLine();
                    this.Write(new string(' ', this.Depth));
                    break;
            }
            this.Write(s);
            this._flow = after;
        }

        private void OutMember(Expression node, Expression instance, MemberInfo member)
        {
            if (instance != null)
            {
                this.ParenthesizedVisit(node, instance);
                this.Out("." + member.Name);
            }
            else
            {
                this.Out(member.DeclaringType.ToString() + "." + member.Name);
            }
        }

        private void ParenthesizedVisit(Expression parent, Expression nodeToVisit)
        {
            if (NeedsParentheses(parent, nodeToVisit))
            {
                this.Out("(");
                this.Visit(nodeToVisit);
                this.Out(")");
            }
            else
            {
                this.Visit(nodeToVisit);
            }
        }

        private static string QuoteName(string name)
        {
            return string.Format(CultureInfo.CurrentCulture, "'{0}'", new object[] { name });
        }

        protected internal override Expression VisitBinary(BinaryExpression node)
        {
            string str;
            if (node.NodeType == ExpressionType.ArrayIndex)
            {
                this.ParenthesizedVisit(node, node.Left);
                this.Out("[");
                this.Visit(node.Right);
                this.Out("]");
                return node;
            }
            bool flag = NeedsParentheses(node, node.Left);
            bool flag2 = NeedsParentheses(node, node.Right);
            bool flag3 = false;
            Flow space = Flow.Space;
            switch (node.NodeType)
            {
                case ExpressionType.Add:
                    str = "+";
                    break;

                case ExpressionType.AddChecked:
                    str = "+";
                    flag3 = true;
                    break;

                case ExpressionType.And:
                    str = "&";
                    break;

                case ExpressionType.AndAlso:
                    str = "&&";
                    space = Flow.Break | Flow.Space;
                    break;

                case ExpressionType.Coalesce:
                    str = "??";
                    break;

                case ExpressionType.Divide:
                    str = "/";
                    break;

                case ExpressionType.Equal:
                    str = "==";
                    break;

                case ExpressionType.ExclusiveOr:
                    str = "^";
                    break;

                case ExpressionType.GreaterThan:
                    str = ">";
                    break;

                case ExpressionType.GreaterThanOrEqual:
                    str = ">=";
                    break;

                case ExpressionType.LeftShift:
                    str = "<<";
                    break;

                case ExpressionType.LessThan:
                    str = "<";
                    break;

                case ExpressionType.LessThanOrEqual:
                    str = "<=";
                    break;

                case ExpressionType.Modulo:
                    str = "%";
                    break;

                case ExpressionType.Multiply:
                    str = "*";
                    break;

                case ExpressionType.MultiplyChecked:
                    str = "*";
                    flag3 = true;
                    break;

                case ExpressionType.NotEqual:
                    str = "!=";
                    break;

                case ExpressionType.Or:
                    str = "|";
                    break;

                case ExpressionType.OrElse:
                    str = "||";
                    space = Flow.Break | Flow.Space;
                    break;

                case ExpressionType.Power:
                    str = "**";
                    break;

                case ExpressionType.RightShift:
                    str = ">>";
                    break;

                case ExpressionType.Subtract:
                    str = "-";
                    break;

                case ExpressionType.SubtractChecked:
                    str = "-";
                    flag3 = true;
                    break;

                case ExpressionType.Assign:
                    str = "=";
                    break;

                case ExpressionType.AddAssign:
                    str = "+=";
                    break;

                case ExpressionType.AndAssign:
                    str = "&=";
                    break;

                case ExpressionType.DivideAssign:
                    str = "/=";
                    break;

                case ExpressionType.ExclusiveOrAssign:
                    str = "^=";
                    break;

                case ExpressionType.LeftShiftAssign:
                    str = "<<=";
                    break;

                case ExpressionType.ModuloAssign:
                    str = "%=";
                    break;

                case ExpressionType.MultiplyAssign:
                    str = "*=";
                    break;

                case ExpressionType.OrAssign:
                    str = "|=";
                    break;

                case ExpressionType.PowerAssign:
                    str = "**=";
                    break;

                case ExpressionType.RightShiftAssign:
                    str = ">>=";
                    break;

                case ExpressionType.SubtractAssign:
                    str = "-=";
                    break;

                case ExpressionType.AddAssignChecked:
                    str = "+=";
                    flag3 = true;
                    break;

                case ExpressionType.MultiplyAssignChecked:
                    str = "*=";
                    flag3 = true;
                    break;

                case ExpressionType.SubtractAssignChecked:
                    str = "-=";
                    flag3 = true;
                    break;

                default:
                    throw new InvalidOperationException();
            }
            if (flag)
            {
                this.Out("(", Flow.None);
            }
            this.Visit(node.Left);
            if (flag)
            {
                this.Out(Flow.None, ")", Flow.Break);
            }
            if (flag3)
            {
                str = string.Format(CultureInfo.CurrentCulture, "#{0}", new object[] { str });
            }
            this.Out(space, str, Flow.Break | Flow.Space);
            if (flag2)
            {
                this.Out("(", Flow.None);
            }
            this.Visit(node.Right);
            if (flag2)
            {
                this.Out(Flow.None, ")", Flow.Break);
            }
            return node;
        }

        protected internal override Expression VisitBlock(BlockExpression node)
        {
            this.Out(".Block");
            if (node.Type != node.GetExpression(node.ExpressionCount - 1).Type)
            {
                this.Out(string.Format(CultureInfo.CurrentCulture, "<{0}>", new object[] { node.Type.ToString() }));
            }
            this.VisitDeclarations(node.Variables);
            this.Out(" ");
            this.VisitExpressions<Expression>('{', ';', node.Expressions);
            return node;
        }

        protected override CatchBlock VisitCatchBlock(CatchBlock node)
        {
            this.Out(Flow.NewLine, "} .Catch (" + node.Test.ToString());
            if (node.Variable != null)
            {
                this.Out(Flow.Space, "");
                this.VisitParameter(node.Variable);
            }
            if (node.Filter != null)
            {
                this.Out(") .If (", Flow.Break);
                this.Visit(node.Filter);
            }
            this.Out(") {", Flow.NewLine);
            this.Indent();
            this.Visit(node.Body);
            this.Dedent();
            return node;
        }

        protected internal override Expression VisitConditional(ConditionalExpression node)
        {
            if (IsSimpleExpression(node.Test))
            {
                this.Out(".If (");
                this.Visit(node.Test);
                this.Out(") {", Flow.NewLine);
            }
            else
            {
                this.Out(".If (", Flow.NewLine);
                this.Indent();
                this.Visit(node.Test);
                this.Dedent();
                this.Out(Flow.NewLine, ") {", Flow.NewLine);
            }
            this.Indent();
            this.Visit(node.IfTrue);
            this.Dedent();
            this.Out(Flow.NewLine, "} .Else {", Flow.NewLine);
            this.Indent();
            this.Visit(node.IfFalse);
            this.Dedent();
            this.Out(Flow.NewLine, "}");
            return node;
        }

        protected internal override Expression VisitConstant(ConstantExpression node)
        {
            object obj2 = node.Value;
            if (obj2 == null)
            {
                this.Out("null");
                return node;
            }
            if ((obj2 is string) && (node.Type == typeof(string)))
            {
                this.Out(string.Format(CultureInfo.CurrentCulture, "\"{0}\"", new object[] { obj2 }));
                return node;
            }
            if ((obj2 is char) && (node.Type == typeof(char)))
            {
                this.Out(string.Format(CultureInfo.CurrentCulture, "'{0}'", new object[] { obj2 }));
                return node;
            }
            if (((obj2 is int) && (node.Type == typeof(int))) || ((obj2 is bool) && (node.Type == typeof(bool))))
            {
                this.Out(obj2.ToString());
                return node;
            }
            string constantValueSuffix = GetConstantValueSuffix(node.Type);
            if (constantValueSuffix != null)
            {
                this.Out(obj2.ToString());
                this.Out(constantValueSuffix);
                return node;
            }
            this.Out(string.Format(CultureInfo.CurrentCulture, ".Constant<{0}>({1})", new object[] { node.Type.ToString(), obj2 }));
            return node;
        }

        protected internal override Expression VisitDebugInfo(DebugInfoExpression node)
        {
            this.Out(string.Format(CultureInfo.CurrentCulture, ".DebugInfo({0}: {1}, {2} - {3}, {4})", new object[] { node.Document.FileName, node.StartLine, node.StartColumn, node.EndLine, node.EndColumn }));
            return node;
        }

        private void VisitDeclarations(IList<ParameterExpression> expressions)
        {
            this.VisitExpressions<ParameterExpression>('(', ',', expressions, delegate (ParameterExpression variable) {
                this.Out(variable.Type.ToString());
                if (variable.IsByRef)
                {
                    this.Out("&");
                }
                this.Out(" ");
                this.VisitParameter(variable);
            });
        }

        protected internal override Expression VisitDefault(DefaultExpression node)
        {
            this.Out(".Default(" + node.Type.ToString() + ")");
            return node;
        }

        protected internal override Expression VisitDynamic(DynamicExpression node)
        {
            this.Out(".Dynamic", Flow.Space);
            this.Out(FormatBinder(node.Binder));
            this.VisitExpressions<Expression>('(', node.Arguments);
            return node;
        }

        protected override ElementInit VisitElementInit(ElementInit node)
        {
            if (node.Arguments.Count == 1)
            {
                this.Visit(node.Arguments[0]);
                return node;
            }
            this.VisitExpressions<Expression>('{', node.Arguments);
            return node;
        }

        private void VisitExpressions<T>(char open, IList<T> expressions) where T: Expression
        {
            this.VisitExpressions<T>(open, ',', expressions);
        }

        private void VisitExpressions<T>(char open, char separator, IList<T> expressions) where T: Expression
        {
            this.VisitExpressions<T>(open, separator, expressions, e => this.Visit(e));
        }

        private void VisitExpressions<T>(char open, char separator, IList<T> expressions, Action<T> visit)
        {
            char ch;
            this.Out(open.ToString());
            if (expressions != null)
            {
                this.Indent();
                bool flag = true;
                foreach (T local in expressions)
                {
                    if (flag)
                    {
                        if ((open == '{') || (expressions.Count > 1))
                        {
                            this.NewLine();
                        }
                        flag = false;
                    }
                    else
                    {
                        this.Out(separator.ToString(), Flow.NewLine);
                    }
                    visit(local);
                }
                this.Dedent();
            }
            switch (open)
            {
                case '[':
                    ch = ']';
                    break;

                case '{':
                    ch = '}';
                    break;

                case '(':
                    ch = ')';
                    break;

                case '<':
                    ch = '>';
                    break;

                default:
                    throw ContractUtils.Unreachable;
            }
            if (open == '{')
            {
                this.NewLine();
            }
            this.Out(ch.ToString(), Flow.Break);
        }

        protected internal override Expression VisitExtension(Expression node)
        {
            this.Out(string.Format(CultureInfo.CurrentCulture, ".Extension<{0}>", new object[] { node.GetType().ToString() }));
            if (node.CanReduce)
            {
                this.Out(Flow.Space, "{", Flow.NewLine);
                this.Indent();
                this.Visit(node.Reduce());
                this.Dedent();
                this.Out(Flow.NewLine, "}");
            }
            return node;
        }

        protected internal override Expression VisitGoto(GotoExpression node)
        {
            this.Out("." + node.Kind.ToString(), Flow.Space);
            this.Out(this.GetLabelTargetName(node.Target), Flow.Space);
            this.Out("{", Flow.Space);
            this.Visit(node.Value);
            this.Out(Flow.Space, "}");
            return node;
        }

        protected internal override Expression VisitIndex(IndexExpression node)
        {
            if (node.Indexer != null)
            {
                this.OutMember(node, node.Object, node.Indexer);
            }
            else
            {
                this.ParenthesizedVisit(node, node.Object);
            }
            this.VisitExpressions<Expression>('[', node.Arguments);
            return node;
        }

        protected internal override Expression VisitInvocation(InvocationExpression node)
        {
            this.Out(".Invoke ");
            this.ParenthesizedVisit(node, node.Expression);
            this.VisitExpressions<Expression>('(', node.Arguments);
            return node;
        }

        protected internal override Expression VisitLabel(LabelExpression node)
        {
            this.Out(".Label", Flow.NewLine);
            this.Indent();
            this.Visit(node.DefaultValue);
            this.Dedent();
            this.NewLine();
            this.DumpLabel(node.Target);
            return node;
        }

        protected internal override Expression VisitLambda<T>(Expression<T> node)
        {
            this.Out(string.Format(CultureInfo.CurrentCulture, "{0} {1}<{2}>", new object[] { ".Lambda", this.GetLambdaName(node), node.Type.ToString() }));
            if (this._lambdas == null)
            {
                this._lambdas = new Queue<LambdaExpression>();
            }
            if (!this._lambdas.Contains(node))
            {
                this._lambdas.Enqueue(node);
            }
            return node;
        }

        protected internal override Expression VisitListInit(ListInitExpression node)
        {
            this.Visit(node.NewExpression);
            this.VisitExpressions<ElementInit>('{', ',', node.Initializers, delegate (ElementInit e) {
                this.VisitElementInit(e);
            });
            return node;
        }

        protected internal override Expression VisitLoop(LoopExpression node)
        {
            this.Out(".Loop", Flow.Space);
            if (node.ContinueLabel != null)
            {
                this.DumpLabel(node.ContinueLabel);
            }
            this.Out(" {", Flow.NewLine);
            this.Indent();
            this.Visit(node.Body);
            this.Dedent();
            this.Out(Flow.NewLine, "}");
            if (node.BreakLabel != null)
            {
                this.Out("", Flow.NewLine);
                this.DumpLabel(node.BreakLabel);
            }
            return node;
        }

        protected internal override Expression VisitMember(MemberExpression node)
        {
            this.OutMember(node, node.Expression, node.Member);
            return node;
        }

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
        {
            this.Out(assignment.Member.Name);
            this.Out(Flow.Space, "=", Flow.Space);
            this.Visit(assignment.Expression);
            return assignment;
        }

        protected internal override Expression VisitMemberInit(MemberInitExpression node)
        {
            this.Visit(node.NewExpression);
            this.VisitExpressions<MemberBinding>('{', ',', node.Bindings, delegate (MemberBinding e) {
                this.VisitMemberBinding(e);
            });
            return node;
        }

        protected override MemberListBinding VisitMemberListBinding(MemberListBinding binding)
        {
            this.Out(binding.Member.Name);
            this.Out(Flow.Space, "=", Flow.Space);
            this.VisitExpressions<ElementInit>('{', ',', binding.Initializers, delegate (ElementInit e) {
                this.VisitElementInit(e);
            });
            return binding;
        }

        protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
        {
            this.Out(binding.Member.Name);
            this.Out(Flow.Space, "=", Flow.Space);
            this.VisitExpressions<MemberBinding>('{', ',', binding.Bindings, delegate (MemberBinding e) {
                this.VisitMemberBinding(e);
            });
            return binding;
        }

        protected internal override Expression VisitMethodCall(MethodCallExpression node)
        {
            this.Out(".Call ");
            if (node.Object != null)
            {
                this.ParenthesizedVisit(node, node.Object);
            }
            else if (node.Method.DeclaringType != null)
            {
                this.Out(node.Method.DeclaringType.ToString());
            }
            else
            {
                this.Out("<UnknownType>");
            }
            this.Out(".");
            this.Out(node.Method.Name);
            this.VisitExpressions<Expression>('(', node.Arguments);
            return node;
        }

        protected internal override Expression VisitNew(NewExpression node)
        {
            this.Out(".New " + node.Type.ToString());
            this.VisitExpressions<Expression>('(', node.Arguments);
            return node;
        }

        protected internal override Expression VisitNewArray(NewArrayExpression node)
        {
            if (node.NodeType == ExpressionType.NewArrayBounds)
            {
                this.Out(".NewArray " + node.Type.GetElementType().ToString());
                this.VisitExpressions<Expression>('[', node.Expressions);
                return node;
            }
            this.Out(".NewArray " + node.Type.ToString(), Flow.Space);
            this.VisitExpressions<Expression>('{', node.Expressions);
            return node;
        }

        protected internal override Expression VisitParameter(ParameterExpression node)
        {
            this.Out("$");
            if (string.IsNullOrEmpty(node.Name))
            {
                this.Out("var" + this.GetParamId(node));
                return node;
            }
            this.Out(GetDisplayName(node.Name));
            return node;
        }

        protected internal override Expression VisitRuntimeVariables(RuntimeVariablesExpression node)
        {
            this.Out(".RuntimeVariables");
            this.VisitExpressions<ParameterExpression>('(', node.Variables);
            return node;
        }

        protected internal override Expression VisitSwitch(SwitchExpression node)
        {
            this.Out(".Switch ");
            this.Out("(");
            this.Visit(node.SwitchValue);
            this.Out(") {", Flow.NewLine);
            ExpressionVisitor.Visit<SwitchCase>(node.Cases, new Func<SwitchCase, SwitchCase>(this.VisitSwitchCase));
            if (node.DefaultBody != null)
            {
                this.Out(".Default:", Flow.NewLine);
                this.Indent();
                this.Indent();
                this.Visit(node.DefaultBody);
                this.Dedent();
                this.Dedent();
                this.NewLine();
            }
            this.Out("}");
            return node;
        }

        protected override SwitchCase VisitSwitchCase(SwitchCase node)
        {
            foreach (Expression expression in node.TestValues)
            {
                this.Out(".Case (");
                this.Visit(expression);
                this.Out("):", Flow.NewLine);
            }
            this.Indent();
            this.Indent();
            this.Visit(node.Body);
            this.Dedent();
            this.Dedent();
            this.NewLine();
            return node;
        }

        protected internal override Expression VisitTry(TryExpression node)
        {
            this.Out(".Try {", Flow.NewLine);
            this.Indent();
            this.Visit(node.Body);
            this.Dedent();
            ExpressionVisitor.Visit<CatchBlock>(node.Handlers, new Func<CatchBlock, CatchBlock>(this.VisitCatchBlock));
            if (node.Finally != null)
            {
                this.Out(Flow.NewLine, "} .Finally {", Flow.NewLine);
                this.Indent();
                this.Visit(node.Finally);
                this.Dedent();
            }
            else if (node.Fault != null)
            {
                this.Out(Flow.NewLine, "} .Fault {", Flow.NewLine);
                this.Indent();
                this.Visit(node.Fault);
                this.Dedent();
            }
            this.Out(Flow.NewLine, "}");
            return node;
        }

        protected internal override Expression VisitTypeBinary(TypeBinaryExpression node)
        {
            this.ParenthesizedVisit(node, node.Expression);
            switch (node.NodeType)
            {
                case ExpressionType.TypeIs:
                    this.Out(Flow.Space, ".Is", Flow.Space);
                    break;

                case ExpressionType.TypeEqual:
                    this.Out(Flow.Space, ".TypeEqual", Flow.Space);
                    break;
            }
            this.Out(node.TypeOperand.ToString());
            return node;
        }

        protected internal override Expression VisitUnary(UnaryExpression node)
        {
            NeedsParentheses(node, node.Operand);
            ExpressionType nodeType = node.NodeType;
            if (nodeType <= ExpressionType.Quote)
            {
                switch (nodeType)
                {
                    case ExpressionType.Negate:
                        this.Out("-");
                        break;

                    case ExpressionType.UnaryPlus:
                        this.Out("+");
                        break;

                    case ExpressionType.NegateChecked:
                        this.Out("#-");
                        break;

                    case ExpressionType.Not:
                        this.Out((node.Type == typeof(bool)) ? "!" : "~");
                        break;

                    case ExpressionType.Quote:
                        this.Out("'");
                        break;

                    case ExpressionType.Convert:
                        this.Out("(" + node.Type.ToString() + ")");
                        break;

                    case ExpressionType.ConvertChecked:
                        this.Out("#(" + node.Type.ToString() + ")");
                        break;
                }
            }
            else if (nodeType <= ExpressionType.Decrement)
            {
                if ((nodeType != ExpressionType.TypeAs) && (nodeType == ExpressionType.Decrement))
                {
                    this.Out(".Decrement");
                }
            }
            else
            {
                switch (nodeType)
                {
                    case ExpressionType.Throw:
                        if (node.Operand != null)
                        {
                            this.Out(".Throw", Flow.Space);
                            break;
                        }
                        this.Out(".Rethrow");
                        break;

                    case ExpressionType.Unbox:
                        this.Out(".Unbox");
                        break;

                    case ExpressionType.Increment:
                        this.Out(".Increment");
                        break;

                    case ExpressionType.PreIncrementAssign:
                        this.Out("++");
                        break;

                    case ExpressionType.PreDecrementAssign:
                        this.Out("--");
                        break;

                    case ExpressionType.OnesComplement:
                        this.Out("~");
                        break;

                    case ExpressionType.IsTrue:
                        this.Out(".IsTrue");
                        break;

                    case ExpressionType.IsFalse:
                        this.Out(".IsFalse");
                        break;
                }
            }
            this.ParenthesizedVisit(node, node.Operand);
            switch (node.NodeType)
            {
                case ExpressionType.PostIncrementAssign:
                    this.Out("++");
                    return node;

                case ExpressionType.PostDecrementAssign:
                    this.Out("--");
                    return node;

                case ExpressionType.TypeAs:
                    this.Out(Flow.Space, ".As", Flow.Break | Flow.Space);
                    this.Out(node.Type.ToString());
                    return node;

                case ExpressionType.ArrayLength:
                    this.Out(".Length");
                    return node;
            }
            return node;
        }

        private void Write(string s)
        {
            this._out.Write(s);
            this._column += s.Length;
        }

        private void WriteLambda(LambdaExpression lambda)
        {
            this.Out(string.Format(CultureInfo.CurrentCulture, ".Lambda {0}<{1}>", new object[] { this.GetLambdaName(lambda), lambda.Type.ToString() }));
            this.VisitDeclarations(lambda.Parameters);
            this.Out(Flow.Space, "{", Flow.NewLine);
            this.Indent();
            this.Visit(lambda.Body);
            this.Dedent();
            this.Out(Flow.NewLine, "}");
        }

        private void WriteLine()
        {
            this._out.WriteLine();
            this._column = 0;
        }

        private void WriteTo(Expression node)
        {
            LambdaExpression lambda = node as LambdaExpression;
            if (lambda != null)
            {
                this.WriteLambda(lambda);
            }
            else
            {
                this.Visit(node);
            }
            while ((this._lambdas != null) && (this._lambdas.Count > 0))
            {
                this.WriteLine();
                this.WriteLine();
                this.WriteLambda(this._lambdas.Dequeue());
            }
        }

        internal static void WriteTo(Expression node, TextWriter writer)
        {
            new DebugViewWriter(writer).WriteTo(node);
        }

        private int Base
        {
            get
            {
                if (this._stack.Count <= 0)
                {
                    return 0;
                }
                return this._stack.Peek();
            }
        }

        private int Delta
        {
            get
            {
                return this._delta;
            }
        }

        private int Depth
        {
            get
            {
                return (this.Base + this.Delta);
            }
        }

        [Flags]
        private enum Flow
        {
            Break = 0x8000,
            NewLine = 2,
            None = 0,
            Space = 1
        }
    }
}

