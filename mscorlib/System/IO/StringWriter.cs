namespace System.IO
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Text;

    [Serializable, ComVisible(true)]
    public class StringWriter : TextWriter
    {
        private bool _isOpen;
        private StringBuilder _sb;
        private static UnicodeEncoding m_encoding;

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public StringWriter() : this(new StringBuilder(), CultureInfo.CurrentCulture)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public StringWriter(IFormatProvider formatProvider) : this(new StringBuilder(), formatProvider)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public StringWriter(StringBuilder sb) : this(sb, CultureInfo.CurrentCulture)
        {
        }

        public StringWriter(StringBuilder sb, IFormatProvider formatProvider) : base(formatProvider)
        {
            if (sb == null)
            {
                throw new ArgumentNullException("sb", Environment.GetResourceString("ArgumentNull_Buffer"));
            }
            this._sb = sb;
            this._isOpen = true;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public override void Close()
        {
            this.Dispose(true);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        protected override void Dispose(bool disposing)
        {
            this._isOpen = false;
            base.Dispose(disposing);
        }

        public virtual StringBuilder GetStringBuilder()
        {
            return this._sb;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public override string ToString()
        {
            return this._sb.ToString();
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public override void Write(char value)
        {
            if (!this._isOpen)
            {
                __Error.WriterClosed();
            }
            this._sb.Append(value);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public override void Write(string value)
        {
            if (!this._isOpen)
            {
                __Error.WriterClosed();
            }
            if (value != null)
            {
                this._sb.Append(value);
            }
        }

        public override void Write(char[] buffer, int index, int count)
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
            if (!this._isOpen)
            {
                __Error.WriterClosed();
            }
            this._sb.Append(buffer, index, count);
        }

        public override System.Text.Encoding Encoding
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                if (m_encoding == null)
                {
                    m_encoding = new UnicodeEncoding(false, false);
                }
                return m_encoding;
            }
        }
    }
}

