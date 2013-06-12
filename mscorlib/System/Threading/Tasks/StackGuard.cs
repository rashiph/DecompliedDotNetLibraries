namespace System.Threading.Tasks
{
    using Microsoft.Win32;
    using System;
    using System.Security;

    internal class StackGuard
    {
        private int m_inliningDepth;
        private ulong m_lastKnownWatermark;
        private const int s_maxUncheckedInliningDepth = 20;
        private static int s_pageSize;
        private const long STACK_RESERVED_SPACE = 0x10000L;

        [SecurityCritical]
        private unsafe bool CheckForSufficientStack()
        {
            if (s_pageSize == 0)
            {
                Win32Native.SYSTEM_INFO lpSystemInfo = new Win32Native.SYSTEM_INFO();
                Win32Native.GetSystemInfo(ref lpSystemInfo);
                s_pageSize = lpSystemInfo.dwPageSize;
            }
            Win32Native.MEMORY_BASIC_INFORMATION buffer = new Win32Native.MEMORY_BASIC_INFORMATION();
            UIntPtr ptr = new UIntPtr((void*) (&buffer - s_pageSize));
            ulong num = ptr.ToUInt64();
            if ((this.m_lastKnownWatermark != 0L) && (num > this.m_lastKnownWatermark))
            {
                return true;
            }
            Win32Native.VirtualQuery(ptr.ToPointer(), ref buffer, new IntPtr(sizeof(Win32Native.MEMORY_BASIC_INFORMATION)));
            UIntPtr allocationBase = (UIntPtr) buffer.AllocationBase;
            if ((num - allocationBase.ToUInt64()) > 0x10000L)
            {
                this.m_lastKnownWatermark = num;
                return true;
            }
            return false;
        }

        [SecuritySafeCritical]
        internal void EndInliningScope()
        {
            this.m_inliningDepth--;
            if (this.m_inliningDepth < 0)
            {
                this.m_inliningDepth = 0;
            }
        }

        [SecuritySafeCritical]
        internal bool TryBeginInliningScope()
        {
            if ((this.m_inliningDepth >= 20) && !this.CheckForSufficientStack())
            {
                return false;
            }
            this.m_inliningDepth++;
            return true;
        }
    }
}

