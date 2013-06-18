namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Reflection;
    using System.Runtime;

    internal class RuleConstructorExpressionInfo : RuleExpressionInfo
    {
        private System.Reflection.ConstructorInfo constructorInfo;
        private bool needsParamsExpansion;

        internal RuleConstructorExpressionInfo(System.Reflection.ConstructorInfo ci, bool needsParamsExpansion) : base(ci.DeclaringType)
        {
            this.constructorInfo = ci;
            this.needsParamsExpansion = needsParamsExpansion;
        }

        internal System.Reflection.ConstructorInfo ConstructorInfo
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.constructorInfo;
            }
        }

        internal bool NeedsParamsExpansion
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.needsParamsExpansion;
            }
        }
    }
}

