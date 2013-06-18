namespace System.ServiceModel
{
    using System;

    public class FaultImportOptions
    {
        private bool useMessageFormat;

        public bool UseMessageFormat
        {
            get
            {
                return this.useMessageFormat;
            }
            set
            {
                this.useMessageFormat = value;
            }
        }
    }
}

