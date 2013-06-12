namespace System.Security.Policy
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Security;

    internal sealed class AppDomainEvidenceFactory : IRuntimeEvidenceFactory
    {
        private Evidence m_entryPointEvidence;
        private AppDomain m_targetDomain;

        internal AppDomainEvidenceFactory(AppDomain target)
        {
            this.m_targetDomain = target;
        }

        [SecuritySafeCritical]
        public EvidenceBase GenerateEvidence(Type evidenceType)
        {
            if (!this.m_targetDomain.IsDefaultAppDomain())
            {
                return AppDomain.GetDefaultDomain().GetHostEvidence(evidenceType);
            }
            if (this.m_entryPointEvidence == null)
            {
                Assembly entryAssembly = Assembly.GetEntryAssembly();
                RuntimeAssembly assembly2 = entryAssembly as RuntimeAssembly;
                if (assembly2 != null)
                {
                    this.m_entryPointEvidence = assembly2.EvidenceNoDemand.Clone();
                }
                else if (entryAssembly != null)
                {
                    this.m_entryPointEvidence = entryAssembly.Evidence;
                }
            }
            if (this.m_entryPointEvidence == null)
            {
                return null;
            }
            return this.m_entryPointEvidence.GetHostEvidence(evidenceType);
        }

        public IEnumerable<EvidenceBase> GetFactorySuppliedEvidence()
        {
            return new EvidenceBase[0];
        }

        public IEvidenceFactory Target
        {
            get
            {
                return this.m_targetDomain;
            }
        }
    }
}

