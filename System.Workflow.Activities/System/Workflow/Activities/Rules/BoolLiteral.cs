namespace System.Workflow.Activities.Rules
{
    using System;

    internal class BoolLiteral : Literal
    {
        private bool m_value;

        internal BoolLiteral(bool literalValue)
        {
            this.m_value = literalValue;
            base.m_type = typeof(bool);
        }

        internal override bool Equal(bool rhs)
        {
            return (this.m_value == rhs);
        }

        internal override bool Equal(Literal rhs)
        {
            return rhs.Equal(this.m_value);
        }

        internal override bool GreaterThan(Literal rhs)
        {
            return rhs.LessThan(this.m_value);
        }

        internal override bool GreaterThanOrEqual(Literal rhs)
        {
            return rhs.LessThanOrEqual(this.m_value);
        }

        internal override bool LessThan(Literal rhs)
        {
            return rhs.GreaterThan(this.m_value);
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

