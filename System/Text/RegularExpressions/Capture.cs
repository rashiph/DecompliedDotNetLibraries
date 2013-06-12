namespace System.Text.RegularExpressions
{
    using System;

    [Serializable]
    public class Capture
    {
        internal int _index;
        internal int _length;
        internal string _text;

        internal Capture(string text, int i, int l)
        {
            this._text = text;
            this._index = i;
            this._length = l;
        }

        internal string GetLeftSubstring()
        {
            return this._text.Substring(0, this._index);
        }

        internal string GetOriginalString()
        {
            return this._text;
        }

        internal string GetRightSubstring()
        {
            return this._text.Substring(this._index + this._length, (this._text.Length - this._index) - this._length);
        }

        public override string ToString()
        {
            return this.Value;
        }

        public int Index
        {
            get
            {
                return this._index;
            }
        }

        public int Length
        {
            get
            {
                return this._length;
            }
        }

        public string Value
        {
            get
            {
                return this._text.Substring(this._index, this._length);
            }
        }
    }
}

