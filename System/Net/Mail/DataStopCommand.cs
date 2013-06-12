namespace System.Net.Mail
{
    using System;

    internal static class DataStopCommand
    {
        private static void CheckResponse(SmtpStatusCode statusCode, string serverResponse)
        {
            switch (statusCode)
            {
                case SmtpStatusCode.Ok:
                    return;
            }
            if (statusCode < ((SmtpStatusCode) 400))
            {
                throw new SmtpException(SR.GetString("net_webstatus_ServerProtocolViolation"), serverResponse);
            }
            throw new SmtpException(statusCode, serverResponse, true);
        }

        private static void PrepareCommand(SmtpConnection conn)
        {
            if (conn.IsStreamOpen)
            {
                throw new InvalidOperationException(SR.GetString("SmtpDataStreamOpen"));
            }
            conn.BufferBuilder.Append(SmtpCommands.DataStop);
        }

        internal static void Send(SmtpConnection conn)
        {
            string str;
            PrepareCommand(conn);
            CheckResponse(CheckCommand.Send(conn, out str), str);
        }
    }
}

