namespace System.Net
{
    using System;
    using System.ComponentModel;

    public class UploadStringCompletedEventArgs : AsyncCompletedEventArgs
    {
        private string m_Result;

        internal UploadStringCompletedEventArgs(string result, Exception exception, bool cancelled, object userToken) : base(exception, cancelled, userToken)
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

