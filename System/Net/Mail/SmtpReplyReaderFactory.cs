namespace System.Net.Mail
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Net;
    using System.Text;

    internal class SmtpReplyReaderFactory
    {
        private BufferedReadStream bufferedStream;
        private byte[] byteBuffer;
        private char[] charBuffer;
        private SmtpReplyReader currentReader;
        private const int DefaultBufferSize = 0x100;
        private ReadState readState;
        private SmtpStatusCode statusCode;

        internal SmtpReplyReaderFactory(Stream stream)
        {
            this.bufferedStream = new BufferedReadStream(stream);
        }

        internal IAsyncResult BeginReadLine(SmtpReplyReader caller, AsyncCallback callback, object state)
        {
            ReadLinesAsyncResult result = new ReadLinesAsyncResult(this, callback, state, true);
            result.Read(caller);
            return result;
        }

        internal IAsyncResult BeginReadLines(SmtpReplyReader caller, AsyncCallback callback, object state)
        {
            ReadLinesAsyncResult result = new ReadLinesAsyncResult(this, callback, state);
            result.Read(caller);
            return result;
        }

        internal void Close(SmtpReplyReader caller)
        {
            if (this.currentReader == caller)
            {
                if (this.readState != ReadState.Done)
                {
                    if (this.byteBuffer == null)
                    {
                        this.byteBuffer = new byte[0x100];
                    }
                    while (this.Read(caller, this.byteBuffer, 0, this.byteBuffer.Length) != 0)
                    {
                    }
                }
                this.currentReader = null;
            }
        }

        internal LineInfo EndReadLine(IAsyncResult result)
        {
            LineInfo[] infoArray = ReadLinesAsyncResult.End(result);
            if ((infoArray != null) && (infoArray.Length > 0))
            {
                return infoArray[0];
            }
            return new LineInfo();
        }

        internal LineInfo[] EndReadLines(IAsyncResult result)
        {
            return ReadLinesAsyncResult.End(result);
        }

        internal SmtpReplyReader GetNextReplyReader()
        {
            if (this.currentReader != null)
            {
                this.currentReader.Close();
            }
            this.readState = ReadState.Status0;
            this.currentReader = new SmtpReplyReader(this);
            return this.currentReader;
        }

        private unsafe int ProcessRead(byte[] buffer, int offset, int read, bool readLine)
        {
            if (read == 0)
            {
                throw new IOException(SR.GetString("net_io_readfailure", new object[] { "net_io_connectionclosed" }));
            }
            fixed (byte* numRef = buffer)
            {
                int num5;
                byte* numPtr = numRef + offset;
                byte* numPtr2 = numPtr;
                byte* numPtr3 = numPtr2 + read;
                switch (this.readState)
                {
                    case ReadState.Status0:
                        break;

                    case ReadState.Status1:
                        goto Label_00C8;

                    case ReadState.Status2:
                        goto Label_0114;

                    case ReadState.ContinueFlag:
                        goto Label_015D;

                    case ReadState.ContinueCR:
                        goto Label_01A0;

                    case ReadState.ContinueLF:
                        goto Label_01B0;

                    case ReadState.LastCR:
                        goto Label_01FC;

                    case ReadState.LastLF:
                        goto Label_0209;

                    case ReadState.Done:
                        goto Label_0231;

                    default:
                        goto Label_0247;
                }
            Label_0083:
                if (numPtr2 < numPtr3)
                {
                    numPtr2++;
                    byte num = numPtr2[0];
                    if ((num < 0x30) && (num > 0x39))
                    {
                        throw new FormatException(SR.GetString("SmtpInvalidResponse"));
                    }
                    this.statusCode = (SmtpStatusCode) (100 * (num - 0x30));
                }
                else
                {
                    this.readState = ReadState.Status0;
                    goto Label_0247;
                }
            Label_00C8:
                if (numPtr2 < numPtr3)
                {
                    numPtr2++;
                    byte num2 = numPtr2[0];
                    if ((num2 < 0x30) && (num2 > 0x39))
                    {
                        throw new FormatException(SR.GetString("SmtpInvalidResponse"));
                    }
                    this.statusCode += 10 * (num2 - 0x30);
                }
                else
                {
                    this.readState = ReadState.Status1;
                    goto Label_0247;
                }
            Label_0114:
                if (numPtr2 < numPtr3)
                {
                    numPtr2++;
                    byte num3 = numPtr2[0];
                    if ((num3 < 0x30) && (num3 > 0x39))
                    {
                        throw new FormatException(SR.GetString("SmtpInvalidResponse"));
                    }
                    this.statusCode += num3 - 0x30;
                }
                else
                {
                    this.readState = ReadState.Status2;
                    goto Label_0247;
                }
            Label_015D:
                if (numPtr2 < numPtr3)
                {
                    numPtr2++;
                    byte num4 = numPtr2[0];
                    if (num4 == 0x20)
                    {
                        goto Label_01FC;
                    }
                    if (num4 != 0x2d)
                    {
                        throw new FormatException(SR.GetString("SmtpInvalidResponse"));
                    }
                    goto Label_01A0;
                }
                this.readState = ReadState.ContinueFlag;
                goto Label_0247;
            Label_0195:
                numPtr2++;
                if (numPtr2[0] == 13)
                {
                    goto Label_01B0;
                }
            Label_01A0:
                if (numPtr2 < numPtr3)
                {
                    goto Label_0195;
                }
                this.readState = ReadState.ContinueCR;
                goto Label_0247;
            Label_01B0:
                if (numPtr2 < numPtr3)
                {
                    numPtr2++;
                    if (numPtr2[0] != 10)
                    {
                        throw new FormatException(SR.GetString("SmtpInvalidResponse"));
                    }
                    if (readLine)
                    {
                        this.readState = ReadState.Status0;
                        return (int) ((long) ((numPtr2 - numPtr) / 1));
                    }
                    goto Label_0083;
                }
                this.readState = ReadState.ContinueLF;
                goto Label_0247;
            Label_01F1:
                numPtr2++;
                if (numPtr2[0] == 13)
                {
                    goto Label_0209;
                }
            Label_01FC:
                if (numPtr2 < numPtr3)
                {
                    goto Label_01F1;
                }
                this.readState = ReadState.LastCR;
                goto Label_0247;
            Label_0209:
                if (numPtr2 < numPtr3)
                {
                    numPtr2++;
                    if (numPtr2[0] != 10)
                    {
                        throw new FormatException(SR.GetString("SmtpInvalidResponse"));
                    }
                }
                else
                {
                    this.readState = ReadState.LastLF;
                    goto Label_0247;
                }
            Label_0231:
                num5 = (int) ((long) ((numPtr2 - numPtr) / 1));
                this.readState = ReadState.Done;
                return num5;
            Label_0247:
                return (int) ((long) ((numPtr2 - numPtr) / 1));
            }
        }

        internal int Read(SmtpReplyReader caller, byte[] buffer, int offset, int count)
        {
            if (((count == 0) || (this.currentReader != caller)) || (this.readState == ReadState.Done))
            {
                return 0;
            }
            int read = this.bufferedStream.Read(buffer, offset, count);
            int num2 = this.ProcessRead(buffer, offset, read, false);
            if (num2 < read)
            {
                this.bufferedStream.Push(buffer, offset + num2, read - num2);
            }
            return num2;
        }

        internal LineInfo ReadLine(SmtpReplyReader caller)
        {
            LineInfo[] infoArray = this.ReadLines(caller, true);
            if ((infoArray != null) && (infoArray.Length > 0))
            {
                return infoArray[0];
            }
            return new LineInfo();
        }

        internal LineInfo[] ReadLines(SmtpReplyReader caller)
        {
            return this.ReadLines(caller, false);
        }

        internal LineInfo[] ReadLines(SmtpReplyReader caller, bool oneLine)
        {
            if ((caller != this.currentReader) || (this.readState == ReadState.Done))
            {
                return new LineInfo[0];
            }
            if (this.byteBuffer == null)
            {
                this.byteBuffer = new byte[0x100];
            }
            if (this.charBuffer == null)
            {
                this.charBuffer = new char[0x100];
            }
            StringBuilder builder = new StringBuilder();
            ArrayList list = new ArrayList();
            int num = 0;
            int offset = 0;
            int num3 = 0;
        Label_005C:
            if (offset == num3)
            {
                num3 = this.bufferedStream.Read(this.byteBuffer, 0, this.byteBuffer.Length);
                offset = 0;
            }
            int num4 = this.ProcessRead(this.byteBuffer, offset, num3 - offset, true);
            if (num < 4)
            {
                int num5 = Math.Min(4 - num, num4);
                num += num5;
                offset += num5;
                num4 -= num5;
                if (num4 == 0)
                {
                    goto Label_005C;
                }
            }
            for (int i = offset; i < (offset + num4); i++)
            {
                this.charBuffer[i] = (char) this.byteBuffer[i];
            }
            builder.Append(this.charBuffer, offset, num4);
            offset += num4;
            if (this.readState == ReadState.Status0)
            {
                num = 0;
                list.Add(new LineInfo(this.statusCode, builder.ToString(0, builder.Length - 2)));
                if (oneLine)
                {
                    this.bufferedStream.Push(this.byteBuffer, offset, num3 - offset);
                    return (LineInfo[]) list.ToArray(typeof(LineInfo));
                }
                builder = new StringBuilder();
                goto Label_005C;
            }
            if (this.readState != ReadState.Done)
            {
                goto Label_005C;
            }
            list.Add(new LineInfo(this.statusCode, builder.ToString(0, builder.Length - 2)));
            this.bufferedStream.Push(this.byteBuffer, offset, num3 - offset);
            return (LineInfo[]) list.ToArray(typeof(LineInfo));
        }

        internal SmtpReplyReader CurrentReader
        {
            get
            {
                return this.currentReader;
            }
        }

        internal SmtpStatusCode StatusCode
        {
            get
            {
                return this.statusCode;
            }
        }

        private class ReadLinesAsyncResult : LazyAsyncResult
        {
            private StringBuilder builder;
            private ArrayList lines;
            private bool oneLine;
            private SmtpReplyReaderFactory parent;
            private int read;
            private static AsyncCallback readCallback = new AsyncCallback(SmtpReplyReaderFactory.ReadLinesAsyncResult.ReadCallback);
            private int statusRead;

            internal ReadLinesAsyncResult(SmtpReplyReaderFactory parent, AsyncCallback callback, object state) : base(null, state, callback)
            {
                this.parent = parent;
            }

            internal ReadLinesAsyncResult(SmtpReplyReaderFactory parent, AsyncCallback callback, object state, bool oneLine) : base(null, state, callback)
            {
                this.oneLine = oneLine;
                this.parent = parent;
            }

            internal static LineInfo[] End(IAsyncResult result)
            {
                SmtpReplyReaderFactory.ReadLinesAsyncResult result2 = (SmtpReplyReaderFactory.ReadLinesAsyncResult) result;
                result2.InternalWaitForCompletion();
                return (LineInfo[]) result2.lines.ToArray(typeof(LineInfo));
            }

            private bool ProcessRead()
            {
                if (this.read == 0)
                {
                    throw new IOException(SR.GetString("net_io_readfailure", new object[] { "net_io_connectionclosed" }));
                }
                int offset = 0;
                while (offset != this.read)
                {
                    int num2 = this.parent.ProcessRead(this.parent.byteBuffer, offset, this.read - offset, true);
                    if (this.statusRead < 4)
                    {
                        int num3 = Math.Min(4 - this.statusRead, num2);
                        this.statusRead += num3;
                        offset += num3;
                        num2 -= num3;
                        if (num2 == 0)
                        {
                            continue;
                        }
                    }
                    for (int i = offset; i < (offset + num2); i++)
                    {
                        this.parent.charBuffer[i] = (char) this.parent.byteBuffer[i];
                    }
                    this.builder.Append(this.parent.charBuffer, offset, num2);
                    offset += num2;
                    if (this.parent.readState == SmtpReplyReaderFactory.ReadState.Status0)
                    {
                        this.lines.Add(new LineInfo(this.parent.statusCode, this.builder.ToString(0, this.builder.Length - 2)));
                        this.builder = new StringBuilder();
                        this.statusRead = 0;
                        if (this.oneLine)
                        {
                            this.parent.bufferedStream.Push(this.parent.byteBuffer, offset, this.read - offset);
                            base.InvokeCallback();
                            return false;
                        }
                    }
                    else if (this.parent.readState == SmtpReplyReaderFactory.ReadState.Done)
                    {
                        this.lines.Add(new LineInfo(this.parent.statusCode, this.builder.ToString(0, this.builder.Length - 2)));
                        this.parent.bufferedStream.Push(this.parent.byteBuffer, offset, this.read - offset);
                        base.InvokeCallback();
                        return false;
                    }
                }
                return true;
            }

            private void Read()
            {
                IAsyncResult result;
            Label_0000:
                result = this.parent.bufferedStream.BeginRead(this.parent.byteBuffer, 0, this.parent.byteBuffer.Length, readCallback, this);
                if (result.CompletedSynchronously)
                {
                    this.read = this.parent.bufferedStream.EndRead(result);
                    if (this.ProcessRead())
                    {
                        goto Label_0000;
                    }
                }
            }

            internal void Read(SmtpReplyReader caller)
            {
                if ((this.parent.currentReader != caller) || (this.parent.readState == SmtpReplyReaderFactory.ReadState.Done))
                {
                    base.InvokeCallback();
                }
                else
                {
                    if (this.parent.byteBuffer == null)
                    {
                        this.parent.byteBuffer = new byte[0x100];
                    }
                    if (this.parent.charBuffer == null)
                    {
                        this.parent.charBuffer = new char[0x100];
                    }
                    this.builder = new StringBuilder();
                    this.lines = new ArrayList();
                    this.Read();
                }
            }

            private static void ReadCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    Exception exception = null;
                    SmtpReplyReaderFactory.ReadLinesAsyncResult asyncState = (SmtpReplyReaderFactory.ReadLinesAsyncResult) result.AsyncState;
                    try
                    {
                        asyncState.read = asyncState.parent.bufferedStream.EndRead(result);
                        if (asyncState.ProcessRead())
                        {
                            asyncState.Read();
                        }
                    }
                    catch (Exception exception2)
                    {
                        exception = exception2;
                    }
                    if (exception != null)
                    {
                        asyncState.InvokeCallback(exception);
                    }
                }
            }
        }

        private enum ReadState
        {
            Status0,
            Status1,
            Status2,
            ContinueFlag,
            ContinueCR,
            ContinueLF,
            LastCR,
            LastLF,
            Done
        }
    }
}

