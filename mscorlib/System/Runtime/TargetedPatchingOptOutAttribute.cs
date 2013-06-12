namespace System.Runtime
{
    using System;

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple=false, Inherited=false)]
    public sealed class TargetedPatchingOptOutAttribute : Attribute
    {
        private string m_reason;

        private TargetedPatchingOptOutAttribute()
        {
        }

        public TargetedPatchingOptOutAttribute(string reason)
        {
            this.m_reason = reason;
        }

        public string Reason
        {
            get
            {
                return this.m_reason;
            }
        }
    }
}

