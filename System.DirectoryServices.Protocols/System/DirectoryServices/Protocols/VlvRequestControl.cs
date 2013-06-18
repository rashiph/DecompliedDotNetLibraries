namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Collections;
    using System.Text;

    public class VlvRequestControl : DirectoryControl
    {
        private int after;
        private int before;
        private byte[] context;
        private int estimateCount;
        private int offset;
        private byte[] target;

        public VlvRequestControl() : base("2.16.840.1.113730.3.4.9", null, true, true)
        {
        }

        public VlvRequestControl(int beforeCount, int afterCount, int offset) : this()
        {
            this.BeforeCount = beforeCount;
            this.AfterCount = afterCount;
            this.Offset = offset;
        }

        public VlvRequestControl(int beforeCount, int afterCount, string target) : this()
        {
            this.BeforeCount = beforeCount;
            this.AfterCount = afterCount;
            if (target != null)
            {
                byte[] bytes = new UTF8Encoding().GetBytes(target);
                this.target = bytes;
            }
        }

        public VlvRequestControl(int beforeCount, int afterCount, byte[] target) : this()
        {
            this.BeforeCount = beforeCount;
            this.AfterCount = afterCount;
            this.Target = target;
        }

        public override byte[] GetValue()
        {
            StringBuilder builder = new StringBuilder(10);
            ArrayList list = new ArrayList();
            builder.Append("{ii");
            list.Add(this.BeforeCount);
            list.Add(this.AfterCount);
            if (this.Target.Length != 0)
            {
                builder.Append("t");
                list.Add(0x81);
                builder.Append("o");
                list.Add(this.Target);
            }
            else
            {
                builder.Append("t{");
                list.Add(160);
                builder.Append("ii");
                list.Add(this.Offset);
                list.Add(this.EstimateCount);
                builder.Append("}");
            }
            if (this.ContextId.Length != 0)
            {
                builder.Append("o");
                list.Add(this.ContextId);
            }
            builder.Append("}");
            object[] objArray = new object[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                objArray[i] = list[i];
            }
            base.directoryControlValue = BerConverter.Encode(builder.ToString(), objArray);
            return base.GetValue();
        }

        public int AfterCount
        {
            get
            {
                return this.after;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException(Res.GetString("ValidValue"), "value");
                }
                this.after = value;
            }
        }

        public int BeforeCount
        {
            get
            {
                return this.before;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException(Res.GetString("ValidValue"), "value");
                }
                this.before = value;
            }
        }

        public byte[] ContextId
        {
            get
            {
                if (this.context == null)
                {
                    return new byte[0];
                }
                byte[] buffer = new byte[this.context.Length];
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = this.context[i];
                }
                return buffer;
            }
            set
            {
                this.context = value;
            }
        }

        public int EstimateCount
        {
            get
            {
                return this.estimateCount;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException(Res.GetString("ValidValue"), "value");
                }
                this.estimateCount = value;
            }
        }

        public int Offset
        {
            get
            {
                return this.offset;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException(Res.GetString("ValidValue"), "value");
                }
                this.offset = value;
            }
        }

        public byte[] Target
        {
            get
            {
                if (this.target == null)
                {
                    return new byte[0];
                }
                byte[] buffer = new byte[this.target.Length];
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = this.target[i];
                }
                return buffer;
            }
            set
            {
                this.target = value;
            }
        }
    }
}

