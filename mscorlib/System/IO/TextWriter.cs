namespace System.IO
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;

    [Serializable, ComVisible(true)]
    public abstract class TextWriter : MarshalByRefObject, IDisposable
    {
        protected char[] CoreNewLine;
        private const string InitialNewLine = "\r\n";
        private IFormatProvider InternalFormatProvider;
        public static readonly TextWriter Null = new NullTextWriter();

        protected TextWriter()
        {
            this.CoreNewLine = new char[] { '\r', '\n' };
            this.InternalFormatProvider = null;
        }

        protected TextWriter(IFormatProvider formatProvider)
        {
            this.CoreNewLine = new char[] { '\r', '\n' };
            this.InternalFormatProvider = formatProvider;
        }

        public virtual void Close()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public virtual void Flush()
        {
        }

        [SecuritySafeCritical, HostProtection(SecurityAction.LinkDemand, Synchronization=true)]
        public static TextWriter Synchronized(TextWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            if (writer is SyncTextWriter)
            {
                return writer;
            }
            return new SyncTextWriter(writer);
        }

        public virtual void Write(bool value)
        {
            this.Write(value ? "True" : "False");
        }

        public virtual void Write(char value)
        {
        }

        public virtual void Write(char[] buffer)
        {
            if (buffer != null)
            {
                this.Write(buffer, 0, buffer.Length);
            }
        }

        public virtual void Write(decimal value)
        {
            this.Write(value.ToString(this.FormatProvider));
        }

        public virtual void Write(double value)
        {
            this.Write(value.ToString(this.FormatProvider));
        }

        public virtual void Write(int value)
        {
            this.Write(value.ToString(this.FormatProvider));
        }

        public virtual void Write(long value)
        {
            this.Write(value.ToString(this.FormatProvider));
        }

        public virtual void Write(object value)
        {
            if (value != null)
            {
                IFormattable formattable = value as IFormattable;
                if (formattable != null)
                {
                    this.Write(formattable.ToString(null, this.FormatProvider));
                }
                else
                {
                    this.Write(value.ToString());
                }
            }
        }

        public virtual void Write(float value)
        {
            this.Write(value.ToString(this.FormatProvider));
        }

        public virtual void Write(string value)
        {
            if (value != null)
            {
                this.Write(value.ToCharArray());
            }
        }

        [CLSCompliant(false)]
        public virtual void Write(uint value)
        {
            this.Write(value.ToString(this.FormatProvider));
        }

        [CLSCompliant(false)]
        public virtual void Write(ulong value)
        {
            this.Write(value.ToString(this.FormatProvider));
        }

        public virtual void Write(string format, object arg0)
        {
            this.Write(string.Format(this.FormatProvider, format, new object[] { arg0 }));
        }

        public virtual void Write(string format, params object[] arg)
        {
            this.Write(string.Format(this.FormatProvider, format, arg));
        }

        public virtual void Write(char[] buffer, int index, int count)
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
            for (int i = 0; i < count; i++)
            {
                this.Write(buffer[index + i]);
            }
        }

        public virtual void Write(string format, object arg0, object arg1)
        {
            this.Write(string.Format(this.FormatProvider, format, new object[] { arg0, arg1 }));
        }

        public virtual void Write(string format, object arg0, object arg1, object arg2)
        {
            this.Write(string.Format(this.FormatProvider, format, new object[] { arg0, arg1, arg2 }));
        }

        public virtual void WriteLine()
        {
            this.Write(this.CoreNewLine);
        }

        public virtual void WriteLine(bool value)
        {
            this.Write(value);
            this.WriteLine();
        }

        public virtual void WriteLine(char value)
        {
            this.Write(value);
            this.WriteLine();
        }

        public virtual void WriteLine(char[] buffer)
        {
            this.Write(buffer);
            this.WriteLine();
        }

        public virtual void WriteLine(decimal value)
        {
            this.Write(value);
            this.WriteLine();
        }

        public virtual void WriteLine(double value)
        {
            this.Write(value);
            this.WriteLine();
        }

        public virtual void WriteLine(int value)
        {
            this.Write(value);
            this.WriteLine();
        }

        public virtual void WriteLine(long value)
        {
            this.Write(value);
            this.WriteLine();
        }

        public virtual void WriteLine(object value)
        {
            if (value == null)
            {
                this.WriteLine();
            }
            else
            {
                IFormattable formattable = value as IFormattable;
                if (formattable != null)
                {
                    this.WriteLine(formattable.ToString(null, this.FormatProvider));
                }
                else
                {
                    this.WriteLine(value.ToString());
                }
            }
        }

        public virtual void WriteLine(float value)
        {
            this.Write(value);
            this.WriteLine();
        }

        [SecuritySafeCritical]
        public virtual void WriteLine(string value)
        {
            if (value == null)
            {
                this.WriteLine();
            }
            else
            {
                int length = value.Length;
                int num2 = this.CoreNewLine.Length;
                char[] destination = new char[length + num2];
                value.CopyTo(0, destination, 0, length);
                switch (num2)
                {
                    case 2:
                        destination[length] = this.CoreNewLine[0];
                        destination[length + 1] = this.CoreNewLine[1];
                        break;

                    case 1:
                        destination[length] = this.CoreNewLine[0];
                        break;

                    default:
                        Buffer.InternalBlockCopy(this.CoreNewLine, 0, destination, length * 2, num2 * 2);
                        break;
                }
                this.Write(destination, 0, length + num2);
            }
        }

        [CLSCompliant(false)]
        public virtual void WriteLine(uint value)
        {
            this.Write(value);
            this.WriteLine();
        }

        [CLSCompliant(false)]
        public virtual void WriteLine(ulong value)
        {
            this.Write(value);
            this.WriteLine();
        }

        public virtual void WriteLine(string format, object arg0)
        {
            this.WriteLine(string.Format(this.FormatProvider, format, new object[] { arg0 }));
        }

        public virtual void WriteLine(string format, params object[] arg)
        {
            this.WriteLine(string.Format(this.FormatProvider, format, arg));
        }

        public virtual void WriteLine(char[] buffer, int index, int count)
        {
            this.Write(buffer, index, count);
            this.WriteLine();
        }

        public virtual void WriteLine(string format, object arg0, object arg1)
        {
            this.WriteLine(string.Format(this.FormatProvider, format, new object[] { arg0, arg1 }));
        }

        public virtual void WriteLine(string format, object arg0, object arg1, object arg2)
        {
            this.WriteLine(string.Format(this.FormatProvider, format, new object[] { arg0, arg1, arg2 }));
        }

        public abstract System.Text.Encoding Encoding { get; }

        public virtual IFormatProvider FormatProvider
        {
            get
            {
                if (this.InternalFormatProvider == null)
                {
                    return Thread.CurrentThread.CurrentCulture;
                }
                return this.InternalFormatProvider;
            }
        }

        public virtual string NewLine
        {
            [SecuritySafeCritical]
            get
            {
                return new string(this.CoreNewLine);
            }
            set
            {
                if (value == null)
                {
                    value = "\r\n";
                }
                this.CoreNewLine = value.ToCharArray();
            }
        }

        [Serializable]
        private sealed class NullTextWriter : TextWriter
        {
            internal NullTextWriter() : base(CultureInfo.InvariantCulture)
            {
            }

            public override void Write(string value)
            {
            }

            public override void Write(char[] buffer, int index, int count)
            {
            }

            public override void WriteLine()
            {
            }

            public override void WriteLine(object value)
            {
            }

            public override void WriteLine(string value)
            {
            }

            public override System.Text.Encoding Encoding
            {
                get
                {
                    return System.Text.Encoding.Default;
                }
            }
        }

        [Serializable]
        internal sealed class SyncTextWriter : TextWriter, IDisposable
        {
            private TextWriter _out;

            internal SyncTextWriter(TextWriter t) : base(t.FormatProvider)
            {
                this._out = t;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override void Close()
            {
                this._out.Close();
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    this._out.Dispose();
                }
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override void Flush()
            {
                this._out.Flush();
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override void Write(bool value)
            {
                this._out.Write(value);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override void Write(char value)
            {
                this._out.Write(value);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override void Write(char[] buffer)
            {
                this._out.Write(buffer);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override void Write(decimal value)
            {
                this._out.Write(value);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override void Write(double value)
            {
                this._out.Write(value);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override void Write(int value)
            {
                this._out.Write(value);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override void Write(long value)
            {
                this._out.Write(value);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override void Write(object value)
            {
                this._out.Write(value);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override void Write(float value)
            {
                this._out.Write(value);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override void Write(string value)
            {
                this._out.Write(value);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override void Write(uint value)
            {
                this._out.Write(value);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override void Write(ulong value)
            {
                this._out.Write(value);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override void Write(string format, object arg0)
            {
                this._out.Write(format, arg0);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override void Write(string format, params object[] arg)
            {
                this._out.Write(format, arg);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override void Write(char[] buffer, int index, int count)
            {
                this._out.Write(buffer, index, count);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override void Write(string format, object arg0, object arg1)
            {
                this._out.Write(format, arg0, arg1);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override void Write(string format, object arg0, object arg1, object arg2)
            {
                this._out.Write(format, arg0, arg1, arg2);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override void WriteLine()
            {
                this._out.WriteLine();
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override void WriteLine(char value)
            {
                this._out.WriteLine(value);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override void WriteLine(decimal value)
            {
                this._out.WriteLine(value);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override void WriteLine(char[] buffer)
            {
                this._out.WriteLine(buffer);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override void WriteLine(bool value)
            {
                this._out.WriteLine(value);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override void WriteLine(double value)
            {
                this._out.WriteLine(value);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override void WriteLine(int value)
            {
                this._out.WriteLine(value);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override void WriteLine(long value)
            {
                this._out.WriteLine(value);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override void WriteLine(object value)
            {
                this._out.WriteLine(value);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override void WriteLine(float value)
            {
                this._out.WriteLine(value);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override void WriteLine(string value)
            {
                this._out.WriteLine(value);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override void WriteLine(uint value)
            {
                this._out.WriteLine(value);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override void WriteLine(ulong value)
            {
                this._out.WriteLine(value);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override void WriteLine(string format, object arg0)
            {
                this._out.WriteLine(format, arg0);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override void WriteLine(string format, params object[] arg)
            {
                this._out.WriteLine(format, arg);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override void WriteLine(char[] buffer, int index, int count)
            {
                this._out.WriteLine(buffer, index, count);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override void WriteLine(string format, object arg0, object arg1)
            {
                this._out.WriteLine(format, arg0, arg1);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override void WriteLine(string format, object arg0, object arg1, object arg2)
            {
                this._out.WriteLine(format, arg0, arg1, arg2);
            }

            public override System.Text.Encoding Encoding
            {
                get
                {
                    return this._out.Encoding;
                }
            }

            public override IFormatProvider FormatProvider
            {
                get
                {
                    return this._out.FormatProvider;
                }
            }

            public override string NewLine
            {
                [MethodImpl(MethodImplOptions.Synchronized)]
                get
                {
                    return this._out.NewLine;
                }
                [MethodImpl(MethodImplOptions.Synchronized)]
                set
                {
                    this._out.NewLine = value;
                }
            }
        }
    }
}

