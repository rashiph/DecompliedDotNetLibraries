namespace System.Data
{
    using System;
    using System.Data.Common;
    using System.Data.SqlTypes;

    internal sealed class LikeNode : BinaryNode
    {
        private int kind;
        internal const int match_all = 5;
        internal const int match_exact = 4;
        internal const int match_left = 1;
        internal const int match_middle = 3;
        internal const int match_right = 2;
        private string pattern;

        internal LikeNode(DataTable table, int op, ExpressionNode left, ExpressionNode right) : base(table, op, left, right)
        {
        }

        internal string AnalyzePattern(string pat)
        {
            int length = pat.Length;
            char[] destination = new char[length + 1];
            pat.CopyTo(0, destination, 0, length);
            destination[length] = '\0';
            string str = null;
            char[] chArray2 = new char[length + 1];
            int num3 = 0;
            int num4 = 0;
            int index = 0;
            while (index < length)
            {
                if ((destination[index] == '*') || (destination[index] == '%'))
                {
                    while (((destination[index] == '*') || (destination[index] == '%')) && (index < length))
                    {
                        index++;
                    }
                    if (((index < length) && (num3 > 0)) || (num4 >= 2))
                    {
                        throw ExprException.InvalidPattern(pat);
                    }
                    num4++;
                }
                else if (destination[index] == '[')
                {
                    index++;
                    if (index >= length)
                    {
                        throw ExprException.InvalidPattern(pat);
                    }
                    chArray2[num3++] = destination[index++];
                    if (index >= length)
                    {
                        throw ExprException.InvalidPattern(pat);
                    }
                    if (destination[index] != ']')
                    {
                        throw ExprException.InvalidPattern(pat);
                    }
                    index++;
                }
                else
                {
                    chArray2[num3++] = destination[index];
                    index++;
                }
            }
            str = new string(chArray2, 0, num3);
            if (num4 == 0)
            {
                this.kind = 4;
                return str;
            }
            if (num3 > 0)
            {
                if ((destination[0] == '*') || (destination[0] == '%'))
                {
                    if ((destination[length - 1] == '*') || (destination[length - 1] == '%'))
                    {
                        this.kind = 3;
                        return str;
                    }
                    this.kind = 2;
                    return str;
                }
                this.kind = 1;
                return str;
            }
            this.kind = 5;
            return str;
        }

        internal override object Eval(DataRow row, DataRowVersion version)
        {
            object obj2 = base.left.Eval(row, version);
            if ((obj2 != DBNull.Value) && (!base.left.IsSqlColumn || !DataStorage.IsObjectSqlNull(obj2)))
            {
                string pattern;
                string str3;
                if (this.pattern == null)
                {
                    object obj3 = base.right.Eval(row, version);
                    if (!(obj3 is string) && !(obj3 is SqlString))
                    {
                        base.SetTypeMismatchError(base.op, obj2.GetType(), obj3.GetType());
                    }
                    if ((obj3 == DBNull.Value) || DataStorage.IsObjectSqlNull(obj3))
                    {
                        return DBNull.Value;
                    }
                    string pat = (string) SqlConvert.ChangeType2(obj3, StorageType.String, typeof(string), base.FormatProvider);
                    pattern = this.AnalyzePattern(pat);
                    if (base.right.IsConstant())
                    {
                        this.pattern = pattern;
                    }
                }
                else
                {
                    pattern = this.pattern;
                }
                if (!(obj2 is string) && !(obj2 is SqlString))
                {
                    base.SetTypeMismatchError(base.op, obj2.GetType(), typeof(string));
                }
                char[] trimChars = new char[] { ' ', '　' };
                if (obj2 is SqlString)
                {
                    SqlString str6 = (SqlString) obj2;
                    str3 = str6.Value;
                }
                else
                {
                    str3 = (string) obj2;
                }
                string str2 = str3.TrimEnd(trimChars);
                switch (this.kind)
                {
                    case 1:
                        return (0 == base.table.IndexOf(str2, pattern));

                    case 2:
                    {
                        string str4 = pattern.TrimEnd(trimChars);
                        return base.table.IsSuffix(str2, str4);
                    }
                    case 3:
                        return (0 <= base.table.IndexOf(str2, pattern));

                    case 4:
                        return (0 == base.table.Compare(str2, pattern));

                    case 5:
                        return true;
                }
            }
            return DBNull.Value;
        }
    }
}

