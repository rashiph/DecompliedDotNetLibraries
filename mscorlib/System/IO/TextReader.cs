namespace System.IO
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;

    [Serializable, ComVisible(true)]
    public abstract class TextReader : MarshalByRefObject, IDisposable
    {
        public static readonly TextReader Null = new NullTextReader();

        protected TextReader()
        {
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

        public virtual int Peek()
        {
            return -1;
        }

        public virtual int Read()
        {
            return -1;
        }

        public virtual int Read([In, Out] char[] buffer, int index, int count)
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
            int num = 0;
            do
            {
                int num2 = this.Read();
                if (num2 == -1)
                {
                    return num;
                }
                buffer[index + num++] = (char) num2;
            }
            while (num < count);
            return num;
        }

        public virtual int ReadBlock([In, Out] char[] buffer, int index, int count)
        {
            int num;
            int num2 = 0;
            do
            {
                num2 += num = this.Read(buffer, index + num2, count - num2);
            }
            while ((num > 0) && (num2 < count));
            return num2;
        }

        public virtual string ReadLine()
        {
            StringBuilder builder = new StringBuilder();
            while (true)
            {
                int num = this.Read();
                switch (num)
                {
                    case -1:
                        if (builder.Length > 0)
                        {
                            return builder.ToString();
                        }
                        return null;

                    case 13:
                    case 10:
                        if ((num == 13) && (this.Peek() == 10))
                        {
                            this.Read();
                        }
                        return builder.ToString();
                }
                builder.Append((char) num);
            }
        }

        public virtual string ReadToEnd()
        {
            int num;
            char[] buffer = new char[0x1000];
            StringBuilder builder = new StringBuilder(0x1000);
            while ((num = this.Read(buffer, 0, buffer.Length)) != 0)
            {
                builder.Append(buffer, 0, num);
            }
            return builder.ToString();
        }

        [HostProtection(SecurityAction.LinkDemand, Synchronization=true)]
        public static TextReader Synchronized(TextReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            if (reader is SyncTextReader)
            {
                return reader;
            }
            return new SyncTextReader(reader);
        }

        [Serializable]
        private sealed class NullTextReader : TextReader
        {
            public override int Read(char[] buffer, int index, int count)
            {
                return 0;
            }

            public override string ReadLine()
            {
                return null;
            }
        }

        [Serializable]
        internal sealed class SyncTextReader : TextReader
        {
            internal TextReader _in;

            internal SyncTextReader(TextReader t)
            {
                this._in = t;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override void Close()
            {
                this._in.Close();
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    this._in.Dispose();
                }
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override int Peek()
            {
                return this._in.Peek();
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override int Read()
            {
                return this._in.Read();
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override int Read([In, Out] char[] buffer, int index, int count)
            {
                return this._in.Read(buffer, index, count);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override int ReadBlock([In, Out] char[] buffer, int index, int count)
            {
                return this._in.ReadBlock(buffer, index, count);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override string ReadLine()
            {
                return this._in.ReadLine();
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override string ReadToEnd()
            {
                return this._in.ReadToEnd();
            }
        }
    }
}

