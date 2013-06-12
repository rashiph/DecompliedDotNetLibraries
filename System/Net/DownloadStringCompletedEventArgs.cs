namespace System.Net
{
    using System;
    using System.ComponentModel;

    public class DownloadStringCompletedEventArgs : AsyncCompletedEventArgs
    {
        private string m_Result;

        internal DownloadStringCompletedEventArgs(string result, Exception exception, bool cancelled, object userToken) : base(exception, cancelled, userToken)
        {
            this.m_Result = result;
        }

        public string Result
        {
            get
            {
                base.RaiseExceptionIfNecessary();
                return this.m_Result;
            }
        }
    }
}

