namespace System.Runtime.InteropServices.TCEAdapterGen
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Threading;

    internal class EventProviderWriter
    {
        private const BindingFlags DefaultLookup = (BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
        private Type m_EventItfType;
        private ModuleBuilder m_OutputModule;
        private Type m_SinkHelperType;
        private Type m_SrcItfType;
        private string m_strDestTypeName;
        private readonly Type[] MonitorEnterParamTypes = new Type[] { typeof(object), Type.GetType("System.Boolean&") };

        public EventProviderWriter(ModuleBuilder OutputModule, string strDestTypeName, Type EventItfType, Type SrcItfType, Type SinkHelperType)
        {
            this.m_OutputModule = OutputModule;
            this.m_strDestTypeName = strDestTypeName;
            this.m_EventItfType = EventItfType;
            this.m_SrcItfType = SrcItfType;
            this.m_SinkHelperType = SinkHelperType;
        }

        private MethodBuilder DefineAddEventMethod(TypeBuilder OutputTypeBuilder, MethodInfo SrcItfMethod, Type SinkHelperClass, FieldBuilder fbSinkHelperArray, FieldBuilder fbEventCP, MethodBuilder mbInitSrcItf)
        {
            FieldInfo field = SinkHelperClass.GetField("m_" + SrcItfMethod.Name + "Delegate");
            FieldInfo info2 = SinkHelperClass.GetField("m_dwCookie");
            ConstructorInfo con = SinkHelperClass.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, new Type[0], null);
            MethodInfo method = typeof(IConnectionPoint).GetMethod("Advise");
            Type[] types = new Type[] { typeof(object) };
            MethodInfo meth = typeof(ArrayList).GetMethod("Add", types, null);
            MethodInfo info6 = typeof(Monitor).GetMethod("Enter", this.MonitorEnterParamTypes, null);
            types[0] = typeof(object);
            MethodInfo info7 = typeof(Monitor).GetMethod("Exit", types, null);
            Type[] parameterTypes = new Type[] { field.FieldType };
            MethodBuilder builder = OutputTypeBuilder.DefineMethod("add_" + SrcItfMethod.Name, MethodAttributes.Virtual | MethodAttributes.Public, null, parameterTypes);
            ILGenerator iLGenerator = builder.GetILGenerator();
            Label label = iLGenerator.DefineLabel();
            LocalBuilder local = iLGenerator.DeclareLocal(SinkHelperClass);
            LocalBuilder builder3 = iLGenerator.DeclareLocal(typeof(int));
            LocalBuilder builder4 = iLGenerator.DeclareLocal(typeof(bool));
            iLGenerator.BeginExceptionBlock();
            iLGenerator.Emit(OpCodes.Ldarg, (short) 0);
            iLGenerator.Emit(OpCodes.Ldloca_S, builder4);
            iLGenerator.Emit(OpCodes.Call, info6);
            iLGenerator.Emit(OpCodes.Ldarg, (short) 0);
            iLGenerator.Emit(OpCodes.Ldfld, fbEventCP);
            iLGenerator.Emit(OpCodes.Brtrue, label);
            iLGenerator.Emit(OpCodes.Ldarg, (short) 0);
            iLGenerator.Emit(OpCodes.Call, mbInitSrcItf);
            iLGenerator.MarkLabel(label);
            iLGenerator.Emit(OpCodes.Newobj, con);
            iLGenerator.Emit(OpCodes.Stloc, local);
            iLGenerator.Emit(OpCodes.Ldc_I4_0);
            iLGenerator.Emit(OpCodes.Stloc, builder3);
            iLGenerator.Emit(OpCodes.Ldarg, (short) 0);
            iLGenerator.Emit(OpCodes.Ldfld, fbEventCP);
            iLGenerator.Emit(OpCodes.Ldloc, local);
            iLGenerator.Emit(OpCodes.Castclass, typeof(object));
            iLGenerator.Emit(OpCodes.Ldloca, builder3);
            iLGenerator.Emit(OpCodes.Callvirt, method);
            iLGenerator.Emit(OpCodes.Ldloc, local);
            iLGenerator.Emit(OpCodes.Ldloc, builder3);
            iLGenerator.Emit(OpCodes.Stfld, info2);
            iLGenerator.Emit(OpCodes.Ldloc, local);
            iLGenerator.Emit(OpCodes.Ldarg, (short) 1);
            iLGenerator.Emit(OpCodes.Stfld, field);
            iLGenerator.Emit(OpCodes.Ldarg, (short) 0);
            iLGenerator.Emit(OpCodes.Ldfld, fbSinkHelperArray);
            iLGenerator.Emit(OpCodes.Ldloc, local);
            iLGenerator.Emit(OpCodes.Castclass, typeof(object));
            iLGenerator.Emit(OpCodes.Callvirt, meth);
            iLGenerator.Emit(OpCodes.Pop);
            iLGenerator.BeginFinallyBlock();
            Label label2 = iLGenerator.DefineLabel();
            iLGenerator.Emit(OpCodes.Ldloc, builder4);
            iLGenerator.Emit(OpCodes.Brfalse_S, label2);
            iLGenerator.Emit(OpCodes.Ldarg, (short) 0);
            iLGenerator.Emit(OpCodes.Call, info7);
            iLGenerator.MarkLabel(label2);
            iLGenerator.EndExceptionBlock();
            iLGenerator.Emit(OpCodes.Ret);
            return builder;
        }

        private void DefineConstructor(TypeBuilder OutputTypeBuilder, FieldBuilder fbCPC)
        {
            ConstructorInfo con = typeof(object).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null);
            MethodAttributes attributes = MethodAttributes.SpecialName | (con.Attributes & MethodAttributes.MemberAccessMask);
            ILGenerator iLGenerator = OutputTypeBuilder.DefineMethod(".ctor", attributes, null, new Type[] { typeof(object) }).GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldarg, (short) 0);
            iLGenerator.Emit(OpCodes.Call, con);
            iLGenerator.Emit(OpCodes.Ldarg, (short) 0);
            iLGenerator.Emit(OpCodes.Ldarg, (short) 1);
            iLGenerator.Emit(OpCodes.Castclass, typeof(IConnectionPointContainer));
            iLGenerator.Emit(OpCodes.Stfld, fbCPC);
            iLGenerator.Emit(OpCodes.Ret);
        }

        private void DefineDisposeMethod(TypeBuilder OutputTypeBuilder, MethodBuilder FinalizeMethod)
        {
            MethodInfo method = typeof(GC).GetMethod("SuppressFinalize");
            ILGenerator iLGenerator = OutputTypeBuilder.DefineMethod("Dispose", MethodAttributes.Virtual | MethodAttributes.Public, null, null).GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldarg, (short) 0);
            iLGenerator.Emit(OpCodes.Callvirt, FinalizeMethod);
            iLGenerator.Emit(OpCodes.Ldarg, (short) 0);
            iLGenerator.Emit(OpCodes.Call, method);
            iLGenerator.Emit(OpCodes.Ret);
        }

        private MethodBuilder DefineFinalizeMethod(TypeBuilder OutputTypeBuilder, Type SinkHelperClass, FieldBuilder fbSinkHelper, FieldBuilder fbEventCP)
        {
            FieldInfo field = SinkHelperClass.GetField("m_dwCookie");
            MethodInfo getMethod = typeof(ArrayList).GetProperty("Item").GetGetMethod();
            MethodInfo meth = typeof(ArrayList).GetProperty("Count").GetGetMethod();
            MethodInfo method = typeof(IConnectionPoint).GetMethod("Unadvise");
            MethodInfo info7 = typeof(Marshal).GetMethod("ReleaseComObject");
            MethodInfo info8 = typeof(Monitor).GetMethod("Enter", this.MonitorEnterParamTypes, null);
            Type[] types = new Type[] { typeof(object) };
            MethodInfo info9 = typeof(Monitor).GetMethod("Exit", types, null);
            MethodBuilder builder = OutputTypeBuilder.DefineMethod("Finalize", MethodAttributes.Virtual | MethodAttributes.Public, null, null);
            ILGenerator iLGenerator = builder.GetILGenerator();
            LocalBuilder local = iLGenerator.DeclareLocal(typeof(int));
            LocalBuilder builder3 = iLGenerator.DeclareLocal(typeof(int));
            LocalBuilder builder4 = iLGenerator.DeclareLocal(SinkHelperClass);
            LocalBuilder builder5 = iLGenerator.DeclareLocal(typeof(bool));
            iLGenerator.BeginExceptionBlock();
            iLGenerator.Emit(OpCodes.Ldarg, (short) 0);
            iLGenerator.Emit(OpCodes.Ldloca_S, builder5);
            iLGenerator.Emit(OpCodes.Call, info8);
            Label loc = iLGenerator.DefineLabel();
            Label label = iLGenerator.DefineLabel();
            Label label3 = iLGenerator.DefineLabel();
            iLGenerator.Emit(OpCodes.Ldarg, (short) 0);
            iLGenerator.Emit(OpCodes.Ldfld, fbEventCP);
            iLGenerator.Emit(OpCodes.Brfalse, label3);
            iLGenerator.Emit(OpCodes.Ldarg, (short) 0);
            iLGenerator.Emit(OpCodes.Ldfld, fbSinkHelper);
            iLGenerator.Emit(OpCodes.Callvirt, meth);
            iLGenerator.Emit(OpCodes.Stloc, local);
            iLGenerator.Emit(OpCodes.Ldc_I4, 0);
            iLGenerator.Emit(OpCodes.Stloc, builder3);
            iLGenerator.Emit(OpCodes.Ldc_I4, 0);
            iLGenerator.Emit(OpCodes.Ldloc, local);
            iLGenerator.Emit(OpCodes.Bge, label);
            iLGenerator.MarkLabel(loc);
            iLGenerator.Emit(OpCodes.Ldarg, (short) 0);
            iLGenerator.Emit(OpCodes.Ldfld, fbSinkHelper);
            iLGenerator.Emit(OpCodes.Ldloc, builder3);
            iLGenerator.Emit(OpCodes.Callvirt, getMethod);
            iLGenerator.Emit(OpCodes.Castclass, SinkHelperClass);
            iLGenerator.Emit(OpCodes.Stloc, builder4);
            iLGenerator.Emit(OpCodes.Ldarg, (short) 0);
            iLGenerator.Emit(OpCodes.Ldfld, fbEventCP);
            iLGenerator.Emit(OpCodes.Ldloc, builder4);
            iLGenerator.Emit(OpCodes.Ldfld, field);
            iLGenerator.Emit(OpCodes.Callvirt, method);
            iLGenerator.Emit(OpCodes.Ldloc, builder3);
            iLGenerator.Emit(OpCodes.Ldc_I4, 1);
            iLGenerator.Emit(OpCodes.Add);
            iLGenerator.Emit(OpCodes.Stloc, builder3);
            iLGenerator.Emit(OpCodes.Ldloc, builder3);
            iLGenerator.Emit(OpCodes.Ldloc, local);
            iLGenerator.Emit(OpCodes.Blt, loc);
            iLGenerator.MarkLabel(label);
            iLGenerator.Emit(OpCodes.Ldarg, (short) 0);
            iLGenerator.Emit(OpCodes.Ldfld, fbEventCP);
            iLGenerator.Emit(OpCodes.Call, info7);
            iLGenerator.Emit(OpCodes.Pop);
            iLGenerator.MarkLabel(label3);
            iLGenerator.BeginCatchBlock(typeof(Exception));
            iLGenerator.Emit(OpCodes.Pop);
            iLGenerator.BeginFinallyBlock();
            Label label4 = iLGenerator.DefineLabel();
            iLGenerator.Emit(OpCodes.Ldloc, builder5);
            iLGenerator.Emit(OpCodes.Brfalse_S, label4);
            iLGenerator.Emit(OpCodes.Ldarg, (short) 0);
            iLGenerator.Emit(OpCodes.Call, info9);
            iLGenerator.MarkLabel(label4);
            iLGenerator.EndExceptionBlock();
            iLGenerator.Emit(OpCodes.Ret);
            return builder;
        }

        private MethodBuilder DefineInitSrcItfMethod(TypeBuilder OutputTypeBuilder, Type SourceInterface, FieldBuilder fbSinkHelperArray, FieldBuilder fbEventCP, FieldBuilder fbCPC)
        {
            ConstructorInfo con = typeof(ArrayList).GetConstructor(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, new Type[0], null);
            byte[] buffer = new byte[0x10];
            Type[] types = new Type[] { typeof(byte[]) };
            ConstructorInfo info2 = typeof(Guid).GetConstructor(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, types, null);
            MethodInfo method = typeof(IConnectionPointContainer).GetMethod("FindConnectionPoint");
            MethodBuilder builder = OutputTypeBuilder.DefineMethod("Init", MethodAttributes.Private, null, null);
            ILGenerator iLGenerator = builder.GetILGenerator();
            LocalBuilder local = iLGenerator.DeclareLocal(typeof(IConnectionPoint));
            LocalBuilder builder3 = iLGenerator.DeclareLocal(typeof(Guid));
            LocalBuilder builder4 = iLGenerator.DeclareLocal(typeof(byte[]));
            iLGenerator.Emit(OpCodes.Ldnull);
            iLGenerator.Emit(OpCodes.Stloc, local);
            buffer = SourceInterface.GUID.ToByteArray();
            iLGenerator.Emit(OpCodes.Ldc_I4, 0x10);
            iLGenerator.Emit(OpCodes.Newarr, typeof(byte));
            iLGenerator.Emit(OpCodes.Stloc, builder4);
            for (int i = 0; i < 0x10; i++)
            {
                iLGenerator.Emit(OpCodes.Ldloc, builder4);
                iLGenerator.Emit(OpCodes.Ldc_I4, i);
                iLGenerator.Emit(OpCodes.Ldc_I4, (int) buffer[i]);
                iLGenerator.Emit(OpCodes.Stelem_I1);
            }
            iLGenerator.Emit(OpCodes.Ldloca, builder3);
            iLGenerator.Emit(OpCodes.Ldloc, builder4);
            iLGenerator.Emit(OpCodes.Call, info2);
            iLGenerator.Emit(OpCodes.Ldarg, (short) 0);
            iLGenerator.Emit(OpCodes.Ldfld, fbCPC);
            iLGenerator.Emit(OpCodes.Ldloca, builder3);
            iLGenerator.Emit(OpCodes.Ldloca, local);
            iLGenerator.Emit(OpCodes.Callvirt, method);
            iLGenerator.Emit(OpCodes.Ldarg, (short) 0);
            iLGenerator.Emit(OpCodes.Ldloc, local);
            iLGenerator.Emit(OpCodes.Castclass, typeof(IConnectionPoint));
            iLGenerator.Emit(OpCodes.Stfld, fbEventCP);
            iLGenerator.Emit(OpCodes.Ldarg, (short) 0);
            iLGenerator.Emit(OpCodes.Newobj, con);
            iLGenerator.Emit(OpCodes.Stfld, fbSinkHelperArray);
            iLGenerator.Emit(OpCodes.Ret);
            return builder;
        }

        private MethodBuilder DefineRemoveEventMethod(TypeBuilder OutputTypeBuilder, MethodInfo SrcItfMethod, Type SinkHelperClass, FieldBuilder fbSinkHelperArray, FieldBuilder fbEventCP)
        {
            FieldInfo field = SinkHelperClass.GetField("m_" + SrcItfMethod.Name + "Delegate");
            FieldInfo info2 = SinkHelperClass.GetField("m_dwCookie");
            Type[] types = new Type[] { typeof(int) };
            MethodInfo meth = typeof(ArrayList).GetMethod("RemoveAt", types, null);
            MethodInfo getMethod = typeof(ArrayList).GetProperty("Item").GetGetMethod();
            MethodInfo info7 = typeof(ArrayList).GetProperty("Count").GetGetMethod();
            types[0] = typeof(Delegate);
            MethodInfo info8 = typeof(Delegate).GetMethod("Equals", types, null);
            MethodInfo info9 = typeof(Monitor).GetMethod("Enter", this.MonitorEnterParamTypes, null);
            types[0] = typeof(object);
            MethodInfo info10 = typeof(Monitor).GetMethod("Exit", types, null);
            MethodInfo method = typeof(IConnectionPoint).GetMethod("Unadvise");
            MethodInfo info12 = typeof(Marshal).GetMethod("ReleaseComObject");
            Type[] parameterTypes = new Type[] { field.FieldType };
            MethodBuilder builder = OutputTypeBuilder.DefineMethod("remove_" + SrcItfMethod.Name, MethodAttributes.Virtual | MethodAttributes.Public, null, parameterTypes);
            ILGenerator iLGenerator = builder.GetILGenerator();
            LocalBuilder local = iLGenerator.DeclareLocal(typeof(int));
            LocalBuilder builder3 = iLGenerator.DeclareLocal(typeof(int));
            LocalBuilder builder4 = iLGenerator.DeclareLocal(SinkHelperClass);
            LocalBuilder builder5 = iLGenerator.DeclareLocal(typeof(bool));
            Label loc = iLGenerator.DefineLabel();
            Label label = iLGenerator.DefineLabel();
            Label label3 = iLGenerator.DefineLabel();
            iLGenerator.DefineLabel();
            iLGenerator.BeginExceptionBlock();
            iLGenerator.Emit(OpCodes.Ldarg, (short) 0);
            iLGenerator.Emit(OpCodes.Ldloca_S, builder5);
            iLGenerator.Emit(OpCodes.Call, info9);
            iLGenerator.Emit(OpCodes.Ldarg, (short) 0);
            iLGenerator.Emit(OpCodes.Ldfld, fbSinkHelperArray);
            iLGenerator.Emit(OpCodes.Brfalse, label);
            iLGenerator.Emit(OpCodes.Ldarg, (short) 0);
            iLGenerator.Emit(OpCodes.Ldfld, fbSinkHelperArray);
            iLGenerator.Emit(OpCodes.Callvirt, info7);
            iLGenerator.Emit(OpCodes.Stloc, local);
            iLGenerator.Emit(OpCodes.Ldc_I4, 0);
            iLGenerator.Emit(OpCodes.Stloc, builder3);
            iLGenerator.Emit(OpCodes.Ldc_I4, 0);
            iLGenerator.Emit(OpCodes.Ldloc, local);
            iLGenerator.Emit(OpCodes.Bge, label);
            iLGenerator.MarkLabel(loc);
            iLGenerator.Emit(OpCodes.Ldarg, (short) 0);
            iLGenerator.Emit(OpCodes.Ldfld, fbSinkHelperArray);
            iLGenerator.Emit(OpCodes.Ldloc, builder3);
            iLGenerator.Emit(OpCodes.Callvirt, getMethod);
            iLGenerator.Emit(OpCodes.Castclass, SinkHelperClass);
            iLGenerator.Emit(OpCodes.Stloc, builder4);
            iLGenerator.Emit(OpCodes.Ldloc, builder4);
            iLGenerator.Emit(OpCodes.Ldfld, field);
            iLGenerator.Emit(OpCodes.Ldnull);
            iLGenerator.Emit(OpCodes.Beq, label3);
            iLGenerator.Emit(OpCodes.Ldloc, builder4);
            iLGenerator.Emit(OpCodes.Ldfld, field);
            iLGenerator.Emit(OpCodes.Ldarg, (short) 1);
            iLGenerator.Emit(OpCodes.Castclass, typeof(object));
            iLGenerator.Emit(OpCodes.Callvirt, info8);
            iLGenerator.Emit(OpCodes.Ldc_I4, 0xff);
            iLGenerator.Emit(OpCodes.And);
            iLGenerator.Emit(OpCodes.Ldc_I4, 0);
            iLGenerator.Emit(OpCodes.Beq, label3);
            iLGenerator.Emit(OpCodes.Ldarg, (short) 0);
            iLGenerator.Emit(OpCodes.Ldfld, fbSinkHelperArray);
            iLGenerator.Emit(OpCodes.Ldloc, builder3);
            iLGenerator.Emit(OpCodes.Callvirt, meth);
            iLGenerator.Emit(OpCodes.Ldarg, (short) 0);
            iLGenerator.Emit(OpCodes.Ldfld, fbEventCP);
            iLGenerator.Emit(OpCodes.Ldloc, builder4);
            iLGenerator.Emit(OpCodes.Ldfld, info2);
            iLGenerator.Emit(OpCodes.Callvirt, method);
            iLGenerator.Emit(OpCodes.Ldloc, local);
            iLGenerator.Emit(OpCodes.Ldc_I4, 1);
            iLGenerator.Emit(OpCodes.Bgt, label);
            iLGenerator.Emit(OpCodes.Ldarg, (short) 0);
            iLGenerator.Emit(OpCodes.Ldfld, fbEventCP);
            iLGenerator.Emit(OpCodes.Call, info12);
            iLGenerator.Emit(OpCodes.Pop);
            iLGenerator.Emit(OpCodes.Ldarg, (short) 0);
            iLGenerator.Emit(OpCodes.Ldnull);
            iLGenerator.Emit(OpCodes.Stfld, fbEventCP);
            iLGenerator.Emit(OpCodes.Ldarg, (short) 0);
            iLGenerator.Emit(OpCodes.Ldnull);
            iLGenerator.Emit(OpCodes.Stfld, fbSinkHelperArray);
            iLGenerator.Emit(OpCodes.Br, label);
            iLGenerator.MarkLabel(label3);
            iLGenerator.Emit(OpCodes.Ldloc, builder3);
            iLGenerator.Emit(OpCodes.Ldc_I4, 1);
            iLGenerator.Emit(OpCodes.Add);
            iLGenerator.Emit(OpCodes.Stloc, builder3);
            iLGenerator.Emit(OpCodes.Ldloc, builder3);
            iLGenerator.Emit(OpCodes.Ldloc, local);
            iLGenerator.Emit(OpCodes.Blt, loc);
            iLGenerator.MarkLabel(label);
            iLGenerator.BeginFinallyBlock();
            Label label4 = iLGenerator.DefineLabel();
            iLGenerator.Emit(OpCodes.Ldloc, builder5);
            iLGenerator.Emit(OpCodes.Brfalse_S, label4);
            iLGenerator.Emit(OpCodes.Ldarg, (short) 0);
            iLGenerator.Emit(OpCodes.Call, info10);
            iLGenerator.MarkLabel(label4);
            iLGenerator.EndExceptionBlock();
            iLGenerator.Emit(OpCodes.Ret);
            return builder;
        }

        public Type Perform()
        {
            TypeBuilder outputTypeBuilder = this.m_OutputModule.DefineType(this.m_strDestTypeName, TypeAttributes.Sealed, typeof(object), new Type[] { this.m_EventItfType, typeof(IDisposable) });
            FieldBuilder fbCPC = outputTypeBuilder.DefineField("m_ConnectionPointContainer", typeof(IConnectionPointContainer), FieldAttributes.Private);
            FieldBuilder fbSinkHelperArray = outputTypeBuilder.DefineField("m_aEventSinkHelpers", typeof(ArrayList), FieldAttributes.Private);
            FieldBuilder fbEventCP = outputTypeBuilder.DefineField("m_ConnectionPoint", typeof(IConnectionPoint), FieldAttributes.Private);
            MethodBuilder mbInitSrcItf = this.DefineInitSrcItfMethod(outputTypeBuilder, this.m_SrcItfType, fbSinkHelperArray, fbEventCP, fbCPC);
            MethodInfo[] nonPropertyMethods = TCEAdapterGenerator.GetNonPropertyMethods(this.m_SrcItfType);
            for (int i = 0; i < nonPropertyMethods.Length; i++)
            {
                if (this.m_SrcItfType == nonPropertyMethods[i].DeclaringType)
                {
                    this.DefineAddEventMethod(outputTypeBuilder, nonPropertyMethods[i], this.m_SinkHelperType, fbSinkHelperArray, fbEventCP, mbInitSrcItf);
                    this.DefineRemoveEventMethod(outputTypeBuilder, nonPropertyMethods[i], this.m_SinkHelperType, fbSinkHelperArray, fbEventCP);
                }
            }
            this.DefineConstructor(outputTypeBuilder, fbCPC);
            MethodBuilder finalizeMethod = this.DefineFinalizeMethod(outputTypeBuilder, this.m_SinkHelperType, fbSinkHelperArray, fbEventCP);
            this.DefineDisposeMethod(outputTypeBuilder, finalizeMethod);
            return outputTypeBuilder.CreateType();
        }
    }
}

