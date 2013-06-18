namespace System.Workflow.Activities.Rules
{
    using System;

    internal class SByteLiteral : Literal
    {
        private sbyte m_value;

        internal SByteLiteral(sbyte literalValue)
        {
            this.m_value = literalValue;
            base.m_type = typeof(sbyte);
        }

        internal override bool Equal(byte rhs)
        {
            return (this.m_value == rhs);
        }

        internal override bool Equal(char rhs)
        {
            return (this.m_value == rhs);
        }

        internal override bool Equal(decimal rhs)
        {
            return (this.m_value == rhs);
        }

        internal override bool Equal(double rhs)
        {
            return (this.m_value == rhs);
        }

        internal override bool Equal(short rhs)
        {
            return (this.m_value == rhs);
        }

        internal override bool Equal(int rhs)
        {
            return (this.m_value == rhs);
        }

        internal override bool Equal(long rhs)
        {
            return (this.m_value == rhs);
        }

        internal override bool Equal(sbyte rhs)
        {
            return (this.m_value == rhs);
        }

        internal override bool Equal(float rhs)
        {
            return (this.m_value == rhs);
        }

        internal override bool Equal(ushort rhs)
        {
            return (this.m_value == rhs);
        }

        internal override bool Equal(uint rhs)
        {
            return (this.m_value == rhs);
        }

        internal override bool Equal(ulong rhs)
        {
            return ((this.m_value >= 0) && (this.m_value == rhs));
        }

        internal override bool Equal(Literal rhs)
        {
            return rhs.Equal(this.m_value);
        }

        internal override bool GreaterThan(byte rhs)
        {
            return (this.m_value > rhs);
        }

        internal override bool GreaterThan(char rhs)
        {
            return (this.m_value > rhs);
        }

        internal override bool GreaterThan(decimal rhs)
        {
            return (this.m_value > rhs);
        }

        internal override bool GreaterThan(double rhs)
        {
            return (this.m_value > rhs);
        }

        internal override bool GreaterThan(short rhs)
        {
            return (this.m_value > rhs);
        }

        internal override bool GreaterThan(int rhs)
        {
            return (this.m_value > rhs);
        }

        internal override bool GreaterThan(long rhs)
        {
            return (this.m_value > rhs);
        }

        internal override bool GreaterThan(sbyte rhs)
        {
            return (this.m_value > rhs);
        }

        internal override bool GreaterThan(float rhs)
        {
            return (this.m_value > rhs);
        }

        internal override bool GreaterThan(ushort rhs)
        {
            return (this.m_value > rhs);
        }

        internal override bool GreaterThan(uint rhs)
        {
            return (this.m_value > rhs);
        }

        internal override bool GreaterThan(Literal rhs)
        {
            return rhs.LessThan(this.m_value);
        }

        internal override bool GreaterThanOrEqual(byte rhs)
        {
            return (this.m_value >= rhs);
        }

        internal override bool GreaterThanOrEqual(char rhs)
        {
            return (this.m_value >= rhs);
        }

        internal override bool GreaterThanOrEqual(decimal rhs)
        {
            return (this.m_value >= rhs);
        }

        internal override bool GreaterThanOrEqual(double rhs)
        {
            return (this.m_value >= rhs);
        }

        internal override bool GreaterThanOrEqual(short rhs)
        {
            return (this.m_value >= rhs);
        }

        internal override bool GreaterThanOrEqual(int rhs)
        {
            return (this.m_value >= rhs);
        }

        internal override bool GreaterThanOrEqual(long rhs)
        {
            return (this.m_value >= rhs);
        }

        internal override bool GreaterThanOrEqual(sbyte rhs)
        {
            return (this.m_value >= rhs);
        }

        internal override bool GreaterThanOrEqual(float rhs)
        {
            return (this.m_value >= rhs);
        }

        internal override bool GreaterThanOrEqual(ushort rhs)
        {
            return (this.m_value >= rhs);
        }

        internal override bool GreaterThanOrEqual(uint rhs)
        {
            return (this.m_value >= rhs);
        }

        internal override bool GreaterThanOrEqual(Literal rhs)
        {
            return rhs.LessThanOrEqual(this.m_value);
        }

        internal override bool LessThan(byte rhs)
        {
            return (this.m_value < rhs);
        }

        internal override bool LessThan(char rhs)
        {
            return (this.m_value < rhs);
        }

        internal override bool LessThan(decimal rhs)
        {
            return (this.m_value < rhs);
        }

        internal override bool LessThan(double rhs)
        {
            return (this.m_value < rhs);
        }

        internal override bool LessThan(short rhs)
        {
            return (this.m_value < rhs);
        }

        internal override bool LessThan(int rhs)
        {
            return (this.m_value < rhs);
        }

        internal override bool LessThan(long rhs)
        {
            return (this.m_value < rhs);
        }

        internal override bool LessThan(sbyte rhs)
        {
            return (this.m_value < rhs);
        }

        internal override bool LessThan(float rhs)
        {
            return (this.m_value < rhs);
        }

        internal override bool LessThan(ushort rhs)
        {
            return (this.m_value < rhs);
        }

        internal override bool LessThan(uint rhs)
        {
            return (this.m_value < rhs);
        }

        internal override bool LessThan(Literal rhs)
        {
            return rhs.GreaterThan(this.m_value);
        }

        internal override bool LessThanOrEqual(byte rhs)
        {
            return (this.m_value <= rhs);
        }

        internal override bool LessThanOrEqual(char rhs)
        {
            return (this.m_value <= rhs);
        }

        internal override bool LessThanOrEqual(decimal rhs)
        {
            return (this.m_value <= rhs);
        }

        internal override bool LessThanOrEqual(double rhs)
        {
            return (this.m_value <= rhs);
        }

        internal override bool LessThanOrEqual(short rhs)
        {
            return (this.m_value <= rhs);
        }

        internal override bool LessThanOrEqual(int rhs)
        {
            return (this.m_value <= rhs);
        }

        internal override bool LessThanOrEqual(long rhs)
        {
            return (this.m_value <= rhs);
        }

        internal override bool LessThanOrEqual(sbyte rhs)
        {
            return (this.m_value <= rhs);
        }

        internal override bool LessThanOrEqual(float rhs)
        {
            return (this.m_value <= rhs);
        }

        internal override bool LessThanOrEqual(ushort rhs)
        {
            return (this.m_value <= rhs);
        }

        internal override bool LessThanOrEqual(uint rhs)
        {
            return (this.m_value <= rhs);
        }

        internal override bool LessThanOrEqual(Literal rhs)
        {
            return rhs.GreaterThanOrEqual(this.m_value);
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

