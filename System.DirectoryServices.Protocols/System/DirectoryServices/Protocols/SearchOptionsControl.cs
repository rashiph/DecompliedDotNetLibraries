namespace System.DirectoryServices.Protocols
{
    using System;
    using System.ComponentModel;

    public class SearchOptionsControl : DirectoryControl
    {
        private System.DirectoryServices.Protocols.SearchOption flag;

        public SearchOptionsControl() : base("1.2.840.113556.1.4.1340", null, true, true)
        {
            this.flag = System.DirectoryServices.Protocols.SearchOption.DomainScope;
        }

        public SearchOptionsControl(System.DirectoryServices.Protocols.SearchOption flags) : this()
        {
            this.SearchOption = flags;
        }

        public override byte[] GetValue()
        {
            base.directoryControlValue = BerConverter.Encode("{i}", new object[] { (int) this.flag });
            return base.GetValue();
        }

        public System.DirectoryServices.Protocols.SearchOption SearchOption
        {
            get
            {
                return this.flag;
            }
            set
            {
                if ((value < System.DirectoryServices.Protocols.SearchOption.DomainScope) || (value > System.DirectoryServices.Protocols.SearchOption.PhantomRoot))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.DirectoryServices.Protocols.SearchOption));
                }
                this.flag = value;
            }
        }
    }
}

