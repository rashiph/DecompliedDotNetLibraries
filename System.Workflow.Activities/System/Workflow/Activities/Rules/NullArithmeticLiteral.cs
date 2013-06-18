namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Runtime;

    internal class NullArithmeticLiteral : ArithmeticLiteral
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal NullArithmeticLiteral(Type type)
        {
            base.m_type = type;
        }

        internal override object Add()
        {
            return null;
        }

        internal override object Add(bool v)
        {
            return null;
        }

        internal override object Add(char v)
        {
            return null;
        }

        internal override object Add(decimal v)
        {
            return null;
        }

        internal override object Add(double v)
        {
            return null;
        }

        internal override object Add(int v)
        {
            return null;
        }

        internal override object Add(long v)
        {
            return null;
        }

        internal override object Add(float v)
        {
            return null;
        }

        internal override object Add(string v)
        {
            return null;
        }

        internal override object Add(ushort v)
        {
            return null;
        }

        internal override object Add(uint v)
        {
            return null;
        }

        internal override object Add(ulong v)
        {
            return null;
        }

        internal override object Add(ArithmeticLiteral v)
        {
            return v.Add();
        }

        internal override object BitAnd()
        {
            return null;
        }

        internal override object BitAnd(bool v)
        {
            if (v)
            {
                return null;
            }
            return false;
        }

        internal override object BitAnd(int v)
        {
            return null;
        }

        internal override object BitAnd(long v)
        {
            return null;
        }

        internal override object BitAnd(ushort v)
        {
            return null;
        }

        internal override object BitAnd(uint v)
        {
            return null;
        }

        internal override object BitAnd(ulong v)
        {
            return null;
        }

        internal override object BitAnd(ArithmeticLiteral v)
        {
            return v.BitAnd();
        }

        internal override object BitOr()
        {
            return null;
        }

        internal override object BitOr(bool v)
        {
            if (!v)
            {
                return null;
            }
            return true;
        }

        internal override object BitOr(int v)
        {
            return null;
        }

        internal override object BitOr(long v)
        {
            return null;
        }

        internal override object BitOr(ushort v)
        {
            return null;
        }

        internal override object BitOr(uint v)
        {
            return null;
        }

        internal override object BitOr(ulong v)
        {
            return null;
        }

        internal override object BitOr(ArithmeticLiteral v)
        {
            return v.BitOr();
        }

        internal override object Divide()
        {
            return null;
        }

        internal override object Divide(decimal v)
        {
            return null;
        }

        internal override object Divide(double v)
        {
            return null;
        }

        internal override object Divide(int v)
        {
            return null;
        }

        internal override object Divide(long v)
        {
            return null;
        }

        internal override object Divide(float v)
        {
            return null;
        }

        internal override object Divide(ushort v)
        {
            return null;
        }

        internal override object Divide(uint v)
        {
            return null;
        }

        internal override object Divide(ulong v)
        {
            return null;
        }

        internal override object Divide(ArithmeticLiteral v)
        {
            return v.Divide();
        }

        internal override object Modulus()
        {
            return null;
        }

        internal override object Modulus(decimal v)
        {
            return null;
        }

        internal override object Modulus(double v)
        {
            return null;
        }

        internal override object Modulus(int v)
        {
            return null;
        }

        internal override object Modulus(long v)
        {
            return null;
        }

        internal override object Modulus(float v)
        {
            return null;
        }

        internal override object Modulus(ushort v)
        {
            return null;
        }

        internal override object Modulus(uint v)
        {
            return null;
        }

        internal override object Modulus(ulong v)
        {
            return null;
        }

        internal override object Modulus(ArithmeticLiteral v)
        {
            return v.Modulus();
        }

        internal override object Multiply()
        {
            return null;
        }

        internal override object Multiply(decimal v)
        {
            return null;
        }

        internal override object Multiply(double v)
        {
            return null;
        }

        internal override object Multiply(int v)
        {
            return null;
        }

        internal override object Multiply(long v)
        {
            return null;
        }

        internal override object Multiply(float v)
        {
            return null;
        }

        internal override object Multiply(ushort v)
        {
            return null;
        }

        internal override object Multiply(uint v)
        {
            return null;
        }

        internal override object Multiply(ulong v)
        {
            return null;
        }

        internal override object Multiply(ArithmeticLiteral v)
        {
            return v.Multiply();
        }

        internal override object Subtract()
        {
            return null;
        }

        internal override object Subtract(decimal v)
        {
            return null;
        }

        internal override object Subtract(double v)
        {
            return null;
        }

        internal override object Subtract(int v)
        {
            return null;
        }

        internal override object Subtract(long v)
        {
            return null;
        }

        internal override object Subtract(float v)
        {
            return null;
        }

        internal override object Subtract(ushort v)
        {
            return null;
        }

        internal override object Subtract(uint v)
        {
            return null;
        }

        internal override object Subtract(ulong v)
        {
            return null;
        }

        internal override object Subtract(ArithmeticLiteral v)
        {
            return v.Subtract();
        }

        protected override string TypeName
        {
            get
            {
                return Messages.NullValue;
            }
        }

        internal override object Value
        {
            get
            {
                return null;
            }
        }
    }
}

