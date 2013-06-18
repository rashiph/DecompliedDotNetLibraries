namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Globalization;
    using System.Reflection;

    internal class LiftedEqualityOperatorMethodInfo : BaseMethodInfo
    {
        public LiftedEqualityOperatorMethodInfo(MethodInfo method) : base(method)
        {
            ParameterInfo[] parameters = method.GetParameters();
            base.expectedParameters = new ParameterInfo[] { new SimpleParameterInfo(parameters[0]), new SimpleParameterInfo(parameters[1]) };
            base.resultType = typeof(bool);
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            if (parameters[0] == null)
            {
                return (parameters[1] == null);
            }
            if (parameters[1] == null)
            {
                return false;
            }
            return base.actualMethod.Invoke(null, invokeAttr, binder, parameters, culture);
        }
    }
}

