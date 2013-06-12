namespace System.Net.Mail
{
    using System;

    internal static class StartTlsCommand
    {
        internal static IAsyncResult BeginSend(SmtpConnection conn, AsyncCallback callback, object state)
        {
            PrepareCommand(conn);
            return CheckCommand.BeginSend(conn, callback, state);
        }

        private static void CheckResponse(SmtpStatusCode statusCode, string response)
        {
            switch (statusCode)
            {
                case SmtpStatusCode.ServiceReady:
                    return;
            }
            if (statusCode < ((SmtpStatusCode) 400))
            {
                throw new SmtpException(SR.GetString("net_webstatus_ServerProtocolViolation"), response);
            }
            throw new SmtpException(statusCode, response, true);
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
            conn.BufferBuilder.Append(SmtpCommands.StartTls);
            conn.BufferBuilder.Append(SmtpCommands.CRLF);
        }

        internal static void Send(SmtpConnection conn)
        {
            string str;
            PrepareCommand(conn);
            CheckResponse(CheckCommand.Send(conn, out str), str);
        }
    }
}

