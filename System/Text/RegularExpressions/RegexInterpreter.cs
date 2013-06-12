namespace System.Text.RegularExpressions
{
    using System;
    using System.Globalization;

    internal sealed class RegexInterpreter : RegexRunner
    {
        internal int runanchors;
        internal RegexBoyerMoore runbmPrefix;
        internal bool runci;
        internal RegexCode runcode;
        internal int runcodepos;
        internal int[] runcodes;
        internal CultureInfo runculture;
        internal RegexPrefix runfcPrefix;
        internal int runoperator;
        internal bool runrtl;
        internal string[] runstrings;

        internal RegexInterpreter(RegexCode code, CultureInfo culture)
        {
            this.runcode = code;
            this.runcodes = code._codes;
            this.runstrings = code._strings;
            this.runfcPrefix = code._fcPrefix;
            this.runbmPrefix = code._bmPrefix;
            this.runanchors = code._anchors;
            this.runculture = culture;
        }

        private void Advance()
        {
            this.Advance(0);
        }

        private void Advance(int i)
        {
            this.runcodepos += i + 1;
            this.SetOperator(this.runcodes[this.runcodepos]);
        }

        private void Backtrack()
        {
            int index = base.runtrack[base.runtrackpos++];
            if (index < 0)
            {
                index = -index;
                this.SetOperator(this.runcodes[index] | 0x100);
            }
            else
            {
                this.SetOperator(this.runcodes[index] | 0x80);
            }
            if (index < this.runcodepos)
            {
                base.EnsureStorage();
            }
            this.runcodepos = index;
        }

        private void Backwardnext()
        {
            base.runtextpos += this.runrtl ? 1 : -1;
        }

        private int Bump()
        {
            if (!this.runrtl)
            {
                return 1;
            }
            return -1;
        }

        private char CharAt(int j)
        {
            return base.runtext[j];
        }

        protected override bool FindFirstChar()
        {
            int num;
            if ((this.runanchors & 0x35) != 0)
            {
                if (!this.runcode._rightToLeft)
                {
                    if ((((this.runanchors & 1) != 0) && (base.runtextpos > base.runtextbeg)) || (((this.runanchors & 4) != 0) && (base.runtextpos > base.runtextstart)))
                    {
                        base.runtextpos = base.runtextend;
                        return false;
                    }
                    if (((this.runanchors & 0x10) != 0) && (base.runtextpos < (base.runtextend - 1)))
                    {
                        base.runtextpos = base.runtextend - 1;
                    }
                    else if (((this.runanchors & 0x20) != 0) && (base.runtextpos < base.runtextend))
                    {
                        base.runtextpos = base.runtextend;
                    }
                }
                else
                {
                    if (((((this.runanchors & 0x20) != 0) && (base.runtextpos < base.runtextend)) || (((this.runanchors & 0x10) != 0) && ((base.runtextpos < (base.runtextend - 1)) || ((base.runtextpos == (base.runtextend - 1)) && (this.CharAt(base.runtextpos) != '\n'))))) || (((this.runanchors & 4) != 0) && (base.runtextpos < base.runtextstart)))
                    {
                        base.runtextpos = base.runtextbeg;
                        return false;
                    }
                    if (((this.runanchors & 1) != 0) && (base.runtextpos > base.runtextbeg))
                    {
                        base.runtextpos = base.runtextbeg;
                    }
                }
                if (this.runbmPrefix != null)
                {
                    return this.runbmPrefix.IsMatch(base.runtext, base.runtextpos, base.runtextbeg, base.runtextend);
                }
                return true;
            }
            if (this.runbmPrefix != null)
            {
                base.runtextpos = this.runbmPrefix.Scan(base.runtext, base.runtextpos, base.runtextbeg, base.runtextend);
                if (base.runtextpos == -1)
                {
                    base.runtextpos = this.runcode._rightToLeft ? base.runtextbeg : base.runtextend;
                    return false;
                }
                return true;
            }
            if (this.runfcPrefix == null)
            {
                return true;
            }
            this.runrtl = this.runcode._rightToLeft;
            this.runci = this.runfcPrefix.CaseInsensitive;
            string prefix = this.runfcPrefix.Prefix;
            if (RegexCharClass.IsSingleton(prefix))
            {
                char ch = RegexCharClass.SingletonChar(prefix);
                for (num = this.Forwardchars(); num > 0; num--)
                {
                    if (ch == this.Forwardcharnext())
                    {
                        this.Backwardnext();
                        return true;
                    }
                }
            }
            else
            {
                for (num = this.Forwardchars(); num > 0; num--)
                {
                    if (RegexCharClass.CharInClass(this.Forwardcharnext(), prefix))
                    {
                        this.Backwardnext();
                        return true;
                    }
                }
            }
            return false;
        }

        private char Forwardcharnext()
        {
            char c = this.runrtl ? base.runtext[--base.runtextpos] : base.runtext[base.runtextpos++];
            if (!this.runci)
            {
                return c;
            }
            return char.ToLower(c, this.runculture);
        }

        private int Forwardchars()
        {
            if (!this.runrtl)
            {
                return (base.runtextend - base.runtextpos);
            }
            return (base.runtextpos - base.runtextbeg);
        }

        protected override void Go()
        {
            this.Goto(0);
        Label_0007:
            switch (this.Operator())
            {
                case 0:
                {
                    int num12 = this.Operand(1);
                    if (this.Forwardchars() < num12)
                    {
                        goto Label_0E4E;
                    }
                    char ch = (char) this.Operand(0);
                    while (num12-- > 0)
                    {
                        if (this.Forwardcharnext() != ch)
                        {
                            goto Label_0E4E;
                        }
                    }
                    this.Advance(2);
                    goto Label_0007;
                }
                case 1:
                {
                    int num13 = this.Operand(1);
                    if (this.Forwardchars() < num13)
                    {
                        goto Label_0E4E;
                    }
                    char ch2 = (char) this.Operand(0);
                    while (num13-- > 0)
                    {
                        if (this.Forwardcharnext() == ch2)
                        {
                            goto Label_0E4E;
                        }
                    }
                    this.Advance(2);
                    goto Label_0007;
                }
                case 2:
                {
                    int num14 = this.Operand(1);
                    if (this.Forwardchars() < num14)
                    {
                        goto Label_0E4E;
                    }
                    string set = this.runstrings[this.Operand(0)];
                    while (num14-- > 0)
                    {
                        if (!RegexCharClass.CharInClass(this.Forwardcharnext(), set))
                        {
                            goto Label_0E4E;
                        }
                    }
                    this.Advance(2);
                    goto Label_0007;
                }
                case 3:
                {
                    int num15 = this.Operand(1);
                    if (num15 > this.Forwardchars())
                    {
                        num15 = this.Forwardchars();
                    }
                    char ch3 = (char) this.Operand(0);
                    int num16 = num15;
                    while (num16 > 0)
                    {
                        if (this.Forwardcharnext() != ch3)
                        {
                            this.Backwardnext();
                            break;
                        }
                        num16--;
                    }
                    if (num15 > num16)
                    {
                        this.TrackPush((num15 - num16) - 1, this.Textpos() - this.Bump());
                    }
                    this.Advance(2);
                    goto Label_0007;
                }
                case 4:
                {
                    int num17 = this.Operand(1);
                    if (num17 > this.Forwardchars())
                    {
                        num17 = this.Forwardchars();
                    }
                    char ch4 = (char) this.Operand(0);
                    int num18 = num17;
                    while (num18 > 0)
                    {
                        if (this.Forwardcharnext() == ch4)
                        {
                            this.Backwardnext();
                            break;
                        }
                        num18--;
                    }
                    if (num17 > num18)
                    {
                        this.TrackPush((num17 - num18) - 1, this.Textpos() - this.Bump());
                    }
                    this.Advance(2);
                    goto Label_0007;
                }
                case 5:
                {
                    int num19 = this.Operand(1);
                    if (num19 > this.Forwardchars())
                    {
                        num19 = this.Forwardchars();
                    }
                    string str2 = this.runstrings[this.Operand(0)];
                    int num20 = num19;
                    while (num20 > 0)
                    {
                        if (!RegexCharClass.CharInClass(this.Forwardcharnext(), str2))
                        {
                            this.Backwardnext();
                            break;
                        }
                        num20--;
                    }
                    if (num19 > num20)
                    {
                        this.TrackPush((num19 - num20) - 1, this.Textpos() - this.Bump());
                    }
                    this.Advance(2);
                    goto Label_0007;
                }
                case 6:
                case 7:
                {
                    int num25 = this.Operand(1);
                    if (num25 > this.Forwardchars())
                    {
                        num25 = this.Forwardchars();
                    }
                    if (num25 > 0)
                    {
                        this.TrackPush(num25 - 1, this.Textpos());
                    }
                    this.Advance(2);
                    goto Label_0007;
                }
                case 8:
                {
                    int num26 = this.Operand(1);
                    if (num26 > this.Forwardchars())
                    {
                        num26 = this.Forwardchars();
                    }
                    if (num26 > 0)
                    {
                        this.TrackPush(num26 - 1, this.Textpos());
                    }
                    this.Advance(2);
                    goto Label_0007;
                }
                case 9:
                    if ((this.Forwardchars() < 1) || (this.Forwardcharnext() != ((char) this.Operand(0))))
                    {
                        goto Label_0E4E;
                    }
                    this.Advance(1);
                    goto Label_0007;

                case 10:
                    if ((this.Forwardchars() < 1) || (this.Forwardcharnext() == ((char) this.Operand(0))))
                    {
                        goto Label_0E4E;
                    }
                    this.Advance(1);
                    goto Label_0007;

                case 11:
                    if ((this.Forwardchars() < 1) || !RegexCharClass.CharInClass(this.Forwardcharnext(), this.runstrings[this.Operand(0)]))
                    {
                        goto Label_0E4E;
                    }
                    this.Advance(1);
                    goto Label_0007;

                case 12:
                    if (!this.Stringmatch(this.runstrings[this.Operand(0)]))
                    {
                        goto Label_0E4E;
                    }
                    this.Advance(1);
                    goto Label_0007;

                case 13:
                {
                    int cap = this.Operand(0);
                    if (!base.IsMatched(cap))
                    {
                        if ((base.runregex.roptions & RegexOptions.ECMAScript) == RegexOptions.None)
                        {
                            goto Label_0E4E;
                        }
                        goto Label_09E4;
                    }
                    if (this.Refmatch(base.MatchIndex(cap), base.MatchLength(cap)))
                    {
                        goto Label_09E4;
                    }
                    goto Label_0E4E;
                }
                case 14:
                    if ((this.Leftchars() > 0) && (this.CharAt(this.Textpos() - 1) != '\n'))
                    {
                        goto Label_0E4E;
                    }
                    this.Advance();
                    goto Label_0007;

                case 15:
                    if ((this.Rightchars() > 0) && (this.CharAt(this.Textpos()) != '\n'))
                    {
                        goto Label_0E4E;
                    }
                    this.Advance();
                    goto Label_0007;

                case 0x10:
                    if (!base.IsBoundary(this.Textpos(), base.runtextbeg, base.runtextend))
                    {
                        goto Label_0E4E;
                    }
                    this.Advance();
                    goto Label_0007;

                case 0x11:
                    if (base.IsBoundary(this.Textpos(), base.runtextbeg, base.runtextend))
                    {
                        goto Label_0E4E;
                    }
                    this.Advance();
                    goto Label_0007;

                case 0x12:
                    if (this.Leftchars() > 0)
                    {
                        goto Label_0E4E;
                    }
                    this.Advance();
                    goto Label_0007;

                case 0x13:
                    if (this.Textpos() != this.Textstart())
                    {
                        goto Label_0E4E;
                    }
                    this.Advance();
                    goto Label_0007;

                case 20:
                    if ((this.Rightchars() > 1) || ((this.Rightchars() == 1) && (this.CharAt(this.Textpos()) != '\n')))
                    {
                        goto Label_0E4E;
                    }
                    this.Advance();
                    goto Label_0007;

                case 0x15:
                    if (this.Rightchars() > 0)
                    {
                        goto Label_0E4E;
                    }
                    this.Advance();
                    goto Label_0007;

                case 0x16:
                    goto Label_0E4E;

                case 0x17:
                    this.TrackPush(this.Textpos());
                    this.Advance(1);
                    goto Label_0007;

                case 0x18:
                    this.StackPop();
                    if ((this.Textpos() - this.StackPeek()) == 0)
                    {
                        this.TrackPush2(this.StackPeek());
                        this.Advance(1);
                    }
                    else
                    {
                        this.TrackPush(this.StackPeek(), this.Textpos());
                        this.StackPush(this.Textpos());
                        this.Goto(this.Operand(0));
                    }
                    goto Label_0007;

                case 0x19:
                {
                    this.StackPop();
                    int num2 = this.StackPeek();
                    if (this.Textpos() == num2)
                    {
                        this.StackPush(num2);
                        this.TrackPush2(this.StackPeek());
                        break;
                    }
                    if (num2 == -1)
                    {
                        this.TrackPush(this.Textpos(), this.Textpos());
                        break;
                    }
                    this.TrackPush(num2, this.Textpos());
                    break;
                }
                case 0x1a:
                    this.StackPush(-1, this.Operand(0));
                    this.TrackPush();
                    this.Advance(1);
                    goto Label_0007;

                case 0x1b:
                    this.StackPush(this.Textpos(), this.Operand(0));
                    this.TrackPush();
                    this.Advance(1);
                    goto Label_0007;

                case 0x1c:
                {
                    this.StackPop(2);
                    int num4 = this.StackPeek();
                    int num5 = this.StackPeek(1);
                    int num6 = this.Textpos() - num4;
                    if ((num5 < this.Operand(1)) && ((num6 != 0) || (num5 < 0)))
                    {
                        this.TrackPush(num4);
                        this.StackPush(this.Textpos(), num5 + 1);
                        this.Goto(this.Operand(0));
                    }
                    else
                    {
                        this.TrackPush2(num4, num5);
                        this.Advance(2);
                    }
                    goto Label_0007;
                }
                case 0x1d:
                {
                    this.StackPop(2);
                    int num7 = this.StackPeek();
                    int num8 = this.StackPeek(1);
                    if (num8 >= 0)
                    {
                        this.TrackPush(num7, num8, this.Textpos());
                        this.Advance(2);
                    }
                    else
                    {
                        this.TrackPush2(num7);
                        this.StackPush(this.Textpos(), num8 + 1);
                        this.Goto(this.Operand(0));
                    }
                    goto Label_0007;
                }
                case 30:
                    this.StackPush(-1);
                    this.TrackPush();
                    this.Advance();
                    goto Label_0007;

                case 0x1f:
                    this.StackPush(this.Textpos());
                    this.TrackPush();
                    this.Advance();
                    goto Label_0007;

                case 0x20:
                    if ((this.Operand(1) != -1) && !base.IsMatched(this.Operand(1)))
                    {
                        goto Label_0E4E;
                    }
                    this.StackPop();
                    if (this.Operand(1) != -1)
                    {
                        base.TransferCapture(this.Operand(0), this.Operand(1), this.StackPeek(), this.Textpos());
                    }
                    else
                    {
                        base.Capture(this.Operand(0), this.StackPeek(), this.Textpos());
                    }
                    this.TrackPush(this.StackPeek());
                    this.Advance(2);
                    goto Label_0007;

                case 0x21:
                    this.StackPop();
                    this.TrackPush(this.StackPeek());
                    this.Textto(this.StackPeek());
                    this.Advance();
                    goto Label_0007;

                case 0x22:
                    this.StackPush(this.Trackpos(), base.Crawlpos());
                    this.TrackPush();
                    this.Advance();
                    goto Label_0007;

                case 0x23:
                    this.StackPop(2);
                    this.Trackto(this.StackPeek());
                    while (base.Crawlpos() != this.StackPeek(1))
                    {
                        base.Uncapture();
                    }
                    goto Label_0E4E;

                case 0x24:
                    this.StackPop(2);
                    this.Trackto(this.StackPeek());
                    this.TrackPush(this.StackPeek(1));
                    this.Advance();
                    goto Label_0007;

                case 0x25:
                    if (!base.IsMatched(this.Operand(0)))
                    {
                        goto Label_0E4E;
                    }
                    this.Advance(1);
                    goto Label_0007;

                case 0x26:
                    this.Goto(this.Operand(0));
                    goto Label_0007;

                case 40:
                    return;

                case 0x29:
                    if (!base.IsECMABoundary(this.Textpos(), base.runtextbeg, base.runtextend))
                    {
                        goto Label_0E4E;
                    }
                    this.Advance();
                    goto Label_0007;

                case 0x2a:
                    if (base.IsECMABoundary(this.Textpos(), base.runtextbeg, base.runtextend))
                    {
                        goto Label_0E4E;
                    }
                    this.Advance();
                    goto Label_0007;

                case 0x83:
                case 0x84:
                {
                    this.TrackPop(2);
                    int num21 = this.TrackPeek();
                    int newpos = this.TrackPeek(1);
                    this.Textto(newpos);
                    if (num21 > 0)
                    {
                        this.TrackPush(num21 - 1, newpos - this.Bump());
                    }
                    this.Advance(2);
                    goto Label_0007;
                }
                case 0x85:
                {
                    this.TrackPop(2);
                    int num23 = this.TrackPeek();
                    int num24 = this.TrackPeek(1);
                    this.Textto(num24);
                    if (num23 > 0)
                    {
                        this.TrackPush(num23 - 1, num24 - this.Bump());
                    }
                    this.Advance(2);
                    goto Label_0007;
                }
                case 0x86:
                {
                    this.TrackPop(2);
                    int num27 = this.TrackPeek(1);
                    this.Textto(num27);
                    if (this.Forwardcharnext() != ((char) this.Operand(0)))
                    {
                        goto Label_0E4E;
                    }
                    int num28 = this.TrackPeek();
                    if (num28 > 0)
                    {
                        this.TrackPush(num28 - 1, num27 + this.Bump());
                    }
                    this.Advance(2);
                    goto Label_0007;
                }
                case 0x87:
                {
                    this.TrackPop(2);
                    int num29 = this.TrackPeek(1);
                    this.Textto(num29);
                    if (this.Forwardcharnext() == ((char) this.Operand(0)))
                    {
                        goto Label_0E4E;
                    }
                    int num30 = this.TrackPeek();
                    if (num30 > 0)
                    {
                        this.TrackPush(num30 - 1, num29 + this.Bump());
                    }
                    this.Advance(2);
                    goto Label_0007;
                }
                case 0x88:
                {
                    this.TrackPop(2);
                    int num31 = this.TrackPeek(1);
                    this.Textto(num31);
                    if (!RegexCharClass.CharInClass(this.Forwardcharnext(), this.runstrings[this.Operand(0)]))
                    {
                        goto Label_0E4E;
                    }
                    int num32 = this.TrackPeek();
                    if (num32 > 0)
                    {
                        this.TrackPush(num32 - 1, num31 + this.Bump());
                    }
                    this.Advance(2);
                    goto Label_0007;
                }
                case 0x97:
                    this.TrackPop();
                    this.Textto(this.TrackPeek());
                    this.Goto(this.Operand(0));
                    goto Label_0007;

                case 0x98:
                    this.TrackPop(2);
                    this.StackPop();
                    this.Textto(this.TrackPeek(1));
                    this.TrackPush2(this.TrackPeek());
                    this.Advance(1);
                    goto Label_0007;

                case 0x99:
                {
                    this.TrackPop(2);
                    int num3 = this.TrackPeek(1);
                    this.TrackPush2(this.TrackPeek());
                    this.StackPush(num3);
                    this.Textto(num3);
                    this.Goto(this.Operand(0));
                    goto Label_0007;
                }
                case 0x9a:
                    this.StackPop(2);
                    goto Label_0E4E;

                case 0x9b:
                    this.StackPop(2);
                    goto Label_0E4E;

                case 0x9c:
                    this.TrackPop();
                    this.StackPop(2);
                    if (this.StackPeek(1) <= 0)
                    {
                        this.StackPush(this.TrackPeek(), this.StackPeek(1) - 1);
                        goto Label_0E4E;
                    }
                    this.Textto(this.StackPeek());
                    this.TrackPush2(this.TrackPeek(), this.StackPeek(1) - 1);
                    this.Advance(2);
                    goto Label_0007;

                case 0x9d:
                {
                    this.TrackPop(3);
                    int num9 = this.TrackPeek();
                    int num10 = this.TrackPeek(2);
                    if ((this.TrackPeek(1) >= this.Operand(1)) || (num10 == num9))
                    {
                        this.StackPush(this.TrackPeek(), this.TrackPeek(1));
                        goto Label_0E4E;
                    }
                    this.Textto(num10);
                    this.StackPush(num10, this.TrackPeek(1) + 1);
                    this.TrackPush2(num9);
                    this.Goto(this.Operand(0));
                    goto Label_0007;
                }
                case 0x9e:
                case 0x9f:
                    this.StackPop();
                    goto Label_0E4E;

                case 160:
                    this.TrackPop();
                    this.StackPush(this.TrackPeek());
                    base.Uncapture();
                    if ((this.Operand(0) != -1) && (this.Operand(1) != -1))
                    {
                        base.Uncapture();
                    }
                    goto Label_0E4E;

                case 0xa1:
                    this.TrackPop();
                    this.StackPush(this.TrackPeek());
                    goto Label_0E4E;

                case 0xa2:
                    this.StackPop(2);
                    goto Label_0E4E;

                case 0xa4:
                    this.TrackPop();
                    while (base.Crawlpos() != this.TrackPeek())
                    {
                        base.Uncapture();
                    }
                    goto Label_0E4E;

                case 280:
                    this.TrackPop();
                    this.StackPush(this.TrackPeek());
                    goto Label_0E4E;

                case 0x119:
                    this.StackPop();
                    this.TrackPop();
                    this.StackPush(this.TrackPeek());
                    goto Label_0E4E;

                case 0x11c:
                    this.TrackPop(2);
                    this.StackPush(this.TrackPeek(), this.TrackPeek(1));
                    goto Label_0E4E;

                case 0x11d:
                    this.TrackPop();
                    this.StackPop(2);
                    this.StackPush(this.TrackPeek(), this.StackPeek(1) - 1);
                    goto Label_0E4E;

                default:
                    throw new NotImplementedException(SR.GetString("UnimplementedState"));
            }
            this.Advance(1);
            goto Label_0007;
        Label_09E4:
            this.Advance(1);
            goto Label_0007;
        Label_0E4E:
            this.Backtrack();
            goto Label_0007;
        }

        private void Goto(int newpos)
        {
            if (newpos < this.runcodepos)
            {
                base.EnsureStorage();
            }
            this.SetOperator(this.runcodes[newpos]);
            this.runcodepos = newpos;
        }

        protected override void InitTrackCount()
        {
            base.runtrackcount = this.runcode._trackcount;
        }

        private int Leftchars()
        {
            return (base.runtextpos - base.runtextbeg);
        }

        private int Operand(int i)
        {
            return this.runcodes[(this.runcodepos + i) + 1];
        }

        private int Operator()
        {
            return this.runoperator;
        }

        private bool Refmatch(int index, int len)
        {
            int runtextpos;
            if (!this.runrtl)
            {
                if ((base.runtextend - base.runtextpos) < len)
                {
                    return false;
                }
                runtextpos = base.runtextpos + len;
            }
            else
            {
                if ((base.runtextpos - base.runtextbeg) < len)
                {
                    return false;
                }
                runtextpos = base.runtextpos;
            }
            int num3 = index + len;
            int num = len;
            if (this.runci)
            {
                while (num-- != 0)
                {
                    if (char.ToLower(base.runtext[--num3], this.runculture) != char.ToLower(base.runtext[--runtextpos], this.runculture))
                    {
                        return false;
                    }
                }
            }
            else
            {
                while (num-- != 0)
                {
                    if (base.runtext[--num3] != base.runtext[--runtextpos])
                    {
                        return false;
                    }
                }
            }
            if (!this.runrtl)
            {
                runtextpos += len;
            }
            base.runtextpos = runtextpos;
            return true;
        }

        private int Rightchars()
        {
            return (base.runtextend - base.runtextpos);
        }

        private void SetOperator(int op)
        {
            this.runci = 0 != (op & 0x200);
            this.runrtl = 0 != (op & 0x40);
            this.runoperator = op & -577;
        }

        private int StackPeek()
        {
            return base.runstack[base.runstackpos - 1];
        }

        private int StackPeek(int i)
        {
            return base.runstack[(base.runstackpos - i) - 1];
        }

        private void StackPop()
        {
            base.runstackpos++;
        }

        private void StackPop(int framesize)
        {
            base.runstackpos += framesize;
        }

        private void StackPush(int I1)
        {
            base.runstack[--base.runstackpos] = I1;
        }

        private void StackPush(int I1, int I2)
        {
            base.runstack[--base.runstackpos] = I1;
            base.runstack[--base.runstackpos] = I2;
        }

        private bool Stringmatch(string str)
        {
            int num;
            int runtextpos;
            if (!this.runrtl)
            {
                if ((base.runtextend - base.runtextpos) < (num = str.Length))
                {
                    return false;
                }
                runtextpos = base.runtextpos + num;
            }
            else
            {
                if ((base.runtextpos - base.runtextbeg) < (num = str.Length))
                {
                    return false;
                }
                runtextpos = base.runtextpos;
            }
            if (this.runci)
            {
                while (num != 0)
                {
                    if (str[--num] != char.ToLower(base.runtext[--runtextpos], this.runculture))
                    {
                        return false;
                    }
                }
            }
            else
            {
                while (num != 0)
                {
                    if (str[--num] != base.runtext[--runtextpos])
                    {
                        return false;
                    }
                }
            }
            if (!this.runrtl)
            {
                runtextpos += str.Length;
            }
            base.runtextpos = runtextpos;
            return true;
        }

        private int Textpos()
        {
            return base.runtextpos;
        }

        private int Textstart()
        {
            return base.runtextstart;
        }

        private void Textto(int newpos)
        {
            base.runtextpos = newpos;
        }

        private int TrackPeek()
        {
            return base.runtrack[base.runtrackpos - 1];
        }

        private int TrackPeek(int i)
        {
            return base.runtrack[(base.runtrackpos - i) - 1];
        }

        private void TrackPop()
        {
            base.runtrackpos++;
        }

        private void TrackPop(int framesize)
        {
            base.runtrackpos += framesize;
        }

        private int Trackpos()
        {
            return (base.runtrack.Length - base.runtrackpos);
        }

        private void TrackPush()
        {
            base.runtrack[--base.runtrackpos] = this.runcodepos;
        }

        private void TrackPush(int I1)
        {
            base.runtrack[--base.runtrackpos] = I1;
            base.runtrack[--base.runtrackpos] = this.runcodepos;
        }

        private void TrackPush(int I1, int I2)
        {
            base.runtrack[--base.runtrackpos] = I1;
            base.runtrack[--base.runtrackpos] = I2;
            base.runtrack[--base.runtrackpos] = this.runcodepos;
        }

        private void TrackPush(int I1, int I2, int I3)
        {
            base.runtrack[--base.runtrackpos] = I1;
            base.runtrack[--base.runtrackpos] = I2;
            base.runtrack[--base.runtrackpos] = I3;
            base.runtrack[--base.runtrackpos] = this.runcodepos;
        }

        private void TrackPush2(int I1)
        {
            base.runtrack[--base.runtrackpos] = I1;
            base.runtrack[--base.runtrackpos] = -this.runcodepos;
        }

        private void TrackPush2(int I1, int I2)
        {
            base.runtrack[--base.runtrackpos] = I1;
            base.runtrack[--base.runtrackpos] = I2;
            base.runtrack[--base.runtrackpos] = -this.runcodepos;
        }

        private void Trackto(int newpos)
        {
            base.runtrackpos = base.runtrack.Length - newpos;
        }
    }
}

