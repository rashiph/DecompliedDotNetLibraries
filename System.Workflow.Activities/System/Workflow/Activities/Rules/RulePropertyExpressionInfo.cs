namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Reflection;
    using System.Runtime;

    internal class RulePropertyExpressionInfo : RuleExpressionInfo
    {
        private bool needsParamsExpansion;
        private System.Reflection.PropertyInfo propertyInfo;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal RulePropertyExpressionInfo(System.Reflection.PropertyInfo pi, Type exprType, bool needsParamsExpansion) : base(exprType)
        {
            this.propertyInfo = pi;
            this.needsParamsExpansion = needsParamsExpansion;
        }

        internal bool NeedsParamsExpansion
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.needsParamsExpansion;
            }
        }

        internal System.Reflection.PropertyInfo PropertyInfo
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.propertyInfo;
            }
        }
    }
}

