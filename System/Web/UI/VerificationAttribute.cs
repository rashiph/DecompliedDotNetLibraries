namespace System.Web.UI
{
    using System;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple=true)]
    public sealed class VerificationAttribute : Attribute
    {
        private string _checkpoint;
        private System.Web.UI.VerificationConditionalOperator _conditionalOperator;
        private string _conditionalProperty;
        private string _conditionalValue;
        private string _guideline;
        private string _guidelineUrl;
        private string _message;
        private int _priority;
        private System.Web.UI.VerificationReportLevel _reportLevel;
        private System.Web.UI.VerificationRule _rule;

        private VerificationAttribute()
        {
        }

        public VerificationAttribute(string guideline, string checkpoint, System.Web.UI.VerificationReportLevel reportLevel, int priority, string message) : this(guideline, checkpoint, reportLevel, priority, message, System.Web.UI.VerificationRule.Required, string.Empty, System.Web.UI.VerificationConditionalOperator.Equals, string.Empty, string.Empty)
        {
        }

        public VerificationAttribute(string guideline, string checkpoint, System.Web.UI.VerificationReportLevel reportLevel, int priority, string message, System.Web.UI.VerificationRule rule, string conditionalProperty) : this(guideline, checkpoint, reportLevel, priority, message, rule, conditionalProperty, System.Web.UI.VerificationConditionalOperator.NotEquals, string.Empty, string.Empty)
        {
        }

        internal VerificationAttribute(string guideline, string checkpoint, System.Web.UI.VerificationReportLevel reportLevel, int priority, string message, System.Web.UI.VerificationRule rule, string conditionalProperty, System.Web.UI.VerificationConditionalOperator conditionalOperator, string conditionalValue) : this(guideline, checkpoint, reportLevel, priority, message, rule, conditionalProperty, conditionalOperator, conditionalValue, string.Empty)
        {
        }

        public VerificationAttribute(string guideline, string checkpoint, System.Web.UI.VerificationReportLevel reportLevel, int priority, string message, System.Web.UI.VerificationRule rule, string conditionalProperty, System.Web.UI.VerificationConditionalOperator conditionalOperator, string conditionalValue, string guidelineUrl)
        {
            this._guideline = guideline;
            this._checkpoint = checkpoint;
            this._reportLevel = reportLevel;
            this._priority = priority;
            this._message = message;
            this._rule = rule;
            this._conditionalProperty = conditionalProperty;
            this._conditionalOperator = conditionalOperator;
            this._conditionalValue = conditionalValue;
            this._guidelineUrl = guidelineUrl;
        }

        public string Checkpoint
        {
            get
            {
                return this._checkpoint;
            }
        }

        public string ConditionalProperty
        {
            get
            {
                return this._conditionalProperty;
            }
        }

        public string ConditionalValue
        {
            get
            {
                return this._conditionalValue;
            }
        }

        public string Guideline
        {
            get
            {
                return this._guideline;
            }
        }

        public string GuidelineUrl
        {
            get
            {
                return this._guidelineUrl;
            }
        }

        public string Message
        {
            get
            {
                return this._message;
            }
        }

        public int Priority
        {
            get
            {
                return this._priority;
            }
        }

        public System.Web.UI.VerificationConditionalOperator VerificationConditionalOperator
        {
            get
            {
                return this._conditionalOperator;
            }
        }

        public System.Web.UI.VerificationReportLevel VerificationReportLevel
        {
            get
            {
                return this._reportLevel;
            }
        }

        public System.Web.UI.VerificationRule VerificationRule
        {
            get
            {
                return this._rule;
            }
        }
    }
}

