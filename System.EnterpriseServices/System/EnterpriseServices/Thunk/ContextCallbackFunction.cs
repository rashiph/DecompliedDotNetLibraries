namespace System.EnterpriseServices.Thunk
{
    using System;
    using System.Runtime.CompilerServices;

    internal unsafe delegate int modopt(IsLong) ContextCallbackFunction(tagComCallData* pData);
}

