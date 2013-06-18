namespace System.ServiceModel.Administration
{
    using System;
    using System.ServiceModel.Channels;

    internal static class AdministrationHelpers
    {
        public static System.Type GetServiceModelBaseType(System.Type type)
        {
            System.Type baseType = type;
            while (null != baseType)
            {
                if (baseType.IsPublic && (baseType.Assembly == typeof(BindingElement).Assembly))
                {
                    return baseType;
                }
                baseType = baseType.BaseType;
            }
            return baseType;
        }
    }
}

