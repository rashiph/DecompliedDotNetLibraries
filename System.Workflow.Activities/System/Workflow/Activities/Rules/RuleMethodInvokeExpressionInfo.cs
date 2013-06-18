namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Reflection;
    using System.Runtime;

    internal class RuleMethodInvokeExpressionInfo : RuleExpressionInfo
    {
        private System.Reflection.MethodInfo methodInfo;
        private bool needsParamsExpansion;

        internal RuleMethodInvokeExpressionInfo(System.Reflection.MethodInfo mi, bool needsParamsExpansion) : base(mi.ReturnType)
        {
            this.methodInfo = mi;
            this.needsParamsExpansion = needsParamsExpansion;
        }

        internal System.Reflection.MethodInfo MethodInfo
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.methodInfo;
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

