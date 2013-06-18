namespace Microsoft.JScript
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Security.Permissions;

    public abstract class FieldAccessor
    {
        private static SimpleHashtable accessorFor = new SimpleHashtable(0x20);
        private static int count = 0;

        protected FieldAccessor()
        {
        }

        internal static FieldAccessor GetAccessorFor(FieldInfo field)
        {
            FieldAccessor accessor = accessorFor[field] as FieldAccessor;
            if (accessor == null)
            {
                lock (accessorFor)
                {
                    accessor = accessorFor[field] as FieldAccessor;
                    if (accessor != null)
                    {
                        return accessor;
                    }
                    accessor = SpitAndInstantiateClassFor(field);
                    accessorFor[field] = accessor;
                }
            }
            return accessor;
        }

        [DebuggerStepThrough, DebuggerHidden]
        public abstract object GetValue(object thisob);
        [DebuggerStepThrough, DebuggerHidden]
        public abstract void SetValue(object thisob, object value);
        [ReflectionPermission(SecurityAction.Assert, Unrestricted=true)]
        private static FieldAccessor SpitAndInstantiateClassFor(FieldInfo field)
        {
            Type fieldType = field.FieldType;
            TypeBuilder builder = Runtime.ThunkModuleBuilder.DefineType("accessor" + count++, TypeAttributes.Public, typeof(FieldAccessor));
            MethodBuilder builder2 = builder.DefineMethod("GetValue", MethodAttributes.Virtual | MethodAttributes.Public, typeof(object), new Type[] { typeof(object) });
            builder2.SetCustomAttribute(new CustomAttributeBuilder(Runtime.TypeRefs.debuggerStepThroughAttributeCtor, new object[0]));
            builder2.SetCustomAttribute(new CustomAttributeBuilder(Runtime.TypeRefs.debuggerHiddenAttributeCtor, new object[0]));
            ILGenerator iLGenerator = builder2.GetILGenerator();
            if (field.IsLiteral)
            {
                new ConstantWrapper(TypeReferences.GetConstantValue(field), null).TranslateToIL(iLGenerator, fieldType);
            }
            else if (field.IsStatic)
            {
                iLGenerator.Emit(OpCodes.Ldsfld, field);
            }
            else
            {
                iLGenerator.Emit(OpCodes.Ldarg_1);
                iLGenerator.Emit(OpCodes.Ldfld, field);
            }
            if (fieldType.IsValueType)
            {
                iLGenerator.Emit(OpCodes.Box, fieldType);
            }
            iLGenerator.Emit(OpCodes.Ret);
            builder2 = builder.DefineMethod("SetValue", MethodAttributes.Virtual | MethodAttributes.Public, typeof(void), new Type[] { typeof(object), typeof(object) });
            builder2.SetCustomAttribute(new CustomAttributeBuilder(Runtime.TypeRefs.debuggerStepThroughAttributeCtor, new object[0]));
            builder2.SetCustomAttribute(new CustomAttributeBuilder(Runtime.TypeRefs.debuggerHiddenAttributeCtor, new object[0]));
            iLGenerator = builder2.GetILGenerator();
            if (!field.IsLiteral)
            {
                if (!field.IsStatic)
                {
                    iLGenerator.Emit(OpCodes.Ldarg_1);
                }
                iLGenerator.Emit(OpCodes.Ldarg_2);
                if (fieldType.IsValueType)
                {
                    Microsoft.JScript.Convert.EmitUnbox(iLGenerator, fieldType, Type.GetTypeCode(fieldType));
                }
                if (field.IsStatic)
                {
                    iLGenerator.Emit(OpCodes.Stsfld, field);
                }
                else
                {
                    iLGenerator.Emit(OpCodes.Stfld, field);
                }
            }
            iLGenerator.Emit(OpCodes.Ret);
            return (FieldAccessor) Activator.CreateInstance(builder.CreateType());
        }
    }
}

