namespace System.Net.Mail
{
    using System;
    using System.Net.Mime;
    using System.Runtime.InteropServices;

    internal static class CheckCommand
    {
        private static AsyncCallback onReadLine = new AsyncCallback(CheckCommand.OnReadLine);
        private static AsyncCallback onWrite = new AsyncCallback(CheckCommand.OnWrite);

        internal static IAsyncResult BeginSend(SmtpConnection conn, AsyncCallback callback, object state)
        {
            MultiAsyncResult result = new MultiAsyncResult(conn, callback, state);
            result.Enter();
            IAsyncResult result2 = conn.BeginFlush(onWrite, result);
            if (result2.CompletedSynchronously)
            {
                conn.EndFlush(result2);
                result.Leave();
            }
            SmtpReplyReader nextReplyReader = conn.Reader.GetNextReplyReader();
            result.Enter();
            IAsyncResult result3 = nextReplyReader.BeginReadLine(onReadLine, result);
            if (result3.CompletedSynchronously)
            {
                LineInfo info = nextReplyReader.EndReadLine(result3);
                if (!(result.Result is Exception))
                {
                    result.Result = info;
                }
                result.Leave();
            }
            result.CompleteSequence();
            return result;
        }

        internal static object EndSend(IAsyncResult result, out string response)
        {
            object obj2 = MultiAsyncResult.End(result);
            if (obj2 is Exception)
            {
                throw ((Exception) obj2);
            }
            LineInfo info = (LineInfo) obj2;
            response = info.Line;
            return info.StatusCode;
        }

        private static void OnReadLine(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                MultiAsyncResult asyncState = (MultiAsyncResult) result.AsyncState;
                try
                {
                    SmtpConnection context = (SmtpConnection) asyncState.Context;
                    LineInfo info = context.Reader.CurrentReader.EndReadLine(result);
                    if (!(asyncState.Result is Exception))
                    {
                        asyncState.Result = info;
                    }
                    asyncState.Leave();
                }
                catch (Exception exception)
                {
                    asyncState.Leave(exception);
                }
            }
        }

        private static void OnWrite(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                MultiAsyncResult asyncState = (MultiAsyncResult) result.AsyncState;
                try
                {
                    ((SmtpConnection) asyncState.Context).EndFlush(result);
                    asyncState.Leave();
                }
                catch (Exception exception)
                {
                    asyncState.Leave(exception);
                }
            }
        }

        internal static SmtpStatusCode Send(SmtpConnection conn, out string response)
        {
            conn.Flush();
            SmtpReplyReader nextReplyReader = conn.Reader.GetNextReplyReader();
            LineInfo info = nextReplyReader.ReadLine();
            response = info.Line;
            nextReplyReader.Close();
            return info.StatusCode;
        }
    }
}

