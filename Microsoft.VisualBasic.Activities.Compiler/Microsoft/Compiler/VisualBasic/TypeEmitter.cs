namespace Microsoft.Compiler.VisualBasic
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal abstract class TypeEmitter
    {
        private ModuleBuilder m_moduleBuilder;
        private SymbolMap m_symbolMap;
        protected static Type VoidType = typeof(void);

        public TypeEmitter(SymbolMap symbolMap, ModuleBuilder moduleBuilder)
        {
            this.m_moduleBuilder = moduleBuilder;
            this.m_symbolMap = symbolMap;
        }

        protected virtual unsafe ConstructorBuilder DefineConstructor(TypeBuilder typeBuilder, BCSYM_Proc* pProc)
        {
            MethodAttributes methodAttributes = GetMethodAttributes(pProc);
            Type[] parameterTypes = this.GetParameterTypes(typeBuilder, pProc);
            return typeBuilder.DefineConstructor(methodAttributes, CallingConventions.Standard, parameterTypes);
        }

        protected virtual unsafe FieldBuilder DefineField(TypeBuilder typeBuilder, BCSYM_Variable* pField)
        {
            string fieldName = new string(*((char**) (pField + 12)));
            BCSYM* pSymbol = BCSYM.DigThroughNamedType(*((BCSYM* modopt(IsConst) modopt(IsConst)*) (pField + 80)));
            Type type = this.GetType(typeBuilder, pSymbol);
            FieldAttributes fieldAttributes = GetFieldAttributes(pField);
            return typeBuilder.DefineField(fieldName, type, fieldAttributes);
        }

        protected virtual unsafe void DefineGenericTypeParameter(TypeBuilder typeBuilder, GenericTypeParameterBuilder paramBuilder, BCSYM_GenericParam* pParam)
        {
            paramBuilder.SetGenericParameterAttributes(GetGenericParameterAttributes(pParam));
            List<Type> list = new List<Type>();
            for (BCSYM_GenericConstraint* constraintPtr = *((BCSYM_GenericConstraint**) (pParam + 0x54)); constraintPtr != null; constraintPtr = *((BCSYM_GenericConstraint**) (constraintPtr + 8)))
            {
                if (((byte) (*(((byte*) constraintPtr)) == 0x26)) != 0)
                {
                    BCSYM* pSymbol = BCSYM.DigThroughNamedType(*((BCSYM* modopt(IsConst) modopt(IsConst)*) (constraintPtr + 12)));
                    Type item = this.GetType(typeBuilder, pSymbol);
                    if (BCSYM.IsInterface(pSymbol))
                    {
                        list.Add(item);
                    }
                    else
                    {
                        paramBuilder.SetBaseTypeConstraint(item);
                    }
                }
            }
            if (list.Count > 0)
            {
                paramBuilder.SetInterfaceConstraints(list.ToArray());
            }
        }

        protected virtual unsafe MethodBuilder DefineMethod(TypeBuilder typeBuilder, BCSYM_Proc* pProc)
        {
            string name = new string(*((char**) (pProc + 12)));
            BCSYM* pSymbol = BCSYM.DigThroughNamedType(*((BCSYM* modopt(IsConst) modopt(IsConst)*) (pProc + 80)));
            Type returnType = this.GetType(typeBuilder, pSymbol);
            MethodAttributes methodAttributes = GetMethodAttributes(pProc);
            Type[] parameterTypes = this.GetParameterTypes(typeBuilder, pProc);
            return typeBuilder.DefineMethod(name, methodAttributes, returnType, parameterTypes);
        }

        protected virtual unsafe PropertyBuilder DefineProperty(TypeBuilder typeBuilder, BCSYM_Property* pProperty)
        {
            string name = new string(*((char**) (pProperty + 12)));
            BCSYM* pSymbol = BCSYM.DigThroughNamedType(*((BCSYM* modopt(IsConst) modopt(IsConst)*) (pProperty + 80)));
            Type returnType = this.GetType(typeBuilder, pSymbol);
            PropertyAttributes attributes = (PropertyAttributes) ((*(((byte*) (pProperty + 0x4a))) & 2) << 8);
            Type[] parameterTypes = this.GetParameterTypes(typeBuilder, (BCSYM_Proc*) pProperty);
            PropertyBuilder builder = typeBuilder.DefineProperty(name, attributes, returnType, parameterTypes);
            int num2 = *((int*) (pProperty + 0x80));
            if (num2 != 0)
            {
                BCSYM_Proc* pProc = (BCSYM_Proc*) num2;
                MethodBuilder mdBuilder = this.DefineMethod(typeBuilder, pProc);
                builder.SetGetMethod(mdBuilder);
            }
            int num = *((int*) (pProperty + 0x84));
            if (num != 0)
            {
                BCSYM_Proc* procPtr = (BCSYM_Proc*) num;
                MethodBuilder builder2 = this.DefineMethod(typeBuilder, procPtr);
                builder.SetSetMethod(builder2);
            }
            return builder;
        }

        protected virtual unsafe TypeBuilder DefineType(BCSYM_NamedRoot* pSymbol)
        {
            TypeBuilder builder;
            string name = new string(*((char**) (pSymbol + 12)));
            TypeAttributes typeAttributes = GetTypeAttributes(pSymbol);
            if (BCSYM.IsClass((BCSYM modopt(IsConst)* modopt(IsConst) modopt(IsConst)) pSymbol) && (BCSYM_Class.GetBaseClass(BCSYM.PClass((BCSYM modopt(IsConst)* modopt(IsConst) modopt(IsConst)) pSymbol)) != null))
            {
                Type parent = this.m_symbolMap.GetType(BCSYM_Class.GetBaseClass(BCSYM.PClass((BCSYM modopt(IsConst)* modopt(IsConst) modopt(IsConst)) pSymbol)));
                builder = this.m_moduleBuilder.DefineType(name, typeAttributes, parent);
            }
            else
            {
                builder = this.m_moduleBuilder.DefineType(name, typeAttributes);
            }
            if (BCSYM.IsGeneric((BCSYM* modopt(IsConst) modopt(IsConst)) pSymbol))
            {
                int num = BCSYM.GetGenericParamCount((BCSYM* modopt(IsConst) modopt(IsConst)) pSymbol);
                string[] names = new string[num];
                BCSYM_GenericParam* paramPtr2 = BCSYM.GetFirstGenericParam((BCSYM* modopt(IsConst) modopt(IsConst)) pSymbol);
                int index = 0;
                if (0 < num)
                {
                    do
                    {
                        ushort* numPtr = *((ushort**) (paramPtr2 + 12));
                        names[index] = new string((char*) numPtr);
                        paramPtr2 = *((BCSYM_GenericParam**) (paramPtr2 + 80));
                        index++;
                    }
                    while (index < num);
                }
                GenericTypeParameterBuilder[] builderArray = builder.DefineGenericParameters(names);
                BCSYM_GenericParam* pParam = BCSYM.GetFirstGenericParam((BCSYM* modopt(IsConst) modopt(IsConst)) pSymbol);
                int num2 = 0;
                if (0 < num)
                {
                    do
                    {
                        this.DefineGenericTypeParameter(builder, builderArray[num2], pParam);
                        pParam = *((BCSYM_GenericParam**) (pParam + 80));
                        num2++;
                    }
                    while (num2 < num);
                }
            }
            return builder;
        }

        protected static unsafe FieldAttributes GetFieldAttributes(BCSYM_Variable* pField)
        {
            FieldAttributes privateScope = FieldAttributes.PrivateScope;
            if ((*(((int*) (pField + 0x5c))) & 7) == 0)
            {
                if ((((byte) (BCSYM.GetVtype(BCSYM.DigThroughNamedType(*((BCSYM* modopt(IsConst) modopt(IsConst)*) (pField + 80)))) == 12)) == 0) && (((byte) (BCSYM.GetVtype(BCSYM.DigThroughNamedType(*((BCSYM* modopt(IsConst) modopt(IsConst)*) (pField + 80)))) == 15)) == 0))
                {
                    privateScope = FieldAttributes.Literal;
                }
                else
                {
                    privateScope = FieldAttributes.InitOnly;
                }
                privateScope |= FieldAttributes.Static;
            }
            else if (((byte) (*(((byte*) (pField + 1))) & 8)) != 0)
            {
                privateScope = FieldAttributes.Static;
            }
            if (((byte) ((*(((int*) (pField + 0x5c))) >> 6) & 1)) != 0)
            {
                privateScope |= FieldAttributes.InitOnly;
            }
            switch (BCSYM_NamedRoot.GetAccess((BCSYM_NamedRoot* modopt(IsConst) modopt(IsConst)) pField))
            {
                case 1:
                    return (privateScope | FieldAttributes.Private);

                case 2:
                    return privateScope;

                case 3:
                    return (privateScope | FieldAttributes.Family);

                case 4:
                    return (privateScope | FieldAttributes.Assembly);

                case 5:
                    return (privateScope | FieldAttributes.FamORAssem);

                case 6:
                    return (privateScope | FieldAttributes.Public);
            }
            return privateScope;
        }

        protected static unsafe GenericParameterAttributes GetGenericParameterAttributes(BCSYM_GenericParam* pParam)
        {
            GenericParameterAttributes none = GenericParameterAttributes.None;
            switch (*(((byte*) (pParam + 0x61))))
            {
                case 1:
                    none = GenericParameterAttributes.Covariant;
                    break;

                case 2:
                    none = GenericParameterAttributes.Contravariant;
                    break;
            }
            for (BCSYM_GenericConstraint* constraintPtr = *((BCSYM_GenericConstraint**) (pParam + 0x54)); constraintPtr != null; constraintPtr = *((BCSYM_GenericConstraint**) (constraintPtr + 8)))
            {
                if (BCSYM_GenericConstraint.IsNewConstraint(constraintPtr))
                {
                    none |= GenericParameterAttributes.DefaultConstructorConstraint;
                }
                else if (BCSYM_GenericConstraint.IsReferenceConstraint(constraintPtr))
                {
                    none |= GenericParameterAttributes.ReferenceTypeConstraint;
                }
                else if (BCSYM_GenericConstraint.IsValueConstraint(constraintPtr))
                {
                    none |= GenericParameterAttributes.NotNullableValueTypeConstraint;
                }
            }
            return none;
        }

        protected static unsafe MethodAttributes GetMethodAttributes(BCSYM_Proc* pProc)
        {
            byte num3;
            int num6;
            MethodAttributes privateScope = MethodAttributes.PrivateScope;
            byte num = *((byte*) (pProc + 120));
            if ((((byte) (num & 0x20)) != 0) && (((byte) (num & 0x40)) == 0))
            {
                num6 = 1;
            }
            else
            {
                num6 = 0;
            }
            if (((byte) num6) != 0)
            {
                privateScope = MethodAttributes.Abstract;
            }
            if (((byte) (*(((byte*) (pProc + 1))) & 8)) != 0)
            {
                privateScope |= MethodAttributes.Static;
            }
            if (BCSYM_Proc.IsVirtual(pProc))
            {
                privateScope |= MethodAttributes.Virtual;
                byte num5 = *((byte*) (pProc + 0x7a));
                if (((byte) (num5 & 4)) != 0)
                {
                    privateScope |= MethodAttributes.CheckAccessOnOverride;
                }
                if (((((byte) (num & 0x10)) != 0) || (((byte) (num & 0x80)) != 0)) || BCSYM_Proc.IsOverrides(pProc))
                {
                    int num4;
                    if ((((byte) (num5 & 0x40)) == 0) && (*(((int*) (pProc + 0x68))) != 0))
                    {
                        num4 = 1;
                    }
                    else
                    {
                        num4 = 0;
                    }
                    if ((((byte) num4) == 0) || (((byte) (*(((byte*) (pProc + 0x79))) & 0x40)) == 0))
                    {
                        goto Label_00A2;
                    }
                }
                privateScope |= MethodAttributes.NewSlot;
            }
        Label_00A2:
            if (((byte) (num & 0x80)) == 0)
            {
                byte num2 = *((byte*) (pProc + 0x79));
                if (((((byte) (num2 & 8)) == 0) && (((byte) (num2 & 0x10)) == 0)) || (((((byte) (num & 0x10)) != 0) || BCSYM_Proc.IsMustOverrideKeywordUsed((BCSYM_Proc modopt(IsConst)* modopt(IsConst) modopt(IsConst)) pProc)) || (((byte) (num2 & 1)) != 0)))
                {
                    goto Label_00D9;
                }
            }
            privateScope |= MethodAttributes.Final;
        Label_00D9:
            num3 = *((byte*) (pProc + 0x4a));
            if (((byte) (num3 & 2)) != 0)
            {
                privateScope |= MethodAttributes.SpecialName;
            }
            if ((((byte) (num & 8)) != 0) && (((byte) (num3 & 0x10)) == 0))
            {
                privateScope |= MethodAttributes.HideBySig;
            }
            if (((byte) (*(((byte*) pProc)) == 13)) != 0)
            {
                privateScope |= MethodAttributes.PinvokeImpl | MethodAttributes.Static;
            }
            switch (BCSYM_NamedRoot.GetAccess((BCSYM_NamedRoot* modopt(IsConst) modopt(IsConst)) pProc))
            {
                case 0:
                case 2:
                    return privateScope;

                case 1:
                    return (privateScope | MethodAttributes.Private);

                case 3:
                    return (privateScope | MethodAttributes.Family);

                case 4:
                    return (privateScope | MethodAttributes.Assembly);

                case 5:
                    return (privateScope | MethodAttributes.FamORAssem);

                case 6:
                    return (privateScope | MethodAttributes.Public);
            }
            return privateScope;
        }

        protected unsafe Type[] GetParameterTypes(TypeBuilder typeBuilder, BCSYM_Proc* pMethodSymbol)
        {
            int num2 = BCSYM_Proc.GetParameterCount(pMethodSymbol);
            Type[] typeArray = new Type[num2];
            BCSYM_Param* paramPtr = *((BCSYM_Param**) (pMethodSymbol + 0x54));
            int index = 0;
            if (0 < num2)
            {
                do
                {
                    BCSYM* pSymbol = BCSYM.DigThroughNamedType(*((BCSYM* modopt(IsConst) modopt(IsConst)*) (paramPtr + 0x10)));
                    typeArray[index] = this.GetType(typeBuilder, pSymbol);
                    paramPtr = *((BCSYM_Param**) (paramPtr + 8));
                    index++;
                }
                while (index < num2);
            }
            return typeArray;
        }

        protected static unsafe PropertyAttributes GetPropertyAttributes(BCSYM_Property* pProperty)
        {
            return (PropertyAttributes) ((*(((byte*) (pProperty + 0x4a))) & 2) << 8);
        }

        protected virtual unsafe Type GetType(TypeBuilder typeBuilder, BCSYM* pSymbol)
        {
            if (pSymbol == null)
            {
                return VoidType;
            }
            return this.m_symbolMap.GetType(pSymbol);
        }

        protected static unsafe TypeAttributes GetTypeAttributes(BCSYM_NamedRoot* pType)
        {
            TypeAttributes ansiClass = TypeAttributes.AnsiClass;
            if (WellKnownAttrVals.GetComImportData(BCSYM_NamedRoot.GetPWellKnownAttrVals(pType)))
            {
                ansiClass = TypeAttributes.Import;
            }
            if (BCSYM.IsClass((BCSYM modopt(IsConst)* modopt(IsConst) modopt(IsConst)) pType))
            {
                BCSYM_Class* classPtr = BCSYM.PClass((BCSYM modopt(IsConst)* modopt(IsConst) modopt(IsConst)) pType);
                uint num = *((uint*) (classPtr + 140));
                if (((byte) ((num >> 13) & 1)) != 0)
                {
                    ansiClass |= TypeAttributes.Sealed;
                }
                else if (((byte) ((num >> 11) & 1)) != 0)
                {
                    short num2;
                    ansiClass |= TypeAttributes.Sealed;
                    if (WellKnownAttrVals.GetStructLayoutData(BCSYM_Container.GetPWellKnownAttrVals((BCSYM_Container* modopt(IsConst) modopt(IsConst)) classPtr), &num2))
                    {
                        if (num2 != 0x18)
                        {
                            ansiClass |= num2 & 0x18;
                        }
                        else
                        {
                            ansiClass |= TypeAttributes.SequentialLayout;
                        }
                    }
                    else
                    {
                        ansiClass |= TypeAttributes.SequentialLayout;
                    }
                }
                else
                {
                    if ((((byte) ((num >> 10) & 1)) != 0) || (((byte) ((num >> 0x12) & 1)) != 0))
                    {
                        ansiClass |= TypeAttributes.Sealed;
                    }
                    if ((((byte) ((num >> 8) & 1)) == 0) || (((byte) ((num >> 9) & 1)) != 0))
                    {
                        ansiClass |= TypeAttributes.Abstract;
                    }
                }
            }
            else if (BCSYM.IsInterface((BCSYM* modopt(IsConst) modopt(IsConst)) pType))
            {
                ansiClass |= TypeAttributes.Abstract | TypeAttributes.ClassSemanticsMask;
            }
            if ((BCSYM_NamedRoot.GetContainer(pType) != null) && (((byte) ((*(BCSYM_NamedRoot.GetContainer(pType)) * 3)[&?s_rgBilkindInfo@BCSYM@@1QBUBilkindInfo@@B] & 0x10)) != 0))
            {
                switch (BCSYM_NamedRoot.GetAccess(pType))
                {
                    case 1:
                        return (ansiClass | TypeAttributes.NestedPrivate);

                    case 2:
                        return ansiClass;

                    case 3:
                        return (ansiClass | TypeAttributes.NestedFamily);

                    case 4:
                        return (ansiClass | TypeAttributes.NestedAssembly);

                    case 5:
                        return (ansiClass | TypeAttributes.NestedFamORAssem);

                    case 6:
                        return (ansiClass | TypeAttributes.NestedPublic);
                }
                return ansiClass;
            }
            switch (BCSYM_NamedRoot.GetAccess(pType))
            {
                case 1:
                case 2:
                case 4:
                    return ansiClass;

                case 3:
                    return (ansiClass | TypeAttributes.Public);

                case 5:
                    return (ansiClass | TypeAttributes.Public);

                case 6:
                    return (ansiClass | TypeAttributes.Public);
            }
            return ansiClass;
        }

        [return: MarshalAs(UnmanagedType.U1)]
        public abstract unsafe bool TryGetType(BCSYM_NamedRoot* pSymbol, ref Type type);

        protected ModuleBuilder DynamicModuleBuilder
        {
            get
            {
                return this.m_moduleBuilder;
            }
        }
    }
}

