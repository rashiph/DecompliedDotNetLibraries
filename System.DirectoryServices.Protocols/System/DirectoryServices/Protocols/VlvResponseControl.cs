namespace System.DirectoryServices.Protocols
{
    using System;

    public class VlvResponseControl : DirectoryControl
    {
        private byte[] context;
        private int count;
        private int position;
        private ResultCode result;

        internal VlvResponseControl(int targetPosition, int count, byte[] context, ResultCode result, bool criticality, byte[] value) : base("2.16.840.1.113730.3.4.10", value, criticality, true)
        {
            this.position = targetPosition;
            this.count = count;
            this.context = context;
            this.result = result;
        }

        public int ContentCount
        {
            get
            {
                return this.count;
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
        }

        public ResultCode Result
        {
            get
            {
                return this.result;
            }
        }

        public int TargetPosition
        {
            get
            {
                return this.position;
            }
        }
    }
}

