using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

[CLSCompliant(false)]
internal class NativeOledbWrapper
{
    internal static int modopt(IsConst) SizeOfPROPVARIANT = 0x10;

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), HandleProcessCorruptedStateExceptions, ResourceExposure(ResourceScope.None)]
    internal static unsafe int modopt(IsLong) IChapteredRowsetReleaseChapter(IntPtr ptr, IntPtr chapter)
    {
        int modopt(IsLong) num = -2147418113;
        uint modopt(IsLong) num3 = 0;
        uint modopt(IsLong) num2 = (uint modopt(IsLong)) chapter.ToPointer();
        IChapteredRowset* rowsetPtr = null;
        IUnknown* unknownPtr = (IUnknown*) ptr.ToPointer();
        RuntimeHelpers.PrepareConstrainedRegions();
        try
        {
        }
        finally
        {
            num = **(*((int*) unknownPtr))(unknownPtr, &IID_IChapteredRowset, &rowsetPtr);
            if (null != rowsetPtr)
            {
                num = **(((int*) rowsetPtr))[0x10](rowsetPtr, num2, &num3);
                **(((int*) rowsetPtr))[8](rowsetPtr);
            }
        }
        return num;
    }

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), ResourceExposure(ResourceScope.None), HandleProcessCorruptedStateExceptions]
    internal static unsafe int modopt(IsLong) ITransactionAbort(IntPtr ptr)
    {
        int modopt(IsLong) num = -2147418113;
        ITransactionLocal* localPtr = null;
        IUnknown* unknownPtr = (IUnknown*) ptr.ToPointer();
        RuntimeHelpers.PrepareConstrainedRegions();
        try
        {
        }
        finally
        {
            num = **(*((int*) unknownPtr))(unknownPtr, &IID_ITransactionLocal, &localPtr);
            if (null != localPtr)
            {
                num = **(((int*) localPtr))[0x10](localPtr, 0, 0, 0);
                **(((int*) localPtr))[8](localPtr);
            }
        }
        return num;
    }

    [ResourceExposure(ResourceScope.None), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), HandleProcessCorruptedStateExceptions]
    internal static unsafe int modopt(IsLong) ITransactionCommit(IntPtr ptr)
    {
        int modopt(IsLong) num = -2147418113;
        ITransactionLocal* localPtr = null;
        IUnknown* unknownPtr = (IUnknown*) ptr.ToPointer();
        RuntimeHelpers.PrepareConstrainedRegions();
        try
        {
        }
        finally
        {
            num = **(*((int*) unknownPtr))(unknownPtr, &IID_ITransactionLocal, &localPtr);
            if (null != localPtr)
            {
                num = **(((int*) localPtr))[12](localPtr, 0, 2, 0);
                **(((int*) localPtr))[8](localPtr);
            }
        }
        return num;
    }

    [return: MarshalAs(UnmanagedType.U1)]
    [ResourceExposure(ResourceScope.None)]
    internal static unsafe bool MemoryCompare(IntPtr buf1, IntPtr buf2, int count)
    {
        int num4;
        byte num5;
        byte num6;
        Debug.Assert(buf1 != buf2, "buf1 and buf2 are the same");
        if ((buf1.ToInt64() >= buf2.ToInt64()) && ((buf2.ToInt64() + count) > buf1.ToInt64()))
        {
            num6 = 0;
        }
        else
        {
            num6 = 1;
        }
        Debug.Assert((bool) num6, "overlapping region buf1");
        if ((buf2.ToInt64() >= buf1.ToInt64()) && ((buf1.ToInt64() + count) > buf2.ToInt64()))
        {
            num5 = 0;
        }
        else
        {
            num5 = 1;
        }
        Debug.Assert((bool) num5, "overlapping region buf2");
        byte num8 = (0 <= count) ? ((byte) 1) : ((byte) 0);
        Debug.Assert((bool) num8, "negative count");
        int num3 = count;
        void* voidPtr = buf2.ToPointer();
        void* voidPtr2 = buf1.ToPointer();
        if (count != 0)
        {
            byte num2 = *((byte*) voidPtr2);
            byte num = *((byte*) voidPtr);
            if (num2 >= num)
            {
                int num7 = (int) (voidPtr2 - voidPtr);
                do
                {
                    if (num2 > num)
                    {
                        break;
                    }
                    if (num3 == 1)
                    {
                        goto Label_00DE;
                    }
                    num3--;
                    voidPtr++;
                    num2 = *((byte*) (num7 + voidPtr));
                    num = *((byte*) voidPtr);
                }
                while (num2 >= num);
            }
            num4 = 1;
            goto Label_00E1;
        }
    Label_00DE:
        num4 = 0;
    Label_00E1:
        return (bool) ((byte) num4);
    }

    [ResourceExposure(ResourceScope.None)]
    internal static void MemoryCopy(IntPtr dst, IntPtr src, int count)
    {
        byte num;
        byte num2;
        Debug.Assert(dst != src, "dst and src are the same");
        if ((dst.ToInt64() >= src.ToInt64()) && ((src.ToInt64() + count) > dst.ToInt64()))
        {
            num2 = 0;
        }
        else
        {
            num2 = 1;
        }
        Debug.Assert((bool) num2, "overlapping region dst");
        if ((src.ToInt64() >= dst.ToInt64()) && ((dst.ToInt64() + count) > src.ToInt64()))
        {
            num = 0;
        }
        else
        {
            num = 1;
        }
        Debug.Assert((bool) num, "overlapping region src");
        byte num3 = (0 <= count) ? ((byte) 1) : ((byte) 0);
        Debug.Assert((bool) num3, "negative count");
        memcpy(dst.ToPointer(), src.ToPointer(), count);
    }
}

