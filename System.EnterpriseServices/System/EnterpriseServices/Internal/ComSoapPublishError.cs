namespace System.EnterpriseServices.Internal
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    public class ComSoapPublishError
    {
        public static void Report(string s)
        {
            try
            {
                new EventLog { Source = "COM+ SOAP Services" }.WriteEntry(s, EventLogEntryType.Warning);
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
            }
        }
    }
}

