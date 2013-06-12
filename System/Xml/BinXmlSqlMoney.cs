namespace System.Xml
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct BinXmlSqlMoney
    {
        private long data;
        public BinXmlSqlMoney(int v)
        {
            this.data = v;
        }

        public BinXmlSqlMoney(long v)
        {
            this.data = v;
        }

        public decimal ToDecimal()
        {
            bool flag;
            ulong data;
            if (this.data < 0L)
            {
                flag = true;
                data = (ulong) -this.data;
            }
            else
            {
                flag = false;
                data = (ulong) this.data;
            }
            return new decimal((int) data, (int) (data >> 0x20), 0, flag, 4);
        }

        public override string ToString()
        {
            return this.ToDecimal().ToString("#0.00##", CultureInfo.InvariantCulture);
        }
    }
}

