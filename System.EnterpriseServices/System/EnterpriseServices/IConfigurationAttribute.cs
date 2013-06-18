namespace System.EnterpriseServices
{
    using System;
    using System.Collections;

    internal interface IConfigurationAttribute
    {
        bool AfterSaveChanges(Hashtable info);
        bool Apply(Hashtable info);
        bool IsValidTarget(string s);
    }
}

