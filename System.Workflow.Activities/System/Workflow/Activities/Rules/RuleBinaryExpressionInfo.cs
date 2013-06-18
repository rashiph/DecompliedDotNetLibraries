namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Reflection;
    using System.Runtime;

    internal class RuleBinaryExpressionInfo : RuleExpressionInfo
    {
        private Type leftType;
        private System.Reflection.MethodInfo methodInfo;
        private Type rightType;

        internal RuleBinaryExpressionInfo(Type lhsType, Type rhsType, System.Reflection.MethodInfo mi) : base(mi.ReturnType)
        {
            this.leftType = lhsType;
            this.rightType = rhsType;
            this.methodInfo = mi;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal RuleBinaryExpressionInfo(Type lhsType, Type rhsType, Type resultType) : base(resultType)
        {
            this.leftType = lhsType;
            this.rightType = rhsType;
        }

        internal Type LeftType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.leftType;
            }
        }

        internal System.Reflection.MethodInfo MethodInfo
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.methodInfo;
            }
        }

        internal Type RightType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.rightType;
            }
        }
    }
}

