namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Reflection;
    using System.Runtime;

    internal class RuleFieldExpressionInfo : RuleExpressionInfo
    {
        private System.Reflection.FieldInfo fieldInfo;

        internal RuleFieldExpressionInfo(System.Reflection.FieldInfo fi) : base(fi.FieldType)
        {
            this.fieldInfo = fi;
        }

        internal System.Reflection.FieldInfo FieldInfo
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.fieldInfo;
            }
        }
    }
}

