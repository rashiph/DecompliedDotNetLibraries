namespace System.Workflow.Activities.Rules
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple=true)]
    public sealed class RuleReadAttribute : RuleReadWriteAttribute
    {
        public RuleReadAttribute(string path) : base(path, RuleAttributeTarget.This)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RuleReadAttribute(string path, RuleAttributeTarget target) : base(path, target)
        {
        }

        internal override void Analyze(RuleAnalysis analysis, MemberInfo member, CodeExpression targetExpression, RulePathQualifier targetQualifier, CodeExpressionCollection argumentExpressions, ParameterInfo[] parameters, List<CodeExpression> attributedExpressions)
        {
            if (!analysis.ForWrites)
            {
                base.AnalyzeReadWrite(analysis, targetExpression, targetQualifier, argumentExpressions, parameters, attributedExpressions);
            }
        }
    }
}

