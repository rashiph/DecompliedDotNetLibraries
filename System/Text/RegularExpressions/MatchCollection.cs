namespace System.Text.RegularExpressions
{
    using System;
    using System.Collections;
    using System.Reflection;

    [Serializable]
    public class MatchCollection : ICollection, IEnumerable
    {
        internal int _beginning;
        internal bool _done;
        internal string _input;
        internal int _length;
        internal ArrayList _matches;
        internal int _prevlen;
        internal Regex _regex;
        internal int _startat;
        private static int infinite = 0x7fffffff;

        internal MatchCollection(Regex regex, string input, int beginning, int length, int startat)
        {
            if ((startat < 0) || (startat > input.Length))
            {
                throw new ArgumentOutOfRangeException("startat", SR.GetString("BeginIndexNotNegative"));
            }
            this._regex = regex;
            this._input = input;
            this._beginning = beginning;
            this._length = length;
            this._startat = startat;
            this._prevlen = -1;
            this._matches = new ArrayList();
            this._done = false;
        }

        public void CopyTo(Array array, int arrayIndex)
        {
            if ((array != null) && (array.Rank != 1))
            {
                throw new ArgumentException(SR.GetString("Arg_RankMultiDimNotSupported"));
            }
            int count = this.Count;
            try
            {
                this._matches.CopyTo(array, arrayIndex);
            }
            catch (ArrayTypeMismatchException)
            {
                throw new ArgumentException(SR.GetString("Arg_InvalidArrayType"));
            }
        }

        public IEnumerator GetEnumerator()
        {
            return new MatchEnumerator(this);
        }

        internal Match GetMatch(int i)
        {
            Match match;
            if (i < 0)
            {
                return null;
            }
            if (this._matches.Count > i)
            {
                return (Match) this._matches[i];
            }
            if (this._done)
            {
                return null;
            }
            do
            {
                match = this._regex.Run(false, this._prevlen, this._input, this._beginning, this._length, this._startat);
                if (!match.Success)
                {
                    this._done = true;
                    return null;
                }
                this._matches.Add(match);
                this._prevlen = match._length;
                this._startat = match._textpos;
            }
            while (this._matches.Count <= i);
            return match;
        }

        public int Count
        {
            get
            {
                if (!this._done)
                {
                    this.GetMatch(infinite);
                }
                return this._matches.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public virtual Match this[int i]
        {
            get
            {
                Match match = this.GetMatch(i);
                if (match == null)
                {
                    throw new ArgumentOutOfRangeException("i");
                }
                return match;
            }
        }

        public object SyncRoot
        {
            get
            {
                return this;
            }
        }
    }
}

