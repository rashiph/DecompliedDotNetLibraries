namespace System.Management.Instrumentation
{
    using Microsoft.CSharp;
    using System;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.Management;
    using System.Reflection;
    using System.Threading;

    internal class InstrumentedAssembly
    {
        private Type lastType;
        private TypeInfo lastTypeInfo;
        public static Hashtable mapIDToPublishedObject = new Hashtable();
        private static Hashtable mapPublishedObjectToID = new Hashtable();
        public Hashtable mapTypeToConverter;
        private Hashtable mapTypeToTypeInfo = new Hashtable();
        private SchemaNaming naming;
        public static ReaderWriterLock readerWriterLock = new ReaderWriterLock();
        public EventSource source;
        private static int upcountId = 0xeff;

        public InstrumentedAssembly(Assembly assembly, SchemaNaming naming)
        {
            SecurityHelper.UnmanagedCode.Demand();
            this.naming = naming;
            Assembly precompiledAssembly = naming.PrecompiledAssembly;
            if (null == precompiledAssembly)
            {
                CSharpCodeProvider provider = new CSharpCodeProvider();
                CompilerParameters parameters = new CompilerParameters {
                    GenerateInMemory = true
                };
                parameters.ReferencedAssemblies.Add(assembly.Location);
                parameters.ReferencedAssemblies.Add(typeof(BaseEvent).Assembly.Location);
                parameters.ReferencedAssemblies.Add(typeof(Component).Assembly.Location);
                foreach (Type type in assembly.GetTypes())
                {
                    if (this.IsInstrumentedType(type))
                    {
                        this.FindReferences(type, parameters);
                    }
                }
                CompilerResults results = provider.CompileAssemblyFromSource(parameters, new string[] { naming.Code });
                foreach (CompilerError error in results.Errors)
                {
                    Console.WriteLine(error.ToString());
                }
                if (results.Errors.HasErrors)
                {
                    throw new Exception(RC.GetString("FAILED_TO_BUILD_GENERATED_ASSEMBLY"));
                }
                precompiledAssembly = results.CompiledAssembly;
            }
            Type type2 = precompiledAssembly.GetType("WMINET_Converter");
            this.mapTypeToConverter = (Hashtable) type2.GetField("mapTypeToConverter").GetValue(null);
            if (!MTAHelper.IsNoContextMTA())
            {
                new ThreadDispatch(new ThreadDispatch.ThreadWorkerMethodWithParam(this.InitEventSource)) { Parameter = this }.Start();
            }
            else
            {
                this.InitEventSource(this);
            }
        }

        public void FindReferences(Type type, CompilerParameters parameters)
        {
            if (!parameters.ReferencedAssemblies.Contains(type.Assembly.Location))
            {
                parameters.ReferencedAssemblies.Add(type.Assembly.Location);
            }
            if (type.BaseType != null)
            {
                this.FindReferences(type.BaseType, parameters);
            }
            foreach (Type type2 in type.GetInterfaces())
            {
                if (type2.Assembly != type.Assembly)
                {
                    this.FindReferences(type2, parameters);
                }
            }
        }

        public void Fire(object o)
        {
            SecurityHelper.UnmanagedCode.Demand();
            this.Fire(o.GetType(), o);
        }

        public void Fire(Type t, object o)
        {
            this.GetTypeInfo(t).Fire(o);
        }

        private TypeInfo GetTypeInfo(Type t)
        {
            lock (this.mapTypeToTypeInfo)
            {
                if (this.lastType == t)
                {
                    return this.lastTypeInfo;
                }
                this.lastType = t;
                TypeInfo info = (TypeInfo) this.mapTypeToTypeInfo[t];
                if (info == null)
                {
                    info = new TypeInfo(this.source, this.naming, (Type) this.mapTypeToConverter[t]);
                    this.mapTypeToTypeInfo.Add(t, info);
                }
                this.lastTypeInfo = info;
                return info;
            }
        }

        private void InitEventSource(object param)
        {
            InstrumentedAssembly assembly = (InstrumentedAssembly) param;
            assembly.source = new EventSource(assembly.naming.NamespaceName, assembly.naming.DecoupledProviderInstanceName, this);
        }

        public bool IsInstrumentedType(Type type)
        {
            if ((null != type.GetInterface("System.Management.Instrumentation.IEvent", false)) || (null != type.GetInterface("System.Management.Instrumentation.IInstance", false)))
            {
                return true;
            }
            object[] customAttributes = type.GetCustomAttributes(typeof(InstrumentationClassAttribute), true);
            return ((customAttributes != null) && (customAttributes.Length != 0));
        }

        public void Publish(object o)
        {
            SecurityHelper.UnmanagedCode.Demand();
            try
            {
                readerWriterLock.AcquireWriterLock(-1);
                if (!mapPublishedObjectToID.ContainsKey(o))
                {
                    mapIDToPublishedObject.Add(upcountId.ToString((IFormatProvider) CultureInfo.InvariantCulture.GetFormat(typeof(int))), o);
                    mapPublishedObjectToID.Add(o, upcountId);
                    upcountId++;
                }
            }
            finally
            {
                readerWriterLock.ReleaseWriterLock();
            }
        }

        public void Revoke(object o)
        {
            SecurityHelper.UnmanagedCode.Demand();
            try
            {
                readerWriterLock.AcquireWriterLock(-1);
                object obj2 = mapPublishedObjectToID[o];
                if (obj2 != null)
                {
                    int num = (int) obj2;
                    mapPublishedObjectToID.Remove(o);
                    mapIDToPublishedObject.Remove(num.ToString((IFormatProvider) CultureInfo.InvariantCulture.GetFormat(typeof(int))));
                }
            }
            finally
            {
                readerWriterLock.ReleaseWriterLock();
            }
        }

        public void SetBatchSize(Type t, int batchSize)
        {
            this.GetTypeInfo(t).SetBatchSize(batchSize);
        }

        private class TypeInfo
        {
            private bool batchEvents = true;
            private int batchSize = 0x40;
            public Thread cleanupThread;
            private Type converterType;
            private ConvertToWMI convertFunctionNoBatch;
            private ConvertToWMI[] convertFunctionsBatch;
            private int currentIndex;
            private FieldInfo fieldInfo;
            public int lastFire;
            public EventSource source;
            private IntPtr[] wbemObjects;

            public TypeInfo(EventSource source, SchemaNaming naming, Type converterType)
            {
                this.converterType = converterType;
                this.source = source;
                object target = Activator.CreateInstance(converterType);
                this.convertFunctionNoBatch = (ConvertToWMI) Delegate.CreateDelegate(typeof(ConvertToWMI), target, "ToWMI");
                this.SetBatchSize(this.batchSize);
            }

            public void Cleanup()
            {
                int num = 0;
                while (num < 20)
                {
                    Thread.Sleep(100);
                    if (this.currentIndex == 0)
                    {
                        num++;
                    }
                    else
                    {
                        num = 0;
                        if ((Environment.TickCount - this.lastFire) >= 100)
                        {
                            lock (this)
                            {
                                if (this.currentIndex > 0)
                                {
                                    this.source.IndicateEvents(this.currentIndex, this.wbemObjects);
                                    this.currentIndex = 0;
                                    this.lastFire = Environment.TickCount;
                                }
                                continue;
                            }
                        }
                    }
                }
                this.cleanupThread = null;
            }

            public IntPtr ExtractIntPtr(object o)
            {
                return (IntPtr) o.GetType().GetField("instWbemObjectAccessIP").GetValue(o);
            }

            public void Fire(object o)
            {
                if (!this.source.Any())
                {
                    if (!this.batchEvents)
                    {
                        lock (this)
                        {
                            this.convertFunctionNoBatch(o);
                            this.wbemObjects[0] = (IntPtr) this.fieldInfo.GetValue(this.convertFunctionNoBatch.Target);
                            this.source.IndicateEvents(1, this.wbemObjects);
                            return;
                        }
                    }
                    lock (this)
                    {
                        this.convertFunctionsBatch[this.currentIndex++](o);
                        this.wbemObjects[this.currentIndex - 1] = (IntPtr) this.fieldInfo.GetValue(this.convertFunctionsBatch[this.currentIndex - 1].Target);
                        if (this.cleanupThread == null)
                        {
                            int tickCount = Environment.TickCount;
                            if ((tickCount - this.lastFire) < 0x3e8)
                            {
                                this.lastFire = Environment.TickCount;
                                this.cleanupThread = new Thread(new ThreadStart(this.Cleanup));
                                this.cleanupThread.SetApartmentState(ApartmentState.MTA);
                                this.cleanupThread.Start();
                            }
                            else
                            {
                                this.source.IndicateEvents(this.currentIndex, this.wbemObjects);
                                this.currentIndex = 0;
                                this.lastFire = tickCount;
                            }
                        }
                        else if (this.currentIndex == this.batchSize)
                        {
                            this.source.IndicateEvents(this.currentIndex, this.wbemObjects);
                            this.currentIndex = 0;
                            this.lastFire = Environment.TickCount;
                        }
                    }
                }
            }

            public void SetBatchSize(int batchSize)
            {
                if (batchSize <= 0)
                {
                    throw new ArgumentOutOfRangeException("batchSize");
                }
                if (!WMICapabilities.MultiIndicateSupported)
                {
                    batchSize = 1;
                }
                lock (this)
                {
                    if (this.currentIndex > 0)
                    {
                        this.source.IndicateEvents(this.currentIndex, this.wbemObjects);
                        this.currentIndex = 0;
                        this.lastFire = Environment.TickCount;
                    }
                    this.wbemObjects = new IntPtr[batchSize];
                    if (batchSize > 1)
                    {
                        this.batchEvents = true;
                        this.batchSize = batchSize;
                        this.convertFunctionsBatch = new ConvertToWMI[batchSize];
                        for (int i = 0; i < batchSize; i++)
                        {
                            object target = Activator.CreateInstance(this.converterType);
                            this.convertFunctionsBatch[i] = (ConvertToWMI) Delegate.CreateDelegate(typeof(ConvertToWMI), target, "ToWMI");
                            this.wbemObjects[i] = this.ExtractIntPtr(target);
                        }
                        this.fieldInfo = this.convertFunctionsBatch[0].Target.GetType().GetField("instWbemObjectAccessIP");
                    }
                    else
                    {
                        this.fieldInfo = this.convertFunctionNoBatch.Target.GetType().GetField("instWbemObjectAccessIP");
                        this.wbemObjects[0] = this.ExtractIntPtr(this.convertFunctionNoBatch.Target);
                        this.batchEvents = false;
                    }
                }
            }
        }
    }
}

