namespace System.DirectoryServices.Protocols
{
    using System;

    public class AsqRequestControl : DirectoryControl
    {
        private string name;

        public AsqRequestControl() : base("1.2.840.113556.1.4.1504", null, true, true)
        {
        }

        public AsqRequestControl(string attributeName) : this()
        {
            this.name = attributeName;
        }

        public override byte[] GetValue()
        {
            base.directoryControlValue = BerConverter.Encode("{s}", new object[] { this.name });
            return base.GetValue();
        }

        public string AttributeName
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }
    }
}

