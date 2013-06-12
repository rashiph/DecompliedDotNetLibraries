namespace System.Net.Mail
{
    using System;

    internal static class HelloCommand
    {
        internal static IAsyncResult BeginSend(SmtpConnection conn, string domain, AsyncCallback callback, object state)
        {
            PrepareCommand(conn, domain);
            return CheckCommand.BeginSend(conn, callback, state);
        }

        private static void CheckResponse(SmtpStatusCode statusCode, string serverResponse)
        {
            if (statusCode != SmtpStatusCode.Ok)
            {
                if (statusCode < ((SmtpStatusCode) 400))
                {
                    throw new SmtpException(SR.GetString("net_webstatus_ServerProtocolViolation"), serverResponse);
                }
                throw new SmtpException(statusCode, serverResponse, true);
            }
        }

        internal static void EndSend(IAsyncResult result)
        {
            string str;
            SmtpStatusCode statusCode = (SmtpStatusCode) CheckCommand.EndSend(result, out str);
            CheckResponse(statusCode, str);
        }

        private static void PrepareCommand(SmtpConnection conn, string domain)
        {
            if (conn.IsStreamOpen)
            {
                throw new InvalidOperationException(SR.GetString("SmtpDataStreamOpen"));
            }
            conn.BufferBuilder.Append(SmtpCommands.Hello);
            conn.BufferBuilder.Append(domain);
            conn.BufferBuilder.Append(SmtpCommands.CRLF);
        }

        internal static void Send(SmtpConnection conn, string domain)
        {
            string str;
            PrepareCommand(conn, domain);
            CheckResponse(CheckCommand.Send(conn, out str), str);
        }
    }
}

