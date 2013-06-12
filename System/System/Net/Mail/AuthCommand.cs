namespace System.Net.Mail
{
    using System;

    internal static class AuthCommand
    {
        internal static IAsyncResult BeginSend(SmtpConnection conn, string message, AsyncCallback callback, object state)
        {
            PrepareCommand(conn, message);
            return ReadLinesCommand.BeginSend(conn, callback, state);
        }

        internal static IAsyncResult BeginSend(SmtpConnection conn, string type, string message, AsyncCallback callback, object state)
        {
            PrepareCommand(conn, type, message);
            return ReadLinesCommand.BeginSend(conn, callback, state);
        }

        private static LineInfo CheckResponse(LineInfo[] lines)
        {
            if ((lines == null) || (lines.Length == 0))
            {
                throw new SmtpException(SR.GetString("SmtpAuthResponseInvalid"));
            }
            return lines[0];
        }

        internal static LineInfo EndSend(IAsyncResult result)
        {
            return CheckResponse(ReadLinesCommand.EndSend(result));
        }

        private static void PrepareCommand(SmtpConnection conn, string message)
        {
            conn.BufferBuilder.Append(message);
            conn.BufferBuilder.Append(SmtpCommands.CRLF);
        }

        private static void PrepareCommand(SmtpConnection conn, string type, string message)
        {
            conn.BufferBuilder.Append(SmtpCommands.Auth);
            conn.BufferBuilder.Append(type);
            conn.BufferBuilder.Append((byte) 0x20);
            conn.BufferBuilder.Append(message);
            conn.BufferBuilder.Append(SmtpCommands.CRLF);
        }

        internal static LineInfo Send(SmtpConnection conn, string message)
        {
            PrepareCommand(conn, message);
            return CheckResponse(ReadLinesCommand.Send(conn));
        }

        internal static LineInfo Send(SmtpConnection conn, string type, string message)
        {
            PrepareCommand(conn, type, message);
            return CheckResponse(ReadLinesCommand.Send(conn));
        }
    }
}

