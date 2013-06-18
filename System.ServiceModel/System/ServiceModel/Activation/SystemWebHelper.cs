namespace System.ServiceModel.Activation
{
    using System;
    using System.Reflection;
    using System.ServiceModel;
    using System.Web.Security;

    internal static class SystemWebHelper
    {
        private static RoleProvider defaultRoleProvider;
        private static bool defaultRoleProviderSet;
        private static Type typeOfMembership;
        private static Type typeOfRoles;
        private static Type typeOfWebContext;

        internal static RoleProvider GetDefaultRoleProvider()
        {
            if (defaultRoleProviderSet)
            {
                return defaultRoleProvider;
            }
            Type typeOfRoles = TypeOfRoles;
            RoleProvider provider = null;
            if (typeOfRoles != null)
            {
                try
                {
                    if ((bool) typeOfRoles.GetProperty("Enabled").GetValue(null, null))
                    {
                        provider = typeOfRoles.GetProperty("Provider").GetValue(null, null) as RoleProvider;
                    }
                }
                catch (TargetInvocationException exception)
                {
                    if (exception.InnerException != null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception.InnerException);
                    }
                    throw;
                }
            }
            defaultRoleProvider = provider;
            defaultRoleProviderSet = true;
            return provider;
        }

        internal static MembershipProvider GetMembershipProvider()
        {
            Type typeOfMembership = TypeOfMembership;
            if (typeOfMembership != null)
            {
                try
                {
                    return (MembershipProvider) typeOfMembership.GetProperty("Provider").GetValue(null, null);
                }
                catch (TargetInvocationException exception)
                {
                    if (exception.InnerException != null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception.InnerException);
                    }
                    throw;
                }
            }
            return null;
        }

        internal static MembershipProvider GetMembershipProvider(string membershipProviderName)
        {
            Type typeOfMembership = TypeOfMembership;
            if (typeOfMembership != null)
            {
                try
                {
                    object obj2 = typeOfMembership.GetProperty("Providers").GetValue(null, null);
                    return (MembershipProvider) obj2.GetType().GetProperty("Item", new Type[] { typeof(string) }).GetValue(obj2, new object[] { membershipProviderName });
                }
                catch (TargetInvocationException exception)
                {
                    if (exception.InnerException != null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception.InnerException);
                    }
                    throw;
                }
            }
            return null;
        }

        internal static RoleProvider GetRoleProvider(string roleProviderName)
        {
            Type typeOfRoles = TypeOfRoles;
            if (typeOfRoles != null)
            {
                try
                {
                    object obj2 = typeOfRoles.GetProperty("Providers").GetValue(null, null);
                    return (RoleProvider) obj2.GetType().GetProperty("Item", new Type[] { typeof(string) }).GetValue(obj2, new object[] { roleProviderName });
                }
                catch (TargetInvocationException exception)
                {
                    if (exception.InnerException != null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception.InnerException);
                    }
                    throw;
                }
            }
            return null;
        }

        private static Type GetSystemWebType(string typeName)
        {
            return Type.GetType(typeName + ", System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", false);
        }

        internal static bool IsWebConfigAboveApplication(object configHostingContext)
        {
            bool flag;
            Type typeOfWebContext = TypeOfWebContext;
            if (((configHostingContext == null) || (typeOfWebContext == null)) || (configHostingContext.GetType() != typeOfWebContext))
            {
                return false;
            }
            try
            {
                flag = ((int) typeOfWebContext.GetProperty("ApplicationLevel").GetValue(configHostingContext, null)) == 10;
            }
            catch (TargetInvocationException exception)
            {
                if (exception.InnerException != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception.InnerException);
                }
                throw;
            }
            return flag;
        }

        private static Type TypeOfMembership
        {
            get
            {
                if (typeOfMembership == null)
                {
                    typeOfMembership = GetSystemWebType("System.Web.Security.Membership");
                }
                return typeOfMembership;
            }
        }

        private static Type TypeOfRoles
        {
            get
            {
                if (typeOfRoles == null)
                {
                    typeOfRoles = GetSystemWebType("System.Web.Security.Roles");
                }
                return typeOfRoles;
            }
        }

        private static Type TypeOfWebContext
        {
            get
            {
                if (typeOfWebContext == null)
                {
                    typeOfWebContext = GetSystemWebType("System.Web.Configuration.WebContext");
                }
                return typeOfWebContext;
            }
        }
    }
}

