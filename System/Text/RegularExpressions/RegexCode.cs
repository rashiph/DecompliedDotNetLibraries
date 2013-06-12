namespace System.Text.RegularExpressions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;

    internal sealed class RegexCode
    {
        internal int _anchors;
        internal RegexBoyerMoore _bmPrefix;
        internal Hashtable _caps;
        internal int _capsize;
        internal int[] _codes;
        internal RegexPrefix _fcPrefix;
        internal bool _rightToLeft;
        internal string[] _strings;
        internal int _trackcount;
        internal const int Back = 0x80;
        internal const int Back2 = 0x100;
        internal const int Backjump = 0x23;
        internal const int Beginning = 0x12;
        internal const int Bol = 14;
        internal const int Boundary = 0x10;
        internal const int Branchcount = 0x1c;
        internal const int Branchmark = 0x18;
        internal const int Capturemark = 0x20;
        internal const int Ci = 0x200;
        internal const int ECMABoundary = 0x29;
        internal const int End = 0x15;
        internal const int EndZ = 20;
        internal const int Eol = 15;
        internal const int Forejump = 0x24;
        internal const int Getmark = 0x21;
        internal const int Goto = 0x26;
        internal const int Lazybranch = 0x17;
        internal const int Lazybranchcount = 0x1d;
        internal const int Lazybranchmark = 0x19;
        internal const int Mask = 0x3f;
        internal const int Multi = 12;
        internal const int Nonboundary = 0x11;
        internal const int NonECMABoundary = 0x2a;
        internal const int Nothing = 0x16;
        internal const int Notone = 10;
        internal const int Notonelazy = 7;
        internal const int Notoneloop = 4;
        internal const int Notonerep = 1;
        internal const int Nullcount = 0x1a;
        internal const int Nullmark = 30;
        internal const int One = 9;
        internal const int Onelazy = 6;
        internal const int Oneloop = 3;
        internal const int Onerep = 0;
        internal const int Prune = 0x27;
        internal const int Ref = 13;
        internal const int Rtl = 0x40;
        internal const int Set = 11;
        internal const int Setcount = 0x1b;
        internal const int Setjump = 0x22;
        internal const int Setlazy = 8;
        internal const int Setloop = 5;
        internal const int Setmark = 0x1f;
        internal const int Setrep = 2;
        internal const int Start = 0x13;
        internal const int Stop = 40;
        internal const int Testref = 0x25;

        internal RegexCode(int[] codes, List<string> stringlist, int trackcount, Hashtable caps, int capsize, RegexBoyerMoore bmPrefix, RegexPrefix fcPrefix, int anchors, bool rightToLeft)
        {
            this._codes = codes;
            this._strings = new string[stringlist.Count];
            this._trackcount = trackcount;
            this._caps = caps;
            this._capsize = capsize;
            this._bmPrefix = bmPrefix;
            this._fcPrefix = fcPrefix;
            this._anchors = anchors;
            this._rightToLeft = rightToLeft;
            stringlist.CopyTo(0, this._strings, 0, stringlist.Count);
        }

        internal static ArgumentException MakeException(string message)
        {
            return new ArgumentException(message);
        }

        internal static bool OpcodeBacktracks(int Op)
        {
            Op &= 0x3f;
            switch (Op)
            {
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 0x17:
                case 0x18:
                case 0x19:
                case 0x1a:
                case 0x1b:
                case 0x1c:
                case 0x1d:
                case 0x1f:
                case 0x20:
                case 0x21:
                case 0x22:
                case 0x23:
                case 0x24:
                case 0x26:
                    return true;
            }
            return false;
        }

        internal static int OpcodeSize(int Opcode)
        {
            Opcode &= 0x3f;
            switch (Opcode)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 0x1c:
                case 0x1d:
                case 0x20:
                    return 3;

                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                case 0x17:
                case 0x18:
                case 0x19:
                case 0x1a:
                case 0x1b:
                case 0x25:
                case 0x26:
                case 0x27:
                    return 2;

                case 14:
                case 15:
                case 0x10:
                case 0x11:
                case 0x12:
                case 0x13:
                case 20:
                case 0x15:
                case 0x16:
                case 30:
                case 0x1f:
                case 0x21:
                case 0x22:
                case 0x23:
                case 0x24:
                case 40:
                case 0x29:
                case 0x2a:
                    return 1;
            }
            throw MakeException(SR.GetString("UnexpectedOpcode", new object[] { Opcode.ToString(CultureInfo.CurrentCulture) }));
        }
    }
}

