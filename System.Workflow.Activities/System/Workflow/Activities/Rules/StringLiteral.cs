namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Globalization;
    using System.Runtime;

    internal class StringLiteral : Literal
    {
        private string m_value;

        internal StringLiteral(string internalValue)
        {
            this.m_value = internalValue;
            base.m_type = typeof(string);
        }

        internal override bool Equal(string rhs)
        {
            return (this.m_value == rhs);
        }

        internal override bool Equal(Literal rhs)
        {
            return rhs.Equal(this.m_value);
        }

        internal override bool GreaterThan()
        {
            return true;
        }

        internal override bool GreaterThan(string rhs)
        {
            return (0 < string.Compare(this.m_value, rhs, false, CultureInfo.CurrentCulture));
        }

        internal override bool GreaterThan(Literal rhs)
        {
            return rhs.LessThan(this.m_value);
        }

        internal override bool GreaterThanOrEqual()
        {
            return true;
        }

        internal override bool GreaterThanOrEqual(string rhs)
        {
            return (0 <= string.Compare(this.m_value, rhs, false, CultureInfo.CurrentCulture));
        }

        internal override bool GreaterThanOrEqual(Literal rhs)
        {
            return rhs.LessThanOrEqual(this.m_value);
        }

        internal override bool LessThan(string rhs)
        {
            return (0 > string.Compare(this.m_value, rhs, false, CultureInfo.CurrentCulture));
        }

        internal override bool LessThan(Literal rhs)
        {
            return rhs.GreaterThan(this.m_value);
        }

        internal override bool LessThanOrEqual(string rhs)
        {
            return (0 >= string.Compare(this.m_value, rhs, false, CultureInfo.CurrentCulture));
        }

        internal override bool LessThanOrEqual(Literal rhs)
        {
            return rhs.GreaterThanOrEqual(this.m_value);
        }

        internal override object Value
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_value;
            }
        }
    }
}

