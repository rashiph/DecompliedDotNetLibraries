namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal class EmitterCache
    {
        private AssemblyBuilder assemblyBuilder;
        private ModuleBuilder DynamicModule;
        private static object initLock = new object();
        private Dictionary<Type, Type> interfaceToClassMap;
        private static EmitterCache Provider = null;

        private EmitterCache()
        {
            AssemblyName name = new AssemblyName {
                Name = Guid.NewGuid().ToString()
            };
            this.assemblyBuilder = Thread.GetDomain().DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
            this.DynamicModule = this.assemblyBuilder.DefineDynamicModule(Guid.NewGuid().ToString());
            this.interfaceToClassMap = new Dictionary<Type, Type>();
        }

        internal Type FindOrCreateType(Type interfaceType)
        {
            if (!interfaceType.IsInterface)
            {
                throw Fx.AssertAndThrow("Passed in type should be an Interface");
            }
            Type type = null;
            lock (this)
            {
                this.interfaceToClassMap.TryGetValue(interfaceType, out type);
                if (type == null)
                {
                    TypeBuilder builder = this.DynamicModule.DefineType(interfaceType.Name + "MarshalByRefObject", TypeAttributes.Abstract | TypeAttributes.Public, typeof(MarshalByRefObject), new Type[] { interfaceType });
                    Type[] types = new Type[] { typeof(ClassInterfaceType) };
                    CustomAttributeBuilder customBuilder = new CustomAttributeBuilder(typeof(ClassInterfaceAttribute).GetConstructor(types), new object[] { ClassInterfaceType.None });
                    builder.SetCustomAttribute(customBuilder);
                    builder.AddInterfaceImplementation(interfaceType);
                    foreach (System.Reflection.MethodInfo info2 in interfaceType.GetMethods())
                    {
                        builder.DefineMethod(info2.Name, MethodAttributes.Abstract | MethodAttributes.NewSlot | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Public, info2.ReturnType, this.GetParameterTypes(info2));
                    }
                    type = builder.CreateType();
                    this.interfaceToClassMap[interfaceType] = type;
                }
            }
            if (type == null)
            {
                throw Fx.AssertAndThrow("Class Type should not be null at this point");
            }
            return type;
        }

        private Type[] GetParameterTypes(System.Reflection.MethodInfo mInfo)
        {
            ParameterInfo[] parameters = mInfo.GetParameters();
            Type[] typeArray = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                typeArray[i] = parameters[i].ParameterType;
            }
            return typeArray;
        }

        internal static EmitterCache TypeEmitter
        {
            get
            {
                lock (initLock)
                {
                    if (Provider == null)
                    {
                        EmitterCache cache = new EmitterCache();
                        Thread.MemoryBarrier();
                        Provider = cache;
                    }
                }
                if (Provider == null)
                {
                    throw Fx.AssertAndThrowFatal("Provider should not be null");
                }
                return Provider;
            }
        }
    }
}

