namespace System.Security.Policy
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Security;

    [Serializable, Obsolete("This type is obsolete and will be removed in a future release of the .NET Framework. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information."), ComVisible(true)]
    public sealed class FirstMatchCodeGroup : CodeGroup
    {
        internal FirstMatchCodeGroup()
        {
        }

        public FirstMatchCodeGroup(IMembershipCondition membershipCondition, PolicyStatement policy) : base(membershipCondition, policy)
        {
        }

        [SecuritySafeCritical]
        public override CodeGroup Copy()
        {
            FirstMatchCodeGroup group = new FirstMatchCodeGroup {
                MembershipCondition = base.MembershipCondition,
                PolicyStatement = base.PolicyStatement,
                Name = base.Name,
                Description = base.Description
            };
            IEnumerator enumerator = base.Children.GetEnumerator();
            while (enumerator.MoveNext())
            {
                group.AddChild((CodeGroup) enumerator.Current);
            }
            return group;
        }

        internal override string GetTypeName()
        {
            return "System.Security.Policy.FirstMatchCodeGroup";
        }

        [SecuritySafeCritical]
        public override PolicyStatement Resolve(Evidence evidence)
        {
            if (evidence == null)
            {
                throw new ArgumentNullException("evidence");
            }
            object usedEvidence = null;
            if (!PolicyManager.CheckMembershipCondition(base.MembershipCondition, evidence, out usedEvidence))
            {
                return null;
            }
            PolicyStatement childPolicy = null;
            IEnumerator enumerator = base.Children.GetEnumerator();
            while (enumerator.MoveNext())
            {
                childPolicy = PolicyManager.ResolveCodeGroup(enumerator.Current as CodeGroup, evidence);
                if (childPolicy != null)
                {
                    break;
                }
            }
            IDelayEvaluatedEvidence dependentEvidence = usedEvidence as IDelayEvaluatedEvidence;
            bool flag = (dependentEvidence != null) && !dependentEvidence.IsVerified;
            PolicyStatement policyStatement = base.PolicyStatement;
            if (policyStatement == null)
            {
                if (flag)
                {
                    childPolicy = childPolicy.Copy();
                    childPolicy.AddDependentEvidence(dependentEvidence);
                }
                return childPolicy;
            }
            if (childPolicy != null)
            {
                PolicyStatement statement3 = policyStatement.Copy();
                if (flag)
                {
                    statement3.AddDependentEvidence(dependentEvidence);
                }
                statement3.InplaceUnion(childPolicy);
                return statement3;
            }
            if (flag)
            {
                policyStatement.AddDependentEvidence(dependentEvidence);
            }
            return policyStatement;
        }

        [SecuritySafeCritical]
        public override CodeGroup ResolveMatchingCodeGroups(Evidence evidence)
        {
            if (evidence == null)
            {
                throw new ArgumentNullException("evidence");
            }
            if (!base.MembershipCondition.Check(evidence))
            {
                return null;
            }
            CodeGroup group = this.Copy();
            group.Children = new ArrayList();
            IEnumerator enumerator = base.Children.GetEnumerator();
            while (enumerator.MoveNext())
            {
                CodeGroup group2 = ((CodeGroup) enumerator.Current).ResolveMatchingCodeGroups(evidence);
                if (group2 != null)
                {
                    group.AddChild(group2);
                    return group;
                }
            }
            return group;
        }

        public override string MergeLogic
        {
            get
            {
                return Environment.GetResourceString("MergeLogic_FirstMatch");
            }
        }
    }
}

