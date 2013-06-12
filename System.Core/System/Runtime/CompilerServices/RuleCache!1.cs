namespace System.Runtime.CompilerServices
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Dynamic.Utils;

    [EditorBrowsable(EditorBrowsableState.Never), DebuggerStepThrough]
    public class RuleCache<T> where T: class
    {
        private T[] _rules;
        private readonly object cacheLock;
        private const int InsertPosition = 0x40;
        private const int MaxRules = 0x80;

        internal RuleCache()
        {
            this._rules = new T[0];
            this.cacheLock = new object();
        }

        private static T[] AddOrInsert(T[] rules, T item)
        {
            T[] localArray;
            if (rules.Length < 0x40)
            {
                return rules.AddLast<T>(item);
            }
            int num = rules.Length + 1;
            if (num > 0x80)
            {
                num = 0x80;
                localArray = rules;
            }
            else
            {
                localArray = new T[num];
            }
            Array.Copy(rules, 0, localArray, 0, 0x40);
            localArray[0x40] = item;
            Array.Copy(rules, 0x40, localArray, 0x41, (num - 0x40) - 1);
            return localArray;
        }

        internal void AddRule(T newRule)
        {
            lock (this.cacheLock)
            {
                this._rules = RuleCache<T>.AddOrInsert(this._rules, newRule);
            }
        }

        internal T[] GetRules()
        {
            return this._rules;
        }

        internal void MoveRule(T rule, int i)
        {
            lock (this.cacheLock)
            {
                int num = this._rules.Length - i;
                if (num > 8)
                {
                    num = 8;
                }
                int index = -1;
                int num3 = Math.Min(this._rules.Length, i + num);
                for (int j = i; j < num3; j++)
                {
                    if (this._rules[j] == rule)
                    {
                        index = j;
                        break;
                    }
                }
                if (index >= 0)
                {
                    T local = this._rules[index];
                    this._rules[index] = this._rules[index - 1];
                    this._rules[index - 1] = this._rules[index - 2];
                    this._rules[index - 2] = local;
                }
            }
        }

        internal void ReplaceRule(T oldRule, T newRule)
        {
            lock (this.cacheLock)
            {
                int index = Array.IndexOf<T>(this._rules, oldRule);
                if (index >= 0)
                {
                    this._rules[index] = newRule;
                }
                else
                {
                    this._rules = RuleCache<T>.AddOrInsert(this._rules, newRule);
                }
            }
        }
    }
}

