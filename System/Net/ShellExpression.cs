namespace System.Net
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct ShellExpression
    {
        private ShExpTokens[] pattern;
        private int[] match;
        internal ShellExpression(string pattern)
        {
            this.pattern = null;
            this.match = null;
            this.Parse(pattern);
        }

        internal bool IsMatch(string target)
        {
            int index = 0;
            int num2 = 0;
            bool flag = false;
            bool flag2 = false;
        Label_0008:
            while (flag)
            {
                switch (this.pattern[--index])
                {
                    case ShExpTokens.End:
                    case ShExpTokens.Start:
                        return flag2;

                    case ShExpTokens.AugmentedQuestion:
                    case ShExpTokens.Asterisk:
                        if (this.match[index] != this.match[index - 1])
                        {
                            num2 = --this.match[index++];
                            flag = false;
                        }
                        break;
                }
            }
            if (num2 > target.Length)
            {
                return flag2;
            }
            switch (this.pattern[index])
            {
                case ShExpTokens.End:
                    if (num2 != target.Length)
                    {
                        flag = true;
                        goto Label_0008;
                    }
                    return true;

                case ShExpTokens.Start:
                    if (num2 != 0)
                    {
                        return flag2;
                    }
                    this.match[index++] = 0;
                    goto Label_0008;

                case ShExpTokens.AugmentedQuestion:
                    if ((num2 != target.Length) && (target[num2] != '.'))
                    {
                        this.match[index++] = ++num2;
                    }
                    else
                    {
                        this.match[index++] = num2;
                    }
                    goto Label_0008;

                case ShExpTokens.AugmentedAsterisk:
                    if ((num2 != target.Length) && (target[num2] != '.'))
                    {
                        this.match[index++] = ++num2;
                    }
                    else
                    {
                        flag = true;
                    }
                    goto Label_0008;

                case ShExpTokens.AugmentedDot:
                    if (num2 != target.Length)
                    {
                        if (target[num2] == '.')
                        {
                            this.match[index++] = ++num2;
                        }
                        else
                        {
                            flag = true;
                        }
                    }
                    else
                    {
                        this.match[index++] = num2;
                    }
                    goto Label_0008;

                case ShExpTokens.Question:
                    if (num2 != target.Length)
                    {
                        this.match[index++] = ++num2;
                    }
                    else
                    {
                        flag = true;
                    }
                    goto Label_0008;

                case ShExpTokens.Asterisk:
                    this.match[index++] = num2 = target.Length;
                    goto Label_0008;
            }
            if ((num2 < target.Length) && (this.pattern[index] == ((ShExpTokens) char.ToLowerInvariant(target[num2]))))
            {
                this.match[index++] = ++num2;
            }
            else
            {
                flag = true;
            }
            goto Label_0008;
        }

        private void Parse(string patString)
        {
            this.pattern = new ShExpTokens[patString.Length + 2];
            this.match = null;
            int num = 0;
            this.pattern[num++] = ShExpTokens.Start;
            for (int i = 0; i < patString.Length; i++)
            {
                switch (patString[i])
                {
                    case '*':
                    {
                        this.pattern[num++] = ShExpTokens.Asterisk;
                        continue;
                    }
                    case '?':
                    {
                        this.pattern[num++] = ShExpTokens.Question;
                        continue;
                    }
                    case '^':
                        if (i >= (patString.Length - 1))
                        {
                            this.pattern = null;
                            if (Logging.On)
                            {
                                Logging.PrintWarning(Logging.Web, SR.GetString("net_log_shell_expression_pattern_format_warning", new object[] { patString }));
                            }
                            throw new FormatException(SR.GetString("net_format_shexp", new object[] { patString }));
                        }
                        i++;
                        break;

                    default:
                        goto Label_0170;
                }
                switch (patString[i])
                {
                    case '*':
                    {
                        this.pattern[num++] = ShExpTokens.AugmentedAsterisk;
                        continue;
                    }
                    case '.':
                    {
                        this.pattern[num++] = ShExpTokens.AugmentedDot;
                        continue;
                    }
                    case '?':
                    {
                        this.pattern[num++] = ShExpTokens.AugmentedQuestion;
                        continue;
                    }
                    default:
                        this.pattern = null;
                        if (Logging.On)
                        {
                            Logging.PrintWarning(Logging.Web, SR.GetString("net_log_shell_expression_pattern_format_warning", new object[] { patString }));
                        }
                        throw new FormatException(SR.GetString("net_format_shexp", new object[] { patString }));
                }
            Label_0170:
                this.pattern[num++] = (ShExpTokens) char.ToLowerInvariant(patString[i]);
            }
            this.pattern[num++] = ShExpTokens.End;
            this.match = new int[num];
        }
        private enum ShExpTokens
        {
            Asterisk = -1,
            AugmentedAsterisk = -4,
            AugmentedDot = -3,
            AugmentedQuestion = -5,
            End = -7,
            Question = -2,
            Start = -6
        }
    }
}

