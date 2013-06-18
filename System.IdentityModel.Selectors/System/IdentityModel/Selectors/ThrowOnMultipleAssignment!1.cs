namespace System.IdentityModel.Selectors
{
    using Microsoft.InfoCards.Diagnostics;
    using System;
    using System.Runtime;

    internal class ThrowOnMultipleAssignment<T>
    {
        private string m_errorString;
        private T m_value;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ThrowOnMultipleAssignment(string errorString)
        {
            this.m_errorString = errorString;
        }

        public T Value
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_value;
            }
            set
            {
                if ((this.m_value != null) && (value != null))
                {
                    throw InfoCardTrace.ThrowHelperArgument(this.m_errorString);
                }
                if (this.m_value == null)
                {
                    this.m_value = value;
                }
            }
        }
    }
}

