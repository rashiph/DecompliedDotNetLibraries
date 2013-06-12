namespace System.Runtime.InteropServices
{
    using System;

    [Serializable, ComVisible(true)]
    public sealed class CurrencyWrapper
    {
        private decimal m_WrappedObject;

        public CurrencyWrapper(decimal obj)
        {
            this.m_WrappedObject = obj;
        }

        public CurrencyWrapper(object obj)
        {
            if (!(obj is decimal))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeDecimal"), "obj");
            }
            this.m_WrappedObject = (decimal) obj;
        }

        public decimal WrappedObject
        {
            get
            {
                return this.m_WrappedObject;
            }
        }
    }
}

