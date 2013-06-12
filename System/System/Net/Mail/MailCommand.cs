namespace System.Net.Mail
{
    using System;

    internal static class MailCommand
    {
        internal static IAsyncResult BeginSend(SmtpConnection conn, byte[] command, string from, AsyncCallback callback, object state)
        {
            PrepareCommand(conn, command, from);
            return CheckCommand.BeginSend(conn, callback, state);
        }

        private static void CheckResponse(SmtpStatusCode statusCode, string response)
        {
            switch (statusCode)
            {
                case SmtpStatusCode.Ok:
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

        private static void PrepareCommand(SmtpConnection conn, byte[] command, string from)
        {
            if (conn.IsStreamOpen)
            {
                throw new InvalidOperationException(SR.GetString("SmtpDataStreamOpen"));
            }
            conn.BufferBuilder.Append(command);
            conn.BufferBuilder.Append(from);
            conn.BufferBuilder.Append(SmtpCommands.CRLF);
        }

        internal static void Send(SmtpConnection conn, byte[] command, string from)
        {
            string str;
            PrepareCommand(conn, command, from);
            CheckResponse(CheckCommand.Send(conn, out str), str);
        }
    }
}

