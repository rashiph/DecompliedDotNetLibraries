namespace System.Runtime.Serialization.Configuration
{
    using System;
    using System.Configuration;
    using System.Runtime.Serialization;

    internal class DeclaredTypeValidator : ConfigurationValidatorBase
    {
        public override bool CanValidate(Type type)
        {
            return (typeof(string) == type);
        }

        public override void Validate(object value)
        {
            string typeName = (string) value;
            if (typeName.StartsWith(Globals.TypeOfObject.FullName, StringComparison.Ordinal))
            {
                Type o = Type.GetType(typeName, false);
                if ((o != null) && Globals.TypeOfObject.Equals(o))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.Runtime.Serialization.SR.GetString("KnownTypeConfigObject"));
                }
            }
        }
    }
}

