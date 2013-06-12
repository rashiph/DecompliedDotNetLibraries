namespace System.Net.Mail
{
    using System;

    internal static class DataCommand
    {
        internal static IAsyncResult BeginSend(SmtpConnection conn, AsyncCallback callback, object state)
        {
            PrepareCommand(conn);
            return CheckCommand.BeginSend(conn, callback, state);
        }

        private static void CheckResponse(SmtpStatusCode statusCode, string serverResponse)
        {
            switch (statusCode)
            {
                case SmtpStatusCode.StartMailInput:
                    return;
            }
            if (statusCode < ((SmtpStatusCode) 400))
            {
                throw new SmtpException(SR.GetString("net_webstatus_ServerProtocolViolation"), serverResponse);
            }
            throw new SmtpException(statusCode, serverResponse, true);
        }

        internal static void EndSend(IAsyncResult result)
        {
            string str;
            SmtpStatusCode statusCode = (SmtpStatusCode) CheckCommand.EndSend(result, out str);
            CheckResponse(statusCode, str);
        }

        private static void PrepareCommand(SmtpConnection conn)
        {
            if (conn.IsStreamOpen)
            {
                throw new InvalidOperationException(SR.GetString("SmtpDataStreamOpen"));
            }
            conn.BufferBuilder.Append(SmtpCommands.Data);
        }

        internal static void Send(SmtpConnection conn)
        {
            string str;
            PrepareCommand(conn);
            CheckResponse(CheckCommand.Send(conn, out str), str);
        }
    }
}

