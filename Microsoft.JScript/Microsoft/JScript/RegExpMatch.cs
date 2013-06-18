namespace Microsoft.JScript
{
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;

    public sealed class RegExpMatch : ArrayObject
    {
        private bool hydrated;
        private Match match;
        private MatchCollection matches;
        private Regex regex;

        internal RegExpMatch(ArrayPrototype parent, Regex regex, Match match, string input) : base(parent, typeof(RegExpMatch))
        {
            this.hydrated = false;
            this.regex = regex;
            this.matches = null;
            this.match = match;
            base.SetMemberValue("input", input);
            base.SetMemberValue("index", match.Index);
            base.SetMemberValue("lastIndex", (match.Length == 0) ? (match.Index + 1) : (match.Index + match.Length));
            string[] groupNames = regex.GetGroupNames();
            int num = 0;
            for (int i = 1; i < groupNames.Length; i++)
            {
                string name = groupNames[i];
                int num3 = regex.GroupNumberFromName(name);
                if (name.Equals(num3.ToString(CultureInfo.InvariantCulture)))
                {
                    if (num3 > num)
                    {
                        num = num3;
                    }
                }
                else
                {
                    Group group = match.Groups[name];
                    base.SetMemberValue(name, group.Success ? group.ToString() : null);
                }
            }
            this.length = num + 1;
        }

        internal RegExpMatch(ArrayPrototype parent, Regex regex, MatchCollection matches, string input) : base(parent)
        {
            this.hydrated = false;
            this.length = matches.Count;
            this.regex = regex;
            this.matches = matches;
            this.match = null;
            Match match = matches[matches.Count - 1];
            base.SetMemberValue("input", input);
            base.SetMemberValue("index", match.Index);
            base.SetMemberValue("lastIndex", (match.Length == 0) ? (match.Index + 1) : (match.Index + match.Length));
        }

        internal override void Concat(ArrayObject source)
        {
            if (!this.hydrated)
            {
                this.Hydrate();
            }
            base.Concat(source);
        }

        internal override void Concat(object value)
        {
            if (!this.hydrated)
            {
                this.Hydrate();
            }
            base.Concat(value);
        }

        internal override bool DeleteValueAtIndex(uint index)
        {
            if (!this.hydrated)
            {
                this.Hydrate();
            }
            return base.DeleteValueAtIndex(index);
        }

        internal override object GetMemberValue(string name)
        {
            if (!this.hydrated)
            {
                long num = ArrayObject.Array_index_for(name);
                if (num >= 0L)
                {
                    return this.GetValueAtIndex((uint) num);
                }
            }
            return base.GetMemberValue(name);
        }

        internal override object GetValueAtIndex(uint index)
        {
            if (!this.hydrated)
            {
                if (this.matches != null)
                {
                    if (index < this.matches.Count)
                    {
                        return this.matches[(int) index].ToString();
                    }
                }
                else if (this.match != null)
                {
                    int num = this.regex.GroupNumberFromName(index.ToString(CultureInfo.InvariantCulture));
                    if (num >= 0)
                    {
                        Group group = this.match.Groups[num];
                        if (!group.Success)
                        {
                            return "";
                        }
                        return group.ToString();
                    }
                }
            }
            return base.GetValueAtIndex(index);
        }

        private void Hydrate()
        {
            if (this.matches != null)
            {
                int num = 0;
                int count = this.matches.Count;
                while (num < count)
                {
                    base.SetValueAtIndex((uint) num, this.matches[num].ToString());
                    num++;
                }
            }
            else if (this.match != null)
            {
                string[] groupNames = this.regex.GetGroupNames();
                int index = 1;
                int length = groupNames.Length;
                while (index < length)
                {
                    string name = groupNames[index];
                    int num5 = this.regex.GroupNumberFromName(name);
                    Group group = this.match.Groups[num5];
                    object obj2 = group.Success ? group.ToString() : "";
                    if (name.Equals(num5.ToString(CultureInfo.InvariantCulture)))
                    {
                        base.SetValueAtIndex((uint) num5, obj2);
                    }
                    index++;
                }
            }
            this.hydrated = true;
            this.regex = null;
            this.matches = null;
            this.match = null;
        }

        internal override void SetValueAtIndex(uint index, object value)
        {
            if (!this.hydrated)
            {
                this.Hydrate();
            }
            base.SetValueAtIndex(index, value);
        }

        internal override object Shift()
        {
            if (!this.hydrated)
            {
                this.Hydrate();
            }
            return base.Shift();
        }

        internal override void Sort(ScriptFunction compareFn)
        {
            if (!this.hydrated)
            {
                this.Hydrate();
            }
            base.Sort(compareFn);
        }

        internal override void Splice(uint start, uint deleteItems, object[] args, ArrayObject outArray, uint oldLength, uint newLength)
        {
            if (!this.hydrated)
            {
                this.Hydrate();
            }
            base.Splice(start, deleteItems, args, outArray, oldLength, newLength);
        }

        internal override void SwapValues(uint pi, uint qi)
        {
            if (!this.hydrated)
            {
                this.Hydrate();
            }
            base.SwapValues(pi, qi);
        }

        internal override ArrayObject Unshift(object[] args)
        {
            if (!this.hydrated)
            {
                this.Hydrate();
            }
            return base.Unshift(args);
        }
    }
}

