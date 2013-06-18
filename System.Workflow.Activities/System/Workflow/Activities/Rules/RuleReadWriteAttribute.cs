namespace System.Workflow.Activities.Rules
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.Workflow.ComponentModel.Compiler;

    public abstract class RuleReadWriteAttribute : RuleAttribute
    {
        private string attributePath;
        private RuleAttributeTarget attributeTarget;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected RuleReadWriteAttribute(string path, RuleAttributeTarget target)
        {
            this.attributeTarget = target;
            this.attributePath = path;
        }

        internal void AnalyzeReadWrite(RuleAnalysis analysis, CodeExpression targetExpression, RulePathQualifier targetQualifier, CodeExpressionCollection argumentExpressions, ParameterInfo[] parameters, List<CodeExpression> attributedExpressions)
        {
            if (string.IsNullOrEmpty(this.attributePath))
            {
                if (this.attributeTarget == RuleAttributeTarget.This)
                {
                    attributedExpressions.Add(targetExpression);
                }
                else if (this.attributeTarget == RuleAttributeTarget.Parameter)
                {
                    for (int i = 0; i < argumentExpressions.Count; i++)
                    {
                        attributedExpressions.Add(argumentExpressions[i]);
                    }
                }
            }
            else
            {
                string attributePath = this.attributePath;
                bool isRead = !analysis.ForWrites;
                bool forWrites = analysis.ForWrites;
                if (this.attributeTarget == RuleAttributeTarget.This)
                {
                    string str2 = "this/";
                    if (attributePath.StartsWith(str2, StringComparison.Ordinal))
                    {
                        attributePath = attributePath.Substring(str2.Length);
                    }
                    RuleExpressionWalker.AnalyzeUsage(analysis, targetExpression, isRead, forWrites, new RulePathQualifier(attributePath, targetQualifier));
                    attributedExpressions.Add(targetExpression);
                }
                else if (this.attributeTarget == RuleAttributeTarget.Parameter)
                {
                    string paramName = null;
                    int index = attributePath.IndexOf('/');
                    if (index >= 0)
                    {
                        paramName = attributePath.Substring(0, index);
                        attributePath = attributePath.Substring(index + 1);
                    }
                    else
                    {
                        paramName = attributePath;
                        attributePath = null;
                    }
                    ParameterInfo info = Array.Find<ParameterInfo>(parameters, p => p.Name == paramName);
                    if (info != null)
                    {
                        RulePathQualifier qualifier = string.IsNullOrEmpty(attributePath) ? null : new RulePathQualifier(attributePath, null);
                        int count = info.Position + 1;
                        if (info.Position == (parameters.Length - 1))
                        {
                            count = argumentExpressions.Count;
                        }
                        for (int j = info.Position; j < count; j++)
                        {
                            CodeExpression expression = argumentExpressions[j];
                            RuleExpressionWalker.AnalyzeUsage(analysis, expression, isRead, forWrites, qualifier);
                            attributedExpressions.Add(expression);
                        }
                    }
                }
            }
        }

        internal override bool Validate(RuleValidation validation, MemberInfo member, Type contextType, ParameterInfo[] parameters)
        {
            ValidationError error = null;
            if (string.IsNullOrEmpty(this.attributePath))
            {
                return true;
            }
            string[] strArray = this.attributePath.Split(new char[] { '/' });
            string str2 = strArray[0];
            int num = 0;
            if (this.attributeTarget == RuleAttributeTarget.This)
            {
                if (str2 == "this")
                {
                    num++;
                }
            }
            else
            {
                bool flag2 = false;
                for (int j = 0; j < parameters.Length; j++)
                {
                    ParameterInfo info = parameters[j];
                    if (info.Name == str2)
                    {
                        flag2 = true;
                        contextType = info.ParameterType;
                        break;
                    }
                }
                if (!flag2)
                {
                    error = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.InvalidRuleAttributeParameter, new object[] { str2, member.Name }), 420);
                    error.UserData["ErrorObject"] = this;
                    validation.AddError(error);
                    return false;
                }
                num++;
            }
            int length = strArray.Length;
            string str3 = strArray[length - 1];
            if (string.IsNullOrEmpty(str3) || (str3 == "*"))
            {
                length--;
            }
            Type fieldType = contextType;
            for (int i = num; i < length; i++)
            {
                if (!(strArray[i] == "*"))
                {
                    goto Label_0171;
                }
                error = new ValidationError(Messages.InvalidWildCardInPathQualifier, 0x195);
                error.UserData["ErrorObject"] = this;
                validation.AddError(error);
                return false;
            Label_0168:
                fieldType = fieldType.GetElementType();
            Label_0171:
                if (fieldType.IsArray)
                {
                    goto Label_0168;
                }
                BindingFlags bindingAttr = BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;
                if (validation.AllowInternalMembers(fieldType))
                {
                    bindingAttr |= BindingFlags.NonPublic;
                }
                FieldInfo field = fieldType.GetField(strArray[i], bindingAttr);
                if (field != null)
                {
                    fieldType = field.FieldType;
                }
                else
                {
                    PropertyInfo property = fieldType.GetProperty(strArray[i], bindingAttr);
                    if (property != null)
                    {
                        fieldType = property.PropertyType;
                    }
                    else
                    {
                        error = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.UpdateUnknownFieldOrProperty, new object[] { strArray[i] }), 390);
                        error.UserData["ErrorObject"] = this;
                        validation.AddError(error);
                        return false;
                    }
                }
            }
            return true;
        }

        public string Path
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.attributePath;
            }
        }

        public RuleAttributeTarget Target
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.attributeTarget;
            }
        }
    }
}

