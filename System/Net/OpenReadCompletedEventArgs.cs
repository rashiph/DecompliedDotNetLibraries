namespace System.Net
{
    using System;
    using System.ComponentModel;
    using System.IO;

    public class OpenReadCompletedEventArgs : AsyncCompletedEventArgs
    {
        private Stream m_Result;

        internal OpenReadCompletedEventArgs(Stream result, Exception exception, bool cancelled, object userToken) : base(exception, cancelled, userToken)
        {
            this.m_Result = result;
        }

        public Stream Result
        {
            get
            {
                base.RaiseExceptionIfNecessary();
                return this.m_Result;
            }
        }
    }
}

