namespace System.StubHelpers
{
    using Microsoft.Win32;
    using System;
    using System.Runtime;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    [StructLayout(LayoutKind.Sequential), SecurityCritical, ForceTokenStabilization, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
    internal struct AsAnyMarshaler
    {
        private const ushort VTHACK_ANSICHAR = 0xfd;
        private const ushort VTHACK_WINBOOL = 0xfe;
        private IntPtr pvArrayMarshaler;
        private BackPropAction backPropAction;
        private Type layoutType;
        private CleanupWorkList cleanupWorkList;
        private static bool IsIn(int dwFlags)
        {
            return ((dwFlags & 0x10000000) != 0);
        }

        private static bool IsOut(int dwFlags)
        {
            return ((dwFlags & 0x20000000) != 0);
        }

        private static bool IsAnsi(int dwFlags)
        {
            return ((dwFlags & 0xff0000) != 0);
        }

        private static bool IsThrowOn(int dwFlags)
        {
            return ((dwFlags & 0xff00) != 0);
        }

        private static bool IsBestFit(int dwFlags)
        {
            return ((dwFlags & 0xff) != 0);
        }

        [ForceTokenStabilization]
        internal AsAnyMarshaler(IntPtr pvArrayMarshaler)
        {
            this.pvArrayMarshaler = pvArrayMarshaler;
            this.backPropAction = BackPropAction.None;
            this.layoutType = null;
            this.cleanupWorkList = null;
        }

        [SecurityCritical]
        private unsafe IntPtr ConvertArrayToNative(object pManagedHome, int dwFlags)
        {
            int num;
            IntPtr ptr;
            Type elementType = pManagedHome.GetType().GetElementType();
            VarEnum enum2 = VarEnum.VT_EMPTY;
            switch (Type.GetTypeCode(elementType))
            {
                case TypeCode.Object:
                    if (!(elementType == typeof(IntPtr)))
                    {
                        if (elementType == typeof(UIntPtr))
                        {
                            enum2 = (IntPtr.Size == 4) ? VarEnum.VT_UI4 : VarEnum.VT_UI8;
                            goto Label_010D;
                        }
                        break;
                    }
                    enum2 = (IntPtr.Size == 4) ? VarEnum.VT_I4 : VarEnum.VT_I8;
                    goto Label_010D;

                case TypeCode.Boolean:
                    enum2 = (VarEnum) 0xfe;
                    goto Label_010D;

                case TypeCode.Char:
                    enum2 = IsAnsi(dwFlags) ? ((VarEnum) 0xfd) : VarEnum.VT_UI2;
                    goto Label_010D;

                case TypeCode.SByte:
                    enum2 = VarEnum.VT_I1;
                    goto Label_010D;

                case TypeCode.Byte:
                    enum2 = VarEnum.VT_UI1;
                    goto Label_010D;

                case TypeCode.Int16:
                    enum2 = VarEnum.VT_I2;
                    goto Label_010D;

                case TypeCode.UInt16:
                    enum2 = VarEnum.VT_UI2;
                    goto Label_010D;

                case TypeCode.Int32:
                    enum2 = VarEnum.VT_I4;
                    goto Label_010D;

                case TypeCode.UInt32:
                    enum2 = VarEnum.VT_UI4;
                    goto Label_010D;

                case TypeCode.Int64:
                    enum2 = VarEnum.VT_I8;
                    goto Label_010D;

                case TypeCode.UInt64:
                    enum2 = VarEnum.VT_UI8;
                    goto Label_010D;

                case TypeCode.Single:
                    enum2 = VarEnum.VT_R4;
                    goto Label_010D;

                case TypeCode.Double:
                    enum2 = VarEnum.VT_R8;
                    goto Label_010D;
            }
            throw new ArgumentException(Environment.GetResourceString("Arg_NDirectBadObject"));
        Label_010D:
            num = (int) enum2;
            if (IsBestFit(dwFlags))
            {
                num |= 0x10000;
            }
            if (IsThrowOn(dwFlags))
            {
                num |= 0x1000000;
            }
            MngdNativeArrayMarshaler.CreateMarshaler(this.pvArrayMarshaler, IntPtr.Zero, num);
            IntPtr pNativeHome = new IntPtr((void*) &ptr);
            MngdNativeArrayMarshaler.ConvertSpaceToNative(this.pvArrayMarshaler, ref pManagedHome, pNativeHome);
            if (IsIn(dwFlags))
            {
                MngdNativeArrayMarshaler.ConvertContentsToNative(this.pvArrayMarshaler, ref pManagedHome, pNativeHome);
            }
            if (IsOut(dwFlags))
            {
                this.backPropAction = BackPropAction.Array;
            }
            return ptr;
        }

        [SecurityCritical]
        private static IntPtr ConvertStringToNative(string pManagedHome, int dwFlags)
        {
            if (IsAnsi(dwFlags))
            {
                return CSTRMarshaler.ConvertToNative(dwFlags & 0xffff, pManagedHome, IntPtr.Zero);
            }
            System.StubHelpers.StubHelpers.CheckStringLength(pManagedHome.Length);
            int cb = (pManagedHome.Length + 1) * 2;
            IntPtr dest = Marshal.AllocCoTaskMem(cb);
            string.InternalCopy(pManagedHome, dest, cb);
            return dest;
        }

        [SecurityCritical]
        private unsafe IntPtr ConvertStringBuilderToNative(StringBuilder pManagedHome, int dwFlags)
        {
            IntPtr ptr;
            if (IsAnsi(dwFlags))
            {
                System.StubHelpers.StubHelpers.CheckStringLength(pManagedHome.Capacity);
                int num = (pManagedHome.Capacity * Marshal.SystemMaxDBCSCharSize) + 4;
                ptr = Marshal.AllocCoTaskMem(num);
                byte* pDest = (byte*) ptr;
                *((pDest + num) - 3) = 0;
                *((pDest + num) - 2) = 0;
                *((pDest + num) - 1) = 0;
                if (IsIn(dwFlags))
                {
                    int num2;
                    Buffer.memcpy(AnsiCharMarshaler.DoAnsiConversion(pManagedHome.ToString(), IsBestFit(dwFlags), IsThrowOn(dwFlags), out num2), 0, pDest, 0, num2);
                    pDest[num2] = 0;
                }
                if (IsOut(dwFlags))
                {
                    this.backPropAction = BackPropAction.StringBuilderAnsi;
                }
                return ptr;
            }
            int cb = (pManagedHome.Capacity * 2) + 4;
            ptr = Marshal.AllocCoTaskMem(cb);
            byte* numPtr2 = (byte*) ptr;
            *((numPtr2 + cb) - 1) = 0;
            *((numPtr2 + cb) - 2) = 0;
            if (IsIn(dwFlags))
            {
                int len = pManagedHome.Length * 2;
                pManagedHome.InternalCopy(ptr, len);
                numPtr2[len] = 0;
                (numPtr2 + len)[1] = 0;
            }
            if (IsOut(dwFlags))
            {
                this.backPropAction = BackPropAction.StringBuilderUnicode;
            }
            return ptr;
        }

        [SecurityCritical]
        private unsafe IntPtr ConvertLayoutToNative(object pManagedHome, int dwFlags)
        {
            IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOfHelper(pManagedHome.GetType(), false));
            if (IsIn(dwFlags))
            {
                System.StubHelpers.StubHelpers.FmtClassUpdateNativeInternal(pManagedHome, (byte*) ptr.ToPointer(), ref this.cleanupWorkList);
            }
            if (IsOut(dwFlags))
            {
                this.backPropAction = BackPropAction.Layout;
            }
            this.layoutType = pManagedHome.GetType();
            return ptr;
        }

        [SecurityCritical, ForceTokenStabilization]
        internal IntPtr ConvertToNative(object pManagedHome, int dwFlags)
        {
            if (pManagedHome == null)
            {
                return IntPtr.Zero;
            }
            if (pManagedHome is ArrayWithOffset)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MarshalAsAnyRestriction"));
            }
            if (pManagedHome.GetType().IsArray)
            {
                return this.ConvertArrayToNative(pManagedHome, dwFlags);
            }
            string str = pManagedHome as string;
            if (str != null)
            {
                return ConvertStringToNative(str, dwFlags);
            }
            StringBuilder builder = pManagedHome as StringBuilder;
            if (builder != null)
            {
                return this.ConvertStringBuilderToNative(builder, dwFlags);
            }
            if (!pManagedHome.GetType().IsLayoutSequential && !pManagedHome.GetType().IsExplicitLayout)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_NDirectBadObject"));
            }
            return this.ConvertLayoutToNative(pManagedHome, dwFlags);
        }

        [SecurityCritical, ForceTokenStabilization]
        internal unsafe void ConvertToManaged(object pManagedHome, IntPtr pNativeHome)
        {
            switch (this.backPropAction)
            {
                case BackPropAction.Array:
                    MngdNativeArrayMarshaler.ConvertContentsToManaged(this.pvArrayMarshaler, ref pManagedHome, new IntPtr((void*) &pNativeHome));
                    return;

                case BackPropAction.Layout:
                    System.StubHelpers.StubHelpers.FmtClassUpdateCLRInternal(pManagedHome, (byte*) pNativeHome.ToPointer());
                    return;

                case BackPropAction.StringBuilderAnsi:
                {
                    sbyte* newBuffer = (sbyte*) pNativeHome.ToPointer();
                    ((StringBuilder) pManagedHome).ReplaceBufferAnsiInternal(newBuffer, Win32Native.lstrlenA(pNativeHome));
                    return;
                }
                case BackPropAction.StringBuilderUnicode:
                {
                    char* chPtr = (char*) pNativeHome.ToPointer();
                    ((StringBuilder) pManagedHome).ReplaceBufferInternal(chPtr, Win32Native.lstrlenW(pNativeHome));
                    return;
                }
            }
        }

        [SecurityCritical, ForceTokenStabilization]
        internal void ClearNative(IntPtr pNativeHome)
        {
            if (pNativeHome != IntPtr.Zero)
            {
                if (this.layoutType != null)
                {
                    Marshal.DestroyStructure(pNativeHome, this.layoutType);
                }
                Win32Native.CoTaskMemFree(pNativeHome);
            }
            System.StubHelpers.StubHelpers.DestroyCleanupList(ref this.cleanupWorkList);
        }
        private enum BackPropAction
        {
            None,
            Array,
            Layout,
            StringBuilderAnsi,
            StringBuilderUnicode
        }
    }
}

