namespace System.Security.Policy
{
    using System;

    [Serializable]
    internal sealed class EvidenceTypeDescriptor
    {
        private EvidenceBase m_assemblyEvidence;
        [NonSerialized]
        private bool m_generated;
        [NonSerialized]
        private bool m_hostCanGenerate;
        private EvidenceBase m_hostEvidence;

        public EvidenceTypeDescriptor()
        {
        }

        private EvidenceTypeDescriptor(EvidenceTypeDescriptor descriptor)
        {
            this.m_hostCanGenerate = descriptor.m_hostCanGenerate;
            if (descriptor.m_assemblyEvidence != null)
            {
                this.m_assemblyEvidence = descriptor.m_assemblyEvidence.Clone();
            }
            if (descriptor.m_hostEvidence != null)
            {
                this.m_hostEvidence = descriptor.m_hostEvidence.Clone();
            }
        }

        public EvidenceTypeDescriptor Clone()
        {
            return new EvidenceTypeDescriptor(this);
        }

        public EvidenceBase AssemblyEvidence
        {
            get
            {
                return this.m_assemblyEvidence;
            }
            set
            {
                this.m_assemblyEvidence = value;
            }
        }

        public bool Generated
        {
            get
            {
                return this.m_generated;
            }
            set
            {
                this.m_generated = value;
            }
        }

        public bool HostCanGenerate
        {
            get
            {
                return this.m_hostCanGenerate;
            }
            set
            {
                this.m_hostCanGenerate = value;
            }
        }

        public EvidenceBase HostEvidence
        {
            get
            {
                return this.m_hostEvidence;
            }
            set
            {
                this.m_hostEvidence = value;
            }
        }
    }
}

