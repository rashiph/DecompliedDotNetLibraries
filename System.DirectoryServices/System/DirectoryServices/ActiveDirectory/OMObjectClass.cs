namespace System.DirectoryServices.ActiveDirectory
{
    using System;

    internal class OMObjectClass
    {
        public byte[] data;

        public OMObjectClass(byte[] data)
        {
            this.data = data;
        }

        public bool Equals(OMObjectClass OMObjectClass)
        {
            bool flag = true;
            if (this.data.Length == OMObjectClass.data.Length)
            {
                for (int i = 0; i < this.data.Length; i++)
                {
                    if (this.data[i] != OMObjectClass.data[i])
                    {
                        return false;
                    }
                }
                return flag;
            }
            return false;
        }

        public byte[] Data
        {
            get
            {
                return this.data;
            }
        }
    }
}

