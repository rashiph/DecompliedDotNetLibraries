namespace System.Workflow.Activities.Rules
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime;
    using System.Text;
    using System.Workflow.ComponentModel.Compiler;

    [Serializable]
    public sealed class RuleExpressionCondition : RuleCondition
    {
        private CodeExpression _expression;
        [NonSerialized]
        private object _expressionLock;
        private string _name;
        private bool _runtimeInitialized;

        public RuleExpressionCondition()
        {
            this._expressionLock = new object();
        }

        public RuleExpressionCondition(CodeExpression expression)
        {
            this._expressionLock = new object();
            this._expression = expression;
        }

        public RuleExpressionCondition(string conditionName)
        {
            this._expressionLock = new object();
            if (conditionName == null)
            {
                throw new ArgumentNullException("conditionName");
            }
            this._name = conditionName;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RuleExpressionCondition(string conditionName, CodeExpression expression) : this(conditionName)
        {
            this._expression = expression;
        }

        public override RuleCondition Clone()
        {
            RuleExpressionCondition condition = (RuleExpressionCondition) base.MemberwiseClone();
            condition._runtimeInitialized = false;
            condition._expression = RuleExpressionWalker.Clone(this._expression);
            return condition;
        }

        public override bool Equals(object obj)
        {
            bool flag = false;
            RuleExpressionCondition condition = obj as RuleExpressionCondition;
            if (condition != null)
            {
                flag = (this.Name == condition.Name) && (((this._expression == null) && (condition.Expression == null)) || ((this._expression != null) && RuleExpressionWalker.Match(this._expression, condition.Expression)));
            }
            return flag;
        }

        public override bool Evaluate(RuleExecution execution)
        {
            return ((this._expression == null) || Executor.EvaluateBool(this._expression, execution));
        }

        public override ICollection<string> GetDependencies(RuleValidation validation)
        {
            RuleAnalysis analysis = new RuleAnalysis(validation, false);
            if (this._expression != null)
            {
                RuleExpressionWalker.AnalyzeUsage(analysis, this._expression, true, false, null);
            }
            return analysis.GetSymbols();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override void OnRuntimeInitialized()
        {
            if (!this._runtimeInitialized)
            {
                this._runtimeInitialized = true;
            }
        }

        public override string ToString()
        {
            if (this._expression != null)
            {
                StringBuilder stringBuilder = new StringBuilder();
                RuleExpressionWalker.Decompile(stringBuilder, this._expression, null);
                return stringBuilder.ToString();
            }
            return "";
        }

        public override bool Validate(RuleValidation validation)
        {
            if (validation == null)
            {
                throw new ArgumentNullException("validation");
            }
            bool flag = true;
            if (this._expression == null)
            {
                flag = false;
                ValidationError error = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.ConditionExpressionNull, new object[] { typeof(CodePrimitiveExpression).ToString() }), 400);
                error.UserData["ErrorObject"] = this;
                validation.AddError(error);
                return flag;
            }
            return validation.ValidateConditionExpression(this._expression);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public CodeExpression Expression
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._expression;
            }
            set
            {
                if (this._runtimeInitialized)
                {
                    throw new InvalidOperationException(SR.GetString("Error_CanNotChangeAtRuntime"));
                }
                lock (this._expressionLock)
                {
                    this._expression = value;
                }
            }
        }

        public override string Name
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._name;
            }
            set
            {
                if (this._runtimeInitialized)
                {
                    throw new InvalidOperationException(SR.GetString("Error_CanNotChangeAtRuntime"));
                }
                this._name = value;
            }
        }
    }
}

