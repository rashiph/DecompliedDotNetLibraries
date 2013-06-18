namespace System.DirectoryServices.Protocols
{
    using System;

    public class DirSyncRequestControl : DirectoryControl
    {
        private int count;
        private byte[] dirsyncCookie;
        private DirectorySynchronizationOptions flag;

        public DirSyncRequestControl() : base("1.2.840.113556.1.4.841", null, true, true)
        {
            this.count = 0x100000;
        }

        public DirSyncRequestControl(byte[] cookie) : this()
        {
            this.dirsyncCookie = cookie;
        }

        public DirSyncRequestControl(byte[] cookie, DirectorySynchronizationOptions option) : this(cookie)
        {
            this.Option = option;
        }

        public DirSyncRequestControl(byte[] cookie, DirectorySynchronizationOptions option, int attributeCount) : this(cookie, option)
        {
            this.AttributeCount = attributeCount;
        }

        public override byte[] GetValue()
        {
            object[] objArray = new object[] { (int) this.flag, this.count, this.dirsyncCookie };
            base.directoryControlValue = BerConverter.Encode("{iio}", objArray);
            return base.GetValue();
        }

        public int AttributeCount
        {
            get
            {
                return this.count;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException(Res.GetString("ValidValue"), "value");
                }
                this.count = value;
            }
        }

        public byte[] Cookie
        {
            get
            {
                if (this.dirsyncCookie == null)
                {
                    return new byte[0];
                }
                byte[] buffer = new byte[this.dirsyncCookie.Length];
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = this.dirsyncCookie[i];
                }
                return buffer;
            }
            set
            {
                this.dirsyncCookie = value;
            }
        }

        public DirectorySynchronizationOptions Option
        {
            get
            {
                return this.flag;
            }
            set
            {
                this.flag = value;
            }
        }
    }
}

