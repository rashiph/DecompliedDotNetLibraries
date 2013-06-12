namespace System.Net.Mail
{
    using System;
    using System.Net.Mime;

    internal static class ReadLinesCommand
    {
        private static AsyncCallback onReadLines = new AsyncCallback(ReadLinesCommand.OnReadLines);
        private static AsyncCallback onWrite = new AsyncCallback(ReadLinesCommand.OnWrite);

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
            IAsyncResult result3 = nextReplyReader.BeginReadLines(onReadLines, result);
            if (result3.CompletedSynchronously)
            {
                LineInfo[] infoArray = conn.Reader.CurrentReader.EndReadLines(result3);
                if (!(result.Result is Exception))
                {
                    result.Result = infoArray;
                }
                result.Leave();
            }
            result.CompleteSequence();
            return result;
        }

        internal static LineInfo[] EndSend(IAsyncResult result)
        {
            object obj2 = MultiAsyncResult.End(result);
            if (obj2 is Exception)
            {
                throw ((Exception) obj2);
            }
            return (LineInfo[]) obj2;
        }

        private static void OnReadLines(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                MultiAsyncResult asyncState = (MultiAsyncResult) result.AsyncState;
                try
                {
                    SmtpConnection context = (SmtpConnection) asyncState.Context;
                    LineInfo[] infoArray = context.Reader.CurrentReader.EndReadLines(result);
                    if (!(asyncState.Result is Exception))
                    {
                        asyncState.Result = infoArray;
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

        internal static LineInfo[] Send(SmtpConnection conn)
        {
            conn.Flush();
            return conn.Reader.GetNextReplyReader().ReadLines();
        }
    }
}

