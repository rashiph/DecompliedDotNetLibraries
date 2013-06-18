namespace System.Workflow.Activities.Rules
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Runtime;

    public class RuleAnalysis
    {
        private bool forWrites;
        private Dictionary<string, object> symbols = new Dictionary<string, object>();
        private RuleValidation validation;

        public RuleAnalysis(RuleValidation validation, bool forWrites)
        {
            this.validation = validation;
            this.forWrites = forWrites;
        }

        public void AddSymbol(string symbol)
        {
            this.symbols[symbol] = null;
        }

        internal void AnalyzeRuleAttributes(MemberInfo member, CodeExpression targetExpr, RulePathQualifier targetQualifier, CodeExpressionCollection argExprs, ParameterInfo[] parameters, List<CodeExpression> attributedExprs)
        {
            object[] customAttributes = member.GetCustomAttributes(typeof(RuleAttribute), true);
            if ((customAttributes != null) && (customAttributes.Length > 0))
            {
                RuleAttribute[] attributeArray = (RuleAttribute[]) customAttributes;
                for (int i = 0; i < attributeArray.Length; i++)
                {
                    attributeArray[i].Analyze(this, member, targetExpr, targetQualifier, argExprs, parameters, attributedExprs);
                }
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public ICollection<string> GetSymbols()
        {
            List<string> list = new List<string>(this.symbols.Keys.Count);
            foreach (KeyValuePair<string, object> pair in this.symbols)
            {
                list.Add(pair.Key);
            }
            return list;
        }

        public bool ForWrites
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.forWrites;
            }
        }

        internal RuleValidation Validation
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.validation;
            }
        }
    }
}

