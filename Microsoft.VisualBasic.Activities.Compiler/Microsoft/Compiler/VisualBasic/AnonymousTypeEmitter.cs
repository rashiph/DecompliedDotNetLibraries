namespace Microsoft.Compiler.VisualBasic
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal class AnonymousTypeEmitter : TypeEmitter
    {
        private Dictionary<string, Type> m_types;
        private static readonly Type ObjectType = typeof(object);
        private static readonly Type StringBuilderType = typeof(StringBuilder);
        private static readonly Type StringType = typeof(string);

        public AnonymousTypeEmitter(SymbolMap symbolMap, ModuleBuilder moduleBuilder) : base(symbolMap, moduleBuilder)
        {
            this.m_types = new Dictionary<string, Type>();
        }

        private void EmitCtor(PropertyAndField[] properties, ConstructorBuilder method)
        {
            ILGenerator iLGenerator = method.GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Call, ObjectType.GetConstructor(Type.EmptyTypes));
            int index = 0;
            if (0 < properties.Length)
            {
                do
                {
                    iLGenerator.Emit(OpCodes.Ldarg_0);
                    int arg = index + 1;
                    iLGenerator.Emit(OpCodes.Ldarg, arg);
                    iLGenerator.Emit(OpCodes.Stfld, properties[index].Field);
                    index = arg;
                }
                while (index < properties.Length);
            }
            iLGenerator.Emit(OpCodes.Ret);
        }

        private void EmitEqualsObj(TypeBuilder typeBuilder, MethodBuilder method, MethodBuilder typedEqualsMethoed)
        {
            ILGenerator iLGenerator = method.GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldarg_1);
            iLGenerator.Emit(OpCodes.Isinst, typeBuilder);
            iLGenerator.EmitCall(OpCodes.Call, typedEqualsMethoed, null);
            iLGenerator.Emit(OpCodes.Ret);
        }

        private void EmitEqualsTyped(PropertyAndField[] properties, MethodBuilder method)
        {
            MethodInfo methodInfo = ObjectType.GetMethod("Equals", BindingFlags.Public | BindingFlags.Instance);
            ILGenerator iLGenerator = method.GetILGenerator();
            Label label4 = iLGenerator.DefineLabel();
            iLGenerator.Emit(OpCodes.Ldarg_1);
            iLGenerator.Emit(OpCodes.Brtrue_S, label4);
            iLGenerator.Emit(OpCodes.Ldc_I4_0);
            iLGenerator.Emit(OpCodes.Ret);
            iLGenerator.MarkLabel(label4);
            int index = 0;
            if (0 < properties.Length)
            {
                do
                {
                    PropertyAndField field = properties[index];
                    FieldInfo info = field.Field;
                    if (!field.Property.CanWrite)
                    {
                        Label label3 = iLGenerator.DefineLabel();
                        iLGenerator.Emit(OpCodes.Ldarg_0);
                        iLGenerator.Emit(OpCodes.Ldfld, info);
                        iLGenerator.Emit(OpCodes.Box, info.FieldType);
                        iLGenerator.Emit(OpCodes.Brtrue_S, label3);
                        Label label2 = iLGenerator.DefineLabel();
                        iLGenerator.Emit(OpCodes.Ldarg_1);
                        iLGenerator.Emit(OpCodes.Ldfld, info);
                        iLGenerator.Emit(OpCodes.Box, info.FieldType);
                        iLGenerator.Emit(OpCodes.Brfalse_S, label2);
                        iLGenerator.Emit(OpCodes.Ldc_I4_0);
                        iLGenerator.Emit(OpCodes.Ret);
                        iLGenerator.MarkLabel(label3);
                        iLGenerator.Emit(OpCodes.Ldarg_1);
                        iLGenerator.Emit(OpCodes.Ldfld, info);
                        iLGenerator.Emit(OpCodes.Box, info.FieldType);
                        iLGenerator.Emit(OpCodes.Brtrue_S, label2);
                        iLGenerator.Emit(OpCodes.Ldc_I4_0);
                        iLGenerator.Emit(OpCodes.Ret);
                        iLGenerator.MarkLabel(label2);
                        Label label = iLGenerator.DefineLabel();
                        iLGenerator.Emit(OpCodes.Ldarg_0);
                        iLGenerator.Emit(OpCodes.Ldfld, info);
                        iLGenerator.Emit(OpCodes.Box, info.FieldType);
                        iLGenerator.Emit(OpCodes.Brfalse_S, label);
                        iLGenerator.Emit(OpCodes.Ldarg_1);
                        iLGenerator.Emit(OpCodes.Ldfld, info);
                        iLGenerator.Emit(OpCodes.Box, info.FieldType);
                        iLGenerator.Emit(OpCodes.Brfalse_S, label);
                        iLGenerator.Emit(OpCodes.Ldarg_0);
                        iLGenerator.Emit(OpCodes.Ldfld, info);
                        iLGenerator.Emit(OpCodes.Box, info.FieldType);
                        iLGenerator.Emit(OpCodes.Ldarg_1);
                        iLGenerator.Emit(OpCodes.Ldfld, info);
                        iLGenerator.Emit(OpCodes.Box, info.FieldType);
                        iLGenerator.EmitCall(OpCodes.Callvirt, methodInfo, null);
                        iLGenerator.Emit(OpCodes.Brtrue_S, label);
                        iLGenerator.Emit(OpCodes.Ldc_I4_0);
                        iLGenerator.Emit(OpCodes.Ret);
                        iLGenerator.MarkLabel(label);
                    }
                    index++;
                }
                while (index < properties.Length);
            }
            iLGenerator.Emit(OpCodes.Ldc_I4_1);
            iLGenerator.Emit(OpCodes.Ret);
        }

        private void EmitGetHashCode(PropertyAndField[] properties, MethodBuilder method)
        {
            MethodInfo methodInfo = ObjectType.GetMethod("GetHashCode", Type.EmptyTypes);
            ILGenerator iLGenerator = method.GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldc_I4, method.GetHashCode());
            int index = 0;
            if (0 < properties.Length)
            {
                do
                {
                    PropertyAndField field = properties[index];
                    FieldBuilder builder = field.Field;
                    if (!field.Property.CanWrite)
                    {
                        iLGenerator.Emit(OpCodes.Ldc_I4, 0x1f);
                        iLGenerator.Emit(OpCodes.Mul);
                        Label label = iLGenerator.DefineLabel();
                        iLGenerator.Emit(OpCodes.Ldarg_0);
                        iLGenerator.Emit(OpCodes.Ldfld, builder);
                        iLGenerator.Emit(OpCodes.Box, builder.FieldType);
                        iLGenerator.Emit(OpCodes.Brfalse_S, label);
                        iLGenerator.Emit(OpCodes.Ldarg_0);
                        iLGenerator.Emit(OpCodes.Ldflda, builder);
                        iLGenerator.Emit(OpCodes.Constrained, builder.FieldType);
                        iLGenerator.EmitCall(OpCodes.Callvirt, methodInfo, null);
                        iLGenerator.Emit(OpCodes.Add);
                        iLGenerator.MarkLabel(label);
                    }
                    index++;
                }
                while (index < properties.Length);
            }
            iLGenerator.Emit(OpCodes.Ret);
        }

        private void EmitProperty(PropertyAndField prop)
        {
            ILGenerator iLGenerator = ((MethodBuilder) prop.Property.GetGetMethod(false)).GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldfld, prop.Field);
            iLGenerator.Emit(OpCodes.Ret);
            MethodBuilder setMethod = (MethodBuilder) prop.Property.GetSetMethod(false);
            if (setMethod != null)
            {
                iLGenerator = setMethod.GetILGenerator();
                iLGenerator.Emit(OpCodes.Ldarg_0);
                iLGenerator.Emit(OpCodes.Ldarg_1);
                iLGenerator.Emit(OpCodes.Stfld, prop.Field);
                iLGenerator.Emit(OpCodes.Ret);
            }
        }

        private void EmitToString(PropertyAndField[] properties, MethodBuilder method)
        {
            ILGenerator iLGenerator = method.GetILGenerator();
            Type[] types = new Type[] { StringType };
            MethodInfo methodInfo = StringBuilderType.GetMethod("Append", types);
            Type[] typeArray = new Type[] { StringType, ObjectType, ObjectType };
            MethodInfo info = StringBuilderType.GetMethod("AppendFormat", typeArray);
            iLGenerator.Emit(OpCodes.Newobj, StringBuilderType.GetConstructor(Type.EmptyTypes));
            iLGenerator.Emit(OpCodes.Ldstr, "{ ");
            iLGenerator.EmitCall(OpCodes.Callvirt, methodInfo, null);
            int index = 0;
            if (0 < (properties.Length - 1))
            {
                do
                {
                    PropertyAndField field = properties[index];
                    PropertyAndField field3 = field;
                    FieldBuilder builder2 = field.Field;
                    iLGenerator.Emit(OpCodes.Ldstr, "{0} = {1}, ");
                    iLGenerator.Emit(OpCodes.Ldstr, field3.Property.Name);
                    iLGenerator.Emit(OpCodes.Ldarg_0);
                    iLGenerator.Emit(OpCodes.Ldfld, builder2);
                    iLGenerator.Emit(OpCodes.Box, builder2.FieldType);
                    iLGenerator.EmitCall(OpCodes.Callvirt, info, null);
                    index++;
                }
                while (index < (properties.Length - 1));
            }
            PropertyAndField field2 = properties[properties.Length - 1];
            FieldBuilder builder = properties[index].Field;
            iLGenerator.Emit(OpCodes.Ldstr, "{0} = {1} ");
            iLGenerator.Emit(OpCodes.Ldstr, field2.Property.Name);
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldfld, builder);
            iLGenerator.Emit(OpCodes.Box, builder.FieldType);
            iLGenerator.EmitCall(OpCodes.Callvirt, info, null);
            iLGenerator.Emit(OpCodes.Ldstr, "}");
            iLGenerator.EmitCall(OpCodes.Callvirt, methodInfo, null);
            iLGenerator.EmitCall(OpCodes.Callvirt, StringBuilderType.GetMethod("ToString", Type.EmptyTypes), null);
            iLGenerator.Emit(OpCodes.Ret);
        }

        public unsafe Type EmitType(BCSYM_NamedRoot* pSymbol)
        {
            TypeBuilder builder = this.DefineType(BCSYM.PNamedRoot((BCSYM modopt(IsConst)* modopt(IsConst) modopt(IsConst)) pSymbol));
            this.m_types.Add(builder.Name, builder);
            bool hasKey = false;
            int num2 = BCSYM.GetGenericParamCount((BCSYM* modopt(IsConst) modopt(IsConst)) pSymbol);
            PropertyAndField[] properties = new PropertyAndField[num2];
            BCSYM_GenericParam* pParam = BCSYM.GetFirstGenericParam((BCSYM* modopt(IsConst) modopt(IsConst)) pSymbol);
            int index = 0;
            if (0 < num2)
            {
                do
                {
                    PropertyAndField prop = this.FindPropertyAndField(pSymbol, pParam, builder, &hasKey);
                    properties[index] = prop;
                    this.EmitProperty(prop);
                    pParam = *((BCSYM_GenericParam**) (pParam + 80));
                    index++;
                }
                while (index < num2);
            }
            ConstructorBuilder ctorBuilder = null;
            MethodBuilder toStringBuilder = null;
            MethodBuilder equalsObjBuilder = null;
            MethodBuilder equalsTypeBuilder = null;
            MethodBuilder getHashBuilder = null;
            this.FindMethods(pSymbol, builder, ref ctorBuilder, ref toStringBuilder, ref equalsObjBuilder, ref equalsTypeBuilder, ref getHashBuilder);
            this.EmitCtor(properties, ctorBuilder);
            this.EmitToString(properties, toStringBuilder);
            if (hasKey)
            {
                Type interfaceType = typeof(IEquatable<>).MakeGenericType(new Type[] { builder });
                builder.AddInterfaceImplementation(interfaceType);
                this.EmitEqualsTyped(properties, equalsTypeBuilder);
                this.EmitEqualsObj(builder, equalsObjBuilder, equalsTypeBuilder);
                this.EmitGetHashCode(properties, getHashBuilder);
            }
            Type type = builder.CreateType();
            this.m_types[builder.Name] = type;
            return type;
        }

        public unsafe void FindMethods(BCSYM_NamedRoot* pAnonymousType, TypeBuilder typeBuilder, ref ConstructorBuilder ctorBuilder, ref MethodBuilder toStringBuilder, ref MethodBuilder equalsObjBuilder, ref MethodBuilder equalsTypeBuilder, ref MethodBuilder getHashBuilder)
        {
            BCITER_CHILD bciter_child;
            Compiler* compilerPtr = BCSYM_NamedRoot.GetCompiler(pAnonymousType);
            ushort* numPtr4 = Compiler.AddString(compilerPtr, &??_C@_1BC@GFPIDKBJ@?$AAT?$AAo?$AAS?$AAt?$AAr?$AAi?$AAn?$AAg?$AA?$AA@);
            ushort* numPtr3 = Compiler.AddString(compilerPtr, &??_C@_1O@FOCMPMJF@?$AAE?$AAq?$AAu?$AAa?$AAl?$AAs?$AA?$AA@);
            ushort* numPtr2 = Compiler.AddString(compilerPtr, &??_C@_1BI@KCPDAEGD@?$AAG?$AAe?$AAt?$AAH?$AAa?$AAs?$AAh?$AAC?$AAo?$AAd?$AAe?$AA?$AA@);
            ushort* numPtr = *((ushort**) (*(((int*) (compilerPtr + 12))) + 900));
            BCITER_CHILD.Init(&bciter_child, pAnonymousType, false, false, false);
            for (BCSYM_NamedRoot* rootPtr = BCITER_CHILD.GetNext(&bciter_child); rootPtr != null; rootPtr = BCITER_CHILD.GetNext(&bciter_child))
            {
                if (BCSYM.IsProc((BCSYM* modopt(IsConst) modopt(IsConst)) rootPtr))
                {
                    int num = *((int*) (rootPtr + 12));
                    if (StringPool.IsEqual((ushort modopt(IsConst)*) num, (ushort modopt(IsConst)*) numPtr4))
                    {
                        toStringBuilder = this.DefineMethod(typeBuilder, (BCSYM_Proc*) rootPtr);
                    }
                    else if (StringPool.IsEqual((ushort modopt(IsConst)*) num, (ushort modopt(IsConst)*) numPtr))
                    {
                        ctorBuilder = this.DefineConstructor(typeBuilder, (BCSYM_Proc*) rootPtr);
                    }
                    else if (StringPool.IsEqual((ushort modopt(IsConst)*) num, (ushort modopt(IsConst)*) numPtr2))
                    {
                        getHashBuilder = this.DefineMethod(typeBuilder, (BCSYM_Proc*) rootPtr);
                    }
                    else if (StringPool.IsEqual((ushort modopt(IsConst)*) num, (ushort modopt(IsConst)*) numPtr3))
                    {
                        if (StringPool.IsEqual(BCSYM_NamedRoot.GetEmittedName(BCSYM.PNamedRoot(BCSYM_Param.GetType(BCSYM_Proc.GetFirstParam((BCSYM_Proc modopt(IsConst)* modopt(IsConst) modopt(IsConst)) rootPtr)))), BCSYM_NamedRoot.GetEmittedName((BCSYM_NamedRoot modopt(IsConst)* modopt(IsConst) modopt(IsConst)) pAnonymousType)))
                        {
                            equalsTypeBuilder = this.DefineMethod(typeBuilder, (BCSYM_Proc*) rootPtr);
                        }
                        else
                        {
                            equalsObjBuilder = this.DefineMethod(typeBuilder, (BCSYM_Proc*) rootPtr);
                        }
                    }
                }
            }
        }

        public unsafe PropertyAndField FindPropertyAndField(BCSYM_NamedRoot* pAnonymousType, BCSYM_GenericParam* pParam, TypeBuilder typeBuilder, bool* modopt(IsImplicitlyDereferenced) hasKey)
        {
            BCITER_CHILD bciter_child;
            if (((pAnonymousType == null) || (typeBuilder == null)) || (pParam == null))
            {
                return null;
            }
            BCITER_CHILD.Init(&bciter_child, pAnonymousType, false, false, false);
            PropertyAndField field = new PropertyAndField();
            for (BCSYM_NamedRoot* rootPtr = BCITER_CHILD.GetNext(&bciter_child); rootPtr != null; rootPtr = BCITER_CHILD.GetNext(&bciter_child))
            {
                byte num = *((byte*) rootPtr);
                if (((byte) ((num * 3)[&?s_rgBilkindInfo@BCSYM@@1QBUBilkindInfo@@B] & 0x80)) != 0)
                {
                    if (BCSYM.DigThroughNamedType(*((BCSYM* modopt(IsConst) modopt(IsConst)*) (rootPtr + 80))) == pParam)
                    {
                        field.Field = this.DefineField(typeBuilder, (BCSYM_Variable*) rootPtr);
                        if (field.Property != null)
                        {
                            return field;
                        }
                    }
                }
                else if ((((byte) (num == 0x1f)) != 0) && (BCSYM.DigThroughNamedType(*((BCSYM* modopt(IsConst) modopt(IsConst)*) (rootPtr + 80))) == pParam))
                {
                    field.Property = this.DefineProperty(typeBuilder, (BCSYM_Property*) rootPtr);
                    if (((byte) (*(((byte*) (rootPtr + 140))) & 1)) != 0)
                    {
                        hasKey[0] = 1;
                    }
                    if (field.Field != null)
                    {
                        return field;
                    }
                }
            }
            return field;
        }

        [return: MarshalAs(UnmanagedType.U1)]
        public override unsafe bool TryGetType(BCSYM_NamedRoot* pSymbol, ref Type type)
        {
            string key = new string(*((char**) (pSymbol + 12)));
            return this.m_types.TryGetValue(key, out type);
        }

        private class PropertyAndField
        {
            private FieldBuilder _Field;
            private PropertyBuilder _Property;

            public FieldBuilder Field
            {
                get
                {
                    return this._Field;
                }
                set
                {
                    this._Field = value;
                }
            }

            public PropertyBuilder Property
            {
                get
                {
                    return this._Property;
                }
                set
                {
                    this._Property = value;
                }
            }
        }
    }
}

