namespace System.EnterpriseServices
{
    using System;

    [Serializable]
    public enum ImpersonationLevelOption
    {
        Default,
        Anonymous,
        Identify,
        Impersonate,
        Delegate
    }
}

