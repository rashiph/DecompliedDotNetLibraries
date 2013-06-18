namespace System.Workflow.Activities.Rules
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime;

    public abstract class RuleAttribute : Attribute
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected RuleAttribute()
        {
        }

        internal abstract void Analyze(RuleAnalysis analysis, MemberInfo member, CodeExpression targetExpression, RulePathQualifier targetQualifier, CodeExpressionCollection argumentExpressions, ParameterInfo[] parameters, List<CodeExpression> attributedExpressions);
        internal abstract bool Validate(RuleValidation validation, MemberInfo member, Type contextType, ParameterInfo[] parameters);
    }
}

