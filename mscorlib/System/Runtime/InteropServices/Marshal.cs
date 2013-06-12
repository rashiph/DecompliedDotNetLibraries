namespace System.Runtime.InteropServices
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices.ComTypes;
    using System.Security;
    using System.Text;
    using System.Threading;

    public static class Marshal
    {
        private static readonly IntPtr HIWORDMASK = new IntPtr(-65536L);
        private static Guid IID_IUnknown = new Guid("00000000-0000-0000-C000-000000000046");
        private const int LMEM_FIXED = 0;
        private const int LMEM_MOVEABLE = 2;
        internal static readonly Guid ManagedNameGuid = new Guid("{0F21F359-AB84-41E8-9A78-36D110E6D2F9}");
        private const string s_strConvertedTypeInfoAssemblyDesc = "Type dynamically generated from ITypeInfo's";
        private const string s_strConvertedTypeInfoAssemblyName = "InteropDynamicTypes";
        private const string s_strConvertedTypeInfoAssemblyTitle = "Interop Dynamic Types";
        private const string s_strConvertedTypeInfoNameSpace = "InteropDynamicTypes";
        public static readonly int SystemDefaultCharSize = 2;
        public static readonly int SystemMaxDBCSCharSize = GetSystemMaxDBCSCharSize();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void _GetTypeLibVersionForAssembly(RuntimeAssembly inputAssembly, out int majorVersion, out int minorVersion);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public static extern int AddRef(IntPtr pUnk);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal static uint AlignedSizeOf<T>() where T: struct
        {
            uint num = SizeOf<T>();
            switch (num)
            {
                case 1:
                case 2:
                    return num;
            }
            if ((IntPtr.Size == 8) && (num == 4))
            {
                return num;
            }
            return AlignedSizeOfType(typeof(T));
        }

        [MethodImpl(MethodImplOptions.InternalCall), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static extern uint AlignedSizeOfType(Type type);
        [SecurityCritical]
        public static IntPtr AllocCoTaskMem(int cb)
        {
            IntPtr ptr = Win32Native.CoTaskMemAlloc(cb);
            if (ptr == Win32Native.NULL)
            {
                throw new OutOfMemoryException();
            }
            return ptr;
        }

        [SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static IntPtr AllocHGlobal(int cb)
        {
            return AllocHGlobal((IntPtr) cb);
        }

        [SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static IntPtr AllocHGlobal(IntPtr cb)
        {
            IntPtr ptr = Win32Native.LocalAlloc_NoSafeHandle(0, cb);
            if (ptr == Win32Native.NULL)
            {
                throw new OutOfMemoryException();
            }
            return ptr;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public static extern bool AreComObjectsAvailableForCleanup();
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("ole32.dll", PreserveSig=false)]
        private static extern void BindMoniker(IMoniker pmk, uint grfOpt, ref Guid iidResult, [MarshalAs(UnmanagedType.Interface)] out object ppvResult);
        [SecurityCritical]
        public static object BindToMoniker(string monikerName)
        {
            object ppvResult = null;
            IBindCtx ppbc = null;
            uint num;
            CreateBindCtx(0, out ppbc);
            IMoniker ppmk = null;
            MkParseDisplayName(ppbc, monikerName, out num, out ppmk);
            BindMoniker(ppmk, 0, ref IID_IUnknown, out ppvResult);
            return ppvResult;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public static extern void ChangeWrapperHandleStrength(object otp, bool fIsWeak);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public static extern void CleanupUnusedObjectsInCurrentContext();
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("ole32.dll", PreserveSig=false)]
        private static extern void CLSIDFromProgID([MarshalAs(UnmanagedType.LPWStr)] string progId, out Guid clsid);
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("ole32.dll", PreserveSig=false)]
        private static extern void CLSIDFromProgIDEx([MarshalAs(UnmanagedType.LPWStr)] string progId, out Guid clsid);
        [SecurityCritical]
        public static void Copy(IntPtr source, byte[] destination, int startIndex, int length)
        {
            CopyToManaged(source, destination, startIndex, length);
        }

        [SecurityCritical]
        public static void Copy(IntPtr source, char[] destination, int startIndex, int length)
        {
            CopyToManaged(source, destination, startIndex, length);
        }

        [SecurityCritical]
        public static void Copy(IntPtr source, double[] destination, int startIndex, int length)
        {
            CopyToManaged(source, destination, startIndex, length);
        }

        [SecurityCritical]
        public static void Copy(IntPtr source, short[] destination, int startIndex, int length)
        {
            CopyToManaged(source, destination, startIndex, length);
        }

        [SecurityCritical]
        public static void Copy(IntPtr source, int[] destination, int startIndex, int length)
        {
            CopyToManaged(source, destination, startIndex, length);
        }

        [SecurityCritical]
        public static void Copy(IntPtr source, long[] destination, int startIndex, int length)
        {
            CopyToManaged(source, destination, startIndex, length);
        }

        [SecurityCritical]
        public static void Copy(IntPtr source, IntPtr[] destination, int startIndex, int length)
        {
            CopyToManaged(source, destination, startIndex, length);
        }

        [SecurityCritical]
        public static void Copy(IntPtr source, float[] destination, int startIndex, int length)
        {
            CopyToManaged(source, destination, startIndex, length);
        }

        [SecurityCritical]
        public static void Copy(byte[] source, int startIndex, IntPtr destination, int length)
        {
            CopyToNative(source, startIndex, destination, length);
        }

        [SecurityCritical]
        public static void Copy(char[] source, int startIndex, IntPtr destination, int length)
        {
            CopyToNative(source, startIndex, destination, length);
        }

        [SecurityCritical]
        public static void Copy(double[] source, int startIndex, IntPtr destination, int length)
        {
            CopyToNative(source, startIndex, destination, length);
        }

        [SecurityCritical]
        public static void Copy(short[] source, int startIndex, IntPtr destination, int length)
        {
            CopyToNative(source, startIndex, destination, length);
        }

        [SecurityCritical]
        public static void Copy(int[] source, int startIndex, IntPtr destination, int length)
        {
            CopyToNative(source, startIndex, destination, length);
        }

        [SecurityCritical]
        public static void Copy(long[] source, int startIndex, IntPtr destination, int length)
        {
            CopyToNative(source, startIndex, destination, length);
        }

        [SecurityCritical]
        public static void Copy(IntPtr[] source, int startIndex, IntPtr destination, int length)
        {
            CopyToNative(source, startIndex, destination, length);
        }

        [SecurityCritical]
        public static void Copy(float[] source, int startIndex, IntPtr destination, int length)
        {
            CopyToNative(source, startIndex, destination, length);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void CopyToManaged(IntPtr source, object destination, int startIndex, int length);
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void CopyToNative(object source, int startIndex, IntPtr destination, int length);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public static extern IntPtr CreateAggregatedObject(IntPtr pOuter, object o);
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("ole32.dll", PreserveSig=false)]
        private static extern void CreateBindCtx(uint reserved, out IBindCtx ppbc);
        [SecurityCritical]
        public static object CreateWrapperOfType(object o, Type t)
        {
            if (t == null)
            {
                throw new ArgumentNullException("t");
            }
            if (!t.IsCOMObject)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_TypeNotComObject"), "t");
            }
            if (t.IsGenericType)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_NeedNonGenericType"), "t");
            }
            if (o == null)
            {
                return null;
            }
            if (!o.GetType().IsCOMObject)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_ObjNotComObject"), "o");
            }
            if (o.GetType() == t)
            {
                return o;
            }
            object comObjectData = GetComObjectData(o, t);
            if (comObjectData == null)
            {
                comObjectData = InternalCreateWrapperOfType(o, t);
                if (!SetComObjectData(o, t, comObjectData))
                {
                    comObjectData = GetComObjectData(o, t);
                }
            }
            return comObjectData;
        }

        [MethodImpl(MethodImplOptions.InternalCall), ComVisible(true), SecurityCritical]
        public static extern void DestroyStructure(IntPtr ptr, Type structuretype);
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void FCallGenerateGuidForType(ref Guid result, Type type);
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void FCallGetTypeInfoGuid(ref Guid result, ITypeInfo typeInfo);
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void FCallGetTypeLibGuid(ref Guid result, ITypeLib pTLB);
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void FCallGetTypeLibGuidForAssembly(ref Guid result, RuntimeAssembly asm);
        [SecurityCritical]
        public static int FinalReleaseComObject(object o)
        {
            if (o == null)
            {
                throw new ArgumentNullException("o");
            }
            __ComObject obj2 = null;
            try
            {
                obj2 = (__ComObject) o;
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_ObjNotComObject"), "o");
            }
            obj2.FinalReleaseSelf();
            return 0;
        }

        [SecurityCritical]
        public static void FreeBSTR(IntPtr ptr)
        {
            if (IsNotWin32Atom(ptr))
            {
                Win32Native.SysFreeString(ptr);
            }
        }

        [SecurityCritical]
        public static void FreeCoTaskMem(IntPtr ptr)
        {
            if (IsNotWin32Atom(ptr))
            {
                Win32Native.CoTaskMemFree(ptr);
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecurityCritical]
        public static void FreeHGlobal(IntPtr hglobal)
        {
            if (IsNotWin32Atom(hglobal) && (Win32Native.NULL != Win32Native.LocalFree(hglobal)))
            {
                ThrowExceptionForHR(GetHRForLastWin32Error());
            }
        }

        [SecurityCritical]
        public static Guid GenerateGuidForType(Type type)
        {
            Guid result = new Guid();
            FCallGenerateGuidForType(ref result, type);
            return result;
        }

        [SecurityCritical]
        public static string GenerateProgIdForType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (type.IsImport)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_TypeMustNotBeComImport"), "type");
            }
            if (type.IsGenericType)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_NeedNonGenericType"), "type");
            }
            if (!RegistrationServices.TypeRequiresRegistrationHelper(type))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_TypeMustBeComCreatable"), "type");
            }
            IList<CustomAttributeData> customAttributes = CustomAttributeData.GetCustomAttributes(type);
            for (int i = 0; i < customAttributes.Count; i++)
            {
                if (customAttributes[i].Constructor.DeclaringType == typeof(ProgIdAttribute))
                {
                    CustomAttributeTypedArgument argument = customAttributes[i].ConstructorArguments[0];
                    string str = (string) argument.Value;
                    if (str == null)
                    {
                        str = string.Empty;
                    }
                    return str;
                }
            }
            return type.FullName;
        }

        [SecurityCritical]
        public static object GetActiveObject(string progID)
        {
            object ppunk = null;
            Guid guid;
            try
            {
                CLSIDFromProgIDEx(progID, out guid);
            }
            catch (Exception)
            {
                CLSIDFromProgID(progID, out guid);
            }
            GetActiveObject(ref guid, IntPtr.Zero, out ppunk);
            return ppunk;
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("oleaut32.dll", PreserveSig=false)]
        private static extern void GetActiveObject(ref Guid rclsid, IntPtr reserved, [MarshalAs(UnmanagedType.Interface)] out object ppunk);
        [SecurityCritical]
        public static IntPtr GetComInterfaceForObject(object o, Type T)
        {
            return GetComInterfaceForObjectNative(o, T, false, true);
        }

        [SecurityCritical]
        public static IntPtr GetComInterfaceForObject(object o, Type T, CustomQueryInterfaceMode mode)
        {
            bool fEnalbeCustomizedQueryInterface = mode == CustomQueryInterfaceMode.Allow;
            return GetComInterfaceForObjectNative(o, T, false, fEnalbeCustomizedQueryInterface);
        }

        [SecurityCritical]
        public static IntPtr GetComInterfaceForObjectInContext(object o, Type t)
        {
            return GetComInterfaceForObjectNative(o, t, true, true);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern IntPtr GetComInterfaceForObjectNative(object o, Type t, bool onlyInContext, bool fEnalbeCustomizedQueryInterface);
        [SecurityCritical]
        public static object GetComObjectData(object obj, object key)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            __ComObject obj2 = null;
            try
            {
                obj2 = (__ComObject) obj;
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_ObjNotComObject"), "obj");
            }
            return obj2.GetData(key);
        }

        [SecurityCritical]
        public static int GetComSlotForMethodInfo(MemberInfo m)
        {
            if (m == null)
            {
                throw new ArgumentNullException("m");
            }
            if (!(m is RuntimeMethodInfo))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeMethodInfo"), "m");
            }
            if (!m.DeclaringType.IsInterface)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeInterfaceMethod"), "m");
            }
            if (m.DeclaringType.IsGenericType)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_NeedNonGenericType"), "m");
            }
            return InternalGetComSlotForMethodInfo((IRuntimeMethodInfo) m);
        }

        [SecurityCritical]
        public static Delegate GetDelegateForFunctionPointer(IntPtr ptr, Type t)
        {
            if (ptr == IntPtr.Zero)
            {
                throw new ArgumentNullException("ptr");
            }
            if (t == null)
            {
                throw new ArgumentNullException("t");
            }
            if (!(t is RuntimeType))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"), "t");
            }
            if (t.IsGenericType)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_NeedNonGenericType"), "t");
            }
            Type baseType = t.BaseType;
            if ((baseType == null) || ((baseType != typeof(Delegate)) && (baseType != typeof(MulticastDelegate))))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeDelegate"), "t");
            }
            return GetDelegateForFunctionPointerInternal(ptr, t);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern Delegate GetDelegateForFunctionPointerInternal(IntPtr ptr, Type t);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public static extern int GetEndComSlot(Type t);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public static extern int GetExceptionCode();
        [SecurityCritical]
        public static Exception GetExceptionForHR(int errorCode)
        {
            if (errorCode < 0)
            {
                return GetExceptionForHRInternal(errorCode, Win32Native.NULL);
            }
            return null;
        }

        [SecurityCritical]
        public static Exception GetExceptionForHR(int errorCode, IntPtr errorInfo)
        {
            if (errorCode < 0)
            {
                return GetExceptionForHRInternal(errorCode, errorInfo);
            }
            return null;
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern Exception GetExceptionForHRInternal(int errorCode, IntPtr errorInfo);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical, ComVisible(true)]
        public static extern IntPtr GetExceptionPointers();
        [SecurityCritical]
        public static IntPtr GetFunctionPointerForDelegate(Delegate d)
        {
            if (d == null)
            {
                throw new ArgumentNullException("d");
            }
            return GetFunctionPointerForDelegateInternal(d);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern IntPtr GetFunctionPointerForDelegateInternal(Delegate d);
        [SecurityCritical]
        public static IntPtr GetHINSTANCE(Module m)
        {
            if (m == null)
            {
                throw new ArgumentNullException("m");
            }
            RuntimeModule internalModule = m as RuntimeModule;
            if (internalModule == null)
            {
                ModuleBuilder builder = m as ModuleBuilder;
                if (builder != null)
                {
                    internalModule = builder.InternalModule;
                }
            }
            if (internalModule == null)
            {
                throw new ArgumentNullException(Environment.GetResourceString("Argument_MustBeRuntimeModule"));
            }
            return GetHINSTANCE(internalModule.GetNativeHandle());
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern IntPtr GetHINSTANCE(RuntimeModule m);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public static extern int GetHRForException(Exception e);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecurityCritical]
        public static int GetHRForLastWin32Error()
        {
            int num = GetLastWin32Error();
            if ((num & 0x80000000L) == 0x80000000L)
            {
                return num;
            }
            return ((num & 0xffff) | -2147024896);
        }

        [SecurityCritical]
        public static IntPtr GetIDispatchForObject(object o)
        {
            return GetIDispatchForObjectNative(o, false);
        }

        [SecurityCritical]
        public static IntPtr GetIDispatchForObjectInContext(object o)
        {
            return GetIDispatchForObjectNative(o, true);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern IntPtr GetIDispatchForObjectNative(object o, bool onlyInContext);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public static extern IntPtr GetITypeInfoForType(Type t);
        [SecurityCritical]
        public static IntPtr GetIUnknownForObject(object o)
        {
            return GetIUnknownForObjectNative(o, false);
        }

        [SecurityCritical]
        public static IntPtr GetIUnknownForObjectInContext(object o)
        {
            return GetIUnknownForObjectNative(o, true);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern IntPtr GetIUnknownForObjectNative(object o, bool onlyInContext);
        [MethodImpl(MethodImplOptions.InternalCall), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecurityCritical]
        public static extern int GetLastWin32Error();
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern Type GetLoadedTypeForGUID(ref Guid guid);
        [MethodImpl(MethodImplOptions.InternalCall), Obsolete("The GetManagedThunkForUnmanagedMethodPtr method has been deprecated and will be removed in a future release.", false), SecurityCritical]
        public static extern IntPtr GetManagedThunkForUnmanagedMethodPtr(IntPtr pfnMethodToWrap, IntPtr pbSignature, int cbSignature);
        [SecurityCritical]
        internal static string GetManagedTypeInfoNameInternal(ITypeLib typeLib, ITypeInfo typeInfo)
        {
            bool flag;
            string typeInfoNameInternal = GetTypeInfoNameInternal(typeInfo, out flag);
            if (flag)
            {
                return typeInfoNameInternal;
            }
            return (GetTypeLibNameInternal(typeLib) + "." + typeInfoNameInternal);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public static extern MemberInfo GetMethodInfoForComSlot(Type t, int slot, ref ComMemberType memberType);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public static extern void GetNativeVariantForObject(object obj, IntPtr pDstNativeVariant);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public static extern object GetObjectForIUnknown(IntPtr pUnk);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public static extern object GetObjectForNativeVariant(IntPtr pSrcNativeVariant);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public static extern object[] GetObjectsForNativeVariants(IntPtr aSrcNativeVariant, int cVars);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public static extern int GetStartComSlot(Type t);
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern int GetSystemMaxDBCSCharSize();
        [SecurityCritical, Obsolete("The GetThreadFromFiberCookie method has been deprecated.  Use the hosting API to perform this operation.", false)]
        public static Thread GetThreadFromFiberCookie(int cookie)
        {
            if (cookie == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_ArgumentZero"), "cookie");
            }
            return InternalGetThreadFromFiberCookie(cookie);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public static extern object GetTypedObjectForIUnknown(IntPtr pUnk, Type t);
        [SecurityCritical]
        public static Type GetTypeForITypeInfo(IntPtr piTypeInfo)
        {
            ITypeInfo typeInfo = null;
            ITypeLib ppTLB = null;
            Type loadedTypeForGUID = null;
            Assembly assembly = null;
            int pIndex = 0;
            if (piTypeInfo == Win32Native.NULL)
            {
                return null;
            }
            typeInfo = (ITypeInfo) GetObjectForIUnknown(piTypeInfo);
            loadedTypeForGUID = GetLoadedTypeForGUID(ref GetTypeInfoGuid(typeInfo));
            if (loadedTypeForGUID != null)
            {
                return loadedTypeForGUID;
            }
            try
            {
                typeInfo.GetContainingTypeLib(out ppTLB, out pIndex);
            }
            catch (COMException)
            {
                ppTLB = null;
            }
            if (ppTLB != null)
            {
                string fullName = TypeLibConverter.GetAssemblyNameFromTypelib(ppTLB, null, null, null, null, AssemblyNameFlags.None).FullName;
                Assembly[] assemblies = Thread.GetDomain().GetAssemblies();
                int length = assemblies.Length;
                for (int i = 0; i < length; i++)
                {
                    if (string.Compare(assemblies[i].FullName, fullName, StringComparison.Ordinal) == 0)
                    {
                        assembly = assemblies[i];
                    }
                }
                if (assembly == null)
                {
                    assembly = new TypeLibConverter().ConvertTypeLibToAssembly(ppTLB, GetTypeLibName(ppTLB) + ".dll", TypeLibImporterFlags.None, new ImporterCallback(), null, null, null, null);
                }
                loadedTypeForGUID = assembly.GetType(GetManagedTypeInfoNameInternal(ppTLB, typeInfo), true, false);
                if ((loadedTypeForGUID != null) && !loadedTypeForGUID.IsVisible)
                {
                    loadedTypeForGUID = null;
                }
                return loadedTypeForGUID;
            }
            return typeof(object);
        }

        [SecurityCritical]
        internal static Guid GetTypeInfoGuid(ITypeInfo typeInfo)
        {
            Guid result = new Guid();
            FCallGetTypeInfoGuid(ref result, typeInfo);
            return result;
        }

        [SecurityCritical]
        public static string GetTypeInfoName(ITypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException("typeInfo");
            }
            string strName = null;
            string strDocString = null;
            int dwHelpContext = 0;
            string strHelpFile = null;
            typeInfo.GetDocumentation(-1, out strName, out strDocString, out dwHelpContext, out strHelpFile);
            return strName;
        }

        [Obsolete("Use System.Runtime.InteropServices.Marshal.GetTypeInfoName(ITypeInfo pTLB) instead. http://go.microsoft.com/fwlink/?linkid=14202&ID=0000011.", false), SecurityCritical]
        public static string GetTypeInfoName(UCOMITypeInfo pTI)
        {
            return GetTypeInfoName((ITypeInfo) pTI);
        }

        [SecurityCritical]
        internal static string GetTypeInfoNameInternal(ITypeInfo typeInfo, out bool hasManagedName)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException("typeInfo");
            }
            ITypeInfo2 info = typeInfo as ITypeInfo2;
            if (info != null)
            {
                object obj2;
                Guid managedNameGuid = ManagedNameGuid;
                try
                {
                    info.GetCustData(ref managedNameGuid, out obj2);
                }
                catch (Exception)
                {
                    obj2 = null;
                }
                if ((obj2 != null) && (obj2.GetType() == typeof(string)))
                {
                    hasManagedName = true;
                    return (string) obj2;
                }
            }
            hasManagedName = false;
            return GetTypeInfoName(typeInfo);
        }

        [SecurityCritical]
        public static Guid GetTypeLibGuid(ITypeLib typelib)
        {
            Guid result = new Guid();
            FCallGetTypeLibGuid(ref result, typelib);
            return result;
        }

        [SecurityCritical, Obsolete("Use System.Runtime.InteropServices.Marshal.GetTypeLibGuid(ITypeLib pTLB) instead. http://go.microsoft.com/fwlink/?linkid=14202&ID=0000011.", false)]
        public static Guid GetTypeLibGuid(UCOMITypeLib pTLB)
        {
            return GetTypeLibGuid((ITypeLib) pTLB);
        }

        [SecurityCritical]
        public static Guid GetTypeLibGuidForAssembly(Assembly asm)
        {
            if (asm == null)
            {
                throw new ArgumentNullException("asm");
            }
            RuntimeAssembly assembly = asm as RuntimeAssembly;
            if (assembly == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeAssembly"), "asm");
            }
            Guid result = new Guid();
            FCallGetTypeLibGuidForAssembly(ref result, assembly);
            return result;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public static extern int GetTypeLibLcid(ITypeLib typelib);
        [Obsolete("Use System.Runtime.InteropServices.Marshal.GetTypeLibLcid(ITypeLib pTLB) instead. http://go.microsoft.com/fwlink/?linkid=14202&ID=0000011.", false), SecurityCritical]
        public static int GetTypeLibLcid(UCOMITypeLib pTLB)
        {
            return GetTypeLibLcid((ITypeLib) pTLB);
        }

        [SecurityCritical]
        public static string GetTypeLibName(ITypeLib typelib)
        {
            if (typelib == null)
            {
                throw new ArgumentNullException("typelib");
            }
            string strName = null;
            string strDocString = null;
            int dwHelpContext = 0;
            string strHelpFile = null;
            typelib.GetDocumentation(-1, out strName, out strDocString, out dwHelpContext, out strHelpFile);
            return strName;
        }

        [SecurityCritical, Obsolete("Use System.Runtime.InteropServices.Marshal.GetTypeLibName(ITypeLib pTLB) instead. http://go.microsoft.com/fwlink/?linkid=14202&ID=0000011.", false)]
        public static string GetTypeLibName(UCOMITypeLib pTLB)
        {
            return GetTypeLibName((ITypeLib) pTLB);
        }

        [SecurityCritical]
        internal static string GetTypeLibNameInternal(ITypeLib typelib)
        {
            if (typelib == null)
            {
                throw new ArgumentNullException("typelib");
            }
            ITypeLib2 lib = typelib as ITypeLib2;
            if (lib != null)
            {
                object obj2;
                Guid managedNameGuid = ManagedNameGuid;
                try
                {
                    lib.GetCustData(ref managedNameGuid, out obj2);
                }
                catch (Exception)
                {
                    obj2 = null;
                }
                if ((obj2 != null) && (obj2.GetType() == typeof(string)))
                {
                    string str = (string) obj2;
                    str = str.Trim();
                    if (str.EndsWith(".DLL", StringComparison.OrdinalIgnoreCase))
                    {
                        return str.Substring(0, str.Length - 4);
                    }
                    if (str.EndsWith(".EXE", StringComparison.OrdinalIgnoreCase))
                    {
                        str = str.Substring(0, str.Length - 4);
                    }
                    return str;
                }
            }
            return GetTypeLibName(typelib);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern void GetTypeLibVersion(ITypeLib typeLibrary, out int major, out int minor);
        [SecurityCritical]
        public static void GetTypeLibVersionForAssembly(Assembly inputAssembly, out int majorVersion, out int minorVersion)
        {
            if (inputAssembly == null)
            {
                throw new ArgumentNullException("inputAssembly");
            }
            RuntimeAssembly assembly = inputAssembly as RuntimeAssembly;
            if (assembly == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeAssembly"), "inputAssembly");
            }
            _GetTypeLibVersionForAssembly(assembly, out majorVersion, out minorVersion);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public static extern object GetUniqueObjectForIUnknown(IntPtr unknown);
        [MethodImpl(MethodImplOptions.InternalCall), Obsolete("The GetUnmanagedThunkForManagedMethodPtr method has been deprecated and will be removed in a future release.", false), SecurityCritical]
        public static extern IntPtr GetUnmanagedThunkForManagedMethodPtr(IntPtr pfnMethodToWrap, IntPtr pbSignature, int cbSignature);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern object InternalCreateWrapperOfType(object o, Type t);
        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern void InternalFinalReleaseComObject(object o);
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern int InternalGetComSlotForMethodInfo(IRuntimeMethodInfo m);
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern Thread InternalGetThreadFromFiberCookie(int cookie);
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern int InternalNumParamBytes(IRuntimeMethodInfo m);
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void InternalPrelink(IRuntimeMethodInfo m);
        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern int InternalReleaseComObject(object o);
        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern bool InternalSwitchCCW(object oldtp, object newtp);
        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern object InternalWrapIUnknownWithComObject(IntPtr i);
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public static extern bool IsComObject(object o);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static bool IsNotWin32Atom(IntPtr ptr)
        {
            long num = (long) ptr;
            return (0L != (num & ((long) HIWORDMASK)));
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public static extern bool IsTypeVisibleFromCom(Type t);
        private static bool IsWin32Atom(IntPtr ptr)
        {
            long num = (long) ptr;
            return (0L == (num & ((long) HIWORDMASK)));
        }

        [SecurityCritical]
        private static IntPtr LoadLicenseManager()
        {
            Type type = Assembly.Load("System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089").GetType("System.ComponentModel.LicenseManager");
            if ((type != null) && type.IsVisible)
            {
                return type.TypeHandle.Value;
            }
            return IntPtr.Zero;
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("ole32.dll", PreserveSig=false)]
        private static extern void MkParseDisplayName(IBindCtx pbc, [MarshalAs(UnmanagedType.LPWStr)] string szUserName, out uint pchEaten, out IMoniker ppmk);
        [SecurityCritical]
        public static int NumParamBytes(MethodInfo m)
        {
            if (m == null)
            {
                throw new ArgumentNullException("m");
            }
            RuntimeMethodInfo info = m as RuntimeMethodInfo;
            if (info == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeMethodInfo"));
            }
            return InternalNumParamBytes(info);
        }

        [SecuritySafeCritical]
        public static IntPtr OffsetOf(Type t, string fieldName)
        {
            if (t == null)
            {
                throw new ArgumentNullException("t");
            }
            FieldInfo field = t.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (field == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_OffsetOfFieldNotFound", new object[] { t.FullName }), "fieldName");
            }
            if (!(field is RuntimeFieldInfo))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeFieldInfo"), "fieldName");
            }
            return OffsetOfHelper(((RuntimeFieldInfo) field).FieldHandle.GetRuntimeFieldInfo());
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern IntPtr OffsetOfHelper(IRuntimeFieldInfo f);
        [SecurityCritical]
        public static void Prelink(MethodInfo m)
        {
            if (m == null)
            {
                throw new ArgumentNullException("m");
            }
            RuntimeMethodInfo info = m as RuntimeMethodInfo;
            if (info == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeMethodInfo"));
            }
            InternalPrelink(info);
        }

        [SecurityCritical]
        public static void PrelinkAll(Type c)
        {
            if (c == null)
            {
                throw new ArgumentNullException("c");
            }
            MethodInfo[] methods = c.GetMethods();
            if (methods != null)
            {
                for (int i = 0; i < methods.Length; i++)
                {
                    Prelink(methods[i]);
                }
            }
        }

        [SecurityCritical]
        public static string PtrToStringAnsi(IntPtr ptr)
        {
            if (Win32Native.NULL == ptr)
            {
                return null;
            }
            if (IsWin32Atom(ptr))
            {
                return null;
            }
            int capacity = Win32Native.lstrlenA(ptr);
            if (capacity == 0)
            {
                return string.Empty;
            }
            StringBuilder pdst = new StringBuilder(capacity);
            Win32Native.CopyMemoryAnsi(pdst, ptr, new IntPtr(1 + capacity));
            return pdst.ToString();
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public static extern string PtrToStringAnsi(IntPtr ptr, int len);
        [SecurityCritical]
        public static string PtrToStringAuto(IntPtr ptr)
        {
            return PtrToStringUni(ptr);
        }

        [SecurityCritical]
        public static string PtrToStringAuto(IntPtr ptr, int len)
        {
            return PtrToStringUni(ptr, len);
        }

        [SecurityCritical]
        public static string PtrToStringBSTR(IntPtr ptr)
        {
            return PtrToStringUni(ptr, Win32Native.SysStringLen(ptr));
        }

        [SecurityCritical]
        public static string PtrToStringUni(IntPtr ptr)
        {
            if (Win32Native.NULL == ptr)
            {
                return null;
            }
            if (IsWin32Atom(ptr))
            {
                return null;
            }
            int capacity = Win32Native.lstrlenW(ptr);
            StringBuilder pdst = new StringBuilder(capacity);
            Win32Native.CopyMemoryUni(pdst, ptr, new IntPtr(2 * (1 + capacity)));
            return pdst.ToString();
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public static extern string PtrToStringUni(IntPtr ptr, int len);
        [ComVisible(true), SecurityCritical]
        public static void PtrToStructure(IntPtr ptr, object structure)
        {
            PtrToStructureHelper(ptr, structure, false);
        }

        [ComVisible(true), SecurityCritical]
        public static object PtrToStructure(IntPtr ptr, Type structureType)
        {
            if (ptr == IntPtr.Zero)
            {
                return null;
            }
            if (structureType == null)
            {
                throw new ArgumentNullException("structureType");
            }
            if (structureType.IsGenericType)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_NeedNonGenericType"), "structureType");
            }
            RuntimeType underlyingSystemType = structureType.UnderlyingSystemType as RuntimeType;
            if (underlyingSystemType == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "type");
            }
            object structure = underlyingSystemType.CreateInstanceDefaultCtor(false, false, false, false);
            PtrToStructureHelper(ptr, structure, true);
            return structure;
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void PtrToStructureHelper(IntPtr ptr, object structure, bool allowValueClasses);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public static extern int QueryInterface(IntPtr pUnk, ref Guid iid, out IntPtr ppv);
        [SecurityCritical]
        public static byte ReadByte(IntPtr ptr)
        {
            return ReadByte(ptr, 0);
        }

        [SecurityCritical]
        public static unsafe byte ReadByte(IntPtr ptr, int ofs)
        {
            byte num;
            try
            {
                byte* numPtr = (byte*) (((void*) ptr) + ofs);
                num = numPtr[0];
            }
            catch (NullReferenceException)
            {
                throw new AccessViolationException();
            }
            return num;
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("mscoree.dll", EntryPoint="ND_RU1")]
        public static extern byte ReadByte([In, MarshalAs(UnmanagedType.AsAny)] object ptr, int ofs);
        [SecurityCritical]
        public static short ReadInt16(IntPtr ptr)
        {
            return ReadInt16(ptr, 0);
        }

        [SecurityCritical]
        public static unsafe short ReadInt16(IntPtr ptr, int ofs)
        {
            short num2;
            try
            {
                short num;
                byte* numPtr = (byte*) (((void*) ptr) + ofs);
                if ((((int) numPtr) & 1) == 0)
                {
                    return *(((short*) numPtr));
                }
                byte* numPtr2 = (byte*) &num;
                numPtr2[0] = numPtr[0];
                numPtr2[1] = numPtr[1];
                num2 = num;
            }
            catch (NullReferenceException)
            {
                throw new AccessViolationException();
            }
            return num2;
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("mscoree.dll", EntryPoint="ND_RI2")]
        public static extern short ReadInt16([In, MarshalAs(UnmanagedType.AsAny)] object ptr, int ofs);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecurityCritical]
        public static int ReadInt32(IntPtr ptr)
        {
            return ReadInt32(ptr, 0);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecurityCritical]
        public static unsafe int ReadInt32(IntPtr ptr, int ofs)
        {
            int num2;
            try
            {
                int num;
                byte* numPtr = (byte*) (((void*) ptr) + ofs);
                if ((((int) numPtr) & 3) == 0)
                {
                    return *(((int*) numPtr));
                }
                byte* numPtr2 = (byte*) &num;
                numPtr2[0] = numPtr[0];
                numPtr2[1] = numPtr[1];
                numPtr2[2] = numPtr[2];
                numPtr2[3] = numPtr[3];
                num2 = num;
            }
            catch (NullReferenceException)
            {
                throw new AccessViolationException();
            }
            return num2;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("mscoree.dll", EntryPoint="ND_RI4")]
        public static extern int ReadInt32([In, MarshalAs(UnmanagedType.AsAny)] object ptr, int ofs);
        [SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static long ReadInt64(IntPtr ptr)
        {
            return ReadInt64(ptr, 0);
        }

        [SecurityCritical]
        public static unsafe long ReadInt64(IntPtr ptr, int ofs)
        {
            long num2;
            try
            {
                long num;
                byte* numPtr = (byte*) (((void*) ptr) + ofs);
                if ((((int) numPtr) & 7) == 0)
                {
                    return *(((long*) numPtr));
                }
                byte* numPtr2 = (byte*) &num;
                numPtr2[0] = numPtr[0];
                numPtr2[1] = numPtr[1];
                numPtr2[2] = numPtr[2];
                numPtr2[3] = numPtr[3];
                numPtr2[4] = numPtr[4];
                numPtr2[5] = numPtr[5];
                numPtr2[6] = numPtr[6];
                numPtr2[7] = numPtr[7];
                num2 = num;
            }
            catch (NullReferenceException)
            {
                throw new AccessViolationException();
            }
            return num2;
        }

        [SuppressUnmanagedCodeSecurity, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecurityCritical, DllImport("mscoree.dll", EntryPoint="ND_RI8")]
        public static extern long ReadInt64([In, MarshalAs(UnmanagedType.AsAny)] object ptr, int ofs);
        [SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static IntPtr ReadIntPtr(IntPtr ptr)
        {
            return (IntPtr) ReadInt32(ptr, 0);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecurityCritical]
        public static IntPtr ReadIntPtr(IntPtr ptr, int ofs)
        {
            return (IntPtr) ReadInt32(ptr, ofs);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecurityCritical]
        public static IntPtr ReadIntPtr([In, MarshalAs(UnmanagedType.AsAny)] object ptr, int ofs)
        {
            return (IntPtr) ReadInt32(ptr, ofs);
        }

        [SecurityCritical]
        public static IntPtr ReAllocCoTaskMem(IntPtr pv, int cb)
        {
            IntPtr ptr = Win32Native.CoTaskMemRealloc(pv, cb);
            if ((ptr == Win32Native.NULL) && (cb != 0))
            {
                throw new OutOfMemoryException();
            }
            return ptr;
        }

        [SecurityCritical]
        public static IntPtr ReAllocHGlobal(IntPtr pv, IntPtr cb)
        {
            IntPtr ptr = Win32Native.LocalReAlloc(pv, cb, 2);
            if (ptr == Win32Native.NULL)
            {
                throw new OutOfMemoryException();
            }
            return ptr;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static extern int Release(IntPtr pUnk);
        [SecurityCritical]
        public static int ReleaseComObject(object o)
        {
            __ComObject obj2 = null;
            try
            {
                obj2 = (__ComObject) o;
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_ObjNotComObject"), "o");
            }
            return obj2.ReleaseSelf();
        }

        [SecurityCritical, Obsolete("This API did not perform any operation and will be removed in future versions of the CLR.", false)]
        public static void ReleaseThreadCache()
        {
        }

        [SecurityCritical]
        public static IntPtr SecureStringToBSTR(SecureString s)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            return s.ToBSTR();
        }

        [SecurityCritical]
        public static IntPtr SecureStringToCoTaskMemAnsi(SecureString s)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            return s.ToAnsiStr(false);
        }

        [SecurityCritical]
        public static IntPtr SecureStringToCoTaskMemUnicode(SecureString s)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            return s.ToUniStr(false);
        }

        [SecurityCritical]
        public static IntPtr SecureStringToGlobalAllocAnsi(SecureString s)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            return s.ToAnsiStr(true);
        }

        [SecurityCritical]
        public static IntPtr SecureStringToGlobalAllocUnicode(SecureString s)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            return s.ToUniStr(true);
        }

        [SecurityCritical]
        public static bool SetComObjectData(object obj, object key, object data)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            __ComObject obj2 = null;
            try
            {
                obj2 = (__ComObject) obj;
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_ObjNotComObject"), "obj");
            }
            return obj2.SetData(key, data);
        }

        [MethodImpl(MethodImplOptions.InternalCall), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static extern void SetLastWin32Error(int error);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static uint SizeOf<T>() where T: struct
        {
            return SizeOfType(typeof(T));
        }

        [ComVisible(true)]
        public static int SizeOf(object structure)
        {
            if (structure == null)
            {
                throw new ArgumentNullException("structure");
            }
            return SizeOfHelper(structure.GetType(), true);
        }

        public static int SizeOf(Type t)
        {
            if (t == null)
            {
                throw new ArgumentNullException("t");
            }
            if (!(t is RuntimeType))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"), "t");
            }
            if (t.IsGenericType)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_NeedNonGenericType"), "t");
            }
            return SizeOfHelper(t, true);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        internal static extern int SizeOfHelper(Type t, bool throwIfNotMarshalable);
        [MethodImpl(MethodImplOptions.InternalCall), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static extern uint SizeOfType(Type type);
        [SecurityCritical]
        public static IntPtr StringToBSTR(string s)
        {
            if (s == null)
            {
                return Win32Native.NULL;
            }
            if ((s.Length + 1) < s.Length)
            {
                throw new ArgumentOutOfRangeException("s");
            }
            IntPtr ptr = Win32Native.SysAllocStringLen(s, s.Length);
            if (ptr == Win32Native.NULL)
            {
                throw new OutOfMemoryException();
            }
            return ptr;
        }

        [SecurityCritical]
        public static IntPtr StringToCoTaskMemAnsi(string s)
        {
            if (s == null)
            {
                return Win32Native.NULL;
            }
            int cb = (s.Length + 1) * SystemMaxDBCSCharSize;
            if (cb < s.Length)
            {
                throw new ArgumentOutOfRangeException("s");
            }
            IntPtr pdst = Win32Native.CoTaskMemAlloc(cb);
            if (pdst == Win32Native.NULL)
            {
                throw new OutOfMemoryException();
            }
            Win32Native.CopyMemoryAnsi(pdst, new StringBuilder(s, cb), new IntPtr(cb));
            return pdst;
        }

        [SecurityCritical]
        public static IntPtr StringToCoTaskMemAuto(string s)
        {
            return StringToCoTaskMemUni(s);
        }

        [SecurityCritical]
        public static IntPtr StringToCoTaskMemUni(string s)
        {
            if (s == null)
            {
                return Win32Native.NULL;
            }
            int cb = (s.Length + 1) * 2;
            if (cb < s.Length)
            {
                throw new ArgumentOutOfRangeException("s");
            }
            IntPtr pdst = Win32Native.CoTaskMemAlloc(cb);
            if (pdst == Win32Native.NULL)
            {
                throw new OutOfMemoryException();
            }
            Win32Native.CopyMemoryUni(pdst, s, new IntPtr(cb));
            return pdst;
        }

        [SecurityCritical]
        public static IntPtr StringToHGlobalAnsi(string s)
        {
            if (s == null)
            {
                return Win32Native.NULL;
            }
            int num = (s.Length + 1) * SystemMaxDBCSCharSize;
            if (num < s.Length)
            {
                throw new ArgumentOutOfRangeException("s");
            }
            IntPtr sizetdwBytes = new IntPtr(num);
            IntPtr pdst = Win32Native.LocalAlloc_NoSafeHandle(0, sizetdwBytes);
            if (pdst == Win32Native.NULL)
            {
                throw new OutOfMemoryException();
            }
            Win32Native.CopyMemoryAnsi(pdst, new StringBuilder(s, num), sizetdwBytes);
            return pdst;
        }

        [SecurityCritical]
        public static IntPtr StringToHGlobalAuto(string s)
        {
            return StringToHGlobalUni(s);
        }

        [SecurityCritical]
        public static IntPtr StringToHGlobalUni(string s)
        {
            if (s == null)
            {
                return Win32Native.NULL;
            }
            int num = (s.Length + 1) * 2;
            if (num < s.Length)
            {
                throw new ArgumentOutOfRangeException("s");
            }
            IntPtr sizetdwBytes = new IntPtr(num);
            IntPtr pdst = Win32Native.LocalAlloc_NoSafeHandle(0, sizetdwBytes);
            if (pdst == Win32Native.NULL)
            {
                throw new OutOfMemoryException();
            }
            Win32Native.CopyMemoryUni(pdst, s, sizetdwBytes);
            return pdst;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), ComVisible(true)]
        public static extern void StructureToPtr(object structure, IntPtr ptr, bool fDeleteOld);
        [SecurityCritical]
        public static void ThrowExceptionForHR(int errorCode)
        {
            if (errorCode < 0)
            {
                ThrowExceptionForHRInternal(errorCode, Win32Native.NULL);
            }
        }

        [SecurityCritical]
        public static void ThrowExceptionForHR(int errorCode, IntPtr errorInfo)
        {
            if (errorCode < 0)
            {
                ThrowExceptionForHRInternal(errorCode, errorInfo);
            }
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern void ThrowExceptionForHRInternal(int errorCode, IntPtr errorInfo);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public static extern IntPtr UnsafeAddrOfPinnedArrayElement(Array arr, int index);
        [SecurityCritical]
        public static void WriteByte(IntPtr ptr, byte val)
        {
            WriteByte(ptr, 0, val);
        }

        [SecurityCritical]
        public static unsafe void WriteByte(IntPtr ptr, int ofs, byte val)
        {
            try
            {
                byte* numPtr = (byte*) (((void*) ptr) + ofs);
                numPtr[0] = val;
            }
            catch (NullReferenceException)
            {
                throw new AccessViolationException();
            }
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("mscoree.dll", EntryPoint="ND_WU1")]
        public static extern void WriteByte([In, Out, MarshalAs(UnmanagedType.AsAny)] object ptr, int ofs, byte val);
        [SecurityCritical]
        public static void WriteInt16(IntPtr ptr, char val)
        {
            WriteInt16(ptr, 0, (short) val);
        }

        [SecurityCritical]
        public static void WriteInt16(IntPtr ptr, short val)
        {
            WriteInt16(ptr, 0, val);
        }

        [SecurityCritical]
        public static void WriteInt16(IntPtr ptr, int ofs, char val)
        {
            WriteInt16(ptr, ofs, (short) val);
        }

        [SecurityCritical]
        public static unsafe void WriteInt16(IntPtr ptr, int ofs, short val)
        {
            try
            {
                byte* numPtr = (byte*) (((void*) ptr) + ofs);
                if ((((int) numPtr) & 1) == 0)
                {
                    *((short*) numPtr) = val;
                }
                else
                {
                    byte* numPtr2 = (byte*) &val;
                    numPtr[0] = numPtr2[0];
                    numPtr[1] = numPtr2[1];
                }
            }
            catch (NullReferenceException)
            {
                throw new AccessViolationException();
            }
        }

        [SecurityCritical]
        public static void WriteInt16([In, Out] object ptr, int ofs, char val)
        {
            WriteInt16(ptr, ofs, (short) val);
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("mscoree.dll", EntryPoint="ND_WI2")]
        public static extern void WriteInt16([In, Out, MarshalAs(UnmanagedType.AsAny)] object ptr, int ofs, short val);
        [SecurityCritical]
        public static void WriteInt32(IntPtr ptr, int val)
        {
            WriteInt32(ptr, 0, val);
        }

        [SecurityCritical]
        public static unsafe void WriteInt32(IntPtr ptr, int ofs, int val)
        {
            try
            {
                byte* numPtr = (byte*) (((void*) ptr) + ofs);
                if ((((int) numPtr) & 3) == 0)
                {
                    *((int*) numPtr) = val;
                }
                else
                {
                    byte* numPtr2 = (byte*) &val;
                    numPtr[0] = numPtr2[0];
                    numPtr[1] = numPtr2[1];
                    numPtr[2] = numPtr2[2];
                    numPtr[3] = numPtr2[3];
                }
            }
            catch (NullReferenceException)
            {
                throw new AccessViolationException();
            }
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("mscoree.dll", EntryPoint="ND_WI4")]
        public static extern void WriteInt32([In, Out, MarshalAs(UnmanagedType.AsAny)] object ptr, int ofs, int val);
        [SecurityCritical]
        public static void WriteInt64(IntPtr ptr, long val)
        {
            WriteInt64(ptr, 0, val);
        }

        [SecurityCritical]
        public static unsafe void WriteInt64(IntPtr ptr, int ofs, long val)
        {
            try
            {
                byte* numPtr = (byte*) (((void*) ptr) + ofs);
                if ((((int) numPtr) & 7) == 0)
                {
                    *((long*) numPtr) = val;
                }
                else
                {
                    byte* numPtr2 = (byte*) &val;
                    numPtr[0] = numPtr2[0];
                    numPtr[1] = numPtr2[1];
                    numPtr[2] = numPtr2[2];
                    numPtr[3] = numPtr2[3];
                    numPtr[4] = numPtr2[4];
                    numPtr[5] = numPtr2[5];
                    numPtr[6] = numPtr2[6];
                    numPtr[7] = numPtr2[7];
                }
            }
            catch (NullReferenceException)
            {
                throw new AccessViolationException();
            }
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("mscoree.dll", EntryPoint="ND_WI8")]
        public static extern void WriteInt64([In, Out, MarshalAs(UnmanagedType.AsAny)] object ptr, int ofs, long val);
        [SecurityCritical]
        public static void WriteIntPtr(IntPtr ptr, IntPtr val)
        {
            WriteInt32(ptr, 0, (int) val);
        }

        [SecurityCritical]
        public static void WriteIntPtr(IntPtr ptr, int ofs, IntPtr val)
        {
            WriteInt32(ptr, ofs, (int) val);
        }

        [SecurityCritical]
        public static void WriteIntPtr([In, Out, MarshalAs(UnmanagedType.AsAny)] object ptr, int ofs, IntPtr val)
        {
            WriteInt32(ptr, ofs, (int) val);
        }

        [SecurityCritical]
        public static void ZeroFreeBSTR(IntPtr s)
        {
            Win32Native.ZeroMemory(s, (uint) (Win32Native.SysStringLen(s) * 2));
            FreeBSTR(s);
        }

        [SecurityCritical]
        public static void ZeroFreeCoTaskMemAnsi(IntPtr s)
        {
            Win32Native.ZeroMemory(s, (uint) Win32Native.lstrlenA(s));
            FreeCoTaskMem(s);
        }

        [SecurityCritical]
        public static void ZeroFreeCoTaskMemUnicode(IntPtr s)
        {
            Win32Native.ZeroMemory(s, (uint) (Win32Native.lstrlenW(s) * 2));
            FreeCoTaskMem(s);
        }

        [SecurityCritical]
        public static void ZeroFreeGlobalAllocAnsi(IntPtr s)
        {
            Win32Native.ZeroMemory(s, (uint) Win32Native.lstrlenA(s));
            FreeHGlobal(s);
        }

        [SecurityCritical]
        public static void ZeroFreeGlobalAllocUnicode(IntPtr s)
        {
            Win32Native.ZeroMemory(s, (uint) (Win32Native.lstrlenW(s) * 2));
            FreeHGlobal(s);
        }
    }
}

