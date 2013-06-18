namespace System.Management.Instrumentation
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Management;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using System.Threading;

    internal sealed class EventSource : IWbemProviderInit, IWbemEventProvider, IWbemEventProviderQuerySink, IWbemEventProviderSecurity, IWbemServices_Old
    {
        private bool alive = true;
        private object critSec = new object();
        private AutoResetEvent doIndicate = new AutoResetEvent(false);
        private static ArrayList eventSources = new ArrayList();
        private InstrumentedAssembly instrumentedAssembly;
        private Hashtable mapQueryIdToQuery = new Hashtable();
        private IWbemServices pNamespaceMTA;
        private IWbemServices pNamespaceNA;
        private static ReaderWriterLock preventShutdownLock = new ReaderWriterLock();
        private IWbemObjectSink pSinkMTA;
        private IWbemObjectSink pSinkNA;
        private IWbemDecoupledRegistrar registrar = ((IWbemDecoupledRegistrar) new WbemDecoupledRegistrar());
        private ArrayList reqList = new ArrayList(3);
        private static int shutdownInProgress = 0;
        private bool workerThreadInitialized;

        static EventSource()
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(EventSource.ProcessExit);
            AppDomain.CurrentDomain.DomainUnload += new EventHandler(EventSource.ProcessExit);
        }

        public EventSource(string namespaceName, string appName, InstrumentedAssembly instrumentedAssembly)
        {
            lock (eventSources)
            {
                if (shutdownInProgress == 0)
                {
                    this.instrumentedAssembly = instrumentedAssembly;
                    int errorCode = this.registrar.Register_(0, null, null, null, namespaceName, appName, this);
                    if (errorCode != 0)
                    {
                        Marshal.ThrowExceptionForHR(errorCode);
                    }
                    eventSources.Add(this);
                }
            }
        }

        public bool Any()
        {
            if (this.pSinkMTA != null)
            {
                return (this.mapQueryIdToQuery.Count == 0);
            }
            return true;
        }

        ~EventSource()
        {
            this.UnRegister();
        }

        public void IndicateEvents(int length, IntPtr[] objects)
        {
            if (this.pSinkMTA != null)
            {
                if (MTAHelper.IsNoContextMTA())
                {
                    int errorCode = this.pSinkMTA.Indicate_(length, objects);
                    if (errorCode < 0)
                    {
                        if ((errorCode & 0xfffff000L) == 0x80041000L)
                        {
                            ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                        }
                        else
                        {
                            Marshal.ThrowExceptionForHR(errorCode);
                        }
                    }
                }
                else
                {
                    MTARequest request = new MTARequest(length, objects);
                    lock (this.critSec)
                    {
                        if (!this.workerThreadInitialized)
                        {
                            Thread thread = new Thread(new ThreadStart(this.MTAWorkerThread2)) {
                                IsBackground = true
                            };
                            thread.SetApartmentState(ApartmentState.MTA);
                            thread.Start();
                            this.workerThreadInitialized = true;
                        }
                        int index = this.reqList.Add(request);
                        if (!this.doIndicate.Set())
                        {
                            this.reqList.RemoveAt(index);
                            throw new ManagementException(RC.GetString("WORKER_THREAD_WAKEUP_FAILED"));
                        }
                    }
                    request.doneIndicate.WaitOne();
                    if (request.exception != null)
                    {
                        throw request.exception;
                    }
                }
                GC.KeepAlive(this);
            }
        }

        public void MTAWorkerThread2()
        {
            MTARequest request;
        Label_0000:
            this.doIndicate.WaitOne();
            if (!this.alive)
            {
                return;
            }
        Label_0015:
            request = null;
            lock (this.critSec)
            {
                if (this.reqList.Count > 0)
                {
                    request = (MTARequest) this.reqList[0];
                    this.reqList.RemoveAt(0);
                }
                else
                {
                    goto Label_0000;
                }
            }
            try
            {
                try
                {
                    if (this.pSinkMTA != null)
                    {
                        int errorCode = this.pSinkMTA.Indicate_(request.lengthFromSTA, request.objectsFromSTA);
                        if (errorCode < 0)
                        {
                            if ((errorCode & 0xfffff000L) == 0x80041000L)
                            {
                                ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                            }
                            else
                            {
                                Marshal.ThrowExceptionForHR(errorCode);
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    request.exception = exception;
                }
                goto Label_0015;
            }
            finally
            {
                request.doneIndicate.Set();
                GC.KeepAlive(this);
            }
        }

        private static void ProcessExit(object o, EventArgs args)
        {
            if (shutdownInProgress == 0)
            {
                Interlocked.Increment(ref shutdownInProgress);
                try
                {
                    preventShutdownLock.AcquireWriterLock(-1);
                    lock (eventSources)
                    {
                        foreach (EventSource source in eventSources)
                        {
                            source.UnRegister();
                        }
                    }
                }
                finally
                {
                    preventShutdownLock.ReleaseWriterLock();
                    Thread.Sleep(50);
                    preventShutdownLock.AcquireWriterLock(-1);
                    preventShutdownLock.ReleaseWriterLock();
                }
            }
        }

        private void RelocateNamespaceRCWToMTA()
        {
            new ThreadDispatch(new ThreadDispatch.ThreadWorkerMethodWithParam(this.RelocateNamespaceRCWToMTA_ThreadFuncion)) { Parameter = this }.Start();
        }

        private void RelocateNamespaceRCWToMTA_ThreadFuncion(object param)
        {
            EventSource source = (EventSource) param;
            source.pNamespaceMTA = (IWbemServices) RelocateRCWToCurrentApartment(source.pNamespaceNA);
            source.pNamespaceNA = null;
        }

        private static object RelocateRCWToCurrentApartment(object comObject)
        {
            if (comObject == null)
            {
                return null;
            }
            IntPtr iUnknownForObject = Marshal.GetIUnknownForObject(comObject);
            if (Marshal.ReleaseComObject(comObject) != 0)
            {
                throw new Exception();
            }
            comObject = Marshal.GetObjectForIUnknown(iUnknownForObject);
            Marshal.Release(iUnknownForObject);
            return comObject;
        }

        private void RelocateSinkRCWToMTA()
        {
            new ThreadDispatch(new ThreadDispatch.ThreadWorkerMethodWithParam(this.RelocateSinkRCWToMTA_ThreadFuncion)) { Parameter = this }.Start();
        }

        private void RelocateSinkRCWToMTA_ThreadFuncion(object param)
        {
            EventSource source = (EventSource) param;
            source.pSinkMTA = (IWbemObjectSink) RelocateRCWToCurrentApartment(source.pSinkNA);
            source.pSinkNA = null;
        }

        int IWbemEventProvider.ProvideEvents_([In, MarshalAs(UnmanagedType.Interface)] IWbemObjectSink pSink, [In] int lFlags)
        {
            this.pSinkNA = pSink;
            this.RelocateSinkRCWToMTA();
            return 0;
        }

        int IWbemEventProviderQuerySink.CancelQuery_([In] uint dwId)
        {
            lock (this.mapQueryIdToQuery)
            {
                this.mapQueryIdToQuery.Remove(dwId);
            }
            return 0;
        }

        int IWbemEventProviderQuerySink.NewQuery_([In] uint dwId, [In, MarshalAs(UnmanagedType.LPWStr)] string wszQueryLanguage, [In, MarshalAs(UnmanagedType.LPWStr)] string wszQuery)
        {
            lock (this.mapQueryIdToQuery)
            {
                if (this.mapQueryIdToQuery.ContainsKey(dwId))
                {
                    this.mapQueryIdToQuery.Remove(dwId);
                }
                this.mapQueryIdToQuery.Add(dwId, wszQuery);
            }
            return 0;
        }

        int IWbemEventProviderSecurity.AccessCheck_([In, MarshalAs(UnmanagedType.LPWStr)] string wszQueryLanguage, [In, MarshalAs(UnmanagedType.LPWStr)] string wszQuery, [In] int lSidLength, [In] ref byte pSid)
        {
            return 0;
        }

        int IWbemProviderInit.Initialize_([In, MarshalAs(UnmanagedType.LPWStr)] string wszUser, [In] int lFlags, [In, MarshalAs(UnmanagedType.LPWStr)] string wszNamespace, [In, MarshalAs(UnmanagedType.LPWStr)] string wszLocale, [In, MarshalAs(UnmanagedType.Interface)] IWbemServices pNamespace, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [In, MarshalAs(UnmanagedType.Interface)] IWbemProviderInitSink pInitSink)
        {
            this.pNamespaceNA = pNamespace;
            this.RelocateNamespaceRCWToMTA();
            this.pSinkNA = null;
            this.pSinkMTA = null;
            lock (this.mapQueryIdToQuery)
            {
                this.mapQueryIdToQuery.Clear();
            }
            pInitSink.SetStatus_(0, 0);
            Marshal.ReleaseComObject(pInitSink);
            return 0;
        }

        int IWbemServices_Old.CancelAsyncCall_([In, MarshalAs(UnmanagedType.Interface)] IWbemObjectSink pSink)
        {
            return -2147217396;
        }

        int IWbemServices_Old.CreateClassEnum_([In, MarshalAs(UnmanagedType.BStr)] string strSuperclass, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [MarshalAs(UnmanagedType.Interface)] out IEnumWbemClassObject ppEnum)
        {
            ppEnum = null;
            return -2147217396;
        }

        int IWbemServices_Old.CreateClassEnumAsync_([In, MarshalAs(UnmanagedType.BStr)] string strSuperclass, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [In, MarshalAs(UnmanagedType.Interface)] IWbemObjectSink pResponseHandler)
        {
            return -2147217396;
        }

        int IWbemServices_Old.CreateInstanceEnum_([In, MarshalAs(UnmanagedType.BStr)] string strFilter, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [MarshalAs(UnmanagedType.Interface)] out IEnumWbemClassObject ppEnum)
        {
            ppEnum = null;
            return -2147217396;
        }

        int IWbemServices_Old.CreateInstanceEnumAsync_([In, MarshalAs(UnmanagedType.BStr)] string strFilter, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [In, MarshalAs(UnmanagedType.Interface)] IWbemObjectSink pResponseHandler)
        {
            try
            {
                preventShutdownLock.AcquireReaderLock(-1);
                if (shutdownInProgress != 0)
                {
                    return 0;
                }
                uint num = (uint) (Environment.TickCount + 100);
                Type type = null;
                foreach (Type type2 in this.instrumentedAssembly.mapTypeToConverter.Keys)
                {
                    if (string.Compare(ManagedNameAttribute.GetMemberName(type2), strFilter, StringComparison.Ordinal) == 0)
                    {
                        type = type2;
                        break;
                    }
                }
                if (null == type)
                {
                    return 0;
                }
                int num2 = 0x40;
                IntPtr[] ptrArray = new IntPtr[num2];
                IntPtr[] apObjArray = new IntPtr[num2];
                ConvertToWMI[] owmiArray = new ConvertToWMI[num2];
                IWbemClassObjectFreeThreaded[] threadedArray = new IWbemClassObjectFreeThreaded[num2];
                int plHandle = 0;
                int index = 0;
                object processIdentity = System.Management.Instrumentation.Instrumentation.ProcessIdentity;
                try
                {
                    InstrumentedAssembly.readerWriterLock.AcquireReaderLock(-1);
                    foreach (DictionaryEntry entry in InstrumentedAssembly.mapIDToPublishedObject)
                    {
                        if (shutdownInProgress != 0)
                        {
                            return 0;
                        }
                        if (type == entry.Value.GetType())
                        {
                            if (owmiArray[index] == null)
                            {
                                object target = Activator.CreateInstance((Type) this.instrumentedAssembly.mapTypeToConverter[type]);
                                owmiArray[index] = (ConvertToWMI) Delegate.CreateDelegate(typeof(ConvertToWMI), target, "ToWMI");
                                lock (entry.Value)
                                {
                                    owmiArray[index](entry.Value);
                                }
                                ptrArray[index] = (IntPtr) target.GetType().GetField("instWbemObjectAccessIP").GetValue(target);
                                Marshal.AddRef(ptrArray[index]);
                                threadedArray[index] = new IWbemClassObjectFreeThreaded(ptrArray[index]);
                                threadedArray[index].Put_("ProcessId", 0, ref processIdentity, 0);
                                if (index == 0)
                                {
                                    int num5;
                                    WmiNetUtilsHelper.GetPropertyHandle_f27(0x1b, (IntPtr) threadedArray[index], "InstanceId", out num5, out plHandle);
                                }
                            }
                            else
                            {
                                lock (entry.Value)
                                {
                                    owmiArray[index](entry.Value);
                                }
                                ptrArray[index] = (IntPtr) owmiArray[index].Target.GetType().GetField("instWbemObjectAccessIP").GetValue(owmiArray[index].Target);
                                Marshal.AddRef(ptrArray[index]);
                                threadedArray[index] = new IWbemClassObjectFreeThreaded(ptrArray[index]);
                                threadedArray[index].Put_("ProcessId", 0, ref processIdentity, 0);
                                if (index == 0)
                                {
                                    int num6;
                                    WmiNetUtilsHelper.GetPropertyHandle_f27(0x1b, (IntPtr) threadedArray[index], "InstanceId", out num6, out plHandle);
                                }
                            }
                            string key = (string) entry.Key;
                            WmiNetUtilsHelper.WritePropertyValue_f28(0x1c, (IntPtr) threadedArray[index], plHandle, (key.Length + 1) * 2, key);
                            index++;
                            if ((index == num2) || (Environment.TickCount >= num))
                            {
                                for (int i = 0; i < index; i++)
                                {
                                    WmiNetUtilsHelper.Clone_f(12, ptrArray[i], out apObjArray[i]);
                                }
                                int num8 = pResponseHandler.Indicate_(index, apObjArray);
                                for (int j = 0; j < index; j++)
                                {
                                    Marshal.Release(apObjArray[j]);
                                }
                                if (num8 != 0)
                                {
                                    return 0;
                                }
                                index = 0;
                                num = (uint) (Environment.TickCount + 100);
                            }
                        }
                    }
                }
                finally
                {
                    InstrumentedAssembly.readerWriterLock.ReleaseReaderLock();
                }
                if (index > 0)
                {
                    for (int k = 0; k < index; k++)
                    {
                        WmiNetUtilsHelper.Clone_f(12, ptrArray[k], out apObjArray[k]);
                    }
                    pResponseHandler.Indicate_(index, apObjArray);
                    for (int m = 0; m < index; m++)
                    {
                        Marshal.Release(apObjArray[m]);
                    }
                }
            }
            finally
            {
                pResponseHandler.SetStatus_(0, 0, null, IntPtr.Zero);
                Marshal.ReleaseComObject(pResponseHandler);
                preventShutdownLock.ReleaseReaderLock();
            }
            return 0;
        }

        int IWbemServices_Old.DeleteClass_([In, MarshalAs(UnmanagedType.BStr)] string strClass, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [In] IntPtr ppCallResult)
        {
            return -2147217396;
        }

        int IWbemServices_Old.DeleteClassAsync_([In, MarshalAs(UnmanagedType.BStr)] string strClass, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [In, MarshalAs(UnmanagedType.Interface)] IWbemObjectSink pResponseHandler)
        {
            return -2147217396;
        }

        int IWbemServices_Old.DeleteInstance_([In, MarshalAs(UnmanagedType.BStr)] string strObjectPath, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [In] IntPtr ppCallResult)
        {
            return -2147217396;
        }

        int IWbemServices_Old.DeleteInstanceAsync_([In, MarshalAs(UnmanagedType.BStr)] string strObjectPath, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [In, MarshalAs(UnmanagedType.Interface)] IWbemObjectSink pResponseHandler)
        {
            return -2147217396;
        }

        int IWbemServices_Old.ExecMethod_([In, MarshalAs(UnmanagedType.BStr)] string strObjectPath, [In, MarshalAs(UnmanagedType.BStr)] string strMethodName, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [In, MarshalAs(UnmanagedType.Interface)] IWbemClassObject_DoNotMarshal pInParams, [In, Out, MarshalAs(UnmanagedType.Interface)] ref IWbemClassObject_DoNotMarshal ppOutParams, [In] IntPtr ppCallResult)
        {
            return -2147217396;
        }

        int IWbemServices_Old.ExecMethodAsync_([In, MarshalAs(UnmanagedType.BStr)] string strObjectPath, [In, MarshalAs(UnmanagedType.BStr)] string strMethodName, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [In, MarshalAs(UnmanagedType.Interface)] IWbemClassObject_DoNotMarshal pInParams, [In, MarshalAs(UnmanagedType.Interface)] IWbemObjectSink pResponseHandler)
        {
            return -2147217396;
        }

        int IWbemServices_Old.ExecNotificationQuery_([In, MarshalAs(UnmanagedType.BStr)] string strQueryLanguage, [In, MarshalAs(UnmanagedType.BStr)] string strQuery, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [MarshalAs(UnmanagedType.Interface)] out IEnumWbemClassObject ppEnum)
        {
            ppEnum = null;
            return -2147217396;
        }

        int IWbemServices_Old.ExecNotificationQueryAsync_([In, MarshalAs(UnmanagedType.BStr)] string strQueryLanguage, [In, MarshalAs(UnmanagedType.BStr)] string strQuery, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [In, MarshalAs(UnmanagedType.Interface)] IWbemObjectSink pResponseHandler)
        {
            return -2147217396;
        }

        int IWbemServices_Old.ExecQuery_([In, MarshalAs(UnmanagedType.BStr)] string strQueryLanguage, [In, MarshalAs(UnmanagedType.BStr)] string strQuery, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [MarshalAs(UnmanagedType.Interface)] out IEnumWbemClassObject ppEnum)
        {
            ppEnum = null;
            return -2147217396;
        }

        int IWbemServices_Old.ExecQueryAsync_([In, MarshalAs(UnmanagedType.BStr)] string strQueryLanguage, [In, MarshalAs(UnmanagedType.BStr)] string strQuery, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [In, MarshalAs(UnmanagedType.Interface)] IWbemObjectSink pResponseHandler)
        {
            return -2147217396;
        }

        int IWbemServices_Old.GetObject_([In, MarshalAs(UnmanagedType.BStr)] string strObjectPath, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [In, Out, MarshalAs(UnmanagedType.Interface)] ref IWbemClassObject_DoNotMarshal ppObject, [In] IntPtr ppCallResult)
        {
            return -2147217396;
        }

        int IWbemServices_Old.GetObjectAsync_([In, MarshalAs(UnmanagedType.BStr)] string strObjectPath, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [In, MarshalAs(UnmanagedType.Interface)] IWbemObjectSink pResponseHandler)
        {
            Match match = Regex.Match(strObjectPath.ToLower(CultureInfo.InvariantCulture), "(.*?)\\.instanceid=\"(.*?)\",processid=\"(.*?)\"");
            if (!match.Success)
            {
                pResponseHandler.SetStatus_(0, -2147217406, null, IntPtr.Zero);
                Marshal.ReleaseComObject(pResponseHandler);
                return -2147217406;
            }
            string text1 = match.Groups[1].Value;
            string str = match.Groups[2].Value;
            string str2 = match.Groups[3].Value;
            if (System.Management.Instrumentation.Instrumentation.ProcessIdentity != str2)
            {
                pResponseHandler.SetStatus_(0, -2147217406, null, IntPtr.Zero);
                Marshal.ReleaseComObject(pResponseHandler);
                return -2147217406;
            }
            int num = ((IConvertible) str).ToInt32((IFormatProvider) CultureInfo.InvariantCulture.GetFormat(typeof(int)));
            object obj2 = null;
            try
            {
                InstrumentedAssembly.readerWriterLock.AcquireReaderLock(-1);
                obj2 = InstrumentedAssembly.mapIDToPublishedObject[num.ToString((IFormatProvider) CultureInfo.InvariantCulture.GetFormat(typeof(int)))];
            }
            finally
            {
                InstrumentedAssembly.readerWriterLock.ReleaseReaderLock();
            }
            if (obj2 != null)
            {
                Type type = (Type) this.instrumentedAssembly.mapTypeToConverter[obj2.GetType()];
                if (type != null)
                {
                    object target = Activator.CreateInstance(type);
                    ConvertToWMI owmi = (ConvertToWMI) Delegate.CreateDelegate(typeof(ConvertToWMI), target, "ToWMI");
                    lock (obj2)
                    {
                        owmi(obj2);
                    }
                    IntPtr[] apObjArray = new IntPtr[] { (IntPtr) target.GetType().GetField("instWbemObjectAccessIP").GetValue(target) };
                    Marshal.AddRef(apObjArray[0]);
                    IWbemClassObjectFreeThreaded threaded = new IWbemClassObjectFreeThreaded(apObjArray[0]);
                    object pVal = num;
                    threaded.Put_("InstanceId", 0, ref pVal, 0);
                    pVal = System.Management.Instrumentation.Instrumentation.ProcessIdentity;
                    threaded.Put_("ProcessId", 0, ref pVal, 0);
                    pResponseHandler.Indicate_(1, apObjArray);
                    pResponseHandler.SetStatus_(0, 0, null, IntPtr.Zero);
                    Marshal.ReleaseComObject(pResponseHandler);
                    return 0;
                }
            }
            pResponseHandler.SetStatus_(0, -2147217406, null, IntPtr.Zero);
            Marshal.ReleaseComObject(pResponseHandler);
            return -2147217406;
        }

        int IWbemServices_Old.OpenNamespace_([In, MarshalAs(UnmanagedType.BStr)] string strNamespace, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [In, Out, MarshalAs(UnmanagedType.Interface)] ref IWbemServices ppWorkingNamespace, [In] IntPtr ppCallResult)
        {
            return -2147217396;
        }

        int IWbemServices_Old.PutClass_([In, MarshalAs(UnmanagedType.Interface)] IWbemClassObject_DoNotMarshal pObject, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [In] IntPtr ppCallResult)
        {
            return -2147217396;
        }

        int IWbemServices_Old.PutClassAsync_([In, MarshalAs(UnmanagedType.Interface)] IWbemClassObject_DoNotMarshal pObject, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [In, MarshalAs(UnmanagedType.Interface)] IWbemObjectSink pResponseHandler)
        {
            return -2147217396;
        }

        int IWbemServices_Old.PutInstance_([In, MarshalAs(UnmanagedType.Interface)] IWbemClassObject_DoNotMarshal pInst, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [In] IntPtr ppCallResult)
        {
            return -2147217396;
        }

        int IWbemServices_Old.PutInstanceAsync_([In, MarshalAs(UnmanagedType.Interface)] IWbemClassObject_DoNotMarshal pInst, [In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [In, MarshalAs(UnmanagedType.Interface)] IWbemObjectSink pResponseHandler)
        {
            return -2147217396;
        }

        int IWbemServices_Old.QueryObjectSink_([In] int lFlags, [MarshalAs(UnmanagedType.Interface)] out IWbemObjectSink ppResponseHandler)
        {
            ppResponseHandler = null;
            return -2147217396;
        }

        private void UnRegister()
        {
            lock (this)
            {
                if (this.registrar != null)
                {
                    if (this.workerThreadInitialized)
                    {
                        this.alive = false;
                        this.doIndicate.Set();
                        GC.KeepAlive(this);
                        this.workerThreadInitialized = false;
                    }
                    this.registrar.UnRegister_();
                    this.registrar = null;
                }
            }
        }

        private class MTARequest
        {
            public AutoResetEvent doneIndicate = new AutoResetEvent(false);
            public Exception exception;
            public int lengthFromSTA = -1;
            public IntPtr[] objectsFromSTA;

            public MTARequest(int length, IntPtr[] objects)
            {
                this.lengthFromSTA = length;
                this.objectsFromSTA = objects;
            }
        }
    }
}

