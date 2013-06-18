namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Globalization;

    internal class IntArithmeticLiteral : ArithmeticLiteral
    {
        private int m_value;

        internal IntArithmeticLiteral(int literalValue)
        {
            this.m_value = literalValue;
            base.m_type = typeof(int);
        }

        internal override object Add()
        {
            return null;
        }

        internal override object Add(char v)
        {
            return (v + this.m_value);
        }

        internal override object Add(decimal v)
        {
            return (v + this.m_value);
        }

        internal override object Add(double v)
        {
            return (v + this.m_value);
        }

        internal override object Add(int v)
        {
            return (v + this.m_value);
        }

        internal override object Add(long v)
        {
            return (v + this.m_value);
        }

        internal override object Add(float v)
        {
            return (v + this.m_value);
        }

        internal override object Add(string v)
        {
            return (v + this.m_value.ToString(CultureInfo.CurrentCulture));
        }

        internal override object Add(ushort v)
        {
            return (v + this.m_value);
        }

        internal override object Add(uint v)
        {
            return (v + this.m_value);
        }

        internal override object Add(ulong v)
        {
            if (this.m_value < 0)
            {
                return base.Add(v);
            }
            return (v + this.m_value);
        }

        internal override object Add(ArithmeticLiteral v)
        {
            return v.Add(this.m_value);
        }

        internal override object BitAnd()
        {
            return null;
        }

        internal override object BitAnd(int v)
        {
            return (v & this.m_value);
        }

        internal override object BitAnd(long v)
        {
            return (v & this.m_value);
        }

        internal override object BitAnd(ushort v)
        {
            return (v & this.m_value);
        }

        internal override object BitAnd(uint v)
        {
            return (v & this.m_value);
        }

        internal override object BitAnd(ulong v)
        {
            if (this.m_value < 0)
            {
                return base.BitAnd(v);
            }
            return (v & this.m_value);
        }

        internal override object BitAnd(ArithmeticLiteral v)
        {
            return v.BitAnd(this.m_value);
        }

        internal override object BitOr()
        {
            return null;
        }

        internal override object BitOr(int v)
        {
            return (v | this.m_value);
        }

        internal override object BitOr(long v)
        {
            long num = this.m_value;
            return (v | num);
        }

        internal override object BitOr(ushort v)
        {
            return (v | this.m_value);
        }

        internal override object BitOr(uint v)
        {
            long num = this.m_value;
            return (v | num);
        }

        internal override object BitOr(ulong v)
        {
            long num = this.m_value;
            if (num < 0L)
            {
                return base.BitOr(v);
            }
            return (v | ((ulong) num));
        }

        internal override object BitOr(ArithmeticLiteral v)
        {
            return v.BitOr(this.m_value);
        }

        internal override object Divide()
        {
            return null;
        }

        internal override object Divide(decimal v)
        {
            return (v / this.m_value);
        }

        internal override object Divide(double v)
        {
            return (v / ((double) this.m_value));
        }

        internal override object Divide(int v)
        {
            return (v / this.m_value);
        }

        internal override object Divide(long v)
        {
            return (v / ((long) this.m_value));
        }

        internal override object Divide(float v)
        {
            return (v / ((float) this.m_value));
        }

        internal override object Divide(ushort v)
        {
            return (v / this.m_value);
        }

        internal override object Divide(uint v)
        {
            return (long) (((ulong) v) / ((long) this.m_value));
        }

        internal override object Divide(ulong v)
        {
            if (this.m_value < 0)
            {
                return base.Divide(v);
            }
            return (v / ((long) this.m_value));
        }

        internal override object Divide(ArithmeticLiteral v)
        {
            return v.Divide(this.m_value);
        }

        internal override object Modulus()
        {
            return null;
        }

        internal override object Modulus(decimal v)
        {
            return (v % this.m_value);
        }

        internal override object Modulus(double v)
        {
            return (v % ((double) this.m_value));
        }

        internal override object Modulus(int v)
        {
            return (v % this.m_value);
        }

        internal override object Modulus(long v)
        {
            return (v % ((long) this.m_value));
        }

        internal override object Modulus(float v)
        {
            return (v % ((float) this.m_value));
        }

        internal override object Modulus(ushort v)
        {
            return (v % this.m_value);
        }

        internal override object Modulus(uint v)
        {
            return (long) (((ulong) v) % ((long) this.m_value));
        }

        internal override object Modulus(ulong v)
        {
            if (this.m_value < 0)
            {
                return base.Modulus(v);
            }
            return (v % ((long) this.m_value));
        }

        internal override object Modulus(ArithmeticLiteral v)
        {
            return v.Modulus(this.m_value);
        }

        internal override object Multiply()
        {
            return null;
        }

        internal override object Multiply(decimal v)
        {
            return (v * this.m_value);
        }

        internal override object Multiply(double v)
        {
            return (v * this.m_value);
        }

        internal override object Multiply(int v)
        {
            return (v * this.m_value);
        }

        internal override object Multiply(long v)
        {
            return (v * this.m_value);
        }

        internal override object Multiply(float v)
        {
            return (v * this.m_value);
        }

        internal override object Multiply(ushort v)
        {
            return (v * this.m_value);
        }

        internal override object Multiply(uint v)
        {
            return (v * this.m_value);
        }

        internal override object Multiply(ulong v)
        {
            if (this.m_value < 0)
            {
                return base.Multiply(v);
            }
            return (v * this.m_value);
        }

        internal override object Multiply(ArithmeticLiteral v)
        {
            return v.Multiply(this.m_value);
        }

        internal override object Subtract()
        {
            return null;
        }

        internal override object Subtract(decimal v)
        {
            return (v - this.m_value);
        }

        internal override object Subtract(double v)
        {
            return (v - this.m_value);
        }

        internal override object Subtract(int v)
        {
            return (v - this.m_value);
        }

        internal override object Subtract(long v)
        {
            return (v - this.m_value);
        }

        internal override object Subtract(float v)
        {
            return (v - this.m_value);
        }

        internal override object Subtract(ushort v)
        {
            return (v - this.m_value);
        }

        internal override object Subtract(uint v)
        {
            return (v - this.m_value);
        }

        internal override object Subtract(ulong v)
        {
            if (this.m_value < 0)
            {
                return base.Subtract(v);
            }
            return (v - this.m_value);
        }

        internal override object Subtract(ArithmeticLiteral v)
        {
            return v.Subtract(this.m_value);
        }

        internal override object Value
        {
            get
            {
                return this.m_value;
            }
        }
    }
}

