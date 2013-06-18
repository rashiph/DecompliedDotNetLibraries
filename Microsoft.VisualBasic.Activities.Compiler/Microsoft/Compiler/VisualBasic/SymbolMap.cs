namespace Microsoft.Compiler.VisualBasic
{
    using Microsoft.VisualBasic.CompilerServices;
    using System;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal sealed class SymbolMap : IDisposable
    {
        private AnonymousDelegateEmitter m_anonymousDelegateEmitter;
        private AnonymousTypeEmitter m_anonymousTypeEmitter;
        private AssemblyBuilder m_assemblyBuilder;
        private ModuleBuilder m_moduleBuilder;

        private void ~SymbolMap()
        {
            bool flag1 = this.m_assemblyBuilder != null;
        }

        public sealed override void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose([MarshalAs(UnmanagedType.U1)] bool flag1)
        {
            if (flag1)
            {
                this.~SymbolMap();
            }
            else
            {
                base.Finalize();
            }
        }

        [return: MarshalAs(UnmanagedType.U1)]
        private unsafe bool Equals(BCSYM* pSymbol, Type type)
        {
            int num2;
            if (pSymbol == null)
            {
                RaiseException(GetLastHResultError(), 0, 0, null);
            }
            if (type == null)
            {
                RaiseException(GetLastHResultError(), 0, 0, null);
            }
            bool flag = false;
            byte num = *((byte*) pSymbol);
            int num3 = num * 3;
            if (((byte) (num3[&?s_rgBilkindInfo@BCSYM@@1QBUBilkindInfo@@B] & 4)) != 0)
            {
                if (((byte) (num == 0x24)) != 0)
                {
                    return this.EqualsGenericParamType((BCSYM_GenericParam*) pSymbol, type);
                }
                if (((byte) (num == 0x29)) != 0)
                {
                    return this.EqualsBoundGenericType((BCSYM_GenericTypeBinding*) pSymbol, type);
                }
                return (!type.IsGenericParameter && this.EqualsBasicType(BCSYM.PNamedRoot((BCSYM modopt(IsConst)* modopt(IsConst) modopt(IsConst)) pSymbol), type));
            }
            if (((byte) (num3[&?s_rgBilkindInfo@BCSYM@@1QBUBilkindInfo@@B + 2] & 2)) != 0)
            {
                return this.EqualsArrayType((BCSYM_ArrayType*) pSymbol, type);
            }
            if (((byte) (num == 2)) == 0)
            {
                RaiseException(GetLastHResultError(), 0, 0, null);
                return flag;
            }
            if (type.IsByRef)
            {
                BCSYM* bcsymPtr = BCSYM.DigThroughNamedType(*((BCSYM* modopt(IsConst) modopt(IsConst)*) (pSymbol + 12)));
                if (this.Equals(bcsymPtr, type.GetElementType()))
                {
                    num2 = 1;
                    goto Label_00CF;
                }
            }
            num2 = 0;
        Label_00CF:
            return (bool) ((byte) num2);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        private unsafe bool EqualsArrayType(BCSYM_ArrayType* pSymbol, Type type)
        {
            int num;
            if (type.IsArray && (*(((int*) (pSymbol + 0x10))) == type.GetArrayRank()))
            {
                BCSYM* bcsymPtr = BCSYM.DigThroughNamedType(*((BCSYM* modopt(IsConst) modopt(IsConst)*) (pSymbol + 12)));
                if (this.Equals(bcsymPtr, type.GetElementType()))
                {
                    num = 1;
                    goto Label_0035;
                }
            }
            num = 0;
        Label_0035:
            return (bool) ((byte) num);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        private unsafe bool EqualsBasicType(BCSYM_NamedRoot* pSymbol, Type type)
        {
            BCSYM_NamedRoot* rootPtr;
            int num;
            if (type.IsGenericParameter)
            {
                RaiseException(GetLastHResultError(), 0, 0, null);
            }
            bool flag = false;
            CompilerProject* projectPtr = BCSYM_NamedRoot.GetContainingProject(pSymbol);
            if (*(((byte*) (projectPtr + 0x1a))) != 0)
            {
                string str = new string(AssemblyIdentity.GetAssemblyIdentityString((AssemblyIdentity* modopt(IsConst) modopt(IsConst)) (projectPtr + 0x40)));
                flag = type.Assembly.FullName.Equals(str);
                if (flag)
                {
                    goto Label_0053;
                }
            }
            if (BCSYM.GetExternalSymbol((BCSYM* modopt(IsConst) modopt(IsConst)) pSymbol) == null)
            {
                return flag;
            }
        Label_0053:
            rootPtr = BCSYM_NamedRoot.GetParent(pSymbol);
            if ((rootPtr == null) || !BCSYM.IsType((BCSYM* modopt(IsConst) modopt(IsConst)) rootPtr))
            {
                return type.FullName.Equals(new string(BCSYM_NamedRoot.GetQualifiedEmittedName(pSymbol, null)));
            }
            if ((type.DeclaringType != null) && this.Equals((BCSYM*) rootPtr, type.DeclaringType))
            {
                ushort* numPtr = *((ushort**) (pSymbol + 12));
                if (type.Name.Equals(new string((char*) numPtr)))
                {
                    num = 1;
                    goto Label_00A3;
                }
            }
            num = 0;
        Label_00A3:
            return (bool) ((byte) num);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        private unsafe bool EqualsBoundGenericType(BCSYM_GenericTypeBinding* pSymbol, Type type)
        {
            int num5;
            bool isGenericType = type.IsGenericType;
            if (!isGenericType)
            {
                return isGenericType;
            }
            int length = 0;
            BCSYM_GenericTypeBinding* bindingPtr = *((BCSYM_GenericTypeBinding**) (pSymbol + 0x5c));
            if (bindingPtr == null)
            {
                isGenericType = this.EqualsBasicType(BCSYM.PNamedRoot((BCSYM modopt(IsConst)* modopt(IsConst) modopt(IsConst)) pSymbol), type.GetGenericTypeDefinition());
                goto Label_00A9;
            }
            Type declaringType = type.DeclaringType;
            if (declaringType == null)
            {
                return false;
            }
            length = declaringType.GetGenericArguments().Length;
            Type[] destinationArray = new Type[length];
            Array.ConstrainedCopy(type.GetGenericArguments(), 0, destinationArray, 0, length);
            declaringType = declaringType.MakeGenericType(destinationArray);
            if (this.Equals((BCSYM*) bindingPtr, declaringType))
            {
                ushort* numPtr = BCSYM.PNamedRoot((BCSYM modopt(IsConst)* modopt(IsConst) modopt(IsConst)) pSymbol)[12];
                if (type.Name.Equals(new string((char*) numPtr)))
                {
                    num5 = 1;
                    goto Label_0090;
                }
            }
            num5 = 0;
        Label_0090:
            isGenericType = (bool) ((byte) num5);
        Label_00A9:
            if (isGenericType)
            {
                Type[] genericArguments = type.GetGenericArguments();
                int num4 = genericArguments.Length;
                isGenericType = (bool) ((byte) (num4 == (BCSYM.GetGenericParamCount((BCSYM* modopt(IsConst) modopt(IsConst)) pSymbol) + length)));
                if (isGenericType)
                {
                    int index = length;
                    if (length < num4)
                    {
                        int num3 = 0;
                        do
                        {
                            if (!isGenericType)
                            {
                                return isGenericType;
                            }
                            BCSYM** bcsymPtr2 = *((BCSYM***) (pSymbol + 0x54));
                            BCSYM* bcsymPtr = BCSYM.DigThroughNamedType(*((BCSYM* modopt(IsConst) modopt(IsConst)*) (num3 + bcsymPtr2)));
                            isGenericType = this.Equals(bcsymPtr, genericArguments[index]);
                            index++;
                            num3 += 4;
                        }
                        while (index < genericArguments.Length);
                    }
                }
            }
            return isGenericType;
        }

        [return: MarshalAs(UnmanagedType.U1)]
        private unsafe bool EqualsGenericParamType(BCSYM_GenericParam* pSymbol, Type type)
        {
            bool isGenericParameter = type.IsGenericParameter;
            if (!isGenericParameter)
            {
                return isGenericParameter;
            }
            BCSYM_NamedRoot* rootPtr = BCSYM_NamedRoot.GetParent((BCSYM_NamedRoot* modopt(IsConst) modopt(IsConst)) pSymbol);
            int genericParameterPosition = type.GenericParameterPosition;
            if (BCSYM.IsType((BCSYM* modopt(IsConst) modopt(IsConst)) rootPtr) && (type.DeclaringMethod == null))
            {
                int num3;
                int length = 0;
                Type declaringType = type.DeclaringType;
                Type type2 = declaringType.DeclaringType;
                if (type2 != null)
                {
                    length = type2.GetGenericArguments().Length;
                    if (length > genericParameterPosition)
                    {
                        do
                        {
                            declaringType = type2;
                            type2 = type2.DeclaringType;
                            if (type2 == null)
                            {
                                length = 0;
                            }
                            else
                            {
                                length = type2.GetGenericArguments().Length;
                            }
                        }
                        while (length > genericParameterPosition);
                    }
                }
                if (((*(((int*) (pSymbol + 0x58))) + length) == genericParameterPosition) && this.EqualsBasicType(rootPtr, declaringType))
                {
                    num3 = 1;
                }
                else
                {
                    num3 = 0;
                }
                return (bool) ((byte) num3);
            }
            return ((((byte) ((*(((byte*) rootPtr)) * 3)[&?s_rgBilkindInfo@BCSYM@@1QBUBilkindInfo@@B] & 0x40)) != 0) && ((bool) ((byte) (*(((int*) (pSymbol + 0x58))) == genericParameterPosition))));
        }

        private unsafe MethodBase FindMethod(BCSYM_Proc* pProcSymbol, string name, MethodBase[] methods, Type type)
        {
            MethodBase base2;
            int num3 = BCSYM.GetGenericParamCount((BCSYM* modopt(IsConst) modopt(IsConst)) pProcSymbol);
            int num5 = BCSYM_Proc.GetParameterCount(pProcSymbol);
            int index = 0;
            if (0 >= methods.Length)
            {
                goto Label_013D;
            }
        Label_001A:
            base2 = methods[index];
            if ((base2.MemberType != MemberTypes.Method) || string.Equals(base2.Name, name, StringComparison.Ordinal))
            {
                ParameterInfo[] parameters = base2.GetParameters();
                int num4 = (int) (num3 > 0);
                if (((base2.IsGenericMethod == num4) && ((num3 == 0) || (base2.GetGenericArguments().Length == num3))) && (parameters.Length == num5))
                {
                    if (base2.MemberType == MemberTypes.Method)
                    {
                        bool flag2;
                        MethodInfo info = (MethodInfo) base2;
                        if (BCSYM_Member.GetType((BCSYM_Member* modopt(IsConst) modopt(IsConst)) pProcSymbol) == null)
                        {
                            flag2 = typeof(void).Equals(info.ReturnType);
                        }
                        else
                        {
                            flag2 = this.Equals(BCSYM_Member.GetType((BCSYM_Member* modopt(IsConst) modopt(IsConst)) pProcSymbol), info.ReturnType);
                        }
                        if (!flag2)
                        {
                            goto Label_010D;
                        }
                    }
                    else if (BCSYM_Member.GetType((BCSYM_Member* modopt(IsConst) modopt(IsConst)) pProcSymbol) != null)
                    {
                        goto Label_010D;
                    }
                    BCSYM_Param* paramPtr = *((BCSYM_Param**) (pProcSymbol + 0x54));
                    bool flag = true;
                    int num = 0;
                    if (0 >= parameters.Length)
                    {
                        goto Label_011B;
                    }
                    do
                    {
                        if (!flag)
                        {
                            goto Label_010D;
                        }
                        BCSYM* pSymbol = BCSYM.DigThroughNamedType(*((BCSYM* modopt(IsConst) modopt(IsConst)*) (paramPtr + 0x10)));
                        flag = this.Equals(pSymbol, parameters[num].ParameterType);
                        paramPtr = *((BCSYM_Param**) (paramPtr + 8));
                        num++;
                    }
                    while (num < parameters.Length);
                    if (flag)
                    {
                        goto Label_011B;
                    }
                }
            }
        Label_010D:
            index++;
            if (index < methods.Length)
            {
                goto Label_001A;
            }
            goto Label_013D;
        Label_011B:
            if (type.IsGenericType)
            {
                RuntimeTypeHandle typeHandle = type.TypeHandle;
                return MethodBase.GetMethodFromHandle(base2.MethodHandle, typeHandle);
            }
            return base2;
        Label_013D:
            return null;
        }

        private unsafe Type GetAnonymousDelegate(BCSYM* pSymbol)
        {
            Type type = null;
            BCSYM_NamedRoot* rootPtr = BCSYM.PNamedRoot((BCSYM modopt(IsConst)* modopt(IsConst) modopt(IsConst)) pSymbol);
            type = null;
            if (this.m_anonymousDelegateEmitter == null)
            {
                this.m_anonymousDelegateEmitter = new AnonymousDelegateEmitter(this, this.DynamicModuleBuilder);
            }
            if (!this.m_anonymousDelegateEmitter.TryGetType(rootPtr, ref type))
            {
                if (this.m_anonymousDelegateEmitter == null)
                {
                    this.m_anonymousDelegateEmitter = new AnonymousDelegateEmitter(this, this.DynamicModuleBuilder);
                }
                type = this.m_anonymousDelegateEmitter.EmitType(rootPtr);
            }
            if (((byte) (*(((byte*) pSymbol)) == 0x29)) != 0)
            {
                return this.MakeConcrete(type, null, (BCSYM_GenericTypeBinding*) pSymbol, null);
            }
            return type;
        }

        private unsafe Type GetAnonymousType(BCSYM_GenericTypeBinding* pSymbol)
        {
            Type type = null;
            BCSYM_NamedRoot* rootPtr = BCSYM.PNamedRoot((BCSYM modopt(IsConst)* modopt(IsConst) modopt(IsConst)) pSymbol);
            type = null;
            if (this.m_anonymousTypeEmitter == null)
            {
                this.m_anonymousTypeEmitter = new AnonymousTypeEmitter(this, this.DynamicModuleBuilder);
            }
            if (!this.m_anonymousTypeEmitter.TryGetType(rootPtr, ref type))
            {
                if (this.m_anonymousTypeEmitter == null)
                {
                    this.m_anonymousTypeEmitter = new AnonymousTypeEmitter(this, this.DynamicModuleBuilder);
                }
                type = this.m_anonymousTypeEmitter.EmitType(rootPtr);
            }
            return this.MakeConcrete(type, null, pSymbol, null);
        }

        private unsafe Type GetArrayType(BCSYM_ArrayType* pSymbol, BCSYM_GenericBinding* pBindingContext)
        {
            Type type2 = null;
            BCSYM* bcsymPtr = BCSYM.DigThroughNamedType(*((BCSYM* modopt(IsConst) modopt(IsConst)*) (pSymbol + 12)));
            Type type = this.GetType(bcsymPtr, pBindingContext);
            if (type == null)
            {
                return type2;
            }
            int rank = *((int*) (pSymbol + 0x10));
            if (rank == 1)
            {
                return type.MakeArrayType();
            }
            return type.MakeArrayType(rank);
        }

        private unsafe Assembly GetAssembly(BCSYM_NamedRoot* pSymbol)
        {
            Assembly assembly = null;
            bool flag = false;
            CompilerProject* projectPtr = BCSYM_NamedRoot.GetContainingProject(pSymbol);
            if (*(((byte*) (projectPtr + 0x1a))) != 0)
            {
                string assemblyString = new string(AssemblyIdentity.GetAssemblyIdentityString((AssemblyIdentity* modopt(IsConst) modopt(IsConst)) (projectPtr + 0x40)));
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                int index = 0;
                do
                {
                    if (index >= assemblies.Length)
                    {
                        if (flag)
                        {
                            return assembly;
                        }
                        if (projectPtr == *(((int*) (projectPtr + 0x38)))[12])
                        {
                            VBRuntimeException exception;
                            assembly = AppDomain.CurrentDomain.Load(assemblyString);
                            if (assembly != null)
                            {
                                return assembly;
                            }
                            VBRuntimeException.{ctor}(&exception, &??_C@_1NE@EIMGALIM@?$AAT?$AAh?$AAe?$AA?5?$AAc?$AAo?$AAn?$AAd?$AAi?$AAt?$AAi?$AAo?$AAn?$AA?5?$AAs?$AAh?$AAo?$AAu?$AAl?$AAd?$AA?5?$AAn?$AAo?$AAt?$AA?5?$AAb?$AAe?$AA?5?$AAf?$AAa?$AAl?$AAs@);
                            _CxxThrowException((void*) &exception, &_TI1?AVVBRuntimeException@@);
                        }
                        return null;
                    }
                    assembly = assemblies[index];
                    flag = assembly.FullName.Equals(assemblyString, StringComparison.OrdinalIgnoreCase);
                    index++;
                }
                while (!flag);
            }
            return assembly;
        }

        private unsafe Type GetBasicType(BCSYM_NamedRoot* pSymbol)
        {
            Type target;
            gcroot<System::Type ^> local;
            BCSYM_NamedRoot* rootPtr = BCSYM_NamedRoot.GetParent(pSymbol);
            Type nestedType = null;
            if (BCSYM.GetExternalSymbol((BCSYM* modopt(IsConst) modopt(IsConst)) pSymbol) == null)
            {
                Assembly assembly = this.GetAssembly(pSymbol);
                if (assembly == null)
                {
                    return nestedType;
                }
                if ((rootPtr != null) && BCSYM.IsType((BCSYM* modopt(IsConst) modopt(IsConst)) rootPtr))
                {
                    Type type2 = this.GetType((BCSYM*) rootPtr, null);
                    if (type2 != null)
                    {
                        string str2 = new string(*((char**) (pSymbol + 12)));
                        nestedType = type2.GetNestedType(str2);
                    }
                    return nestedType;
                }
                string name = new string(BCSYM_NamedRoot.GetQualifiedEmittedName(pSymbol, null));
                return assembly.GetType(name, false, true);
            }
            *((int*) &local) = ((IntPtr) GCHandle.Alloc(null)).ToPointer();
            try
            {
                if (ConvertCOMTypeToSystemType(BCSYM.GetExternalSymbol((BCSYM* modopt(IsConst) modopt(IsConst)) pSymbol), &local) < 0)
                {
                    try
                    {
                        if (pSymbol == null)
                        {
                            RaiseException(GetLastHResultError(), 0, 0, null);
                        }
                    }
                    fault
                    {
                        ___CxxCallUnwindDtor(gcroot<System::Type ^>.{dtor}, (void*) &local);
                    }
                    IntPtr ptr = new IntPtr(*((void**) &local));
                    ((GCHandle) ptr).Free();
                    return nestedType;
                }
                IntPtr ptr3 = new IntPtr(*((void**) &local));
                GCHandle handle3 = (GCHandle) ptr3;
                target = (Type) handle3.Target;
            }
            fault
            {
                ___CxxCallUnwindDtor(gcroot<System::Type ^>.{dtor}, (void*) &local);
            }
            IntPtr ptr2 = new IntPtr(*((void**) &local));
            ((GCHandle) ptr2).Free();
            return target;
        }

        private unsafe Type GetBoundGenericType(BCSYM_GenericTypeBinding* pSymbol, BCSYM_GenericBinding* pBindingContext)
        {
            Type nestedType;
            Type type2 = null;
            Type[] parentArguments = null;
            BCSYM_GenericTypeBinding* bindingPtr = *((BCSYM_GenericTypeBinding**) (pSymbol + 0x5c));
            BCSYM_NamedRoot* rootPtr = BCSYM.PNamedRoot((BCSYM modopt(IsConst)* modopt(IsConst) modopt(IsConst)) pSymbol);
            if (bindingPtr != null)
            {
                type2 = this.GetType((BCSYM*) bindingPtr, pBindingContext);
            }
            if (type2 != null)
            {
                string name = new string(*((char**) (rootPtr + 12)));
                nestedType = type2.GetNestedType(name);
                parentArguments = type2.GetGenericArguments();
            }
            else
            {
                nestedType = this.GetBasicType(rootPtr);
            }
            if (nestedType != null)
            {
                nestedType = this.MakeConcrete(nestedType, parentArguments, pSymbol, pBindingContext);
            }
            return nestedType;
        }

        public unsafe ConstructorInfo GetConstructor(BCSYM_Proc* pConstructorSymbol, BCSYM_GenericTypeBinding* pBindingContext)
        {
            ConstructorInfo methodFromHandle;
            gcroot<System::Reflection::ConstructorInfo ^> local;
            if (pConstructorSymbol == null)
            {
                RaiseException(GetLastHResultError(), 0, 0, null);
            }
            if (BCSYM.GetExternalSymbol((BCSYM* modopt(IsConst) modopt(IsConst)) pConstructorSymbol) == null)
            {
                BCSYM_NamedRoot* rootPtr;
                ConstructorInfo info;
                if (pBindingContext != null)
                {
                    rootPtr = (BCSYM_NamedRoot*) pBindingContext;
                }
                else
                {
                    rootPtr = BCSYM_NamedRoot.GetParent((BCSYM_NamedRoot* modopt(IsConst) modopt(IsConst)) pConstructorSymbol);
                }
                Type type = this.GetType((BCSYM*) rootPtr, null);
                Type[] parameterTypes = this.GetParameterTypes(pConstructorSymbol, (BCSYM_GenericBinding*) pBindingContext);
                try
                {
                    return type.GetConstructor(parameterTypes);
                }
                catch (AmbiguousMatchException)
                {
                    ConstructorInfo[] constructors = (!type.IsGenericType ? type : type.GetGenericTypeDefinition()).GetConstructors();
                    info = (ConstructorInfo) this.FindMethod(pConstructorSymbol, null, constructors, type);
                }
                return info;
            }
            *((int*) &local) = ((IntPtr) GCHandle.Alloc(null)).ToPointer();
            try
            {
                if (ConvertCOMToManagedObject<class System::Reflection::ConstructorInfo>(BCSYM.GetExternalSymbol((BCSYM* modopt(IsConst) modopt(IsConst)) pConstructorSymbol), &local) < 0)
                {
                    ConstructorInfo info3;
                    try
                    {
                        info3 = null;
                    }
                    fault
                    {
                        ___CxxCallUnwindDtor(gcroot<System::Reflection::ConstructorInfo ^>.{dtor}, (void*) &local);
                    }
                    IntPtr ptr = new IntPtr(*((void**) &local));
                    ((GCHandle) ptr).Free();
                    return info3;
                }
                if (pBindingContext == null)
                {
                    ConstructorInfo target;
                    try
                    {
                        IntPtr ptr3 = new IntPtr(*((void**) &local));
                        GCHandle handle4 = (GCHandle) ptr3;
                        target = (ConstructorInfo) handle4.Target;
                    }
                    fault
                    {
                        ___CxxCallUnwindDtor(gcroot<System::Reflection::ConstructorInfo ^>.{dtor}, (void*) &local);
                    }
                    IntPtr ptr2 = new IntPtr(*((void**) &local));
                    ((GCHandle) ptr2).Free();
                    return target;
                }
                RuntimeTypeHandle typeHandle = this.GetType((BCSYM*) pBindingContext, null).TypeHandle;
                IntPtr ptr5 = new IntPtr(*((void**) &local));
                GCHandle handle6 = (GCHandle) ptr5;
                methodFromHandle = (ConstructorInfo) MethodBase.GetMethodFromHandle(handle6.Target.MethodHandle, typeHandle);
            }
            fault
            {
                ___CxxCallUnwindDtor(gcroot<System::Reflection::ConstructorInfo ^>.{dtor}, (void*) &local);
            }
            IntPtr ptr4 = new IntPtr(*((void**) &local));
            ((GCHandle) ptr4).Free();
            return methodFromHandle;
        }

        public unsafe FieldInfo GetField(BCSYM_Variable* pFieldSymbol, BCSYM_GenericTypeBinding* pBindingContext)
        {
            BCSYM_NamedRoot* rootPtr;
            if (pFieldSymbol == null)
            {
                RaiseException(GetLastHResultError(), 0, 0, null);
            }
            if (pBindingContext != null)
            {
                rootPtr = (BCSYM_NamedRoot*) pBindingContext;
            }
            else
            {
                rootPtr = BCSYM_NamedRoot.GetParent((BCSYM_NamedRoot* modopt(IsConst) modopt(IsConst)) pFieldSymbol);
            }
            string name = new string(*((char**) (pFieldSymbol + 12)));
            BindingFlags bindingAttr = (((byte) (*(((byte*) (pFieldSymbol + 1))) & 8)) != 0) ? (BindingFlags.Public | BindingFlags.Static) : (BindingFlags.Public | BindingFlags.Instance);
            return this.GetType((BCSYM*) rootPtr, null).GetField(name, bindingAttr);
        }

        private unsafe Type GetGenericParamType(BCSYM_GenericParam* pSymbol, BCSYM_GenericBinding* pBindingContext)
        {
            Type type = null;
            if (pBindingContext != null)
            {
                BCSYM* bcsymPtr = BCSYM_GenericBinding.GetCorrespondingArgument(pBindingContext, pSymbol);
                return this.GetType(bcsymPtr, pBindingContext);
            }
            if (BCSYM_NamedRoot.GetParent((BCSYM_NamedRoot* modopt(IsConst) modopt(IsConst)) pSymbol) != null)
            {
                type = null;
                if (BCSYM.IsAnonymousType(BCSYM_NamedRoot.GetParent((BCSYM_NamedRoot* modopt(IsConst) modopt(IsConst)) pSymbol)))
                {
                    this.DynamicAnonymousTypeEmitter.TryGetType(BCSYM.PNamedRoot(BCSYM_NamedRoot.GetParent((BCSYM_NamedRoot* modopt(IsConst) modopt(IsConst)) pSymbol)), ref type);
                }
                else if (BCSYM.IsAnonymousDelegate(BCSYM_NamedRoot.GetParent((BCSYM_NamedRoot* modopt(IsConst) modopt(IsConst)) pSymbol)))
                {
                    this.DynamicAnonymousDelegateEmitter.TryGetType(BCSYM.PNamedRoot(BCSYM_NamedRoot.GetParent((BCSYM_NamedRoot* modopt(IsConst) modopt(IsConst)) pSymbol)), ref type);
                }
                if (type != null)
                {
                    Type[] genericArguments = type.GetGenericArguments();
                    uint index = *((uint*) (pSymbol + 0x58));
                    if (genericArguments.Length > index)
                    {
                        return genericArguments[index];
                    }
                }
            }
            return null;
        }

        public unsafe MethodInfo GetMethod(BCSYM_Proc* pMethodSymbol, BCSYM_GenericBinding* pBindingContext)
        {
            IntPtr ptr4;
            gcroot<System::Reflection::MethodInfo ^> local;
            if (pMethodSymbol == null)
            {
                RaiseException(GetLastHResultError(), 0, 0, null);
            }
            MethodInfo methodFromHandle = null;
            if (BCSYM.GetExternalSymbol((BCSYM* modopt(IsConst) modopt(IsConst)) pMethodSymbol) == null)
            {
                BCSYM* bcsymPtr;
                if (pBindingContext != null)
                {
                    if (((byte) (*(((byte*) pBindingContext)) == 0x29)) != 0)
                    {
                        bcsymPtr = (BCSYM*) pBindingContext;
                    }
                    else
                    {
                        BCSYM_GenericTypeBinding* bindingPtr = *((BCSYM_GenericTypeBinding**) (pBindingContext + 0x5c));
                        if (bindingPtr != null)
                        {
                            bcsymPtr = (BCSYM*) bindingPtr;
                        }
                        else
                        {
                            bcsymPtr = BCSYM_NamedRoot.GetParent((BCSYM_NamedRoot* modopt(IsConst) modopt(IsConst)) pMethodSymbol);
                        }
                    }
                }
                else
                {
                    bcsymPtr = BCSYM_NamedRoot.GetParent((BCSYM_NamedRoot* modopt(IsConst) modopt(IsConst)) pMethodSymbol);
                }
                Type type = this.GetType(bcsymPtr, null);
                string name = new string(*((char**) (pMethodSymbol + 12)));
                BindingFlags bindingAttr = (((byte) (*(((byte*) (pMethodSymbol + 1))) & 8)) != 0) ? (BindingFlags.Public | BindingFlags.Static) : (BindingFlags.Public | BindingFlags.Instance);
                if (((*(((int*) (pMethodSymbol + 0x70))) != 0) ? ((byte) 1) : ((byte) 0)) == 0)
                {
                    Type[] parameterTypes = this.GetParameterTypes(pMethodSymbol, pBindingContext);
                    try
                    {
                        methodFromHandle = type.GetMethod(name, bindingAttr, null, parameterTypes, null);
                    }
                    catch (AmbiguousMatchException)
                    {
                    }
                }
                if (methodFromHandle != null)
                {
                    return methodFromHandle;
                }
                MethodInfo[] methods = (!type.IsGenericType ? type : type.GetGenericTypeDefinition()).GetMethods(bindingAttr);
                methodFromHandle = (MethodInfo) this.FindMethod(pMethodSymbol, name, methods, type);
                int num2 = BCSYM.GetGenericParamCount((BCSYM* modopt(IsConst) modopt(IsConst)) pMethodSymbol);
                if (num2 <= 0)
                {
                    return methodFromHandle;
                }
                Type[] typeArguments = new Type[num2];
                BCSYM_GenericParam* paramPtr2 = BCSYM_Proc.GetFirstGenericParam(pMethodSymbol);
                int index = 0;
                if (0 < num2)
                {
                    do
                    {
                        typeArguments[index] = this.GetType((BCSYM*) paramPtr2, pBindingContext);
                        paramPtr2 = *((BCSYM_GenericParam**) (paramPtr2 + 80));
                        index++;
                    }
                    while (index < num2);
                }
                return methodFromHandle.MakeGenericMethod(typeArguments);
            }
            *((int*) &local) = ((IntPtr) GCHandle.Alloc(null)).ToPointer();
            try
            {
                BCSYM* bcsymPtr2;
                Type[] typeArray2;
                if (ConvertCOMToManagedObject<class System::Reflection::MethodInfo>(BCSYM.GetExternalSymbol((BCSYM* modopt(IsConst) modopt(IsConst)) pMethodSymbol), &local) < 0)
                {
                    MethodInfo info2;
                    try
                    {
                        info2 = null;
                    }
                    fault
                    {
                        ___CxxCallUnwindDtor(gcroot<System::Reflection::MethodInfo ^>.{dtor}, (void*) &local);
                    }
                    IntPtr ptr = new IntPtr(*((void**) &local));
                    ((GCHandle) ptr).Free();
                    return info2;
                }
                if (pBindingContext == null)
                {
                    MethodInfo target;
                    try
                    {
                        IntPtr ptr3 = new IntPtr(*((void**) &local));
                        GCHandle handle4 = (GCHandle) ptr3;
                        target = (MethodInfo) handle4.Target;
                    }
                    fault
                    {
                        ___CxxCallUnwindDtor(gcroot<System::Reflection::MethodInfo ^>.{dtor}, (void*) &local);
                    }
                    IntPtr ptr2 = new IntPtr(*((void**) &local));
                    ((GCHandle) ptr2).Free();
                    return target;
                }
                bool flag = false;
                if (((byte) (*(((byte*) pBindingContext)) == 0x29)) != 0)
                {
                    bcsymPtr2 = (BCSYM*) pBindingContext;
                }
                else
                {
                    flag = true;
                    BCSYM_GenericTypeBinding* bindingPtr2 = *((BCSYM_GenericTypeBinding**) (pBindingContext + 0x5c));
                    if (bindingPtr2 != null)
                    {
                        bcsymPtr2 = (BCSYM*) bindingPtr2;
                    }
                    else
                    {
                        IntPtr ptr6 = new IntPtr(*((void**) &local));
                        GCHandle handle7 = (GCHandle) ptr6;
                        methodFromHandle = (MethodInfo) handle7.Target;
                        goto Label_00D9;
                    }
                }
                if (bcsymPtr2 != null)
                {
                    RuntimeTypeHandle typeHandle = this.GetType(bcsymPtr2, null).TypeHandle;
                    IntPtr ptr5 = new IntPtr(*((void**) &local));
                    GCHandle handle6 = (GCHandle) ptr5;
                    methodFromHandle = (MethodInfo) MethodBase.GetMethodFromHandle(handle6.Target.MethodHandle, typeHandle);
                }
                if (!flag)
                {
                    goto Label_0128;
                }
            Label_00D9:
                typeArray2 = new Type[BCSYM.GetGenericParamCount((BCSYM* modopt(IsConst) modopt(IsConst)) pMethodSymbol)];
                BCSYM_GenericParam* paramPtr = BCSYM_Proc.GetFirstGenericParam(pMethodSymbol);
                for (int i = 0; paramPtr != null; i++)
                {
                    typeArray2[i] = this.GetType((BCSYM*) paramPtr, pBindingContext);
                    paramPtr = *((BCSYM_GenericParam**) (paramPtr + 80));
                }
                methodFromHandle = methodFromHandle.MakeGenericMethod(typeArray2);
            }
            fault
            {
                ___CxxCallUnwindDtor(gcroot<System::Reflection::MethodInfo ^>.{dtor}, (void*) &local);
            }
        Label_0128:
            ptr4 = new IntPtr(*((void**) &local));
            ((GCHandle) ptr4).Free();
            return methodFromHandle;
        }

        private unsafe Type[] GetParameterTypes(BCSYM_Proc* pMethodSymbol, BCSYM_GenericBinding* pBindingContext)
        {
            BCSYM* bcsymPtr;
            Type type2;
            int num3 = BCSYM_Proc.GetParameterCount(pMethodSymbol);
            Type[] typeArray = new Type[num3];
            BCSYM_Param* paramPtr = *((BCSYM_Param**) (pMethodSymbol + 0x54));
            int index = 0;
            if (0 >= num3)
            {
                return typeArray;
            }
        Label_0022:
            bcsymPtr = BCSYM.DigThroughNamedType(*((BCSYM* modopt(IsConst) modopt(IsConst)*) (paramPtr + 0x10)));
            if (bcsymPtr == null)
            {
                RaiseException(GetLastHResultError(), 0, 0, null);
            }
            Type anonymousType = null;
            if (BCSYM.IsAnonymousType(bcsymPtr))
            {
                anonymousType = this.GetAnonymousType((BCSYM_GenericTypeBinding*) bcsymPtr);
            }
            else if (BCSYM.IsAnonymousDelegate(bcsymPtr))
            {
                anonymousType = this.GetAnonymousDelegate(bcsymPtr);
            }
            else
            {
                if ((((((byte) ((*(((byte*) bcsymPtr)) * 3)[&?s_rgBilkindInfo@BCSYM@@1QBUBilkindInfo@@B] & 8)) != 0) && (BCSYM_Container.GetSourceFile(BCSYM.PContainer((BCSYM modopt(IsConst)* modopt(IsConst) modopt(IsConst)) bcsymPtr)) != null)) && (((BCSYM_Container.GetSourceFile(BCSYM.PContainer((BCSYM modopt(IsConst)* modopt(IsConst) modopt(IsConst)) bcsymPtr))[80] != 0) ? ((byte) 1) : ((byte) 0)) != 0)) && (*(BCSYM_Container.GetSourceFile(BCSYM.PContainer((BCSYM modopt(IsConst)* modopt(IsConst) modopt(IsConst)) bcsymPtr))[80]) == 1))
                {
                    type2 = typeof(InternalXmlHelper);
                    goto Label_0184;
                }
                byte num = *((byte*) bcsymPtr);
                int num4 = num * 3;
                if (((byte) (num4[&?s_rgBilkindInfo@BCSYM@@1QBUBilkindInfo@@B] & 4)) != 0)
                {
                    if (((byte) (num == 0x29)) != 0)
                    {
                        anonymousType = this.GetBoundGenericType((BCSYM_GenericTypeBinding*) bcsymPtr, pBindingContext);
                    }
                    else if (((byte) (num == 0x24)) != 0)
                    {
                        anonymousType = this.GetGenericParamType((BCSYM_GenericParam*) bcsymPtr, pBindingContext);
                    }
                    else
                    {
                        anonymousType = this.GetBasicType(BCSYM.PNamedRoot((BCSYM modopt(IsConst)* modopt(IsConst) modopt(IsConst)) bcsymPtr));
                    }
                }
                else if (((byte) (num4[&?s_rgBilkindInfo@BCSYM@@1QBUBilkindInfo@@B + 2] & 2)) != 0)
                {
                    anonymousType = this.GetArrayType((BCSYM_ArrayType*) bcsymPtr, pBindingContext);
                }
                else if (((byte) (num == 2)) != 0)
                {
                    BCSYM* pSymbol = BCSYM.DigThroughNamedType(*((BCSYM* modopt(IsConst) modopt(IsConst)*) (bcsymPtr + 12)));
                    anonymousType = this.GetType(pSymbol, pBindingContext);
                    if (anonymousType != null)
                    {
                        anonymousType = anonymousType.MakeByRefType();
                    }
                }
                else
                {
                    if (((byte) (num == 1)) != 0)
                    {
                        type2 = typeof(void);
                        goto Label_0184;
                    }
                    RaiseException(GetLastHResultError(), 0, 0, null);
                }
            }
            type2 = anonymousType;
        Label_0184:
            typeArray[index] = type2;
            paramPtr = *((BCSYM_Param**) (paramPtr + 8));
            index++;
            if (index < num3)
            {
                goto Label_0022;
            }
            return typeArray;
        }

        public unsafe Type GetType(BCSYM* pSymbol)
        {
            return this.GetType(pSymbol, null);
        }

        private unsafe Type GetType(BCSYM* pSymbol, BCSYM_GenericBinding* pBindingContext)
        {
            if (pSymbol == null)
            {
                RaiseException(GetLastHResultError(), 0, 0, null);
            }
            Type type = null;
            if (BCSYM.IsAnonymousType(pSymbol))
            {
                return this.GetAnonymousType((BCSYM_GenericTypeBinding*) pSymbol);
            }
            if (BCSYM.IsAnonymousDelegate(pSymbol))
            {
                return this.GetAnonymousDelegate(pSymbol);
            }
            if ((((((byte) ((*(((byte*) pSymbol)) * 3)[&?s_rgBilkindInfo@BCSYM@@1QBUBilkindInfo@@B] & 8)) != 0) && (BCSYM_Container.GetSourceFile(BCSYM.PContainer((BCSYM modopt(IsConst)* modopt(IsConst) modopt(IsConst)) pSymbol)) != null)) && (((BCSYM_Container.GetSourceFile(BCSYM.PContainer((BCSYM modopt(IsConst)* modopt(IsConst) modopt(IsConst)) pSymbol))[80] != 0) ? ((byte) 1) : ((byte) 0)) != 0)) && (*(BCSYM_Container.GetSourceFile(BCSYM.PContainer((BCSYM modopt(IsConst)* modopt(IsConst) modopt(IsConst)) pSymbol))[80]) == 1))
            {
                return typeof(InternalXmlHelper);
            }
            byte num = *((byte*) pSymbol);
            int num2 = num * 3;
            if (((byte) (num2[&?s_rgBilkindInfo@BCSYM@@1QBUBilkindInfo@@B] & 4)) != 0)
            {
                if (((byte) (num == 0x29)) != 0)
                {
                    return this.GetBoundGenericType((BCSYM_GenericTypeBinding*) pSymbol, pBindingContext);
                }
                if (((byte) (num == 0x24)) != 0)
                {
                    return this.GetGenericParamType((BCSYM_GenericParam*) pSymbol, pBindingContext);
                }
                return this.GetBasicType(BCSYM.PNamedRoot((BCSYM modopt(IsConst)* modopt(IsConst) modopt(IsConst)) pSymbol));
            }
            if (((byte) (num2[&?s_rgBilkindInfo@BCSYM@@1QBUBilkindInfo@@B + 2] & 2)) != 0)
            {
                return this.GetArrayType((BCSYM_ArrayType*) pSymbol, pBindingContext);
            }
            if (((byte) (num == 2)) != 0)
            {
                BCSYM* bcsymPtr = BCSYM.DigThroughNamedType(*((BCSYM* modopt(IsConst) modopt(IsConst)*) (pSymbol + 12)));
                type = this.GetType(bcsymPtr, pBindingContext);
                if (type != null)
                {
                    type = type.MakeByRefType();
                }
                return type;
            }
            if (((byte) (num == 1)) != 0)
            {
                return typeof(void);
            }
            RaiseException(GetLastHResultError(), 0, 0, null);
            return type;
        }

        private unsafe Type MakeConcrete(Type typeDef, Type[] parentArguments, BCSYM_GenericTypeBinding* pSymbol, BCSYM_GenericBinding* pBindingContext)
        {
            Type[] genericArguments = typeDef.GetGenericArguments();
            int length = 0;
            if (parentArguments != null)
            {
                length = parentArguments.Length;
            }
            int index = 0;
            if (0 < length)
            {
                do
                {
                    genericArguments[index] = parentArguments[index];
                    index++;
                }
                while (index < length);
            }
            int num3 = length;
            if (length < genericArguments.Length)
            {
                BCSYM_GenericTypeBinding* bindingPtr = pSymbol + 0x54;
                int num4 = 0;
                do
                {
                    BCSYM** bcsymPtr2 = *((BCSYM***) bindingPtr);
                    BCSYM* bcsymPtr = BCSYM.DigThroughNamedType(*((BCSYM* modopt(IsConst) modopt(IsConst)*) (num4 + bcsymPtr2)));
                    Type type = this.GetType(bcsymPtr, pBindingContext);
                    if (type != null)
                    {
                        genericArguments[num3] = type;
                    }
                    num3++;
                    num4 += 4;
                }
                while (num3 < genericArguments.Length);
            }
            return typeDef.MakeGenericType(genericArguments);
        }

        private AnonymousDelegateEmitter DynamicAnonymousDelegateEmitter
        {
            get
            {
                if (this.m_anonymousDelegateEmitter == null)
                {
                    this.m_anonymousDelegateEmitter = new AnonymousDelegateEmitter(this, this.DynamicModuleBuilder);
                }
                return this.m_anonymousDelegateEmitter;
            }
        }

        private AnonymousTypeEmitter DynamicAnonymousTypeEmitter
        {
            get
            {
                if (this.m_anonymousTypeEmitter == null)
                {
                    this.m_anonymousTypeEmitter = new AnonymousTypeEmitter(this, this.DynamicModuleBuilder);
                }
                return this.m_anonymousTypeEmitter;
            }
        }

        private AssemblyBuilder DynamicAssemblyBuilder
        {
            get
            {
                if (this.m_assemblyBuilder == null)
                {
                    AssemblyName name = new AssemblyName("$HostedHelperAssembly$");
                    this.m_assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndCollect);
                }
                return this.m_assemblyBuilder;
            }
        }

        private ModuleBuilder DynamicModuleBuilder
        {
            get
            {
                if (this.m_moduleBuilder == null)
                {
                    this.m_moduleBuilder = this.DynamicAssemblyBuilder.DefineDynamicModule("$HostedHelperAssembly$");
                }
                return this.m_moduleBuilder;
            }
        }
    }
}

