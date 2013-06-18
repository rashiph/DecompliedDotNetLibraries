namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Globalization;
    using System.Reflection;

    internal class LiftedConversionMethodInfo : BaseMethodInfo
    {
        public LiftedConversionMethodInfo(MethodInfo method) : base(method)
        {
            base.resultType = typeof(Nullable<>).MakeGenericType(new Type[] { method.ReturnType });
            ParameterInfo[] parameters = method.GetParameters();
            base.expectedParameters = new ParameterInfo[] { new SimpleParameterInfo(parameters[0]) };
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            if (parameters[0] == null)
            {
                return Activator.CreateInstance(base.resultType);
            }
            object operandValue = base.actualMethod.Invoke(null, invokeAttr, binder, parameters, culture);
            return Executor.AdjustType(base.actualMethod.ReturnType, operandValue, base.resultType);
        }
    }
}

