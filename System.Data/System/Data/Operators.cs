namespace System.Data
{
    using System;

    internal sealed class Operators
    {
        internal const int And = 0x1a;
        internal const int Between = 6;
        internal const int BetweenAnd = 4;
        internal const int BitwiseAnd = 0x16;
        internal const int BitwiseNot = 0x19;
        internal const int BitwiseOr = 0x17;
        internal const int BitwiseXor = 0x18;
        internal const int Date = 0x23;
        internal const int Divide = 0x12;
        internal const int Dot = 0x1f;
        internal const int EqualTo = 7;
        internal const int False = 0x22;
        internal const int GenGUID = 0x25;
        internal const int GenUniqueId = 0x24;
        internal const int GreaterOrEqual = 10;
        internal const int GreaterThen = 8;
        internal const int GUID = 0x26;
        internal const int Iff = 0x1d;
        internal const int In = 5;
        internal const int Is = 13;
        internal const int IsNot = 0x27;
        internal const int LessOrEqual = 11;
        internal const int LessThen = 9;
        internal const int Like = 14;
        private static readonly string[] Looks = new string[] { 
            "", "-", "+", "Not", "BetweenAnd", "In", "Between", "=", ">", "<", ">=", "<=", "<>", "Is", "Like", "+", 
            "-", "*", "/", @"\", "Mod", "**", "&", "|", "^", "~", "And", "Or", "Proc", "Iff", ".", ".", 
            "Null", "True", "False", "Date", "GenUniqueId()", "GenGuid()", "Guid {..}", "Is Not"
         };
        internal const int Minus = 0x10;
        internal const int Modulo = 20;
        internal const int Multiply = 0x11;
        internal const int Negative = 1;
        internal const int Noop = 0;
        internal const int Not = 3;
        internal const int NotEqual = 12;
        internal const int Null = 0x20;
        internal const int Or = 0x1b;
        internal const int Plus = 15;
        internal const int priAnd = 8;
        internal const int priBetweenAnd = 12;
        internal const int priBetweenInLike = 11;
        internal const int priConcat = 14;
        internal const int priContains = 15;
        internal const int priDot = 0x17;
        internal const int priEqv = 5;
        internal const int priExp = 0x15;
        internal const int priIDiv = 0x12;
        internal const int priImp = 4;
        internal const int priIs = 10;
        internal const int priLow = 3;
        internal const int priMax = 0x18;
        internal const int priMod = 0x11;
        internal const int priMulDiv = 0x13;
        internal const int priNeg = 20;
        internal const int priNot = 9;
        internal const int priOr = 7;
        private static readonly int[] priority = new int[] { 
            0, 20, 20, 9, 12, 11, 11, 13, 13, 13, 13, 13, 13, 10, 11, 0x10, 
            0x10, 0x13, 0x13, 0x12, 0x11, 0x15, 8, 7, 6, 9, 8, 7, 2, 0x16, 0x17, 0x17, 
            0x18, 0x18, 0x18, 0x18, 0x18, 0x18, 0x18, 0x18, 0x18, 0x18, 0x18, 0x18
         };
        internal const int priParen = 2;
        internal const int priPlusMinus = 0x10;
        internal const int priProc = 0x16;
        internal const int priRelOp = 13;
        internal const int priStart = 0;
        internal const int priSubstr = 1;
        internal const int priXor = 6;
        internal const int Proc = 0x1c;
        internal const int Qual = 30;
        internal const int True = 0x21;
        internal const int UnaryPlus = 2;

        private Operators()
        {
        }

        internal static bool IsArithmetical(int op)
        {
            if (((op != 15) && (op != 0x10)) && ((op != 0x11) && (op != 0x12)))
            {
                return (op == 20);
            }
            return true;
        }

        internal static bool IsLogical(int op)
        {
            if (((op != 0x1a) && (op != 0x1b)) && ((op != 3) && (op != 13)))
            {
                return (op == 0x27);
            }
            return true;
        }

        internal static bool IsRelational(int op)
        {
            return ((7 <= op) && (op <= 12));
        }

        internal static int Priority(int op)
        {
            if (op > priority.Length)
            {
                return 0x18;
            }
            return priority[op];
        }

        internal static string ToString(int op)
        {
            if (op <= Looks.Length)
            {
                return Looks[op];
            }
            return "Unknown op";
        }
    }
}

