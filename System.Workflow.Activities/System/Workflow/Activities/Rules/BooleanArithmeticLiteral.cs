namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Globalization;

    internal class BooleanArithmeticLiteral : ArithmeticLiteral
    {
        private bool m_value;

        internal BooleanArithmeticLiteral(bool literalValue)
        {
            this.m_value = literalValue;
            base.m_type = typeof(bool);
        }

        internal override object Add()
        {
            return null;
        }

        internal override object Add(string v)
        {
            return (v + this.m_value.ToString(CultureInfo.CurrentCulture));
        }

        internal override object Add(ArithmeticLiteral v)
        {
            return v.Add(this.m_value);
        }

        internal override object BitAnd()
        {
            if (this.m_value)
            {
                return null;
            }
            return false;
        }

        internal override object BitAnd(bool v)
        {
            return (v & this.m_value);
        }

        internal override object BitAnd(ArithmeticLiteral v)
        {
            return v.BitAnd(this.m_value);
        }

        internal override object BitOr()
        {
            if (!this.m_value)
            {
                return null;
            }
            return true;
        }

        internal override object BitOr(bool v)
        {
            return (v | this.m_value);
        }

        internal override object BitOr(ArithmeticLiteral v)
        {
            return v.BitOr(this.m_value);
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

