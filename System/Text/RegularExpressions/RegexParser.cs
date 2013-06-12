namespace System.Text.RegularExpressions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;

    internal sealed class RegexParser
    {
        internal RegexNode _alternation;
        internal int _autocap;
        internal int _capcount;
        internal List<string> _capnamelist;
        internal Hashtable _capnames;
        internal int[] _capnumlist;
        internal Hashtable _caps;
        internal int _capsize;
        internal int _captop;
        internal static readonly byte[] _category = new byte[] { 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 0, 2, 2, 0, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            2, 0, 0, 3, 4, 0, 0, 0, 4, 4, 5, 5, 0, 0, 4, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 4, 0, 4, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 4, 0, 0, 0
         };
        internal RegexNode _concatenation;
        internal CultureInfo _culture;
        internal int _currentPos;
        internal RegexNode _group;
        internal bool _ignoreNextParen;
        internal RegexOptions _options;
        internal List<RegexOptions> _optionsStack;
        internal string _pattern;
        internal RegexNode _stack;
        internal RegexNode _unit;
        internal const byte E = 1;
        internal const int MaxValueDiv10 = 0xccccccc;
        internal const int MaxValueMod10 = 7;
        internal const byte Q = 5;
        internal const byte S = 4;
        internal const byte X = 2;
        internal const byte Z = 3;

        private RegexParser(CultureInfo culture)
        {
            this._culture = culture;
            this._optionsStack = new List<RegexOptions>();
            this._caps = new Hashtable();
        }

        internal void AddAlternate()
        {
            if ((this._group.Type() == 0x22) || (this._group.Type() == 0x21))
            {
                this._group.AddChild(this._concatenation.ReverseLeft());
            }
            else
            {
                this._alternation.AddChild(this._concatenation.ReverseLeft());
            }
            this._concatenation = new RegexNode(0x19, this._options);
        }

        internal void AddConcatenate()
        {
            this._concatenation.AddChild(this._unit);
            this._unit = null;
        }

        internal void AddConcatenate(bool lazy, int min, int max)
        {
            this._concatenation.AddChild(this._unit.MakeQuantifier(lazy, min, max));
            this._unit = null;
        }

        internal void AddConcatenate(int pos, int cch, bool isReplacement)
        {
            if (cch != 0)
            {
                RegexNode node;
                if (cch > 1)
                {
                    string str = this._pattern.Substring(pos, cch);
                    if (this.UseOptionI() && !isReplacement)
                    {
                        StringBuilder builder = new StringBuilder(str.Length);
                        for (int i = 0; i < str.Length; i++)
                        {
                            builder.Append(char.ToLower(str[i], this._culture));
                        }
                        str = builder.ToString();
                    }
                    node = new RegexNode(12, this._options, str);
                }
                else
                {
                    char c = this._pattern[pos];
                    if (this.UseOptionI() && !isReplacement)
                    {
                        c = char.ToLower(c, this._culture);
                    }
                    node = new RegexNode(9, this._options, c);
                }
                this._concatenation.AddChild(node);
            }
        }

        internal void AddGroup()
        {
            if ((this._group.Type() == 0x22) || (this._group.Type() == 0x21))
            {
                this._group.AddChild(this._concatenation.ReverseLeft());
                if (((this._group.Type() == 0x21) && (this._group.ChildCount() > 2)) || (this._group.ChildCount() > 3))
                {
                    throw this.MakeException(SR.GetString("TooManyAlternates"));
                }
            }
            else
            {
                this._alternation.AddChild(this._concatenation.ReverseLeft());
                this._group.AddChild(this._alternation);
            }
            this._unit = this._group;
        }

        internal void AddUnitNode(RegexNode node)
        {
            this._unit = node;
        }

        internal void AddUnitNotone(char ch)
        {
            if (this.UseOptionI())
            {
                ch = char.ToLower(ch, this._culture);
            }
            this._unit = new RegexNode(10, this._options, ch);
        }

        internal void AddUnitOne(char ch)
        {
            if (this.UseOptionI())
            {
                ch = char.ToLower(ch, this._culture);
            }
            this._unit = new RegexNode(9, this._options, ch);
        }

        internal void AddUnitSet(string cc)
        {
            this._unit = new RegexNode(11, this._options, cc);
        }

        internal void AddUnitType(int type)
        {
            this._unit = new RegexNode(type, this._options);
        }

        internal void AssignNameSlots()
        {
            if (this._capnames != null)
            {
                for (int i = 0; i < this._capnamelist.Count; i++)
                {
                    while (this.IsCaptureSlot(this._autocap))
                    {
                        this._autocap++;
                    }
                    string str = this._capnamelist[i];
                    int pos = (int) this._capnames[str];
                    this._capnames[str] = this._autocap;
                    this.NoteCaptureSlot(this._autocap, pos);
                    this._autocap++;
                }
            }
            if (this._capcount < this._captop)
            {
                this._capnumlist = new int[this._capcount];
                int num3 = 0;
                IDictionaryEnumerator enumerator = this._caps.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    this._capnumlist[num3++] = (int) enumerator.Key;
                }
                Array.Sort<int>(this._capnumlist, Comparer<int>.Default);
            }
            if ((this._capnames != null) || (this._capnumlist != null))
            {
                List<string> list;
                int num4;
                int num5 = 0;
                if (this._capnames == null)
                {
                    list = null;
                    this._capnames = new Hashtable();
                    this._capnamelist = new List<string>();
                    num4 = -1;
                }
                else
                {
                    list = this._capnamelist;
                    this._capnamelist = new List<string>();
                    num4 = (int) this._capnames[list[0]];
                }
                for (int j = 0; j < this._capcount; j++)
                {
                    int num7 = (this._capnumlist == null) ? j : this._capnumlist[j];
                    if (num4 == num7)
                    {
                        this._capnamelist.Add(list[num5++]);
                        num4 = (num5 == list.Count) ? -1 : ((int) this._capnames[list[num5]]);
                    }
                    else
                    {
                        string item = Convert.ToString(num7, this._culture);
                        this._capnamelist.Add(item);
                        this._capnames[item] = num7;
                    }
                }
            }
        }

        internal int CaptureSlotFromName(string capname)
        {
            return (int) this._capnames[capname];
        }

        internal char CharAt(int i)
        {
            return this._pattern[i];
        }

        internal int CharsRight()
        {
            return (this._pattern.Length - this._currentPos);
        }

        internal void CountCaptures()
        {
            this.NoteCaptureSlot(0, 0);
            this._autocap = 1;
            while (this.CharsRight() > 0)
            {
                int pos = this.Textpos();
                switch (this.MoveRightGetChar())
                {
                    case '(':
                        if (((this.CharsRight() < 2) || (this.RightChar(1) != '#')) || (this.RightChar() != '?'))
                        {
                            break;
                        }
                        this.MoveLeft();
                        this.ScanBlank();
                        goto Label_01C2;

                    case ')':
                    {
                        if (!this.EmptyOptionsStack())
                        {
                            this.PopOptions();
                        }
                        continue;
                    }
                    case '#':
                    {
                        if (this.UseOptionX())
                        {
                            this.MoveLeft();
                            this.ScanBlank();
                        }
                        continue;
                    }
                    case '[':
                    {
                        this.ScanCharClass(false, true);
                        continue;
                    }
                    case '\\':
                    {
                        if (this.CharsRight() > 0)
                        {
                            this.MoveRight();
                        }
                        continue;
                    }
                    default:
                    {
                        continue;
                    }
                }
                this.PushOptions();
                if ((this.CharsRight() > 0) && (this.RightChar() == '?'))
                {
                    this.MoveRight();
                    if ((this.CharsRight() > 1) && ((this.RightChar() == '<') || (this.RightChar() == '\'')))
                    {
                        this.MoveRight();
                        char ch = this.RightChar();
                        if ((ch != '0') && RegexCharClass.IsWordChar(ch))
                        {
                            if ((ch >= '1') && (ch <= '9'))
                            {
                                this.NoteCaptureSlot(this.ScanDecimal(), pos);
                            }
                            else
                            {
                                this.NoteCaptureName(this.ScanCapname(), pos);
                            }
                        }
                        goto Label_01C2;
                    }
                    this.ScanOptions();
                    if (this.CharsRight() <= 0)
                    {
                        goto Label_01C2;
                    }
                    if (this.RightChar() == ')')
                    {
                        this.MoveRight();
                        this.PopKeepOptions();
                        goto Label_01C2;
                    }
                    if (this.RightChar() != '(')
                    {
                        goto Label_01C2;
                    }
                    this._ignoreNextParen = true;
                    continue;
                }
                if (!this.UseOptionN() && !this._ignoreNextParen)
                {
                    this.NoteCaptureSlot(this._autocap++, pos);
                }
            Label_01C2:
                this._ignoreNextParen = false;
            }
            this.AssignNameSlots();
        }

        internal bool EmptyOptionsStack()
        {
            return (this._optionsStack.Count == 0);
        }

        internal bool EmptyStack()
        {
            return (this._stack == null);
        }

        internal static string Escape(string input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (!IsMetachar(input[i]))
                {
                    continue;
                }
                StringBuilder builder = new StringBuilder();
                char ch = input[i];
                builder.Append(input, 0, i);
                do
                {
                    builder.Append('\\');
                    switch (ch)
                    {
                        case '\t':
                            ch = 't';
                            break;

                        case '\n':
                            ch = 'n';
                            break;

                        case '\f':
                            ch = 'f';
                            break;

                        case '\r':
                            ch = 'r';
                            break;
                    }
                    builder.Append(ch);
                    i++;
                    int startIndex = i;
                    while (i < input.Length)
                    {
                        ch = input[i];
                        if (IsMetachar(ch))
                        {
                            break;
                        }
                        i++;
                    }
                    builder.Append(input, startIndex, i - startIndex);
                }
                while (i < input.Length);
                return builder.ToString();
            }
            return input;
        }

        internal static int HexDigit(char ch)
        {
            int num = ch - '0';
            if (num <= 9)
            {
                return num;
            }
            num = ch - 'a';
            if (num <= 5)
            {
                return (num + 10);
            }
            num = ch - 'A';
            if (num <= 5)
            {
                return (num + 10);
            }
            return -1;
        }

        internal bool IsCaptureName(string capname)
        {
            if (this._capnames == null)
            {
                return false;
            }
            return this._capnames.ContainsKey(capname);
        }

        internal bool IsCaptureSlot(int i)
        {
            if (this._caps != null)
            {
                return this._caps.ContainsKey(i);
            }
            return ((i >= 0) && (i < this._capsize));
        }

        internal static bool IsMetachar(char ch)
        {
            return ((ch <= '|') && (_category[ch] >= 1));
        }

        internal bool IsOnlyTopOption(RegexOptions option)
        {
            if (((option != RegexOptions.RightToLeft) && (option != RegexOptions.Compiled)) && (option != RegexOptions.CultureInvariant))
            {
                return (option == RegexOptions.ECMAScript);
            }
            return true;
        }

        internal static bool IsQuantifier(char ch)
        {
            return ((ch <= '{') && (_category[ch] >= 5));
        }

        internal static bool IsSpace(char ch)
        {
            return ((ch <= ' ') && (_category[ch] == 2));
        }

        internal static bool IsSpecial(char ch)
        {
            return ((ch <= '|') && (_category[ch] >= 4));
        }

        internal static bool IsStopperX(char ch)
        {
            return ((ch <= '|') && (_category[ch] >= 2));
        }

        internal bool IsTrueQuantifier()
        {
            int num = this.CharsRight();
            if (num == 0)
            {
                return false;
            }
            int i = this.Textpos();
            char index = this.CharAt(i);
            if (index == '{')
            {
                int num3 = i;
                while (((--num > 0) && ((index = this.CharAt(++num3)) >= '0')) && (index <= '9'))
                {
                }
                if ((num == 0) || ((num3 - i) == 1))
                {
                    return false;
                }
                if (index == '}')
                {
                    return true;
                }
                if (index != ',')
                {
                    return false;
                }
                while (((--num > 0) && ((index = this.CharAt(++num3)) >= '0')) && (index <= '9'))
                {
                }
                return ((num > 0) && (index == '}'));
            }
            return ((index <= '{') && (_category[index] >= 5));
        }

        internal ArgumentException MakeException(string message)
        {
            return new ArgumentException(SR.GetString("MakeException", new object[] { this._pattern, message }));
        }

        internal void MoveLeft()
        {
            this._currentPos--;
        }

        internal void MoveRight()
        {
            this.MoveRight(1);
        }

        internal void MoveRight(int i)
        {
            this._currentPos += i;
        }

        internal char MoveRightGetChar()
        {
            return this._pattern[this._currentPos++];
        }

        internal void NoteCaptureName(string name, int pos)
        {
            if (this._capnames == null)
            {
                this._capnames = new Hashtable();
                this._capnamelist = new List<string>();
            }
            if (!this._capnames.ContainsKey(name))
            {
                this._capnames.Add(name, pos);
                this._capnamelist.Add(name);
            }
        }

        internal void NoteCaptures(Hashtable caps, int capsize, Hashtable capnames)
        {
            this._caps = caps;
            this._capsize = capsize;
            this._capnames = capnames;
        }

        internal void NoteCaptureSlot(int i, int pos)
        {
            if (!this._caps.ContainsKey(i))
            {
                this._caps.Add(i, pos);
                this._capcount++;
                if (this._captop <= i)
                {
                    if (i == 0x7fffffff)
                    {
                        this._captop = i;
                    }
                    else
                    {
                        this._captop = i + 1;
                    }
                }
            }
        }

        internal static RegexOptions OptionFromCode(char ch)
        {
            if ((ch >= 'A') && (ch <= 'Z'))
            {
                ch = (char) (ch + ' ');
            }
            switch (ch)
            {
                case 'c':
                    return RegexOptions.Compiled;

                case 'e':
                    return RegexOptions.ECMAScript;

                case 'i':
                    return RegexOptions.IgnoreCase;

                case 'm':
                    return RegexOptions.Multiline;

                case 'n':
                    return RegexOptions.ExplicitCapture;

                case 'r':
                    return RegexOptions.RightToLeft;

                case 's':
                    return RegexOptions.Singleline;

                case 'x':
                    return RegexOptions.IgnorePatternWhitespace;
            }
            return RegexOptions.None;
        }

        internal static RegexTree Parse(string re, RegexOptions op)
        {
            string[] strArray;
            RegexParser parser = new RegexParser(((op & RegexOptions.CultureInvariant) != RegexOptions.None) ? CultureInfo.InvariantCulture : CultureInfo.CurrentCulture) {
                _options = op
            };
            parser.SetPattern(re);
            parser.CountCaptures();
            parser.Reset(op);
            RegexNode root = parser.ScanRegex();
            if (parser._capnamelist == null)
            {
                strArray = null;
            }
            else
            {
                strArray = parser._capnamelist.ToArray();
            }
            return new RegexTree(root, parser._caps, parser._capnumlist, parser._captop, parser._capnames, strArray, op);
        }

        internal string ParseProperty()
        {
            if (this.CharsRight() < 3)
            {
                throw this.MakeException(SR.GetString("IncompleteSlashP"));
            }
            if (this.MoveRightGetChar() != '{')
            {
                throw this.MakeException(SR.GetString("MalformedSlashP"));
            }
            int startIndex = this.Textpos();
            while (this.CharsRight() > 0)
            {
                char ch = this.MoveRightGetChar();
                if (!RegexCharClass.IsWordChar(ch) && (ch != '-'))
                {
                    this.MoveLeft();
                    break;
                }
            }
            string str = this._pattern.Substring(startIndex, this.Textpos() - startIndex);
            if ((this.CharsRight() == 0) || (this.MoveRightGetChar() != '}'))
            {
                throw this.MakeException(SR.GetString("IncompleteSlashP"));
            }
            return str;
        }

        internal static RegexReplacement ParseReplacement(string rep, Hashtable caps, int capsize, Hashtable capnames, RegexOptions op)
        {
            RegexParser parser = new RegexParser(((op & RegexOptions.CultureInvariant) != RegexOptions.None) ? CultureInfo.InvariantCulture : CultureInfo.CurrentCulture) {
                _options = op
            };
            parser.NoteCaptures(caps, capsize, capnames);
            parser.SetPattern(rep);
            return new RegexReplacement(rep, parser.ScanReplacement(), caps);
        }

        internal void PopGroup()
        {
            this._concatenation = this._stack;
            this._alternation = this._concatenation._next;
            this._group = this._alternation._next;
            this._stack = this._group._next;
            if ((this._group.Type() == 0x22) && (this._group.ChildCount() == 0))
            {
                if (this._unit == null)
                {
                    throw this.MakeException(SR.GetString("IllegalCondition"));
                }
                this._group.AddChild(this._unit);
                this._unit = null;
            }
        }

        internal void PopKeepOptions()
        {
            this._optionsStack.RemoveAt(this._optionsStack.Count - 1);
        }

        internal void PopOptions()
        {
            this._options = this._optionsStack[this._optionsStack.Count - 1];
            this._optionsStack.RemoveAt(this._optionsStack.Count - 1);
        }

        internal void PushGroup()
        {
            this._group._next = this._stack;
            this._alternation._next = this._group;
            this._concatenation._next = this._alternation;
            this._stack = this._concatenation;
        }

        internal void PushOptions()
        {
            this._optionsStack.Add(this._options);
        }

        internal void Reset(RegexOptions topopts)
        {
            this._currentPos = 0;
            this._autocap = 1;
            this._ignoreNextParen = false;
            if (this._optionsStack.Count > 0)
            {
                this._optionsStack.RemoveRange(0, this._optionsStack.Count - 1);
            }
            this._options = topopts;
            this._stack = null;
        }

        internal char RightChar()
        {
            return this._pattern[this._currentPos];
        }

        internal char RightChar(int i)
        {
            return this._pattern[this._currentPos + i];
        }

        internal RegexNode ScanBackslash()
        {
            char ch;
            if (this.CharsRight() == 0)
            {
                throw this.MakeException(SR.GetString("IllegalEndEscape"));
            }
            switch ((ch = this.RightChar()))
            {
                case 'S':
                    this.MoveRight();
                    if (this.UseOptionE())
                    {
                        return new RegexNode(11, this._options, "\x0001\x0004\0\t\x000e !");
                    }
                    return new RegexNode(11, this._options, RegexCharClass.NotSpaceClass);

                case 'W':
                    this.MoveRight();
                    if (this.UseOptionE())
                    {
                        return new RegexNode(11, this._options, "\x0001\n\00:A[_`a{İı");
                    }
                    return new RegexNode(11, this._options, RegexCharClass.NotWordClass);

                case 'Z':
                case 'A':
                case 'B':
                case 'G':
                case 'b':
                case 'z':
                    this.MoveRight();
                    return new RegexNode(this.TypeFromCode(ch), this._options);

                case 'D':
                    this.MoveRight();
                    if (!this.UseOptionE())
                    {
                        return new RegexNode(11, this._options, RegexCharClass.NotDigitClass);
                    }
                    return new RegexNode(11, this._options, "\x0001\x0002\00:");

                case 'P':
                case 'p':
                {
                    this.MoveRight();
                    RegexCharClass class2 = new RegexCharClass();
                    class2.AddCategoryFromName(this.ParseProperty(), ch != 'p', this.UseOptionI(), this._pattern);
                    if (this.UseOptionI())
                    {
                        class2.AddLowercase(this._culture);
                    }
                    return new RegexNode(11, this._options, class2.ToStringClass());
                }
                case 'd':
                    this.MoveRight();
                    if (!this.UseOptionE())
                    {
                        return new RegexNode(11, this._options, RegexCharClass.DigitClass);
                    }
                    return new RegexNode(11, this._options, "\0\x0002\00:");

                case 's':
                    this.MoveRight();
                    if (this.UseOptionE())
                    {
                        return new RegexNode(11, this._options, "\0\x0004\0\t\x000e !");
                    }
                    return new RegexNode(11, this._options, RegexCharClass.SpaceClass);

                case 'w':
                    this.MoveRight();
                    if (this.UseOptionE())
                    {
                        return new RegexNode(11, this._options, "\0\n\00:A[_`a{İı");
                    }
                    return new RegexNode(11, this._options, RegexCharClass.WordClass);
            }
            return this.ScanBasicBackslash();
        }

        internal RegexNode ScanBasicBackslash()
        {
            if (this.CharsRight() == 0)
            {
                throw this.MakeException(SR.GetString("IllegalEndEscape"));
            }
            bool flag = false;
            char ch2 = '\0';
            int pos = this.Textpos();
            char ch = this.RightChar();
            if (ch == 'k')
            {
                if (this.CharsRight() >= 2)
                {
                    this.MoveRight();
                    ch = this.MoveRightGetChar();
                    switch (ch)
                    {
                        case '<':
                        case '\'':
                            flag = true;
                            ch2 = (ch == '\'') ? '\'' : '>';
                            break;
                    }
                }
                if (!flag || (this.CharsRight() <= 0))
                {
                    throw this.MakeException(SR.GetString("MalformedNameRef"));
                }
                ch = this.RightChar();
            }
            else if (((ch == '<') || (ch == '\'')) && (this.CharsRight() > 1))
            {
                flag = true;
                ch2 = (ch == '\'') ? '\'' : '>';
                this.MoveRight();
                ch = this.RightChar();
            }
            if ((flag && (ch >= '0')) && (ch <= '9'))
            {
                int i = this.ScanDecimal();
                if ((this.CharsRight() > 0) && (this.MoveRightGetChar() == ch2))
                {
                    if (!this.IsCaptureSlot(i))
                    {
                        throw this.MakeException(SR.GetString("UndefinedBackref", new object[] { i.ToString(CultureInfo.CurrentCulture) }));
                    }
                    return new RegexNode(13, this._options, i);
                }
            }
            else if ((flag || (ch < '1')) || (ch > '9'))
            {
                if (flag && RegexCharClass.IsWordChar(ch))
                {
                    string capname = this.ScanCapname();
                    if ((this.CharsRight() > 0) && (this.MoveRightGetChar() == ch2))
                    {
                        if (!this.IsCaptureName(capname))
                        {
                            throw this.MakeException(SR.GetString("UndefinedNameRef", new object[] { capname }));
                        }
                        return new RegexNode(13, this._options, this.CaptureSlotFromName(capname));
                    }
                }
            }
            else if (!this.UseOptionE())
            {
                int num6 = this.ScanDecimal();
                if (this.IsCaptureSlot(num6))
                {
                    return new RegexNode(13, this._options, num6);
                }
                if (num6 <= 9)
                {
                    throw this.MakeException(SR.GetString("UndefinedBackref", new object[] { num6.ToString(CultureInfo.CurrentCulture) }));
                }
            }
            else
            {
                int m = -1;
                int num4 = ch - '0';
                int num5 = this.Textpos() - 1;
                while (num4 <= this._captop)
                {
                    if (this.IsCaptureSlot(num4) && ((this._caps == null) || (((int) this._caps[num4]) < num5)))
                    {
                        m = num4;
                    }
                    this.MoveRight();
                    if (((this.CharsRight() == 0) || ((ch = this.RightChar()) < '0')) || (ch > '9'))
                    {
                        break;
                    }
                    num4 = (num4 * 10) + (ch - '0');
                }
                if (m >= 0)
                {
                    return new RegexNode(13, this._options, m);
                }
            }
            this.Textto(pos);
            ch = this.ScanCharEscape();
            if (this.UseOptionI())
            {
                ch = char.ToLower(ch, this._culture);
            }
            return new RegexNode(9, this._options, ch);
        }

        internal void ScanBlank()
        {
            if (this.UseOptionX())
            {
                while (true)
                {
                    while ((this.CharsRight() > 0) && IsSpace(this.RightChar()))
                    {
                        this.MoveRight();
                    }
                    if (this.CharsRight() == 0)
                    {
                        return;
                    }
                    if (this.RightChar() == '#')
                    {
                        while ((this.CharsRight() > 0) && (this.RightChar() != '\n'))
                        {
                            this.MoveRight();
                        }
                    }
                    else
                    {
                        if (((this.CharsRight() < 3) || (this.RightChar(2) != '#')) || ((this.RightChar(1) != '?') || (this.RightChar() != '(')))
                        {
                            return;
                        }
                        while ((this.CharsRight() > 0) && (this.RightChar() != ')'))
                        {
                            this.MoveRight();
                        }
                        if (this.CharsRight() == 0)
                        {
                            throw this.MakeException(SR.GetString("UnterminatedComment"));
                        }
                        this.MoveRight();
                    }
                }
            }
            while (((this.CharsRight() >= 3) && (this.RightChar(2) == '#')) && ((this.RightChar(1) == '?') && (this.RightChar() == '(')))
            {
                while ((this.CharsRight() > 0) && (this.RightChar() != ')'))
                {
                    this.MoveRight();
                }
                if (this.CharsRight() == 0)
                {
                    throw this.MakeException(SR.GetString("UnterminatedComment"));
                }
                this.MoveRight();
            }
        }

        internal string ScanCapname()
        {
            int startIndex = this.Textpos();
            while (this.CharsRight() > 0)
            {
                if (!RegexCharClass.IsWordChar(this.MoveRightGetChar()))
                {
                    this.MoveLeft();
                    break;
                }
            }
            return this._pattern.Substring(startIndex, this.Textpos() - startIndex);
        }

        internal RegexCharClass ScanCharClass(bool caseInsensitive)
        {
            return this.ScanCharClass(caseInsensitive, false);
        }

        internal RegexCharClass ScanCharClass(bool caseInsensitive, bool scanOnly)
        {
            char first = '\0';
            char c = '\0';
            bool flag = false;
            bool flag2 = true;
            bool flag3 = false;
            RegexCharClass class2 = scanOnly ? null : new RegexCharClass();
            if ((this.CharsRight() > 0) && (this.RightChar() == '^'))
            {
                this.MoveRight();
                if (!scanOnly)
                {
                    class2.Negate = true;
                }
            }
            while (this.CharsRight() > 0)
            {
                bool flag4 = false;
                first = this.MoveRightGetChar();
                if (first == ']')
                {
                    if (flag2)
                    {
                        goto Label_029F;
                    }
                    flag3 = true;
                    break;
                }
                if ((first == '\\') && (this.CharsRight() > 0))
                {
                    switch ((first = this.MoveRightGetChar()))
                    {
                        case '-':
                            if (!scanOnly)
                            {
                                class2.AddRange(first, first);
                            }
                            goto Label_03BE;

                        case 'D':
                        case 'd':
                            if (!scanOnly)
                            {
                                if (flag)
                                {
                                    throw this.MakeException(SR.GetString("BadClassInCharRange", new object[] { first.ToString() }));
                                }
                                class2.AddDigit(this.UseOptionE(), first == 'D', this._pattern);
                            }
                            goto Label_03BE;

                        case 'P':
                        case 'p':
                            if (!scanOnly)
                            {
                                if (flag)
                                {
                                    throw this.MakeException(SR.GetString("BadClassInCharRange", new object[] { first.ToString() }));
                                }
                                class2.AddCategoryFromName(this.ParseProperty(), first != 'p', caseInsensitive, this._pattern);
                            }
                            else
                            {
                                this.ParseProperty();
                            }
                            goto Label_03BE;

                        case 'S':
                        case 's':
                            if (!scanOnly)
                            {
                                if (flag)
                                {
                                    throw this.MakeException(SR.GetString("BadClassInCharRange", new object[] { first.ToString() }));
                                }
                                class2.AddSpace(this.UseOptionE(), first == 'S');
                            }
                            goto Label_03BE;

                        case 'W':
                        case 'w':
                            if (!scanOnly)
                            {
                                if (flag)
                                {
                                    throw this.MakeException(SR.GetString("BadClassInCharRange", new object[] { first.ToString() }));
                                }
                                class2.AddWord(this.UseOptionE(), first == 'W');
                            }
                            goto Label_03BE;
                    }
                    this.MoveLeft();
                    first = this.ScanCharEscape();
                    flag4 = true;
                }
                else if (((first == '[') && (this.CharsRight() > 0)) && ((this.RightChar() == ':') && !flag))
                {
                    int pos = this.Textpos();
                    this.MoveRight();
                    this.ScanCapname();
                    if (((this.CharsRight() < 2) || (this.MoveRightGetChar() != ':')) || (this.MoveRightGetChar() != ']'))
                    {
                        this.Textto(pos);
                    }
                }
            Label_029F:
                if (flag)
                {
                    flag = false;
                    if (!scanOnly)
                    {
                        if (((first == '[') && !flag4) && !flag2)
                        {
                            class2.AddChar(c);
                            class2.AddSubtraction(this.ScanCharClass(caseInsensitive, false));
                            if ((this.CharsRight() > 0) && (this.RightChar() != ']'))
                            {
                                throw this.MakeException(SR.GetString("SubtractionMustBeLast"));
                            }
                        }
                        else
                        {
                            if (c > first)
                            {
                                throw this.MakeException(SR.GetString("ReversedCharRange"));
                            }
                            class2.AddRange(c, first);
                        }
                    }
                }
                else if (((this.CharsRight() >= 2) && (this.RightChar() == '-')) && (this.RightChar(1) != ']'))
                {
                    c = first;
                    flag = true;
                    this.MoveRight();
                }
                else if ((((this.CharsRight() >= 1) && (first == '-')) && (!flag4 && (this.RightChar() == '['))) && !flag2)
                {
                    if (!scanOnly)
                    {
                        this.MoveRight(1);
                        class2.AddSubtraction(this.ScanCharClass(caseInsensitive, false));
                        if ((this.CharsRight() > 0) && (this.RightChar() != ']'))
                        {
                            throw this.MakeException(SR.GetString("SubtractionMustBeLast"));
                        }
                    }
                    else
                    {
                        this.MoveRight(1);
                        this.ScanCharClass(caseInsensitive, true);
                    }
                }
                else if (!scanOnly)
                {
                    class2.AddRange(first, first);
                }
            Label_03BE:
                flag2 = false;
            }
            if (!flag3)
            {
                throw this.MakeException(SR.GetString("UnterminatedBracket"));
            }
            if (!scanOnly && caseInsensitive)
            {
                class2.AddLowercase(this._culture);
            }
            return class2;
        }

        internal char ScanCharEscape()
        {
            char ch = this.MoveRightGetChar();
            if ((ch >= '0') && (ch <= '7'))
            {
                this.MoveLeft();
                return this.ScanOctal();
            }
            switch (ch)
            {
                case 'a':
                    return '\a';

                case 'b':
                    return '\b';

                case 'c':
                    return this.ScanControl();

                case 'e':
                    return '\x001b';

                case 'f':
                    return '\f';

                case 'n':
                    return '\n';

                case 'r':
                    return '\r';

                case 't':
                    return '\t';

                case 'u':
                    return this.ScanHex(4);

                case 'v':
                    return '\v';

                case 'x':
                    return this.ScanHex(2);
            }
            if (!this.UseOptionE() && RegexCharClass.IsWordChar(ch))
            {
                throw this.MakeException(SR.GetString("UnrecognizedEscape", new object[] { ch.ToString() }));
            }
            return ch;
        }

        internal char ScanControl()
        {
            if (this.CharsRight() <= 0)
            {
                throw this.MakeException(SR.GetString("MissingControl"));
            }
            char ch = this.MoveRightGetChar();
            if ((ch >= 'a') && (ch <= 'z'))
            {
                ch = (char) (ch - ' ');
            }
            ch = (char) (ch - '@');
            if (ch >= ' ')
            {
                throw this.MakeException(SR.GetString("UnrecognizedControl"));
            }
            return ch;
        }

        internal int ScanDecimal()
        {
            int num2;
            int num = 0;
            while ((this.CharsRight() > 0) && ((num2 = this.RightChar() - '0') <= 9))
            {
                this.MoveRight();
                if ((num > 0xccccccc) || ((num == 0xccccccc) && (num2 > 7)))
                {
                    throw this.MakeException(SR.GetString("CaptureGroupOutOfRange"));
                }
                num *= 10;
                num += num2;
            }
            return num;
        }

        internal RegexNode ScanDollar()
        {
            if (this.CharsRight() != 0)
            {
                bool flag;
                char ch = this.RightChar();
                int pos = this.Textpos();
                int num2 = pos;
                if ((ch == '{') && (this.CharsRight() > 1))
                {
                    flag = true;
                    this.MoveRight();
                    ch = this.RightChar();
                }
                else
                {
                    flag = false;
                }
                if ((ch >= '0') && (ch <= '9'))
                {
                    if (flag || !this.UseOptionE())
                    {
                        int i = this.ScanDecimal();
                        if ((!flag || ((this.CharsRight() > 0) && (this.MoveRightGetChar() == '}'))) && this.IsCaptureSlot(i))
                        {
                            return new RegexNode(13, this._options, i);
                        }
                    }
                    else
                    {
                        int m = -1;
                        int num4 = ch - '0';
                        this.MoveRight();
                        if (this.IsCaptureSlot(num4))
                        {
                            m = num4;
                            num2 = this.Textpos();
                        }
                        while (((this.CharsRight() > 0) && ((ch = this.RightChar()) >= '0')) && (ch <= '9'))
                        {
                            int num5 = ch - '0';
                            if ((num4 > 0xccccccc) || ((num4 == 0xccccccc) && (num5 > 7)))
                            {
                                throw this.MakeException(SR.GetString("CaptureGroupOutOfRange"));
                            }
                            num4 = (num4 * 10) + num5;
                            this.MoveRight();
                            if (this.IsCaptureSlot(num4))
                            {
                                m = num4;
                                num2 = this.Textpos();
                            }
                        }
                        this.Textto(num2);
                        if (m >= 0)
                        {
                            return new RegexNode(13, this._options, m);
                        }
                    }
                }
                else if (flag && RegexCharClass.IsWordChar(ch))
                {
                    string capname = this.ScanCapname();
                    if (((this.CharsRight() > 0) && (this.MoveRightGetChar() == '}')) && this.IsCaptureName(capname))
                    {
                        return new RegexNode(13, this._options, this.CaptureSlotFromName(capname));
                    }
                }
                else if (!flag)
                {
                    int num7 = 1;
                    switch (ch)
                    {
                        case '$':
                            this.MoveRight();
                            return new RegexNode(9, this._options, '$');

                        case '&':
                            num7 = 0;
                            break;

                        case '\'':
                            num7 = -2;
                            break;

                        case '+':
                            num7 = -3;
                            break;

                        case '_':
                            num7 = -4;
                            break;

                        case '`':
                            num7 = -1;
                            break;
                    }
                    if (num7 != 1)
                    {
                        this.MoveRight();
                        return new RegexNode(13, this._options, num7);
                    }
                }
                this.Textto(pos);
            }
            return new RegexNode(9, this._options, '$');
        }

        internal RegexNode ScanGroupOpen()
        {
            int num;
            int num4;
            char ch = '\0';
            char ch2 = '>';
            if (((this.CharsRight() == 0) || (this.RightChar() != '?')) || (((this.RightChar() == '?') && (this.CharsRight() > 1)) && (this.RightChar(1) == ')')))
            {
                if (!this.UseOptionN() && !this._ignoreNextParen)
                {
                    return new RegexNode(0x1c, this._options, this._autocap++, -1);
                }
                this._ignoreNextParen = false;
                return new RegexNode(0x1d, this._options);
            }
            this.MoveRight();
            if (this.CharsRight() == 0)
            {
                goto Label_055F;
            }
            switch ((ch = this.MoveRightGetChar()))
            {
                case '\'':
                    ch2 = '\'';
                    break;

                case '(':
                {
                    num4 = this.Textpos();
                    if (this.CharsRight() <= 0)
                    {
                        goto Label_048D;
                    }
                    ch = this.RightChar();
                    if ((ch < '0') || (ch > '9'))
                    {
                        if (RegexCharClass.IsWordChar(ch))
                        {
                            string capname = this.ScanCapname();
                            if ((this.IsCaptureName(capname) && (this.CharsRight() > 0)) && (this.MoveRightGetChar() == ')'))
                            {
                                return new RegexNode(0x21, this._options, this.CaptureSlotFromName(capname));
                            }
                        }
                        goto Label_048D;
                    }
                    int i = this.ScanDecimal();
                    if ((this.CharsRight() <= 0) || (this.MoveRightGetChar() != ')'))
                    {
                        throw this.MakeException(SR.GetString("MalformedReference", new object[] { i.ToString(CultureInfo.CurrentCulture) }));
                    }
                    if (!this.IsCaptureSlot(i))
                    {
                        throw this.MakeException(SR.GetString("UndefinedReference", new object[] { i.ToString(CultureInfo.CurrentCulture) }));
                    }
                    return new RegexNode(0x21, this._options, i);
                }
                case '!':
                    this._options &= ~RegexOptions.RightToLeft;
                    num = 0x1f;
                    goto Label_0552;

                case ':':
                    num = 0x1d;
                    goto Label_0552;

                case '<':
                    break;

                case '=':
                    this._options &= ~RegexOptions.RightToLeft;
                    num = 30;
                    goto Label_0552;

                case '>':
                    num = 0x20;
                    goto Label_0552;

                default:
                    goto Label_0528;
            }
            if (this.CharsRight() == 0)
            {
                goto Label_055F;
            }
            char ch5 = ch = this.MoveRightGetChar();
            if (ch5 != '!')
            {
                if (ch5 != '=')
                {
                    this.MoveLeft();
                    int num2 = -1;
                    int num3 = -1;
                    bool flag = false;
                    if ((ch >= '0') && (ch <= '9'))
                    {
                        num2 = this.ScanDecimal();
                        if (!this.IsCaptureSlot(num2))
                        {
                            num2 = -1;
                        }
                        if (((this.CharsRight() > 0) && (this.RightChar() != ch2)) && (this.RightChar() != '-'))
                        {
                            throw this.MakeException(SR.GetString("InvalidGroupName"));
                        }
                        if (num2 == 0)
                        {
                            throw this.MakeException(SR.GetString("CapnumNotZero"));
                        }
                    }
                    else if (RegexCharClass.IsWordChar(ch))
                    {
                        string str = this.ScanCapname();
                        if (this.IsCaptureName(str))
                        {
                            num2 = this.CaptureSlotFromName(str);
                        }
                        if (((this.CharsRight() > 0) && (this.RightChar() != ch2)) && (this.RightChar() != '-'))
                        {
                            throw this.MakeException(SR.GetString("InvalidGroupName"));
                        }
                    }
                    else
                    {
                        if (ch != '-')
                        {
                            throw this.MakeException(SR.GetString("InvalidGroupName"));
                        }
                        flag = true;
                    }
                    if (((num2 != -1) || flag) && ((this.CharsRight() > 0) && (this.RightChar() == '-')))
                    {
                        this.MoveRight();
                        ch = this.RightChar();
                        if ((ch >= '0') && (ch <= '9'))
                        {
                            num3 = this.ScanDecimal();
                            if (!this.IsCaptureSlot(num3))
                            {
                                throw this.MakeException(SR.GetString("UndefinedBackref", new object[] { num3 }));
                            }
                            if ((this.CharsRight() > 0) && (this.RightChar() != ch2))
                            {
                                throw this.MakeException(SR.GetString("InvalidGroupName"));
                            }
                        }
                        else
                        {
                            if (!RegexCharClass.IsWordChar(ch))
                            {
                                throw this.MakeException(SR.GetString("InvalidGroupName"));
                            }
                            string str2 = this.ScanCapname();
                            if (!this.IsCaptureName(str2))
                            {
                                throw this.MakeException(SR.GetString("UndefinedNameRef", new object[] { str2 }));
                            }
                            num3 = this.CaptureSlotFromName(str2);
                            if ((this.CharsRight() > 0) && (this.RightChar() != ch2))
                            {
                                throw this.MakeException(SR.GetString("InvalidGroupName"));
                            }
                        }
                    }
                    if (((num2 != -1) || (num3 != -1)) && ((this.CharsRight() > 0) && (this.MoveRightGetChar() == ch2)))
                    {
                        return new RegexNode(0x1c, this._options, num2, num3);
                    }
                    goto Label_055F;
                }
                if (ch2 == '\'')
                {
                    goto Label_055F;
                }
                this._options |= RegexOptions.RightToLeft;
                num = 30;
            }
            else
            {
                if (ch2 == '\'')
                {
                    goto Label_055F;
                }
                this._options |= RegexOptions.RightToLeft;
                num = 0x1f;
            }
            goto Label_0552;
        Label_048D:
            num = 0x22;
            this.Textto(num4 - 1);
            this._ignoreNextParen = true;
            int num6 = this.CharsRight();
            if ((num6 < 3) || (this.RightChar(1) != '?'))
            {
                goto Label_0552;
            }
            char ch3 = this.RightChar(2);
            switch (ch3)
            {
                case '#':
                    throw this.MakeException(SR.GetString("AlternationCantHaveComment"));

                case '\'':
                    throw this.MakeException(SR.GetString("AlternationCantCapture"));

                default:
                    if (((num6 >= 4) && (ch3 == '<')) && ((this.RightChar(3) != '!') && (this.RightChar(3) != '=')))
                    {
                        throw this.MakeException(SR.GetString("AlternationCantCapture"));
                    }
                    goto Label_0552;
            }
        Label_0528:
            this.MoveLeft();
            num = 0x1d;
            this.ScanOptions();
            if (this.CharsRight() == 0)
            {
                goto Label_055F;
            }
            ch = this.MoveRightGetChar();
            if (ch == ')')
            {
                return null;
            }
            if (ch != ':')
            {
                goto Label_055F;
            }
        Label_0552:
            return new RegexNode(num, this._options);
        Label_055F:
            throw this.MakeException(SR.GetString("UnrecognizedGrouping"));
        }

        internal char ScanHex(int c)
        {
            int num = 0;
            if (this.CharsRight() >= c)
            {
                int num2;
                while ((c > 0) && ((num2 = HexDigit(this.MoveRightGetChar())) >= 0))
                {
                    num *= 0x10;
                    num += num2;
                    c--;
                }
            }
            if (c > 0)
            {
                throw this.MakeException(SR.GetString("TooFewHex"));
            }
            return (char) num;
        }

        internal char ScanOctal()
        {
            int num;
            int num3 = 3;
            if (num3 > this.CharsRight())
            {
                num3 = this.CharsRight();
            }
            int num2 = 0;
            while ((num3 > 0) && ((num = this.RightChar() - '0') <= 7))
            {
                this.MoveRight();
                num2 *= 8;
                num2 += num;
                if (this.UseOptionE() && (num2 >= 0x20))
                {
                    break;
                }
                num3--;
            }
            num2 &= 0xff;
            return (char) num2;
        }

        internal void ScanOptions()
        {
            bool flag = false;
            while (this.CharsRight() > 0)
            {
                char ch = this.RightChar();
                switch (ch)
                {
                    case '-':
                        flag = true;
                        break;

                    case '+':
                        flag = false;
                        break;

                    default:
                    {
                        RegexOptions option = OptionFromCode(ch);
                        if ((option == RegexOptions.None) || this.IsOnlyTopOption(option))
                        {
                            return;
                        }
                        if (flag)
                        {
                            this._options &= ~option;
                        }
                        else
                        {
                            this._options |= option;
                        }
                        break;
                    }
                }
                this.MoveRight();
            }
        }

        internal RegexNode ScanRegex()
        {
            char ch = '@';
            bool flag = false;
            this.StartGroup(new RegexNode(0x1c, this._options, 0, -1));
            while (this.CharsRight() > 0)
            {
                int num2;
                RegexNode node;
                bool flag2 = flag;
                flag = false;
                this.ScanBlank();
                int pos = this.Textpos();
                if (!this.UseOptionX())
                {
                    goto Label_006D;
                }
                while ((this.CharsRight() > 0) && (!IsStopperX(ch = this.RightChar()) || ((ch == '{') && !this.IsTrueQuantifier())))
                {
                    this.MoveRight();
                }
                goto Label_0092;
            Label_0067:
                this.MoveRight();
            Label_006D:
                if ((this.CharsRight() > 0) && (!IsSpecial(ch = this.RightChar()) || ((ch == '{') && !this.IsTrueQuantifier())))
                {
                    goto Label_0067;
                }
            Label_0092:
                num2 = this.Textpos();
                this.ScanBlank();
                if (this.CharsRight() == 0)
                {
                    ch = '!';
                }
                else if (IsSpecial(ch = this.RightChar()))
                {
                    flag = IsQuantifier(ch);
                    this.MoveRight();
                }
                else
                {
                    ch = ' ';
                }
                if (pos < num2)
                {
                    int cch = (num2 - pos) - (flag ? 1 : 0);
                    flag2 = false;
                    if (cch > 0)
                    {
                        this.AddConcatenate(pos, cch, false);
                    }
                    if (flag)
                    {
                        this.AddUnitOne(this.CharAt(num2 - 1));
                    }
                }
                switch (ch)
                {
                    case ' ':
                    {
                        continue;
                    }
                    case '!':
                        goto Label_044B;

                    case '$':
                        this.AddUnitType(this.UseOptionM() ? 15 : 20);
                        goto Label_02D9;

                    case '(':
                    {
                        this.PushOptions();
                        node = this.ScanGroupOpen();
                        if (node != null)
                        {
                            break;
                        }
                        this.PopKeepOptions();
                        continue;
                    }
                    case ')':
                        if (this.EmptyStack())
                        {
                            throw this.MakeException(SR.GetString("TooManyParens"));
                        }
                        goto Label_0202;

                    case '*':
                    case '+':
                    case '?':
                    case '{':
                        if (this.Unit() == null)
                        {
                            throw this.MakeException(flag2 ? SR.GetString("NestedQuantify", new object[] { ch.ToString() }) : SR.GetString("QuantifyAfterNothing"));
                        }
                        this.MoveLeft();
                        goto Label_02D9;

                    case '.':
                        if (!this.UseOptionS())
                        {
                            goto Label_0279;
                        }
                        this.AddUnitSet("\0\x0001\0\0");
                        goto Label_02D9;

                    case '[':
                        this.AddUnitSet(this.ScanCharClass(this.UseOptionI()).ToStringClass());
                        goto Label_02D9;

                    case '\\':
                        this.AddUnitNode(this.ScanBackslash());
                        goto Label_02D9;

                    case '^':
                        this.AddUnitType(this.UseOptionM() ? 14 : 0x12);
                        goto Label_02D9;

                    case '|':
                    {
                        this.AddAlternate();
                        continue;
                    }
                    default:
                        throw this.MakeException(SR.GetString("InternalError"));
                }
                this.PushGroup();
                this.StartGroup(node);
                continue;
            Label_0202:
                this.AddGroup();
                this.PopGroup();
                this.PopOptions();
                if (this.Unit() != null)
                {
                    goto Label_02D9;
                }
                continue;
            Label_0279:
                this.AddUnitNotone('\n');
            Label_02D9:
                this.ScanBlank();
                if ((this.CharsRight() == 0) || !(flag = this.IsTrueQuantifier()))
                {
                    this.AddConcatenate();
                }
                else
                {
                    ch = this.MoveRightGetChar();
                    while (this.Unit() != null)
                    {
                        int num4;
                        int num5;
                        bool flag3;
                        switch (ch)
                        {
                            case '*':
                                num4 = 0;
                                num5 = 0x7fffffff;
                                goto Label_03EB;

                            case '+':
                                num4 = 1;
                                num5 = 0x7fffffff;
                                goto Label_03EB;

                            case '?':
                                num4 = 0;
                                num5 = 1;
                                goto Label_03EB;

                            case '{':
                                pos = this.Textpos();
                                num5 = num4 = this.ScanDecimal();
                                if (((pos < this.Textpos()) && (this.CharsRight() > 0)) && (this.RightChar() == ','))
                                {
                                    this.MoveRight();
                                    if ((this.CharsRight() != 0) && (this.RightChar() != '}'))
                                    {
                                        break;
                                    }
                                    num5 = 0x7fffffff;
                                }
                                goto Label_03AE;

                            default:
                                throw this.MakeException(SR.GetString("InternalError"));
                        }
                        num5 = this.ScanDecimal();
                    Label_03AE:
                        if (((pos == this.Textpos()) || (this.CharsRight() == 0)) || (this.MoveRightGetChar() != '}'))
                        {
                            this.AddConcatenate();
                            this.Textto(pos - 1);
                            continue;
                        }
                    Label_03EB:
                        this.ScanBlank();
                        if ((this.CharsRight() == 0) || (this.RightChar() != '?'))
                        {
                            flag3 = false;
                        }
                        else
                        {
                            this.MoveRight();
                            flag3 = true;
                        }
                        if (num4 > num5)
                        {
                            throw this.MakeException(SR.GetString("IllegalRange"));
                        }
                        this.AddConcatenate(flag3, num4, num5);
                    }
                }
            }
        Label_044B:
            if (!this.EmptyStack())
            {
                throw this.MakeException(SR.GetString("NotEnoughParens"));
            }
            this.AddGroup();
            return this.Unit();
        }

        internal RegexNode ScanReplacement()
        {
            this._concatenation = new RegexNode(0x19, this._options);
            while (true)
            {
                int num = this.CharsRight();
                if (num == 0)
                {
                    return this._concatenation;
                }
                int pos = this.Textpos();
                while ((num > 0) && (this.RightChar() != '$'))
                {
                    this.MoveRight();
                    num--;
                }
                this.AddConcatenate(pos, this.Textpos() - pos, true);
                if (num > 0)
                {
                    if (this.MoveRightGetChar() == '$')
                    {
                        this.AddUnitNode(this.ScanDollar());
                    }
                    this.AddConcatenate();
                }
            }
        }

        internal void SetPattern(string Re)
        {
            if (Re == null)
            {
                Re = string.Empty;
            }
            this._pattern = Re;
            this._currentPos = 0;
        }

        internal void StartGroup(RegexNode openGroup)
        {
            this._group = openGroup;
            this._alternation = new RegexNode(0x18, this._options);
            this._concatenation = new RegexNode(0x19, this._options);
        }

        internal int Textpos()
        {
            return this._currentPos;
        }

        internal void Textto(int pos)
        {
            this._currentPos = pos;
        }

        internal int TypeFromCode(char ch)
        {
            switch (ch)
            {
                case 'A':
                    return 0x12;

                case 'B':
                    if (this.UseOptionE())
                    {
                        return 0x2a;
                    }
                    return 0x11;

                case 'G':
                    return 0x13;

                case 'Z':
                    return 20;

                case 'b':
                    if (!this.UseOptionE())
                    {
                        return 0x10;
                    }
                    return 0x29;

                case 'z':
                    return 0x15;
            }
            return 0x16;
        }

        internal static string Unescape(string input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '\\')
                {
                    StringBuilder builder = new StringBuilder();
                    RegexParser parser = new RegexParser(CultureInfo.InvariantCulture);
                    parser.SetPattern(input);
                    builder.Append(input, 0, i);
                    do
                    {
                        i++;
                        parser.Textto(i);
                        if (i < input.Length)
                        {
                            builder.Append(parser.ScanCharEscape());
                        }
                        i = parser.Textpos();
                        int startIndex = i;
                        while ((i < input.Length) && (input[i] != '\\'))
                        {
                            i++;
                        }
                        builder.Append(input, startIndex, i - startIndex);
                    }
                    while (i < input.Length);
                    return builder.ToString();
                }
            }
            return input;
        }

        internal RegexNode Unit()
        {
            return this._unit;
        }

        internal bool UseOptionE()
        {
            return ((this._options & RegexOptions.ECMAScript) != RegexOptions.None);
        }

        internal bool UseOptionI()
        {
            return ((this._options & RegexOptions.IgnoreCase) != RegexOptions.None);
        }

        internal bool UseOptionM()
        {
            return ((this._options & RegexOptions.Multiline) != RegexOptions.None);
        }

        internal bool UseOptionN()
        {
            return ((this._options & RegexOptions.ExplicitCapture) != RegexOptions.None);
        }

        internal bool UseOptionS()
        {
            return ((this._options & RegexOptions.Singleline) != RegexOptions.None);
        }

        internal bool UseOptionX()
        {
            return ((this._options & RegexOptions.IgnorePatternWhitespace) != RegexOptions.None);
        }
    }
}

