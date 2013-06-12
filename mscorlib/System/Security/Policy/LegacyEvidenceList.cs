namespace System.Security.Policy
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Security;
    using System.Security.Permissions;

    [Serializable]
    internal sealed class LegacyEvidenceList : EvidenceBase, IEnumerable<EvidenceBase>, IEnumerable, ILegacyEvidenceAdapter
    {
        private List<EvidenceBase> m_legacyEvidenceList = new List<EvidenceBase>();

        public void Add(EvidenceBase evidence)
        {
            this.m_legacyEvidenceList.Add(evidence);
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override EvidenceBase Clone()
        {
            return base.Clone();
        }

        public IEnumerator<EvidenceBase> GetEnumerator()
        {
            return this.m_legacyEvidenceList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.m_legacyEvidenceList.GetEnumerator();
        }

        public object EvidenceObject
        {
            get
            {
                if (this.m_legacyEvidenceList.Count <= 0)
                {
                    return null;
                }
                return this.m_legacyEvidenceList[0];
            }
        }

        public Type EvidenceType
        {
            get
            {
                ILegacyEvidenceAdapter adapter = this.m_legacyEvidenceList[0] as ILegacyEvidenceAdapter;
                if (adapter != null)
                {
                    return adapter.EvidenceType;
                }
                return this.m_legacyEvidenceList[0].GetType();
            }
        }
    }
}

