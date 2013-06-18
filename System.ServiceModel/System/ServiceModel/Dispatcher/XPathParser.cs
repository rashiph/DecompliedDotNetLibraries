namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.Xml;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    internal class XPathParser
    {
        private XsltContext context;
        private IFunctionLibrary[] functionLibraries;
        private XPathLexer lexer;
        private XmlNamespaceManager namespaces;
        private XPathToken readToken;

        internal XPathParser(string xpath, XmlNamespaceManager namespaces, IFunctionLibrary[] functionLibraries)
        {
            this.functionLibraries = functionLibraries;
            this.namespaces = namespaces;
            this.lexer = new XPathLexer(xpath);
            this.context = namespaces as XsltContext;
        }

        private XPathExpr EnsureReturnsNodeSet(XPathExpr expr)
        {
            if (expr.ReturnType != ValueDataType.Sequence)
            {
                this.ThrowError(QueryCompileError.InvalidFunction);
            }
            return expr;
        }

        private XPathToken NextToken()
        {
            if (this.readToken == null)
            {
                while (this.lexer.MoveNext())
                {
                    if (XPathTokenID.Whitespace != this.lexer.Token.TokenID)
                    {
                        return this.lexer.Token;
                    }
                }
                return null;
            }
            XPathToken readToken = this.readToken;
            this.readToken = null;
            return readToken;
        }

        private XPathToken NextToken(XPathTokenID id)
        {
            XPathToken token = this.NextToken();
            if (token != null)
            {
                if (id == token.TokenID)
                {
                    return token;
                }
                this.readToken = token;
            }
            return null;
        }

        private XPathToken NextToken(XPathTokenID id, QueryCompileError error)
        {
            XPathToken token = this.NextToken(id);
            if (token == null)
            {
                this.ThrowError(error);
            }
            return token;
        }

        private XPathToken NextTokenClass(XPathTokenID tokenClass)
        {
            XPathToken token = this.NextToken();
            if (token != null)
            {
                if ((token.TokenID & tokenClass) != XPathTokenID.Unknown)
                {
                    return token;
                }
                this.readToken = token;
            }
            return null;
        }

        internal XPathExpr Parse()
        {
            XPathExpr expr = this.ParseExpression();
            if (expr == null)
            {
                this.ThrowError(QueryCompileError.InvalidExpression);
            }
            if (this.NextToken() != null)
            {
                this.ThrowError(QueryCompileError.UnexpectedToken);
            }
            return expr;
        }

        private XPathExprList ParseAbsolutePath()
        {
            XPathExprList path = null;
            XPathToken token = this.NextToken();
            if (token != null)
            {
                XPathTokenID tokenID = token.TokenID;
                if (tokenID == XPathTokenID.Slash)
                {
                    path = new XPathExprList();
                    path.Add(new XPathStepExpr(new NodeSelectCriteria(QueryAxisType.Child, NodeQName.Empty, QueryNodeType.Root)));
                }
                else if (tokenID == XPathTokenID.DblSlash)
                {
                    path = new XPathExprList();
                    path.Add(new XPathStepExpr(new NodeSelectCriteria(QueryAxisType.Child, NodeQName.Empty, QueryNodeType.Root)));
                    path.Add(new XPathStepExpr(new NodeSelectCriteria(QueryAxisType.DescendantOrSelf, NodeQName.Empty, QueryNodeType.All)));
                }
                else
                {
                    this.PushToken(token);
                }
            }
            if (path != null)
            {
                this.ParseRelativePath(path);
            }
            return path;
        }

        private XPathExpr ParseAdditiveExpression()
        {
            XPathExpr left = this.ParseMultiplicativeExpression();
            if (left != null)
            {
                MathOperator none;
                do
                {
                    none = MathOperator.None;
                    XPathToken token = this.NextToken();
                    if (token != null)
                    {
                        switch (token.TokenID)
                        {
                            case XPathTokenID.Plus:
                                none = MathOperator.Plus;
                                break;

                            case XPathTokenID.Minus:
                                none = MathOperator.Minus;
                                break;

                            default:
                                this.PushToken(token);
                                break;
                        }
                        if (none != MathOperator.None)
                        {
                            XPathExpr right = this.ParseMultiplicativeExpression();
                            if (right == null)
                            {
                                this.ThrowError(QueryCompileError.InvalidExpression);
                            }
                            left = new XPathMathExpr(none, left, right);
                        }
                    }
                }
                while (none != MathOperator.None);
            }
            return left;
        }

        private XPathExpr ParseAndExpression()
        {
            XPathExpr expr = this.ParseEqualityExpression();
            if ((expr == null) || (this.NextToken(XPathTokenID.And) == null))
            {
                return expr;
            }
            XPathExpr expr2 = new XPathExpr(XPathExprType.And, ValueDataType.Boolean);
            expr2.AddBooleanExpression(XPathExprType.And, expr);
            do
            {
                expr = this.ParseEqualityExpression();
                if (expr == null)
                {
                    this.ThrowError(QueryCompileError.InvalidExpression);
                }
                expr2.AddBooleanExpression(XPathExprType.And, expr);
            }
            while (this.NextToken(XPathTokenID.And) != null);
            return expr2;
        }

        private QueryAxisType ParseAxisSpecifier()
        {
            if (this.NextToken(XPathTokenID.AtSign) != null)
            {
                return QueryAxisType.Attribute;
            }
            QueryAxisType none = QueryAxisType.None;
            XPathToken token = this.NextTokenClass(XPathTokenID.Axis);
            if (token != null)
            {
                switch (token.TokenID)
                {
                    case XPathTokenID.Attribute:
                        none = QueryAxisType.Attribute;
                        break;

                    case XPathTokenID.Child:
                        none = QueryAxisType.Child;
                        break;

                    case XPathTokenID.Descendant:
                        none = QueryAxisType.Descendant;
                        break;

                    case XPathTokenID.DescendantOrSelf:
                        none = QueryAxisType.DescendantOrSelf;
                        break;

                    case XPathTokenID.Self:
                        none = QueryAxisType.Self;
                        break;

                    default:
                        this.ThrowError(QueryCompileError.UnsupportedAxis);
                        break;
                }
                this.NextToken(XPathTokenID.DblColon, QueryCompileError.InvalidAxisSpecifier);
            }
            return none;
        }

        private XPathExpr ParseEqualityExpression()
        {
            XPathExpr left = this.ParseRelationalExpression();
            if (left != null)
            {
                RelationOperator none;
                do
                {
                    none = RelationOperator.None;
                    XPathToken token = this.NextToken();
                    if (token != null)
                    {
                        switch (token.TokenID)
                        {
                            case XPathTokenID.Eq:
                                none = RelationOperator.Eq;
                                break;

                            case XPathTokenID.Neq:
                                none = RelationOperator.Ne;
                                break;

                            default:
                                this.PushToken(token);
                                break;
                        }
                        if (none != RelationOperator.None)
                        {
                            XPathExpr right = this.ParseRelationalExpression();
                            if (right == null)
                            {
                                this.ThrowError(QueryCompileError.InvalidExpression);
                            }
                            left = new XPathRelationExpr(none, left, right);
                        }
                    }
                }
                while (none != RelationOperator.None);
            }
            return left;
        }

        private XPathExpr ParseExpression()
        {
            return this.ParseOrExpression();
        }

        private XPathExpr ParseFilterExpression()
        {
            XPathExpr expr = this.ParsePrimaryExpression();
            if (expr == null)
            {
                return null;
            }
            XPathExpr expr2 = new XPathExpr(XPathExprType.Filter, expr.ReturnType);
            expr2.Add(expr);
            XPathExpr expr3 = this.ParsePredicateExpression();
            if (expr3 == null)
            {
                return expr;
            }
            this.EnsureReturnsNodeSet(expr);
            expr2.Add(expr3);
            while ((expr3 = this.ParsePredicateExpression()) != null)
            {
                expr2.Add(expr3);
            }
            return expr2;
        }

        private XPathExpr ParseFunctionExpression()
        {
            XPathExpr expr;
            XPathToken token = this.NextToken(XPathTokenID.Function);
            if (token == null)
            {
                return null;
            }
            NodeQName name = this.QualifyName(token.Prefix, token.Name);
            this.NextToken(XPathTokenID.LParen, QueryCompileError.InvalidFunction);
            XPathExprList args = new XPathExprList();
            while ((expr = this.ParseExpression()) != null)
            {
                args.Add(expr);
                if (this.NextToken(XPathTokenID.Comma) == null)
                {
                    break;
                }
            }
            XPathExpr expr2 = null;
            if (this.functionLibraries != null)
            {
                QueryFunction function = null;
                for (int i = 0; i < this.functionLibraries.Length; i++)
                {
                    function = this.functionLibraries[i].Bind(name.Name, name.Namespace, args);
                    if (function != null)
                    {
                        expr2 = new XPathFunctionExpr(function, args);
                        break;
                    }
                }
            }
            if ((expr2 == null) && (this.context != null))
            {
                XPathResultType[] argTypes = new XPathResultType[args.Count];
                for (int j = 0; j < args.Count; j++)
                {
                    argTypes[j] = XPathXsltFunctionExpr.ConvertTypeToXslt(args[j].ReturnType);
                }
                string prefix = this.context.LookupPrefix(name.Namespace);
                IXsltContextFunction function2 = this.context.ResolveFunction(prefix, name.Name, argTypes);
                if (function2 != null)
                {
                    expr2 = new XPathXsltFunctionExpr(this.context, function2, args);
                }
            }
            if (expr2 == null)
            {
                this.ThrowError(QueryCompileError.UnsupportedFunction);
            }
            this.NextToken(XPathTokenID.RParen, QueryCompileError.InvalidFunction);
            return expr2;
        }

        private XPathExpr ParseLiteralExpression()
        {
            XPathToken token = this.NextToken(XPathTokenID.Literal);
            if (token != null)
            {
                return new XPathStringExpr(token.Name);
            }
            return null;
        }

        internal XPathExpr ParseLocationPath()
        {
            XPathExprList subExpr = this.ParseAbsolutePath();
            if (subExpr == null)
            {
                subExpr = this.ParseRelativePath();
            }
            if (subExpr != null)
            {
                return new XPathExpr(XPathExprType.LocationPath, ValueDataType.Sequence, subExpr);
            }
            return null;
        }

        private XPathExpr ParseMultiplicativeExpression()
        {
            XPathExpr left = this.ParseUnaryExpression();
            if (left != null)
            {
                MathOperator none;
                do
                {
                    none = MathOperator.None;
                    XPathToken token = this.NextToken();
                    if (token != null)
                    {
                        XPathTokenID tokenID = token.TokenID;
                        if (tokenID == XPathTokenID.Multiply)
                        {
                            none = MathOperator.Multiply;
                        }
                        else if (tokenID == XPathTokenID.Mod)
                        {
                            none = MathOperator.Mod;
                        }
                        else if (tokenID == XPathTokenID.Div)
                        {
                            none = MathOperator.Div;
                        }
                        else
                        {
                            this.PushToken(token);
                        }
                        if (none != MathOperator.None)
                        {
                            XPathExpr right = this.ParseUnaryExpression();
                            if (right == null)
                            {
                                this.ThrowError(QueryCompileError.InvalidExpression);
                            }
                            left = new XPathMathExpr(none, left, right);
                        }
                    }
                }
                while (none != MathOperator.None);
            }
            return left;
        }

        private NodeSelectCriteria ParseNodeTest(QueryAxisType axisType)
        {
            QueryNodeType principalNodeType;
            QueryAxis axis = QueryDataModel.GetAxis(axisType);
            NodeQName empty = NodeQName.Empty;
            XPathToken token = this.NextTokenClass(XPathTokenID.NameTest);
            if (token != null)
            {
                switch (token.TokenID)
                {
                    case XPathTokenID.Wildcard:
                        empty = new NodeQName(QueryDataModel.Wildcard, QueryDataModel.Wildcard);
                        goto Label_0085;

                    case XPathTokenID.NameWildcard:
                        empty = this.QualifyName(token.Prefix, QueryDataModel.Wildcard);
                        goto Label_0085;

                    case XPathTokenID.NameTest:
                        empty = this.QualifyName(token.Prefix, token.Name);
                        goto Label_0085;
                }
                this.ThrowError(QueryCompileError.UnexpectedToken);
            }
        Label_0085:
            principalNodeType = QueryNodeType.Any;
            if (!empty.IsEmpty)
            {
                principalNodeType = axis.PrincipalNodeType;
            }
            else
            {
                token = this.NextTokenClass(XPathTokenID.NodeType);
                if (token == null)
                {
                    return null;
                }
                switch (token.TokenID)
                {
                    case XPathTokenID.Comment:
                        principalNodeType = QueryNodeType.Comment;
                        break;

                    case XPathTokenID.Text:
                        principalNodeType = QueryNodeType.Text;
                        break;

                    case XPathTokenID.Processing:
                        principalNodeType = QueryNodeType.Processing;
                        break;

                    case XPathTokenID.Node:
                        principalNodeType = QueryNodeType.All;
                        break;

                    default:
                        this.ThrowError(QueryCompileError.UnsupportedNodeTest);
                        break;
                }
                if (((byte) (axis.ValidNodeTypes & principalNodeType)) == 0)
                {
                    this.ThrowError(QueryCompileError.InvalidNodeType);
                }
                this.NextToken(XPathTokenID.LParen, QueryCompileError.InvalidNodeTest);
                this.NextToken(XPathTokenID.RParen, QueryCompileError.InvalidNodeTest);
            }
            return new NodeSelectCriteria(axisType, empty, principalNodeType);
        }

        private XPathExpr ParseNumberExpression()
        {
            XPathToken token = this.NextTokenClass(XPathTokenID.Number);
            if (token != null)
            {
                return new XPathNumberExpr(token.Number);
            }
            return null;
        }

        private XPathExpr ParseOrExpression()
        {
            XPathExpr expr = this.ParseAndExpression();
            if ((expr == null) || (this.NextToken(XPathTokenID.Or) == null))
            {
                return expr;
            }
            XPathExpr expr2 = new XPathExpr(XPathExprType.Or, ValueDataType.Boolean);
            expr2.AddBooleanExpression(XPathExprType.Or, expr);
            do
            {
                expr = this.ParseAndExpression();
                if (expr == null)
                {
                    this.ThrowError(QueryCompileError.InvalidExpression);
                }
                expr2.AddBooleanExpression(XPathExprType.Or, expr);
            }
            while (this.NextToken(XPathTokenID.Or) != null);
            return expr2;
        }

        private XPathExpr ParsePathExpression()
        {
            XPathExpr expr = this.ParseLocationPath();
            if (expr != null)
            {
                return expr;
            }
            XPathExpr expr2 = this.ParseFilterExpression();
            if (expr2 == null)
            {
                return expr;
            }
            if (this.NextToken(XPathTokenID.Slash) != null)
            {
                this.EnsureReturnsNodeSet(expr2);
                XPathExprList subExpr = this.ParseRelativePath();
                if (subExpr == null)
                {
                    this.ThrowError(QueryCompileError.InvalidLocationPath);
                }
                XPathExpr expr3 = new XPathExpr(XPathExprType.RelativePath, ValueDataType.Sequence, subExpr);
                expr = new XPathExpr(XPathExprType.Path, ValueDataType.Sequence);
                expr.Add(expr2);
                expr.Add(expr3);
                return expr;
            }
            if (this.NextToken(XPathTokenID.DblSlash) != null)
            {
                this.EnsureReturnsNodeSet(expr2);
                XPathExprList list2 = this.ParseRelativePath();
                if (list2 == null)
                {
                    this.ThrowError(QueryCompileError.InvalidLocationPath);
                }
                XPathExpr expr4 = new XPathExpr(XPathExprType.RelativePath, ValueDataType.Sequence, list2);
                expr = new XPathExpr(XPathExprType.Path, ValueDataType.Sequence);
                expr.Add(expr2);
                expr.Add(new XPathStepExpr(new NodeSelectCriteria(QueryAxisType.DescendantOrSelf, NodeQName.Empty, QueryNodeType.All)));
                expr.Add(expr4);
                return expr;
            }
            return expr2;
        }

        private XPathExpr ParsePredicateExpression()
        {
            XPathExpr expr = null;
            if (this.NextToken(XPathTokenID.LBracket) != null)
            {
                expr = this.ParseExpression();
                if (expr == null)
                {
                    this.ThrowError(QueryCompileError.InvalidPredicate);
                }
                this.NextToken(XPathTokenID.RBracket, QueryCompileError.InvalidPredicate);
            }
            return expr;
        }

        private XPathExprList ParsePredicates()
        {
            XPathExprList list = null;
            XPathExpr expr = this.ParsePredicateExpression();
            if (expr != null)
            {
                list = new XPathExprList();
                list.Add(expr);
                while ((expr = this.ParsePredicateExpression()) != null)
                {
                    list.Add(expr);
                }
            }
            return list;
        }

        private XPathExpr ParsePrimaryExpression()
        {
            XPathExpr expr = this.ParseVariableExpression();
            if ((expr == null) && (this.NextToken(XPathTokenID.LParen) != null))
            {
                expr = this.ParseExpression();
                if ((expr == null) || (this.NextToken(XPathTokenID.RParen) == null))
                {
                    this.ThrowError(QueryCompileError.InvalidExpression);
                }
            }
            if (expr == null)
            {
                expr = this.ParseLiteralExpression();
            }
            if (expr == null)
            {
                expr = this.ParseNumberExpression();
            }
            if (expr == null)
            {
                expr = this.ParseFunctionExpression();
            }
            return expr;
        }

        private XPathExpr ParseRelationalExpression()
        {
            XPathExpr left = this.ParseAdditiveExpression();
            if (left != null)
            {
                RelationOperator none;
                do
                {
                    none = RelationOperator.None;
                    XPathToken token = this.NextToken();
                    if (token != null)
                    {
                        switch (token.TokenID)
                        {
                            case XPathTokenID.Gt:
                                none = RelationOperator.Gt;
                                break;

                            case XPathTokenID.Gte:
                                none = RelationOperator.Ge;
                                break;

                            case XPathTokenID.Lt:
                                none = RelationOperator.Lt;
                                break;

                            case XPathTokenID.Lte:
                                none = RelationOperator.Le;
                                break;

                            default:
                                this.PushToken(token);
                                break;
                        }
                        if (none != RelationOperator.None)
                        {
                            XPathExpr right = this.ParseAdditiveExpression();
                            if (right == null)
                            {
                                this.ThrowError(QueryCompileError.InvalidExpression);
                            }
                            left = new XPathRelationExpr(none, left, right);
                        }
                    }
                }
                while (none != RelationOperator.None);
            }
            return left;
        }

        private XPathExprList ParseRelativePath()
        {
            XPathExprList path = new XPathExprList();
            if (this.ParseRelativePath(path))
            {
                return path;
            }
            return null;
        }

        private bool ParseRelativePath(XPathExprList path)
        {
            XPathStepExpr expr = this.ParseStep();
            if (expr == null)
            {
                return false;
            }
            path.Add(expr);
            while (true)
            {
                if (this.NextToken(XPathTokenID.Slash) != null)
                {
                    expr = this.ParseStep();
                }
                else
                {
                    if (this.NextToken(XPathTokenID.DblSlash) == null)
                    {
                        return true;
                    }
                    expr = new XPathStepExpr(new NodeSelectCriteria(QueryAxisType.DescendantOrSelf, NodeQName.Empty, QueryNodeType.All));
                    path.Add(expr);
                    expr = this.ParseStep();
                }
                if (expr == null)
                {
                    this.ThrowError(QueryCompileError.InvalidLocationPath);
                }
                path.Add(expr);
            }
        }

        private XPathStepExpr ParseStep()
        {
            QueryAxisType axisType = this.ParseAxisSpecifier();
            NodeSelectCriteria desc = null;
            bool flag = false;
            if (axisType != QueryAxisType.None)
            {
                desc = this.ParseNodeTest(axisType);
            }
            else if (this.NextToken(XPathTokenID.Period) != null)
            {
                desc = new NodeSelectCriteria(QueryAxisType.Self, NodeQName.Empty, QueryNodeType.All);
                flag = true;
            }
            else if (this.NextToken(XPathTokenID.DblPeriod) != null)
            {
                desc = new NodeSelectCriteria(QueryAxisType.Parent, NodeQName.Empty, QueryNodeType.Ancestor);
                flag = true;
            }
            else
            {
                desc = this.ParseNodeTest(QueryAxisType.Child);
                if (desc == null)
                {
                    return null;
                }
            }
            if (desc == null)
            {
                this.ThrowError(QueryCompileError.InvalidLocationStep);
            }
            XPathExprList predicates = null;
            if (!flag)
            {
                predicates = this.ParsePredicates();
            }
            return new XPathStepExpr(desc, predicates);
        }

        private XPathExpr ParseUnaryExpression()
        {
            bool flag = false;
            bool flag2 = false;
            while (this.NextToken(XPathTokenID.Minus) != null)
            {
                flag2 = true;
                flag = !flag;
            }
            XPathExpr expr = this.ParseUnionExpression();
            if (expr != null)
            {
                if (flag2 && (expr.ReturnType != ValueDataType.Double))
                {
                    expr.ReturnType = ValueDataType.Double;
                    expr.TypecastRequired = true;
                }
                expr.Negate = flag;
            }
            return expr;
        }

        internal XPathExpr ParseUnionExpression()
        {
            XPathExpr expr = this.ParsePathExpression();
            if ((expr == null) || (this.NextToken(XPathTokenID.Pipe) == null))
            {
                return expr;
            }
            this.EnsureReturnsNodeSet(expr);
            XPathExpr expr2 = this.ParseUnionExpression();
            if (expr2 == null)
            {
                this.ThrowError(QueryCompileError.CouldNotParseExpression);
            }
            this.EnsureReturnsNodeSet(expr2);
            return new XPathConjunctExpr(XPathExprType.Union, ValueDataType.Sequence, expr, expr2);
        }

        internal XPathExpr ParseVariableExpression()
        {
            XPathExpr expr = null;
            if (this.context != null)
            {
                XPathToken token = this.NextToken(XPathTokenID.Variable);
                if (token != null)
                {
                    NodeQName name = this.QualifyName(token.Prefix, token.Name);
                    string prefix = this.context.LookupPrefix(name.Namespace);
                    IXsltContextVariable variable = this.context.ResolveVariable(prefix, name.Name);
                    if (variable != null)
                    {
                        expr = new XPathXsltVariableExpr(this.context, variable);
                    }
                }
            }
            return expr;
        }

        private void PushToken(XPathToken token)
        {
            this.readToken = token;
        }

        private NodeQName QualifyName(string prefix, string name)
        {
            if (((this.namespaces == null) || (prefix == null)) || (prefix.Length <= 0))
            {
                return new NodeQName(name);
            }
            prefix = this.namespaces.NameTable.Add(prefix);
            string ns = this.namespaces.LookupNamespace(prefix);
            if (ns == null)
            {
                this.ThrowError(QueryCompileError.NoNamespaceForPrefix);
            }
            return new NodeQName(name, ns);
        }

        internal void ThrowError(QueryCompileError error)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryCompileException(error, this.lexer.ConsumedSubstring()));
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct QName
        {
            private string prefix;
            private string name;
            internal QName(string prefix, string name)
            {
                this.prefix = prefix;
                this.name = name;
            }

            internal string Prefix
            {
                get
                {
                    return this.prefix;
                }
            }
            internal string Name
            {
                get
                {
                    return this.name;
                }
            }
        }
    }
}

