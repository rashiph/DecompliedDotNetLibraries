namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Xml.XPath;

    internal sealed class QueryBuilder
    {
        private bool allowCurrent;
        private bool allowKey;
        private bool allowVar;
        private BaseAxisQuery firstInput;
        private bool needContext;
        private string query;

        private Query Build(AstNode root, string query)
        {
            Props props;
            this.Reset();
            this.query = query;
            return this.ProcessNode(root, Flags.None, out props);
        }

        internal Query Build(string query, out bool needContext)
        {
            Query query2 = this.Build(query, true, true);
            needContext = this.needContext;
            return query2;
        }

        internal Query Build(string query, bool allowVar, bool allowKey)
        {
            this.allowVar = allowVar;
            this.allowKey = allowKey;
            this.allowCurrent = true;
            return this.Build(XPathParser.ParseXPathExpresion(query), query);
        }

        internal Query BuildPatternQuery(string query, out bool needContext)
        {
            Query query2 = this.BuildPatternQuery(query, true, true);
            needContext = this.needContext;
            return query2;
        }

        internal Query BuildPatternQuery(string query, bool allowVar, bool allowKey)
        {
            this.allowVar = allowVar;
            this.allowKey = allowKey;
            this.allowCurrent = false;
            return this.Build(XPathParser.ParseXPathPattern(query), query);
        }

        private bool CanBeNumber(Query q)
        {
            if (q.StaticType != XPathResultType.Any)
            {
                return (q.StaticType == XPathResultType.Number);
            }
            return true;
        }

        private List<Query> ProcessArguments(ArrayList args, out Props props)
        {
            int capacity = (args != null) ? args.Count : 0;
            List<Query> list = new List<Query>(capacity);
            props = Props.None;
            for (int i = 0; i < capacity; i++)
            {
                Props props2;
                list.Add(this.ProcessNode((AstNode) args[i], Flags.None, out props2));
                props |= props2;
            }
            return list;
        }

        private Query ProcessAxis(Axis root, Flags flags, out Props props)
        {
            Query qyParent = null;
            Query query2;
            if (root.Prefix.Length > 0)
            {
                this.needContext = true;
            }
            this.firstInput = null;
            if (root.Input != null)
            {
                Flags none = Flags.None;
                if ((flags & Flags.PosFilter) == Flags.None)
                {
                    Axis input = root.Input as Axis;
                    if (((input != null) && (root.TypeOfAxis == Axis.AxisType.Child)) && ((input.TypeOfAxis == Axis.AxisType.DescendantOrSelf) && (input.NodeType == XPathNodeType.All)))
                    {
                        Query query3;
                        if (input.Input != null)
                        {
                            query3 = this.ProcessNode(input.Input, Flags.SmartDesc, out props);
                        }
                        else
                        {
                            query3 = new ContextQuery();
                            props = Props.None;
                        }
                        qyParent = new DescendantQuery(query3, root.Name, root.Prefix, root.NodeType, false, input.AbbrAxis);
                        if ((props & Props.NonFlat) != Props.None)
                        {
                            qyParent = new DocumentOrderQuery(qyParent);
                        }
                        props |= Props.NonFlat;
                        return qyParent;
                    }
                    if ((root.TypeOfAxis == Axis.AxisType.Descendant) || (root.TypeOfAxis == Axis.AxisType.DescendantOrSelf))
                    {
                        none |= Flags.SmartDesc;
                    }
                }
                query2 = this.ProcessNode(root.Input, none, out props);
            }
            else
            {
                query2 = new ContextQuery();
                props = Props.None;
            }
            switch (root.TypeOfAxis)
            {
                case Axis.AxisType.Ancestor:
                    qyParent = new XPathAncestorQuery(query2, root.Name, root.Prefix, root.NodeType, false);
                    props |= Props.NonFlat;
                    return qyParent;

                case Axis.AxisType.AncestorOrSelf:
                    qyParent = new XPathAncestorQuery(query2, root.Name, root.Prefix, root.NodeType, true);
                    props |= Props.NonFlat;
                    return qyParent;

                case Axis.AxisType.Attribute:
                    return new AttributeQuery(query2, root.Name, root.Prefix, root.NodeType);

                case Axis.AxisType.Child:
                    if ((props & Props.NonFlat) == Props.None)
                    {
                        return new ChildrenQuery(query2, root.Name, root.Prefix, root.NodeType);
                    }
                    return new CacheChildrenQuery(query2, root.Name, root.Prefix, root.NodeType);

                case Axis.AxisType.Descendant:
                    if ((flags & Flags.SmartDesc) == Flags.None)
                    {
                        qyParent = new DescendantQuery(query2, root.Name, root.Prefix, root.NodeType, false, false);
                        if ((props & Props.NonFlat) != Props.None)
                        {
                            qyParent = new DocumentOrderQuery(qyParent);
                        }
                        break;
                    }
                    qyParent = new DescendantOverDescendantQuery(query2, false, root.Name, root.Prefix, root.NodeType, false);
                    break;

                case Axis.AxisType.DescendantOrSelf:
                    if ((flags & Flags.SmartDesc) == Flags.None)
                    {
                        qyParent = new DescendantQuery(query2, root.Name, root.Prefix, root.NodeType, true, root.AbbrAxis);
                        if ((props & Props.NonFlat) != Props.None)
                        {
                            qyParent = new DocumentOrderQuery(qyParent);
                        }
                    }
                    else
                    {
                        qyParent = new DescendantOverDescendantQuery(query2, true, root.Name, root.Prefix, root.NodeType, root.AbbrAxis);
                    }
                    props |= Props.NonFlat;
                    return qyParent;

                case Axis.AxisType.Following:
                    qyParent = new FollowingQuery(query2, root.Name, root.Prefix, root.NodeType);
                    props |= Props.NonFlat;
                    return qyParent;

                case Axis.AxisType.FollowingSibling:
                    qyParent = new FollSiblingQuery(query2, root.Name, root.Prefix, root.NodeType);
                    if ((props & Props.NonFlat) != Props.None)
                    {
                        qyParent = new DocumentOrderQuery(qyParent);
                    }
                    return qyParent;

                case Axis.AxisType.Namespace:
                    if ((((root.NodeType == XPathNodeType.All) || (root.NodeType == XPathNodeType.Element)) || (root.NodeType == XPathNodeType.Attribute)) && (root.Prefix.Length == 0))
                    {
                        return new NamespaceQuery(query2, root.Name, root.Prefix, root.NodeType);
                    }
                    return new EmptyQuery();

                case Axis.AxisType.Parent:
                    return new ParentQuery(query2, root.Name, root.Prefix, root.NodeType);

                case Axis.AxisType.Preceding:
                    qyParent = new PrecedingQuery(query2, root.Name, root.Prefix, root.NodeType);
                    props |= Props.NonFlat;
                    return qyParent;

                case Axis.AxisType.PrecedingSibling:
                    return new PreSiblingQuery(query2, root.Name, root.Prefix, root.NodeType);

                case Axis.AxisType.Self:
                    return new XPathSelfQuery(query2, root.Name, root.Prefix, root.NodeType);

                default:
                    throw XPathException.Create("Xp_NotSupported", this.query);
            }
            props |= Props.NonFlat;
            return qyParent;
        }

        private Query ProcessFilter(MS.Internal.Xml.XPath.Filter root, Flags flags, out Props props)
        {
            Props props2;
            bool flag = (flags & Flags.Filter) == Flags.None;
            Query q = this.ProcessNode(root.Condition, Flags.None, out props2);
            if (this.CanBeNumber(q) || ((props2 & (Props.HasLast | Props.HasPosition)) != Props.None))
            {
                props2 |= Props.HasPosition;
                flags |= Flags.PosFilter;
            }
            flags &= ~Flags.SmartDesc;
            Query input = this.ProcessNode(root.Input, flags | Flags.Filter, out props);
            if (root.Input.Type != AstNode.AstType.Filter)
            {
                props &= ~Props.PosFilter;
            }
            if ((props2 & Props.HasPosition) != Props.None)
            {
                props |= Props.PosFilter;
            }
            FilterQuery query3 = input as FilterQuery;
            if (((query3 != null) && ((props2 & Props.HasPosition) == Props.None)) && (query3.Condition.StaticType != XPathResultType.Any))
            {
                Query condition = query3.Condition;
                if (condition.StaticType == XPathResultType.Number)
                {
                    condition = new LogicalExpr(Operator.Op.EQ, new NodeFunctions(Function.FunctionType.FuncPosition, null), condition);
                }
                q = new BooleanExpr(Operator.Op.AND, condition, q);
                input = query3.qyInput;
            }
            if (((props & Props.PosFilter) != Props.None) && (input is DocumentOrderQuery))
            {
                input = ((DocumentOrderQuery) input).input;
            }
            if (this.firstInput == null)
            {
                this.firstInput = input as BaseAxisQuery;
            }
            bool flag2 = (input.Properties & QueryProps.Merge) != QueryProps.None;
            bool flag3 = (input.Properties & QueryProps.Reverse) != QueryProps.None;
            if ((props2 & Props.HasPosition) != Props.None)
            {
                if (flag3)
                {
                    input = new ReversePositionQuery(input);
                }
                else if ((props2 & Props.HasLast) != Props.None)
                {
                    input = new ForwardPositionQuery(input);
                }
            }
            if (flag && (this.firstInput != null))
            {
                if (flag2 && ((props & Props.PosFilter) != Props.None))
                {
                    input = new FilterQuery(input, q, false);
                    Query qyInput = this.firstInput.qyInput;
                    if (!(qyInput is ContextQuery))
                    {
                        this.firstInput.qyInput = new ContextQuery();
                        this.firstInput = null;
                        return new MergeFilterQuery(qyInput, input);
                    }
                    this.firstInput = null;
                    return input;
                }
                this.firstInput = null;
            }
            return new FilterQuery(input, q, (props2 & Props.HasPosition) == Props.None);
        }

        private Query ProcessFunction(Function root, out Props props)
        {
            props = Props.None;
            Query query = null;
            switch (root.TypeOfFunction)
            {
                case Function.FunctionType.FuncLast:
                    query = new NodeFunctions(root.TypeOfFunction, null);
                    props |= Props.HasLast;
                    return query;

                case Function.FunctionType.FuncPosition:
                    query = new NodeFunctions(root.TypeOfFunction, null);
                    props |= Props.HasPosition;
                    return query;

                case Function.FunctionType.FuncCount:
                    return new NodeFunctions(Function.FunctionType.FuncCount, this.ProcessNode((AstNode) root.ArgumentList[0], Flags.None, out props));

                case Function.FunctionType.FuncID:
                    query = new IDQuery(this.ProcessNode((AstNode) root.ArgumentList[0], Flags.None, out props));
                    props |= Props.NonFlat;
                    return query;

                case Function.FunctionType.FuncLocalName:
                case Function.FunctionType.FuncNameSpaceUri:
                case Function.FunctionType.FuncName:
                    if ((root.ArgumentList == null) || (root.ArgumentList.Count <= 0))
                    {
                        return new NodeFunctions(root.TypeOfFunction, null);
                    }
                    return new NodeFunctions(root.TypeOfFunction, this.ProcessNode((AstNode) root.ArgumentList[0], Flags.None, out props));

                case Function.FunctionType.FuncString:
                case Function.FunctionType.FuncConcat:
                case Function.FunctionType.FuncStartsWith:
                case Function.FunctionType.FuncContains:
                case Function.FunctionType.FuncSubstringBefore:
                case Function.FunctionType.FuncSubstringAfter:
                case Function.FunctionType.FuncSubstring:
                case Function.FunctionType.FuncStringLength:
                case Function.FunctionType.FuncNormalize:
                case Function.FunctionType.FuncTranslate:
                    return new StringFunctions(root.TypeOfFunction, this.ProcessArguments(root.ArgumentList, out props));

                case Function.FunctionType.FuncBoolean:
                case Function.FunctionType.FuncNot:
                case Function.FunctionType.FuncLang:
                    return new BooleanFunctions(root.TypeOfFunction, this.ProcessNode((AstNode) root.ArgumentList[0], Flags.None, out props));

                case Function.FunctionType.FuncNumber:
                case Function.FunctionType.FuncSum:
                case Function.FunctionType.FuncFloor:
                case Function.FunctionType.FuncCeiling:
                case Function.FunctionType.FuncRound:
                    if ((root.ArgumentList == null) || (root.ArgumentList.Count <= 0))
                    {
                        return new NumberFunctions(Function.FunctionType.FuncNumber, null);
                    }
                    return new NumberFunctions(root.TypeOfFunction, this.ProcessNode((AstNode) root.ArgumentList[0], Flags.None, out props));

                case Function.FunctionType.FuncTrue:
                case Function.FunctionType.FuncFalse:
                    return new BooleanFunctions(root.TypeOfFunction, null);

                case Function.FunctionType.FuncUserDefined:
                    this.needContext = true;
                    if ((!this.allowCurrent && (root.Name == "current")) && (root.Prefix.Length == 0))
                    {
                        throw XPathException.Create("Xp_CurrentNotAllowed");
                    }
                    if ((!this.allowKey && (root.Name == "key")) && (root.Prefix.Length == 0))
                    {
                        throw XPathException.Create("Xp_InvalidKeyPattern", this.query);
                    }
                    query = new FunctionQuery(root.Prefix, root.Name, this.ProcessArguments(root.ArgumentList, out props));
                    props |= Props.NonFlat;
                    return query;
            }
            throw XPathException.Create("Xp_NotSupported", this.query);
        }

        private Query ProcessNode(AstNode root, Flags flags, out Props props)
        {
            props = Props.None;
            switch (root.Type)
            {
                case AstNode.AstType.Axis:
                    return this.ProcessAxis((Axis) root, flags, out props);

                case AstNode.AstType.Operator:
                    return this.ProcessOperator((Operator) root, out props);

                case AstNode.AstType.Filter:
                    return this.ProcessFilter((MS.Internal.Xml.XPath.Filter) root, flags, out props);

                case AstNode.AstType.ConstantOperand:
                    return new OperandQuery(((Operand) root).OperandValue);

                case AstNode.AstType.Function:
                    return this.ProcessFunction((Function) root, out props);

                case AstNode.AstType.Group:
                    return new GroupQuery(this.ProcessNode(((Group) root).GroupNode, Flags.None, out props));

                case AstNode.AstType.Root:
                    return new AbsoluteQuery();

                case AstNode.AstType.Variable:
                    return this.ProcessVariable((Variable) root);
            }
            return null;
        }

        private Query ProcessOperator(Operator root, out Props props)
        {
            Props props2;
            Props props3;
            Query query = this.ProcessNode(root.Operand1, Flags.None, out props2);
            Query query2 = this.ProcessNode(root.Operand2, Flags.None, out props3);
            props = props2 | props3;
            switch (root.OperatorType)
            {
                case Operator.Op.OR:
                case Operator.Op.AND:
                    return new BooleanExpr(root.OperatorType, query, query2);

                case Operator.Op.EQ:
                case Operator.Op.NE:
                case Operator.Op.LT:
                case Operator.Op.LE:
                case Operator.Op.GT:
                case Operator.Op.GE:
                    return new LogicalExpr(root.OperatorType, query, query2);

                case Operator.Op.PLUS:
                case Operator.Op.MINUS:
                case Operator.Op.MUL:
                case Operator.Op.DIV:
                case Operator.Op.MOD:
                    return new NumericExpr(root.OperatorType, query, query2);

                case Operator.Op.UNION:
                    props |= Props.NonFlat;
                    return new UnionExpr(query, query2);
            }
            return null;
        }

        private Query ProcessVariable(Variable root)
        {
            this.needContext = true;
            if (!this.allowVar)
            {
                throw XPathException.Create("Xp_InvalidKeyPattern", this.query);
            }
            return new VariableQuery(root.Localname, root.Prefix);
        }

        private void Reset()
        {
            this.needContext = false;
        }

        private enum Flags
        {
            Filter = 4,
            None = 0,
            PosFilter = 2,
            SmartDesc = 1
        }

        private enum Props
        {
            HasLast = 4,
            HasPosition = 2,
            None = 0,
            NonFlat = 8,
            PosFilter = 1
        }
    }
}

