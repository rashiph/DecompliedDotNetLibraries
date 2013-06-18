namespace System.Web.Util
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Threading;
    using System.Web;

    internal class FactoryGenerator
    {
        private ModuleBuilder _dynamicModule;
        private Type[] _emptyParameterList;
        private Type _factoryInterface;
        private Type[] _interfacesToImplement;
        private MethodInfo _methodToOverride;
        private Type _returnedType;

        internal FactoryGenerator() : this(typeof(object), typeof(IWebObjectFactory))
        {
        }

        private FactoryGenerator(Type returnedType, Type factoryInterface)
        {
            this._emptyParameterList = new Type[0];
            this._returnedType = returnedType;
            this._factoryInterface = factoryInterface;
            this._methodToOverride = factoryInterface.GetMethod("CreateInstance", new Type[0]);
            if (this._methodToOverride.ReturnType != this._returnedType)
            {
                throw new ArgumentException(System.Web.SR.GetString("FactoryInterface"));
            }
            this._interfacesToImplement = new Type[] { factoryInterface };
        }

        internal static void CheckPublicParameterlessConstructor(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (!type.IsPublic && !type.IsNestedPublic)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("FactoryGenerator_TypeNotPublic", new object[] { type.Name }));
            }
            if (type.GetConstructor(Type.EmptyTypes) == null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("FactoryGenerator_TypeHasNoParameterlessConstructor", new object[] { type.Name }));
            }
        }

        internal IWebObjectFactory CreateFactory(Type type)
        {
            return (IWebObjectFactory) Activator.CreateInstance(this.GetFactoryTypeWithAssert(type));
        }

        private Type GetFactoryTypeWithAssert(Type type)
        {
            CheckPublicParameterlessConstructor(type);
            if (this._dynamicModule == null)
            {
                lock (this)
                {
                    if (this._dynamicModule == null)
                    {
                        string uniqueCompilationName = GetUniqueCompilationName();
                        AssemblyName name = new AssemblyName {
                            Name = "A_" + uniqueCompilationName
                        };
                        this._dynamicModule = Thread.GetDomain().DefineDynamicAssembly(name, AssemblyBuilderAccess.Run, null, true, null).DefineDynamicModule("M_" + uniqueCompilationName);
                    }
                }
            }
            TypeBuilder builder2 = this._dynamicModule.DefineType("T_" + GetUniqueCompilationName(), TypeAttributes.Public, typeof(object), this._interfacesToImplement);
            MethodBuilder methodInfoBody = builder2.DefineMethod("CreateInstance", MethodAttributes.Virtual | MethodAttributes.Public, this._returnedType, null);
            ILGenerator iLGenerator = methodInfoBody.GetILGenerator();
            ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
            iLGenerator.Emit(OpCodes.Newobj, constructor);
            iLGenerator.Emit(OpCodes.Ret);
            builder2.DefineMethodOverride(methodInfoBody, this._methodToOverride);
            return builder2.CreateType();
        }

        private static string GetUniqueCompilationName()
        {
            return Guid.NewGuid().ToString().Replace('-', '_');
        }
    }
}

