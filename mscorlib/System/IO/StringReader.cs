namespace System.IO
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class StringReader : TextReader
    {
        private int _length;
        private int _pos;
        private string _s;

        [SecuritySafeCritical]
        public StringReader(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            this._s = s;
            this._length = (s == null) ? 0 : s.Length;
        }

        public override void Close()
        {
            this.Dispose(true);
        }

        protected override void Dispose(bool disposing)
        {
            this._s = null;
            this._pos = 0;
            this._length = 0;
            base.Dispose(disposing);
        }

        [SecuritySafeCritical]
        public override int Peek()
        {
            if (this._s == null)
            {
                __Error.ReaderClosed();
            }
            if (this._pos == this._length)
            {
                return -1;
            }
            return this._s[this._pos];
        }

        [SecuritySafeCritical]
        public override int Read()
        {
            if (this._s == null)
            {
                __Error.ReaderClosed();
            }
            if (this._pos == this._length)
            {
                return -1;
            }
            return this._s[this._pos++];
        }

        public override int Read([In, Out] char[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer", Environment.GetResourceString("ArgumentNull_Buffer"));
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((buffer.Length - index) < count)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
            }
            if (this._s == null)
            {
                __Error.ReaderClosed();
            }
            int num = this._length - this._pos;
            if (num > 0)
            {
                if (num > count)
                {
                    num = count;
                }
                this._s.CopyTo(this._pos, buffer, index, num);
                this._pos += num;
            }
            return num;
        }

        [SecuritySafeCritical]
        public override string ReadLine()
        {
            if (this._s == null)
            {
                __Error.ReaderClosed();
            }
            int num = this._pos;
            while (num < this._length)
            {
                char ch = this._s[num];
                switch (ch)
                {
                    case '\r':
                    case '\n':
                    {
                        string str = this._s.Substring(this._pos, num - this._pos);
                        this._pos = num + 1;
                        if (((ch == '\r') && (this._pos < this._length)) && (this._s[this._pos] == '\n'))
                        {
                            this._pos++;
                        }
                        return str;
                    }
                }
                num++;
            }
            if (num > this._pos)
            {
                string str2 = this._s.Substring(this._pos, num - this._pos);
                this._pos = num;
                return str2;
            }
            return null;
        }

        public override string ReadToEnd()
        {
            string str;
            if (this._s == null)
            {
                __Error.ReaderClosed();
            }
            if (this._pos == 0)
            {
                str = this._s;
            }
            else
            {
                str = this._s.Substring(this._pos, this._length - this._pos);
            }
            this._pos = this._length;
            return str;
        }
    }
}

