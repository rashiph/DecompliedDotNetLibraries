namespace System.Management.Instrumentation
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Management;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class Instrumentation
    {
        private static Hashtable instrumentedAssemblies = new Hashtable();
        private static string processIdentity = null;

        public static void Fire(object eventData)
        {
            IEvent event2 = eventData as IEvent;
            if (event2 != null)
            {
                event2.Fire();
            }
            else
            {
                GetFireFunction(eventData.GetType())(eventData);
            }
        }

        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        private static extern int GetCurrentProcessId();
        internal static ProvisionFunction GetFireFunction(Type type)
        {
            return new ProvisionFunction(GetInstrumentedAssembly(type.Assembly).Fire);
        }

        private static InstrumentedAssembly GetInstrumentedAssembly(Assembly assembly)
        {
            lock (instrumentedAssemblies)
            {
                if (!instrumentedAssemblies.ContainsKey(assembly))
                {
                    Initialize(assembly);
                }
                return (InstrumentedAssembly) instrumentedAssemblies[assembly];
            }
        }

        internal static ProvisionFunction GetPublishFunction(Type type)
        {
            return new ProvisionFunction(GetInstrumentedAssembly(type.Assembly).Publish);
        }

        internal static ProvisionFunction GetRevokeFunction(Type type)
        {
            return new ProvisionFunction(GetInstrumentedAssembly(type.Assembly).Revoke);
        }

        private static void Initialize(Assembly assembly)
        {
            lock (instrumentedAssemblies)
            {
                if (!instrumentedAssemblies.ContainsKey(assembly))
                {
                    SchemaNaming schemaNaming = SchemaNaming.GetSchemaNaming(assembly);
                    if (schemaNaming != null)
                    {
                        if (!schemaNaming.IsAssemblyRegistered())
                        {
                            if (!WMICapabilities.IsUserAdmin())
                            {
                                throw new Exception(RC.GetString("ASSEMBLY_NOT_REGISTERED"));
                            }
                            schemaNaming.DecoupledProviderInstanceName = AssemblyNameUtility.UniqueToAssemblyFullVersion(assembly);
                            schemaNaming.RegisterNonAssemblySpecificSchema(null);
                            schemaNaming.RegisterAssemblySpecificSchema();
                        }
                        InstrumentedAssembly assembly2 = new InstrumentedAssembly(assembly, schemaNaming);
                        instrumentedAssemblies.Add(assembly, assembly2);
                    }
                }
            }
        }

        public static bool IsAssemblyRegistered(Assembly assemblyToRegister)
        {
            if (null == assemblyToRegister)
            {
                throw new ArgumentNullException("assemblyToRegister");
            }
            lock (instrumentedAssemblies)
            {
                if (instrumentedAssemblies.ContainsKey(assemblyToRegister))
                {
                    return true;
                }
            }
            SchemaNaming schemaNaming = SchemaNaming.GetSchemaNaming(assemblyToRegister);
            if (schemaNaming == null)
            {
                return false;
            }
            return schemaNaming.IsAssemblyRegistered();
        }

        public static void Publish(object instanceData)
        {
            Type type = instanceData as Type;
            Assembly assembly = instanceData as Assembly;
            IInstance instance = instanceData as IInstance;
            if (type != null)
            {
                GetInstrumentedAssembly(type.Assembly);
            }
            else if (assembly != null)
            {
                GetInstrumentedAssembly(assembly);
            }
            else if (instance != null)
            {
                instance.Published = true;
            }
            else
            {
                GetPublishFunction(instanceData.GetType())(instanceData);
            }
        }

        public static void RegisterAssembly(Assembly assemblyToRegister)
        {
            if (null == assemblyToRegister)
            {
                throw new ArgumentNullException("assemblyToRegister");
            }
            GetInstrumentedAssembly(assemblyToRegister);
        }

        public static void Revoke(object instanceData)
        {
            IInstance instance = instanceData as IInstance;
            if (instance != null)
            {
                instance.Published = false;
            }
            else
            {
                GetRevokeFunction(instanceData.GetType())(instanceData);
            }
        }

        public static void SetBatchSize(Type instrumentationClass, int batchSize)
        {
            GetInstrumentedAssembly(instrumentationClass.Assembly).SetBatchSize(instrumentationClass, batchSize);
        }

        internal static string ProcessIdentity
        {
            get
            {
                lock (typeof(System.Management.Instrumentation.Instrumentation))
                {
                    if (processIdentity == null)
                    {
                        processIdentity = Guid.NewGuid().ToString().ToLower(CultureInfo.InvariantCulture);
                    }
                }
                return processIdentity;
            }
        }
    }
}

