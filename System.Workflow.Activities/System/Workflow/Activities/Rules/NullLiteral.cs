namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Runtime;

    internal class NullLiteral : Literal
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal NullLiteral(Type type)
        {
            base.m_type = type;
        }

        internal override bool Equal(Literal rhs)
        {
            return (rhs.Value == null);
        }

        internal override bool GreaterThan(byte literalValue)
        {
            return false;
        }

        internal override bool GreaterThan(char literalValue)
        {
            return false;
        }

        internal override bool GreaterThan(decimal literalValue)
        {
            return false;
        }

        internal override bool GreaterThan(double literalValue)
        {
            return false;
        }

        internal override bool GreaterThan(short literalValue)
        {
            return false;
        }

        internal override bool GreaterThan(int literalValue)
        {
            return false;
        }

        internal override bool GreaterThan(long literalValue)
        {
            return false;
        }

        internal override bool GreaterThan(sbyte literalValue)
        {
            return false;
        }

        internal override bool GreaterThan(float literalValue)
        {
            return false;
        }

        internal override bool GreaterThan(string literalValue)
        {
            return false;
        }

        internal override bool GreaterThan(ushort literalValue)
        {
            return false;
        }

        internal override bool GreaterThan(uint literalValue)
        {
            return false;
        }

        internal override bool GreaterThan(ulong literalValue)
        {
            return false;
        }

        internal override bool GreaterThan(Literal rhs)
        {
            return rhs.LessThan();
        }

        internal override bool GreaterThanOrEqual()
        {
            return (base.m_type == typeof(string));
        }

        internal override bool GreaterThanOrEqual(byte literalValue)
        {
            return false;
        }

        internal override bool GreaterThanOrEqual(char literalValue)
        {
            return false;
        }

        internal override bool GreaterThanOrEqual(decimal literalValue)
        {
            return false;
        }

        internal override bool GreaterThanOrEqual(double literalValue)
        {
            return false;
        }

        internal override bool GreaterThanOrEqual(short literalValue)
        {
            return false;
        }

        internal override bool GreaterThanOrEqual(int literalValue)
        {
            return false;
        }

        internal override bool GreaterThanOrEqual(long literalValue)
        {
            return false;
        }

        internal override bool GreaterThanOrEqual(sbyte literalValue)
        {
            return false;
        }

        internal override bool GreaterThanOrEqual(float literalValue)
        {
            return false;
        }

        internal override bool GreaterThanOrEqual(string literalValue)
        {
            return false;
        }

        internal override bool GreaterThanOrEqual(ushort literalValue)
        {
            return false;
        }

        internal override bool GreaterThanOrEqual(uint literalValue)
        {
            return false;
        }

        internal override bool GreaterThanOrEqual(ulong literalValue)
        {
            return false;
        }

        internal override bool GreaterThanOrEqual(Literal rhs)
        {
            return rhs.LessThanOrEqual();
        }

        internal override bool LessThan(byte literalValue)
        {
            return false;
        }

        internal override bool LessThan(char literalValue)
        {
            return false;
        }

        internal override bool LessThan(decimal literalValue)
        {
            return false;
        }

        internal override bool LessThan(double literalValue)
        {
            return false;
        }

        internal override bool LessThan(short literalValue)
        {
            return false;
        }

        internal override bool LessThan(int literalValue)
        {
            return false;
        }

        internal override bool LessThan(long literalValue)
        {
            return false;
        }

        internal override bool LessThan(sbyte literalValue)
        {
            return false;
        }

        internal override bool LessThan(float literalValue)
        {
            return false;
        }

        internal override bool LessThan(string literalValue)
        {
            return true;
        }

        internal override bool LessThan(ushort literalValue)
        {
            return false;
        }

        internal override bool LessThan(uint literalValue)
        {
            return false;
        }

        internal override bool LessThan(ulong literalValue)
        {
            return false;
        }

        internal override bool LessThan(Literal rhs)
        {
            return rhs.GreaterThan();
        }

        internal override bool LessThanOrEqual()
        {
            return (base.m_type == typeof(string));
        }

        internal override bool LessThanOrEqual(byte literalValue)
        {
            return false;
        }

        internal override bool LessThanOrEqual(char literalValue)
        {
            return false;
        }

        internal override bool LessThanOrEqual(decimal literalValue)
        {
            return false;
        }

        internal override bool LessThanOrEqual(double literalValue)
        {
            return false;
        }

        internal override bool LessThanOrEqual(short literalValue)
        {
            return false;
        }

        internal override bool LessThanOrEqual(int literalValue)
        {
            return false;
        }

        internal override bool LessThanOrEqual(long literalValue)
        {
            return false;
        }

        internal override bool LessThanOrEqual(sbyte literalValue)
        {
            return false;
        }

        internal override bool LessThanOrEqual(float literalValue)
        {
            return false;
        }

        internal override bool LessThanOrEqual(string literalValue)
        {
            return true;
        }

        internal override bool LessThanOrEqual(ushort literalValue)
        {
            return false;
        }

        internal override bool LessThanOrEqual(uint literalValue)
        {
            return false;
        }

        internal override bool LessThanOrEqual(ulong literalValue)
        {
            return false;
        }

        internal override bool LessThanOrEqual(Literal rhs)
        {
            return rhs.GreaterThanOrEqual();
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

