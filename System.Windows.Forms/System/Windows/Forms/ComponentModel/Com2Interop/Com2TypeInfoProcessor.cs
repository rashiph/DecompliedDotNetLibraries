namespace System.Windows.Forms.ComponentModel.Com2Interop
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Forms;

    internal class Com2TypeInfoProcessor
    {
        private static Hashtable builtEnums;
        private static TraceSwitch DbgTypeInfoProcessorSwitch;
        private static System.Reflection.Emit.ModuleBuilder moduleBuilder = null;
        private static Hashtable processedLibraries;

        private Com2TypeInfoProcessor()
        {
        }

        public static UnsafeNativeMethods.ITypeInfo FindTypeInfo(object obj, bool wantCoClass)
        {
            UnsafeNativeMethods.ITypeInfo classInfo = null;
            for (int i = 0; (classInfo == null) && (i < 2); i++)
            {
                if (wantCoClass == (i == 0))
                {
                    if (obj is System.Windows.Forms.NativeMethods.IProvideClassInfo)
                    {
                        System.Windows.Forms.NativeMethods.IProvideClassInfo info2 = (System.Windows.Forms.NativeMethods.IProvideClassInfo) obj;
                        try
                        {
                            classInfo = info2.GetClassInfo();
                        }
                        catch
                        {
                        }
                    }
                }
                else if (obj is UnsafeNativeMethods.IDispatch)
                {
                    UnsafeNativeMethods.IDispatch dispatch = (UnsafeNativeMethods.IDispatch) obj;
                    try
                    {
                        classInfo = dispatch.GetTypeInfo(0, SafeNativeMethods.GetThreadLCID());
                    }
                    catch
                    {
                    }
                }
            }
            return classInfo;
        }

        public static UnsafeNativeMethods.ITypeInfo[] FindTypeInfos(object obj, bool wantCoClass)
        {
            UnsafeNativeMethods.ITypeInfo[] infoArray = null;
            int pcti = 0;
            UnsafeNativeMethods.ITypeInfo pTypeInfo = null;
            if (obj is System.Windows.Forms.NativeMethods.IProvideMultipleClassInfo)
            {
                System.Windows.Forms.NativeMethods.IProvideMultipleClassInfo info2 = (System.Windows.Forms.NativeMethods.IProvideMultipleClassInfo) obj;
                if (!System.Windows.Forms.NativeMethods.Succeeded(info2.GetMultiTypeInfoCount(ref pcti)) || (pcti == 0))
                {
                    pcti = 0;
                }
                if (pcti > 0)
                {
                    infoArray = new UnsafeNativeMethods.ITypeInfo[pcti];
                    for (int i = 0; i < pcti; i++)
                    {
                        if (!System.Windows.Forms.NativeMethods.Failed(info2.GetInfoOfIndex(i, 1, ref pTypeInfo, 0, 0, IntPtr.Zero, IntPtr.Zero)))
                        {
                            infoArray[i] = pTypeInfo;
                        }
                    }
                }
            }
            if ((infoArray == null) || (infoArray.Length == 0))
            {
                pTypeInfo = FindTypeInfo(obj, wantCoClass);
                if (pTypeInfo != null)
                {
                    infoArray = new UnsafeNativeMethods.ITypeInfo[] { pTypeInfo };
                }
            }
            return infoArray;
        }

        private static Guid GetGuidForTypeInfo(UnsafeNativeMethods.ITypeInfo typeInfo, StructCache structCache, int[] versions)
        {
            IntPtr zero = IntPtr.Zero;
            int typeAttr = typeInfo.GetTypeAttr(ref zero);
            if (!System.Windows.Forms.NativeMethods.Succeeded(typeAttr))
            {
                throw new ExternalException(System.Windows.Forms.SR.GetString("TYPEINFOPROCESSORGetTypeAttrFailed", new object[] { typeAttr }), typeAttr);
            }
            Guid empty = Guid.Empty;
            System.Windows.Forms.NativeMethods.tagTYPEATTR data = null;
            try
            {
                if (structCache == null)
                {
                    data = new System.Windows.Forms.NativeMethods.tagTYPEATTR();
                }
                else
                {
                    data = (System.Windows.Forms.NativeMethods.tagTYPEATTR) structCache.GetStruct(typeof(System.Windows.Forms.NativeMethods.tagTYPEATTR));
                }
                UnsafeNativeMethods.PtrToStructure(zero, data);
                empty = data.guid;
                if (versions != null)
                {
                    versions[0] = data.wMajorVerNum;
                    versions[1] = data.wMinorVerNum;
                }
            }
            finally
            {
                typeInfo.ReleaseTypeAttr(zero);
                if ((structCache != null) && (data != null))
                {
                    structCache.ReleaseStruct(data);
                }
            }
            return empty;
        }

        public static int GetNameDispId(UnsafeNativeMethods.IDispatch obj)
        {
            int num = -1;
            string[] rgszNames = null;
            ComNativeDescriptor instance = ComNativeDescriptor.Instance;
            bool succeeded = false;
            instance.GetPropertyValue(obj, "__id", ref succeeded);
            if (succeeded)
            {
                rgszNames = new string[] { "__id" };
            }
            else
            {
                instance.GetPropertyValue(obj, -800, ref succeeded);
                if (succeeded)
                {
                    num = -800;
                }
                else
                {
                    instance.GetPropertyValue(obj, "Name", ref succeeded);
                    if (succeeded)
                    {
                        rgszNames = new string[] { "Name" };
                    }
                }
            }
            if (rgszNames != null)
            {
                int[] rgDispId = new int[] { -1 };
                Guid empty = Guid.Empty;
                if (System.Windows.Forms.NativeMethods.Succeeded(obj.GetIDsOfNames(ref empty, rgszNames, 1, SafeNativeMethods.GetThreadLCID(), rgDispId)))
                {
                    num = rgDispId[0];
                }
            }
            return num;
        }

        public static Com2Properties GetProperties(object obj)
        {
            if ((obj == null) || !Marshal.IsComObject(obj))
            {
                return null;
            }
            UnsafeNativeMethods.ITypeInfo[] infoArray = FindTypeInfos(obj, false);
            if ((infoArray == null) || (infoArray.Length == 0))
            {
                return null;
            }
            int defaultIndex = -1;
            int num2 = -1;
            ArrayList list = new ArrayList();
            for (int i = 0; i < infoArray.Length; i++)
            {
                UnsafeNativeMethods.ITypeInfo typeInfo = infoArray[i];
                if (typeInfo != null)
                {
                    int[] versions = new int[2];
                    Guid key = GetGuidForTypeInfo(typeInfo, null, versions);
                    PropertyDescriptor[] props = null;
                    bool flag = ((key != Guid.Empty) && (processedLibraries != null)) && processedLibraries.Contains(key);
                    if (flag)
                    {
                        CachedProperties properties = (CachedProperties) processedLibraries[key];
                        if ((versions[0] == properties.MajorVersion) && (versions[1] == properties.MinorVersion))
                        {
                            props = properties.Properties;
                            if ((i == 0) && (properties.DefaultIndex != -1))
                            {
                                defaultIndex = properties.DefaultIndex;
                            }
                        }
                        else
                        {
                            flag = false;
                        }
                    }
                    if (!flag)
                    {
                        props = InternalGetProperties(obj, typeInfo, -1, ref num2);
                        if ((i == 0) && (num2 != -1))
                        {
                            defaultIndex = num2;
                        }
                        if (processedLibraries == null)
                        {
                            processedLibraries = new Hashtable();
                        }
                        if (key != Guid.Empty)
                        {
                            processedLibraries[key] = new CachedProperties(props, (i == 0) ? defaultIndex : -1, versions[0], versions[1]);
                        }
                    }
                    if (props != null)
                    {
                        list.AddRange(props);
                    }
                }
            }
            Com2PropertyDescriptor[] array = new Com2PropertyDescriptor[list.Count];
            list.CopyTo(array, 0);
            return new Com2Properties(obj, array, defaultIndex);
        }

        private static System.Type GetValueTypeFromTypeDesc(System.Windows.Forms.NativeMethods.tagTYPEDESC typeDesc, UnsafeNativeMethods.ITypeInfo typeInfo, object[] typeData, StructCache structCache)
        {
            IntPtr unionMember;
            int hr = 0;
            switch (((System.Windows.Forms.NativeMethods.tagVT) typeDesc.vt))
            {
                case System.Windows.Forms.NativeMethods.tagVT.VT_DISPATCH:
                case System.Windows.Forms.NativeMethods.tagVT.VT_UNKNOWN:
                    typeData[0] = GetGuidForTypeInfo(typeInfo, structCache, null);
                    return VTToType((System.Windows.Forms.NativeMethods.tagVT) typeDesc.vt);

                case System.Windows.Forms.NativeMethods.tagVT.VT_PTR:
                {
                    System.Windows.Forms.NativeMethods.tagTYPEDESC data = (System.Windows.Forms.NativeMethods.tagTYPEDESC) structCache.GetStruct(typeof(System.Windows.Forms.NativeMethods.tagTYPEDESC));
                    try
                    {
                        try
                        {
                            UnsafeNativeMethods.PtrToStructure(typeDesc.unionMember, data);
                        }
                        catch
                        {
                            data = new System.Windows.Forms.NativeMethods.tagTYPEDESC {
                                unionMember = (IntPtr) Marshal.ReadInt32(typeDesc.unionMember),
                                vt = Marshal.ReadInt16(typeDesc.unionMember, 4)
                            };
                        }
                        if (data.vt == 12)
                        {
                            return VTToType((System.Windows.Forms.NativeMethods.tagVT) data.vt);
                        }
                        unionMember = data.unionMember;
                    }
                    finally
                    {
                        structCache.ReleaseStruct(data);
                    }
                    break;
                }
                case System.Windows.Forms.NativeMethods.tagVT.VT_USERDEFINED:
                    unionMember = typeDesc.unionMember;
                    break;

                default:
                    return VTToType((System.Windows.Forms.NativeMethods.tagVT) typeDesc.vt);
            }
            UnsafeNativeMethods.ITypeInfo pTypeInfo = null;
            hr = typeInfo.GetRefTypeInfo(unionMember, ref pTypeInfo);
            if (!System.Windows.Forms.NativeMethods.Succeeded(hr))
            {
                throw new ExternalException(System.Windows.Forms.SR.GetString("TYPEINFOPROCESSORGetRefTypeInfoFailed", new object[] { hr }), hr);
            }
            try
            {
                if (pTypeInfo != null)
                {
                    IntPtr zero = IntPtr.Zero;
                    hr = pTypeInfo.GetTypeAttr(ref zero);
                    if (!System.Windows.Forms.NativeMethods.Succeeded(hr))
                    {
                        throw new ExternalException(System.Windows.Forms.SR.GetString("TYPEINFOPROCESSORGetTypeAttrFailed", new object[] { hr }), hr);
                    }
                    System.Windows.Forms.NativeMethods.tagTYPEATTR gtypeattr = (System.Windows.Forms.NativeMethods.tagTYPEATTR) structCache.GetStruct(typeof(System.Windows.Forms.NativeMethods.tagTYPEATTR));
                    UnsafeNativeMethods.PtrToStructure(zero, gtypeattr);
                    try
                    {
                        Guid g = gtypeattr.guid;
                        if (!Guid.Empty.Equals(g))
                        {
                            typeData[0] = g;
                        }
                        switch (gtypeattr.typekind)
                        {
                            case 0:
                                return ProcessTypeInfoEnum(pTypeInfo, structCache);

                            case 3:
                            case 5:
                                return VTToType(System.Windows.Forms.NativeMethods.tagVT.VT_UNKNOWN);

                            case 4:
                                return VTToType(System.Windows.Forms.NativeMethods.tagVT.VT_DISPATCH);

                            case 6:
                                return GetValueTypeFromTypeDesc(gtypeattr.Get_tdescAlias(), pTypeInfo, typeData, structCache);
                        }
                        return null;
                    }
                    finally
                    {
                        pTypeInfo.ReleaseTypeAttr(zero);
                        structCache.ReleaseStruct(gtypeattr);
                    }
                }
            }
            finally
            {
                pTypeInfo = null;
            }
            return null;
        }

        private static PropertyDescriptor[] InternalGetProperties(object obj, UnsafeNativeMethods.ITypeInfo typeInfo, int dispidToGet, ref int defaultIndex)
        {
            if (typeInfo == null)
            {
                return null;
            }
            Hashtable propInfoList = new Hashtable();
            int nameDispId = GetNameDispId((UnsafeNativeMethods.IDispatch) obj);
            bool addAboutBox = false;
            StructCache structCache = new StructCache();
            try
            {
                ProcessFunctions(typeInfo, propInfoList, dispidToGet, nameDispId, ref addAboutBox, structCache);
            }
            catch (ExternalException)
            {
            }
            try
            {
                ProcessVariables(typeInfo, propInfoList, dispidToGet, nameDispId, structCache);
            }
            catch (ExternalException)
            {
            }
            typeInfo = null;
            int count = propInfoList.Count;
            if (addAboutBox)
            {
                count++;
            }
            PropertyDescriptor[] descriptorArray = new PropertyDescriptor[count];
            int hr = 0;
            object[] retval = new object[1];
            ComNativeDescriptor instance = ComNativeDescriptor.Instance;
            foreach (PropInfo info in propInfoList.Values)
            {
                if (!info.NonBrowsable)
                {
                    try
                    {
                        hr = instance.GetPropertyValue(obj, info.DispId, retval);
                    }
                    catch (ExternalException exception)
                    {
                        hr = exception.ErrorCode;
                    }
                    if (!System.Windows.Forms.NativeMethods.Succeeded(hr))
                    {
                        info.Attributes.Add(new BrowsableAttribute(false));
                        info.NonBrowsable = true;
                    }
                }
                else
                {
                    hr = 0;
                }
                Attribute[] array = new Attribute[info.Attributes.Count];
                info.Attributes.CopyTo(array, 0);
                descriptorArray[info.Index] = new Com2PropertyDescriptor(info.DispId, info.Name, array, info.ReadOnly != 2, info.ValueType, info.TypeData, !System.Windows.Forms.NativeMethods.Succeeded(hr));
                if (info.IsDefault)
                {
                    int index = info.Index;
                }
            }
            if (addAboutBox)
            {
                descriptorArray[descriptorArray.Length - 1] = new Com2AboutBoxPropertyDescriptor();
            }
            return descriptorArray;
        }

        private static PropInfo ProcessDataCore(UnsafeNativeMethods.ITypeInfo typeInfo, IDictionary propInfoList, int dispid, int nameDispID, System.Windows.Forms.NativeMethods.tagTYPEDESC typeDesc, int flags, StructCache structCache)
        {
            string pBstrName = null;
            string pBstrDocString = null;
            int hr = typeInfo.GetDocumentation(dispid, ref pBstrName, ref pBstrDocString, null, null);
            ComNativeDescriptor instance = ComNativeDescriptor.Instance;
            if (!System.Windows.Forms.NativeMethods.Succeeded(hr))
            {
                throw new COMException(System.Windows.Forms.SR.GetString("TYPEINFOPROCESSORGetDocumentationFailed", new object[] { dispid, hr, instance.GetClassName(typeInfo) }), hr);
            }
            if (pBstrName == null)
            {
                return null;
            }
            PropInfo info = (PropInfo) propInfoList[pBstrName];
            if (info == null)
            {
                info = new PropInfo {
                    Index = propInfoList.Count
                };
                propInfoList[pBstrName] = info;
                info.Name = pBstrName;
                info.DispId = dispid;
                info.Attributes.Add(new DispIdAttribute(info.DispId));
            }
            if (pBstrDocString != null)
            {
                info.Attributes.Add(new DescriptionAttribute(pBstrDocString));
            }
            if (info.ValueType == null)
            {
                object[] typeData = new object[1];
                try
                {
                    info.ValueType = GetValueTypeFromTypeDesc(typeDesc, typeInfo, typeData, structCache);
                }
                catch (Exception)
                {
                }
                if (info.ValueType == null)
                {
                    info.NonBrowsable = true;
                }
                if (info.NonBrowsable)
                {
                    flags |= 0x400;
                }
                if (typeData[0] != null)
                {
                    info.TypeData = typeData[0];
                }
            }
            if ((flags & 1) != 0)
            {
                info.ReadOnly = 1;
            }
            if ((((flags & 0x40) != 0) || ((flags & 0x400) != 0)) || ((info.Name[0] == '_') || (dispid == -515)))
            {
                info.Attributes.Add(new BrowsableAttribute(false));
                info.NonBrowsable = true;
            }
            if ((flags & 0x200) != 0)
            {
                info.IsDefault = true;
            }
            if (((flags & 4) != 0) && ((flags & 0x10) != 0))
            {
                info.Attributes.Add(new BindableAttribute(true));
            }
            if (dispid == nameDispID)
            {
                info.Attributes.Add(new ParenthesizePropertyNameAttribute(true));
                info.Attributes.Add(new MergablePropertyAttribute(false));
            }
            return info;
        }

        private static void ProcessFunctions(UnsafeNativeMethods.ITypeInfo typeInfo, IDictionary propInfoList, int dispidToGet, int nameDispID, ref bool addAboutBox, StructCache structCache)
        {
            IntPtr zero = IntPtr.Zero;
            int typeAttr = typeInfo.GetTypeAttr(ref zero);
            if (!System.Windows.Forms.NativeMethods.Succeeded(typeAttr) || (zero == IntPtr.Zero))
            {
                throw new ExternalException(System.Windows.Forms.SR.GetString("TYPEINFOPROCESSORGetTypeAttrFailed", new object[] { typeAttr }), typeAttr);
            }
            System.Windows.Forms.NativeMethods.tagTYPEATTR data = (System.Windows.Forms.NativeMethods.tagTYPEATTR) structCache.GetStruct(typeof(System.Windows.Forms.NativeMethods.tagTYPEATTR));
            UnsafeNativeMethods.PtrToStructure(zero, data);
            try
            {
                if (data != null)
                {
                    System.Windows.Forms.NativeMethods.tagFUNCDESC gfuncdesc = (System.Windows.Forms.NativeMethods.tagFUNCDESC) structCache.GetStruct(typeof(System.Windows.Forms.NativeMethods.tagFUNCDESC));
                    System.Windows.Forms.NativeMethods.tagELEMDESC structure = (System.Windows.Forms.NativeMethods.tagELEMDESC) structCache.GetStruct(typeof(System.Windows.Forms.NativeMethods.tagELEMDESC));
                    for (int i = 0; i < data.cFuncs; i++)
                    {
                        IntPtr pFuncDesc = IntPtr.Zero;
                        if (System.Windows.Forms.NativeMethods.Succeeded(typeInfo.GetFuncDesc(i, ref pFuncDesc)) && (pFuncDesc != IntPtr.Zero))
                        {
                            UnsafeNativeMethods.PtrToStructure(pFuncDesc, gfuncdesc);
                            try
                            {
                                if ((gfuncdesc.invkind == 1) || ((dispidToGet != -1) && (gfuncdesc.memid != dispidToGet)))
                                {
                                    if (gfuncdesc.memid == -552)
                                    {
                                        addAboutBox = true;
                                    }
                                }
                                else
                                {
                                    System.Windows.Forms.NativeMethods.tagTYPEDESC tdesc;
                                    bool flag = gfuncdesc.invkind == 2;
                                    if (flag)
                                    {
                                        if (gfuncdesc.cParams != 0)
                                        {
                                            continue;
                                        }
                                        tdesc = gfuncdesc.elemdescFunc.tdesc;
                                    }
                                    else
                                    {
                                        if ((gfuncdesc.lprgelemdescParam == IntPtr.Zero) || (gfuncdesc.cParams != 1))
                                        {
                                            continue;
                                        }
                                        Marshal.PtrToStructure(gfuncdesc.lprgelemdescParam, structure);
                                        tdesc = structure.tdesc;
                                    }
                                    PropInfo info = ProcessDataCore(typeInfo, propInfoList, gfuncdesc.memid, nameDispID, tdesc, gfuncdesc.wFuncFlags, structCache);
                                    if ((info != null) && !flag)
                                    {
                                        info.ReadOnly = 2;
                                    }
                                }
                            }
                            finally
                            {
                                typeInfo.ReleaseFuncDesc(pFuncDesc);
                            }
                        }
                    }
                    structCache.ReleaseStruct(gfuncdesc);
                    structCache.ReleaseStruct(structure);
                }
            }
            finally
            {
                typeInfo.ReleaseTypeAttr(zero);
                structCache.ReleaseStruct(data);
            }
        }

        private static System.Type ProcessTypeInfoEnum(UnsafeNativeMethods.ITypeInfo enumTypeInfo, StructCache structCache)
        {
            if (enumTypeInfo != null)
            {
                try
                {
                    IntPtr zero = IntPtr.Zero;
                    int typeAttr = enumTypeInfo.GetTypeAttr(ref zero);
                    if (!System.Windows.Forms.NativeMethods.Succeeded(typeAttr) || (zero == IntPtr.Zero))
                    {
                        throw new ExternalException(System.Windows.Forms.SR.GetString("TYPEINFOPROCESSORGetTypeAttrFailed", new object[] { typeAttr }), typeAttr);
                    }
                    System.Windows.Forms.NativeMethods.tagTYPEATTR data = (System.Windows.Forms.NativeMethods.tagTYPEATTR) structCache.GetStruct(typeof(System.Windows.Forms.NativeMethods.tagTYPEATTR));
                    UnsafeNativeMethods.PtrToStructure(zero, data);
                    if (zero == IntPtr.Zero)
                    {
                        return null;
                    }
                    try
                    {
                        int cVars = data.cVars;
                        ArrayList list = new ArrayList();
                        ArrayList list2 = new ArrayList();
                        System.Windows.Forms.NativeMethods.tagVARDESC gvardesc = (System.Windows.Forms.NativeMethods.tagVARDESC) structCache.GetStruct(typeof(System.Windows.Forms.NativeMethods.tagVARDESC));
                        object objectForNativeVariant = null;
                        string pBstrName = null;
                        string pBstrDocString = null;
                        enumTypeInfo.GetDocumentation(-1, ref pBstrName, ref pBstrDocString, null, null);
                        for (int i = 0; i < cVars; i++)
                        {
                            IntPtr pVarDesc = IntPtr.Zero;
                            if (System.Windows.Forms.NativeMethods.Succeeded(enumTypeInfo.GetVarDesc(i, ref pVarDesc)) && (pVarDesc != IntPtr.Zero))
                            {
                                try
                                {
                                    UnsafeNativeMethods.PtrToStructure(pVarDesc, gvardesc);
                                    if (((gvardesc != null) && (gvardesc.varkind == 2)) && (gvardesc.unionMember != IntPtr.Zero))
                                    {
                                        str2 = (string) (pBstrDocString = null);
                                        objectForNativeVariant = null;
                                        if (System.Windows.Forms.NativeMethods.Succeeded(enumTypeInfo.GetDocumentation(gvardesc.memid, null, ref pBstrDocString, null, null)))
                                        {
                                            string str4;
                                            try
                                            {
                                                objectForNativeVariant = Marshal.GetObjectForNativeVariant(gvardesc.unionMember);
                                            }
                                            catch (Exception)
                                            {
                                            }
                                            list2.Add(objectForNativeVariant);
                                            if (pBstrDocString != null)
                                            {
                                                str4 = pBstrDocString;
                                            }
                                            else
                                            {
                                                str4 = str2;
                                            }
                                            list.Add(str4);
                                        }
                                    }
                                }
                                finally
                                {
                                    if (pVarDesc != IntPtr.Zero)
                                    {
                                        enumTypeInfo.ReleaseVarDesc(pVarDesc);
                                    }
                                }
                            }
                        }
                        structCache.ReleaseStruct(gvardesc);
                        if (list.Count > 0)
                        {
                            IntPtr iUnknownForObject = Marshal.GetIUnknownForObject(enumTypeInfo);
                            try
                            {
                                pBstrName = iUnknownForObject.ToString() + "_" + pBstrName;
                                if (builtEnums == null)
                                {
                                    builtEnums = new Hashtable();
                                }
                                else if (builtEnums.ContainsKey(pBstrName))
                                {
                                    return (System.Type) builtEnums[pBstrName];
                                }
                                System.Type underlyingType = typeof(int);
                                if ((list2.Count > 0) && (list2[0] != null))
                                {
                                    underlyingType = list2[0].GetType();
                                }
                                EnumBuilder builder = ModuleBuilder.DefineEnum(pBstrName, TypeAttributes.Public, underlyingType);
                                for (int j = 0; j < list.Count; j++)
                                {
                                    builder.DefineLiteral((string) list[j], list2[j]);
                                }
                                System.Type type2 = builder.CreateType();
                                builtEnums[pBstrName] = type2;
                                return type2;
                            }
                            finally
                            {
                                if (iUnknownForObject != IntPtr.Zero)
                                {
                                    Marshal.Release(iUnknownForObject);
                                }
                            }
                        }
                    }
                    finally
                    {
                        enumTypeInfo.ReleaseTypeAttr(zero);
                        structCache.ReleaseStruct(data);
                    }
                }
                catch
                {
                }
            }
            return null;
        }

        private static void ProcessVariables(UnsafeNativeMethods.ITypeInfo typeInfo, IDictionary propInfoList, int dispidToGet, int nameDispID, StructCache structCache)
        {
            IntPtr zero = IntPtr.Zero;
            int typeAttr = typeInfo.GetTypeAttr(ref zero);
            if (!System.Windows.Forms.NativeMethods.Succeeded(typeAttr) || (zero == IntPtr.Zero))
            {
                throw new ExternalException(System.Windows.Forms.SR.GetString("TYPEINFOPROCESSORGetTypeAttrFailed", new object[] { typeAttr }), typeAttr);
            }
            System.Windows.Forms.NativeMethods.tagTYPEATTR data = (System.Windows.Forms.NativeMethods.tagTYPEATTR) structCache.GetStruct(typeof(System.Windows.Forms.NativeMethods.tagTYPEATTR));
            UnsafeNativeMethods.PtrToStructure(zero, data);
            try
            {
                if (data != null)
                {
                    System.Windows.Forms.NativeMethods.tagVARDESC gvardesc = (System.Windows.Forms.NativeMethods.tagVARDESC) structCache.GetStruct(typeof(System.Windows.Forms.NativeMethods.tagVARDESC));
                    for (int i = 0; i < data.cVars; i++)
                    {
                        IntPtr pVarDesc = IntPtr.Zero;
                        if (System.Windows.Forms.NativeMethods.Succeeded(typeInfo.GetVarDesc(i, ref pVarDesc)) && (pVarDesc != IntPtr.Zero))
                        {
                            UnsafeNativeMethods.PtrToStructure(pVarDesc, gvardesc);
                            try
                            {
                                if ((gvardesc.varkind != 2) && ((dispidToGet == -1) || (gvardesc.memid == dispidToGet)))
                                {
                                    PropInfo info = ProcessDataCore(typeInfo, propInfoList, gvardesc.memid, nameDispID, gvardesc.elemdescVar.tdesc, gvardesc.wVarFlags, structCache);
                                    if (info.ReadOnly != 1)
                                    {
                                        info.ReadOnly = 2;
                                    }
                                }
                            }
                            finally
                            {
                                if (pVarDesc != IntPtr.Zero)
                                {
                                    typeInfo.ReleaseVarDesc(pVarDesc);
                                }
                            }
                        }
                    }
                    structCache.ReleaseStruct(gvardesc);
                }
            }
            finally
            {
                typeInfo.ReleaseTypeAttr(zero);
                structCache.ReleaseStruct(data);
            }
        }

        private static System.Type VTToType(System.Windows.Forms.NativeMethods.tagVT vt)
        {
            System.Windows.Forms.NativeMethods.tagVT gvt = vt;
            if (gvt <= System.Windows.Forms.NativeMethods.tagVT.VT_VECTOR)
            {
                switch (gvt)
                {
                    case System.Windows.Forms.NativeMethods.tagVT.VT_EMPTY:
                    case System.Windows.Forms.NativeMethods.tagVT.VT_NULL:
                        return null;

                    case System.Windows.Forms.NativeMethods.tagVT.VT_I2:
                        return typeof(short);

                    case System.Windows.Forms.NativeMethods.tagVT.VT_I4:
                    case System.Windows.Forms.NativeMethods.tagVT.VT_INT:
                        return typeof(int);

                    case System.Windows.Forms.NativeMethods.tagVT.VT_R4:
                        return typeof(float);

                    case System.Windows.Forms.NativeMethods.tagVT.VT_R8:
                        return typeof(double);

                    case System.Windows.Forms.NativeMethods.tagVT.VT_CY:
                        return typeof(decimal);

                    case System.Windows.Forms.NativeMethods.tagVT.VT_DATE:
                        return typeof(DateTime);

                    case System.Windows.Forms.NativeMethods.tagVT.VT_BSTR:
                    case System.Windows.Forms.NativeMethods.tagVT.VT_LPSTR:
                    case System.Windows.Forms.NativeMethods.tagVT.VT_LPWSTR:
                        return typeof(string);

                    case System.Windows.Forms.NativeMethods.tagVT.VT_DISPATCH:
                        return typeof(UnsafeNativeMethods.IDispatch);

                    case System.Windows.Forms.NativeMethods.tagVT.VT_ERROR:
                    case System.Windows.Forms.NativeMethods.tagVT.VT_HRESULT:
                        return typeof(int);

                    case System.Windows.Forms.NativeMethods.tagVT.VT_BOOL:
                        return typeof(bool);

                    case System.Windows.Forms.NativeMethods.tagVT.VT_VARIANT:
                        return typeof(Com2Variant);

                    case System.Windows.Forms.NativeMethods.tagVT.VT_UNKNOWN:
                        return typeof(object);

                    case System.Windows.Forms.NativeMethods.tagVT.VT_I1:
                        return typeof(sbyte);

                    case System.Windows.Forms.NativeMethods.tagVT.VT_UI1:
                        return typeof(byte);

                    case System.Windows.Forms.NativeMethods.tagVT.VT_UI2:
                        return typeof(ushort);

                    case System.Windows.Forms.NativeMethods.tagVT.VT_UI4:
                    case System.Windows.Forms.NativeMethods.tagVT.VT_UINT:
                        return typeof(uint);

                    case System.Windows.Forms.NativeMethods.tagVT.VT_I8:
                        return typeof(long);

                    case System.Windows.Forms.NativeMethods.tagVT.VT_UI8:
                        return typeof(ulong);

                    case System.Windows.Forms.NativeMethods.tagVT.VT_USERDEFINED:
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("COM2UnhandledVT", new object[] { "VT_USERDEFINED" }));

                    case System.Windows.Forms.NativeMethods.tagVT.VT_FILETIME:
                        return typeof(System.Windows.Forms.NativeMethods.FILETIME);

                    case System.Windows.Forms.NativeMethods.tagVT.VT_CLSID:
                        return typeof(Guid);
                }
            }
            else if (((gvt == System.Windows.Forms.NativeMethods.tagVT.VT_ARRAY) || (gvt == System.Windows.Forms.NativeMethods.tagVT.VT_BYREF)) || (gvt == System.Windows.Forms.NativeMethods.tagVT.VT_RESERVED))
            {
            }
            object[] args = new object[] { ((int) vt).ToString(CultureInfo.InvariantCulture) };
            throw new ArgumentException(System.Windows.Forms.SR.GetString("COM2UnhandledVT", args));
        }

        private static System.Reflection.Emit.ModuleBuilder ModuleBuilder
        {
            get
            {
                if (moduleBuilder == null)
                {
                    AppDomain domain = Thread.GetDomain();
                    AssemblyName name = new AssemblyName {
                        Name = "COM2InteropEmit"
                    };
                    moduleBuilder = domain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run).DefineDynamicModule("COM2Interop.Emit");
                }
                return moduleBuilder;
            }
        }

        internal class CachedProperties
        {
            private int defaultIndex;
            public readonly int MajorVersion;
            public readonly int MinorVersion;
            private PropertyDescriptor[] props;

            internal CachedProperties(PropertyDescriptor[] props, int defIndex, int majVersion, int minVersion)
            {
                this.props = this.ClonePropertyDescriptors(props);
                this.MajorVersion = majVersion;
                this.MinorVersion = minVersion;
                this.defaultIndex = defIndex;
            }

            private PropertyDescriptor[] ClonePropertyDescriptors(PropertyDescriptor[] props)
            {
                PropertyDescriptor[] descriptorArray = new PropertyDescriptor[props.Length];
                for (int i = 0; i < props.Length; i++)
                {
                    if (props[i] is ICloneable)
                    {
                        descriptorArray[i] = (PropertyDescriptor) ((ICloneable) props[i]).Clone();
                    }
                    else
                    {
                        descriptorArray[i] = props[i];
                    }
                }
                return descriptorArray;
            }

            public int DefaultIndex
            {
                get
                {
                    return this.defaultIndex;
                }
            }

            public PropertyDescriptor[] Properties
            {
                get
                {
                    return this.ClonePropertyDescriptors(this.props);
                }
            }
        }

        private class PropInfo
        {
            private readonly ArrayList attributes = new ArrayList();
            private int dispid = -1;
            private int index;
            private bool isDefault;
            private string name;
            private bool nonbrowsable;
            private int readOnly;
            public const int ReadOnlyFalse = 2;
            public const int ReadOnlyTrue = 1;
            public const int ReadOnlyUnknown = 0;
            private object typeData;
            private System.Type valueType;

            public override int GetHashCode()
            {
                if (this.name != null)
                {
                    return this.name.GetHashCode();
                }
                return base.GetHashCode();
            }

            public ArrayList Attributes
            {
                get
                {
                    return this.attributes;
                }
            }

            public int DispId
            {
                get
                {
                    return this.dispid;
                }
                set
                {
                    this.dispid = value;
                }
            }

            public int Index
            {
                get
                {
                    return this.index;
                }
                set
                {
                    this.index = value;
                }
            }

            public bool IsDefault
            {
                get
                {
                    return this.isDefault;
                }
                set
                {
                    this.isDefault = value;
                }
            }

            public string Name
            {
                get
                {
                    return this.name;
                }
                set
                {
                    this.name = value;
                }
            }

            public bool NonBrowsable
            {
                get
                {
                    return this.nonbrowsable;
                }
                set
                {
                    this.nonbrowsable = value;
                }
            }

            public int ReadOnly
            {
                get
                {
                    return this.readOnly;
                }
                set
                {
                    this.readOnly = value;
                }
            }

            public object TypeData
            {
                get
                {
                    return this.typeData;
                }
                set
                {
                    this.typeData = value;
                }
            }

            public System.Type ValueType
            {
                get
                {
                    return this.valueType;
                }
                set
                {
                    this.valueType = value;
                }
            }
        }

        public class StructCache
        {
            private Hashtable queuedTypes = new Hashtable();

            private Queue GetQueue(System.Type t, bool create)
            {
                object obj2 = this.queuedTypes[t];
                if ((obj2 == null) && create)
                {
                    obj2 = new Queue();
                    this.queuedTypes[t] = obj2;
                }
                return (Queue) obj2;
            }

            public object GetStruct(System.Type t)
            {
                Queue queue = this.GetQueue(t, true);
                if (queue.Count == 0)
                {
                    return Activator.CreateInstance(t);
                }
                return queue.Dequeue();
            }

            public void ReleaseStruct(object str)
            {
                System.Type t = str.GetType();
                Queue queue = this.GetQueue(t, false);
                if (queue != null)
                {
                    queue.Enqueue(str);
                }
            }
        }
    }
}

