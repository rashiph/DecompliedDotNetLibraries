namespace System.Drawing.Printing
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential)]
    internal struct TriState
    {
        private byte value;
        public static readonly TriState Default;
        public static readonly TriState False;
        public static readonly TriState True;
        private TriState(byte value)
        {
            this.value = value;
        }

        public bool IsDefault
        {
            get
            {
                return (this == Default);
            }
        }
        public bool IsFalse
        {
            get
            {
                return (this == False);
            }
        }
        public bool IsNotDefault
        {
            get
            {
                return (this != Default);
            }
        }
        public bool IsTrue
        {
            get
            {
                return (this == True);
            }
        }
        public static bool operator ==(TriState left, TriState right)
        {
            return (left.value == right.value);
        }

        public static bool operator !=(TriState left, TriState right)
        {
            return !(left == right);
        }

        public override bool Equals(object o)
        {
            TriState state = (TriState) o;
            return (this.value == state.value);
        }

        public override int GetHashCode()
        {
            return this.value;
        }

        public static implicit operator TriState(bool value)
        {
            if (!value)
            {
                return False;
            }
            return True;
        }

        public static explicit operator bool(TriState value)
        {
            if (value.IsDefault)
            {
                throw new InvalidCastException(System.Drawing.SR.GetString("TriStateCompareError"));
            }
            return (value == True);
        }

        public override string ToString()
        {
            if (this == Default)
            {
                return "Default";
            }
            if (this == False)
            {
                return "False";
            }
            return "True";
        }

        static TriState()
        {
            Default = new TriState(0);
            False = new TriState(1);
            True = new TriState(2);
        }
    }
}

