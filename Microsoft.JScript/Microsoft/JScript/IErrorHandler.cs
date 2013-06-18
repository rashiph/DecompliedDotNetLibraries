namespace Microsoft.JScript
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true), Guid("E93D012C-56BB-4f32-864F-7C75EDA17B14")]
    public interface IErrorHandler
    {
        bool OnCompilerError(IVsaFullErrorInfo error);
    }
}

