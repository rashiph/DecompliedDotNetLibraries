namespace System.Text.RegularExpressions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    internal sealed class RegexNode
    {
        internal char _ch;
        internal List<RegexNode> _children;
        internal int _m;
        internal int _n;
        internal RegexNode _next;
        internal RegexOptions _options;
        internal string _str;
        internal int _type;
        internal const int Alternate = 0x18;
        internal const int Beginning = 0x12;
        internal const int Bol = 14;
        internal const int Boundary = 0x10;
        internal const int Capture = 0x1c;
        internal const int Concatenate = 0x19;
        internal const int ECMABoundary = 0x29;
        internal const int Empty = 0x17;
        internal const int End = 0x15;
        internal const int EndZ = 20;
        internal const int Eol = 15;
        internal const int Greedy = 0x20;
        internal const int Group = 0x1d;
        internal const int Lazyloop = 0x1b;
        internal const int Loop = 0x1a;
        internal const int Multi = 12;
        internal const int Nonboundary = 0x11;
        internal const int NonECMABoundary = 0x2a;
        internal const int Nothing = 0x16;
        internal const int Notone = 10;
        internal const int Notonelazy = 7;
        internal const int Notoneloop = 4;
        internal const int One = 9;
        internal const int Onelazy = 6;
        internal const int Oneloop = 3;
        internal const int Prevent = 0x1f;
        internal const int Ref = 13;
        internal const int Require = 30;
        internal const int Set = 11;
        internal const int Setlazy = 8;
        internal const int Setloop = 5;
        internal const int Start = 0x13;
        internal const int Testgroup = 0x22;
        internal const int Testref = 0x21;

        internal RegexNode(int type, RegexOptions options)
        {
            this._type = type;
            this._options = options;
        }

        internal RegexNode(int type, RegexOptions options, char ch)
        {
            this._type = type;
            this._options = options;
            this._ch = ch;
        }

        internal RegexNode(int type, RegexOptions options, int m)
        {
            this._type = type;
            this._options = options;
            this._m = m;
        }

        internal RegexNode(int type, RegexOptions options, string str)
        {
            this._type = type;
            this._options = options;
            this._str = str;
        }

        internal RegexNode(int type, RegexOptions options, int m, int n)
        {
            this._type = type;
            this._options = options;
            this._m = m;
            this._n = n;
        }

        internal void AddChild(RegexNode newChild)
        {
            if (this._children == null)
            {
                this._children = new List<RegexNode>(4);
            }
            RegexNode item = newChild.Reduce();
            this._children.Add(item);
            item._next = this;
        }

        internal RegexNode Child(int i)
        {
            return this._children[i];
        }

        internal int ChildCount()
        {
            if (this._children != null)
            {
                return this._children.Count;
            }
            return 0;
        }

        internal RegexNode MakeQuantifier(bool lazy, int min, int max)
        {
            if ((min == 0) && (max == 0))
            {
                return new RegexNode(0x17, this._options);
            }
            if ((min == 1) && (max == 1))
            {
                return this;
            }
            switch (this._type)
            {
                case 9:
                case 10:
                case 11:
                    this.MakeRep(lazy ? 6 : 3, min, max);
                    return this;
            }
            RegexNode node = new RegexNode(lazy ? 0x1b : 0x1a, this._options, min, max);
            node.AddChild(this);
            return node;
        }

        internal void MakeRep(int type, int min, int max)
        {
            this._type += type - 9;
            this._m = min;
            this._n = max;
        }

        internal RegexNode Reduce()
        {
            switch (this.Type())
            {
                case 0x18:
                    return this.ReduceAlternation();

                case 0x19:
                    return this.ReduceConcatenation();

                case 0x1a:
                case 0x1b:
                    return this.ReduceRep();

                case 0x1d:
                    return this.ReduceGroup();

                case 11:
                case 5:
                    return this.ReduceSet();
            }
            return this;
        }

        internal RegexNode ReduceAlternation()
        {
            if (this._children == null)
            {
                return new RegexNode(0x16, this._options);
            }
            bool flag = false;
            bool flag2 = false;
            RegexOptions none = RegexOptions.None;
            int num = 0;
            int index = 0;
            while (num < this._children.Count)
            {
                RegexCharClass class2;
                RegexNode node = this._children[num];
                if (index < num)
                {
                    this._children[index] = node;
                }
                if (node._type == 0x18)
                {
                    for (int i = 0; i < node._children.Count; i++)
                    {
                        node._children[i]._next = this;
                    }
                    this._children.InsertRange(num + 1, node._children);
                    index--;
                    goto Label_01C2;
                }
                if ((node._type != 11) && (node._type != 9))
                {
                    goto Label_01AB;
                }
                RegexOptions options2 = node._options & (RegexOptions.RightToLeft | RegexOptions.IgnoreCase);
                if (node._type == 11)
                {
                    if ((flag && (none == options2)) && (!flag2 && RegexCharClass.IsMergeable(node._str)))
                    {
                        goto Label_011B;
                    }
                    flag = true;
                    flag2 = !RegexCharClass.IsMergeable(node._str);
                    none = options2;
                    goto Label_01C2;
                }
                if ((!flag || (none != options2)) || flag2)
                {
                    flag = true;
                    flag2 = false;
                    none = options2;
                    goto Label_01C2;
                }
            Label_011B:
                index--;
                RegexNode node2 = this._children[index];
                if (node2._type == 9)
                {
                    class2 = new RegexCharClass();
                    class2.AddChar(node2._ch);
                }
                else
                {
                    class2 = RegexCharClass.Parse(node2._str);
                }
                if (node._type == 9)
                {
                    class2.AddChar(node._ch);
                }
                else
                {
                    RegexCharClass cc = RegexCharClass.Parse(node._str);
                    class2.AddCharClass(cc);
                }
                node2._type = 11;
                node2._str = class2.ToStringClass();
                goto Label_01C2;
            Label_01AB:
                if (node._type == 0x16)
                {
                    index--;
                }
                else
                {
                    flag = false;
                    flag2 = false;
                }
            Label_01C2:
                num++;
                index++;
            }
            if (index < num)
            {
                this._children.RemoveRange(index, num - index);
            }
            return this.StripEnation(0x16);
        }

        internal RegexNode ReduceConcatenation()
        {
            if (this._children == null)
            {
                return new RegexNode(0x17, this._options);
            }
            bool flag = false;
            RegexOptions none = RegexOptions.None;
            int num = 0;
            int index = 0;
            while (num < this._children.Count)
            {
                RegexNode node = this._children[num];
                if (index < num)
                {
                    this._children[index] = node;
                }
                if ((node._type == 0x19) && ((node._options & RegexOptions.RightToLeft) == (this._options & RegexOptions.RightToLeft)))
                {
                    for (int i = 0; i < node._children.Count; i++)
                    {
                        node._children[i]._next = this;
                    }
                    this._children.InsertRange(num + 1, node._children);
                    index--;
                }
                else if ((node._type == 12) || (node._type == 9))
                {
                    RegexOptions options2 = node._options & (RegexOptions.RightToLeft | RegexOptions.IgnoreCase);
                    if (!flag || (none != options2))
                    {
                        flag = true;
                        none = options2;
                    }
                    else
                    {
                        RegexNode node2 = this._children[--index];
                        if (node2._type == 9)
                        {
                            node2._type = 12;
                            node2._str = Convert.ToString(node2._ch, CultureInfo.InvariantCulture);
                        }
                        if ((options2 & RegexOptions.RightToLeft) == RegexOptions.None)
                        {
                            if (node._type == 9)
                            {
                                node2._str = node2._str + node._ch.ToString();
                            }
                            else
                            {
                                node2._str = node2._str + node._str;
                            }
                        }
                        else if (node._type == 9)
                        {
                            node2._str = node._ch.ToString() + node2._str;
                        }
                        else
                        {
                            node2._str = node._str + node2._str;
                        }
                    }
                }
                else if (node._type == 0x17)
                {
                    index--;
                }
                else
                {
                    flag = false;
                }
                num++;
                index++;
            }
            if (index < num)
            {
                this._children.RemoveRange(index, num - index);
            }
            return this.StripEnation(0x17);
        }

        internal RegexNode ReduceGroup()
        {
            RegexNode node = this;
            while (node.Type() == 0x1d)
            {
                node = node.Child(0);
            }
            return node;
        }

        internal RegexNode ReduceRep()
        {
            RegexNode node = this;
            int num = this.Type();
            int num2 = this._m;
            int num3 = this._n;
            while (true)
            {
                if (node.ChildCount() == 0)
                {
                    break;
                }
                RegexNode node2 = node.Child(0);
                if (node2.Type() != num)
                {
                    int num4 = node2.Type();
                    if ((((num4 < 3) || (num4 > 5)) || (num != 0x1a)) && (((num4 < 6) || (num4 > 8)) || (num != 0x1b)))
                    {
                        break;
                    }
                }
                if (((node._m == 0) && (node2._m > 1)) || (node2._n < (node2._m * 2)))
                {
                    break;
                }
                node = node2;
                if (node._m > 0)
                {
                    node._m = num2 = ((0x7ffffffe / node._m) < num2) ? 0x7fffffff : (node._m * num2);
                }
                if (node._n > 0)
                {
                    node._n = num3 = ((0x7ffffffe / node._n) < num3) ? 0x7fffffff : (node._n * num3);
                }
            }
            if (num2 != 0x7fffffff)
            {
                return node;
            }
            return new RegexNode(0x16, this._options);
        }

        internal RegexNode ReduceSet()
        {
            if (RegexCharClass.IsEmpty(this._str))
            {
                this._type = 0x16;
                this._str = null;
            }
            else if (RegexCharClass.IsSingleton(this._str))
            {
                this._ch = RegexCharClass.SingletonChar(this._str);
                this._str = null;
                this._type += -2;
            }
            else if (RegexCharClass.IsSingletonInverse(this._str))
            {
                this._ch = RegexCharClass.SingletonChar(this._str);
                this._str = null;
                this._type += -1;
            }
            return this;
        }

        internal RegexNode ReverseLeft()
        {
            if ((this.UseOptionR() && (this._type == 0x19)) && (this._children != null))
            {
                this._children.Reverse(0, this._children.Count);
            }
            return this;
        }

        internal RegexNode StripEnation(int emptyType)
        {
            switch (this.ChildCount())
            {
                case 0:
                    return new RegexNode(emptyType, this._options);

                case 1:
                    return this.Child(0);
            }
            return this;
        }

        internal int Type()
        {
            return this._type;
        }

        internal bool UseOptionR()
        {
            return ((this._options & RegexOptions.RightToLeft) != RegexOptions.None);
        }
    }
}

