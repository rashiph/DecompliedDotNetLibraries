namespace System.Reflection.Emit
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential), ComVisible(true)]
    public struct SignatureToken
    {
        public static readonly SignatureToken Empty;
        internal int m_signature;
        internal ModuleBuilder m_moduleBuilder;
        internal SignatureToken(int str, ModuleBuilder mod)
        {
            this.m_signature = str;
            this.m_moduleBuilder = mod;
        }

        public int Token
        {
            get
            {
                return this.m_signature;
            }
        }
        public override int GetHashCode()
        {
            return this.m_signature;
        }

        public override bool Equals(object obj)
        {
            return ((obj is SignatureToken) && this.Equals((SignatureToken) obj));
        }

        public bool Equals(SignatureToken obj)
        {
            return (obj.m_signature == this.m_signature);
        }

        public static bool operator ==(SignatureToken a, SignatureToken b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(SignatureToken a, SignatureToken b)
        {
            return !(a == b);
        }

        static SignatureToken()
        {
            Empty = new SignatureToken();
        }
    }
}

