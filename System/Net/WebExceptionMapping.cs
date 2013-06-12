namespace System.Net
{
    using System;

    internal static class WebExceptionMapping
    {
        private static readonly string[] s_Mapping = new string[0x15];

        internal static string GetWebStatusString(WebExceptionStatus status)
        {
            int index = (int) status;
            if ((index >= s_Mapping.Length) || (index < 0))
            {
                throw new InternalException();
            }
            string str = s_Mapping[index];
            if (str == null)
            {
                str = "net_webstatus_" + status.ToString();
                s_Mapping[index] = str;
            }
            return str;
        }
    }
}

