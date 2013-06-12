namespace System.Net.Mime
{
    using System;
    using System.Runtime.CompilerServices;

    internal abstract class WriteStateInfoBase
    {
        protected byte[] _footer;
        protected byte[] _header;
        protected int _maxLineLength;
        protected int _mimeHeaderLength;
        protected byte[] buffer;
        protected const int defaultBufferSize = 0x400;

        internal WriteStateInfoBase()
        {
            this.buffer = new byte[0x400];
            this._header = new byte[0];
            this._footer = new byte[0];
            this._maxLineLength = EncodedStreamFactory.DefaultMaxLineLength;
            this._mimeHeaderLength = 0;
        }

        internal WriteStateInfoBase(int bufferSize, byte[] header, byte[] footer, int maxLineLength)
        {
            this.buffer = new byte[bufferSize];
            this._header = header;
            this._footer = footer;
            this._maxLineLength = maxLineLength;
            this._mimeHeaderLength = 0;
        }

        internal void AppendFooter()
        {
            if (this.Footer != null)
            {
                this.Footer.CopyTo(this.buffer, this.Length);
                this.CurrentLineLength += this.FooterLength;
                this.Length += this.FooterLength;
            }
        }

        internal void AppendHeader()
        {
            if (this.Header != null)
            {
                this.Header.CopyTo(this.buffer, this.Length);
                this.CurrentLineLength += this.HeaderLength;
                this.Length += this.HeaderLength;
            }
        }

        internal void ResizeBuffer()
        {
            int num = this.buffer.Length * 2;
            byte[] array = new byte[num];
            this.buffer.CopyTo(array, 0);
            this.buffer = array;
        }

        internal byte[] Buffer
        {
            get
            {
                return this.buffer;
            }
        }

        internal int CurrentLineLength { get; set; }

        internal byte[] Footer
        {
            get
            {
                return this._footer;
            }
        }

        internal int FooterLength
        {
            get
            {
                return this._footer.Length;
            }
        }

        internal byte[] Header
        {
            get
            {
                return this._header;
            }
        }

        internal int HeaderLength
        {
            get
            {
                return this._header.Length;
            }
        }

        internal int Length { get; set; }

        internal int MaxLineLength
        {
            get
            {
                return this._maxLineLength;
            }
        }

        internal int MimeHeaderLength
        {
            get
            {
                return this._mimeHeaderLength;
            }
            set
            {
                this._mimeHeaderLength = value;
            }
        }
    }
}

