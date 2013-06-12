namespace System.Runtime.InteropServices.TCEAdapterGen
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    internal class EventSinkHelperWriter
    {
        public static readonly string GeneratedTypeNamePostfix = "_SinkHelper";
        private Type m_EventItfType;
        private Type m_InputType;
        private ModuleBuilder m_OutputModule;

        public EventSinkHelperWriter(ModuleBuilder OutputModule, Type InputType, Type EventItfType)
        {
            this.m_InputType = InputType;
            this.m_OutputModule = OutputModule;
            this.m_EventItfType = EventItfType;
        }

        private void AddReturn(Type ReturnType, ILGenerator il, MethodBuilder Meth)
        {
            if (ReturnType != typeof(void))
            {
                if (ReturnType.IsPrimitive)
                {
                    switch (Type.GetTypeCode(ReturnType))
                    {
                        case TypeCode.Boolean:
                        case TypeCode.Char:
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                            il.Emit(OpCodes.Ldc_I4_0);
                            return;

                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                            il.Emit(OpCodes.Ldc_I4_0);
                            il.Emit(OpCodes.Conv_I8);
                            return;

                        case TypeCode.Single:
                            il.Emit(OpCodes.Ldc_R4, 0);
                            return;

                        case TypeCode.Double:
                            il.Emit(OpCodes.Ldc_R4, 0);
                            il.Emit(OpCodes.Conv_R8);
                            return;
                    }
                    if (ReturnType == typeof(IntPtr))
                    {
                        il.Emit(OpCodes.Ldc_I4_0);
                    }
                }
                else if (ReturnType.IsValueType)
                {
                    Meth.InitLocals = true;
                    LocalBuilder local = il.DeclareLocal(ReturnType);
                    il.Emit(OpCodes.Ldloc_S, local);
                }
                else
                {
                    il.Emit(OpCodes.Ldnull);
                }
            }
        }

        private void DefineBlankMethod(TypeBuilder OutputTypeBuilder, MethodInfo Method)
        {
            ParameterInfo[] parameters = Method.GetParameters();
            Type[] parameterTypes = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                parameterTypes[i] = parameters[i].ParameterType;
            }
            MethodBuilder meth = OutputTypeBuilder.DefineMethod(Method.Name, Method.Attributes & ~MethodAttributes.Abstract, Method.CallingConvention, Method.ReturnType, parameterTypes);
            ILGenerator iLGenerator = meth.GetILGenerator();
            this.AddReturn(Method.ReturnType, iLGenerator, meth);
            iLGenerator.Emit(OpCodes.Ret);
        }

        private void DefineConstructor(TypeBuilder OutputTypeBuilder, FieldBuilder fbCookie, FieldBuilder[] afbDelegates)
        {
            ConstructorInfo con = typeof(object).GetConstructor(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, new Type[0], null);
            ILGenerator iLGenerator = OutputTypeBuilder.DefineMethod(".ctor", MethodAttributes.SpecialName | MethodAttributes.Assembly, CallingConventions.Standard, null, null).GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldarg, (short) 0);
            iLGenerator.Emit(OpCodes.Call, con);
            iLGenerator.Emit(OpCodes.Ldarg, (short) 0);
            iLGenerator.Emit(OpCodes.Ldc_I4, 0);
            iLGenerator.Emit(OpCodes.Stfld, fbCookie);
            for (int i = 0; i < afbDelegates.Length; i++)
            {
                if (afbDelegates[i] != null)
                {
                    iLGenerator.Emit(OpCodes.Ldarg, (short) 0);
                    iLGenerator.Emit(OpCodes.Ldnull);
                    iLGenerator.Emit(OpCodes.Stfld, afbDelegates[i]);
                }
            }
            iLGenerator.Emit(OpCodes.Ret);
        }

        private void DefineEventMethod(TypeBuilder OutputTypeBuilder, MethodInfo Method, Type DelegateCls, FieldBuilder fbDelegate)
        {
            Type[] typeArray;
            MethodInfo method = DelegateCls.GetMethod("Invoke");
            Type returnType = Method.ReturnType;
            ParameterInfo[] parameters = Method.GetParameters();
            if (parameters != null)
            {
                typeArray = new Type[parameters.Length];
                for (int j = 0; j < parameters.Length; j++)
                {
                    typeArray[j] = parameters[j].ParameterType;
                }
            }
            else
            {
                typeArray = null;
            }
            MethodAttributes attributes = MethodAttributes.Virtual | MethodAttributes.Public;
            MethodBuilder meth = OutputTypeBuilder.DefineMethod(Method.Name, attributes, CallingConventions.Standard, returnType, typeArray);
            ILGenerator iLGenerator = meth.GetILGenerator();
            Label label = iLGenerator.DefineLabel();
            iLGenerator.Emit(OpCodes.Ldarg, (short) 0);
            iLGenerator.Emit(OpCodes.Ldfld, fbDelegate);
            iLGenerator.Emit(OpCodes.Brfalse, label);
            iLGenerator.Emit(OpCodes.Ldarg, (short) 0);
            iLGenerator.Emit(OpCodes.Ldfld, fbDelegate);
            ParameterInfo[] infoArray2 = Method.GetParameters();
            for (int i = 0; i < infoArray2.Length; i++)
            {
                iLGenerator.Emit(OpCodes.Ldarg, (short) (i + 1));
            }
            iLGenerator.Emit(OpCodes.Callvirt, method);
            iLGenerator.Emit(OpCodes.Ret);
            iLGenerator.MarkLabel(label);
            this.AddReturn(returnType, iLGenerator, meth);
            iLGenerator.Emit(OpCodes.Ret);
        }

        public Type Perform()
        {
            Type[] aInterfaceTypes = new Type[] { this.m_InputType };
            string str = null;
            string str2 = NameSpaceExtractor.ExtractNameSpace(this.m_EventItfType.FullName);
            if (str2 != "")
            {
                str = str2 + ".";
            }
            TypeBuilder tb = TCEAdapterGenerator.DefineUniqueType(str + this.m_InputType.Name + GeneratedTypeNamePostfix, TypeAttributes.Sealed | TypeAttributes.Public, null, aInterfaceTypes, this.m_OutputModule);
            TCEAdapterGenerator.SetHiddenAttribute(tb);
            TCEAdapterGenerator.SetClassInterfaceTypeToNone(tb);
            foreach (MethodInfo info in TCEAdapterGenerator.GetPropertyMethods(this.m_InputType))
            {
                this.DefineBlankMethod(tb, info);
            }
            MethodInfo[] nonPropertyMethods = TCEAdapterGenerator.GetNonPropertyMethods(this.m_InputType);
            FieldBuilder[] afbDelegates = new FieldBuilder[nonPropertyMethods.Length];
            for (int i = 0; i < nonPropertyMethods.Length; i++)
            {
                if (this.m_InputType == nonPropertyMethods[i].DeclaringType)
                {
                    Type parameterType = this.m_EventItfType.GetMethod("add_" + nonPropertyMethods[i].Name).GetParameters()[0].ParameterType;
                    afbDelegates[i] = tb.DefineField("m_" + nonPropertyMethods[i].Name + "Delegate", parameterType, FieldAttributes.Public);
                    this.DefineEventMethod(tb, nonPropertyMethods[i], parameterType, afbDelegates[i]);
                }
            }
            FieldBuilder fbCookie = tb.DefineField("m_dwCookie", typeof(int), FieldAttributes.Public);
            this.DefineConstructor(tb, fbCookie, afbDelegates);
            return tb.CreateType();
        }
    }
}

