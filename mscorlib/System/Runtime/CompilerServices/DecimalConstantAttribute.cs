namespace System.Runtime.CompilerServices
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field, Inherited=false), ComVisible(true)]
    public sealed class DecimalConstantAttribute : Attribute
    {
        private decimal dec;

        public DecimalConstantAttribute(byte scale, byte sign, int hi, int mid, int low)
        {
            this.dec = new decimal(low, mid, hi, sign != 0, scale);
        }

        [CLSCompliant(false)]
        public DecimalConstantAttribute(byte scale, byte sign, uint hi, uint mid, uint low)
        {
            this.dec = new decimal((int) low, (int) mid, (int) hi, sign != 0, scale);
        }

        public decimal Value
        {
            get
            {
                return this.dec;
            }
        }
    }
}

