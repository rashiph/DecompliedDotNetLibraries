namespace System.Net
{
    using System;
    using System.ComponentModel;
    using System.IO;

    public class OpenWriteCompletedEventArgs : AsyncCompletedEventArgs
    {
        private Stream m_Result;

        internal OpenWriteCompletedEventArgs(Stream result, Exception exception, bool cancelled, object userToken) : base(exception, cancelled, userToken)
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

