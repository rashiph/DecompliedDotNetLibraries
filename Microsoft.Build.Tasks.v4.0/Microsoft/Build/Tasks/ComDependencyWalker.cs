namespace Microsoft.Build.Tasks
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;

    internal class ComDependencyWalker
    {
        private Dictionary<string, object> analyzedTypes = new Dictionary<string, object>();
        private Hashtable dependencies = new Hashtable();
        private List<Exception> encounteredProblems = new List<Exception>();
        private MarshalReleaseComObject marshalReleaseComObject;

        internal ComDependencyWalker(MarshalReleaseComObject marshalReleaseComObject)
        {
            this.marshalReleaseComObject = marshalReleaseComObject;
        }

        private void AnalyzeElement(ITypeInfo typeInfo, System.Runtime.InteropServices.ComTypes.ELEMDESC elementDesc)
        {
            System.Runtime.InteropServices.ComTypes.TYPEDESC tdesc = elementDesc.tdesc;
            while ((tdesc.vt == 0x1a) || (tdesc.vt == 0x1b))
            {
                tdesc = (System.Runtime.InteropServices.ComTypes.TYPEDESC) Marshal.PtrToStructure(tdesc.lpValue, typeof(System.Runtime.InteropServices.ComTypes.TYPEDESC));
            }
            if (tdesc.vt == 0x1d)
            {
                IntPtr lpValue = tdesc.lpValue;
                IFixedTypeInfo ppTI = null;
                try
                {
                    ((IFixedTypeInfo) typeInfo).GetRefTypeInfo(lpValue, out ppTI);
                    this.AnalyzeTypeInfo((ITypeInfo) ppTI);
                }
                finally
                {
                    if (ppTI != null)
                    {
                        this.marshalReleaseComObject(ppTI);
                    }
                }
            }
        }

        private void AnalyzeTypeInfo(ITypeInfo typeInfo)
        {
            ITypeLib ppTLB = null;
            try
            {
                int num;
                System.Runtime.InteropServices.ComTypes.TYPELIBATTR typelibattr;
                System.Runtime.InteropServices.ComTypes.TYPEATTR typeattr;
                typeInfo.GetContainingTypeLib(out ppTLB, out num);
                ComReference.GetTypeLibAttrForTypeLib(ref ppTLB, out typelibattr);
                string key = string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}.{3}:{4}", new object[] { typelibattr.guid, typelibattr.wMajorVerNum, typelibattr.wMinorVerNum, typelibattr.lcid, num });
                ComReference.GetTypeAttrForTypeInfo(typeInfo, out typeattr);
                if (!this.CanSkipType(typeInfo, ppTLB, typeattr, typelibattr))
                {
                    this.dependencies[typelibattr] = null;
                    if (!this.analyzedTypes.ContainsKey(key))
                    {
                        this.analyzedTypes.Add(key, null);
                        this.ScanImplementedTypes(typeInfo, typeattr);
                        this.ScanDefinedVariables(typeInfo, typeattr);
                        this.ScanDefinedFunctions(typeInfo, typeattr);
                    }
                }
                else if (!this.analyzedTypes.ContainsKey(key))
                {
                    this.analyzedTypes.Add(key, null);
                }
            }
            finally
            {
                if (ppTLB != null)
                {
                    this.marshalReleaseComObject(ppTLB);
                }
            }
        }

        internal void AnalyzeTypeLibrary(ITypeLib typeLibrary)
        {
            try
            {
                int typeInfoCount = typeLibrary.GetTypeInfoCount();
                for (int i = 0; i < typeInfoCount; i++)
                {
                    ITypeInfo ppTI = null;
                    try
                    {
                        typeLibrary.GetTypeInfo(i, out ppTI);
                        this.AnalyzeTypeInfo(ppTI);
                    }
                    finally
                    {
                        if (ppTI != null)
                        {
                            this.marshalReleaseComObject(ppTI);
                        }
                    }
                }
            }
            catch (COMException exception)
            {
                this.encounteredProblems.Add(exception);
            }
        }

        private bool CanSkipType(ITypeInfo typeInfo, ITypeLib typeLib, System.Runtime.InteropServices.ComTypes.TYPEATTR typeAttributes, System.Runtime.InteropServices.ComTypes.TYPELIBATTR typeLibAttributes)
        {
            if (((typeAttributes.guid == Microsoft.Build.Tasks.NativeMethods.IID_IUnknown) || (typeAttributes.guid == Microsoft.Build.Tasks.NativeMethods.IID_IDispatch)) || (((typeAttributes.guid == Microsoft.Build.Tasks.NativeMethods.IID_IDispatchEx) || (typeAttributes.guid == Microsoft.Build.Tasks.NativeMethods.IID_IEnumVariant)) || (typeAttributes.guid == Microsoft.Build.Tasks.NativeMethods.IID_ITypeInfo)))
            {
                return true;
            }
            if (typeLibAttributes.guid == Microsoft.Build.Tasks.NativeMethods.IID_StdOle)
            {
                string str;
                string str2;
                string str3;
                int num;
                typeInfo.GetDocumentation(-1, out str, out str2, out num, out str3);
                if (string.CompareOrdinal(str, "GUID") == 0)
                {
                    return true;
                }
            }
            ITypeLib2 lib = typeLib as ITypeLib2;
            if (lib != null)
            {
                object obj2;
                lib.GetCustData(ref Microsoft.Build.Tasks.NativeMethods.GUID_ExportedFromComPlus, out obj2);
                string str4 = obj2 as string;
                if (!string.IsNullOrEmpty(str4))
                {
                    return true;
                }
            }
            return false;
        }

        internal void ClearAnalyzedTypeCache()
        {
            this.analyzedTypes.Clear();
        }

        internal void ClearDependencyList()
        {
            this.dependencies.Clear();
        }

        internal ICollection<string> GetAnalyzedTypeNames()
        {
            return this.analyzedTypes.Keys;
        }

        internal System.Runtime.InteropServices.ComTypes.TYPELIBATTR[] GetDependencies()
        {
            System.Runtime.InteropServices.ComTypes.TYPELIBATTR[] array = new System.Runtime.InteropServices.ComTypes.TYPELIBATTR[this.dependencies.Keys.Count];
            this.dependencies.Keys.CopyTo(array, 0);
            return array;
        }

        private void ScanDefinedFunctions(ITypeInfo typeInfo, System.Runtime.InteropServices.ComTypes.TYPEATTR typeAttributes)
        {
            for (int i = 0; i < typeAttributes.cFuncs; i++)
            {
                IntPtr zero = IntPtr.Zero;
                try
                {
                    System.Runtime.InteropServices.ComTypes.FUNCDESC funcdesc;
                    ComReference.GetFuncDescForDescIndex(typeInfo, i, out funcdesc, out zero);
                    int num2 = 0;
                    for (int j = 0; j < funcdesc.cParams; j++)
                    {
                        System.Runtime.InteropServices.ComTypes.ELEMDESC elementDesc = (System.Runtime.InteropServices.ComTypes.ELEMDESC) Marshal.PtrToStructure(new IntPtr(funcdesc.lprgelemdescParam.ToInt64() + num2), typeof(System.Runtime.InteropServices.ComTypes.ELEMDESC));
                        this.AnalyzeElement(typeInfo, elementDesc);
                        num2 += Marshal.SizeOf(typeof(System.Runtime.InteropServices.ComTypes.ELEMDESC));
                    }
                    this.AnalyzeElement(typeInfo, funcdesc.elemdescFunc);
                }
                finally
                {
                    if (zero != IntPtr.Zero)
                    {
                        typeInfo.ReleaseFuncDesc(zero);
                    }
                }
            }
        }

        private void ScanDefinedVariables(ITypeInfo typeInfo, System.Runtime.InteropServices.ComTypes.TYPEATTR typeAttributes)
        {
            for (int i = 0; i < typeAttributes.cVars; i++)
            {
                IntPtr zero = IntPtr.Zero;
                try
                {
                    System.Runtime.InteropServices.ComTypes.VARDESC vardesc;
                    ComReference.GetVarDescForVarIndex(typeInfo, i, out vardesc, out zero);
                    this.AnalyzeElement(typeInfo, vardesc.elemdescVar);
                }
                finally
                {
                    if (zero != IntPtr.Zero)
                    {
                        typeInfo.ReleaseVarDesc(zero);
                    }
                }
            }
        }

        private void ScanImplementedTypes(ITypeInfo typeInfo, System.Runtime.InteropServices.ComTypes.TYPEATTR typeAttributes)
        {
            for (int i = 0; i < typeAttributes.cImplTypes; i++)
            {
                IFixedTypeInfo ppTI = null;
                try
                {
                    IntPtr ptr;
                    IFixedTypeInfo info2 = (IFixedTypeInfo) typeInfo;
                    info2.GetRefTypeOfImplType(i, out ptr);
                    info2.GetRefTypeInfo(ptr, out ppTI);
                    this.AnalyzeTypeInfo((ITypeInfo) ppTI);
                }
                finally
                {
                    if (ppTI != null)
                    {
                        this.marshalReleaseComObject(ppTI);
                    }
                }
            }
        }

        internal List<Exception> EncounteredProblems
        {
            get
            {
                return this.encounteredProblems;
            }
        }
    }
}

