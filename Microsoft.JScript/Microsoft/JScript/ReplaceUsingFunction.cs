namespace Microsoft.JScript
{
    using System;
    using System.Text.RegularExpressions;

    internal class ReplaceUsingFunction : RegExpReplace
    {
        private int cArgs;
        private ScriptFunction function;
        private int[] groupNumbers;
        private string source;

        internal ReplaceUsingFunction(Regex regex, ScriptFunction function, string source)
        {
            this.function = function;
            this.cArgs = function.GetNumberOfFormalParameters();
            bool flag = (function is Closure) && ((Closure) function).func.hasArgumentsObject;
            this.groupNumbers = null;
            this.source = source;
            if ((this.cArgs > 1) || flag)
            {
                string[] groupNames = regex.GetGroupNames();
                int num = groupNames.Length - 1;
                if (flag)
                {
                    this.cArgs = num + 3;
                }
                if (num > 0)
                {
                    if (num > (this.cArgs - 1))
                    {
                        num = this.cArgs - 1;
                    }
                    this.groupNumbers = new int[num];
                    for (int i = 0; i < num; i++)
                    {
                        this.groupNumbers[i] = regex.GroupNumberFromName(groupNames[i + 1]);
                    }
                }
            }
        }

        internal override string Evaluate(Match match)
        {
            base.lastMatch = match;
            object[] args = new object[this.cArgs];
            if (this.cArgs > 0)
            {
                args[0] = match.ToString();
                if (this.cArgs > 1)
                {
                    int index = 1;
                    if (this.groupNumbers != null)
                    {
                        while (index <= this.groupNumbers.Length)
                        {
                            Group group = match.Groups[this.groupNumbers[index - 1]];
                            args[index] = group.Success ? group.ToString() : null;
                            index++;
                        }
                    }
                    if (index < this.cArgs)
                    {
                        args[index++] = match.Index;
                        if (index < this.cArgs)
                        {
                            args[index++] = this.source;
                            while (index < this.cArgs)
                            {
                                args[index] = null;
                                index++;
                            }
                        }
                    }
                }
            }
            object obj2 = this.function.Call(args, null);
            return match.Result((obj2 is Microsoft.JScript.Empty) ? "" : Microsoft.JScript.Convert.ToString(obj2));
        }
    }
}

