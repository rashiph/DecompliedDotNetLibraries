namespace System.EnterpriseServices.Admin
{
    using System;

    [Serializable]
    internal enum FileFlags
    {
        AlreadyInstalled = 0x200,
        BadTLB = 0x400,
        ClassNotAvailable = 0x1000,
        COM = 2,
        ContainsComp = 8,
        ContainsPS = 4,
        ContainsTLB = 0x10,
        DLLRegsvrFailed = 0x8000,
        DoesNotExists = 0x100,
        Error = 0x40000,
        GetClassObjFailed = 0x800,
        Loadable = 1,
        NoRegistrar = 0x4000,
        Registrar = 0x2000,
        RegistrarFailed = 0x20000,
        RegTLBFailed = 0x10000,
        SelfReg = 0x20,
        SelfUnReg = 0x40,
        UnloadableDLL = 0x80
    }
}

