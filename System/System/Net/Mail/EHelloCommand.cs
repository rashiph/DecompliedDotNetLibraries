namespace System.Net.Mail
{
    using System;

    internal static class EHelloCommand
    {
        internal static IAsyncResult BeginSend(SmtpConnection conn, string domain, AsyncCallback callback, object state)
        {
            PrepareCommand(conn, domain);
            return ReadLinesCommand.BeginSend(conn, callback, state);
        }

        private static string[] CheckResponse(LineInfo[] lines)
        {
            if ((lines == null) || (lines.Length == 0))
            {
                throw new SmtpException(SR.GetString("SmtpEhloResponseInvalid"));
            }
            if (lines[0].StatusCode != SmtpStatusCode.Ok)
            {
                if (lines[0].StatusCode < ((SmtpStatusCode) 400))
                {
                    throw new SmtpException(SR.GetString("net_webstatus_ServerProtocolViolation"), lines[0].Line);
                }
                throw new SmtpException(lines[0].StatusCode, lines[0].Line, true);
            }
            string[] strArray = new string[lines.Length - 1];
            for (int i = 1; i < lines.Length; i++)
            {
                strArray[i - 1] = lines[i].Line;
            }
            return strArray;
        }

        internal static string[] EndSend(IAsyncResult result)
        {
            return CheckResponse(ReadLinesCommand.EndSend(result));
        }

        private static void PrepareCommand(SmtpConnection conn, string domain)
        {
            if (conn.IsStreamOpen)
            {
                throw new InvalidOperationException(SR.GetString("SmtpDataStreamOpen"));
            }
            conn.BufferBuilder.Append(SmtpCommands.EHello);
            conn.BufferBuilder.Append(domain);
            conn.BufferBuilder.Append(SmtpCommands.CRLF);
        }

        internal static string[] Send(SmtpConnection conn, string domain)
        {
            PrepareCommand(conn, domain);
            return CheckResponse(ReadLinesCommand.Send(conn));
        }
    }
}

