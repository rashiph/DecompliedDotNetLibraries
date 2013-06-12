namespace System.Security.Policy
{
    using System;

    internal interface ILegacyEvidenceAdapter
    {
        object EvidenceObject { get; }

        Type EvidenceType { get; }
    }
}

