namespace System.Data
{
    using System;
    using System.Data.Common;
    using System.Globalization;
    using System.Runtime.InteropServices;

    internal sealed class ExpressionParser
    {
        private readonly DataTable _table;
        private char DecimalSeparator = '.';
        private const int Empty = 0;
        private char Escape = '\\';
        private char ExponentL = 'e';
        private char ExponentU = 'E';
        private const int Expr = 2;
        internal ExpressionNode expression;
        private char ListSeparator = ',';
        private const int MaxPredicates = 100;
        internal ExpressionNode[] NodeStack = new ExpressionNode[100];
        internal int op;
        internal OperatorInfo[] ops = new OperatorInfo[100];
        internal int pos;
        internal int prevOperand;
        private static readonly ReservedWords[] reservedwords = new ReservedWords[] { new ReservedWords("And", Tokens.BinaryOp, 0x1a), new ReservedWords("Between", Tokens.BinaryOp, 6), new ReservedWords("Child", Tokens.Child, 0), new ReservedWords("False", Tokens.ZeroOp, 0x22), new ReservedWords("In", Tokens.BinaryOp, 5), new ReservedWords("Is", Tokens.BinaryOp, 13), new ReservedWords("Like", Tokens.BinaryOp, 14), new ReservedWords("Not", Tokens.UnaryOp, 3), new ReservedWords("Null", Tokens.ZeroOp, 0x20), new ReservedWords("Or", Tokens.BinaryOp, 0x1b), new ReservedWords("Parent", Tokens.Parent, 0), new ReservedWords("True", Tokens.ZeroOp, 0x21) };
        private const int Scalar = 1;
        internal int start;
        internal char[] text;
        internal Tokens token;
        internal int topNode;
        internal int topOperator;

        internal ExpressionParser(DataTable table)
        {
            this._table = table;
        }

        private void BuildExpression(int pri)
        {
            OperatorInfo info;
            ExpressionNode node = null;
        Label_0002:
            info = this.ops[this.topOperator - 1];
            if (info.priority >= pri)
            {
                ExpressionNode node2;
                ExpressionNode node3;
                this.topOperator--;
                switch (info.type)
                {
                    case Nodes.Unop:
                        node3 = null;
                        node2 = this.NodePop();
                        switch (info.op)
                        {
                            case 0x19:
                                throw ExprException.UnsupportedOperator(info.op);
                        }
                        node = new UnaryNode(this._table, info.op, node2);
                        goto Label_016C;

                    case Nodes.UnopSpec:
                    case Nodes.BinopSpec:
                        return;

                    case Nodes.Binop:
                        node2 = this.NodePop();
                        node3 = this.NodePop();
                        switch (info.op)
                        {
                            case 4:
                            case 6:
                            case 0x16:
                            case 0x17:
                            case 0x18:
                            case 0x19:
                                throw ExprException.UnsupportedOperator(info.op);
                        }
                        if (info.op == 14)
                        {
                            node = new LikeNode(this._table, info.op, node3, node2);
                        }
                        else
                        {
                            node = new BinaryNode(this._table, info.op, node3, node2);
                        }
                        goto Label_016C;

                    case Nodes.Zop:
                        node = new ZeroOpNode(info.op);
                        goto Label_016C;
                }
            }
            return;
        Label_016C:
            this.NodePush(node);
            goto Label_0002;
        }

        internal void CheckToken(Tokens token)
        {
            if (this.token != token)
            {
                throw ExprException.UnknownToken(token, this.token, this.pos);
            }
        }

        private bool IsAlpha(char ch)
        {
            switch (ch)
            {
                case 'A':
                case 'B':
                case 'C':
                case 'D':
                case 'E':
                case 'F':
                case 'G':
                case 'H':
                case 'I':
                case 'J':
                case 'K':
                case 'L':
                case 'M':
                case 'N':
                case 'O':
                case 'P':
                case 'Q':
                case 'R':
                case 'S':
                case 'T':
                case 'U':
                case 'V':
                case 'W':
                case 'X':
                case 'Y':
                case 'Z':
                case '_':
                case 'a':
                case 'b':
                case 'c':
                case 'd':
                case 'e':
                case 'f':
                case 'g':
                case 'h':
                case 'i':
                case 'j':
                case 'k':
                case 'l':
                case 'm':
                case 'n':
                case 'o':
                case 'p':
                case 'q':
                case 'r':
                case 's':
                case 't':
                case 'u':
                case 'v':
                case 'w':
                case 'x':
                case 'y':
                case 'z':
                    return true;
            }
            return false;
        }

        private bool IsAlphaNumeric(char ch)
        {
            switch (ch)
            {
                case '$':
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case 'A':
                case 'B':
                case 'C':
                case 'D':
                case 'E':
                case 'F':
                case 'G':
                case 'H':
                case 'I':
                case 'J':
                case 'K':
                case 'L':
                case 'M':
                case 'N':
                case 'O':
                case 'P':
                case 'Q':
                case 'R':
                case 'S':
                case 'T':
                case 'U':
                case 'V':
                case 'W':
                case 'X':
                case 'Y':
                case 'Z':
                case '_':
                case 'a':
                case 'b':
                case 'c':
                case 'd':
                case 'e':
                case 'f':
                case 'g':
                case 'h':
                case 'i':
                case 'j':
                case 'k':
                case 'l':
                case 'm':
                case 'n':
                case 'o':
                case 'p':
                case 'q':
                case 'r':
                case 's':
                case 't':
                case 'u':
                case 'v':
                case 'w':
                case 'x':
                case 'y':
                case 'z':
                    return true;
            }
            return (ch > '\x007f');
        }

        private bool IsDigit(char ch)
        {
            switch (ch)
            {
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return true;
            }
            return false;
        }

        private bool IsWhiteSpace(char ch)
        {
            return ((ch <= ' ') && (ch != '\0'));
        }

        internal void LoadExpression(string data)
        {
            int length;
            if (data == null)
            {
                length = 0;
                this.text = new char[length + 1];
            }
            else
            {
                length = data.Length;
                this.text = new char[length + 1];
                data.CopyTo(0, this.text, 0, length);
            }
            this.text[length] = '\0';
            if (this.expression != null)
            {
                this.expression = null;
            }
        }

        private ExpressionNode NodePeek()
        {
            if (this.topNode <= 0)
            {
                return null;
            }
            return this.NodeStack[this.topNode - 1];
        }

        private ExpressionNode NodePop()
        {
            return this.NodeStack[--this.topNode];
        }

        private void NodePush(ExpressionNode node)
        {
            if (this.topNode >= 0x62)
            {
                throw ExprException.ExpressionTooComplex();
            }
            this.NodeStack[this.topNode++] = node;
        }

        internal ExpressionNode Parse()
        {
            ExpressionNode node;
            OperatorInfo info;
            this.expression = null;
            this.StartScan();
            int num = 0;
            goto Label_078F;
        Label_0014:
            this.Scan();
            switch (this.token)
            {
                case Tokens.Name:
                case Tokens.Numeric:
                case Tokens.Decimal:
                case Tokens.Float:
                case Tokens.StringConst:
                case Tokens.Date:
                case Tokens.Parent:
                {
                    node = null;
                    string constant = null;
                    if (this.prevOperand != 0)
                    {
                        throw ExprException.MissingOperator(new string(this.text, this.start, this.pos - this.start));
                    }
                    if (this.topOperator > 0)
                    {
                        info = this.ops[this.topOperator - 1];
                        if (((info.type == Nodes.Binop) && (info.op == 5)) && (this.token != Tokens.Parent))
                        {
                            throw ExprException.InWithoutParentheses();
                        }
                    }
                    this.prevOperand = 1;
                    switch (this.token)
                    {
                        case Tokens.Name:
                            info = this.ops[this.topOperator - 1];
                            node = new NameNode(this._table, this.text, this.start, this.pos);
                            goto Label_0338;

                        case Tokens.Numeric:
                            constant = new string(this.text, this.start, this.pos - this.start);
                            node = new ConstNode(this._table, System.Data.ValueType.Numeric, constant);
                            goto Label_0338;

                        case Tokens.Decimal:
                            constant = new string(this.text, this.start, this.pos - this.start);
                            node = new ConstNode(this._table, System.Data.ValueType.Decimal, constant);
                            goto Label_0338;

                        case Tokens.Float:
                            constant = new string(this.text, this.start, this.pos - this.start);
                            node = new ConstNode(this._table, System.Data.ValueType.Float, constant);
                            goto Label_0338;

                        case Tokens.StringConst:
                            constant = new string(this.text, this.start + 1, (this.pos - this.start) - 2);
                            node = new ConstNode(this._table, System.Data.ValueType.Str, constant);
                            goto Label_0338;

                        case Tokens.Date:
                            constant = new string(this.text, this.start + 1, (this.pos - this.start) - 2);
                            node = new ConstNode(this._table, System.Data.ValueType.Date, constant);
                            goto Label_0338;

                        case Tokens.Parent:
                        {
                            string str2;
                            try
                            {
                                this.Scan();
                                if (this.token == Tokens.LeftParen)
                                {
                                    this.ScanToken(Tokens.Name);
                                    str2 = NameNode.ParseName(this.text, this.start, this.pos);
                                    this.ScanToken(Tokens.RightParen);
                                    this.ScanToken(Tokens.Dot);
                                }
                                else
                                {
                                    str2 = null;
                                    this.CheckToken(Tokens.Dot);
                                }
                            }
                            catch (Exception exception)
                            {
                                if (!ADP.IsCatchableExceptionType(exception))
                                {
                                    throw;
                                }
                                throw ExprException.LookupArgument();
                            }
                            this.ScanToken(Tokens.Name);
                            string columnName = NameNode.ParseName(this.text, this.start, this.pos);
                            info = this.ops[this.topOperator - 1];
                            node = new LookupNode(this._table, columnName, str2);
                            goto Label_0338;
                        }
                    }
                    break;
                }
                case Tokens.ListSeparator:
                {
                    if (this.prevOperand == 0)
                    {
                        throw ExprException.MissingOperandBefore(",");
                    }
                    this.BuildExpression(3);
                    info = this.ops[this.topOperator - 1];
                    if (info.type != Nodes.Call)
                    {
                        throw ExprException.SyntaxError();
                    }
                    ExpressionNode argument = this.NodePop();
                    FunctionNode node4 = (FunctionNode) this.NodePop();
                    node4.AddArgument(argument);
                    this.NodePush(node4);
                    this.prevOperand = 0;
                    goto Label_0014;
                }
                case Tokens.LeftParen:
                    num++;
                    if (this.prevOperand != 0)
                    {
                        this.BuildExpression(0x16);
                        this.prevOperand = 0;
                        ExpressionNode node5 = this.NodePeek();
                        if ((node5 == null) || (node5.GetType() != typeof(NameNode)))
                        {
                            throw ExprException.SyntaxError();
                        }
                        NameNode node9 = (NameNode) this.NodePop();
                        node = new FunctionNode(this._table, node9.name);
                        Aggregate aggregate = (Aggregate) ((FunctionNode) node).Aggregate;
                        if (aggregate != Aggregate.None)
                        {
                            node = this.ParseAggregateArgument((FunctionId) aggregate);
                            this.NodePush(node);
                            this.prevOperand = 2;
                        }
                        else
                        {
                            this.NodePush(node);
                            this.ops[this.topOperator++] = new OperatorInfo(Nodes.Call, 0, 2);
                        }
                    }
                    else
                    {
                        info = this.ops[this.topOperator - 1];
                        if ((info.type != Nodes.Binop) || (info.op != 5))
                        {
                            this.ops[this.topOperator++] = new OperatorInfo(Nodes.Paren, 0, 2);
                        }
                        else
                        {
                            node = new FunctionNode(this._table, "In");
                            this.NodePush(node);
                            this.ops[this.topOperator++] = new OperatorInfo(Nodes.Call, 0, 2);
                        }
                    }
                    goto Label_0014;

                case Tokens.RightParen:
                    if (this.prevOperand != 0)
                    {
                        this.BuildExpression(3);
                    }
                    if (this.topOperator <= 1)
                    {
                        throw ExprException.TooManyRightParentheses();
                    }
                    this.topOperator--;
                    info = this.ops[this.topOperator];
                    if ((this.prevOperand == 0) && (info.type != Nodes.Call))
                    {
                        throw ExprException.MissingOperand(info);
                    }
                    if (info.type == Nodes.Call)
                    {
                        if (this.prevOperand != 0)
                        {
                            ExpressionNode node8 = this.NodePop();
                            FunctionNode node2 = (FunctionNode) this.NodePop();
                            node2.AddArgument(node8);
                            node2.Check();
                            this.NodePush(node2);
                        }
                    }
                    else
                    {
                        node = this.NodePop();
                        node = new UnaryNode(this._table, 0, node);
                        this.NodePush(node);
                    }
                    this.prevOperand = 2;
                    num--;
                    goto Label_0014;

                case Tokens.ZeroOp:
                    if (this.prevOperand != 0)
                    {
                        throw ExprException.MissingOperator(new string(this.text, this.start, this.pos - this.start));
                    }
                    this.ops[this.topOperator++] = new OperatorInfo(Nodes.Zop, this.op, 0x18);
                    this.prevOperand = 2;
                    goto Label_0014;

                case Tokens.UnaryOp:
                    goto Label_0647;

                case Tokens.BinaryOp:
                    if (this.prevOperand != 0)
                    {
                        this.prevOperand = 0;
                        this.BuildExpression(Operators.Priority(this.op));
                        this.ops[this.topOperator++] = new OperatorInfo(Nodes.Binop, this.op, Operators.Priority(this.op));
                        goto Label_0014;
                    }
                    if (this.op != 15)
                    {
                        if (this.op != 0x10)
                        {
                            throw ExprException.MissingOperandBefore(Operators.ToString(this.op));
                        }
                        this.op = 1;
                    }
                    else
                    {
                        this.op = 2;
                    }
                    goto Label_0647;

                case Tokens.Dot:
                {
                    ExpressionNode node3 = this.NodePeek();
                    if ((node3 == null) || !(node3.GetType() == typeof(NameNode)))
                    {
                        goto Label_0763;
                    }
                    this.Scan();
                    if (this.token != Tokens.Name)
                    {
                        goto Label_0763;
                    }
                    NameNode node6 = (NameNode) this.NodePop();
                    string name = node6.name + "." + NameNode.ParseName(this.text, this.start, this.pos);
                    this.NodePush(new NameNode(this._table, name));
                    goto Label_0014;
                }
                case Tokens.EOS:
                    if (this.prevOperand != 0)
                    {
                        this.BuildExpression(3);
                        if (this.topOperator != 1)
                        {
                            throw ExprException.MissingRightParen();
                        }
                    }
                    else if (this.topNode != 0)
                    {
                        info = this.ops[this.topOperator - 1];
                        throw ExprException.MissingOperand(info);
                    }
                    goto Label_078F;

                default:
                    goto Label_0763;
            }
        Label_0338:
            this.NodePush(node);
            goto Label_0014;
        Label_0647:
            this.ops[this.topOperator++] = new OperatorInfo(Nodes.Unop, this.op, Operators.Priority(this.op));
            goto Label_0014;
        Label_0763:
            throw ExprException.UnknownToken(new string(this.text, this.start, this.pos - this.start), this.start + 1);
        Label_078F:
            if (this.token != Tokens.EOS)
            {
                goto Label_0014;
            }
            this.expression = this.NodeStack[0];
            return this.expression;
        }

        private ExpressionNode ParseAggregateArgument(FunctionId aggregate)
        {
            string str;
            string str2;
            bool flag;
            this.Scan();
            try
            {
                if (this.token != Tokens.Child)
                {
                    if (this.token != Tokens.Name)
                    {
                        throw ExprException.AggregateArgument();
                    }
                    str = NameNode.ParseName(this.text, this.start, this.pos);
                    this.ScanToken(Tokens.RightParen);
                    return new AggregateNode(this._table, aggregate, str);
                }
                flag = this.token == Tokens.Child;
                this.prevOperand = 1;
                this.Scan();
                if (this.token == Tokens.LeftParen)
                {
                    this.ScanToken(Tokens.Name);
                    str2 = NameNode.ParseName(this.text, this.start, this.pos);
                    this.ScanToken(Tokens.RightParen);
                    this.ScanToken(Tokens.Dot);
                }
                else
                {
                    str2 = null;
                    this.CheckToken(Tokens.Dot);
                }
                this.ScanToken(Tokens.Name);
                str = NameNode.ParseName(this.text, this.start, this.pos);
                this.ScanToken(Tokens.RightParen);
            }
            catch (Exception exception)
            {
                if (!ADP.IsCatchableExceptionType(exception))
                {
                    throw;
                }
                throw ExprException.AggregateArgument();
            }
            return new AggregateNode(this._table, aggregate, str, !flag, str2);
        }

        internal Tokens Scan()
        {
            char[] text = this.text;
            this.token = Tokens.None;
        Label_000E:
            this.start = this.pos;
            this.op = 0;
            char ch = text[this.pos++];
            switch (ch)
            {
                case '\t':
                case '\n':
                case '\r':
                case ' ':
                    this.ScanWhite();
                    goto Label_000E;

                case '\0':
                    this.token = Tokens.EOS;
                    break;

                case '#':
                    this.ScanDate();
                    this.CheckToken(Tokens.Date);
                    break;

                case '%':
                    this.token = Tokens.BinaryOp;
                    this.op = 20;
                    break;

                case '&':
                    this.token = Tokens.BinaryOp;
                    this.op = 0x16;
                    break;

                case '\'':
                    this.ScanString('\'');
                    this.CheckToken(Tokens.StringConst);
                    break;

                case '(':
                    this.token = Tokens.LeftParen;
                    break;

                case ')':
                    this.token = Tokens.RightParen;
                    break;

                case '*':
                    this.token = Tokens.BinaryOp;
                    this.op = 0x11;
                    break;

                case '+':
                    this.token = Tokens.BinaryOp;
                    this.op = 15;
                    break;

                case '-':
                    this.token = Tokens.BinaryOp;
                    this.op = 0x10;
                    break;

                case '/':
                    this.token = Tokens.BinaryOp;
                    this.op = 0x12;
                    break;

                case '<':
                    this.token = Tokens.BinaryOp;
                    this.ScanWhite();
                    if (text[this.pos] != '=')
                    {
                        if (text[this.pos] == '>')
                        {
                            this.pos++;
                            this.op = 12;
                        }
                        else
                        {
                            this.op = 9;
                        }
                    }
                    else
                    {
                        this.pos++;
                        this.op = 11;
                    }
                    break;

                case '=':
                    this.token = Tokens.BinaryOp;
                    this.op = 7;
                    break;

                case '>':
                    this.token = Tokens.BinaryOp;
                    this.ScanWhite();
                    if (text[this.pos] != '=')
                    {
                        this.op = 8;
                    }
                    else
                    {
                        this.pos++;
                        this.op = 10;
                    }
                    break;

                case '[':
                    this.ScanName(']', this.Escape, @"]\");
                    this.CheckToken(Tokens.Name);
                    break;

                case '^':
                    this.token = Tokens.BinaryOp;
                    this.op = 0x18;
                    break;

                case '`':
                    this.ScanName('`', '`', "`");
                    this.CheckToken(Tokens.Name);
                    break;

                case '|':
                    this.token = Tokens.BinaryOp;
                    this.op = 0x17;
                    break;

                case '~':
                    this.token = Tokens.BinaryOp;
                    this.op = 0x19;
                    break;

                default:
                    if (ch == this.ListSeparator)
                    {
                        this.token = Tokens.ListSeparator;
                    }
                    else if (ch == '.')
                    {
                        if (this.prevOperand == 0)
                        {
                            this.ScanNumeric();
                        }
                        else
                        {
                            this.token = Tokens.Dot;
                        }
                    }
                    else if ((ch == '0') && ((text[this.pos] == 'x') || (text[this.pos] == 'X')))
                    {
                        this.ScanBinaryConstant();
                        this.token = Tokens.BinaryConst;
                    }
                    else if (this.IsDigit(ch))
                    {
                        this.ScanNumeric();
                    }
                    else
                    {
                        this.ScanReserved();
                        if (this.token == Tokens.None)
                        {
                            if (this.IsAlphaNumeric(ch))
                            {
                                this.ScanName();
                                if (this.token != Tokens.None)
                                {
                                    this.CheckToken(Tokens.Name);
                                    break;
                                }
                            }
                            this.token = Tokens.Unknown;
                            throw ExprException.UnknownToken(new string(text, this.start, this.pos - this.start), this.start + 1);
                        }
                    }
                    break;
            }
            return this.token;
        }

        private void ScanBinaryConstant()
        {
        }

        private void ScanDate()
        {
            char[] text = this.text;
            do
            {
                this.pos++;
            }
            while ((this.pos < text.Length) && (text[this.pos] != '#'));
            if ((this.pos >= text.Length) || (text[this.pos] != '#'))
            {
                if (this.pos >= text.Length)
                {
                    throw ExprException.InvalidDate(new string(text, this.start, (this.pos - 1) - this.start));
                }
                throw ExprException.InvalidDate(new string(text, this.start, this.pos - this.start));
            }
            this.token = Tokens.Date;
            this.pos++;
        }

        private void ScanName()
        {
            char[] text = this.text;
            while (this.IsAlphaNumeric(text[this.pos]))
            {
                this.pos++;
            }
            this.token = Tokens.Name;
        }

        private void ScanName(char chEnd, char esc, string charsToEscape)
        {
            char[] text = this.text;
            do
            {
                if (((text[this.pos] == esc) && ((this.pos + 1) < text.Length)) && (charsToEscape.IndexOf(text[this.pos + 1]) >= 0))
                {
                    this.pos++;
                }
                this.pos++;
            }
            while ((this.pos < text.Length) && (text[this.pos] != chEnd));
            if (this.pos >= text.Length)
            {
                throw ExprException.InvalidNameBracketing(new string(text, this.start, (this.pos - 1) - this.start));
            }
            this.pos++;
            this.token = Tokens.Name;
        }

        private void ScanNumeric()
        {
            char[] text = this.text;
            bool flag2 = false;
            bool flag = false;
            while (this.IsDigit(text[this.pos]))
            {
                this.pos++;
            }
            if (text[this.pos] == this.DecimalSeparator)
            {
                flag2 = true;
                this.pos++;
            }
            while (this.IsDigit(text[this.pos]))
            {
                this.pos++;
            }
            if ((text[this.pos] == this.ExponentL) || (text[this.pos] == this.ExponentU))
            {
                flag = true;
                this.pos++;
                if ((text[this.pos] == '-') || (text[this.pos] == '+'))
                {
                    this.pos++;
                }
                while (this.IsDigit(text[this.pos]))
                {
                    this.pos++;
                }
            }
            if (flag)
            {
                this.token = Tokens.Float;
            }
            else if (flag2)
            {
                this.token = Tokens.Decimal;
            }
            else
            {
                this.token = Tokens.Numeric;
            }
        }

        private void ScanReserved()
        {
            char[] text = this.text;
            if (this.IsAlpha(text[this.pos]))
            {
                this.ScanName();
                string str = new string(text, this.start, this.pos - this.start);
                CompareInfo compareInfo = CultureInfo.InvariantCulture.CompareInfo;
                int num3 = 0;
                int num2 = reservedwords.Length - 1;
                do
                {
                    int index = (num3 + num2) / 2;
                    int num4 = compareInfo.Compare(reservedwords[index].word, str, CompareOptions.IgnoreCase);
                    if (num4 == 0)
                    {
                        this.token = reservedwords[index].token;
                        this.op = reservedwords[index].op;
                        return;
                    }
                    if (num4 < 0)
                    {
                        num3 = index + 1;
                    }
                    else
                    {
                        num2 = index - 1;
                    }
                }
                while (num3 <= num2);
            }
        }

        private void ScanString(char escape)
        {
            char[] text = this.text;
            while (this.pos < text.Length)
            {
                char ch = text[this.pos++];
                if (((ch == escape) && (this.pos < text.Length)) && (text[this.pos] == escape))
                {
                    this.pos++;
                }
                else if (ch == escape)
                {
                    break;
                }
            }
            if (this.pos >= text.Length)
            {
                throw ExprException.InvalidString(new string(text, this.start, (this.pos - 1) - this.start));
            }
            this.token = Tokens.StringConst;
        }

        internal void ScanToken(Tokens token)
        {
            this.Scan();
            this.CheckToken(token);
        }

        private void ScanWhite()
        {
            char[] text = this.text;
            while ((this.pos < text.Length) && this.IsWhiteSpace(text[this.pos]))
            {
                this.pos++;
            }
        }

        internal void StartScan()
        {
            this.op = 0;
            this.pos = 0;
            this.start = 0;
            this.topOperator = 0;
            this.ops[this.topOperator++] = new OperatorInfo(Nodes.Noop, 0, 0);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ReservedWords
        {
            internal readonly string word;
            internal readonly Tokens token;
            internal readonly int op;
            internal ReservedWords(string word, Tokens token, int op)
            {
                this.word = word;
                this.token = token;
                this.op = op;
            }
        }
    }
}

