namespace System.Text.RegularExpressions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;

    internal sealed class RegexReplacement
    {
        internal string _rep;
        internal List<int> _rules;
        internal List<string> _strings;
        internal const int LastGroup = -3;
        internal const int LeftPortion = -1;
        internal const int RightPortion = -2;
        internal const int Specials = 4;
        internal const int WholeString = -4;

        internal RegexReplacement(string rep, RegexNode concat, Hashtable _caps)
        {
            this._rep = rep;
            if (concat.Type() != 0x19)
            {
                throw new ArgumentException(SR.GetString("ReplacementError"));
            }
            StringBuilder builder = new StringBuilder();
            List<string> list = new List<string>();
            List<int> list2 = new List<int>();
            for (int i = 0; i < concat.ChildCount(); i++)
            {
                RegexNode node = concat.Child(i);
                switch (node.Type())
                {
                    case 9:
                    {
                        builder.Append(node._ch);
                        continue;
                    }
                    case 12:
                    {
                        builder.Append(node._str);
                        continue;
                    }
                    case 13:
                    {
                        if (builder.Length > 0)
                        {
                            list2.Add(list.Count);
                            list.Add(builder.ToString());
                            builder.Length = 0;
                        }
                        int num = node._m;
                        if ((_caps != null) && (num >= 0))
                        {
                            num = (int) _caps[num];
                        }
                        list2.Add(-5 - num);
                        continue;
                    }
                }
                throw new ArgumentException(SR.GetString("ReplacementError"));
            }
            if (builder.Length > 0)
            {
                list2.Add(list.Count);
                list.Add(builder.ToString());
            }
            this._strings = list;
            this._rules = list2;
        }

        internal string Replace(Regex regex, string input, int count, int startat)
        {
            StringBuilder builder;
            if (count < -1)
            {
                throw new ArgumentOutOfRangeException("count", SR.GetString("CountTooSmall"));
            }
            if ((startat < 0) || (startat > input.Length))
            {
                throw new ArgumentOutOfRangeException("startat", SR.GetString("BeginIndexNotNegative"));
            }
            if (count == 0)
            {
                return input;
            }
            Match match = regex.Match(input, startat);
            if (!match.Success)
            {
                return input;
            }
            if (regex.RightToLeft)
            {
                List<string> al = new List<string>();
                int length = input.Length;
            Label_00DD:
                if ((match.Index + match.Length) != length)
                {
                    al.Add(input.Substring(match.Index + match.Length, (length - match.Index) - match.Length));
                }
                length = match.Index;
                this.ReplacementImplRTL(al, match);
                if (--count != 0)
                {
                    match = match.NextMatch();
                    if (match.Success)
                    {
                        goto Label_00DD;
                    }
                }
                builder = new StringBuilder();
                if (length > 0)
                {
                    builder.Append(input, 0, length);
                }
                for (int i = al.Count - 1; i >= 0; i--)
                {
                    builder.Append(al[i]);
                }
                goto Label_017A;
            }
            builder = new StringBuilder();
            int startIndex = 0;
        Label_0066:
            if (match.Index != startIndex)
            {
                builder.Append(input, startIndex, match.Index - startIndex);
            }
            startIndex = match.Index + match.Length;
            this.ReplacementImpl(builder, match);
            if (--count != 0)
            {
                match = match.NextMatch();
                if (match.Success)
                {
                    goto Label_0066;
                }
            }
            if (startIndex < input.Length)
            {
                builder.Append(input, startIndex, input.Length - startIndex);
            }
        Label_017A:
            return builder.ToString();
        }

        internal static string Replace(MatchEvaluator evaluator, Regex regex, string input, int count, int startat)
        {
            StringBuilder builder;
            if (evaluator == null)
            {
                throw new ArgumentNullException("evaluator");
            }
            if (count < -1)
            {
                throw new ArgumentOutOfRangeException("count", SR.GetString("CountTooSmall"));
            }
            if ((startat < 0) || (startat > input.Length))
            {
                throw new ArgumentOutOfRangeException("startat", SR.GetString("BeginIndexNotNegative"));
            }
            if (count == 0)
            {
                return input;
            }
            Match match = regex.Match(input, startat);
            if (!match.Success)
            {
                return input;
            }
            if (regex.RightToLeft)
            {
                List<string> list = new List<string>();
                int length = input.Length;
            Label_00F1:
                if ((match.Index + match.Length) != length)
                {
                    list.Add(input.Substring(match.Index + match.Length, (length - match.Index) - match.Length));
                }
                length = match.Index;
                list.Add(evaluator(match));
                if (--count != 0)
                {
                    match = match.NextMatch();
                    if (match.Success)
                    {
                        goto Label_00F1;
                    }
                }
                builder = new StringBuilder();
                if (length > 0)
                {
                    builder.Append(input, 0, length);
                }
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    builder.Append(list[i]);
                }
                goto Label_0193;
            }
            builder = new StringBuilder();
            int startIndex = 0;
        Label_0074:
            if (match.Index != startIndex)
            {
                builder.Append(input, startIndex, match.Index - startIndex);
            }
            startIndex = match.Index + match.Length;
            builder.Append(evaluator(match));
            if (--count != 0)
            {
                match = match.NextMatch();
                if (match.Success)
                {
                    goto Label_0074;
                }
            }
            if (startIndex < input.Length)
            {
                builder.Append(input, startIndex, input.Length - startIndex);
            }
        Label_0193:
            return builder.ToString();
        }

        internal string Replacement(Match match)
        {
            StringBuilder sb = new StringBuilder();
            this.ReplacementImpl(sb, match);
            return sb.ToString();
        }

        private void ReplacementImpl(StringBuilder sb, Match match)
        {
            for (int i = 0; i < this._rules.Count; i++)
            {
                int num2 = this._rules[i];
                if (num2 >= 0)
                {
                    sb.Append(this._strings[num2]);
                }
                else if (num2 < -4)
                {
                    sb.Append(match.GroupToStringImpl(-5 - num2));
                }
                else
                {
                    switch ((-5 - num2))
                    {
                        case -4:
                            sb.Append(match.GetOriginalString());
                            break;

                        case -3:
                            sb.Append(match.LastGroupToStringImpl());
                            break;

                        case -2:
                            sb.Append(match.GetRightSubstring());
                            break;

                        case -1:
                            sb.Append(match.GetLeftSubstring());
                            break;
                    }
                }
            }
        }

        private void ReplacementImplRTL(List<string> al, Match match)
        {
            for (int i = this._rules.Count - 1; i >= 0; i--)
            {
                int num2 = this._rules[i];
                if (num2 >= 0)
                {
                    al.Add(this._strings[num2]);
                }
                else if (num2 < -4)
                {
                    al.Add(match.GroupToStringImpl(-5 - num2));
                }
                else
                {
                    switch ((-5 - num2))
                    {
                        case -4:
                            al.Add(match.GetOriginalString());
                            break;

                        case -3:
                            al.Add(match.LastGroupToStringImpl());
                            break;

                        case -2:
                            al.Add(match.GetRightSubstring());
                            break;

                        case -1:
                            al.Add(match.GetLeftSubstring());
                            break;
                    }
                }
            }
        }

        internal static string[] Split(Regex regex, string input, int count, int startat)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", SR.GetString("CountTooSmall"));
            }
            if ((startat < 0) || (startat > input.Length))
            {
                throw new ArgumentOutOfRangeException("startat", SR.GetString("BeginIndexNotNegative"));
            }
            if (count == 1)
            {
                return new string[] { input };
            }
            count--;
            Match match = regex.Match(input, startat);
            if (!match.Success)
            {
                return new string[] { input };
            }
            List<string> list = new List<string>();
            if (regex.RightToLeft)
            {
                int length = input.Length;
            Label_011A:
                list.Add(input.Substring(match.Index + match.Length, (length - match.Index) - match.Length));
                length = match.Index;
                for (int j = 1; j < match.Groups.Count; j++)
                {
                    if (match.IsMatched(j))
                    {
                        list.Add(match.Groups[j].ToString());
                    }
                }
                if (--count != 0)
                {
                    match = match.NextMatch();
                    if (match.Success)
                    {
                        goto Label_011A;
                    }
                }
                list.Add(input.Substring(0, length));
                list.Reverse(0, list.Count);
                goto Label_01BD;
            }
            int startIndex = 0;
        Label_0082:
            list.Add(input.Substring(startIndex, match.Index - startIndex));
            startIndex = match.Index + match.Length;
            for (int i = 1; i < match.Groups.Count; i++)
            {
                if (match.IsMatched(i))
                {
                    list.Add(match.Groups[i].ToString());
                }
            }
            if (--count != 0)
            {
                match = match.NextMatch();
                if (match.Success)
                {
                    goto Label_0082;
                }
            }
            list.Add(input.Substring(startIndex, input.Length - startIndex));
        Label_01BD:
            return list.ToArray();
        }

        internal string Pattern
        {
            get
            {
                return this._rep;
            }
        }
    }
}

