namespace System
{
    internal enum DelegateBindingFlags
    {
        CaselessMatching = 0x20,
        ClosedDelegateOnly = 8,
        InstanceMethodOnly = 2,
        NeverCloseOverNull = 0x10,
        OpenDelegateOnly = 4,
        RelaxedSignature = 0x80,
        SkipSecurityChecks = 0x40,
        StaticMethodOnly = 1
    }
}

