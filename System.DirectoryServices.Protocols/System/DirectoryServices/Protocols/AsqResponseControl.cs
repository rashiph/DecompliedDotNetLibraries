namespace System.DirectoryServices.Protocols
{
    using System;

    public class AsqResponseControl : DirectoryControl
    {
        private ResultCode result;

        internal AsqResponseControl(int result, bool criticality, byte[] controlValue) : base("1.2.840.113556.1.4.1504", controlValue, criticality, true)
        {
            this.result = (ResultCode) result;
        }

        public ResultCode Result
        {
            get
            {
                return this.result;
            }
        }
    }
}

