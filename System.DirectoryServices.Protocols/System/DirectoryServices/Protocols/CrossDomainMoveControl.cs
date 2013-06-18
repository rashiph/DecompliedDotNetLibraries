namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Text;

    public class CrossDomainMoveControl : DirectoryControl
    {
        private string dcName;

        public CrossDomainMoveControl() : base("1.2.840.113556.1.4.521", null, true, true)
        {
        }

        public CrossDomainMoveControl(string targetDomainController) : this()
        {
            this.dcName = targetDomainController;
        }

        public override byte[] GetValue()
        {
            if (this.dcName != null)
            {
                byte[] bytes = new UTF8Encoding().GetBytes(this.dcName);
                base.directoryControlValue = new byte[bytes.Length + 2];
                for (int i = 0; i < bytes.Length; i++)
                {
                    base.directoryControlValue[i] = bytes[i];
                }
            }
            return base.GetValue();
        }

        public string TargetDomainController
        {
            get
            {
                return this.dcName;
            }
            set
            {
                this.dcName = value;
            }
        }
    }
}

