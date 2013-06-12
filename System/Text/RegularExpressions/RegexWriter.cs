namespace System.Text.RegularExpressions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;

    internal sealed class RegexWriter
    {
        internal Hashtable _caps;
        internal int _count;
        internal bool _counting;
        internal int _curpos;
        internal int _depth;
        internal int[] _emitted = new int[0x20];
        internal int[] _intStack = new int[0x20];
        internal Dictionary<string, int> _stringhash = new Dictionary<string, int>();
        internal List<string> _stringtable = new List<string>();
        internal int _trackcount;
        internal const int AfterChild = 0x80;
        internal const int BeforeChild = 0x40;

        private RegexWriter()
        {
        }

        internal int CurPos()
        {
            return this._curpos;
        }

        internal void Emit(int op)
        {
            if (this._counting)
            {
                this._count++;
                if (RegexCode.OpcodeBacktracks(op))
                {
                    this._trackcount++;
                }
            }
            else
            {
                this._emitted[this._curpos++] = op;
            }
        }

        internal void Emit(int op, int opd1)
        {
            if (this._counting)
            {
                this._count += 2;
                if (RegexCode.OpcodeBacktracks(op))
                {
                    this._trackcount++;
                }
            }
            else
            {
                this._emitted[this._curpos++] = op;
                this._emitted[this._curpos++] = opd1;
            }
        }

        internal void Emit(int op, int opd1, int opd2)
        {
            if (this._counting)
            {
                this._count += 3;
                if (RegexCode.OpcodeBacktracks(op))
                {
                    this._trackcount++;
                }
            }
            else
            {
                this._emitted[this._curpos++] = op;
                this._emitted[this._curpos++] = opd1;
                this._emitted[this._curpos++] = opd2;
            }
        }

        internal void EmitFragment(int nodetype, RegexNode node, int CurIndex)
        {
            int num = 0;
            if (nodetype <= 13)
            {
                if (node.UseOptionR())
                {
                    num |= 0x40;
                }
                if ((node._options & RegexOptions.IgnoreCase) != RegexOptions.None)
                {
                    num |= 0x200;
                }
            }
            switch (nodetype)
            {
                case 3:
                case 4:
                case 6:
                case 7:
                    if (node._m > 0)
                    {
                        this.Emit((((node._type == 3) || (node._type == 6)) ? 0 : 1) | num, node._ch, node._m);
                    }
                    if (node._n > node._m)
                    {
                        this.Emit(node._type | num, node._ch, (node._n == 0x7fffffff) ? 0x7fffffff : (node._n - node._m));
                    }
                    return;

                case 5:
                case 8:
                    if (node._m > 0)
                    {
                        this.Emit(2 | num, this.StringCode(node._str), node._m);
                    }
                    if (node._n > node._m)
                    {
                        this.Emit(node._type | num, this.StringCode(node._str), (node._n == 0x7fffffff) ? 0x7fffffff : (node._n - node._m));
                    }
                    return;

                case 9:
                case 10:
                    this.Emit(node._type | num, node._ch);
                    return;

                case 11:
                    this.Emit(node._type | num, this.StringCode(node._str));
                    return;

                case 12:
                    this.Emit(node._type | num, this.StringCode(node._str));
                    return;

                case 13:
                    this.Emit(node._type | num, this.MapCapnum(node._m));
                    return;

                case 14:
                case 15:
                case 0x10:
                case 0x11:
                case 0x12:
                case 0x13:
                case 20:
                case 0x15:
                case 0x16:
                case 0x29:
                case 0x2a:
                    this.Emit(node._type);
                    return;

                case 0x17:
                case 0x59:
                case 0x5d:
                case 0x99:
                case 0x9d:
                    return;

                case 0x58:
                    if (CurIndex < (node._children.Count - 1))
                    {
                        this.PushInt(this.CurPos());
                        this.Emit(0x17, 0);
                    }
                    return;

                case 90:
                case 0x5b:
                    if ((node._n >= 0x7fffffff) && (node._m <= 1))
                    {
                        this.Emit((node._m == 0) ? 30 : 0x1f);
                    }
                    else
                    {
                        this.Emit((node._m == 0) ? 0x1a : 0x1b, (node._m == 0) ? 0 : (1 - node._m));
                    }
                    if (node._m == 0)
                    {
                        this.PushInt(this.CurPos());
                        this.Emit(0x26, 0);
                    }
                    this.PushInt(this.CurPos());
                    return;

                case 0x5c:
                    this.Emit(0x1f);
                    return;

                case 0x5e:
                    this.Emit(0x22);
                    this.Emit(0x1f);
                    return;

                case 0x5f:
                    this.Emit(0x22);
                    this.PushInt(this.CurPos());
                    this.Emit(0x17, 0);
                    return;

                case 0x60:
                    this.Emit(0x22);
                    return;

                case 0x61:
                    if (CurIndex == 0)
                    {
                        this.Emit(0x22);
                        this.PushInt(this.CurPos());
                        this.Emit(0x17, 0);
                        this.Emit(0x25, this.MapCapnum(node._m));
                        this.Emit(0x24);
                        return;
                    }
                    return;

                case 0x62:
                    if (CurIndex == 0)
                    {
                        this.Emit(0x22);
                        this.Emit(0x1f);
                        this.PushInt(this.CurPos());
                        this.Emit(0x17, 0);
                        return;
                    }
                    return;

                case 0x98:
                {
                    if (CurIndex >= (node._children.Count - 1))
                    {
                        for (int i = 0; i < CurIndex; i++)
                        {
                            this.PatchJump(this.PopInt(), this.CurPos());
                        }
                        return;
                    }
                    int offset = this.PopInt();
                    this.PushInt(this.CurPos());
                    this.Emit(0x26, 0);
                    this.PatchJump(offset, this.CurPos());
                    return;
                }
                case 0x9a:
                case 0x9b:
                {
                    int jumpDest = this.CurPos();
                    int num7 = nodetype - 0x9a;
                    if ((node._n >= 0x7fffffff) && (node._m <= 1))
                    {
                        this.Emit(0x18 + num7, this.PopInt());
                    }
                    else
                    {
                        this.Emit(0x1c + num7, this.PopInt(), (node._n == 0x7fffffff) ? 0x7fffffff : (node._n - node._m));
                    }
                    if (node._m == 0)
                    {
                        this.PatchJump(this.PopInt(), jumpDest);
                    }
                    return;
                }
                case 0x9c:
                    this.Emit(0x20, this.MapCapnum(node._m), this.MapCapnum(node._n));
                    return;

                case 0x9e:
                    this.Emit(0x21);
                    this.Emit(0x24);
                    return;

                case 0x9f:
                    this.Emit(0x23);
                    this.PatchJump(this.PopInt(), this.CurPos());
                    this.Emit(0x24);
                    return;

                case 160:
                    this.Emit(0x24);
                    return;

                case 0xa1:
                    switch (CurIndex)
                    {
                        case 0:
                        {
                            int num4 = this.PopInt();
                            this.PushInt(this.CurPos());
                            this.Emit(0x26, 0);
                            this.PatchJump(num4, this.CurPos());
                            this.Emit(0x24);
                            if (node._children.Count > 1)
                            {
                                return;
                            }
                            goto Label_025E;
                        }
                    }
                    return;

                case 0xa2:
                    switch (CurIndex)
                    {
                        case 0:
                            this.Emit(0x21);
                            this.Emit(0x24);
                            return;

                        case 1:
                        {
                            int num5 = this.PopInt();
                            this.PushInt(this.CurPos());
                            this.Emit(0x26, 0);
                            this.PatchJump(num5, this.CurPos());
                            this.Emit(0x21);
                            this.Emit(0x24);
                            if (node._children.Count > 2)
                            {
                                return;
                            }
                            goto Label_0312;
                        }
                        case 2:
                            goto Label_0312;
                    }
                    return;

                default:
                    throw this.MakeException(SR.GetString("UnexpectedOpcode", new object[] { nodetype.ToString(CultureInfo.CurrentCulture) }));
            }
        Label_025E:
            this.PatchJump(this.PopInt(), this.CurPos());
            return;
        Label_0312:
            this.PatchJump(this.PopInt(), this.CurPos());
        }

        internal bool EmptyStack()
        {
            return (this._depth == 0);
        }

        internal ArgumentException MakeException(string message)
        {
            return new ArgumentException(message);
        }

        internal int MapCapnum(int capnum)
        {
            if (capnum == -1)
            {
                return -1;
            }
            if (this._caps != null)
            {
                return (int) this._caps[capnum];
            }
            return capnum;
        }

        internal void PatchJump(int Offset, int jumpDest)
        {
            this._emitted[Offset + 1] = jumpDest;
        }

        internal int PopInt()
        {
            return this._intStack[--this._depth];
        }

        internal void PushInt(int I)
        {
            if (this._depth >= this._intStack.Length)
            {
                int[] destinationArray = new int[this._depth * 2];
                Array.Copy(this._intStack, 0, destinationArray, 0, this._depth);
                this._intStack = destinationArray;
            }
            this._intStack[this._depth++] = I;
        }

        internal RegexCode RegexCodeFromRegexTree(RegexTree tree)
        {
            int length;
            RegexBoyerMoore moore;
            if ((tree._capnumlist == null) || (tree._captop == tree._capnumlist.Length))
            {
                length = tree._captop;
                this._caps = null;
            }
            else
            {
                length = tree._capnumlist.Length;
                this._caps = tree._caps;
                for (int i = 0; i < tree._capnumlist.Length; i++)
                {
                    this._caps[tree._capnumlist[i]] = i;
                }
            }
            this._counting = true;
        Label_007B:
            if (!this._counting)
            {
                this._emitted = new int[this._count];
            }
            RegexNode node = tree._root;
            int curIndex = 0;
            this.Emit(0x17, 0);
        Label_00A6:
            if (node._children == null)
            {
                this.EmitFragment(node._type, node, 0);
            }
            else if (curIndex < node._children.Count)
            {
                this.EmitFragment(node._type | 0x40, node, curIndex);
                node = node._children[curIndex];
                this.PushInt(curIndex);
                curIndex = 0;
                goto Label_00A6;
            }
            if (!this.EmptyStack())
            {
                curIndex = this.PopInt();
                node = node._next;
                this.EmitFragment(node._type | 0x80, node, curIndex);
                curIndex++;
                goto Label_00A6;
            }
            this.PatchJump(0, this.CurPos());
            this.Emit(40);
            if (this._counting)
            {
                this._counting = false;
                goto Label_007B;
            }
            RegexPrefix fcPrefix = RegexFCD.FirstChars(tree);
            RegexPrefix prefix2 = RegexFCD.Prefix(tree);
            bool rightToLeft = (tree._options & RegexOptions.RightToLeft) != RegexOptions.None;
            CultureInfo culture = ((tree._options & RegexOptions.CultureInvariant) != RegexOptions.None) ? CultureInfo.InvariantCulture : CultureInfo.CurrentCulture;
            if ((prefix2 != null) && (prefix2.Prefix.Length > 0))
            {
                moore = new RegexBoyerMoore(prefix2.Prefix, prefix2.CaseInsensitive, rightToLeft, culture);
            }
            else
            {
                moore = null;
            }
            return new RegexCode(this._emitted, this._stringtable, this._trackcount, this._caps, length, moore, fcPrefix, RegexFCD.Anchors(tree), rightToLeft);
        }

        internal int StringCode(string str)
        {
            if (this._counting)
            {
                return 0;
            }
            if (str == null)
            {
                str = string.Empty;
            }
            if (this._stringhash.ContainsKey(str))
            {
                return this._stringhash[str];
            }
            int count = this._stringtable.Count;
            this._stringhash[str] = count;
            this._stringtable.Add(str);
            return count;
        }

        internal static RegexCode Write(RegexTree t)
        {
            RegexWriter writer = new RegexWriter();
            return writer.RegexCodeFromRegexTree(t);
        }
    }
}

