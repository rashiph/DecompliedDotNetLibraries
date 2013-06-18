namespace System.ServiceModel.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Reflection;
    using System.ServiceModel;

    internal class ServiceModelEnumValidator : ConfigurationValidatorBase
    {
        private Type enumHelperType;
        private MethodInfo isDefined;

        public ServiceModelEnumValidator(Type enumHelperType)
        {
            this.enumHelperType = enumHelperType;
            this.isDefined = this.enumHelperType.GetMethod("IsDefined", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        }

        public override bool CanValidate(Type type)
        {
            return (this.isDefined != null);
        }

        public override void Validate(object value)
        {
            if (!((bool) this.isDefined.Invoke(null, new object[] { value })))
            {
                ParameterInfo[] parameters = this.isDefined.GetParameters();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int) value, parameters[0].ParameterType));
            }
        }
    }
}

