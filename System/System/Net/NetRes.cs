namespace System.Net
{
    using System;
    using System.Globalization;

    internal class NetRes
    {
        private NetRes()
        {
        }

        public static string GetWebStatusCodeString(FtpStatusCode statusCode, string statusDescription)
        {
            string str = "(" + ((int) statusCode).ToString(NumberFormatInfo.InvariantInfo) + ")";
            string str2 = null;
            try
            {
                str2 = SR.GetString("net_ftpstatuscode_" + statusCode.ToString(), (object[]) null);
            }
            catch
            {
            }
            if ((str2 != null) && (str2.Length > 0))
            {
                return (str + " " + str2);
            }
            if ((statusDescription != null) && (statusDescription.Length > 0))
            {
                str = str + " " + statusDescription;
            }
            return str;
        }

        public static string GetWebStatusCodeString(HttpStatusCode statusCode, string statusDescription)
        {
            string str = "(" + ((int) statusCode).ToString(NumberFormatInfo.InvariantInfo) + ")";
            string str2 = null;
            try
            {
                str2 = SR.GetString("net_httpstatuscode_" + statusCode.ToString(), (object[]) null);
            }
            catch
            {
            }
            if ((str2 != null) && (str2.Length > 0))
            {
                return (str + " " + str2);
            }
            if ((statusDescription != null) && (statusDescription.Length > 0))
            {
                str = str + " " + statusDescription;
            }
            return str;
        }

        public static string GetWebStatusString(WebExceptionStatus Status)
        {
            return SR.GetString(WebExceptionMapping.GetWebStatusString(Status));
        }

        public static string GetWebStatusString(string Res, WebExceptionStatus Status)
        {
            string str2 = SR.GetString(WebExceptionMapping.GetWebStatusString(Status));
            string format = SR.GetString(Res);
            return string.Format(CultureInfo.CurrentCulture, format, new object[] { str2 });
        }
    }
}

