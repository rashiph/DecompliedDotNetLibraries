namespace System
{
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    [Serializable, StructLayout(LayoutKind.Sequential), ForceTokenStabilization]
    internal struct Currency
    {
        internal long m_value;
        [ForceTokenStabilization]
        public Currency(decimal value)
        {
            this.m_value = decimal.ToCurrency(value).m_value;
        }

        internal Currency(long value, int ignored)
        {
            this.m_value = value;
        }

        public static Currency FromOACurrency(long cy)
        {
            return new Currency(cy, 0);
        }

        public long ToOACurrency()
        {
            return this.m_value;
        }

        [SecuritySafeCritical]
        public static decimal ToDecimal(Currency c)
        {
            decimal result = 0M;
            FCallToDecimal(ref result, c);
            return result;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void FCallToDecimal(ref decimal result, Currency c);
    }
}

