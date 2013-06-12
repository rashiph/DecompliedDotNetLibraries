namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Collections;
    using System.Xml.XPath;

    internal class XPathParser
    {
        private static Hashtable AxesTable;
        private static Hashtable functionTable;
        private XPathScanner scanner;
        private static readonly XPathResultType[] temparray1 = new XPathResultType[0];
        private static readonly XPathResultType[] temparray2 = new XPathResultType[] { XPathResultType.NodeSet };
        private static readonly XPathResultType[] temparray3 = new XPathResultType[] { XPathResultType.Any };
        private static readonly XPathResultType[] temparray4 = new XPathResultType[] { XPathResultType.String };
        private static readonly XPathResultType[] temparray5 = new XPathResultType[] { XPathResultType.String, XPathResultType.String };
        private static readonly XPathResultType[] temparray6;
        private static readonly XPathResultType[] temparray7;
        private static readonly XPathResultType[] temparray8;
        private static readonly XPathResultType[] temparray9;

        static XPathParser()
        {
            XPathResultType[] typeArray5 = new XPathResultType[3];
            typeArray5[0] = XPathResultType.String;
            temparray6 = typeArray5;
            temparray7 = new XPathResultType[] { XPathResultType.String, XPathResultType.String, XPathResultType.String };
            temparray8 = new XPathResultType[] { XPathResultType.Boolean };
            temparray9 = new XPathResultType[1];
            functionTable = CreateFunctionTable();
            AxesTable = CreateAxesTable();
        }

        private XPathParser(XPathScanner scanner)
        {
            this.scanner = scanner;
        }

        private void CheckNodeSet(XPathResultType t)
        {
            if ((t != XPathResultType.NodeSet) && (t != XPathResultType.Any))
            {
                throw XPathException.Create("Xp_NodeSetExpected", this.scanner.SourceText);
            }
        }

        private void CheckToken(XPathScanner.LexKind t)
        {
            if (this.scanner.Kind != t)
            {
                throw XPathException.Create("Xp_InvalidToken", this.scanner.SourceText);
            }
        }

        private static Hashtable CreateAxesTable()
        {
            Hashtable hashtable = new Hashtable(13);
            hashtable.Add("ancestor", Axis.AxisType.Ancestor);
            hashtable.Add("ancestor-or-self", Axis.AxisType.AncestorOrSelf);
            hashtable.Add("attribute", Axis.AxisType.Attribute);
            hashtable.Add("child", Axis.AxisType.Child);
            hashtable.Add("descendant", Axis.AxisType.Descendant);
            hashtable.Add("descendant-or-self", Axis.AxisType.DescendantOrSelf);
            hashtable.Add("following", Axis.AxisType.Following);
            hashtable.Add("following-sibling", Axis.AxisType.FollowingSibling);
            hashtable.Add("namespace", Axis.AxisType.Namespace);
            hashtable.Add("parent", Axis.AxisType.Parent);
            hashtable.Add("preceding", Axis.AxisType.Preceding);
            hashtable.Add("preceding-sibling", Axis.AxisType.PrecedingSibling);
            hashtable.Add("self", Axis.AxisType.Self);
            return hashtable;
        }

        private static Hashtable CreateFunctionTable()
        {
            Hashtable hashtable = new Hashtable(0x24);
            hashtable.Add("last", new ParamInfo(Function.FunctionType.FuncLast, 0, 0, temparray1));
            hashtable.Add("position", new ParamInfo(Function.FunctionType.FuncPosition, 0, 0, temparray1));
            hashtable.Add("name", new ParamInfo(Function.FunctionType.FuncName, 0, 1, temparray2));
            hashtable.Add("namespace-uri", new ParamInfo(Function.FunctionType.FuncNameSpaceUri, 0, 1, temparray2));
            hashtable.Add("local-name", new ParamInfo(Function.FunctionType.FuncLocalName, 0, 1, temparray2));
            hashtable.Add("count", new ParamInfo(Function.FunctionType.FuncCount, 1, 1, temparray2));
            hashtable.Add("id", new ParamInfo(Function.FunctionType.FuncID, 1, 1, temparray3));
            hashtable.Add("string", new ParamInfo(Function.FunctionType.FuncString, 0, 1, temparray3));
            hashtable.Add("concat", new ParamInfo(Function.FunctionType.FuncConcat, 2, 100, temparray4));
            hashtable.Add("starts-with", new ParamInfo(Function.FunctionType.FuncStartsWith, 2, 2, temparray5));
            hashtable.Add("contains", new ParamInfo(Function.FunctionType.FuncContains, 2, 2, temparray5));
            hashtable.Add("substring-before", new ParamInfo(Function.FunctionType.FuncSubstringBefore, 2, 2, temparray5));
            hashtable.Add("substring-after", new ParamInfo(Function.FunctionType.FuncSubstringAfter, 2, 2, temparray5));
            hashtable.Add("substring", new ParamInfo(Function.FunctionType.FuncSubstring, 2, 3, temparray6));
            hashtable.Add("string-length", new ParamInfo(Function.FunctionType.FuncStringLength, 0, 1, temparray4));
            hashtable.Add("normalize-space", new ParamInfo(Function.FunctionType.FuncNormalize, 0, 1, temparray4));
            hashtable.Add("translate", new ParamInfo(Function.FunctionType.FuncTranslate, 3, 3, temparray7));
            hashtable.Add("boolean", new ParamInfo(Function.FunctionType.FuncBoolean, 1, 1, temparray3));
            hashtable.Add("not", new ParamInfo(Function.FunctionType.FuncNot, 1, 1, temparray8));
            hashtable.Add("true", new ParamInfo(Function.FunctionType.FuncTrue, 0, 0, temparray8));
            hashtable.Add("false", new ParamInfo(Function.FunctionType.FuncFalse, 0, 0, temparray8));
            hashtable.Add("lang", new ParamInfo(Function.FunctionType.FuncLang, 1, 1, temparray4));
            hashtable.Add("number", new ParamInfo(Function.FunctionType.FuncNumber, 0, 1, temparray3));
            hashtable.Add("sum", new ParamInfo(Function.FunctionType.FuncSum, 1, 1, temparray2));
            hashtable.Add("floor", new ParamInfo(Function.FunctionType.FuncFloor, 1, 1, temparray9));
            hashtable.Add("ceiling", new ParamInfo(Function.FunctionType.FuncCeiling, 1, 1, temparray9));
            hashtable.Add("round", new ParamInfo(Function.FunctionType.FuncRound, 1, 1, temparray9));
            return hashtable;
        }

        private Axis.AxisType GetAxis(XPathScanner scaner)
        {
            object obj2 = AxesTable[scaner.Name];
            if (obj2 == null)
            {
                throw XPathException.Create("Xp_InvalidToken", this.scanner.SourceText);
            }
            return (Axis.AxisType) obj2;
        }

        private static bool IsNodeType(XPathScanner scaner)
        {
            if (scaner.Prefix.Length != 0)
            {
                return false;
            }
            if ((!(scaner.Name == "node") && !(scaner.Name == "text")) && !(scaner.Name == "processing-instruction"))
            {
                return (scaner.Name == "comment");
            }
            return true;
        }

        private static bool IsPrimaryExpr(XPathScanner scanner)
        {
            return ((((scanner.Kind == XPathScanner.LexKind.String) || (scanner.Kind == XPathScanner.LexKind.Number)) || ((scanner.Kind == XPathScanner.LexKind.Dollar) || (scanner.Kind == XPathScanner.LexKind.LParens))) || (((scanner.Kind == XPathScanner.LexKind.Name) && scanner.CanBeFunction) && !IsNodeType(scanner)));
        }

        private static bool IsStep(XPathScanner.LexKind lexKind)
        {
            if ((((lexKind != XPathScanner.LexKind.Dot) && (lexKind != XPathScanner.LexKind.DotDot)) && ((lexKind != XPathScanner.LexKind.At) && (lexKind != XPathScanner.LexKind.Axe))) && (lexKind != XPathScanner.LexKind.Star))
            {
                return (lexKind == XPathScanner.LexKind.Name);
            }
            return true;
        }

        private void NextLex()
        {
            this.scanner.NextLex();
        }

        private AstNode ParseAdditiveExpr(AstNode qyInput)
        {
            AstNode node = this.ParseMultiplicativeExpr(qyInput);
            while (true)
            {
                Operator.Op op = (this.scanner.Kind == XPathScanner.LexKind.Plus) ? Operator.Op.PLUS : ((this.scanner.Kind == XPathScanner.LexKind.Minus) ? Operator.Op.MINUS : Operator.Op.INVALID);
                if (op == Operator.Op.INVALID)
                {
                    return node;
                }
                this.NextLex();
                node = new Operator(op, node, this.ParseMultiplicativeExpr(qyInput));
            }
        }

        private AstNode ParseAndExpr(AstNode qyInput)
        {
            AstNode node = this.ParseEqualityExpr(qyInput);
            while (this.TestOp("and"))
            {
                this.NextLex();
                node = new Operator(Operator.Op.AND, node, this.ParseEqualityExpr(qyInput));
            }
            return node;
        }

        private AstNode ParseEqualityExpr(AstNode qyInput)
        {
            AstNode node = this.ParseRelationalExpr(qyInput);
            while (true)
            {
                Operator.Op op = (this.scanner.Kind == XPathScanner.LexKind.Eq) ? Operator.Op.EQ : ((this.scanner.Kind == XPathScanner.LexKind.Ne) ? Operator.Op.NE : Operator.Op.INVALID);
                if (op == Operator.Op.INVALID)
                {
                    return node;
                }
                this.NextLex();
                node = new Operator(op, node, this.ParseRelationalExpr(qyInput));
            }
        }

        private AstNode ParseExpresion(AstNode qyInput)
        {
            return this.ParseOrExpr(qyInput);
        }

        private AstNode ParseFilterExpr(AstNode qyInput)
        {
            AstNode input = this.ParsePrimaryExpr(qyInput);
            while (this.scanner.Kind == XPathScanner.LexKind.LBracket)
            {
                input = new MS.Internal.Xml.XPath.Filter(input, this.ParsePredicate(input));
            }
            return input;
        }

        private AstNode ParseIdKeyPattern(AstNode qyInput)
        {
            ArrayList argumentList = new ArrayList();
            if (this.scanner.Prefix.Length == 0)
            {
                if (this.scanner.Name == "id")
                {
                    ParamInfo info = (ParamInfo) functionTable["id"];
                    this.NextLex();
                    this.PassToken(XPathScanner.LexKind.LParens);
                    this.CheckToken(XPathScanner.LexKind.String);
                    argumentList.Add(new Operand(this.scanner.StringValue));
                    this.NextLex();
                    this.PassToken(XPathScanner.LexKind.RParens);
                    return new Function(info.FType, argumentList);
                }
                if (this.scanner.Name == "key")
                {
                    this.NextLex();
                    this.PassToken(XPathScanner.LexKind.LParens);
                    this.CheckToken(XPathScanner.LexKind.String);
                    argumentList.Add(new Operand(this.scanner.StringValue));
                    this.NextLex();
                    this.PassToken(XPathScanner.LexKind.Comma);
                    this.CheckToken(XPathScanner.LexKind.String);
                    argumentList.Add(new Operand(this.scanner.StringValue));
                    this.NextLex();
                    this.PassToken(XPathScanner.LexKind.RParens);
                    return new Function("", "key", argumentList);
                }
            }
            return null;
        }

        private AstNode ParseLocationPath(AstNode qyInput)
        {
            if (this.scanner.Kind == XPathScanner.LexKind.Slash)
            {
                this.NextLex();
                AstNode node = new Root();
                if (IsStep(this.scanner.Kind))
                {
                    node = this.ParseRelativeLocationPath(node);
                }
                return node;
            }
            if (this.scanner.Kind == XPathScanner.LexKind.SlashSlash)
            {
                this.NextLex();
                return this.ParseRelativeLocationPath(new Axis(Axis.AxisType.DescendantOrSelf, new Root()));
            }
            return this.ParseRelativeLocationPath(qyInput);
        }

        private AstNode ParseLocationPathPattern(AstNode qyInput)
        {
            AstNode input = null;
            switch (this.scanner.Kind)
            {
                case XPathScanner.LexKind.Slash:
                    this.NextLex();
                    input = new Root();
                    if ((this.scanner.Kind == XPathScanner.LexKind.Eof) || (this.scanner.Kind == XPathScanner.LexKind.Union))
                    {
                        return input;
                    }
                    break;

                case XPathScanner.LexKind.SlashSlash:
                    this.NextLex();
                    input = new Axis(Axis.AxisType.DescendantOrSelf, new Root());
                    break;

                case XPathScanner.LexKind.Name:
                    if (this.scanner.CanBeFunction)
                    {
                        input = this.ParseIdKeyPattern(qyInput);
                        if (input != null)
                        {
                            XPathScanner.LexKind kind = this.scanner.Kind;
                            if (kind != XPathScanner.LexKind.Slash)
                            {
                                if (kind != XPathScanner.LexKind.SlashSlash)
                                {
                                    return input;
                                }
                                this.NextLex();
                                input = new Axis(Axis.AxisType.DescendantOrSelf, input);
                                break;
                            }
                            this.NextLex();
                        }
                    }
                    break;
            }
            return this.ParseRelativePathPattern(input);
        }

        private AstNode ParseMethod(AstNode qyInput)
        {
            ArrayList argumentList = new ArrayList();
            string name = this.scanner.Name;
            string prefix = this.scanner.Prefix;
            this.PassToken(XPathScanner.LexKind.Name);
            this.PassToken(XPathScanner.LexKind.LParens);
            if (this.scanner.Kind != XPathScanner.LexKind.RParens)
            {
                while (true)
                {
                    argumentList.Add(this.ParseExpresion(qyInput));
                    if (this.scanner.Kind == XPathScanner.LexKind.RParens)
                    {
                        break;
                    }
                    this.PassToken(XPathScanner.LexKind.Comma);
                }
            }
            this.PassToken(XPathScanner.LexKind.RParens);
            if (prefix.Length == 0)
            {
                ParamInfo info = (ParamInfo) functionTable[name];
                if (info != null)
                {
                    int count = argumentList.Count;
                    if (count < info.Minargs)
                    {
                        throw XPathException.Create("Xp_InvalidNumArgs", name, this.scanner.SourceText);
                    }
                    if (info.FType == Function.FunctionType.FuncConcat)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            AstNode arg = (AstNode) argumentList[i];
                            if (arg.ReturnType != XPathResultType.String)
                            {
                                arg = new Function(Function.FunctionType.FuncString, arg);
                            }
                            argumentList[i] = arg;
                        }
                    }
                    else
                    {
                        if (info.Maxargs < count)
                        {
                            throw XPathException.Create("Xp_InvalidNumArgs", name, this.scanner.SourceText);
                        }
                        if (info.ArgTypes.Length < count)
                        {
                            count = info.ArgTypes.Length;
                        }
                        for (int j = 0; j < count; j++)
                        {
                            AstNode node2 = (AstNode) argumentList[j];
                            if ((info.ArgTypes[j] != XPathResultType.Any) && (info.ArgTypes[j] != node2.ReturnType))
                            {
                                switch (info.ArgTypes[j])
                                {
                                    case XPathResultType.Number:
                                        node2 = new Function(Function.FunctionType.FuncNumber, node2);
                                        break;

                                    case XPathResultType.String:
                                        node2 = new Function(Function.FunctionType.FuncString, node2);
                                        break;

                                    case XPathResultType.Boolean:
                                        node2 = new Function(Function.FunctionType.FuncBoolean, node2);
                                        break;

                                    case XPathResultType.NodeSet:
                                        if (!(node2 is Variable) && (!(node2 is Function) || (node2.ReturnType != XPathResultType.Any)))
                                        {
                                            throw XPathException.Create("Xp_InvalidArgumentType", name, this.scanner.SourceText);
                                        }
                                        break;
                                }
                                argumentList[j] = node2;
                            }
                        }
                    }
                    return new Function(info.FType, argumentList);
                }
            }
            return new Function(prefix, name, argumentList);
        }

        private AstNode ParseMultiplicativeExpr(AstNode qyInput)
        {
            AstNode node = this.ParseUnaryExpr(qyInput);
            while (true)
            {
                Operator.Op op = (this.scanner.Kind == XPathScanner.LexKind.Star) ? Operator.Op.MUL : (this.TestOp("div") ? Operator.Op.DIV : (this.TestOp("mod") ? Operator.Op.MOD : Operator.Op.INVALID));
                if (op == Operator.Op.INVALID)
                {
                    return node;
                }
                this.NextLex();
                node = new Operator(op, node, this.ParseUnaryExpr(qyInput));
            }
        }

        private AstNode ParseNodeTest(AstNode qyInput, Axis.AxisType axisType, XPathNodeType nodeType)
        {
            string stringValue;
            string prefix;
            XPathScanner.LexKind kind = this.scanner.Kind;
            if (kind != XPathScanner.LexKind.Star)
            {
                if (kind != XPathScanner.LexKind.Name)
                {
                    throw XPathException.Create("Xp_NodeSetExpected", this.scanner.SourceText);
                }
                if (this.scanner.CanBeFunction && IsNodeType(this.scanner))
                {
                    prefix = string.Empty;
                    stringValue = string.Empty;
                    nodeType = (this.scanner.Name == "comment") ? XPathNodeType.Comment : ((this.scanner.Name == "text") ? XPathNodeType.Text : ((this.scanner.Name == "node") ? XPathNodeType.All : ((this.scanner.Name == "processing-instruction") ? XPathNodeType.ProcessingInstruction : XPathNodeType.Root)));
                    this.NextLex();
                    this.PassToken(XPathScanner.LexKind.LParens);
                    if ((nodeType == XPathNodeType.ProcessingInstruction) && (this.scanner.Kind != XPathScanner.LexKind.RParens))
                    {
                        this.CheckToken(XPathScanner.LexKind.String);
                        stringValue = this.scanner.StringValue;
                        this.NextLex();
                    }
                    this.PassToken(XPathScanner.LexKind.RParens);
                }
                else
                {
                    prefix = this.scanner.Prefix;
                    stringValue = this.scanner.Name;
                    this.NextLex();
                    if (stringValue == "*")
                    {
                        stringValue = string.Empty;
                    }
                }
            }
            else
            {
                prefix = string.Empty;
                stringValue = string.Empty;
                this.NextLex();
            }
            return new Axis(axisType, qyInput, prefix, stringValue, nodeType);
        }

        private AstNode ParseOrExpr(AstNode qyInput)
        {
            AstNode node = this.ParseAndExpr(qyInput);
            while (this.TestOp("or"))
            {
                this.NextLex();
                node = new Operator(Operator.Op.OR, node, this.ParseAndExpr(qyInput));
            }
            return node;
        }

        private AstNode ParsePathExpr(AstNode qyInput)
        {
            if (IsPrimaryExpr(this.scanner))
            {
                AstNode node = this.ParseFilterExpr(qyInput);
                if (this.scanner.Kind == XPathScanner.LexKind.Slash)
                {
                    this.NextLex();
                    return this.ParseRelativeLocationPath(node);
                }
                if (this.scanner.Kind == XPathScanner.LexKind.SlashSlash)
                {
                    this.NextLex();
                    node = this.ParseRelativeLocationPath(new Axis(Axis.AxisType.DescendantOrSelf, node));
                }
                return node;
            }
            return this.ParseLocationPath(null);
        }

        private AstNode ParsePattern(AstNode qyInput)
        {
            AstNode node = this.ParseLocationPathPattern(qyInput);
            while (this.scanner.Kind == XPathScanner.LexKind.Union)
            {
                this.NextLex();
                node = new Operator(Operator.Op.UNION, node, this.ParseLocationPathPattern(qyInput));
            }
            return node;
        }

        private AstNode ParsePredicate(AstNode qyInput)
        {
            this.CheckNodeSet(qyInput.ReturnType);
            this.PassToken(XPathScanner.LexKind.LBracket);
            AstNode node = this.ParseExpresion(qyInput);
            this.PassToken(XPathScanner.LexKind.RBracket);
            return node;
        }

        private AstNode ParsePrimaryExpr(AstNode qyInput)
        {
            AstNode groupNode = null;
            XPathScanner.LexKind kind = this.scanner.Kind;
            if (kind <= XPathScanner.LexKind.LParens)
            {
                switch (kind)
                {
                    case XPathScanner.LexKind.Dollar:
                        this.NextLex();
                        this.CheckToken(XPathScanner.LexKind.Name);
                        groupNode = new Variable(this.scanner.Name, this.scanner.Prefix);
                        this.NextLex();
                        return groupNode;

                    case XPathScanner.LexKind.LParens:
                        this.NextLex();
                        groupNode = this.ParseExpresion(qyInput);
                        if (groupNode.Type != AstNode.AstType.ConstantOperand)
                        {
                            groupNode = new Group(groupNode);
                        }
                        this.PassToken(XPathScanner.LexKind.RParens);
                        return groupNode;
                }
                return groupNode;
            }
            if (kind != XPathScanner.LexKind.Number)
            {
                if (kind != XPathScanner.LexKind.Name)
                {
                    if (kind == XPathScanner.LexKind.String)
                    {
                        groupNode = new Operand(this.scanner.StringValue);
                        this.NextLex();
                    }
                    return groupNode;
                }
                if (this.scanner.CanBeFunction && !IsNodeType(this.scanner))
                {
                    groupNode = this.ParseMethod(null);
                }
                return groupNode;
            }
            groupNode = new Operand(this.scanner.NumberValue);
            this.NextLex();
            return groupNode;
        }

        private AstNode ParseRelationalExpr(AstNode qyInput)
        {
            AstNode node = this.ParseAdditiveExpr(qyInput);
            while (true)
            {
                Operator.Op op = (this.scanner.Kind == XPathScanner.LexKind.Lt) ? Operator.Op.LT : ((this.scanner.Kind == XPathScanner.LexKind.Le) ? Operator.Op.LE : ((this.scanner.Kind == XPathScanner.LexKind.Gt) ? Operator.Op.GT : ((this.scanner.Kind == XPathScanner.LexKind.Ge) ? Operator.Op.GE : Operator.Op.INVALID)));
                if (op == Operator.Op.INVALID)
                {
                    return node;
                }
                this.NextLex();
                node = new Operator(op, node, this.ParseAdditiveExpr(qyInput));
            }
        }

        private AstNode ParseRelativeLocationPath(AstNode qyInput)
        {
            AstNode input = this.ParseStep(qyInput);
            if (XPathScanner.LexKind.SlashSlash == this.scanner.Kind)
            {
                this.NextLex();
                return this.ParseRelativeLocationPath(new Axis(Axis.AxisType.DescendantOrSelf, input));
            }
            if (XPathScanner.LexKind.Slash == this.scanner.Kind)
            {
                this.NextLex();
                input = this.ParseRelativeLocationPath(input);
            }
            return input;
        }

        private AstNode ParseRelativePathPattern(AstNode qyInput)
        {
            AstNode input = this.ParseStepPattern(qyInput);
            if (XPathScanner.LexKind.SlashSlash == this.scanner.Kind)
            {
                this.NextLex();
                return this.ParseRelativePathPattern(new Axis(Axis.AxisType.DescendantOrSelf, input));
            }
            if (XPathScanner.LexKind.Slash == this.scanner.Kind)
            {
                this.NextLex();
                input = this.ParseRelativePathPattern(input);
            }
            return input;
        }

        private AstNode ParseStep(AstNode qyInput)
        {
            if (XPathScanner.LexKind.Dot == this.scanner.Kind)
            {
                this.NextLex();
                return new Axis(Axis.AxisType.Self, qyInput);
            }
            if (XPathScanner.LexKind.DotDot == this.scanner.Kind)
            {
                this.NextLex();
                return new Axis(Axis.AxisType.Parent, qyInput);
            }
            Axis.AxisType child = Axis.AxisType.Child;
            switch (this.scanner.Kind)
            {
                case XPathScanner.LexKind.At:
                    child = Axis.AxisType.Attribute;
                    this.NextLex();
                    break;

                case XPathScanner.LexKind.Axe:
                    child = this.GetAxis(this.scanner);
                    this.NextLex();
                    break;
            }
            XPathNodeType nodeType = (child == Axis.AxisType.Attribute) ? XPathNodeType.Attribute : XPathNodeType.Element;
            AstNode input = this.ParseNodeTest(qyInput, child, nodeType);
            while (XPathScanner.LexKind.LBracket == this.scanner.Kind)
            {
                input = new MS.Internal.Xml.XPath.Filter(input, this.ParsePredicate(input));
            }
            return input;
        }

        private AstNode ParseStepPattern(AstNode qyInput)
        {
            Axis.AxisType child = Axis.AxisType.Child;
            switch (this.scanner.Kind)
            {
                case XPathScanner.LexKind.At:
                    child = Axis.AxisType.Attribute;
                    this.NextLex();
                    break;

                case XPathScanner.LexKind.Axe:
                    child = this.GetAxis(this.scanner);
                    if ((child != Axis.AxisType.Child) && (child != Axis.AxisType.Attribute))
                    {
                        throw XPathException.Create("Xp_InvalidToken", this.scanner.SourceText);
                    }
                    this.NextLex();
                    break;
            }
            XPathNodeType nodeType = (child == Axis.AxisType.Attribute) ? XPathNodeType.Attribute : XPathNodeType.Element;
            AstNode input = this.ParseNodeTest(qyInput, child, nodeType);
            while (XPathScanner.LexKind.LBracket == this.scanner.Kind)
            {
                input = new MS.Internal.Xml.XPath.Filter(input, this.ParsePredicate(input));
            }
            return input;
        }

        private AstNode ParseUnaryExpr(AstNode qyInput)
        {
            if (this.scanner.Kind == XPathScanner.LexKind.Minus)
            {
                this.NextLex();
                return new Operator(Operator.Op.MUL, this.ParseUnaryExpr(qyInput), new Operand(-1.0));
            }
            return this.ParseUnionExpr(qyInput);
        }

        private AstNode ParseUnionExpr(AstNode qyInput)
        {
            AstNode node = this.ParsePathExpr(qyInput);
            while (this.scanner.Kind == XPathScanner.LexKind.Union)
            {
                this.NextLex();
                AstNode node2 = this.ParsePathExpr(qyInput);
                this.CheckNodeSet(node.ReturnType);
                this.CheckNodeSet(node2.ReturnType);
                node = new Operator(Operator.Op.UNION, node, node2);
            }
            return node;
        }

        public static AstNode ParseXPathExpresion(string xpathExpresion)
        {
            XPathScanner scanner = new XPathScanner(xpathExpresion);
            AstNode node = new XPathParser(scanner).ParseExpresion(null);
            if (scanner.Kind != XPathScanner.LexKind.Eof)
            {
                throw XPathException.Create("Xp_InvalidToken", scanner.SourceText);
            }
            return node;
        }

        public static AstNode ParseXPathPattern(string xpathPattern)
        {
            XPathScanner scanner = new XPathScanner(xpathPattern);
            AstNode node = new XPathParser(scanner).ParsePattern(null);
            if (scanner.Kind != XPathScanner.LexKind.Eof)
            {
                throw XPathException.Create("Xp_InvalidToken", scanner.SourceText);
            }
            return node;
        }

        private void PassToken(XPathScanner.LexKind t)
        {
            this.CheckToken(t);
            this.NextLex();
        }

        private bool TestOp(string op)
        {
            return (((this.scanner.Kind == XPathScanner.LexKind.Name) && (this.scanner.Prefix.Length == 0)) && this.scanner.Name.Equals(op));
        }

        private class ParamInfo
        {
            private XPathResultType[] argTypes;
            private Function.FunctionType ftype;
            private int maxargs;
            private int minargs;

            internal ParamInfo(Function.FunctionType ftype, int minargs, int maxargs, XPathResultType[] argTypes)
            {
                this.ftype = ftype;
                this.minargs = minargs;
                this.maxargs = maxargs;
                this.argTypes = argTypes;
            }

            public XPathResultType[] ArgTypes
            {
                get
                {
                    return this.argTypes;
                }
            }

            public Function.FunctionType FType
            {
                get
                {
                    return this.ftype;
                }
            }

            public int Maxargs
            {
                get
                {
                    return this.maxargs;
                }
            }

            public int Minargs
            {
                get
                {
                    return this.minargs;
                }
            }
        }
    }
}

