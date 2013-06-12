namespace System.Net.Mail
{
    using System;
    using System.Runtime.InteropServices;

    internal static class RecipientCommand
    {
        internal static IAsyncResult BeginSend(SmtpConnection conn, string to, AsyncCallback callback, object state)
        {
            PrepareCommand(conn, to);
            return CheckCommand.BeginSend(conn, callback, state);
        }

        private static bool CheckResponse(SmtpStatusCode statusCode, string response)
        {
            switch (statusCode)
            {
                case SmtpStatusCode.Ok:
                case SmtpStatusCode.UserNotLocalWillForward:
                    return true;

                case SmtpStatusCode.MailboxBusy:
                case SmtpStatusCode.InsufficientStorage:
                case SmtpStatusCode.MailboxUnavailable:
                case SmtpStatusCode.UserNotLocalTryAlternatePath:
                case SmtpStatusCode.ExceededStorageAllocation:
                case SmtpStatusCode.MailboxNameNotAllowed:
                    return false;
            }
            if (statusCode < ((SmtpStatusCode) 400))
            {
                throw new SmtpException(SR.GetString("net_webstatus_ServerProtocolViolation"), response);
            }
            throw new SmtpException(statusCode, response, true);
        }

        internal static bool EndSend(IAsyncResult result, out string response)
        {
            SmtpStatusCode statusCode = (SmtpStatusCode) CheckCommand.EndSend(result, out response);
            return CheckResponse(statusCode, response);
        }

        private static void PrepareCommand(SmtpConnection conn, string to)
        {
            if (conn.IsStreamOpen)
            {
                throw new InvalidOperationException(SR.GetString("SmtpDataStreamOpen"));
            }
            conn.BufferBuilder.Append(SmtpCommands.Recipient);
            conn.BufferBuilder.Append(to);
            conn.BufferBuilder.Append(SmtpCommands.CRLF);
        }

        internal static bool Send(SmtpConnection conn, string to, out string response)
        {
            PrepareCommand(conn, to);
            return CheckResponse(CheckCommand.Send(conn, out response), response);
        }
    }
}

