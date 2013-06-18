namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Globalization;
    using System.Reflection;

    internal class RulePropertyResult : RuleExpressionResult
    {
        private object[] indexerArguments;
        private PropertyInfo propertyInfo;
        private object targetObject;

        public RulePropertyResult(PropertyInfo propertyInfo, object targetObject, object[] indexerArguments)
        {
            if (propertyInfo == null)
            {
                throw new ArgumentNullException("propertyInfo");
            }
            this.targetObject = targetObject;
            this.propertyInfo = propertyInfo;
            this.indexerArguments = indexerArguments;
        }

        public override object Value
        {
            get
            {
                object obj2;
                if (!this.propertyInfo.GetGetMethod(true).IsStatic && (this.targetObject == null))
                {
                    RuleEvaluationException exception = new RuleEvaluationException(string.Format(CultureInfo.CurrentCulture, Messages.TargetEvaluatedNullProperty, new object[] { this.propertyInfo.Name }));
                    exception.Data["ErrorObject"] = this.propertyInfo;
                    throw exception;
                }
                try
                {
                    obj2 = this.propertyInfo.GetValue(this.targetObject, this.indexerArguments);
                }
                catch (TargetInvocationException exception2)
                {
                    if (exception2.InnerException == null)
                    {
                        throw;
                    }
                    throw new TargetInvocationException(string.Format(CultureInfo.CurrentCulture, Messages.Error_PropertyGet, new object[] { RuleDecompiler.DecompileType(this.propertyInfo.ReflectedType), this.propertyInfo.Name, exception2.InnerException.Message }), exception2.InnerException);
                }
                return obj2;
            }
            set
            {
                if (!this.propertyInfo.GetSetMethod(true).IsStatic && (this.targetObject == null))
                {
                    RuleEvaluationException exception = new RuleEvaluationException(string.Format(CultureInfo.CurrentCulture, Messages.TargetEvaluatedNullProperty, new object[] { this.propertyInfo.Name }));
                    exception.Data["ErrorObject"] = this.propertyInfo;
                    throw exception;
                }
                try
                {
                    this.propertyInfo.SetValue(this.targetObject, value, this.indexerArguments);
                }
                catch (TargetInvocationException exception2)
                {
                    if (exception2.InnerException == null)
                    {
                        throw;
                    }
                    throw new TargetInvocationException(string.Format(CultureInfo.CurrentCulture, Messages.Error_PropertySet, new object[] { RuleDecompiler.DecompileType(this.propertyInfo.ReflectedType), this.propertyInfo.Name, exception2.InnerException.Message }), exception2.InnerException);
                }
            }
        }
    }
}

