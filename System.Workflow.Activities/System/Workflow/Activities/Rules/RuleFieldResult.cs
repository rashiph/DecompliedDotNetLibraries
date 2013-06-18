namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Globalization;
    using System.Reflection;

    internal class RuleFieldResult : RuleExpressionResult
    {
        private FieldInfo fieldInfo;
        private object targetObject;

        public RuleFieldResult(object targetObject, FieldInfo fieldInfo)
        {
            if (fieldInfo == null)
            {
                throw new ArgumentNullException("fieldInfo");
            }
            this.targetObject = targetObject;
            this.fieldInfo = fieldInfo;
        }

        public override object Value
        {
            get
            {
                if (!this.fieldInfo.IsStatic && (this.targetObject == null))
                {
                    RuleEvaluationException exception = new RuleEvaluationException(string.Format(CultureInfo.CurrentCulture, Messages.TargetEvaluatedNullField, new object[] { this.fieldInfo.Name }));
                    exception.Data["ErrorObject"] = this.fieldInfo;
                    throw exception;
                }
                return this.fieldInfo.GetValue(this.targetObject);
            }
            set
            {
                if (!this.fieldInfo.IsStatic && (this.targetObject == null))
                {
                    RuleEvaluationException exception = new RuleEvaluationException(string.Format(CultureInfo.CurrentCulture, Messages.TargetEvaluatedNullField, new object[] { this.fieldInfo.Name }));
                    exception.Data["ErrorObject"] = this.fieldInfo;
                    throw exception;
                }
                this.fieldInfo.SetValue(this.targetObject, value);
            }
        }
    }
}

