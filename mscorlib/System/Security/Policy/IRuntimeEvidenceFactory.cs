namespace System.Security.Policy
{
    using System;
    using System.Collections.Generic;
    using System.Security;

    internal interface IRuntimeEvidenceFactory
    {
        EvidenceBase GenerateEvidence(Type evidenceType);
        IEnumerable<EvidenceBase> GetFactorySuppliedEvidence();

        IEvidenceFactory Target { get; }
    }
}

