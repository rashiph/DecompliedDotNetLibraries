namespace System.StubHelpers
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential), ForceTokenStabilization]
    internal struct CopyCtorStubCookie
    {
        public IntPtr m_srcInstancePtr;
        public uint m_dstStackOffset;
        public IntPtr m_ctorPtr;
        public IntPtr m_dtorPtr;
        public IntPtr m_pNext;
        [ForceTokenStabilization]
        public void SetData(IntPtr srcInstancePtr, uint dstStackOffset, IntPtr ctorPtr, IntPtr dtorPtr)
        {
            this.m_srcInstancePtr = srcInstancePtr;
            this.m_dstStackOffset = dstStackOffset;
            this.m_ctorPtr = ctorPtr;
            this.m_dtorPtr = dtorPtr;
        }

        [ForceTokenStabilization]
        public void SetNext(IntPtr pNext)
        {
            this.m_pNext = pNext;
        }
    }
}

