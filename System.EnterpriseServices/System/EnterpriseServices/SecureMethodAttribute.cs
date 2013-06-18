namespace System.EnterpriseServices
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited=true, AllowMultiple=false), ComVisible(false)]
    public sealed class SecureMethodAttribute : Attribute, IConfigurationAttribute
    {
        bool IConfigurationAttribute.AfterSaveChanges(Hashtable info)
        {
            return false;
        }

        bool IConfigurationAttribute.Apply(Hashtable cache)
        {
            string str = (string) cache["CurrentTarget"];
            if (str == "Method")
            {
                cache["SecurityOnMethods"] = true;
            }
            return false;
        }

        bool IConfigurationAttribute.IsValidTarget(string s)
        {
            if (!(s == "Method"))
            {
                return (s == "Component");
            }
            return true;
        }
    }
}

