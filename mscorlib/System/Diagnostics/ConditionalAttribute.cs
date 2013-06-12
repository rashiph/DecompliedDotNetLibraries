namespace System.Diagnostics
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple=true), ComVisible(true)]
    public sealed class ConditionalAttribute : Attribute
    {
        private string m_conditionString;

        public ConditionalAttribute(string conditionString)
        {
            this.m_conditionString = conditionString;
        }

        public string ConditionString
        {
            get
            {
                return this.m_conditionString;
            }
        }
    }
}

