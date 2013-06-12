namespace System.Configuration.Assemblies
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential), Obsolete("The AssemblyHash class has been deprecated. http://go.microsoft.com/fwlink/?linkid=14202"), ComVisible(true)]
    public struct AssemblyHash : ICloneable
    {
        private AssemblyHashAlgorithm _Algorithm;
        private byte[] _Value;
        [Obsolete("The AssemblyHash class has been deprecated. http://go.microsoft.com/fwlink/?linkid=14202")]
        public static readonly AssemblyHash Empty;
        [Obsolete("The AssemblyHash class has been deprecated. http://go.microsoft.com/fwlink/?linkid=14202")]
        public AssemblyHash(byte[] value)
        {
            this._Algorithm = AssemblyHashAlgorithm.SHA1;
            this._Value = null;
            if (value != null)
            {
                int length = value.Length;
                this._Value = new byte[length];
                Array.Copy(value, this._Value, length);
            }
        }

        [Obsolete("The AssemblyHash class has been deprecated. http://go.microsoft.com/fwlink/?linkid=14202")]
        public AssemblyHash(AssemblyHashAlgorithm algorithm, byte[] value)
        {
            this._Algorithm = algorithm;
            this._Value = null;
            if (value != null)
            {
                int length = value.Length;
                this._Value = new byte[length];
                Array.Copy(value, this._Value, length);
            }
        }

        [Obsolete("The AssemblyHash class has been deprecated. http://go.microsoft.com/fwlink/?linkid=14202")]
        public AssemblyHashAlgorithm Algorithm
        {
            get
            {
                return this._Algorithm;
            }
            set
            {
                this._Algorithm = value;
            }
        }
        [Obsolete("The AssemblyHash class has been deprecated. http://go.microsoft.com/fwlink/?linkid=14202")]
        public byte[] GetValue()
        {
            return this._Value;
        }

        [Obsolete("The AssemblyHash class has been deprecated. http://go.microsoft.com/fwlink/?linkid=14202")]
        public void SetValue(byte[] value)
        {
            this._Value = value;
        }

        [Obsolete("The AssemblyHash class has been deprecated. http://go.microsoft.com/fwlink/?linkid=14202")]
        public object Clone()
        {
            return new AssemblyHash(this._Algorithm, this._Value);
        }

        static AssemblyHash()
        {
            Empty = new AssemblyHash(AssemblyHashAlgorithm.None, null);
        }
    }
}

