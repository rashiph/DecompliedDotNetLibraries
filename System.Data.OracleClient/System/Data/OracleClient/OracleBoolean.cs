namespace System.Data.OracleClient
{
    using System;
    using System.Data.Common;
    using System.Globalization;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct OracleBoolean : IComparable
    {
        private const byte x_Null = 0;
        private const byte x_True = 1;
        private const byte x_False = 2;
        private byte _value;
        public static readonly OracleBoolean False;
        public static readonly OracleBoolean Null;
        public static readonly OracleBoolean One;
        public static readonly OracleBoolean True;
        public static readonly OracleBoolean Zero;
        public OracleBoolean(bool value)
        {
            this._value = value ? ((byte) 1) : ((byte) 2);
        }

        public OracleBoolean(int value) : this(value, false)
        {
        }

        private OracleBoolean(int value, bool isNull)
        {
            if (isNull)
            {
                this._value = 0;
            }
            else
            {
                this._value = (value != 0) ? ((byte) 1) : ((byte) 2);
            }
        }

        private byte ByteValue
        {
            get
            {
                return this._value;
            }
        }
        public bool IsFalse
        {
            get
            {
                return (this._value == 2);
            }
        }
        public bool IsNull
        {
            get
            {
                return (this._value == 0);
            }
        }
        public bool IsTrue
        {
            get
            {
                return (this._value == 1);
            }
        }
        public bool Value
        {
            get
            {
                switch (this._value)
                {
                    case 1:
                        return true;

                    case 2:
                        return false;
                }
                throw System.Data.Common.ADP.DataIsNull();
            }
        }
        public int CompareTo(object obj)
        {
            if (!(obj is OracleBoolean))
            {
                throw System.Data.Common.ADP.WrongType(obj.GetType(), typeof(OracleBoolean));
            }
            OracleBoolean flag = (OracleBoolean) obj;
            if (this.IsNull)
            {
                if (!flag.IsNull)
                {
                    return -1;
                }
                return 0;
            }
            if (flag.IsNull)
            {
                return 1;
            }
            if (this.ByteValue < flag.ByteValue)
            {
                return -1;
            }
            if (this.ByteValue > flag.ByteValue)
            {
                return 1;
            }
            return 0;
        }

        public override bool Equals(object value)
        {
            if (!(value is OracleBoolean))
            {
                return false;
            }
            OracleBoolean flag = (OracleBoolean) value;
            if (flag.IsNull || this.IsNull)
            {
                return (flag.IsNull && this.IsNull);
            }
            OracleBoolean flag2 = this == flag;
            return flag2.Value;
        }

        public override int GetHashCode()
        {
            if (!this.IsNull)
            {
                return this._value.GetHashCode();
            }
            return 0;
        }

        public static OracleBoolean Parse(string s)
        {
            OracleBoolean flag;
            try
            {
                flag = new OracleBoolean(int.Parse(s, CultureInfo.InvariantCulture));
            }
            catch (Exception exception)
            {
                Type type = exception.GetType();
                if ((!(type == System.Data.Common.ADP.ArgumentNullExceptionType) && !(type == System.Data.Common.ADP.FormatExceptionType)) && !(type == System.Data.Common.ADP.OverflowExceptionType))
                {
                    throw exception;
                }
                return new OracleBoolean(bool.Parse(s));
            }
            return flag;
        }

        public override string ToString()
        {
            if (this.IsNull)
            {
                return System.Data.Common.ADP.NullString;
            }
            return this.Value.ToString(CultureInfo.CurrentCulture);
        }

        public static OracleBoolean And(OracleBoolean x, OracleBoolean y)
        {
            return (x & y);
        }

        public static OracleBoolean Equals(OracleBoolean x, OracleBoolean y)
        {
            return (x == y);
        }

        public static OracleBoolean NotEquals(OracleBoolean x, OracleBoolean y)
        {
            return (x != y);
        }

        public static OracleBoolean OnesComplement(OracleBoolean x)
        {
            return ~x;
        }

        public static OracleBoolean Or(OracleBoolean x, OracleBoolean y)
        {
            return (x | y);
        }

        public static OracleBoolean Xor(OracleBoolean x, OracleBoolean y)
        {
            return (x ^ y);
        }

        public static implicit operator OracleBoolean(bool x)
        {
            return new OracleBoolean(x);
        }

        public static explicit operator OracleBoolean(string x)
        {
            return Parse(x);
        }

        public static explicit operator OracleBoolean(OracleNumber x)
        {
            if (!x.IsNull)
            {
                return new OracleBoolean(x.Value != 0M);
            }
            return Null;
        }

        public static explicit operator bool(OracleBoolean x)
        {
            return x.Value;
        }

        public static OracleBoolean op_LogicalNot(OracleBoolean x)
        {
            switch (x._value)
            {
                case 1:
                    return False;

                case 2:
                    return True;
            }
            return Null;
        }

        public static OracleBoolean operator ~(OracleBoolean x)
        {
            return !x;
        }

        public static bool operator true(OracleBoolean x)
        {
            return x.IsTrue;
        }

        public static bool operator false(OracleBoolean x)
        {
            return x.IsFalse;
        }

        public static OracleBoolean operator &(OracleBoolean x, OracleBoolean y)
        {
            if ((x._value == 2) || (y._value == 2))
            {
                return False;
            }
            if ((x._value == 1) && (y._value == 1))
            {
                return True;
            }
            return Null;
        }

        public static OracleBoolean operator ==(OracleBoolean x, OracleBoolean y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new OracleBoolean(x._value == y._value);
            }
            return Null;
        }

        public static OracleBoolean operator !=(OracleBoolean x, OracleBoolean y)
        {
            return !(x == y);
        }

        public static OracleBoolean operator |(OracleBoolean x, OracleBoolean y)
        {
            if ((x._value == 1) || (y._value == 1))
            {
                return True;
            }
            if ((x._value == 2) && (y._value == 2))
            {
                return False;
            }
            return Null;
        }

        public static OracleBoolean operator ^(OracleBoolean x, OracleBoolean y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new OracleBoolean(x._value != y._value);
            }
            return Null;
        }

        static OracleBoolean()
        {
            False = new OracleBoolean(false);
            Null = new OracleBoolean(0, true);
            One = new OracleBoolean(1);
            True = new OracleBoolean(true);
            Zero = new OracleBoolean(0);
        }
    }
}

