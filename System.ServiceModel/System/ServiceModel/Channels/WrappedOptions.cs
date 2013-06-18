namespace System.ServiceModel.Channels
{
    using System;

    public class WrappedOptions
    {
        private bool wrappedFlag;

        public bool WrappedFlag
        {
            get
            {
                return this.wrappedFlag;
            }
            set
            {
                this.wrappedFlag = value;
            }
        }
    }
}

