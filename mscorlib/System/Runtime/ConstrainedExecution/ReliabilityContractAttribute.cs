namespace System.Runtime.ConstrainedExecution
{
    using System;

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, Inherited=false)]
    public sealed class ReliabilityContractAttribute : Attribute
    {
        private System.Runtime.ConstrainedExecution.Cer _cer;
        private Consistency _consistency;

        public ReliabilityContractAttribute(Consistency consistencyGuarantee, System.Runtime.ConstrainedExecution.Cer cer)
        {
            this._consistency = consistencyGuarantee;
            this._cer = cer;
        }

        public System.Runtime.ConstrainedExecution.Cer Cer
        {
            get
            {
                return this._cer;
            }
        }

        public Consistency ConsistencyGuarantee
        {
            get
            {
                return this._consistency;
            }
        }
    }
}

