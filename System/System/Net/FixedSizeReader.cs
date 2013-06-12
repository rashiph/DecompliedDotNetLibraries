namespace System.Net
{
    using System;
    using System.IO;

    internal class FixedSizeReader
    {
        private static readonly AsyncCallback _ReadCallback = new AsyncCallback(FixedSizeReader.ReadCallback);
        private AsyncProtocolRequest _Request;
        private int _TotalRead;
        private readonly Stream _Transport;

        public FixedSizeReader(Stream transport)
        {
            this._Transport = transport;
        }

        public void AsyncReadPacket(AsyncProtocolRequest request)
        {
            this._Request = request;
            this._TotalRead = 0;
            this.StartReading();
        }

        private bool CheckCompletionBeforeNextRead(int bytes)
        {
            if (bytes == 0)
            {
                if (this._TotalRead != 0)
                {
                    throw new IOException(SR.GetString("net_io_eof"));
                }
                this._Request.CompleteRequest(0);
                return true;
            }
            if ((this._TotalRead += bytes) == this._Request.Count)
            {
                this._Request.CompleteRequest(this._Request.Count);
                return true;
            }
            return false;
        }

        private static void ReadCallback(IAsyncResult transportResult)
        {
            if (!transportResult.CompletedSynchronously)
            {
                FixedSizeReader asyncState = (FixedSizeReader) transportResult.AsyncState;
                AsyncProtocolRequest request = asyncState._Request;
                try
                {
                    int bytes = asyncState._Transport.EndRead(transportResult);
                    if (!asyncState.CheckCompletionBeforeNextRead(bytes))
                    {
                        asyncState.StartReading();
                    }
                }
                catch (Exception exception)
                {
                    if (request.IsUserCompleted)
                    {
                        throw;
                    }
                    request.CompleteWithError(exception);
                }
            }
        }

        public int ReadPacket(byte[] buffer, int offset, int count)
        {
            int num = count;
            do
            {
                int num2 = this._Transport.Read(buffer, offset, num);
                if (num2 == 0)
                {
                    if (num != count)
                    {
                        throw new IOException(SR.GetString("net_io_eof"));
                    }
                    return 0;
                }
                num -= num2;
                offset += num2;
            }
            while (num != 0);
            return count;
        }

        private void StartReading()
        {
            IAsyncResult result;
        Label_0000:
            result = this._Transport.BeginRead(this._Request.Buffer, this._Request.Offset + this._TotalRead, this._Request.Count - this._TotalRead, _ReadCallback, this);
            if (result.CompletedSynchronously)
            {
                int bytes = this._Transport.EndRead(result);
                if (!this.CheckCompletionBeforeNextRead(bytes))
                {
                    goto Label_0000;
                }
            }
        }
    }
}

