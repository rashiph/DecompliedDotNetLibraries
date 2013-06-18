namespace System.Workflow.Activities.Rules
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.Workflow.ComponentModel.Compiler;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple=true)]
    public sealed class RuleInvokeAttribute : RuleAttribute
    {
        private string methodInvoked;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RuleInvokeAttribute(string methodInvoked)
        {
            this.methodInvoked = methodInvoked;
        }

        internal override void Analyze(RuleAnalysis analysis, MemberInfo member, CodeExpression targetExpression, RulePathQualifier targetQualifier, CodeExpressionCollection argumentExpressions, ParameterInfo[] parameters, List<CodeExpression> attributedExpressions)
        {
            Stack<MemberInfo> methodStack = new Stack<MemberInfo>();
            methodStack.Push(member);
            this.AnalyzeInvokeAttribute(analysis, member.DeclaringType, methodStack, targetExpression, targetQualifier, argumentExpressions, parameters, attributedExpressions);
            methodStack.Pop();
        }

        private void AnalyzeInvokeAttribute(RuleAnalysis analysis, Type contextType, Stack<MemberInfo> methodStack, CodeExpression targetExpression, RulePathQualifier targetQualifier, CodeExpressionCollection argumentExpressions, ParameterInfo[] parameters, List<CodeExpression> attributedExpressions)
        {
            foreach (MemberInfo info in contextType.GetMember(this.methodInvoked, MemberTypes.Property | MemberTypes.Method, BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
            {
                if (!methodStack.Contains(info))
                {
                    methodStack.Push(info);
                    object[] customAttributes = info.GetCustomAttributes(typeof(RuleAttribute), true);
                    if ((customAttributes != null) && (customAttributes.Length != 0))
                    {
                        foreach (RuleAttribute attribute in (RuleAttribute[]) customAttributes)
                        {
                            RuleReadWriteAttribute attribute2 = attribute as RuleReadWriteAttribute;
                            if (attribute2 != null)
                            {
                                attribute2.Analyze(analysis, info, targetExpression, targetQualifier, argumentExpressions, parameters, attributedExpressions);
                            }
                            else
                            {
                                ((RuleInvokeAttribute) attribute).AnalyzeInvokeAttribute(analysis, contextType, methodStack, targetExpression, targetQualifier, argumentExpressions, parameters, attributedExpressions);
                            }
                        }
                    }
                    methodStack.Pop();
                }
            }
        }

        internal override bool Validate(RuleValidation validation, MemberInfo member, Type contextType, ParameterInfo[] parameters)
        {
            Stack<MemberInfo> methodStack = new Stack<MemberInfo>();
            methodStack.Push(member);
            bool flag = this.ValidateInvokeAttribute(validation, member, contextType, methodStack);
            methodStack.Pop();
            return flag;
        }

        private bool ValidateInvokeAttribute(RuleValidation validation, MemberInfo member, Type contextType, Stack<MemberInfo> methodStack)
        {
            ValidationError error;
            if (string.IsNullOrEmpty(this.methodInvoked))
            {
                error = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.AttributeMethodNotFound, new object[] { member.Name, base.GetType().Name, Messages.NullValue }), 0x56b, true);
                error.UserData["ErrorObject"] = this;
                validation.AddError(error);
                return false;
            }
            bool flag = true;
            MemberInfo[] infoArray = contextType.GetMember(this.methodInvoked, MemberTypes.Property | MemberTypes.Method, BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            if ((infoArray == null) || (infoArray.Length == 0))
            {
                error = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.AttributeMethodNotFound, new object[] { member.Name, base.GetType().Name, this.methodInvoked }), 0x56b, true);
                error.UserData["ErrorObject"] = this;
                validation.AddError(error);
                return false;
            }
            for (int i = 0; i < infoArray.Length; i++)
            {
                MemberInfo item = infoArray[i];
                if (!methodStack.Contains(item))
                {
                    methodStack.Push(item);
                    object[] customAttributes = item.GetCustomAttributes(typeof(RuleAttribute), true);
                    if ((customAttributes != null) && (customAttributes.Length != 0))
                    {
                        foreach (RuleAttribute attribute in customAttributes)
                        {
                            RuleReadWriteAttribute attribute2 = attribute as RuleReadWriteAttribute;
                            if (attribute2 != null)
                            {
                                if (attribute2.Target == RuleAttributeTarget.Parameter)
                                {
                                    error = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.InvokeAttrRefersToParameterAttribute, new object[] { item.Name }), 0x1a5, true);
                                    error.UserData["ErrorObject"] = this;
                                    validation.AddError(error);
                                    flag = false;
                                }
                                else
                                {
                                    attribute2.Validate(validation, item, contextType, null);
                                }
                            }
                            else
                            {
                                ((RuleInvokeAttribute) attribute).ValidateInvokeAttribute(validation, item, contextType, methodStack);
                            }
                        }
                    }
                    methodStack.Pop();
                }
            }
            return flag;
        }

        public string MethodInvoked
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.methodInvoked;
            }
        }
    }
}

