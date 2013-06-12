namespace System.Net.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Net;
    using System.Threading;

    internal sealed class AuthenticationModulesSectionInternal
    {
        private List<Type> authenticationModules;
        private static object classSyncObject;

        internal AuthenticationModulesSectionInternal(AuthenticationModulesSection section)
        {
            if (section.AuthenticationModules.Count > 0)
            {
                this.authenticationModules = new List<Type>(section.AuthenticationModules.Count);
                foreach (AuthenticationModuleElement element in section.AuthenticationModules)
                {
                    Type c = null;
                    try
                    {
                        c = Type.GetType(element.Type, true, true);
                        if (!typeof(IAuthenticationModule).IsAssignableFrom(c))
                        {
                            throw new InvalidCastException(System.SR.GetString("net_invalid_cast", new object[] { c.FullName, "IAuthenticationModule" }));
                        }
                    }
                    catch (Exception exception)
                    {
                        if (NclUtilities.IsFatal(exception))
                        {
                            throw;
                        }
                        throw new ConfigurationErrorsException(System.SR.GetString("net_config_authenticationmodules"), exception);
                    }
                    this.authenticationModules.Add(c);
                }
            }
        }

        internal static AuthenticationModulesSectionInternal GetSection()
        {
            lock (ClassSyncObject)
            {
                AuthenticationModulesSection section = System.Configuration.PrivilegedConfigurationManager.GetSection(ConfigurationStrings.AuthenticationModulesSectionPath) as AuthenticationModulesSection;
                if (section == null)
                {
                    return null;
                }
                return new AuthenticationModulesSectionInternal(section);
            }
        }

        internal List<Type> AuthenticationModules
        {
            get
            {
                List<Type> authenticationModules = this.authenticationModules;
                if (authenticationModules == null)
                {
                    authenticationModules = new List<Type>(0);
                }
                return authenticationModules;
            }
        }

        internal static object ClassSyncObject
        {
            get
            {
                if (classSyncObject == null)
                {
                    object obj2 = new object();
                    Interlocked.CompareExchange(ref classSyncObject, obj2, null);
                }
                return classSyncObject;
            }
        }
    }
}

