namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Globalization;
    using System.Runtime;

    internal class StringArithmeticLiteral : ArithmeticLiteral
    {
        private string m_value;

        internal StringArithmeticLiteral(string literalValue)
        {
            this.m_value = literalValue;
            base.m_type = typeof(string);
        }

        internal override object Add()
        {
            return this.m_value;
        }

        internal override object Add(bool v)
        {
            return (v.ToString(CultureInfo.CurrentCulture) + this.m_value);
        }

        internal override object Add(char v)
        {
            return (v.ToString(CultureInfo.CurrentCulture) + this.m_value);
        }

        internal override object Add(decimal v)
        {
            return (v.ToString(CultureInfo.CurrentCulture) + this.m_value);
        }

        internal override object Add(double v)
        {
            return (v.ToString(CultureInfo.CurrentCulture) + this.m_value);
        }

        internal override object Add(int v)
        {
            return (v.ToString(CultureInfo.CurrentCulture) + this.m_value);
        }

        internal override object Add(long v)
        {
            return (v.ToString(CultureInfo.CurrentCulture) + this.m_value);
        }

        internal override object Add(float v)
        {
            return (v.ToString(CultureInfo.CurrentCulture) + this.m_value);
        }

        internal override object Add(string v)
        {
            return (v + this.m_value);
        }

        internal override object Add(ushort v)
        {
            return (v.ToString(CultureInfo.CurrentCulture) + this.m_value);
        }

        internal override object Add(uint v)
        {
            return (v.ToString(CultureInfo.CurrentCulture) + this.m_value);
        }

        internal override object Add(ulong v)
        {
            return (v.ToString(CultureInfo.CurrentCulture) + this.m_value);
        }

        internal override object Add(ArithmeticLiteral v)
        {
            return v.Add(this.m_value);
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

