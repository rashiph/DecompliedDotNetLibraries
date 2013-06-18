namespace System.Drawing
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct CharacterRange
    {
        private int first;
        private int length;
        public CharacterRange(int First, int Length)
        {
            this.first = First;
            this.length = Length;
        }

        public int First
        {
            get
            {
                return this.first;
            }
            set
            {
                this.first = value;
            }
        }
        public int Length
        {
            get
            {
                return this.length;
            }
            set
            {
                this.length = value;
            }
        }
        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(CharacterRange))
            {
                return false;
            }
            CharacterRange range = (CharacterRange) obj;
            return ((this.first == range.First) && (this.length == range.Length));
        }

        public static bool operator ==(CharacterRange cr1, CharacterRange cr2)
        {
            return ((cr1.First == cr2.First) && (cr1.Length == cr2.Length));
        }

        public static bool operator !=(CharacterRange cr1, CharacterRange cr2)
        {
            return !(cr1 == cr2);
        }

        public override int GetHashCode()
        {
            return (this.first << (8 + this.length));
        }
    }
}

