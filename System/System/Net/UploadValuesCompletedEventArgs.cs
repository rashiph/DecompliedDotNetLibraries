namespace System.Net
{
    using System;
    using System.ComponentModel;

    public class UploadValuesCompletedEventArgs : AsyncCompletedEventArgs
    {
        private byte[] m_Result;

        internal UploadValuesCompletedEventArgs(byte[] result, Exception exception, bool cancelled, object userToken) : base(exception, cancelled, userToken)
        {
            this.m_Result = result;
        }

        public byte[] Result
        {
            get
            {
                base.RaiseExceptionIfNecessary();
                return this.m_Result;
            }
        }
    }
}

