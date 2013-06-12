namespace System.Net
{
    using System;
    using System.IO;
    using System.Text;

    internal class CommandStream : PooledStream
    {
        private const int _CompletedPipeline = 2;
        private const int _WaitingForPipeline = 1;
        private bool m_Aborted;
        protected string m_AbortReason;
        protected bool m_Async;
        private string m_Buffer;
        protected PipelineEntry[] m_Commands;
        private ResponseDescription m_CurrentResponseDescription;
        private System.Text.Decoder m_Decoder;
        private bool m_DoRead;
        private bool m_DoSend;
        private System.Text.Encoding m_Encoding;
        protected int m_Index;
        private static readonly AsyncCallback m_ReadCallbackDelegate = new AsyncCallback(CommandStream.ReadCallback);
        private bool m_RecoverableFailure;
        protected WebRequest m_Request;
        private static readonly AsyncCallback m_WriteCallbackDelegate = new AsyncCallback(CommandStream.WriteCallback);

        internal CommandStream(ConnectionPool connectionPool, TimeSpan lifetime, bool checkLifetime) : base(connectionPool, lifetime, checkLifetime)
        {
            this.m_Buffer = string.Empty;
            this.m_Encoding = System.Text.Encoding.UTF8;
            this.m_Decoder = this.m_Encoding.GetDecoder();
        }

        internal virtual void Abort(Exception e)
        {
            lock (this)
            {
                if (this.m_Aborted)
                {
                    return;
                }
                this.m_Aborted = true;
                base.CanBePooled = false;
            }
            try
            {
                base.Close(0);
            }
            finally
            {
                if (e != null)
                {
                    this.InvokeRequestCallback(e);
                }
                else
                {
                    this.InvokeRequestCallback(null);
                }
            }
        }

        protected virtual PipelineEntry[] BuildCommandsList(WebRequest request)
        {
            return null;
        }

        internal void CheckContinuePipeline()
        {
            if (!this.m_Async)
            {
                try
                {
                    this.ContinueCommandPipeline();
                }
                catch (Exception exception)
                {
                    this.Abort(exception);
                }
            }
        }

        protected virtual bool CheckValid(ResponseDescription response, ref int validThrough, ref int completeLength)
        {
            return false;
        }

        protected virtual void ClearState()
        {
            this.InitCommandPipeline(null, null, false);
        }

        protected Stream ContinueCommandPipeline()
        {
            bool async = this.m_Async;
            while (this.m_Index < this.m_Commands.Length)
            {
                if (this.m_DoSend)
                {
                    if (this.m_Index < 0)
                    {
                        throw new InternalException();
                    }
                    byte[] bytes = this.Encoding.GetBytes(this.m_Commands[this.m_Index].Command);
                    if (Logging.On)
                    {
                        string str = this.m_Commands[this.m_Index].Command.Substring(0, this.m_Commands[this.m_Index].Command.Length - 2);
                        if (this.m_Commands[this.m_Index].HasFlag(PipelineEntryFlags.DontLogParameter))
                        {
                            int index = str.IndexOf(' ');
                            if (index != -1)
                            {
                                str = str.Substring(0, index) + " ********";
                            }
                        }
                        Logging.PrintInfo(Logging.Web, this, SR.GetString("net_log_sending_command", new object[] { str }));
                    }
                    try
                    {
                        if (async)
                        {
                            this.BeginWrite(bytes, 0, bytes.Length, m_WriteCallbackDelegate, this);
                        }
                        else
                        {
                            this.Write(bytes, 0, bytes.Length);
                        }
                    }
                    catch (IOException)
                    {
                        this.MarkAsRecoverableFailure();
                        throw;
                    }
                    catch
                    {
                        throw;
                    }
                    if (async)
                    {
                        return null;
                    }
                }
                Stream stream = null;
                if (this.PostSendCommandProcessing(ref stream))
                {
                    return stream;
                }
            }
            lock (this)
            {
                this.Close();
            }
            return null;
        }

        protected override void Dispose(bool disposing)
        {
            this.InvokeRequestCallback(null);
        }

        protected Exception GenerateException(WebExceptionStatus status, Exception innerException)
        {
            return new WebException(NetRes.GetWebStatusString("net_connclosed", status), innerException, status, null);
        }

        protected Exception GenerateException(FtpStatusCode code, string statusDescription, Exception innerException)
        {
            return new WebException(SR.GetString("net_servererror", new object[] { NetRes.GetWebStatusCodeString(code, statusDescription) }), innerException, WebExceptionStatus.ProtocolError, null);
        }

        protected void InitCommandPipeline(WebRequest request, PipelineEntry[] commands, bool async)
        {
            this.m_Commands = commands;
            this.m_Index = 0;
            this.m_Request = request;
            this.m_Aborted = false;
            this.m_DoRead = true;
            this.m_DoSend = true;
            this.m_CurrentResponseDescription = null;
            this.m_Async = async;
            this.m_RecoverableFailure = false;
            this.m_AbortReason = string.Empty;
        }

        protected void InvokeRequestCallback(object obj)
        {
            WebRequest request = this.m_Request;
            if (request != null)
            {
                request.RequestCallback(obj);
            }
        }

        protected void MarkAsRecoverableFailure()
        {
            if (this.m_Index <= 1)
            {
                this.m_RecoverableFailure = true;
            }
        }

        protected virtual PipelineInstruction PipelineCallback(PipelineEntry entry, ResponseDescription response, bool timeout, ref Stream stream)
        {
            return PipelineInstruction.Abort;
        }

        private bool PostReadCommandProcessing(ref Stream stream)
        {
            if (this.m_Index < this.m_Commands.Length)
            {
                PipelineInstruction advance;
                PipelineEntry entry;
                this.m_DoSend = false;
                this.m_DoRead = false;
                if (this.m_Index == -1)
                {
                    entry = null;
                }
                else
                {
                    entry = this.m_Commands[this.m_Index];
                }
                if ((this.m_CurrentResponseDescription == null) && (entry.Command == "QUIT\r\n"))
                {
                    advance = PipelineInstruction.Advance;
                }
                else
                {
                    advance = this.PipelineCallback(entry, this.m_CurrentResponseDescription, false, ref stream);
                }
                switch (advance)
                {
                    case PipelineInstruction.Advance:
                        this.m_CurrentResponseDescription = null;
                        this.m_DoSend = true;
                        this.m_DoRead = true;
                        this.m_Index++;
                        break;

                    case PipelineInstruction.Pause:
                        return true;

                    case PipelineInstruction.Abort:
                        Exception exception;
                        if (this.m_AbortReason != string.Empty)
                        {
                            exception = new WebException(this.m_AbortReason);
                        }
                        else
                        {
                            exception = this.GenerateException(WebExceptionStatus.ServerProtocolViolation, null);
                        }
                        this.Abort(exception);
                        throw exception;

                    case PipelineInstruction.GiveStream:
                        this.m_CurrentResponseDescription = null;
                        this.m_DoRead = true;
                        if (this.m_Async)
                        {
                            this.ContinueCommandPipeline();
                            this.InvokeRequestCallback(stream);
                        }
                        return true;

                    case PipelineInstruction.Reread:
                        this.m_CurrentResponseDescription = null;
                        this.m_DoRead = true;
                        break;
                }
            }
            return false;
        }

        private bool PostSendCommandProcessing(ref Stream stream)
        {
            if (this.m_DoRead)
            {
                bool async = this.m_Async;
                int index = this.m_Index;
                PipelineEntry[] commands = this.m_Commands;
                try
                {
                    ResponseDescription description = this.ReceiveCommandResponse();
                    if (async)
                    {
                        return true;
                    }
                    this.m_CurrentResponseDescription = description;
                }
                catch
                {
                    if (((index < 0) || (index >= commands.Length)) || (commands[index].Command != "QUIT\r\n"))
                    {
                        throw;
                    }
                }
            }
            return this.PostReadCommandProcessing(ref stream);
        }

        private static void ReadCallback(IAsyncResult asyncResult)
        {
            ReceiveState asyncState = (ReceiveState) asyncResult.AsyncState;
            try
            {
                Stream connection = asyncState.Connection;
                int bytesRead = 0;
                try
                {
                    bytesRead = connection.EndRead(asyncResult);
                    if (bytesRead == 0)
                    {
                        asyncState.Connection.CloseSocket();
                    }
                }
                catch (IOException)
                {
                    asyncState.Connection.MarkAsRecoverableFailure();
                    throw;
                }
                catch
                {
                    throw;
                }
                asyncState.Connection.ReceiveCommandResponseCallback(asyncState, bytesRead);
            }
            catch (Exception exception)
            {
                asyncState.Connection.Abort(exception);
            }
        }

        private ResponseDescription ReceiveCommandResponse()
        {
            ReceiveState state = new ReceiveState(this);
            try
            {
                if (this.m_Buffer.Length > 0)
                {
                    this.ReceiveCommandResponseCallback(state, -1);
                }
                else
                {
                    try
                    {
                        if (this.m_Async)
                        {
                            this.BeginRead(state.Buffer, 0, state.Buffer.Length, m_ReadCallbackDelegate, state);
                            return null;
                        }
                        int bytesRead = this.Read(state.Buffer, 0, state.Buffer.Length);
                        if (bytesRead == 0)
                        {
                            base.CloseSocket();
                        }
                        this.ReceiveCommandResponseCallback(state, bytesRead);
                    }
                    catch (IOException)
                    {
                        this.MarkAsRecoverableFailure();
                        throw;
                    }
                    catch
                    {
                        throw;
                    }
                }
            }
            catch (Exception exception)
            {
                if (exception is WebException)
                {
                    throw;
                }
                throw this.GenerateException(WebExceptionStatus.ReceiveFailure, exception);
            }
            return state.Resp;
        }

        private void ReceiveCommandResponseCallback(ReceiveState state, int bytesRead)
        {
            int num2;
            int completeLength = -1;
        Label_0002:
            num2 = state.ValidThrough;
            if (this.m_Buffer.Length > 0)
            {
                state.Resp.StatusBuffer.Append(this.m_Buffer);
                this.m_Buffer = string.Empty;
                if (!this.CheckValid(state.Resp, ref num2, ref completeLength))
                {
                    throw this.GenerateException(WebExceptionStatus.ServerProtocolViolation, null);
                }
            }
            else
            {
                if (bytesRead <= 0)
                {
                    throw this.GenerateException(WebExceptionStatus.ServerProtocolViolation, null);
                }
                char[] chars = new char[this.m_Decoder.GetCharCount(state.Buffer, 0, bytesRead)];
                int length = this.m_Decoder.GetChars(state.Buffer, 0, bytesRead, chars, 0, false);
                string str = new string(chars, 0, length);
                state.Resp.StatusBuffer.Append(str);
                if (!this.CheckValid(state.Resp, ref num2, ref completeLength))
                {
                    throw this.GenerateException(WebExceptionStatus.ServerProtocolViolation, null);
                }
                if (completeLength >= 0)
                {
                    int num4 = state.Resp.StatusBuffer.Length - completeLength;
                    if (num4 > 0)
                    {
                        this.m_Buffer = str.Substring(str.Length - num4, num4);
                    }
                }
            }
            if (completeLength < 0)
            {
                state.ValidThrough = num2;
                try
                {
                    if (this.m_Async)
                    {
                        this.BeginRead(state.Buffer, 0, state.Buffer.Length, m_ReadCallbackDelegate, state);
                        return;
                    }
                    bytesRead = this.Read(state.Buffer, 0, state.Buffer.Length);
                    if (bytesRead == 0)
                    {
                        base.CloseSocket();
                    }
                    goto Label_0002;
                }
                catch (IOException)
                {
                    this.MarkAsRecoverableFailure();
                    throw;
                }
                catch
                {
                    throw;
                }
            }
            string str2 = state.Resp.StatusBuffer.ToString();
            state.Resp.StatusDescription = str2.Substring(0, completeLength);
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.Web, this, SR.GetString("net_log_received_response", new object[] { str2.Substring(0, completeLength - 2) }));
            }
            if (this.m_Async)
            {
                if (state.Resp != null)
                {
                    this.m_CurrentResponseDescription = state.Resp;
                }
                Stream stream = null;
                if (!this.PostReadCommandProcessing(ref stream))
                {
                    this.ContinueCommandPipeline();
                }
            }
        }

        internal Stream SubmitRequest(WebRequest request, bool async, bool readInitalResponseOnConnect)
        {
            this.ClearState();
            base.UpdateLifetime();
            PipelineEntry[] commands = this.BuildCommandsList(request);
            this.InitCommandPipeline(request, commands, async);
            if (readInitalResponseOnConnect && base.JustConnected)
            {
                this.m_DoSend = false;
                this.m_Index = -1;
            }
            return this.ContinueCommandPipeline();
        }

        private static void WriteCallback(IAsyncResult asyncResult)
        {
            CommandStream asyncState = (CommandStream) asyncResult.AsyncState;
            try
            {
                try
                {
                    asyncState.EndWrite(asyncResult);
                }
                catch (IOException)
                {
                    asyncState.MarkAsRecoverableFailure();
                    throw;
                }
                catch
                {
                    throw;
                }
                Stream stream = null;
                if (!asyncState.PostSendCommandProcessing(ref stream))
                {
                    asyncState.ContinueCommandPipeline();
                }
            }
            catch (Exception exception)
            {
                asyncState.Abort(exception);
            }
        }

        protected System.Text.Encoding Encoding
        {
            get
            {
                return this.m_Encoding;
            }
            set
            {
                this.m_Encoding = value;
                this.m_Decoder = this.m_Encoding.GetDecoder();
            }
        }

        internal bool RecoverableFailure
        {
            get
            {
                return this.m_RecoverableFailure;
            }
        }

        internal class PipelineEntry
        {
            internal string Command;
            internal CommandStream.PipelineEntryFlags Flags;

            internal PipelineEntry(string command)
            {
                this.Command = command;
            }

            internal PipelineEntry(string command, CommandStream.PipelineEntryFlags flags)
            {
                this.Command = command;
                this.Flags = flags;
            }

            internal bool HasFlag(CommandStream.PipelineEntryFlags flags)
            {
                return ((this.Flags & flags) != 0);
            }
        }

        [Flags]
        internal enum PipelineEntryFlags
        {
            CreateDataConnection = 4,
            DontLogParameter = 8,
            GiveDataStream = 2,
            UserCommand = 1
        }

        internal enum PipelineInstruction
        {
            Abort,
            Advance,
            Pause,
            Reread,
            GiveStream
        }
    }
}

