namespace System.Workflow.Activities.Rules
{
    using System;
    using System.CodeDom;
    using System.Runtime;
    using System.Text;

    internal abstract class RuleCodeDomStatement
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected RuleCodeDomStatement()
        {
        }

        internal abstract void AnalyzeUsage(RuleAnalysis analysis);
        internal abstract CodeStatement Clone();
        internal abstract void Decompile(StringBuilder decompilation);
        internal abstract void Execute(RuleExecution execution);
        internal abstract bool Match(CodeStatement expression);
        internal abstract bool Validate(RuleValidation validation);
    }
}

