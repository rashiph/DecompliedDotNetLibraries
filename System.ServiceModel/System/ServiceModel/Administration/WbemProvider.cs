namespace System.ServiceModel.Administration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Text.RegularExpressions;
    using System.Threading;

    internal class WbemProvider : WbemNative.IWbemProviderInit, WbemNative.IWbemServices
    {
        private string appName;
        private bool initialized;
        private string nameSpace;
        private object syncRoot = new object();
        private WbemNative.IWbemDecoupledRegistrar wbemRegistrar;
        private WbemNative.IWbemServices wbemServices;
        private Dictionary<string, IWmiProvider> wmiProviders = new Dictionary<string, IWmiProvider>(StringComparer.OrdinalIgnoreCase);

        internal WbemProvider(string nameSpace, string appName)
        {
            this.nameSpace = nameSpace;
            this.appName = appName;
        }

        private void ExitOrUnloadEventHandler(object sender, EventArgs e)
        {
            if (this.wbemRegistrar != null)
            {
                MTAExecute(new WaitCallback(this.UnRegisterWbemProvider), null);
            }
        }

        private IWmiProvider GetProvider(string className)
        {
            IWmiProvider provider;
            lock (this.wmiProviders)
            {
                if (!this.wmiProviders.TryGetValue(className, out provider))
                {
                    provider = NoInstanceWMIProvider.Default;
                }
            }
            return provider;
        }

        internal void Initialize()
        {
            try
            {
                AppDomain.CurrentDomain.DomainUnload += new EventHandler(this.ExitOrUnloadEventHandler);
                AppDomain.CurrentDomain.ProcessExit += new EventHandler(this.ExitOrUnloadEventHandler);
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(this.ExitOrUnloadEventHandler);
                MTAExecute(new WaitCallback(this.RegisterWbemProvider), null);
                this.initialized = true;
            }
            catch (SecurityException)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("PartialTrustWMINotEnabled")));
            }
        }

        internal static void MTAExecute(WaitCallback callback, object state)
        {
            if (Thread.CurrentThread.GetApartmentState() != ApartmentState.MTA)
            {
                using (ThreadJob job = new ThreadJob(callback, state))
                {
                    Thread thread = new Thread(new ThreadStart(job.Run));
                    thread.SetApartmentState(ApartmentState.MTA);
                    thread.IsBackground = true;
                    thread.Start();
                    Exception innerException = job.Wait();
                    if (innerException != null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ApplicationException(System.ServiceModel.SR.GetString("AdminMTAWorkerThreadException"), innerException));
                    }
                    return;
                }
            }
            callback(state);
        }

        public void Register(string className, IWmiProvider wmiProvider)
        {
            lock (this.syncRoot)
            {
                if (!this.initialized)
                {
                    this.Initialize();
                }
                this.wmiProviders.Add(className, wmiProvider);
            }
        }

        private void RegisterWbemProvider(object state)
        {
            this.wbemRegistrar = (WbemNative.IWbemDecoupledRegistrar) new WbemNative.WbemDecoupledRegistrar();
            int num = this.wbemRegistrar.Register(0, null, null, null, this.nameSpace, this.appName, this);
            if (num != 0)
            {
                DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error, EventLogCategory.Wmi, (EventLogEventId) (-1073610734), new string[] { TraceUtility.CreateSourceString(this), num.ToString("x", CultureInfo.InvariantCulture) });
                this.wbemRegistrar = null;
            }
        }

        private void RelocateWbemServicesRCWToMTA(object comObject)
        {
            IntPtr iUnknownForObject = Marshal.GetIUnknownForObject(comObject);
            Marshal.ReleaseComObject(comObject);
            this.wbemServices = (WbemNative.IWbemServices) Marshal.GetObjectForIUnknown(iUnknownForObject);
            Marshal.Release(iUnknownForObject);
        }

        int WbemNative.IWbemProviderInit.Initialize(string wszUser, int lFlags, string wszNamespace, string wszLocale, WbemNative.IWbemServices wbemServices, WbemNative.IWbemContext wbemContext, WbemNative.IWbemProviderInitSink wbemSink)
        {
            if (((wbemServices == null) || (wbemContext == null)) || (wbemSink == null))
            {
                return -2147217400;
            }
            try
            {
                MTAExecute(new WaitCallback(this.RelocateWbemServicesRCWToMTA), wbemServices);
                wbemSink.SetStatus(0, 0);
            }
            catch (WbemException exception)
            {
                wbemSink.SetStatus(exception.ErrorCode, 0);
                return exception.ErrorCode;
            }
            catch (Exception)
            {
                wbemSink.SetStatus(-2147217407, 0);
                return -2147217407;
            }
            finally
            {
                Marshal.ReleaseComObject(wbemSink);
            }
            return 0;
        }

        int WbemNative.IWbemServices.CancelAsyncCall(WbemNative.IWbemObjectSink wbemSink)
        {
            return -2147217396;
        }

        int WbemNative.IWbemServices.CreateClassEnum(string superClassName, int flags, WbemNative.IWbemContext wbemContext, out WbemNative.IEnumWbemClassObject wbemEnum)
        {
            wbemEnum = null;
            return -2147217396;
        }

        int WbemNative.IWbemServices.CreateClassEnumAsync(string superClassName, int flags, WbemNative.IWbemContext wbemContext, WbemNative.IWbemObjectSink wbemSink)
        {
            return -2147217396;
        }

        int WbemNative.IWbemServices.CreateInstanceEnum(string filter, int flags, WbemNative.IWbemContext wbemContext, out WbemNative.IEnumWbemClassObject wbemEnum)
        {
            wbemEnum = null;
            return -2147217396;
        }

        int WbemNative.IWbemServices.CreateInstanceEnumAsync(string className, int flags, WbemNative.IWbemContext wbemContext, WbemNative.IWbemObjectSink wbemSink)
        {
            if (((wbemContext == null) || (wbemSink == null)) || (this.wbemServices == null))
            {
                return -2147217400;
            }
            try
            {
                ParameterContext parms = new ParameterContext(className, this.wbemServices, wbemContext, wbemSink);
                this.GetProvider(parms.ClassName).EnumInstances(new InstancesContext(parms));
                WbemException.ThrowIfFail(wbemSink.SetStatus(0, 0, null, null));
            }
            catch (WbemException exception)
            {
                wbemSink.SetStatus(0, exception.ErrorCode, null, null);
                DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error, EventLogCategory.Wmi, (EventLogEventId) (-1073610737), new string[] { className, exception.ToString() });
                return exception.ErrorCode;
            }
            catch (Exception exception2)
            {
                DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error, EventLogCategory.Wmi, (EventLogEventId) (-1073610737), new string[] { className, exception2.ToString() });
                wbemSink.SetStatus(0, -2147217407, null, null);
                return -2147217407;
            }
            finally
            {
                Marshal.ReleaseComObject(wbemSink);
            }
            return 0;
        }

        int WbemNative.IWbemServices.DeleteClass(string className, int flags, WbemNative.IWbemContext wbemContext, IntPtr wbemCallResult)
        {
            return -2147217396;
        }

        int WbemNative.IWbemServices.DeleteClassAsync(string className, int lFlags, WbemNative.IWbemContext wbemContext, WbemNative.IWbemObjectSink wbemSink)
        {
            return -2147217396;
        }

        int WbemNative.IWbemServices.DeleteInstance(string objectPath, int flags, WbemNative.IWbemContext wbemContext, IntPtr wbemCallResult)
        {
            return -2147217396;
        }

        int WbemNative.IWbemServices.DeleteInstanceAsync(string objectPath, int lFlags, WbemNative.IWbemContext wbemContext, WbemNative.IWbemObjectSink wbemSink)
        {
            if (((wbemContext == null) || (wbemSink == null)) || (this.wbemServices == null))
            {
                return -2147217400;
            }
            try
            {
                ObjectPathRegex objPathRegex = new ObjectPathRegex(objectPath);
                ParameterContext parms = new ParameterContext(objPathRegex.ClassName, this.wbemServices, wbemContext, wbemSink);
                WbemInstance wbemInstance = new WbemInstance(parms, objPathRegex);
                if (this.GetProvider(parms.ClassName).DeleteInstance(new InstanceContext(wbemInstance)))
                {
                    wbemInstance.Indicate();
                }
                WbemException.ThrowIfFail(wbemSink.SetStatus(0, 0, null, null));
            }
            catch (WbemException exception)
            {
                DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error, EventLogCategory.Wmi, (EventLogEventId) (-1073610738), new string[] { exception.ToString() });
                wbemSink.SetStatus(0, exception.ErrorCode, null, null);
                return exception.ErrorCode;
            }
            catch (Exception exception2)
            {
                DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error, EventLogCategory.Wmi, (EventLogEventId) (-1073610738), new string[] { exception2.ToString() });
                wbemSink.SetStatus(0, -2147217407, null, null);
                return -2147217407;
            }
            finally
            {
                Marshal.ReleaseComObject(wbemSink);
            }
            return 0;
        }

        int WbemNative.IWbemServices.ExecMethod(string objectPath, string methodName, int flags, WbemNative.IWbemContext wbemContext, WbemNative.IWbemClassObject wbemInParams, ref WbemNative.IWbemClassObject wbemOutParams, IntPtr wbemCallResult)
        {
            return -2147217396;
        }

        int WbemNative.IWbemServices.ExecMethodAsync(string objectPath, string methodName, int flags, WbemNative.IWbemContext wbemContext, WbemNative.IWbemClassObject wbemInParams, WbemNative.IWbemObjectSink wbemSink)
        {
            if (((wbemContext == null) || (wbemInParams == null)) || ((wbemSink == null) || (this.wbemServices == null)))
            {
                return -2147217400;
            }
            int hResult = 0;
            try
            {
                ObjectPathRegex objPathRegex = new ObjectPathRegex(objectPath);
                ParameterContext parms = new ParameterContext(objPathRegex.ClassName, this.wbemServices, wbemContext, wbemSink);
                WbemInstance wbemInstance = new WbemInstance(parms, objPathRegex);
                MethodContext method = new MethodContext(parms, methodName, wbemInParams, wbemInstance);
                if (!this.GetProvider(parms.ClassName).InvokeMethod(method))
                {
                    hResult = -2147217406;
                }
                WbemException.ThrowIfFail(wbemSink.SetStatus(0, hResult, null, null));
            }
            catch (WbemException exception)
            {
                DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error, EventLogCategory.Wmi, (EventLogEventId) (-1073610735), new string[] { exception.ToString() });
                hResult = exception.ErrorCode;
                wbemSink.SetStatus(0, hResult, null, null);
            }
            catch (Exception exception2)
            {
                DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error, EventLogCategory.Wmi, (EventLogEventId) (-1073610735), new string[] { exception2.ToString() });
                hResult = -2147217407;
                wbemSink.SetStatus(0, hResult, null, null);
            }
            finally
            {
                Marshal.ReleaseComObject(wbemSink);
            }
            return hResult;
        }

        int WbemNative.IWbemServices.ExecNotificationQuery(string queryLanguage, string query, int flags, WbemNative.IWbemContext wbemContext, out WbemNative.IEnumWbemClassObject wbemEnum)
        {
            wbemEnum = null;
            return -2147217396;
        }

        int WbemNative.IWbemServices.ExecNotificationQueryAsync(string queryLanguage, string query, int flags, WbemNative.IWbemContext wbemContext, WbemNative.IWbemObjectSink wbemSink)
        {
            return -2147217396;
        }

        int WbemNative.IWbemServices.ExecQuery(string queryLanguage, string query, int flags, WbemNative.IWbemContext wbemContext, out WbemNative.IEnumWbemClassObject wbemEnum)
        {
            wbemEnum = null;
            return -2147217396;
        }

        int WbemNative.IWbemServices.ExecQueryAsync(string queryLanguage, string query, int flags, WbemNative.IWbemContext wbemContext, WbemNative.IWbemObjectSink wbemSink)
        {
            if (((wbemContext == null) || (wbemSink == null)) || (this.wbemServices == null))
            {
                return -2147217400;
            }
            try
            {
                QueryRegex regex = new QueryRegex(query);
                ParameterContext parms = new ParameterContext(regex.ClassName, this.wbemServices, wbemContext, wbemSink);
                this.GetProvider(parms.ClassName).EnumInstances(new InstancesContext(parms));
                WbemException.ThrowIfFail(wbemSink.SetStatus(0, 0, null, null));
            }
            catch (WbemException exception)
            {
                DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error, EventLogCategory.Wmi, (EventLogEventId) (-1073610736), new string[] { exception.ToString() });
                wbemSink.SetStatus(0, exception.ErrorCode, null, null);
                return exception.ErrorCode;
            }
            catch (Exception exception2)
            {
                DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error, EventLogCategory.Wmi, (EventLogEventId) (-1073610736), new string[] { exception2.ToString() });
                wbemSink.SetStatus(0, -2147217407, null, null);
                return -2147217407;
            }
            finally
            {
                Marshal.ReleaseComObject(wbemSink);
            }
            return 0;
        }

        int WbemNative.IWbemServices.GetObject(string objectPath, int flags, WbemNative.IWbemContext wbemContext, ref WbemNative.IWbemClassObject wbemObject, IntPtr wbemResult)
        {
            return -2147217396;
        }

        int WbemNative.IWbemServices.GetObjectAsync(string objectPath, int flags, WbemNative.IWbemContext wbemContext, WbemNative.IWbemObjectSink wbemSink)
        {
            if (((wbemContext == null) || (wbemSink == null)) || (this.wbemServices == null))
            {
                return -2147217400;
            }
            using (DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateActivity(true, System.ServiceModel.SR.GetString("WmiGetObject", new object[] { string.IsNullOrEmpty(objectPath) ? string.Empty : objectPath }), ActivityType.WmiGetObject) : null)
            {
                try
                {
                    ObjectPathRegex objPathRegex = new ObjectPathRegex(objectPath);
                    ParameterContext parms = new ParameterContext(objPathRegex.ClassName, this.wbemServices, wbemContext, wbemSink);
                    WbemInstance wbemInstance = new WbemInstance(parms, objPathRegex);
                    if (this.GetProvider(parms.ClassName).GetInstance(new InstanceContext(wbemInstance)))
                    {
                        wbemInstance.Indicate();
                    }
                    WbemException.ThrowIfFail(wbemSink.SetStatus(0, 0, null, null));
                }
                catch (WbemException exception)
                {
                    DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error, EventLogCategory.Wmi, (EventLogEventId) (-1073610740), new string[] { TraceUtility.CreateSourceString(this), exception.ToString() });
                    wbemSink.SetStatus(0, exception.ErrorCode, null, null);
                    return exception.ErrorCode;
                }
                catch (Exception exception2)
                {
                    DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error, EventLogCategory.Wmi, (EventLogEventId) (-1073610740), new string[] { TraceUtility.CreateSourceString(this), exception2.ToString() });
                    wbemSink.SetStatus(0, -2147217407, null, null);
                    return -2147217407;
                }
                finally
                {
                    Marshal.ReleaseComObject(wbemSink);
                }
            }
            return 0;
        }

        int WbemNative.IWbemServices.OpenNamespace(string nameSpace, int flags, WbemNative.IWbemContext wbemContext, ref WbemNative.IWbemServices wbemServices, IntPtr wbemCallResult)
        {
            return -2147217396;
        }

        int WbemNative.IWbemServices.PutClass(WbemNative.IWbemClassObject wbemObject, int flags, WbemNative.IWbemContext wbemContext, IntPtr wbemCallResult)
        {
            return -2147217396;
        }

        int WbemNative.IWbemServices.PutClassAsync(WbemNative.IWbemClassObject wbemObject, int flags, WbemNative.IWbemContext wbemContext, WbemNative.IWbemObjectSink wbemSink)
        {
            return -2147217396;
        }

        int WbemNative.IWbemServices.PutInstance(WbemNative.IWbemClassObject pInst, int lFlags, WbemNative.IWbemContext wbemContext, IntPtr wbemCallResult)
        {
            return -2147217396;
        }

        int WbemNative.IWbemServices.PutInstanceAsync(WbemNative.IWbemClassObject wbemObject, int lFlags, WbemNative.IWbemContext wbemContext, WbemNative.IWbemObjectSink wbemSink)
        {
            if (((wbemObject == null) || (wbemContext == null)) || ((wbemSink == null) || (this.wbemServices == null)))
            {
                return -2147217400;
            }
            using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity() : null)
            {
                try
                {
                    WbemException.ThrowIfFail(wbemObject.Get("__CLASS", 0, null, 0, 0));
                    string str = (string) pVal;
                    ServiceModelActivity.Start(activity, System.ServiceModel.SR.GetString("WmiPutInstance", new object[] { string.IsNullOrEmpty(str) ? string.Empty : str }), ActivityType.WmiPutInstance);
                    ParameterContext parms = new ParameterContext(str, this.wbemServices, wbemContext, wbemSink);
                    WbemInstance wbemInstance = new WbemInstance(parms, wbemObject);
                    if (this.GetProvider(parms.ClassName).PutInstance(new InstanceContext(wbemInstance)))
                    {
                        wbemInstance.Indicate();
                    }
                    WbemException.ThrowIfFail(wbemSink.SetStatus(0, 0, null, null));
                }
                catch (WbemException exception)
                {
                    DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error, EventLogCategory.Wmi, (EventLogEventId) (-1073610739), new string[] { TraceUtility.CreateSourceString(this), exception.ToString() });
                    wbemSink.SetStatus(0, exception.ErrorCode, null, null);
                    return exception.ErrorCode;
                }
                catch (Exception exception2)
                {
                    DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error, EventLogCategory.Wmi, (EventLogEventId) (-1073610739), new string[] { TraceUtility.CreateSourceString(this), exception2.ToString() });
                    wbemSink.SetStatus(0, -2147217407, null, null);
                    return -2147217407;
                }
                finally
                {
                    Marshal.ReleaseComObject(wbemSink);
                }
            }
            return 0;
        }

        int WbemNative.IWbemServices.QueryObjectSink(int flags, out WbemNative.IWbemObjectSink wbemSink)
        {
            wbemSink = null;
            return -2147217396;
        }

        private void UnRegisterWbemProvider(object state)
        {
            if (this.wbemRegistrar != null)
            {
                int num = this.wbemRegistrar.UnRegister();
                if (num != 0)
                {
                    DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error, EventLogCategory.Wmi, (EventLogEventId) (-1073610733), new string[] { TraceUtility.CreateSourceString(this), num.ToString("x", CultureInfo.InvariantCulture) });
                }
                this.wbemRegistrar = null;
            }
        }

        private class InstanceContext : IWmiInstance
        {
            private WbemProvider.WbemInstance wbemInstance;

            internal InstanceContext(WbemProvider.WbemInstance wbemInstance)
            {
                this.wbemInstance = wbemInstance;
            }

            object IWmiInstance.GetProperty(string name)
            {
                return this.wbemInstance.GetProperty(name);
            }

            IWmiInstance IWmiInstance.NewInstance(string className)
            {
                return new WbemProvider.InstanceContext(new WbemProvider.WbemInstance(this.wbemInstance, className));
            }

            void IWmiInstance.SetProperty(string name, object val)
            {
                this.wbemInstance.SetProperty(name, val);
            }

            internal WbemNative.IWbemClassObject WbemObject
            {
                get
                {
                    return this.wbemInstance.WbemObject;
                }
            }
        }

        private class InstancesContext : IWmiInstances
        {
            private WbemProvider.ParameterContext parms;

            internal InstancesContext(WbemProvider.ParameterContext parms)
            {
                this.parms = parms;
            }

            void IWmiInstances.AddInstance(IWmiInstance inst)
            {
                WbemException.ThrowIfFail(this.parms.WbemSink.Indicate(1, new WbemNative.IWbemClassObject[] { ((WbemProvider.InstanceContext) inst).WbemObject }));
            }

            IWmiInstance IWmiInstances.NewInstance(string className)
            {
                return new WbemProvider.InstanceContext(new WbemProvider.WbemInstance(this.parms, className));
            }
        }

        private class MethodContext : IWmiMethodContext
        {
            private IWmiInstance instance;
            private string methodName;
            private WbemProvider.ParameterContext parms;
            private WbemNative.IWbemClassObject wbemInParms;
            private WbemNative.IWbemClassObject wbemOutParms;

            internal MethodContext(WbemProvider.ParameterContext parms, string methodName, WbemNative.IWbemClassObject wbemInParms, WbemProvider.WbemInstance wbemInstance)
            {
                this.parms = parms;
                this.methodName = methodName;
                this.wbemInParms = wbemInParms;
                this.instance = new WbemProvider.InstanceContext(wbemInstance);
                WbemNative.IWbemClassObject ppObject = null;
                WbemException.ThrowIfFail(parms.WbemServices.GetObject(parms.ClassName, 0, parms.WbemContext, ref ppObject, IntPtr.Zero));
                WbemNative.IWbemClassObject ppOutSignature = null;
                WbemException.ThrowIfFail(ppObject.GetMethod(methodName, 0, IntPtr.Zero, out ppOutSignature));
                WbemException.ThrowIfFail(ppOutSignature.SpawnInstance(0, out this.wbemOutParms));
            }

            object IWmiMethodContext.GetParameter(string name)
            {
                object pVal = null;
                WbemException.ThrowIfFail(this.wbemInParms.Get(name, 0, ref pVal, 0, 0));
                return pVal;
            }

            void IWmiMethodContext.SetParameter(string name, object value)
            {
                WbemException.ThrowIfFail(this.wbemOutParms.Put(name, 0, ref value, 0));
            }

            IWmiInstance IWmiMethodContext.Instance
            {
                get
                {
                    return this.instance;
                }
            }

            string IWmiMethodContext.MethodName
            {
                get
                {
                    return this.methodName;
                }
            }

            object IWmiMethodContext.ReturnParameter
            {
                set
                {
                    object pVal = value;
                    WbemException.ThrowIfFail(this.wbemOutParms.Put("ReturnValue", 0, ref pVal, 0));
                    WbemException.ThrowIfFail(this.parms.WbemSink.Indicate(1, new WbemNative.IWbemClassObject[] { this.wbemOutParms }));
                }
            }
        }

        private class NoInstanceWMIProvider : IWmiProvider
        {
            private static WbemProvider.NoInstanceWMIProvider singleton;

            bool IWmiProvider.DeleteInstance(IWmiInstance instance)
            {
                return false;
            }

            void IWmiProvider.EnumInstances(IWmiInstances instances)
            {
            }

            bool IWmiProvider.GetInstance(IWmiInstance instance)
            {
                return false;
            }

            bool IWmiProvider.InvokeMethod(IWmiMethodContext method)
            {
                return false;
            }

            bool IWmiProvider.PutInstance(IWmiInstance instance)
            {
                return false;
            }

            internal static WbemProvider.NoInstanceWMIProvider Default
            {
                get
                {
                    if (singleton == null)
                    {
                        singleton = new WbemProvider.NoInstanceWMIProvider();
                    }
                    return singleton;
                }
            }
        }

        private class ObjectPathRegex
        {
            private string className;
            private static Regex classRegEx = new Regex(@"^(?<className>.*?)\.(?<keys>.*)");
            private Dictionary<string, object> keys = new Dictionary<string, object>();
            private static Regex keysRegEx = new Regex("(?<key>.*?)=((?<ival>[\\d]+)|\"(?<sval>.*?)\"),?");
            private static Regex nsRegEx = new Regex("^(?<namespace>[^\"]*?:)(?<path>.*)");

            public ObjectPathRegex(string objectPath)
            {
                objectPath = objectPath.Replace(@"\\", @"\");
                Match match = nsRegEx.Match(objectPath);
                if (match.Success)
                {
                    objectPath = match.Groups["path"].Value;
                }
                match = classRegEx.Match(objectPath);
                this.className = match.Groups["className"].Value;
                string input = match.Groups["keys"].Value;
                match = keysRegEx.Match(input);
                if (!match.Success)
                {
                    WbemException.Throw(WbemNative.WbemStatus.WBEM_E_INVALID_OBJECT_PATH);
                }
                while (match.Success)
                {
                    if (!string.IsNullOrEmpty(match.Groups["ival"].Value))
                    {
                        this.keys.Add(match.Groups["key"].Value, int.Parse(match.Groups["ival"].Value, CultureInfo.CurrentCulture));
                    }
                    else
                    {
                        this.keys.Add(match.Groups["key"].Value, match.Groups["sval"].Value);
                    }
                    match = match.NextMatch();
                }
            }

            internal string ClassName
            {
                get
                {
                    return this.className;
                }
            }

            internal Dictionary<string, object> Keys
            {
                get
                {
                    return this.keys;
                }
            }
        }

        private class ParameterContext
        {
            private string className;
            private WbemNative.IWbemContext wbemContext;
            private WbemNative.IWbemServices wbemServices;
            private WbemNative.IWbemObjectSink wbemSink;

            internal ParameterContext(string className, WbemNative.IWbemServices wbemServices, WbemNative.IWbemContext wbemContext, WbemNative.IWbemObjectSink wbemSink)
            {
                this.className = className;
                this.wbemServices = wbemServices;
                this.wbemContext = wbemContext;
                this.wbemSink = wbemSink;
            }

            internal string ClassName
            {
                get
                {
                    return this.className;
                }
            }

            internal WbemNative.IWbemContext WbemContext
            {
                get
                {
                    return this.wbemContext;
                }
            }

            internal WbemNative.IWbemServices WbemServices
            {
                get
                {
                    return this.wbemServices;
                }
            }

            internal WbemNative.IWbemObjectSink WbemSink
            {
                get
                {
                    return this.wbemSink;
                }
            }
        }

        private class QueryRegex
        {
            private string className;
            private static Regex regEx = new Regex(@"\bfrom\b\s+(?<className>\w+)", RegexOptions.IgnoreCase);

            internal QueryRegex(string query)
            {
                Match match = regEx.Match(query);
                if (!match.Success)
                {
                    WbemException.Throw(WbemNative.WbemStatus.WBEM_E_INVALID_QUERY);
                }
                this.className = match.Groups["className"].Value;
            }

            internal string ClassName
            {
                get
                {
                    return this.className;
                }
            }
        }

        private class ThreadJob : IDisposable
        {
            private WaitCallback callback;
            private ManualResetEvent evtDone = new ManualResetEvent(false);
            private Exception exception;
            private object state;

            public ThreadJob(WaitCallback callback, object state)
            {
                this.callback = callback;
                this.state = state;
            }

            public void Dispose()
            {
                if (this.evtDone != null)
                {
                    this.evtDone.Close();
                    this.evtDone = null;
                }
            }

            public void Run()
            {
                try
                {
                    this.callback(this.state);
                }
                catch (Exception exception)
                {
                    this.exception = exception;
                }
                finally
                {
                    this.evtDone.Set();
                }
            }

            public Exception Wait()
            {
                this.evtDone.WaitOne();
                return this.exception;
            }
        }

        private class WbemInstance
        {
            private string className;
            private WbemProvider.ParameterContext parms;
            private WbemNative.IWbemClassObject wbemObject;

            internal WbemInstance(WbemProvider.ParameterContext parms, WbemNative.IWbemClassObject wbemObject)
            {
                this.parms = parms;
                this.wbemObject = wbemObject;
            }

            internal WbemInstance(WbemProvider.ParameterContext parms, WbemProvider.ObjectPathRegex objPathRegex) : this(parms, objPathRegex.ClassName)
            {
                foreach (KeyValuePair<string, object> pair in objPathRegex.Keys)
                {
                    this.SetProperty(pair.Key, pair.Value);
                }
            }

            internal WbemInstance(WbemProvider.ParameterContext parms, string className)
            {
                this.parms = parms;
                if (string.IsNullOrEmpty(className))
                {
                    className = parms.ClassName;
                }
                this.className = className;
                WbemNative.IWbemClassObject ppObject = null;
                WbemException.ThrowIfFail(parms.WbemServices.GetObject(className, 0, parms.WbemContext, ref ppObject, IntPtr.Zero));
                if (ppObject != null)
                {
                    WbemException.ThrowIfFail(ppObject.SpawnInstance(0, out this.wbemObject));
                }
            }

            internal WbemInstance(WbemProvider.WbemInstance wbemInstance, string className) : this(wbemInstance.parms, className)
            {
            }

            internal object GetProperty(string name)
            {
                object pVal = null;
                WbemException.ThrowIfFail(this.wbemObject.Get(name, 0, ref pVal, 0, 0));
                return pVal;
            }

            internal void Indicate()
            {
                WbemException.ThrowIfFail(this.parms.WbemSink.Indicate(1, new WbemNative.IWbemClassObject[] { this.wbemObject }));
            }

            internal void SetProperty(string name, object val)
            {
                if (val != null)
                {
                    WbemNative.CIMTYPE cimtype = WbemNative.CIMTYPE.CIM_EMPTY;
                    if (val is DateTime)
                    {
                        val = ((DateTime) val).ToString("yyyyMMddhhmmss.ffffff", CultureInfo.InvariantCulture) + "+000";
                    }
                    else if (val is TimeSpan)
                    {
                        TimeSpan span = (TimeSpan) val;
                        long num = (span.Ticks % 0x3e8L) / 10L;
                        val = string.Format(CultureInfo.InvariantCulture, "{0:00000000}{1:00}{2:00}{3:00}.{4:000}{5:000}:000", new object[] { span.Days, span.Hours, span.Minutes, span.Seconds, span.Milliseconds, num });
                    }
                    else if (val is WbemProvider.InstanceContext)
                    {
                        WbemProvider.InstanceContext context = (WbemProvider.InstanceContext) val;
                        val = context.WbemObject;
                    }
                    else if (val is Array)
                    {
                        Array array = (Array) val;
                        if ((array.GetLength(0) > 0) && (array.GetValue(0) is WbemProvider.InstanceContext))
                        {
                            WbemNative.IWbemClassObject[] objArray = new WbemNative.IWbemClassObject[array.GetLength(0)];
                            for (int i = 0; i < objArray.Length; i++)
                            {
                                objArray[i] = ((WbemProvider.InstanceContext) array.GetValue(i)).WbemObject;
                            }
                            val = objArray;
                        }
                    }
                    else if (val is long)
                    {
                        val = ((long) val).ToString(CultureInfo.InvariantCulture);
                        cimtype = WbemNative.CIMTYPE.CIM_SINT64;
                    }
                    int hr = this.wbemObject.Put(name, 0, ref val, (int) cimtype);
                    if ((-2147217403 == hr) || (-2147217406 == hr))
                    {
                        EventLogEventId id;
                        if (-2147217403 == hr)
                        {
                            id = (EventLogEventId) (-1073610732);
                        }
                        else
                        {
                            id = (EventLogEventId) (-1073610731);
                        }
                        DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error, EventLogCategory.Wmi, id, new string[] { this.className, name, val.GetType().ToString() });
                    }
                    else
                    {
                        WbemException.ThrowIfFail(hr);
                    }
                }
            }

            internal WbemNative.IWbemClassObject WbemObject
            {
                get
                {
                    return this.wbemObject;
                }
            }
        }
    }
}

