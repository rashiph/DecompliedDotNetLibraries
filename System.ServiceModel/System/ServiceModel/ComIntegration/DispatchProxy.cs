namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    internal class DispatchProxy : IPseudoDispatch, IDisposable
    {
        private IProvideChannelBuilderSettings channelBuilderSettings;
        private ContractDescription contract;
        private Dictionary<uint, string> dispToName = new Dictionary<uint, string>();
        private Dictionary<uint, MethodInfo> dispToOperationDescription = new Dictionary<uint, MethodInfo>();
        private Dictionary<string, uint> nameToDisp = new Dictionary<string, uint>();

        private DispatchProxy(ContractDescription contract, IProvideChannelBuilderSettings channelBuilderSettings)
        {
            if (channelBuilderSettings == null)
            {
                throw Fx.AssertAndThrow("channelBuilderSettings cannot be null cannot be null");
            }
            if (contract == null)
            {
                throw Fx.AssertAndThrow("contract cannot be null");
            }
            this.channelBuilderSettings = channelBuilderSettings;
            this.contract = contract;
            this.ProcessContractDescription();
            ComPlusDispatchMethodTrace.Trace(TraceEventType.Verbose, 0x50020, "TraceCodeComIntegrationDispatchMethod", this.dispToOperationDescription);
        }

        internal static ComProxy Create(IntPtr outer, ContractDescription contract, IProvideChannelBuilderSettings channelBuilderSettings)
        {
            DispatchProxy proxy = null;
            ComProxy proxy3;
            IntPtr zero = IntPtr.Zero;
            ComProxy proxy2 = null;
            try
            {
                proxy = new DispatchProxy(contract, channelBuilderSettings);
                zero = OuterProxyWrapper.CreateDispatchProxy(outer, proxy);
                proxy2 = new ComProxy(zero, proxy);
                proxy3 = proxy2;
            }
            finally
            {
                if (proxy2 == null)
                {
                    if (proxy != null)
                    {
                        ((IDisposable) proxy).Dispose();
                    }
                    if (zero != IntPtr.Zero)
                    {
                        Marshal.Release(zero);
                    }
                }
            }
            return proxy3;
        }

        private object FetchVariant(IntPtr baseArray, int index, Type type)
        {
            if (baseArray == IntPtr.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("baseArrray");
            }
            uint disp = (uint) (index * Marshal.SizeOf(typeof(TagVariant)));
            object objectForNativeVariant = Marshal.GetObjectForNativeVariant(this.GetDisp(baseArray, disp));
            if (type == typeof(int))
            {
                if (objectForNativeVariant.GetType() == typeof(short))
                {
                    return (int) ((short) objectForNativeVariant);
                }
                if (objectForNativeVariant.GetType() != typeof(int))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(System.ServiceModel.SR.GetString("UnsupportedConversion", new object[] { objectForNativeVariant.GetType(), type.GetElementType() }), HR.DISP_E_TYPEMISMATCH));
                }
                return objectForNativeVariant;
            }
            if (type == typeof(long))
            {
                if (objectForNativeVariant.GetType() == typeof(short))
                {
                    return (long) ((short) objectForNativeVariant);
                }
                if (objectForNativeVariant.GetType() == typeof(int))
                {
                    return (long) ((int) objectForNativeVariant);
                }
                if (objectForNativeVariant.GetType() != typeof(long))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(System.ServiceModel.SR.GetString("UnsupportedConversion", new object[] { objectForNativeVariant.GetType(), type }), HR.DISP_E_TYPEMISMATCH));
                }
            }
            return objectForNativeVariant;
        }

        private object FetchVariants(IntPtr baseArray, int index, Type type)
        {
            object obj2;
            if (baseArray == IntPtr.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("baseArrray");
            }
            uint disp = (uint) (index * Marshal.SizeOf(typeof(TagVariant)));
            TagVariant variant = (TagVariant) Marshal.PtrToStructure(this.GetDisp(baseArray, disp), typeof(TagVariant));
            if ((variant.vt & 0x400c) == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(System.ServiceModel.SR.GetString("OnlyVariantAllowedByRef"), HR.DISP_E_TYPEMISMATCH));
            }
            TagVariant variant2 = (TagVariant) Marshal.PtrToStructure(variant.ptr, typeof(TagVariant));
            if ((variant2.vt & 0x600c) == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(System.ServiceModel.SR.GetString("OnlyByRefVariantSafeArraysAllowed"), HR.DISP_E_TYPEMISMATCH));
            }
            IntPtr pSafeArray = (IntPtr) Marshal.PtrToStructure(variant2.ptr, typeof(IntPtr));
            if (SafeNativeMethods.SafeArrayGetDim(pSafeArray) != 1)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(System.ServiceModel.SR.GetString("OnlyOneDimensionalSafeArraysAllowed"), HR.DISP_E_TYPEMISMATCH));
            }
            if (SafeNativeMethods.SafeArrayGetElemsize(pSafeArray) != Marshal.SizeOf(typeof(TagVariant)))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(System.ServiceModel.SR.GetString("OnlyVariantTypeElementsAllowed"), HR.DISP_E_TYPEMISMATCH));
            }
            if (SafeNativeMethods.SafeArrayGetLBound(pSafeArray, 1) > 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(System.ServiceModel.SR.GetString("OnlyZeroLBoundAllowed"), HR.DISP_E_TYPEMISMATCH));
            }
            int num5 = SafeNativeMethods.SafeArrayGetUBound(pSafeArray, 1);
            IntPtr aSrcNativeVariant = SafeNativeMethods.SafeArrayAccessData(pSafeArray);
            try
            {
                object[] objectsForNativeVariants = Marshal.GetObjectsForNativeVariants(aSrcNativeVariant, num5 + 1);
                Array array = Array.CreateInstance(type.GetElementType(), objectsForNativeVariants.Length);
                if (objectsForNativeVariants.Length == 0)
                {
                    return array;
                }
                if ((type.GetElementType() != typeof(int)) && (type.GetElementType() != typeof(long)))
                {
                    try
                    {
                        objectsForNativeVariants.CopyTo(array, 0);
                        goto Label_0410;
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(System.ServiceModel.SR.GetString("UnsupportedConversion", new object[] { objectsForNativeVariants[0].GetType(), type.GetElementType() }), HR.DISP_E_TYPEMISMATCH));
                    }
                }
                if (type.GetElementType() == typeof(int))
                {
                    for (int i = 0; i < objectsForNativeVariants.Length; i++)
                    {
                        if (objectsForNativeVariants[i].GetType() == typeof(short))
                        {
                            array.SetValue((int) ((short) objectsForNativeVariants[i]), i);
                        }
                        else
                        {
                            if (objectsForNativeVariants[i].GetType() != typeof(int))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(System.ServiceModel.SR.GetString("UnsupportedConversion", new object[] { objectsForNativeVariants[i].GetType(), type.GetElementType() }), HR.DISP_E_TYPEMISMATCH));
                            }
                            array.SetValue(objectsForNativeVariants[i], i);
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < objectsForNativeVariants.Length; j++)
                    {
                        if (objectsForNativeVariants[j].GetType() == typeof(short))
                        {
                            array.SetValue((long) ((short) objectsForNativeVariants[j]), j);
                        }
                        else if (objectsForNativeVariants[j].GetType() == typeof(int))
                        {
                            array.SetValue((long) ((int) objectsForNativeVariants[j]), j);
                        }
                        else
                        {
                            if (objectsForNativeVariants[j].GetType() != typeof(long))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(System.ServiceModel.SR.GetString("UnsupportedConversion", new object[] { objectsForNativeVariants[j].GetType(), type.GetElementType() }), HR.DISP_E_TYPEMISMATCH));
                            }
                            array.SetValue(objectsForNativeVariants[j], j);
                        }
                    }
                }
            Label_0410:
                obj2 = array;
            }
            finally
            {
                SafeNativeMethods.SafeArrayUnaccessData(pSafeArray);
            }
            return obj2;
        }

        private IntPtr GetDisp(IntPtr baseAddress, uint disp)
        {
            long num = (long) baseAddress;
            num += disp;
            return (IntPtr) num;
        }

        private bool IsByRef(IntPtr baseArray, int index)
        {
            if (baseArray == IntPtr.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("baseArrray");
            }
            uint disp = (uint) (index * Marshal.SizeOf(typeof(TagVariant)));
            ushort num2 = (ushort) Marshal.ReadInt16(this.GetDisp(baseArray, disp));
            return ((num2 & 0x4000) != 0);
        }

        private void PopulateByRef(IntPtr baseArray, int index, object val)
        {
            if (val != null)
            {
                if (baseArray == IntPtr.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("baseArrray");
                }
                uint disp = (uint) (index * Marshal.SizeOf(typeof(TagVariant)));
                TagVariant variant = (TagVariant) Marshal.PtrToStructure(this.GetDisp(baseArray, disp), typeof(TagVariant));
                if ((variant.vt & 12) == 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(System.ServiceModel.SR.GetString("OnlyVariantAllowedByRef"), HR.DISP_E_TYPEMISMATCH));
                }
                if (!val.GetType().IsArray)
                {
                    Marshal.GetNativeVariantForObject(val, variant.ptr);
                }
                else
                {
                    Array array = val as Array;
                    Array array2 = Array.CreateInstance(typeof(object), array.Length);
                    array.CopyTo(array2, 0);
                    Marshal.GetNativeVariantForObject(array2, variant.ptr);
                }
            }
        }

        private void ProcessContractDescription()
        {
            uint num = 10;
            Dictionary<string, ParamInfo> dictionary = null;
            foreach (OperationDescription description in this.contract.Operations)
            {
                this.dispToName[num] = description.Name;
                this.nameToDisp[description.Name] = num;
                MethodInfo info = null;
                info = new MethodInfo(description);
                this.dispToOperationDescription[num++] = info;
                dictionary = new Dictionary<string, ParamInfo>();
                bool flag = true;
                flag = true;
                int num2 = 0;
                foreach (MessageDescription description2 in description.Messages)
                {
                    num2 = 0;
                    if (description2.Body.ReturnValue != null)
                    {
                        if (string.IsNullOrEmpty(description2.Body.ReturnValue.BaseType))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(System.ServiceModel.SR.GetString("CannotResolveTypeForParamInMessageDescription", new object[] { "ReturnValue", description2.Body.WrapperName, description2.Body.WrapperNamespace }), HR.DISP_E_MEMBERNOTFOUND));
                        }
                        description2.Body.ReturnValue.Type = Type.GetType(description2.Body.ReturnValue.BaseType);
                    }
                    foreach (MessagePartDescription description3 in description2.Body.Parts)
                    {
                        uint num3 = 0;
                        ParamInfo info2 = null;
                        info2 = null;
                        if (!this.nameToDisp.TryGetValue(description3.Name, out num3))
                        {
                            this.dispToName[num] = description3.Name;
                            this.nameToDisp[description3.Name] = num;
                            num3 = num;
                            num++;
                        }
                        if (!dictionary.TryGetValue(description3.Name, out info2))
                        {
                            info2 = new ParamInfo();
                            info.paramList.Add(info2);
                            info.dispIdToParamInfo[num3] = info2;
                            if (string.IsNullOrEmpty(description3.BaseType))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(System.ServiceModel.SR.GetString("CannotResolveTypeForParamInMessageDescription", new object[] { description3.Name, description2.Body.WrapperName, description2.Body.WrapperNamespace }), HR.DISP_E_MEMBERNOTFOUND));
                            }
                            info2.type = Type.GetType(description3.BaseType, true);
                            info2.name = description3.Name;
                            dictionary[description3.Name] = info2;
                            description3.Index = num2;
                        }
                        description3.Type = info2.type;
                        if (flag)
                        {
                            info2.inIndex = num2;
                        }
                        else
                        {
                            info2.outIndex = num2;
                        }
                        num2++;
                    }
                    flag = false;
                }
            }
        }

        private object SendMessage(OperationDescription opDesc, string action, object[] ins, object[] outs)
        {
            ProxyOperationRuntime operationByName = this.channelBuilderSettings.ServiceChannel.ClientRuntime.GetRuntime().GetOperationByName(opDesc.Name);
            if (operationByName == null)
            {
                throw Fx.AssertAndThrow("Operation runtime should not be null");
            }
            return this.channelBuilderSettings.ServiceChannel.Call(action, opDesc.IsOneWay, operationByName, ins, outs);
        }

        void IDisposable.Dispose()
        {
            this.dispToName.Clear();
            this.nameToDisp.Clear();
            this.dispToOperationDescription.Clear();
        }

        void IPseudoDispatch.GetIDsOfNames(uint cNames, [MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPWStr, SizeParamIndex=0)] string[] rgszNames, IntPtr pDispID)
        {
            for (int i = 0; i < cNames; i++)
            {
                uint num2;
                if (!this.nameToDisp.TryGetValue(rgszNames[i], out num2))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(System.ServiceModel.SR.GetString("OperationNotFound", new object[] { rgszNames[i] }), HR.DISP_E_UNKNOWNNAME));
                }
                Marshal.WriteInt32(pDispID, i * 4, (int) num2);
            }
        }

        int IPseudoDispatch.Invoke(uint dispIdMember, uint cArgs, uint cNamedArgs, IntPtr rgvarg, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] uint[] rgdispidNamedArgs, IntPtr pVarResult, IntPtr pExcepInfo, out uint pArgErr)
        {
            pArgErr = 0;
            try
            {
                if (cNamedArgs > 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(System.ServiceModel.SR.GetString("NamedArgsNotSupported"), HR.DISP_E_BADPARAMCOUNT));
                }
                MethodInfo info = null;
                if (!this.dispToOperationDescription.TryGetValue(dispIdMember, out info))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(System.ServiceModel.SR.GetString("BadDispID", new object[] { dispIdMember }), HR.DISP_E_MEMBERNOTFOUND));
                }
                object[] ins = null;
                object[] outs = null;
                string action = null;
                if (info.paramList.Count != cArgs)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(System.ServiceModel.SR.GetString("BadDispID", new object[] { dispIdMember }), HR.DISP_E_BADPARAMCOUNT));
                }
                ins = new object[info.opDesc.Messages[0].Body.Parts.Count];
                outs = new object[info.opDesc.Messages[1].Body.Parts.Count];
                if (cArgs > 0)
                {
                    if (info.opDesc.Messages[0].Body.Parts.Count > 0)
                    {
                        for (int j = 0; j < info.opDesc.Messages[0].Body.Parts.Count; j++)
                        {
                            ins[j] = null;
                        }
                    }
                    if (!info.opDesc.IsOneWay && (info.opDesc.Messages[1].Body.Parts.Count > 0))
                    {
                        for (int k = 0; k < info.opDesc.Messages[1].Body.Parts.Count; k++)
                        {
                            outs[k] = null;
                        }
                    }
                }
                action = info.opDesc.Messages[0].Action;
                int num3 = 0;
                for (int i = 0; i < cArgs; i++)
                {
                    if (info.paramList[i].inIndex != -1)
                    {
                        try
                        {
                            object obj2 = null;
                            if (!info.paramList[i].type.IsArray)
                            {
                                obj2 = this.FetchVariant(rgvarg, (int) ((cArgs - i) - ((ulong) 1L)), info.paramList[i].type);
                            }
                            else
                            {
                                obj2 = this.FetchVariants(rgvarg, (int) ((cArgs - i) - ((ulong) 1L)), info.paramList[i].type);
                            }
                            ins[info.paramList[i].inIndex] = obj2;
                            num3++;
                        }
                        catch (ArgumentNullException)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(System.ServiceModel.SR.GetString("VariantArrayNull", new object[] { (cArgs - i) - 1L }));
                        }
                    }
                }
                if (num3 != ins.Length)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(System.ServiceModel.SR.GetString("BadParamCount"), HR.DISP_E_BADPARAMCOUNT));
                }
                object obj3 = null;
                try
                {
                    obj3 = this.SendMessage(info.opDesc, action, ins, outs);
                }
                catch (Exception baseException)
                {
                    if (Fx.IsFatal(baseException))
                    {
                        throw;
                    }
                    if (pExcepInfo != IntPtr.Zero)
                    {
                        System.Runtime.InteropServices.ComTypes.EXCEPINFO structure = new System.Runtime.InteropServices.ComTypes.EXCEPINFO();
                        baseException = baseException.GetBaseException();
                        structure.bstrDescription = baseException.Message;
                        structure.bstrSource = baseException.Source;
                        structure.scode = Marshal.GetHRForException(baseException);
                        Marshal.StructureToPtr(structure, pExcepInfo, false);
                    }
                    return HR.DISP_E_EXCEPTION;
                }
                if (!info.opDesc.IsOneWay)
                {
                    if (outs != null)
                    {
                        bool[] flagArray = new bool[outs.Length];
                        for (uint m = 0; m < flagArray.Length; m++)
                        {
                            flagArray[m] = false;
                        }
                        for (int n = 0; n < cArgs; n++)
                        {
                            if (info.paramList[n].outIndex != -1)
                            {
                                try
                                {
                                    if (this.IsByRef(rgvarg, (int) ((cArgs - n) - ((ulong) 1L))))
                                    {
                                        this.PopulateByRef(rgvarg, (int) ((cArgs - n) - ((ulong) 1L)), outs[info.paramList[n].outIndex]);
                                    }
                                }
                                catch (ArgumentNullException)
                                {
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(System.ServiceModel.SR.GetString("VariantArrayNull", new object[] { (cArgs - n) - 1L }));
                                }
                                flagArray[info.paramList[n].outIndex] = true;
                            }
                        }
                    }
                    if ((obj3 != null) && (pVarResult != IntPtr.Zero))
                    {
                        if (!obj3.GetType().IsArray)
                        {
                            Marshal.GetNativeVariantForObject(obj3, pVarResult);
                        }
                        else
                        {
                            Array array = obj3 as Array;
                            Array array2 = Array.CreateInstance(typeof(object), array.Length);
                            array.CopyTo(array2, 0);
                            Marshal.GetNativeVariantForObject(array2, pVarResult);
                        }
                    }
                }
                return HR.S_OK;
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
                return Marshal.GetHRForException(exception2.GetBaseException());
            }
        }

        internal class MethodInfo
        {
            public Dictionary<uint, DispatchProxy.ParamInfo> dispIdToParamInfo;
            public OperationDescription opDesc;
            public List<DispatchProxy.ParamInfo> paramList;
            public DispatchProxy.ParamInfo ReturnVal;

            public MethodInfo(OperationDescription opDesc)
            {
                this.opDesc = opDesc;
                this.paramList = new List<DispatchProxy.ParamInfo>();
                this.dispIdToParamInfo = new Dictionary<uint, DispatchProxy.ParamInfo>();
            }
        }

        [Serializable]
        internal class ParamInfo
        {
            public int inIndex = -1;
            public string name;
            public int outIndex = -1;
            public Type type;
        }
    }
}

