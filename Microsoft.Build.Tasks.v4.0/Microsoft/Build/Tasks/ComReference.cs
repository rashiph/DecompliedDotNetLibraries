namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Shared;
    using Microsoft.Build.Utilities;
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Text;

    internal abstract class ComReference
    {
        internal static string ado27ErrorMessage;
        internal static bool ado27Installed;
        internal static bool ado27PropertyInitialized = false;
        private static readonly Guid guidADO20 = new Guid("{00000200-0000-0010-8000-00AA006D2EA4}");
        private static readonly Guid guidADO21 = new Guid("{00000201-0000-0010-8000-00AA006D2EA4}");
        private static readonly Guid guidADO25 = new Guid("{00000205-0000-0010-8000-00AA006D2EA4}");
        private static readonly Guid guidADO26 = new Guid("{00000206-0000-0010-8000-00AA006D2EA4}");
        private static Guid guidADO27 = new Guid("{EF53050B-882E-4776-B643-EDA472E8E3F2}");
        private string itemName;
        private TaskLoggingHelper log;
        private ComReferenceInfo referenceInfo;

        internal ComReference(TaskLoggingHelper taskLoggingHelper, ComReferenceInfo referenceInfo, string itemName)
        {
            this.referenceInfo = referenceInfo;
            this.itemName = itemName;
            this.log = taskLoggingHelper;
        }

        internal static bool AreTypeLibAttrEqual(System.Runtime.InteropServices.ComTypes.TYPELIBATTR attr1, System.Runtime.InteropServices.ComTypes.TYPELIBATTR attr2)
        {
            return ((((attr1.wMajorVerNum == attr2.wMajorVerNum) && (attr1.wMinorVerNum == attr2.wMinorVerNum)) && (attr1.lcid == attr2.lcid)) && (attr1.guid == attr2.guid));
        }

        internal abstract bool FindExistingWrapper(out ComReferenceWrapperInfo wrapperInfo, DateTime componentTimestamp);
        internal static void GetFuncDescForDescIndex(ITypeInfo typeInfo, int funcIndex, out System.Runtime.InteropServices.ComTypes.FUNCDESC funcDesc, out IntPtr funcDescHandle)
        {
            IntPtr zero = IntPtr.Zero;
            typeInfo.GetFuncDesc(funcIndex, out zero);
            if (zero == IntPtr.Zero)
            {
                throw new COMException(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("ResolveComReference.CannotRetrieveTypeInformation", new object[0]));
            }
            funcDesc = (System.Runtime.InteropServices.ComTypes.FUNCDESC) Marshal.PtrToStructure(zero, typeof(System.Runtime.InteropServices.ComTypes.FUNCDESC));
            funcDescHandle = zero;
        }

        internal static bool GetPathOfTypeLib(TaskLoggingHelper log, ref System.Runtime.InteropServices.ComTypes.TYPELIBATTR typeLibAttr, out string typeLibPath)
        {
            typeLibPath = "";
            try
            {
                typeLibPath = Microsoft.Build.Tasks.NativeMethods.QueryPathOfRegTypeLib(ref typeLibAttr.guid, typeLibAttr.wMajorVerNum, typeLibAttr.wMinorVerNum, typeLibAttr.lcid);
                typeLibPath = Environment.ExpandEnvironmentVariables(typeLibPath);
            }
            catch (COMException exception)
            {
                log.LogWarningWithCodeFromResources("ResolveComReference.CannotGetPathForTypeLib", new object[] { typeLibAttr.guid, typeLibAttr.wMajorVerNum, typeLibAttr.wMinorVerNum, exception.Message });
                return false;
            }
            if (((typeLibPath != null) && (typeLibPath.Length > 0)) && (typeLibPath[typeLibPath.Length - 1] == '\0'))
            {
                typeLibPath = typeLibPath.Substring(0, typeLibPath.Length - 1);
            }
            if ((typeLibPath != null) && (typeLibPath.Length > 0))
            {
                return true;
            }
            log.LogWarningWithCodeFromResources("ResolveComReference.CannotGetPathForTypeLib", new object[] { typeLibAttr.guid, typeLibAttr.wMajorVerNum, typeLibAttr.wMinorVerNum, "" });
            return false;
        }

        internal static void GetTypeAttrForTypeInfo(ITypeInfo typeInfo, out System.Runtime.InteropServices.ComTypes.TYPEATTR typeAttr)
        {
            IntPtr zero = IntPtr.Zero;
            typeInfo.GetTypeAttr(out zero);
            if (zero == IntPtr.Zero)
            {
                throw new COMException(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("ResolveComReference.CannotRetrieveTypeInformation", new object[0]));
            }
            try
            {
                typeAttr = (System.Runtime.InteropServices.ComTypes.TYPEATTR) Marshal.PtrToStructure(zero, typeof(System.Runtime.InteropServices.ComTypes.TYPEATTR));
            }
            finally
            {
                typeInfo.ReleaseTypeAttr(zero);
            }
        }

        internal static void GetTypeLibAttrForTypeLib(ref ITypeLib typeLib, out System.Runtime.InteropServices.ComTypes.TYPELIBATTR typeLibAttr)
        {
            IntPtr zero = IntPtr.Zero;
            typeLib.GetLibAttr(out zero);
            if (zero == IntPtr.Zero)
            {
                throw new COMException(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("ResolveComReference.CannotGetTypeLibAttrForTypeLib", new object[0]));
            }
            try
            {
                typeLibAttr = (System.Runtime.InteropServices.ComTypes.TYPELIBATTR) Marshal.PtrToStructure(zero, typeof(System.Runtime.InteropServices.ComTypes.TYPELIBATTR));
            }
            finally
            {
                typeLib.ReleaseTLibAttr(zero);
            }
        }

        internal static bool GetTypeLibNameForITypeLib(TaskLoggingHelper log, ITypeLib typeLib, string typeLibId, out string typeLibName)
        {
            typeLibName = "";
            ITypeLib2 lib = typeLib as ITypeLib2;
            if (lib == null)
            {
                typeLibName = Marshal.GetTypeLibName(typeLib);
                return true;
            }
            try
            {
                object pVarVal = null;
                lib.GetCustData(ref Microsoft.Build.Tasks.NativeMethods.GUID_TYPELIB_NAMESPACE, out pVarVal);
                if ((pVarVal == null) || (string.Compare(pVarVal.GetType().ToString(), "system.string", StringComparison.OrdinalIgnoreCase) != 0))
                {
                    typeLibName = Marshal.GetTypeLibName(typeLib);
                    return true;
                }
                typeLibName = (string) pVarVal;
                if ((typeLibName.Length >= 4) && (string.Compare(typeLibName.Substring(typeLibName.Length - 4), ".dll", StringComparison.OrdinalIgnoreCase) == 0))
                {
                    typeLibName = typeLibName.Substring(0, typeLibName.Length - 4);
                }
            }
            catch (COMException exception)
            {
                log.LogWarningWithCodeFromResources("ResolveComReference.CannotAccessTypeLibName", new object[] { typeLibId, exception.Message });
                typeLibName = Marshal.GetTypeLibName(typeLib);
                return true;
            }
            return true;
        }

        internal static bool GetTypeLibNameForTypeLibAttrs(TaskLoggingHelper log, System.Runtime.InteropServices.ComTypes.TYPELIBATTR typeLibAttr, out string typeLibName)
        {
            bool flag;
            typeLibName = "";
            ITypeLib typeLib = null;
            try
            {
                try
                {
                    System.Runtime.InteropServices.ComTypes.TYPELIBATTR typelibattr = typeLibAttr;
                    typeLib = (ITypeLib) Microsoft.Build.Tasks.NativeMethods.LoadRegTypeLib(ref typelibattr.guid, typelibattr.wMajorVerNum, typelibattr.wMinorVerNum, typelibattr.lcid);
                }
                catch (COMException exception)
                {
                    log.LogWarningWithCodeFromResources("ResolveComReference.CannotLoadTypeLib", new object[] { typeLibAttr.guid, typeLibAttr.wMajorVerNum, typeLibAttr.wMinorVerNum, exception.Message });
                    return false;
                }
                string typeLibId = log.FormatResourceString("ResolveComReference.TypeLibAttrId", new object[] { typeLibAttr.guid.ToString(), typeLibAttr.wMajorVerNum, typeLibAttr.wMinorVerNum });
                flag = GetTypeLibNameForITypeLib(log, typeLib, typeLibId, out typeLibName);
            }
            finally
            {
                if (typeLib != null)
                {
                    Marshal.ReleaseComObject(typeLib);
                }
            }
            return flag;
        }

        internal static void GetVarDescForVarIndex(ITypeInfo typeInfo, int varIndex, out System.Runtime.InteropServices.ComTypes.VARDESC varDesc, out IntPtr varDescHandle)
        {
            IntPtr zero = IntPtr.Zero;
            typeInfo.GetVarDesc(varIndex, out zero);
            if (zero == IntPtr.Zero)
            {
                throw new COMException(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("ResolveComReference.CannotRetrieveTypeInformation", new object[0]));
            }
            varDesc = (System.Runtime.InteropServices.ComTypes.VARDESC) Marshal.PtrToStructure(zero, typeof(System.Runtime.InteropServices.ComTypes.VARDESC));
            varDescHandle = zero;
        }

        internal static bool RemapAdoTypeLib(TaskLoggingHelper log, ref System.Runtime.InteropServices.ComTypes.TYPELIBATTR typeLibAttr)
        {
            if ((typeLibAttr.wMajorVerNum != 2) || ((((typeLibAttr.wMinorVerNum != 0) || (typeLibAttr.guid != guidADO20)) && ((typeLibAttr.wMinorVerNum != 1) || (typeLibAttr.guid != guidADO21))) && (((typeLibAttr.wMinorVerNum != 5) || (typeLibAttr.guid != guidADO25)) && ((typeLibAttr.wMinorVerNum != 6) || !(typeLibAttr.guid == guidADO26)))))
            {
                return false;
            }
            if (!Ado27Installed)
            {
                log.LogWarningWithCodeFromResources("ResolveComReference.FailedToRemapAdoTypeLib", new object[] { typeLibAttr.wMajorVerNum, typeLibAttr.wMinorVerNum, Ado27ErrorMessage });
                return false;
            }
            typeLibAttr.guid = guidADO27;
            typeLibAttr.wMajorVerNum = 2;
            typeLibAttr.wMinorVerNum = 7;
            typeLibAttr.lcid = 0;
            return true;
        }

        internal static string StripTypeLibNumberFromPath(string typeLibPath, Microsoft.Build.Shared.FileExists fileExists)
        {
            bool flag = false;
            if (((typeLibPath != null) && (typeLibPath.Length > 0)) && !fileExists(typeLibPath))
            {
                int length = typeLibPath.LastIndexOf('\\');
                if (length == -1)
                {
                    flag = true;
                }
                else
                {
                    bool flag2 = true;
                    for (int i = length + 1; i < typeLibPath.Length; i++)
                    {
                        if (!char.IsDigit(typeLibPath[i]))
                        {
                            flag2 = false;
                            break;
                        }
                    }
                    if (flag2)
                    {
                        typeLibPath = typeLibPath.Substring(0, length);
                        if (!fileExists(typeLibPath))
                        {
                            flag = true;
                        }
                    }
                    else
                    {
                        flag = true;
                    }
                }
            }
            if (flag)
            {
                IntPtr handle = Microsoft.Build.Shared.NativeMethodsShared.LoadLibrary(typeLibPath);
                if (IntPtr.Zero != handle)
                {
                    try
                    {
                        StringBuilder wrapper = new StringBuilder(Microsoft.Build.Shared.NativeMethodsShared.MAX_PATH);
                        HandleRef hModule = new HandleRef(wrapper, handle);
                        if ((Microsoft.Build.Shared.NativeMethodsShared.GetModuleFileName(hModule, wrapper, wrapper.Capacity) != 0) && (Marshal.GetLastWin32Error() != -2147024774))
                        {
                            typeLibPath = wrapper.ToString();
                            return typeLibPath;
                        }
                        typeLibPath = "";
                        return typeLibPath;
                    }
                    finally
                    {
                        Microsoft.Build.Shared.NativeMethodsShared.FreeLibrary(handle);
                    }
                }
                typeLibPath = "";
            }
            return typeLibPath;
        }

        internal static string UniqueKeyFromTypeLibAttr(System.Runtime.InteropServices.ComTypes.TYPELIBATTR attr)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}|{1}.{2}|{3}", new object[] { attr.guid, attr.wMajorVerNum, attr.wMinorVerNum, attr.lcid });
        }

        internal static string Ado27ErrorMessage
        {
            get
            {
                return ado27ErrorMessage;
            }
        }

        internal static bool Ado27Installed
        {
            get
            {
                if (!ado27PropertyInitialized)
                {
                    ado27Installed = true;
                    ado27PropertyInitialized = true;
                    ITypeLib o = null;
                    try
                    {
                        o = (ITypeLib) Microsoft.Build.Tasks.NativeMethods.LoadRegTypeLib(ref guidADO27, 2, 7, 0);
                    }
                    catch (COMException exception)
                    {
                        ado27Installed = false;
                        ado27ErrorMessage = exception.Message;
                    }
                    finally
                    {
                        if (o != null)
                        {
                            Marshal.ReleaseComObject(o);
                        }
                    }
                }
                return ado27Installed;
            }
        }

        internal virtual string ItemName
        {
            get
            {
                return this.itemName;
            }
        }

        protected internal TaskLoggingHelper Log
        {
            get
            {
                return this.log;
            }
        }

        internal virtual ComReferenceInfo ReferenceInfo
        {
            get
            {
                return this.referenceInfo;
            }
        }
    }
}

