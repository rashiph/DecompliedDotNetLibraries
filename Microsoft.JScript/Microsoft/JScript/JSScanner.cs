namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Reflection;
    using System.Text;

    public sealed class JSScanner
    {
        private DocumentContext currentDocument;
        private int currentLine;
        private int currentPos;
        private Context currentToken;
        private int endPos;
        private string escapedString;
        private Globals globals;
        private bool gotEndOfLine;
        private StringBuilder identifier;
        private int idLastPosOnBuilder;
        private bool IsAuthoring;
        private JSKeyword[] keywords;
        private int matchIf;
        private bool peekModeOn;
        private SimpleHashtable ppTable;
        private bool preProcessorOn;
        private object preProcessorValue;
        private static readonly JSKeyword[] s_Keywords = JSKeyword.InitKeywords();
        private static readonly OpPrec[] s_OperatorsPrec = InitOperatorsPrec();
        private static readonly OpPrec[] s_PPOperatorsPrec = InitPPOperatorsPrec();
        private bool scanForDebugger;
        private int startLinePos;
        private int startPos;
        private string strSourceCode;

        public JSScanner()
        {
            this.keywords = s_Keywords;
            this.strSourceCode = null;
            this.startPos = 0;
            this.endPos = 0;
            this.currentPos = 0;
            this.currentLine = 1;
            this.startLinePos = 0;
            this.currentToken = null;
            this.escapedString = null;
            this.identifier = new StringBuilder(0x80);
            this.idLastPosOnBuilder = 0;
            this.gotEndOfLine = false;
            this.IsAuthoring = false;
            this.peekModeOn = false;
            this.preProcessorOn = false;
            this.matchIf = 0;
            this.ppTable = null;
            this.currentDocument = null;
            this.globals = null;
            this.scanForDebugger = false;
        }

        public JSScanner(Context sourceContext)
        {
            this.IsAuthoring = false;
            this.peekModeOn = false;
            this.keywords = s_Keywords;
            this.preProcessorOn = false;
            this.matchIf = 0;
            this.ppTable = null;
            this.SetSource(sourceContext);
            this.currentDocument = null;
            this.globals = sourceContext.document.engine.Globals;
        }

        internal static bool CanParseAsExpression(JSToken token)
        {
            return (((JSToken.FirstBinaryOp <= token) && (token <= JSToken.Comma)) || ((JSToken.LeftParen <= token) && (token <= JSToken.AccessField)));
        }

        internal static bool CanStartStatement(JSToken token)
        {
            return ((JSToken.If <= token) && (token <= JSToken.Function));
        }

        private char GetChar(int index)
        {
            if (index < this.endPos)
            {
                return this.strSourceCode[index];
            }
            return '\0';
        }

        public int GetCurrentLine()
        {
            return this.currentLine;
        }

        public int GetCurrentPosition(bool absolute)
        {
            return this.currentPos;
        }

        private int GetHexValue(char hex)
        {
            if (('0' <= hex) && (hex <= '9'))
            {
                return (hex - '0');
            }
            if (('a' <= hex) && (hex <= 'f'))
            {
                return ((hex - 'a') + 10);
            }
            return ((hex - 'A') + 10);
        }

        internal string GetIdentifier()
        {
            string code = null;
            if (this.identifier.Length > 0)
            {
                code = this.identifier.ToString();
                this.identifier.Length = 0;
            }
            else
            {
                code = this.currentToken.GetCode();
            }
            if (code.Length > 500)
            {
                code = code.Substring(0, 500) + code.GetHashCode().ToString(CultureInfo.InvariantCulture);
            }
            return code;
        }

        public void GetNextToken()
        {
            JSToken none = JSToken.None;
            this.gotEndOfLine = false;
            try
            {
                bool flag;
                int currentLine = this.currentLine;
            Label_0010:
                this.SkipBlanks();
                this.currentToken.startPos = this.currentPos;
                this.currentToken.lineNumber = this.currentLine;
                this.currentToken.startLinePos = this.startLinePos;
                char cStringTerminator = this.GetChar(this.currentPos++);
                switch (cStringTerminator)
                {
                    case '\0':
                        if (this.currentPos < this.endPos)
                        {
                            goto Label_0010;
                        }
                        this.currentPos--;
                        none = JSToken.EndOfFile;
                        if (this.matchIf > 0)
                        {
                            this.currentToken.endLineNumber = this.currentLine;
                            this.currentToken.endLinePos = this.startLinePos;
                            this.currentToken.endPos = this.currentPos;
                            this.HandleError(JSError.NoCcEnd);
                        }
                        goto Label_0EB6;

                    case '\n':
                        this.currentLine++;
                        this.startLinePos = this.currentPos;
                        goto Label_0010;

                    case '\r':
                        if (this.GetChar(this.currentPos) == '\n')
                        {
                            this.currentPos++;
                        }
                        this.currentLine++;
                        this.startLinePos = this.currentPos;
                        goto Label_0010;

                    case '!':
                        none = JSToken.FirstOp;
                        if ('=' == this.GetChar(this.currentPos))
                        {
                            this.currentPos++;
                            none = JSToken.NotEqual;
                            if ('=' == this.GetChar(this.currentPos))
                            {
                                this.currentPos++;
                                none = JSToken.StrictNotEqual;
                            }
                        }
                        goto Label_0EB6;

                    case '"':
                    case '\'':
                        none = JSToken.StringLiteral;
                        this.ScanString(cStringTerminator);
                        goto Label_0EB6;

                    case '$':
                    case '_':
                        this.ScanIdentifier();
                        none = JSToken.Identifier;
                        goto Label_0EB6;

                    case '%':
                        none = JSToken.Modulo;
                        if ('=' == this.GetChar(this.currentPos))
                        {
                            this.currentPos++;
                            none = JSToken.ModuloAssign;
                        }
                        goto Label_0EB6;

                    case '&':
                        none = JSToken.BitwiseAnd;
                        cStringTerminator = this.GetChar(this.currentPos);
                        if ('&' != cStringTerminator)
                        {
                            goto Label_0447;
                        }
                        this.currentPos++;
                        none = JSToken.LogicalAnd;
                        goto Label_0EB6;

                    case '(':
                        none = JSToken.LeftParen;
                        goto Label_0EB6;

                    case ')':
                        none = JSToken.RightParen;
                        goto Label_0EB6;

                    case '*':
                        none = JSToken.Multiply;
                        if ('=' == this.GetChar(this.currentPos))
                        {
                            this.currentPos++;
                            none = JSToken.MultiplyAssign;
                        }
                        goto Label_0EB6;

                    case '+':
                        none = JSToken.FirstBinaryOp;
                        cStringTerminator = this.GetChar(this.currentPos);
                        if ('+' != cStringTerminator)
                        {
                            goto Label_04D9;
                        }
                        this.currentPos++;
                        none = JSToken.Increment;
                        goto Label_0EB6;

                    case ',':
                        none = JSToken.Comma;
                        goto Label_0EB6;

                    case '-':
                        none = JSToken.Minus;
                        cStringTerminator = this.GetChar(this.currentPos);
                        if ('-' != cStringTerminator)
                        {
                            goto Label_0522;
                        }
                        this.currentPos++;
                        none = JSToken.Decrement;
                        goto Label_0EB6;

                    case '.':
                        none = JSToken.AccessField;
                        cStringTerminator = this.GetChar(this.currentPos);
                        if (!IsDigit(cStringTerminator))
                        {
                            goto Label_03E7;
                        }
                        none = this.ScanNumber('.');
                        goto Label_0EB6;

                    case '/':
                        none = JSToken.Divide;
                        cStringTerminator = this.GetChar(this.currentPos);
                        flag = false;
                        switch (cStringTerminator)
                        {
                            case '*':
                                goto Label_07C0;

                            case '/':
                                goto Label_0665;

                            case '=':
                                goto Label_0935;
                        }
                        goto Label_0946;

                    case ':':
                        none = JSToken.Colon;
                        if (':' == this.GetChar(this.currentPos))
                        {
                            this.currentPos++;
                            none = JSToken.DoubleColon;
                        }
                        goto Label_0EB6;

                    case ';':
                        none = JSToken.Semicolon;
                        goto Label_0EB6;

                    case '<':
                        none = JSToken.LessThan;
                        if ('<' == this.GetChar(this.currentPos))
                        {
                            this.currentPos++;
                            none = JSToken.LeftShift;
                        }
                        if ('=' == this.GetChar(this.currentPos))
                        {
                            this.currentPos++;
                            if (none == JSToken.LessThan)
                            {
                                none = JSToken.LessThanEqual;
                            }
                            else
                            {
                                none = JSToken.LeftShiftAssign;
                            }
                        }
                        goto Label_0EB6;

                    case '=':
                        none = JSToken.Assign;
                        if ('=' == this.GetChar(this.currentPos))
                        {
                            this.currentPos++;
                            none = JSToken.Equal;
                            if ('=' == this.GetChar(this.currentPos))
                            {
                                this.currentPos++;
                                none = JSToken.StrictEqual;
                            }
                        }
                        goto Label_0EB6;

                    case '>':
                        none = JSToken.GreaterThan;
                        if ('>' == this.GetChar(this.currentPos))
                        {
                            this.currentPos++;
                            none = JSToken.RightShift;
                            if ('>' == this.GetChar(this.currentPos))
                            {
                                this.currentPos++;
                                none = JSToken.UnsignedRightShift;
                            }
                        }
                        if ('=' == this.GetChar(this.currentPos))
                        {
                            this.currentPos++;
                            switch (none)
                            {
                                case JSToken.RightShift:
                                    none = JSToken.RightShiftAssign;
                                    break;

                                case JSToken.UnsignedRightShift:
                                    none = JSToken.UnsignedRightShiftAssign;
                                    break;
                            }
                        }
                        goto Label_0EB6;

                    case '?':
                        none = JSToken.ConditionalIf;
                        goto Label_0EB6;

                    case '@':
                        goto Label_0A8B;

                    case '[':
                        none = JSToken.LeftBracket;
                        goto Label_0EB6;

                    case '\\':
                        this.currentPos--;
                        if (!this.IsIdentifierStartChar(ref cStringTerminator))
                        {
                            goto Label_05A0;
                        }
                        this.currentPos++;
                        this.ScanIdentifier();
                        none = JSToken.Identifier;
                        goto Label_0EB6;

                    case ']':
                        none = JSToken.RightBracket;
                        goto Label_0EB6;

                    case '^':
                        none = JSToken.BitwiseXor;
                        if ('=' == this.GetChar(this.currentPos))
                        {
                            this.currentPos++;
                            none = JSToken.BitwiseXorAssign;
                        }
                        goto Label_0EB6;

                    case '{':
                        none = JSToken.LeftCurly;
                        goto Label_0EB6;

                    case '|':
                        none = JSToken.BitwiseOr;
                        cStringTerminator = this.GetChar(this.currentPos);
                        if ('|' != cStringTerminator)
                        {
                            goto Label_0490;
                        }
                        this.currentPos++;
                        none = JSToken.LogicalOr;
                        goto Label_0EB6;

                    case '}':
                        none = JSToken.RightCurly;
                        goto Label_0EB6;

                    case '~':
                        none = JSToken.BitwiseNot;
                        goto Label_0EB6;

                    case '\u2028':
                        this.currentLine++;
                        this.startLinePos = this.currentPos;
                        goto Label_0010;

                    case '\u2029':
                        this.currentLine++;
                        this.startLinePos = this.currentPos;
                        goto Label_0010;

                    default:
                        if (('a' <= cStringTerminator) && (cStringTerminator <= 'z'))
                        {
                            JSKeyword keyword2 = this.keywords[cStringTerminator - 'a'];
                            if (keyword2 != null)
                            {
                                none = this.ScanKeyword(keyword2);
                            }
                            else
                            {
                                none = JSToken.Identifier;
                                this.ScanIdentifier();
                            }
                        }
                        else if (IsDigit(cStringTerminator))
                        {
                            none = this.ScanNumber(cStringTerminator);
                        }
                        else if ((('A' <= cStringTerminator) && (cStringTerminator <= 'Z')) || IsUnicodeLetter(cStringTerminator))
                        {
                            none = JSToken.Identifier;
                            this.ScanIdentifier();
                        }
                        else
                        {
                            this.HandleError(JSError.IllegalChar);
                            goto Label_0010;
                        }
                        goto Label_0EB6;
                }
                none = JSToken.GreaterThanEqual;
                goto Label_0EB6;
            Label_03E7:
                if ('.' == cStringTerminator)
                {
                    cStringTerminator = this.GetChar(this.currentPos + 1);
                    if ('.' == cStringTerminator)
                    {
                        this.currentPos += 2;
                        none = JSToken.ParamArray;
                    }
                }
                goto Label_0EB6;
            Label_0447:
                if ('=' == cStringTerminator)
                {
                    this.currentPos++;
                    none = JSToken.BitwiseAndAssign;
                }
                goto Label_0EB6;
            Label_0490:
                if ('=' == cStringTerminator)
                {
                    this.currentPos++;
                    none = JSToken.BitwiseOrAssign;
                }
                goto Label_0EB6;
            Label_04D9:
                if ('=' == cStringTerminator)
                {
                    this.currentPos++;
                    none = JSToken.PlusAssign;
                }
                goto Label_0EB6;
            Label_0522:
                if ('=' == cStringTerminator)
                {
                    this.currentPos++;
                    none = JSToken.MinusAssign;
                }
                goto Label_0EB6;
            Label_05A0:
                this.currentPos++;
                cStringTerminator = this.GetChar(this.currentPos);
                if (('a' <= cStringTerminator) && (cStringTerminator <= 'z'))
                {
                    JSKeyword keyword = this.keywords[cStringTerminator - 'a'];
                    if (keyword != null)
                    {
                        this.currentToken.startPos++;
                        none = this.ScanKeyword(keyword);
                        if (none != JSToken.Identifier)
                        {
                            none = JSToken.Identifier;
                            goto Label_0EB6;
                        }
                        this.currentToken.startPos--;
                    }
                }
                this.currentPos = this.currentToken.startPos + 1;
                this.HandleError(JSError.IllegalChar);
                goto Label_0EB6;
            Label_0665:
                if ((this.GetChar(++this.currentPos) == '@') && !this.peekModeOn)
                {
                    if (!this.preProcessorOn)
                    {
                        if (((('c' != this.GetChar(++this.currentPos)) || ('c' != this.GetChar(++this.currentPos))) || (('_' != this.GetChar(++this.currentPos)) || ('o' != this.GetChar(++this.currentPos)))) || ('n' != this.GetChar(++this.currentPos)))
                        {
                            goto Label_07A7;
                        }
                        char c = this.GetChar(this.currentPos + 1);
                        if ((IsDigit(c) || IsAsciiLetter(c)) || IsUnicodeLetter(c))
                        {
                            goto Label_07A7;
                        }
                        this.SetPreProcessorOn();
                        this.currentPos++;
                        goto Label_0010;
                    }
                    if (IsBlankSpace(this.GetChar(++this.currentPos)))
                    {
                        goto Label_0010;
                    }
                    flag = true;
                    goto Label_0946;
                }
            Label_07A7:
                this.SkipSingleLineComment();
                if (!this.IsAuthoring)
                {
                    goto Label_0010;
                }
                none = JSToken.Comment;
                goto Label_0946;
            Label_07C0:
                if ((this.GetChar(++this.currentPos) == '@') && !this.peekModeOn)
                {
                    if (!this.preProcessorOn)
                    {
                        if (((('c' != this.GetChar(++this.currentPos)) || ('c' != this.GetChar(++this.currentPos))) || (('_' != this.GetChar(++this.currentPos)) || ('o' != this.GetChar(++this.currentPos)))) || ('n' != this.GetChar(++this.currentPos)))
                        {
                            goto Label_08FF;
                        }
                        char ch3 = this.GetChar(this.currentPos + 1);
                        if ((IsDigit(ch3) || IsAsciiLetter(ch3)) || IsUnicodeLetter(ch3))
                        {
                            goto Label_08FF;
                        }
                        this.SetPreProcessorOn();
                        this.currentPos++;
                        goto Label_0010;
                    }
                    if (IsBlankSpace(this.GetChar(++this.currentPos)))
                    {
                        goto Label_0010;
                    }
                    flag = true;
                    goto Label_0946;
                }
            Label_08FF:
                this.SkipMultiLineComment();
                if (!this.IsAuthoring)
                {
                    goto Label_0010;
                }
                if (this.currentPos > this.endPos)
                {
                    none = JSToken.UnterminatedComment;
                    this.currentPos = this.endPos;
                }
                else
                {
                    none = JSToken.Comment;
                }
                goto Label_0946;
            Label_0935:
                this.currentPos++;
                none = JSToken.DivideAssign;
            Label_0946:
                if (!flag)
                {
                    goto Label_0EB6;
                }
            Label_0A8B:
                if (this.scanForDebugger)
                {
                    this.HandleError(JSError.CcInvalidInDebugger);
                }
                if (this.peekModeOn)
                {
                    this.currentToken.token = JSToken.PreProcessDirective;
                    goto Label_0EB6;
                }
                int currentPos = this.currentPos;
                this.currentToken.startPos = currentPos;
                this.currentToken.lineNumber = this.currentLine;
                this.currentToken.startLinePos = this.startLinePos;
                this.ScanIdentifier();
                switch ((this.currentPos - currentPos))
                {
                    case 0:
                        if ((!this.preProcessorOn || ('*' != this.GetChar(this.currentPos))) || ('/' != this.GetChar(++this.currentPos)))
                        {
                            break;
                        }
                        this.currentPos++;
                        goto Label_0010;

                    case 2:
                        if (('i' != this.strSourceCode[currentPos]) || ('f' != this.strSourceCode[currentPos + 1]))
                        {
                            goto Label_0DD9;
                        }
                        if (!this.preProcessorOn)
                        {
                            this.SetPreProcessorOn();
                        }
                        this.matchIf++;
                        if (!this.PPTestCond())
                        {
                            this.PPSkipToNextCondition(true);
                        }
                        goto Label_0010;

                    case 3:
                        if ((('s' != this.strSourceCode[currentPos]) || ('e' != this.strSourceCode[currentPos + 1])) || ('t' != this.strSourceCode[currentPos + 2]))
                        {
                            goto Label_0C2C;
                        }
                        if (!this.preProcessorOn)
                        {
                            this.SetPreProcessorOn();
                        }
                        this.PPScanSet();
                        goto Label_0010;

                    case 4:
                        if ((('e' != this.strSourceCode[currentPos]) || ('l' != this.strSourceCode[currentPos + 1])) || (('s' != this.strSourceCode[currentPos + 2]) || ('e' != this.strSourceCode[currentPos + 3])))
                        {
                            goto Label_0D07;
                        }
                        if (0 < this.matchIf)
                        {
                            goto Label_0CFB;
                        }
                        this.HandleError(JSError.CcInvalidElse);
                        goto Label_0010;

                    case 5:
                        if (((('c' != this.GetChar(currentPos)) || ('c' != this.GetChar(currentPos + 1))) || (('_' != this.GetChar(currentPos + 2)) || ('o' != this.GetChar(currentPos + 3)))) || ('n' != this.GetChar(currentPos + 4)))
                        {
                            goto Label_0DD9;
                        }
                        if (!this.preProcessorOn)
                        {
                            this.SetPreProcessorOn();
                        }
                        goto Label_0010;

                    default:
                        goto Label_0DD9;
                }
                this.HandleError(JSError.IllegalChar);
                goto Label_0010;
            Label_0C2C:
                if ((('e' != this.strSourceCode[currentPos]) || ('n' != this.strSourceCode[currentPos + 1])) || ('d' != this.strSourceCode[currentPos + 2]))
                {
                    goto Label_0DD9;
                }
                if (0 >= this.matchIf)
                {
                    this.HandleError(JSError.CcInvalidEnd);
                }
                else
                {
                    this.matchIf--;
                }
                goto Label_0010;
            Label_0CFB:
                this.PPSkipToNextCondition(false);
                goto Label_0010;
            Label_0D07:
                if ((('e' == this.strSourceCode[currentPos]) && ('l' == this.strSourceCode[currentPos + 1])) && (('i' == this.strSourceCode[currentPos + 2]) && ('f' == this.strSourceCode[currentPos + 3])))
                {
                    if (0 >= this.matchIf)
                    {
                        this.HandleError(JSError.CcInvalidElif);
                    }
                    else
                    {
                        this.PPSkipToNextCondition(false);
                    }
                    goto Label_0010;
                }
            Label_0DD9:
                if (!this.preProcessorOn)
                {
                    this.HandleError(JSError.CcOff);
                    goto Label_0010;
                }
                object obj2 = this.ppTable[this.strSourceCode.Substring(currentPos, this.currentPos - currentPos)];
                if (obj2 == null)
                {
                    this.preProcessorValue = (double) 1.0 / (double) 0.0;
                }
                else
                {
                    this.preProcessorValue = obj2;
                }
                none = JSToken.PreProcessorConstant;
            Label_0EB6:
                this.currentToken.endLineNumber = this.currentLine;
                this.currentToken.endLinePos = this.startLinePos;
                this.currentToken.endPos = this.currentPos;
                this.gotEndOfLine = (this.currentLine > currentLine) || (none == JSToken.EndOfFile);
                if ((this.gotEndOfLine && (none == JSToken.StringLiteral)) && (this.currentToken.lineNumber == currentLine))
                {
                    this.gotEndOfLine = false;
                }
            }
            catch (IndexOutOfRangeException)
            {
                none = JSToken.None;
                this.currentToken.endPos = this.currentPos;
                this.currentToken.endLineNumber = this.currentLine;
                this.currentToken.endLinePos = this.startLinePos;
                throw new ScannerException(JSError.ErrEOF);
            }
            this.currentToken.token = none;
        }

        internal static OpPrec GetOperatorPrecedence(JSToken token)
        {
            return s_OperatorsPrec[((int) token) - 0x2e];
        }

        internal static OpPrec GetPPOperatorPrecedence(JSToken token)
        {
            return s_PPOperatorsPrec[((int) token) - 0x2e];
        }

        internal object GetPreProcessorValue()
        {
            return this.preProcessorValue;
        }

        public string GetSourceCode()
        {
            return this.strSourceCode;
        }

        public int GetStartLinePosition()
        {
            return this.startLinePos;
        }

        public string GetStringLiteral()
        {
            return this.escapedString;
        }

        public bool GotEndOfLine()
        {
            return this.gotEndOfLine;
        }

        private void HandleError(JSError error)
        {
            if (!this.IsAuthoring)
            {
                this.currentToken.HandleError(error);
            }
        }

        private static OpPrec[] InitOperatorsPrec()
        {
            OpPrec[] precArray = new OpPrec[0x24];
            precArray[0] = OpPrec.precAdditive;
            precArray[1] = OpPrec.precAdditive;
            precArray[2] = OpPrec.precLogicalOr;
            precArray[3] = OpPrec.precLogicalAnd;
            precArray[4] = OpPrec.precBitwiseOr;
            precArray[5] = OpPrec.precBitwiseXor;
            precArray[6] = OpPrec.precBitwiseAnd;
            precArray[7] = OpPrec.precEquality;
            precArray[8] = OpPrec.precEquality;
            precArray[9] = OpPrec.precEquality;
            precArray[10] = OpPrec.precEquality;
            precArray[0x15] = OpPrec.precRelational;
            precArray[0x16] = OpPrec.precRelational;
            precArray[11] = OpPrec.precRelational;
            precArray[12] = OpPrec.precRelational;
            precArray[13] = OpPrec.precRelational;
            precArray[14] = OpPrec.precRelational;
            precArray[15] = OpPrec.precShift;
            precArray[0x10] = OpPrec.precShift;
            precArray[0x11] = OpPrec.precShift;
            precArray[0x12] = OpPrec.precMultiplicative;
            precArray[0x13] = OpPrec.precMultiplicative;
            precArray[20] = OpPrec.precMultiplicative;
            precArray[0x17] = OpPrec.precAssignment;
            precArray[0x18] = OpPrec.precAssignment;
            precArray[0x19] = OpPrec.precAssignment;
            precArray[0x1a] = OpPrec.precAssignment;
            precArray[0x1b] = OpPrec.precAssignment;
            precArray[0x1c] = OpPrec.precAssignment;
            precArray[0x1d] = OpPrec.precAssignment;
            precArray[30] = OpPrec.precAssignment;
            precArray[0x1f] = OpPrec.precAssignment;
            precArray[0x20] = OpPrec.precAssignment;
            precArray[0x21] = OpPrec.precAssignment;
            precArray[0x22] = OpPrec.precAssignment;
            precArray[0x23] = OpPrec.precConditional;
            return precArray;
        }

        private static OpPrec[] InitPPOperatorsPrec()
        {
            return new OpPrec[] { 
                OpPrec.precAdditive, OpPrec.precAdditive, OpPrec.precLogicalOr, OpPrec.precLogicalAnd, OpPrec.precBitwiseOr, OpPrec.precBitwiseXor, OpPrec.precBitwiseAnd, OpPrec.precEquality, OpPrec.precEquality, OpPrec.precEquality, OpPrec.precEquality, OpPrec.precRelational, OpPrec.precRelational, OpPrec.precRelational, OpPrec.precRelational, OpPrec.precShift, 
                OpPrec.precShift, OpPrec.precShift, OpPrec.precMultiplicative, OpPrec.precMultiplicative, OpPrec.precMultiplicative
             };
        }

        internal static bool IsAsciiLetter(char c)
        {
            return ((('A' <= c) && (c <= 'Z')) || (('a' <= c) && (c <= 'z')));
        }

        internal static bool IsAssignmentOperator(JSToken token)
        {
            return ((JSToken.Assign <= token) && (token <= JSToken.UnsignedRightShiftAssign));
        }

        private static bool IsBlankSpace(char c)
        {
            switch (c)
            {
                case '\t':
                case '\v':
                case '\f':
                case ' ':
                case '\x00a0':
                    return true;
            }
            return ((c >= '\x0080') && (char.GetUnicodeCategory(c) == UnicodeCategory.SpaceSeparator));
        }

        internal static bool IsDigit(char c)
        {
            return (('0' <= c) && (c <= '9'));
        }

        private bool IsEndLineOrEOF(char c, int increment)
        {
            return (this.IsLineTerminator(c, increment) || ((c == '\0') && (this.currentPos >= this.endPos)));
        }

        internal static bool IsHexDigit(char c)
        {
            return ((IsDigit(c) || (('A' <= c) && (c <= 'F'))) || (('a' <= c) && (c <= 'f')));
        }

        internal bool IsIdentifierPartChar(char c)
        {
            if (this.IsIdentifierStartChar(ref c))
            {
                return true;
            }
            if (('0' <= c) && (c <= '9'))
            {
                return true;
            }
            if (c >= '\x0080')
            {
                switch (char.GetUnicodeCategory(c))
                {
                    case UnicodeCategory.NonSpacingMark:
                    case UnicodeCategory.SpacingCombiningMark:
                    case UnicodeCategory.DecimalDigitNumber:
                    case UnicodeCategory.ConnectorPunctuation:
                        return true;
                }
            }
            return false;
        }

        internal bool IsIdentifierStartChar(ref char c)
        {
            bool flag = false;
            if (('\\' == c) && ('u' == this.GetChar(this.currentPos + 1)))
            {
                char ch = this.GetChar(this.currentPos + 2);
                if (IsHexDigit(ch))
                {
                    char ch2 = this.GetChar(this.currentPos + 3);
                    if (IsHexDigit(ch2))
                    {
                        char ch3 = this.GetChar(this.currentPos + 4);
                        if (IsHexDigit(ch3))
                        {
                            char ch4 = this.GetChar(this.currentPos + 5);
                            if (IsHexDigit(ch4))
                            {
                                flag = true;
                                c = (char) ((((this.GetHexValue(ch) << 12) | (this.GetHexValue(ch2) << 8)) | (this.GetHexValue(ch3) << 4)) | this.GetHexValue(ch4));
                            }
                        }
                    }
                }
            }
            if (((('a' > c) || (c > 'z')) && (('A' > c) || (c > 'Z'))) && (('_' != c) && ('$' != c)))
            {
                if (c >= '\x0080')
                {
                    switch (char.GetUnicodeCategory(c))
                    {
                        case UnicodeCategory.UppercaseLetter:
                        case UnicodeCategory.LowercaseLetter:
                        case UnicodeCategory.TitlecaseLetter:
                        case UnicodeCategory.ModifierLetter:
                        case UnicodeCategory.OtherLetter:
                        case UnicodeCategory.LetterNumber:
                            goto Label_0117;
                    }
                }
                return false;
            }
        Label_0117:
            if (flag)
            {
                int startIndex = (this.idLastPosOnBuilder > 0) ? this.idLastPosOnBuilder : this.currentToken.startPos;
                if ((this.currentPos - startIndex) > 0)
                {
                    this.identifier.Append(this.strSourceCode.Substring(startIndex, this.currentPos - startIndex));
                }
                this.identifier.Append((char) c);
                this.currentPos += 5;
                this.idLastPosOnBuilder = this.currentPos + 1;
            }
            return true;
        }

        public static bool IsKeyword(JSToken token)
        {
            switch (token)
            {
                case JSToken.If:
                case JSToken.For:
                case JSToken.Do:
                case JSToken.While:
                case JSToken.Continue:
                case JSToken.Break:
                case JSToken.Return:
                case JSToken.Import:
                case JSToken.With:
                case JSToken.Switch:
                case JSToken.Throw:
                case JSToken.Try:
                case JSToken.Package:
                case JSToken.Abstract:
                case JSToken.Public:
                case JSToken.Static:
                case JSToken.Private:
                case JSToken.Protected:
                case JSToken.Final:
                case JSToken.Var:
                case JSToken.Const:
                case JSToken.Class:
                case JSToken.Function:
                case JSToken.Null:
                case JSToken.True:
                case JSToken.False:
                case JSToken.This:
                case JSToken.Delete:
                case JSToken.Void:
                case JSToken.Typeof:
                case JSToken.Instanceof:
                case JSToken.In:
                case JSToken.Case:
                case JSToken.Catch:
                case JSToken.Debugger:
                case JSToken.Default:
                case JSToken.Else:
                case JSToken.Export:
                case JSToken.Extends:
                case JSToken.Finally:
                case JSToken.Get:
                case JSToken.Implements:
                case JSToken.Interface:
                case JSToken.New:
                case JSToken.Set:
                case JSToken.Super:
                case JSToken.Boolean:
                case JSToken.Byte:
                case JSToken.Char:
                case JSToken.Double:
                case JSToken.Enum:
                case JSToken.Float:
                case JSToken.Goto:
                case JSToken.Int:
                case JSToken.Long:
                case JSToken.Native:
                case JSToken.Short:
                case JSToken.Synchronized:
                case JSToken.Transient:
                case JSToken.Throws:
                case JSToken.Volatile:
                    return true;
            }
            return false;
        }

        private bool IsLineTerminator(char c, int increment)
        {
            switch (c)
            {
                case '\u2028':
                    return true;

                case '\u2029':
                    return true;

                case '\r':
                    if ('\n' == this.GetChar(this.currentPos + increment))
                    {
                        this.currentPos++;
                    }
                    return true;

                case '\n':
                    return true;
            }
            return false;
        }

        public static bool IsOperator(JSToken token)
        {
            return ((JSToken.FirstOp <= token) && (token <= JSToken.Comma));
        }

        internal static bool IsPPOperator(JSToken token)
        {
            return ((JSToken.FirstBinaryOp <= token) && (token <= JSToken.Modulo));
        }

        internal static bool IsProcessableOperator(JSToken token)
        {
            return ((JSToken.FirstBinaryOp <= token) && (token <= JSToken.ConditionalIf));
        }

        internal static bool IsRightAssociativeOperator(JSToken token)
        {
            return ((JSToken.Assign <= token) && (token <= JSToken.ConditionalIf));
        }

        internal static bool IsUnicodeLetter(char c)
        {
            return ((c >= '\x0080') && char.IsLetter(c));
        }

        internal JSToken PeekToken()
        {
            int currentPos = this.currentPos;
            int currentLine = this.currentLine;
            int startLinePos = this.startLinePos;
            bool gotEndOfLine = this.gotEndOfLine;
            int idLastPosOnBuilder = this.idLastPosOnBuilder;
            this.peekModeOn = true;
            JSToken none = JSToken.None;
            Context currentToken = this.currentToken;
            this.currentToken = this.currentToken.Clone();
            try
            {
                this.GetNextToken();
                none = this.currentToken.token;
            }
            finally
            {
                this.currentToken = currentToken;
                this.currentPos = currentPos;
                this.currentLine = currentLine;
                this.startLinePos = startLinePos;
                this.gotEndOfLine = gotEndOfLine;
                this.identifier.Length = 0;
                this.idLastPosOnBuilder = idLastPosOnBuilder;
                this.peekModeOn = false;
                this.escapedString = null;
            }
            return none;
        }

        private void PPDebugDirective()
        {
            this.GetNextToken();
            bool flag = false;
            if (JSToken.Identifier == this.currentToken.token)
            {
                if (this.currentToken.Equals("off"))
                {
                    flag = false;
                }
                else if (this.currentToken.Equals("on"))
                {
                    flag = true;
                }
                else
                {
                    this.HandleError(JSError.InvalidDebugDirective);
                    goto Label_00C3;
                }
                this.GetNextToken();
                if (JSToken.RightParen == this.currentToken.token)
                {
                    this.currentToken.document.debugOn = flag && this.globals.engine.GenerateDebugInfo;
                    this.ppTable["_debug"] = flag;
                }
                else
                {
                    this.HandleError(JSError.InvalidDebugDirective);
                }
            }
            else
            {
                this.HandleError(JSError.InvalidDebugDirective);
            }
        Label_00C3:
            while (JSToken.RightParen != this.currentToken.token)
            {
                this.GetNextToken();
            }
            this.SkipBlanks();
            if (';' == this.GetChar(this.currentPos))
            {
                this.currentPos++;
                this.SkipBlanks();
            }
            if (!this.IsLineTerminator(this.GetChar(this.currentPos++), 0))
            {
                this.HandleError(JSError.MustBeEOL);
                while (!this.IsLineTerminator(this.GetChar(this.currentPos++), 0))
                {
                }
            }
            this.currentLine++;
            this.startLinePos = this.currentPos;
        }

        private object PPGetValue(JSToken op, object op1, object op2)
        {
            switch (op)
            {
                case JSToken.FirstBinaryOp:
                    return (Microsoft.JScript.Convert.ToNumber(op1) + Microsoft.JScript.Convert.ToNumber(op2));

                case JSToken.Minus:
                    return (Microsoft.JScript.Convert.ToNumber(op1) - Microsoft.JScript.Convert.ToNumber(op2));

                case JSToken.LogicalOr:
                    return (Microsoft.JScript.Convert.ToBoolean(op1) ? ((object) 1) : ((object) Microsoft.JScript.Convert.ToBoolean(op2)));

                case JSToken.LogicalAnd:
                    return (!Microsoft.JScript.Convert.ToBoolean(op1) ? ((object) 0) : ((object) Microsoft.JScript.Convert.ToBoolean(op2)));

                case JSToken.BitwiseOr:
                    return (Microsoft.JScript.Convert.ToInt32(op1) | Microsoft.JScript.Convert.ToInt32(op2));

                case JSToken.BitwiseXor:
                    return (Microsoft.JScript.Convert.ToInt32(op1) ^ Microsoft.JScript.Convert.ToInt32(op2));

                case JSToken.BitwiseAnd:
                    return (Microsoft.JScript.Convert.ToInt32(op1) & Microsoft.JScript.Convert.ToInt32(op2));

                case JSToken.Equal:
                    return (Microsoft.JScript.Convert.ToNumber(op1) == Microsoft.JScript.Convert.ToNumber(op2));

                case JSToken.NotEqual:
                    return !(Microsoft.JScript.Convert.ToNumber(op1) == Microsoft.JScript.Convert.ToNumber(op2));

                case JSToken.StrictEqual:
                    return (op1 == op2);

                case JSToken.StrictNotEqual:
                    return (op1 != op2);

                case JSToken.GreaterThan:
                    return (Microsoft.JScript.Convert.ToNumber(op1) > Microsoft.JScript.Convert.ToNumber(op2));

                case JSToken.LessThan:
                    return (Microsoft.JScript.Convert.ToNumber(op1) < Microsoft.JScript.Convert.ToNumber(op2));

                case JSToken.LessThanEqual:
                    return (Microsoft.JScript.Convert.ToNumber(op1) <= Microsoft.JScript.Convert.ToNumber(op2));

                case JSToken.GreaterThanEqual:
                    return (Microsoft.JScript.Convert.ToNumber(op1) >= Microsoft.JScript.Convert.ToNumber(op2));

                case JSToken.LeftShift:
                    return (Microsoft.JScript.Convert.ToInt32(op1) << Microsoft.JScript.Convert.ToInt32(op2));

                case JSToken.RightShift:
                    return (Microsoft.JScript.Convert.ToInt32(op1) >> Microsoft.JScript.Convert.ToInt32(op2));

                case JSToken.UnsignedRightShift:
                    return (uint) (Microsoft.JScript.Convert.ToInt32(op1) >> Microsoft.JScript.Convert.ToInt32(op2));

                case JSToken.Multiply:
                    return (Microsoft.JScript.Convert.ToNumber(op1) * Microsoft.JScript.Convert.ToNumber(op2));

                case JSToken.Divide:
                    return (Microsoft.JScript.Convert.ToNumber(op1) / Microsoft.JScript.Convert.ToNumber(op2));

                case JSToken.Modulo:
                    return (Microsoft.JScript.Convert.ToInt32(op1) % Microsoft.JScript.Convert.ToInt32(op2));
            }
            return null;
        }

        private void PPLanguageOption()
        {
            this.GetNextToken();
            this.HandleError(JSError.InvalidLanguageOption);
            this.GetNextToken();
            Context context = null;
            while (JSToken.RightParen != this.currentToken.token)
            {
                if (context == null)
                {
                    context = this.currentToken.Clone();
                }
                else
                {
                    context.UpdateWith(this.currentToken);
                }
                this.GetNextToken();
            }
            if (context != null)
            {
                this.HandleError(JSError.NoRightParen);
            }
        }

        private void PPRemapPositionInfo()
        {
            this.GetNextToken();
            string documentName = null;
            int startLine = 0;
            int startCol = -1;
            bool flag = false;
            while (JSToken.RightParen != this.currentToken.token)
            {
                if (JSToken.Identifier != this.currentToken.token)
                {
                    goto Label_0342;
                }
                if (this.currentToken.Equals("file"))
                {
                    if (this.currentDocument == null)
                    {
                        if (documentName == null)
                        {
                            this.GetNextToken();
                            if (JSToken.Assign != this.currentToken.token)
                            {
                                this.HandleError(JSError.InvalidPositionDirective);
                            }
                            else
                            {
                                this.GetNextToken();
                                if (JSToken.StringLiteral != this.currentToken.token)
                                {
                                    this.HandleError(JSError.InvalidPositionDirective);
                                }
                                else
                                {
                                    documentName = this.GetStringLiteral();
                                    if (!(documentName == this.currentToken.document.documentName))
                                    {
                                        goto Label_0316;
                                    }
                                    documentName = null;
                                    this.HandleError(JSError.InvalidPositionDirective);
                                }
                            }
                        }
                        else
                        {
                            this.HandleError(JSError.InvalidPositionDirective);
                        }
                    }
                    else
                    {
                        this.HandleError(JSError.CannotNestPositionDirective);
                    }
                }
                else if (this.currentToken.Equals("line"))
                {
                    if (this.currentDocument == null)
                    {
                        if (startLine == 0)
                        {
                            this.GetNextToken();
                            if (JSToken.Assign != this.currentToken.token)
                            {
                                this.HandleError(JSError.InvalidPositionDirective);
                            }
                            else
                            {
                                this.GetNextToken();
                                if (JSToken.IntegerLiteral != this.currentToken.token)
                                {
                                    this.HandleError(JSError.InvalidPositionDirective);
                                }
                                else
                                {
                                    double num3 = Microsoft.JScript.Convert.ToNumber(this.currentToken.GetCode(), true, true, Microsoft.JScript.Missing.Value);
                                    if ((((int) num3) == num3) && (num3 > 0.0))
                                    {
                                        startLine = (int) num3;
                                        goto Label_0316;
                                    }
                                    startLine = 1;
                                    this.HandleError(JSError.InvalidPositionDirective);
                                }
                            }
                        }
                        else
                        {
                            this.HandleError(JSError.InvalidPositionDirective);
                        }
                    }
                    else
                    {
                        this.HandleError(JSError.CannotNestPositionDirective);
                    }
                }
                else if (this.currentToken.Equals("column"))
                {
                    if (this.currentDocument == null)
                    {
                        if (startCol == -1)
                        {
                            this.GetNextToken();
                            if (JSToken.Assign != this.currentToken.token)
                            {
                                this.HandleError(JSError.InvalidPositionDirective);
                            }
                            else
                            {
                                this.GetNextToken();
                                if (JSToken.IntegerLiteral != this.currentToken.token)
                                {
                                    this.HandleError(JSError.InvalidPositionDirective);
                                }
                                else
                                {
                                    double num4 = Microsoft.JScript.Convert.ToNumber(this.currentToken.GetCode(), true, true, Microsoft.JScript.Missing.Value);
                                    if ((((int) num4) == num4) && (num4 >= 0.0))
                                    {
                                        startCol = (int) num4;
                                        goto Label_0316;
                                    }
                                    startCol = 0;
                                    this.HandleError(JSError.InvalidPositionDirective);
                                }
                            }
                        }
                        else
                        {
                            this.HandleError(JSError.InvalidPositionDirective);
                        }
                    }
                    else
                    {
                        this.HandleError(JSError.CannotNestPositionDirective);
                    }
                }
                else if (this.currentToken.Equals("end"))
                {
                    if (this.currentDocument != null)
                    {
                        this.GetNextToken();
                        if (JSToken.RightParen != this.currentToken.token)
                        {
                            this.HandleError(JSError.InvalidPositionDirective);
                            goto Label_0355;
                        }
                        this.currentToken.document = this.currentDocument;
                        this.currentDocument = null;
                        flag = true;
                        break;
                    }
                    this.HandleError(JSError.WrongDirective);
                }
                else
                {
                    this.HandleError(JSError.InvalidPositionDirective);
                }
                goto Label_0355;
            Label_0316:
                this.GetNextToken();
                if (JSToken.RightParen == this.currentToken.token)
                {
                    break;
                }
                if (JSToken.Semicolon == this.currentToken.token)
                {
                    this.GetNextToken();
                }
                continue;
            Label_0342:
                this.HandleError(JSError.InvalidPositionDirective);
            Label_0355:
                while ((JSToken.RightParen != this.currentToken.token) && (this.currentToken.token != JSToken.EndOfFile))
                {
                    this.GetNextToken();
                }
                break;
            }
            this.SkipBlanks();
            if (';' == this.GetChar(this.currentPos))
            {
                this.currentPos++;
                this.SkipBlanks();
            }
            if ((this.currentPos < this.endPos) && !this.IsLineTerminator(this.GetChar(this.currentPos++), 0))
            {
                this.HandleError(JSError.MustBeEOL);
                while ((this.currentPos < this.endPos) && !this.IsLineTerminator(this.GetChar(this.currentPos++), 0))
                {
                }
            }
            this.currentLine++;
            this.startLinePos = this.currentPos;
            if (!flag)
            {
                if (((documentName == null) && (startLine == 0)) && (startCol == -1))
                {
                    this.HandleError(JSError.InvalidPositionDirective);
                }
                else
                {
                    if (documentName == null)
                    {
                        documentName = this.currentToken.document.documentName;
                    }
                    if (startLine == 0)
                    {
                        startLine = 1;
                    }
                    if (startCol == -1)
                    {
                        startCol = 0;
                    }
                    this.currentDocument = this.currentToken.document;
                    this.currentToken.document = new DocumentContext(documentName, startLine, startCol, this.currentLine, this.currentDocument.sourceItem);
                }
            }
        }

        private object PPScanConstant()
        {
            this.GetNextToken();
            switch (this.currentToken.token)
            {
                case JSToken.True:
                    return true;

                case JSToken.False:
                    return false;

                case JSToken.IntegerLiteral:
                    return Microsoft.JScript.Convert.ToNumber(this.currentToken.GetCode(), true, true, Microsoft.JScript.Missing.Value);

                case JSToken.NumericLiteral:
                    return Microsoft.JScript.Convert.ToNumber(this.currentToken.GetCode(), false, false, Microsoft.JScript.Missing.Value);

                case JSToken.LeftParen:
                {
                    object obj2 = this.PPScanExpr();
                    this.GetNextToken();
                    if (JSToken.RightParen != this.currentToken.token)
                    {
                        this.currentToken.endPos = this.currentToken.startPos + 1;
                        this.currentToken.endLineNumber = this.currentLine;
                        this.currentToken.endLinePos = this.startLinePos;
                        this.HandleError(JSError.NoRightParen);
                        this.currentPos = this.currentToken.startPos;
                    }
                    return obj2;
                }
                case JSToken.FirstOp:
                    return !Microsoft.JScript.Convert.ToBoolean(this.PPScanConstant());

                case JSToken.BitwiseNot:
                    return ~Microsoft.JScript.Convert.ToInt32(this.PPScanConstant());

                case JSToken.FirstBinaryOp:
                    return Microsoft.JScript.Convert.ToNumber(this.PPScanConstant());

                case JSToken.Minus:
                    return -Microsoft.JScript.Convert.ToNumber(this.PPScanConstant());

                case JSToken.PreProcessorConstant:
                    return this.preProcessorValue;
            }
            this.HandleError(JSError.NotConst);
            this.currentPos = this.currentToken.startPos;
            return true;
        }

        private object PPScanExpr()
        {
            object obj2;
            OpListItem prev = new OpListItem(JSToken.None, OpPrec.precNone, null);
            ConstantListItem item2 = new ConstantListItem(this.PPScanConstant(), null);
            while (true)
            {
                this.GetNextToken();
                if (!IsPPOperator(this.currentToken.token))
                {
                    goto Label_00E4;
                }
                OpPrec pPOperatorPrecedence = GetPPOperatorPrecedence(this.currentToken.token);
                while (pPOperatorPrecedence < prev._prec)
                {
                    obj2 = this.PPGetValue(prev._operator, item2.prev.term, item2.term);
                    prev = prev._prev;
                    item2 = item2.prev.prev;
                    item2 = new ConstantListItem(obj2, item2);
                }
                prev = new OpListItem(this.currentToken.token, pPOperatorPrecedence, prev);
                item2 = new ConstantListItem(this.PPScanConstant(), item2);
            }
        Label_00AB:
            obj2 = this.PPGetValue(prev._operator, item2.prev.term, item2.term);
            prev = prev._prev;
            item2 = item2.prev.prev;
            item2 = new ConstantListItem(obj2, item2);
        Label_00E4:
            if (prev._operator != JSToken.None)
            {
                goto Label_00AB;
            }
            this.currentPos = this.currentToken.startPos;
            return item2.term;
        }

        private void PPScanSet()
        {
            this.SkipBlanks();
            if ('@' != this.GetChar(this.currentPos++))
            {
                this.HandleError(JSError.NoAt);
                this.currentPos--;
            }
            int currentPos = this.currentPos;
            this.ScanIdentifier();
            int length = this.currentPos - currentPos;
            string str = null;
            if (length == 0)
            {
                this.currentToken.startPos = this.currentPos - 1;
                this.currentToken.lineNumber = this.currentLine;
                this.currentToken.startLinePos = this.startLinePos;
                this.HandleError(JSError.NoIdentifier);
                str = "#_Missing CC Identifier_#";
            }
            else
            {
                str = this.strSourceCode.Substring(currentPos, length);
            }
            this.SkipBlanks();
            char ch = this.GetChar(this.currentPos++);
            if ('(' == ch)
            {
                if (str.Equals("position"))
                {
                    this.PPRemapPositionInfo();
                }
                else if (str.Equals("option"))
                {
                    this.PPLanguageOption();
                }
                else if (str.Equals("debug"))
                {
                    this.PPDebugDirective();
                }
                else
                {
                    this.currentToken.startPos = this.currentPos - 1;
                    this.currentToken.lineNumber = this.currentLine;
                    this.currentToken.startLinePos = this.startLinePos;
                    this.HandleError(JSError.NoEqual);
                    this.currentPos--;
                }
            }
            else
            {
                if ('=' != ch)
                {
                    this.currentToken.startPos = this.currentPos - 1;
                    this.currentToken.lineNumber = this.currentLine;
                    this.currentToken.startLinePos = this.startLinePos;
                    this.HandleError(JSError.NoEqual);
                    this.currentPos--;
                }
                object obj2 = this.PPScanConstant();
                this.ppTable[str] = obj2;
            }
        }

        private void PPSkipToNextCondition(bool checkCondition)
        {
            int num = 0;
            while (true)
            {
                switch (this.GetChar(this.currentPos++))
                {
                    case '\u2028':
                        this.currentLine++;
                        this.startLinePos = this.currentPos;
                        break;

                    case '\u2029':
                        this.currentLine++;
                        this.startLinePos = this.currentPos;
                        break;

                    case '@':
                        this.currentToken.startPos = this.currentPos;
                        this.currentToken.lineNumber = this.currentLine;
                        this.currentToken.startLinePos = this.startLinePos;
                        this.ScanIdentifier();
                        switch ((this.currentPos - this.currentToken.startPos))
                        {
                            case 2:
                                if (('i' == this.strSourceCode[this.currentToken.startPos]) && ('f' == this.strSourceCode[this.currentToken.startPos + 1]))
                                {
                                    num++;
                                }
                                break;

                            case 3:
                                if ((('e' != this.strSourceCode[this.currentToken.startPos]) || ('n' != this.strSourceCode[this.currentToken.startPos + 1])) || ('d' != this.strSourceCode[this.currentToken.startPos + 2]))
                                {
                                    break;
                                }
                                if (num != 0)
                                {
                                    num--;
                                    break;
                                }
                                this.matchIf--;
                                return;

                            case 4:
                                if ((('e' != this.strSourceCode[this.currentToken.startPos]) || ('l' != this.strSourceCode[this.currentToken.startPos + 1])) || (('s' != this.strSourceCode[this.currentToken.startPos + 2]) || ('e' != this.strSourceCode[this.currentToken.startPos + 3])))
                                {
                                    if (((('e' != this.strSourceCode[this.currentToken.startPos]) || ('l' != this.strSourceCode[this.currentToken.startPos + 1])) || (('i' != this.strSourceCode[this.currentToken.startPos + 2]) || ('f' != this.strSourceCode[this.currentToken.startPos + 3]))) || (((num != 0) || !checkCondition) || !this.PPTestCond()))
                                    {
                                        break;
                                    }
                                    return;
                                }
                                if ((num != 0) || !checkCondition)
                                {
                                    break;
                                }
                                return;
                        }
                        break;

                    case '\r':
                        if (this.GetChar(this.currentPos) == '\n')
                        {
                            this.currentPos++;
                        }
                        this.currentLine++;
                        this.startLinePos = this.currentPos;
                        break;

                    case '\0':
                        if (this.currentPos >= this.endPos)
                        {
                            this.currentPos--;
                            this.currentToken.endPos = this.currentPos;
                            this.currentToken.endLineNumber = this.currentLine;
                            this.currentToken.endLinePos = this.startLinePos;
                            this.HandleError(JSError.NoCcEnd);
                            throw new ScannerException(JSError.ErrEOF);
                        }
                        break;

                    case '\n':
                        this.currentLine++;
                        this.startLinePos = this.currentPos;
                        break;
                }
            }
        }

        private bool PPTestCond()
        {
            this.SkipBlanks();
            if ('(' != this.GetChar(this.currentPos))
            {
                this.currentToken.startPos = this.currentPos - 1;
                this.currentToken.lineNumber = this.currentLine;
                this.currentToken.startLinePos = this.startLinePos;
                this.HandleError(JSError.NoLeftParen);
            }
            else
            {
                this.currentPos++;
            }
            object obj2 = this.PPScanExpr();
            if (')' != this.GetChar(this.currentPos++))
            {
                this.currentToken.startPos = this.currentPos - 1;
                this.currentToken.lineNumber = this.currentLine;
                this.currentToken.startLinePos = this.startLinePos;
                this.HandleError(JSError.NoRightParen);
                this.currentPos--;
            }
            return Microsoft.JScript.Convert.ToBoolean(obj2);
        }

        private void ScanIdentifier()
        {
            while (true)
            {
                char c = this.GetChar(this.currentPos);
                if (!this.IsIdentifierPartChar(c))
                {
                    break;
                }
                this.currentPos++;
            }
            if (this.idLastPosOnBuilder > 0)
            {
                this.identifier.Append(this.strSourceCode.Substring(this.idLastPosOnBuilder, this.currentPos - this.idLastPosOnBuilder));
                this.idLastPosOnBuilder = 0;
            }
        }

        private JSToken ScanKeyword(JSKeyword keyword)
        {
            char ch;
            while (true)
            {
                ch = this.GetChar(this.currentPos);
                if (('a' > ch) || (ch > 'z'))
                {
                    break;
                }
                this.currentPos++;
            }
            if (this.IsIdentifierPartChar(ch))
            {
                this.ScanIdentifier();
                return JSToken.Identifier;
            }
            return keyword.GetKeyword(this.currentToken, this.currentPos - this.currentToken.startPos);
        }

        private JSToken ScanNumber(char leadChar)
        {
            char ch;
            bool flag = '.' == leadChar;
            JSToken numericLiteral = flag ? JSToken.NumericLiteral : JSToken.IntegerLiteral;
            bool flag2 = false;
            if ('0' == leadChar)
            {
                ch = this.GetChar(this.currentPos);
                if (('x' == ch) || ('X' == ch))
                {
                    if (!IsHexDigit(this.GetChar(this.currentPos + 1)))
                    {
                        this.HandleError(JSError.BadHexDigit);
                    }
                    while (IsHexDigit(this.GetChar(++this.currentPos)))
                    {
                    }
                    return numericLiteral;
                }
            }
            while (true)
            {
                ch = this.GetChar(this.currentPos);
                if (!IsDigit(ch))
                {
                    if ('.' == ch)
                    {
                        if (flag)
                        {
                            break;
                        }
                        flag = true;
                        numericLiteral = JSToken.NumericLiteral;
                    }
                    else if (('e' == ch) || ('E' == ch))
                    {
                        if (flag2)
                        {
                            break;
                        }
                        flag2 = true;
                        numericLiteral = JSToken.NumericLiteral;
                    }
                    else
                    {
                        if (('+' != ch) && ('-' != ch))
                        {
                            break;
                        }
                        char ch2 = this.GetChar(this.currentPos - 1);
                        if (('e' != ch2) && ('E' != ch2))
                        {
                            break;
                        }
                    }
                }
                this.currentPos++;
            }
            ch = this.GetChar(this.currentPos - 1);
            if (('+' == ch) || ('-' == ch))
            {
                this.currentPos--;
                ch = this.GetChar(this.currentPos - 1);
            }
            if (('e' == ch) || ('E' == ch))
            {
                this.currentPos--;
            }
            return numericLiteral;
        }

        internal string ScanRegExp()
        {
            char ch;
            int currentPos = this.currentPos;
            bool flag = false;
            while (!this.IsEndLineOrEOF(ch = this.GetChar(this.currentPos++), 0))
            {
                if (flag)
                {
                    flag = false;
                }
                else
                {
                    switch (ch)
                    {
                        case '/':
                            if (currentPos == this.currentPos)
                            {
                                return null;
                            }
                            this.currentToken.endPos = this.currentPos;
                            this.currentToken.endLinePos = this.startLinePos;
                            this.currentToken.endLineNumber = this.currentLine;
                            return this.strSourceCode.Substring(this.currentToken.startPos + 1, (this.currentToken.endPos - this.currentToken.startPos) - 2);

                        case '\\':
                            flag = true;
                            break;
                    }
                }
            }
            this.currentPos = currentPos;
            return null;
        }

        internal string ScanRegExpFlags()
        {
            int currentPos = this.currentPos;
            while (IsAsciiLetter(this.GetChar(this.currentPos)))
            {
                this.currentPos++;
            }
            if (currentPos != this.currentPos)
            {
                this.currentToken.endPos = this.currentPos;
                this.currentToken.endLineNumber = this.currentLine;
                this.currentToken.endLinePos = this.startLinePos;
                return this.strSourceCode.Substring(currentPos, this.currentToken.endPos - currentPos);
            }
            return null;
        }

        private void ScanString(char cStringTerminator)
        {
            char ch;
            bool flag;
            int num2;
            int currentPos = this.currentPos;
            this.escapedString = null;
            StringBuilder builder = null;
        Label_0010:
            ch = this.GetChar(this.currentPos++);
            if (ch == '\\')
            {
                if (builder == null)
                {
                    builder = new StringBuilder(0x80);
                }
                if (((this.currentPos - currentPos) - 1) > 0)
                {
                    builder.Append(this.strSourceCode, currentPos, (this.currentPos - currentPos) - 1);
                }
                flag = false;
                num2 = 0;
                ch = this.GetChar(this.currentPos++);
                switch (ch)
                {
                    case '\'':
                        builder.Append('\'');
                        ch = '\0';
                        goto Label_067A;

                    case '0':
                    case '1':
                    case '2':
                    case '3':
                        flag = true;
                        num2 = (ch - 0x30) << 6;
                        goto Label_059C;

                    case '4':
                    case '5':
                    case '6':
                    case '7':
                        goto Label_059C;

                    case '"':
                        builder.Append('"');
                        ch = '\0';
                        goto Label_067A;

                    case '\n':
                    case '\u2028':
                    case '\u2029':
                        goto Label_01E3;

                    case '\r':
                        if ('\n' == this.GetChar(this.currentPos))
                        {
                            this.currentPos++;
                        }
                        goto Label_01E3;

                    case '\\':
                        builder.Append('\\');
                        goto Label_067A;

                    case 'b':
                        builder.Append('\b');
                        goto Label_067A;

                    case 'n':
                        builder.Append('\n');
                        goto Label_067A;

                    case 'r':
                        builder.Append('\r');
                        goto Label_067A;

                    case 't':
                        builder.Append('\t');
                        goto Label_067A;

                    case 'u':
                        ch = this.GetChar(this.currentPos++);
                        if ((ch - '0') > 9)
                        {
                            if ((ch - 'A') <= 5)
                            {
                                num2 = ((ch + 10) - 0x41) << 12;
                            }
                            else if ((ch - 'a') <= 5)
                            {
                                num2 = ((ch + 10) - 0x61) << 12;
                            }
                            else
                            {
                                this.HandleError(JSError.BadHexDigit);
                                if (ch != cStringTerminator)
                                {
                                    this.currentPos--;
                                }
                                goto Label_067A;
                            }
                        }
                        else
                        {
                            num2 = (ch - 0x30) << 12;
                        }
                        ch = this.GetChar(this.currentPos++);
                        if ((ch - '0') <= 9)
                        {
                            num2 |= (ch - 0x30) << 8;
                        }
                        else if ((ch - 'A') <= 5)
                        {
                            num2 |= ((ch + 10) - 0x41) << 8;
                        }
                        else if ((ch - 'a') <= 5)
                        {
                            num2 |= ((ch + 10) - 0x61) << 8;
                        }
                        else
                        {
                            this.HandleError(JSError.BadHexDigit);
                            if (ch != cStringTerminator)
                            {
                                this.currentPos--;
                            }
                            goto Label_067A;
                        }
                        ch = this.GetChar(this.currentPos++);
                        if ((ch - '0') <= 9)
                        {
                            num2 |= (ch - 0x30) << 4;
                        }
                        else if ((ch - 'A') <= 5)
                        {
                            num2 |= ((ch + 10) - 0x41) << 4;
                        }
                        else if ((ch - 'a') <= 5)
                        {
                            num2 |= ((ch + 10) - 0x61) << 4;
                        }
                        else
                        {
                            this.HandleError(JSError.BadHexDigit);
                            if (ch != cStringTerminator)
                            {
                                this.currentPos--;
                            }
                            goto Label_067A;
                        }
                        ch = this.GetChar(this.currentPos++);
                        if ((ch - '0') <= 9)
                        {
                            num2 |= ch - '0';
                        }
                        else if ((ch - 'A') <= 5)
                        {
                            num2 |= (ch + '\n') - 0x41;
                        }
                        else if ((ch - 'a') <= 5)
                        {
                            num2 |= (ch + '\n') - 0x61;
                        }
                        else
                        {
                            this.HandleError(JSError.BadHexDigit);
                            if (ch != cStringTerminator)
                            {
                                this.currentPos--;
                            }
                            goto Label_067A;
                        }
                        builder.Append((char) num2);
                        goto Label_067A;

                    case 'v':
                        builder.Append('\v');
                        goto Label_067A;

                    case 'x':
                        ch = this.GetChar(this.currentPos++);
                        if ((ch - '0') > 9)
                        {
                            if ((ch - 'A') <= 5)
                            {
                                num2 = ((ch + 10) - 0x41) << 4;
                            }
                            else if ((ch - 'a') <= 5)
                            {
                                num2 = ((ch + 10) - 0x61) << 4;
                            }
                            else
                            {
                                this.HandleError(JSError.BadHexDigit);
                                if (ch != cStringTerminator)
                                {
                                    this.currentPos--;
                                }
                                goto Label_067A;
                            }
                        }
                        else
                        {
                            num2 = (ch - 0x30) << 4;
                        }
                        ch = this.GetChar(this.currentPos++);
                        if ((ch - '0') <= 9)
                        {
                            num2 |= ch - '0';
                        }
                        else if ((ch - 'A') <= 5)
                        {
                            num2 |= (ch + '\n') - 0x41;
                        }
                        else if ((ch - 'a') <= 5)
                        {
                            num2 |= (ch + '\n') - 0x61;
                        }
                        else
                        {
                            this.HandleError(JSError.BadHexDigit);
                            if (ch != cStringTerminator)
                            {
                                this.currentPos--;
                            }
                            goto Label_067A;
                        }
                        builder.Append((char) num2);
                        goto Label_067A;

                    case 'f':
                        builder.Append('\f');
                        goto Label_067A;
                }
                builder.Append(ch);
                goto Label_067A;
            }
            if (this.IsLineTerminator(ch, 0))
            {
                this.HandleError(JSError.UnterminatedString);
                this.currentPos--;
            }
            else
            {
                if (ch != '\0')
                {
                    goto Label_0681;
                }
                this.currentPos--;
                this.HandleError(JSError.UnterminatedString);
            }
            goto Label_0688;
        Label_01E3:
            this.currentLine++;
            this.startLinePos = this.currentPos;
            goto Label_067A;
        Label_059C:
            if (!flag)
            {
                num2 = (ch - 0x30) << 3;
            }
            ch = this.GetChar(this.currentPos++);
            if ((ch - '0') <= 7)
            {
                if (flag)
                {
                    num2 |= (ch - 0x30) << 3;
                    ch = this.GetChar(this.currentPos++);
                    if ((ch - '0') <= 7)
                    {
                        num2 |= ch - '0';
                        builder.Append((char) num2);
                    }
                    else
                    {
                        builder.Append((char) (num2 >> 3));
                        if (ch != cStringTerminator)
                        {
                            this.currentPos--;
                        }
                    }
                }
                else
                {
                    num2 |= ch - '0';
                    builder.Append((char) num2);
                }
            }
            else
            {
                if (flag)
                {
                    builder.Append((char) (num2 >> 6));
                }
                else
                {
                    builder.Append((char) (num2 >> 3));
                }
                if (ch != cStringTerminator)
                {
                    this.currentPos--;
                }
            }
        Label_067A:
            currentPos = this.currentPos;
        Label_0681:
            if (ch != cStringTerminator)
            {
                goto Label_0010;
            }
        Label_0688:
            if (builder != null)
            {
                if (((this.currentPos - currentPos) - 1) > 0)
                {
                    builder.Append(this.strSourceCode, currentPos, (this.currentPos - currentPos) - 1);
                }
                this.escapedString = builder.ToString();
            }
            else if (this.currentPos <= (this.currentToken.startPos + 2))
            {
                this.escapedString = "";
            }
            else
            {
                this.escapedString = this.currentToken.source_string.Substring(this.currentToken.startPos + 1, (this.currentPos - this.currentToken.startPos) - 2);
            }
        }

        public void SetAuthoringMode(bool mode)
        {
            this.IsAuthoring = mode;
        }

        private void SetPreProcessorOn()
        {
            this.preProcessorOn = true;
            this.ppTable = new SimpleHashtable(0x10);
            this.ppTable["_debug"] = this.globals.engine.GenerateDebugInfo;
            this.ppTable["_fast"] = ((IActivationObject) this.globals.ScopeStack.Peek()).GetGlobalScope().fast;
            this.ppTable["_jscript"] = true;
            this.ppTable["_jscript_build"] = GlobalObject.ScriptEngineBuildVersion();
            this.ppTable["_jscript_version"] = Microsoft.JScript.Convert.ToNumber(GlobalObject.ScriptEngineMajorVersion() + "." + GlobalObject.ScriptEngineMinorVersion());
            this.ppTable["_microsoft"] = true;
            if ((this.globals.engine.PEMachineArchitecture == ImageFileMachine.I386) && (this.globals.engine.PEKindFlags == PortableExecutableKinds.ILOnly))
            {
                this.ppTable["_win32"] = Environment.OSVersion.Platform.ToString().StartsWith("Win32", StringComparison.Ordinal);
                this.ppTable["_x86"] = true;
            }
            Hashtable option = (Hashtable) this.globals.engine.GetOption("defines");
            if (option != null)
            {
                foreach (DictionaryEntry entry in option)
                {
                    this.ppTable[entry.Key] = entry.Value;
                }
            }
        }

        public void SetSource(Context sourceContext)
        {
            this.strSourceCode = sourceContext.source_string;
            this.startPos = sourceContext.startPos;
            this.startLinePos = sourceContext.startLinePos;
            this.endPos = ((0 < sourceContext.endPos) && (sourceContext.endPos < this.strSourceCode.Length)) ? sourceContext.endPos : this.strSourceCode.Length;
            this.currentToken = sourceContext;
            this.escapedString = null;
            this.identifier = new StringBuilder(0x80);
            this.idLastPosOnBuilder = 0;
            this.currentPos = this.startPos;
            this.currentLine = (sourceContext.lineNumber > 0) ? sourceContext.lineNumber : 1;
            this.gotEndOfLine = false;
            this.scanForDebugger = ((sourceContext.document != null) && (sourceContext.document.engine != null)) && VsaEngine.executeForJSEE;
        }

        private void SkipBlanks()
        {
            for (char ch = this.GetChar(this.currentPos); IsBlankSpace(ch); ch = this.GetChar(++this.currentPos))
            {
            }
        }

        public int SkipMultiLineComment()
        {
            char ch;
        Label_0000:
            ch = this.GetChar(this.currentPos);
            while ('*' == ch)
            {
                ch = this.GetChar(++this.currentPos);
                if ('/' == ch)
                {
                    this.currentPos++;
                    return this.currentPos;
                }
                if (ch == '\0')
                {
                    break;
                }
                if (this.IsLineTerminator(ch, 1))
                {
                    ch = this.GetChar(++this.currentPos);
                    this.currentLine++;
                    this.startLinePos = this.currentPos + 1;
                }
            }
            if ((ch != '\0') || (this.currentPos < this.endPos))
            {
                if (this.IsLineTerminator(ch, 1))
                {
                    this.currentLine++;
                    this.startLinePos = this.currentPos + 1;
                }
                this.currentPos++;
                goto Label_0000;
            }
            if (!this.IsAuthoring)
            {
                this.currentToken.endPos = --this.currentPos;
                this.currentToken.endLinePos = this.startLinePos;
                this.currentToken.endLineNumber = this.currentLine;
                throw new ScannerException(JSError.NoCommentEnd);
            }
            return this.currentPos;
        }

        private void SkipSingleLineComment()
        {
            while (!this.IsEndLineOrEOF(this.GetChar(this.currentPos++), 0))
            {
            }
            if (this.IsAuthoring)
            {
                this.currentToken.endPos = this.currentPos;
                this.currentToken.endLineNumber = this.currentLine;
                this.currentToken.endLinePos = this.startLinePos;
                this.gotEndOfLine = true;
            }
            this.currentLine++;
            this.startLinePos = this.currentPos;
        }
    }
}

