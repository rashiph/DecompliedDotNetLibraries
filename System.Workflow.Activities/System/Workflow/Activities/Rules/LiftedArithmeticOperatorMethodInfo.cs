namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Globalization;
    using System.Reflection;

    internal class LiftedArithmeticOperatorMethodInfo : BaseMethodInfo
    {
        public LiftedArithmeticOperatorMethodInfo(MethodInfo method) : base(method)
        {
            ParameterInfo[] parameters = method.GetParameters();
            base.expectedParameters = new ParameterInfo[] { new SimpleParameterInfo(parameters[0]), new SimpleParameterInfo(parameters[1]) };
            base.resultType = typeof(Nullable<>).MakeGenericType(new Type[] { method.ReturnType });
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            if (parameters[0] == null)
            {
                return null;
            }
            if (parameters[1] == null)
            {
                return null;
            }
            object operandValue = base.actualMethod.Invoke(null, invokeAttr, binder, parameters, culture);
            return Executor.AdjustType(base.actualMethod.ReturnType, operandValue, base.resultType);
        }
    }
}

