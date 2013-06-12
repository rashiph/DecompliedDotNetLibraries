namespace System.Runtime.InteropServices
{
    using System;
    using System.Reflection;
    using System.Security;

    [AttributeUsage(AttributeTargets.Method, Inherited=false), ComVisible(true)]
    public sealed class DllImportAttribute : Attribute
    {
        internal string _val;
        public bool BestFitMapping;
        public System.Runtime.InteropServices.CallingConvention CallingConvention;
        public System.Runtime.InteropServices.CharSet CharSet;
        public string EntryPoint;
        public bool ExactSpelling;
        public bool PreserveSig;
        public bool SetLastError;
        public bool ThrowOnUnmappableChar;

        public DllImportAttribute(string dllName)
        {
            this._val = dllName;
        }

        internal DllImportAttribute(string dllName, string entryPoint, System.Runtime.InteropServices.CharSet charSet, bool exactSpelling, bool setLastError, bool preserveSig, System.Runtime.InteropServices.CallingConvention callingConvention, bool bestFitMapping, bool throwOnUnmappableChar)
        {
            this._val = dllName;
            this.EntryPoint = entryPoint;
            this.CharSet = charSet;
            this.ExactSpelling = exactSpelling;
            this.SetLastError = setLastError;
            this.PreserveSig = preserveSig;
            this.CallingConvention = callingConvention;
            this.BestFitMapping = bestFitMapping;
            this.ThrowOnUnmappableChar = throwOnUnmappableChar;
        }

        [SecurityCritical]
        internal static Attribute GetCustomAttribute(RuntimeMethodInfo method)
        {
            string str;
            if ((method.Attributes & MethodAttributes.PinvokeImpl) == MethodAttributes.PrivateScope)
            {
                return null;
            }
            MetadataImport metadataImport = ModuleHandle.GetMetadataImport(method.Module.ModuleHandle.GetRuntimeModule());
            string importDll = null;
            int metadataToken = method.MetadataToken;
            PInvokeAttributes bestFitUseAssem = PInvokeAttributes.BestFitUseAssem;
            metadataImport.GetPInvokeMap(metadataToken, out bestFitUseAssem, out str, out importDll);
            System.Runtime.InteropServices.CharSet none = System.Runtime.InteropServices.CharSet.None;
            switch ((bestFitUseAssem & PInvokeAttributes.CharSetAuto))
            {
                case PInvokeAttributes.BestFitUseAssem:
                    none = System.Runtime.InteropServices.CharSet.None;
                    break;

                case PInvokeAttributes.CharSetAnsi:
                    none = System.Runtime.InteropServices.CharSet.Ansi;
                    break;

                case PInvokeAttributes.CharSetUnicode:
                    none = System.Runtime.InteropServices.CharSet.Unicode;
                    break;

                case PInvokeAttributes.CharSetAuto:
                    none = System.Runtime.InteropServices.CharSet.Auto;
                    break;
            }
            System.Runtime.InteropServices.CallingConvention cdecl = System.Runtime.InteropServices.CallingConvention.Cdecl;
            switch ((bestFitUseAssem & PInvokeAttributes.CallConvMask))
            {
                case PInvokeAttributes.CallConvStdcall:
                    cdecl = System.Runtime.InteropServices.CallingConvention.StdCall;
                    break;

                case PInvokeAttributes.CallConvThiscall:
                    cdecl = System.Runtime.InteropServices.CallingConvention.ThisCall;
                    break;

                case PInvokeAttributes.CallConvFastcall:
                    cdecl = System.Runtime.InteropServices.CallingConvention.FastCall;
                    break;

                case PInvokeAttributes.CallConvWinapi:
                    cdecl = System.Runtime.InteropServices.CallingConvention.Winapi;
                    break;

                case PInvokeAttributes.CallConvCdecl:
                    cdecl = System.Runtime.InteropServices.CallingConvention.Cdecl;
                    break;
            }
            bool exactSpelling = (bestFitUseAssem & PInvokeAttributes.NoMangle) != PInvokeAttributes.BestFitUseAssem;
            bool setLastError = (bestFitUseAssem & PInvokeAttributes.SupportsLastError) != PInvokeAttributes.BestFitUseAssem;
            bool bestFitMapping = (bestFitUseAssem & PInvokeAttributes.BestFitMask) == PInvokeAttributes.BestFitEnabled;
            bool throwOnUnmappableChar = (bestFitUseAssem & PInvokeAttributes.ThrowOnUnmappableCharMask) == PInvokeAttributes.ThrowOnUnmappableCharEnabled;
            return new DllImportAttribute(importDll, str, none, exactSpelling, setLastError, (method.GetMethodImplementationFlags() & MethodImplAttributes.PreserveSig) != MethodImplAttributes.IL, cdecl, bestFitMapping, throwOnUnmappableChar);
        }

        internal static bool IsDefined(RuntimeMethodInfo method)
        {
            return ((method.Attributes & MethodAttributes.PinvokeImpl) != MethodAttributes.PrivateScope);
        }

        public string Value
        {
            get
            {
                return this._val;
            }
        }
    }
}

